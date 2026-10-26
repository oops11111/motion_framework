# workbuddy 派题 #24 — 弹簧 rate 折叠：同跑 rate=1 对照

日期：2026-10-20 Asia/Taipei
依据：deepseek_m1b_spec §5.2#2/#21；Claude FINAL Open#3；#15 抽查 rate=2 RMS≈13.6%（≈固有差异），无法与底残差分开。

## 目标
同一次亮屏会话、同一对齐管线，对比：
- **rate=1**：Period=0.32，IVel=v0（基线，期望 RMS ≈ 产品固有残差 ~12–28%）
- **rate=2**：Period=0.16，IVel=2·v0（折叠公式）

判断 rate=2 的额外误差是否显著高于 rate=1 基线。若两者同量级 → 公式可保留、v1 场景可继续用 rate 折叠（仍建议产品默认 rate=1 直到打断种子测完）。

## 步骤
1. Translation.X，0→240，ζ=0.78；WGC+DwmFlush 同前；时间对齐方法与 #13/#15 一致并写明。
2. 选一个非零 IVel（建议 v0=400 DIP/s，或与 #15 相同），两跑共用。
3. **A**：rate=1 → Period=0.32，IVel=v0；记对齐后 vs spring.h 闭式 `x(t)` 的 RMS/max。
4. **B**：rate=2 → Period=0.16，IVel=2·v0；对齐后 vs 闭式 `x(2·t)`（或等价折叠模型）的 RMS/max。
5. 报告：RMS_A、RMS_B、Δ=RMS_B−RMS_A；是否「同量级（Δ 小于基线一半）」或「rate=2 明显更差」。

## 交付
一行结论 + `artifacts/win_rate_fold_probe.txt`；`docs/workbuddy_soft_demo.md` 追加 `## 派题 #24`。
不改 composition.h / 不改公式，只记录。
