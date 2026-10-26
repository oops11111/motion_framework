# workbuddy 派题 #20 — K2 CubicBezier y∉[0,1]

日期：2026-10-20 Asia/Taipei
依据：deepseek_m1b_spec §5.2#6；composition.h 默认 bezierOvershoot=false

## 目标
`CreateCubicBezierEasingFunction` 对控制点 **y∉[0,1]**：拒绝 / 截断 / 生效？决定是否可开 `Capabilities::bezierOvershoot`。

## 步骤
1. KeyFrame 单段，easing 控制点故意 y 越界，例如 (0.2, **-0.5**)-(0.8, **1.5**) 或 (0.4, 1.4)-(0.6, 1.2)。
2. 记录：Create 是否抛；Start 是否抛；若能跑，WGC 是否出现过冲/回抽（y>1 / y<0 语义）。
3. 对照 y∈[0,1] 同 x 控制点。

## 交付
`artifacts/win_k2_overshoot_probe.txt` + 一行结论：reject / clamp / honor → 是否建议打开 bezierOvershoot。
