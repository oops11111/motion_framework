# DeepSeek · M2-H5-DENOM-VERIFY 可 LOCK 补洞（验证采数 + 新分母另批 calib）

- 时间：2026-10-26（Asia/Taipei, UTC+8）
- 状态：**文档 only**；NOT LOCKED；**禁 LOCK 前采数**；**默认不落地**
- **出处说明**：**Harness 空回代拟**；对照 `docs/m2_h5_denom_verify_gate_DRAFT.md`、`docs/m2_h5_denom_revise_result.md`（H5-DENOM-PASS · doc-only）、`docs/notion_m2_h5_denom_revise_pack.md` / `next.md`、`docs/m2_h5_denom_revise_gate_LOCKED.md`、`docs/deepseek_m2_h5_denom_revise_review.md` 起草。
- composition.h sha 锁：`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（未改、不改）
- **声明**：前序 `H5-DENOM-PASS`（doc-only）≠ 已落地；本补洞 ≠ 授权采数；成功 ≠ 落地采用 ≠ Δv-PASS。

## 禁令复述（文首）

不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；禁抬 thr / 降 17/20；禁立刻 Δv″；禁 LOCK 前采数；禁静默 0.7511；禁翻盘七 hash（五 FAIL + TRUTH `276b97f0…4f69` + DENOM `d891df9f…33c7`）；≠PERF-CI；成功≠落地采用；结题主用 `H5-DENOM-VERIFY-*`；产物前缀 `m2_h5_denom_verify_*` only。

---

## 1) 验证采数范围拍板（可 LOCK）

| 项 | 写死 |
|---|---|
| **目的** | 在 **已 LOCK 的 `v_denom≡v_pre` 契约**下，另批新 calib 后，验证 **H5 比 + t060 绝对 smoke 可否同批齐**；**非**翻盘七 hash；**非** PERF-CI；**非**落地启用 |
| **格表** | **主：t060 fwd + t060 rev**（绝对 smoke）；**同批报 H5 比**（`r_i=\|v_seed/v_denom\|`，`med_r` / any `r_i≥2`） |
| **辅证（可选，不进 PASS）** | 旧 `r_2F`、`|v_denom−v_screen|`、`|ivel−v_pre|` 分位 |
| **会话数** | **≥3 独立进程**（建议 s1/s2/s3；进程间冷启；禁同进程伪独立） |
| **机器** | **单机** `DESKTOP-K93N3P8` · 60 Hz；**不开第二台**；**≠PERF-CI**（产物不得作 PERF-CI 合格证据） |
| **对照 KEEP** | basePass ∧ IVel0_t060_both_FAIL |
| **产物前缀** | `artifacts/m2_h5_denom_verify_*` / `docs/m2_h5_denom_verify_*` only；**不得**冒充 path_a / path_b / calib / abs / delta_v / truth / denom_revise 翻盘包 |

**采数时机**：**仅** LOCK + APPROVED 且 APPROVED 正文写死「验证采数=**是**」之后，由 workbuddy 执行；本 DRAFT / 本补洞 **不授权**采数。

---

## 2) 落地 = 否（默认）

| 字段 | 本票拍板 |
|---|---|
| **落地** | **否**（不改分母落地启用；不把探针默认切到新分母生产路径） |
| 验证探针 | 允许在 **验证会话**内按 prereg 使用 `v_denom≡v_pre` + 新 calib 计分；**≠** 默认落地采用 |
| 若将来落地 | **另票** LOCK+APPROVED，且 APPROVED **另字段**显式「落地=是」+ 边界 |

**PASS ≠ 已落地采用。**

---

## 3) calib 标定式拍板（禁静默 0.7511）

### 推荐标定式 **C1**

\[
\texttt{calib\_new} = \mathrm{median}\bigl(v_{\mathrm{denom}} / \hat{v}\bigr)
\]

对计入标定的有效样：Arm-WGC-seed（或等价写入路径）、`v_denom` 与 \(\hat{v}\) 均有限且 \(\hat{v}\neq 0\)、\(|\hat{v}|\ge\varepsilon\)（ε=5.0 KEEP）、同 QPC / 同 `t_int` 几何。

### 何时标定 / 用哪些样（写死流程）

1. **Phase-Calib（标定窗）**  
   - 在 LOCK+APPROVED 后，于验证机跑 t060 fwd+rev、≥3 独立进程。  
   - 估速侧：**P-LS+TS · N=5 · order=2 · V2-clamp `t_int`**（见 §4 路径 **A**）；**本阶段写入 IV 可用 `calib_probe=1.0` 仅用于露出 \(\hat{v}\)**，或直接从探针导出 \(\hat{v}\) **不依赖**旧 0.7511。  
   - 分母：严格 **`v_denom≡v_pre`**（二次 LS · w=5 · 同估点）。  
   - 汇总有效样算 **`calib_new = median(v_denom/v_hat)`**；落盘到 `artifacts/m2_h5_denom_verify_calib_*` 与 result 正文（写死 exact 小数）。

2. **Phase-Score（计分窗）**  
   - **冻结** `calib_new`（APPROVED 可写「标定式 C1 → 本批 exact=…」或预先授权「执行 C1 后冻结」）。  
   - **同一 prereg 路径 A** 下重跑或同批第二臂：`InitialVelocity = (float)(calib_new · v̂)`（若走 A′ 则为 `calib_new · v_s`——本票默认不走 A′）。  
   - 用冻结常量评 **H5 比 + smoke ≥17/20**；**禁止**计分窗再改 calib。

3. **禁令**  
   - **禁静默沿用** exact=`0.7510829456` / 显示 `0.7511`（旧 2F）。  
   - **禁**用 `median(v_screen/v_hat)` 或 `median(v_screen/ivel)` 冒充新分母标定。  
   - **禁**标定窗用旧 2F 分母、计分窗改称 `v_denom`。

### APPROVED 须声明

| 字段 | 须填 |
|---|---|
| calib | **「标定式 C1 = median(v_denom/v_hat)」** 或 **写死新常量 exact=…**（二者择一；禁空、禁「沿用 0.7511」） |
| 验证采数 | **是**（目的/格表/≥3 进程/单机） |
| 落地 | **否** |
| KEEP v_denom≡v_pre | 是 |
| 路径 | **PATHB-A**（见 §4） |
| 禁抬 thr / 降 17/20 | 是 |
| 七 hash 不翻盘 | 是 |
| 禁立刻 Δv″ | 是 |

---

## 4) KEEP 表与路径拍板

### 4.1 契约 / 门槛 KEEP

| 项 | 处置 |
|---|---|
| **`v_denom ≡ v_pre`** | **KEEP**（二次 LS · w=5 · 同 QPC · `t_int=min(t_comp,t_last_WGC)`）；单位 DIP/s |
| 比式 | `r_i=\|v_seed/v_denom\|`；旧 2F 仅辅证 |
| H5 比门槛 | `med_r ∈ [0.8,1.2]` 且无 `r_i≥2.0` |
| smoke | ≥**17/20**；`thr_i=max(20,0.05·\|v_ref,i\|)`；meas w=5；**禁抬 thr / 降 17/20** |
| N / order / ε | **KEEP** 5 / 2 / 5.0 |
| `t_int` | **KEEP** V2-clamp |
| composition.h | **KEEP** `a55d4952…e418`；不改 |
| 影子 IV | **禁** |

### 4.2 路径拍板：**推荐 KEEP PATHB-A（V2-clamp；P-VS=OFF）**

| 项 | 写死 |
|---|---|
| **主路径** | **`PATHB-A = V2-clamp`**（P-LS+TS · N=5 · order=2 · P-VS=**OFF** · `t_int` 钳制） |
| IV | `InitialVelocity = (float)(calib_new · v̂)` |
| **A′（P-VS α_v=0.30）** | **本票默认不启用**；仅当 Phase-Score 下 A 出现「比已进窗但 rev 尖峰导致 smoke 系统性灭、且归因证明需抑尖」时，**另票** prereg A′——**禁止**本票执行中偷开 |
| 理由 | 前序 A/A′ 在旧 2F+旧 calib 下均 CLOSED FAIL；TRUTH/DENOM 已把根因钉在**分母定义**而非平滑层。新分母 + C1 标定后应先验证**最小路径 A**；A′ 多 α 自由度，易与新 calib 纠缠、归因不清 |

**否决**：本票默认同跑 A+A′ 择优；执行中改 α；KEEP 旧 calib 却开 A′。

---

## 5) 七 hash 不翻盘（全文）

结题须逐条声明以下 **未改、未重跑、未宣称通过**：

1. PATH-A′ FAIL `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362`
2. PATH-B′ FAIL `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`
3. SEED-CALIB FAIL `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`
4. SEED-ABS FAIL `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`
5. Δv′ FAIL `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`
6. TRUTH gate `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69`
7. DENOM gate `d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7`

禁止把本票产物标为 `Δv-PASS` / 前序 `SEED-*-PASS` / `SEED-TRUTH-PASS` / 重写 `H5-DENOM-PASS` 为已落地。

---

## 6) 虚 PASS / INVALID 否决表

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
| 把 doc-only `H5-DENOM-PASS` / 本 DRAFT 当已授权采数 | INVALID |
| 执行中偷开 A′ / 改 α / 改 N·order·ε | INVALID |
| 计分窗未冻结 calib 又改标定 | INVALID |

### 结题标签 / 产物

- 结局 ∈ {`H5-DENOM-VERIFY-PASS`, `H5-DENOM-VERIFY-FAIL`, `H5-DENOM-VERIFY-INVALID`, `H5-DENOM-VERIFY-STOP`}  
- **PASS ≠ 已落地采用 ≠ Δv-PASS ≠ 七 hash 翻盘 ≠ 可立刻 Δv″**  
- 前缀：`m2_h5_denom_verify_*` only

---

## 7) 可 LOCK？

### **是 — 可 LOCK**

验证采数范围（t060 fwd+rev + H5 比；≥3 进程；单机 DESKTOP-K93N3P8；≠PERF-CI）、落地=否、标定式 **C1=`median(v_denom/v_hat)`**（Phase-Calib→冻结→Phase-Score）、路径 **PATHB-A**、KEEP 契约/门槛、七 hash、虚 PASS、标签与产物前缀均已写死。

### 最小修订点（LOCK 粘贴）

1. APPROVED 强制三字段：**验证采数=是**；**落地=否**；**calib=C1 或新常量 exact**（禁 0.7511）。  
2. 正文粘贴 §1 格表 + §3 C1 两阶段 + §4 路径=**PATHB-A**。  
3. 七 hash 全文不翻盘；禁立刻 Δv″；结题 `H5-DENOM-VERIFY-*`。

### G4 ONELINE（建议）

`M2-H5-DENOM-VERIFY | gate=<hash> @ <LOCKED_AT Asia/Taipei> | approved=<who>@<when> | mode=verify | ticket=H5-DENOM-VERIFY-* | KEEP v_denom≡v_pre；path=PATHB-A；calib=C1 median(v_denom/v_hat)；禁静默0.7511；验证采数=是；落地=否；七hash不翻盘；禁抬thr；成功≠落地采用≠Δv-PASS；≠PERF-CI；禁立刻Δv″；单机 DESKTOP-K93N3P8`

---

## 回执摘要（给协调）

| 项 | 值 |
|---|---|
| 落盘 | `docs/deepseek_m2_h5_denom_verify_review.md` |
| 可 LOCK | **是** |
| calib | **C1 = median(v_denom/v_hat)**（Phase-Calib 冻结后计分；禁静默 0.7511） |
| 路径 | **PATHB-A**（V2-clamp；P-VS=OFF）；A′ 默认不启用 |

— DeepSeek网页版（M2-H5-DENOM-VERIFY 补洞；Harness 空回代拟；文档 only）
