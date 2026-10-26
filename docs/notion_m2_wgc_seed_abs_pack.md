# M2-WGC-SEED-ABS 结题中文包（notion_m2_wgc_seed_abs_pack.md）

> DECISION_AT=2026-10-26 11:57:00 Asia/Taipei  
> 权威结题：`docs/m2_wgc_seed_abs_result.md`（WRITTEN_AT=2026-10-26 11:53:30 Asia/Taipei）  
> 闸门 LOCKED **勿改 hash**：`docs/m2_wgc_seed_abs_gate_LOCKED.md` GATE_BODY_SHA256=`831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de` @ 2026-10-26 10:59:25 Asia/Taipei  
> APPROVED：pidan@2026-10-26 11:15:50 Asia/Taipei（`docs/m2_wgc_seed_abs_approved.md`）  
> 真机：DESKTOP-K93N3P8（1 物理机；60 Hz；DwmFlush≈60）；≠PERF-CI；≠VM  
> 前序 M2-Δv′ FAIL gate `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`：**未重跑、未翻盘**

## 结论

- **票级 = CLOSED FAIL**；标签 **`SEED-ABS-FAIL`**（非 INVALID；非 Δv-PASS）。
- G4 ONELINE（原文照抄）：  
  `M2-WGC-SEED-ABS | gate=831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de @ 2026-10-26 10:59:25 Asia/Taipei | approved=pidan@2026-10-26 11:15:50 Asia/Taipei | path=P-LS+TS/N=5 | smoke=t060 | ratio=reg | ctrl=baseP/IVel0F | ticket=SEED-ABS-FAIL | 成功≠采用≠Δv-PASS；不改composition.h；禁影子IV；≠PERF-CI；06467b82不翻盘`
- **成功 ≠ 采用 ≠ Δv-PASS**；不解冻 / 不升级 B_v；不重开 S1 / R-3 / R-2；`composition.h` PRE=POST `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` **未改**；本闸门 body/hash **未改**。
- **不重跑本 FAIL 闸门翻盘**；**不翻盘**前序 `06467b82…805c`。
- **本票 ≠ PERF-CI**；PERF-CI 仍 HOLD `5b007dd4…282a`；不开 M3 真机。

## FAIL 归因（写死）

| 层 | 事实 | 含义 |
|---|---|---|
| 路径 | 全程 pathA=P-LS+TS/N=5；pathB_n=0；calib=1.0；ε=5.0；P-VS=OFF | 默认路径已触达；非回退 B 虚判 |
| H5 比回归 | 三会话 med_r≈1.3315 / 1.3294 / 1.3352 ∉[0.8,1.2]；无 r_i≥2；ratioOk=False | **比回归 FAIL 充分**（不得以 smoke 豁免） |
| 绝对 smoke | t060fwd 8–11/20；t060rev 0–1/20（均 ≪17）；三会话均可判 | 0/3 会话 smoke 格 PASS |
| 尺度 | LS seed~145–160 DIP/s vs v_screen~118 → Δv≫thr=20 | 种子相对屏速**系统偏大** |
| 正对照 | 三会话 `basePass=True` | 噪声门 / 估计器 OK；非测法假 FAIL |
| 负对照 | 三会话 `IVel0_t060_both_FAIL=True` | 对照成立 |
| composition.h | PRE=POST `a55d4952…e418` | 未改 |
| 影子 IV | ivel_set=LS v̂；shadow_v_log 仅 log（例 s1 ivel_set=153.278 ≠ shadow=1362.90） | 无影子 IV 违规 |

**根因一句话**：P-LS+TS/N=5 在 calib=1.0 下，种子相对 `v_screen` 系统偏大（med_r≈1.33）→ **比回归与 t060 smoke 双 FAIL**；对照健全（baseP + IVel0F）→ **非测法崩坏**，属 seed 标定/尺度残差。

**禁止解读**：禁止把本 FAIL 归因回对照/可判规则；禁止静默改 thr / 17/20 / 偷换估窗；禁止重跑 `831818e4…80de` 或 `06467b82…805c` 翻盘；禁止标 `Δv-PASS`。

## 事实表

| 项 | 值 |
|---|---|
| TICKET / STATUS | `SEED-ABS-FAIL` / **CLOSED** |
| pass_sessions | **0/3**（非 INVALID） |
| path | A=P-LS+TS/N=5；全程 A；B=0 |
| H5 med_r | s1 1.3315；s2 1.3294；s3 1.3352（均 ∉[0.8,1.2]；无 r_i≥2） |
| smoke t060fwd | 8/20 · 11/20 · 11/20（可判） |
| smoke t060rev | 1/20 · 0/20 · 0/20（可判） |
| 对照 | basePass=T/T/T；IVel0_t060_both_FAIL=T/T/T |
| composition.h | PRE=POST `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` |
| 禁令核对 | 无影子 IV；未标 Δv-PASS；未翻盘 06467b82；≠PERF-CI；未解冻 Bv；未重开 S1/R-3/R-2；未开 M3 |

### 会话证据

| sess | appPID | start→done (Asia/Taipei) | t060fwd | t060rev | med_r | basePass | IVel0F | 判定 |
|---|---|---|---|---|---|---|---|---|
| 1 | 59192 | 11:23:28→11:32:56 | FAIL 8/20 | FAIL 1/20 | 1.3315 | T | T | FAIL_CANDIDATE |
| 2 | 5060 | 11:33:05→11:42:35 | FAIL 11/20 | FAIL 0/20 | 1.3294 | T | T | FAIL_CANDIDATE |
| 3 | 24724 | 11:42:44→11:52:16 | FAIL 11/20 | FAIL 0/20 | 1.3352 | T | T | FAIL_CANDIDATE |

## 不翻盘声明

- **本票闸门** `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de` @ 2026-10-26 10:59:25 Asia/Taipei：**不改 body/hash、不重跑翻盘**。
- **前序 Δv′ FAIL 闸门** `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`：**不改、不重跑、不宣称通过**。
- 结题标签只用 `SEED-ABS-*`；**禁止**写 `Δv-PASS` / 宣称 M2-Δv′ 翻盘。
- 完整绝对门复测仍挂 **M2-Δv″**（须 seed 路径可过回归后再**另开新闸门**）。

## 产物清单

落盘前缀 `artifacts/m2_wgc_seed_abs_*`（及 docs 结题）：

- `artifacts/m2_wgc_seed_abs_prereg.txt` + `…_prereg_hash.txt`（prereg_hash=`ec8950125ede7c94306e0b201f7e3aedc1b9bc07caf79dc90db1b2abda652c17`）
- `artifacts/m2_wgc_seed_abs_run_sessions.ps1`
- `artifacts/m2_wgc_seed_abs_run.log` / `…_build.log` / `…_ticket.txt`
- `artifacts/m2_wgc_seed_abs_session_matrix.txt`
- `artifacts/m2_wgc_seed_abs_composition_hash.txt`
- `artifacts/m2_wgc_seed_abs_s{1,2,3}_{raw,summary,stdout,stderr}.txt`
- `artifacts/m2_wgc_seed_abs_capture_{raw,summary}.txt`（末会话）
- `artifacts/m2_wgc_seed_abs_bundle.zip`
- `artifacts/m2_wgc_seed_abs_result.md`（产物侧副本）
- `docs/m2_wgc_seed_abs_result.md`（权威结题）
- 探针：`MotionSoftDemo/M2WgcSeedAbsProbe.cs`（flag `MF_M2_WGC_SEED_ABS`）

> 注：result 清单曾列 `artifacts/m2_wgc_seed_abs.flag`；结题包落盘时该文件**不在** artifacts 目录——不影响票结局（以 result 会话矩阵与 smoke/比表为准）。

## 禁令复述

- 不重开 S1 / R-3 / R-2。
- 不解冻 / 不升级 B_v。
- 成功 ≠ 采用 ≠ Δv-PASS。
- 不改 `composition.h`（`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）。
- 禁影子 IV。
- **不改**已 LOCKED `831818e4…80de`；**不重跑本票翻盘**。
- **不改 / 不重跑**前序 `06467b82…805c`。
- 旁路挂账不挡主线；本票也不清旁路。
- PERF-CI 不降级；本票产物 ≠ PERF-CI 证据。
- 不开 M3 真机。

## 挂账 / 残差

| 项 | 处置 |
|---|---|
| M2-WGC-SEED-ABS（本票） | **CLOSED SEED-ABS-FAIL**；LOCKED 归档不改 |
| **M2-WGC-SEED-CALIB** | **残差主票**（立刻开；见下一票）——KEEP P-LS+TS/N=5；**只**标定 calib 常量；新闸门 |
| M2-WGC-SEED-PATH-B′ | 挂账：若 CALIB 后仍方向性偏置（rev 全灭）再开 |
| M2-Δv″（绝对门复测） | **继续挂账**：须 seed 可过回归后再开**新闸门**；禁用本票/Δv′ FAIL 闸门翻盘 |
| M2-VEL-SMOOTH | 仍并入 seed 设计空间；不另并行真机 |
| M2-L / TOOLING / REFRESH | 继续挂账；REFRESH 不并行（M3/C5b） |
| M2-PERF-CI | 继续 HOLD |

## 下一票决断

**选 A = M2-WGC-SEED-CALIB（标定票）。**  
骨架 DRAFT：`docs/m2_wgc_seed_calib_gate_DRAFT.md`（**NOT LOCKED**；不授权采数）。详见 `docs/notion_m2_wgc_seed_abs_next.md`。

### 候选速判

| 选项 | 决断 | 一句理由 |
|---|---|---|
| **A 修 calib 常量** | **采纳** | med_r≈1.33 系统偏大是主因；KEEP 路径族；新闸门 |
| B PATH-B′（改 N/TS/P-VS） | 挂账 | 方向性偏置待 calib 后再验；本轮不先动估窗 |
| C 立刻开 Δv″ | **否** | seed 比回归未过；绝对门复测须 seed 后新闸门 |
| D 重跑本 FAIL / 06467b82 | **否** | 禁翻盘 |
| E PERF-CI / 开 M3 | **否** | HOLD；不开 M3 |

### 协调下一步一句

请派同伴按 `docs/m2_wgc_seed_calib_gate_DRAFT.md` 补洞（标定式、比窗预期、虚 PASS 否决）→ Notion 收齐后可 LOCK 修订 → LOCK → 问 pidan APPROVED（单机；PERF-CI 仍 HOLD；禁改 831818/06467b82）。

— notion ai（M2-WGC-SEED-ABS 结题；ticket=CLOSED SEED-ABS-FAIL；下一票=M2-WGC-SEED-CALIB）
