# Windows 桌面端动效框架 (FluidMotion Engine) 工程规范与架构设计文档

> 权威来源：`code/` 下的头文件（C++17，命名空间 `motion`）。公开接口以
> [`api.md`](./api.md) 为准，本文件是总览，不得与头文件冲突。
> 本文件 2026-11-23 按代码重写：此前版本中的 `/src/core` 路径、C++20、
> `FluidMotion` 命名空间及 `SpringSolver` / `MotionAnimator` / `AnimateProperty`
> 等伪 API 均不存在，已删除；`motion.json` schema 一节按 `tokens.h` v1 重写。
> 接入步骤见 [`usage.md`](./usage.md)。

---

## 一、项目概述与目标 / 非目标

### 1.1 项目概述

`FluidMotion Engine` 是为 Windows 桌面端（首要宿主 WinUI 3）打造的嵌入式动效与
微交互引擎：设计端产出 motion token，引擎把声明式时间线（弹簧、贝塞尔、顺序 /
并行 / 交错、标记）求值，优先下放给合成器动画，降级时在应用线程用同一套数学
逐帧求值。当前已落地的只有软件路径；合成器路径只有规划器，真实后端不存在
（见 §5 与 §9）。

### 1.2 核心目标（计划值，待真机验证）

* **流畅**：支持 60/75Hz 以上、高刷与 VRR；常规动效下放到合成器线程后，宿主
  主线程卡住时动效不受影响。合成器路径尚未接线，该目标目前只对软件路径的
  数学正确性有单测覆盖。
* **低闲置开销**：无动画时不存在已调度的工作（`FramePump` 在 `ActiveCount() == 0`
  时停泵，`render(0)` 通知宿主退订帧源）。CPU/GPU 占用预算见 §7，均为估计、
  未在参考机上标定。
* **设计开发一体化**：Figma 产出 motion token JSON，`TokenSet::Parse` 严格校验后
  编译为 `Timeline`。Figma 插件、热重载预览宿主尚未实现。
* **开发友好**：C++17 头文件核心 + C-ABI（`fm_capi.h`，14 个导出函数）+ C#
  P/Invoke（`MotionSoftDemo/MotionSystem.cs`）。WinMD 投影尚未实现。

### 1.3 明确非目标

* **不做跨平台 UI 渲染**：深度绑定 Windows 桌面生态。
* **首发不提供正式 WPF 支持**：受支持的 `EmbedToHwnd` 推迟到 V1.5（见
  `disputes.md` §1）；首发只给不受支持的 HWND 参考宿主示例，不提供正式 API。
  当前代码里连参考宿主示例也未作为正式 API 交付，勿当作已有能力。
* **不自研 2D 光栅化引擎**：复杂矢量交互插画计划依赖 Rive C++ Runtime；Rive
  集成尚未实现。

---

## 二、架构总览

```text
[ Figma 设计端 ] ──► motion token JSON ──► TokenSet::Parse ──► Timeline
                                                              │
[ MotionSystem ] ──► AnimationHandle ──► IDispatcher ──┼──► CompositionDispatcher
                                                              │      （规划器已实现，
                                                              │       真实 WinRT 后端不存在）
                                                              └──► SoftwareDispatcher
                                                                     + FramePump
                                                                     （应用线程逐帧求值）
C# 接入：fm_capi.h（C-ABI，软件路径）──► MotionSystem.cs（FluidMotion.L1）
```

### 2.1 核心组件分层

1. **Core Math & Timeline（C++17，`code/`）**：纯数学计算层，弹簧闭式解、贝塞尔
   求值、时间线状态机及重定向。无 OS 依赖（`SteadyClock` 仅用 `std::chrono`）。
2. **控制器（`controller.h`）**：`MotionSystem` 拥有时钟引用、reduced-motion 开关、
   属性所有权表与运行中的控制器集合；`AnimationHandle` 是控制器的可拷贝门面；
   应用每帧调一次 `MotionSystem::Tick()`。
3. **分发缝（`IDispatcher`）**：`Submit` / `Withdraw` / `Apply` 三个方法，见 §5。
   能力探测与 lowering 规划在 `composition.h`；软件实现在
   `code/explore/software_dispatcher.h`。
4. **Token 管线（`tokens.h`）**：手写严格 JSON 解析器 + 编译器，把 token 文件
   编译为不可变的 `Timeline`；`reduced` 变体是 schema 的强制部分。
5. **C-ABI 与 C#（`code/capi/`、`MotionSoftDemo/MotionSystem.cs`）**：软件路径的
   唯一对外 ABI；`FluidMotion.L1` 0.1.0 是内部候选包，未发布。

---

## 三、模块说明与目录约定

### 3.1 源码目录结构（实际布局）

```text
code/
  ├── spring.h              # 闭式阻尼弹簧
  ├── bezier.h              # 三次贝塞尔 + 内置缓动占位表
  ├── timeline.h            # 时钟、轨道、组合节点、播放器
  ├── tokens.h              # token schema v1、校验器、加载器
  ├── controller.h          # MotionSystem、AnimationHandle、IDispatcher
  ├── composition.h         # 能力探测、lowering 规划器、CompositionDispatcher
  ├── motion.json           # schema v1 示例（全部数值为占位）
  ├── capi/                 # fm_capi.h / fm_capi.cpp / fm_capi_test.cpp
  └── explore/              # software_dispatcher.h（SoftwareDispatcher + FramePump）
src/FluidMotion.L1/         # .NET 8 内部候选包工程（FluidMotion.L1.csproj + README.md）
MotionSoftDemo/             # WinUI 演示工程；MotionSystem.cs / MotionEtw.cs 是 L1 公开面来源
tools/delivery/             # build_component.py / validate_native.py（L1 打包验证）
tests/delivery/Consumer/    # L1 独立 consumer 验收工程
docs/                       # api.md（权威）、usage.md、engineering.md（本文件）、评审稿与闸门材料
```

注意：不存在 `/src/core`、`/src/rendering`、`/src/bindings`、`/src/host`、
`tools/figma-plugin` 这些目录；`Player::Retarget` 在头文件里仍然存在，但应用层
应走 `AnimationHandle::Retarget`（头文件未加弃用标记，见审核 MF-09，待处理）。

### 3.2 当前代码落地状态

* **已实现**：`spring.h`、`bezier.h`、`timeline.h`、`tokens.h`、`controller.h`、
  `composition.h`（规划器与分发器，仅模拟后端验证）、`fm_capi.h`（软件路径）。
* **测试数引用**：`api.md` §10 记载 timeline 422、tokens 743、controller 236、
  composition 2063、capi 23 项检查，均为文档自述；本文件不重复断言，复验前
  以 `api.md` §10 与各测试源文件为准。
* **未实现**：真实 WinRT 合成器后端、Rive、WinMD 投影、Figma 插件、预览宿主、
  性能 CI（PERF-CI HOLD，见 `docs/z6_closed.md`）。

---

## 四、控制器与重定向架构

### 4.1 控制器与句柄 API（`controller.h`，以头文件为准）

```cpp
namespace motion {

using TargetId = std::uint64_t;
using ControllerId = std::uint64_t;

enum class AnimationState { Idle, Playing, Paused, Finished, Cancelled, Superseded };
enum class EndReason { Completed, Cancelled, Superseded };

class MotionSystem {
public:
    explicit MotionSystem(const IClock& clock, IDispatcher* dispatcher = nullptr);
    bool ReducedMotion() const;
    void SetReducedMotion(bool on);
    AnimationHandle Create(TargetId target,
                           std::shared_ptr<const Timeline> full = nullptr,
                           std::shared_ptr<const Timeline> reduced = nullptr);
    AnimationHandle Create(TargetId target, const TokenSet& tokens,
                           std::string_view transition);  // 过渡不存在 → 空句柄
    void Tick();  // 每帧调用一次：推进活跃控制器并 Apply owned 值
    std::optional<Sample> Current(TargetId target, std::string_view property) const;
    std::size_t ActiveCount() const;
};

class AnimationHandle {
public:
    void Play();
    void Pause();
    void Seek(double t);
    void Reverse();
    void SetRate(double r);
    void SetLoops(int n);  // n < 0：无限
    void Cancel();
    bool Retarget(const std::string& property, double newTarget, SpringParams spring,
                  std::optional<double> from = std::nullopt);
    void Release(const std::string& property);
    AnimationState State() const;
    // ……
};

} // namespace motion
```

以下方法**不存在**，勿引用：`MotionSystem::Advance(dt)`、`MotionSystem::AnimateProperty`、
`AnimationHandle::SnapToEnd`（公开面）、`SetReducedMotionEnabled`。吸附终点由
`MotionSystem::SetReducedMotion(true)` 对活跃控制器在内部完成；句柄晚于
`MotionSystem` 析构则变为惰性（`State() == Cancelled`，调用 no-op）。

### 4.2 属性所有权与重定向规则

1. **最后胜出（Last-Wins）**：每个 `(target, property)` 同一时刻仅一个所有者。
   `Play` / `Seek` / `Reverse` 声明时间线属性、或 `Retarget` 某属性时 Claim；
   前所有者立即失去该属性，对其余仍拥有的属性可继续跑。
2. **飞行中丢光属性 → `Superseded`**：Playing / Paused / 弹簧未 settle 且 owned
   变为空的控制器进入终态 `Superseded`；空闲或已 Finished 的控制器丢属性后仍
   可复用，再次 `Play()` 会重新 Claim。
3. **保留速度重定向**：`Retarget` 从当前所有者的值与速度起步（bezier 段经
   `CubicBezier::Slope`，弹簧段经解析状态，override 经自身状态）；无人驱动且无
   rest 值时需 `from` 种子，否则返回 `false`。`Play()` 从时间线字面量起步，
   故意不连续。
4. **生命周期**：活跃控制器由系统 `shared_ptr` 保活，丢光句柄未 `Cancel` 仍会
   跑完，终值写入 rest（`Current` 可读）。终态（Cancelled / Superseded）清空用户
   回调；Finished 保留回调（可重播），回调按值捕获自身句柄会成环，须弱捕获或
   用完后 `Cancel`。

### 4.3 减少动效（Reduced Motion）

* `SetReducedMotion(true)`：所有活跃控制器 `SnapToEnd`（内部方法），正向到终点、
  反向到 0，并以 `Completed` 结束。
* `Play` / `Reverse` / `Seek` 使用 reduced 时间线（`TokenSet` 提供，或对 full 做
  instant 推导：每个被动画属性在 t=0 跳到终值）；零时长时间线在 `Play` 时立即
  完成，跳过 markers。
* `Retarget` 在 reduced 模式下跳到目标、速度为 0，不起弹簧。
* 系统开关的接入（`UISettings.AnimationsEnabled`）由宿主负责调用
  `SetReducedMotion`，核心不直接读系统设置。

---

## 五、`IDispatcher` 与 lowering

```cpp
namespace motion {

struct ControllerSnapshot {
    ControllerId id = 0;
    TargetId target = 0;
    std::shared_ptr<const Timeline> timeline;
    double position = 0;
    double rate = 1;
    bool reversed = false, playing = false;
    int loops = 1;  // < 0：无限
    double clockTime = 0;
    std::vector<SpringOverrideInfo> overrides;
    std::vector<std::string> ownedProperties;
};

class IDispatcher {
public:
    virtual ~IDispatcher() = default;
    virtual void Submit(const ControllerSnapshot&) = 0;
    virtual void Withdraw(ControllerId, EndReason) = 0;
    virtual void Apply(TargetId, const SampleMap&) = 0;
};

} // namespace motion
```

* **调用时机**：`Submit` 在每个间断点（play、pause、seek、reverse、rate/loop
  变化、retarget、所有权变化、reduced-motion 吸附、`Release` 掉 override）调用；
  `Withdraw` 仅在控制器此前 `Submit` 过且尚未撤回时调用；`Apply` 只在 `Tick()`
  内对活跃集调用，每 target 每 Tick 至多一次。
* **`Withdraw(Completed)` 不只表示播完**：暂停姿态、Seek 后停顿、override 弹簧
  settle 后退出活跃集时也会收到 `Withdraw(Completed)`，即“变为空闲”。合成器
  后端不得把该信号一律理解为“播到终值”（`CompositionDispatcher` 靠 Hold 计划
  区分，见 `composition.h`）。
* **规划器规则（v1，`composition.h`）**：图属性降到合成器须同时满足有映射、
  playing、loops == 1、单轨驱动、无零时长段、全 linear/bezier/hold 段或恰一个
  非反向 spring 段、bezier y 越界须能力允许、关键帧数在限额内；override 优先于
  图属性，已 settle 的 override 是 Hold；其余回退 AppThread 并记录原因。
* **合成器假设 S1–S5、K1–K5 均未全部验证**：其中 S1（`Period = response/(2π)`）
  与 K2（bezier y 越界）按头文件注释称有真机证据（#36/#41、派题 #20），其余
  仍待模块 7 真机打勾。`MapSpringToCompositor` 与 `SpringMapping` 是换算的唯一
  落点，勿在别处重复换算。
* **软件路径**：`SoftwareDispatcher` 只转发 `Apply`，忽略 `Submit` / `Withdraw`；
  `FramePump` 负责按需启停帧源，`MotionSystem` 没有唤醒钩子，shim 须在任何
  可能启动或改变运动的调用后调 `Wake()`（见 `fm_capi.cpp` 与
  `software_dispatcher.h` 的 CONTRACT 注释）。`CompositionTarget.Rendering`
  一类的帧源由宿主提供，核心不绑定。

---

## 六、API 使用示例（以头文件为准）

### 6.1 C++ 核心求值示例

```cpp
#include "spring.h"
#include "timeline.h"

namespace motion {

// 闭式弹簧：从 0 到 100，response 为无阻尼周期（秒），damping 为阻尼比。
Spring spring(SpringParams{0.35, 0.8}, /*from=*/0.0, /*target=*/100.0);
SpringState at01 = spring.At(0.1);  // {value, velocity}，t 自 (re)start 起

// 单属性轨道：先 0.2s 线性到 1，再弹簧回到 0（时长由 settle 上界推导）。
Track track("opacity", 0.0);
track.To(1.0, 0.2).SpringTo(0.0, SpringParams{0.35, 0.8});
Sample sample = track.Eval(0.1);

// 时间线 + 播放器：播放器由一条线程拥有，每帧调 Update()。
ManualClock clock;
auto timeline = std::make_shared<const Timeline>(Animate(std::move(track)));
Player player(timeline, clock);
player.Play();

} // namespace motion
```

`Spring::Retarget(t, newTarget)` 存在于弹簧层，但 At() 的计时原点不随 Retarget
移动；应用层打断改目标请用 `AnimationHandle::Retarget`。

### 6.2 C# 接入示例（L1 面，`MotionSystem.cs`）

```csharp
using MotionSoftDemo;

// 系统与调用线程绑定；帧源由宿主在 FramePumpChanged 里启停，并调 OnFrame。
var system = MotionSystem.Create();
system.ValueChanged += (target, property, value) => { /* 写回 UI 属性 */ };
system.FramePumpChanged += on => { /* on 为 true 时开始调 system.OnFrame() */ };

string? diagnostics = system.LoadTokens(json);  // null = 成功，否则为诊断文本
var handle = system.CreateAnimation(targetId, "card.enter");
handle.Play();
bool ok = handle.Retarget("offsetX", 240.0, new SpringParams(0.32, 0.78));
system.Dispose();
```

C# 侧的 P/Invoke、委托保活与“回调内不得调 `fm_*`”规则见 `MotionSystem.cs`
文件头注释与 `docs/api.md` §7。演示工程 `MotionSoftDemo/MainPage.xaml.cs`
目前走的是另一套旧桥 `MotionBridge`（`FmBridge.cs`），与 L1 面的线程与销毁
语义不一致，勿把演示写法当作 L1 接入范式。MF-04 的第一步已做：
`MotionBridge` 已标记 `[Obsolete]` 并冻结（只保留与 `MotionSystem` 的对齐修正：
全构建线程检查、`fm_animation_release`、4096 字节诊断缓冲、具名重定向默认值、
扩展的 `FmProp` 分类）；把演示迁到 L1 面仍待办，须在 Windows 构建上验证后才做。

---

## 七、Motion token schema v1

权威定义在 `code/tokens.h` 文件头注释与 `docs/api.md` §4；示例在
`code/motion.json`（全部数值为占位，设计师拥有最终词表）。

```json
{
  "version": 1,
  "durations": { "fast": "100ms", "normal": "200ms", "default": "normal" },
  "easing": { "standard": [0.33, 0.0, 0.67, 1.0], "enter": "decelerate" },
  "springs": { "snappy": { "response": "300ms", "damping": 0.8 } },
  "transitions": {
    "card.enter": {
      "root": { "type": "track", "property": "opacity", "from": 0,
                "steps": [{ "to": 1, "duration": "default", "ease": "enter" }] },
      "reduced": "instant"
    }
  }
}
```

规则摘要：顶层严格（未知键即错），仅 `$schema` / `$comment` 可选且忽略；
`version` 必需且仅支持 `1`；时长必须是带单位的字面量（`"120ms"` / `"0.3s"`）
或时长 token 名，裸数字拒绝，范围 `[0, 600]` 秒；easing 为 `[x1, y1, x2, y2]`
（x 在 `[0, 1]`）或 easing token 名，内置名仅 `"linear"`；spring 的 response 在
`[0.001, 10]` 秒、damping 在 `[0, 100]`；token 名匹配
`[A-Za-z][A-Za-z0-9_.-]*`，别名链允许、环为错误；节点类型为 track / sequence /
parallel / stagger / delay / marker，嵌套深度上限 32；每个 transition 的
`reduced` 必需，`"instant"` 由 root 推导、不得手写。

---

## 八、性能与测试要求

### 8.1 帧预算分配（按刷新间隔比例，估计）

* **UI / 宿主业务线程中框架部分**：约 12%（120Hz 下稳态 ≤1ms，启动 ≤2ms）。
* **Rive 渲染线程**：约 35%（120Hz 下 ≤3ms）。Rive 尚未接入，该预算未验证。
* **GPU**：约 35%（120Hz 下 ≤3ms）。未验证。
* **红线**：8.3ms 是每个阶段的失败红线，不是可用满的预算（见 `disputes.md` §2）。

### 8.2 测试与验证门槛

* **单元测试**：弹簧与 RK4 对照、重定向前后位置与速度连续、阻尼比扫过 1.0；
  贝塞尔与二分法基准对照、单调性、斜率与有限差分对照；token 校验；时间线
  跳转确定性。测试源文件在 `code/*_tests.cpp`，计数见 §3.2 的引用说明。
* **黄金帧**：Rive 内容固定时钟渲染、按 GPU 厂商存基线带容差比较；合成器动画
  在实验室机器上屏幕录制校验。两者均未落地。
* **性能 CI**：专用物理机、ETW + PresentMon / PIX、P95 同时卡绝对上限与相对
  基线回退、P99 以每千帧丢帧率作门禁（初期只告警）。PERF-CI 继续 HOLD。
* **多屏与 DPI 矩阵**：混合刷新率、100%/150%/200% 缩放、跨屏拖动、电池模式
  动态刷新率、RDP、GPU 切换、热插拔。未落地。
* **无障碍**：API 强制要求提供减少动效替代方案（schema 强制 `reduced`），用
  Narrator、键盘、高对比度检查。检查本身未落地。
* **闲置测试**：断言无动画时不存在已调度的工作（`FramePump` 停泵）。

---

## 九、技术验证与实施清单

计划总周期约 30–34 周、约 6 名全职（`plan.md`，均为模型估计，技术验证后
重新核算）。里程碑与当前状态：

* **M0（第 2 周）技术验证**：Rive 接入合成表面、75Hz 拖动错位测试、指针属性
  集、ALLOW_TEARING、实测管线延迟。未完成；M0 的各项结论仍待确认。
* **M1（第 10 周）核心**：数学核心、时间线、token 可无界面运行并通过单元测试；
  分发器把弹簧与贝塞尔降级为合成器动画。数学与 token 部分已实现；分发器
  只有规划器（模拟后端），真机弹簧参数对应关系未验证。
* **M2（第 16 周）Alpha**：C# API 可用、可打断重定向、ETW 可用、预览宿主 v0、
  性能 CI 在 2 台物理机上运行。C# 软件路径与 ETW 已有；预览宿主与性能 CI 未有。
* **M3（第 22 周）Rive 层 + 首个试点**：Rive 表面 DPI 与设备丢失恢复、跟随部件
  拆分、混合刷新率矩阵、试点 1 接入。未开始。
* **M4（第 28 周）Beta**：试点 1 接入完成、参考机 P95/P99 达标、无障碍检查、
  文档齐备、HWND 参考宿主冒烟。未开始。
* **M5（第 32–34 周）1.0**：试点 2 接入完成、无未关闭 P0/P1、完整矩阵达标。
  未开始。

---

## 十、已知限制与待真机验证项

1. **合成器对接未验证**：`spring.h`、`bezier.h`、`timeline.h`、`tokens.h`、
   `controller.h`、`composition.h` 均未在真实 `Windows.UI.Composition` 后端挂载；
   `CubicBezierEasingFunction` 是否接受 y 超出 `[0, 1]`、`SpringVectorNaturalMotionAnimation`
   的 Period/DampingRatio 语义、合成器实际输出与求解器吻合度，均待模块 7 验证
   （S1/K2 除外，见 §5）。
2. **Rive 未接入**：指针跟随延迟、DPI 重新光栅化、设备丢失恢复均无代码；0.5–1.5
   帧采样延迟的说法是设计约束（`disputes.md` §3），不是实测结论。
3. **HWND 宿主**：正式 API 推迟到 V1.5；当前无可用的参考宿主实现。
4. **遥测占位**：`PresentStats` 当前 emitter 传占位值（dropped=0、intervalMs=16.666），
   不是实测，不得读取其字段作时间/FPS 结论（见 `MotionEtw.cs` 注释）；`FrameTick`
   的 activeCount 在 L1 面传的是运行布尔值，不是计数。待 MF-06 处理。
5. **性能与无障碍**：§8 的全部预算、矩阵与检查均未在参考机上跑过；`docs/z6_closed.md`
   的 PASS 均为 dev-only，不代表性能合格或产品采用。
