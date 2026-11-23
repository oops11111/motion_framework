# M2-PERF-CI 门槛闸门 r2（m2_perf_ci_gate_r2_DRAFT.md）

STATUS=DRAFT（**NOT LOCKED**；本文件**不授权采数**；不解除 PERF-CI HOLD；不改 v1 LOCKED 闸门）
LOCKED_AT: （LOCK 时填）
GATE_BODY_SHA256: （LOCK 时算；约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希）

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> 起草：2026-11-23。触发：Windows 验证确立 demo 进程本身不 Present，现行判分实现（按 demo PID 过滤 PresentMon 行）下该 demo 永远采不到可判分轮次；Q1–Q8 裁决（见 `docs/m2_perf_ci_tooling_gate_DRAFT.md`）中 Q1/Q2/Q3/Q7 改判据，已言明须出新版闸门。本文件为该新版闸门草案，只定口径，不改 v1 闸门（`5b007dd4…282a` 不动）、不改 `composition.h`、不授权采数。
> PERF-CI gate `5b007dd4…282a` **不改、不翻盘**，仍 HOLD（等第二台物理机 + r2 LOCK + APPROVED）。八 hash 和 ABS-RESOLVE `e96bf623…5ade` 也不翻盘。

## G0 硬边界
- mode=**offline-decision**。采数=**否**；真机=**否**；落地=**否**。本票 PASS 仅表示口径已定，**≠ PERF-CI PASS ≠ 授权采数 ≠ 采用**。
- 不改 v1 LOCKED 闸门及其哈希；不改 `composition.h`（`a55d4952…e418`）。不重开 S1、R-3、R-2。不解冻 Bv。禁影子 IV。
- 必须 ≥2 台物理机；1 机 / VM / 同机复跑冒充 = 整票 INVALID（沿用 v1 G2-LOCK-10）。
- 本票不改判分脚本与采集 harness；LOCK 后另票实施（见“实施清单”）。

## 证据（已确立的事实，Windows 验证报告，用户提供，基线均为 main 提交）

1. **demo 进程不 Present**：0018/0019 harness（`tools/perf_ci/capture_run.ps1`）按 `--process_name MotionSoftDemo.exe` 采集，PresentMon 2.6.0（SHA256 `b2a706bc…188f1af`）正常启动（stdout 仅 `Started recording.`）、demo 正常退出、logman ETL 与 demo 日志齐全，但 CSV 始终不存在（非零字节、非仅表头）。加 `--terminate_on_proc_exit`、等 120 秒、正规 `--terminate_existing_session` 停止，均不产生 CSV（REPORT-ba0a5447.md，基线 34bc45e3）。
2. **不过滤 / 只滤 dwm.exe 能产 CSV**：E1（不传 `--process_name`）产出 CSV，12728 数据行，全采集分组 dwm.exe 4494 行、MotionSoftDemo.exe 按名及实测 PID 均为 0 行；E2（只滤 dwm.exe）产出 CSV，2082 行全为 dwm.exe（REPORT-e0e67a5f.md，基线 34bc45e3）。
3. **归属未确立**：CSV 时间基为相对列（`TimeInMs` / `MsBetweenSimulationStart`），未与 UTC 建立严格映射；启动前基线窗口仅约 0.13 秒；故不能由全采集行数差断言 demo 造成 dwm Present，窗口内/基线分组与 FrameTime 分布为 N/A（不是 0）。
4. **表头为 v1 指标**：PM 2.6.0 默认输出含 `MsBetweenPresents`，不含 `FrameTime`。判分脚本两种列均已支持（`FRAME_COLUMNS`），无兼容问题；r2 须把两种列写死（R6）。
5. **Q1–Q8 裁决已定**（2026-10-28，`m2_perf_ci_tooling_gate_DRAFT.md`）：Q1 Idle 按时长判定、断言 0 Present，1000 帧要求不适用；Q2 替代路径帧间隔 P95 ≤ 1.1·T；Q3 两边丢帧数都 ≤2 视为同量级；Q4 采数前实现真实 PresentStats 统计（已由 0009 落地：M202/Z4 会话汇总 + 0018/0019 harness）；Q5 P95 最近秩；Q6 WPA 阶段导出口径待懂 WPA 的人定义（**仍未定**）；Q7 先跑一轮只建基线、不判分；Q8 machineId 取 SMBIOS UUID。Q1/Q2/Q3/Q7 并入 r2；Q6 仍为采数前提，不在本票代定。

## 决策项（每一条须由协调裁决：(a) 按建议接受；(b) 改建议；(c) 另议）

### R1 采集范围：PresentMon 滤谁

- **问题**：v1 闸门文字称主判据为“系统 Present 侧帧级序列”，未写死过滤进程；判分脚本与 harness 的既成约定是按 demo PID 过滤。证据 1–2 证明该约定对合成器路径 demo 恒为空。
- **选项**：(a) 只滤 `dwm.exe`（E2 已证可行、文件最小、语义最干净）；(b) 不过滤、事后取 dwm.exe 行（保留全量、可复算，但文件大、含无关进程）；(c) 保留 demo PID 过滤 + 另立直接 Present 探针（如 m0 泪滴探针式）测性能（归因最干净，但测的是探针不是 demo，须另立场景定义）。
- **建议**：(a)。(b) 为备选；(c) 仅当协调认定 dwm 口径归因不可接受时采用。
- **影响**：闸门文字（本文件 R1–R3）+ harness 过滤参数 + 判分脚本行过滤（R2）。

### R2 行归属：PID 语义

- **问题**：判分脚本 `read_presentmon` 按 manifest run 的 `pid` 过滤 `ProcessID` 列；该 `pid` 现为 demo 进程。dwm 口径下须区分“场景进程”与“被测 Present 进程”。
- **建议**：manifest run 记录新增 `presentmonPid`（实测 dwm PID，采集时记录）与 `presentmonFilter`（过滤参数原文，如 `--process_name dwm.exe`）；`pid` 保留为场景进程（demo）身份；判分脚本按 `presentmonPid` 过滤（缺失时回落 `pid`，旧 manifest 可读）。harness 在 run.json / fragment 中同时记录 demo PID、dwm PID 与过滤参数。
- **影响**：manifest schema + 判分脚本 + harness（LOCK 后另票）。

### R3 安静桌面 precondition（归因有效性条件）

- **问题**：dwm Present 覆盖整桌面；其它应用的 Present 会污染计数，E1 全采集即有 msedgewebview2 / ChatGPT 等进程的行。无此条件，R1(a) 的计数不可归因。
- **建议**：采集窗口内除 dwm.exe 与 demo 进程外不得有其它进程 Present。核验办法：采集前后各一份全进程 PresentMon 清单（E1 式不过滤短采，或 CSV 全量分组），附产物；违反 = 该轮 INVALID。另要求 demo 窗口前台独占可见、其它窗口最小化，操作记入轮次元数据。
- **影响**：闸门新增有效性条件；harness 或手工核验程序（LOCK 后另票）。

### R4 时间锚定（E1 教训）

- **问题**：E1 未建立 CSV 时间基到 UTC 的映射，窗口分析无法做。不得以 PM 进程启动时刻冒充 CSV 零点。
- **建议**：harness 须记录 PM 启动 / demo 启动 / demo 退出 / PM 停止四项墙钟时刻（实际进程 StartTime/ExitTime）+ CSV 时间基原点说明，落 run.json；窗口分析以锚定为准。
- **影响**：harness（LOCK 后另票）。

### R5 Idle 在 dwm 口径下的重定义

- **问题**：Q1 裁决（Idle 按时长判定、断言应用 0 Present）与 dwm 口径直接冲突：dwm 恒 Present，0 Present 断言永假。
- **建议**：Idle 改判 PresentStats 侧（0 render 回调 / 无动画计时器，沿 Q1“闲置 0 计时器”本意）+ dwm 帧率不超过采前桌面基线（基线轮同法测量；超限倍数阈值待裁决，开放数，LOCK 前填）。1000 帧要求仍不适用于 Idle（沿 Q1）。
- **影响**：闸门 G1 场景 1 定义；Q1 裁决修订（须在 r2 中写明取代关系）。

### R6 指标 schema 与帧定义

- **建议**：写死接受 PresentMon v1 指标（`MsBetweenPresents`，PM 2.6.0 默认）与 v2 指标（`FrameTime`）两种时长列，表头随产物落盘；丢帧定义沿用 >1.5×T（列名按实际表头）；≥1000 帧指过滤后、锚定窗口内的有效行（R4 窗口；Idle 除外，见 R5）。
- **影响**：闸门文字；判分脚本列处理已支持两种列，须确认窗口过滤逻辑新增（LOCK 后另票）。

### R7 对账口径在 dwm 口径下

- **建议**：保留 v1 规则——本进程计数 ≤ 系统计数、差 >2× 记口径冲突、Q3 两边 ≤2 视为同量级、`reconcileNote` 非空要求保留。冲突判读须附 R3 进程清单方可解（无清单的冲突 = INVALID，不可解）。PresentStats 仍仅会话汇总对账，不作主判据。
- **影响**：闸门文字；判分脚本对账分支须读 R3 清单（LOCK 后另票）。

### R8 承接未决项

- Q6（WPA 阶段导出口径）**仍未定**，为采数前提；r2 不代定，LOCK 前须由懂 WPA 的人定义，否则首轮只能走 EventPipe 替代路径并注明。
- Q7（基线轮先行）沿用：首轮只建基线、不判分；Q2（1.1·T）、Q5（最近秩）、Q8（SMBIOS UUID）原样并入 r2。
- G2-LOCK-12 采集方式（logman ETL 优先、EventPipe 降级条件）与 G2-LOCK-10 机器矩阵原样沿用；最低产物清单不变（CSV + ETL/nettrace + PresentStats 汇总）。

## G3 禁令
不改 v1 LOCKED 闸门与哈希；不改 composition.h；不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；本票 ≠ 授权采数 ≠ PERF-CI PASS；PERF-CI HOLD 不解（等第二台物理机 + r2 LOCK + APPROVED 后方可派 workbuddy）。

## 实施清单（LOCK 后另票，均不在本票内）
1. 判分脚本：`presentmonPid` / `presentmonFilter` 字段、锚定窗口过滤、R3 清单读取、两种时长列确认；合成数据单测补齐（dwm 口径、Idle 新定义、窗口锚定）。
2. 采集 harness：dwm PID 记录、过滤参数记录、四项墙钟锚定、安静桌面核验清单采集。
3. Windows 提示词：按 R1–R4 更新采集与核验步骤；B 轮重跑须提权 + 全新 OutDir。
4. 本 DRAFT 按裁决修订后 LOCK（填 LOCKED_AT、算 GATE_BODY_SHA256、APPROVED_*），v1 闸门保持 LOCKED 不动。

## G4 ONELINE 模板
M2-PERF-CI-R2 | gate=<hash> @ <LOCKED_AT> | approved=<who>@<when> | mode=offline-decision | R1–R8=<裁决摘要> | ticket=<PASS/FAIL> | ≠PERF-CI PASS；不授权采数；不改composition.h；不改v1闸门
