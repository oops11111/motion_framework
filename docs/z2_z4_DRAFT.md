# Z2–Z4 DRAFT（新建，非还原原文）

- **状态**：DRAFT · NOT LOCKED · 2026-11-06 Asia/Shanghai
- **来源**：Claude 网页 bot 按 pidan「先补 Z2–Z4」指令新建；原文从未存在
- **依据**：`plan.md`（WinUI 3 only；M2 Alpha = C# API + 可打断 Retarget + ETW + preview v0；狗粮 = MotionSoftDemo）
- **硬边界**：Success ≠ adopt；不合 `main`；不改 `composition.h`（除非另 LOCK）；无 APPROVED 不采数；PERF-CI HOLD（除非 pidan 另说）
- **Y4**：见 `docs/COORDINATION.md`

## Z1 — Pilot apps（已挂起）

按 `plan.md` §7 提名 1–3 个 WinUI 3 应用（主=友好中等+真痛点；对照=更重 WinUI 3；不要 WPF 作 v1.0 试点）。

**Exit：** pidan 点名应用，或明示继续挂起。  
**Status：** deferred；暂无应用。

## Z2 — Public C# / WinMD surface contract（只文档）

冻结 M2 Alpha 必须暴露的 L1 表面：create/load tokens → Play → Retarget（飞行中、同属性、跨动画）→ Cancel；线程/dispatcher 规则；减少动效钩子；明确非目标（无 HWND EmbedToHwnd、无 WPF）。与 `docs/api.md` / 现有 C-ABI 狗粮对齐，公开 WinMD 不与 C-ABI 混用。

**Exit：** 短合同 DRAFT→可 LOCK 文本落入 `docs/`，协调侧同意；尚不要求实现 commit。

## Z3 — MotionSoftDemo dogfood host readiness

把现有 WinUI 3 宿主做成 API v0（或同调用形状下的临时 C-ABI 桥）的第一消费者脚手架：加载 token pack、在 Composition 上驱动一个弹簧属性、证明可打断 Retarget，不宣称产品采用。

**Exit：** DESKTOP lab 上一份可复现狗粮脚本/跑法，展示 Play / Retarget≥5 / Cancel 连续性；结果只加实验室文件；Success ≠ adopt。

## Z4 — ETW + L1 smoke gate（先文档）

定义最小 ETW 事件集，以及门控「C# API usable」（M2 Alpha）的 L1 smoke / 票 PASS 规则（会话、格、INVALID；不把 PERF-CI 降级当绿）。

**Exit：** `*_gate_DRAFT.md` 经评审；任何新采数前须 LOCK+APPROVED；闸门正文不解冻 Bv、不重开 S1/R-3。

## Ordering

- Z2 与 Z4 文档优先，可并行。
- Z3 可先对着当前 C-ABI/Composition 路径并行，Z2 锁定后再切 WinMD。
- **pidan 同意本 DRAFT（或修订版）之前，不开 C# API v0 实现。**


---
**唯一主文档**：后续请改 [`docs/Z_SERIES_CONVERGE.md`](Z_SERIES_CONVERGE.md)；本文保留为快照。
