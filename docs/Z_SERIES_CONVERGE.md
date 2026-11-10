# Z 序列收敛文档（唯一主文档）

- **STATUS**：DRAFT · NOT LOCKED · 唯一维护入口
- **UPDATED**：2026-11-10 22:59 AWST（Australia/Perth）
- **FILE_SHA256**：`f546d3834b691916ff2c7943e34e20e2243aa92d9199cb1f9920f32a06240420`（对落盘全文做 SHA-256，**排除**以「- **FILE_SHA256**」开头的那一行）
- **规则**：后续 Z1–Z4 / API v0 前置的开发与审议，**只追加或修订本文件**；三方原文归档在 `docs/_out/`，不另开平行主文档。
- **硬边界**：Success≠adopt；不合 main；不改 composition.h（除非另 LOCK）；无 APPROVED 不采数；PERF-CI HOLD；Y4 见 `docs/COORDINATION.md`；冻结 L1 合同 ≠ 授权编码；另票「开 API v0」之前不开 C# API v0 实现。

## 指针

| 文件 | 角色 |
|---|---|
| **本文件** | 唯一收敛主文档 |
| `docs/z2_z4_DRAFT.md` | 初始 DRAFT 快照（可被本文件取代叙述） |
| `docs/z2_z4_triad_converge_20261106.md` | 三方收敛草案（已并入下文） |
| `docs/_out/z2_z4_*_converge_20261106.md` | Claude / Gemini / Notion 原文归档 |
| `docs/api_surface_m2_alpha_contract_LOCKED.md` | API v0 L1 冻结合同（GATE_BODY_SHA256=`77416e8bbc700238b42a940a2f93d4466edec1ebd79352b007f86c267a663a0f`） |
| `docs/api_surface_m2_alpha_contract_DRAFT.md` | L1 合同起草稿（SUPERSEDED_BY_LOCKED） |
| `docs/api_v0_frozen.md` | 冻结规则（STATUS=**L1 FROZEN**） |
| `docs/api_changelog.md` | 冻结后变更日志 |
| `docs/z4_etw_l1_smoke_gate_DRAFT.md` | Z4 ETW + L1 smoke 闸门（DRAFT · NOT LOCKED；E1–E4 已由协调裁决；无真机授权；仍须 pidan LOCK+APPROVED 才采数） |
| `docs/z1_pilot_scoring_table_DRAFT.md` | Z1 打分表（B/C **APPROVED BY PIDAN** 已入席；侧叉本地；不代选 Winner；试点编码另票） |
| `docs/m3_matrix_pacing_r4_adoption.md` | r4 条件采用记录（WP；不合 main；非 API v0 默认） |
| `docs/m3_matrix_budget_gate_DRAFT.md` | 预算闸门 DRAFT（**SUPERSEDED**；勿再 LOCK / 采数） |
| `docs/m3_matrix_render_budget_gate_DRAFT.md` | render 预算闸门 DRAFT（**SUPERSEDED**；勿再 LOCK / 采数） |

## tip 口径

| 角色 | 哈希 |
|---|---|
| tip_read（采数批准基线） | `e94600332307e4482cf0ff91d4e16d3b60deefc5` |
| results | `6b7a290864ef58ff8b908f8d46532e449c8e4530`（`m3/results_matrix_pacing_r4/`） |
| code | `69094ee`（vblank + 0.3·T） |
| close-pack tip | `907da1ee…`（结题**文档推送** tip；勿与 tip_read 混用） |

## 当前收敛结论（三方 → 协调）

1. **Z1** 候选已提名并打分（**未选定 Winner**；协调不代选 Winner）。2026-11-10：pidan 无自有 WinUI 3 产品，委托协调在 GitHub 公开仓库中找 B/C。2026-11-10 22:59 AWST pidan 书面「同意 B=FluentHub、C=Files，推上去」——B/C **APPROVED BY PIDAN，已入席**：
   - **A · MotionSoftDemo**（内部狗粮 / 脚手架）：已有 WinUI 3 宿主；Rive/混刷/探针已跑通；兼作 Z3a 狗粮。初评 33/50。**不当试点赢家**（Success≠adopt）。
   - **B · 0x5bfa/FluentHub**（真试点席，**APPROVED BY PIDAN**，34/50）：MIT；中等体量 WinUI 3 GitHub 客户端；列表/页面导航。侧叉本地；不向上游提 PR。备份（未入席）：Gaoyifei1011/GetStoreApp（30/50）。
   - **C · files-community/Files**（对照席，**APPROVED BY PIDAN**，32/50）：MIT；重体量、活跃的 WinUI 3 文件管理器。侧叉本地；不向上游提 PR。备份（未入席）：bkaankose/Wino-Mail（32/50，GPL-3.0，仅侧叉）。详见 `docs/z1_pilot_scoring_table_DRAFT.md`。
   - 入席 ≠ 选定 Winner ≠ 授权试点编码（试点编码仍须另票）。Success≠adopt。
2. **Z2 / API v0 L1 合同已冻结**（pidan「冻结 API v0 L1」，2026-11-06 16:32:43 AWST；2026-11-10 22:35 AWST 再确认）：权威文本 `docs/api_surface_m2_alpha_contract_LOCKED.md`（GATE_BODY_SHA256=`77416e8b…3a0f`）；L1=MotionSystem / AnimationHandle / MotionTokens 三表 + 诚实非目标；RiveView / PointerFollow 另票；**禁默认 0.3·T**。冻结 = 合同仅冻结；**仍须另票才编码**；不合 main；Success≠adopt。规则见 `docs/api_v0_frozen.md`。
3. **Z3a** 用现有软件 / C-ABI 桥在 MotionSoftDemo 跑 Play→Retarget→Cancel（可并行；Wave B 彩排派工已落）；**Z3b** WinMD 狗粮（须 L1 冻结 + 另票开 API v0）。真实 WinRT Composition 后端为诚实非目标，另排。
4. **Z4** ETW + L1 smoke 闸门 DRAFT 已交 `docs/z4_etw_l1_smoke_gate_DRAFT.md`（NOT LOCKED；无真机授权）。L1 smoke ≠ PERF-CI；不折进 M3 budget；不解冻 Bv；不重开 S1/R-3。四条 INVALID 已写死。**E1–E4 已由协调按 pidan 2026-11-10 委托裁决**（采纳 G3 原建议，不扩张）；**裁决 ≠ LOCK ≠ APPROVED ≠ 授权采数**。
5. **顺序**：Wave A（Z2∥Z4 文档，已完成）→ Wave B（Z3a 彩排）→ Gate → **另票**才开 API v0。
6. **人日估计**：本窗 8–14 人日；API v0 实现另计（plan 模块7：5–7 人周）。
7. **M3 r4**：条件采用为产品 Rive 宿主工作路径（实验分支 + 产品化模块默认开启；仍不合 main；非 API v0 默认）。两份预算 DRAFT **SUPERSEDED**。PERF-CI 仍 HOLD。

## 裁定（pidan 书面确认 · 2026-11-10 22:35 AWST）

pidan 于 2026-11-10 22:35 AWST（Australia/Perth）对协调五条建议回复「确认」。下列与 2026-11-06 已落盘口径一致，本次为**书面再确认**；不以本确认重开编码、采数、LOCK 或合 main。

1. **API v0 L1 合同 — 已冻结。** 冻结面仅 `MotionSystem` / `AnimationHandle` / `MotionTokens`；RiveView / PointerFollow 另票；不把 0.3·T 写成 API 默认；三表 + 诚实非目标以 LOCKED 合同为准。冻结 = 合同仅冻结；编码仍须另票「开 API v0」；不合 main；Success≠adopt。权威：`docs/api_surface_m2_alpha_contract_LOCKED.md`（`77416e8b…3a0f`，LOCKED_AT=2026-11-06 16:32:43 AWST）；规则：`docs/api_v0_frozen.md`。
2. **Z1。** A=MotionSoftDemo 只作内部狗粮（33/50），不当试点赢家。协调不代选 Winner。2026-11-10 22:56 AWST 提案 B=`0x5bfa/FluentHub`、C=`files-community/Files`（备份 GetStoreApp / Wino-Mail）。**2026-11-10 22:59 AWST pidan 书面「同意 B=FluentHub、C=Files，推上去」：B/C 已入席（APPROVED BY PIDAN）**。范围：侧叉本地；不向上游提 PR；Success≠adopt；未选 Winner；试点编码仍须另票。备份仍为备份、未入席。
3. **E1–E4 按主文档开放项，解释为 Z4 ETW + L1 smoke。** 已开 `docs/z4_etw_l1_smoke_gate_DRAFT.md`（**NOT LOCKED**，无真机授权）：最小 ETW 事件集 +「C# API usable」L1 smoke PASS 判据。INVALID 写死：(a) 未 LOCK+APPROVED 就采数；(b) 改 `composition.h`；(c) 影子 / 未声明 tip；(d) 把 PERF-CI HOLD 当绿。L1 smoke ≠ PERF-CI；不折进 M3 budget；不解冻 Bv；不重开 S1/R-3。2026-11-10 22:35 确认的是 DRAFT 路径与四条 INVALID。**2026-11-10 22:56 AWST：pidan 委托「E1–E4 你来定」，协调已按 G3 建议作保守裁决**（见下文专节与 Z4 DRAFT G3）；**仍 NOT LOCKED**，不授权采数。
4. **后端路径 — C-interface 先通（Z3a）。** 用现有软件 / C-ABI 桥在 MotionSoftDemo 跑 Play→Retarget→Cancel；真实 WinRT Composition 后端是诚实非目标，另排；试点不被 compositor 卡住。
5. **r4 节奏 — 条件采用为产品 Rive 宿主工作路径。** 代码 `69094ee`（vblank + offset=0.3·T）；PASS 闸门 `81fc74ebba270feb818646cd5f5c8c90fd2d3812c3447c642ed126413108934c`；结果 `m3/results_matrix_pacing_r4/`。范围 = 实验分支 + 产品化模块默认开启；**仍不合 main**；**不写成 API v0 默认**。`docs/m3_matrix_budget_gate_DRAFT.md`（文首前 sha `49bbc975…4745`）与 `docs/m3_matrix_render_budget_gate_DRAFT.md`（文首前 sha `e10bf9e5…5dd1`）正式标 **SUPERSEDED**（正文保留不删；勿再 LOCK、勿再采数）。PERF-CI 仍 HOLD。记录：`docs/m3_matrix_pacing_r4_adoption.md`。

## 开 API v0 前清单

1. pidan 书面同意本收敛（或后继修订）— **已满足**（2026-11-06 冻结 + 2026-11-10 22:35 AWST 再确认）
2. Z2 可 LOCK 文本（含三表）— **已满足**（LOCKED `77416e8b…3a0f`）
3. Z4 评审版 DRAFT — **已交**；E1–E4 **已由协调裁决**（2026-11-10 22:56 AWST）；本项 ≠ LOCK 采数，仍须 pidan LOCK+APPROVED
4. Z3a 跑法 + lab 彩排一次 — **派工已落** `docs/workbuddy_z3a_rehearsal_dispatch.md`；彩排结果仍待回报
5. **另票**「开 API v0 实现」— **未开**（默认否）
6. 基线 tip/branch 声明；PERF-CI HOLD；Y4 — 持续有效

## 待 pidan（开放）

- [x] Z2：**API v0 L1 已冻结**（pidan「冻结 API v0 L1」，2026-11-06 16:32:43 AWST；2026-11-10 22:35 AWST 再确认）：`docs/api_surface_m2_alpha_contract_LOCKED.md`，GATE_BODY_SHA256=`77416e8b…3a0f`；规则 `docs/api_v0_frozen.md`；变更日志 `docs/api_changelog.md`。冻结 ≠ 合 main ≠ adopt；编码仍须另票「开 API v0」（在 Z3a 彩排之后）。
- [x] Z1：**B/C 已入席（APPROVED BY PIDAN，2026-11-10 22:59 AWST，「同意 B=FluentHub、C=Files，推上去」）**。B=`0x5bfa/FluentHub`（34/50），C=`files-community/Files`（32/50）；备份 GetStoreApp / Wino-Mail 仍为备份、未入席。A = MotionSoftDemo 只作内部狗粮（33/50），不当试点赢家。协调不代选 Winner。入席 = 侧叉本地；不向上游提 PR；Success≠adopt；**未选 Winner**；试点编码仍须另票。
- [x] Wave A 的 Z4 ETW/L1 smoke DRAFT 已交：`docs/z4_etw_l1_smoke_gate_DRAFT.md`（**NOT LOCKED**；无真机授权；四条 INVALID 与边界句已写死）。
- [x] Z4 **E1–E4 已由协调裁决**（pidan 2026-11-10 委托「E1–E4 你来定」；2026-11-10 22:56 AWST；正文见下节与 `docs/z4_etw_l1_smoke_gate_DRAFT.md` G3）。
- [ ] Z4 闸门仍 **NOT LOCKED**：E1–E4 裁决 ≠ LOCK ≠ APPROVED ≠ 授权采数 / 真机。采数前须 pidan 书面 LOCK+APPROVED。
- [x] 后端路径：**A = C-ABI 先通**（pidan 2026-11-06；2026-11-10 22:35 AWST 再确认）。Z3a 用现有软件 / C-ABI 桥在 MotionSoftDemo 跑 Play→Retarget→Cancel；真实 WinRT Composition 后端是诚实非目标，另排；试点不被合成器卡住。
- [x] M3：r4 **有条件采用（WP）**（pidan 2026-11-06；2026-11-10 22:35 AWST 再确认），范围 = 实验分支 + 产品化 Rive 宿主默认开启，不进 API v0 L1 默认、不合 main；两份预算 DRAFT **SUPERSEDED**（本次把正文纳入仓库并加文首横幅；见 `docs/m3_matrix_pacing_r4_adoption.md`）。PERF-CI 仍 HOLD。
- [ ] 另票开 API v0？（默认否）

## 变更日志

| 时间 | 变更 |
|---|---|
| 2026-11-10 22:59 AWST | pidan 书面「同意 B=FluentHub、C=Files，推上去」：Z1 B/C **APPROVED BY PIDAN，已入席**（侧叉本地；不向上游提 PR；Success≠adopt；未选 Winner；试点编码仍须另票）。备份 GetStoreApp / Wino-Mail 仍为备份。 |
| 2026-11-10 22:56 AWST | pidan 无自有 WinUI 3 项目，委托 GitHub 选 B/C，并委托「E1–E4 你来定」。Z1：当时 B/C **PROPOSED**（FluentHub / Files；备份 GetStoreApp / Wino-Mail）；已由 22:59 条目入席。E1–E4：协调按 Z4 DRAFT G3 原建议作保守裁决（不扩张）；Z4 仍 NOT LOCKED，不授权采数。 |
| 2026-11-10 22:35 AWST | pidan 书面「确认」五条裁定（再确认 2026-11-06 已落口径）：L1 合同冻结仍有效；Z4 DRAFT 路径与四条 INVALID 再确认（当时 E1–E4 仍开，已由 22:56 条目关闭裁决但仍不 LOCK）；C-interface / Z3a 先通；r4 条件采用 WP；两份预算 DRAFT 正式标 SUPERSEDED 并入仓加横幅；PERF-CI 仍 HOLD。 |
| 2026-11-06 AWST | Claude：Wave B 第一步——Z3a 彩排派工单 `docs/workbuddy_z3a_rehearsal_dispatch.md`（K93N3P8；用现有 M2-02 探针跑 Play → Retarget → Cancel + ETW 6 事件；不采数、不提交、先备份后还原 `artifacts/m2_02_*`） |
| 2026-11-06 16:32:43 AWST | API v0 L1 冻结：契约 LOCKED `77416e8b…3a0f`（DRAFT 标 SUPERSEDED_BY_LOCKED）；`api_v0_frozen.md` 改为已冻结；新建 `api_changelog.md` |
| 2026-11-06 AWST | Claude 按 pidan 意见：删除 4 个文件中的个人邮箱；契约补“implemented vs missing”附录；Z4 写死四条 INVALID 与边界句；记录后端路径 = C-ABI 先通；r4 有条件采用、两份预算 DRAFT SUPERSEDED |
| 2026-11-06 AWST | Claude：Z4 DRAFT `docs/z4_etw_l1_smoke_gate_DRAFT.md`（L1 序列、连续性、减少动效、空闲、线程、ETW 六条判据；E1 建议冻结 ETW 名字与 GUID）；`claude/m3-wp4-api` 上的 `docs/api_v0_DRAFT.md` 与 `FluidMotion.Api.v0.cs` 由 Z2 契约取代，仅作参考（L1 名字一致；RiveView / PointerFollow 部分对应另票） |
| 2026-11-06 15:28 Asia/Shanghai | 写入 remaining_effort_ESTIMATE；准备推 GitHub |
| 2026-11-06 15:26 AWST | Claude Wave A：`api_surface_…` / `api_v0_frozen` / `z1_pilot_scoring_table` 已核哈希；指针并入本文件；仍不冻结 |
| 2026-11-06 15:19:25 AWST | 建档：合并三方收敛；pidan 指示「后续开发后收敛给一个文档」→ 本文件为唯一主文档 |



## 2026-11-10 22:56 AWST · Z1 B/C 提案与 E1–E4 裁决

依据：pidan（2026-11-10）「手头没有 WinUI 3 项目，去 GitHub 看看？」+「E1–E4 你来定」；随后于 2026-11-10 22:59 AWST 书面「同意 B=FluentHub、C=Files，推上去」。公开仓库经 GitHub REST API 核验（存在性、许可证、stars、最后提交、WinUI 3 vs UWP/WPF/Avalonia）。试点 = 侧叉本地；不向上游提 PR；Success≠adopt。协调不代选 Winner；未选 Winner；试点编码仍须另票。

### Z1 B/C（2026-11-10 22:56 提案；**2026-11-10 22:59 AWST APPROVED BY PIDAN，已入席**）

| 席 | 应用 | URL | 许可证 | Stars | 最后提交（AWST） | 加权分 | 状态 / 为何 |
|---|---|---|---|---|---|---|---|
| **B** | 0x5bfa/FluentHub | https://github.com/0x5bfa/FluentHub | MIT | 1263 | 2026-09-20 00:42 | **34/50** | **已入席（APPROVED BY PIDAN）。** 友好中等、许可宽松、纯 WinUI 3（`UseWinUI` + WASDK 2.0.1）、仓库/Issue/PR 列表与页面导航；侧叉稳定（近季 40 提交，9 月有 0.10.0）。侧叉本地；不向上游提 PR。 |
| **C** | files-community/Files | https://github.com/files-community/Files | MIT（树内另有 LICENSE-MPL） | 46573 | 2026-11-10 22:22 | **32/50** | **已入席（APPROVED BY PIDAN）。** 重体量对照：活跃、列表/导航/预览面多；主应用确为 WinUI 3（WASDK 2.6.5），伴有 shell / 对话框互操作，适合压边界。侧叉本地；不向上游提 PR。 |
| B 备份 | Gaoyifei1011/GetStoreApp | https://github.com/Gaoyifei1011/GetStoreApp | MIT | 1463 | 2026-11-07 16:35 | 30/50 | **仍为备份，未入席。** 更近活跃（11-06 发 v6.27.1106.0；WASDK.WinUI 2.6.5）；Store/WinGet 列表页全；W3 因 ShellExtension / WindowsAPI / WebView 扣分。若已入席的 FluentHub 在实验室站不起来，再议是否改坐。 |
| C 备份 | bkaankose/Wino-Mail | https://github.com/bkaankose/Wino-Mail | **GPL-3.0** | 1661 | 2026-11-10 22:06 | 32/50 | **仍为备份，未入席。** 重型 WinUI 3 邮件客户端。仅侧叉；未得 GPL 合规前不对外分发该叉。若已入席的 Files 在实验室站不起来，再议是否改坐。 |

已排除（详见打分表附录）：UniGetUI 已迁 Avalonia；DevToys v2 为 Blazor+WPF；ambie / Nightingale / Fluent Screen Recorder 为 UWP；ModernFlyouts 已归档；Pixeval 为 Avalonia；PowerToys 仅 Settings 为 WinUI 3；tiny11builder 无 SPDX 许可证；WinUI-Gallery 是控件展厅而非产品应用（与 A 同生态位）。

pidan 已于 2026-11-10 22:59 AWST 书面同意本提案（B=FluentHub，C=Files）。**已入席**。入席 ≠ 选定 Winner ≠ 授权试点编码（试点编码仍须另票）。试点 = 侧叉本地；不向上游提 PR；Success≠adopt。备份仍为备份。

### E1–E4（协调已裁，Z4 仍 NOT LOCKED）

出处：`docs/z4_etw_l1_smoke_gate_DRAFT.md` G3。四条均**采纳原建议、不扩张**。与既有硬边界一致：Z4 保持 DRAFT，直至 pidan LOCK+APPROVED；四条 INVALID 仍写死；PERF-CI HOLD；`composition.h` 冻结；L1 smoke ≠ PERF-CI；C-ABI 先通。

| # | 是什么 | 裁决 |
|---|---|---|
| **E1** | ETW 事件名、provider 名和 GUID 要不要随 L1 一起冻结 | **冻结**事件名、provider 名 `FluidMotion-Motion`、GUID `8f3a2c1d-6b4e-4a91-9c7d-2e5f8a1b3c4d` 与事件 id；字段只许末尾追加，不得改名 / 删字段 / 换 GUID。这是闸门合同文字，不是 LOCK 本 DRAFT，也不授权采数。 |
| **E2** | 在哪些机器上跑 | **仅** K93N3P8（Win10）与 IJI1KJG（Win11）；每台 ≥ 3 个独立进程会话；unpackaged 与打包各至少 1 个。不扩机台。 |
| **E3** | ETW 怎么采 | **EventPipe（无需管理员）为主路径**；`logman` ETL→WPA 仅为有管理员时的可选对照，**不是** PASS 必要条件。 |
| **E4** | 由谁、在哪个宿主上跑 | **MotionSoftDemo** 经现有软件 / C-ABI 桥跑 Play→Retarget→Cancel；探针改用 L1 名字。不在 Z1 试点应用内跑本闸门；真实 WinRT Composition 后端仍为诚实非目标。本条不授权编码。 |

采数仍须本闸门 LOCK + APPROVED。PERF-CI 继续 HOLD。


## API v0 评审（协调 · 2026-11-06 15:25 AWST）

> **嗣后状态**：本节为 2026-11-06 15:25 评审原文（当时裁决「暂不冻结」）。L1 已于 2026-11-06 16:32:43 AWST 冻结，并经 2026-11-10 22:35 AWST 再确认。勿把本节误读为现行状态。

**裁决：暂不冻结**。Claude 已按改点写出 Wave A 目标文件（见指针表）；仍 NOT FROZEN / 不开编码。

### 须改后再谈冻结
1. **拆面**：L1（M2 Alpha）只冻 `MotionSystem` / `AnimationHandle` / `MotionTokens`（Play→Retarget→Cancel + 减少动效钩子）。`RiveView`（H1–H8）与 `PointerFollow` 另票（产品化 / WP1–WP2），勿塞进 API v0 L1。
2. **落三表**（写入可 LOCK 合同，建议路径 `docs/api_surface_m2_alpha_contract_DRAFT.md`）：L1↔`AnimationHandle`/`MotionSystem`/`fm_capi.h`；线程/非线程安全写死句；诚实非目标（尚无真实 WinRT Composition 后端；v0 允许软件/C-ABI 桥；WinMD≠C-ABI）。
3. **命名统一**：公开面用 `MotionSystem`/`AnimationHandle`（对齐 C++/`api.md`），不要并行再推 `MotionEngine`/`MotionAnimation`。
4. **禁止写入默认**：不得把 PACING r4 `offset=0.3·T` / tip `69094ee` 写成 API 默认行为（条件采用仍开）。
5. **冻结规则（Z3）**：`docs/api_v0_frozen.md` 须写清「试点期只许新增；改名/删除走闸门；LOCK 合同 ≠ 授权合 main」。
6. **前置**：先完成 Wave A（Z2∥Z4 文档）与 Z3a 彩排门槛，**另票**「开 API v0 实现」后再编码；现有可工作成员可先列「implemented vs missing」清单作附录，但清单 ≠ 冻结。

### 冻结后才允许
实现今日可工作成员 + 补缺 C-interface + 测试（仍不合 main；Success≠adopt；PERF-CI HOLD）。
