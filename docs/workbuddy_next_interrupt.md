# workbuddy 派题 #25 — 打断连续性（对照显示轨迹，单时间偏置）

日期：2026-10-20 Asia/Taipei
依据：Claude FINAL Open#1；deepseek_m1b_spec §2.4 / §4 打断判据；#11–#12 曾记「已过」，需按 **产品映射 + 全程同一时间偏置** 重测（显式 seed 误差预算 = 固有残差 12–28%）。

## 目标
弹簧飞行中改目标：影子种子 `spring.h::At(elapsed + seedLead)` 设 InitialValue/InitialVelocity 后 StartAnimation 替换；对照 **WGC 显示轨迹**，全程只用 **一个** 时间偏置 τ（不要打断前后分别对齐把 δ 抹掉）。

## 步骤（亮屏）
1. Period=0.32，ζ=0.78，0→240，IVel=0；WGC 连续采。
2. 约 150 ms（或行程 ~30–50% 时）打断 → 新目标 0；种子用 spring.h::At(t_wall + seedLead)。
3. **seedLead 试 0 与 1 帧（≈16.7 ms）** 各跑一次（或同日志两段）。
4. 选单一 τ（可用整段 best-τ 或固定呈现滞后中位数），对齐后算：
   - 打断瞬间 |Δposition|、|Δvelocity|（影子种子 vs WGC 数值微分）
   - 打断后 0–200 ms 轨迹 RMS vs 影子续跑模型
5. 对照 §4：|Δv|≤0.5px 且 ≤5% 帧位移；|Δvel|≤max(20px/s, 5%|v|)。达不到则记录实际量级（对照固有残差带），**不改门控**。

## 交付
一行结论 + `artifacts/win_interrupt_seed_probe.txt`；`## 派题 #25`。
说明所用 τ、seedLead、是否单偏置。
