# M2 并行下一票决断（PERF-CI HOLD 期间）

> DECISION_AT=2026-10-25 23:15:37 AWST（Australia/Perth）  
> 依据：pidan「先不派第二台真机，继续」@ 2026-10-25 23:13 AWST；`docs/m2_mainline_brief.md`；`docs/side_parked_hang_degrade.md`；`docs/notion_brief.md` §4.1；`plan.md` M2/M3；`docs/m2_perf_ci_gate_LOCKED.md`  
> 闸门骨架：`docs/m2_delta_v_gate_DRAFT.md`（NOT LOCKED；不授权采数）

## 结论

**选 A = M2-Δv（Δv 绝对门）。可立刻派 Gemini/DeepSeek 补洞 → Notion 收齐后 LOCK。**

## 现状（不变）

| 项 | 状态 |
|---|---|
| M2-PERF-CI | LOCKED `5b007dd4…282a` @ 01:37:05；**HOLD / 不 APPROVED / 不派双机**；**不改闸门、不降级为单机合格** |
| M2-01 / H5 / M2-02 | CLOSED PASS |
| 物理机 | 仅 DESKTOP-K93N3P8（iPhone 不算） |
| M3 | **本轮不开真机**（等 M2 Alpha 含 PERF-CI 收口） |

## 候选速判

| 选项 | 决断 | 一句理由 |
|---|---|---|
| **A M2-Δv** | **采纳** | R-2→M2 明文余项；H5 已清 seed 比病根后，绝对门可单机立；质量主线最硬 |
| B M2-L | 挂账 | 「查清」调查票；可跟在 Δv 后，不挡本轮 |
| C M2-REFRESH | **本轮不并行** | plan.md 混合刷新率矩阵属 **M3**；C5(b) 第二机/换刷新率在旁路；单机切 Hz 也易与 PERF-CI/C5(b) 纠缠 |
| D M2-VEL-SMOOTH | 挂账（可选） | brief 标可选；优先级低于绝对门 |
| E M2-TOOLING | 挂账 | engineering↔plan 交付物不对齐；闸门以 plan 为准；文档票可后开，不挡质量门 |
| F 另提 | 无 | 无更优、且不依赖 ≥2 物理机的主线票 |

## 理由（A）

1. **承接明确**：`notion_brief` M2 目标把「0.5 DIP 与 **Δv 绝对门**」从 R-2 相对门（逐格 ≤A / 合并 ≤0.25×A）转出；现相对门时代已结束，缺绝对预算。
2. **前置已清**：H5-SEED CLOSED PASS（禁影子 IV；WGC/屏一致或 IV=null+0）；再开绝对 Δv 不会踩 2.6× seed 坑。
3. **单机足够**：不依赖第二台物理机；与 PERF-CI HOLD 正交并行。
4. **不碰硬约束**：不重开 S1/R-3/R-2；不改 `composition.h` `a55d4952…e418`；不解冻/升级 Bv；成功≠采用；旁路挂账不挡；**PERF-CI 闸门一字不改**。
5. **不开 M3**：不抢 Rive/试点/混合刷新矩阵。

## 余项挂账（本轮不挡）

- M2-L（L 偏高 40–45 vs 20–29 ms）
- M2-REFRESH（→ 对齐 M3 / 或 PERF-CI 收口后另议）
- M2-VEL-SMOOTH（可选）
- M2-TOOLING（engineering 文档票，可后开）
- M2-PERF-CI 继续 HOLD 待第二台物理机

## 协调下一步一句

请立刻派 **Gemini**（Δv 绝对阈提案 + 格表/采样点）与 **DeepSeek**（可判门 / Nv_h / 与 H5 seed 衔接 + G2 风险）补洞；Notion 收齐后覆盖可 LOCK 修订 → LOCK → 问 pidan APPROVED（**单机即可**；提醒 PERF-CI 仍 HOLD 需第二台）。

## 禁令复述

不重开 S1/R-3/R-2；不改 `composition.h`；不解冻/升级 Bv；禁影子 IV；成功≠采用；旁路挂账不挡；PERF-CI 不降级为单机合格；不开 M3 真机。

— notion ai（HOLD 并行下一票决断）
