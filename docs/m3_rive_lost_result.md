# M3-RIVE-LOST 结题结果

STATUS: CLOSED
TICKET: RIVE-LOST-INCONCLUSIVE
METRICS: 非 METRICS_PASS（未完成 3 会话 + CRASHED_NO_VERDICT + 注入未检出；恢复路径未被证伪亦未被证实）
ADOPT: 未采用（成功 ≠ 采用；本票无采用结论可采）
CSPROJ_BACKPORT: 否（不回灌主线）
WRITTEN_AT: 2026-10-29 17:45:00 Asia/Shanghai
GATE: docs/m3_rive_lost_gate_DRAFT.md（**本地无 LOCKED**；STATUS=DRAFT；GATE_BODY_SHA256 按 DRAFT 约定于 LOCK 时再算 → **本结题无完整 gate hash**）
GATE_BODY_SHA256: （无；本地未找到 `docs/m3_rive_lost_gate_LOCKED.md` / approved / 含完整 hash 的 LOCK 草稿）
LOCKED_AT: （无）
APPROVED: （本地无 `docs/m3_rive_lost_approved.md`）
BRANCH: claude/m3-rive-lost
TIP: 82a53ccee2dd7ed6456c635421e70eb18902694d
HOST: DESKTOP-IJI1KJG；ROOT 惯例 mf_m0b
composition.h: PRE=POST；CRLF=`33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97`；LF=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`；edited=False

## 授权 / 闸门核验摘要

| 项 | 结果 |
|---|---|
| LOCKED 闸门 | **本地不存在** `docs/m3_rive_lost_gate_LOCKED.md` |
| DRAFT | 存在：`docs/m3_rive_lost_gate_DRAFT.md`（STATUS=DRAFT；**NOT LOCKED**；明确不授权采数） |
| GATE_BODY_SHA256 | **无**（DRAFT 写死：LOCK 时再算；本结题**未改** DRAFT body） |
| approved 文件 | **本地不存在** |
| 本结题任务 | **不采数、不派 workbuddy、不重跑**；仅据 workbuddy ONELINE 事实 + 本地文档结题 |
| composition.h | **未改** PRE=POST；本地 `code/composition.h` LF hash 与 ONELINE LF 一致；CRLF 由同文件换行复算 MATCH `33d826c7…6e97` |
| PERF-CI | **仍 HOLD** |

> 说明：若协调侧另有已 LOCK/APPROVED 的 LOST 闸门未同步到本根 `/workspace/motion_framework/`，本结题**不虚构**其 hash；以本根可读文件为准。DRAFT G2 将「注入未真正发生（探针未捕获）」列为 INVALID 类；本票标签在任务给定的 `FAIL`/`INCONCLUSIVE` 二选一中取 **INCONCLUSIVE**（见下口径），并显式 **非 METRICS_PASS**。

## workbuddy ONELINE（核实后；**字段勿改写**）

```
M3-RIVE-LOST | commit=82a53ccee2dd7ed6456c635421e70eb18902694d @ claude/m3-rive-lost | BUILD_OK | SIM_SMOKE=PASS | elevated=True | dxcap=OK | sessions=1/3 | s1=CRASHED_NO_VERDICT | stop=K5 inject1 not detected within 30s（无后续注入；闸门：当日不重跑） | lost_detected / recover_ok / recover_max_ms / memΔ = N/A | private_mb_seg0=179 | event4101=0 | composition PRE=POST CRLF=33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97 LF=a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418 edited=False | host=DESKTOP-IJI1KJG ROOT惯例 mf_m0b | Success≠adopt | PERF-CI HOLD | ticket=RIVE-LOST-INCONCLUSIVE | 非METRICS_PASS | 未采用
```

## 票结局声明

- 真机对照 DRAFT G2 意图（≥3 会话；① 注入检测；② 恢复成功等）：**sessions=1/3**；**s1=CRASHED_NO_VERDICT**；**stop=K5 inject1 not detected within 30s**（无后续注入）；lost_detected / recover_ok / recover_max_ms / memΔ = **N/A** → **无法形成 METRICS_PASS**。
- 票标签 **`RIVE-LOST-INCONCLUSIVE`**；STATUS=**CLOSED**；协调对照 **非 METRICS_PASS**。
- **口径说明（INCONCLUSIVE vs FAIL）**：
  - **非 `RIVE-LOST-FAIL`**：恢复路径（重建 D3D + Rive + `SetRenderingDevice`）**未被证伪**——注入在 30s 检测窗内**未被探针检出**，恢复判据未进入可裁决状态；不能据此宣称「恢复失败」。
  - **取 `RIVE-LOST-INCONCLUSIVE`**：未完成 3 会话 + CRASHED_NO_VERDICT + 探针/检测窗未成立 → **未完成验证 / 探针失败**；DRAFT 亦未将「K5 当日停」写死为 FAIL（本地仅 DRAFT；ONELINE 记「闸门：当日不重跑」→ 写入正文，**当日不重跑本 gate**）。
  - DRAFT G2 INVALID 列举含「注入未真正发生（探针未捕获）」→ 本票与 INVALID 类程序/探针问题**同向**，但任务标签二选一取 INCONCLUSIVE，并写清 **非 METRICS_PASS**。
- **明确声明**：**成功 ≠ 采用 ≠ 默认可合入 main ≠ 授权改 composition.h ≠ PERF-CI ≠ 解冻 Bv ≠ 翻盘 M2 ≠ 重开 S1/R-3/R-2**。
- **是否采用 = 未采用**：无 PASS 验证结论可有条件采用；实验分支 tip `82a53cce…` **不**视为可合入主线。
- **composition.h 未改**；**PERF-CI 仍 HOLD**。

## METRICS 核实（对照 ONELINE；本地无完整 `m3/results_lost/` 矩阵可读）

| 项 | ONELINE | 对照 DRAFT G2 | 结果 |
|---|---|---|---|
| sessions | 1/3 | 要求 ≥3 独立进程 | **未满足** |
| s1 | CRASHED_NO_VERDICT | 每会话须可裁决 | **无 verdict** |
| stop | K5 inject1 not detected within 30s；无后续注入；当日不重跑 | ① 注入须被探针捕获 | **未检出** |
| lost_detected | N/A | ① | **N/A** |
| recover_ok | N/A | ② | **N/A** |
| recover_max_ms | N/A | ③ 只记录 | **N/A** |
| memΔ | N/A | ⑥ 只记录 | **N/A** |
| private_mb_seg0 | 179 | 记录 | 记录 |
| event4101 | 0 | 记录 | 记录 |
| BUILD_OK / SIM_SMOKE / elevated / dxcap | BUILD_OK / PASS / True / OK | 前置 | 前置 OK；**不升 METRICS_PASS** |

**ticket 判据：非 METRICS_PASS**（①②④⑤ 未形成可裁决会话集）。

## 采用决断（写死）

| 项 | 决断 | 理由 |
|---|---|---|
| 票标签 | **RIVE-LOST-INCONCLUSIVE** | 探针/检测窗/会话未完成；非恢复路径证伪 |
| METRICS | **非 METRICS_PASS** | 1/3 + CRASHED_NO_VERDICT + inject 未检出 |
| 是否采用 | **未采用** | 成功≠采用；无 PASS 结论 |
| composition.h | **未改** PRE=POST CRLF/LF 全 hash | 硬禁；edited=False |
| csproj 回灌主线 | **否** | 沿用 DPI 边界 |
| PERF-CI | **HOLD** | 硬禁 |
| 当日重跑本 gate | **否** | ONELINE stop：闸门当日不重跑 |

## 挂账 / 残差

| 项 | 处置 |
|---|---|
| K5：inject1 30s 内未检出 | **挂账**：探针/检测窗/注入探测未成立；无后续注入 |
| s1 CRASHED_NO_VERDICT | **挂账**：会话崩溃无裁决 |
| sessions 仅 1/3 | **挂账**：未达 ≥3 |
| lost/recover/mem 全 N/A | **挂账**：恢复路径未进入可测状态 |
| 当日不重跑 | **遵守**：本 LOST 采数当日不重跑；新跑须新 DRAFT→LOCK→APPROVED |
| Lab / Demo 启动 | 若仍需 csproj Lab 前置：**不回灌主线** |
| M2 旁路 / VERIFY-FAIL | **不翻盘** |
| PERF-CI | **仍 HOLD** |
| 下一张 | 见 `docs/notion_m3_rive_lost_next.md`（探针/检测窗新 DRAFT；**不**授权采数） |

## 标签边界

- 本票标签仅 `RIVE-LOST-*`；**INCONCLUSIVE ≠ PASS ≠ FAIL（恢复证伪）≠ 默认可合入 main ≠ ship**
- 不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；不改 composition.h
- 不把非 PASS 写成 PASS；不立刻 Δv″；不抬 thr；不开无关 M2 翻盘
- 产物前缀：`m3_rive_lost_*` / `m3lost_*` / `notion_m3_rive_lost_*`
- 本地 LOST gate **仅 DRAFT**：**不**把 DRAFT 写成 LOCKED；**不**伪造 GATE_BODY_SHA256

## 写结

- written_at: 2026-10-29 17:45:00 Asia/Shanghai
- notion ai（动效框架执行助手）结题；**本任务未采数、未派 workbuddy、未改 composition.h、未改 DRAFT body**；依据 workbuddy ONELINE + `docs/m3_rive_lost_gate_DRAFT.md` + DPI 结题样式对照

## 更正附录（pidan 裁决 r2 L5，2026-10-29；正文不改）
- **标签按 r1 LOCKED G2 记为 `RIVE-LOST-INVALID`**（“注入命令执行了但探针 30 s 内没检测到丢失”）。与正文的 INCONCLUSIVE 同为非 PASS、未采用，结论不变。
- **闸门已 LOCK 且 APPROVED**：`docs/m3_rive_lost_gate_LOCKED.md`，GATE_BODY_SHA256=`753c0a6c82f06c81cf741eecba278b926127771befa2a51b515a925a7873f27d`（LOCKED_AT 2026-10-29 12:31:48 AWST；在 tip `82a53cc` 上复算 MATCH）；`docs/m3_rive_lost_approved.md`（pidan @ 2026-10-29 12:48:46 AWST）。正文“本地无 LOCKED / approved”是协调侧本地副本过旧所致。
- **应用没有崩溃**：`CRASHED_NO_VERDICT` 是 runner 先结束进程、再判断是否退出造成的误标（r2 L3 已修）。
- **注入未生效**：`dxcap -forcetdr` 退出码 0、无输出、0 条 4101 事件。r2 改用 `pnputil /restart-device`（L1）。
- 详见 `docs/m3_rive_lost_result_claude_review.md`。
