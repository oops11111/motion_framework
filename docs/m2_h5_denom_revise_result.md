# M2-H5-DENOM-REVISE 结题结果

STATUS: CLOSED
TICKET: H5-DENOM-PASS
MODE: doc-only
WRITTEN_AT: 2026-10-26 16:02:00 Asia/Taipei
GATE: docs/m2_h5_denom_revise_gate_LOCKED.md
GATE_BODY_SHA256: d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7
LOCKED_AT: 2026-10-26 15:55:00 Asia/Taipei
APPROVED: docs/m2_h5_denom_revise_approved.md — pidan@2026-10-26 16:00:17 Australia/Perth
APPROVED_NOTE: widget reply「APPROVED doc-only（采数否/落地否/calib不适用）」（t260s1191）after LOCK d891df9f…33c7；LOCKED_AT=2026-10-26 15:55:00 Asia/Taipei

## 授权核验摘要

| 项 | 结果 |
|---|---|
| approved 文件 | 存在：`docs/m2_h5_denom_revise_approved.md` |
| APPROVED_BY / AT | pidan / **2026-10-26 16:00:17 Australia/Perth**（原文照抄；同 UTC+8） |
| mode | **doc-only** |
| 是否授权验证采数 | **否** |
| 是否授权落地 | **否**（不改分母落地；不启用真机新分母） |
| calib | **不适用**（契约定义阶段；禁静默沿用 0.7511；验证/落地须另 APPROVED 显式另批） |
| 会话数 / 格表 | **0** / **无**（不开真机） |
| GATE_BODY_SHA256 | 期望 `d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7` @ LOCKED_AT=2026-10-26 15:55:00 Asia/Taipei |
| 结题复算 hash | **MATCH**（排除 GATE_BODY_SHA256 行后全文 SHA-256）；**未改** LOCKED body/hash |

## G4 ONELINE

M2-H5-DENOM-REVISE | gate=d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7 @ 2026-10-26 15:55:00 Asia/Taipei | approved=pidan@2026-10-26 16:00:17 Australia/Perth | mode=doc-only | ticket=H5-DENOM-PASS | v_denom≡v_pre | 成功≠已落地采用≠Δv-PASS；未采数；未落地；calib未批；禁静默0.7511；六hash不翻盘；≠PERF-CI；禁立刻Δv″

## 票结局声明

- 勾选 doc-only 契约完备 → 票标签 **`H5-DENOM-PASS`**；STATUS=**CLOSED**；MODE=**doc-only**。
- **明确声明**：`H5-DENOM-PASS` ≠ 已落地采用 ≠ Δv-PASS ≠ 授权本票采数/落地/新 calib；≠ 六 hash 翻盘；≠ PERF-CI；≠ 解冻 Bv。
- 本票仅完成 **offline 契约结题**（新分母 / 比–绝对同源 / 门槛 KEEP / 另批 calib 规则写死）；**未**执行验证采数；**未**改分母落地；**未**批新 calib。

## 契约全文摘要（G2-LOCK-1）

### 新分母定义（G1-D1）

| 项 | 写死 |
|---|---|
| **符号** | **`v_denom`**（H5 比回归分母；取代旧闸门角色上的 `v_screen`） |
| **定义** | **`v_denom ≡ v_pre`**：与绝对 Δv smoke **同一**打断前连续估速——对打断前 WGC 位置 `(t_k,x_k)` 做**二次 LS，窗宽 w=5**，在打断估点求速（与 smoke 侧 `v_pre` 同几何） |
| **估点** | 与 seed/`t_int` 对齐：优先 **`t_int = min(t_compositor_interrupt_QPC, t_last_WGC)`**（禁外推过末样）；`v_denom` 在该估点取值，**不得**另开第二时基 |
| **时基** | compositor / WGC **同一 QPC 原点**；禁异时基 |
| **单位** | **DIP/s** |
| **分子** | **`v_seed` = 实际写入 compositor 的 `InitialVelocity`**（pathA 下 `calib·v̂` 或 A′ 下 `calib·v_s`；与 raw `ivel_set` 一致） |
| **比式** | **`r_i = \|v_seed / v_denom\|`**（`v_denom=0` 或非有限 → 该样 INVALID/不计 PASS） |
| **旧 2F** | 旧 `v_screen`（末两帧差分）**仅辅证 log**（可报 `r_2F`、`|v_denom−v_screen|`）；**不再作**计入 PASS 的 H5 门控分母 |

一句话：**`v_denom ≡ v_pre`（与 t060 smoke 同窗同几何的打断前连续估速）作 H5 分母，废除旧 2F `v_screen` 作门控分母。**

### 比–绝对同源句（G1-D2）

**比回归分母 `v_denom` 与 t060 绝对 smoke 的 `v_pre` 必须同一定义（二次 LS · w=5 · 同 QPC · 同打断估点）；`r_i=|v_seed/v_denom|` 与 smoke 的 `|v_post−v_pre|` 共享同一前打断连续真值。禁止「比钉旧 2F `v_screen`、绝对钉另一套 `v_pre`」。**

### 回归门槛 KEEP（G1-D3；禁抬 thr / 降 17/20）

| 门 | 门槛 | 备注 |
|---|---|---|
| H5 比（新） | 计入 PASS 的会话：`med_r ∈ [0.8,1.2]` 且无 `r_i≥2.0`（`r_i=\|v_seed/v_denom\|`） | 比出窗 → FAIL/INVALID；**不得** smoke 豁免 |
| 绝对 smoke | t060 fwd+rev：可判且格 PASS ≥**17/20** | thr / 17/20 / w=5 **相对旧票 KEEP；禁抬 thr、禁降 17/20** |
| 对照 | basePass ∧ IVel0_t060_both_FAIL | KEEP |
| 辅证 | 可报旧 `r_2F`、`|v_denom−v_screen|` | **不进** PASS 判据 |

### KEEP / 另批（G1-D4）

| 项 | 处置 |
|---|---|
| composition.h | **KEEP** `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`；不改 |
| 影子 IV | **禁** |
| N / order / ε | **默认 KEEP**（5 / 2 / 5.0）；改须 APPROVED **显式另批** |
| `t_int` 钳制 | **默认 KEEP** `min(t_comp,t_last_WGC)` |
| thr / 17/20 / meas w | **KEEP**；禁抬 / 禁降 |
| **calib** | **契约定义阶段不落地**；验证/落地须在新分母下 **显式另批** calib（推荐 `median(v_denom/v_hat)` 或写死常量表）。**禁止**静默沿用 exact=`0.7510829456`（旧 2F）却宣称新契约比回归 PASS |
| 六 hash 不翻盘 | 见下节全文 |

旧 H5-SEED（2F 契约）CLOSED PASS **归档保留**；本票是**新契约**，不重开/不宣称翻盘旧 H5-SEED。

### 采数 / 落地（G1-D5 · 本票）

- APPROVED 写死：**mode=doc-only**；验证采数=**否**；落地=**否**；calib=**不适用**。
- 本票 **未采数**；**未落地**；**未批 calib**。
- 若后续真机验证或落地，须**另票** LOCK+APPROVED，且 APPROVED 须显式写死：是否授权验证采数 / 是否授权落地 / calib 新常量或标定式（禁静默 0.7511）等字段。

## 六 hash 不翻盘（G2-LOCK-3 · 全文）

结题逐条声明：以下六 hash **未改、未重跑、未宣称通过**：

1. PATH-A′ FAIL `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362`
2. PATH-B′ FAIL `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`
3. SEED-CALIB FAIL `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`
4. SEED-ABS FAIL `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`
5. Δv′ FAIL `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`
6. TRUTH gate `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69`

本 DENOM gate `d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7`：**未改 body/hash**（结题复算 MATCH）。

## 未采数 / 未落地 / calib 未批声明

- APPROVED：**mode=doc-only**；验证采数=**否**；落地=**否**；calib=**不适用**。
- 本票**未**派真机 / workbuddy 采数；**未**改探针；**未**改分母落地；**未**启用真机新分母；**未**批新 calib；**未**静默沿用 0.7511。
- 无 `artifacts/m2_h5_denom_revise_*` 新采数包（本票不依赖采数）。

## 禁令遵守

- 未重开 S1/R-3/R-2；未解冻 Bv；禁影子 IV；未开 M3；禁立刻 Δv″
- 未执行改分母落地 / 抬 thr / 降 17/20 / 改 smoke 参照
- PERF-CI 仍 HOLD `5b007dd4…282a`；本票 ≠ PERF-CI
- `composition.h` 保持 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（本票未触）
- 结题标签仅 `H5-DENOM-PASS`；禁止写 `Δv-PASS` / 前序 `SEED-*-PASS` / `SEED-TRUTH-PASS` 翻盘
- 禁止把 doc-only PASS 写成已落地采用 / 已改 H5

## 下一票占位

**`M2-H5-DENOM-VERIFY`**（DRAFT：`docs/m2_h5_denom_verify_gate_DRAFT.md`；**NOT LOCKED**；本 next **不**授权采数；须新闸门 + APPROVED 显式：验证采数=是；落地=否或另字段；calib=新常量或标定式）。

## 产物清单

- `docs/m2_h5_denom_revise_result.md`（本文件 · 权威结题）
- `docs/notion_m2_h5_denom_revise_pack.md`
- `docs/notion_m2_h5_denom_revise_next.md`
- `docs/m2_h5_denom_verify_gate_DRAFT.md`（下一票骨架 · NOT LOCKED；不授权采数）

— notion ai（M2-H5-DENOM-REVISE CLOSED H5-DENOM-PASS；mode=doc-only；未采数；未落地；calib未批）
