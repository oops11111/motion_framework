# M2-PERF-CI 门槛闸门 r2（m2_perf_ci_gate_r2_DRAFT.md）

STATUS=DRAFT（**NOT LOCKED**；本文件**不授权采数**）
LOCKED_AT: （LOCK 时填）
GATE_BODY_SHA256: （LOCK 时算；约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希）

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> 起草：Claude（Claude Code 接手会话），2026-10-28。
> 取代对象：r1 `docs/m2_perf_ci_gate_LOCKED.md`（`5b007dd43ba351dc0985a52d1745485954d149575feeca622791275005cd282a`，2026-10-25 01:37:05 AWST）。r1 从未 APPROVED，也没有采过数，文件保留不改。
> 修订依据：pidan 对 Q1–Q8 的裁决「Q 按建议」（2026-10-28 20:31:11 AWST，记录在 `docs/m2_perf_ci_tooling_gate_DRAFT.md`）。下文只写与 r1 不同的地方，其余条款**沿用 r1 原文**。
> 判分脚本：`tools/perf_ci/perf_ci_grade.py`，manifest 写 `"gateVersion": "r2"`。r2 LOCK 后，把 hash 填进脚本里的 `RULES["r2"]["sha"]`；在那之前只接受 `--dry-run`。

## G0 硬边界（同 r1）
不重开 S1/R-3/R-2；不改 `composition.h`；成功 ≠ 采用；未 LOCK 且未 APPROVED 不得采数；**必须 ≥2 台物理机，且 GPU 档不同**。

## 相对 r1 的改动
| # | r1 条款 | r2 | 依据 |
|---|---|---|---|
| **Q1** | Idle 也要求每轮 PresentMon 有效帧 ≥1000 | Idle **按时长判定**：每轮 ≥ 60 秒；PASS ⇔ 本进程 Present 数 = 0，且进程 CPU 平均 ≤ 0.1%、GPU 引擎占用平均 ≤ 0.1%（plan.md“闲置 CPU/GPU 接近 0”，取 Gemini 的 ≤0.1%）。1000 帧要求、丢帧率、P95、回退、对账都不适用于 Idle | 闲置的合成器应用本来就不该 Present |
| **Q2** | 降级路径：帧间隔 P95 ≤ T | 帧间隔 P95 ≤ **1.1·T** | 垂直同步抖动使 P95 必然略大于 T |
| **Q3** | 对账：两边差 >2× 即冲突 | 两边丢帧数都 ≤ 2 时**视为同量级**，不计冲突 | 小计数时 2× 过严 |
| **Q4** | — | **采数前提**：`PresentStats` 必须是真实统计（`M202Probe.cs` 现在写死 `EmitPresentStats(0, 16.666)`），runner 记录其实现 commit | 占位值无法对账 |
| **Q5** | 未写死 | P95 = 最近秩，不插值 | 写死口径 |
| **Q6** | 阶段 P95 来自 WPA，但未定义口径 | **采数前提**：由懂 WPA 的人提交 UI、渲染、GPU 三个阶段的导出 profile（`.wpaProfile`）和说明，落盘到 `tools/perf_ci/wpa/`；各机器用同一份 | 否则各机不可比 |
| **Q7** | 相对基线 = 同机同场景基线版本（未说明怎样产生） | **先跑一轮只建基线的轮次**（manifest `"mode": "baseline"`，每机每个非 Idle 场景 ≥1 轮），由脚本生成 `perf_ci_baselines.json`（按 machineId，取各轮中位数），不判分；判分轮通过 `baselinesFile` 引用它 | 首轮没有基线可比 |
| **Q8** | machineId 为主机级稳定标识 | machineId **必须是 SMBIOS UUID**（`Get-CimInstance Win32_ComputerSystemProduct`.UUID） | 与 M0 两台机器一致 |

## G1 场景与目标句（r2 版）
1. **Idle**：启动后保持静止，不交互，采集 ≥ 60 秒；runner 同时记录进程 CPU 平均占用和 GPU 引擎平均占用（性能计数器 `\Process(*)\% Processor Time`，以及 `\GPU Engine(pid_<PID>*)\Utilization Percentage` 的总和）。
2. **Steady Play**、3. **Interrupted Retarget**：同 r1（每轮 PresentMon 有效帧 ≥1000，每机每场景 ≥2 个独立进程的判分轮）。
- P95 目标：同 r1（UI 12% / 渲染 35% / GPU 35%，相对基线回退 ≤10%）；降级路径改为 ≤1.1·T（Q2）。
- P99：同 r1（每千帧丢帧 <1.0，丢帧 = 帧间隔 >1.5·T）。

## G2 判据（r2 版；未列出的条款同 r1 的 G2-LOCK-9…14）
- **G2-r2-1 采数前提**：Q4、Q6 都完成并落盘，否则**不得 LOCK**。
- **G2-r2-2 轮次**：每机先完成 1 轮基线（Q7），再完成 ≥2 轮判分；基线轮和判分轮必须是不同进程，不得混用。
- **G2-r2-3 机器**：≥2 台物理机，machineId 为 SMBIOS UUID 且互异，GPU 档 ≥2 种（集显 / 独显 / 低端笔记本）。**现状**：DESKTOP-K93N3P8（GTX 970）和 DESKTOP-IJI1KJG（RX 7800 XT）都是独显，还缺一台集显或低端笔记本。
- **G2-r2-4 PASS/FAIL**：同 r1 的 G2-LOCK-13，其中 Idle 按 Q1 判，降级路径按 Q2 判，对账按 Q3 判。

## G3 需要裁决的事项（附建议）
| # | 问题 | 建议 |
|---|---|---|
| **H1** | Idle 的 CPU/GPU 门槛用 0.1% 是否合适 | **采用 0.1%**（plan.md 中 Gemini 的数字）。若实测发现 WinUI 宿主本身的底噪就超过它，再按“框架开启 vs 关闭”的差值另议 |
| **H2** | 第三台机器（集显或低端笔记本）由谁提供、何时到位 | 由 pidan 安排；到位前本票只能 LOCK，不能采数 |
| **H3** | Q4（真实 PresentStats）和 Q6（WPA profile）由谁做 | Q4 由 Claude 在 MotionSoftDemo 中实现（用 DXGI frame statistics 或 PresentMon 同源口径），Q6 由懂 WPA 的人做 |

## G4 禁令
同 r1 G3；另加：未完成 Q4/Q6 不得 LOCK；基线轮不得作为判分轮使用。

## G5 ONELINE 模板
M2-PERF-CI r2 | gate=<hash> @ <LOCKED_AT> | approved=<who>@<when> | machines=<n>（GPU 档=<…>） | baseline=<已建/未建> | ticket=<PASS/FAIL/INVALID> | idle presents/cpu/gpu=<…> | 成功≠采用；不改composition.h；≥2物理机；主判据PresentMon
