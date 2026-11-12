# M3-MATRIX-PACING 门槛闸门（m3_matrix_pacing_gate_LOCKED.md）

STATUS=LOCKED
LOCKED_AT: 2026-11-06 09:55:00 Asia/Shanghai（UTC+8）
GATE_BODY_SHA256: 2543e424cd71eea2959fe942ce0bfe7d7118ea6ee5c28b31da7902bce904812c
> GATE_BODY_SHA256 计算约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。本行写入后不得再改 body。

**本 LOCK 不授权采数；不授权真机。** 须另有 approved 留证（APPROVED_* 非空）后才可采数；构建不采数，可先行。**LOCK ≠ 授权采数 ≠ 授权真机**；**不派 workbuddy**；**不暗示可采数**。

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> 起草 / LOCK：notion ai（动效框架执行助手），2026-11-06 09:55:00 Asia/Shanghai（UTC+8）。  
> 依据：DRAFT `docs/m3_matrix_pacing_gate_DRAFT.md`（sha256=`569695033907ec981b1fb7dddd54571f7f7991228ad370442be8402c172ef427`）；`docs/notion_m3_matrix_next.md`；MATRIX CLOSED `RIVE-MATRIX-FAIL`（`pacing_ok=0`）。  
> 协调转述：pidan 拍板「按建议 1–8 全部 LOCK」。本文件将 1–8 写死；**APPROVED_* 保持空**；**LOCK ≠ 采数**。  
> DRAFT：保留不删；顶部标 SUPERSEDED_BY_LOCKED。  
> 前序 MATRIX（**只读、勿翻**）：gate MATCH `d8c21d0304931c42d805ceef35fccf4df50b5f2931fd916b81a13eea5db1963f`；commit `c0ef236fff3a501bef002fff7db42398f686ed89` @ `claude/m3-matrix`；tip_read `9fe1ffe850162b9eb374d4552bba49462894e4df`；sessions=3×RECORDED；**pacing_ok=0**；hitch/size/budget/stability=1；scale_changes=4；双屏 Philips17S4 1280×1024@**75Hz**/100% + VSC-VX2758-2K-PRO 2560×1440@**180Hz**/150%。  
> lab 备注（只读）：runner `UseShellExecute=false`→`MF_M0_ARTDIR`；probe 用 `After()`；pacing 失败未重跑；仅提交 results。  
> 标签：**`RIVE-MATRIX-PACING-*`**（票 id **M3-MATRIX-PACING**）。  
> 产物前缀：`m3_matrix_pacing_*` / `m3matrixpacing_*` / `notion_m3_matrix_pacing_*`；结果目录（若日后授权）：**`m3/results_matrix_pacing/`**（**勿覆写** `m3/results_matrix/` 及其它 `m3/results_*`）。

## G0 硬边界
- 不改 `composition.h`（冻结 LF `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`；CRLF `33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97`）；不重开 S1/R-3/R-2；禁影子 IV；PERF-CI 仍 HOLD；不解冻 Bv。
- **STATUS=LOCKED**；**LOCK ≠ 采数**；APPROVED_* **空**时禁止采数、禁止真机、禁止派 workbuddy。
- 成功 ≠ 采用 ≠ 默认可合入 main；不合 main。
- **不翻 CLOSED** `M3-MATRIX`（`RIVE-MATRIX-FAIL`）及其它 CLOSED 旧票；不改其结题/pack/next body；**禁止回改 CLOSED MATRIX 标签**；不借本票把 MATRIX FAIL 洗成 PASS。
- **不**把 hitch/size/budget/stability=1 写成「整票已绿」；本票刀口仅 **pacing**。
- Lab csproj / runner 备注若仍需要：仅本机 Demo 启动前置，**不**顺带合入主线。
- 采数目录（若日后授权）写死：`m3/results_matrix_pacing/`（与 `m3/results_matrix/` 分立；**勿覆写**）。
- 硬件写死：**DESKTOP-IJI1KJG**；强制同拓扑 **75Hz@100% + 180Hz@150%**（对照前序 MATRIX）。

## G1 要回答的问题（专攻 pacing；已 LOCK）
前序 MATRIX 已在双屏混刷+混 DPI 下采到：预算/稳态/尺寸/hitch **通过**，唯 **`pacing_ok=0`** 致票级 FAIL。本票只验证：

1. **`pacing_ok` 操作定义（写死）**：帧到达间隔相对**各列目标刷新周期**的节奏误差；混刷下 **双列**（primary 75Hz + external 180Hz）；稳态段硬判；跨屏/缩放瞬态窗内只记录不挡列级 PASS（见 G2）。  
2. **失败机制可观测**：summary 必带双列间隔分布，禁止只有布尔；计时须 `After()` / QPC（或等价非 GC 路径）。  
3. **门槛**：pacing 为与 `budget_ok` 同形的**硬 PASS/FAIL**（**不降记录型**）。  
4. **复测范围**：强制同拓扑；≥3 可裁决会话；**只测 pacing 场景**（稳态 + 至少 1 次跨屏）；**不**重开整张 MATRIX；**不**覆写 `m3/results_matrix/`。  
5. **与 hitch 分立**：hitch=突发卡顿/长帧；pacing=相对目标刷新的节奏误差；字段独立；`hitch_ok=1` **不得**自动置 `pacing_ok=1`。

## G2 判据（写死）
| 判据 | 门槛（写死） |
|---|---|
| ① **pacing 定义可裁决** | 采样时钟：宿主/渲染路径上的帧呈现时刻（QPC 或等价）；**禁止**依赖会被 GC 收回的本地定时器。目标间隔：`T_primary=1000/75 ms`（≈13.333 ms）；`T_external=1000/180 ms`（≈5.556 ms）。统计窗：默认**稳态**；跨屏拖动结束或缩放切换事件结束后进入瞬态豁免窗（见 ⑦）。`pacing_ok` 会话级 =（primary 列过）∧（external 列过）。 |
| ② **pacing_ok（硬）** | 每个可裁决会话 **`pacing_ok=1`**（硬；**不降记录型**）。列级通过：在该列稳态样本上，`P95(|Δt − T|) ≤ 0.25·T` 且 `|Δt − T| > 0.50·T` 的帧占比 ≤ **5%**（瞬态豁免窗内样本**不计入**列级硬判，只写入报告）。 |
| ③ **会话数 / 场景** | 可裁决 ≥ **3**；每会话须含：（a）各屏至少一段稳态；（b）**至少 1 次跨屏**拖动。只测 pacing 场景；**不**重开整张 MATRIX 单元格。 |
| ④ **不破坏前序已过字段** | hitch/size/budget/stability 在复测中若测到：写入报告。任一项系统性回退到 0 → 该会话 **FAIL**（升硬；与 pacing 分立计数）。 |
| ⑤ **探针计时可信** | 计时写死 **`After()` / QPC**（或文档等价、非 GC 可达路径）。使用会被 GC 的本地定时器 → 该会话 **INVALID**。summary **必带**双列分布：至少 P50/P95/max 间隔（或等价直方图）；**只有布尔无分布 → INVALID**。 |
| ⑥ **与 MATRIX 目录隔离** | 新结果只进 **`m3/results_matrix_pacing/`**；**禁止**覆写 `m3/results_matrix/`；禁止把本票产物回标为 MATRIX PASS；**禁止回改 CLOSED MATRIX 标签**。 |
| ⑦ **瞬态豁免窗（N/M 写死）** | 事件：跨屏拖动结束、或缩放切换（DPI scale change）完成。豁免窗 = 事件结束后 **N=2.0 s** 墙钟 **与** 窗口所在屏上 **M=120** 帧，取**较晚**结束者。窗内 pacing 违规 **只记录**，不挡列级硬判。稳态段（窗外）硬。 |

- **composition.h（硬）**：PRE=POST；LF=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`；改动 → INVALID。  
- **PASS** ⇔ 每个可裁决会话满足 ①–⑦ 中硬项：② `pacing_ok=1`、③、⑤分布与计时、⑥目录、composition PRE=POST；④ 无系统性回退到 0。  
- **FAIL** ⇔ 已进入可裁决采数且硬判据未满足（含 `pacing_ok=0`；含 ④ 回退升硬）。  
- **INVALID**：缺产物；GATE hash 不符；**未授权采数**（APPROVED_* 空仍真机）；改 `composition.h`；影子 IV；覆写 `m3/results_matrix/`；回改 CLOSED MATRIX 标签/结题；GC 定时器；只有布尔无分布；拓扑不符强制同拓扑要求却强行采数。  
- **INCONCLUSIVE**：硬件无法复现强制同拓扑且已书面记 `BLOCKED_HW`（须 APPROVED 路径说明；本 LOCK 本身不授权）。

## G3 裁决（写死；对应拍板 1–8）
| # | 问题 | 裁决（写死） |
|---|---|---|
| **P1** | 硬门槛 vs 记录型 | **硬 PASS**（不降记录型） |
| **P2** | 混刷目标间隔 | **双列**：75Hz + 180Hz 均过才会话级 `pacing_ok=1` |
| **P3** | 跨屏/缩放瞬态 | 稳态硬；豁免窗 **N=2.0 s** ∧ **M=120** 帧（取较晚）；窗内只记录 |
| **P4** | 拓扑 / 会话 / 范围 | **强制** IJI 同拓扑 75Hz@100%+180Hz@150%；≥3 可裁决；只测 pacing 场景；目录 `m3/results_matrix_pacing/` |
| **P5** | hitch vs pacing | **分立**；不得互推自动置位 |
| **P6** | 计时 | **After()/QPC**；summary 必带分布；GC 定时器 → **INVALID** |
| **P7** | CLOSED MATRIX 标签 | **禁止回改**；MATRIX 保持 CLOSED `RIVE-MATRIX-FAIL` |
| **P8** | 标签 | **`RIVE-MATRIX-PACING-*`** |

### G3-HOST
| 机台 | 角色 | 纳入？ |
|---|---|---|
| **DESKTOP-IJI1KJG** | **主测（写死）** | **是** |
| DESKTOP-K93N3P8 | 不纳入本残差票 | **否** |

## G4 与前序关系（只读）
| 前序 | 状态 | 本票如何消费 |
|---|---|---|
| **M3-MATRIX** CLOSED `RIVE-MATRIX-FAIL` | **勿翻**；**禁止回改标签** | 残差来源；拓扑/其余 ok=1 对照；`m3/results_matrix/` **只读** |
| DPI / FOLLOW / LOST* / XAML-* | CLOSED；**勿翻** | 不重开；不借本 LOCK 采数令 |
| MATRIX gate DRAFT/LOCKED | 不改其 body/hash | 本票独立 LOCKED |

## G5 实现（成功实现 ≠ 授权采数 ≠ 采用）
| 文件（建议） | 内容 | 状态 |
|---|---|---|
| pacing 统计 / probe 字段 | 双列间隔分布；`pacing_ok` 公式；与 hitch 分立；瞬态窗标记 | **待写**（实现可先行；≠采数授权） |
| runner | 输出 → `m3/results_matrix_pacing/`；继承 ARTDIR / After() | **待写** |
| 文档 | 本 LOCKED；APPROVED / dispatch **另票** | LOCKED **本文件** |

- **禁**整 tip 无 PR 合入 main；**禁**覆写 `m3/results_matrix/`。

## G6 禁令
不改 `composition.h`；不重开 S1/R-3/R-2；禁影子 IV；PERF-CI 仍 HOLD；不解冻 Bv；**本 LOCK ≠ 授权采数 ≠ 授权真机**；成功 ≠ 采用 ≠ 默认可合入 main；不合 main；csproj 不回灌主线；不改任何其它 LOCKED/CLOSED body；不派 workbuddy；不翻 CLOSED MATRIX；不把 FAIL 洗成 PASS；不插队解冻 PERF-CI；**不暗示可采数**；**禁止回改 CLOSED MATRIX 标签**。

## G7 ONELINE 模板
```
M3-MATRIX-PACING | gate=<GATE_BODY_SHA256> @ 2026-11-06 09:55:00 Asia/Shanghai（UTC+8） | approved=<who|空>@<when|空> | host=DESKTOP-IJI1KJG | topology=75Hz@100%+180Hz@150% | sessions=<n> | pacing_ok=<0/1> | pacing_p95_ms_primary=<…> | pacing_p95_ms_external=<…> | hitch_ok=<…> | budget_ok=<…> | size_ok=<…> | stability_ok=<…> | ticket=<RIVE-MATRIX-PACING-PASS|FAIL|INVALID|…> | dir=m3/results_matrix_pacing/ | LOCK≠采数 | 不改composition.h；成功≠采用；不合main；PERF-CI HOLD；不授权真机
```

## 产物与标签约定
| 项 | 约定 |
|---|---|
| 票 id | **M3-MATRIX-PACING** |
| 票标签 | `RIVE-MATRIX-PACING-PASS` / `FAIL` / `INVALID` / `INCONCLUSIVE` |
| 文档前缀 | `m3_matrix_pacing_*` / `notion_m3_matrix_pacing_*` |
| 结果目录（若授权） | **`m3/results_matrix_pacing/`**（**勿覆写** `m3/results_matrix/`） |

## 继承（写死）
| 项 | 值 |
|---|---|
| DRAFT sha | `569695033907ec981b1fb7dddd54571f7f7991228ad370442be8402c172ef427` |
| 残差来源 | MATRIX `pacing_ok=0`；其余 ok=1；3×RECORDED；scale_changes=4 |
| MATRIX 票级 | **保持** CLOSED `RIVE-MATRIX-FAIL`；不采用；不合 main；**禁回改标签** |
| composition.h | 冻结 LF/CRLF 如上 |
| PERF-CI | **HOLD**；Success≠adopt |
| 本文件 | **LOCKED**；**LOCK ≠ 采数**；**不授权真机**；APPROVED_* **空** |

— notion ai（M3-MATRIX-PACING 闸门 LOCKED；LOCK≠采数；不授权真机；APPROVED_* 空；PERF-CI HOLD；Success≠adopt；禁回改 CLOSED MATRIX）
