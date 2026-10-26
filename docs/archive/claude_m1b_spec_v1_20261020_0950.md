# M1b 规格：真实合成器单属性试点（WinUI 3 / Microsoft.UI.Composition）

状态：规格，未实现。事实分三级：**[文档]** 已在 Microsoft Learn 查到；**[推断]** 我的推理，需实测；**[未验证]** 文档查不到或沉默。
已知基线：box g++ 与 MSVC 19.44 四套测试 422/743/236/2063 全过；Windows 软件路径打断连续性 13/0（原生 C-ABI，|Δv|=|Δvel|=0）。

## 0. 范围

- 一个目标（一个 Border 的 Visual），每次一个属性：`opacity`（keyframe+bezier）、`offsetX`（spring、可打断）。
- 经 `composition.h` 的 `CompositionDispatcher` → 新增 C# `WinRtBackend`；软件路径保留作对照。
- **不做**：partialStart（elapsed>0）、rate≠1、reverse、loop、多轨同属性、chained stages（计划器会走 AppThread/Hold 回退）。
- **前置代码任务**（`composition.h` 目前没有这些开关）：`Capabilities` 增加 `partialStart / playbackRate / reverse`，试点后端全设 false，使 SetRate、暂停后恢复等场景落回 AppThread 而不是下发错误动画。
- 不宣称零 CPU：引擎仍需每帧 `Tick` 做完成记账（`Withdraw`、AppThread 值）。UI 线程卡死时合成器动画继续，引擎记账在解除后补上。

## 1. 已查到的事实（Microsoft Learn）

| 项 | 内容 | 级别 |
|---|---|---|
| `SpringScalarNaturalMotionAnimation` | 成员：`DampingRatio`(float)、`Period`(TimeSpan)；继承 `InitialValue`/`FinalValue`(`IReference<float>`)、`InitialVelocity`(float)；`DelayTime`/`DelayBehavior`/`StopBehavior` 同 KeyFrame 动画 | [文档] |
| DampingRatio | 0 无阻尼；0<ζ<1 欠阻尼；=1 临界；>1 过阻尼 | [文档]，与我们一致 |
| Period | 文档只写“弹簧完成一次振荡所需时间”，**未说明阻尼/无阻尼周期** | [文档]沉默 → **undocumented** |
| 初值/终值 | 未设置则取当前值 | [文档] |
| InitialVelocity | “程序化初速度”，**单位未写** | [文档]沉默 |
| 推荐值 | 偏移：ζ=0.8，Period=50 ms；示例代码却用 0.5 s、0.07 s | [文档]，自洽性差 |
| KeyFrame 缓动 | “属性在两个关键帧之间按曲线变化”，未指明缓动属于哪一侧；样例 `InsertKeyFrame(0,..)` 再 `InsertKeyFrame(1,.., easing)` 与“缓动属于**结束关键帧**所在段”一致 | [文档]弱证据 |
| CubicBezier 控制点范围 | 文档未限制 y，也未保证 y∈[0,1] 之外有效 | [未验证] |
| 表达式读值 | 表达式用被引用对象**在合成器里的当前值**计算 | [文档]；这是合成器内部读值，不是回读到 UI 线程 |
| `ElementCompositionPreview.SetIsTranslationEnabled` | WASDK 命名空间 `Microsoft.UI.Xaml.Hosting` 中存在：“渲染时、布局后的平移变换”；弹簧文档的示例对元素 Visual 用 `"Translation.X"` | [文档] |
| `GetPointerPositionPropertySet(UIElement)` | WASDK 文档中存在（相对元素的指针位置） | [文档] |
| `InitialValueExpression["FinalValue"]="this.StartingValue + 250"` | 弹簧文档示例写法；在我检索到的类成员列表里**没看到**该成员 | [未验证]，需在 WASDK 2.5.1 编译确认 |
| UI 线程回读动画中的值 | 我没查到同步回读 API | [未验证]，假设**不可用** |

## 2. 映射

### 2.1 弹簧

我们的模型：`x'' + 2ζω₀x' + ω₀²x = 0`，`ω₀ = 2π/T₀`，`T₀ = response`，`ζ = damping`。
stiffness/damping/mass 互换：`k = mω₀²`，`c = 2ζ√(km)`；反向 `ω₀=√(k/m)`，`ζ=c/(2√(km))`，`T₀=2π√(m/k)`。m=1 时 `k=(2π/T₀)²`，`c=4πζ/T₀`。

`ζ_c = ζ`（文档语义一致，仍以 M-1 校验）。`Period` 有三个互斥假设，**由实测二选一，`SpringMapping::ToCompositor` 只改这一处**：

| 假设 | 含义 | 换算 `P_c = ?` |
|---|---|---|
| H1（当前代码） | Period = 无阻尼固有周期 | `P_c = T₀` |
| H2 | Period = 阻尼周期（仅 ζ<1 有意义） | `P_c = T₀/√(1−ζ²)`；ζ≥1 需另行约定 |
| H3 | Period = `1/ω₀` | `P_c = T₀/(2π)` |

[推断] 文档推荐 Period=50 ms（ζ=0.8）：H1 下 T₀=50 ms（20 Hz，极硬）；H3 下 T₀≈314 ms，更像常见响应时间。文档自身又用 0.5 s，所以只能当线索，**不能当结论**。

**判别（两次捕获）：**
- (a) ζ_c=0，P_c=0.4 s，0→1：理论 `x=1−cos ωt`，量测振荡周期 `T_m`，`k₀=T_m/P_c`。k₀≈1→H1，≈6.283→H3。
- (b) ζ_c=0.5，P_c=0.4 s：量测相邻波峰间隔 `T_d`。若 `T_d≈k₀P_c/√0.75` 则 Period 属无阻尼型（H1/H3）；若 `T_d≈P_c` 则属阻尼型（H2）。
- 对数衰减校验 ζ：`δ=ln(xₙ/xₙ₊₁)`，`ζ_eff=δ/√(4π²+δ²)`，与设置值比较。
- 初速度单位（S3）：ζ_c=1，设 `InitialVelocity=v₀`，量前 3 帧斜率，应为 v₀ 每**秒**。

调用（`SpringAnimation` 已是合成器参数，含 `SpringMapping`）：
```csharp
var a = compositor.CreateSpringScalarAnimation();
a.DampingRatio = (float)s.dampingRatio;
a.Period = TimeSpan.FromSeconds(s.period);
a.InitialValue = hybrid ? (float?)null : (float)s.from;   // 见 3.B
a.FinalValue = (float)s.target;
a.InitialVelocity = (float)s.fromVelocity;                // 每秒（S3 待测）
a.DelayTime = TimeSpan.Zero;                              // 试点只允许 0
visual.StartAnimation("Translation.X", a);                // 元素先 SetIsTranslationEnabled(el, true)
```
注意 `Translation.X`、`Opacity` 路径要换：默认 `StaticCapabilityProbe` 把 `offsetX` 映射为 `"Offset.X"`，对 XAML 元素大概率被布局覆盖，试点的探针须映射为 `"Translation.X"`。

### 2.2 Bezier

`ScalarKeyFrameAnimation`：
- `Duration = TimeSpan.FromSeconds(spec.duration)`，`IterationCount=1`，`DelayTime=0`。
- 每个 `KeyframeSpec k`：`progress=(float)(k.time/spec.duration)`；首帧 progress=0 不带缓动；**末帧 progress 强制 1.0f**（防 float 舍入）。
- 缓动：`k.bezier ? compositor.CreateCubicBezierEasingFunction(new Vector2(x1,y1), new Vector2(x2,y2)) : compositor.CreateLinearEasingFunction()`，作为**结束关键帧**的缓动传入 `InsertKeyFrame(progress, value, easing)`（K1，实测确认）。
- 控制点 y∉[0,1]：默认 `bezierOvershoot=false`；T-K2 通过后才放开。
- `StopAnimation(path)` 后属性是否留在当前值（K5）、完成后是否保持终值（K4）：实测。

## 3. 飞行中 retarget 不跳变

先说清误差来源：引擎在调用时刻 t_c 算出 (值, 速度)，合成器实际显示的是提交时刻 t_c+δ（δ≤约 1 帧）。直接用引擎值重启弹簧，可见跳变约 `v·δ`（v=700/s、δ=8 ms 时 ≈5.6）。

| 方案 | 做法 | 优点 | 缺点 |
|---|---|---|---|
| **B 混合（推荐）** | 不设 `InitialValue`（文档：缺省=当前值）；`InitialVelocity` 用引擎速度；同一 UI 线程回合 `StartAnimation` 替换旧动画 | 位置对屏幕上实际值**天然连续**，不需回读 | 依赖“缺省=动画中的当前值，而非最后一次 UI 设置值”（T-C1 验证）；速度仍有 `a·δ` 量级误差 |
| **A 引擎播种+前瞻（后备）** | 设 `InitialValue/InitialVelocity` 为引擎在 `t_c+δ_est` 的值（旧弹簧解析式外推 δ_est≈1 帧） | 不依赖缺省行为；可数学补偿 | 需实测 δ_est；δ 抖动时残差仍在 |
| C 表达式/属性集跟随 | 动画驱动一个 PropertySet，Visual 用 ExpressionAnimation 跟随 | 可分离驱动与显示 | 连续性问题原样转移，复杂度高，不推荐 |
| D `InitialValueExpression` | `"this.StartingValue + Δ"` 表示相对终点 | 相对位移很方便 | 成员在 WASDK 2.5.1 未确认；只解决相对目标，不解决速度 |
| E 回读 | 回读动画中的值到 UI 线程 | — | 没查到同步 API，假设不可用 |

**决策规则：** T-C1 通过用 B；不通过用 A（δ_est 取实测中位数）。两者都做 M-4，选 J 更小者。

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
typedef struct { int32_t springNaturalMotion, cubicBezier, bezierOvershoot, partialStart, playbackRate, reverse, maxKeyframes; } fm_caps;
FM_API fm_system* fm_create_composition(fm_sink_fn sink, fm_render_fn render, void* user,
                                        const fm_backend* backend, const fm_caps* caps);
```
C# 侧 `WinRtBackend` 实现四个回调，须捕获所有异常（见审查第 1 条），`TargetId→(UIElement,Visual)` 由应用注册表解析（目标销毁前应用必须先 Cancel）。`set_value` 对 `Translation.X` 用 `UIElement.Translation` 或 `Visual.Properties.InsertVector3("Translation", …)`，试点内定一种并记录（[未验证]）。

## 4. 测试判据（阈值为提议值，M1b-0 后可调）

**量测方法：** `Windows.Graphics.Capture` 抓取窗口，`SystemRelativeTime` 作时间戳；高对比度方块，亮度质心取亚像素位置。帧时间 Δt 取实测刷新周期。注意：此前显示器休眠导致 Rendering 约 32 fps，**先确认前台显示器点亮并能出 60 Hz**（见审查附注的 DwmFlush 探针）。

| 编号 | 项目 | 判据 |
|---|---|---|
| M-1 | 弹簧映射 | 5 组 (ζ,P)：(0,0.4)(0.5,0.4)(0.8,0.3)(1.0,0.3)(1.5,0.3)，300 px 行程，3 s；与 `Spring.At` 对比，RMSE≤0.5 px，最大误差≤1.0 px，峰值时刻误差≤1 帧；选 RMSE 最小的假设，5 组全过才采纳 |
| M-2 | 时长/收尾 | 合成器完成时刻（`CompositionScopedBatch.Completed` 或抓帧）≤ 我们 `SpringSettleTime` + 1 帧（不得提前截断）且 ≥ 我们 − max(100 ms, 0.5·T₀)；在我们的结算时刻 `|x−target| ≤ 1.5·SpringPosEps(span)` |
| M-3 | Keyframe 精度 | 5 条曲线（含 3 条 token 曲线）：与 `Timeline::Evaluate` 比，位置≤0.5 px，opacity≤0.005，总时长误差 p95≤1.5 帧；首末关键帧精确；T-K2：y∉[0,1] 曲线是否过冲，决定 `bezierOvershoot` |
| M-4 | 打断连续性（位置） | 取打断前后 3 帧：`J = abs(p(t_r+Δt) − (p(t_r) + v_r·Δt))` 与 `|p(t_r)−p(t_r−Δt)|` 对比；通过 `J ≤ 1 px + 0.25·|v_r|·Δt`；并且打断后 10 帧与引擎预测轨迹误差≤1 px。对照：软件路径同一场景 |
| M-5 | 前瞻补偿（方案 A） | 实测 δ（调用到首个反映新弹簧的帧）≤2 帧；补偿使 J 降低≥50% |
| M-6 | 卸载证明 | 动画中 UI 线程 `Sleep(200)`：期间显示位置与引擎预测轨迹误差≤1 px，不同位置数≥预期帧数的 80%；软件路径对照应冻结 |
| M-7 | 释放 | 1000 次 start/replace/stop 后：私有内存增长≤1 MB、内核句柄数 ±5、GDI/USER 对象数不变；Unloaded/Closed 后 `SubscribeCount==UnsubscribeCount` 且无 Rendering 处理器残留；`fm_destroy` 后回调计数不再增加；全部完成后 `LiveAnimationCount()==0`，1 s 内属性值变化≤0.01 |
| M-8 | Reduced motion | 切换后启动的合成器动画数为 0；终值与目标相等（≤ float 精度） |
| M-9 | 初速度单位 | 前 3 帧斜率与设置值之比在 1±5%（S3） |

## 5. 阶段与退出条件

1. **M1b-0（只测，不接引擎，约 2–3 天）**：独立 C# 小程序直接调合成器 API，回答 S1–S5/K1–K5/T-K2/T-C1/M-9，输出 `SpringMapping` 结论与 δ。退出：H1/H2/H3 之一胜出且 M-1 通过。
2. **M1b-1**：`fm_create_composition` + `WinRtBackend` + `opacity` keyframes + `Capabilities` 新开关；退出：M-3、M-7（keyframes 部分）、M-8。
3. **M1b-2**：`offsetX` spring + 方案 B/A 的 retarget；退出：M-1、M-2、M-4、M-5。
4. **M1b-3**：M-6 卸载证明与完整 M-7；写入 `api.md`/`usage.md`。

## 6. 风险与待办

- Period 语义与 InitialVelocity 单位文档沉默，只能实测；推荐值与示例相差 10 倍，勿据此取值。
- 显示器休眠使 Rendering 约 32 fps，M1b 全部量测要求前台显示器点亮。
- `Translation` 单位按 DIP/有效像素处理，需在 150% 缩放下实测。
- 对象释放：`CompositionAnimation`、缓动函数是 `IClosable`，替换/停止后何时 `Dispose` 安全，在 M-7 里一并验证。
- 可选后续：给 `MotionSystem` 加 `NextWakeTime()`，让完成记账靠一次性定时器而非每帧 Rendering。
