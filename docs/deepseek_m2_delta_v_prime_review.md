# DeepSeek · M2-Δv′ 闸门补洞（虚 PASS / base / harness）

- 时间：2026-10-26（Australia/Perth, UTC+8）
- 状态：**文档 only**；不授权采数；**不改**前序 LOCKED `d2349ceb…09c6`；不改 composition.h
- **出处说明**：DeepSeek Harness / 网页本轮不可用（探活失败）。本文件由 DeepSeek网页版 bot **按** `docs/m2_delta_v_prime_gate_DRAFT.md`、`docs/notion_m2_delta_v_pack.md`、`docs/m2_delta_v_result.md`、`artifacts/m2_delta_v_s{1,2,3}_summary.txt` 起草，供 Notion LOCK；Harness 恢复后可对照重跑。
- KEEP：`thr=max(20,5%|v_ref|)`；估计器二次 w=5；t156 规则 SKIP；禁影子 IV；≠PERF-CI；不开 M3

---

# M2-Δv′ 复核补洞（DRAFT；文档 only）

- 前序：M2-Δv = CLOSED INCONCLUSIVE（gate `d2349ceb…09c6` **不改**）
- composition.h sha256：`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（未改）
- 硬边界：不重开 S1/R-3/R-2；不解冻 Bv；成功≠采用；单机合法；PERF-CI 不降级

## 0) 真机数字速览（s1–s3；权威摘要）

| 格 | s1 Nv_h / base / wgc / IVel0 | s2 | s3 | 旧可判失败主因 |
|---|---|---|---|---|
| t060fwd | 7.5 / **19**/20 / 2 / 0 | 10.2 / **19** / 2 / 0 | 11.4 / **20** / 4 / 0 | \|a\|σ≈10.5–10.9 > thr/2=10 |
| t060rev | 7.3 / **20** / 0 / 2 | 24.8 / **18** / 0 / 0 | 24.6 / **18** / 0 / 0 | 同上；s2/s3 另 Nv_h>thr |
| t300fwd | 32.5 / 13 / 0 / 0 | 20.8 / **18** / 0 / 0 | 26.0 / **17** / 0 / 0 | thr<2·Nv_h（且常 Nv_h>thr） |
| t300rev | 18.0 / **19** / 4 / 4 | 25.0 / **17** / 0 / 0 | 24.9 / 15 / 0 / 0 | thr<2·Nv_h |
| t300dbl | 35.7 / 15 / 0 / 0/10崩 | 31.2 / 15 / 0 / 0/9 | 36.7 / 13 / 0 / 0/11 | thr<2·Nv_h；IVel0 崩 |

三会话：`basePass=False`；`IVel0_t060_both_FAIL=True`；核心 5 格全 `Δv-SKIP` → 票 INCONCLUSIVE。

---

## 1) 虚 PASS 反例（P2 / 兼 P1）

### 1.1 若只放宽可判、不保留 PASS 约束 → 会被洗成虚 PASS 的臂

**反例 A（去 2×，且去掉「PASS 样本 Nv_h≤thr_i」）**  
- 构造：噪声臂（高测量噪声），格级 `Nv_h_cell ≈ 18`、`thrMed=20` → 在 `thr≥Nv_h` 下**可判**；真 Δv 分布宽，偶然有 ≥17/20 次 `Δv≤thr`。  
- 若**不**要求每次计 PASS 的样本满足 `Nv_h ≤ thr_i`，则噪声样本被算进 pass 率 → **虚 PASS**。  
- 真机影子：s1 `t300rev` IVel0 已有 4/20 过 thr；噪声再抬一点即可逼近 17/20。

**反例 B（去 2×，且用「格中位 Nv_h」代替逐样本）**  
- 格 `Nv_h_cell≤thr` 可判，但单次样本 `Nv_h_i≫thr` 仍计 PASS → 尾部噪声洗票。

**反例 C（|a|σ 从 thr/2 放宽到 thr，且无 FAIL-确定路径）**  
- t060  timing 一过，WGC 仍可能因真不连续而大 Δv；若不可判与 FAIL 混淆、或禁止记 FAIL，会把「差」挤回 INCONCLUSIVE，掩盖失败（见 §3）。本身不直接造虚 PASS，但与「只放宽可判」叠加时，会削弱负对照叙事。

### 1.2 否决条件 / 仍须保留的约束（写死建议）

| 约束 | 处置 | 理由 |
|---|---|---|
| 可判 SNR：`thrMed ≥ 2·Nv_h_cell` → **改为** `thrMed ≥ Nv_h_cell`（或 `≥ Nv_h+κ`，κ 预登记） | **允许改可判** | 2× 与 floor thr=20 在 Nv_h≈18–37 下互斥，是本票 INCONCLUSIVE 根因之一 |
| **PASS 样本** `Nv_h ≤ thr_i`（逐样本） | **必须 KEEP** | 堵住反例 A/B |
| 格 PASS：`Δv≤thr` 且满足上条 ≥**17/20**；Δv 中位 ≤ 格 thr 中位 | **必须 KEEP**；**禁止**放宽 17/20 | DRAFT P3 明文；防噪声臂碰运气 |
| FAIL-确定：Δv 的 CI 下界 > `thr_i + Nv_h` → **FAIL**，不得 SKIP/INCONCLUSIVE | **必须 KEEP** | 大跃迁不被可判几何吞掉 |
| 负对照 IVel0：t060 fwd+rev 须 FAIL（非 PASS 候选） | **KEEP** | s1–s3 已成立；可判放宽后应仍 FAIL（0–2/20） |
| 绝对阈公式 / 估计器 w=5 | **KEEP** | 禁为过门改 thr 或阶数 |

**结论**：可判从 `2×` 降到 `1×`（及 `|a|σ≤thr` 替代 `thr/2`）**可以 LOCK**，当且仅当上表 PASS/FAIL-确定约束一字不放。否则否决「只改可判」。

---

## 2) P3 · base 正对照可达性（用 s1–s3）

### 2.1 可判修复后，哪些格 base「已经像 PASS」

假设升版可判 = `thrMed ≥ Nv_h_cell` ∧ `|a|·σ_ts ≤ thrMed`（去 /2、去 2×），**不改 thr、不改 17/20**：

| 格 | 可判前景（s1–s3） | base_pass | 解读 |
|---|---|---|---|
| t060fwd | Nv_h 7.5–11.4 ≤20；\|a\|σ≈10.5–10.9 ≤20 → **可判** | 19/20, 19/20, 20/20 | **base 已达格 PASS** |
| t060rev | s1：Nv_h=7.3→可判；s2/s3：Nv_h≈24.6–24.8 **>20 → 仍 SKIP** | 20, 18, 18 | s1 可达；s2/s3 被 Nv_h>thr 挡住（非 17/20 问题） |
| t300fwd | Nv_h 20.8–32.5，多 **>20 → 仍 SKIP**；仅边界格可能进 | 13 / 18 / 17 | 一旦可判，s2/s3 已 ≥17；s1 的 13/20 见下 |
| t300rev | s1 Nv_h=18.0≤20→可判；s2/s3 Nv_h≈25>20→SKIP | 19 / 17 / 15 | s1/s2 达标；s3=15 见下 |
| t300dbl | Nv_h 31–37 **>20 → 仍 SKIP** | 15 / 15 / 13 | 可判未开；见噪声路径 |

### 2.2 base 噪声本身 > thr → FAIL-确定 vs 升 Nv_h 测法

- **证据**：在 thr 未改前提下，已出现 base_pass **<17/20**：s1 t300fwd=13、s3 t300rev=15、三会话 t300dbl=13–15。这不是「不可判」造成的假低，而是样本上 `Δv>thr` 过多 → **正对照未成立**。  
- **升版后若该格变为可判**且 base 仍 <17/20：记 **ctrl base FAIL-确定** → 挡票 PASS（与旧闸门一致）；**禁止**为救 base 放宽 ≥17/20 或改 thr。  
- **可选（文档旁路，不进本票过门）**：另票升 Nv_h / σ_ts **测法**（预登记公式），使估计噪声更贴近；在测法未升前，接受「部分 t300 格 base 不可达 → 票不能 PASS」。  
- **可达性总判**：可判修复后，**t060fwd（及 s1 类低 Nv_h 的 t060rev/t300rev）base 正对照已数字可达**；全 5/5 base PASS **不保证**——尤其 Nv_h>thr 的 t300 格仍 SKIP，dbl 上 base 偏弱。这支持「先修可判再跑」，不支持「放宽 17/20 硬凑 basePass」。

---

## 3) P4 · FAIL vs INCONCLUSIVE 边界

| 结局 | 条件（升版后写死） |
|---|---|
| **Δv-SKIP**（格） | 可判失败（timing 或 SNR）；或 t156 规则导出。SKIP ≠ PASS ≠ FAIL |
| **Δv-PASS**（格） | 可判 ∧（逐样本 Nv_h≤thr_i 且 Δv≤thr）≥17/20 ∧ 中位约束 |
| **Δv-FAIL**（格） | **可判** ∧ 未达格 PASS（含 WGC pass 0–4/20 这类） |
| **票 INCONCLUSIVE** | 会话不足；或可判核心格**全部** SKIP（无法分胜负）；或协议 INVALID。**不得**因「WGC 很差但不可判」单开 INCONCLUSIVE 来回避 FAIL |
| **票 FAIL** | 存在可判核心格 Δv-FAIL；或正对照 base 在可判核心未 PASS；或负对照 t060 未双 FAIL |
| **票 PASS** | 可判核心全 PASS + base 正对照成立 + IVel0 t060 双 FAIL + 会话≥3 等 G 条 |

**P4 拍板句**：可判修复后，WGC 样本差（真机已见 0–4/20）→ 记 **Δv-FAIL**，**禁止**再标成 INCONCLUSIVE。INCONCLUSIVE 仅保留给「不可判 / 证据不足」，不留给「已经可判但臂差」。

---

## 4) P5 · harness 崩旁路（G0/G1 注记草案）

**事实**：每会话探针在 **IVel0·t300dbl** 中途崩（s1 10/20、s2 9/20、s3 11/20 未齐）；base+WGC 在 t300dbl 仍齐；offline adj 留证。

**G0 注记草案（可粘贴）**  
> IVel0 在 t300dbl 上**不进票**。负对照范围写死为 **t060 fwd 与 t060 rev 均 FAIL**；t300{fwd,rev,dbl} 上 IVel0 缺测 / 崩 **不构成** 票 INVALID，也不挡 LOCK。探针崩溃属 harness 稳定旁路，另挂，不挡本票采数授权（LOCK+APPROVED 后）。

**G1 注记草案（可粘贴）**  
> 跑程允许 IVel0·t300dbl 提前结束；session 完整性以可判核心五格的 **base+WGC**（及 t060 的 IVel0）为准。崩后须 offline adjudicate；`IVel0_t060_both_FAIL` 三会话均 True 方可认负对照成立。

**与真机一致**：s1–s3 负对照已按 t060 双 FAIL 成立；旁路不改变该口径。

---

## 5) 给 Notion 的 LOCK 建议摘要

1. **可判**：采纳 `thrMed ≥ Nv_h_cell`（去 2×）+ `|a|·σ ≤ thrMed`（去 /2）；σ 公式预登记；**KEEP** thr 与 w=5。  
2. **反虚 PASS**：KEEP 逐样本 `Nv_h≤thr_i`、≥17/20、FAIL-确定路径、IVel0 t060 负对照。  
3. **base**：用 s1–s3 论证 t060 等格可达；base<17 的可判格 → FAIL-确定挡 PASS；**禁**放宽 17/20。  
4. **P4**：可判后 WGC 差 = **Δv-FAIL**。  
5. **P5**：G0/G1 写入 IVel0·t300dbl 旁路。  
6. 前序 gate hash **不改**；本票产物 ≠ PERF-CI。

— DeepSeek网页版（M2-Δv′ 补洞；Harness 不可用代拟；文档 only）
