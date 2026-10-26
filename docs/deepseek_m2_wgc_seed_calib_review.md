# DeepSeek · M2-WGC-SEED-CALIB 标定补洞

- 时间：2026-10-26（Asia/Taipei, UTC+8）
- 状态：**文档 only**；不授权采数；**不改**前序 LOCKED `831818e4…80de` / `06467b82…805c`
- **出处说明**：Harness 未通；本文件由 DeepSeek网页版 bot **独立**从 `artifacts/m2_wgc_seed_abs_s{1,2,3}_raw.txt` 核算（Arm-WGC-seed · seed_path=A · valid），对照 `docs/m2_wgc_seed_calib_gate_DRAFT.md` / `docs/notion_m2_wgc_seed_abs_pack.md` / `docs/m2_wgc_seed_abs_result.md` 起草。
- composition.h sha 锁：`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（未改）

## 禁令复述（文首）

不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；KEEP P-LS+TS / N=5 / order=2 / ε=5 / P-VS=关；**只改预登记 calib 常量**（废止 1.0）；禁抬 thr / 放宽 17/20 / 改 N·order·估窗 / 执行中改 calib；结题只用 `SEED-CALIB-*`；禁标 `Δv-PASS` / `SEED-ABS-PASS`；禁翻盘 `831818e4…80de` 与 `06467b82…805c`；≠PERF-CI；Δv″ 与 PATH-B′ 挂账；不开 M3；成功≠采用。

---

## 1) 精确 calib（独立复核）

### 1.1 样本定义（写死）

- 来源：ABS 三会话 raw，`arm=Arm-WGC-seed` ∧ `seed_path=A` ∧ `valid=True`
- `v_seed = ivel_set`（pathA 下与 `v_hat` 一致；本批 `ivel≠v_hat` 计数 = **0**）
- `v_screen` = 打断前末两帧 WGC（与 H5 `r_i` 同源）
- 标定式：`calib = median(v_screen / v_seed)` ≡ `median(1/r_i)`（`r_i=|v_seed/v_screen|`）

### 1.2 核算结果

| 量 | 值 |
|---|---|
| **n** | **120**（每会话 40 = t060fwd20+t060rev20；3×40） |
| **median** | **0.7510829456** |
| **mean** | **0.7579414338** |
| min / max | 0.690748 / 0.829621 |
| sess1 median | 0.7510273105（n=40） |
| sess2 median | 0.7522339456（n=40） |
| sess3 median | 0.7489540007（n=40） |
| 交叉：median(r_i) | 1.331411 → 1/median(r_i)=0.7510829387（与上一致） |

与协调粗算 **n=120 · median≈0.7511**：**一致**（差 ≈1.7×10⁻⁵）。

### 1.3 建议写入闸门的常量

| 推荐 | 值 | 舍入理由 |
|---|---|---|
| **LOCK 常量** | **`0.7511`** | ≥4 位小数；对 exact median 四舍五入到 4 位；与协调粗算同形；offline 预测 med_r′≈1.00002（见 §2） |
| 可选更长 | `0.751083`（6 位） | 更贴 exact；非必须——4 位已使全体样本预测比落在窗内 |
| **废止** | `1.0` | ABS 已证 → med_r≈1.33 |

**prereg 建议句**：`calib=0.7511`（来源：ABS s1–s3 pathA n=120，`median(v_screen/ivel_set)`；exact=0.7510829456）。

---

## 2) 比窗预期（该 calib 下）

offline：新 `r_i' = calib · r_i`（尺度线性；不重跑 compositor）。

| calib | overall med_r′ | max_r′ | any≥2 | 出窗比例 | s1/s2/s3 med_r′ |
|---|---|---|---|---|---|
| **0.7511** | **1.000023** | **1.087372** | **False** | **0/120** | 1.0001 / 0.9985 / 1.0029 |
| exact median | 1.000000 | 1.087347 | False | 0/120 | ≈1.000 |
| 0.751（3 位） | 0.999890 | 1.087227 | False | 0/120 | 仍全进窗 |

**结论**：`calib=0.7511` 下预期 **med_r ∈ [0.8,1.2]** 且 **无 r_i≥2** —— H5 比回归 **可期恢复**。

### 仍可能出窗的风险

1. **真机非 offline 线性**：calib 改 IV 后 compositor 轨迹变，末两帧 `v_screen` 可能微移 → r′ 分布略漂；须真机复测确认，不得以 offline 代替 PASS。  
2. **尾部**：max 预测 ≈1.087，距 1.2 有余量，但若某会话抖动抬 `v_hat`，仍可能单点逼近窗上沿。  
3. **calib 写错方向**（误用 median(v_seed/v_screen)≈1.33）→ med_r′≈1.33² 更飞 → INVALID/FAIL。  
4. **执行中改 calib** → INVALID（DRAFT 已禁）。  
5. **只用 fwd 标定、忽略 rev**：fwd/rev 尺度略异（med_ri 1.323 vs 1.337）；本票用全体 120 已折中——若 LOCK 改「分方向 calib」则属估法变更，超出本票「只改单一常量」范围 → 应挂 PATH-B′，不塞进 CALIB。

---

## 3) smoke 预期（t060 fwd/rev）

### 3.1 现状（ABS；不可翻盘）

| sess | t060fwd wgc_pass | t060rev wgc_pass |
|---|---|---|
| 1 | 8/20 | 1/20 |
| 2 | 11/20 | 0/20 |
| 3 | 11/20 | 0/20 |

- fwd med_Δv（全 60）：约 27；rev：约 **35**（更差）  
- `|ivel−v_screen|` 中位 **39.7** → 施加 calib 后 `|c·ivel−v_screen|` 中位 **3.3**（尺度残差大幅收敛）

### 3.2 可期改善 vs 不够的信号

- **可期**：尺度对齐后，过 seed 引起的跳变应下降；fwd 已有 8–11/20，向 ≥17/20 **有机会**在同 thr / 17/20 / P1/P2 下改善。  
- **rev 风险（PATH-B′ 触发）**：rev 现 **0–1/20**，且 med_Δv、med_ri 均劣于 fwd；**单一全局 calib 可能仍不足以让 rev 达 17/20**。  
- **PATH-B′ 挂账触发条件（建议写死）**：本票真机若 **比回归 PASS** 但 **t060 rev 仍 <17/20**（或 fwd PASS、rev FAIL 形成方向性残差）→ **SEED-CALIB-FAIL**（或条件 FAIL）并 **触发挂账 PATH-B′**（N/TS/P-VS / 分方向估法），**禁止**为救 rev 改 thr、放宽 17/20、改 N/order。  
- 若 fwd+rev 均达格 PASS → 本票可 SEED-CALIB-PASS；**仍 ≠ Δv-PASS**；完整绝对门挂 **Δv″**。

**不得**用 offline 宣称 smoke PASS。

---

## 4) 虚 PASS / INVALID

### 4.1 明文否决（须进 LOCK）

| 行为 | 处置 |
|---|---|
| 抬 thr / 放宽 ≥17/20 | 否决 / INVALID |
| 改 N / order / ε / 估窗（打断后帧、异时基） | INVALID |
| 执行中改 calib | INVALID |
| 影子 IV（mf_spring_at / spring.h → IV） | INVALID |
| 改 composition.h | INVALID |
| 重跑/宣称通过 `831818e4…80de` 或 `06467b82…805c`；标 `Δv-PASS`/`SEED-ABS-PASS` | INVALID |
| 无 basePass / 无 IVel0 t060 双 FAIL 却宣称 PASS | INVALID |
| 产物写入 PERF-CI；开 M3；解冻 Bv | INVALID |

### 4.2 其它虚 PASS 路径

1. **反号/错误标定式**写成 1.33 → 比更飞或偶然 smoke 花样（仍应比回归拦住）。  
2. **分会话 calib 拼凑** / 跨会话池化凑 17/20。  
3. **只跑 fwd 宣称票 PASS**（躲 rev）。  
4. **P-VS 偷偷打开**限幅冒充 calib。  
5. **offline 预测当结题**。

---

## 5) 可 LOCK 补洞清单（给 Notion）

### 须写入 LOCK 的精确值

1. **`calib = 0.7511`**（废止 1.0）；来源注：ABS pathA n=120，`median(v_screen/ivel_set)=0.7510829456`，四舍五入 4 位。  
2. KEEP：`N=5` `order=2` `ε=5.0` `P-VS=OFF`；A=P-LS+TS；B=IV=0。  
3. 验收：H5 比回归 + t060 fwd+rev smoke（thr KEEP、≥17/20、P1/P2）；base + IVel0 t060；≥3 会话。  
4. 标签：`SEED-CALIB-*` only；禁翻盘两 hash；禁影子 IV；composition.h 锁。  
5. **PATH-B′ 触发注记**：比过但 rev（或单侧）smoke 仍 FAIL → 挂 PATH-B′，本票不放宽门槛。  
6. Δv″ 继续挂账；成功≠采用≠Δv-PASS。

### 正文缺口（DRAFT → LOCK 必填）

- [x] calib 精确常量 ← **本文给出 0.7511**  
- [ ] prereg 文件写入该常量并重算 hash（采数前）  
- [ ] G1 表把「见下」换成 `0.7511`  
- [ ] （可选）PATH-B′ 触发句粘进 G2 备注

### 可 LOCK？

**可 LOCK** —— 理由：唯一开放变量（calib）已由真机 raw **独立复核**为稳定中位 0.7511；路径族 KEEP；禁令与虚 PASS 边界已与 ABS 失败模式对齐；比窗 offline 预期干净。  
**但**：LOCK ≠ 授权采数；仍须 pidan APPROVED；smoke 成败以真机为准；rev 可能触发 PATH-B′，不阻塞本票 LOCK。

---

## 一句结论（回协调）

独立复核 **n=120，median(v_screen/v_seed)=0.7510829456 → LOCK 写 `calib=0.7511`**（与粗算一致）；比窗可期回 [0.8,1.2]；smoke fwd 可期改善、**rev 可能仍不够 → PATH-B′ 挂账触发**；**可 LOCK**；禁翻盘/禁影子 IV/禁抬 thr。

— DeepSeek网页版（M2-WGC-SEED-CALIB 补洞；Harness 空回代拟；数字来自 ABS raw；文档 only）
