# M2-WGC-SEED-PATH-B′ 门槛闸门（m2_wgc_seed_path_b_prime_gate_DRAFT.md）

STATUS=NOT_LOCKED（可 LOCK 正文；正式 LOCKED 见 m2_wgc_seed_path_b_prime_gate_LOCKED.md）
LOCKED_AT: 2026-10-26 13:49:20 Asia/Taipei
GATE_BODY_SHA256: （见 LOCKED `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`；本 DRAFT 不单独作权威 hash）
> **本 DRAFT/LOCK 正文不授权采数。** 须 APPROVED_* 非空后，才可派 workbuddy。GATE_BODY_SHA256 计算约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。本行写入后不得再改 body。
> 补洞依据：`docs/notion_m2_wgc_seed_path_b_prime_gate_revise.md`；同伴稿 `docs/deepseek_m2_wgc_seed_path_b_prime_design.md`（Harness 空回代拟；判可 LOCK）。
> 前序闸门**不改、不重跑翻盘**：  
> - SEED-CALIB FAIL `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d` @ 2026-10-26 12:03:33 Asia/Taipei  
> - SEED-ABS FAIL `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`  
> - Δv′ FAIL `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`  
> H5-SEED CLOSED PASS；M2-PERF-CI HOLD `5b007dd4…282a`（本票 ≠ PERF-CI）。  
> 触发：M2-WGC-SEED-CALIB = CLOSED SEED-CALIB-FAIL；PATH_B_PRIME=HUNG（G2-LOCK-2b）；result `docs/m2_wgc_seed_calib_result.md`；结题包 `docs/notion_m2_wgc_seed_calib_pack.md`；下一票 `docs/notion_m2_wgc_seed_calib_next.md`。

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

**本票结论不解冻 B_v / 不重开 R-2 / 不重开 S1 / R-3；不改 composition.h。**  
**本票成功 ≠ Δv-PASS ≠ SEED-ABS-PASS ≠ SEED-CALIB-PASS ≠ 采用；结题标签只用 `SEED-PATHB-*`。**

---

## G0 硬边界

- 不重开 S1 / R-3 / R-2；不解冻 / 不升级 B_v；成功 ≠ 采用；旁路挂账不挡、本票也不清旁路。
- 不改 `composition.h`（sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）；每会话 PRE=POST。
- **禁影子 IV**：禁止 `mf_spring_at` / spring.h 闭式 v 写入 compositor `InitialVelocity`；`shadow_v_log` 仅 log-only。
- **禁翻盘**：禁止重跑 / 改写 / 宣称通过  
  - SEED-CALIB FAIL `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`  
  - SEED-ABS FAIL `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`  
  - Δv′ FAIL `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`  
  禁止把本票产物标为 `Δv-PASS` / `SEED-ABS-PASS` / `SEED-CALIB-PASS` / 前序翻盘。
- **本票 ≠ PERF-CI**；产物不得作 PERF-CI 合格证据；PERF-CI 仍 HOLD。
- 机器：1 台物理机 DESKTOP-K93N3P8；**60 Hz**；亮屏；VM = INVALID。
- 不开 M3 真机。
- 未 LOCKED+APPROVED 禁止采数。
- **禁**为过门改 thr / 放宽 17/20 / 偷换估窗（打断后帧、异时基伪装）。
- **禁**只修 fwd（或只修 rev）却放宽另一侧门槛。
- **禁**默认改 N/order；**禁**V4 单侧作默认；**禁**再标定 / 废止 calib exact；**禁**立刻开 Δv″。

## G1 路径 · 默认 A = PATHB-A = V2-clamp

### 主路径 A（默认 · 本票 PASS 只评 A）

打断前末 **N=5** 帧 WGC/屏一致位置 `(t_k, x_k)`（Translation.X，QPC），做**二次**最小二乘，在估点 **`t_int`（QPC）** 取  
`v̂ = b + 2c·t_int`，  
`InitialVelocity = (float)(calib · v̂)`。

| 参数 | 值 | 备注 |
|---|---|---|
| 族 | **P-LS+TS** | KEEP |
| 变体 ID | **`PATHB-A = V2-clamp`** | 本票默认主路径 |
| `N` | **5** | KEEP；**禁**本票改 N |
| `order` | **2** | KEEP；**禁**本票改 order |
| `calib` | **0.7511**（exact=`0.7510829456`） | **KEEP**；比较/写入用 exact；显示四位；执行中改 = INVALID；**禁**再标定 |
| `ε` | **5.0 DIP/s** | KEEP；`|v̂|<ε` → 回退 B |
| `P-VS` | **OFF** | 主路径 A 默认关 |
| 单位 | DIP/s | 禁止两帧差分冒充本默认 |
| **`t_int`（相对 CALIB 的核心 diff）** | **`t_int = min(t_compositor_interrupt_QPC, t_last_WGC)`** | `t_last_WGC` = 打断前最后一帧单调 WGC 样点 QPC；**禁**外推超过 `t_last_WGC`；若 `t_compositor > t_last_WGC`，钳制到末样（等价于在末样求速） |
| 时基 | 仅 compositor / WGC **同一 QPC 原点** | **禁**异时基 |
| 估窗帧 | 仅打断前末 N 帧 | **禁**打断后帧 |

A 前提：有效帧 ≥5；拟合有限；时戳单调；`|v̂|≥ε`；取 WGC 成功；`t_int` 已按上式钳制。

### 回退 B

A 失败 → `IV=null + InitialVelocity=0`（path=B）。  
**票级 PASS 只评默认 A**；全程 B 的会话不计 PASS。

### A′ 挂账次选（本票 LOCK **不**默认同跑第二臂）

| 项 | 预登记值 |
|---|---|
| 变体 ID | **`PATHB-A′ = V2-clamp + V3-VS`** |
| 继承 A | N/order/calib/ε/`t_int` 钳制 **全部同 A** |
| `P-VS` | **ON** |
| `α_v` | **0.30**（`v_s = α_v·v̂ + (1−α_v)·v_s_prev`；首样 `v_s=v̂`） |
| 限幅 `v_max` | **不设** |
| 写入 IV | `InitialVelocity=(float)(calib·v_s)`（仍 KEEP calib） |

**A′ 定位**：挂账次选；抑 rev 侧 `v_post` 上冲尖峰。启用须 **Notion 显式 APPROVED 第二臂或 A 结题后另票**；**禁止**本票默认同跑；**禁止**执行中把 A 偷换成 A′ 或改 α。

### 相对 CALIB 的 diff

| 参数 | CALIB（CLOSED FAIL） | PATH-B′ **A** | PATH-B′ **A′**（挂账） |
|---|---|---|---|
| 族 | P-LS+TS | 同 | 同 |
| N / order | 5 / 2 | **KEEP** | **KEEP** |
| calib | exact 0.7510829456 | **KEEP** | **KEEP** |
| ε | 5.0 | KEEP | KEEP |
| P-VS | OFF | **OFF** | ON · α_v=0.30 |
| t_int | compositor QPC（可外推过末样） | **钳制 `min(t_comp, t_last_WGC)`，禁过后外推** | 同 A |
| 估窗帧 | 打断前 N=5 | KEEP；显式禁打断后帧 | 同 A |
| 测量 / thr / 17/20 | w=5；max(20,5%\|v_ref\|)；≥17/20 | **KEEP（禁改）** | KEEP |
| 结题标签 | SEED-CALIB-* | **SEED-PATHB-*** | SEED-PATHB-* |

### 否决（不得作本票默认）

| 候选 | 决断 |
|---|---|
| 默认改 N∈{3,7} / order=1 | **否**（KEEP calib 下极易拧出比窗） |
| V3 P-VS 作默认主路径 | **否**（降为 A′ 挂账） |
| V4 分方向估法作默认 | **否**（fwd 亦远未过；易滑向单侧放宽） |
| 再标定 / 废止 exact | **否**（med_r 已 ≈1.00） |
| 立刻 Δv″ | **否**（比过但 smoke 未齐则继续挂账） |
| 打断后帧 / 异时基 / 影子 IV / 改 composition.h / 抬 thr / 降 17/20 | **禁** → INVALID |

### 结构风险（写死进正文）

KEEP calib + H5 分母=2F `v_screen` 下，CALIB 已见 `|ivel−v_pre|≈50` DIP/s；幅度缺口**无法靠标量再抹平**。A/A′ 只赌相位/尖峰；**smoke 仍 FAIL 合法**——**禁抬 thr / 降 17/20 / 偷换估窗**；不得因此宣称虚 PASS。

### 可触及层

允许改 seed 估点钳制（`t_int`）与 prereg：`MotionSoftDemo` / harness / 控制器 seed 契约；**不**触 `composition.h`；**不**把 spring.h 闭式 v 写入 compositor IV；**不**改 N/order/ε/calib（KEEP）；**不**默认开 P-VS。

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

### G2-LOCK-2b 再挂账注记（写死）

若本票 **H5 比回归过**，但 **t060 fwd 与 rev smoke 仍双灭**（两侧均未达 ≥17/20）→  
1. 本票结局 = **`SEED-PATHB-FAIL`**（不得标 PASS）；  
2. **挂账 `SEED-TRUTH-CONFLICT`**（名可微调）：2F 比真值 vs 绝对 `v_pre` 冲突；**禁止**再开 calib 废止票当主线；**禁止**抬 thr；**不**预授权改 H5 分母；  
3. 本票**禁止**因此放宽 thr / 17/20 / 估窗 / 改 N·order·ε·calib。  
若比过但仅单侧 smoke FAIL → 仍为本票 **FAIL**；Δv″ **不**立刻开。  
offline 预测不得宣称 smoke PASS。

### G2-LOCK-3 对照

- 正对照 base：smoke 格上 `basePass=True`  
- 负对照 IVel0：仅 t060 fwd **与** rev 均 FAIL（t300* IVel0 不进票）

### G2-LOCK-4 会话

≥**3** 独立进程；禁跨会话池化；FAIL 不得替换翻盘；INVALID 可替换并留证。

### G2-LOCK-5 票结局

结局 ∈ {`SEED-PATHB-PASS`, `SEED-PATHB-FAIL`, `SEED-PATHB-INVALID`}：

- **PASS**：3/3 会话 t060 fwd+rev 格 PASS；basePass；IVel0_t060_both_FAIL；H5 比回归过；无影子 IV；composition.h 未改；路径=默认 A=`PATHB-A=V2-clamp`；calib=预登记 exact（显示 0.7511）；`t_int` 钳制已落实；P-VS=OFF。  
- **FAIL**：有效样本足但未达 PASS（含：比过但 smoke 仍灭 → 见 G2-LOCK-2b）。  
- **INVALID**：见 G2-LOCK-6。

**禁止**写 `Δv-PASS` / `SEED-ABS-PASS` / `SEED-CALIB-PASS` / 宣称翻盘。PASS ≠ 采用 ≠ PERF-CI ≠ 解冻 Bv。  
完整绝对门复测仍挂 **M2-Δv″**（须本票 **比回归过 ∧ t060 fwd+rev smoke 格 PASS** 后另开新闸门；**不立刻开**）。  
A′ 继续挂账次选（不默认同跑）。

### G2-LOCK-6 INVALID（任一即整票）

1. 影子 IV / spring.h → compositor IV  
2. composition.h sha ≠ `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`  
3. 重跑/改写/宣称通过 `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d` / `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de` / `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`；产物标 `Δv-PASS` / `SEED-ABS-PASS` / `SEED-CALIB-PASS`  
4. 未预登记改 N/order/ε/calib/估窗/P-VS/`t_int`；或估窗用打断后帧 / 异时基；外推超过 `t_last_WGC`；执行中改 KEEP 常量；执行中把 A 偷换成 A′  
5. 为过门改 thr 或放宽 17/20；躲 t060；只修一侧却放宽另一侧门槛；V4 单侧作默认却宣称本票 PASS  
6. 无 base / 无 IVel0 t060 双 FAIL 却宣称 PASS  
7. VM / 未亮屏 / 未 LOCK+APPROVED 采数；FAIL 后补测拼凑；跨会话池化；offline 预测当结题  
8. 本票产物写入 PERF-CI 合格证据；开 M3；解冻 Bv / 重开 S1/R-3/R-2；立刻开 Δv″（比/smoke 未齐）

### G2-LOCK-7 预登记与产物声明

采数前落盘并哈希：路径参数（变体 ID=`PATHB-A`；N/order/`t_int` 钳制/calib exact=`0.7510829456`/ε/P-VS=OFF）、smoke 格表、测量脚本、对照名单。  
每会话落盘：`seed_path`（A/B）、`pathb_variant`、`calib`、`t_int`/`t_last_WGC`、`v_wgc`/`iv_set`、`shadow_v_log`（若有）、`med_r`/`max_r`、smoke 格结果、`prereg_hash`、`session_id`/`pid`、composition hash。

## G3 产物

- `docs/m2_wgc_seed_path_b_prime_result.md`
- `artifacts/m2_wgc_seed_path_b_prime_*`（或 `m2_wgc_seed_pathb_*`；**不得**冒充 `m2_wgc_seed_calib_*` / `m2_wgc_seed_abs_*` / `m2_delta_v_prime_*` 翻盘包）

## G4 ONELINE

M2-WGC-SEED-PATH-B′ | gate=<hash> @ <LOCKED_AT> | approved=<who>@<when> | path=PATHB-A=V2-clamp/N=5/t_int=min(t_comp,t_last_WGC) | calib=0.7511(exact=0.7510829456 KEEP) | P-VS=OFF | smoke=t060 | ratio=reg | ctrl=baseP/IVel0F | ticket=<SEED-PATHB-PASS/FAIL/INVALID> | 成功≠采用≠Δv-PASS；不改composition.h；禁影子IV；≠PERF-CI；09f6414b/831818/06467b82不翻盘；A′挂账；Δv″继续挂账；结构风险见G1；SEED-TRUTH-CONFLICT见G2-LOCK-2b

## 禁令复述

不重开 S1/R-3/R-2；不改 composition.h；不解冻/升级 Bv；禁影子 IV；成功≠采用；禁翻盘三 FAIL gate `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d` / `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de` / `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`；结题只用 `SEED-PATHB-*`；禁标 `Δv-PASS` / `SEED-ABS-PASS` / `SEED-CALIB-PASS`；PERF-CI 不降级；不开 M3 真机；禁抬 thr / 降 17/20 / 偷换估窗；禁单侧放宽；禁默认改 N/order；禁再标定；禁立刻 Δv″；A′ 挂账次选不默认同跑；结构风险下 FAIL 合法；Δv″ 与 SEED-TRUTH-CONFLICT 挂账条件见正文。

— notion ai（M2-WGC-SEED-PATH-B′ DRAFT 对齐 LOCKED；正式权威见 LOCKED；不授权采数）
