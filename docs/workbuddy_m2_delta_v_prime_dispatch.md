# workbuddy 派单：M2-Δv′（自包含）

派单时间：2026-10-26 09:09:19 AWST
执行环境：Windows DESKTOP-K93N3P8，经 CodeBuddy 执行。**1 物理机合法**。

## 授权
- 闸门：`docs/m2_delta_v_prime_gate_LOCKED.md`（STATUS=LOCKED，LOCKED_AT=2026-10-26 09:05:37 AWST，GATE_BODY_SHA256=06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c）
- 批准：`docs/m2_delta_v_prime_approved.md`（APPROVED_BY=pidan，APPROVED_AT=2026-10-26 09:09:19 AWST）
- **严格逐字遵循 LOCKED 闸门**；不得修改闸门 body / hash；不得在本闸门下追加补测（违规 → INVALID，须新 LOCK）。
- 前序 M2-Δv gate `d2349ceb…09c6` **禁止重跑翻盘**。

## 范围（仅 M2-Δv′，依 LOCKED）
- **被测** Arm-WGC-seed；**正对照** base 须 Δv-PASS；**负对照** IVel=0 在 t060 fwd+rev 须 FAIL。
- 可判核心 5：t060{fwd,rev}+t300{fwd,rev,dbl}；n=20；≥17/20；禁放宽 17/20；无中位句。
- thr=max(20, 0.05·|v_ref|) KEEP；估计器二次 LS w=5 KEEP；禁两帧差分。
- **升版可判**：`|a|·σ_ts ≤ thr`（去 /2）；`thr ≥ Nv_h_cell`（去 2×）；PASS 样本仍须 Nv_h≤thr_i。
- 可判后 WGC 差 → **Δv-FAIL**（禁再标 INCONCLUSIVE）。
- IVel0·t300dbl **不进票**；崩 ≠ INVALID。
- ≥3 独立进程会话；禁池化；t156 规则 SKIP ACCEPT。
- 1 物理机；≠ PERF-CI。

## 硬禁令
- composition.h PRE/POST = `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`
- 不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；成功≠采用；不开 M3。

## 产出
- `artifacts/m2_delta_v_prime_*` + `docs/m2_delta_v_prime_result.md` + G4 ONELINE：
  `M2-Δv′ | gate=06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c @ 2026-10-26 09:05:37 AWST | approved=pidan@2026-10-26 09:09:19 AWST | sessions=… | cells=… | ctrl=… | ticket=<PASS/FAIL/INCONCLUSIVE/INVALID> | t156=rule-SKIP | 成功≠采用；不改composition.h；禁影子IV；≠PERF-CI`

## 参考
- `docs/m2_delta_v_prime_gate_LOCKED.md`
- `docs/m2_delta_v_prime_approved.md`
- `docs/notion_m2_delta_v_prime_gate_revise.md`
- `docs/gemini_m2_delta_v_prime_design.md`
- `docs/deepseek_m2_delta_v_prime_review.md`
