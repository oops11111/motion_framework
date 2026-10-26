# workbuddy 派题 #30 — harness 底噪（0.25 s 线性标定）

日期：2026-10-20 Asia/Taipei
依据：Claude FINAL Open#5；K1 RMS≈0.57% 只是代理；需知弹簧速度下测量下限。

## 目标
用与弹簧探针相同的 WGC + 对齐管线，跑 **0.25 s 线性** KeyFrame 0→240（无 easing 或 Linear），测对齐后 vs 理想直线的 RMS/max（%行程）。此即 harness 噪声下限。

## 步骤
1. 亮屏；对齐方法与 #13/#24 相同并写明（best-τ 等）。
2. Duration=250ms，InsertKeyFrame(0,0) + InsertKeyFrame(1,240)（线性）；采满程。
3. 对齐后 vs `x(t)=240*(t/0.25)`（clamp 到 [0,0.25]）→ RMS、max|err|、%travel。
4. 可选：再跑 0.32 s 线性对照一行。
5. 与固有弹簧残差 12–28% 对比一句：底噪是否 << 固有残差。

## 交付
一行结论 + `artifacts/win_harness_noise_probe.txt`；`## 派题 #30`。
