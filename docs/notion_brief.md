# 项目说明页要点（Notion AI 整理）

> 来源：Notion AI · Opus 5.5 · 2026-10-20（Asia/Taipei）  
> 输入提示：`docs/_notion_prompt_brief.txt`  
> 说明：正文基于 Notion AI（Opus 5.5）「Copy response」；已还原假链接文件名。§3.2 按协调 bot 2026-10-20 反馈小改（真实 fm_* Tick；C-ABI 边界已说明）。固有差异数字与 §4.1 按 FINAL / 空窗期 A/B/C 同步。

---

# FluidMotion 项目说明

> **一句话**：FluidMotion 是 Windows 桌面端动效框架，为 WinUI 3 提供由合成器驱动的过渡和微交互。动画默认可打断，打断后保留速度。设计侧通过 Figma motion tokens 和 Rive 接入，首发宿主为 WinUI 3。
> 

---

## 1. 架构

*出处：summary.md / plan.md，另有注明的除外*

| 层面 | 要点 |
| --- | --- |
| 执行层 | 基于 WinUI 3 / Windows.UI.Composition。常规过渡在 **DWM 合成线程**上运行 |
| 引擎 | 用 C++/WinRT 编写，对外提供 WinMD，供 C# 调用。**不用 Rust**。公开接口不混入 C ABI，Rive/Skia 的内部边界除外 |
| 动画模型 | 声明式时间线，使用三次贝塞尔曲线和命名缓动 token。弹簧采用闭式解，改目标时保留速度。**默认可打断** |
| 复杂矢量 | Rive 内容渲染到 CompositionDrawingSurface。表面的位移、缩放和淡入由合成器属性驱动 |
| 设计到代码 | Figma 插件导出 JSON motion tokens。支持热重载预览，CI 校验 token，并做黄金帧回归 |
| 宿主 | 主接口为 `GetCompositionVisual()`。首发附带一个 HWND 参考示例，**但不提供支持**。`EmbedToHwnd` 推迟到 **V1.5**（disputes.md） |
| 备选方案 | 如果 Rive/D3D 验证失败，改用 **Skia + swapchain**，token 和引擎保持不变 |

---

## 2. 里程碑

*出处：plan.md / explore_round.md*

- **总工期（裁决建议）**：约 6 名全职人员 × 30–34 周。**待验证**：技术验证完成后重新计算。

| 里程碑 | 预计时间 | 内容 |
| --- | --- | --- |
| M0 技术验证 | 约第 2 周 | 验证 Rive 渲染进合成表面、120Hz 拖动时的错位、指针属性集、ALLOW_TEARING 和管线延迟 |
| M1 核心 | 约第 10 周 | 数学、时间线和 token 可在无界面环境下测试。分发器降级为合成器动画 |
| ├ M1a | 与 M1b 并行 | 纯逻辑部分，**已通过** |
| └ M1b | 与 M1a 并行 | 合成器真机部分 |
| M2 Alpha | 约第 16 周 | — |
| M3 Rive + 首个试点 | 约第 22 周 | — |
| M4 Beta | 约第 28 周 | — |
| M5 1.0 | 约第 32–34 周 | — |
- **已裁决**：M1 拆分为 M1a 和 M1b 并行推进。任务 6 可以与 M0 并行（explore_round.md Q2）。
- **探索轮建议**（explore_round.md）：
    - 先做 DeepSeek 阶段 0–2，即 golden 测试和软件路径演示，同时补充 composition 测试。
    - 合成器单属性试点与 Gemini Day2 对齐。

---

## 3. 当前定案与真机进展

### 3.1 决策（explore_round.md）

- **Q1 = A**：要求两侧都有测试证据，即 Linux g++ 和 Windows MSVC cl。日志存放在 `artifacts/`。
- **Q2**：M1 拆分为 M1a 和 M1b，并行推进。
- **Q3**：只要软件路径能跑通，就算「可演示」，不必先完成合成器 lowering。

### 3.2 软件路径（workbuddy_soft_demo.md #4–#12）

- MotionSoftDemo（WinUI 3 / WASDK 2.5.1）**已编译并成功启动**。
- **已接入** `code/capi`（`fluidmotion.dll` / `fm_*`），使用**真实弹簧 Tick**；早期假 Tick **已移除**（详见 workbuddy_soft_demo.md 派题 #4–#12）。
- **已说明**：此处 C-ABI 属于演示 / WinUI 接入边界，与「公开 WinMD 不混 C ABI」**不冲突**（公开产品接口仍走 WinMD；C-ABI 仅作本机演示与桥接）。

### 3.3 弹簧参数

*出处：workbuddy_soft_demo.md / deepseek_m1b_spec.md*

- **定案（S1 v2，2026-10-20）**：`Period = response/(2π)`（合成器按 ω₀ = 1/Period 解释）；`DampingRatio = ζ`。见下方「2026-10-20 裁决」与 [notion_s1_remap_ab.md](./notion_s1_remap_ab.md)。
- **证据**：#36 时间缩放 s=0.159≈1/(2π)（9/9 格）；#41 remap 后 best-τ RMS 0.07–0.20%（10/10）。
- ~~v1 `Period = response`~~（已作废；早期“峰峰≈0.511 s”证据不能区分时间缩放，仅历史记录）。
- ⚠️ **不要使用** `Period = response / √(1−ζ²)`。

### 3.4 NaturalMotion 与播放速率（已真机核实）

- 对 NaturalMotion（弹簧）调用 `TryGetAnimationController`，结果**始终为 null**。
- AnimationController 只适用于 KeyFrameAnimation，所以弹簧**不能靠 PlaybackRate 调速**。
- 处理方式：把速率折算进 `Period` 和 `InitialVelocity`。

### 3.5 帧时序（workbuddy_soft_demo.md）

- 亮屏时，DwmFlush 和合成器约为 **60Hz vsync**。
- UI 线程的 `CompositionTarget.Rendering` 仍约为 **p50 ≈ 31ms（约 32Hz）**。这是 UI 线程节流造成的，与熄屏无关。
- 软件路径的物理积分应改用**引擎稳态时钟**。这是派题 #9，**状态待确认**。

### 3.6 XAML 偏移

- 驱动 offsetX/Y 时，必须使用 `Translation.X/Y` 并调用 `SetIsTranslationEnabled`。直接设置 `Offset` 会被布局覆盖。

### 3.7 Claude M1b 单属性试点（claude_m1b_spec.md）

- 覆盖两项：opacity 使用 keyframe + bezier；offsetX 使用可打断的 spring。
- 在 Capabilities 中需要关闭尚未支持的 `partialStart`、`rate` 和 `reverse`。

---

## 4. 已知约束与分歧裁决

*出处：disputes.md / summary.md*

- **HWND**：首发不提供完整的 `EmbedToHwnd`。Airspace 等属于硬性限制。
- **帧预算**：
    - 按刷新间隔的比例来设定。
    - **8.3ms** 是每个阶段的失败红线。
    - P95 同时受绝对上限和相对回退约束。
    - 初期对 P99 丢帧率只做告警。
- **Rive 跟随指针的延迟**：无法普遍消除，作为设计约束写入。具体延迟**以实测为准**。
- **合成器弹簧 vs `spring.h` 固有轨迹差异（known-fail R-1，FINAL）**：~~v1 映射（`Period=response`，`ζ_c=ζ`）下形状残差为场景 A **28.6%** / E **12.1%** RMS~~（**已作废**，仅历史对照；S1 v2 后见「2026-10-20 裁决」）；扫参最优约 8%/6% 仅作对照。≤1%·travel 门槛不放宽、不为达标调参。记为已接受固有差异（claude_m1b_deepseek_review.md FINAL；notion_claude_offline_abc.md）。

---


### 4.1 R-2 打断续接——本机 FAIL 结题（#47）；M1b 降级实现 B_v，不满足 R-2
- 根因：影子 seed 来源有缺陷。#37 测得影子速度约为屏幕 2.6×，A 臂可见跳变约 100–207 DIP。
- 缓解：hybrid B 进入 M1b，flag 默认 ON。
  - IV=null 取当前值（#34 P1 已证实）；速度取 WGC 末两帧；
  - 守卫不满足时降级为 IV=null + v=0，永不回退 A。
- 证据（#35，440 次交错，composition.h 未改）：
  - J_vis 中位 A 170 / B 2.0 DIP，比值 0.01–0.02；
  - |dvel| 中位 A 359 / B 58；
  - 宽阈（帧位移 + 3.11）下 B 7 格均 20/20，dbl 格 20/20。
- 门槛变更：原 0.5 DIP 严格门低于 harness 底噪（3.11 DIP），测不出来，改为
  J_vis ≤ d_frame + N_h（逐格 ≥95%）
  + 合并比值 CI 上界 ≤ 0.25
  + Δv 相对 A（逐格 ≤ A，合并 ≤ 0.25×A）。
  0.5 DIP 与 Δv 绝对门转为 M2 目标。
- 淘汰：B′（加速度外推）。合并 J 2.02→2.95，|dvel| 58→59，没有稳定改善，还放大了时间抖动。
- 冻结前置：#35 重算②′；第二台机器（换刷新率）补测 t156dbl（n≥60）与 t060rev。不过 → 默认改 OFF，B 可 opt-in。
- 不变：seedLead=0；interruptible 保持 opt-in；refit-τ 不作验收。
- M2：
  - (c) 影子追踪合成器先查清 2.6× 根因；
  - harness 底噪降到 < 0.5 DIP；
  - Δv 绝对门；
  - 查清 L 偏高（B 40–45 ms vs A 20–29 ms）；
  - 多刷新率矩阵；
  - 平滑速度估计（可选）。
- 演示：飞行中改目标镜头仍建议规避，直至 C5 冻结前置通过。
- 出处：notion_r2_b_gate_ab.md B 节（2026-10-20 Asia/Taipei）。
- **#45 裁决（R-2 条件冻结，C5(b) 除外）〔已被 #46/#47 取代〕**：#45 会话有效（P0 b=1.00）；无臂升主，M1b 维持 hybrid B 并以 B 条件冻结 R-2；跨机 C5(b)（第二台机器/换刷新率）仍未完成，不在冻结范围内。Aφ 弃用；A+8 只做零新跑诊断，不达立项条件即永久关闭。
  - B 已知缺陷：t156×3、t300×2 存在真实速度不连续（Δv FAIL-确定）；seedV 异常（t060 约 −14000、φ*=2 一格为 0；t156 约 1830 vs v_ref 522，疑陈旧速度）；B t060 数值「待 #46」。
  - 解冻条件（同时满足）：#46 A0 t060 J 降到 A0 J 中位 ≤ B，且 E3r 下 A0 t156 Δv 达标。
  - 负 lead 已否定（#43：J 随 X 单调增大，Am 系列删除）。
  - ω₀ 口径：ω₀ = 2π/r = 1/Period（Period = r/(2π)）；“ω₀=2π/Period”为笔误作废。
  - 估计器沿革：A5/E2（#43）→ r2-est-v3/E3（#43 后）→ D1–D3 偏差并入（#45）；#46 追加零新跑项见 deepseek_m1b_spec.md §5.3.4–5.3.6。
  - 出处：notion_r2_estimator_ab.md、notion_r2_43_ab.md、notion_r2_45_ab.md B 节。
- **#46/#47 裁决（R-2 本机 FAIL 结题，取代上文 #45 条件冻结）**：
  - #46：现有臂（B/A0/Aφ）按 ③ 修订 + h45 口径终局不通过，A+8 关闭；t156 Δv 定为结构性不可判；新鲜帧规则 v2 登记偏差 D4（仅测试工具）；R-2-IMP-1：禁止 vB=0，种子速度一律取影子解析速度，WGC 不再取速度。
  - #47（会话有效，P0 b=1.00）：B_v = 不设 InitialValue（位置取合成器当前值）+ 种子速度 S1 v2 影子解析速度，禁止 vB=0。J 仅 t300 两格通过；t060 20.9/16.7、t156 7.0/3.5/7.0 DIP 失败；Δv 可判 4 格达标 3（t300rev 21.4>20）；③ 修订 (a)✗(b)✓(c)✗。
  - B_v 的 J ≈ |v|×7–13 ms（位置约落后 1 帧）；滞后补偿不在本裁决范围。
  - 结论：R-2 本机 FAIL 结题；M1b 以 B_v 作降级实现，标注「不满足 R-2」；剩余只有 C5(b) 第二台机器。
  - 出处：notion_r2_46_ab.md B 节、artifacts/win_r2_p11_47.txt；规格见 deepseek_m1b_spec.md §5.3.7。

- **R-3 注记（2026-10-24 Asia/Taipei，#49）**：开题 R-3（commit/位置陈旧，取代 R-2 的 R-1 归因）。#49 E3 真机对照叠加 Sup vs Bv：t060 上 Sup J 中位 1.55≪Bv 25.86、Δv_fd 130.7≪716.8，但 ε 中位 21.8 ms > 1 帧 → **不建议采用叠加**（失败模式=响应延迟；E2 未跑）。R-3 仍 OPEN。详见 [r3_commit_staleness.md](./r3_commit_staleness.md)、artifacts/p13_49.txt。
- **R-3 = CLOSED（2026-10-24 Australia/Perth）**；S1 = STOP / NO-ADOPT；Bv = 降级兜底（非合格基线，不作发版依据）。Notion / Gemini / DeepSeek #52 复核一致同意关闭并保留 Bv；Claude Free 对照不阻塞关闭（claude_r3_52_close.md 为 Notion AI 代笔兜底，已落盘）。composition.h 未改（sha256 a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418）。
- **三条挂账**：① 关闭理由 = 闸门触发（A v2×3=2/3 < 3/3 → STOP S1），非数据复审结论；② Bv 挂账：P1 a1 分不开（pooled purity=0.83）、P3 = CONFIRM_F5_REFUTE_2X（med_res=+2.70；SSE 14.5×）、≈30c=无、S2 不复活、不与 #49 对比、不作发版依据；③ A1a 跟踪转出：a3 @ τ0=21.8，RMS 中位=0.781，不随 R-3 关闭而消失。
- 详见：[r3_commit_staleness.md ## 结题（CLOSED）](./r3_commit_staleness.md)｜[notion_r3_52_pack.md](./notion_r3_52_pack.md)｜[artifacts/p16_52.txt](../artifacts/p16_52.txt)｜[claude_r3_52_close.md](./claude_r3_52_close.md)

### 2026-10-20 裁决：S1 映射改为 Period = r/(2π)
- 事实：#36 证明合成器把 Period 当作 1/ω₀；#41 中 remap 后最优 τ 对齐 RMS 0.07–0.20%（10/10 ≤ 1%），orig 为 36–57%。
- 决定：composition.h 落地 Period = r/(2π)，ζ_c = ζ；落地后的新 sha 记入本页（旧：030e3a8d…daeb82）。
- **已落地（2026-10-20 Asia/Taipei）**：`code/composition.h` sha256 = `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（`MapSpringToCompositor` 单一映射函数；composition_tests 2305/0）。
- R-1：形状判据改为逐次 best-τ（窗口 25–50 ms）RMS ≤ 0.5%；延迟单独判，τ ∈ [28,45] ms。单机有条件关闭，跨机确认搭 C5(b)。
- 打断：remap 后 A 臂 J 由 203 降到 4.4，但严格判据 0/20，原因是 seed 时钟在屏幕显示之前。hybrid B 仍为 M1b 缓解。
- 待办：
  - #41b 离线：细网格联合拟合 (τ, s)；反解 Δt_eff。
  - #42：按 R-2′ 门判四臂（N_h 在 remap 下重建）。
  - #43（条件触发）：负 lead（−Δt_eff、−21 ms）+ hybrid B 对照。〔已执行：负 lead 已否定，见 §4.1 #45 裁决〕
  - C5(a)：若含影子比对，在 remap 下复跑。
  - t156dbl：仍等 C5(b)。

## 5. 待验证清单

| 事项 | 出处 |
| --- | --- |
| WinUI 3 中是否可用 `GetPointerPositionPropertySet` | disputes.md / summary.md |
| ALLOW_TEARING | disputes.md / summary.md |
| 管线延迟 | disputes.md / summary.md |
| 弹簧边界：ζ>1 轨迹保真、Period 极小值钳制仍待验；InitialVelocity 单位与 ζ 接受域已核实（#14/#15） | disputes.md / deepseek_m1b_spec.md |
| 总工期 30–34 周，需在 M0 后重新计算 | plan.md |
| Rive/D3D 路径能否通过验证（失败则启用 Skia 备选） | plan.md |
| 软件路径改用引擎稳态时钟（派题 #9）的落实情况 | workbuddy_soft_demo.md |

---

## 6. 协作分工

*出处：collab_roster.md*

| 角色 | 承担方 |
| --- | --- |
| 协调 | 动效框架 bot |
| 设计 / 探索 | Gemini |
| 实现 / 审阅 | Claude |
| 复评 | DeepSeek |
| 本机执行 | workbuddy |
| 文档整理 | Notion AI |
- **规则**：不花钱，不改动各 bot 的账号。

---

## 主线 / 旁路分流（2026-10-24 23:35:47 AWST，Australia/Perth）

- **主线** = **M2 → M5**（继续推进；见 [m2_mainline_brief.md](./m2_mainline_brief.md)）。
- **旁路停放** = 挂账与 B_v 降级，见 [side_parked_hang_degrade.md](./side_parked_hang_degrade.md)（建议分支名 `side/parked-hang-degrade`；仓当前非 git）。
- 旁路 **不阻塞** 主线；**不重开 S1 / R-3**；挂账工作 **不改 `composition.h`**；成功 ≠ 采用。
