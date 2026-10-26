# M2-H5-DENOM-REVISE 门槛闸门（m2_h5_denom_revise_gate_DRAFT.md）

STATUS=SUPERSEDED_BY_LOCKED（正式 LOCKED 见 `docs/m2_h5_denom_revise_gate_LOCKED.md`）
LOCKED_AT: 见 LOCKED 文件 `2026-10-26 15:55:00 Asia/Taipei`
GATE_BODY_SHA256: 权威 hash 仅以 LOCKED 为准：`d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7`（本 DRAFT 不单独作权威 hash）
> **已 LOCK。** 正式正文：`docs/m2_h5_denom_revise_gate_LOCKED.md`；补洞：`docs/notion_m2_h5_denom_revise_gate_revise.md`。本 DRAFT 仅保留骨架溯源，**不以本文件作执行权威**。
> **本 DRAFT/LOCK 正文默认不授权真机采数；默认不授权改分母落地。** 须 APPROVED_* 非空，且 APPROVED 正文**显式写死**（是否授权验证采数 / 是否授权落地 / calib 新常量或 KEEP 声明）后，才可派执行或真机。GATE_BODY_SHA256 计算约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。
> 触发：M2-SEED-TRUTH-CONFLICT = CLOSED **SEED-TRUTH-PASS**；menu=**M1**；mode=offline-resolve；result `docs/m2_seed_truth_conflict_result.md`；结题包 `docs/notion_m2_seed_truth_conflict_pack.md`；下一票 `docs/notion_m2_seed_truth_conflict_next.md`。  
> TRUTH gate（**不改、不翻盘**）：`276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69` @ 2026-10-26 15:43:30 Asia/Taipei。  
> 前序五 FAIL（**不改、不重跑翻盘**）：  
> - PATH-A′ `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362`  
> - PATH-B′ `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`  
> - SEED-CALIB `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`  
> - SEED-ABS `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`  
> - Δv′ `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`  
> H5-SEED CLOSED PASS（旧 2F 契约仍归档）；M2-PERF-CI HOLD `5b007dd4…282a`（本票 ≠ PERF-CI）。  
> **本票 = 新分母 / 估窗契约定义**；**不是**落地执行；**不是** Δv″；**不是**抬 thr；**不是**翻盘 TRUTH/五 FAIL。

**本票结论不解冻 B_v / 不重开 R-2 / 不重开 S1 / R-3；不改 composition.h。**  
**本票成功 ≠ Δv-PASS ≠ 五 FAIL 翻盘 ≠ TRUTH 翻盘 ≠ 采用；结题标签主用 `H5-DENOM-*`（或等价）。**  
**已 LOCK；权威见 LOCKED。** DRAFT/LOCK 正文默认不授权真机采数；默认不授权未批的分母落地。

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
  - TRUTH gate `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69`  
  禁止把本票产物标为 `Δv-PASS` / 前序 `SEED-*-PASS` / `SEED-TRUTH-PASS` 翻盘重写。
- **本票 ≠ PERF-CI**；产物不得作 PERF-CI 合格证据；PERF-CI 仍 HOLD。
- 不开 M3 真机。
- **禁立刻开 Δv″**（须新契约下比+smoke 可齐后再另开）。
- **禁抬 thr**；**禁**降 17/20 伪装 PASS。
- 未 LOCKED+APPROVED 禁止采数 / 禁止分母落地；即便 APPROVED，**采数与落地范围须正文写死**。

## G1 票性质 · 新分母契约（M1 执行票）

### 根因引用（只读；不重采）

TRUTH 已写死：旧 H5 分母=`v_screen`（打断前末两帧 WGC）与绝对 `v_pre` / t060 smoke **不可同票 PASS**；平滑臂已耗尽；结构风险 FAIL 合法。本票任务是**另开契约**使比真值与绝对连续**同源**，不是调 thr。

### 须在 LOCK 前写死（补洞清单）

1. **新分母定义**：符号、测量窗、时基、单位（DIP/s）、与 seed 赋值对齐规则。  
2. **比–绝对同源**：新分母与 t060 / `v_pre`（或写死等价绝对参照）如何同源；禁止「比钉旧 2F、绝对钉另一套」。  
3. **回归门槛**：比回归窗口（med_r / any_ri）与 smoke 格门槛；**不得**相对旧票抬 thr / 降 17/20。  
4. **KEEP 表**：composition.h；禁影子 IV；五 FAIL + TRUTH 不翻盘；calib / N / order / ε 等默认 KEEP 或显式另批。  
5. **采数**：默认关；若需真机，APPROVED 须写死目的、格表、会话数、仍禁改 KEEP。  
6. **结题标签**：`H5-DENOM-PASS|FAIL|INVALID|STOP`（或等价）；明确 ≠ Δv-PASS ≠ 五 FAIL/TRUTH 翻盘。

### 明确否决

抬 thr；降 17/20；翻盘五 FAIL / TRUTH；立刻 Δv″；未授权采数；未 LOCK 即改分母落地；改 composition.h；影子 IV；开 M3；解冻 Bv；把「比过」写成任一 FAIL 闸门已翻盘 PASS。

## G2 主判据（骨架；LOCK 时补齐数值）

### G2-LOCK-1 契约完备

结题须含：新分母定义全文；比–绝对同源句；禁抬 thr 句；五+TRUTH 不翻盘逐条；本票是否执行落地 / 是否采数声明。缺任一项 → INVALID 或退回补洞。

### G2-LOCK-2 执行边界

- LOCK+APPROVED 前：**禁止**改分母落地与采数。  
- 勾选/决议 ≠ 自动执行；执行范围以 APPROVED 正文为准。

### G2-LOCK-3 不翻盘核对

结题须逐条声明六 hash **未改、未重跑、未宣称通过**（五 FAIL + TRUTH `276b97f0…4f69`）。

### G2-LOCK-4 票结局（建议）

结局 ∈ {`H5-DENOM-PASS`, `H5-DENOM-FAIL`, `H5-DENOM-INVALID`, `H5-DENOM-STOP`}（或等价）。  
**PASS ≠ 采用 ≠ Δv-PASS ≠ 五 FAIL/TRUTH 翻盘**。

### G2-LOCK-5 INVALID（骨架）

1. 影子 IV / 改 composition.h  
2. 重跑/改写/宣称通过五 FAIL 或 TRUTH gate  
3. 抬 thr / 降 17/20  
4. 未 LOCK+APPROVED 采数或分母落地  
5. 立刻开 Δv″；开 M3；解冻 Bv / 重开 S1/R-3/R-2  
6. 产物作 PERF-CI 合格证据  
7. 把「比过」写成任一 FAIL/TRUTH 已翻盘 PASS

## G3 产物（预期）

- `docs/m2_h5_denom_revise_result.md`（LOCK+结题后）
- `docs/notion_m2_h5_denom_revise_pack.md`（若走结题包体例）
- `artifacts/m2_h5_denom_revise_*`（若有授权采数；**不得**冒充 path_a/path_b/calib/abs/delta_v/truth 翻盘包）

## G4 ONELINE（模板）

M2-H5-DENOM-REVISE | gate=<hash> @ <LOCKED_AT Asia/Taipei> | approved=<who>@<when> | mode=<doc-only|exec|…> | ticket=<H5-DENOM-*> | 新分母须LOCK；比–绝对同源；禁抬thr；五FAIL+TRUTH不翻盘；成功≠采用≠Δv-PASS；不改composition.h；禁影子IV；≠PERF-CI；禁立刻Δv″；采数须APPROVED写死范围

## 禁令复述

不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；成功≠采用；禁翻盘五 FAIL + TRUTH gate `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69`；禁抬 thr / 降 17/20；禁立刻 Δv″；PERF-CI HOLD；不开 M3；本 DRAFT **不授权采数**；新分母定义须 LOCK 后另批方可执行。

— notion ai（M2-H5-DENOM-REVISE DRAFT；SUPERSEDED_BY_LOCKED；权威见 LOCKED；不授权采数不落地）
