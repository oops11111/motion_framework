数字出来了，结果对 DeepSeek 的"定案"有影响，我稍后归纳。先核对 `TryStartAnimation` 是否存在，以及 `TryGetAnimationController` 的文档说法。

核对完毕：`CompositionObject` 的方法清单里没有 `TryStartAnimation`。现在把我自己的 spec 按以上结论更新成完整版。

我读完了 DeepSeek 的 spec，并对照了 `composition.h`、用闭式解算了它的关键数字、核对了 `CompositionObject` 的成员清单。DeepSeek 引用的 workbuddy 数据我都没看到原始日志，下面凡是出自 workbuddy 的都标"转述"。

## 1. Period / DampingRatio 映射

**两边对比：** DeepSeek 把 `Period=response`、`DampingRatio=ζ` 写成"定案"，依据是亮屏复测的轨迹间隔约 0.511 s。我的修订 2 把它列为 H1，同时列了 H2（Period 是阻尼周期）和 H3（Period=1/ω₀）两个备选。

**谁对：**
- 工作映射上 DeepSeek 对。0.511 s 恰好等于 `0.32/√(1−0.78²)`；H2 预测 0.32 s，H3 预测约 3.2 s，两者都被否定。我拿"推荐值 50 ms"推出 H3，是我的错。
- ζ>1 时 Period 不需要特殊处理。H2 作废后，(ω₀, ζ) 对任何 ζ 都有定义，Period 仍取 response。
- "定案"说过头了。我算了：
  - 在 ζ=0.78、240 px 行程上，首次过冲只有 4.78 px（1.99%），第二个极值只有 0.095 px。所以"峰峰间隔"不可能是两个相邻峰的实测间隔。它与首峰时刻 255.7 ms 的两倍吻合，应该是推出来的，需要原始数据确认。
  - 阻尼周期是一个方程两个未知数：(T₀=0.25, ζ=0.872)、(0.20, 0.92) 给出同样的 0.511 s，要靠过冲幅度才能区分。
  - ζ 偏 ±0.1 只让曲线变化约 1.7–1.9% RMS，所以 ζ=0.78 附近几乎辨不出 ζ，要用低 ζ（0.2–0.6）来识别。
  - 峰值速度约 2029 px/s，一帧（16.7 ms）的时间错位就是 34 px，占行程 14%。

**形状残差 28.6%/12.1% 不能"结案"：** DeepSeek 一边把它记为"固有差异已结案"，一边说 1% 门槛不放宽，两者矛盾。这个残差也可能主要是测量对齐误差。要先用 0.25 s 的线性动画走同样的流水线测出测量本身的噪声下限，再判断弹簧是否真有差异。

## 2. InitialValue / InitialVelocity 的中途 retarget

**两边对比：** 共识是不能回读、`InitialVelocity` 必须显式设、弹簧没有控制器。分歧在 `InitialValue`：DeepSeek 总是显式设，种子来自影子状态；我原来推荐不设 `InitialValue`（缺省取当前值）。

**谁对：** DeepSeek 的做法应当作默认，我退一步。理由是失败代价不对称：如果文档没写清的"缺省值"其实是最后一次静态设置的值，我的方案会让画面跳回旧起点，是大跳变；显式种子的误差只是 `速度×δ` 量级。亮屏复测里，显式种子也已通过打断位置和速度判据（转述）。

**两边都漏了一点：** 种子要按提交时刻外推，而不是调用时刻。`composition.h` 的 `PlanOverride` 现在取 `s.clockTime`（调用时刻）。显示的是提交时刻，差 δ。用同一条弹簧算：8.3 ms 的 δ 就有约 17 px 的位置误差。所以 spec 里加了 `seedLead`，初值 1 帧，由实测校准。

另外 DeepSeek 比较验收时可能把这个误差吸收掉了。它对打断前后分别对齐时间，会把 δ 造成的跳变抵消。我把 M-4 改成全程只用一个时间偏移。

**DeepSeek 的关键帧首帧写法我不采纳：** 它建议用 `this.StartingValue`。我保留显式写初值，因为我们的时间线是绝对值，`Evaluate(0)` 就是初值；`StartingValue` 取合成器当前值，两者不等时画面会与引擎模型分叉。

## 3. `TryStartAnimation` 与 `StartAnimation`+`TryGetAnimationController`

**`TryStartAnimation` 不存在。** `CompositionObject` 的完整成员清单是两个 `StartAnimation` 重载（含带 `AnimationController` 的 3 参）、`StartAnimationGroup`、`StopAnimation`、`StopAnimationGroup`、`TryGetAnimationController`。DeepSeek 对，所以没有"二选一"。

试点怎么用（已写入 spec §2.3）：
- **弹簧：** 一律用 2 参 `StartAnimation`。`TryGetAnimationController` 对弹簧按设计恒为 null（转述），中途改目标和 rate 靠重新播种、`period/rate`、`velocity×rate` 完成。
- **关键帧（elapsed=0、rate=1）：** 2 参 `StartAnimation`。
- **关键帧（elapsed>0、rate≠1、reverse，不在试点范围）：** 先 `CreateAnimationController()`，再用 3 参 `StartAnimation`。不要先启动再取控制器，因为取到时动画已经过了一个提交窗口、已开始播放。`TryGetAnimationController` 只作接管已运行动画的后备。

## 4. ζ>1 是否下发

**事实：**
- `composition.h` 不看 ζ，凡是弹簧就下发。
- DeepSeek"v1 暂不 lower ζ>1"在你给我的文件里只是一句括号和执行 bot 的审校注，没有正文论证；原始回复不在附件里。
- 文档把 ζ>1 明确列为过阻尼，所以纸面上支持。

**我的建议：试点默认不下发（以 DeepSeek 为准），但用能力开关实现，不改规划器的长期行为。**
- 新增 `Capabilities::overdampedSpring`，默认 false。规划器在 `ζ > 1+1e-6` 且开关为 false 时回退到应用线程，reason 写明。ζ=1（临界，`gentle` token 在用）不受影响。
- 理由和 `bezierOvershoot` 一致：未验证的区域默认关。回退到应用线程只是花主线程帧，显示结果与引擎一致；错误下发则画面和引擎分叉，且无法发现。
- 开关翻转条件：ζ=1.0、1.5、3.0 各测一次，无过冲、结算时刻与模型差不超过 15%、ζ=1.5 与 3.0 的曲线不得相同（相同说明被截断）。

## 汇总

| 议题 | 结论 | 谁对 |
|---|---|---|
| 1. Period/ζ 映射 | 工作映射 `Period=response`、`ζ_c=ζ`；H2/H3 作废；ζ>1 的 Period 不特殊处理。但不是定案：28.6%/12.1% 的形状残差未解释，先测噪声下限并用低 ζ 识别 | 映射本身 DeepSeek 对；"定案/结案"我不同意；我的 H3 是错的 |
| 2. retarget | 默认显式种子（影子状态），加 `seedLead` 前瞻；不设 `InitialValue` 的方案降为实验开关；关键帧首帧显式写初值 | 种子方案 DeepSeek 对，我让步；前瞻是两边都漏的，我加；首帧写法我对 |
| 3. 启动 API | `TryStartAnimation` 不存在。试点全用 2 参 `StartAnimation`；关键帧中途定位用 `CreateAnimationController` + 3 参重载；弹簧无控制器，靠重新播种 | DeepSeek 对 |
| 4. ζ>1 | 试点默认不下发，用 `overdampedSpring` 开关实现，通过 T-S2 后翻转；规划器的长期行为保留 | DeepSeek 的策略对，`composition.h` 的长期行为对；落地方式是开关 |

`claude_m1b_spec.md` 已更新为完整的第 3 版。除上面的结论外，我还采纳了 DeepSeek/workbuddy 的几项测量修正：
- 熄屏时 WGC 仍约 46 fps 到帧但画面冻结，所以新增位移门槛，位移不足的运行判 `INVALID`，不是 fail。
- `cRefresh` 不再参与门禁。
- WinUI 3 下用 `CreateForWindow` 加裁切，`CreateFromVisual` 不可用。
- 160–200 ms 的呈现滞后要做时间对齐。

这些我无法独立核实，都标了"转述"。我建议把 workbuddy 的原始逐帧 `(t,x)` CSV 放进仓库，才能复算首峰时刻、过冲比和结算时刻。