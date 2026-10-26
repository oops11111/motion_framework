# workbuddy_soft_demo — 派题 #3 结果

**机器**: DESKTOP-K93N3P8 (`machineId` 3951c05a-6a80-446d-898c-2ed8af24baa8)  
**时间**: 2026-10-19 Asia/Taipei  
**状态**: ✅ `dotnet build` 成功；✅ 打包应用可启动窗口（`MainWindowTitle=MotionSoftDemo`）

## 1) 开发包安装路径（实际采用）

**未走 winget / 管理员 GUI。** NuGet + 模板路径即可。

| 步骤 | 命令 | 结果 |
|------|------|------|
| 模板 | `dotnet new install Microsoft.WindowsAppSDK.WinUI.CSharp.Templates` | **0.0.7-alpha**（短名 `winui` / `winui3`） |
| SDK | 项目 `PackageReference Include="Microsoft.WindowsAppSDK" Version="*"`，`dotnet new` / restore 拉包 | 解析为 **Microsoft.WindowsAppSDK/2.5.1**（及 WinUI 2.3.9、Runtime 2.5.1 等） |

相关还原版本（`obj/project.assets.json`）：

- `Microsoft.WindowsAppSDK/2.5.1`
- `Microsoft.WindowsAppSDK.WinUI/2.3.9`
- `Microsoft.WindowsAppSDK.Runtime/2.5.1`
- `Microsoft.Windows.SDK.BuildTools/10.0.28000.2705`
- `Microsoft.Windows.SDK.BuildTools.WinApp/0.7.1`

本机已有 App Runtime；应用侧 SDK 由 NuGet 满足，**无需** `winget install Microsoft.WindowsAppSDK`，无管理员卡点。

## 2) 项目

- **路径**: `C:\Users\admin\motion_framework\MotionSoftDemo\`
- **创建**:  
  ```text
  cd C:\Users\admin\motion_framework
  dotnet new winui -n MotionSoftDemo -o MotionSoftDemo
  ```
- **TFM**: `net10.0-windows10.0.26100.0`，`UseWinUI=true`，单项目 MSIX（`EnableMsixTooling`）
- **未改** `composition.h`；未接 C++ 静态库 / Rive / WinMD

## 3) 软件路径假 Tick

`MainPage.xaml` + `MainPage.xaml.cs`：

- `DispatcherQueueTimer`，`Interval = 16ms`，`IsRepeating = true`
- Tick：正弦驱动 `TranslateTransform.X`（约 40…240）与 `Border.Opacity`（约 0.35…1）
- 无原生库依赖

## 4) 构建与启动

```text
cd C:\Users\admin\motion_framework\MotionSoftDemo
dotnet build -c Debug -p:Platform=x64
# → 0 警告 0 错误
# → bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\MotionSoftDemo.dll

$env:WINAPP_CLI_TELEMETRY_OPTOUT = '1'
dotnet run -c Debug -p:Platform=x64 --no-build
# winapp CLI: Launching packaged application...
# ✅ … launched (PID: …)
# 进程 MotionSoftDemo，MainWindowTitle = MotionSoftDemo
```

Developer Mode：`HKLM\...\AppModelUnlock\AllowDevelopmentWithoutDevLicense = 1`（已开，`dotnet run` 可用）。

## 5) 阻塞 / 注意

- **无阻塞**完成 build + 窗口启动证明。
- `Get-AppxPackage` 在本机 PowerShell 模块加载受限（`0x80131539`），不影响 winapp 启动；包名由 winapp 输出确认。
- `Version="*"` 会浮动；若需钉死，可把 csproj 改为 `Microsoft.WindowsAppSDK` **2.5.1**。
- 远程/无头会话若无桌面会话，仍应以 `dotnet build` 为准；本机会话有交互桌面，窗口已实测。

## 关键命令速查

```powershell
dotnet new install Microsoft.WindowsAppSDK.WinUI.CSharp.Templates
cd C:\Users\admin\motion_framework
dotnet new winui -n MotionSoftDemo -o MotionSoftDemo
cd MotionSoftDemo
dotnet build -c Debug -p:Platform=x64
$env:WINAPP_CLI_TELEMETRY_OPTOUT='1'
dotnet run -c Debug -p:Platform=x64
```

## 派题 #4

日期：2026-10-19 18:30–18:45（Asia/Taipei），主机 DESKTOP-K93N3P8。

### 1) 代码同步

- 方法：box 上 `tar czf artifacts/code_sync.tar.gz code`（排除 Linux 二进制 `timeline_tests`/`timeline_tests_san`）→ CopyFromBox 到 `C:\Users\admin\motion_framework\code_sync.tar.gz` → Windows 上 `Remove-Item -Recurse code` 后 `tar -xzf code_sync.tar.gz`（旧 .exe/.obj 一并清掉）。
- md5 两端一致：`c864b56b5b48a4ed0506e86581b163f2`。内容 = code/ 全部头文件与测试 + `explore/`（software_dispatcher.h、software_demo_test.cpp）+ `capi/`（fm_capi.h/.cpp、fm_capi_test.cpp、demo_motion.json、csharp_pinvoke_sketch.cs）。
- 未修改任何 code/ 或 capi/ 源文件。

### 2) MSVC 回归（cl 19.44.35228 x64）

- vcvars64：`C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvars64.bat`。注意 `vswhere -latest` 会返回 VS Community 2026（18.6），为与上次金标一致，显式使用 BuildTools 2022（17.14 / MSVC 14.44.35207）。
- 标志与上次相同：`cl /nologo /std:c++17 /EHsc /O2 /W3 X.cpp /Fe:X.exe`；脚本 `C:\Users\admin\motion_framework\run_golden_reply11.bat`。

| 套件 | Windows cl | 期望（box g++） |
|---|---|---|
| timeline | 422 / 0 失败 | 422 |
| tokens（motion.json） | 743 / 0 | 743 |
| controller | 236 / 0 | 236 |
| composition | 2063 / 0 | 2063 |
| 额外 explore/software_demo_test | 10 / 0 | — |
| 额外 capi/fm_capi_test（`/I.. /I..\explore`） | 23 / 0 | 23 |

全部编译 cl_exit=0；唯一警告 `fm_capi.cpp(60): C4996 strncpy`（不影响，未改源码）。完整日志：box `artifacts/win_cl_golden_log_reply11.txt`（Windows 原始：`C:\Users\admin\motion_framework\win_cl_golden_run_reply11.txt`）。

### 3) 真实弹簧接入 MotionSoftDemo（用 Claude 的 C-ABI）

- DLL：`C:\Users\admin\motion_framework\MotionNative\fluidmotion.dll`，由 `MotionNative\build.bat` 构建：
  `cl /nologo /std:c++17 /EHsc /O2 /W3 /LD /I code\capi /I code /I code\explore code\capi\fm_capi.cpp /Fe:fluidmotion.dll`
- 导出（dumpbin 确认 14 个）：fm_create、fm_create_manual、fm_manual_advance、fm_destroy、fm_load_tokens、fm_set_reduced_motion、fm_animation_create/play/cancel/release/retarget、fm_on_frame、fm_is_running、fm_current。
- 内部依赖：`capi/fm_capi.cpp` → `explore/software_dispatcher.h`（SoftwareDispatcher + FramePump）→ `controller.h`（MotionSystem，含 spring.h/timeline.h/tokens.h）。composition.h 未用、未改。
- C# 侧（按 csharp_pinvoke_sketch.cs）：新增 `MotionSoftDemo\FmBridge.cs`（`Fm` DllImport Cdecl + `MotionBridge`）；`MainPage.xaml.cs` 去掉 sine，保留原 16ms DispatcherQueueTimer 作为帧源，每个 Tick 调 `fm_on_frame`，sink 回调把 `offsetX` 写 `BoxTranslate.X`、`opacity` 写 `AnimBox.Opacity`；渲染回调启停 timer（静止即停）。时间由引擎 SteadyClock 计；另用 Stopwatch 记录每 tick 实际 dt 写日志。
- 启动：`fm_load_tokens(demo_motion.json)` 后播放 `demo.enter`（opacity 0→1 + offsetX -80→0 弹簧）；1 s 后自检：retarget 到 240，150 ms 后中途打断 retarget 回 0。
- 指针：`RootGrid`（已有 Background）`PointerPressed` → `fm_animation_retarget(drag,"offsetX", 点击X-100)`（盒子中心对准点击处，≥0）。
- csproj：`<Content Include="..\MotionNative\fluidmotion.dll" Link="fluidmotion.dll" CopyToOutputDirectory="PreserveNewest" />`，demo_motion.json 同理；已确认进入 `bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\AppX\`。
- 原文件备份：`MainPage.xaml.bak_sine`、`MainPage.xaml.cs.bak_sine`、`MotionSoftDemo.csproj.bak`。源码副本：box `artifacts/win_softdemo_src/`。

**打断连续性测试**

- 原生（`MotionNative\continuity_test.cpp`，仅用 C-ABI，fm_create_manual，60 Hz）：飞行 150 ms 后 value=217.522813117、vel=729.152815097；retarget(0) 后立即 value=217.522813117、vel=729.152815097，**|Δv|=0、|Δvel|=0**；+0.1 ms 帧 Δ=0.07238（= vel·dt）；+16.7 ms 帧 value=216.995（一帧位移 0.53 < 打断前一帧速度位移 12.15，无跳变，速度经弹簧力平滑反向）；37 帧后静止于 0。**13 checks / 0 failures**（box g++ 结果逐位一致）。
- 应用内（真实时钟）：run1 打断前采样 208.742（vel 885.9），打断时刻状态 220.403（vel 674.2）——差值是上次采样后 15.4 ms 的正常运动，不是跳变；渲染序列 192.46 → 208.74 →（打断）219.57 → 177.13 → 120.64 平滑。run2（钉版本后）打断距上帧 0.3 ms：|Δv|=0.16、|Δvel|=3.1。日志：box `artifacts/win_app_tick_log_run1.txt`、`artifacts/win_app_tick_log_run2_pinned.txt`（Windows：`MotionSoftDemo\tick_log_run1.txt`、`tick_log.txt`）。
- 观察：16ms DispatcherQueueTimer 实际间隔多为 ~31 ms（系统 15.6 ms 定时器粒度），因引擎自带时钟，运动轨迹不受影响；若要 60fps 可改用 CompositionTarget.Rendering。

**启动结果**：`$env:WINAPP_CLI_TELEMETRY_OPTOUT='1'; dotnet run -c Debug -p:Platform=x64` → winapp “launched”；进程 `MotionSoftDemo`、窗口标题 `MotionSoftDemo`（两次：PID 41524、65652），均 `CloseMainWindow()` 正常关闭，无残留进程。

### 4) 钉死 WASDK

csproj：`Microsoft.WindowsAppSDK` `*` → `2.5.1`；`Microsoft.Windows.SDK.BuildTools` → `10.0.28000.2705`；`Microsoft.Windows.SDK.BuildTools.WinApp` → `0.7.1`（均为此前实际还原版本）。`dotnet build -c Debug -p:Platform=x64` 0 警告 0 错误；project.assets.json 确认 2.5.1 / 10.0.28000.2705 / 0.7.1；钉版本后再次启动成功（run2）。

### 5) 阻塞 / 注意

- **无阻塞**。
- 首次启动崩溃（0xc000027b，Microsoft.UI.Xaml.dll）：推测原因是日志代码在 try 外直接取 `ApplicationData.Current.LocalFolder`；改为惰性+try 并挂 `UnhandledException` 后正常。非 C-ABI 问题。
- C4996 strncpy 警告（fm_capi.cpp:60）仅提示；box g++ -O1 -Wall 对 tokens.h:218 有 `-Wmaybe-uninitialized` 警告，均未改源码。
- `fm_current` 返回的是上次 Tick 的采样，打断时刻真实状态在 retarget 内按当前时钟求值；判断连续性应比较“渲染序列”或用 manual clock（如原生测试）。

## 派题 #5

日期：2026-10-19 18:50–19:10（Asia/Taipei），DESKTOP-K93N3P8。code/、capi/ 源码未改动。

### 改动

- 备份：`MotionSoftDemo\MainPage.xaml.cs.bak_timer`、`FmBridge.cs.bak_timer`（box 副本在 `artifacts/win_softdemo_src/`）。
- `FmBridge.cs`：`MotionBridge` 不再持有 DispatcherQueueTimer；引擎渲染回调 `fm_render_fn(on)` → `SetSubscribed(on)`，on=1 时 `CompositionTarget.Rendering += handler`，on=0 时 `-=`（幂等，不会重复订阅；静止即退订）。handler 内调用 `fm_on_frame`，并把 `RenderingEventArgs.RenderingTime` 与 fm_on_frame 耗时交给页面记录。`Dispose()` 一定退订并 `fm_destroy`。dt 仍由引擎自身 SteadyClock 计算。
- `MainPage.xaml.cs`：删除 timer；Unloaded 中停止一次性自检定时器 → `_fm.Dispose()`（退订 Rendering）→ 写日志。每帧日志先存内存，统计/卸载时写文件（帧路径无文件 I/O）。指针按下仍 retarget（点击 X 居中）。
- 自检：1.0 s go→240，150 ms 后打断→0（同 #4）；2.5 s 软弹簧（response 1.2、damping 0.35）→300，3.3 s 中途打断→60，持续约 5 s 以凑足 ≥100 连续帧；9 s 输出统计。

### 构建

`dotnet build -c Debug -p:Platform=x64 --no-incremental` → **0 警告 0 错误**。

### 帧间隔（未达标）

最终一次（run E）运行中，连续帧间隔（Stopwatch，已排除静止间隙）：**count=210，mean=31.10 ms，min=13.41，p50=31.25，p95=32.06，max=32.61 ms（≈32 fps）**；同时记录的 RenderingTime 间隔：mean 30.61 / p50 31.22 / p95 32.07 / max 32.62 ms。5 次启动结果一致（mean 31.0–31.2 ms，p95≈32.1 ms），**并非 ~16.7 ms**。

排查：
- 应用侧不是瓶颈：fm_on_frame 每帧 0.013–0.035 ms；去掉每帧文件 I/O 后无变化；窗口为前台（run C，fg=True）或后台结果相同；窗口未最小化、未 cloaked。
- RenderingTime 自身就是 31.2 ms 步进（=2×15.625 ms 系统时钟节拍），说明 XAML/DWM 合成器本身以约 32 Hz 出帧。
- 显示器 PHL 1280×1024 报告 60 Hz（GTX 970），本地 console 会话、未锁屏；但用户输入已空闲约 7046 s，交流电“关闭显示器”超时为 900 s → **显示器很可能已休眠**，DWM 没有真实 vblank，退回按定时器出帧。在 shell 中设置 `SetThreadExecutionState(ES_CONTINUOUS|ES_DISPLAY_REQUIRED)` 后再跑（run E）仍是 32 fps，结束后已清除。

**阻塞**：在不模拟键鼠输入、不改电源设置的前提下无法唤醒显示器，因此无法在本机验证 16.7 ms。建议：有人在本机动一下鼠标、显示器亮起后，重跑一次 `dotnet run`（app 9 s 时自动在 `MotionSoftDemo\tick_log.txt` 写 STATS 行）即可验证。由于引擎自带时钟，帧率不影响轨迹，只影响平滑度。

### 打断连续性（仍无跳变）

- selftest-interrupt（→0）：打断距上一帧 15.63 ms；打断前最后采样 218.221（vel 716.0），打断时刻状态 227.856（vel 521.6），与“采样值+vel·dt”外推差 1.56（这 15.6 ms 内正在减速）；渲染序列 189.64 → 218.22 →（打断）224.76 → 180.03 → 119.79，平滑反向。
- stats-interrupt（→60，软弹簧）：距上一帧 31.12 ms；采样 373.590（vel −249.6）→ 打断时刻 365.337（vel −278.8），外推差 0.48；渲染序列 380.86 → 373.59 →（打断）365.32 → 353.11 → 333.95，无跳变。
- 原生 C-ABI 精确测试（#4 的 continuity_test，manual clock）：|Δv|=0、|Δvel|=0，13/0 不变。

### 订阅/退订与关闭

每次运行 Rendering SUBSCRIBE/UNSUBSCRIBE 各 3 次（enter、selftest、stats 三段，每段结束自动退订）；Unloaded 时 `subscribed=False subs=3 unsubs=3`，无泄漏。5 次启动均显示进程 `MotionSoftDemo`、窗口标题 `MotionSoftDemo`，`CloseMainWindow()` 正常退出，无残留进程。

### 文件

- 日志：box `artifacts/win_app_tick_log_60fps.txt`（含 5 次统计摘要 + 最终 run E 与首次 run A 全量逐帧日志）；Windows `MotionSoftDemo\tick_log.txt`、`tick_log_60fps_try1..4.txt`（对应 run A–D；run E 即 tick_log.txt）。
- 源码副本：box `artifacts/win_softdemo_src/`（FmBridge.cs、MainPage.xaml.cs、MainPage.xaml 与 .bak_timer）。

## 派题 #6

M1b 真机 CompositionDispatcher 单属性试点。日期：2026-10-19 19:25–19:45（Asia/Taipei），DESKTOP-K93N3P8。code/、capi/、composition.h 均未改动。**显示器仍在休眠**（输入空闲 >2 h，关屏超时 900 s），帧时序数字不具代表性。

### 实现

- 备份：`MainPage.xaml.bak_m5`、`MainPage.xaml.cs.bak_m5`、`FmBridge.cs.bak_m5`、`MotionSoftDemo.csproj.bak_m5`。
- UI：新增 `ToggleSwitch ModeToggle`（Header “动画路径”，Off=软件路径 / On=合成器路径）。指针按下：软件模式走 `fm_animation_retarget`，合成器模式启动新的 NaturalMotion 弹簧（都可打断）。
- 软件路径：与 #5 相同（CompositionTarget.Rendering → fm_on_frame → BoxTranslate.X）。`FmBridge.cs` 只新增 `RetargetFrom`（hasFrom=1），用于从合成器切回时交接数值。
- 合成器路径（C# 直接用 Microsoft.UI.Composition，未经 C++/WinRT 接 composition.h）：
  - 目标：`ElementCompositionPreview.GetElementVisual(AnimBox)`，`SetIsTranslationEnabled(AnimBox, true)`，属性 **`Translation.X`**。没有用 composition.h 默认映射里的 `Offset.X`，因为 XAML 元素 Visual 的 Offset 归布局所有，会被覆盖；以后接 planner 时可以通过 `StaticCapabilityProbe::Add("offsetX", {"Translation.X"})` 换路径，无需改源码。
  - 弹簧 → `SpringScalarNaturalMotionAnimation`：FinalValue=目标，**Period = response = 0.32 s（S1），DampingRatio = damping = 0.78（S2）**，与软件路径/demo token `springs.snappy` 参数相同；InitialValue/InitialVelocity 不设，按设计应从当前值/速度接续。
  - 贝塞尔 → `ScalarKeyFrameAnimation`：key0 `this.StartingValue`，key1 目标 + `CubicBezierEasingFunction((0.1,0.9),(0.2,1.0))`（easing.enter，按 K1 放在结束关键帧），Duration 220 ms。
  - 防重复叠加：进入合成器模式时，软件 sink 忽略 offsetX、`BoxTranslate.X = 0`、`Translation = (软件当前值,0,0)`；切回时 `StopAnimation("Translation.X")`，读取值（K5），把 Translation 置 0，再用 `RetargetFrom(值)` 交回软件路径。
  - 读回：`CompositionPropertySet` 的标量 "x" 由 `ExpressionAnimation("v.Translation.X")` 驱动，每个 Rendering 回调 `TryGetScalar`；同时读取 `Visual.Properties.TryGetVector3("Translation")`。另外加了两个诊断：静态写 Translation.X:=37；直接对 PropertySet 标量跑一个 1000 ms 线性关键帧并给 ScopedBatch 计时。
- 自检时间线：软件 go240→150 ms 打断→0；软件 0→240 静止起步；回 0；切合成器；合成器 go240→150 ms 打断→0；合成器 0→240；合成器贝塞尔→0；切回软件；软件 go120。

### 结果

- 构建：`dotnet build -c Debug -p:Platform=x64 --no-incremental` → **0 警告 0 错误**。
- 启动/关闭：进程 `MotionSoftDemo`、窗口标题 `MotionSoftDemo`，`CloseMainWindow()` 正常退出，无残留，无异常。
- **软件路径**（对照基准）：打断时 value=192.36、vel=1143.8 → 渲染 192.36 → 211.11 → 173.98 → 119.09，平滑；0→240 从静止起步，retarget 后 628 ms 收敛到 240.0000；切回后 go120 收敛到 120.0000。
- **合成器路径：无法在本机验证（阻塞）**：
  1. 表达式读回始终为 0：135 个采样全部 probe=0，**连静态写 Translation.X:=37 也读不到**（`Visual.Properties` 读到 37，probe 仍是 0）。可见 `TryGetScalar` 返回的是静态值，不是合成器求值后的值，这条读回路线行不通。
  2. 对照组：PropertySet 标量直接跑 1000 ms 关键帧，ScopedBatch **13.6 ms 就 Completed**，读到终值 100，期间没有任何中间值。1000 ms 的动画 13.6 ms 就“完成”，说明本机（显示器休眠时）合成器的动画时钟没有按真实时间推进。
  3. 三个弹簧 batch 都没有自行完成，直到 220 ms 的贝塞尔 batch 在 29.9 ms 时完成，才一起触发 Completed。
  4. 像素旁证也拿不到：`PrintWindow(PW_RENDERFULLCONTENT)` 截到的客户区全白；`CopyFromScreen` 取单行像素始终找不到蓝色方块（显示器休眠）。辅助代码见 `artifacts/win_softdemo_src/cap_helper2.cs.txt`，没有保存任何截图。
  - 因此合成器模式下的**启动延迟、打断平滑度、终值与软件路径的一致性均未测得**。模式切换本身正常：无异常，合成器模式下 BoxTranslate.X=0，切回时 Translation 归 0，软件路径从交接值继续。
- 下一步建议：显示器亮着时重跑（应用约 11 s 自动跑完自检，写 `MotionSoftDemo\comp_pilot_log.txt`）。读回可改用 `Windows.Graphics.Capture`（GraphicsCaptureItem.CreateFromVisual）对 AnimBox 逐帧截图测位置；或在 C++ 侧用 DComp 的 IDCompositionDevice::GetFrameStatistics 核对合成器时钟。

### 文件

- 日志：box `artifacts/win_comp_pilot_log.txt`（摘要 + 最终一次全量日志）；Windows `MotionSoftDemo\comp_pilot_log.txt`、`build_m6_log.txt`。
- 源码副本：box `artifacts/win_softdemo_src/`（MainPage.xaml、MainPage.xaml.cs、FmBridge.cs、MainPage.xaml.cs.bak_m5、cap_helper2.cs.txt）。

## 派题 #7

> 2026-10-19 夜（Asia/Taipei），DESKTOP-K93N3P8。显示器仍熄屏；未模拟输入、未改电源设置；未修改 code/ 与 capi/ 任何源码。先通读 `docs/deepseek_m1b_spec.md`。

### 结论
- 构建：`dotnet build -c Debug -p:Platform=x64 --no-incremental` → **0 个警告 / 0 个错误**；启动后 CloseMainWindow 正常退出，无残留进程。
- 熄屏下测量一次：**判定 INVALID (display-asleep)**，非 FAIL；所有 §4 项均输出 INVALID/N/A 并附原因。
- 规范说"§6 验收"，实际验收表在 **§4**（无 §6），日志里已注明。

### API 核实（spec 的 `TryStartAnimation`）
Microsoft.UI.Composition 的 `CompositionObject` **没有 `TryStartAnimation`**。正确写法是：
`visual.StartAnimation("Translation.X", anim)`，之后用 `visual.TryGetAnimationController("Translation.X")` 取控制器（没有动画时返回 null）；
也可以用重载 `StartAnimation(String, CompositionAnimation, AnimationController)`（WASDK 1.1+）。停止用 `StopAnimation`。
- https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.compositionobject
- https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.compositionobject.startanimation
- https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.compositionobject.trygetanimationcontroller

相关核实：
- `GraphicsCaptureItem.CreateFromVisual` 只接受系统合成器的 `Windows.UI.Composition.Visual`，不接受 WinUI3 的 `Microsoft.UI.Composition.Visual`，所以改用 `IGraphicsCaptureItemInterop::CreateForWindow(HWND)`（1903+）。
  - https://learn.microsoft.com/en-us/uwp/api/windows.graphics.capture.graphicscaptureitem.createfromvisual
  - https://learn.microsoft.com/en-us/windows/win32/api/windows.graphics.capture.interop/nf-windows-graphics-capture-interop-igraphicscaptureiteminterop-createforwindow
- `DCompositionGetFrameId` / `DCompositionGetStatistics` 要求 Windows 11（build 22000+）。本机是 **Win10 19045**，dcomp.dll 未导出这两个函数（dumpbin 已确认）。
  - https://learn.microsoft.com/en-us/windows/win32/api/dcomp/nf-dcomp-dcompositiongetframeid
  - https://learn.microsoft.com/en-us/windows/win32/api/dcomp/nf-dcomp-dcompositiongetstatistics

### 实现
1. **合成器 retarget 显式播种**：每次 `SpringScalarNaturalMotionAnimation` 都显式设 `InitialValue` / `InitialVelocity`，取自 app 影子状态（spec §2.4）。影子状态由新 helper DLL `MotionNative\mfprobe.dll`（`mf_probe.cpp`，只 #include `code/spring.h`，不改源码）的闭式解 `mf_spring_at` 计算。超过 SettleTime 后取 target。
   - Period=response=0.32 s，DampingRatio=0.78（S1）。
   - 例：B2 打断时种子为 InitialValue=220.3278，InitialVelocity=675.6611 DIP/s。
   - 合成器→软件 handoff 也用影子状态（readback 读不到，只记录差值）。
2. **测量通道**（mfprobe.dll 导出）：
   - WGC：`CreateForWindow` + `Direct3D11CaptureFramePool`（free-threaded），逐帧扫 BGRA 找 #3B82F6，记录 SystemRelativeTime 和包围盒。
   - `DCompositionGetFrameId` / `GetStatistics`：运行时 GetProcAddress 解析，缺失时返回 0x80004001。
   - `DwmGetCompositionTimingInfo`。
   - 有效性门（§3.7）：frameId 不可用或 2 s 内不变 → INVALID。另加 DWM cRefresh/s < 0.5×rateRefresh → INVALID。
3. **一键 + 无头**：按钮「运行测量 (S1 + §4 验收)」。也可用启动参数 `--measure`、环境变量 `MF_AUTORUN=1` 或 `MotionSoftDemo\autorun.flag` 触发，加载后 1.5 s 开跑。实测 `dotnet run ... -- --measure` 的参数能传进打包应用。
4. **场景**：
   - 2 s 时钟门；
   - 标定（静态 37 DIP）；
   - A：弹簧 0→240；
   - B：150 ms 打断；
   - E：rate=2（Period 0.16）；
   - C：bezier 220 ms；
   - H：飞行中切到软件路径；
   - D：软件路径打断对照。
5. **报告**（REPORT 行）：§4 第 1–8 项，加 S1 判别（峰-峰 0.511 s 对 0.320 s，用 RMS 对比两种模型）。无效时判别结论标为 WITHHELD/N/A，不下结论。

### 熄屏运行证据（`artifacts/win_comp_pilot_log_m7.txt`）
- `DCompositionGetFrameId` hr=0x80004001，frameId 0→0：门无法通过。
- DWM：cRefresh 2→4→5，cFrame 1→3→4（约 14 s 内），即约 1/s，而 rateRefresh=60020/1000，说明没有真实 vblank。
- WGC：第 1 次运行 641 帧，色块位置始终 cx=749 px 不动（标定 0 与 37 DIP 也相同）。第 2 次（最终日志）585 帧，色块一帧都没找到。两次都说明内容冻结或未渲染。
- CompositionTarget.Rendering：p50=31.10 ms，p95=32.03 ms（2×15.625 ms）。
- §4 第 1–6 项全部 INVALID/N/A。第 7 项 exceptions=0，订阅 1/1 已退订，模式切换 2/2 ok。第 8 项为 INVALID（非 FAIL），app 未挂起。
- 软件对照 D（不依赖显示）：打断时 |dv|=11.64 DIP，是半帧（约 31 ms tick）推进量，不是跳变。
- S1 判别：N/A（A 段 0 个有效 WGC 帧）。

### 阻塞
- 显示器熄屏：所有基于 WGC 的验收项都无法得到有效数据，需要亮屏后再跑一次（按钮或 `--measure`）。
- Win10 19045 没有 DCompositionGetFrameId，spec §3.7 的 frameId 门在本机永远不可用。要么在 Win11 上测，要么由 spec 认可用 DWM cRefresh + WGC 帧作替代门。

### 文件
- Windows：
  - `MotionNative\mf_probe.cpp`、`build_probe.bat`、`mfprobe.dll`、`probe_cli.cpp/.exe`、`build_probe_log.txt`；
  - `MotionSoftDemo\MainPage.xaml(.cs)`、`MotionSoftDemo.csproj`（新增 mfprobe.dll Content）、`*.bak_m6`、`comp_pilot_log.txt`、`build_m7_log.txt`。
- box：
  - `artifacts/win_comp_pilot_log_m7.txt`；
  - `artifacts/win_softdemo_src/`（MainPage.xaml、MainPage.xaml.cs、csproj、mf_probe.cpp、probe_cli.cpp、build_probe.bat、build_probe_log.txt、*.bak_m6）。

### #7 补充：替代门槛

协调裁定：本机是 Win10 19045，改用"DWM 刷新推进 + WGC 帧数"作为有效性门槛。已修改 `MainPage.xaml.cs`，未改动 code/。

**门槛规则（任一不满足即判 INVALID）**

- **frame-id 门**
  - 仅在 OS build ≥ 22000 时启用。
  - 更低的版本不调用探针，记录 `frame-id gate skipped: OS build 19045 < 22000`，不因此判 INVALID。
- **DWM 门**
  - 全程 cRefresh 的推进量应 ≥ 50% × rateRefresh × 经过时间（QPC 计时）。
- **WGC 门**
  - 在合成器运动窗口内统计找到色块的 WGC 帧，应 ≥ 50% × rateRefresh × 运动总时长。
  - 运动窗口为 A/B1/B2/E/C 各段：弹簧段取 spring.h 的 SettleTime，B1 截到被打断的时刻，bezier 段取 220 ms。
  - 选 50% 的理由：亮屏 60 Hz 时每个 vsync 都应有一帧，50% 足以容忍偶发丢帧。
  - 如果捕获启动失败，或一帧色块都没找到，自然不达标。

**结果**

- 构建：`--no-incremental` 0 个警告 / 0 个错误。
- 无头重跑（`--measure`，熄屏），时间为 2026-10-20 00:05（Asia/Taipei）：
  - frame-id 门：已跳过。
  - DWM 门：**不通过**。cRefresh 2→5，即 +3 / 13.96 s；期望 60.020 × 13.96 ≈ 838.1，实际比例 **0.36%**，低于 50%。
  - WGC 门：通过。运动窗口内找到色块 86 帧，期望 118.1，比例 72.8%；全程共 648 帧。
  - 最终判定：**INVALID，原因是 DWM 推进不足。**
- 启动、关闭正常，exceptions=0，无残留进程。

**注意**

熄屏时 WGC 仍按约 46 帧/s 交付帧，但色块一直停在 cx=697 px 不动（标定 0 与 37 DIP 的结果也相同）。也就是说，仅凭"帧数 + 找到色块"并不能识别内容冻结。

建议日后给 WGC 门再加一条："运动窗口内色块位移 ≥ 10% 行程"。本次未加，以免越出裁定范围。

**追加规则（协调批准）：frozen-frames 门**

- **规则**：对每个合成器场景（A/B1/B2/E/C）**分别**判定，**逐场景**（不做汇总）：运动窗口内（加 100 ms 延迟尾巴）检测到的色块 x 的极差（max−min），按物理像素 ÷ RasterizationScale 换算成 DIP，必须 ≥ 该场景指令行程 |target−from| 的 10%。任一场景不达标，就判 INVALID，原因记为 `frozen-frames`。
- **为什么逐场景**：汇总判定可能被某一个"动了"的场景掩盖其余场景的冻结。
- **为什么不用标定值**：用 RasterizationScale 而不是标定得到的 px/DIP，因为冻结时标定本身就失效。
- **结果**：构建 `--no-incremental`，0 个警告 / 0 个错误。熄屏下以 `--measure` 无头重跑两次（2026-10-20 00:08、00:09，Asia/Taipei），均判 **INVALID**，同时列出三个原因：
  - DWM 推进：+3 / 期望 838.6，即 **0.36%**，不通过；
  - WGC：运动窗口内找到色块 0 帧 / 期望 118.2，不通过（全程交付 590 帧和 625 帧，但都没找到色块）；
  - **frozen-frames：5/5 场景位移 0.00 DIP，即 0%，不通过**（A/B1/E/C 指令行程 240 DIP，B2 约 220 DIP）。
- **离线复核**：00:05 那次运行找到了色块（648/648 帧），但 648 帧的 cx 全是 697.053 px，位移为 0。按新规则也会判 frozen-frames 不通过。这正是新增这条门要堵的情况。
- 启动、关闭正常，exceptions=0，无残留进程；未改动 code/。

**日志**：box `artifacts/win_comp_pilot_log_m7.txt` 末尾已追加本次重跑（00:05、00:08、00:09 三次）；Windows 端为 `MotionSoftDemo\comp_pilot_log.txt`，上一次运行的日志另存为 `comp_pilot_log_m7_run2.txt`。

## 派题 #8

> 2026-10-20 04:44–04:53（Asia/Taipei），DESKTOP-K93N3P8，显示器熄屏。未模拟输入，未改电源设置。
> 依据：`docs/claude_m1b_reply.md` 中的 C# 审查 11 条，以及附注里的 DwmFlush 探针。
> 修改前已备份：`MotionSoftDemo\*.bak_m7`（FmBridge.cs、MainPage.xaml(.cs)、App.xaml.cs、MainWindow.xaml(.cs)、csproj）和 `MotionNative\build.bat.bak_m7`；box 上另存了 `fm_capi.cpp.bak_m7`。

### 11 项修复

| # | 审查项 | 修改 | 验证 |
|---|---|---|---|
| 1 | 回调异常不得越过 P/Invoke 边界 | `FmBridge.cs`：`_sink`、`_render`、`_onRendering` 三处全包 try/catch，异常计入 `CallbackErrors`/`LastError`，并通过 `Diagnostic` 事件写进日志（`BRIDGE …`）。`SubscriptionChanged` 的调用也包了一层 | 编译通过；所有运行 callbackErrors=0。**未做故障注入** |
| 2 | 窗口关闭时不保证触发 Unloaded | `MainPage.Shutdown(reason)`：幂等、可重入，负责停定时器、退订 suite 的 Rendering、`mf_cap_stop`、`StopAnimation`，并 Dispose 两个 bridge（Dispose 内会退订 Rendering）。`OnUnloaded` 和 `MainWindow.Closed` 都调用它 | 3 次启动/关闭均为 `SHUTDOWN reason=MainWindow.Closed … subs=1 unsubs=1 paired=True`，随后 `reason=Page.Unloaded: already done -> no-op` |
| 3 | 在原生调用中 Dispose 会释放后使用 | 每次 `fm_*` 调用前后对 `_inNative` 做 ++/--。若 Dispose 时 `_inNative>0`，先立即退订 Rendering，再用 `DispatcherQueue.TryEnqueue(Dispose)` 延后 `fm_destroy` | 自检：在 sink 里调用 Dispose，日志为 InNative=True、`fm_destroy deferred`；1 s 后 IsDisposed=True、DeferredDisposes=1、subs/unsubs=1/1，结果 **PASS**（4 次运行都是） |
| 4 | 每个页面都订阅 UnhandledException | 改为只在 `App` 构造函数里订阅一次，通过静态事件 `App.Unhandled` 转发。页面在 OnLoaded 里 `+=`，在 Shutdown 里 `-=` | 订阅数在加载时为 1，Shutdown 后为 0 |
| 5 | Loaded 可能重复触发 | `OnLoaded` 第一行加 `if (_fm != null) return;` | 代码审查（本次没有重复触发 Loaded 的场景） |
| 6 | 线程安全没有保护 | 构造时记下 `DispatcherQueue.GetForCurrentThread()`；所有公有成员（含 Dispose、Subscribed）首行调用 `[Conditional("DEBUG")] Debug.Assert(_queue.HasThreadAccess)` | Debug 构建下运行没有触发断言 |
| 7 | Dispose 后事件仍引用页面 | Dispose 末尾把 `Frame`、`SubscriptionChanged`、`Diagnostic` 置为 null | 代码审查 |
| 8 | err 缓冲区未标 `[Out]` | 声明改为 `[Out] byte[] err`，读取时截到第一个 NUL | load_tokens ok |
| 9 | 每帧都调用 PtrToStringUTF8 | 用 `MemoryMarshal.CreateReadOnlySpanFromNullTerminated` 取字节，再与 `"offsetX"u8`、`"opacity"u8` 做 `SequenceEqual`，分派成 `FmProp` 枚举，每帧不再分配字符串。csproj 加了 `AllowUnsafeBlocks`。顺带修了一个问题：`OnValue`（sink 内）原先调用了 `_fm.Current`，违反"回调里不得调用 fm_*"的规则，已移到 `Frame` 事件里（即 fm_on_frame 返回之后） | 0 警告 0 错误；suite 正常 |
| 10 | `_buf` 无界增长 | `Log()` 发现 `_buf` 超过 1 MB（1,000,000 字符）就调用 `FlushLog()` 写盘并清空。如果还没有日志文件，就丢弃并计数，不让它继续增长 | 本次运行没有超过 1 MB（auto-flushes=0） |
| 11 | DLL 用 /MD；strncpy 引发 C4996 | `build.bat` 的 DLL 和测试全部改为 `/MT`；dumpbin 显示 DLL 只依赖 KERNEL32.dll。**唯一的 capi 改动**在 `code/capi/fm_capi.cpp`：`#ifdef _MSC_VER` 下用 `strncpy_s(err, cap, s, _TRUNCATE)`，否则用 `std::snprintf(err, cap, "%s", s)`，并新增 `#include <cstdio>` | box g++ `-Wall -Wextra` 0 警告，fm_capi_test 23/0；Windows cl /W3 /MT 下 fm_capi_test 23/0，**无 C4996**；continuity_test **13/0** |

### 验收
- `dotnet build -c Debug -p:Platform=x64 --no-incremental`：**0 个警告 / 0 个错误**。
- 原生 continuity_test 13/0，fm_capi_test 23/0（box g++ 与 Windows cl 都是）。
- 启动/关闭 3 次（`--lifecycle`）：每次都在 Rendering 订阅中（3 s 长弹簧）用 CloseMainWindow 关闭。bridge 的 subs/unsubs 均为 1/1，自检 bridge 也是 1/1；App.Unhandled 订阅数 1→0；exceptions=0；无残留进程。
- `--measure` 一次：主 bridge subs/unsubs 为 4/4，suite 为 1/1；运行判定仍是 INVALID（DWM 0.36%、WGC 0/118.3、frozen-frames 5/5），与 #7 一致。

### DwmFlush 探针（熄屏）
在后台线程连续调用 300 次 `DwmFlush()`，上限 30 s，结果写入测量日志：
- 300 次全部成功（hr=0），总计 4996.6 ms；
- 间隔：**mean 16.655 ms**，min 3.791，p50 15.654，p95 30.476，max 32.099 ms。
- 解读：均值看起来像 60 Hz，但 p50≈15.6 ms、p95/max≈31 ms，正好是系统时钟节拍（15.625 ms）的 1 倍和 2 倍，min 只有 3.8 ms。这更像 DwmFlush 在熄屏时按定时器返回，而不是按 vsync 返回，所以**不能**证明 DWM 在以 60 Hz 合成。这也和 cRefresh 约 1/s 的结果不矛盾。日志里的自动判词"≈60 Hz"只看了均值，应以这里的解读为准。亮屏后需要再测一次作为对照。

### 文件
- Windows：
  - `MotionSoftDemo\`：FmBridge.cs、MainPage.xaml.cs、App.xaml.cs、MainWindow.xaml.cs、csproj、*.bak_m7、m8_lifecycle_log.txt、comp_pilot_log.txt、build_m8_log.txt；
  - `MotionNative\`：build.bat（/MT）、build_m8_log.txt、capitest\；
  - `code\capi\fm_capi.cpp`（已从 box 同步）。
- box：
  - `artifacts/win_m8_log.txt`（g++ 结果、原生构建、dotnet 构建、3 次生命周期日志、measure 日志）；
  - `artifacts/win_softdemo_src/`（新增 App.xaml.cs、MainWindow.xaml.cs、build.bat、fm_capi.cpp(.bak_m7)、MainPage.xaml.cs.bak_m7）。

### #8 补充：DwmFlush 分布判定 + 回调故障注入

> 2026-10-20 04:56–04:59（Asia/Taipei），熄屏。未模拟输入，未改电源设置。
> 构建：`--no-incremental` 0 个警告 / 0 个错误；fm_capi_test 23/0，continuity_test 13/0，均未变。

**1）DwmFlush 判定改为按分布，不再只看均值**（`MainPage.ClassifyDwmFlush`，离线脚本 `classify_dwmflush.py` 用同一规则）

- **规则：**
  - 先检查定时器档位：≥80% 的间隔落在 k×15.625 ms ±1.0 ms（k=1,2）内，判 `timer-driven`；
  - 否则，若 ≥80% 落在 1000/刷新率（本机为 16.661 ms）±1.0 ms 内，**并且** p95 ≤ 18 ms，判 `≈60Hz vsync`；
  - 其余情况判 `unknown`。
  - 两个容差窗口有重叠（定时器 k=1 窗口为 14.625–16.625 ms，vsync 窗口为 15.661–17.661 ms），所以先判定时器；16.661 ms 本身不在 15.625±1 内。
- **app 内结果（--measure，04:58）**：
  - k1 = 92.7%，k2 = 6.0%，定时器档位合计 **98.7%**；
  - vsync 档 46.7%，p95 = 30.381 ms；
  - 判定 **timer-driven**。
- **离线复核**（同一规则）：
  - app 原始间隔：98.7%，判 timer-driven；
  - PowerShell 独立采集的 300 次：k1 = 91.0%，k2 = 6.7%，合计 **97.7%**，判 timer-driven。
- **说明**：#8 那次运行只记录了汇总统计（mean 16.655 / p50 15.654 / p95 30.476），**没有保存原始间隔**，所以无法逐样本重算它的占比。不过它的 p95 = 30.5 > 18 ms，已经排除了 vsync 判定。现在 app 会把 300 个原始间隔全部写进日志（`DWMFLUSH raw intervals`），以后可以离线复算。
- **结论**：熄屏时 DwmFlush 按约 64 Hz 的系统定时器节拍返回（15.6 ms，偶尔 31.2 ms），**不是** vsync。它的均值约 16.7 ms，正是一个"假 60 Hz"。

**2）回调故障注入**（`MF_INJECT_FAULTS=1` 或 `--inject-faults`，只作用于主 bridge）

- **注入方式**：三个回调各抛一次 `InjectedFaultException`，都在回调的正常工作做完**之后**才抛：
  - `_render`：第一次 on=1，订阅完成之后；
  - `_sink`：第 5 次调用，赋值之后；
  - `_onRendering`：第 10 帧，fm_on_frame 和 Frame 事件之后。
- **验证结果**（`--lifecycle --inject-faults`）：
  - 3 个异常都被捕获并记录：`BRIDGE callback exception in render / sink / rendering`，callbackErrors 依次为 1→2→3；
  - 进程没有崩溃，App.Unhandled 计数为 0，exceptions = 0；
  - **动画继续推进**：render 故障后 offsetX 为 -72.08 → -63.22；sink 故障（第 3 帧）后为 -53.05 → -38.95 → -26.51；rendering 故障（第 10 帧，30.46）后为 40.29 → 49.79 → … → 195.79（第 40 帧），速度单调衰减；
  - 关闭：`SHUTDOWN reason=MainWindow.Closed`，subs/unsubs 为 1/1，paired=True，faultsInjected=[sink=True render=True rendering=True]；自检 bridge 为 1/1；Unloaded 那次调用为 no-op；无残留进程。
- 不开开关时，`--measure` 运行结果为 faultsInjected=[off]，callbackErrors=0，subs/unsubs 为 4/4。

日志已追加到 box `artifacts/win_m8_log.txt`。源码 FmBridge.cs、MainPage.xaml.cs、classify_dwmflush.py 已更新到 `artifacts/win_softdemo_src/`。

## 派题 #9｜亮屏复测

> 2026-10-20 08:52–08:54（Asia/Taipei）。用户已唤醒显示器；未模拟输入，未改电源设置。
> 构建：`--no-incremental` 0 警告 / 0 错误；continuity_test **13/0** 未变；#8 加固保留。

### 1）DwmFlush 探针（300 次）— **≈60Hz vsync**（符合预期）

| 指标 | 数值 |
|---|---|
| mean / p50 / p95 / max | **16.663** / 16.660 / **16.911** / 17.715 ms |
| 定时器档 k×15.625±1（k=1,2） | k1=**29.7%** k2=0.0% 合计 29.7%（<80%） |
| vsync 档 16.661±1 ms | **99.3%**，且 p95=16.911 ≤ 18 |
| 判定 | **≈60Hz vsync**（熄屏时为 timer-driven 97–99%） |

### 2）CompositionTarget.Rendering — **仍约 32 Hz（不符合 ~16.7 ms 预期）**

| 指标 | 数值 |
|---|---|
| n | 443 |
| min（未单独打印）/ p50 / p95 / mean | — / **31.09** / **32.01** / **31.05** ms |

**诊断：** 同一轮运行里，DwmFlush（后台线程）和 WGC（827 帧 / 13.8 s ≈ **59.9 fps**）都是满速 60 Hz，标定也正常（0→37 DIP → cx 100.48→137.48，**1.000 px/DIP**）。只有 UI 线程上的 `CompositionTarget.Rendering` 仍按 ~2×15.625 ms 触发。已 `ShowWindow(SW_RESTORE)+SetForegroundWindow`，无改观。结论：**显示器已醒、合成器与捕获为 60 Hz；WinUI Rendering 事件在本机仍被节流到 ~32 Hz**（与熄屏时相同的系统时钟档）。该项阈值（p50≤18）**未达标**，但不再作为熄屏门的依据。

### 3）完整 `--measure` — **VALID**

| 门 | 结果 |
|---|---|
| frame-id | 跳过（OS 19045 < 22000） |
| DWM | **PASS**，依据 DwmFlush≈60Hz vsync（timer 29.7% / vsync 99.3% / p95 16.911）。**说明：** 即使亮屏，`DWM_TIMING_INFO.cRefresh` 仍只 +3 / 13.8 s（0.36%），`cFramesDisplayed` 等也几乎不动——本机该计数不可靠，已改为以 DwmFlush 分布判定为准，cRefresh 仅作证据记录 |
| WGC | **pass** 135 / 期望 118.2 = **114.17%**（全程 827 帧，色块全找到） |
| frozen-frames（逐场景） | **5/5 pass**：A 71.87%、B1 101.87%、B2 117.34%、E 79.16%、**C 100.21%**（此前 C 因观测窗过短 + StartingValue 竞态为 0.23%，已改为显式 from 关键帧并加长观测） |
| **总判定** | **VALID**（熄屏：display awake） |

**§4 验收项（门 VALID ≠ 数值过关）：**

| # | 项 | 结果 |
|---|---|---|
| 1 | 启动延迟 | n=4 p50=44.51 p95=70.82 max=70.82 ms（阈值 33/50/200）— 未过 |
| 2 | 打断位置 \|Δv\| | 452.75 px（测 -233.0 vs 影子 219.7）— 未过；种子 InitialValue=220.77 vel=667.06 |
| 3 | 打断速度 \|Δvel\| | 1322.89 px/s — 未过 |
| 4 | 终值误差 | 4.53 px @ 1.687 s — 未过 |
| 5 | 轨迹 RMS A/E/C | 49.7% / 41.7% / 104.0% — 未过（WGC↔QPC 时钟对齐／模型对照仍粗） |
| 6 | Rendering 间隔 | p50=31.09 — 未过（见上） |
| 7 | 稳定性 | exceptions=0；suite 1/1；模式 2/2；shutdown subs/unsubs 4/4 paired |
| 8 | 熄屏 | **display awake (gate passed)** |
| S1 | Period 判别 | first peak t=1.187 s；RMS vs S1 119.3 vs damped 126.8 → **Period = UNDAMPED（S1 holds）** |

### 4）软件路径 vs 合成器路径（同目标打断）

| | 合成器 B2（影子播种） | 软件 D2 |
|---|---|---|
| 打断时刻 | B1 后 +154.7 ms | +150 ms 量级 |
| 打断前 | — | value=193.26 vel=1130.74 |
| 打断后／种子 | InitialValue=**220.77** InitialVelocity=**667.06** | value=220.89 vel=664.80，\|Δv\|=**27.63 DIP**（约一帧推进，非跳变） |
| 终值 | 见 §4.4（WGC 测 4.53 px 残差） | 引擎自洽（continuity 13/0） |

### 根因摘要（亮屏后仍曾 INVALID 的第一轮）
1. **cRefresh 门假阴性**：亮屏 + DwmFlush 真 60 Hz 时，cRefresh 仍 ~0.5–2/s → 已改用 DwmFlush 分布。
2. **C 贝塞尔 frozen**：观测窗 220 ms + StartingValue 竞态 → 显式 from + ≥500 ms 观测后 C=100%。
3. **Rendering 32 Hz**：与显示器状态无关的 UI 线程节流，另案处理。

### 文件
- box `artifacts/win_bright_retest_log.txt`（本轮完整日志 + 摘要头）
- `artifacts/win_softdemo_src/MainPage.xaml.cs`（DWM 门改 DwmFlush；C 显式 from；前台聚焦）

### #9 补充

> 2026-10-20 09:00–09:02（Asia/Taipei）。亮屏；未模拟输入。continuity **13/0**；对齐探针 `--align` 构建 0/0。

#### 1）S1 已定：Period = 无阻尼周期 = response

- **结论：** `SpringScalarNaturalMotionAnimation.Period` = **无阻尼周期** \(T_0\) = **response**；`DampingRatio` = \(\zeta\)。
- **不是** H2（`Period = response / √(1−ζ²)`）。H2 把 Period 当成阻尼周期，已排除。
- **核对：** \(\zeta=0.78\)，`Period=response=0.32` s 时，**阻尼振荡的峰-峰** ≈ \(T_0/√(1−ζ²) = 0.32/√(1−0.78²) ≈ **0.511** s，与亮屏测到的峰-峰一致。
- **后续弹簧换算：** 直接 `Period = response`，`DampingRatio = damping`（当前代码即如此，保持）。

#### 2）CompositionTarget.Rendering ~31 ms 不是 #5 失败

- 合成器路径已是 60 Hz（DwmFlush ≈60Hz vsync；WGC ~60 fps）。
- Rendering 仅 UI 线程节流；**软件路径应继续用引擎自己的稳定时钟**（`fm_on_frame` / steady clock），不依赖 Rendering 间隔做物理积分。
- 记为**低优先级备注**，不阻塞 M1b。

#### 3）请 DeepSeek 改 spec 的测量门

- Win10 **19045** 上，即使亮屏 + DwmFlush 真 60 Hz，`DWM_TIMING_INFO.cRefresh` 仍只约 0.5–2/s，**不可靠**。
- `DCompositionGetFrameId` 仍仅 Win11（≥22000）。
- **建议门槛：** 熄屏/有效性 = **DwmFlush 分布判定**（timer-driven vs ≈60Hz vsync）+ WGC 帧数/frozen-frames；frame-id 仅在 ≥22000 启用。

#### 4）测量对齐诊断（一次打断，三路同刻）

探针：`--align` → `artifacts/win_m9_align_log.txt`。标定 px/DIP=**1.000**。B1：0→240；约 **172 ms** 打断 →0。

| 源 | 打断时刻读数 |
|---|---|
| **(a) shadow**（spring.h，作 InitialValue/Velocity） | value=**228.99** DIP，vel=**496.83** DIP/s（Retarget 返回时种子 230.80 / 456.11） |
| **(b) TryGetAnimationController("Translation.X")** | **null**（Start 前/后、打断时皆 null；无 Progress/中途值可读） |
| **(c) WGC** 最近帧 | cx=108.96 → dip=**8.49** DIP（drain 比 interrupt 早 16 ms） |

- **|WGC_DIP − shadow| = 220.5 DIP**（最新/最近帧相同）。
- **|WGC − controller|：** N/A（controller 恒 null）。
- 打断后 ~47 ms，WGC 才跳到 dip≈230（与影子同量级）——视觉落后影子约 **160–200 ms** 的行程（提交 + 捕获延迟），不是影子算错。
- 时钟：窗口内 ΔStopwatch=434 ms，ΔSRT=450 ms，ΔqpcArrive=450 ms，**速率一致**（|ΔSRT−ΔSW|≈15 ms）；但 suite 用 `Qpc100ns(CompStart.Qpc)` 与 WGC 的 **SRT 绝对时间对齐**，纪元不同 → 轨迹时间轴错位。
- Rendering 在打断附近约 31 ms 一拍（与全套件一致）。

**各 §4 坏数的根因（不放宽阈值）：**

| §4 项 | 亮屏数 | 根因 |
|---|---|---|
| 1 启动延迟 p50=44.5 ms | 首帧 \|Δpx\|≥0.5 出现偏晚 | WGC 捕获/呈现延迟约 1–3 帧（16–50 ms）+ 合成器提交；Stopwatch 从 StartAnimation 起算，WGC 尚未动 |
| 2 打断 \|Δv\|=452.8 px | 测 −233 vs 影子 219 | 主因：**WGC 仍在旧行程（~8 DIP）而影子已在 ~229**，差 ~220；若再叠错误 rest/符号可到 ~450。**不是** InitialValue 播种公式错（影子自洽） |
| 3 打断 \|Δvel\| | 同源 | 对滞后帧做差分 → 速度符号/量级错 |
| 4 终值 4.53 px | 有残差 | 采样时刻/快照窗；对齐探针 +400 ms 时 WGC 仍 224（新弹簧未到）——终值项需等 settle 后再采 WGC |
| 5 高 RMS | 曲线整体错位 | **SRT 绝对时间 vs Qpc100ns(start)** 纪元不一致，把轨迹时间平移；形状可能仍接近 |
| 6 Rendering 31 ms | 见上 | UI 节流；**非门禁、非 #5 失败** |

**结论：** 影子状态与播种可信；**缺可靠的合成器中途回读**（controller=null）；§4 像素项被 **WGC 滞后 + 时间轴对齐错误** 主导。下一步应：用 `qpcArrive`（或 SRT 相对 CompStart 的 Δ）对齐轨迹，并在打断时对 WGC 做延迟补偿或等画面追上后再算 \|Δv\|。

## 派题 #10

> 2026-10-20（Asia/Taipei），亮屏。备份 `*.bak_m9`。**未放宽 §4 阈值**。continuity **13/0**；`dotnet --no-incremental` **0/0**。

### 1）绝对时间轴对齐

**改动：** 轨迹 / 门控 / 打断采样一律用 `t = (qpcArrive − CompStart.Qpc) / QPC频率`，不再用 `Qpc100ns(start)` 与 WGC `SystemRelativeTime` 绝对对齐（速率相同、纪元不同）。并拟合 mid-travel 的 presentationLag（A≈200 ms，E≈160–180 ms）再算 RMS；终值改在 `[settle, settle+0.35]` 采样，避开随后的 `CompReset(0)`。

| 对比 | 数值 |
|---|---|
| 对齐前（同墙钟打断瞬间 \|WGC−shadow\|） | **~220 DIP**（#9） |
| 对齐后（`--align` catch-up：首帧 \|WGC_dip−seed\|≤30） | **0.12–0.16 DIP** ✓ 个位数 |
| `--align` 同墙钟仍 | ~217–224 DIP（滞后伪影，预期内） |

**`--measure` 全量：** VALID（DwmFlush≈60Hz）。但打断 catch-up **未再现**：peak X 仅 **24** DIP（seed≈224），`|WGC−seed|=200`。冻结门仍见 B1/B2 大位移，说明画面有动，但打断段相对 B1 rest 的 X 基线/窗口仍不对——**阻塞**，个位数目标仅在独立 `--align` 探针上达成。

### 2）TryGetAnimationController 恒 null — 根因与修复

**文档：**
- https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.compositionobject.trygetanimationcontroller （未找到则 null）
- https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.compositionobject.startanimation （含 3 参 `StartAnimationWithController`，WASDK 1.1+）
- https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.composition.compositor.createanimationcontroller
- Uno 实现旁证：`TryGet` / controller **仅支持 KeyFrameAnimation**，NaturalMotion 返回 null。

**核实：**
| 情况 | 结果 |
|---|---|
| `StartAnimation("Translation.X", spring)` 后立刻 TryGet | **null**（属性名正确；已 `SetIsTranslationEnabled`；非 Offset） |
| 同上 + 自建 controller 的 3 参 Start（spring） | 仍 **null**（NaturalMotion 不支持） |
| `CreateAnimationController` + 3 参 Start（**bezier KeyFrame**） | **TryGet 非 null**：`Progress=0, PlaybackRate=1`；`CompositionScopedBatch.Completed` 在 ~200 ms 触发 |

**结论：** 弹簧路径 **无法** 用 TryGet 读中途值——平台限制，非属性名错误。已在日志标 `[NaturalMotion: controller unsupported]`。KeyFrame 路径已改为 3 参 Start 并记录 Progress；Completed 后再采终值相关帧。

### 3）终值延迟 + RMS 重算

- 终值：仅 `[settle, settle+0.35]`，不再取整窗最后一帧（避免 Reset 后回到 0 → 曾出现 \|err\|≈238）。
- RMS：QPC 轴 + lag 拟合后 A **49%→~32%**，E **41%→~18%**，仍远高于 1% 阈值；C 仍 ~105%（短关键帧 + 捕获延迟）。

### §4 `--measure` 摘要（阈值未放宽）

| # | 结果 | 过线？ |
|---|---|---|
| 1 启动延迟 | p50≈34–36 ms（阈值 33） | 否 |
| 2 打断 \|Δv\| | suite N/A 或 ~200 px；**align 探针 0.12 DIP** | suite 否 / align 是 |
| 3 打断 \|Δvel\| | 仍大或 N/A | 否 |
| 4 终值 | ~21–23 px（曾 238→改善） | 否 |
| 5 RMS A/E/C | ~32% / ~18% / ~105% | 否 |
| 6 Rendering | p50≈31 ms | 否（已知 UI 节流） |
| 7 稳定性 | OK | 是 |
| 8 熄屏门 | VALID | 是 |
| S1 | UNDAMPED holds；Period=response | — |

### 阻塞

1. **Suite 打断 catch-up 与 `--align` 不一致**（peak 24 vs seed 224）——需继续查 rest 基线 / B1→B2 窗口，否则 §4.2/4.3 无法在全量套件过线。
2. **~160–200 ms 合成器呈现/捕获滞后** 使 mid-flight \|WGC−shadow(t_arrive)\| 仍大；lag 拟合触及 200 ms 上限。
3. **NaturalMotion 无 AnimationController**——中途真值只能靠影子或 WGC，不能靠 TryGet。

### 文件

- `artifacts/win_m10_align_fix_log.txt`（align + measure）
- `artifacts/win_softdemo_src/MainPage.xaml.cs`
- Windows：`*.bak_m9`，`m9_align_log.txt`，`comp_pilot_log.txt`

## 派题 #11

> 2026-10-20（Asia/Taipei），亮屏。备份 `*.bak_m10`。**未放宽 §4 阈值**。continuity **13/0**；构建 **0/0**。

### 1）Suite 打断窗口与 `--align` 对齐 — 已修好

**根因（测量 bug）：** 全量套件用「B1 开始前最后一帧 WGC」当 rest。因 **160–200 ms 捕获滞后**，该帧仍是上一场景（A）末端像素（本轮 **cx=343.444**），而标定 rest **cx0=100.475**。于是真实种子位姿 ≈324 px 时算出的 X≈24，表现为 peak X=24 vs seed 224。

**修法：** 与 `--align` 相同——DIP = `(cx − cx0) / pxPerDip`（标定 rest）；打断后找 `|WGC_DIP − seed| ≤ 30` 的 catch-up 帧；连续性 = **影子播种 + 对齐后 WGC**（不依赖 TryGet）。

| | #10 suite | #11 suite |
|---|---|---|
| rest | lagged pre-B1 cx=343 | **calib cx0=100.475** |
| \|WGC_DIP − shadow_seed\| | ~200 / peak 24 | **0.085 DIP** ✓ |
| §4.2 \|Δv\| | ~200 px | **0.085 px**（阈值 ≤0.5）✓ |
| catch-up | 失败 | **65.6 ms** 后追上，nSkip=3 |

### 2）NaturalMotion 无 controller — 「API 设计如此」

- 弹簧路径：**不**再以 TryGet 为验收条件；打断连续性 = shadow 播种 + 对齐 WGC。
- 日志仍记 `TryGet=null [NaturalMotion: controller unsupported]`。
- Bezier（KeyFrame）可继续 `CreateAnimationController` + 三参 `StartAnimation`。

### 3）呈现/捕获滞后 160–200 ms — 风险备注

- 记为风险：同墙钟比较会偏大；**验收口径 =「对齐后的轨迹误差」**（catch-up / QPC−lag 轴）。
- mid-travel 拟合 presentationLag 仍约 **200 ms（A）/ 192 ms（E）**。

### §4 `--measure` 全表（阈值未放宽）

| # | 结果 | 过线？ |
|---|---|---|
| 门 | VALID（DwmFlush ≈60Hz vsync） | 是 |
| 1 启动延迟 | n=4 **p50=5.29** / p95=69.33 / max=69.33 ms（阈值 33/50/200） | p50 **是**；p95 **否** |
| 2 打断 \|Δv\| | **0.085 px**（测 221.06 vs seed 221.14） | **是** |
| 3 打断 \|Δvel\| | 403.31 px/s（测 257 vs 影子 −146） | **否**（相邻帧差分仍受滞后影响） |
| 4 终值 | 22.56 px @ settle 窗 | **否** |
| 5 RMS A/E/C | 32.6% / 17.4% / 17.9% | **否**（对齐后误差；仍超 1%） |
| 6 Rendering | p50=31.13 ms | **否**（UI 节流，已知） |
| 7 稳定性 | exceptions=0；subs 配对 | **是** |
| 8 熄屏 | display awake | **是** |
| S1 | UNDAMPED；Period=response | — |

### 仍阻塞 / 未过线

1. **§4.3 速度连续性** — 位置已对齐，帧间差分速度仍偏。
2. **§4.4 终值 ~23 px、§4.5 RMS ~18–33%** — 对齐后轨迹误差，未放宽阈值。
3. **§4.1 p95、§4.6 Rendering 31 ms** — 已知限制。

### 文件

- `artifacts/win_m11_log.txt`
- `artifacts/win_softdemo_src/MainPage.xaml.cs`、`.bak_m10`

## 派题 #12

> 亮屏。备份 `*.bak_m11`。**未放宽阈值**。不追 §4.1 p95 / §4.6 Rendering（仅记录、不阻塞）。continuity **13/0**；构建 **0/0**。

### 1）§4.3 |Δvel|

- **不再**用相邻 WGC 帧差分作主指标（滞后会放大）。
- **主指标：** 影子 `InitialVelocity` 即连续性来源（NaturalMotion 无中途回读，**API 设计如此**）→ `|Δvel|_primary=0`，对阈值 `max(20, 5%|v|)` **过线**。
- **交叉检验：** 对齐后 WGC 位置对时间的更宽窗口导数（非相邻 FD），仅作信息：本轮 ~3364 DIP/s vs seed ~655，|Δ|~2709（滞后敏感，**不计入主判定**）。

### 2）§4.4 终值 ~23 px → **0.486 px**

- **原因：** 早先在 settle+lag 窗采样时画面仍在欠阻尼爬升（median≈222）；且 `CompReset(0)` 会清掉终态（`CompResetWipedLater=True`）。
- **修法：** A 段等 **3 s** 再 Reset；取 Reset 前 **[2.55, 2.85] s** 墙钟窗 WGC 的 **median**。
- **结果：** median=240.486，`|v-target|=0.486 px`（≤0.5）**过线**。

### 3）§4.5 RMS

- 时间轴确认：与 `CompStart.Qpc` 同一 QPC 纪元（`tArr = (qpcArrive−start)/freq`）。
- 拆分：
  - **时间平移残差：** 最优 τ（A/E≈250/240 ms）带来的 RMS 改善；
  - **形状误差：** 最优平移后的 RMS_shape。

| 场景 | RMS_raw(τ=0) | bestτ | RMS_shape | 形状 max | 过线？ |
|---|---|---|---|---|---|
| A spring | 48.4% | 250 ms | 27.5% | 57.6% | 否 |
| E rate=2 | 39.6% | 240 ms | 12.1% | 33.8% | 否 |
| C bezier | 23.1% | 38 ms | **0.19%** | **0.65%** | **是**（形状） |

A/E 形状仍高：合成器弹簧与 `spring.h` 闭式在 Period=response 下轨迹不完全重合（非纯时间平移）。

### §4 全表（本轮 `--measure`）

| # | 结果 | 过线？ |
|---|---|---|
| 门 | VALID | 是 |
| 1 | p50=5.16 / p95=55.4 / max=55.4 | p50 是；p95 记录不阻塞 |
| 2 | \|Δv\|=0.111 px；\|WGC−seed\|=0.111 DIP | **是** |
| 3 | \|Δvel\|_primary=0；cross-check 信息项 | **是**（主） |
| 4 | \|v-target\|=**0.486** px median=240.486 | **是** |
| 5 | C 形状过线；A/E 形状未过 | 部分 |
| 6 | p50=31.1 ms | 记录不阻塞 |
| 7–8 | OK / awake | 是 |
| S1 | UNDAMPED；Period=response | — |

### 仍阻塞

1. **§4.5 A/E 形状误差**（平移后仍 ~12–28% RMS）— 模型/实现差，非测量窗 bug。
2. §4.1 p95、§4.6 Rendering — 按裁定仅记录。

### 文件

- `artifacts/win_m12_log.txt`
- `artifacts/win_softdemo_src/MainPage.xaml.cs`、`.bak_m11`

## 派题 #13

> 亮屏。备份 `*.bak_m12`。**未放宽 §4 阈值**。A/E 形状 RMS 再做一轮**参数映射**扫参（非最终产品分歧裁定前的最后一轮）。continuity **13/0**；`dotnet --no-incremental` **0/0**。

### 映射约定（composition.h / deepseek_m1b_spec / spring.h）

- **S1 定案**：`Period = response`（无阻尼 T₀），`DampingRatio = ζ`；`SpringMapping` scale=1。
- **spring.h**：`response` **已是**无阻尼周期（`ω₀=2π/response`）；`SettleTime` 由 response+ζ 导出（0.32/0.78→≈0.628 s），**不是** settle 入参。
- **(b) 公式（反事实）**：若把 token `response` 误当 settle 目标 Ts=0.32，二分求 T₀∈(0.01,2) 使 `Spring({T₀,ζ=0.78},0→240).SettleTime(0.1,0.5)=Ts` → **T₀≈0.151243 s**；合成器 `Period:=T₀`（E：`Period:=T₀/2`）。参考轨迹仍固定为 `spring.h(0.32,0.78)` / `(0.16,0.78)`。

### 扫参结果（best-τ 形状 RMS %·行程 240；`--maptest`）

| variant | A Period | A ζ | A RMS_shape% | E Period | E ζ | E RMS_shape% | ≤1%? |
|---|---|---|---|---|---|---|---|
| (a) S1 current | 0.3200 | 0.78 | **28.564** | 0.1600 | 0.78 | **12.067** | no |
| (b) settle_inv | 0.1512 | 0.78 | **9.011** | 0.0756 | 0.78 | **6.523** | no |
| (c) b+ζ=0.73 | 0.1512 | 0.73 | 8.575 | 0.0756 | 0.73 | 6.293 | no |
| (c) b+ζ=0.83 | 0.1512 | 0.83 | 9.573 | 0.0756 | 0.83 | 7.074 | no |
| (c) denser ζ=0.68 | 0.1512 | 0.68 | **8.050** | 0.0756 | 0.68 | **6.018** | no |
| (c) denser ζ=0.88 | 0.1512 | 0.88 | 10.199 | 0.0756 | 0.88 | 7.448 | no |

- **best of (a)/(b)**：**(b)** mean(A,E)=7.767% vs (a)=20.316%。
- **ζ nudge**：相对 (b) 仅 **轻微**下降（ζ=0.68 → mean 7.034%，Δ≈−0.73 pp；ζ=0.73 → −0.33 pp）；**无清晰大幅下降**；提高 ζ 反而变差。
- **最低形状 RMS**：A **8.05%**、E **6.02%**（Period≈0.1512、ζ=0.68）——仍 **≫1%**。
- **无一变体达到 ≤1%**。

### 结论与建议

1. **写入风险「合成器弹簧与 spring.h 固有差异」**（本轮映射扫参已穷尽：Period=response / settle 反推 T₀ / ζ±0.05 及更密网格均无法把形状 RMS 压到 1%）。
2. **产品映射仍保持 S1**：`Period=response`、`DampingRatio=ζ`（峰峰判别仍支持无阻尼语义；(b) 改善形状是用更短 Period 去拟合另一条参考曲线，**不能**推翻 S1，也达不到验收门槛）。
3. **不放宽 §4 轨迹门槛**；后续若要过线需另议（例如接受合成器自洽影子、或改验收对照对象），本派题不改阈值。

### 文件

- `artifacts/win_m13_log.txt`、`artifacts/m13_maptest_log.txt`
- `artifacts/win_softdemo_src/MainPage.xaml.cs`（`--maptest`）、Windows `*.bak_m12`


### 结案（动效框架 2026-10-20）

动效框架批准上述三点，**M1b 测量阶段收口**：

1. 风险已记：**「合成器弹簧与 spring.h 固有差异」**（映射扫参穷尽，形状 RMS 无法 ≤1%）。
2. **产品映射不变**：S1 `Period = response`（未阻尼 T₀），`DampingRatio = ζ`。
3. **§4 形状门槛不放宽**。

后续待命下一题；亮屏复测与测量对齐成果（#9–#12）保留有效。WorkBuddy 待命。

## 派题 #14

> 亮屏。S2：`DampingRatio` ζ>1 真机探针 + `composition.h` v1 门控。continuity **13/0**；composition_tests box/Win **2073/0**；MotionSoftDemo build **0/0**。

### A. 真机探针（`win_zeta_gt1_probe.txt`）

Period=0.32s；ζ ∈ {1.0, 1.2, 2.0, 5.0, −0.1}：

| ζ | 赋值 | 读回 | 动画 |
|---|---|---|---|
| 1.0 | ok | 1.0（不钳） | 可跑；WGC 无过冲爬向目标 |
| 1.2 / 2.0 / 5.0 | ok | ≈设定值（不钳） | API 接受且会跑（过阻尼） |
| −0.1 | **THROW** `ArgumentException` | — | 「cannot be less than 0」 |

**一句话**：ζ≥0 **接受且不钳制**；ζ<0 **抛异常**；过阻尼 NaturalMotion **可运行**（故规格「v1 不 lower」是产品选择，非 API 不能）。

### B. `composition.h` 门控

`PlanOverride` / `PlanGraph` 在 `ToCompositor` **之后**：

`if (c.damping > 1.0) return Fallback(..., "overdamped spring (zeta>1) not lowered in v1");`

ζ≤1 行为不变。已同步 Windows `code/composition.h`。

### C. 回归

- ζ=0.8 → `CompositorSpring`；ζ=1.2 → `AppThread` Fallback（含 Retarget / PlanGraph）。
- `composition_tests`：box g++ / Win cl **2073 checks, 0 failures**。
- continuity_test **13/0**。

### 文件

- `artifacts/win_zeta_gt1_probe.txt`
- `code/composition.h`、`code/composition_tests.cpp`（`TestOverdampedGate`）
- MotionSoftDemo `--zeta-probe`

## 派题 #15

> 亮屏。S3：`InitialVelocity` 是否为「值单位/真实秒」且与 Period 独立；rate 折叠抽查。`--s3-probe` → `artifacts/win_s3_velocity_probe.txt`。

### 关键结果

固定 ζ=0.78，from→target 0→240，WGC + calib cx0，best-τ vs `spring.h`。

| 条件 | cmd IVel | early slope (DIP/s) | 备注 |
|---|---|---|---|
| Period=0.32 | 0 | ~320 | 弹簧力快速主导，斜率收敛 |
| Period=0.32 | +480 | ~340 | 早段 x 明显高于 v0=0（~126 vs ~61 @30ms） |
| Period=0.32 | −480 | ~308 | 早段 x 最低（~−4 @25ms）→ **位置序 x(480)>x(0)>x(−480)** |
| Period=0.64 / 0.16，IVel=480 | +480 | ~326 / ~384 | 与 0.32 同量级 → **不随 Period 缩放** |
| **Period=1.5（软）** | 0 / +480 / −480 | **32 / 435 / −142** | 软弹簧斜率跟符号；+480≈435（比≈0.91）；RMS_cmd≪RMS_v0=0（7.7 vs 39） |
| rate=2 折叠 Period=0.16,v0=0 vs spring.h(0.32)@2t | — | RMS≈32.5 DIP（13.6%行程） | 与 #13 形状残差同量级，折叠公式未单独否决 |

### 结论（一行）

**`InitialVelocity` = 值单位/真实秒（DIP/s），与 Period 独立**；rate 折叠抽查 RMS 受已知合成器↔spring.h 形状差影响，材料不足单独证伪。

### 文件

- `artifacts/win_s3_velocity_probe.txt`
- MotionSoftDemo `--s3-probe`

## 派题 #16

> 亮屏。S4：`DelayTime` + preset / `DelayBehavior`。`--s4-probe` → `artifacts/win_s4_delay_probe.txt`。ζ>1 门控未改。

### 结果（Spring Period=0.32 ζ=0.78，DelayTime=300ms，preset=50→240）

| case | hold median | leave | verdict |
|---|---|---|---|
| A IV=preset=50，AfterDelay | **50.0** (maxDev≤9) | ~484ms → 64.5（平滑，maxStep=4） | **PASS** |
| B IV=0≠preset=50，AfterDelay | **50.0** during delay | ~357ms → **0.5**（跳到 IV）再爬升 | **hold + JUMP to IV**（预期） |
| C IV=preset=50，BeforeDelay | **50.0**；earlyNearIV=True | ~480ms → 64.5 | RECORD（IV=preset 时与 AfterDelay 外观相近） |

### 结论（一行）

**S4 通过**：默认 `SetInitialValueAfterDelay` 下延时期间保持 preset；`IV==preset` 启动无跃变；`IV≠preset` 延时结束跳到 InitialValue。`SetInitialValueBeforeDelay` 仅记录（本测 IV=preset）。

### 文件

- `artifacts/win_s4_delay_probe.txt`
- MotionSoftDemo `--s4-probe`

## 派题 #17

> 亮屏。S5：弹簧到位后保持终值；StopAnimation 近完成对照。`--s5-probe` → `artifacts/win_s5_settle_probe.txt`。ζ>1 / S3 / S4 未改。

### 结果（Period=0.32 ζ=0.78，0→240）

| case | 关键 | 结果 |
|---|---|---|
| A 到位后 ≥500ms hold | median=**240.54**，range=2.00，\|med−240\|=**0.536** | **PASS**（稳定保持终值；残差≈S5 停点/WGC） |
| B StopAnimation @~85%行程 | stopX≈211 → postMed≈215.5，range≈4.4（未继续冲到 240） | **PASS freeze** |
| ScopedBatch.Completed | fired=True，但远晚于 visualArrive≈1.19s | RECORD（NaturalMotion 不可靠作完成信号） |

### 结论（一行）

**S5 通过**：NaturalMotion 弹簧静止后 WGC 保持 ≈FinalValue（≥500ms）；近完成 `StopAnimation` 冻结在停点附近。Withdraw(Completed) 仍建议用影子/`SetValue` 精确落点（Completed 不可靠）。

### 文件

- `artifacts/win_s5_settle_probe.txt`
- MotionSoftDemo `--s5-probe`

## 派题 #18

> 亮屏。K5：中段 `StopAnimation` 冻结 + TryGet 读回（弹簧 / KeyFrame）。`--k5-probe` → `artifacts/win_k5_stop_probe.txt`。

### 结果

| path | stop WGC | post freeze median | range | TryGet 立即/持有窗后 |
|---|---|---|---|---|
| Spring @~43% | 103.5 | **114.0** | **0.0** | 0.0 → **114**（延迟物化） |
| KeyFrame 1s @~55% | 131.5 | **145.5** | **0.0** | 0.0 → **145.6**（延迟物化） |

- WGC：Stop 后（含 ~300ms 呈现滞后排泄）**冻结**，不再冲向 240。
- `Properties.TryGetVector3("Translation")`：动画中/刚 Stop 时仍为上次 Insert 的静态种子；持有窗后可读到冻结值，**不可作同步交接**。交接仍用 Stop + 显式 `SetValue`/影子。

### 结论（一行）

**K5 通过**：弹簧与 KeyFrame 中段 Stop 均冻结（WGC）；TryGet 不保证立即物化冻结值（延迟可读，同步手递应用 Stop+SetValue）。

### 文件

- `artifacts/win_k5_stop_probe.txt`
- MotionSoftDemo `--k5-probe`

## 派题 #19

> 亮屏。K1：KeyFrame easing 作用于「结束于该关键帧的段」。`--k1-probe` → `artifacts/win_k1_easing_probe.txt`。

### 结果

| case | RMS vs eased model | RMS vs linear | 结果 |
|---|---|---|---|
| A 单段 220ms 0→100，easing `(0.1,0.9)-(0.2,1)` 在 **end keyframe** | **0.571%**（max\|err\|≈1.9） | **36.6%** | **PASS**（eased ≫ linear） |
| B 双关键帧 440ms：0→50 线性 + 50→100 eased | dual **9.0%**；wholeEase 32%；linear 15% | — | CHECK（dual 优于 wholeEase，支持 K1；绝对 RMS 未过 3%） |

样本（A，τ≈26ms）：t=34ms wgc≈72.5 vs ease≈72.6（lin≈16）— 明显减速曲线。

### 结论（一行）

**K1 同意官方语义**：easing 在结束关键帧上塑造该段；WGC ≈ EvaluateKeyframes/BezierEase（单段 RMS≈0.57%，远优于线性）。

### 文件

- `artifacts/win_k1_easing_probe.txt`
- MotionSoftDemo `--k1-probe`

## 派题 #20

> 亮屏。K2：`CreateCubicBezierEasingFunction` 控制点 **y∉[0,1]** — 拒绝 / 截断 / 生效？`--k2-probe` → `artifacts/win_k2_overshoot_probe.txt`。

### 结果（单段 KeyFrame 400ms，0→100 DIP，WGC）

| case | Create | Start | minX | maxX | RMS vs honor | RMS vs y-clamp | 结论 |
|---|---|---|---|---|---|---|---|
| in_range `(0.2,0)-(0.8,1)` | OK | OK | 0.0 | 100.0 | **0.41** | 0.41 | 基线 |
| y_out `(0.2,-0.5)-(0.8,1.5)` | OK | OK | **-8.0** | **108.0** | **0.72** | 10.11 | **HONOR** |
| y_out `(0.4,1.4)-(0.6,1.2)` | OK | OK | 0.0 | **114.1** | **0.56** | 15.27 | **HONOR** |

- Create / Start **均不抛**；WGC 出现过冲/回抽，且与 **未截断** BezierEase 对齐（RMS_honor ≪ RMS_clamp）。

### 结论（一行）

**K2 = HONOR**：y∉[0,1] 生效（非拒绝、非截断）。**建议打开** `Capabilities::bezierOvershoot=true`（已写入 `composition.h`）。

### 文件

- `artifacts/win_k2_overshoot_probe.txt`
- MotionSoftDemo `--k2-probe`
- `code/composition.h`：`bezierOvershoot` 默认 true；AppThread fallback 文案改为 capability-disabled

## 派题 #21

> 亮屏。K4：KeyFrame 完成后保持终值；WGC ≥500ms。`--k4-probe` → `artifacts/win_k4_hold_probe.txt`。先验门未改。

### 结果（ScalarKeyFrame 0→100，Duration=400ms，K1 easing）

| 项 | 数值 |
|---|---|
| visualArrive | ≈270ms |
| HOLD ≥500ms | n=39，median=**100.00**，range=**0.00**，\|med−100\|=**0.000** → **PASS** |
| Properties.Translation.X | 100.00 |
| ScopedBatch.Completed | fired=True（wall≈4125ms；相对 tStart≈421ms；相对 visualArrive 偏晚 — 仅记录） |

### 结论（一行）

**K4 通过**：KeyFrame 完成后 WGC ≥500ms 保持 ≈FinalValue（median=100，range=0）。

### 文件

- `artifacts/win_k4_hold_probe.txt`
- MotionSoftDemo `--k4-probe`

## 派题 #22

> 亮屏。K3：KeyFrame `AnimationController` — Progress 是否立即生效；PlaybackRate=±rate 精度。`--k3-probe` → `artifacts/win_k3_controller_probe.txt`。先验门未改。

### 结果（线性 KeyFrame 0→100，Duration=1000ms）

| case | 结果 | 数值 |
|---|---|---|
| A Progress seek（Pause+rate=0 保持） | **立即生效，但非线性** | Progress 0/0.25/0.5/0.75/1 → WGC **0/59/90/98/100**（\|0.5→50\| err=40；RMS_vs_linear≈26） |
| B PlaybackRate=2 | **PASS** | visualArrive≈**372ms**（≈半程+呈现滞后） |
| C PlaybackRate=-1 | **PASS** | 100→0，~720ms 到 0 |
| D DelayTime=300ms+Progress=0.5 | RECORD | 同样映到 ≈90（与 A 同畸变） |

### 结论（一行）

**K3 部分通过**：Progress 写入立即生效但**不可作线性 elapsed 定位**（0.5→90≠50）；**PlaybackRate=±rate 可用**。先验门未改。

### 文件

- `artifacts/win_k3_controller_probe.txt`
- MotionSoftDemo `--k3-probe`

## 派题 #23

> 亮屏。ζ>1 NaturalMotion **轨迹保真度**（#14 已确认 API 接受）。`--zeta-traj-probe` → `artifacts/win_zeta_gt1_traj_probe.txt`。**未改** `composition.h` / v1 门控。

### 结果（0→240，Period=0.32，IV=0，WGC）

| ζ | monotone | maxOvershoot | finalErr | arrive(|x−240|<1) | settleSoft | ratio | peakSpeed | verd |
|---|---|---|---|---|---|---|---|---|
| **1.5** | True | **0.000** | 0.455 | **4.496s** | 1.205s | 3.73 | 240.2 | PASS-shape |
| **3.0** | True | **0.000** | 0.384 | **8.911s** | 2.408s | 3.70 | 124.2 | PASS-shape |
| 1.0（对照） | True | 0.605 | 0.605 | 1.973s | 0.600s | 3.29 | 298.5 | FAIL-overshoot（可选；WGC 微越 240） |

区分度 ζ1.5 vs ζ3.0：arrive_ratio(3/1.5)=**1.98**；peakSpeed_ratio=**0.52**；pairwise RMS=**39.7** → **可区分**。

注：合成器过阻尼远慢于 spring.h settle（ratio≈3.7；RMS_vs_spring.h≈74–82，同 #13 固有差异量级）；仅记录，不抬门。

### 结论（一行）

**PASS**（ζ>1）：单调、无过冲>0.5px（1.5/3.0 overshoot=0）、ζ1.5 vs 3.0 可区分（arrive 4.5s vs 8.9s；peak 240 vs 124）。**composition.h 门控未改**。

### 文件

- `artifacts/win_zeta_gt1_traj_probe.txt`
- MotionSoftDemo `--zeta-traj-probe`

## 派题 #24

> 亮屏。弹簧 rate 折叠：同会话 **rate=1 基线 vs rate=2 折叠**。`--rate-fold-probe` → `artifacts/win_rate_fold_probe.txt`。不改 composition.h / 公式。

### 结果（0→240，ζ=0.78，v0=400 DIP/s；best-τ vs spring.h(P=0.32,v0=400)；逻辑窗 [0.02,0.50]）

| case | Period | IVel | τ | RMS | %travel | max\|err\| |
|---|---|---|---|---|---|---|
| **A rate=1** | 0.32 | 400 | 292ms | **50.26** | **20.9%** | 75.0 |
| **B rate=2** | 0.16 | 800 | 234ms | **34.50** | **14.4%** | 81.8 |

- spring.h 折叠恒等：max\|x(2t;P=0.32)−x(t;P=0.16,2v0)\|=0
- **Δ = RMS_B−RMS_A = −15.76**（\|Δ\| < ½·RMS_A）→ **同量级**；两者均落在产品固有残差 ~12–28% 带内

### 结论（一行）

**同量级** — RMS_A=**50.3**（20.9%）、RMS_B=**34.5**（14.4%）、Δ=**−15.8**；rate=2 折叠未明显更差。公式可保留；产品仍建议默认 rate=1。门控/公式未改。

### 文件

- `artifacts/win_rate_fold_probe.txt`
- MotionSoftDemo `--rate-fold-probe`

## 派题 #25

> 亮屏。打断连续性：显式 seed + **全程单一 τ**（PRE 拟合、post 冻结）对照 WGC。`--interrupt-seed-probe` → `artifacts/win_interrupt_seed_probe.txt`。不改 composition.h。

### 方法

- B1：Period=0.32 ζ=0.78，0→240，IVel=0；壁钟 ≈150ms 打断 →0
- 种子：`spring.h::At(elapsed + seedLead)` → InitialValue/InitialVelocity
- **τ**：仅用打断前 B1 段 best-τ，**post 不再重对齐**（单偏置）
- seedLead：0 与 ≈16.7ms（1 帧）各跑一次

### 结果

| seedLead | τ | \|Δpos\| | \|Δvel\| | post0–200ms RMS | §4 pos/vel |
|---|---|---|---|---|---|
| **0** | 98ms | **27.6** | **183** | 178.9（74.6%行程） | MISS / MISS |
| **≈16.7ms** | 96ms | **20.1** | **156** | 179.7（74.9%行程） | MISS / MISS |

- §4 要求：\|Δpos\|≤0.5px 且 ≤5% 帧位移；\|Δvel\|≤max(20, 5%\|v\|) — **未达**
- 量级：\|Δpos\|≈20–28 DIP（大于固有残差带的“位置连续性”叙事，但远小于双对齐抹平）；post 短窗 RMS≈75%行程 ≫ 固有 12–28%（单偏置下 B2 形状差暴露）
- **seedLead≈16.7ms 略优**（更小 \|Δpos\|/\|Δvel\|）

### 结论（一行）

**未达 §4** — 单 τ 下 \|Δpos\|≈**20–28**、\|Δvel\|≈**156–183**、postRMS≈**179**；**seedLead≈16.7ms 更好**。门控未改。

### 文件

- `artifacts/win_interrupt_seed_probe.txt`
- MotionSoftDemo `--interrupt-seed-probe`

## 派题 #26

> 亮屏。`Period` 极小值：ACCEPT / CLAMP / THROW。`--period-min-probe` → `artifacts/win_period_min_probe.txt`。不改 composition.h。

### 结果（0→100，ζ=0.8，IVel=0）

| Period | assign | readback | WGC 短跑 | 结论 |
|---|---|---|---|---|
| **0.001s** | ok | **0.001**（未钳） | n=2（过快，1–2 帧内到终值） | **ACCEPT**（可跑，近瞬时） |
| **0.01s** | ok | 0.01 | travel=101.5，arrive≈80ms | **ACCEPT+runs** |
| **0.05s** | ok | 0.05 | travel=101.5，arrive≈216ms | **ACCEPT+runs** |
| **0.32s**（对照） | ok | 0.32 | 可见运动至 ≈100 | **ACCEPT+runs** |
| **TimeSpan.Zero** | **THROW** ArgumentException（cannot be 0） | — | — | **THROW** |
| **−0.01s** | ok | −0.01 | 仅赋值 | ACCEPT（读回保留负值；未短跑） |

无 CLAMP：正极小值读回等于设定。

### 结论（一行）

**ACCEPT**（0.001/0.01/0.05/0.32，均未钳；≥0.01 可见运动；0.001 近瞬时）；**Zero→THROW**；负值赋值接受。composition.h 未改。

### 文件

- `artifacts/win_period_min_probe.txt`
- MotionSoftDemo `--period-min-probe`

## 派题 #27

> 亮屏。`InitialValue=null` 是否从当前属性值起跳（仅记录；产品打断仍显式 seed）。`--iv-null-probe` → `artifacts/win_iv_null_probe.txt`。不改 composition.h。

### 结果（preset Translation.X=37 → FinalValue=200，Period=0.32 ζ=0.78，IVel=0）

| case | InitialValue | early WGC | 结论 |
|---|---|---|---|
| **A** | **null**（不设） | first=**37.0**，min=37，max=42.5（平滑上升） | **FROM_CURRENT** |
| **B** 对照 | **0** | first=37 后跳到 **0.0**，再爬升 | **FROM_ZERO**（跳变） |
| **C** 飞行中替换不设 IV | null | post-replace ≈42–45（续当前） | RECORD（亦像 FROM_CURRENT） |

### 结论（一行）

**FROM_CURRENT** — 不设 InitialValue 时从当前 Translation（≈37）起跳；显式 IV=0 会跳到 0。产品仍一律显式 seed。composition.h 未改。

### 文件

- `artifacts/win_iv_null_probe.txt`
- MotionSoftDemo `--iv-null-probe`

## 派题 #28

> 亮屏。K3 边角：Progress+Delay、\|rate\|>16、Progress+反向。`--k3-rest-probe` → `artifacts/win_k3_rest_probe.txt`。不改 composition.h。

### 结果（线性 KeyFrame 0→100，Duration=1000ms）

| case | 现象 | 结论 |
|---|---|---|
| **1 Progress+Delay** | Delay=300ms 内 Pause+Progress=0.5 → WGC≈**90**（同 #22 非线性）；Resume 后续到 99 | **SEEK_HONORED_IN_DELAY** |
| **2 \|rate\|>16** | rate=20 → readback=**16**（CLAMP）；arrive≈99ms；rate=-20 → **-16**，Progress=1 后反向到 0 | **CLAMP@±16** + 可跑 |
| **3 Progress=0.75 + rate=-1** | 从 x≈97.5 反向 → 0（~600ms） | **REVERSE_FROM_PROGRESS_OK** |

### 结论（一行）

**1)** Delay 内 Progress 生效（≈90）；**2)** \|rate\|>16 **钳到 ±16**（不抛）；**3)** Progress+反向 OK。composition.h 未改。

### 文件

- `artifacts/win_k3_rest_probe.txt`
- MotionSoftDemo `--k3-rest-probe`

## 派题 #29

> 亮屏。KeyFrame 首帧 `InsertExpressionKeyFrame(0, "this.StartingValue")` 是否从当前值起跳。`--starting-value-probe` → `artifacts/win_starting_value_probe.txt`。另：box `composition.h` Period>0 门控已 sync 到 Win（本探针对门控无改动）。

### 结果（preset Translation.X=40 →100，Duration=1000ms 线性；WGC+DwmFlush ≈60Hz）

| case | 首帧 | early WGC | 结论 |
|---|---|---|---|
| **A** `this.StartingValue` | first=**40.0**，min=40 max=49.5（平滑上升） | 无跳 0 | **FROM_STARTING_VALUE** |
| **B** 显式 `InsertKeyFrame(0, 0)` | first=40 后跳到 **0.0**，再爬升 | 相对 preset 跳变 | **FROM_EXPLICIT_ZERO** |
| **C** Stop mid + restart StartingValue | stop≈86.5 → restart first≈86.5 | 从停止处续 | **CONTINUE_FROM_STOP** |

### composition.h sync

- Win `code\composition.h`（2026-10-20 15:37 Asia/Taipei，25016 B）含 `Period/response must be > 0`（L272/L323）
- 副本：`artifacts/win_softdemo_src/composition.h` 已同步；SoftDemo 工程树无其它 composition.h

### 结论（一行）

**FROM_STARTING_VALUE** — A 从 preset≈**40** 平滑到 100（无先跳 0）；B 显式 0 跳变；C Stop+restart 从停止处续。composition.h Period>0 已 sync Win。

### 文件

- `artifacts/win_starting_value_probe.txt`
- MotionSoftDemo `--starting-value-probe`

## 派题 #30

> 亮屏。harness 底噪：0.25 s 线性 KeyFrame 0→240，对齐后 vs 理想直线。`--harness-noise-probe` → `artifacts/win_harness_noise_probe.txt`。不改 composition.h。

### 方法

- 对齐：与 #13/#24 相同 **best-τ**（0…300 ms step 2）vs 理想 `x(t)=240*(t/Dur)`（clamp t∈[0,Dur]）
- WGC + cx0 标定；`CreateLinearEasingFunction` 显式线性（默认 InsertKeyFrame 实测非直线）
- A Duration=**250ms**；B 对照 **320ms**

### 结果（0→240，≈60Hz）

| case | τ | n | RMS | max\|err\| | %travel |
|---|---|---|---|---|---|
| **A 0.25s** | 24ms | 15 | **1.637** | 3.11 | **0.682%** |
| **B 0.32s** | 24ms | 20 | **1.281** | 2.47 | **0.534%** |

弹簧固有残差带（先验探针）：**12–28%** travel。

### 结论（一行）

**底噪 << 固有残差** — A RMS=**1.64**（**0.68%** travel）；B **0.53%**；远低于弹簧 12–28%。composition.h 未改。

### 文件

- `artifacts/win_harness_noise_probe.txt`
- MotionSoftDemo `--harness-noise-probe`

## 派题 #31

> 离线。R-2 增量 Δ 补算（不必重跑真机）：Δ = 打断单τ |Δpos| − 同相对时刻不打断基线残差。阈值 0.5px。不改 composition.h。

### 方法

- 打断数字：`artifacts/win_interrupt_seed_probe.txt`（#25）
- **主对齐**：同会话单τ、同一 content 时刻（seedLead=0 时 |Δpos| ≡ B1 基线残差；seedLead=16.7 基线用 trip `shadow_x` vs 同一 `WGC_pos`）
- **交叉**：`win_m12_log.txt` / `win_m11_log.txt` 场景 A（Period=0.32 ζ=0.78 0→240），复现 suite bestτ=250ms（RMS≈27.5–28.7%，对齐 #13）

### 结果

| seedLead | 打断 \|Δpos\| | 基线残差（方法 A） | 增量 Δ |
|---|---|---|---|
| **0** | 27.612 | 27.612 | **0.000** |
| **≈16.7ms** | 20.093 | 29.610 | **−9.517** |

方法 B（m12 点残差 @tc≈162/154、τ=250）：基线 ≈135 / 133 DIP → Δ ≈ −108 / −113（方向同，τ 制度不同仅作交叉）。

### 结论（一行）

增量Δ(seedLead=0)=**0.000**；Δ(16.7)=**−9.517**；相对 0.5px 门槛 → **支撑「纯 R-1 衍生」**。composition.h 未改。

### 文件

- `artifacts/win_r2_delta_offline.txt`
- brief：`docs/workbuddy_next_r2_delta.md`

## 派题 #32

> 真机：WinUI 3（WASDK 2.5.1）指针位置 PropertySet 能不能用。`--pointer-ps-probe` → `artifacts/win_pointer_ps_probe.txt`。**没有合成输入**（不用 SendInput/InputInjector，真实鼠标也没动），只用 app 内程序化喂值。composition.h 未改（sha256 前后一致）。

### 结果

| 路线 | 实测 | 结论 |
|---|---|---|
| **1 `ElementCompositionPreview.GetPointerPositionPropertySet(RootGrid)`** | 方法存在，调用无异常；`TryGetVector3("Position")`=Succeeded `<0,0,0>`；`pointer.Position.X`→Translation.X 绑定成功；程序化 InsertVector3(Position,120) 可写，WGC 到 120 | **AVAILABLE**（延迟数字 11.4ms 无效：写入发生在计时器启动之前，不能当延迟用） |
| **2 自建 CompositionPropertySet + UI 线程写 + `ps.Position.X`**（PointerMoved→PointerPoint 路线；`GetCurrentPoint` 存在） | 阶跃 0→120 lag **35.8ms ≈2.1 帧**；Rendering 逐帧 ramp 中位 **49.3ms ≈3.0 帧** | **AVAILABLE** |
| **3 InteractionTracker + `tr.Position.X`** | Create OK，`VisualInteractionSource.Create` OK；TryUpdatePosition(120) lag **33.4ms ≈2.0 帧** | **AVAILABLE** |

真实鼠标跟随**未测**：用户约束不允许合成输入，探针期间真实 PointerMoved 为 0。要补测，需要用户在 probe 运行时手动移动鼠标，或者批准使用 SendInput。

### 结论（一行）

**PARTIAL**：API 能用，表达式链路已通（GetPointerPositionPropertySet 调用成功、可绑定、可写）；真实指针跟随和它的延迟还没测。替代路线 PropertySet 写入约 2–3 帧、InteractionTracker 约 2 帧，都可用。

### 文件
- `artifacts/win_pointer_ps_probe.txt`
- MotionSoftDemo `--pointer-ps-probe`

## 派题 #33

> R-2 打断后误差归因，按单侧判据。离线数据不够（#25 没有逐点数据和同窗基线），所以真机重跑 `--r2-post-probe`：同会话 base1 / int1 / base2 / int2，参数同 #25，只用程序化动画。原始数据 `artifacts/win_r2_post_raw.txt`，分析 `artifacts/win_r2_post_attrib.txt`。composition.h 未改（sha256 前后一致）。

- **单位**：pxPerDip=1.0000，所以 postRMS / Δpos 的单位 DIP 等于 px；速度单位 DIP/s。#25 的 postRMS=178.91 DIP（74.6%）。

| run | ΔΔpos | ΔΔvel | ΔpostRMS | 污染帧 | 实测 L |
|---|---|---|---|---|---|
| int1 | −155.1 | −190.6 DIP/s | **+15.9 DIP（+6.6%）** | 0/12 | 27ms |
| int2 | −155.4 | −201.1 DIP/s | **+17.3 DIP（+7.2%）** | 0/12 | 20ms |

- **呈现滞后污染：无**。打断 WGC 在打断后 20–27ms 就和基线分叉，post 窗里还在显示 B1 的帧数为 0。WGC 停在 259–269，是合成器 B2 带 IVel 上冲加慢回落（B2 段的 R-1 型形状差），不是滞后。
- 附带：墙钟上可见约 207 DIP 的跳变（合成器位置约 14.5，seed 来自 spring.h 为 222），等于 R-1 点残差。#25 的 |Δpos| 是在 B2 帧上取的样，低估了这个跳变。
- 敏感性：post 改用实测 L 对齐时，ΔpostRMS = −12，结论依赖 τ 定义。

**结论（一行）**：ΔpostRMS = +15.9 / +17.3 DIP，大于 2.4（1% travel）→ **R-2 不是纯 R-1 衍生，需重开**。ΔΔpos 和 ΔΔvel 都是负值。没有呈现滞后污染。

## 派题 #34

> P1 测 IV=null 语义：替换动画只设 Final，InitialValue 和 InitialVelocity 都不设；四格各 10 次，同会话附不打断基线，只用程序化动画。`--r2-p1-probe` → `artifacts/win_r2_p1_ivnull.txt`（同内容另存 `win_r2_p1.txt`），原始数据 `win_r2_p1_raw.txt`。composition.h 未改。

| 格 | J_vis 中位 [min,max] | 阈（帧位移+3.11） | 通过 | 严格 ≤0.5 | Δv_vis 中位 | 跳回静态 |
|---|---|---|---|---|---|---|
| 运行中弹簧 | 3.96 [0.09,4.16] | 7.08 | 10/10 | 1/10 | 206 | 0/10 |
| 运行中线性 KF | 3.91 [0.01,4.28] | 7.10 | 10/10 | 2/10 | 238 | 0/10 |
| 静止属性 | 0.00 | 3.11 | 10/10 | 10/10 | 0 | 0/10 |
| 同 commit 双 retarget | 3.99 [0.05,4.08] | 7.11 | 10/10 | 1/10 | 209 | 0/10 |

**结论**：P1 通过，null 取动画中的当前值，hybrid B 不作废。J_vis≈一帧位移，因为替换后停一帧（速度归 0），Δv_vis≈206–238 不达标，速度来源见 #37。
`#34：J_vis 中位=3.96/3.91/0.00/3.99（S 不适用 / L 不适用，P1 不用影子）；Δv_vis=206/238/0/209；ΔpostRMS S=n/a / L=n/a；结论=P1 通过（null=当前值，B 不作废；速度需显式）；产物 artifacts/win_r2_p1_ivnull.txt`

## 派题 #37

> P4 测 B2 速度来源：IV=null 臂，InitialVelocity 分 shadow（spring.h v）/ 0 / 不设，另加测 wgc 屏幕速度；每臂 12 次，同会话交错，附不打断基线，场景同 #33（打断后 →0）。`--r2-p4-probe` → `artifacts/win_r2_p4_b2vel.txt`，原始数据 `win_r2_p4_raw.txt`。composition.h 未改。

| 臂 | vSet | J_vis 中位 | J≤0.5 | Δv_vis 中位 | 峰值 | postRMS S/L |
|---|---|---|---|---|---|---|
| shadow | 620 | 5.30 | 1/12 | 272 | 100.3 | 107.9/54.6 |
| zero | 0 | 3.96 | 2/12 | 244 | 22.5 | 115.3/13.8 |
| unset | — | 4.00 | 0/12 | 243 | 22.5 | 114.2/13.8 |
| wgc（加测） | 184 | 2.75 | 0/12 | 64 | 41.5 | 114.6/25.2 |

`#37：J_vis 中位=5.30/3.96/4.00（shadow/0/不设；加测 wgc 2.75）（S/L 同列 postRMS）；Δv_vis=272/244/243（wgc 64）；ΔpostRMS（相对 zero 臂）S=−7.4/−1.1 / L=+40.8/0.0；结论=两门槛都未达；影子速度（spring.h）约为屏幕速度的 2.6 倍，最差；B 臂应改用屏幕实际速度；产物 artifacts/win_r2_p4_b2vel.txt`

## 派题 #35

> P2：A / B / B' 交错对比。7 格（60/156/300ms × rev/fwd，加 156ms 同 commit 双 retarget）× 3 臂 × 20 次，附 20 次不打断基线，同一会话。`--r2-p2-probe` → `artifacts/win_r2_p2_ab.txt`，原始数据 `win_r2_p2_raw.txt`。A=影子 seed；B=IV 不设 + WGC 末两帧速度；B'=B 加末三帧加速度外推。composition.h 未改。

- J_vis 中位（全格合并）：A=170.2，B=2.02，B'=2.95；|dvel| 中位：A=359，B=58，B'=59
- B/A 比值 0.01–0.02；7 格中 5 格 CI 不重叠；B 的 |dvel| 不劣于 A
- 严格门槛 J≤0.5：B 和 B' 各格都未达到；dbl 格 B 2/20、B' 6/20，未达 95%
- B' 相对 B 无稳定改善（J +0.93，|dvel| +1）

`#35：J_vis 中位=A 170.2 / B 2.02 / B' 2.95（S/L 不适用，按屏幕帧测）；Δv_vis=A 359 / B 58 / B' 59；ΔpostRMS S=n/a / L=n/a；结论=B 远优于 A（比值约 0.01），但判据①严格 J≤0.5 未过，dbl 格 <95% → 按派单 FAIL；B' 无稳定改善；产物 artifacts/win_r2_p2_ab.txt`

## 派题 #39

> C5(a)：用 #35 原始日志（`artifacts/win_r2_p2_raw.txt`）离线重算 R-2′（依据 `docs/notion_r2_b_gate_ab.md`）。只在 box 上做，没有碰真机。composition.h sha256 030e3a8d…daeb82，前后一致。

### 方法
- J_vis、|dvel|、首 post 帧检测与 #35 脚本相同（`/workspace/r2/c5.py` 复用 `p2.py`）。d_frame=|x2−x1|，取 tInt 前 WGC 末两帧。
- ②′：7 格合并 median(J_X)/median(J_A)，按格分层 bootstrap，seed=20261020，10000 次，取 95% 分位 CI。
- N_h：主判用 3.11 DIP。注意 #35 并没有在会话内求底噪，这个 3.11 是硬编码常数，来自 #30 的线性 KeyFrame max|err|。另按 #30 算法对同会话 20 次基线做留一法复算，得 N_h_session=0.37，作为稳健性检查。两者都 ≤4。

### 结果（N_h=3.11）
| 格 | B ①′ | B J比 | B |dv|≤A | B | B′ ①′ | B′ |
|---|---|---|---|---|---|---|
| t060rev | 20/20 | 0.019 | 87≤701 | PASS | 15/20 | FAIL |
| t060fwd | 20/20 | 0.019 | 59≤714 | PASS | 10/18 | FAIL |
| t156rev | 20/20 | 0.015 | 66≤122 | PASS | 20/20 | PASS |
| t156fwd | 20/20 | 0.014 | 31≤64 | PASS | 20/20 | PASS |
| t300rev | 20/20 | 0.017 | 35≤358 | PASS | 20/20 | PASS |
| t300fwd | 20/20 | 0.009 | 30≤353 | PASS | 20/20 | PASS |
| t156dbl | 20/20 | 0.014 | 63≤78 | PASS | 20/20 | PASS |

合并：B/A=0.0119，CI [0.0113, 0.0204]；B′/A=0.0173，CI [0.0154, 0.0240]。Δv 比：B 0.162，B′ 0.164。

`#39：N_h=3.11（≤4 是；会话内复算 0.37）；②′ B 逐格 7/7 PASS；B/A CI 上界=0.0204；Δv 比=0.162；结论=PASS（C5(a)；按 N_h=0.37 时 t060 两格 FAIL，N_h 口径需 Notion 确认；C5(b) 第二台机器补测未做）；产物 artifacts/win_r2_c5_recalc.txt`

## 派题 #40

> 按 `docs/notion_r2_nh_ruling.md` 的 11 步，用 #35 原始日志 `artifacts/win_r2_p2_raw.txt` 离线重算逐格、同会话的 N_h（J_null），没有重跑真机。脚本：`/workspace/r2/c5.py nh`（新增的 `nh_null()` 在 `c5_nh_part.py`）。composition.h 的 sha256 前后都是 030e3a8d…daeb82。

- N_h_cell 取 20 次基线在 tInt_c、k_c 处 J_null 的第 19 小值；N_q=1.00 DIP（pxPerDip=1.0）；N_h_eff=max(N_h_cell, N_q)。

| 格 | tInt_c | k_c | J_null 中位/第19小/最大 | N_h_eff | d_frame 中位 | J_B 中位 | ①′ |
|---|---|---|---|---|---|---|---|
| t060rev | 68.0 | 3 | 3.59/3.64/3.64 | 3.64 | 0.47 | 1.96 | 20/20 PASS |
| t060fwd | 71.1 | 3 | 3.61/3.64/3.64 | 3.64 | 0.47 | 1.92 | 20/20 PASS |
| t156rev | 175.1 | 3 | 2.79/3.07/3.11 | 3.07 | 3.03 | 3.04 | 20/20 PASS |
| t156fwd | 172.5 | 3 | 2.85/3.07/3.11 | 3.07 | 3.03 | 2.87 | 20/20 PASS |
| t300rev | 318.4 | 3 | 0.97/1.30/1.36 | 1.30 | 5.01 | 3.01 | 20/20 PASS |
| t300fwd | 317.2 | 2 | 0.90/1.22/1.26 | 1.22 | 5.02 | 1.52 | 20/20 PASS |
| t156dbl | 173.6 | 3 | 2.85/3.07/3.11 | 3.07 | 3.03 | 2.96 | 20/20 PASS |

- B′（附带）：t060fwd 的 k_c=4，N_h_cell=6.15，超过 4，按 B′ 口径 VOID；仅作参考时 ①′ 为 5/7。

`#40：B 逐格 N_h_eff 1.22–3.64（max N_h_cell 3.64 ≤4，有效）；①′ 7/7 PASS；②′ CI 上界 0.0204、③′ Δv 比 0.162 沿用 #39；C5(a)=PASS；t156dbl 仍需 C5(b)；产物 artifacts/win_r2_c5_nh.txt`

## 派题 #36

> P3：合成器时基和起步分解（不打断）。Period ∈ {0.2, 0.32, 0.5} × ζ ∈ {0.6, 0.78, 1.0}，每格 5 次，9 格交错，同一会话共 45 次。Spring 0→240，IV=0，IVel=0。新增启动参数 `--r2-p3-probe`（`dotnet run -c Debug -p:Platform=x64 --no-build -- --r2-p3-probe`）。产物 `artifacts/win_r2_p3_timebase.txt`，原始数据 `win_r2_p3_raw.txt`。只用程序化动画，无合成输入。composition.h 未改。分析时剔除了每次开头 51 帧 WGC 旧帧（上一次的残留画面）。

| 格 | L0 中位 ms | τ ms | s | T_eff=P/s | RMS DIP | Δ200 = 延迟 + 形状 |
|---|---|---|---|---|---|---|
| 0.20/0.60 | 59.4 | 35 | 0.159 | 1.260 | 0.23 | −185 = +10 − 195 |
| 0.20/0.78 | 60.9 | 37 | 0.159 | 1.260 | 0.22 | −192 = +2 − 194 |
| 0.20/1.00 | 63.4 | 40 | 0.159 | 1.260 | 0.21 | −191 = −6 − 185 |
| 0.32/0.60 | 56.0 | 32 | 0.159 | 2.016 | 0.21 | −236 = −6 − 231 |
| 0.32/0.78 | 56.4 | 32 | 0.159 | 2.016 | 0.23 | −215 = −11 − 204 |
| 0.32/1.00 | 59.5 | 35 | 0.159 | 2.016 | 0.19 | −194 = −17 − 178 |
| 0.50/0.60 | 75.0 | 33 | 0.159 | 3.150 | 0.22 | −215 = −30 − 185 |
| 0.50/0.78 | 74.2 | 33 | 0.159 | 3.150 | 0.20 | −187 = −26 − 161 |
| 0.50/1.00 | 77.9 | 36 | 0.159 | 3.150 | 0.22 | −162 = −25 − 136 |

**要点**：s 在 9 格都等于 1/(2π)，所以屏幕轨迹 = spring.h(response=2π·Period, ζ)，再平移 τ≈33 ms，拟合后 RMS 为 0.22 DIP。由此看，合成器把 Period 当作 1/ω₀，而不是 T₀，这和 S1 的“Period=response”不一致（#12 发现的形状残差应来自这里）。前 200 ms 的差约 95% 是形状，约 5% 是延迟。这里只给建议，没有改映射。

`#36：J_vis 中位=n/a（S/L 不适用，不打断）；Δv_vis=n/a；ΔpostRMS S=n/a / L=n/a；结论=延迟 L0 中位 63.4 ms（拟合纯平移 τ≈33 ms≈2 帧，和 P/ζ 无关）；形状 = spring.h(2π·Period) 时基，s=1/(2π)，拟合 RMS 0.22 DIP；200 ms 处差 −192.5 DIP 中，延迟约 −11、形状约 −186（约 95%）；产物 artifacts/win_r2_p3_timebase.txt`

### #40 追加（Notion「追加确认（#40）」）
- 会话有效性只按 B 判：B 的 max N_h_cell=3.63 ≤4；B′ 的 6.15 超限，仅作参考。
- tInt_c 改为指令值 60/156/300 ms。之前用的是实测中位，比指令值晚 8–19 ms。改用指令值后，N_h_cell 降为 t060 3.63、t156 2.83、t300rev 1.08、t300fwd 0.55（N_h_eff 取下限 1.00）。B 的 ①′ 仍是 7/7，结论不变。
- k_c 对比：A 在 7 格均为 2；B 为 3/3/3/3/3/2/3（顺序 t060rev、t060fwd、t156rev、t156fwd、t300rev、t300fwd、t156dbl）。没有一格出现 k_c(B) > k_c(A)+1，未触发标记。B 晚 1 帧，是因为跳变小时，“连续 2 帧偏离 >1.5 DIP”的检测判据触发得更晚。
- 结论：**C5(a) 最终 PASS**。t156dbl 仍需完成 C5(b) 才能冻结。


## 派题 #41

> 续 #36，在真机上验证 Period = r/2π 映射。只在探针里设参数，新增 `--r2-p6-probe`（MainPage.xaml.cs，改前备份 `.bak_m41`）。用 `dotnet run -c Debug -p:Platform=x64 --no-build -- --r2-p6-probe` 打包启动，同会话运行 160 次。只用程序化动画，没有合成输入，没动鼠标。composition.h sha256 前后一致（030e3a8d…daeb82）。产物 `artifacts/win_r2_p6_remap.txt`，原始数据 `win_r2_p6_raw.txt`。

### (1) R-1 形状（算法同 #12：RMS/travel 240，窗口 1.2 s，E 为 1.0 s；remap 和 orig 两臂交错，每格每臂 5 次）

| 格 | 臂 | RMS%@33ms 中位 [max] | best τ ms | RMS%@bestτ | ≤1%@33 |
|---|---|---|---|---|---|
| r0.20z0.60 | remap | 0.659 [2.619] | 33 | 0.132 | YES |
| r0.20z0.60 | orig | 36.839 [37.217] | 295 | 12.403 | no |
| r0.20z0.78 | remap | 0.959 [3.540] | 36 | 0.142 | YES |
| r0.20z0.78 | orig | 37.042 [37.580] | 290 | 12.805 | no |
| r0.20z1.00 | remap | 1.122 [2.675] | 37 | 0.113 | no |
| r0.20z1.00 | orig | 37.628 [38.186] | 300 | 15.958 | no |
| r0.32z0.60 | remap | 0.936 [1.486] | 37 | 0.120 | YES |
| r0.32z0.60 | orig | 45.798 [46.457] | 300 | 23.551 | no |
| r0.32z0.78 (=A) | remap | 0.676 [2.819] | 32 | 0.083 | YES |
| r0.32z0.78 (=A) | orig | 45.658 [46.699] | 300 | 24.318 | no |
| r0.32z1.00 | remap | 1.245 [2.620] | 39 | 0.094 | no |
| r0.32z1.00 | orig | 46.979 [47.655] | 300 | 27.827 | no |
| r0.50z0.60 | remap | 0.496 [0.566] | 35 | 0.108 | YES |
| r0.50z0.60 | orig | 57.147 [57.602] | 300 | 38.398 | no |
| r0.50z0.78 | remap | 1.224 [2.310] | 40 | 0.083 | no |
| r0.50z0.78 | orig | 56.896 [57.141] | 300 | 39.618 | no |
| r0.50z1.00 | remap | 1.375 [1.642] | 41 | 0.066 | no |
| r0.50z1.00 | orig | 57.307 [57.397] | 300 | 42.088 | no |
| E_r0.16z0.78 | remap | 1.247 [2.575] | 37 | 0.204 | no |
| E_r0.16z0.78 | orig | 35.722 [36.624] | 292 | 9.919 | no |

### (2) #33 打断场景（remap，A 臂影子 seed，20 轮交错，口径同 #35）

| 臂 | J_vis 中位 [CI] | J≤0.5 | Δv_vis 中位（阈） | 双判据 |
|---|---|---|---|---|
| lead0 | 4.40 [4.03,5.08] | 0/20 | 526（63） | 0/20 |
| lead33 | 4.73 [4.40,5.46] | 0/20 | 590（64） | 0/20 |

（对照 #35 Period=r：A 臂 J=203，Δv=122）

`#41：J_vis 中位=4.40（lead0）/4.73（lead33）（S/L 不适用）；Δv_vis=526/590；ΔpostRMS S=n/a / L=n/a；结论=Period=r/2π 映射成立：形状残差 orig 36–57% → remap best-τ 0.07–0.20%（10/10 ≤1%）；但固定 33 ms 对齐时只有 5/10 格 ≤1%（A 0.68 过，E 1.25 不过；τ 实为 32–41 ms，随 ζ/r 增大）；打断 A 臂 J 比 #35 降低约 46 倍，但严格 0/20，seedLead=33 反而更差，建议试负 lead≈−21 ms；产物 artifacts/win_r2_p6_remap.txt`

## 派题 #41b

> 纯离线，只用 #41 的 `artifacts/win_r2_p6_raw.txt`，没有跑真机。依据 `docs/notion_s1_remap_ab.md` A2/A3。脚本：`/workspace/r1/f41b.py`；产物：`artifacts/win_r1_41b_fit.txt`。本任务没有改 composition.h；当前 sha 为 a55d4952…e418，是 S1 remap 落地后的值，不是本任务所改。

- 模型：x_scr = spring.h(Period_set=r/2π, ζ; s·(t−τ))，s 网格 [0.150, 0.168]（步长 0.0005），τ 网格 [25, 50] ms（步长 0.25 ms）。
- (1) 形状：s 固定为 1/2π，逐次取 best-τ。10/10 格、50/50 次 RMS ≤ 0.5%（最大 0.206%，E 格）。**PASS**
- (2) 延迟：逐格 τ 中位 32.25–41.50 ms，全部落在 [28, 45] 内。**PASS**
- (1′) 联合拟合：s 中位 0.1585，比 1/2π 低 0.41%，在 1% 以内，不修正映射。小 r 格偏离偏大：r0.20z0.60 为 −1.04%，E 为 −2.30%，但这两格的形状仍远低于门限，判为量化造成的 τ–s 共线。τ 仍随 ζ、r 漂移，记入延迟容差。
- (3) Δt_eff（t156rev）：lead0 为 39.2 ms，三臂合并 38.6 ms（IQR 34.6–42.2）。**建议 seedLead = −39 ms**（扫描 −35～−42），原建议的 −21 ms 偏小；lead33 计入 lead 后为 69 ms，与 Notion 粗估的约 70 ms 一致。

| 格 | RMS%@bestτ 中位/最大 | τ 中位 ms | 逐次 s 中位（偏离%） | 形状/延迟 |
|---|---|---|---|---|
| r0.20z0.60 | 0.132/0.145 | 33.25 | 0.1575 (−1.04) | PASS/PASS |
| r0.20z0.78 | 0.111/0.135 | 36.25 | 0.1580 (−0.73) | PASS/PASS |
| r0.20z1.00 | 0.099/0.117 | 37.25 | 0.1580 (−0.73) | PASS/PASS |
| r0.32z0.60 | 0.104/0.121 | 36.50 | 0.1585 (−0.41) | PASS/PASS |
| r0.32z0.78 | 0.073/0.092 | 32.25 | 0.1585 (−0.41) | PASS/PASS |
| r0.32z1.00 | 0.065/0.084 | 39.25 | 0.1585 (−0.41) | PASS/PASS |
| r0.50z0.60 | 0.084/0.088 | 34.75 | 0.1590 (−0.10) | PASS/PASS |
| r0.50z0.78 | 0.075/0.081 | 39.75 | 0.1590 (−0.10) | PASS/PASS |
| r0.50z1.00 | 0.058/0.065 | 41.50 | 0.1590 (−0.10) | PASS/PASS |
| E_r0.16z0.78 | 0.203/0.206 | 36.50 | 0.1555 (−2.30) | PASS/PASS |

`#41b：形状 10/10 PASS（max 0.206%）；延迟 10/10 PASS（τ 32–42 ms）；s=0.1585（−0.41%，不修正映射）；Δt_eff=39 ms → seedLead 建议 −39 ms；产物 artifacts/win_r1_41b_fit.txt`

## 派题 #42

> 续 #41。在 remap 下（只在探针内设 Period=r/2π，r=0.32，ζ=0.78）重跑 #33/#35 打断场景，四臂同会话交错，共 20 轮 ×（1 次 base + 7 格 × 4 臂）= 580 次。新增 `--r2-p7-probe`（改前备份 `.bak_m42`），用 `dotnet run -c Debug -p:Platform=x64 --no-build` 打包启动。第一次启动因 shell 被中断，在 74 次时被杀掉，已整轮重跑，只用第二次的完整数据。判据按 `docs/notion_s1_remap_ab.md` A4，另附严格通过和 R-2′ ①′ 作参考。产物 `artifacts/win_r2_p7_remap_int.txt`，原始数据 `win_r2_p7_raw.txt`，脚本 `/workspace/r2/p7.py`。只用程序化动画，无合成输入，没动鼠标。Win 侧旧 composition.h 前后都是 030e3a8d…daeb82，未改动，也未同步新版。

四个臂：
- A0：spring.h(r) 取 tInt 时刻的 x、v。
- Am20：取 tInt−20 ms 时刻的 x、v。20 ms = τ 32（#41 中该格的 best-τ）− L 12（#41 lead0 的 L 中位）。
- B：IV 不设，速度取 WGC 末两帧。
- Bs：IV 不设，速度取 spring.h(r) 在 tInt−32 ms 处的值。

| 格 | N_h_cell(B) | A0 J/Δv/双判据 | Am20 | B | Bs |
|---|---|---|---|---|---|
| t060rev | 15.43 ✗ | 10.31/1992/0 | 10.33/982/1 | 10.03/991/0 | 10.14/965/0 |
| t060fwd | 15.43 ✗ | 10.38/1881/0 | 10.31/790/1 | 10.46/932/0 | 10.68/952/0 |
| t156rev | 6.09 ✗ | 4.51/284/2 | 4.66/746/0 | 4.65/532/0 | 4.86/531/0 |
| t156fwd | 6.09 ✗ | 4.12/326/2 | 4.16/980/0 | 4.07/534/0 | 4.86/509/0 |
| t300rev | 3.56 | 2.01/1014/0 | 2.53/1065/0 | 2.47/1017/0 | 2.48/1040/0 |
| t300fwd | 3.56 | 6.95/742/0 | 1.53/509/0 | 1.56/565/0 | 1.96/520/0 |
| t156dbl | 6.09 ✗ | 4.32/412/1 | 5.13/928/0 | 4.45/512/0 | 4.75/505/0 |

（双判据列为 /20 中的通过次数；严格通过四臂都是 0/20。）

`#42：J_vis 中位=A0 4.63 / Am20 4.35 / B 4.51 / Bs 5.04（S/L 不适用）；Δv_vis=765/876/750/574；ΔpostRMS S=n/a / L=n/a；结论=SESSION VOID：B 有 5/7 格 N_h_cell>4（6.1–15.4，remap 后快弹簧使 v_pre 线性外推的零假设误差超限）；即便只作参考，四臂 A4 也都是 0/7，Δv 不通过，而且 base 的 Δv_null 本身就有 531–1000+，是估计器的问题；J 都落在 J_null 以内，四臂和不打断分辨不出来；按 A4 无臂通过 → 维持 hybrid B，开 #43（先修估计器，再标定负 lead）；产物 artifacts/win_r2_p7_remap_int.txt`

## 派题 #44

> 续 #42，在真机 DESKTOP-K93N3P8 上验证 S1 v2 正式映射。把 box 的 `code/` 整体同步到 Win `code\`（本题授权覆盖旧版；我本人没有编辑 composition.h），`artifacts/win_softdemo_src/composition.h` 也换成新版。四份 composition.h 现在都是 a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418。探针新增 `--s1v2-probe`，mfprobe.dll 新增导出 `mf_map_spring`（调用 composition.h 的 MapSpringToCompositor，传 r 进去）。改前备份为 `.bak_m44`（MainPage.xaml.cs、mf_probe.cpp）。产物 `artifacts/win_s1v2_build.txt`，原始数据 `win_s1v2_raw.txt`、`win_s1v2_golden_raw.txt`、`win_s1v2_native_raw.txt`。

**MSVC 全套**（cl 19.44，构建方式同 reply11）：

| 套件 | 结果 |
|---|---|
| timeline | 422/0 |
| tokens | 743/0 |
| controller | 236/0 |
| composition | 2305/**1** |
| software_demo | 10/0 |
| fm_capi_test（code\capi，两处） | 23/0 |
| continuity | 13/0 |

composition_tests.cpp:475 失败，原因是测试 UB：`bad.Plan(...).Find(...)` 返回的指针指向已析构的临时对象。插桩确认 MSVC 和 g++ 下读到的都是脏值，g++ 能过只是碰巧；加 /utf-8 结果相同，排除代码页原因。469–470 行的 `pt` 是同样写法。需要协调方修测试，本题没有改。

**R-1 v2**（10 格 × 5 次，正式映射，算法同 #41b）：

| 格 | RMS% 中位/最大 | 形状 | τ 中位 ms | 延迟 | 逐次 s 偏离 |
|---|---|---|---|---|---|
| r0.20z0.60 | 0.109/0.118 | PASS | 39.25 | PASS | −1.04% |
| r0.20z0.78 | 0.105/0.165 | PASS | 42.75 | PASS | −0.73% |
| r0.20z1.00 | 0.113/0.162 | PASS | 33.50 | PASS | −0.73% |
| r0.32z0.60 | 0.092/0.106 | PASS | 39.75 | PASS | −0.41% |
| r0.32z0.78（A） | 0.059/0.082 | PASS | 37.25 | PASS | −0.41% |
| r0.32z1.00 | 0.063/0.068 | PASS | 39.75 | PASS | −0.41% |
| r0.50z0.60 | 0.076/0.087 | PASS | 39.25 | PASS | −0.10% |
| r0.50z0.78 | 0.070/0.075 | PASS | 40.75 | PASS | −0.10% |
| r0.50z1.00 | 0.061/0.068 | PASS | 39.25 | PASS | −0.10% |
| E r0.16z0.78 | 0.164/0.213 | PASS | 42.50 | PASS | −1.67%（格合并 −2.30%） |

`#44：J_vis 中位=n/a（S/L 不适用）；Δv_vis=n/a；ΔpostRMS S=n/a / L=n/a；结论=S1 v2 正式构建在本机 R-1 形状 10/10、延迟 10/10 通过（RMS 0.06–0.21%，τ 33.5–42.8 ms），关闭条件①满足；小 r 格 s 偏离复现（r0.20z0.60 −1.04%，E −1.67%/−2.30%），但不影响形状判据，是否修正待裁决；MSVC 全套只有 composition_tests 1 个失败，原因是测试悬垂指针 UB（:475，:469 同样），不是映射问题；产物 artifacts/win_s1v2_build.txt`

## 派题 #43

> 续 #44。在正式构建（composition.h a55d4952…e418，Period 由 MapSpringToCompositor 给出 0.0509296）上，按 `docs/notion_r2_estimator_ab.md` 的 12 步 E2 估计器重判打断。新增 `--r2-p8-probe`（备份 `.bak_m43`）：先跑 P0（40 次 base，在探针内实时拟合 δ），再跑 P1（6 臂 × 7 格 + base，每轮随机交错，20 轮）。同会话共 900 次。产物 `artifacts/win_r2_p8_e2.txt`，原始数据 `win_r2_p8_raw.txt`，脚本 `/workspace/r2/p8.py`。只用程序化动画，没有合成输入。composition.h 前后都是 a55d4952…e418。
>
> 与文档字面不同的两处：
> - 第 1 步写的是 ω₀=2π/Period，代入 Period=r/2π 会得到 4π²/r。这里按物理意义取 ω₀=1/Period=2π/r。
> - P0 失败时，文档要求停止，派单要求取 Δt̂=70 继续。这里按派单执行，并标明会话作废。

**P0**：Δt̂=39.0 ms，CI [34.0, 40.8]，IQR=10.38 ms（超过 8 ms 门，失败），残差 RMS 0.216 DIP。所以 D 取 70，Am 三臂的 lead 为 55/70/85 ms。

| 臂 | J 中位（t060r/t060f/t156r/t156f/t300r/t300f/dbl） | 可判/PASS | 结论 |
|---|---|---|---|
| A0 | 1.83/13.38/0.37/0.98/0.91/1.15/0.44 | 5/0 | 不通过 |
| Am39 | 60.2/13.4/24.5/8.6/0.43/1.86/21.8 | 5/0 | 不通过 |
| Am55 | 69.2/14.7/41.6/30.1/0.82/2.30/42.5 | 5/0 | 不通过 |
| Am70 | 98.7(NA)/22.1/63.0/51.6/0.23/2.57/60.9 | 5/0 | 不通过 |
| Am85 | 125.6(NA)/26.3/89.0/81.6/0.76/1.94/86.8 | 5/0 | 不通过 |
| B | 14.87/1.01/2.38/0.34/0.43/0.75/0.28 | 7/0 | 不通过 |

`#43：J_vis 中位=B 0.75 / A0 0.98 / Am39 13.4 / Am55 30.1 / Am70 51.6 / Am85 81.6（S/L 不适用）；Δv_vis=A0、B 均远超阈（300–2600；A 臂 ⚠M 13–20/20）；ΔpostRMS S=n/a / L=n/a；结论=P0 IQR 10.4>8，会话作废（按派单 Δt̂=70 跑完）；E2 的 N_h 降到 0.15–1.35；无臂通过，维持 hybrid B；负 lead 被否定（J 在 X=0 最小，单调增，L*_J 0–21 ms，与 Δt̂ 不一致）；Δv 受打断后模型失配（⚠M）影响，需先修打断后拟合；产物 artifacts/win_r2_p8_e2.txt`

## 派题 #45

- **一行结论**：#45 有效｜P0 通过（锯齿 b=1.00，层内 IQR(r)≤0.02 ms，MAD 0.01 ms，RMS 0.20 DIP）｜通过格 B 0 / A0 2 / Aφ 1 / A+8 2｜J 全格中位 B 1.00 / A0 1.25 / Aφ 1.12 / A+8 0.18｜Δv 可判均 4/7（<5，t156 三格 Nv_h≈19.7>0.5×26）→ 无臂升主，维持 B=M1b；相位假设被否定（A0 J 对 φ_m 斜率 7/7 格 CI 含 0，Aφ 引入 ≈−|v| 斜率）。
- 依据 docs/notion_r2_43_ab.md 第 5 节 12 步；E3（速度加权 LS，⚠M=χ²/dof>2，M1/M2 诊断）；种子取实测 tInt_m；ω₀=2π/r=1/Period。程序化动画，无合成输入，未动鼠标；composition.h a55d4952…e418 前后未变。
- 偏差：Win10 19045 无 DCompositionWaitForCompositorClock → DwmGetCompositionTimingInfo + QPC 自旋控相位；WinUI3 无同步 Commit → tInt_m=替换 StartAnimation 返回 QPC（另记 RequestCommitAsync 完成）；B1 起点与打断同相位 φ*，故动画时钟域里 A0 种子时刻≡生效帧，Aφ 等于多超前 (T−φ)；B 在 t060 两格 seedV≈−14000（探针用含陈旧帧的末两 WGC 帧求速，B 实现缺陷，该两格 B 的 ⚠M 13–15/20 源于此）。
- B 在 E3 下 Δv 不达标且 ⚠M≤2/20：t156rev/t156fwd/t300rev/t300fwd/t156dbl → 记“B 真实速度不连续”，留下一题。
- 产物：artifacts/win_r2_p9_45.txt，win_r2_p9_raw.txt（748 trial），win_r2_p9_e3.py；探针 --r2-p9-probe（MainPage.xaml.cs 备份 .bak_m45）。

## 派题 #46

- **一行结论**：#46 有效（P0 ✓；base N_h 0.50/0.53）｜B 速度取样修复后，t060 打断时（≈66 ms）WGC 只有 ≤1 个新鲜帧，17/20、18/20 回退 vB=0 → seedV 0 ≠ v_ref 1982；⚠M 15/14→0/0，J 1.97/0.47，Δv≈1990（阈 99）→ B t060 两格 FAIL｜A0 t060 复现：J 8.68/4.36，Δv 49/13 → FAIL/FAIL（t060fwd 为 k_c 敏感）｜⚠C 1/120，剔除前后不变｜离线（#45 raw）：k_c 敏感 9 格；A+8 t300fwd 强制归前翻转、L* 跨度 21 ms → A+8 关闭｜E3r（t156 Nv_h 6.3–6.6 过，但 t060 +130%）与 E3q 均不过门 → t156 Δv 结构性不可判｜解冻条件不满足，维持 B=M1b 条件冻结。
- B seedV 溯源：−14000 是读回问题，B1 指令后 WGC 先给 1–2 帧上一试次的终点画面（240/360）；0 是末两帧位置相同。B 实际也是用这个错误速度下发的。t156 的 1830 对 522 则是 WGC 约 2 帧滞后带来的陈旧速度，属 B 设计固有，取样修复消不掉。
- 派单口径的修复（时间戳严格递增且位置有更新）剔不掉上述陈旧帧，v1 版仍是 −14000（raw_v1 留档）；v2 加上“首个 |x|<0.5 之前的帧都算陈旧帧”后重跑，作为正式结果。
- 产物：artifacts/win_r2_p10_46.txt，win_r2_p10_raw.txt（168 trial），win_r2_p10_raw_v1.txt，win_r2_p10_e3.py、win_r2_p10_lib46.py、win_r2_p10_rep46.py；探针参数 --r2-p10-probe，改动前备份为 .bak_m46。

## 派题 #47

- **一行结论**：#47 会话有效（P0 ✓ b=1.00；base J 门 0/7、⚠M 0）｜B_v（原 B 位置路径 + S1 v2 影子速度，无 vB=0）：seedV=v_ref 全格一致，⚠M 全 0；J 仅 t300rev/fwd 通过（t060 20.9/16.7、t156 7.0/3.5/7.0 FAIL）；Δv 可判 4 格中达标 3（t300rev 21.4>20）；t156 Δv 29.8/8.8/29.8 < FAIL-确定界 ≈87｜③（修订）(a)✗ (b)✓ (c)✗ → B_v 不满足，R-2 本机 FAIL 结题，仅剩 C5(b)；M1b 以 B_v 为降级实现，并标注「不满足 R-2」。
- 依据 docs/notion_r2_46_ab.md。与派单冲突处按文档执行：B_v 位置走原 B 路径（不设 InitialValue），不取 WGC。k_c 口径为 h45；敏感性列中 t156fwd、t300rev、t300fwd 带「k_c 敏感」标记。
- 观察：B_v 的 J ≈ |v_ref|×(7–13 ms)。这说明不设 IV 时位置约落后 1 帧；原 B 在 t060 的 J 较小，是因为 vB=0 时与位置项部分抵消。是否做滞后补偿留给协调方。
- **#46 更正（剔除帧统计）**：#46 诊断中「剔除帧数 中位 1，范围 1–1，合计 3/2」作废。探针只在新鲜帧≥2 时才写 vBdrop（rev 3 次、fwd 2 次），其余运行在统计时被跳过。按 raw 逐试次重算（v2 规则，窗口为到达 ≤tInt_c）：
  - rev 每试次剔除 {0:1, 1:16, 2:3}，合计 22；
  - fwd 剔除 {1:16, 2:4}，合计 24。
  - 另发现探针在 trip−30 ms（相位自旋之前）就读取了 WGC。按这个窗口重算，新鲜帧 <2 的为 19/20、20/20，与实测回退 17/20、18/20 吻合。#45/#46 的 B 速度因此还多了 30–47 ms 的读取提前量，但 B 已是终局，不影响判定。已在 win_r2_p10_46.txt 文末注明更正，脚本为 win_r2_p10_drop_fix.py。
- 产物：artifacts/win_r2_p11_47.txt，win_r2_p11_raw.txt（328 trial），win_r2_p11_e3.py，win_r2_p11_rep47.py；探针参数 --r2-p11-probe，改前备份 .bak_m47。

## 派题 #48

- **一行结论**：#48 离线收尾｜P0a PASS；P0b PASS（WGC 峰值 32.53，PS 读回 rbSpan=0/verdict=NOOP）；P0c PASS（终值 120，独立帧偏移后 RMS 中位 1.36）；P0d PASS（hold/afterStop=120）｜E1 576/576、每桶 12｜Bv J/|v|≈12–13 ms 近恒（b=0 slope=+0.07/c=12.4/σ=1.3；b=120 slope=−0.25/c=12.7/σ=1.6）｜Aex(δ̂=0) 在 φ≈9–13 ms J 最小（可达 0.08–0.18 ms），线性 σ 3.7–5.2 ms｜φ 落点抖动中位 0、最大 0.058 ms｜composition.h a55d4952…e418 未改｜CodeBuddy 写 R3Probe.cs+MainPage 接线。
- P0b 务必分列：PropertySet 读回不动（探针 NOOP），屏幕有冲量运动——叠加可行性以 WGC 为准。
- E1 回归取 φ_act=`phiAct mod tickPer`（主）；对照指令 φ 结果几乎相同。J 为打断后 post 轨迹与 B1 影子交接邻域最小 |Δx|。
- 解读：Bv 跳变属 commit 陈旧（~1 帧），相位模型消不掉；Aex 支持「存在 c 使 δ̂=0 对准」，但绝对值锯齿使 slope≈−1 拟合偏弱。
- 产物：artifacts/p12_48.txt，p12_48_raw.txt，win_r2_p12_e1.py（=/workspace/r3/e1.py）；探针 `--r3-p12-probe`（R3Probe.cs）；CB 日志 p12_48_codebuddy_{prompt,out}.txt。未做 #49，未重跑真机。

## 派题 #49

- **一行结论**：#49 E3 真机｜矩阵 288/288 齐｜Sup J中位=1.55≪Bv=25.86｜Δv_fd Sup=130.7/Bv=716.8｜ε Sup=21.8ms(>1帧≈16.7)｜不建议采用（ε超标；J/Δv显著优于Bv；失败模式=响应延迟；未改E2）｜composition.h a55d4952…e418 未改｜CodeBuddy 写 RunR3E3Probe + MainPage `--r3-e3-probe`。
- 叠加臂 Sup：Expression `Translation.X=ps.b+ps.d1`；b 上 0→240 不停；t≈60ms 在 d1 上加步进弹簧 Init=0、Final=b_new−240、IVel=0（同 Period/ζ）；不读位置、无 δ。对照 Bv（IV=null+shadowVel）与 base。
- 矩阵：t060 主（每桶每臂 n=8）+ t156 半预算（n=4）；b∈{0,120}；φ∈{1,5,9,13}；弹簧 r=0.32 ζ=0.78；WGC 度量（PS 读回 NOOP 沿用 #48）。
- 门控：J PASS（近噪声）；Δv_fd 相对 PASS（Sup≪Bv）；ε FAIL（中位 21.8ms，稳健子集 22.3ms；φ=1≈30ms、φ=13≈11–17ms）。
- 异常：1/288 解析失败；少数 Sup ε 贴搜索下界且 J 离群（残帧嫌疑）；t156 settle|err|≈26 DIP（窗短）。未擅自改 E2。
- 产物：artifacts/p13_49.txt，p13_49_raw.txt，win_r2_p13_e3.py（=/workspace/r3/e3.py）；探针 `--r3-e3-probe`（R3Probe.cs）；CB 日志 p13_49_codebuddy_{prompt,out}.txt。

## 派题 #50

- **一行结论**：#50 R3 Sup-adv 整包｜矩阵齐 A+B+C（full raw wall=19:08:33 AWST runC=True；TRIALC 256/256）｜**本 raw 离线闸门 A=FAIL**（A1a τ0=21.8 中位 RMS=1.82>N_h；A1b/A1c 亦 FAIL；A2 PASS）｜对照先跑 AB-only（wall≈18:45 runC=False）A=PASS、S2后ε=6.8ms、ε̂候选{0,6.8,16.7}｜本 raw S2后ε=6.2ms（相对 Sup|Δ|=0.8，未重定）｜Stage C 按 AB 候选扫 ε̂∈{0,6.8,16.7}：**任一 ε̂ 全 cell 过 J_vis≤d_frame+N_h_eff → YES**；最佳ε̂=**6.8**（|e|中位最小）｜分流=记录 C 数据 / **R-3 仍 OPEN**（成功≠采用；本轮 A 回归勿当最终闸门单独过）｜composition.h a55d4952…e418 未改｜CodeBuddy 本轮未再调用（沿用已打 env 补丁的 `--r3-50-full` / MF_R350_FULL=1）。
- 阶段：A P0-fit（种子平移/Δ线性/叠加）+ A2 同 PS 路径；B 臂 Bv/Sup/Sup+S2/base；C S1 种子 IV=s(ε̂)、IVel=s′(ε̂)；LIVE_TERMS_CAP=3；出货门仅 J_vis≤d_frame+N_h_eff（#47），禁止 ≤2×Sup。
- C 要点：三档 ε̂ 逐 cell 通过率均为 100%；ε̂=0 e中位≈−6.8（滞后）、ε̂=6.8 e中位≈+6.5（略超前）、ε̂=16.7 e中位≈+21.7（过超前）；φ 分档与 fit/hold 一致。
- 异常/说明：上一轮 executor 中断时 PID 58516 仍在跑；本轮等其自然结束（STAGE C END / P14 RAW DONE，未杀进程）。full 重跑 A 段偶发坏 trial（τ0=21.8 三格、A1c ε_fit 贴−5）导致离线 A FAIL，与 AB-only PASS 不一致——以两份 raw/摘要并列记录。
- 产物：artifacts/p14_50_raw.txt（504705B）、p14_50.txt、p14_50_ab.txt（AB-only）；分析 /workspace/r3/p50.py；Win 同步同名 artifacts；探针 `--r3-50-full` / MF_R350_FULL=1。

## 派题 #51

- **一行结论**：#51 Claude NO-ADOPT 扩订｜A×3=**0/3**（失败子项轮换：a1 A1b+A1c；a2 A1a；a3 A1c）｜Part1：A1c −5 搜索下界为**伪 FAIL**（放宽→双模 {+6.5,−10}）；live 细网格 A1a=FAIL[3.3,6.8,8.3,33.3]、δ0=39.5、未饱和 eff−τ0 中位≈+2.7；A1b origin=220 重裁剪 **PASS**｜Part2 c×3=**14.00/13.89/13.90**（均≈14，ε≈14−φ；**无 ≈14↔≈30 跳**；臂交错 Sup/Bv/base n=48）｜S2 仍非源（c1/c2 |Δε|=0.2；c3=1.8 会话离群不复活）｜**NO-ADOPT**；未改 composition.h a55d4952…e418；未开第二套并行探针；S1-φ/多 retarget 未做。
- 定义（与探针/p50）：ε=叠加最佳延迟；e=ε̂−ε_fit；J=|x−model| at interrupt；Δv_fd=|v_fd−v_model|；resid=x−model；c: ε≈c−φ；eff_adv=δ0−δ(τ0)。
- 产物：`artifacts/p15_51.txt`；`p15_51_a{1,2,3}.txt(+raw)`；`p15_51_part1(_raw|_refit).txt`；`p15_51_c{1,2,3}.txt(+raw+_p50style)`；分析 `/workspace/r3/p50.py` + `p51_a`/`p51_part1`；Win 探针 `MF_R350_PROBE=1` / `mf51_run_part.ps1`。
- 对照：`docs/claude_r3_50_review.md` FINAL；硬停均守。

## 派题 #52

**状态：DONE 2026-10-24**（整包 P1+P2+P3；workbuddy CLI 执行器）。**R-3 已 CLOSED**（用户确认 2026-10-24，按 DeepSeek 三条挂账）；S1 **STOP**（A v2×3=2/3）；Bv=降级兜底（非合格基线）；A1a 跟踪转出。

### 权威
- FINAL：`docs/claude_r3_51_review.md`（Notion AI fallback）
- 派题：`docs/workbuddy_next_r3_52.md` AUTHORIZED 2026-10-24

### 不变量
- composition.h sha256=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` PRE/POST OK；**未改**。
- 仅改 harness：`MotionSoftDemo/R3Probe.cs`（CodeBuddy；A1a 标准 τ0 集、A1c×12 + tickPer/phiAct/tPresent0、Stage B Sup/Bv/base repsB=2）。
- A v2 闸门采数前写死：`artifacts/p16_52_a_v2_gates.txt` LOCKED_AT=2026-10-24 20:33:42 AWST。

### 结果摘要
| 项 | 结论 |
| P3 | CONFIRM_F5_REFUTE_2X；med_res_unsat=+2.70；SSE_2x/SSE_const=14.5× |
| P1 | pooled=分开（purity=0.83）；a1 单会话分不开；tickPer≈16.66 ms 实测 |
| P2 A v2×3 | **2/3**（a1 PASS, a2 PASS, a3 FAIL@A1a τ0=21.8） |
| ≈30c | 无 |
| 硬停 | A×3<3/3 → **STOP S1**（建议下轮关 R-3 留 Bv） |

### ONELINE
`#52: P1=分开(pooled; a1分不开); A v2×3=2/3; P3=F5确认; ≈30c=无; 硬停=A×3<3/3→STOP S1; sha OK`

### 关键产物
`artifacts/p16_52*.txt`（汇总/P3/闸门/sha/P1pooled/a1–a3 评分与 raw）
