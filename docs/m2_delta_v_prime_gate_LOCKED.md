# M2-Δv′ 门槛闸门（m2_delta_v_prime_gate_LOCKED.md）

STATUS=LOCKED
LOCKED_AT: 2026-10-26 09:05:37 AWST
GATE_BODY_SHA256: 06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c
> **本 LOCK 不授权采数。** 须 APPROVED_* 非空后，才可派 workbuddy。GATE_BODY_SHA256 计算约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。本行写入后不得再改 body。
> 补洞依据：`docs/notion_m2_delta_v_prime_gate_revise.md`；同伴稿 Gemini / DeepSeek（Harness 代拟已标）。
> 前序：M2-Δv = CLOSED INCONCLUSIVE（gate `d2349ceb0d37fdf332a4f00cb2c1f1217fe6856d0f3e5222ffaf82b8b15509c6` **不改**）；M2-PERF-CI HOLD `5b007dd4…282a`（本票**不**替代、**不**降级）。
> 升版点：可判几何 P1 `|a|σ≤thr`（去 /2）；P2 `thr≥Nv_h`（去 2×）；P4/P5 结局与 harness 旁路。

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
- 禁为过门调 seedLead / ε̂ / φ-估计器（视同重开 S1/R-3）；禁把 hang / c−φ / 影子当 seed 源；禁把 #50 `Δv_fd` 中位当本估计器期望；**禁为过门改 thr 公式、估计器阶数/w、改臂**。
- **P5 harness 旁路**：`IVel=0` 在 **t300dbl 上不进票**。负对照范围写死为 t060 fwd **与** t060 rev 均 Δv-FAIL。t300{fwd,rev,dbl} 上 IVel0 缺测 / 探针崩 **不构成** 票 INVALID，也不挡本票（崩属 harness 稳定旁路，另挂）。

## G1 臂 · 对照 · 格表

### 臂（写死）

| 角色 | 名称 | 定义 |
|---|---|---|
| **被测臂** | `Arm-WGC-seed` | H5 主路径 A：`InitialValue` 取 WGC/屏一致位置（或合成器当前值）；`InitialVelocity` 取打断前末两帧 WGC（DIP/s，QPC） |
| **正对照** | `base` | 同内容时刻**未打断**；同估计器；须在可判核心格达 Δv-PASS（噪声门）；可判后 base<17/20 → **ctrl FAIL** 挡票 PASS（**禁**放宽 17/20 或改 thr 救 base） |
| **负对照** | `IVel=0` | `InitialVelocity = 0`（记号：**IVel**，非 InitialValue）；在 t060 fwd **与** rev 须 Δv-FAIL；**非 PASS 候选** |

- `IV_NULL0`（InitialValue unset + IVel=0）**仅**可作负对照变体登记，**不得**计入被测臂 PASS。
- 闸门不另提名其他被测臂；改臂 = 新闸门版本。
- **G1 跑程注记**：允许 IVel0·t300dbl 提前结束；会话完整性以可判核心五格的 **base+WGC**（及 t060 的 IVel0）为准。崩后须 offline adjudicate；`IVel0_t060_both_FAIL` 每会话均为 True 方可认负对照成立。

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

`thr_i = max(20 DIP/s, 0.05 · |v_ref,i|)`，`v_ref,i = v_pre(t_s−)`（逐样本）。**去掉**相对臂 A / ≤0.25×A。**KEEP**（相对前序 Δv 不变）。

### G2-LOCK-2 估计器（采数前冻结）

- 每侧对邻接 `t_s` 的 **w=5** 帧做**二次**最小二乘，外推到 `t_s`。**KEEP**。
- `Δv = |v_post(t_s+) − v_pre(t_s−)|`。
- **禁止**两帧差分冒充。
- post 窗不得被切换后加速污染：只用 `t_s` 之后、且未跨越下一次目标变更的帧；不足 w 帧 → 该次试验无效。
- w / 阶数仅允许在 **base null** 上选定并预登记；采数后改 = 新闸门版本。

### G2-LOCK-3 t_s

- `t_s` = 切换窗 `W = [τ_{k_c−1}, τ_{k_c}]` 内使 `|x_post − x_pre|` 最小的时刻（时基 QPC；结合每试 φ）。
- 报告每试 `σ_ts`；`σ_ts` **只**用于安置测量窗与可判规则，不作 seed。
- **σ_ts 预登记（本机默认）**：均匀帧模型 `σ_ts = 1/F_refresh/√12`；60 Hz → `1/60/√12 ≈ 0.00481 s`。改公式 = 新闸门版本。
- dbl：第一次切换 → `t_s1`（本票不判 Δv）；第二次 → `t_s2`（本票 Δv 点）。

### G2-LOCK-4 Nv_h

- 在**未打断 base**、**同一估计器**、同一内容时刻、**打断臂开跑之前**测得。
- `Nv_h_cell` = 该格 base 上算得的 Δv 的 **P95**（DIP/s）。
- 样本欲计 Δv-PASS：该次 `Nv_h ≤ thr_i`（**只约束通过**；**KEEP** — 堵虚 PASS）。
- 若 Δv 的 CI 下界 > `thr_i + Nv_h` → **FAIL-确定**（不受可判豁免；**KEEP**）。

### G2-LOCK-5 可判规则（非标签）· **Δv′ 升版**

格可判 ⇔ 同时：

1. `thr ≥ Nv_h_cell`（**去 2×**；相对前序 `thr ≥ 2·Nv_h`）
2. `|a(t_s)| · σ_ts ≤ thr`（**去 /2**；相对前序 `≤ thr/2`）

（`a(t_s)`、`σ_ts` 来自 base / 同会话标定；参数预登记。）

任一条失败 → `Δv-SKIP`，报数字；**不得**计 Δv-PASS。  
- Nv_h_cell ≤ thr 的格可诚实进入可判；Nv_h_cell > thr 的格保持诚实 SKIP。  
- 预期：t156 仍常为规则导出 SKIP；t060 在本机数字下可进入可判；部分 t300 在 Nv_h≤20 时可判，高噪格 SKIP。  
- 若规则与命名冲突 → **再升闸门版本**，禁止硬改标签。

### G2-LOCK-6 试验有效性

有效试验须同时：G0 过；亮屏；无 capture 丢帧；`t_s` 检出；`v_ref` 可算；全程行程在 capture crop 内。  
hang 试验计无效并计数；单格·会话 hang 数 > 预登记 `hang_cap`（**默认提案 = 2**）→ 会话 INVALID（可替换，须留证）。禁止选择性丢 hang。  
**IVel0·t300dbl 崩不计 hang_cap / 不计 INVALID**（见 G0 P5）。

### G2-LOCK-7 格·会话 PASS

- 每格每会话 **n=20** 有效试验；
- ≥**17/20** 满足 `Δv_i ≤ thr_i` **且** 该样本 `Nv_h ≤ thr_i`；
- 报告 `r_i = Δv_i / thr_i`；
- **无**「中位 ≤ 阈中位」条款（不复活）。
- **禁**放宽 17/20。

### G2-LOCK-8 会话

- ≥**3** 独立进程会话；每会话完整重建；同进程复跑只计 1。
- **每会话均须**达票内要求；禁跨会话池化；FAIL 会话不得替换/重跑翻盘。
- INVALID 会话（G0/未亮屏/VM/超 hang_cap）可替换并留证。

### G2-LOCK-9 票结局

结局 ∈ {`Δv-PASS`, `Δv-FAIL`, `Δv-INCONCLUSIVE`}：

- **Δv-PASS**：在每一会话中，5 个可判核心格均按规则**可判**且格·会话 PASS；正对照 base PASS；负对照 IVel=0 在 t060 fwd 与 rev 均 FAIL。
- **Δv-FAIL**：任一会话存在**可判**核心格 Δv-FAIL（含 FAIL-确定；含可判后 WGC 差如 0–4/20）；或正对照 base 在可判核心未 PASS；或负对照 t060 未双 FAIL。FAIL 不重跑。
- **Δv-INCONCLUSIVE**：可判核心格**全部** SKIP（无法分胜负）；或协议/证据不足。**禁止**把「已可判但臂差」标成 INCONCLUSIVE 以回避 FAIL。

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
| `judicable` / `skip_reason` | 可判布尔与 SKIP 理由（P1/P2） |

### G2-LOCK-11 INVALID（任一即整票 INVALID）

1. 影子 IV / spring.h 闭式 v → compositor InitialVelocity  
2. composition.h sha ≠ `a55d4952…e418`  
3. 相对-A / ≤0.25×A 冒充绝对门  
4. VM / 非物理机 / 未亮屏强跑  
5. 本票产出写入 PERF-CI 合格证据或宣称 PERF-CI 单机 PASS  
6. 未 LOCK+APPROVED 采数；口径偏离（两帧差分、硬标签覆盖 SKIP、把 IVel=0 当 PASS、砍 dbl 仍报 5/5、删预登记改参、复活中位句、放宽 17/20 等）  
7. 重开 S1/R-3/R-2 或解冻 Bv；FAIL 后补测拼凑；选择性丢 hang  
8. 静默改前序 gate `d2349ceb…09c6` 或把本票产物冒充前序 Δv PASS

### G2-LOCK-12 预登记

采数前落盘并哈希：分析脚本、格表、w、可判规则参数（含本升版：`thr≥Nv_h`、`|a|σ≤thr`、`σ_ts` 公式、`hang_cap`）、阈公式、臂名单。  
`prereg_hash` 写入每会话产物。采后改任一参 = **新闸门版本**（不得原地改 LOCKED body）。

### G2-LOCK-13 辅证

- J_vis（#47 口径 `J_vis ≤ d_frame + N_h_eff`）对**所有格含 t156** 另表报告；**不**单独定本票生死。
- 本票不重开底噪门（M2-01 CLOSED）；不重开 M2-Δv 前序票（CLOSED INCONCLUSIVE 归档）。

## G3 产物

- `docs/m2_delta_v_prime_result.md`
- `artifacts/m2_delta_v_prime_*`（per-trial 行：φ, t_s, v_pre, v_post, thr, Δv, r, validity；SKIP 理由；对照结果；session meta；composition hash；prereg_hash）

## G4 ONELINE

M2-Δv′ | gate=<hash> @ <LOCKED_AT> | approved=<who>@<when> | sessions=3/3|… | cells=k/5 judicable|… | ctrl=baseP/IVel0F | ticket=<PASS/FAIL/INCONCLUSIVE/INVALID> | t156=rule-SKIP | P1=|a|σ≤thr;P2=thr≥Nv_h | 成功≠采用；不改composition.h；禁影子IV；≠PERF-CI；前序d2349ceb不改

## 禁令复述

不重开 S1/R-3/R-2；不改 composition.h；不解冻/升级 Bv；禁影子 IV；成功≠采用；旁路挂账不挡；PERF-CI 不降级；单机合法；不开 M3 真机；前序 `d2349ceb…09c6` 不改。

— notion ai（M2-Δv′ 可 LOCK 正文）
