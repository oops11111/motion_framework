// code/capi/fm_capi.cpp. Build (Linux test, from code/capi): g++ -std=c++17 -O2 -shared -fPIC -I.. -I../explore fm_capi.cpp -o libfluidmotion.so
//   Windows (verified 2026-10-19, MSVC 19.44): cl /std:c++17 /EHsc /O2 /LD /I.. /I..\explore fm_capi.cpp /Fe:fluidmotion.dll   (x64)
//                         or:  g++ -std=c++17 -O2 -shared -static-libgcc -static-libstdc++ -o fluidmotion.dll -I.. -I../explore fm_capi.cpp
#include "fm_capi.h"
#include "software_dispatcher.h"
#include <cstdio>
#include <cstring>
#include <map>
#include <memory>
#include <optional>

using namespace motion;

namespace {
struct CSink final : IPropertySink {
    fm_sink_fn fn = nullptr; void* user = nullptr;
    void Set(TargetId t, const std::string& p, double v) override { if (fn) fn(t, p.c_str(), v, user); }
};
}  // namespace

struct fm_system {
    std::unique_ptr<IClock> clock;
    ManualClock* manual = nullptr;
    CSink sink;
    SoftwareDispatcher disp;
    MotionSystem sys;
    fm_render_fn render; void* user;
    FramePump pump;
    std::optional<TokenSet> tokens;
    std::map<uint64_t, AnimationHandle> handles;
    uint64_t next = 0;

    fm_system(std::unique_ptr<IClock> c, ManualClock* m, fm_sink_fn s, fm_render_fn r, void* u)
        : clock(std::move(c)), manual(m), sink(), disp(sink), sys(*clock, &disp), render(r), user(u),
          pump(sys, [this](bool on) { if (render) render(on ? 1 : 0, user); }) {
        sink.fn = s; sink.user = u;
    }
    AnimationHandle* Find(uint64_t id) { auto it = handles.find(id); return it == handles.end() ? nullptr : &it->second; }
};

extern "C" {

fm_system* fm_create(fm_sink_fn s, fm_render_fn r, void* u) {
    try { return new fm_system(std::make_unique<SteadyClock>(), nullptr, s, r, u); } catch (...) { return nullptr; }
}
fm_system* fm_create_manual(fm_sink_fn s, fm_render_fn r, void* u) {
    try { auto m = std::make_unique<ManualClock>(); ManualClock* raw = m.get(); return new fm_system(std::move(m), raw, s, r, u); }
    catch (...) { return nullptr; }
}
void fm_manual_advance(fm_system* s, double dt) { if (s && s->manual) s->manual->Advance(dt); }
void fm_destroy(fm_system* s) { delete s; }   // handles die before the system: they become inert, no callbacks fire

int fm_load_tokens(fm_system* s, const char* json, char* err, int cap) {
    if (!s || !json) return 1;
    try {
        std::vector<Diagnostic> errs;
        auto t = TokenSet::Parse(json, errs);
        if (t) { s->tokens = std::move(t); if (err && cap > 0) err[0] = 0; return 0; }
        if (err && cap > 0) {
            std::string all; for (auto& e : errs) all += e.ToString() + "\n";
#ifdef _MSC_VER
            strncpy_s(err, static_cast<size_t>(cap), all.c_str(), _TRUNCATE);   // bounded, always NUL-terminated; no C4996
#else
            std::snprintf(err, static_cast<size_t>(cap), "%s", all.c_str());      // portable bounded copy (glibc has no strncpy_s)
#endif
        }
        return static_cast<int>(errs.size());
    } catch (...) { return 1; }
}

void fm_set_reduced_motion(fm_system* s, int on) {
    if (!s) return;
    try { s->sys.SetReducedMotion(on != 0); s->pump.Wake(); } catch (...) {}
}

uint64_t fm_animation_create(fm_system* s, uint64_t target, const char* transition) {
    if (!s) return 0;
    try {
        AnimationHandle h;
        if (!transition) h = s->sys.Create(target);
        else if (s->tokens) h = s->sys.Create(target, *s->tokens, transition);
        if (!h) return 0;
        s->handles.emplace(++s->next, std::move(h));
        return s->next;
    } catch (...) { return 0; }
}
void fm_animation_play(fm_system* s, uint64_t id) {
    if (!s) return;
    try { if (auto* h = s->Find(id)) { h->Play(); s->pump.Wake(); } } catch (...) {}
}
void fm_animation_cancel(fm_system* s, uint64_t id) {
    if (!s) return;
    try { if (auto* h = s->Find(id)) h->Cancel(); } catch (...) {}
}
void fm_animation_release(fm_system* s, uint64_t id) { if (s) s->handles.erase(id); }

int fm_animation_retarget(fm_system* s, uint64_t id, const char* prop, double target, double response,
                          double damping, int hasFrom, double from) {
    if (!s || !prop) return 0;
    try {
        auto* h = s->Find(id);
        if (!h) return 0;
        const bool ok = h->Retarget(prop, target, SpringParams{response, damping},
                                    hasFrom ? std::optional<double>(from) : std::nullopt);
        if (ok) s->pump.Wake();
        return ok ? 1 : 0;
    } catch (...) { return 0; }
}

void fm_on_frame(fm_system* s) { if (s) { try { s->pump.OnFrame(); } catch (...) {} } }
int fm_is_running(fm_system* s) { return s && s->pump.Running() ? 1 : 0; }
int fm_current(fm_system* s, uint64_t target, const char* prop, double* v, double* vel) {
    if (!s || !prop) return 0;
    auto c = s->sys.Current(target, prop);
    if (!c) return 0;
    if (v) *v = c->value;
    if (vel) *vel = c->velocity;
    return 1;
}

}  // extern "C"
