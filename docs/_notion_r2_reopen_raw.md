## A 重新裁决 R-2

**裁决：重开 R-2。** 不再用「known-fail（R-1 衍生）」收口，改为 **OPEN：seed 来源缺陷，严重度高**。R-1 仍是根因之一，但它不足以概括问题的规模和后果。

**依据**

1. **核心问题是 seed 来源。** 打断时，合成器在屏幕上的实际位置约 14.5，spring.h 影子已到 222。A 用影子 seed，新动画首帧被拽到影子位置，墙钟同时刻可见跳变约 207 DIP，约占 86% 行程。#31 算出 Δpos≈0，只说明这一跳变等于不打断基线在该时刻的 R-1 点残差，因果上确实「衍生」。但上轮 known-fail 是按 20–28 px 量级定的。#25 在 tInt+τ 取样，那时已是 B2，所以严重低估。量级被证伪，结论随之作废。
2. **单侧判据已触发。** 按 Mode S 算，ΔpostRMS = +15.9/+17.3 DIP，即 +6.6%/+7.2% 行程，大于 1%·travel = 2.4。这些超出部分来自 B2 继承影子速度：以约 642 DIP/s 的初速冲到约 269，再慢慢回落；spring.h 的 B2 在 200 ms 内就回到 0。这一项 R-1 解释不了，也回答了 Claude 的疑问：postRMS≈75% 远高于 28.6%，是因为 B2 的形状和速度继承叠加在 R-1 之上。
3. ΔΔpos≈−155、ΔΔvel≈−191/−201 是负值，按单侧规则不算失败。记录为「与 R-1 混杂」，不计为通过。

**方案评估**

| 方案 | 位置跳变 | 速度 / postRMS | 架构代价 | 结论 |
| --- | --- | --- | --- | --- |
| (a) 维持 A + known-fail | 约 207 DIP | 不变 | 无 | **否决**：近满行程的跳变是产品缺陷，不能当限制项发布 |
| (b) hybrid B 提前（IV=null，即 FROM_CURRENT） | 若 null 取的是动画中的当前值，则构造上≈0 | 速度仍是影子速度，B2 上冲可能更明显（从 14.5 带着 +v 起步）；|dvel| 不能闭合 | 影子与屏幕偏移约 207，引擎看不到；后续 Hold 或回落 AppThread 会再跳 | **M1b 采纳为 flag 缓解**，前提是 P1 通过 |
| (c) 影子追踪合成器实际轨迹（合成器时钟 + 实测形状模型） | 根治 | 根治，后续交接也一致 | 需要 R-1 形状模型（preRMS≈59，目前单 τ 都拟合不好）+ 合成器时基 | **列为 M2 正解**，和 R-1 一起做；M1b 只做时基测量（P3） |
| (d) 打断场景不 lower，走 AppThread | ≈0（spring.h 自洽） | 自洽 | 是否会被打断无法事后判断：已 lower 的动画没有 readback，回落 AppThread 时同样跳 | **仅作 opt-in**：调用方声明 `interruptible` 的动画从一开始就不 lower |
| (e) 其它 | — | — | — | B 的速度分支：IV=null 时速度取 0 或不设，作为 P4 实验臂；**不进默认路径** |

**推荐组合：(b) + (d)，(c) 进 M2，(a) 否决。**

**对 M1b 的影响**

- R-2 升为 M1b 冻结门槛。冻结条件二选一：
    1. P1 证明 IV=null 取的是动画中的当前值，且 P2 中 B 的 J_vis 达标；
    2. 否则默认策略改为：带 retarget 的动画不 lower，走 (d)；没有声明 `interruptible` 的动画在 retarget 时维持 A，并标为 OPEN。
- hybrid B 从 Future Work 提前到 M1b，但只作 flag 缓解，不算 R-2 已闭合。|dvel| 和 postRMS 仍未解决。
- 结论不变：refit-τ 不作验收；seedLead 默认 0；#11–#12 只算 seed 逻辑回归。

**是否动 composition.h**

- ζ>1 不 lower 的门控**不动**。
- 只做最小改动：
    - lower 判定处新增一个条件：`interruptible` hint → 不 lower。
    - retarget 分支新增 seed 模式 flag（A：显式 IV；B：不设 IV），默认 A，直到 P1/P2 通过。
    - 速度来源仍为 C++ spring.h，不改。

**ΔpostRMS 对 τ 取法敏感，怎么处理**

- 同一份数据，Mode S（拟合 τ）得 +15.9，按实测呈现滞后 L（20–27 ms）对齐得 −12.1，连符号都翻了。所以 ΔpostRMS **降级为诊断项**，不单独决定通过或失败。
- 对齐方式按优先级：实测 L（来自呈现时间戳，不靠拟合，不吸收 R-1）> E0 纯滞后 τ > 拟合单 τ。preRMS≈59 说明本配置下单 τ 拟合本身不可信。
- 报告必须并列 S 和 L 两列。重开判定从严：**任一列**超过 +1%·travel 即算触发。
- 本次重开不依赖 ΔpostRMS，可见跳变单独就足以构成重开理由。

**可见跳变成为新主指标：是。**

- **J_vis**：墙钟同时刻采样，不需要 τ。
    
    J_vis = |x_disp(首个 post 帧) − [x_disp(末个 pre 帧) + v_disp,pre · Δt_frame]|
    
    其中 v_disp,pre 取打断前两帧屏幕位置的差分。
    
- **Δv_vis**：首个 post 帧与打断前的屏幕速度差。
- 门槛沿用 §4：J_vis ≤ 0.5 DIP 且 ≤ 5% 帧位移；Δv_vis ≤ max(20, 5%·|v|)。
- 原来的 |dpos|/|dvel|（影子对拟合曲线）降为 seed 逻辑诊断项。

## B 可粘贴修订

```markdown
<!-- deepseek_m1b_spec.md §5.3 R-2（整节替换） -->
### 5.3 R-2 飞行中 retarget：OPEN — seed 来源缺陷（严重度：高）

**状态变更**：原 KNOWN-FAIL（R-1 衍生）→ 重开。依据为 #33 真机（Period 0.32，ζ 0.78，0→240 DIP，约 156 ms 打断→0，seedLead 0）。

**机制**：retarget 时 A 路径用影子 spring.h::At(elapsed+seedLead) 显式设 InitialValue/InitialVelocity。合成器弹簧远慢于 spring.h：打断时屏幕实际位置约 14.5，影子已到约 222。新动画首帧被拽到影子位置。

**实测**
- 可见跳变 J_vis ≈ 207 DIP（约 86% 行程），等于不打断基线该时刻的 R-1 点残差。#25 报的 20–28 px 在 tInt+τ 取样（已是 B2），严重低估，作废。
- ΔpostRMS = +15.9 / +17.3 DIP（Mode S），> 1%·travel = 2.4，触发单侧重开判据。按实测呈现滞后 L（20–27 ms）对齐则为 −12.1，对对齐方式敏感。
- B2 继承影子速度，以约 642 DIP/s 冲到约 269 后慢慢回落；spring.h 的 B2 在 200 ms 内到 0。
- ΔΔpos ≈ −155、ΔΔvel ≈ −191/−201：负值，记为「与 R-1 混杂」，不计为通过。
- 呈现滞后污染 0/12 帧；单 τ ≈ 88–98 ms，preRMS ≈ 59（打断前拟合差）。

**主指标（新）**：墙钟同时刻采样，不需要 τ。
- J_vis = |x_disp(首 post 帧) − [x_disp(末 pre 帧) + v_disp,pre·Δt_frame]|，门槛 ≤ 0.5 DIP 且 ≤ 5% 帧位移。
- Δv_vis（屏幕速度差分）门槛 ≤ max(20, 5%·|v|)。
- |dpos| / |dvel|（影子对拟合曲线）降为 seed 逻辑诊断项。

**对齐规则**
- 优先级：实测 L > E0 纯滞后 τ > 拟合单 τ。refit-τ（Mode R）只作 DIAGNOSTIC。
- ΔpostRMS 必须并列 S 与 L 两列；任一列 > +1%·travel 即触发重开。ΔpostRMS 单独不决定通过或失败。

**处置（M1b）**
1. retarget 分支加 seed 模式 flag：A（显式 IV，默认）/ B（不设 IV，IV=null = FROM_CURRENT；速度仍取自 spring.h）。B 须先过 P1（null 取的是动画中的当前值）和 P2（J_vis 达标、\|dvel\| 不劣于 A）才可默认开启。
2. 新增 `interruptible` hint：声明的动画不 lower，走 AppThread。
3. composition.h：ζ>1 不 lower 门控不变；只加 hint 门控与 seed 模式 flag。
4. M1b 冻结条件：B 过 P1+P2；或默认对带 retarget 的动画不 lower。
5. 即使 B 达标，R-2 在 \|dvel\| / postRMS 上仍为 OPEN。正解是影子追踪合成器轨迹（合成器时钟 + 实测形状模型），与 R-1 一并列入 M2。

**不变项**：§4 不放宽；seedLead 默认 0（16.7 ms 仅实验 flag，不据 #25/#31/#33 调参）；#11–#12 仅作 seed 逻辑回归。
```

```markdown
<!-- notion_brief.md §4.1（整节替换） -->
### 4.1 R-2 飞行中 retarget —— 重开（OPEN，高）

- 结论：上轮「known-fail（R-1 衍生）」作废。原因是跳变量级被低估约 8 倍。
- 根因：seed 来源。影子（spring.h）远快于合成器，打断时影子约 222，屏幕约 14.5。用影子 seed，画面被拽到影子位置。
- 实测（#33）：可见跳变约 207 DIP（约 86% 行程）；ΔpostRMS +15.9/+17.3 DIP，超过单侧阈值 2.4（实测滞后对齐时为 −12.1，对对齐方式敏感，降为诊断项）。
- 新主指标：可见跳变 J_vis 与 Δv_vis（墙钟同时刻、不需要 τ），门槛沿用 §4。
- M1b 路线：hybrid B（IV=null）提前为 flag 缓解，须过 P1/P2；`interruptible` 动画不 lower；影子追踪合成器轨迹放 M2（与 R-1 一起）。
- composition.h：ζ>1 门控不动；只加 hint 门控与 seed 模式 flag。
- 演示：禁止飞行中改目标镜头。此前按 20–28 px 估计的规避建议仍成立，且更必要。
```

```markdown
<!-- usage.md · Known limitations（替换原 R-2 条目） -->
**Mid-flight retarget of a hardware-composited spring (R-2, open).**
Retargeting a spring that runs on the compositor can show a large visible position jump. The engine seeds the new animation from its own spring model, which runs well ahead of what the compositor is showing. In our test scenario (Period 0.32, ζ 0.78, 0→240 DIP, retarget at about 156 ms) the measured visible jump was about 207 DIP, with an overshoot afterwards. (Earlier documentation stated 20–28 px; that figure was measured too late and is withdrawn.)

Workarounds:
- Mark animations that may be retargeted as `interruptible`; they stay on the app thread and retarget continuously.
- Avoid retargeting compositor springs mid-flight in demos and critical UI.
- An experimental seed mode (`seedMode = FromCurrent`) may reduce the position jump; it does not fix the velocity discontinuity and is off by default.
```

## C 下一步真机探针（派 workbuddy）

共同前提：G0 通过，屏幕常亮，单个 Border 的 `Translation.X`，WGC 捕获，单位 DIP。记录呈现时间戳并算实测 L。只用程序化动画，不合成输入，不花钱，不改账号。每格都附同会话的不打断基线。报告 J_vis、Δv_vis、postRMS（S 与 L 两列）和各项增量。

1. **P1：IV=null 语义（不用影子）。** 替换目标分三种：运行中的弹簧、运行中的线性关键帧、静止属性。另加一格：同一 commit 内两次 retarget。每格 ≥10 次。
    - 判据：J_vis ≤ 一帧位移 + 噪声底 → null 取的是动画中的当前值；跳回静态起点 → B 作废。
2. **P2：A 对 B 交错对比（仅当 P1 通过）。** 打断时刻取约 60 / 156 / 300 ms（高速段、首峰附近、衰减中段），正反两个方向，每格 ≥20 次。
    - 判据：B 的 J_vis 中位数达标，且 ≤ 0.25 × A；95% bootstrap 区间不重叠；|dvel|_B 不劣于 A；双 retarget 格 ≥95% 达标。
3. **P3：合成器时基与起步分解。** 不打断，Period ∈ {0.2, 0.32, 0.5} × ζ ∈ {0.6, 0.78, 1.0}，每格 5 次。
    - 测量：从 StartAnimation 调用到首个位移帧的纯延迟 L0；以及前 200 ms 屏幕位置与影子的差值曲线。
    - 目的：把约 207 的差拆成延迟和形状两部分，为 (c) 的形状模型提供输入。
4. **P4：B2 速度来源（仅 IV=null 臂）。** InitialVelocity 三个实验臂：影子速度 / 0 / 不设。场景与 #33 相同，每臂 ≥10 次。
    - 测量：B2 上冲峰值（对照约 269）、Δv_vis、postRMS。
    - 只作诊断，不改默认的速度来源。
5. **P5：(d) AppThread 基线。** 场景与 #33 相同，强制不 lower（`interruptible`），每格 10 次。
    - 测量：J_vis、Δv_vis、postRMS、掉帧率、UI 线程占用。
    - 同时回归验证 ζ>1 不 lower 门控和 lower 判定没有被新 hint 误伤。