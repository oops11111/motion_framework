# 派题 · M3-RIVE-LOST r2（DESKTOP-IJI1KJG）

闸门：`docs/m3_rive_lost_gate_r2_LOCKED.md`，GATE_BODY_SHA256=`dc0770c1f6a6819a5f6cffeb6bea2ef828b93045ec8e75f39910ee9537d29d10`，LOCKED_AT=2026-10-29 18:13:07 AWST。L1–L5 已按建议裁决。
代码分支：`claude/m3-rive-lost`。**已 APPROVED（`docs/m3_rive_lost_r2_approved.md`，pidan @ 2026-10-29 18:15:58 AWST）：第 1–5 步都可以执行；第 3 步模拟冒烟通过后再做第 4 步。**
`<ROOT>` 沿用 `C:\Users\Lh\mf_m0b`；数据目录用新的 `<ROOT>\artifacts_m3lost_r2`。r1 的 `artifacts_m3lost` 和 `m3/results_lost/` 不动、不混用。

## 和 r1 的区别
- 不再用 `dxcap`。注入改为**重启显卡驱动**（`pnputil /restart-device`，Windows 自带），每次屏幕会黑几秒（L1）。
- runner 会在第 1 个会话前，**不启动应用**先重启一次驱动作为自检（L2）。自检不过就停，记 `BLOCKED_INJECT`。
- DLL（`m0rive.dll`）没有改动，可以沿用 r1 构建的那个。

## 0. 前提
- DESKTOP-IJI1KJG；**管理员** PowerShell。
- 运行 `Get-PnpDevice -Class Display -PresentOnly`，记下 RX 7800 XT 那一行的 InstanceId。如果除了它还有其他状态为 OK 的显示适配器（Microsoft 基本显示适配器除外），第 4 步要加 `-GpuInstanceId "<该 ID>"`。
- 本机 csproj 补丁照旧只留本机；不改 TDR 注册表和显卡驱动设置。

## 1. 取代码
```powershell
git -C <ROOT>\repo fetch origin claude/m3-rive-lost
git -C <ROOT>\repo checkout claude/m3-rive-lost
git -C <ROOT>\repo pull origin claude/m3-rive-lost
git -C <ROOT>\repo log -1 --format=%H
```

## 2. 构建
把 `<ROOT>\repo\MotionSoftDemo\M3LostProbe.cs` 复制到工程目录（覆盖 r1 版本），重新构建 MotionSoftDemo（Debug，直接启动 exe）。MainPage 补丁和 `m0rive.dll` 不用动。失败时记 BLOCKED_TOOLCHAIN 并回报日志，不要改代码去凑。

## 3. 模拟冒烟（不采数、不注入）
```powershell
powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m3\run_m3_lost.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <ROOT>\artifacts_m3lost_r2_sim -Sessions 1 -Simulate
```
期望 `verdict=SIM_RECORDED`、`recover_ok=1`、summary 里 `probe=M3-LOST v2`。数据不提交。失败就先停下回报。

## 4. 采数（已 APPROVED）
```powershell
powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m3\run_m3_lost.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <ROOT>\artifacts_m3lost_r2 -Sessions 3 [-GpuInstanceId "<ID>"]
```
- 开始前关掉其他用 GPU 的程序，保存手头工作。
- 先自检（约 30 s，屏幕黑一下），然后冷却 120 s，再跑 3 个会话，每会话 2 次注入，全程约 12–18 分钟。全程不碰鼠标键盘。
- **立即停止**：蓝屏；显示器 30 s 内没恢复；驱动报错弹窗；runner 打出 `STOP:`。记 `INVALID_SYSTEM` 或 runner 给出的标签，当天不重试，回报事件查看器“系统”日志里的相关记录。
- 应用崩溃（`FAIL_ABORT` / `CRASHED_NO_VERDICT`）是本票要找的结论（K7），照常交付，不重跑凑数。

## 5. 交付
- 把 `<ROOT>\artifacts_m3lost_r2` 下的 `m3lost_*` 文件（summary、raw、`_seg*.png`、`_seg*_ref.bgra`、session matrix、run log）提交到分支 `claude/m3-rive-lost` 的 `m3/results_lost_r2/` 目录。
- 回一行中文：
  `M3-RIVE-LOST r2 | commit=<sha> | build=<BUILD_OK/BLOCKED_TOOLCHAIN> | sim=<…> | selftest=<OK/BLOCKED_INJECT> | sessions=<n> | lost_detected=<s1/s2/s3> | recover_ok=<s1/s2/s3> | recover_max_ms=<…> | budget_ok/stability_ok=<…> | frames_missed=<…> | mem_delta_mb=<…> | injections_ok=<…> | composition.h PRE=POST`

## 禁令
不改 composition.h；不重开 S1/R-3/R-2；禁影子 IV；不改 TDR 注册表；approved 落盘前不做真实注入；构建失败不改代码硬凑；崩溃不重跑凑数；成功 ≠ 采用。
