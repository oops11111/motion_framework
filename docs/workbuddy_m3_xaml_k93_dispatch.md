# 派题 · M3-XAML-K93（DESKTOP-K93N3P8，诊断票）

闸门：`docs/m3_xaml_k93_gate_LOCKED.md`，GATE_BODY_SHA256=`22fb8b92d8703619215ffaae3f0d5d9efea003951bb44b42d790d488ac28b657`，LOCKED_AT=2026-11-01 18:37:27 AWST。范围由 pidan S2 裁定。
代码分支：**`claude/m3-xaml-rebuild`**（最新提交含 S1 截图修正）。**尚未 APPROVED：第 1–3 步可以先做；第 4 步必须等 `docs/m3_xaml_k93_approved.md` 落盘后才能执行。**

## 要做什么
在 K93N3P8 上带 Rive 再跑 1 个会话，确认驱动重启是否真的让应用的图形设备丢失，以及丢失后界面文字是否还在。截图方式已改为直接向窗口取内容，**不会再截到别的窗口**；但仍请**开始前关闭聊天软件、打开“请勿打扰”**。

## 0. 前提
K93N3P8，管理员 PowerShell；不改 TDR 注册表和驱动设置；启动方式沿用上次 C 组（打包）。

## 1. 取代码
```powershell
git -C <ROOT>\repo fetch origin claude/m3-xaml-rebuild
git -C <ROOT>\repo checkout claude/m3-xaml-rebuild
git -C <ROOT>\repo pull origin claude/m3-xaml-rebuild
git -C <ROOT>\repo log -1 --format=%H
```

## 2. 构建
1. **构建带设备丢失恢复的 `m0rive.dll`**（这台机器上次只有 M0-A 时的旧版）：
   ```powershell
   powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m0\build_m0.ps1 -ArtDir <ROOT>\artifacts_m3xaml_k93 -DemoBinDir <MotionSoftDemo.exe 所在目录>
   ```
   这一步同时把 `Knight_square_2.riv` 放到 `<ROOT>\artifacts_m3xaml_k93\m0_assets\`。
2. 把 `MotionSoftDemo\M3XamlLostProbe.cs`、`M3LostProbe.cs` 复制到工程目录（覆盖），重新构建 MotionSoftDemo。MainPage 补丁上次已打，不用动。
3. 失败记 BLOCKED_TOOLCHAIN 并回报 `m0\out\build_m0.log` 的最后 80 行；不改代码去凑。

## 3. 走一遍流程（不重启驱动、不采数）
```powershell
powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m3\run_m3_xaml_lost.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <ROOT>\artifacts_m3xaml_k93_dry -Rebuild -PlainSessions 0 -RiveSessions 1 -LaunchMode Package -DemoProj <工程目录> -DryRun
```
期望 `verdict=RECORDED`、主窗口截图里能看到 Rive 画面、W_pre / W_post 截图是这两个小窗口本身（不是桌面其他部分）。数据不提交；有问题先停下回报。

## 4. 采数（**必须等 approved 落盘**）
```powershell
powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m3\run_m3_xaml_lost.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <ROOT>\artifacts_m3xaml_k93 -Rebuild -PlainSessions 0 -RiveSessions 1 -LaunchMode Package -DemoProj <工程目录>
```
- 自检 1 次 + 会话 1 次，共 2 次驱动重启，约 5 分钟。全程不碰鼠标键盘。
- **立即停止**：蓝屏；显示器 30 s 内没恢复；驱动报错弹窗；runner 打出 `STOP:`。当天不重试。

## 5. 交付
- `<ROOT>\artifacts_m3xaml_k93` 下的 `m3xrb_*` 文件（summary、raw、stderr、全部 png、session matrix、run log）和 `m0\out\build_m0.log` 提交到分支 `claude/m3-xaml-rebuild` 的 `m3/results_xaml_k93/`。
- **提交前先 `git pull origin claude/m3-xaml-rebuild`**，只新增结果文件，不改 `docs/` 下已有文件。
- 回一行中文：
  `M3-XAML-K93 | commit=<sha> | build=<…> | dryrun=<…> | verdict=<…> | rive.lost_detected=<0/1> | rive.recovered=<0/1> | 目视：主窗口/W_pre/W_post <有字/没字> | composition.h PRE=POST`

## 禁令
不改 composition.h；不重开 S1/R-3/R-2；禁影子 IV；不改 TDR 注册表；approved 落盘前不重启驱动；构建失败不改代码硬凑；诊断结果 ≠ 采用。
