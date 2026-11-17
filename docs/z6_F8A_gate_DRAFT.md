# Z6 F8a · 真实 PresentStats 闸门（z6_f8a_gate_DRAFT.md）

STATUS=DRAFT（**NOT LOCKED**；不授权采数；不授权编码；不授权写、编译或运行任何探针 / 计时代码；不授权试跑工具）
LOCKED_AT: （LOCK 时由协调填写）
GATE_BODY_SHA256: （LOCK 时由协调计算；约定同其他闸门：对落盘全文做 SHA-256，排除以 GATE_BODY_SHA256 开头的那一行）

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> 起草：workbuddy cli，2026-11-18 AWST，Z6 票 F8a，分支 `wb/z6-f8a`。**未复核**。
> 依据：`z6_workbuddy_handover_FINAL_r2.md`（sha256 `82ab4e29db68e943ab04b034d9e3e360c4a84f2a05451ae904772e10761dd2b1`）第三节 C 类 F8a；`docs/api_v0_frozen.md`「已知限制」PresentStats 一条；`docs/api_changelog.md` 2026-11-18 已知限制登记行；`docs/z5_closed.md` 第 5 节后续票 F8；`MotionSoftDemo/MotionEtw.cs` 事件 id 4 的注释与定义。
> **绑定**：main `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`，或之后 pidan 认可的 main（LOCK 时写明最终绑定的 sha）。
> **LOCKED 文件只由协调在 pidan 本人说 LOCK 之后落盘**；起草方不新建、不改名、不复制任何 `*_LOCKED.md`。采数还须 pidan 本人说「APPROVED F8a」。
> 本 DRAFT 中所有标「**待定**」的数值、口径和机台，都没有来源，必须由评审提出、pidan 拍板后才能写入；起草方没有填任何数。

## G0 硬边界
- Success ≠ adopt；不合 main；不改 `code/composition.h`（PRE/POST 每次记录）；无 LOCK + APPROVED 不采数。
- **PERF-CI 继续 HOLD**：本闸门的 APPROVED 不解除 PERF-CI HOLD，也不改任何已锁闸门（含 `docs/m2_perf_ci_gate_LOCKED.md`）和已锁阈值。
- **E1 不变**：事件名 `PresentStats`、id `4`、字段（`sessionId` / `dropped` / `intervalMs`，顺序与类型不变）、provider `FluidMotion-Motion`、GUID `8f3a2c1d-6b4e-4a91-9c7d-2e5f8a1b3c4d` 都不改。实现只把事件 id 4 的**值**由占位改为实测。
- 不改 `code/capi/fm_capi.h/.cpp` 的 C ABI、L1 公开面（`MotionSystem` / `AnimationHandle` / `MotionTokens` 公开成员）、任何 `*_LOCKED.md`、`docs/api_v0_frozen.md`（若实现须改其中任一项，按「碰冻结面」停下请示）。
- `MotionSoftDemo/MotionEtw.cs` 不改（定稿第二节第 2 条）。若真实值只能在 MotionEtw.cs 内部产生，按「碰冻结面」停下请示。
- 禁影子 IV；不重开 S1 / R-3 / R-2；不解冻 Bv。

## G1 要验证什么
1. PresentStats 的 `dropped` 与 `intervalMs` 来自真实的呈现统计，而不是现有发射点写死的占位值（`dropped=0`、`intervalMs=16.666`，见 `docs/api_v0_frozen.md` 已知限制）。
2. 换成实测值后，E1 的名字、id、字段、GUID 与公开面都没有变化。
3. 实测值的口径（每个字段是什么、在哪一层取、多久发一次）明确、可复核。

## G2 判据（全部**待定**，LOCK 前由评审提出、pidan 拍板）
| 判据 | 门槛 | 来源 |
|---|---|---|
| ① 字段语义 | `dropped`：**待定**（统计窗口、计数口径）；`intervalMs`：**待定**（取哪一种间隔、取何种统计量） | 待定 |
| ② 测量来源 | **待定**（取值所在层与 API；须不改 fm_capi.h/.cpp 与 MotionEtw.cs，否则停下） | 待定 |
| ③ 发射频率与时机 | **待定** | 待定 |
| ④ 与占位值区分 | 实测值须能与现有写死占位（`dropped=0`、`intervalMs=16.666`）区分；判定方法**待定** | `docs/api_v0_frozen.md` 已知限制 |
| ⑤ E1 / 公开面不变 | Release 公开面 dump 与 `pilot/fluenthub/surface_l1pack.txt` 相比只允许首行不同；CheckSurface `fails=0`（Debug、Release 各一次，原始输出入库） | 定稿第三节 F7 / 第四节 T3 标准 |
| ⑥ 数值合理性 | **待定**（不得以 Z4 / T6 旧日志作阈值来源，见 G4） | 待定 |

- **会话**：机台、会话数、每会话时长与采样数**待定**。
- **PASS** ⇔ 上表全部判据在每个会话都满足（判据数值待 LOCK）。**FAIL**：已进入可裁决采数且任一条不满足。计数会话失败不重跑凑数；首跑失败原始记录保留在 `attempt1/`。
- **INVALID**（任一即 INVALID）：
  1. 未 LOCK 或未 APPROVED 就采数，或在 LOCK 前写、编译、运行探针 / 计时代码，或试跑工具；
  2. 改动 `composition.h`（PRE ≠ POST）；
  3. 改动 E1（事件名、id、字段、provider、GUID）或 L1 公开面；
  4. 采数所用代码 tip 未在回报中声明，或与 LOCK 记录的绑定不符；
  5. 把本闸门结论当作 PERF-CI 通过，或据此修改任何已锁闸门 / 阈值；
  6. 缺原始产物，或闸门 hash 不符。

## G3 待裁决（E1–E? 由评审补全；起草方只列问题，不给建议数值）
| # | 问题 | 起草方说明 |
|---|---|---|
| Q1 | `dropped`、`intervalMs` 的精确语义 | 现有字段名和类型冻结（uint / double），只能定义语义，不能改字段 |
| Q2 | 真实值从哪里取（宿主侧、L1 内部、C ABI 之下哪一层），以及是否需要碰冻结面 | 若需要改 fm_capi.h/.cpp 或 MotionEtw.cs，本票须先停下请示，不在 DRAFT 内预设 |
| Q3 | 采数机台与会话数 | 定稿未指定；待定 |
| Q4 | 宿主（MotionSoftDemo / FluentHub 侧叉 / 其他）与构建配置 | 待定；若为 PublishAot 宿主，须先按 `docs/z6_aot_host_guide.md`（`wb/z6-f4`）确认 EventSource 可用并记为偏差 |
| Q5 | 现有写死占位值的发射点（`Z4L1Session.cs`、`Z3aL1DevSmoke.cs`、`M202Probe.cs`）是否一并改、怎么改 | 待定 |
| Q6 | 采集方式（EventPipe / EventListener / ETL） | 待定 |

## G4 前置与顺序
1. 本 DRAFT 经评审补全 G2 / G3 的待定项，pidan 本人说 **LOCK** → 协调落盘 `docs/z6_f8a_gate_*_LOCKED.md`（文件名由协调定）。
2. pidan 本人说「**APPROVED F8a**」后，才可编码与采数。
3. 实现完成后：在 `docs/api_changelog.md` 登记（内部改动，E1 不变）；**F7 须重做**（以及 F11）；若改了 pack 源，按 T3 标准跑 CheckSurface（Debug、Release）与 devsmoke（Debug、Release）。
4. Z4 r1 与 Z5 T6 的旧记录（例如 `docs/api_v0_frozen.md` 引用的 K93 `frame_dt_ms.p50`、`docs/k93_frame_rate_diag.md`）**只能当背景**，不得作为本闸门的阈值来源或通过依据。

## G5 禁令
同 G0。本 DRAFT ≠ LOCK ≠ APPROVED ≠ 授权采数 ≠ 授权编码。PresentStats 实测 PASS ≠ PERF-CI ≠ 采用。在 LOCK + APPROVED 之前，任何报告都不得读取 PresentStats 的字段值。
