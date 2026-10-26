# M2-Δv 门槛闸门（m2_delta_v_gate_DRAFT.md）

STATUS=NOT_LOCKED
DRAFT_AT: 2026-10-25 23:33:39 AWST
> **第二刀**（应协调 STOP；并入 Claude INTERIM）。本文件为可 LOCK 正文（NOT LOCKED）。**不授权采数。** 须 LOCKED + APPROVED_* 非空后，才可派 workbuddy。
> 补洞依据：`docs/notion_m2_delta_v_gate_revise.md`；同伴稿 Gemini / DeepSeek / Claude INTERIM。
> 前序：M2-H5-SEED CLOSED PASS；M2-PERF-CI HOLD `5b007dd4…282a`（本票**不**替代、**不**降级）。
> Claude Free→2026-10-26 16:20 AWST；INTERIM 已并入；明日 FINAL 仅对照。

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

**本票结论不解冻 B_v / 不重开 R-2 / 不重开 S1 / R-3；不改 composition.h。**

---

## G0 硬边界

- 不重开 S1 / R-3（CLOSED / STOP / NO-ADOPT）；不重开 R-2；**不为过本门新造 t156 专用估计器或重推 R-2′ 估计器**。
- 不解冻 / 不升级 B_v；成功 ≠ 采用；旁路挂账不挡、本票也不清旁路。
- 不改 `composition.h`（sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）；每会话前后校验 PRE=POST。
- **禁影子 InitialValue / 影子 IV**：禁止 `mf_spring_at` / spring.h 闭式 v 写入 compositor `InitialVelocity`。
- **本票 ≠ PERF-CI**：产物不得作 PERF-CI 合格证据；PERF-CI 仍须 ≥2 物理机（LOCKED `5b007dd4…282a` 仍 HOLD）。
- 机器：1 台物理机 DESKTOP-K93N3P8；**60 Hz**；亮屏；G0 过；VM/模拟器 = INVALID。
- 不开 M3 真机。
- 未 LOCKED+APPROVED 禁止采数。
- 禁为过门调 seedLead / ε̂ / φ-估计器（视同重开 S1/R-3）；禁把 hang / c−φ / 影子当 seed 源；禁把 #50 `Δv_fd` 中位当本估计器期望。

## G1 臂 · 对照 · 格表

### 臂（写死）

| 角色 | 名称 | 定义 |
|---|---|---|
| **被测臂** | `Arm-WGC-seed` | H5 主路径 A：`InitialValue` 取 WGC/屏一致位置（或合成器当前值）；`InitialVelocity` 取打断前末两帧 WGC（DIP/s，QPC） |
| **正对照** | `base` | 同内容时刻**未打断**；同估计器；须 Δv-PASS（噪声门） |
| **负对照** | `IVel=0` | `InitialVelocity = 0`（记号：**IVel**，非 InitialValue）；在 t060 fwd **与** rev 须 Δv-FAIL；**非 PASS 候选** |

- `IV_NULL0`（InitialValue unset + IVel=0）**仅**可作负对照变体登记，**不得**计入被测臂 PASS。
- 闸门不另提名其他被测臂；改臂 = 新闸门版本。

### 格表（写死）

| 格 | 方向 | 角色 | n/会话 | 说明 |
|---|---|---|---|---|
| t060 | fwd, rev | 可判核心（须过规则） | 20 有效 | |
| t156 | fwd, rev | **规则导出** `Δv-SKIP` 候选 | 20（辅证） | timing-bound；SKIP≠PASS；不挡票；禁硬标签覆盖规则 |
| t300 | fwd, rev, **dbl** | 可判核心 | 20 有效 | floor-bound；**dbl**：Δv/`v_ref` 取**第二次**切换 `t_s2` |

- **可判核心 = 5**：t060{fwd,rev} + t300{fwd,rev,dbl}。与 #47 四可判格差异 = **有意 +dbl**，非重开 #47。
- 其它格不得 SKIP。

## G2 主判据（写死）

### G2-LOCK-1 绝对阈

`thr_i = max(20 DIP/s, 0.05 · |v_ref,i|)`，`v_ref,i = v_pre(t_s−)`（逐样本）。**去掉**相对臂 A / ≤0.25×A。

### G2-LOCK-2 估计器（采数前冻结）

- 每侧对邻接 `t_s` 的 **w=5** 帧做**二次**最小二乘，外推到 `t_s`。
- `Δv = |v_post(t_s+) − v_pre(t_s−)|`。
- **禁止**两帧差分冒充。
- post 窗不得被切换后加速污染：只用 `t_s` 之后、且未跨越下一次目标变更的帧；不足 w 帧 → 该次试验无效。
- w / 阶数仅允许在 **base null** 上选定并预登记；采数后改 = 新闸门版本。

### G2-LOCK-3 t_s

- `t_s` = 切换窗 `W = [τ_{k_c−1}, τ_{k_c}]` 内使 `|x_post − x_pre|` 最小的时刻（时基 QPC；结合每试 φ）。
- 报告每试 `σ_ts`；`σ_ts` **只**用于安置测量窗与可判规则，不作 seed。
- dbl：第一次切换 → `t_s1`（本票不判 Δv）；第二次 → `t_s2`（本票 Δv 点）。

### G2-LOCK-4 Nv_h

- 在**未打断 base**、**同一估计器**、同一内容时刻、**打断臂开跑之前**测得。
- `Nv_h_cell` = 该格 base 上算得的 Δv 的 **P95**（DIP/s）。
- 样本欲计 Δv-PASS：该次 `Nv_h ≤ thr_i`（**只约束通过**）。
- 若 Δv 的 CI 下界 > `thr_i + Nv_h` → **FAIL-确定**（不受可判豁免）。

### G2-LOCK-5 可判规则（非标签）

格可判 ⇔ 同时：

1. `thr ≥ 2 · Nv_h_cell`
2. `|a(t_s)| · σ_ts ≤ thr / 2`（`a(t_s)`、`σ_ts` 来自 base / 同会话标定；参数预登记）

任一条失败 → `Δv-SKIP`，报数字；**不得**计 Δv-PASS。  
预期：t156 = timing-bound SKIP；t300 = floor-bound 可判。若规则与命名冲突 → **升闸门版本**，禁止硬改标签。

### G2-LOCK-6 试验有效性

有效试验须同时：G0 过；亮屏；无 capture 丢帧；`t_s` 检出；`v_ref` 可算；全程行程在 capture crop 内。  
hang 试验计无效并计数；单格·会话 hang 数 > 预登记 `hang_cap`（**默认提案 = 2**）→ 会话 INVALID（可替换，须留证）。禁止选择性丢 hang。

### G2-LOCK-7 格·会话 PASS

- 每格每会话 **n=20** 有效试验；
- ≥**17/20** 满足 `Δv_i ≤ thr_i`；
- 报告 `r_i = Δv_i / thr_i`；
- **无**「中位 ≤ 阈中位」条款。

### G2-LOCK-8 会话

- ≥**3** 独立进程会话；每会话完整重建；同进程复跑只计 1。
- **每会话均须**达票内要求；禁跨会话池化；FAIL 会话不得替换/重跑翻盘。
- INVALID 会话（G0/未亮屏/VM/超 hang_cap）可替换并留证。

### G2-LOCK-9 票结局

结局 ∈ {`Δv-PASS`, `Δv-FAIL`, `Δv-INCONCLUSIVE`}：

- **Δv-PASS**：在每一会话中，5 个可判核心格均按规则可判且格·会话 PASS；正对照 base PASS；负对照 IVel=0 在 t060 fwd 与 rev 均 FAIL。
- **Δv-FAIL**：任一会话或任一必判格 FAIL（含 FAIL-确定）；FAIL 不重跑。
- **Δv-INCONCLUSIVE**：必判格不可判、对照行为错误、或仅 t300 格过而 t060 未过等（按上列缺口）。

PASS ≠ 采用 ≠ PERF-CI 证据。

### G2-LOCK-10 H5 / seed 声明

每会话落盘：

| 字段 | 含义 |
|---|---|
| `arm` | `Arm-WGC-seed` / `base` / `IVel0` |
| `seed_path` | `WGC`（被测）或对照专用 |
| `calib` | 常数；H5 基线 1.0 |
| `v_wgc` / `ivel_set` | 实际 InitialVelocity 或 0 |
| `shadow_v_log` | 若记 spring.h 闭式 v → **log-only**，禁写入 compositor |
| `session_id` / `pid` / `hostname` | 独立性 |
| `prereg_hash` | 见 G2-LOCK-12 |

### G2-LOCK-11 INVALID（任一即整票 INVALID）

1. 影子 IV / spring.h 闭式 v → compositor InitialVelocity  
2. composition.h sha ≠ `a55d4952…e418`  
3. 相对-A / ≤0.25×A 冒充绝对门  
4. VM / 非物理机 / 未亮屏强跑  
5. 本票产出写入 PERF-CI 合格证据或宣称 PERF-CI 单机 PASS  
6. 未 LOCK+APPROVED 采数；口径偏离（两帧差分、硬标签覆盖 SKIP、把 IVel=0 当 PASS、砍 dbl 仍报 5/5、删预登记改参等）  
7. 重开 S1/R-3/R-2 或解冻 Bv；FAIL 后补测拼凑；选择性丢 hang  

### G2-LOCK-12 预登记

采数前落盘并哈希：分析脚本、格表、w、可判规则参数（含 `hang_cap`）、阈公式、臂名单。  
`prereg_hash` 写入每会话产物。采后改任一参 = **新闸门版本**（不得原地改 LOCKED body）。

### G2-LOCK-13 辅证

- J_vis（#47 口径 `J_vis ≤ d_frame + N_h_eff`）对**所有格含 t156** 另表报告；**不**单独定本票生死。
- 本票不重开底噪门（M2-01 CLOSED）。

## G3 产物

- `docs/m2_delta_v_result.md`
- `artifacts/m2_delta_v_*`（per-trial 行：φ, t_s, v_pre, v_post, thr, Δv, r, validity；SKIP 理由；对照结果；session meta；composition hash；prereg_hash）

## G4 ONELINE

M2-Δv | gate=<hash> @ <LOCKED_AT> | approved=<who>@<when> | sessions=3/3|… | cells=5/5|… | ctrl=baseP/IVel0F | ticket=<PASS/FAIL/INCONCLUSIVE/INVALID> | t156=rule-SKIP | 成功≠采用；不改composition.h；禁影子IV；≠PERF-CI

## 禁令复述

不重开 S1/R-3/R-2；不改 composition.h；不解冻/升级 Bv；禁影子 IV；成功≠采用；旁路挂账不挡；PERF-CI 不降级；单机合法；不开 M3 真机。

— notion ai（M2-Δv 可 LOCK 正文 · 并入 Claude INTERIM）
