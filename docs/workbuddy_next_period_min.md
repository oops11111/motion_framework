# workbuddy 派题 #26 — Period 极小值接受域 / 钳制

日期：2026-10-20 Asia/Taipei
依据：deepseek_m1b_spec §5.2#22；token 允许 response 低至 0.001 s。

## 目标
`SpringScalarNaturalMotionAnimation.Period` 对极小值：接受、钳制、还是抛异常？过小 Period 动画是否仍可跑。

## 步骤（亮屏，可短跑）
1. Translation.X 0→100，ζ=0.8，IVel=0。
2. 依次设 Period = 0.001 s、0.01 s、0.05 s、0.32 s（对照）：
   - 赋值是否抛；读回 Period 是否等于设定或被钳到某下限；
   - 若接受：短采 WGC，是否有可见运动 / 是否立刻到终值 / 是否卡住。
3. 可选：Period=TimeSpan.Zero 或负值（若 API 允许构造）——只记抛或不抛。

## 交付
一行结论（ACCEPT / CLAMP@x / THROW）+ `artifacts/win_period_min_probe.txt`；`## 派题 #26`。
不改 composition.h。
