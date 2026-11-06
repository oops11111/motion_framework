# M3-MATRIX-PACING 门槛闸门（m3_matrix_pacing_gate_DRAFT.md）

> **SUPERSEDED_BY_LOCKED**：`docs/m3_matrix_pacing_gate_LOCKED.md`
> GATE_BODY_SHA256=`2543e424cd71eea2959fe942ce0bfe7d7118ea6ee5c28b31da7902bce904812c`
> LOCKED_AT=2026-11-06 09:55:00 Asia/Shanghai（UTC+8）
> 本 DRAFT 保留不删；以 LOCKED 为准。DRAFT sha（落盘时）=`569695033907ec981b1fb7dddd54571f7f7991228ad370442be8402c172ef427`。


STATUS=DRAFT（**NOT LOCKED**；本文件**不授权采数**；**不授权真机**；**不派 workbuddy**）
LOCKED_AT: （LOCK 时填）
GATE_BODY_SHA256: （**LOCK 时再算**；DRAFT 不算。约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希）

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> **文首硬声明（写死）**  
> - **STATUS=DRAFT · NOT LOCKED**。  
> - **LOCK ≠ 采数**：即便日后 LOCK，仍须另有 APPROVED_* 非空留证后才可采数。  
> - **本 DRAFT 不授权真机**；不派 workbuddy；不签采数令；**不暗示可采数**。  
> - **APPROVED_BY / APPROVED_AT 现为空**。  
> - Success ≠ adopt ≠ 默认可合入 main；PERF-CI **HOLD**；不合 main；不改 `composition.h`。  
> - **勿擅自 LOCK**；本文件落盘 ≠ LOCK。

> 起草：notion ai（动效框架执行助手），2026-11-06 09:50:00 Asia/Shanghai（UTC+8）。  
> 依据：`docs/notion_m3_matrix_next.md`（主推 MATRIX-PACING DRAFT）；`docs/notion_m3_matrix_pack.md` / `docs/m3_matrix_result.md`（CLOSED **RIVE-MATRIX-FAIL**；`pacing_ok=0`）。  
> 结构对齐：`docs/m3_matrix_gate_DRAFT.md` / `docs/m3_rive_follow_gate_DRAFT.md` / `docs/m3_rive_lost_recover_gate_DRAFT.md`（G0–G6；开放问题标开放；勿擅自 LOCK）。  
> 前序 MATRIX（**只读、勿翻**）：gate MATCH `d8c21d0304931c42d805ceef35fccf4df50b5f2931fd916b81a13eea5db1963f`；commit `c0ef236fff3a501bef002fff7db42398f686ed89` @ `claude/m3-matrix`；tip_read `9fe1ffe850162b9eb374d4552bba49462894e4df`；sessions=3×RECORDED；**pacing_ok=0**；hitch_ok=1；size_ok=1；budget_ok=1；stability_ok=1；scale_changes=4；双屏 Philips17S4 1280×1024@**75Hz**/100% + VSC-VX2758-2K-PRO 2560×1440@**180Hz**/150%。  
> lab 备注（只读）：runner `UseShellExecute=false`→`MF_M0_ARTDIR`；probe 用 `After()`；**pacing 失败未重跑**；仅提交 results。  
> **本 DRAFT ≠ 授权采数 ≠ 授权真机**；须另走 LOCK → APPROVED（APPROVED_* 非空）后才可采数。  
> 标签建议：`RIVE-MATRIX-PACING-*`（族一致；票 id **M3-MATRIX-PACING**）。  
> 产物前缀建议：`m3_matrix_pacing_*` / `m3matrixpacing_*` / `notion_m3_matrix_pacing_*`；结果目录（若日后授权）：**`m3/results_matrix_pacing/`**（**勿覆写** `m3/results_matrix/` 及其它 `m3/results_*`）。

## G0 硬边界
- 不改 `composition.h`（冻结 LF `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`；CRLF `33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97`）；不重开 S1/R-3/R-2；禁影子 IV；PERF-CI 仍 HOLD；不解冻 Bv。
- **STATUS=DRAFT · NOT LOCKED**；未 LOCK 且 APPROVED_* 为空时禁止采数。**本 DRAFT ≠ 授权采数 ≠ 授权真机**；不派 workbuddy；不把本文件写成 LOCKED。
- 成功 ≠ 采用 ≠ 默认可合入 main；不合 main。
- **不翻 CLOSED** `M3-MATRIX`（`RIVE-MATRIX-FAIL`）及其它 CLOSED 旧票；不改其结题/pack/next body；不借本 DRAFT 把 MATRIX FAIL 洗成 PASS。
- **不**把 hitch/size/budget/stability=1 写成「整票已绿」；本票刀口仅 **pacing**。
- Lab csproj / runner 备注若仍需要：仅本机 Demo 启动前置，**不**顺带合入主线。
- 采数目录（若日后授权）写死：`m3/results_matrix_pacing/`（与 `m3/results_matrix/` 分立；**勿覆写**）。
- 硬件：前序 MATRIX 已证明该机双屏混刷+混 DPI **具备**；本票默认同拓扑复用（是否强制同机同拓扑 → **开放给 LOCK**）。

## G1 要回答的问题（专攻 pacing）
前序 MATRIX 已在双屏混刷+混 DPI 下采到：预算/稳态/尺寸/hitch **通过**，唯 **`pacing_ok=0`** 致票级 FAIL。本票只回答：

1. **`pacing_ok` 的操作定义是什么？** 帧到达间隔相对哪块屏的刷新周期？混刷（75Hz + 180Hz）下分母 T / 目标间隔取 primary、external、还是双列？跨屏拖动段与单屏稳态段是否分开算？  
2. **失败机制**：是呈现节奏相对目标刷新偏慢/偏快、掉帧簇、跨屏切换瞬态、缩放切换（scale_changes=4）叠加，还是探针/计时实现假阴性（对照 lab：`After()` 替换 `CreateTimer`）？  
3. **门槛**：pacing 继续作与 `budget_ok` 同形的**硬 PASS/FAIL**，还是降为**记录型**（不挡票级 PASS）？若硬：P95/max 间隔误差、连续违规窗、每会话是否允许瞬态豁免 → 须写死。  
4. **复测范围**：是否必须同拓扑（75Hz@100% + 180Hz@150%）≥3 可裁决会话？是否只跑 pacing 相关单元格、不重开整张 MATRIX 矩阵？是否禁止重跑已 CLOSED 的 MATRIX results 目录？  
5. **与 hitch 的边界**：`hitch_ok=1` 已过时，pacing 失败是否允许解释为「无 hitch 但仍不达标节奏」——定义上二者如何分立，避免双重计数或互相覆盖。

## G2 判据（建议骨架；数字多为建议 / 开放）
| 判据 | 建议门槛 | 状态 |
|---|---|---|
| ① **pacing 定义可裁决** | LOCK 后正文写死：采样时钟、目标间隔来源（哪块屏 / 双列）、统计窗（稳态 vs 跨屏）、`pacing_ok` 布尔计算公式 | **开放给 LOCK**（本 DRAFT 不假装已定义） |
| ② **pacing_ok** | 建议：每个可裁决会话 `pacing_ok=1`；若降记录型 → 只写入报告不挡 PASS | **开放给 LOCK**（硬 vs 记录型） |
| ③ **会话数** | 可裁决 ≥ **3**（建议同拓扑；粒度 → 开放） | **建议**；细节 → **开放给 LOCK** |
| ④ **不破坏前序已过字段** | hitch/size/budget/stability 在复测中不得系统性回退到 0（若测到则记录；是否升硬 FAIL → 开放） | **建议记录**；升硬 → **开放给 LOCK** |
| ⑤ **探针计时可信** | 使用非 GC 不可达计时器（对齐 lab `After()`）；summary 含 pacing 原始分布（P50/P95/max 间隔或等价），禁止只有布尔无分布 | **建议硬**；字段表 → **开放给 LOCK** |
| ⑥ **与 MATRIX 目录隔离** | 新结果只进 `m3/results_matrix_pacing/`；**禁止**覆写 `m3/results_matrix/`；禁止把本票产物回标为 MATRIX PASS | **写死建议** |

- **PASS** ⇔ LOCK 拍板后的硬判据（建议至少 ①⑤ + 拍板后的 ②③）在每个可裁决会话满足；若 ② 定为记录型，则 PASS 不以 pacing_ok=1 为必要条件（须在 LOCK 正文显式写出）。  
- **FAIL** ⇔ 已进入可裁决采数且硬判据未满足（非缺产物）。  
- **INVALID**：缺产物；hash 不符（LOCK 后）；**未授权采数**；改 `composition.h`；影子 IV；覆写 `m3/results_matrix/`；翻 CLOSED MATRIX 结题改判；本 DRAFT/未 APPROVED 状态下真机采数；只有布尔无分布却宣称 pacing 可裁决。  
- **INCONCLUSIVE**：硬件拓扑相对前序不可复现且 LOCK 允许记 `BLOCKED_HW` —— **开放给 LOCK**。

## G3 裁决建议（**待 pidan / 协调拍板**；本 DRAFT 非最终 LOCK 裁决）
| # | 问题 | 建议裁决 | 理由 |
|---|---|---|---|
| **P1** | pacing 硬门槛还是记录型 | **建议先硬 PASS**（与 MATRIX 原 `pacing_ok` 同形）；若基线证明定义过苛，再书面降记录型 | FAIL 已发生；勿未定义就降级洗票 |
| **P2** | 混刷下目标间隔 | **建议双列报告**：primary 75Hz 与 external 180Hz 各一列 pacing 统计；会话级 `pacing_ok` 须两列均过（或 LOCK 改为「窗口所在屏」单列） | 混刷是 MATRIX 刀口；单列易掩盖 |
| **P3** | 跨屏 / 缩放瞬态豁免 | **建议**：稳态段硬；跨屏拖动后 N 秒 / M 帧窗内违规只记录；缩放切换（对照 scale_changes=4）同理 | 避免瞬态单独打死；须写死 N/M |
| **P4** | 复测拓扑 | **建议强制同拓扑** 75Hz@100% + 180Hz@150%（IJI 已具备）；换拓扑须新 APPROVED 说明 | 对照前序 FAIL |
| **P5** | 会话数 / 是否整矩阵重开 | **建议 ≥3 可裁决**；**只测 pacing 场景**（稳态+至少 1 次跨屏），**不**重开整张 MATRIX 单元格爆炸；**不**覆写 matrix results | 吃残差，非翻盘 |
| **P6** | hitch vs pacing | **建议分立**：hitch=突发卡顿/长帧；pacing=相对目标刷新的节奏误差；二者字段独立；不得用 hitch_ok=1 自动置 pacing_ok=1 | 前序已出现 hitch=1 且 pacing=0 |
| **P7** | 计时实现 | **建议写死** 禁止依赖会被 GC 的本地定时器；沿用 lab `After()` 或等价宿主/QPC 路径；summary 必带分布 | lab 已踩坑 |
| **P8** | 标签前缀 | **建议 `RIVE-MATRIX-PACING-*`** | 族一致 |
| **P9** | 是否允许「改判 MATRIX 为有条件 PASS」 | **本票默认否**；MATRIX 保持 CLOSED FAIL；本票独立 CLOSED 后也不回写 MATRIX 标签，除非协调另开 doc-only 改判票 | Success≠洗票 |

### G3-HOST
| 机台 | 建议角色 | 纳入？ |
|---|---|---|
| **DESKTOP-IJI1KJG**（前序 MATRIX 主场；双屏已具备） | **主测** | **建议写死**（LOCK 时可确认） |
| DESKTOP-K93N3P8 | 不纳入本残差票 | **建议否**（工具链挂账不挡；非本票刀口） |

## G4 与前序关系（只读）
| 前序 | 状态 | 本票如何消费 |
|---|---|---|
| **M3-MATRIX** CLOSED `RIVE-MATRIX-FAIL` | **勿翻**；勿改判为 PASS | 残差来源；拓扑/其余 ok=1 作对照；results_matrix **只读** |
| DPI / FOLLOW / LOST* / XAML-* | CLOSED；**勿翻** | 不重开；不借本票采数令 |
| MATRIX DRAFT / LOCKED | DRAFT 仍在 docs；LOCKED 本机或未见 | 本票**不**修改 MATRIX gate body/hash |

## G5 实现（**待写**；成功实现 ≠ LOCK ≠ 授权采数 ≠ 采用）
| 文件（建议） | 内容 | 状态 |
|---|---|---|
| pacing 统计 / probe 字段 | 双列间隔分布；`pacing_ok` 公式；与 hitch 分立 | **待写** |
| runner | 输出 → `m3/results_matrix_pacing/`；继承 ARTDIR / After() 备注 | **待写** |
| 文档 | 本 DRAFT；日后 LOCKED / APPROVED / dispatch **另票** | DRAFT **本文件** |

- 实现可在 LOCK 前另分支推进，但 **成功实现 ≠ 本 DRAFT 已 LOCK ≠ 授权采数 ≠ 采用 ≠ 合 main**。  
- **禁**整 tip 无 PR 合入 main；**禁**覆写 `m3/results_matrix/`。

## G6 禁令
不改 `composition.h`；不重开 S1/R-3/R-2；禁影子 IV；PERF-CI 仍 HOLD；不解冻 Bv；**本 DRAFT ≠ 授权采数 ≠ 授权真机**；成功 ≠ 采用 ≠ 默认可合入 main；不合 main；csproj 不回灌主线；不改任何 LOCKED/CLOSED body；不把本 DRAFT 写成 LOCKED；不派 workbuddy；不翻 CLOSED MATRIX；不把 FAIL 洗成 PASS；不插队解冻 PERF-CI；**不暗示可采数**。

## G7 ONELINE 模板（草稿）
```
M3-MATRIX-PACING | gate=<hash|DRAFT> @ <LOCKED_AT|n/a> | approved=<who|空>@<when|空> | host=<IJI> | topology=<75Hz@100%+180Hz@150%|…> | sessions=<n> | pacing_ok=<0/1> | pacing_p95_ms_primary=<…> | pacing_p95_ms_external=<…> | hitch_ok=<…> | budget_ok=<…> | ticket=<RIVE-MATRIX-PACING-PASS|FAIL|INVALID|…> | dir=m3/results_matrix_pacing/ | NOT_LOCKED说明若仍DRAFT | 不改composition.h；成功≠采用；不合main；PERF-CI HOLD；不授权真机
```

## 产物与标签约定
| 项 | 约定 |
|---|---|
| 票 id | **M3-MATRIX-PACING** |
| 票标签（建议） | `RIVE-MATRIX-PACING-PASS` / `FAIL` / `INVALID` / `INCONCLUSIVE` |
| 文档前缀 | `m3_matrix_pacing_*` / `notion_m3_matrix_pacing_*` |
| 结果目录（若授权） | **`m3/results_matrix_pacing/`**（**勿覆写** `m3/results_matrix/`） |

## 继承（写死）
| 项 | 值 |
|---|---|
| 残差来源 | MATRIX `pacing_ok=0`；其余 ok=1；3×RECORDED；scale_changes=4 |
| MATRIX 票级 | **保持** CLOSED `RIVE-MATRIX-FAIL`；不采用；不合 main |
| composition.h | 冻结 LF/CRLF 如上 |
| PERF-CI | **HOLD**；Success≠adopt |
| 本文件 | **NOT LOCKED**；**不授权真机**；APPROVED_* **空** |

## 待 LOCK 开放问题（供 **pidan** 拍板→LOCK；**勿擅自 LOCK**）
1. **`pacing_ok` 操作定义**：采样时钟、目标间隔、稳态/跨屏/缩放窗、布尔公式；summary 必带哪些分布字段？  
2. **硬门槛还是记录型**？若硬：P95/max 误差与连续违规阈值写多少？瞬态豁免 N 秒 / M 帧？  
3. **混刷双列 vs 窗口所在屏单列**：会话级 `pacing_ok` 如何合成？  
4. **复测是否强制同拓扑**（75Hz@100% + 180Hz@150%）？换拓扑是否另批？  
5. **可裁决会话数**与是否包含「至少一次跨屏」硬要求？  
6. **hitch 与 pacing 字段分立**的正式定义；复测中 hitch/size/budget/stability 回退是否升硬 FAIL？  
7. **计时实现**写死 `After()`/QPC 还是允许其它等价？GC 定时器是否列为 INVALID？  
8. **是否禁止任何对 CLOSED MATRIX 标签的改判**（建议禁止；另开 doc-only 票才可议）？  
9. **标签前缀**最终 `RIVE-MATRIX-PACING-*`？结果目录名是否认可 `m3/results_matrix_pacing/`？  
10. **INVALID vs FAIL** 边界（无分布、未授权采数、覆写 matrix results、拓扑不符）是否再细化？

— notion ai（M3-MATRIX-PACING 闸门 DRAFT；STATUS=DRAFT · NOT LOCKED；不授权采数；不授权真机；APPROVED_* 空；PERF-CI HOLD；Success≠adopt；勿擅自 LOCK；不暗示可采数）
