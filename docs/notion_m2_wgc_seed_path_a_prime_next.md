# M2 下一票决断（M2-WGC-SEED-PATH-A′ CLOSED SEED-PATHA-PRIME-FAIL 后 · SEED-TRUTH-CONFLICT HUNG · 平滑臂已耗尽 · PERF-CI HOLD）

> DECISION_AT=2026-10-26 15:37:30 Asia/Taipei  
> 依据：`docs/notion_m2_wgc_seed_path_a_prime_pack.md`；`docs/m2_wgc_seed_path_a_prime_result.md`  
> 本票 gate `f17871f1…2362` **不改、不重跑**；前序 `a1161c77…3a43` / `09f6414b…793d` / `831818e4…80de` / `06467b82…805c` **不翻盘**  
> SEED_TRUTH_CONFLICT=**HUNG（G2-LOCK-2b）**；预登记 A′ 平滑臂**已耗尽**

## 结论

**选 A = M2-SEED-TRUTH-CONFLICT**（诊断/决议票；结题标签建议 `SEED-TRUTH-*`）。  
DRAFT：`docs/m2_seed_truth_conflict_gate_DRAFT.md`（**NOT LOCKED**；默认不开真机采数）。

**一句话为什么**：PATH-B′（V2-clamp，P-VS=OFF）与 PATH-A′（+P-VS α_v=0.30）均 CLOSED FAIL；预登记平滑臂已耗尽；G2-LOCK-2b 冲突持续（s2/s3 比过∧smoke 双灭；s1 cross-dir EMA max_r=6.60）——下一步须显式决议「2F 比真值 vs 绝对 v_pre / smoke」冲突，**不得**默认抬 thr / 改分母 / 再标定 / 再跑 α 调参救票。

## 现状

| 项 | 状态 |
|---|---|
| M2-Δv | CLOSED INCONCLUSIVE；`d2349ceb…09c6` 不改 |
| M2-Δv′ | CLOSED FAIL；`06467b82…805c` 不改、不重跑翻盘 |
| M2-WGC-SEED-ABS | CLOSED SEED-ABS-FAIL；`831818e4…80de` 不改、不重跑翻盘 |
| M2-WGC-SEED-CALIB | CLOSED SEED-CALIB-FAIL；`09f6414b…793d` 不改、不重跑翻盘 |
| M2-WGC-SEED-PATH-B′ | CLOSED SEED-PATHB-FAIL；`a1161c77…3a43` 不改、不重跑翻盘；曾触发 TRUTH-CONFLICT |
| M2-WGC-SEED-PATH-A′ | **CLOSED SEED-PATHA-PRIME-FAIL**；`f17871f1…2362` 不改、不重跑翻盘；**SEED-TRUTH-CONFLICT=HUNG**；平滑臂耗尽 |
| M2-PERF-CI | HOLD；不降级 |
| M2-01 / H5 / M2-02 | CLOSED PASS |
| M3 | 不开真机 |

## 候选速判

| 选项 | 决断 | 理由 |
|---|---|---|
| **A M2-SEED-TRUTH-CONFLICT** | **采纳** | 双路径 CLOSED FAIL；平滑臂耗尽；须显式决议比真值 vs 绝对冲突；不预授权改分母/抬 thr |
| B 再跑 α 调参 / 新平滑臂主线 | **否** | 预登记 α_v=0.30 已跑；禁默认同跑新臂偷换；禁抬 thr 救票 |
| C 再开 / 重标定 calib | **否** | 禁再标定主线；exact KEEP；比窗部分已过 |
| D 立刻 M2-Δv″ | **否** | smoke 未齐；绝对门复测须冲突决议后再开新闸门 |
| E 重跑 A′ / PATH-B′ / CALIB / ABS / Δv′ FAIL | **否** | 禁翻盘（含本票 `f17871f1…2362`） |
| F PERF-CI / 开 M3 | **否** | HOLD；不开 M3 |
| G STOP / 旁路挂起主线 | **否（本轮）** | 冲突证据已齐（双路径）；可决议；无强理由停主线 |

**为何不否决 A、不改选 STOP**：result 已写死 A′ 触达 + G2-LOCK-2b；s1 max_r=6.60 与 s2/s3 med_r 贴窗下沿是冲突证据，不是「再调 α 就能过」的信号；STOP 会使冲突无决议出口。若决议菜单选「STOP 挂账」——须**本 TRUTH 票内显式拍板**，不是本 next 默认。

## 可 LOCK 修订要点（给同伴 / Notion）

### 范围写死

1. **offline / 文档决议为主**；真机采数**默认不开**，除非本票 LOCK+APPROVED **显式授权**且范围写死。
2. **产出**：冲突根因陈述；可选处置菜单（例：授权改 H5 分母 / 改 smoke 参照 / 新估法族 / STOP 挂账等）——**每项须另票 LOCK**；本票本身**不**预授权执行改分母或抬 thr。
3. **禁翻盘**五 FAIL gate（含 A′ `f17871f1…2362`）；**禁立刻 Δv″**；PERF-CI HOLD；不开 M3。
4. **可引用证据**：A′ s1 cross-dir EMA max_r=**6.5997**；A′ s2/s3 med_r=**0.8527 / 0.8649**（贴窗下沿）；PATH-B′ 与 PATH-A′ 均比过∧smoke 双灭（B′ 三会话比过；A′ 在 s2/s3）。

### KEEP（相对前序）

- composition.h `a55d4952…e418` **不改**
- 不解冻 Bv；不重开 S1/R-3/R-2
- 禁影子 IV；禁立刻 Δv″
- calib exact / thr / 17/20 / H5 分母：**本票不执行改动**；若菜单项选「授权改…」→ **另票**
- 结题标签：`SEED-TRUTH-PASS|FAIL|INVALID|STOP`（或等价 `SEED-TRUTH-*`）；**≠** 五 FAIL gate 翻盘；**≠** `Δv-PASS`

### 否决虚 PASS / 偷换

| 反例 | 否决 |
|---|---|
| 本票内抬 thr / 降 17/20 / 改 H5 分母并宣称已执行 | 否决 / INVALID |
| 默认同跑新 α / 新估法族偷换 A′ FAIL | INVALID |
| 重跑/改写五 FAIL gate | INVALID |
| 产物标 Δv-PASS / SEED-*-PASS 翻盘 / PERF-CI 合格证据 | INVALID |
| 未 LOCK+APPROVED 真机采数 | INVALID |
| 立刻开 Δv″ | 否决 |
| 再标定主线 | 否决 |

### Δv″ / 其他挂账

- **Δv″：继续挂账**——冲突决议且后续 seed 路径可过比回归+smoke 后再开新闸门；**不立刻开**
- PERF-CI 仍 HOLD；不开 M3
- 旁路 `side_parked_hang_degrade.md` 不挡、本票也不清旁路
- PATH-B′ / PATH-A′ FAIL 归档保持

## DRAFT 路径

`docs/m2_seed_truth_conflict_gate_DRAFT.md`（已落盘骨架；**NOT LOCKED**）。

## 协调下一步

派 DeepSeek（核对：冲突陈述边界 / 处置菜单「每项另票」/ 五 FAIL 不翻盘 / 默认不开采数）→ Notion 可 LOCK → LOCK → pidan APPROVED。  
**禁**在 LOCK+APPROVED 前真机采数（除非 APPROVED 显式授权写死范围）。  
**禁**把本票当 A′/PATH-B′ 翻盘或默认同跑新臂。

## 禁令

不重开 S1/R-3/R-2；不改 composition.h（`a55d4952…e418`）；不解冻 Bv；禁影子 IV；成功≠采用≠Δv-PASS；**不改/不重跑** `f17871f1…2362` / `a1161c77…3a43` / `09f6414b…793d` / `831818e4…80de` / `06467b82…805c`；结题只用 `SEED-TRUTH-*`；禁标 Δv-PASS / 前序 SEED-*-PASS 翻盘；PERF-CI 不降级；不开 M3；禁抬 thr / 降 17/20 / 偷换估窗 / 本票预授权改 H5 分母；禁再标定主线；禁再跑 α 调参主线；禁立刻 Δv″；结构风险下 FAIL 合法；SEED-TRUTH-CONFLICT 本票升主线决议。

— notion ai（下一票=M2-SEED-TRUTH-CONFLICT）
