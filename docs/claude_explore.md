timestamp: 2026-10-19 18:17:11 +0800 Asia/Taipei
chat URL: https://claude.ai/chat/5b0701a6-cbce-4ee5-a392-87e8d3c95643
prompt=explore
status: complete — Continue after Free limit reset at 6:10 PM; four-section reply captured
account: miitwo.e@gmail.com (Free)
artifacts: software_dispatcher.h, software_demo_test.cpp (in /workspace/claude_explore_artifacts/)

---

`api.md` and `usage.md` do not exist anywhere in the workspace. I searched the project directory, the uploads folder and everything under `/mnt/user-data`. The only documentation is the banner comment at the top of each header, so the review below compares those banners with the code's behaviour.

I ran two probes against the headers and wrote one small skeleton. Everything about WinUI 3 and Windows.UI.Composition API names is from memory and unverified, and I have not compiled any WinRT code. I changed none of the six existing headers. The new files are in `motion/mvp/`:
- `software_dispatcher.h`
- `software_demo_test.cpp`

## 1) Docs review

**D1. The documentation does not exist (High)**
- Evidence: the search found only `API_INVENTORY_members.md`. Rules that matter (ownership, reduced motion, tick cadence) live in header banners or in chat.
- Fix: create `api.md` with these sections and move each contract below into it:
  - Contracts: tick, dispatcher, target, property vocabulary, units, threading, reduced motion.
  - Lifecycle states.
  - Known limits.
- Also create `usage.md` with three recipes: a token transition, an interruptible drag with `Retarget`, and reduced motion.

**D2. `IDispatcher` has no ordering, reentrancy or time contract, and one comment is wrong (High)**
- Evidence:
  - The `Withdraw` comment says "finished, cancelled, superseded". But `Withdraw(Completed)` also fires when a controller merely goes idle, for example paused on a held pose. The controller tests show two withdraws in one run for that reason.
  - `Submit` is called synchronously inside handle calls such as `Play` and `Retarget`.
  - `Apply` carries the merged values of all controllers on a target, and only on ticks where something is active or dirty.
  - Nothing says whether a dispatcher may call back into a handle.
- Fix, paste-ready, to replace the comment block on `IDispatcher`:
  > Submit: called synchronously from handle calls and Tick, on the dispatcher thread, at every discontinuity. Withdraw(id, Completed) means "this controller went inactive" (finished OR paused/idle), not "the animation reached its end". Withdraw(Cancelled/Superseded) means stop. Apply: once per Tick per target, merged across controllers, only for properties each controller owns, only when something is active or dirty. Implementations MUST NOT call back into AnimationHandle or MotionSystem from any of these three methods.

**D3. `TargetId` has no resolution or lifetime contract (High)**
- Evidence: the header says only `using TargetId = std::uint64_t; // opaque id`, yet `ICompositorBackend` takes it. No API registers a target, resolves it to a Visual, or says what happens when the target is destroyed mid-animation.
- Fix, paste-ready, next to `TargetId`:
  > TargetId is chosen by the app and resolved ONLY by the backend or sink, through an app-owned registry. The registry must outlive every controller targeting an id. If a target dies mid-animation, the app must Cancel its handles first; a backend receiving an unknown id must ignore the call.

**D4. Property names alias compositor paths, and ownership is by name (High)**
- Evidence (probe): controller A plays `scale`, which starts `Scale.X` and `Scale.Y`. Controller B then retargets `scaleX`. The dispatcher logs `start-spring Scale.X` with no stop, and A stays `Playing` with `Owns(scale)` still true. Nothing tells A, and the compositor now runs B's spring on `Scale.X`. The probe's own table in `StaticCapabilityProbe` defines both `scale` and `scaleX`.
- Fix: until Q1 below is decided, add this to the property vocabulary docs:
  > Properties whose compositor paths overlap (scale vs scaleX/scaleY) must never be animated concurrently by different controllers on one target. Ownership is tracked per property NAME, not per path.

**D5. The tick and idle contract is unstated (Medium)**
- Evidence: `MotionSystem` has no wake or activity hook (no match for any such name). `SetReducedMotion` only marks controllers dirty. The skeleton's test shows the final values reach the sink only after one more `Tick`.
- Fix, paste-ready, in the `MotionSystem` banner:
  > Call Tick at every frame while ActiveCount() > 0. After ANY handle call that can start or change motion, and after SetReducedMotion, call Tick at least once; the library does not wake your frame loop. Stop pumping when ActiveCount() == 0.

**D6. "Reduced motion is required" holds only on the token path (Medium)**
- Evidence: the `tokens.h` banner says `reduced` is required for every transition. `MotionSystem::Create(target, full, reduced = nullptr)` silently derives an instant version when it is absent.
- Fix:
  > Token transitions must declare a reduced variant. Hand-built timelines passed to Create without one get a derived "instant" variant (every property jumps to its end value). Pass a reduced timeline explicitly to override.

**D7. Two override mechanisms and two token vocabularies (Medium)**
- Evidence: `Player::Retarget` is public but controllers use their own override table. `Curve::FromToken` resolves five placeholder curves via `FindEasing`, while `TokenSet` treats only `"linear"` as built in.
- Fix, paste-ready, above `Player::Retarget` and `Curve::FromToken`:
  > SUPERSEDED for application use: prefer AnimationHandle::Retarget. FromToken uses placeholder built-ins that TokenSet does not honour; use TokenSet for design tokens.

**D8. The backend has no time-reference contract (Medium)**
- Evidence: the snapshot's `clockTime` is in `IClock` time, and the planner computes `elapsed` and `delay` relative to it. `ICompositorBackend` has no "now" parameter. The test double starts animations on the same clock, which a real backend cannot assume.
- Fix, paste-ready, on `ICompositorBackend`:
  > Start* is called within the same dispatcher call as Submit. The backend must treat the animation as starting NOW and MUST honour delay/elapsed/rate/reversed relative to that instant. Submit-to-start latency is not compensated.

**D9. `MotionSystem` vs `Timeline` contract is silent (Medium)**
- Evidence: a `Timeline` is immutable, target-agnostic and holds literal values; one instance can be shared across controllers and targets. Nothing validates that the target has the property, and token transitions cannot express relative values.
- Fix:
  > Timelines carry absolute values and know nothing about targets. A property the target lacks is silently ignored by the backend. For values relative to the current state, use Retarget.

**D10. Units are scattered (Low)**
- Evidence: rotation in degrees appears only in a comment inside `StaticCapabilityProbe`. `Timeline::Evaluate` velocity is per timeline-second, while `Player::Sample` is per real second, and `AnimationHandle::Value` does not say.
- Fix:
  > Values are DIPs (offsets), unitless (scale, opacity) or degrees (rotation). Timeline::Evaluate velocity is per timeline-second; Player::Sample and AnimationHandle::Value velocity is per real second.

## 2) Smallest WinUI 3 MVP

**Stage 0 (prerequisites, about 2 days):** write D2–D5 into the headers and `api.md`, and decide Q1 below.

**Stage A: software-path Tick demo (one spring + one fade, no Composition lowering)**

New files:

| File | State |
|---|---|
| `mvp/software_dispatcher.h` | written, compiled, tested |
| `mvp/software_demo_test.cpp` | written, compiled, tested |
| `App/VisualSink.h` | not written |
| `App/RenderPump.cpp` | not written |
| `App/MainWindow.xaml` + `.cpp` | not written |
| App project | not created |

- `software_dispatcher.h` has `IPropertySink`, `SoftwareDispatcher` (forwards `Apply`, ignores `Submit` and `Withdraw`) and `FramePump` (render on demand). The test drives it at 120 Hz: the fade and the spring settle in 69 frames (575 ms), the pump stops itself, nothing is written while idle, the pump is not woken by the library, and a mid-flight `SetReducedMotion` reaches the sink only after `Wake` plus one frame. 10 checks, 0 failures, ASan and UBSan clean.
- Still to write: `VisualSink` resolves `TargetId` through an app registry and writes `opacity` and `offsetX` (as `UIElement.Translation`, not `Visual.Offset`, since XAML layout may overwrite `Offset`). `RenderPump` subscribes `CompositionTarget.Rendering` in `setRendering`. `MainWindow` has a Rectangle with two buttons: "move" (retarget `offsetX` to 0 or 300) and "fade".
- Deliverables: a running demo, a CSV of frame intervals from `Rendering`, and a checklist covering interruption mid-move, reduced-motion toggle, and idle (no frame callbacks when nothing animates).
- Risks:
  - Whether `Rendering` follows the monitor's refresh rate under mixed 60/120 Hz.
  - Layout fighting position properties.
  - Offset or Translation units against DPI scaling.
  - Everything runs on the UI thread, so jank under load is expected; it is the baseline.
- Exit: spring and fade look right, interruption has no jump, idle is idle.

**Stage B: single-property `CompositionDispatcher` pilot**

| File | Role |
|---|---|
| `App/WinRtBackend.h/.cpp` | `ICompositorBackend` for one visual: opacity keyframes and one translation spring |
| `App/WinRtProbe.h` | `Caps()` from API presence; maps only `opacity` and `offsetX` |
| `mvp/backend_conformance.h` | Reusable backend test (task 3 below) |
| Capture protocol note | How to record the verification results |

- Deliverables:
  - The pilot with the UI thread deliberately blocked for 200 ms while the animation keeps running, which is the proof of offload.
  - Recorded answers for S1–S5 and K1–K5, written into `SpringMapping` and `Capabilities`.
- Risks:
  - Period semantics (Q3).
  - Starting part-way, rate and reverse (K3): the MVP backend should decline these, which needs the capability flags in task 2.
  - `Stop` and replace behaviour.
  - Translation versus Offset (Q2).
  - The `TargetId` lifetime (D3).
- Exit: the pilot spring and fade run while the UI thread is blocked, and recorded values match `SimulateSpring` and `SimulateKeyframes` within tolerance.

## 3) Module 6 status and next tasks

**Done (tested against the simulated compositor):** planner rules and reasons, all four plan kinds, dispatcher start, replace, stop, hold and withdraw, handoff between controllers, golden app-thread fallback, and seven end-to-end scenarios (2,063 checks).

**Stub or scaffold:**
- `StaticCapabilityProbe` is a hardcoded table, and the real probe is not written.
- `ICompositorBackend` is an interface only.
- `SpringMapping` is the identity.
- The `Simulate*` functions model assumed behaviour.
- The default table maps `offsetX` to `Offset.X`, which is probably wrong for XAML elements (Q2).

**Tests still missing:**
- A custom `ICapabilityProbe` subclass.
- More than one target in the dispatcher.
- `maxKeyframes` through the dispatcher.
- A hold on a path owned by another controller.
- Path aliasing (the D4 probe becomes this test).
- A dedicated `CubicBezier` test file, which has never been written.

**Next three coding tasks:**
1. **Remove alias conflicts (D4).**
   - Default to the Q1 option A: only per-axis scalar properties (`scaleX`, `scaleY`), with a token-compiler expansion so a `scale` token becomes two tracks.
   - The probe maps one property to one path.
   - Turn the probe into a regression test.
   - Files: `composition.h`, `tokens.h`, tests.
2. **Capability flags for partial start, rate and reverse.**
   - Add `partialStart`, `playbackRate` and `reverse` to `Capabilities` (default true).
   - The planner falls back with a reason, or holds, when a backend declines. For example, an MVP backend that only starts at 0 gets an app-thread plan for any resubmit with `elapsed > 0`.
   - Tests: each flag off gives the expected fallback.
3. **Reusable backend conformance suite (`mvp/backend_conformance.h`).**
   - Takes a backend factory plus a "value at time" probe.
   - Runs start, replace, stop and set, a delayed preset, an elapsed start, reversed and rate scenarios against `Simulate*` with stated tolerances.
   - Runs now on `SimBackend`, and later on the WinRT backend.
   - Add the missing tests above in the same pass.

Chained keyframes-then-spring stages are deferred. They are not needed for the Stage B pilot.

## 4) Three open questions for Gemini, DeepSeek and Claude

Each answer should be a one-line verdict plus evidence.

1. **Alias handling.** Which ownership model should the library use?
   - **A:** only canonical per-axis scalar properties, with composite tokens such as `scale` expanded into per-axis tracks when the token file is compiled.
   - **B:** keep composite properties, and make `MotionSystem` or the dispatcher path-aware with an alias table that supersedes on any shared path.
   - Answer A or B, and show that your choice prevents this failure: A plays `scale` while B retargets `scaleX` on the same target.
2. **Position property on XAML elements.** For a WinUI 3 `UIElement` in a packaged app, should position be animated through `UIElement.Translation` (with `ElementCompositionPreview.SetIsTranslationEnabled`) rather than `Visual.Offset`?
   - Answer YES or NO with a link to official documentation or a sample.
   - Give the exact property-path string for the X axis to pass to `StartAnimation`.
3. **Spring `Period` semantics.** In the documented `SpringScalarNaturalMotionAnimation`, is `Period` the undamped natural period T0 (so our `response` equals `Period`), the damped period (so `response = Period * sqrt(1 - ζ²)`), or something else?
   - Quote the documentation, and give the formula that makes our `Spring{response, damping}` match the same curve.
   - If the documentation is silent, answer "undocumented" and propose a measurement protocol of at most three captures.