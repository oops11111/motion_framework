# M2-SEED-TRUTH-CONFLICT 结题中文包（notion_m2_seed_truth_conflict_pack.md）

> DECISION_AT=2026-10-26 15:49:00 Asia/Taipei  
> 权威结题：`docs/m2_seed_truth_conflict_result.md`（WRITTEN_AT=2026-10-26 15:49:00 Asia/Taipei）  
> 闸门 LOCKED **勿改 hash**：`docs/m2_seed_truth_conflict_gate_LOCKED.md` GATE_BODY_SHA256=`276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69` @ 2026-10-26 15:43:30 Asia/Taipei（结题复算 **MATCH**）  
> APPROVED：`docs/m2_seed_truth_conflict_approved.md` — pidan@2026-10-26 15:47:34 Australia/Perth（**未**显式授权采数 → **mode=offline-resolve**）  
> 同伴：`docs/deepseek_m2_seed_truth_conflict_review.md`（推荐主决议 M1）  
> 前序 PATH-A′ FAIL `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362`：**未重跑、未翻盘**  
> 前序 PATH-B′ FAIL `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`：**未重跑、未翻盘**  
> 前序 SEED-CALIB FAIL `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`：**未重跑、未翻盘**  
> 前序 SEED-ABS FAIL `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`：**未重跑、未翻盘**  
> 前序 Δv′ FAIL `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`：**未重跑、未翻盘**  
> **本 TRUTH gate `276b97f0…4f69`：结题后不改 body/hash**  
> 旁路：`docs/side_parked_hang_degrade.md`（不挡、本票也不清）

## 结论

- **票级 = CLOSED**；标签 **`SEED-TRUTH-PASS`**；主决议勾选 **恰好 M1**；mode=**offline-resolve**。
- G4 ONELINE（原文照抄 result）：  
  `M2-SEED-TRUTH-CONFLICT | gate=276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69 @ 2026-10-26 15:43:30 Asia/Taipei | approved=pidan@2026-10-26 15:47:34 Australia/Perth | mode=offline-resolve | menu=M1 | ticket=SEED-TRUTH-PASS | next=M2-H5-DENOM-REVISE | 成功≠已改H5≠采用≠Δv-PASS；不改composition.h；禁影子IV；≠PERF-CI；五FAIL不翻盘；本票未执行改分母/抬thr；未采数`
- **`SEED-TRUTH-PASS` ≠ 已改 H5 ≠ 采用 ≠ Δv-PASS ≠ 授权本票执行改分母**；执行须另票 **`M2-H5-DENOM-REVISE`**。
- **未选 M4**：approved / 文件无强制 STOP；证据齐且可决议 → 不归档停线。
- 菜单否决 **M2 / M3 / M5** 作结题主决议。
- 成功 ≠ 采用 ≠ Δv-PASS ≠ 前序 SEED-*-PASS 翻盘；不解冻 Bv；不重开 S1/R-3/R-2；`composition.h` `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` **本票未触**；本闸门 body/hash **未改**。
- **本票 ≠ PERF-CI**；PERF-CI 仍 HOLD `5b007dd4…282a`；不开 M3；禁立刻 Δv″；禁影子 IV。
- **未采数**；未改探针；本票未执行改分母 / 抬 thr / 改 smoke 参照。

## 授权核验

| 项 | 值 |
|---|---|
| approved 文件 | 存在且刚批：`docs/m2_seed_truth_conflict_approved.md` |
| APPROVED_BY / AT | pidan / 2026-10-26 15:47:34 Australia/Perth（原文） |
| 采数授权 | **未**显式授权 → offline-resolve |
| gate hash | `276b97f0…4f69` @ 15:43:30 Asia/Taipei；复算 **MATCH**；**勿改** |

## 冲突根因（写死）

**2F 比真值（H5 分母=`v_screen`）与绝对 `v_pre` / t060 smoke 门不一致**——KEEP calib + KEEP H5@2F 下种子钉 ≈`v_screen`，绝对 `v_pre` 系统性高出约 **50+ DIP/s**；比与 smoke **不可同票 PASS**。

| 票 | H5 比 | smoke | 结局 |
|---|---|---|---|
| PATH-B′ `a1161c77…3a43` | med_r=**0.9465 / 0.8055 / 0.8062** 比过 | fwd 0/20×3；rev 0/20 | SEED-PATHB-FAIL；首次 TRUTH-CONFLICT |
| PATH-A′ `f17871f1…2362` | s1 max_r=**6.5997** 比 FAIL；s2/s3 med_r=**0.8527 / 0.8649** 比过 | 0/3 格 PASS | SEED-PATHA-PRIME-FAIL；保持 TRUTH-CONFLICT |

- **平滑臂已耗尽**（A + A′ 均 CLOSED FAIL）。
- **结构风险下 FAIL 合法**（对照 baseP ∧ IVel0F 健全；例 A′ s3 Δv=75.577 thr=20）。
- **本票未执行**改分母 / 抬 thr。

## 主决议与菜单

| 菜单 | 决断 | 一句 |
|---|---|---|
| **M1** | **勾选** | 对准根因：另票定义新分母/估窗使比–绝对同源 |
| M2 | 否决 | 易把绝对门弱化成「跟 2F 比」 |
| M3 | 否决 | KEEP H5@2F 下重复撞墙；平滑臂已证无效 |
| M4 | 未选 | 无强制 STOP；次选保留 |
| M5 | 否决 | 不收敛 |

下一票占位：**`M2-H5-DENOM-REVISE`**。

## 事实表

| 项 | 值 |
|---|---|
| TICKET / STATUS | `SEED-TRUTH-PASS` / **CLOSED** |
| mode | **offline-resolve** |
| menu | **M1** |
| gate | `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69` @ 2026-10-26 15:43:30 Asia/Taipei |
| approved | pidan@2026-10-26 15:47:34 Australia/Perth |
| A′ s1 max_r | **6.5997** |
| A′ s2/s3 med_r | **0.8527 / 0.8649** |
| 平滑臂 | **已耗尽** |
| 采数 | **未采数** |
| 改分母/抬 thr | **本票未执行** |
| composition.h | 未触 `a55d4952…e418` |
| PERF-CI | HOLD `5b007dd4…282a` |

## 五 FAIL 不翻盘（全文）

- PATH-A′ `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362`：**不改、不重跑、不宣称通过**
- PATH-B′ `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`：**不改、不重跑、不宣称通过**
- SEED-CALIB `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`：**不改、不重跑、不宣称通过**
- SEED-ABS `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`：**不改、不重跑、不宣称通过**
- Δv′ `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`：**不改、不重跑、不宣称通过**
- 本 TRUTH gate `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69`：**不改 body/hash**（复算 MATCH）
- 结题标签只用 `SEED-TRUTH-*`；**禁止**写 `Δv-PASS` / 前序 `SEED-*-PASS` 翻盘

## 产物清单

- `docs/m2_seed_truth_conflict_result.md`（权威结题）
- `docs/notion_m2_seed_truth_conflict_pack.md`（本包）
- `docs/notion_m2_seed_truth_conflict_next.md`
- `docs/m2_h5_denom_revise_gate_DRAFT.md`（下一票骨架 · NOT LOCKED）
- 引用只读：`docs/m2_wgc_seed_path_a_prime_result.md` / `docs/m2_wgc_seed_path_b_prime_result.md` / `docs/deepseek_m2_seed_truth_conflict_review.md` / approved / LOCKED

## 禁令复述

- 不重开 S1 / R-3 / R-2；不解冻 Bv；不改 composition.h；禁影子 IV
- 成功 ≠ 采用 ≠ 已改 H5 ≠ Δv-PASS
- **不改/不重跑**五 FAIL 全文 hash；不改本 TRUTH gate
- PERF-CI 不降级；不开 M3；禁立刻 Δv″
- 本票未执行改分母 / 抬 thr / 改 smoke；未采数
- 勾选 M1 ≠ 本票执行改分母

## 挂账 / 残差

| 项 | 处置 |
|---|---|
| M2-SEED-TRUTH-CONFLICT（本票） | **CLOSED SEED-TRUTH-PASS**；menu=M1；LOCKED 归档不改 |
| **M2-H5-DENOM-REVISE** | **下一主线**（另票定义新分母；见 next） |
| M2-Δv″ | **继续挂账**；禁立刻开 |
| M2-PERF-CI | 继续 HOLD |
| M3 | 不开真机 |
| 五 FAIL 归档 | 保持；不翻盘 |

## 下一票决断

**选 A = M2-H5-DENOM-REVISE**。  
骨架 DRAFT：`docs/m2_h5_denom_revise_gate_DRAFT.md`（**NOT LOCKED**；本 next **不**授权采数）。详见 `docs/notion_m2_seed_truth_conflict_next.md`。

— notion ai（M2-SEED-TRUTH-CONFLICT 结题；ticket=CLOSED SEED-TRUTH-PASS；menu=M1；mode=offline-resolve；下一票=M2-H5-DENOM-REVISE）
