# 派题 #31 · R-2 增量 Δ 离线补算（不必重跑真机）

## 目标
验证 Notion 裁决：R-2 是否「纯 R-1 衍生」。

定义：
**增量 Δ** = |打断时刻 单τ |Δpos|| − |同一墙钟时刻、不打断基线（同映射弹簧全程 0→240）相对 spring.h/影子 的残差|

若增量 Δ ≈ 0（建议阈值：Δ ≤ 0.5 px）→ 支撑「R-2 = R-1 衍生 known-fail」。
若增量 Δ > 0.5 px → 报告需重开 R-2（seed 另有问题）。

## 数据源（优先离线，勿重跑）
- 打断：`artifacts/win_interrupt_seed_probe.txt`（#25）
  - seedLead=0：打断时 WGC_pos≈252.96，seed=225.35，|Δpos|=27.612，τ≈98ms，el≈162ms
  - seedLead=16.7：|Δpos|=20.093，τ≈96ms，el≈154ms
- 不打断基线：优先复用同产品映射（Period=0.32 ζ=0.78 0→240）的既有日志，例如：
  - `artifacts/win_m13_log.txt` / 派题 #13 产品映射残差相关
  - `artifacts/win_rate_fold_probe.txt` rate=1 段
  - `artifacts/win_m11_log.txt` / `win_m12_log.txt` 若含完整轨迹
  - 其它已对齐 WGC vs spring.h 的弹簧全程跑

## 做法
1. 在基线轨迹上，取与 #25 打断时刻相当的「行程进度」或「对齐后 elapsed」（优先用单τ 对齐后的同一相对时间）。
2. 计算该时刻 |WGC − spring.h/影子|（或既有 RMS 采样点插值）。
3. 对 seedLead=0 与 16.7 各报一行：打断|Δpos|、基线残差、增量 Δ。
4. 若基线缺精确时刻采样：说明近似方法与不确定度；**仍不要**为凑数重跑，除非数据完全不够且你注明原因后征得协调 bot。

## 交付
- `artifacts/win_r2_delta_offline.txt`（含计算步骤、数字、结论一句）
- 在 `docs/workbuddy_soft_demo.md` 或等价处加 ## 派题 #31
- **不改** composition.h

## 一行结论模板
增量Δ(seedLead=0)=…；Δ(16.7)=…；相对 0.5px 门槛 → 支撑/不支撑「纯 R-1 衍生」。
