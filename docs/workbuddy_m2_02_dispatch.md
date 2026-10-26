# workbuddy 派单：M2-02（自包含）

派单时间：2026-10-25 01:11:43 AWST
执行环境：Windows DESKTOP，经 CodeBuddy 执行。

## 授权
- 闸门：`docs/m2_02_gate_LOCKED.md`（STATUS=LOCKED，LOCKED_AT=2026-10-25 00:42:43 AWST，GATE_BODY_SHA256=ce147d212d31910b84c6c8e5566fbf50e740efcaadb27747cde827db2ec0a180）
- 批准：`docs/m2_02_approved.md`（APPROVED_BY=pidan，APPROVED_AT=2026-10-25 01:11:43 AWST）
- **严格逐字遵循 LOCKED 闸门**；不得修改闸门 body / hash；不得在本闸门下追加补测（违规 → INVALID，须新 LOCK）。

## 范围（仅 M2-02，依 LOCKED G0–G4）
票级 PASS ⇔ G1 三项均 PASS（G2-LOCK-2/3/4）。

### G1-1 / G2-LOCK-2 C# API + 可打断重定向
- ≥1 会话：Create → LoadTokens → Play → Retarget ≥5 次（须覆盖：播放中途打断、同属性改目标、跨动画切换，各 ≥1）→ Cancel。
- 判据：每次 Retarget 从当前状态接续，应用内渲染序列无跳变；Play / Cancel / Retarget 三项均须命中。
- Smoke 方向见 `docs/gemini_m2_02_design.md`；API 草案 `docs/deepseek_m2_02_api_draft.md`（§3.4 **禁止影子 IV**：须 WGC/屏一致 seed 或 IV=null+0）。

### G1-2 / G2-LOCK-3 ETW 可用
- 最小集冻结（6）：`AnimationLifecycle`、`Retarget`、`FrameTick`、`PresentStats`、`IdleTransition`、`TokenLoad`。
- LoweringDecision 不进判分；NoiseProbe 移出；FrameTick Verbose 默认关。
- ≥1 真机会话：`AnimationLifecycle` / `Retarget` / `FrameTick` / `IdleTransition` 各 ≥1；产物可进 WPA，provider 可见最小集 6 事件名。

### G1-3 / G2-LOCK-4 预览宿主 v0
- ≥1 步挂钩：宿主加载并预览 ≥1 条 token 动画；按 Gemini smoke（启动→导入 motion.json→选 token→加载→播放→观察）无视觉崩溃。

### 会话（G2-LOCK-1）
- 1 会话 = 独立进程 + 全新引擎 + 全新 sessionId。
- 样本量 ≥3 会话，或等价 CI 矩阵（≥3 个「二进制×机器×轮次」，须在产物列全）。

### PASS / FAIL / INVALID
- PASS：G2-LOCK-2/3/4 全达且可复核。
- FAIL：样本量达标但任一项未达。
- INVALID：改 composition.h；未授权采数；接影子 IV；口径偏离；重开 S1/R-3/R-2 或解冻 Bv；FAIL 后补测拼凑。
- **性能 CI ≥2 机 = 另票**，不进本票 PASS。

## 硬禁令（G0/G3）
- 不改 `composition.h`；开工前 PRE 与结题 POST 各校验 sha256 = `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`，不符即停并报 INVALID。
- 不重开 S1 / R-3 / R-2；不解冻 / 不升级 B_v。
- **禁止影子 IV**（须 WGC/屏一致 seed 或 IV=null+0）。
- 成功 ≠ 采用；旁路挂账不挡本票，本票也不清旁路。

## 产出
- 全部写入 `artifacts/m2_02_*`（C# smoke 日志/截图或序列证据、ETW 采集产物、预览宿主 smoke 证据、composition.h PRE/POST hash、会话矩阵说明）。
- 结题文件：`docs/m2_02_result.md`，含三项判定、证据路径、G3 禁令重申，以及 ONELINE（G4）：
  `M2-02 | gate=ce147d212d31910b84c6c8e5566fbf50e740efcaadb27747cde827db2ec0a180 @ 2026-10-25 00:42:43 AWST | approved=pidan@2026-10-25 01:11:43 AWST | csharp=<P/F/I> | etw=<P/F/I> | preview=<P/F/I> | ticket=<PASS/FAIL/INVALID> | 成功≠采用；不改composition.h；禁影子IV；perfCI另票`

## 参考
- `docs/m2_02_gate_LOCKED.md`
- `docs/m2_02_approved.md`
- `docs/gemini_m2_02_design.md`
- `docs/deepseek_m2_02_review.md`
- `docs/deepseek_m2_02_api_draft.md`
- `docs/notion_m2_02_gate_revise.md`
