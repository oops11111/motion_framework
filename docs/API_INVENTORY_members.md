# FluidMotion API inventory: member-level appendix

Generated from the headers as they stand (spring.h, bezier.h, timeline.h, tokens.h, controller.h,
composition.h). Nothing was changed to produce this. Namespace: `motion` (internal helpers in `motion::detail`).

Status vocabulary
- **implemented** — code present and hit by the Linux test suites (g++ 13, ASan/UBSan clean).
- **implemented (indirect)** — exercised only through other APIs, no direct assertion on it.
- **implemented (untested)** — code present, no test touches it.
- **unverified** — implemented, but its correctness depends on Windows compositor behaviour never checked on hardware.
- **internal** — public in C++ terms only because the header needs it; not app-facing.

## spring.h
| Member | Status | Notes |
|---|---|---|
| `SpringParams{response, damping}` | implemented | seconds / ratio; clamped inside `Spring` (response >= 1e-4, damping >= 0) |
| `SpringState{value, velocity}` | implemented | |
| `Spring(params, from, target, v0)` | implemented | closed form, 3 regimes, stable overdamped roots |
| `Spring::At(t)` | implemented | checked against RK4, regime boundaries, large zeta |
| `Spring::Retarget(t, newTarget)` | implemented | continuity tested; controllers do not use it (they build a new Spring) |
| `Spring::SettleTime(posEps, velEps, cap)` | implemented | O(1) envelope bound; never optimistic vs the scan, up to ~0.25 s pessimistic |
| `Spring::SettleTimeScan(...)` | internal | test oracle only |
| `Spring::Target()`, `Params()` | implemented (indirect) | |

## bezier.h
| Member | Status | Notes |
|---|---|---|
| `CubicBezier(x1,y1,x2,y2)` | implemented | constexpr |
| `CubicBezier::IsValid` (static) | implemented (untested) | |
| `Ease(x)`, `Slope(x)` | implemented (indirect) | no dedicated test file; see Gaps |
| `X1() Y1() X2() Y2() IsLinear()` | implemented (indirect) | added for module 6; used by the planner |
| `EasingToken`, `kEasingTokens` | implemented | PLACEHOLDER curve values |
| `FindEasing(name)` | implemented (indirect) | only reached through `Curve::FromToken` |

## timeline.h
| Member | Status | Notes |
|---|---|---|
| `IClock::Now()` | implemented | |
| `SteadyClock` | implemented (untested) | wraps std::chrono::steady_clock |
| `ManualClock::Set/Advance` | implemented | |
| `FixedStepClock::Tick/Frame/Step` | implemented | |
| `Curve::Linear / Bezier(x1..y2) / Bezier(CubicBezier) / FromSpring / FromToken` | implemented | `FromSpring` only indirect; `FromToken` uses placeholder tokens |
| `Sample{value, velocity}`, `SampleMap` | implemented | |
| `kSpringPosEps/kSpringVelEps`, `SpringPosEps(span)`, `SpringVelEps(span)`, `SpringSettleTime(spring, from, target)` | implemented | span-relative thresholds (post module-6 fix) |
| `Track(property, from)` | implemented | |
| `Track::To / SpringTo(inheritVelocity) / Hold` | implemented | |
| `Track::Property / Duration / InitialValue / FinalValue / Segments` | implemented | `Segment` nested struct |
| `Track::Eval(t)` | implemented | value + velocity, holds ends |
| `Track::EndVelocity()` | implemented (indirect) | undocumented |
| `MarkerEvent`, `PlacedTrack` | implemented | `PlacedTrack::track` points into the Timeline |
| `Node` (+ `TrackNode`, `DelayNode`, `MarkerNode`, `GroupNode::Kind`) | implemented | build via factories, not directly |
| `NodePtr`, `Animate`, `Delay`, `Marker`, `Sequence`, `Parallel`, `Stagger` | implemented | |
| `Timeline(root)` | implemented | |
| `Timeline::Duration / Root / Markers / Tracks / MarkerTime / Evaluate` | implemented | `Evaluate` is pure, clamped, exact end values |
| `Player(timeline, clock)` | implemented | |
| `Player::Play / Pause / Seek / Reverse / SetRate / SetLoops / Update` | implemented | |
| `Player::OnComplete / OnMarker` | implemented | |
| `Player::IsPlaying / IsFinished / IsReversed / Position / Rate / Sample` | implemented | `Rate()` has no direct test |
| `Player::Retarget / ReleaseOverride` | implemented | superseded in practice by controllers; no deprecation note in the header |
| `Player::ReleaseAllOverrides` | implemented (untested) | |

## tokens.h
| Member | Status | Notes |
|---|---|---|
| Motion token schema v1 (JSON document) | implemented | grammar in the header banner; sample `motion.json` |
| `Diagnostic{path, message}`, `ToString()` | implemented | JSON-Pointer paths |
| `TokenSet::Parse(json, errors)` (static) | implemented | collects all errors |
| `TokenSet::Version / Duration / Easing / Spring` | implemented | `Easing("linear")` is the only built-in |
| `TokenSet::TransitionNames / HasTransition / BuildTimeline(name, reduced)` | implemented | immutable shared timelines |
| `detail::Json`, `detail::JsonParser`, `detail::Compiler` | internal | embedded strict parser |

## controller.h
| Member | Status | Notes |
|---|---|---|
| `TargetId`, `ControllerId` | implemented | |
| `AnimationState`, `EndReason` | implemented | |
| `SpringOverrideInfo`, `ControllerSnapshot` | implemented | the data the dispatcher receives |
| `IDispatcher::Submit / Withdraw / Apply` | implemented | the seam |
| `NullDispatcher` | implemented (indirect) | default when none supplied |
| `MakeInstantTimeline(tl)` | implemented (indirect) | duplicates the token loader's "instant" rule |
| `AnimationHandle::Play Pause Seek Reverse SetRate SetLoops Cancel` | implemented | |
| `AnimationHandle::Retarget(property, target, spring, from?)`, `Release(property)` | implemented | |
| `AnimationHandle::State IsActive Position Owns Value Values Id SameAs` | implemented | `IsActive`, `Values`, `SameAs` lightly tested |
| `AnimationHandle::OnEnd / OnMarker` | implemented | callbacks cleared on terminal states |
| `MotionSystem(clock, dispatcher?)` | implemented | non-copyable |
| `MotionSystem::Create(target, full?, reduced?)`, `Create(target, TokenSet, name)` | implemented | second returns a null handle for unknown names |
| `MotionSystem::Tick / SetReducedMotion / ReducedMotion / Current / ActiveCount` | implemented | |
| `MotionSystem::Clock Reduced Dispatcher Register Unregister Activate Claim ReleaseOwnership OwnerOf RestValue` | internal | public only so `detail::Controller` can call them |
| `detail::Controller`, `detail::SpringOverride` | internal | |

## composition.h
| Member | Status | Notes |
|---|---|---|
| `Capabilities{springNaturalMotion, cubicBezier, bezierOvershoot, maxKeyframes}` | unverified | defaults are assumptions |
| `CompositorProperty{paths}` | unverified | property path names not checked on Windows |
| `ICapabilityProbe::Caps / Map` | implemented | no custom subclass exercised in tests |
| `StaticCapabilityProbe::Caps Map SetCaps MutableCaps Add Remove` | implemented | hardcoded table: opacity, offsetX/Y, scale, scaleX/Y, rotation |
| `SpringMapping{periodScale, dampingScale}`, `ToCompositor` | unverified | identity mapping, assumptions S1/S2 |
| `PlanKind`, `ToString(PlanKind)` | implemented | CompositorKeyframes, CompositorSpring, Hold, AppThread |
| `KeyframeSpec`, `KeyframeAnimation`, `SpringAnimation` | unverified | time model and easing semantics assumed (K1, K3, S3, S4) |
| `PropertyPlan`, `LoweringPlan::Find / Count / FullyOffloaded / ToString` | implemented | |
| `EvaluateKeyframes`, `SimulateKeyframes`, `SimulateSpring` | unverified | reference semantics, not the real compositor |
| `CompositionLoweringPlanner(probe, mapping)::Plan(snapshot)` | implemented | v1 rules |
| `ICompositorBackend::StartKeyframes StartSpring Stop SetValue` | unverified | interface only; test double in the tests |
| `CompositionDispatcher::Submit Withdraw Apply PlanFor LiveAnimationCount` | implemented | verified against the simulation only |
