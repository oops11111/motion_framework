# M3-MATRIX-PACING r3 结题中文包（m3_matrix_pacing_r3_close_pack.md）

> DECISION_AT=2026-11-06 11:26:00 AWST（Australia/Perth（AWST，UTC+8））  
> 权威结题：`docs/m3_matrix_pacing_r3_result.md`  
> 闸门：gate=**MATCH** `90f9356bf2739ea2a95a56619fc73af561b0c34e4ebd097be05442f8c5719476`；LOCKED_AT=2026-11-06 11:14:15 AWST  
> APPROVED：pidan @ 2026-11-06 11:15:19 AWST；approved_in_tree=yes  
> 前序：**禁回改** — MATRIX CLOSED `RIVE-MATRIX-FAIL`；PACING **r1** CLOSED `RIVE-MATRIX-PACING-FAIL`（budget_ok=0 / pacing_ok=1）；PACING **r2** CLOSED `RIVE-MATRIX-PACING-FAIL`（pacing_ok=0 / budget_ok=1）  
> 分支 tip=`82eacb0b59e1bab7ee809b4b62c8a100545b4073`；results_commit=`39cdcf42e7758555b223c8058cc8a2e9e3793aff` @ **`claude/m3-matrix`**；push_exit=0  
> composition.h PRE=POST CRLF=`33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97`；LF=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`；**未改**

## 结论

- **票级 = CLOSED**；标签 **`RIVE-MATRIX-PACING-FAIL`**（**r3**）。
- **METRICS = METRICS_FAIL**，主因 **`budget_ok=0`**（实测；**未**重跑）。
- **并行通过字段**：`pacing_ok=1` `hitch_ok=1` `size_ok=1` `stability_ok=1`；sessions=3×RECORDED；`scale_changes=4`。
- **出帧 / GPU 旁证**：代码 `878892d`（只等 WaitForVBlank；GPU 拆分 render/copy；预算按 render）；probe v3；pacing_source=vblank；三会话 MATCH；**gpu_copy≈0**；**gpu_render_p95_frac_T@180Hz≈1.05–1.08** 仍超 0.35·T — **可引用**，**不**升 adopt。
- **与 r1 / r2 对偶（禁洗票）**：  
  - r1 = pacing✓ / budget✗（合计 GPU 含等 DWM）  
  - r2 = pacing✗ / budget✓（vblank+dwmflush）  
  - r3 = pacing✓ / budget✗（拆分后失败在 render，非 copy）  
  **三票均 FAIL**；禁止合并洗票或回改 r1/r2/MATRIX。
- **是否采用 = 不采用**（Success≠adopt；**不合 main**）。
- **Success≠adopt**；**PERF-CI HOLD**；composition.h **未触**。

ONELINE（结题）：  
`M3-MATRIX-PACING r3 | gate=90f9356bf2739ea2a95a56619fc73af561b0c34e4ebd097be05442f8c5719476 @ 2026-11-06 11:14:15 AWST | approved=pidan@2026-11-06 11:15:19 AWST | host=DESKTOP-IJI1KJG | topology=75Hz@100%+180Hz@150% | sessions=3 | pacing_ok=1 | pacing_p95_ms_primary=0.060/0.065/0.064 | pacing_p95_ms_external=0.111/0.107/0.114 | hitch_ok=1 | budget_ok=0 | size_ok=1 | stability_ok=1 | ticket=RIVE-MATRIX-PACING-FAIL | dir=m3/results_matrix_pacing_r3/ | probe=M3-MATRIX-PACING v3 | pacing_source=vblank | gpu_render_p95_frac_T(180Hz)≈1.05–1.08 | gpu_copy_p95_ms≈0.008 | LOCK≠采数 | 不改composition.h；成功≠采用；不合main；PERF-CI HOLD`

## 口径表

| 选项 | 决断 | 理由 |
|---|---|---|
| RIVE-MATRIX-PACING-PASS / 有条件 PASS | **否** | budget_ok=0；pacing 通过不抵消 |
| INCONCLUSIVE / INVALID | **否** | 3×RECORDED；gate MATCH；approved 在树；probe 路径 MATCH |
| **RIVE-MATRIX-PACING-FAIL** | **是** | budget_ok=0 |
| 采用 tip / 合 main | **否** | Success≠adopt |
| 回改 r1 / r2 / MATRIX | **否** | 硬禁 |
| 把 pacing_ok=1 写成整票 PASS | **否** | 禁止掩盖 budget |
| 合并 r1+r2+r3 改 PASS | **否** | 禁洗票 |

### 可引用 vs 不采用

| 可引用 | 不采用 / 禁止 |
|---|---|
| pacing_ok=1 @ vblank-only；p95≪T | 整 tip 合 main / ship |
| GPU render/copy 拆分；gpu_copy≈0；render@180Hz 超 0.35·T | 「r3 已全绿 / 三轮拼 PASS」叙事 |
| results_commit `39cdcf42e7758555b223c8058cc8a2e9e3793aff` + `m3/results_matrix_pacing_r3/` | 静默丢掉 budget_ok=0；翻 r1/r2/MATRIX |
| 与 r1/r2 字段对偶对照 | 本 pack 授权新采数；自动 LOCK 下票 |

## lab / 实现备注

1. 代码 **878892d**：只等 WaitForVBlank；GPU 拆分 render/copy；预算按 render。  
2. probe v3；pacing_source=vblank；三会话 MATCH。  
3. **FAIL 照交未重跑**；未动 r1 / r2 / matrix results。  
4. 预算失败主因在 **标为 render 的区间**（非 copy 等待）。

## 授权核验

| 项 | 值 |
|---|---|
| GATE_BODY_SHA256 | `90f9356bf2739ea2a95a56619fc73af561b0c34e4ebd097be05442f8c5719476` MATCH |
| APPROVED | pidan @2026-11-06 11:15:19 AWST；approved_in_tree=yes |
| composition.h | 未触 |
| 本结题采数 | **否**（纯文档） |

## 产物清单

- `docs/m3_matrix_pacing_r3_result.md`（权威）
- `docs/m3_matrix_pacing_r3_close_brief.md`
- `docs/m3_matrix_pacing_r3_close_pack.md`（本包）
- `docs/m3_matrix_pacing_r3_next.md`
- `docs/m3_matrix_pacing_r3_mainline.md`
- brief 一行：`docs/m2_mainline_brief.md`（追加）
- 只读结果：`m3/results_matrix_pacing_r3/`（勿覆写 r1/r2/matrix）

## 禁令复述

不改 composition.h；不合 main；Success≠adopt；PERF-CI HOLD；不翻 CLOSED；不回改 MATRIX / r1 / r2；不授权新采数；不自动 LOCK 下一票；不把 FAIL 洗成 PASS；不合并 r1+r2+r3。

## 下一票决断

见 `docs/m3_matrix_pacing_r3_next.md`。  
**主推：消化 r3 `budget_ok=0`（gpu_render@180Hz）的纯文档 DRAFT（RENDER-BUDGET / 重排 BUDGET DRAFT；票名待拍板）；pacing 残差可挂账对照；勿擅自 LOCK；不授权真机。**

— notion ai（PACING r3 结题；CLOSED FAIL；pacing_ok=1；budget_ok=0；不采用；不合 main；Australia/Perth（AWST，UTC+8））
