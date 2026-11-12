# M3-MATRIX-BUDGET 门槛闸门（m3_matrix_budget_gate_DRAFT.md）

> **SUPERSEDED**（pidan 正式裁定：2026-11-06；2026-11-10 22:35 AWST 书面再确认）。依据：PACING r4 CLOSED `RIVE-MATRIX-PACING-PASS`（gate `81fc74ebba270feb818646cd5f5c8c90fd2d3812c3447c642ed126413108934c`；code `69094ee`；`budget_ok=1`；gpu_render_p95_frac_T@180Hz≈0.040·T；r3 render 残差已实证消除）。**勿再 LOCK、勿再采数、不授权真机**。正文保留不删，仅作对照溯源；不以本文件作执行权威。PERF-CI 仍 HOLD。见 `docs/m3_matrix_pacing_r4_adoption.md` 与 `docs/Z_SERIES_CONVERGE.md`。
> 文首前全文 sha256（加横幅前）：49bbc9757feeade1f5b571c317e01be2c9d14fb9e464fa98438e5f31dba54745

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

> 起草：notion ai（动效框架执行助手），2026-11-06 11:00:00 Asia/Shanghai（UTC+8）。  
> 依据：`docs/m3_matrix_pacing_next.md`（主推 MATRIX-BUDGET DRAFT）；`docs/m3_matrix_pacing_result.md` / `docs/m3_matrix_pacing_close_pack.md`（CLOSED **RIVE-MATRIX-PACING-FAIL**；**budget_ok=0**；pacing_ok=1 已验证）。  
> 结构对齐：`docs/m3_matrix_pacing_gate_LOCKED.md` / `docs/m3_matrix_pacing_gate_DRAFT.md` / `docs/m3_matrix_gate_DRAFT.md`（G0–G6；开放问题标开放；勿擅自 LOCK）。  
> 前序 PACING（**只读、勿翻**）：gate MATCH `2543e424cd71eea2959fe942ce0bfe7d7118ea6ee5c28b31da7902bce904812c`；approved tip `fd05c6c058022c233f1cd896338e03d665c43d0f`；results `7d5f545c7a3af0b7410421006a3683ce64c93f07` @ `claude/m3-matrix`；sessions=3×RECORDED；**budget_ok=0**；pacing_ok=1；hitch/size/stability=1；scale_changes=4；双屏 75Hz@100% + 180Hz@150% @ DESKTOP-IJI1KJG；节奏修复 `m0r_set_pacing_window` @ `248a2d47e40ea86180a72c8102eab385a7770d52`。  
> 前序 MATRIX（**只读、禁回改标签**）：CLOSED `RIVE-MATRIX-FAIL`；gate MATCH `d8c21d0304931c42d805ceef35fccf4df50b5f2931fd916b81a13eea5db1963f`。  
> lab / 实现备注（只读）：PACING 跑 `run_m3_matrix.ps1 -Pacing`；临时 After()/Temp runner 已还原；**FAIL 照交未重跑**。  
> **本 DRAFT ≠ 授权采数 ≠ 授权真机**；须另走 LOCK → APPROVED（APPROVED_* 非空）后才可采数。  
> 标签建议：`RIVE-MATRIX-BUDGET-*`（族一致；票 id 暂名 **M3-MATRIX-BUDGET** → **开放确认**）。  
> 产物前缀建议：`m3_matrix_budget_*` / `m3matrixbudget_*` / `notion_m3_matrix_budget_*`；结果目录（若日后授权）：**`m3/results_matrix_budget/`**（**勿覆写** `m3/results_matrix_pacing/` / `m3/results_matrix/` 及其它 `m3/results_*`）。

## G0 硬边界
- 不改 `composition.h`（冻结 LF `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`；CRLF `33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97`）；不重开 S1/R-3/R-2；禁影子 IV；PERF-CI 仍 HOLD；不解冻 Bv。
- **STATUS=DRAFT · NOT LOCKED**；未 LOCK 且 APPROVED_* 为空时禁止采数。**本 DRAFT ≠ 授权采数 ≠ 授权真机**；不派 workbuddy；不把本文件写成 LOCKED。
- 成功 ≠ 采用 ≠ 默认可合入 main；不合 main。
- **不翻 / 不回改 CLOSED** `M3-MATRIX`（`RIVE-MATRIX-FAIL`）与 `M3-MATRIX-PACING`（`RIVE-MATRIX-PACING-FAIL`）及其它 CLOSED 旧票；不改其结题/pack/next body；不借本 DRAFT 把 FAIL 洗成 PASS。
- **不**把 pacing_ok=1 / hitch/size/stability=1 写成「整票已绿」；本票刀口仅 **budget**（除非 LOCK 另拍板纳入 pacing 复测）。
- Lab csproj / runner 备注若仍需要：仅本机 Demo 启动前置，**不**顺带合入主线。
- 采数目录（若日后授权）写死建议：`m3/results_matrix_budget/`（与 pacing/matrix 分立；**勿覆写**）→ **开放确认**。
- 硬件：前序已证明 IJI 双屏混刷+混 DPI **具备**；是否强制同拓扑 → **开放给 LOCK**。

## G1 要回答的问题（专攻 budget）
前序 PACING 已在同拓扑下采到：`pacing_ok=1`（节奏修复验证）、hitch/size/stability **通过**，唯 **`budget_ok=0`** 致票级 FAIL。本票只回答：

1. **`budget_ok` 的操作定义是什么？** 混刷（75Hz + 180Hz）下分母 T 取 primary、external、还是双列同形 pacing？CPU/GPU P95 相对 0.35·T 是否仍写死？稳态 vs 跨屏/缩放窗如何切？  
2. **失败机制**：是混刷下 T 定义过苛、跨屏瞬态抬高 P95、节奏修复副作用、探针计时/采样窗，还是真实超预算？  
3. **门槛**：budget 继续作与 PACING 闸门同形的**硬 PASS/FAIL**，还是降为**记录型** / 放宽阈值？若硬：P95 阈值、丢弃样本上限、瞬态豁免 → 须写死。  
4. **复测范围**：是否必须同拓扑（75Hz@100% + 180Hz@150%）≥3 可裁决会话？是否只跑 budget 场景、不重开 pacing 单元格？是否禁止覆写 pacing/matrix results？  
5. **与 pacing 的边界**：pacing_ok=1 已验证 — 本票是否复测 pacing（硬/记录），还是只读旁证、budget 单刀口？节奏修复 tip 是否必含？

## G2 判据（建议骨架；数字多为建议 / 开放）
| 判据 | 建议门槛 | 状态 |
|---|---|---|
| ① **budget 定义可裁决** | LOCK 后正文写死：采样时钟、T 来源（哪块屏 / 双列）、统计窗、`budget_ok` 布尔公式；summary 必带 CPU/GPU P95（及丢弃率）分布 | **开放给 LOCK** |
| ② **budget_ok** | 建议：每个可裁决会话 `budget_ok=1`；建议仍对齐 plan：**CPU P95 ≤ 0.35·T**、**GPU P95 ≤ 0.35·T**、GPU 丢弃样本 ≤ 10%；若降记录型 / 放宽 → 须 LOCK 显式写出 | **开放给 LOCK**（硬 vs 放宽 vs 记录型） |
| ③ **会话数 / 拓扑** | 可裁决 ≥ **3**；建议强制同拓扑 IJI 75Hz@100%+180Hz@150% | **建议**；细节 → **开放给 LOCK** |
| ④ **不破坏前序已过字段** | hitch/size/stability（及若纳入的 pacing）在复测中不得系统性回退到 0（若测到则记录；是否升硬 FAIL → 开放；PACING 闸门先例=升硬） | **建议升硬**；→ **开放给 LOCK** |
| ⑤ **与 pacing/matrix 目录隔离** | 新结果只进 `m3/results_matrix_budget/`；**禁止**覆写 `m3/results_matrix_pacing/` / `m3/results_matrix/`；禁止回改 CLOSED 标签 | **写死建议** |
| ⑥ **节奏修复基线** | tip 是否**必含** `m0r_set_pacing_window`（`248a2d47e40ea86180a72c8102eab385a7770d52`） | **开放给 LOCK** |

- **PASS** ⇔ LOCK 拍板后的硬判据在每个可裁决会话满足。  
- **FAIL** ⇔ 已进入可裁决采数且硬判据未满足（非缺产物）。  
- **INVALID**：缺产物；hash 不符（LOCK 后）；**未授权采数**；改 `composition.h`；影子 IV；覆写 pacing/matrix results；回改 CLOSED 标签；本 DRAFT/未 APPROVED 状态下真机采数。  
- **INCONCLUSIVE**：硬件拓扑相对前序不可复现且 LOCK 允许记 `BLOCKED_HW` —— **开放给 LOCK**。

## G3 裁决建议（**待 pidan / 协调拍板**；本 DRAFT 非最终 LOCK 裁决）
| # | 问题 | 建议裁决 | 理由 |
|---|---|---|---|
| **B1** | 票名 | **建议 `M3-MATRIX-BUDGET`** | next 主推暂名；亦可 `M3-MATRIX-PACING-BUDGET` |
| **B2** | budget 硬门槛 vs 放宽/记录型 | **建议先保持硬 PASS**（CPU/GPU P95≤0.35·T；丢弃≤10%）；若基线证明混刷下过苛，再书面放宽或降记录型 | FAIL 已发生；勿未定义就降级洗票 |
| **B3** | 混刷下 T | **建议双列同形 pacing**：primary 75Hz 与 external 180Hz 各算一列；会话级 `budget_ok` 须两列均过（或 LOCK 改为窗口所在屏单列） | 与 PACING 双列一致，避免混刷掩盖 |
| **B4** | 瞬态豁免 | **建议**对齐 PACING：稳态硬；跨屏/缩放后 N=2.0s 与 M=120 帧取较晚，窗内只记录 | 同拓扑对照 |
| **B5** | 复测拓扑 / 会话 / 范围 | **建议强制同拓扑** IJI；≥3；**只测 budget 场景**；**不**重开整张 MATRIX/PACING 单元格；dir=`m3/results_matrix_budget/` | 吃残差，非翻盘 |
| **B6** | 与 pacing 边界 | **建议 budget 单刀口**；pacing_ok=1 作只读旁证；若复测 pacing 则只记录不挡（除非 LOCK 升硬） | 刀口清晰 |
| **B7** | 节奏修复 tip | **建议 tip 必含** `m0r_set_pacing_window`（`248a2d47e40ea86180a72c8102eab385a7770d52` 或其后代） | APPROVED 已纳入且已验证 |
| **B8** | FAIL 照交 | **建议仍禁自动重跑**；FAIL 照交 | 与 MATRIX/PACING lab 一致 |
| **B9** | 标签前缀 | **建议 `RIVE-MATRIX-BUDGET-*`** | 族一致 |
| **B10** | doc-only 挂账旁路 | **建议否作主路径**；除非 pidan 明示走 H5-ABS 类 doc-only；默认仍走 DRAFT→LOCK→APPROVED→采数 | 残差宜实证；旁路须另票 |

### G3-HOST
| 机台 | 建议角色 | 纳入？ |
|---|---|---|
| **DESKTOP-IJI1KJG**（PACING/MATRIX 主场） | **主测** | **建议写死** |
| DESKTOP-K93N3P8 | 不纳入本残差票 | **建议否** |

## G4 与前序关系（只读）
| 前序 | 状态 | 本票如何消费 |
|---|---|---|
| **M3-MATRIX-PACING** CLOSED `RIVE-MATRIX-PACING-FAIL` | **勿翻**；禁回改标签 | 残差来源；pacing_ok=1 / 拓扑 / 其余 ok 作对照；`m3/results_matrix_pacing/` **只读** |
| **M3-MATRIX** CLOSED `RIVE-MATRIX-FAIL` | **勿翻**；禁回改标签 | 更早残差；`m3/results_matrix/` **只读** |
| DPI / FOLLOW / LOST* / XAML-* | CLOSED；**勿翻** | 不重开；不借本 DRAFT 采数令 |
| PACING LOCKED / APPROVED | 只读 | 本票**不**修改其 body/hash |

## G5 实现（**待写**；成功实现 ≠ LOCK ≠ 授权采数 ≠ 采用）
| 文件（建议） | 内容 | 状态 |
|---|---|---|
| budget 统计 / probe 字段 | 双列（若拍板）CPU/GPU P95；`budget_ok` 公式；与 pacing 分立 | **待写** |
| runner | 输出 → `m3/results_matrix_budget/`；继承 ARTDIR / After()；建议含 pacing_window tip | **待写** |
| 文档 | 本 DRAFT；日后 LOCKED / APPROVED / dispatch **另票** | DRAFT **本文件** |

- 实现可在 LOCK 前另分支推进，但 **成功实现 ≠ 本 DRAFT 已 LOCK ≠ 授权采数 ≠ 采用 ≠ 合 main**。  
- **禁**整 tip 无 PR 合入 main；**禁**覆写 pacing/matrix results。

## G6 禁令
不改 `composition.h`；不重开 S1/R-3/R-2；禁影子 IV；PERF-CI 仍 HOLD；不解冻 Bv；**本 DRAFT ≠ 授权采数 ≠ 授权真机**；成功 ≠ 采用 ≠ 默认可合入 main；不合 main；csproj 不回灌主线；不改任何 LOCKED/CLOSED body；不把本 DRAFT 写成 LOCKED；不派 workbuddy；不翻 / 不回改 CLOSED MATRIX / PACING；不把 FAIL 洗成 PASS；不插队解冻 PERF-CI；**不暗示可采数**。

## G7 ONELINE 模板（草稿）
```
M3-MATRIX-BUDGET | gate=<hash|DRAFT> @ <LOCKED_AT|n/a> | approved=<who|空>@<when|空> | host=<IJI> | topology=<75Hz@100%+180Hz@150%|…> | sessions=<n> | budget_ok=<0/1> | cpu_p95_primary=<…> | gpu_p95_primary=<…> | cpu_p95_external=<…> | gpu_p95_external=<…> | pacing_ok=<旁证|复测> | hitch_ok=<…> | size_ok=<…> | stability_ok=<…> | ticket=<RIVE-MATRIX-BUDGET-PASS|FAIL|INVALID|…> | dir=m3/results_matrix_budget/ | NOT_LOCKED说明若仍DRAFT | 不改composition.h；成功≠采用；不合main；PERF-CI HOLD；不授权真机
```

## 产物与标签约定
| 项 | 约定 |
|---|---|
| 票 id（暂名） | **M3-MATRIX-BUDGET**（**开放确认**） |
| 票标签（建议） | `RIVE-MATRIX-BUDGET-PASS` / `FAIL` / `INVALID` / `INCONCLUSIVE` |
| 文档前缀 | `m3_matrix_budget_*` / `notion_m3_matrix_budget_*` |
| 结果目录（若授权） | **`m3/results_matrix_budget/`**（**勿覆写** pacing/matrix） |

## 继承（写死）
| 项 | 值 |
|---|---|
| 残差来源 | PACING `budget_ok=0`；pacing_ok=1；hitch/size/stability=1；3×RECORDED；scale_changes=4 |
| PACING / MATRIX 票级 | **保持** CLOSED FAIL；不采用；不合 main；**禁回改标签** |
| composition.h | 冻结 LF/CRLF 如上 |
| PERF-CI | **HOLD**；Success≠adopt |
| 本文件 | **NOT LOCKED**；**不授权真机**；APPROVED_* **空** |

## 待 LOCK 开放问题（供 **pidan** 拍板→LOCK；**勿擅自 LOCK**）
1. **票名**：确认 `M3-MATRIX-BUDGET`，或其它（如 `M3-MATRIX-PACING-BUDGET`）？  
2. **budget 失败机制 / 门槛**：混刷双列下 T 取哪块屏？是否与 pacing 双列同形？CPU/GPU P95≤0.35·T 仍写死，还是放宽/改记录型？瞬态豁免是否对齐 N=2.0s / M=120？  
3. **复测范围**：是否强制同拓扑 IJI 75Hz@100%+180Hz@150%？≥3 会话？是否只测 budget、不重开 pacing 单元格？结果目录是否认可 `m3/results_matrix_budget/`？  
4. **与 pacing 边界**：下票是否复测 pacing（硬/记录），还是只读旁证、budget 单刀口？  
5. **节奏修复**：`m0r_set_pacing_window` @ `248a2d47e40ea86180a72c8102eab385a7770d52` 是否要求 tip 必含？  
6. **FAIL 照交**：budget 再 FAIL 是否仍禁止自动重跑？  
7. **标签前缀**：最终 `RIVE-MATRIX-BUDGET-*`？  
8. **doc-only 挂账旁路**：是否允许（对照 H5 ABS）而不采数？若否，主路径是否确认 DRAFT→LOCK→APPROVED→采数？

— notion ai（M3-MATRIX-BUDGET 闸门 DRAFT；STATUS=DRAFT · NOT LOCKED；不授权采数；不授权真机；APPROVED_* 空；PERF-CI HOLD；Success≠adopt；勿擅自 LOCK；不暗示可采数；禁回改 CLOSED MATRIX/PACING）
