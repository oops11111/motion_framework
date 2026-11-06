# M3-MATRIX 门槛闸门（m3_matrix_gate_LOCKED.md）

STATUS=LOCKED
LOCKED_AT: 2026-11-02 09:34:33 AWST（Australia/Perth）
GATE_BODY_SHA256: d8c21d0304931c42d805ceef35fccf4df50b5f2931fd916b81a13eea5db1963f
> GATE_BODY_SHA256 计算约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。本行写入后不得再改 body。

**本 LOCK 不授权采数。** 须另有 approved 留证（APPROVED_* 非空）后才可采数；构建可先行。

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> 起草：Claude，2026-11-02。DRAFT：`docs/m3_matrix_gate_DRAFT.md`。依据：`docs/m3_plan_DRAFT.md` WP3 与 §7 第 4 张票；plan.md §6 的多屏与 DPI 矩阵；M3 验收门槛“通过混合刷新率测试矩阵”。**pidan 授权本票的开放问题由 Claude 决定（T4，2026-11-02）**，下面 X1–X7 均为 Claude 的决定，记录理由。
> 前置：WP1（DPI、设备丢失恢复）和 WP2（跟随）已有结论。分支：`claude/m3-matrix`，从 `claude/m3-follow` 拉出。结果目录 `m3/results_matrix/`。

## G0 硬边界
不改 `composition.h`；不重开 S1/R-3/R-2；禁影子 IV；不解冻 Bv；PERF-CI 仍 HOLD；未 LOCK 且 APPROVED_* 为空时禁止采数；成功 ≠ 采用 ≠ 默认可合入 main；本机 csproj 补丁不进仓库。**不注入指针、不重启驱动、不需要管理员。**

## G1 要验证什么
Rive 动效在一台接两块显示器、刷新率和缩放都不同的机器上：
1. 窗口在每块显示器上时，Rive 渲染线程的节奏跟随**该显示器**的刷新率（不是一直按主显示器），不掉帧；
2. 窗口在动画进行中跨屏（拖动和直接跳转）时，没有可见的卡顿；
3. 跨到缩放不同的显示器时，Rive 表面按新缩放重新光栅化（复用 M3-RIVE-DPI 的路径），尺寸正确、画面清晰；
4. 每块显示器上预算仍满足 plan.md（渲染线程、GPU ≤ 0.35·T，T 为该显示器的帧周期）。

## G2 判据（每个会话）
| 判据 | 门槛 |
|---|---|
| ① 节奏 | 每个显示器段：窗口确实在预期显示器上；渲染循环间隔 P50 在该显示器帧周期的 ±10% 以内；间隔 > 1.5·T 的比例 ≤ 1% |
| ② 稳定 | 每段 `draw_failures=0`，帧数 ≥ 300 |
| ③ 跨屏卡顿 | 每次跨屏，从开始移动到移动结束后 1 s，渲染循环最长间隔 ≤ 100 ms |
| ④ 预算 | 每段 CPU P95 ≤ 0.35·T、GPU P95 ≤ 0.35·T、GPU 丢弃样本 ≤ 10% |
| ⑤ 尺寸 | 每段 Rive 表面像素尺寸 = round(400 × 当前缩放) |
| ⑥ 清晰度 | 每段截图中 Rive 画面清晰（Claude 复核目视） |
| ⑦ 合成器帧统计 | DComp 帧数和帧周期（**只记录**，用于判断合成器在两块屏上的节奏） |

- **会话**：≥ 3 个独立进程；每会话 5 个显示器段（主 → 副 → 主 → 副 → 主）和 4 次跨屏（拖动、跳转交替）。
- **PASS** ⇔ ①–⑥ 每个会话都满足；⑦只记录。
- **FAIL**：任一条不满足（例如 ① 不满足说明渲染线程没有跟随所在显示器的刷新率——这本身就是本票要找的结论）。
- **BLOCKED_HARDWARE**：机器上不是恰好两块显示器（X1）。
- **INVALID**：缺产物；hash 不符；未授权采数；改 composition.h；两块显示器刷新率和缩放都相同（不构成“混合”）。

## G3 决定（X1–X7，Claude 代定，pidan 授权）
| # | 问题 | 决定 | 理由 |
|---|---|---|---|
| **X1** | 用什么硬件 | **DESKTOP-IJI1KJG（75 Hz，主显示器）+ 一块外接显示器（60 Hz 即可）**，恰好两块。执行方没有第二块显示器时记 `BLOCKED_HARDWARE`，不采数 | IJI1KJG 是 Win11、构建环境现成、已有全部 M3 数据的机器；60 Hz 显示器最容易找到，60 + 75 就是 plan 要求的第一种混合 |
| **X2** | 144 Hz | **本票不要求**；执行方若有 ≥ 120 Hz 的显示器，就用它代替 60 Hz 那块，并在回报里写明；否则记为遗留项 | 不让高刷屏卡住本票；60 + 75 已能检验“是否跟随所在显示器” |
| **X3** | 缩放混合 | **两块显示器设成不同缩放**：主显示器 100%（1280×1024 只能 100% 或 125%），外接显示器 125%（或 150%，若分辨率 ≥ 1920×1080 优先 150%）。200% 需要 4K 显示器，记为遗留项 | 能检验跨屏时的 DPI 变化；不要求买新设备 |
| **X4** | 怎样“跨屏拖动” | **探针自己移动窗口**：拖动 = 每 16 ms 移动 40 px（约 2.5 px/ms，较快的拖动）；跳转 = 一次移到另一块屏。不用 `SendInput`，不要求操作员动手 | 可重复、无需注入授权；与真人拖动的差别是 Windows 的模态移动循环（拖动时 Windows 自己处理消息），记为局限 |
| **X5** | 测什么 | Rive 渲染线程的循环间隔（原始样本，新增 `m0r_get_intervals`）、预算、表面尺寸和重新光栅化、跨屏期间最长间隔、每段截图（`PrintWindow`）、DComp 帧统计（只记录）。**不测**合成器动画在两块屏上的逐帧时间（需要两块屏各自的捕获，留给后续） | 本框架自己能控制的是 Rive 渲染线程；合成器动画由 DWM 驱动 |
| **X6** | plan §6 其余项（电池模式动态刷新率、RDP、GPU 切换、热插拔显示器） | **本票不做**，各自留作后续票（DRR 需要支持 Win11 动态刷新率的屏；RDP 和 GPU 切换需要不同的环境；热插拔需要人工插拔并另定安全规则） | 一张票只回答一类问题；先把最常见的双屏混合做实 |
| **X7** | 判据门槛 | 如 G2：节奏 ±10%、超长间隔 ≤ 1%、跨屏 ≤ 100 ms、预算 0.35·T、尺寸 0 px 误差 | ±10% 能区分 60 Hz（16.7 ms）和 75 Hz（13.3 ms）；100 ms 约为人能察觉的停顿（6 帧 @ 60 Hz）；其余沿用 DPI 票 |

## G4 实现（已写，未在真机运行；代码 = 分支 `claude/m3-matrix` 提交 `fe81db8`）
| 文件 | 改动 | 本地验证 |
|---|---|---|
| `m0/native/m0rive/m0rive.{h,cpp}` | 新增 `m0r_get_intervals`：复制自上次 `m0r_reset_stats` 以来的渲染循环间隔原始样本 | MinGW 对照钉住的 Rive 头文件：0 警告 0 错误 |
| `MotionSoftDemo/M3MatrixProbe.cs`（新，`--m3-matrix-probe`） | 枚举显示器（设备名、分辨率、刷新率 `EnumDisplaySettings`、缩放 `GetDpiForMonitor`）；不是恰好两块就 `BLOCKED_HARDWARE`；Rive 表面 400 DIP，`XamlRoot.Changed` 时 `m0r_resize`（同 DPI 票）；窗口放在主显示器 → 段 0 → 拖到副显示器 → 段 1 → 跳回主 → 段 2 → 拖到副 → 段 3 → 跳回主 → 段 4；每段 8 s，记录所在显示器、间隔 P50 / 最大 / 超长比例、帧数、失败、CPU / GPU、表面尺寸、DComp 帧统计、截图；每次跨屏记录移动耗时、最长间隔、缩放变化时刻、重新光栅化耗时。summary 汇总 `pacing_ok / budget_ok / stability_ok / size_ok / hitch_ok` | 对照 WinAppSDK 2.5.1 编译 0 个 C# 错误 |
| `MotionSoftDemo/MainPage.xaml.cs` +12 行；`m3/mainpage_m3matrix_hook.patch` | 入口。补丁基于已打 DPI、LOST、XAML-LOST、XAML-REBUILD、FOLLOW 补丁的 MainPage | 补丁由 `git diff` 生成 |
| `m3/run_m3_matrix.ps1`（新） | 按会话启动、响应截图请求（`PrintWindow`，每显示器 DPI 感知）、输出会话矩阵；遇 `BLOCKED_HARDWARE` 即停 | 本环境没有 PowerShell，人工复查 |

## G5 禁令
不改 composition.h；不重开 S1/R-3/R-2；禁影子 IV；不注入指针；不重启驱动；本 DRAFT ≠ 授权采数；成功 ≠ 采用；PERF-CI 仍 HOLD。
