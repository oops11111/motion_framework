# M3-RIVE-LOST-RECOVER 结题中文包（notion_m3_rive_lost_recover_pack.md）

> DECISION_AT=2026-10-30 12:10:00 Asia/Shanghai  
> 权威结题：`docs/m3_rive_lost_recover_result.md`（WRITTEN_AT=2026-10-30 12:10:00 Asia/Shanghai）  
> 闸门：`docs/m3_rive_lost_recover_gate_DRAFT.md`（STATUS=**DRAFT**；**未 LOCK**；本结题**未改**）  
> APPROVED：本地**无** `docs/m3_rive_lost_recover_approved.md` → **不虚构**批准文件；采数口径=**按用户指令采数 / 闸门未 LOCK**（协调转述：pidan 跳过开放问题拍板，直接下令 4 步采数；协调侧视该指令为本次采数授权；据转述 + 已发生结果落盘）  
> 计划：`docs/m3_plan_DRAFT.md` §7 第 2 张票（WP1 设备丢失恢复）· **RECOVER**  
> 前序：r1 CLOSED `RIVE-LOST-INCONCLUSIVE`（**勿改**）；r2 CLOSED `RIVE-LOST-FAIL`（**勿改**）；PROBE LOCKED `7afbcd98…6b54`（**不改**；探测 r2 实证）；DPI CLOSED `RIVE-DPI-PASS` gate `92e6b4e9…97bf5` **不改**  
> 分支：tip_read=`c954696470849f8f466d3c5d7f7dd57159cd8cbd`；commit=`68ce59d3cea4c9e8ec155015207113ce718a7ab6`；tip_after_push=同 commit @ `claude/m3-rive-lost`  
> dll_sha256=`d3aef01a0e410c30a147588cbc3dab6be18f0fdf50ba2674d1223e9ca1f0b840`  
> composition.h PRE=POST CRLF=`33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97` LF=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` edited=False  
> 旁路：`docs/side_parked_hang_degrade.md`（M2；**本票勿翻盘 M2**）

## 结论

- **票级 = CLOSED**；标签 **`RIVE-LOST-RECOVER-PASS`**；协调对照 **METRICS_PASS（有条件）**。
- **是否采用 = 有条件采用验证结论**（成功 ≠ 采用；**不合 main**）。
- **csproj 回灌主线 = 否**。
- **闸门未 LOCK**；**按用户指令采数**（协调转述授权；本地无 approved 文件可引用）。
- ONELINE（结题；字段与 workbuddy 一致，仅追加票标签/口径）：  
  `M3-RIVE-LOST-RECOVER | tip_read=c954696470849f8f466d3c5d7f7dd57159cd8cbd | commit=68ce59d3cea4c9e8ec155015207113ce718a7ab6 @ claude/m3-rive-lost | tip_after_push=同 commit | BUILD_OK | dll_sha256=d3aef01a0e410c30a147588cbc3dab6be18f0fdf50ba2674d1223e9ca1f0b840 | SIM_SMOKE=PASS（SIM_RECORDED recover_ok=1 recover_max_ms=756.94 probe=M3-LOST v3）| selftest=OK（pnputil exit0 ~3.3s）| elevated=True | s1/s2/s3=RECORDED/RECORDED/RECORDED | lost_detected=2/2/2 | recovered=2/2/2 | recover_ok=1/1/1 | recover_max_ms=2319.99/2206.54/2160.70 | rive_abort=none | budget_ok/stability_ok=1/1 | frames_missed=331/347,341/342,340/331 | mem_delta_mb=-23/-25/-25 | injections_ok=1/1/1 | composition PRE=POST CRLF=33d826c7… LF=a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418 edited=False | results=m3/results_lost_recover/（31 files incl s1–s3 _stderr.txt）| gate=DRAFT未LOCK | 按用户指令采数 | ticket=RIVE-LOST-RECOVER-PASS | METRICS_PASS有条件 | 有条件采用验证结论；不合main | Success≠adopt | PERF-CI HOLD`
- **声明**：`RIVE-LOST-RECOVER-PASS` / 有条件 METRICS_PASS ≠ 默认可合入 main ≠ ship ≠ 闸门已 LOCK ≠ 改 composition.h ≠ PERF-CI ≠ 解冻 Bv ≠ 翻盘 M2 ≠ 翻盘 r1/r2。
- `composition.h` **本票未触**；RECOVER DRAFT **未改成 LOCKED**；r1/r2 CLOSED **未改**；PROBE LOCKED **未改**。
- **本票 ≠ PERF-CI**；PERF-CI 仍 HOLD；禁影子 IV；不解冻 Bv；不重开 S1/R-3/R-2；**禁**把 SIM_SMOKE PASS 写成真机唯一 PASS。

## 口径（PASS 有条件；对照 r2 FAIL）

| 选项 | 决断 | 理由 |
|---|---|---|
| RIVE-LOST-RECOVER-FAIL | **否** | rive_abort=none；recover_ok=1；三会话 RECORDED；相对 r2 恢复已成立 |
| RIVE-LOST-RECOVER-INCONCLUSIVE | **否** | 非「未进入裁决」；可裁决会话齐 |
| **RIVE-LOST-RECOVER-PASS** | **是（有条件 METRICS）** | ②④ ONELINE PASS；①凭 lost_detected/recovered=2 有条件齐；③ frames_missed 挂账 |
| 采用 | **有条件采用验证结论；不合 main** | 成功≠采用 |
| 翻盘 r1/r2 | **否** | 硬禁 |

## 授权核验

| 项 | 值 |
|---|---|
| RECOVER DRAFT | `docs/m3_rive_lost_recover_gate_DRAFT.md`；**未 LOCK**；未改 |
| GATE_BODY_SHA256 | **无** |
| approved | **无**（本根） |
| 采数口径 | **按用户指令采数 / 闸门未 LOCK**（协调转述 pidan 4 步采数令；本结题不虚构批准文件） |
| 本结题采数 | **否**（只归档 ONELINE 已跑事实） |
| tip_read / commit | `c9546964…8cbd` / `68ce59d3…7ab6` |

## 事实表

| 项 | 值 |
|---|---|
| TICKET / STATUS | `RIVE-LOST-RECOVER-PASS` / **CLOSED** |
| METRICS | **METRICS_PASS（有条件）** |
| 采用 | **有条件采用验证结论；不合 main** |
| tip_read / commit / branch | `c954696470849f8f466d3c5d7f7dd57159cd8cbd` / `68ce59d3cea4c9e8ec155015207113ce718a7ab6` / `claude/m3-rive-lost` |
| BUILD / dll_sha256 | BUILD_OK / `d3aef01a0e410c30a147588cbc3dab6be18f0fdf50ba2674d1223e9ca1f0b840` |
| SIM_SMOKE | PASS（仅 SIM；recover_ok=1；756.94 ms；probe=M3-LOST v3） |
| selftest / elevated | OK（pnputil ~3.3s exit0）/ True |
| s1/s2/s3 | **RECORDED / RECORDED / RECORDED** |
| lost_detected / recovered | **2/2/2** / **2/2/2** |
| recover_ok | **1/1/1** |
| recover_max_ms | **2319.99 / 2206.54 / 2160.70**（只记录；超参考 500ms） |
| rive_abort | **none** |
| budget_ok / stability_ok | **1/1** |
| frames_missed | **331/347, 341/342, 340/331**（字段名≠frames_post；挂账） |
| mem_delta_mb | **−23 / −25 / −25** |
| injections_ok | **1/1/1**（有条件齐；见 result 挂账） |
| results | `m3/results_lost_recover/`（ONELINE 31 files；本根本地无矩阵可读） |
| composition.h | 未触；CRLF `33d826c7…6e97`；LF `a55d4952…e418`；edited=False |
| PERF-CI | HOLD |

## DRAFT G2 对照摘要

| 判据 | 结果 |
|---|---|
| ① 保活 / N_inj=2 | **有条件 PASS**（lost_detected/recovered=2；injections_ok 字段挂账） |
| ② recover_ok | **PASS** |
| ③ 稳态帧 ≥300 | **有条件 / 挂账**（`frames_missed`；勿盲升） |
| ④ 预算/稳定性 | **PASS**（budget_ok/stability_ok=1/1） |
| ⑤ abort 字段 | N/A（abort=none） |
| ⑥⑦ 耗时/内存 | **只记录**（耗时超参考不挡；mem 负值） |

## 产物清单

- `docs/m3_rive_lost_recover_result.md`（权威结题）
- `docs/notion_m3_rive_lost_recover_pack.md`（本包）
- `docs/notion_m3_rive_lost_recover_next.md`
- `docs/m3_rive_lost_recover_result_brief.md`（协调简报）
- brief：`docs/m2_mainline_brief.md`（追加 M3-LOST-RECOVER CLOSED 条目）
- 只读引用：RECOVER DRAFT；r1/r2 result/pack/next；PROBE LOCKED；DPI result；`m3_plan_DRAFT.md`
- 产物前缀 only：`m3_rive_lost_recover_*` / `notion_m3_rive_lost_recover_*`；结果目录 `m3/results_lost_recover/`

## 禁令复述

- 不重开 S1 / R-3 / R-2；不解冻 Bv；不改 composition.h；禁影子 IV
- 成功 ≠ 采用 ≠ 默认可合入 main；**不合 main**；PERF-CI HOLD
- 不把 SIM_SMOKE 当真机唯一 PASS；不立刻 Δv″；不抬 thr；不翻盘 M2
- **不**改写 r1/r2 CLOSED；**不**改 PROBE LOCKED；**不**把 DRAFT 写成 LOCKED；**不**伪造 approved
- 本结题任务 **不采数、不派 workbuddy**；禁插队未 LOCK 的真机采数授权

## 挂账 / 残差

| 项 | 处置 |
|---|---|
| M3-RIVE-LOST-RECOVER（本票） | **CLOSED RIVE-LOST-RECOVER-PASS**；METRICS_PASS 有条件；有条件采用；不合 main |
| 闸门未 LOCK / 无 approved | **写死口径**；建议协调补 LOCK 归档（见 next；**勿当采数授权**） |
| frames_missed / injections_ok | **挂账复核** results |
| recover_max_ms ~2.2s | **记录**；可作后续观察 |
| Lab csproj | **不回灌主线** |
| M2 旁路 | 保持；不翻盘 |
| PERF-CI | 继续 HOLD |
| 下一张 | 见 next → **FOLLOW DRAFT**（可开文档）；**不**授权采数 |

## 下一票决断

详见 `docs/notion_m3_rive_lost_recover_next.md`。  
**主推：准备 `M3-RIVE-FOLLOW` 新 DRAFT（文档）**；建议补 RECOVER 闸门 LOCK 归档（事后纸面；**不**授权采数）；MATRIX 仍排 FOLLOW 后；**本 pack/next 不授权采数**；不解冻 PERF-CI。

— notion ai（M3-RIVE-LOST-RECOVER 结题；CLOSED PASS；METRICS_PASS 有条件；按指令采数/闸门未 LOCK；有条件采用验证结论；不合 main）
