# M3 下一票决断（M3-XAML-LOST CLOSED XAML-LOST-PASS · METRICS_PASS 有条件 · 有条件采用 · 不合 main · PERF-CI HOLD）

> DECISION_AT=2026-10-30 17:05:00 Asia/Shanghai  
> 依据：`docs/notion_m3_xaml_lost_pack.md`；`docs/m3_xaml_lost_result.md`；`docs/m3_plan_DRAFT.md` §7；**本地无 XAML-LOST LOCKED**；r1 INCONCLUSIVE / r2 FAIL / RECOVER PASS 有条件（**不翻盘**）；PROBE LOCKED `7afbcd98…6b54`（**不改**）  
> **背景（写死）**：`M3-RIVE-FOLLOW` 已 LOCKED；GATE_BODY_SHA256=`0f85a95277dd279e51085a42cd491b55db332ba49077055d045f794fc6b30a13`；**APPROVED_* 空**；**等 pidan**。LOCK ≠ 采数授权。  
> tip/commit：`bb61eae8…e012` / `efce2195…f556` @ `claude/m3-xaml-lost` = 实验分支证据归档；**成功≠采用≠默认可合入 main**；本票 **不合 main**  
> 硬禁：不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；PERF-CI HOLD；不开与本票无关的 M2 翻盘；不立刻 Δv″；不抬 thr；**本 next 不授权采数**；不派 workbuddy

## 结论

**主推：推进已 LOCKED 的 `M3-RIVE-FOLLOW` — 等 pidan 填写 APPROVED_*（或另有 approved 留证）后，方可真机采数。本 next 不 LOCK、不代填 APPROVED、不授权采数。**

**附带（可选）**：建议补办 `M3-XAML-LOST` 事后闸门纸面归档（写明「采数时本地无 LOCKED；按用户指令采数 / pidan 2026-10-30 16:43 四步」）— **补档 ≠ 授权新采数 ≠ 翻盘本票**。

**MATRIX**：仍排在 FOLLOW 闸门路径（LOCK+APPROVED+结题）之后；**本 next 不插队 MATRIX 真机**。

**一句话为什么**：XAML-LOST 有条件 PASS 已 CLOSED；主线下一张真机仍是 plan §7 第 3 张 FOLLOW；FOLLOW 已 LOCKED 但 APPROVED 空，卡点在 pidan 批准，**不要**用本 next 再开新采数。

## 现状

| 项 | 状态 |
|---|---|
| M3-RIVE-DPI | CLOSED RIVE-DPI-PASS；有条件采用；gate `92e6b4e9…97bf5` 不改 |
| M3-RIVE-LOST r1 | **CLOSED RIVE-LOST-INCONCLUSIVE**；**不翻盘** |
| M3-RIVE-LOST r2 | **CLOSED RIVE-LOST-FAIL**；**不翻盘** |
| M3-RIVE-LOST-RECOVER | **CLOSED RIVE-LOST-RECOVER-PASS**；有条件采用；不合 main；事后 LOCK `304a8046…272c` **不改** |
| M3-XAML-LOST | **CLOSED XAML-LOST-PASS**；METRICS_PASS **有条件**；**有条件采用验证结论；不合 main**；**本地无 XAML-LOST LOCKED** |
| M3-RIVE-FOLLOW | **LOCKED** hash `0f85a95277dd279e51085a42cd491b55db332ba49077055d045f794fc6b30a13`；**APPROVED 空**；**等 pidan**；LOCK ≠ 采数 |
| PROBE gate | LOCKED `7afbcd98…6b54`；**不改 body** |
| composition.h | 冻结 LF `a55d4952…e418` / CRLF `33d826c7…6e97`；本票未触 |
| M3 plan 票序 | 1 DPI ✓ → 2 LOST（r1/r2/RECOVER/XAML ✓有条件）→ **3 FOLLOW（LOCKED；等 APPROVED）** → 4 MATRIX |
| PERF-CI | **HOLD** |
| M2 旁路 / VERIFY-FAIL | **不翻盘** |
| S1 / R-3 / R-2 / Bv | 不重开；不解冻 |

## 候选速判

| 选项 | 决断 | 理由 |
|---|---|---|
| **A 主推：`M3-RIVE-FOLLOW` 等 pidan APPROVED** | **采纳（主推）** | 已 LOCKED `0f85a952…0a13`；APPROVED 空；plan 第 3 张 |
| B 本 next 授权 FOLLOW / XAML-LOST 真机采数 | **否** | 硬禁；本 next 不授权采数 |
| C 补 XAML-LOST 事后闸门纸面归档 | **建议（附带；可选）** | 消化「无 LOCKED 已采数」残差；**≠授权采数** |
| D 立刻插队 MATRIX 真机 | **否** | 须 FOLLOW 路径与各自 LOCK+APPROVED；不插队 |
| E 把 tip/commit 整包合入 main / csproj 回灌 | **否** | 不合 main；成功≠采用 |
| F 解冻 PERF-CI / 开第二机 | **否** | HOLD；本 next 不批准 |
| G 重开 S1/R-3/R-2 / 解冻 Bv / 改 composition.h | **否** | 硬禁 |
| H 翻盘 r1/r2/RECOVER / 改 PROBE/FOLLOW LOCKED / 改本票 CLOSED | **否** | 硬禁 |
| I 翻盘 M2 / 立刻 Δv″ / 抬 thr / 影子 IV | **否** | 硬禁 |
| J 以 with_rive n=1 为由重开 XAML-LOST 真机硬凑 | **否（本批）** | 有条件 PASS 已 CLOSED；挂账复核 results 即可；本 next 不授权采数 |
| K 新开与 FOLLOW 无关的旁路采数票 | **否** | 不插队；卡点在 FOLLOW APPROVED |

## FOLLOW 待 APPROVED 后要点（本 next 不代填、不采数）

1. 须 APPROVED_* 非空（或 approved 留证）后才可采数；**本 next 不授权**。  
2. **不改** composition.h；不重开 S1/R-3/R-2；禁影子 IV；PERF-CI HOLD。  
3. 成功 ≠ 采用；结果评审后经 PR；**不合 main 默认**。  
4. 继承：WP1 DPI PASS + LOST-RECOVER / XAML-LOST 有条件 PASS 为前置旁证；**不**翻盘 r1/r2。  
5. 结果目录写死：`m3/results_follow/`（勿覆写 xaml_lost / lost_recover 等）。  
6. Lab csproj 若仍需要：仅本机 Demo 启动前置，**不**回灌主线。

## 协调下一步

1. **主路径**：归档本票 CLOSED PASS / 有条件 METRICS / 有条件采用 / 不合 main；**等 pidan APPROVED `M3-RIVE-FOLLOW`**（gate `0f85a952…0a13`）。  
2. **附带（可选）**：协调决定是否补 XAML-LOST 事后纸面归档；**勿**把补档当采数令。  
3. **挂账复核**（只读）：待 `m3/results_xaml_lost/` 同步到本根后核对 session 关键字段（不重跑；勿改）。  
4. **禁**本 next 当采数授权；**禁**整 tip 无 PR 合入；**禁** csproj 回灌主线。  
5. MATRIX：**不插队真机**；FOLLOW 结题后再议。  
6. DPI / PROBE / r1 / r2 / RECOVER / FOLLOW LOCKED：**不改不重跑翻盘**。

## 确认（硬边界）

- 不重开 S1 / R-3 / R-2；不改 composition.h（`a55d4952…e418` / CRLF `33d826c7…6e97`）；不解冻 Bv；禁影子 IV  
- PERF-CI HOLD；不立刻 Δv″；不抬 thr；不翻盘 M2  
- 不改 r1/r2/RECOVER CLOSED；不改 PROBE/FOLLOW/RECOVER LOCKED；不虚构本票 XAML-LOST LOCKED  
- 本 next **不**授权采数、**不**派 workbuddy；**不**把 FOLLOW LOCK 当成已 APPROVED

## 禁令

不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；成功≠采用≠默认可合入 main；不合 main；PERF-CI HOLD；不开无关 M2 翻盘；禁抬 thr；禁立刻 Δv″；**本 next 不授权采数**；**不**预开 FOLLOW/MATRIX 真机；**不**翻盘 r1/r2/RECOVER；**不**把 DRYRUN/selftest 当真机唯一 PASS。

— notion ai（下一票主推=`M3-RIVE-FOLLOW` 等 pidan APPROVED；背景 FOLLOW LOCKED `0f85a952…0a13` APPROVED 空；本 next 不授权采数）
