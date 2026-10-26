# M2 下一票决断（M2-WGC-SEED-ABS CLOSED SEED-ABS-FAIL 后 · PERF-CI HOLD）

> DECISION_AT=2026-10-26 11:57:00 Asia/Taipei  
> 依据：`docs/notion_m2_wgc_seed_abs_pack.md`；`docs/m2_wgc_seed_abs_result.md`  
> 本票 gate `831818e4…80de` **不改、不重跑**；前序 `06467b82…805c` **不翻盘**

## 结论

**选 A = M2-WGC-SEED-CALIB。**  
DRAFT：`docs/m2_wgc_seed_calib_gate_DRAFT.md`（**NOT LOCKED**；不授权采数）。

**一句话为什么**：本票 P-LS+TS/N=5 已触达且对照健全，残差是 calib=1.0 下种子相对 `v_screen` 系统偏大（med_r≈1.33）导致比回归与 smoke 双 FAIL——**KEEP 路径族，只开标定票**；Δv″ 继续挂账到 seed 可过回归后再开。

## 现状

| 项 | 状态 |
|---|---|
| M2-Δv | CLOSED INCONCLUSIVE；`d2349ceb…09c6` 不改 |
| M2-Δv′ | CLOSED FAIL；`06467b82…805c` 不改、不重跑翻盘 |
| M2-WGC-SEED-ABS | **CLOSED SEED-ABS-FAIL**；`831818e4…80de` 不改、不重跑翻盘 |
| M2-PERF-CI | HOLD；不降级 |
| M2-01 / H5 / M2-02 | CLOSED PASS |
| M3 | 不开真机 |

## 候选速判

| 选项 | 决断 | 理由 |
|---|---|---|
| **A M2-WGC-SEED-CALIB** | **采纳** | med_r≈1.33 系统尺度偏；KEEP P-LS+TS；新闸门只改 calib 常量 |
| B PATH-B′（N/TS/P-VS） | 挂账 | rev 0–1 vs fwd 8–11 的方向性偏置：**先**用 calib 收尺度后再验；不足再开 |
| C 立刻 M2-Δv″ | **否** | 比回归未过；绝对门复测须 seed 后新闸门 |
| D 重跑 SEED-ABS / Δv′ FAIL | **否** | 禁翻盘 |
| E PERF-CI / 开 M3 | **否** | HOLD；不开 M3 |

## 可 LOCK 修订要点（给同伴 / Notion）

### 路径 KEEP

- **A = P-LS+TS**：打断前末 **N=5** 二次 LS @ `t_int`（QPC）；`InitialVelocity=(float)(calib·v̂)`  
- **回退 B** = IV null+0；票 PASS 只评 A  
- **ε=5.0**；**P-VS 默认关**  
- **禁**改估窗几何伪装 PASS（打断后帧、异时基、偷换 N/order）

### calib 怎么取（写死）

- 旧默认 calib=1.0 **废止为本票预登记值**（本票已证 1.0 → med_r≈1.33）。  
- **建议标定式**（预登记常量，禁执行中改）：  
  `calib := median(v_screen / v_seed)` 取自本票三会话 pathA 全体可计入样本  
  （等价约 `1/med_r ≈ 1/1.33 ≈ 0.75`；LOCK 前用 result/raw 算出精确常数并写入闸门）。  
- 允许协调指定等价 offline/真机标定式（LS 残差回归等），但须：  
  1) 仍是**单一常量** calib；  
  2) 预期把 H5 `med_r` 拉回 `[0.8,1.2]`；  
  3) **禁止**用抬 thr / 降 17/20 / 偷换估窗伪装 PASS。

### 验收 KEEP（相对本票）

- smoke=**t060 fwd+rev**；二次 w=5；`thr=max(20,0.05·|v_ref|)`；≥**17/20**；可判 P1/P2  
- H5 比回归：`med_r∈[0.8,1.2]` 且无 `r_i≥2`（必要）  
- 对照：basePass + IVel0_t060_both_FAIL  
- ≥3 独立进程；单机 DESKTOP-K93N3P8 / 60 Hz  
- 结题标签：`SEED-CALIB-PASS|FAIL|INVALID`（**≠** `SEED-ABS-*` 翻盘；**≠** Δv-PASS）

### 否决虚 PASS

| 反例 | 否决 |
|---|---|
| 影子 IV / spring.h→IV | INVALID |
| 执行中改 calib / N / order / ε / 估窗 | INVALID |
| 抬 thr / 放宽 17/20 | 否决 |
| 改 composition.h | INVALID |
| 重跑/改写 `831818e4…80de` 或 `06467b82…805c` | INVALID |
| 产物标 Δv-PASS / PERF-CI 合格证据 | INVALID |
| 无 base / 无 IVel0 双 FAIL 却宣称 PASS | INVALID |

## DRAFT 路径

`docs/m2_wgc_seed_calib_gate_DRAFT.md`（已落盘骨架；**NOT LOCKED**）。

## 挂账（本决断明确）

- **M2-Δv″**：继续挂账——seed（含本 CALIB）可过比回归与 smoke 后再开**新闸门**绝对门复测。  
- **PATH-B′**：若 CALIB CLOSED 后仍 rev 方向性全灭而 fwd 近过，再决断开估窗变体票。  
- PERF-CI 仍 HOLD；不开 M3。

## 协调下一步

派 DeepSeek（标定式精确值 + 比窗预期 + 虚 PASS）→ Notion 可 LOCK → LOCK → pidan APPROVED（单机）。  
**禁**在 LOCK+APPROVED 前采数。

## 禁令

不重开 S1/R-3/R-2；不改 composition.h（`a55d4952…e418`）；不解冻 Bv；禁影子 IV；成功≠采用≠Δv-PASS；**不改/不重跑** `831818e4…80de` 与 `06467b82…805c`；PERF-CI 不降级；不开 M3；禁抬 thr / 降 17/20 / 偷换估窗伪装 PASS。

— notion ai（下一票=M2-WGC-SEED-CALIB）
