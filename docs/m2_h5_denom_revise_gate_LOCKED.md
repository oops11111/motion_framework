# M2-H5-DENOM-REVISE 门槛闸门（m2_h5_denom_revise_gate_LOCKED.md）

STATUS=LOCKED
LOCKED_AT: 2026-10-26 15:55:00 Asia/Taipei
GATE_BODY_SHA256: d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7
> **本 LOCK 只定义契约；默认不授权真机采数；默认不授权改分母落地。** 须 APPROVED_* 非空，且 APPROVED 正文**显式写死**（是否授权验证采数 / 是否授权落地 / calib 新常量或 KEEP 声明）后，方可派执行或真机。GATE_BODY_SHA256 计算约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。本行写入后不得再改 body。
> 补洞依据：`docs/notion_m2_h5_denom_revise_gate_revise.md`；同伴稿 `docs/deepseek_m2_h5_denom_revise_review.md`（Harness 空回代拟；判可 LOCK；推荐 **v_denom≡v_pre**；废旧 2F 作门控分母；验证/落地须另批 calib，禁静默 0.7511）。
> 触发：M2-SEED-TRUTH-CONFLICT = CLOSED **SEED-TRUTH-PASS**；menu=**M1**；mode=offline-resolve；result `docs/m2_seed_truth_conflict_result.md`；结题包 `docs/notion_m2_seed_truth_conflict_pack.md`；下一票 `docs/notion_m2_seed_truth_conflict_next.md`。  
> TRUTH gate（**不改、不翻盘**）：`276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69` @ 2026-10-26 15:43:30 Asia/Taipei。  
> 前序五 FAIL（**不改、不重跑翻盘**）：  
> - PATH-A′ `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362`  
> - PATH-B′ `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`  
> - SEED-CALIB `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`  
> - SEED-ABS `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`  
> - Δv′ `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`  
> H5-SEED CLOSED PASS（旧 2F 契约仍归档）；M2-PERF-CI HOLD `5b007dd4…282a`（本票 ≠ PERF-CI）。  
> **本票 = 新分母 / 估窗契约定义**；**不是**落地执行改分母；**不是** Δv″；**不是**抬 thr；**不是**翻盘 TRUTH/五 FAIL。  
> **声明**：TRUTH `SEED-TRUTH-PASS` ≠ 已改 H5；本 LOCK ≠ 落地执行。  
> LOCKED_AT 时区：Asia/Taipei（与项目 LOCKED 惯例一致；同 UTC+8 = Australia/Perth）。

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

**本票结论不解冻 B_v / 不重开 R-2 / 不重开 S1 / R-3；不改 composition.h。**  
**本票成功 ≠ 已落地采用 ≠ Δv-PASS ≠ 五 FAIL 翻盘 ≠ TRUTH 翻盘 ≠ 采用；结题标签主用 `H5-DENOM-*`。**  
**LOCK ≠ 授权采数 ≠ 授权改分母落地；勾选/决议 ≠ 自动执行；APPROVED_* 空。**  
**默认：不开采数；不落地改分母。**

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
  禁止把本票产物标为 `Δv-PASS` / 前序 `SEED-*-PASS` / `SEED-TRUTH-PASS` 翻盘重写。
- **本票 ≠ PERF-CI**；产物不得作 PERF-CI 合格证据；PERF-CI 仍 HOLD。
- 不开 M3 真机。
- **禁立刻开 Δv″**（须新契约下比+smoke 可齐后再另开）。
- **禁抬 thr**；**禁**降 17/20 伪装 PASS。
- 未 LOCKED+APPROVED 禁止采数 / 禁止分母落地；即便 APPROVED，**采数与落地范围须正文写死**；默认仍不开采数、不落地。

## G1 票性质 · 新分母契约（M1 执行票 · 定义阶段）

### 根因引用（只读；不重采）

TRUTH 已写死：旧 H5 分母=`v_screen`（打断前末两帧 WGC）与绝对 `v_pre` / t060 smoke **不可同票 PASS**；平滑臂已耗尽；结构风险 FAIL 合法。本票任务是**另开契约**使比真值与绝对连续**同源**，不是调 thr。**本 LOCK 只定义契约，不执行改分母落地。**

### G1-D1 新分母定义（写死）

| 项 | 写死 |
|---|---|
| **符号** | **`v_denom`**（H5 比回归分母；取代旧闸门角色上的 `v_screen`） |
| **定义** | **`v_denom ≡ v_pre`**：与绝对 Δv smoke **同一**打断前连续估速——对打断前 WGC 位置 `(t_k,x_k)` 做**二次 LS，窗宽 w=5**，在打断估点求速（与 smoke 侧 `v_pre` 同几何） |
| **估点** | 与 seed/`t_int` 对齐：优先 **`t_int = min(t_compositor_interrupt_QPC, t_last_WGC)`**（禁外推过末样）；`v_denom` 在该估点取值，**不得**另开第二时基 |
| **时基** | compositor / WGC **同一 QPC 原点**；禁异时基 |
| **单位** | **DIP/s** |
| **分子** | **`v_seed` = 实际写入 compositor 的 `InitialVelocity`**（即 pathA 下 `calib·v̂` 或 A′ 下 `calib·v_s`；与 raw `ivel_set` 一致） |
| **比式** | **`r_i = \|v_seed / v_denom\|`**（`v_denom=0` 或非有限 → 该样 INVALID/不计 PASS） |
| **旧 2F** | 旧 `v_screen`（末两帧差分）**仅辅证 log**（可报 `r_2F`、`|v_denom−v_screen|`）；**不再作**计入 PASS 的 H5 门控分母（可归档对照） |

**否决的替代（不作默认）**

| 候选 | 决断 | 理由 |
|---|---|---|
| 保持 2F `v_screen` 作分母 | 否 | TRUTH 已证与 `v_pre` 冲突 |
| 分母用 seed 窗 LS `v̂`（自比） | 否 | 易虚 PASS；与绝对 smoke 仍可不齐 |
| 分母用 `v_post` | 否 | 含 seed 后果；因果倒置 |
| M2：改 smoke 参照去就 2F | 否 | TRUTH 已否决；弱化绝对门 |

**一句话**：`v_denom ≡ v_pre`（与 t060 smoke 同窗同几何的打断前连续估速）作 H5 分母，废除旧 2F `v_screen` 作门控分母。

### G1-D2 比–绝对同源句（写死）

**比回归分母 `v_denom` 与 t060 绝对 smoke 的 `v_pre` 必须同一定义（二次 LS · w=5 · 同 QPC · 同打断估点）；`r_i=|v_seed/v_denom|` 与 smoke 的 `|v_post−v_pre|` 共享同一前打断连续真值。禁止「比钉旧 2F `v_screen`、绝对钉另一套 `v_pre`」。**

- smoke 门槛几何：**KEEP**——`thr_i=max(20,0.05·|v_ref,i|)`；二次 LS w=5；≥**17/20**；P1/P2。  
- **禁**为对齐而去改 thr 或把 smoke 参照改回 2F。

### G1-D3 回归门槛（KEEP；禁抬 thr / 降 17/20）

| 门 | 门槛 | 备注 |
|---|---|---|
| H5 比（新） | 计入 PASS 的会话：`med_r ∈ [0.8,1.2]` 且无 `r_i≥2.0`（`r_i=\|v_seed/v_denom\|`） | 比出窗 → FAIL/INVALID；**不得** smoke 豁免 |
| 绝对 smoke | t060 fwd+rev：可判且格 PASS ≥**17/20** | thr / 17/20 / w=5 **相对旧票 KEEP；禁抬 thr、禁降 17/20** |
| 对照 | basePass ∧ IVel0_t060_both_FAIL | KEEP |
| 辅证 | 可报旧 `r_2F`、`|v_denom−v_screen|` | **不进** PASS 判据 |
| 可判规则 | KEEP（相对旧 H5/seed 票可判定义） | 不得借「不可判」降门槛伪装 PASS |

### G1-D4 KEEP 表与另批

| 项 | 处置 |
|---|---|
| composition.h | **KEEP** `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`；不改 |
| 影子 IV | **禁** |
| N / order / ε | **默认 KEEP**（5 / 2 / 5.0）；改须 APPROVED **显式另批** |
| `t_int` 钳制 | **默认 KEEP** `min(t_comp,t_last_WGC)` |
| thr / 17/20 / meas w | **KEEP**；禁抬 / 禁降 |
| **calib** | **契约定义阶段不落地**；若 APPROVED 授权**验证/落地**，须在新分母下 **显式另批** calib（推荐公式仍 `median(v_denom/v_hat)` 或写死常量表）。**禁止**静默沿用 exact=`0.7510829456`（该值相对旧 2F）却宣称新契约比回归 PASS——旧 calib 钉 2F，换 `v_denom≡v_pre` 后预期 med_r 系统性下偏出窗 |
| P-VS / α_v | 默认不强制；若沿用 A′ 须另批写死 |
| 六 hash 不翻盘 | PATH-A′ `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362`；PATH-B′ `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`；CALIB `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`；ABS `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`；Δv′ `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`；TRUTH `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69` |

旧 H5-SEED（2F 契约）CLOSED PASS **归档保留**；本票是**新契约**，不重开/不宣称翻盘旧 H5-SEED。

### G1-D5 采数 / 落地（默认关）

- **默认：不开采数；不改分母落地。**  
- 本票可先 **doc-only PASS**：契约全文完备即可能 `H5-DENOM-PASS`（mode=doc-only），**≠** 已在真机改分母 / 已落地采用。  
- 若需真机验证或落地，APPROVED 须**显式写死**至少下列字段：

| 字段 | 须填 |
|---|---|
| 是否授权验证采数 | 是/否；若是：目的、格表、会话数 |
| 是否授权落地 | 是/否；若是：落地范围（分母定义启用边界） |
| calib | **新常量**或 **KEEP 声明**（禁静默沿用 0.7511） |
| 格表 | t060 fwd+rev（+ 可选辅证） |
| 会话数 | ≥3 独立进程（若采数） |
| 路径/prereg | N/order/ε/`t_int`/是否 P-VS |
| 仍禁改 KEEP | composition.h；thr；17/20；六 hash；禁影子 IV |
| 产物前缀 | `m2_h5_denom_revise_*` only |

### G1-D6 虚 PASS / INVALID 否决表（须进结题核对）

| 行为 | 处置 |
|---|---|
| 未 LOCK+APPROVED 改分母落地 / 采数 | INVALID |
| 抬 thr / 降 17/20 | INVALID |
| 比仍用旧 2F 分母却宣称新契约 PASS | INVALID |
| 静默沿用 calib=0.7511（旧 2F）宣称新分母比 PASS | INVALID |
| 重跑/宣称通过六 hash；标 `Δv-PASS` / 前序 `SEED-*-PASS` / 重写 `SEED-TRUTH-PASS` | INVALID |
| 立刻 Δv″；开 M3；解冻 Bv；改 composition.h；影子 IV | INVALID |
| 把 TRUTH PASS 写成已改 H5 | INVALID |
| 产物作 PERF-CI 合格证据 | INVALID |
| 把 doc-only PASS 写成已落地采用 | INVALID |

### 明确否决

抬 thr；降 17/20；翻盘五 FAIL / TRUTH；立刻 Δv″；未授权采数；未 LOCK/未 APPROVED 显式字段即改分母落地；改 composition.h；影子 IV；开 M3；解冻 Bv；把「比过」写成任一 FAIL/TRUTH 已翻盘 PASS；静默 0.7511。

## G2 主判据

### G2-LOCK-1 契约完备

结题须含：新分母定义全文（G1-D1）；比–绝对同源句（G1-D2）；回归门槛 KEEP（G1-D3）；禁抬 thr / 禁降 17/20 句；KEEP/另批 calib（G1-D4）；采数/落地默认关与 APPROVED 须声明字段（G1-D5）；虚 PASS 否决表（G1-D6）；六 hash 不翻盘逐条。缺任一项 → INVALID 或退回补洞。

### G2-LOCK-2 执行边界

- LOCK+APPROVED 前：**禁止**改分母落地与采数。  
- 勾选/决议 ≠ 自动执行；执行范围以 APPROVED 正文为准。  
- **本 LOCK 阶段**：只定义契约；**不执行**改分母落地。

### G2-LOCK-3 不翻盘核对

结题须逐条声明六 hash **未改、未重跑、未宣称通过**：  
`f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362` /  
`a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43` /  
`09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d` /  
`831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de` /  
`06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c` /  
`276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69`。

### G2-LOCK-4 票结局

结局 ∈ {`H5-DENOM-PASS`, `H5-DENOM-FAIL`, `H5-DENOM-INVALID`, `H5-DENOM-STOP`}：

- **PASS**（含 doc-only）：契约全文完备 ∧ 六 hash 不翻盘 ∧ 未违规执行改动 ∧（若未授权则无采数、无落地）。**`H5-DENOM-PASS` ≠ 已落地采用 ≠ Δv-PASS ≠ 五 FAIL/TRUTH 翻盘**。  
- **FAIL**：契约未能在闸门范围内收敛（须写清缺口；不得借 FAIL 偷开采数或落地）。  
- **STOP**：显式归档停线（仍禁翻盘/立刻 Δv″）。  
- **INVALID**：见 G2-LOCK-5。

**禁止**写 `Δv-PASS` / 前序 `SEED-*-PASS` / `SEED-TRUTH-PASS` 翻盘。PASS ≠ 采用 ≠ PERF-CI ≠ 解冻 Bv ≠ 已落地改分母。

### G2-LOCK-5 INVALID（任一即整票）

1. 影子 IV / 改 composition.h  
2. 重跑/改写/宣称通过六 hash（五 FAIL + TRUTH）；产物标翻盘 PASS  
3. 抬 thr / 降 17/20  
4. 未 LOCK+APPROVED 采数或分母落地；或 APPROVED 未写死声明字段却采数/落地  
5. 立刻开 Δv″；开 M3；解冻 Bv / 重开 S1/R-3/R-2  
6. 产物作 PERF-CI 合格证据  
7. 比仍用旧 2F 分母却宣称新契约 PASS；静默沿用 calib=0.7511 宣称新分母比 PASS  
8. 把 TRUTH PASS / doc-only PASS 写成已改 H5 / 已落地采用  
9. 把「比过」写成任一 FAIL/TRUTH 已翻盘 PASS

## G3 产物

- `docs/m2_h5_denom_revise_result.md`（LOCK+结题后）
- `docs/notion_m2_h5_denom_revise_pack.md`（若走结题包体例）
- `artifacts/m2_h5_denom_revise_*`（若有授权采数；**不得**冒充 path_a/path_b/calib/abs/delta_v/truth 翻盘包）

## G4 ONELINE

M2-H5-DENOM-REVISE | gate=<hash> @ 2026-10-26 15:55:00 Asia/Taipei | approved=<who>@<when> | mode=<doc-only|exec|…> | ticket=<H5-DENOM-*> | v_denom≡v_pre（二次LS w=5·同QPC·同t_int）；旧2F不作门控分母；比–绝对同源；禁抬thr/降17/20；calib须另批禁静默0.7511；六hash不翻盘；成功≠落地采用≠Δv-PASS；不改composition.h；禁影子IV；≠PERF-CI；禁立刻Δv″；默认不开采数不落地；APPROVED须声明字段

## 禁令复述

不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；成功≠采用≠落地采用；禁翻盘六 hash：TRUTH `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69` / PATH-A′ `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362` / PATH-B′ `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43` / CALIB `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d` / ABS `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de` / Δv′ `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`；结题主用 `H5-DENOM-*`；禁标 `Δv-PASS` / 前序 SEED-*-PASS / TRUTH 翻盘；PERF-CI HOLD；不开 M3；禁抬 thr / 降 17/20；禁立刻 Δv″；**v_denom≡v_pre**；旧 2F 不作门控分母；calib 验证/落地须另批；禁静默 0.7511；默认不开采数、不落地；LOCK≠授权采数≠授权落地；TRUTH PASS≠已改 H5。

— notion ai（M2-H5-DENOM-REVISE LOCKED；只定义契约；不授权采数；不授权落地）
