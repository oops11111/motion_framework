# M3-MATRIX-RENDER-BUDGET 门槛闸门（m3_matrix_render_budget_gate_DRAFT.md）

> **SUPERSEDED**（pidan 正式裁定：2026-11-06；2026-11-10 22:35 AWST 书面再确认）。依据：PACING r4 CLOSED `RIVE-MATRIX-PACING-PASS`（gate `81fc74ebba270feb818646cd5f5c8c90fd2d3812c3447c642ed126413108934c`；code `69094ee`；`budget_ok=1`；gpu_render_p95_frac_T@180Hz≈0.040·T；r3 render 残差已实证消除）。**勿再 LOCK、勿再采数、不授权真机**。正文保留不删，仅作对照溯源；不以本文件作执行权威。PERF-CI 仍 HOLD。见 `docs/m3_matrix_pacing_r4_adoption.md` 与 `docs/Z_SERIES_CONVERGE.md`。
> 文首前全文 sha256（加横幅前）：e10bf9e5bf2944f13faec600a2db1de914d214a55025714ecc921fb9550b5dd1

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
> - 时区口径：文档时间戳按 **Australia/Perth（AWST，UTC+8）**。

> 起草：notion ai（动效框架执行助手），2026-11-06 11:30:10 AWST（Australia/Perth（AWST，UTC+8））。  
> 依据：`docs/m3_matrix_pacing_r3_next.md`（主推 A：RENDER-BUDGET / GPU-RENDER 新 DRAFT）；`docs/m3_matrix_pacing_r3_result.md` / close_pack（CLOSED **RIVE-MATRIX-PACING-FAIL**；**budget_ok=0**；pacing_ok=1；刀口在 **gpu_render**）。  
> 结构对齐：`docs/m3_matrix_budget_gate_DRAFT.md` / `docs/m3_matrix_pacing_r3_gate_DRAFT.md`（G0–G6；开放问题标开放；勿擅自 LOCK）。  
> 前序 **r3**（**只读、勿翻**）：gate MATCH `90f9356bf2739ea2a95a56619fc73af561b0c34e4ebd097be05442f8c5719476` @ 2026-11-06 11:14:15 AWST；approved pidan@2026-11-06 11:15:19 AWST；tip `82eacb0b59e1bab7ee809b4b62c8a100545b4073`；results `39cdcf42e7758555b223c8058cc8a2e9e3793aff` → `m3/results_matrix_pacing_r3/`；sessions=3×RECORDED；**pacing_ok=1**；**budget_ok=0**；hitch/size/stability=1；scale_changes=4；probe=M3-MATRIX-PACING **v3**；pacing_source=**vblank**（三会话 MATCH）；pacing_p95_ms primary=0.060/0.065/0.064；external=0.111/0.107/0.114；**gpu_render_p95_frac_T(180Hz seg1/3)≈1.05–1.08**（超 **0.35·T**）；**gpu_copy_p95_ms≈0.008**（可忽略）；代码 `878892d`（只等 WaitForVBlank；GPU 拆分 render/copy；预算按 render）；双屏 75Hz@100%+180Hz@150% @ DESKTOP-IJI1KJG；FAIL 照交未重跑。  
> 前序 **r2**（**只读、禁回改**）：CLOSED FAIL；pacing_ok=0；budget_ok=1；gate `a1fa4590285f52948ecf51bc9c32629d4d2c3bc42efc7f5bb4e4922bd2be2883`；results → `m3/results_matrix_pacing_r2/`；probe v2；pacing_source=vblank+dwmflush。  
> 前序 **r1**（**只读、禁回改**）：CLOSED FAIL；pacing_ok=1；budget_ok=0；gate `2543e424cd71eea2959fe942ce0bfe7d7118ea6ee5c28b31da7902bce904812c`；results → `m3/results_matrix_pacing/`（合计 GPU 含等 DWM）。  
> 前序 **MATRIX**（**只读、禁回改**）：CLOSED `RIVE-MATRIX-FAIL`；gate MATCH `d8c21d0304931c42d805ceef35fccf4df50b5f2931fd916b81a13eea5db1963f`。  
> **三票对照（禁洗票）**：r1 pacing✓/budget✗；r2 pacing✗/budget✓；r3 pacing✓/budget✗（失败在 **render**，非 copy）。三票各自 CLOSED FAIL；**禁止**合并 r1+r2+r3 字段洗成 PASS。  
> **BUDGET DRAFT 挂账**：已落盘 `docs/m3_matrix_budget_gate_DRAFT.md`（sha `49bbc9757feeade1f5b571c317e01be2c9d14fb9e464fa98438e5f31dba54745`）。本续票 DRAFT 将既有 BUDGET DRAFT 记为 **挂账/对照或待 SUPERSEDED**（除非 pidan 另拍板仍 LOCK 原预算口径）；**本票专吃 render 预算残差**。  
> **pacing 残差相对 r2**：r3 pacing_ok=1 + pacing_source=vblank + p95≪T → **可挂账闭合**（只读旁证）；**不**因此把本票或三轮升 PASS。  
> **本 DRAFT ≠ 授权采数 ≠ 授权真机**。  
> 票 id **暂名 `M3-MATRIX-RENDER-BUDGET`**（开放可改为 `M3-MATRIX-GPU-RENDER` 等）。  
> 标签建议：`RIVE-MATRIX-RENDER-*`（**开放确认**）。  
> 结果目录建议：`m3/results_matrix_render_budget/`（**勿覆写** r1/r2/r3/matrix/budget）。

## G0 硬边界
- 不改 `composition.h`（冻结 LF `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`；CRLF `33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97`）；不重开 S1/R-3/R-2；禁影子 IV；PERF-CI 仍 HOLD；不解冻 Bv。
- **STATUS=DRAFT · NOT LOCKED**；未 LOCK 且 APPROVED_* 为空时禁止采数。**本 DRAFT ≠ 授权采数 ≠ 授权真机**；不派 workbuddy；不把本文件写成 LOCKED。
- 成功 ≠ 采用 ≠ 默认可合入 main；不合 main。
- **不翻 / 不回改 CLOSED** MATRIX / PACING r1 / PACING r2 / PACING r3 及其它 CLOSED；不改其结题/pack/next body；不借本 DRAFT 把 FAIL 洗成 PASS；**禁止**合并 r1+r2+r3 字段洗票。
- **不**把 r3 的 pacing_ok=1 / hitch/size/stability=1 写成「整票已绿」；本票刀口仅 **render 预算**（除非 LOCK 另拍板硬复测 pacing）。
- Lab csproj / runner 备注若仍需要：仅本机 Demo 启动前置，**不**顺带合入主线。
- 采数目录（若日后授权）建议：`m3/results_matrix_render_budget/`（与 r1/r2/r3/matrix/budget 分立；**勿覆写**）→ **开放确认**。
- 硬件：IJI 双屏混刷+混 DPI 已具备（primary_PHL 1280×1024@75Hz/100% + external_VSC 2560×1440@180Hz/150%）；是否强制同拓扑 → **开放给 LOCK**。
- **BUDGET DRAFT**：`docs/m3_matrix_budget_gate_DRAFT.md` **挂账/对照或待 SUPERSEDED**；本票不自动 LOCK 原预算口径，亦不自动把它标 SUPERSEDED。
- **pacing**：相对 r2 残差可挂账闭合；本票默认只读旁证（r3 pacing_ok=1）。

## G1 要回答的问题（专攻 render 预算残差）
r3 在 vblank-only 出帧 + GPU 拆分 render/copy 路径下采到：`pacing_ok=1`，唯 **`budget_ok=0`**，且 **`gpu_render_p95_frac_T@180Hz≈1.05–1.08`（超 0.35·T）**、`gpu_copy≈0.008` 致票级 FAIL。本票只回答：

1. **`budget_ok` / render 计费的操作定义是什么？** 是否写死「预算只计 render、不计 copy / 不计 WaitForVBlank」？混刷下 T 取 primary 75Hz、external 180Hz、还是双列同形？稳态 vs 跨屏/缩放窗如何切？  
2. **失败机制**：r3 `gpu_render_p95_frac_T@180Hz≈1.05–1.08` 是真超预算、探针归因/采样窗、混刷双列合成过严，还是 0.35·T 相对 180Hz（T≈5.56ms）过苛？copy≈0 是否足以排除「等 DWM/copy」归因？  
3. **门槛**：保持 **0.35·T** 硬 PASS/FAIL，还是书面调整（放宽 / 改记录型 / 改双列规则）？若硬：P95 阈值、丢弃样本上限、瞬态豁免 → 须写死。  
4. **复测范围**：是否必须同拓扑（75Hz@100% + 180Hz@150%）≥3 可裁决会话？是否只跑 render-budget 场景、不重开 pacing 单元格？dir=`m3/results_matrix_render_budget/`？  
5. **与 pacing / 既有 BUDGET DRAFT 的边界**：pacing 只读旁证还是硬复测？既有 BUDGET DRAFT 挂账 / SUPERSEDED / 仍并行？

## G2 判据（建议骨架；数字多为建议 / 开放）
| 判据 | 建议门槛 | 状态 |
|---|---|---|
| ① **render-budget 定义可裁决** | LOCK 后正文写死：采样时钟、T 来源（双列 75Hz+180Hz）、统计窗、`budget_ok` 布尔公式；summary 必带 **gpu_render** P95 / frac_T（及丢弃率）；copy 作旁证字段 | **开放给 LOCK** |
| ② **budget_ok（render）** | 建议：每个可裁决会话 `budget_ok=1`；建议仍对齐：**gpu_render P95 ≤ 0.35·T**（相对各列 T；刀口尤重 180Hz）；GPU 丢弃 ≤ 10%；若降记录型 / 放宽 → 须 LOCK 显式写出 | **开放给 LOCK**（硬 vs 放宽 vs 记录型） |
| ③ **计费口径** | 建议写死：**只计 render**；copy 旁证可忽略（r3≈0.008）；不计 WaitForVBlank 等待 | **建议**；→ **开放给 LOCK** |
| ④ **会话数 / 拓扑** | ≥ **3**；建议强制同拓扑 IJI 75Hz@100%+180Hz@150% | **建议**；→ **开放** |
| ⑤ **探针 / pacing_source** | 建议基线：probe **v3** + `pacing_source=vblank`；seg MATCH **建议升硬**（缺 → INVALID） | **开放给 LOCK** |
| ⑥ **pacing** | 建议 **只读旁证**（r3=1）；若硬复测回退到 0 → 是否升硬 FAIL **开放** | **开放给 LOCK** |
| ⑦ **目录隔离** | 只进 `m3/results_matrix_render_budget/`；禁覆写 r1/r2/r3/matrix/budget；禁回改 CLOSED 标签 | **写死建议** |
| ⑧ **出帧 / 拆分基线** | tip 是否必含 WaitForVBlank-only + GPU render/copy 拆分（`878892d` 或其后代） | **建议必含**；→ **开放** |

- **PASS** ⇔ LOCK 拍板后硬判据均满足。  
- **FAIL** ⇔ 可裁决采数且硬判据未满足。  
- **INVALID**：缺产物；未授权采数；改 `composition.h`；影子 IV；覆写 r1/r2/r3/matrix/budget；回改 CLOSED；DRAFT/未 APPROVED 真机采数；缺合法 probe/source（若 LOCK 升硬）。  
- **INCONCLUSIVE**：`BLOCKED_HW` —— **开放给 LOCK**。

## G3 裁决建议（**待 pidan 拍板**；本 DRAFT 非最终 LOCK）
| # | 问题 | 建议裁决 | 理由 |
|---|---|---|---|
| **RB1** | 票名 | **暂名 `M3-MATRIX-RENDER-BUDGET`**；可选 `M3-MATRIX-GPU-RENDER` | next 主推 A；开放确认 |
| **RB2** | 与 BUDGET DRAFT | **挂账/对照或待 SUPERSEDED**；不自动 SUPERSEDED，除非 pidan 明示；**不**并行抢 LOCK 原口径 | 刀口收窄到 render；避免双票抢锁 |
| **RB3** | 0.35·T 门槛 | **建议先保持硬门槛**并书面解释 r3 render frac≈1.05–1.08；若证明过苛再放宽 | 勿未定义洗票 |
| **RB4** | 计费口径 | **建议只计 render**；copy 旁证可忽略 | r3 已排除 copy 归因 |
| **RB5** | 探针基线 | **建议唯一合法**：v3 + pacing_source=vblank；seg MATCH 升硬 | r3 可裁决路径 |
| **RB6** | 复测 | **强制同拓扑 IJI**；≥3；只测 render-budget；dir=`m3/results_matrix_render_budget/` | 吃残差 |
| **RB7** | pacing | **只读旁证**（r3=1）；不挡本票 PASS（除非 LOCK 升硬复测） | pacing 相对 r2 可挂账闭合 |
| **RB8** | FAIL 照交 | **仍禁自动重跑** | 与 r1/r2/r3 一致 |
| **RB9** | 标签 | **建议 `RIVE-MATRIX-RENDER-*`** | 与刀口对齐；开放确认 |
| **RB10** | doc-only 挂账 | **建议否作主路径**；须 pidan 明示 | 残差宜实证 |
| **RB11** | APPROVED_* | **保持空**直至 LOCK 后另拍板 APPROVED | LOCK≠采数 |

### G3-HOST
| 机台 | 建议角色 | 纳入？ |
|---|---|---|
| **DESKTOP-IJI1KJG** | **主测** | **建议写死** |
| DESKTOP-K93N3P8 | 不纳入 | **建议否** |

## G4 与前序关系（只读）
| 前序 | 状态 | 本票如何消费 |
|---|---|---|
| PACING **r3** CLOSED FAIL | **勿翻** | 残差来源；gpu_render@180Hz / copy≈0 / pacing=1 对照；`results_matrix_pacing_r3/` **只读** |
| PACING **r2** CLOSED FAIL | **勿翻**；禁回改 | 字段对偶挂账（budget=1 被 r3 否定作整线结论）；`results_matrix_pacing_r2/` **只读** |
| PACING **r1** CLOSED FAIL | **勿翻**；禁回改 | 合计 GPU 时代挂账；`results_matrix_pacing/` **只读** |
| MATRIX CLOSED FAIL | **勿翻**；禁回改 | 只读 |
| **M3-MATRIX-BUDGET** DRAFT | **挂账/对照或待 SUPERSEDED** | 不自动 LOCK；优先级让位于本 render 残差票，除非另拍板 |
| PACING-R3 DRAFT | 续票入口已 CLOSED | **勿擅自再 LOCK 同票** |

## G5 实现（**待写**；成功实现 ≠ LOCK ≠ 授权采数）
| 文件（建议） | 内容 | 状态 |
|---|---|---|
| render-budget 统计 / probe v3 | 双列 gpu_render P95/frac_T；copy 旁证；`budget_ok` 公式；source=vblank | **待写/沿用** |
| runner | 输出 → `m3/results_matrix_render_budget/`；出帧基线含 WaitForVBlank-only + render/copy 拆分 | **待写** |
| 文档 | 本 DRAFT；LOCKED / APPROVED / dispatch **另票** | DRAFT **本文件** |

- **禁**整 tip 无 PR 合入 main；**禁**覆写 r1/r2/r3/matrix/budget results。

## G6 禁令
不改 `composition.h`；不重开 S1/R-3/R-2；禁影子 IV；PERF-CI 仍 HOLD；不解冻 Bv；**本 DRAFT ≠ 授权采数 ≠ 授权真机**；成功 ≠ 采用 ≠ 默认可合入 main；不合 main；csproj 不回灌主线；不改任何 LOCKED/CLOSED body；不把本 DRAFT 写成 LOCKED；不派 workbuddy；不翻 / 不回改 CLOSED MATRIX / r1 / r2 / r3；不合并洗票；不插队解冻 PERF-CI；**不暗示可采数**；不自动 LOCK / SUPERSEDED 既有 BUDGET DRAFT。

## G7 ONELINE 模板（草稿）
```
M3-MATRIX-RENDER-BUDGET | gate=<hash|DRAFT> @ <LOCKED_AT|n/a> | approved=<who|空>@<when|空> | host=DESKTOP-IJI1KJG | topology=75Hz@100%+180Hz@150% | sessions=<n> | budget_ok=<0/1> | gpu_render_p95_frac_T_180Hz=<…> | gpu_copy_p95_ms=<…> | pacing_ok=<旁证|复测> | hitch_ok=<…> | size_ok=<…> | stability_ok=<…> | probe=M3-MATRIX-PACING v3 | pacing_source=vblank | ticket=<RIVE-MATRIX-RENDER-PASS|FAIL|INVALID|…> | dir=m3/results_matrix_render_budget/ | NOT_LOCKED说明若仍DRAFT | 不改composition.h；成功≠采用；不合main；PERF-CI HOLD；不授权真机
```

## 产物与标签约定
| 项 | 约定 |
|---|---|
| 票 id（暂名） | **M3-MATRIX-RENDER-BUDGET**（开放可改为 `M3-MATRIX-GPU-RENDER` 等） |
| 票标签（建议） | `RIVE-MATRIX-RENDER-PASS` / `FAIL` / `INVALID` / `INCONCLUSIVE` |
| 文档前缀 | `m3_matrix_render_budget_*` |
| 结果目录（若授权） | **`m3/results_matrix_render_budget/`**（勿覆写 r1/r2/r3/matrix/budget） |

## 继承（写死）
| 项 | 值 |
|---|---|
| 残差来源 | r3 `budget_ok=0`；pacing_ok=1；**gpu_render_p95_frac_T@180Hz≈1.05–1.08**；gpu_copy≈0.008；probe v3；pacing_source=vblank |
| r1 / r2 / r3 / MATRIX | **保持** CLOSED FAIL；禁回改；禁合并洗票 |
| BUDGET DRAFT | **挂账/对照或待 SUPERSEDED** |
| pacing 相对 r2 | **可挂账闭合**（只读旁证）；不升三轮 PASS |
| composition.h | 冻结 LF/CRLF 如上 |
| PERF-CI | **HOLD**；Success≠adopt |
| 本文件 | **NOT LOCKED**；**不授权真机**；APPROVED_* **空** |

## 待 LOCK 开放问题（供 **pidan** 拍板→LOCK；**勿擅自 LOCK**）
1. **票名**：确认 `M3-MATRIX-RENDER-BUDGET` / `M3-MATRIX-GPU-RENDER` / 其它？  
2. **与 `docs/m3_matrix_budget_gate_DRAFT.md`**：继续挂账对照 / 标 SUPERSEDED 由本票取代 / 仍并行 LOCK 原预算口径？  
3. **0.35·T 门槛**：相对 180Hz T≈5.56ms，保持硬门槛还是书面调整？r3 `gpu_render_p95_frac_T≈1.05–1.08` 如何解释？  
4. **计费口径**：是否写死「只计 render、不计 copy / 不计 WaitForVBlank」？copy≈0 是否升硬旁证？  
5. **探针基线**：v3 + `pacing_source=vblank` 是否唯一合法？seg MATCH 是否升硬？  
6. **复测**：强制同拓扑 IJI？≥3？dir=`m3/results_matrix_render_budget/`？是否只测 render-budget？  
7. **pacing**：只读旁证（r3 pacing_ok=1）还是硬复测？  
8. **FAIL 照交**：是否仍禁自动重跑？  
9. **标签前缀**：最终 `RIVE-MATRIX-RENDER-*`？  
10. **是否允许 doc-only 挂账**（对照 H5 ABS）而不采数？  
11. **APPROVED_***：确认保持空直至 LOCK 后另拍板；LOCK≠采数；本 DRAFT **不**填 APPROVED。

— notion ai（M3-MATRIX-RENDER-BUDGET 闸门 DRAFT；STATUS=DRAFT · NOT LOCKED；不授权采数；不授权真机；APPROVED_* 空；PERF-CI HOLD；Success≠adopt；勿擅自 LOCK；禁回改 CLOSED MATRIX/r1/r2/r3；禁合并洗票；BUDGET 挂账对照；刀口=gpu_render@180Hz；Australia/Perth（AWST，UTC+8））
