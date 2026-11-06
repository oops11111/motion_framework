# M3-MATRIX-PACING r3 结题结果

STATUS: CLOSED
TICKET: RIVE-MATRIX-PACING-FAIL
ROUND: **r3**（相对 r1/r2 分立；**不翻** r1 / r2 / MATRIX）
METRICS: METRICS_FAIL（**budget_ok=0**；pacing_ok=1；hitch/size/stability=1）
ADOPT: **不采用**（Success≠adopt；不合 main；实验证据可引用但不升采用）
WRITTEN_AT: 2026-11-06 11:26:00 AWST（Australia/Perth（AWST，UTC+8））
GATE: gate=MATCH `90f9356bf2739ea2a95a56619fc73af561b0c34e4ebd097be05442f8c5719476`（LOCKED_AT 2026-11-06 11:14:15 AWST；workbuddy 宣称；**本机 box 未见独立 r3 LOCK 文件副本时不重算 body**）
GATE_MATCH: workbuddy ONELINE **MATCH**
APPROVED: pidan @ 2026-11-06 11:15:19 AWST（approved_in_tree=yes；本结题**不新签** APPROVED；不授权新采数）
BRANCH: claude/m3-matrix
TIP_READ: 82eacb0b59e1bab7ee809b4b62c8a100545b4073
RESULTS_COMMIT: 39cdcf42e7758555b223c8058cc8a2e9e3793aff（dir=`m3/results_matrix_pacing_r3/`；push_exit=0）
HOST: DESKTOP-IJI1KJG
DISPLAY: primary_PHL_1280x1024@75Hz/100% + external_VSC_2560x1440@180Hz/150%（count=2）
BUILD: OK；dll sha256=`81cbdbbb571110303ae8f9a2b95f6b7284176ac9995d2f9c4b81be0ff60e9f6b`
PROBE: M3-MATRIX-PACING **v3**；pacing_source=**vblank**（三会话 MATCH）
composition.h: PRE=POST CRLF=`33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97`；LF=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`；**未改**
RESULTS: m3/results_matrix_pacing_r3/（**勿覆写** `m3/results_matrix_pacing/` / `m3/results_matrix_pacing_r2/` / `m3/results_matrix/`；本机 box 结题时未见该目录副本，以 tip/results commit 为准）
PERF-CI: HOLD
Success≠adopt: 是
本结题: **纯文档 ≠ 采数 ≠ 授权真机**；不 LOCK；不改 composition.h

## 与 r1 / r2 对照（写死；不翻盘；禁洗票）

| 项 | r1（CLOSED；**勿翻**） | r2（CLOSED；**勿翻**） | r3（本结题） |
|---|---|---|---|
| 票标签 | RIVE-MATRIX-PACING-FAIL | RIVE-MATRIX-PACING-FAIL | RIVE-MATRIX-PACING-FAIL |
| 主因 | **budget_ok=0**；pacing_ok=1 | **pacing_ok=0**；budget_ok=1 | **budget_ok=0**；pacing_ok=1 |
| results dir | m3/results_matrix_pacing/ | m3/results_matrix_pacing_r2/ | m3/results_matrix_pacing_r3/ |
| tip / results | `fd05c6c058022c233f1cd896338e03d665c43d0f` / `7d5f545c7a3af0b7410421006a3683ce64c93f07` | `27ad82395e3eed94c40ad99feba6aed46eeca715` / `e3feb5cde7441e1855727dd0468275a6254e56bc` | `82eacb0b59e1bab7ee809b4b62c8a100545b4073` / `39cdcf42e7758555b223c8058cc8a2e9e3793aff` |
| gate | `2543e424cd71eea2959fe942ce0bfe7d7118ea6ee5c28b31da7902bce904812c` | `a1fa4590285f52948ecf51bc9c32629d4d2c3bc42efc7f5bb4e4922bd2be2883` | `90f9356bf2739ea2a95a56619fc73af561b0c34e4ebd097be05442f8c5719476` |
| 旁证 | 节奏窗 `m0r_set_pacing_window`；合计 GPU 含等 DWM | 出帧 **WaitForVBlank 后再 DwmFlush**（`53cedde`）；vblank+dwmflush | 代码 **878892d**（只等 WaitForVBlank；GPU 拆分 render/copy；预算按 render）；pacing_source=vblank |

**口径：** r1 / r2 / r3 **均为 CLOSED FAIL**；字段此消彼长**不**合并成 PASS；**禁止**回改 r1 / r2 / MATRIX 标签；**禁止**把三轮拼成「节奏已绿+预算曾绿=整线 PASS」。

**r3 相对拆分说明（必写）：** 拆分后 **gpu_copy_p95_ms≈0.008（可忽略）**，**gpu_render_p95_frac_T@180Hz（seg1/3）≈1.05–1.08** 仍超 **0.35·T**。说明本轮预算失败主要在「标为 render」的区间，而非 copy 等待。勿用 r2 的 budget_ok=1 抵消本轮 budget_ok=0。

## 授权 / 闸门核验摘要

| 项 | 结果 |
|---|---|
| gate hash | `90f9356bf2739ea2a95a56619fc73af561b0c34e4ebd097be05442f8c5719476` **MATCH** |
| LOCKED_AT | 2026-11-06 11:14:15 AWST |
| APPROVED | pidan @ 2026-11-06 11:15:19 AWST；approved_in_tree=yes |
| composition.h | PRE=POST；**守冻结** |
| 合 main / adopt | **否** |
| 本结题任务 | **纯文档结题 ≠ 采数**；FAIL 照交未重跑 |

## 硬字段对照（ONELINE / workbuddy）

| 字段 | 值 | 裁决 |
|---|---|---|
| sessions | 3×RECORDED | 可裁决会话成立 |
| pacing_ok | **1**（p95_ms primary=0.060/0.065/0.064；external=0.111/0.107/0.114） | PASS（相对 r2 pacing 残差本轮已过；**不**抵消 budget FAIL） |
| hitch_ok | 1 | PASS |
| size_ok | 1 | PASS |
| budget_ok | **0** | **FAIL**（主因；实测；**未**重跑） |
| stability_ok | 1 | PASS |
| scale_changes | 4 | 记录 |
| pacing_source | vblank；三会话 MATCH | 探针路径可裁决 |
| probe | M3-MATRIX-PACING v3 | 记录 |
| gpu_render_p95_frac_T(180Hz seg1/3) | ≈1.05–1.08（相对 0.35·T） | 预算失败主证据（render 区间） |
| gpu_copy_p95_ms | ≈0.008 | 可忽略；非主因 |
| 拓扑 | 75Hz@100% + 180Hz@150% @ IJI | 满足 |

**票级理由：** `budget_ok=0` → **`RIVE-MATRIX-PACING-FAIL`** / **METRICS_FAIL**。`pacing_ok=1` 与 hitch/size/stability=1 **不**抵消 budget 失败；亦不把本票升格 PASS/有条件 PASS；**不**回写 r1/r2；**不**合并三轮改 PASS。

## lab / 实现备注（必须写入结题）

1. 代码路径：**878892d** — 只等 WaitForVBlank；GPU 拆分 render/copy；预算按 render。  
2. probe=M3-MATRIX-PACING **v3**；pacing_source=**vblank**；三会话 **MATCH**。  
3. **FAIL 照交未重跑**（budget_ok=0 为实测）。  
4. 结果只进 `m3/results_matrix_pacing_r3/`；**未**动 r1 / r2 / `m3/results_matrix/`；**未**回改 CLOSED 标签。  
5. 拆分结论：gpu_copy≈0；gpu_render_p95_frac_T@180Hz≈1.05–1.08 仍超 0.35·T → 预算残差在 **render** 侧。

## workbuddy ONELINE（结题口径）

```
M3-MATRIX-PACING r3 | gate=90f9356bf2739ea2a95a56619fc73af561b0c34e4ebd097be05442f8c5719476 @ 2026-11-06 11:14:15 AWST | approved=pidan@2026-11-06 11:15:19 AWST | host=DESKTOP-IJI1KJG | topology=75Hz@100%+180Hz@150% | sessions=3 | pacing_ok=1 | pacing_p95_ms_primary=0.060/0.065/0.064 | pacing_p95_ms_external=0.111/0.107/0.114 | hitch_ok=1 | budget_ok=0 | size_ok=1 | stability_ok=1 | ticket=RIVE-MATRIX-PACING-FAIL | dir=m3/results_matrix_pacing_r3/ | probe=M3-MATRIX-PACING v3 | pacing_source=vblank | gpu_render_p95_frac_T(180Hz)≈1.05–1.08 | gpu_copy_p95_ms≈0.008 | LOCK≠采数 | 不改composition.h；成功≠采用；不合main；PERF-CI HOLD
```

## 采用口径

| 项 | 决断 |
|---|---|
| 采用 tip / 合 main | **否** |
| 采用「r3 已修 pacing / 残差清零」叙事 | **否**（budget_ok=0） |
| 引用实验证据（pacing_ok=1 + vblank-only + render/copy 拆分 + results） | **可引用**；须同时写明 **budget FAIL** 且主因在 **gpu_render@180Hz**；并对照 r1/r2 字段对偶 |
| 解冻 PERF-CI | **否** |
| 改 composition.h | **否** |
| 回改 CLOSED MATRIX / PACING r1 / r2 标签 | **否**（禁） |
| 合并 r1+r2+r3 改 PASS | **否**（禁洗票） |

## 残差 / 挂账

| 项 | 处置 |
|---|---|
| **budget_ok=0**（r3；gpu_render_p95_frac_T@180Hz≈1.05–1.08） | **主残差**；见 next；**勿擅自 LOCK** |
| pacing_ok=1（r3；vblank-only；p95≪T） | 相对 r2 pacing 残差旁证；**pacing 残差大体可挂账闭合**；**不**洗本票 |
| r1 pacing✓/budget✗；r2 pacing✗/budget✓ | **保持 CLOSED**；字段对偶挂账 |
| MATRIX CLOSED FAIL | **保持**；禁回改 |
| M3-MATRIX-BUDGET DRAFT | 因 r3 再显 budget 残差（且定位到 render），建议按 next **重排**：对照/挂账 ↔ 重开 LOCK 路径 / 新开 RENDER-BUDGET DRAFT |
| PERF-CI | HOLD |

## 禁令复述

不改 composition.h；不合 main；Success≠adopt；PERF-CI HOLD；不翻 CLOSED；不回改 MATRIX / PACING r1 / r2；本结题不授权新采数/真机；不把 FAIL 改写成 PASS；不合并 r1+r2+r3 洗票；不自动 LOCK 下一票。

— notion ai（M3-MATRIX-PACING r3 结题；CLOSED RIVE-MATRIX-PACING-FAIL；pacing_ok=1；budget_ok=0；不采用；不合 main；Australia/Perth（AWST，UTC+8））
