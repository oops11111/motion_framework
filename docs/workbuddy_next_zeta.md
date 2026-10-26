# workbuddy 派题 #14 — ζ>1（S2）真机 + composition.h 门控

日期：2026-10-20 Asia/Taipei  
角色：workbuddy cli @ DESKTOP-K93N3P8  
约束：不花钱；改动尽量小；证据写入 artifacts/ 与 docs。

## 背景
Notion 草稿 `docs/notion_deepseek_review.md` 第 4 点 + DeepSeek §5.2#1 / 审校注①：
- v1 对 ζ>1 **暂不 lower** 到 NaturalMotion
- 现有 `composition.h` 只要 `caps.springNaturalMotion` 就会 lower（含 ζ>1）→ 规格与规划器不一致
- 需真机确认：`DampingRatio=ζ>1` 是接受还是截断

## A. 真机探针（优先）
在 MotionSoftDemo 或最小 C# 片段里，对 `SpringScalarNaturalMotionAnimation`（或 Vector3）设置：
- Period = 0.32s
- DampingRatio ∈ {1.0, 1.2, 2.0, 5.0, -0.1}（负值仅看是否抛/夹）
- FinalValue 有位移；可选 InitialVelocity=0

记录每档：
1. 赋值是否抛异常
2. 读回 `DampingRatio` 实际值（是否被钳）
3. 若能跑：亮屏下用现有 WGC/日志看是否过阻尼形态（无振荡），或至少 Completed/终值是否合理

门槛：显示器亮着；沿用既有 G0（DwmFlush / 亮屏）习惯。
日志写到：`C:\Users\admin\motion_framework\artifacts\win_zeta_gt1_probe.txt`（没有 artifacts 就建），并让协调侧能收到副本。

## B. composition.h 门控（与探针并行可先做）
源：`C:\Users\admin\motion_framework\code\composition.h`（改完把 diff 或整文件同步思路交回；box 源在 `/workspace/motion_framework/code/composition.h`）

在两处弹簧 lower 前增加 ζ>1 回退（`PlanOverride` ≈274 行附近、`PlanGraph` hasSpring ≈314 行附近）：

```cpp
if (c.damping > 1.0) return Fallback(pp, "overdamped spring (ζ>1) not lowered in v1");
```

`c` 为 `mapping_.ToCompositor(...)` 之后的参数（即 `dampingRatio` 字段）。`PlanOverride` 里在 `ToCompositor` 之后、设 `pp.kind` 之前插入；`PlanGraph` 同理。

注释补一句：v1 过阻尼走 AppThread / spring.h；待 S2 真机后再议是否开放。

## C. 回归
- box 或 Win：若有 composition smoke / 现有 tests，跑一遍；至少手动确认 ζ=0.8 仍 lower、ζ=1.2 变 Fallback。
- 若 Win 无现成 composition 单测，写 10 行内的本地断言或在现有 explore 测试里加两行亦可。

## 交付
1. 探针日志 + 一句话：接受 / 截断 / 抛异常（各 ζ）
2. composition.h diff（或说明已改路径）
3. 回归结果
