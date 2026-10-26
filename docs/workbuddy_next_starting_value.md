# workbuddy 派题 #29 — InsertExpressionKeyFrame(0, "this.StartingValue")

日期：2026-10-20 Asia/Taipei
依据：deepseek_m1b_spec §5.2#20；DeepSeek 推荐首帧用 StartingValue；Claude 倾向显式初值。

## 目标
KeyFrame 首帧 `InsertExpressionKeyFrame(0f, "this.StartingValue")` 是否从动画开始时属性当前值起跳、无跳变；中断重提交时行为。

## 步骤（亮屏）
1. 静态 SetValue Translation.X=40；1000ms 线性 KeyFrame：帧0=`this.StartingValue`，帧1=100；Start。WGC 是否从≈40 平滑到 100（无先跳 0）。
2. 对照：帧0 显式 InsertKeyFrame(0, 0) → 应从 0 起（相对 preset 有跳变）。
3. 可选：飞行中 Stop + 再 Start 同表达式首帧 —— 是否从停止处续（仅记录）。

## 交付
一行结论 + `artifacts/win_starting_value_probe.txt`；`## 派题 #29`。
