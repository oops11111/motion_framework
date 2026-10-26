# M2-SEED-TRUTH-CONFLICT 闸门补洞决断（可 LOCK → 已 LOCK）

> DECISION_AT=2026-10-26 15:43:30 Asia/Taipei（同 UTC+8 = Australia/Perth）  
> 依据：`docs/deepseek_m2_seed_truth_conflict_review.md`（Harness 空回代拟；判可 LOCK；推荐主决议 **M1**；次选 M4；不推荐 M2/M3/M5）；`docs/m2_seed_truth_conflict_gate_DRAFT.md`；`docs/notion_m2_wgc_seed_path_a_prime_next.md` / `docs/notion_m2_wgc_seed_path_a_prime_pack.md`  
> 前序 FAIL gate **不改、不重跑翻盘**：  
> - PATH-A′ `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362`  
> - PATH-B′ `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`  
> - CALIB `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`  
> - ABS `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`  
> - Δv′ `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`  
> composition.h **不改** `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`  
> LOCKED：`docs/m2_seed_truth_conflict_gate_LOCKED.md`  GATE_BODY_SHA256=`276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69` @ 2026-10-26 15:43:30 Asia/Taipei  
> **不授权采数**；须 APPROVED_* 非空且显式写死采数范围后方可派 workbuddy；PERF-CI 仍 HOLD；不开 M3；禁立刻 Δv″。

## 逐条决断

| ID | 项 | 决断 |
|---|---|---|
| T1 | 票性质 | **诊断/决议**；非再跑 seed；非 Δv″；**默认不开采数**；LOCK ≠ 授权采数 |
| T2 | 冲突根因 | **写死**：2F 比真值（H5 `v_screen`）vs 绝对 `v_pre`/t060 smoke；比与 smoke 不可同票 PASS |
| T3 | 双路径证据 | **引用归档**：PATH-B′ `a1161c77…3a43`（三会话比过∧smoke 全灭）+ PATH-A′ `f17871f1…2362`（s2/s3 比过∧smoke 双灭；s1 max_r=6.5997） |
| T4 | 平滑臂 / 结构风险 | **写死**：平滑臂已耗尽；对照健全；结构风险下 FAIL 合法；不得抬 thr/改分母/再标定翻盘 |
| T5 | 主决议倾向 | **采纳 M1**：授权另票改 H5 分母 / 比–绝对同源；**本票不执行** |
| T6 | 次选 | **可记 M4 STOP**（拒 M1 时显式归档） |
| T7 | 结题勾选 | **恰好一条**：本 LOCK 允许 **M1 或 M4** 二选一；勾选 M2/M3/M5 作主决议 → INVALID |
| T8 | 不推荐 | **否** M2（弱化绝对门）；**否** M3（撞同一结构墙）；**否** M5（不收敛） |
| T9 | 另票占位（若 M1） | 建议名 `M2-H5-DENOM-REVISE`；须新闸门写死分母/对齐/回归/禁抬 thr/禁翻盘 |
| T10 | 五禁翻盘 | **禁**重跑/改写/宣称通过五全文 hash（含 A′） |
| T11 | 标签 | 主用 `SEED-TRUTH-PASS\|FAIL\|INVALID\|STOP`；禁 `Δv-PASS` / 前序 `SEED-*-PASS` 翻盘；`SEED-TRUTH-PASS` ≠ 已改 H5 ≠ 采用 |
| T12 | 虚 PASS 否决表 | **进 LOCK 正文**（抬 thr/执行改分母/再标定/立刻 Δv″/未 APPROVED 采数/PERF-CI/影子 IV 等） |
| T13 | Δv″ / PERF-CI / M3 | Δv″ **继续挂账**；PERF-CI HOLD；不开 M3；不解冻 Bv；不重开 S1/R-3/R-2 |
| T14 | DeepSeek 可 LOCK | **采纳**；三条最小修订点并入 LOCK |

## 采纳点（写死并入 LOCK）

1. **冲突根因**：2F 比真值 vs 绝对 v_pre/smoke；双路径证据表；平滑臂耗尽；结构风险 FAIL 合法。  
2. **主决议倾向 M1**（授权另票改 H5 分母）；本票**不执行**改分母/抬 thr/改 smoke。  
3. **次选 M4 STOP**；结题须勾选**恰好一条**（M1 或 M4）。  
4. **不推荐 M2/M3/M5**；勾选之作主决议 → INVALID。  
5. **五 FAIL 不翻盘**（全文 hash）；禁影子 IV；composition.h 锁。  
6. **默认不开采数**；未来另票采数须新 LOCK+APPROVED。  
7. 成功≠采用≠Δv-PASS≠已改 H5；≠PERF-CI；不开 M3；不解冻 Bv；不重开 S1/R-3/R-2；禁立刻 Δv″。

## 否决虚 PASS（须进 LOCK）

| 行为 | 处置 |
|---|---|
| 本票内抬 thr / 降 17/20 | INVALID |
| 本票内执行改 H5 分母（勾选 M1 ≠ 执行） | INVALID |
| 再标定 / 再跑 α / 默认同跑新估法偷换 A′/PATH-B′ | INVALID |
| 重跑/宣称通过五 FAIL gate；标 `Δv-PASS` / 前序 `SEED-*-PASS` | INVALID |
| 立刻开 Δv″ | 否决 / INVALID |
| 未 LOCK+APPROVED 采数；或 APPROVED 未写死范围却采数 | INVALID |
| 把「比过」写成任一 FAIL 闸门已翻盘 PASS | INVALID |
| 产物作 PERF-CI；开 M3；解冻 Bv；改 composition.h；影子 IV | INVALID |
| 结题主决议勾选 M2/M3/M5 | INVALID |

## LOCK 粘贴句（对齐 DeepSeek §5 / 一句结论）

> 本票=诊断/决议；冲突根因=2F 比真值 vs 绝对 v_pre/smoke；双路径 PATH-B′+PATH-A′ 证据；平滑臂耗尽；结构风险 FAIL 合法；**主决议倾向 M1**（授权另票改 H5 分母；本票不执行）；**次选 M4 STOP**；结题恰好勾选 M1 或 M4；不推荐 M2/M3/M5；五 FAIL 不翻盘；默认不开采数；虚 PASS 否决表进正文；`SEED-TRUTH-*`；禁立刻 Δv″；≠PERF-CI；不改 composition.h；**可 LOCK**；LOCK≠授权采数。

## 与 DeepSeek 分歧

无实质分歧。DeepSeek 判「可 LOCK」、推荐主决议 **M1**、次选 M4、不推荐 M2/M3/M5、默认不开采数、五 FAIL 不翻盘——均已并入 LOCK。协调补强（相对 DeepSeek 最小修订点）：

1. 结题勾选规则写死为本 LOCK 允许的 **M1∨M4 二选一**（DeepSeek 菜单含 M1–M5；本票把「不推荐」升为结题硬约束：勾选 M2/M3/M5 → INVALID）。  
2. next 曾否「本轮默认 STOP」——与 DeepSeek「次选 M4、不默认 STOP」一致：LOCK **不**默认 STOP，仅作次选。  
3. 下一条另票占位名 `M2-H5-DENOM-REVISE` 已写入 G1。

## 结论

**可 LOCK。** 已 LOCK → `docs/m2_seed_truth_conflict_gate_LOCKED.md`（GATE_BODY_SHA256=`276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69` @ 2026-10-26 15:43:30 Asia/Taipei）。APPROVED_* 空；不授权采数；本票不执行改 H5 分母。

— notion ai（M2-SEED-TRUTH-CONFLICT 可 LOCK 修订）
