# M3 下一票决断（M3-MATRIX-PACING CLOSED RIVE-MATRIX-PACING-FAIL · budget_ok=0 · pacing_ok=1 已验证 · 不采用 · 不合 main · PERF-CI HOLD）

> DECISION_AT=2026-11-06 10:55:00 Asia/Shanghai  
> 依据：`docs/m3_matrix_pacing_close_pack.md`；`docs/m3_matrix_pacing_result.md`；gate MATCH `2543e424cd71eea2959fe942ce0bfe7d7118ea6ee5c28b31da7902bce904812c`  
> tip/results：`fd05c6c058022c233f1cd896338e03d665c43d0f` / `7d5f545c7a3af0b7410421006a3683ce64c93f07` @ `claude/m3-matrix` = 实验证据；**成功≠采用≠默认可合入 main**  
> **硬禁**：不改 composition.h；不合 main；Success≠adopt；PERF-CI HOLD；不翻 CLOSED；**本 next 不授权新采数/真机**；**勿擅自 LOCK**

## 结论

**主推：新开 `M3-MATRIX-BUDGET`（暂名）闸门 DRAFT（纯文档）— 专吃本票 `budget_ok=0` 残差；STATUS=DRAFT·NOT LOCKED；不授权真机；不派 workbuddy；APPROVED_* 空。**

**并行（文档，非采数）**：pacing 修复（`m0r_set_pacing_window` @ `248a2d47e40ea86180a72c8102eab385a7770d52`）可作为后续票的只读旁证；**不得**借此回改 CLOSED MATRIX / 本票为 PASS；**不**插队解冻 PERF-CI / 合 main。

**一句话为什么**：同拓扑下 pacing 硬字段已绿，但 budget 升硬失败且照交未重跑——下一张应先把 budget 残差的定义/复测/与 pacing 边界写成 DRAFT，而不是自动 LOCK 或假装整票通过。

## 现状

| 项 | 状态 |
|---|---|
| M3-MATRIX-PACING | **CLOSED RIVE-MATRIX-PACING-FAIL**；budget_ok=0；pacing_ok=1；不采用；不合 main |
| M3-MATRIX | **CLOSED RIVE-MATRIX-FAIL**；**禁回改标签** |
| 其余 M3 前序 | CLOSED；**不翻** |
| composition.h | 冻结；本票未触 |
| PERF-CI | **HOLD** |
| 工具链 A/B | 挂账不挡 |

## 候选速判

| 选项 | 决断 | 理由 |
|---|---|---|
| **A 主推：MATRIX-BUDGET 新 DRAFT（纯文档）** | **建议采纳** | 吃掉 budget 残差；开放问题见下 |
| B 自动 LOCK / 立刻采数 | **否** | **勿擅自 LOCK**；须另走 LOCK→APPROVED |
| C 把本票改判 PASS 并采用 tip | **否** | budget_ok=0；Success≠adopt |
| D 回改 CLOSED MATRIX 为 PASS（因 pacing_ok=1） | **否** | 闸门禁令；两票分立 |
| E 合 main / 解冻 PERF-CI | **否** | 硬禁 |
| F 翻 CLOSED 重跑 PACING 同闸 | **否（本 next）** | 除非协调新 APPROVED；本文件不授权 |
| G 只推进试点 1、忽略 budget | **否作主推** | budget 是本票升硬残差 |

## 开放问题（供 **pidan** 拍板；写 DRAFT 前/LOCK 前）

1. **票名**：确认 `M3-MATRIX-BUDGET`，或其它命名（如 `M3-MATRIX-PACING-BUDGET`）？  
2. **budget 失败机制**：混刷双列下 T 取哪块屏？是否与 pacing 双列同形？CPU/GPU P95≤0.35·T 是否仍写死，还是放宽/改记录型？  
3. **复测范围**：是否强制同拓扑 IJI 75Hz@100%+180Hz@150%？≥3 会话？是否只测 budget 场景、不重开 pacing 单元格？结果目录建议 `m3/results_matrix_budget/`（勿覆写 pacing/matrix）？  
4. **与 pacing 边界**：本票已验证 pacing_ok=1 — 下票是否复测 pacing（硬/记录），还是只读旁证、budget 单刀口？  
5. **节奏修复是否进入下票基线**：`m0r_set_pacing_window` 是否要求 tip 必含，还是可选？  
6. **FAIL 照交策略**：budget 再 FAIL 是否仍禁止自动重跑？  
7. **标签前缀**：建议 `RIVE-MATRIX-BUDGET-*`？  
8. **是否允许 doc-only 挂账旁路**（对照 H5 ABS 先例）而不采数？  

## 协调下一步

1. 归档本票 CLOSED FAIL / 不采用 / 不合 main。  
2. 若采纳 A：指定文档助手写 `docs/m3_matrix_budget_gate_DRAFT.md`（**NOT LOCKED**；等开放问题拍板）。  
3. 推送结题 docs tar → 由协调另派 workbuddy/K93（本 next **不**派真机）。  
4. 禁整 tip 无 PR 合入；禁改 composition.h；**禁擅自 LOCK**。

## 确认（硬边界）

- 不改 composition.h（CRLF `33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97` / LF `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）  
- PERF-CI HOLD；不合 main；Success≠adopt  
- **本 next 不授权新采数/真机**；**勿擅自 LOCK**  
- **不回改** CLOSED MATRIX / 本票标签

— notion ai（下一票建议=M3-MATRIX-BUDGET DRAFT；勿擅自 LOCK；PACING=CLOSED FAIL；pacing 已验证但 budget 残差）
