# 派题 · M3-MATRIX-PACING r4（DESKTOP-IJI1KJG，同 MATRIX 双屏）

闸门：`docs/m3_matrix_pacing_gate_r4_LOCKED.md`，GATE_BODY_SHA256=`81fc74ebba270feb818646cd5f5c8c90fd2d3812c3447c642ed126413108934c`，LOCKED_AT=2026-11-06 11:27:50 AWST。判据同 r3；唯一改动：垂直同步之后再等 0.3·T 才开始画（动画时间仍按垂直同步）。
代码分支：**`claude/m3-matrix`**（提交 `69094ee` 起）。**已 APPROVED（`docs/m3_matrix_pacing_r4_approved.md`，pidan @ 2026-11-06 11:36:53 AWST）：第 0–4 步都可以执行。**
数据目录 `<ROOT>\artifacts_m3matrixpacing_r4`（新目录，不要和 r1–r3 混用）。

## 0. 硬件
与 r1–r3 完全相同：主 1280×1024 @ 75 Hz / 100%，外接 2560×1440 @ 180 Hz / 150%，扩展模式。

## 1. 取代码
```powershell
git -C <ROOT>\repo fetch origin claude/m3-matrix
git -C <ROOT>\repo checkout claude/m3-matrix
git -C <ROOT>\repo pull origin claude/m3-matrix
git -C <ROOT>\repo log -1 --format=%H
```

## 2. 构建
1. **重新构建 `m0rive.dll`**：`powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m0\build_m0.ps1 -ArtDir <ROOT>\artifacts_m3matrixpacing_r4 -DemoBinDir <exe 所在目录>`
2. 复制 `MotionSoftDemo\M3MatrixProbe.cs`（覆盖），重新构建 MotionSoftDemo。MainPage 不用动。
3. 失败记 BLOCKED_TOOLCHAIN 并回报日志；不改代码去凑。

## 3. 采数（已 APPROVED）
```powershell
powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m3\run_m3_matrix.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <ROOT>\artifacts_m3matrixpacing_r4 -Sessions 3 -Pacing
```
确认 summary 里 `probe=M3-MATRIX-PACING v4`、`pacing_source=vblank`、`pacing_offset_fraction=0.30`，以及每段的 `pacing_offset_last_ms`、`gpu_render_p95_frac_T`。FAIL 照常交付，不重跑凑数。

## 4. 交付
- `<ROOT>\artifacts_m3matrixpacing_r4` 下的 `m3matrixpacing_*` 文件和 `m0\out\build_m0.log` 提交到分支 `claude/m3-matrix` 的 **`m3/results_matrix_pacing_r4/`**。
- 提交前先 `git pull origin claude/m3-matrix`，只新增结果文件，不改 `docs/`。
- 回一行（按 r1 闸门 G7 模板，gate 改为 r4 的 hash）。

## 禁令
同闸门；不改 composition.h；不注入指针；不重启驱动；approved 落盘前不采数；成功 ≠ 采用。
