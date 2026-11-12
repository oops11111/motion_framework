# M3 下一票决断（M3-MATRIX CLOSED RIVE-MATRIX-FAIL · pacing_ok=0 · 不采用 · 不合 main · PERF-CI HOLD）

> DECISION_AT=2026-11-06 09:45:00 Asia/Shanghai  
> 依据：`docs/notion_m3_matrix_pack.md`；`docs/m3_matrix_result.md`；gate MATCH `d8c21d0304931c42d805ceef35fccf4df50b5f2931fd916b81a13eea5db1963f`  
> tip/commit：`9fe1ffe850162b9eb374d4552bba49462894e4df` / `c0ef236fff3a501bef002fff7db42398f686ed89` @ `claude/m3-matrix` = 实验证据；**成功≠采用≠默认可合入 main**  
> **硬禁**：不改 composition.h；不合 main；Success≠adopt；PERF-CI HOLD；不翻 CLOSED；**本 next 不授权新采数**；**勿自动 LOCK**

## 结论

**主推：新开 `M3-MATRIX-PACING` 闸门 DRAFT（纯文档）— 专吃 `pacing_ok=0` 残差；STATUS=DRAFT·NOT LOCKED；不授权真机；不派 workbuddy；APPROVED_* 空。**

**并行（文档，非采数）**：plan §7.5 试点 1 选型可继续纯文档推进；**不**借 MATRIX FAIL 插队解冻 PERF-CI / 合 main。

**一句话为什么**：混刷+混 DPI 双屏拓扑与预算/稳态/尺寸已采到，但 pacing 硬字段失败且未重跑——下一张应先把 pacing 判据/复测设计写成 DRAFT，而不是自动 LOCK 或假装 PASS。

## 现状

| 项 | 状态 |
|---|---|
| M3-MATRIX | **CLOSED RIVE-MATRIX-FAIL**；pacing_ok=0；不采用；不合 main |
| 其余 M3 前序 | CLOSED；**不翻** |
| composition.h | 冻结；本票未触 |
| PERF-CI | **HOLD** |
| 工具链 A/B | 挂账不挡 |

## 候选速判

| 选项 | 决断 | 理由 |
|---|---|---|
| **A 主推：MATRIX-PACING 新 DRAFT（纯文档）** | **建议采纳** | 吃掉 pacing 残差；开放：pacing 定义/门槛/是否降记录型/是否同拓扑复测 |
| B 自动 LOCK MATRIX-PACING / 立刻采数 | **否** | 用户/协调：**勿自动 LOCK**；须另走 LOCK→APPROVED |
| C 把本票改判 PASS 并采用 tip | **否** | pacing_ok=0；Success≠adopt |
| D 合 main / 解冻 PERF-CI | **否** | 硬禁 |
| E 翻 CLOSED 重跑 MATRIX 同闸 | **否（本 next）** | 除非协调新 APPROVED；本文件不授权 |
| F 只推进试点 1、忽略 pacing | **否作主推** | pacing 是矩阵核心残差；试点可并行文档 |

## 协调下一步

1. 归档本票 CLOSED FAIL / 不采用 / 不合 main。  
2. 若采纳 A：指定文档助手写 `docs/m3_matrix_pacing_gate_DRAFT.md`（**NOT LOCKED**）。  
3. 可选：把 LOCKED 闸门正文补进本机 docs（目前 box 未见）— **≠采数授权**。  
4. 禁整 tip 无 PR 合入；禁改 composition.h；禁自动 LOCK。

## 确认（硬边界）

- 不改 composition.h（CRLF `33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97` / LF `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）  
- PERF-CI HOLD；不合 main；Success≠adopt  
- **本 next 不授权新采数**；**勿自动 LOCK**

— notion ai（下一票建议=M3-MATRIX-PACING DRAFT；勿自动 LOCK；MATRIX=CLOSED FAIL）
