# M3 下一票决断（M3-RIVE-DPI CLOSED RIVE-DPI-PASS · 有条件采用 · PERF-CI HOLD）

> DECISION_AT=2026-10-29 10:00:00 Asia/Shanghai  
> 依据：`docs/notion_m3_rive_dpi_pack.md`；`docs/m3_rive_dpi_result.md`；`docs/m3_plan_DRAFT.md` §7  
> 本 RIVE-DPI gate `92e6b4e9fd1014faf74714eb6cb20fc7e998169dc1171785bb8ea43f68d97bf5` **不改、不重跑翻盘**  
> tip `67decd39…e47614` = 验证结论归档；**成功≠默认可合入 main**；csproj **不回灌主线**  
> 硬禁：不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；PERF-CI HOLD；不开与本票无关的 M2 翻盘；不立刻 Δv″；不抬 thr

## 结论

**主推：M3-RIVE-LOST**（按 `docs/m3_plan_DRAFT.md` §7 第 2 张票：设备丢失注入后自动恢复；恢复时间与内存增长记录；需管理员权限）。

**一句话为什么**：M3-RIVE-DPI 已 CLOSED PASS 并有条件采用验证结论；WP1 下一项是设备丢失恢复；须新 DRAFT→LOCK→APPROVED，本 next **不**授权采数、**不**解冻 PERF-CI、**不**翻盘 M2。

## 现状

| 项 | 状态 |
|---|---|
| M3-RIVE-DPI | **CLOSED RIVE-DPI-PASS**；METRICS_PASS；**有条件采用**；gate `92e6b4e9…97bf5` 不改 |
| csproj Lab 补丁 | **不回灌主线**；挂账另票 |
| composition.h | 冻结 `a55d4952…e418`；本票未触 |
| M3 plan 票序 | 1 DPI ✓ → **2 LOST** → 3 FOLLOW → 4 MATRIX；试点选型可并行 |
| PERF-CI | **HOLD**（第二台未就绪） |
| M2 旁路 / VERIFY-FAIL | **不翻盘**；不挡 M3 WP1 |
| S1 / R-3 / R-2 / Bv | 不重开；不解冻 |

## 候选速判

| 选项 | 决断 | 理由 |
|---|---|---|
| **A M3-RIVE-LOST** | **采纳（主推）** | m3_plan §7 下一张；WP1 关键路径 |
| B 立刻把 tip 整包合入 main | **否** | 成功≠采用；APPROVED 落地=否；须 PR；tip 以 results 为主 |
| C 回灌 csproj 进主线 | **否** | Lab 绕过；非 Rive；另票 |
| D M3-FOLLOW / MATRIX 插队 | **否（本批）** | 依赖 WP1；LOST 未完前不插 |
| E 解冻 PERF-CI / 开第二机采数 | **否** | HOLD；本 next 不批准 |
| F 重开 S1/R-3/R-2 / 解冻 Bv / 改 composition.h | **否** | 硬禁 |
| G 翻盘 M2 VERIFY-FAIL / 立刻 Δv″ / 抬 thr | **否** | 硬禁；与本票无关 |
| H 影子 IV | **否** | 硬禁 |

## 可 LOCK 要点（仅当开 M3-RIVE-LOST）

本 next **不**落新 DRAFT、**不**授权采数。若协调开 LOST，须：

1. 新闸门 DRAFT → LOCK → APPROVED（显式：host；注入方式；采数目录；禁令）。  
2. **不改** composition.h；不重开 S1/R-3/R-2；禁影子 IV；PERF-CI HOLD。  
3. 成功 ≠ 采用；结果评审后经 PR。  
4. 设备丢失注入需管理员（plan / gate 惯例）；DESKTOP-IJI1KJG 可提权。  
5. Lab csproj 若仍需要：在 LOST 派工说明中**单独**写清本机 Demo 启动前置，**不**顺带默默合入主线 csproj。

## 协调下一步

1. **主路径**：归档本票 CLOSED PASS / 有条件采用；gate 不改；准备 **M3-RIVE-LOST** DRAFT（另会话）。  
2. **禁**本 next 当采数授权；**禁**整 tip 无 PR 合入；**禁** csproj 回灌主线。  
3. WP1 DPI 代码（`7c6a4b17` 等）若要进 main：另开 PR 评审，与 LOST 闸门流程分开。

## 确认（硬边界）

- 不重开 S1 / R-3 / R-2；不改 composition.h（`a55d4952…e418`）；不解冻 Bv；禁影子 IV  
- PERF-CI HOLD；不立刻 Δv″；不抬 thr；不翻盘 M2  
- 本 RIVE-DPI gate **不改不重跑**  
- 本 next **不**授权采数、**不**派 workbuddy

## 禁令

不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；成功≠采用≠默认可合入 main；**不改**本 RIVE-DPI gate `92e6b4e9…97bf5`；PERF-CI HOLD；不开无关 M2 翻盘；禁抬 thr；禁立刻 Δv″；本 next **不**授权采数；**不**预开 FOLLOW/MATRIX 真机。

— notion ai（下一票主推=M3-RIVE-LOST；禁本 next 当采数授权）
