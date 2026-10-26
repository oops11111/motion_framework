timestamp: 2026-10-19 13:46:44 Asia/Taipei
chat: https://claude.ai/chat/5b0701a6-cbce-4ee5-a392-87e8d3c95643
prompt: api_inventory

---

I'll read the headers as they stand now rather than work from memory, then build the inventory from that.

# FluidMotion API inventory

I built this by reading the six headers as they stand (`spring.h`, `bezier.h`, `timeline.h`, `tokens.h`, `controller.h`, `composition.h`, about 2,665 lines), plus the test suites to see what is actually exercised. Nothing was modified.

`api.md` and `usage.md` do not exist in the project. The only documentation is the banner comment at the top of each header, so "docs" below means those banners. The project's name appears only in your message; in the code everything lives in `namespace motion`. Member-level detail for every class is in `API_INVENTORY_members.md`.

Status words:
- **implemented**: code exists and the Linux suites exercise it (422 + 743 + 236 + 2,063 = 3,464 checks, all passing, ASan/UBSan clean).
- **indirect**: exercised only through other APIs, with no assertion on it directly.
- **untested**: no test touches it.
- **unverified**: implemented, but it depends on Windows compositor behaviour never checked on hardware.
- **internal**: public in C++ terms only because the header needs it.
- **planned**: discussed in the roadmap, with no header.

## 1. Public surface

Sibling free functions are grouped into one row.

| Symbol | Header | Status | Role |
|---|---|---|---|
| `SpringParams`, `SpringState` | spring.h | implemented | Response/damping inputs and (value, velocity) output |
| `Spring` | spring.h | implemented | Closed-form damped spring: `At`, `Retarget`, `SettleTime` (analytic bound), `SettleTimeScan` (internal test oracle) |
| `CubicBezier` | bezier.h | implemented (indirect) | CSS-style easing: `Ease`, `Slope`, control-point accessors; no dedicated test file |
| `EasingToken`, `kEasingTokens`, `FindEasing` | bezier.h | implemented (indirect) | Five named curves with **placeholder values**; reached only via `Curve::FromToken` |
| `IClock` | timeline.h | implemented | Injectable time source in seconds |
| `SteadyClock` | timeline.h | untested | Real clock over `std::chrono` |
| `ManualClock`, `FixedStepClock` | timeline.h | implemented | Deterministic clocks for tests and replay |
| `Curve` (+ `Kind`) | timeline.h | implemented | Segment interpolation: linear, bezier, spring, or named token |
| `Sample`, `SampleMap` | timeline.h | implemented | Value and velocity per property |
| `kSpringPosEps`, `kSpringVelEps`, `SpringPosEps()`, `SpringVelEps()`, `SpringSettleTime()` | timeline.h | implemented | Span-relative spring settle thresholds (added after a bug found in module 6) |
| `Track` (+ `Segment`) | timeline.h | implemented | Scalar keyframe track: `To`, `SpringTo`, `Hold`, `Eval` |
| `MarkerEvent`, `PlacedTrack` | timeline.h | implemented | Marker time; track with its start offset (the list the compositor planner reads) |
| `Node`, `NodePtr`, `TrackNode`, `DelayNode`, `MarkerNode`, `GroupNode` | timeline.h | implemented | Immutable composition tree; meant to be built via factories |
| `Animate`, `Delay`, `Marker`, `Sequence`, `Parallel`, `Stagger` | timeline.h | implemented | Node factories |
| `Timeline` | timeline.h | implemented | Pure `Evaluate(t)`, duration, sorted markers, flattened tracks |
| `Player` | timeline.h | implemented | Play, pause, seek, reverse, rate, loops, callbacks; also a built-in `Retarget` |
| Token schema v1 + `motion.json` | tokens.h | implemented | Strict JSON document format; reduced-motion variant required per transition |
| `Diagnostic` | tokens.h | implemented | Error with JSON-Pointer path and message |
| `TokenSet` | tokens.h | implemented | `Parse`, preset lookups, `BuildTimeline(name, reduced)` |
| `detail::Json`, `JsonParser`, `Compiler` | tokens.h | internal | Embedded strict parser and compiler |
| `TargetId`, `ControllerId` | controller.h | implemented | Opaque ids |
| `AnimationState`, `EndReason` | controller.h | implemented | Lifecycle and termination enums |
| `SpringOverrideInfo`, `ControllerSnapshot` | controller.h | implemented | What a dispatcher receives at each discontinuity |
| `IDispatcher`, `NullDispatcher` | controller.h | implemented / indirect | The seam: `Submit`, `Withdraw`, `Apply`; no-op default |
| `MakeInstantTimeline` | controller.h | indirect | Derives the "instant" reduced timeline |
| `AnimationHandle` | controller.h | implemented | Copyable handle: control, `Retarget`, `Release`, queries, `OnEnd`/`OnMarker` |
| `MotionSystem` | controller.h | implemented | `Create`, `Tick`, `SetReducedMotion`, `Current`; ten more public methods are internal-facing |
| `detail::Controller`, `detail::SpringOverride` | controller.h | internal | Per-animation state machine and spring override |
| `Capabilities`, `CompositorProperty` | composition.h | unverified | What the compositor can offload; property-to-path mapping |
| `ICapabilityProbe`, `StaticCapabilityProbe` | composition.h | implemented | Probe interface and a settable default table |
| `SpringMapping` | composition.h | unverified | Our response/damping to compositor Period/DampingRatio (identity today) |
| `PlanKind`, `PropertyPlan`, `LoweringPlan` | composition.h | implemented | Plan data model: keyframes, spring, hold, app-thread |
| `KeyframeSpec`, `KeyframeAnimation`, `SpringAnimation` | composition.h | unverified | Animation specs; timing and easing semantics assumed |
| `EvaluateKeyframes`, `SimulateKeyframes`, `SimulateSpring` | composition.h | unverified | Reference semantics under those assumptions |
| `CompositionLoweringPlanner` | composition.h | implemented | Snapshot to `LoweringPlan`, with a reason on every fallback |
| `ICompositorBackend` | composition.h | unverified | The only thing a Windows backend must implement; a test double exists, no real backend |
| `CompositionDispatcher` | composition.h | implemented | `IDispatcher` over planner + backend; verified only against the simulation |

**Planned, with no header:**

| Item | Status |
|---|---|
| Real WinRT backend and probe (module 7) | planned |
| WinMD / C# projection and XAML attached properties | planned |
| Rive surface host (Tier 2) | planned |
| Follower splitting and pointer pipeline | planned |
| ETW tracing and debug overlay | planned |
| Preview host and Figma plugin | planned |
| Quality tiers | planned |
| Expressions, state machines, decay | planned |
| Per-window clocks / DPI / refresh APIs | planned |
| HWND reference host | planned |

## 2. Call graph sketch

```
App
 ├─ TokenSet::Parse(json) ─► Compiler ─► Timeline(full), Timeline(reduced)
 │                                         ▲ built from Animate/Track/Sequence/... (or by hand)
 ├─ MotionSystem(clock, IDispatcher*) ◄── Create(target, timeline | TokenSet+name)
 │      │ Tick()                                      │
 │      ▼                                             ▼
 │   detail::Controller  ◄────────────── AnimationHandle (Play/Seek/Retarget/Cancel/...)
 │      ├─ Player ─► Timeline::Evaluate ─► Track::Eval ─► CubicBezier / Spring
 │      ├─ SpringOverride (Retarget) ─► Spring::At / SpringSettleTime
 │      └─ property ownership table (last wins) + rest values
 │
 └─ IDispatcher  (Submit / Apply / Withdraw)
        ├─ NullDispatcher
        └─ CompositionDispatcher
              ├─ CompositionLoweringPlanner ─► ICapabilityProbe, SpringMapping ─► LoweringPlan
              └─ ICompositorBackend  ─► (test double today; WinRT backend planned)
```

- **Per frame:** `Tick()` advances every active controller, then calls `Apply` once per target with the values it owns. App-thread properties are written from that `Apply`.
- **On state changes:** play, pause, seek, rate, retarget and ownership changes call `Submit` with a snapshot. That is when the planner re-lowers, and the dispatcher starts, replaces or stops compositor animations.
- **On end:** `Withdraw` stops animations. A normal completion also writes the exact final value.

## 3. Gaps

**Mentioned in docs or earlier design, missing in headers**

| Item | Where mentioned | Header status |
|---|---|---|
| `api.md`, `usage.md` | your request | files do not exist |
| Fluent C# API (`Motion.Transition(...).On(Trigger.Enter)`, `Motion.Animate(...).Reduced(...)`) | first design answer | absent |
| "Reduced alternative is a required parameter" | first design answer | enforced only on the token path; `MotionSystem::Create(..., reduced = nullptr)` silently derives an instant one |
| Expressions/bindings, state machines, decay animations | design answer | absent |
| Per-window clocks, DPI and refresh-rate APIs | design answer | absent; only `IClock` exists |
| ETW events, debug overlay, quality tiers | design answer | absent |
| Rive, preview host, Figma plugin, WinMD projection | plan and WBS | absent |
| Chained keyframes-then-spring stages | `composition.h` banner | explicitly planned, not implemented |
| Real compositor probe and backend | `composition.h` banner | module 7, absent |

**In headers but undocumented or inconsistent**

| Item | Issue |
|---|---|
| `Curve::FromToken` vs `TokenSet` | `FromToken` resolves all five placeholder tokens; the loader only treats `"linear"` as built in. Two token vocabularies can disagree. |
| `Player::Retarget` / overrides | Still public and tested, but controllers use their own override table. The header does not mark it as superseded. |
| `MotionSystem` internal methods | Ten public methods labelled internal in a comment, but nothing stops an app calling them. |
| `Spring::SettleTimeScan` | Public, though only a test oracle. |
| `detail::*` types | Visible in headers, no stability promise stated. |
| Undocumented members | `Track::EndVelocity`, `Player::Rate`, `Spring::Params`, `Spring::Target`, `Timeline::Root`, `StaticCapabilityProbe::MutableCaps`, `SpringOverrideInfo` field units |
| Numbering | The earlier project plan numbered the dispatcher "module 4" and the planner work separately; the headers call timeline/tokens/controller/composition modules 3/4/5/6. |

**Test gaps among implemented APIs:**
- There is no dedicated `CubicBezier` test. The solver is covered by one finite-difference slope check, a token easing comparison and the keyframe reference comparison. The planned endpoint, monotonicity, oracle and symmetry tests from that module were never written.
- `SteadyClock`, `Player::ReleaseAllOverrides` and `CubicBezier::IsValid` are untested.
- No test uses a custom `ICapabilityProbe` subclass or more than one target in the dispatcher.

## 4. Module 6 (`composition.h`)

**Scaffold: the structure is present, behaviour is minimal or assumed**

| Item | State |
|---|---|
| `StaticCapabilityProbe` | Hardcoded table of seven property names; the real probe (API-presence checks) is not written |
| `ICompositorBackend` | Interface only; no WinRT implementation |
| `SpringMapping` | Identity (factors of 1.0); the compositor convention is unknown |
| `EvaluateKeyframes`, `SimulateKeyframes`, `SimulateSpring` | Reference model of the assumed compositor, used by tests; not the real thing |
| `Capabilities` defaults | Assumed (springs available, cubic-bezier available, overshoot off) |

**Tested (against the simulated compositor only)**

| Area | Coverage |
|---|---|
| Pure compositor plans | Keyframe content, easing control points, spring plans, scale on both paths, mapping knob, rate folding |
| Fallbacks | Unknown property, mixed spring/keyframe, overshoot (on and off), multiple tracks, looping, instant jump, reversed spring, each capability switch, keyframe limit, extended mapping, mixed plan |
| Timing and holds | Delay, elapsed, rate, reverse in all positions, paused pose, finished track, delayed spring, override seeds, settled override, override masking graph |
| Dispatcher | Replace without duplicates, pause as hold, cancel stops everything, completion writes the exact final value, reversed run ends on the initial value, reduced motion produces holds and no animations |
| Retarget and handoff | Seed updates on retarget and on resubmit; old animation stops before the new one starts with no value jump; superseded controller releases its plan |
| Golden fallback | App-thread values match independent `Evaluate()` over 360 ticks; compositor-driven properties are never written by `Apply` |
| End to end | Seven scenarios matching controller to the simulation, to 1e-9 on keyframes and early spring, and to 2e-3 in spring tails |

**Still TODO**

| Item | Why it matters |
|---|---|
| Real WinRT probe and backend (module 7) | Nothing runs on Windows yet |
| Hardware verification of S1–S5 and K1–K5 | Every lowering decision rests on these assumptions |
| Chained stages (keyframes then spring) | Common micro-interactions like button press fall back to the app thread |
| Multi-track conflict lowering | Same-property sequences fall back |
| Looping, instant jumps, reversed springs | All fall back in v1 |
| Expression-driven followers / pointer binding | Needed for zero-lag follower parts |
| Exact rest value on cancel | Compositor value may differ from the controller's by up to a frame |
| Failure handling | No behaviour defined if a backend call fails |
| Threading and batching | UI-thread marshalling and per-frame allocation are unmeasured |
| Telemetry | No tracing hooks for plans or fallbacks |
| Extra tests | Custom probe subclass, multiple targets, `maxKeyframes` through the dispatcher |
