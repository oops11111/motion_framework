# M3 下一票决断（M3-RIVE-LOST-RECOVER CLOSED RIVE-LOST-RECOVER-PASS · METRICS_PASS 有条件 · 有条件采用 · 不合 main · PERF-CI HOLD）

> DECISION_AT=2026-10-30 12:10:00 Asia/Shanghai  
> 依据：`docs/notion_m3_rive_lost_recover_pack.md`；`docs/m3_rive_lost_recover_result.md`；`docs/m3_plan_DRAFT.md` §7；RECOVER DRAFT（**未 LOCK**）；r1 INCONCLUSIVE / r2 FAIL（**不翻盘**）；PROBE LOCKED `7afbcd98…6b54`（**不改**）  
> tip/commit：`c9546964…8cbd` / `68ce59d3…7ab6` = 实验分支证据归档；**成功≠采用≠默认可合入 main**；本票 **不合 main**  
> 硬禁：不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；PERF-CI HOLD；不开与本票无关的 M2 翻盘；不立刻 Δv″；不抬 thr；**本 next 不授权采数**；不派 workbuddy

## 结论

**主推：新 DRAFT 票 `M3-RIVE-FOLLOW`（按 plan §7 第 3 张；仅文档/闸门起草）— 本 next 不 LOCK、不 APPROVED、不授权采数。**

**附带：建议补办 `M3-RIVE-LOST-RECOVER` 闸门 LOCK 归档**（把已跑事实与「按用户指令采数 / 闸门曾为 DRAFT」写入事后 LOCK/纸面；开放问题可标「采数时跳过拍板」）— **补 LOCK 归档 ≠ 授权新采数 ≠ 翻盘本票**。

**FOLLOW / MATRIX 是否仍挡**：相对 r2「恢复未成立」挡板 — **本票有条件 PASS 后，FOLLOW/MATRIX 不再因「WP1 恢复未成立」硬挡**；但仍须各自 **DRAFT→LOCK→APPROVED** 才可真机，**本 next 不插队真机、不授权采数**。MATRIX 仍建议排在 FOLLOW 闸门路径之后（plan 序）。

**一句话为什么**：RECOVER 相对 r2 abort 已越过（recover_ok / abort=none / 3×RECORDED）；plan 下一张为 FOLLOW；闸门未 LOCK 的残差用「补归档」消化，**不要**用本 next 再开采数。

## 现状

| 项 | 状态 |
|---|---|
| M3-RIVE-DPI | CLOSED RIVE-DPI-PASS；有条件采用；gate `92e6b4e9…97bf5` 不改 |
| M3-RIVE-LOST r1 | **CLOSED RIVE-LOST-INCONCLUSIVE**；**不翻盘** |
| M3-RIVE-LOST r2 | **CLOSED RIVE-LOST-FAIL**；**不翻盘**；禁当日曾重跑口径已过 |
| M3-RIVE-LOST-RECOVER | **CLOSED RIVE-LOST-RECOVER-PASS**；METRICS_PASS **有条件**；**有条件采用验证结论；不合 main** |
| RECOVER gate | **仍 DRAFT**；本地无 approved；采数曾=按用户指令（协调转述） |
| PROBE gate | LOCKED `7afbcd98…6b54`；探测 r2 实证；**不改 body** |
| composition.h | 冻结 LF `a55d4952…e418` / CRLF `33d826c7…6e97`；本票未触 |
| M3 plan 票序 | 1 DPI ✓ → 2 LOST（r1/r2/RECOVER ✓有条件）→ **3 FOLLOW** → 4 MATRIX |
| PERF-CI | **HOLD** |
| M2 旁路 / VERIFY-FAIL | **不翻盘** |
| S1 / R-3 / R-2 / Bv | 不重开；不解冻 |

## 候选速判

| 选项 | 决断 | 理由 |
|---|---|---|
| **A 新 DRAFT：`M3-RIVE-FOLLOW`（文档）** | **采纳（主推）** | plan §7 第 3 张；恢复有条件成立后可开 |
| B 补 RECOVER 闸门 LOCK 归档（事后纸面） | **建议（附带）** | 消化「未 LOCK 已采数」残差；**≠授权采数** |
| C 本 next 授权 RECOVER/FOLLOW 真机采数 | **否** | 硬禁；本 next 不授权采数 |
| D 立刻插队 MATRIX 真机 | **否** | 须 FOLLOW 路径与各自 LOCK+APPROVED；不插队 |
| E 把 tip/commit 整包合入 main / csproj 回灌 | **否** | 不合 main；成功≠采用 |
| F 解冻 PERF-CI / 开第二机 | **否** | HOLD；本 next 不批准 |
| G 重开 S1/R-3/R-2 / 解冻 Bv / 改 composition.h | **否** | 硬禁 |
| H 翻盘 r1/r2 / 改 PROBE / 改本票 CLOSED | **否** | 硬禁 |
| I 翻盘 M2 / 立刻 Δv″ / 抬 thr / 影子 IV | **否** | 硬禁 |
| J 以 frames_missed 未澄清为由重开 RECOVER 真机 | **否（本批）** | 挂账复核 results 即可；禁无同参硬凑；本 next 不授权采数 |
| K 当日/同参重跑 RECOVER 硬凑无条件 METRICS | **否** | 有条件 PASS 已 CLOSED；复核用已落盘 results |

## FOLLOW DRAFT 应写死的要点（仅当开 A；本 next 不落闸门正文）

1. 新闸门 DRAFT →（协调）LOCK → APPROVED 后才可采数；**本 next 不授权**。  
2. **不改** composition.h；不重开 S1/R-3/R-2；禁影子 IV；PERF-CI HOLD。  
3. 成功 ≠ 采用；结果评审后经 PR；**不合 main 默认**。  
4. 继承：WP1 DPI PASS + LOST-RECOVER 有条件 PASS 为前置；**不**翻盘 r1/r2。  
5. 挂账可写入 FOLLOW 观察：`recover_max_ms≈2.2s`（RECOVER 只记录项）；`frames_missed` 语义复核结论。  
6. Lab csproj 若仍需要：仅本机 Demo 启动前置，**不**回灌主线。

## 补 LOCK 归档（选项 B）说明

- **目的**：纸面闭环「DRAFT 未 LOCK 却已按用户指令采数」。  
- **内容建议**：LOCKED_AT / APPROVED_*（若协调补签）或显式写「采数授权=用户指令（日期）；开放问题采数时跳过」；GATE_BODY_SHA256 按 DRAFT 约定计算；**不改**已 CLOSED result 结论。  
- **明确**：**补归档 ≠ 新采数授权 ≠ 派 workbuddy**。

## 协调下一步

1. **主路径**：归档本票 CLOSED PASS / 有条件 METRICS / 有条件采用 / 不合 main；准备 **`M3-RIVE-FOLLOW` 新 DRAFT**（另会话；文档）。  
2. **附带**：协调决定是否补 RECOVER LOCK 归档；**勿**把补档当采数令。  
3. **挂账复核**（只读）：待 `m3/results_lost_recover/` 同步到本根后澄清 `frames_missed` / `injections_ok`（不重跑）。  
4. **禁**本 next 当采数授权；**禁**整 tip 无 PR 合入；**禁** csproj 回灌主线。  
5. MATRIX：**不插队真机**；FOLLOW 闸门路径推进后再议。  
6. DPI / PROBE / r1 / r2：**不改不重跑翻盘**。

## 确认（硬边界）

- 不重开 S1 / R-3 / R-2；不改 composition.h（`a55d4952…e418` / CRLF `33d826c7…6e97`）；不解冻 Bv；禁影子 IV  
- PERF-CI HOLD；不立刻 Δv″；不抬 thr；不翻盘 M2  
- 不改 r1/r2 CLOSED；不改 PROBE LOCKED；不把本票 RECOVER DRAFT 在本 next 擅自写成 LOCKED（补档另办）  
- 本 next **不**授权采数、**不**派 workbuddy

## 禁令

不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；成功≠采用≠默认可合入 main；不合 main；PERF-CI HOLD；不开无关 M2 翻盘；禁抬 thr；禁立刻 Δv″；**本 next 不授权采数**；**不**预开 FOLLOW/MATRIX 真机；**不**翻盘 r1/r2；**不**把 SIM_SMOKE 当真机唯一 PASS。

— notion ai（下一票主推=`M3-RIVE-FOLLOW` DRAFT 文档；建议补 RECOVER LOCK 归档≠采数授权；FOLLOW/MATRIX 不再因恢复未成立硬挡但仍须各自 LOCK+APPROVED；本 next 不授权采数）
