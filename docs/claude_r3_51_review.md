# R-3 #51 review — FINAL (paste into docs/claude_r3_51_review.md)

**Status.** R-3 stays OPEN. S1 seeding (ε̂ > 0) is NO-ADOPT and paused. Plain Sup (ε̂ = 0) becomes the lead candidate to replace Bv, but is not adoptable yet. Bv remains the degraded shipped fallback. composition.h is unchanged (sha256 a55d4952…e418). Tags: [M] measured, [D] derived by me from the reported numbers, [H] hypothesis.

**1. Verdict**
- **S1 seeding.** Stop pursuing it. It fails the seeded-start identity check at the S1 operating seeds (A1a FAIL at 3.3, 6.8, 8.3 ms) [M]. At ε̂ = 6.8 it did not beat Sup's mean |e| in #50 [D], and the applied advance is not controllable (Q3).
- **Plain Sup against Bv, same session (#50 Stage B, tInt = 60) [M].**

| Arm | J med | Δv_fd med | ε med |
|---|---|---|---|
| Sup | 0.26 | 507.0 | 7.0 ms |
| Bv | 16.09 | 1046.9 | 8.5 ms |

  - That is about 62× on J and 2.1× on Δv_fd [D].
  - Sup passes J_vis in all four Stage C cells at ε̂ = 0 [M]. Bv failed at t060 and t156 in #47 [M].
- **Why Sup is still not adopted.**
  - A is unstable, and A1c is the subgate that covers its two-term stack.
  - Multi-retarget behaviour (fold, term cap) is untested, and composition.h has no term or fold support.
  - There is no "strong ship case" yet.

**2. Does A×3 = 0/3 permanently block? No, but this is a gate redesign, not a relaxation.**
- **Part of the failures are artifacts.** The ε_fit = −5 values were search-bound hits [M]. After widening, the d0+d1 fit is bimodal at {+6.5, −10} ms [M]. The two modes are 16.5 ms apart, about one frame period [D].
- **Likely cause [H].** The step lands on one of two adjacent frames, so a unimodal single-τ fit with a narrow bound fails depending on which phases n=5 happens to sample. That would explain the rotating subgates.
- **A retraction.** My #50 note that failures occur at non-multiples of 16.7 ms is not supported. 33.3 fails in A1a [M], and the failing τ0 set differs between #50 and #51.
- **Redesign (proposed):**
  - Search bound of at least ±1.5 frames; per-trial fit with a landing-frame term (k ∈ {0, 1}) or stratify by φ.
  - n ≥ 12 per subgate, instead of n=5.
  - Pass level stays at N_h; only the fit design changes. Report the fitted shift against the predicted c − φ.
  - Gate Sup on A1a at τ0 = 0, A1c, A1b and A2 only. A1a at τ0 > 0 belongs to S1 and drops out while S1 is paused.
  - Keep the A1b re-crop at origin 220, which passes.
- **Still unknown.** The A1a failures at 3.3, 6.8, 8.3 and 33.3 may also be bound-hit. Check them with the same widened refit.

**3. Part1 against the "≈2× advance" hypothesis**
- **Refuted in its multiplicative form for the isolated seeded start.** The unsaturated median of (eff − τ0) is ≈ +2.7 ms [M]. A 2× advance would put it near τ0 itself. The excess is an additive offset, not proportional.
- **Stage C shows a bigger excess.** With ε = 14 − φ, the effective advance at ε̂ = 6.8 is 13.8 / 13.8 / 13.3 / 12.8 ms for φ = 1 / 5 / 9 / 13 [D]. That is about 13.4 mean, or about 2× by coincidence.
- **About 3.9 ms stays unexplained.** After the +2.7 from Part1, the remaining excess is 3.3–4.3 ms (mean 3.9) [D]. Candidates [H]: interaction with the running d0, or the e definition in Stage C.
- **Cannot yet separate additive from multiplicative in the interrupt case.** Stage C had one ε̂ with a φ breakdown. The ε̂ = 16.7 result was reported only as a lead range of ~20.7–21.7 without the per-φ values. Please supply e by φ at ε̂ = 16.7; additive and multiplicative offsets predict different slopes.
- **Needs confirming.** The definitions of δ0 = 39.5 (I read it as a saturation level) and of eff.

**4. Part2 (c locked at about 14)**
- **Result [M].** c is 14.00 / 13.89 / 13.90 across three sessions, with ε ≈ 14 − φ and no ≈ 30 mode.
- **What it clears.** The cross-session toggle between ≈14 and ≈30 was not reproduced under the current configuration, on this machine, at 60 Hz.
- **What it does not clear.**
  - The earlier larger ε values from #49 stay unexplained rather than cleared.
  - c is a pipeline constant with no runtime readback. It would need one-time calibration per machine, driver and refresh rate.
  - Do not extend it to 120 Hz, since P changes and c is untested there.

**5. Next probe: #52, Sup-only validity and multi-retarget (S1 seeding paused)**
- **A redesign** from item 2, three sessions.
- **Multi-retarget chains** of 2, 3 and 5 retargets (spacing proposed, not measured). Per retarget report J_vis, Δv and residual. Include the fold step and a live-term cap, compared to Bv in the same session.
- **More cells:** t040 and t060 (b0, b120, reversal), plus t156 for the cell where Bv failed in #47, and t300 as a slow control.
- **No composition.h change.** Shipping Sup needs a separate design review for term lists, fold and the gate.
- **Strong ship case needs all of:**
  - A redesign passes in three sessions.
  - Sup passes J_vis in the cells where Bv fails.
  - The multi-retarget chain stays inside the gate or has a defined fallback rule.
  - No regression in Δv against Bv.

**Hard stops for #52**
- No ship configuration change.
- No composition.h change.
- No adoption from J_vis alone.
- No comparison with #49.
- No generalization of c to other refresh rates.
- Same-session interleaving of Sup, Bv and the baseline.
- Log G0 and screen-lit state per run.

**Caveats.** The derived values come from your reported figures. Only four φ-bins exist. The bimodality and frame-landing explanation is a hypothesis. Several definitions (δ0, eff, e, the ε_fit bound) need confirming.