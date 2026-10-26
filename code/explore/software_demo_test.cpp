// Portable Stage A demo: spring + opacity fade through SoftwareDispatcher and FramePump,
// driven by a fixed-step clock at 120 Hz. Build:
//   g++ -std=c++17 -O1 -Wall -Wextra -Wpedantic software_demo_test.cpp -o demo && ./demo
#include "software_dispatcher.h"
#include <cstdio>
#include <vector>

using namespace motion;
static int g_fail = 0, g_checks = 0;
#define CHECK(c) do { ++g_checks; if (!(c)) { ++g_fail; std::printf("FAIL %s:%d  %s\n", __FILE__, __LINE__, #c); } } while (0)

struct FakeSink final : IPropertySink {
    std::map<std::string, double> last; int sets = 0;
    void Set(TargetId, const std::string& p, double v) override { last[p] = v; ++sets; }
};

int main() {
    FixedStepClock ck(1.0 / 120.0);
    FakeSink sink; SoftwareDispatcher disp(sink);
    MotionSystem sys(ck, &disp);
    bool subscribed = false; int subscribeCalls = 0;
    FramePump pump(sys, [&](bool on) { subscribed = on; ++subscribeCalls; });
    auto frame = [&] { ck.Tick(); pump.OnFrame(); };

    // one opacity fade (bezier) and one spring on offsetX
    auto fade = std::make_shared<const Timeline>(Animate(Track("opacity", 0).To(1, 0.25, Curve::Bezier(0.2, 0, 0, 1))));
    auto h = sys.Create(1, fade);
    h.Play();
    CHECK(h.Retarget("offsetX", 200.0, {0.3, 0.8}, 0.0));
    pump.Wake();
    CHECK(subscribed && pump.Running());

    int frames = 0;
    while (pump.Running() && frames < 600) { frame(); ++frames; }
    CHECK(!subscribed && !pump.Running());                      // pump stopped by itself
    CHECK(sink.last.at("opacity") == 1.0 && sink.last.at("offsetX") == 200.0);
    std::printf("  settled in %d frames (%.0f ms); %d sink writes\n", frames, frames * 1000.0 / 120.0, sink.sets);
    const int before = sink.sets; frame(); frame();
    CHECK(sink.sets == before);                                 // nothing written while idle

    // retarget again mid-rest: pump must be woken by the app
    h.Retarget("offsetX", 0.0, {0.3, 0.8});
    CHECK(!pump.Running());                                     // library does not wake the pump
    pump.Wake();
    for (int i = 0; i < 600 && pump.Running(); ++i) frame();
    CHECK(sink.last.at("offsetX") == 0.0 && !pump.Running());

    // reduced motion while animating: values only reach the sink after Wake + one frame
    h.Retarget("offsetX", 120.0, {0.3, 0.8}); pump.Wake();
    for (int i = 0; i < 10; ++i) frame();
    CHECK(pump.Running() && sink.last.at("offsetX") < 120.0);
    sys.SetReducedMotion(true);
    const double stale = sink.last.at("offsetX");
    CHECK(sink.last.at("offsetX") == stale);                    // SetReducedMotion alone wrote nothing
    pump.Wake(); frame();
    CHECK(sink.last.at("offsetX") == 120.0);                    // jumped to the target on the next frame

    std::printf("%d checks, %d failures\n", g_checks, g_fail);
    return g_fail ? 1 : 0;
}
