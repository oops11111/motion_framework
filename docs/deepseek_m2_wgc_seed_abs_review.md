# DeepSeek · M2-WGC-SEED-ABS 闸门补洞（H5 衔接 / 验收 / 风险）

- 时间：2026-10-26（Asia/Taipei, UTC+8）
- 状态：**文档 only**；不授权采数；**不改** FAIL 闸门 `06467b82…805c`；不改 composition.h
- **出处说明**：DeepSeek Harness / 网页本轮未探活成功。本文件由 DeepSeek网页版 bot **按** `docs/m2_wgc_seed_abs_gate_DRAFT.md`、`docs/notion_m2_delta_v_prime_pack.md`、`docs/m2_delta_v_prime_result.md`、`docs/m2_h5_seed_gate_LOCKED.md`、`docs/m2_h5_seed_result.md` 起草，供 Notion LOCK；Harness 恢复后可对照重跑。
- 前序：M2-Δv′ = CLOSED FAIL；H5-SEED = CLOSED PASS（只锁速度比）；composition.h `a55d4952…e418` 未改

---

# M2-WGC-SEED-ABS 复核补洞（DRAFT；文档 only）

- 票目标：修 **WGC→InitialVelocity**，使绝对 Δv 在既有阈下可过；**禁影子 IV**
- 硬边界：不重开 S1/R-3/R-2；不解冻 Bv；成功≠采用；≠PERF-CI；不开 M3；**不重跑** `06467b82…805c` 翻盘

## 1) H5 衔接（med_r / calib / 分级 A→B）

### 1.1 与 H5 `med_r ∈ [0.8,1.2]` 的关系

| 项 | 写死建议 |
|---|---|
| H5 已锁什么 | **只锁速度比**：3/3 会话 `med_r∈[0.8,1.2]`；无单样本 `r_i≥2.0`；seed=WGC×calib；禁影子 IV。J/Δv **非** H5 PASS 门槛（H5 G2-LOCK-5 明文）。 |
| 本票锁什么 | **绝对连续改善**（缩小 Δv smoke，见 §2）；**不是**重开 H5、也不是改 H5 窗口。 |
| 速度比辅证 | **仍须报**（每会话 `med_r` / `max_r` / 是否 `any≥2.0`），作 **辅证观测**，防止「为过绝对 Δv 把 seed 拧歪导致比又飞」。 |
| 辅证门槛 | 建议：**回归门槛**——计入票 PASS 的会话须 `med_r∈[0.8,1.2]` 且无 `r_i≥2.0`（继承 H5 强口径）。若某改 IVel 路径令比出窗 → **本票 FAIL / INVALID**，不得以「绝对 smoke 好看」豁免。 |
| 非充分提醒 | H5 已 PASS（med_r≡1.0）而 Δv′ 仍 FAIL（wgc 0–2/20）——证明速度比 **必要非充分**；本票正是补「充分」侧，不得倒退破坏必要侧。 |

### 1.2 calib=1.0 默认

- **默认 KEEP `calib=1.0`**（H5 结题写死常量；同窗 seed=v_screen ⇒ r≡1）。
- **禁止**为过绝对 Δv 静默改 calib（例如放大 seed 凑 thr）。
- 若候选路径必须改 calib：须 **预登记** 证明（公式 + 与 H5 窗口兼容的预期 `med_r`），LOCK 前写入闸门；执行中改 calib = INVALID。

### 1.3 分级 A→B 如何继承

- **主路径 A**：仍为 WGC/屏一致位置导出的速度 → compositor `InitialVelocity`（单位 DIP/s，时基 QPC）。本票只改 A 的 **估法/接线**（多帧 LS、平滑、时戳对齐等），不改「A=屏一致源」命题。
- **回退 B**：取 WGC 失败 / `|v|<ε` / NaN → 该次 `IV=null + InitialVelocity=0`；B **不计** 票 PASS 会话。
- **票级 PASS 只评 A**：与 H5 G2-LOCK-0 同构；全程落 B 的会话不计 PASS。
- **ε**：建议继承 H5 `ε=5.0 DIP/s`（除非预登记另写）；不得执行中改。
- **禁**：`mf_spring_at` / spring.h 闭式 v → compositor IV（shadow_v_log 仅 log-only）。

---

## 2) 验收草案

### 2.1 会话 / 对照

| 项 | 建议（LOCK 写死） |
|---|---|
| 会话数 | **≥3** 独立进程（不同 PID；场景重建；同进程复跑只计 1） |
| 机器 | 单机 DESKTOP-K93N3P8 / 60 Hz 合法；VM=INVALID |
| 正对照 base | 继承 Δv′：可判核心 smoke 格上 **base 须 PASS**（证明估计器/阈未坏） |
| 负对照 IVel0 | 继承 Δv′ P5：**仅**要求 t060 fwd+rev 均 FAIL；t300* IVel0 不进票 |
| composition.h | PRE=POST = `a55d4952…e418`；任何改动 = INVALID |
| seed 声明 | 每会话落盘：`seed_path`（A/B）、`calib`、`v_wgc`/`iv_set`、`shadow_v_log`（若有，log-only）、`session_id`/`pid` |

### 2.2 绝对 Δv smoke 格表（缩小；非完整 Δv′）

**目的**：证明 WGC→IVel 修复后，绝对连续相对 H5/Δv′ 基线 **可分胜负地改善**；**不是**重跑 FAIL 闸门翻盘。

| 格 | 角色 | 估计器 / 阈 | 格 PASS 草案 |
|---|---|---|---|
| **t060 fwd** | **必测核心** | 二次 LS **w=5** 外推到 `t_s`；`thr_i=max(20,0.05·\|v_ref\|)`；可判继承 Δv′：`\|a\|σ≤thr` ∧ `thr≥Nv_h`；PASS 样本 `Nv_h≤thr_i`；≥**17/20** | 须 PASS |
| **t060 rev** | **必测核心** | 同上 | 须 PASS |
| t300 fwd/rev/dbl | **可选辅证** | 同上；`Nv_h>thr` → SKIP 可接受 | 不挡票；若可判则记 FAIL/PASS 入库 |
| t156 | 规则 SKIP | — | 不进 smoke |

**票 PASS（smoke）建议**：3/3 会话上 **t060 fwd 与 rev 均格 PASS**；basePass=True；IVel0_t060_both_FAIL=True；H5 比回归门槛满足（§1.1）；无影子 IV；composition.h 未改。

**相对基线（写死）**：与 Δv′ 真机对照——同机同估计器下，t060 被测臂由 **0–2/20** 提升到 **≥17/20**。允许引用 Δv′ **产物数字**作基线，**禁止**引用 gate hash `06467b82…805c` 作「重跑翻盘」授权。

### 2.3 引用估计器 vs 禁翻盘

- **允许**：复用 Δv′ 探针中的 **二次 w=5**、`thr` 公式、可判 P1/P2、PASS 约束（Nv_h≤thr_i、17/20、FAIL-确定）——作为 **测量工具**。
- **禁止**：把本票宣告为「M2-Δv′ PASS」；改写 / 重跑 `docs/m2_delta_v_prime_gate_LOCKED.md`；静默改 thr / 17/20「为过门」。
- 本票产物标签建议：`artifacts/m2_wgc_seed_abs_*`；结题写 `ticket=SEED-ABS-PASS|FAIL`，**不**写 `Δv-PASS`。

---

## 3) 风险 / 虚「绝对连续」PASS

### 3.1 会被洗成虚 PASS 的改 IVel 方式

| 反例 | 机制 | 否决 |
|---|---|---|
| **A 影子 IV** | `mf_spring_at` / spring.h 闭式 v → compositor IV | **INVALID**（H5/本票双禁） |
| **B 静默抬 calib** | calib≫1 把 seed 放大，偶然让 Δv 过 thr，但破坏与屏一致 | 未预登记改 calib = INVALID；有预登记仍须比回归过窗 |
| **C 估窗偷换** | 用打断**后**帧、或与 compositor 不同时基，制造「假连续」 | 估窗须预登记（打断前末 N 帧 / QPC）；偏离 = INVALID |
| **D 降 thr / 放宽 17/20** | 测法不变、门槛放水 | **否决**；KEEP thr 与 17/20 |
| **E 只测不可判格** | 专挑 Nv_h>thr 的格 SKIP，回避 t060 | smoke **强制** t060 fwd+rev 可判且 PASS |
| **F 无负对照** | IVel0 也「过」→ 估计器/阈失效 | 须 IVel0 t060 双 FAIL |
| **G 无正对照** | base 也 FAIL 却宣称 seed PASS | 须 basePass |
| **H 跨会话池化 / FAIL 后补测拼凑** | 拼出 17/20 | INVALID |

### 3.2 仍须保留的约束（摘要）

1. 禁影子 IV；calib 默认 1.0  
2. H5 比回归：`med_r∈[0.8,1.2]` 且无 `r_i≥2.0`  
3. smoke：t060 fwd+rev；w=5；thr KEEP；可判 P1/P2；Nv_h≤thr_i；≥17/20  
4. base PASS + IVel0 t060 双 FAIL  
5. 不改 composition.h；不重跑 `06467b82…805c`  

---

## 4) 与 Δv″ 关系

| 命题 | 写死 |
|---|---|
| 本票成功 | = WGC→IVel 路径在 **smoke** 下改善成立；**成功 ≠ 采用**；**≠ Δv PASS**；**≠ 解冻 Bv** |
| M2-Δv″ | **挂账**：须本票 CLOSED 后，**另开新闸门版本** 做完整绝对门复测（可判核心 5 格等） |
| 禁止 | 用本票 smoke PASS 宣称 Δv′/Δv″ PASS；用 FAIL 闸门 `06467b82…805c` 重跑翻盘 |
| 产物隔离 | 本票 artifacts **不得**写入 PERF-CI 合格证据；≠PERF-CI |

**一句话**：本票修 seed；Δv″ 才是「绝对门是否过」的正式复测——二者闸门、hash、结题标签必须分离。

---

## 5) 候选路径清单（衔接 Gemini；DeepSeek 否决面）

| 候选（屏一致源上） | DeepSeek 态度 |
|---|---|
| 打断前末 N 帧二次 LS（对齐 w=5） | **优先可 LOCK**；须预登记 N、时戳 |
| 多帧平滑 / 低通后再赋 IVel | 可；须证明不抬 calib、比不破窗 |
| 时戳对齐（WGC 与 compositor 同 QPC 原点） | 可；属接线修复 |
| spring.h / mf_spring_at → IV | **否决 / INVALID** |
| 改 composition.h / 解冻 Bv | **否决** |
| 为过门改 thr 或 17/20 | **否决** |

---

## 6) 给 Notion 的 LOCK 建议摘要

1. **H5**：仍报 `med_r` 为回归门槛；calib=1.0；A→B 继承；禁影子 IV。  
2. **验收**：≥3 会话；base / IVel0(t060)；smoke=**t060 fwd+rev**；工具链引用二次 w=5 + Δv′ 可判/PASS 约束；**不**引用 `06467b82…805c` 翻盘。  
3. **风险**：否决影子 IV、静默 calib、估窗偷换、降 thr、躲可判、无对照。  
4. **Δv″**：seed 后挂账新闸门；本票成功 ≠ Δv PASS。

— DeepSeek网页版（M2-WGC-SEED-ABS 补洞；Harness 未通代拟；文档 only）
