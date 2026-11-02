# M3 下一票决断（M3-RIVE-FOLLOW CLOSED RIVE-FOLLOW-PASS · METRICS_PASS · 有条件采用设计约束 · 不合 main · PERF-CI HOLD）

> DECISION_AT=2026-11-02 08:11:19 Australia/Perth（AWST）  
> 依据：`docs/notion_m3_rive_follow_pack.md`；`docs/m3_rive_follow_result.md`；gate MATCH `285e39dc…aab3`；approved pidan 2026-11-01 16:26 Asia/Shanghai  
> **K93 背景（写死）**：`M3-XAML-K93` **已 APPROVED** 且 **正在 DESKTOP-K93N3P8 跑**。本 next **不**重复授权采数、**不**阻拦 K93 workbuddy / 真机。  
> tip/commit：`0fb0760d…9e4f` @ `claude/m3-follow` = 实验分支证据；**成功≠采用≠默认可合入 main**  
> 硬禁：不改 composition.h；不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；PERF-CI HOLD；不翻 CLOSED；**本 next 不授权新采数**

## 结论

**主推：继续 / 待命已 APPROVED 且在跑的 `M3-XAML-K93`（并行进行中）— 本 next 不新开采数令、不改派工、不阻拦 K93。**

**附带（文档，非采数）**：把 FOLLOW WP2 设计约束写入试点/设计备忘（C=默认拆分跟随；B不升表面内默认；接缝弹簧阻尼）；MATRIX 仍排 FOLLOW 结题之后再议闸门，**不插队真机**。

**一句话为什么**：FOLLOW 真机与规则裁决已 CLOSED PASS；计划序下一张实验室矩阵可议，但现场已有 **K93 票在跑**——结题侧应让路并行，而不是再签一张采数票或挡道。

## 现状

| 项 | 状态 |
|---|---|
| M3-RIVE-FOLLOW | **CLOSED RIVE-FOLLOW-PASS**；METRICS_PASS；有条件采用设计约束；不合 main |
| M3-XAML-K93 | **APPROVED + 真机进行中（K93）** — **勿阻拦** |
| M3-XAML-REBUILD 等前序 | CLOSED；**不翻** |
| M3 plan 票序 | … → 3 FOLLOW ✓ → **4 MATRIX**（文档可议；真机不插队挡 K93） |
| composition.h | 冻结；本票未触 |
| PERF-CI | **HOLD** |

## 候选速判

| 选项 | 决断 | 理由 |
|---|---|---|
| **A 主推：K93 待命/并行（已 APPROVED 在跑）** | **采纳** | 用户写死；勿重复授权；勿阻拦 |
| B 本 next 再授权 FOLLOW / K93 / MATRIX 采数 | **否** | 硬禁；FOLLOW 已结题；K93 已有授权 |
| C 阻拦 / 改派 K93 | **否** | 用户硬禁 |
| D 立刻插队 MATRIX 真机并采数 | **否（本批）** | 可另开 DRAFT 文档；本 next 不授权真机 |
| E 合 main / 解冻 PERF-CI | **否** | Success≠adopt；HOLD |
| F 翻 CLOSED / 改 LOCKED body | **否** | 硬禁 |
| G 以 seam_ok=0 为由重开 FOLLOW 采数 | **否** | 规则已裁决为设计约束；禁硬凑 |

## 协调下一步

1. 归档本票 CLOSED PASS / METRICS_PASS / 有条件采用设计约束 / 不合 main。  
2. **让 `M3-XAML-K93` 继续**；协调只收其回报，不在本 next 加令。  
3. 可选：设计/试点约束补写 FOLLOW 三条规则结论——**纯文档**。  
4. MATRIX：另会话 DRAFT→LOCK→APPROVED 后再真机；**不**借本 next 插队。  
5. 禁整 tip 无 PR 合入；禁改 composition.h。

## 确认（硬边界）

- 不改 composition.h（CRLF `33d826c7…6e97` / LF `a55d4952…e418`）  
- PERF-CI HOLD；不合 main；Success≠adopt  
- 不翻 CLOSED；不阻拦 K93；**本 next 不授权新采数**

## 禁令

不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；成功≠采用；不合 main；PERF-CI HOLD；**不授权新采数**；不阻拦已派/在跑的 K93。

— notion ai（下一票主推=M3-XAML-K93 已派待命/并行；本 next 不授权采数；FOLLOW=CLOSED PASS）
