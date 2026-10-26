# M2-H5-SEED 闸门补洞决断（可 LOCK）

> DECISION_AT=2026-10-25 00:16:02 AWST（Australia/Perth）  
> 依据：`docs/gemini_m2_h5_seed_design.md`、`docs/deepseek_m2_h5_seed_review.md`、`docs/m2_h5_seed_gate_DRAFT.md`（旧）、`artifacts/win_r2_p4_b2vel.txt`、`artifacts/m2_01_g3_h1h5_audit.txt`  
> 完整可 LOCK 正文：`docs/m2_h5_seed_gate_DRAFT.md`（本决断后覆盖）  
> **不授权采数**；须 LOCKED + APPROVED 后才可派 workbuddy。

## 拍板（缺口 1–6）

| # | 缺口 | 决断 |
|---|---|---|
| 1 | G2-LOCK-0 默认路径 | **采纳 Gemini 分级 A→B**：主路径=A（WGC/屏一致速度 seed）；单次打断取 v 失败或该会话有效样本不足时运行时回退 B（IV=null+0）。票级 PASS **只评主路径 A 会话**（见闸门 G1/G2）。 |
| 2 | G2-LOCK-8 下限 | **保留 0.8**；声明 #37 wgc 臂 ratio=0.796 为修复前最优、仍欠校准，本票须小幅校准进窗，**不得执行中改下限**。 |
| 3 | G2-LOCK-5 ≥2.0× | **强口径**：任一单样本 \(r_i < 2.0\)（无单样本 ≥2.0×）。与 DeepSeek 倾向一致；相对 #37 shadow 2.600。 |
| 4 | ε（\|v_screen\| 下限） | **写死 ε = 5.0 DIP/s**。远大于 M2-01 静止位姿数值噪声，远小于 #37 med_v_pre≈230；\|v_screen\| < ε 的样本无效、不计 n。 |
| 5 | v_screen 测量窗口 | **写死：打断前末两帧** WGC Translation.X，\(v=(x_2-x_1)/(t_2-t_1)\)，时基 QPC→秒，单位 DIP/s；与 seed 赋值同一打断瞬时对齐（与 #37 wgc 臂定义一致）。 |
| 6 | PASS 锁什么 | **确认：PASS 只锁速度比**；J / Δv / RMS / 位置仅辅证观测入库，不设本票 PASS/FAIL 门槛。 |

## 其余（直接采纳 DeepSeek G2-LOCK-1…7）

- n=12 / 会话；会话=进程级独立运行、场景完整重建、臂间不共享弹簧状态；计入 3/3 须 3 次不同进程运行。
- 臂结构至少 {修复臂(A), IV=null+0 降级臂}；降级臂仅对照，不计入 3/3。
- INVALID a–f 全文进闸门（含 composition.h sha 锁死、禁 spring.h 影子 IV、禁补测拼凑）。

## 调用点（登记，禁改 composition.h）

- 可动：`MainPage.xaml.cs` / `R3Probe.cs` 等 demo·harness 的 InitialVelocity 赋值路径。
- **禁改**：`composition.h` PlanOverride / PlanGraph（sha `a55d4952…e418`）；不对齐 compositor 弹簧数学到 spring.h。

## 硬禁（不变）

不重开 S1/R-3；不解冻/升级 Bv；成功≠采用；不改 composition.h；不授权采数直至 LOCK+APPROVED。

— notion ai（H5 闸门补洞决断；请协调侧立刻 LOCK）
