# M3 下一票决断（M3-XAML-K93 CLOSED XAML-K93-PASS · METRICS_PASS 有条件 · 有条件采用 · 不合 main · PERF-CI HOLD）

> DECISION_AT=2026-11-02 09:20:00 Australia/Perth（AWST）  
> 依据：`docs/notion_m3_xaml_k93_pack.md`；`docs/m3_xaml_k93_result.md`；gate MATCH `22fb8b92…b657`（本地未见 LOCK；tip/协调旁证）；Decision C HIT；原 APPROVED 仍有效  
> **FOLLOW 背景（写死）**：`M3-RIVE-FOLLOW` **已 CLOSED** `RIVE-FOLLOW-PASS`（METRICS_PASS；有条件采用设计约束）。**勿翻**；**勿重复授权采数**。  
> tip/commit：`cbc5fdb2…` / `bfd76c36…` @ `claude/m3-xaml-rebuild` = 实验分支证据；**成功≠采用≠默认可合入 main**  
> 硬禁：不改 composition.h；不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；PERF-CI HOLD；不翻 CLOSED；**本 next 不授权新采数**

## 结论

**主推：开 / 推进 `M3-MATRIX` 的 DRAFT（纯文档）— 本 next 不授权真机采数。**

**并行挂账（文档，非采数、不挡）**：工具链 **A**（钉/降 DirectX-Headers）与 **B**（升/对齐 Windows SDK）— 源自 K93 `BLOCKED_TOOLCHAIN`；可另开工具链文档票归档根因，**不**插队 MATRIX 真机，**不**借本 next 再派 K93 采数。

**一句话为什么**：FOLLOW 与 K93 诊断真机均已 CLOSED；计划序下一张实验室矩阵（`docs/m3_plan_DRAFT.md` WP3 / 票序 4）可进 **DRAFT 纸面**；工具链根因挂账并行，但 **禁** 再签采数令或把旁路当成原生已修。

## 现状

| 项 | 状态 |
|---|---|
| M3-XAML-K93 | **CLOSED XAML-K93-PASS**；METRICS_PASS 有条件；IJI_BYPASS_COPY；有条件采用；不合 main |
| M3-RIVE-FOLLOW | **CLOSED RIVE-FOLLOW-PASS**；**勿翻**；勿重复授权 |
| M3-XAML-REBUILD 等前序 | CLOSED；**不翻** |
| 工具链 A/B | **挂账**（本票不挡；另文档） |
| M3 plan 票序 | … → 3 FOLLOW ✓ → **4 MATRIX**（**DRAFT 可**；真机须另 LOCK+APPROVED） |
| composition.h | 冻结；本票未触 |
| PERF-CI | **HOLD** |

## 候选速判

| 选项 | 决断 | 理由 |
|---|---|---|
| **A 主推：MATRIX DRAFT（文档）** | **采纳** | FOLLOW+K93 已结；计划序到 WP3；本 next **不**授权真机 |
| B 本 next 再授权 K93 / FOLLOW / MATRIX 真机采数 | **否** | 硬禁；K93 已结；勿重复授权 |
| C 立刻插队 MATRIX 真机并采数 | **否** | 须另 LOCK+APPROVED；本 next 只到 DRAFT |
| D 以 A/B 工具链阻塞主线 / 翻 K93 PASS | **否** | Decision C 已挂账不挡；PASS 条件已写旁路 |
| E 合 main / 解冻 PERF-CI | **否** | Success≠adopt；HOLD |
| F 翻 CLOSED / 改 LOCKED body | **否** | 硬禁 |
| G 为补 TEXT 目视再派 K93 复采 | **否（本批）** | 缺字段挂账；≠本 next 采数令 |

## 协调下一步

1. 归档本票 CLOSED PASS / METRICS_PASS 有条件 / 有条件采用 / 不合 main / IJI_BYPASS_COPY。  
2. **MATRIX**：另会话写 DRAFT→（日后）LOCK→APPROVED 后再真机；**本 next 不授权**。  
3. **工具链 A/B**：可选开文档挂账票（根因/版本钉扎方案）— **纯文档**；不挡 MATRIX DRAFT。  
4. 禁整 tip 无 PR 合入；禁改 composition.h；禁把 Success 写成 ship。

## 确认（硬边界）

- 不改 composition.h（CRLF `33d826c7…6e97` / LF `a55d4952…e418`）  
- PERF-CI HOLD；不合 main；Success≠adopt  
- 不翻 CLOSED；**本 next 不授权新采数**（含不授权 MATRIX 真机、不授权 K93 复采）

## 禁令

不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；成功≠采用；不合 main；PERF-CI HOLD；**不授权新采数**；不翻 FOLLOW/REBUILD/K93 结论。

— notion ai（下一票主推=M3-MATRIX DRAFT 文档；工具链 A/B 挂账；本 next 不授权采数；K93=CLOSED PASS）
