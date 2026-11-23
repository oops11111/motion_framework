# M2-PERF-CI 门槛闸门 r2（m2_perf_ci_gate_r2_DRAFT.md）

STATUS=DRAFT（**NOT LOCKED**；本文件**不授权采数**；不解除 PERF-CI HOLD；不改 v1 LOCKED 闸门）
LOCKED_AT: （LOCK 时填）
GATE_BODY_SHA256: （LOCK 时算；约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希）

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> 起草：2026-11-23。协调裁决：2026-11-23，基线 `aa581ca1`，R1–R8 全部 (b) 改建议，裁决文本见下文各条（替换文字为裁决原文）。**r2 尚不具备 LOCK 条件**：R5 的 deferred 数值未定、Q6 仍阻塞；本裁决不是 APPROVED，不授权采数，不构成性能 PASS 或采用决定；PERF-CI HOLD 保持。
> PERF-CI gate `5b007dd4…282a` **不改、不翻盘**，仍 HOLD（等第二台物理机 + r2 LOCK + APPROVED）。八 hash 和 ABS-RESOLVE `e96bf623…5ade` 也不翻盘。

## G0 硬边界
- mode=**offline-decision**。采数=**否**；真机=**否**；落地=**否**。本票 PASS 仅表示口径已定，**≠ PERF-CI PASS ≠ 授权采数 ≠ 采用**。
- 不改 v1 LOCKED 闸门及其哈希；不改 `composition.h`（`a55d4952…e418`）。不重开 S1、R-3、R-2。不解冻 Bv。禁影子 IV。
- 必须 ≥2 台物理机；1 机 / VM / 同机复跑冒充 = 整票 INVALID（沿用 v1 G2-LOCK-10）。
- 本票不改判分脚本与采集 harness；LOCK 后另票实施（见“实施票清单”）。

## 证据（Windows 验证报告，用户提供，基线均为 main 提交）

**证据表述校正（裁决）**：“demo 本身不 Present／永远采不到”应限定为“本次指定构建、参数和采集条件下，未观察到 Demo 名称或 PID 对应的 PresentMon 行”。E1/E2 不足以证明永久性质，也未证明 DWM 行由 Demo 引起。本文件以下各条均按此限定表述理解。

1. **指定条件下按 demo 过滤采不到 CSV**：0018/0019 harness（`tools/perf_ci/capture_run.ps1`）按 `--process_name MotionSoftDemo.exe` 采集，PresentMon 2.6.0（SHA256 `b2a706bc…188f1af`）正常启动（stdout 仅 `Started recording.`）、demo 正常退出、logman ETL 与 demo 日志齐全，但 CSV 始终不存在（非零字节、非仅表头）。加 `--terminate_on_proc_exit`、等 120 秒、正规 `--terminate_existing_session` 停止，均不产生 CSV（REPORT-ba0a5447.md，基线 34bc45e3）。
2. **不过滤 / 只滤 dwm.exe 能产 CSV**：E1（不传 `--process_name`）产出 CSV，12728 数据行，全采集分组 dwm.exe 4494 行、MotionSoftDemo.exe 按名及实测 PID 均为 0 行；E2（只滤 dwm.exe）产出 CSV，2082 行全为 dwm.exe（REPORT-e0e67a5f.md，基线 34bc45e3）。
3. **归属未确立**：CSV 时间基为相对列（`TimeInMs` / `MsBetweenSimulationStart`），未与 UTC 建立严格映射；启动前基线窗口仅约 0.13 秒；故不能由全采集行数差断言 demo 造成 dwm Present，窗口内/基线分组与 FrameTime 分布为 N/A（不是 0）。
4. **表头为 v1 指标**：PM 2.6.0 默认输出含 `MsBetweenPresents`，不含 `FrameTime`。判分脚本两种列均已支持（`FRAME_COLUMNS`），无兼容问题；r2 须把两种列写死（R6）。
5. **Q1–Q8 裁决已定**（2026-10-28，`m2_perf_ci_tooling_gate_DRAFT.md`）：Q1 Idle 按时长判定、断言 0 Present，1000 帧要求不适用；Q2 替代路径帧间隔 P95 ≤ 1.1·T；Q3 两边丢帧数都 ≤2 视为同量级；Q4 采数前实现真实 PresentStats 统计（已由 0009 落地：M202/Z4 会话汇总 + 0018/0019 harness）；Q5 P95 最近秩；Q6 WPA 阶段导出口径待懂 WPA 的人定义（**仍未定**）；Q7 先跑一轮只建基线、不判分；Q8 machineId 取 SMBIOS UUID。Q1/Q2/Q3/Q7 并入 r2（Q1 按 R5 修订）；Q6 仍为采数前提，不在本票代定。

## 裁决项 R1–R8（2026-11-23 协调裁决，均为 (b) 改建议）

### R1 采集范围

**裁决：(b) 改建议。** 三个路线中明确选择 (b) 不过滤、事后取 dwm.exe 行。

理由：只过滤 DWM 无法同时核验测量窗口内其它进程的 Present；不过滤能保留 R3 所需的同窗证据。所得指标限定为受控桌面条件下的系统 DWM 表现，不宣称应用专属因果归属。

PresentMon 主采集不设置进程过滤，保留完整原始 CSV。判分时依据 R2 的实测身份及 R6 的序列规则选择目标 dwm.exe 行；其它进程行保留用于 R3 污染核验，不进入性能指标计算。不得以全文件计数替代锚定窗口计数，不得将 DWM 指标表述为已经证明由 Demo 单独产生的 Present。此次不选择直接 Present 探针路线，不变更既有场景集。

遗留开放数：无。

### R2 PID、字段与兼容

**裁决：(b) 改建议，保留字段名，限制回落范围。**

理由：r2 缺少 presentmonPid 时回落 Demo PID，会重新引入已观察到的空集问题。历史可读性不能成为绕过 r2 身份要求的通道。

`pid` 固定表示场景进程 Demo 的 PID；`presentmonPid` 固定表示本轮实测、所属交互会话已核验的目标 DWM PID，不表示 PresentMon 采集器 PID。`presentmonFilter` 在本版固定为字符串 `"none"`，表示实际命令未传进程过滤参数；实际完整参数另行留存核对。

run.json 与 manifest fragment 必须一致记录这些字段及 Demo、DWM 的进程身份证据。测量窗口内目标 DWM 重启、PID 身份改变或不能唯一确定时，该轮 INVALID。

r2 缺少或不能核验 `presentmonPid`、`presentmonFilter` 时直接 INVALID，禁止回落 `pid`。旧 manifest 保持可读取；仅在明确识别为历史 v1 规则的兼容路径中，允许沿用缺字段时回落 `pid` 的行为，并标记历史规则，不能据此产生 r2 PASS。闸门版本依据 gate 引用与哈希识别，不依据字段缺失猜测。

遗留开放数：无。

### R3 安静桌面核验

**裁决：(b) 改建议，要求整个测量窗口的证据。**

理由：前后短采不能证明中间窗口无污染；最小化也不能证明进程没有 Present。

采集操作者负责布置安静桌面：Demo 窗口前台可见，其它普通应用窗口最小化，此要求为强制准备条件，但不是充分证明。

harness 从 R1 同一次、覆盖完整测量窗口的未过滤 CSV 生成按 Application／PID 分组的清单，并关联进程身份。必须附原始 CSV、时间映射、窗口范围、全窗口分组清单和前台／最小化操作记录；采前、采后清单仅作辅助。

测量窗口内，除已核验的目标 DWM 与 Demo 外，任何进程出现 Present 行，该轮 INVALID。身份未知、清单缺失、采集覆盖或事件完整性不能核验，同样 INVALID。不能以“窗口已最小化”或前后短采为空替代全窗口证据。

离线判分脚本执行上述有效性检查，主代理复核证据；不得用人工说明豁免已发生的污染。满足本条仅表示符合约定的污染控制条件，不等于证明 DWM 事件全部由 Demo 引起。

遗留开放数：无；其它进程允许的 Present 行数明确为 0。

### R4 时间锚定

**裁决：(b) 改建议，固定字段并补齐映射契约。**

理由：四个进程时刻只定义外部时间边界，不能单独确定 CSV 零点。映射误差必须参与窗口过滤。

run.json 必须记录以下四项 ISO 8601 UTC 字段，并同步至 manifest fragment：`presentmonStartUtc`、`demoStartUtc`、`demoExitUtc`、`presentmonStopUtc`。分别取实际进程 StartTime／ExitTime；请求时刻、观察时刻另记，不能代替。

CSV 时间基说明固定存放于 run.json 的 `csvTimeBase` 对象，包含：`column`、`unit`、`kind`、`rowTimestampMeaning`、`originValue`、`originUtc`、`secondsPerUnit`、`maxErrorMs`、`mappingEvidence`。`kind` 为 relative、qpc 或 utc；必须明确原值如何转换为数值，以及映射公式：`UTC = originUtc + (value − originValue) × secondsPerUnit`。`mappingEvidence` 指向可复读的原始锚定证据及工具版本对应的时间语义依据；`maxErrorMs` 必须有依据，不能任填 0。不得把 PM 进程启动直接当作 CSV 零点，也不得把间隔列误当时间戳。

另记录 `measurementWindow` 与 `baselineWindow` 的起止 UTC、用途及实际时长，窗口统一为左闭右开。只有计入映射误差后仍能确定整个帧间隔位于窗口内的行，才可计为该窗口有效帧；边界不确定行排除并报告数量。映射不可建立或无法复读时，该轮 INVALID。

遗留开放数：无固定误差阈值待填；实际映射误差必须逐轮提供证据，不能凭假设补值。

### R5 Idle 与 Q1 的取代关系

**裁决：(b) 改建议；采用应用活动证据，数值缺口明确 deferred。**

理由：DWM 的系统侧活动不能套用应用侧“0 Present”断言；零动画回调也不能单独证明 CPU/GPU 开销接近零。现有证据不足以确定 DWM 超限倍率。

本条仅修订 Q1 中以“应用 0 次 Present”作为 Idle 主验收依据的表述：改为在完整 Idle 测量窗口内，应用动画计时器无活动、动画相关 render 回调增量为 0，并提供能直接证明这些事实的记录。现有 dropped／intervalMs 汇总不能替代上述计数证据。

Idle 按时长验收，持续时间本次定为 **60 秒**；这是采用 Q1 所举时长形成统一协议，不是声称实测证明 60 秒足以消除全部噪声。保留 Q1 的“≥1000 帧要求不适用于 Idle”及“闲置 CPU/GPU 接近零”要求。

DWM 辅助比较采用同机、同显示配置、相同核验方法的 60 秒安静桌面基线，比较锚定窗口内的有效行率，不能比较不同长度窗口的原始行数。DWM 超限倍率、CPU/GPU“接近零”的量化算法及容差尚未确定，均为 deferred；不得用 1.0、1.1 等无依据数字补齐。

这些数值须在取得另行授权的、覆盖至少两台物理机的可复读基线与噪声材料后再次裁决，并在 r2 LOCK 前写死。本裁决不授权该诊断或采集；未定前不得宣告 Idle 或完整 r2 闸门已收口。

遗留开放数（**均阻止 r2 LOCK**）：
- DWM 超限倍率：deferred。
- CPU/GPU 近零判定算法与容差：deferred。
- 60 秒为本次新定数，不冒称历史 Q1 已写死。

### R6 指标列与有效帧

**裁决：(b) 改建议，接受两列并补齐有效行定义。**

理由：现有脚本已经接受两种列；缺 FrameTime 本身不应再作为 schema 不合格。DWM 的不同序列不能简单拼行凑满 1000。

接受毫秒单位的 `MsBetweenPresents` 与 `FrameTime` 两种时长列，保留原始表头并记录实际选择列。未显式选择时沿用现实现顺序：优先 `MsBetweenPresents`，否则 `FrameTime`；两者均不存在时 INVALID。当前轮与对应基线须使用相同指标定义。

有效帧为同时满足以下条件的原始行：Application 与目标 DWM 身份相符、ProcessID 等于 `presentmonPid`、属于预先声明并有证据对应目标显示器的同一 Present 序列、时长可解析且有限并大于 0、整个间隔按 R4 确定处于测量窗口内。禁止跨显示器／交换链拼行或重复计入同一原始行；序列选择无法核验时 INVALID。

非 Idle 场景每轮要求上述有效行 ≥1000，不足为 INVALID；Idle 沿 R5 豁免。丢帧仍定义为时长 >1.5×T，每千有效帧丢帧数仍须严格 <1.0，不得改变边界或用窗外行补数。

遗留开放数：无。显示器／序列对应证据是逐轮有效性要求，缺失时不能判 PASS。

### R7 对账与冲突处理

**裁决：(b) 改建议，保留既有规则，禁止仅凭备注消除冲突。**

理由：R3 清单是必要证据，但不自动证明对账可比；当前脚本将非空 `reconcileNote` 直接降为说明，达不到“报告可解”的要求。

PresentMon 仍为主判据，PresentStats 仅作存在性和会话汇总对账；两侧必须对应同一场景、轮次和可核验的统计窗口。

保留本进程计数 ≤ 系统计数的要求及 >2× 的量级冲突规则。Q3 原样并入：两侧丢帧数均 ≤2 时，免除丢帧倍数／量级冲突；该豁免不取消独立的计数包含关系检查，也不豁免 intervalMs 冲突。

冲突必须附 R3 全窗口清单、非空 `reconcileNote`、可复算的对账证据及复核结论。无清单的冲突一律 INVALID；有清单但污染、口径不可比或解释不能解决冲突，同样 INVALID。任意非空文本不得自动解除冲突，更不能覆盖其它 FAIL／INVALID。

遗留开放数：无；≤2 与 >2× 沿既有裁决。

### R8 Q6 与其余既有裁决

**裁决：(b) 改建议。** 不接受“Q6 未定则首轮只能走 EventPipe 并注明”作为通用许可。

理由：该句同时绕过 Q6 的既有采数前提和 G2-LOCK-12 的降级资格。Q6 未定不是 logman Access denied。

Q6 仍未定，继续保留既有“由懂 WPA 的人员定义阶段 P95 导出口径，作为采数前提”的要求；本裁决不代定、不豁免。删除“Q6 未定则首轮只能走 EventPipe”这一自动降级表述。

G2-LOCK-12 原样沿用：有管理员权限必须使用 logman ETL；仅目标机无管理员权限且 logman 实测 Access denied、拒绝证据留档时，才具备 EventPipe 降级资格。满足该资格不自动豁免 Q6 或其它前提。

Q7 原样并入：全部前提满足后，首轮只建基线、不判分；不得将基线轮标成性能 PASS。Q2 原样并入合法替代路径的帧间隔 P95 ≤1.1·T；Q5 原样采用最近秩；Q8 原样采用 SMBIOS UUID。

G2-LOCK-10 机器矩阵、至少两台物理机及至少两个 GPU 档要求不变；最低产物仍为 CSV、ETL 或符合降级条件的 nettrace、PresentStats 汇总。Q6 未解决前保持相应阻塞，不以备注绕过。

遗留开放数：Q6 的事件、阶段边界及导出算法 deferred，待 WPA 专项口径完成后确认；本项不另定性能数字。

## G3 禁令
不改 v1 LOCKED 闸门与哈希；不改 composition.h；不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；本票 ≠ 授权采数 ≠ PERF-CI PASS；PERF-CI HOLD 不解（等第二台物理机 + R5 数值补齐 + Q6 解决 + r2 LOCK + APPROVED 后方可派 workbuddy）。

## 实施票清单（以下票均须在相应裁决补齐、r2 LOCK 后另开，本次不执行）
1. 判分脚本票：实现 r2 版本隔离、Present 身份与序列选择、窗口过滤、R3 有效性检查、Idle 分支及不能由备注自动消除的对账冲突，并补合成数据测试。
2. harness 票：实现不过滤采集、实测 DWM 身份、四项 UTC 与 CSV 时间映射、同窗进程清单、可核验的正常停止和文件锁处理。
3. Idle 证据票：补齐动画计时器、render 回调及已裁定 CPU/GPU 指标的可复读采集证据，不以 dropped／intervalMs 冒充。
4. 提示词与 README 票：同步字段、60 秒 Idle、安静桌面全窗核验、基线轮不判分及 EventPipe 的严格资格。

LOCK 前仍须另行处理 R5 的 deferred 数值，并保留 Q6 阻塞；本清单不授权为此采数。

## G4 ONELINE（裁决原文）
M2-PERF-CI-R2 | gate=UNLOCKED @ NOT_SET | approved=NONE | mode=offline-decision | R1=不过滤后取DWM；R2=r2严格身份/历史兼容隔离；R3=全窗清单；R4=四UTC+有据映射；R5=Idle60s/阈值deferred；R6=双列+同序列窗内有效行；R7=有据解冲突；R8=Q6不豁免/禁止自动降级 | ticket=FAIL（口径尚未收口，非性能FAIL） | PERF-CI HOLD；≠PERF-CI PASS；不授权采数；不改composition.h；不改v1闸门；不重开S1/R-3/R-2；不解冻Bv；禁影子IV；≥2物理机；成功≠采用
