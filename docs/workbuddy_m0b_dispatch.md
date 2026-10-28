> **已被 r3 取代**：重跑请用 `docs/workbuddy_m0b_r3_dispatch.md`（闸门 r3 `74172978…8ede`）。本文件保留，作为 r2 那一轮的记录。

# 派题 · M0-B Win11 复核

闸门：`docs/m0b_win11_gate_r2_LOCKED.md`，GATE_BODY_SHA256=`fd3acfc787564137532cc52adb64066427a88b159f0c6b4312dfd9dece40505c`，LOCKED_AT=2026-10-28 09:16:57 AWST。r2 取代 r1（`5e1bbb6d…f203`），唯一改动是 M0-2 判定所需刷新率从 ≥120Hz 改为 ≥75Hz。E1–E4 已按建议裁决。
代码分支：`claude/m0b-win11-run`。**已 APPROVED（`docs/m0b_win11_approved.md`，pidan @ 2026-10-28 09:18:00 AWST）：第 0–6 步都可以执行。第 0 步预检不满足 G1 第 1 或第 4 行（不是 Win11，或机器还是 DESKTOP-K93N3P8）时，停下回报，不采数。**
下文的 `<ROOT>` 指 Win11 机器上的工作目录，例如 `C:\Users\<用户名>\mf_m0b`；`<ART>` 指 artifacts 目录，例如 `<ROOT>\artifacts`。

## 0. 预检（不采数，先回报）
在 Win11 机器的 PowerShell 里运行，把输出原样回报：
```powershell
$env:COMPUTERNAME
(Get-CimInstance Win32_ComputerSystemProduct).UUID
(Get-CimInstance Win32_OperatingSystem) | Select-Object Caption, BuildNumber
Get-CimInstance Win32_VideoController | Select-Object Name, CurrentRefreshRate, CurrentHorizontalResolution, CurrentVerticalResolution
([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
```
对照闸门 G1：
- build ≥ 22000，UUID 不是 `03000200-0400-0500-0006-000700080009`，否则整票 INVALID，停在这里；
- 刷新率 < 75Hz：可以继续，但 M0-2 只记录、不判定（E1）。如果显示设置里能调到 75Hz 或更高，先调好（选这台显示器支持的最高刷新率）；
- 没有管理员权限：可以继续，但 M0-4 拿不到 PresentMode。

## 1. 取代码
```powershell
git clone -b claude/m0b-win11-run https://github.com/oops11111/motion_framework.git <ROOT>\repo
```
把 `git -C <ROOT>\repo log -1 --format=%H` 的输出写进回报。

## 2. 准备 MotionSoftDemo 工程
仓库里**没有**完整的 MotionSoftDemo 工程：缺包清单、`App.xaml`，以及原生 DLL `mfprobe.dll`（WGC 采集）和 `fluidmotion.dll`（C ABI）的构建产物。请：
1. 从 DESKTOP-K93N3P8 把能正常构建的整个工程目录 `C:\Users\admin\motion_framework\MotionSoftDemo`，连同它依赖的原生 DLL（M0-A 时 MotionSoftDemo.exe 旁边的 `mfprobe.dll`、`fluidmotion.dll`），复制到 `<ROOT>\MotionSoftDemo`；
2. 用 `<ROOT>\repo\MotionSoftDemo\M0Probe.cs` 覆盖工程里的同名文件（本分支是 v2）；`MainPage.xaml.cs` 的 M0 入口在 M0-A 时已经加过，不要重复打补丁；
3. 在这台机器上构建一次，确认能启动。如果这台机器缺 .NET SDK 或 WinAppSDK 运行时，按 M0-A 机器上的版本安装（WinAppSDK 2.5.1），并在回报里写明。

## 3. 构建 native 部分
```powershell
powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m0\build_m0.ps1 -ArtDir <ART> -DemoBinDir <MotionSoftDemo.exe 所在目录>
```
- `build_m0.ps1` 中有 M0-A 机器的路径（GnuWin32 make、WorkBuddy 自带的 python）。如果这台机器的路径不同，只改 PATH 相关的行，并在回报里写明；**不要改 Rive 的构建参数或 m0rive.cpp**。
- 失败时记 BLOCKED_TOOLCHAIN，回报 `m0\out\build_m0.log` 的最后 80 行。

## 4. 准备 PresentMon（M0-4）
- 有管理员权限时：用**提权**的 PowerShell 执行第 5 步，并传入 `-PresentMon <PresentMon.exe 路径>`（2.x 版本）。
- 没有管理员权限：不传 `-PresentMon`，在回报里写明。

## 5. 采数（已 APPROVED）
```powershell
powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m0\run_m0.ps1 -DemoExe <...\MotionSoftDemo.exe> -DemoProj <ROOT>\MotionSoftDemo -ArtDir <ART> -Inject -Sessions 3 [-PresentMon <...\PresentMon.exe>]
```
- 运行期间不要碰鼠标和键盘；屏幕保持常亮，不要锁屏。
- 显示缩放保持 100%，与 M0-A 一致（缩放矩阵不在本票范围内）。

## 6. 交付
- 把 `<ART>` 下的 `m0_*` 文件、`m0_tear\` 目录（包括 `presentmon.csv`，如果有的话）、`m0_run.log`，以及 `m0\out\build_m0.log`，提交到分支 `claude/m0b-win11-run` 的 `m0/results_m0b/` 目录。截图现在只截应用窗口，可以直接提交。
- 回一行中文：
  `M0-B | commit=<sha> | host=<名> build=<n> refresh=<Hz> elevated=<是/否> | build=<BUILD_OK/BLOCKED_TOOLCHAIN> | sessions=<n> | misalign_ms=<s1/s2/s3> | m0_1a frames/cpu/gpu p95=<…> | ref 非空=<是/否> | comp_tear PresentMode=<…或 无PresentMon> | composition.h PRE=POST`

## 禁令
不改 composition.h；不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；approved 落盘前不采数；构建失败不改 Rive 参数硬凑；成功 ≠ 采用。
