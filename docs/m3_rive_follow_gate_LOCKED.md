# M3-RIVE-FOLLOW 门槛闸门（m3_rive_follow_gate_LOCKED.md）

STATUS=LOCKED
LOCKED_AT: 2026-10-30 17:40:39 AWST（Australia/Perth）
GATE_BODY_SHA256: 285e39dc9c1100f92ddddc5560a3631b94129a9b56c561c9803c8cbee57eaab3
> GATE_BODY_SHA256 计算约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。本行写入后不得再改 body。

**本 LOCK 不授权采数。** 须另有 approved 留证（APPROVED_* 非空）后才可采数；构建可先行。

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> 起草：Claude，2026-10-30。DRAFT：`docs/m3_rive_follow_gate_DRAFT.md`。依据：`docs/m3_plan_DRAFT.md` WP2 与 §7 第 3 张票（“跟随部件拆分，与 Rive 内部指针变形的延迟对比”）；`disputes.md` §3（Rive 跟随指针的延迟：无法普遍消除，首选把跟随部件拆成合成器表达式驱动的 visual，其次渲染线程最后时刻读取指针）；M0 实测（M0-B r3，75Hz：指针属性集表达式 38.7–40.6 ms，UI 线程写入 52.6–55.5 ms）。
> 前置：WP1 的 DPI（RIVE-DPI-PASS）和设备丢失恢复（RIVE-LOST-RECOVER-PASS）已成立。按 XAML-LOST N1 裁决，本票真机排在 M3-XAML-LOST 之后。
> 分支：`claude/m3-follow`，从 `claude/m3-xaml-lost` 拉出，并合入了 `claude/m3-xaml-rebuild`（两张票改的是 MainPage 的同一几行，合并后补丁按顺序可打）。结果目录 `m3/results_follow/`。

## G0 硬边界
不改 `composition.h`；不重开 S1/R-3/R-2；禁影子 IV；不解冻 Bv；PERF-CI 仍 HOLD；未 LOCK 且 APPROVED_* 为空时禁止采数；成功 ≠ 采用 ≠ 默认可合入 main；本机 csproj 补丁不进仓库。

## G1 要回答的问题
一个 Rive 动效里“跟着指针走”的部分（例如眼睛、高光、光标附近的装饰），有几种做法，各自从指针移动到屏幕上反映出来要多久，拆出来以后和 Rive 其余部分会不会出现可见的错位：
- **A 表面内、UI 线程转交**（= Rive 内部响应指针的现状）：UI 线程收到 `PointerMoved` → 交给渲染线程 → 下一帧在 Rive 表面里画到新位置 → 合成器。
- **B 表面内、渲染线程最后时刻读取**（disputes §3 的第二种缓解）：渲染线程在每帧画之前自己读光标位置（`GetCursorPos`），不经过 UI 线程。
- **C 拆出的 visual、合成器表达式驱动**（disputes §3 的首选）：跟随部件是单独的 `SpriteVisual`，位置由指针属性集的 `ExpressionAnimation` 直接驱动。
- **D 拆出的 visual、UI 线程写入**（M0 的基线，用于和 M0 数据衔接）。

## G2 测量与判读（数字为建议值，见 P3、P6）
- **延迟**（沿用 M0-5 的方法）：`SendInput` 让光标做阶跃移动，WGC 采集应用窗口，追踪跟随块（#3B82F6）的中心，记录从注入到“第一帧越过半程”的时间。每条路径 P50 / P95，正反两个方向都要检出。
- **拖动错位**（拆分后的接缝）：沿用 M0-2 的做法——WGC 每次只追踪一种颜色，所以对 A 和 C 分别做 40 次匀速拖动，算出各自相对光标轨迹的滞后，接缝 = C 的滞后 P50 − A 的滞后 P50（毫秒，再换算成帧）。这就是拆出的部件和留在 Rive 里的内容同时跟随时，用户看到的“部件和身体脱节”。
- **预算**：每条路径运行时，渲染线程 CPU / GPU P95 仍 ≤ 0.35·T（plan.md）。
- **判读规则（记录型，写入 WP2 设计约束）**：
  - C 的 P50 比 A 少 ≥ 1 帧（T）→ 把“拆出合成器 visual”定为跟随部件的**默认做法**；
  - B 的 P50 比 A 少 ≥ 0.5 帧 → 把“渲染线程最后时刻读取”定为**表面内跟随的默认做法**；
  - 拖动错位 P50 ≤ 1 帧 → 拆分的接缝可接受；否则在设计约束里写明“拆出的部件需要加轻微弹簧阻尼来掩盖”（disputes §3）。
- **INVALID**：缺产物；hash 不符；未授权采数；改 composition.h；某条路径检出步数不足一半。

## G3 裁决（pidan：P1–P6 全部按建议）
| # | 问题 | 裁决（= 原建议） | 理由 |
|---|---|---|---|
| **P1** | 用什么代表“Rive 内部的指针响应”（A、B） | **在 Rive 表面里、Rive 画完之后由渲染线程画一个跟随块**（沿用 M0-2 的标记块机制），同时每帧把指针位置交给 Rive 状态机（`pointerMove`），让 Rive 的处理开销也算在内。等试点有设计师做的、真正带指针监听的 .riv 时再补测一次 | 钉住版本的 Rive 测试素材里没有“跟随光标”的 .riv。延迟由“指针 → 渲染线程 → 表面 → 合成器”这条管线决定，跟随块和 Rive 内部变形走的是同一条管线；Rive 状态机处理指针只多零点几毫秒 CPU |
| **P2** | 测哪些路径 | **A、B、C、D 四条全测** | A 是现状，B、C 是 disputes §3 的两种缓解，D 让结果能和 M0 直接对照，防止测量方法本身漂移 |
| **P3** | 样本量和会话数 | **3 个会话**；每会话每条路径 40 次阶跃（正反各 20）和 40 次拖动（与 M0-B r3 相同，保证每条路径 ≥ 200 个拖动样本） | M0-A 每路径 20 步时出现过反向步全部漏检；M0-B r2 样本 161 不够，r3 加到 40 次拖动后稳定 |
| **P4** | 是否允许 `SendInput` 合成指针 | **允许，仅限本票**；只移动光标、不点击；运行期间操作员不碰鼠标键盘 | M0 D1 的授权只覆盖 M0；不注入就测不了真实指针延迟 |
| **P5** | 用哪台机器、什么刷新率 | **DESKTOP-IJI1KJG，75Hz**（Win11，M0-B 的那台，有现成构建和 WGC 追踪）；60Hz / 144Hz 和混合刷新率留给 M3-MATRIX | 与 M0-B r3 同机同刷新率，结果可直接对比 |
| **P6** | 判读规则（G2 的三条阈值）是否合适 | **按 G2**：C 比 A 少 ≥ 1 帧才定为默认；B 比 A 少 ≥ 0.5 帧才定为默认；接缝 ≤ 1 帧可接受 | 少于 1 帧的差别，用户很难感知，不值得为它增加拆分部件的工程成本；B 不改结构，门槛可以更低 |

## G4 实现（已写，未在真机运行；代码 = 分支 `claude/m3-follow` 提交 `b9e6f32`）
| 文件 | 改动 | 本地验证 |
|---|---|---|
| `m0/native/m0rive/m0rive.{h,cpp}` | 新增 `m0r_set_follow_cursor(hwnd, 原点, 宽, 高, 颜色)`（路径 B）：渲染线程每帧开头 `GetCursorPos` → `ScreenToClient`，把跟随块画在光标处。只要跟随块在显示（B，或 A 用的 `m0r_set_marker`），每帧把块中心作为 `pointerMove` 交给 Rive 状态机（P1）。注意：M0-2 的表面拖动也用 `m0r_set_marker`，此后同样会多这一步 Rive 指针处理；M0 已关闭，不重跑 | MinGW 对照钉住的 Rive 头文件：本文件 0 警告 0 错误 |
| `MotionSoftDemo/M3FollowProbe.cs`（新，`--m3-follow-probe`） | 与 M0 相同的 Rive 表面（400 px，盖在 AnimBox 上）和几何（光标停在 AnimBox 中心，阶跃和拖动沿 +x，幅度 120 DIP）。A：UI 线程 `PointerMoved` → `m0r_set_marker`；B：`m0r_set_follow_cursor`；C：AnimBox 的 visual 由指针属性集表达式驱动（= M0 expr）；D：UI 线程写 Translation（= M0 ui）。A、B 时 AnimBox 透明、只有表面里的块是追踪色；C、D 时隐藏 Rive 表面。每条路径 40 次阶跃（正反各 20）+ 40 次拖动，并记录该路径期间的渲染线程 CPU / GPU P95。summary 写出 G2 的三条规则结果（`rule.split_default`、`rule.latch_default`、`rule.seam_ok`）；任一路径正向或反向检出不足一半 → `INVALID_FEW_STEPS` | 对照 WinAppSDK 2.5.1 编译 0 个 C# 错误 |
| `MotionSoftDemo/M0Probe.cs` | `M0RunSteps` / `M0RunDrags` 增加可选参数（步数、键名前缀），默认值保持 M0 行为不变；`M0RunSteps` 另报正向 / 反向各自的检出数，并返回每步延迟 | 同上 |
| `MotionSoftDemo/MainPage.xaml.cs` +12 行；`m3/mainpage_m3follow_hook.patch` | 入口。补丁基于**已打 DPI、LOST、XAML-LOST、XAML-REBUILD 补丁**的 MainPage | 补丁由 `git diff` 生成 |
| `m3/run_m3_follow.ps1`（新） | 按会话启动、等待 summary、输出会话矩阵（每条路径的延迟 P50、检出数，三条规则，预算）；不需要管理员，不重启驱动 | 本环境没有 PowerShell，人工复查 |

## G5 禁令
不改 composition.h；不重开 S1/R-3/R-2；禁影子 IV；`SendInput` 只用于本票且只移动光标；本 DRAFT ≠ 授权采数；成功 ≠ 采用；PERF-CI 仍 HOLD。
