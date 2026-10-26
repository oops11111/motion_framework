# DeepSeek · M2-02 复核补洞（ETW 最小集 + G2）

- 时间：2026-10-25（Australia/Perth, UTC+8）
- Harness：session-`4825fd9d-b0da-4160-8463-86159fd4d028`；model=deepseek-v4-pro；reasoningEffort=max
- 状态：**文档 only**；不授权采数；不改 DRAFT/LOCKED 闸门本体；不改 composition.h
- 关联修订：`docs/deepseek_m2_02_api_draft.md` §3.4 已改为「禁止影子 IV / 须 WGC 或 IV=null+0」（承接 H5 CLOSED PASS）
- 原始回复：`/workspace/tmp_m202/dsh_m2_02_review_reply.md`（含推理草稿，正式稿以下方正文为准）

---

## M2-02 复核补洞（DRAFT，文档 only）

- 票：M2-02 复核补洞（G2 闸门条文补洞 + ETW 最小集冻结建议）
- 状态：仅文档；不授权采数、真机与实现
- composition.h sha256：`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（未改）

---

### 段 A — ETW 最小事件集冻结建议

对 api_draft §2 事件 1–7 逐条：

1. `AnimationLifecycle` — **保留**：动画终态主链，C# Play/Cancel/终态验收的唯一直接对账点。
2. `Retarget` — **保留**：可打断重定向的核心观测，L1 子目标连续性对账。
3. `LoweringDecision` — **降级**（保留在 schema、不进最小验收集）：真实合成器后端不存在，M2-02 真机多为 AppThread/Hold，对验收无增值。
4. `FrameTick` — **保留**：引擎帧/活跃计数基线；Verbose 级默认关。
5. `PresentStats` — **保留**：丢帧/间隔汇总，性能 CI 与 PresentMon 对照（CI 另票也先冻结事件）。
6. `IdleTransition` — **保留**：静止即停（ActiveCount 0↔1）验收点。
7. `TokenLoad` — **保留**：预览宿主 smoke 与 token 上屏观测起点。
8. `NoiseProbe` — **移出最小集**：挂账项，M2-02 不触发。

**M2-02 最小集冻结清单（6 项）**：`AnimationLifecycle`、`Retarget`、`FrameTick`、`PresentStats`、`IdleTransition`、`TokenLoad`。

**Verbose 默认关**：`FrameTick`（最小集内唯一 Verbose 项，验收采集时按需开 Verbose 通道）；其余 5 项 Information 级默认开。

**验收一句（写死）**：「ETW 可用」= 最小集 6 事件在真机可采集且可进 WPA——WPA 打开 provider 能看到上述事件名，且 ≥1 会话内 `AnimationLifecycle`/`Retarget`/`FrameTick`/`IdleTransition` 各有出现。

**明确不承诺**：GUID、事件号、负载布局均未冻结（归实现任务定）；`LoweringDecision` 不参与 M2-02 判分；`NoiseProbe` 不进本票。

---

### 段 B — G2 补洞（可粘贴 LOCK 修订）

以下条目可直接粘入 Notion 闸门（M2-02 G2）：

- **G2-LOCK-1 会话与样本量（写死）**：1 会话 = 独立进程 + 全新引擎实例 + 全新 sessionId（同进程重启不算）。样本量 ≥3 个会话，或等价 CI 矩阵（≥3 个「二进制×机器×轮次」组合，LOCK 时列全）。
- **G2-LOCK-2 C# API 子目标（写死）**：≥1 会话跑通 L1 smoke：Create → LoadTokens → Play → Retarget ≥5 次（须覆盖：播放中途打断、同属性改目标、跨动画切换，各 ≥1）→ Cancel；判据 = 每次 Retarget 从当前状态接续，应用内渲染序列无跳变；Play/Cancel/Retarget 三项均须命中。
- **G2-LOCK-3 ETW 子目标（写死）**：≥1 真机会话采集，`AnimationLifecycle`/`Retarget`/`FrameTick`/`IdleTransition` 各 ≥1 次；产物可进 WPA，打开 provider 可见最小集 6 事件名。
- **G2-LOCK-4 预览宿主 v0（写死）**：≥1 步挂钩——宿主可加载并预览 ≥1 条 token 动画（起/止/取消其一即可，不要求全参数）；细节验收由 Gemini 写，本票只冻结「1 步挂钩存在且可执行」。
- **G2-LOCK-5 PASS**：G2-LOCK-2/3/4 三项子目标全部达成，且判据可复核。
- **G2-LOCK-6 FAIL**：样本量达标（≥3 会话或等价矩阵）但任一子目标未达。
- **G2-LOCK-7 INVALID**（任一命中即 INVALID）：改 composition.h；未 LOCK+APPROVED 采数/真机；接影子 IV（须 WGC/屏一致 seed 或 IV=null+0）；口径偏离（补测拼凑、重开 S1/R-3/R-2、解冻或升级 Bv）。
- **G2-LOCK-8 性能 CI ≥2 机：建议另票**。理由一句：本票锁定的是可用性判据，≥2 机依赖硬件排期，并入会让 LOCK 卡在机器供给；另票可复用已冻结的 `PresentStats` 事件。（LOCK 时二选一写死，本建议为「另票」。）
- **G2-LOCK-9 通则**：成功 ≠ 采用；旁路挂账不挡本票。

— DeepSeek（M2-02 复核补洞；文档 only）
