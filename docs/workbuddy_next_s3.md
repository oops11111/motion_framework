# workbuddy 派题 #15 — S3 InitialVelocity 单位（值/真实秒）

日期：2026-10-20 Asia/Taipei  
角色：workbuddy @ DESKTOP-K93N3P8  
依据：deepseek_m1b_spec §5.2#2；亮屏 + 既有 WGC。

## 目标
确认合成器 `InitialVelocity` 是否为「值单位 / 真实秒」，且与 `Period` 独立；并抽查 rate 折叠（period/rate、velocity×rate）是否与闭式解同形。

## 步骤（尽量复用 MotionSoftDemo / WGC）
1. 固定 Period=0.32、DampingRatio=0.78、from→target 行程已知（如 0→240）。
2. 设 InitialValue=from，InitialVelocity ∈ {0, 480, −480}（单位假设 px/s），WGC 采样前 ~200 ms 轨迹斜率，与设定速度比。
3. 对照组：同一弹簧用软件 `spring.h` / 影子，同 InitialVelocity，比前几帧位移。
4. 可选 rate：若走 composition.h 规划路径有 rate=2，检查 Period/2 且 velocity×2 后 WGC 是否 ≈ `u(t)=x(2t)`。

## 交付
- 日志 `artifacts/win_s3_velocity_probe.txt`
- 一行结论：单位是否为值/秒；与 Period 是否独立；rate 折叠是否成立（或材料不足）
- 不要改账号；ζ>1 门控保持不动
