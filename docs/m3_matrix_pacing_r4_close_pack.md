# M3-MATRIX-PACING r4 结题中文包（m3_matrix_pacing_r4_close_pack.md）

> DECISION_AT=2026-11-06 11:58:00 AWST（Australia/Perth（AWST，UTC+8））  
> 权威结题：`docs/m3_matrix_pacing_r4_result.md`  
> 闸门：gate=**MATCH** `81fc74ebba270feb818646cd5f5c8c90fd2d3812c3447c642ed126413108934c`；LOCKED_AT=2026-11-06 11:27:50 AWST  
> APPROVED：pidan @ 2026-11-06 11:36:53 AWST（`docs/m3_matrix_pacing_r4_approved.md`）  
> 前序：**禁回改** — MATRIX CLOSED `RIVE-MATRIX-FAIL`；PACING **r1/r2/r3** CLOSED `RIVE-MATRIX-PACING-FAIL`  
> 分支 tip=`e94600332307e4482cf0ff91d4e16d3b60deefc5`；results_commit=`6b7a290864ef58ff8b908f8d46532e449c8e4530` @ **`claude/m3-matrix`**  
> composition.h PRE=POST CRLF=`33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97`；LF=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`；**未改**

## 结论

- **票级 = CLOSED**；标签 **`RIVE-MATRIX-PACING-PASS`**（**r4**）。
- **METRICS = METRICS_PASS**，**全判据绿**：`pacing_ok=1` `hitch_ok=1` `size_ok=1` `budget_ok=1` `stability_ok=1`；sessions=3×RECORDED；`scale_changes=4`。
- **出帧 / GPU 旁证**：代码 `69094ee`（vblank 后再等 **0.3·T**）；probe v4；pacing_source=vblank；`pacing_offset_fraction=0.30`；三会话 MATCH；**gpu_render_p95_frac_T@180Hz≈0.040**（远低于 0.35·T；对照 r3≈1.05–1.08）— **可引用**，**不**升 adopt。
- **与 r1 / r2 / r3 对偶（禁洗票）**：  
  - r1 = pacing✓ / budget✗  
  - r2 = pacing✗ / budget✓  
  - r3 = pacing✓ / budget✗（失败在 render）  
  - r4 = **全绿**（分立 tip `69094ee` / results `m3/results_matrix_pacing_r4/`）  
  **前三票保持 FAIL**；禁止用 r4 PASS 洗前序或合并改判。
- **是否采用 = 不自动采用**（Success≠adopt；**不合 main**；有条件采用须 **pidan 另拍板**）。
- **PERF-CI HOLD**；composition.h **未触**。
- **RENDER-BUDGET / BUDGET DRAFT**：建议 **挂账或 SUPERSEDED 候选**（r4 budget_ok=1 已实证消掉 render 残差）；**开放给 pidan**；本 pack **不**自动 LOCK / 不改写 DRAFT 正文。

ONELINE（结题）：  
`M3-MATRIX-PACING r4 | gate=81fc74ebba270feb818646cd5f5c8c90fd2d3812c3447c642ed126413108934c @ 2026-11-06 11:27:50 AWST | approved=pidan@2026-11-06 11:36:53 AWST | host=DESKTOP-IJI1KJG | topology=75Hz@100%+180Hz@150% | sessions=3 | pacing_ok=1 | pacing_p95_ms_primary≈13.38–13.39 | pacing_p95_ms_external≈5.63–5.69 | hitch_ok=1 | budget_ok=1 | size_ok=1 | stability_ok=1 | ticket=RIVE-MATRIX-PACING-PASS | dir=m3/results_matrix_pacing_r4/ | probe=M3-MATRIX-PACING v4 | pacing_source=vblank | pacing_offset_fraction=0.30 | gpu_render_p95_frac_T(180Hz)≈0.040 | tip=e94600332307e4482cf0ff91d4e16d3b60deefc5 | results=6b7a290864ef58ff8b908f8d46532e449c8e4530 | code=69094ee | Success≠adopt | LOCK≠采数 | 不改composition.h；不合main；PERF-CI HOLD`

## 口径表

| 选项 | 决断 | 理由 |
|---|---|---|
| **RIVE-MATRIX-PACING-PASS** | **是** | 全判据绿（含 budget_ok=1） |
| 有条件 PASS / 自动 adopt | **否** | Success≠adopt；有条件采用须 pidan |
| INCONCLUSIVE / INVALID | **否** | 3×RECORDED；gate MATCH；approved 在树；probe 路径 MATCH |
| RIVE-MATRIX-PACING-FAIL | **否** | 无硬 FAIL 字段 |
| 采用 tip / 合 main | **否** | Success≠adopt；PERF-CI HOLD |
| 回改 r1 / r2 / r3 / MATRIX | **否** | 硬禁 |
| 合并 r1+r2+r3 改 PASS / 洗票 | **否** | 禁；r4 分立 tip |
| 自动 LOCK/SUPERSEDE DRAFT | **否** | 仅建议候选；开放 pidan |

### 可引用 vs 不采用

| 可引用 | 不采用 / 禁止 |
|---|---|
| 全判据绿 @ offset=0.3·T；gpu_render≈0.040 | 整 tip 合 main / ship |
| results_commit `6b7a290864ef58ff8b908f8d46532e449c8e4530` + `m3/results_matrix_pacing_r4/` | 「r1–r3 其实已过 / 四轮拼 PASS」叙事 |
| 与 r1–r3 字段对偶对照（r3 render 残差被本 tip 消掉） | 翻 CLOSED FAIL；静默改前序标签 |
| WP（offset=0.3·T）作**有条件采用候选** | 本 pack 授权新采数；自动 LOCK 下票；解冻 PERF-CI |

## lab / 实现备注

1. 代码 **69094ee**：vblank 后再等 **0.3·T**。  
2. probe v4；pacing_source=vblank；`pacing_offset_fraction=0.30`；三会话 MATCH。  
3. **gpu_render_p95_frac_T@180Hz≈0.040**（对照 r3≈1.05–1.08）→ render 残差实证清除。  
4. 未动 r1 / r2 / r3 / matrix results；composition 冻结。

## 授权核验

| 项 | 值 |
|---|---|
| GATE_BODY_SHA256 | `81fc74ebba270feb818646cd5f5c8c90fd2d3812c3447c642ed126413108934c` MATCH |
| APPROVED | pidan @2026-11-06 11:36:53 AWST（`docs/m3_matrix_pacing_r4_approved.md`） |
| composition.h | 未触 |
| 本结题采数 | **否**（纯文档） |

## 产物清单

- `docs/m3_matrix_pacing_r4_result.md`（权威）
- `docs/m3_matrix_pacing_r4_close_brief.md`
- `docs/m3_matrix_pacing_r4_close_pack.md`（本包）
- `docs/m3_matrix_pacing_r4_next.md`
- `docs/m3_matrix_pacing_r4_mainline.md`
- brief 一行：`docs/m2_mainline_brief.md`（追加）
- 只读结果：`m3/results_matrix_pacing_r4/`（勿覆写 r1/r2/r3/matrix）

## 禁令复述

不改 composition.h；不合 main；Success≠adopt；PERF-CI HOLD；不翻 CLOSED；不回改 MATRIX / r1 / r2 / r3；不授权新采数；不自动 LOCK 下一票；不把前序 FAIL 洗成 PASS；不合并 r1+r2+r3。

## 下一票决断

见 `docs/m3_matrix_pacing_r4_next.md`。  
**主推：归档 r4 CLOSED PASS（有条件采用须 pidan）；挂账/SUPERSEDED 候选 RENDER-BUDGET + BUDGET DRAFT；勿擅自 LOCK；不授权真机；PERF-CI 仍 HOLD。**

— notion ai（PACING r4 结题；CLOSED PASS；全判据绿；Success≠adopt；不合 main；Australia/Perth（AWST，UTC+8））
