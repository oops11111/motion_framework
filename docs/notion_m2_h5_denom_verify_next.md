# M2 下一票决断（M2-H5-DENOM-VERIFY CLOSED H5-DENOM-VERIFY-FAIL · PERF-CI HOLD）

> DECISION_AT=2026-10-26 17:27:00 Asia/Taipei  
> 依据：`docs/notion_m2_h5_denom_verify_pack.md`；`docs/m2_h5_denom_verify_result.md`  
> 本 VERIFY gate `fd00a06058b239abfe03a52175463a52741718318ce99416a4ce762cf037889b` **不改、不重跑翻盘**  
> 七 hash：PATH-A′ `f17871f1…2362` / PATH-B′ `a1161c77…3a43` / CALIB `09f6414b…793d` / ABS `831818e4…80de` / Δv′ `06467b82…805c` / TRUTH `276b97f0…4f69` / DENOM `d891df9f…33c7` **不翻盘**  
> mode 本票已 CLOSED verify；`H5-DENOM-VERIFY-FAIL` ≠ PASS ≠ 落地采用 ≠ Δv-PASS；禁把 FAIL 当 PASS

## 结论

**选 C = 挂账旁路**（保留 `H5-DENOM-VERIFY-FAIL` 记录；不挫主线；不立刻再 VERIFY / 不虚构「再跑一遍」）。  
**次选 D = M2-H5-ABS-CONFLICT-RESOLVE**（协调决议票 · **doc-only**；专门决议「分母同源+C1 后比过、smoke 仍灭」的处置）。

**一句话为什么**：VERIFY 已证明相对比可过、绝对 smoke 仍系统性灭——与 SEED-TRUTH 核心矛盾未解；A′/另批 calib 式无明确新信息价值；禁抬 thr、禁立刻 Δv″ → 诚实路径是挂账或升级协调决议，而非再开采数翻盘。

若走次选 D，新 DRAFT 闸门文件名：`docs/m2_h5_abs_conflict_resolve_gate_DRAFT.md`（**NOT LOCKED**；本 next **不**授权采数）。  
主推 C **不**需新 DRAFT；本 VERIFY gate **不改不重跑**。

## 现状

| 项 | 状态 |
|---|---|
| M2-H5-DENOM-VERIFY | **CLOSED H5-DENOM-VERIFY-FAIL**；gate `fd00a060…889b` 不改；C1 exact=`1.3358225694150683`；pass_sessions=**0/3** |
| M2-H5-DENOM-REVISE | CLOSED H5-DENOM-PASS；mode=doc-only；gate `d891df9f…33c7` 不翻盘 |
| M2-SEED-TRUTH-CONFLICT | CLOSED SEED-TRUTH-PASS；menu=M1；gate `276b97f0…4f69` 不翻盘 |
| M2-WGC-SEED-PATH-A′ | CLOSED SEED-PATHA-PRIME-FAIL；`f17871f1…2362` 不翻盘（SEED 阶段 A′ 已耗尽） |
| M2-WGC-SEED-PATH-B′ | CLOSED SEED-PATHB-FAIL；`a1161c77…3a43` 不翻盘 |
| M2-WGC-SEED-CALIB / ABS / Δv′ | CLOSED FAIL；三 hash 不翻盘 |
| M2-Δv″ | 继续挂账；**不立刻开** |
| M2-PERF-CI | **HOLD**；不降级 |
| M3 | 不开真机 |

**已证事实（写死）**：KEEP `v_denom≡v_pre` + C1 后，H5 比三项齐（med_r≈0.99–1.16），smoke fwd/rev 均 ≪17/20（最高 fwd 12/20）；对照 KEEP。比 vs 绝对 **未解**。

## 候选速判

| 选项 | 决断 | 理由 |
|---|---|---|
| **A 开 A′ 另票**（V2-clamp+P-VS α_v=0.30） | **否（排除/不默认）** | SEED PATH-A′ 已 FAIL（s1 max_r=6.60）耗尽；本 VERIFY 比已过且 **fwd+rev 系统性灭**（非闸门所写「仅 rev 尖峰致灭」归因）；新分母+C1 下再开 A′ **新信息有限**，易与 C1 纠缠、归因不清；非默认必选 |
| **B 另批 calib 式**（非静默 0.7511；非翻本次 C1 exact） | **否** | 本次 C1 exact=`1.3358225694150683` 已钉；med_r≈1 说明标定对分母已对齐；改式追 smoke 易沦钓鱼并破坏比窗；**禁翻本次 C1 exact** |
| **C 挂账旁路** | **采纳（主推）** | 保留 FAIL 记录；不挫主线；无新信息路径时不虚构再跑；禁抬 thr / 禁立刻 Δv″ / 禁翻盘 |
| **D 其它：M2-H5-ABS-CONFLICT-RESOLVE**（协调决议 · doc-only） | **次选** | 若需主动收敛：决议「比过 smoke 仍灭」→ STOP / 另开绝对门定义票（仍禁抬 thr）/ 或显式归档；**≠** 立刻采数；**≠** Δv″ |
| E 抬 thr / 降 17/20 | **否** | 硬禁；不治比–绝对矛盾 |
| F 立刻 Δv″ | **否** | 禁立刻；比+smoke 未齐 |
| G 重跑本 VERIFY / 翻七+VERIFY hash | **否** | 禁翻盘；本 gate 不改不重跑 |
| H 默认可落地 / 把 FAIL 当 PASS | **否** | 落地=否；禁把 FAIL 当 PASS |
| I PERF-CI / 开 M3 / 解冻 Bv / 重开 S1·R-3·R-2 | **否** | HOLD；硬禁 |
| J 改 composition.h / 影子 IV | **否** | 硬禁 |

## 可 LOCK 要点（仅当走次选 D）

须**新闸门**；APPROVED 须**显式**（本 next **不**授权）：

1. mode=**doc-only**（默认）；验证采数=**否**（除非另字段显式写死且有新假设）。  
2. 议题写死：分母同源+C1 后「比过、smoke 灭」的处置菜单（STOP / 另开绝对门定义 / 挂账确认 / …）。  
3. **禁抬 thr**；禁降 17/20。  
4. **七 + VERIFY hash 不翻盘**（含本 VERIFY `fd00a060…889b`）。  
5. **禁立刻 Δv″**；PERF-CI HOLD；不开 M3；不解冻 Bv；不重开 S1/R-3/R-2；不改 composition.h（`a55d4952…e418`）；禁影子 IV。  
6. 结题标签建议 `H5-ABS-RESOLVE-*`（或等价）；**≠** 把 VERIFY-FAIL 改写成 PASS；**≠** Δv-PASS。

主推 C **无需** LOCK 新闸门；VERIFY 归档即止。

### 否决虚 PASS / 偷换

| 反例 | 否决 |
|---|---|
| 抬 thr / 降 17/20 救 VERIFY | INVALID |
| 立刻 Δv″ | INVALID |
| 重跑/改写七+VERIFY hash；把 FAIL 标 PASS | INVALID |
| 默认可落地；偷开 A′ 当本票续跑 | INVALID |
| 翻本次 C1 exact / 静默回 0.7511 | INVALID |
| 开 M3；解冻 Bv；改 composition.h；影子 IV | INVALID |
| 把本 next / 未 LOCK DRAFT 当已授权采数 | 否决 |

## DRAFT 路径

- 主推 C：无新 DRAFT。  
- 次选 D：`docs/m2_h5_abs_conflict_resolve_gate_DRAFT.md`（**NOT LOCKED**；**不授权采数**；本 next 仅写文件名占位，**未**落盘该 DRAFT 正文——由协调派写）。

## 协调下一步

1. **主路径**：归档 `H5-DENOM-VERIFY-FAIL`；挂账旁路；主线不因本 FAIL 强行再采；Δv″ / PERF-CI 继续 HOLD。  
2. **若需主动收敛**：派同伴写 `docs/m2_h5_abs_conflict_resolve_gate_DRAFT.md`（doc-only 决议菜单）→ Notion 可 LOCK → LOCK → pidan APPROVED（须显式字段）。  
3. **禁**在无新假设下重开 VERIFY 采数；**禁**把 FAIL 当 PASS；**禁**立刻 Δv″；**禁**抬 thr。

## 确认（硬边界）

- 不重开 S1 / R-3 / R-2  
- 不改 composition.h（`a55d4952…e418`）  
- 不解冻 Bv  
- 禁影子 IV  
- 禁把 FAIL 当 PASS  
- 本 VERIFY gate **不改不重跑翻盘**  
- 禁抬 thr；禁立刻 Δv″；PERF-CI HOLD  

## 禁令

不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；成功≠落地采用≠Δv-PASS；禁把 FAIL 当 PASS；**不改/不重跑** `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362` / `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43` / `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d` / `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de` / `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c` / TRUTH `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69` / DENOM `d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7` / 本 VERIFY `fd00a06058b239abfe03a52175463a52741718318ce99416a4ce762cf037889b`；PERF-CI HOLD；不开 M3；禁抬 thr；禁立刻 Δv″；禁翻本次 C1 exact；本 next **不**授权采数。

— notion ai（下一票主推=挂账旁路 C；次选=M2-H5-ABS-CONFLICT-RESOLVE D；禁立刻 Δv″）
