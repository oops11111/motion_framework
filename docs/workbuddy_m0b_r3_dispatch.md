# 派题 · M0-B r3 重跑（DESKTOP-IJI1KJG）

闸门：`docs/m0b_win11_gate_r3_LOCKED.md`，GATE_BODY_SHA256=`741729784693893dedb6190b5d2768617045554ae90a58dfe00fd52855f48ede`，LOCKED_AT=2026-10-28 14:53:18 AWST。r3 取代 r2；判据不变，只要求用探针 v3 在同一台机器上整票重跑。
代码分支：`claude/m0b-win11-run`。**已 APPROVED（`docs/m0b_win11_r3_approved.md`，pidan @ 2026-10-28 15:26:25 AWST）：第 1–5 步都可以执行；要求在提权 shell 中运行并带 `-PresentMon`。**
`<ROOT>` 沿用上一轮的 `C:\Users\Lh\mf_m0b`。新数据放在**新目录** `<ROOT>\artifacts_r3`，不要和上一轮的 `<ROOT>\artifacts` 混用。

## 0. 前提
必须是同一台 DESKTOP-IJI1KJG（SMBIOS UUID `03FF0210-04E0-057F-E006-E90700080009`），刷新率保持 75Hz，缩放 100%。如果换了机器，就停下回报。

## 1. 更新代码
```powershell
git -C <ROOT>\repo pull origin claude/m0b-win11-run
git -C <ROOT>\repo log -1 --format=%H
```
把 commit 写进回报。

## 2. 重新构建（m0rive.dll 和 M0Probe.cs 都改了）
1. 用 `<ROOT>\repo\MotionSoftDemo\M0Probe.cs` 覆盖工程里的同名文件，并确认文件里有 `Kv("probe", "M0 v3")`。
2. 重新构建 native 部分。Rive 库上次已经编好，脚本会跳过，只重编 `m0rive.dll` 和撕裂探针：
   ```powershell
   powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m0\build_m0.ps1 -ArtDir <ROOT>\artifacts_r3 -DemoBinDir <MotionSoftDemo.exe 所在目录>
   ```
3. 按上一轮的方式重新构建 MotionSoftDemo，确认新的 `m0rive.dll` 和 exe 在同一目录。
4. 失败时记 BLOCKED_TOOLCHAIN，回报 `m0\out\build_m0.log` 的最后 80 行；不要改 Rive 参数或 m0rive.cpp 去凑。

## 3. PresentMon（建议这次补上）
如果能拿到管理员权限，就用**提权**的 PowerShell 跑第 4 步，并传入 `-PresentMon <PresentMon.exe 路径>`（2.x 版本），这样 M0-4 能拿到 PresentMode。拿不到就照常跑，并在回报里写明。

## 4. 采数（已 APPROVED）
```powershell
powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m0\run_m0.ps1 -DemoExe <...\MotionSoftDemo.exe> -DemoProj <ROOT>\MotionSoftDemo -ArtDir <ROOT>\artifacts_r3 -Inject -Sessions 3 [-PresentMon <...\PresentMon.exe>]
```
- 运行期间不要碰鼠标和键盘；屏幕保持常亮。每个会话比上一轮长约 25 秒（拖动从 16 次增加到 40 次）。
- 跑完后检查：三个 `m0_s*_summary.txt` 都应有 `probe=M0 v3`，以及 `m0_2.drag_expr.samples`、`m0_2.drag_surface.samples` 和 `m0_1a.gpu_samples_dropped` 这几行。

## 5. 交付
- 把 `<ROOT>\artifacts_r3` 下的 `m0_*` 文件、`m0_tear\` 目录、`m0_run.log`，以及 `m0\out\build_m0.log`，提交到分支 `claude/m0b-win11-run` 的 **`m0/results_m0b_r3/`** 目录。不要动 `m0/results_m0b/`。
- 回一行中文：
  `M0-B r3 | commit=<sha> | build=<BUILD_OK/BLOCKED_TOOLCHAIN> | elevated=<是/否> | sessions=<n> | samples(expr/surface)=<s1/s2/s3> | misalign_ms=<s1/s2/s3> | m0_1a cpu/gpu p95=<…> gpu_dropped=<…> | comp_tear PresentMode=<…或 无PresentMon> | composition.h PRE=POST`

## 禁令
不改 composition.h；不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；r3 approved 落盘前不采数；不与 r2 数据混用；构建失败不改 Rive 参数硬凑；成功 ≠ 采用。
