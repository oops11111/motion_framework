// Tests for tokens.h.  Build: g++ -std=c++17 -O1 -Wall -Wextra tokens_tests.cpp -o tok && ./tok [motion.json]
#include "tokens.h"
#include <cstdio>
#include <fstream>
#include <sstream>

using namespace motion;
static int g_fail = 0, g_checks = 0;
#define CHECK(c) do { ++g_checks; if (!(c)) { ++g_fail; std::printf("FAIL %s:%d  %s\n", __FILE__, __LINE__, #c); } } while (0)
#define NEAR(a, b, tol) do { ++g_checks; double _a = (a), _b = (b); \
  if (!(std::abs(_a - _b) <= (tol))) { ++g_fail; \
  std::printf("FAIL %s:%d  %s=%.12g vs %s=%.12g\n", __FILE__, __LINE__, #a, _a, #b, _b); } } while (0)

static bool Has(const std::string& s, const std::string& sub) { return s.find(sub) != std::string::npos; }

// Expect Parse to fail with a diagnostic at exactly `path` whose message contains `msg`.
static void Bad(const std::string& json, const std::string& path, const std::string& msg, int line) {
    std::vector<Diagnostic> errs;
    auto ts = TokenSet::Parse(json, errs);
    ++g_checks;
    bool found = false;
    for (auto& e : errs) if (e.path == path && Has(e.message, msg)) found = true;
    if (ts || !found) {
        ++g_fail;
        std::printf("FAIL line %d: expected [%s] '%s'; %s; got:\n", line, path.c_str(), msg.c_str(), ts ? "but it parsed" : "diagnostics differ");
        for (auto& e : errs) std::printf("    %s\n", e.ToString().c_str());
    }
}
#define BAD(json, path, msg) Bad(json, path, msg, __LINE__)

static std::string Doc(const std::string& frag) { return "{\"version\":1," + frag + "}"; }
static std::string TR(const std::string& steps) { return "{\"type\":\"track\",\"property\":\"p\",\"from\":0,\"steps\":" + steps + "}"; }
static std::string Tx(const std::string& root, const std::string& reduced = "\"instant\"") {
    return Doc("\"transitions\":{\"t\":{\"root\":" + root + ",\"reduced\":" + reduced + "}}");
}

static void TestJsonParser() {
    using detail::Json; using detail::JsonParser;
    auto P = [](const std::string& s, Json& j, std::string& e) { return JsonParser(s).Parse(j, e); };
    Json j; std::string e;
    CHECK(P("{\"a\":[1,2.5,-0,1e2,true,false,null,\"x\"]}", j, e));
    CHECK(j.Find("a")->vals.size() == 8 && j.Find("a")->vals[3].n == 100.0);
    CHECK(P("\"\\u00e9\\ud83d\\ude00\\n\"", j, e) && j.s == "\xC3\xA9\xF0\x9F\x98\x80\n");
    CHECK(P("\xEF\xBB\xBF{}", j, e));                       // BOM ok
    CHECK(!P("{\"a\":1,}", j, e));  CHECK(!P("[1,]", j, e));  CHECK(!P("{\"a\":1,\"a\":2}", j, e) && Has(e, "duplicate key"));
    CHECK(!P("01", j, e));          CHECK(!P("1.", j, e));    CHECK(!P("-", j, e));   CHECK(!P(".5", j, e));
    CHECK(!P("1e", j, e));          CHECK(!P("1e999", j, e) && Has(e, "out of range"));
    CHECK(!P("\"\\ud800\"", j, e)); CHECK(!P("\"\\x\"", j, e)); CHECK(!P("\"a\nb\"", j, e));
    CHECK(!P("{} x", j, e));        CHECK(!P("", j, e));      CHECK(!P("tru", j, e)); CHECK(!P("'a'", j, e));
    CHECK(!P("[\n\n  @]", j, e) && Has(e, "line 3, col 3"));
    CHECK(P(std::string(128, '[') + std::string(128, ']'), j, e));
    CHECK(!P(std::string(140, '[') + std::string(140, ']'), j, e) && Has(e, "too deep"));
}

static const char* kRoundTrip = R"({
 "version":1,
 "durations":{"fast":"100ms","normal":"0.25s","default":"normal"},
 "easing":{"standard":[0.33,0,0.67,1],"enter":"standard"},
 "springs":{"snappy":{"response":"300ms","damping":0.8}},
 "transitions":{
  "card.enter":{
   "root":{"type":"sequence","children":[
     {"type":"parallel","children":[
       {"type":"track","property":"opacity","from":0,"steps":[{"to":1,"duration":"default","ease":"enter"}]},
       {"type":"track","property":"scale","from":0.96,"steps":[{"springTo":1,"spring":"snappy"}]}]},
     {"type":"marker","name":"entered"},
     {"type":"delay","duration":"fast"},
     {"type":"stagger","delay":"50ms","children":[
       {"type":"track","property":"y0","from":10,"steps":[{"to":0,"duration":"200ms","ease":[0.2,0,0,1]},{"hold":"100ms"}]},
       {"type":"track","property":"y1","from":10,"steps":[{"to":0,"duration":"200ms"}]}]}]},
   "reduced":{"type":"track","property":"opacity","from":0,"steps":[{"to":1,"duration":"fast"}]}},
  "card.exit":{
   "root":{"type":"sequence","children":[
     {"type":"track","property":"x","from":0,"steps":[{"to":10,"duration":"1s"}]},
     {"type":"track","property":"x","from":50,"steps":[{"to":60,"duration":"1s"}]},
     {"type":"track","property":"s","from":1,"steps":[{"to":0.5,"duration":"100ms"},{"springTo":1,"spring":{"response":"250ms","damping":0.7},"inheritVelocity":true}]}]},
   "reduced":"instant"}
 }})";

static void TestRoundTrip() {
    std::vector<Diagnostic> errs;
    auto ts = TokenSet::Parse(kRoundTrip, errs);
    CHECK(ts.has_value());
    if (!ts) { for (auto& e : errs) std::printf("    %s\n", e.ToString().c_str()); return; }
    NEAR(*ts->Duration("fast"), 0.1, 0); NEAR(*ts->Duration("default"), 0.25, 0);
    NEAR(ts->Easing("enter")->Ease(0.5), CubicBezier(0.33, 0, 0.67, 1).Ease(0.5), 0);
    NEAR(ts->Spring("snappy")->response, 0.3, 0); NEAR(ts->Spring("snappy")->damping, 0.8, 0);
    CHECK(ts->Easing("linear").has_value() && !ts->Easing("standardx") && !ts->Duration("nope") && !ts->Spring("nope"));
    CHECK(ts->TransitionNames().size() == 2 && ts->HasTransition("card.enter") && !ts->HasTransition("x"));
    CHECK(ts->BuildTimeline("missing") == nullptr);

    auto hand = std::make_shared<const Timeline>(Sequence({
        Parallel({Animate(Track("opacity", 0).To(1, 0.25, Curve::Bezier(0.33, 0, 0.67, 1))),
                  Animate(Track("scale", 0.96).SpringTo(1, {0.3, 0.8}))}),
        Marker("entered"), Delay(0.1),
        Stagger(0.05, {Animate(Track("y0", 10).To(0, 0.2, Curve::Bezier(0.2, 0, 0, 1)).Hold(0.1)),
                       Animate(Track("y1", 10).To(0, 0.2))})}));
    auto loaded = ts->BuildTimeline("card.enter");
    NEAR(loaded->Duration(), hand->Duration(), 1e-12);
    CHECK(loaded->Markers().size() == 1 && loaded->Markers()[0].name == "entered");
    NEAR(loaded->Markers()[0].time, hand->Markers()[0].time, 1e-12);
    int compared = 0;
    for (double t = 0; t <= hand->Duration() + 0.05; t += 0.013) {
        SampleMap a = loaded->Evaluate(t), b = hand->Evaluate(t);
        CHECK(a.size() == b.size());
        for (auto& kv : b) {
            NEAR(a.at(kv.first).value, kv.second.value, 1e-12);
            NEAR(a.at(kv.first).velocity, kv.second.velocity, 1e-9);
            ++compared;
        }
    }
    CHECK(compared > 200);
    // timelines are shared immutable objects
    CHECK(ts->BuildTimeline("card.enter") == loaded);

    // reduced variants
    auto red = ts->BuildTimeline("card.enter", true);
    NEAR(red->Duration(), 0.1, 1e-12); CHECK(red->Evaluate(0.05).count("opacity") == 1 && red->Evaluate(0.05).size() == 1);
    auto exitFull = ts->BuildTimeline("card.exit"), exitRed = ts->BuildTimeline("card.exit", true);
    NEAR(exitRed->Duration(), 0.0, 0);
    SampleMap end = exitFull->Evaluate(exitFull->Duration()), inst = exitRed->Evaluate(0.0);
    CHECK(end.size() == inst.size());
    for (auto& kv : end) { NEAR(inst.at(kv.first).value, kv.second.value, 1e-12); NEAR(inst.at(kv.first).velocity, 0, 0); }
    NEAR(inst.at("x").value, 60, 1e-12);   // conflict rule preserved: last track wins
    // the loaded timeline plays through the Player
    ManualClock ck; Player pl(loaded, ck); int marks = 0; pl.OnMarker([&](const std::string&, double) { ++marks; });
    pl.Play(); ck.Advance(5); pl.Update(); CHECK(marks == 1 && pl.IsFinished());
}

static void TestValid() {
    std::vector<Diagnostic> errs;
    CHECK(TokenSet::Parse("{\"version\":1}", errs).has_value() && errs.empty());
    CHECK(TokenSet::Parse("{\"version\":1,\"$schema\":\"x\",\"$comment\":1}", errs).has_value());
    // units and aliases
    auto ts = TokenSet::Parse(Doc("\"durations\":{\"a\":\"1.5s\",\"b\":\"250ms\",\"c\":\"b\",\"d\":\"c\",\"z\":\"0s\",\"m\":\"600s\"}"), errs);
    CHECK(ts.has_value());
    if (ts) { NEAR(*ts->Duration("a"), 1.5, 0); NEAR(*ts->Duration("b"), 0.25, 0); NEAR(*ts->Duration("d"), 0.25, 0);
              NEAR(*ts->Duration("z"), 0, 0); NEAR(*ts->Duration("m"), 600, 0); }
    // built-in linear usable without an easing section; user token may override it
    CHECK(TokenSet::Parse(Tx(TR("[{\"to\":1,\"duration\":\"1s\",\"ease\":\"linear\"}]")), errs).has_value());
    auto ov = TokenSet::Parse(Doc("\"easing\":{\"linear\":[0.5,0,0.5,1]}"), errs);
    CHECK(ov && std::abs(ov->Easing("linear")->Ease(0.25) - 0.25) > 1e-3);
    // y control points may leave [0,1]
    CHECK(TokenSet::Parse(Doc("\"easing\":{\"back\":[0.3,-0.5,0.7,1.5]}"), errs).has_value());
    // inline spring/easing, all node kinds, unused tokens still validated
    CHECK(TokenSet::Parse(Tx("{\"type\":\"parallel\",\"children\":[" + TR("[{\"hold\":\"10ms\"}]") + ",{\"type\":\"delay\",\"duration\":\"1s\"},{\"type\":\"marker\",\"name\":\"m\"}]}"), errs).has_value());
    // 32 levels of nesting is the maximum allowed
    std::string deep = TR("[{\"hold\":\"1ms\"}]");
    for (int i = 0; i < 31; ++i) deep = "{\"type\":\"sequence\",\"children\":[" + deep + "]}";
    CHECK(TokenSet::Parse(Tx(deep), errs).has_value());
}

static void TestInvalid() {
    // syntax
    BAD("{\"version\":1,}", "", "JSON syntax error");
    BAD("[]", "", "expected object, got array");
    // top level
    BAD("{}", "", "missing required key 'version'");
    BAD("{\"version\":2}", "/version", "unsupported version 2");
    BAD("{\"version\":\"1\"}", "/version", "expected number, got string");
    BAD(Doc("\"foo\":1"), "/foo", "unknown key 'foo'");
    BAD(Doc("\"durations\":[]"), "/durations", "expected object, got array");
    // durations
    BAD(Doc("\"durations\":{\"a\":120}"), "/durations/a", "expected string");
    BAD(Doc("\"durations\":{\"a\":\"12\"}"), "/durations/a", "invalid duration literal");
    BAD(Doc("\"durations\":{\"a\":\"12xs\"}"), "/durations/a", "invalid duration literal");
    BAD(Doc("\"durations\":{\"a\":\"-5ms\"}"), "/durations/a", "duration must be in [0, 600]");
    BAD(Doc("\"durations\":{\"a\":\"700s\"}"), "/durations/a", "duration must be in [0, 600]");
    BAD(Doc("\"durations\":{\"a\":\"nope\"}"), "/durations/a", "unknown duration token 'nope'");
    BAD(Doc("\"durations\":{\"1bad\":\"1s\"}"), "/durations/1bad", "invalid token name");
    BAD(Doc("\"durations\":{\"a/b\":\"1s\"}"), "/durations/a~1b", "invalid token name");
    // easing
    BAD(Doc("\"easing\":{\"e\":[1.2,0,0.5,1]}"), "/easing/e/0", "x1 must be in [0, 1] (got 1.2)");
    BAD(Doc("\"easing\":{\"e\":[0.2,0,-0.1,1]}"), "/easing/e/2", "x2 must be in [0, 1]");
    BAD(Doc("\"easing\":{\"e\":[0,0,1]}"), "/easing/e", "got 3");
    BAD(Doc("\"easing\":{\"e\":[0,0,\"a\",1]}"), "/easing/e/2", "expected number, got string");
    BAD(Doc("\"easing\":{\"e\":5}"), "/easing/e", "expected array");
    BAD(Doc("\"easing\":{\"e\":\"nope\"}"), "/easing/e", "unknown easing token 'nope'");
    // circular references
    BAD(Doc("\"durations\":{\"a\":\"b\",\"b\":\"a\"}"), "/durations/b", "circular reference: a -> b -> a");
    BAD(Doc("\"durations\":{\"a\":\"c\",\"b\":\"a\",\"c\":\"b\"}"), "/durations/b", "circular reference: a -> c -> b -> a");
    BAD(Doc("\"easing\":{\"x\":\"x\"}"), "/easing/x", "circular reference: x -> x");
    BAD(Doc("\"springs\":{\"a\":\"b\",\"b\":\"a\"}"), "/springs/b", "circular reference: a -> b -> a");
    // springs
    BAD(Doc("\"springs\":{\"s\":{\"response\":\"300ms\"}}"), "/springs/s", "missing required key 'damping'");
    BAD(Doc("\"springs\":{\"s\":{\"response\":\"0ms\",\"damping\":1}}"), "/springs/s/response", "response must be in [0.001, 10]");
    BAD(Doc("\"springs\":{\"s\":{\"response\":\"300ms\",\"damping\":-1}}"), "/springs/s/damping", "damping must be in [0, 100]");
    BAD(Doc("\"springs\":{\"s\":{\"response\":\"300ms\",\"damping\":1,\"mass\":2}}"), "/springs/s/mass", "unknown key 'mass'");
    BAD(Doc("\"springs\":{\"s\":{\"response\":0.3,\"damping\":1}}"), "/springs/s/response", "expected string");
    // transitions
    BAD(Doc("\"transitions\":{\"t\":{\"root\":" + TR("[{\"hold\":\"1s\"}]") + "}}"), "/transitions/t", "missing required key 'reduced'");
    BAD(Tx("{\"type\":\"wobble\"}"), "/transitions/t/root/type", "unknown node type 'wobble'");
    BAD(Tx("{\"children\":[]}"), "/transitions/t/root", "missing required key 'type'");
    BAD(Tx("{\"type\":\"sequence\",\"children\":[]}"), "/transitions/t/root/children", "at least one child");
    BAD(Tx("{\"type\":\"sequence\",\"children\":[],\"x\":1}"), "/transitions/t/root/x", "unknown key 'x'");
    BAD(Tx("{\"type\":\"stagger\",\"children\":[" + TR("[{\"hold\":\"1s\"}]") + "]}"), "/transitions/t/root", "missing required key 'delay'");
    BAD(Tx("{\"type\":\"marker\",\"name\":\"\"}"), "/transitions/t/root/name", "must not be empty");
    BAD(Tx(TR("[]")), "/transitions/t/root/steps", "at least one step");
    BAD(Tx("{\"type\":\"track\",\"property\":\"p\",\"steps\":[{\"hold\":\"1s\"}]}"), "/transitions/t/root", "missing required key 'from'");
    BAD(Tx(TR("[{\"to\":1,\"hold\":\"1s\"}]")), "/transitions/t/root/steps/0", "more than one of");
    BAD(Tx(TR("[{\"duration\":\"1s\"}]")), "/transitions/t/root/steps/0", "exactly one of");
    BAD(Tx(TR("[{\"to\":1,\"duration\":\"1s\",\"foo\":2}]")), "/transitions/t/root/steps/0/foo", "unknown key 'foo'");
    BAD(Tx(TR("[{\"to\":1}]")), "/transitions/t/root/steps/0", "missing required key 'duration'");
    BAD(Tx(TR("[{\"to\":1,\"duration\":\"1s\",\"ease\":\"nope\"}]")), "/transitions/t/root/steps/0/ease", "unknown easing token 'nope'");
    BAD(Tx(TR("[{\"to\":1,\"duration\":\"1s\",\"ease\":[2,0,0,1]}]")), "/transitions/t/root/steps/0/ease/0", "x1 must be in [0, 1]");
    BAD(Tx(TR("[{\"springTo\":1}]")), "/transitions/t/root/steps/0", "missing required key 'spring'");
    BAD(Tx(TR("[{\"springTo\":1,\"spring\":\"nope\"}]")), "/transitions/t/root/steps/0/spring", "unknown spring token 'nope'");
    BAD(Tx(TR("[{\"springTo\":1,\"spring\":\"x\",\"inheritVelocity\":1}]")), "/transitions/t/root/steps/0/inheritVelocity", "expected boolean");
    BAD(Tx(TR("[{\"to\":1,\"duration\":\"-1s\"}]")), "/transitions/t/root/steps/0/duration", "duration must be in");
    BAD(Tx(TR("[{\"hold\":5}]")), "/transitions/t/root/steps/0/hold", "expected string");
    BAD(Tx(TR("[{\"to\":\"1\",\"duration\":\"1s\"}]")), "/transitions/t/root/steps/0/to", "expected number, got string");
    BAD(Tx(TR("[{\"hold\":\"1s\"}]"), "\"bogus\""), "/transitions/t/reduced", "\"instant\"");
    BAD(Tx(TR("[{\"hold\":\"1s\"}]"), "5"), "/transitions/t/reduced", "expected node object");
    BAD(Doc("\"transitions\":{\"bad name\":{}}"), "/transitions/bad name", "invalid transition name");
    {   std::string deep = TR("[{\"hold\":\"1ms\"}]");
        for (int i = 0; i < 40; ++i) deep = "{\"type\":\"sequence\",\"children\":[" + deep + "]}";
        std::vector<Diagnostic> errs; CHECK(!TokenSet::Parse(Tx(deep), errs));
        bool tooDeep = false; for (auto& e : errs) if (Has(e.message, "nesting too deep")) tooDeep = true; CHECK(tooDeep); }
    // easing "standard" is not built in: designers must define it
    BAD(Tx(TR("[{\"to\":1,\"duration\":\"1s\",\"ease\":\"standard\"}]")), "/transitions/t/root/steps/0/ease", "unknown easing token 'standard'");
    // all problems are collected, not just the first
    std::vector<Diagnostic> errs;
    CHECK(!TokenSet::Parse(Doc("\"foo\":1,\"durations\":{\"a\":\"x1\",\"b\":3},\"easing\":{\"e\":[5,0,0,1]}"), errs) && errs.size() >= 4);
}

int main(int argc, char** argv) {
    TestJsonParser(); TestRoundTrip(); TestValid(); TestInvalid();
    if (argc > 1) {
        std::ifstream f(argv[1]); std::stringstream ss; ss << f.rdbuf();
        std::vector<Diagnostic> errs; auto ts = TokenSet::Parse(ss.str(), errs);
        CHECK(ts.has_value());
        for (auto& e : errs) std::printf("    %s\n", e.ToString().c_str());
        if (ts) {
            CHECK(ts->TransitionNames().size() == 4);
            for (auto& n : ts->TransitionNames()) {
                CHECK(ts->BuildTimeline(n) && ts->BuildTimeline(n, true));
                CHECK(ts->BuildTimeline(n)->Duration() > 0.0);
            }
            NEAR(ts->BuildTimeline("list.stagger-in")->Duration(), 0.08 + 0.2, 1e-12);
            NEAR(*ts->Duration("default"), 0.2, 0);
            CHECK(ts->BuildTimeline("card.enter")->MarkerTime("entered").has_value());
            std::printf("  sample %s: %zu transitions OK\n", argv[1], ts->TransitionNames().size());
        }
    }
    std::printf("%d checks, %d failures\n", g_checks, g_fail);
    return g_fail ? 1 : 0;
}
