# M2-H5-DENOM-VERIFY 门槛闸门（m2_h5_denom_verify_gate_LOCKED.md）

STATUS=LOCKED
LOCKED_AT: 2026-10-26 16:09:00 Asia/Taipei
GATE_BODY_SHA256: fd00a06058b239abfe03a52175463a52741718318ce99416a4ce762cf037889b
> **本 LOCK 定义验证采数闸门 + 另批 calib 流程；不授权落地；LOCK ≠ 授权采数直至 APPROVED_* 显式字段齐。** 须 APPROVED_* 非空，且 APPROVED 正文**显式写死**（验证采数=是；落地=否；calib=C1 流程或冻结常量 exact；路径=PATHB-A；KEEP v_denom≡v_pre；禁抬 thr/降 17/20；七 hash 不翻盘；禁立刻 Δv″）后，方可派 workbuddy 执行验证采数。GATE_BODY_SHA256 计算约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。本行写入后不得再改 body。
> 补洞依据：`docs/notion_m2_h5_denom_verify_gate_revise.md`；同伴稿 `docs/deepseek_m2_h5_denom_verify_review.md`（Harness 空回代拟；判可 LOCK；验证采数=是；落地=否；calib=**C1=median(v_denom/v_hat)**；路径=**PATHB-A**；A′ 默认不启用）。
> 触发：M2-H5-DENOM-REVISE = CLOSED **H5-DENOM-PASS**；mode=**doc-only**；result `docs/m2_h5_denom_revise_result.md`；结题包 `docs/notion_m2_h5_denom_revise_pack.md`；下一票 `docs/notion_m2_h5_denom_revise_next.md`。  
> DENOM gate（**不改、不翻盘**）：`d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7` @ 2026-10-26 15:55:00 Asia/Taipei。  
> TRUTH gate（**不改、不翻盘**）：`276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69` @ 2026-10-26 15:43:30 Asia/Taipei。  
> 前序五 FAIL（**不改、不重跑翻盘**）：  
> - PATH-A′ `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362`  
> - PATH-B′ `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`  
> - SEED-CALIB `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`  
> - SEED-ABS `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`  
> - Δv′ `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`  
> M2-PERF-CI HOLD `5b007dd4…282a`（本票 ≠ PERF-CI）。  
> **本票 = 新分母契约下的验证采数 + 另批 calib（C1）**；**不是**落地启用；**不是** Δv″；**不是**抬 thr；**不是**翻盘七 hash。  
> **声明**：`H5-DENOM-PASS`（doc-only）≠ 已落地采用；本 LOCK ≠ 授权采数（直至 APPROVED 显式字段齐）；成功 ≠ 落地采用 ≠ Δv-PASS。  
> LOCKED_AT 时区：Asia/Taipei（与项目 LOCKED 惯例一致；同 UTC+8 = Australia/Perth）。

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

**本票结论不解冻 B_v / 不重开 R-2 / 不重开 S1 / R-3；不改 composition.h。**  
**本票成功 ≠ 已落地采用 ≠ Δv-PASS ≠ 七 hash 翻盘 ≠ 采用；结题标签主用 `H5-DENOM-VERIFY-*`。**  
**LOCK ≠ 授权采数 ≠ 授权落地；勾选/决议 ≠ 自动执行；APPROVED_* 空。**  
**默认：落地=否；禁立刻 Δv″；禁静默 0.7511。**

---

## G0 硬边界

- 不重开 S1 / R-3 / R-2；不解冻 / 不升级 B_v；成功 ≠ 采用；旁路挂账不挡、本票也不清旁路。
- 不改 `composition.h`（sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）。
- **禁影子 IV**。
- **禁翻盘**：禁止重跑 / 改写 / 宣称通过  
  - PATH-A′ FAIL `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362`  
  - PATH-B′ FAIL `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`  
  - SEED-CALIB FAIL `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`  
  - SEED-ABS FAIL `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`  
  - Δv′ FAIL `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`  
  - TRUTH gate `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69`  
  - DENOM gate `d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7`  
  禁止把本票产物标为 `Δv-PASS` / 前序 `SEED-*-PASS` / `SEED-TRUTH-PASS` / 重写 `H5-DENOM-PASS` 为已落地。
- **本票 ≠ PERF-CI**；产物不得作 PERF-CI 合格证据；PERF-CI 仍 HOLD。
- 不开 M3 真机（本票仅单机验证范围，若 APPROVED）。
- **禁立刻开 Δv″**（须本票验证过比+smoke 可齐后再另开）。
- **禁抬 thr**；**禁**降 17/20 伪装 PASS。
- **禁静默沿用 calib=0.7511**（旧 2F）。
- 未 LOCKED+APPROVED 禁止采数 / 禁止分母落地；即便 APPROVED，**采数与落地与 calib 范围须正文写死**；落地默认否。

## G1 票性质 · 验证采数 + 另批 calib（C1）

### 继承契约（只读；KEEP）

自 DENOM LOCK 继承，本票**不得改定义**：

| 项 | KEEP |
|---|---|
| **`v_denom ≡ v_pre`** | 二次 LS · w=5 · 同 QPC · 同 `t_int=min(t_comp,t_last_WGC)`；单位 DIP/s |
| 比式 | `r_i=\|v_seed/v_denom\|` |
| 旧 2F | 仅辅证；**不作**门控分母 |
| 比–绝对同源 | 比分母与 smoke `v_pre` 同一定义 |
| H5 比门槛 | `med_r ∈ [0.8,1.2]` 且无 `r_i≥2.0` |
| smoke | ≥**17/20**；`thr_i=max(20,0.05·\|v_ref,i\|)`；w=5；**禁抬 thr / 降 17/20** |
| 对照 | **KEEP** basePass ∧ IVel0_t060_both_FAIL（前序有则 KEEP） |
| N / order / ε | **KEEP** 5 / 2 / 5.0 |
| `t_int` | **KEEP** V2-clamp |
| composition.h | **KEEP** `a55d4952…e418`；不改 |
| 影子 IV | **禁** |

### G1-V1 验证采数范围（写死）

| 项 | 写死 |
|---|---|
| **目的** | 在已 LOCK 的 `v_denom≡v_pre` 契约下，另批新 calib（C1）后，验证 **H5 比 + t060 绝对 smoke 可否同批齐**；**非**翻盘七 hash；**非** PERF-CI；**非**落地启用 |
| **格表** | **主：t060 fwd + t060 rev**（绝对 smoke）；**同批报 H5 比**（`r_i=\|v_seed/v_denom\|`，`med_r` / any `r_i≥2`） |
| **辅证（可选，不进 PASS）** | 旧 `r_2F`、`\|v_denom−v_screen\|`、`\|ivel−v_pre\|` 分位 |
| **会话数** | **≥3 独立进程**（建议 s1/s2/s3；进程间冷启；禁同进程伪独立） |
| **机器** | **单机** `DESKTOP-K93N3P8` · 60 Hz；**不开第二台**；**≠PERF-CI** |
| **对照 KEEP** | basePass ∧ IVel0_t060_both_FAIL |
| **产物前缀** | `artifacts/m2_h5_denom_verify_*` / `docs/m2_h5_denom_verify_*` only；**不得**冒充 path_a / path_b / calib / abs / delta_v / truth / denom_revise 翻盘包 |

**采数时机**：**仅** LOCK + APPROVED 且 APPROVED 正文写死「验证采数=**是**」之后，由 workbuddy 执行；本 LOCK **单独不授权**采数。

### G1-V2 落地 = 否（写死）

| 字段 | 本票拍板 |
|---|---|
| **落地** | **否**（不改分母落地启用；不把探针默认切到新分母生产路径） |
| 验证探针 | 允许在 **验证会话**内按 prereg 使用 `v_denom≡v_pre` + 新 calib 计分；**≠** 默认落地采用 |
| 若将来落地 | **另票** LOCK+APPROVED，且 APPROVED **另字段**显式「落地=是」+ 边界 |

**PASS ≠ 已落地采用。**

### G1-V3 calib 标定式 **C1**（禁静默 0.7511）

\[
\texttt{calib\_new} = \mathrm{median}\bigl(v_{\mathrm{denom}} / \hat{v}\bigr)
\]

对计入标定的有效样：Arm-WGC-seed（或等价写入路径）、`v_denom` 与 \(\hat{v}\) 均有限且 \(\hat{v}\neq 0\)、\(|\hat{v}|\ge\varepsilon\)（ε=5.0 KEEP）、同 QPC / 同 `t_int` 几何。

**两阶段流程（写死）**

1. **Phase-Calib（标定窗）**  
   - 在 LOCK+APPROVED 后，于验证机跑 t060 fwd+rev、≥3 独立进程。  
   - 估速侧：**P-LS+TS · N=5 · order=2 · V2-clamp `t_int`**（路径 **PATHB-A**）；本阶段写入 IV 可用 `calib_probe=1.0` 仅用于露出 \(\hat{v}\)，或直接从探针导出 \(\hat{v}\)**不依赖**旧 0.7511。  
   - 分母：严格 **`v_denom≡v_pre`**（二次 LS · w=5 · 同估点）。  
   - 汇总有效样算 **`calib_new = median(v_denom/v_hat)`**；落盘到 `artifacts/m2_h5_denom_verify_calib_*` 与 result 正文（写死 exact 小数）。

2. **Phase-Score（计分窗）**  
   - **冻结** `calib_new`（APPROVED 可写「标定式 C1 → 本批 exact=…」或预先授权「执行 C1 后冻结」）。  
   - **同一 prereg 路径 PATHB-A** 下重跑或同批第二臂：`InitialVelocity = (float)(calib_new · v̂)`（本票默认不走 A′）。  
   - 用冻结常量评 **H5 比 + smoke ≥17/20**；**禁止**计分窗再改 calib。

3. **禁令**  
   - **禁静默沿用** exact=`0.7510829456` / 显示 `0.7511`（旧 2F）。  
   - **禁**用 `median(v_screen/v_hat)` 或 `median(v_screen/ivel)` 冒充新分母标定。  
   - **禁**标定窗用旧 2F 分母、计分窗改称 `v_denom`。

### G1-V4 路径拍板：**PATHB-A**（V2-clamp；P-VS=OFF）

| 项 | 写死 |
|---|---|
| **主路径** | **`PATHB-A = V2-clamp`**（P-LS+TS · N=5 · order=2 · P-VS=**OFF** · `t_int` 钳制） |
| IV | `InitialVelocity = (float)(calib_new · v̂)` |
| **A′（P-VS α_v=0.30）** | **本票默认不启用**；仅当 Phase-Score 下 A 出现「比已进窗但 rev 尖峰导致 smoke 系统性灭、且归因证明需抑尖」时，**另票** prereg A′——**禁止**本票执行中偷开 |
| 理由 | 前序 A/A′ 在旧 2F+旧 calib 下均 CLOSED FAIL；TRUTH/DENOM 已把根因钉在**分母定义**而非平滑层。新分母 + C1 标定后应先验证**最小路径 A**；A′ 多 α 自由度，易与新 calib 纠缠、归因不清 |

**否决**：本票默认同跑 A+A′ 择优；执行中改 α；KEEP 旧 calib 却开 A′。

### G1-V5 APPROVED 须显式字段（LOCK 后拍板；现空）

| 字段 | 须填 |
|---|---|
| 验证采数 | **是**（目的/格表=t060 fwd+rev+H5 比；≥3 独立进程；单机 DESKTOP-K93N3P8；≠PERF-CI） |
| 落地 | **否** |
| calib | **「标定式 C1 = median(v_denom/v_hat)」**（Phase-Calib→冻结→Phase-Score）**或**写死新常量 exact=…（二者择一；禁空、禁「沿用 0.7511」） |
| 路径 | **PATHB-A**（V2-clamp；P-VS=OFF）；A′ 默认不启用 |
| KEEP v_denom≡v_pre | 是 |
| 禁抬 thr / 降 17/20 | 是 |
| 七 hash 不翻盘 | 是 |
| PERF-CI | HOLD |
| 禁立刻 Δv″ | 是 |

### G1-V6 PASS 判据（比 + smoke + 对照）

结题宣称 `H5-DENOM-VERIFY-PASS` 须同时满足：

1. **H5 比**：计入 PASS 的会话 `med_r ∈ [0.8,1.2]` 且无 `r_i≥2.0`（`r_i=\|v_seed/v_denom\|`；`v_denom≡v_pre`）。  
2. **绝对 smoke**：t060 fwd+rev 可判且格 PASS ≥**17/20**；`thr_i=max(20,0.05·\|v_ref,i\|)`；meas w=5；**禁抬 thr / 降 17/20**。  
3. **对照 KEEP**：basePass ∧ IVel0_t060_both_FAIL（前序有则 KEEP；缺对照记入结题说明，不得借缺对照抬门槛）。  
4. **calib**：已按 C1 流程冻结并写入 exact（或 APPROVED 写死常量）；**非**静默 0.7511。  
5. **路径**：PATHB-A；未偷开 A′；未改 N/order/ε/`t_int`。  
6. **七 hash 不翻盘**逐条声明；未违规落地；未立刻 Δv″；≠PERF-CI。

**PASS ≠ 已落地采用 ≠ Δv-PASS ≠ 七 hash 翻盘 ≠ 可立刻 Δv″。**

### G1-V7 虚 PASS / INVALID 否决表

| 行为 | 处置 |
|---|---|
| 未 LOCK+APPROVED 即采数或分母落地 | INVALID |
| 静默沿用 calib=0.7511 / exact 0.7510829456 宣称新分母比 PASS | INVALID |
| 标定仍用 `v_screen` 却称 C1 / `v_denom` | INVALID |
| 比仍用旧 2F 分母却宣称新契约 PASS | INVALID |
| 抬 thr / 降 17/20 | INVALID |
| 重跑/改写/宣称通过七 hash；产物标翻盘 PASS | INVALID |
| 立刻开 Δv″；开 M3；解冻 Bv；改 composition.h；影子 IV | INVALID |
| 产物作 PERF-CI 合格证据；开第二台冒充矩阵 | INVALID |
| 默认可落地；把 VERIFY-PASS 写成已落地采用（无落地=是） | INVALID |
| 把 doc-only `H5-DENOM-PASS` / 本 LOCK 当已授权采数（无 APPROVED 显式字段） | INVALID |
| 执行中偷开 A′ / 改 α / 改 N·order·ε | INVALID |
| 计分窗未冻结 calib 又改标定 | INVALID |

### 明确否决

立刻 Δv″；默认可落地；静默沿用旧 calib=0.7511；翻盘七 hash；抬 thr；降 17/20；未 LOCK/未 APPROVED 显式字段即采数或落地；改 composition.h；影子 IV；开 M3；解冻 Bv；把 doc-only `H5-DENOM-PASS` 写成已落地采用；把本票产物标 `Δv-PASS`；偷开 A′。

## G2 主判据

### G2-LOCK-1 契约继承完备

结题须确认：继承 `v_denom≡v_pre` 与比–绝对同源；门槛 KEEP；禁抬 thr；calib=C1 另批已显式；路径=PATHB-A；落地=否；七 hash 不翻盘逐条。缺任一项 → INVALID 或退回补洞。

### G2-LOCK-2 执行边界

- LOCK+APPROVED 前：**禁止**采数与分母落地。  
- 勾选/决议 ≠ 自动执行；执行范围以 APPROVED 正文为准。  
- **本 LOCK 阶段**：定义闸门与 C1 流程；**APPROVED_* 空时不授权**采数。

### G2-LOCK-3 不翻盘核对

结题须逐条声明七 hash **未改、未重跑、未宣称通过**：  
`f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362` /  
`a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43` /  
`09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d` /  
`831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de` /  
`06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c` /  
`276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69` /  
`d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7`。

### G2-LOCK-4 票结局

结局 ∈ {`H5-DENOM-VERIFY-PASS`, `H5-DENOM-VERIFY-FAIL`, `H5-DENOM-VERIFY-INVALID`, `H5-DENOM-VERIFY-STOP`}：

- **PASS**：G1-V6 判据齐 ∧ 七 hash 不翻盘 ∧ 未违规落地 ∧ calib 已按 C1 冻结（或 APPROVED 写死常量）。**`H5-DENOM-VERIFY-PASS` ≠ 已落地采用 ≠ Δv-PASS ≠ 七 hash 翻盘 ≠ 可立刻 Δv″**。  
- **FAIL**：验证范围内比/smoke/对照未齐（须写清缺口；不得借 FAIL 偷落地或立刻 Δv″）。  
- **STOP**：显式归档停线（仍禁翻盘/立刻 Δv″/默认可落地）。  
- **INVALID**：见 G2-LOCK-5。

**禁止**写 `Δv-PASS` / 前序 `SEED-*-PASS` / `SEED-TRUTH-PASS` / 重写 `H5-DENOM-PASS` 为已落地。PASS ≠ 采用 ≠ PERF-CI ≠ 解冻 Bv ≠ 已落地改分母。

### G2-LOCK-5 INVALID（任一即整票）

1. 影子 IV / 改 composition.h  
2. 重跑/改写/宣称通过七 hash；产物标翻盘 PASS  
3. 抬 thr / 降 17/20  
4. 未 LOCK+APPROVED 采数或分母落地；或 APPROVED 未写死声明字段却采数/落地  
5. 立刻开 Δv″；开 M3；解冻 Bv / 重开 S1/R-3/R-2  
6. 产物作 PERF-CI 合格证据；开第二台冒充矩阵  
7. 静默沿用 calib=0.7511 宣称新分母比 PASS；比仍用旧 2F 分母却宣称新契约 PASS；标定仍用 `v_screen` 却称 C1  
8. 把 doc-only `H5-DENOM-PASS` / 本票 PASS 写成已落地采用（无显式落地授权时）  
9. 默认可落地；把本 LOCK 当已授权采数（无 APPROVED 显式字段）；执行中偷开 A′ / 计分窗改 calib

## G3 产物

- `docs/m2_h5_denom_verify_result.md`（LOCK+APPROVED+结题后）
- `docs/notion_m2_h5_denom_verify_pack.md`（若走结题包体例）
- `artifacts/m2_h5_denom_verify_*`（若有授权采数；**不得**冒充翻盘包）
- `artifacts/m2_h5_denom_verify_calib_*`（C1 标定落盘；写死 exact）

## G4 ONELINE

M2-H5-DENOM-VERIFY | gate=<hash> @ 2026-10-26 16:09:00 Asia/Taipei | approved=<who>@<when> | mode=verify | ticket=H5-DENOM-VERIFY-* | KEEP v_denom≡v_pre；path=PATHB-A；calib=C1 median(v_denom/v_hat)；禁静默0.7511；验证采数=是（须APPROVED）；落地=否；七hash不翻盘；禁抬thr；成功≠落地采用≠Δv-PASS；≠PERF-CI；禁立刻Δv″；单机 DESKTOP-K93N3P8

## 禁令复述

不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；成功≠采用≠落地采用；禁翻盘七 hash（五 FAIL + TRUTH `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69` + DENOM `d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7`）；结题主用 `H5-DENOM-VERIFY-*`；禁标 `Δv-PASS` / 前序翻盘；PERF-CI HOLD；不开 M3；禁抬 thr / 降 17/20；禁立刻 Δv″；**KEEP v_denom≡v_pre**；calib=**C1**（Phase-Calib→冻结→Phase-Score）；禁静默 0.7511；路径=**PATHB-A**；A′ 默认不启用；落地=否；LOCK≠授权采数（直至 APPROVED 显式字段齐）；`H5-DENOM-PASS`≠已落地采用；产物前缀 `m2_h5_denom_verify_*` only。

— notion ai（M2-H5-DENOM-VERIFY LOCKED；验证采数闸门+C1；落地=否；APPROVED_* 空；不单独授权采数）
