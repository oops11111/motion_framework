# M2-SEED-TRUTH-CONFLICT 结题结果

STATUS: CLOSED
TICKET: SEED-TRUTH-PASS
MENU: M1（授权另票改 H5 分母 / 比–绝对同源契约）
MODE: offline-resolve
WRITTEN_AT: 2026-10-26 15:49:00 Asia/Taipei
GATE: docs/m2_seed_truth_conflict_gate_LOCKED.md
GATE_BODY_SHA256: 276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69
LOCKED_AT: 2026-10-26 15:43:30 Asia/Taipei
APPROVED: docs/m2_seed_truth_conflict_approved.md — pidan@2026-10-26 15:47:34 Australia/Perth
APPROVED_NOTE: widget reply「APPROVED」（t257s1172）；主决议倾向 M1；次选 M4；**未**显式授权真机采数 → mode=offline-resolve

## 授权核验摘要

| 项 | 结果 |
|---|---|
| approved 文件 | 存在：`docs/m2_seed_truth_conflict_approved.md` |
| APPROVED_BY / AT | pidan / **2026-10-26 15:47:34 Australia/Perth**（原文照抄；同 UTC+8） |
| 显式授权采数？ | **否**（APPROVED_NOTE：**未**显式授权真机采数） |
| mode | **offline-resolve**（仅写结题；禁派真机采数） |
| GATE_BODY_SHA256 | 期望 `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69` @ LOCKED_AT=2026-10-26 15:43:30 Asia/Taipei |
| 结题复算 hash | **MATCH**（排除 GATE_BODY_SHA256 行后全文 SHA-256）；**未改** LOCKED body/hash |

## G4 ONELINE

M2-SEED-TRUTH-CONFLICT | gate=276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69 @ 2026-10-26 15:43:30 Asia/Taipei | approved=pidan@2026-10-26 15:47:34 Australia/Perth | mode=offline-resolve | menu=M1 | ticket=SEED-TRUTH-PASS | next=M2-H5-DENOM-REVISE | 成功≠已改H5≠采用≠Δv-PASS；不改composition.h；禁影子IV；≠PERF-CI；五FAIL不翻盘；本票未执行改分母/抬thr；未采数

## 主决议

- **勾选恰好 M1**（按 LOCK 主决议倾向 + DeepSeek 推荐）。
- **未选 M4**：approved / 文件**无**强制 STOP；证据已齐且可决议出口 → 不归档停线。
- 勾选 M1 → 票标签 **`SEED-TRUTH-PASS`**。
- **明确声明**：`SEED-TRUTH-PASS` ≠ 已改 H5 ≠ 采用 ≠ Δv-PASS ≠ 授权本票执行改分母；执行须另票 `M2-H5-DENOM-REVISE` LOCK+APPROVED。
- 菜单否决（不作结题主决议）：**M2 / M3 / M5**（与 LOCK 二选一规则一致）。

## 冲突根因完备（G2-LOCK-1）

**2F 比真值（H5 分母=`v_screen`=打断前末两帧 WGC）与绝对 `v_pre` / t060 smoke 门不一致**——在 KEEP calib + KEEP H5@2F 下，种子被钉在 ≈`v_screen`，而绝对连续测量 `v_pre` 系统性高出约 **50+ DIP/s**；比与 smoke **不可同票 PASS**。

### 双路径证据（只引用已归档 result；本票未采数）

| 票 | 路径 | H5 比 | t060 smoke | 对照 | 结局 |
|---|---|---|---|---|---|
| **PATH-B′** gate `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43` | V2-clamp；P-VS=**OFF** | 三会话比过：med_r=**0.9465 / 0.8055 / 0.8062** | fwd **0/20×3**；rev **0/20**（含 1 SKIP） | baseP ∧ IVel0F ×3 | `SEED-PATHB-FAIL`；**首次** TRUTH-CONFLICT |
| **PATH-A′** gate `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362` | V2-clamp+P-VS α_v=**0.30** | s1 med_r=0.9289 **max_r=6.5997** any_ri≥2 → **比 FAIL**（cross-dir EMA）；s2/s3 med_r=**0.8527 / 0.8649** 比过 | fwd 1/0(SKIP)/0；rev **0/20×3**；0/3 格 PASS | baseP ∧ IVel0F ×3 有效 | `SEED-PATHA-PRIME-FAIL`；**保持** TRUTH-CONFLICT |

补充写死：

- A′ s1 **max_r=6.5997**（跨向 EMA 尖刺）；s2/s3 **med_r=0.8527 / 0.8649**（贴窗下沿）。
- 结构风险例：A′ s3 Δv=**75.577**（thr=20）；B′/CALIB 系谱均见 `|ivel−v_pre|≈50+`。
- **平滑臂已耗尽**：预登记 A（P-VS=OFF）与 A′（P-VS=ON α_v=0.30）均 CLOSED FAIL。
- **对照健全** → FAIL ≠ 测法崩坏；**结构风险下 FAIL 合法**。
- **本票未执行**改 H5 分母 / 抬 thr / 改 smoke 参照 / 改 composition.h。

## 处置决议（G2-LOCK-2）

| 菜单 | 本票 | 说明 |
|---|---|---|
| **M1** | **勾选（主决议）** | 授权**另票**改 H5 分母 / 比–绝对同源；下一条占位 **`M2-H5-DENOM-REVISE`** |
| M2 | 否决 | 易弱化绝对门；不推荐 |
| M3 | 否决 | 平滑臂已耗尽；KEEP H5@2F 下重复撞墙 |
| M4 | 未选 | 无强制 STOP；次选保留但不作结题勾选 |
| M5 | 否决 | 不收敛 |

## 五 FAIL 不翻盘（G2-LOCK-3 · 全文）

结题逐条声明：以下五 FAIL gate hash **未改、未重跑、未宣称通过**：

1. PATH-A′ FAIL `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362`
2. PATH-B′ FAIL `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`
3. SEED-CALIB FAIL `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`
4. SEED-ABS FAIL `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`
5. Δv′ FAIL `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`

本 TRUTH gate `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69`：**未改 body/hash**（结题复算 MATCH）。

## 未采数声明

- APPROVED **未**显式授权采数范围 → **mode=offline-resolve**。
- 本票**未**派真机 / workbuddy 采数；**未**改探针；**未**改 `composition.h`。
- 无 `artifacts/m2_seed_truth_*` 新采数包（本票不依赖采数）。

## 禁令遵守

- 未重开 S1/R-3/R-2；未解冻 Bv；禁影子 IV；未开 M3；禁立刻 Δv″
- 未执行改分母 / 抬 thr / 降 17/20 / 改 smoke 参照
- PERF-CI 仍 HOLD `5b007dd4…282a`；本票 ≠ PERF-CI
- `composition.h` 保持 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（本票未触）
- 结题标签仅 `SEED-TRUTH-PASS`；禁止写 `Δv-PASS` / 前序 `SEED-*-PASS` 翻盘

## 下一票占位

**`M2-H5-DENOM-REVISE`**（DRAFT：`docs/m2_h5_denom_revise_gate_DRAFT.md`；**NOT LOCKED**；本 next **不**授权采数）。

## 产物清单

- `docs/m2_seed_truth_conflict_result.md`（本文件 · 权威结题）
- `docs/notion_m2_seed_truth_conflict_pack.md`
- `docs/notion_m2_seed_truth_conflict_next.md`
- `docs/m2_h5_denom_revise_gate_DRAFT.md`（下一票骨架 · NOT LOCKED）

— notion ai（M2-SEED-TRUTH-CONFLICT CLOSED SEED-TRUTH-PASS；menu=M1；mode=offline-resolve；未采数）
