# 派题 · M3-RIVE-LOST（DESKTOP-IJI1KJG）

闸门：`docs/m3_rive_lost_gate_LOCKED.md`，GATE_BODY_SHA256=`753c0a6c82f06c81cf741eecba278b926127771befa2a51b515a925a7873f27d`，LOCKED_AT=2026-10-29 12:31:48 AWST。K1–K7 已按建议裁决。
代码分支：`claude/m3-rive-lost`。**尚未 APPROVED：第 1–3 步（取代码、构建、模拟冒烟）可以先做；第 4 步真实注入，必须等 `docs/m3_rive_lost_approved.md` 落盘后才能执行。**
`<ROOT>` 沿用 `C:\Users\Lh\mf_m0b`；数据目录用新的 `<ROOT>\artifacts_m3lost`。

## 0. 前提
- DESKTOP-IJI1KJG（K3）；**管理员** PowerShell。
- 确认 `dxcap -?` 能运行；不能就到“设置 → 系统 → 可选功能”添加“图形工具”（K3）。
- 本机 Demo 启动前置：DPI 票时你在本机 `MotionSoftDemo.csproj` 里加的 `WindowsPackageType=None` + `WindowsAppSDKSelfContained=true` 可以继续用，**只留在本机，不提交**（结题决断：不回灌主线）。
- **不改** TDR 注册表项（`TdrDelay`、`TdrLimitCount` 等），不改显卡驱动设置（G0）。

## 1. 取代码
```powershell
git -C <ROOT>\repo fetch origin claude/m3-rive-lost
git -C <ROOT>\repo checkout claude/m3-rive-lost
git -C <ROOT>\repo pull origin claude/m3-rive-lost
git -C <ROOT>\repo log -1 --format=%H
```

## 2. 构建
1. 把 `<ROOT>\repo\MotionSoftDemo\` 下的 `M0Probe.cs`、`M3DpiProbe.cs` 和 **新文件** `M3LostProbe.cs` 复制到工程目录。
2. 你本机的 MainPage 已经打过 DPI 补丁。在工程目录的上一级执行 `git apply --check <ROOT>\repo\m3\mainpage_m3lost_hook.patch`，通过后去掉 `--check` 再执行一次。打不上就照补丁手工加，并在回报里写明。
3. 重新构建 `m0rive.dll`（新增设备丢失恢复），并暂存测试素材：
   ```powershell
   powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m0\build_m0.ps1 -ArtDir <ROOT>\artifacts_m3lost -DemoBinDir <MotionSoftDemo.exe 所在目录>
   ```
4. 按 DPI 票的方式重新构建 MotionSoftDemo（Debug，直接启动 exe），确认新的 `m0rive.dll` 在 exe 旁边。
5. 失败时记 BLOCKED_TOOLCHAIN，回报 `m0\out\build_m0.log` 的最后 80 行；不要改 Rive 源码、Rive 参数或 m0rive.cpp 去凑（K7：本票不改 Rive）。

## 3. 模拟冒烟（不采数，不注入，可在 APPROVED 前做）
```powershell
powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m3\run_m3_lost.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <ROOT>\artifacts_m3lost_sim -Sessions 1 -Simulate
```
- 走的是进程内模拟丢失，不碰 GPU，不需要 dxcap。
- 期望：`m3lostsim_s1_summary.txt` 里 `verdict=SIM_RECORDED`、`recover_ok=1`，两张恢复后截图里有 Rive 画面。
- 回报这三项即可；模拟数据**不提交、不计入**判据（K1）。如果模拟就失败，先停下回报，不要进第 4 步。

## 4. 采数（真实注入；**必须等 approved 落盘**）
```powershell
powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m3\run_m3_lost.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <ROOT>\artifacts_m3lost -Sessions 3
```
- 开始前：关掉其他用 GPU 的程序（浏览器、游戏、视频等），保存手头工作（K5）。
- 每个会话注入 2 次，每次屏幕会黑 1–3 秒，然后恢复。runner 自动控制间隔（两次 ≥ 45 s，会话间 ≥ 120 s，全程 ≤ 8 次）。全程大约 10–15 分钟。
- 全程不碰鼠标键盘，屏幕保持常亮。
- **立即停止**的情况（K5）：蓝屏；显示器 30 s 内没恢复；驱动报错弹窗；runner 打出 `STOP:`。停下后记 `INVALID_SYSTEM`（runner 自己停下的照实写），**当天不重试**，回报事件查看器“Windows 日志 → 系统”里的 4101 和 BugCheck 记录。
- 应用崩溃（summary 里有 `verdict=FAIL_ABORT`，或矩阵里是 `CRASHED_NO_VERDICT`）**不是**你的问题，也不要重跑凑数：这是本票要找的结论（K7）。照常交付。

## 5. 交付
- 把 `<ROOT>\artifacts_m3lost` 下的 `m3lost_*` 文件（summary、raw、`_seg*.png`、`_seg*_ref.bgra`、session matrix、run log）和 `m0\out\build_m0.log`，提交到分支 `claude/m3-rive-lost` 的 `m3/results_lost/` 目录。
- 回一行中文：
  `M3-RIVE-LOST | commit=<sha> | build=<BUILD_OK/BLOCKED_TOOLCHAIN> | sim=<SIM_RECORDED/…> | sessions=<n> | lost_detected=<s1/s2/s3> | recover_ok=<s1/s2/s3> | recover_max_ms=<…> | budget_ok/stability_ok=<…> | frames_missed=<…> | mem_delta_mb=<…> | event4101=<…> | composition.h PRE=POST`

## 禁令
不改 composition.h；不重开 S1/R-3/R-2；禁影子 IV；不改 TDR 注册表；approved 落盘前不做真实注入；构建失败不改 Rive 硬凑；崩溃不重跑凑数；成功 ≠ 采用。
