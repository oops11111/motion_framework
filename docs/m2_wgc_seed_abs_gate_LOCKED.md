# M2-WGC-SEED-ABS 门槛闸门（m2_wgc_seed_abs_gate_LOCKED.md）

STATUS=LOCKED
LOCKED_AT: 2026-10-26 10:59:25 Asia/Taipei
GATE_BODY_SHA256: 831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de
> **本 LOCK 不授权采数。** 须 APPROVED_* 非空后，才可派 workbuddy。GATE_BODY_SHA256 计算约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。本行写入后不得再改 body。
> 补洞依据：`docs/notion_m2_wgc_seed_abs_gate_revise.md`；同伴稿 DeepSeek review + path（顶替 Gemini；pidan 已批）。
> 前序：M2-Δv′ = CLOSED FAIL（gate `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c` **不改、不重跑翻盘**）；H5-SEED CLOSED PASS；M2-PERF-CI HOLD `5b007dd4…282a`（本票 ≠ PERF-CI）。

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

**本票结论不解冻 B_v / 不重开 R-2 / 不重开 S1 / R-3；不改 composition.h。**
**本票成功 ≠ Δv-PASS ≠ 采用；结题标签只用 `SEED-ABS-*`。**

---

## G0 硬边界

- 不重开 S1 / R-3（CLOSED / STOP / NO-ADOPT）；不重开 R-2。
- 不解冻 / 不升级 B_v；成功 ≠ 采用；旁路挂账不挡、本票也不清旁路。
- 不改 `composition.h`（sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）；每会话 PRE=POST。
- **禁影子 IV**：禁止 `mf_spring_at` / spring.h 闭式 v 写入 compositor `InitialVelocity`；`shadow_v_log` 仅 log-only。
- **禁翻盘**：禁止重跑 / 改写 / 宣称通过前序 FAIL 闸门 `06467b82…805c`；禁止把本票产物标为 `Δv-PASS` / M2-Δv′ PASS。
- **本票 ≠ PERF-CI**；产物不得作 PERF-CI 合格证据；PERF-CI 仍 HOLD。
- 机器：1 台物理机 DESKTOP-K93N3P8；**60 Hz**；亮屏；VM = INVALID。
- 不开 M3 真机。
- 未 LOCKED+APPROVED 禁止采数。
- 禁为过门改 thr / 放宽 17/20 / 静默改 calib / 估窗偷换（打断后帧、异时基）。

## G1 路径（写死）· P-LS+TS

### 主路径 A（默认）

打断前末 **N=5** 帧 WGC/屏一致位置 `(t_k, x_k)`（Translation.X，QPC），做**二次**最小二乘，在 compositor 打断时刻 **`t_int`（QPC，与 WGC 同源）** 取  
`v̂ = b + 2c·t_int`，  
`InitialVelocity = (float)(calib · v̂)`，**calib = 1.0**。

| 参数 | 值 | 备注 |
|---|---|---|
| `N` | **5** | 与 Δv′ 测量 w=5 同宽；seed 窗 ≠ 改测量估计器 |
| `order` | **2** | 二次 |
| `t_int` | 探针/compositor 打断 QPC | P-TS：时戳同源对齐 |
| `calib` | **1.0** | 改须预登记 + 比窗证明；执行中改 = INVALID |
| `ε` | **5.0 DIP/s** | 继承 H5；`|v̂|<ε` → 回退 B |
| `P-VS` | **默认关** | 另开须预登记 `α_v` 且不破比窗 |
| 单位 | DIP/s | 禁止两帧差分冒充本默认（P-2F 仅基线对照） |

A 前提：有效帧 ≥5；拟合有限；时戳单调；`|v̂|≥ε`；取 WGC 成功。

### 回退 B

A 失败 → `IV=null + InitialVelocity=0`（path=B）。  
**票级 PASS 只评 A**；全程 B 的会话不计 PASS（可作对照登记）。

### 可触及层

允许改 seed 赋值：`MotionSoftDemo` / harness / 控制器 seed 契约；**不**触 `composition.h`；**不**把 spring.h 闭式 v 写入 compositor IV。

## G2 主判据

### G2-LOCK-1 H5 速度比回归（必要）

每计入 PASS 的会话须同时：

1. 报 `med_r` / `max_r` / 是否存在 `r_i≥2.0`（`r_i = |v_seed / v_screen|`；`v_screen` = 打断前末两帧 WGC，与 H5 分母一致）；
2. `med_r ∈ [0.8, 1.2]`；
3. **无**单样本 `r_i ≥ 2.0`。

比出窗 → 本票 FAIL/INVALID，**不得**以绝对 smoke 好看豁免。  
注：A=LS 后 `r` 相对末两帧分母**不必≡1**（预期）；可选另报同窗 LS 比作辅证，**不**替代本门槛。

### G2-LOCK-2 绝对 Δv smoke（本票主验收；缩小；非翻盘）

| 格 | 角色 | 要求 |
|---|---|---|
| t060 fwd | 必测核心 | 可判且格 PASS |
| t060 rev | 必测核心 | 可判且格 PASS |
| t300* | 可选辅证 | Nv_h>thr → SKIP 可接受；可判则入库 |
| t156 | 规则 SKIP | 不进 smoke |

**测量工具**（引用 Δv′ 几何作工具，**非**重跑其 LOCKED 闸门）：

- 估计器：每侧二次 LS **w=5** 外推到 `t_s`；`Δv=|v_post−v_pre|`；禁两帧差分冒充测量。
- 阈：`thr_i = max(20 DIP/s, 0.05·|v_ref,i|)`（**KEEP**；禁为过门改 thr）。
- 可判：`thr ≥ Nv_h_cell` ∧ `|a|·σ_ts ≤ thr`（σ 预登记默认 `1/60/√12`）。
- 格 PASS：n=20；≥**17/20** 满足 `Δv_i≤thr_i` **且** 该样本 `Nv_h≤thr_i`；FAIL-确定路径 KEEP；**无**中位句；**禁**放宽 17/20。

**基线**：相对 Δv′ 真机产物（t060 wgc_pass 0–2/20）→ 本票须 ≥17/20。可引用产物数字；**禁止**引用 gate `06467b82…805c` 作翻盘授权。

### G2-LOCK-3 对照

- **正对照 base**：smoke 格上 base 须 PASS（`basePass=True`）。
- **负对照 IVel=0**：仅 t060 fwd **与** rev 均 FAIL；t300* 上 IVel0 **不进票**（继承 P5）。

### G2-LOCK-4 会话

- ≥**3** 独立进程；每会话完整重建；同进程复跑只计 1。
- 禁跨会话池化；FAIL 会话不得替换翻盘。
- INVALID（G0/VM/未亮屏等）可替换并留证。

### G2-LOCK-5 票结局

结局 ∈ {`SEED-ABS-PASS`, `SEED-ABS-FAIL`, `SEED-ABS-INVALID`}：

- **PASS**：3/3 会话上 t060 fwd 与 rev 均格 PASS；basePass；IVel0_t060_both_FAIL；H5 比回归过；无影子 IV；composition.h 未改；路径声明为 A=P-LS+TS。
- **FAIL**：有效样本足但未达 PASS（含可判后臂差）。
- **INVALID**：见 G2-LOCK-6。

**禁止**写 `Δv-PASS` / 宣称 M2-Δv′ 翻盘。PASS ≠ 采用 ≠ PERF-CI ≠ 解冻 Bv。  
完整绝对门复测挂 **M2-Δv″**（须本票 CLOSED 后**另开新闸门**）。

### G2-LOCK-6 INVALID（任一即整票）

1. 影子 IV / spring.h → compositor IV  
2. composition.h sha ≠ `a55d4952…e418`  
3. 重跑/改写/宣称通过 `06467b82…805c`；产物标 `Δv-PASS`  
4. 未预登记改 calib / N / order / ε / 估窗；或估窗用打断后帧 / 异时基  
5. 为过门改 thr 或放宽 17/20；躲 t060 只报 SKIP 格  
6. 无 base / 无 IVel0 t060 双 FAIL 却宣称 PASS  
7. VM / 未亮屏 / 未 LOCK+APPROVED 采数；FAIL 后补测拼凑；跨会话池化  
8. 本票产物写入 PERF-CI 合格证据；开 M3 真机；解冻 Bv / 重开 S1/R-3/R-2  

### G2-LOCK-7 预登记与产物声明

采数前落盘并哈希：路径参数（N/order/t_int/calib/ε/P-VS）、smoke 格表、测量脚本、对照名单。  
每会话落盘：`seed_path`（A/B）、`calib`、`v_wgc`/`iv_set`、`shadow_v_log`（若有）、`med_r`/`max_r`、smoke 格结果、`prereg_hash`、`session_id`/`pid`、composition hash。

## G3 产物

- `docs/m2_wgc_seed_abs_result.md`
- `artifacts/m2_wgc_seed_abs_*`（**不得**冒充 `m2_delta_v_prime_*` 翻盘包）

## G4 ONELINE

M2-WGC-SEED-ABS | gate=<hash> @ <LOCKED_AT> | approved=<who>@<when> | path=P-LS+TS/N=5 | smoke=t060 | ratio=reg | ctrl=baseP/IVel0F | ticket=<SEED-ABS-PASS/FAIL/INVALID> | 成功≠采用≠Δv-PASS；不改composition.h；禁影子IV；≠PERF-CI；06467b82不翻盘

## 禁令复述

不重开 S1/R-3/R-2；不改 composition.h；不解冻/升级 Bv；禁影子 IV；成功≠采用；禁翻盘 `06467b82…805c`；PERF-CI 不降级；不开 M3 真机。

— notion ai（M2-WGC-SEED-ABS 可 LOCK 正文）
