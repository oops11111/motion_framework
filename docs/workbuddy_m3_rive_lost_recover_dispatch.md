# 派题 · M3-RIVE-LOST-RECOVER（DESKTOP-IJI1KJG）

闸门：`docs/m3_rive_lost_recover_gate_LOCKED.md`，GATE_BODY_SHA256=`16202071fd08d091d0a0d825eda2297297ae19227d72a6cf981d4ed8b25484ce`，LOCKED_AT=2026-10-30 11:31:47 AWST。M1–M5 已按建议裁决。
代码分支：`claude/m3-rive-lost`。**已 APPROVED（`docs/m3_rive_lost_recover_approved.md`，pidan @ 2026-10-30 11:47:20 AWST）：第 1–5 步都可以执行；第 3 步模拟冒烟通过后再做第 4 步。**
`<ROOT>` 沿用 `C:\Users\Lh\mf_m0b`；数据目录用新的 `<ROOT>\artifacts_m3lost_recover`。r1、r2 的数据目录和 `m3/results_lost*/` 不动、不混用。

## 和 r2 的区别
- **这次 `m0rive.dll` 改了，必须重新构建**（恢复前先确认新设备可用；记录 abort 位置）。
- 注入手段、自检、次数、间隔、停止条件和 r2 完全一样。
- 每个会话会多一个 `m3lost_s<n>_stderr.txt`，里面是恢复过程的记录；如果 Rive 再 abort，里面会有出错的那一行。**一定要提交。**

## 0. 前提
同 r2：DESKTOP-IJI1KJG；管理员 PowerShell；如有多块可用显示适配器，第 4 步加 `-GpuInstanceId "<RX 7800 XT 的 InstanceId>"`；本机 csproj 补丁只留本机；不改 TDR 注册表和驱动设置。

## 1. 取代码
```powershell
git -C <ROOT>\repo fetch origin claude/m3-rive-lost
git -C <ROOT>\repo checkout claude/m3-rive-lost
git -C <ROOT>\repo pull origin claude/m3-rive-lost
git -C <ROOT>\repo log -1 --format=%H
```

## 2. 构建
1. 重新构建 `m0rive.dll`（Rive 库已缓存，只会重编 DLL）：
   ```powershell
   powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m0\build_m0.ps1 -ArtDir <ROOT>\artifacts_m3lost_recover -DemoBinDir <MotionSoftDemo.exe 所在目录>
   ```
2. 把 `<ROOT>\repo\MotionSoftDemo\M3LostProbe.cs` 复制到工程目录（覆盖），重新构建 MotionSoftDemo（Debug，直接启动 exe），确认新的 `m0rive.dll` 在 exe 旁边。MainPage 补丁不用动。
3. 失败时记 BLOCKED_TOOLCHAIN，回报 `m0\out\build_m0.log` 的最后 80 行；不要改代码或 Rive 去凑。

## 3. 模拟冒烟（不采数、不注入）
```powershell
powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m3\run_m3_lost.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <ROOT>\artifacts_m3lost_recover_sim -Sessions 1 -Simulate
```
期望 `verdict=SIM_RECORDED`、`recover_ok=1`、`probe=M3-LOST v3`；恢复耗时会比 r2 多约 0.5 s（新设备要稳定 500 ms）。数据不提交。失败就先停下回报。

## 4. 采数（已 APPROVED）
```powershell
powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m3\run_m3_lost.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <ROOT>\artifacts_m3lost_recover -Sessions 3 [-GpuInstanceId "<ID>"]
```
- 开始前关掉其他用 GPU 的程序，保存手头工作。全程不碰鼠标键盘。
- 先自检，冷却 120 s，再跑 3 个会话，每会话 2 次驱动重启；顺利的话全程约 12–18 分钟。
- **立即停止**：蓝屏；显示器 30 s 内没恢复；驱动报错弹窗；runner 打出 `STOP:`。当天不重试。
- 应用 abort（`FAIL_ABORT`）照常交付，不重跑凑数；这次的 stderr 文件和 `abort_stage` 就是要找的东西。

## 5. 交付
- 把 `<ROOT>\artifacts_m3lost_recover` 下的 `m3lost_*` 文件（summary、raw、**stderr**、`_seg*.png`、`_seg*_ref.bgra`、session matrix、run log）和 `m0\out\build_m0.log` 提交到分支 `claude/m3-rive-lost` 的 `m3/results_lost_recover/` 目录。
- **提交前先 `git pull origin claude/m3-rive-lost`**，只新增结果文件，不改 `docs/` 下已有文件。
- 回一行中文：
  `M3-RIVE-LOST-RECOVER | commit=<sha> | build=<BUILD_OK/BLOCKED_TOOLCHAIN> | sim=<…> | selftest=<OK/BLOCKED_INJECT> | sessions=<n> | lost_detected=<s1/s2/s3> | recover_ok=<s1/s2/s3> | recover_max_ms=<…> | abort_stage=<无/…> | budget_ok/stability_ok=<…> | frames_missed=<…> | mem_delta_mb=<…> | injections_ok=<…> | composition.h PRE=POST`

## 禁令
不改 composition.h；不重开 S1/R-3/R-2；禁影子 IV；不改 TDR 注册表；approved 落盘前不做真实注入；构建失败不改代码硬凑；abort 不重跑凑数；成功 ≠ 采用。
