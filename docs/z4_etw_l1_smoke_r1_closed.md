# Z4 · ETW + L1 smoke r1 结题（z4_etw_l1_smoke_r1_closed.md）

STATUS: CLOSED
TICKET: Z4-ETW-L1-SMOKE-PASS
ROUND: r1
METRICS: METRICS_PASS（有效会话 G2 ①–⑥ 全绿）
ADOPT: **不自动采用**（Success≠adopt；不合 main；本结题不写 adopt；有条件采用须 **pidan 另拍板**）
WRITTEN_AT: 2026-11-11 10:02:00 Australia/Perth（AWST，UTC+8）
GATE: docs/z4_etw_l1_smoke_gate_r1_LOCKED.md；GATE_BODY_SHA256=`acb89d1b0cb975e5d63ae2c6fc47adcbc74dddd959b63089b2546b692cadab14` **MATCH**（LOCKED_AT 2026-11-10 23:09:00 AWST；本机按约定排除 `GATE_BODY_SHA256` 行后复算，与 LOCKED 声明一致）
APPROVED: pidan 口令「APPROVED Z4 r1」；APPROVED_AT=2026-11-10 23:16 AWST；留证 `docs/z4_etw_l1_smoke_r1_approved.md`（本结题**不新签** APPROVED；**不改** LOCKED 闸门正文）
BRANCH: claude/z3a-cabi-l1
TIP_READ（采数声明 tip）: fe59c6229784ee7712926e3e0f94bcc0187f9069
RESULTS_COMMIT: 0aa4cd665438d2d3f612f0cc7221ee8f4dd72aa5（parent=`fe59c6229784ee7712926e3e0f94bcc0187f9069`；40 个文件；未合 main、未 force）
RESULTS: z4/results_etw_l1_smoke_r1/（事实清单 `docs/z4_etw_l1_smoke_r1_k93_iji_manifest.md`；本结题数字以该清单与各会话 `verdict.json` 为准，不另造）
HOSTS: DESKTOP-K93N3P8（Win10，`Microsoft Windows NT 10.0.19045.0`）+ DESKTOP-IJI1KJG（Win11，`Microsoft Windows NT 10.0.26300.0`）
composition.h: PRE=POST；归一化（CRLF→LF）=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`；raw（CRLF）=`33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97`；**未改**
PERF-CI: HOLD（各 PASS `verdict.json` 均为 `perf_ci=HOLD`；本票不是 PERF-CI，不判帧预算）
Success≠adopt: 是
本结题: **纯文档 ≠ 采数 ≠ 授权真机**；不改 LOCKED 闸门正文；不 LOCK 新闸门；不改 composition.h

## 正式判定

**票级 = CLOSED；标签 `Z4-ETW-L1-SMOKE-PASS`。**

按闸门原文：PASS ⇔ 每个**可裁决会话**满足 G2 ①–⑥。两机有效会话共 6 个（K93N3P8：s2 / s4 / s5；IJI1KJG：s1 / s2 / s3），均为 `status=PASS`，G2 ①–⑥ `pass: true`，`runtime_matches_frozen=true`，`invalid_reasons=[]`，`check_trace` `verdict=PASS`（`magic=ok provider=ok guid=absent_from_metadata`；六个事件名均为 ok）。每台均满足「≥ 3 个独立进程会话，且含 1 打包 + 1 非打包」（E2）。

K93N3P8 s1、s3 记 **INVALID** 并保留：属操作问题（非打包误用松散布局 exe / 其拷贝，启动即 `COMException (0x80040154)`，Z4 场景未开始，无 `verdict.json` / 会话产物）。按 pidan 2026-11-11 06:55 Australia/Perth 裁决，失败会话号不复用，另开 s4、s5。**INVALID 不计入 FAIL，不触发失败重跑，不改变本票 PASS 判定。** 试跑均不计会话。

**不采用、不合 main。** Success≠adopt。本结题不把 L1 smoke PASS 写成可 ship 或 PERF-CI 已通过。

## pidan 裁决（闸门正文未改）

以下裁决解释/适用既有门槛，**未改** LOCKED 正文，**未改** 单帧步长门槛 80 DIP，**未改** dynTol（各 PASS `verdict.json` 记 `dynTol=max(2, |vel|*0.040+1)`）。

1. 2026-11-11 06:24 Australia/Perth：G2② 按逐帧 OnFrame 步长 `max_frame_step_dip` < 80 判定；轮询累计 `probe_max_step_dip` **只作诊断**，不单独构成 FAIL。
2. 同日：打包会话用开发者模式松散注册（`Add-AppxPackage -Register`，`SignatureKind=None`，`packaged=true`）。
3. 2026-11-11 06:55 Australia/Perth：K93 s1 / s3 记 INVALID 保留；非打包独立构建后另开 s4、s5（失败会话号不复用）。

## 有效会话（G2 ①–⑥ PASS）

`probe_max_step_dip` 仅诊断。所有有效会话：`jumpFails=0`，`frame_step_fails=0`，`frame_step_limit_dip=80`，`retargets=7`（含 mid_interrupt / same_prop / cross_anim），`cancel=true`，`release_motion_continued=true`，故意出错 token 诊断带 JSON-Pointer 路径 `/bogus`；减少动效 ③ PASS；空闲 ④ `frames_to_stop=0`、`callbacks_in_5s=0`、`idle_transition_events=10`；线程 ⑤ `cross_thread_hits=0`；ETW ⑥ 主路径为 EventPipe（E3：`logman` ETL→WPA 为可选对照，**不是** PASS 必要条件；本票未把它当作通过条件）。六个冻结事件名（`AnimationLifecycle` / `Retarget` / `FrameTick` / `PresentStats` / `IdleTransition` / `TokenLoad`）均在 EventPipe 结果中为 ok；provider `FluidMotion-Motion`，GUID `8f3a2c1d-6b4e-4a91-9c7d-2e5f8a1b3c4d` 与冻结一致。`mode=Z4`，`not_z3a_dev=true`。

| 机台 | 会话 | 形态 | max_frame_step_dip | probe_max_step_dip（诊断） | max_frame_step_dt_ms | frame_steps_observed | 判定 |
|---|---|---|---|---|---|---|---|
| K93N3P8 | s2 | 打包（松散注册；`packaged=true`，`windows_package_type=msix`，`package_full_name=MotionSoftDemo_1.0.0.0_x64__zb6e6ksq6rq3m`） | 59.556363960383294 | 59.556363960383294 | 31.3512 | 54 | PASS |
| K93N3P8 | s4 | 非打包（独立 `WindowsPackageType=None` 构建） | 58.75254321577532 | 100.52055126421062 | 31.0939 | 54 | PASS（probe 超 80 只作诊断，不判 FAIL） |
| K93N3P8 | s5 | 非打包（与 s4 同一份输出） | 59.30694151773841 | 79.32506072134615 | 30.6259 | 51 | PASS |
| IJI1KJG | s1 | 非打包 | 11.401583976065893 | 63.552718670445216 | 5.6892 | 311 | PASS |
| IJI1KJG | s2 | 打包（松散注册；`SignatureKind=None`，`IsDevelopmentMode=True`） | 11.40885089835919 | 64.57578701379745 | 5.6636 | 311 | PASS |
| IJI1KJG | s3 | 非打包（与 s1 同一份输出） | 11.39727848613424 | 62.13105502537627 | 5.6243 | 305 | PASS |

帧间隔旁证（非 PERF-CI、不构成帧预算判定）：K93 有效会话 `max_frame_step_dt_ms` ≈ 30.6–31.4 ms；IJI ≈ 5.6 ms。

## INVALID（保留，不计入失败重跑）

| 机台 | 会话 | 记录 | 说明 |
|---|---|---|---|
| K93N3P8 | s1 | `INVALID.md` + `Report.wer`（sha `9a0ee9ef7c0da15dde42e17a095a30a0704809a9e7b28892e95ab6867b2d88d2`） | 启动的是已注册包 InstallLocation 下的松散布局 exe；`DeploymentManagerCS.AutoInitialize` 抛 `COMException (0x80040154)`；2026-11-11 06:38:22.93 Australia/Perth；`EXE_EXIT=-532462766`；无会话产物 |
| K93N3P8 | s3 | `INVALID.md` + `Report.wer`（sha `44a18610a4ae71c8da509cb4b7fe5e75c288eb611c4e7a0d11643657da796969`） | 启动的仍是从 InstallLocation 拷出并去掉清单的松散布局产物，不是独立非打包构建；同样 `0x80040154`，场景未启动；2026-11-11 06:41:26.19 Australia/Perth |

## 授权 / 闸门核验

| 项 | 结果 |
|---|---|
| GATE_BODY_SHA256 | `acb89d1b0cb975e5d63ae2c6fc47adcbc74dddd959b63089b2546b692cadab14` **MATCH**（本机复算） |
| LOCKED 正文 | **未改**（阈值 80 DIP 与 dynTol 未改） |
| APPROVED | pidan「APPROVED Z4 r1」@ 2026-11-10 23:16 AWST；留证 `docs/z4_etw_l1_smoke_r1_approved.md` |
| 采数 tip | 已声明并与各 PASS `env.json` / `session.log` 一致：`fe59c6229784ee7712926e3e0f94bcc0187f9069` @ `claude/z3a-cabi-l1` |
| composition.h | PRE=POST；守冻结 |
| 机台 / 会话（E2） | 仅点名的两机；有效会话每机 3 个，且各含打包 + 非打包 |
| 采集（E3） | EventPipe 为主路径；各 PASS `g2.6_etw.source=eventpipe` |
| 宿主（E4） | 仅 MotionSoftDemo；`mode=Z4`，`not_z3a_dev=true` |
| 结果提交 | `0aa4cd665438d2d3f612f0cc7221ee8f4dd72aa5` 推送到 `z4/results_etw_l1_smoke_r1/` + 事实清单；未合 main、未 force |
| 合 main / adopt | **否** |
| 本结题任务 | **纯文档结题 ≠ 采数**；不 LOCK 新闸门 |

补充事实（不构成 FAIL）：K93 s2 会话树无 `BUILD_NOTES.md`；该会话 `SignatureKind=None` / `IsDevelopmentMode=True` 来自归档时只读查询 `Get-AppxPackage -Name MotionSoftDemo`，不是会话目录内文件。IJI s2 的同等字段写在该会话 `BUILD_NOTES.md`。

## 两机二进制 sha（预期不同）

两机链接器 PE Optional Header 版本戳均为 `14.44`，但 exe / 托管 dll / `fluidmotion.dll` 的 sha（以及 `fluidmotion.dll` 体积 438784 对 448000、托管非打包 dll 体积 1315328 对 1314816）在两机之间均不同，与各自本机工具链产出一致，属预期。数值见事实清单第 2 节，本结题不重复造表。`z4_build_info.txt` 在两侧 sha 均为 `c7169179725d0acb44833e6b8077d29af00b90a8e19d2a08075950709023f3d7`（内容为声明 tip 与 composition 哈希）。

## 采用口径

| 项 | 决断 |
|---|---|
| 采用 tip / 合 main | **否**（默认）；有条件采用须 **pidan 另拍板** |
| 采用「L1 smoke PASS = 可 ship / PERF-CI 已过」叙事 | **否**（Success≠adopt；PERF-CI HOLD；本票不判帧预算） |
| 引用实验证据（有效会话全过、INVALID 已归档、results commit、清单） | **可引用**；须同时写明 **Success≠adopt**、不合 main、PERF-CI HOLD |
| 解冻 PERF-CI / 解冻 Bv / 重开 S1 / R-3 / R-2 | **否** |
| 改 composition.h / 改 LOCKED 闸门正文 | **否** |
| 把 INVALID 改判 FAIL 或用失败会话号重跑凑数 | **否** |
| 把 Z3a 开发冒烟当作本闸门会话 | **否** |
| 本结题授权新采数或真机 | **否** |

## 残差 / 挂账

| 项 | 处置 |
|---|---|
| 有效会话 G2 ①–⑥ 全过 | 本票 CLOSED PASS；**不**自动 adopt |
| K93 s1 / s3 INVALID | **保留归档**；不计入 FAIL；不复用会话号 |
| K93 s4 `probe_max_step_dip=100.52055126421062` | **诊断字段**；按已裁决口径不判 FAIL |
| `logman` ETL→WPA 对照 | 可选，非 PASS 条件；本票未作为通过依据 |
| PERF-CI | **HOLD** |
| 有条件采用本 tip | **开放**；须 pidan 拍板 |
| 下一票 | 本结题**不**自动 LOCK；不授权真机 |

## ONELINE（结题口径）

```
Z4 ETW+L1 smoke r1 | gate=acb89d1b0cb975e5d63ae2c6fc47adcbc74dddd959b63089b2546b692cadab14 @ 2026-11-10 23:09:00 AWST MATCH | approved=pidan「APPROVED Z4 r1」@2026-11-10 23:16 AWST | hosts=K93N3P8+IJI1KJG | valid=K93 s2/s4/s5 + IJI s1/s2/s3 all G2 ①–⑥ PASS | invalid=K93 s1,s3 kept not FAIL | packaged+unpackaged both hosts | ticket=Z4-ETW-L1-SMOKE-PASS | dir=z4/results_etw_l1_smoke_r1/ | tip=fe59c6229784ee7712926e3e0f94bcc0187f9069 | results=0aa4cd665438d2d3f612f0cc7221ee8f4dd72aa5 | composition PRE=POST a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418 | Success≠adopt | 不合main | 不改LOCKED | PERF-CI HOLD
```

## 产物

- 权威结题草稿（本文件，待协调派推送）：`docs/z4_etw_l1_smoke_r1_closed.md`
- 事实清单（已在 results commit）：`docs/z4_etw_l1_smoke_r1_k93_iji_manifest.md`
- 只读结果：`z4/results_etw_l1_smoke_r1/`（commit `0aa4cd665438d2d3f612f0cc7221ee8f4dd72aa5`）
- 闸门 / 留证（正文不动）：`docs/z4_etw_l1_smoke_gate_r1_LOCKED.md`；`docs/z4_etw_l1_smoke_r1_approved.md`

## 禁令复述

不改 composition.h；不合 main；Success≠adopt；本结题不写 adopt；PERF-CI HOLD；不改 LOCKED 闸门正文；不把 L1 smoke PASS 写成 PERF-CI 已过；不把 INVALID 计为 FAIL 或失败重跑；不把 Z3a 开发冒烟当本闸门会话；本结题不授权新采数/真机；不自动 LOCK 下一票。

— notion ai（Z4 ETW+L1 smoke r1 结题；CLOSED Z4-ETW-L1-SMOKE-PASS；有效会话全过；INVALID 不计入失败重跑；Success≠adopt；不合 main；Australia/Perth（AWST，UTC+8））
