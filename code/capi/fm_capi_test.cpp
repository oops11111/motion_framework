// Exercises ONLY the C ABI, exactly as the P/Invoke layer will. Build:
//   g++ -std=c++17 -O1 -g -Wall -Wextra -Wpedantic -fsanitize=address,undefined fm_capi_test.cpp fm_capi.cpp -o capi && ./capi demo_motion.json
#include "fm_capi.h"
#include <cmath>
#include <cstdio>
#include <fstream>
#include <map>
#include <sstream>
#include <string>
static int g_fail = 0, g_checks = 0;
#define CHECK(c) do { ++g_checks; if (!(c)) { ++g_fail; std::printf("FAIL %s:%d  %s\n", __FILE__, __LINE__, #c); } } while (0)

struct Ctx { std::map<std::string, double> last; int sinkCalls = 0; int renderOn = 0, renderOff = 0; bool running = false; };
static void Sink(uint64_t, const char* p, double v, void* u) { auto* c = static_cast<Ctx*>(u); c->last[p] = v; ++c->sinkCalls; }
static void Render(int on, void* u) { auto* c = static_cast<Ctx*>(u); (on ? c->renderOn : c->renderOff)++; c->running = on != 0; }

static int RunFrames(fm_system* s, int max) {   // 120 Hz frames until the app's frame source would stop
    int n = 0;
    while (fm_is_running(s) && n < max) { fm_manual_advance(s, 1.0 / 120.0); fm_on_frame(s); ++n; }
    return n;
}

int main(int argc, char** argv) {
    std::string json;
    { std::ifstream in(argc > 1 ? argv[1] : "demo_motion.json"); std::stringstream b; b << in.rdbuf(); json = b.str(); }
    Ctx c; fm_system* s = fm_create_manual(Sink, Render, &c);
    CHECK(s != nullptr);
    CHECK(fm_active_count(nullptr) == 0);   // MF-06: null system reports no active controllers
    char err[512];

    // tokens: good and bad
    CHECK(fm_load_tokens(s, json.c_str(), err, sizeof err) == 0);
    CHECK(fm_load_tokens(s, "{\"version\":1,\"bogus\":1}", err, sizeof err) == 1 && std::string(err).find("unknown key") != std::string::npos);
    CHECK(fm_load_tokens(s, "{\"version\":1,\"durations\":{\"a\":\"b\",\"b\":\"a\"}}", err, sizeof err) >= 1 && std::string(err).find("circular") != std::string::npos);
    CHECK(fm_animation_create(s, 1, "no.such") == 0);

    // token transition: pump starts, runs, stops by itself, ends exactly on the final values
    uint64_t a = fm_animation_create(s, 1, "demo.enter");
    CHECK(a != 0 && !fm_is_running(s));
    fm_animation_play(s, a);
    CHECK(fm_is_running(s) && c.running && c.renderOn == 1);
    CHECK(fm_active_count(s) > 0);   // MF-06: one live controller while the transition runs
    const int frames = RunFrames(s, 1000);
    CHECK(!fm_is_running(s) && !c.running && c.renderOff == 1 && frames > 20 && frames < 200);
    CHECK(fm_active_count(s) == 0);   // MF-06: drained animations leave no active controllers
    CHECK(c.last.at("opacity") == 1.0 && c.last.at("offsetX") == 0.0);
    std::printf("  demo.enter: %d frames (%.0f ms), %d sink calls\n", frames, frames * 1000.0 / 120.0, c.sinkCalls);
    const int calls = c.sinkCalls; fm_on_frame(s); fm_on_frame(s); CHECK(c.sinkCalls == calls);   // idle writes nothing

    // interruptible Retarget: velocity carries; both entry points wake the pump
    uint64_t d = fm_animation_create(s, 1, nullptr);
    CHECK(fm_animation_retarget(s, d, "offsetX", 240.0, 0.32, 0.78, 0, 0.0) == 1);   // seeds from the rest value (0)
    CHECK(fm_is_running(s));
    for (int i = 0; i < 12; ++i) { fm_manual_advance(s, 1.0 / 120.0); fm_on_frame(s); }
    double v1 = 0, vel1 = 0; CHECK(fm_current(s, 1, "offsetX", &v1, &vel1) == 1 && v1 > 0 && vel1 > 0);
    CHECK(fm_animation_retarget(s, d, "offsetX", 0.0, 0.32, 0.78, 0, 0.0) == 1);
    double v2 = 0, vel2 = 0; CHECK(fm_current(s, 1, "offsetX", &v2, &vel2) == 1);
    CHECK(std::fabs(v2 - v1) < 1e-9 && std::fabs(vel2 - vel1) < 1e-9);                 // no jump at the interruption
    RunFrames(s, 1000); CHECK(c.last.at("offsetX") == 0.0);
    CHECK(fm_animation_retarget(s, 9999, "x", 1, 0.3, 0.8, 1, 0) == 0);                // unknown animation id
    CHECK(fm_animation_retarget(s, d, "nobody", 1, 0.3, 0.8, 0, 0) == 0);              // no value and no `from`

    // second transition supersedes the first on shared properties
    uint64_t e = fm_animation_create(s, 1, "demo.exit"); fm_animation_play(s, e); RunFrames(s, 1000);
    CHECK(c.last.at("opacity") == 0.0 && c.last.at("offsetX") == 80.0);

    // Z6 F1 A-layer: enter transition interrupted mid-flight by a second holder on the same (target, offsetX)
    // ("hover"). Manual clock only; nothing is timed. Later writer wins; the hand-off keeps value and velocity.
    {
        Ctx c3; fm_system* s3 = fm_create_manual(Sink, Render, &c3);
        CHECK(s3 != nullptr && fm_load_tokens(s3, json.c_str(), err, sizeof err) == 0);
        const uint64_t T = 11;
        const uint64_t enter = fm_animation_create(s3, T, "demo.enter");   // holder 1: token transition
        const uint64_t hover = fm_animation_create(s3, T, nullptr);        // holder 2: retarget-only
        CHECK(enter != 0 && hover != 0 && enter != hover);
        fm_animation_play(s3, enter);
        for (int i = 0; i < 6; ++i) { fm_manual_advance(s3, 1.0 / 120.0); fm_on_frame(s3); }
        double ex = 0, evx = 0, eo = 0, evo = 0;
        CHECK(fm_current(s3, T, "offsetX", &ex, &evx) == 1 && fm_current(s3, T, "opacity", &eo, &evo) == 1);
        CHECK(ex > -80.0 && ex < 0.0 && evx > 0.0 && eo > 0.0 && eo < 1.0 && fm_is_running(s3));   // enter is mid-flight
        std::printf("  F1A_HOLDER1 enter=%llu target=%llu property=offsetX in_flight value=%.9g vel=%.9g opacity=%.9g\n",
                    (unsigned long long)enter, (unsigned long long)T, ex, evx, eo);
        fm_animation_play(s3, hover);
        CHECK(fm_animation_retarget(s3, hover, "offsetX", 40.0, 0.32, 0.78, 0, 0.0) == 1);   // later writer, no `from`
        double hx = 0, hvx = 0; CHECK(fm_current(s3, T, "offsetX", &hx, &hvx) == 1);
        CHECK(std::fabs(hx - ex) < 1e-9 && std::fabs(hvx - evx) < 1e-9);                     // Retarget starts at the live value
        std::printf("  F1A_RETARGET_FROM hover=%llu target=%llu property=offsetX from=%.9g vel=%.9g to=40\n",
                    (unsigned long long)hover, (unsigned long long)T, hx, hvx);
        int overlap = 0; double prevO = eo, prevX = hx;                                       // both holders write each frame
        for (int i = 0; i < 6; ++i) {
            fm_manual_advance(s3, 1.0 / 120.0); fm_on_frame(s3);
            const double o = c3.last.at("opacity"), x = c3.last.at("offsetX");
            if (o > prevO && x != prevX) ++overlap;
            prevO = o; prevX = x;
        }
        CHECK(overlap == 6);
        std::printf("  F1A_OVERLAP frames=6 enter_opacity_advancing_and_hover_offsetX_moving=%d\n", overlap);
        RunFrames(s3, 1000);
        double fx = 0; CHECK(fm_current(s3, T, "offsetX", &fx, nullptr) == 1 && fx == 40.0);
        CHECK(!fm_is_running(s3) && c3.last.at("offsetX") == 40.0 && c3.last.at("opacity") == 1.0);
        std::printf("  F1A_FINAL offsetX=%.9g opacity=%.9g expected_offsetX=40 (later writer target)\n",
                    c3.last.at("offsetX"), c3.last.at("opacity"));
        fm_animation_release(s3, enter); fm_animation_release(s3, hover); fm_destroy(s3);
    }

    // reduced motion: toggling mid-flight needs the pump woken (the shim does it); jump lands on the next frame
    fm_animation_play(s, a);                                   // demo.enter again, mid-flight below
    for (int i = 0; i < 6; ++i) { fm_manual_advance(s, 1.0 / 120.0); fm_on_frame(s); }
    CHECK(c.last.at("offsetX") < 0.0 && c.last.at("opacity") < 1.0);
    fm_set_reduced_motion(s, 1); CHECK(fm_is_running(s));
    RunFrames(s, 10); CHECK(c.last.at("opacity") == 1.0 && c.last.at("offsetX") == 0.0 && !fm_is_running(s));

    // cancel, release, and destroying with animations still running (ASan checks for leaks)
    fm_animation_cancel(s, a); fm_animation_release(s, a); fm_animation_release(s, 424242);
    fm_animation_play(s, fm_animation_create(s, 7, "demo.enter"));
    fm_destroy(s);
    fm_destroy(nullptr);

    // Z6 F3: empty animation + Play + Retarget at the C layer (mirrors the Z5-T3 devsmoke case). Fresh system and target.
    {
        Ctx c2; fm_system* s2 = fm_create_manual(Sink, Render, &c2);
        CHECK(s2 != nullptr && fm_load_tokens(s2, json.c_str(), err, sizeof err) == 0);
        uint64_t z = fm_animation_create(s2, 6, nullptr);            // empty animation (Retarget only)
        CHECK(z != 0);
        fm_animation_play(s2, z);                                    // Play on an empty animation: wakes the pump, nothing to drive
        RunFrames(s2, 10);
        CHECK(!fm_is_running(s2));                                   // ...so it goes idle again
        CHECK(fm_animation_retarget(s2, z, "offsetX", 120.0, 0.32, 0.78, 0, 0.0) == 0);   // never-sampled property, no `from`: rejected
        CHECK(!fm_is_running(s2));                                   // a rejected Retarget does not wake the pump
        CHECK(fm_animation_retarget(s2, z, "offsetX", 120.0, 0.32, 0.78, 1, 0.0) == 1);   // from: 0 -> 120
        CHECK(fm_is_running(s2) && c2.running);
        CHECK(fm_active_count(s2) > 0);   // MF-06: retargeted handle counts as active
        double v0 = -1, vel0 = -1; CHECK(fm_current(s2, 6, "offsetX", &v0, &vel0) == 1 && v0 == 0.0);
        const int f2 = RunFrames(s2, 1000);
        CHECK(!fm_is_running(s2) && !c2.running && f2 > 0 && f2 < 1000);
        CHECK(c2.last.at("offsetX") == 120.0);                       // lands exactly on the target
        CHECK(fm_animation_retarget(s2, z, "offsetX", 0.0, 0.32, 0.78, 0, 0.0) == 1);     // now sampled: `from` may be omitted
        for (int i = 0; i < 3; ++i) { fm_manual_advance(s2, 1.0 / 120.0); fm_on_frame(s2); }
        CHECK(fm_is_running(s2));
        fm_destroy(s2);                                              // destroy with the pump on and the handle still held
    }

    std::printf("%d checks, %d failures\n", g_checks, g_fail);
    return g_fail ? 1 : 0;
}
