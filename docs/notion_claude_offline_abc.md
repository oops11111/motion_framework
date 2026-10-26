> ⚠️ **R-2 已因 #33 重开**（OPEN：seed 来源缺陷，可见跳变约 207 DIP）。本文中 R-2「known-fail / 纯 R-1 衍生 / 20–28 px」等结论已作废，以 [notion_r2_reopen_abc.md](./notion_r2_reopen_abc.md) 为准。

# Claude 空窗期接手 · A/B/C 中文交付（Notion AI）

> 来源：Notion AI · Opus 5.5 · 2026-10-20（Asia/Taipei，约 15:00）  
> 输入提示：`docs/_notion_prompt_claude_offline_full.txt`  
> 依据：`docs/claude_m1b_deepseek_review.md`（FINAL）  
> 说明：正文基于 Notion AI「拷贝回复」；已还原假链接文件名。文末 Notion 自注已略去。

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

### 相对旧 Notion 草稿（notion_deepseek_review.md）的修正

- **Point 1**：旧稿把扫参最优的 8%/6% 写得像产品残差。现在改为产品残差 **28.6%/12.1%**，8%/6% 只作对照；并补上「定案 ≠ 联合辨识唯一解」的限定。
- **Point 2（重点）**：旧稿用「`TryGetScalar` 恒为静态」来论证「没有引擎速度」，这个推论是错的。**引擎速度来自 C++ 引擎，不来自合成器**，`TryGetScalar` 为静态与此无关（该发现来自 workbuddy）。另外只降级 B，A 保留。
- **Point 4**：旧稿建议修改 composition.h 门控，这已经落地。现在改为记录门控现状和日后放开的条件。

## Open（复核结果）

- [x] ζ>1 过阻尼轨迹（#23）：PASS，门槛不抬；settle 远小于 spring.h，属于固有量级差异。
- [x] rate 折叠（#24）：误差同量级，默认 rate=1 不变。
- [x] 参数门控（#26）：Period>0 生效，Zero 抛异常；composition.h 作者参数门控 2076/0。
- [x] 初值语义（#27/#29）：IV=null 等同 FROM_CURRENT；StartingValue 等同 FROM_STARTING_VALUE。产品仍显式 seed。
- [x] K3 时序（#28）：Delay 期间 Progress 走 SEEK；|rate|>16 CLAMP 到 ±16；Progress+反向 OK。
- [x] 打断 seed（#25）：未达 §4，见下方 R-2。
- 补充：#30 harness 线性底噪 0.68%/0.53%，测量下限远低于被测残差。

## R-2 打断连续性（known-fail，R-1 衍生）

真机 #25（artifacts/win_interrupt_seed_probe.txt）：产品映射、显式 seed、全程单一 τ（PRE 拟合，post 冻结）。

| seedLead | |Δpos| px | |Δvel| | postRMS |
|---|---|---|---|
| 0 ms    | 27.612 | 183.0 | 178.91 |
| 16.7 ms | 20.093 | 156.4 | 179.66 |

§4 门槛：|Δpos|≤0.5px 且 ≤5% 帧位移；|Δvel|≤max(20, 5%|v|)。两组均未达。

裁决：
- 记为 known-fail，§4 门槛不改。理由：seedLead 只改善打断瞬间的单点指标，postRMS 不变，说明误差来自 R-1 形状残差（12–28%·travel），不是 seed 逻辑。
- 不允许打断后重拟合 τ 作为验收（会吞掉 δ）；#11–#12 的 seed 逻辑结论保留。
- 新增诊断指标（不是验收）：增量 Δ = 打断时刻 Δ − 同一时刻不打断的基线残差。增量 Δ >0.5px 时重开 R-2。**#31 已测 Δ≤0.5（实为 ≤0 / 负）→ 不重开。**
- **#31 确认纯 R-1 衍生**：增量Δ(seedLead=0)=0.000；Δ(16.7)=−9.517；artifact `artifacts/win_r2_delta_offline.txt`；无真机重跑；composition.h 未改。
- seedLead 默认 0 ms；两种对齐方式写入文档。
- hybrid B：触发条件已满足，登记为 M1b 冻结后的纸面评估。

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
| S2 | **已核实（派题 #14）**：ζ≥0 接受且不钳；ζ<0 抛 ArgumentException；过阻尼可跑。产品仍 v1 不 lower（见 §2.4）。协调 bot 补注：FINAL 设备表未单列 S2，但 Point 4 / #14 已有结论，故不标待补。 |
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
    - S1 H1 已定案；S2 已核实（#14：ζ≥0 接受不钳、ζ<0 抛）；S3：InitialVelocity 单位为 DIP/真实秒；S4 通过；S5 保持终值，飞行中 Stop 冻结，Completed 偏晚。
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
