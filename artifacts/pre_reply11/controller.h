// controller.h: interruptible controllers, handles, property ownership, and the
// dispatcher seam. Module 5. Header-only C++17, standard library only.
// Depends on timeline.h and tokens.h (and through them spring.h / bezier.h).
//
// MODEL
//   MotionSystem   owns the clock reference, the reduced-motion switch, the
//                  property-ownership table and the set of running controllers.
//                  The app calls Tick() once per frame/tick.
//   AnimationHandle  cheap, copyable (one shared_ptr) face of a controller.
//   IDispatcher    the seam module 6 plugs into (see below).
//
// OWNERSHIP RULE (last wins, per (target, property))
//   A controller claims a property when it Plays/Seeks/Reverses a timeline that
//   animates it, or when it Retargets it. The previous owner loses that property
//   immediately and keeps running for the properties it still owns. A controller
//   that was in flight (playing, paused mid-way, or springs unsettled) and ends up
//   owning nothing becomes Superseded (terminal, EndReason::Superseded). An idle or
//   finished controller that loses properties stays reusable; Play() reclaims them.
//   Velocity handoff: Retarget() on a property owned by another controller starts
//   from that controller's current value AND velocity. Play() starts from the
//   timeline's literal values (discontinuous by design).
//
// LIFETIME
//   Active controllers are kept alive by the system, so dropping every handle
//   without Cancel() is safe: the animation runs out, then the controller is freed.
//   Its final values are kept as "rest values" (MotionSystem::Current). Terminal
//   transitions clear user callbacks (breaks handle-capturing-lambda cycles).
//   Callbacks on a controller that merely FINISHED are kept (it can replay); a
//   callback that captures its own handle by value forms a cycle there, so capture
//   weakly or Cancel() when done. Handles that outlive the MotionSystem become
//   inert (State() == Cancelled, calls are no-ops).
//
// REDUCED MOTION (MotionSystem::SetReducedMotion)
//   Play/Reverse/Seek use the reduced timeline (from TokenSet, or an instant one
//   derived from the full timeline: every animated property jumps to its end value).
//   Any zero-duration timeline completes immediately at Play (markers are skipped).
//   Retarget() jumps to the target with zero velocity (no spring). Turning reduced
//   motion ON mid-flight finishes every active controller at its end state.
//
// THREADING: single dispatcher thread only. Nothing here is thread-safe.
#pragma once
#include <cmath>
#include <cstdint>
#include <functional>
#include <map>
#include <memory>
#include <optional>
#include <set>
#include <string>
#include <string_view>
#include <utility>
#include <vector>

#include "timeline.h"
#include "tokens.h"

namespace motion {

using TargetId = std::uint64_t;      // opaque id of the object being animated
using ControllerId = std::uint64_t;

enum class AnimationState { Idle, Playing, Paused, Finished, Cancelled, Superseded };
enum class EndReason { Completed, Cancelled, Superseded };

// ---------------------------------------------------------- dispatcher seam ----

struct SpringOverrideInfo {
    std::string property;
    SpringParams params;
    double from = 0, fromVelocity = 0, target = 0;
    double startClock = 0;               // IClock time at which the spring started
};

// Everything a backend needs to (re)lower a controller to compositor animations.
// Submitted at every discontinuity: play, pause, seek, reverse, rate/loop change,
// retarget, ownership change, reduced-motion snap.
struct ControllerSnapshot {
    ControllerId id = 0;
    TargetId target = 0;
    std::shared_ptr<const Timeline> timeline;     // the variant in use (full or reduced)
    double position = 0;                          // timeline seconds at clockTime
    double rate = 1;
    bool reversed = false, playing = false;
    int loops = 1;                                // < 0: infinite
    double clockTime = 0;
    std::vector<SpringOverrideInfo> overrides;
    std::vector<std::string> ownedProperties;     // only these may be applied for this controller
};

class IDispatcher {
public:
    virtual ~IDispatcher() = default;
    // New or changed motion description (lowering opportunity).
    virtual void Submit(const ControllerSnapshot&) = 0;
    // Release anything lowered for this controller (finished, cancelled, superseded).
    virtual void Withdraw(ControllerId, EndReason) = 0;
    // App-thread values, once per Tick, for the properties each controller owns.
    // A backend that lowered a property to the compositor may ignore it here.
    virtual void Apply(TargetId, const SampleMap&) = 0;
};

class NullDispatcher final : public IDispatcher {
public:
    void Submit(const ControllerSnapshot&) override {}
    void Withdraw(ControllerId, EndReason) override {}
    void Apply(TargetId, const SampleMap&) override {}
};

// Every animated property jumps to its final value at t = 0 (same rule as the
// token loader's "instant"; kept separate so tokens.h is unchanged).
inline std::shared_ptr<const Timeline> MakeInstantTimeline(const Timeline& tl) {
    std::vector<NodePtr> kids;
    for (const PlacedTrack& pt : tl.Tracks())
        kids.push_back(Animate(Track(pt.track->Property(), pt.track->InitialValue()).To(pt.track->FinalValue(), 0.0)));
    return std::make_shared<const Timeline>(Parallel(std::move(kids)));
}

class MotionSystem;
namespace detail { class Controller; }

// ------------------------------------------------------------ the handle ----

class AnimationHandle;

namespace detail {

struct SpringOverride {
    SpringParams params;
    Spring spring;
    double from, fromVelocity, target, start, settle;
    SpringOverride(SpringParams p, double f, double v, double t, double s)
        : params(p), spring(p, f, t, v), from(f), fromVelocity(v), target(t), start(s),
          settle(spring.SettleTime(kSpringPosEps, kSpringVelEps)) {}
    static SpringOverride Held(double target, double start) {
        SpringOverride o(SpringParams{}, target, 0.0, target, start);
        o.settle = 0.0;
        return o;
    }
    Sample Eval(double now) const {
        double el = now - start;
        if (el < 0.0) el = 0.0;
        if (el >= settle) return {target, 0.0};   // snaps to target, like a track
        const SpringState st = spring.At(el);
        return {st.value, st.velocity};
    }
};

class Controller : public std::enable_shared_from_this<Controller> {
public:
    using EndCallback = std::function<void(EndReason)>;

    Controller(MotionSystem* sys, ControllerId id, TargetId target,
               std::shared_ptr<const Timeline> full, std::shared_ptr<const Timeline> reduced);
    ~Controller();
    Controller(const Controller&) = delete;
    Controller& operator=(const Controller&) = delete;

    ControllerId Id() const { return id_; }
    TargetId Target() const { return target_; }
    AnimationState State() const { return state_; }
    bool Terminal() const { return state_ == AnimationState::Cancelled || state_ == AnimationState::Superseded; }

    // user operations
    void Play();
    void Pause();
    void Seek(double t);
    void Reverse();
    void SetRate(double r);
    void SetLoops(int n);
    void Cancel();
    bool Retarget(const std::string& property, double target, SpringParams p, std::optional<double> from);
    void Release(const std::string& property);

    bool Owns(const std::string& p) const { return owned_.count(p) != 0; }
    std::optional<Sample> ValueOf(const std::string& property) const;
    SampleMap Values() const { SampleMap m; CollectOwned(m); return m; }
    double Position() const { return player_ ? player_->Position() : 0.0; }
    bool IsActive() const { return NeedsTick(); }
    void SetOnEnd(EndCallback cb) { if (!Terminal()) onEnd_ = std::move(cb); }
    void SetOnMarker(Player::MarkerCallback cb) { if (!Terminal()) onMarker_ = std::move(cb); }

    // system-facing
    void Advance();
    bool NeedsTick() const;
    void CollectOwned(SampleMap& out) const;
    void OnLost(const std::string& property);
    void SnapToEnd();
    void OnDeactivated();
    void DetachSystem();

    bool dirty = false;       // needs one more Apply even if nothing is animating
    bool inActive = false;    // present in MotionSystem::active_

private:
    bool WantReduced() const;
    bool InFlight() const { return state_ == AnimationState::Playing || state_ == AnimationState::Paused; }
    bool HasUnsettledOverrides() const;
    void EnsurePlayer();
    void BuildPlayer(bool reduced);
    std::shared_ptr<const Timeline> ReducedTimeline();
    void ClaimGraph();
    void ReleaseProp(const std::string& p);
    void AfterStart();
    void OnPlayerComplete();
    void FireEnd(EndReason r);
    void Terminate(EndReason r);
    void Submit();
    ControllerSnapshot MakeSnapshot() const;

    MotionSystem* sys_;
    ControllerId id_;
    TargetId target_;
    std::shared_ptr<const Timeline> full_, reduced_, current_, instant_;
    double now_;
    std::unique_ptr<Player> player_;
    bool playerReduced_ = false, submitted_ = false;
    AnimationState state_ = AnimationState::Idle;
    double rate_ = 1.0;
    int loops_ = 1;
    std::set<std::string> graphProps_, owned_;
    std::map<std::string, SpringOverride> overrides_;
    EndCallback onEnd_;
    Player::MarkerCallback onMarker_;
};

}  // namespace detail

class AnimationHandle {
public:
    AnimationHandle() = default;                        // null handle: inert
    explicit operator bool() const { return static_cast<bool>(c_); }
    bool SameAs(const AnimationHandle& o) const { return c_ == o.c_; }
    ControllerId Id() const { return c_ ? c_->Id() : 0; }

    void Play()                { if (c_) c_->Play(); }
    void Pause()               { if (c_) c_->Pause(); }
    void Seek(double t)        { if (c_) c_->Seek(t); }
    void Reverse()             { if (c_) c_->Reverse(); }
    void SetRate(double r)     { if (c_) c_->SetRate(r); }
    void SetLoops(int n)       { if (c_) c_->SetLoops(n); }   // n < 0: infinite
    void Cancel()              { if (c_) c_->Cancel(); }

    // Velocity-preserving interruption: spring from the property's current value
    // and velocity (whoever drives it) to `newTarget`. `from` seeds a property that
    // nobody drives and has no rest value. Returns false if the handle is dead or
    // no starting value is known. Under reduced motion it jumps (no spring).
    bool Retarget(const std::string& property, double newTarget, SpringParams spring,
                  std::optional<double> from = std::nullopt) {
        return c_ && c_->Retarget(property, newTarget, spring, from);
    }
    // Drop an override / give a property back (graph properties resume the graph).
    void Release(const std::string& property) { if (c_) c_->Release(property); }

    AnimationState State() const { return c_ ? c_->State() : AnimationState::Cancelled; }
    bool IsActive() const { return c_ && c_->IsActive(); }
    double Position() const { return c_ ? c_->Position() : 0.0; }
    bool Owns(const std::string& property) const { return c_ && c_->Owns(property); }
    std::optional<Sample> Value(const std::string& property) const {
        return c_ ? c_->ValueOf(property) : std::nullopt;
    }
    SampleMap Values() const { return c_ ? c_->Values() : SampleMap{}; }   // owned properties only

    // Callbacks live on the controller (shared by all copies of the handle).
    void OnEnd(std::function<void(EndReason)> cb) { if (c_) c_->SetOnEnd(std::move(cb)); }
    void OnMarker(Player::MarkerCallback cb)      { if (c_) c_->SetOnMarker(std::move(cb)); }

private:
    friend class MotionSystem;
    explicit AnimationHandle(std::shared_ptr<detail::Controller> c) : c_(std::move(c)) {}
    std::shared_ptr<detail::Controller> c_;
};

// ------------------------------------------------------------ the system ----

class MotionSystem {
public:
    explicit MotionSystem(const IClock& clock, IDispatcher* dispatcher = nullptr)
        : clock_(&clock), dispatcher_(dispatcher ? dispatcher : &null_) {}
    ~MotionSystem() { for (detail::Controller* c : all_) c->DetachSystem(); }
    MotionSystem(const MotionSystem&) = delete;
    MotionSystem& operator=(const MotionSystem&) = delete;

    bool ReducedMotion() const { return reduced_; }
    void SetReducedMotion(bool on) {
        if (on == reduced_) return;
        reduced_ = on;
        if (!on) return;
        auto batch = active_;
        for (auto& c : batch) c->SnapToEnd();
    }

    // Empty/absent timelines are allowed (a controller used only for Retarget).
    AnimationHandle Create(TargetId target, std::shared_ptr<const Timeline> full = nullptr,
                           std::shared_ptr<const Timeline> reduced = nullptr) {
        return AnimationHandle(std::make_shared<detail::Controller>(this, ++nextId_, target, std::move(full), std::move(reduced)));
    }
    // Null handle if the transition does not exist.
    AnimationHandle Create(TargetId target, const TokenSet& tokens, std::string_view transition) {
        auto full = tokens.BuildTimeline(transition, false);
        if (!full) return AnimationHandle();
        return Create(target, std::move(full), tokens.BuildTimeline(transition, true));
    }

    // Advance every active controller to the clock's time and push owned values
    // to the dispatcher. Call once per frame/tick.
    void Tick() {
        auto batch = active_;
        for (auto& c : batch) c->Advance();
        std::map<TargetId, SampleMap> out;
        for (auto& c : batch) {
            if (c->Terminal()) continue;
            c->CollectOwned(out[c->Target()]);
            c->dirty = false;
        }
        for (auto& kv : out) if (!kv.second.empty()) dispatcher_->Apply(kv.first, kv.second);
        std::vector<std::shared_ptr<detail::Controller>> keep, dropped;
        for (auto& c : active_) {
            if (c->Terminal()) { c->inActive = false; continue; }
            if (c->NeedsTick() || c->dirty) keep.push_back(c);
            else { c->inActive = false; dropped.push_back(c); }
        }
        active_.swap(keep);
        for (auto& c : dropped) c->OnDeactivated();
    }

    // Current value: the owner's live value, else the last rest value.
    std::optional<Sample> Current(TargetId target, std::string_view property) const {
        const Key k{target, std::string(property)};
        auto o = owners_.find(k);
        if (o != owners_.end()) return o->second->ValueOf(k.second);
        auto r = rest_.find(k);
        if (r != rest_.end()) return Sample{r->second, 0.0};
        return std::nullopt;
    }
    std::size_t ActiveCount() const { return active_.size(); }

    // ---- used by detail::Controller; not part of the app-facing API ----
    const IClock& Clock() const { return *clock_; }
    bool Reduced() const { return reduced_; }
    IDispatcher& Dispatcher() { return *dispatcher_; }
    void Register(detail::Controller* c) { all_.insert(c); }
    void Unregister(detail::Controller* c) { all_.erase(c); }
    void Activate(detail::Controller* c) {
        if (!c->inActive) { c->inActive = true; active_.push_back(c->shared_from_this()); }
    }
    void Claim(detail::Controller* c, const std::string& prop) {
        const Key k{c->Target(), prop};
        auto it = owners_.find(k);
        if (it == owners_.end()) { owners_.emplace(k, c); return; }
        if (it->second == c) return;
        detail::Controller* prev = it->second;
        it->second = c;
        prev->OnLost(prop);                 // may fire callbacks; `it` is not used afterwards
    }
    void ReleaseOwnership(detail::Controller* c, const std::string& prop, double restValue) {
        const Key k{c->Target(), prop};
        auto it = owners_.find(k);
        if (it != owners_.end() && it->second == c) { owners_.erase(it); rest_[k] = restValue; }
    }
    detail::Controller* OwnerOf(TargetId t, const std::string& prop) const {
        auto it = owners_.find(Key{t, prop});
        return it == owners_.end() ? nullptr : it->second;
    }
    std::optional<double> RestValue(TargetId t, const std::string& prop) const {
        auto it = rest_.find(Key{t, prop});
        if (it == rest_.end()) return std::nullopt;
        return it->second;
    }

private:
    using Key = std::pair<TargetId, std::string>;
    const IClock* clock_;
    NullDispatcher null_;
    IDispatcher* dispatcher_;
    bool reduced_ = false;
    ControllerId nextId_ = 0;
    std::vector<std::shared_ptr<detail::Controller>> active_;
    std::set<detail::Controller*> all_;
    std::map<Key, detail::Controller*> owners_;
    std::map<Key, double> rest_;
};

// ------------------------------------------------- Controller definitions ----

namespace detail {

inline Controller::Controller(MotionSystem* sys, ControllerId id, TargetId target,
                              std::shared_ptr<const Timeline> full, std::shared_ptr<const Timeline> reduced)
    : sys_(sys), id_(id), target_(target),
      full_(full ? std::move(full) : std::make_shared<const Timeline>(Delay(0.0))),
      reduced_(std::move(reduced)), current_(full_), now_(sys->Clock().Now()) {
    sys_->Register(this);
}

inline Controller::~Controller() {
    if (!sys_) return;
    const std::vector<std::string> owned(owned_.begin(), owned_.end());
    for (const auto& p : owned) ReleaseProp(p);       // persist rest values, drop ownership
    sys_->Unregister(this);
}

inline void Controller::DetachSystem() {
    sys_ = nullptr;
    if (!Terminal()) state_ = AnimationState::Cancelled;
    onEnd_ = nullptr; onMarker_ = nullptr;
    if (player_) { player_->OnComplete(nullptr); player_->OnMarker(nullptr); }
}

inline bool Controller::WantReduced() const { return sys_ && sys_->Reduced(); }

inline bool Controller::HasUnsettledOverrides() const {
    for (const auto& kv : overrides_) if (now_ - kv.second.start < kv.second.settle) return true;
    return false;
}

inline bool Controller::NeedsTick() const {
    return !Terminal() && (state_ == AnimationState::Playing || HasUnsettledOverrides());
}

inline std::shared_ptr<const Timeline> Controller::ReducedTimeline() {
    if (reduced_) return reduced_;
    if (!instant_) instant_ = MakeInstantTimeline(*full_);
    return instant_;
}

inline void Controller::BuildPlayer(bool reduced) {
    if (player_) {   // properties only the old variant animated are given back
        const std::vector<std::string> old(graphProps_.begin(), graphProps_.end());
        for (const auto& p : old) if (!overrides_.count(p)) ReleaseProp(p);
    }
    current_ = reduced ? ReducedTimeline() : full_;
    player_ = std::make_unique<Player>(current_, sys_->Clock());
    player_->SetRate(rate_);
    player_->SetLoops(loops_);
    player_->OnComplete([this] { OnPlayerComplete(); });
    player_->OnMarker([this](const std::string& n, double t) {
        if (onMarker_) { auto cb = onMarker_; cb(n, t); }
    });
    playerReduced_ = reduced;
    graphProps_.clear();
    for (const PlacedTrack& pt : current_->Tracks()) graphProps_.insert(pt.track->Property());
}

// Build the player if missing, or switch variant when the reduced-motion mode
// changed and the controller is not mid-flight. (Rebuilding resets direction to forward.)
inline void Controller::EnsurePlayer() {
    const bool want = WantReduced();
    if (!player_ || (playerReduced_ != want && !InFlight())) BuildPlayer(want);
}

inline void Controller::ClaimGraph() {
    for (const auto& p : graphProps_)
        if (!owned_.count(p)) { owned_.insert(p); sys_->Claim(this, p); }
}

inline void Controller::ReleaseProp(const std::string& p) {
    if (!owned_.count(p)) return;
    const std::optional<Sample> v = ValueOf(p);
    owned_.erase(p);
    if (sys_) sys_->ReleaseOwnership(this, p, v ? v->value : 0.0);
}

inline ControllerSnapshot Controller::MakeSnapshot() const {
    ControllerSnapshot s;
    s.id = id_; s.target = target_; s.timeline = current_; s.clockTime = now_; s.loops = loops_; s.rate = rate_;
    if (player_) { s.position = player_->Position(); s.reversed = player_->IsReversed(); s.playing = player_->IsPlaying(); }
    for (const auto& kv : overrides_) {
        const SpringOverride& o = kv.second;
        s.overrides.push_back({kv.first, o.params, o.from, o.fromVelocity, o.target, o.start});
    }
    s.ownedProperties.assign(owned_.begin(), owned_.end());
    return s;
}

inline void Controller::Submit() {
    if (!sys_) return;
    submitted_ = true;
    sys_->Dispatcher().Submit(MakeSnapshot());
}

inline void Controller::Advance() {
    if (Terminal() || !sys_) return;
    now_ = sys_->Clock().Now();
    if (player_) player_->Update();
}

inline void Controller::FireEnd(EndReason r) {
    if (!onEnd_) return;
    auto cb = onEnd_;            // copy: the callback may replace or clear it
    cb(r);
}

inline void Controller::OnPlayerComplete() {
    state_ = AnimationState::Finished;
    dirty = true;
    FireEnd(EndReason::Completed);
}

// Common tail of Play/Reverse.
inline void Controller::AfterStart() {
    state_ = AnimationState::Playing;
    if (current_->Duration() <= 0.0) {      // nothing to animate over time: complete now
        player_->Pause();
        state_ = AnimationState::Finished;
        dirty = true;
        sys_->Activate(this);
        Submit();
        FireEnd(EndReason::Completed);
        return;
    }
    dirty = true;
    sys_->Activate(this);
    Submit();
}

inline void Controller::Play() {
    if (Terminal() || !sys_) return;
    Advance();
    EnsurePlayer();
    for (const auto& p : graphProps_) overrides_.erase(p);   // the graph takes precedence again
    ClaimGraph();
    if (Terminal()) return;
    player_->Play();
    AfterStart();
}

inline void Controller::Reverse() {
    if (Terminal() || !sys_) return;
    Advance();
    EnsurePlayer();
    for (const auto& p : graphProps_) overrides_.erase(p);
    ClaimGraph();
    if (Terminal()) return;
    player_->Reverse();
    AfterStart();
}

inline void Controller::Pause() {
    if (Terminal() || !sys_ || !player_) return;
    Advance();
    player_->Pause();
    if (state_ == AnimationState::Playing) state_ = AnimationState::Paused;
    dirty = true;
    sys_->Activate(this);
    Submit();
}

inline void Controller::Seek(double t) {
    if (Terminal() || !sys_) return;
    Advance();
    EnsurePlayer();
    for (const auto& p : graphProps_) overrides_.erase(p);
    ClaimGraph();
    if (Terminal()) return;
    player_->Seek(t);
    if (state_ == AnimationState::Idle || state_ == AnimationState::Finished) state_ = AnimationState::Paused;
    dirty = true;
    sys_->Activate(this);
    Submit();
}

inline void Controller::SetRate(double r) {
    if (Terminal() || !(r > 0.0) || !std::isfinite(r)) return;
    if (sys_) Advance();
    rate_ = r;
    if (player_) { player_->SetRate(r); if (sys_) Submit(); }
}

inline void Controller::SetLoops(int n) {
    if (Terminal()) return;
    loops_ = n == 0 ? 1 : n;
    if (player_) { player_->SetLoops(loops_); if (sys_) Submit(); }
}

inline std::optional<Sample> Controller::ValueOf(const std::string& p) const {
    if (!owned_.count(p)) return std::nullopt;
    auto o = overrides_.find(p);
    if (o != overrides_.end()) return o->second.Eval(now_);
    if (!player_) return std::nullopt;
    const SampleMap m = player_->Sample();
    auto g = m.find(p);
    if (g == m.end()) return std::nullopt;
    return g->second;
}

inline void Controller::CollectOwned(SampleMap& out) const {
    SampleMap m;
    if (player_) m = player_->Sample();
    for (const auto& p : owned_) {
        auto o = overrides_.find(p);
        if (o != overrides_.end()) { out[p] = o->second.Eval(now_); continue; }
        auto g = m.find(p);
        if (g != m.end()) out[p] = g->second;
    }
}

inline bool Controller::Retarget(const std::string& prop, double target, SpringParams params,
                                 std::optional<double> from) {
    if (Terminal() || !sys_) return false;
    Advance();
    if (sys_->Reduced()) {                         // jump: no spring, zero velocity
        if (!owned_.count(prop)) { owned_.insert(prop); sys_->Claim(this, prop); }
        if (Terminal()) return false;
        overrides_.insert_or_assign(prop, SpringOverride::Held(target, now_));
        dirty = true;
        sys_->Activate(this);
        Submit();
        return true;
    }
    // Seed (value, velocity) BEFORE claiming: claiming makes the previous owner drop it.
    std::optional<Sample> cur = ValueOf(prop);
    if (!cur) {
        Controller* owner = sys_->OwnerOf(target_, prop);
        if (owner && owner != this) { owner->Advance(); cur = owner->ValueOf(prop); }
    }
    if (!cur) {
        if (auto r = sys_->RestValue(target_, prop)) cur = Sample{*r, 0.0};
        else if (from) cur = Sample{*from, 0.0};
    }
    if (!cur) return false;
    if (!owned_.count(prop)) { owned_.insert(prop); sys_->Claim(this, prop); }
    if (Terminal()) return false;
    overrides_.insert_or_assign(prop, SpringOverride(params, cur->value, cur->velocity, target, now_));
    dirty = true;
    sys_->Activate(this);
    Submit();
    return true;
}

inline void Controller::Release(const std::string& prop) {
    if (Terminal() || !sys_ || !owned_.count(prop)) return;
    Advance();
    // Read the value (for the rest record) before dropping the override.
    if (!(graphProps_.count(prop) && player_)) ReleaseProp(prop);   // graph properties stay owned
    const bool wasOverride = overrides_.erase(prop) != 0;
    if (wasOverride) { dirty = true; sys_->Activate(this); Submit(); }
}

inline void Controller::OnLost(const std::string& prop) {
    const bool inFlight = InFlight() || HasUnsettledOverrides();   // before erasing
    owned_.erase(prop);
    overrides_.erase(prop);
    if (owned_.empty() && inFlight) { Terminate(EndReason::Superseded); return; }
    if (sys_ && submitted_) Submit();
}

inline void Controller::Terminate(EndReason r) {
    if (Terminal()) return;
    state_ = r == EndReason::Cancelled ? AnimationState::Cancelled : AnimationState::Superseded;
    const std::vector<std::string> owned(owned_.begin(), owned_.end());
    for (const auto& p : owned) ReleaseProp(p);
    overrides_.clear();
    if (player_) { player_->Pause(); player_->OnComplete(nullptr); player_->OnMarker(nullptr); }
    if (sys_ && submitted_) { submitted_ = false; sys_->Dispatcher().Withdraw(id_, r); }
    auto cb = std::move(onEnd_);
    onEnd_ = nullptr; onMarker_ = nullptr;      // break handle-capturing cycles
    if (cb) cb(r);
}

inline void Controller::Cancel() {
    if (Terminal()) return;
    if (sys_) Advance();                         // capture the final values for rest
    Terminate(EndReason::Cancelled);
}

inline void Controller::SnapToEnd() {
    if (Terminal() || !sys_) return;
    Advance();
    for (auto& kv : overrides_) kv.second = SpringOverride::Held(kv.second.target, now_);
    const bool wasInFlight = InFlight();
    if (player_ && wasInFlight) {
        player_->Pause();
        player_->Seek(player_->IsReversed() ? 0.0 : current_->Duration());
        state_ = AnimationState::Finished;
    }
    dirty = true;
    sys_->Activate(this);
    Submit();
    if (wasInFlight) FireEnd(EndReason::Completed);
}

inline void Controller::OnDeactivated() {
    if (submitted_ && !Terminal() && sys_) { submitted_ = false; sys_->Dispatcher().Withdraw(id_, EndReason::Completed); }
}

}  // namespace detail
}  // namespace motion
