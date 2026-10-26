# workbuddy 派单：M2-WGC-SEED-PATH-A′（自包含）

派单时间：2026-10-26 14:45:22 Asia/Taipei
执行环境：Windows DESKTOP-K93N3P8，经 CodeBuddy 执行。**1 物理机合法**。

## 授权
- 闸门：`docs/m2_wgc_seed_path_a_prime_gate_LOCKED.md`（STATUS=LOCKED，LOCKED_AT=2026-10-26 14:38:15 Asia/Taipei，GATE_BODY_SHA256=f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362）
- 批准：`docs/m2_wgc_seed_path_a_prime_approved.md`（APPROVED_BY=pidan，APPROVED_AT=2026-10-26 14:45:22 Asia/Taipei）
- **严格逐字遵循 LOCKED 闸门**；不得修改闸门 body / hash；不得在本闸门下追加补测（违规 → INVALID，须新 LOCK）。
- 前序四 FAIL 闸门 **禁止重跑翻盘**；产物禁标 `Δv-PASS` / `SEED-ABS-PASS` / `SEED-CALIB-PASS` / `SEED-PATHB-PASS`。

## 范围（仅 PATH-A′，依 LOCKED）
- **默认主路径 PATHB-A′ = V2-clamp + V3-VS**：`t_int = min(t_comp, t_last_WGC)`（**禁**过后外推）；KEEP N=5 / order=2 / calib exact=`0.7510829456`（显示 0.7511）/ ε=5；**P-VS=ON；α_v=0.30 写死**；首样 `v_s=v̂`；限幅不设；IV=`(float)(calib·v_s)`；ε 在 VS **前**判。
- prereg 显式字段：`pathb_variant=PATHB-A′`、`P-VS=ON`、`α_v=0.30`、`calib_exact=0.7510829456`、`t_int` 钳制、`eps_before_vs=true`；产物记 `α_v`/`v_s`（或等价）。
- H5 比回归：med_r∈[0.8,1.2] 且无 r_i≥2（必要；不得 smoke 豁免）。注意 PATH-B′ med_r 贴下沿——α 可能进一步欠 seed → 出窗则 FAIL，**不得**改 calib/分母救票。
- smoke=t060 fwd+rev；二次 w=5 + thr KEEP + P1/P2；≥17/20；base PASS；IVel0 t060 双 FAIL。
- 结构风险：KEEP calib + H5@2F 下 |ivel−v_pre|≈50+ 可能仍 FAIL → **FAIL 合法，禁抬 thr / 放宽 17/20 / 改 N·α·calib / 改 H5 分母**。
- 比过 ∧ smoke 双灭 → 本票 `SEED-PATHA-PRIME-FAIL` + **保持** SEED-TRUTH-CONFLICT 挂账（不预授权改 H5）。
- 结题标签 ∈ {`SEED-PATHA-PRIME-PASS`,`SEED-PATHA-PRIME-FAIL`,`SEED-PATHA-PRIME-INVALID`}（禁写作 `SEED-PATHB-PASS`）。
- ≥3 独立进程；1 物理机；≠ PERF-CI；A′ 为本票主路径（非 PATH-B′ 默认同跑第二臂）。

## 硬禁令
- composition.h PRE/POST = `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`
- 不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；成功≠采用；不开 M3；禁立刻 Δv″；禁翻盘 a1161c77…3a43 / 09f6414b…793d / 831818e4…80de / 06467b82…805c。
- 执行中改 α_v / 关 P-VS 偷换回 PATHB-A / 改 N·order·ε·calib = INVALID。

## 产出
- `artifacts/m2_wgc_seed_path_a_prime_*`（不得冒充 path_b / calib / abs / Δv′ 翻盘包）+ `docs/m2_wgc_seed_path_a_prime_result.md` + G4 ONELINE：
  `M2-WGC-SEED-PATH-A′ | gate=f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362 @ 2026-10-26 14:38:15 Asia/Taipei | approved=pidan@2026-10-26 14:45:22 Asia/Taipei | path=PATHB-A′=V2-clamp+V3-VS/N=5/α_v=0.30 | calib=0.7511(exact=0.7510829456 KEEP) | P-VS=ON | smoke=t060 | ratio=reg | ctrl=baseP/IVel0F | ticket=<SEED-PATHA-PRIME-PASS/FAIL/INVALID> | 成功≠采用≠Δv-PASS；不改composition.h；禁影子IV；≠PERF-CI；四闸门不翻盘；SEED-TRUTH-CONFLICT挂账；结构风险见G1`

## 参考
- `docs/m2_wgc_seed_path_a_prime_gate_LOCKED.md`
- `docs/m2_wgc_seed_path_a_prime_approved.md`
- `docs/notion_m2_wgc_seed_path_a_prime_gate_revise.md`
- `docs/deepseek_m2_wgc_seed_path_a_prime_review.md`
