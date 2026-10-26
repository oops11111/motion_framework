# M2-PERF-CI-TOOLING 门槛闸门（m2_perf_ci_tooling_gate_DRAFT.md）

STATUS=DRAFT（**NOT LOCKED**；本文件**不授权采数**；不解除 PERF-CI HOLD）
LOCKED_AT: （LOCK 时填）
GATE_BODY_SHA256: （LOCK 时算；约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希）

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> 起草：Claude（Claude Code 接手会话），2026-10-26。触发：协调在主线待命期间点名 plan.md §8 第 8 项“测试工具与 CI 分析脚本”。依据：`docs/notion_m2_h5_abs_conflict_resolve_next.md`，允许另点名非冲突的文档或工具链票。
> PERF-CI gate `5b007dd43ba351dc0985a52d1745485954d149575feeca622791275005cd282a` **不改、不翻盘**，仍 HOLD（等第二台物理机）。八 hash 和 ABS-RESOLVE `e96bf623…5ade` 也不翻盘。

## G0 硬边界
- mode=**offline-tooling**。采数=**否**；真机=**否**；落地=**否**。
- 不改 `composition.h`（`a55d4952…e418`）。不重开 S1、R-3、R-2。不解冻 Bv。禁影子 IV。
- 本票 PASS 仅表示判分脚本可用，**≠ PERF-CI PASS ≠ 授权采数 ≠ 采用**。

## G1 交付物（已在仓库，等 LOCK 后复核）
| 文件 | 内容 |
|---|---|
| `tools/perf_ci/perf_ci_grade.py` | PERF-CI G2-LOCK-9 至 G2-LOCK-13 的离线判分，只用标准库；校验闸门哈希；没有 APPROVED 文件时判 INVALID；`--dry-run` 永远不出 PASS |
| `tools/perf_ci/test_perf_ci_grade.py` | 合成数据单测，共 29 项（PASS；每种 INVALID 和 FAIL；丢帧边界 1.0/1000；替代路径；对账冲突；CLI 退出码） |
| `tools/perf_ci/README.md` | manifest 格式和判分口径表 |

## G2 判据
- **G2-T1**：`python3 -m unittest discover -s tools/perf_ci` 全部通过（落盘时已过：29/29）。
- **G2-T2**：脚本里写死的哈希等于 `5b007dd4…282a`，并且单测会对落盘的 LOCKED 闸门复算哈希。
- **G2-T3**：下面 Q1 至 Q8 每一条都要由协调给出裁决：(a) 按现有解读接受；(b) 改脚本；(c) 需要 PERF-CI 闸门出新版本（**本票不改** `5b007dd4…282a`）。
- **G2-T4**：脚本不启动采集，不读也不写 `composition.h`。

## 开放问题（LOCK 前须裁决；均来自对 LOCKED PERF-CI 闸门的逐条实现）

| # | 问题 | 现脚本解读 | 影响 |
|---|---|---|---|
| **Q1** | **Idle 与 ≥1000 帧冲突**：闲置时合成器应用应当不 Present（plan.md 要求“闲置 0 计时器”）。如果按本进程统计 PresentMon 帧，Idle 场景采不到 1000 帧 | 按本进程统计，不足 1000 帧判 INVALID | **阻断**：闲置开销做得越好，Idle 越可能 INVALID。需要另定 Idle 口径（例如按时长统计、统计 DWM 帧，或断言 0 Present） |
| **Q2** | **替代路径“帧间隔 P95 ≤ T”几乎无法满足**：垂直同步下帧间隔围绕 T 抖动，P95 必然略大于 T（单测中 ±5% 抖动即 FAIL） | 照字面执行，不加容差 | eventpipe 机器大概率 FAIL；需要容差或改用其他指标 |
| **Q3** | **对账在小计数时过严**：系统丢 1 帧、本进程计 0 帧，按“差 >2×”就是冲突 | 照字面执行；`reconcileNote` 非空时降为说明 | 低丢帧会话容易被判 INVALID；建议定一个下限，例如两边都 ≤2 时视为同量级 |
| **Q4** | **PresentStats 目前是占位值**：`M202Probe.cs` 写死 `EmitPresentStats(0, 16.666)` | 照常对账 | 在 120Hz 机器上 intervalMs 正好差 2.0 倍，处在临界；有任何真实丢帧就会冲突。采数前需要接入真实统计 |
| **Q5** | P95 的算法 | 最近秩，不插值 | 帧数 ≥1000 时与插值的差别很小；需要写死 |
| **Q6** | UI、渲染、GPU 三个阶段 P95 的来源：由 WPA 导出，但用哪些事件或区间界定各阶段没有定义 | 从 manifest 读人工填写的数值 | 需要指定 WPA profile 或导出口径，否则各机不可比 |
| **Q7** | **相对基线目前不存在**：首次运行时没有“同机同场景基线版本” | 缺基线判 INVALID | 首轮 PERF-CI 必然 INVALID；需要定义基线怎样产生（例如单独一轮只建基线、不判分） |
| **Q8** | machineId 用什么来源 | 只检查非空且互异 | 建议写死为 SMBIOS UUID（`wmic csproduct get uuid` / `Get-CimInstance Win32_ComputerSystemProduct`） |

## G3 禁令
不改 composition.h；不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；本票 ≠ 授权采数 ≠ PERF-CI PASS；PERF-CI gate `5b007dd4…282a` 不改；八 hash 不翻盘；禁立刻 Δv″。

## G4 ONELINE 模板
M2-PERF-CI-TOOLING | gate=<hash> @ <LOCKED_AT> | approved=<who>@<when> | mode=offline-tooling | tests=<n>/<n> | Q1–Q8=<裁决摘要> | ticket=<PASS/FAIL> | ≠PERF-CI PASS；不授权采数；不改composition.h
