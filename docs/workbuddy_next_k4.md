# workbuddy 派题 #21 — K4 KeyFrame 完成后保持终值

日期：2026-10-20 Asia/Taipei
依据：deepseek_m1b_spec §5.2#8

## 目标
亮屏确认 KeyFrame 动画完成后属性保持终值（与 S5 弹簧对照）。

## 步骤
1. ScalarKeyFrame 0→100，Duration=400ms，线性或 K1 已验证 easing。
2. 到位后 WGC 再采 ≥500ms：应≈100，range 小。
3. 可选：ScopedBatch.Completed 时刻 vs 视觉到位（仅记录）。

## 交付
`artifacts/win_k4_hold_probe.txt` + 一行结论。
