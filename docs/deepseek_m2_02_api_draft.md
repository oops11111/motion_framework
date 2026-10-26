# DeepSeek · M2-02 ETW / C# API 草案（文档 only）

- 时间：同上会话 `session-f2e8f3af-b61b-47e3-b3da-a8d8db44d93b`
- 状态：**DRAFT**；不授权实现/测试；不并行派 workbuddy
- 修订：（§3.4 已按 H5 CLOSED PASS 改为禁影子 IV；见 `docs/deepseek_m2_02_review.md`）
- 依据：`plan.md` M2、`docs/api.md` §7–§8

---

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
3. **禁止影子 IV**（承接 M2-H5-SEED CLOSED PASS）：L1/L2 投影与托管封装 **须** 走 WGC/屏一致速度 seed，或降级 IV=null+0；**禁止** 把 spring.h 闭式/影子速度接到 compositor `InitialVelocity`。H5 PASS 已验证主路径（A=`calib * v_wgc`，calib=1.0；B=IV=null+0）；成功 ≠ 采用。
4. **合成器路径接续假设未验证**：派题 #6 中「InitialValue/InitialVelocity 不设 = 从当前接续」是**设计假设**，真机验证缺失；L1 若涉及跨路径切换须挂「未验证」标记。
5. **帧源**：软件路径用 `CompositionTarget.Rendering`（16ms `DispatcherQueueTimer` 实际 ~31 ms 只影响平滑度，不影响轨迹——时间由引擎自身 SteadyClock 计）；帧源选择归 L1 内部，不暴露给调用方。
6. **静止即停**：ActiveCount==0 → 退订帧源（对应 ETW `IdleTransition`）。
7. **线程/回调纪律**沿用 L0：单 UI 线程；sink 回调内禁调 `fm_*`。

### 3.5 明确不承诺 / 未验证清单

WinMD 投影、真实合成器后端（模块 7）、公开 `SnapToEnd`（未暴露）、`EmbedToHwnd`/HWND 正式 API（V1.5 再评估）、帧预算与 Rive 指针延迟数字（见 `disputes.md`）；`fm_current` 返回上次 Tick 采样（打断时刻真值在 retarget 内按当前时钟求值）——L1 文档须写明该语义。

## 4. 验收对应与状态重申

- **对照 `plan.md` M2 Alpha**：C# API 可用 ⇔ 本草案 L1（若采纳 → 实现 + 真机验证）；ETW 可用 ⇔ §2 最小事件集（若采纳 → 任务 10 实现）；预览宿主 v0 与性能 CI ≥2 台物理机**不在本草案范围**。
- **状态**：DRAFT；不授权实现/测试；不改 `composition.h`（`a55d4952…`）；不重开 S1/R-3；不解冻 Bv；成功 ≠ 采用；旁路挂账不挡。
