# M2-Δv′ 闸门补洞决断（可 LOCK）

> DECISION_AT=2026-10-26 09:05:37 AWST（Australia/Perth）  
> 依据：`docs/gemini_m2_delta_v_prime_design.md`；`docs/deepseek_m2_delta_v_prime_review.md`（Harness 空回代拟，文首已标）；`docs/notion_m2_delta_v_pack.md`；前序 LOCKED `d2349ceb…09c6` **不改**  
> 完整可 LOCK 正文：`docs/m2_delta_v_prime_gate_DRAFT.md`（本刀覆盖）→ LOCKED `docs/m2_delta_v_prime_gate_LOCKED.md`  
> **不授权采数**；单机即可；PERF-CI 仍 HOLD。

## 逐条决断

| ID | 项 | 决断 |
|---|---|---|
| P1 | t060 timing 可判 | **采纳 Gemini**：`|a|·σ_ts ≤ thr`（去 /2）；σ 预登记（本机默认 `1/60/√12`） |
| P2 | t300 SNR 可判 | **采纳 Gemini**：`thr ≥ Nv_h_cell`（去 2×）；Nv_h>thr → 诚实 SKIP |
| 虚 PASS | DeepSeek 反例 A/B/C | **采纳保留**：逐样本 `Nv_h≤thr_i`；≥17/20；FAIL-确定；IVel0 t060 负对照；**禁**放宽 17/20 |
| P3 | base 正对照 | **采纳**：可判后 base<17 → ctrl FAIL 挡 PASS；t060fwd 真机已可达；**禁**为救 base 改 thr/17 |
| P4 | FAIL vs INCONCLUSIVE | **采纳**：可判且 WGC 差 → **Δv-FAIL**；INCONCLUSIVE 仅不可判/证据不足 |
| P5 | IVel0·t300dbl 崩 | **采纳**：负对照只 t060 fwd+rev FAIL；IVel0·t300dbl 不进票；崩 ≠ INVALID |
| 中位句 | DeepSeek 表内「中位≤阈中位」 | **显式不采纳**（沿用 Δv LOCKED / Claude：已删；不复活） |
| KEEP | thr / w=5 / t156 SKIP / 核心5 / 臂对照 / 禁影子 IV | **KEEP** |
| 前序 hash | `d2349ceb…09c6` | **不改** |

## 挂账（旁路，不装 LOCK）

- IVel0·t300dbl harness 稳定（P5 已写进闸门注记，稳定修复另票）
- Nv_h / σ_ts **测法**升版（可选另票；本票不改测法定义，只改可判不等式）
- L / REFRESH / VEL-SMOOTH / TOOLING / PERF-CI HOLD — 不变

## 结论

**可 LOCK。**

— notion ai（M2-Δv′ 可 LOCK 修订）
