# 派题 · M3-XAML-LOST（DESKTOP-IJI1KJG，诊断票）

闸门：`docs/m3_xaml_lost_gate_LOCKED.md`，GATE_BODY_SHA256=`69a52e2de66153348bff5197dbb037dd2650c26055b834173abde6ae6a1e1f55`，LOCKED_AT=2026-10-30 13:36:05 AWST。N1–N5 已按建议裁决。
代码分支：**`claude/m3-xaml-lost`**（新分支）。**已 APPROVED（`docs/m3_xaml_lost_approved.md`，pidan @ 2026-10-30 13:42:56 AWST）：第 1–5 步都可以执行；第 3 步走流程通过后再做第 4 步。**
`<ROOT>` 沿用 `C:\Users\Lh\mf_m0b`；数据目录用新的 `<ROOT>\artifacts_m3xaml`。

## 要做什么
看普通 WinUI 3 窗口（不加载 Rive）在显卡驱动重启后，界面会不会恢复；再用带 Rive 的窗口对照一次。窗口里会出现一个每 0.25 秒变一次的数字和一个红绿交替的方块，**这是正常的**。

## 0. 前提
同 RECOVER：管理员 PowerShell；只有一块可用显卡时不需要 `-GpuInstanceId`；本机 csproj 补丁只留本机；不改 TDR 注册表和驱动设置。

## 1. 取代码
```powershell
git -C <ROOT>\repo fetch origin claude/m3-xaml-lost
git -C <ROOT>\repo checkout claude/m3-xaml-lost
git -C <ROOT>\repo pull origin claude/m3-xaml-lost
git -C <ROOT>\repo log -1 --format=%H
```

## 2. 构建
1. 把 `<ROOT>\repo\MotionSoftDemo\M3XamlLostProbe.cs`（**新文件**）复制到工程目录。
2. 你本机的 MainPage 已经打过 DPI 和 LOST 补丁。在工程目录的上一级执行 `git apply --check <ROOT>\repo\m3\mainpage_m3xaml_hook.patch`，通过后去掉 `--check` 再执行一次。打不上就照补丁手工加，并在回报里写明。
3. 重新构建 MotionSoftDemo（Debug，直接启动 exe）。`m0rive.dll` **沿用 RECOVER 那次构建的**（不用重编），确认它在 exe 旁边，`<ROOT>\artifacts_m3xaml\m0_assets\` 下有 `Knight_square_2.riv`（可从 `artifacts_m3lost_recover\m0_assets\` 复制）。
4. 失败时记 BLOCKED_TOOLCHAIN 并回报日志；不要改代码去凑。

## 3. 走一遍流程（不重启驱动，不采数）
```powershell
powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m3\run_m3_xaml_lost.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <ROOT>\artifacts_m3xaml_dry -PlainSessions 1 -RiveSessions 1 -DryRun
```
期望两个会话都是 `verdict=RECORDED`，每个会话 14 张截图（7 个观测点 × 2）。数据不提交。失败就先停下回报。

## 4. 采数（已 APPROVED）
```powershell
powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m3\run_m3_xaml_lost.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <ROOT>\artifacts_m3xaml
```
- 默认：3 个普通会话 + 1 个带 Rive 的会话，每个会话重启 1 次驱动；加自检共 5 次。全程约 15 分钟。
- 开始前关掉其他用 GPU 的程序，保存手头工作。全程不碰鼠标键盘（探针自己会改窗口大小、最小化再还原，**不要去管它**）。
- **立即停止**：蓝屏；显示器 30 s 内没恢复；驱动报错弹窗；runner 打出 `STOP:`。当天不重试。

## 5. 交付
- 把 `<ROOT>\artifacts_m3xaml` 下的 `m3xaml_*` 文件（summary、raw、全部 png、stderr、session matrix、run log）提交到分支 `claude/m3-xaml-lost` 的 `m3/results_xaml_lost/` 目录。
- **提交前先 `git pull origin claude/m3-xaml-lost`**，只新增结果文件，不改 `docs/` 下已有文件。
- 回一行中文：
  `M3-XAML-LOST | commit=<sha> | build=<…> | dryrun=<…> | selftest=<OK/BLOCKED_INJECT> | sessions=<n> | verdict=<s1/s2/s3/s4> | inject_ok=<…> | rive_recovered(s4)=<…> | 目视：普通窗口重启后界面<恢复/没恢复/某步后恢复> | 带Rive窗口<…> | composition.h PRE=POST`

## 禁令
不改 composition.h；不重开 S1/R-3/R-2；禁影子 IV；不改 TDR 注册表；approved 落盘前不重启驱动；构建失败不改代码硬凑；诊断结果 ≠ 采用。
