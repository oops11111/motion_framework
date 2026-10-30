# 派题 · M3-RIVE-FOLLOW（DESKTOP-IJI1KJG）

闸门：`docs/m3_rive_follow_gate_LOCKED.md`，GATE_BODY_SHA256=`285e39dc9c1100f92ddddc5560a3631b94129a9b56c561c9803c8cbee57eaab3`，LOCKED_AT=2026-10-30 17:40:39 AWST。P1–P6 已按建议裁决。
代码分支：**`claude/m3-follow`**（已包含 XAML-REBUILD 的代码）。**尚未 APPROVED：第 1–2 步可以先做；第 3 步必须等 `docs/m3_rive_follow_approved.md` 落盘后才能执行。**
`<ROOT>` 沿用 `C:\Users\Lh\mf_m0b`；数据目录用新的 `<ROOT>\artifacts_m3follow`。**建议先做完 XAML-REBUILD 再做本票**（两票的 MainPage 补丁有先后顺序）。

## 要做什么
测四种“跟着指针走”的做法各自的延迟：表面内由 UI 线程转交（A）、表面内由渲染线程自己读光标（B）、拆出的 visual 由合成器表达式驱动（C）、拆出的 visual 由 UI 线程写（D）。探针会自己移动鼠标（`SendInput`，P4），**运行时不要碰鼠标和键盘**，也不要让别的窗口挡住应用。

## 0. 前提
DESKTOP-IJI1KJG，75Hz，缩放 100%；不需要管理员；`mfprobe.dll`（M0 的 WGC 追踪）在 exe 旁边（M0-B r3 时已有）；本机 csproj 补丁照旧只留本机。

## 1. 取代码
```powershell
git -C <ROOT>\repo fetch origin claude/m3-follow
git -C <ROOT>\repo checkout claude/m3-follow
git -C <ROOT>\repo pull origin claude/m3-follow
git -C <ROOT>\repo log -1 --format=%H
```

## 2. 构建
1. **重新构建 `m0rive.dll`**（新增 `m0r_set_follow_cursor`）：
   ```powershell
   powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m0\build_m0.ps1 -ArtDir <ROOT>\artifacts_m3follow -DemoBinDir <MotionSoftDemo.exe 所在目录>
   ```
2. 把 `MotionSoftDemo\M0Probe.cs`（有改动）、`M3XamlLostProbe.cs`、**新文件** `M3FollowProbe.cs` 复制到工程目录。
3. MainPage：在已打过 DPI、LOST、XAML-LOST、XAML-REBUILD 补丁的工程上，`git apply --check <ROOT>\repo\m3\mainpage_m3follow_hook.patch` 通过后正式打。如果 XAML-REBUILD 补丁还没打，先打 `m3\mainpage_m3xrb_hook.patch`。打不上就照补丁手工加，并在回报里写明。
4. 重新构建 MotionSoftDemo（Debug，直接启动 exe），确认新的 `m0rive.dll` 在 exe 旁边。
5. 失败记 BLOCKED_TOOLCHAIN 并回报 `m0\out\build_m0.log` 的最后 80 行；不改代码去凑。

## 3. 采数（**必须等 approved 落盘**）
```powershell
powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m3\run_m3_follow.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <ROOT>\artifacts_m3follow -Sessions 3
```
- 每个会话约 3–4 分钟（四条路径 × 40 次阶跃 + 40 次拖动）。全程不碰鼠标键盘，屏幕保持常亮，应用窗口保持在最前、不被遮挡。
- 某个会话 `INVALID_FEW_STEPS`（检出不足）照常交付，不重跑凑数；回报里写明。

## 4. 交付
- 把 `<ROOT>\artifacts_m3follow` 下的 `m3follow_*` 文件（summary、raw、session matrix、run log）和 `m0\out\build_m0.log` 提交到分支 `claude/m3-follow` 的 `m3/results_follow/`。
- **提交前先 `git pull origin claude/m3-follow`**，只新增结果文件，不改 `docs/` 下已有文件。
- 回一行中文：
  `M3-RIVE-FOLLOW | commit=<sha> | build=<…> | sessions=<n> | verdict=<s1/s2/s3> | A/B/C/D p50_ms=<…> | c_vs_a_frames=<…> | b_vs_a_frames=<…> | seam_frames=<…> | budget_ok=<…> | composition.h PRE=POST`

## 禁令
不改 composition.h；不重开 S1/R-3/R-2；禁影子 IV；`SendInput` 只用于本票且只移动光标；approved 落盘前不采数；构建失败不改代码硬凑；成功 ≠ 采用。
