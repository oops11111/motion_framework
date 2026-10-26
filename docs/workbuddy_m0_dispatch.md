# 派题 · M0-A 技术验证（DESKTOP-K93N3P8）

闸门：`docs/m0_tech_validation_gate_LOCKED.md`，GATE_BODY_SHA256=`942d6965dae1aa41f3b853923d08a11aac9714d60018888a5c486bfde5809768`，LOCKED_AT=2026-10-26 21:38:45 AWST。D1–D7 已按建议裁决，D1 = 允许 SendInput，仅限 M0。
代码分支：`claude/m0-tech-validation`。**已 APPROVED（`docs/m0_tech_validation_approved.md`，2026-10-26 21:43:10 AWST）：构建和采数（第 5 步）都可以执行。**

## 1. 取代码
这台机器上的 `C:\Users\admin\motion_framework` 不是 git 仓库，**不要覆盖它**。把分支取到旁边的新目录：
```powershell
git clone -b claude/m0-tech-validation https://github.com/oops11111/motion_framework.git C:\Users\admin\mf_m0
```
本机拉不到时（没有 GitHub 权限），请协调从 GitHub 下载该分支的 zip，解压到同一位置。

核对：
- `git -C C:\Users\admin\mf_m0 log -1 --format=%H` 输出的 commit 写进回报；
- `C:\Users\admin\mf_m0\code\composition.h` 的 sha256 应为 `a55d4952…e418`。

## 2. 把探针接进本机的 MotionSoftDemo 工程
1. 把 `C:\Users\admin\mf_m0\MotionSoftDemo\M0Probe.cs` 复制到本机 MotionSoftDemo 工程目录。
2. 在**工程目录的上一级**执行：`git apply --check m0\mainpage_m0_hook.patch`；通过后去掉 `--check` 再执行一次。这个补丁在 `MainPage.xaml.cs` 里加 11 行：一个字段、一个开关解析、一个派发分支，再加两处日志名和日志开关。
3. 如果本机的 `MainPage.xaml.cs` 和仓库版本不一致，导致补丁打不上，就照补丁内容手工加这 5 处，并在回报里写明。
4. 不改 `composition.h`，也不改别的探针。

## 3. 构建 native 部分
```powershell
powershell -ExecutionPolicy Bypass -File C:\Users\admin\mf_m0\m0\build_m0.ps1 -DemoBinDir <MotionSoftDemo.exe 所在目录>
```
- 用 MSVC（D4），**不要安装 clang-cl**。需要 Git for Windows 的 bash 在 PATH 上，并且能访问 github.com（Rive 构建脚本会自动下载 premake）。
- 成功时日志末尾是 `BUILD_OK`，并打印 `m0rive.dll` 和 `m0_tear_probe.exe` 的 sha256。
- 失败时**不要改 Rive 的构建参数或 m0rive.cpp 去凑**。把 `m0\out\build_m0.log` 的最后 80 行原样回报，结论写 `BLOCKED_TOOLCHAIN`。最可能出问题的是链接阶段：依赖库缺失或重复定义。

## 4. 重新构建 MotionSoftDemo
按平常方式构建，确认 `m0rive.dll` 和 `MotionSoftDemo.exe` 在同一目录。

## 5. 采数（已 APPROVED）
```powershell
powershell -ExecutionPolicy Bypass -File C:\Users\admin\mf_m0\m0\run_m0.ps1 -DemoExe <...\MotionSoftDemo.exe> -Inject -Sessions 3 [-PresentMon <...\PresentMon.exe>]
```
- 跑的时候不要碰鼠标和键盘，`-Inject` 会移动光标。屏幕保持常亮，不要锁屏（#9 的教训：熄屏后 DWM 会改成按计时器出帧）。
- 没有 PresentMon，或者因为没有管理员权限跑不了，照常继续，并在回报里写明。

## 6. 交付
- `C:\Users\admin\motion_framework\artifacts\` 下的 `m0_*` 文件和 `m0_tear\` 目录（清单见 `m0/README.md`），外加 `m0\out\build_m0.log`。
- 有推送权限：提交到分支 `claude/m0-tech-validation` 的 `artifacts/` 目录。没有：打包成 `m0_a_results.zip` 交给协调。
- 回一行中文：
  `M0-A | commit=<sha> | build=<BUILD_OK/BLOCKED_TOOLCHAIN> | sessions=<n> | m0_1a=<各会话 verdict 和 frames> | m0_1a cpu/gpu p95=<ms> | expr/ui 延迟 p50=<ms> | misalign_px=<值> | tear=<comp_tear.create_hr / first_present_hr> | composition.h PRE=POST`

## 禁令
不改 composition.h；不重开 S1、R-3、R-2；不解冻 Bv；禁影子 IV；approved 落盘前不采数；构建失败不改参数硬凑；成功 ≠ 采用。
