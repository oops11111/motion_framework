> PresentStats 为占位值（记 N/A）；PERF-CI HOLD；Success≠adopt；不合 main；不提上游（FluentHub 只在本地侧叉，不推 fork、不提 PR）。

# Z5-T5 · FluentHub 试点接入（K93N3P8）— 结果：**PASS（编过、启动、动效可见、ETW 齐、L1 公开面 diff 为空）**

> composition.h 冻结（`code/composition.h` 归一化 sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`，本票前后不变）；L1 公开面 / fm_* / E1 不改；不改任何 *_LOCKED.md；dev-only，不采数，不计任何闸门会话；**本报告不含任何计时判据或帧率结论**。

- 派工：`workbuddy_z5_wave2_dispatch.md`（sha256 `e7deb80ce08c3c42b308852d3d42d04091aa56c97e71e83d5046971734130506`）Z5-T5；开票 pidan 2026-11-18 00:46 AWST「开 Z5-T3、Z5-T5 编码票」。
- 前置（全部满足）：T4 基线 PASS（`wb/z5-t4` `6d8c8e5c7e5afefface668e1bc53245f823a0c2d`，BASELINE.md 第 8 节，非原样 `-p:NuGetAudit=false`，SDK 10.0.26100.0 由 pidan 本人安装）；T5-0 完成（`wb/z5-t5-pack` `0a166e1822c83d1d3e8dc9f314a0f74e07d0dbb2`）；T6 事实：会话 Console、AnimationsEnabled=True（本次运行时由应用再次读取，见 4.1）；controller.h / fm_capi_test 的 Cancel/Retarget 语义已读（见第 6 节）。
- 本报告分支 `wb/z5-t5`，起点 `wb/z5-t5-pack` tip `0a166e18`；本分支只新增 `pilot/fluenthub/` 下的报告与证据，不提交 FluentHub 源码树。
- 时间：2026-11-18 01:42–02:00 AWST（Australia/Perth, UTC+8）。

## 1. 输入与标识

| 项 | 值 |
|---|---|
| FluentHub 本地侧叉 | `C:\Users\admin\pilot\FluentHub` @ `eb08f0dbcbb43880ab34038b0cb6742f0c0deca5`（未提交、未推送；改动见 `fluenthub_t5_side_fork.patch`） |
| L1 预编包（T5-0） | `C:\Users\admin\mf_z5\t5pack\pilot\l1pack\bin\x64\Release\net8.0\`；FluentHub **不编译 L1 源码**，只 `<Reference HintPath>` 引用 DLL |
| sha256 核对（引用前 = SHA256SUMS = FluentHub 输出目录内副本） | `FluidMotion.L1.dll` `2e2af911d484c3068d1a59b42a81637f153409370d9a3ecae7dd8714a0b29865`；`fluidmotion.dll` `b39d58ef612f7dd2cd89cc4373e02954fb02e93723cae7632cf55c018d1e6996`；`demo_motion.json` `8d5db5775acad5db0c04cbc3c2a214bb7309670c304b71ff40860757973c410e` — 三者 MATCH |
| 构建 | 与 T4 A1 相同命令：`dotnet build src\FluentHub\FluentHub.csproj -c Debug -p:Platform=x64 -p:NuGetAudit=false`；不做 publish/AOT |
| 运行时 | `Microsoft.WindowsAppRuntime.2_2.6.5.0_x64__8wekyb3d8bbwe`（pidan 裁决 2.6.5 替代 2.0.x；T4 第 8 节已实测加载模块） |
| 包注册 | 用户级开发者模式松散注册 `49462fluenthub-uwp.FluentHub_0.11.0.0_x64__tq8wa33sfftnm`（`IsDevelopmentMode=True`，`SignatureKind=None`）；清理：`Get-AppxPackage -Name 49462fluenthub-uwp.FluentHub \| Remove-AppxPackage` |

## 2. FluentHub 侧改动（仅本地侧叉；全文见 `fluenthub_t5_side_fork.patch`，sha256 `79b00ba22f8d45a0999a0a48e59be8b490608f29cbfdf041359e7c670b92e6ab`）

- `src/FluentHub/FluentHub.csproj`（+14 行）：pilot-only 注释；`FmL1PackDir` 指向 T5-0 输出；`<Reference Include="FluidMotion.L1" HintPath=…>`；`fluidmotion.dll`、`demo_motion.json` 以 `None CopyToOutputDirectory` 复制到输出；`<EventSourceSupport>true</EventSourceSupport>`（原因见 3.2）。
- `src/FluentHub/Views/RootView.xaml.cs`（+5 行）：字段 `_motionPilot`；`OnRootViewLoaded` 内 `_motionPilot ??= new NavMotionPilot(NavigationView);`（防 Loaded 重入）；`OnRootViewUnloaded` 内 `Dispose()` 并置 null（RootView 重建时先 Dispose 后 Create）。
- 新增 `src/FluentHub/Views/NavMotionPilot.cs`（399 行，宿主侧适配，不含 L1 源码）：
  - 页面：左侧 NavigationView 的 4 个菜单项（Inbox / My pulls / Reviews / Assigned，未登录可见），target id 1–4。
  - Loaded：`MotionTokens.Load(demo_motion.json)` → 每项 `CreateAnimation(target, "demo.enter")` + `Play()`（opacity 0→1，offsetX −80→0）。
  - PointerEntered：每项首次悬停时 `CreateAnimation(target, null)` + `Play()`（空动画），随后对该句柄 `Retarget("offsetX", 8, snappy)`；句柄复用。
  - PointerExited / PointerCanceled / PointerCaptureLost：对**同一句柄** `Retarget("offsetX", 0, snappy)`（不 Cancel、不新建句柄）。
  - Unloaded / `MainWindow.Closed`：解绑指针事件 → 对每个句柄 `Cancel()` 再 `Release()` → `MotionSystem.Dispose()`；`_disposed` 标志保证幂等。
  - 帧源：`FramePumpChanged(true/false)` 时订阅 / 退订 `CompositionTarget.Rendering`，回调内只调 `OnFrame()`；sink / 回调内不回调 API。
  - 值映射：`offsetX` → 菜单项 `TranslateTransform.X`；`opacity` → 菜单项 `Opacity`。
  - `SetReducedMotion(!UISettings.AnimationsEnabled)`，记录取值，不绕过系统设置。
  - ETW：宿主内 `PilotEtwListener : EventListener`；`OnEventSourceCreated` 若在基类构造期间触发则缓存 source，构造体内再 `EnableEvents(Verbose)`；日志缓冲上限 4096 行（溢出计 dropped）；FrameTick 只计数、不记 payload（不留计时数据）；日志写入包 LocalState `z5_t5_motion.log`。
  - snappy = `demo_motion.json` springs.snappy（response 0.32 s，damping 0.78）。

## 3. 构建与开发迭代（如实记录）

### 3.1 构建 1（01:47:18–01:48:28 AWST）
- BUILD_EXIT=0，0 警告 0 错误。输出目录含 `FluidMotion.L1.dll` / `fluidmotion.dll` / `demo_motion.json`（sha 同上）。
- GUI run1：应用启动、入场动画执行（sink 调用 136 次）、Dispose 走通；**但 ETW 0 事件**（`z5_t5_motion_run1_etw_off.log`：`ETW_SUMMARY distinctNames=0`）；合成鼠标（`SetCursorPos`）未触发悬停（hover 句柄 0）。

### 3.2 原因与改动（开发迭代，不是对验收结果的重跑凑数）
- ETW 0 事件：FluentHub 工程 `PublishAot=true`，SDK 因此在 runtimeconfig 写入 `System.Diagnostics.Tracing.EventSource.IsSupported=false`（Debug build 也写），整个 EventSource 被关闭。处理：在本地侧叉 csproj 的 pilot-only 块加 `<EventSourceSupport>true</EventSourceSupport>`，重建后 runtimeconfig 为 `true`。仅影响本地试点的运行时特性开关；未做 publish/AOT；L1 / E1 未改。
- 悬停未触发：`SetCursorPos` 不产生 WinUI 指针输入；GUI 脚本改用 `SendInput`（绝对坐标、分 5 步移动）。仅为测试驱动脚本改动，应用代码未因此改。

### 3.3 构建 2（01:51:21–01:51:45 AWST）
- 同一命令，BUILD_EXIT=0，0 警告 0 错误；runtimeconfig `EventSource.IsSupported=true`；重新 `Add-AppxPackage -Register`（同一用户级包，原地更新）。

## 4. 运行验收（GUI run2，01:51:59–01:52:20 AWST）

启动：计划任务 `schtasks /Create /TN z5_t5_run … /IT /RL LIMITED` + `/Run`，会话 1（Console）；任务内 `explorer.exe shell:AppsFolder\49462fluenthub-uwp.FluentHub_tq8wa33sfftnm!App`；脚本用 `SendInput` 移动鼠标经过菜单项，最后把光标移回原位（935,511），`CloseMainWindow()` 正常关闭（5 s 后进程已退出）；任务已删除。全程未点击、未登录、未输入凭据。

### 4.1 环境事实（运行时由应用读取）
- `UISettings.AnimationsEnabled=True` → `SetReducedMotion(False)`；会话 Console（`query session`：`>console admin 1 Active`）。→ 不触发「AnimationsEnabled=false 停下」条件。

### 4.2 动效可见（截图只截 FluentHub 当前窗口，DWM 窗口边界 1424×748）
| 文件 | 内容 |
|---|---|
| `t5_window_rest.png` | 静止（入场已结束），菜单项对齐 |
| `t5_window_hover_mypulls.png` | 光标停在 My pulls：该项（含悬停背景）整体右移（offsetX=8 DIP） |
| `t5_window_hover_reviews.png` | 光标移到 Reviews：Reviews 右移，My pulls 回到原位 |
| `t5_window_after_hover.png` | 光标移开：全部回到原位（与 rest 逐字节相同，sha `b76535b8…0887`） |
| `t5_window_hoverin_frame02.png` | 悬停过渡中的一帧 |
| `t5_menu_crop_strip.png` | 上面 rest / hover_mypulls / hover_reviews / after_hover 四张窗口截图左上菜单区的同尺寸裁切拼图（派生图，便于肉眼对比） |
- 入场动画（−80→0）在窗口首帧可见前基本完成，入场连拍未拍到明显位移；入场执行的证据为 ETW `Created/Played` 与 sink 调用计数（见 4.3）。**不据此做任何计时判断。**

### 4.3 ETW（宿主内 EventListener，`z5_t5_motion_run2.log`）
- `ETW_LISTENER enabled provider=FluidMotion-Motion guid=8f3a2c1d-6b4e-4a91-9c7d-2e5f8a1b3c4d level=Verbose`
- 事件名 **5 个**：`AnimationLifecycle=28`、`FrameTick=85`（仅计数）、`IdleTransition=8`、`Retarget=8`、`TokenLoad=1`。
- AnimationLifecycle 状态：**Created=7、Played=7、Cancelled=7、Released=7** → **Created == Released（7 = 7），无泄漏**。7 个句柄 = 入场 4 + 悬停 3（My pulls、Reviews、Assigned；Inbox 未被悬停）。
- Dispose 时帧泵已关（最后一条 IdleTransition pumpOn=0 在 Closed 之前），故 T3 新增的「Dispose 时补发 IdleTransition」本次未触发（预期）。
- `Dispose end handles=7 (enter=4, hover=3) sinkCalls=262`；日志 65 行，dropped=0。
- PresentStats：本宿主不发，记 **N/A**（占位值）。

### 4.4 L1 公开面
- 用 Z5 dev 工具 `%TEMP%\z5_wave2\tool\bin\SurfaceDump.dll`（与 T3/T5-0 同一工具）分别 dump：T5-0 包内 `FluidMotion.L1.dll` → `surface_l1pack.txt`；FluentHub 输出目录内副本 → `surface_fluenthub_output.txt`。
- `git diff --no-index --exit-code` **exit 0（diff 为空）**；两边 `CHECKSURFACE fails=0`；两文件 sha256 同为 `7b67b77bce6b36dc58cc870a17fe1b5588df3c6e526b39ca5a3e36d5280bc5cc`。
- mf 侧 `MotionSoftDemo/` 与 `code/` 在本分支无任何改动；fm_* / E1 未动。

## 5. 仲裁（入场与悬停都驱动 offsetX）
- 语义（读 controller.h 所得）：同一 (target, property) **最后写入者获胜**；Retarget 起始值优先级：本句柄自己的当前值/速度 → 另一持有者的值 → rest → from。
- 本次实测：每个菜单项首次悬停时，入场已结束，悬停句柄 Retarget 的 ETW `from=0`（= 入场留下的 rest），`hasFrom=0`；悬停中途移开时 `from` 为当时值、`vel` 非 0（如 `from=0.00537 vel=5.64`），说明同句柄打断保留值与速度。
- **未验证项**：入场进行中被悬停打断（两持有者真正重叠）本次未发生；RootView 重建（Unloaded→Loaded，先 Dispose 后 Create）本次未触发（关闭走的是 `MainWindow.Closed` → Dispose），代码路径已就位但无运行证据。

## 6. Cancel / Retarget 语义与 fm_capi_test 覆盖
- Cancel：把当前值冻结为 rest、丢弃速度 → 因此移开时不 Cancel，而是对同一句柄 Retarget 回 0。
- Retarget：`from=null`（hasFrom=0）保留当前值与速度，无影子初速度。
- `code/capi/fm_capi_test.cpp` 第 48–56 行：`fm_animation_create(s, 1, nullptr)` 后**直接** Retarget（未 Play）；**未覆盖「空动画 + Play + Retarget」**。该组合由 devsmoke（`Z3aL1DevSmoke.cs` 第 103–106 行：`CreateAnimation(1, null)` → `Play()` → `Retarget("offsetX", 240, Snappy)`）与 T3 新用例覆盖；本试点运行时亦走此路径（悬停句柄 Created→Played→Retarget）。

## 7. 验收汇总
| 项 | 结果 |
|---|---|
| 编过 | PASS（构建 2：0 警告 0 错误） |
| 启动 | PASS（未登录主界面；运行时 2.6.5） |
| 动效可见（当前窗口截图） | PASS（悬停右移 / 回位，见 4.2） |
| ETW ≥5 事件名，含 Created/Played/Cancelled/Released | PASS（5 个事件名；4 个生命周期状态齐） |
| Created == Released | PASS（7 = 7） |
| L1 公开面 diff 为空 | PASS（exit 0，fails=0） |
| 计时判据 | 无（不适用；PresentStats N/A） |

## 8. 停下请示点
- 无新的停下点（AnimationsEnabled=True；未遇登录要求、证书、UAC、安装、系统设置改动）。
- `wb/z5-t5` **仅本地提交，未 push**（未获 pidan 推送许可）。
- 偏差请评审知悉：本地侧叉 csproj 加了 `EventSourceSupport=true`（3.2），否则 PublishAot 工程里 EventSource 被整体关闭。

更正（2026-11-18）：pidan 02:02 AWST 拍板卡「wb/z5-t5 可以普通 push，推完交 Claude 和 notion 复核并结题」之后，本分支已普通 push（远端 `3809757…`）。第 4.4 节「两边 CHECKSURFACE fails=0」没有对 pack DLL 的原始输出入库，以公开面 dump 一致为准。

## 9. 本地副作用
- FluentHub 本地侧叉：2 个文件修改 + 1 个新文件（未提交；`git diff` 即 patch）；`bin/`、`obj/` 产物更新。
- 用户级包注册 `49462fluenthub-uwp.FluentHub_0.11.0.0_x64__tq8wa33sfftnm` 保留（清理命令见第 1 节）；包 LocalState 内 `z5_t5_motion.log`（run2）与 `z5_t5_motion.run1.log`。
- `%TEMP%\z5_wave2\t5\`：`build1.log`/`build2.log`、`run1\`、`run2\`（全部截图与 run.log）、apply/run 脚本、surface dump。
- 计划任务 `z5_t5_run` 已删除；GUI 运行期间合成鼠标移动（结束时光标归位）。
- git worktree `C:\Users\admin\mf_z5\t5`（分支 `wb/z5-t5`）。无安装、无证书、无系统设置改动。
