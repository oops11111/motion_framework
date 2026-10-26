Both files written. Now deliver the final answer: full text of Part A, separator, full text of Part B, per the 交卷格式. Also note file locations. Keep the reply as the two full documents (they asked for full text). This will be long but that's the required format.
交卷。两份文档已写入 `motion_framework_review/docs/`：`docs/deepseek_m2_01_noise.md` 与 `docs/deepseek_m2_02_api_draft.md`（该目录为本工作区仅有的仓库快照，`docs/m2_01_gate_DRAFT.md` 与 `artifacts/win_harness_noise_probe.txt` 不在此工作区，复核依据为任务书转述条文，已在文内声明）。全程未运行任何测量、未修改任何源文件；快照 `composition.h` 实测 sha256（3411aebd…）与禁令所列（a55d4952…）不一致，仅作提示未作处理。

# M2-01：harness 底噪分解与 G2（DRAFT）复核意见

> 交付：DeepSeek · M2-01 文档票（analysis-only，简体中文）
> 落点：`docs/deepseek_m2_01_noise.md`
> 日期：2026-10-24（本地时区 Australia/Perth）
> 状态：分析文档；**不含新测量、不含采数、不含实现**。

## 1. 元信息与禁令重申

- **本票范围**：harness 底噪分解（目标 <0.5 DIP）＋ 闸门 G2（DRAFT）复核意见。不测试、不采数、不改码。
- **事实来源**：仅用任务书「共享事实」#30（`artifacts/win_harness_noise_probe.txt`，2026-10-20）、#37、notion_brief，以及本仓库既有文档（`plan.md` / `api.md` / `workbuddy_soft_demo.md`）。所有数字注明出处；由给出数字直接推算的量显式标「推算」。
- **硬边界（本票执行时严格遵守，重申）**：
  1. 不改 `composition.h`；被冻结修订 sha256 = `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`。
     - 提示：本工作区快照中 `composition.h` 的实测 sha256 为 `3411aebdb10b7956c731ab0541796d9d671fae97e9835103c770cbc858c899c3`，与禁令所列**不一致**——本快照不是受保护修订。G2 的「hash 不符」校验必须以受保护修订为准。
  2. 不重开 S1 / R-3；不解冻 Bv。
  3. 成功 ≠ 采用：本文档（含全部「建议验证」项）不产生任何授权；采数仍须 LOCK + APPROVED。
  4. 旁路挂账不挡：旁路事项（如 M2-02 ETW 草案）只挂账，不阻塞本票。
- **证据现场说明**：`artifacts/win_harness_noise_probe.txt` 与 `docs/m2_01_gate_DRAFT.md` **均不在本工作区**。本票未运行任何测量；对 G2 的复核依据 = 任务书转述的 DRAFT 条文。LOCK 前若实际 gate 文件条文与转述有出入，以实际文件为准并重跑本复核。

## 2. 底噪分解

按噪声源分类。每类给：**机制假说 / 与 #30 数字的挂钩 / 建议验证**（仍 analysis-only：写「建议 workbuddy 在 LOCK+APPROVED 后测什么」）。

### 2.1 位置估计器（WGC 捕获 + 像素/模板定位）

- **机制假说**：位置由逐帧捕获图像估计。像素网格量化产生 ±半像素级误差；亚像素插值、目标边缘模糊/运动模糊引入偏置与方差。这是任何基于图像的 harness 的**纯底噪下界来源**。
- **与 #30 挂钩**：#30 的 RMS 1.637 / 1.281 DIP，在 100% DPI 下即 ≈1.6 / 1.3 像素（推算），说明噪声以**像素级**为主，而非 DPI 换算级（换算级误差会随缩放改变量级）。max|err| 3.11 DIP ≈ 3 px（100% DPI 下）与「几个像素的估计抖动」量级吻合。
- **建议验证（LOCK+APPROVED 后）**：a) 静止探针连测 n≥60，估计器输出自方差 → 纯估计噪声下界；b) 同一静止场景在 100% / 150% / 200% 缩放下重复，看 RMS(DIP) 是否随缩放变化（判别像素级 vs 逻辑级）；c) 用已知精确位置的目标（细线、棋盘格角点）做亚像素标定。

### 2.2 对齐 τ（best-τ 对齐窗口）

- **机制假说**：运动探针做时间对齐（#30：τ=24ms）。τ 估计抖动 Δτ 直接折算为位置误差 ≈ 速度 × Δτ。对齐目标函数若平坦/多峰，Δτ 可达毫秒级。
- **与 #30 挂钩（推算）**：A 探针速度 = 240 DIP / 0.25 s = **960 DIP/s** → 1 ms τ 误差 ≈ **0.96 DIP**；max|err| 3.11 DIP ≈ 3.2 ms 等效 τ 误差。B 探针 240 / 0.32 s = 750 DIP/s → 1 ms ≈ 0.75 DIP。
  - **关键观察**：RMS 比 A/B = 1.637/1.281 ≈ **1.28**，速度比 = 960/750 = **1.28**，两者一致（推算）。加性像素噪声不应随速度缩放，而时间类误差天然与速度成比例——这**指向「时间类误差是主要候选」**。注意：这是 n=15/20 的两点观察，只作假说排序依据，不是证明。
- **建议验证**：a) 对 #30 两条原始序列重算 τ 敏感性：τ ±1/±2/±3 ms 扰动后 RMS 增长曲线，估计时间误差贡献占比；b) 静止口径下 τ 无物理意义，改测相邻帧时间戳的直方图/自相关，量化时间戳抖动；c) 若锁定时间类为主，优先投资时间戳源与对齐算法，而非像素估计器。

### 2.3 时钟 / vsync / 帧时间戳

- **机制假说**：应用 SteadyClock、合成器节拍、捕获时间戳三条时间线各自量化，节拍抖动折算为位置误差（同 2.2 的乘法）。仓库记录（`workbuddy_soft_demo.md` 派题 #5）：显示器休眠时 DWM 退回 ~32 Hz（帧间隔 mean 31.1 ms，RenderingTime 步进 31.2 ms ≈ 2×15.625 ms 系统节拍）；派题 #6：休眠时合成器动画时钟不按真实时间推进（1000 ms 关键帧 13.6 ms 即「完成」）。
- **与 #30 挂钩**：若 #30 会话存在同类节拍量化（±7.8 ms 级），未对齐的原始残差将是 7.5 DIP 级（推算：960 DIP/s × 7.8 ms）；#30 报告 1.6 / 1.28 DIP，说明 best-τ 已吸收大部分时间误差，残余 = 对齐后的 τ 抖动。这支持 2.2 的候选排序。
- **建议验证**：a) 采数时逐帧记录捕获时间戳 + RenderingTime + Stopwatch，输出间隔直方图/自相关，证明 vblank 真实存在；b) LOCK 下保证显示器处于唤醒状态（人工输入或 `SetThreadExecutionState`，并在日志留痕）；c) 用 DComp `GetFrameStatistics` 核对合成节拍。

### 2.4 合成器调度 / 合成批处理

- **机制假说**：DWM 在 vblank 边界合成，捕获拿到的是上一节拍的画面；调度抖动 → 重采样误差；休眠/锁屏/后台状态会改变合成节拍（见 2.3 引用）。此外，运动探针把**合成器轨迹本身**当作观测对象，若合成器轨迹与理想直线存在系统偏差（起止段缓动、vblank 对齐相位），该偏差会被计入「噪声」。
- **与 #30 挂钩**：#30 的「vs 理想直线」参照天然混入上述轨迹保真偏差；RMS 0.682% / 0.534% travel 的百分比很小，说明没有大的系统偏移（常数偏移会以百分比形式显现）。
- **建议验证**：a) 静止口径天然免疫本节大部分误差——这正是 G2 选静止/零激励场景的合理性所在；b) 若要保留运动口径作参考，先测合成器实际轨迹得出「已知系统偏差曲线」，再从残差中分解。

### 2.5 DPI / 坐标 / 捕获几何

- **机制假说**：DIP↔像素换算舍入；捕获裁切、窗口边框/阴影/圆角造成几何原点偏移；WGC 缩放/边框模式（freeform vs BorderRequired）差异；光标排除与否。
- **与 #30 挂钩**：#30 未报告缩放与捕获几何参数——**这本身就是一个口径缺口**（见 §4）。两条探针的百分比误差很小，间接说明无大的常数几何偏移。
- **建议验证**：a) 采数时写死并记录：DPI 缩放、捕获分辨率、目标窗口逻辑尺寸、坐标原点、是否含边框；b) 不同缩放下的静止 RMS 对照（同 2.1b）。

### 2.6 动画路径 vs 静止探针差异（口径缺口，详见 §3）

- **机制假说**：运动探针误差 = 纯底噪 + 速度×时间误差 + 轨迹保真偏差；静止探针误差 = 纯底噪（估计器 + 合成静止抖动）。
- **与 #30 挂钩**：#30 全部来自线性运动（0.25s/0.32s KeyFrame 0→240）；G2 要求静止/零激励。#30 的数字是**运动口径**的误差，不能直接当作静止底噪的预期值。
- **建议验证**：按 G2 静止口径重新采数（LOCK 后）——这正是 G2 的全部意义。

### 2.7 影子 2.6×（点到为止；根因主责 Gemini / G3）

- #37：影子速度约为屏幕 2.6×；A 臂可见跳变约 100–207 DIP；禁止 `spring.h` 影子 x/v 作 seed（deepseek_m1b_spec）。
- **对本票的含义**：若 harness 读数路径混入影子副本，位置读数会被缩放，噪声数字随之失真。G2 条文应声明测量目标 = 屏幕主副本，排除影子路径。此点不展开（G3 主责）。

## 3. 现状 vs 目标

- **目标**：<0.5 DIP（静止/零激励；会话中位；3/3 会话均达标）。
- **#30 现状**：RMS 1.637 / 1.281 DIP（运动口径），max|err| 3.11 / 2.47 DIP。
- **关系**：若直接拿运动 RMS 对照 0.5 → 差距 3.3× / 2.6×（推算）。但**该对照口径不成立**（运动 ≠ 静止）。两种可能世界（均为假设，未验证）：
  - **H1 时间类主导**：静止时速度 → 0，时间误差项消失，静止底噪可能显著低于 1.6 DIP，甚至天然 <0.5；
  - **H2 估计器/像素类主导**：静止底噪仍是像素级 ≈1.5 DIP 上下，0.5 DIP 需要亚像素估计或高 DPI 捕获，逼近估计器下限。
  - #30 内部证据（§2.2 速度比≈误差比）**偏 H1**，但样本太小，只作排序依据。判定只能靠 LOCK 后的静止采数。
- **口径缺口（必须明说）**：
  1. **运动 vs 静止**：#30 是线性 KeyFrame 0→240，非静止；G2 要求静止/零激励（或 LOCK 写死等价场景）。测量对象不同。
  2. **参照不同**：#30 对照理想直线（含轨迹保真假设）；G2 对照恒定位置（零激励真值自明）。
  3. **样本量**：#30 n=15/20（≈60 采样/s 折算），G2 待写死「n≥60 帧或既有惯例」二选一。#30 样本不可复用为 G2 样本。
  4. **会话数**：#30 两条探针 ≠ 3 个有效会话。
  5. **指标未定**：#30 报告 RMS 与 max|err|；G2 的「RMS 或等价 harness 底噪量」未写死。
- **结论**：#30 是背景证据（说明时间类误差候选与像素级量级），不可作 G2 判定样本。notion_brief 中「0.5 DIP 严格门低于 harness 底噪（3.11 DIP）」应理解为**当时以 max|err| 3.11 作底噪上界的历史判断**，不代表静止口径的预期；R-2′ 已改相对门，0.5 DIP 转 M2 目标——本票即围绕该目标。

## 4. G2 复核意见

**结论：有条件同意，须改后方可 LOCK。** DRAFT 框架（静止/零激励、会话≥3、中位聚合、DIP 单位、INVALID 类目）方向正确；但下列项不写死，「口径偏离」将无从判定，INVALID 条款形同虚设。

| # | 漏洞/待写死项 | 复核意见 |
|---|---|---|
| G2-1 | 指标「RMS **或**等价 harness 口径」两可 | 写死公式：逐帧误差 RMS 与「既有 harness 口径」二选一，LOCK 时给出公式与计算代码哈希，禁止「或」 |
| G2-2 | 「静止/零激励」未定义时长与场景 | 写死：会话全程目标属性恒定、无动画、无输入、无 retarget；场景参数逐会话记录 |
| G2-3 | 单位/几何未写死 | 写死：误差以 DIP 计；记录 DPI 缩放、捕获分辨率、坐标原点、px↔DIP 换算公式 |
| G2-4 | 样本「n≥60 或既有惯例」两可；#30 n=15/20 不满足 | 写死二选一；明示 #30 样本不可复用 |
| G2-5 | 有效会话 <3 时的判定路径缺失 | 写死：有效会话 ≥3 方可判定；无效会话整会话重采，不得补测拼凑 |
| G2-6 | 「hash 不符」校验对象缺失 | 写死：受保护二进制的名称、版本、sha256（本快照 composition.h 与禁令 hash 不一致，LOCK 时必须指认正确修订） |
| G2-7 | 授权状态 | 保留：LOCK+APPROVED 前任何采数 → INVALID（DRAFT 已含，确认） |
| G2-8 | #30 与 G2 的对照关系未声明 | 增加：G2 判定仅用静止口径新采数；#30 仅作背景证据，不参与判定 |
| G2-9 | max\|err\| 无着落 | 建议：每会话记录 max\|err\| 存档、不设判定门（避免与 RMS 中位双指标冲突）；若设门须单独写死阈值 |
| G2-10 | 影子 2.6× 风险未排除 | 增加：测量目标 = 屏幕主副本，排除影子路径（呼应 #37 与 deepseek_m1b_spec） |

**可粘贴的修订建议条文（短）：**

> **G2 底噪（修订建议，LOCK 前写死）**
> - 场景：静止/零激励——会话全程目标属性恒定、无动画、无输入、无 retarget；场景参数（目标、属性、位置、窗口、缩放）逐会话记录；显示器须处于有真实 vblank 的唤醒状态并在日志留痕。
> - 会话：≥3 个有效会话；任一会话无效须整会话重采，不得补测拼凑；有效会话 <3 → INVALID。
> - 样本：每会话有效帧 n≥60（或 LOCK 批准的既有惯例，二选一写死，附计算公式）；无效帧（捕获丢失/时间戳异常）剔除并记录。
> - 指标：每帧位置误差 e_i = 估计位置 − 参照位置（DIP）；会话内指标 S = sqrt(mean(e_i²))（RMS）；判定量 = 各会话 S 的**中位**。
> - 单位与几何：误差以 DIP 计；记录 DPI 缩放、捕获分辨率、坐标原点；px↔DIP 换算公式随结果存档。
> - 判定：PASS ⇔ 有效会话 ≥3 且 3/3 会话中位均 <0.5 DIP；FAIL ⇔ 任一会话 ≥0.5 DIP 且样本足；INVALID = 样本不足 / 口径偏离（以上各项任一不匹配）/ 未授权采数 / 受保护二进制 hash 不符（对象 = __[LOCK 时填写：二进制名 + sha256]__）。
> - 背景证据：#30（线性运动探针）只作背景，不参与 G2 判定；测量目标为屏幕主副本，排除影子路径。
> - max|err|：每会话记录并存档，不设判定门。

## 5. LOCK 前一句话

LOCK 前还缺：把 G2 口径五项——指标公式（RMS vs 既有口径）、静止场景定义、单位/几何、每会话 n≥60、有效会话≥3——连同授权与 hash 校验对象写死并 LOCK，然后以**静止口径重新采数**（#30 不可代用）；本票不解冻 Bv、不改 composition.h（`a55d4952…`），2.6× 根因归 G3（Gemini）。

===== PART B =====

# M2-02：ETW / C# API 草案（仅文档，DRAFT）

> 交付：DeepSeek · M2-02 并行文档票（仅文档，不测、不采数）
> 落点：`docs/deepseek_m2_02_api_draft.md`
> 日期：2026-10-24（本地时区 Australia/Perth）
> 状态：**DRAFT**。本草案**不授权实现、不授权测试**；是否采用另行决策（成功 ≠ 采用）。

## 1. 定位与边界

- **依据**：`plan.md`（M2 Alpha：C# API 可用、可打断重定向；ETW 可用；预览宿主 v0；性能 CI ≥2 台物理机；任务 7「WinMD 接口与 C# 投影」、任务 10「ETW 事件与调试浮层」、任务 13「测试基础设施」）、`api.md` §7/§8、`workbuddy_soft_demo.md`。
- **现状基线**：C# 经 C-ABI `fm_capi.h` + P/Invoke **已实现（仅软件路径）**；WinMD/WinUI 投影**规划中、代码中不存在**；真实合成器后端（`ICompositorBackend` 的 WinRT 实现，模块 7）不存在（`api.md` §8）。
- **分层**：L0 现状（fm_capi P/Invoke）→ L1 本草案（托管封装面）→ L2 规划占位（WinMD 投影，不展开）。
- **硬边界**：不改 `composition.h`（sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）；不重开 S1 / R-3；不解冻 Bv；旁路挂账不挡。

## 2. ETW 事件 schema 草案（DRAFT）

**Provider（占位）**：名称 `FluidMotion-Motion`；GUID、事件号、负载布局**均未冻结**（事件定义归 `plan.md` 任务 10，本草案只给出方向与最小集）。

约定（DRAFT）：全部无 PII；字符串 UTF-8 且长度受限；Verbose 级（FrameTick / NoiseProbe）默认关，Information 级（生命周期 / 决策 / Idle）默认开；ETW 供 WPA 分析，PresentStats 与 PresentMon 对照。

| # | 事件名 | 关键字段（类型占位） | 何时发出 |
|---|---|---|---|
| 1 | `AnimationLifecycle` | sessionId(UInt64), animId(UInt64), target(UInt64), transition(UTF8,可空), state(Enum: Created/Played/Completed/Cancelled/Superseded/Paused), endReason(Enum,可空) | 动画创建、播放、终态与暂停时 |
| 2 | `Retarget` | sessionId, animId, property(UTF8), from(Float64,可空), to(Float64), response(Float64), damping(Float64), hasFrom(UInt8), velocityAtHandoff(Float64,可空) | 每次可打断重定向调用与交接时——**可打断重定向的观测核心** |
| 3 | `LoweringDecision` | controllerId, target, property, planKind(Enum: CompositorKeyframes/CompositorSpring/Hold/AppThread), reason(UTF8), keyframeCount(UInt32,可空) | 能力分发器每次重算计划时——能力分发/降级的可观测性 |
| 4 | `FrameTick` | thread(Enum: UI/Render), deltaUs(UInt32), engineTickUs(UInt32), activeCount(UInt32) | 每个引擎帧（UI 线程）；渲染线程侧由 WPA/PresentMon 对照 |
| 5 | `PresentStats` | dropped(UInt32), intervalMs(Float64), vsyncSource(Enum,可空) | 每会话汇总或按秒汇总——供性能 CI 与丢帧率核对 |
| 6 | `IdleTransition` | activeCount(UInt32), pumpOn(UInt8) | 活跃动画数 0↔1 切换时——对应「闲置 0 计时器、静止即停」验收 |
| 7 | `TokenLoad` | name(UTF8), problemCount(UInt32), parseUs(UInt32) | token 加载/替换成功或失败时——「token 修改 2 秒上屏」观测起点 |
| 8 | `NoiseProbe`（挂账，服务 M2-01 G2，仅为草案） | probeSessionId, frameIndex(UInt32), estPosDIP(Float64), refPosDIP(Float64), errDIP(Float64), tauMs(Float64,可空), captureMode(UTF8) | 探针每帧估计输出；**仅 LOCK+APPROVED 采数后使用**，M2-02 不触发 |

**明确不承诺**：本表是方向草案；「ETW 可用」的验收 = 上述最小集（事件 1–7）在真机可采集且可进 WPA；实现与验收另行授权。

## 3. C# API 面草案（DRAFT）

### 3.1 L0 现状（已实现，勿当作规划）

`Fm` / `FmBridge`：对 `fm_capi.h` 14 个导出的 Cdecl P/Invoke 薄封装。既有规则（`api.md` §7.5）：单 UI 线程调用；sink/render 回调内**禁调** `fm_*`；`SinkFn`/`RenderFn` 委托保活（存为字段）；UTF-8 字符串与 `IntPtr` 类型对应；帧源用 `CompositionTarget.Rendering`，静止即退订（`workbuddy_soft_demo.md` 派题 #4/#5）。

### 3.2 L1 草案（托管封装面；DRAFT，不授权实现）

- **目标**：把「句柄生命周期 + 委托保活 + 帧泵 + 线程亲和」收进可 dispose 的托管对象；应用只面向高层操作；保留 `Fm` 直接 P/Invoke 出口作诊断逃生舱。
- **面（草图，签名仅为方向，不冻结）**：

```csharp
public sealed class MotionEngine : IDisposable {
    public static MotionEngine Create();              // 构造线程即 UI 线程，线程亲和断言
    public void LoadTokens(string json);              // 失败给出问题列表（沿用 fm_load_tokens 语义）
    public bool IsRunning { get; }
    public event Action? IdleChanged;                 // 对应 ETW IdleTransition
    public void Dispose();                            // 必退订帧源 + fm_destroy
}
public sealed class MotionAnimation {
    public void Play();
    public void Cancel();                             // 终态事件 Completed/Cancelled/Superseded
    public void Release();                            // 只丢本侧句柄，动画继续跑完
    public bool RetargetTo(string property, double target,
                           double response, double damping, double? from = null);
    // from != null ⇔ hasFrom=1（跨路径交接，见 3.4.2）
}
```

- 若被采纳：并入 `plan.md` 任务 7 的 C# 投影工作（依赖任务 4/5——均已交付初版）；本草案不写实现细节。

### 3.3 L2 规划占位（WinMD 投影；不展开）

规划中的 `MotionAnimator` / `AnimateProperty` 风格面，经 C++/WinRT + WinMD 提供；**当前代码中不存在**（`api.md` §8），勿当作已有 API。本草案只登记。落地前提 = 真实合成器后端（`ICompositorBackend` 的 WinRT 实现，模块 7，**亦不存在**）——该层不在 M2-02 草案范围内。

### 3.4 可打断重定向要点（基于已实现语义，供 L1 草案固化）

1. **连续性 = 从当前状态接管**：Retarget 以当前值/速度为起点（K5 语义）；原生 manual-clock 实测 |Δv|=0、|Δvel|=0，应用内渲染序列无跳变（`workbuddy_soft_demo.md` 派题 #4/#5）。
2. **hasFrom 交接**：合成器路径 → 软件路径切换必须带 from（对应 `fm_animation_retarget` 的 hasFrom/from 与派题 #6 的 RetargetFrom）；L1 的 `RetargetTo` 已为此保留 `from?` 形参。
3. **合成器路径接续假设未验证**：派题 #6 中「InitialValue/InitialVelocity 不设 = 从当前接续」是**设计假设**，真机验证缺失；L1 若涉及跨路径切换须挂「未验证」标记。
4. **帧源**：软件路径用 `CompositionTarget.Rendering`（16ms `DispatcherQueueTimer` 实际 ~31 ms 只影响平滑度，不影响轨迹——时间由引擎自身 SteadyClock 计）；帧源选择归 L1 内部，不暴露给调用方。
5. **静止即停**：ActiveCount==0 → 退订帧源（对应 ETW `IdleTransition`）。
6. **线程/回调纪律**沿用 L0：单 UI 线程；sink 回调内禁调 `fm_*`。

### 3.5 明确不承诺 / 未验证清单

WinMD 投影、真实合成器后端（模块 7）、公开 `SnapToEnd`（未暴露）、`EmbedToHwnd`/HWND 正式 API（V1.5 再评估）、帧预算与 Rive 指针延迟数字（见 `disputes.md`）；`fm_current` 返回上次 Tick 采样（打断时刻真值在 retarget 内按当前时钟求值）——L1 文档须写明该语义。

## 4. 验收对应与状态重申

- **对照 `plan.md` M2 Alpha**：C# API 可用 ⇔ 本草案 L1（若采纳 → 实现 + 真机验证）；ETW 可用 ⇔ §2 最小事件集（若采纳 → 任务 10 实现）；预览宿主 v0 与性能 CI ≥2 台物理机**不在本草案范围**。
- **状态**：DRAFT；不授权实现/测试；不改 `composition.h`（`a55d4952…`）；不重开 S1/R-3；不解冻 Bv；成功 ≠ 采用；旁路挂账不挡。

—— 交卷完毕。要点：Part A 结论为「有条件同意 G2 框架，10 处须 LOCK 前写死」并附可粘贴修订条文；关键分析发现 #30 的误差比≈速度比，指向时间类误差（τ 对齐/时钟）为主要候选（仅假说排序，非证明），且 #30 运动口径不可对照 G2 静止口径。Part B 给出 8 事件 ETW 最小集与 L0→L1→L2 分层 C# 面草案，全部标记 DRAFT、不授权实现/测试。两份文件可在 <motion_framework_review/docs/deepseek_m2_01_noise.md> 与 <motion_framework_review/docs/deepseek_m2_02_api_draft.md> 打开。
Both files written. Now deliver the final answer: full text of Part A, separator, full text of Part B, per the 交卷格式. Also note file locations. Keep the reply as the two full documents (they asked for full text). This will be long but that's the required format.
交卷。两份文档已写入 `motion_framework_review/docs/`：`docs/deepseek_m2_01_noise.md` 与 `docs/deepseek_m2_02_api_draft.md`（该目录为本工作区仅有的仓库快照，`docs/m2_01_gate_DRAFT.md` 与 `artifacts/win_harness_noise_probe.txt` 不在此工作区，复核依据为任务书转述条文，已在文内声明）。全程未运行任何测量、未修改任何源文件；快照 `composition.h` 实测 sha256（3411aebd…）与禁令所列（a55d4952…）不一致，仅作提示未作处理。

# M2-01：harness 底噪分解与 G2（DRAFT）复核意见

> 交付：DeepSeek · M2-01 文档票（analysis-only，简体中文）
> 落点：`docs/deepseek_m2_01_noise.md`
> 日期：2026-10-24（本地时区 Australia/Perth）
> 状态：分析文档；**不含新测量、不含采数、不含实现**。

## 1. 元信息与禁令重申

- **本票范围**：harness 底噪分解（目标 <0.5 DIP）＋ 闸门 G2（DRAFT）复核意见。不测试、不采数、不改码。
- **事实来源**：仅用任务书「共享事实」#30（`artifacts/win_harness_noise_probe.txt`，2026-10-20）、#37、notion_brief，以及本仓库既有文档（`plan.md` / `api.md` / `workbuddy_soft_demo.md`）。所有数字注明出处；由给出数字直接推算的量显式标「推算」。
- **硬边界（本票执行时严格遵守，重申）**：
  1. 不改 `composition.h`；被冻结修订 sha256 = `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`。
     - 提示：本工作区快照中 `composition.h` 的实测 sha256 为 `3411aebdb10b7956c731ab0541796d9d671fae97e9835103c770cbc858c899c3`，与禁令所列**不一致**——本快照不是受保护修订。G2 的「hash 不符」校验必须以受保护修订为准。
  2. 不重开 S1 / R-3；不解冻 Bv。
  3. 成功 ≠ 采用：本文档（含全部「建议验证」项）不产生任何授权；采数仍须 LOCK + APPROVED。
  4. 旁路挂账不挡：旁路事项（如 M2-02 ETW 草案）只挂账，不阻塞本票。
- **证据现场说明**：`artifacts/win_harness_noise_probe.txt` 与 `docs/m2_01_gate_DRAFT.md` **均不在本工作区**。本票未运行任何测量；对 G2 的复核依据 = 任务书转述的 DRAFT 条文。LOCK 前若实际 gate 文件条文与转述有出入，以实际文件为准并重跑本复核。

## 2. 底噪分解

按噪声源分类。每类给：**机制假说 / 与 #30 数字的挂钩 / 建议验证**（仍 analysis-only：写「建议 workbuddy 在 LOCK+APPROVED 后测什么」）。

### 2.1 位置估计器（WGC 捕获 + 像素/模板定位）

- **机制假说**：位置由逐帧捕获图像估计。像素网格量化产生 ±半像素级误差；亚像素插值、目标边缘模糊/运动模糊引入偏置与方差。这是任何基于图像的 harness 的**纯底噪下界来源**。
- **与 #30 挂钩**：#30 的 RMS 1.637 / 1.281 DIP，在 100% DPI 下即 ≈1.6 / 1.3 像素（推算），说明噪声以**像素级**为主，而非 DPI 换算级（换算级误差会随缩放改变量级）。max|err| 3.11 DIP ≈ 3 px（100% DPI 下）与「几个像素的估计抖动」量级吻合。
- **建议验证（LOCK+APPROVED 后）**：a) 静止探针连测 n≥60，估计器输出自方差 → 纯估计噪声下界；b) 同一静止场景在 100% / 150% / 200% 缩放下重复，看 RMS(DIP) 是否随缩放变化（判别像素级 vs 逻辑级）；c) 用已知精确位置的目标（细线、棋盘格角点）做亚像素标定。

### 2.2 对齐 τ（best-τ 对齐窗口）

- **机制假说**：运动探针做时间对齐（#30：τ=24ms）。τ 估计抖动 Δτ 直接折算为位置误差 ≈ 速度 × Δτ。对齐目标函数若平坦/多峰，Δτ 可达毫秒级。
- **与 #30 挂钩（推算）**：A 探针速度 = 240 DIP / 0.25 s = **960 DIP/s** → 1 ms τ 误差 ≈ **0.96 DIP**；max|err| 3.11 DIP ≈ 3.2 ms 等效 τ 误差。B 探针 240 / 0.32 s = 750 DIP/s → 1 ms ≈ 0.75 DIP。
  - **关键观察**：RMS 比 A/B = 1.637/1.281 ≈ **1.28**，速度比 = 960/750 = **1.28**，两者一致（推算）。加性像素噪声不应随速度缩放，而时间类误差天然与速度成比例——这**指向「时间类误差是主要候选」**。注意：这是 n=15/20 的两点观察，只作假说排序依据，不是证明。
- **建议验证**：a) 对 #30 两条原始序列重算 τ 敏感性：τ ±1/±2/±3 ms 扰动后 RMS 增长曲线，估计时间误差贡献占比；b) 静止口径下 τ 无物理意义，改测相邻帧时间戳的直方图/自相关，量化时间戳抖动；c) 若锁定时间类为主，优先投资时间戳源与对齐算法，而非像素估计器。

### 2.3 时钟 / vsync / 帧时间戳

- **机制假说**：应用 SteadyClock、合成器节拍、捕获时间戳三条时间线各自量化，节拍抖动折算为位置误差（同 2.2 的乘法）。仓库记录（`workbuddy_soft_demo.md` 派题 #5）：显示器休眠时 DWM 退回 ~32 Hz（帧间隔 mean 31.1 ms，RenderingTime 步进 31.2 ms ≈ 2×15.625 ms 系统节拍）；派题 #6：休眠时合成器动画时钟不按真实时间推进（1000 ms 关键帧 13.6 ms 即「完成」）。
- **与 #30 挂钩**：若 #30 会话存在同类节拍量化（±7.8 ms 级），未对齐的原始残差将是 7.5 DIP 级（推算：960 DIP/s × 7.8 ms）；#30 报告 1.6 / 1.28 DIP，说明 best-τ 已吸收大部分时间误差，残余 = 对齐后的 τ 抖动。这支持 2.2 的候选排序。
- **建议验证**：a) 采数时逐帧记录捕获时间戳 + RenderingTime + Stopwatch，输出间隔直方图/自相关，证明 vblank 真实存在；b) LOCK 下保证显示器处于唤醒状态（人工输入或 `SetThreadExecutionState`，并在日志留痕）；c) 用 DComp `GetFrameStatistics` 核对合成节拍。

### 2.4 合成器调度 / 合成批处理

- **机制假说**：DWM 在 vblank 边界合成，捕获拿到的是上一节拍的画面；调度抖动 → 重采样误差；休眠/锁屏/后台状态会改变合成节拍（见 2.3 引用）。此外，运动探针把**合成器轨迹本身**当作观测对象，若合成器轨迹与理想直线存在系统偏差（起止段缓动、vblank 对齐相位），该偏差会被计入「噪声」。
- **与 #30 挂钩**：#30 的「vs 理想直线」参照天然混入上述轨迹保真偏差；RMS 0.682% / 0.534% travel 的百分比很小，说明没有大的系统偏移（常数偏移会以百分比形式显现）。
- **建议验证**：a) 静止口径天然免疫本节大部分误差——这正是 G2 选静止/零激励场景的合理性所在；b) 若要保留运动口径作参考，先测合成器实际轨迹得出「已知系统偏差曲线」，再从残差中分解。

### 2.5 DPI / 坐标 / 捕获几何

- **机制假说**：DIP↔像素换算舍入；捕获裁切、窗口边框/阴影/圆角造成几何原点偏移；WGC 缩放/边框模式（freeform vs BorderRequired）差异；光标排除与否。
- **与 #30 挂钩**：#30 未报告缩放与捕获几何参数——**这本身就是一个口径缺口**（见 §4）。两条探针的百分比误差很小，间接说明无大的常数几何偏移。
- **建议验证**：a) 采数时写死并记录：DPI 缩放、捕获分辨率、目标窗口逻辑尺寸、坐标原点、是否含边框；b) 不同缩放下的静止 RMS 对照（同 2.1b）。

### 2.6 动画路径 vs 静止探针差异（口径缺口，详见 §3）

- **机制假说**：运动探针误差 = 纯底噪 + 速度×时间误差 + 轨迹保真偏差；静止探针误差 = 纯底噪（估计器 + 合成静止抖动）。
- **与 #30 挂钩**：#30 全部来自线性运动（0.25s/0.32s KeyFrame 0→240）；G2 要求静止/零激励。#30 的数字是**运动口径**的误差，不能直接当作静止底噪的预期值。
- **建议验证**：按 G2 静止口径重新采数（LOCK 后）——这正是 G2 的全部意义。

### 2.7 影子 2.6×（点到为止；根因主责 Gemini / G3）

- #37：影子速度约为屏幕 2.6×；A 臂可见跳变约 100–207 DIP；禁止 `spring.h` 影子 x/v 作 seed（deepseek_m1b_spec）。
- **对本票的含义**：若 harness 读数路径混入影子副本，位置读数会被缩放，噪声数字随之失真。G2 条文应声明测量目标 = 屏幕主副本，排除影子路径。此点不展开（G3 主责）。

## 3. 现状 vs 目标

- **目标**：<0.5 DIP（静止/零激励；会话中位；3/3 会话均达标）。
- **#30 现状**：RMS 1.637 / 1.281 DIP（运动口径），max|err| 3.11 / 2.47 DIP。
- **关系**：若直接拿运动 RMS 对照 0.5 → 差距 3.3× / 2.6×（推算）。但**该对照口径不成立**（运动 ≠ 静止）。两种可能世界（均为假设，未验证）：
  - **H1 时间类主导**：静止时速度 → 0，时间误差项消失，静止底噪可能显著低于 1.6 DIP，甚至天然 <0.5；
  - **H2 估计器/像素类主导**：静止底噪仍是像素级 ≈1.5 DIP 上下，0.5 DIP 需要亚像素估计或高 DPI 捕获，逼近估计器下限。
  - #30 内部证据（§2.2 速度比≈误差比）**偏 H1**，但样本太小，只作排序依据。判定只能靠 LOCK 后的静止采数。
- **口径缺口（必须明说）**：
  1. **运动 vs 静止**：#30 是线性 KeyFrame 0→240，非静止；G2 要求静止/零激励（或 LOCK 写死等价场景）。测量对象不同。
  2. **参照不同**：#30 对照理想直线（含轨迹保真假设）；G2 对照恒定位置（零激励真值自明）。
  3. **样本量**：#30 n=15/20（≈60 采样/s 折算），G2 待写死「n≥60 帧或既有惯例」二选一。#30 样本不可复用为 G2 样本。
  4. **会话数**：#30 两条探针 ≠ 3 个有效会话。
  5. **指标未定**：#30 报告 RMS 与 max|err|；G2 的「RMS 或等价 harness 底噪量」未写死。
- **结论**：#30 是背景证据（说明时间类误差候选与像素级量级），不可作 G2 判定样本。notion_brief 中「0.5 DIP 严格门低于 harness 底噪（3.11 DIP）」应理解为**当时以 max|err| 3.11 作底噪上界的历史判断**，不代表静止口径的预期；R-2′ 已改相对门，0.5 DIP 转 M2 目标——本票即围绕该目标。

## 4. G2 复核意见

**结论：有条件同意，须改后方可 LOCK。** DRAFT 框架（静止/零激励、会话≥3、中位聚合、DIP 单位、INVALID 类目）方向正确；但下列项不写死，「口径偏离」将无从判定，INVALID 条款形同虚设。

| # | 漏洞/待写死项 | 复核意见 |
|---|---|---|
| G2-1 | 指标「RMS **或**等价 harness 口径」两可 | 写死公式：逐帧误差 RMS 与「既有 harness 口径」二选一，LOCK 时给出公式与计算代码哈希，禁止「或」 |
| G2-2 | 「静止/零激励」未定义时长与场景 | 写死：会话全程目标属性恒定、无动画、无输入、无 retarget；场景参数逐会话记录 |
| G2-3 | 单位/几何未写死 | 写死：误差以 DIP 计；记录 DPI 缩放、捕获分辨率、坐标原点、px↔DIP 换算公式 |
| G2-4 | 样本「n≥60 或既有惯例」两可；#30 n=15/20 不满足 | 写死二选一；明示 #30 样本不可复用 |
| G2-5 | 有效会话 <3 时的判定路径缺失 | 写死：有效会话 ≥3 方可判定；无效会话整会话重采，不得补测拼凑 |
| G2-6 | 「hash 不符」校验对象缺失 | 写死：受保护二进制的名称、版本、sha256（本快照 composition.h 与禁令 hash 不一致，LOCK 时必须指认正确修订） |
| G2-7 | 授权状态 | 保留：LOCK+APPROVED 前任何采数 → INVALID（DRAFT 已含，确认） |
| G2-8 | #30 与 G2 的对照关系未声明 | 增加：G2 判定仅用静止口径新采数；#30 仅作背景证据，不参与判定 |
| G2-9 | max\|err\| 无着落 | 建议：每会话记录 max\|err\| 存档、不设判定门（避免与 RMS 中位双指标冲突）；若设门须单独写死阈值 |
| G2-10 | 影子 2.6× 风险未排除 | 增加：测量目标 = 屏幕主副本，排除影子路径（呼应 #37 与 deepseek_m1b_spec） |

**可粘贴的修订建议条文（短）：**

> **G2 底噪（修订建议，LOCK 前写死）**
> - 场景：静止/零激励——会话全程目标属性恒定、无动画、无输入、无 retarget；场景参数（目标、属性、位置、窗口、缩放）逐会话记录；显示器须处于有真实 vblank 的唤醒状态并在日志留痕。
> - 会话：≥3 个有效会话；任一会话无效须整会话重采，不得补测拼凑；有效会话 <3 → INVALID。
> - 样本：每会话有效帧 n≥60（或 LOCK 批准的既有惯例，二选一写死，附计算公式）；无效帧（捕获丢失/时间戳异常）剔除并记录。
> - 指标：每帧位置误差 e_i = 估计位置 − 参照位置（DIP）；会话内指标 S = sqrt(mean(e_i²))（RMS）；判定量 = 各会话 S 的**中位**。
> - 单位与几何：误差以 DIP 计；记录 DPI 缩放、捕获分辨率、坐标原点；px↔DIP 换算公式随结果存档。
> - 判定：PASS ⇔ 有效会话 ≥3 且 3/3 会话中位均 <0.5 DIP；FAIL ⇔ 任一会话 ≥0.5 DIP 且样本足；INVALID = 样本不足 / 口径偏离（以上各项任一不匹配）/ 未授权采数 / 受保护二进制 hash 不符（对象 = __[LOCK 时填写：二进制名 + sha256]__）。
> - 背景证据：#30（线性运动探针）只作背景，不参与 G2 判定；测量目标为屏幕主副本，排除影子路径。
> - max|err|：每会话记录并存档，不设判定门。

## 5. LOCK 前一句话

LOCK 前还缺：把 G2 口径五项——指标公式（RMS vs 既有口径）、静止场景定义、单位/几何、每会话 n≥60、有效会话≥3——连同授权与 hash 校验对象写死并 LOCK，然后以**静止口径重新采数**（#30 不可代用）；本票不解冻 Bv、不改 composition.h（`a55d4952…`），2.6× 根因归 G3（Gemini）。

===== PART B =====

# M2-02：ETW / C# API 草案（仅文档，DRAFT）

> 交付：DeepSeek · M2-02 并行文档票（仅文档，不测、不采数）
> 落点：`docs/deepseek_m2_02_api_draft.md`
> 日期：2026-10-24（本地时区 Australia/Perth）
> 状态：**DRAFT**。本草案**不授权实现、不授权测试**；是否采用另行决策（成功 ≠ 采用）。

## 1. 定位与边界

- **依据**：`plan.md`（M2 Alpha：C# API 可用、可打断重定向；ETW 可用；预览宿主 v0；性能 CI ≥2 台物理机；任务 7「WinMD 接口与 C# 投影」、任务 10「ETW 事件与调试浮层」、任务 13「测试基础设施」）、`api.md` §7/§8、`workbuddy_soft_demo.md`。
- **现状基线**：C# 经 C-ABI `fm_capi.h` + P/Invoke **已实现（仅软件路径）**；WinMD/WinUI 投影**规划中、代码中不存在**；真实合成器后端（`ICompositorBackend` 的 WinRT 实现，模块 7）不存在（`api.md` §8）。
- **分层**：L0 现状（fm_capi P/Invoke）→ L1 本草案（托管封装面）→ L2 规划占位（WinMD 投影，不展开）。
- **硬边界**：不改 `composition.h`（sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）；不重开 S1 / R-3；不解冻 Bv；旁路挂账不挡。

## 2. ETW 事件 schema 草案（DRAFT）

**Provider（占位）**：名称 `FluidMotion-Motion`；GUID、事件号、负载布局**均未冻结**（事件定义归 `plan.md` 任务 10，本草案只给出方向与最小集）。

约定（DRAFT）：全部无 PII；字符串 UTF-8 且长度受限；Verbose 级（FrameTick / NoiseProbe）默认关，Information 级（生命周期 / 决策 / Idle）默认开；ETW 供 WPA 分析，PresentStats 与 PresentMon 对照。

| # | 事件名 | 关键字段（类型占位） | 何时发出 |
|---|---|---|---|
| 1 | `AnimationLifecycle` | sessionId(UInt64), animId(UInt64), target(UInt64), transition(UTF8,可空), state(Enum: Created/Played/Completed/Cancelled/Superseded/Paused), endReason(Enum,可空) | 动画创建、播放、终态与暂停时 |
| 2 | `Retarget` | sessionId, animId, property(UTF8), from(Float64,可空), to(Float64), response(Float64), damping(Float64), hasFrom(UInt8), velocityAtHandoff(Float64,可空) | 每次可打断重定向调用与交接时——**可打断重定向的观测核心** |
| 3 | `LoweringDecision` | controllerId, target, property, planKind(Enum: CompositorKeyframes/CompositorSpring/Hold/AppThread), reason(UTF8), keyframeCount(UInt32,可空) | 能力分发器每次重算计划时——能力分发/降级的可观测性 |
| 4 | `FrameTick` | thread(Enum: UI/Render), deltaUs(UInt32), engineTickUs(UInt32), activeCount(UInt32) | 每个引擎帧（UI 线程）；渲染线程侧由 WPA/PresentMon 对照 |
| 5 | `PresentStats` | dropped(UInt32), intervalMs(Float64), vsyncSource(Enum,可空) | 每会话汇总或按秒汇总——供性能 CI 与丢帧率核对 |
| 6 | `IdleTransition` | activeCount(UInt32), pumpOn(UInt8) | 活跃动画数 0↔1 切换时——对应「闲置 0 计时器、静止即停」验收 |
| 7 | `TokenLoad` | name(UTF8), problemCount(UInt32), parseUs(UInt32) | token 加载/替换成功或失败时——「token 修改 2 秒上屏」观测起点 |
| 8 | `NoiseProbe`（挂账，服务 M2-01 G2，仅为草案） | probeSessionId, frameIndex(UInt32), estPosDIP(Float64), refPosDIP(Float64), errDIP(Float64), tauMs(Float64,可空), captureMode(UTF8) | 探针每帧估计输出；**仅 LOCK+APPROVED 采数后使用**，M2-02 不触发 |

**明确不承诺**：本表是方向草案；「ETW 可用」的验收 = 上述最小集（事件 1–7）在真机可采集且可进 WPA；实现与验收另行授权。

## 3. C# API 面草案（DRAFT）

### 3.1 L0 现状（已实现，勿当作规划）

`Fm` / `FmBridge`：对 `fm_capi.h` 14 个导出的 Cdecl P/Invoke 薄封装。既有规则（`api.md` §7.5）：单 UI 线程调用；sink/render 回调内**禁调** `fm_*`；`SinkFn`/`RenderFn` 委托保活（存为字段）；UTF-8 字符串与 `IntPtr` 类型对应；帧源用 `CompositionTarget.Rendering`，静止即退订（`workbuddy_soft_demo.md` 派题 #4/#5）。

### 3.2 L1 草案（托管封装面；DRAFT，不授权实现）

- **目标**：把「句柄生命周期 + 委托保活 + 帧泵 + 线程亲和」收进可 dispose 的托管对象；应用只面向高层操作；保留 `Fm` 直接 P/Invoke 出口作诊断逃生舱。
- **面（草图，签名仅为方向，不冻结）**：

```csharp
public sealed class MotionEngine : IDisposable {
    public static MotionEngine Create();              // 构造线程即 UI 线程，线程亲和断言
    public void LoadTokens(string json);              // 失败给出问题列表（沿用 fm_load_tokens 语义）
    public bool IsRunning { get; }
    public event Action? IdleChanged;                 // 对应 ETW IdleTransition
    public void Dispose();                            // 必退订帧源 + fm_destroy
}
public sealed class MotionAnimation {
    public void Play();
    public void Cancel();                             // 终态事件 Completed/Cancelled/Superseded
    public void Release();                            // 只丢本侧句柄，动画继续跑完
    public bool RetargetTo(string property, double target,
                           double response, double damping, double? from = null);
    // from != null ⇔ hasFrom=1（跨路径交接，见 3.4.2）
}
```

- 若被采纳：并入 `plan.md` 任务 7 的 C# 投影工作（依赖任务 4/5——均已交付初版）；本草案不写实现细节。

### 3.3 L2 规划占位（WinMD 投影；不展开）

规划中的 `MotionAnimator` / `AnimateProperty` 风格面，经 C++/WinRT + WinMD 提供；**当前代码中不存在**（`api.md` §8），勿当作已有 API。本草案只登记。落地前提 = 真实合成器后端（`ICompositorBackend` 的 WinRT 实现，模块 7，**亦不存在**）——该层不在 M2-02 草案范围内。

### 3.4 可打断重定向要点（基于已实现语义，供 L1 草案固化）

1. **连续性 = 从当前状态接管**：Retarget 以当前值/速度为起点（K5 语义）；原生 manual-clock 实测 |Δv|=0、|Δvel|=0，应用内渲染序列无跳变（`workbuddy_soft_demo.md` 派题 #4/#5）。
2. **hasFrom 交接**：合成器路径 → 软件路径切换必须带 from（对应 `fm_animation_retarget` 的 hasFrom/from 与派题 #6 的 RetargetFrom）；L1 的 `RetargetTo` 已为此保留 `from?` 形参。
3. **合成器路径接续假设未验证**：派题 #6 中「InitialValue/InitialVelocity 不设 = 从当前接续」是**设计假设**，真机验证缺失；L1 若涉及跨路径切换须挂「未验证」标记。
4. **帧源**：软件路径用 `CompositionTarget.Rendering`（16ms `DispatcherQueueTimer` 实际 ~31 ms 只影响平滑度，不影响轨迹——时间由引擎自身 SteadyClock 计）；帧源选择归 L1 内部，不暴露给调用方。
5. **静止即停**：ActiveCount==0 → 退订帧源（对应 ETW `IdleTransition`）。
6. **线程/回调纪律**沿用 L0：单 UI 线程；sink 回调内禁调 `fm_*`。

### 3.5 明确不承诺 / 未验证清单

WinMD 投影、真实合成器后端（模块 7）、公开 `SnapToEnd`（未暴露）、`EmbedToHwnd`/HWND 正式 API（V1.5 再评估）、帧预算与 Rive 指针延迟数字（见 `disputes.md`）；`fm_current` 返回上次 Tick 采样（打断时刻真值在 retarget 内按当前时钟求值）——L1 文档须写明该语义。

## 4. 验收对应与状态重申

- **对照 `plan.md` M2 Alpha**：C# API 可用 ⇔ 本草案 L1（若采纳 → 实现 + 真机验证）；ETW 可用 ⇔ §2 最小事件集（若采纳 → 任务 10 实现）；预览宿主 v0 与性能 CI ≥2 台物理机**不在本草案范围**。
- **状态**：DRAFT；不授权实现/测试；不改 `composition.h`（`a55d4952…`）；不重开 S1/R-3；不解冻 Bv；成功 ≠ 采用；旁路挂账不挡。

—— 交卷完毕。要点：Part A 结论为「有条件同意 G2 框架，10 处须 LOCK 前写死」并附可粘贴修订条文；关键分析发现 #30 的误差比≈速度比，指向时间类误差（τ 对齐/时钟）为主要候选（仅假说排序，非证明），且 #30 运动口径不可对照 G2 静止口径。Part B 给出 8 事件 ETW 最小集与 L0→L1→L2 分层 C# 面草案，全部标记 DRAFT、不授权实现/测试。两份文件可在 <motion_framework_review/docs/deepseek_m2_01_noise.md> 与 <motion_framework_review/docs/deepseek_m2_02_api_draft.md> 打开。