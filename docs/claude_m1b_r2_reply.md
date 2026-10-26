## 1. Paste-ready replacement for the interrupt/re-seed section of `claude_m1b_spec.md`

This replaces §3 ("飞行中 retarget 不跳变"). Keep the C-ABI block that follows it.

---

### 3. Interrupt / re-seed of a running spring: status KNOWN-FAIL (R-2, derived from R-1)

**Mechanism.** On a retarget of a lowered spring, the dispatcher sets `InitialValue` and `InitialVelocity` explicitly from the shadow state, `spring.h::At(elapsed + seedLead)`. It then replaces the animation with the 2-argument `StartAnimation`. The compositor is never read back, and velocity always comes from the engine.

**Result under the product mapping (#25, single τ).**

| seedLead | \|dpos\| (px) | \|dvel\| | postRMS |
|---|---|---|---|
| 0 ms (default) | 27.612 | 183.0 | 178.91 |
| 16.7 ms | 20.093 | 156.4 | 179.66 |

Section 4 gates (|dpos| ≤ 0.5 px and ≤ 5% of frame travel; |dvel| ≤ max(20, 5%·|v|)) are missed.

**Status: KNOWN-FAIL, caused by R-1.**
- R-1 is the shape residual between NaturalMotion and `spring.h` under the product mapping (A 28.6%, E 12.1%).
- #31 (below) shows that the interrupt adds no position error beyond what R-1 already has at that instant.
- The measured 20–28 px step is R-1 made visible at the handoff.

**Why Section 4 is not relaxed.**
- The harness linear noise floor is about 0.53–0.68% (#30), so the residual is real, not measurement noise.
- Loosening the gate would turn a measured defect into a pass and hide R-1.
- The fix path is R-1 (the mapping), not the gate.
- Acceptance reports list R-2 as `KNOWN-FAIL (R-1-derived)` with the numbers above.

**Acceptance metric: single-τ only.** τ is fitted on the pre-interrupt window and frozen after the interrupt. Metrics are |dpos|, |dvel| and postRMS as computed in #25.

**Alignment modes.**
- **Mode S (single τ, frozen):** the only mode allowed in a pass/fail table.
- **Mode R (τ refitted per segment):** `DIAGNOSTIC` only. It absorbs commit latency and part of R-1, so it can make a defective handoff look continuous.

**Relation to #11–#12.**
- Their "passed" status stays, but only as a seed-logic regression: the seed equals `spring.h::At`, with R-1 shape not in play.
- They are not evidence of displayed continuity under the product mapping.
- If either run used a refitted τ, label it Mode R.

**seedLead.**
- The default stays 0 ms; 16.7 ms is an experiment flag.
- Do not tune it from #25 or #31. Its apparent benefit moves the seed toward the displayed curve in a direction set by R-1's sign at that instant, so it fits the residual rather than measuring commit latency.
- Revisit only with δ measured under M-5 after R-1 is resolved.

**Incremental-delta diagnostic (#31).**
- `Δpos = |dpos|_interrupt − |residual|_baseline`, with the baseline taken from a non-interrupted run at the seed's content time.
- Reopen R-2 as an interrupt-specific defect if `Δpos > +0.5 px`. The rule is one-sided and signed.
- Result: Δpos = 0.000 at seedLead 0 and −9.517 px at 16.7 ms. Both satisfy the rule.
- Note that −9.517 px is not within ±0.5 px. A negative value is recorded as "seed moved toward the displayed trajectory (confounded with R-1)", never as a pass.
- At seedLead 0, Δpos = 0 holds by construction if the baseline comes from the same fit. State the baseline's provenance in the report.
- Proposed extension, same form: `Δvel` against max(20, 5%·|v|), and `ΔpostRMS` against the non-interrupted run over the same content window.
- #31 covered position only, so "purely R-1-derived" is established for |dpos| only until Δvel and ΔpostRMS are computed.

**Hybrid B moved to Future Work (post-M1b freeze).** B omits `InitialValue` and takes engine velocity. It is not built in M1b. See the paper evaluation.

**Product handling.** Demos do not use a mid-flight retarget as a headline shot. Release note: "Retargeting a hardware-composited spring mid-flight can show a visible position step (20–28 px measured in our test scenario). Known limitation."

---

## 2. Hybrid B paper evaluation (no code)

**What B would change.**
- `InitialValue = null`: #27 measured this as FROM_CURRENT, so the new spring starts from the compositor's own current value at commit.
- `InitialVelocity` stays the shadow velocity `spring.h::At(elapsed + seedLead)`.
- Position at the handoff becomes continuous with the displayed trajectory by construction, if "current" means the animated value.
- Velocity is unchanged: both A and B seed `v_s`, so the velocity step `v_s − v_c` is identical. B cannot flip R-2 to pass the |dvel| gate (183 against max(20, 5%·|v|)).
- postRMS stays dominated by the compositor-versus-shadow shape (R-1); B does not touch it.

**Main risk: the null-`InitialValue` semantics are undocumented.**
- #27 shows what null does, but the summary does not say whether the property had a running animation at the time. That is the case that matters.
- Failure mode: if "current" means the last committed static value, a mid-flight replacement jumps back to a stale start. That is a large jump, against A's bounded R-1-sized step.
- Two further hazards: two retargets inside one commit, and behaviour differing by the type of animation being replaced (keyframe vs spring).
- Architectural cost: the compositor starts at `x_c` while the engine shadow stays at `x_s`, and there is no readback, so the engine cannot see the offset. Any later handoff that uses shadow values (fall back to the app thread, Hold) can reintroduce a step of that size.

**Could it plausibly beat A?** On position, probably: A's |dpos| is entirely R-1 (Δpos = 0), which B removes if null is the animated current. On |dvel| and postRMS, no. So B is a UX mitigation for the visible step, not a way to close R-2. Fixing R-1 would improve A directly. That is why deferral is right.

**Device probe that decides it after M1b.** Prerequisites: G0 passes, screen lit, one Border on `Translation.X`, WGC capture, single-τ alignment.

- **P1 (semantics, shadow-free).** Mid-flight, replace a running spring, and separately a running linear keyframe animation, with a spring whose `InitialValue` is null. Compare the first displayed frames against the old trajectory.
  - Jump ≤ one frame of motion plus the noise floor: null is the animated current.
  - Jump back toward the static start: stale; B is dead.
  - Include a double retarget within one frame.
- **P2 (benefit, only if P1 passes).**
  - Interleave Arm A (explicit seed, lead 0) and Arm B (null `InitialValue`, shadow velocity).
  - Use at least 20 runs per cell, at interrupt content times at high speed, near the first peak and mid-decay, in both directions.
  - Each cell includes a non-interrupted baseline run. Report |dpos|, |dvel|, postRMS and the incremental deltas.
- **Adopt B as a UX mitigation only if:**
  - median |dpos|_B meets the gate, is at most 0.25× |dpos|_A, and the 95% bootstrap intervals do not overlap;
  - |dvel|_B is no worse than A and postRMS_B is within one noise floor of A;
  - the double-retarget case stays within the gate in at least 95% of runs.
- Any failure rejects B. Even if all pass, R-2 stays KNOWN-FAIL on |dvel|.

---

## 3. Agree / Disagree with the Notion verdict

- **R-2 is a known-fail derived from R-1; Section 4 is not relaxed. Agree, with a caveat.**
  - #31 supports this for position only. |dvel| = 183 and postRMS ≈ 179 are not covered; extend the diagnostic to them.
  - Please state the units of postRMS and the travel. If 178.9 is px over a travel near 240 px, that is about 75%, well above R-1's 28.6%, and then "purely R-1-derived" would not hold for the post-interrupt trajectory.
- **Refitted τ is not acceptance, only a labelled diagnostic. Agree.** Add one check: τ fitted on a window that contains R-1's shape mismatch is itself biased. Cross-check it against the E0 pure-lag τ, and report it if the two differ by more than one frame.
- **#11–#12 "passed" is kept. Partial.** Keep them only as seed-logic regressions, and relabel them. If they used a per-segment τ, they are exactly the absorbed-latency metric the verdict now forbids as acceptance.
- **seedLead default stays 0 ms. Agree, with a stronger reason:** the 16.7 ms improvement is confounded with R-1's sign. Choosing it from #25 would tune to the residual.
- **#31 confirms R-2 is purely R-1-derived and "both are ≤ 0.5 px". Disagree with the wording.**
  - −9.517 px is not within ±0.5 px. It is ≤ +0.5 only as a signed value.
  - Define the reopen rule one-sided and record negatives as confounded.
  - Δpos(0) = 0.000 is true by construction if the baseline residual comes from the same fit, so it is independent only if the baseline came from a separate non-interrupted capture. Ask for the provenance.
- **Hybrid B is formally triggered but deferred. Agree.** The stronger basis is that B leaves |dvel| unchanged by construction, so it cannot close R-2, and its shadow-versus-display offset is invisible to the engine. The UX handling (demos avoid mid-flight retarget, release-note limitation) is also right.