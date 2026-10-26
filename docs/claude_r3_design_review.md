Before reviewing, I'll check the "time shift" argument and the claimed second-order sensitivity with the closed-form step response. I'll use only ω0 ≈ 19.635 and ζ = 0.78 from the brief, and the ε values from #49.

# R-3 redesign review: S1 (Sup-adv)

## 1. Verdict on S1: REVISE (keep as priority 1 for #50, but not as written)

**Why it stays on the list.**
- #49 shows plain Sup fixes the staleness (J median 1.55 vs Bv 25.86; Δv_fd 130.7 vs 716.8) and leaves a response lag (ε median 21.8 ms > 16.7 ms).
- Time-advancing the step term is the first-principles remedy for a lag of that kind.

**What is wrong or missing in the draft (details in 2–5).**
1. The sensitivity claim is wrong. Residual is first-order in the ε̂ error, not second-order.
2. Seeding trades the lag for a deliberate landing hop and velocity kink.
3. The proposed "≤ 2× same-batch Sup" gates misjudge S1, because Sup starts at rest and S1 lands on a seeded state by design.
4. S1-φ rests on a two-bin fit, and E1's phase model was not validated in #48.
5. ε̂ is calibrated on a configuration that S2 would change.

One notation fix: the expression must be `base + d0 + d1`, with `d0` the old spring still running. The draft writes `b + d1`.

## 2. The "state seed = time shift" argument and its sensitivity

**The identity holds.** For the isolated term with the same (Period, ζ), seeding at (s(ε̂), s′(ε̂)) gives s(t+ε̂) exactly. I checked it with the closed form at ω0 = 19.635, ζ = 0.78: worst difference 3.3e-16 per unit Δ.

**The second-order sensitivity claim does not hold.** The draft's ½ω²Δe² is the landing position for a seed of size e when the true lag is zero. With true lag ε, a seed error e = ε̂ − ε is a time shift of the whole step, and its position residual is s′(t)·e, first-order in e.

Per unit Δ, with true lag ε = 21.8 ms (derived from the closed form, not measured):

| Seed error e | Worst deviation over the step | Draft's ½ω²e² |
|---|---|---|
| ±5 ms | 0.042 | 0.0048 |
| −10 ms | 0.084 | 0.019 |
| +8 ms | 0.068 | 0.012 |

The draft underestimates the residual by about 9× at ±5 ms. The coefficient is the step's peak speed, 8.45/s × Δ, which is the same kind of first-order dependence as Bv's v·e. S1 helps only to the extent that ε is predictable.

**Expected benefit, bounded by the evidence.**
- Plain Sup leaves a worst deviation of 0.093 / 0.141 / 0.183 / 0.250 of Δ at ε = 11 / 16.7 / 21.8 / 30 ms.
- With a constant ε̂ = 21.8 ms, the e span implied by #49's two bins (ε ≈ 11–30 ms) gives roughly 0.07–0.09 of Δ.
- That is about a 2× reduction in lag error, not elimination. It rests on two bins and is an expectation, not a measurement.

**The landing step is real, and the draft's quadratic form overstates it.** At ε̂ = 21.8 ms the exact landing position is s = 0.073 Δ (the quadratic gives 0.092 Δ) and the velocity step is 5.95·Δ DIP/s. Per unit Δ, ε̂ = 16.7 ms gives 0.045 Δ and 4.95·Δ DIP/s.

## 3. NaturalMotion vs the analytic spring: is P0-fit necessary and sufficient?

**Necessary, not sufficient.**
- The earlier 0.06–0.21% residual was for a rest-started spring on the visual. S1 relies on a PropertySet scalar with a seeded start (IV≠0, IVel≠0, FV=Δ), and #48's P0 checks only covered IV=FV=0 with IVel.
- Whether the compositor integrates in closed form or in steps is undocumented, so the shift identity may only hold approximately on device.

**Additions to P0-fit:**
- A seeded-start shift test: start d1 with IV = s(τ0) and IVel = s′(τ0) for several τ0, and fit each WGC curve to the rest-started curve shifted by τ0, with a single τ per run.
- Δ linearity in magnitude and sign.
- d1 started while d0 is running, with the expression summing both.
- A gate built on the per-cell N_h from #47, not a new constant.
- Large residual means stop the main matrix, as the draft says.

## 4. Is the #50 matrix and gate set sufficient?

**Not as written. Changes:**
1. **Gates for S1.** Keep J_vis ≤ d_frame + N_h_eff from #47 as the ship gate. Treat "≤ 2× Sup" only as a screening proxy, and do not use it for S1.
   - The velocity-step arithmetic suggests it would fail S1 for large Δ. If Δv_fd measures the same kind of step, 2 × 130.7 = 261.4 and 261.4 / 5.95 ≈ 44, so it passes only for Δ up to about 44 DIP. That depends on how Δv_fd is defined; I don't have the Δ of the #49 t060 cell.
   - Add residual against the ideal retargeted trajectory (position and velocity at landing and over the next frames), signed, as the primary S1 metric.
2. **ε gate.** "ε median ≤ 16.7 ms" is a Sup-era gate. For S1 report the residual lag e: signed, median, P90, and per-φ maximum. Report over-advance (e>0) and under-advance (e<0) separately. Over-advance can amplify overshoot when the new target is close ahead.
3. **ε̂ sweep.** Run at least ε̂ ∈ {0 (= Sup), 16.7 ms (one frame), 21.8 ms} so the lag-versus-hop trade-off is visible. Capping the advance at one frame bounds the landing hop.
4. **Cells.** One cell (t060, n≈64) cannot support a ship decision. Add at least one cell with different speed and one with a reversal (Δ sign flipped).
5. **φ coverage and hold-out.**
   - #49 had only two informative bins, and a line through two points has no residual to check.
   - Design the landing phases so each bin has enough runs, fit on one batch, and evaluate on a fresh one.
   - Test S1-φ with the phase estimate the product could actually have (for example from `DwmFlush` ticks), not the harness's logged φ. Otherwise a pass does not transfer to shipping.
6. **ε definition.** Confirm the pre-interrupt spring in the Sup/S1 arm runs through the same PropertySet and expression path as d1. If it ran directly on the visual, the frozen τ and ε include a path-latency difference, not only commit-phase jitter.
7. **Multi-retarget.** The fold-into-base step is not covered by #50. Either add a 3-retarget chain or state the cap on live terms as a ship limitation.
8. **Reporting.** Raw per-trial rows (φ, ε, e, J, Δv, N_h_eff), interleaved randomized arms, and a non-interrupted baseline per cell.

## 5. Ordering and blockers

- **S2 is a prerequisite for calibrating S1, not an orthogonal option.** If expression-slot bind latency is part of ε, then ε̂ must be re-measured after S2. Run Sup+S2 first, then S1 on top of it.
- **S1 > S4 (E2):** the demotion stands, since #48 found weak phase dependence and σ ≫ 2 ms.
- **S3** needs a compositor-side latch of the retarget time, so that the lag cancels. No probe has shown that is possible, so keep it as research.
- **S5** stays the fallback and does not close R-3.

**Blockers before workbuddy runs #50:**
1. The extended P0-fit, with the seeded-start shift test.
2. The Δ and the d_frame of the #49 t060 cell.
3. The ε definition check (item 6).
4. The replaced gates and the multi-cell design.

## FINAL (paste into `docs/claude_r3_design_review.md`)

**Verdict.** S1 (Sup-adv) is REVISE: keep it as priority 1 for #50, but not as written. The state-seed time-shift identity holds (closed form, 3.3e-16). The claimed second-order sensitivity does not: residual is first-order in the ε̂ error, with peak step speed (8.45/s × Δ) as coefficient. S1 converts response lag into a deliberate landing hop (7.3% of Δ and 5.95·Δ DIP/s at ε̂ = 21.8 ms), so the screening gates must change.

**#50: conditional GO.**
- P0-fit comes first and gates the main matrix. It must include seeded-start shift tests, Δ linearity, and d1 under a running d0.
- The matrix changes: ε̂ sweep, residual-versus-ideal as the primary S1 metric (J_vis ≤ d_frame + N_h_eff as the ship gate), more than one cell, φ hold-out with an estimable phase source, and signed e reporting.
- Run Sup+S2 before calibrating ε̂.

**Caveats.**
- S1-φ rests on two informative bins.
- NaturalMotion's integration scheme is undocumented.
- Over-advance carries overshoot risk.
- Multi-retarget accumulation is untested.

Derived numbers use ω0 = 19.635, ζ = 0.78 and #49's ε values, and are marked as derived. Thresholds are proposed, not measured.

**R-3 stays OPEN.** No variant is adopted; the Bv-degraded fallback and the R-2 supersession note are unchanged.