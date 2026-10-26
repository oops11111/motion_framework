# workbuddy 派题 #22 — K3 KeyFrame AnimationController Progress/PlaybackRate

日期：2026-10-20 Asia/Taipei
依据：deepseek_m1b_spec §5.2#7
已核实：CreateAnimationController + 三参 StartAnimation 可拿到控制器；NaturalMotion TryGet 恒 null。

## 目标
KeyFrame 路径上：`Progress` 写入是否立即生效（含 DelayTime）；`PlaybackRate=±rate` 精度是否可用。

## 步骤
1. 1000ms 线性 KeyFrame 0→100；CreateAnimationController + StartAnimation(controller)。
2. 启动后设 Progress=0.5，WGC/视觉是否立刻到约中点（允许 1 帧滞后）。
3. PlaybackRate=2：时长是否约减半；PlaybackRate=-1：是否反向（从当前或需配合 Progress）。
4. 可选：DelayTime=300ms 时写 Progress 的行为（仅记录）。

## 交付
`artifacts/win_k3_controller_probe.txt` + 一行结论。
