# 【三方审议 · Notion AI 意见】Z2–Z4 DRAFT 工期收敛

- 角色：Notion AI（评审/规格）
- 时区：Australia/Perth（AWST）· 2026-11-06
- 依据（只读）：`docs/z2_z4_DRAFT.md`；`docs/COORDINATION.md`（Y4）；`plan.md`；`docs/m3_matrix_pacing_r4_next.md`；`docs/m3_matrix_render_budget_gate_DRAFT.md` / `docs/m3_matrix_budget_gate_DRAFT.md` 文首 STATUS
- 硬禁遵从：不开 C# API v0 实现；不 LOCK；不授权真机；不改 composition.h；Success≠adopt；不合 main；PERF-CI HOLD；不翻 CLOSED
- tip 核对：结题包/仓库一致记 tip_read=`e94600332307e4482cf0ff91d4e16d3b60deefc5`、code=`69094ee`、results=`6b7a290864ef58ff8b908f8d46532e449c8e4530` @ `claude/m3-matrix`。协调口述 tip `907da1ee` **未见于**本仓结题 docs；**以仓库为准**，请协调侧核对是否另有 tip 推进。

---

## 1) 对 Z2/Z3/Z4 范围与 Exit 的接受或修订（硬边界保留）

### 硬边界（三票共用 · 保留写死）
- Success ≠ adopt；不合 `main`；不改 `composition.h`（除非另 LOCK）
- 无 APPROVED 不采数；LOCK ≠ 采数；PERF-CI **HOLD**（除非 pidan 另说）
- API 表面在同意前 **只文档**；**pidan 同意本 DRAFT（或修订版）之前，不开 C# API v0 实现**（与 `z2_z4_DRAFT.md` Ordering 一致）
- Y4：`docs/COORDINATION.md` 已落；写前同步、主线只追加、采数前批准、实验室只加结果、本地改动上报
- Z1 应用暂不定（deferred）— 本意见不重开 Z1 选点

### Z2 — Public C# / WinMD surface contract（只文档）
- **裁决：Accept-with-revision（轻修订）**
- **接受**：冻结 M2 Alpha 必须暴露的 L1：create/load tokens → Play → Retarget（飞行中、同属性、跨动画）→ Cancel；线程/dispatcher；减少动效钩子；非目标（无 HWND EmbedToHwnd、无 WPF）；与 `docs/api.md` / C-ABI 狗粮对齐；公开 WinMD 不与 C-ABI 混用；尚不要求实现 commit。
- **建议 Exit 修订（写入下一版 DRAFT 正文，仍 NOT LOCKED）**：
  1. Exit 明确交付物路径建议：`docs/api_surface_m2_alpha_contract_DRAFT.md`（或协调另定名）→ 经协调侧同意后可升 LOCK 文本；**LOCK 合同 ≠ 授权实现**。
  2. 合同须单列「与现有 C-ABI 狗粮的映射表 / 非混用边界」为可裁决清单（条目级，非散文）。
  3. 合同须声明：**不**把 M3-MATRIX-PACING r4 PASS / offset=0.3·T 写成 API 默认行为，除非 pidan 另拍板「条件采用 WP」并写入合同附录。
  4. APPROVED_* 对 Z2 不适用采数；但 **LOCK 合同**仍须协调同意 + 留证（谁同意、何时）。

### Z3 — MotionSoftDemo dogfood host readiness
- **裁决：Accept-with-revision（边界加硬）**
- **接受**：WinUI 3 宿主作 API v0（或同调用形状临时 C-ABI 桥）第一消费者脚手架；加载 token pack、Composition 上驱动弹簧属性、证明可打断 Retarget；不宣称产品采用；结果只加实验室文件。
- **建议 Exit 修订**：
  1. **分两阶段写死**，避免未 adopt tip 上「伪产品狗粮」：
     - **Z3a（可先并行）**：对 **当前已真机验证过的 C-ABI/Composition 路径** 做 Play / Retarget≥5 / Cancel 可复现脚本（DESKTOP lab）；Success≠adopt；**禁止**暗示 WinMD/API v0 已可用。
     - **Z3b（依赖 Z2 LOCK +「开 API v0」同意）**：同调用形状切到 WinMD/API v0；Exit 才允许称「API v0 狗粮」。
  2. Exit 脚本/跑法须记录：host、tip/sha、dll sha、拓扑（若动 Composition）、是否触碰 composition.h（预期否）。
  3. **明确**：Z3a **不**构成 r4 WP 条件采用；Z3 **不**授权合 main / 不解冻 PERF-CI。
  4. 若狗粮跑在 tip `69094ee` / offset=0.3·T 路径上，须在结果旁注标「实验 tip；adopt 未拍板」。

### Z4 — ETW + L1 smoke gate（先文档）
- **裁决：Accept-with-revision（与 PERF-CI HOLD 对齐）**
- **接受**：最小 ETW 事件集；门控「C# API usable」（M2 Alpha）的 L1 smoke / 票 PASS 规则；会话、格、INVALID；**不把 PERF-CI 降级当绿**；闸门正文不解冻 Bv、不重开 S1/R-3；任何新采数前须 LOCK+APPROVED。
- **建议 Exit 修订**：
  1. 闸门文件建议名：`docs/z4_etw_l1_smoke_gate_DRAFT.md`；STATUS=DRAFT·NOT LOCKED；APPROVED_* 空。
  2. 写死：**L1 smoke ≠ PERF-CI**；PERF-CI 继续 HOLD，除非 pidan 另拍板；L1 PASS **不**自动解冻 PERF-CI。
  3. INVALID 须含：未 LOCK/未 APPROVED 采数；改 composition.h；影子 IV；把 PERF-CI HOLD 当绿。
  4. 与 M3 矩阵闸门族对齐用语（MATCH/INVALID/FAIL 照交），但 **Z4 不重开** MATRIX/PACING r1–r3 FAIL，也不用 r4 洗票。

### 总评（范围）
- **三票范围整体可接受**；修订点集中在：Z2 不绑定未 adopt 的 pacing WP；Z3 拆 a/b 防过早 WinMD；Z4 显式隔离 PERF-CI HOLD。
- **不接受**任何把「开 C# API v0 编码」塞进 Z2–Z4 Exit 的写法。

---

## 2) 建议日历顺序、可并行点、人日估计（标「估计」）

对齐 `plan.md`：M2 Alpha≈第16周；首试点≈第22周；6人全职约30–34周（**估计**）。当前为 M0–M3 真机验证主线收尾区；Z 序列是 **M2 Alpha 入场文档闸**，不是把日历直接拨到第16周实现。

### 建议顺序（文档优先）
| 阶段 | 内容 | 并行 | 人日（估计） | 依赖 |
|---|---|---|---|---|
| **D0** | pidan 拍板：是否接受/修订 `z2_z4_DRAFT.md`；r4 条件采用 / DRAFT SUPERSEDED（见 §4） | — | 0（决策） | pidan |
| **D1** | **Z2** 合同短文 DRAFT（映射表+非目标+线程规则） | 与 Z4 文档并行 | **3–5 人日（估计）** | D0 同意开写；读 `api.md` |
| **D1′** | **Z4** ETW+L1 smoke 闸门 DRAFT | 与 Z2 并行 | **2–4 人日（估计）** | D0；Y4 采数规则 |
| **D2** | 协调评审 Z2/Z4 DRAFT → 可 LOCK 文本（仍可不实现） | 串在 D1/D1′ 后 | **1–2 人日（估计）** | 协调+Notion/Claude |
| **D3a** | **Z3a** C-ABI/现路径狗粮脚本（lab 可复现） | 可与 D1 后期部分并行 | **3–6 人日（估计）** | 实验室窗口；**不**需 API v0 |
| **D3b** | **Z3b** WinMD/API v0 同形狗粮 | **禁止**早于「开 API v0」同意 | **5–10 人日（估计）** | Z2 LOCK + 开码同意 +（建议）r4 adopt 决断 |
| **D4** | 开 C# API v0 **实现**（本意见范围外；仅占位） | 在前置清单完成后 | 见 `plan.md` 模块7：**5–7 人周（估计）** | §3 清单全绿 |

### 可并行点（写死）
- **Z2 文档 ∥ Z4 文档**（DRAFT 已写）
- **Z3a ∥ Z2/Z4 文档后半**（对着现 C-ABI；不宣称 WinMD）
- **不可并行**：Z3b / API v0 编码 ∥ 未 LOCK 的 Z2；任何真机采数 ∥ 无 APPROVED

### 与 plan.md 周次的对齐方式（勿假精确）
- 把 D0–D2 视作 **进入 M2 Alpha 前的合同/闸门缓冲**（**估计 1–2 周日历**，视评审回合）
- 首试点仍瞄准约第22周；**Z1 应用未定** → 试点日程保持挂起，不在本意见里假排

### 对开放挂账的日程依赖
- 若 pidan **暂不**条件采用 r4 WP：Z3a 仍可在「已验证过的非 offset 或旁注实验 tip」上跑，但合同/狗粮须标注实验性；**不阻塞** Z2/Z4 文档。
- 若 pidan **正式 SUPERSEDED** RENDER-BUDGET/BUDGET DRAFT：可减少并行预算票干扰，利于 Z4 只谈 L1 smoke。
- 若坚持保留预算 DRAFT 并行：Z4 与预算票 **抢实验室窗口** 的风险上升 → 日程加缓冲（**估计 +2–4 人日**协调税）。

---

## 3) 开 C# API v0 编码前必须完成什么

**门禁清单（全部完成后才允许开码；本条本身不授权开码）**

1. **pidan 书面同意**修订后的 `docs/z2_z4_DRAFT.md`（或等价 LOCK 纪要）：允许进入 Z2/Z4 文档收敛；**仍不等于**开 API v0。
2. **Z2 表面合同**落入 `docs/` 且经协调同意，达到「可 LOCK」文本；建议实际 **LOCK**（合同 LOCK ≠ 实现授权，但开码前应有冻结面）。
3. **Z4** `*_gate_DRAFT.md` 经评审；若开码后立刻要 L1 采数，则另需 **LOCK + APPROVED_***（Y4）；开码本身可先不采数，但 **不得**无闸门先写实现再补定义。
4. **明确「开 API v0 实现」的单独同意**（pidan/协调派工单）：与「同意 Z2–Z4 DRAFT」分立；一票一事。
5. **M3 挂账决断至少落地一项最小集**（见 §4）：  
   - r4 **条件采用 WP** 的是/否/范围；或书面「编码基线 tip=____，adopt 另议」；  
   - RENDER-BUDGET / BUDGET DRAFT：**挂账对照 / 正式 SUPERSEDED / 仍并行** 三选一（勿擅自 LOCK 采数）。
6. **基线冻结声明**：编码对照 tip/branch、是否含 `69094ee` offset=0.3·T、composition.h 哈希仍冻结、PERF-CI 仍 HOLD。
7. **Y4 合规**：写前同步 tip；主线 brief 只追加；本地旁路改动上报路径。
8. **非目标再确认**：无 EmbedToHwnd、无 WPF 作 v1.0；WinMD 不与 C-ABI 混用。
9. **禁止项仍有效**：不合 main；Success≠adopt；不翻 CLOSED MATRIX/PACING r1–r3；不用 r4 洗前序 FAIL。

> 缺任一条 → **继续暂停 API v0 实现**（与现行 DRAFT Ordering 一致）。

---

## 4) 相对当前 M3 挂账的风险

| 风险 | 来源（文件级） | 对 Z2–Z4 / API v0 的影响 | 缓解（建议，待拍板） |
|---|---|---|---|
| **r4 条件采用未拍板** | `m3_matrix_pacing_r4_next.md` 开放 B | Z3 若默认绑 offset=0.3·T tip，易把实验 WP 误写成产品基线 | Z3 旁注「实验 tip」；合同不默认 offset；pidan 书面 adopt 范围 |
| **tip 口径不一致** | 协调口述 `907da1ee` vs 仓内结题 `e9460033…` / code `69094ee` | 写合同/狗粮时对错 tip → 废跑 | **以仓库结题为准**；协调核验 `907da1ee` 是否存在 |
| **RENDER-BUDGET / BUDGET DRAFT 仍挂账** | 两份 DRAFT 均为 NOT LOCKED；next 建议 SUPERSEDED 候选 | 可能再派预算真机，与 Z3a/Z4 抢窗；或误 LOCK 预算票 | pidan 选：挂账对照 / 正式 SUPERSEDED / 并行；**勿擅自 LOCK** |
| **offset=0.3·T WP 未 adopt** | r4 PASS 但 Success≠adopt | API 时间线/出帧语义若暗含 offset，C# 表面会泄漏实验参数 | Z2 合同将 offset 列为「实验附录 / 非默认」直至 adopt |
| **不合 main + PERF-CI HOLD** | Y4 + 各结题硬禁 | 狗粮绿 ≠ 可合入；L1 smoke 绿 ≠ PERF 绿 | Z4 正文隔离；发布话术禁「已采用」 |
| **Z3 在未 adopt tip 上狗粮** | Z3 Exit 现写「API v0 或临时 C-ABI」 | 易对外解读为产品就绪 | 拆 Z3a/Z3b；结果目录只加 lab 文件 |
| **前序 FAIL 禁翻 / 禁洗票** | r1–r3/MATRIX CLOSED FAIL | 若用 r4 全绿回写旧票，治理破产 | 结题只引用旁证；标签不回改 |
| **Z1 应用未定** | Z2–Z4 DRAFT / plan §7 | 狗粮成功无法对接真实试点反馈 | 保持 Z1 deferred；不把 Z3 当试点 |
| **过早开 API v0** | 现行 Ordering 已禁 | 表面漂移、返工、与 ETW 事件集错位 | 守 §3 门禁；三方商议后仍须 pidan 拍板 |

### 风险一句话
M3 主线 **验证证据已够归档 r4 PASS**，但 **采用/预算挂账/PERF-CI 仍开着**；Z2–Z4 应先收敛 **文档合同与 L1 定义**，把「实验 tip 全绿」与「可编码 / 可 adopt」拆开，否则最大风险是 **未拍板 adopt 的 WP 渗进公开 API 与狗粮叙事**。

---

## 待 pidan 拍板（一行汇总）

请拍板：①是否接受本意见对 Z2/Z3a·b/Z4 的 Exit 修订；②r4 WP（offset=0.3·T / tip `69094ee`）条件采用范围（或明确不采用）；③RENDER-BUDGET+BUDGET DRAFT 挂账对照 vs 正式 SUPERSEDED vs 仍并行；④PERF-CI 是否仍 HOLD；⑤**是否另票授权**开 C# API v0 实现（默认否）。以上未定前：Z2/Z4 可继续纯文档；**不开 API v0 编码；不 LOCK 采数闸；不授权真机。**

— Notion AI（三方审议意见 · 协调侧收敛用 · 最终须 pidan 拍板）
