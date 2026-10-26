# M2-WGC-SEED-ABS 结题结果

STATUS: CLOSED
TICKET: SEED-ABS-FAIL
WRITTEN_AT: 2026-10-26 11:53:30 Asia/Taipei
GATE: docs/m2_wgc_seed_abs_gate_LOCKED.md
GATE_BODY_SHA256: 831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de
LOCKED_AT: 2026-10-26 10:59:25 Asia/Taipei
APPROVED: pidan@2026-10-26 11:15:50 Asia/Taipei
DISPATCH: docs/workbuddy_m2_wgc_seed_abs_dispatch.md
prereg_hash: ec8950125ede7c94306e0b201f7e3aedc1b9bc07caf79dc90db1b2abda652c17

## G4 ONELINE

M2-WGC-SEED-ABS | gate=831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de @ 2026-10-26 10:59:25 Asia/Taipei | approved=pidan@2026-10-26 11:15:50 Asia/Taipei | path=P-LS+TS/N=5 | smoke=t060 | ratio=reg | ctrl=baseP/IVel0F | ticket=SEED-ABS-FAIL | 成功≠采用≠Δv-PASS；不改composition.h；禁影子IV；≠PERF-CI；06467b82不翻盘

## 路径声明

- A = P-LS+TS：打断前末 N=5 帧 WGC 二次 LS @ t_int（QPC）；`InitialVelocity=(float)(calib·v̂)`；calib=1.0；ε=5.0；P-VS=OFF
- 回退 B = IV null+0（本三会话 pathB_n=0，全程 A）
- 禁影子 IV：`shadow_v_log` 仅日志；`ivel_set` = LS `v̂`（例 s1 Arm-WGC-seed k0: ivel_set=153.278 ≠ shadow_v_log=1362.90）
- 测量：二次 LS w=5；thr=max(20,0.05·|v_ref|)；P1/P2 同 Δv′ 升版工具（非翻盘）

## 机器 / 会话

- machine=DESKTOP-K93N3P8；60Hz（DwmFlush≈60）；≠PERF-CI；≠VM
- 3 独立进程（禁池化；FAIL 不翻盘）

| sess | appPID | dotnetPID | start (Asia/Taipei) | done | verdict |
|---|---|---|---|---|---|
| 1 | 59192 | 59936 | 11:23:28 | 11:32:56 | FAIL_CANDIDATE |
| 2 | 5060 | 37500 | 11:33:05 | 11:42:35 | FAIL_CANDIDATE |
| 3 | 24724 | 42988 | 11:42:44 | 11:52:16 | FAIL_CANDIDATE |

pass_sessions=0/3

## H5 比回归（必要；比出窗不得以 smoke 豁免）

`r_i=|v_seed/v_screen|`；v_screen=打断前末两帧 WGC

| sess | n | med_r | max_r | any_ri≥2 | ratioOk | pathA_n | pathB_n |
|---|---|---|---|---|---|---|---|
| 1 | 40 | 1.3315 | 1.4133 | False | **False** | 40 | 0 |
| 2 | 40 | 1.3294 | 1.4161 | False | **False** | 40 | 0 |
| 3 | 40 | 1.3352 | 1.4477 | False | **False** | 40 | 0 |

→ med_r ∉ [0.8,1.2] → 比回归未过（FAIL 充分条件之一）

## 绝对 Δv smoke（t060 必测）

格 PASS 需 ≥17/20 且可判

| sess | cell | Nv_h | thrMed | \|a\|σ | jud | wgc_pass | base_pass | IVel0_pass | cell verdict |
|---|---|---|---|---|---|---|---|---|---|
| 1 | t060fwd | 6.133 | 20 | 10.858 | True | **8/20** | 20/20 | 0/20 | FAIL |
| 1 | t060rev | 12.898 | 20 | 10.740 | True | **1/20** | 20/20 | 1/20 | FAIL |
| 2 | t060fwd | 11.476 | 20 | 10.756 | True | **11/20** | 19/20 | 0/20 | FAIL |
| 2 | t060rev | 18.604 | 20 | 10.706 | True | **0/20** | 19/20 | 0/20 | FAIL |
| 3 | t060fwd | 14.591 | 20 | 10.545 | True | **11/20** | 19/20 | 0/20 | FAIL |
| 3 | t060rev | 17.174 | 20 | 10.726 | True | **0/20** | 19/20 | 5/20 | FAIL |

→ 0/3 会话 t060 fwd+rev 格 PASS

## 对照

| sess | basePass | IVel0_t060_both_FAIL | pathAUsed |
|---|---|---|---|
| 1 | True | True | True |
| 2 | True | True | True |
| 3 | True | True | True |

正对照 base 与负对照 IVel0 均满足；票仍 FAIL（smoke+ratio）

## composition.h

- expected / PRE / POST = `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`
- PRE=POST；未改 composition.h

## 禁令核验

- 无影子 IV（ivel_set=LS v̂；shadow_v_log 仅 log）
- 产物前缀 `m2_wgc_seed_abs_*`；**未**标 Δv-PASS；**未**重跑/翻盘 gate `06467b82…805c`
- ≠PERF-CI；未解冻 Bv；未重开 S1/R-3/R-2；未开 M3
- 结题标签只用 SEED-ABS-*；成功≠采用≠Δv-PASS

## 票结局判定（G2-LOCK-5）

- PASS 条件未满足：0/3 会话 smoke 格 PASS；H5 ratioOk=False
- 有效样本足、可判、路径 A、对照齐 → **SEED-ABS-FAIL**（非 INVALID）
- FAIL 不翻盘；无本闸门下补测

## 产物清单

- artifacts/m2_wgc_seed_abs_prereg.txt (+ prereg_hash)
- artifacts/m2_wgc_seed_abs_run_sessions.ps1
- artifacts/m2_wgc_seed_abs_run.log
- artifacts/m2_wgc_seed_abs_session_matrix.txt
- artifacts/m2_wgc_seed_abs_composition_hash.txt
- artifacts/m2_wgc_seed_abs_s{1,2,3}_{raw,summary,stdout,stderr}.txt
- artifacts/m2_wgc_seed_abs_capture_{raw,summary}.txt（末会话）
- artifacts/m2_wgc_seed_abs_build.log
- artifacts/m2_wgc_seed_abs.flag
- docs/m2_wgc_seed_abs_result.md（本文件）
- probe: MotionSoftDemo/M2WgcSeedAbsProbe.cs（flag MF_M2_WGC_SEED_ABS）

## 备注

LS@t_int seed（~145–160 DIP/s）相对末两帧 v_screen（~118）系统偏高 → med_r≈1.33；绝对 Δv 在 Arm-WGC-seed 上仍远超 thr=20（典型 Δv 30–100），未能把 t060 推到 ≥17/20。base 臂仍格 PASS，说明测量/环境可判；问题在 seed→compositor 绝对匹配，非对照失效。
