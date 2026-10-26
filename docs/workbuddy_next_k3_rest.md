# workbuddy 派题 #28 — K3 剩余：Progress+Delay、|rate|>16、Progress+反向

日期：2026-10-20 Asia/Taipei
依据：deepseek_m1b_spec §5.2#7/#14；Claude FINAL Open#4；#22 已确认 Progress 立即生效（时间归一化）与 PlaybackRate 2/−1。

## 目标
补齐 KeyFrame 控制器边角：
1. Progress + DelayTime 组合
2. |PlaybackRate|>16（超出文档 [−16,16]）
3. PlaybackRate=-1 且配合 Progress 定位

## 步骤（亮屏，CreateAnimationController + 三参 Start）
1. **Progress+Delay**：1000ms 线性 0→100；DelayTime=300ms；启动后（延时期或延时后）写 Progress=0.5；记录 WGC 是否跳到约中点、延时是否被打断/重置（只记录现象）。
2. **|rate|>16**：设 PlaybackRate=20 与 −20（或 17）；是否钳到 16、抛异常、还是接受；时长/方向是否异常。规划器预期 |rate|>16 → AppThread——本测只记 API。
3. **Progress+反向**：PlaybackRate=-1，Progress=0.75（或先播一段再 -1）；是否从对应点反向走向 0。

## 交付
一行结论 + `artifacts/win_k3_rest_probe.txt`；`## 派题 #28`。
不改 composition.h。
