# M2-H5-ABS-CONFLICT-RESOLVE 门槛闸门（m2_h5_abs_conflict_resolve_gate_DRAFT.md）

> **SUPERSEDED_BY_LOCKED**：`docs/m2_h5_abs_conflict_resolve_gate_LOCKED.md`
> GATE_BODY_SHA256=`e96bf6238ace42ce2edcfe4191ef5ec45fa53eefca94fcf777f2b52c17425ade`
> LOCKED_AT=2026-10-26 18:30:00 Asia/Taipei
> 本 DRAFT 保留不删；以 LOCKED 为准。


STATUS=DRAFT（**NOT LOCKED**；本文件**不授权采数**；不授权改 thr；不授权落地；不授权立刻 Δv″）
LOCKED_AT: （LOCK 时填）
GATE_BODY_SHA256: （LOCK 时算；约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希）
> **本 DRAFT 只搭骨架；mode 默认 doc-only；默认不授权真机采数。** 须另票 LOCK + APPROVED_* 非空，且 APPROVED 正文**显式写死**（mode；menu 选项；禁抬 thr；八 hash 不翻盘；禁立刻 Δv″）后，方可结题。  
> 触发：M2-H5-DENOM-VERIFY = CLOSED **H5-DENOM-VERIFY-FAIL**；result `docs/m2_h5_denom_verify_result.md`；结题包 `docs/notion_m2_h5_denom_verify_pack.md`；下一票 `docs/notion_m2_h5_denom_verify_next.md`（主推挂账 C；次选本票 D）。  
> VERIFY gate（**不改、不翻盘**）：`fd00a06058b239abfe03a52175463a52741718318ce99416a4ce762cf037889b` @ 2026-10-26 16:09:00 Asia/Taipei。  
> DENOM gate（**不改、不翻盘**）：`d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7`。  
> TRUTH gate（**不改、不翻盘**）：`276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69`。  
> 前序五 FAIL（**不改、不重跑翻盘**）：  
> - PATH-A′ `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362`  
> - PATH-B′ `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`  
> - SEED-CALIB `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`  
> - SEED-ABS `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`  
> - Δv′ `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`  
> M2-PERF-CI HOLD `5b007dd4…282a`（本票 ≠ PERF-CI）。  
> **已证（写死）**：KEEP `v_denom≡v_pre` + C1 exact=`1.3358225694150683` 后，H5 **比三项齐**、smoke **≪17/20**（pass_sessions=0/3）；比 vs 绝对在分母同源后**仍未解**。  
> **本票 = 协调决议（doc-only）**：「比过、smoke 灭」的处置菜单；**不是**再 VERIFY 采数；**不是**抬 thr；**不是**立刻 Δv″；**不是**把 FAIL 改写成 PASS。  
> **声明**：`H5-DENOM-VERIFY-FAIL` 归档保留；本 DRAFT ≠ 授权采数。

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

**本票结论不解冻 B_v / 不重开 R-2 / 不重开 S1 / R-3；不改 composition.h。**  
**本票成功 ≠ 采用 ≠ 把 VERIFY-FAIL 改 PASS ≠ Δv-PASS ≠ 八 hash 翻盘；结题标签主用 `H5-ABS-RESOLVE-*`（或等价）。**  
**DRAFT ≠ 授权采数 ≠ 授权抬 thr ≠ 授权立刻 Δv″；勾选/决议 ≠ 自动执行；APPROVED_* 空。**  
**默认：mode=doc-only；不开采数。**

---

## G0 硬边界

- 不重开 S1 / R-3 / R-2；不解冻 / 不升级 B_v；成功 ≠ 采用；旁路挂账不挡、本票也不清旁路（除非 menu 显式「挂账确认」写入）。
- 不改 `composition.h`（sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）。
- **禁影子 IV**。
- **禁翻盘**：禁止重跑 / 改写 / 宣称通过  
  - PATH-A′ FAIL `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362`  
  - PATH-B′ FAIL `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`  
  - SEED-CALIB FAIL `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`  
  - SEED-ABS FAIL `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`  
  - Δv′ FAIL `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`  
  - TRUTH gate `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69`  
  - DENOM gate `d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7`  
  - VERIFY gate `fd00a06058b239abfe03a52175463a52741718318ce99416a4ce762cf037889b`  
  禁止把本票产物标为 `Δv-PASS` / 前序 `SEED-*-PASS` / `SEED-TRUTH-PASS` / 重写 `H5-DENOM-PASS` 为已落地 / **把 `H5-DENOM-VERIFY-FAIL` 改写成 PASS**。
- **本票 ≠ PERF-CI**；产物不得作 PERF-CI 合格证据；PERF-CI 仍 HOLD。
- 不开 M3 真机。
- **禁立刻开 Δv″**（须比+smoke 可齐或 menu 显式另路径后再论；本票默认不开）。
- **禁抬 thr**；**禁**降 17/20 伪装 PASS。
- **禁翻本次 C1 exact** `1.3358225694150683`；禁静默回 0.7511。
- 未 LOCKED+APPROVED 禁止采数；即便 APPROVED，默认仍 **doc-only**（采数须另字段显式写死且有新假设）。

## G1 票性质 · 比过 smoke 灭 · 处置菜单（骨架）

### 继承事实（只读；不翻盘）

| 项 | KEEP / 写死 |
|---|---|
| VERIFY 结局 | CLOSED **H5-DENOM-VERIFY-FAIL**；pass_sessions=**0/3** |
| 分母 | KEEP **`v_denom ≡ v_pre`** |
| calib | C1 exact=**`1.3358225694150683`**（n=120）；**禁翻 / 禁静默 0.7511** |
| 路径（VERIFY） | PATHB-A；P-VS=OFF；A′=OFF |
| H5 比 | 三项齐（med_r≈0.99–1.16；无 r_i≥2） |
| smoke | 三项皆 ≪17/20（最高 fwd 12/20）；**未抬 thr；未降 17/20** |
| 对照 | KEEP basePass ∧ IVel0_t060_both_FAIL |

### 须在 LOCK 前写死（补洞清单）

1. **议题**：分母同源 + C1 后「**比过、smoke 灭**」的处置。  
2. **menu 选项写死**（见同伴 review；须择一主决议）：  
   - **R1 STOP** — 停止在现行 thr/17/20 下继续追 H5 绝对齐；VERIFY-FAIL 归档。  
   - **R2 挂账确认** — 显式挂账「比–绝对冲突」旁路；主线不因本 FAIL 强行再采；与 Notion next 主推 C 对齐。  
   - **R3 另开绝对门定义票** — **另票**讨论绝对 smoke 的**定义/几何**（仍 **禁抬 thr / 禁降 17/20**）；本票只决议「要不要开」、不开采数。  
   - **R4 否决项**（不得作主决议）：抬 thr；降 17/20；立刻 Δv″；重跑 VERIFY 翻盘；默认同跑 A′；翻 C1 exact；把 FAIL 当 PASS；默认可落地。  
3. **mode=doc-only**（默认）；验证采数=**否**。  
4. **八 hash 不翻盘**（七前序 + VERIFY）。  
5. **结题标签**：`H5-ABS-RESOLVE-PASS|FAIL|INVALID|STOP`（或等价）；明确 ≠ VERIFY-PASS 改写 ≠ Δv-PASS。  
6. **产物前缀**：`m2_h5_abs_conflict_resolve_*` only。

### APPROVED 须显式字段（LOCK 后拍板）

| 字段 | 须填 |
|---|---|
| mode | **doc-only**（默认） |
| 主决议 menu | **R1 / R2 / R3**（择一；禁 R4） |
| 验证采数 | **否**（除非另字段+新假设） |
| 禁抬 thr / 降 17/20 | 是 |
| 八 hash 不翻盘 | 是 |
| 禁立刻 Δv″ | 是 |
| 禁翻 C1 exact | 是 |
| PERF-CI | HOLD |

### 明确否决

立刻 Δv″；抬 thr；降 17/20；重跑/改写八 hash；把 VERIFY-FAIL 标 PASS；翻 C1 exact / 静默 0.7511；默认同跑 A′；未 LOCK/未 APPROVED 即采数；改 composition.h；影子 IV；开 M3；解冻 Bv；把本 DRAFT 当已授权采数。

## G2 主判据（骨架；LOCK 时补齐）

### G2-LOCK-1 议题与菜单完备

结题须确认：已证事实（比过 smoke 灭）写死；menu 主决议 ∈ {R1,R2,R3}；R4 否决表齐；禁抬 thr；八 hash 不翻盘逐条。缺任一项 → INVALID 或退回补洞。

### G2-LOCK-2 执行边界

- LOCK+APPROVED 前：**禁止**采数与抬 thr。  
- 勾选/决议 ≠ 自动执行；执行范围以 APPROVED 正文为准。  
- **本 DRAFT 阶段**：只搭骨架；**不授权**采数。

### G2-LOCK-3 不翻盘核对

结题须逐条声明八 hash **未改、未重跑、未宣称通过**：  
`f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362` /  
`a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43` /  
`09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d` /  
`831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de` /  
`06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c` /  
`276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69` /  
`d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7` /  
`fd00a06058b239abfe03a52175463a52741718318ce99416a4ce762cf037889b`。

### G2-LOCK-4 票结局（建议）

结局 ∈ {`H5-ABS-RESOLVE-PASS`, `H5-ABS-RESOLVE-FAIL`, `H5-ABS-RESOLVE-INVALID`, `H5-ABS-RESOLVE-STOP`}（或等价）。  
**PASS** = doc-only 菜单决议完备（含主决议 R*）；**≠** 把 VERIFY-FAIL 改 PASS；**≠** Δv-PASS；**≠** 默认可落地。

### G2-LOCK-5 INVALID（任一即整票 · 骨架）

1. 影子 IV / 改 composition.h  
2. 重跑/改写/宣称通过八 hash；把 VERIFY-FAIL 标 PASS  
3. 抬 thr / 降 17/20  
4. 未 LOCK+APPROVED 采数；或 APPROVED 未写死却采数  
5. 立刻开 Δv″；开 M3；解冻 Bv / 重开 S1/R-3/R-2  
6. 产物作 PERF-CI 合格证据  
7. 翻 C1 exact / 静默 0.7511  
8. 主决议落在 R4 否决项  
9. 把本 DRAFT 当已授权采数

## G3 产物（建议）

- `docs/m2_h5_abs_conflict_resolve_result.md`（LOCK+结题后）
- `docs/notion_m2_h5_abs_conflict_resolve_pack.md`（若走结题包体例）
- 同伴：`docs/deepseek_m2_h5_abs_conflict_resolve_review.md`

## G4 ONELINE（模板）

M2-H5-ABS-CONFLICT-RESOLVE | gate=<hash> @ <LOCKED_AT Asia/Taipei> | approved=<who>@<when> | mode=doc-only | ticket=<H5-ABS-RESOLVE-*> | menu=<R1|R2|R3> | 比过smoke灭已证；禁抬thr；八hash不翻盘；禁立刻Δv″；禁翻C1；成功≠VERIFY-PASS改写≠Δv-PASS；≠PERF-CI；不授权采数

## 禁令复述

不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；成功≠采用≠把 FAIL 当 PASS；禁翻盘八 hash（七 + VERIFY `fd00a060…889b`）；结题主用 `H5-ABS-RESOLVE-*`；禁标 `Δv-PASS` / 前序翻盘；PERF-CI HOLD；不开 M3；禁抬 thr / 降 17/20；禁立刻 Δv″；禁翻 C1 exact；默认不开采数；**本 DRAFT ≠ 授权采数**；`H5-DENOM-VERIFY-FAIL` 归档不改写。

— notion ai 骨架 / DeepSeek网页版代拟落盘（M2-H5-ABS-CONFLICT-RESOLVE DRAFT；NOT LOCKED；不授权采数）
