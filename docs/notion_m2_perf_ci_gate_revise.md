# M2-PERF-CI 闸门补洞决断（可 LOCK）

> DECISION_AT=2026-10-25 01:35:00 AWST（Australia/Perth）  
> 依据：`docs/gemini_m2_perf_ci_design.md`、`docs/deepseek_m2_perf_ci_review.md`、`docs/m2_perf_ci_gate_DRAFT.md`（旧）  
> 完整可 LOCK 正文：`docs/m2_perf_ci_gate_DRAFT.md`（本决断后覆盖）  
> **不授权采数**；须 LOCKED + APPROVED 后才可派 workbuddy；提醒需第二台物理机。

## 拍板

| # | 缺口 | 决断 |
|---|---|---|
| 1 | 场景三件套 | **采纳 Gemini**：Idle / Steady Play / Interrupted Retarget；每场景 ≥1000 帧；每机每场景 ≥2 轮独立进程 |
| 2 | P95/P99 + 对账 | **P95/P99 目标句采纳 Gemini**；**主判据 = PresentMon**（DeepSeek）；PresentStats 仅存在性+会话汇总对账；丢帧定义写死 **Frame Duration > 1.5×T**（采 Gemini，不用 DeepSeek 2×） |
| 3 | 机器矩阵 / INVALID | **采纳 DeepSeek**：≥2 物理机、machineId 互异、≥2 GPU 档；1 机 / VM / 同机复跑 / 伪造 = INVALID |
| 4 | EventPipe vs logman | **采纳 DeepSeek**：仅无 admin 且 logman Access denied（留证）才允许 EventPipe；有 admin 必须 logman ETL |
| 5 | G2-LOCK-9…14 | **采纳 DeepSeek**，场景集以本闸门 G1 三件套为准 |
| 6 | 禁令 | 不重开 S1/R-3/R-2；不解冻 Bv；不改 composition.h `a55d4952…e418`；禁影子 IV；成功≠采用；旁路挂账不挡 |

## 结论

**可 LOCK。** 请协调侧立刻写 hash + LOCKED_AT，并向 pidan 要 APPROVED（须提醒准备第二台物理机）。

— notion ai（M2-PERF-CI 闸门补洞决断）
