# FluidMotion 使用方法

> **前提**：核心在 `code/*.h`（header-only C++17，命名空间 `motion`）。  
> **测试状态**：timeline 422 / tokens 743 / controller 236 / composition 2063 项检查、0 失败，Linux g++ 14.2 与 Windows MSVC 19.44 x64 两平台均已跑通（另 C-ABI 测试 23 项）。  
> **已接线**：C# 经 C-ABI（`code/capi/fm_capi.h`）+ P/Invoke 走软件路径（§4）。`composition.h` 规划器 / `CompositionDispatcher` 已实现但仅在模拟后端验证。  
> **未接线**：Windows 真实合成器后端、WinMD / WinUI 投影 — **规划中**。  
> 估计与待真机项单独标明。

配套 API 清单见 [`api.md`](./api.md)。架构背景见 [`engineering.md`](./engineering.md)。

---

## 1. 端到端：软件路径（当前可用）

软件路径 = 自己提供时钟与 `IDispatcher`（或 `NullDispatcher`），每帧 `Tick()`，在 `Apply` 里写回 UI。

### 1.1 初始化 `MotionSystem` + Dispatcher

```cpp
#include "controller.h"  // 拉入 timeline / tokens / spring / bezier
#include <fstream>
#include <iostream>       // std::cerr（§1.2）
#include <sstream>

using namespace motion;

// 测试或应用：可控时钟。全局变量勿命名为 clock：会与 C 库函数 ::clock() 冲突
ManualClock gClock;

// 最小软件分发：把 Apply 写到你的属性表
struct SoftDispatcher final : IDispatcher {
    std::map<TargetId, SampleMap> latest;

    void Submit(const ControllerSnapshot&) override {
        // 合成器路径：在此 lowering；软件路径可空实现
    }
    void Withdraw(ControllerId, EndReason) override {
        // Completed 也会在控制器「变为空闲」时到来（如暂停姿态），不一定是播完
    }
    void Apply(TargetId id, const SampleMap& samples) override {
        // 仅在有活跃控制器的 Tick 调用；同一 target 的 owned 值已合并为一个 map
        latest[id] = samples;
        // 例：把 samples["opacity"].value 写到控件
    }
};

SoftDispatcher dispatcher;
MotionSystem sys(gClock, &dispatcher);
// 或：MotionSystem sys(gClock);  // 使用 NullDispatcher，仅靠 handle.Value / sys.Current 读值
```

**线程**：所有 `Create` / `Play` / `Tick` / 句柄操作须在同一条 dispatcher 线程。头文件声明：**非线程安全**。

### 1.2 加载 tokens

```cpp
// 需要 <fstream> <sstream> <iostream>（已在 §1.1 包含）
std::ifstream in("motion.json");
std::stringstream buf;
buf << in.rdbuf();

std::vector<Diagnostic> errors;
auto tokens = TokenSet::Parse(buf.str(), errors);
if (!tokens) {
    for (const auto& e : errors)
        std::cerr << e.ToString() << '\n';  // 如 "/transitions/t: missing required key 'reduced'"
    return;
}

// 查表
auto dur = tokens->Duration("normal");           // 秒
auto ease = tokens->Easing("standard");
auto spring = tokens->Spring("snappy");
auto names = tokens->TransitionNames();
```

也可不用 JSON，直接用 `Track` / `Sequence` / `Parallel` 搭 `Timeline`（见 §2）。

### 1.3 创建句柄并 Play / Cancel

```cpp
constexpr TargetId kCard = 42;

AnimationHandle h = sys.Create(kCard, *tokens, "card.enter");
if (!h) {
    // 过渡名不存在 → 空句柄
    return;
}

h.OnEnd([](EndReason r) {
    // Completed / Cancelled / Superseded
});
h.OnMarker([](const std::string& name, double t) {
    // 如 "entered"
});

h.Play();
```

纯代码时间线：

```cpp
auto tl = std::make_shared<const Timeline>(
    Parallel({
        Animate(Track("opacity", 0.0).To(1.0, 0.2, Curve::Bezier(0.1, 0.9, 0.2, 1.0))),
        Animate(Track("scale", 0.96).SpringTo(1.0, SpringParams{0.3, 0.8})),
    }));

AnimationHandle h2 = sys.Create(kCard, tl /*, reduced 可选 */);
h2.Play();
```

仅打断用（无图）：

```cpp
AnimationHandle drag = sys.Create(kCard);  // 空时间线
drag.Retarget("offsetX", 120.0, SpringParams{0.3, 0.8}, /*from=*/0.0);
```

### 1.4 每帧推进（软件路径）

```cpp
void OnFrame(double dtSeconds) {
    gClock.Advance(dtSeconds);  // 或用 SteadyClock，则只需 Tick
    sys.Tick();                 // Advance 控制器 → Apply owned 值
    // 之后读 dispatcher.latest 或 sys.Current(kCard, "opacity")
}
```

> **注意**：没有 `MotionSystem::Advance(dt)`。时间由 `IClock` 提供，系统 API 是 `Tick()`。

### 1.5 Cancel / 查询

```cpp
h.Pause();
h.Seek(0.5);
h.SetRate(2.0);
h.Reverse();
h.Cancel();   // EndReason::Cancelled；终态后再 Play/Retarget 无效

if (h.Owns("opacity")) {
    auto s = h.Value("opacity");
}
auto live = sys.Current(kCard, "opacity");  // 所有者或 rest
```

### 1.6 Reduced Motion（吸附终点）

```cpp
sys.SetReducedMotion(true);
// 1) 之后 Create/Play 使用 reduced 时间线（或 instant）
// 2) 正在跑的活跃控制器立即 SnapToEnd → 终点 + Completed
// 3) Retarget 变为跳变（速度 0）

sys.SetReducedMotion(false);  // 之后新 Play 再用 full 时间线
```

句柄上**没有**公开 `SnapToEnd`；无障碍开关请走系统级 `SetReducedMotion`。

### 1.7 Retarget（推荐打断方式）

```cpp
// 从当前驱动者的值与速度接上（last-wins Claim）
bool ok = h.Retarget("scale", 1.05, SpringParams{0.25, 0.85});
if (!ok) {
    // 空句柄 / 已终态 / 无当前值且未传 from
}

// 无人驱动时必须给 from
sys.Create(kCard).Retarget("x", 10.0, SpringParams{0.3, 0.8}, 0.0);
```

放下 override：`h.Release("scale")`（若该属性仍属于时间线图，所有权保留，回到图采样）。

**已知限制 / Known limitation（R-2，本机 FAIL 结题 / 不满足 R-2）**：

**状态更新（#47，取代下文 #45 描述）**：R-2 在本机以 FAIL 结题。M1b 的合成器打断续接改为降级实现 B_v：不设 InitialValue（位置取合成器当前值），初速度取 S1 v2 影子解析速度，不再使用 WGC 速度，也不会回退到初速度 0。B_v **不满足 R-2**：
- 位置续接约落后 1 帧，跳变 J ≈ |v|×7–13 ms：约 60 ms 处打断约 17–21 DIP，约 156 ms 处约 3.5–7 DIP，仅约 300 ms 处（低速）通过；
- 速度续接在可判 4 格中达标 3（300 ms 反向打断略超）。
剩余待办只有 C5(b) 第二台机器（换刷新率）补测。需要严格连续请用 opt-in 的 `interruptible`（AppThread）。下文 hybrid B / WGC 速度描述为历史记录。

**R-3 注记（2026-10-24 Asia/Taipei，#49）**：飞行中改目标的 commit 陈旧另开 R-3。#49 叠加臂（Expression `b+d1`）相对 B_v：J/Δv 显著更优（t060 J 1.55 vs 25.86），但响应延迟 ε≈21.8 ms > 1 帧 → **不建议采用叠加**；R-3 仍 OPEN，见 [r3_commit_staleness.md](./r3_commit_staleness.md)。


**打断动画后的速度残差（R-2）**
打断进行中的动画（retarget）时，位置续接可见残差约 2–3 DIP，在一帧位移加测量底噪的范围内，肉眼一般看不出。
但打断后仍有小幅速度不连续，中位约 30–90 DIP/s，反向打断较明显（如约 60 ms 时反向打断，中位约 87）。
成因：打断速度取自屏幕捕获（WGC）的末两帧，受帧时间抖动影响。
捕获不可用或帧过旧时，自动降级为“取当前值、初速度 0”，此时会出现可见的减速再加速。
同一 tick 内连续两次 retarget 已覆盖测试，但目前只在单台 60 Hz 机器上验证过；高刷新率设备的表现待验证。
需要完全平滑衔接时，请使用 opt-in 的 interruptible 动画。
如遇异常，可关闭 flag `interrupt.hybridB`（关闭后走降级路径，不会回到旧的影子 seed）。

> 旧版「可见跳变约 207 DIP / seedMode=FromCurrent 实验默认关」描述已被取代：hybrid B 现默认开启。

**状态更新（#45，〔已被 #47 取代〕）**：M1b 以 hybrid B 条件冻结 R-2；跨机器/高刷新率确认（C5(b)）尚未完成，不在冻结范围内。已知缺陷：
- 部分时点（约 156 ms、300 ms 处打断）有真实的速度不连续；
- 打断初速度（seedV）偶有异常值（极大负值或 0，或疑似陈旧速度），根因待查（#46）。
若后续测量显示 A 路径（解析 seed）全面优于 B，将解冻重审。旧的“负 lead”seed 方案已否定，不要使用。

见 [notion_r2_b_gate_ab.md](./notion_r2_b_gate_ab.md)、[notion_r2_reopen_abc.md](./notion_r2_reopen_abc.md)；体验评估见 [gemini_r2_ux_note.md](./gemini_r2_ux_note.md)。

---

## 2. 手写时间线速查

```cpp
// TokenSet::Easing 签名：std::optional<CubicBezier> Easing(std::string_view name) const
auto emphasized = tokens->Easing("emphasized");      // 取设计师 token 文件中的曲线
Track t("x", 0.0);
t.To(100.0, 0.35, emphasized ? Curve::Bezier(*emphasized) : Curve::Linear());
t.SpringTo(80.0, SpringParams{0.4, 0.7}, /*inheritVelocity=*/true);
t.Hold(0.1);

auto root = Sequence({
    Animate(std::move(t)),
    Marker("done"),
    Delay(0.05),
});
Timeline timeline(root);
SampleMap at = timeline.Evaluate(0.2);
```

`SpringTo` 没有时长参数：段长由弹簧本身与本段跨度决定，settle 阈值随跨度缩放（位置 0.1%·跨度、速度 0.5%·跨度/s，上限 0.1 / 0.5），详见 api.md §3.2–§3.3。

> 不建议用 `Curve::FromToken("emphasized")`：它查的是 `bezier.h` 的**占位表** `kEasingTokens`，而不是 token 文件。

`Player` 适合单测或无所有权场景；**带抢占的产品路径请用 `MotionSystem`**。

---

## 3. 常见陷阱

### 3.1 Last-wins 所有权

同一 `(TargetId, property)` 上后启动的 `Play` / `Retarget` 抢走属性：

- 前一个控制器若仍在飞行且 **owned 清空** → `Superseded`，触发 `OnEnd(Superseded)`。
- 只抢走部分属性时，前一个可继续跑剩余属性。

```cpp
auto A = sys.Create(1, tlXY); A.Play();           // 拥有 x,y
auto B = sys.Create(1);
B.Retarget("x", 0, SpringParams{0.3, 0.8});       // A 失去 x，仍有 y
auto C = sys.Create(1);
C.Retarget("y", 0, SpringParams{0.3, 0.8});       // A → Superseded
```

`Play()` **不**继承速度（从时间线字面量起跳）；要接上当前值/速度请用 `Retarget`。合成器路径飞行中改目标：M1b 降级实现 B_v（IV 不设取合成器当前值 + S1 v2 影子解析速度，禁止 v=0），位置约落后 1 帧，跳变 J ≈ |v|×7–13 ms（60 ms 处打断约 17–21 DIP），**不满足 R-2**，R-2 本机 FAIL 结题（见 [notion_r2_46_ab.md](./notion_r2_46_ab.md)、artifacts/win_r2_p11_47.txt）；需要严格连续可用 `interruptible` 走 AppThread。

### 3.2 空 / 失效句柄

```cpp
AnimationHandle nh;
nh.Play();                          // no-op
CHECK(!nh);
CHECK(nh.State() == AnimationState::Cancelled);

auto missing = sys.Create(1, *tokens, "no.such");  // !missing

h.Cancel();
h.Play();                           // 终态：无效
h.Retarget("x", 1, {});             // false
```

`MotionSystem` 析构后，外部仍持有的句柄同样惰性。

### 3.3 缺失 token / 校验失败

- `Parse` 失败：看 `Diagnostic::path`（JSON Pointer）与 `message`；未知键、缺 `reduced`、环引用均会报。
- `BuildTimeline` / `Create(..., tokens, name)`：名不存在 → `nullptr` / 空句柄。
- JSON easing 引用 `"standard"` 等时，**必须在 `easing` 表定义**（与 `bezier.h` 的 `kEasingTokens` 不是自动打通的）。内置可免定义的只有 `"linear"`。

### 3.4 回调与生命周期

- `OnEnd` / `OnMarker` 挂在控制器上，句柄拷贝共享。
- 终态会清回调；`Finished` 保留（可重播）。
- **勿**在回调里按值捕获自身 `AnimationHandle` 形成环；弱捕获或结束后 `Cancel`。
- 丢光句柄不 `Cancel`：动画仍会跑完，终值进 rest（`sys.Current` 可读）。

### 3.5 Reduced / 零时长

- reduced 或 instant 时间线 `Duration()==0`：`Play` 立刻 `Completed`，**markers 不触发**。
- 中途 `SetReducedMotion(true)` 会结束所有活跃动画到终点。

---

## 4. C# / WinUI：经 C-ABI P/Invoke（软件路径，已实现）

C# 不能直接包含 C++ 头文件，桥接层为 `code/capi/fm_capi.h` / `fm_capi.cpp`（14 个 `fm_*` 函数，签名与语义见 [`api.md`](./api.md) §7）。实际接入过程（DLL 构建、dumpbin 确认 14 个导出、MotionSoftDemo 改造、打断连续性测试）见 [`workbuddy_soft_demo.md`](./workbuddy_soft_demo.md)「派题 #4」§3。该路径**不经过** `composition.h`。

### 4.1 构建原生 DLL（Windows x64，MSVC）

```text
cl /nologo /std:c++17 /EHsc /O2 /W3 /LD /I code\capi /I code /I code\explore code\capi\fm_capi.cpp /Fe:fluidmotion.dll
```

### 4.2 C# 侧（摘自 `code/capi/csharp_pinvoke_sketch.cs`）

```csharp
public sealed class MotionBridge : IDisposable
{
    readonly Fm.SinkFn _sink;        // 委托必须存为字段，保活到 fm_destroy 之后
    readonly Fm.RenderFn _render;
    readonly DispatcherQueueTimer _timer;
    readonly IntPtr _sys;

    public MotionBridge(DispatcherQueue queue, Action<ulong, string, double> onValue)
    {
        _timer = queue.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(16);
        _timer.IsRepeating = true;
        _timer.Tick += (_, __) => Fm.fm_on_frame(_sys);                       // UI 线程
        _sink = (t, p, v, u) => onValue(t, Marshal.PtrToStringUTF8(p)!, v);   // 回调内不得调用 Fm.*
        _render = (on, u) => { if (on != 0) _timer.Start(); else _timer.Stop(); };  // 静止即停
        _sys = Fm.fm_create(_sink, _render, IntPtr.Zero);
    }

    public string? LoadTokens(string json)
    {
        var err = new byte[1024];
        return Fm.fm_load_tokens(_sys, json, err, err.Length) == 0
            ? null : System.Text.Encoding.UTF8.GetString(err).TrimEnd('\0');
    }
    public ulong Create(ulong target, string? transition = null) => Fm.fm_animation_create(_sys, target, transition);
    public void Play(ulong anim) => Fm.fm_animation_play(_sys, anim);
    public bool Retarget(ulong anim, string prop, double target, double response = 0.32, double damping = 0.78)
        => Fm.fm_animation_retarget(_sys, anim, prop, target, response, damping, 0, 0.0) != 0;
    public void SetReducedMotion(bool on) => Fm.fm_set_reduced_motion(_sys, on ? 1 : 0);
    public void Dispose() { _timer.Stop(); Fm.fm_destroy(_sys); }
}

// MainWindow 接线：
//   _bridge = new MotionBridge(DispatcherQueue, (target, prop, v) => {
//       if (prop == "opacity") box.Opacity = v;        // Border.Opacity
//       else if (prop == "offsetX") xform.X = v;       // TranslateTransform.X
//   });
//   _bridge.LoadTokens(File.ReadAllText("demo_motion.json"));
//   _enter = _bridge.Create(1, "demo.enter");  _drag = _bridge.Create(1);   // 无过渡名 = 仅供 Retarget
//   按钮 Enter：_bridge.Play(_enter);
//   按钮 Move： _bridge.Retarget(_drag, "offsetX", moved ? 0 : 240);   // 可改目标并尝试接速度；勿宣传无缝飞行中改目标（R-2）
//   开关 Reduce：_bridge.SetReducedMotion(isOn);
```

`Fm` 静态类的 `[DllImport("fluidmotion", CallingConvention = Cdecl)]` 声明与类型对应见 api.md §7.5。

### 4.3 注意

- **单线程**：timer Tick 与按钮 / 指针处理器都在 UI 线程，满足 `fm_*` 同线程规则。
- **时间来源**：引擎自带 `SteadyClock`，timer 抖动（实测间隔约 31 ms）不改变运动轨迹；要 60 fps 可改用 `CompositionTarget.Rendering`。
- **唤醒**：`fm_animation_play`、`fm_animation_retarget`（成功）、`fm_set_reduced_motion` 会唤醒帧泵；`fm_animation_cancel` 不唤醒。
- **部署**：DLL 为 x64，csproj 用 `<Content Include="..\MotionNative\fluidmotion.dll" Link="fluidmotion.dll" CopyToOutputDirectory="PreserveNewest" />`，`demo_motion.json` 同理。
- **读值**：`fm_current` 返回上次 Tick 的采样（或 rest 值）；判断打断连续性请看渲染序列或用 `fm_create_manual` + `fm_manual_advance`。
- WinMD / WinUI 正式投影（`MotionAnimator` 等）仍为**规划中**；合成器路径（`composition.h` 的真实 WinRT 后端）**待真机验证**。

---

## 5. 推荐接入清单（C++）

1. 选时钟：`SteadyClock`（产品）或 `ManualClock`（测试）。
2. 实现 `IDispatcher::Apply`（软件）或先用 `NullDispatcher` + `Current`/`Value`。
3. `TokenSet::Parse`，处理全部 `Diagnostic`。
4. `MotionSystem::Create(TargetId, tokens, transition)` → 设回调 → `Play`。
5. 每帧 `Tick()`（软件路径）。
6. 交互打断用 `AnimationHandle::Retarget`（勿直接依赖 `Player::Retarget`，见 api.md §3.6 / §9.1）；取消用 `AnimationHandle::Cancel`。
7. 无障碍：`SetReducedMotion` 跟随系统动画开关（宿主侧读取方式待接 WinUI）。

---

## 6. 当前限制一览

| 项 | 状态 |
|----|------|
| 单测（core） | timeline 422 / tokens 743 / controller 236 / composition 2063，0 失败；Linux g++ 与 Windows MSVC 两平台已验证 |
| Windows Composition 下放 | 规划器 + `CompositionDispatcher` 已实现（仅模拟后端）；真实 WinRT 后端**未实现，待真机验证** |
| C# | 经 C-ABI `fm_capi.h` + P/Invoke（软件路径）**已实现**，MotionSoftDemo 已接入 |
| WinMD 投影 | **规划中 / 未实现** |
| HWND / WPF 宿主 | 首发仅**不受支持的参考示例**；`EmbedToHwnd` 正式 API 推迟 **V1.5**（`disputes.md` §1） |
| Rive 表面 | **规划中**；指针跟随延迟无法普遍消除：写为设计约束 + 分级缓解，**不承诺帧数**，实测为交付指标（`disputes.md` §3） |
| 帧预算（流水线） | 按刷新间隔 T 比例：UI≈12%、Rive 渲染≈35%、GPU≈35%，**每阶段单独** ≤T；8.3ms 是 120Hz 每阶段红线，不是可加总预算（`disputes.md` §2、`plan.md`） |
| 合成器路径飞行中改目标连续性（R-2） | **本机 FAIL 结题；M1b 以 B_v 降级实现，不满足 R-2**（不设 InitialValue，位置取合成器当前值；种子速度 = S1 v2 影子解析速度；禁止 vB=0）。#47：J 仅 t300 两格通过，t060 20.9/16.7、t156 7.0/3.5/7.0 DIP；Δv 可判 4 格达标 3；J≈\|v\|×7–13 ms（位置约落后 1 帧）。剩余仅 C5(b) 第二台机器。旧「hybrid B 条件冻结 / 跳变约 1 DIP」已作废。见 [notion_r2_46_ab.md](./notion_r2_46_ab.md)、artifacts/win_r2_p11_47.txt |
| 公开 `SnapToEnd` 句柄 API | **无**；用 `SetReducedMotion` |
| 闲置 0% CPU（合成器路径） | **估计目标**，真机未测（目标：0 计时器、CPU/GPU ≈0，≤0.1% 为 Gemini 口径） |

## Spring 参数映射（自 2026-10-20 起）
- 调用方传入设计参数 response r（秒）与阻尼比 ζ；composition.h 内部设置 Period = r/(2π)，DampingRatio = ζ。
- 不要直接把 r 写进 Period，否则屏幕上的等效 response 会变成 2π·r，约慢 6.3 倍。
- 预期屏幕轨迹：spring.h(r, ζ; t − τ)，τ ≈ 33 ms（实测 32–41 ms，随 ζ/r 略增；60Hz，WGC 到达时刻口径）。
- 打断续接：默认使用 hybrid B（WGC 末两帧速度，IV=null）。不要用 spring.h(tInt + 正 lead) 作 seed，会领先屏幕状态。
- 回归命令：--r2-p6-probe（探针）；正式验收以落地后构建复跑 R-1 v2 判据为准。

更多架构与里程碑：[`engineering.md`](./engineering.md)、[`../plan.md`](../plan.md)、[`../summary.md`](../summary.md)、[`../disputes.md`](../disputes.md)、[`deepseek_review.md`](./deepseek_review.md)。
