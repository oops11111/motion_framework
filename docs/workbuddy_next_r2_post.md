# 派题 #33 · R-2 打断后误差归因（离线优先，不合成输入）

## 背景
Claude 复核（docs/claude_m1b_r2_reply.md）质疑：#25 的 postRMS≈179 DIP、行程 240 DIP，约 75%，远高于 R-1 产品残差 28.6%，因此「R-2 完全由 R-1 导致」可能不成立。#31 只比较了打断瞬间的 Δpos。

## 要做
1. 单位确认：#25 的 postRMS、|Δpos|、travel 是否都是 DIP（pxPerDip=1.0）；写明。
2. 补算三项增量（与 #31 同一套单 τ 定义，打断 − 不打断基线）：
   - ΔΔpos（已有，复述）
   - ΔΔvel：打断时刻 |Δvel| − 不打断基线在同一时刻的速度残差
   - ΔpostRMS：打断后 0–200ms 的 RMS − 不打断基线在同一时间窗、同一 τ 下的 RMS
3. 判断 postRMS 是否被呈现滞后污染：#25 单 τ≈96–98ms，但此前测得呈现/WGC 滞后约 160–200ms；around-int 采样显示打断后 WGC 仍在 259–265 而模型已经回落。说明 post 窗口里有多少帧其实还在显示 B1，给出扣除这部分后的 RMS。
4. 优先用 #25 现有逐点数据离线算；如果数据不够，可以重跑 #25 同参数真机（只用程序化动画，不合成鼠标/键盘输入），并同时跑一条不打断的 B1 作基线。

## 判据（Claude 建议的单侧规则）
任何一项增量明显 > 0（Δpos > +0.5 DIP；Δvel > max(20, 5%|v|)；ΔpostRMS > 1%·travel）→ R-2 不是纯 R-1 衍生，需重开。负值不算失败。

## 交付
- artifacts/win_r2_post_attrib.txt
- docs/workbuddy_soft_demo.md 加 ## 派题 #33
- 不改 composition.h；一行中文结论
