# M2-WGC-SEED-PATH-A′ 结题中文包（notion_m2_wgc_seed_path_a_prime_pack.md）

> DECISION_AT=2026-10-26 15:37:30 Asia/Taipei  
> 权威结题：`docs/m2_wgc_seed_path_a_prime_result.md`（WRITTEN_AT=2026-10-26 15:33:30 Asia/Taipei）  
> 闸门 LOCKED **勿改 hash**：`docs/m2_wgc_seed_path_a_prime_gate_LOCKED.md` GATE_BODY_SHA256=`f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362` @ 2026-10-26 14:38:15 Asia/Taipei  
> APPROVED：pidan@2026-10-26 14:45:22 Asia/Taipei  
> 真机：DESKTOP-K93N3P8（1 物理机；60 Hz）；≠PERF-CI；≠VM  
> 前序 PATH-B′ FAIL gate `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`：**未重跑、未翻盘**  
> 前序 SEED-CALIB FAIL gate `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`：**未重跑、未翻盘**  
> 前序 SEED-ABS FAIL gate `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`：**未重跑、未翻盘**  
> 前序 Δv′ FAIL gate `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`：**未重跑、未翻盘**  
> **本票 FAIL gate `f17871f1…2362`：结题后不改 body/hash、不重跑翻盘**  
> **SEED_TRUTH_CONFLICT=HUNG（G2-LOCK-2b）**  
> 旁路：`docs/side_parked_hang_degrade.md`（不挡、本票也不清）

## 结论

- **票级 = CLOSED FAIL**；标签 **`SEED-PATHA-PRIME-FAIL`**（非 INVALID；非 PASS；**比过 ≠ 票 PASS**）。
- G4 ONELINE（原文照抄 result）：  
  `M2-WGC-SEED-PATH-A′ | gate=f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362 @ 2026-10-26 14:38:15 Asia/Taipei | approved=pidan@2026-10-26 14:45:22 Asia/Taipei | path=PATHB-A′=V2-clamp+V3-VS/N=5/α_v=0.30 | calib=0.7511(exact=0.7510829456 KEEP) | P-VS=ON | smoke=t060 | ratio=reg | ctrl=baseP/IVel0F | ticket=SEED-PATHA-PRIME-FAIL | 成功≠采用≠Δv-PASS；不改composition.h；禁影子IV；≠PERF-CI；四闸门不翻盘；SEED-TRUTH-CONFLICT挂账；结构风险见G1`
- **成功 ≠ 采用 ≠ Δv-PASS ≠ SEED-ABS-PASS ≠ SEED-CALIB-PASS ≠ SEED-PATHB-PASS ≠ SEED-PATHA-PRIME-PASS 翻盘**；不解冻 / 不升级 B_v；不重开 S1 / R-3 / R-2；`composition.h` PRE=POST `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` **未改**；本闸门 body/hash **未改**（结题落盘复算 body hash **match** `f17871f1…2362`）。
- **不重跑本 FAIL 闸门翻盘**；**不翻盘**前序 `a1161c77…3a43` / `09f6414b…793d` / `831818e4…80de` / `06467b82…805c`。
- **本票 ≠ PERF-CI**；PERF-CI 仍 HOLD `5b007dd4…282a`；不开 M3 真机。
- **SEED-TRUTH-CONFLICT=HUNG**：s2/s3 上 H5 比回归过 ∧ t060 fwd+rev smoke 均未达 ≥17/20 → 触发 G2-LOCK-2b；s1 另有 any_ri≥2（比 FAIL）。
- **预登记 A′ 平滑臂已耗尽**（PATH-B′ A 与 PATH-A′ 均 CLOSED FAIL）。

## FAIL 归因（写死）

| 层 | 事实 | 含义 |
|---|---|---|
| 路径 | 默认 **PATHB-A′=V2-clamp+V3-VS** 已触达；N=5；`t_int=min(t_comp,t_last_WGC)`；每有效会话 `t_int_clamp n_clamped≈40`；pathA_n≈40–41；pathB_n=0；calib exact=`0.7510829456` **KEEP**；ε=5.0（**VS 前判**）；**P-VS=ON α_v=0.30**；IV=`(float)(calib·v_s)` | A′ 已落实；非回退 B 虚判；非再标定；非关 P-VS 偷换 |
| H5 比回归 | s1 med_r=**0.9289** max_r=**6.5997** any_ri≥2=**True** → ratioOk=**False**（cross-dir EMA）；s2 med_r=**0.8527** / s3 med_r=**0.8649** ∈[0.8,1.2] 且无 r_i≥2 → ratioOk=**True** | **s1 比 FAIL**；**s2/s3 比过**（贴下沿 0.85–0.86） |
| 绝对 smoke | t060fwd：**1/20 · 0/20(SKIP) · 0/20**；t060rev：**0/20 · 0/20 · 0/20**；0/3 会话 fwd+rev 格 PASS | **绝对 smoke 全灭**（可判格均 ≪17） |
| 尺度 vs 绝对 | VS 后 med_v_s 相对 med_v_hat 抬高约 +10（s2/s3）；IV=calib·v_s 仍系统性低于 v_pre（例 s3 Δv=75.577 thr=20） | **结构风险下 FAIL 合法**——平滑未抹平幅度缺口 |
| 正对照 | 三有效会话 `basePass=True` | 噪声门 / 估计器 OK；非测法假 FAIL |
| 负对照 | 三有效会话 `IVel0_t060_both_FAIL=True` | 对照成立 |
| composition.h | PRE=POST `a55d4952…e418` | 未改 |
| 影子 IV | ivel_set=calib·v_s；shadow_v_log 仅 log（例 s3 Arm-WGC-seed t060fwd k0: ivel_set=94.224 ≠ shadow=1361.87） | 无影子 IV 违规 |
| 未违规放宽 | 未抬 thr；未放宽 17/20；未改 N·order·ε·calib·α_v；未关 P-VS；未改 H5 分母；未偷换估窗 | 闸门纪律守住 |
| G2-LOCK-2b | s2/s3：比过 ∧ smoke 双灭 | **SEED-TRUTH-CONFLICT=HUNG**；票仍 = **SEED-PATHA-PRIME-FAIL** |
| s3 hang | 原 s3 hang_cap IVel0 t060rev too_few_frames n=0 → INVALID 归档 `s3_INVALID_*`；替换会话 FAIL_CANDIDATE | INVALID 可替换留证；不翻盘 |

**根因一句话**：**PATH-A′（V2-clamp+P-VS α_v=0.30）已触达且 calib KEEP；s1 因 cross-dir EMA 比出窗；s2/s3 比过但 t060 smoke 全灭 → SEED-TRUTH-CONFLICT 保持**——对照健全（baseP + IVel0F）→ **非测法崩坏**；结构风险（\|ivel−v_pre\|≈50+）下 FAIL 合法；预登记平滑臂已耗尽；残差在 **2F 比真值 vs 绝对 `v_pre` / smoke** 冲突，**超出本票 A′ 可修范围**。

**禁止解读**：禁止把「s2/s3 比过」误读成票 PASS；禁止静默改 thr / 17/20 / 偷换估窗 / 改 H5 分母；禁止重跑 `f17871f1…2362` / `a1161c77…3a43` / `09f6414b…793d` / `831818e4…80de` / `06467b82…805c` 翻盘；禁止标 `Δv-PASS` / `SEED-ABS-PASS` / `SEED-CALIB-PASS` / `SEED-PATHB-PASS` / 本票 PASS；禁止再开 α 调参 / 再标定救票；禁止立刻开 Δv″。

## 事实表

| 项 | 值 |
|---|---|
| TICKET / STATUS | `SEED-PATHA-PRIME-FAIL` / **CLOSED** |
| SEED_TRUTH_CONFLICT | **HUNG（G2-LOCK-2b）** |
| pass_sessions | **0/3**（3 有效 FAIL；1 INVALID 已替换） |
| path | PATHB-A′=V2-clamp+V3-VS；N=5；`t_int=min(t_comp,t_last_WGC)`；全程 A′；B=0 |
| calib | 显示 0.7511；exact=`0.7510829456`（**KEEP**；执行中未改） |
| P-VS | **ON**；α_v=**0.30**（写死；执行中未改） |
| H5 med_r / max_r | s1 **0.9289 / 6.5997**（any_ri≥2 → **比 FAIL**）；s2 **0.8527 / 0.9216**（比过）；s3 **0.8649 / 0.9173**（比过） |
| smoke t060fwd | **1/20 · 0/20(SKIP) · 0/20**（≪17；0/3 会话格 PASS） |
| smoke t060rev | **0/20 · 0/20 · 0/20**（≪17） |
| 对照 | basePass=T/T/T；IVel0_t060_both_FAIL=T/T/T |
| composition.h | PRE=POST `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` |
| 禁令核对 | 无影子 IV；未标 Δv-PASS/SEED-ABS-PASS/SEED-CALIB-PASS/SEED-PATHB-PASS；未翻盘五 FAIL gate（含本票）；≠PERF-CI；未解冻 Bv；未重开 S1/R-3/R-2；未开 M3；未抬 thr/放宽17/20/改N·α_v；未立刻 Δv″；未改 H5 分母 |

### 会话证据

| sess | appPID | note | t060fwd | t060rev | med_r | max_r | ratioOk | basePass | IVel0F | 判定 |
|---|---|---|---|---|---|---|---|---|---|---|
| 1 | 34936 | | FAIL 1/20 | FAIL 0/20 | 0.9289 | **6.5997** | **F** | T | T | FAIL_CANDIDATE |
| 2 | 63980 | | SKIP 0/20 | FAIL 0/20 | 0.8527 | 0.9216 | T | T | T | FAIL_CANDIDATE |
| 3 (orig) | 11164 | hang_cap IVel0 t060rev n=0；archived `s3_INVALID_*` | — | — | — | — | — | — | — | **INVALID**（已替换） |
| 3 (repl) | 62768 | replacement of INVALID | FAIL 0/20 | FAIL 0/20 | 0.8649 | 0.9173 | T | T | T | FAIL_CANDIDATE |

## 对照健全

| sess | basePass | IVel0_t060_both_FAIL | pathAUsed | ratioOk |
|---|---|---|---|---|
| 1 | True | True | True | False |
| 2 | True | True | True | True |
| 3 repl | True | True | True | True |

正对照 base 与负对照 IVel0 均满足；票仍 FAIL（s1 比出窗；s2/s3 smoke；SEED-TRUTH-CONFLICT）——**问题不在对照失效**。

## 不翻盘声明

- **本票闸门** `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362` @ 2026-10-26 14:38:15 Asia/Taipei：**不改 body/hash、不重跑翻盘**（结题落盘时已复算 body hash **match**）。
- **前序 PATH-B′ FAIL 闸门** `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`：**不改、不重跑、不宣称通过**。
- **前序 SEED-CALIB FAIL 闸门** `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`：**不改、不重跑、不宣称通过**。
- **前序 SEED-ABS FAIL 闸门** `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`：**不改、不重跑、不宣称通过**。
- **前序 Δv′ FAIL 闸门** `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`：**不改、不重跑、不宣称通过**。
- 结题标签只用 `SEED-PATHA-PRIME-*`；**禁止**写 `Δv-PASS` / `SEED-ABS-PASS` / `SEED-CALIB-PASS` / `SEED-PATHB-PASS` / 宣称前序或本票翻盘。
- 完整绝对门复测仍挂 **M2-Δv″**（须 seed 路径可过比回归+smoke 后再**另开新闸门**；**不立刻开**）。

## 产物清单

落盘前缀 `artifacts/m2_wgc_seed_path_a_prime_*`（及 docs 结题）：

- `docs/m2_wgc_seed_path_a_prime_result.md`（权威结题）
- `artifacts/m2_wgc_seed_path_a_prime_prereg.txt` + `_prereg_hash.txt`（prereg_hash=`6a12e95e1f7174c11357b9d81914a7416586070b901f2d492347022af3cfd69f`）
- `artifacts/m2_wgc_seed_path_a_prime_run_sessions.ps1` + `_run.log` / `_run_replace.log`
- `artifacts/m2_wgc_seed_path_a_prime_s{1,2,3}_{raw,summary,stdout,stderr}.txt`
- `artifacts/m2_wgc_seed_path_a_prime_s3_INVALID_{raw,summary,stdout,stderr}.txt`（hang 留证）
- `artifacts/m2_wgc_seed_path_a_prime_invalid_replace_log.txt`
- `artifacts/m2_wgc_seed_path_a_prime_session_matrix.txt`
- `artifacts/m2_wgc_seed_path_a_prime_composition_hash.txt`
- `artifacts/m2_wgc_seed_path_a_prime_ticket.txt` / `_ONELINE.txt` / `_bundle.txt`

## 禁令复述

- 不重开 S1 / R-3 / R-2。
- 不解冻 / 不升级 B_v。
- 成功 ≠ 采用 ≠ Δv-PASS ≠ SEED-ABS-PASS ≠ SEED-CALIB-PASS ≠ SEED-PATHB-PASS ≠ 本票 PASS 翻盘。
- 不改 `composition.h`（`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）。
- 禁影子 IV。
- **不改**已 LOCKED `f17871f1…2362`；**不重跑本票翻盘**。
- **不改 / 不重跑**前序 `a1161c77…3a43` / `09f6414b…793d` / `831818e4…80de` / `06467b82…805c`。
- 旁路挂账不挡主线；本票也不清旁路。
- PERF-CI 不降级；本票产物 ≠ PERF-CI 证据。
- 不开 M3 真机。
- 禁抬 thr / 降 17/20 / 偷换估窗 / 改 H5 分母伪装 PASS。
- **禁**再开 calib 废止票当主线；**禁**立刻开 Δv″；**禁**再跑 α 调参主线救票。

## 挂账 / 残差

| 项 | 处置 |
|---|---|
| M2-WGC-SEED-PATH-A′（本票） | **CLOSED SEED-PATHA-PRIME-FAIL**；LOCKED 归档不改；**SEED-TRUTH-CONFLICT=HUNG**；平滑臂**已耗尽** |
| **M2-SEED-TRUTH-CONFLICT** | **下一主线**（诊断/决议票；见下一票）——显式决议「2F 比真值 vs 绝对 v_pre / smoke」；**不**预授权改分母/抬 thr |
| SEED-TRUTH-CONFLICT | **挂账保持并升主线**（PATH-B′ 与 PATH-A′ 均触发） |
| M2-Δv″（绝对门复测） | **继续挂账**：须比回归+smoke 齐后再开**新闸门**；禁用五 FAIL gate 翻盘；**不立刻开** |
| 再开 α 调参 / 再标定 | **否** |
| M2-PERF-CI | 继续 HOLD |
| M3 | 不开真机 |

## 下一票决断

**选 A = M2-SEED-TRUTH-CONFLICT（诊断/决议票；标签建议 `SEED-TRUTH-*`）。**  
骨架 DRAFT：`docs/m2_seed_truth_conflict_gate_DRAFT.md`（**NOT LOCKED**；默认不开真机采数）。详见 `docs/notion_m2_wgc_seed_path_a_prime_next.md`。

### 候选速判

| 选项 | 决断 | 一句理由 |
|---|---|---|
| **A M2-SEED-TRUTH-CONFLICT** | **采纳** | PATH-B′ 与 PATH-A′ 均 CLOSED FAIL；平滑臂已耗尽；须显式决议比真值 vs 绝对冲突 |
| B 再跑 α 调参主线 | **否** | 预登记 α_v=0.30 已耗尽；禁默认同跑新臂偷换 |
| C 再开 calib / 重标定 | **否** | 禁再标定主线；calib KEEP 已触达 |
| D 立刻开 Δv″ | **否** | smoke 未齐；绝对门复测须冲突决议后再论 |
| E 重跑五 FAIL gate | **否** | 禁翻盘（含本票 `f17871f1…2362`） |
| F PERF-CI / 开 M3 | **否** | HOLD；不开 M3 |
| G STOP / 旁路挂起主线 | **否（本轮）** | 冲突可决议；无强理由停主线 |

### 协调下一步一句

请派同伴按 `docs/m2_seed_truth_conflict_gate_DRAFT.md` 补洞（offline/文档决议为主；冲突根因 + 可选处置菜单每项另票 LOCK；禁翻盘五 FAIL；禁立刻 Δv″；PERF-CI HOLD）→ Notion 收齐后可 LOCK 修订 → LOCK → 问 pidan APPROVED（默认不开真机；真机采数须本票 LOCK+APPROVED 显式授权且范围写死）。

— notion ai（M2-WGC-SEED-PATH-A′ 结题；ticket=CLOSED SEED-PATHA-PRIME-FAIL；SEED-TRUTH-CONFLICT=HUNG；下一票=M2-SEED-TRUTH-CONFLICT）
