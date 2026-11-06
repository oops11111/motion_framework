# Z 序列收敛文档（唯一主文档）

- **STATUS**：DRAFT · NOT LOCKED · 唯一维护入口
- **UPDATED**：2026-11-06 15:28 Asia/Shanghai
- **规则**：后续 Z1–Z4 / API v0 前置的开发与审议，**只追加或修订本文件**；三方原文归档在 `docs/_out/`，不另开平行主文档。
- **硬边界**：Success≠adopt；不合 main；不改 composition.h（除非另 LOCK）；无 APPROVED 不采数；PERF-CI HOLD；Y4 见 `docs/COORDINATION.md`；pidan 同意前不开 C# API v0 实现。

## 指针

| 文件 | 角色 |
|---|---|
| **本文件** | 唯一收敛主文档 |
| `docs/z2_z4_DRAFT.md` | 初始 DRAFT 快照（可被本文件取代叙述） |
| `docs/z2_z4_triad_converge_20261106.md` | 三方收敛草案（已并入下文） |
| `docs/_out/z2_z4_*_converge_20261106.md` | Claude / Gemini / Notion 原文归档 |

## tip 口径

| 角色 | 哈希 |
|---|---|
| tip_read（采数批准基线） | `e94600332307e4482cf0ff91d4e16d3b60deefc5` |
| results | `6b7a290864ef58ff8b908f8d46532e449c8e4530`（`m3/results_matrix_pacing_r4/`） |
| code | `69094ee`（vblank + 0.3·T） |
| close-pack tip | `907da1ee…`（结题**文档推送** tip；勿与 tip_read 混用） |

## 当前收敛结论（三方 → 协调）

1. **Z1** 候选已提名（待打分表；未选定）：
   - **A · MotionSoftDemo**（主候选 / 内部）：已有 WinUI 3 宿主；Rive/混刷/探针已跑通；兼作 Z3 狗粮（Success≠adopt）。
   - **B · 友好中等 WinUI 3 产品应用**（真试点席）：列表/导航/引导痛点 + 遥测 + 设计师愿写 token + 非关键业务——**请 pidan 填真实应用名**后进打分表。
   - **C · 更重/更老 WinUI 3 对照应用**（对照席）：压边界——**请 pidan 填真实应用名**；暂无则先只评 A+B。
2. **Z2** Wave A 合同 DRAFT 已落 `docs/api_surface_m2_alpha_contract_DRAFT.md`（三表齐；L1=MotionSystem/AnimationHandle/MotionTokens；RiveView/PointerFollow 另票；禁默认 0.3·T；**NOT FROZEN**）。规则见 `docs/api_v0_frozen.md`。
3. **Z3a** MotionSoftDemo C-ABI 狗粮（可并行）；**Z3b** WinMD 狗粮（须 Z2 LOCK + 另票开 API v0）。
4. **Z4** ETW + L1 smoke 闸门 DRAFT；L1≠PERF-CI；与 M3 budget 解耦。
5. **顺序**：Wave A（Z2∥Z4 文档）→ Wave B（Z3a）→ Gate → **另票**才开 API v0。
6. **人日估计**：本窗 8–14 人日；API v0 实现另计（plan 模块7：5–7 人周）。

## 开 API v0 前清单

1. pidan 书面同意本收敛（或后继修订）
2. Z2 可 LOCK 文本（含三表）
3. Z4 评审版 DRAFT
4. Z3a 跑法 + lab 彩排一次
5. **另票**「开 API v0 实现」
6. 基线 tip/branch 声明；PERF-CI HOLD；Y4

## 待 pidan（开放）

- [ ] 审 `api_surface_m2_alpha_contract_DRAFT.md`：修订 / 或回「冻结 API v0」？（冻结仍须另票才编码）
- [ ] Z1：填 B/C 真实应用名；审 `z1_pilot_scoring_table_DRAFT.md`（A 初评 33/50；不代选）？
- [ ] Wave A 还差 Z4 ETW/L1 smoke DRAFT（本批未交）？
- [ ] 后端路径 A=C-ABI 先通 / B=等真实 Composition？
- [ ] M3：r4 WP 条件采用？BUDGET/RENDER-BUDGET 挂账对照 vs SUPERSEDED？
- [ ] 另票开 API v0？（默认否）

## 变更日志

| 时间 | 变更 |
|---|---|
| 2026-11-06 15:28 Asia/Shanghai | 写入 remaining_effort_ESTIMATE；准备推 GitHub |
| 2026-11-06 15:26 AWST | Claude Wave A：`api_surface_…` / `api_v0_frozen` / `z1_pilot_scoring_table` 已核哈希；指针并入本文件；仍不冻结 |
| 2026-11-06 15:19:25 AWST | 建档：合并三方收敛；pidan 指示「后续开发后收敛给一个文档」→ 本文件为唯一主文档 |


## API v0 评审（协调 · 2026-11-06 15:25 AWST）

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
