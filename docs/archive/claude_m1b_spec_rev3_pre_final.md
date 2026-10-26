# M1b 规格：真实合成器单属性试点（WinUI 3 / Microsoft.UI.Composition）

状态：规格，未实现（修订 3：对照 DeepSeek 版与 workbuddy 亮屏复测后修订）。事实分五级：**[文档]** 已在 Microsoft Learn / API 目录查到；**[实测]** 我方直接可见的试点结果；**[转述]** workbuddy 真机结果，经 DeepSeek 文档转述，**我没看到原始日志**；**[推断]** 我的推理；**[未验证]** 文档查不到或沉默。
已知基线：box g++ 与 MSVC 19.44 四套测试 422/743/236/2063 全过；Windows 软件路径打断连续性 13/0（原生 C-ABI，|Δv|=|Δvel|=0）。
**本版相对修订 2 的主要变化：** ① 弹簧映射采纳 `Period=response, ζ_c=ζ` 为工作默认，H2/H3 作废，但**不算定案**（§2.1）；② retarget 默认改为“显式种子 + 前瞻”，原方案 B 降为实验开关（§3）；③ 新增 §2.3 启动 API 选择；④ ζ>1 改为能力开关 `overdampedSpring`，试点默认不下发（§2.1）；⑤ 测量方法采纳 DeepSeek/workbuddy 的修正（§7）。

## 0. 范围

- 一个目标（一个 Border 的 Visual），每次一个属性：`opacity`（keyframe+bezier）、`offsetX`（spring、可打断）。
- 经 `composition.h` 的 `CompositionDispatcher` → 新增 C# `WinRtBackend`；软件路径保留作对照。
- **不做**：keyframe 的 elapsed>0 / rate≠1 / reverse、loop、多轨同属性、chained stages（计划器会走 AppThread/Hold 回退）。
- **前置代码任务**（`composition.h` 目前没有这些开关）：
  - `Capabilities` 增加 `keyframePartialStart / keyframeRate / keyframeReverse`（只约束 KeyFrame 动画；弹簧的中途重启与 rate 折叠靠参数完成，不需要控制器），试点后端全设 false；
  - 增加 `overdampedSpring`（ζ>1，默认 false）；
  - 规划器增加 `seedLead`（前瞻秒数，默认 0），见 §3。
- 不宣称零 CPU：引擎仍需每帧 `Tick` 做完成记账。UI 线程卡死时合成器动画继续，引擎记账在解除后补上。
- **完成事件的风险：** 合成器动画若被提前判定完成，画面会在十几毫秒内跳到终值，而引擎侧一切“正常”。因此 **M1b 一切通过判据以截帧为准，不以完成事件为准**。

## 1. 已查到的事实

| 项 | 内容 | 级别 |
|---|---|---|
| `SpringScalarNaturalMotionAnimation` | 成员：`DampingRatio`(float)、`Period`(TimeSpan)；继承 `InitialValue`/`FinalValue`(`IReference<float>`)、`InitialVelocity`(float)；`DelayTime`/`DelayBehavior`/`StopBehavior` 同 KeyFrame 动画 | [文档] |
| DampingRatio | 0 无阻尼；0<ζ<1 欠阻尼；=1 临界；**>1 过阻尼**（文档明确列出） | [文档] |
| Period | 文档只写“弹簧完成一次振荡所需时间”，未说明阻尼/无阻尼周期 | [文档]沉默 |
| 推荐值 | 偏移：ζ=0.8，Period=50 ms；示例代码却用 0.5 s、0.07 s | [文档]，自洽性差 |
| InitialValue 缺省 | 未设置则取动画开始时被动画属性的值；**该值是“动画中的当前值”还是“最后一次静态设置的值”文档没说** | [文档]沉默 |
| InitialVelocity | “程序化初速度”，单位未写；DeepSeek 称缺省 0、不继承旧动画速度 | [文档]单位沉默；缺省值 [转述] |
| `CompositionObject` 方法 | `StartAnimation(string, CompositionAnimation)`、`StartAnimation(string, CompositionAnimation, AnimationController)`、`StartAnimationGroup`、`StopAnimation`、`StopAnimationGroup`、`TryGetAnimationController(string)`；**没有 `TryStartAnimation`** | [文档]（API 目录成员清单）；workbuddy 亦未找到 [转述] |
| `TryGetAnimationController` | 文档：“返回指定属性上正在运行的动画的 AnimationController”；对 KeyFrame 非 null，对 **NaturalMotion 恒为 null** | 前半 [文档]；后半 [转述] |
| 3 参 `StartAnimation` 的最低系统版本 | 目录页只标了类的版本 | [未验证]；本机 Win10 19045 上可用 [转述] |
| KeyFrame 缓动 | “属性在两个关键帧之间按曲线变化”，未指明哪一侧；样例与“缓动属于**结束关键帧**所在段”一致 | [文档]弱证据 |
| CubicBezier 控制点范围 | 文档未限制 y，也未保证 y∉[0,1] 有效 | [未验证] |
| 表达式读值 | 表达式用被引用对象**在合成器里的当前值**计算；不是回读到 UI 线程 | [文档] |
| `CompositionPropertySet.TryGetScalar` | 文档沉默；试点读到静态值（全 0，连静态写 37 也读不到） | [文档]沉默；[实测] |
| `CompositionCommitBatch`（动画型） | 回调在批内最长动画的 delay+duration 过去后触发 | [文档] |
| `CompositionScopedBatch` | 必须显式 `End()`；`IsActive`/`IsEnded`；`Completed`=批内所有动画和效果完成后触发 | [文档] |
| `IInteractionTrackerOwner.ValuesChanged` | 位置或缩放变化时的回调，文档称是异步模型下更新应用逻辑的最佳方式 | [文档]；频率 [未验证] |
| `Direct3D11CaptureFrame.SystemRelativeTime` | “合成器渲染该帧时的 QPC 时间” | [文档] |
| WGC 与 WinUI 3 | `GraphicsCaptureItem.CreateFromVisual` 只接受 `Windows.UI.Composition.Visual`，**不能用于 WinUI 3 的 `Microsoft.UI.Composition.Visual`**；改用 `IGraphicsCaptureItemInterop::CreateForWindow(HWND)` 截整窗再裁切 | [转述]（DeepSeek 引文档） |
| 弹簧轨迹（亮屏复测） | `response=0.32, ζ=0.78, Period=0.32`：WGC 轨迹的“峰峰间隔”≈0.511 s，与 `T₀/√(1−ζ²)` 一致 | [转述]，**证据有局限，见 §2.1** |
| 弹簧形状残差（亮屏复测） | S1 映射下最佳平移后形状 RMS≈28.6%（0→240）/12.1%（另一场景）；扫参最低≈8%/6% | [转述]；**未解释** |
| 亮屏 vs 熄屏 | 亮屏：`DwmFlush`≈60 Hz、WGC≈60 Hz、`CompositionTarget.Rendering` 仍≈31 ms；熄屏：WGC 仍约 46 fps 到帧但**画面冻结** | [转述] |
| `DwmGetCompositionTimingInfo.cRefresh` | Win10 19045 上亮屏也几乎不涨，不可靠 | [转述] |
| `DCompositionGetFrameId`/`GetStatistics` | Win10 19045 返回 `E_NOTIMPL`，22000+ 才可用 | [转述] |
| 呈现/截屏滞后 | 动画开始到 WGC 看见运动约 160–200 ms | [转述] |
| 亮屏下已过的判据 | 打断位置/速度、终值、贝塞尔形状已过（派题 #11–#12） | [转述]；比对方式见 §4 M-4 的保留意见 |
| `ElementCompositionPreview.SetIsTranslationEnabled` | WASDK `Microsoft.UI.Xaml.Hosting` 中存在；弹簧文档示例对元素 Visual 用 `"Translation.X"` | [文档] |
| `InitialValueExpression["FinalValue"]=…` | 弹簧文档示例写法；类成员列表里没看到该成员 | [未验证] |

## 2. 映射

### 2.1 弹簧

我们的模型：`x'' + 2ζω₀x' + ω₀²x = 0`，`ω₀ = 2π/T₀`，`T₀ = response`，`ζ = damping`。
stiffness/damping/mass 互换：`k = mω₀²`，`c = 2ζ√(km)`；反向 `ω₀=√(k/m)`，`ζ=c/(2√(km))`，`T₀=2π√(m/k)`。m=1 时 `k=(2π/T₀)²`，`c=4πζ/T₀`。

**工作映射（采纳 DeepSeek/workbuddy 的结论）：** `Period = response`，`ζ_c = ζ`（即 `SpringMapping` 两个 scale 保持 1.0）。
- 之前我列的 H3（`Period = 1/ω₀`，推断自“推荐值 50 ms”）**作废**：它预测阻尼周期 3.2 s，与 0.511 s 相差一个数量级。H2（Period=阻尼周期）也作废：它预测 0.32 s。
- H2 作废的直接后果：不再需要为 ζ≥1 另定 Period 换算。在 H1 下 (ω₀, ζ) 对任意 ζ 都有定义，**Period 在过阻尼时仍取 response**。

**但这不是定案，理由（我用闭式解算过）：**
1. 在 ζ=0.78、240 px 行程上，首次过冲只有 **1.99%（4.78 px）**，第二个极值只有 **0.095 px**。所以“峰峰间隔≈0.511 s”不可能是两个相邻峰的实测间隔；与首峰时刻 255.7 ms 的两倍恰好吻合，推测是据首峰时刻推出的，需要原始数据确认。
2. 阻尼周期 `T_d=T₀/√(1−ζ²)` 是**一个方程两个未知数**：(T₀=0.32, ζ=0.78)、(0.25, 0.872)、(0.20, 0.92)、(0.15, 0.956) 都给出 0.511 s。区分它们要靠过冲幅度（4.78 px 对 0.15 px）。
3. 形状残差 28.6%/12.1% 与“Period 正确”并不矛盾也不支持：ζ 偏 ±0.1 只改变曲线约 1.7–1.9% RMS（ζ 在此区间几乎辨不出），而 T₀ 差 2 倍就是约 10% RMS；扫参里 T₀≈0.15 的变体明显更好，提示时间尺度可能有偏差，也可能是测量对齐问题。
4. 峰值速度 2029 px/s：**一帧（16.7 ms）的时间错位 = 34 px = 14% 行程**。帧粒度的截帧若不做亚帧对齐，RMS 就是这个量级。所以不能在没有测出 harness 噪声下限的情况下，把 28% 称为“固有差异”。

**识别协议（用低 ζ，过冲大、衰减慢，信息量足）：**
- 取 `ζ_c ∈ {0.2, 0.4, 0.6}`、`Period ∈ {0.25, 0.4}`，各 0→240，量测首峰时刻 `t_p`、首次过冲比 `M_p`、第二峰（若可测）、最终 1 px 内的时刻。
- 解：`ζ_eff = −ln M_p / √(π²+ln²M_p)`，`ω_d = π/t_p`，`ω₀ = ω_d/√(1−ζ_eff²)`，`T₀_eff = 2π/ω₀`。
- 判定：`T₀_eff/Period ≈ 1` 且 `ζ_eff ≈ ζ_c`（各 ±5%）才算 S1/S2 对 ζ<1 成立；第三个量（结算时刻）用于一致性检查。
- 初速度单位（S3）：ζ_c=1，设 `InitialVelocity=v₀`，量前 3 帧斜率，应为 v₀ 每**秒**。

**ζ>1 策略（见 §2.4 冲突裁决）：** 能力开关 `overdampedSpring`，试点默认 false，测试通过后再开。

调用（`SpringAnimation` 已是合成器参数，含 `SpringMapping`）：
```csharp
var a = compositor.CreateSpringScalarAnimation();
a.DampingRatio = (float)s.dampingRatio;
a.Period = TimeSpan.FromSeconds(s.period);
a.InitialValue = (float)s.from;             // 显式：来自影子状态 + 前瞻，见 §3
a.FinalValue = (float)s.target;
a.InitialVelocity = (float)s.fromVelocity;  // 每秒（S3 待测），必须显式设
a.DelayTime = TimeSpan.Zero;                // 试点只允许 0
visual.StartAnimation("Translation.X", a);  // 2 参，见 §2.3；元素先 SetIsTranslationEnabled(el, true)
```
注意路径：默认 `StaticCapabilityProbe` 把 `offsetX` 映射为 `"Offset.X"`，对 XAML 元素大概率被布局覆盖，试点探针须映射为 `"Translation.X"`。

### 2.2 Bezier

`ScalarKeyFrameAnimation`：
- `Duration = TimeSpan.FromSeconds(spec.duration)`，`IterationCount=1`，`DelayTime=0`。**StartAnimation 前日志 `Duration.TotalMilliseconds` 与全部 progress**（§7.4）。
- 每个 `KeyframeSpec k`：`progress=(float)(k.time/spec.duration)`；首帧 progress=0 不带缓动；**末帧 progress 强制 1.0f**。
- **首帧值：显式写 `tr.InitialValue`，不用 `this.StartingValue`。** 理由：我们的时间线语义是绝对值（`Evaluate(0)`=初值），`StartingValue` 取合成器当前值，两者不等时画面与引擎模型就分叉；它还依赖同一个未验证的“当前值”语义。`StartingValue` 只适用于“从当前值出发”的场景，而 token schema 里没有这种语义。
- 缓动：`k.bezier ? compositor.CreateCubicBezierEasingFunction(new Vector2(x1,y1), new Vector2(x2,y2)) : compositor.CreateLinearEasingFunction()`，作为**结束关键帧**的缓动传入（K1，实测确认）。
- 控制点 y∉[0,1]：默认 `bezierOvershoot=false`；T-K2 通过后才放开。
- `StopAnimation(path)` 后属性是否留在当前值（K5）、完成后是否保持终值（K4）：实测。

### 2.3 启动 API 选择（`TryStartAnimation` 与 `StartAnimation`+`TryGetAnimationController`）

**`TryStartAnimation` 不存在**（`CompositionObject` 成员清单里没有），所以不存在“二选一”。试点用法：

| 场景 | 用法 | 理由 |
|---|---|---|
| 弹簧（任何场景） | `StartAnimation(path, spring)` 2 参 | NaturalMotion 的 `TryGetAnimationController` 恒 null [转述]，没有控制器可用；中途改目标/速率一律靠重新播种和 rate 折叠（`period/rate`、`velocity×rate`）完成 |
| KeyFrame，elapsed=0、rate=1 | `StartAnimation(path, anim)` 2 参 | 试点范围内只有这种 |
| KeyFrame，elapsed>0 / rate≠1 / reverse（**不在试点内**，K3） | `compositor.CreateAnimationController()` + **3 参** `StartAnimation(path, anim, controller)`，再设 `Progress`/`PlaybackRate` | 先建控制器再启动，避免“先启动、再 `TryGetAnimationController`”之间已经过了一个提交窗口、动画已开始播放；`TryGet` 仅在需要接管已运行动画时作为后备 |

K3 的行为（`Progress` 写入是否立即生效、与 `DelayTime` 组合、`PlaybackRate` 精度）都在 M1b-0 的 K3 实验里测，试点之外不依赖。

### 2.4 冲突裁决：ζ>1 是否下发

事实：`composition.h` 的 `PlanOverride`/`PlanGraph` **不看 ζ**，只要 `springNaturalMotion` 为真就下发；DeepSeek 文档里“v1 对 ζ>1 暂不 lower”只出现在一句括号和执行 bot 的审校注里（原始回复不在附件中）。文档本身把 ζ>1 列为“过阻尼”，所以纸面上支持。

裁决：**试点默认不下发（以 DeepSeek 为准），但用能力开关实现，而不是删掉规划器里的下发能力（保留 `composition.h` 的长期行为）。** 做法同 `bezierOvershoot`：
- 规划器在 `ζ > 1 + 1e-6` 且 `!caps.overdampedSpring` 时回退 AppThread，reason=“overdamped (ζ>1) spring unverified on the compositor”；ζ=1（临界，`gentle` token 用的就是它）不受影响。
- 开关翻转条件（T-S2）：ζ=1.0/1.5/3.0 各一次，0→240：无过冲（>0.5 px 即失败）、结算时刻与模型相差≤15%、ζ=1.5 与 3.0 的曲线不得相同（相同说明被截断）。

## 3. 飞行中 retarget 不跳变

两份 spec 的共识：**没有可用的回读**；`InitialVelocity` 缺省不继承旧动画速度，所以必须显式设；弹簧没有控制器。分歧在 `InitialValue`：
- DeepSeek：**总是显式设** `InitialValue`/`InitialVelocity`，种子来自影子状态（闭式解外推）。
- 我的修订 2：不设 `InitialValue`（缺省=当前值），速度用引擎值（方案 B）。

**裁决：采用 DeepSeek 的显式种子作默认（方案 A），B 降为实验开关。** 理由是失败模式不对称：B 若遇到“缺省值是最后一次静态设置的值”，画面会跳回旧起点（大跳变）；A 的失败模式只是 `v·δ` 量级的小误差。B 还依赖文档沉默的语义。亮屏下 A 已通过打断位置/速度判据 [转述]。

**两份 spec（以及现有 `composition.h`）都漏了的一点：种子必须按提交时刻外推，而不是调用时刻。** 规划器目前取 `s.clockTime`（调用时刻 t_c）。合成器实际显示的是提交时刻 t_c+δ。用我们的模型（response 0.32、ζ 0.78、行程 240 px）算：峰值速度 2029 px/s，δ=8.3 ms 就是 17 px 的位置误差，δ=16.7 ms 是 34 px；速度误差 `a·δ` 最大约 `ω₀²·240·δ` ≈ 770 px/s（δ=8.3 ms）。所以种子用 `At(elapsed + seedLead)`，`seedLead≈δ_est`（初值取 1 帧，M-5 实测校准）。

| 方案 | 做法 | 角色 |
|---|---|---|
| **A 显式种子+前瞻（默认）** | 影子状态 `(value, velocity, tLast)` 外推到 `t_c+seedLead`，设 `InitialValue/InitialVelocity`；同一 UI 线程回合 `StartAnimation` 替换旧动画 | 默认 |
| B 混合 | 不设 `InitialValue`，速度同样外推 | 实验开关，T-C1 通过且 M-4 优于 A 才可启用 |
| C 表达式/属性集跟随 | 动画驱动 PropertySet，Visual 表达式跟随 | 不采用 |
| D `InitialValueExpression` | `"this.StartingValue + Δ"` | 仅记录，成员未确认 |
| E 回读到 UI 线程 | `TryGetScalar` / 读 Visual 属性 | 不可行（[实测]恒静态） |

软件→合成器交接沿用 DeepSeek 的做法：`StopAnimation` 后若读回值与影子偏差超阈值，用影子值经 `fm_animation_retarget(hasFrom=1)` 交接。

**后端接口增量（C ABI，纯 blittable）：**
```c
typedef struct { double time, value; int32_t bezier; double x1, y1, x2, y2; } fm_keyframe;
typedef struct { const fm_keyframe* keys; int32_t count; double duration, delay, elapsed, rate;
                 int32_t reversed, hasPreset; double preset; } fm_kf_anim;
typedef struct { double period, dampingRatio, from, fromVelocity, target, delay;
                 int32_t hasPreset; double preset; } fm_spring_anim;
typedef struct { void (*start_keyframes)(uint64_t target, const char* path, const fm_kf_anim*, void* user);
                 void (*start_spring)(uint64_t target, const char* path, const fm_spring_anim*, void* user);
                 void (*stop)(uint64_t target, const char* path, void* user);
                 void (*set_value)(uint64_t target, const char* key, double value, void* user);
                 void* user; } fm_backend;
typedef struct { int32_t springNaturalMotion, cubicBezier, bezierOvershoot, overdampedSpring,
                         keyframePartialStart, keyframeRate, keyframeReverse, maxKeyframes;
                 double seedLead; } fm_caps;
FM_API fm_system* fm_create_composition(fm_sink_fn sink, fm_render_fn render, void* user,
                                        const fm_backend* backend, const fm_caps* caps);
```
C# 侧 `WinRtBackend` 实现四个回调，须捕获所有异常；`TargetId→(UIElement,Visual)` 由应用注册表解析（目标销毁前应用必须先 Cancel）。`set_value` 对 `Translation.X` 设值时要保留 Y/Z 分量（用 `UIElement.Translation` 或 `Properties.InsertVector3("Translation", …)` 时整向量被覆盖），试点内定一种并记录。

## 4. 测试判据（阈值为提议值）

**量测方法：以 §7.2 方法 1（截帧）为真值，先过 §7.3 的 G0 与 E0。** 帧时间 Δt 取实测刷新周期。

| 编号 | 项目 | 判据 |
|---|---|---|
| M-1 | 弹簧识别（替代原“5 组映射拟合”） | 按 §2.1 识别协议（低 ζ）：`T₀_eff/Period` 与 `ζ_eff/ζ_c` 均在 1±5%，三个量（t_p、M_p、结算）自洽；不自洽则判模型不是二阶弹簧，**不得**用“固有差异”结案 |
| M-1b | 形状残差（门槛） | 以 **E0-fast（§7.3）测得的 harness 噪声下限 N** 为基准：弹簧形状 RMS ≤ max(1%·行程, 2N)；N 未测出前该项为 `UNKNOWN`，不是 fail 也不是 pass |
| M-2 | 时长/收尾 | 截帧得到的运动结束时刻 ≤ 我们 `SpringSettleTime` + 1 帧（不得提前截断）且 ≥ 我们 − max(100 ms, 0.5·T₀)；在结算时刻 `|x−target| ≤ 1.5·SpringPosEps(span)` |
| M-3 | Keyframe 精度 | 5 条曲线（含 3 条 token 曲线）：与 `Timeline::Evaluate` 比，位置≤0.5 px，opacity≤0.005；首末关键帧精确；T-K2：y∉[0,1] 是否过冲，决定 `bezierOvershoot` |
| M-4 | 打断连续性 | **全程只用一个时间偏移 τ**（不得对打断前后分段分别拟合 τ，否则会把 δ 造成的跳变吸收掉）：打断后 10 帧与影子预测误差≤1 px；打断时刻位置残差 `≤ 1 px + 0.25·|v_r|·Δt`；对照软件路径同场景。A 与 B（实验）都测 |
| M-5 | 前瞻补偿 | 实测 δ（调用时刻 QPC 到首个反映新弹簧的帧）；`seedLead` 取其中位数；补偿使打断残差降低≥50% |
| M-6 | 卸载证明 | 动画中 UI 线程 `Sleep(200)`：期间截帧位置与影子预测误差≤1 px；软件路径对照应冻结 |
| M-7 | 释放 | 1000 次 start/replace/stop：私有内存增长≤1 MB、内核句柄 ±5、GDI/USER 不变；Unloaded/Closed 后订阅数==退订数；`fm_destroy` 后回调不再增加；全部完成后 `LiveAnimationCount()==0` |
| M-8 | Reduced motion | 切换后合成器动画数为 0；终值精确 |
| M-9 | 初速度单位 | 前 3 帧斜率与设置值之比在 1±5% |
| M-10 | ζ>1 | T-S2（§2.4） |

## 5. 阶段与退出条件

1. **M1b-0（只测，不接引擎，约 3 天）**：独立 C# 小程序直接调合成器 API。先做 G0 与 E0/E0-fast（§7.3），再做 S1/S2 识别（M-1）、T-S2、K1–K5、T-K2、T-C1、K3 实验、M-9，并按 §7.4 清单复现“14–30 ms 完成”。退出：G0、E0 通过；N 已测；S1/S2 在 ζ<1 上通过 M-1（否则 `SpringMapping` 改为实测拟合的分段映射）。
2. **M1b-1**：`fm_create_composition` + `WinRtBackend` + `opacity` keyframes + 新的能力开关；退出：M-3、M-7（keyframes 部分）、M-8。
3. **M1b-2**：`offsetX` spring + 方案 A 的 retarget（含 `seedLead`）；退出：M-1、M-2、M-4、M-5。
4. **M1b-3**：M-6、完整 M-7、M-10；写入 `api.md`/`usage.md`。

## 6. 风险与待办

- **弹簧形状 28.6%/12.1% 未解释**（S1 映射下）；扫参最优 8%/6% 的变体提示时间尺度可能偏差，也可能是对齐误差。需要 E0-fast 噪声下限和低 ζ 识别后才能结案；**不要用“固有差异”结案同时保留 1% 门槛**，两者互相矛盾。
- 下游所有 workbuddy 数字都是转述，请把原始 `(t,x)` 逐帧 CSV 放进仓库，便于复算 `t_p`、`M_p`、结算时刻。
- **屏幕休眠使合成器时钟与画面不可信，数据一律标 `INVALID`（不是 fail）**；软件路径结果不受影响。
- 完成事件与读值接口不可靠（§7.1），M1b 的通过判据只认截帧。
- 无头/无显示环境没有可靠方案（§7.2 末条）。
- `Translation` 单位按 DIP/有效像素处理，需在 150% 缩放下实测。
- 对象释放：`CompositionAnimation`、缓动函数是 `IClosable`，何时 `Dispose` 安全，在 M-7 里验证。
- 可选后续：给 `MotionSystem` 加 `NextWakeTime()`，让完成记账靠一次性定时器而非每帧 Rendering。

## 7. 读取飞行中数值 / 测量合成器路径

### 7.1 两个现象与可能原因

**现象 A：`TryGetScalar` 只返回静态值（全 0）。**
- **最可能（设计如此）：** 属性集的值由 UI 侧写入；动画和表达式在合成器线程对合成器侧副本求值，**不回写**应用侧副本。文档沉默，实测一致。所以“全 0”就是插入的初值，**不能说明表达式有没有在跑**。
- 判别：先 `InsertScalar("x", 7)` 再读应得 7；同一表达式再驱动一个可见元素，用截帧看它是否真的在变。
- 结论：这不是 bug，不要用它做测量。

**现象 B：关键帧动画 14–30 ms 就报告完成。** 文档：动画批在“批内最长动画的 delay+duration 过去后”触发，所以 14–30 ms 约等于合成器 1–2 帧。

| 编号 | 假设 | 判别实验 | 现状 |
|---|---|---|---|
| B1 | 批里没有动画（空批）：`StartAnimation` 在 `End()` 之后调用；批与动画不是同一个 Compositor；路径无效 | 打印 `IsActive/IsEnded`；用 3 s `Opacity` 标定动画重测 | 亮屏下贝塞尔形状已过 [转述]，代码类原因可能性下降 |
| B2 | 有效时长只有十几毫秒：单位/赋值错、关键帧 progress 写成秒 | `StartAnimation` 前日志 `Duration` 与全部 progress | 同上 |
| B3 | 路径无效（`Translation.X` 未调 `SetIsTranslationEnabled`） | 改用 `Opacity` 标定 | 同上 |
| B4 | **显示休眠/DWM 暂停，合成器动画时钟不按真实时间推进** | 亮屏重跑；先做 G0 | **现在是最可能原因**：亮屏复测里贝塞尔与打断已过 [转述]，而 14–30 ms 现象出自熄屏试点 |
| B5 | 测量方法自身粗：在 Rendering 里读完成时刻，分辨率就是帧间隔（亮屏下该事件仍≈31 ms） | 与截帧时间戳交叉验证 | 成立，但只影响精度 |

### 7.2 可靠读值/测量方法（按推荐度）

| # | 方法 | 能测什么 | 做法与局限 |
|---|---|---|---|
| 1 | **Windows.Graphics.Capture 截帧（首选，真值）** | 位置、透明度的逐帧轨迹，帧时刻 | **WinUI 3：`CreateForWindow(HWND)` 截整窗再按目标元素矩形裁切**（`CreateFromVisual` 不可用）。`Direct3D11CaptureFramePool` + `FrameArrived`，时间用 `SystemRelativeTime`；`IsBorderRequired=false`、`IsCursorCaptureEnabled=false`、设置 `MinUpdateInterval`。拷到 staging 纹理 Map，算质心（位置，亚像素）或亮度（opacity）。读数元素：用同一动画驱动专用 SpriteVisual 的 `Size.X`/Opacity，使数值编码成像素。**局限：** 动画开始到画面可见约有 160–200 ms 滞后 [转述]，必须做时间对齐（见 M-4：全程单一 τ）；**熄屏时 WGC 仍约 46 fps 到帧但画面冻结，所以“有帧到达”不能证明有效，必须叠加位移门槛（G0 第 3 条）** |
| 2 | **InteractionTracker + `ValuesChanged`（推送通道）** | 合成器侧值回到 UI 线程 | [文档] 位置/缩放变化时回调。**[未验证]：** 频率/合并/延迟；`TryUpdatePositionWithAnimation` 是否接受 NaturalMotion。用途：与截帧交叉验证 |
| 3 | **DWM 节拍（只测节拍，不测数值）** | 合成器真实帧率 | `DwmFlush()` 循环测间隔（亮屏≈16.7 ms）；`PresentMon` 监视 `dwm.exe`；ETW。**不要用 `DwmGetCompositionTimingInfo.cRefresh`**（Win10 19045 上亮屏也不涨 [转述]）；`DCompositionGetFrameId`/`GetStatistics` 仅 22000+ 作旁证（Win10 19045 为 `E_NOTIMPL` [转述]） |
| 4 | **Batch `Completed`（只测完成时刻）** | 动画完成时刻 | E0 通过后才可用，必须与截帧交叉验证，不得单独作判据 |
| 5 | **外部真值**（高速相机/光电管/采集卡） | 不依赖 DWM | 兜底，用于仲裁 |
| 6 | **不可用/已证伪** | — | `TryGetScalar`（[实测]恒静态）；`CompositionTarget.Rendering` 间隔当合成器时钟（亮屏仍≈31 ms [转述]）；`PrintWindow`/`CopyFromScreen`（熄屏全白/找不到像素 [转述]）；`CreateFromVisual`（WinUI 3） |
| 7 | **无头方案** | — | 没找到可靠方案；保持显示器点亮，或 HDMI dummy plug（我的经验，非文档） |

### 7.3 门禁 G0 与标定 E0

**G0 环境门禁**（失败 → 该次运行判 `INVALID`，不是 fail）：
1. `DwmFlush()` 连续 300 次，平均间隔 = 1/刷新率 ±10%，p99 ≤ 1.5×；
2. WGC（`CreateForWindow`）在观察窗内持续到帧；
3. **位移门槛：** 已知行程的动画期间，裁切区域内方块位移（峰值−起点）≥ 行程的 10%，否则判 `INVALID(frozen-frames)`；
4. 窗口可见、前台、显示器点亮。
（`Rendering` 间隔与 `cRefresh` 都不参与门禁。）

**E0 标定动画**（已知答案）：`ScalarKeyFrameAnimation`，Duration=**2.000 s**，线性，0→300（`Translation.X`）和 0→1（`Opacity`）各一次。截帧位置对时间线性拟合 R²≥0.999，斜率误差≤1%，起止时刻误差≤1 帧；`Batch.Completed` 与截帧结束相差≤2 帧。

**E0-fast（新增，测 harness 噪声下限 N）：** 线性 0→240，Duration=**0.25 s**（峰值速度≈960 px/s，与弹簧场景同量级），按与弹簧测量**完全相同**的对齐流水线（全程单一 τ、亚帧拟合）处理。N = 该线性动画的位置 RMS 占行程的百分比。M-1b 以 N 为基准。

**E0b：** 同一标定再跑一次 NaturalMotion（ζ=1，P=0.4 s）作为弹簧基线。

### 7.4 复现“14–30 ms 完成”：StartAnimation 前必须记录

`Duration(ms)`、`DelayTime`、`IterationCount`、全部关键帧 progress（首=0、末=1）、目标路径、`Visual` 是否已在树中、`batch.IsActive/IsEnded`（Create 后与 End 后各记一次）、批与动画是否同一个 `Compositor`、`StartAnimation` 是否抛异常、G0 结果。
