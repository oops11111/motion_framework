# M3 下一票决断（M3-RIVE-LOST CLOSED RIVE-LOST-INCONCLUSIVE · 非 METRICS_PASS · 未采用 · PERF-CI HOLD）

> DECISION_AT=2026-10-29 17:45:00 Asia/Shanghai  
> 依据：`docs/notion_m3_rive_lost_pack.md`；`docs/m3_rive_lost_result.md`；`docs/m3_plan_DRAFT.md` §7；`docs/m3_rive_lost_gate_DRAFT.md`  
> 本 LOST：本地**仅 DRAFT**（无 LOCKED hash）；**未改** DRAFT body；**当日不重跑**本 gate 采数（ONELINE stop）  
> tip `82a53cce…02694d` = 实验分支证据归档；**成功≠采用≠默认可合入 main**  
> 硬禁：不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；PERF-CI HOLD；不开与本票无关的 M2 翻盘；不立刻 Δv″；不抬 thr；**不授权采数**除非新 LOCK+APPROVED

## 结论

**主推：新 DRAFT 票 — 探针 / 检测窗 / 注入探测修复后再 LOCK**（建议名：`M3-RIVE-LOST-PROBE` 或覆盖修订 `m3_rive_lost_gate_DRAFT.md` 后重新 LOCK；**不得**当日重跑本已停跑 gate）。

**一句话为什么**：本票 CLOSED **INCONCLUSIVE**（inject1 30s 未检出 + s1 crash + 仅 1/3），恢复路径未进入可裁决状态；须先修探针/检测窗/注入探测并走新 DRAFT→LOCK→APPROVED，**本 next 不授权采数**、不插队 FOLLOW/MATRIX、不解冻 PERF-CI。

## 现状

| 项 | 状态 |
|---|---|
| M3-RIVE-DPI | CLOSED RIVE-DPI-PASS；有条件采用；gate `92e6b4e9…97bf5` 不改 |
| M3-RIVE-LOST | **CLOSED RIVE-LOST-INCONCLUSIVE**；非 METRICS_PASS；**未采用**；当日不重跑 |
| LOST gate 文件 | 本地仅 DRAFT；**无** LOCKED hash |
| composition.h | 冻结 LF `a55d4952…e418` / CRLF `33d826c7…6e97`；本票未触 |
| M3 plan 票序 | 1 DPI ✓ → **2 LOST（本票 INCONCLUSIVE 挂账）** → 3 FOLLOW → 4 MATRIX |
| PERF-CI | **HOLD** |
| M2 旁路 / VERIFY-FAIL | **不翻盘** |
| S1 / R-3 / R-2 / Bv | 不重开；不解冻 |

## 候选速判

| 选项 | 决断 | 理由 |
|---|---|---|
| **A 新 DRAFT：修探针 / 加长检测窗 / 换注入探测** | **采纳（主推）** | 证据指向探针/窗/注入未成立，非恢复证伪；诚实下一刀 |
| B 当日重跑同一 LOST gate / 同 tip 硬凑 3 会话 | **否** | ONELINE：闸门当日不重跑；且仍缺 LOCKED+APPROVED 同步到本根也不授权本 next 采数 |
| C 标 RIVE-LOST-FAIL 后直接进 FOLLOW | **否** | 恢复路径未证伪；plan FOLLOW 依赖 WP1 丢失恢复成立 |
| D 立刻 MATRIX / 试点真机 | **否** | 依赖 WP1；LOST 验证未成立 |
| E 挂账旁路、永久跳过 LOST | **否（本批）** | plan §7 第 2 张仍关键；可挂账残差但须新探针票，非静默跳过 |
| F 解冻 PERF-CI / 开第二机 | **否** | HOLD；本 next 不批准 |
| G 重开 S1/R-3/R-2 / 解冻 Bv / 改 composition.h | **否** | 硬禁 |
| H 翻盘 M2 / 立刻 Δv″ / 抬 thr / 影子 IV | **否** | 硬禁 |
| I 把 tip 整包合入 main / csproj 回灌 | **否** | 成功≠采用；非 PASS |

## 新 DRAFT 应写死的要点（仅当开 A）

本 next **不**落新闸门正文、**不**授权采数。若协调开探针修订票，须：

1. 新（或修订）闸门 DRAFT → LOCK → APPROVED（显式：host；注入手段与探测信号；检测窗秒数；N_inj；sessions≥3；禁令；**采数目录**）。  
2. 针对挂账：inject 探测（为何 30s 未检出）、crash 防护、检测窗是否加长、dxcap/TDR 与探针握手。  
3. **不改** composition.h；不重开 S1/R-3/R-2；禁影子 IV；PERF-CI HOLD。  
4. 成功 ≠ 采用；结果评审后经 PR。  
5. Lab csproj 若仍需要：仅本机 Demo 启动前置，**不**回灌主线。  
6. **禁止**把本已 CLOSED 的 INCONCLUSIVE 票当日重跑翻盘为 PASS。

## 协调下一步

1. **主路径**：归档本票 CLOSED INCONCLUSIVE / 非 METRICS_PASS / 未采用；准备 **探针·检测窗** 新 DRAFT（另会话）。  
2. **禁**本 next 当采数授权；**禁**当日重跑；**禁**整 tip 无 PR 合入；**禁** csproj 回灌主线。  
3. FOLLOW / MATRIX：**LOST 验证成立前不插队真机**。  
4. DPI gate `92e6b4e9…97bf5`：**不改不重跑**。

## 确认（硬边界）

- 不重开 S1 / R-3 / R-2；不改 composition.h（`a55d4952…e418` / CRLF `33d826c7…6e97`）；不解冻 Bv；禁影子 IV  
- PERF-CI HOLD；不立刻 Δv″；不抬 thr；不翻盘 M2  
- 本 LOST DRAFT **不改成 LOCKED**；无 LOCKED hash 可改  
- 本 next **不**授权采数、**不**派 workbuddy

## 禁令

不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；成功≠采用≠默认可合入 main；PERF-CI HOLD；不开无关 M2 翻盘；禁抬 thr；禁立刻 Δv″；本 next **不**授权采数；**不**预开 FOLLOW/MATRIX 真机；**不**当日重跑本 LOST。

— notion ai（下一票主推=新 DRAFT 修探针/检测窗/注入探测；禁本 next 当采数授权；LOST=CLOSED INCONCLUSIVE 非 METRICS_PASS）
