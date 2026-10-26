# workbuddy 派题 #19 — K1 KeyFrame easing 作用段

日期：2026-10-20 Asia/Taipei
依据：deepseek_m1b_spec §5.2#5（K1）

## 目标
确认 `InsertKeyFrame` / `InsertExpressionKeyFrame` 的 easing 作用于「**结束于该关键帧的那一段**」（官方语义），用单段对照 `EvaluateKeyframes`。

## 步骤
1. 单段 220 ms KeyFrame：0 → 100，easing = decelerate 类三次贝塞尔（如 0.1,0.9,0.2,1.0）或文档示例控制点。
2. 亮屏 WGC 采样；与引擎 `EvaluateKeyframes` / bezier.h 同参数曲线时间对齐后比 RMS。
3. 可选双关键帧：线性段 + 带 easing 段，确认 easing 只影响后一段。

## 交付
`artifacts/win_k1_easing_probe.txt` + 一行结论（同意官方语义 / 否 / 部分）。
