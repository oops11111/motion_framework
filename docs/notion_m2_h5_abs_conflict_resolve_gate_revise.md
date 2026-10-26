# M2-H5-ABS-CONFLICT-RESOLVE 闸门补洞决断（可 LOCK → 已 LOCK）

> DECISION_AT=2026-10-26 18:30:00 Asia/Taipei（同 UTC+8 = Australia/Perth）  
> 依据：`docs/deepseek_m2_h5_abs_conflict_resolve_review.md`（Harness 空回代拟；判可 LOCK；推荐主决议 **R2=挂账确认**；mode=**doc-only**；验证采数=**否**）；`docs/m2_h5_abs_conflict_resolve_gate_DRAFT.md`；`docs/notion_m2_h5_denom_verify_next.md` / pack / VERIFY LOCKED `fd00a060…889b`  
> 八 hash **不改、不重跑翻盘**：  
> - PATH-A′ `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362`  
> - PATH-B′ `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`  
> - SEED-CALIB `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`  
> - SEED-ABS `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`  
> - Δv′ `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`  
> - TRUTH `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69`  
> - DENOM `d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7`  
> - VERIFY `fd00a06058b239abfe03a52175463a52741718318ce99416a4ce762cf037889b`  
> composition.h **不改** `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`  
> LOCKED：`docs/m2_h5_abs_conflict_resolve_gate_LOCKED.md`  GATE_BODY_SHA256=`e96bf6238ace42ce2edcfe4191ef5ec45fa53eefca94fcf777f2b52c17425ade` @ 2026-10-26 18:30:00 Asia/Taipei  
> **LOCK ≠ 授权采数**；mode=**doc-only**；验证采数=**否**；APPROVED_* 空；PERF-CI 仍 HOLD；不开 M3；禁立刻 Δv″；禁抬 thr；禁翻 C1 exact。  
> **声明**：`H5-DENOM-VERIFY-FAIL` 归档保留、不改写为 PASS；本票 LOCK 只决议「比过 smoke 灭」处置菜单，不单独授权采数、不开 Δv″。

## 逐条决断

| ID | 项 | 决断 |
|---|---|---|
| A1 | 议题 | **比过、smoke 灭**（VERIFY FAIL；分母同源 + C1 后相对比可过、绝对 smoke 系统性灭） |
| A2 | 主决议 menu | **R2=挂账确认**（推荐；对齐 Notion next 主推 C） |
| A3 | 可选同批 | **R1 STOP** 可与 R2 同批声明「STOP+挂账」；**R3** 仅占位（须另闸门+新假设；本票不开采数） |
| A4 | R4 否决 | 抬 thr / 降 17/20；立刻 Δv″；重跑 VERIFY / 翻八 hash / 把 FAIL 当 PASS；默认同跑 A′；翻 C1；默认可落地 / 改 composition.h / 影子 IV / 开 M3 / 解冻 Bv — **一律否决** |
| A5 | mode | **doc-only** |
| A6 | 验证采数 | **否** |
| A7 | C1 exact | **KEEP 不翻** `1.3358225694150683`；禁静默 0.7511 |
| A8 | thr / 17/20 | **KEEP**；禁抬 / 禁降 |
| A9 | 八 hash | **不翻盘**（七前序 + VERIFY；全文写入 LOCK） |
| A10 | Δv″ / PERF-CI / M3 | 禁立刻 Δv″；PERF-CI HOLD；不开 M3；不解冻 Bv；不重开 S1/R-3/R-2 |
| A11 | 标签 / 产物 | `H5-ABS-RESOLVE-*`；前缀 `m2_h5_abs_conflict_resolve_*`；PASS≠VERIFY-PASS 改写≠Δv-PASS |
| A12 | 虚 PASS 否决表 | **进 LOCK 正文** |
| A13 | DeepSeek 可 LOCK | **采纳**；最小修订点并入 LOCK；协调拍板写死 R2 |

## 采纳点（写死并入 LOCK）

1. **议题写死**：KEEP `v_denom≡v_pre` + C1 exact=`1.3358225694150683` 后，H5 比三项齐、smoke ≪17/20（pass_sessions=0/3）；比–绝对冲突未解；非「只差再跑一遍」。  
2. **主决议=R2 挂账确认**：显式挂账「比–绝对冲突」旁路；保留 `H5-DENOM-VERIFY-FAIL`；主线不因本 FAIL 强行再采 / 再 VERIFY；可与 R1 同批「STOP+挂账」；R3 仅占位。  
3. **mode=doc-only**；**验证采数=否**；落地=否；LOCK ≠ 授权采数。  
4. **禁抬 thr / 降 17/20**；**禁立刻 Δv″**；**禁翻 C1 exact** / 静默 0.7511。  
5. **八 hash 不翻盘**（全文）；禁影子 IV；composition.h 锁 `a55d4952…e418`。  
6. 标签 `H5-ABS-RESOLVE-*`；产物前缀 `m2_h5_abs_conflict_resolve_*`；PASS = doc-only 菜单完备且主决议 ∈ {R1,R2,R3}；**≠** 把 VERIFY-FAIL 改 PASS；**≠** Δv-PASS；**≠** 默认可落地。  
7. 成功≠采用；≠PERF-CI；不开 M3；不解冻 Bv；不重开 S1/R-3/R-2。  
8. APPROVED_* 空；即便将来 APPROVED，默认仍 doc-only；采数须另字段+新假设。

## 否决虚 PASS（须进 LOCK）

| 行为 | 处置 |
|---|---|
| 抬 thr / 降 17/20 救 VERIFY | INVALID |
| 立刻 Δv″ | INVALID |
| 重跑/改写八 hash；把 VERIFY-FAIL 标 PASS | INVALID |
| 翻 C1 exact / 静默 0.7511 | INVALID |
| 未 LOCK+APPROVED 采数；把 DRAFT/本 LOCK 当授权采数 | INVALID |
| 主决议落在 R4* | INVALID |
| 改 composition.h；影子 IV；开 M3；解冻 Bv；重开 S1/R-3/R-2 | INVALID |
| 产物作 PERF-CI 合格证据 | INVALID |
| 默认可落地 / 默认同跑 A′ / 另批 calib 式追 smoke | INVALID |

## LOCK 粘贴句（对齐 DeepSeek §6 / 协调拍板）

> 本票=协调决议（**doc-only**）；议题=「比过、smoke 灭」处置；主决议=**R2 挂账确认**（可选同批 R1 STOP；R3 仅占位）；验证采数=**否**；KEEP v_denom≡v_pre；C1 exact=`1.3358225694150683` **不翻**；八 hash 不翻盘（七前序 + VERIFY `fd00a060…889b`）；禁抬 thr / 降 17/20；禁立刻 Δv″；禁翻 C1；标签 `H5-ABS-RESOLVE-*`；产物 `m2_h5_abs_conflict_resolve_*`；成功≠VERIFY-PASS 改写≠Δv-PASS；PERF-CI HOLD；不开 M3；不解冻 Bv；不改 composition.h；禁影子 IV；**可 LOCK**；LOCK≠授权采数；APPROVED_* 空；`H5-DENOM-VERIFY-FAIL` 归档不改写。

## 与 DeepSeek 分歧

无实质分歧。DeepSeek 判「可 LOCK」、推荐 R2、mode=doc-only、采数=否、八 hash、禁抬 thr、禁立刻 Δv″、禁翻 C1、标签与产物前缀——均已并入 LOCK。协调补强（相对 DeepSeek 最小修订点）：

1. 主决议写死为 **R2**（DeepSeek 已推荐；本票拍板钉死，非「或 R1/R3」开放主选）。  
2. 明确「可选同批 R1 STOP」与「R3 仅占位」分层（DeepSeek §1 一句话已含；LOCK G1 菜单表再钉）。  
3. APPROVED 强制字段表：mode=doc-only / 主决议=R2 / 验证采数=否 / 禁抬 thr / 八 hash 不翻盘 / 禁立刻 Δv″ / 禁翻 C1 / PERF-CI=HOLD（DeepSeek §2/§6 已含；LOCK 升为同级必填表）。  
4. G4 ONELINE 写入本 LOCK 时区戳 `2026-10-26 18:30:00 Asia/Taipei`。

## 结论

**可 LOCK。** 已 LOCK → `docs/m2_h5_abs_conflict_resolve_gate_LOCKED.md`（GATE_BODY_SHA256=`e96bf6238ace42ce2edcfe4191ef5ec45fa53eefca94fcf777f2b52c17425ade` @ 2026-10-26 18:30:00 Asia/Taipei）。APPROVED_* 空；LOCK ≠ 授权采数；mode=doc-only；验证采数=否；主决议 R2；本票不执行采数、不开 Δv″、不抬 thr、不翻 C1。

— notion ai（M2-H5-ABS-CONFLICT-RESOLVE 可 LOCK 修订）
