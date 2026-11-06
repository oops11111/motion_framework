# M3-MATRIX 结题中文包（notion_m3_matrix_pack.md）

> DECISION_AT=2026-11-06 09:45:00 Asia/Shanghai  
> 权威结题：`docs/m3_matrix_result.md`  
> 闸门：workbuddy ONELINE gate=**MATCH** `d8c21d0304931c42d805ceef35fccf4df50b5f2931fd916b81a13eea5db1963f`（**本机 box 未见** `m3_matrix_gate_LOCKED.md`；DRAFT 对照 `docs/m3_matrix_gate_DRAFT.md`）  
> 计划：`docs/m3_plan_DRAFT.md` WP3 / §7 票序 4  
> 前序：DPI / LOST* / FOLLOW / XAML-* **均勿翻**  
> 分支 tip_read=`9fe1ffe850162b9eb374d4552bba49462894e4df`；commit=`c0ef236fff3a501bef002fff7db42398f686ed89` @ **`claude/m3-matrix`**；push=OK  
> composition.h PRE=POST CRLF=`33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97`；LF=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`；**未改**

## 结论

- **票级 = CLOSED**；标签 **`RIVE-MATRIX-FAIL`**。
- **METRICS = METRICS_FAIL**，主因 **`pacing_ok=0`**（实测；lab **未**因失败重跑）。
- **并行通过字段**：`hitch_ok=1` `size_ok=1` `budget_ok=1` `stability_ok=1`；sessions=3×RECORDED；`scale_changes=4`。
- **拓扑**：Philips17S4 1280×1024@**75Hz**/100% + VSC-VX2758-2K-PRO 2560×1440@**180Hz**/150% — 双屏混刷+混 DPI **已具备并采到**，非 BLOCKED_HW 硬凑。
- **是否采用 = 不采用**（Success≠adopt；**不合 main**）。证据可引用，但必须同时写明 pacing FAIL。
- **Success≠adopt**；**PERF-CI HOLD**；composition.h **未触**。

ONELINE（结题）：  
`M3-MATRIX | gate=MATCH d8c21d0304931c42d805ceef35fccf4df50b5f2931fd916b81a13eea5db1963f | tip=9fe1ffe850162b9eb374d4552bba49462894e4df | commit=c0ef236fff3a501bef002fff7db42398f686ed89 @ claude/m3-matrix | dll=0d7044c7ed9b6fb494ae73194f5775a8c7bd50e713abf8008f9e25d3a67fb6da | display=75Hz@100%+180Hz@150% | sessions=3 RECORDED | pacing_ok=0 hitch/size/budget/stability=1/1/1/1 | scale_changes=4 | composition PRE=POST | results=m3/results_matrix/ | ticket=RIVE-MATRIX-FAIL | 不采用；不合main | Success≠adopt | PERF-CI HOLD`

## 口径表

| 选项 | 决断 | 理由 |
|---|---|---|
| RIVE-MATRIX-PASS / 有条件 PASS | **否** | pacing_ok 与 budget_ok 同形二进制字段为 0；lab 保留失败未重跑 |
| RIVE-MATRIX-INCONCLUSIVE / INVALID | **否** | 3×RECORDED；gate MATCH；非缺产物；非未授权形态（协调已 CLOSED 真机） |
| **RIVE-MATRIX-FAIL** | **是** | pacing_ok=0 |
| 采用 tip / 合 main | **否** | Success≠adopt |
| 解冻 PERF-CI / 改 composition.h | **否** | 硬禁 |
| 把其余 ok=1 写成整票 PASS | **否** | 禁止掩盖 pacing |

### 可引用 vs 不采用

| 可引用 | 不采用 / 禁止 |
|---|---|
| 双屏 75+180Hz、100%+150% 拓扑下跑通 3 会话 | 整 tip 默认可合入 main |
| hitch/size/budget/stability 全 1 的旁证 | 「矩阵已全绿 / ship」叙事 |
| commit `c0ef236fff3a501bef002fff7db42398f686ed89` + `m3/results_matrix/` | 静默丢掉 pacing_ok=0 |
| lab runner/timer 修复备注（见下） | 本 pack 授权新采数；自动 LOCK 下票 |

## lab 备注（未入库 → 结题强制记录）

1. runner `UseShellExecute=false` 使 `MF_M0_ARTDIR` 生效（默认 admin 路径不可写 → 曾 `INVALID_NO_SUMMARY`）。
2. probe 启动计时器改用 `After()`（`CreateTimer` 被 GC，Tick 不触发）。
3. 仅提交 results；**pacing_ok=0 为实测，未重跑**。

## 授权核验

| 项 | 值 |
|---|---|
| GATE_BODY_SHA256 | `d8c21d0304931c42d805ceef35fccf4df50b5f2931fd916b81a13eea5db1963f` MATCH（workbuddy） |
| 本机 LOCK 文件 | **未见**（同 K93 结题口径：不伪造 LOCK） |
| composition.h | 未触 |
| 本结题采数 | **否**（纯文档） |

## 产物清单

- `docs/m3_matrix_result.md`（权威）
- `docs/m3_matrix_result_brief.md`
- `docs/notion_m3_matrix_pack.md`（本包）
- `docs/notion_m3_matrix_next.md`
- brief 一行：`docs/m2_mainline_brief.md`
- 只读结果：`m3/results_matrix/`（远端 commit；勿覆写其它 results_*）
- DRAFT 对照：`docs/m3_matrix_gate_DRAFT.md`（不改写成 LOCKED）

## 禁令复述

不改 composition.h；不合 main；Success≠adopt；PERF-CI HOLD；不翻 CLOSED；不授权新采数；不自动 LOCK 下一票；不把 FAIL 洗成 PASS。

## 下一票决断

见 `docs/notion_m3_matrix_next.md`。  
**主推建议：M3-MATRIX-PACING 新 DRAFT（纯文档；NOT LOCK；不授权真机）** — 消化 pacing_ok=0；**勿自动 LOCK**。

— notion ai（M3-MATRIX 结题；CLOSED FAIL；pacing_ok=0；不采用；不合 main）
