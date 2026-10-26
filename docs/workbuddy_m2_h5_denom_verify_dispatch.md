# workbuddy 派单：M2-H5-DENOM-VERIFY（自包含）

派单时间：2026-10-26 16:12:48 Australia/Perth
执行环境：Windows DESKTOP-K93N3P8，经 CodeBuddy 执行。**1 物理机合法**。

## 授权
- 闸门：`docs/m2_h5_denom_verify_gate_LOCKED.md`（STATUS=LOCKED，LOCKED_AT=2026-10-26 16:09:00 Asia/Taipei，GATE_BODY_SHA256=fd00a06058b239abfe03a52175463a52741718318ce99416a4ce762cf037889b）
- 批准：`docs/m2_h5_denom_verify_approved.md`（APPROVED_BY=pidan，APPROVED_AT=2026-10-26 16:12:48 Australia/Perth）
- 显式字段：验证采数=**是**；落地=**否**；calib=**C1=median(v_denom/v_hat)**；路径=**PATHB-A**
- **严格逐字遵循 LOCKED 闸门**；不得修改闸门 body / hash；不得在本闸门下追加补测（违规 → INVALID，须新 LOCK）。
- 七 hash **禁止重跑翻盘**；产物禁标 `Δv-PASS` / 前序 `SEED-*-PASS` / `SEED-TRUTH-PASS` / 重写 `H5-DENOM-PASS` 为已落地。

## 范围（仅 VERIFY，依 LOCKED）
1. **Phase-Calib**：t060 fwd+rev、≥3 独立进程；PATHB-A（P-LS+TS · N=5 · order=2 · V2-clamp · P-VS=OFF）；IV 可用 `calib_probe=1.0` 或导出 `v_hat`（**不依赖**旧 0.7511）；分母严格 `v_denom≡v_pre`（二次 LS · w=5 · 同估点）；汇总 **`calib_new = median(v_denom/v_hat)`** → 落盘 `artifacts/m2_h5_denom_verify_calib_*`（写死 exact）。
2. **Phase-Score**：冻结 `calib_new`；同路径 PATHB-A：`InitialVelocity=(float)(calib_new·v̂)`；评 **H5 比**（`r_i=|v_seed/v_denom|`，`med_r∈[0.8,1.2]` 且无 `r_i≥2`）+ **smoke ≥17/20**（thr KEEP；禁抬 thr/降 17/20）；计分窗禁再改 calib。
3. A′ **默认不启用**；禁执行中偷开。
4. 对照 KEEP：basePass ∧ IVel0_t060_both_FAIL（有则 KEEP）。
5. 结题标签 ∈ {`H5-DENOM-VERIFY-PASS`,`H5-DENOM-VERIFY-FAIL`,`H5-DENOM-VERIFY-INVALID`,`H5-DENOM-VERIFY-STOP`}。
6. ≥3 独立进程；1 物理机；≠ PERF-CI；**不落地**。

## 硬禁令
- composition.h PRE/POST = `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`
- 不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；成功≠采用≠落地采用；不开 M3；禁立刻 Δv″；禁静默 0.7511
- 禁翻盘：PATH-A′ `f17871f1…2362` / PATH-B′ `a1161c77…3a43` / CALIB `09f6414b…793d` / ABS `831818e4…80de` / Δv′ `06467b82…805c` / TRUTH `276b97f0…4f69` / DENOM `d891df9f…33c7`

## 产出
- `artifacts/m2_h5_denom_verify_*` + `artifacts/m2_h5_denom_verify_calib_*`（写死 exact）
- `docs/m2_h5_denom_verify_result.md`
- G4 ONELINE：
  `M2-H5-DENOM-VERIFY | gate=fd00a06058b239abfe03a52175463a52741718318ce99416a4ce762cf037889b @ 2026-10-26 16:09:00 Asia/Taipei | approved=pidan@2026-10-26 16:12:48 Australia/Perth | mode=verify | ticket=<H5-DENOM-VERIFY-PASS/FAIL/INVALID/STOP> | KEEP v_denom≡v_pre；path=PATHB-A；calib=C1 exact=<…>；禁静默0.7511；验证采数=是；落地=否；七hash不翻盘；禁抬thr；成功≠落地采用≠Δv-PASS；≠PERF-CI；禁立刻Δv″；单机 DESKTOP-K93N3P8`

## 参考
- `docs/m2_h5_denom_verify_gate_LOCKED.md`
- `docs/m2_h5_denom_verify_approved.md`
- `docs/notion_m2_h5_denom_verify_gate_revise.md`
- `docs/deepseek_m2_h5_denom_verify_review.md`
- `docs/m2_h5_denom_revise_gate_LOCKED.md`（契约源；不改）
