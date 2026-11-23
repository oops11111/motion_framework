# Module 7 plan: a real Windows.UI.Composition backend (MF-02)

Status: planning only. As of main `f29485e` (2026-11-23) the tree has the
lowering planner (`code/composition.h`: `SpringMapping`, `KeyframeAnimation`,
`SpringAnimation`) and the `ICompositorBackend` interface, but no WinRT
implementation. Lowering output is consumed only by tests/simulation.
`StaticCapabilityProbe` returns all-on `Capabilities` defaults
(`springNaturalMotion`, `cubicBezier`, `bezierOvershoot` all `true`,
`maxKeyframes` 512) and its own comment says the real probe is Module 7's job
(API presence checks via `ApiInformation`: unverified).

## Scope

1. A WinRT backend implementing `ICompositorBackend` (`StartKeyframes`,
   `StartSpring`, `Stop`, `SetValue`) against `Windows.UI.Composition`
   `Visual`/`Compositor` objects, behind the existing interface so a wrong
   assumption is fixed in one place (`SpringMapping`, `Capabilities`, or the
   backend) as `composition.h` already requires.
2. A real capability probe replacing the all-on defaults, using
   `ApiInformation` presence checks, recording the OS build each probe ran on.
3. Per-assumption verification probes (below). No behavior change to the
   planner, the software path (`fm_capi`), or the token/timeline core.

Non-goals: Rive, a WinMD projection, a Figma plugin, and PERF-CI (still HOLD
per `docs/z6_closed.md`). Overdamped springs (zeta > 1) stay on the AppThread
software path until the S2 probe covers them, as `composition.h` already gates.

## Verification checklist (S1-S5, K1-K5)

`composition.h` heads the list "ALL UNVERIFIED" while S1 and K2 carry
verification claims. The plan resolves the contradiction by re-verifying S1
and K2 on the target OS and recording the evidence, not by editing the claim.
Each item needs a named probe that compares planner output with compositor
behavior on a real machine, with OS build, machine, and date recorded.

- S1 (claimed VERIFIED: ruling A1, 2026-10-20, #36/#41): `Period = response /
  (2*pi)`, i.e. the compositor reads `Period` as 1/omega0, not the undamped
  period. Probe: measure the actual oscillation period of a compositor spring
  for a known `response` and compare with `r / (2*pi)`.
- S2 (unverified): `DampingRatio` is our `damping` (zeta, 1.0 = critical).
  Probe: zeta = 1 shows no overshoot; zeta < 1 overshoots by the analytic
  amount.
- S3 (unverified): `InitialVelocity` is in value-units per real second.
  Probe: start a compositor spring with a known initial velocity and compare
  the first sampled values with `spring.h` `At(t)`.
- S4 (unverified): an animation may start with a `DelayTime` and the property
  keeps its preset value until the delay elapses. Probe: sample during the
  delay window.
- S5 (unverified): the compositor spring stops at its own rest threshold;
  ours snaps to target at `SpringSettleTime`. Probe: record the end-of-motion
  delta between the two and document the expected difference; do not force
  them to match.
- K1 (unverified): a keyframe's easing shapes the segment ENDING at that
  keyframe. Probe: two-segment keyframe animation, sample both segments.
- K2 (claimed verified: 派题 #20): `CubicBezierEasingFunction` honors
  control-point y outside [0,1] (`bezierOvershoot = true`). Probe: overshoot
  easing on the target OS; if it does not overshoot, `bezierOvershoot` must
  become probe-driven, not a constant.
- K3 (unverified): an animation can start part-way (`elapsed > 0`, e.g. via
  `AnimationController.Progress`) and play at a non-1 rate or in reverse.
  Probe: start at 50% progress, rate 2, and reversed; compare sampled values.
- K4 (unverified): after an animation completes, the property keeps its final
  value. Probe: sample after completion with no further animation.
- K5 (unverified): `StopAnimation` leaves the property at its current
  animated value. Probe: stop mid-flight and read the held value.

## Sequencing and acceptance

1. Probe harness first (read-only probes, no behavior change), one probe per
   assumption above.
2. Backend implementation behind `ICompositorBackend`, wired to a single
   opt-in demo path; the software path stays the default.
3. Per-assumption verification; enable a compositor path per property only
   after that property's assumptions have recorded evidence.
4. Acceptance: every S/K item has a named probe with recorded evidence on the
   target OS build; planner-vs-compositor sampled values agree within a
   stated tolerance for the spring and keyframe paths; the capability probe
   replaces the all-on defaults; the frozen files (`composition.h`,
   `fm_capi.h/.cpp`, `MotionSystem.cs`, `MotionEtw.cs`) are untouched unless a
   BASE rotation is separately approved.
