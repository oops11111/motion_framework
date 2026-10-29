# M3-RIVE-LOST 门槛闸门（m3_rive_lost_gate_DRAFT.md）

STATUS=DRAFT（**NOT LOCKED**；本文件**不授权采数**）
LOCKED_AT: （LOCK 时填）
GATE_BODY_SHA256: （LOCK 时算；约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希）

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> 起草：Claude（Claude Code 接手会话），2026-10-29。依据：`docs/m3_plan_DRAFT.md` §7 第 2 张票（WP1，plan 任务 8）；M3 验收门槛第 1 条“Rive 表面支持……设备丢失恢复”；`docs/notion_m3_rive_dpi_next.md`（主推 M3-RIVE-LOST）。
> 分支：`claude/m3-rive-lost`，从 `claude/m3-rive-dpi`（`2bb1f14`）拉出。原因：设备丢失后的重建要复用 DPI 票的重建路径（`ApplyResize` 中重建离屏纹理、Rive 渲染目标和标记 RTV）。DPI 代码进 main 走 PR #6 评审，与本票流程分开。

## G0 硬边界
- 不改 `composition.h`；不重开 S1/R-3/R-2；禁影子 IV；不解冻 Bv；PERF-CI 仍 HOLD。
- 未 LOCK 且 APPROVED_* 为空时禁止采数。成功 ≠ 采用 ≠ 默认可合入 main。
- **不改系统的 TDR 注册表项**（`TdrDelay`、`TdrLimitCount` 等），也不改显卡驱动设置。
- DPI 票中执行方本机的 csproj 补丁（`WindowsPackageType=None` + `WindowsAppSDKSelfContained=true`）：派工单会**单独**写明这是本机启动的前置条件，**不进仓库，不回灌主线**。

## G1 要验证什么
Rive 渲染所用的 D3D11 设备被移除后（真实 GPU 超时恢复，即 TDR）：
1. 渲染线程自己发现设备已丢失（`DXGI_ERROR_DEVICE_REMOVED` / `DEVICE_RESET`，来自 `BeginDraw`、Rive flush 或每帧检查的 `GetDeviceRemovedReason`）；
2. 不重启进程，自动完成恢复：新建 D3D11 设备（特性级别 11_1）→ 新建 Rive 渲染上下文 → 从内存中的 .riv 字节重新导入 → 重建离屏纹理和渲染目标 → 通过 `ICompositionGraphicsDeviceInterop::SetRenderingDevice` 把新设备交给原来的 `CompositionGraphicsDevice`；原有合成表面和 visual 保持不变，只是重画内容；
3. 恢复后画面正确、渲染预算仍满足 plan.md（渲染线程 ≤0.35·T、GPU ≤0.35·T）；
4. 多次丢失后不泄漏（旧设备和旧 Rive 上下文确实释放了）。

## G2 判据（每个会话；数字为建议值，见 K2）
| 判据 | 门槛 |
|---|---|
| ① 注入生效 | 每个会话 K4 规定次数的注入，每次都被渲染线程检测到（`lost_detected` 次数 = 注入次数，带 removed reason HRESULT） |
| ② 自动恢复 | 每次丢失都恢复成功（`recover_ok`），进程不重启；单次恢复耗时（检测到丢失 → 新设备上第一帧画完）**≤ 5000 ms**，超过判 FAIL；P95 **记录**，参考目标 ≤ 1000 ms |
| ③ 恢复后稳定 | 每次恢复后的段：`draw_failures=0`（丢失期间的失败单独计数，不算在内）；`device_removed_hr=0`；帧数 ≥ 300 |
| ④ 恢复后预算 | 每次恢复后的段：CPU P95 ≤ 0.35·T，GPU P95 ≤ 0.35·T，GPU 丢弃样本 ≤ 10% |
| ⑤ 画面正确 | 每次恢复后的窗口截图不是空白，且与新设备上导出的参考图目视一致（协调目视确认，写入结题） |
| ⑥ 丢帧窗口 | 从检测到丢失到恢复后第一帧之间缺失的帧数（**记录**） |
| ⑦ 内存 | 最后一段末尾的进程专用内存，比第一次注入前的段末尾多出 ≤ 50 MB（**记录**；超出列为 M3 待查项，不判 FAIL）；同时记录旧设备释放时的引用计数（应为 0） |

- **会话**：≥3 个独立进程（K4）。
- **PASS** ⇔ ①②③④⑤ 每个会话都满足；⑥⑦只记录。
- **INVALID**：缺产物；hash 不符；未授权采数；改 composition.h；注入命令执行了但渲染线程没检测到丢失（这一次注入作废，见 K1）；系统蓝屏或显示器 30 s 内没恢复（`INVALID_SYSTEM`，当天停止，见 K5）。

## G3 待裁决（K1–K6，附建议）
| # | 问题 | 建议 | 理由 |
|---|---|---|---|
| **K1** | 怎样让设备丢失 | **以真实 TDR 为准**：管理员运行 `dxcap -forcetdr`（Windows“图形工具”可选功能自带）。另外在探针里留一个**模拟丢失**开关（`--m3-lost-sim`：渲染线程把当前设备当作已丢失，走完整的恢复路径），只用于本地冒烟和操作员熟悉流程，**不计入判据** | D3D11 没有像 D3D12 `RemoveDevice` 那样的进程内接口；只有真实 TDR 能同时测到驱动、DWM 和合成器三方的行为。模拟开关便宜、可重复，能在采数前把代码路径先跑一遍 |
| **K2** | 门槛取值 | 如 G2：恢复成功是硬判据；单次恢复上限 5 s，P95 只记录（参考目标 1 s）；恢复后预算沿用 DPI 票的 0.35·T；内存 50 MB 只记录 | plan 写的是“恢复时间和内存增长在记录内”，所以只给一个防止挂死的上限。TDR 本身让整个显示停顿 1–3 s，用户看到的停顿主要由系统决定；内存上限比 DPI 票（20 MB）宽，因为要重建整个设备和 Rive 上下文 |
| **K3** | 用哪台机器 | **DESKTOP-IJI1KJG**（Win11、可提权、RX 7800 XT，M0-B 和 DPI 票的那台）。采数前确认 `dxcap -?` 能运行；没有就在“设置 → 可选功能”装“图形工具” | 唯一能提权的机器；构建和运行环境现成 |
| **K4** | 注入次数和间隔 | **3 个会话 × 每会话 2 次注入**；同一会话两次注入间隔 ≥ 45 s；会话之间冷却 ≥ 120 s；全天合计 ≤ 8 次 | Windows 默认 60 s 内 TDR 超过 5 次会直接蓝屏（`TdrLimitCount`/`TdrLimitTime`），间隔 45 s 保证 60 s 窗口内最多 2 次；每会话 2 次能测到“恢复后再丢一次”的情形（旧对象没清干净时常在第二次暴露） |
| **K5** | 整机风险怎么控制 | 采数前关掉其他用 GPU 的程序，保存手头工作；runner 每次注入前检查距离上次注入 ≥ 45 s，否则拒绝执行；出现蓝屏、显示器 30 s 内没恢复、或驱动报错弹窗，立即停止，记 `INVALID_SYSTEM`，当天不重试，回报事件查看器“系统”日志里的 4101（显示驱动已停止响应并已恢复）和 BugCheck 记录 | TDR 会重置整块 GPU，影响机器上所有程序；由 runner 控制间隔，比依赖操作员手速可靠 |
| **K6** | 恢复后 Rive 动画从哪里接着播 | **允许从头播放**（新状态机从 t=0 开始），记录 `resume_mode=restart`；“恢复到丢失前的播放位置”记为 WP1 产品化待办，不在本票判据内 | 新导入的状态机没有可靠的“跳到某时刻”接口，用一次大步长 `advance` 补时间，不同 .riv 的结果不一定一致；本票验证的是“能恢复并画对”，播放连续性留给产品层 |

## G4 实现计划（K 裁决后写，LOCK 前完成并本地验证）
| 文件 | 内容 |
|---|---|
| `m0/native/m0rive/m0rive.{h,cpp}` | 新增 `m0r_attach_graphics_device(IUnknown*)`（QI `ICompositionGraphicsDeviceInterop` 4AFA8030-BC70-4B0C-B1C7-6E69F933DC83）；渲染循环每帧检查设备状态，发现丢失后在渲染线程内执行 `RecoverDevice()`：释放查询、离屏纹理、渲染目标、Rive 上下文和旧设备 → 新建设备 → `SetRenderingDevice` → 重新导入 .riv → 复用 DPI 的重建路径 → 记录耗时。新增 `m0r_get_lost(...)`（次数、每次的 removed reason、恢复耗时、丢失期间失败数、缺失帧数、旧设备释放时的引用计数）和 `m0r_simulate_lost()`（K1 的模拟开关）。交换链路径返回 E_NOTIMPL（F1） |
| `MotionSoftDemo/M3LostProbe.cs`（`--m3-lost-probe`，可加 `--m3-lost-sim`） | 把 C# 侧创建的 `CompositionGraphicsDevice` 交给 DLL；按段统计（注入前一段 + 每次恢复后一段）；每段请 runner 截图并导出参考图；写 summary |
| `MotionSoftDemo/MainPage.xaml.cs` 入口 + `m3/mainpage_m3lost_hook.patch` | 与 DPI 票同样的接法（CRLF 保持不变） |
| `m3/run_m3_lost.ps1` | 需要管理员；检查 `dxcap` 可用；按 K4 的次数和间隔执行 `dxcap -forcetdr`，并强制最小间隔（K5）；每次注入后等探针报告恢复，或等到 5 s 上限；截图；读系统日志里的 4101 事件作为注入证据；输出会话矩阵 |

本地验证：MinGW 对照钉住的 Rive 头文件做语法检查；C# 对照 WinAppSDK 2.5.1 编译 0 错误；PowerShell 在本环境不能运行，只做人工复查。真实恢复路径只能在真机上验证。

## G5 禁令
不改 composition.h；不重开 S1/R-3/R-2；禁影子 IV；不解冻 Bv；不改 TDR 注册表；本 DRAFT ≠ 授权采数；成功 ≠ 采用；PERF-CI 仍 HOLD。

## G6 ONELINE 模板
M3-RIVE-LOST | gate=<hash> @ <LOCKED_AT> | approved=<who>@<when> | host=<名> | inject=<dxcap-forcetdr> | sessions=<n> | lost_detected=<s1/s2/s3> | recover_ok=<s1/s2/s3> | recover_max_ms=<…> | budget_ok/stability_ok=<…> | sharp=<目视结论> | frames_missed=<…> | mem_delta_mb=<…> | ticket=<PASS/FAIL/INVALID> | 不改composition.h；成功≠采用
