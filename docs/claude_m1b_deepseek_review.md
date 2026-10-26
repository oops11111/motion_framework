> **FINAL** — Claude Free · 2026-10-20 Asia/Taipei（约 14:54）
> 依据：Notion 草稿 + 派题 #14–#22 真机更新；四点全部 Agree。
> 原文英文如下；`claude_m1b_spec.md` 已按文内 edits 清单修订为 rev4。

# DeepSeek M1b review — FINAL

I made no file edits. The deltas below are for you to apply.

## Point 1 — Period / DampingRatio mapping: Agree (settle H1), with three corrections to the Notion draft

**Verdict.** `Period = response`, `ζ_c = ζ` is settled as the product mapping. H2 predicts a 0.32 s damped period and H3 predicts about 3.2 s, against about 0.511 s observed as relayed, so both are rejected. For ζ≥1, `Period` stays at `response`, with no special case.

**Corrections:**
1. **What "settled" means here.**
   - The observed 0.511 s is one number. `T_d = T₀/√(1−ζ²)` has two unknowns, so (T₀=0.25, ζ=0.872) gives the same value.
   - At ζ=0.78 the second extremum is about 0.095 px, so it cannot have been a measured peak-to-peak interval.
   - So "settled" means chosen and unrefuted, not jointly identified. The Notion draft dropped this caveat from my earlier check.
2. **Quote the shipped residual, not the best-of-sweep one.**
   - Under the shipped mapping the spring-shape RMS is 28.6% (scenario A) and 12.1% (scenario E).
   - The 8% and 6% figures belong to non-product variants such as T₀=0.1512. Please do not present them as the product residual.
3. **My earlier objection to "inherent difference" is now mostly resolved.**
   - K1 reports 0.57% RMS on a keyframe shape. If that used the same alignment pipeline at comparable speed, the harness floor is low and the spring residual is real.
   - So closing it as an accepted risk is acceptable. The ≤1% gate stays unrelaxed and is reported as known-fail. Do not tune `Period` or ζ to reach it.

## Point 2 — Interrupt seeding: Agree (explicit seed), but the Notion rationale is partly wrong

**Verdict.** Always set `InitialValue` and `InitialVelocity` explicitly from `spring.h::At(elapsed + seedLead)` on the shadow state. Demote only hybrid B.

**Corrections to Notion:**
- The engine velocity does not come from the compositor, so `TryGetScalar` being static is irrelevant. It comes from the C++ engine.
- The real reasons to demote B are that it relies on documentation-silent null-`InitialValue` semantics, and that its failure mode is a large jump to a stale start. The explicit seed's failure mode is only a small δ·v error.
- A (explicit seed plus lookahead) is not demoted. It is the plan.
- The `TryGetScalar` finding is workbuddy's observation, not mine.

**Consequence of Point 1.** The shadow-versus-compositor residual (12–28% RMS) is the error budget of explicit seeding, so interrupt continuity must be measured against the displayed trajectory (see Open).

## Point 3 — `TryStartAnimation` vs `StartAnimation`+`TryGet`: Agree

**Verdict.** `TryStartAnimation` does not exist. The `CompositionObject` member list has `StartAnimation` (2- and 3-argument), the group variants, `StopAnimation` and `TryGetAnimationController` only. For springs, `TryGetAnimationController` is always null (workbuddy), so springs use plain `StartAnimation` and re-seed.

For KeyFrame, prefer the controller-first pattern: `CreateAnimationController()` plus the 3-argument `StartAnimation`. Use `TryGet` after `Start` only as a fallback.

K3 is only partly confirmed:
- `Progress` is time-normalized, which matches `elapsed/duration` in our time model.
- `PlaybackRate` 2 and −1 work.
- The keyframe capability flags stay false until the rest is verified (see Open).

## Point 4 — ζ>1: Agree (v1 does not lower)

**Verdict.** The API accepts ζ≥0 without clamping (1.0/1.2/2.0/5.0 read back ≈ set), ζ<0 throws, and overdamped animations run.

**Caveats:**
- The evidence is API-level acceptance only. Trajectory fidelity is unmeasured.
- The gate is landed. I have not seen the diff, so I am relying on your description: `c.damping > 1.0` after `ToCompositor`, with ζ=0.8→`CompositorSpring`, ζ=1.2→`AppThread` and composition_tests 2073/0. Checking after the mapping is right.
- To lift the gate later, convert it to `Capabilities::overdampedSpring` and pass the trajectory checks below.

## `claude_m1b_spec.md` edits

- **§2.1:**
  - Heading "工作映射…" → "**H1 settled:** `Period = response`, `ζ_c = ζ` (`SpringMapping` scales 1.0)."
  - H2 and H3 rows → "rejected: predict 0.32 s and ≈3.2 s vs ≈0.511 s observed."
  - "但这不是定案" paragraph → "Evidence limits (record only; do not reopen the mapping)." Keep the four numbers.
  - The low-ζ identification protocol becomes an optional diagnostic, not a gate.
- **§1 table and §4 M-1/M-1b:**
  - M-1 becomes "report only: record spring-shape RMS under the shipped mapping per scenario."
  - M-1b keeps the gate (≤1%·travel), with the note "known-fail, accepted risk R-1: A 28.6% / E 12.1% shipped; best swept variants 8% / 6%; do not tune to pass."
- **§3:**
  - "B 降为实验开关" → "B not built in M1b; documented alternative only; revisit if A's interrupt residual against the displayed trajectory fails."
  - Add: "Seed = `spring.h::At(elapsed + seedLead)` for both `InitialValue` and `InitialVelocity`. Velocity comes from the engine, never the compositor."
- **§2.3 and §1:**
  - Add: "`TryStartAnimation` does not exist; NaturalMotion `TryGetAnimationController` is always null [device]."
  - K3 row: "Progress immediate and time-normalized; PlaybackRate 2/−1 work; Progress+Delay and |rate|>16 untested; keyframe flags stay false."
- **§2.4:**
  - Replace the capability-flag paragraph with: "Landed as a hard gate in `composition.h` (see Point 4). Device: ζ≥0 accepted and not clamped, ζ<0 throws, overdamped animation runs. v1 does not lower ζ>1. To lift it: convert to `overdampedSpring` and pass T-S2 (monotone, no overshoot >0.5 px, settle within 15% of model, ζ=1.5 vs 3.0 curves differ)."
- **§1 facts, §2.2 and §0:**
  - S3 → "`InitialVelocity` is DIP per real second, independent of `Period` [device]."
  - Add rows: S4 pass; S5 holds the final value, a mid-flight Stop freezes, and `Completed` is late; K1 RMS ≈0.57%; K4 final value held; K5 "Stop freezes, `TryGet` unreliable, use Stop+`SetValue`."
  - K2: y∉[0,1] honored, so `bezierOvershoot=true` replaces "default false until T-K2 passes".
- **§7.1:**
  - B4 becomes "confirmed more strongly: on a lit screen `Completed` is late, not early, so the 14–30 ms early completion was a sleeping-display artifact; B1–B3 unsupported."

## Open / needs measurement

1. Interrupt continuity against the displayed trajectory, using one time offset for the whole run under the shipped mapping.
   - Includes δ and the `seedLead` value.
   - The interrupt numbers relayed earlier were not in the material I was given.
2. ζ>1 trajectory fidelity (monotonic, settle time, ζ=1.5 vs 3.0).
3. Spring rate-fold shape: the 13.6% spot RMS cannot be separated from the base residual. It needs a rate=1 control in the same run. v1 scenarios should keep rate=1 until then.
4. K3 remainder: `Progress` combined with `DelayTime`, |rate|>16, and reverse with `Progress`.
5. Harness noise floor at spring speeds (the 0.25 s linear calibration). K1's 0.57% is only a proxy at an unknown speed.
6. Confirm the landed gate boundary (`> 1.0` strict) against the diff.

## Summary

H1 (`Period=response`, `ζ_c=ζ`) is settled and H2/H3 are rejected; explicit shadow seeding replaces hybrid B; no `TryStartAnimation` exists and springs have no controller; ζ>1 is not lowered in v1 (the gate is landed). The shipped spring-shape residual is 28.6%/12.1% (not 8%/6%), and it is the error budget for interrupt continuity, which is still unmeasured against the displayed trajectory.
