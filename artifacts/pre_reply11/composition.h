// composition.h: capability probe, lowering planner, and CompositionDispatcher.
// Module 6. C++17, standard library only (no WinRT here: the compositor is behind
// ICompositorBackend, which module 7 implements with Windows.UI.Composition).
// Depends on controller.h (IDispatcher, ControllerSnapshot) and earlier headers.
//
// WHAT THIS MODULE DECIDES
//   For every property a controller owns, the planner picks ONE of:
//     CompositorKeyframes  linear/cubic-bezier keyframes run by the compositor
//     CompositorSpring     one NaturalMotion spring run by the compositor
//     Hold                 set a value once, nothing animates (paused pose, settled)
//     AppThread            keep evaluating on the app thread (reason recorded)
//   `Hold` is an addition to the three kinds in the brief: a paused pose or a
//   settled spring needs "set once and stop", which is neither an animation nor
//   per-frame work.
//
// LOWERING RULES (v1). A graph property is lowered only when ALL hold:
//   * it has a compositor mapping (probe), and the controller is playing
//     (otherwise it becomes a Hold at Timeline::Evaluate(position));
//   * loops == 1;
//   * exactly one track drives it (the overwrite/backfill conflict rule is not lowered);
//   * the track has no zero-duration (instant jump) segment;
//   * the track is either all linear/bezier/hold segments (keyframes) or exactly one
//     spring segment (CompositorSpring; not reversed). Mixed spring + keyframe
//     tracks stay on the app thread (planned extension: chained stages);
//   * bezier y control points stay in [0,1] unless the probe says overshoot works;
//   * the keyframe count is within the probe's limit.
//   A spring OVERRIDE (from Retarget) is lowered to CompositorSpring seeded with the
//   state at the snapshot's clock time (the spring ODE is time-invariant, so a
//   restart from the current state is equivalent). A settled override is a Hold.
//   Anything finished (forward: position >= start + duration; reverse: position <=
//   start) is a Hold. Reduced motion needs nothing here: reduced timelines are
//   ordinary timelines, and instant ones complete at Play (playing == false -> Hold).
//
// TIME MODEL of a lowered animation (all in "animation-local" seconds except delay):
//   delay   real seconds until it starts advancing (presetValue is set immediately)
//   elapsed local seconds already played when submitted
//   rate    local seconds per real second
//   reversed plays backward; elapsed counts distance from the top
//   position(t_real) = elapsed + rate * (t_real - delay)          (forward)
//                    = duration - (elapsed + rate * (t_real - delay))  (reversed)
//   Springs fold the rate into the spring: period / rate, initial velocity * rate.
//
// ASSUMPTIONS ABOUT Windows.UI.Composition (ALL UNVERIFIED; see SpringMapping)
//   S1  SpringScalarNaturalMotionAnimation.Period is our `response` (undamped natural
//       period, 2*pi/omega0), i.e. NOT a damped period.
//   S2  DampingRatio is our `damping` (zeta, 1.0 = critical).
//   S3  InitialVelocity is in value-units per real second.
//   S4  The animation may be started with a DelayTime and the property keeps its
//       preset value until the delay elapses.
//   S5  The compositor's spring stops at ITS OWN rest threshold; ours snaps to the
//       target at SettleTime(0.1, 0.5). Small end-of-motion differences are expected.
//   K1  A keyframe's easing function shapes the segment ENDING at that keyframe.
//   K2  CubicBezierEasingFunction accepts control-point y outside [0,1] (assumed NO
//       by default: Capabilities::bezierOvershoot = false).
//   K3  An animation can start part-way (elapsed > 0), e.g. by AnimationController
//       Progress, and be played at a non-1 rate or in reverse.
//   K4  After an animation completes the property keeps its final value.
//   K5  StopAnimation leaves the property at its current animated value.
//   Everything the backend does with these is hidden behind ICompositorBackend so a
//   wrong assumption is fixed in one place (SpringMapping, Capabilities, or backend).
#pragma once
#include <algorithm>
#include <cmath>
#include <map>
#include <memory>
#include <optional>
#include <set>
#include <string>
#include <utility>
#include <vector>

#include "controller.h"

namespace motion {

// ----------------------------------------------------------- capabilities ----

struct Capabilities {
    bool springNaturalMotion = true;   // NaturalMotion spring animations exist on this OS
    bool cubicBezier = true;           // CubicBezierEasingFunction usable
    bool bezierOvershoot = false;      // y control points outside [0,1] honored (K2, unverified)
    std::size_t maxKeyframes = 512;
};

struct CompositorProperty {
    std::vector<std::string> paths;    // e.g. {"Scale.X", "Scale.Y"} for scalar "scale"
};

class ICapabilityProbe {
public:
    virtual ~ICapabilityProbe() = default;
    virtual Capabilities Caps() const = 0;
    virtual std::optional<CompositorProperty> Map(const std::string& property) const = 0;
};

// Default table + settable capabilities. Units by convention: rotation in degrees.
// Module 7 supplies a real probe (API presence checks via ApiInformation: unverified).
class StaticCapabilityProbe : public ICapabilityProbe {
public:
    StaticCapabilityProbe() {
        table_ = {{"opacity", {{"Opacity"}}},
                  {"offsetX", {{"Offset.X"}}}, {"offsetY", {{"Offset.Y"}}},
                  {"scale", {{"Scale.X", "Scale.Y"}}}, {"scaleX", {{"Scale.X"}}}, {"scaleY", {{"Scale.Y"}}},
                  {"rotation", {{"RotationAngleInDegrees"}}}};
    }
    Capabilities Caps() const override { return caps_; }
    std::optional<CompositorProperty> Map(const std::string& p) const override {
        auto it = table_.find(p);
        if (it == table_.end()) return std::nullopt;
        return it->second;
    }
    void SetCaps(const Capabilities& c) { caps_ = c; }
    Capabilities& MutableCaps() { return caps_; }
    void Add(const std::string& property, std::vector<std::string> paths) { table_[property] = {std::move(paths)}; }
    void Remove(const std::string& property) { table_.erase(property); }
private:
    Capabilities caps_;
    std::map<std::string, CompositorProperty> table_;
};

// UNVERIFIED mapping from our spring parameters to the compositor's (S1, S2).
// If hardware shows a different convention, change ONLY these two factors (or
// replace ToCompositor); nothing else in the planner depends on the convention.
struct SpringMapping {
    double periodScale = 1.0;     // compositorPeriod = response * periodScale
    double dampingScale = 1.0;    // compositorDampingRatio = damping * dampingScale
    SpringParams ToCompositor(SpringParams p) const { return {p.response * periodScale, p.damping * dampingScale}; }
};

// ------------------------------------------------------------------ plans ----

enum class PlanKind { CompositorKeyframes, CompositorSpring, Hold, AppThread };

inline const char* ToString(PlanKind k) {
    switch (k) {
        case PlanKind::CompositorKeyframes: return "compositor-keyframes";
        case PlanKind::CompositorSpring:    return "compositor-spring";
        case PlanKind::Hold:                return "hold";
        case PlanKind::AppThread:           return "app-thread";
    }
    return "?";
}

// Easing shapes the segment ENDING at this keyframe (K1).
struct KeyframeSpec {
    double time = 0, value = 0;                 // seconds within the animation, [0, duration]
    bool bezier = false;
    double x1 = 0, y1 = 0, x2 = 1, y2 = 1;      // used when bezier
};

struct KeyframeAnimation {
    std::vector<KeyframeSpec> keys;             // first key at time 0
    double duration = 0, delay = 0, elapsed = 0, rate = 1;
    bool reversed = false;
    std::optional<double> presetValue;          // set immediately when delay > 0
};

struct SpringAnimation {                        // already in COMPOSITOR terms
    double period = 0.35, dampingRatio = 0.8;   // seconds, ratio
    double from = 0, fromVelocity = 0, target = 0;   // velocity per real second
    double delay = 0;
    std::optional<double> presetValue;
};

struct PropertyPlan {
    std::string property;
    std::vector<std::string> paths;             // compositor paths; empty if unmapped
    PlanKind kind = PlanKind::AppThread;
    KeyframeAnimation keyframes;                // CompositorKeyframes
    SpringAnimation spring;                     // CompositorSpring
    double holdValue = 0;                       // Hold
    double finalValue = 0;                      // value when the animation completes
    std::string reason;                         // why AppThread (empty otherwise)
};

struct LoweringPlan {
    ControllerId id = 0;
    TargetId target = 0;
    double clockTime = 0;
    std::vector<PropertyPlan> props;

    const PropertyPlan* Find(const std::string& property) const {
        for (const auto& p : props) if (p.property == property) return &p;
        return nullptr;
    }
    std::size_t Count(PlanKind k) const {
        std::size_t n = 0; for (const auto& p : props) n += p.kind == k; return n;
    }
    bool FullyOffloaded() const { return Count(PlanKind::AppThread) == 0; }
    std::string ToString() const {
        std::string s;
        for (const auto& p : props) {
            s += p.property + ": " + motion::ToString(p.kind);
            if (!p.reason.empty()) s += " (" + p.reason + ")";
            s += "\n";
        }
        return s;
    }
};

// ------------------------- reference semantics (under the assumptions above) ----

inline double EvaluateKeyframes(const std::vector<KeyframeSpec>& k, double t) {
    if (k.empty()) return 0.0;
    if (!(t > k.front().time)) return k.front().value;
    if (t >= k.back().time) return k.back().value;
    auto it = std::upper_bound(k.begin(), k.end(), t, [](double v, const KeyframeSpec& s) { return v < s.time; });
    const KeyframeSpec& b = *it;
    const KeyframeSpec& a = *(it - 1);
    const double frac = (t - a.time) / (b.time - a.time);
    const double e = b.bezier ? CubicBezier(b.x1, b.y1, b.x2, b.y2).Ease(frac) : frac;
    return a.value + (b.value - a.value) * e;
}

// Value `t` real seconds after Start; `before` is the value held until the delay ends.
inline double SimulateKeyframes(const KeyframeAnimation& a, double t, double before) {
    if (t < a.delay) return before;
    const double adv = a.elapsed + a.rate * (t - a.delay);
    const double pos = a.reversed ? std::max(a.duration - adv, 0.0) : std::min(adv, a.duration);
    return EvaluateKeyframes(a.keys, pos);
}
inline double SimulateSpring(const SpringAnimation& a, double t, double before) {
    if (t < a.delay) return before;
    return Spring({a.period, a.dampingRatio}, a.from, a.target, a.fromVelocity).At(t - a.delay).value;
}

// ---------------------------------------------------------------- planner ----

class CompositionLoweringPlanner {
public:
    explicit CompositionLoweringPlanner(const ICapabilityProbe& probe, SpringMapping mapping = {})
        : probe_(&probe), mapping_(mapping) {}

    LoweringPlan Plan(const ControllerSnapshot& s) const {
        LoweringPlan plan;
        plan.id = s.id; plan.target = s.target; plan.clockTime = s.clockTime;
        const Capabilities caps = probe_->Caps();
        const std::set<std::string> owned(s.ownedProperties.begin(), s.ownedProperties.end());
        std::set<std::string> handled;
        for (const auto& o : s.overrides) {                       // overrides mask the graph
            if (!owned.count(o.property)) continue;
            handled.insert(o.property);
            plan.props.push_back(PlanOverride(o, s, caps));
        }
        if (s.timeline) {
            const SampleMap pose = s.timeline->Evaluate(s.position);
            std::map<std::string, std::vector<const PlacedTrack*>> byProp;
            for (const PlacedTrack& pt : s.timeline->Tracks()) byProp[pt.track->Property()].push_back(&pt);
            for (const auto& kv : pose) {
                if (!owned.count(kv.first) || handled.count(kv.first)) continue;
                plan.props.push_back(PlanGraph(kv.first, kv.second.value, byProp[kv.first], s, caps));
            }
        }
        return plan;
    }

private:
    static PropertyPlan Fallback(PropertyPlan pp, std::string why) {
        pp.kind = PlanKind::AppThread; pp.reason = std::move(why); return pp;
    }
    static PropertyPlan HoldAt(PropertyPlan pp, double v) {
        pp.kind = PlanKind::Hold; pp.holdValue = v; pp.finalValue = v; return pp;
    }

    PropertyPlan PlanOverride(const SpringOverrideInfo& o, const ControllerSnapshot& s, const Capabilities& caps) const {
        PropertyPlan pp; pp.property = o.property;
        auto m = probe_->Map(o.property);
        if (!m) return Fallback(pp, "property '" + o.property + "' has no compositor mapping");
        pp.paths = m->paths;
        const Spring sp(o.params, o.from, o.target, o.fromVelocity);
        const double elapsed = std::max(0.0, s.clockTime - o.startClock);
        if (elapsed >= sp.SettleTime(kSpringPosEps, kSpringVelEps)) return HoldAt(pp, o.target);
        if (!caps.springNaturalMotion) return Fallback(pp, "NaturalMotion spring animation unavailable");
        const SpringState st = sp.At(elapsed);                    // restart from the current state
        const SpringParams c = mapping_.ToCompositor(sp.Params());
        pp.kind = PlanKind::CompositorSpring;
        pp.spring.period = c.response; pp.spring.dampingRatio = c.damping;
        pp.spring.from = st.value; pp.spring.fromVelocity = st.velocity; pp.spring.target = o.target;
        pp.finalValue = o.target;
        return pp;
    }

    PropertyPlan PlanGraph(const std::string& p, double pose, const std::vector<const PlacedTrack*>& tracks,
                           const ControllerSnapshot& s, const Capabilities& caps) const {
        PropertyPlan pp; pp.property = p;
        auto m = probe_->Map(p);
        if (!m) return Fallback(pp, "property '" + p + "' has no compositor mapping");
        pp.paths = m->paths;
        if (!s.playing) return HoldAt(pp, pose);                  // paused or seeked pose
        if (s.loops != 1) return Fallback(pp, "looping timelines are not lowered (v1)");
        if (tracks.size() != 1)
            return Fallback(pp, std::to_string(tracks.size()) + " tracks drive '" + p +
                                "'; the overwrite/backfill conflict rule is not lowered (v1)");
        const Track& tr = *tracks[0]->track;
        const double start = tracks[0]->start, d = tr.Duration(), pos = s.position, rate = s.rate;
        const bool done = s.reversed ? pos <= start : pos >= start + d;
        if (done || tr.Segments().empty()) return HoldAt(pp, pose);

        bool hasSpring = false, hasZero = false, anyBezier = false, overshoot = false;
        for (const auto& sg : tr.Segments()) {
            if (!(sg.duration > 0.0)) hasZero = true;
            if (sg.curve.kind == Curve::Kind::Spring) hasSpring = true;
            if (sg.curve.kind == Curve::Kind::Bezier && !sg.curve.bezier.IsLinear()) {
                anyBezier = true;
                const CubicBezier& b = sg.curve.bezier;
                if (b.Y1() < 0 || b.Y1() > 1 || b.Y2() < 0 || b.Y2() > 1) overshoot = true;
            }
        }
        if (hasZero) return Fallback(pp, "instant-jump segment (duration 0) is not representable as keyframes");

        if (hasSpring) {
            if (tr.Segments().size() != 1) return Fallback(pp, "mixed spring and keyframe segments are not lowered (v1)");
            if (!caps.springNaturalMotion) return Fallback(pp, "NaturalMotion spring animation unavailable");
            if (s.reversed) return Fallback(pp, "spring segments cannot be played in reverse");
            const auto& sg = tr.Segments()[0];
            const Spring sp(sg.curve.spring, sg.from, sg.to, sg.entryVelocity);
            const SpringParams c = mapping_.ToCompositor(sp.Params());
            pp.kind = PlanKind::CompositorSpring;
            pp.spring.target = sg.to;
            pp.spring.period = c.response / rate;                 // rate folded into the spring
            pp.spring.dampingRatio = c.damping;
            const double local = pos - start;
            if (local <= 0.0) {
                pp.spring.delay = (start - pos) / rate;
                pp.spring.from = sg.from; pp.spring.fromVelocity = sg.entryVelocity * rate;
                pp.spring.presetValue = sg.from;
            } else {
                const SpringState st = sp.At(local);
                pp.spring.from = st.value; pp.spring.fromVelocity = st.velocity * rate;
            }
            pp.finalValue = sg.to;
            return pp;
        }

        if (anyBezier && !caps.cubicBezier) return Fallback(pp, "cubic-bezier easing unavailable");
        if (overshoot && !caps.bezierOvershoot)
            return Fallback(pp, "bezier overshoot (y outside [0,1]) is unverified on the compositor");
        if (tr.Segments().size() + 1 > caps.maxKeyframes) return Fallback(pp, "too many keyframes");

        KeyframeAnimation ka;
        ka.keys.push_back({0.0, tr.InitialValue(), false, 0, 0, 1, 1});
        for (const auto& sg : tr.Segments()) {
            KeyframeSpec k; k.time = sg.start + sg.duration; k.value = sg.to;
            if (sg.curve.kind == Curve::Kind::Bezier && !sg.curve.bezier.IsLinear()) {
                const CubicBezier& b = sg.curve.bezier;
                k.bezier = true; k.x1 = b.X1(); k.y1 = b.Y1(); k.x2 = b.X2(); k.y2 = b.Y2();
            }
            ka.keys.push_back(k);
        }
        ka.duration = d; ka.rate = rate; ka.reversed = s.reversed;
        if (!s.reversed) {
            if (pos >= start) ka.elapsed = pos - start;
            else { ka.delay = (start - pos) / rate; ka.presetValue = tr.InitialValue(); }
        } else {
            if (pos < start + d) ka.elapsed = d - (pos - start);
            else { ka.delay = (pos - (start + d)) / rate; ka.presetValue = tr.FinalValue(); }
        }
        pp.kind = PlanKind::CompositorKeyframes;
        pp.keyframes = std::move(ka);
        pp.finalValue = s.reversed ? tr.InitialValue() : tr.FinalValue();
        return pp;
    }

    const ICapabilityProbe* probe_;
    SpringMapping mapping_;
};

// ---------------------------------------------------------------- backend ----

// The only thing module 7 has to implement against Windows.UI.Composition.
// Start* REPLACES any animation already running on (target, path).
class ICompositorBackend {
public:
    virtual ~ICompositorBackend() = default;
    virtual void StartKeyframes(TargetId, const std::string& path, const KeyframeAnimation&) = 0;
    virtual void StartSpring(TargetId, const std::string& path, const SpringAnimation&) = 0;
    virtual void Stop(TargetId, const std::string& path) = 0;                 // keeps current value (K5)
    // `key` is a compositor path when the property is mapped, else the property name.
    virtual void SetValue(TargetId, const std::string& key, double value) = 0;
};

// ------------------------------------------------------------- dispatcher ----

class CompositionDispatcher final : public IDispatcher {
public:
    CompositionDispatcher(const ICapabilityProbe& probe, ICompositorBackend& backend, SpringMapping mapping = {})
        : planner_(probe, mapping), backend_(&backend) {}

    // Replace this controller's plan: (re)start what is lowered, stop what no longer is.
    void Submit(const ControllerSnapshot& snap) override {
        State& st = states_[snap.id];
        LoweringPlan plan = planner_.Plan(snap);
        const TargetId t = plan.target;
        std::set<std::string> newLive, newApp;
        for (const PropertyPlan& pp : plan.props) {
            switch (pp.kind) {
                case PlanKind::CompositorKeyframes:
                    for (const auto& path : pp.paths) {
                        backend_->StartKeyframes(t, path, pp.keyframes);
                        live_[{t, path}] = snap.id; newLive.insert(path);
                    }
                    break;
                case PlanKind::CompositorSpring:
                    for (const auto& path : pp.paths) {
                        backend_->StartSpring(t, path, pp.spring);
                        live_[{t, path}] = snap.id; newLive.insert(path);
                    }
                    break;
                case PlanKind::Hold:
                    for (const auto& path : pp.paths) { StopIfOwned(t, path, snap.id); backend_->SetValue(t, path, pp.holdValue); }
                    break;
                case PlanKind::AppThread:
                    for (const auto& path : pp.paths) StopIfOwned(t, path, snap.id);
                    app_[{t, pp.property}] = {snap.id, pp.paths.empty() ? std::vector<std::string>{pp.property} : pp.paths};
                    newApp.insert(pp.property);
                    break;
            }
        }
        for (const auto& path : st.livePaths) if (!newLive.count(path)) StopIfOwned(t, path, snap.id);
        for (const auto& prop : st.appProps) if (!newApp.count(prop)) EraseAppIfOwned(t, prop, snap.id);
        st.livePaths = std::move(newLive); st.appProps = std::move(newApp);
        st.target = t; st.plan = std::move(plan);
    }

    // Completed: the controller finished what it was doing, so end exactly on the
    // final value. Cancelled/Superseded: just stop (the value stays where the
    // compositor has it, K5).
    void Withdraw(ControllerId id, EndReason reason) override {
        auto it = states_.find(id);
        if (it == states_.end()) return;
        State& st = it->second;
        std::map<std::string, double> finals;
        for (const auto& pp : st.plan.props)
            if (pp.kind == PlanKind::CompositorKeyframes || pp.kind == PlanKind::CompositorSpring)
                for (const auto& path : pp.paths) finals[path] = pp.finalValue;
        for (const auto& path : st.livePaths) {
            if (!StopIfOwned(st.target, path, id)) continue;
            if (reason == EndReason::Completed) backend_->SetValue(st.target, path, finals[path]);
        }
        for (const auto& prop : st.appProps) EraseAppIfOwned(st.target, prop, id);
        states_.erase(it);
    }

    // App-thread values: only for properties some plan left on the app thread.
    // Compositor-driven and held properties are ignored (the compositor owns them).
    void Apply(TargetId target, const SampleMap& values) override {
        for (const auto& kv : values) {
            auto it = app_.find({target, kv.first});
            if (it == app_.end()) continue;
            for (const auto& key : it->second.keys) backend_->SetValue(target, key, kv.second.value);
        }
    }

    const LoweringPlan* PlanFor(ControllerId id) const {
        auto it = states_.find(id);
        return it == states_.end() ? nullptr : &it->second.plan;
    }
    std::size_t LiveAnimationCount() const { return live_.size(); }

private:
    using Key = std::pair<TargetId, std::string>;
    struct State { LoweringPlan plan; std::set<std::string> livePaths, appProps; TargetId target = 0; };
    struct AppEntry { ControllerId id = 0; std::vector<std::string> keys; };

    bool StopIfOwned(TargetId t, const std::string& path, ControllerId id) {
        auto it = live_.find({t, path});
        if (it == live_.end() || it->second != id) return false;
        backend_->Stop(t, path);
        live_.erase(it);
        return true;
    }
    void EraseAppIfOwned(TargetId t, const std::string& prop, ControllerId id) {
        auto it = app_.find({t, prop});
        if (it != app_.end() && it->second.id == id) app_.erase(it);
    }

    CompositionLoweringPlanner planner_;
    ICompositorBackend* backend_;
    std::map<ControllerId, State> states_;
    std::map<Key, ControllerId> live_;        // running compositor animations by (target, path)
    std::map<Key, AppEntry> app_;             // app-thread properties by (target, property)
};

}  // namespace motion
