## A. FINAL 中文摘要

### 结论：四点均已 Agree，不再重开

**Point 1：Period / DampingRatio（H1 已定案）**

- 产品映射为 `Period = response`，`ζ_c = ζ`（SpringMapping 缩放 1.0）。ζ≥1 时 Period 仍取 response，没有特例。
- H2 预测阻尼周期约 0.32 s，H3 预测约 3.2 s，实测约 0.511 s，两者都被拒绝。
- 这里的「定案」是指已选定且没有被推翻，不等于联合辨识出了唯一解。例如 (T₀=0.25, ζ=0.872) 也能得到同样的 0.511 s。ζ=0.78 时第二个极值只有约 0.095 px，没法据此可靠地测峰峰间隔。
- **产品映射下的形状残差**：场景 A **28.6%** RMS，场景 E **12.1%** RMS。扫参最优约 8%/6%（如 T₀=0.1512 等非产品变体）**只作对照**，不是产品残差。
- ≤1%·行程的门槛**不放宽**，记为 known-fail / 已接受风险 R-1。不为达标去调 Period 或 ζ。
- K1 关键帧形状 RMS≈0.57%。如果对齐管线和速度可比，说明 harness 底噪低，弹簧残差是真实存在的。

**Point 2：打断种子**

- 每次都显式设置 `InitialValue` 和 `InitialVelocity`，两者都取自影子状态的 `spring.h::At(elapsed + seedLead)`。
- **前瞻方案 A**（显式种子 + lookahead）保留为计划，不属于降级项。
- **只降级 hybrid B**（不设 InitialValue，InitialVelocity 取「引擎速度」）。M1b 不实现 B，只在文档中作为备选；只有当 A 在显示轨迹上的打断残差不达标时，才重新评估。
- 降级 B 的真实理由：B 依赖文档没有写明的 null-InitialValue 语义，一旦失败会大幅跳回陈旧起点。显式种子即使失败，也只是小的 δ·v 误差。
- 与 Point 1 的关系：影子状态相对合成器有 12–28% RMS 的残差，这就是显式种子的误差预算。所以打断连续性必须对照**显示轨迹**来测。

**Point 3：TryStartAnimation / 控制器**

- 不存在 `TryStartAnimation`。CompositionObject 的成员只有 StartAnimation（2 参和 3 参）、组变体、StopAnimation 和 TryGetAnimationController。
- 弹簧：`TryGetAnimationController` 恒为 null（设备实测），所以弹簧用普通 StartAnimation 并重新播种。
- 关键帧：优先用 `CreateAnimationController()` + 三参 `StartAnimation`。Start 之后再调用 TryGet 只作后备。
- K3 部分确认：
    - Progress 按时间归一化（与 elapsed/duration 一致）。
    - PlaybackRate 2 和 −1 可用。
    - Progress+Delay、|rate|>16、带 Progress 的反向播放还没测。
    - 关键帧 capability 标志暂时保持 false。

**Point 4：ζ>1**

- API 接受 ζ≥0 且不钳制（设 1.0/1.2/2.0/5.0，回读约等于设定值）。ζ<0 会抛异常。过阻尼动画能跑。
- 证据只到 API 层面接受，轨迹保真度还没测。
- **composition.h 硬门控已落地**：`ToCompositor` 之后，`c.damping > 1.0` 的情况不 lower。ζ=0.8 走 CompositorSpring，ζ=1.2 走 AppThread；composition_tests 结果 2073/0。v1 对 ζ>1 一律不 lower。

### 相对旧 Notion 草稿（notion_deepseek_[review.md](http://review.md)）的修正

- **Point 1**：旧稿把扫参最优的 8%/6% 写得像产品残差。现在改为产品残差 **28.6%/12.1%**，8%/6% 只作对照；并补上「定案 ≠ 联合辨识唯一解」的限定。
- **Point 2（重点）**：旧稿用「`TryGetScalar` 恒为静态」来论证「没有引擎速度」，这个推论是错的。**引擎速度来自 C++ 引擎，不来自合成器**，`TryGetScalar` 为静态与此无关（该发现来自 workbuddy）。另外只降级 B，A 保留。
- **Point 4**：旧稿建议修改 composition.h 门控，这已经落地。现在改为记录门控现状和日后放开的条件。

### Open / 需要实测（六条）

1. 对照显示轨迹测打断连续性：整次运行用同一时间偏置，包含 δ 和 seedLead。此前的材料里没有给出打断相关的数字。
2. ζ>1 的轨迹保真度：是否单调、settle 时间、ζ=1.5 与 3.0 的曲线是否有区别。
3. 弹簧 rate-fold 的形状：13.6% 的抽测 RMS 无法和底残差分开，需要同跑一组 rate=1 对照。在此之前，v1 场景保持 rate=1。
4. K3 剩余项：Progress+DelayTime、|rate|>16、带 Progress 的反向播放。
5. 弹簧速度下的 harness 底噪（用 0.25 s 线性动画标定）。K1 的 0.57% 只是速度未知时的代理指标。
6. 对照 diff 确认门控边界（严格 `> 1.0`）。

---

## B. claude_m1b_spec 修订 4：可粘贴段落

### §2.1 Period / DampingRatio 映射

**H1 已定案：** Period = response，ζ_c = ζ

产品映射：`Period = response`，`DampingRatio = ζ`（SpringMapping 缩放 1.0）。ζ≥1 时 Period 仍为 response，没有特例。

- **H2（已拒绝）**：预测阻尼周期约 0.32 s；实测约 0.511 s。
- **H3（已拒绝）**：预测阻尼周期约 3.2 s；实测约 0.511 s。

**证据限制（映射不因此重开）**：「已定案」是指已选定且没有被推翻，不是联合辨识的唯一解。

- (T₀=0.25, ζ=0.872) 同样给出 0.511 s 的阻尼周期。
- ζ=0.78 时第二个极值约 0.095 px，没法据此可靠地测峰峰间隔。
- 产品映射下的形状残差：场景 A 28.6% RMS，场景 E 12.1% RMS。
- 扫参最优约 8%/6%（如 T₀=0.1512 等非产品变体），只作对照，不是产品残差。
- K1 关键帧形状 RMS≈0.57%。如果对齐管线和速度可比，说明 harness 底噪低，弹簧残差是真实存在的。

**低 ζ 辨识**：改为可选诊断，**不作为门禁**。不为降低残差去调整 Period 或 ζ。

### §3 打断与重播种

**方案 A（计划实现）**：显式种子 + 前瞻。每次打断都显式设置 `InitialValue` 和 `InitialVelocity`，两者都取自影子状态：

```
Seed = spring.h::At(elapsed + seedLead)
InitialValue    ← Seed.value
InitialVelocity ← Seed.velocity
```

**方案 B（hybrid）：M1b 不实现；仅文档备选。** B 的做法是不设 InitialValue，InitialVelocity 取引擎速度。

- **降级理由**：B 依赖文档没有写明的 null-InitialValue 语义，失败时会大幅跳回陈旧起点。方案 A 失败时只有小的 δ·v 误差。
- **重评条件**：只有当方案 A 在显示轨迹上的打断残差不达标时，才重新评估 B。

**速度来源**：速度来自 C++ 引擎（影子状态 `spring.h`），**永远不来自合成器**。`TryGetScalar` 恒为静态与此无关。

**误差预算**：影子状态相对合成器有 12–28% RMS 的残差（见 §2.1），这就是显式种子的误差预算。所以打断连续性必须对照**显示轨迹**来测，整次运行用同一时间偏置，并记录 δ 和 seedLead。

### §2.4 ζ>1（过阻尼）

**API 层面**：ζ≥0 被接受且不钳制（设 1.0/1.2/2.0/5.0，回读约等于设定值）；ζ<0 抛异常；过阻尼动画能跑。这只是 API 层面的证据，轨迹保真度还没测。

**硬门控已落地（composition.h）**：`ToCompositor` 之后，`c.damping > 1.0` 的情况不 lower。

- ζ=0.8 → CompositorSpring
- ζ=1.2 → AppThread
- composition_tests：2073/0

v1 对 ζ>1 **不 lower**。边界是严格的 `> 1.0`，需要对照 diff 确认。

**日后放开的条件**：把门控改为 `Capabilities::overdampedSpring`，并且必须通过以下轨迹检查：

- 轨迹单调；
- 过冲 ≤0.5 px；
- 在模型预测的 15% 以内 settle；
- ζ=1.5 和 ζ=3.0 的曲线有可分辨的差别。

### M-1 / M-1b

| ID | 修订后内容 |
| --- | --- |
| M-1 | **改为仅报告**：按场景记录产品映射下的弹簧形状 RMS，不作为门禁。 |
| M-1b | 门槛保留 ≤1%·travel，**注明 known-fail，已接受风险 R-1**。产品映射：场景 A 28.6% / 场景 E 12.1%；扫参最优 8%/6%（仅作对照）；不为通过而调参。 |

### 设备事实（S2–S5 / K1–K5 / B4）

| ID | 结论 |
| --- | --- |
| S2 | 本次材料没有给出结论（待补，不填数字）。 |
| S3 | `InitialVelocity` 的单位是 DIP/真实秒，与 Period 无关 [device] |
| S4 | 通过 |
| S5 | 保持终值；飞行中 Stop 会冻结；Completed 事件偏晚 |
| K1 | 形状 RMS ≈0.57% |
| K2 | y∉[0,1] 的控制点被尊重，所以 `bezierOvershoot=true`（取代「默认 false 直到 T-K2」） |
| K3 | Progress 立即生效且按时间归一化；PlaybackRate 2/−1 可用；Progress+Delay 和 |rate|>16 未测；关键帧 flags 暂为 false |
| K4 | 保持终值 |
| K5 | Stop 会冻结；TryGet 不可靠，改用 Stop+SetValue |
| B4 | 亮屏时 Completed 偏晚，而不是偏早；14–30 ms 的提前完成是熄屏伪影；B1–B3 无支持证据 |

---

## C. notion_brief 增量

- **M1b 门控现状**：
    - S3：InitialVelocity 单位为 DIP/真实秒；S4 通过；S5 保持终值，飞行中 Stop 冻结，Completed 偏晚。S1、S2 本次材料没有给出结论。
    - K1：RMS≈0.57%；K2：`bezierOvershoot=true`；K3 部分确认（关键帧 flags 暂为 false）；K4：保持终值；K5：用 Stop+SetValue 代替 TryGet。
    - composition.h 硬门控已落地：ζ>1 不 lower，composition_tests 2073/0。
- **固有差异 known-fail（R-1）**：
    - 产品映射（Period = response，ζ_c = ζ）下的弹簧形状残差为 A 28.6% / E 12.1% RMS。
    - ≤1%·travel 门槛不放宽，也不为达标调参。
    - 扫参最优 8%/6% 只作对照。
- **打断种子**：显式设置 InitialValue 和 InitialVelocity，均取自 `spring.h::At(elapsed+seedLead)`。速度来自 C++ 引擎，不来自合成器（修正旧稿）。hybrid B 只保留为文档备选。
- **下一步优先真机项**：
    - ① 对照显示轨迹测打断连续性（同一时间偏置，含 δ 和 seedLead）；
    - ② ζ>1 轨迹保真度（单调、settle 时间、ζ=1.5 与 3.0 对比）；
    - ③ 弹簧 rate-fold 跑 rate=1 对照，在此之前 v1 保持 rate=1；
    - ④ K3 剩余项（Progress+DelayTime、|rate|>16、带 Progress 的反向播放）。
- **其余待测**：在弹簧速度下标定 harness 底噪（0.25 s 线性动画）；对照 diff 确认门控边界（严格 `> 1.0`）。

---

注：1. 我没法在对话里切换模型，本次回复直接由当前可用的模型完成。2. 你要求写 S2–S5，但材料里没有 S2 和 S1 的结论，所以 B 里的 S2 写成「待补」、C 里注明了这一点，没有编数字。3. 材料没写清 §2.1 要保留的「四组数字」具体是哪四组，B 里把材料给出的相关数字都列进去了。