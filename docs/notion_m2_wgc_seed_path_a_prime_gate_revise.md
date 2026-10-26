# M2-WGC-SEED-PATH-A′ 闸门补洞决断（可 LOCK → 已 LOCK）

> DECISION_AT=2026-10-26 14:38:15 Asia/Taipei  
> 依据：`docs/deepseek_m2_wgc_seed_path_a_prime_review.md`（Harness 空回代拟；判可 LOCK；无硬缺口）；`docs/m2_wgc_seed_path_a_prime_gate_DRAFT.md`；`docs/notion_m2_wgc_seed_path_b_prime_next.md` / `docs/notion_m2_wgc_seed_path_b_prime_pack.md`  
> 前序 FAIL gate **不改、不重跑翻盘**：  
> - PATH-B′ `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`  
> - CALIB `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`  
> - ABS `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`  
> - Δv′ `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`  
> composition.h **不改** `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`  
> LOCKED：`docs/m2_wgc_seed_path_a_prime_gate_LOCKED.md`  GATE_BODY_SHA256=`f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362` @ 2026-10-26 14:38:15 Asia/Taipei  
> **不授权采数**；须 APPROVED_* 非空后方可派 workbuddy；PERF-CI 仍 HOLD；不开 M3；禁立刻 Δv″。

## 逐条决断

| ID | 项 | 决断 |
|---|---|---|
| P1 | 默认主路径 A′ | **采纳** `PATHB-A′ = V2-clamp + V3-VS`：P-VS=**ON**；α_v=**0.30** 写死；首样 `v_s=v̂`；限幅不设；IV=`(float)(calib·v_s)`；ε 在 VS **前**判（`|v̂|<ε` → 回退 B） |
| P2 | KEEP 常量 | **KEEP** N=5 / order=2 / calib exact=`0.7510829456`（显示 0.7511）/ ε=5.0 / `t_int=min(t_comp,t_last_WGC)` 禁外推 / thr=`max(20,0.05·|v_ref|)` / ≥17/20 / 二次 w=5 / H5@2F 比回归 |
| P3 | 回退 B / PASS 臂 | 回退 B=IV null+0；票 PASS **只评默认 A′**；禁默认同跑偷换 PATH-B′ |
| P4 | 否决 | **否**关 P-VS/偷换回 A；**否**改 α_v；**否**改 N/order；**否**V4 单侧；**否**再标定；**否**立刻 Δv″；**否**预授权改 H5 分母 |
| P5 | H5 比回归 | **必要**：`med_r∈[0.8,1.2]` 且无 `r_i≥2`；比出窗不得以 smoke 豁免 |
| P6 | smoke | t060 fwd+rev；二次 w=5；`thr=max(20,0.05·|v_ref|)`；≥**17/20**；可判 P1/P2；**禁**抬 thr / 放宽 17/20 |
| P7 | 对照 / 会话 | basePass + IVel0_t060_both_FAIL；≥3 会话；单机 DESKTOP-K93N3P8 / 60 Hz |
| P8 | 标签 | 主用 `SEED-PATHA-PRIME-*`；若留别名 `SEED-PATHB-APRIME-*` 须声明**禁止**写作 `SEED-PATHB-PASS`；禁标 `Δv-PASS` / `SEED-ABS-PASS` / `SEED-CALIB-PASS` |
| P9 | 四禁翻盘 | **禁**重跑/改写/宣称通过 `a1161c77…3a43` / `09f6414b…793d` / `831818e4…80de` / `06467b82…805c` |
| P10 | 结构风险 | **写死进 LOCK**：KEEP calib + H5@2F 下 \|ivel−v_pre\|≈50+ 可能使 smoke 仍 FAIL——**FAIL 合法，禁抬 thr / 降 17/20 / 偷换估窗 / 改 H5 分母** |
| P11 | G2-LOCK-2b 延续 | 比过 ∧ smoke 双灭 → **FAIL** + **保持** `SEED-TRUTH-CONFLICT`；不预授权改 H5；禁抬 thr/降17/20/偷换估窗 |
| P12 | 软提醒 | PATH-B′ med_r 贴下沿 → α 可能进一步欠 seed；出窗则 FAIL，**不得**改 calib/分母救票（非挡 LOCK） |
| P13 | prereg 显式 | α_v=0.30 / `v_s` 定义 / `eps_before_vs=true` / `pathb_variant=PATHB-A′` |
| P14 | Δv″ | **继续挂账**；比+smoke 齐后再开；**不立刻开** |
| P15 | DeepSeek 可 LOCK | **采纳**；LOCK ≠ 授权采数 |

## 采纳点（写死并入 LOCK）

1. **A′ = PATHB-A′ = V2-clamp + V3-VS**：P-VS=ON；α_v=0.30；首样 `v_s=v̂`；限幅不设；IV=`(float)(calib·v_s)`；ε 在 VS **前**判。  
2. **KEEP**：N=5 / order=2 / calib exact=`0.7510829456`（显示 0.7511）/ ε=5.0 / `t_int=min(t_comp,t_last_WGC)` 禁外推 / thr / ≥17/20 / w=5 / H5@2F；PASS 只评 A′；B=IV null+0。  
3. **验收**：H5 比回归必要 + t060 fwd+rev smoke ≥17/20 + 可判 + basePass + IVel0_t060_both_FAIL；≥3 会话；DESKTOP-K93N3P8 / 60 Hz。  
4. **标签 / 四禁翻盘**：主用 `SEED-PATHA-PRIME-*`；禁写作 `SEED-PATHB-PASS`；禁翻盘四 hash；禁影子 IV；composition.h 锁。  
5. **结构风险 + 软提醒**：KEEP calib + H5@2F → smoke 仍 FAIL 合法；PATH-B′ med_r 贴下沿 → A′ 出窗不得改 calib/分母救票。  
6. **再挂账**：比过 ∧ smoke 双灭 → FAIL + 保持 `SEED-TRUTH-CONFLICT`；不预授权改 H5；Δv″ 继续挂账。  
7. **prereg 显式**：`pathb_variant=PATHB-A′` / `α_v=0.30` / `v_s` 定义 / `eps_before_vs=true`。  
8. 成功≠采用≠Δv-PASS≠SEED-ABS-PASS≠SEED-CALIB-PASS≠SEED-PATHB-PASS≠PERF-CI；不开 M3；不解冻 Bv；不重开 S1/R-3/R-2。

## 否决虚 PASS（须进 LOCK）

| 行为 | 处置 |
|---|---|
| 抬 thr / 放宽 ≥17/20 | 否决 / INVALID |
| 关 P-VS / 偷换回 PATHB-A；改 α_v≠0.30 | INVALID |
| 改 N / order；再标定 / 废止 exact | 否决 / INVALID |
| V4 单侧作默认；只修一侧却放宽另一侧门槛 | 否决 / INVALID |
| 偷换估窗：打断后帧、异时基、外推过 `t_last_WGC`；ε 改判到 VS 后 | INVALID |
| 影子 IV（mf_spring_at / spring.h → IV） | INVALID |
| 改 composition.h | INVALID |
| 重跑/宣称通过四 FAIL gate；标 `Δv-PASS`/`SEED-ABS-PASS`/`SEED-CALIB-PASS`/`SEED-PATHB-PASS` | INVALID |
| 无 basePass / 无 IVel0 t060 双 FAIL 却宣称 PASS | INVALID |
| 产物写入 PERF-CI；开 M3；解冻 Bv；立刻 Δv″；默认同跑偷换 PATH-B′ | INVALID |
| offline 预测当结题；躲 t060；本票预授权改 H5 分母 | INVALID |

## LOCK 粘贴句（对齐 DeepSeek §6 / 一句结论）

> 默认 **A′=PATHB-A′=V2-clamp+V3-VS**：P-VS=ON；α_v=**0.30** 写死；首样 `v_s=v̂`；限幅不设；IV=`(float)(calib·v_s)`；ε 在 VS **前**判；KEEP N=5/order=2/calib exact=`0.7510829456`（显示 0.7511）/ε=5.0/`t_int=min(t_comp,t_last_WGC)` 禁外推/thr/≥17/20/w=5/H5@2F；PASS 只评 A′；B=IV null+0；结题主用 `SEED-PATHA-PRIME-*`（禁写作 `SEED-PATHB-PASS`）；prereg 显式 α_v/`v_s`/`eps_before_vs=true`/`pathb_variant=PATHB-A′`；G2-LOCK-2b 延续：比过∧smoke 双灭→FAIL+保持 SEED-TRUTH-CONFLICT；结构风险 FAIL 合法；软提醒 med_r 贴下沿出窗不得改 calib/分母；四禁翻盘；禁影子 IV；Δv″ 继续挂账；**可 LOCK**；LOCK≠授权采数。

## 与 DeepSeek 分歧

无实质分歧。DeepSeek 判「可 LOCK」、Harness 空回代拟、无硬缺口；三条最小修订点（标签主用 `SEED-PATHA-PRIME-*` + 禁写作 `SEED-PATHB-PASS`；prereg 显式 α_v/`v_s`/`eps_before_vs`/`pathb_variant`；文首/正文软提醒 med_r 贴下沿）均已并入 LOCK 正文。协调写死：本票 LOCK 只锁 A′ 主路径；KEEP 全表；G2-LOCK-2b 延续；Δv″ 继续挂账——与同伴稿一致。

## 结论

**可 LOCK。** 已 LOCK → `docs/m2_wgc_seed_path_a_prime_gate_LOCKED.md`（GATE_BODY_SHA256=`f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362` @ 2026-10-26 14:38:15 Asia/Taipei）。APPROVED_* 空；不授权采数。

— notion ai（M2-WGC-SEED-PATH-A′ 可 LOCK 修订）
