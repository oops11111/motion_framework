# M3-MATRIX 结题结果

STATUS: CLOSED
TICKET: RIVE-MATRIX-FAIL
METRICS: METRICS_FAIL（pacing_ok=0；hitch/size/budget/stability=1）
ADOPT: **不采用**（Success≠adopt；不合 main；实验证据可引用但不升采用）
WRITTEN_AT: 2026-11-06 09:45:00 Asia/Shanghai
GATE: docs/m3_matrix_gate_LOCKED.md（workbuddy 宣称；**本机 box 未见 LOCK 文件**；对照 DRAFT=`docs/m3_matrix_gate_DRAFT.md`）
GATE_BODY_SHA256: d8c21d0304931c42d805ceef35fccf4df50b5f2931fd916b81a13eea5db1963f
GATE_MATCH: workbuddy ONELINE **MATCH**（结题**不重算** body；本机无 LOCKED 副本）
APPROVED: 协调派工已进入真机 CLOSED（本结题**不新签** APPROVED；不授权新采数）
BRANCH: claude/m3-matrix
TIP_READ: 9fe1ffe850162b9eb374d4552bba49462894e4df
COMMIT: c0ef236fff3a501bef002fff7db42398f686ed89（results 已 push=OK）
HOST / DISPLAY: primary Philips17S4 1280x1024@75Hz/100% + external VSC-VX2758-2K-PRO 2560x1440@180Hz/150%（count=2）
BUILD: OK；dll sha256=`0d7044c7ed9b6fb494ae73194f5775a8c7bd50e713abf8008f9e25d3a67fb6da`
composition.h: PRE=POST CRLF=`33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97`；LF=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`；**未改**
RESULTS: m3/results_matrix/（workbuddy 交付；本机结题时目录未必同步——以远端 commit 为准）
PERF-CI: HOLD
Success≠adopt: 是

## 授权 / 闸门核验摘要

| 项 | 结果 |
|---|---|
| gate hash（ONELINE） | `d8c21d0304931c42d805ceef35fccf4df50b5f2931fd916b81a13eea5db1963f` |
| MATCH | **是**（workbuddy）；本机无 LOCKED 文件 → **不重算、不改 body** |
| DRAFT 对照 | `docs/m3_matrix_gate_DRAFT.md` 仍存在；LOCK 后真机已跑；结题不把 DRAFT 写成 LOCKED |
| composition.h | PRE=POST；edited=False；**守冻结** |
| 合 main / adopt | **否** |
| 本结题任务 | **纯文档结题 ≠ 采数**；未因 pacing 失败要求重跑 |

## 硬字段对照（ONELINE）

| 字段 | 值 | 裁决 |
|---|---|---|
| sessions | 3×RECORDED | 可裁决会话成立 |
| pacing_ok | **0** | **FAIL**（实测；lab 明确未因该失败重跑） |
| hitch_ok | 1 | PASS |
| size_ok | 1 | PASS |
| budget_ok | 1 | PASS |
| stability_ok | 1 | PASS |
| scale_changes | 4 | 已发生缩放变化（记录） |
| 双屏混刷+混 DPI | 75Hz@100% + 180Hz@150% | 硬件具备；**非** BLOCKED_HW 伪造 |

**票级理由：** 与 `budget_ok` 同形的二进制硬字段 `pacing_ok=0` → 票标签 **`RIVE-MATRIX-FAIL`**。其余判据通过**不**抵消 pacing 失败；亦不把本票升格 PASS/有条件 PASS（除非协调事后书面改判 LOCK 口径——本结题不做）。

## lab 备注（未入库；**必须写入结题**）

1. runner `UseShellExecute=false` 使 `MF_M0_ARTDIR` 生效（原默认 admin 路径 Lh 不可写 → 曾首轮 `INVALID_NO_SUMMARY`）。
2. probe 启动计时器改用 `After()`（本地 `CreateTimer` 被 GC，Tick 不触发）。
3. **仅提交了 results**；`pacing_ok=0` 为实测，**未**因该失败重跑。

## workbuddy ONELINE（核实后结题口径）

```
M3-MATRIX | gate=MATCH d8c21d0304931c42d805ceef35fccf4df50b5f2931fd916b81a13eea5db1963f | tip_read=9fe1ffe850162b9eb374d4552bba49462894e4df | dll=0d7044c7ed9b6fb494ae73194f5775a8c7bd50e713abf8008f9e25d3a67fb6da | build=OK | branch=claude/m3-matrix | display=Philips17S4 1280x1024@75Hz/100% + VSC-VX2758-2K-PRO 2560x1440@180Hz/150% count=2 | sessions=3×RECORDED | pacing_ok=0 hitch_ok=1 size_ok=1 budget_ok=1 stability_ok=1 scale_changes=4 | composition PRE=POST 33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97 (LF a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418 未改) | results=m3/results_matrix/ | commit=c0ef236fff3a501bef002fff7db42398f686ed89 | push=OK | ticket=RIVE-MATRIX-FAIL | METRICS_FAIL(pacing) | 不采用；不合main | Success≠adopt | PERF-CI HOLD
```

## 采用口径

| 项 | 决断 |
|---|---|
| 采用 tip / 合 main | **否** |
| 采用「混刷矩阵已全绿」叙事 | **否**（pacing_ok=0） |
| 引用实验证据（双屏拓扑 + 其余 ok=1 + results commit） | **可引用**；须同时写明 pacing FAIL |
| 解冻 PERF-CI | **否** |
| 改 composition.h | **否** |

## 残差 / 挂账

| 项 | 处置 |
|---|---|
| pacing_ok=0 | 主残差；建议下张 **MATRIX-PACING** 纯文档 DRAFT（勿自动 LOCK） |
| 本机无 LOCKED 副本 | 协调侧归档 LOCK 文件到 docs；结题不补写假 LOCK |
| lab runner/timer 备注 | 已写入本结果；未入库代码说明保留在备注 |
| 工具链 A/B | 仍挂账；不挡 |
| PERF-CI | HOLD |

## 禁令复述

不改 composition.h；不合 main；Success≠adopt；PERF-CI HOLD；不翻 CLOSED；本结题不授权新采数；不把 FAIL 改写成 PASS；不自动 LOCK 下一票。

— notion ai（M3-MATRIX 结题；CLOSED RIVE-MATRIX-FAIL；pacing_ok=0；不采用；不合 main）
