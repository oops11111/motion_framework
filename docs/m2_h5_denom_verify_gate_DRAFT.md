# M2-H5-DENOM-VERIFY 门槛闸门（m2_h5_denom_verify_gate_DRAFT.md）

STATUS=SUPERSEDED_BY_LOCKED（正式 LOCKED 见 `docs/m2_h5_denom_verify_gate_LOCKED.md`）
LOCKED_AT: 见 LOCKED 文件 `2026-10-26 16:09:00 Asia/Taipei`
GATE_BODY_SHA256: 权威 hash 仅以 LOCKED 为准：`fd00a06058b239abfe03a52175463a52741718318ce99416a4ce762cf037889b`（本 DRAFT 不单独作权威 hash）
> **已 LOCK。** 正式正文：`docs/m2_h5_denom_verify_gate_LOCKED.md`；补洞：`docs/notion_m2_h5_denom_verify_gate_revise.md`。本 DRAFT 仅保留骨架溯源，**不以本文件作执行权威**。
> **本 DRAFT/LOCK 正文默认不授权真机采数（直至 APPROVED 显式字段齐）；落地=否；不授权未批的新 calib 执行。** 须 APPROVED_* 非空，且 APPROVED 正文**显式写死**（验证采数=是；落地=否；calib=C1 或冻结常量；禁静默 0.7511）后，方可派执行或真机。GATE_BODY_SHA256 计算约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。
> 触发：M2-H5-DENOM-REVISE = CLOSED **H5-DENOM-PASS**；mode=**doc-only**；result `docs/m2_h5_denom_revise_result.md`；结题包 `docs/notion_m2_h5_denom_revise_pack.md`；下一票 `docs/notion_m2_h5_denom_revise_next.md`。  
> DENOM gate（**不改、不翻盘**）：`d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7` @ 2026-10-26 15:55:00 Asia/Taipei。  
> TRUTH gate（**不改、不翻盘**）：`276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69` @ 2026-10-26 15:43:30 Asia/Taipei。  
> 前序五 FAIL（**不改、不重跑翻盘**）：  
> - PATH-A′ `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362`  
> - PATH-B′ `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`  
> - SEED-CALIB `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`  
> - SEED-ABS `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`  
> - Δv′ `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`  
> M2-PERF-CI HOLD `5b007dd4…282a`（本票 ≠ PERF-CI）。  
> **本票 = 新分母契约下的验证采数 + 另批 calib**；**默认不是**落地启用；**不是** Δv″；**不是**抬 thr；**不是**翻盘六 hash / 本 DENOM。  
> **声明**：`H5-DENOM-PASS`（doc-only）≠ 已落地采用；本 DRAFT ≠ 授权采数。

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

**本票结论不解冻 B_v / 不重开 R-2 / 不重开 S1 / R-3；不改 composition.h。**  
**本票成功 ≠ 已落地采用 ≠ Δv-PASS ≠ 六 hash 翻盘 ≠ 采用；结题标签主用 `H5-DENOM-VERIFY-*`（或等价）。**  
**已 LOCK；权威见 LOCKED。** DRAFT/LOCK 正文默认不授权真机采数（直至 APPROVED 显式字段齐）；落地=否；不授权未批的新 calib 执行；APPROVED_* 空。

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
  - DENOM gate `d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7`  
  禁止把本票产物标为 `Δv-PASS` / 前序 `SEED-*-PASS` / `SEED-TRUTH-PASS` / 重写 `H5-DENOM-PASS` 为已落地。
- **本票 ≠ PERF-CI**；产物不得作 PERF-CI 合格证据；PERF-CI 仍 HOLD。
- 不开 M3 真机（本票仅单机验证范围，若 APPROVED）。
- **禁立刻开 Δv″**（须本票验证过比+smoke 可齐后再另开）。
- **禁抬 thr**；**禁**降 17/20 伪装 PASS。
- **禁静默沿用 calib=0.7511**（旧 2F）。
- 未 LOCKED+APPROVED 禁止采数 / 禁止分母落地；即便 APPROVED，**采数与落地与 calib 范围须正文写死**；本 DRAFT 默认仍不开采数、不落地。

## G1 票性质 · 验证采数 + 另批 calib（骨架）

### 继承契约（只读；KEEP）

自 DENOM LOCK 继承，本票**不得改定义**：

| 项 | KEEP |
|---|---|
| **`v_denom ≡ v_pre`** | 二次 LS · w=5 · 同 QPC · 同 `t_int=min(t_comp,t_last_WGC)`；单位 DIP/s |
| 比式 | `r_i=\|v_seed/v_denom\|` |
| 旧 2F | 仅辅证；**不作**门控分母 |
| 比–绝对同源 | 比分母与 smoke `v_pre` 同一定义 |
| H5 比门槛 | `med_r ∈ [0.8,1.2]` 且无 `r_i≥2.0` |
| smoke | ≥**17/20**；`thr_i=max(20,0.05·\|v_ref,i\|)`；w=5；**禁抬 thr / 降 17/20** |

### 须在 LOCK 前写死（补洞清单）

1. **验证采数范围**：目的、格表（推荐 t060 fwd+rev）、会话数（≥3 独立进程）、**单机**声明。  
2. **落地字段**：默认 **落地=否**；若另开落地须 APPROVED 显式另字段写死边界（本票骨架默认不落地）。  
3. **calib 新常量或标定式**：推荐 `median(v_denom/v_hat)` 或写死常量表；**禁静默 0.7511**。  
4. **路径/prereg**：N/order/ε/`t_int`/是否 P-VS（默认 KEEP 或显式另批）。  
5. **仍禁改 KEEP**：composition.h；thr；17/20；六+DENOM hash；禁影子 IV。  
6. **结题标签**：`H5-DENOM-VERIFY-PASS|FAIL|INVALID|STOP`（或等价）；明确 ≠ Δv-PASS ≠ 六 hash 翻盘 ≠ 默认可落地。  
7. **产物前缀**：`m2_h5_denom_verify_*` only（不得冒充 path_a/path_b/calib/abs/delta_v/truth/denom_revise 翻盘包）。

### APPROVED 须显式字段（LOCK 后拍板）

| 字段 | 须填 |
|---|---|
| 是否授权验证采数 | **是**（本票目标）；目的、格表、会话数、单机 |
| 是否授权落地 | **否**（默认）或另字段显式写死 |
| calib | **新常量**或 **标定式**（禁静默 0.7511） |
| KEEP v_denom≡v_pre | 是 |
| 禁抬 thr / 降 17/20 | 是 |
| 六+本 DENOM hash 不翻盘 | 是 |
| PERF-CI | HOLD |
| 禁立刻 Δv″ | 是 |

### 明确否决

立刻 Δv″；默认可落地；静默沿用旧 calib=0.7511；翻盘六 hash / 本 DENOM；抬 thr；降 17/20；未 LOCK/未 APPROVED 显式字段即采数或落地；改 composition.h；影子 IV；开 M3；解冻 Bv；把 doc-only `H5-DENOM-PASS` 写成已落地采用；把本票产物标 `Δv-PASS`。

## G2 主判据（骨架；LOCK 时补齐）

### G2-LOCK-1 契约继承完备

结题须确认：继承 `v_denom≡v_pre` 与比–绝对同源；门槛 KEEP；禁抬 thr；calib 另批已显式；六+DENOM 不翻盘逐条。缺任一项 → INVALID 或退回补洞。

### G2-LOCK-2 执行边界

- LOCK+APPROVED 前：**禁止**采数与分母落地。  
- 勾选/决议 ≠ 自动执行；执行范围以 APPROVED 正文为准。  
- **本 DRAFT 已 SUPERSEDED**：权威见 LOCKED；LOCK+APPROVED 前仍**禁止**采数与分母落地。

### G2-LOCK-3 不翻盘核对

结题须逐条声明七 hash **未改、未重跑、未宣称通过**：  
`f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362` /  
`a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43` /  
`09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d` /  
`831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de` /  
`06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c` /  
`276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69` /  
`d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7`。

### G2-LOCK-4 票结局（建议）

结局 ∈ {`H5-DENOM-VERIFY-PASS`, `H5-DENOM-VERIFY-FAIL`, `H5-DENOM-VERIFY-INVALID`, `H5-DENOM-VERIFY-STOP`}（或等价）。  
**PASS ≠ 已落地采用 ≠ Δv-PASS ≠ 六 hash 翻盘**（除非 APPROVED 显式落地=是且另满足落地条件）。

### G2-LOCK-5 INVALID（任一即整票 · 骨架）

1. 影子 IV / 改 composition.h  
2. 重跑/改写/宣称通过六+DENOM hash；产物标翻盘 PASS  
3. 抬 thr / 降 17/20  
4. 未 LOCK+APPROVED 采数或分母落地；或 APPROVED 未写死声明字段却采数/落地  
5. 立刻开 Δv″；开 M3；解冻 Bv / 重开 S1/R-3/R-2  
6. 产物作 PERF-CI 合格证据  
7. 静默沿用 calib=0.7511 宣称新分母比 PASS；比仍用旧 2F 分母却宣称新契约 PASS  
8. 把 doc-only `H5-DENOM-PASS` / 本票 PASS 写成已落地采用（无显式落地授权时）  
9. 默认可落地；把本 DRAFT 当已授权采数

## G3 产物（建议）

- `docs/m2_h5_denom_verify_result.md`（LOCK+结题后）
- `docs/notion_m2_h5_denom_verify_pack.md`（若走结题包体例）
- `artifacts/m2_h5_denom_verify_*`（若有授权采数；**不得**冒充翻盘包）

## G4 ONELINE（模板）

M2-H5-DENOM-VERIFY | gate=<hash> @ <LOCKED_AT Asia/Taipei> | approved=<who>@<when> | mode=<verify|…> | ticket=<H5-DENOM-VERIFY-*> | KEEP v_denom≡v_pre；calib=<新常量或标定式>；禁静默0.7511；验证采数=<是/否>；落地=<否/…>；六+DENOM不翻盘；禁抬thr；成功≠落地采用≠Δv-PASS；≠PERF-CI；禁立刻Δv″；单机

## 禁令复述

不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；成功≠采用≠落地采用；禁翻盘七 hash（六 + DENOM `d891df9f…33c7`）；结题主用 `H5-DENOM-VERIFY-*`；禁标 `Δv-PASS` / 前序翻盘；PERF-CI HOLD；不开 M3；禁抬 thr / 降 17/20；禁立刻 Δv″；**KEEP v_denom≡v_pre**；calib 须显式另批；禁静默 0.7511；默认不开采数、不落地；**本 DRAFT ≠ 授权采数 ≠ 授权落地**；`H5-DENOM-PASS`≠已落地采用。

— notion ai（M2-H5-DENOM-VERIFY DRAFT；SUPERSEDED_BY_LOCKED；权威见 LOCKED `fd00a060…889b` @ 2026-10-26 16:09:00 Asia/Taipei；不单独授权采数；落地=否）
