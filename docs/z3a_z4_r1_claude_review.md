# Z3a C-ABI L1 实现 + Z4 r1 结果复核（Claude）

> 依据：分支 `claude/z3a-cabi-l1`。代码/采数 tip `fe59c622`，结果 `0aa4cd66`，结题 `7105c88`（`docs/z4_etw_l1_smoke_r1_closed.md`，CLOSED `Z4-ETW-L1-SMOKE-PASS`）。闸门 `docs/z4_etw_l1_smoke_gate_r1_LOCKED.md`，留证 `docs/z4_etw_l1_smoke_r1_approved.md`，票 `docs/z3a_cabi_l1_ticket.md`，合同 `docs/api_surface_m2_alpha_contract_LOCKED.md`。
> 实现由 Cursor Agent 提交（票内写的是 Claude 编码）；本文是 Claude 对代码和结果的独立复核。**复核 ≠ 采用。** Success≠adopt；不合 main；PERF-CI HOLD。

## 1. 结论
**同意 Z4 r1 CLOSED PASS**：有效会话 6 个（K93 s2/s4/s5，IJI s1/s2/s3），G2 ①–⑥ 全部通过，没有 INVALID 情形。
**附一项补证要求**：Z3a 票的 K93 验收记录（`fm_capi_test` PASS、开发冒烟日志）没有落盘，见 §3 第 1 条。补齐之前，「采数前 Z3a 已通过验收」只能由间接证据推出。

## 2. 我核对过的项
| 项 | 结果 |
|---|---|
| 闸门 hash | `acb89d1b…ab14`，在 `fe59c622` 上复算 MATCH |
| 合同 hash | `77416e8b…3a0f`，复算 MATCH |
| composition.h | 在 `fe59c622` 上 LF/CRLF 归一化后为 `a55d4952…e418`；`code/` 相对 `claude/m3-matrix` 无任何改动 |
| C-ABI | `fm_capi.h` / `fm_capi.cpp` 未改，仍是 14 个导出，未新增 `fm_*` |
| 时序 | APPROVED 2026-11-10 23:16 AWST → 实现提交 23:30 起 → tip `fe59c622` 11-11 06:28 → 正式会话 06:38 起。批准在先，采数在后 |
| 采数 tip | 各 PASS `env.json` 的 `tip` 都是 `fe59c622…9069`，与声明一致 |
| E1 | provider 名、GUID、事件 id 1–6 与 103 未改（`MotionEtw.cs` 只改了注释）；运行时 `EventSource.Guid` = 冻结 GUID |
| E2 / E3 / E4 | 只用了两台点名机器；每台 3 个有效会话，打包、非打包都有；EventPipe；宿主只有 MotionSoftDemo |
| 公开面 | `MotionSystem.Create / Dispose / LoadTokens / SetReducedMotion / IsRunning / OnFrame / Current / CreateAnimation`，`AnimationHandle.Play / Cancel / Release / Retarget(property, target, spring, from?)`，`MotionTokens.Load`，与合同表 1 逐行对上。`MotionEngine` 已删除；`CreateManual` / `Advance` 是 internal；没有 `RiveView` / `PointerFollow`，也没有 0.3·T |
| 表 2（线程） | 跨线程调用会抛异常并计数；在 sink / render 回调里重入会抛异常；在回调里 Dispose 会延后执行。符合 |
| **独立复跑**（Linux，仅开发用，**不是 Z4**） | 在 `fe59c622` 上：`fm_capi_test` 用 ASan + UBSan 编译，**23 checks, 0 failures**；`z3a_l1_devsmoke`（net8.0，接 Linux 编的 `libfluidmotion.so`）**verdict=PASS fails=0**，六个 E1 事件名都打出来了 |

## 3. 发现（都不推翻 PASS）
1. **Z3a 验收没有留证（需要补）。** 留证要求「K93 构建通过票内验收（能编过、API 对表、`fm_capi_test` PASS、开发冒烟打出 E1、composition.h PRE=POST）」之后才能采数。分支上没有对应的回报或日志：结果目录和 `BUILD_NOTES.md` 都没有提到 `fm_capi_test` 或 devsmoke。闸门 INVALID 第 1 条只看「L1 构建是否存在」，构建确实存在，所以**不构成 INVALID**。**建议**：workbuddy 补交 K93 上 `fe59c622`（或其祖先 `f2ab250`/`58ae6a6`）的 `fm_capi_test` 输出和 devsmoke 输出，放到 `z3a/results_dev/`，并标 dev-only。我在 Linux 的复跑可以作为旁证，不能替代 K93 的记录。
2. **`PresentStats` 是写死的占位值。** `Z4L1Session.cs:440` 和 `Z3aL1DevSmoke.cs:175` 发出的都是 `dropped=0, intervalMs=16.666`，不是测量值；而 K93 实测的帧间隔 p50 约 31 ms。G2 ⑥ 只要求事件名出现，所以本票不受影响。但**试点遥测不能读这个字段的值**，否则会把 K93 上约 30 Hz 的节奏看成 60 Hz。建议在 `MotionEtw.cs` 注释里写明，并作为已知限制记到 `docs/api_v0_frozen.md`。要发真实的 present 统计，需要另开票。
3. **G2 ② 的判定口径是在试跑之后改的。** pidan 06:24 的裁决把「单帧步长」从 `Task.Delay(16)` 轮询累计改成逐个 `OnFrame` 回调之间的步长，`probe_max_step_dip` 改为只作诊断（K93 s4 为 100.5）。我认同：闸门原文就是「单帧步长 < 80 DIP」，轮询累计跨了 1–2 帧，本来就不是单帧；而且口径在正式会话（06:38）之前已经定下并写进代码，80 DIP 和 dynTol 都没改。这里只记一笔：下一轮闸门应当在 LOCK 文本里直接写明逐帧口径。
4. **K93 回调频率约 32 Hz**（`frame_dt_ms` p50 31.2，max 78.9）。这不在 Z4 判据里，也不是 PERF-CI 判定，只作为 PERF-CI 解冻时要先查的线索：K93 上的 `CompositionTarget.Rendering` 为什么只有半刷新率。
5. **公开面多出几项**：`ValueChanged`、`FramePumpChanged` 事件，`SpringParams` / `MotionSample` 结构，`AnimationHandle.Id`。它们是表 1「sink 投影 / 宿主绑定帧源 / Retarget 参数 / Current 返回值」的 C# 形态，不是新能力，可以接受；但 `docs/api_changelog.md` 没有登记。建议补一行，说明 L1 的 C# 形态包括这些成员，以免以后被当成「冻结外新增」。
6. **`FM_FLUIDMOTION_PATH`**：静态构造里按环境变量加载任意 DLL，狗粮期可以；进试点 / 产品前要删掉或限制在 Debug 构建，否则是 DLL 劫持入口。
7. 小问题：`DestroyCore` 在帧泵开着时调用 `pump(false)`，但不发 `IdleTransition`，ETW 上看到的最后一个状态会是「开」。不影响本票。

## 4. 采用口径
- Z4 r1：**同意 CLOSED PASS**，前提是补齐 §3-1。不采用 tip、不合 main，有条件采用由 pidan 决定。
- §3-2、3-5、3-6 都是改文档或开小票的事，不需要重新采数。
- 下一步（由 pidan 决定）：Z1 试点编码票（FluentHub 先行）可以引用本结果，但须同时写明 PresentStats 是占位值、PERF-CI HOLD、Success≠adopt。
