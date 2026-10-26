# DeepSeek · M2-SEED-TRUTH-CONFLICT 可 LOCK 补洞（诊断/决议）

- 时间：2026-10-26（Asia/Taipei, UTC+8）
- 状态：**文档/决议 only**；NOT LOCKED；**默认不开真机采数**
- **出处说明**：**Harness 空回代拟**；对照 `docs/m2_seed_truth_conflict_gate_DRAFT.md`、`docs/notion_m2_wgc_seed_path_a_prime_pack.md`、`docs/notion_m2_wgc_seed_path_a_prime_next.md`、`docs/m2_wgc_seed_path_a_prime_result.md`、`docs/m2_wgc_seed_path_b_prime_result.md` 独立核对。
- composition.h sha 锁：`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（未改、不改）

## 禁令复述（文首）

不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；禁翻盘五 FAIL `f17871f1…2362` / `a1161c77…3a43` / `09f6414b…793d` / `831818e4…80de` / `06467b82…805c`；禁立刻 Δv″；禁 LOCK 前采数；≠PERF-CI；成功≠采用；本票**不**预授权执行改 H5 分母 / 抬 thr / 降 17/20 / 再标定 / 再跑 α；结题主用 `SEED-TRUTH-*`。

---

## 1) 冲突根因陈述边界

### 1.1 核心句（写死）

**2F 比真值（H5 分母=`v_screen`=打断前末两帧 WGC）与绝对 `v_pre` / t060 smoke 门不一致**——在 KEEP calib + KEEP H5@2F 下，种子被钉在 ≈`v_screen`，而绝对连续测量 `v_pre` 系统性高出约 **50+ DIP/s**；比与 smoke **不可同票 PASS**。不得默认用抬 thr / 改分母执行 / 再标定 / 再跑 α 把任一 FAIL 闸门翻盘。

### 1.2 双路径证据（已归档；本票只引用）

| 票 | 路径 | H5 比 | t060 smoke | 对照 | 结局 |
|---|---|---|---|---|---|
| **PATH-B′** gate `a1161c77…3a43` | V2-clamp；P-VS=**OFF** | 三会话比过：med_r=**0.9465 / 0.8055 / 0.8062** | fwd **0/20×3**；rev **0/20**（含 1 SKIP） | baseP ∧ IVel0F ×3 | `SEED-PATHB-FAIL`；**首次** TRUTH-CONFLICT |
| **PATH-A′** gate `f17871f1…2362` | V2-clamp+P-VS α_v=**0.30** | s1 med_r=0.9289 **max_r=6.5997** any_ri≥2 → **比 FAIL**（cross-dir EMA）；s2/s3 med_r=**0.8527 / 0.8649** 比过 | fwd 1/0(SKIP)/0；rev **0/20×3**；0/3 格 PASS | baseP ∧ IVel0F ×3 有效 | `SEED-PATHA-PRIME-FAIL`；**保持** TRUTH-CONFLICT |

补充：

- 结构风险例：A′ s3 Δv=**75.577**（thr=20）；B′/CALIB 系谱均见 `|ivel−v_pre|≈50+`。  
- **平滑臂已耗尽**：预登记 A（P-VS=OFF）与 A′（P-VS=ON α_v=0.30）均 CLOSED FAIL。  
- **对照健全** → FAIL ≠ 测法崩坏；**结构风险下 FAIL 合法**。  
- s1 `max_r=6.5997`：跨向 EMA 尖刺，是 VS 副作用证据，**不是**「再调 α 就能同票过比+smoke」的信号。

### 1.3 本票边界（只决议，不采数主路径）

- 本票 = **诊断/决议**；**不是**再跑 seed；**不是** Δv″。  
- 勾选菜单 ≠ 授权执行改分母 / 抬 thr / 新估法落地。

---

## 2) 处置菜单核对 + 推荐主决议

| ID | 含义 | 本票可做 | 执行 | 优劣（相对冲突） |
|---|---|---|---|---|
| **M1** 授权改 H5 分母 | 另票定义新分母/估窗契约，使比真值与绝对连续同源 | 决议是否推荐 | **另票 LOCK** | **对准根因**；不动 thr/17/20；须重开 H5 契约另票，成本高但可解「比钉 2F、绝对钉 v_pre」 |
| **M2** 改 smoke 参照 | 另票改 `v_pre`/参照定义（**非**抬 thr） | 决议是否推荐 | **另票** | 易把绝对门弱化成「跟 2F 比」；产品绝对连续性变虚；**劣于 M1** |
| **M3** 新估法族 | 另票预登记新路径（非默认同跑 α） | 决议是否推荐 | **另票** | 在 KEEP H5@2F+calib 下仍撞同一结构墙；**平滑臂已证无效**；除非叠加 M1，否则重复 FAIL |
| **M4** STOP 挂账 | 主线暂停该类 seed 绝对对齐；冲突归档 | 可本票拍板 STOP | 本票可 `SEED-TRUTH-STOP` | 证据已齐却无决议出口；next 已否「本轮默认 STOP」 |
| **M5** 维持现状继续挂账 | 不改定义；等外因 | 可本票拍板 | 无执行改动 | 不收敛；与「升主线决议」相悖 |

### **推荐主决议：M1**

**拍板倾向 = M1（授权另票改 H5 分母 / 比–绝对同源契约）。**

理由一句：双路径 + calib + V2-clamp + P-VS 已穷尽「KEEP H5@2F」下的 seed 修法；残差是**真值定义冲突**，不是未试平滑参数；M2 弱化绝对门，M3 单干重复墙，M4/M5 不决议。

- 本票若 LOCK 后结题勾选 M1 → 标签倾向 **`SEED-TRUTH-PASS`**（陈述完备 ∧ 主决议写死 ∧ 五 FAIL 不翻盘 ∧ 未违规执行）。  
- **下一条另票占位（建议名）**：`M2-H5-DENOM-REVISE`（或等价）——须新闸门写死：新分母定义、与 t060/`v_pre` 对齐规则、回归门槛、禁抬 thr、禁翻盘五 FAIL。  
- **本票明确未执行**任何改分母 / 抬 thr / 改 composition.h。

并列说明：若 Notion 拒 M1，次选 **M4 STOP**（显式归档；仍禁翻盘/立刻 Δv″）；**不推荐** M2/M3/M5 作主决议。

---

## 3) 虚 PASS / 偷换否决表

| 行为 | 处置 |
|---|---|
| 本票内抬 thr / 降 17/20 | INVALID |
| 本票内**执行**改 H5 分母（勾选 M1 ≠ 执行） | INVALID |
| 再标定 / 再跑 α / 默认同跑新估法偷换 A′ 或 PATH-B′ | INVALID |
| 重跑/宣称通过五 FAIL gate；产物标前序 `SEED-*-PASS` / `Δv-PASS` | INVALID |
| 立刻开 Δv″ | 否决 / INVALID |
| 未 LOCK+APPROVED 采数；或 APPROVED 未写死范围却采数 | INVALID |
| 把「比过」写成任一 FAIL 闸门已翻盘 PASS | INVALID |
| 产物作 PERF-CI；开 M3；解冻 Bv；改 composition.h；影子 IV | INVALID |

---

## 4) 默认不开采数

- **默认：不开。**  
- 例外：仅当 APPROVED 正文**显式**写死——采数目的、格表、会话数、仍禁改的 KEEP 表——方可派 workbuddy。  
- 采数不得用于翻盘五 FAIL；产物前缀仅 `m2_seed_truth_*`。  
- 推荐主路径（M1）**不依赖**本票采数。

---

## 5) 可 LOCK？

### **是 — 可 LOCK**

DRAFT 已具备：冲突陈述框架、菜单 M1–M5、默认不开采数、五 FAIL 不翻盘、结局 `SEED-TRUTH-*`、虚 PASS。本补洞补齐证据边界与**主决议倾向 M1**。

### 最小修订点（LOCK 时粘贴；非挡 LOCK）

1. 正文写死主决议默认倾向：**M1**；下一条另票占位名（建议 `M2-H5-DENOM-REVISE`）。  
2. 结题模板强制含：双路径证据表 + A′ s1 `max_r=6.5997` + s2/s3 med_r +「平滑臂耗尽」+「结构风险 FAIL 合法」+「本票未执行改分母/抬 thr」。  
3. 明确：`SEED-TRUTH-PASS` ≠ 已改 H5 ≠ 采用 ≠ Δv-PASS。

---

## 6) 结题标签

- **`SEED-TRUTH-PASS`** / **`SEED-TRUTH-FAIL`** / **`SEED-TRUTH-INVALID`** / **`SEED-TRUTH-STOP`**  
- 禁止：`Δv-PASS`；五 FAIL 的 `SEED-*-PASS` 翻盘标签  

---

## 明确声明

成功 ≠ 采用 ≠ Δv-PASS；Δv″ 继续挂账（冲突决议且后续路径可过比+smoke 后再开）；PERF-CI HOLD；不开 M3；旁路不挡不清。

— DeepSeek网页版（M2-SEED-TRUTH-CONFLICT 可 LOCK 补洞；推荐主决议 **M1**；Harness 空回代拟；文档 only）
