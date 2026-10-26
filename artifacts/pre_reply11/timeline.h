// timeline.h: clocks, keyframe tracks, composition nodes, playback. Module 3.
// Header-only C++17, no OS dependencies (std::chrono only in SteadyClock).
// Units: seconds (double) and DIPs/arbitrary value units.
//
// Threading: nothing here is thread-safe. A Timeline is immutable after
// construction and may be shared; a Player is owned by one thread (the UI
// dispatcher per the project's single threading rule).
#pragma once
#include <algorithm>
#include <chrono>
#include <cmath>
#include <cstdint>
#include <functional>
#include <map>
#include <memory>
#include <optional>
#include <string>
#include <string_view>
#include <utility>
#include <vector>

#include "bezier.h"
#include "spring.h"

namespace motion {

// ---------------------------------------------------------------- clocks ----

struct IClock {
    virtual ~IClock() = default;
    virtual double Now() const = 0;  // seconds, monotonic non-decreasing
};

class SteadyClock final : public IClock {
public:
    SteadyClock() : origin_(std::chrono::steady_clock::now()) {}
    double Now() const override {
        return std::chrono::duration<double>(std::chrono::steady_clock::now() - origin_).count();
    }
private:
    std::chrono::steady_clock::time_point origin_;
};

// Tests: you decide exactly when and how far time moves.
class ManualClock final : public IClock {
public:
    double Now() const override { return t_; }
    void Set(double t) { t_ = t; }
    void Advance(double dt) { if (dt > 0.0) t_ += dt; }
private:
    double t_ = 0.0;
};

// Tests/replay: Now() = frame * step, so there is no accumulated rounding drift.
class FixedStepClock final : public IClock {
public:
    explicit FixedStepClock(double step = 1.0 / 120.0) : step_(step) {}
    double Now() const override { return static_cast<double>(frame_) * step_; }
    void Tick(std::int64_t n = 1) { if (n > 0) frame_ += n; }
    std::int64_t Frame() const { return frame_; }
    double Step() const { return step_; }
private:
    double step_;
    std::int64_t frame_ = 0;
};

// ---------------------------------------------------------------- curves ----

// How one segment moves from its start value to its end value.
struct Curve {
    enum class Kind { Linear, Bezier, Spring };
    Kind kind = Kind::Linear;
    CubicBezier bezier{0.0, 0.0, 1.0, 1.0};
    SpringParams spring{};

    static Curve Linear() { return Curve{}; }
    static Curve Bezier(double x1, double y1, double x2, double y2) {
        Curve c; c.kind = Kind::Bezier;
        c.bezier = CubicBezier(std::clamp(x1, 0.0, 1.0), y1, std::clamp(x2, 0.0, 1.0), y2);
        return c;
    }
    static Curve Bezier(const CubicBezier& b) { Curve c; c.kind = Kind::Bezier; c.bezier = b; return c; }
    static Curve FromSpring(SpringParams p) { Curve c; c.kind = Kind::Spring; c.spring = p; return c; }
    // nullopt if the token is unknown (the token loader reports it).
    static std::optional<Curve> FromToken(std::string_view name) {
        if (const CubicBezier* b = FindEasing(name)) return Bezier(*b);
        return std::nullopt;
    }
};

struct Sample {
    double value = 0.0;
    double velocity = 0.0;   // d(value)/d(time) in the units of the time it was evaluated in
};

// Settle thresholds used to size spring segments (DIPs and DIPs/s).
inline constexpr double kSpringPosEps = 0.1;
inline constexpr double kSpringVelEps = 0.5;

// ---------------------------------------------------------------- tracks ----

// Scalar keyframe track for ONE property. Before t=0 it holds the initial
// value; after its end it holds the final value with zero velocity.
class Track {
public:
    struct Segment {
        double start, duration, from, to, entryVelocity;
        Curve curve;
    };

    Track(std::string property, double from)
        : property_(std::move(property)), initial_(from), last_(from) {}

    // Linear/Bezier segment of fixed duration. A zero duration is an instant jump.
    // A Spring curve here is redirected to SpringTo (duration is derived).
    Track& To(double value, double duration, Curve curve = Curve::Linear()) {
        if (curve.kind == Curve::Kind::Spring) return SpringTo(value, curve.spring);
        if (!(duration > 0.0)) duration = 0.0;
        Push(duration, value, 0.0, curve);
        return *this;
    }

    // Spring segment. Duration = analytic SettleTime. With inheritVelocity the
    // spring starts with the previous segment's exit velocity. At the end the
    // value snaps to `value` (error < kSpringPosEps by construction).
    Track& SpringTo(double value, SpringParams p, bool inheritVelocity = false) {
        const double v0 = inheritVelocity ? EndVelocity() : 0.0;
        const Spring s(p, last_, value, v0);
        Push(s.SettleTime(kSpringPosEps, kSpringVelEps), value, v0, Curve::FromSpring(p));
        return *this;
    }

    Track& Hold(double duration) { return To(last_, duration); }

    const std::string& Property() const { return property_; }
    double Duration() const { return end_; }
    double InitialValue() const { return initial_; }
    double FinalValue() const { return last_; }
    const std::vector<Segment>& Segments() const { return segs_; }

    // Pure. t is local time in seconds.
    Sample Eval(double t) const {
        if (!(t >= 0.0)) return {initial_, 0.0};            // negative or NaN
        auto it = std::upper_bound(segs_.begin(), segs_.end(), t,
            [](double tv, const Segment& s) { return tv < s.start + s.duration; });
        if (it == segs_.end()) return {last_, 0.0};
        return EvalSegment(*it, t - it->start);
    }

    double EndVelocity() const {
        return segs_.empty() ? 0.0 : EvalSegment(segs_.back(), segs_.back().duration).velocity;
    }

private:
    void Push(double duration, double to, double entryV, const Curve& c) {
        segs_.push_back(Segment{end_, duration, last_, to, entryV, c});
        end_ += duration;
        last_ = to;
    }

    static Sample EvalSegment(const Segment& s, double local) {
        if (!(s.duration > 0.0)) return {s.to, 0.0};
        const double span = s.to - s.from;
        switch (s.curve.kind) {
            case Curve::Kind::Linear:
                return {s.from + span * (local / s.duration), span / s.duration};
            case Curve::Kind::Bezier: {
                const double x = local / s.duration;
                return {s.from + span * s.curve.bezier.Ease(x),
                        span * s.curve.bezier.Slope(x) / s.duration};
            }
            case Curve::Kind::Spring: {
                const SpringState st =
                    Spring(s.curve.spring, s.from, s.to, s.entryVelocity).At(local);
                return {st.value, st.velocity};
            }
        }
        return {s.to, 0.0};
    }

    std::string property_;
    double initial_, last_, end_ = 0.0;
    std::vector<Segment> segs_;
};

// ----------------------------------------------------------------- nodes ----

using SampleMap = std::map<std::string, Sample>;   // ordered: deterministic iteration

struct MarkerEvent { std::string name; double time; };
struct PlacedTrack { double start; const Track* track; };  // pointer valid while Timeline lives

// Immutable composition node. Evaluate() is pure.
//
// Conflict rule when several tracks drive one property (also what a compositor
// lowering must reproduce): a child that has started (t >= its start) overwrites
// earlier children and then holds its end value; a child that has NOT started
// only supplies its initial value if no earlier child has set the property.
class Node {
public:
    virtual ~Node() = default;
    virtual double Duration() const = 0;
    virtual void Evaluate(double t, SampleMap& out) const = 0;
    virtual void CollectMarkers(double /*offset*/, std::vector<MarkerEvent>&) const {}
    virtual void CollectTracks(double /*offset*/, std::vector<PlacedTrack>&) const {}
};
using NodePtr = std::shared_ptr<const Node>;

class TrackNode final : public Node {
public:
    explicit TrackNode(Track t) : track_(std::move(t)) {}
    double Duration() const override { return track_.Duration(); }
    void Evaluate(double t, SampleMap& out) const override { out[track_.Property()] = track_.Eval(t); }
    void CollectTracks(double offset, std::vector<PlacedTrack>& out) const override {
        out.push_back({offset, &track_});
    }
private:
    Track track_;
};

class DelayNode final : public Node {
public:
    explicit DelayNode(double d) : d_(d > 0.0 ? d : 0.0) {}
    double Duration() const override { return d_; }
    void Evaluate(double, SampleMap&) const override {}
private:
    double d_;
};

class MarkerNode final : public Node {   // zero duration; fires at its position
public:
    explicit MarkerNode(std::string n) : name_(std::move(n)) {}
    double Duration() const override { return 0.0; }
    void Evaluate(double, SampleMap&) const override {}
    void CollectMarkers(double offset, std::vector<MarkerEvent>& out) const override {
        out.push_back({name_, offset});
    }
private:
    std::string name_;
};

class GroupNode final : public Node {
public:
    enum class Kind { Sequence, Parallel, Stagger };

    GroupNode(Kind kind, std::vector<NodePtr> children, double stagger = 0.0) {
        double cursor = 0.0;
        for (auto& c : children) {
            if (!c) continue;
            const double start = kind == Kind::Sequence ? cursor
                               : kind == Kind::Parallel ? 0.0
                               : static_cast<double>(children_.size()) * std::max(stagger, 0.0);
            children_.push_back(std::move(c));
            starts_.push_back(start);
            const double d = children_.back()->Duration();
            durs_.push_back(d);
            duration_ = std::max(duration_, start + d);
            cursor += d;
        }
    }

    double Duration() const override { return duration_; }

    void Evaluate(double t, SampleMap& out) const override {
        for (std::size_t i = 0; i < children_.size(); ++i) {
            if (t >= starts_[i]) {
                // Once the child has finished, pass at least its exact duration so
                // float roundoff in (t - start) can never leave it just short of
                // its end (a spring would otherwise show up to kSpringPosEps of
                // residual error at Duration()). start + dur is the same
                // expression used for duration_, so t == Duration() hits this.
                const bool done = t >= starts_[i] + durs_[i];
                children_[i]->Evaluate(done ? std::max(t - starts_[i], durs_[i]) : t - starts_[i], out);
            } else {
                SampleMap initial;                       // negative time = initial values
                children_[i]->Evaluate(-1.0, initial);
                out.insert(initial.begin(), initial.end());   // insert never overwrites
            }
        }
    }
    void CollectMarkers(double offset, std::vector<MarkerEvent>& out) const override {
        for (std::size_t i = 0; i < children_.size(); ++i)
            children_[i]->CollectMarkers(offset + starts_[i], out);
    }
    void CollectTracks(double offset, std::vector<PlacedTrack>& out) const override {
        for (std::size_t i = 0; i < children_.size(); ++i)
            children_[i]->CollectTracks(offset + starts_[i], out);
    }
private:
    std::vector<NodePtr> children_;
    std::vector<double> starts_, durs_;
    double duration_ = 0.0;
};

inline NodePtr Animate(Track t)                 { return std::make_shared<TrackNode>(std::move(t)); }
inline NodePtr Delay(double seconds)            { return std::make_shared<DelayNode>(seconds); }
inline NodePtr Marker(std::string name)         { return std::make_shared<MarkerNode>(std::move(name)); }
inline NodePtr Sequence(std::vector<NodePtr> c) { return std::make_shared<GroupNode>(GroupNode::Kind::Sequence, std::move(c)); }
inline NodePtr Parallel(std::vector<NodePtr> c) { return std::make_shared<GroupNode>(GroupNode::Kind::Parallel, std::move(c)); }
inline NodePtr Stagger(double delayPerChild, std::vector<NodePtr> c) {
    return std::make_shared<GroupNode>(GroupNode::Kind::Stagger, std::move(c), delayPerChild);
}

// -------------------------------------------------------------- timeline ----

class Timeline {
public:
    explicit Timeline(NodePtr root) : root_(root ? std::move(root) : Delay(0.0)) {
        duration_ = root_->Duration();
        root_->CollectMarkers(0.0, markers_);
        std::stable_sort(markers_.begin(), markers_.end(),
            [](const MarkerEvent& a, const MarkerEvent& b) { return a.time < b.time; });
        root_->CollectTracks(0.0, tracks_);
    }

    double Duration() const { return duration_; }
    const NodePtr& Root() const { return root_; }
    const std::vector<MarkerEvent>& Markers() const { return markers_; }      // sorted by time
    const std::vector<PlacedTrack>& Tracks() const { return tracks_; }        // for lowering
    std::optional<double> MarkerTime(std::string_view name) const {
        for (const auto& m : markers_) if (m.name == name) return m.time;
        return std::nullopt;
    }

    // Pure and side-effect free. t is clamped to [0, Duration()]; NaN -> 0.
    // Velocity is per timeline-second.
    SampleMap Evaluate(double t) const {
        if (!(t > 0.0)) t = 0.0;
        if (t > duration_) t = duration_;
        SampleMap m;
        root_->Evaluate(t, m);
        return m;
    }

private:
    NodePtr root_;
    double duration_ = 0.0;
    std::vector<MarkerEvent> markers_;
    std::vector<PlacedTrack> tracks_;
};

// ---------------------------------------------------------------- player ----

class Player {
public:
    using CompleteCallback = std::function<void()>;
    using MarkerCallback = std::function<void(const std::string& name, double timelineTime)>;

    Player(std::shared_ptr<const Timeline> tl, const IClock& clock)
        : tl_(std::move(tl)), clock_(&clock), lastNow_(clock.Now()) {}

    void OnComplete(CompleteCallback cb) { onComplete_ = std::move(cb); }
    void OnMarker(MarkerCallback cb) { onMarker_ = std::move(cb); }

    void Play() {
        Update();
        if (finished_ || AtEndEdge()) {            // restart from the start edge
            pos_ = reversed_ ? tl_->Duration() : 0.0;
            loopsDone_ = 0;
        }
        finished_ = false;
        playing_ = true;
        inclusiveFrom_ = AtStartEdge();
    }
    void Pause() { Update(); playing_ = false; }

    // Jump to timeline time t. Fires no callbacks; Seek(t) twice == once.
    void Seek(double t) {
        Update();
        if (!(t > 0.0)) t = 0.0;
        pos_ = std::min(t, tl_->Duration());
        finished_ = false;
        inclusiveFrom_ = false;
    }

    // Flip direction and play (like Web Animations reverse()).
    void Reverse() { Update(); reversed_ = !reversed_; Play(); }

    void SetRate(double r) { Update(); if (r > 0.0 && std::isfinite(r)) rate_ = r; }
    void SetLoops(int n) { loops_ = n == 0 ? 1 : n; }   // n < 0: infinite

    // Advance to the clock's current time. Call once per frame/tick.
    void Update() {
        const double now = clock_->Now();
        const double dt = now - lastNow_;
        lastNow_ = now;
        if (playing_ && dt > 0.0) Advance(dt * rate_);
    }

    bool IsPlaying() const { return playing_; }
    bool IsFinished() const { return finished_; }
    bool IsReversed() const { return reversed_; }
    double Position() const { return pos_; }
    double Rate() const { return rate_; }

    // Values as of the last Update(). Velocity is per REAL second (timeline
    // velocity x rate x direction; zero when not playing). Overrides win.
    SampleMap Sample() const {
        SampleMap m = tl_->Evaluate(pos_);
        const double speed = playing_ ? rate_ * (reversed_ ? -1.0 : 1.0) : 0.0;
        for (auto& kv : m) kv.second.velocity *= speed;
        for (const auto& kv : overrides_) {
            const SpringState s = kv.second.spring.At(std::max(0.0, lastNow_ - kv.second.start));
            m[kv.first] = {s.value, s.velocity};
        }
        return m;
    }

    // Interruption: take over `property` with a spring toward `target`, starting
    // from its current value and current real-time velocity (bezier segments via
    // CubicBezier::Slope, springs via their analytic state, overrides via their
    // own state). The override holds `target` until released.
    // If the property is neither animated nor overridden, `from` seeds the value
    // (velocity 0); without `from` the call returns false.
    bool Retarget(const std::string& property, double target, SpringParams params,
                  std::optional<double> from = std::nullopt) {
        Update();
        const SampleMap m = Sample();
        double cur, vel = 0.0;
        auto it = m.find(property);
        if (it != m.end()) { cur = it->second.value; vel = it->second.velocity; }
        else if (from)     { cur = *from; }
        else               { return false; }
        overrides_.insert_or_assign(property, Override{Spring(params, cur, target, vel), lastNow_});
        return true;
    }
    void ReleaseOverride(const std::string& property) { overrides_.erase(property); }
    void ReleaseAllOverrides() { overrides_.clear(); }

private:
    struct Override { Spring spring; double start; };

    bool AtStartEdge() const { return reversed_ ? pos_ >= tl_->Duration() : pos_ <= 0.0; }
    bool AtEndEdge() const { return reversed_ ? pos_ <= 0.0 : pos_ >= tl_->Duration(); }

    // Markers crossed moving from `from` to `to` in the current direction.
    // Forward: (from, to]; reverse: [to, from). The `from` side is inclusive
    // right after Play() at the start edge or a loop wrap, so a marker at the
    // very start fires once per pass.
    void FireMarkers(double from, double to) {
        const auto& ms = tl_->Markers();
        const bool inc = inclusiveFrom_;
        inclusiveFrom_ = false;
        if (!onMarker_) return;
        if (!reversed_) {
            for (std::size_t i = 0; i < ms.size(); ++i) {
                const double t = ms[i].time;
                if ((inc ? t >= from : t > from) && t <= to) onMarker_(ms[i].name, t);
            }
        } else {
            for (std::size_t i = ms.size(); i-- > 0;) {
                const double t = ms[i].time;
                if ((inc ? t <= from : t < from) && t >= to) onMarker_(ms[i].name, t);
            }
        }
    }

    void Advance(double d) {   // d > 0 timeline seconds, in the current direction
        const double dur = tl_->Duration();
        const bool fwd = !reversed_;
        while (playing_) {
            if (dur <= 0.0) { FireMarkers(0.0, 0.0); Finish(); return; }
            const double edge = fwd ? dur : 0.0;
            const double dist = fwd ? dur - pos_ : pos_;
            if (d < dist) {
                const double next = fwd ? pos_ + d : pos_ - d;
                const double from = pos_;
                pos_ = next;
                FireMarkers(from, next);
                return;
            }
            const double from = pos_;
            pos_ = edge;
            d -= dist;
            FireMarkers(from, edge);
            if (!playing_) return;                         // a callback paused us
            ++loopsDone_;
            if (loops_ >= 0 && loopsDone_ >= loops_) { Finish(); return; }
            pos_ = fwd ? 0.0 : dur;                        // wrap
            inclusiveFrom_ = true;
        }
    }

    void Finish() {
        playing_ = false;
        finished_ = true;
        if (onComplete_) onComplete_();
    }

    std::shared_ptr<const Timeline> tl_;
    const IClock* clock_;
    double lastNow_;
    double pos_ = 0.0;
    double rate_ = 1.0;
    int loops_ = 1;
    int loopsDone_ = 0;
    bool playing_ = false, finished_ = false, reversed_ = false, inclusiveFrom_ = false;
    CompleteCallback onComplete_;
    MarkerCallback onMarker_;
    std::map<std::string, Override> overrides_;
};

} // namespace motion
