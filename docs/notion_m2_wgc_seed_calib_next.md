# M2 下一票决断（M2-WGC-SEED-CALIB CLOSED SEED-CALIB-FAIL 后 · PATH-B′ HUNG · PERF-CI HOLD）

> DECISION_AT=2026-10-26 13:42:30 Asia/Taipei  
> 依据：`docs/notion_m2_wgc_seed_calib_pack.md`；`docs/m2_wgc_seed_calib_result.md`  
> 本票 gate `09f6414b…793d` **不改、不重跑**；前序 `831818e4…80de` / `06467b82…805c` **不翻盘**  
> PATH_B_PRIME=**HUNG（G2-LOCK-2b）**

## 结论

**选 A = M2-WGC-SEED-PATH-B′**（别名可写 `M2-WGC-SEED-PATHB`；结题标签建议 `SEED-PATHB-*`）。  
DRAFT：`docs/m2_wgc_seed_path_b_prime_gate_DRAFT.md`（**NOT LOCKED**；不授权采数）。

**一句话为什么**：本票 calib exact=`0.7510829456` 已把 H5 比拉回 med_r≈0.99–1.00（尺度目标达成），但 t060 smoke 仍全灭（fwd 1–3/20；**rev 0/20×3**）——G2-LOCK-2b 已写死挂账 PATH-B′；残差在估窗/TS/P-VS/方向性，**KEEP calib，开变体票**；Δv″ 继续挂账到比+smoke 齐后再开。

## 现状

| 项 | 状态 |
|---|---|
| M2-Δv | CLOSED INCONCLUSIVE；`d2349ceb…09c6` 不改 |
| M2-Δv′ | CLOSED FAIL；`06467b82…805c` 不改、不重跑翻盘 |
| M2-WGC-SEED-ABS | CLOSED SEED-ABS-FAIL；`831818e4…80de` 不改、不重跑翻盘 |
| M2-WGC-SEED-CALIB | **CLOSED SEED-CALIB-FAIL**；`09f6414b…793d` 不改、不重跑翻盘；**PATH_B_PRIME=HUNG** |
| M2-PERF-CI | HOLD；不降级 |
| M2-01 / H5 / M2-02 | CLOSED PASS |
| M3 | 不开真机 |

## 候选速判

| 选项 | 决断 | 理由 |
|---|---|---|
| **A M2-WGC-SEED-PATH-B′** | **采纳** | G2-LOCK-2b 已触发：比过 + smoke FAIL（尤其 rev 全灭）；单标量 calib 已尽 |
| B 再开 / 重标定 calib | **否** | med_r∈[0.8,1.2] 且≈1.00；无新证据废止 exact |
| C 立刻 M2-Δv″ | **否** | smoke 未齐；绝对门复测须 seed 后新闸门 |
| D 重跑 SEED-CALIB / ABS / Δv′ FAIL | **否** | 禁翻盘 |
| E PERF-CI / 开 M3 | **否** | HOLD；不开 M3 |
| F 先单独估窗诊断票 | **否（本轮）** | 闸门已挂 PATH-B′；诊断并入同伴补洞即可，勿挡主线 |

**为何不否决 PATH-B′**：result 明确 PATH_B_PRIME=HUNG；fwd 亦远未过（1–3/20），不是「只剩诊断」；对照健全 → 值得开变体闸门而非再标定。

## 可 LOCK 修订要点（给同伴 / Notion）

### KEEP（写死）

- **calib exact=`0.7510829456`**（显示 0.7511）：**不重开 calib 废止/重标定**，除非所选变体**显式**要求并预登记新常量  
- **ε=5.0**；禁影子 IV；不改 `composition.h`（`a55d4952…e418`）  
- **H5 比回归仍必要**：`med_r∈[0.8,1.2]` 且无 `r_i≥2`  
- 对照同：basePass + IVel0_t060_both_FAIL  
- smoke=**t060 fwd+rev**；二次 w=5；`thr=max(20,0.05·|v_ref|)`；≥**17/20**；可判 P1/P2  
- ≥3 独立进程；单机 DESKTOP-K93N3P8 / 60 Hz  
- 回退 B = IV null+0；票 PASS 只评主路径 A（或本票选定之 A′）  
- 结题标签：`SEED-PATHB-PASS|FAIL|INVALID`（**≠** `SEED-CALIB-*` 翻盘；**≠** `SEED-ABS-*`；**≠** Δv-PASS）

### 变体方向（写进要点；LOCK 前由同伴补洞拍板其一或分级）

1. **估窗几何**：N 或 order 微调、或打断前采样对齐；**禁**打断后帧 / 异时基伪装  
2. **P-TS 采样点**：`t_int` 定义微调（仍 QPC compositor）  
3. **启用 P-VS**（此前默认关）作默认 A 或 A′ 分级（须预登记 `α_v`/限幅；不破比窗）  
4. **方向性**：专门处理 rev 全灭（fwd 1–3 亦远未过）——**不得**只修一侧却放宽另一侧门槛  

参考：`docs/deepseek_m2_wgc_seed_abs_path_design.md`（P-LS / P-TS / P-VS / P-MF）。

### 否决虚 PASS

| 反例 | 否决 |
|---|---|
| 抬 thr / 降 17/20 | 否决 |
| 偷换估窗（打断后帧、异时基） | INVALID |
| 改 composition.h | INVALID |
| 影子 IV / spring.h→IV | INVALID |
| 重跑/改写 `09f6414b…793d` / `831818e4…80de` / `06467b82…805c` | INVALID |
| 产物标 Δv-PASS / SEED-ABS-PASS / SEED-CALIB-PASS 翻盘 / PERF-CI 合格证据 | INVALID |
| 只修 fwd（或只修 rev）却放宽另一侧门槛 | 否决 |
| 无 base / 无 IVel0 双 FAIL 却宣称 PASS | INVALID |
| 执行中改未预登记的 N/order/ε/calib/P-VS | INVALID |

### Δv″ / 其他挂账

- **Δv″：继续挂账**——PATH-B′ 可过比回归+smoke 后再开新闸门绝对门；**不立刻开 Δv″**  
- PERF-CI 仍 HOLD；不开 M3  
- 旁路 `side_parked_hang_degrade.md` 不挡、本票也不清旁路

## DRAFT 路径

`docs/m2_wgc_seed_path_b_prime_gate_DRAFT.md`（已落盘骨架；**NOT LOCKED**）。

## 协调下一步

派 DeepSeek（变体择一或 A/A′ 分级 + 预登记参数 + 虚 PASS）→ Notion 可 LOCK → LOCK → pidan APPROVED（单机）。  
**禁**在 LOCK+APPROVED 前采数。

## 禁令

不重开 S1/R-3/R-2；不改 composition.h（`a55d4952…e418`）；不解冻 Bv；禁影子 IV；成功≠采用≠Δv-PASS；**不改/不重跑** `09f6414b…793d` / `831818e4…80de` / `06467b82…805c`；结题只用 `SEED-PATHB-*`；禁标 Δv-PASS / SEED-ABS-PASS / 本票 SEED-CALIB 翻盘；PERF-CI 不降级；不开 M3；禁抬 thr / 降 17/20 / 偷换估窗伪装 PASS；Δv″ 继续挂账。

— notion ai（下一票=M2-WGC-SEED-PATH-B′）
