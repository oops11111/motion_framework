# M3-MATRIX-PACING r2 结题结果

STATUS: CLOSED
TICKET: RIVE-MATRIX-PACING-FAIL
ROUND: **r2**（相对 r1 分立；**不翻** r1 / MATRIX）
METRICS: METRICS_FAIL（**pacing_ok=0**；budget_ok=1；hitch/size/stability=1）
ADOPT: **不采用**（Success≠adopt；不合 main；实验证据可引用但不升采用）
WRITTEN_AT: 2026-11-06 11:08:00 AWST（Australia/Perth（AWST，UTC+8））
GATE: gate=MATCH `a1fa4590285f52948ecf51bc9c32629d4d2c3bc42efc7f5bb4e4922bd2be2883`（LOCKED_AT 2026-11-06 10:57:03 AWST；workbuddy 宣称；**本机 box 未见独立 r2 LOCK 文件副本时不重算 body**）
GATE_MATCH: workbuddy ONELINE **MATCH**
APPROVED: pidan @ 2026-11-06 10:57:53 AWST（approved_in_tree=yes；本结题**不新签** APPROVED；不授权新采数）
BRANCH: claude/m3-matrix
TIP_READ: 27ad82395e3eed94c40ad99feba6aed46eeca715
RESULTS_COMMIT: e3feb5cde7441e1855727dd0468275a6254e56bc（dir=`m3/results_matrix_pacing_r2/`；push_exit=0）
HOST: DESKTOP-IJI1KJG
DISPLAY: primary_PHL_1280x1024@75Hz/100% + external_VSC_2560x1440@180Hz/150%（count=2）
BUILD: OK；dll sha256=`2c76816f29e7812616188bd25d4c4565ef601a047d2b66988df0bb950ce8d6a6`
PROBE: M3-MATRIX-PACING **v2**；pacing_source=**vblank+dwmflush**（三会话 seg0–4 MATCH）
composition.h: PRE=POST CRLF=`33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97`；LF=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`；**未改**
RESULTS: m3/results_matrix_pacing_r2/（**勿覆写** `m3/results_matrix_pacing/` / `m3/results_matrix/`）
PERF-CI: HOLD
Success≠adopt: 是
本结题: **纯文档 ≠ 采数 ≠ 授权真机**；不 LOCK；不改 composition.h

## 与 r1 对照（写死；不翻盘）

| 项 | r1（CLOSED；**勿翻**） | r2（本结题） |
|---|---|---|
| 票标签 | RIVE-MATRIX-PACING-FAIL | RIVE-MATRIX-PACING-FAIL |
| 主因 | **budget_ok=0**；pacing_ok=1 | **pacing_ok=0**；budget_ok=1 |
| results dir | m3/results_matrix_pacing/ | m3/results_matrix_pacing_r2/ |
| tip / results | `fd05c6c058022c233f1cd896338e03d665c43d0f` / `7d5f545c7a3af0b7410421006a3683ce64c93f07` | `27ad82395e3eed94c40ad99feba6aed46eeca715` / `e3feb5cde7441e1855727dd0468275a6254e56bc` |
| gate | `2543e424cd71eea2959fe942ce0bfe7d7118ea6ee5c28b31da7902bce904812c` | `a1fa4590285f52948ecf51bc9c32629d4d2c3bc42efc7f5bb4e4922bd2be2883` |
| 旁证 | 节奏窗 `m0r_set_pacing_window` | 出帧时机 **WaitForVBlank 后再 DwmFlush**（`53cedde`） |

**口径：** r1 与 r2 **均为 CLOSED FAIL**；字段此消彼长**不**合并成 PASS；**禁止**回改 r1 / MATRIX 标签。

## 授权 / 闸门核验摘要

| 项 | 结果 |
|---|---|
| gate hash | `a1fa4590285f52948ecf51bc9c32629d4d2c3bc42efc7f5bb4e4922bd2be2883` **MATCH** |
| LOCKED_AT | 2026-11-06 10:57:03 AWST |
| APPROVED | pidan @ 2026-11-06 10:57:53 AWST；approved_in_tree=yes |
| composition.h | PRE=POST；**守冻结** |
| 合 main / adopt | **否** |
| 本结题任务 | **纯文档结题 ≠ 采数**；FAIL 照交未重跑 |

## 硬字段对照（ONELINE / workbuddy）

| 字段 | 值 | 裁决 |
|---|---|---|
| sessions | 3×RECORDED | 可裁决会话成立 |
| pacing_ok | **0**（p95_ms primary=5.164/5.273/5.208；external=6.682/6.686/6.496） | **FAIL**（主因；实测；**未**重跑） |
| hitch_ok | 1 | PASS |
| size_ok | 1 | PASS |
| budget_ok | **1** | PASS（相对 r1 的 budget 残差本轮已过；**不**抵消 pacing FAIL） |
| stability_ok | 1 | PASS |
| scale_changes | 4 | 记录 |
| pacing_source | vblank+dwmflush；seg0–4 MATCH | 探针路径可裁决 |
| 拓扑 | 75Hz@100% + 180Hz@150% @ IJI | 满足 |

**票级理由：** `pacing_ok=0` → **`RIVE-MATRIX-PACING-FAIL`** / **METRICS_FAIL**。`budget_ok=1` 与 hitch/size/stability=1 **不**抵消 pacing 失败；亦不把本票升格 PASS/有条件 PASS；**不**回写 r1。

## lab / 实现备注（必须写入结题）

1. 代码路径：**WaitForVBlank 后再 DwmFlush**（commit `53cedde`）— 出帧时机改动之旁证。  
2. probe=M3-MATRIX-PACING **v2**；pacing_source=vblank+dwmflush；三会话 seg0–4 **MATCH**。  
3. **FAIL 照交未重跑**（pacing_ok=0 为实测）。  
4. 结果只进 `m3/results_matrix_pacing_r2/`；**未**动 r1 / `m3/results_matrix_pacing/` / `m3/results_matrix/`；**未**回改 CLOSED 标签。

## workbuddy ONELINE（结题口径）

```
M3-MATRIX-PACING r2 | gate=a1fa4590285f52948ecf51bc9c32629d4d2c3bc42efc7f5bb4e4922bd2be2883 @ 2026-11-06 10:57:03 AWST | approved=pidan@2026-11-06 10:57:53 AWST | host=DESKTOP-IJI1KJG | topology=75Hz@100%+180Hz@150% | sessions=3 | pacing_ok=0 | pacing_p95_ms_primary=5.164/5.273/5.208 | pacing_p95_ms_external=6.682/6.686/6.496 | hitch_ok=1 | budget_ok=1 | size_ok=1 | stability_ok=1 | ticket=RIVE-MATRIX-PACING-FAIL | dir=m3/results_matrix_pacing_r2/ | probe=M3-MATRIX-PACING v2 | pacing_source=vblank+dwmflush | LOCK≠采数 | 不改composition.h；成功≠采用；不合main；PERF-CI HOLD
```

## 采用口径

| 项 | 决断 |
|---|---|
| 采用 tip / 合 main | **否** |
| 采用「r2 已修 budget / 残差清零」叙事 | **否**（pacing_ok=0） |
| 引用实验证据（budget_ok=1 + vblank+dwmflush + results） | **可引用**；须同时写明 **pacing FAIL**；并对照 r1 字段对偶 |
| 解冻 PERF-CI | **否** |
| 改 composition.h | **否** |
| 回改 CLOSED MATRIX / r1 标签 | **否**（禁） |

## 残差 / 挂账

| 项 | 处置 |
|---|---|
| **pacing_ok=0**（r2） | **主残差**；见 next；**勿擅自 LOCK** |
| budget_ok=1（r2） | 相对 r1 budget 残差的旁证；**不**洗本票 / r1 |
| r1 pacing_ok=1 / budget_ok=0 | **保持 CLOSED**；字段对偶挂账 |
| MATRIX CLOSED FAIL | **保持**；禁回改 |
| M3-MATRIX-BUDGET DRAFT | 仍可能相关；须按 next 与开放问题重排优先级（r2 主残差回到 pacing） |
| PERF-CI | HOLD |

## 禁令复述

不改 composition.h；不合 main；Success≠adopt；PERF-CI HOLD；不翻 CLOSED；不回改 MATRIX / PACING r1；本结题不授权新采数/真机；不把 FAIL 改写成 PASS；不自动 LOCK 下一票。

— notion ai（M3-MATRIX-PACING r2 结题；CLOSED RIVE-MATRIX-PACING-FAIL；pacing_ok=0；budget_ok=1；不采用；不合 main；Australia/Perth（AWST，UTC+8））
