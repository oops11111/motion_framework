# M3 下一票决断（M3-MATRIX-PACING r2 CLOSED RIVE-MATRIX-PACING-FAIL · pacing_ok=0 · budget_ok=1 · 不采用 · 不合 main · PERF-CI HOLD）

> DECISION_AT=2026-11-06 11:08:00 AWST（Australia/Perth（AWST，UTC+8））  
> 依据：`docs/m3_matrix_pacing_r2_close_pack.md`；`docs/m3_matrix_pacing_r2_result.md`；gate MATCH `a1fa4590285f52948ecf51bc9c32629d4d2c3bc42efc7f5bb4e4922bd2be2883`  
> tip/results：`27ad82395e3eed94c40ad99feba6aed46eeca715` / `e3feb5cde7441e1855727dd0468275a6254e56bc` @ `claude/m3-matrix`  
> **硬禁**：不改 composition.h；不合 main；Success≠adopt；PERF-CI HOLD；不翻 CLOSED；**不回改 MATRIX / PACING r1**；**本 next 不授权新采数/真机**；**勿擅自 LOCK**

## 结论

**主推：新开（或改写）专吃 r2 `pacing_ok=0` 残差的纯文档闸门 DRAFT — STATUS=DRAFT·NOT LOCKED；不授权真机；不派 workbuddy；APPROVED_* 空。**

**对照已落盘 `docs/m3_matrix_budget_gate_DRAFT.md`：** r1 主残差是 budget；r2 主残差回到 **pacing**，且 budget 本轮已过。建议 **pidan 拍板优先级**：  
- **P1（建议）**：PACING 残差续票（r3 / 新 DRAFT；名称开放）  
- **P2**：M3-MATRIX-BUDGET DRAFT 降为挂账或合并进对照节（因 r2 budget_ok=1，budget 专票紧迫性下降，**除非**仍要锁预算口径）

**一句话为什么：** 出帧路径改为 vblank+dwmflush 后 budget 变绿，但 pacing 硬字段失败且照交未重跑——应用文档先消化 pacing 残差与阈值/探针定义，而不是自动 LOCK 或合并 r1+r2 洗票。

## 现状

| 项 | 状态 |
|---|---|
| PACING **r2** | **CLOSED FAIL**；pacing_ok=0；budget_ok=1；不采用 |
| PACING **r1** | **CLOSED FAIL**；pacing_ok=1；budget_ok=0；**禁回改** |
| MATRIX | **CLOSED FAIL**；**禁回改** |
| M3-MATRIX-BUDGET DRAFT | 已落盘 NOT LOCKED；优先级待重排 |
| composition.h / PERF-CI | 冻结 / **HOLD** |

## 候选速判

| 选项 | 决断 | 理由 |
|---|---|---|
| **A 主推：PACING 残差新 DRAFT（r3/续票）** | **建议采纳** | 吃 r2 pacing_ok=0 |
| B 自动 LOCK / 立刻采数 | **否** | 勿擅自 LOCK |
| C 合并 r1+r2 改判 PASS | **否** | 各有硬 FAIL；禁洗票 |
| D 回改 r1 / MATRIX | **否** | 硬禁 |
| E 合 main / 解冻 PERF-CI | **否** | 硬禁 |
| F 优先 LOCK BUDGET DRAFT | **否作主推** | r2 已 budget_ok=1；除非拍板仍要锁预算口径 |
| G doc-only 挂账旁路 | **开放** | 须 pidan 明示 |

## 开放问题（供 **pidan** 拍板；**勿擅自 LOCK**）

1. **续票票名**：`M3-MATRIX-PACING-R3` / `M3-MATRIX-PACING-VBLANK` / 其它？  
2. **与 BUDGET DRAFT 关系**：搁置 / 关闭为 SUPERSEDED / 仍并行 LOCK 预算口径？  
3. **pacing 失败机制**：vblank+dwmflush 下 p95≈5–7ms 相对 75Hz/180Hz 的 T 如何解释？阈值（原 ≤0.25·T 等）是否过苛需书面调整，还是保持硬门槛？  
4. **探针 v2 / pacing_source**：是否写死 vblank+dwmflush 为唯一合法源？seg MATCH 是否升硬？  
5. **复测**：强制同拓扑 IJI？≥3？结果目录建议 `m3/results_matrix_pacing_r3/`（勿覆写 r1/r2）？  
6. **是否复测 budget**：只读旁证（r2=1）还是硬复测？  
7. **FAIL 照交**：是否仍禁自动重跑？  
8. **标签**：仍 `RIVE-MATRIX-PACING-*` 还是新前缀？  
9. **是否允许 doc-only 挂账**（对照 H5 ABS）而不采数？

## 协调下一步

1. 归档 r2 CLOSED FAIL / 不采用 / 不合 main。  
2. 请 pidan 拍板开放问题与相对 BUDGET DRAFT 的优先级。  
3. 若采纳 A：指定文档助手写新 DRAFT（**NOT LOCKED**）。  
4. 推送结题 docs tar → 协调另派 workbuddy/K93（本 next **不**派真机）。  
5. 禁合 main；禁改 composition.h；**禁擅自 LOCK**；**禁回改 r1/MATRIX**。

## 确认（硬边界）

- 不改 composition.h（CRLF `33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97` / LF `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）  
- PERF-CI HOLD；不合 main；Success≠adopt  
- **本 next 不授权新采数/真机**；**勿擅自 LOCK**  
- **不回改** CLOSED MATRIX / PACING r1 / 本 r2 标签

— notion ai（下一票建议=PACING 残差新 DRAFT；重排 BUDGET DRAFT 优先级；勿擅自 LOCK；r2=CLOSED FAIL；Australia/Perth（AWST，UTC+8））
