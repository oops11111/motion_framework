Source: Notion AI fallback (Claude Free blocked). Not Claude-authored. Same [M]/[D]/[H] style as Claude re-reviews.

# R-3 #52 close — FINAL

**Verdict:** R-3 CLOSED; S1 STOP/NO-ADOPT; Bv degraded fallback (not a qualified baseline).

#52 ONELINE (verbatim):
`#52: P1=分开(pooled; a1分不开); A v2×3=2/3; P3=F5确认; ≈30c=无; 硬停=A×3<3/3→STOP S1; sha OK`

Prior hard-stop authority: docs/claude_r3_51_review.md (Notion AI fallback FINAL).
User confirmed R-3 CLOSED on 2026-10-24 (Australia/Perth). Notion / Gemini / DeepSeek #52 reviews all agreed: close R-3, keep Bv.

## Findings

- [M] A v2×3 = 2/3. Only a3 FAILs: A1a @ τ0=21.8, median RMS=0.781. A1b/A1c/A2 = 3/3. Gates were locked before data.
- [M] Hard stop triggered: A v2×3 < 3/3 → STOP S1.
- [M] P3 = CONFIRM_F5_REFUTE_2X; med_res_unsat=+2.70; SSE_2x/SSE_const=14.5×.
- [M] P1: pooled separates (n=36, purity=0.83); a1 alone does not separate (purity=0.50); tickPer≈16.66 ms.
- [M] ≈30 c mode: none in three Stage-B sessions.
- [D] Close follows the pre-locked hard-stop rule. It is not a judgment that came from re-reviewing the data.
- [D] Success ≠ adopt. J_vis alone is not grounds to adopt. Pooled P1 separation does not reverse the S1 STOP.
- [H] The single-run a3 failure (A1a) may be a separate tracking issue, not a defect in S1 as a whole. This is unverified and does not affect the close.

## Three hang-accounts

1. **Close reason = gate trigger.** A v2×3=2/3 < 3/3 → STOP S1. This is NOT a conclusion from re-reviewing the data.
2. **Bv = degraded fallback, NOT a qualified baseline.** Open items that stay with Bv:
   - P1: a1 does not separate; pooled purity=0.83.
   - P3: CONFIRM_F5_REFUTE_2X (med_res=+2.70; SSE 14.5×).
   - ≈30c = none.
   - S2 not revived.
   - No #49 compare.
   - NOT a ship basis.
3. **A1a tracking spun out.** a3 @ τ0=21.8, median RMS=0.781. This item does NOT go away when R-3 closes.

## Default

Do not reopen S1.

## Optional — suggested next gate (A1a tracker only)

Suggestion only. This does NOT authorize a new probe.
- Any future A1a work runs under its own ticket, separate from R-3.
- Lock its pass/fail gate before data, as #52 did.
- Scope it to the a3 failure (τ0=21.8, median RMS=0.781).
- A pass does not reopen S1 or upgrade Bv, and does not imply adoption.

## Discipline

- composition.h unchanged: sha256 a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418
- Same-session discipline: gates locked before data. Verdict uses only #52 Stage-B measurements; no cross-run mixing.
- S2 not revived.
- No cross-compare to #49.
