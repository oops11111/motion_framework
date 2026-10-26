# DeepSeek · M2-WGC-SEED-PATH-B′ 变体设计（估窗/TS/P-VS）

- 时间：2026-10-26（Asia/Taipei, UTC+8）
- 状态：**文档 only**；NOT LOCKED；不授权采数
- **出处说明**：**Harness 空回代拟**；本文件由 DeepSeek网页版 bot 独立依据 `docs/m2_wgc_seed_path_b_prime_gate_DRAFT.md`、`docs/notion_m2_wgc_seed_calib_pack.md`、`docs/notion_m2_wgc_seed_calib_next.md`、`docs/m2_wgc_seed_calib_result.md`、`artifacts/m2_wgc_seed_calib_s{1,2,3}_raw.txt`、`docs/deepseek_m2_wgc_seed_abs_path_design.md` 起草。数字均可核。
- composition.h sha 锁：`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（未改、不改）

## 禁令复述（文首）

不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；**KEEP calib exact=`0.7510829456`**（除非变体显式要求并预登记）；禁抬 thr / 放宽 17/20 / 偷换估窗（打断后帧、异时基）；禁只修一侧却放宽另一侧门槛；禁翻盘 `09f6414b…793d` / `831818e4…80de` / `06467b82…805c`；结题只用 `SEED-PATHB-*`；禁标 `Δv-PASS` / `SEED-ABS-PASS` / `SEED-CALIB-PASS`；≠PERF-CI；Δv″ 继续挂账（本票过比+smoke 前不立刻开）；不开 M3；成功≠采用。前序 CALIB CLOSED SEED-CALIB-FAIL；PATH_B_PRIME=HUNG（G2-LOCK-2b）。

---

## 1) 根因一句话（估窗/相位；可用 raw；禁 offline 当结题）

**calib 已把 ivel 钉在 2F `v_screen`（H5 比过），但绝对 smoke 的连续 `v_pre`（二次 LS w=5）仍 ≈170 DIP/s，|ivel−v_pre| 中位 ≈50 → 种子相对绝对连续态系统性欠速，打断后 `v_post` 跳变（rev 尤甚）——比过而 smoke 灭；残差在估窗/相位与「2F 比真值 vs 绝对连续真值」冲突，超出单标量 calib。**

### 1.1 可核数字（CALIB s1–s3 · Arm-WGC-seed · valid；DESKTOP-K93N3P8）

| 格 | n | med ivel | med v_hat | med v_screen | med v_pre | med \|ivel−v_screen\| | med \|ivel−v_pre\| | med Δv | pass(Δv≤20) |
|---|---|---|---|---|---|---|---|---|---|
| t060fwd | 60 | 116.4 | 154.9 | 117.7 | 169.5 | **3.0** | **49.3** | 59.0 | **5/60** |
| t060rev | 60 | 120.6 | 160.5 | 120.2 | 169.8 | **2.5** | **50.0** | 52.8 | **0/60** |

对照（同批）：

| 票 | fwd \|ivel−v_pre\| | fwd pass | rev pass | H5 med_r |
|---|---|---|---|---|
| ABS（calib=1.0） | **≈9** | 30/60（会话 8–11/20） | 1/60 | ≈1.33 **出窗** |
| CALIB（exact 0.751…） | **≈49** | 5/60（会话 1–3/20） | **0/60×3** | ≈0.99–1.00 **进窗** |

- 比：`r_i≈1`（ivel≈v_screen）→ ratioOk。  
- 绝对：欠速 ≈50 DIP/s ≫ thr=20 → 格灭；rev 侧 `med(v_post−v_pre)≈+51`、下冲比例仅 ≈0.18（更易上冲失控）。  
- base 仍格 PASS（fwd 60/60；rev 58/60）→ **非测法崩坏**。  
- **禁止**把上表 offline/复盘数字当本票 PASS。

---

## 2) 变体拍板：默认主路径 A + 可选 A′

### 2.1 默认主路径 **A = V2（P-TS 估点钳制）为主，附着轻量 V1 采样规则**

| 项 | 预登记值 |
|---|---|
| 族 | **P-LS+TS**（保持） |
| 变体 ID | **`PATHB-A = V2-clamp`**（主） |
| `N` | **5**（KEEP） |
| `order` | **2**（KEEP） |
| `calib` | **exact=`0.7510829456`**（显示 0.7511；**KEEP**；执行中不改） |
| `ε` | **5.0 DIP/s**（KEEP） |
| `P-VS` | **OFF**（主路径 A） |
| 单位 | DIP/s |
| 回退 B | IV=null+0；票 PASS 只评 A |
| **`t_int` 定义（本票相对 CALIB 的核心 diff）** | **`t_int = min(t_compositor_interrupt_QPC, t_last_WGC)`**，其中 `t_last_WGC` = 打断前最后一帧单调 WGC 样点 QPC；**在 `t_int` 取 `v̂=b+2c·t_int`，且禁止用打断后帧；若 `t_compositor > t_last_WGC`，不得外推超过 `t_last_WGC`（钳制到末样，等价于在末样求速）** |
| 时基 | 仅 compositor / WGC **同一 QPC 原点**；禁异时基 |
| 采样对齐 | 仅 **打断前** 末 N 帧；帧不足 / 非单调 / NaN / `|v̂|<ε` → B |

**机制意图（非结题）**：保持尺度（KEEP calib → 比窗可维持），只修「compositor 打断时刻相对末 WGC 样点」的半帧级外推相位，减少错误外推加速度灌进 IV；不碰 N/order，避免无必要重标定。

### 2.2 可选分级 **A′ = A + V3 轻量 P-VS**（仅当 A 预登记失败模式见 §4 后启用；须**另**写进 LOCK 或第二 prereg，不得执行中偷开）

| 项 | 预登记值 |
|---|---|
| 变体 ID | **`PATHB-A′ = V2-clamp + V3-VS`** |
| 继承 A | N/order/calib/ε/t_int 钳制 **全部同 A** |
| `P-VS` | **ON** |
| `α_v` | **0.30**（对 `v̂` 做一阶 EMA：`v_s = α_v·v̂ + (1−α_v)·v_s_prev`；会话内打断序列；首样 `v_s=v̂`） |
| 限幅 `v_max` | **不设**（禁另开口径；靠 α 抑尖峰） |
| 写入 IV | `InitialVelocity=(float)(calib·v_s)`（仍 KEEP calib） |

**A′ 定位**：抑 rev 侧 `v_post` 上冲尖峰；**不得**用限幅假造比窗；若 A′ 使 med_r 出窗 → FAIL/INVALID（视是否预登记），**禁止**执行中改 α/calib。

### 2.3 相对 CALIB 的 diff 表

| 参数 | CALIB（CLOSED FAIL） | PATH-B′ **A** | PATH-B′ **A′** |
|---|---|---|---|
| 族 | P-LS+TS | 同 | 同 |
| N / order | 5 / 2 | **KEEP** | **KEEP** |
| calib | exact 0.7510829456 | **KEEP** | **KEEP** |
| ε | 5.0 | KEEP | KEEP |
| P-VS | OFF | **OFF** | **ON · α_v=0.30** |
| t_int | compositor QPC（可外推过末样） | **钳制 `min(t_comp, t_last_WGC)`，禁过后外推** | 同 A |
| 估窗帧 | 打断前 N=5 | KEEP；**显式禁打断后帧** | 同 A |
| 测量 / thr / 17/20 | w=5；max(20,5%\|v_ref\|)；≥17/20 | **KEEP（禁改）** | KEEP |
| 结题标签 | SEED-CALIB-* | **SEED-PATHB-*** | SEED-PATHB-* |

---

## 3) 为何不选其它（否决与风险）

| 候选 | 决断 | 理由 |
|---|---|---|
| **V1 改 N∈{3,7} / order=1 作默认** | **否（默认）** | 改估速分布会立刻拧动 `v_hat`；在 **KEEP calib** 下极易把 med_r 推出 [0.8,1.2]（例：order→1 使 v_hat≈v_screen 时 ivel≈0.75·v_screen → r≈0.75 触下沿）。若将来要改 N/order，须**变体显式重登记 calib**——本票默认不做。 |
| **V3 作默认主路径** | **否** | 多一层 α 调参；比窗与绝对同时漂移难归因；降为 **A′**。 |
| **V4 分方向估法作默认** | **否** | fwd 亦仅 1–3/20（非「只剩 rev」）；分方向易滑向「只修一侧」；闸门禁单侧放宽。诊断可挂账，不作本票 A。 |
| **再标定 / 废止 exact** | **否** | med_r 已 ≈1.00；ABS→CALIB 已证尺度到位；再标定与 G2-LOCK-2b 挂 PATH-B′ 相悖。 |
| **立刻 Δv″** | **否** | 比过但 smoke 未齐。 |
| **打断后帧 / 异时基** | **禁** | INVALID。 |
| **影子 IV / 改 composition.h** | **禁** | INVALID。 |
| **抬 thr / 降 17/20** | **禁** | 虚 PASS。 |
| **P-MF 替换 LS** | **否（本票）** | 与测量二次族更远；ABS 设计已否决为默认。 |

**结构风险（须进 Notion 视野，仍可 LOCK 做受控票）**：H5 分母=2F `v_screen` 强制 ivel≈120，而绝对 `v_pre≈170`；**KEEP calib + KEEP H5 窗** 下，幅度缺口 ≈50 DIP/s **无法靠标量再抹平**。A/A′ 只赌相位/尖峰；**成功概率审慎偏低**，FAIL 仍是合法结题（禁翻盘、禁改 thr）。

---

## 4) 比窗 + smoke 预期（KEEP calib；禁 offline 当结题）

| 项 | A（V2-clamp）预期 | A′（+P-VS α=0.30）预期 | 失败模式 |
|---|---|---|---|
| med_r / max_r | **可维持** ∈[0.8,1.2]、无 r_i≥2（尺度未改；相位微扰） | 中位大致可维持；α 过大则欠 seed → 触下沿 | 出窗 → FAIL（不得 smoke 豁免） |
| t060 fwd | **可期略改善**（减错误外推）；从 1–3/20 向中段靠拢；**不承诺 ≥17/20** | 尖峰抑制或再抬一点 | 仍 ≪17 → 票 FAIL |
| t060 rev | **改善有限**；0/20 可能仍近 0；上冲若来自欠速换轨而非外推，V2 不够 | **相对 A 更值得一试**（抑 v_post 上冲） | 持续 0–低位 → FAIL |
| 结构墙 | 若 \|ivel−v_pre\| 仍 ≈50 | 同 | **PATH-B′ FAIL 后挂账**见下 |

**PATH-B′ 后再挂什么（写死建议）**

1. **Δv″**：仅当本票 **比回归过 ∧ t060 fwd+rev smoke 格 PASS** 后另开新闸门；否则 **继续挂账、不立刻开**。  
2. 若本票 **比过 ∧ smoke 仍双灭**（重复 G2-LOCK-2b 形态）→ 挂账 **`SEED-TRUTH-CONFLICT`（名可微调）**：2F 比真值 vs 绝对 `v_pre` 冲突；**禁止**再开 calib 废止票当主线；**禁止**抬 thr。处置归 Notion（可能含 H5 分母修订讨论——**本票不预授权**）。  
3. M2-VEL-SMOOTH 并行真机：**否**（已并入 A′）。  
4. PERF-CI / M3：**仍 HOLD / 不开**。

---

## 5) 虚 PASS / INVALID 清单

| 行为 | 处置 |
|---|---|
| 抬 thr / 放宽 ≥17/20 | 否决 / INVALID |
| 偷换估窗：打断后帧、异时基、未预登记改 N/order/ε/calib/t_int/P-VS | INVALID |
| 影子 IV（mf_spring_at / spring.h → compositor IV） | INVALID |
| 改 composition.h（≠ `a55d4952…e418`） | INVALID |
| 重跑/改写/宣称通过 `09f6414b…793d` / `831818e4…80de` / `06467b82…805c` | INVALID |
| 产物标 `Δv-PASS` / `SEED-ABS-PASS` / `SEED-CALIB-PASS` | INVALID |
| 只修 fwd 或只修 rev 却放宽另一侧门槛 | 否决 / INVALID |
| 无 basePass / 无 IVel0 t060 双 FAIL 却宣称 PASS | INVALID |
| offline 预测 / 复盘表当结题 PASS | INVALID |
| 跨会话池化凑 17/20；FAIL 后补测拼凑翻盘 | INVALID |
| 本票产物作 PERF-CI 合格证据；开 M3；解冻 Bv；重开 S1/R-3/R-2 | INVALID |
| 执行中把 A 偷换成 A′（或改 α）未入 prereg | INVALID |

---

## 6) 可 LOCK 补洞清单（给 Notion）

### 6.1 须写入 LOCK 的精确参数（默认 A）

```
path = PATHB-A = V2-clamp
family = P-LS+TS
N = 5
order = 2
calib_exact = 0.7510829456
calib_display = 0.7511
eps = 5.0
P-VS = OFF
t_int = min(t_compositor_interrupt_QPC, t_last_WGC_before_interrupt)
extrapolation = forbidden beyond t_last_WGC; no post-interrupt frames
fallback_B = IV null+0
smoke = t060 fwd + t060 rev
measure = quad LS w=5; thr=max(20,0.05*|v_ref|); >=17/20; P1/P2 KEEP
H5 = med_r∈[0.8,1.2] ∧ no r_i≥2 (v_screen=last-two WGC)
controls = basePass ∧ IVel0_t060_both_FAIL
sessions >= 3; machine = DESKTOP-K93N3P8; 60Hz
labels = SEED-PATHB-* only
```

可选：LOCK 同文预留 A′ 块（`P-VS=ON; α_v=0.30`），注明 **默认跑 A；A′ 仅当 Notion 显式 APPROVED 第二臂或 A 结题后另票**——推荐 **本票 LOCK 只锁 A**，A′ 作挂账次选，避免双臂并行真机。

### 6.2 必改正文缺口（相对当前 DRAFT）

1. 把 V1–V4「占位」替换为上表 **A 常量表**（及是否收录 A′）。  
2. 写死 `t_int` 钳制句与「禁打断后帧 / 禁异时基」。  
3. 文首/结局标签：`SEED-PATHB-*`；三闸门不翻盘哈希列表含 **`09f6414b…793d`**。  
4. 结构风险一句：KEEP calib + H5@2F 下 \|ivel−v_pre\|≈50 可能使 smoke 仍 FAIL——**FAIL 合法，禁抬 thr**。  
5. PATH-B′ 后再挂：`Δv″` 条件开；若再双灭 → `SEED-TRUTH-CONFLICT` 挂账（名称可改）。

### 6.3 结论

**可 LOCK**（默认 **A=V2-clamp**；参数已写死；虚 PASS/禁令齐全；不改 composition.h；KEEP calib；不授权在 LOCK+APPROVED 前采数）。

**不可 LOCK 的情形**（若同伴改稿踩中则打回）：未写死 t_int；默认同锁未隔离的 V4 单侧；默认改 N/order 却仍写 KEEP calib 且无比窗重证计划；删掉 H5 比回归；把 A′ 当未预登记可执行中开关。

---

## 7) 明确声明

- 本票成功 ≠ 采用 ≠ Δv-PASS ≠ SEED-ABS-PASS ≠ SEED-CALIB-PASS  
- 不改已 LOCKED：`09f6414b…793d` / `831818e4…80de` / `06467b82…805c`  
- ≠PERF-CI（HOLD `5b007dd4…282a`）；不开 M3  
- 旁路挂账不挡、本票也不清旁路  

— DeepSeek网页版（M2-WGC-SEED-PATH-B′ 变体设计；Harness 空回代拟；文档 only）
