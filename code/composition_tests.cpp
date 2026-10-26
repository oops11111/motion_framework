// Tests for composition.h. Build:
//   g++ -std=c++17 -O1 -Wall -Wextra -Wpedantic composition_tests.cpp -o comp && ./comp
#include "composition.h"
#include <cstdio>

using namespace motion;
static int g_fail = 0, g_checks = 0;
#define CHECK(c) do { ++g_checks; if (!(c)) { ++g_fail; std::printf("FAIL %s:%d  %s\n", __FILE__, __LINE__, #c); } } while (0)
#define NEAR(a, b, tol) do { ++g_checks; double _a = (a), _b = (b); \
  if (!(std::abs(_a - _b) <= (tol))) { ++g_fail; \
  std::printf("FAIL %s:%d  %s=%.12g vs %s=%.12g\n", __FILE__, __LINE__, #a, _a, #b, _b); } } while (0)
static bool Has(const std::string& s, const std::string& sub) { return s.find(sub) != std::string::npos; }

static std::shared_ptr<const Timeline> TL(NodePtr n) { return std::make_shared<const Timeline>(std::move(n)); }
static const Curve kEase = Curve::Bezier(0.4, 0, 0.2, 1);
static const SpringParams kSp{0.3, 0.8};

// Simulates the compositor under the assumptions documented in composition.h.
struct SimBackend final : ICompositorBackend {
    using Key = std::pair<TargetId, std::string>;
    struct Anim { bool spring = false; KeyframeAnimation kf; SpringAnimation sp; double start = 0, before = 0; };
    const ManualClock* clock;
    std::map<Key, Anim> anims;
    std::map<Key, double> values;
    std::vector<std::string> log;
    int startKf = 0, startSp = 0, stops = 0, sets = 0;
    explicit SimBackend(const ManualClock* c) : clock(c) {}

    double Value(TargetId t, const std::string& k) const {
        auto a = anims.find({t, k});
        if (a != anims.end()) {
            const double el = clock->Now() - a->second.start;
            return a->second.spring ? SimulateSpring(a->second.sp, el, a->second.before)
                                    : SimulateKeyframes(a->second.kf, el, a->second.before);
        }
        auto v = values.find({t, k});
        return v == values.end() ? std::nan("") : v->second;
    }
    void Begin(TargetId t, const std::string& path, Anim a, std::optional<double> preset) {
        double cur = Value(t, path);
        if (preset) cur = *preset;
        a.before = cur; a.start = clock->Now();
        anims[{t, path}] = a;
    }
    void StartKeyframes(TargetId t, const std::string& path, const KeyframeAnimation& k) override {
        ++startKf; log.push_back("start-kf " + path);
        Anim a; a.kf = k; Begin(t, path, a, k.presetValue);
    }
    void StartSpring(TargetId t, const std::string& path, const SpringAnimation& s) override {
        ++startSp; log.push_back("start-spring " + path);
        Anim a; a.spring = true; a.sp = s; Begin(t, path, a, s.presetValue);
    }
    void Stop(TargetId t, const std::string& path) override {
        ++stops; log.push_back("stop " + path);
        values[{t, path}] = Value(t, path); anims.erase({t, path});
    }
    void SetValue(TargetId t, const std::string& key, double v) override {
        ++sets; log.push_back("set " + key);
        anims.erase({t, key}); values[{t, key}] = v;
    }
    int Count(const std::string& entry) const { int n = 0; for (auto& l : log) n += l == entry; return n; }
    int IndexOf(const std::string& entry, int from = 0) const {
        for (std::size_t i = static_cast<std::size_t>(from); i < log.size(); ++i) if (log[i] == entry) return static_cast<int>(i);
        return -1;
    }
};

struct Rig {
    ManualClock ck;
    SimBackend be{&ck};
    StaticCapabilityProbe probe;
    CompositionDispatcher disp{probe, be};
    MotionSystem sys{ck, &disp};
    void Step(double dt) { ck.Advance(dt); sys.Tick(); }
};

static ControllerSnapshot Snap(const std::shared_ptr<const Timeline>& tl, double pos = 0, bool playing = true,
                               double rate = 1, bool rev = false, int loops = 1) {
    ControllerSnapshot s; s.id = 1; s.target = 1; s.timeline = tl; s.position = pos; s.playing = playing;
    s.rate = rate; s.reversed = rev; s.loops = loops;
    for (const auto& pt : tl->Tracks()) {
        const auto& p = pt.track->Property();
        if (std::find(s.ownedProperties.begin(), s.ownedProperties.end(), p) == s.ownedProperties.end()) s.ownedProperties.push_back(p);
    }
    return s;
}

static void TestPurePlans() {
    StaticCapabilityProbe probe; CompositionLoweringPlanner pl(probe);
    auto tl = TL(Parallel({Animate(Track("opacity", 0).To(1, 0.5, kEase).Hold(0.1)),
                           Animate(Track("offsetX", 0).To(100, 1.0)),
                           Animate(Track("rotation", 0).To(90, 0.3, Curve::Bezier(CubicBezier(0, 0, 1, 1))))}));
    LoweringPlan plan = pl.Plan(Snap(tl));
    CHECK(plan.FullyOffloaded() && plan.Count(PlanKind::CompositorKeyframes) == 3 && plan.props.size() == 3);
    const PropertyPlan* o = plan.Find("opacity"); CHECK(o != nullptr);
    if (o) {
        CHECK(o->paths == std::vector<std::string>{"Opacity"} && o->kind == PlanKind::CompositorKeyframes && o->reason.empty());
        const auto& k = o->keyframes;
        CHECK(k.keys.size() == 3);
        NEAR(k.keys[0].time, 0, 0); NEAR(k.keys[0].value, 0, 0); CHECK(!k.keys[0].bezier);
        NEAR(k.keys[1].time, 0.5, 1e-12); NEAR(k.keys[1].value, 1, 0);
        CHECK(k.keys[1].bezier); NEAR(k.keys[1].x1, 0.4, 0); NEAR(k.keys[1].y1, 0, 0); NEAR(k.keys[1].x2, 0.2, 0); NEAR(k.keys[1].y2, 1, 0);
        NEAR(k.keys[2].time, 0.6, 1e-12); CHECK(!k.keys[2].bezier);
        NEAR(k.duration, 0.6, 1e-12); NEAR(k.delay, 0, 0); NEAR(k.elapsed, 0, 0); NEAR(k.rate, 1, 0); CHECK(!k.reversed && !k.presetValue);
        NEAR(o->finalValue, 1, 0);
    }
    CHECK(!plan.Find("rotation")->keyframes.keys[1].bezier);          // linear bezier -> linear easing
    // sampled keyframes reproduce Track::Eval (reference semantics K1)
    const Track tr = Track("opacity", 0).To(1, 0.5, kEase).Hold(0.1).To(0.2, 0.4, Curve::Bezier(0.1, 0.9, 0.2, 1.0));
    auto tl2 = TL(Animate(tr)); LoweringPlan p2 = pl.Plan(Snap(tl2));
    for (double t = 0; t <= tr.Duration() + 0.1; t += 0.0137)
        NEAR(EvaluateKeyframes(p2.Find("opacity")->keyframes.keys, t), tr.Eval(t).value, 1e-9);

    // spring-only track -> one NaturalMotion spring on both scale paths
    auto sp = TL(Animate(Track("scale", 0.96).SpringTo(1, kSp)));
    LoweringPlan ps = pl.Plan(Snap(sp));
    const PropertyPlan* s = ps.Find("scale"); CHECK(s && s->kind == PlanKind::CompositorSpring);
    if (s) {
        CHECK(s->paths == std::vector<std::string>({"Scale.X", "Scale.Y"}));
        NEAR(s->spring.period, 0.3 / kTwoPi, 1e-15); NEAR(s->spring.dampingRatio, 0.8, 0);   // S1 v2: Period = r/(2π)
        NEAR(s->spring.from, 0.96, 0); NEAR(s->spring.fromVelocity, 0, 0); NEAR(s->spring.target, 1, 0); NEAR(s->spring.delay, 0, 0);
        NEAR(s->finalValue, 1, 0);
    }
    // the calibration knob is the single place that changes (periodScale overrides 1/(2π))
    CompositionLoweringPlanner scaled(probe, SpringMapping{2.0, 1.0});
    NEAR(scaled.Plan(Snap(sp)).Find("scale")->spring.period, 0.6, 1e-12);
    NEAR(scaled.Plan(Snap(sp)).Find("scale")->spring.dampingRatio, 0.8, 1e-12);
    // dampingScale that would push ζ>1 after ToCompositor → v1 Fallback (not lowered)
    CompositionLoweringPlanner hiMap(probe, SpringMapping{1.0, 1.5});  // 0.8*1.5=1.2
    CHECK(hiMap.Plan(Snap(sp)).Find("scale")->kind == PlanKind::AppThread);
    CHECK(Has(hiMap.Plan(Snap(sp)).Find("scale")->reason, "overdamped"));
    // rate folds into the spring: period / rate, velocity * rate; mid-flight seed from the closed form
    LoweringPlan pr = pl.Plan(Snap(sp, 0.1, true, 2.0));
    const Spring ref(kSp, 0.96, 1.0, 0.0); const SpringState st = ref.At(0.1);
    NEAR(pr.Find("scale")->spring.from, st.value, 1e-12); NEAR(pr.Find("scale")->spring.fromVelocity, st.velocity * 2.0, 1e-9);
    NEAR(pr.Find("scale")->spring.period, 0.15 / kTwoPi, 1e-12);
}

static void TestFallbacks() {
    StaticCapabilityProbe probe; CompositionLoweringPlanner pl(probe);
    auto expect = [&](const std::shared_ptr<const Timeline>& tl, const char* prop, const char* why,
                      double pos = 0, bool playing = true, double rate = 1, bool rev = false, int loops = 1, int line = 0) {
        LoweringPlan p = pl.Plan(Snap(tl, pos, playing, rate, rev, loops));
        const PropertyPlan* pp = p.Find(prop);
        ++g_checks;
        if (!pp || pp->kind != PlanKind::AppThread || !Has(pp->reason, why)) {
            ++g_fail; std::printf("FAIL line %d: %s should fall back with '%s'; plan:\n%s", line, prop, why, p.ToString().c_str());
        }
    };
    expect(TL(Animate(Track("item0.opacity", 0).To(1, 1.0))), "item0.opacity", "no compositor mapping", 0, true, 1, false, 1, __LINE__);
    expect(TL(Animate(Track("scale", 1).To(0.96, 0.1).SpringTo(1, kSp))), "scale", "mixed spring and keyframe", 0, true, 1, false, 1, __LINE__);
    // overshoot fallback when capability is OFF (default is now true after 派题 #20)
    {
        StaticCapabilityProbe pOff; Capabilities cOff; cOff.bezierOvershoot = false; pOff.SetCaps(cOff);
        LoweringPlan pOv = CompositionLoweringPlanner(pOff).Plan(Snap(TL(Animate(Track("opacity", 0).To(1, 1.0, Curve::Bezier(0.3, -0.5, 0.7, 1.5))))));
        ++g_checks;
        if (!pOv.Find("opacity") || pOv.Find("opacity")->kind != PlanKind::AppThread || !Has(pOv.Find("opacity")->reason, "overshoot")) {
            ++g_fail; std::printf("FAIL overshoot-off fallback\n");
        }
    }
    expect(TL(Sequence({Animate(Track("opacity", 0).To(1, 1.0)), Animate(Track("opacity", 1).To(0, 1.0))})), "opacity", "2 tracks drive", 0, true, 1, false, 1, __LINE__);
    expect(TL(Animate(Track("opacity", 0).To(1, 1.0))), "opacity", "looping", 0, true, 1, false, 3, __LINE__);
    expect(TL(Animate(Track("opacity", 0).To(1, 0.0).To(2, 1.0))), "opacity", "instant-jump", 0, true, 1, false, 1, __LINE__);
    expect(TL(Animate(Track("scale", 0).SpringTo(1, kSp))), "scale", "cannot be played in reverse", 0.1, true, 1, true, 1, __LINE__);
    // capability switches
    StaticCapabilityProbe p2; Capabilities c; c.springNaturalMotion = false; p2.SetCaps(c);
    LoweringPlan a = CompositionLoweringPlanner(p2).Plan(Snap(TL(Animate(Track("scale", 0).SpringTo(1, kSp)))));
    CHECK(a.Find("scale")->kind == PlanKind::AppThread && Has(a.Find("scale")->reason, "unavailable"));
    c = Capabilities{}; c.cubicBezier = false; p2.SetCaps(c);
    CompositionLoweringPlanner nb(p2);
    CHECK(nb.Plan(Snap(TL(Animate(Track("opacity", 0).To(1, 1.0, kEase))))).Find("opacity")->kind == PlanKind::AppThread);
    CHECK(nb.Plan(Snap(TL(Animate(Track("opacity", 0).To(1, 1.0))))).Find("opacity")->kind == PlanKind::CompositorKeyframes);
    c = Capabilities{}; c.maxKeyframes = 2; p2.SetCaps(c);
    CHECK(Has(CompositionLoweringPlanner(p2).Plan(Snap(TL(Animate(Track("opacity", 0).To(1, 1.0).To(0, 1.0))))).Find("opacity")->reason, "too many"));
    p2.MutableCaps() = Capabilities{}; p2.MutableCaps().bezierOvershoot = true;
    LoweringPlan ov = CompositionLoweringPlanner(p2).Plan(Snap(TL(Animate(Track("opacity", 0).To(1, 1.0, Curve::Bezier(0.3, -0.5, 0.7, 1.5))))));
    CHECK(ov.Find("opacity")->kind == PlanKind::CompositorKeyframes && ov.Find("opacity")->keyframes.keys[1].y1 == -0.5);
    // runtime-extensible mapping
    p2.Add("blur", {"Effects.Blur"}); p2.Remove("opacity");
    LoweringPlan mp = CompositionLoweringPlanner(p2).Plan(Snap(TL(Parallel({Animate(Track("blur", 0).To(1, 1.0)), Animate(Track("opacity", 0).To(1, 1.0))}))));
    CHECK(mp.Find("blur")->kind == PlanKind::CompositorKeyframes && mp.Find("opacity")->kind == PlanKind::AppThread);
    // mixed plan
    auto mix = TL(Parallel({Animate(Track("opacity", 0).To(1, 1.0)), Animate(Track("foo", 0).To(1, 1.0)),
                            Animate(Track("scale", 0.9).SpringTo(1, kSp))}));
    LoweringPlan mx = pl.Plan(Snap(mix));
    CHECK(mx.Count(PlanKind::CompositorKeyframes) == 1 && mx.Count(PlanKind::CompositorSpring) == 1 &&
          mx.Count(PlanKind::AppThread) == 1 && !mx.FullyOffloaded() && Has(mx.ToString(), "foo: app-thread"));
}

static void TestTimingAndHolds() {
    StaticCapabilityProbe probe; CompositionLoweringPlanner pl(probe);
    auto tl = TL(Sequence({Delay(0.2), Animate(Track("opacity", 0).To(1, 0.5))}));     // track: start 0.2, end 0.7
    // copy the entry: Find() points into the plan, which would otherwise die with the temporary
    auto k = [&](double pos, double rate, bool rev) { return std::make_shared<PropertyPlan>(*pl.Plan(Snap(tl, pos, true, rate, rev)).Find("opacity")); };
    auto a = k(0.0, 2.0, false);
    CHECK(a->kind == PlanKind::CompositorKeyframes);
    NEAR(a->keyframes.delay, 0.1, 1e-12); NEAR(a->keyframes.elapsed, 0, 0); NEAR(*a->keyframes.presetValue, 0, 0); NEAR(a->keyframes.rate, 2, 0);
    auto b = k(0.3, 1.0, false);
    NEAR(b->keyframes.delay, 0, 0); NEAR(b->keyframes.elapsed, 0.1, 1e-12); CHECK(!b->keyframes.presetValue);
    auto c = k(0.8, 1.0, false);                                         // finished: hold the final value
    CHECK(c->kind == PlanKind::Hold); NEAR(c->holdValue, 1.0, 0);
    auto d = k(0.5, 1.0, true);                                          // reverse, in progress
    NEAR(d->keyframes.elapsed, 0.2, 1e-12); NEAR(d->keyframes.delay, 0, 0); CHECK(d->keyframes.reversed); NEAR(d->finalValue, 0, 0);
    auto e = k(0.9, 2.0, true);                                          // reverse, not yet reached
    NEAR(e->keyframes.delay, 0.1, 1e-12); NEAR(*e->keyframes.presetValue, 1.0, 0); NEAR(e->keyframes.elapsed, 0, 0);
    auto f = k(0.2, 1.0, true);                                          // reverse, finished
    CHECK(f->kind == PlanKind::Hold); NEAR(f->holdValue, 0.0, 0);
    // paused/seeked pose -> Hold at Evaluate(position)
    auto g = std::make_shared<PropertyPlan>(*pl.Plan(Snap(tl, 0.35, false)).Find("opacity"));
    CHECK(g->kind == PlanKind::Hold); NEAR(g->holdValue, 0.3, 1e-12);
    // delayed spring
    auto ds = TL(Sequence({Delay(0.2), Animate(Track("scale", 0.5).SpringTo(1, kSp))}));
    auto h = std::make_shared<PropertyPlan>(*pl.Plan(Snap(ds, 0.0, true, 2.0)).Find("scale"));
    CHECK(h->kind == PlanKind::CompositorSpring); NEAR(h->spring.delay, 0.1, 1e-12); NEAR(*h->spring.presetValue, 0.5, 0);
    NEAR(h->spring.from, 0.5, 0);

    // overrides: seed = state at the snapshot clock; settled -> Hold; unmapped -> app thread; mask the graph
    const SpringParams p{0.3, 0.8};
    ControllerSnapshot s; s.id = 1; s.target = 1; s.clockTime = 1.1;
    s.overrides.push_back({"opacity", p, 0.2, 5.0, 1.0, 1.0});
    s.overrides.push_back({"foo", p, 0, 0, 1, 1.0});
    s.ownedProperties = {"opacity", "foo"};
    LoweringPlan op = pl.Plan(s);
    const SpringState st = Spring(p, 0.2, 1.0, 5.0).At(0.1);
    CHECK(op.Find("opacity")->kind == PlanKind::CompositorSpring);
    NEAR(op.Find("opacity")->spring.from, st.value, 1e-12); NEAR(op.Find("opacity")->spring.fromVelocity, st.velocity, 1e-9);
    NEAR(op.Find("opacity")->spring.target, 1.0, 0);
    CHECK(op.Find("foo")->kind == PlanKind::AppThread && Has(op.Find("foo")->reason, "no compositor mapping"));
    s.clockTime = 30.0; LoweringPlan settled = pl.Plan(s);
    CHECK(settled.Find("opacity")->kind == PlanKind::Hold); NEAR(settled.Find("opacity")->holdValue, 1.0, 0);
    ControllerSnapshot m = Snap(TL(Animate(Track("opacity", 0).To(1, 1.0))), 0.1);
    m.clockTime = 1.0; m.overrides.push_back({"opacity", p, 0.5, 0, 0.0, 1.0});
    LoweringPlan mp = pl.Plan(m);
    CHECK(mp.props.size() == 1 && mp.Find("opacity")->kind == PlanKind::CompositorSpring);
    // properties that are owned but have no source produce no plan entry
    ControllerSnapshot e2; e2.ownedProperties = {"ghost"}; CHECK(pl.Plan(e2).props.empty());
}

static std::shared_ptr<const Timeline> OpacityKf(double d = 1.0) { return TL(Animate(Track("opacity", 0).To(1, d, kEase))); }

static void TestDispatcherLifecycle() {
    {   // start, finish exactly on the final value, nothing left running
        Rig r; auto h = r.sys.Create(1, OpacityKf(0.5)); h.Play();
        CHECK(r.be.startKf == 1 && r.disp.LiveAnimationCount() == 1 && r.disp.PlanFor(h.Id()) != nullptr);
        for (int i = 0; i < 90; ++i) r.Step(1.0 / 120.0);
        CHECK(h.State() == AnimationState::Finished && r.disp.LiveAnimationCount() == 0 && r.disp.PlanFor(h.Id()) == nullptr);
        NEAR(r.be.Value(1, "Opacity"), 1.0, 0);
        CHECK(r.be.stops == 1 && r.be.sets == 1);
    }
    {   // a changed snapshot REPLACES the animation, never duplicates it
        Rig r; auto h = r.sys.Create(1, OpacityKf()); h.Play(); r.Step(0.3); h.SetRate(2.0);
        CHECK(r.be.startKf == 2 && r.disp.LiveAnimationCount() == 1);
        h.Pause(); CHECK(r.disp.LiveAnimationCount() == 0 && r.be.stops == 1);       // pause = hold the pose
        const double held = r.be.Value(1, "Opacity"); r.Step(0.5); NEAR(r.be.Value(1, "Opacity"), held, 0);
        NEAR(held, h.Value("opacity")->value, 1e-9);
    }
    {   // withdraw stops everything; Cancelled does not set a value
        Rig r;
        auto h = r.sys.Create(1, TL(Parallel({Animate(Track("opacity", 0).To(1, 1.0)), Animate(Track("scale", 1).SpringTo(2, kSp)),
                                              Animate(Track("offsetX", 0).To(5, 1.0))})));
        h.Play(); r.Step(0.2);
        CHECK(r.disp.LiveAnimationCount() == 4);                                      // Opacity, Scale.X, Scale.Y, Offset.X
        h.Cancel();
        CHECK(r.disp.LiveAnimationCount() == 0 && r.be.stops == 4 && r.be.sets == 0 && r.be.anims.empty());
        CHECK(r.disp.PlanFor(h.Id()) == nullptr);
        const double v = r.be.Value(1, "Opacity"); r.Step(1.0); NEAR(r.be.Value(1, "Opacity"), v, 0);   // frozen
    }
    {   // a reversed run ends on the INITIAL value
        Rig r; auto h = r.sys.Create(1, OpacityKf(1.0)); h.Play(); r.Step(0.6); h.Reverse();
        for (int i = 0; i < 100; ++i) r.Step(0.01);
        CHECK(h.State() == AnimationState::Finished); NEAR(r.be.Value(1, "Opacity"), 0.0, 0);
    }
    {   // reduced motion: instant completion becomes a Hold, no compositor animation at all
        Rig r; r.sys.SetReducedMotion(true);
        auto h = r.sys.Create(1, TL(Parallel({Animate(Track("opacity", 0).To(1, 1.0)), Animate(Track("scale", 0.9).SpringTo(1, kSp))})));
        h.Play();
        CHECK(h.State() == AnimationState::Finished && r.be.startKf == 0 && r.be.startSp == 0 && r.disp.LiveAnimationCount() == 0);
        NEAR(r.be.Value(1, "Opacity"), 1.0, 0); NEAR(r.be.Value(1, "Scale.X"), 1.0, 0); NEAR(r.be.Value(1, "Scale.Y"), 1.0, 0);
    }
}

static void TestRetargetUpdatesSeed() {
    Rig r; auto h = r.sys.Create(1, OpacityKf()); h.Play(); r.Step(0.3);
    const Sample before = *h.Value("opacity");
    CHECK(h.Retarget("opacity", 0.0, kSp));
    const PropertyPlan* p = r.disp.PlanFor(h.Id())->Find("opacity");
    CHECK(p && p->kind == PlanKind::CompositorSpring && r.be.startSp == 1);
    if (p) { NEAR(p->spring.from, before.value, 1e-9); NEAR(p->spring.fromVelocity, before.velocity, 1e-9); NEAR(p->spring.target, 0.0, 0); }
    CHECK(r.disp.LiveAnimationCount() == 1);                    // the spring replaced the keyframes on the same path
    r.Step(0.1); h.Pause();                                     // resubmit mid-spring: the seed is advanced to now
    const Sample now = *h.Value("opacity");
    const PropertyPlan* held = r.disp.PlanFor(h.Id())->Find("opacity");
    CHECK(held->kind == PlanKind::CompositorSpring);            // override still running while the graph is paused
    NEAR(held->spring.from, now.value, 1e-9); NEAR(held->spring.fromVelocity, now.velocity, 1e-9);
    r.Step(0.05); const Sample b2 = *h.Value("opacity");
    CHECK(h.Retarget("opacity", 0.8, {0.2, 1.0}));
    const PropertyPlan* p2 = r.disp.PlanFor(h.Id())->Find("opacity");
    NEAR(p2->spring.from, b2.value, 1e-9); NEAR(p2->spring.fromVelocity, b2.velocity, 1e-9); NEAR(p2->spring.target, 0.8, 0);
    CHECK(r.be.startSp == 3);
    r.Step(5.0);
    CHECK(r.disp.LiveAnimationCount() == 0); NEAR(r.be.Value(1, "Opacity"), 0.8, 0);
}

static void TestOwnershipHandoff() {
    Rig r;
    auto A = r.sys.Create(1, TL(Parallel({Animate(Track("opacity", 0).To(1, 1.0)), Animate(Track("offsetX", 0).To(10, 1.0))})));
    A.Play(); r.Step(0.2);
    const double beforeV = r.be.Value(1, "Opacity");
    auto B = r.sys.Create(1);
    CHECK(B.Retarget("opacity", 0.0, kSp));
    const int stopIdx = r.be.IndexOf("stop Opacity");
    const int startIdx = r.be.IndexOf("start-spring Opacity");
    CHECK(stopIdx >= 0 && startIdx > stopIdx);                  // old animation stopped, then the new one starts
    NEAR(r.be.Value(1, "Opacity"), beforeV, 1e-9);              // no jump at the handoff
    NEAR(r.be.Value(1, "Opacity"), B.Value("opacity")->value, 1e-9);
    CHECK(A.State() == AnimationState::Playing && r.disp.LiveAnimationCount() == 2);   // A keeps offsetX
    auto C = r.sys.Create(1); CHECK(C.Retarget("offsetX", 0.0, kSp));
    CHECK(A.State() == AnimationState::Superseded && r.disp.LiveAnimationCount() == 2 && r.disp.PlanFor(A.Id()) == nullptr);
    r.Step(5.0);
    NEAR(r.be.Value(1, "Opacity"), 0.0, 0); NEAR(r.be.Value(1, "Offset.X"), 0.0, 0);
    CHECK(r.disp.LiveAnimationCount() == 0);
}

static void TestUnknownAndGolden() {
    Rig r;
    auto tl = TL(Parallel({Animate(Track("foo", 0).To(10, 0.5, kEase).Hold(0.2)),
                           Animate(Track("bar", 1).SpringTo(5, {0.3, 0.7})),
                           Animate(Track("opacity", 0).To(1, 1.0))}));
    auto h = r.sys.Create(1, tl); h.Play();
    const double D = tl->Duration();
    CHECK(r.disp.PlanFor(h.Id())->Find("foo")->kind == PlanKind::AppThread &&
          r.disp.PlanFor(h.Id())->Find("bar")->kind == PlanKind::AppThread &&
          r.disp.PlanFor(h.Id())->Find("opacity")->kind == PlanKind::CompositorKeyframes);
    int compared = 0;
    for (int i = 1; i <= 360; ++i) {
        r.Step(1.0 / 120.0);
        const SampleMap g = tl->Evaluate(std::min(r.ck.Now(), D));
        NEAR(r.be.Value(1, "foo"), g.at("foo").value, 1e-9);        // Apply() == golden Evaluate()
        NEAR(r.be.Value(1, "bar"), g.at("bar").value, 1e-9);
        ++compared;
        if (r.ck.Now() < 0.99) CHECK(r.be.Count("set Opacity") == 0);   // compositor-driven: never written by Apply
    }
    CHECK(compared == 360 && h.State() == AnimationState::Finished);
    NEAR(r.be.Value(1, "foo"), 10.0, 0); NEAR(r.be.Value(1, "bar"), 5.0, 0); NEAR(r.be.Value(1, "Opacity"), 1.0, 0);
    CHECK(r.be.Count("set Opacity") == 1);                          // exactly one: the completion value
    // unknown properties use their own name as the backend key
    CHECK(r.be.Count("set foo") > 100 && r.be.Count("set bar") > 100);
    CHECK(r.disp.LiveAnimationCount() == 0);
}

// Run a scenario, comparing the simulated compositor with the controller at every tick.
static void Compare(Rig& r, AnimationHandle& h, const char* prop, const char* path, int steps, double dt, double tol) {
    for (int i = 0; i < steps; ++i) {
        r.Step(dt);
        auto v = h.Value(prop);
        if (!v) continue;
        NEAR(r.be.Value(1, path), v->value, tol);
    }
}

static void TestEndToEndSimulation() {
    const double dt = 1.0 / 120.0;
    {   Rig r; auto h = r.sys.Create(1, TL(Parallel({Animate(Track("opacity", 0).To(1, 0.5, kEase)), Animate(Track("offsetX", 0).To(100, 1.0))})));
        h.Play(); Compare(r, h, "opacity", "Opacity", 130, dt, 1e-9);
        NEAR(r.be.Value(1, "Offset.X"), 100.0, 0); }
    {   Rig r; auto h = r.sys.Create(1, TL(Sequence({Delay(0.2), Animate(Track("opacity", 0).To(1, 0.5))})));   // delayed start
        h.Play(); Compare(r, h, "opacity", "Opacity", 120, dt, 1e-9); }
    {   Rig r; auto h = r.sys.Create(1, TL(Animate(Track("opacity", 0).To(1, 1.0, kEase))));                      // rate change mid-flight
        h.Play(); Compare(r, h, "opacity", "Opacity", 36, dt, 1e-9);
        h.SetRate(2.0); Compare(r, h, "opacity", "Opacity", 60, dt, 1e-9); }
    {   Rig r; auto h = r.sys.Create(1, TL(Animate(Track("opacity", 0).To(1, 1.0, kEase))));                      // pause, resume, reverse
        h.Play(); Compare(r, h, "opacity", "Opacity", 48, dt, 1e-9);
        h.Pause(); Compare(r, h, "opacity", "Opacity", 36, dt, 1e-9);
        h.Play(); Compare(r, h, "opacity", "Opacity", 24, dt, 1e-9);
        h.Reverse(); Compare(r, h, "opacity", "Opacity", 100, dt, 1e-9);
        CHECK(h.State() == AnimationState::Finished); }
    {   Rig r; auto h = r.sys.Create(1, TL(Sequence({Delay(0.1), Animate(Track("scale", 0.5).SpringTo(1, {0.3, 0.6}))})));   // delayed spring
        h.Play();
        Compare(r, h, "scale", "Scale.X", 12, dt, 1e-9);                      // still in the delay: preset value
        Compare(r, h, "scale", "Scale.X", 48, dt, 1e-9);                      // mid-flight: exact
        Compare(r, h, "scale", "Scale.X", 120, dt, 2e-3);     // tail: compositor rest vs our snap (S5)
        NEAR(r.be.Value(1, "Scale.Y"), 1.0, 0); }
    {   Rig r; auto h = r.sys.Create(1, TL(Animate(Track("opacity", 0).To(1, 1.0, kEase))));                      // retarget mid-flight
        h.Play(); Compare(r, h, "opacity", "Opacity", 30, dt, 1e-9);
        h.Retarget("opacity", 0.2, kSp);
        Compare(r, h, "opacity", "Opacity", 30, dt, 1e-9);
        h.Retarget("opacity", 0.9, {0.2, 1.0});
        Compare(r, h, "opacity", "Opacity", 30, dt, 1e-9);
        Compare(r, h, "opacity", "Opacity", 120, dt, 2e-3); }
    {   Rig r; auto h = r.sys.Create(1, TL(Animate(Track("rotation", 0).To(90, 0.4, Curve::Bezier(0.2, 0, 0, 1)))));   // at 2x from the start
        h.SetRate(2.0); h.Play(); Compare(r, h, "rotation", "RotationAngleInDegrees", 60, dt, 1e-9); }
}

// 派题 #14: ζ>1 must not lower to CompositorSpring (v1); ζ≤1 unchanged.
static void TestOverdampedGate() {
    StaticCapabilityProbe probe; CompositionLoweringPlanner pl(probe);
    // PlanGraph: underdamped / critical still lower
    auto u = TL(Animate(Track("scale", 0.96).SpringTo(1, {0.3, 0.8})));
    CHECK(pl.Plan(Snap(u)).Find("scale")->kind == PlanKind::CompositorSpring);
    auto crit = TL(Animate(Track("scale", 0.96).SpringTo(1, {0.3, 1.0})));
    CHECK(pl.Plan(Snap(crit)).Find("scale")->kind == PlanKind::CompositorSpring);
    NEAR(pl.Plan(Snap(crit)).Find("scale")->spring.dampingRatio, 1.0, 0);
    // PlanGraph: overdamped → Fallback
    auto od = TL(Animate(Track("scale", 0.96).SpringTo(1, {0.3, 1.2})));
    LoweringPlan pod = pl.Plan(Snap(od));   // keep plan alive: Find() points into it
    const PropertyPlan* p = pod.Find("scale");
    CHECK(p && p->kind == PlanKind::AppThread && Has(p->reason, "overdamped spring (zeta>1)"));
    // PlanOverride / Retarget path via MotionSystem
    Rig r; auto h = r.sys.Create(1, TL(Animate(Track("opacity", 0).To(1, 0.5))));
    h.Play(); r.Step(0.1);
    CHECK(h.Retarget("opacity", 0.0, {0.3, 0.8}));
    r.sys.Tick();
    CHECK(r.disp.PlanFor(h.Id())->Find("opacity")->kind == PlanKind::CompositorSpring);
    CHECK(h.Retarget("opacity", 1.0, {0.3, 1.2}));
    r.sys.Tick();
    const PropertyPlan* po = r.disp.PlanFor(h.Id())->Find("opacity");
    CHECK(po && po->kind == PlanKind::AppThread && Has(po->reason, "overdamped"));
}


static void TestPeriodPositiveGate() {
    StaticCapabilityProbe probe; CompositionLoweringPlanner pl(probe);
    // Normal period lowers
    auto ok = TL(Animate(Track("scale", 0.96).SpringTo(1, {0.3, 0.8})));
    CHECK(pl.Plan(Snap(ok)).Find("scale")->kind == PlanKind::CompositorSpring);
    // response == 0 → AppThread (#26 Zero throws on API)
    auto z = TL(Animate(Track("scale", 0.96).SpringTo(1, {0.0, 0.8})));
    LoweringPlan pz = pl.Plan(Snap(z));
    const PropertyPlan* p = pz.Find("scale");
    CHECK(p && p->kind == PlanKind::AppThread && Has(p->reason, "Period/response must be > 0"));
    // negative response → AppThread
    auto neg = TL(Animate(Track("scale", 0.96).SpringTo(1, {-0.01, 0.8})));
    LoweringPlan pn = pl.Plan(Snap(neg));
    const PropertyPlan* q = pn.Find("scale");
    CHECK(q && q->kind == PlanKind::AppThread && Has(q->reason, "Period/response must be > 0"));
}

// S1 v2 (ruling A1, 2026-10-20): centralized mapping Period = r/(2π), DampingRatio = ζ.
static void TestS1RemapV2() {
    // the free function and the default SpringMapping agree
    for (double r : {0.001, 0.05, 0.3, 0.32, 1.0, 10.0}) for (double z : {0.0, 0.5, 0.78, 1.0, 1.3}) {
        const SpringParams c = MapSpringToCompositor({r, z});
        NEAR(c.response, r / kTwoPi, 1e-15); NEAR(c.damping, z, 0);
        const SpringParams d = SpringMapping{}.ToCompositor({r, z});
        NEAR(d.response, c.response, 0); NEAR(d.damping, c.damping, 0);
    }
    NEAR(kPeriodPerResponse * kTwoPi, 1.0, 1e-15);
    NEAR(kPeriodPerResponse, 0.159154943, 1e-9);                      // #36 fit s = 0.159
    // compositor-side reference (ω0 = 1/Period) reproduces spring.h(r, ζ) exactly after the remap
    StaticCapabilityProbe probe; CompositionLoweringPlanner pl(probe);
    const SpringParams P{0.32, 0.78};
    auto tl = TL(Animate(Track("scale", 0.0).SpringTo(1, P)));
    auto planS = pl.Plan(Snap(tl));
    const PropertyPlan* s = planS.Find("scale");
    CHECK(s && s->kind == PlanKind::CompositorSpring);
    if (s) {
        NEAR(s->spring.period, 0.32 / kTwoPi, 1e-15);
        const Spring ref(P, 0.0, 1.0, 0.0);
        for (double t = 0; t <= 1.0; t += 0.01) NEAR(SimulateSpring(s->spring, t, 0.0), ref.At(t).value, 1e-12);
        // the superseded v1 mapping (Period = r) would be ~2π slower: clearly different mid-flight
        SpringAnimation v1 = s->spring; v1.period = 0.32;
        CHECK(std::fabs(SimulateSpring(v1, 0.1, 0.0) - ref.At(0.1).value) > 0.2);
    }
    // gates still hold after the remap: ζ ≤ 1 lowers, ζ > 1 does not (mapped DampingRatio = ζ)
    auto z1 = TL(Animate(Track("scale", 0.0).SpringTo(1, {0.32, 1.0})));
    CHECK(pl.Plan(Snap(z1)).Find("scale")->kind == PlanKind::CompositorSpring);
    auto z2 = TL(Animate(Track("scale", 0.0).SpringTo(1, {0.32, 1.0001})));
    CHECK(Has(pl.Plan(Snap(z2)).Find("scale")->reason, "overdamped"));
    // tiny authoring response still lowers (mapped Period = 0.001/(2π) > 0)
    auto tiny = TL(Animate(Track("scale", 0.0).SpringTo(1, {0.001, 0.8})));
    auto planTiny = pl.Plan(Snap(tiny));
    const PropertyPlan* pt = planTiny.Find("scale");
    CHECK(pt && pt->kind == PlanKind::CompositorSpring && pt->spring.period > 0.0);
    // post-mapping Period gate: a bad calibration knob (periodScale ≤ 0) falls back instead of sending Period ≤ 0
    CompositionLoweringPlanner bad(probe, SpringMapping{0.0, 1.0});
    auto planBad = bad.Plan(Snap(tl));
    const PropertyPlan* pb = planBad.Find("scale");
    CHECK(pb && pb->kind == PlanKind::AppThread && Has(pb->reason, "mapped Period"));
}

int main() {
    TestPurePlans(); TestFallbacks(); TestTimingAndHolds(); TestDispatcherLifecycle();
    TestRetargetUpdatesSeed(); TestOwnershipHandoff(); TestUnknownAndGolden(); TestEndToEndSimulation();
    TestOverdampedGate();
    TestPeriodPositiveGate();
    TestS1RemapV2();
    std::printf("%d checks, %d failures\n", g_checks, g_fail);
    return g_fail ? 1 : 0;
}
