# M3-MATRIX-PACING r2 结题中文包（m3_matrix_pacing_r2_close_pack.md）

> DECISION_AT=2026-11-06 11:08:00 AWST（Australia/Perth（AWST，UTC+8））  
> 权威结题：`docs/m3_matrix_pacing_r2_result.md`  
> 闸门：gate=**MATCH** `a1fa4590285f52948ecf51bc9c32629d4d2c3bc42efc7f5bb4e4922bd2be2883`；LOCKED_AT=2026-11-06 10:57:03 AWST  
> APPROVED：pidan @ 2026-11-06 10:57:53 AWST；approved_in_tree=yes  
> 前序：**禁回改** — MATRIX CLOSED `RIVE-MATRIX-FAIL`；PACING **r1** CLOSED `RIVE-MATRIX-PACING-FAIL`（budget_ok=0 / pacing_ok=1）  
> 分支 tip=`27ad82395e3eed94c40ad99feba6aed46eeca715`；results_commit=`e3feb5cde7441e1855727dd0468275a6254e56bc` @ **`claude/m3-matrix`**；push_exit=0  
> composition.h PRE=POST CRLF=`33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97`；LF=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`；**未改**

## 结论

- **票级 = CLOSED**；标签 **`RIVE-MATRIX-PACING-FAIL`**（**r2**）。
- **METRICS = METRICS_FAIL**，主因 **`pacing_ok=0`**（实测；**未**重跑）。
- **并行通过字段**：`budget_ok=1` `hitch_ok=1` `size_ok=1` `stability_ok=1`；sessions=3×RECORDED；`scale_changes=4`。
- **出帧旁证**：WaitForVBlank 后再 DwmFlush（`53cedde`）；probe v2；pacing_source=vblank+dwmflush；seg0–4 MATCH — **可引用**，**不**升 adopt。
- **与 r1 对偶**：r1 = pacing✓ / budget✗；r2 = pacing✗ / budget✓ — **两票均 FAIL**；禁止合并洗票或回改 r1。
- **是否采用 = 不采用**（Success≠adopt；**不合 main**）。
- **Success≠adopt**；**PERF-CI HOLD**；composition.h **未触**。

ONELINE（结题）：  
`M3-MATRIX-PACING r2 | gate=a1fa4590285f52948ecf51bc9c32629d4d2c3bc42efc7f5bb4e4922bd2be2883 @ 2026-11-06 10:57:03 AWST | approved=pidan@2026-11-06 10:57:53 AWST | host=DESKTOP-IJI1KJG | topology=75Hz@100%+180Hz@150% | sessions=3 | pacing_ok=0 | pacing_p95_ms_primary=5.164/5.273/5.208 | pacing_p95_ms_external=6.682/6.686/6.496 | hitch_ok=1 | budget_ok=1 | size_ok=1 | stability_ok=1 | ticket=RIVE-MATRIX-PACING-FAIL | dir=m3/results_matrix_pacing_r2/ | probe=M3-MATRIX-PACING v2 | pacing_source=vblank+dwmflush | LOCK≠采数 | 不改composition.h；成功≠采用；不合main；PERF-CI HOLD`

## 口径表

| 选项 | 决断 | 理由 |
|---|---|---|
| RIVE-MATRIX-PACING-PASS / 有条件 PASS | **否** | pacing_ok=0；budget 通过不抵消 |
| INCONCLUSIVE / INVALID | **否** | 3×RECORDED；gate MATCH；approved 在树；probe 路径 MATCH |
| **RIVE-MATRIX-PACING-FAIL** | **是** | pacing_ok=0 |
| 采用 tip / 合 main | **否** | Success≠adopt |
| 回改 r1 / MATRIX | **否** | 硬禁 |
| 把 budget_ok=1 写成整票 PASS | **否** | 禁止掩盖 pacing |

### 可引用 vs 不采用

| 可引用 | 不采用 / 禁止 |
|---|---|
| budget_ok=1 @ 同拓扑 | 整 tip 合 main / ship |
| vblank+dwmflush 出帧路径 + seg MATCH | 「r2 已全绿」叙事 |
| results_commit `e3feb5cde7441e1855727dd0468275a6254e56bc` + `m3/results_matrix_pacing_r2/` | 静默丢掉 pacing_ok=0；翻 r1 |
| 与 r1 字段对偶对照 | 本 pack 授权新采数；自动 LOCK 下票 |

## lab / 实现备注

1. WaitForVBlank → DwmFlush（`53cedde`）。  
2. probe v2；pacing_source=vblank+dwmflush；seg0–4 MATCH。  
3. **FAIL 照交未重跑**；未动 r1 / matrix results。

## 授权核验

| 项 | 值 |
|---|---|
| GATE_BODY_SHA256 | `a1fa4590285f52948ecf51bc9c32629d4d2c3bc42efc7f5bb4e4922bd2be2883` MATCH |
| APPROVED | pidan @2026-11-06 10:57:53 AWST；approved_in_tree=yes |
| composition.h | 未触 |
| 本结题采数 | **否**（纯文档） |

## 产物清单

- `docs/m3_matrix_pacing_r2_result.md`（权威）
- `docs/m3_matrix_pacing_r2_close_brief.md`
- `docs/m3_matrix_pacing_r2_close_pack.md`（本包）
- `docs/m3_matrix_pacing_r2_next.md`
- `docs/m3_matrix_pacing_r2_mainline.md`
- brief 一行：`docs/m2_mainline_brief.md`
- 只读结果：`m3/results_matrix_pacing_r2/`（勿覆写 r1/matrix）

## 禁令复述

不改 composition.h；不合 main；Success≠adopt；PERF-CI HOLD；不翻 CLOSED；不回改 MATRIX / r1；不授权新采数；不自动 LOCK 下一票；不把 FAIL 洗成 PASS。

## 下一票决断

见 `docs/m3_matrix_pacing_r2_next.md`。  
**主推：消化 r2 `pacing_ok=0` 的纯文档 DRAFT（票名待拍板）；勿擅自 LOCK；不授权真机。** 并重排相对已落盘的 M3-MATRIX-BUDGET DRAFT 优先级。

— notion ai（PACING r2 结题；CLOSED FAIL；pacing_ok=0；budget_ok=1；不采用；不合 main；Australia/Perth（AWST，UTC+8））
