# M2-PERF-CI 门槛闸门（DRAFT · NOT LOCKED · 补洞后可 LOCK）

STATUS=DRAFT
DRAFT_AT: 2026-10-25 01:35:00 AWST
GATE_BODY_SHA256: （LOCK 时由协调侧填写；计算约定：对落盘全文做 SHA-256，排除以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希）
APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

**本 DRAFT 不授权采数。** 须 STATUS=LOCKED 且 APPROVED_* 非空后，才可派 workbuddy。
前序：M2-02 CLOSED PASS；本票收口 `plan.md` M2 Alpha「性能 CI 至少在 2 台物理机上运行」。
补洞依据：`docs/notion_m2_perf_ci_gate_revise.md` + Gemini 场景/P95P99 + DeepSeek 对账/矩阵。
可复用：Provider `FluidMotion-Motion`、`PresentStats`（见 `artifacts/m2_02_etw_provider.md`）。

---

## G0 硬边界
- 不重开 S1 / R-3 / R-2；不解冻 / 不升级 B_v。
- 不改 `composition.h`（sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）。
- 成功 ≠ 采用；旁路挂账不挡；禁影子 IV（须 WGC/屏一致 seed 或 IV=null+0）。
- 未 LOCKED+APPROVED 禁止采数。
- **必须 ≥2 台物理机**；仅 1 机 / 虚拟机凑数 / 同机复跑冒充两机 = 整票 INVALID。

## G1 票目标（场景三件套 · 写死）

每台物理机均须跑同一场景集；每场景 PresentMon 有效帧 **≥1000**；每机每场景 **≥2 轮独立进程**。

1. **Idle（闲置零开销）**：启动应用保持静止、无交互，采集 ≥1000 帧。
2. **Steady Play（平稳播放）**：连续加载并循环播放标准卡片展开 token 动画，采集 ≥1000 帧。
3. **Interrupted Retarget（打断重定向负载）**：弹簧动画中途高频连续打断与反向重定向（承接 M2-02 smoke），采集 ≥1000 帧。

### P95 / P99 目标句（写死）
- **P95**：UI 线程框架部分、渲染阶段、GPU 阶段相对刷新间隔 \(T\) 的周期占比分别不超过 **12% / 35% / 35%**，且相对本机同场景基线版本回退 **≤10%**。
- **P99 / 丢帧**：每千帧丢帧率严格 **< 1.0**；丢帧定义写死为 **Frame Duration > 1.5×T**（PresentMon 帧级序列计数）。

### 对账主路径（写死）
- **主判据 = PresentMon**（系统 Present 侧帧级序列：帧间隔、帧数、丢帧）。
- **PresentStats**（Provider `FluidMotion-Motion`）：仅存在性 + 会话汇总对账（dropped / intervalMs 等）；本进程计数 ≤ 系统计数且同向同量级；差 >2× 记口径冲突，报告不可解 = INVALID。
- 相对基线 = 同机、同场景、同采集时长的基线版本。
- 阶段占比（UI/渲染/GPU）优先自 WPA（logman ETL）；EventPipe 降级路径若阶段拆分不可得，仍须以 PresentMon 帧间隔 P95 ≤ \(T\) 作为该轮绝对预算替代，且 P99 丢帧门槛不变（结题须注明替代）。

## G2 判据（写死）

- **G2-LOCK-9（会话/轮次）**：每机 ≥2 轮独立进程；每场景每轮 PresentMon 有效帧 ≥1000；轮次元数据随报告落盘；场景集 = G1 三件套，不得另增未 LOCK 场景凑数。
- **G2-LOCK-10（机器矩阵）**：≥2 台物理机；每机一行写死 hostname / machineId（主机级稳定标识，禁用 PID/会话 ID/随机数）/ GPU 档 / 是否物理机 / 采集轮次 / 采集方式；两机 machineId **互异**且覆盖 **≥2 个 GPU 档**（集显/独显/低端笔记本，按 plan.md）；**1 机 / VM / 同机复跑或改 hostname 冒充 / machineId 重复或不可核验 = 整票 INVALID**。
- **G2-LOCK-11（对账口径）**：主判据 PresentMon；PresentStats 仅会话汇总对账；相对基线 = 同机同场景基线版本；口径偏离本闸门须新 LOCK。
- **G2-LOCK-12（采集方式）**：有管理员权限 → **必须** logman ETL（供 WPA）；仅当目标机无 admin 且 logman ETL 实测 Access denied（拒绝证据留档）时，允许 EventPipe（dotnet-trace，Provider `FluidMotion-Motion`）产出 `.nettrace`。in-proc ledger 允许，仅作 PresentStats 会话汇总载体，不作主判据。每机每轮最低产物：PresentMon 帧级 CSV + ETL（或附拒证的 `.nettrace`）+ PresentStats/ledger 会话汇总；缺任一 = INVALID。
- **G2-LOCK-13（PASS/FAIL）**：票级 **PASS** ⇔ 两机每轮每场景均满足「帧数 ≥1000 ∧ P95 绝对预算（G1）∧ P95 相对回退 ≤10% ∧ 每千帧丢帧 <1.0」且 PresentStats 对账同向同量级且报告可独立复读。任一不满足 = **FAIL**；样本不足或报告不可复读 = 按 INVALID/FAIL 分列（缺产物/不可复读 → INVALID）。
- **G2-LOCK-14（成功≠采用）**：本票 PASS 仅为性能达标证据，不构成采用/合入授权；不得以本票替代 IV 或其它闸门结论。
- **其它 INVALID**：改 composition.h；未授权采数；接影子 IV；补测拼凑；解冻/升级 Bv；重开 S1/R-3/R-2。

## G3 禁令（结题必须重申）
不重开 S1/R-3/R-2；不改 composition.h；成功≠采用；不解冻/升级 Bv；禁影子 IV；旁路挂账不挡；≥2 物理机硬门槛。

## G4 ONELINE 模板
M2-PERF-CI | gate=<hash> @ <LOCKED_AT> | approved=<who>@<when> | machines=<n> | ticket=<PASS/FAIL/INVALID> | 成功≠采用；不改composition.h；禁影子IV；≥2物理机；主判据PresentMon
