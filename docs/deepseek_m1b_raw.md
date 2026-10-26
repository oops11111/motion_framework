# DeepSeek M1b 原始回复

- 时间：2026-10-19 23:41:39 Asia/Taipei
- session：`session-05cb9ffd-6ede-454c-add3-6669147890e7`
- model：`deepseek-account` / `deepseek-v4-pro` / `reasoningEffort=max`
- replyChars（脚本汇总，可能含其它 assistant/message 拼接）：59763
- 下列正文取自 turn/end 前最后一条 `assistant/message` 的 text 块（不含 reasoning）

---

已核对完毕（本地 `api.md`/`explore_round.md`/`bezier.h` + 官方 API 文档）。以下是规格正文。

---

# M1b 合成器路径规格（FluidMotion / Windows 桌面端）

> 适用范围：`composition.h` 的 lowering 到 **Microsoft.UI.Composition（WinUI3 / WASDK 2.5.1）** 的精确映射与真机验证方案。
> 状态标记：「已文档化」= 官方文档明确；「S1–S5 / K1–K5」= 沿用 `composition.h` 未验证假设；「待真机验证」= 文档未明说或本机（显示器休眠）无法验证的行为。
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

能力探测（模块 7）：用 `ApiInformation.IsTypePresent("Microsoft.UI.Composition.SpringScalarNaturalMotionAnimation")` 等把结果写回 `Capabilities`；具体可探测类型清单与最低 OS 门槛**待真机验证**（本机 build 26100 应全通过）。

### 1.2 弹簧：SpringScalarNaturalMotionAnimation

#### 物理换算（m=1 假设）

弹簧 ODE（x = value − target）：`x″ + 2ζω₀·x′ + ω₀²·x = 0`，其中 `ω₀ = 2π/response`。

| 量 | 公式（m=1） | 一般质量 m |
|---|---|---|
| 刚度 k | `k = ω₀² = (2π/response)²` | `k = m·(2π/response)²` |
| 阻尼系数 c | `c = 2ζω₀ = 4πζ/response` | `c = 2ζm·(2π/response)` |
| 反解 Period | `Period = 2π/√k` | `Period = 2π√(m/k)` |
| 反解 DampingRatio | `DampingRatio = c/(2√k)` | `= c/(2√(k·m))` |

**Period 的语义（S1）**：官方文档表述为 “the time for the Spring to complete a single oscillation”（[SpringVector3NaturalMotionAnimation.Period](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.springvector3naturalmotionanimation.period)），未明说无阻尼/有阻尼。默认按**无阻尼周期**理解：`Period = response`、`DampingRatio = damping`（即 `SpringMapping` 两个 scale = 1）。

- 若真机证明 Period 是**阻尼周期**输入：阻尼振荡周期 `T_d = T₀/√(1−ζ²)`（欠阻尼），则 `Period = response/√(1−ζ²)`；ζ≥1 无振荡，无法用峰峰法，用包络衰减 `e^(−ζω₀t)` 反推 ω₀。
- **判别法（真机第一测）**：ζ=0.78、response=0.32 s 时，若按无阻尼解释，实测峰峰间隔应为 `0.32/√(1−0.78²) ≈ 0.51 s`；若按阻尼周期解释则为 0.32 s。两分支换算公式全部保留在 `SpringMapping`（或替换 `ToCompositor`），一处修正全局生效。
- `DampingRatio`（S2）：默认 `= damping`。ζ>1（过阻尼）是否被接受、是否截断到 1：**待真机验证**（v1 对 ζ>1 弹簧暂不 lower，回 AppThread，直到真机确认）。
- `Period` 的钳制（极小 response，token 允许 0.001 s）：**待真机验证**。

#### 参数映射

| `SpringAnimation` 字段 | API |
|---|---|
| `target` | `SpringScalarNaturalMotionAnimation.FinalValue = target` |
| `period` / `dampingRatio` | `Period = TimeSpan.FromSeconds(period)`；`DampingRatio = dampingRatio` |
| `from` | `InitialValue = (float)from`（C# 投影为 `Nullable<float>`；**必须显式设**，见 §2） |
| `fromVelocity` | `InitialVelocity = (float)fromVelocity`。文档：默认值 **0**（[InitialVelocity](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.scalarnaturalmotionanimation.initialvelocity)）→ 不设=从 0 起跳，**不会继承速度**（S3 单位「值单位/真实秒」**待真机验证**） |
| `delay` | `CompositionAnimation.DelayTime = TimeSpan.FromSeconds(delay)`（真实秒，不折算） |
| `presetValue` | 先 `visual.Properties.InsertVector3("Translation", …)` / `SetValue(presetValue)`，再 `StartAnimation`；`DelayBehavior` 保持默认 `SetInitialValueAfterDelay`（官方语义：延时期间动画不写属性、属性保持当前值 → 即 presetValue；[AnimationDelayBehavior](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.animationdelaybehavior)）。延时结束无跳变要求 `presetValue == InitialValue`（S4 整体**待真机验证**） |
| 启动 | `visual.StartAnimation("Translation.X", spring)`；已在跑的动画被**隐式替换**（`ICompositorBackend` 的“Start\* REPLACES”语义） |

#### rate 折叠（不用 PlaybackRate）

设计：**不**对 NaturalMotion 动画设置 `AnimationController.PlaybackRate`（对自然运动是否生效**待真机验证**），把 rate 折进参数：

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
| `Withdraw(Cancelled/Superseded)` | `StopAnimation(path)`，值停在当前（K5，与 [AnimationStopBehavior](https://learn.microsoft.com/en-us/uwp/api/windows.ui.composition.animationstopbehavior) 的 `LeaveCurrentValue` 语义一致；**待真机验证**） |
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

- **K1（easing 作用段）**：官方文档称 easing 用于“帧间插值”（[InsertExpressionKeyFrame](https://learn.microsoft.com/en-us/uwp/api/windows.ui.composition.keyframeanimation.insertexpressionkeyframe)），即**结束于该关键帧的那一段**；与 `KeyframeSpec` 注释一致。线性段不带 easing。**待真机验证**（用单段 220 ms + `decelerate(0.1,0.9,0.2,1.0)` 对照 `EvaluateKeyframes` 即可）。
- **首帧**：优先 `InsertExpressionKeyFrame(0f, "this.StartingValue")` —— `this.StartingValue` = 动画开始时该属性值（文档化表达式关键字），保证起跑无跳变；仅当 app 侧状态与合成器当前值确定一致时才用显式值（推荐仍用表达式首帧，省去一致性论证）。
- **K2（y 越界）**：`CreateCubicBezierEasingFunction` 文档**未声明**控制点 y∈[0,1] 约束（[Compositor.CreateCubicBezierEasingFunction](https://learn.microsoft.com/en-us/uwp/api/windows.ui.composition.compositor.createcubicbeziereasingfunction)）→ 行为（拒绝/截断/生效）**待真机验证**。规划器默认 `bezierOvershoot=false`：y 越界段**不 lower**，整轨回 AppThread；真机确认支持后置 `Capabilities::bezierOvershoot=true`。
- **elapsed>0 / rate≠1 / reversed（K3）**：`var ctl = visual.TryStartAnimation(path, anim)` 拿 `AnimationController`：
  - `Progress = elapsed / duration`（正向）；`Progress = 1 − elapsed / duration`（反向，`elapsed` 为距顶部的已播放量）；
  - `PlaybackRate = rate`（正向）/ `−rate`（反向）。官方文档：PlaybackRate 范围 **[−16, 16]**，负值反向（[AnimationController.PlaybackRate](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.animationcontroller.playbackrate)）；`AnimationController` 无 Direction 属性，反向=负 PlaybackRate。
  - **rate>16 时无法用 PlaybackRate 表达** → 该属性回 AppThread（reason 记录）。
  - Progress 写入提交后是否立即生效、与 DelayTime 的相互作用：**K3 待真机验证**。失败回退：关键帧中段提交回 AppThread（或按 §2.4 影子状态重启）。
- **DelayTime / DelayBehavior**：同弹簧（`DelayTime` + 默认 `SetInitialValueAfterDelay` + 先 `SetValue(presetValue)`）。注意 `SetInitialValueBeforeDelay` 的官方语义是“延时前先应用 Keyframe 0 的值”——与我们的 presetValue 模式不符，**不要用**。
- **K4**：关键帧动画完成后属性保持终值（试点在 ScopedBatch 完成后读到终值 100，但为熄屏数据；**待真机验证**）。

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

原理：弹簧 ODE 时不变，从当前 (value, velocity) 重启等价于继续原轨迹后改目标。**必须显式设 `InitialValue` / `InitialVelocity`**：
- `InitialValue` 文档语义：null 时“使用被动画属性的起始值”（[InitialValue](https://learn.microsoft.com/en-us/uwp/api/windows.ui.composition.scalarnaturalmotionanimation.initialvalue)）——依赖合成器内部状态，不采用；
- `InitialVelocity` 默认 **0**，不继承 → 不显式设必跳变。
- 若 override 无 rate 概念（`SpringOverrideInfo` 无 rate），按 rate=1 处理。

### 2.2 关键帧中段（K3 路径）

用 `this.StartingValue` 首帧 + `ctl.Progress = elapsed/duration` + `PlaybackRate = ±rate` 定位（§1.3）。**不做**“用闭式解重算值当首帧”的近似（会丢失段内速度方向连续性）。K3 验证失败 → 该属性回 AppThread，并在 `reason` 注明。

### 2.3 读回能力矩阵（能/不能拿到“正在动画属性的当前值”）

| 通道 | 官方语义 | 试点实测（熄屏） | 结论 |
|---|---|---|---|
| `CompositionPropertySet.TryGetScalar/TryGetVector3` | 文档**未承诺**返回动画求值后的值（[CompositionPropertySet](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.compositionpropertyset)） | 恒为静态值：135 采样全 0；静态写 `Translation.X:=37` 也读不到（表达式未重求值） | **不可用**作动画值读回；亮屏下是否逐帧更新**待真机验证**，仍不作设计依赖 |
| `Visual.Properties.TryGetVector3("Translation")` | 同上（属性集存储语义） | 读到静态写 37 | 仅静态值；动画中值**待真机验证**，不依赖 |
| `StopAnimation` 后读回 | `StopAnimation` 停在当前值（K5 / `AnimationStopBehavior.LeaveCurrentValue`） | 熄屏未验证 | 预期可行（值被“物化”为静态）；关键帧中段与弹簧中段分别**待真机验证** |
| `AnimationController.Progress` | 进度 0..1，无“值”通道 | 熄屏未验证 | 关键帧可经 `EvaluateKeyframes(keys, Progress·duration)` 反解值；弹簧无此通道。K3 **待真机验证** |
| XAML DP（如 `element.Translation`） | 合成器动画绕过 XAML 属性系统 | — | 预期不反映动画值；**待真机验证**，不依赖 |
| `ExpressionAnimation("v.Translation.X")` → PropertySet → `TryGetScalar` | — | 熄屏下表达式不重求值（恒 0） | 试点否定；亮屏行为**待真机验证**，不作设计依赖 |
| Windows.Graphics.Capture 像素 | 合成器真实输出 | 熄屏无帧 | **亮屏唯一 ground truth**（§3.1） |

### 2.4 回退方案（app 影子状态，设计主保险）

`CompositionDispatcher` 为每个 `(target, property)` 维护影子状态 `(value, velocity, tLast)`：
- 每次 `Submit` 前用 `spring.h::At` / `EvaluateKeyframes` + `bezier.h::Slope`（数值微分）从 `tLast` 外推到 `snapshot.clockTime`，得到确定性种子；
- 合成器→软件切换（试点已实现）：`StopAnimation` → 尝试读回（§2.3 第 3 行）；若读回值与影子状态偏差 > 阈值或读回失败 → 用影子状态，`fm_animation_retarget(..., hasFrom=1, from=影子值)` 交接；
- 该影子状态同时是验收时“无跳变”判定的预测基准。

---

## 3. 可靠测量合成器路径

### 3.1 Windows.Graphics.Capture（像素级，主通道）

- `GraphicsCaptureItem.CreateFromVisual(ElementCompositionPreview.GetElementVisual(AnimBox))`（Windows 10 1809+，本机 26100 满足；[CreateFromVisual](https://learn.microsoft.com/en-us/uwp/api/windows.graphics.capture.graphicscaptureitem.createfromvisual)）。捕获自身 Visual 无需 picker/consent；WASDK 2.5.1 桌面应用调用路径**待真机验证**。
- `Direct3D11CaptureFramePool.Create(d3dDevice, Bgra8, 2, size)` → `pool.CreateCaptureSession(item)` → `StartCapture()`；`FrameArrived`：`frame.Surface` → `CreateDirect3D11SurfaceFromDXGIResource` → `CopyResource` 到 STAGING → `Map` → 扫蓝色块 bbox（质心/边梯度亚像素）→ `DIP = px / XamlRoot.RasterizationScale`。
- 时间戳：`frame.SystemRelativeTime`（QPC 域）与 `Stopwatch.GetTimestamp()` 对齐，逐帧记录 `(t, x)`。
- **亮屏**：逐合成帧出帧，60 Hz 下可测到帧级轨迹；**熄屏**：预期无帧或长时间停帧（**待真机验证**）→ 数据判无效。
- 局限：只含合成器内容（纯色块 OK；文本等 XAML 混合渲染**待真机验证**）。

### 3.2 DComp 帧统计（合成器时钟探针，C++ 侧）

| API | 说明 | WinUI3 可拿性 |
|---|---|---|
| `IDCompositionDevice::GetFrameStatistics(DCOMPOSITION_FRAME_STATISTICS)` | 该设备 lastFrameTime / currentCompositionRate / nextEstimatedFrameTime | XAML 合成器的 `IDCompositionDevice` **无公开互操作**（预期拿不到，**待真机验证**确认）。可自建设备（`DCompositionCreateDevice` + D3D11），但需它自己有提交（挂一个哑视觉 + 定时提交）统计才会推进（**待真机验证**） |
| `DCompositionGetFrameId(COMPOSITION_FRAME_ID_TYPE, COMPOSITION_FRAME_ID*)` | **全局**最新合成帧 id，无设备参数，Windows build **22000+**（本机 26100 可用，dcomp.lib；[DCompositionGetFrameId](https://learn.microsoft.com/en-us/windows/win32/api/dcomp/nf-dcomp-dcompositiongetframeid)） | 可（桌面 C++）——**首选探针**，不依赖自建设备提交 |
| `DCompositionGetStatistics(frameId, COMPOSITION_FRAME_STATS*, …)` | 指定帧的统计，build 22000+（[DCompositionGetStatistics](https://learn.microsoft.com/en-us/windows/win32/api/dcomp/nf-dcomp-dcompositiongetstatistics)） | 可 |

亮屏预期：帧 id 按 vblank 单调推进、速率=刷新率；熄屏预期：停滞或按 15.6 ms 定时器网格缓慢推进（**待真机验证**）。用「2 秒内帧 id 无变化」作为**熄屏判据**与运行门禁。

### 3.3 CompositionTarget.Rendering

XAML 渲染帧回调（`RenderingEventArgs.RenderingTime`）。亮屏 ≈ 16.7 ms@60 Hz；熄屏试点实测 **31.2 ms = 2×15.625 ms**（系统定时器粒度）。用途：帧间隔统计与软件路径泵；**不是**动画时钟真值。

### 3.4 DwmGetCompositionTimingInfo

`DWM_TIMING_INFO`（`qpcVBlank` / `qpcCompose` / `rateCompose`，dwmapi，C# P/Invoke）。亮屏逐 vblank 推进；熄屏 `qpcVBlank` 预期停走（**待真机验证**）→ 与 §3.2 一起做熄屏判据。

### 3.5 CompositionScopedBatch 计时

官方语义：Completed = “batch 内动画与效果全部完成”（[CompositionScopedBatch.Completed](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.compositionscopedbatch.completed)）。试点熄屏实测 1000 ms 关键帧 batch **13.6 ms** 即 Completed 且读到终值 100、弹簧 batch 直到 29.9 ms 才随他人触发——与官方语义矛盾，唯一自洽解释是**熄屏时合成器动画时钟不按真实时间推进**。用途：亮屏下测提交→处理往返与完成信号（动画本身无 Completed 事件）；熄屏数据一律标注无效。**亮屏行为待真机验证**。

### 3.6 亮屏 vs 熄屏可信度对照

| 方法 | 亮屏 | 熄屏（无 vblank） | 可信度 |
|---|---|---|---|
| WGC 逐帧像素 | 真实帧，帧级轨迹 | 预期无帧（待验证） | 高（亮屏唯一真值） |
| DCompositionGetFrameId | 逐 vblank 递增 | 停滞/定时器网格（待验证） | 高（时钟探针） |
| DwmGetCompositionTimingInfo | 逐 vblank | qpcVBlank 停走（待验证） | 高（判据） |
| CompositionTarget.Rendering | ≈16.7 ms | 31.2 ms（已实测） | 中（仅帧间隔） |
| ScopedBatch | 正常 | 时钟异常快进（已实测） | 中（仅提交延迟/完成信号） |
| TryGetScalar / TryGetVector3 | 待验证 | 恒静态值（已实测） | 低 |
| PrintWindow / CopyFromScreen | 可用 | 全白/找不到像素（已实测） | 低（不采用） |

### 3.7 具体测量流程

1. **门禁**：启动后 2 s 轮询 `DwmGetCompositionTimingInfo` + `DCompositionGetFrameId`；帧 id 停滞 → 判熄屏，本次运行全部数据打 `invalid(display-asleep)`，照常跑完以验证“无效而非失败”路径。
2. **标定**：静态写 `Translation.X:=37`，WGC 验证像素位移与 DIP 换算（scale、bbox 检测阈值）。
3. **场景矩阵**（每场景独立起停）：弹簧 0→240（snappy 0.32/0.78）；150 ms 打断→0；弹簧 rate=2；贝塞尔 0→240@220 ms；hold；反向关键帧（若 K3 可用）。
4. **记录**：`StartAnimation` 调用时刻（QPC）→ WGC 逐帧 `(SystemRelativeTime, px)` + ScopedBatch 提交延迟 + Rendering 帧间隔，日志先内存后落盘。
5. **离线比对**：WGC 轨迹 vs `spring.h` 闭式解 / `EvaluateKeyframes`（同参数、同种子）→ RMS、最大偏差、终值误差；打断点前后采样 vs 闭式解外推 → 跳变量。
6. **重复**：亮屏 ≥5 次；熄屏 1 次仅验无效路径。

---

## 4. 验收标准（量化）

| 指标 | 阈值（60 Hz 亮屏） | 备注 |
|---|---|---|
| 启动延迟 | p50 ≤ 2 帧（33 ms），p95 ≤ 3 帧（50 ms），上限 200 ms | `StartAnimation`（QPC）→ WGC 首个 `|Δpx| ≥ 0.5` 帧 |
| 打断位置连续性 | `|Δv| ≤ 0.5 px` 且 ≤ 5% 该帧正常位移 | 打断前后采样与闭式解外推之差 |
| 打断速度连续性 | `|Δvel| ≤ max(20 px/s, 5%·|v|)`；无方向反向尖峰 | 同上 |
| 终值误差 | 完成+1 s：`|v−target| ≤ 0.1 px`（Withdraw 快照后应 = 0） | 无快照时允许 ≤ 0.5 px（S5 差异，待测后收紧） |
| 轨迹偏差（弹簧/关键帧） | RMS ≤ 1%·行程，max ≤ 2%·行程（行程=|target−from|） | vs spring.h / EvaluateKeyframes，含 rate=2 场景 |
| 帧间隔（CompositionTarget.Rendering） | p50 ≤ 18 ms，p95 ≤ 25 ms | 仅亮屏；用于平滑度而非轨迹 |
| 稳定性 | 5 次运行 0 崩溃/0 异常；Rendering 订阅=退订；软件↔合成器模式切换 100% 成功且交接 `|Δv| ≤ 0.5 px` | |
| 熄屏 | 判 **invalid 而非 fail**（附帧 id 停滞证据）；应用不挂死（ScopedBatch 超时保护） | 见 §3.7 |

---

## 5. 待真机验证清单

1. **S1**：`Period` 是否=无阻尼周期。用 ζ=0.78/response=0.32 s 测峰峰间隔：≈0.51 s=无阻尼解释，0.32 s=阻尼周期解释；若为后者启用 `Period = response/√(1−ζ²)`。
2. **S2**：`DampingRatio` 是否=ζ；ζ=0.78 下拟合包络 `e^(−ζω₀t)` 反推 ω₀ 交叉验证；ζ>1 是否被接受/截断。
3. **S3**：`InitialVelocity` 单位是否为「值单位/真实秒」且与 `Period` 独立（rate 折叠后轨迹与闭式解一致）。
4. **S4**：`DelayTime` + 默认 `SetInitialValueAfterDelay` 期间属性是否保持 presetValue、延时结束无跳变（presetValue==InitialValue 时）；`SetInitialValueBeforeDelay` 行为（仅记录，不使用）。
5. **S5**：合成器弹簧自身停点阈值；`Withdraw(Completed)` 快照前后终值误差；停止后是否保持最终值。
6. **K1**：`InsertKeyFrame` 的 easing 是否作用于“结束于该关键帧的段”（单段 220 ms + decelerate 对照 `EvaluateKeyframes`）。
7. **K2**：`CubicBezierEasingFunction` 对 y∉[0,1] 的行为（生效/截断/拒绝）；x 是否必须∈[0,1]；确认后才开 `bezierOvershoot`。
8. **K3**：`TryStartAnimation` → `AnimationController`：`Progress` 写入提交后是否立即生效（含与 DelayTime 组合）；`PlaybackRate=±rate`（含负值反向）对关键帧动画是否生效、精度如何。
9. **K4**：关键帧动画完成后属性是否保持终值。
10. **K5**：`StopAnimation` 是否停在当前动画值（关键帧中段/弹簧中段分别测）；停止后 `TryGetScalar`/`TryGetVector3` 是否可读回冻结值。
11. `InitialValue` 不设（null）时是否如文档所述使用被动画属性当前值（仅记录，不作为设计依赖）。
12. `CompositionPropertySet.TryGetScalar` / `Visual.Properties.TryGetVector3` 对**正在动画的属性**的读回语义（亮屏：静态值 or 逐帧值）。
13. `ExpressionAnimation` 驱动 PropertySet 标量 + `TryGetScalar` 逐帧读回（亮屏）是否可行。
14. `CompositionScopedBatch.Completed` 在亮屏下是否=“动画全部完成”、时长=动画时长；能否作为完成事件使用。
15. rate>16 时 `PlaybackRate` 行为（超出官方 [−16,16] 范围）；NaturalMotion 动画设置 `PlaybackRate` 是否生效。
16. WGC `GraphicsCaptureItem.CreateFromVisual` 在 WASDK 2.5.1 桌面应用的可用性与捕获内容范围（纯色块应可，文本待验）；熄屏时 `FrameArrived` 行为；`frame.SystemRelativeTime` 与 QPC 对齐方式。
17. `DCompositionGetFrameId`/`DCompositionGetStatistics`（build 22000+）在本机可用性与返回值语义；自建 `IDCompositionDevice` 加哑视觉时 `GetFrameStatistics` 是否随全局合成推进；两者熄屏时行为。
18. `DwmGetCompositionTimingInfo` 熄屏时 `qpcVBlank`/`qpcCompose` 是否停走（熄屏判据可靠性）。
19. WinUI3 下能否经任何公开途径取得 XAML 合成器的 `IDCompositionDevice`（预期不能）。
20. NaturalMotion 弹簧轨迹是否按真实时间推进（熄屏时是否如 ScopedBatch 现象所示异常快进/停走；亮屏是否与刷新率解耦）。
21. `InsertExpressionKeyFrame(0, "this.StartingValue")` 在 WASDK 2.5.1 的语义（=提交前属性值）与中断重提交时的行为。
22. 弹簧 rate 折叠公式（period/rate、velocity×rate）真机轨迹与闭式解偏差（RMS/max）。
23. `Period` 极小值（response=0.001 s）与 `DampingRatio` 全范围（0、0.35、0.78、1.0、1.2）的可接受域/钳制行为。
24. `StartAnimation` 对已存在动画的隐式替换行为（含替换瞬间值连续性，与 K5 交叉）。

---

**引用**：[Period](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.springvector3naturalmotionanimation.period) ・ [InitialValue](https://learn.microsoft.com/en-us/uwp/api/windows.ui.composition.scalarnaturalmotionanimation.initialvalue) ・ [InitialVelocity](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.scalarnaturalmotionanimation.initialvelocity) ・ [AnimationController.PlaybackRate](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.animationcontroller.playbackrate) ・ [InsertExpressionKeyFrame](https://learn.microsoft.com/en-us/uwp/api/windows.ui.composition.keyframeanimation.insertexpressionkeyframe) ・ [AnimationDelayBehavior](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.animationdelaybehavior) ・ [AnimationStopBehavior](https://learn.microsoft.com/en-us/uwp/api/windows.ui.composition.animationstopbehavior) ・ [CompositionScopedBatch.Completed](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.compositionscopedbatch.completed) ・ [CompositionPropertySet](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.compositionpropertyset) ・ [CreateCubicBezierEasingFunction](https://learn.microsoft.com/en-us/uwp/api/windows.ui.composition.compositor.createcubicbeziereasingfunction) ・ [CreateFromVisual](https://learn.microsoft.com/en-us/uwp/api/windows.graphics.capture.graphicscaptureitem.createfromvisual) ・ [DCompositionGetFrameId](https://learn.microsoft.com/en-us/windows/win32/api/dcomp/nf-dcomp-dcompositiongetframeid) ・ [DCompositionGetStatistics](https://learn.microsoft.com/en-us/windows/win32/api/dcomp/nf-dcomp-dcompositiongetstatistics)
