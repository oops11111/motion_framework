# Z6 · AOT 宿主接入指引：让 FluidMotion-Motion 的 E1 ETW 事件在 PublishAot 宿主里发出来

> dev-only 指引，**未复核**。本文只写宿主侧怎么做，不改 L1 公开面、fm_*、E1（事件名、id、字段、provider GUID），也不改任何 `*_LOCKED.md` 或 `docs/api_v0_frozen.md`。对应已知限制 **K-AOT-ETW**（`docs/z5_closed.md` 第 9 节，pidan 2026-11-18 06:03 AWST 登记）。Z6 票 F4，分支 `wb/z6-f4`，基线 main `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`。

## 1. 结论先说

宿主工程只要（直接或间接）设了 `PublishAot=true`，就在宿主自己的工程文件里显式加上：

```xml
<PropertyGroup>
  <!-- PublishAot=true makes the SDK turn EventSource off (runtimeconfig IsSupported=false).
       Turn it back on so the FluidMotion-Motion provider (E1) can emit. -->
  <EventSourceSupport>true</EventSourceSupport>
</PropertyGroup>
```

然后按第 4 节查输出目录的 `*.runtimeconfig.json`，确认 `System.Diagnostics.Tracing.EventSource.IsSupported` 是 `true`。

- 这是**宿主侧**的运行时特性开关。L1 包（`FluidMotion.L1.dll`、`fluidmotion.dll`）不需要、也不应该为此改动。
- 不需要 ETW 的宿主可以不加；L1 的动效功能不依赖 ETW，关掉 EventSource 只是看不到 E1 事件。

## 2. 为什么

- 宿主设了 `PublishAot=true` 后，.NET SDK 把 `EventSourceSupport` 默认为 `false`（为了 AOT 裁剪体积），并在 runtimeconfig 里写入 `System.Diagnostics.Tracing.EventSource.IsSupported=false`。**普通 `dotnet build` 的 Debug 输出也会这样写**，不只是 publish。
- 结果：进程里 `EventSource.IsSupported` 为 false，所有 EventSource（包括 L1 的 `FluidMotion-Motion`，GUID `8f3a2c1d-6b4e-4a91-9c7d-2e5f8a1b3c4d`）都不发事件。进程内 `EventListener` 收不到，外部 ETW 会话也收不到。不报错，只是静默。
- 显式设 `<EventSourceSupport>true</EventSourceSupport>` 后，SDK 写入 `IsSupported=true`，E1 事件恢复。

## 3. 证据（Z5-T5 FluentHub 试点，库内原始记录）

FluentHub 工程本身设了 `PublishAot=true`（`Directory.Build.props`，见 [`pilot/fluenthub/BASELINE.md`](../pilot/fluenthub/BASELINE.md) 目标框架一行）。T5 只做了 `dotnet build -c Debug`，没有 publish，也没有生成 AOT 产物。

| 运行 | 宿主 csproj | runtimeconfig `EventSource.IsSupported` | 宿主内 EventListener 收到的 E1 | 原始记录 |
|---|---|---|---|---|
| run1（构建 1） | 未设 `EventSourceSupport` | `false`（PublishAot 带来的默认值） | `ETW_SUMMARY distinctNames=0`：0 个事件名；但入场动画照常执行（sink 调用 136 次） | [`pilot/fluenthub/z5_t5_motion_run1_etw_off.log`](../pilot/fluenthub/z5_t5_motion_run1_etw_off.log) |
| run2（构建 2） | 加了 `<EventSourceSupport>true</EventSourceSupport>` | `true` | `ETW_SUMMARY distinctNames=5`：AnimationLifecycle、FrameTick（只计数）、IdleTransition、Retarget、TokenLoad；Created=7=Released | [`pilot/fluenthub/z5_t5_motion_run2.log`](../pilot/fluenthub/z5_t5_motion_run2.log) |

- 原因分析和改动经过：[`pilot/fluenthub/T5_REPORT.md`](../pilot/fluenthub/T5_REPORT.md) 第 3.1–3.3 节、第 4.3 节。
- 侧叉 csproj 里的实际写法（带注释）：[`pilot/fluenthub/fluenthub_t5_side_fork.patch`](../pilot/fluenthub/fluenthub_t5_side_fork.patch) 第 12–13 行。
- 登记：[`docs/z5_closed.md`](z5_closed.md) 第 9 节 K-AOT-ETW，以及第 131 行附近的偏差 D6。
- 说明：run1 → run2 之间同时改了 GUI 测试脚本（`SetCursorPos` → `SendInput`，只影响悬停是否触发）。ETW 从 0 到 5 个事件名只对应 `EventSourceSupport` 这一项改动，见 T5_REPORT 第 3.2 节。

## 4. 自检：查 runtimeconfig

构建后，在宿主的**输出目录**（例如 `bin\x64\Debug\<tfm>\<rid>\`，以工程实际输出为准）找 `<程序集名>.runtimeconfig.json`，看 `runtimeOptions.configProperties` 里的这一项：

```json
"System.Diagnostics.Tracing.EventSource.IsSupported": true
```

- `false`：E1 事件不会发出，按第 1 节加开关后重建。
- 没有这一项：SDK 没写这个特性开关，运行时按默认值（支持）处理。
- PowerShell 一行自检（在宿主工程目录执行；路径用相对路径，不写本机绝对路径）：

```powershell
Get-ChildItem -Recurse -Filter *.runtimeconfig.json bin |
  ForEach-Object { $j = Get-Content $_.FullName -Raw | ConvertFrom-Json
    "{0}  IsSupported={1}" -f $_.FullName, $j.runtimeOptions.configProperties.'System.Diagnostics.Tracing.EventSource.IsSupported' }
```

- 也可以不构建，直接问 MSBuild 求值后的属性（.NET 8 及以上 SDK 支持 `-getProperty`），配置和平台要与实际构建一致：

```powershell
dotnet msbuild <宿主>.csproj -getProperty:PublishAot -getProperty:EventSourceSupport -p:Configuration=Debug -p:Platform=x64
```

  `PublishAot` 为 `true` 而 `EventSourceSupport` 为空或 `false`，就需要加开关。runtimeconfig 是最终依据。

## 5. 间接引入 PublishAot 的情况（pubxml / PublishProfile）

`PublishAot` 不一定写在 csproj 里，以下几种都算「设了」：

1. `Directory.Build.props` / `Directory.Build.targets` 里设（FluentHub 就是这样，见第 3 节）。
2. csproj 设 `<PublishProfile>…</PublishProfile>`，由 SDK 导入 `Properties/PublishProfiles/*.pubxml`，而 pubxml 里有 `<PublishAot>true</PublishAot>`。据 Z6 定稿 F9a 的记载，Files（`v4.3` = `cbfb4ba54a89632fbceb82ea94ada60a9bae3737`）的 `src/Files.App/Files.App.csproj` 设了 `PublishProfile=…win-$(Platform).pubxml`，pubxml 里 `PublishAot=true`。**这一条本票没有在本地核实**（F9a 还没做），以 F9a 基线为准。
3. 命令行 `-p:PublishAot=true` 或 CI 脚本传入。

处理方法相同：

- 把 `<EventSourceSupport>true</EventSourceSupport>` 写在宿主 csproj（或宿主自己的 `Directory.Build.props`），不要只写在某一个 pubxml 里，否则普通 build 和其他 profile 不生效。
- 如果某个 pubxml 显式写了 `<EventSourceSupport>false</EventSourceSupport>`，MSBuild 按导入顺序后写者生效，结果可能被覆盖。**以第 4 节 runtimeconfig 的实际值为准**。
- 按 Z6 定稿第四节，宿主开了 `PublishAot` 就先查 runtimeconfig；需要 ETW 就显式设 `EventSourceSupport=true`，并在报告里**记为偏差**。

## 6. 范围与未验证项

- 已有证据只覆盖 **`dotnet build`（JIT）输出**：runtimeconfig 的值和运行时 EventListener 的结果（第 3 节）。T5 **没有做 publish/AOT**，所以「真正 NativeAOT 发布产物里 E1 能否被外部 ETW 会话收到」**没有证据**。如果需要，另开票验证；按定稿，那不算采数，但要遵守「不计时」。
- 本文不涉及采数：不读 PresentStats 字段值（占位值），FrameTick 只计数。
- **L1 侧检测提示（可选代码）本次未做**：定稿允许在 `MotionSystem.cs` 内部加 `#if DEBUG` 的提示（例如检测到 `EventSource.IsSupported==false` 时输出一行诊断），但本票没有做，留待 pidan 或评审决定。要做的话须在 F7 之前，并按 T3 标准做（`docs/api_changelog.md` 登记 internal 改动、Debug/Release 各跑 CheckSurface、devsmoke 两配置 PASS、Release 公开面 dump 与 `pilot/fluenthub/surface_l1pack.txt` 只允许首行不同），之后 F7（及 F11）须重做。
