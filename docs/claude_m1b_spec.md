# M1b 规格：真实合成器单属性试点（WinUI 3 / Microsoft.UI.Composition）

状态：规格，未实现（**修订 4**：对照 Claude FINAL DeepSeek 评审 + workbuddy 派题 #14–#22 真机更新）。事实分五级：**[文档]** 已在 Microsoft Learn / API 目录查到；**[实测]** 我方直接可见的试点结果；**[转述]/[device]** workbuddy 真机结果，经 DeepSeek/Claude 转述，**我没看到原始日志**；**[推断]** 我的推理；**[未验证]** 文档查不到或沉默。
已知基线：box g++ 与 MSVC 19.44 四套测试 422/743/236/2063 全过；Windows 软件路径打断连续性 13/0（原生 C-ABI，|Δv|=|Δvel|=0）。
**本版相对修订 3 的主要变化（Claude FINAL 四点 Agree）：** ① **H1 settled**（`Period=response`, `ζ_c=ζ`）；H2/H3 rejected；证据局限仅作 record（§2.1）；② 产品形状残差以 **S1 已发货 28.6%/12.1%** 为准（扫参最优 8%/6% 仅非产品变体）；固有差异结为 **accepted risk R-1 / known-fail**，§4 ≤1% 门槛不放宽、不调 Period/ζ 以求过关；③ B **不在 M1b 建造**；Seed = `spring.h::At(elapsed + seedLead)`；速度来自引擎非合成器（§3）；④ ζ>1：`composition.h` **硬门控已落地**；v1 不下发；日后经 `overdampedSpring` + T-S2 再抬（§2.4）；⑤ S3–S5 / K1–K5 写入真机事实。

## 0. 范围

- 一个目标（一个 Border 的 Visual），每次一个属性：`opacity`（keyframe+bezier）、`offsetX`（spring、可打断）。
- 经 `composition.h` 的 `CompositionDispatcher` → 新增 C# `WinRtBackend`；软件路径保留作对照。
- **不做**：keyframe 的 elapsed>0 / rate≠1 / reverse、loop、多轨同属性、chained stages（计划器会走 AppThread/Hold 回退）。
- **前置 / 已落地：**
  - `Capabilities`：`keyframePartialStart / keyframeRate / keyframeReverse` 仍保持 false（K3 余项未测完：Progress+Delay、|rate|>16、reverse+Progress）；弹簧中途重启与 rate 折叠靠参数完成，不需要控制器；
  - **ζ>1 硬门控已落地**于 `composition.h`（`c.damping > 1.0` → AppThread）；能力位 `overdampedSpring` 是日后抬门控的路径（默认 false），见 §2.4；
  - 规划器 `seedLead`（前瞻秒数，默认 0），见 §3；Seed = `spring.h::At(elapsed + seedLead)`。
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
| InitialVelocity | “程序化初速度”；**S3 [device]：单位为 DIP/真实秒，与 `Period` 独立**；缺省 0、不继承旧动画速度 | [文档]单位曾沉默；单位/独立性 [device]；缺省 [转述] |
| `CompositionObject` 方法 | `StartAnimation(string, CompositionAnimation)`、`StartAnimation(string, CompositionAnimation, AnimationController)`、`StartAnimationGroup`、`StopAnimation`、`StopAnimationGroup`、`TryGetAnimationController(string)`；**没有 `TryStartAnimation`** | [文档]（API 目录成员清单）；workbuddy 亦未找到 [转述] |
| `TryGetAnimationController` | 文档：“返回指定属性上正在运行的动画的 AnimationController”；对 KeyFrame 非 null，对 **NaturalMotion 恒为 null**；**`TryStartAnimation` 不存在** | 前半 [文档]；NaturalMotion null / 无 TryStart [device] |
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
| 弹簧形状残差（亮屏复测） | **产品/S1 已发货残差** RMS ≈ **28.6%（A）/ 12.1%（E）**；扫参非产品变体最低 ≈8%/6%（如 T₀=0.1512）—**勿当作产品残差**；已结为 accepted risk R-1 / known-fail（§4 ≤1% 门槛不放宽） | [转述]/[device] |
| 亮屏 vs 熄屏 | 亮屏：`DwmFlush`≈60 Hz、WGC≈60 Hz、`CompositionTarget.Rendering` 仍≈31 ms；熄屏：WGC 仍约 46 fps 到帧但**画面冻结** | [转述] |
| `DwmGetCompositionTimingInfo.cRefresh` | Win10 19045 上亮屏也几乎不涨，不可靠 | [转述] |
| `DCompositionGetFrameId`/`GetStatistics` | Win10 19045 返回 `E_NOTIMPL`，22000+ 才可用 | [转述] |
| 呈现/截屏滞后 | 动画开始到 WGC 看见运动约 160–200 ms | [转述] |
| 亮屏下已过的判据 | 打断位置/速度、终值、贝塞尔形状已过（派题 #11–#12） | [转述]；比对方式见 §4 M-4 的保留意见 |
| S4 DelayTime/preset | PASS：延时期间保持 preset；IV==preset 无跳变；IV≠preset 结束跳 IV（派题 #16） | [device] |
| S5 停点/保持 | 到位后保持终值；中段 Stop 冻结；`Completed` 偏晚（派题 #17） | [device] |
| K1 easing 作用段 | 结束关键帧 easing 塑造该段；WGC vs BezierEase RMS≈**0.57%**（派题 #19） | [device] |
| K2 y∉[0,1] | **HONOR**（非 reject/clamp）→ `bezierOvershoot=true`（派题 #20） | [device] |
| K3 Progress/PlaybackRate | Progress 立即生效且时间归一化；PlaybackRate 2/−1 可用；Progress+Delay、|rate|>16、reverse+Progress 未测；keyframe 能力旗标保持 false（派题 #22 PARTIAL） | [device] |
| K4 终值保持 | KeyFrame 完成后保持终值（|err|=0）（派题 #21） | [device] |
| K5 Stop/TryGet | Stop 冻结；`TryGet` 不可靠同步物化 → 用 Stop+`SetValue`（派题 #18） | [device] |
| `ElementCompositionPreview.SetIsTranslationEnabled` | WASDK `Microsoft.UI.Xaml.Hosting` 中存在；弹簧文档示例对元素 Visual 用 `"Translation.X"` | [文档] |
| `InitialValueExpression["FinalValue"]=…` | 弹簧文档示例写法；类成员列表里没看到该成员 | [未验证] |

## 2. 映射

### 2.1 弹簧

我们的模型：`x'' + 2ζω₀x' + ω₀²x = 0`，`ω₀ = 2π/T₀`，`T₀ = response`，`ζ = damping`。
stiffness/damping/mass 互换：`k = mω₀²`，`c = 2ζ√(km)`；反向 `ω₀=√(k/m)`，`ζ=c/(2√(km))`，`T₀=2π√(m/k)`。m=1 时 `k=(2π/T₀)²`，`c=4πζ/T₀`。

**H1 settled:** `Period = response`，`ζ_c = ζ`（`SpringMapping` scales 1.0）。产品映射已定案；“settled”= chosen and unrefuted，非联合可辨识（见下 Evidence limits）。
<!-- SUPERSEDED 2026-10-20 (Asia/Taipei): S1 mapping changed to Period = response/(2π), ζ_c = ζ per ruling A1 in docs/notion_s1_remap_ab.md (#36 s≈1/(2π); #41 best-τ RMS 0.07–0.20%). Landed in code/composition.h (MapSpringToCompositor), sha256 a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418. R-1 numbers 28.6%/12.1% are void (historical only). -->
- **H2 rejected：** 预测阻尼周期 0.32 s，vs ≈0.511 s observed。
- **H3 rejected：** 预测 ≈3.2 s，vs ≈0.511 s observed。
- H2 作废后不再为 ζ≥1 另定 Period：在 H1 下 (ω₀, ζ) 对任意 ζ 都有定义，**Period 在过阻尼时仍取 response**。

**Evidence limits（record only；do not reopen the mapping）：**
1. 在 ζ=0.78、240 px 行程上，首次过冲只有 **1.99%（4.78 px）**，第二个极值只有 **0.095 px**。所以“峰峰间隔≈0.511 s”不可能是两个相邻峰的实测间隔；与首峰时刻 255.7 ms 的两倍恰好吻合，推测是据首峰时刻推出的。
2. 阻尼周期 `T_d=T₀/√(1−ζ²)` 是**一个方程两个未知数**：(T₀=0.32, ζ=0.78)、(0.25, 0.872)、(0.20, 0.92)、(0.15, 0.956) 都给出 0.511 s。区分它们要靠过冲幅度（4.78 px 对 0.15 px）。
3. **产品残差**以 S1 已发货 **28.6%（A）/ 12.1%（E）** 为准；扫参非产品变体（如 T₀≈0.15）可达 ≈8%/6%，**勿当作产品残差**。ζ 偏 ±0.1 只改变曲线约 1.7–1.9% RMS；T₀ 差 2 倍约 10% RMS。
4. 峰值速度 2029 px/s：**一帧（16.7 ms）的时间错位 = 34 px = 14% 行程**。K1 关键帧 RMS≈0.57% 若同对齐流水线，则 harness 底噪偏低、弹簧残差偏真实 → 结为 **accepted risk R-1 / known-fail**（见 §4 M-1b）；**不得**为过关调 Period/ζ。

**可选诊断（低 ζ 识别协议；非门槛、非重开映射的门）：**
- 取 `ζ_c ∈ {0.2, 0.4, 0.6}`、`Period ∈ {0.25, 0.4}`，各 0→240，量测首峰时刻 `t_p`、首次过冲比 `M_p`、第二峰（若可测）、最终 1 px 内的时刻。
- 解：`ζ_eff = −ln M_p / √(π²+ln²M_p)`，`ω_d = π/t_p`，`ω₀ = ω_d/√(1−ζ_eff²)`，`T₀_eff = 2π/ω₀`。
- 记录 `T₀_eff/Period` 与 `ζ_eff/ζ_c`；**不**据此改产品映射。
- 初速度单位（S3，**已核实 [device]**）：`InitialVelocity` 为 DIP/真实秒，与 `Period` 独立。

**ζ>1（见 §2.4）：** `composition.h` 硬门控已落地；v1 不下发；日后经 `Capabilities::overdampedSpring` + T-S2 抬门。

调用（`SpringAnimation` 已是合成器参数，含 `SpringMapping`）：
```csharp
var a = compositor.CreateSpringScalarAnimation();
a.DampingRatio = (float)s.dampingRatio;
a.Period = TimeSpan.FromSeconds(s.period);
a.InitialValue = (float)s.from;             // 显式：来自影子状态 + 前瞻，见 §3
a.FinalValue = (float)s.target;
a.InitialVelocity = (float)s.fromVelocity;  // DIP/真实秒（S3 [device]），必须显式设；来自引擎非合成器
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
- 控制点 y∉[0,1]：**HONOR [device]** → `bezierOvershoot=true`（派题 #20；取代“默认 false 待 T-K2”）。
- K4 [device]：完成后保持终值。K5 [device]：Stop 冻结；`TryGet` 不可靠 → Stop+`SetValue`。

### 2.3 启动 API 选择（`TryStartAnimation` 与 `StartAnimation`+`TryGetAnimationController`）

**`TryStartAnimation` 不存在**（`CompositionObject` 成员清单里没有），所以不存在“二选一”。试点用法：

| 场景 | 用法 | 理由 |
|---|---|---|
| 弹簧（任何场景） | `StartAnimation(path, spring)` 2 参 | NaturalMotion 的 `TryGetAnimationController` 恒 null [转述]，没有控制器可用；中途改目标/速率一律靠重新播种和 rate 折叠（`period/rate`、`velocity×rate`）完成 |
| KeyFrame，elapsed=0、rate=1 | `StartAnimation(path, anim)` 2 参 | 试点范围内只有这种 |
| KeyFrame，elapsed>0 / rate≠1 / reverse（**不在试点内**，K3） | `compositor.CreateAnimationController()` + **3 参** `StartAnimation(path, anim, controller)`，再设 `Progress`/`PlaybackRate` | 先建控制器再启动，避免“先启动、再 `TryGetAnimationController`”之间已经过了一个提交窗口、动画已开始播放；`TryGet` 仅在需要接管已运行动画时作为后备 |

K3 [device，PARTIAL]：`Progress` 立即生效且时间归一化（匹配 `elapsed/duration`）；`PlaybackRate` 2/−1 可用；**Progress+Delay、|rate|>16、reverse+Progress 未测** → keyframe 能力旗标保持 false。`TryStartAnimation` 不存在；NaturalMotion `TryGetAnimationController` 恒 null [device]。

### 2.4 冲突裁决：ζ>1 是否下发

**Landed as a hard gate in `composition.h`（见 Point 4 / 派题 #14）。** Device：ζ≥0 接受且不钳（1.0/1.2/2.0/5.0 回读≈设定），ζ<0 抛异常，过阻尼动画可跑。证据目前仅 API 接受域；轨迹保真未测。
- 实现：`ToCompositor` 后 `c.damping > 1.0` → Fallback AppThread；ζ=0.8→`CompositorSpring`，ζ=1.2→`AppThread`；`composition_tests` 2073/0。
- **v1 does not lower ζ>1**（不下发过阻尼到合成器）。
- **日后抬门：** 转为 `Capabilities::overdampedSpring`，并过 T-S2（monotone、无过冲 >0.5 px、结算在模型 15% 内、ζ=1.5 vs 3.0 曲线须不同）。轨迹保真未测前保持硬门控。

<!-- R-2 状态：OPEN（seed 来源缺陷，见 docs/notion_r2_reopen_abc.md；#33 可见跳变 ≈207 DIP）。Δpos 判定为单侧规则：仅 Δpos > +0.5 判失败；负值记为「与 R-1 混杂」，不计通过。旧稿中“both ≤0.5”一类字样已修正。本节由 docs/claude_m1b_r2_reply.md §1 合并（2026-10-20 Asia/Taipei）。 M1b 门槛改为 R-2′ 底噪相对门，hybrid B（WGC 末两帧速度）flag 默认开，见 notion_r2_b_gate_ab.md。 -->

## 3. Interrupt / re-seed of a running spring: status OPEN (R-2, seed-source defect; supersedes earlier KNOWN-FAIL)

**Mechanism.** On a retarget of a lowered spring, the dispatcher sets `InitialValue` and `InitialVelocity` explicitly from the shadow state, `spring.h::At(elapsed + seedLead)`. It then replaces the animation with the 2-argument `StartAnimation`. The compositor is never read back, and velocity always comes from the engine.

**Result under the product mapping (#25, single τ).**

| seedLead | \|dpos\| (px) | \|dvel\| | postRMS |
|---|---|---|---|
| 0 ms (default) | 27.612 | 183.0 | 178.91 |
| 16.7 ms | 20.093 | 156.4 | 179.66 |

Section 4 gates (|dpos| ≤ 0.5 px and ≤ 5% of frame travel; |dvel| ≤ max(20, 5%·|v|)) are missed.

**Status (superseded by #33): previously KNOWN-FAIL, caused by R-1. Now OPEN — see `notion_r2_reopen_abc.md`.**
- R-1 is the shape residual between NaturalMotion and `spring.h` under the product mapping (A 28.6%, E 12.1%).
- #31 (below) shows that the interrupt adds no position error beyond what R-1 already has at that instant.
- #25 sampled at tInt+τ (already in B2) and under-reported the step; #33 wall-clock sampling shows a visible jump J_vis ≈ 207 DIP (~86% of travel). The 20–28 px figure is withdrawn.

**Why Section 4 is not relaxed.**
- The harness linear noise floor is about 0.53–0.68% (#30), so the residual is real, not measurement noise.
- Loosening the gate would turn a measured defect into a pass and hide R-1.
- The fix path is R-1 (the mapping), not the gate.
- Acceptance reports list R-2 as `OPEN (seed-source defect)`; primary metrics are J_vis and Δv_vis (wall-clock, τ-free); the #25 numbers above are seed-logic diagnostics only.

**Acceptance metric: single-τ only.** τ is fitted on the pre-interrupt window and frozen after the interrupt. Metrics are |dpos|, |dvel| and postRMS as computed in #25.

**Alignment modes.**
- **Mode S (single τ, frozen):** the only mode allowed in a pass/fail table.
- **Mode R (τ refitted per segment):** `DIAGNOSTIC` only. It absorbs commit latency and part of R-1, so it can make a defective handoff look continuous.

**Relation to #11–#12.**
- Their "passed" status stays, but only as a seed-logic regression: the seed equals `spring.h::At`, with R-1 shape not in play.
- They are not evidence of displayed continuity under the product mapping.
- If either run used a refitted τ, label it Mode R.

**seedLead.**
- The default stays 0 ms; 16.7 ms is an experiment flag.
- Do not tune it from #25 or #31. Its apparent benefit moves the seed toward the displayed curve in a direction set by R-1's sign at that instant, so it fits the residual rather than measuring commit latency.
- Revisit only with δ measured under M-5 after R-1 is resolved.

**Incremental-delta diagnostic (#31).**
- `Δpos = |dpos|_interrupt − |residual|_baseline`, with the baseline taken from a non-interrupted run at the seed's content time.
- Reopen R-2 as an interrupt-specific defect if `Δpos > +0.5 px`. The rule is one-sided and signed.
- Result: Δpos = 0.000 at seedLead 0 and −9.517 px at 16.7 ms. Neither exceeds +0.5 px under the one-sided rule (only Δpos > +0.5 fails); this is not "both ≤ 0.5 px in magnitude".
- Note that −9.517 px is not within ±0.5 px. A negative value is recorded as "seed moved toward the displayed trajectory (confounded with R-1)", never as a pass.
- At seedLead 0, Δpos = 0 holds by construction if the baseline comes from the same fit. State the baseline's provenance in the report.
- Proposed extension, same form: `Δvel` against max(20, 5%·|v|), and `ΔpostRMS` against the non-interrupted run over the same content window.
- #31 covered position only. #33 then computed ΔpostRMS = +15.9 / +17.3 DIP (> 2.4 = 1%·travel, Mode S), which triggered the reopen; R-2 is not R-1-derived only.

**Hybrid B pulled into M1b as a flag mitigation (per reopen verdict).** B omits `InitialValue` (FROM_CURRENT) and takes engine velocity. Default stays A until P1/P2 pass; `interruptible` animations are not lowered. See `notion_r2_reopen_abc.md` and `claude_m1b_r2_reply.md` §2.

**Product handling.** Demos do not use a mid-flight retarget as a headline shot. Release note: "Retargeting a hardware-composited spring mid-flight can show a large visible position jump (about 207 DIP measured in our test scenario). Open issue." (See usage.md Known limitation.)

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
| M-1 | 弹簧形状 RMS（report only） | **仅记录**已发货映射下各场景弹簧形状 RMS（产品残差 A 28.6% / E 12.1%）；低 ζ 识别协议降为可选诊断，**非门槛** |
| M-1b | 形状残差（门槛，**known-fail**） | 门槛保持 ≤1%·行程（§4 **不放宽**）。**known-fail / accepted risk R-1**：已发货 A **28.6%** / E **12.1%**；扫参最佳非产品变体 8% / 6%；**do not tune Period/ζ to pass** |
| M-2 | 时长/收尾 | 截帧得到的运动结束时刻 ≤ 我们 `SpringSettleTime` + 1 帧（不得提前截断）且 ≥ 我们 − max(100 ms, 0.5·T₀)；在结算时刻 `|x−target| ≤ 1.5·SpringPosEps(span)` |
| M-3 | Keyframe 精度 | 5 条曲线（含 3 条 token 曲线）：与 `Timeline::Evaluate` 比，位置≤0.5 px，opacity≤0.005；首末关键帧精确；T-K2：y∉[0,1] 是否过冲，决定 `bezierOvershoot` |
| M-4 | 打断连续性 | **全程只用一个时间偏移 τ**（不得对打断前后分段分别拟合 τ，否则会把 δ 造成的跳变吸收掉）：打断后 10 帧与影子预测误差≤1 px；打断时刻位置残差 `≤ 1 px + 0.25·|v_r|·Δt`；对照软件路径同场景。仅测 A（B not built） |
| M-5 | 前瞻补偿 | 实测 δ（调用时刻 QPC 到首个反映新弹簧的帧）；`seedLead` 取其中位数；补偿使打断残差降低≥50% |
| M-6 | 卸载证明 | 动画中 UI 线程 `Sleep(200)`：期间截帧位置与影子预测误差≤1 px；软件路径对照应冻结 |
| M-7 | 释放 | 1000 次 start/replace/stop：私有内存增长≤1 MB、内核句柄 ±5、GDI/USER 不变；Unloaded/Closed 后订阅数==退订数；`fm_destroy` 后回调不再增加；全部完成后 `LiveAnimationCount()==0` |
| M-8 | Reduced motion | 切换后合成器动画数为 0；终值精确 |
| M-9 | 初速度单位 | 前 3 帧斜率与设置值之比在 1±5% |
| M-10 | ζ>1 | T-S2（§2.4） |

## 5. 阶段与退出条件

1. **M1b-0（只测，不接引擎，约 3 天）**：独立 C# 小程序直接调合成器 API。先做 G0 与 E0/E0-fast（§7.3），记录 M-1（report-only RMS），跑 T-S2（ζ>1 轨迹，抬门前）、确认 K1–K5 / S3–S5 真机事实、K3 余项、M-9，并按 §7.4 清单确认熄屏伪影。退出：G0、E0 通过；N 已测；**H1 映射不重开**。
2. **M1b-1**：`fm_create_composition` + `WinRtBackend` + `opacity` keyframes（`bezierOvershoot=true`）；退出：M-3、M-7（keyframes 部分）、M-8。
3. **M1b-2**：`offsetX` spring + 方案 A retarget（`Seed = At(elapsed+seedLead)`）；退出：M-1（记录）、M-2、M-4（相对显示轨迹）、M-5。M-1b 预期 known-fail（R-1）。
4. **M1b-3**：M-6、完整 M-7、M-10（仅当准备抬 `overdampedSpring`）；写入 `api.md`/`usage.md`。

## 6. 风险与待办

- **R-1 accepted risk / known-fail：** 产品/S1 形状残差 **28.6%（A）/ 12.1%（E）**；扫参非产品变体最低 8%/6%。§4 ≤1% 门槛**不放宽**；**不**调 Period/ζ 以求过关。K1≈0.57% 暗示 harness 底噪偏低。打断连续性已在 #25 相对显示轨迹测量 → **R-2 known-fail（R-1 衍生）**（Notion A 裁决；英文定稿待 Claude；见 `notion_r2_review_abc.md`）。
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
| B4 | **显示休眠/DWM 暂停，合成器动画时钟不按真实时间推进** | 亮屏重跑；先做 G0 | **confirmed more strongly [device]**：亮屏下 `Completed` **偏晚而非偏早**，故 14–30 ms 早完成是熄屏伪影；**B1–B3 unsupported** |
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

## 对 DeepSeek spec 的评审

# DeepSeek M1b review — FINAL

I made no file edits. The deltas below are for you to apply.

## Point 1 — Period / DampingRatio mapping: Agree (settle H1), with three corrections to the Notion draft

**Verdict.** `Period = response`, `ζ_c = ζ` is settled as the product mapping. H2 predicts a 0.32 s damped period and H3 predicts about 3.2 s, against about 0.511 s observed as relayed, so both are rejected. For ζ≥1, `Period` stays at `response`, with no special case.

**Corrections:**
1. **What "settled" means here.**
   - The observed 0.511 s is one number. `T_d = T₀/√(1−ζ²)` has two unknowns, so (T₀=0.25, ζ=0.872) gives the same value.
   - At ζ=0.78 the second extremum is about 0.095 px, so it cannot have been a measured peak-to-peak interval.
   - So "settled" means chosen and unrefuted, not jointly identified. The Notion draft dropped this caveat from my earlier check.
2. **Quote the shipped residual, not the best-of-sweep one.**
   - Under the shipped mapping the spring-shape RMS is 28.6% (scenario A) and 12.1% (scenario E).
   - The 8% and 6% figures belong to non-product variants such as T₀=0.1512. Please do not present them as the product residual.
3. **My earlier objection to "inherent difference" is now mostly resolved.**
   - K1 reports 0.57% RMS on a keyframe shape. If that used the same alignment pipeline at comparable speed, the harness floor is low and the spring residual is real.
   - So closing it as an accepted risk is acceptable. The ≤1% gate stays unrelaxed and is reported as known-fail. Do not tune `Period` or ζ to reach it.

## Point 2 — Interrupt seeding: Agree (explicit seed), but the Notion rationale is partly wrong

**Verdict.** Always set `InitialValue` and `InitialVelocity` explicitly from `spring.h::At(elapsed + seedLead)` on the shadow state. Demote only hybrid B.

**Corrections to Notion:**
- The engine velocity does not come from the compositor, so `TryGetScalar` being static is irrelevant. It comes from the C++ engine.
- The real reasons to demote B are that it relies on documentation-silent null-`InitialValue` semantics, and that its failure mode is a large jump to a stale start. The explicit seed's failure mode is only a small δ·v error.
- A (explicit seed plus lookahead) is not demoted. It is the plan.
- The `TryGetScalar` finding is workbuddy's observation, not mine.

**Consequence of Point 1.** The shadow-versus-compositor residual (12–28% RMS) is the error budget of explicit seeding, so interrupt continuity must be measured against the displayed trajectory (see Open).

## Point 3 — `TryStartAnimation` vs `StartAnimation`+`TryGet`: Agree

**Verdict.** `TryStartAnimation` does not exist. The `CompositionObject` member list has `StartAnimation` (2- and 3-argument), the group variants, `StopAnimation` and `TryGetAnimationController` only. For springs, `TryGetAnimationController` is always null (workbuddy), so springs use plain `StartAnimation` and re-seed.

For KeyFrame, prefer the controller-first pattern: `CreateAnimationController()` plus the 3-argument `StartAnimation`. Use `TryGet` after `Start` only as a fallback.

K3 is only partly confirmed:
- `Progress` is time-normalized, which matches `elapsed/duration` in our time model.
- `PlaybackRate` 2 and −1 work.
- The keyframe capability flags stay false until the rest is verified (see Open).

## Point 4 — ζ>1: Agree (v1 does not lower)

**Verdict.** The API accepts ζ≥0 without clamping (1.0/1.2/2.0/5.0 read back ≈ set), ζ<0 throws, and overdamped animations run.

**Caveats:**
- The evidence is API-level acceptance only. Trajectory fidelity is unmeasured.
- The gate is landed. I have not seen the diff, so I am relying on your description: `c.damping > 1.0` after `ToCompositor`, with ζ=0.8→`CompositorSpring`, ζ=1.2→`AppThread` and composition_tests 2073/0. Checking after the mapping is right.
- To lift the gate later, convert it to `Capabilities::overdampedSpring` and pass the trajectory checks below.

## `claude_m1b_spec.md` edits

- **§2.1:**
  - Heading "工作映射…" → "**H1 settled:** `Period = response`, `ζ_c = ζ` (`SpringMapping` scales 1.0)."
  - H2 and H3 rows → "rejected: predict 0.32 s and ≈3.2 s vs ≈0.511 s observed."
  - "但这不是定案" paragraph → "Evidence limits (record only; do not reopen the mapping)." Keep the four numbers.
  - The low-ζ identification protocol becomes an optional diagnostic, not a gate.
- **§1 table and §4 M-1/M-1b:**
  - M-1 becomes "report only: record spring-shape RMS under the shipped mapping per scenario."
  - M-1b keeps the gate (≤1%·travel), with the note "known-fail, accepted risk R-1: A 28.6% / E 12.1% shipped; best swept variants 8% / 6%; do not tune to pass."
- **§3:**
  - "B 降为实验开关" → "B not built in M1b; documented alternative only; revisit if A's interrupt residual against the displayed trajectory fails."
  - Add: "Seed = `spring.h::At(elapsed + seedLead)` for both `InitialValue` and `InitialVelocity`. Velocity comes from the engine, never the compositor."
- **§2.3 and §1:**
  - Add: "`TryStartAnimation` does not exist; NaturalMotion `TryGetAnimationController` is always null [device]."
  - K3 row: "Progress immediate and time-normalized; PlaybackRate 2/−1 work; Progress+Delay and |rate|>16 untested; keyframe flags stay false."
- **§2.4:**
  - Replace the capability-flag paragraph with: "Landed as a hard gate in `composition.h` (see Point 4). Device: ζ≥0 accepted and not clamped, ζ<0 throws, overdamped animation runs. v1 does not lower ζ>1. To lift it: convert to `overdampedSpring` and pass T-S2 (monotone, no overshoot >0.5 px, settle within 15% of model, ζ=1.5 vs 3.0 curves differ)."
- **§1 facts, §2.2 and §0:**
  - S3 → "`InitialVelocity` is DIP per real second, independent of `Period` [device]."
  - Add rows: S4 pass; S5 holds the final value, a mid-flight Stop freezes, and `Completed` is late; K1 RMS ≈0.57%; K4 final value held; K5 "Stop freezes, `TryGet` unreliable, use Stop+`SetValue`."
  - K2: y∉[0,1] honored, so `bezierOvershoot=true` replaces "default false until T-K2 passes".
- **§7.1:**
  - B4 becomes "confirmed more strongly: on a lit screen `Completed` is late, not early, so the 14–30 ms early completion was a sleeping-display artifact; B1–B3 unsupported."

## Open / needs measurement

1. Interrupt continuity against the displayed trajectory, using one time offset for the whole run under the shipped mapping.
   - Includes δ and the `seedLead` value.
   - The interrupt numbers relayed earlier were not in the material I was given.
2. ζ>1 trajectory fidelity (monotonic, settle time, ζ=1.5 vs 3.0).
3. Spring rate-fold shape: the 13.6% spot RMS cannot be separated from the base residual. It needs a rate=1 control in the same run. v1 scenarios should keep rate=1 until then.
4. K3 remainder: `Progress` combined with `DelayTime`, |rate|>16, and reverse with `Progress`.
5. Harness noise floor at spring speeds (the 0.25 s linear calibration). K1's 0.57% is only a proxy at an unknown speed.
6. Confirm the landed gate boundary (`> 1.0` strict) against the diff.

## Summary

H1 (`Period=response`, `ζ_c=ζ`) is settled and H2/H3 are rejected; explicit shadow seeding replaces hybrid B; no `TryStartAnimation` exists and springs have no controller; ζ>1 is not lowered in v1 (the gate is landed). The shipped spring-shape residual is 28.6%/12.1% (not 8%/6%), and it is the error budget for interrupt continuity, which is still unmeasured against the displayed trajectory.
