# Z2–Z4 三方收敛（协调裁决草案）· 2026-11-06 Asia/Shanghai

**状态**：协调收敛草案 · **NOT LOCKED** · 须 pidan 拍板后才生效  
**依据**：Claude / Gemini / Notion 三方意见（`docs/_out/z2_z4_*_converge_20261106.md`）+ `docs/z2_z4_DRAFT.md`  
**硬边界（三方一致，写死）**：Success≠adopt；不合 main；不改 composition.h（除非另 LOCK）；无 APPROVED 不采数；PERF-CI HOLD；Y4 `docs/COORDINATION.md`；pidan 同意前不开 C# API v0 实现；不翻 CLOSED；不用 r4 洗前序 FAIL。

## tip 口径（协调核）

| 角色 | 哈希 | 说明 |
|---|---|---|
| tip_read（采数批准基线） | `e94600332307e4482cf0ff91d4e16d3b60deefc5` | 结题 docs / mainline 所记 |
| results | `6b7a290864ef58ff8b908f8d46532e449c8e4530` | `m3/results_matrix_pacing_r4/` |
| code | `69094ee` | vblank + 0.3·T |
| close-pack tip（workbuddy 推送 ACK） | `907da1eeff3dee09739f63ddd39e41a45f9c6394` | **结题文档推送 tip**；与 tip_read 不同属正常，勿混用 |

写合同/狗粮时：**以 tip_read + results + code 为准**；提及 close pack 时才用 `907da1ee`。

## 三方共识（直接采纳）

1. **Z1** 继续 deferred（暂不定应用）。
2. **Z2 ∥ Z4** 文档优先并行；**Z3** 可先走现有 C-ABI 狗粮。
3. **Wave**：D0 拍板 → Wave A（Z2∥Z4 文档）→ Wave B（Z3a 狗粮）→ Gate → **另票**才开 API v0。
4. 本窗人日 **估计 8–14 人日**（约 1–2 周墙钟 @1 FTE）；**不是**整个 M2 Alpha（plan≈第16周）。
5. r4 条件采用 / 预算 SUPERSEDED 与 Z2–Z4 **正交**；不挡 Z2/Z4 纯文档；但开 API v0 前至少要有「基线 tip / adopt 另议」书面一句。

## 修订合并（协调裁决建议）

### Z2 — 表面合同（只文档）
- 采纳 Claude：写明狗粮可临时用 C-ABI 满足的 **call shapes**，公开 WinMD ≠ C-ABI。
- 采纳 Gemini：合同含 **三表**——(1) L1↔`AnimationHandle`/`MotionSystem`/`fm_capi.h`；(2) 线程/非线程安全写死句；(3) **诚实非目标**（当前无真实 WinRT Composition 后端；v0 允许软件/C-ABI 桥，不得写成已有 Composition 投影）。
- 采纳 Notion：交付物建议名 `docs/api_surface_m2_alpha_contract_DRAFT.md`；**不**把 r4 PASS / offset=0.3·T 写成 API 默认，除非 pidan 另拍板条件采用；LOCK 合同 ≠ 授权实现。

### Z3 — 狗粮（拆 a/b）
- 采纳 Notion（Claude/Gemini 方向一致）：
  - **Z3a**（可先并行）：现有 C-ABI/已验证路径；Play / Retarget≥5 / Cancel；记录 tip/DLL/composition 哈希；Success≠adopt；禁称 API v0 可用。
  - **Z3b**（依赖 Z2 LOCK + **另票开 API v0**）：同形切 WinMD。
- Exit 可裁决句（Gemini）：单次会话 Retarget≥5 且位置/速度连续可观察；Cancel 后无残留调度；结果只进 lab 文件。
- 若跑在 `69094ee`/0.3·T 上：旁注「实验 tip；adopt 未拍板」。

### Z4 — ETW + L1 smoke（先文档）
- 三方一致：L1 smoke ≠ PERF-CI；不解冻 PERF-CI；不折入 M3 budget 单元格；不解冻 Bv；不重开 S1/R-3。
- 采纳 Gemini：优先复用 M2-02 PresentStats / FluidMotion 事件，只增量 Play/Retarget/Cancel/ReduceMotion。
- 采纳 Notion：建议文件 `docs/z4_etw_l1_smoke_gate_DRAFT.md`；INVALID 含未 LOCK/未 APPROVED 采数、改 composition.h、影子 IV、把 PERF-CI HOLD 当绿。

## 开 C# API v0 编码前清单（合并）

1. pidan **书面同意**修订后的 Z2–Z4（本收敛或后继）。
2. Z2 达可 LOCK 文本（含三表 + 非目标 + C-ABI 映射）。
3. Z4 至少有评审版 DRAFT（采数另需 LOCK+APPROVED；本阶段默认可不采数）。
4. Z3a 跑法存在，且 lab 至少彩排一次 Play/Retarget≥5/Cancel。
5. **另票**「开 API v0 实现」同意（≠ 同意 Z2–Z4 DRAFT）。
6. 基线声明：对照 tip/branch；是否含 `69094ee`；composition.h 仍冻；PERF-CI HOLD；Y4。
7. **不要求**：Z1 点名；合 main；解冻 PERF-CI；正式 SUPERSEDED（建议有，但可用「挂账对照」一行代替）。

## 日历（估计）

| 阶段 | 内容 | 并行 | 人日（估计） |
|---|---|---|---|
| D0 | pidan 拍本收敛 +（可选）M3 挂账一句 | — | 0 |
| Wave A | Z2 合同 + Z4 闸门 DRAFT | 互并行 | Z2 3–5；Z4 2–3 |
| Wave B | Z3a C-ABI 狗粮 | 可与 Wave A 后半并行 | 3–6 |
| Gate | 协调评审 → pidan 另票是否开 API v0 | — | 1–2 |
| 之后 | API v0 编码（plan 模块7：5–7 人周，另计） | Z3b / Z4 LOCK+采数另议 | — |

## 待 pidan 拍板

**A. Z2–Z4 修订**  
是否采纳本收敛（含 Z3a/b、三表、诚实非目标、L1≠PERF/budget）？

**B. 后端路径（写进 Z2）**  
A=软件/C-ABI 桥先通（三方推荐） / B=等真实 WinRT Composition（选 B 则不开 v0 编码）

**C. M3 挂账（正交，可分别答；默认可继续跳过采数）**  
1. r4 WP（offset=0.3·T / `69094ee`）：条件采用范围 / 不采用 / 仍挂  
2. RENDER-BUDGET + BUDGET DRAFT：挂账对照 / 正式 SUPERSEDED / 仍并行  
3. PERF-CI：仍 HOLD？（默认是）

**D. 是否另票开 C# API v0**（默认否；须 A 同意后才谈）

未定前：可开 Z2/Z4 **纯文档**；**不开** API v0 编码；**不 LOCK 采数**；**不授权真机**。


---
**唯一主文档**：后续请改 [`docs/Z_SERIES_CONVERGE.md`](Z_SERIES_CONVERGE.md)；本文保留为三方收敛快照。
