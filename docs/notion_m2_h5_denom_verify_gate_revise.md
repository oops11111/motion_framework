# M2-H5-DENOM-VERIFY 闸门补洞决断（可 LOCK → 已 LOCK）

> DECISION_AT=2026-10-26 16:09:00 Asia/Taipei（同 UTC+8 = Australia/Perth）  
> 依据：`docs/deepseek_m2_h5_denom_verify_review.md`（Harness 空回代拟；判可 LOCK；验证采数=是；落地=否；calib=**C1=median(v_denom/v_hat)**；路径=**PATHB-A**；A′ 默认不启用）；`docs/m2_h5_denom_verify_gate_DRAFT.md`；`docs/notion_m2_h5_denom_revise_next.md` / pack / DENOM LOCKED `d891df9f…33c7`  
> 七 hash **不改、不重跑翻盘**：  
> - DENOM `d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7`  
> - TRUTH `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69`  
> - PATH-A′ `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362`  
> - PATH-B′ `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`  
> - CALIB `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`  
> - ABS `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`  
> - Δv′ `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`  
> composition.h **不改** `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`  
> LOCKED：`docs/m2_h5_denom_verify_gate_LOCKED.md`  GATE_BODY_SHA256=`fd00a06058b239abfe03a52175463a52741718318ce99416a4ce762cf037889b` @ 2026-10-26 16:09:00 Asia/Taipei  
> **LOCK ≠ 授权采数**（直至 APPROVED_* 显式字段齐）；**落地=否**；APPROVED_* 空；PERF-CI 仍 HOLD；不开 M3；禁立刻 Δv″；禁静默 0.7511。  
> **声明**：`H5-DENOM-PASS`（doc-only）≠ 已落地采用；本票 LOCK 只定义验证闸门+C1，不单独授权采数、不落地。

## 逐条决断

| ID | 项 | 决断 |
|---|---|---|
| V1 | 验证采数 | **是**（t060 fwd+rev + H5 比；≥3 独立进程；单机 DESKTOP-K93N3P8；≠PERF-CI） |
| V2 | 落地 | **否** |
| V3 | calib | **C1 = median(v_denom/v_hat)**（Phase-Calib→冻结→Phase-Score；禁静默 0.7511） |
| V4 | 路径 | **PATHB-A**（V2-clamp；P-VS=OFF）；A′ 默认不启用 |
| V5 | KEEP v_denom≡v_pre | **KEEP**；旧 2F 不作门控分母 |
| V6 | 门槛 | **KEEP** med_r∈[0.8,1.2]∧无 r_i≥2；t060≥17/20；**禁抬 thr / 降 17/20** |
| V7 | 对照 | **KEEP** basePass ∧ IVel0_t060_both_FAIL（前序有则 KEEP） |
| V8 | 七 hash | **不翻盘**（全文写入 LOCK） |
| V9 | Δv″ / PERF-CI / M3 | 禁立刻 Δv″；PERF-CI HOLD；不开 M3；不解冻 Bv；不重开 S1/R-3/R-2 |
| V10 | 标签 / 产物 | `H5-DENOM-VERIFY-*`；前缀 `m2_h5_denom_verify_*`；PASS≠落地采用≠Δv-PASS |
| V11 | 虚 PASS 否决表 | **进 LOCK 正文** |
| V12 | DeepSeek 可 LOCK | **采纳**；最小修订点并入 LOCK |

## 采纳点（写死并入 LOCK）

1. **验证采数=是**：格表 t060 fwd+rev + H5 比；≥3 独立进程；单机 `DESKTOP-K93N3P8`；≠PERF-CI。采数时机仅 LOCK+APPROVED 且 APPROVED 写死「验证采数=是」之后。  
2. **落地=否**：不改分母落地启用；将来落地须另票+APPROVED 另字段「落地=是」。  
3. **calib=C1**：`median(v_denom/v_hat)`；Phase-Calib→冻结→Phase-Score；禁静默 0.7511；禁用 `v_screen` 冒充。  
4. **路径=PATHB-A**（V2-clamp；P-VS=OFF）；A′ 默认不启用；禁执行中偷开。  
5. **KEEP** `v_denom≡v_pre`；门槛 KEEP；禁抬 thr / 降 17/20；对照 basePass+IVel0 KEEP。  
6. **七 hash 不翻盘**（全文）；禁影子 IV；composition.h 锁。  
7. 标签 `H5-DENOM-VERIFY-*`；产物前缀 `m2_h5_denom_verify_*`；PASS ≠ 已落地采用 ≠ Δv-PASS ≠ 翻盘前序 ≠ 可立刻 Δv″。  
8. 成功≠采用；≠PERF-CI；不开 M3；不解冻 Bv；不重开 S1/R-3/R-2；禁立刻 Δv″。  
9. APPROVED 须声明：验证采数=是；落地=否；calib=C1 流程或冻结常量；路径=PATHB-A；PASS 判据（比+smoke+对照）；虚 PASS 否决表。

## 否决虚 PASS（须进 LOCK）

| 行为 | 处置 |
|---|---|
| 未 LOCK+APPROVED 即采数或分母落地 | INVALID |
| 静默沿用 calib=0.7511 / exact 0.7510829456 宣称新分母比 PASS | INVALID |
| 标定仍用 `v_screen` 却称 C1 / `v_denom` | INVALID |
| 比仍用旧 2F 分母却宣称新契约 PASS | INVALID |
| 抬 thr / 降 17/20 | INVALID |
| 重跑/宣称通过七 hash；标 `Δv-PASS` / 前序翻盘 / 重写 `H5-DENOM-PASS` 为已落地 | INVALID |
| 立刻 Δv″；开 M3；解冻 Bv；改 composition.h；影子 IV | INVALID |
| 产物作 PERF-CI 合格证据；开第二台冒充矩阵 | INVALID |
| 默认可落地；把 VERIFY-PASS 写成已落地采用（无落地=是） | INVALID |
| 把 doc-only `H5-DENOM-PASS` / 本 LOCK 当已授权采数 | INVALID |
| 执行中偷开 A′ / 改 α / 改 N·order·ε；计分窗改 calib | INVALID |

## LOCK 粘贴句（对齐 DeepSeek §7 / 协调拍板）

> 本票=验证采数+另批 calib（C1）；**KEEP v_denom≡v_pre**；路径=**PATHB-A**（V2-clamp；P-VS=OFF；A′ 默认不启用）；calib=**C1=median(v_denom/v_hat)**（Phase-Calib→冻结→Phase-Score；禁静默 0.7511）；验证采数=是（t060 fwd+rev+H5 比；≥3 进程；单机 DESKTOP-K93N3P8；≠PERF-CI）；**落地=否**；门槛 KEEP（禁抬 thr/降 17/20）；对照 basePass+IVel0 KEEP；七 hash 不翻盘；虚 PASS 否决表进正文；`H5-DENOM-VERIFY-*`；产物 `m2_h5_denom_verify_*`；成功≠落地采用≠Δv-PASS；禁立刻 Δv″；**可 LOCK**；LOCK≠授权采数（直至 APPROVED 显式字段齐）；`H5-DENOM-PASS`≠已落地采用。

## 与 DeepSeek 分歧

无实质分歧。DeepSeek 判「可 LOCK」、验证采数范围、落地=否、C1、PATHB-A、七 hash、虚 PASS、标签与产物前缀——均已并入 LOCK。协调补强（相对 DeepSeek 最小修订点）：

1. APPROVED 强制字段表显式升为同级必填：**验证采数=是** / **落地=否** / **calib=C1 或冻结常量** / **路径=PATHB-A**（DeepSeek §3 已含；本票把路径与对照 KEEP 一并钉进 APPROVED 表）。  
2. PASS 判据独立成 G1-V6（比+smoke+对照+calib 冻结+路径+七 hash）；DeepSeek 散落 §1/§3/§4，LOCK 集中核对。  
3. 对照 basePass∧IVel0 前序有则 KEEP 写入继承契约表（DeepSeek §1 已写；LOCK G1 继承表再钉）。  
4. G4 ONELINE 写入本 LOCK 时区戳 `2026-10-26 16:09:00 Asia/Taipei`。

## 结论

**可 LOCK。** 已 LOCK → `docs/m2_h5_denom_verify_gate_LOCKED.md`（GATE_BODY_SHA256=`fd00a06058b239abfe03a52175463a52741718318ce99416a4ce762cf037889b` @ 2026-10-26 16:09:00 Asia/Taipei）。APPROVED_* 空；LOCK ≠ 授权采数；落地=否；本票不执行采数、不落地、不开 Δv″。

— notion ai（M2-H5-DENOM-VERIFY 可 LOCK 修订）
