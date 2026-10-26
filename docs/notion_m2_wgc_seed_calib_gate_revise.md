# M2-WGC-SEED-CALIB 闸门补洞决断（可 LOCK）

> DECISION_AT=2026-10-26 12:03:33 Asia/Taipei  
> 依据：`docs/deepseek_m2_wgc_seed_calib_review.md`（判可 LOCK）；`docs/m2_wgc_seed_calib_gate_DRAFT.md`；`docs/notion_m2_wgc_seed_abs_next.md` / `docs/notion_m2_wgc_seed_abs_pack.md`  
> 前序 FAIL gate `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`（SEED-ABS）与 `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`（Δv′）**不改、不重跑翻盘**；composition.h **不改** `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`  
> LOCKED：`docs/m2_wgc_seed_calib_gate_LOCKED.md`  GATE_BODY_SHA256=`09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d` @ 2026-10-26 12:03:33 Asia/Taipei  
> **不授权采数**；须 APPROVED_* 非空后方可派 workbuddy；PERF-CI 仍 HOLD；不开 M3。

## 逐条决断

| ID | 项 | 决断 |
|---|---|---|
| C1 | calib 常量 | **采纳** 显示 `0.7511`；exact=`0.7510829456`（=median(v_screen/ivel_set)；ABS pathA n=120）；废止 1.0；prereg/执行比较与写入用 **exact**；闸门显示四位 |
| C2 | 路径 KEEP | **KEEP** A=P-LS+TS / N=5 / order=2 / ε=5.0 / P-VS=关；票 PASS **只评 A**；回退 B=IV null+0 |
| C3 | H5 比回归 | **必要**：`med_r∈[0.8,1.2]` 且无 `r_i≥2`；比出窗不得以 smoke 豁免 |
| C4 | smoke | t060 fwd+rev；二次 w=5；`thr=max(20,0.05·|v_ref|)`；≥**17/20**；可判 P1/P2；**禁**抬 thr / 放宽 17/20 |
| C5 | 对照 | basePass + IVel0_t060_both_FAIL **KEEP** |
| C6 | 标签 | 只用 `SEED-CALIB-*`；**禁**标 `Δv-PASS` / `SEED-ABS-PASS` |
| C7 | 双禁翻盘 | **禁**重跑/改写/宣称通过 `831818e4…80de` 与 `06467b82…805c` |
| C8 | PATH-B′ 注记 | 若本票比回归过但 t060 rev（或单侧）smoke 仍 FAIL → **挂 PATH-B′**；本票**不**放宽 thr/17/20/估窗；不得因此标 PASS（票结局为 SEED-CALIB-FAIL） |
| C9 | Δv″ | **继续挂账**（须本票 CLOSED 且可比回归/smoke 后再另开新闸门） |
| C10 | 虚 PASS | DeepSeek §4 反例全保留：影子 IV；改 composition.h；执行中改 calib/N/order/ε/估窗；抬 thr；offline 当结题；只跑 fwd；P-VS 偷开；分会话拼凑；错误标定式 1.33 等 → INVALID/否决 |
| C11 | DeepSeek 可 LOCK | **采纳**；LOCK ≠ 授权采数 |

## 采纳点（写死并入 LOCK）

1. **calib**：显示 `0.7511`；exact=`0.7510829456`；来源=ABS pathA n=120；公式=`median(v_screen/ivel_set)`；废止 1.0。  
2. **KEEP**：P-LS+TS / N=5 / order=2 / ε=5.0 / P-VS=关；PASS 只评 A；B=IV null+0。  
3. **验收**：H5 比回归必要 + t060 fwd+rev smoke ≥17/20 + 可判 P1/P2 + basePass + IVel0_t060_both_FAIL；≥3 会话；单机 DESKTOP-K93N3P8 / 60 Hz。  
4. **标签 / 禁翻盘**：`SEED-CALIB-*` only；禁翻盘两 hash；禁影子 IV；composition.h 锁。  
5. **PATH-B′**：比过但 rev（或单侧）smoke 仍 FAIL → 挂 PATH-B′，本票不放宽门槛、不得标 PASS。  
6. **Δv″** 挂账；成功≠采用≠Δv-PASS≠PERF-CI；不开 M3；不解冻 Bv；不重开 S1/R-3/R-2。

## 否决虚 PASS（须进 LOCK）

| 行为 | 处置 |
|---|---|
| 抬 thr / 放宽 ≥17/20 | 否决 / INVALID |
| 改 N / order / ε / 估窗（打断后帧、异时基） | INVALID |
| 执行中改 calib | INVALID |
| 影子 IV（mf_spring_at / spring.h → IV） | INVALID |
| 改 composition.h | INVALID |
| 重跑/宣称通过 `831818e4…80de` 或 `06467b82…805c`；标 `Δv-PASS`/`SEED-ABS-PASS` | INVALID |
| 无 basePass / 无 IVel0 t060 双 FAIL 却宣称 PASS | INVALID |
| 产物写入 PERF-CI；开 M3；解冻 Bv | INVALID |
| offline 预测当结题；只跑 fwd 躲 rev | INVALID |

## LOCK 粘贴句（对齐 DeepSeek §5 / 一句结论）

> 独立复核 n=120，median(v_screen/ivel_set)=0.7510829456 → LOCK 写 calib 显示 `0.7511`（exact=`0.7510829456`；废止 1.0）；KEEP P-LS+TS/N=5/order=2/ε=5.0/P-VS=关；比窗可期回 [0.8,1.2]；smoke fwd 可期改善、**rev 可能仍不够 → PATH-B′ 挂账触发**；结题只用 `SEED-CALIB-*`；禁翻盘 `831818e4…80de` / `06467b82…805c`；禁影子 IV；禁抬 thr；**可 LOCK**；LOCK≠授权采数。

## 与 DeepSeek 分歧

无实质分歧。协调写死 exact=`0.7510829456` 作 prereg/执行浮点；DeepSeek LOCK 显示四位 `0.7511`——二者并入（显示四位 + exact 注记），无冲突。

## 结论

**可 LOCK。**

— notion ai（M2-WGC-SEED-CALIB 可 LOCK 修订）
