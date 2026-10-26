# DeepSeek · M2-H5-DENOM-REVISE 可 LOCK 补洞（新分母契约）

- 时间：2026-10-26（Asia/Taipei, UTC+8）
- 状态：**文档 only**；NOT LOCKED；**禁采数**；**禁改分母落地**
- **出处说明**：**Harness 空回代拟**；对照 `docs/m2_h5_denom_revise_gate_DRAFT.md`、`docs/notion_m2_seed_truth_conflict_next.md`、`docs/m2_seed_truth_conflict_result.md`（menu=M1）、PATH-B′/PATH-A′ 已归档证据起草。
- composition.h sha 锁：`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（未改、不改）
- **声明**：TRUTH `SEED-TRUTH-PASS` ≠ 已改 H5；本补洞 ≠ 落地执行。

## 禁令复述（文首）

不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；禁抬 thr / 降 17/20；禁立刻 Δv″；禁翻盘六 hash（五 FAIL + TRUTH `276b97f0…4f69`）；禁 LOCK 前采数 / 改分母落地；≠PERF-CI；成功≠采用；结题主用 `H5-DENOM-*`。

---

## 1) 新分母定义拍板（可 LOCK）

### 推荐方案 **D1 = 绝对同源连续分母**

| 项 | 写死 |
|---|---|
| **符号** | **`v_denom`**（H5 比回归分母；取代旧闸门角色上的 `v_screen`） |
| **定义** | **`v_denom ≡ v_pre`**：与绝对 Δv smoke **同一**打断前连续估速——对打断前 WGC 位置 `(t_k,x_k)` 做**二次 LS，窗宽 w=5**，在打断估点求速（与 smoke 侧 `v_pre` 同几何） |
| **估点** | 与 seed/`t_int` 对齐：优先 **`t_int = min(t_compositor_interrupt_QPC, t_last_WGC)`**（禁外推过末样）；`v_denom` 在该估点取值，**不得**另开第二时基 |
| **时基** | compositor / WGC **同一 QPC 原点**；禁异时基 |
| **单位** | **DIP/s** |
| **分子** | **`v_seed` = 实际写入 compositor 的 `InitialVelocity`**（即 pathA 下 `calib·v̂` 或 A′ 下 `calib·v_s`；与 raw `ivel_set` 一致） |
| **比式** | **`r_i = \|v_seed / v_denom\|`**（`v_denom=0` 或非有限 → 该样 INVALID/不计 PASS） |
| **旧 2F** | 旧 `v_screen`（末两帧差分）**仅辅证 log**（可报 `r_2F`）；**不再作**计入 PASS 的 H5 分母 |

**否决的替代（不作默认）**

| 候选 | 决断 | 理由 |
|---|---|---|
| 保持 2F `v_screen` 作分母 | 否 | TRUTH 已证与 `v_pre` 冲突 |
| 分母用 seed 窗 LS `v̂`（自比） | 否 | 易虚 PASS；与绝对 smoke 仍可不齐 |
| 分母用 `v_post` | 否 | 含 seed 后果；因果倒置 |
| M2：改 smoke 参照去就 2F | 否 | TRUTH 已否决；弱化绝对门 |

---

## 2) 比–绝对同源句（写死）

**比回归分母 `v_denom` 与 t060 绝对 smoke 的 `v_pre` 必须同一定义（二次 LS · w=5 · 同 QPC · 同打断估点）；`r_i=\|v_seed/v_denom\|` 与 smoke 的 `|v_post−v_pre|` 共享同一前打断连续真值。禁止「比钉旧 2F `v_screen`、绝对钉另一套 `v_pre`」。**

- smoke 门槛几何：**KEEP**——`thr_i=max(20,0.05·|v_ref,i|)`；二次 LS w=5；≥**17/20**；P1/P2。  
- **禁**为对齐而去改 thr 或把 smoke 参照改回 2F。

---

## 3) 回归门槛（禁抬 thr / 降 17/20）

| 门 | 门槛 | 备注 |
|---|---|---|
| H5 比（新） | 计入 PASS 的会话：`med_r ∈ [0.8,1.2]` 且无 `r_i≥2.0`（`r_i=\|v_seed/v_denom\|`） | 比出窗 → FAIL/INVALID；**不得** smoke 豁免 |
| 绝对 smoke | t060 fwd+rev：可判且格 PASS ≥**17/20** | thr / 17/20 / w=5 **相对旧票 KEEP；禁抬 thr、禁降 17/20** |
| 对照 | basePass ∧ IVel0_t060_both_FAIL | KEEP |
| 辅证 | 可报旧 `r_2F`、`|v_denom−v_screen|` | **不进** PASS 判据 |

---

## 4) KEEP 表与另批

| 项 | 处置 |
|---|---|
| composition.h | **KEEP** `a55d4952…e418`；不改 |
| 影子 IV | **禁** |
| N / order / ε | **默认 KEEP**（5 / 2 / 5.0）；改须 APPROVED **显式另批** |
| `t_int` 钳制 | **默认 KEEP** `min(t_comp,t_last_WGC)` |
| thr / 17/20 / meas w | **KEEP**；禁抬 / 禁降 |
| **calib** | **契约定义阶段不落地**；若 APPROVED 授权**验证/落地**，须在新分母下 **显式另批** calib（推荐公式仍 `median(v_denom/v_hat)` 或写死常量表）。**禁止**静默沿用 exact=`0.7510829456`（该值相对旧 2F）却宣称新契约比回归 PASS——旧 calib 钉 2F，换 `v_denom≡v_pre` 后预期 med_r 系统性下偏出窗 |
| P-VS / α_v | 默认不强制；若沿用 A′ 须另批写死 |
| 六 hash 不翻盘 | PATH-A′ `f17871f1…2362`；PATH-B′ `a1161c77…3a43`；CALIB `09f6414b…793d`；ABS `831818e4…80de`；Δv′ `06467b82…805c`；TRUTH `276b97f0…4f69` |

旧 H5-SEED（2F 契约）CLOSED PASS **归档保留**；本票是**新契约**，不重开/不宣称翻盘旧 H5-SEED。

---

## 5) 采数默认关

- **默认：不开采数；不改分母落地。**  
- 本票可先 **doc-only PASS**：契约全文完备即可能 `H5-DENOM-PASS`（mode=doc-only），**≠** 已在真机改分母。  
- 若需真机验证，APPROVED 须**显式写死**：

| 字段 | 须填 |
|---|---|
| 目的 | 新分母回归验证（非翻盘六 hash） |
| 格表 | t060 fwd+rev（+ 可选辅证） |
| 会话数 | ≥3 独立进程 |
| 路径/prereg | N/order/ε/`t_int`/是否 P-VS；**新 calib 常量或标定式** |
| 仍禁改 KEEP | composition.h；thr；17/20；六 hash；禁影子 IV |
| 产物前缀 | `m2_h5_denom_revise_*` only |

---

## 6) 虚 PASS / INVALID 否决表

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

---

## 7) 可 LOCK？

### **是 — 可 LOCK**

新分母 **D1=`v_denom≡v_pre`（二次 LS w=5 · 同 QPC · 同 `t_int`）** 已写死；比–绝对同源句、门槛、KEEP/另批 calib、采数关、虚 PASS、六 hash 齐。

### 最小修订点（LOCK 粘贴）

1. 正文粘贴 §1 常量表 + §2 同源句。  
2. 写死：验证/落地时 **calib 须另批**；禁静默 0.7511。  
3. 结局标签：`H5-DENOM-PASS|FAIL|INVALID|STOP`；doc-only PASS ≠ 落地。  
4. Δv″：**继续挂账**；须新契约下比+smoke 可齐后再另开。

### 结题标签

- 主用 **`H5-DENOM-*`**  
- 禁止：`Δv-PASS`；五 FAIL / TRUTH 翻盘标签  

### 推荐新分母一句话

**`v_denom ≡ v_pre`（与 t060 smoke 同窗同几何的打断前连续估速）作 H5 分母，废除旧 2F `v_screen` 作门控分母。**

— DeepSeek网页版（M2-H5-DENOM-REVISE 可 LOCK 补洞；Harness 空回代拟；文档 only）
