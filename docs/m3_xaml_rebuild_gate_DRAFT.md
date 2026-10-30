# M3-XAML-REBUILD 门槛闸门（m3_xaml_rebuild_gate_DRAFT.md）

STATUS=DRAFT（**NOT LOCKED**；本文件**不授权采数**）
LOCKED_AT: （LOCK 时填）
GATE_BODY_SHA256: （LOCK 时算；约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希）

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> 起草：Claude，2026-10-30。来源：M3-XAML-LOST 结果 `XAML-LOST`（`docs/m3_xaml_lost_result_claude_review.md`；pidan Q1–Q4 裁决：Q2 = 做一张短的后续诊断，找宿主侧的补救办法）。
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

## G3 待裁决（R1–R4，附建议）
| # | 问题 | 建议 | 理由 |
|---|---|---|---|
| **R1** | 在哪个设置下测新窗口 | **DESKTOP-IJI1KJG，沿用执行方本机的 unpackaged + self-contained 设置**，3 个会话，每会话 1 次驱动重启；加自检共 4 次 | 与 XAML-LOST 同一环境，结果可以直接对照；这是回答“每窗口还是整个进程”的主数据 |
| **R2** | 要不要测打包部署（MSIX） | **要，尝试 1 个会话**：执行方另建一份**不带**本机 csproj 补丁的打包构建，用 `m0/run_m0.ps1` 的 Package 启动方式（M0-A 在 K93N3P8 上用过）。如果启动就报 `REGDB_E_CLASSNOTREG` 或其他部署错误，记 `BLOCKED_PACKAGED` 并跳过，不算失败，不去修部署问题 | 试点应用多半是打包部署；但打包在这台机器上之前启动失败过，所以只尝试，不为它扩大范围 |
| **R3** | 要不要在第二台机器上测 | **默认不测。** DESKTOP-K93N3P8（Win10 19045，GTX 970）在 M0-A 时没有管理员权限（PresentMon 没能输出），而驱动重启需要管理员。如果 pidan 能给那台机器安排管理员权限，就加 1 个会话（unpackaged，同样设置），能顺带看出 Windows 预览版 26300 和 AMD 驱动是不是原因 | 不让第二台机器卡住本票；能安排就是额外收益 |
| **R4** | 结果怎么用 | W_post 能画文字 → 下一步写宿主侧补救的设计（框架在检测到设备丢失时通知宿主，宿主重建窗口），作为 WP4 试点接入的约束；W_pre 也能画 → 说明只是“当时显示着的窗口”受影响，补救更简单；都不能 → 写成 M3 已知限制（建议重启应用），整理最小复现，由 pidan 决定是否向 microsoft-ui-xaml 报告（Q3） | 先拿到“新窗口能不能画”这个关键事实，再决定补救方案 |

## G4 实现计划（R 裁决后写，LOCK 前完成）
| 文件 | 改动 |
|---|---|
| `MotionSoftDemo/M3XamlLostProbe.cs` | 加 rebuild 模式：W_pre / W_post 的创建（`new Window`，内容为文字、按钮、圆角块、计数器），截图请求带窗口句柄 |
| `MainPage.xaml.cs` 入口 + 补丁 | `--m3-xaml-rebuild` |
| `m3/run_m3_xaml_lost.ps1` | 截图请求支持“路径\|句柄”；`-Rebuild` 开关；R2 的打包会话通过 `-LaunchMode Package` 启动 |

## G5 禁令
同 XAML-LOST；本 DRAFT ≠ 授权采数；诊断结果 ≠ 采用；不为 R2 去修部署问题；Q3 的对外报告由 pidan 决定。
