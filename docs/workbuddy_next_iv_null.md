# workbuddy 派题 #27 — InitialValue=null 行为（仅记录）

日期：2026-10-20 Asia/Taipei
依据：deepseek_m1b_spec §5.2#10；产品**不依赖**此行为（打断一律显式 seed）。

## 目标
不设 InitialValue（null）时，NaturalMotion 是否从「当前被动画属性值」起跳。

## 步骤
1. 先静态 SetValue Translation.X=37（或 InsertVector3），再 StartAnimation：FinalValue=200，Period=0.32，ζ=0.78，**不设 InitialValue**，IVel=0。
2. WGC：起步是否从 ≈37（无跳到 0 或其它）？
3. 对照：同参但 InitialValue=0 —— 是否从 0 起跳（应有跳变）。
4. 可选：动画飞行中再 Start 替换且不设 IV（仅记录，不依赖）。

## 交付
一行结论（FROM_CURRENT / FROM_ZERO / OTHER）+ `artifacts/win_iv_null_probe.txt`；`## 派题 #27`。
不改 composition.h；结论仅记录。
