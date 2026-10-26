# workbuddy 派单：M2-WGC-SEED-PATH-B′（自包含）

派单时间：2026-10-26 13:51:27 Asia/Taipei
执行环境：Windows DESKTOP-K93N3P8，经 CodeBuddy 执行。**1 物理机合法**。

## 授权
- 闸门：`docs/m2_wgc_seed_path_b_prime_gate_LOCKED.md`（STATUS=LOCKED，LOCKED_AT=2026-10-26 13:49:20 Asia/Taipei，GATE_BODY_SHA256=a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43）
- 批准：`docs/m2_wgc_seed_path_b_prime_approved.md`（APPROVED_BY=pidan，APPROVED_AT=2026-10-26 13:51:27 Asia/Taipei）
- **严格逐字遵循 LOCKED 闸门**；不得修改闸门 body / hash；不得在本闸门下追加补测（违规 → INVALID，须新 LOCK）。
- 前序三 FAIL 闸门 **禁止重跑翻盘**；产物禁标 `Δv-PASS` / `SEED-ABS-PASS` / `SEED-CALIB-PASS`。

## 范围（仅 PATH-B′，依 LOCKED）
- **默认 A = PATHB-A = V2-clamp**：`t_int = min(t_comp, t_last_WGC)`（**禁**过后外推）；KEEP N=5 / order=2 / calib exact=`0.7510829456`（显示 0.7511）/ ε=5 / **P-VS=OFF**。
- **A′**（A+P-VS α_v=0.30）：**挂账次选，本票不同跑**。
- H5 比回归：med_r∈[0.8,1.2] 且无 r_i≥2。
- smoke=t060 fwd+rev；二次 w=5 + thr KEEP + P1/P2；≥17/20；base PASS；IVel0 t060 双 FAIL。
- 结构风险：KEEP calib + H5@2F 下 |ivel−v_pre|≈50 可能仍 FAIL → **FAIL 合法，禁抬 thr / 放宽 17/20 / 改 N**。
- 比过 ∧ smoke 双灭 → 挂账 SEED-TRUTH-CONFLICT（本票仍标 SEED-PATHB-FAIL）。
- 结题标签 ∈ {`SEED-PATHB-PASS`,`SEED-PATHB-FAIL`,`SEED-PATHB-INVALID`}。
- ≥3 独立进程；1 物理机；≠ PERF-CI。

## 硬禁令
- composition.h PRE/POST = `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`
- 不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；成功≠采用；不开 M3；禁翻盘 09f6414b…793d / 831818e4…80de / 06467b82…805c。

## 产出
- `artifacts/m2_wgc_seed_path_b_prime_*`（不得冒充 calib/abs/Δv′ 翻盘包）+ `docs/m2_wgc_seed_path_b_prime_result.md` + G4 ONELINE：
  `M2-WGC-SEED-PATH-B′ | gate=a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43 @ 2026-10-26 13:49:20 Asia/Taipei | approved=pidan@2026-10-26 13:51:27 Asia/Taipei | path=PATHB-A=V2-clamp | calib=0.7511(exact=0.7510829456) | smoke=t060 | ratio=reg | ctrl=baseP/IVel0F | ticket=<SEED-PATHB-PASS/FAIL/INVALID> | 成功≠采用≠Δv-PASS；不改composition.h；禁影子IV；≠PERF-CI；三闸门不翻盘`

## 参考
- `docs/m2_wgc_seed_path_b_prime_gate_LOCKED.md`
- `docs/m2_wgc_seed_path_b_prime_approved.md`
- `docs/notion_m2_wgc_seed_path_b_prime_gate_revise.md`
- `docs/deepseek_m2_wgc_seed_path_b_prime_design.md`
