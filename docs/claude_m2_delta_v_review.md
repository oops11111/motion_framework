# M2-Δv gate review — Claude English FINAL (INTERIM — no DeepSeek; Free blocked until 4:20 PM Perth; merge pending)

M2-Δv gate review (DRAFT): FINAL

Review only. No sampling, no code changes, composition.h untouched. [M] means measured in a prior closed ticket. [D] means derived by me from reported numbers or the model, and is not new data. [H] means a hypothesis or proposal.

Derivations assume ω0 = 19.635, ζ = 0.78, a single prior spring from rest, and a 60 Hz harness. Cells are given per unit of Δ, plus an illustrative Δ = 240 DIP (the Δ of the real cells is not in the draft). I treat N_h (0.39–0.41 [M]) as a per-frame position σ, which is an assumption [H].

1. Verdict: NEEDS-REVISE, not LOCK-ready

The formula is acceptable. The draft is not testable as written, for these reasons:

The velocity estimator is undefined, and a naive one fails by construction [D].
With a linear fit on each side whose centres are 2 frames apart (33 ms), the true trajectory alone produces |Δv| of 1.19× thr at t060 and 13× thr at t156.
At 4 frames apart those become 2.4× and 26×.
The gate must fix a curvature-corrected estimator (per-side quadratic extrapolated to t_s) and say where v_ref is evaluated.
There is no velocity noise floor (Nv_h).
Position has N_h; velocity has nothing.
A 20 DIP/s floor against two-window noise is 0.83σ at n=3, 1.9σ at n=5, 3.8σ at n=8 [D/H].
Nv_h must be measured on non-interrupted base runs with the same estimator.
“Judgable” is a label, not a rule.
The derived resolvability is mixed up, as the table below shows.
t156 is timing-bound, t300 is floor-bound.
“Median ≤ thresh median” is ambiguous and redundant.
The threshold varies per trial because v_ref varies.
With ≥17/20 trials under threshold, the median ratio is automatically ≤ 1.
n=20 and 2 sessions are weak.
17/20 has a one-sided 95% lower bound of 65.6% [D]. A cell with a true per-trial pass rate of 0.80 passes 17/20 in 41% of sessions, and in 17% of two-session runs [D].
A×3 showed failing subgates rotating across sessions [M], so I’d require 3.
The arms and controls are missing.
The draft doesn’t say which arm is judged.
It needs a positive control (base must pass) and a negative control (see the IV=null+0 note below).
t_s is undefined (“E2/E3-class switch window”). Its timing uncertainty drives the t156 result.
The cell set differs from #47 (four judgeable cells [M]; the draft has five). t300 dbl is not defined for Δv.
Trial validity is undefined: the parked hang, G0/screen-lit, and the capture crop. A1b’s −Δ pseudo-fail was an off-crop artifact [M].
Nothing is pre-registered (script and parameter hash before sampling).

IV=null+0 in the seed bullet. If it means InitialVelocity = 0, then Δv = |v_pre| by construction. That is ≥ 20× the 5% term at t060 and above 20 DIP/s at t300 [D, Δ=240 example]. It cannot be a PASS candidate. Treat it as the negative control. Please fix the IV/IVel notation.

Resolvability by cell [D] (per unit Δ; Δ = 240 example in brackets; |a|σ_ts at σ_ts = 3.5 / 8.3 ms, where 3.5 comes from ε ≈ 14−φ over φ ∈ [1,13] ms [M] treated as uniform):

Cell	v (/s)	a (/s²)	thr (DIP/s)	|a|σ_ts (× thr)
t060	8.42	−15.0	101	0.12× / 0.30×
t156	2.71	−53.5	32.5	1.38× / 3.28×
t300	−0.16	−0.82	20.0 (floor)	0.03× / 0.08×

The relative term binds only above Δ* = 47.5 DIP (t060), 148 (t156) and 2,435 (t300), so t300 is floor-bound for any realistic Δ.

2. Absolute threshold: KEEP
Keep max(20 DIP/s, 5%·|v_ref|). It is the inherited form, and no data supports tightening. Loosening after seeing results moves the goalposts. Add a resolvability rule instead of changing the number. t300’s threshold sits at about half its own velocity at Δ = 240 [D], so its Nv_h must be small to be judgable.

3. t156 Δv-SKIP: ACCEPT, as a rule-derived skip, not a hard-coded label
Physics: deceleration at t156 is about 3.6× that at t060 (|a|), and |a|σ_ts is 1.4× thr already at σ_ts = 3.5 ms [D]. So Δv cannot be resolved there without sub-millisecond t_s, which is a measurement limit, not a seed fault.
Conditions: the rule is pre-registered and confirmed on base runs, and per-trial values are reported.
SKIP is not PASS, and gives no coverage claim about the t156 position failures seen in #47 [M].
J_vis still applies at t156, and no new estimator is built for t156 here (it would reopen the R-2 estimator).
If the rule disagrees with the named classification, bump the gate version. Do not override.
4. Ticket PASS rule (written dead)

The ticket outcome is one of Δv-PASS, Δv-FAIL or Δv-INCONCLUSIVE.

Δv-PASS iff all of these hold in each of ≥3 independent process sessions:
Each of t060 fwd, t060 rev, t300 fwd, t300 rev and t300 dbl is judgable by the pre-registered rule, and has n=20 valid trials with ≥17/20 trials where Δv_i ≤ thr_i.
The positive control (base) PASSes and the negative control (IVel=0) FAILs on both t060 cells.
Δv-FAIL if any session or any required cell fails. FAIL sessions are never replaced or rerun.
Δv-INCONCLUSIVE if any required cell is not judgable, if a control behaves wrongly, or if only t300 cells pass.
An INVALID session (G0 fail, screen not lit, VM, hang cap exceeded) may be replaced, and is logged.
PASS is not adopt, and not evidence for PERF-CI.
5. G2-LOCK (paste-ready)
Scope. M2-Δv. Review ticket only. M2-PERF-CI stays HOLD (LOCKED 5b007dd4…282a; needs ≥2 physical machines). This ticket’s result is not evidence for that gate.
Frozen. composition.h sha256 a55d4952…e418 unchanged (checked before and after every session).
Bans. No S1/R-3/R-2 reopen, no Bv unfreeze, no shadow InitialValue, success ≠ adopt, parked hang stays parked.
Machine. One physical machine (DESKTOP-K93N3P8), 60 Hz, screen lit, G0 passing. VM = INVALID.
Arms. The arm under test is named in the ticket. The gate nominates none. Controls: base (positive) and IVel=0 (negative).
Seeds. InitialValue from WGC/screen-consistent state or null with InitialVelocity 0 (the latter as the negative control only). Shadow InitialValue banned.
Cells. t060 fwd/rev, t156 fwd/rev, t300 fwd/rev/dbl. t156 is a candidate Δv-SKIP only if the rule in item 12 says so. No other cell may be skipped.
Threshold. thr_i = max(20 DIP/s, 0.05·|v_ref,i|), with v_ref,i = the pre-switch velocity at t_s, per trial.
Estimator (frozen before sampling).
Per-side quadratic least-squares fit, window of w frames adjacent to t_s (proposal w=5 [H]), each side extrapolated to t_s.
Δv = |v_post(t_s+) − v_pre(t_s−)|.
w and fit order are chosen on base-run nulls only.
A two-frame difference is not allowed.
t_s. From logged QPC plus the per-trial phase φ; σ_ts is reported. Use it only to place measurement windows.
Nv_h. P95 of Δv computed with the same estimator on non-interrupted base runs at the same content time, per cell, measured before the interrupted arms.
Judgable rule (parameters pre-registered; k below is proposed [H]):
thr ≥ 2·Nv_h.
|a(t_s)|·σ_ts ≤ thr/2.
The values come from base data. A cell failing either is Δv-SKIP and is reported with its numbers.
Trials. n=20 valid trials per cell per session. Trial validity requires G0 and screen lit, no capture drops, t_s detected, v_ref computed, and the full travel inside the capture crop. Hang trials are counted and invalid. A hang count above the pre-registered cap invalidates the session. No selective dropping.
Cell-session PASS. At least 17/20 trials with Δv_i ≤ thr_i. Report r_i = Δv_i/thr_i. No “median ≤ thresh median” clause.
Sessions. At least 3 independent process sessions. Each must pass. No pooling, no reruns to flip a result.
Ticket rule. As section 4 above.
J_vis. The #47 gate J_vis ≤ d_frame + N_h_eff is reported for all cells including t156, separately.
Reporting. Per-trial rows (φ, t_s, v_pre, v_post, thr, Δv, r, validity), SKIP reasons, control outcomes, session metadata. No outlier trimming unless pre-registered.
Pre-registration. Hash of analysis script, cell list, window, rule parameters and thresholds recorded before sampling. Any change after sampling is a new gate version.
Status. DRAFT until LOCK + APPROVED. No M3 live.
6. STOP callouts
Any seed or advance tuned to pass this gate (ε̂, seedLead, φ-estimators). That is S1/R-3 reopened.
Building a new Δv estimator for t156, or re-deriving R-2’s. The draft bans it.
Using the hang, c−φ, or the shadow as a seed, or anything from this ticket as a seed source.
Any compile-time threshold or constant in composition.h (constants belong in the harness config).
Presenting a PASS as an adoption, an unfreeze of Bv, or PERF-CI evidence, or running on a VM.
Replacing a FAIL session, or dropping hang trials selectively.
Using Δv_fd medians from #50 (Sup 507.0, Bv 1046.9, base 56.8) as expectations. That was a frame-difference proxy [M], not this estimator.
7. Holes DeepSeek must close before LOCK
 Estimator: fit order, window w, the evaluation point, and how the post window avoids contamination by post-switch acceleration.
 Nv_h: procedure and a per-cell number from base runs.
 Judgable rule: exact definitions and the value of k. Confirm whether it reproduces t156 = SKIP and t300 = judgable.
 t_s and σ_ts: the definition of “E2/E3-class switch window”, and how φ is used.
 v_ref: definition per trial, including at the second switch of t300 dbl.
 Cell set: reconcile with #47 (four judgeable cells). Define t300 dbl for Δv.
 Arms and controls: which arms are judged, and the IV/IVel notation in the seed bullet.
 Sessions: the count and the pass rule (my proposal is 3 and no pooling).
 Trial validity and the hang cap, and the screen/crop conditions.
 PASS wording: INCONCLUSIVE handling and “PASS ≠ adopt” in the ticket text.
 Pre-registration hash procedure.
