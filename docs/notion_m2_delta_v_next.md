# M2 下一票决断（M2-Δv CLOSED 后 · PERF-CI HOLD）

> DECISION_AT=2026-10-26 08:59:30 AWST（Australia/Perth）  
> 依据：`docs/notion_m2_delta_v_pack.md`；`docs/m2_delta_v_result.md`；`docs/m2_perf_ci_gate_LOCKED.md` HOLD

## 结论

**选 A = M2-Δv′（升版绝对门）。** DRAFT：`docs/m2_delta_v_prime_gate_DRAFT.md`（NOT LOCKED）。

## 现状

| 项 | 状态 |
|---|---|
| M2-Δv | **CLOSED INCONCLUSIVE**；gate `d2349ceb…09c6` 不改 |
| M2-PERF-CI | LOCKED HOLD；不 APPROVED；不降级 |
| M2-01 / H5 / M2-02 | CLOSED PASS |
| 物理机 | 仅 DESKTOP-K93N3P8 |
| M3 | 本轮不开真机 |

## 候选速判

| 选项 | 决断 | 一句理由 |
|---|---|---|
| **A M2-Δv′** | **采纳** | 可判几何与 thr 互斥致空转；升版才能分胜负 |
| B M2-L | 挂账 | 不挡；可随后 |
| C 多刷新率单机 | 本轮不并行 | M3/C5b |
| D VEL-SMOOTH | 挂账 | 可选 |
| E TOOLING | 挂账 | 文档 |
| 重跑 Δv LOCKED | 否 | 同几何必再 INCONCLUSIVE |

## 协调下一步

派 Gemini（可判几何修订，KEEP 阈）+ DeepSeek（虚 PASS / base / harness 旁路）→ Notion 可 LOCK 修订 → LOCK → pidan APPROVED（单机；PERF-CI 仍 HOLD）。

## 禁令

不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；成功≠采用；PERF-CI 不降级；不开 M3 真机。

— notion ai
