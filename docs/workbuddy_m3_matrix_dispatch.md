# 派题 · M3-MATRIX（DESKTOP-IJI1KJG + 第二块显示器）

闸门：`docs/m3_matrix_gate_LOCKED.md`，GATE_BODY_SHA256=`d8c21d0304931c42d805ceef35fccf4df50b5f2931fd916b81a13eea5db1963f`，LOCKED_AT=2026-11-02 09:34:33 AWST。开放问题 X1–X7 由 Claude 代定（pidan 授权）。
代码分支：**`claude/m3-matrix`**。**尚未 APPROVED：第 0–2 步可以先做；第 3 步必须等 `docs/m3_matrix_approved.md` 落盘后才能执行。**
`<ROOT>` 沿用 `C:\Users\Lh\mf_m0b`；数据目录 `<ROOT>\artifacts_m3mx`。

## 要做什么
窗口里的 Rive 动画一直在播，探针**自己**把窗口在两块显示器之间来回移动（拖动和直接跳转），测每块屏上的帧节奏、跨屏时有没有卡顿、换到不同缩放的屏时画面是否重新渲染清晰。**不需要动鼠标键盘，不需要管理员，不重启驱动。**

## 0. 硬件准备（X1–X3）
- 给 IJI1KJG 接**一块**外接显示器，**一共恰好两块**。主显示器保持 IJI1KJG 现在那块（75 Hz）。
- 外接显示器：刷新率 60 Hz 即可（如果有 ≥ 120 Hz 的就用它，并在回报里写明）。
- 缩放：主显示器 **100%**；外接显示器 **125%**（分辨率 ≥ 1920×1080 时用 **150%**）。“设置 → 显示”里两块屏分别设。
- 两块屏设为“扩展这些显示器”，不要“复制”。
- 没有第二块显示器：不用继续，回报 `BLOCKED_HARDWARE`。

## 1. 取代码
```powershell
git -C <ROOT>\repo fetch origin claude/m3-matrix
git -C <ROOT>\repo checkout claude/m3-matrix
git -C <ROOT>\repo pull origin claude/m3-matrix
git -C <ROOT>\repo log -1 --format=%H
```

## 2. 构建
1. 重新构建 `m0rive.dll`（新增 `m0r_get_intervals`）：
   ```powershell
   powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m0\build_m0.ps1 -ArtDir <ROOT>\artifacts_m3mx -DemoBinDir <MotionSoftDemo.exe 所在目录>
   ```
2. 复制**新文件** `MotionSoftDemo\M3MatrixProbe.cs` 到工程目录；MainPage 在已打过 FOLLOW 补丁的基础上打 `m3\mainpage_m3matrix_hook.patch`（先 `--check`）。打不上就照补丁手工加，并在回报里写明。
3. 重新构建 MotionSoftDemo（Debug，直接启动 exe）。失败记 BLOCKED_TOOLCHAIN 并回报日志；不改代码去凑。

## 3. 采数（**必须等 approved 落盘**）
```powershell
powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m3\run_m3_matrix.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <ROOT>\artifacts_m3mx -Sessions 3
```
- 每个会话约 1 分钟（5 段 × 8 s + 4 次跨屏）。全程不碰鼠标键盘，两块屏都保持常亮；关掉聊天软件或打开“请勿打扰”。
- 某会话 FAIL（例如节奏不对）照常交付，不重跑凑数——这正是本票要找的结论。

## 4. 交付
- `<ROOT>\artifacts_m3mx` 下的 `m3mx_*` 文件（summary、raw、png、session matrix、run log）和 `m0\out\build_m0.log` 提交到分支 `claude/m3-matrix` 的 `m3/results_matrix/`。
- **提交前先 `git pull origin claude/m3-matrix`**，只新增结果文件，不改 `docs/` 下已有文件。
- 回一行中文：
  `M3-MATRIX | commit=<sha> | build=<…> | 显示器=<主 Hz/缩放 + 副 Hz/缩放> | sessions=<n> | verdict=<s1/s2/s3> | pacing_ok/hitch_ok/size_ok/budget_ok=<…> | composition.h PRE=POST`

## 禁令
不改 composition.h；不重开 S1/R-3/R-2；禁影子 IV；不注入指针；不重启驱动；approved 落盘前不采数；构建失败不改代码硬凑；成功 ≠ 采用。
