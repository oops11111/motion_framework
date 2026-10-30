# 派题 · M3-XAML-REBUILD（DESKTOP-IJI1KJG + DESKTOP-K93N3P8，诊断票）

闸门：`docs/m3_xaml_rebuild_gate_LOCKED.md`，GATE_BODY_SHA256=`3ea74c371ae983cebe480e1b4dea99e61aec92cfbe9646fb87b91b7a1d456fdc`，LOCKED_AT=2026-10-30 17:35:36 AWST。R1–R4 已按建议裁决；K93N3P8 有管理员权限，纳入 1 个会话。
代码分支：**`claude/m3-xaml-rebuild`**。**尚未 APPROVED：第 1–3 步可以先做；第 4 步必须等 `docs/m3_xaml_rebuild_approved.md` 落盘后才能执行。**

## 要做什么
驱动重启后，**重启前就开着的第二个窗口（W_pre）**和**重启后新建的窗口（W_post）**能不能画文字。运行时屏幕上会出现主窗口和下方两个小窗口（W_pre 在左下，W_post 后出现在右下），**都是正常的，不要动它们**。

## 0. 前提
- 两台机器都用**管理员** PowerShell；不改 TDR 注册表和驱动设置。
- IJI1KJG：本机 csproj 补丁（unpackaged + self-contained）照旧只留本机，用于 A 部分。
- **B 部分（打包）需要另一份不带本机 csproj 补丁的工程副本**（例如 `<ROOT>\repo_pkg`，从同一分支检出，按第 2 步加文件和补丁后构建，不加 `WindowsPackageType=None` / `WindowsAppSDKSelfContained`）。runner 会改写该工程的 `Properties\launchSettings.json`，有自己的配置请先备份。
- K93N3P8：按 M0-A 时的方式运行（当时是打包启动：`dotnet run` + MSIX 启动配置）。如果那台机器上的工程能直接启动 exe，也可以用 Exe 方式；用哪种都可以，回报里写明。

## 1. 取代码（两台机器）
```powershell
git -C <ROOT>\repo fetch origin claude/m3-xaml-rebuild
git -C <ROOT>\repo checkout claude/m3-xaml-rebuild
git -C <ROOT>\repo pull origin claude/m3-xaml-rebuild
git -C <ROOT>\repo log -1 --format=%H
```

## 2. 构建
1. 把 `MotionSoftDemo\M3XamlLostProbe.cs` 复制到工程目录（覆盖 XAML-LOST 的版本）。
2. MainPage：已经打过 DPI、LOST、XAML-LOST 补丁的工程，执行 `git apply --check <ROOT>\repo\m3\mainpage_m3xrb_hook.patch` 再正式打。**K93N3P8 上的工程如果没打过前面的补丁**，按顺序打 `m3\mainpage_m3_hook.patch`、`m3\mainpage_m3lost_hook.patch`、`m3\mainpage_m3xaml_hook.patch`、`m3\mainpage_m3xrb_hook.patch`；打不上就照补丁手工加，并在回报里写明。
3. 重新构建 MotionSoftDemo（Debug, x64）。本票普通会话**不加载** m0rive.dll，不需要 Rive 构建。
4. 失败记 BLOCKED_TOOLCHAIN 并回报日志；不改代码去凑。

## 3. 走一遍流程（不重启驱动、不采数；两台机器各一次）
```powershell
powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m3\run_m3_xaml_lost.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <ROOT>\artifacts_m3xrb_dry -Rebuild -PlainSessions 1 -RiveSessions 0 -DryRun
```
（打包方式再加 `-LaunchMode Package -DemoProj <工程目录>`。）期望 `verdict=RECORDED`，截图 20 张（base、t05 各 2 个窗口 × 2 张；t10、t30 各 3 个窗口 × 2 张），文件前缀 `m3xrbdry_`。数据不提交。

## 4. 采数（**必须等 approved 落盘**）
- **A：IJI1KJG，unpackaged（R1）**
  ```powershell
  powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m3\run_m3_xaml_lost.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <ROOT>\artifacts_m3xrb_a -Rebuild -PlainSessions 3 -RiveSessions 0
  ```
- **B：IJI1KJG，打包（R2）**——A 跑完后至少隔 120 s：
  ```powershell
  powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m3\run_m3_xaml_lost.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <ROOT>\artifacts_m3xrb_b -Rebuild -PlainSessions 1 -RiveSessions 0 -LaunchMode Package -DemoProj <repo_pkg 的工程目录>
  ```
  启动不了会自动记 `BLOCKED_PACKAGED` 并跳过，**不要去修部署问题**。IJI1KJG 当天合计 6 次重启（两次自检 + 4 次会话）。
- **C：K93N3P8（R3）**
  ```powershell
  powershell -ExecutionPolicy Bypass -File <ROOT>\repo\m3\run_m3_xaml_lost.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <ROOT>\artifacts_m3xrb_c -Rebuild -PlainSessions 1 -RiveSessions 0 [-LaunchMode Package -DemoProj <工程目录>]
  ```
- 开始前关掉其他用 GPU 的程序，保存手头工作；全程不碰鼠标键盘。
- **立即停止**：蓝屏；显示器 30 s 内没恢复；驱动报错弹窗；runner 打出 `STOP:`。当天不重试。

## 5. 交付
- A、B、C 三个数据目录下的 `m3xrb_*` 文件（summary、raw、全部 png、session matrix、run log）分别提交到分支 `claude/m3-xaml-rebuild` 的 `m3/results_xaml_rebuild/a/`、`b/`、`c/`。
- **提交前先 `git pull origin claude/m3-xaml-rebuild`**，只新增结果文件，不改 `docs/` 下已有文件。
- 回一行中文：
  `M3-XAML-REBUILD | commit=<sha> | A(IJI1KJG unpackaged)=<s1/s2/s3 verdict> | B(IJI1KJG packaged)=<verdict/BLOCKED_PACKAGED> | C(K93N3P8, <launch>)=<verdict> | 目视：W_pre <有字/没字>，W_post <有字/没字>（按 A/B/C 分别写） | composition.h PRE=POST`

## 禁令
不改 composition.h；不重开 S1/R-3/R-2；禁影子 IV；不改 TDR 注册表；approved 落盘前不重启驱动；构建失败不改代码硬凑；不为打包启动去修部署问题；诊断结果 ≠ 采用。
