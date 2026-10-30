# M3 下一票决断（M3-RIVE-LOST r2 CLOSED RIVE-LOST-FAIL · 非 METRICS_PASS · 未采用 · PERF-CI HOLD）

> DECISION_AT=2026-10-30 08:50:00 Asia/Shanghai  
> 依据：`docs/notion_m3_rive_lost_r2_pack.md`；`docs/m3_rive_lost_r2_result.md`；`docs/m3_plan_DRAFT.md` §7；PROBE LOCKED `7afbcd98…6b54`；r1 CLOSED INCONCLUSIVE（**不翻盘**）  
> tip/commit：`382683cf…1a04ec` / tip_read `0aea9d17…012807` = 实验分支证据归档；**成功≠采用≠默认可合入 main**  
> 硬禁：不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；PERF-CI HOLD；不开与本票无关的 M2 翻盘；不立刻 Δv″；不抬 thr；**不授权采数**除非新 LOCK+APPROVED；**禁当日重跑**本 r2；禁插队 FOLLOW/MATRIX

## 结论

**主推：新 DRAFT 票 `M3-RIVE-LOST-RECOVER`（或同类名）— 专攻 rive_abort / 无 recovery / 首次注入后进程死导致二次注入不可达；再 LOCK→APPROVED 后才可采数。**

**PROBE 闸门关系（诚实）**：PROBE LOCKED `7afbcd980d464abdeb835f059e05089edbf1d8f1112efb3e1d1330bd386e6b54` 的**探测能力目标已由 r2 实证**（`lost_detected=1/1/1`）→ **挂账关闭「探针未检出」残差**（相对 r1）；PROBE 文件本身**不删不改 hash**，可标「探测能力已由 r2 实证」并**保留为 RECOVER 前置证据**（非再开同参探测重跑）。**本 next 不**把 PROBE 再当「须先修探针」主推。

**一句话为什么**：r2 CLOSED **FAIL**（探测已过、恢复 abort 未成立）；下一刀应切在 **abort/重建恢复路径**，而非再修探针；**本 next 不授权采数**、不插队 FOLLOW/MATRIX、不解冻 PERF-CI、不翻盘 r1。

## 现状

| 项 | 状态 |
|---|---|
| M3-RIVE-DPI | CLOSED RIVE-DPI-PASS；有条件采用；gate `92e6b4e9…97bf5` 不改 |
| M3-RIVE-LOST r1 | **CLOSED RIVE-LOST-INCONCLUSIVE**；**不翻盘** |
| M3-RIVE-LOST r2 | **CLOSED RIVE-LOST-FAIL**；非 METRICS_PASS；**未采用**；禁当日重跑 |
| PROBE gate | LOCKED `7afbcd98…6b54`；**探测能力 r2 实证**；approved 本地仍空；**不改 body** |
| LOST gate 文件 | 仍仅 DRAFT；**未改** |
| composition.h | 冻结 LF `a55d4952…e418` / CRLF `33d826c7…6e97`；本票未触 |
| M3 plan 票序 | 1 DPI ✓ → 2 LOST（r1 INCONCLUSIVE / **r2 FAIL 挂账恢复**）→ 3 FOLLOW → 4 MATRIX |
| PERF-CI | **HOLD** |
| M2 旁路 / VERIFY-FAIL | **不翻盘** |
| S1 / R-3 / R-2 / Bv | 不重开；不解冻 |

## 候选速判

| 选项 | 决断 | 理由 |
|---|---|---|
| **A 新 DRAFT：`M3-RIVE-LOST-RECOVER`（abort/重建）** | **采纳（主推）** | 证据指向 rive_abort、无 recovery、首注后死；诚实下一刀 |
| B 当日重跑同一 LOST/r2 / 同 tip 硬凑 PASS | **否** | K7 no re-run；禁当日重跑；仍非 PASS 可硬凑 |
| C 再开探针票 / 重跑 PROBE 采数 | **否（主推否）** | 探测已由 r2 实证；重复修探针偏离失败点 |
| D PROBE 挂账关闭「探针未检出」+ 保留前置 | **是（附带）** | 与 A 兼容；不删 LOCKED；不授权采数 |
| E 标 FAIL 后直接进 FOLLOW / MATRIX | **否** | plan FOLLOW/MATRIX 依赖 WP1 丢失**恢复**成立 |
| F 挂账旁路、永久跳过恢复 | **否（本批）** | plan §7 第 2 张仍关键；须 RECOVER 票 |
| G 解冻 PERF-CI / 开第二机 | **否** | HOLD；本 next 不批准 |
| H 重开 S1/R-3/R-2 / 解冻 Bv / 改 composition.h | **否** | 硬禁 |
| I 翻盘 M2 / 立刻 Δv″ / 抬 thr / 影子 IV | **否** | 硬禁 |
| J 把 tip/commit 整包合入 main / csproj 回灌 | **否** | 成功≠采用；非 PASS |
| K 翻盘 r1 INCONCLUSIVE→PASS/FAIL | **否** | 硬禁；r1 探针口径仍成立 |

## 新 DRAFT（RECOVER）应写死的要点（仅当开 A）

本 next **不**落新闸门正文、**不**授权采数。若协调开 RECOVER 票，须：

1. 新闸门 DRAFT → LOCK → APPROVED（显式：host；注入手段；N_inj；sessions≥3；**abort/重建成功判据**；二次注入可达；禁令；**采数目录**建议 `m3/results_lost_recover/`，**勿覆写** `m3/results_lost_r2/` / r1 / probe）。  
2. 针对挂账：`rive_abort=1` 根因；重建 D3D + Rive + `SetRenderingDevice`；首注后进程保活以便第二次注入；恢复后帧/稳定性/预算记录口径。  
3. **继承**：探测能力以 r2 `lost_detected=1/1/1` 为前置实证；**不**要求再开「仅探针」主票。  
4. **不改** composition.h；不重开 S1/R-3/R-2；禁影子 IV；PERF-CI HOLD。  
5. 成功 ≠ 采用；结果评审后经 PR。  
6. Lab csproj 若仍需要：仅本机 Demo 启动前置，**不**回灌主线。  
7. **禁止**把本已 CLOSED 的 r2 FAIL 当日重跑翻盘为 PASS；**禁止**翻盘 r1。

## 协调下一步

1. **主路径**：归档本票 CLOSED FAIL / 非 METRICS_PASS / 未采用；准备 **`M3-RIVE-LOST-RECOVER` 新 DRAFT**（另会话）。  
2. **PROBE**：标注「探测能力已由 r2 实证」挂账关闭探针残差；LOCKED 文件保留作前置；**不**再 APPROVED 同参探测重跑作为主路径。  
3. **禁**本 next 当采数授权；**禁**当日重跑；**禁**整 tip 无 PR 合入；**禁** csproj 回灌主线。  
4. FOLLOW / MATRIX：**恢复验证成立前不插队真机**。  
5. DPI gate `92e6b4e9…97bf5` / PROBE hash `7afbcd98…6b54`：**不改不重跑**（PROBE 不重跑探测主路径）。

## 确认（硬边界）

- 不重开 S1 / R-3 / R-2；不改 composition.h（`a55d4952…e418` / CRLF `33d826c7…6e97`）；不解冻 Bv；禁影子 IV  
- PERF-CI HOLD；不立刻 Δv″；不抬 thr；不翻盘 M2  
- 不改 r1 CLOSED；不改 PROBE/LOST gate body  
- 本 next **不**授权采数、**不**派 workbuddy

## 禁令

不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；成功≠采用≠默认可合入 main；PERF-CI HOLD；不开无关 M2 翻盘；禁抬 thr；禁立刻 Δv″；本 next **不**授权采数；**不**预开 FOLLOW/MATRIX 真机；**不**当日重跑本 r2；**不**翻盘 r1；**不**把 SIM_SMOKE 当真机 PASS。

— notion ai（下一票主推=`M3-RIVE-LOST-RECOVER` 新 DRAFT；PROBE 探测能力 r2 实证挂账；禁本 next 当采数授权；LOST r2=CLOSED FAIL 非 METRICS_PASS）
