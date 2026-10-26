# M2-WGC-SEED-PATH-B′ 闸门补洞决断（可 LOCK）

> DECISION_AT=2026-10-26 13:49:20 Asia/Taipei  
> 依据：`docs/deepseek_m2_wgc_seed_path_b_prime_design.md`（Harness 空回代拟；判可 LOCK）；`docs/m2_wgc_seed_path_b_prime_gate_DRAFT.md`；`docs/notion_m2_wgc_seed_calib_next.md` / `docs/notion_m2_wgc_seed_calib_pack.md`  
> 前序 FAIL gate **不改、不重跑翻盘**：  
> - CALIB `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`  
> - ABS `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`  
> - Δv′ `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`  
> composition.h **不改** `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`  
> LOCKED：`docs/m2_wgc_seed_path_b_prime_gate_LOCKED.md`  GATE_BODY_SHA256=`a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43` @ 2026-10-26 13:49:20 Asia/Taipei  
> **不授权采数**；须 APPROVED_* 非空后方可派 workbuddy；PERF-CI 仍 HOLD；不开 M3。

## 逐条决断

| ID | 项 | 决断 |
|---|---|---|
| P1 | 默认主路径 A | **采纳** `PATHB-A = V2-clamp`：`t_int = min(t_comp, t_last_WGC)`；**禁**过后外推；禁打断后帧 / 异时基 |
| P2 | KEEP 常量 | **KEEP** N=5 / order=2 / calib exact=`0.7510829456`（显示 0.7511）/ ε=5.0 / **P-VS=OFF** |
| P3 | A′ | **挂账次选**：A + P-VS α_v=0.30；本票 LOCK **不**默认同跑第二臂；启用须另 APPROVED / 另票 |
| P4 | 否决 | **否**默认改 N/order；**否**V4 单侧作默认；**否**再标定；**否**立刻 Δv″ |
| P5 | H5 比回归 | **必要**：`med_r∈[0.8,1.2]` 且无 `r_i≥2`；比出窗不得以 smoke 豁免 |
| P6 | smoke | t060 fwd+rev；二次 w=5；`thr=max(20,0.05·|v_ref|)`；≥**17/20**；可判 P1/P2；**禁**抬 thr / 放宽 17/20 |
| P7 | 对照 / 会话 | basePass + IVel0_t060_both_FAIL；≥3 会话；单机 DESKTOP-K93N3P8 / 60 Hz；PASS **只评默认 A**；回退 B=IV null+0 |
| P8 | 标签 | 只用 `SEED-PATHB-*`；**禁**标 `Δv-PASS` / `SEED-ABS-PASS` / `SEED-CALIB-PASS` |
| P9 | 三禁翻盘 | **禁**重跑/改写/宣称通过 `09f6414b…793d` / `831818e4…80de` / `06467b82…805c` |
| P10 | 结构风险 | **写死进 LOCK**：KEEP calib + H5@2F 下 \|ivel−v_pre\|≈50 可能使 smoke 仍 FAIL——**FAIL 合法，禁抬 thr / 降 17/20 / 偷换估窗** |
| P11 | SEED-TRUTH-CONFLICT | 若本票比过 ∧ smoke 仍双灭 → 挂账（名可微调）；**不**预授权改 H5 分母 |
| P12 | Δv″ / A′ | Δv″ **继续挂账**（比+smoke 齐后再开）；A′ 挂账次选 |
| P13 | DeepSeek 可 LOCK | **采纳**；LOCK ≠ 授权采数 |

## 采纳点（写死并入 LOCK）

1. **A = PATHB-A = V2-clamp**：`t_int = min(t_compositor_interrupt_QPC, t_last_WGC)`；禁过后外推；禁打断后帧。  
2. **KEEP**：N=5 / order=2 / calib exact=`0.7510829456`（显示 0.7511）/ ε=5.0 / P-VS=OFF；PASS 只评 A；B=IV null+0。  
3. **A′**：V2-clamp + P-VS α_v=0.30；**挂账次选**；本票不默认同跑。  
4. **验收**：H5 比回归必要 + t060 fwd+rev smoke ≥17/20 + 可判 + basePass + IVel0_t060_both_FAIL；≥3 会话；DESKTOP-K93N3P8 / 60 Hz。  
5. **标签 / 三禁翻盘**：`SEED-PATHB-*` only；禁翻盘三 hash；禁影子 IV；composition.h 锁。  
6. **结构风险**：KEEP calib + H5@2F → \|ivel−v_pre\|≈50 可能 smoke 仍 FAIL；FAIL 合法；禁抬 thr / 降 17/20 / 偷换估窗。  
7. **再挂账**：比过 ∧ smoke 双灭 → `SEED-TRUTH-CONFLICT`（名可微调；不预授权改 H5 分母）；Δv″ 继续挂账；A′ 挂账。  
8. 成功≠采用≠Δv-PASS≠SEED-ABS-PASS≠SEED-CALIB-PASS≠PERF-CI；不开 M3；不解冻 Bv；不重开 S1/R-3/R-2。

## 否决虚 PASS（须进 LOCK）

| 行为 | 处置 |
|---|---|
| 抬 thr / 放宽 ≥17/20 | 否决 / INVALID |
| 默认改 N / order；再标定 / 废止 exact | 否决 / INVALID |
| V4 单侧作默认；只修一侧却放宽另一侧门槛 | 否决 / INVALID |
| 偷换估窗：打断后帧、异时基、外推过 `t_last_WGC` | INVALID |
| 执行中改 KEEP 常量 / 把 A 偷换成 A′ | INVALID |
| 影子 IV（mf_spring_at / spring.h → IV） | INVALID |
| 改 composition.h | INVALID |
| 重跑/宣称通过三 FAIL gate；标 `Δv-PASS`/`SEED-ABS-PASS`/`SEED-CALIB-PASS` | INVALID |
| 无 basePass / 无 IVel0 t060 双 FAIL 却宣称 PASS | INVALID |
| 产物写入 PERF-CI；开 M3；解冻 Bv；立刻 Δv″ | INVALID |
| offline 预测当结题；躲 t060 | INVALID |

## LOCK 粘贴句（对齐 DeepSeek §6 / 一句结论）

> 默认 **A=PATHB-A=V2-clamp**：`t_int=min(t_comp,t_last_WGC)` 禁过后外推；KEEP N=5/order=2/calib exact=`0.7510829456`（显示 0.7511）/ε=5.0/P-VS=OFF；A′=A+P-VS α_v=0.30 **挂账次选**（本票不默认同跑）；否决默认改 N/order、V4 单侧、再标定、立刻 Δv″；验收 KEEP H5 比窗 + t060 smoke ≥17/20 + base/IVel0；结构风险下 FAIL 合法禁抬 thr；比过∧smoke 双灭 → 挂 `SEED-TRUTH-CONFLICT`；结题只用 `SEED-PATHB-*`；三禁翻盘；禁影子 IV；**可 LOCK**；LOCK≠授权采数。

## 与 DeepSeek 分歧

无实质分歧。协调写死：本票 LOCK **只锁 A**、A′ 作挂账次选（DeepSeek §6.1 推荐同）；结构风险与 `SEED-TRUTH-CONFLICT` 挂账条件并入正文；否决默认改 N/order / V4 / 再标定 / 立刻 Δv″——与同伴稿一致。

## 结论

**可 LOCK。** 已 LOCK → `docs/m2_wgc_seed_path_b_prime_gate_LOCKED.md`（GATE_BODY_SHA256=`a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43` @ 2026-10-26 13:49:20 Asia/Taipei）。APPROVED_* 空；不授权采数。

— notion ai（M2-WGC-SEED-PATH-B′ 可 LOCK 修订）
