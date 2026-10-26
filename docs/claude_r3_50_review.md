Not rate-limited. R-3 stays OPEN.

# R-3 #50 re-review

**Tags:** [M] = measured in #50, [D] = derived by me from your reported numbers (arithmetic only, no new data), [H] = hypothesis, untested.

## 1. Verdict on S1/#50: NO-ADOPT now; REVISE the program

The constant-ε̂ result is not a basis to adopt. The reasons below come from the #50 numbers.

1. **J_vis cannot rank candidates.**
   - It passes for all three ε̂ in all four cells, including ε̂=0 [M]. Plain Sup alone has J=0.26 [M].
   - At ε̂=16.7 the residual P90 reaches ~6.4 against 0.3–0.45 at ε̂=6.8 [M], yet the gate still passes.
   - So "J_vis = YES" says no candidate lands with a snap. It does not say S1 is better than Sup.
2. **The constant ε̂ buys no net accuracy.**
   - Per-φ lag at ε̂=0 is −13/−9/−5/−1 ms, mean |e| 7.0 [D].
   - At ε̂=6.8 the signed e is +0.8/+4.8/+8.3/+11.8, mean |e| 6.4 [D].
   - It turns lag into lead at high φ, and the lead is the riskier sign (overshoot).
3. **Seeding appears to advance about twice as much as commanded [D, H].**
   - From Stage B, ε(φ) = 13/9/5/1 at φ = 1/5/9/13, which fits ε = 14 − φ exactly. That is the sawtooth.
   - The design predicts e = ε̂ − ε = −6.2/−2.2/+1.8/+5.8 at ε̂=6.8. Reported minus predicted is +7.0/+7.0/+6.5/+6.0, about ε̂ itself.
   - So the data fit e ≈ 2ε̂ − ε. The reported e definition needs confirming before this is trusted.
4. **ε is not stable across sessions.**
   - Sup ε median was 21.8 ms in #49 and is 7.0 in #50 (3.1×).
   - Sup Δv_fd went 130.7 → 507.0 and Bv J went 25.86 → 16.09.
   - #49's φ-bins imply c ≈ 31 (φ=1) and about 24–30 (φ=13), against c = 14 now, a shift of about one frame [D].
   - Neither a constant ε̂ nor ε̂(φ) can ship until it is known whether the pipeline toggles by a frame. There is no runtime readback of c.
5. **S2 is not needed.** Sup and SupS2 differ by 0.8 ms and agree per φ [M], so bind latency is not the lag source. Drop S2 from the critical path.

## 2. Does the unstable Stage A block a ship decision even with J_vis YES? Yes.

Stage A is the validity precondition for the "seed = time shift" mechanism.
- **A1a fails at exactly the frame-non-multiple τ0.** Seeds at 0, 16.7, 33.3 and 50 ms pass (the multiples of 16.7), and 21.8 fails 3 of 5 (RMS 1.82–1.96, about 4.4–4.8× N_h) [D]. S1 only operates at non-multiples (6.8, 21.8, ε̂(φ)). The pattern points to frame-phase interplay rather than smooth model error [H].
- **A1c has the same signature.** 3 of 5 fail at RMS ≈1.76–1.79 with ε_fit=−5. If −5 is a search bound, the failures may be a fit artifact. Check by refitting the stored raw data.
- **A1b leaves reversal untested.** +Δ fails (RMS 2.6 and 2.0), and −Δ is invalid because it moved off the capture crop (WGC cx0≈100), not a fail. Reversal is the common retarget direction.
- **The AB-only control passed while full raw failed the same day.** Treat A as unstable until a same-config repeat shows otherwise.

## 3. Is ε̂=6.8 acceptable, or cap at 0 or one frame?

Neither as a shipped constant.
- A one-frame cap (16.7 ms) does not bind at 6.8 ms. Capping at 0 is plain Sup.
- Don't ship 6.8: it leads in all four φ bins, its mean |e| is no better than Sup's, its effect is unexplained (the ≈2× advance), and c may be session-specific.
- If an interim change from the degraded Bv is wanted, plain Sup is the lower-risk candidate on #50 data. #49 disagreed (ε 21.8), so it needs a cross-session repeat first.

## 4. Next probe

Order, and what can run together:
1. **#51 Part 1, A-stability plus effective-advance calibration.**
   - First refit the stored raw frames with a widened τ bound, and check whether ε_fit=−5 hit it.
   - A1a on a fine τ0 grid, with frame multiples and non-multiples, and at least 5 repeats per value.
   - Report the fitted shift minus the commanded τ0 (effective advance), not only the shape RMS.
   - Re-crop A1b so −Δ stays on screen.
2. **#51 Part 2, c-stability.** Repeat the Sup ε(φ) bins across at least three independent sessions or load states, interleaved with Bv and base, to see whether c toggles between ≈14 and ≈30.
3. **Only after (1) resolves the advance factor and (2) shows c is stable or estimable:** S1-φ with a product-estimable φ (a `DwmFlush` tick ring buffer), fit and hold-out on separate batches.
4. **Multi-retarget** stays deferred. Keep Bv as the shipped fallback.

## 5. Hard stops before #51
- No change to composition.h (sha must match) and no ship configuration change.
- No adoption claim from J_vis alone.
- Confirm the definitions of e, ε, J, Δv_fd and resid in the one-line summary.
- Interleave arms within one session, always including Sup, Bv and base.
- Log G0 and screen-lit state per run.
- Same-session numbers only; do not compare to #49.

## FINAL (paste into docs/claude_r3_50_review.md)

**Verdict.** S1 constant-ε̂ is NO-ADOPT; the S1 program is REVISE. J_vis passes for every candidate including ε̂=0 and Sup (J 0.26), so it does not discriminate. At ε̂=6.8 mean |e| is 6.4 against Sup's 7.0 (derived from reported numbers), so lag turns into lead without a net gain. R-3 stays OPEN.

**Findings.**
- Stage B ε(φ) fits 14 − φ ms exactly across 4 bins (derived), so the lag is frame-phase quantization.
- #49's φ-bins imply a c about one frame larger (derived). ε is not stable across sessions: Sup ε 21.8 → 7.0 ms.
- At ε̂=6.8 the reported lead exceeds the design value by about ε̂ (derived); seeding appears to advance about 2× what is commanded (hypothesis, e definition to be confirmed).
- Stage A: A1a fails only at the non-frame-multiple seed 21.8 ms (3 of 5); A1c fails 3 of 5 with ε_fit=−5; A1b −Δ is invalid (off crop), +Δ fails. A is unstable and blocks any adoption.
- S2 is not needed (Sup ≈ SupS2, |Δε|=0.8 ms).

**#51: GO, with these conditions.**
- Part 1: refit raw with a widened τ bound; A1a fine-grid with effective-advance reporting; fix the A1b crop.
- Part 2: Sup ε(φ) across at least three sessions.
- S1-φ only if the advance factor is resolved and c is stable or estimable.

**Hard stops.** composition.h unchanged; no ship change; no J_vis-only adoption; same-session interleaving; definitions confirmed.

**Caveats.** All derived numbers come from your reported figures. Only 4 φ-bins in #50. The #49-versus-#50 comparison has unknown condition differences. The frame-multiple pattern is a hypothesis.

**R-3 stays OPEN.** Bv remains the shipped degraded fallback; the R-2 supersession note is unchanged.