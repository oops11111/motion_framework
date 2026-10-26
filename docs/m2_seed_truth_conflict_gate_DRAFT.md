# M2-SEED-TRUTH-CONFLICT 门槛闸门（m2_seed_truth_conflict_gate_DRAFT.md）

STATUS=SUPERSEDED_BY_LOCKED（正式 LOCKED 见 `docs/m2_seed_truth_conflict_gate_LOCKED.md`）
LOCKED_AT: 见 LOCKED 文件 `2026-10-26 15:43:30 Asia/Taipei`
GATE_BODY_SHA256: 权威 hash 仅以 LOCKED 为准：`276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69`（本 DRAFT 不单独作权威 hash）
> **已 LOCK。** 正式正文：`docs/m2_seed_truth_conflict_gate_LOCKED.md`；补洞：`docs/notion_m2_seed_truth_conflict_gate_revise.md`。本 DRAFT 仅保留骨架溯源，**不以本文件作执行权威**。
> **本 DRAFT/LOCK 正文默认不授权真机采数。** 须 APPROVED_* 非空，且 APPROVED 正文**显式授权采数并写死范围**后，才可派 workbuddy 采数。GATE_BODY_SHA256 计算约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。本行写入后不得再改 body。
> 触发：M2-WGC-SEED-PATH-A′ = CLOSED SEED-PATHA-PRIME-FAIL；SEED-TRUTH-CONFLICT=HUNG（G2-LOCK-2b）；预登记 A′ 平滑臂已耗尽；result `docs/m2_wgc_seed_path_a_prime_result.md`；结题包 `docs/notion_m2_wgc_seed_path_a_prime_pack.md`；下一票 `docs/notion_m2_wgc_seed_path_a_prime_next.md`。  
> 前序双路径均 CLOSED FAIL：PATH-B′ `a1161c77…3a43`；PATH-A′ `f17871f1…2362`。  
> 前序闸门**不改、不重跑翻盘**：  
> - PATH-A′ FAIL `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362` @ 2026-10-26 14:38:15 Asia/Taipei  
> - PATH-B′ FAIL `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43` @ 2026-10-26 13:49:20 Asia/Taipei  
> - SEED-CALIB FAIL `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`  
> - SEED-ABS FAIL `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`  
> - Δv′ FAIL `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`  
> H5-SEED CLOSED PASS；M2-PERF-CI HOLD `5b007dd4…282a`（本票 ≠ PERF-CI）。  
> **本票 = 诊断/决议票**；**不是**再跑 seed 平滑臂；**不是** Δv″。

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

**本票结论不解冻 B_v / 不重开 R-2 / 不重开 S1 / R-3；不改 composition.h。**  
**本票成功 ≠ Δv-PASS ≠ SEED-ABS-PASS ≠ SEED-CALIB-PASS ≠ SEED-PATHB-PASS ≠ SEED-PATHA-PRIME-PASS ≠ 采用；结题标签主用 `SEED-TRUTH-*`。**  
**本票本身不预授权执行改 H5 分母 / 抬 thr / 降 17/20 / 再标定 / 立刻 Δv″。**

---

## G0 硬边界

- 不重开 S1 / R-3 / R-2；不解冻 / 不升级 B_v；成功 ≠ 采用；旁路挂账不挡、本票也不清旁路。
- 不改 `composition.h`（sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）。
- **禁影子 IV**。
- **禁翻盘**：禁止重跑 / 改写 / 宣称通过  
  - PATH-A′ FAIL `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362`  
  - PATH-B′ FAIL `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`  
  - SEED-CALIB FAIL `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`  
  - SEED-ABS FAIL `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`  
  - Δv′ FAIL `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`  
  禁止把本票产物标为 `Δv-PASS` / 前序 `SEED-*-PASS` / 五 FAIL 翻盘。
- **本票 ≠ PERF-CI**；产物不得作 PERF-CI 合格证据；PERF-CI 仍 HOLD。
- 不开 M3 真机。
- **禁立刻开 Δv″**。
- **禁**再开 calib 废止票当主线；**禁**再跑 α 调参 / 新平滑臂作本票主线。
- **禁**本票内执行抬 thr / 降 17/20 / 偷换估窗 / 改 H5 分母（菜单项最多「授权另票」，本票不执行）。
- 未 LOCKED+APPROVED 禁止采数；即便 APPROVED，**默认仍不开采数**，除非 APPROVED 显式写死采数范围。

## G1 票性质 · 诊断/决议（非采数主路径）

### 冲突陈述（须落盘）

本票须书面陈述并引用已归档证据：

1. **PATH-B′**（P-VS=OFF）：三会话 H5 比回归过 ∧ t060 smoke 全灭 → 首次挂账 SEED-TRUTH-CONFLICT。  
2. **PATH-A′**（P-VS=ON α_v=0.30）：s2/s3 比过 ∧ smoke 双灭 → **保持**挂账；s1 另因 cross-dir EMA **max_r=6.5997** 比出窗。  
3. 对照健全：两票有效会话均 basePass ∧ IVel0_t060_both_FAIL。  
4. 结构风险合法：\|ivel−v_pre\| 系统性偏大（例 A′ s3 Δv=75.577 thr=20）；FAIL ≠ 测法崩坏。  
5. **预登记平滑臂已耗尽**；比 / smoke **仍不可同票 PASS**。

冲突核心句（写死进结题）：**2F 比真值（H5 `v_screen` 分母）与绝对 `v_pre` / t060 smoke 门不一致**——不得默认用抬 thr / 改分母 / 再标定把任一 FAIL 闸门翻盘。

### 可选处置菜单（本票只决议，不执行）

以下每项若被选中，**必须另开新闸门 LOCK+APPROVED** 后方可执行；本票勾选 ≠ 授权执行：

| 菜单项 | 含义 | 本票可做 | 执行 |
|---|---|---|---|
| M1 授权改 H5 分母 | 另票定义新分母/估窗契约 | 决议是否推荐 | **另票** |
| M2 改 smoke 参照 | 另票改 v_pre / 参照定义（非抬 thr） | 决议是否推荐 | **另票** |
| M3 新估法族 | 另票预登记新路径（非默认同跑 α） | 决议是否推荐 | **另票** |
| M4 STOP 挂账 | 主线暂停 seed 绝对对齐；冲突归档 | 可本票拍板 STOP | 本票可 CLOSED STOP |
| M5 维持现状继续挂账 | 不改定义；等外因 | 可本票拍板 | 无执行改动 |

**明确否决为本票执行项**：抬 thr；降 17/20；再标定；立刻 Δv″；翻盘五 FAIL；关/改 α 偷换 A′。

### 真机采数（默认关）

- **默认：不开。**  
- 仅当 APPROVED 显式写：采数目的、格表、会话数、仍禁改的 KEEP 表，方可派 workbuddy。  
- 采数不得用于翻盘五 FAIL gate；产物前缀 `m2_seed_truth_*`。

## G2 主判据

### G2-LOCK-1 冲突根因陈述完备

结题须含：双路径证据表（比 / smoke / 对照）；A′ s1 max_r 与 s2/s3 med_r；「平滑臂耗尽」句；「结构风险 FAIL 合法」句。缺任一项 → 本票 **INVALID** 或退回补洞（不得标 PASS）。

### G2-LOCK-2 处置决议

结题须明确勾选菜单 **恰好一条主决议**（M1–M5 之一或经 LOCK 修订写入的等价项），并写明：  
- 若非 M4/M5：下一条另票名称占位；  
- **本票未执行**任何改分母 / 抬 thr / 改 composition.h。

### G2-LOCK-3 不翻盘核对

结题须逐条声明五 FAIL gate hash **未改、未重跑、未宣称通过**。

### G2-LOCK-4 票结局

结局 ∈ {`SEED-TRUTH-PASS`, `SEED-TRUTH-FAIL`, `SEED-TRUTH-INVALID`, `SEED-TRUTH-STOP`}（或 LOCK 修订等价标签）：

- **PASS**：根因陈述完备 ∧ 主决议写死 ∧ 五 FAIL 不翻盘 ∧ 未违规执行改动 ∧（若未授权则无采数）。  
- **STOP**：主决议=M4；归档冲突；主线暂停该类 seed 对齐（仍禁翻盘/立刻 Δv″）。  
- **FAIL**：决议未能在闸门范围内收敛（须写清缺口；不得借 FAIL 偷开采数或改分母）。  
- **INVALID**：见 G2-LOCK-5。

**禁止**写 `Δv-PASS` / 前序 `SEED-*-PASS` 翻盘。PASS ≠ 采用 ≠ PERF-CI ≠ 解冻 Bv ≠ 已改 H5 分母。

### G2-LOCK-5 INVALID（任一即整票）

1. 影子 IV / 改 composition.h  
2. 重跑/改写/宣称通过五 FAIL gate；产物标翻盘 PASS  
3. 本票内执行抬 thr / 降 17/20 / 改 H5 分母 / 再标定 / 立刻 Δv″  
4. 默认同跑新 α / 新估法偷换 A′ 或 PATH-B′ FAIL  
5. 未 LOCK+APPROVED 采数；或 APPROVED 未写死范围却采数  
6. 本票产物写入 PERF-CI 合格证据；开 M3；解冻 Bv / 重开 S1/R-3/R-2  
7. 把「比过」写成任一 FAIL 闸门已翻盘 PASS

## G3 产物

- `docs/m2_seed_truth_conflict_result.md`（LOCK+结题后）
- `docs/notion_m2_seed_truth_conflict_pack.md`（若走结题包体例）
- `artifacts/m2_seed_truth_*`（若有授权采数；**不得**冒充 `m2_wgc_seed_path_a_prime_*` / `m2_wgc_seed_path_b_prime_*` / calib / abs / delta_v_prime 翻盘包）

## G4 ONELINE

M2-SEED-TRUTH-CONFLICT | gate=<hash> @ <LOCKED_AT> | approved=<who>@<when> | mode=offline-resolve | evidence=PATHB+PATHA′ | s1_max_r=6.60 | s2s3_med_r≈0.85–0.86 | menu=<M?> | ticket=<SEED-TRUTH-*> | 成功≠采用≠Δv-PASS；不改composition.h；禁影子IV；≠PERF-CI；五FAIL不翻盘；禁立刻Δv″；本票不预授权改分母/抬thr；结构风险见G1

## 禁令复述

不重开 S1/R-3/R-2；不改 composition.h；不解冻/升级 Bv；禁影子 IV；成功≠采用；禁翻盘五 FAIL gate `f17871f1…2362` / `a1161c77…3a43` / `09f6414b…793d` / `831818e4…80de` / `06467b82…805c`；结题主用 `SEED-TRUTH-*`；禁标 `Δv-PASS` / 前序 SEED-*-PASS 翻盘；PERF-CI 不降级；不开 M3；禁抬 thr / 降 17/20 / 偷换估窗；禁本票预授权执行改 H5 分母；禁再标定主线；禁再跑 α 调参主线；禁立刻 Δv″；默认不开真机采数；结构风险下既有 FAIL 合法；处置菜单每项另票 LOCK。

— notion ai（M2-SEED-TRUTH-CONFLICT DRAFT；已 SUPERSEDED → LOCKED `276b97f0…4f69` @ 2026-10-26 15:43:30 Asia/Taipei；不授权采数）
