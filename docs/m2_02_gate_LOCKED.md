# M2-02 门槛闸门（m2_02_gate_LOCKED.md）

STATUS=LOCKED
LOCKED_AT: 2026-10-25 00:42:43 AWST
GATE_BODY_SHA256: ce147d212d31910b84c6c8e5566fbf50e740efcaadb27747cde827db2ec0a180
> GATE_BODY_SHA256 计算约定（与 t_a1a / H5 / artifacts/p16_52_a_v2_gates.txt 一致）：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。本行写入后不得再改 body。

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

**本 LOCK 不授权采数。** 须 APPROVED_* 非空后，才可派 workbuddy。
**本票结论不解冻 B_v / 不重开 R-2 / 不重开 S1 / R-3；不改 composition.h；禁止影子 IV；性能 CI ≥2 机另票。**
补洞依据：docs/notion_m2_02_gate_revise.md + docs/gemini_m2_02_design.md + docs/deepseek_m2_02_review.md
前序：M2-01 CLOSED PASS；M2-H5-SEED CLOSED PASS。本票对齐 plan.md M2 Alpha。

---

## G0 硬边界
- 不重开 S1 / R-3 / R-2；不解冻 / 不升级 B_v。
- 不改 `composition.h`（sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）；PRE=POST 必须一致。
- 成功 ≠ 采用；旁路挂账不挡本票，本票也不清旁路。
- C# / 托管封装 **禁止影子 IV**：须 WGC/屏一致速度 seed，或降级 IV=null+0；**禁止** spring.h 闭式/影子速度作 compositor `InitialVelocity`（承接 H5 CLOSED PASS）。
- 未 LOCKED+APPROVED 禁止采数与真机验收跑。
- **性能 CI ≥2 物理机 = 另票**，不进本票 PASS。

## G1 票目标（三项；票级 PASS ⇔ 三项均 PASS）

### G1-1 C# API 可用 + 可打断重定向
- 验收句：C# 托管 API（L1 或等价）须具备 Play / Cancel / RetargetTo（或等价 Retarget）的可打断重定向能力，且连续性从当前实时状态接管、无可见跳变。
- Smoke（Gemini，写死方向）：初始化托管引擎并绑定测试 UIElement → Play → 中途 RetargetTo 新目标或 Cancel → 观察位置/速度连续性（无硬切、无瞬时位移跳变、无异常速度清零感）。
- 细则见 G2-LOCK-2。

### G1-2 ETW 可用
- **最小集冻结（6 项）**：`AnimationLifecycle`、`Retarget`、`FrameTick`、`PresentStats`、`IdleTransition`、`TokenLoad`。
- `LoweringDecision`：schema 可保留，**不进**本票最小验收集 / 判分。
- `NoiseProbe`：**移出**本票最小集（挂账；M2-02 不触发）。
- Verbose：`FrameTick` 默认关，验收采集时按需开；其余 5 项 Information 级默认开。
- 验收句：「ETW 可用」= 最小集 6 事件在真机可采集且可进 WPA——打开 provider 能看到上述事件名，且 ≥1 会话内 `AnimationLifecycle` / `Retarget` / `FrameTick` / `IdleTransition` 各有出现。
- GUID / 事件号 / 负载布局**未冻结**（归实现任务）；本闸门只锁事件名与可观测性。
- 细则见 G2-LOCK-3。

### G1-3 预览宿主 v0
- 验收句：预览宿主成功加载指定 token 动画，并在点击播放后完整呈现、无视觉崩溃即为 PASS。
- Smoke（Gemini，≤6 步）：启动宿主 → 导入合规 `motion.json` → 选 ≥1 条 token 动画 → 加载绑定 → 播放 → 观察呈现无崩溃。
- 细则见 G2-LOCK-4。

## G2 判据（写死）

- **G2-LOCK-1 会话与样本量**：1 会话 = 独立进程 + 全新引擎实例 + 全新 sessionId（同进程重启不算）。样本量 ≥3 个会话，或等价 CI 矩阵（≥3 个「二进制×机器×轮次」组合，LOCK 附件列全亦可）。
- **G2-LOCK-2 C# 子目标**：≥1 会话跑通 L1 smoke：Create → LoadTokens → Play → Retarget ≥5 次（须覆盖：播放中途打断、同属性改目标、跨动画切换，各 ≥1）→ Cancel。判据 = 每次 Retarget 从当前状态接续，应用内渲染序列无跳变；Play / Cancel / Retarget 三项均须命中。
- **G2-LOCK-3 ETW 子目标**：≥1 真机会话采集，`AnimationLifecycle` / `Retarget` / `FrameTick` / `IdleTransition` 各 ≥1 次；产物可进 WPA，打开 provider 可见最小集 6 事件名。
- **G2-LOCK-4 预览宿主 v0**：≥1 步挂钩——宿主可加载并预览 ≥1 条 token 动画（起/止/取消其一即可）；按 G1-3 smoke 执行，无视觉崩溃。
- **G2-LOCK-5 PASS**：G2-LOCK-2/3/4 三项子目标全部达成，且判据可复核。
- **G2-LOCK-6 FAIL**：样本量达标（≥3 会话或等价矩阵）但任一子目标未达。
- **G2-LOCK-7 INVALID**（任一即整票 INVALID，不允许补测拼凑）：  
  a. 改 `composition.h`（sha ≠ `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）；  
  b. 未 LOCKED+APPROVED 即采数/真机验收；  
  c. 接影子 IV（未走 WGC/屏一致 seed 或 IV=null+0）；  
  d. 口径偏离本 G2（改最小集、改会话定义、改 PASS 门槛等）；  
  e. 重开 S1/R-3/R-2，或解冻/升级 Bv；  
  f. FAIL 后追加补测回填拼凑。
- **G2-LOCK-8 性能 CI ≥2 机**：**另票**。本票只锁可用性；≥2 机依赖硬件排期；另票可复用已冻结的 `PresentStats`。
- **G2-LOCK-9 通则**：成功 ≠ 采用；旁路挂账不挡本票。

## G3 禁令（结题必须重申）
不重开 S1/R-3/R-2；不改 composition.h；成功≠采用；不解冻/升级 Bv；禁止影子 IV（须 WGC/屏一致 seed 或 IV=null+0）；旁路挂账不挡、本票也不清旁路；性能 CI ≥2 机另票。

## G4 ONELINE 模板
M2-02 | gate=<hash> @ <LOCKED_AT> | approved=<who>@<when> | csharp=<P/F/I> | etw=<P/F/I> | preview=<P/F/I> | ticket=<PASS/FAIL/INVALID> | 成功≠采用；不改composition.h；禁影子IV；perfCI另票
