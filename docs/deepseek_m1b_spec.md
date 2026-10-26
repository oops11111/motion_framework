# M1b 合成器路径规格（DeepSeek）

> 项目：FluidMotion / Windows 桌面端动效框架  
> 来源：DeepSeek Harness（deepseek-account / deepseek-v4-pro / reasoningEffort=max）  
> 整理：执行 bot 轻量排版 + 文末审校注；正文 API 结论主要来自 DeepSeek。**2026-10-20** 按 workbuddy 真机核实与**亮屏复测**修订了 AnimationController / WGC / 帧 id / frozen-frames / **S1 Period 定案** / **`DwmFlush` 门槛** / Rendering 说明（见元信息修订记录）；其余不确定处仍标「待真机验证」。

---

# M1b 合成器路径规格（FluidMotion / Windows 桌面端）

> 适用范围：`composition.h` 的 lowering 到 **Microsoft.UI.Composition（WinUI3 / WASDK 2.5.1）** 的精确映射与真机验证方案。
> 状态标记：「已文档化」= 官方文档明确；「已核实」= workbuddy 真机已确认；「S1–S5 / K1–K5」= 沿用 `composition.h` 未验证假设；「待真机验证」= 文档未明说或亮屏/行为尚未确认。
> 试点结论依据：`workbuddy_soft_demo.md` 派题 #6（显示器休眠，帧时序不具代表性，但读回/批处理现象有效）。

---

## 1. composition.h lowering → Microsoft.UI.Composition 精确映射

### 1.1 属性路径映射

| 框架属性 | 合成器路径 | 备注 |
|---|---|---|
| `opacity` | `Opacity` | 直接映射 |
| `offsetX` / `offsetY` | **`Translation.X` / `Translation.Y`**（试点修订） | XAML 元素 Visual 的 `Offset` 归布局所有、会被覆盖（#6 实测）。需先 `ElementCompositionPreview.GetElementVisual(elem)` + `ElementCompositionPreview.SetIsTranslationEnabled(elem, true)`；经 `StaticCapabilityProbe::Add("offsetX", {"Translation.X"})` 换路径，无需改规划器 |
| `scale` | `Scale.X` + `Scale.Y`（两路径同值） | 一个计划项→两条 `Start*` 调用 |
| `scaleX` / `scaleY` | `Scale.X` / `Scale.Y` | |
| `rotation` | `RotationAngleInDegrees` | 单位：度 |

能力探测（模块 7）：用 `ApiInformation.IsTypePresent("Microsoft.UI.Composition.SpringScalarNaturalMotionAnimation")` 等把结果写回 `Capabilities`；具体可探测类型清单与最低 OS 门槛**待真机验证**（目标 TFM `windows10.0.26100.0`；真机 OS 为 **Win10 19045**，与 SDK 目标版本不同）。

### 1.2 弹簧：SpringScalarNaturalMotionAnimation

#### 物理换算（m=1 假设）

弹簧 ODE（x = value − target）：`x″ + 2ζω₀·x′ + ω₀²·x = 0`，其中 `ω₀ = 2π/response`。

| 量 | 公式（m=1） | 一般质量 m |
|---|---|---|
| 刚度 k | `k = ω₀² = (2π/response)²` | `k = m·(2π/response)²` |
| 阻尼系数 c | `c = 2ζω₀ = 4πζ/response` | `c = 2ζm·(2π/response)` |
| 反解 Period | `Period = 2π/√k` | `Period = 2π√(m/k)` |
| 反解 DampingRatio | `DampingRatio = c/(2√k)` | `= c/(2√(k·m))` |

**Period 的语义（S1）**：**已亮屏复测定案（workbuddy）** — 合成器 `Period` 为**无阻尼周期**（UNDAMPED）。

- **换算（v1，已作废）**（**v1，已被 S1 v2 取代**：2026-10-20 裁决 A1 改为 `Period = r/(2π)`，见 [notion_s1_remap_ab.md](./notion_s1_remap_ab.md)）：`Period = response`，`DampingRatio = ζ`（`SpringMapping` 两 scale 保持 1.0）。文档原文 “time for the Spring to complete a single oscillation”（[Period](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.springvector3naturalmotionanimation.period)）在本机对应无阻尼自然周期 `T₀ = response = 2π/ω₀`。
- **证据**：`response=0.32 s`、`ζ=0.78` 时，WGC 轨迹峰峰间隔 ≈ **0.511 s**（= `0.32/√(1−0.78²)`），**不是** 0.320 s。若 Period 被当成阻尼周期输入，峰峰应≈0.32 s；实测否定该分支。**不要**用 `Period = response/√(1−ζ²)`（那是「阻尼周期输入」假设下的换算，已否决）。
- 欠阻尼振荡周期关系（仅用于理解峰峰，不写入 Period）：`T_d = T₀/√(1−ζ²)`。
- `DampingRatio`（S2）：定案侧 `= ζ`。**已核实（派题 #14）**：ζ≥0 **接受且不钳制**（测 1.0/1.2/2.0/5.0 读回≈设定值）；ζ<0 **抛** `ArgumentException`（cannot be less than 0）；过阻尼 NaturalMotion **可运行**。产品仍选 **v1 对 ζ>1 不 lower**（走 AppThread/`spring.h`）——`composition.h` 已加门控（见审校注①）；**轨迹保真已测（派题 #23 PASS）**：单调、overshoot=0、ζ1.5≠3.0 可区分，但 settle≪spring.h（ratio≈3.7）属固有差异量级——**仍不抬门**；ζ=0.78 包络拟合交叉验证仍待做。
- `Period` 极小值（派题 **#26**）：0.001+ **ACCEPT 不钳**；Zero **THROW**；负值赋值接受。token 0.001 s 可用但近瞬时；产品建议 **Period>0**（规划器可拒 ≤0）。

#### 参数映射

| `SpringAnimation` 字段 | API |
|---|---|
| `target` | `SpringScalarNaturalMotionAnimation.FinalValue = target` |
| `period` / `dampingRatio` | `Period = TimeSpan.FromSeconds(period)`；`DampingRatio = dampingRatio` |
| `from` | `InitialValue = (float)from`（C# 投影为 `Nullable<float>`；**必须显式设**，见 §2） |
| `fromVelocity` | `InitialVelocity = (float)fromVelocity`。文档：默认值 **0**（[InitialVelocity](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.scalarnaturalmotionanimation.initialvelocity)）→ 不设=从 0 起跳，**不会继承速度**。**S3 已核实（派题 #15）**：单位为**值/真实秒（DIP/s）**，与 `Period` **独立**（同 IVel 换 Period 初段斜率不按 Period 缩放）；软弹簧 Period=1.5 时斜率符号/量级跟命令；硬弹簧初段位置仍按 v0 排序。rate 折叠抽查 RMS≈13.6%——与 #13 固有形状残差同量级，**不能干净证伪**公式，仍作规划假设（见 §5.2 残余 / §5.1#13）。 |
| `delay` | `CompositionAnimation.DelayTime = TimeSpan.FromSeconds(delay)`（真实秒，不折算） |
| `presetValue` | 先 `visual.Properties.InsertVector3("Translation", …)` / `SetValue(presetValue)`，再 `StartAnimation`；`DelayBehavior` 保持默认 `SetInitialValueAfterDelay`（官方语义：延时期间动画不写属性、属性保持当前值 → 即 presetValue；[AnimationDelayBehavior](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.animationdelaybehavior)）。**S4 已核实（派题 #16）**：延时期间保持 preset；`presetValue == InitialValue` → 延时结束**无跳变**；`IV≠preset` → 延时结束**跳到 InitialValue** 再继续。`SetInitialValueBeforeDelay` 在 IV=preset 时与 AfterDelay 表现相近（仅记录）。证据 `artifacts/win_s4_delay_probe.txt`。 |
| 启动 | `visual.StartAnimation("Translation.X", spring)`；已在跑的动画被**隐式替换**（`ICompositorBackend` 的“Start\* REPLACES”语义） |

#### rate 折叠（不用 PlaybackRate）

设计：**不**对 NaturalMotion 动画设置 `AnimationController.PlaybackRate`——**已真机核实（workbuddy）**：`TryGetAnimationController` / `AnimationController` **仅适用于 KeyFrameAnimation**；对 **NaturalMotion（弹簧）按设计恒为 `null`**，无从设 PlaybackRate。弹簧 rate 一律折进参数：

```
Period_comp = response / rate        // ω₀' = rate·ω₀ → 真实时间轨迹 = 原弹簧加速 rate 倍
InitialVelocity_comp = v0_real × rate // u'(0) = rate·x'(0)，推导见 §2.1 注释
DampingRatio 不变；from/target 不变
```

验证方法：`u(t)=x(rate·t)` 代入 ODE 得 `u″ + 2ζ(rate·ω₀)u′ + (rate·ω₀)²u = 0`，即 `Period/rate`、`velocity×rate`（公式**待真机验证**，规划器已按此实现）。

**Hold / Stop 对应**：

| 框架动作 | API |
|---|---|
| `Hold` | `visual.StopAnimation(path)` → `SetValue(path, holdValue)`（先停后设，否则动画覆盖） |
| `Withdraw(Cancelled/Superseded)` | `StopAnimation(path)`，值停在当前（K5，与 [AnimationStopBehavior](https://learn.microsoft.com/en-us/uwp/api/windows.ui.composition.animationstopbehavior) 的 `LeaveCurrentValue` 语义一致；**S5 已核实（派题 #17）**：完成后保持终值；中段 Stop 冻结（详见 §5.1#15）） |
| `Withdraw(Completed)` | `StopAnimation(path)` → `SetValue(path, finalValue)` 精确落终值（规避 S5 合成器自身停点差异） |

### 1.3 关键帧：ScalarKeyFrameAnimation

**映射**（`KeyframeSpec` 列表，首帧 `time=0`）：

```
duration = keys.back().time (>0，规划器已排除零时长段)
对每个 KeyframeSpec k：
  p = clamp(k.time / duration, 0, 1)              // 归一化进度，官方要求 [0,1]
  easing = k.bezier ? compositor.CreateCubicBezierEasingFunction(new Vector2(k.x1,k.y1), new Vector2(k.x2,k.y2))
                    : null
  InsertKeyFrame((float)p, (float)k.value, easing)
```

- **K1（easing 作用段）**：**已核实（派题 #19）**——easing 作用于结束于该关键帧的段；单段 220ms decelerate 对照 EvaluateKeyframes/BezierEase RMS≈0.57%。
- **首帧**：优先 `InsertExpressionKeyFrame(0f, "this.StartingValue")` —— `this.StartingValue` = 动画开始时该属性值（文档化表达式关键字），保证起跑无跳变；仅当 app 侧状态与合成器当前值确定一致时才用显式值（推荐仍用表达式首帧，省去一致性论证）。
- **K2（y 越界）**：真机（派题 #20）确认 **HONOR**：Create/Start 不抛；WGC 过冲与未截断 BezierEase 对齐（例 y=(-0.5,1.5) → min≈-8 max≈108）。规划器默认 `bezierOvershoot=true`。文档仍未声明 y∈[0,1] 约束（[CreateCubicBezierEasingFunction](https://learn.microsoft.com/en-us/uwp/api/windows.ui.composition.compositor.createcubicbeziereasingfunction)）。
- **elapsed>0 / rate≠1 / reversed（K3，仅 KeyFrameAnimation）**：**已真机核实（workbuddy）** —
  - 不存在 `TryStartAnimation`；
  - **贝塞尔 / 关键帧路径**：用 `compositor.CreateAnimationController()` + **三参** `visual.StartAnimation("Translation.X", anim, controller)`（带 `AnimationController` 的重载）取得控制器；亦可用 `StartAnimation` 后再 `TryGetAnimationController`（KeyFrame 下非 null）。
  - **弹簧 / NaturalMotion 路径**：`TryGetAnimationController` **按设计恒为 `null`**——**不要**指望用控制器做 Progress/PlaybackRate；中段改目标只走影子种子 + 显式 `InitialValue`/`InitialVelocity`（§2.1 / §2.4）。
  - 关键帧定位：`Progress = elapsed / duration`（正向）或 `1 − elapsed / duration`（反向）；`PlaybackRate = ±rate`（负值反向；范围 [−16, 16]，见 [PlaybackRate](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.animationcontroller.playbackrate)）。无 Direction 属性。
  - **rate>16** → 该属性回 AppThread（reason 记录）。
  - Progress 写入后立即生效（派题 #22），但线性 KeyFrame 上 **Progress≠value 分数**（例 Progress=0.5→WGC≈90 非 50）；DelayTime 组合同畸变。失败/elapsed 定位回退：关键帧中段回 AppThread 或按 §2.4 影子状态重启。PlaybackRate=±rate **可用**。
- **DelayTime / DelayBehavior**：同弹簧（`DelayTime` + 默认 `SetInitialValueAfterDelay` + 先 `SetValue(presetValue)`）。注意 `SetInitialValueBeforeDelay` 的官方语义是“延时前先应用 Keyframe 0 的值”——与我们的 presetValue 模式不符，**不要用**。
- **K4（派题 #21）**：关键帧完成后 WGC≥500ms 保持终值（median=100，range=0；`artifacts/win_k4_hold_probe.txt`）。

### 1.4 时间模型核对

`position(t) = elapsed + rate·(t−delay)`（正）/ `= duration − (elapsed + rate·(t−delay))`（反）；`delay` 为真实秒。后端实现必须让合成器侧进度与此一致（Progress 定位 + PlaybackRate + DelayTime 的组合），不一致即报 `reason` 回 AppThread。

---

## 2. 飞行中改目标不跳变（Retarget / override）

### 2.1 弹簧 override：闭式解种子（主路径，已实现于规划器）

```
elapsed = clockTime − startClock
st = Spring(params, from, target, v0).At(elapsed)      // spring.h 闭式解
若 elapsed ≥ SpringSettleTime → Hold(target)
否则 CompositorSpring{ from=st.value, fromVelocity=st.velocity, target=新目标, period=response/rate, dampingRatio=ζ }
```

原理：弹簧 ODE 时不变，从当前 (value, velocity) 重启等价于继续原轨迹后改目标。**必须显式设 `InitialValue` / `InitialVelocity`**（种子来自影子状态 / `spring.h::At`，见 §2.4）：
- `InitialValue` 文档语义：null 时“使用被动画属性的起始值”（[InitialValue](https://learn.microsoft.com/en-us/uwp/api/windows.ui.composition.scalarnaturalmotionanimation.initialvalue)）——依赖合成器内部状态，不采用；
- `InitialVelocity` 默认 **0**，不继承 → 不显式设必跳变；
- **已真机核实**：NaturalMotion 上 `TryGetAnimationController` 恒 `null` → **弹簧打断 / 改目标不依赖、也不使用 TryGet**；
- 若 override 无 rate 概念（`SpringOverrideInfo` 无 rate），按 rate=1 处理。

### 2.2 关键帧中段（K3 路径，仅 KeyFrame）

**已真机核实**：用 `CreateAnimationController` + 三参 `StartAnimation(..., controller)`（或 Start 后 `TryGetAnimationController`，KeyFrame 非 null）拿到控制器，再设 `Progress` / `PlaybackRate`（§1.3）。首帧优先 `this.StartingValue`。**不做**“用闭式解重算值当首帧”的近似（会丢失段内速度方向连续性）。Progress 行为验证失败 → 该属性回 AppThread，并在 `reason` 注明。

与弹簧对比：NaturalMotion **没有**可用的 AnimationController；中段改目标只走 §2.1 / §2.4。

### 2.3 读回能力矩阵（能/不能拿到“正在动画属性的当前值”）

| 通道 | 官方语义 | 试点实测（熄屏） | 结论 |
|---|---|---|---|
| `CompositionPropertySet.TryGetScalar/TryGetVector3` | 文档**未承诺**返回动画求值后的值（[CompositionPropertySet](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.compositionpropertyset)） | 恒为静态值：135 采样全 0；静态写 `Translation.X:=37` 也读不到（表达式未重求值） | **不可用**作动画值读回；亮屏下是否逐帧更新**待真机验证**，仍不作设计依赖 |
| `Visual.Properties.TryGetVector3("Translation")` | 同上（属性集存储语义） | 读到静态写 37 | 仅静态值；动画中值**待真机验证**，不依赖 |
| `StopAnimation` 后读回 | `StopAnimation` 停在当前值（K5 / `AnimationStopBehavior.LeaveCurrentValue`） | 熄屏未验证 | 预期可行（值被“物化”为静态）；关键帧中段与弹簧中段分别**已核实（派题 #18 / §5.1#16）** |
| `TryGetAnimationController` / `AnimationController` | KeyFrame 可取控制器；NaturalMotion **无** | **已核实**：弹簧上恒 `null`；KeyFrame 可用 `CreateAnimationController`+三参 `StartAnimation` | 弹簧**禁止**依赖；关键帧可用 Progress 反解值。KeyFrame Progress 行为仍部分待验 |
| XAML DP（如 `element.Translation`） | 合成器动画绕过 XAML 属性系统 | — | 预期不反映动画值；**待真机验证**，不依赖 |
| `ExpressionAnimation("v.Translation.X")` → PropertySet → `TryGetScalar` | — | 熄屏下表达式不重求值（恒 0） | 试点否定；亮屏行为**待真机验证**，不作设计依赖 |
| Windows.Graphics.Capture 像素 | 合成器真实输出 | 熄屏无帧 | **亮屏唯一 ground truth**（§3.1） |

### 2.4 影子状态（弹簧打断 / 交接主路径）

`CompositionDispatcher` 为每个 `(target, property)` 维护影子状态 `(value, velocity, tLast)`：
- 每次 `Submit` / Retarget 前用 `spring.h::At`（或关键帧 `EvaluateKeyframes` + `bezier.h::Slope`）从 `tLast` 外推到 `snapshot.clockTime`，得到确定性 **InitialValue / InitialVelocity** 种子；
- **弹簧打断验收（已裁决）**：以**影子状态种子**为预测基准，对照**时间对齐后的 WGC 轨迹**（见 §3 滞后说明）；**不依赖** `TryGetAnimationController` / `TryGetScalar`（NaturalMotion 无控制器；PropertySet 读回不可靠）；
- 合成器→软件切换：`StopAnimation` 后可尝试物化读回（§2.3）；若与影子偏差 > 阈值或失败 → 用影子状态 `fm_animation_retarget(..., hasFrom=1, from=影子值)` 交接。

---

## 3. 可靠测量合成器路径

### 3.1 Windows.Graphics.Capture（像素级，主通道）

- **WinUI3 路径（已真机核实，workbuddy）**：`GraphicsCaptureItem.CreateFromVisual` **只接受** `Windows.UI.Composition.Visual`，**不能**用于 WinUI3 的 `Microsoft.UI.Composition.Visual`（`ElementCompositionPreview.GetElementVisual` 返回后者）。改用 [`IGraphicsCaptureItemInterop::CreateForWindow`](https://learn.microsoft.com/en-us/windows/win32/api/windows.graphics.capture.interop/nf-windows-graphics-capture-interop-igraphicscaptureiteminterop-createforwindow)（HWND）截整个窗口，再按目标元素的屏幕矩形裁切到 AnimBox 区域。`CreateFromVisual` 仅适用于 UWP/`Windows.UI.Composition` 场景，本项目不采用。
- `Direct3D11CaptureFramePool.Create(d3dDevice, Bgra8, 2, size)` → `pool.CreateCaptureSession(item)` → `StartCapture()`；`FrameArrived`：`frame.Surface` → `CreateDirect3D11SurfaceFromDXGIResource` → `CopyResource` 到 STAGING → `Map` → 在裁切区域内扫蓝色块 bbox（质心/边梯度亚像素）→ `DIP = px / XamlRoot.RasterizationScale`。
- 时间戳：`frame.SystemRelativeTime`（QPC 域）与 `Stopwatch.GetTimestamp()` / `StartAnimation` 时刻对齐，逐帧记录 `(t, x)`。
- **呈现 / 截屏滞后（已真机观察，workbuddy）**：中途呈现或 WGC 截屏相对动画时钟可能有约 **160–200 ms** 滞后。验收与轨迹比对必须用**时间对齐后的轨迹**（估计并扣除滞后，或相对事件对齐），**禁止**用未对齐的原始时间戳与闭式解 / 影子状态硬比。
- **亮屏**：逐合成帧出帧，`DwmFlush`/WGC ≈ 60 Hz 下可测到帧级轨迹（对齐后）。
- **熄屏（已真机观察）**：WGC 仍可约 **46 fps** 到达帧，但**画面冻结** → **不能**仅凭「有帧到达」判有效；须叠加位移门槛（见 §3.2 / §3.7 / §4）。
- 局限：窗口级捕获含非目标像素，须裁切；文本等 XAML 混合渲染在裁切后的可测性**待真机验证**。

### 3.2 DComp 帧统计与有效性门槛（合成器时钟探针，C++ 侧）

| API | 说明 | WinUI3 / 本机可拿性 |
|---|---|---|
| `IDCompositionDevice::GetFrameStatistics(DCOMPOSITION_FRAME_STATISTICS)` | 该设备 lastFrameTime / currentCompositionRate / nextEstimatedFrameTime | XAML 合成器的 `IDCompositionDevice` **无公开互操作**（预期拿不到，**待真机验证**确认）。可自建设备（`DCompositionCreateDevice` + D3D11），但需它自己有提交（挂一个哑视觉 + 定时提交）统计才会推进（**待真机验证**） |
| `DCompositionGetFrameId(COMPOSITION_FRAME_ID_TYPE, COMPOSITION_FRAME_ID*)` | **全局**最新合成帧 id，无设备参数，Windows build **22000+**（[DCompositionGetFrameId](https://learn.microsoft.com/en-us/windows/win32/api/dcomp/nf-dcomp-dcompositiongetframeid)） | **已真机核实（workbuddy）**：本机 **Win10 19045** 返回 `E_NOTIMPL`。仅在 OS build **22000+** 启用为探针 |
| `DCompositionGetStatistics(frameId, COMPOSITION_FRAME_STATS*, …)` | 指定帧的统计，build 22000+（[DCompositionGetStatistics](https://learn.microsoft.com/en-us/windows/win32/api/dcomp/nf-dcomp-dcompositiongetstatistics)） | 同上：Win10 19045 不可用；**22000+** 才启用 |

**已裁决：Win10 / 通用有效性门槛**（缺一不可；失败则整次运行 `invalid`，而非 fail）：

1. **`DwmFlush` 分布**：观察窗内多次 `DwmFlush` 等待时间的分布接近刷新周期（亮屏复测 ≈ **60 Hz / ~16.7 ms**）。用于判断 DWM 是否按真实刷新推进；
2. **WGC 有帧**：`CreateForWindow` 会话在观察窗内收到帧（亮屏复测 ≈ **60 Hz**；熄屏仍可到帧——**不够**单独判有效）；
3. **位移门槛（frozen-frames）**：某段已知行程的动画期间，WGC 裁切区域内方块位移（峰值−起点，DIP）**≥ 行程的 10%**；否则判 `INVALID(frozen-frames)`。依据：熄屏时 WGC 仍约 **46 fps** 到帧但画面冻结。

**不要**用 `DwmGetCompositionTimingInfo.cRefresh` 作门槛：**已亮屏复测（workbuddy）** — 本机 Win10 19045 上即使亮屏 `cRefresh` 也几乎不涨，不可靠。`qpcVBlank`/`qpcCompose` 可作诊断旁证，但不作为放行条件。

帧 id 探针仅在 **22000+** 额外启用（旁证）。亮屏预期：`DwmFlush`≈60 Hz、WGC≈60 Hz、位移门槛通过；熄屏预期：位移门槛失败（常见），`DwmFlush` 分布异常/变稀。

### 3.3 CompositionTarget.Rendering

XAML 渲染帧回调（`RenderingEventArgs.RenderingTime`）。

- **已亮屏复测（workbuddy）**：即便 `DwmFlush` / WGC ≈ **60 Hz**，`CompositionTarget.Rendering` 仍常为 ≈ **31 ms**（WinUI 节流 / 与合成器时钟解耦）。熄屏试点亦曾见 ~31.2 ms。
- **用途**：软件路径帧泵、UI 线程负载观察。
- **禁止**：把 Rendering 间隔当作合成器路径是否「有效 / 60 Hz」的代理；合成器验收以 WGC 轨迹 + `DwmFlush` 分布为准（见 §3.2）。

### 3.4 DwmFlush 与 DwmGetCompositionTimingInfo

- **`DwmFlush`（门槛主通道，已亮屏复测）**：调用后阻塞至 DWM 处理完一帧；亮屏等待分布 ≈ 60 Hz。用作 §3.2 有效性门槛第 1 条。
- **`DwmGetCompositionTimingInfo`**：`DWM_TIMING_INFO`（`cRefresh` / `qpcVBlank` / `qpcCompose` / `rateCompose`，dwmapi）。**已亮屏复测（workbuddy）**：Win10 **19045** 上 **`cRefresh` 即使亮屏也几乎不涨 → 不可靠，禁止作门槛**。`qpcVBlank`/`qpcCompose` 仅诊断旁证。22000+ 可再叠加帧 id。

### 3.5 CompositionScopedBatch 计时

官方语义：Completed = “batch 内动画与效果全部完成”（[CompositionScopedBatch.Completed](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.compositionscopedbatch.completed)）。试点熄屏实测 1000 ms 关键帧 batch **13.6 ms** 即 Completed 且读到终值 100、弹簧 batch 直到 29.9 ms 才随他人触发——与官方语义矛盾，唯一自洽解释是**熄屏时合成器动画时钟不按真实时间推进**。用途：亮屏下测提交→处理往返与完成信号（动画本身无 Completed 事件）；熄屏数据一律标注无效。**亮屏行为待真机验证**。

### 3.6 亮屏 vs 熄屏可信度对照

| 方法 | 亮屏 | 熄屏（无 vblank） | 可信度 |
|---|---|---|---|
| WGC 像素轨迹（`CreateForWindow`+裁切） | ≈60 Hz 轨迹（已复测） | 仍可到帧（约 46 fps）但画面冻结 | 高（真值）；**须**叠加位移≥10%行程 |
| WGC 帧数 alone | ≈60 Hz | 有帧（误导） | **不足**单独作门槛 |
| `DwmFlush` 分布 | ≈60 Hz / ~16.7 ms（已复测） | 异常/变稀（预期） | 高（**Win10 门槛主通道**） |
| 位移门槛（峰值−起点 ≥10%行程） | 通过 | 失败 → `INVALID(frozen-frames)` | 高（熄屏鉴别） |
| `DwmGetCompositionTimingInfo.cRefresh` | 几乎不涨（已复测） | — | **不可靠，禁止作门槛**（Win10 19045） |
| `qpcVBlank` / `qpcCompose` | 可作诊断 | 停走或降频（待验） | 低–中（旁证，非放行） |
| DCompositionGetFrameId / GetStatistics | 仅 **22000+** | 待验证 | 高（22000+ 旁证；Win10=`E_NOTIMPL`） |
| CompositionTarget.Rendering | 仍 ≈ **31 ms**（WinUI 节流，已复测） | ≈31.2 ms（已实测） | **不能**作合成器路径有效性代理 |
| ScopedBatch | 正常 | 时钟异常快进（已实测） | 中（仅提交延迟/完成信号） |
| TryGetScalar / TryGetVector3 | 待验证 | 恒静态值（已实测） | 低 |
| PrintWindow / CopyFromScreen | 可用 | 全白/找不到像素（已实测） | 低（不采用） |
| `CreateFromVisual`（WinUI3 Visual） | — | — | **不可用**（已核实） |

### 3.7 具体测量流程

1. **门禁（已裁决）**：启动后观察窗（约 2 s）检查：
   - **`DwmFlush` 分布**是否接近刷新周期（亮屏 ≈60 Hz）；
   - WGC（`CreateForWindow`）是否收到帧（亮屏 ≈60 Hz；仅旁证）；
   - **不要**读 `cRefresh` 作放行（Win10 19045 亮屏也不涨，已复测）；
   - OS build **≥22000** 时额外轮询 `DCompositionGetFrameId`（旁证）；**Win10 19045 不调用帧 id**（`E_NOTIMPL`）。
   `DwmFlush` 分布异常且无合理帧 → 可打 `invalid(display-asleep)`；仅有 WGC 帧**不能**放行。`CompositionTarget.Rendering` **不参与**门禁。
2. **标定**：静态写 `Translation.X:=37`，WGC（窗口捕获+裁切到 AnimBox）验证像素位移与 DIP 换算（scale、bbox 检测阈值）。
3. **场景矩阵**（每场景独立起停）：弹簧 0→240（snappy 0.32/0.78）；150 ms 打断→0；弹簧 rate=2；贝塞尔 0→240@220 ms；hold；反向关键帧（若 K3 可用）。
4. **记录**：`StartAnimation` 调用时刻（QPC）→ WGC 逐帧 `(SystemRelativeTime, px)` + `DwmFlush` 样本 + ScopedBatch 提交延迟；可附带 Rendering 间隔作对照（**不**作有效性依据）。记下估计的呈现/WGC 滞后（常见 ~160–200 ms）。日志先内存后落盘。
5. **frozen-frames 门槛（每动画段）**：对该段行程 `|target−from|`，取 WGC 轨迹位移幅度 `max(x)−x₀`；若 **< 10%·行程** → `INVALID(frozen-frames)`。通过后才进入离线比对。
6. **离线比对**（仅有效运行，**须时间对齐**）：将 WGC 时间轴扣除呈现/截屏滞后（或按事件对齐）后，再 vs 影子状态 / `spring.h` / `EvaluateKeyframes` → RMS、最大偏差、终值误差。**弹簧打断**：影子种子 vs 对齐后 WGC（**不**用 TryGet）。S1 v2：`Period = r/(2π)`，影子 `spring.h(r, ζ; t − τ)`（v1 `Period=response` 已作废）。
7. **重复**：亮屏有效运行 ≥5 次；熄屏/冻结 1 次仅验 `invalid` 路径。

---

## 4. 验收标准（量化）

> **说明（2026-10-20）**：下表数值**不放宽**（含轨迹形状 RMS）。弹簧形状未达 1% 门槛，已结案为 **NaturalMotion vs spring.h 固有差异**（§4.1 / §5.3），**不**据此改 S1 映射、**不**放宽门槛。合成器有效性不以 Rendering 为准；弹簧打断用影子种子 + 时间对齐 WGC；呈现/WGC 滞后 ~160–200 ms 须对齐。

| 指标 | 阈值（60 Hz 亮屏） | 备注 |
|---|---|---|
| 启动延迟 | p50 ≤ 2 帧（33 ms），p95 ≤ 3 帧（50 ms），上限 200 ms | `StartAnimation`（QPC）→ WGC 首个 `|Δpx| ≥ 0.5` 帧 |
| 打断位置连续性（弹簧） | `|Δv| ≤ 0.5 px` 且 ≤ 5% 该帧正常位移 | **影子状态种子** vs **时间对齐后 WGC**；**不依赖 TryGet** / AnimationController |
| 打断速度连续性（弹簧） | `|Δvel| ≤ max(20 px/s, 5%·|v|)`；无方向反向尖峰 | 同上（影子速度 vs 对齐后轨迹数值微分或段拟合） | <!-- M1b 打断场景以 §5.3 R-2′ 底噪相对门为准；本行绝对门为 M2 目标（见 notion_r2_b_gate_ab.md） -->
| 终值误差 | 完成+1 s：`|v−target| ≤ 0.1 px`（Withdraw 快照后应 = 0） | 无快照时允许 ≤ 0.5 px（S5 差异，待测后收紧）；比较前须时间对齐 |
| 轨迹偏差（弹簧/关键帧） | RMS ≤ 1%·行程，max ≤ 2%·行程（行程=|target−from|） | **对齐后** WGC vs spring.h / EvaluateKeyframes；弹簧 **S1 v2** `Period = r/(2π)`（v1 `Period=response` 下的 28.6%/12.1% 已作废，仅历史对照；R-1 判据见文末「R-1 判据（v2）」）。贝塞尔形状已过；**产品/S1 残差 ≈28.6%（A）/ 12.1%（E）**（扫参非产品变体最低 ≈**8% / 6%**，次要）—**未过**。**已结案（固有差异 / R-1 known-fail）**（见 §4.1 / §5.3）；**门槛不放宽**；不调 Period/ζ 以求过关 |
| 帧间隔（CompositionTarget.Rendering） | （参考）亮屏常见 ≈31 ms | **非**合成器验收项；勿与 16.7 ms 门槛混用 |
| 稳定性 | 5 次运行 0 崩溃/0 异常；Rendering 订阅=退订；软件↔合成器模式切换 100% 成功且交接 `|Δv| ≤ 0.5 px` | |
| 熄屏 / 无效合成 | 判 **invalid 而非 fail**（`invalid(display-asleep)`：`DwmFlush` 分布异常；`INVALID(frozen-frames)`：位移 < 10%·行程——即使 WGC 仍有帧）。**不以** `cRefresh` 判定。附 `DwmFlush`/WGC/位移证据；22000+ 可附帧 id。应用不挂死（ScopedBatch 超时保护） | 见 §3.2、§3.7 |

### 4.1 亮屏进展 / 开放风险 — **已结案（固有差异）**（派题 #13）

| 项 | 状态（亮屏 VALID 门通过后） |
|---|---|
| 打断位置 / 速度主指标 / 终值 / 贝塞尔形状 | #11–#12 曾记**已过**；**#25 单τ 重测未达 §4** → **R-2 本机 FAIL 结题（#47；M1b 降级实现 B_v，不满足 R-2；见 §5.3.7）**（#33 的 207 DIP 为旧映射，已作废）（#25 的 |Δpos|≈20–28 采样过晚已作废，见 §5.3）；§4 不放宽 |
| 弹簧轨迹形状（场景 A / E） | **未过**：**产品/S1 残差** best-τ 形状 RMS ≈ **28.6% / 12.1%**；扫参非产品变体最低 ≈ **8% / 6%**（门槛 1%；勿把后者当产品残差） |

**映射扫参（派题 #13，阈值未放宽）**

| 变体 | 含义 | A / E 形状 RMS |
|---|---|---|
| (a) | `Period=response=0.32`，ζ=0.78（S1 v1，已作废） | 28.6% / 12.1% |
| (b) | settle 反推 T₀：`SettleTime(T₀,0.78,0→240)=0.32` → T₀≈0.1512；E 用 T₀/2（**反事实**：spring.h 的 response 本已是 T₀） | 9.0% / 6.5% |
| (c) | 取 (b) Period，ζ∈{0.68,0.73,0.83,0.88} | 最低 **8.05% / 6.02%**（ζ=0.68） |

- ζ±0.05（及更密）相对 (b) **无清晰大幅下降**（最大约 −0.7 pp）。
- **无一变体 ≤1%** → **已结案**：记风险 **「NaturalMotion 弹簧与 spring.h 固有差异」**（非单纯 Period/DampingRatio 换算错误；亦称「合成器弹簧与 spring.h 固有差异」）。
- **产品映射仍保持 S1**（`Period=response`，`DampingRatio=ζ`）（**v1，已被 S1 v2 取代**：2026-10-20 裁决 A1 改为 `Period = r/(2π)`，见 [notion_s1_remap_ab.md](./notion_s1_remap_ab.md)）；不因 (b) 形状略优而改 Period 语义（峰峰仍支持无阻尼）。**§4 形状/轨迹数值门槛不放宽**。
- 本条原为「待决 / 待扫参」；扫参结案后见 §5.3（已从 §5.2 待验证移出）。


### 5.1 已核实（workbuddy 真机 / 亮屏复测）

1. **不存在 `TryStartAnimation`**。
2. **WGC `CreateFromVisual`**：不能用于 WinUI3 `Microsoft.UI.Composition.Visual`；改用 `CreateForWindow(HWND)` + 裁切。
3. **`DCompositionGetFrameId` / `GetStatistics`**：Win10 19045 = `E_NOTIMPL`；仅 22000+ 启用。
4. **熄屏 WGC 冻结帧**：约 46 fps 到帧但画面冻结 → 须位移≥10%行程，否则 `INVALID(frozen-frames)`。
5. **S1 Period = 无阻尼周期**（**v1，已被 S1 v2 取代**：2026-10-20 裁决 A1 改为 `Period = r/(2π)`，见 [notion_s1_remap_ab.md](./notion_s1_remap_ab.md)）：`Period = response`，`DampingRatio = ζ`；峰峰 ≈ 0.511 s（不是 0.320 s）。
6. **`cRefresh` 不可靠**：亮屏也不涨 → 禁止作门槛；改用 `DwmFlush` + WGC 帧数 + frozen-frames。
7. **Rendering ≠ 合成器时钟**：亮屏 `DwmFlush`/WGC ≈ 60 Hz，Rendering 仍 ≈ 31 ms → 不作合成器有效性代理。
8. **`TryGetAnimationController` / NaturalMotion**：对**弹簧（NaturalMotion）按设计恒为 `null`**；弹簧路径**不要**用 TryGet 拿控制器。控制器能力**仅**属于 **KeyFrameAnimation**。
9. **KeyFrame 控制器**：**`CreateAnimationController` + 三参 `StartAnimation`**（带 `AnimationController` 的重载）可用于贝塞尔/关键帧路径（已核实）；与弹簧 NaturalMotion 明确区分。
10. **呈现/WGC 滞后**：中途呈现或截屏可能有约 **160–200 ms** 滞后；验收用**时间对齐后的轨迹**。
11. **（指针）固有差异已结案** → 见 **§5.3 / §4.1**（扫参后从待验证移出，不占待验名额）。
12. **S2 `DampingRatio` 接受域（派题 #14）**：ζ≥0 接受且不钳；ζ<0 抛 `ArgumentException`；过阻尼可跑（WGC）。证据 `artifacts/win_zeta_gt1_probe.txt`。
13. **S3 `InitialVelocity` 单位（派题 #15）**：值为 **DIP/真实秒**，与 Period 独立；证据 `artifacts/win_s3_velocity_probe.txt`。rate 折叠（period/rate、v×rate）抽查 RMS≈13.6%，残差≈固有差异量级——公式保留，不作否决。
14. **S4 DelayTime/presetValue（派题 #16）**：默认 AfterDelay 延时期间保持 preset；IV==preset 无跳变；IV≠preset 结束时跳到 IV。证据 `artifacts/win_s4_delay_probe.txt`。
15. **S5 停点/保持（派题 #17）**：到位后 WGC≥500ms 保持 ≈FinalValue（|err|≈0.5）；~85% 处 StopAnimation 冻结不继续到终点。ScopedBatch.Completed 相对视觉到位偏晚——NaturalMotion 不作完成门控。证据 `artifacts/win_s5_settle_probe.txt`。
16. **K5 StopAnimation/TryGet（派题 #18）**：弹簧与 KeyFrame 中段 Stop → WGC 冻结（range≈0）。TryGetVector3 立刻常读 0/种子，保持片刻后才≈冻结值——**不可**作同步读回。产品：Stop 后显式 SetValue。证据 `artifacts/win_k5_stop_probe.txt`。
17. **K1 easing 作用段（派题 #19）**：结束关键帧 easing 塑造该段；WGC vs BezierEase RMS≈0.57%（线性基线 36.6%）。证据 `artifacts/win_k1_easing_probe.txt`。
18. **K2 y∉[0,1]（派题 #20）**：**HONOR**（非 reject/clamp）；`Capabilities::bezierOvershoot` 默认 **true**。证据 `artifacts/win_k2_overshoot_probe.txt`。
19. **K4 KeyFrame 终值保持（派题 #21）**：完成后 WGC≥500ms median=100、range=0。证据 `artifacts/win_k4_hold_probe.txt`。
20. **K3 Progress/PlaybackRate（派题 #22+#28，已核实）**：Progress 立即生效但是时间进度非数值线性；PlaybackRate=2 约半时长、−1 可反向。证据 `artifacts/win_k3_controller_probe.txt`。
21. **ζ>1 轨迹（派题 #23，PASS；门控不抬）**：过阻尼单调、overshoot=0；ζ1.5 arrive≈4.5s / ζ3≈8.9s（peak 240 vs 124）可区分；合成器 settle≪ spring.h 比值≈3.7。证据 `artifacts/win_zeta_gt1_traj_probe.txt`。v1 仍不 lower。
22. **rate 折叠对照（派题 #24）**：同会话 rate=1 RMS≈50（21%）vs rate=2 RMS≈35（14%），Δ≈−16 → **同量级**；公式可留，产品默认仍 rate=1。证据 `artifacts/win_rate_fold_probe.txt`。
23. **打断单τ（派题 #25）**：PRE 拟合τ冻结 post；\|Δpos\|≈20–28（tInt+τ 取样，已被 #33 作废；可见跳变 ≈207 DIP）、\|Δvel\|≈156–183，未达§4；seedLead≈16.7ms 略优。证据 `artifacts/win_interrupt_seed_probe.txt`。门控不改。
24. **Period 极小值（派题 #26）**：0.001/0.01/0.05/0.32 **ACCEPT**（读回=设定、不钳；≥0.01 可见运动；0.001 近瞬时）；`TimeSpan.Zero` **THROW** ArgumentException（cannot be 0）；负 Period（−0.01）赋值接受。产品建议规划器 **Period>0** 门控（Zero 必炸；负值未定义）。**已落地**：`composition.h` 在 PlanOverride/PlanGraph 对**作者参数** `response>0` 门控（须在 spring.h 钳到 1e-4 之前检查）；composition_tests 2076/0。证据 `artifacts/win_period_min_probe.txt`。
25. **InitialValue=null（派题 #27，仅记录）**：**FROM_CURRENT** — 不设 IV 时从被动画属性当前值（preset≈37）平滑起跳；对照 IV=0 约 28ms 跳到 0。产品打断仍一律显式 seed，**不依赖** null 语义。证据 `artifacts/win_iv_null_probe.txt`。
26. **K3 剩余（派题 #28）**：① Delay 内 Progress=0.5 → WGC≈90（SEEK_HONORED，与 #22 时间归一化一致）；② PlaybackRate=±20 → **CLAMP@±16**（不抛；arrive≈99ms / 反向 OK）；③ Progress=0.75 + rate=-1 → 从≈97.5 反向到 0。证据 `artifacts/win_k3_rest_probe.txt`。规划器 |rate|>16→AppThread 仍合理（合成器会钳到 16）。
27. **StartingValue 首帧（派题 #29）**：**FROM_STARTING_VALUE** — `InsertExpressionKeyFrame(0,"this.StartingValue")` 从当前值（40）平滑到终值、无跳 0；显式帧0=0 会 JUMP；Stop 后重提约从 87 续。证据 `artifacts/win_starting_value_probe.txt`。产品 KeyFrame 首帧可采用表达式。
28. **harness 底噪（派题 #30）**：0.25s 线性 0→240 对齐后 RMS≈**1.64**（**0.682%** travel）；0.32s≈**0.534%**。**≪** 弹簧固有残差 12–28% → 测量下限低，R-1 形状残差为真。证据 `artifacts/win_harness_noise_probe.txt`。

### 5.2 仍待真机验证

1. **S2（残余）**：ζ=0.78 包络拟合交叉验证。（ζ>1 接受/不钳、ζ<0 抛异常 → 已核实，见 §5.1#12；**ζ>1 轨迹保真 → 已核实 PASS，见 §5.1#21，门控不抬**。）
2. **~~S3（残余 rate 折叠）~~** → **同量级（派题 #24 / §5.1#22）**：同跑 rate=1 RMS≈21% vs rate=2≈14%，Δ 为负；公式保留，产品默认仍 rate=1。单位与 Period 独立性见 §5.1#13。
3. **~~S4~~** → **已核实**（§5.1#14）。`SetInitialValueBeforeDelay` 仅作记录，无额外产品依赖。
4. **~~S5~~** → **已核实**（§5.1#15）。到位后保持 FinalValue；中段 StopAnimation 冻结；NaturalMotion 上 ScopedBatch.Completed 偏晚不可靠（仅记录）。
5. **~~K1~~** → **已核实**（§5.1#17）。结束关键帧上的 easing 塑造该段；单段 RMS≈0.57% vs 线性 36%；双关键帧支持段局部 easing。
6. **~~K2~~** → **已核实**（§5.1#18）。y∉[0,1] **HONOR**；`bezierOvershoot=true`。
7. **~~K3~~** → **已核实（#22+#28 / §5.1#20/#26）**：Progress 立即生效且时间归一化（0.5→≈90）；Delay 内 Progress 仍 SEEK；PlaybackRate± 可用；|rate|>16 **CLAMP@±16**；Progress+反向 OK。|rate|>16 产品仍可回 AppThread。
8. **~~K4~~** → **已核实**（§5.1#19）。KeyFrame 完成后保持终值（|err|=0）。
9. **~~K5~~** → **已核实**（§5.1#16）。弹簧/KeyFrame 中段 Stop 均冻结；TryGet **非**可靠同步物化（立刻常为种子/0，稍后才到冻结值）→ 交接用 **Stop + 显式 SetValue（影子/WGC）**。
10. **~~InitialValue=null~~** → **FROM_CURRENT（#27 / §5.1#25）**：不设 IV 用当前属性值；对照 IV=0 会跳变。仅记录，产品不依赖。
11. `CompositionPropertySet.TryGetScalar` / `Visual.Properties.TryGetVector3` 对正在动画属性的读回（亮屏）。
12. `ExpressionAnimation` → PropertySet → `TryGetScalar` 逐帧读回（亮屏）。
13. `CompositionScopedBatch.Completed` 在亮屏下是否等于动画时长完成。
14. **~~rate>16 PlaybackRate~~** → **CLAMP@±16（#28）**；不抛。NaturalMotion 无控制器——已核实。
15. WGC 亮屏捕获内容范围、SystemRelativeTime 对齐；位移门槛误杀率；**滞后估计方法**的稳定性（160–200 ms 量级）。
16. build **22000+** 上帧 id API 语义与熄屏行为；自建 `IDCompositionDevice` 的 `GetFrameStatistics`。
17. `DwmFlush` 熄屏等待分布；`qpcVBlank`/`qpcCompose` 诊断旁证（不恢复 `cRefresh`）。
18. WinUI3 能否取得 XAML 合成器的 `IDCompositionDevice`（预期不能）。
19. NaturalMotion 亮屏轨迹与刷新率关系；与 S1/影子种子对齐后的定量偏差收紧。
20. **~~StartingValue 首帧~~** → **FROM_STARTING_VALUE（#29 / §5.1#27）**：表达式首帧无跳变；显式 0 会跳；Stop 后重提可续。
21. **~~弹簧 rate 折叠~~** → **同量级（#24）**：rate=1/2 同会话对照，额外误差未高于基线；公式保留；产品默认 rate=1。详见 §5.1#22。
22. **~~Period 极小值~~** → **已核实（#26 / §5.1#24）**：≥0.001 ACCEPT 不钳；Zero THROW；负值赋值接受。DampingRatio 全范围见 #14；产品建议 Period>0 门控。
23. `StartAnimation` 隐式替换时的值连续性（与 K5 交叉）；弹簧替换不经过 Controller。

### 5.3 已结案风险（非待验证）

1. **NaturalMotion 弹簧与 spring.h 固有差异（派题 #13，已结案；Claude FINAL 确认为 R-1 known-fail）**：**产品/S1 残差 ≈ A 28.6% / E 12.1%**（须以此为准）；**harness 线性底噪 ≈0.5–0.7%（#30）**，故残差非测量噪声；扫参非产品变体最低仍约 **A 8% / E 6%**，**无一组 ≤1%**。记风险「固有差异」（非换算错误）。产品映射仍 **S1**（`Period=response`，`DampingRatio=ζ`）（**v1，已被 S1 v2 取代**：2026-10-20 裁决 A1 改为 `Period = r/(2π)`，见 [notion_s1_remap_ab.md](./notion_s1_remap_ab.md)）；**§4 形状门槛不放宽**；不调 Period/ζ 以求过关。详见 §4.1。（原 §5.2 第 24 条「待决」已结案移入此处。）

#### 5.3 R-2 打断续接（R-2）——seed 与验收门（R-2′）

##### 5.3.1 Seed（hybrid B，flag `interrupt.hybridB`，默认 ON）
- InitialValue：不设（null），由合成器取动画中当前值（#34 P1 已证实）。
- InitialVelocity：v = (x2 − x1)/(t2 − t1)，取打断前 WGC 末两帧；x 为 DIP，t 为帧到达 QPC。
- 禁止使用 spring.h 影子 x(t)/v(t) 作 seed（#37：影子速度约为屏幕 2.6×；A 臂可见跳变约 100–207 DIP）。
- 速度可信性守卫，任一不满足即降级：
  - WGC 可用；
  - 末帧距打断时刻 ≤ 2 个帧周期；
  - dt12 ∈ [0.5, 2] × 帧周期。
  降级 = IV=null + InitialVelocity=0；计数并写日志；永不回退影子 seed（A）。
- Kill-switch：flag OFF ≡ 降级路径，不是 A。
- 加速度外推（B′）已淘汰，不实现。
- 不变项：seedLead=0；interruptible 保持 opt-in，不降级；refit-τ 不作验收；composition.h 不改。

##### 5.3.2 验收门 R-2′（底噪相对门）
- 底噪 N_h：同会话 ≥20 次不打断基线，沿用 #35 的底噪算法；每次会话重测并写入结果头。
  N_h > 4 DIP，或超过上一有效会话 2× → 会话作废。
- ①′ 单次：J_vis(run) ≤ G_J(run) = d_frame(run) + N_h，其中 d_frame = |x2 − x1|（打断前末两帧位移，DIP）。
  逐格通过率 ≥ 95%（n=20 时 ≥19/20）；dbl 格冻结验证要求 n ≥ 60 且 0 失败。
- ②′ 相对 A：7 格合并后，median(J_X)/median(J_A) 的 95% bootstrap（4000 次）CI 上界 ≤ 0.25；逐格点估计 ≤ 0.25。
  原逐格“CI 不重叠”判据作废（A 分布双峰）。
- ③′ Δv：逐格 median|dvel|_X ≤ median|dvel|_A；合并 median|dvel|_X ≤ 0.25 × median|dvel|_A。
- M2 目标（不作 M1b 验收）：J_vis ≤ 0.5 DIP 且 ≤ 5% 帧位移；|dvel| ≤ max(20, 5%|v|)。
  前者须待 harness 底噪 < 0.5 DIP 后启用。
- L（首个打断后帧的延迟）：只记录，不作门槛。

##### 5.3.3 现状与冻结前置
- #35（DESKTOP-K93N3P8，440 次）：B 的①′ 7 格均为 20/20；③′逐格通过，合并 58 ≤ 89.75；②′待用原始日志重算。
- 冻结前置（C5）：
  - (a) #35 重算②′通过；
  - (b) 第二台机器（不同刷新率，如 120/144 Hz）补测 t156dbl（n≥60）与 t060rev（n≥20），按 R-2′ 通过。
- C5 未过 → flag 默认改 OFF，R-2 作为限制项保持开启，B 可 opt-in。

##### 5.3.4 估计器 A5（#43 起；出处 notion_r2_estimator_ab.md B 节）
> 其中「负 lead 扫描 / Am_X」已被 5.3.5 §6 否定，仅作历史记录。

##### A5（#43 起生效，替代 A4 中的 J/Δv 估计器；R-2′ 阈值结构不变）
- 估计器 E2：打断前 = 已知弹簧解析式 + 每次拟合屏幕延迟 δ（0.5 ms 网格）；打断后 = 已知弹簧对 (e0,v0) 线性 LS（k_c 起 2–6 帧）；切换时刻 t_s = W=[τ_{k_c−1},τ_{k_c}] 内 |x_post−x_pre| 最小点；J、Δv 均在 t_s 取，不做帧差分，不用线性或 2 阶外推。
- 零假设：同会话 base（n=20）同 k_c、同 W，打断后目标取 T_old；N_h_cell=J_null 第 19 小，N_h_eff=max(N_h_cell,1.0)；Nv_h_cell=Δv_null 第 19 小。
- 门：N_h_cell≤4；Δv 阈=max(20,5%|v_pre(t_s)|)；Nv_h_cell>阈中位 → 该格 J-only（最高 J-PASS）；base ≥3/7 格未过 J 门或 P0 失败 → 会话作废。
- P0 标定：会话前 40 次 base 拟合 δ，Δt̂=中位；IQR>8 ms 或残差 RMS 中位>1 DIP → MODEL-MISMATCH，停止。
- 负 lead 扫描：Am_X 种子 = spring.h(r) 在 tInt_c−X 的 (x,v)，X∈{39, Δt̂−15, Δt̂, Δt̂+15}，P0 失败时 Δt̂ 取 70；臂 = A0、Am×4、B，每轮随机交错。
- 臂通过 = 可判格全 PASS 且可判格 ≥5/7；L* 由 J、Δv 中位对 X 的二次拟合求得，与 Δt̂ 相差 ≤8 ms 记为一致。
- 交叉核对：base 中位轨迹替代 x_pre 后 J 中位差 >1 DIP 标 ⚠E；打断后拟合残差 RMS>1 DIP 标 ⚠M；⚠ 格不参与选主。

##### 5.3.5 修订 r2-est-v3（#43；出处 notion_r2_43_ab.md B 节）

##### 修订记录 r2-est-v3（依据 #43）

###### 0. 优先级
- 文档与派单冲突时以文档为准；执行方须在结果头注明冲突项及采用口径。

###### 1. 第1步 ω₀ 口径修正（勘误）
- 正确：ω₀ = 2π/r = 1/Period，Period = r/(2π)。
- 原文“ω₀=2π/Period”为笔误，代入得 4π²/r（本构建 123.4 rad/s），与 #36/#41 不符，作废。
- 本构建（composition.h a55d4952…e418，MapSpringToCompositor）：Period=0.0509296 → r=0.3200 s，ω₀=19.635 rad/s；ζ=0.78 → ω_d=12.29 rad/s。

###### 2. 第2步 P0 标定修订
- 原 IQR≤8 ms 门作废：在 60 Hz 下，提交量化误差 U[0,T) 单独贡献的 IQR 已有 T/2=8.33 ms。
- 新流程：
  1. 每次打断前，先对齐合成器时钟，再等待到目标相位 φ*∈{2,6,10,14} ms。
  2. 记录 φ_m=(tInt_m−lastFrameTime) mod T。
  3. 拟合 δ̂ = a + b·((T−φ_m) mod T)，记残差 r。
- 门：b∈[0.8,1.2]；每层 IQR(r)≤4 ms；全体 1.4826·MAD(r)≤3 ms；残差 RMS 中位≤1 DIP。
- 失败：记 MODEL-MISMATCH，会话不出判定；其余内容只作诊断并标 DIAG；不设默认 Δt̂。
- σ_t := r 的稳健标准差，供第 4 节加权使用。

###### 3. 种子时刻
- tInt_m := 替换命令 Commit 返回时的 QPC；同时记录 tInt_c、tInt_m、t̂_apply（tInt_m 后第一个合成器帧的预测时刻）。
- A0 种子取在 tInt_m；Aφ 种子取在 t̂_apply。
- #43 实测 tInt_m−tInt_c = 8–19 ms，归因于提交落点的帧相位。

###### 4. 打断后估计器 E3（取代“2–6 帧线性 LS”）
- 帧范围：k_c+1…k_c+5。k_c 帧与打断前模型偏差≤1.5 DIP 时归入打断前，否则剔除。
- 权重：wᵢ = 1/(σ_p² + vᵢ²σ_t²)，σ_p=0.5 DIP。
- ⚠M := χ²/dof > 2（不再用固定的 1 DIP 门）。
- M0：按指令的 ω₀、ζ、目标值，(e0,v0) 加权 LS。
- M1：M0 + 常数偏移 c。
- M2（仅诊断）：网格 s∈[0.8,1.25]（ω₀缩放）、ζ∈[0.6,1.0]；M2 明显优于 M0 时记 PARAM-MISMATCH。
- 禁止加“自由起始相位”：参数固定时，它只是 (e0,v0) 的重参数化，属退化自由度。
- t_s := tInt_m 后第一个合成器帧时刻；Δv = v_post(t_s) − v_pre(t_s)。只用 M0 通过、或 M1 通过且 |c|≤1 DIP 的运行。
- 格级：⚠M≤2/20 才判 Δv，否则仅判 J。

###### 5. 零假设基线 Δv_null / J_null
- 每格≤2 个 k_c 误检的离群值标 ⚠E 剔除后，取剩余样本 0.95 分位作为 N_h_cell / Nv_h_cell；>2 个时记 ⚠E 格。
- 会话作废条件追加：base 的 ⚠M 有 ≥2 格超过 2/20。

###### 6. 已否定：负 lead（种子提前 X）
- 证据（#43，E2 估计器，n=20/格）：t060/t156 共 5 个可比格中，J 在 X=0 最小，并随 X∈{0,39,55,70,85} 增大，例如 t156rev 0.37→24.5→41.6→63.0→89.0。唯一例外是 t060fwd，在 0–39 区间持平（13.38/13.36），之后增大。J 全格中位：A0 0.98，Am39 13.4，Am55 30.1，Am70 51.6，Am85 81.6。
- A0 在 t156 的 seedV 464/489，对应 v_ref 455/454，速度一致。
- 结论：#42 的“种子错位”是线性外推估计器的假象。删除 Am 系列，不再扫负 lead。
- #43 会话因 P0 作废，但这条结论的效应量远大于 N_h（0.15–1.35），作为诊断级证据写入。

###### 7. A 路径地位
- A0 重新进入候选，主路径仍为 hybrid B（M1b）。
- A 升主条件（同一有效会话内全部满足）：
  1. 通过格数 ≥ B。
  2. J 全格中位 ≤ B。
  3. ≥5/7 格可判 Δv，其中 ≥4/7 通过。
  4. 各格 ⚠M ≤ 2/20。
- #45 增设 Aφ（种子取在 t̂_apply）和 A+8（固定 +8 ms）两臂，检验“A0 残余 J 来自生效帧相位”。预测：A0 的 J 对相位斜率 ≈ |v_ref|，Aφ 的斜率 ≈ 0。

##### 5.3.6 裁决 #45：M1b 以 hybrid B 条件冻结 R-2（C5(b) 除外）（出处 notion_r2_45_ab.md B 节）

###### 裁决 #45（会话有效｜维持 B=M1b｜R-2 条件冻结，C5(b) 除外）

**结论**：#45 有效（P0 ✓ b=1.00、MAD 0.01 ms；base J 门 0/7、⚠M 超限 0 格）。无臂升主，维持 B 为 M1b。Aφ 弃用：A0 的 J、Δv 对 φ_m 斜率 CI 7/7 含 0；Aφ 的 L 随 φ\* 线性变化，反而引入相位依赖。
更正：B 臂 Δv 可判 2/7（t060 两格仅可判 J）。

**偏差确认（并入规程）**
- D1：Win10 19045 用 DwmGetCompositionTimingInfo(qpcVBlank, qpcRefreshPeriod) + QPC 自旋控相位。逐试次记 φ_m，分层和回归用 φ_m。
- D2：WinUI3 下 tInt_m = StartAnimation 返回时的 QPC（隐式批次），另记 tCommitDone 和 tSeed。tInt_m−tInt_c > Q3+3·IQR 标 ⚠C，报告剔除前后结论；不追溯 #45。
- D3：t_s = t̂_apply；k_c = τ ≥ t_s−T/2 的首帧；⚠E 用 J > Q3+3·IQR；J 阈 = max(N_h,1)；J 门 N_h ≤ 4；Δv 阈 = max(20, 5%·|v_ref|)。
- D3 附加：每次必须做 k_c 双口径（强制不归前 / 强制归前）。只在一种口径下通过的格标「k_c 敏感」，不计为稳健通过。

**t156 Δv 不可判的处置**
- #45 不受影响：现行 ③ 下非 t156 格最多 4 个，<5，任何臂都不满足；即使 t156 可判，A+8（54/82/53）和 A0（rev 35、dbl 34.5）的 t156 Δv 中位都超过阈 26。
- 不对称判定（#47 起执行）：Nv_h 只约束通过。Δv 的 CI 下界 > 阈 + Nv_h 时直接判 FAIL-确定，不受可判性限制。按此 B 的 t156×3 为真实速度不连续。
- 离线修估计器（用 #45 原始数据，不新跑）：首选 E3r（减去 ω0、ζ 解析 base 轨迹后，在残差上算前后斜率差），备选 E3q（局部二次）。门槛：base 在 t156×3 的 Nv_h ≤ 13，且 t060、t300 的 Nv_h 增幅 ≤ 20%。
- 两者都不过门：t156 Δv 定为结构性不可判。③ 改为「非结构性不可判格全部可判且全部达标；t156×3 无 FAIL-确定；t156×3 J 通过」。#47 开跑前登记。

**A+8 处置：只做零新跑诊断，不作候选臂**
- 疑点：rev 方向 J 约 0.05–0.13，fwd 方向约 3.2–3.7；A0 本身就有方向差（t060 rev 8.82 / fwd 4.52）；通过格与「k_c 归前 20/20」完全重合；J 中位 0.18 掩盖了 3 个 J>1 的格。
- 诊断 1：k_c 双口径重算。A+8 的通过格一旦翻转就关闭。
- 诊断 2：用有符号 J 按格、按方向求 L\* = 8·J0s / (J0s − J8s)。
- 立项条件：7 格 L\* 跨度 ≤ 2 ms，且同一时刻 rev/fwd 的 L\* 相差 ≤ 1 ms，并能用实测 t̂_apply−tSeed 解释。满足后才在 #47 开确认臂 A_L\*（只前置位置，速度取 t̂_apply 处的 v_ref）；否则永久关闭 A+8。

**M1b / R-2 条件冻结（C5(b) 除外）**
- 稳健性：B t060 两格不论重测成什么值，B 的 J 全格中位都在 [0.18, 1.00]。A0 t060 数据干净（χ²/dof ≈ 0.12、⚠M 0），预计复现 J≈8.8，② 仍不过。A+8 的 ③ 在任何口径下都不过（t300rev 21.1 > 20）。
- 解冻触发（两项同时满足）：#46 的 A0 t060 J 降到使 A0 的 J 中位 ≤ B，且 E3r 下 A0 t156 的 Δv 达标。
- 冻结文本必须写明的 B 已知缺陷：t156×3 和 t300×2 有真实速度不连续；seedV 异常（t060 约 −14000、φ\*=2 有一格为 0；t156 约 1830 对比 v_ref 522，疑为陈旧速度）；B 的 t060 数值标「待 #46」。

**#46 追加项（都是零新跑）**
1. 末两帧修复后，用 A0、Aφ、A+8 的 #45 原始数据去掉末两帧重算，确认结论不变。
2. 查明 B 的 seedV（约 −14000 和 0）是读回问题还是实际行为。
3. 交付：k_c 双口径重算表、有符号 J 和 L\* 表、E3r/E3q 的 base Nv_h 表。

**#47 预登记待议**：考虑增加 ②'（J>1 的格数 ≤ B），防止中位掩盖双峰；须在开跑前登记，不追溯。

##### 5.3.7 裁决 #46 与 #47：R-2 本机 FAIL 结题，M1b 降级实现 B_v（不满足 R-2）（出处 notion_r2_46_ab.md B 节、artifacts/win_r2_p11_47.txt）

> 本节取代 §5.3.6 中「M1b 以 hybrid B 条件冻结 R-2」的结论；§5.3.6 仅作历史保留。

###### 裁决 #46（续 #45）

**会话有效性**：#46 有效（P0 ✓，b=1.00；base N_h 0.50/0.53；base ⚠M 0/20）。⚠C 1/120，剔除与否结论不变。

**#46 判定（h45 口径）**
- B t060rev/fwd：FAIL/FAIL。v2 新鲜帧规则下 17/20、18/20 次无 ≥2 个新鲜帧，回退 vB=0，Δv≈1990（阈 99）。v1 口径下 seedV≈−14000，同为 FAIL；结论对新鲜帧口径稳健。
- A0 t060rev/fwd：J 8.68/4.36，FAIL/FAIL（Δv 49/13 达标）。A0 t060fwd 为 k_c 敏感格，按锁定口径判 FAIL。
- A+8：永久关闭（双口径翻转；L* 跨度 21 ms，门≤2）。
- t156 Δv：E3r、E3q 均不过门，定为「结构性不可判」。
- 解冻条件（A0 t060 J 中位≤B，且 E3r 使 A0 t156 Δv 达标）不满足，B=M1b 条件冻结维持。

**偏差 D4（新鲜帧规则，有条件追认）**
- 测试工具判据：剔除首个 |x|<0.5 帧之前的所有帧，再要求到达 QPC 与 SRT 严格递增、|Δx|≥0.01、SRT>B1 指令。
- 依赖测试已知起点 x=0，仅限测试工具使用，不得作为产品判据。
- 待更正：「剔除帧数 范围 1–1，合计 3/2」与 n=20 矛盾，须给出统计口径并更正。
- v1 原始文件 win_r2_p10_raw_v1.txt 存档。

**③ 修订（登记于 #46，自 #47 生效，不追溯）**
- 结构性不可判：t156rev/fwd/dbl 的 Δv。
- 通过条件（同时满足）：
  (a) 非结构性可判项（t060、t300 各格的 J 与 Δv；t156 三格的 J）全部可判且全部达标；
  (b) t156 三格无「FAIL-确定」；
  (c) t156 三格 J 通过。
- FAIL-确定：E3 Δv 中位 > 阈 + 3·Nv_h(E3)；按 #45 base，t156 ≈ 26 + 3×19.7 ≈ 85。只用于判不通过。
- k_c 口径锁定 h45：J 窗 [τ_kc−1, τ_kc]，后段拟合 kc+1…kc+5；「k_c≤1.5 归前」仅作计数标记。强制归前/不归前仅作敏感性列，k_c 敏感格须标注。
- 附注：t060 打断前仅 4–5 帧，δ 拟合欠约束（去末两帧后 2/21 格结论改变），长期保留，不设门。

**现有臂在 ③ 修订下的终局判定（h45 口径）**
| 臂 | 不通过项 | 判定 |
| B | t060rev J；t060/t300 Δv 全格；t156 Δv 1323–1329 > 85（FAIL-确定）；t156rev/dbl J | 不通过 |
| A0 | t060rev/fwd J；t156rev/dbl J | 不通过 |
| Aφ | t060rev/fwd；t300rev；t156fwd J | 不通过 |
| A+8 | 已关闭 | — |

**M1b 实现规定（R-2-IMP-1）**
1. 禁止以 vB=0 作为任何情况下的回退值。
2. WGC 速度存在约 28.8 ms（≈2 帧）的结构性滞后，在所有格都不可用（t060 有新鲜帧时 seedV ≤153，v_ref 1982；t156 seedV≈1830，v_ref 522；#45 B 在全部格 Δv FAIL）。种子速度一律取 remap 后的影子解析速度；WGC 只用来取位置。
3. 产品新鲜度判据须与画面内容无关（帧 SRT ≥ 本次提交的 t̂_apply，或帧序号晚于提交确认）。
4. 不回退到 A0 整臂（A0 位置项在 t060 J 已失败）。
5. 上述组合记为新臂 B_v（B 位置 + 影子速度），不继承 B 的任何判定；位置项滞后补偿不在本规定范围，B_v 按原 B 的位置路径实现。

**R-2 状态（本机）**
- 未结题。现有臂判定为终局；开放项为 B_v（#47）与 C5(b)。
- #47：只跑 B_v 一臂，7 格（含 t156dbl），同场跑 base；口径同 #46，新鲜帧判据用产品口径（规定第 3 条）。
- 若 B_v 满足 ③ 修订：M1b 冻结对象由 B 改为 B_v，R-2 本机通过，仅剩 C5(b)。
- 若不满足：R-2 本机以 FAIL 结题，仅剩 C5(b)；M1b 以 B_v 作为降级实现，并在文档中标注「不满足 R-2」。

###### #47 结果与 R-2 本机结题（出处 artifacts/win_r2_p11_47.txt）
- 会话有效：P0 ✓（b=1.00）；base J 门 0/7、⚠M 0。
- B_v 实现：不设 InitialValue（位置取合成器当前值，即原 B 位置路径，不做滞后补偿）；种子速度 = S1 v2 remap 影子解析速度（seedV=v_ref 全格一致）；**禁止 vB=0**；不读 WGC 速度。
- J：仅 t300rev/fwd 两格通过（0.28/0.59）；t060rev/fwd 20.9/16.7 DIP、t156rev/fwd/dbl 7.0/3.5/7.0 DIP，FAIL。
- Δv：可判 4 格中达标 3（t300rev 21.4 > 20 不达标）；t156 Δv 29.8/8.8/29.8 均 < FAIL-确定界 ≈87。
- ③（修订）：(a)✗ (b)✓ (c)✗ → **B_v 不满足 R-2**。
- 观察：B_v 的 J ≈ |v_ref| × 7–13 ms，即位置约落后 1 帧（原 B 在 t060 J 较小，是 vB=0 时位置与速度项部分抵消）。位置滞后补偿超出本裁决，留待协调方。

**R-2 状态（本机）：FAIL 结题**。M1b 以 B_v 作为降级实现，标注「**不满足 R-2**」；R-2 剩余开放项仅 C5(b)（第二台机器/换刷新率）。

**取代说明（2026-10-20 Asia/Taipei，notion_r2_b_gate_ab.md B 节）**：原「J_vis ≤ 0.5 DIP 且 ≤ 5% 帧位移」「Δv_vis ≤ max(20, 5%·|v|)」M1b 门槛及 seed 模式 A 默认 / P1+P2 前置均被 R-2′ 取代；绝对门转为 M2 目标。

---

**引用**：[Period](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.springvector3naturalmotionanimation.period) ・ [InitialValue](https://learn.microsoft.com/en-us/uwp/api/windows.ui.composition.scalarnaturalmotionanimation.initialvalue) ・ [InitialVelocity](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.scalarnaturalmotionanimation.initialvelocity) ・ [AnimationController.PlaybackRate](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.animationcontroller.playbackrate) ・ [InsertExpressionKeyFrame](https://learn.microsoft.com/en-us/uwp/api/windows.ui.composition.keyframeanimation.insertexpressionkeyframe) ・ [AnimationDelayBehavior](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.animationdelaybehavior) ・ [AnimationStopBehavior](https://learn.microsoft.com/en-us/uwp/api/windows.ui.composition.animationstopbehavior) ・ [CompositionScopedBatch.Completed](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.compositionscopedbatch.completed) ・ [CompositionPropertySet](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.compositionpropertyset) ・ [CreateCubicBezierEasingFunction](https://learn.microsoft.com/en-us/uwp/api/windows.ui.composition.compositor.createcubicbeziereasingfunction) ・ [CreateFromVisual](https://learn.microsoft.com/en-us/uwp/api/windows.graphics.capture.graphicscaptureitem.createfromvisual)（仅 `Windows.UI.Composition.Visual`） ・ [IGraphicsCaptureItemInterop::CreateForWindow](https://learn.microsoft.com/en-us/windows/win32/api/windows.graphics.capture.interop/nf-windows-graphics-capture-interop-igraphicscaptureiteminterop-createforwindow) ・ [TryGetAnimationController](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.compositionobject.trygetanimationcontroller) ・ [DCompositionGetFrameId](https://learn.microsoft.com/en-us/windows/win32/api/dcomp/nf-dcomp-dcompositiongetframeid) ・ [DCompositionGetStatistics](https://learn.microsoft.com/en-us/windows/win32/api/dcomp/nf-dcomp-dcompositiongetstatistics)

---

## 审校注（执行 bot，非 DeepSeek 原文）

1. **ζ>1 是否 lower（派题 #14 已落地）**：API **接受** ζ>1 且不钳、可跑过阻尼；产品仍选 **v1 不 lower**。`composition.h` 已在 `PlanOverride` / `PlanGraph` 于 `ToCompositor` 后加 `if (c.damping > 1.0) Fallback(...)`；ζ≤1 仍 CompositorSpring。回归：ζ=0.8→CompositorSpring，ζ=1.2→AppThread；`composition_tests` 2073/0。 **轨迹 #23 PASS**（单调/无过冲/可区分；settle ratio≈3.7 vs spring.h）→ 仍不抬门。
2. **动画控制器（已真机核实）**：无 `TryStartAnimation`。**KeyFrame**：`CreateAnimationController` + 三参 `StartAnimation`（或 TryGet 非 null）。**NaturalMotion（弹簧）**：`TryGetAnimationController` **恒 null**——弹簧打断/改目标只用影子种子 + InitialValue/Velocity，**不要**指望 TryGet。
3. **`InitialVelocity` 默认 0**：必须显式设置；与影子状态种子一致。
4. **S1**：Period = response（无阻尼）（**v1，已被 S1 v2 取代**：2026-10-20 裁决 A1 改为 `Period = r/(2π)`，见 [notion_s1_remap_ab.md](./notion_s1_remap_ab.md)）；峰峰≈0.511 s。否决 `response/√` 分支。
5. **门槛 / Rendering / 滞后**：`DwmFlush`+WGC+frozen-frames；禁用 `cRefresh`；Rendering≈31 ms 不作代理；WGC/呈现 ~160–200 ms 滞后须时间对齐后比对。
6. **固有差异（已结案 / Claude FINAL R-1）**：产品/S1 残差 ≈28.6%/12.1%；扫参非产品变体最低≈8%/6%；记风险；保持 S1；§4 形状门槛不放宽（§4.1 / §5.3）。
7. 其余待验证保留。验收在 **§4**；弹簧打断见影子+对齐 WGC。

---

## 元信息

| 项 | 值 |
|---|---|
| 时间 | 2026-10-19 23:41:39 Asia/Taipei |
| session id | `session-05cb9ffd-6ede-454c-add3-6669147890e7` |
| provider | `deepseek-account` |
| model | `deepseek-v4-pro` |
| reasoningEffort | `max` |
| Harness 入口 | `http://127.0.0.1:27921/`（cookie 绑定 RPC `http://127.0.0.1:54300/`） |
| 原始回复 | [`deepseek_m1b_raw.md`](./deepseek_m1b_raw.md) |
| 修订记录 | **2026-10-20 Asia/Taipei**（早）：TryStartAnimation / WGC CreateForWindow / 帧 id / frozen-frames。<br>**2026-10-20 Asia/Taipei**（亮屏复测）：S1 `Period=response`；`DwmFlush` 门槛；禁用 cRefresh；Rendering≠合成器时钟；§4 暂不放宽。<br>**2026-10-20 Asia/Taipei**（控制器/打断）：TryGet 仅 KeyFrame；弹簧恒 null；打断=影子+对齐 WGC；CreateAnimationController；滞后 160–200 ms。<br>**2026-10-20 Asia/Taipei**（亮屏 §4 进展）：打断/终值/贝塞尔**已过**；弹簧形状未过 → 曾记「模型差异**待决**」。<br>**2026-10-20 Asia/Taipei**（派题 #13 扫参**结案**）：(a)/(b)/(c) 无一 ≤1%；最低 A≈8% / E≈6% → **已结案（固有差异）**；产品仍 S1；**§4 形状门槛不放宽**；条目自 §5.2#24 移入 **§5.3**。<br>**2026-10-20 Asia/Taipei**（派题 **#14**）：S2 ζ≥0 接受不钳、ζ<0 抛；过阻尼可跑；`composition.h` ζ>1 不 lower 门控已落地；§5.1#12。<br>**2026-10-20 Asia/Taipei**（派题 **#15**）：S3 InitialVelocity=DIP/s、与 Period 独立；rate 折叠抽查 RMS≈13.6%；§5.1#13。<br>**2026-10-20 Asia/Taipei**（派题 **#16**）：S4 DelayTime+preset PASS（IV==preset 无跳变；IV≠preset 结束跳 IV）；§5.1#14。<br>**2026-10-20 Asia/Taipei**（派题 **#17**）：S5 PASS（到位保持终值；Stop 冻结；Completed 偏晚仅记录）；§5.1#15。<br>**2026-10-20 Asia/Taipei**（派题 **#18**）：K5 PASS（Stop 冻结；TryGet 非同步物化→Stop+SetValue）；§5.1#16。<br>**2026-10-20 Asia/Taipei**（派题 **#19**）：K1 AGREES（段末 easing；RMS≈0.57%）；§5.1#17。<br>**2026-10-20 Asia/Taipei**（派题 **#20**）：K2 HONOR y∉[0,1]；bezierOvershoot 默认 true；§5.1#18。<br>**2026-10-20 Asia/Taipei**（派题 **#21**）：K4 PASS（KeyFrame 完成后保持终值 |err|=0）；§5.1#19。 |<br>**2026-10-20 Asia/Taipei**（派题 **#22**）：K3 PARTIAL（Progress 生效但非线性；PlaybackRate± 可用）；§5.1。<br>**2026-10-20 Asia/Taipei**（派题 **#23**）：ζ>1 轨迹 PASS（单调/overshoot=0/1.5≠3.0）；**门控不抬**。<br>**2026-10-20 Asia/Taipei**（派题 **#24**）：rate 折叠同量级（RMS_A≈50/RMS_B≈35）；公式保留、默认仍 rate=1。<br>**2026-10-20 Asia/Taipei**（派题 **#25**）：打断单τ 未达§4（\|Δpos\|≈20–28）；seedLead 16.7ms 略优；门控不改。<br>**2026-10-20 Asia/Taipei**（派题 **#26**）：Period≥0.001 ACCEPT 不钳；Zero THROW。<br>**2026-10-20 Asia/Taipei**（派题 **#27**）：IV=null → FROM_CURRENT（仅记录）。<br>**2026-10-20 Asia/Taipei**（派题 **#28**）：K3 边角 Delay+Progress OK；\|rate\|>16 CLAMP@±16；Progress+反向 OK。<br>**2026-10-20 Asia/Taipei**（**Claude FINAL**）：四点 Agree；H1 settled（H2/H3 rejected）；产品残差 **28.6%/12.1%**（扫参 8%/6% 次要）；显式 seed=`spring.h::At(elapsed+seedLead)`；ζ>1 门控已落地确认；固有差异 = R-1 known-fail，§4 ≤1% 不放宽。见 `claude_m1b_deepseek_review.md`（FINAL）/ `claude_m1b_spec.md` 修订 4。<br>**2026-10-20 Asia/Taipei（Notion R-2）：R-2 known-fail（R-1 衍生）；§4 不放宽；见 docs/notion_r2_review_abc.md**。<br>**2026-10-20 Asia/Taipei（派题 #31）：离线增量Δ(0)=0.000 / (16.7)=−9.517 → 确认纯 R-1 衍生，不重开；artifact `artifacts/win_r2_delta_offline.txt`；composition.h 未改。<br>**2026-10-20 Asia/Taipei**（R-2 重开）：§5.3 R-2 按 notion_r2_reopen_abc.md B 节整节替换为 OPEN（seed 来源缺陷，J_vis≈207 DIP）；§4.1 表与 §5.2#23 的 20–28 口径标注作废。<br>**2026-10-20 Asia/Taipei**（R-2′）：§5.3 R-2 按 notion_r2_b_gate_ab.md B 节替换：hybrid B（WGC 末两帧速度）flag 默认 ON；M1b 门槛改为 R-2′ 底噪相对门；0.5 DIP / max(20,5%\|v\|) 转 M2 目标；composition.h 未改。 |

---

## 完整提示词

```
你是 Windows 合成器（Microsoft.UI.Composition / WinUI3）与动画系统方面的资深工程师。请用中文写一份「M1b 合成器路径规格」，简洁实用、可直接落地。项目是「FluidMotion / Windows 桌面端动效框架」（协调 bot：「动效框架」）。

工作目录 C:\Users\admin\Temp\motion_framework_review\ 下有本次相关文件（composition.h、spring.h、fm_capi.h、bezier.h、workbuddy_soft_demo.md、api.md、explore_round.md 等），下文也内联了关键内容。可以用工具阅读目录文件核对，但请不要修改任何文件，只输出规格正文。

【背景 / 已知现象】
workbuddy 在 MotionSoftDemo（WinUI3，WASDK 2.5.1，C#，net10.0-windows10.0.26100.0）里，用 ElementCompositionPreview.GetElementVisual(AnimBox) + SetIsTranslationEnabled，对 Visual 的 Translation.X 跑：
- SpringScalarNaturalMotionAnimation：FinalValue=目标，Period=0.32s，DampingRatio=0.78（直接把我们的 response/damping 当 Period/DampingRatio，即 composition.h 中未验证假设 S1/S2），未设 InitialValue/InitialVelocity；
- ScalarKeyFrameAnimation：key0 "this.StartingValue"，key1 目标 + CubicBezierEasingFunction((0.1,0.9),(0.2,1.0))，Duration 220ms。
显示器很可能处于熄屏/休眠状态（输入空闲 >2h，关屏超时 900s；软件路径 CompositionTarget.Rendering 间隔 ≈31.2ms = 2×15.625ms，而非 16.7ms）时观察到：
1. 用 ExpressionAnimation("v.Translation.X") 驱动一个 CompositionPropertySet 标量 "x"，每帧 TryGetScalar 读回始终为 0（135 个采样全 0），连静态写 Translation.X:=37 也读不到（Visual.Properties 读到 37，probe 仍为 0）——即 TryGetScalar 只返回静态值；
2. 对照组：对 PropertySet 标量直接跑 1000ms 线性关键帧，CompositionScopedBatch 13.6ms 就 Completed，读到终值 100，无中间值；
3. 三个弹簧 batch 不自行完成，直到 220ms 贝塞尔 batch 在 29.9ms 完成才一起 Completed；
4. PrintWindow(PW_RENDERFULLCONTENT) 截窗客户区全白；CopyFromScreen 找不到方块。
软件路径（C ABI fm_capi，CompositionTarget.Rendering 驱动）打断连续性已验证：|Δv|=0、|Δvel|=0（manual clock）。

【规格必须包含】
1) composition.h 的 lowering 到 Microsoft.UI.Composition API 的精确映射：
   - 弹簧：我们的 SpringParams{response=无阻尼周期 2π/ω0，damping=ζ} 与 SpringScalarNaturalMotionAnimation.Period / DampingRatio 的换算公式；写出刚度 k、阻尼系数 c 与 Period/DampingRatio 的关系（含质量假设，如 m=1），并说明 Period 是无阻尼周期还是有阻尼周期、若是后者如何换算；rate 折算（period/rate、velocity*rate）；DelayTime、presetValue、Hold、Stop 的对应 API；
   - 贝塞尔关键帧：KeyframeSpec 列表如何映射为 ScalarKeyFrameAnimation.InsertKeyFrame(normalizedProgress, value, CubicBezierEasingFunction) —— 归一化进度计算、easing 作用在哪一段（K1）、首帧用 this.StartingValue 还是显式值、elapsed>0 / rate≠1 / reversed 的实现方式（AnimationController.Progress / PlaybackRate / Direction 等）、y 超出 [0,1] 的处理（K2）、DelayTime 与 DelayBehavior。
2) 飞行中改目标不跳变：InitialValue/InitialVelocity 的用法、不设时 NaturalMotion 是否自动继承当前值与速度；用 ExpressionAnimation / 动画捕获拿当前值的可行方式；明确写出哪些 API 真能拿到合成器上正在动画属性的当前值（例如 CompositionPropertySet.TryGetScalar / Visual.Properties.TryGetVector3 对正在动画的属性的行为、CompositionObject.StopAnimation 之后读回、AnimationController 等），哪些不能；以及回退方案（在 app 侧用 spring.h 闭式解按时钟推算当前状态再作为 InitialValue/InitialVelocity 下发）。
3) 可靠测量合成器路径的方法：Windows.Graphics.Capture（GraphicsCaptureItem.CreateFromVisual / CreateFromWindowId，Direct3D11CaptureFramePool 逐帧 + SystemRelativeTime）、DComp 帧统计（IDCompositionDevice::GetFrameStatistics / DCompositionGetFrameId / DCompositionGetStatistics 等，注明 WinUI3 下能否拿到）、CompositionTarget.Rendering、DwmGetCompositionTimingInfo、ScopedBatch 计时；每种方法在亮屏 vs 熄屏（显示器休眠、无 vblank）时的表现差异与可信度；给出具体测量流程。
4) 验收标准（可量化）：例如启动延迟、打断时位置/速度连续性阈值（px、px/s）、终值误差、与软件路径/spring.h 闭式解的轨迹偏差（RMS/最大）、帧间隔 p50/p95、熄屏时的处理（判无效而非失败）等。

【硬性要求】
- 不确定的 API 行为一律标「待真机验证」，不要编造 API 名称或行为；能引用官方文档语义就说明依据。
- 文末给出「待真机验证清单」（编号列表）。
- 用 Markdown，结构清晰，简洁。

=================== 附：code/composition.h（全文） ===================
// composition.h: capability probe, lowering planner, and CompositionDispatcher.
// Module 6. C++17, standard library only (no WinRT here: the compositor is behind
// ICompositorBackend, which module 7 implements with Windows.UI.Composition).
// Depends on controller.h (IDispatcher, ControllerSnapshot) and earlier headers.
//
// WHAT THIS MODULE DECIDES
//   For every property a controller owns, the planner picks ONE of:
//     CompositorKeyframes  linear/cubic-bezier keyframes run by the compositor
//     CompositorSpring     one NaturalMotion spring run by the compositor
//     Hold                 set a value once, nothing animates (paused pose, settled)
//     AppThread            keep evaluating on the app thread (reason recorded)
//   `Hold` is an addition to the three kinds in the brief: a paused pose or a
//   settled spring needs "set once and stop", which is neither an animation nor
//   per-frame work.
//
// LOWERING RULES (v1). A graph property is lowered only when ALL hold:
//   * it has a compositor mapping (probe), and the controller is playing
//     (otherwise it becomes a Hold at Timeline::Evaluate(position));
//   * loops == 1;
//   * exactly one track drives it (the overwrite/backfill conflict rule is not lowered);
//   * the track has no zero-duration (instant jump) segment;
//   * the track is either all linear/bezier/hold segments (keyframes) or exactly one
//     spring segment (CompositorSpring; not reversed). Mixed spring + keyframe
//     tracks stay on the app thread (planned extension: chained stages);
//   * bezier y control points stay in [0,1] unless the probe says overshoot works;
//   * the keyframe count is within the probe's limit.
//   A spring OVERRIDE (from Retarget) is lowered to CompositorSpring seeded with the
//   state at the snapshot's clock time (the spring ODE is time-invariant, so a
//   restart from the current state is equivalent). A settled override is a Hold.
//   Anything finished (forward: position >= start + duration; reverse: position <=
//   start) is a Hold. Reduced motion needs nothing here: reduced timelines are
//   ordinary timelines, and instant ones complete at Play (playing == false -> Hold).
//
// TIME MODEL of a lowered animation (all in "animation-local" seconds except delay):
//   delay   real seconds until it starts advancing (presetValue is set immediately)
//   elapsed local seconds already played when submitted
//   rate    local seconds per real second
//   reversed plays backward; elapsed counts distance from the top
//   position(t_real) = elapsed + rate * (t_real - delay)          (forward)
//                    = duration - (elapsed + rate * (t_real - delay))  (reversed)
//   Springs fold the rate into the spring: period / rate, initial velocity * rate.
//
// ASSUMPTIONS ABOUT Windows.UI.Composition (ALL UNVERIFIED; see SpringMapping)
//   S1  SpringScalarNaturalMotionAnimation.Period is our `response` (undamped natural
//       period, 2*pi/omega0), i.e. NOT a damped period.
//   S2  DampingRatio is our `damping` (zeta, 1.0 = critical).
//   S3  InitialVelocity is in value-units per real second.
//   S4  The animation may be started with a DelayTime and the property keeps its
//       preset value until the delay elapses.
//   S5  The compositor's spring stops at ITS OWN rest threshold; ours snaps to the
//       target at SpringSettleTime (relative thresholds, <= 0.1 / 0.5 caps). Small
//       end-of-motion differences are expected.
//   K1  A keyframe's easing function shapes the segment ENDING at that keyframe.
//   K2  CubicBezierEasingFunction honors control-point y outside [0,1] (verified 派题 #20;
//       Capabilities::bezierOvershoot = true).
//   K3  An animation can start part-way (elapsed > 0), e.g. by AnimationController
//       Progress, and be played at a non-1 rate or in reverse.
//   K4  After an animation completes the property keeps its final value.
//   K5  StopAnimation leaves the property at its current animated value.
//   Everything the backend does with these is hidden behind ICompositorBackend so a
//   wrong assumption is fixed in one place (SpringMapping, Capabilities, or backend).
#pragma once
#include <algorithm>
#include <cmath>
#include <map>
#include <memory>
#include <optional>
#include <set>
#include <string>
#include <utility>
#include <vector>

#include "controller.h"

namespace motion {

// ----------------------------------------------------------- capabilities ----

struct Capabilities {
    bool springNaturalMotion = true;   // NaturalMotion spring animations exist on this OS
    bool cubicBezier = true;           // CubicBezierEasingFunction usable
    bool bezierOvershoot = true;       // y control points outside [0,1] honored (K2, verified 派题 #20)
    std::size_t maxKeyframes = 512;
};

struct CompositorProperty {
    std::vector<std::string> paths;    // e.g. {"Scale.X", "Scale.Y"} for scalar "scale"
};

class ICapabilityProbe {
public:
    virtual ~ICapabilityProbe() = default;
    virtual Capabilities Caps() const = 0;
    virtual std::optional<CompositorProperty> Map(const std::string& property) const = 0;
};

// Default table + settable capabilities. Units by convention: rotation in degrees.
// Module 7 supplies a real probe (API presence checks via ApiInformation: unverified).
class StaticCapabilityProbe : public ICapabilityProbe {
public:
    StaticCapabilityProbe() {
        table_ = {{"opacity", {{"Opacity"}}},
                  {"offsetX", {{"Offset.X"}}}, {"offsetY", {{"Offset.Y"}}},
                  {"scale", {{"Scale.X", "Scale.Y"}}}, {"scaleX", {{"Scale.X"}}}, {"scaleY", {{"Scale.Y"}}},
                  {"rotation", {{"RotationAngleInDegrees"}}}};
    }
    Capabilities Caps() const override { return caps_; }
    std::optional<CompositorProperty> Map(const std::string& p) const override {
        auto it = table_.find(p);
        if (it == table_.end()) return std::nullopt;
        return it->second;
    }
    void SetCaps(const Capabilities& c) { caps_ = c; }
    Capabilities& MutableCaps() { return caps_; }
    void Add(const std::string& property, std::vector<std::string> paths) { table_[property] = {std::move(paths)}; }
    void Remove(const std::string& property) { table_.erase(property); }
private:
    Capabilities caps_;
    std::map<std::string, CompositorProperty> table_;
};

// S1 v2 (2026-10-20 ruling A1, #36/#41): Period = response/(2*pi), DampingRatio = zeta.
constexpr double kTwoPi = 6.283185307179586476925286766559;
constexpr double kPeriodPerResponse = 1.0 / kTwoPi;
inline SpringParams MapSpringToCompositor(SpringParams p, double periodScale = kPeriodPerResponse,
                                          double dampingScale = 1.0);   // asserts p.response > 0
struct SpringMapping {
    double periodScale = kPeriodPerResponse;   // compositorPeriod = response * periodScale
    double dampingScale = 1.0;                 // compositorDampingRatio = damping * dampingScale
    SpringParams ToCompositor(SpringParams p) const { return MapSpringToCompositor(p, periodScale, dampingScale); }
};

// ------------------------------------------------------------------ plans ----

enum class PlanKind { CompositorKeyframes, CompositorSpring, Hold, AppThread };

inline const char* ToString(PlanKind k) {
    switch (k) {
        case PlanKind::CompositorKeyframes: return "compositor-keyframes";
        case PlanKind::CompositorSpring:    return "compositor-spring";
        case PlanKind::Hold:                return "hold";
        case PlanKind::AppThread:           return "app-thread";
    }
    return "?";
}

// Easing shapes the segment ENDING at this keyframe (K1).
struct KeyframeSpec {
    double time = 0, value = 0;                 // seconds within the animation, [0, duration]
    bool bezier = false;
    double x1 = 0, y1 = 0, x2 = 1, y2 = 1;      // used when bezier
};

struct KeyframeAnimation {
    std::vector<KeyframeSpec> keys;             // first key at time 0
    double duration = 0, delay = 0, elapsed = 0, rate = 1;
    bool reversed = false;
    std::optional<double> presetValue;          // set immediately when delay > 0
};

struct SpringAnimation {                        // already in COMPOSITOR terms
    double period = 0.35, dampingRatio = 0.8;   // seconds, ratio
    double from = 0, fromVelocity = 0, target = 0;   // velocity per real second
    double delay = 0;
    std::optional<double> presetValue;
};

struct PropertyPlan {
    std::string property;
    std::vector<std::string> paths;             // compositor paths; empty if unmapped
    PlanKind kind = PlanKind::AppThread;
    KeyframeAnimation keyframes;                // CompositorKeyframes
    SpringAnimation spring;                     // CompositorSpring
    double holdValue = 0;                       // Hold
    double finalValue = 0;                      // value when the animation completes
    std::string reason;                         // why AppThread (empty otherwise)
};

struct LoweringPlan {
    ControllerId id = 0;
    TargetId target = 0;
    double clockTime = 0;
    std::vector<PropertyPlan> props;

    const PropertyPlan* Find(const std::string& property) const {
        for (const auto& p : props) if (p.property == property) return &p;
        return nullptr;
    }
    std::size_t Count(PlanKind k) const {
        std::size_t n = 0; for (const auto& p : props) n += p.kind == k; return n;
    }
    bool FullyOffloaded() const { return Count(PlanKind::AppThread) == 0; }
    std::string ToString() const {
        std::string s;
        for (const auto& p : props) {
            s += p.property + ": " + motion::ToString(p.kind);
            if (!p.reason.empty()) s += " (" + p.reason + ")";
            s += "\n";
        }
        return s;
    }
};

// ------------------------- reference semantics (under the assumptions above) ----

inline double EvaluateKeyframes(const std::vector<KeyframeSpec>& k, double t) {
    if (k.empty()) return 0.0;
    if (!(t > k.front().time)) return k.front().value;
    if (t >= k.back().time) return k.back().value;
    auto it = std::upper_bound(k.begin(), k.end(), t, [](double v, const KeyframeSpec& s) { return v < s.time; });
    const KeyframeSpec& b = *it;
    const KeyframeSpec& a = *(it - 1);
    const double frac = (t - a.time) / (b.time - a.time);
    const double e = b.bezier ? CubicBezier(b.x1, b.y1, b.x2, b.y2).Ease(frac) : frac;
    return a.value + (b.value - a.value) * e;
}

// Value `t` real seconds after Start; `before` is the value held until the delay ends.
inline double SimulateKeyframes(const KeyframeAnimation& a, double t, double before) {
    if (t < a.delay) return before;
    const double adv = a.elapsed + a.rate * (t - a.delay);
    const double pos = a.reversed ? std::max(a.duration - adv, 0.0) : std::min(adv, a.duration);
    return EvaluateKeyframes(a.keys, pos);
}
inline double SimulateSpring(const SpringAnimation& a, double t, double before) {
    if (t < a.delay) return before;
    return Spring({a.period, a.dampingRatio}, a.from, a.target, a.fromVelocity).At(t - a.delay).value;
}

// ---------------------------------------------------------------- planner ----

class CompositionLoweringPlanner {
public:
    explicit CompositionLoweringPlanner(const ICapabilityProbe& probe, SpringMapping mapping = {})
        : probe_(&probe), mapping_(mapping) {}

    LoweringPlan Plan(const ControllerSnapshot& s) const {
        LoweringPlan plan;
        plan.id = s.id; plan.target = s.target; plan.clockTime = s.clockTime;
        const Capabilities caps = probe_->Caps();
        const std::set<std::string> owned(s.ownedProperties.begin(), s.ownedProperties.end());
        std::set<std::string> handled;
        for (const auto& o : s.overrides) {                       // overrides mask the graph
            if (!owned.count(o.property)) continue;
            handled.insert(o.property);
            plan.props.push_back(PlanOverride(o, s, caps));
        }
        if (s.timeline) {
            const SampleMap pose = s.timeline->Evaluate(s.position);
            std::map<std::string, std::vector<const PlacedTrack*>> byProp;
            for (const PlacedTrack& pt : s.timeline->Tracks()) byProp[pt.track->Property()].push_back(&pt);
            for (const auto& kv : pose) {
                if (!owned.count(kv.first) || handled.count(kv.first)) continue;
                plan.props.push_back(PlanGraph(kv.first, kv.second.value, byProp[kv.first], s, caps));
            }
        }
        return plan;
    }

private:
    static PropertyPlan Fallback(PropertyPlan pp, std::string why) {
        pp.kind = PlanKind::AppThread; pp.reason = std::move(why); return pp;
    }
    static PropertyPlan HoldAt(PropertyPlan pp, double v) {
        pp.kind = PlanKind::Hold; pp.holdValue = v; pp.finalValue = v; return pp;
    }

    PropertyPlan PlanOverride(const SpringOverrideInfo& o, const ControllerSnapshot& s, const Capabilities& caps) const {
        PropertyPlan pp; pp.property = o.property;
        auto m = probe_->Map(o.property);
        if (!m) return Fallback(pp, "property '" + o.property + "' has no compositor mapping");
        pp.paths = m->paths;
        const Spring sp(o.params, o.from, o.target, o.fromVelocity);
        const double elapsed = std::max(0.0, s.clockTime - o.startClock);
        if (elapsed >= SpringSettleTime(sp, o.from, o.target)) return HoldAt(pp, o.target);
        if (!caps.springNaturalMotion) return Fallback(pp, "NaturalMotion spring animation unavailable");
        const SpringState st = sp.At(elapsed);                    // restart from the current state
        const SpringParams c = mapping_.ToCompositor(sp.Params());
        pp.kind = PlanKind::CompositorSpring;
        pp.spring.period = c.response; pp.spring.dampingRatio = c.damping;
        pp.spring.from = st.value; pp.spring.fromVelocity = st.velocity; pp.spring.target = o.target;
        pp.finalValue = o.target;
        return pp;
    }

    PropertyPlan PlanGraph(const std::string& p, double pose, const std::vector<const PlacedTrack*>& tracks,
                           const ControllerSnapshot& s, const Capabilities& caps) const {
        PropertyPlan pp; pp.property = p;
        auto m = probe_->Map(p);
        if (!m) return Fallback(pp, "property '" + p + "' has no compositor mapping");
        pp.paths = m->paths;
        if (!s.playing) return HoldAt(pp, pose);                  // paused or seeked pose
        if (s.loops != 1) return Fallback(pp, "looping timelines are not lowered (v1)");
        if (tracks.size() != 1)
            return Fallback(pp, std::to_string(tracks.size()) + " tracks drive '" + p +
                                "'; the overwrite/backfill conflict rule is not lowered (v1)");
        const Track& tr = *tracks[0]->track;
        const double start = tracks[0]->start, d = tr.Duration(), pos = s.position, rate = s.rate;
        const bool done = s.reversed ? pos <= start : pos >= start + d;
        if (done || tr.Segments().empty()) return HoldAt(pp, pose);

        bool hasSpring = false, hasZero = false, anyBezier = false, overshoot = false;
        for (const auto& sg : tr.Segments()) {
            if (!(sg.duration > 0.0)) hasZero = true;
            if (sg.curve.kind == Curve::Kind::Spring) hasSpring = true;
            if (sg.curve.kind == Curve::Kind::Bezier && !sg.curve.bezier.IsLinear()) {
                anyBezier = true;
                const CubicBezier& b = sg.curve.bezier;
                if (b.Y1() < 0 || b.Y1() > 1 || b.Y2() < 0 || b.Y2() > 1) overshoot = true;
            }
        }
        if (hasZero) return Fallback(pp, "instant-jump segment (duration 0) is not representable as keyframes");

        if (hasSpring) {
            if (tr.Segments().size() != 1) return Fallback(pp, "mixed spring and keyframe segments are not lowered (v1)");
            if (!caps.springNaturalMotion) return Fallback(pp, "NaturalMotion spring animation unavailable");
            if (s.reversed) return Fallback(pp, "spring segments cannot be played in reverse");
            const auto& sg = tr.Segments()[0];
            const Spring sp(sg.curve.spring, sg.from, sg.to, sg.entryVelocity);
            const SpringParams c = mapping_.ToCompositor(sp.Params());
            pp.kind = PlanKind::CompositorSpring;
            pp.spring.target = sg.to;
            pp.spring.period = c.response / rate;                 // rate folded into the spring
            pp.spring.dampingRatio = c.damping;
            const double local = pos - start;
            if (local <= 0.0) {
                pp.spring.delay = (start - pos) / rate;
                pp.spring.from = sg.from; pp.spring.fromVelocity = sg.entryVelocity * rate;
                pp.spring.presetValue = sg.from;
            } else {
                const SpringState st = sp.At(local);
                pp.spring.from = st.value; pp.spring.fromVelocity = st.velocity * rate;
            }
            pp.finalValue = sg.to;
            return pp;
        }

        if (anyBezier && !caps.cubicBezier) return Fallback(pp, "cubic-bezier easing unavailable");
        if (overshoot && !caps.bezierOvershoot)
            return Fallback(pp, "bezier overshoot (y outside [0,1]) is unverified on the compositor");
        if (tr.Segments().size() + 1 > caps.maxKeyframes) return Fallback(pp, "too many keyframes");

        KeyframeAnimation ka;
        ka.keys.push_back({0.0, tr.InitialValue(), false, 0, 0, 1, 1});
        for (const auto& sg : tr.Segments()) {
            KeyframeSpec k; k.time = sg.start + sg.duration; k.value = sg.to;
            if (sg.curve.kind == Curve::Kind::Bezier && !sg.curve.bezier.IsLinear()) {
                const CubicBezier& b = sg.curve.bezier;
                k.bezier = true; k.x1 = b.X1(); k.y1 = b.Y1(); k.x2 = b.X2(); k.y2 = b.Y2();
            }
            ka.keys.push_back(k);
        }
        ka.duration = d; ka.rate = rate; ka.reversed = s.reversed;
        if (!s.reversed) {
            if (pos >= start) ka.elapsed = pos - start;
            else { ka.delay = (start - pos) / rate; ka.presetValue = tr.InitialValue(); }
        } else {
            if (pos < start + d) ka.elapsed = d - (pos - start);
            else { ka.delay = (pos - (start + d)) / rate; ka.presetValue = tr.FinalValue(); }
        }
        pp.kind = PlanKind::CompositorKeyframes;
        pp.keyframes = std::move(ka);
        pp.finalValue = s.reversed ? tr.InitialValue() : tr.FinalValue();
        return pp;
    }

    const ICapabilityProbe* probe_;
    SpringMapping mapping_;
};

// ---------------------------------------------------------------- backend ----

// The only thing module 7 has to implement against Windows.UI.Composition.
// Start* REPLACES any animation already running on (target, path).
class ICompositorBackend {
public:
    virtual ~ICompositorBackend() = default;
    virtual void StartKeyframes(TargetId, const std::string& path, const KeyframeAnimation&) = 0;
    virtual void StartSpring(TargetId, const std::string& path, const SpringAnimation&) = 0;
    virtual void Stop(TargetId, const std::string& path) = 0;                 // keeps current value (K5)
    // `key` is a compositor path when the property is mapped, else the property name.
    virtual void SetValue(TargetId, const std::string& key, double value) = 0;
};

// ------------------------------------------------------------- dispatcher ----

class CompositionDispatcher final : public IDispatcher {
public:
    CompositionDispatcher(const ICapabilityProbe& probe, ICompositorBackend& backend, SpringMapping mapping = {})
        : planner_(probe, mapping), backend_(&backend) {}

    // Replace this controller's plan: (re)start what is lowered, stop what no longer is.
    void Submit(const ControllerSnapshot& snap) override {
        State& st = states_[snap.id];
        LoweringPlan plan = planner_.Plan(snap);
        const TargetId t = plan.target;
        std::set<std::string> newLive, newApp;
        for (const PropertyPlan& pp : plan.props) {
            switch (pp.kind) {
                case PlanKind::CompositorKeyframes:
                    for (const auto& path : pp.paths) {
                        backend_->StartKeyframes(t, path, pp.keyframes);
                        live_[{t, path}] = snap.id; newLive.insert(path);
                    }
                    break;
                case PlanKind::CompositorSpring:
                    for (const auto& path : pp.paths) {
                        backend_->StartSpring(t, path, pp.spring);
                        live_[{t, path}] = snap.id; newLive.insert(path);
                    }
                    break;
                case PlanKind::Hold:
                    for (const auto& path : pp.paths) { StopIfOwned(t, path, snap.id); backend_->SetValue(t, path, pp.holdValue); }
                    break;
                case PlanKind::AppThread:
                    for (const auto& path : pp.paths) StopIfOwned(t, path, snap.id);
                    app_[{t, pp.property}] = {snap.id, pp.paths.empty() ? std::vector<std::string>{pp.property} : pp.paths};
                    newApp.insert(pp.property);
                    break;
            }
        }
        for (const auto& path : st.livePaths) if (!newLive.count(path)) StopIfOwned(t, path, snap.id);
        for (const auto& prop : st.appProps) if (!newApp.count(prop)) EraseAppIfOwned(t, prop, snap.id);
        st.livePaths = std::move(newLive); st.appProps = std::move(newApp);
        st.target = t; st.plan = std::move(plan);
    }

    // Completed: the controller finished what it was doing, so end exactly on the
    // final value. Cancelled/Superseded: just stop (the value stays where the
    // compositor has it, K5).
    void Withdraw(ControllerId id, EndReason reason) override {
        auto it = states_.find(id);
        if (it == states_.end()) return;
        State& st = it->second;
        std::map<std::string, double> finals;
        for (const auto& pp : st.plan.props)
            if (pp.kind == PlanKind::CompositorKeyframes || pp.kind == PlanKind::CompositorSpring)
                for (const auto& path : pp.paths) finals[path] = pp.finalValue;
        for (const auto& path : st.livePaths) {
            if (!StopIfOwned(st.target, path, id)) continue;
            if (reason == EndReason::Completed) backend_->SetValue(st.target, path, finals[path]);
        }
        for (const auto& prop : st.appProps) EraseAppIfOwned(st.target, prop, id);
        states_.erase(it);
    }

    // App-thread values: only for properties some plan left on the app thread.
    // Compositor-driven and held properties are ignored (the compositor owns them).
    void Apply(TargetId target, const SampleMap& values) override {
        for (const auto& kv : values) {
            auto it = app_.find({target, kv.first});
            if (it == app_.end()) continue;
            for (const auto& key : it->second.keys) backend_->SetValue(target, key, kv.second.value);
        }
    }

    const LoweringPlan* PlanFor(ControllerId id) const {
        auto it = states_.find(id);
        return it == states_.end() ? nullptr : &it->second.plan;
    }
    std::size_t LiveAnimationCount() const { return live_.size(); }

private:
    using Key = std::pair<TargetId, std::string>;
    struct State { LoweringPlan plan; std::set<std::string> livePaths, appProps; TargetId target = 0; };
    struct AppEntry { ControllerId id = 0; std::vector<std::string> keys; };

    bool StopIfOwned(TargetId t, const std::string& path, ControllerId id) {
        auto it = live_.find({t, path});
        if (it == live_.end() || it->second != id) return false;
        backend_->Stop(t, path);
        live_.erase(it);
        return true;
    }
    void EraseAppIfOwned(TargetId t, const std::string& prop, ControllerId id) {
        auto it = app_.find({t, prop});
        if (it != app_.end() && it->second.id == id) app_.erase(it);
    }

    CompositionLoweringPlanner planner_;
    ICompositorBackend* backend_;
    std::map<ControllerId, State> states_;
    std::map<Key, ControllerId> live_;        // running compositor animations by (target, path)
    std::map<Key, AppEntry> app_;             // app-thread properties by (target, property)
};

}  // namespace motion

=================== 附：code/spring.h（全文） ===================
// spring.h: closed-form damped spring. Pure math, no OS dependencies.
// Units: DIPs (or any value unit) and seconds. Revision 2.
#pragma once
#include <algorithm>
#include <cmath>

namespace motion {

struct SpringParams {
    double response = 0.35;   // undamped period in seconds (> 0)
    double damping  = 0.8;    // damping ratio (>= 0)
};

struct SpringState { double value; double velocity; };

class Spring {
public:
    // Starts at `from` with `velocity`, moving toward `target`.
    Spring(SpringParams p, double from, double target, double velocity = 0.0)
        : target_(target)
    {
        p_.response = std::max(p.response, 1e-4);
        p_.damping  = std::max(p.damping, 0.0);          // negative would grow
        w0_ = 2.0 * 3.14159265358979323846 / p_.response;
        x0_ = from - target;
        v0_ = velocity;
    }

    // Closed-form state at time t >= 0 since (re)start.
    SpringState At(double t) const {
        t = std::max(t, 0.0);
        double x, v;
        const double z = p_.damping;
        if (z < 1.0 - kEps) {                       // underdamped
            double a = z * w0_, wd = w0_ * std::sqrt(1.0 - z * z);
            double B = (v0_ + a * x0_) / wd;
            double e = std::exp(-a * t), c = std::cos(wd * t), s = std::sin(wd * t);
            x = e * (x0_ * c + B * s);
            v = e * (-a * (x0_ * c + B * s) + wd * (B * c - x0_ * s));
        } else if (z <= 1.0 + kEps) {               // critically damped
            double C = v0_ + w0_ * x0_;
            double e = std::exp(-w0_ * t);
            x = e * (x0_ + C * t);
            v = e * (C - w0_ * (x0_ + C * t));
        } else {                                    // overdamped
            double r1, r2;
            Roots(r1, r2);
            double c2 = (v0_ - r1 * x0_) / (r2 - r1), c1 = x0_ - c2;
            double e1 = std::exp(r1 * t), e2 = std::exp(r2 * t);
            x = c1 * e1 + c2 * e2;
            v = c1 * r1 * e1 + c2 * r2 * e2;
        }
        return { target_ + x, v };
    }

    // Continue from the current state toward a new target. `t` = time since
    // the last (re)start. Prefer the timeline/player, which tracks this for you.
    void Retarget(double t, double newTarget) {
        SpringState s = At(t);
        target_ = newTarget;
        x0_ = s.value - newTarget;
        v0_ = s.velocity;
    }

    // O(1) conservative bound on the time after which |displacement| < posEps
    // and |velocity| < velEps for the rest of the motion, measured from the last
    // (re)start. It is an upper bound built from the decay envelope: it may
    // exceed the true settle time by up to roughly one oscillation period.
    // Undamped (or nearly) springs never settle and return `cap`.
    double SettleTime(double posEps = 0.1, double velEps = 0.5, double cap = 10.0) const {
        posEps = std::max(posEps, 1e-12);
        velEps = std::max(velEps, 1e-12);
        const double z = p_.damping;
        double t;
        if (z < 1.0 - kEps) {
            const double a = z * w0_;
            if (a <= 1e-12) return cap;
            const double wd = w0_ * std::sqrt(1.0 - z * z);
            const double B = (v0_ + a * x0_) / wd;
            const double A = std::hypot(x0_, B);   // |x| <= A e^{-at}, |v| <= w0 A e^{-at}
            t = std::max(DecayTime(A, posEps, a), DecayTime(w0_ * A, velEps, a));
        } else if (z <= 1.0 + kEps) {
            const double C = std::abs(v0_ + w0_ * x0_), X = std::abs(x0_);
            t = std::max(CriticalTime(X, C, w0_, posEps),
                         CriticalTime(C + w0_ * X, w0_ * C, w0_, velEps));
        } else {
            double r1, r2;
            Roots(r1, r2);
            const double c2 = (v0_ - r1 * x0_) / (r2 - r1), c1 = x0_ - c2;
            const double rate = -r1;                 // slow mode dominates the tail
            const double K1 = std::abs(c1) + std::abs(c2);
            const double K2 = std::abs(c1) * std::abs(r1) + std::abs(c2) * std::abs(r2);
            t = std::max(DecayTime(K1, posEps, rate), DecayTime(K2, velEps, rate));
        }
        return std::min(t, cap);
    }

    // Brute-force fixed-step scan. TEST ORACLE ONLY (about cap/step exp+sincos calls).
    double SettleTimeScan(double posEps = 0.1, double velEps = 0.5,
                          double step = 1.0 / 240.0, double cap = 10.0) const {
        double last = 0.0;
        for (double t = 0.0; t <= cap; t += step) {
            SpringState s = At(t);
            if (std::abs(s.value - target_) >= posEps || std::abs(s.velocity) >= velEps)
                last = t + step;
        }
        return std::min(last, cap);
    }

    double Target() const { return target_; }
    const SpringParams& Params() const { return p_; }

private:
    static constexpr double kEps = 1e-6;

    // Overdamped roots, both negative, |r1| < |r2|. Computed without
    // cancellation: r2 = -(z w0 + q), then r1 = w0^2 / r2 (product of roots).
    void Roots(double& r1, double& r2) const {
        const double z = p_.damping;
        const double q = w0_ * std::sqrt(z * z - 1.0);
        r2 = -(z * w0_ + q);
        r1 = (w0_ * w0_) / r2;
    }

    static double DecayTime(double K, double eps, double rate) {
        return K <= eps ? 0.0 : std::log(K / eps) / rate;
    }

    // Smallest t after which g(t) = e^{-w t}(p + q t) stays <= eps (p, q >= 0).
    static double CriticalTime(double p, double q, double w, double eps) {
        auto g = [&](double t) { return std::exp(-w * t) * (p + q * t); };
        const double tpeak = q > 0.0 ? std::max(0.0, 1.0 / w - p / q) : 0.0;
        if (g(tpeak) <= eps) return 0.0;             // global max is already below eps
        double lo = tpeak, hi = tpeak + 1.0 / w;
        for (int i = 0; i < 64 && g(hi) > eps; ++i) hi *= 2.0;
        for (int i = 0; i < 80; ++i) {
            const double mid = 0.5 * (lo + hi);
            (g(mid) > eps ? lo : hi) = mid;
        }
        return hi;
    }

    SpringParams p_;
    double target_, w0_, x0_ = 0, v0_ = 0;
};

} // namespace motion

=================== 附：code/capi/fm_capi.h（全文） ===================
/* code/capi/fm_capi.h: C ABI over MotionSystem + SoftwareDispatcher + FramePump, for P/Invoke from
 * MotionSoftDemo (C#). Software path only: no Composition lowering.
 * RULES: call every function on ONE thread (the UI thread). Never call fm_* from inside the
 * sink/render callbacks. Strings are UTF-8. Ids are opaque; 0 means failure/none. */
#ifndef FM_CAPI_H
#define FM_CAPI_H
#include <stdint.h>
#ifdef __cplusplus
extern "C" {
#endif
#if defined(_WIN32)
#define FM_API __declspec(dllexport)
#else
#define FM_API __attribute__((visibility("default")))
#endif

typedef struct fm_system fm_system;
/* Called from fm_on_frame for every owned property, every frame while something animates. */
typedef void (*fm_sink_fn)(uint64_t target, const char* property, double value, void* user);
/* on=1: start your frame source (DispatcherQueueTimer); on=0: stop it (idle means idle). */
typedef void (*fm_render_fn)(int on, void* user);

FM_API fm_system* fm_create(fm_sink_fn sink, fm_render_fn render, void* user);       /* real clock */
FM_API fm_system* fm_create_manual(fm_sink_fn sink, fm_render_fn render, void* user); /* tests: fm_manual_advance */
FM_API void fm_manual_advance(fm_system*, double seconds);
FM_API void fm_destroy(fm_system*);

/* 0 = ok. Otherwise the number of problems; `err` receives them, one per line, JSON-Pointer paths. */
FM_API int fm_load_tokens(fm_system*, const char* json, char* err, int errCap);

FM_API void fm_set_reduced_motion(fm_system*, int on);             /* wakes the pump */

/* transition == NULL: empty animation (for Retarget only). Returns 0 if the transition is unknown. */
FM_API uint64_t fm_animation_create(fm_system*, uint64_t target, const char* transition);
FM_API void fm_animation_play(fm_system*, uint64_t anim);          /* wakes the pump */
FM_API void fm_animation_cancel(fm_system*, uint64_t anim);
FM_API void fm_animation_release(fm_system*, uint64_t anim);       /* drop OUR handle; motion keeps running */
FM_API int  fm_animation_retarget(fm_system*, uint64_t anim, const char* property, double target,
                                  double response, double damping, int hasFrom, double from);   /* 1 ok, 0 fail; wakes the pump */

FM_API void fm_on_frame(fm_system*);                               /* call from your frame callback */
FM_API int  fm_is_running(fm_system*);
FM_API int  fm_current(fm_system*, uint64_t target, const char* property, double* value, double* velocity);

#ifdef __cplusplus
}
#endif
#endif

=================== 附：docs/workbuddy_soft_demo.md 派题 #4–#6 ===================
## 派题 #4

日期：2026-10-19 18:30–18:45（Asia/Taipei），主机 DESKTOP-K93N3P8。

### 1) 代码同步

- 方法：box 上 `tar czf artifacts/code_sync.tar.gz code`（排除 Linux 二进制 `timeline_tests`/`timeline_tests_san`）→ CopyFromBox 到 `C:\Users\admin\motion_framework\code_sync.tar.gz` → Windows 上 `Remove-Item -Recurse code` 后 `tar -xzf code_sync.tar.gz`（旧 .exe/.obj 一并清掉）。
- md5 两端一致：`c864b56b5b48a4ed0506e86581b163f2`。内容 = code/ 全部头文件与测试 + `explore/`（software_dispatcher.h、software_demo_test.cpp）+ `capi/`（fm_capi.h/.cpp、fm_capi_test.cpp、demo_motion.json、csharp_pinvoke_sketch.cs）。
- 未修改任何 code/ 或 capi/ 源文件。

### 2) MSVC 回归（cl 19.44.35228 x64）

- vcvars64：`C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvars64.bat`。注意 `vswhere -latest` 会返回 VS Community 2026（18.6），为与上次金标一致，显式使用 BuildTools 2022（17.14 / MSVC 14.44.35207）。
- 标志与上次相同：`cl /nologo /std:c++17 /EHsc /O2 /W3 X.cpp /Fe:X.exe`；脚本 `C:\Users\admin\motion_framework\run_golden_reply11.bat`。

| 套件 | Windows cl | 期望（box g++） |
|---|---|---|
| timeline | 422 / 0 失败 | 422 |
| tokens（motion.json） | 743 / 0 | 743 |
| controller | 236 / 0 | 236 |
| composition | 2063 / 0 | 2063 |
| 额外 explore/software_demo_test | 10 / 0 | — |
| 额外 capi/fm_capi_test（`/I.. /I..\explore`） | 23 / 0 | 23 |

全部编译 cl_exit=0；唯一警告 `fm_capi.cpp(60): C4996 strncpy`（不影响，未改源码）。完整日志：box `artifacts/win_cl_golden_log_reply11.txt`（Windows 原始：`C:\Users\admin\motion_framework\win_cl_golden_run_reply11.txt`）。

### 3) 真实弹簧接入 MotionSoftDemo（用 Claude 的 C-ABI）

- DLL：`C:\Users\admin\motion_framework\MotionNative\fluidmotion.dll`，由 `MotionNative\build.bat` 构建：
  `cl /nologo /std:c++17 /EHsc /O2 /W3 /LD /I code\capi /I code /I code\explore code\capi\fm_capi.cpp /Fe:fluidmotion.dll`
- 导出（dumpbin 确认 14 个）：fm_create、fm_create_manual、fm_manual_advance、fm_destroy、fm_load_tokens、fm_set_reduced_motion、fm_animation_create/play/cancel/release/retarget、fm_on_frame、fm_is_running、fm_current。
- 内部依赖：`capi/fm_capi.cpp` → `explore/software_dispatcher.h`（SoftwareDispatcher + FramePump）→ `controller.h`（MotionSystem，含 spring.h/timeline.h/tokens.h）。composition.h 未用、未改。
- C# 侧（按 csharp_pinvoke_sketch.cs）：新增 `MotionSoftDemo\FmBridge.cs`（`Fm` DllImport Cdecl + `MotionBridge`）；`MainPage.xaml.cs` 去掉 sine，保留原 16ms DispatcherQueueTimer 作为帧源，每个 Tick 调 `fm_on_frame`，sink 回调把 `offsetX` 写 `BoxTranslate.X`、`opacity` 写 `AnimBox.Opacity`；渲染回调启停 timer（静止即停）。时间由引擎 SteadyClock 计；另用 Stopwatch 记录每 tick 实际 dt 写日志。
- 启动：`fm_load_tokens(demo_motion.json)` 后播放 `demo.enter`（opacity 0→1 + offsetX -80→0 弹簧）；1 s 后自检：retarget 到 240，150 ms 后中途打断 retarget 回 0。
- 指针：`RootGrid`（已有 Background）`PointerPressed` → `fm_animation_retarget(drag,"offsetX", 点击X-100)`（盒子中心对准点击处，≥0）。
- csproj：`<Content Include="..\MotionNative\fluidmotion.dll" Link="fluidmotion.dll" CopyToOutputDirectory="PreserveNewest" />`，demo_motion.json 同理；已确认进入 `bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\AppX\`。
- 原文件备份：`MainPage.xaml.bak_sine`、`MainPage.xaml.cs.bak_sine`、`MotionSoftDemo.csproj.bak`。源码副本：box `artifacts/win_softdemo_src/`。

**打断连续性测试**

- 原生（`MotionNative\continuity_test.cpp`，仅用 C-ABI，fm_create_manual，60 Hz）：飞行 150 ms 后 value=217.522813117、vel=729.152815097；retarget(0) 后立即 value=217.522813117、vel=729.152815097，**|Δv|=0、|Δvel|=0**；+0.1 ms 帧 Δ=0.07238（= vel·dt）；+16.7 ms 帧 value=216.995（一帧位移 0.53 < 打断前一帧速度位移 12.15，无跳变，速度经弹簧力平滑反向）；37 帧后静止于 0。**13 checks / 0 failures**（box g++ 结果逐位一致）。
- 应用内（真实时钟）：run1 打断前采样 208.742（vel 885.9），打断时刻状态 220.403（vel 674.2）——差值是上次采样后 15.4 ms 的正常运动，不是跳变；渲染序列 192.46 → 208.74 →（打断）219.57 → 177.13 → 120.64 平滑。run2（钉版本后）打断距上帧 0.3 ms：|Δv|=0.16、|Δvel|=3.1。日志：box `artifacts/win_app_tick_log_run1.txt`、`artifacts/win_app_tick_log_run2_pinned.txt`（Windows：`MotionSoftDemo\tick_log_run1.txt`、`tick_log.txt`）。
- 观察：16ms DispatcherQueueTimer 实际间隔多为 ~31 ms（系统 15.6 ms 定时器粒度），因引擎自带时钟，运动轨迹不受影响；若要 60fps 可改用 CompositionTarget.Rendering。

**启动结果**：`$env:WINAPP_CLI_TELEMETRY_OPTOUT='1'; dotnet run -c Debug -p:Platform=x64` → winapp “launched”；进程 `MotionSoftDemo`、窗口标题 `MotionSoftDemo`（两次：PID 41524、65652），均 `CloseMainWindow()` 正常关闭，无残留进程。

### 4) 钉死 WASDK

csproj：`Microsoft.WindowsAppSDK` `*` → `2.5.1`；`Microsoft.Windows.SDK.BuildTools` → `10.0.28000.2705`；`Microsoft.Windows.SDK.BuildTools.WinApp` → `0.7.1`（均为此前实际还原版本）。`dotnet build -c Debug -p:Platform=x64` 0 警告 0 错误；project.assets.json 确认 2.5.1 / 10.0.28000.2705 / 0.7.1；钉版本后再次启动成功（run2）。

### 5) 阻塞 / 注意

- **无阻塞**。
- 首次启动崩溃（0xc000027b，Microsoft.UI.Xaml.dll）：推测原因是日志代码在 try 外直接取 `ApplicationData.Current.LocalFolder`；改为惰性+try 并挂 `UnhandledException` 后正常。非 C-ABI 问题。
- C4996 strncpy 警告（fm_capi.cpp:60）仅提示；box g++ -O1 -Wall 对 tokens.h:218 有 `-Wmaybe-uninitialized` 警告，均未改源码。
- `fm_current` 返回的是上次 Tick 的采样，打断时刻真实状态在 retarget 内按当前时钟求值；判断连续性应比较“渲染序列”或用 manual clock（如原生测试）。

## 派题 #5

日期：2026-10-19 18:50–19:10（Asia/Taipei），DESKTOP-K93N3P8。code/、capi/ 源码未改动。

### 改动

- 备份：`MotionSoftDemo\MainPage.xaml.cs.bak_timer`、`FmBridge.cs.bak_timer`（box 副本在 `artifacts/win_softdemo_src/`）。
- `FmBridge.cs`：`MotionBridge` 不再持有 DispatcherQueueTimer；引擎渲染回调 `fm_render_fn(on)` → `SetSubscribed(on)`，on=1 时 `CompositionTarget.Rendering += handler`，on=0 时 `-=`（幂等，不会重复订阅；静止即退订）。handler 内调用 `fm_on_frame`，并把 `RenderingEventArgs.RenderingTime` 与 fm_on_frame 耗时交给页面记录。`Dispose()` 一定退订并 `fm_destroy`。dt 仍由引擎自身 SteadyClock 计算。
- `MainPage.xaml.cs`：删除 timer；Unloaded 中停止一次性自检定时器 → `_fm.Dispose()`（退订 Rendering）→ 写日志。每帧日志先存内存，统计/卸载时写文件（帧路径无文件 I/O）。指针按下仍 retarget（点击 X 居中）。
- 自检：1.0 s go→240，150 ms 后打断→0（同 #4）；2.5 s 软弹簧（response 1.2、damping 0.35）→300，3.3 s 中途打断→60，持续约 5 s 以凑足 ≥100 连续帧；9 s 输出统计。

### 构建

`dotnet build -c Debug -p:Platform=x64 --no-incremental` → **0 警告 0 错误**。

### 帧间隔（未达标）

最终一次（run E）运行中，连续帧间隔（Stopwatch，已排除静止间隙）：**count=210，mean=31.10 ms，min=13.41，p50=31.25，p95=32.06，max=32.61 ms（≈32 fps）**；同时记录的 RenderingTime 间隔：mean 30.61 / p50 31.22 / p95 32.07 / max 32.62 ms。5 次启动结果一致（mean 31.0–31.2 ms，p95≈32.1 ms），**并非 ~16.7 ms**。

排查：
- 应用侧不是瓶颈：fm_on_frame 每帧 0.013–0.035 ms；去掉每帧文件 I/O 后无变化；窗口为前台（run C，fg=True）或后台结果相同；窗口未最小化、未 cloaked。
- RenderingTime 自身就是 31.2 ms 步进（=2×15.625 ms 系统时钟节拍），说明 XAML/DWM 合成器本身以约 32 Hz 出帧。
- 显示器 PHL 1280×1024 报告 60 Hz（GTX 970），本地 console 会话、未锁屏；但用户输入已空闲约 7046 s，交流电“关闭显示器”超时为 900 s → **显示器很可能已休眠**，DWM 没有真实 vblank，退回按定时器出帧。在 shell 中设置 `SetThreadExecutionState(ES_CONTINUOUS|ES_DISPLAY_REQUIRED)` 后再跑（run E）仍是 32 fps，结束后已清除。

**阻塞**：在不模拟键鼠输入、不改电源设置的前提下无法唤醒显示器，因此无法在本机验证 16.7 ms。建议：有人在本机动一下鼠标、显示器亮起后，重跑一次 `dotnet run`（app 9 s 时自动在 `MotionSoftDemo\tick_log.txt` 写 STATS 行）即可验证。由于引擎自带时钟，帧率不影响轨迹，只影响平滑度。

### 打断连续性（仍无跳变）

- selftest-interrupt（→0）：打断距上一帧 15.63 ms；打断前最后采样 218.221（vel 716.0），打断时刻状态 227.856（vel 521.6），与“采样值+vel·dt”外推差 1.56（这 15.6 ms 内正在减速）；渲染序列 189.64 → 218.22 →（打断）224.76 → 180.03 → 119.79，平滑反向。
- stats-interrupt（→60，软弹簧）：距上一帧 31.12 ms；采样 373.590（vel −249.6）→ 打断时刻 365.337（vel −278.8），外推差 0.48；渲染序列 380.86 → 373.59 →（打断）365.32 → 353.11 → 333.95，无跳变。
- 原生 C-ABI 精确测试（#4 的 continuity_test，manual clock）：|Δv|=0、|Δvel|=0，13/0 不变。

### 订阅/退订与关闭

每次运行 Rendering SUBSCRIBE/UNSUBSCRIBE 各 3 次（enter、selftest、stats 三段，每段结束自动退订）；Unloaded 时 `subscribed=False subs=3 unsubs=3`，无泄漏。5 次启动均显示进程 `MotionSoftDemo`、窗口标题 `MotionSoftDemo`，`CloseMainWindow()` 正常退出，无残留进程。

### 文件

- 日志：box `artifacts/win_app_tick_log_60fps.txt`（含 5 次统计摘要 + 最终 run E 与首次 run A 全量逐帧日志）；Windows `MotionSoftDemo\tick_log.txt`、`tick_log_60fps_try1..4.txt`（对应 run A–D；run E 即 tick_log.txt）。
- 源码副本：box `artifacts/win_softdemo_src/`（FmBridge.cs、MainPage.xaml.cs、MainPage.xaml 与 .bak_timer）。

## 派题 #6

M1b 真机 CompositionDispatcher 单属性试点。日期：2026-10-19 19:25–19:45（Asia/Taipei），DESKTOP-K93N3P8。code/、capi/、composition.h 均未改动。**显示器仍在休眠**（输入空闲 >2 h，关屏超时 900 s），帧时序数字不具代表性。

### 实现

- 备份：`MainPage.xaml.bak_m5`、`MainPage.xaml.cs.bak_m5`、`FmBridge.cs.bak_m5`、`MotionSoftDemo.csproj.bak_m5`。
- UI：新增 `ToggleSwitch ModeToggle`（Header “动画路径”，Off=软件路径 / On=合成器路径）。指针按下：软件模式走 `fm_animation_retarget`，合成器模式启动新的 NaturalMotion 弹簧（都可打断）。
- 软件路径：与 #5 相同（CompositionTarget.Rendering → fm_on_frame → BoxTranslate.X）。`FmBridge.cs` 只新增 `RetargetFrom`（hasFrom=1），用于从合成器切回时交接数值。
- 合成器路径（C# 直接用 Microsoft.UI.Composition，未经 C++/WinRT 接 composition.h）：
  - 目标：`ElementCompositionPreview.GetElementVisual(AnimBox)`，`SetIsTranslationEnabled(AnimBox, true)`，属性 **`Translation.X`**。没有用 composition.h 默认映射里的 `Offset.X`，因为 XAML 元素 Visual 的 Offset 归布局所有，会被覆盖；以后接 planner 时可以通过 `StaticCapabilityProbe::Add("offsetX", {"Translation.X"})` 换路径，无需改源码。
  - 弹簧 → `SpringScalarNaturalMotionAnimation`：FinalValue=目标，**Period = response = 0.32 s（S1），DampingRatio = damping = 0.78（S2）**，与软件路径/demo token `springs.snappy` 参数相同；InitialValue/InitialVelocity 不设，按设计应从当前值/速度接续。
  - 贝塞尔 → `ScalarKeyFrameAnimation`：key0 `this.StartingValue`，key1 目标 + `CubicBezierEasingFunction((0.1,0.9),(0.2,1.0))`（easing.enter，按 K1 放在结束关键帧），Duration 220 ms。
  - 防重复叠加：进入合成器模式时，软件 sink 忽略 offsetX、`BoxTranslate.X = 0`、`Translation = (软件当前值,0,0)`；切回时 `StopAnimation("Translation.X")`，读取值（K5），把 Translation 置 0，再用 `RetargetFrom(值)` 交回软件路径。
  - 读回：`CompositionPropertySet` 的标量 "x" 由 `ExpressionAnimation("v.Translation.X")` 驱动，每个 Rendering 回调 `TryGetScalar`；同时读取 `Visual.Properties.TryGetVector3("Translation")`。另外加了两个诊断：静态写 Translation.X:=37；直接对 PropertySet 标量跑一个 1000 ms 线性关键帧并给 ScopedBatch 计时。
- 自检时间线：软件 go240→150 ms 打断→0；软件 0→240 静止起步；回 0；切合成器；合成器 go240→150 ms 打断→0；合成器 0→240；合成器贝塞尔→0；切回软件；软件 go120。

### 结果

- 构建：`dotnet build -c Debug -p:Platform=x64 --no-incremental` → **0 警告 0 错误**。
- 启动/关闭：进程 `MotionSoftDemo`、窗口标题 `MotionSoftDemo`，`CloseMainWindow()` 正常退出，无残留，无异常。
- **软件路径**（对照基准）：打断时 value=192.36、vel=1143.8 → 渲染 192.36 → 211.11 → 173.98 → 119.09，平滑；0→240 从静止起步，retarget 后 628 ms 收敛到 240.0000；切回后 go120 收敛到 120.0000。
- **合成器路径：无法在本机验证（阻塞）**：
  1. 表达式读回始终为 0：135 个采样全部 probe=0，**连静态写 Translation.X:=37 也读不到**（`Visual.Properties` 读到 37，probe 仍是 0）。可见 `TryGetScalar` 返回的是静态值，不是合成器求值后的值，这条读回路线行不通。
  2. 对照组：PropertySet 标量直接跑 1000 ms 关键帧，ScopedBatch **13.6 ms 就 Completed**，读到终值 100，期间没有任何中间值。1000 ms 的动画 13.6 ms 就“完成”，说明本机（显示器休眠时）合成器的动画时钟没有按真实时间推进。
  3. 三个弹簧 batch 都没有自行完成，直到 220 ms 的贝塞尔 batch 在 29.9 ms 时完成，才一起触发 Completed。
  4. 像素旁证也拿不到：`PrintWindow(PW_RENDERFULLCONTENT)` 截到的客户区全白；`CopyFromScreen` 取单行像素始终找不到蓝色方块（显示器休眠）。辅助代码见 `artifacts/win_softdemo_src/cap_helper2.cs.txt`，没有保存任何截图。
  - 因此合成器模式下的**启动延迟、打断平滑度、终值与软件路径的一致性均未测得**。模式切换本身正常：无异常，合成器模式下 BoxTranslate.X=0，切回时 Translation 归 0，软件路径从交接值继续。
- 下一步建议：显示器亮着时重跑（应用约 11 s 自动跑完自检，写 `MotionSoftDemo\comp_pilot_log.txt`）。读回可改用 `Windows.Graphics.Capture`（GraphicsCaptureItem.CreateFromVisual）对 AnimBox 逐帧截图测位置；或在 C++ 侧用 DComp 的 IDCompositionDevice::GetFrameStatistics 核对合成器时钟。

### 文件

- 日志：box `artifacts/win_comp_pilot_log.txt`（摘要 + 最终一次全量日志）；Windows `MotionSoftDemo\comp_pilot_log.txt`、`build_m6_log.txt`。
- 源码副本：box `artifacts/win_softdemo_src/`（MainPage.xaml、MainPage.xaml.cs、FmBridge.cs、MainPage.xaml.cs.bak_m5、cap_helper2.cs.txt）。

（其余参考：目录中 api.md、explore_round.md、deepseek_explore.md、bezier.h。）

```

## S1 映射（v2，2026-10-20 裁决，替代 v1 Period=response）
- Period = r / (2π)；ζ_c = ζ。依据：#36 s=0.159≈1/(2π)（9/9 格）；#41 best-τ RMS 0.07–0.20%（10/10）。
- 合成器语义：Period 按 ω₀ = 1/Period 解释，而非 T₀ = Period。
- 实现：composition.h 单一映射函数；response ≤ 0 时拒绝/断言；注释引用 #36/#41。
- 影子模型：x_scr(t) = spring.h(r, ζ; t − τ)，τ_model = 33 ms，容差 [28,45] ms。
- 旧口径 R-1（A 28.6% / E 12.1%）作废，仅作历史对照。

## R-1 判据（v2）
- 形状：逐次 τ∈[25,50] ms 内取 best-τ，RMS ≤ 0.5% travel（逐次全部满足）；best-τ 记日志。
- 延迟：逐格 τ 中位 ∈ [28,45] ms。固定 33 ms 下的残差仅作参考，不作门。
- 关闭条件：正式构建复跑 10 格×5 次 + #41b 联合 (τ, s) 拟合结论 + C5(b) 第二台机器同轮确认（失败则重开）。

## M1b 打断（v2）
- 维持 hybrid B（WGC 末两帧速度，IV=null）为 M1b 缓解。
- A 臂（spring.h seed）存在 seed 时钟与屏幕时钟错位：屏幕显示 spring.h(t−τ)，seed 取 spring.h(tInt+lead)，seed 在前。须用负 lead = −Δt_eff 标定（Δt_eff 由 #41b 按 (x_pre, v_pre) 反解）。
  - **〔已取代，#43〕负 lead 已否定**（J 随 X 单调增大；#42 错位为线性外推估计器假象），删除 Am 系列；见 §5.3.5 §6。#45 起 M1b 曾以 hybrid B 条件冻结 R-2（已被 §5.3.7 取代：#47 后 R-2 本机 FAIL 结题，M1b 降级实现 B_v，不满足 R-2）。
- 判定用 R-2′ 门：N_h = remap 下 base J_null 第 19 小（逐格重建）；N_h_eff = max(N_h, 1 物理像素)；
  单次通过 = J_vis ≤ N_h_eff 且 Δv_vis ≤ max(20, 5%|v_pre|)；
  臂通过 = 双判据 ≥ 17/20 且 J 中位 95% CI 上界 ≤ N_h_eff 且 Δv 中位 ≤ 阈值中位。
- 选主：通过臂中 J 中位最低者；A 臂与 hybrid B 都通过时优先 A，B 作后备。
