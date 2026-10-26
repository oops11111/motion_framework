// tokens.h: motion-token schema (v1), strict validator, loader. Module 4.
// Header-only C++17, standard library only, namespace motion.
// Depends on timeline.h (and through it spring.h / bezier.h).
//
// Public surface: Diagnostic, TokenSet (Parse + lookups + BuildTimeline).
// Everything in motion::detail (embedded JSON parser, compiler) is internal.
//
// ---------------------------------------------------------------------------
// SCHEMA v1 (all objects are strict: unknown keys are errors)
//
// {
//   "$schema": "...",            // optional, ignored          (top level only)
//   "$comment": "...",           // optional, ignored          (top level only)
//   "version": 1,                // required
//   "durations": { name: "120ms" | "0.3s" | <duration-name> },
//   "easing":    { name: [x1,y1,x2,y2] | <easing-name> },      // x in [0,1]
//   "springs":   { name: {"response": <duration>, "damping": number} | <spring-name> },
//   "transitions": { name: { "root": <node>, "reduced": <node> | "instant" } }
// }
//
// <duration>  string: literal with unit ("120ms", "0.3s") OR a duration token name.
//             Bare numbers are rejected (unit ambiguity). Range [0, 600] s.
// <easing>    inline array OR easing token name. Built-in name: "linear" only.
// <spring>    inline {response, damping} OR spring token name.
//             response in [0.001, 10] s, damping in [0, 100].
// Token names match [A-Za-z][A-Za-z0-9_.-]* (so they never look like literals).
// Alias chains are allowed; cycles are errors.
//
// <node> (object with "type"):
//   track    {property, from, steps:[step...]}   steps non-empty
//              step: {"to": n, "duration": <duration>, "ease": <easing>?}
//                  | {"springTo": n, "spring": <spring>, "inheritVelocity": bool?}
//                  | {"hold": <duration>}                (exactly one of to/springTo/hold)
//   sequence {children:[node...]}   parallel {children:[...]}   (non-empty)
//   stagger  {delay: <duration>, children:[...]}
//   delay    {duration: <duration>}
//   marker   {name: non-empty string}
// Node nesting depth is limited to 32 (JSON nesting to 128, which covers it).
//
// "reduced" is REQUIRED for every transition (reduced-motion is a first-class
// requirement). "instant" means: every property the root animates jumps to its
// final value at t = 0 (derived from the root, never hand-written).
//
// Error paths are JSON Pointers ("/transitions/card.enter/root/children/1").
// ---------------------------------------------------------------------------
#pragma once
#include <algorithm>
#include <charconv>
#include <cmath>
#include <cstdint>
#include <initializer_list>
#include <map>
#include <memory>
#include <optional>
#include <string>
#include <string_view>
#include <utility>
#include <vector>

#include "timeline.h"

namespace motion {

struct Diagnostic {
    std::string path;      // JSON Pointer; "" = document root
    std::string message;
    std::string ToString() const { return (path.empty() ? std::string("/") : path) + ": " + message; }
};

namespace detail {

// ------------------------------------------------- embedded JSON parser ----
// Minimal strict RFC 8259 parser, written for this module (no third-party code).
// Rejects: trailing commas, duplicate object keys, leading zeros, control
// characters in strings, unpaired surrogates, trailing garbage, nesting > 128.
// Accepts a leading UTF-8 BOM. Does not validate UTF-8 byte sequences.
// Numbers use std::from_chars (locale independent).
struct Json {
    enum class Type { Null, Bool, Number, String, Array, Object };
    Type type = Type::Null;
    bool b = false;
    double n = 0.0;
    std::string s;
    std::vector<Json> vals;            // array elements, or object values
    std::vector<std::string> keys;     // object keys, parallel to vals (insertion order)

    const Json* Find(std::string_view k) const {
        for (std::size_t i = 0; i < keys.size(); ++i) if (keys[i] == k) return &vals[i];
        return nullptr;
    }
    const char* TypeName() const {
        switch (type) {
            case Type::Null: return "null";   case Type::Bool: return "boolean";
            case Type::Number: return "number"; case Type::String: return "string";
            case Type::Array: return "array"; case Type::Object: return "object";
        }
        return "?";
    }
};

class JsonParser {
public:
    explicit JsonParser(std::string_view t) : t_(t) {}

    bool Parse(Json& out, std::string& err) {
        if (t_.size() >= 3 && static_cast<unsigned char>(t_[0]) == 0xEF &&
            static_cast<unsigned char>(t_[1]) == 0xBB && static_cast<unsigned char>(t_[2]) == 0xBF) p_ = 3;
        Ws();
        if (!Value(out, 0)) { err = err_; return false; }
        Ws();
        if (p_ != t_.size()) { Fail("unexpected trailing characters"); err = err_; return false; }
        return true;
    }

private:
    static constexpr int kMaxDepth = 128;

    char Peek() const { return p_ < t_.size() ? t_[p_] : '\0'; }
    bool IsDigit() const { return Peek() >= '0' && Peek() <= '9'; }
    void Ws() { while (p_ < t_.size() && (t_[p_] == ' ' || t_[p_] == '\t' || t_[p_] == '\n' || t_[p_] == '\r')) ++p_; }

    bool Fail(const std::string& m) {
        std::size_t line = 1, col = 1;
        for (std::size_t i = 0; i < p_ && i < t_.size(); ++i) { if (t_[i] == '\n') { ++line; col = 1; } else ++col; }
        err_ = "line " + std::to_string(line) + ", col " + std::to_string(col) + ": " + m;
        return false;
    }

    bool Lit(std::string_view w) {
        if (t_.compare(p_, w.size(), w) != 0) return Fail("invalid literal");
        p_ += w.size();
        return true;
    }

    bool Value(Json& v, int depth) {
        if (depth > kMaxDepth) return Fail("nesting too deep");
        if (p_ >= t_.size()) return Fail("unexpected end of input");
        const char c = t_[p_];
        if (c == '{') return Object(v, depth);
        if (c == '[') return Array(v, depth);
        if (c == '"') { v.type = Json::Type::String; return String(v.s); }
        if (c == 't') { v.type = Json::Type::Bool; v.b = true;  return Lit("true"); }
        if (c == 'f') { v.type = Json::Type::Bool; v.b = false; return Lit("false"); }
        if (c == 'n') { v.type = Json::Type::Null; return Lit("null"); }
        if (c == '-' || (c >= '0' && c <= '9')) return Number(v);
        return Fail(std::string("unexpected character '") + c + "'");
    }

    bool Number(Json& v) {
        const std::size_t s = p_;
        if (Peek() == '-') ++p_;
        if (Peek() == '0') ++p_;
        else if (Peek() >= '1' && Peek() <= '9') { while (IsDigit()) ++p_; }
        else return Fail("invalid number");
        if (Peek() == '.') { ++p_; if (!IsDigit()) return Fail("digit expected after '.'"); while (IsDigit()) ++p_; }
        if (Peek() == 'e' || Peek() == 'E') {
            ++p_; if (Peek() == '+' || Peek() == '-') ++p_;
            if (!IsDigit()) return Fail("digit expected in exponent");
            while (IsDigit()) ++p_;
        }
        double d = 0.0;
        const char* b = t_.data() + s; const char* e = t_.data() + p_;
        const auto r = std::from_chars(b, e, d);
        if (r.ec != std::errc() || r.ptr != e || !std::isfinite(d)) { p_ = s; return Fail("number out of range"); }
        v.type = Json::Type::Number; v.n = d;
        return true;
    }

    bool Hex4(unsigned& out) {
        if (p_ + 4 > t_.size()) return Fail("truncated \\u escape");
        out = 0;
        for (int i = 0; i < 4; ++i) {
            const char c = t_[p_++];
            out <<= 4;
            if (c >= '0' && c <= '9') out |= static_cast<unsigned>(c - '0');
            else if (c >= 'a' && c <= 'f') out |= static_cast<unsigned>(c - 'a' + 10);
            else if (c >= 'A' && c <= 'F') out |= static_cast<unsigned>(c - 'A' + 10);
            else { --p_; return Fail("invalid hex digit in \\u escape"); }
        }
        return true;
    }

    static void AppendUtf8(std::string& o, unsigned cp) {
        if (cp < 0x80) o += static_cast<char>(cp);
        else if (cp < 0x800) { o += static_cast<char>(0xC0 | (cp >> 6)); o += static_cast<char>(0x80 | (cp & 0x3F)); }
        else if (cp < 0x10000) {
            o += static_cast<char>(0xE0 | (cp >> 12)); o += static_cast<char>(0x80 | ((cp >> 6) & 0x3F));
            o += static_cast<char>(0x80 | (cp & 0x3F));
        } else {
            o += static_cast<char>(0xF0 | (cp >> 18)); o += static_cast<char>(0x80 | ((cp >> 12) & 0x3F));
            o += static_cast<char>(0x80 | ((cp >> 6) & 0x3F)); o += static_cast<char>(0x80 | (cp & 0x3F));
        }
    }

    bool String(std::string& out) {
        ++p_;  // opening quote
        for (;;) {
            if (p_ >= t_.size()) return Fail("unterminated string");
            const unsigned char c = static_cast<unsigned char>(t_[p_]);
            if (c == '"') { ++p_; return true; }
            if (c < 0x20) return Fail("control character in string");
            if (c != '\\') { out += static_cast<char>(c); ++p_; continue; }
            ++p_;
            if (p_ >= t_.size()) return Fail("unterminated escape");
            const char e = t_[p_++];
            switch (e) {
                case '"': out += '"'; break;   case '\\': out += '\\'; break;  case '/': out += '/'; break;
                case 'b': out += '\b'; break;  case 'f': out += '\f'; break;   case 'n': out += '\n'; break;
                case 'r': out += '\r'; break;  case 't': out += '\t'; break;
                case 'u': {
                    unsigned cp = 0;
                    if (!Hex4(cp)) return false;
                    if (cp >= 0xD800 && cp <= 0xDBFF) {
                        if (!(p_ + 1 < t_.size() && t_[p_] == '\\' && t_[p_ + 1] == 'u')) return Fail("unpaired surrogate");
                        p_ += 2;
                        unsigned lo;
                        if (!Hex4(lo)) return false;
                        if (lo < 0xDC00 || lo > 0xDFFF) return Fail("invalid surrogate pair");
                        cp = 0x10000 + ((cp - 0xD800) << 10) + (lo - 0xDC00);
                    } else if (cp >= 0xDC00 && cp <= 0xDFFF) return Fail("unpaired surrogate");
                    AppendUtf8(out, cp);
                    break;
                }
                default: --p_; return Fail("invalid escape sequence");
            }
        }
    }

    bool Array(Json& v, int depth) {
        v.type = Json::Type::Array; ++p_; Ws();
        if (Peek() == ']') { ++p_; return true; }
        for (;;) {
            Ws();
            Json child;
            if (!Value(child, depth + 1)) return false;
            v.vals.push_back(std::move(child));
            Ws();
            const char c = Peek();
            if (c == ',') { ++p_; continue; }
            if (c == ']') { ++p_; return true; }
            return Fail("expected ',' or ']'");
        }
    }

    bool Object(Json& v, int depth) {
        v.type = Json::Type::Object; ++p_; Ws();
        if (Peek() == '}') { ++p_; return true; }
        for (;;) {
            Ws();
            if (Peek() != '"') return Fail("expected string key");
            std::string key; const std::size_t kpos = p_;
            if (!String(key)) return false;
            for (const auto& k : v.keys) if (k == key) { p_ = kpos; return Fail("duplicate key '" + key + "'"); }
            Ws();
            if (Peek() != ':') return Fail("expected ':'");
            ++p_; Ws();
            Json child;
            if (!Value(child, depth + 1)) return false;
            v.keys.push_back(std::move(key)); v.vals.push_back(std::move(child));
            Ws();
            const char c = Peek();
            if (c == ',') { ++p_; continue; }
            if (c == '}') { ++p_; return true; }
            return Fail("expected ',' or '}'");
        }
    }

    std::string_view t_;
    std::size_t p_ = 0;
    std::string err_;
};

class Compiler;
}  // namespace detail

// ------------------------------------------------------------- TokenSet ----

class TokenSet {
public:
    // Validates and compiles `json`. On failure returns nullopt and fills
    // `errors` (all problems found, not just the first). `errors` is cleared first.
    static std::optional<TokenSet> Parse(std::string_view json, std::vector<Diagnostic>& errors);

    int Version() const { return version_; }

    std::optional<double> Duration(std::string_view name) const { return Get(durations_, name); }
    std::optional<CubicBezier> Easing(std::string_view name) const {
        if (auto e = Get(easings_, name)) return e;
        if (name == "linear") return CubicBezier(0.0, 0.0, 1.0, 1.0);
        return std::nullopt;
    }
    std::optional<SpringParams> Spring(std::string_view name) const { return Get(springs_, name); }

    std::vector<std::string> TransitionNames() const {
        std::vector<std::string> n;
        for (const auto& kv : transitions_) n.push_back(kv.first);
        return n;
    }
    bool HasTransition(std::string_view name) const { return transitions_.find(name) != transitions_.end(); }

    // Immutable, shareable. nullptr if the transition does not exist.
    // reducedMotion selects the reduced-motion variant.
    std::shared_ptr<const Timeline> BuildTimeline(std::string_view name, bool reducedMotion = false) const {
        auto it = transitions_.find(name);
        if (it == transitions_.end()) return nullptr;
        return reducedMotion ? it->second.reduced : it->second.full;
    }

private:
    friend class detail::Compiler;
    struct Compiled { std::shared_ptr<const Timeline> full, reduced; };
    template <class V>
    static std::optional<V> Get(const std::map<std::string, V, std::less<>>& m, std::string_view k) {
        auto it = m.find(k);
        if (it == m.end()) return std::nullopt;
        return it->second;
    }

    int version_ = 0;
    std::map<std::string, double, std::less<>> durations_;
    std::map<std::string, CubicBezier, std::less<>> easings_;
    std::map<std::string, SpringParams, std::less<>> springs_;
    std::map<std::string, Compiled, std::less<>> transitions_;
};

namespace detail {

class Compiler {
    using Type = Json::Type;
public:
    Compiler(const Json& doc, std::vector<Diagnostic>& errs) : doc_(doc), errs_(errs) {}

    std::optional<TokenSet> Run() {
        if (doc_.type != Type::Object) { TypeErr("", "object", doc_); return std::nullopt; }
        CheckKeys(doc_, "", {"$schema", "$comment", "version", "durations", "easing", "springs", "transitions"}, {"version"});

        int version = 0;
        if (const Json* v = doc_.Find("version")) {
            if (auto n = GetNumber(*v, "/version")) {
                if (*n != 1.0) Err("/version", "unsupported version " + Fmt(*n) + " (this loader supports 1)");
                else version = 1;
            }
        }

        Register("durations", dur_);
        Register("easing", easing_);
        Register("springs", spring_);
        for (auto& kv : dur_.e)    LookupDuration(kv.first, kv.second.path);
        for (auto& kv : easing_.e) LookupEasing(kv.first, kv.second.path);
        for (auto& kv : spring_.e) LookupSpring(kv.first, kv.second.path);

        std::vector<Pending> pending;
        CompileTransitions(pending);

        if (!errs_.empty()) return std::nullopt;

        TokenSet ts;
        ts.version_ = version;
        for (auto& kv : dur_.e)    ts.durations_.emplace(kv.first, *kv.second.val);
        for (auto& kv : easing_.e) ts.easings_.emplace(kv.first, *kv.second.val);
        for (auto& kv : spring_.e) ts.springs_.emplace(kv.first, *kv.second.val);
        for (auto& p : pending) {
            TokenSet::Compiled c;
            c.full = std::make_shared<const Timeline>(p.root);
            c.reduced = std::make_shared<const Timeline>(p.instant ? MakeInstant(*c.full) : p.reduced);
            ts.transitions_.emplace(p.name, std::move(c));
        }
        return ts;
    }

private:
    static constexpr int kMaxNodeDepth = 32;
    static constexpr double kMaxDuration = 600.0;

    template <class T> struct Table {
        struct Entry { const Json* v = nullptr; std::string path; int state = 0; std::optional<T> val; };
        const char* label;
        std::map<std::string, Entry, std::less<>> e;
        std::vector<std::string> chain;
        explicit Table(const char* l) : label(l) {}
    };
    struct Pending { std::string name; NodePtr root, reduced; bool instant = false; };
    struct Step { enum Kind { To, SpringTo, Hold } kind = To; double value = 0, duration = 0; Curve curve; SpringParams sp; bool inherit = false; };

    // ----- diagnostics helpers -----
    void Err(const std::string& path, std::string msg) { errs_.push_back({path, std::move(msg)}); }
    void TypeErr(const std::string& path, const char* expected, const Json& got) {
        Err(path, std::string("expected ") + expected + ", got " + got.TypeName());
    }
    static std::string Esc(const std::string& k) {
        std::string o;
        for (char c : k) { if (c == '~') o += "~0"; else if (c == '/') o += "~1"; else o += c; }
        return o;
    }
    static std::string Join(const std::string& p, const std::string& k) { return p + "/" + Esc(k); }
    static std::string Fmt(double d) {
        char buf[48]; auto r = std::to_chars(buf, buf + sizeof buf, d);
        return std::string(buf, r.ptr);
    }
    static std::string JoinList(const std::vector<std::string>& v) {
        std::string o; for (std::size_t i = 0; i < v.size(); ++i) { if (i) o += ", "; o += v[i]; } return o;
    }
    static bool ValidName(const std::string& s) {
        if (s.empty() || !((s[0] >= 'a' && s[0] <= 'z') || (s[0] >= 'A' && s[0] <= 'Z'))) return false;
        for (char c : s)
            if (!((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_' || c == '.' || c == '-')) return false;
        return true;
    }

    // Reports unknown and missing keys; returns true if there were none.
    bool CheckKeys(const Json& obj, const std::string& path, std::initializer_list<const char*> allowed,
                   std::initializer_list<const char*> required) {
        bool ok = true;
        std::vector<std::string> al(allowed.begin(), allowed.end());
        for (const auto& k : obj.keys)
            if (std::find(al.begin(), al.end(), k) == al.end()) {
                Err(Join(path, k), "unknown key '" + k + "' (allowed: " + JoinList(al) + ")");
                ok = false;
            }
        for (const char* r : required)
            if (!obj.Find(r)) { Err(path, std::string("missing required key '") + r + "'"); ok = false; }
        return ok;
    }

    std::optional<double> GetNumber(const Json& v, const std::string& path) {
        if (v.type != Type::Number) { TypeErr(path, "number", v); return std::nullopt; }
        return v.n;
    }

    // ----- token tables -----
    template <class T>
    void Register(const char* key, Table<T>& tb) {
        const Json* sec = doc_.Find(key);
        if (!sec) return;
        const std::string sp = std::string("/") + key;
        if (sec->type != Type::Object) { TypeErr(sp, "object", *sec); return; }
        for (std::size_t i = 0; i < sec->keys.size(); ++i) {
            const std::string& name = sec->keys[i];
            const std::string path = Join(sp, name);
            if (!ValidName(name)) { Err(path, "invalid token name '" + name + "' (must match [A-Za-z][A-Za-z0-9_.-]*)"); continue; }
            typename Table<T>::Entry en; en.v = &sec->vals[i]; en.path = path;
            tb.e.emplace(name, std::move(en));
        }
    }

    template <class T, class F>
    std::optional<T> Lookup(Table<T>& tb, std::string_view name, const std::string& refPath, F parse) {
        auto it = tb.e.find(name);
        if (it == tb.e.end()) {
            std::vector<std::string> known; for (auto& kv : tb.e) known.push_back(kv.first);
            Err(refPath, std::string("unknown ") + tb.label + " token '" + std::string(name) + "'" +
                         (known.empty() ? std::string(" (none defined)") : " (defined: " + JoinList(known) + ")"));
            return std::nullopt;
        }
        auto& en = it->second;
        if (en.state == 2) return en.val;
        if (en.state == 1) {
            std::string chain;
            std::size_t start = 0;
            for (std::size_t i = 0; i < tb.chain.size(); ++i) if (tb.chain[i] == name) { start = i; break; }
            for (std::size_t i = start; i < tb.chain.size(); ++i) chain += tb.chain[i] + " -> ";
            chain += std::string(name);
            Err(refPath, "circular reference: " + chain);
            return std::nullopt;
        }
        en.state = 1;
        tb.chain.push_back(std::string(name));
        en.val = parse(*en.v, en.path);
        tb.chain.pop_back();
        en.state = 2;
        return en.val;
    }

    std::optional<double> LookupDuration(std::string_view n, const std::string& rp) {
        return Lookup(dur_, n, rp, [this](const Json& v, const std::string& p) { return ParseDuration(v, p); });
    }
    std::optional<CubicBezier> LookupEasing(std::string_view n, const std::string& rp) {
        if (n == "linear" && easing_.e.find(n) == easing_.e.end()) return CubicBezier(0.0, 0.0, 1.0, 1.0);
        return Lookup(easing_, n, rp, [this](const Json& v, const std::string& p) { return ParseEasing(v, p); });
    }
    std::optional<SpringParams> LookupSpring(std::string_view n, const std::string& rp) {
        return Lookup(spring_, n, rp, [this](const Json& v, const std::string& p) { return ParseSpring(v, p); });
    }

    // ----- value parsers (each used for both token entries and inline use) -----
    std::optional<double> ParseDuration(const Json& v, const std::string& path) {
        if (v.type != Type::String) { TypeErr(path, "string (e.g. \"120ms\", \"0.3s\") or duration token name", v); return std::nullopt; }
        const std::string& s = v.s;
        const bool literal = !s.empty() && ((s[0] >= '0' && s[0] <= '9') || s[0] == '.' || s[0] == '-' || s[0] == '+');
        if (!literal) {
            if (!ValidName(s)) { Err(path, "invalid duration '" + s + "'"); return std::nullopt; }
            return LookupDuration(s, path);
        }
        double scale, div = 1.0; std::size_t unit;
        if (s.size() > 2 && s.compare(s.size() - 2, 2, "ms") == 0) { unit = 2; div = 1000.0; scale = 1.0; }
        else if (s.size() > 1 && s.back() == 's') { unit = 1; scale = 1.0; }
        else { Err(path, "invalid duration literal '" + s + "' (expected a unit: \"120ms\" or \"0.3s\")"); return std::nullopt; }
        double d = 0.0;
        const char* b = s.data(); const char* e = s.data() + s.size() - unit;
        const auto r = std::from_chars(b, e, d);
        if (r.ec != std::errc() || r.ptr != e || !std::isfinite(d)) {
            Err(path, "invalid duration literal '" + s + "' (expected e.g. \"120ms\" or \"0.3s\")"); return std::nullopt;
        }
        d = d * scale / div;
        if (d < 0.0 || d > kMaxDuration) {
            Err(path, "duration must be in [0, " + Fmt(kMaxDuration) + "] seconds (got " + Fmt(d) + ")"); return std::nullopt;
        }
        return d;
    }

    std::optional<CubicBezier> ParseEasing(const Json& v, const std::string& path) {
        if (v.type == Type::String) {
            if (!ValidName(v.s)) { Err(path, "invalid easing token name '" + v.s + "'"); return std::nullopt; }
            return LookupEasing(v.s, path);
        }
        if (v.type != Type::Array) { TypeErr(path, "array [x1, y1, x2, y2] or easing token name", v); return std::nullopt; }
        if (v.vals.size() != 4) { Err(path, "expected 4 numbers [x1, y1, x2, y2], got " + std::to_string(v.vals.size())); return std::nullopt; }
        double c[4] = {0, 0, 0, 0}; bool ok = true;
        for (std::size_t i = 0; i < 4; ++i) {
            auto n = GetNumber(v.vals[i], Join(path, std::to_string(i)));
            if (!n) ok = false; else c[i] = *n;
        }
        if (!ok) return std::nullopt;
        if (c[0] < 0.0 || c[0] > 1.0) { Err(Join(path, "0"), "x1 must be in [0, 1] (got " + Fmt(c[0]) + ")"); ok = false; }
        if (c[2] < 0.0 || c[2] > 1.0) { Err(Join(path, "2"), "x2 must be in [0, 1] (got " + Fmt(c[2]) + ")"); ok = false; }
        if (!ok) return std::nullopt;
        return CubicBezier(c[0], c[1], c[2], c[3]);
    }

    std::optional<SpringParams> ParseSpring(const Json& v, const std::string& path) {
        if (v.type == Type::String) {
            if (!ValidName(v.s)) { Err(path, "invalid spring token name '" + v.s + "'"); return std::nullopt; }
            return LookupSpring(v.s, path);
        }
        if (v.type != Type::Object) { TypeErr(path, "object {response, damping} or spring token name", v); return std::nullopt; }
        bool ok = CheckKeys(v, path, {"response", "damping"}, {"response", "damping"});
        SpringParams p;
        if (const Json* r = v.Find("response")) {
            auto d = ParseDuration(*r, Join(path, "response"));
            if (!d) ok = false;
            else if (*d < 0.001 || *d > 10.0) { Err(Join(path, "response"), "response must be in [0.001, 10] seconds (got " + Fmt(*d) + ")"); ok = false; }
            else p.response = *d;
        }
        if (const Json* dj = v.Find("damping")) {
            auto d = GetNumber(*dj, Join(path, "damping"));
            if (!d) ok = false;
            else if (*d < 0.0 || *d > 100.0) { Err(Join(path, "damping"), "damping must be in [0, 100] (got " + Fmt(*d) + ")"); ok = false; }
            else p.damping = *d;
        }
        if (!ok) return std::nullopt;
        return p;
    }

    // ----- nodes -----
    NodePtr ParseNode(const Json& v, const std::string& path, int depth) {
        if (depth > kMaxNodeDepth) { Err(path, "nesting too deep (max " + std::to_string(kMaxNodeDepth) + ")"); return nullptr; }
        if (v.type != Type::Object) { TypeErr(path, "node object", v); return nullptr; }
        const Json* t = v.Find("type");
        if (!t) { Err(path, "missing required key 'type'"); return nullptr; }
        if (t->type != Type::String) { TypeErr(Join(path, "type"), "string", *t); return nullptr; }
        const std::string& ty = t->s;
        if (ty == "track") return ParseTrack(v, path);
        if (ty == "sequence" || ty == "parallel") {
            const bool keysOk = CheckKeys(v, path, {"type", "children"}, {"children"});
            std::vector<NodePtr> kids;
            const bool kidsOk = ParseChildren(v, path, depth, kids);
            if (!keysOk || !kidsOk) return nullptr;
            return ty == "sequence" ? Sequence(std::move(kids)) : Parallel(std::move(kids));
        }
        if (ty == "stagger") {
            bool ok = CheckKeys(v, path, {"type", "delay", "children"}, {"delay", "children"});
            std::optional<double> d;
            if (const Json* dj = v.Find("delay")) { d = ParseDuration(*dj, Join(path, "delay")); if (!d) ok = false; }
            std::vector<NodePtr> kids;
            if (!ParseChildren(v, path, depth, kids)) ok = false;
            if (!ok) return nullptr;
            return Stagger(*d, std::move(kids));
        }
        if (ty == "delay") {
            bool ok = CheckKeys(v, path, {"type", "duration"}, {"duration"});
            std::optional<double> d;
            if (const Json* dj = v.Find("duration")) { d = ParseDuration(*dj, Join(path, "duration")); if (!d) ok = false; }
            if (!ok) return nullptr;
            return Delay(*d);
        }
        if (ty == "marker") {
            bool ok = CheckKeys(v, path, {"type", "name"}, {"name"});
            std::string name;
            if (const Json* nj = v.Find("name")) {
                if (nj->type != Type::String) { TypeErr(Join(path, "name"), "string", *nj); ok = false; }
                else if (nj->s.empty()) { Err(Join(path, "name"), "marker name must not be empty"); ok = false; }
                else name = nj->s;
            }
            if (!ok) return nullptr;
            return Marker(std::move(name));
        }
        Err(Join(path, "type"), "unknown node type '" + ty + "' (expected track, sequence, parallel, stagger, delay, marker)");
        return nullptr;
    }

    bool ParseChildren(const Json& v, const std::string& path, int depth, std::vector<NodePtr>& out) {
        const Json* c = v.Find("children");
        if (!c) return false;                       // missing key already reported
        const std::string cp = Join(path, "children");
        if (c->type != Type::Array) { TypeErr(cp, "array", *c); return false; }
        if (c->vals.empty()) { Err(cp, "must contain at least one child"); return false; }
        bool ok = true;
        for (std::size_t i = 0; i < c->vals.size(); ++i) {
            NodePtr n = ParseNode(c->vals[i], Join(cp, std::to_string(i)), depth + 1);
            if (n) out.push_back(std::move(n)); else ok = false;
        }
        return ok;
    }

    NodePtr ParseTrack(const Json& v, const std::string& path) {
        bool ok = CheckKeys(v, path, {"type", "property", "from", "steps"}, {"property", "from", "steps"});
        std::string prop;
        if (const Json* pj = v.Find("property")) {
            if (pj->type != Type::String) { TypeErr(Join(path, "property"), "string", *pj); ok = false; }
            else if (pj->s.empty()) { Err(Join(path, "property"), "property must not be empty"); ok = false; }
            else prop = pj->s;
        }
        std::optional<double> from;
        if (const Json* fj = v.Find("from")) { from = GetNumber(*fj, Join(path, "from")); if (!from) ok = false; }
        std::vector<Step> steps;
        if (const Json* sj = v.Find("steps")) {
            const std::string sp = Join(path, "steps");
            if (sj->type != Type::Array) { TypeErr(sp, "array", *sj); ok = false; }
            else if (sj->vals.empty()) { Err(sp, "must contain at least one step"); ok = false; }
            else for (std::size_t i = 0; i < sj->vals.size(); ++i) {
                auto st = ParseStep(sj->vals[i], Join(sp, std::to_string(i)));
                if (st) steps.push_back(*st); else ok = false;
            }
        }
        if (!ok) return nullptr;
        Track t(prop, *from);
        for (const Step& s : steps) {
            switch (s.kind) {
                case Step::To:       t.To(s.value, s.duration, s.curve); break;
                case Step::SpringTo: t.SpringTo(s.value, s.sp, s.inherit); break;
                case Step::Hold:     t.Hold(s.duration); break;
            }
        }
        return Animate(std::move(t));
    }

    std::optional<Step> ParseStep(const Json& s, const std::string& path) {
        if (s.type != Type::Object) { TypeErr(path, "step object", s); return std::nullopt; }
        const Json* to = s.Find("to"); const Json* st = s.Find("springTo"); const Json* hold = s.Find("hold");
        const int n = (to ? 1 : 0) + (st ? 1 : 0) + (hold ? 1 : 0);
        if (n != 1) {
            Err(path, n == 0 ? "step must contain exactly one of 'to', 'springTo', 'hold'"
                             : "step has more than one of 'to', 'springTo', 'hold'");
            CheckKeys(s, path, {"to", "springTo", "hold", "duration", "ease", "spring", "inheritVelocity"}, {});
            return std::nullopt;
        }
        Step r; bool ok = true;
        if (to) {
            r.kind = Step::To;
            ok = CheckKeys(s, path, {"to", "duration", "ease"}, {"duration"});
            auto val = GetNumber(*to, Join(path, "to")); if (!val) ok = false; else r.value = *val;
            if (const Json* dj = s.Find("duration")) { auto d = ParseDuration(*dj, Join(path, "duration")); if (!d) ok = false; else r.duration = *d; }
            r.curve = Curve::Linear();
            if (const Json* ej = s.Find("ease")) { auto e = ParseEasing(*ej, Join(path, "ease")); if (!e) ok = false; else r.curve = Curve::Bezier(*e); }
        } else if (st) {
            r.kind = Step::SpringTo;
            ok = CheckKeys(s, path, {"springTo", "spring", "inheritVelocity"}, {"spring"});
            auto val = GetNumber(*st, Join(path, "springTo")); if (!val) ok = false; else r.value = *val;
            if (const Json* sj = s.Find("spring")) { auto sp = ParseSpring(*sj, Join(path, "spring")); if (!sp) ok = false; else r.sp = *sp; }
            if (const Json* ij = s.Find("inheritVelocity")) {
                if (ij->type != Type::Bool) { TypeErr(Join(path, "inheritVelocity"), "boolean", *ij); ok = false; } else r.inherit = ij->b;
            }
        } else {
            r.kind = Step::Hold;
            ok = CheckKeys(s, path, {"hold"}, {});
            auto d = ParseDuration(*hold, Join(path, "hold")); if (!d) ok = false; else r.duration = *d;
        }
        if (!ok) return std::nullopt;
        return r;
    }

    void CompileTransitions(std::vector<Pending>& out) {
        const Json* sec = doc_.Find("transitions");
        if (!sec) return;
        if (sec->type != Type::Object) { TypeErr("/transitions", "object", *sec); return; }
        for (std::size_t i = 0; i < sec->keys.size(); ++i) {
            const std::string& name = sec->keys[i];
            const std::string p = Join("/transitions", name);
            const Json& e = sec->vals[i];
            if (!ValidName(name)) { Err(p, "invalid transition name '" + name + "' (must match [A-Za-z][A-Za-z0-9_.-]*)"); continue; }
            if (e.type != Type::Object) { TypeErr(p, "object {root, reduced}", e); continue; }
            bool ok = CheckKeys(e, p, {"root", "reduced"}, {"root", "reduced"});
            Pending pe; pe.name = name;
            if (const Json* r = e.Find("root")) { pe.root = ParseNode(*r, Join(p, "root"), 0); if (!pe.root) ok = false; }
            if (const Json* rd = e.Find("reduced")) {
                const std::string rp = Join(p, "reduced");
                if (rd->type == Type::String) {
                    if (rd->s == "instant") pe.instant = true;
                    else { Err(rp, "expected a node object or \"instant\", got string '" + rd->s + "'"); ok = false; }
                } else { pe.reduced = ParseNode(*rd, rp, 0); if (!pe.reduced) ok = false; }
            }
            if (ok) out.push_back(std::move(pe));
        }
    }

    // Every animated property jumps to its final value at t = 0.
    static NodePtr MakeInstant(const Timeline& tl) {
        std::vector<NodePtr> kids;
        for (const PlacedTrack& pt : tl.Tracks())
            kids.push_back(Animate(Track(pt.track->Property(), pt.track->InitialValue()).To(pt.track->FinalValue(), 0.0)));
        return Parallel(std::move(kids));
    }

    const Json& doc_;
    std::vector<Diagnostic>& errs_;
    Table<double> dur_{"duration"};
    Table<CubicBezier> easing_{"easing"};
    Table<SpringParams> spring_{"spring"};
};

}  // namespace detail

inline std::optional<TokenSet> TokenSet::Parse(std::string_view json, std::vector<Diagnostic>& errors) {
    errors.clear();
    detail::Json doc;
    std::string perr;
    if (!detail::JsonParser(json).Parse(doc, perr)) {
        errors.push_back({"", "JSON syntax error: " + perr});
        return std::nullopt;
    }
    return detail::Compiler(doc, errors).Run();
}

}  // namespace motion
