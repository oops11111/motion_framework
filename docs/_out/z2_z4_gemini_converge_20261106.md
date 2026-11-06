# Gemini · Z2–Z4 DRAFT 工期收敛意见（2026-11-06 Asia/Shanghai）

只文档意见；未改账号、未花钱、未 LOCK、未派真机。依据：docs/z2_z4_DRAFT.md、docs/COORDINATION.md、plan.md、docs/api.md、docs/m3_matrix_pacing_r4_next.md。硬边界全部保留：Success≠adopt；不合 main；不改 composition.h（除非另 LOCK）；无 APPROVED 不采数；PERF-CI HOLD；Y4 写前同步 / 状态只追加 / 实验室只加结果。

## 1) 对 Z2/Z3/Z4 范围与 Exit：接受 + 小修订

**总评**：接受 Z2–Z4 作为「M2 Alpha 前置文档三角」；接受「pidan 同意本 DRAFT（或修订版）之前不开 C# API v0 实现」。Z1 继续 deferred 无异议。

**Z2（公开 C#/WinMD 表面合同 · 只文档）— 接受，修订 Exit 口径**
- 范围 OK：create/load tokens → Play → Retarget（飞行中、同属性、跨动画）→ Cancel；线程/dispatcher；减少动效钩子；非目标（无 HWND EmbedToHwnd、无 WPF）；公开 WinMD 不与 C-ABI 混用。
- **修订建议**：Exit 除「短合同 DRAFT→可 LOCK 文本 + 协调同意」外，合同正文须含三表：
  1) L1 方法/类型清单 ↔ 现有 `AnimationHandle`/`MotionSystem`/`fm_capi.h` 对照；
  2) 线程亲和与「非线程安全」写死句；
  3) **诚实非目标**：当前无真实 WinRT Composition 后端（api.md §6/§8），v0 允许「软件路径 / 临时 C-ABI 桥」但**不得**在合同里写成已有 Composition 投影。
- 尚不要求实现 commit — 同意保留。

**Z3（MotionSoftDemo 狗粮）— 接受，修订依赖与 Exit 可裁决性**
- 范围 OK：第一消费者脚手架；Play / 可打断 Retarget / Cancel；不宣称产品采用。
- **修订建议**：
  - 明确允许 **先对着当前 C-ABI/Composition 软件路径** 做狗粮，**不阻塞于 WinMD 落地**；Z2 LOCK 后再切投影形状。
  - Exit 写死可裁决句：「单次连续会话内 Retarget≥5 次且位置/速度连续可观察；Cancel 后无残留调度；结果只进实验室文件」。
  - 禁止把狗粮 PASS 写成 adopt / 合 main / 解冻 PERF-CI。

**Z4（ETW + L1 smoke 闸门 · 先文档）— 接受，修订与 M3/PERF 边界**
- 范围 OK：最小 ETW 事件集 + 门控「C# API usable」的 L1 smoke / 票 PASS 规则。
- **修订建议**：
  - L1 smoke **不得**把 PERF-CI 绿或 M3 matrix budget 单元格当前置；PERF-CI 仍 HOLD。
  - 事件集优先复用已冻结的 M2-02 PresentStats / FluidMotion-Motion 定义（若仍有效），只增量「Play/Retarget/Cancel/ReduceMotion」生命周期，避免另起一套。
  - Exit：`*_gate_DRAFT.md` 经评审即可；**任何新采数**仍须 LOCK+APPROVED；闸门正文不解冻 Bv、不重开 S1/R-3、不回改 CLOSED M3 标签。

## 2) 建议日历顺序、可并行点、人日（均为**估计**）

**建议顺序（日历）**
1. **D0（本拍板）**：pidan 对 Z2–Z4 DRAFT（含上列修订）点头；同步拍板 M3 挂账（见 §4）— 文档决策，不采数。
2. **D1–D5**：**Z2 ∥ Z4** 纯文档并行（最高优先）。
3. **同窗或紧随**：**Z3** 用现有 C-ABI/软件路径并行脚手架（不依赖 WinMD commit）。
4. **Z2 文本 LOCK（或协调书面同意可 LOCK 稿）之后**：才允许开 C# API v0 **编码**（仍不开采数；仍不合 main）。
5. Z1 / 真 Composition 后端 / PERF-CI / 合 main：**不在本收敛窗**。

**可并行**
- Z2 文档 ↔ Z4 文档（无代码依赖）
- Z3 C-ABI 狗粮脚手架 ↔ Z2/Z4 文档（Z3 不写死 WinMD 名亦可先跑）
- M3「条件采用 WP / SUPERSEDED 预算 DRAFT」doc-only 收敛 ↔ Z2/Z4（勿插队采数）

**人日估计（1 名熟悉仓库的 C#/文档人 + 协调审阅；估计）**
| 项 | 估计人日 | 备注 |
|---|---|---|
| Z2 短合同 + 对照表 | **2–4** | 含与 api.md/fm_capi 对齐；不含实现 |
| Z4 ETW+L1 smoke DRAFT | **2–3** | 复用 M2-02 事件则偏下限 |
| Z3 狗粮脚手架+复现脚本（C-ABI） | **3–5** | 不含 WinMD；不含新采数矩阵 |
| 三方评审收敛 + pidan 拍板卡 | **1–2** | 协调侧 |
| **合计（本收敛窗）** | **约 8–14 人日** | 日历约 **1–1.5 周**（若单人串行偏上限） |

相对 plan.md「M2 Alpha≈第16周」：本窗只清 Alpha **文档与狗粮前置**，**不能单独宣告 Alpha**；真实 Composition 后端与 WinMD 投影仍是后续大块（plan 模块 7 估计 5–7 人周，另计）。

## 3) 开 C# API v0 编码前必须完成什么

**必须（建议写成拍板清单）**
1. pidan **书面同意**本 Z2–Z4 DRAFT（或本意见修订版）。
2. **Z2** 短合同达到「可 LOCK」文本，且协调侧同意；至少含 §1 三表与诚实非目标。
3. **Z4** gate 至少完成评审版 DRAFT（编码可先于 Z4 LOCK；**但**若编码伴随新采数则必须先 LOCK+APPROVED — 默认本阶段不采数）。
4. 明确 v0 后端路径二选一（写进 Z2）：**A** 软件/C-ABI 桥先通（推荐，与现状一致）；**B** 等真实 WinRT Composition — 若选 B 则**不要**开 v0 编码。
5. **Y4**：写前同步 tip；状态日志只追加；无 APPROVED 不开 lab。
6. **M3 挂账最低闭合（doc-only）**：见 §4 — 至少记录「pidan 跳过/挂账/SUPERSEDED」之一，避免 API 叙事与矩阵挂账冲突。

**明确不要求（开编码前）**
- Z1 试点点名；合 main；解冻 PERF-CI；改 composition.h；翻 CLOSED M3 标签；新真机矩阵采数。

## 4) 相对当前 M3 挂账的风险

现状对齐：M3-MATRIX-PACING **r4 CLOSED PASS**（tip 907da1ee 叙事 / 结题写 tip 基线与 offset=0.3·T）；Success≠adopt；不合 main；**有条件采用 WP** 与 **RENDER-BUDGET/BUDGET DRAFT 挂账或 SUPERSEDED** 仍开放（pidan 跳过）。

**风险（按优先级）**
1. **WP 条件采用未拍板** → Z3/API v0 狗粮若默默依赖 `pacing_offset_fraction=0.30` 实验 tip，日后「不采用」会导致表面合同与实验室行为分叉。（缓解：Z2/Z3 写明狗粮 tip 范围=实验分支；adopt 另令。）
2. **预算 DRAFT 挂账未闭合** → Z4 L1 smoke 易被误读为「还要再吃 budget 单元格」。（缓解：Z4 正文写死与 M3-MATRIX-BUDGET/RENDER-BUDGET **解耦**；SUPERSEDED/挂账由 pidan 一句结案即可。）
3. **Success≠adopt + 不合 main** → 对外/对试点若提前说「C# API 可用」，会被当成产品采用。（缓解：Z2 Exit 与发布说明固定「实验室可用 ≠ 产品采用」。）
4. **无真实 Composition 后端**（api.md）→ WinMD 合同若写成 Composition 已接线，实现会欠债或假绿。（缓解：§1 Z2 诚实非目标；Z3 先 C-ABI。）
5. **PERF-CI HOLD** → 不可用性能 CI 绿给 Alpha 背书；Z4 只能做功能 smoke。（缓解：已在 Z4 修订。）
6. **文档债堆积**（r1/r2/r3 FAIL 禁翻 + r4 PASS 旁证）→ 新人易洗票。（缓解：Z 系列禁令复述「不回改 CLOSED；不用 r4 洗前序 FAIL」。）

**对「pidan 跳过 SUPERSEDED」的建议**：允许继续跳过采数，但请在 COORDINATION/主线 brief **追加一行**「BUDGET/RENDER-BUDGET DRAFT = 挂账对照，本阶段不 LOCK、不采数」——否则 Z2–Z4 收敛时每次都要重解释。

## 拍板请求（请转 pidan）

1. 是否采纳上述 Z2/Z3/Z4 **修订**（三表 + C-ABI 先行狗粮 + L1 与 PERF/M3 budget 解耦）？
2. 是否确认 **同意 DRAFT 前不开 C# API v0 编码**（与现稿一致）？
3. M3：条件采用 offset=0.3·T WP？BUDGET/RENDER-BUDGET → 挂账对照 / 正式 SUPERSEDED / 仍并行？（三项可分别答；默认可继续跳过采数。）

— Gemini网页版（三方审议意见；最终须 pidan 拍板）
