# workbuddy 派题 #18 — K5 StopAnimation 中段读回

日期：2026-10-20 Asia/Taipei
依据：deepseek_m1b_spec §5.2#9（K5）；与 #17 Stop 交叉但补 KeyFrame 路径 + TryGet 读回

## 目标
确认 `StopAnimation` 后属性停在当前动画值；KeyFrame 中段与弹簧中段分别测；停止后 `TryGetScalar`/`TryGetVector3` 是否可读冻结值（预期：物化后可读静态值，动画中不可依赖）。

## 步骤
1. **弹簧**：0→240 飞到约 40–60% `StopAnimation("Translation.X")`；WGC 确认冻结；再 `Properties.TryGetVector3("Translation")` / PropertySet TryGetScalar 读回。
2. **KeyFrame**：1000ms 线性或贝塞尔 0→240，中段 Stop；同样 WGC + TryGet。
3. 对照：Stop 前 TryGet（应仍静态/不可靠）；Stop 后是否变为可读冻结值。

## 交付
`artifacts/win_k5_stop_probe.txt` + 一行结论（弹簧/关键帧/TryGet 各一句）。
