# M3 下一票决断（M3-XAML-REBUILD CLOSED XAML-REBUILD-PASS · R4=KNOWN_LIMIT · 有条件采用 · 不合 main · PERF-CI HOLD）

> DECISION_AT=2026-11-01 17:55:00 Asia/Shanghai  
> 依据：`docs/notion_m3_xaml_rebuild_pack.md`；`docs/m3_xaml_rebuild_result.md`；gate MATCH `3ea74c37…6fdc`；approved pidan 2026-11-01 16:26 Asia/Shanghai  
> **FOLLOW 背景（写死）**：`M3-RIVE-FOLLOW` 已 APPROVED（SendInput LOCK `285e39dc9c1100f92ddddc5560a3631b94129a9b56c561c9803c8cbee57eaab3`）且 **已派 workbuddy**（`docs/workbuddy_m3_rive_follow_dispatch.md`）。本 next **不**重复授权采数、**不**阻拦 workbuddy。  
> tip/commit：`24ccce6c…599c` / `d00ffd0f…9e9f` @ `claude/m3-xaml-rebuild` = 实验分支证据；**成功≠采用≠默认可合入 main**  
> 硬禁：不改 composition.h；不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；PERF-CI HOLD；不翻 CLOSED；**本 next 不授权新采数**

## 结论

**主推：继续 / 待命已派的 `M3-RIVE-FOLLOW`（并行进行中）— 本 next 不新开采数令、不改派工、不阻拦 workbuddy。**

**附带（文档，非采数）**：把本票 R4=KNOWN_LIMIT 写入试点约束备忘（重启应用；非默认「重建窗口」）；Q3 是否报 microsoft-ui-xaml **等 pidan**。C 组 W_* 截图 INCONCLUSIVE 仅挂账，**不**借本 next 复采。

**一句话为什么**：XAML-REBUILD 诊断已 CLOSED；主线真机下一张仍是 FOLLOW，且 **已批准并已派出**——结题侧应让路，而不是再签一张采数票。

## 现状

| 项 | 状态 |
|---|---|
| M3-XAML-REBUILD | **CLOSED XAML-REBUILD-PASS**；R4=**KNOWN_LIMIT**；有条件采用；不合 main |
| M3-RIVE-FOLLOW | **APPROVED** + **已派 workbuddy**；LOCK `285e39dc…aab3`；结果目录写死 `m3/results_follow/` |
| M3-XAML-LOST 等前序 | CLOSED；**不翻** |
| composition.h | 冻结；本票未触 |
| PERF-CI | **HOLD** |

## 候选速判

| 选项 | 决断 | 理由 |
|---|---|---|
| **A 主推：FOLLOW 待命/并行（已派）** | **采纳** | 已 APPROVED+dispatch；勿重复授权 |
| B 本 next 再授权 FOLLOW / REBUILD 采数 | **否** | 硬禁 |
| C 阻拦 / 改派 workbuddy | **否** | 用户硬禁：不阻拦 workbuddy |
| D 立刻开「宿主重建窗口」实现票并采数 | **否** | R4 已否决充分性；仅文档约束 |
| E 复采 C 组 W_* 补 INCONCLUSIVE | **否（本批）** | 挂账；≠本 next 采数令 |
| F 合 main / csproj 回灌 | **否** | Success≠adopt |
| G 解冻 PERF-CI | **否** | HOLD |
| H 翻 CLOSED / 改 LOCKED body | **否** | 硬禁 |

## 协调下一步

1. 归档本票 CLOSED PASS / KNOWN_LIMIT / 有条件采用 / 不合 main。  
2. **让 FOLLOW workbuddy 继续**；协调只收其回报，不在本 next 加令。  
3. 可选：试点 README/约束补一句「GPU 设备丢失后建议重启应用（XAML-REBUILD）」——**纯文档**。  
4. Q3 对外报告：**等 pidan**。  
5. MATRIX：仍排 FOLLOW 路径之后；**不插队**。

## 确认（硬边界）

- 不改 composition.h（`a55d4952…e418` / CRLF `33d826c7…6e97`）  
- PERF-CI HOLD；不合 main；Success≠adopt  
- 不翻 CLOSED；不阻拦 workbuddy；**本 next 不授权新采数**

## 禁令

不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；成功≠采用；不合 main；PERF-CI HOLD；**不授权新采数**；不阻拦已派 FOLLOW。

— notion ai（下一票主推=FOLLOW 已派待命/并行；本 next 不授权采数；REBUILD=KNOWN_LIMIT 已 CLOSED）
