# M2-WGC-SEED-PATH-A′ 门槛闸门（m2_wgc_seed_path_a_prime_gate_LOCKED.md）

STATUS=LOCKED
LOCKED_AT: 2026-10-26 14:38:15 Asia/Taipei
GATE_BODY_SHA256: f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362
> **本 LOCK 不授权采数。** 须 APPROVED_* 非空后，才可派 workbuddy。GATE_BODY_SHA256 计算约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。本行写入后不得再改 body。
> 补洞依据：`docs/notion_m2_wgc_seed_path_a_prime_gate_revise.md`；同伴稿 `docs/deepseek_m2_wgc_seed_path_a_prime_review.md`（Harness 空回代拟；判可 LOCK；无硬缺口）。
> 前序闸门**不改、不重跑翻盘**：  
> - PATH-B′ FAIL `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43` @ 2026-10-26 13:49:20 Asia/Taipei  
> - SEED-CALIB FAIL `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`  
> - SEED-ABS FAIL `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`  
> - Δv′ FAIL `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`  
> H5-SEED CLOSED PASS；M2-PERF-CI HOLD `5b007dd4…282a`（本票 ≠ PERF-CI）。  
> 触发：M2-WGC-SEED-PATH-B′ = CLOSED SEED-PATHB-FAIL；SEED-TRUTH-CONFLICT=HUNG（G2-LOCK-2b）；result `docs/m2_wgc_seed_path_b_prime_result.md`；结题包 `docs/notion_m2_wgc_seed_path_b_prime_pack.md`；下一票 `docs/notion_m2_wgc_seed_path_b_prime_next.md`。  
> **本票 = 显式另票**启用 PATH-B′ 闸门预登记之 A′；**禁止**默认同跑偷换 PATH-B′ FAIL。

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

**本票结论不解冻 B_v / 不重开 R-2 / 不重开 S1 / R-3；不改 composition.h。**  
**本票成功 ≠ Δv-PASS ≠ SEED-ABS-PASS ≠ SEED-CALIB-PASS ≠ SEED-PATHB-PASS ≠ 采用；结题标签主用 `SEED-PATHA-PRIME-*`（若留别名 `SEED-PATHB-APRIME-*` 须声明禁止写作 `SEED-PATHB-PASS`）。**

---

## G0 硬边界

- 不重开 S1 / R-3 / R-2；不解冻 / 不升级 B_v；成功 ≠ 采用；旁路挂账不挡、本票也不清旁路。
- 不改 `composition.h`（sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）；每会话 PRE=POST。
- **禁影子 IV**：禁止 `mf_spring_at` / spring.h 闭式 v 写入 compositor `InitialVelocity`；`shadow_v_log` 仅 log-only。
- **禁翻盘**：禁止重跑 / 改写 / 宣称通过  
  - PATH-B′ FAIL `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`  
  - SEED-CALIB FAIL `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`  
  - SEED-ABS FAIL `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`  
  - Δv′ FAIL `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`  
  禁止把本票产物标为 `Δv-PASS` / `SEED-ABS-PASS` / `SEED-CALIB-PASS` / `SEED-PATHB-PASS` / 前序翻盘。
- **本票 ≠ PERF-CI**；产物不得作 PERF-CI 合格证据；PERF-CI 仍 HOLD。
- 机器：1 台物理机 DESKTOP-K93N3P8；**60 Hz**；亮屏；VM = INVALID。
- 不开 M3 真机。
- 未 LOCKED+APPROVED 禁止采数。
- **禁**为过门改 thr / 放宽 17/20 / 偷换估窗（打断后帧、异时基伪装）。
- **禁**只修 fwd（或只修 rev）却放宽另一侧门槛。
- **禁**改 N/order；**禁**V4 单侧作默认；**禁**再标定 / 废止 calib exact；**禁**立刻开 Δv″。
- **禁**本票预授权改 H5 分母；**禁**把本票当 PATH-B′ 第二臂默认同跑。

## G1 路径 · 默认主路径 = PATHB-A′ = V2-clamp + V3-VS

### 主路径 A′（本票 PASS 只评 A′）

打断前末 **N=5** 帧 WGC/屏一致位置 `(t_k, x_k)`（Translation.X，QPC），做**二次**最小二乘，在估点 **`t_int`（QPC）** 取  
`v̂ = b + 2c·t_int`，  
再经 P-VS：`v_s = α_v·v̂ + (1−α_v)·v_s_prev`（首样 `v_s=v̂`），  
`InitialVelocity = (float)(calib · v_s)`。

| 参数 | 值 | 备注 |
|---|---|---|
| 族 | **P-LS+TS + P-VS** | KEEP 基线 + 本票启用 VS |
| 变体 ID | **`PATHB-A′ = V2-clamp + V3-VS`** | 本票默认主路径（显式另票） |
| `N` | **5** | KEEP；**禁**本票改 N |
| `order` | **2** | KEEP；**禁**本票改 order |
| `calib` | **0.7511**（exact=`0.7510829456`） | **KEEP**；比较/写入用 exact；显示四位；执行中改 = INVALID；**禁**再标定 |
| `ε` | **5.0 DIP/s** | KEEP；`|v̂|<ε` → 回退 B；**判在 VS 之前**（对平滑前的 `v̂`，不是对 `v_s`） |
| `P-VS` | **ON** | 本票主路径 **必须开**；关 = 偷换回 A → INVALID |
| `α_v` | **0.30** | **预登记写死**；执行中改 = INVALID |
| 首样 | **`v_s = v̂`** | 写死 |
| 限幅 `v_max` | **不设** | 禁另开口径 |
| 单位 | DIP/s | 禁止两帧差分冒充本默认 |
| **`t_int`** | **`t_int = min(t_compositor_interrupt_QPC, t_last_WGC)`** | `t_last_WGC` = 打断前最后一帧单调 WGC 样点 QPC；**禁**外推超过 `t_last_WGC` |
| 时基 | 仅 compositor / WGC **同一 QPC 原点** | **禁**异时基 |
| 估窗帧 | 仅打断前末 N 帧 | **禁**打断后帧 |
| 写入 IV | **`InitialVelocity = (float)(calib · v_s)`** | 非 `(calib·v̂)` |

A′ 前提：有效帧 ≥5；拟合有限；时戳单调；`|v̂|≥ε`（ε 在 VS **前**判）；取 WGC 成功；`t_int` 已按上式钳制；P-VS 已按 α_v=0.30 落实。

### 回退 B

A′ 失败（含 `|v̂|<ε` 在 VS 前判）→ `IV=null + InitialVelocity=0`（path=B）。  
**票级 PASS 只评默认 A′**；全程 B 的会话不计 PASS。

### 相对 PATH-B′ A / CALIB 的 diff

| 参数 | CALIB（CLOSED FAIL） | PATH-B′ A（CLOSED FAIL） | **PATH-A′（本票）** |
|---|---|---|---|
| 族 | P-LS+TS | P-LS+TS + V2-clamp | **同 A + P-VS** |
| N / order | 5 / 2 | KEEP | **KEEP** |
| calib | exact 0.7510829456 | KEEP | **KEEP** |
| ε | 5.0 | KEEP | KEEP；**VS 前判** |
| P-VS | OFF | **OFF** | **ON · α_v=0.30** |
| IV 公式 | `(float)(calib·v̂)` | `(float)(calib·v̂)` | **`(float)(calib·v_s)`** |
| t_int | compositor QPC（可外推） | **钳制 min(t_comp,t_last_WGC)** | **同 PATH-B′ A** |
| 估窗帧 | 打断前 N=5 | KEEP；禁打断后帧 | 同 |
| 测量 / thr / 17/20 | w=5；max(20,5%\|v_ref\|)；≥17/20 | KEEP | **KEEP（禁改）** |
| 结题标签 | SEED-CALIB-* | SEED-PATHB-* | **SEED-PATHA-PRIME-***（别名可并存但**禁止**写作 `SEED-PATHB-PASS`） |

**唯一实质性 diff = 启用 P-VS（α_v=0.30）及 IV 改用 `v_s`。** 无其它估窗/尺度变更。

### 否决（不得作本票默认）

| 候选 | 决断 |
|---|---|
| 关 P-VS / 偷换回 PATHB-A | **否**（本票主路径必须 A′；偷换 = INVALID） |
| 改 α_v（非 0.30）未预登记 | **否** → INVALID |
| 默认改 N∈{3,7} / order=1 | **否** |
| V4 分方向估法作默认 | **否** |
| 再标定 / 废止 exact | **否** |
| 立刻 Δv″ | **否** |
| 立刻以本票预授权改 H5 分母 | **否** |
| 打断后帧 / 异时基 / 影子 IV / 改 composition.h / 抬 thr / 降 17/20 | **禁** → INVALID |

### 结构风险（写死进正文）

KEEP calib + H5 分母=2F `v_screen` 下，CALIB/PATH-B′ 已见 `|ivel−v_pre|≈50+` DIP/s；幅度缺口**无法靠标量再抹平**。A′ 只赌尖峰平滑（P-VS）；**smoke 仍 FAIL 合法**——**禁抬 thr / 降 17/20 / 偷换估窗 / 改 H5 分母**；不得因此宣称虚 PASS。若本票仍 **比过 ∧ smoke 双灭** → 票 = FAIL，并 **保持** `SEED-TRUTH-CONFLICT` 挂账（或升主线），仍不预授权改 H5 分母。

**软提醒（非挡 LOCK）**：PATH-B′ med_r 贴下沿（s2/s3≈0.805–0.806），α_v=0.30 可能进一步欠 seed → 出窗则本票 FAIL/看 INVALID 条款，**不得**因此改 calib 或改分母救票。

### 可触及层

允许改 seed 路径启用 P-VS（α_v=0.30）与 prereg：`MotionSoftDemo` / harness / 控制器 seed 契约；**不**触 `composition.h`；**不**把 spring.h 闭式 v 写入 compositor IV；**不**改 N/order/ε/calib（KEEP）；**不**关 P-VS。

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

### G2-LOCK-2b 再挂账注记（写死 · 延续）

若本票 **H5 比回归过**，但 **t060 fwd 与 rev smoke 仍双灭**（两侧均未达 ≥17/20）→  
1. 本票结局 = **`SEED-PATHA-PRIME-FAIL`**（不得标 PASS）；  
2. **保持**挂账 `SEED-TRUTH-CONFLICT`（或升主线）：2F 比真值 vs 绝对 `v_pre` 冲突；**禁止**再开 calib 废止票当主线；**禁止**抬 thr；**不**预授权改 H5 分母；  
3. 本票**禁止**因此放宽 thr / 17/20 / 估窗 / 改 N·order·ε·calib·α_v。  
若比过但仅单侧 smoke FAIL → 仍为本票 **FAIL**；Δv″ **不**立刻开。  
offline 预测不得宣称 smoke PASS。  
结构风险下 **FAIL 合法**。

### G2-LOCK-3 对照

- 正对照 base：smoke 格上 `basePass=True`  
- 负对照 IVel0：仅 t060 fwd **与** rev 均 FAIL（t300* IVel0 不进票）

### G2-LOCK-4 会话

≥**3** 独立进程；禁跨会话池化；FAIL 不得替换翻盘；INVALID 可替换并留证。  
单机 DESKTOP-K93N3P8 / 60 Hz。

### G2-LOCK-5 票结局

结局 ∈ {`SEED-PATHA-PRIME-PASS`, `SEED-PATHA-PRIME-FAIL`, `SEED-PATHA-PRIME-INVALID`}  
（别名标签 `SEED-PATHB-APRIME-*` 可并存，但**禁止**写 `SEED-PATHB-PASS` 宣称翻盘 PATH-B′）：

- **PASS**：3/3 会话 t060 fwd+rev 格 PASS；basePass；IVel0_t060_both_FAIL；H5 比回归过；无影子 IV；composition.h 未改；路径=默认 A′=`PATHB-A′=V2-clamp+V3-VS`；calib=预登记 exact；`t_int` 钳制已落实；**P-VS=ON α_v=0.30**；ε 在 VS 前判；IV=`(float)(calib·v_s)`。  
- **FAIL**：有效样本足但未达 PASS（含：比过但 smoke 仍灭 → 见 G2-LOCK-2b）。  
- **INVALID**：见 G2-LOCK-6。

**禁止**写 `Δv-PASS` / `SEED-ABS-PASS` / `SEED-CALIB-PASS` / `SEED-PATHB-PASS` / 宣称翻盘。PASS ≠ 采用 ≠ PERF-CI ≠ 解冻 Bv。  
完整绝对门复测仍挂 **M2-Δv″**（须本票 **比回归过 ∧ t060 fwd+rev smoke 格 PASS** 后另开新闸门；**不立刻开**）。  
SEED-TRUTH-CONFLICT 挂账条件见 G2-LOCK-2b。

### G2-LOCK-6 INVALID（任一即整票）

1. 影子 IV / spring.h → compositor IV  
2. composition.h sha ≠ `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`  
3. 重跑/改写/宣称通过 `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43` / `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d` / `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de` / `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`；产物标 `Δv-PASS` / `SEED-ABS-PASS` / `SEED-CALIB-PASS` / `SEED-PATHB-PASS`  
4. 未预登记改 N/order/ε/calib/估窗/α_v/`t_int`；关 P-VS；或估窗用打断后帧 / 异时基；外推超过 `t_last_WGC`；执行中改 KEEP 常量；ε 改判到 VS 后  
5. 为过门改 thr 或放宽 17/20；躲 t060；只修一侧却放宽另一侧门槛；V4 单侧作默认却宣称本票 PASS；本票内预授权改 H5 分母  
6. 无 base / 无 IVel0 t060 双 FAIL 却宣称 PASS  
7. VM / 未亮屏 / 未 LOCK+APPROVED 采数；FAIL 后补测拼凑；跨会话池化；offline 预测当结题；默认同跑偷换 PATH-B′  
8. 本票产物写入 PERF-CI 合格证据；开 M3；解冻 Bv / 重开 S1/R-3/R-2；立刻开 Δv″（比/smoke 未齐）

### G2-LOCK-7 预登记与产物声明

采数前落盘并哈希：路径参数（**显式** `pathb_variant=PATHB-A′`；N/order/`t_int` 钳制/calib exact=`0.7510829456`/ε/**P-VS=ON** / **`α_v=0.30`** / **`v_s` 定义** / **`eps_before_vs=true`**）、smoke 格表、测量脚本、对照名单。  
每会话落盘：`seed_path`（A′/B）、`pathb_variant`、`calib`、`α_v`、`v_s`（或等价）、`eps_before_vs`、`t_int`/`t_last_WGC`、`v_wgc`/`iv_set`、`shadow_v_log`（若有）、`med_r`/`max_r`、smoke 格结果、`prereg_hash`、`session_id`/`pid`、composition hash。

## G3 产物

- `docs/m2_wgc_seed_path_a_prime_result.md`
- `artifacts/m2_wgc_seed_path_a_prime_*`（或 `m2_wgc_seed_pathb_aprime_*`；**不得**冒充 `m2_wgc_seed_path_b_prime_*` / `m2_wgc_seed_calib_*` / `m2_wgc_seed_abs_*` / `m2_delta_v_prime_*` 翻盘包）

## G4 ONELINE

M2-WGC-SEED-PATH-A′ | gate=<hash> @ <LOCKED_AT> | approved=<who>@<when> | path=PATHB-A′=V2-clamp+V3-VS/N=5/t_int=min(t_comp,t_last_WGC) | calib=0.7511(exact=0.7510829456 KEEP) | P-VS=ON α_v=0.30 eps_before_vs=true | smoke=t060 | ratio=reg | ctrl=baseP/IVel0F | ticket=<SEED-PATHA-PRIME-PASS/FAIL/INVALID> | 成功≠采用≠Δv-PASS；不改composition.h；禁影子IV；≠PERF-CI；a1161c77/09f6414b/831818/06467b82不翻盘；Δv″继续挂账；结构风险见G1；SEED-TRUTH-CONFLICT见G2-LOCK-2b；禁写作SEED-PATHB-PASS

## 禁令复述

不重开 S1/R-3/R-2；不改 composition.h；不解冻/升级 Bv；禁影子 IV；成功≠采用；禁翻盘四 FAIL gate `a1161c77…3a43` / `09f6414b…793d` / `831818e4…80de` / `06467b82…805c`；结题主用 `SEED-PATHA-PRIME-*`（别名可并存但**禁止**写作 `SEED-PATHB-PASS`）；禁标 `Δv-PASS` / `SEED-ABS-PASS` / `SEED-CALIB-PASS` / `SEED-PATHB-PASS`；PERF-CI 不降级；不开 M3 真机；禁抬 thr / 降 17/20 / 偷换估窗 / 改 H5 分母；禁单侧放宽；禁改 N/order；禁再标定；禁立刻 Δv″；禁默认同跑偷换 PATH-B′；结构风险下 FAIL 合法；PATH-B′ med_r 贴下沿 → A′ 出窗不得改 calib/分母救票；Δv″ 与 SEED-TRUTH-CONFLICT 挂账条件见正文。

— notion ai（M2-WGC-SEED-PATH-A′ LOCKED；不授权采数）
