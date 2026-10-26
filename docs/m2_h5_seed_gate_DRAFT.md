# M2-H5-SEED 门槛闸门（DRAFT · NOT LOCKED · 补洞后可 LOCK）

STATUS=DRAFT
DRAFT_AT: 2026-10-25 00:16:02 AWST
GATE_BODY_SHA256: （LOCK 时由协调侧填写；计算约定：对落盘全文做 SHA-256，排除以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希）
APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

**本 DRAFT 不授权采数。** 须 STATUS=LOCKED 且 APPROVED_* 非空后，才可派 workbuddy。
前序：M2-01 PASS；主因 H5 Seed 源（#37 shadow ratio=2.600）；修复不碰 composition.h。
补洞依据：`docs/notion_m2_h5_seed_gate_revise.md`（Notion 拍板）+ Gemini 分级推荐 + DeepSeek G2-LOCK 草案。

---

## G0 硬边界
- 不重开 S1 / R-3（CLOSED / STOP / NO-ADOPT）；不重开 R-2。
- 不解冻 / 不升级 B_v；B_v ≠ ship 基线。
- 不改 `composition.h`（sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）；PRE=POST 必须一致，任何改动即 INVALID。
- 成功 ≠ 采用；旁路挂账不挡本票，本票也不清旁路。
- 本票只改 seed 路径（demo / harness / 控制器契约层），**不对齐** compositor 弹簧数学到 spring.h；`composition.h` 内 PlanOverride / PlanGraph 禁改（只登记挂账）。
- 未 LOCKED+APPROVED 禁止采数。

## G1 目标与默认路径（G2-LOCK-0）
禁止以 spring.h 影子闭式速度（`mf_spring_at` / spring.h `v(t)`）作为 compositor `InitialVelocity`。

**默认路径 = 分级 A→B（写死）：**
- **主路径 A**：WGC / 屏一致速度 seed（单位 DIP/s，时基 QPC）。
- **运行时回退 B**：单次打断取 WGC 速度失败，或该会话有效样本不足时，该次/该会话回退 `IV=null + InitialVelocity=0`。
- **票级 PASS 只评主路径 A**：计入 3/3 的会话，其有效样本须来自 A 路径 seed；全程落在 B 的会话不计 PASS（可作对照登记）。
- 禁止 spring.h 影子闭式 v 作 compositor InitialVelocity。

可触及层（允许改 seed 赋值）：`MotionSoftDemo` / `win_softdemo_src` 的 `MainPage.xaml.cs`、`R3Probe.cs` 及相关 harness；控制器仅限 seed 契约/文档且不触 composition.h。

## G2 判据（写死）

### 探针与会话
- 场景：弹簧动画运动中途触发打断 / 反向重定向（沿用 #37 臂对照结构）。
- **G2-LOCK-1**：每会话有效打断次数 **n=12**；有效样本 = seed 与 v_screen 均可测得且 `|v_screen| ≥ ε`。
- **ε = 5.0 DIP/s**（写死）；`|v_screen| < ε` 样本无效、不计 n。
- **G2-LOCK-2**：会话 = 一次**进程级**独立运行、场景完整重建、臂间不共享弹簧状态；计入 3/3 的 3 个会话必须来自 **3 次不同进程运行**（同一进程重复 3 轮只计 1 个会话），且均针对修复主路径 A。
- **G2-LOCK-3**：至少臂结构 `{修复臂(A), IV=null+0 降级臂}`；降级臂仅对照登记，**不计入 3/3**（zero ratio=0.000、unset ratio=nan 均落在窗口外）。

### 指标口径
- **G2-LOCK-4**：逐样本 \(r_i = |v_{\mathrm{seed},i} / v_{\mathrm{screen},i}|\)，会话中位 \(\mathrm{med}_r\)。
- **v_screen 测量窗口（写死）**：打断前末两帧 WGC `Translation.X`，\(v=(x_2-x_1)/(t_2-t_1)\)，时基 QPC→秒，单位 DIP/s；与 seed 赋值对齐同一打断瞬时（与 #37 wgc 臂定义一致）。滞后补偿属实现细节，不冻结，但不得造成口径漂移。
- **G2-LOCK-8**：窗口下限 **保留 0.8**。声明：#37 wgc 臂 ratio=0.796 为修复前最优且略低于 0.8，本票 wgc 类 seed **须小幅校准**进窗；**不得在执行中改下限或改窗口**。

### PASS / FAIL / INVALID
- **G2-LOCK-5 PASS（强口径）**：同时满足  
  1. 3/3 会话 \(\mathrm{med}_r \in [0.8,\ 1.2]\)；  
  2. 任一单样本 \(r_i < 2.0\)（即**无单样本 ≥2.0×**）；  
  3. 相对 #37 shadow 臂 2.600 已消除。  
  **PASS 只锁速度比**；J_vis / Δv_vis / RMS / 位置仅辅证观测入库，**不设**本票 PASS/FAIL 门槛。
- **G2-LOCK-6 FAIL**：有效样本足（会话数 ≥3 且每会话有效 n ≥12）但未达 PASS。
- **G2-LOCK-7 INVALID**（任一即整票 INVALID，不允许补测拼凑）：  
  a. `composition.h` sha256 ≠ `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`；  
  b. 未授权采数（无 LOCKED+APPROVED）；  
  c. seed 仍来自 spring.h 影子闭式 v（赋值仍取 `mf_spring_at` / spring.h `v(t)` 作 compositor IV）；  
  d. 口径偏离本 G2（改中位为均值、改臂、改窗口、改分母/ε、改 ≥2.0× 强弱口径等）；  
  e. 会话数 <3 或任一计入会话有效 n <12；  
  f. FAIL 后追加补测回填拼凑。

## G3 禁令（结题必须重申）
不重开 S1/R-3；不改 composition.h；成功≠采用；不解冻/升级 Bv；不对齐 compositor 弹簧数学到 spring.h；旁路挂账不挡、本票也不清旁路。

## G4 ONELINE 模板
M2-H5-SEED | gate=<hash> @ <LOCKED_AT> | approved=<who>@<when> | ratio=<P/F/I> | ticket=<PASS/FAIL/INVALID> | 成功≠采用；不改composition.h；分级A→B；强口径无单样本≥2.0×
