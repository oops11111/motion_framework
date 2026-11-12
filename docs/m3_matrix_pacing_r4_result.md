# M3-MATRIX-PACING r4 结题结果

STATUS: CLOSED
TICKET: RIVE-MATRIX-PACING-PASS
ROUND: **r4**（相对 r1/r2/r3 分立；**不翻** r1 / r2 / r3 / MATRIX）
METRICS: METRICS_PASS（**全判据绿**：pacing_ok=1；hitch_ok=1；size_ok=1；budget_ok=1；stability_ok=1）
ADOPT: **不自动采用**（Success≠adopt；不合 main；有条件采用须 **pidan 另拍板**；实验证据可引用但不升 adopt）
WRITTEN_AT: 2026-11-06 11:58:00 AWST（Australia/Perth（AWST，UTC+8））
GATE: gate=MATCH `81fc74ebba270feb818646cd5f5c8c90fd2d3812c3447c642ed126413108934c`（LOCKED_AT 2026-11-06 11:27:50 AWST；workbuddy 宣称；**本机 box 未见独立 r4 LOCK 文件副本时不重算 body**）
GATE_MATCH: workbuddy ONELINE **MATCH**
APPROVED: pidan @ 2026-11-06 11:36:53 AWST（`docs/m3_matrix_pacing_r4_approved.md`；本结题**不新签** APPROVED；不授权新采数）
BRANCH: claude/m3-matrix
TIP_READ: e94600332307e4482cf0ff91d4e16d3b60deefc5
RESULTS_COMMIT: 6b7a290864ef58ff8b908f8d46532e449c8e4530（dir=`m3/results_matrix_pacing_r4/`）
HOST: DESKTOP-IJI1KJG
DISPLAY: primary_PHL_1280x1024@75Hz/100% + external_VSC_2560x1440@180Hz/150%（topology=75Hz@100%+180Hz@150%）
BUILD: OK；dll sha256=`29daae326b5ee3903b8e2ed1352372825007065de92f966db66310746ad16026`
PROBE: M3-MATRIX-PACING **v4**；pacing_source=**vblank**；pacing_offset_fraction=**0.30**（三会话 MATCH）
composition.h: PRE=POST CRLF=`33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97`；LF=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`；**未改**
RESULTS: m3/results_matrix_pacing_r4/（**勿覆写** `m3/results_matrix_pacing/` / `_r2/` / `_r3/` / `m3/results_matrix/`；本机 box 结题时未见该目录副本，以 tip/results commit 为准）
PERF-CI: HOLD
Success≠adopt: 是
本结题: **纯文档 ≠ 采数 ≠ 授权真机**；不 LOCK 新闸门；不改 composition.h

## 与 r1 / r2 / r3 对照（写死；不翻盘；禁洗票）

| 项 | r1（CLOSED；**勿翻**） | r2（CLOSED；**勿翻**） | r3（CLOSED；**勿翻**） | r4（本结题） |
|---|---|---|---|---|
| 票标签 | RIVE-MATRIX-PACING-FAIL | RIVE-MATRIX-PACING-FAIL | RIVE-MATRIX-PACING-FAIL | **RIVE-MATRIX-PACING-PASS** |
| 主因 / 结果 | **budget_ok=0**；pacing_ok=1 | **pacing_ok=0**；budget_ok=1 | **budget_ok=0**；pacing_ok=1 | **全绿**（含 budget_ok=1） |
| results dir | m3/results_matrix_pacing/ | m3/results_matrix_pacing_r2/ | m3/results_matrix_pacing_r3/ | m3/results_matrix_pacing_r4/ |
| tip / results | `fd05c6c058022c233f1cd896338e03d665c43d0f` / `7d5f545c7a3af0b7410421006a3683ce64c93f07` | `27ad82395e3eed94c40ad99feba6aed46eeca715` / `e3feb5cde7441e1855727dd0468275a6254e56bc` | `82eacb0b59e1bab7ee809b4b62c8a100545b4073` / `39cdcf42e7758555b223c8058cc8a2e9e3793aff` | `e94600332307e4482cf0ff91d4e16d3b60deefc5` / `6b7a290864ef58ff8b908f8d46532e449c8e4530` |
| gate | `2543e424cd71eea2959fe942ce0bfe7d7118ea6ee5c28b31da7902bce904812c` | `a1fa4590285f52948ecf51bc9c32629d4d2c3bc42efc7f5bb4e4922bd2be2883` | `90f9356bf2739ea2a95a56619fc73af561b0c34e4ebd097be05442f8c5719476` | `81fc74ebba270feb818646cd5f5c8c90fd2d3812c3447c642ed126413108934c` |
| 旁证 | 节奏窗；合计 GPU 含等 DWM | WaitForVBlank 后再 DwmFlush；vblank+dwmflush | 878892d：只等 WaitForVBlank；拆分 render/copy；gpu_render@180Hz≈1.05–1.08 | **69094ee**：vblank 后再等 **0.3·T**；gpu_render_p95_frac_T@180Hz≈**0.040** |

**口径：** r1 / r2 / r3 **保持 CLOSED FAIL**；r4 为**分立 tip/results** 下的 CLOSED PASS。**禁止**回改 r1 / r2 / r3 / MATRIX 标签；**禁止**把 r1+r2+r3「拼成」PASS；**禁止**用 r4 PASS 洗前序 FAIL。

**r4 相对 r3 说明（必写）：** 代码基线 **`69094ee`（vblank 后再等 0.3·T）** 下，r3 的 render 预算残差实证消掉：`gpu_render_p95_frac_T@180Hz≈0.040`（远低于 0.35·T；对照 r3≈1.05–1.08）；`budget_ok=1`。pacing_ok=1 与其余判据同步绿。**Success≠adopt**；有条件采用 WP（offset=0.3·T）须 pidan 另拍板。

## 授权 / 闸门核验摘要

| 项 | 结果 |
|---|---|
| gate hash | `81fc74ebba270feb818646cd5f5c8c90fd2d3812c3447c642ed126413108934c` **MATCH** |
| LOCKED_AT | 2026-11-06 11:27:50 AWST |
| APPROVED | pidan @ 2026-11-06 11:36:53 AWST（`docs/m3_matrix_pacing_r4_approved.md`） |
| composition.h | PRE=POST；**守冻结** |
| 合 main / adopt | **否**（有条件采用须 pidan 另拍板） |
| 本结题任务 | **纯文档结题 ≠ 采数**；不 LOCK 新闸门 |

## 硬字段对照（ONELINE / workbuddy）

| 字段 | 值 | 裁决 |
|---|---|---|
| sessions | 3×RECORDED | 可裁决会话成立 |
| pacing_ok | **1**（p95_ms primary≈13.38–13.39；external≈5.63–5.69） | PASS |
| hitch_ok | 1 | PASS |
| size_ok | 1 | PASS |
| budget_ok | **1** | PASS（对照 r3=0；render 残差实证清除） |
| stability_ok | 1 | PASS |
| scale_changes | 4 | 记录 |
| pacing_source | vblank；三会话 MATCH | 探针路径可裁决 |
| pacing_offset_fraction | **0.30**（三会话 MATCH） | 记录；WP 候选参数 |
| pacing_offset_last_ms | @75Hz≈4.06–4.52（over_T≈0.304–0.339）；@180Hz≈1.79–2.19（over_T≈0.322–0.394） | 记录 |
| probe | M3-MATRIX-PACING v4 | 记录 |
| gpu_render_p95_frac_T@180Hz | ≈**0.040**（相对 0.35·T；对照 r3≈1.05–1.08） | PASS（主消残差证据） |
| 拓扑 | 75Hz@100% + 180Hz@150% @ IJI | 满足 |

**票级理由：** 全判据绿 → **`RIVE-MATRIX-PACING-PASS`** / **METRICS_PASS**。**Success≠adopt**；**不合 main**；**不**回写 r1/r2/r3/MATRIX；**不**把前序 FAIL 洗成 PASS。

## lab / 实现备注（必须写入结题）

1. 代码路径：**69094ee** — vblank 后再等 **0.3·T**（`pacing_offset_fraction=0.30`）。  
2. probe=M3-MATRIX-PACING **v4**；pacing_source=**vblank**；offset_fraction=0.30；三会话 **MATCH**。  
3. **gpu_render_p95_frac_T@180Hz≈0.040**（远低于 0.35·T）；对照 r3≈1.05–1.08 → render 预算残差在本 tip **实证清除**。  
4. 结果只进 `m3/results_matrix_pacing_r4/`；**未**动 r1 / r2 / r3 / `m3/results_matrix/`；**未**回改 CLOSED 标签。  
5. composition.h PRE=POST；**未改**。

## workbuddy ONELINE（结题口径）

```
M3-MATRIX-PACING r4 | gate=81fc74ebba270feb818646cd5f5c8c90fd2d3812c3447c642ed126413108934c @ 2026-11-06 11:27:50 AWST | approved=pidan@2026-11-06 11:36:53 AWST | host=DESKTOP-IJI1KJG | topology=75Hz@100%+180Hz@150% | sessions=3 | pacing_ok=1 | pacing_p95_ms_primary≈13.38–13.39 | pacing_p95_ms_external≈5.63–5.69 | hitch_ok=1 | budget_ok=1 | size_ok=1 | stability_ok=1 | ticket=RIVE-MATRIX-PACING-PASS | dir=m3/results_matrix_pacing_r4/ | probe=M3-MATRIX-PACING v4 | pacing_source=vblank | pacing_offset_fraction=0.30 | gpu_render_p95_frac_T(180Hz)≈0.040 | tip=e94600332307e4482cf0ff91d4e16d3b60deefc5 | results=6b7a290864ef58ff8b908f8d46532e449c8e4530 | code=69094ee | Success≠adopt | LOCK≠采数 | 不改composition.h；不合main；PERF-CI HOLD
```

## 采用口径

| 项 | 决断 |
|---|---|
| 采用 tip / 合 main | **否**（默认）；有条件采用须 **pidan 另拍板** |
| 采用「r4 全绿=可 ship」叙事 | **否**（Success≠adopt；PERF-CI HOLD） |
| 引用实验证据（全判据绿 + offset=0.3·T + gpu_render≈0.040 + results） | **可引用**；须同时写明 **Success≠adopt**、不合 main、前序 r1–r3 仍 CLOSED FAIL |
| 解冻 PERF-CI | **否**（建议仍 HOLD；见 next） |
| 改 composition.h | **否** |
| 回改 CLOSED MATRIX / PACING r1 / r2 / r3 标签 | **否**（禁） |
| 合并 r1+r2+r3 改 PASS / 用 r4 洗前序 FAIL | **否**（禁洗票） |
| 自动 LOCK RENDER-BUDGET / BUDGET DRAFT 为 SUPERSEDED | **否**；仅建议挂账/SUPERSEDED **候选**（见 next） |

## 残差 / 挂账

| 项 | 处置 |
|---|---|
| **r4 全绿（含 budget_ok=1）** | 本票 CLOSED PASS；**不**自动 adopt |
| r3 render 残差（gpu_render@180Hz≈1.05–1.08） | 在 tip `69094ee` / offset=0.3·T 下 **实证清除**；**不**翻 r3 CLOSED FAIL |
| `docs/m3_matrix_render_budget_gate_DRAFT.md` | 建议 **挂账 / SUPERSEDED 候选**（因 r4 budget_ok=1）；**开放给 pidan**；**勿擅自 LOCK / 勿自动改写 DRAFT 正文** |
| `docs/m3_matrix_budget_gate_DRAFT.md` | 同上建议挂账 / SUPERSEDED 候选；开放给 pidan |
| r1 / r2 / r3 / MATRIX | **保持 CLOSED FAIL**；禁回改 |
| PERF-CI | **HOLD** |
| 有条件采用 WP（offset=0.3·T） | **开放**；须 pidan 拍板 |

## 禁令复述

不改 composition.h；不合 main；Success≠adopt；PERF-CI HOLD；不翻 CLOSED；不回改 MATRIX / PACING r1 / r2 / r3；本结题不授权新采数/真机；不自动 LOCK 下一票；不把前序 FAIL 洗成 PASS；不合并 r1+r2+r3；不自动改写 RENDER-BUDGET / BUDGET DRAFT 为 LOCK/SUPERSEDED。

— notion ai（M3-MATRIX-PACING r4 结题；CLOSED RIVE-MATRIX-PACING-PASS；全判据绿；Success≠adopt；不合 main；Australia/Perth（AWST，UTC+8））
