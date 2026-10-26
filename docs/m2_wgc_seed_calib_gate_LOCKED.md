# M2-WGC-SEED-CALIB 门槛闸门（m2_wgc_seed_calib_gate_LOCKED.md）

STATUS=LOCKED
LOCKED_AT: 2026-10-26 12:03:33 Asia/Taipei
GATE_BODY_SHA256: 09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d
> **本 LOCK 不授权采数。** 须 APPROVED_* 非空后，才可派 workbuddy。GATE_BODY_SHA256 计算约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。本行写入后不得再改 body。
> 补洞依据：`docs/notion_m2_wgc_seed_calib_gate_revise.md`；同伴稿 `docs/deepseek_m2_wgc_seed_calib_review.md`（判可 LOCK）。
> 前序：M2-WGC-SEED-ABS = CLOSED SEED-ABS-FAIL（gate `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de` **不改、不重跑翻盘**）；M2-Δv′ = CLOSED FAIL（gate `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c` **不改、不重跑翻盘**）；H5-SEED CLOSED PASS；M2-PERF-CI HOLD `5b007dd4…282a`（本票 ≠ PERF-CI）。

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

**本票结论不解冻 B_v / 不重开 R-2 / 不重开 S1 / R-3；不改 composition.h。**  
**本票成功 ≠ Δv-PASS ≠ SEED-ABS-PASS ≠ 采用；结题标签只用 `SEED-CALIB-*`。**

---

## G0 硬边界

- 不重开 S1 / R-3 / R-2；不解冻 / 不升级 B_v；成功 ≠ 采用；旁路挂账不挡、本票也不清旁路。
- 不改 `composition.h`（sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）；每会话 PRE=POST。
- **禁影子 IV**：禁止 `mf_spring_at` / spring.h 闭式 v 写入 compositor `InitialVelocity`；`shadow_v_log` 仅 log-only。
- **禁翻盘**：禁止重跑 / 改写 / 宣称通过  
  - 本前序 SEED-ABS FAIL 闸门 `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`  
  - 前序 Δv′ FAIL 闸门 `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`  
  禁止把本票产物标为 `Δv-PASS` / `SEED-ABS-PASS` / M2-Δv′ PASS。
- **本票 ≠ PERF-CI**；产物不得作 PERF-CI 合格证据；PERF-CI 仍 HOLD。
- 机器：1 台物理机 DESKTOP-K93N3P8；**60 Hz**；亮屏；VM = INVALID。
- 不开 M3 真机。
- 未 LOCKED+APPROVED 禁止采数。
- **禁**为过门改 thr / 放宽 17/20 / 偷换估窗（打断后帧、异时基、改 N/order 伪装）。

## G1 路径（KEEP 族）· P-LS+TS + 标定 calib

### 主路径 A（默认）

打断前末 **N=5** 帧 WGC/屏一致位置 `(t_k, x_k)`（Translation.X，QPC），做**二次**最小二乘，在 compositor 打断时刻 **`t_int`（QPC）** 取  
`v̂ = b + 2c·t_int`，  
`InitialVelocity = (float)(calib · v̂)`。

| 参数 | 值 | 备注 |
|---|---|---|
| `N` | **5** | KEEP；与测量 w=5 同宽；**禁**本票改 N |
| `order` | **2** | KEEP |
| `t_int` | 探针/compositor 打断 QPC | P-TS KEEP |
| `calib` | **0.7511**（exact=`0.7510829456`） | 废止 1.0；比较/写入用 exact；显示四位；执行中改 = INVALID |
| `ε` | **5.0 DIP/s** | KEEP；`|v̂|<ε` → 回退 B |
| `P-VS` | **默认关** | KEEP |
| 单位 | DIP/s | 禁止两帧差分冒充本默认 |

A 前提：有效帧 ≥5；拟合有限；时戳单调；`|v̂|≥ε`；取 WGC 成功。

### calib 预登记（本票唯一允许变更点 · 已写死）

- **显示值**：`0.7511`  
- **exact（prereg / 执行浮点常量；比较与写入用）**：`0.7510829456`  
- **来源**：M2-WGC-SEED-ABS 三会话 pathA（`arm=Arm-WGC-seed` ∧ `seed_path=A` ∧ `valid=True`）**n=120**  
- **公式**：`calib = median(v_screen / ivel_set)`（≡ `median(v_screen / v_seed)`；pathA 下 `ivel_set` 与 `v_hat` 一致）  
- **废止**：`1.0`（ABS 已证 → med_r≈1.33）  
- **禁止**：抬 thr；降 17/20；改 N/order/ε/估窗；用影子 IV；执行中改 calib；分方向/分会话拼凑 calib（属 PATH-B′，超出本票）。

### 回退 B

A 失败 → `IV=null + InitialVelocity=0`（path=B）。  
**票级 PASS 只评 A**；全程 B 的会话不计 PASS。

### 可触及层

允许改 seed 赋值处的 **calib 常量**（及 prereg 声明）：`MotionSoftDemo` / harness / 控制器 seed 契约；**不**触 `composition.h`；**不**把 spring.h 闭式 v 写入 compositor IV；**不**改 N/order/ε/估窗/P-VS。

## G2 主判据

### G2-LOCK-1 H5 速度比回归（必要）

计入 PASS 的会话须：`med_r ∈ [0.8,1.2]`；无 `r_i≥2.0`（`r_i=|v_seed/v_screen|`；`v_screen`=打断前末两帧 WGC）。  
比出窗 → FAIL/INVALID，**不得**以 smoke 豁免。

### G2-LOCK-2 绝对 Δv smoke

| 格 | 角色 | 要求 |
|---|---|---|
| t060 fwd | 必测核心 | 可判且格 PASS |
| t060 rev | 必测核心 | 可判且格 PASS |
| t300* | 可选辅证 | Nv_h>thr → SKIP 可接受 |
| t156 | 规则 SKIP | 不进 smoke |

测量工具（引用 Δv′ 几何；**非翻盘**）：二次 LS **w=5**；`thr_i=max(20,0.05·|v_ref,i|)`；可判 `thr≥Nv_h ∧ |a|·σ≤thr`；格 PASS ≥**17/20** 且样本 `Nv_h≤thr_i`。  
**禁**为过门改 thr / 17/20。

### G2-LOCK-2b PATH-B′ 触发注记（写死）

若本票 **H5 比回归过**，但 **t060 rev（或单侧）smoke 仍 FAIL**（未达 ≥17/20）→  
1. 本票结局 = **`SEED-CALIB-FAIL`**（不得标 PASS）；  
2. **挂账 PATH-B′**（N/TS/P-VS / 分方向估法另开新闸门）；  
3. 本票**禁止**因此放宽 thr / 17/20 / 估窗 / 改 N·order·ε。  
offline 预测不得宣称 smoke PASS。

### G2-LOCK-3 对照

- 正对照 base：smoke 格上 `basePass=True`  
- 负对照 IVel0：仅 t060 fwd **与** rev 均 FAIL（t300* IVel0 不进票）

### G2-LOCK-4 会话

≥**3** 独立进程；禁跨会话池化；FAIL 不得替换翻盘；INVALID 可替换并留证。

### G2-LOCK-5 票结局

结局 ∈ {`SEED-CALIB-PASS`, `SEED-CALIB-FAIL`, `SEED-CALIB-INVALID`}：

- **PASS**：3/3 会话 t060 fwd+rev 格 PASS；basePass；IVel0_t060_both_FAIL；H5 比回归过；无影子 IV；composition.h 未改；路径 A=P-LS+TS；calib=预登记 exact（显示 0.7511）。  
- **FAIL**：有效样本足但未达 PASS（含：比过但 rev/单侧 smoke 仍 FAIL → 触发 PATH-B′ 挂账）。  
- **INVALID**：见 G2-LOCK-6。

**禁止**写 `Δv-PASS` / `SEED-ABS-PASS` / 宣称翻盘。PASS ≠ 采用 ≠ PERF-CI ≠ 解冻 Bv。  
完整绝对门复测仍挂 **M2-Δv″**（须本票 CLOSED 后视情况另开新闸门）。

### G2-LOCK-6 INVALID（任一即整票）

1. 影子 IV / spring.h → compositor IV  
2. composition.h sha ≠ `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`  
3. 重跑/改写/宣称通过 `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de` 或 `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`；产物标 `Δv-PASS` / `SEED-ABS-PASS`  
4. 未预登记改 calib / N / order / ε / 估窗；或估窗用打断后帧 / 异时基；执行中改 calib  
5. 为过门改 thr 或放宽 17/20；躲 t060（只跑 fwd / 只报 SKIP 格）  
6. 无 base / 无 IVel0 t060 双 FAIL 却宣称 PASS  
7. VM / 未亮屏 / 未 LOCK+APPROVED 采数；FAIL 后补测拼凑；跨会话池化；offline 预测当结题  
8. 本票产物写入 PERF-CI 合格证据；开 M3；解冻 Bv / 重开 S1/R-3/R-2  

### G2-LOCK-7 预登记与产物声明

采数前落盘并哈希：路径参数（N/order/t_int/**calib exact=0.7510829456**/ε/P-VS）、smoke 格表、测量脚本、对照名单。  
每会话落盘：`seed_path`（A/B）、`calib`、`v_wgc`/`iv_set`、`shadow_v_log`（若有）、`med_r`/`max_r`、smoke 格结果、`prereg_hash`、`session_id`/`pid`、composition hash。

## G3 产物

- `docs/m2_wgc_seed_calib_result.md`
- `artifacts/m2_wgc_seed_calib_*`（**不得**冒充 `m2_wgc_seed_abs_*` / `m2_delta_v_prime_*` 翻盘包）

## G4 ONELINE

M2-WGC-SEED-CALIB | gate=<hash> @ <LOCKED_AT> | approved=<who>@<when> | path=P-LS+TS/N=5 | calib=0.7511(exact=0.7510829456) | smoke=t060 | ratio=reg | ctrl=baseP/IVel0F | ticket=<SEED-CALIB-PASS/FAIL/INVALID> | 成功≠采用≠Δv-PASS；不改composition.h；禁影子IV；≠PERF-CI；831818/06467b82不翻盘；PATH-B′挂账触发见G2-LOCK-2b

## 禁令复述

不重开 S1/R-3/R-2；不改 composition.h；不解冻/升级 Bv；禁影子 IV；成功≠采用；禁翻盘 `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de` 与 `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`；结题只用 `SEED-CALIB-*`；禁标 `Δv-PASS` / `SEED-ABS-PASS`；PERF-CI 不降级；不开 M3 真机；禁抬 thr / 降 17/20 / 偷换估窗；Δv″ 与 PATH-B′ 挂账。

— notion ai（M2-WGC-SEED-CALIB LOCKED；不授权采数）
