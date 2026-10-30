# M3-XAML-REBUILD 门槛闸门（m3_xaml_rebuild_gate_LOCKED.md）

STATUS=LOCKED
LOCKED_AT: 2026-10-30 17:35:36 AWST（Australia/Perth）
GATE_BODY_SHA256: 3ea74c371ae983cebe480e1b4dea99e61aec92cfbe9646fb87b91b7a1d456fdc
> GATE_BODY_SHA256 计算约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。本行写入后不得再改 body。

**本 LOCK 不授权采数。** 须另有 approved 留证（APPROVED_* 非空）后才可采数；构建和 `-DryRun` 走流程不采数，可先行。

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> 起草：Claude，2026-10-30。DRAFT：`docs/m3_xaml_rebuild_gate_DRAFT.md`。来源：M3-XAML-LOST 结果 `XAML-LOST`（`docs/m3_xaml_lost_result_claude_review.md`；pidan Q1–Q4 裁决：Q2 = 做一张短的后续诊断，找宿主侧的补救办法）。
> 已知：显卡驱动重启后，同一窗口里的文字、圆角元素、开关和按钮全部画不出来，等待、改大小、最小化还原、重设页面内容都无效；XAML 仍在出帧，纯色矩形正常；**重新启动应用后界面正常**（每个新会话的基线截图都完整）。
> 分支：`claude/m3-xaml-rebuild`，从 `claude/m3-xaml-lost` 拉出。诊断票，不判 PASS/FAIL。

## G0 硬边界
同 XAML-LOST：不改 `composition.h`；不重开 S1/R-3/R-2；禁影子 IV；不解冻 Bv；PERF-CI 仍 HOLD；不改 TDR 注册表和驱动设置；未 LOCK 且 APPROVED_* 为空时禁止采数；注入沿用 r2 的 `pnputil /restart-device`、自检、间隔、上限和停止规则。

## G1 要回答的问题
1. 丢失是**每个窗口**的，还是**整个进程**的？驱动重启**之前**就开着的第二个窗口，和重启**之后**新建的窗口，能不能画文字？
2. **打包部署**（MSIX，WinAppSDK 由系统框架包提供）是否也一样？目前的数据都来自执行方本机的 unpackaged + self-contained 设置。
3. 由此决定宿主侧补救：新窗口能画 → 框架提供“检测到设备丢失后重建窗口”的做法；新窗口也不能画 → 写成已知限制，建议重启应用，并整理最小复现向 Microsoft 报告（Q3）。

## G2 观测设计
- 探针在原有 XAML-LOST 探针上加 `--m3-xaml-rebuild`：启动时除主窗口外，再开一个**重启前窗口** W_pre（文字、按钮、圆角块、计数器）；驱动重启、显卡回到 OK 后 5 s，再新建一个**重启后窗口** W_post（同样内容）。
- 观测点：base（重启前，三个窗口中的两个）、t05（主窗口和 W_pre）、t10（W_post 建好 5 s 后，三个窗口都截）、t30（三个窗口都截）。每个观测点截两张（相隔 1 s）。截图请求里带上窗口句柄，runner 按句柄截对应窗口。
- 判读（复核时由 Claude 做）：每个窗口的文字区和圆角块区是否有内容，计数器是否在变；标签按窗口记 `TEXT-OK` / `TEXT-LOST`。

## G3 裁决（pidan：R1–R4 全部按建议；pidan 确认 DESKTOP-K93N3P8 有管理员权限 → R3 的第二台机器会话**纳入**）
| # | 问题 | 裁决（= 原建议） | 理由 |
|---|---|---|---|
| **R1** | 在哪个设置下测新窗口 | **DESKTOP-IJI1KJG，沿用执行方本机的 unpackaged + self-contained 设置**，3 个会话，每会话 1 次驱动重启；加自检共 4 次 | 与 XAML-LOST 同一环境，结果可以直接对照；这是回答“每窗口还是整个进程”的主数据 |
| **R2** | 要不要测打包部署（MSIX） | **要，尝试 1 个会话**：执行方另建一份**不带**本机 csproj 补丁的打包构建，用 `m0/run_m0.ps1` 的 Package 启动方式（M0-A 在 K93N3P8 上用过）。如果启动就报 `REGDB_E_CLASSNOTREG` 或其他部署错误，记 `BLOCKED_PACKAGED` 并跳过，不算失败，不去修部署问题 | 试点应用多半是打包部署；但打包在这台机器上之前启动失败过，所以只尝试，不为它扩大范围 |
| **R3** | 要不要在第二台机器上测 | **默认不测。** DESKTOP-K93N3P8（Win10 19045，GTX 970）在 M0-A 时没有管理员权限（PresentMon 没能输出），而驱动重启需要管理员。如果 pidan 能给那台机器安排管理员权限，就加 1 个会话（unpackaged，同样设置），能顺带看出 Windows 预览版 26300 和 AMD 驱动是不是原因 | 不让第二台机器卡住本票；能安排就是额外收益。**pidan 已确认有管理员权限，本票纳入 1 个会话**：该机在 M0-A 中以打包方式（`dotnet run` + MSIX 启动配置）运行，所以按该机能用的方式启动并在 summary 的 `packaged=` 中记录；若为打包启动，同时作为 R2 的第二个数据点（Win10 19045 / NVIDIA GTX 970 / 60Hz） |
| **R4** | 结果怎么用 | W_post 能画文字 → 下一步写宿主侧补救的设计（框架在检测到设备丢失时通知宿主，宿主重建窗口），作为 WP4 试点接入的约束；W_pre 也能画 → 说明只是“当时显示着的窗口”受影响，补救更简单；都不能 → 写成 M3 已知限制（建议重启应用），整理最小复现，由 pidan 决定是否向 microsoft-ui-xaml 报告（Q3） | 先拿到“新窗口能不能画”这个关键事实，再决定补救方案 |

## G4 实现（已写，未在真机运行；代码 = 分支 `claude/m3-xaml-rebuild` 提交 `12c720d`）
| 文件 | 改动 | 本地验证 |
|---|---|---|
| `MotionSoftDemo/M3XamlLostProbe.cs` | `--m3-xaml-rebuild`：主窗口移到屏幕左上角，W_pre 在 (0, 720)、W_post 在 (640, 720)，各 620×240 px，互不重叠（截图按屏幕像素复制）；两个测试窗口都有文字、计数器、按钮和圆角块（与 XAML-LOST 中消失的内容同类）。观测点 base（主窗口 + W_pre）→ 请求驱动重启 → t05（主窗口 + W_pre）→ 新建 W_post → t10（重启后 12 s，三个窗口）→ t30（三个窗口），每点每窗口两张，相隔 1 s。文件前缀 `m3xrb_s<n>_<观测点>_<main/wpre/wpost>_<a/b>.png`。summary 记录 `packaged=`（是否有包标识）、各窗口句柄、W_post 创建时间。数据目录和会话号也可由参数 `--m3-artdir=`、`--m3-session=` 给出（打包启动可能拿不到环境变量） | 对照 WinAppSDK 2.5.1 编译 0 个 C# 错误 |
| `MotionSoftDemo/MainPage.xaml.cs` +2 行；`m3/mainpage_m3xrb_hook.patch` | 入口 `--m3-xaml-rebuild` / `MF_M3_XAML_REBUILD`。补丁基于**已打 DPI、LOST、XAML-LOST 补丁**的 MainPage | 补丁由 `git diff` 生成 |
| `m3/run_m3_xaml_lost.ps1` | `-Rebuild`；截图请求“路径\|句柄”按句柄截窗口；`-LaunchMode Package -DemoProj <工程目录>` 改写 `Properties\launchSettings.json` 并用 `dotnet run` 打包启动（与 `m0/run_m0.ps1` 相同），90 s 内没有应用进程就记 `BLOCKED_PACKAGED` 并跳过该会话；注入、自检、间隔、上限、停止规则不变 | 本环境没有 PowerShell，人工复查 |

**运行安排**：IJI1KJG 上 unpackaged 3 个会话（R1，含自检共 4 次重启）；IJI1KJG 上打包 1 个会话（R2，另一次运行，含自检共 2 次）；K93N3P8 上 1 个会话（R3，含自检共 2 次）。每台机器每天 ≤ 8 次。

## G5 禁令
同 XAML-LOST；本 DRAFT ≠ 授权采数；诊断结果 ≠ 采用；不为 R2 去修部署问题；Q3 的对外报告由 pidan 决定。
