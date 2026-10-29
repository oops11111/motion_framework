# 派题 · M3-RIVE-DPI（DESKTOP-IJI1KJG）

闸门：`docs/m3_rive_dpi_gate_LOCKED.md`，GATE_BODY_SHA256=`92e6b4e9fd1014faf74714eb6cb20fc7e998169dc1171785bb8ea43f68d97bf5`，LOCKED_AT=2026-10-28 20:39:58 AWST。G1–G4 已按建议裁决。
代码分支：`claude/m3-rive-dpi`。**已 APPROVED（`docs/m3_rive_dpi_approved.md`，pidan @ 2026-10-28 21:28:50 AWST）：第 1–5 步都可以执行。**
`<ROOT>` 沿用 `C:\Users\Lh\mf_m0b`；数据目录用新的 `<ROOT>\artifacts_m3dpi`。

## 0. 前提
DESKTOP-IJI1KJG（G3）；开始时缩放为 **100%**；在“设置 → 显示 → 缩放”里确认能选 125%（1280×1024 下通常只有 100% 和 125%）。不需要管理员权限，也不注入指针（G4）。

## 1. 取代码
```powershell
git -C <ROOT>\repo fetch origin claude/m3-rive-dpi
git -C <ROOT>\repo checkout claude/m3-rive-dpi
git -C <ROOT>\repo pull origin claude/m3-rive-dpi
git -C <ROOT>\repo log -1 --format=%H
```

## 2. 构建
1. 把 `<ROOT>\repo\MotionSoftDemo\M0Probe.cs` 和 **新文件** `M3DpiProbe.cs` 复制到工程目录；在工程目录的上一级执行 `git apply --check <ROOT>\repo\m3\mainpage_m3_hook.patch`，通过后去掉 `--check` 再执行一次。补丁打不上就照补丁手工加 5 处，并在回报里写明。
2. 重新构建 `m0rive.dll`（新增了 `m0r_resize`），并暂存测试素材：
   ```powershell
   powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m0\build_m0.ps1 -ArtDir <ROOT>\artifacts_m3dpi -DemoBinDir <MotionSoftDemo.exe 所在目录>
   ```
3. 按 M0-B r3 的方式重新构建 MotionSoftDemo（Debug，直接启动 exe），确认新的 `m0rive.dll` 在 exe 旁边。
4. 失败时记 BLOCKED_TOOLCHAIN，回报 `m0\out\build_m0.log` 的最后 80 行；不要改 Rive 参数或 m0rive.cpp 去凑。

## 3. 熟悉一次流程（不采数，可选）
不带 runner、手动启动一次 `MotionSoftDemo.exe --m3-dpi-probe`，看清楚提示出现在哪里。这一次的输出不提交。

## 4. 采数（已 APPROVED）
```powershell
powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m3\run_m3_dpi.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <ROOT>\artifacts_m3dpi -Sessions 3
```
- 每个会话：应用窗口里会依次提示“请把显示缩放改为 125%”“…100%”，共 4 次。每次看到提示，就到“设置 → 显示 → 缩放”切换，然后**把焦点放回应用窗口旁边，不要再动**，直到下一次提示。每个会话大约 1–3 分钟，取决于切换速度。
- 每个会话开始前，缩放必须是 100%（4 次切换后会回到 100%）。
- 除了切换缩放，不要碰鼠标和键盘；屏幕保持常亮。
- runner 会在每一段自动截应用窗口。

## 5. 交付
- 把 `<ROOT>\artifacts_m3dpi` 下的 `m3dpi_*` 文件（summary、raw、`_seg*.png`、`_seg*_ref.bgra`、session matrix、run log）和 `m0\out\build_m0.log`，提交到分支 `claude/m3-rive-dpi` 的 `m3/results_dpi/` 目录。
- 回一行中文：
  `M3-RIVE-DPI | commit=<sha> | build=<BUILD_OK/BLOCKED_TOOLCHAIN> | sessions=<n> | switches=<s1/s2/s3> | size_ok=<s1/s2/s3> | reraster_max_ms=<…> | budget_ok/stability_ok=<…> | mem_delta_mb=<…> | composition.h PRE=POST`

## 禁令
不改 composition.h；不重开 S1/R-3/R-2；禁影子 IV；approved 落盘前不采数；构建失败不改 Rive 参数硬凑；成功 ≠ 采用。
