# workbuddy 派题 #17 — S5 弹簧停点 / Withdraw(Completed)

日期：2026-10-20 Asia/Taipei
依据：deepseek_m1b_spec §5.2#4

## 目标
亮屏确认：NaturalMotion 弹簧到达 FinalValue 后属性保持终值；Withdraw/Stop 在「已完成」附近快照前后终值一致。

## 步骤
1. SpringScalar：Period=0.32 ζ=0.78，0→240，无 Delay；跑到静止（WGC |Δ| 小且近 240）。
2. 完成后继续采样 ≥500ms：值应保持 ≈240（S5 保持终值）。
3. 另跑一趟：接近完成时 `StopAnimation`，读回/WGC 终值是否留在当前（与 K5 交叉，本条侧重「完成后保持」）。
4. 若有 ScopedBatch.Completed：记录 Completed 时刻 vs 视觉到位时刻（可与 #13 滞后对照，不强制过门）。

## 交付
`artifacts/win_s5_settle_probe.txt` + 一行结论。
