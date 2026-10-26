# workbuddy 派题 #23 — ζ>1 NaturalMotion 轨迹保真度

日期：2026-10-20 Asia/Taipei
依据：deepseek_m1b_spec §5.2 残余 / Claude FINAL Open#2 / T-S2；#14 已核实 API 接受 ζ>1 且不钳、过阻尼可跑，但**轨迹未测**。产品仍 v1 不 lower（composition.h 硬门控）；本探针只测 API 行为，不改门控。

## 目标
过阻尼 NaturalMotion 轨迹是否：单调逼近终值、无明显过冲、settle 时间合理、ζ=1.5 与 ζ=3.0 曲线可区分。

## 步骤（亮屏；WGC + DwmFlush 门槛同前）
1. 公共：Translation.X，0→240，Period=0.32 s，不设 InitialVelocity（或 0）；记录 WGC 对齐轨迹。
2. **ζ=1.5** 跑完：是否单调（无反向过冲 >0.5 px）；终值 |err|；到达 |x−240|<1 px 的时间 vs spring.h 同参闭式 settle（允许 ±15% 或只记录比值）。
3. **ζ=3.0** 同样跑：曲线应比 1.5 **更「钝」**（更慢靠近 / 更无明显振荡）；两曲线 RMS 或峰值速度应可区分（不能几乎重合）。
4. 对照（可选一行）：ζ=1.0（临界）同 Period，确认仍可跑且无振荡峰。
5. **不要**改 composition.h；本测是合成器 API 直调，与 planner 门控无关。

## 判据（记录，不据此抬门）
- PASS 记录：单调、无过冲>0.5px、ζ=1.5 vs 3.0 可区分。
- 任一条失败只记 FAIL 现象，**不**建议立刻抬 v1 门控。

## 交付
一行结论 + `artifacts/win_zeta_gt1_traj_probe.txt`；在 `docs/workbuddy_soft_demo.md` 追加 `## 派题 #23`。
