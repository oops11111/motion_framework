// Tests for controller.h.
// Build: g++ -std=c++17 -O1 -Wall -Wextra -Wpedantic controller_tests.cpp -o ct && ./ct
#include "controller.h"
#include <cstdio>

using namespace motion;
static int g_fail = 0, g_checks = 0;
#define CHECK(c) do { ++g_checks; if (!(c)) { ++g_fail; std::printf("FAIL %s:%d  %s\n", __FILE__, __LINE__, #c); } } while (0)
#define NEAR(a, b, tol) do { ++g_checks; double _a = (a), _b = (b); \
  if (!(std::abs(_a - _b) <= (tol))) { ++g_fail; \
  std::printf("FAIL %s:%d  %s=%.12g vs %s=%.12g\n", __FILE__, __LINE__, #a, _a, #b, _b); } } while (0)

struct Recorder final : IDispatcher {
    std::vector<ControllerSnapshot> submits;
    std::vector<std::pair<ControllerId, EndReason>> withdraws;
    std::vector<std::pair<TargetId, SampleMap>> applies;
    void Submit(const ControllerSnapshot& s) override { submits.push_back(s); }
    void Withdraw(ControllerId id, EndReason r) override { withdraws.push_back({id, r}); }
    void Apply(TargetId t, const SampleMap& m) override { applies.push_back({t, m}); }
};

static std::shared_ptr<const Timeline> TL(NodePtr n) { return std::make_shared<const Timeline>(std::move(n)); }
static auto LinX(double to = 100, double d = 1.0, const char* p = "x") { return TL(Animate(Track(p, 0).To(to, d))); }
static const Curve kEase = Curve::Bezier(0.4, 0, 0.2, 1);
static void Step(ManualClock& c, MotionSystem& s, double dt) { c.Advance(dt); s.Tick(); }

static void TestRetargetFromBezier() {
    ManualClock ck; MotionSystem sys(ck); int ended = 0; EndReason last = EndReason::Cancelled;
    auto h = sys.Create(1, TL(Animate(Track("x", 0).To(100, 1.0, kEase))));
    h.OnEnd([&](EndReason r) { ++ended; last = r; });
    h.Play(); Step(ck, sys, 0.3);
    Sample before = *h.Value("x");
    const double slope = CubicBezier(0.4, 0, 0.2, 1).Slope(0.3) * 100.0;
    NEAR(before.velocity, slope, 1e-9);
    const SpringParams sp{0.3, 0.8};
    CHECK(h.Retarget("x", 0.0, sp));
    Sample after = *h.Value("x");
    NEAR(after.value, before.value, 1e-9); NEAR(after.velocity, before.velocity, 1e-9);
    Step(ck, sys, 0.1);
    NEAR(h.Value("x")->value, Spring(sp, before.value, 0.0, before.velocity).At(0.1).value, 1e-12);
    NEAR(h.Value("x")->velocity, Spring(sp, before.value, 0.0, before.velocity).At(0.1).velocity, 1e-9);
    Step(ck, sys, 5.0);
    NEAR(h.Value("x")->value, 0.0, 0); NEAR(h.Value("x")->velocity, 0.0, 0);
    CHECK(h.State() == AnimationState::Finished && ended == 1 && last == EndReason::Completed);
    CHECK(sys.ActiveCount() == 0 && !h.IsActive());
}

static void TestRetargetFromSpring() {
    ManualClock ck; MotionSystem sys(ck);
    auto h = sys.Create(1, TL(Animate(Track("x", 0).SpringTo(100, {0.35, 0.5}))));
    h.Play(); Step(ck, sys, 0.12);
    Sample s0 = *h.Value("x");
    NEAR(s0.velocity, Spring({0.35, 0.5}, 0, 100).At(0.12).velocity, 1e-9);
    CHECK(h.Retarget("x", 40, {0.35, 0.8}));
    NEAR(h.Value("x")->value, s0.value, 1e-9); NEAR(h.Value("x")->velocity, s0.velocity, 1e-9);
    Step(ck, sys, 0.07);
    Sample s1 = *h.Value("x");
    CHECK(h.Retarget("x", 90, {0.2, 1.0}));                    // retarget of a retarget
    NEAR(h.Value("x")->value, s1.value, 1e-9); NEAR(h.Value("x")->velocity, s1.velocity, 1e-9);
    // rapid-fire retargets (drag following): continuity every time
    for (int i = 0; i < 50; ++i) {
        Step(ck, sys, 1.0 / 120.0);
        Sample b = *h.Value("x");
        h.Retarget("x", 90 + 20 * std::sin(i * 0.3), {0.15, 0.9});
        NEAR(h.Value("x")->value, b.value, 1e-9); NEAR(h.Value("x")->velocity, b.velocity, 1e-9);
    }
}

static void TestRateAndReverse() {
    ManualClock ck; MotionSystem sys(ck);
    auto h = sys.Create(1, LinX());
    h.SetRate(2.0); h.Play(); Step(ck, sys, 0.1);
    NEAR(h.Value("x")->velocity, 200.0, 1e-9); NEAR(h.Value("x")->value, 20.0, 1e-9);
    h.Retarget("x", 0, {0.3, 0.8});
    NEAR(h.Value("x")->velocity, 200.0, 1e-9);
    // reverse flips velocity sign on an independent controller
    auto r = sys.Create(2, LinX());
    r.Play(); Step(ck, sys, 0.5); r.Reverse();
    NEAR(r.Value("x")->velocity, -100.0, 1e-9);
    Step(ck, sys, 0.25); NEAR(r.Position(), 0.25, 1e-12);
    // pause: velocity is zero, retarget starts from rest
    r.Pause(); CHECK(r.State() == AnimationState::Paused); NEAR(r.Value("x")->velocity, 0.0, 0);
    r.Retarget("x", 80, {0.3, 0.8}); NEAR(r.Value("x")->velocity, 0.0, 1e-9);
}

static void TestCancel() {
    ManualClock ck; Recorder rec; MotionSystem sys(ck, &rec);
    int ended = 0; EndReason why = EndReason::Completed;
    auto h = sys.Create(7, LinX());
    h.OnEnd([&](EndReason r) { ++ended; why = r; });
    h.Play(); Step(ck, sys, 0.4);
    h.Cancel(); h.Cancel();
    CHECK(ended == 1 && why == EndReason::Cancelled && h.State() == AnimationState::Cancelled);
    CHECK(!h.Owns("x") && !h.Value("x"));
    NEAR(sys.Current(7, "x")->value, 40.0, 1e-9);              // frozen where it was
    NEAR(sys.Current(7, "x")->velocity, 0.0, 0);
    Step(ck, sys, 1.0);
    NEAR(sys.Current(7, "x")->value, 40.0, 1e-9);
    h.Play(); h.Seek(0.9); h.SetRate(3);
    CHECK(!h.Retarget("x", 1, {0.3, 0.8}) && h.State() == AnimationState::Cancelled);
    CHECK(sys.ActiveCount() == 0);
    CHECK(rec.withdraws.size() == 1 && rec.withdraws[0].second == EndReason::Cancelled);
    // a later controller can pick the property up from the rest value
    auto h2 = sys.Create(7);
    CHECK(h2.Retarget("x", 0, {0.3, 0.8}));
    NEAR(h2.Value("x")->value, 40.0, 1e-9);
    // null handle is inert
    AnimationHandle nh; nh.Play(); nh.Cancel(); CHECK(!nh && !nh.Retarget("x", 1, {}) && nh.State() == AnimationState::Cancelled);
}

static void TestHandleLifetime() {
    ManualClock ck;
    {   // dropping every handle without Cancel: the animation still runs, then is freed
        MotionSystem sys(ck);
        { auto h = sys.Create(1, LinX()); h.Play(); }
        CHECK(sys.ActiveCount() == 1);
        Step(ck, sys, 0.5); NEAR(sys.Current(1, "x")->value, 50.0, 1e-9);
        Step(ck, sys, 1.0);
        CHECK(sys.ActiveCount() == 0);
        NEAR(sys.Current(1, "x")->value, 100.0, 0);            // rest value after the controller is gone
        auto again = sys.Create(1); CHECK(again.Retarget("x", 0, {0.3, 0.8}));  // seeds from rest
        NEAR(again.Value("x")->value, 100.0, 0);
    }
    {   // copies share one controller
        MotionSystem sys(ck);
        auto a = sys.Create(1, LinX()); auto b = a;
        CHECK(a.SameAs(b) && a.Id() == b.Id());
        a.Play(); b.Cancel(); CHECK(a.State() == AnimationState::Cancelled);
    }
    {   // handle-capturing callbacks do not leak after a terminal transition (checked by LeakSanitizer)
        MotionSystem sys(ck);
        auto h = sys.Create(1, LinX()); h.OnEnd([h](EndReason) { (void)h; }); h.Play(); h.Cancel();
        auto g = sys.Create(2, LinX()); g.OnMarker([g](const std::string&, double) { (void)g; }); g.Play(); g.Cancel();
    }
    {   // handle outliving the system is inert
        AnimationHandle h;
        { MotionSystem sys(ck); h = sys.Create(1, LinX()); h.Play(); }
        CHECK(h.State() == AnimationState::Cancelled);
        h.Play(); h.Seek(1); CHECK(!h.Retarget("x", 1, {}));
        h.Cancel();
    }
    {   // churn: many handles created, played, dropped, cancelled
        MotionSystem sys(ck);
        for (int i = 0; i < 2000; ++i) {
            auto h = sys.Create(static_cast<TargetId>(i % 17), LinX());
            h.Play();
            if (i % 3 == 0) h.Cancel();
            if (i % 50 == 0) Step(ck, sys, 0.3);
        }
        Step(ck, sys, 5.0); Step(ck, sys, 0.01);
        CHECK(sys.ActiveCount() == 0);
    }
}

static void TestOverlap() {
    ManualClock ck; MotionSystem sys(ck);
    int endedA = 0; EndReason whyA = EndReason::Completed;
    auto A = sys.Create(1, TL(Parallel({Animate(Track("x", 0).To(100, 1.0)), Animate(Track("y", 0).To(10, 1.0))})));
    A.OnEnd([&](EndReason r) { ++endedA; whyA = r; });
    A.Play(); Step(ck, sys, 0.2);
    auto B = sys.Create(1);
    CHECK(B.Retarget("x", 0, {0.3, 0.8}));
    NEAR(B.Value("x")->value, 20.0, 1e-9); NEAR(B.Value("x")->velocity, 100.0, 1e-9);   // value AND velocity handed over
    CHECK(!A.Owns("x") && A.Owns("y") && !A.Value("x") && A.State() == AnimationState::Playing && endedA == 0);
    SampleMap av = A.Values(); CHECK(av.size() == 1 && av.count("y") == 1);
    CHECK(sys.Current(1, "x")->value == B.Value("x")->value);
    Step(ck, sys, 0.1);
    NEAR(A.Value("y")->value, 3.0, 1e-9);                       // A kept animating y
    auto C = sys.Create(1);
    CHECK(C.Retarget("y", 0, {0.3, 0.8}));                      // A now owns nothing
    CHECK(A.State() == AnimationState::Superseded && endedA == 1 && whyA == EndReason::Superseded);
    Step(ck, sys, 3.0);
    NEAR(B.Value("x")->value, 0.0, 0); NEAR(C.Value("y")->value, 0.0, 0);
    // different targets do not interfere
    auto D = sys.Create(2, LinX()); D.Play(); auto E = sys.Create(3, LinX()); E.Play();
    CHECK(D.State() == AnimationState::Playing && E.State() == AnimationState::Playing);
    // Play() last-wins on the same (target, property)
    auto F = sys.Create(2, LinX()); int endedD = 0; D.OnEnd([&](EndReason) { ++endedD; });
    F.Play();
    CHECK(D.State() == AnimationState::Superseded && endedD == 1 && F.Owns("x"));
    // an idle/finished controller that loses its property stays reusable and can reclaim it
    ManualClock c2; MotionSystem s2(c2);
    auto G = s2.Create(1, LinX()); G.Play(); Step(c2, s2, 2.0); CHECK(G.State() == AnimationState::Finished);
    auto H = s2.Create(1); H.Retarget("x", 5, {0.3, 0.8});
    CHECK(G.State() == AnimationState::Finished && !G.Owns("x"));
    G.Play(); CHECK(G.Owns("x") && G.State() == AnimationState::Playing && H.State() != AnimationState::Cancelled);
}

static const char* kTok = R"({
 "version":1,
 "springs":{"snappy":{"response":"300ms","damping":0.8}},
 "transitions":{
  "card.enter":{
   "root":{"type":"parallel","children":[
     {"type":"track","property":"opacity","from":0,"steps":[{"to":1,"duration":"200ms"}]},
     {"type":"track","property":"scale","from":0.96,"steps":[{"springTo":1,"spring":"snappy"}]}]},
   "reduced":{"type":"track","property":"opacity","from":0,"steps":[{"to":1,"duration":"100ms"}]}},
  "card.exit":{
   "root":{"type":"track","property":"x","from":0,"steps":[{"to":10,"duration":"1s"}]},
   "reduced":"instant"}
 }})";

static void TestReducedMotion() {
    std::vector<Diagnostic> errs; auto tok = TokenSet::Parse(kTok, errs); CHECK(tok.has_value());
    if (!tok) return;
    {   // reduced timeline from tokens is honored: fade only, no spring
        ManualClock ck; MotionSystem sys(ck); sys.SetReducedMotion(true);
        auto h = sys.Create(1, *tok, "card.enter"); CHECK(h);
        h.Play();
        CHECK(h.Owns("opacity") && !h.Owns("scale") && !h.Value("scale"));
        Step(ck, sys, 0.05); NEAR(h.Value("opacity")->value, 0.5, 1e-12);
        Step(ck, sys, 0.1); NEAR(h.Value("opacity")->value, 1.0, 0); CHECK(h.State() == AnimationState::Finished);
    }
    {   // "instant": completes at Play, end state immediately, completion fired once
        ManualClock ck; Recorder rec; MotionSystem sys(ck, &rec); sys.SetReducedMotion(true);
        auto h = sys.Create(1, *tok, "card.exit"); int ended = 0; h.OnEnd([&](EndReason r) { ended += r == EndReason::Completed; });
        h.Play();
        CHECK(h.State() == AnimationState::Finished && ended == 1);
        NEAR(h.Value("x")->value, 10.0, 0); NEAR(h.Value("x")->velocity, 0.0, 0);
        sys.Tick(); CHECK(!rec.applies.empty() && rec.applies.back().second.at("x").value == 10.0);
        CHECK(sys.ActiveCount() == 0);
    }
    {   // no reduced timeline supplied: derived instant (springs jump to their end value)
        ManualClock ck; MotionSystem sys(ck); sys.SetReducedMotion(true);
        auto h = sys.Create(1, TL(Animate(Track("x", 0).To(40, 1.0).SpringTo(100, {0.35, 0.5}))));
        h.Play(); CHECK(h.State() == AnimationState::Finished); NEAR(h.Value("x")->value, 100.0, 0);
    }
    {   // Retarget jumps, with no spring and zero velocity
        ManualClock ck; MotionSystem sys(ck); sys.SetReducedMotion(true);
        auto h = sys.Create(1);
        CHECK(h.Retarget("x", 5.0, {0.3, 0.3}));                // no seed value needed
        NEAR(h.Value("x")->value, 5.0, 0); NEAR(h.Value("x")->velocity, 0.0, 0);
        sys.Tick(); CHECK(sys.ActiveCount() == 0);
        Step(ck, sys, 0.05); NEAR(h.Value("x")->value, 5.0, 0);
    }
    {   // switching reduced motion ON mid-flight snaps everything to its end state
        ManualClock ck; MotionSystem sys(ck); int ended = 0;
        auto h = sys.Create(1, TL(Animate(Track("x", 0).SpringTo(100, {0.35, 0.5}))));
        h.OnEnd([&](EndReason r) { ended += r == EndReason::Completed; });
        h.Play(); h.Retarget("y", 50, {0.3, 0.8}, 0.0); Step(ck, sys, 0.1);
        CHECK(h.State() == AnimationState::Playing);
        sys.SetReducedMotion(true);
        CHECK(h.State() == AnimationState::Finished && ended == 1);
        NEAR(h.Value("x")->value, 100.0, 0); NEAR(h.Value("y")->value, 50.0, 0); NEAR(h.Value("y")->velocity, 0, 0);
        sys.Tick(); CHECK(sys.ActiveCount() == 0);
        sys.SetReducedMotion(false);                             // later plays use the full timeline again
        h.Play(); CHECK(h.State() == AnimationState::Playing);
    }
    {   // variant is re-chosen at the next Play when the mode changed while idle
        ManualClock ck; MotionSystem sys(ck);
        auto h = sys.Create(1, *tok, "card.enter");
        h.Play(); CHECK(h.Owns("scale")); Step(ck, sys, 2.0); CHECK(h.State() == AnimationState::Finished);
        sys.SetReducedMotion(true); h.Play();
        CHECK(h.Owns("opacity") && !h.Owns("scale") && !h.Value("scale"));
        CHECK(sys.Current(1, "scale").has_value());              // scale was given back with its rest value
    }
}

static void TestSeekAndRetarget() {
    ManualClock ck; MotionSystem sys(ck);
    auto h = sys.Create(1, LinX()); h.Play(); Step(ck, sys, 0.1);
    h.Seek(0.5);                                                // playing: continues from 0.5
    NEAR(h.Value("x")->value, 50.0, 1e-12); NEAR(h.Value("x")->velocity, 100.0, 1e-9);
    h.Seek(0.5); NEAR(h.Value("x")->value, 50.0, 1e-12);        // idempotent
    h.Retarget("x", 0, {0.3, 0.8});
    NEAR(h.Value("x")->value, 50.0, 1e-12); NEAR(h.Value("x")->velocity, 100.0, 1e-9);
    // paused seek: zero velocity
    auto p = sys.Create(2, LinX()); p.Play(); Step(ck, sys, 0.1); p.Pause(); p.Seek(0.25);
    NEAR(p.Value("x")->value, 25.0, 1e-12); NEAR(p.Value("x")->velocity, 0.0, 0);
    p.Retarget("x", 100, {0.3, 0.8});
    NEAR(p.Value("x")->value, 25.0, 1e-12); NEAR(p.Value("x")->velocity, 0.0, 1e-9);
    p.Seek(0.9);                                                // seek after retarget: the graph reclaims x
    NEAR(p.Value("x")->value, 90.0, 1e-12);
    // seek on an idle controller holds a pose; Play resumes from it
    auto q = sys.Create(3, LinX()); q.Seek(0.5);
    CHECK(q.State() == AnimationState::Paused); NEAR(q.Value("x")->value, 50.0, 1e-12);
    q.Play(); Step(ck, sys, 0.25); NEAR(q.Value("x")->value, 75.0, 1e-9);
    // seek clamps; retarget after seeking past the end
    q.Seek(99); NEAR(q.Value("x")->value, 100.0, 0);
    q.Retarget("x", 0, {0.3, 0.8}); NEAR(q.Value("x")->value, 100.0, 0); NEAR(q.Value("x")->velocity, 0.0, 1e-9);
    // Play() while playing continues, but drops a masking override: the graph shows again
    auto r = sys.Create(4, LinX()); r.Play(); Step(ck, sys, 0.5); r.Retarget("x", 0, {0.3, 0.8});
    Step(ck, sys, 0.1); CHECK(r.Value("x")->value < 50.0);      // override pulling x back toward 0
    r.Play(); NEAR(r.Value("x")->value, 60.0, 1e-9);            // graph value at position 0.6
    Step(ck, sys, 0.2); NEAR(r.Value("x")->value, 80.0, 1e-9);
    // Release gives a non-graph property back and keeps its value as the rest value
    auto s = sys.Create(5); s.Retarget("z", 8, {0.3, 0.8}, 0.0); Step(ck, sys, 3.0);
    s.Release("z"); CHECK(!s.Owns("z")); NEAR(sys.Current(5, "z")->value, 8.0, 0);
    // Retarget with no seed and nobody driving the property fails cleanly
    CHECK(!sys.Create(6).Retarget("nothing", 1, {0.3, 0.8}));
}

static void TestDispatcher() {
    ManualClock ck; Recorder rec; MotionSystem sys(ck, &rec);
    auto tl = LinX();
    auto h = sys.Create(9, tl);
    h.Play();
    CHECK(rec.submits.size() == 1);
    const auto& s0 = rec.submits.back();
    CHECK(s0.playing && s0.timeline == tl && s0.target == 9 && s0.id == h.Id() && s0.ownedProperties.size() == 1 && s0.overrides.empty());
    Step(ck, sys, 0.25);
    CHECK(rec.applies.size() == 1 && rec.applies[0].first == 9);
    NEAR(rec.applies[0].second.at("x").value, 25.0, 1e-12);
    h.SetRate(2.0); CHECK(rec.submits.back().rate == 2.0);
    h.Seek(0.1); NEAR(rec.submits.back().position, 0.1, 1e-12);
    h.Pause(); CHECK(!rec.submits.back().playing);
    sys.Tick(); CHECK(!rec.applies.empty());                    // paused + dirty: one more Apply for the seek pose
    const std::size_t n = rec.applies.size();
    Step(ck, sys, 1.0); CHECK(rec.applies.size() == n);         // nothing animating, nothing applied
    h.Play(); h.Retarget("x", 3.0, {0.3, 0.8});
    const auto& s1 = rec.submits.back();
    CHECK(s1.overrides.size() == 1 && s1.overrides[0].property == "x" && s1.overrides[0].target == 3.0);
    NEAR(s1.overrides[0].from, 10.0, 1e-9); NEAR(s1.overrides[0].fromVelocity, 200.0, 1e-9);  // resumed at rate 2: 100 * 2
    Step(ck, sys, 5.0);
    CHECK(sys.ActiveCount() == 0);
    CHECK(rec.withdraws.size() == 2 && rec.withdraws[0].second == EndReason::Completed && rec.withdraws[1].second == EndReason::Completed);  // once per activation period
    // Apply only carries owned properties
    ManualClock c2; Recorder r2; MotionSystem s2(c2, &r2);
    auto A = s2.Create(1, TL(Parallel({Animate(Track("x", 0).To(1, 1.0)), Animate(Track("y", 0).To(1, 1.0))})));
    A.Play(); auto B = s2.Create(1); B.Retarget("x", 0, {0.3, 0.8});
    Step(c2, s2, 0.1);
    CHECK(!r2.applies.empty() && r2.applies.back().second.count("x") == 1 && r2.applies.back().second.count("y") == 1);
    // A's snapshot after the loss no longer lists x
    bool aLostX = false;
    for (const auto& s : r2.submits) if (s.id == A.Id() && s.ownedProperties.size() == 1) aLostX = true;
    CHECK(aLostX);
}

static void TestMarkersAndReplay() {
    ManualClock ck; MotionSystem sys(ck); int marks = 0, ended = 0;
    auto h = sys.Create(1, TL(Sequence({Animate(Track("x", 0).To(1, 0.5)), Marker("m"), Animate(Track("x", 1).To(2, 0.5))})));
    h.OnMarker([&](const std::string&, double) { ++marks; }); h.OnEnd([&](EndReason) { ++ended; });
    h.Play(); Step(ck, sys, 0.7); CHECK(marks == 1 && ended == 0);
    Step(ck, sys, 0.5); CHECK(ended == 1 && h.State() == AnimationState::Finished);
    h.Play(); CHECK(h.State() == AnimationState::Playing && h.Position() == 0.0);
    Step(ck, sys, 2.0); CHECK(marks == 2 && ended == 2);
    h.Reverse(); Step(ck, sys, 0.25); NEAR(h.Position(), 0.75, 1e-12);
    // a controller whose callback cancels another does not break the tick loop
    auto a = sys.Create(10, LinX()), b = sys.Create(11, LinX());
    a.OnEnd([&](EndReason r) { if (r == EndReason::Completed) b.Cancel(); });
    a.Play(); b.Play(); Step(ck, sys, 2.0);
    CHECK(b.State() == AnimationState::Cancelled || b.State() == AnimationState::Finished);
}

int main() {
    TestRetargetFromBezier(); TestRetargetFromSpring(); TestRateAndReverse(); TestCancel();
    TestHandleLifetime(); TestOverlap(); TestReducedMotion(); TestSeekAndRetarget();
    TestDispatcher(); TestMarkersAndReplay();
    std::printf("%d checks, %d failures\n", g_checks, g_fail);
    return g_fail ? 1 : 0;
}
