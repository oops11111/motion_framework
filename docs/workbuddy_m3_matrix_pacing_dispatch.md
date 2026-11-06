# 派题 · M3-MATRIX-PACING（DESKTOP-IJI1KJG，同 MATRIX 双屏）

闸门：`docs/m3_matrix_pacing_gate_LOCKED.md`，GATE_BODY_SHA256=`2543e424cd71eea2959fe942ce0bfe7d7118ea6ee5c28b31da7902bce904812c`，LOCKED_AT=2026-11-06 09:55:00（UTC+8）。实现说明：`docs/m3_matrix_pacing_impl_claude.md`。
代码分支：**`claude/m3-matrix`**（提交 `248a2d4` 起）。**尚未 APPROVED：第 0–2 步可以先做；第 3 步必须等 `docs/m3_matrix_pacing_approved.md` 落盘后才能执行。**
`<ROOT>` 沿用 `C:\Users\Lh\mf_m0b`；数据目录 `<ROOT>\artifacts_m3matrixpacing`。

## 0. 硬件（G0：强制同拓扑）
与 MATRIX 完全相同：主显示器 1280×1024 @ 75 Hz / 100%，外接 2560×1440 @ 180 Hz / 150%，扩展模式。拓扑不同就不要采，回报 `BLOCKED_HW`。

## 1. 取代码
```powershell
git -C <ROOT>\repo fetch origin claude/m3-matrix
git -C <ROOT>\repo checkout claude/m3-matrix
git -C <ROOT>\repo pull origin claude/m3-matrix
git -C <ROOT>\repo log -1 --format=%H
```

## 2. 构建
1. **重新构建 `m0rive.dll`**（渲染节奏修复）：`powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m0\build_m0.ps1 -ArtDir <ROOT>\artifacts_m3matrixpacing -DemoBinDir <exe 所在目录>`
2. 复制 `MotionSoftDemo\M3MatrixProbe.cs`（覆盖）。MainPage：在已打过 MATRIX 补丁的工程上打 `m3\mainpage_m3matrixpacing_hook.patch`（先 `--check`）；**之前本机为 MATRIX 做的两处临时修改（runner `UseShellExecute=false`、探针 `After()`）请先还原**，仓库里的版本已经修好。打不上就照补丁手工加，并在回报里写明。
3. 重新构建 MotionSoftDemo（Debug，直接启动 exe）。失败记 BLOCKED_TOOLCHAIN 并回报日志；不改代码去凑。

## 3. 采数（**必须等 approved 落盘**）
```powershell
powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m3\run_m3_matrix.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <ROOT>\artifacts_m3matrixpacing -Sessions 3 -Pacing
```
- 每个会话约 1.5 分钟。不碰鼠标键盘；关聊天软件或开“请勿打扰”。
- FAIL 照常交付，不重跑凑数。

## 4. 交付
- `<ROOT>\artifacts_m3matrixpacing` 下的 `m3matrixpacing_*` 文件（summary、raw、png、session matrix、run log）和 `m0\out\build_m0.log` 提交到分支 `claude/m3-matrix` 的 **`m3/results_matrix_pacing/`**（不要动 `m3/results_matrix/`）。
- 提交前先 `git pull origin claude/m3-matrix`，只新增结果文件，不改 `docs/`。
- 回一行（按闸门 G7 模板）。

## 禁令
同闸门 G0/G6；不改 composition.h；不注入指针；不重启驱动；approved 落盘前不采数；成功 ≠ 采用。
