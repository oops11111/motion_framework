// MotionNative/continuity_test.cpp: interruption continuity via the C ABI only (fm_capi.h).
// Spring retargeted mid-flight: value must not jump, velocity must carry over.
#include "fm_capi.h"
#include <cmath>
#include <cstdio>
static int g_fail = 0, g_checks = 0;
#define CHECK(c) do { ++g_checks; if (!(c)) { ++g_fail; std::printf("FAIL line %d: %s\n", __LINE__, #c); } } while (0)
static double g_sinkX = 0; static int g_sinkCalls = 0;
static void Sink(uint64_t, const char* p, double v, void*) { if (p[0]=='o' && p[1]=='f') { g_sinkX = v; ++g_sinkCalls; } }
static void Render(int, void*) {}
int main() {
    fm_system* s = fm_create_manual(Sink, Render, nullptr);
    CHECK(s != nullptr);
    uint64_t a = fm_animation_create(s, 1, nullptr);
    CHECK(a != 0);
    CHECK(fm_animation_retarget(s, a, "offsetX", 240.0, 0.32, 0.78, 1, 0.0) == 1);
    const double dt = 1.0 / 60.0;
    for (int i = 0; i < 9; ++i) { fm_manual_advance(s, dt); fm_on_frame(s); }   // 150 ms in flight
    double v0 = 0, vel0 = 0; CHECK(fm_current(s, 1, "offsetX", &v0, &vel0) == 1);
    double sinkBefore = g_sinkX;
    CHECK(fm_is_running(s) == 1);
    CHECK(fm_animation_retarget(s, a, "offsetX", 0.0, 0.32, 0.78, 0, 0.0) == 1);   // interrupt: back to 0
    double v1 = 0, vel1 = 0; CHECK(fm_current(s, 1, "offsetX", &v1, &vel1) == 1);
    // one more frame at a tiny dt to show the first rendered value after retarget is continuous
    fm_manual_advance(s, 1e-4); fm_on_frame(s);
    double v2 = 0, vel2 = 0; fm_current(s, 1, "offsetX", &v2, &vel2);
    // and a full frame
    fm_manual_advance(s, dt); fm_on_frame(s);
    double v3 = 0, vel3 = 0; fm_current(s, 1, "offsetX", &v3, &vel3);
    std::printf("before retarget: value=%.9f vel=%.9f (sink=%.9f)\n", v0, vel0, sinkBefore);
    std::printf("right after retarget(0): value=%.9f vel=%.9f  |dv|=%.3e |dvel|=%.3e\n", v1, vel1, std::fabs(v1 - v0), std::fabs(vel1 - vel0));
    std::printf("after +0.1ms frame: value=%.9f vel=%.9f  |dv|=%.3e\n", v2, vel2, std::fabs(v2 - v0));
    std::printf("after +16.7ms frame: value=%.9f vel=%.9f  step=%.6f (pre-retarget vel*dt=%.6f)\n", v3, vel3, v3 - v1, vel0 * dt);
    CHECK(vel0 > 100.0);                               // genuinely mid-flight, moving toward 240
    CHECK(std::fabs(v1 - v0) < 1e-9);                  // no jump at the retarget instant
    CHECK(std::fabs(vel1 - vel0) < 1e-6);              // velocity preserved
    CHECK(std::fabs((v2 - v0) - vel0 * 1e-4) < 1e-3);   // first 0.1ms after retarget = pre-retarget velocity * dt (no jump)
    CHECK(std::fabs(v3 - v1) <= std::fabs(vel0) * dt);  // one full frame moves less than one frame at pre-retarget speed
    int n = 0; while (fm_is_running(s) && n < 2000) { fm_manual_advance(s, dt); fm_on_frame(s); ++n; }
    double vf = 0; fm_current(s, 1, "offsetX", &vf, nullptr);
    std::printf("settled after %d more frames at value=%.9f (sink=%.9f, sink calls=%d)\n", n, vf, g_sinkX, g_sinkCalls);
    CHECK(!fm_is_running(s) && std::fabs(g_sinkX) < 1e-9);
    fm_destroy(s);
    std::printf("%d checks, %d failures\n", g_checks, g_fail);
    return g_fail ? 1 : 0;
}

