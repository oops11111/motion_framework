// Dependency-free tests for spring.h, bezier.h, timeline.h.
// Build: g++ -std=c++17 -O1 -Wall -Wextra timeline_tests.cpp -o tests && ./tests
#include "timeline.h"
#include <cstdio>
#include <vector>

using namespace motion;
static int g_fail = 0, g_checks = 0;
#define CHECK(c) do { ++g_checks; if (!(c)) { ++g_fail; std::printf("FAIL %s:%d  %s\n", __FILE__, __LINE__, #c); } } while (0)
#define NEAR(a, b, tol) do { ++g_checks; double _a = (a), _b = (b); \
  if (!(std::abs(_a - _b) <= (tol))) { ++g_fail; \
  std::printf("FAIL %s:%d  %s=%.12g vs %s=%.12g\n", __FILE__, __LINE__, #a, _a, #b, _b); } } while (0)

static std::shared_ptr<const Timeline> TL(NodePtr n) { return std::make_shared<Timeline>(std::move(n)); }
static NodePtr Lin(const char* p, double a, double b, double d) { return Animate(Track(p, a).To(b, d)); }

// ---- spring ----
static double Rk4(double z, double w0, double x0, double v0, double T) {
    double x = x0, v = v0; const double h = 1e-5; const int n = static_cast<int>(std::llround(T / h));
    auto f = [&](double xx, double vv, double& dx, double& dv) { dx = vv; dv = -w0 * w0 * xx - 2 * z * w0 * vv; };
    for (int i = 0; i < n; ++i) {
        double k1x, k1v, k2x, k2v, k3x, k3v, k4x, k4v;
        f(x, v, k1x, k1v); f(x + .5*h*k1x, v + .5*h*k1v, k2x, k2v);
        f(x + .5*h*k2x, v + .5*h*k2v, k3x, k3v); f(x + h*k3x, v + h*k3v, k4x, k4v);
        x += h/6*(k1x + 2*k2x + 2*k3x + k4x); v += h/6*(k1v + 2*k2v + 2*k3v + k4v);
    }
    return x;
}
static void TestSpring() {
    const double two_pi = 2 * 3.14159265358979323846;
    for (double z : {0.05, 0.3, 0.8, 0.999, 1.0, 1.001, 1.5, 4.0, 20.0})
        for (double v0 : {0.0, 300.0, -150.0}) {
            SpringParams p{0.4, z};
            Spring s(p, 100.0, 0.0, v0);
            NEAR(s.At(0.5).value, Rk4(z, two_pi / 0.4, 100.0, v0, 0.5), 1e-5);
        }
    // continuity of the closed form across the critical/under/over boundaries
    for (double zc : {1.0 - 2e-6, 1.0 - 1e-6 + 1e-9, 1.0, 1.0 + 1e-6, 1.0 + 2e-6}) {
        Spring s({0.35, zc}, 100.0, 0.0, 50.0), r({0.35, 1.0}, 100.0, 0.0, 50.0);
        NEAR(s.At(0.2).value, r.At(0.2).value, 1e-3);
    }
    // large damping: no cancellation, slow mode r1 ~ -w0/(2z)
    Spring big({0.35, 1e4}, 100.0, 0.0, 0.0);
    const double w0 = two_pi / 0.35;
    NEAR(big.At(100.0).value / (100.0 * std::exp(-w0 / (2e4) * 100.0)), 1.0, 1e-3);
    Spring huge({0.35, 1e8}, 100.0, 0.0, 0.0);
    CHECK(std::isfinite(huge.At(1.0).value) && huge.At(1.0).value > 99.0 && huge.At(1.0).value <= 100.0);
    // negative damping is clamped (no blow-up)
    CHECK(std::abs(Spring({0.35, -1.0}, 100.0, 0.0).At(50.0).value) <= 100.0 + 1e-9);  // clamped to undamped: bounded, never grows
    // retarget continuity
    Spring s({0.35, 0.5}, 0.0, 100.0, 0.0);
    SpringState a = s.At(0.2); s.Retarget(0.2, -40.0); SpringState b = s.At(0.0);
    NEAR(a.value, b.value, 1e-9); NEAR(a.velocity, b.velocity, 1e-9);
    // analytic bound vs scan oracle
    double worstOver = 0, worstUnder = 0;
    for (double r : {0.1, 0.2, 0.35, 0.6, 1.0})
        for (double z : {0.1, 0.3, 0.5, 0.8, 0.95, 1.0, 1.05, 1.5, 3.0, 10.0})
            for (double v0 : {0.0, 400.0, -400.0}) {
                Spring sp({r, z}, 100.0, 0.0, v0);
                const double an = sp.SettleTime(), sc = sp.SettleTimeScan();
                worstUnder = std::min(worstUnder, an - sc);
                worstOver = std::max(worstOver, an - sc);
                CHECK(an >= sc - 1.0 / 240.0 - 1e-9);            // never optimistic
                CHECK(an <= sc + 1.5 * r + 0.25 || an >= 10.0);   // never wildly pessimistic
            }
    std::printf("  settle bound: min(an-scan)=%.4f  max(an-scan)=%.4f s\n", worstUnder, worstOver);
    CHECK(Spring({0.35, 0.0}, 0.0, 1.0).SettleTime() == 10.0);   // undamped: cap
    CHECK(Spring({0.35, 0.8}, 5.0, 5.0, 0.0).SettleTime() == 0.0); // already at rest
}

// ---- tracks / nodes ----
static void TestTracks() {
    Track t = Track("x", 0).To(10, 1).To(20, 1, Curve::Bezier(0.4, 0, 0.2, 1)).Hold(0.5);
    NEAR(t.Duration(), 2.5, 1e-12);
    NEAR(t.Eval(0.5).value, 5, 1e-12);  NEAR(t.Eval(0.5).velocity, 10, 1e-12);
    NEAR(t.Eval(1.0).value, 10, 1e-12);
    NEAR(t.Eval(2.2).value, 20, 1e-12); NEAR(t.Eval(2.2).velocity, 0, 1e-12);
    NEAR(t.Eval(-1).value, 0, 0);  NEAR(t.Eval(99).value, 20, 0);  NEAR(t.Eval(99).velocity, 0, 0);
    NEAR(t.Eval(std::nan("")).value, 0, 0);
    // bezier velocity matches finite difference
    const double h = 1e-6;
    NEAR(t.Eval(1.4).velocity, (t.Eval(1.4 + h).value - t.Eval(1.4 - h).value) / (2 * h), 1e-4);
    // zero-duration jump
    Track j = Track("y", 0).To(5, 0).To(9, 1);
    NEAR(j.Eval(0).value, 5, 1e-12); NEAR(j.Eval(-0.1).value, 0, 0);
    // spring segment
    SpringParams sp{0.35, 0.8};
    Track s = Track("x", 0).SpringTo(100, sp);
    NEAR(s.Duration(), Spring(sp, 0, 100).SettleTime(kSpringPosEps, kSpringVelEps), 1e-12);
    // span-relative thresholds: a unitless spring is not cut short, and the end snap stays tiny
    {
        Track u = Track("scale", 0.96).SpringTo(1.0, sp);
        CHECK(u.Duration() > 0.3);
        NEAR(Spring(sp, 0.96, 1.0).At(u.Duration()).value, 1.0, 2e-4);
        NEAR(Track("x", 0).SpringTo(100, sp).Duration(), Spring(sp, 0, 100).SettleTime(0.1, 0.5), 1e-12);   // large spans unchanged
    }
    NEAR(s.Eval(0.1).value, Spring(sp, 0, 100).At(0.1).value, 1e-12);
    NEAR(s.Eval(s.Duration()).value, 100, 0);
    // inheritVelocity continues from the bezier exit velocity
    Track c = Track("x", 0).To(100, 1, Curve::Bezier(0.2, 0.0, 0.4, 1.0)).SpringTo(0, sp, true);
    NEAR(c.Eval(1.0).velocity, c.Segments()[0].to == 100 ? Track("x", 0).To(100, 1, Curve::Bezier(0.2, 0.0, 0.4, 1.0)).EndVelocity() : 0, 1e-9);
    CHECK(Curve::FromToken("standard").has_value());
    CHECK(!Curve::FromToken("nope").has_value());
}

static void TestComposition() {
    auto seq = Sequence({Lin("x", 0, 10, 1), Lin("x", 50, 60, 1), Lin("y", 7, 8, 1)});
    auto tl = TL(seq);
    NEAR(tl->Duration(), 3, 1e-12);
    NEAR(tl->Evaluate(0.5).at("x").value, 5, 1e-12);        // B backfills only when absent
    NEAR(tl->Evaluate(1.5).at("x").value, 55, 1e-12);       // B overwrites once started
    NEAR(tl->Evaluate(0.0).at("y").value, 7, 1e-12);        // y not started: initial value
    NEAR(tl->Evaluate(2.5).at("x").value, 60, 1e-12);       // holds end value
    auto par = TL(Parallel({Lin("a", 0, 1, 1), Lin("b", 0, 1, 2)}));
    NEAR(par->Duration(), 2, 1e-12);
    NEAR(par->Evaluate(1.0).at("a").value, 1, 1e-12); NEAR(par->Evaluate(1.0).at("b").value, 0.5, 1e-12);
    auto stg = TL(Stagger(0.1, {Lin("a", 0, 1, 1), Lin("b", 0, 1, 1), Lin("c", 0, 1, 1)}));
    NEAR(stg->Duration(), 1.2, 1e-12);
    NEAR(stg->Evaluate(0.1).at("b").value, 0.0, 1e-12); NEAR(stg->Evaluate(0.6).at("b").value, 0.5, 1e-12);
    auto del = TL(Sequence({Delay(0.5), Lin("x", 0, 1, 1)}));
    NEAR(del->Duration(), 1.5, 1e-12); NEAR(del->Evaluate(0.25).at("x").value, 0, 1e-12);
    // spring duration flows into composition
    auto spr = TL(Sequence({Animate(Track("x", 0).SpringTo(1, {0.3, 0.7})), Marker("end")}));
    NEAR(spr->MarkerTime("end").value(), spr->Duration(), 1e-12);
    // purity: repeated and out-of-order evaluation agree
    auto a1 = tl->Evaluate(1.3).at("x").value; tl->Evaluate(0.2); tl->Evaluate(2.9);
    NEAR(tl->Evaluate(1.3).at("x").value, a1, 0);
    // flattening for lowering
    CHECK(tl->Tracks().size() == 3); NEAR(tl->Tracks()[1].start, 1.0, 1e-12);
    // clamping
    NEAR(tl->Evaluate(-5).at("x").value, 0, 0); NEAR(tl->Evaluate(1e9).at("x").value, 60, 0);
    // exact end values: Evaluate(Duration()) is exactly final even when a spring ends a sequence
    {
        auto e = TL(Sequence({Lin("x", 0, 10, 1.0), Lin("x", 50, 60, 0.7), Animate(Track("s", 1).To(0.5, 0.1).SpringTo(1, {0.25, 0.7}, true))}));
        SampleMap m = e->Evaluate(e->Duration());
        NEAR(m.at("s").value, 1.0, 0); NEAR(m.at("s").velocity, 0.0, 0); NEAR(m.at("x").value, 60.0, 0);
    }
    // marker ordering
    auto mk = TL(Sequence({Marker("a"), Delay(1), Marker("b"), Delay(1), Marker("c")}));
    CHECK(mk->Markers().size() == 3 && mk->Markers()[0].name == "a" && mk->Markers()[2].name == "c");
}

// ---- playback ----
static void TestPlayer() {
    {   // determinism: stepping vs seek, and seek idempotence
        FixedStepClock ck(1.0 / 120.0);
        auto tl = TL(Parallel({Lin("x", 0, 100, 1.0), Animate(Track("s", 0).SpringTo(50, {0.3, 0.6}))}));
        Player p(tl, ck); p.Play();
        for (int i = 0; i < 92; ++i) { ck.Tick(); p.Update(); }
        NEAR(p.Position(), 92.0 / 120.0, 1e-12);
        SampleMap stepped = p.Sample();
        Player q(tl, ck); q.Seek(92.0 / 120.0);
        SampleMap sought = q.Sample();
        NEAR(stepped.at("x").value, sought.at("x").value, 1e-9);
        NEAR(stepped.at("s").value, sought.at("s").value, 1e-9);
        q.Seek(92.0 / 120.0);
        NEAR(q.Sample().at("s").value, sought.at("s").value, 0);
        // refresh-rate independence: 60 Hz vs 240 Hz arrive at the same state
        FixedStepClock c60(1.0 / 60.0), c240(1.0 / 240.0);
        Player a(tl, c60), b(tl, c240); a.Play(); b.Play();
        for (int i = 0; i < 30; ++i) { c60.Tick(); a.Update(); }
        for (int i = 0; i < 120; ++i) { c240.Tick(); b.Update(); }
        NEAR(a.Sample().at("s").value, b.Sample().at("s").value, 1e-9);
    }
    {   // markers + completion exactly once; Seek fires nothing
        ManualClock ck; int marks = 0, done = 0; std::vector<std::string> order;
        auto tl = TL(Sequence({Lin("x", 0, 1, 1), Marker("mid"), Lin("x", 1, 2, 1), Marker("end")}));
        Player p(tl, ck);
        p.OnMarker([&](const std::string& n, double) { ++marks; order.push_back(n); });
        p.OnComplete([&] { ++done; });
        p.Seek(1.5); CHECK(marks == 0);
        p.Seek(0); p.Play();
        for (int i = 0; i < 300; ++i) { ck.Advance(0.01); p.Update(); }
        CHECK(marks == 2 && done == 1 && order[0] == "mid" && order[1] == "end");
        CHECK(p.IsFinished() && !p.IsPlaying());
        NEAR(p.Position(), 2.0, 0);
        p.Play(); ck.Advance(0.5); p.Update();            // replay restarts
        NEAR(p.Position(), 0.5, 1e-12); CHECK(done == 1);
    }
    {   // loops
        ManualClock ck; int marks = 0, done = 0;
        auto tl = TL(Sequence({Delay(0.5), Marker("m"), Delay(0.5)}));
        Player p(tl, ck); p.SetLoops(3);
        p.OnMarker([&](const std::string&, double) { ++marks; }); p.OnComplete([&] { ++done; });
        p.Play(); ck.Advance(10.0); p.Update();            // one huge step crosses all loops
        CHECK(marks == 3 && done == 1 && p.IsFinished());
        ManualClock c2; int m2 = 0; Player inf(tl, c2); inf.SetLoops(-1);
        inf.OnMarker([&](const std::string&, double) { ++m2; }); inf.Play();
        c2.Advance(10.25); inf.Update();
        CHECK(m2 == 10 && inf.IsPlaying());
        NEAR(inf.Position(), 0.25, 1e-9);
    }
    {   // reverse, rate, pause
        ManualClock ck; auto tl = TL(Lin("x", 0, 10, 1.0)); Player p(tl, ck);
        p.Play(); ck.Advance(0.6); p.Update(); NEAR(p.Position(), 0.6, 1e-12);
        p.Reverse(); ck.Advance(0.3); p.Update(); NEAR(p.Position(), 0.3, 1e-12);
        p.Pause(); ck.Advance(5); p.Update(); NEAR(p.Position(), 0.3, 1e-12);
        p.Play(); p.SetRate(2.0); ck.Advance(0.1); p.Update(); NEAR(p.Position(), 0.1, 1e-12);  // reversed x2
        ManualClock c2; Player r(tl, c2); r.SetRate(2.0); r.Play(); c2.Advance(0.25); r.Update();
        NEAR(r.Position(), 0.5, 1e-12);
        // reverse of a finished animation plays backward from the end
        ManualClock c3; int marks = 0; auto mt = TL(Sequence({Delay(0.5), Marker("m"), Delay(0.5)}));
        Player f(mt, c3); f.OnMarker([&](const std::string&, double) { ++marks; });
        f.Play(); c3.Advance(2); f.Update(); CHECK(f.IsFinished() && marks == 1);
        f.Reverse(); c3.Advance(0.25); f.Update();
        NEAR(f.Position(), 0.75, 1e-12); c3.Advance(1); f.Update();
        CHECK(f.IsFinished() && marks == 2 && f.Position() == 0.0);
    }
    {   // zero-length timeline
        ManualClock ck; int marks = 0, done = 0; Player p(TL(Marker("z")), ck);
        p.OnMarker([&](const std::string&, double) { ++marks; }); p.OnComplete([&] { ++done; });
        p.Play(); ck.Advance(0.01); p.Update(); CHECK(marks == 1 && done == 1);
    }
}

static void TestRetarget() {
    {   // bezier segment, rate 2: velocity = Slope-based, carried exactly into the spring
        ManualClock ck;
        auto tl = TL(Animate(Track("x", 0).To(100, 1.0, Curve::Bezier(0.4, 0, 0.2, 1))));
        Player p(tl, ck); p.SetRate(2.0); p.Play(); ck.Advance(0.15); p.Update();
        Sample before = p.Sample().at("x");
        const double expectV = Track("x", 0).To(100, 1.0, Curve::Bezier(0.4, 0, 0.2, 1)).Eval(0.3).velocity * 2.0;
        NEAR(before.velocity, expectV, 1e-9);
        CHECK(p.Retarget("x", 0.0, {0.3, 0.7}));
        Sample after = p.Sample().at("x");
        NEAR(after.value, before.value, 1e-9); NEAR(after.velocity, before.velocity, 1e-9);
        ck.Advance(2.0); p.Update(); NEAR(p.Sample().at("x").value, 0.0, kSpringPosEps);
        p.ReleaseOverride("x");   // back to the graph
        NEAR(p.Sample().at("x").value, tl->Evaluate(p.Position()).at("x").value, 0);
    }
    {   // spring segment: state velocity carried; retarget of a retarget also continuous
        ManualClock ck; auto tl = TL(Animate(Track("x", 0).SpringTo(100, {0.35, 0.5})));
        Player p(tl, ck); p.Play(); ck.Advance(0.12); p.Update();
        Sample s0 = p.Sample().at("x");
        p.Retarget("x", 40, {0.35, 0.8});
        NEAR(p.Sample().at("x").velocity, s0.velocity, 1e-9);
        ck.Advance(0.07); p.Update(); Sample s1 = p.Sample().at("x");
        p.Retarget("x", 90, {0.2, 1.0});
        NEAR(p.Sample().at("x").value, s1.value, 1e-9); NEAR(p.Sample().at("x").velocity, s1.velocity, 1e-9);
    }
    {   // unknown property
        ManualClock ck; Player p(TL(Lin("x", 0, 1, 1)), ck);
        CHECK(!p.Retarget("nope", 1, {0.3, 0.8}));
        CHECK(p.Retarget("nope", 1, {0.3, 0.8}, 0.25));
        NEAR(p.Sample().at("nope").value, 0.25, 1e-12);
    }
    {   // paused: velocity is zero, so a retarget starts from rest
        ManualClock ck; Player p(TL(Lin("x", 0, 10, 1)), ck); p.Play(); ck.Advance(0.4); p.Update(); p.Pause();
        p.Retarget("x", 0, {0.3, 0.8}); NEAR(p.Sample().at("x").velocity, 0, 0);
    }
}

int main() {
    FixedStepClock fc(1.0 / 120.0); fc.Tick(1200); NEAR(fc.Now(), 10.0, 1e-9);
    ManualClock mc; mc.Advance(-1); NEAR(mc.Now(), 0, 0);
    TestSpring(); TestTracks(); TestComposition(); TestPlayer(); TestRetarget();
    std::printf("%d checks, %d failures\n", g_checks, g_fail);
    return g_fail ? 1 : 0;
}
