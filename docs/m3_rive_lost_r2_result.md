# M3-RIVE-LOST r2 结题结果

STATUS: CLOSED
TICKET: RIVE-LOST-FAIL
METRICS: 非 METRICS_PASS（探测已过；恢复未成立；三会话 FAIL_ABORT）
ADOPT: 未采用（成功 ≠ 采用；本票无采用结论可采）
CSPROJ_BACKPORT: 否（不回灌主线）
WRITTEN_AT: 2026-10-30 08:50:00 Asia/Shanghai
GATE: 本轮真机对照 `docs/m3_rive_lost_probe_gate_LOCKED.md`（GATE_BODY_SHA256=`7afbcd980d464abdeb835f059e05089edbf1d8f1112efb3e1d1330bd386e6b54`；LOCKED_AT=2026-10-29 17:49:40 Asia/Taipei）；原 LOST 仍仅 `docs/m3_rive_lost_gate_DRAFT.md`（**未改**；**不**翻盘 r1）
GATE_BODY_SHA256: `7afbcd980d464abdeb835f059e05089edbf1d8f1112efb3e1d1330bd386e6b54`（PROBE LOCKED；本结题**未改** body）
LOCKED_AT: 2026-10-29 17:49:40 Asia/Taipei（UTC+8）
APPROVED: 本地**无** `docs/m3_rive_lost_probe_approved.md` / `docs/m3_rive_lost_approved.md`（本结题**不虚构**授权；仅据 workbuddy ONELINE 已跑事实归档）
BRANCH: claude/m3-rive-lost
COMMIT: 382683cfa369f789a8f926ee7c0eba0a581a04ec
TIP_READ: 0aea9d172b28065b453abb3b635eccb5e1012807
HOST: （ONELINE 未单列 host 名；与 r1/DPI 同机惯例 DESKTOP-IJI1KJG；注入 selftest=`pnputil /restart-device` RX7800XT）
composition.h: PRE=POST；CRLF=`33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97`；LF=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`；edited=False
RESULTS_DIR: `m3/results_lost_r2/`（**r1 未触**；本根本地**无**可读矩阵文件 → 以 ONELINE 为准）

## 授权 / 闸门核验摘要

| 项 | 结果 |
|---|---|
| PROBE LOCKED | 存在：`docs/m3_rive_lost_probe_gate_LOCKED.md`；hash `7afbcd98…6b54` |
| LOST DRAFT | 仍存在：`docs/m3_rive_lost_gate_DRAFT.md`（STATUS=DRAFT；**未改**；r1 结论不翻盘） |
| approved 文件 | **本地不存在** PROBE/LOST approved |
| 本结题任务 | **不采数、不派 workbuddy、不重跑**；仅据 workbuddy ONELINE + 本地文档结题 |
| composition.h | **未改** PRE=POST；本地 `code/composition.h` LF/CRLF 与 ONELINE 全 hash **一致** |
| PERF-CI | **仍 HOLD** |
| r1 CLOSED | **不改写**：`docs/m3_rive_lost_result.md` 仍为 `RIVE-LOST-INCONCLUSIVE` |

## workbuddy ONELINE（核实后；**字段勿改写**）

```
M3-RIVE-LOST r2 | commit=382683cfa369f789a8f926ee7c0eba0a581a04ec @ claude/m3-rive-lost | tip_read=0aea9d172b28065b453abb3b635eccb5e1012807 | BUILD_OK（DLL reuse BD6F4827…）| SIM_SMOKE=PASS（recover_ok=1 recover_max_ms≈221/210 probe=M3-LOST v2）| selftest=OK（pnputil /restart-device RX7800XT exit0 ~3.3s）| elevated=True | sessions=3/3 | s1/s2/s3=FAIL_ABORT rive_abort=1 | lost_detected=1/1/1 | recover_ok/recover_max_ms/memΔ=N/A（无 recovery）| injections_ok=1/1/1（每会话 2 次里只做成 1 次；第一次后 app 死）| event4101=0（pnputil 预期）| bugcheck=0 | composition PRE=POST CRLF=33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97 LF=a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418 edited=False | results=m3/results_lost_r2/（r1 未触）| K7 finding delivered no re-run | Success≠adopt | PERF-CI HOLD | ticket=RIVE-LOST-FAIL | 非METRICS_PASS | 未采用
```

## 与 r1 对照（写死）

| 项 | r1（CLOSED INCONCLUSIVE） | r2（本票 CLOSED FAIL） |
|---|---|---|
| tip / commit | `82a53cce…02694d` | commit=`382683cf…1a04ec`；tip_read=`0aea9d17…012807` |
| sessions | 1/3 | **3/3** |
| 会话结局 | s1=CRASHED_NO_VERDICT | s1/s2/s3=**FAIL_ABORT**（rive_abort=1） |
| 注入检出 | inject1 30s 未检出；lost_detected=N/A | **lost_detected=1/1/1**（探测成立） |
| injections_ok | N/A（停于首注未检出） | **1/1/1**（每会话目标 2 次里只做成 1 次；第一次后 app 死） |
| recover_ok / recover_max_ms / memΔ | N/A（未进入） | **N/A（无 recovery）** — 进入探测后 abort，**恢复未成立** |
| SIM_SMOKE | PASS | PASS（recover_ok=1 ≈221/210；**仅 SIM**，**勿升真机 PASS**） |
| 票标签 | **RIVE-LOST-INCONCLUSIVE** | **RIVE-LOST-FAIL** |
| 口径 | 探针/检测窗未成立 → 恢复**未被证伪亦未被证实** | 探测路径已越过；恢复目标未达 → **票级 FAIL** |

> **勿翻盘 r1**：r1 仍为 INCONCLUSIVE（探针问题）；r2 **不**把 r1 改写成 PASS/FAIL。  
> **勿把 SIM_SMOKE PASS 当成真机 PASS**。

## 票结局声明

- 真机：**sessions=3/3**；**lost_detected=1/1/1** → 相对 r1，**探针/检测路径已成立**。
- 恢复：**s1/s2/s3=FAIL_ABORT**；**rive_abort=1**；**recover_ok/recover_max_ms/memΔ=N/A（无 recovery）**；**injections_ok=1/1/1**（第二次注入未做成，因首次后 app 死）→ **恢复路径未成立 / 被证伪于 abort**。
- 票标签 **`RIVE-LOST-FAIL`**；STATUS=**CLOSED**；协调对照 **非 METRICS_PASS**。
- **口径说明（FAIL vs INCONCLUSIVE）**：
  - **取 `RIVE-LOST-FAIL`**：恢复目标（重建 D3D + Rive + `SetRenderingDevice` 后继续渲染）**未达**；三会话可裁决为 FAIL_ABORT，非「未进入裁决」。
  - **非 INCONCLUSIVE**：r1 的探针缺口在 r2 已越过（lost_detected=1/1/1）；本票失败点在 **abort / 无 recovery**。
  - **非 PASS / 非 METRICS_PASS**：SIM_SMOKE 与 selftest 前置 OK **不升**真机 METRICS_PASS。
- **明确声明**：**成功 ≠ 采用 ≠ 默认可合入 main ≠ 授权改 composition.h ≠ PERF-CI ≠ 解冻 Bv ≠ 翻盘 M2 ≠ 重开 S1/R-3/R-2 ≠ 翻盘 r1**。
- **是否采用 = 未采用**：无 PASS 验证结论可有条件采用；commit `382683cf…` / tip_read `0aea9d17…` **不**视为可合入主线。
- **composition.h 未改**；**PERF-CI 仍 HOLD**；**K7 finding delivered no re-run**；**禁当日重跑**本 r2。

## METRICS 核实（对照 ONELINE；本地无 `m3/results_lost_r2/` 矩阵可读）

| 项 | ONELINE | 对照意图（PROBE/LOST 恢复目标） | 结果 |
|---|---|---|---|
| sessions | 3/3 | ≥3 | **会话数满足** |
| s1/s2/s3 | FAIL_ABORT；rive_abort=1 | 每会话可裁决且恢复成功 | **FAIL（abort）** |
| lost_detected | 1/1/1 | ① 注入须被探针捕获 | **探测 PASS（相对 r1 已越过）** |
| injections_ok | 1/1/1（目标 2 次/会话） | N_inj=2 均须成立 | **未满 2**（首次后死） |
| recover_ok | N/A（无 recovery） | ② 恢复成功 | **未成立 → FAIL** |
| recover_max_ms | N/A | ③ 只记录 | **N/A** |
| memΔ | N/A | ⑥ 只记录 | **N/A** |
| event4101 | 0（pnputil 预期） | 记录 | 记录 |
| bugcheck | 0 | 记录 | 记录 |
| BUILD_OK / SIM_SMOKE / elevated / selftest | BUILD_OK（DLL reuse BD6F4827…）/ PASS / True / OK | 前置 | 前置 OK；**不升 METRICS_PASS** |

**ticket 判据：非 METRICS_PASS** → 标签 **`RIVE-LOST-FAIL`**（恢复未成立）。

## 采用决断（写死）

| 项 | 决断 | 理由 |
|---|---|---|
| 票标签 | **RIVE-LOST-FAIL** | 探测已过、恢复未成立；三会话 FAIL_ABORT |
| METRICS | **非 METRICS_PASS** | 无 recovery；rive_abort；injections 未满 2 |
| 是否采用 | **未采用** | 成功≠采用；无 PASS 结论 |
| composition.h | **未改** PRE=POST 全 hash | 硬禁；edited=False |
| csproj 回灌主线 | **否** | 沿用 DPI/r1 边界 |
| PERF-CI | **HOLD** | 硬禁 |
| 当日重跑本 r2 | **否** | K7 finding delivered no re-run；禁当日重跑 |
| 翻盘 r1 | **否** | r1 仍 INCONCLUSIVE |

## 挂账 / 残差

| 项 | 处置 |
|---|---|
| rive_abort / 无 recovery | **挂账 → 下一票 RECOVER**：专攻 abort 后重建 D3D+Rive+SetRenderingDevice |
| injections 仅 1/2（首次后 app 死） | **挂账**：与 abort 同源；RECOVER 闸门须写死二次注入可达 |
| 探测能力（lost_detected=1/1/1） | **r2 实证成立**；PROBE LOCKED `7afbcd98…6b54` 可标「探测能力已由 r2 实证」挂账关闭或保留为 RECOVER 前置（见 next） |
| K7 finding | **delivered；no re-run**（遵守） |
| SIM_SMOKE PASS | **仅 SIM**；不升真机 PASS |
| 当日不重跑 | **遵守**；新跑须新 DRAFT→LOCK→APPROVED |
| Lab / Demo csproj | **不回灌主线** |
| M2 旁路 / VERIFY-FAIL | **不翻盘** |
| PERF-CI | **仍 HOLD** |
| FOLLOW / MATRIX | **不插队**（恢复未成立前） |
| 下一张 | 见 `docs/notion_m3_rive_lost_r2_next.md`（`M3-RIVE-LOST-RECOVER` 新 DRAFT；**不**授权采数） |

## 标签边界

- 本票标签仅 `RIVE-LOST-*`；**FAIL ≠ PASS ≠ INCONCLUSIVE（r1）≠ 默认可合入 main ≠ ship**
- 不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；不改 composition.h
- 不把非 PASS 写成 PASS；不立刻 Δv″；不抬 thr；不开无关 M2 翻盘
- **不**改写 r1 CLOSED 文件把 INCONCLUSIVE 改 PASS/FAIL
- 产物前缀：`m3_rive_lost_r2_*` / `notion_m3_rive_lost_r2_*`（结果目录 `m3/results_lost_r2/`；**不覆写** r1 / probe 目录约定）
- 本结题**不**把 LOST DRAFT 写成 LOCKED；**不**改 PROBE LOCKED body/hash

## 写结

- written_at: 2026-10-30 08:50:00 Asia/Shanghai
- notion ai（动效框架执行助手）结题；**本任务未采数、未派 workbuddy、未改 composition.h、未改 r1 CLOSED、未改 ONELINE 事实字段、未改 PROBE/LOST gate body**；依据 workbuddy ONELINE + PROBE LOCKED + LOST DRAFT + DPI/r1 结题样式对照

## 更正附录（pidan 裁决 RECOVER M5，2026-10-30；正文不改，FAIL 结论不变）
- **本次运行对应的闸门是 r2 LOCKED**：`docs/m3_rive_lost_gate_r2_LOCKED.md`，GATE_BODY_SHA256=`dc0770c1f6a6819a5f6cffeb6bea2ef828b93045ec8e75f39910ee9537d29d10`（LOCKED_AT 2026-10-29 18:13:07 AWST；在 `382683c` 上复算 MATCH）。正文引用的 `docs/m3_rive_lost_probe_gate_LOCKED.md`（`7afbcd98…6b54`）**不在仓库中**，是协调侧本地副本另起的文件。
- **采数有授权**：`docs/m3_rive_lost_r2_approved.md`（pidan @ 2026-10-29 18:15:58 AWST）。
- **r1 标签**已按 L5 更正为 `RIVE-LOST-INVALID`（见 `docs/m3_rive_lost_result.md` 末尾附录）。
- abort 位置分析见 `docs/m3_rive_lost_r2_result_claude_review.md`；下一票 `docs/m3_rive_lost_recover_gate_LOCKED.md`。
