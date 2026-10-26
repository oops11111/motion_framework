# M2-WGC-SEED-PATH-B′ 结题结果

STATUS: CLOSED
TICKET: SEED-PATHB-FAIL
SEED_TRUTH_CONFLICT: HUNG (G2-LOCK-2b) — H5 ratioOk 全会话过，但 t060 fwd+rev smoke 均未达 ≥17/20 格 PASS
WRITTEN_AT: 2026-10-26 14:27:40 Asia/Taipei
GATE: docs/m2_wgc_seed_path_b_prime_gate_LOCKED.md
GATE_BODY_SHA256: a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43
LOCKED_AT: 2026-10-26 13:49:20 Asia/Taipei
APPROVED: pidan@2026-10-26 13:51:27 Asia/Taipei
DISPATCH: docs/workbuddy_m2_wgc_seed_path_b_prime_dispatch.md
prereg_hash: ad7335017c618cc82150990c453fa2c4888fb7dce23f1c084dc26bc9dc276b64

## G4 ONELINE

M2-WGC-SEED-PATH-B′ | gate=a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43 @ 2026-10-26 13:49:20 Asia/Taipei | approved=pidan@2026-10-26 13:51:27 Asia/Taipei | path=PATHB-A=V2-clamp/N=5/t_int=min(t_comp,t_last_WGC) | calib=0.7511(exact=0.7510829456 KEEP) | P-VS=OFF | smoke=t060 | ratio=reg | ctrl=baseP/IVel0F | ticket=SEED-PATHB-FAIL | SEED-TRUTH-CONFLICT | 成功≠采用≠Δv-PASS；不改composition.h；禁影子IV；≠PERF-CI；09f6414b/831818/06467b82不翻盘；A′挂账；Δv″继续挂账；结构风险见G1；SEED-TRUTH-CONFLICT见G2-LOCK-2b

## 路径声明

- **PATHB-A = V2-clamp**：打断前末 N=5 帧 WGC 二次 LS @ `t_int = min(t_compositor_interrupt_QPC, t_last_WGC)`；**禁**外推超过 `t_last_WGC`
- **calib 实际写入 exact=0.7510829456**（显示 0.7511）；KEEP；执行中未改；未再标定
- ε=5.0；**P-VS=OFF**；回退 B=IV null+0（本三会话 pathB_n=0，全程 A）
- **A′ 不同跑**（挂账次选）
- 禁影子 IV：`shadow_v_log` 仅日志；例 s1 Arm-WGC-seed t060fwd k0: ivel_set=94.783 (=calib·v_hat=0.7510829456·126.195) ≠ shadow_v_log=1362.56
- 测量：二次 LS w=5；thr=max(20,0.05·|v_ref|) KEEP；P1/P2 KEEP；**未**为过门改 thr/17/20/N/order/ε
- t_int 钳制落实：每会话 `t_int_clamp n_clamped=40`；例 t_comp=110.054ms / t_last_WGC=98.159ms / t_int=98.159ms / clamped=True

## 机器 / 会话

- machine=DESKTOP-K93N3P8；60Hz；≠PERF-CI；≠VM
- 3 独立进程（禁池化；FAIL 不翻盘；本闸门下无补测）

| sess | appPID | dotnetPID | start (Asia/Taipei) | done | verdict |
|---|---|---|---|---|---|
| 1 | 35864 | 47612 | 13:56:11 | ~14:05:50 | FAIL_CANDIDATE |
| 2 | 56216 | 36272 | 14:06:08 | 14:15:42 | FAIL_CANDIDATE |
| 3 | 54392 | 60748 | 14:15:51 | 14:25:22 | FAIL_CANDIDATE |

pass_sessions=0/3

## H5 比回归（必要）

`r_i=|v_seed/v_screen|`；v_screen=打断前末两帧 WGC

| sess | n | med_r | max_r | any_ri≥2 | ratioOk | pathA_n | pathB_n |
|---|---|---|---|---|---|---|---|
| 1 | 40 | 0.9465 | 0.9607 | False | **True** | 40 | 0 |
| 2 | 40 | 0.8055 | 0.9502 | False | **True** | 40 | 0 |
| 3 | 40 | 0.8062 | 0.9538 | False | **True** | 40 | 0 |

→ med_r ∈ [0.8,1.2] 且无 r_i≥2.0 → **比回归过**

## 绝对 Δv smoke（t060 必测）

格 PASS 需 ≥17/20 且可判

| sess | cell | Nv_h | thrMed | \|a\|σ | jud | wgc_pass | base_pass | IVel0_pass | cell verdict |
|---|---|---|---|---|---|---|---|---|---|
| 1 | t060fwd | 7.129 | 20 | 10.812 | True | **0/20** | 20/20 | 0/20 | FAIL |
| 1 | t060rev | 10.454 | 20 | 10.756 | True | **0/20** | 20/20 | 2/20 | FAIL |
| 2 | t060fwd | 8.498 | 20 | 10.709 | True | **0/20** | 20/20 | 0/20 | FAIL |
| 2 | t060rev | 20.630 | 20 | 10.656 | False | **0/20** | 18/20 | 0/20 | SEED-PATHB-SKIP |
| 3 | t060fwd | 11.448 | 20 | 10.529 | True | **0/20** | 19/20 | 0/20 | FAIL |
| 3 | t060rev | 10.350 | 20 | 10.735 | True | **0/20** | 20/20 | 5/20 | FAIL |

→ 0/3 会话 t060 fwd+rev 格 PASS；结构风险下 |ivel−v_pre|≈50+ 仍见（例 Δv=86.185 thr=20）

## SEED-TRUTH-CONFLICT 挂账（G2-LOCK-2b）

- 条件满足：**H5 比回归过** ∧ **t060 fwd 与 rev smoke 均未达 ≥17/20**
- 本票结局 = **SEED-PATHB-FAIL**（不得标 PASS）
- **挂账 `SEED-TRUTH-CONFLICT`**：2F 比真值 vs 绝对 `v_pre` 冲突
- **禁**再开 calib 废止票当主线；**禁**抬 thr；**不**预授权改 H5 分母
- **未**因此抬 thr / 放宽 17/20 / 改 N·order·ε·calib / 偷换估窗
- **未**立刻开 Δv″；A′ 继续挂账次选（本票不同跑）

## 对照

| sess | basePass | IVel0_t060_both_FAIL | pathAUsed |
|---|---|---|---|
| 1 | True | True | True |
| 2 | True | True | True |
| 3 | True | True | True |

## composition.h

- expected = `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`
- PRE = POST = `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`
- **未改**

## 禁令合规

- 未重跑/翻盘 SEED-CALIB `09f6414b…793d` / SEED-ABS `831818e4…80de` / Δv′ `06467b82…805c`
- 产物前缀仅 `m2_wgc_seed_path_b_prime_*`；结题标签仅 `SEED-PATHB-*`
- 禁影子 IV；≠PERF-CI；未开 M3；未解冻 Bv；未重开 S1/R-3/R-2
- 成功≠采用≠Δv-PASS≠SEED-ABS-PASS≠SEED-CALIB-PASS

## 产物清单

- `docs/m2_wgc_seed_path_b_prime_result.md`（本文件）
- `artifacts/m2_wgc_seed_path_b_prime_prereg.txt` + `_prereg_hash.txt`
- `artifacts/m2_wgc_seed_path_b_prime_run_sessions.ps1` + `_run.log`
- `artifacts/m2_wgc_seed_path_b_prime_s{1,2,3}_{raw,summary,stdout,stderr}.txt`
- `artifacts/m2_wgc_seed_path_b_prime_session_matrix.txt`
- `artifacts/m2_wgc_seed_path_b_prime_composition_hash.txt`
- `artifacts/m2_wgc_seed_path_b_prime_ticket.txt`
- `artifacts/m2_wgc_seed_path_b_prime_bundle.zip`
- `artifacts/M2WgcSeedPathBPrimeProbe.cs` + `MotionSoftDemo/M2WgcSeedPathBPrimeProbe.cs`

— workbuddy executor（M2-WGC-SEED-PATH-B′ CLOSED SEED-PATHB-FAIL + SEED-TRUTH-CONFLICT）
