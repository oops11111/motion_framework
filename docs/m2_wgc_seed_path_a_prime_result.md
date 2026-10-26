# M2-WGC-SEED-PATH-A′ 结题结果

STATUS: CLOSED
TICKET: SEED-PATHA-PRIME-FAIL
SEED_TRUTH_CONFLICT: HUNG — H5 ratioOk 在 s2/s3 过，但 t060 fwd+rev smoke 均未达 ≥17/20 格 PASS；s1 另有 any_ri≥2
WRITTEN_AT: 2026-10-26 15:33:30 Asia/Taipei
GATE: docs/m2_wgc_seed_path_a_prime_gate_LOCKED.md
GATE_BODY_SHA256: f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362
LOCKED_AT: 2026-10-26 14:38:15 Asia/Taipei
APPROVED: pidan@2026-10-26 14:45:22 Asia/Taipei
DISPATCH: docs/workbuddy_m2_wgc_seed_path_a_prime_dispatch.md
prereg_hash: 6a12e95e1f7174c11357b9d81914a7416586070b901f2d492347022af3cfd69f

## G4 ONELINE

M2-WGC-SEED-PATH-A′ | gate=f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362 @ 2026-10-26 14:38:15 Asia/Taipei | approved=pidan@2026-10-26 14:45:22 Asia/Taipei | path=PATHB-A′=V2-clamp+V3-VS/N=5/α_v=0.30 | calib=0.7511(exact=0.7510829456 KEEP) | P-VS=ON | smoke=t060 | ratio=reg | ctrl=baseP/IVel0F | ticket=SEED-PATHA-PRIME-FAIL | 成功≠采用≠Δv-PASS；不改composition.h；禁影子IV；≠PERF-CI；四闸门不翻盘；SEED-TRUTH-CONFLICT挂账；结构风险见G1

## 路径声明

- **PATHB-A′ = V2-clamp + V3-VS**：打断前末 N=5 帧 WGC 二次 LS；`t_int = min(t_comp, t_last_WGC)`（禁外推过末样）
- **ε=5.0 在 VS 前判** `|v̂|<ε` → 回退 B；**不是**对 `v_s` 判（`eps_before_vs=true`）
- **P-VS=ON；α_v=0.30 写死**：`v_s = α_v·v̂ + (1−α_v)·v_s_prev`；首样 `v_s=v̂`；限幅不设
- `InitialVelocity = (float)(calib · v_s)`（非 calib·v̂）；exact calib=`0.7510829456`（显示 0.7511）KEEP
- KEEP：N=5 / order=2 / ε=5 / calib；执行中未改 α_v、未关 P-VS、未偷换 PATHB-A
- 禁影子 IV：`shadow_v_log` 仅日志；例 s3 Arm-WGC-seed t060fwd k0: ivel_set=94.224 (=calib·v_s=0.7510829456·125.451) ≠ shadow_v_log=1361.87
- 测量：二次 LS w=5；thr KEEP；P1/P2 KEEP；**未**为过门改 thr/17/20/N/order/ε/calib/α_v
- t_int 钳制落实：每有效会话 `t_int_clamp n_clamped≈40`；例 s3 t_comp=110.095 / t_last_WGC=96.222 / t_int=96.222 / clamped=True

## 机器 / 会话

- machine=DESKTOP-K93N3P8；60Hz；≠PERF-CI；≠VM
- ≥3 独立进程；FAIL 不翻盘；INVALID 可替换留证；本闸门下无补测（仅替换 INVALID）

| sess | appPID | dotnetPID | note | verdict |
|---|---|---|---|---|
| 1 | 34936 | 35344 | | FAIL_CANDIDATE |
| 2 | 63980 | 34408 | | FAIL_CANDIDATE |
| 3 (orig) | 11164 | 46332 | hang_cap IVel0 t060rev too_few_frames n=0；archived `s3_INVALID_*` | INVALID |
| 3 (repl) | 62768 | 35600 | replacement of INVALID | FAIL_CANDIDATE |

pass_sessions=0/3（3 有效 FAIL；1 INVALID 已替换）

## H5 比回归（必要）

`r_i=|v_seed/v_screen|`；判据 med_r∈[0.8,1.2] 且无 r_i≥2.0

| sess | n | med_r | max_r | any_ri≥2 | ratioOk | pathA_n | pathB_n | med_v_s | med_v_hat |
|---|---|---|---|---|---|---|---|---|---|
| 1 | 40 | 0.9289 | 6.5997 | **True** | **False** | 41 | 0 | 152.349 | 151.517 |
| 2 | 39 | 0.8527 | 0.9216 | False | **True** | 40 | 0 | 137.020 | 127.671 |
| 3 repl | 40 | 0.8649 | 0.9173 | False | **True** | 40 | 0 | 137.596 | 127.569 |

→ s1 因 cross-dir EMA 致 any_ri≥2 出窗；s2/s3 比回归过。α_v 平滑后 med_r 仍贴下沿（0.85–0.86），未改 calib/分母救票。

## 绝对 Δv smoke（t060 必测）

格 PASS 需 ≥17/20 且可判

| sess | cell | Nv_h | thrMed | \|a\|σ | jud | wgc_pass | base_pass | IVel0_pass | cell verdict |
|---|---|---|---|---|---|---|---|---|---|
| 1 | t060fwd | 13.562 | 20 | 10.528 | True | **1/20** | 19/20 | 0/20 | FAIL |
| 1 | t060rev | 10.241 | 20 | 10.761 | True | **0/20** | 20/20 | 3/20 | FAIL |
| 2 | t060fwd | 51.722 | 20 | 10.661 | False | **0/20** | 18/20 | 0/20 | SEED-PATHA-PRIME-SKIP |
| 2 | t060rev | 16.864 | 20 | 10.723 | True | **0/20** | 20/20 | 2/20 | FAIL |
| 3 | t060fwd | 11.196 | 20 | 10.795 | True | **0/20** | 20/20 | 0/20 | FAIL |
| 3 | t060rev | 8.806 | 20 | 10.881 | True | **0/20** | 20/20 | 2/20 | FAIL |

→ 0/3 会话 t060 fwd+rev 格 PASS；结构风险下 |ivel−v_pre| 仍大（例 s3 Δv=75.577 thr=20）

## SEED-TRUTH-CONFLICT 挂账

- 条件满足（s2、s3）：**H5 比回归过** ∧ **t060 fwd 与 rev smoke 均未达 ≥17/20**
- 本票结局 = **SEED-PATHA-PRIME-FAIL**（不得标 PASS / Δv-PASS / SEED-ABS/CALIB/PATHB-PASS）
- **保持挂账 `SEED-TRUTH-CONFLICT`**
- **禁**抬 thr / 改 calib / 改 H5 分母 / 立刻开 Δv″
- **未**因此抬 thr / 放宽 17/20 / 改 N·order·ε·calib·α_v / 关 P-VS

## 对照

| sess | basePass | IVel0_t060_both_FAIL | pathAUsed | ratioOk |
|---|---|---|---|---|
| 1 | True | True | True | False |
| 2 | True | True | True | True |
| 3 repl | True | True | True | True |

→ ctrl=baseP/IVel0F 全有效会话满足

## composition.h

- PRE = `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`
- POST = `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`
- PRE=POST；未改

## 禁令遵守

- 未重开 S1/R-3/R-2；未解冻 Bv；成功≠采用；未开 M3
- 未重跑/翻盘前序四 FAIL：PATH-B′ a1161c77…3a43 / CALIB 09f6414b…793d / ABS 831818e4…80de / Δv′ 06467b82…805c
- 产物仅标 `SEED-PATHA-PRIME-*`；未冒充 path_b/calib/abs/delta_v_prime
- 无合成输入；不花钱；≠PERF-CI

## 结构风险（G1）

α_v=0.30 跨向 EMA 可致首几样/换向后 r_i 尖刺（s1 max_r=6.60）；VS 后 med_v_s 相对 med_v_hat 抬高约 +10（s2/s3），IV=calib·v_s 仍系统性低于 v_pre → smoke 双灭。PATH-B′ 已贴 med_r 下沿，A′ 进一步平滑未救绝对 Δv。

## 产物清单（前缀 m2_wgc_seed_path_a_prime_*）

见 artifacts/ 与 docs/m2_wgc_seed_path_a_prime_result.md；含 s1–s3 raw/summary、s3_INVALID_* 留证、session_matrix、prereg、ONELINE、ticket、run logs。
