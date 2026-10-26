# M2-WGC-SEED-CALIB 结题结果

STATUS: CLOSED
TICKET: SEED-CALIB-FAIL
PATH_B_PRIME: HUNG (G2-LOCK-2b) — H5 ratioOk 全会话过，但 t060 fwd+rev smoke 均未达 ≥17/20
WRITTEN_AT: 2026-10-26 13:39:29 Asia/Taipei
GATE: docs/m2_wgc_seed_calib_gate_LOCKED.md
GATE_BODY_SHA256: 09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d
LOCKED_AT: 2026-10-26 12:03:33 Asia/Taipei
APPROVED: pidan@2026-10-26 13:04:53 Asia/Taipei
DISPATCH: docs/workbuddy_m2_wgc_seed_calib_dispatch.md
prereg_hash: 9c84530896eac5ade0c9a0df2fe9a3bfed05a17acc7781b906840cb18cdc6c91

## G4 ONELINE

M2-WGC-SEED-CALIB | gate=09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d @ 2026-10-26 12:03:33 Asia/Taipei | approved=pidan@2026-10-26 13:04:53 Asia/Taipei | path=P-LS+TS/N=5 | calib=0.7511(exact=0.7510829456) | smoke=t060 | ratio=reg | ctrl=baseP/IVel0F | ticket=SEED-CALIB-FAIL | 成功≠采用≠Δv-PASS；不改composition.h；禁影子IV；≠PERF-CI；831818/06467b82不翻盘；PATH-B′挂账触发见G2-LOCK-2b

## 路径声明

- A = P-LS+TS：打断前末 N=5 帧 WGC 二次 LS @ t_int（QPC）；`InitialVelocity=(float)(calib·v̂)`
- **calib 实际写入 exact=0.7510829456**（显示 0.7511；summary F 打印 0.751083）；废止 1.0；执行中未改
- ε=5.0；P-VS=OFF；回退 B=IV null+0（本三会话 pathB_n=0，全程 A）
- 禁影子 IV：`shadow_v_log` 仅日志；例 s1 Arm-WGC-seed k0: ivel_set=119.775 (=calib·v_hat=0.7510829456·159.470) ≠ shadow_v_log=1363.49
- 测量：二次 LS w=5；thr=max(20,0.05·|v_ref|) KEEP；P1/P2 KEEP；**未**为过门改 thr/17/20/N/order/ε

## 机器 / 会话

- machine=DESKTOP-K93N3P8；60Hz；≠PERF-CI；≠VM
- 3 独立进程（禁池化；FAIL 不翻盘；本闸门下无补测）

| sess | appPID | dotnetPID | start (Asia/Taipei) | done | verdict |
|---|---|---|---|---|---|
| 1 | 27796 | 9180 | 13:09:23 | 13:18:53 | FAIL_CANDIDATE |
| 2 | 40692 | 13228 | 13:19:02 | 13:28:33 | FAIL_CANDIDATE |
| 3 | 1340 | 53732 | 13:28:42 | 13:38:13 | FAIL_CANDIDATE |

pass_sessions=0/3

## H5 比回归（必要）

`r_i=|v_seed/v_screen|`；v_screen=打断前末两帧 WGC

| sess | n | med_r | max_r | any_ri≥2 | ratioOk | pathA_n | pathB_n |
|---|---|---|---|---|---|---|---|
| 1 | 40 | 0.9893 | 1.1024 | False | **True** | 40 | 0 |
| 2 | 40 | 1.0002 | 1.0380 | False | **True** | 40 | 0 |
| 3 | 40 | 1.0007 | 1.0820 | False | **True** | 40 | 0 |

→ med_r ∈ [0.8,1.2] 且无 r_i≥2.0 → **比回归过**（calib 达成 H5 目标）

## 绝对 Δv smoke（t060 必测）

格 PASS 需 ≥17/20 且可判

| sess | cell | Nv_h | thrMed | \|a\|σ | jud | wgc_pass | base_pass | IVel0_pass | cell verdict |
|---|---|---|---|---|---|---|---|---|---|
| 1 | t060fwd | 6.799 | 20 | 10.753 | True | **1/20** | 20/20 | 0/20 | FAIL |
| 1 | t060rev | 12.697 | 20 | 10.702 | True | **0/20** | 19/20 | 2/20 | FAIL |
| 2 | t060fwd | 7.864 | 20 | 10.776 | True | **3/20** | 20/20 | 0/20 | FAIL |
| 2 | t060rev | 18.850 | 20 | 10.669 | True | **0/20** | 19/20 | 3/20 | FAIL |
| 3 | t060fwd | 10.075 | 20 | 10.760 | True | **1/20** | 20/20 | 0/20 | FAIL |
| 3 | t060rev | 10.227 | 20 | 10.742 | True | **0/20** | 20/20 | 4/20 | FAIL |

→ 0/3 会话 t060 fwd+rev 格 PASS；rev 侧尤甚（0/20×3）

## PATH-B′ 挂账（G2-LOCK-2b）

- 条件满足：**H5 比回归过** ∧ **t060 fwd 与 rev smoke 均 FAIL**（未达 ≥17/20）
- 本票结局 = **SEED-CALIB-FAIL**（不得标 PASS）
- **挂账 PATH-B′**（N/TS/P-VS / 分方向估法须另开新闸门）
- **未**因此抬 thr / 放宽 17/20 / 改 N·order·ε / 偷换估窗

## 对照

| sess | basePass | IVel0_t060_both_FAIL | pathAUsed |
|---|---|---|---|
| 1 | True | True | True |
| 2 | True | True | True |
| 3 | True | True | True |

正对照 base 与负对照 IVel0 均满足；票仍 FAIL（smoke；PATH-B′）

## composition.h

- expected / PRE / POST = `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`
- PRE=POST；未改 composition.h

## 禁令核验

- 无影子 IV（ivel_set=calib·LS v̂；shadow_v_log 仅 log）
- 产物前缀 `m2_wgc_seed_calib_*`；**未**标 Δv-PASS / SEED-ABS-PASS；**未**重跑/翻盘 gate `831818e4…80de` 或 `06467b82…805c`
- ≠PERF-CI；未解冻 Bv；未重开 S1/R-3/R-2；未开 M3
- 结题标签只用 SEED-CALIB-*；成功≠采用≠Δv-PASS≠SEED-ABS-PASS
- calib 仅预登记 exact；执行中未改

## 票结局判定（G2-LOCK-5）

- PASS 条件未满足：0/3 会话 smoke 格 PASS（尽管 H5 ratioOk=True 且对照齐）
- 有效样本足、可判、路径 A、对照齐、composition 未改 → **SEED-CALIB-FAIL**（非 INVALID）
- 触发 G2-LOCK-2b → PATH-B′ 挂账
- FAIL 不翻盘；无本闸门下补测

## 产物清单

- artifacts/m2_wgc_seed_calib_prereg.txt (+ prereg_hash)
- artifacts/m2_wgc_seed_calib_run_sessions.ps1
- artifacts/m2_wgc_seed_calib_run.log
- artifacts/m2_wgc_seed_calib_session_matrix.txt
- artifacts/m2_wgc_seed_calib_composition_hash.txt
- artifacts/m2_wgc_seed_calib_s{1,2,3}_{raw,summary,stdout,stderr}.txt
- artifacts/m2_wgc_seed_calib_capture_{raw,summary}.txt（末会话）
- artifacts/m2_wgc_seed_calib_build.log
- artifacts/M2WgcSeedCalibProbe.cs
- docs/m2_wgc_seed_calib_result.md（本文件）
- probe: MotionSoftDemo/M2WgcSeedCalibProbe.cs（flag MF_M2_WGC_SEED_CALIB / m2_wgc_seed_calib.flag）

## 备注

calib=0.7510829456 把 med_r 从 ABS≈1.33 拉回 ≈0.99–1.00（H5 比过），但绝对 Δv 在 Arm-WGC-seed 上仍远超 thr=20（fwd 典型 1–3/20；rev 0/20）。base 臂仍格 PASS → 测量/环境可判；问题不在对照失效，而在 seed→compositor 绝对匹配超出单标量 calib 可修范围 → PATH-B′。
