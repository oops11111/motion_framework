# M3-RIVE-FOLLOW 门槛闸门（m3_rive_follow_gate_DRAFT.md）

STATUS=DRAFT（**NOT LOCKED**；本文件**不授权采数**）
LOCKED_AT: （LOCK 时填）
GATE_BODY_SHA256: （LOCK 时算；约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希）

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> 起草：Claude，2026-10-30。依据：`docs/m3_plan_DRAFT.md` WP2 与 §7 第 3 张票（“跟随部件拆分，与 Rive 内部指针变形的延迟对比”）；`disputes.md` §3（Rive 跟随指针的延迟：无法普遍消除，首选把跟随部件拆成合成器表达式驱动的 visual，其次渲染线程最后时刻读取指针）；M0 实测（M0-B r3，75Hz：指针属性集表达式 38.7–40.6 ms，UI 线程写入 52.6–55.5 ms）。
> 前置：WP1 的 DPI（RIVE-DPI-PASS）和设备丢失恢复（RIVE-LOST-RECOVER-PASS）已成立。按 XAML-LOST N1 裁决，本票真机排在 M3-XAML-LOST 之后。
> 分支：`claude/m3-follow`，从 `claude/m3-xaml-lost` 拉出。

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
- **拖动错位**（拆分后的接缝）：光标匀速拖动时，C（拆出的部件）与 A（仍在 Rive 表面里的内容，用第二种颜色的块表示）之间的位置差，换算成毫秒，P50 / P95。这是用户能看到的“部件和身体脱节”。
- **预算**：每条路径运行时，渲染线程 CPU / GPU P95 仍 ≤ 0.35·T（plan.md）。
- **判读规则（记录型，写入 WP2 设计约束）**：
  - C 的 P50 比 A 少 ≥ 1 帧（T）→ 把“拆出合成器 visual”定为跟随部件的**默认做法**；
  - B 的 P50 比 A 少 ≥ 0.5 帧 → 把“渲染线程最后时刻读取”定为**表面内跟随的默认做法**；
  - 拖动错位 P50 ≤ 1 帧 → 拆分的接缝可接受；否则在设计约束里写明“拆出的部件需要加轻微弹簧阻尼来掩盖”（disputes §3）。
- **INVALID**：缺产物；hash 不符；未授权采数；改 composition.h；某条路径检出步数不足一半。

## G3 待裁决（P1–P6，附建议）
| # | 问题 | 建议 | 理由 |
|---|---|---|---|
| **P1** | 用什么代表“Rive 内部的指针响应”（A、B） | **在 Rive 表面里、Rive 画完之后由渲染线程画一个跟随块**（沿用 M0-2 的标记块机制），同时每帧把指针位置交给 Rive 状态机（`pointerMove`），让 Rive 的处理开销也算在内。等试点有设计师做的、真正带指针监听的 .riv 时再补测一次 | 钉住版本的 Rive 测试素材里没有“跟随光标”的 .riv。延迟由“指针 → 渲染线程 → 表面 → 合成器”这条管线决定，跟随块和 Rive 内部变形走的是同一条管线；Rive 状态机处理指针只多零点几毫秒 CPU |
| **P2** | 测哪些路径 | **A、B、C、D 四条全测** | A 是现状，B、C 是 disputes §3 的两种缓解，D 让结果能和 M0 直接对照，防止测量方法本身漂移 |
| **P3** | 样本量和会话数 | **3 个会话**；每会话每条路径 40 次阶跃（正反各 20）和 40 次拖动（与 M0-B r3 相同，保证每条路径 ≥ 200 个拖动样本） | M0-A 每路径 20 步时出现过反向步全部漏检；M0-B r2 样本 161 不够，r3 加到 40 次拖动后稳定 |
| **P4** | 是否允许 `SendInput` 合成指针 | **允许，仅限本票**；只移动光标、不点击；运行期间操作员不碰鼠标键盘 | M0 D1 的授权只覆盖 M0；不注入就测不了真实指针延迟 |
| **P5** | 用哪台机器、什么刷新率 | **DESKTOP-IJI1KJG，75Hz**（Win11，M0-B 的那台，有现成构建和 WGC 追踪）；60Hz / 144Hz 和混合刷新率留给 M3-MATRIX | 与 M0-B r3 同机同刷新率，结果可直接对比 |
| **P6** | 判读规则（G2 的三条阈值）是否合适 | **按 G2**：C 比 A 少 ≥ 1 帧才定为默认；B 比 A 少 ≥ 0.5 帧才定为默认；接缝 ≤ 1 帧可接受 | 少于 1 帧的差别，用户很难感知，不值得为它增加拆分部件的工程成本；B 不改结构，门槛可以更低 |

## G4 实现计划（P 裁决后写，LOCK 前完成）
| 文件 | 改动 |
|---|---|
| `m0/native/m0rive/m0rive.{h,cpp}` | 新增 `m0r_set_follow(mode, …)`：mode 0 关闭；1 = 由 UI 线程给位置（A）；2 = 渲染线程每帧开头 `GetCursorPos` + `ScreenToClient` 自己读（B）。跟随块画在 Rive 之后；每帧把位置交给 `scene->pointerMove`（P1）。第二种颜色的“身体块”留在固定相对位置，用于拖动错位 |
| `MotionSoftDemo/M3FollowProbe.cs`（新，`--m3-follow-probe`） | 复用 M0Probe 的阶跃、拖动和 WGC 追踪；四条路径依次运行；输出每条路径的延迟、检出步数、拖动错位、CPU/GPU 预算 |
| `MainPage.xaml.cs` 入口 + `m3/mainpage_m3follow_hook.patch`；`m3/run_m3_follow.ps1` | 入口和 runner（不需要管理员，不重启驱动） |

## G5 禁令
不改 composition.h；不重开 S1/R-3/R-2；禁影子 IV；`SendInput` 只用于本票且只移动光标；本 DRAFT ≠ 授权采数；成功 ≠ 采用；PERF-CI 仍 HOLD。
