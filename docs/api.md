# FluidMotion 公开 API 说明

> **范围**：本文档只描述 `code/` 下已实现的 C++17 头文件公开接口。  
> **命名空间**：全部类型与函数位于 `motion`。  
> **状态标注**：已实现 / 规划中（未实现）/ 估计 / 待真机验证。  
> **权威来源**：`code/spring.h`、`code/bezier.h`、`code/timeline.h`、`code/tokens.h`、`code/controller.h`、`code/composition.h`、`code/motion.json`、`code/capi/fm_capi.h`（及 `code/explore/software_dispatcher.h`）。  
> `composition.h`：规划器与 `CompositionDispatcher` **已实现（仅模拟后端验证）**，真实 WinRT 合成器后端**不存在**（§6）。  
> C# 接入：经 C-ABI `fm_capi.h` + P/Invoke **已实现（软件路径）**（§7）；WinMD / WinUI 投影：**规划中，当前代码中不存在**。

---

## 0. 阅读约定

| 标记 | 含义 |
|------|------|
| 已实现 | 头文件中有定义，单元测试覆盖（Linux g++ 与 Windows MSVC 均已跑通，见 §10） |
| 应用层应优先使用 | 对 App 暴露的推荐入口 |
| 底层 / 可弃用 | 仍在头文件中，但工程文档标明应用层应走 `AnimationHandle` |
| 规划中 / 未实现 | 产品计划中有，但 `code/` 无对应 API |
| 待真机验证 | 行为在单测通过，Windows 真实合成器后端未接线 |

**线程规则（来自头文件注释）**：`timeline.h` / `controller.h` **均非线程安全**。`Timeline` 构造后不可变、可共享；`Player` / `MotionSystem` / `AnimationHandle` 操作须在同一条 UI dispatcher 线程上。

**单位**：时间用秒（`double`）；数值单位为 DIP 或任意标量（由调用方约定）。

---

## 1. `spring.h` — 闭式阻尼弹簧

纯数学，无 OS 依赖。

### 1.1 `SpringParams`

```cpp
struct SpringParams {
    double response = 0.35;  // 无阻尼周期（秒）；由 Spring 构造时钳制为 >= 1e-4
    double damping  = 0.8;   // 阻尼比；由 Spring 构造时钳制为 >= 0（负值会发散）
};
```

| 字段 | 含义 |
|------|------|
| `response` | 无阻尼周期（秒） |
| `damping` | 阻尼比：`< 1` 欠阻尼，`≈ 1` 临界，`> 1` 过阻尼 |

### 1.2 `SpringState`

```cpp
struct SpringState { double value; double velocity; };
```

### 1.3 `Spring`

```cpp
class Spring {
public:
    Spring(SpringParams p, double from, double target, double velocity = 0.0);

    SpringState At(double t) const;           // t >= 0，自上次 (re)start 起
    void Retarget(double t, double newTarget); // 从当前状态改目标；优先用时间线/控制器跟踪 t

    // O(1) 包络上界：|位移|<posEps 且 |速度|<velEps 之后的时间上界（可能多约一个周期）
    // 近似无阻尼时永不 settle，返回 cap
    double SettleTime(double posEps = 0.1, double velEps = 0.5, double cap = 10.0) const;

    // 固定步长扫描；仅作测试 oracle
    double SettleTimeScan(double posEps = 0.1, double velEps = 0.5,
                          double step = 1.0/240.0, double cap = 10.0) const;

    double Target() const;
    const SpringParams& Params() const;
};
```

**说明**：`Retarget` 在弹簧层存在；应用层打断改目标请用 `AnimationHandle::Retarget`（见 §5）。

---

## 2. `bezier.h` — CSS 兼容三次贝塞尔

### 2.1 `CubicBezier`

```cpp
class CubicBezier {
public:
    constexpr CubicBezier(double x1, double y1, double x2, double y2);

    // x1,x2 须在 [0,1]；y 可为任意有限值
    static bool IsValid(double x1, double y1, double x2, double y2);

    // 原样返回构造时的控制点（模块 6 交给合成器的缓动函数）
    constexpr double X1() const;
    constexpr double Y1() const;
    constexpr double X2() const;
    constexpr double Y2() const;
    constexpr bool IsLinear() const;   // 构造时判定：x1 == y1 且 x2 == y2

    double Ease(double x) const;   // x<=0（含 NaN）→0，x>=1→1；IsLinear 时直接返回 x
    double Slope(double x) const;  // dy/dx；IsLinear 时为 1
};
```

- `X1()/Y1()/X2()/Y2()` 返回**未经钳制**的构造参数（钳制发生在 `Curve::Bezier(x1,y1,x2,y2)`，见 §3.2）。
- `IsLinear()` 为真时 `Ease` / `Slope` 走直线快路径；`composition.h` 规划器据此把该段当线性关键帧，并用 `X1..Y2` 填 `KeyframeSpec`（§6.3）。

### 2.2 内置缓动表（占位）

```cpp
struct EasingToken { std::string_view name; CubicBezier curve; };

inline constexpr std::array<EasingToken, 5> kEasingTokens{{
    {"linear",     CubicBezier(0.0, 0.0, 1.0, 1.0)},
    {"standard",   CubicBezier(0.33, 0.0, 0.67, 1.0)},
    {"emphasized", CubicBezier(0.2, 0.0, 0.0, 1.0)},
    {"decelerate", CubicBezier(0.1, 0.9, 0.2, 1.0)},
    {"accelerate", CubicBezier(0.9, 0.1, 1.0, 0.2)},
}};

inline const CubicBezier* FindEasing(std::string_view name);  // 未找到返回 nullptr
```

> **注意**：注释标明数值为 **PLACEHOLDER**，最终以 token 文件为准。`TokenSet` 的 easing 与本表是两套来源；JSON 加载器内置名仅保证 `"linear"`（见 `tokens.h`）。

---

## 3. `timeline.h` — 时钟、轨道、组合节点、播放器

### 3.1 时钟

```cpp
struct IClock {
    virtual ~IClock() = default;
    virtual double Now() const = 0;  // 秒，单调非递减
};

class SteadyClock final : public IClock;           // 相对构造时刻的 steady_clock
class ManualClock final : public IClock;           // 测试：Set / Advance
class FixedStepClock final : public IClock;        // 测试：Now = frame * step；Tick(n)
```

`ManualClock`：`void Set(double t)`；`void Advance(double dt)`（仅 `dt > 0` 推进）。  
`FixedStepClock`：`explicit FixedStepClock(double step = 1.0/120.0)`；`void Tick(std::int64_t n = 1)`；`Frame()` / `Step()`。

### 3.2 `Curve` / `Sample`

```cpp
struct Curve {
    enum class Kind { Linear, Bezier, Spring };
    Kind kind = Kind::Linear;
    CubicBezier bezier{0.0, 0.0, 1.0, 1.0};
    SpringParams spring{};

    static Curve Linear();
    static Curve Bezier(double x1, double y1, double x2, double y2);  // x 钳制到 [0,1]
    static Curve Bezier(const CubicBezier& b);
    static Curve FromSpring(SpringParams p);
    static std::optional<Curve> FromToken(std::string_view name);  // 查 FindEasing；未知 nullopt
};

struct Sample {
    double value = 0.0;
    double velocity = 0.0;  // d(value)/d(time)，所用时间域见求值处注释
};

// 弹簧段的 settle 阈值：以下两个常量是【上限】（DIP / DIP·s⁻¹，适合 ~100 单位的位移）
inline constexpr double kSpringPosEps = 0.1;
inline constexpr double kSpringVelEps = 0.5;
inline double SpringPosEps(double span);   // clamp(0.001 * |span|, 1e-4, kSpringPosEps)
inline double SpringVelEps(double span);   // clamp(0.005 * |span|, 1e-3, kSpringVelEps)
// from → target 的 O(1) 解析 settle 上界：
// s.SettleTime(SpringPosEps(|from-target|), SpringVelEps(|from-target|))
inline double SpringSettleTime(const Spring& s, double from, double target);
```

**阈值随跨度缩放**：位置阈值 = 跨度的 0.1%，速度阈值 = 跨度的 0.5%/s，下限分别为 `1e-4`、`1e-3`，上限为 `kSpringPosEps`、`kSpringVelEps`。无量纲属性（`scale`、`opacity`，跨度≈1）因此用小得多的阈值；头文件注释记录了修复前 0.96→1 的 scale 弹簧约 0.06 s 就被判定 settle、末尾 1–2% 被吸附的问题。

### 3.3 `Track`（单属性关键帧轨道）

```cpp
class Track {
public:
    struct Segment {
        double start, duration, from, to, entryVelocity;
        Curve curve;
    };

    Track(std::string property, double from);

    // Linear/Bezier 定长段；duration<=0 为瞬时跳变；若 curve 为 Spring 则转 SpringTo（忽略 duration 实参）
    Track& To(double value, double duration, Curve curve = Curve::Linear());

    // 弹簧段（无 duration 参数）：以本段跨度 |value - 上一段终值| 计算相对 settle 阈值
    // SpringPosEps(span) / SpringVelEps(span)（见 §3.2）；inheritVelocity 时以上一段出口速度起步；
    // 段末值吸附到 value，吸附误差按构造小于 SpringPosEps(span)
    Track& SpringTo(double value, SpringParams p, bool inheritVelocity = false);

    Track& Hold(double duration);

    const std::string& Property() const;
    double Duration() const;
    double InitialValue() const;
    double FinalValue() const;
    const std::vector<Segment>& Segments() const;

    Sample Eval(double t) const;   // 局部时间秒；t<0 或 NaN → 初值
    double EndVelocity() const;
};
```

行为要点：`t < 0` 保持初值；超过终点保持终值且速度为 0。

### 3.4 组合节点

```cpp
using SampleMap = std::map<std::string, Sample>;  // 有序，迭代确定

struct MarkerEvent { std::string name; double time; };
struct PlacedTrack { double start; const Track* track; };  // 指针在 Timeline 存活期内有效

class Node {
public:
    virtual ~Node() = default;
    virtual double Duration() const = 0;
    virtual void Evaluate(double t, SampleMap& out) const = 0;
    virtual void CollectMarkers(double offset, std::vector<MarkerEvent>&) const;
    virtual void CollectTracks(double offset, std::vector<PlacedTrack>&) const;
};
using NodePtr = std::shared_ptr<const Node>;
```

**属性冲突规则（注释原文语义）**：同一属性被多条子轨驱动时——已开始的子节点（`t >= start`）覆盖更早子节点并保持终值；尚未开始的子节点仅在无人写入该属性时贡献初值。

工厂函数：

```cpp
NodePtr Animate(Track t);
NodePtr Delay(double seconds);
NodePtr Marker(std::string name);
NodePtr Sequence(std::vector<NodePtr> c);
NodePtr Parallel(std::vector<NodePtr> c);
NodePtr Stagger(double delayPerChild, std::vector<NodePtr> c);
```

具体节点类型：`TrackNode`、`DelayNode`、`MarkerNode`、`GroupNode`（`Kind::Sequence|Parallel|Stagger`）。

### 3.5 `Timeline`

不可变；构造后可共享。

```cpp
class Timeline {
public:
    explicit Timeline(NodePtr root);  // root 为空则等价 Delay(0)

    double Duration() const;
    const NodePtr& Root() const;
    const std::vector<MarkerEvent>& Markers() const;   // 按时间排序
    const std::vector<PlacedTrack>& Tracks() const;    // 供 lowering
    std::optional<double> MarkerTime(std::string_view name) const;

    // 纯函数；t 钳到 [0, Duration()]；NaN→0；速度单位为时间线秒
    SampleMap Evaluate(double t) const;
};
```

### 3.6 `Player`

由一条线程拥有（UI dispatcher）。

```cpp
class Player {
public:
    using CompleteCallback = std::function<void()>;
    using MarkerCallback = std::function<void(const std::string& name, double timelineTime)>;

    Player(std::shared_ptr<const Timeline> tl, const IClock& clock);

    void OnComplete(CompleteCallback cb);
    void OnMarker(MarkerCallback cb);

    void Play();
    void Pause();
    void Seek(double t);          // 不触发回调；Seek 两次 == 一次
    void Reverse();               // 翻转方向并 Play（类 Web Animations）
    void SetRate(double r);       // 仅 r>0 且有限
    void SetLoops(int n);         // n==0 视为 1；n<0 无限循环
    void Update();                // 推进到 clock.Now()；每帧调用一次

    bool IsPlaying() const;
    bool IsFinished() const;
    bool IsReversed() const;
    double Position() const;
    double Rate() const;

    // 上次 Update 后的值；速度 = 时间线速度 × rate × 方向；未播放时速度为 0；override 优先
    SampleMap Sample() const;

    // 【底层 / 应用层弃用倾向】工程文档：打断应走 AnimationHandle::Retarget
    // 仍存在于头文件：从当前值与实时速度弹簧改目标；属性既无动画也无 override 时需 from，否则 false
    bool Retarget(const std::string& property, double target, SpringParams params,
                  std::optional<double> from = std::nullopt);
    void ReleaseOverride(const std::string& property);
    void ReleaseAllOverrides();
};
```

> **`Player::Retarget` 状态**：头文件**未**写 `[[deprecated]]`，但 `docs/engineering.md` / 计划中明确：应用层重定向统一收拢到 `MotionSystem` / `AnimationHandle`。文档建议：**应用代码不要直接依赖 `Player::Retarget`**。

---

## 4. `tokens.h` + `motion.json` — Token schema v1

公开面：`Diagnostic`、`TokenSet`（`Parse`、查找、`BuildTimeline`）。`motion::detail`（JSON 解析器、Compiler）为内部实现。

### 4.1 `Diagnostic`

```cpp
struct Diagnostic {
    std::string path;     // JSON Pointer；"" = 文档根
    std::string message;
    std::string ToString() const;  // "/path: message" 或 "/: message"
};
```

### 4.2 `TokenSet`

```cpp
class TokenSet {
public:
    // 失败返回 nullopt，errors 填入全部问题（先 clear）
    static std::optional<TokenSet> Parse(std::string_view json, std::vector<Diagnostic>& errors);

    int Version() const;

    std::optional<double> Duration(std::string_view name) const;
    std::optional<CubicBezier> Easing(std::string_view name) const;  // 另：未表内时 "linear" 内置
    std::optional<SpringParams> Spring(std::string_view name) const;

    std::vector<std::string> TransitionNames() const;
    bool HasTransition(std::string_view name) const;

    // 不可变可共享；不存在返回 nullptr；reducedMotion 选 reduced 变体
    std::shared_ptr<const Timeline> BuildTimeline(std::string_view name,
                                                  bool reducedMotion = false) const;
};
```

### 4.3 Schema v1 摘要（严格：未知键即错误）

顶层：

| 键 | 要求 |
|----|------|
| `version` | 必需，仅支持 `1` |
| `$schema` / `$comment` | 可选，忽略（仅顶层） |
| `durations` | `name → "120ms"\|"0.3s"\|时长名`；裸数字拒绝；范围 `[0, 600]` 秒 |
| `easing` | `name → [x1,y1,x2,y2]\|缓动名`；x∈[0,1]；内置名仅 `"linear"` |
| `springs` | `name → {response, damping}\|弹簧名`；response∈[0.001,10]s，damping∈[0,100] |
| `transitions` | `name → { root: <node>, reduced: <node>\|"instant" }`；**reduced 必需** |

Token 名：`[A-Za-z][A-Za-z0-9_.-]*`；允许别名链，环为错误。

`<node>`（带 `"type"`）：

| type | 字段 |
|------|------|
| `track` | `property`, `from`, `steps`（非空） |
| step `to` | `to`, `duration`, 可选 `ease` |
| step `springTo` | `springTo`, `spring`, 可选 `inheritVelocity` |
| step `hold` | `hold`（时长）— 三者恰一 |
| `sequence` / `parallel` | `children`（非空） |
| `stagger` | `delay`, `children` |
| `delay` | `duration` |
| `marker` | `name`（非空） |

节点嵌套深度上限 32；JSON 嵌套上限 128。  
`"reduced": "instant"`：由 root 推导——每个被动画属性在 t=0 跳到终值。

示例文件：`code/motion.json`（注释标明 **全部数值为占位**，设计师拥有最终词表）。

---

## 5. `controller.h` — 句柄、所有权、分发缝

### 5.1 类型与枚举

```cpp
using TargetId = std::uint64_t;      // 被动画对象的不透明 id
using ControllerId = std::uint64_t;

enum class AnimationState { Idle, Playing, Paused, Finished, Cancelled, Superseded };
enum class EndReason { Completed, Cancelled, Superseded };
```

### 5.2 `IDispatcher`（模块 6 合成器对接缝）

```cpp
struct SpringOverrideInfo {
    std::string property;
    SpringParams params;
    double from = 0, fromVelocity = 0, target = 0;
    double startClock = 0;  // 弹簧启动时的 IClock 时间
};

struct ControllerSnapshot {
    ControllerId id = 0;
    TargetId target = 0;
    std::shared_ptr<const Timeline> timeline;
    double position = 0;
    double rate = 1;
    bool reversed = false, playing = false;
    int loops = 1;            // <0 无限
    double clockTime = 0;
    std::vector<SpringOverrideInfo> overrides;
    std::vector<std::string> ownedProperties;
};

class IDispatcher {
public:
    virtual ~IDispatcher() = default;
    virtual void Submit(const ControllerSnapshot&) = 0;           // 新的或变化的运动描述（lowering 时机）
    virtual void Withdraw(ControllerId, EndReason) = 0;           // 释放为该控制器 lower 的一切
    virtual void Apply(TargetId, const SampleMap&) = 0;           // app 线程值；已 lower 的属性后端可忽略
};

class NullDispatcher final : public IDispatcher;  // 空实现
```

```cpp
// 与 token 的 "instant" 同规则；独立函数以免改 tokens.h
std::shared_ptr<const Timeline> MakeInstantTimeline(const Timeline& tl);
```

**调用时机（依据 `controller.h` 实现）**：

- **`Submit`**：每个间断点调用一次——play、pause、seek、reverse、rate / loop 变化、retarget、所有权变化（失去部分属性）、reduced-motion 吸附、`Release` 掉 override。快照的 `ownedProperties` 列出该控制器此刻拥有、可被 Apply 的属性。
- **`Withdraw(id, reason)`**：仅当该控制器此前 `Submit` 过且尚未撤回时调用，调用后需再次 `Submit` 才会再撤回。三种来源：
  - `Cancel()` → `EndReason::Cancelled`；
  - 飞行中失去最后一个 owned 属性 → `EndReason::Superseded`；
  - 控制器**退出活跃集**（不再 Playing、无未 settle 的 override、且已无 dirty 待 Apply）→ `EndReason::Completed`。这**不仅**发生在正常播完：暂停姿态、Seek 后的停顿姿态、override 弹簧 settle 后，都会在下一次 `Tick` 收到 `Withdraw(Completed)`，即「空闲」也算 Completed。
  - `MotionSystem` 析构**不**调用 `Withdraw`（控制器被分离并置为 Cancelled）。
- **`Apply(target, samples)`**：只在 `Tick()` 内、对**活跃集**中的非终态控制器调用（Playing、有未 settle 的 override，或 dirty——状态变化后补一次）；同一 `TargetId` 下多个控制器的 owned 值合并成**一个** `SampleMap`，每 Tick 每 target 至多一次，空 map 不调用。没有活跃控制器的 Tick **不会**调用 `Apply`。

合成器实现：`CompositionDispatcher`（`composition.h`，§6.5），真实 WinRT 后端未实现；软件实现：`SoftwareDispatcher`（`code/explore/software_dispatcher.h`，只转发 `Apply`，忽略 `Submit` / `Withdraw`）或 `NullDispatcher`。

### 5.3 `AnimationHandle`（应用层主入口）

可拷贝（共享同一 `Controller`）；空句柄惰性（操作 no-op）。

```cpp
class AnimationHandle {
public:
    AnimationHandle() = default;
    explicit operator bool() const;
    bool SameAs(const AnimationHandle& o) const;
    ControllerId Id() const;   // 空句柄为 0

    void Play();
    void Pause();
    void Seek(double t);
    void Reverse();
    void SetRate(double r);
    void SetLoops(int n);      // n<0 无限
    void Cancel();

    // 保留速度的打断：从当前值与速度（无论谁在驱动）弹簧到 newTarget
    // from：无人驱动且无 rest 时的种子；失败返回 false
    // reduced motion 下跳变（无弹簧）
    bool Retarget(const std::string& property, double newTarget, SpringParams spring,
                  std::optional<double> from = std::nullopt);

    void Release(const std::string& property);  // 放下 override；图属性仍由图拥有

    AnimationState State() const;               // 空句柄 → Cancelled
    bool IsActive() const;
    double Position() const;
    bool Owns(const std::string& property) const;
    std::optional<Sample> Value(const std::string& property) const;
    SampleMap Values() const;                   // 仅 owned

    void OnEnd(std::function<void(EndReason)> cb);   // 所有拷贝共享
    void OnMarker(Player::MarkerCallback cb);
};
```

**头文件中没有** `AnimationHandle::SnapToEnd`。吸附终点由 `MotionSystem::SetReducedMotion(true)` 对活跃控制器调用内部 `SnapToEnd` 完成。

### 5.4 `MotionSystem`

```cpp
class MotionSystem {
public:
    explicit MotionSystem(const IClock& clock, IDispatcher* dispatcher = nullptr);
    // dispatcher 为空则用内置 NullDispatcher；不可拷贝
    ~MotionSystem();

    bool ReducedMotion() const;
    void SetReducedMotion(bool on);
    // on→true：对每个活跃控制器 SnapToEnd（in-flight 含 Playing/Paused：正向到终点、反向到 0，并 Completed）

    AnimationHandle Create(TargetId target,
                           std::shared_ptr<const Timeline> full = nullptr,
                           std::shared_ptr<const Timeline> reduced = nullptr);
    // 允许空时间线（仅 Retarget 用）

    AnimationHandle Create(TargetId target, const TokenSet& tokens,
                           std::string_view transition);
    // 过渡不存在 → 空句柄

    void Tick();  // 推进活跃控制器并 Apply owned 值；每帧调用一次

    std::optional<Sample> Current(TargetId target, std::string_view property) const;
    // 所有者实时值，否则 rest 值（速度 0）

    std::size_t ActiveCount() const;

    // ---- 供 detail::Controller；注释写明「非应用层 API」但仍为 public ----
    const IClock& Clock() const;
    bool Reduced() const;
    IDispatcher& Dispatcher();
    void Register / Unregister / Activate / Claim / ReleaseOwnership / ...
    detail::Controller* OwnerOf(TargetId, const std::string&) const;
    std::optional<double> RestValue(TargetId, const std::string&) const;
};
```

### 5.5 所有权规则（Last-Wins）

来自 `controller.h` 文件头注释：

1. **每 `(target, property)` 同一时刻仅一个所有者**。
2. `Play` / `Seek` / `Reverse` 声明时间线属性，或 `Retarget` 某属性时 **Claim**；前所有者立即失去该属性，对其余仍有产权的属性可继续跑。
3. 飞行中（Playing / Paused / 弹簧未 settle）且 **owned 变为空** → `Superseded`（终态，`EndReason::Superseded`）。
4. 空闲或已 Finished 的控制器丢属性后仍可复用；再次 `Play()` 会重新 Claim。
5. **速度交接**：`Retarget` 从**当前所有者**的值与速度起步；`Play()` 从时间线字面量起步（**故意不连续**）。

### 5.6 生命周期

- 活跃控制器由系统 `shared_ptr` 保活；丢光句柄且未 `Cancel` 仍会跑完，终值写入 rest（`Current` 可读）。
- 终态（Cancelled / Superseded）会清空用户回调（打破句柄捕获 lambda 环）。
- **Finished** 保留回调（可重播）；若回调按值捕获自身句柄会成环——弱捕获或用完后 `Cancel`。
- 句柄晚于 `MotionSystem` 析构 → 惰性（`State() == Cancelled`，调用 no-op）。

### 5.7 Reduced Motion

- `Play` / `Reverse` / `Seek` 使用 reduced 时间线（TokenSet 提供，或对 full 做 instant）。
- 零时长时间线在 `Play` / `Reverse` 时立即完成（**跳过 markers**）。
- `Retarget`：跳到目标、速度 0（无弹簧）。
- **中途打开** `SetReducedMotion(true)`：所有活跃控制器 `SnapToEnd`。

---

## 6. `composition.h` — 能力探测、lowering 规划器、`CompositionDispatcher`（模块 6）

> **状态**：已实现（C++17，仅标准库，无 WinRT）。规划器与分发器在**模拟合成器**上通过 2063 项检查（Linux g++ 与 Windows MSVC 各一次）。  
> **不存在**：真实 `Windows.UI.Composition` 后端——`ICompositorBackend` 的 WinRT 实现属于模块 7，`code/` 中没有。头文件中关于合成器的假设 S1–S5、K1–K5 在头文件注释中仍标为未验证；其中 S1 已按 2026-10-20 裁决 A1 改为 `Period = response/(2π)`（#36/#41 真机证据，见 notion_s1_remap_ab.md）。  
> 依赖：`controller.h`（`IDispatcher`、`ControllerSnapshot`）。

### 6.1 能力与属性映射

```cpp
struct Capabilities {
    bool springNaturalMotion = true;   // 系统有 NaturalMotion 弹簧动画
    bool cubicBezier = true;           // 可用 CubicBezierEasingFunction
    bool bezierOvershoot = false;      // y 控制点超出 [0,1] 是否被支持（K2，未验证，默认否）
    std::size_t maxKeyframes = 512;
};

struct CompositorProperty { std::vector<std::string> paths; };  // 如 "scale" → {"Scale.X","Scale.Y"}

class ICapabilityProbe {
public:
    virtual ~ICapabilityProbe() = default;
    virtual Capabilities Caps() const = 0;
    virtual std::optional<CompositorProperty> Map(const std::string& property) const = 0;
};

class StaticCapabilityProbe : public ICapabilityProbe {   // 默认表 + 可设能力
public:
    StaticCapabilityProbe();
    Capabilities Caps() const override;
    std::optional<CompositorProperty> Map(const std::string& p) const override;
    void SetCaps(const Capabilities& c);
    Capabilities& MutableCaps();
    void Add(const std::string& property, std::vector<std::string> paths);
    void Remove(const std::string& property);
};

// 弹簧参数换算（S1 v2，2026-10-20 裁决 A1：Period = response/(2π)，DampingRatio = ζ）；集中在这里
inline SpringParams MapSpringToCompositor(SpringParams p, double periodScale = 1.0 / (2π),
                                          double dampingScale = 1.0);   // 断言 response > 0
struct SpringMapping {
    double periodScale = 1.0 / (2π);   // compositorPeriod = response * periodScale（kPeriodPerResponse）
    double dampingScale = 1.0;    // compositorDampingRatio = damping * dampingScale
    SpringParams ToCompositor(SpringParams p) const;
};
```

`StaticCapabilityProbe` 默认映射：`opacity→Opacity`、`offsetX→Offset.X`、`offsetY→Offset.Y`、`scale→Scale.X+Scale.Y`、`scaleX→Scale.X`、`scaleY→Scale.Y`、`rotation→RotationAngleInDegrees`（旋转单位为度）。

### 6.2 计划类型

```cpp
enum class PlanKind { CompositorKeyframes, CompositorSpring, Hold, AppThread };
const char* ToString(PlanKind k);   // "compositor-keyframes" / "compositor-spring" / "hold" / "app-thread"

struct KeyframeSpec {               // easing 作用于「以本关键帧结束」的那一段（K1）
    double time = 0, value = 0;
    bool bezier = false;
    double x1 = 0, y1 = 0, x2 = 1, y2 = 1;
};
struct KeyframeAnimation {
    std::vector<KeyframeSpec> keys;  // 首帧 time = 0
    double duration = 0, delay = 0, elapsed = 0, rate = 1;
    bool reversed = false;
    std::optional<double> presetValue;   // delay > 0 时立即设置
};
struct SpringAnimation {             // 已换算为合成器口径
    double period = 0.35, dampingRatio = 0.8;
    double from = 0, fromVelocity = 0, target = 0;   // 速度单位：每真实秒
    double delay = 0;
    std::optional<double> presetValue;
};
struct PropertyPlan {
    std::string property;
    std::vector<std::string> paths;  // 未映射时为空
    PlanKind kind = PlanKind::AppThread;
    KeyframeAnimation keyframes;     // CompositorKeyframes
    SpringAnimation spring;          // CompositorSpring
    double holdValue = 0;            // Hold
    double finalValue = 0;           // 动画完成时的值
    std::string reason;              // AppThread 的原因（否则为空）
};
struct LoweringPlan {
    ControllerId id = 0; TargetId target = 0; double clockTime = 0;
    std::vector<PropertyPlan> props;
    const PropertyPlan* Find(const std::string& property) const;
    std::size_t Count(PlanKind k) const;
    bool FullyOffloaded() const;     // 没有 AppThread 项
    std::string ToString() const;
};
```

参考语义（按上述假设，用于测试比对）：`EvaluateKeyframes(keys, t)`、`SimulateKeyframes(anim, t, before)`、`SimulateSpring(anim, t, before)`，其中 `t` 为 Start 后的真实秒数，`before` 为延迟结束前保持的值。

### 6.3 `CompositionLoweringPlanner`

```cpp
class CompositionLoweringPlanner {
public:
    explicit CompositionLoweringPlanner(const ICapabilityProbe& probe, SpringMapping mapping = {});
    LoweringPlan Plan(const ControllerSnapshot& s) const;   // 只为 ownedProperties 出计划
};
```

每个 owned 属性恰好得到一种 `PlanKind`。规则（v1，头文件注释原意）：

- **Override（来自 Retarget）优先于图属性**：已过 `SpringSettleTime` → `Hold(target)`；否则 `CompositorSpring`，以快照 `clockTime` 时刻的状态重启（弹簧 ODE 时不变）；无 NaturalMotion → `AppThread`。
- **图属性**降到合成器须同时满足：有映射；`playing == true`（否则 `Hold` 在 `Timeline::Evaluate(position)` 的姿态）；`loops == 1`；只有一条 track 驱动该属性；无零时长段；全部为 linear/bezier/hold 段（→ `CompositorKeyframes`）**或**恰好一个 spring 段且非反向（→ `CompositorSpring`）；bezier 的 y 在 [0,1] 内，除非 `bezierOvershoot`；关键帧数不超过 `maxKeyframes`。
- 已完成（正向 `position >= start+duration`，反向 `position <= start`）→ `Hold`。
- 其余回退 `AppThread`，`reason` 记录原因（如混合 spring + 关键帧段、循环、多轨冲突）。
- 关键帧使用 `CubicBezier::X1/Y1/X2/Y2`，`IsLinear()` 的段按线性处理（见 §2.1）。
- Reduced motion 无需特殊处理：reduced 时间线是普通时间线；instant 时间线在 Play 时即完成（`playing == false` → `Hold`）。

### 6.4 `ICompositorBackend`（模块 7 需实现的唯一接口）

```cpp
class ICompositorBackend {
public:
    virtual ~ICompositorBackend() = default;
    virtual void StartKeyframes(TargetId, const std::string& path, const KeyframeAnimation&) = 0;
    virtual void StartSpring(TargetId, const std::string& path, const SpringAnimation&) = 0;
    virtual void Stop(TargetId, const std::string& path) = 0;          // 保留当前值（K5）
    virtual void SetValue(TargetId, const std::string& key, double value) = 0;
    // key：已映射为合成器路径，否则为属性名
};
```

`Start*` 会**替换** `(target, path)` 上已在运行的动画。

### 6.5 `CompositionDispatcher`

```cpp
class CompositionDispatcher final : public IDispatcher {
public:
    CompositionDispatcher(const ICapabilityProbe& probe, ICompositorBackend& backend, SpringMapping mapping = {});
    void Submit(const ControllerSnapshot& snap) override;
    void Withdraw(ControllerId id, EndReason reason) override;
    void Apply(TargetId target, const SampleMap& values) override;
    const LoweringPlan* PlanFor(ControllerId id) const;   // 无则 nullptr
    std::size_t LiveAnimationCount() const;               // 正在运行的合成器动画数（按 target+path）
};
```

- `Submit`：用规划器重算该控制器的计划；Keyframes/Spring → `Start*`；Hold → 停止本控制器拥有的该路径动画后 `SetValue(holdValue)`；AppThread → 登记为 app 线程属性。上一份计划中不再存活的路径被 `Stop`（仅当仍归本控制器所有）。
- `Withdraw`：`Completed` → 停止本控制器拥有的动画并 `SetValue(finalValue)`，精确落在终值；`Cancelled` / `Superseded` → 仅 `Stop`，值停在合成器当前值（K5）。随后移除该控制器的 app 线程登记与计划。未知 id 忽略。
- `Apply`：只对计划中留在 app 线程的属性调用 `SetValue`；合成器驱动或 Hold 的属性被忽略。

---

## 7. C-ABI：`code/capi/fm_capi.h`（供 C# P/Invoke）

> **状态**：已实现，**仅软件路径**（`MotionSystem` + `SoftwareDispatcher` + `FramePump`，见 `code/explore/software_dispatcher.h`）；**不经过** `composition.h`，无合成器 lowering。  
> 验证：`fm_capi_test.cpp` 23 项（Linux g++，ASan/UBSan 干净；Windows MSVC 19.44 x64 亦 23/0）。Windows DLL 构建、14 个导出（dumpbin）与 MotionSoftDemo 接入见 [`workbuddy_soft_demo.md`](./workbuddy_soft_demo.md)「派题 #4」§3。  
> 注：头文件/实现文件首行注释写的是 `mvp/fm_capi.h`，实际位置为 `code/capi/`。

### 7.1 规则（头文件注释）

- **所有函数在同一线程（UI 线程）调用**；**不得在 sink / render 回调里调用任何 `fm_*`**。
- 字符串为 UTF-8；id 不透明，`0` 表示失败 / 无。
- 导出宏 `FM_API`：Windows 为 `__declspec(dllexport)`，其他平台为 `visibility("default")`。调用约定为平台默认 C 调用（C# 侧用 `Cdecl`）。
- 实现中每个函数都 `try/catch(...)`，异常不跨 ABI 边界；传入 `fm_system*` 为空时安全返回（no-op 或 0）。

### 7.2 类型

```c
typedef struct fm_system fm_system;   /* 不透明 */
/* fm_on_frame 内对每个 owned 属性回调；仅在有动画的帧 */
typedef void (*fm_sink_fn)(uint64_t target, const char* property, double value, void* user);
/* on=1：启动你的帧源（如 DispatcherQueueTimer）；on=0：停止（静止即停） */
typedef void (*fm_render_fn)(int on, void* user);
```

### 7.3 14 个函数

| # | 签名 | 语义（依据 `fm_capi.h` / `fm_capi.cpp`） |
|---|------|------|
| 1 | `fm_system* fm_create(fm_sink_fn sink, fm_render_fn render, void* user)` | 用真实 `SteadyClock` 创建系统；失败返回 `NULL` |
| 2 | `fm_system* fm_create_manual(fm_sink_fn sink, fm_render_fn render, void* user)` | 测试用：`ManualClock`，时间只由 `fm_manual_advance` 推进 |
| 3 | `void fm_manual_advance(fm_system*, double seconds)` | 推进手动时钟（仅 `seconds > 0` 生效）；对 `fm_create` 创建的系统无效 |
| 4 | `void fm_destroy(fm_system*)` | 销毁；仍在运行的动画随之失效，不再触发回调 |
| 5 | `int fm_load_tokens(fm_system*, const char* json, char* err, int errCap)` | 解析 token JSON（`TokenSet::Parse`）。`0` = 成功并替换当前 token 集；否则返回问题数，`err` 按行写入 `"/path: message"`（截断到 `errCap-1` 并以 0 结尾）；`sys` 或 `json` 为空返回 `1` |
| 6 | `void fm_set_reduced_motion(fm_system*, int on)` | `MotionSystem::SetReducedMotion(on != 0)`，并唤醒帧泵 |
| 7 | `uint64_t fm_animation_create(fm_system*, uint64_t target, const char* transition)` | `transition == NULL`：空动画（仅供 Retarget）；否则按已加载 token 创建；过渡不存在或尚未加载 token → `0` |
| 8 | `void fm_animation_play(fm_system*, uint64_t anim)` | `AnimationHandle::Play()`，并唤醒帧泵 |
| 9 | `void fm_animation_cancel(fm_system*, uint64_t anim)` | `AnimationHandle::Cancel()`（不唤醒帧泵；id 仍保留，需另行 release） |
| 10 | `void fm_animation_release(fm_system*, uint64_t anim)` | 仅丢弃本侧句柄；动画继续跑完，终值进 rest（`fm_current` 可读） |
| 11 | `int fm_animation_retarget(fm_system*, uint64_t anim, const char* property, double target, double response, double damping, int hasFrom, double from)` | `AnimationHandle::Retarget(property, target, SpringParams{response, damping}, hasFrom ? from : nullopt)`；成功返回 `1` 并唤醒帧泵，失败 `0` |
| 12 | `void fm_on_frame(fm_system*)` | 在帧回调中调用：`MotionSystem::Tick()` → sink 回调；`ActiveCount()==0` 时调用 `render(0)` 停泵 |
| 13 | `int fm_is_running(fm_system*)` | 帧泵是否在运行（`1`/`0`） |
| 14 | `int fm_current(fm_system*, uint64_t target, const char* property, double* value, double* velocity)` | `MotionSystem::Current`：所有者的值（上次 Tick 的采样）或 rest 值；有值返回 `1`，否则 `0`；`value`/`velocity` 可为 `NULL` |

**唤醒约定**：`MotionSystem` 没有唤醒钩子；shim 在 `fm_animation_play`、`fm_animation_retarget`（成功时）、`fm_set_reduced_motion` 内部调用 `FramePump::Wake()`，首次唤醒时回调 `render(1)`。

### 7.4 构建（`fm_capi.cpp` 注释 + Windows 实测）

```text
# Linux（测试）
g++ -std=c++17 -O2 -shared -fPIC -I code -I code/explore code/capi/fm_capi.cpp -o libfluidmotion.so
# Windows x64（MSVC，已实测，见 workbuddy_soft_demo.md 派题 #4 §3）
cl /nologo /std:c++17 /EHsc /O2 /W3 /LD /I code\capi /I code /I code\explore code\capi\fm_capi.cpp /Fe:fluidmotion.dll
```

`fm_capi.cpp` 依赖 `explore/software_dispatcher.h`，需加 `-I code/explore`（`fm_capi.cpp` 注释中的单行命令未写包含路径）。MSVC 下 `strncpy` 有 C4996 警告，不影响功能。

### 7.5 C# P/Invoke 接入

参考：`code/capi/csharp_pinvoke_sketch.cs`（未编译草图）；实际接入版本为 `artifacts/win_softdemo_src/FmBridge.cs`、`MainPage.xaml.cs`，过程见 [`workbuddy_soft_demo.md`](./workbuddy_soft_demo.md)「派题 #4」§3。

```csharp
internal static class Fm {
    const string Dll = "fluidmotion";
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void SinkFn(ulong target, IntPtr property, double value, IntPtr user);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void RenderFn(int on, IntPtr user);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr fm_create(SinkFn s, RenderFn r, IntPtr user);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int fm_load_tokens(IntPtr sys, [MarshalAs(UnmanagedType.LPUTF8Str)] string json, byte[] err, int cap);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern ulong fm_animation_create(IntPtr sys, ulong target, [MarshalAs(UnmanagedType.LPUTF8Str)] string? transition);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int fm_animation_retarget(IntPtr sys, ulong anim, [MarshalAs(UnmanagedType.LPUTF8Str)] string prop, double target, double response, double damping, int hasFrom, double from);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void fm_on_frame(IntPtr sys);
    // 其余同理：fm_destroy / fm_set_reduced_motion / fm_animation_play / fm_animation_cancel ...
}
```

类型对应：`uint64_t ↔ ulong`，`int ↔ int`，`double ↔ double`，`fm_system* ↔ IntPtr`，`const char*` 入参 `↔ [MarshalAs(UnmanagedType.LPUTF8Str)] string`，sink 中的 `property` 用 `Marshal.PtrToStringUTF8` 读取。

接入要点：

1. **委托保活**：`SinkFn` / `RenderFn` 委托须存为字段，生命周期覆盖整个原生系统，否则 GC 回收后下一次回调崩溃。
2. **帧源**：保留 16 ms `DispatcherQueueTimer`，由 `render` 回调 `Start()/Stop()`（静止即停）；Tick 中只调 `fm_on_frame`。时间来自引擎自身 `SteadyClock`，计时器抖动不影响轨迹（实测 timer 间隔约 31 ms；需要 60 fps 可改 `CompositionTarget.Rendering`）。
3. **写值**：sink 回调里把 `opacity` 写 `Border.Opacity`、`offsetX` 写 `TranslateTransform.X`；**回调内不得调用 `fm_*`**。
4. **同线程**：按钮 / 指针事件处理器与 timer Tick 都在 UI 线程，满足单线程规则。
5. **部署**：DLL 须为 x64；csproj 用 `<Content Include="..\MotionNative\fluidmotion.dll" Link="fluidmotion.dll" CopyToOutputDirectory="PreserveNewest" />`，`demo_motion.json` 同理（已确认进入 AppX 输出目录）。
6. 示例 token：`code/capi/demo_motion.json`（`demo.enter` / `demo.exit`：opacity 用 bezier、offsetX 用 spring，含 reduced 变体）。

---

## 8. 规划中 / 未实现（勿当作已有 API）

以下在产品文档中出现，**当前 `code/` 中不存在**：

| 概念 | 状态 |
|------|------|
| WinMD 投影、`MotionAnimator` 等 | 规划中（C# 目前经 C-ABI P/Invoke 接入，§7） |
| `Windows.UI.Composition` 真实后端（`ICompositorBackend` 的 WinRT 实现，模块 7） | **不存在**；规划器与 `CompositionDispatcher` 已实现，仅在模拟后端验证（§6） |
| Rive → CompositionSurface | 规划中 |
| `AnimationHandle::SnapToEnd`（公开） | **未暴露**；系统级 reduced snap 已有 |
| `MotionSystem::Advance(dt)` | **无此方法**；用时钟 + `Tick()` |
| `AnimateProperty(target, prop, value, token)` | **无**；用 `Create` + `Play` / `Retarget` |
| namespace `FluidMotion` | 代码为 `motion` |
| `EmbedToHwnd` / HWND 宿主正式 API | **V1.5**；首发仅随附不受支持的参考示例（`disputes.md` §1） |
| 帧预算 / Rive 指针延迟设计约束 | 见 `disputes.md`；不承诺具体延迟帧数 |

---

## 9. 头文件中含糊或需注意之处

1. **`Player::Retarget` vs `AnimationHandle::Retarget`**：二者并存；工程意图是应用层只用后者，但前者无编译期弃用标记。
2. **`MotionSystem` 一批 `Claim`/`OwnerOf`/…**：注释称非应用 API，却是 public——外部勿依赖签名稳定。
3. **`FindEasing`（bezier）vs `TokenSet::Easing`**：内置占位表 vs JSON token；JSON 侧默认仅 `"linear"` 免定义可用。
4. **无公开 `SnapToEnd` 句柄方法**：单测与产品叙述中的「吸附」走 `SetReducedMotion` 或 Seek 终值。
5. **`detail::Controller`**：实现细节，勿从应用代码直接包含使用（虽在同头内联可见）。
6. **`Curve::FromToken` 用的是 `bezier.h` 占位表**，不是设计师 token 文件；应用代码请用 `TokenSet::Easing(name)`（返回 `std::optional<CubicBezier>`）再 `Curve::Bezier(*e)`。
7. **`Withdraw(Completed)` 也表示「变为空闲」**（暂停 / 停顿姿态），不等于动画播完（§5.2）。
8. **`fm_capi.*` / `software_dispatcher.h` 首行注释路径为 `mvp/`**，实际在 `code/capi/`、`code/explore/`；`fm_capi.cpp` 编译需额外 `-I code/explore`（§7.4）。

---

## 10. 测试覆盖

| 模块 | 检查数（0 失败） | 说明 |
|------|------------|------|
| timeline | 422 | 含 Player / Track / 组合 / 相对 settle 阈值 |
| tokens | 743 | 需以 `code/motion.json` 为参数运行（不带参数为 730） |
| controller | 236 | 所有权、Retarget、reduced |
| composition | 2063 | 规划器 + `CompositionDispatcher`，模拟合成器 |
| 合计 | 3464 | |
| 附加：`capi/fm_capi_test` | 23 | 仅经 C-ABI；Linux 下 ASan/UBSan 干净 |
| 附加：`explore/software_demo_test` | 10 | `SoftwareDispatcher` + `FramePump` |

**两平台验证**：Linux g++ 14.2（`-std=c++17 -O1 -Wall -Wextra -Wpedantic`，`artifacts/linux_gtest_log_reply11.txt`）与 Windows MSVC cl 19.44 x64（`/std:c++17 /EHsc /O2 /W3`，`artifacts/win_cl_golden_log_reply11.txt`）结果一致。

Windows **真实合成器后端**：尚未实现，待真机验证。
