# M2-H5-DENOM-REVISE 闸门补洞决断（可 LOCK → 已 LOCK）

> DECISION_AT=2026-10-26 15:55:00 Asia/Taipei（同 UTC+8 = Australia/Perth）  
> 依据：`docs/deepseek_m2_h5_denom_revise_review.md`（Harness 空回代拟；判可 LOCK；推荐 **v_denom≡v_pre**；废旧 2F 作门控分母；验证/落地须另批 calib，禁静默 0.7511）；`docs/m2_h5_denom_revise_gate_DRAFT.md`；`docs/notion_m2_seed_truth_conflict_next.md` / pack / `docs/m2_seed_truth_conflict_result.md`  
> 六 hash **不改、不重跑翻盘**：  
> - TRUTH `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69`  
> - PATH-A′ `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362`  
> - PATH-B′ `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`  
> - CALIB `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`  
> - ABS `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`  
> - Δv′ `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`  
> composition.h **不改** `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`  
> LOCKED：`docs/m2_h5_denom_revise_gate_LOCKED.md`  GATE_BODY_SHA256=`d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7` @ 2026-10-26 15:55:00 Asia/Taipei  
> **LOCK ≠ 授权采数 ≠ 授权改分母落地**；APPROVED_* 空；默认不开采数、不落地；PERF-CI 仍 HOLD；不开 M3；禁立刻 Δv″。  
> **声明**：TRUTH PASS ≠ 已改 H5；本票 LOCK 只定义契约，不执行改分母。

## 逐条决断

| ID | 项 | 决断 |
|---|---|---|
| D1 | 新分母 | **采纳 D1**：`v_denom ≡ v_pre`（二次 LS · w=5 · 同 QPC · 同 `t_int`） |
| D2 | 旧 2F | **废**旧 `v_screen` 作门控分母；可归档对照 / 辅证 log |
| D3 | 比–绝对同源 | **写死**：比分母与 smoke `v_pre` 同定义；禁「比钉 2F、绝对钉另一套」 |
| D4 | 回归门槛 | **KEEP**：med_r∈[0.8,1.2] 且无 r_i≥2；t060 ≥17/20；可判规则 KEEP；**禁抬 thr / 降 17/20** |
| D5 | calib | **验证/落地须另批**；禁静默沿用 0.7511；APPROVED 须声明新常量或 KEEP |
| D6 | 采数/落地 | **默认关**；LOCK≠授权；APPROVED 须显式字段（是否验证采数 / 是否落地 / calib） |
| D7 | 标签 | 主用 `H5-DENOM-PASS\|FAIL\|INVALID\|STOP`；PASS ≠ 已落地采用 ≠ Δv-PASS ≠ 翻盘前序 |
| D8 | 虚 PASS 否决表 | **进 LOCK 正文** |
| D9 | 六 hash | **不翻盘**（全文写入 LOCK） |
| D10 | composition.h / 影子 IV | **KEEP / 禁** |
| D11 | Δv″ / PERF-CI / M3 | Δv″ **继续挂账**；PERF-CI HOLD；不开 M3；不解冻 Bv；不重开 S1/R-3/R-2 |
| D12 | DeepSeek 可 LOCK | **采纳**；最小修订点并入 LOCK |

## 采纳点（写死并入 LOCK）

1. **新分母**：`v_denom ≡ v_pre`（与 t060 smoke 同窗二次 LS w=5 · 同 QPC · 同 t_int）；旧 2F `v_screen` **不作**本票门控分母。  
2. **比–绝对同源句**：比真值与绝对连续同源契约写死。  
3. **回归门槛 KEEP**：med_r∈[0.8,1.2] 且无 r_i≥2；t060 ≥17/20；禁抬 thr / 降 17/20。  
4. **calib**：验证/落地须另批；禁静默 0.7511；APPROVED 须声明字段。  
5. **默认不开采数、不落地**；落地须另 APPROVED 显式字段。  
6. **六 hash 不翻盘**（全文）；禁影子 IV；composition.h 锁。  
7. 标签 `H5-DENOM-*`；PASS ≠ 已落地采用 ≠ Δv-PASS ≠ 翻盘前序；虚 PASS 否决表进正文。  
8. 成功≠采用；≠PERF-CI；不开 M3；不解冻 Bv；不重开 S1/R-3/R-2；禁立刻 Δv″。

## 否决虚 PASS（须进 LOCK）

| 行为 | 处置 |
|---|---|
| 未 LOCK+APPROVED 改分母落地 / 采数 | INVALID |
| 抬 thr / 降 17/20 | INVALID |
| 比仍用旧 2F 分母却宣称新契约 PASS | INVALID |
| 静默沿用 calib=0.7511（旧 2F）宣称新分母比 PASS | INVALID |
| 重跑/宣称通过六 hash；标 `Δv-PASS` / 前序 `SEED-*-PASS` / 重写 `SEED-TRUTH-PASS` | INVALID |
| 立刻 Δv″；开 M3；解冻 Bv；改 composition.h；影子 IV | INVALID |
| 把 TRUTH PASS 写成已改 H5 | INVALID |
| 产物作 PERF-CI 合格证据 | INVALID |
| 把 doc-only PASS 写成已落地采用 | INVALID |

## LOCK 粘贴句（对齐 DeepSeek §7 / 一句结论）

> 本票=新分母契约定义（不执行落地）；**v_denom≡v_pre**（二次 LS w=5·同 QPC·同 t_int）；旧 2F 不作门控分母；比–绝对同源；门槛 KEEP（med_r∈[0.8,1.2]∧无 r_i≥2；t060≥17/20；禁抬 thr/降 17/20）；calib 验证/落地须另批（禁静默 0.7511）；默认不开采数不落地；虚 PASS 否决表进正文；`H5-DENOM-*`；六 hash 不翻盘；≠PERF-CI；不改 composition.h；**可 LOCK**；LOCK≠授权采数≠授权落地；TRUTH PASS≠已改 H5。

## 与 DeepSeek 分歧

无实质分歧。DeepSeek 判「可 LOCK」、推荐 **D1=`v_denom≡v_pre`**、废旧 2F 作门控分母、calib 另批禁静默 0.7511、默认不开采数、六 hash 不翻盘——均已并入 LOCK。协调补强（相对 DeepSeek 最小修订点）：

1. APPROVED 须声明字段表写死为三字段骨架：**是否授权验证采数** / **是否授权落地** / **calib 新常量或 KEEP 声明**（DeepSeek §5 已含采数字段；本票显式把「落地」与「calib」升为同级必填）。  
2. 可判规则 KEEP 写入回归门槛表（DeepSeek 门槛表未单列；并入 G1-D3）。  
3. 结局标签明确 **doc-only PASS ≠ 已落地采用**（DeepSeek 已述；LOCK G2-LOCK-4 / 否决表再钉一次）。

## 结论

**可 LOCK。** 已 LOCK → `docs/m2_h5_denom_revise_gate_LOCKED.md`（GATE_BODY_SHA256=`d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7` @ 2026-10-26 15:55:00 Asia/Taipei）。APPROVED_* 空；不授权采数；不授权改分母落地；本票不执行改分母。

— notion ai（M2-H5-DENOM-REVISE 可 LOCK 修订）
