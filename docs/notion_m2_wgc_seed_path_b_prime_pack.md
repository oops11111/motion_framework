# M2-WGC-SEED-PATH-B′ 结题中文包（notion_m2_wgc_seed_path_b_prime_pack.md）

> DECISION_AT=2026-10-26 14:32:00 Asia/Taipei  
> 权威结题：`docs/m2_wgc_seed_path_b_prime_result.md`（WRITTEN_AT=2026-10-26 14:27:40 Asia/Taipei）  
> 闸门 LOCKED **勿改 hash**：`docs/m2_wgc_seed_path_b_prime_gate_LOCKED.md` GATE_BODY_SHA256=`a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43` @ 2026-10-26 13:49:20 Asia/Taipei  
> APPROVED：pidan@2026-10-26 13:51:27 Asia/Taipei  
> 真机：DESKTOP-K93N3P8（1 物理机；60 Hz）；≠PERF-CI；≠VM  
> 前序 SEED-CALIB FAIL gate `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`：**未重跑、未翻盘**  
> 前序 SEED-ABS FAIL gate `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`：**未重跑、未翻盘**  
> 前序 Δv′ FAIL gate `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`：**未重跑、未翻盘**  
> **SEED_TRUTH_CONFLICT=HUNG（G2-LOCK-2b）**  
> 旁路：`docs/side_parked_hang_degrade.md`（不挡、本票也不清）

## 结论

- **票级 = CLOSED FAIL**；标签 **`SEED-PATHB-FAIL`**（非 INVALID；非 PASS；比过 ≠ 票 PASS）。
- G4 ONELINE（原文照抄 result）：  
  `M2-WGC-SEED-PATH-B′ | gate=a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43 @ 2026-10-26 13:49:20 Asia/Taipei | approved=pidan@2026-10-26 13:51:27 Asia/Taipei | path=PATHB-A=V2-clamp/N=5/t_int=min(t_comp,t_last_WGC) | calib=0.7511(exact=0.7510829456 KEEP) | P-VS=OFF | smoke=t060 | ratio=reg | ctrl=baseP/IVel0F | ticket=SEED-PATHB-FAIL | SEED-TRUTH-CONFLICT | 成功≠采用≠Δv-PASS；不改composition.h；禁影子IV；≠PERF-CI；09f6414b/831818/06467b82不翻盘；A′挂账；Δv″继续挂账；结构风险见G1；SEED-TRUTH-CONFLICT见G2-LOCK-2b`
- **成功 ≠ 采用 ≠ Δv-PASS ≠ SEED-ABS-PASS ≠ SEED-CALIB-PASS ≠ SEED-PATHB-PASS 翻盘**；不解冻 / 不升级 B_v；不重开 S1 / R-3 / R-2；`composition.h` PRE=POST `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` **未改**；本闸门 body/hash **未改**。
- **不重跑本 FAIL 闸门翻盘**；**不翻盘**前序 `09f6414b…793d` / `831818e4…80de` / `06467b82…805c`。
- **本票 ≠ PERF-CI**；PERF-CI 仍 HOLD `5b007dd4…282a`；不开 M3 真机。
- **SEED-TRUTH-CONFLICT=HUNG**：H5 比回归过 ∧ t060 fwd+rev smoke 均未达 ≥17/20 → 触发 G2-LOCK-2b。
- **A′ 未跑**（本票不同跑；挂账次选）；P-VS=OFF。

## FAIL 归因（写死）

| 层 | 事实 | 含义 |
|---|---|---|
| 路径 | 默认 **PATHB-A=V2-clamp** 已触达；N=5；`t_int=min(t_comp,t_last_WGC)`；每会话 `t_int_clamp n_clamped=40`；pathA_n=40×3；pathB_n=0；calib exact=`0.7510829456` **KEEP**；ε=5.0；**P-VS=OFF** | V2-clamp 已落实；非回退 B 虚判；非再标定 |
| H5 比回归 | 三会话 med_r=**0.9465 / 0.8055 / 0.8062** ∈[0.8,1.2]；无 r_i≥2；ratioOk=True×3 | **比回归过** |
| 绝对 smoke | t060fwd **0/20×3**；t060rev **0/20×3**（s2 rev 不可判→SEED-PATHB-SKIP；其余可判）；均 ≪17 | **绝对 smoke 全灭**；0/3 会话格 PASS |
| 尺度 vs 绝对 | V2-clamp + KEEP calib 下比窗维持，但 Arm-WGC-seed 绝对 Δv 仍远超 thr=20（例 Δv=86.185 thr=20；\|ivel−v_pre\|≈50+） | **结构风险下 FAIL 合法**——幅度缺口无法靠相位钳制抹平 |
| 正对照 | 三会话 `basePass=True` | 噪声门 / 估计器 OK；非测法假 FAIL |
| 负对照 | 三会话 `IVel0_t060_both_FAIL=True` | 对照成立 |
| composition.h | PRE=POST `a55d4952…e418` | 未改 |
| 影子 IV | ivel_set=calib·v̂；shadow_v_log 仅 log（例 s1 t060fwd k0: ivel_set=94.783 ≠ shadow=1362.56） | 无影子 IV 违规 |
| 未违规放宽 | 未抬 thr；未放宽 17/20；未改 N·order·ε·calib；未偷换估窗；未默认同跑 A′ | 闸门纪律守住 |
| G2-LOCK-2b | 比过 ∧ smoke 双灭 | **SEED-TRUTH-CONFLICT=HUNG**；票仍 = **SEED-PATHB-FAIL** |

**根因一句话**：**V2-clamp 已触达且 calib KEEP，比回归过但绝对 smoke 全灭 → SEED-TRUTH-CONFLICT=HUNG**——对照健全（baseP + IVel0F）→ **非测法崩坏**；结构风险（\|ivel−v_pre\|≈50+）下 FAIL 合法；残差在 2F 比真值 vs 绝对 `v_pre` 冲突及尖峰，**超出本票默认 A（P-VS=OFF）可修范围**；A′（P-VS ON α_v=0.30）**尚未跑**。

**禁止解读**：禁止把「比过」误读成票 PASS；禁止静默改 thr / 17/20 / 偷换估窗 / 改 H5 分母；禁止重跑 `a1161c77…3a43` / `09f6414b…793d` / `831818e4…80de` / `06467b82…805c` 翻盘；禁止标 `Δv-PASS` / `SEED-ABS-PASS` / `SEED-CALIB-PASS` / 本票 PASS；禁止把本 FAIL 偷换成 A′ 同跑结果；禁止立刻开 Δv″。

## 事实表

| 项 | 值 |
|---|---|
| TICKET / STATUS | `SEED-PATHB-FAIL` / **CLOSED** |
| SEED_TRUTH_CONFLICT | **HUNG（G2-LOCK-2b）** |
| pass_sessions | **0/3**（非 INVALID） |
| path | PATHB-A=V2-clamp；N=5；`t_int=min(t_comp,t_last_WGC)`；全程 A；B=0 |
| calib | 显示 0.7511；exact=`0.7510829456`（**KEEP**；执行中未改） |
| P-VS | **OFF**；A′ **未跑** |
| H5 med_r | s1 **0.9465**；s2 **0.8055**；s3 **0.8062**（均 ∈[0.8,1.2]；无 r_i≥2；**比过**） |
| smoke t060fwd | **0/20 · 0/20 · 0/20**（可判；≪17） |
| smoke t060rev | **0/20 · 0/20(SKIP) · 0/20**（≪17） |
| 对照 | basePass=T/T/T；IVel0_t060_both_FAIL=T/T/T |
| composition.h | PRE=POST `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` |
| 禁令核对 | 无影子 IV；未标 Δv-PASS/SEED-ABS-PASS/SEED-CALIB-PASS；未翻盘四 FAIL gate；≠PERF-CI；未解冻 Bv；未重开 S1/R-3/R-2；未开 M3；未抬 thr/放宽17/20/改N；未立刻 Δv″ |

### 会话证据

| sess | appPID | start→done (Asia/Taipei) | t060fwd | t060rev | med_r | ratioOk | basePass | IVel0F | 判定 |
|---|---|---|---|---|---|---|---|---|---|
| 1 | 35864 | 13:56:11→~14:05:50 | FAIL 0/20 | FAIL 0/20 | 0.9465 | T | T | T | FAIL_CANDIDATE |
| 2 | 56216 | 14:06:08→14:15:42 | FAIL 0/20 | SKIP 0/20 | 0.8055 | T | T | T | FAIL_CANDIDATE |
| 3 | 54392 | 14:15:51→14:25:22 | FAIL 0/20 | FAIL 0/20 | 0.8062 | T | T | T | FAIL_CANDIDATE |

## 对照健全

| sess | basePass | IVel0_t060_both_FAIL | pathAUsed |
|---|---|---|---|
| 1 | True | True | True |
| 2 | True | True | True |
| 3 | True | True | True |

正对照 base 与负对照 IVel0 均满足；票仍 FAIL（smoke；SEED-TRUTH-CONFLICT）——**问题不在对照失效**。

## 不翻盘声明

- **本票闸门** `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43` @ 2026-10-26 13:49:20 Asia/Taipei：**不改 body/hash、不重跑翻盘**（结题落盘时已复算 body hash **match**）。
- **前序 SEED-CALIB FAIL 闸门** `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`：**不改、不重跑、不宣称通过**。
- **前序 SEED-ABS FAIL 闸门** `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`：**不改、不重跑、不宣称通过**。
- **前序 Δv′ FAIL 闸门** `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`：**不改、不重跑、不宣称通过**。
- 结题标签只用 `SEED-PATHB-*`；**禁止**写 `Δv-PASS` / `SEED-ABS-PASS` / `SEED-CALIB-PASS` / 宣称前序或本票翻盘。
- 完整绝对门复测仍挂 **M2-Δv″**（须 seed 路径可过比回归+smoke 后再**另开新闸门**；**不立刻开**）。

## 产物清单

落盘前缀 `artifacts/m2_wgc_seed_path_b_prime_*`（及 docs 结题）：

- `docs/m2_wgc_seed_path_b_prime_result.md`（权威结题）
- `artifacts/m2_wgc_seed_path_b_prime_prereg.txt` + `_prereg_hash.txt`（prereg_hash=`ad7335017c618cc82150990c453fa2c4888fb7dce23f1c084dc26bc9dc276b64`）
- `artifacts/m2_wgc_seed_path_b_prime_run_sessions.ps1` + `_run.log`
- `artifacts/m2_wgc_seed_path_b_prime_s{1,2,3}_{raw,summary,stdout,stderr}.txt`
- `artifacts/m2_wgc_seed_path_b_prime_session_matrix.txt`
- `artifacts/m2_wgc_seed_path_b_prime_composition_hash.txt`
- `artifacts/m2_wgc_seed_path_b_prime_ticket.txt`
- `artifacts/m2_wgc_seed_path_b_prime_bundle.zip`
- `artifacts/M2WgcSeedPathBPrimeProbe.cs` + `MotionSoftDemo/M2WgcSeedPathBPrimeProbe.cs`

## 禁令复述

- 不重开 S1 / R-3 / R-2。
- 不解冻 / 不升级 B_v。
- 成功 ≠ 采用 ≠ Δv-PASS ≠ SEED-ABS-PASS ≠ SEED-CALIB-PASS ≠ 本票 PASS 翻盘。
- 不改 `composition.h`（`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）。
- 禁影子 IV。
- **不改**已 LOCKED `a1161c77…3a43`；**不重跑本票翻盘**。
- **不改 / 不重跑**前序 `09f6414b…793d` / `831818e4…80de` / `06467b82…805c`。
- 旁路挂账不挡主线；本票也不清旁路。
- PERF-CI 不降级；本票产物 ≠ PERF-CI 证据。
- 不开 M3 真机。
- 禁抬 thr / 降 17/20 / 偷换估窗 / 改 H5 分母伪装 PASS。
- **禁**再开 calib 废止票当主线；**禁**立刻开 Δv″；**禁**默认同跑 A′ 偷换本 FAIL。

## 挂账 / 残差

| 项 | 处置 |
|---|---|
| M2-WGC-SEED-PATH-B′（本票） | **CLOSED SEED-PATHB-FAIL**；LOCKED 归档不改；**SEED-TRUTH-CONFLICT=HUNG** |
| **M2-WGC-SEED-PATH-A′** | **下一主线**（显式另票；见下一票）——KEEP V2-clamp + calib exact；**P-VS=ON α_v=0.30**（闸门预登记）；新闸门 |
| SEED-TRUTH-CONFLICT | **挂账保持**（比过∧smoke 双灭）；不预授权改 H5 分母；若 A′ 仍比过∧smoke 双灭 → 保持或升主线 |
| M2-Δv″（绝对门复测） | **继续挂账**：须比回归+smoke 齐后再开**新闸门**；禁用本票/CALIB/ABS/Δv′ FAIL 闸门翻盘；**不立刻开** |
| 重开 calib 废止/重标定 | **否** |
| M2-PERF-CI | 继续 HOLD |
| M3 | 不开真机 |

## 下一票决断

**选 A = M2-WGC-SEED-PATH-A′（显式另票；P-VS ON α_v=0.30）。**  
骨架 DRAFT：`docs/m2_wgc_seed_path_a_prime_gate_DRAFT.md`（**NOT LOCKED**；不授权采数）。详见 `docs/notion_m2_wgc_seed_path_b_prime_next.md`。

### 候选速判

| 选项 | 决断 | 一句理由 |
|---|---|---|
| **A PATH-A′（V2-clamp+P-VS α_v=0.30）** | **采纳** | 默认 A 已跑且触发 TRUTH-CONFLICT；闸门预登记 A′ **尚未跑**——先耗尽禁抬 thr 的平滑臂 |
| B 立刻 SEED-TRUTH-CONFLICT 主线 | **否（本轮）** | A′ 无望证据不足（未跑）；改真值/改 H5 前须先耗尽预登记臂 |
| C 再开 calib / 重标定 | **否** | 禁再标定主线；calib KEEP 已触达 |
| D 立刻开 Δv″ | **否** | smoke 未齐；绝对门复测须 seed 后新闸门 |
| E 重跑本 FAIL / 09f6414b / 831818 / 06467b82 | **否** | 禁翻盘 |
| F PERF-CI / 开 M3 | **否** | HOLD；不开 M3 |
| G 默认同跑 A′ 偷换本 FAIL | **否** | 须显式另票并预登记 |

### 协调下一步一句

请派同伴按 `docs/m2_wgc_seed_path_a_prime_gate_DRAFT.md` 补洞（预登记 P-VS ON α_v=0.30；KEEP V2-clamp/calib；验收同 H5 比+t060 smoke+对照）→ Notion 收齐后可 LOCK 修订 → LOCK → 问 pidan APPROVED（单机；禁抬 thr；禁改 a1161c77/09f6414b/831818/06467b82；Δv″ 继续挂账；SEED-TRUTH-CONFLICT 保持挂账）。

— notion ai（M2-WGC-SEED-PATH-B′ 结题；ticket=CLOSED SEED-PATHB-FAIL；SEED-TRUTH-CONFLICT=HUNG；下一票=M2-WGC-SEED-PATH-A′）
