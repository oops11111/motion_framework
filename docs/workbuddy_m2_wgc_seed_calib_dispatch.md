# workbuddy 派单：M2-WGC-SEED-CALIB（自包含）

派单时间：2026-10-26 13:04:53 Asia/Taipei
执行环境：Windows DESKTOP-K93N3P8，经 CodeBuddy 执行。**1 物理机合法**。

## 授权
- 闸门：`docs/m2_wgc_seed_calib_gate_LOCKED.md`（STATUS=LOCKED，LOCKED_AT=2026-10-26 12:03:33 Asia/Taipei，GATE_BODY_SHA256=09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d）
- 批准：`docs/m2_wgc_seed_calib_approved.md`（APPROVED_BY=pidan，APPROVED_AT=2026-10-26 13:04:53 Asia/Taipei）
- **严格逐字遵循 LOCKED 闸门**；不得修改闸门 body / hash；不得在本闸门下追加补测（违规 → INVALID，须新 LOCK）。
- 前序 SEED-ABS gate `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de` 与 Δv′ gate `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c` **禁止重跑翻盘**；产物**禁止**标 `Δv-PASS` / `SEED-ABS-PASS`。

## 范围（仅 M2-WGC-SEED-CALIB，依 LOCKED）
- **KEEP 路径 A = P-LS+TS**：末 N=5 二次 LS @ `t_int`；`InitialVelocity=(float)(calib·v̂)`。
- **calib 预登记**：显示 **0.7511**；比较/写入用 **exact=0.7510829456**（ABS pathA n=120 median；废止 1.0）；执行中改 = INVALID。
- ε=5.0；P-VS 默认关；回退 B=IV null+0；票 PASS 只评 A。
- **H5 比回归**：`med_r∈[0.8,1.2]` 且无 `r_i≥2.0`。
- **smoke**：t060 fwd+rev；二次 w=5 + thr KEEP + P1/P2；≥17/20；base PASS；IVel0 t060 双 FAIL。
- **PATH-B′ 触发**：比回归过但 t060 rev（或单侧）smoke 仍 FAIL → 本票 FAIL/条件 FAIL 并挂 PATH-B′；**禁止**为救 rev 改 thr / 放宽 17/20 / 改 N/order。
- 结题标签 ∈ {`SEED-CALIB-PASS`,`SEED-CALIB-FAIL`,`SEED-CALIB-INVALID`}。
- ≥3 独立进程；1 物理机；≠ PERF-CI。

## 硬禁令
- composition.h PRE/POST = `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`
- 不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；成功≠采用；不开 M3；禁翻盘双 FAIL 闸门。

## 产出
- `artifacts/m2_wgc_seed_calib_*`（不得冒充 m2_wgc_seed_abs_* / m2_delta_v_prime_*）+ `docs/m2_wgc_seed_calib_result.md` + G4 ONELINE：
  `M2-WGC-SEED-CALIB | gate=09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d @ 2026-10-26 12:03:33 Asia/Taipei | approved=pidan@2026-10-26 13:04:53 Asia/Taipei | path=P-LS+TS/N=5 | calib=0.7511(exact=0.7510829456) | smoke=t060 | ratio=reg | ctrl=baseP/IVel0F | ticket=<SEED-CALIB-PASS/FAIL/INVALID> | 成功≠采用≠Δv-PASS；不改composition.h；禁影子IV；≠PERF-CI；831818/06467b82不翻盘`

## 参考
- `docs/m2_wgc_seed_calib_gate_LOCKED.md`
- `docs/m2_wgc_seed_calib_approved.md`
- `docs/notion_m2_wgc_seed_calib_gate_revise.md`
- `docs/deepseek_m2_wgc_seed_calib_review.md`
