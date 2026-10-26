# DeepSeek · M2-PERF-CI 复核补洞

- 时间：2026-10-25（Australia/Perth, UTC+8）
- Harness：session-`2292ee8f-9585-4254-ab16-f680749ad411`；model=deepseek-v4-pro；reasoningEffort=max
- 状态：**文档 only**；不授权采数；不改 DRAFT/LOCKED 闸门本体；不改 composition.h
- 前序：M2-02 CLOSED PASS；本票收口 plan.md「性能 CI ≥2 物理机」
- 原始回复：`/workspace/tmp_perf/dsh_m2_perf_ci_reply.md`（含推理草稿，正式稿以下方正文为准）

---

# M2-PERF-CI 复核补洞（DRAFT；文档 only；不授权采数）

- 前序：M2-02 CLOSED PASS；性能 CI 按 G2-LOCK-8 另票 → 本票；Provider `FluidMotion-Motion`
- composition.h sha256：`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`
- 硬边界：不改 composition.h；禁影子 IV、不重开 S1/R-3/R-2、不解冻 Bv；成功≠采用
- 硬边界：≥2 台物理机（1 机/VM 整票 INVALID）；本轮仅文档、不采数、不改闸门本体

## 段 1 — PresentStats / PresentMon 对账口径

- 写死：PresentStats = 本进程会话汇总（dropped、intervalMs 等聚合字段，Provider `FluidMotion-Motion`）；PresentMon = 系统 Present 侧帧级序列（帧间隔、帧数、丢帧），不区分来源进程。
- 写死：主判据 = PresentMon。P95 = 帧间隔分布 95 分位，判「绝对上限（LOCK 填 Gemini 目标句）」+「相对基线回退」；P99/丢帧 = 帧间隔 > 2×目标帧间隔记 1 次丢帧，判丢帧率（丢帧数/总帧数，目标 LOCK 填 Gemini 目标句）；每场景有效帧 ≥1000。
- 写死：相对基线 = 同机、同场景、同采集时长的基线版本；本版本 P95 相对基线回退超阈值（LOCK 填 Gemini 目标句）即 FAIL。
- 写死：对账 = 同一采集窗口内 PresentStats.dropped 与 PresentMon 丢帧数须同向同量级（本进程计数 ≤ 系统计数；差 >2× 记口径冲突，报告标注，复读不可解 = INVALID）。
- 二选一（已定，写死）：**主判据 = PresentMon；PresentStats 仅存在性 + 会话汇总对账**。本票允许 PresentStats 保持 session-end placeholder，但须含 dropped/intervalMs 汇总并随轮次落盘；升级为可聚合字段另票处理，不在本票采数。
- PASS/FAIL 一句写死：票级 PASS ⇔ 两机每轮每场景均满足「帧数 ≥1000 且 P95 ≤ 绝对上限且 P95 相对回退 ≤ 阈值且丢帧率 ≤ 目标（阈值均 LOCK 填 Gemini 目标句）」且 PresentStats 对账同向同量级；任一不满足 = FAIL；对账产物缺失或不可复读 = INVALID。

## 段 2 — 机器矩阵 / INVALID 补洞

- 每机一行写死字段：hostname / machineId / GPU 档 / 是否物理机 / 采集轮次 / 采集方式。
- machineId 写死：主机级稳定标识（资产号或主板序列号）；禁用进程 ID、会话 ID、随机数冒充。
- 写死：≥2 台物理机；两机 machineId 互异且覆盖 ≥2 个 GPU 档（集显/独显/低端笔记本，按 plan.md 三档）；每机 ≥2 轮独立进程（独立生命周期、独立 ledger 落盘）。
- INVALID 写死：仅 1 台机器 = 整票 INVALID；虚拟机凑数 = INVALID；同机复跑或改 hostname 冒充两机 = INVALID；machineId 重复或不可核验 = INVALID。
- 报告须含每机矩阵行、采集命令与进程起止证据；缺任一 = 报告不可独立复读 = INVALID。

## 段 3 — EventPipe 是否可替代 logman ETL

- 写死允许条件（沿 M2-02 先例）：仅当目标机无管理员权限且 logman ETL 实测 Access denied（拒绝证据留档）时，允许 EventPipe（dotnet-trace，Provider `FluidMotion-Motion`）替代，产出 .nettrace。
- 写死：有管理员权限时不得以省事为由改用 EventPipe；logman 可用即必须用 logman ETL（供 WPA）。
- in-proc ledger 写死：允许，仅作 PresentStats 会话汇总载体（沿 M2-02 先例），不作主判据。
- 对账产物最低要求（每机每轮）：PresentMon 帧级 CSV + ETL（logman 路径）或 .nettrace（EventPipe 降级路径，附拒绝证据）+ PresentStats/ledger 会话汇总；缺任一项 = INVALID。

## 段 4 — G2-LOCK 可粘贴条目（续 G2-LOCK-8 编号）

- **G2-LOCK-9（会话/轮次）**：每机 ≥2 轮独立进程；每场景每轮 PresentMon 有效帧 ≥1000；轮次元数据随报告落盘；场景集以 G1 LOCK 为准（建议 M2-02 smoke + ≥1 打断负载），本票不新增场景。
- **G2-LOCK-10（机器矩阵）**：≥2 台物理机，machineId 唯一且主机级，覆盖 ≥2 GPU 档；1 机 / VM / 同机复跑 / 伪造身份 = 整票 INVALID。
- **G2-LOCK-11（对账口径）**：主判据 PresentMon；PresentStats 仅会话汇总对账（同向同量级）；相对基线 = 同机同场景基线版本；口径引用本票段 1，改动须 LOCK。
- **G2-LOCK-12（采集方式）**：无 admin 且 logman 被拒（留证）→ EventPipe .nettrace 允许；有 admin 必须 logman ETL；产物缺 CSV / ETL（或 .nettrace）/ PresentStats 汇总 = INVALID。
- **G2-LOCK-13（PASS/FAIL）**：票级 PASS ⇔ 两机均达 P95 绝对上限、P95 相对回退、丢帧率目标（阈值 LOCK 填 Gemini 目标句）且报告可独立复读；任一不满足 = FAIL；不可复读 = INVALID。
- **G2-LOCK-14（成功≠采用）**：本票 PASS 仅为性能达标证据，不构成采用/合入授权；不得以本票结论替代 IV 或闸门结论；DRAFT 不授权采数。

— DeepSeek（M2-PERF-CI 复核补洞；文档 only）
