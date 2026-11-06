# 派题 · M3-MATRIX-PACING r2（DESKTOP-IJI1KJG，同 MATRIX 双屏）

闸门：`docs/m3_matrix_pacing_gate_r2_LOCKED.md`，GATE_BODY_SHA256=`a1fa4590285f52948ecf51bc9c32629d4d2c3bc42efc7f5bb4e4922bd2be2883`，LOCKED_AT=2026-11-06 10:57:03 AWST。判据与 r1 完全相同，只改了出帧时机（垂直同步之后再等一次合成）。
代码分支：**`claude/m3-matrix`**（提交 `53cedde` 起）。**尚未 APPROVED：第 0–2 步可以先做；第 3 步必须等 `docs/m3_matrix_pacing_r2_approved.md` 落盘后才能执行。**
数据目录 `<ROOT>\artifacts_m3matrixpacing_r2`（新目录，不要和 r1 混用）。

## 0. 硬件
与 r1 完全相同：主 1280×1024 @ 75 Hz / 100%，外接 2560×1440 @ 180 Hz / 150%，扩展模式。

## 1. 取代码
```powershell
git -C <ROOT>\repo fetch origin claude/m3-matrix
git -C <ROOT>\repo checkout claude/m3-matrix
git -C <ROOT>\repo pull origin claude/m3-matrix
git -C <ROOT>\repo log -1 --format=%H
```

## 2. 构建
1. **重新构建 `m0rive.dll`**：`powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m0\build_m0.ps1 -ArtDir <ROOT>\artifacts_m3matrixpacing_r2 -DemoBinDir <exe 所在目录>`
2. 复制 `MotionSoftDemo\M3MatrixProbe.cs`（覆盖），重新构建 MotionSoftDemo。MainPage 不用动。
3. 失败记 BLOCKED_TOOLCHAIN 并回报日志；不改代码去凑。

## 3. 采数（**必须等 approved 落盘**）
```powershell
powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m3\run_m3_matrix.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <ROOT>\artifacts_m3matrixpacing_r2 -Sessions 3 -Pacing
```
确认 summary 里 `probe=M3-MATRIX-PACING v2`、`pacing_source=vblank+dwmflush`。FAIL 照常交付，不重跑凑数。

## 4. 交付
- `<ROOT>\artifacts_m3matrixpacing_r2` 下的 `m3matrixpacing_*` 文件和 `m0\out\build_m0.log` 提交到分支 `claude/m3-matrix` 的 **`m3/results_matrix_pacing_r2/`**。
- 提交前先 `git pull origin claude/m3-matrix`，只新增结果文件，不改 `docs/`。
- 回一行（按 r1 闸门 G7 模板，gate 改为 r2 的 hash）。

## 禁令
同闸门；不改 composition.h；不注入指针；不重启驱动；approved 落盘前不采数；成功 ≠ 采用。
