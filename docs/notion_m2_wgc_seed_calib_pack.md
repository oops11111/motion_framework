# M2-WGC-SEED-CALIB 结题中文包（notion_m2_wgc_seed_calib_pack.md）

> DECISION_AT=2026-10-26 13:42:30 Asia/Taipei  
> 权威结题：`docs/m2_wgc_seed_calib_result.md`（WRITTEN_AT=2026-10-26 13:39:29 Asia/Taipei）  
> 闸门 LOCKED **勿改 hash**：`docs/m2_wgc_seed_calib_gate_LOCKED.md` GATE_BODY_SHA256=`09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d` @ 2026-10-26 12:03:33 Asia/Taipei  
> APPROVED：pidan@2026-10-26 13:04:53 Asia/Taipei（`docs/m2_wgc_seed_calib_approved.md`）  
> 真机：DESKTOP-K93N3P8（1 物理机；60 Hz）；≠PERF-CI；≠VM  
> 前序 SEED-ABS FAIL gate `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`：**未重跑、未翻盘**  
> 前序 Δv′ FAIL gate `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`：**未重跑、未翻盘**  
> **PATH_B_PRIME=HUNG（G2-LOCK-2b）**

## 结论

- **票级 = CLOSED FAIL**；标签 **`SEED-CALIB-FAIL`**（非 INVALID；非 Δv-PASS；非 SEED-ABS-PASS）。
- G4 ONELINE（原文照抄）：  
  `M2-WGC-SEED-CALIB | gate=09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d @ 2026-10-26 12:03:33 Asia/Taipei | approved=pidan@2026-10-26 13:04:53 Asia/Taipei | path=P-LS+TS/N=5 | calib=0.7511(exact=0.7510829456) | smoke=t060 | ratio=reg | ctrl=baseP/IVel0F | ticket=SEED-CALIB-FAIL | 成功≠采用≠Δv-PASS；不改composition.h；禁影子IV；≠PERF-CI；831818/06467b82不翻盘；PATH-B′挂账触发见G2-LOCK-2b`
- **成功 ≠ 采用 ≠ Δv-PASS ≠ SEED-ABS-PASS**；不解冻 / 不升级 B_v；不重开 S1 / R-3 / R-2；`composition.h` PRE=POST `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` **未改**；本闸门 body/hash **未改**。
- **不重跑本 FAIL 闸门翻盘**；**不翻盘**前序 `831818e4…80de` 与 `06467b82…805c`。
- **本票 ≠ PERF-CI**；PERF-CI 仍 HOLD `5b007dd4…282a`；不开 M3 真机。
- **PATH_B_PRIME=HUNG**：比回归过 ∧ t060 fwd+rev smoke 均 FAIL → 触发 G2-LOCK-2b。

## FAIL 归因（写死）

| 层 | 事实 | 含义 |
|---|---|---|
| 路径 | 全程 pathA=P-LS+TS/N=5；pathB_n=0；calib exact=`0.7510829456`；ε=5.0；P-VS=OFF | 默认路径已触达；非回退 B 虚判 |
| H5 比回归 | 三会话 med_r≈0.9893 / 1.0002 / 1.0007 ∈[0.8,1.2]；无 r_i≥2；ratioOk=True×3 | **比回归过**——**calib 达成尺度目标** |
| 绝对 smoke | t060fwd 1–3/20；t060rev **0/20×3**（均 ≪17）；三会话均可判 | 0/3 会话 smoke 格 PASS |
| 尺度 vs 绝对 | calib 把 med_r 从 ABS≈1.33 拉回 ≈0.99–1.00，但 Arm-WGC-seed 绝对 Δv 仍远超 thr=20 | **单标量 calib 已尽；绝对匹配超出可修范围** |
| 正对照 | 三会话 `basePass=True` | 噪声门 / 估计器 OK；非测法假 FAIL |
| 负对照 | 三会话 `IVel0_t060_both_FAIL=True` | 对照成立 |
| composition.h | PRE=POST `a55d4952…e418` | 未改 |
| 影子 IV | ivel_set=calib·LS v̂；shadow_v_log 仅 log（例 s1 ivel_set=119.775 ≠ shadow=1363.49） | 无影子 IV 违规 |
| 未违规放宽 | 未抬 thr；未放宽 17/20；未改 N·order·ε；未偷换估窗 | 闸门纪律守住 |

**根因一句话**：**calib 尺度目标达成（比回归过），绝对 smoke 仍 FAIL 且触发 PATH-B′（G2-LOCK-2b）**——对照健全（baseP + IVel0F）→ **非测法崩坏**；残差在估窗几何 / TS / P-VS / 方向性，**超出单标量 calib**。

**禁止解读**：禁止把本 FAIL 归因回对照/可判规则；禁止静默改 thr / 17/20 / 偷换估窗；禁止重跑 `09f6414b…793d` / `831818e4…80de` / `06467b82…805c` 翻盘；禁止标 `Δv-PASS` / `SEED-ABS-PASS`；禁止把「比过」误读成票 PASS。

## 事实表

| 项 | 值 |
|---|---|
| TICKET / STATUS | `SEED-CALIB-FAIL` / **CLOSED** |
| PATH_B_PRIME | **HUNG（G2-LOCK-2b）** |
| pass_sessions | **0/3**（非 INVALID） |
| path | A=P-LS+TS/N=5；全程 A；B=0 |
| calib | 显示 0.7511；exact=`0.7510829456`（执行中未改） |
| H5 med_r | s1 0.9893；s2 1.0002；s3 1.0007（均 ∈[0.8,1.2]；无 r_i≥2；**比过**） |
| smoke t060fwd | 1/20 · 3/20 · 1/20（可判；≪17） |
| smoke t060rev | 0/20 · 0/20 · 0/20（可判；≪17） |
| 对照 | basePass=T/T/T；IVel0_t060_both_FAIL=T/T/T |
| composition.h | PRE=POST `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` |
| 禁令核对 | 无影子 IV；未标 Δv-PASS/SEED-ABS-PASS；未翻盘 831818/06467b82；≠PERF-CI；未解冻 Bv；未重开 S1/R-3/R-2；未开 M3；未抬 thr/放宽17/20/改N |

### 会话证据

| sess | appPID | start→done (Asia/Taipei) | t060fwd | t060rev | med_r | ratioOk | basePass | IVel0F | 判定 |
|---|---|---|---|---|---|---|---|---|---|
| 1 | 27796 | 13:09:23→13:18:53 | FAIL 1/20 | FAIL 0/20 | 0.9893 | T | T | T | FAIL_CANDIDATE |
| 2 | 40692 | 13:19:02→13:28:33 | FAIL 3/20 | FAIL 0/20 | 1.0002 | T | T | T | FAIL_CANDIDATE |
| 3 | 1340 | 13:28:42→13:38:13 | FAIL 1/20 | FAIL 0/20 | 1.0007 | T | T | T | FAIL_CANDIDATE |

## 对照健全

| sess | basePass | IVel0_t060_both_FAIL | pathAUsed |
|---|---|---|---|
| 1 | True | True | True |
| 2 | True | True | True |
| 3 | True | True | True |

正对照 base 与负对照 IVel0 均满足；票仍 FAIL（smoke；PATH-B′）——**问题不在对照失效**。

## 不翻盘声明

- **本票闸门** `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d` @ 2026-10-26 12:03:33 Asia/Taipei：**不改 body/hash、不重跑翻盘**（结题落盘时已复算 hash **match**）。
- **前序 SEED-ABS FAIL 闸门** `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`：**不改、不重跑、不宣称通过**。
- **前序 Δv′ FAIL 闸门** `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`：**不改、不重跑、不宣称通过**。
- 结题标签只用 `SEED-CALIB-*`；**禁止**写 `Δv-PASS` / `SEED-ABS-PASS` / 宣称前序翻盘。
- 完整绝对门复测仍挂 **M2-Δv″**（须 PATH-B′ 等 seed 路径可过比回归+smoke 后再**另开新闸门**）。

## 产物清单

落盘前缀 `artifacts/m2_wgc_seed_calib_*`（及 docs 结题）：

- `artifacts/m2_wgc_seed_calib_prereg.txt` + `…_prereg_hash.txt`（prereg_hash=`9c84530896eac5ade0c9a0df2fe9a3bfed05a17acc7781b906840cb18cdc6c91`）
- `artifacts/m2_wgc_seed_calib_run_sessions.ps1`
- `artifacts/m2_wgc_seed_calib_run.log` / `…_build.log` / `…_ticket.txt`
- `artifacts/m2_wgc_seed_calib_session_matrix.txt`
- `artifacts/m2_wgc_seed_calib_composition_hash.txt`
- `artifacts/m2_wgc_seed_calib_s{1,2,3}_{raw,summary,stdout,stderr}.txt`
- `artifacts/m2_wgc_seed_calib_capture_{raw,summary}.txt`（末会话）
- `artifacts/m2_wgc_seed_calib_bundle.zip`
- `artifacts/m2_wgc_seed_calib_result.md`（产物侧副本）
- `artifacts/M2WgcSeedCalibProbe.cs`
- `docs/m2_wgc_seed_calib_result.md`（权威结题）
- 探针：`MotionSoftDemo/M2WgcSeedCalibProbe.cs`（flag `MF_M2_WGC_SEED_CALIB` / `m2_wgc_seed_calib.flag`）

## 禁令复述

- 不重开 S1 / R-3 / R-2。
- 不解冻 / 不升级 B_v。
- 成功 ≠ 采用 ≠ Δv-PASS ≠ SEED-ABS-PASS。
- 不改 `composition.h`（`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）。
- 禁影子 IV。
- **不改**已 LOCKED `09f6414b…793d`；**不重跑本票翻盘**。
- **不改 / 不重跑**前序 `831818e4…80de` 与 `06467b82…805c`。
- 旁路挂账不挡主线；本票也不清旁路。
- PERF-CI 不降级；本票产物 ≠ PERF-CI 证据。
- 不开 M3 真机。
- 禁抬 thr / 降 17/20 / 偷换估窗伪装 PASS。

## 挂账 / 残差

| 项 | 处置 |
|---|---|
| M2-WGC-SEED-CALIB（本票） | **CLOSED SEED-CALIB-FAIL**；LOCKED 归档不改；**PATH_B_PRIME=HUNG** |
| **M2-WGC-SEED-PATH-B′** | **残差主票**（立刻开；见下一票）——KEEP calib exact；估窗/TS/P-VS/方向性变体；新闸门 |
| M2-Δv″（绝对门复测） | **继续挂账**：须 PATH-B′ 可过比回归+smoke 后再开**新闸门**；禁用本票/ABS/Δv′ FAIL 闸门翻盘 |
| 重开 calib 废止/重标定 | **否**（除非 PATH-B′ 变体显式要求）；本票 exact 已证尺度到位 |
| M2-VEL-SMOOTH | 可并入 PATH-B′ 设计空间（P-VS）；不另并行真机 |
| M2-L / TOOLING / REFRESH | 继续挂账；REFRESH 不并行（M3/C5b） |
| M2-PERF-CI | 继续 HOLD |

## 下一票决断

**选 A = M2-WGC-SEED-PATH-B′（估窗/TS/P-VS/方向性变体票）。**  
骨架 DRAFT：`docs/m2_wgc_seed_path_b_prime_gate_DRAFT.md`（**NOT LOCKED**；不授权采数）。详见 `docs/notion_m2_wgc_seed_calib_next.md`。

### 候选速判

| 选项 | 决断 | 一句理由 |
|---|---|---|
| **A PATH-B′（N/TS/P-VS/方向性）** | **采纳** | G2-LOCK-2b 已 HUNG：比过 + smoke（尤其 rev 0/20）FAIL；单标量 calib 已尽 |
| B 再开 calib / 重标定 | **否** | med_r 已 ≈1.00；重开 calib 废止无新证据 |
| C 立刻开 Δv″ | **否** | 比过但 smoke 未齐；绝对门复测须 seed 后新闸门 |
| D 重跑本 FAIL / 831818 / 06467b82 | **否** | 禁翻盘 |
| E PERF-CI / 开 M3 | **否** | HOLD；不开 M3 |
| F 先估窗诊断票（不改路径） | **否（本轮）** | 闸门已写死挂账 PATH-B′；诊断可并入 PATH-B′ 同伴补洞，不必另开挡主线票 |

### 协调下一步一句

请派同伴按 `docs/m2_wgc_seed_path_b_prime_gate_DRAFT.md` 补洞（变体择一或分级：估窗几何 / P-TS / P-VS / 方向性）→ Notion 收齐后可 LOCK 修订 → LOCK → 问 pidan APPROVED（单机；KEEP calib exact；PERF-CI 仍 HOLD；禁改 09f6414b/831818/06467b82）。

— notion ai（M2-WGC-SEED-CALIB 结题；ticket=CLOSED SEED-CALIB-FAIL；PATH_B_PRIME=HUNG；下一票=M2-WGC-SEED-PATH-B′）
