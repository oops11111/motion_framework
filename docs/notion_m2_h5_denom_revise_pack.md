# M2-H5-DENOM-REVISE 结题中文包（notion_m2_h5_denom_revise_pack.md）

> DECISION_AT=2026-10-26 16:02:00 Asia/Taipei  
> 权威结题：`docs/m2_h5_denom_revise_result.md`（WRITTEN_AT=2026-10-26 16:02:00 Asia/Taipei）  
> 闸门 LOCKED **勿改 hash**：`docs/m2_h5_denom_revise_gate_LOCKED.md` GATE_BODY_SHA256=`d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7` @ 2026-10-26 15:55:00 Asia/Taipei（结题复算 **MATCH**）  
> APPROVED：`docs/m2_h5_denom_revise_approved.md` — pidan@2026-10-26 16:00:17 Australia/Perth（mode=**doc-only**；验证采数=**否**；落地=**否**；calib=**不适用**）  
> 同伴：`docs/deepseek_m2_h5_denom_revise_review.md`（推荐 **v_denom≡v_pre**；废旧 2F 作门控分母；calib 须另批）  
> 补洞：`docs/notion_m2_h5_denom_revise_gate_revise.md`  
> 前序 PATH-A′ FAIL `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362`：**未重跑、未翻盘**  
> 前序 PATH-B′ FAIL `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`：**未重跑、未翻盘**  
> 前序 SEED-CALIB FAIL `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`：**未重跑、未翻盘**  
> 前序 SEED-ABS FAIL `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`：**未重跑、未翻盘**  
> 前序 Δv′ FAIL `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`：**未重跑、未翻盘**  
> 前序 TRUTH gate `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69`：**未重跑、未翻盘**  
> **本 DENOM gate `d891df9f…33c7`：结题后不改 body/hash**  
> 旁路：`docs/side_parked_hang_degrade.md`（不挡、本票也不清）

## 结论

- **票级 = CLOSED**；标签 **`H5-DENOM-PASS`**；mode=**doc-only**。
- G4 ONELINE（原文照抄 result）：  
  `M2-H5-DENOM-REVISE | gate=d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7 @ 2026-10-26 15:55:00 Asia/Taipei | approved=pidan@2026-10-26 16:00:17 Australia/Perth | mode=doc-only | ticket=H5-DENOM-PASS | v_denom≡v_pre | 成功≠已落地采用≠Δv-PASS；未采数；未落地；calib未批；禁静默0.7511；六hash不翻盘；≠PERF-CI；禁立刻Δv″`
- **`H5-DENOM-PASS` ≠ 已落地采用 ≠ Δv-PASS ≠ 授权本票采数/落地/新 calib**；执行验证须另票 **`M2-H5-DENOM-VERIFY`**。
- 契约写死：**`v_denom ≡ v_pre`**（二次 LS · w=5 · 同 QPC · 同 `t_int`）；旧 2F `v_screen` **不作**门控分母；比–绝对同源；门槛 KEEP；禁抬 thr / 降 17/20；calib 须另批；禁静默 0.7511。
- 成功 ≠ 已落地采用 ≠ Δv-PASS ≠ 前序 SEED-*-PASS / TRUTH 翻盘；不解冻 Bv；不重开 S1/R-3/R-2；`composition.h` `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` **本票未触**；本闸门 body/hash **未改**。
- **本票 ≠ PERF-CI**；PERF-CI 仍 HOLD `5b007dd4…282a`；不开 M3；禁立刻 Δv″；禁影子 IV。
- **未采数**；**未落地**；**calib 未批**；未改探针。

## 授权核验

| 项 | 值 |
|---|---|
| approved 文件 | 存在：`docs/m2_h5_denom_revise_approved.md` |
| APPROVED_BY / AT | pidan / 2026-10-26 16:00:17 Australia/Perth（原文） |
| mode | **doc-only** |
| 验证采数 | **否** |
| 落地 | **否** |
| calib | **不适用** |
| gate hash | `d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7` @ 15:55:00 Asia/Taipei；复算 **MATCH**；**勿改** |

## 契约摘要（写死）

### 新分母

**`v_denom ≡ v_pre`**：打断前 WGC `(t_k,x_k)` 二次 LS · w=5 · 同 QPC · 估点 `t_int=min(t_comp,t_last_WGC)`；单位 DIP/s；比式 `r_i=|v_seed/v_denom|`；旧 2F 仅辅证。

### 比–绝对同源

比分母与 t060 smoke 的 `v_pre` **同一定义**；禁止「比钉旧 2F、绝对钉另一套」。

### 门槛 KEEP

| 门 | KEEP |
|---|---|
| H5 比 | `med_r ∈ [0.8,1.2]` 且无 `r_i≥2.0` |
| smoke | ≥**17/20**；`thr_i=max(20,0.05·|v_ref,i|)`；w=5 |
| thr / 17/20 | **禁抬 / 禁降** |
| calib | 定义阶段不落地；验证/落地须**另批**；**禁静默 0.7511** |

## 事实表

| 项 | 值 |
|---|---|
| TICKET / STATUS | `H5-DENOM-PASS` / **CLOSED** |
| mode | **doc-only** |
| gate | `d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7` @ 2026-10-26 15:55:00 Asia/Taipei |
| approved | pidan@2026-10-26 16:00:17 Australia/Perth |
| v_denom | **≡ v_pre** |
| 采数 | **未采数** |
| 落地 | **未落地** |
| calib | **未批**（禁静默 0.7511） |
| composition.h | 未触 `a55d4952…e418` |
| PERF-CI | HOLD `5b007dd4…282a` |

## 六 hash 不翻盘（全文）

- PATH-A′ `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362`：**不改、不重跑、不宣称通过**
- PATH-B′ `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`：**不改、不重跑、不宣称通过**
- SEED-CALIB `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`：**不改、不重跑、不宣称通过**
- SEED-ABS `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`：**不改、不重跑、不宣称通过**
- Δv′ `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`：**不改、不重跑、不宣称通过**
- TRUTH `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69`：**不改、不重跑、不宣称通过**
- 本 DENOM gate `d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7`：**不改 body/hash**（复算 MATCH）
- 结题标签只用 `H5-DENOM-*`；**禁止**写 `Δv-PASS` / 前序 `SEED-*-PASS` / `SEED-TRUTH-PASS` 翻盘

## 产物清单

- `docs/m2_h5_denom_revise_result.md`（权威结题）
- `docs/notion_m2_h5_denom_revise_pack.md`（本包）
- `docs/notion_m2_h5_denom_revise_next.md`
- `docs/m2_h5_denom_verify_gate_DRAFT.md`（下一票骨架 · NOT LOCKED）
- 引用只读：`docs/deepseek_m2_h5_denom_revise_review.md` / `docs/notion_m2_h5_denom_revise_gate_revise.md` / approved / LOCKED / `docs/m2_seed_truth_conflict_result.md`

## 禁令复述

- 不重开 S1 / R-3 / R-2；不解冻 Bv；不改 composition.h；禁影子 IV
- 成功 ≠ 已落地采用 ≠ Δv-PASS ≠ 已改 H5（本票 doc-only）
- **不改/不重跑**六 hash 全文；不改本 DENOM gate
- PERF-CI 不降级；不开 M3；禁立刻 Δv″
- 本票未采数；未落地；calib 未批；禁静默 0.7511
- 禁抬 thr / 降 17/20；KEEP `v_denom≡v_pre`
- doc-only PASS ≠ 默认可落地

## 挂账 / 残差

| 项 | 处置 |
|---|---|
| M2-H5-DENOM-REVISE（本票） | **CLOSED H5-DENOM-PASS**；mode=doc-only；LOCKED 归档不改 |
| **M2-H5-DENOM-VERIFY** | **下一主线**（验证采数 + 新分母下另批 calib；见 next） |
| M2-Δv″ | **继续挂账**；须验证过比+smoke 后再开；禁立刻 |
| M2-PERF-CI | 继续 HOLD |
| M3 | 不开真机 |
| 六 hash 归档 | 保持；不翻盘 |
| 本 DENOM hash | 归档；下一票亦不翻盘 |

## 下一票决断

**选 A = M2-H5-DENOM-VERIFY**。  
骨架 DRAFT：`docs/m2_h5_denom_verify_gate_DRAFT.md`（**NOT LOCKED**；本 next **不**授权采数）。详见 `docs/notion_m2_h5_denom_revise_next.md`。

— notion ai（M2-H5-DENOM-REVISE 结题；ticket=CLOSED H5-DENOM-PASS；mode=doc-only；下一票=M2-H5-DENOM-VERIFY）
