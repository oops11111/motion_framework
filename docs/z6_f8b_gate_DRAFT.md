# Z6 F8b · K93 约 32Hz 回调计时探针闸门（z6_f8b_gate_DRAFT.md）

STATUS=DRAFT（**NOT LOCKED**；不授权采数；不授权编码；不授权写、编译或运行任何探针 / 计时代码；不授权试跑工具）
LOCKED_AT: （LOCK 时由协调填写）
GATE_BODY_SHA256: （LOCK 时由协调计算；约定同其他闸门：对落盘全文做 SHA-256，排除以 GATE_BODY_SHA256 开头的那一行）

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> 起草：workbuddy cli，2026-11-18 AWST，Z6 票 F8b，分支 `wb/z6-f8b`。**未复核**。
> 依据：`z6_workbuddy_handover_FINAL_r2.md`（sha256 `82ab4e29db68e943ab04b034d9e3e360c4a84f2a05451ae904772e10761dd2b1`）第三节 C 类 F8b（「同 F8a」）；`docs/k93_frame_rate_diag.md`（Z5 T6，只读诊断，假设 H1–H8）；`docs/z5_closed.md` 第 5 节后续票 F8。
> **绑定**：main `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`，或之后 pidan 认可的 main（LOCK 时写明最终绑定的 sha）。
> **LOCKED 文件只由协调在 pidan 本人说 LOCK 之后落盘**；起草方不新建、不改名、不复制任何 `*_LOCKED.md`。采数还须 pidan 本人说「APPROVED F8b」。
> 本 DRAFT 中所有标「**待定**」的数值、口径和机台，都没有来源，必须由评审提出、pidan 拍板后才能写入；起草方没有填任何数。

## G0 硬边界
- Success ≠ adopt；不合 main；不改 `code/composition.h`（PRE/POST 每次记录）；无 LOCK + APPROVED 不采数。
- **PERF-CI 继续 HOLD**：F8a、F8b 的 APPROVED 都不解除 PERF-CI HOLD，也不改任何已锁闸门（含 `docs/m2_perf_ci_gate_LOCKED.md`）和已锁阈值。
- **T6、Z4 的旧日志只能当背景引用，不能作阈值来源或通过依据**（定稿原文）。
- 不改 E1（事件名、id、字段、provider、GUID）、`code/capi/fm_capi.h/.cpp`、`MotionSoftDemo/MotionEtw.cs`、L1 公开面、任何 `*_LOCKED.md`、`docs/api_v0_frozen.md`；若探针须改其中任一项，按「碰冻结面」停下请示。
- 探针是诊断用的 dev 代码，不进 L1 pack；若探针改了 pack 源，F7、F11 须重做。
- 系统级步骤（改显示 / DPI、电源、辅助功能动画、HwSchMode 等）不在本闸门授权内，须逐次请示 pidan；测后恢复原值并记录。
- 禁影子 IV；不重开 S1 / R-3 / R-2；不解冻 Bv。

## G1 要验证什么
1. 在 K93N3P8 上，宿主帧回调（`CompositionTarget.Rendering` / OnFrame）的实际间隔是什么样的分布。
2. 用对照设计区分 `docs/k93_frame_rate_diag.md` 第 3 节列出的假设 H1–H8（哪些成立、哪些排除）；T6 只列假设、不下结论，本票才下结论。
3. 每个会话都把 T6 标为「采数时未记录」的环境事实一并记录（显示模式与刷新率、适配器、会话类型、前台 / 遮挡、电源计划、构建配置与 trace 开关、HwSchMode、AnimationsEnabled）。

## G2 判据（全部**待定**，LOCK 前由评审提出、pidan 拍板）
| 判据 | 门槛 | 来源 |
|---|---|---|
| ① 测量量 | 回调间隔的取法（时钟源、统计量、样本数、预热与丢弃规则）**待定** | 待定 |
| ② 对照矩阵 | 需要哪些对照（例如构建配置、trace 开关、前台状态等）以区分 H1–H8：**待定** | `docs/k93_frame_rate_diag.md` 第 3 节（只取假设清单，不取数值） |
| ③ 环境记录 | 每会话记录 G1-3 列出的全部事实，缺一项即该会话 INVALID | `docs/k93_frame_rate_diag.md` 第 1 节与附录的采集项 |
| ④ 结论规则 | 每个假设 H1–H8 的「成立 / 排除 / 无法判定」判定规则**待定** | 待定 |
| ⑤ 冻结面不变 | composition.h PRE=POST；E1 / 公开面不变（若改了 pack 源，按 T3 标准跑 CheckSurface 与 devsmoke） | 定稿第二节、第四节 |

- **会话**：机台（定稿只点名 K93；是否加对照机**待定**）、会话数、每会话时长**待定**。
- **PASS / FAIL / 无法判定**的定义**待定**（本票是诊断票，评审可决定是否只出结论表而不设 PASS）。计数会话失败不重跑凑数；首跑失败原始记录保留在 `attempt1/`。
- **INVALID**（任一即 INVALID）：
  1. 未 LOCK 或未 APPROVED 就采数，或在 LOCK 前写、编译、运行探针 / 计时代码，或试跑工具；
  2. 改动 `composition.h`（PRE ≠ POST）；
  3. 把 T6 / Z4 旧日志的数值当作阈值来源或通过依据；
  4. 采数所用代码 tip 未在回报中声明，或与 LOCK 记录的绑定不符；
  5. 把本闸门结论当作 PERF-CI 通过，或据此修改任何已锁闸门 / 阈值；
  6. 未经 pidan 逐次许可改动系统设置，或改动后未恢复、未记录；
  7. 缺原始产物，或闸门 hash 不符。

## G3 待裁决（由评审补全；起草方只列问题，不给建议数值）
| # | 问题 | 起草方说明 |
|---|---|---|
| Q1 | 探针放在哪个宿主（MotionSoftDemo / FluentHub 侧叉 / 独立最小 WinUI 宿主） | 待定；FluentHub 构建涉及非原样开关 `-p:NuGetAudit=false`，须 pidan 另行拍板 |
| Q2 | 计时时钟源与统计口径 | 待定 |
| Q3 | 对照矩阵（G2-②）与每格会话数 | 待定 |
| Q4 | 是否需要改系统设置做对照（显示模式、电源、HwSchMode、动画效果），以及恢复流程 | 均为系统级步骤，须逐次请示 |
| Q5 | 是否加 Win11 对照机（例如 Z4 r1 用过的 IJI1KJG） | 定稿只写 K93；待定 |
| Q6 | 与 F8a 的关系（是否共用会话 / 宿主） | 待定；两闸门各自 LOCK、各自 APPROVED |

## G4 前置与顺序
1. 本 DRAFT 经评审补全待定项，pidan 本人说 **LOCK** → 协调落盘 `docs/z6_f8b_gate_*_LOCKED.md`（文件名由协调定）。
2. pidan 本人说「**APPROVED F8b**」后，才可编码与采数。
3. 背景（只读、只当背景）：`docs/k93_frame_rate_diag.md`；`z4/results_etw_l1_smoke_r1/K93N3P8/` 下的已有会话记录；`docs/api_v0_frozen.md` 已知限制中的 K93 帧间隔引用。**这些都不得作阈值来源或通过依据**。

## G5 禁令
同 G0。本 DRAFT ≠ LOCK ≠ APPROVED ≠ 授权采数 ≠ 授权编码。诊断结论 ≠ PERF-CI ≠ 采用。
