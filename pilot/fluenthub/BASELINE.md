# FluentHub 基线 · K93N3P8（Z5 T4）— 结果：**FAIL（原样构建失败，未启动）**
> 最新结论（2026-11-18 01:40 AWST，第 8 节）：**非原样基线 PASS**（`-p:NuGetAudit=false`；SDK 10.0.26100 由 pidan 本人安装；运行时 2.6.5 替代 2.0.x）。标题行的 FAIL 只指第 3 节原样构建。构建/运行日志在 K93 `%TEMP%`，未入库。

> composition.h 冻结；L1 公开面 / fm_* / E1 不改；不采数（dev-only，不计任何闸门会话）；PERF-CI HOLD；Success≠adopt；不合 main；不 force/amend/rebase；不改任何 *_LOCKED.md；系统级改动一律停下请示 pidan。
> 本分支只放本报告，不提交 FluentHub 源码；本地克隆不 fork 到公开处，不向上游提 PR。

- 派工：`workbuddy_z5_wave1_dispatch.md`（sha256 `75c9fceba0987d66d70e15fc73065bea13f01bdff060c595fff3164e1af80c99`）T4
- 时间：2026-11-18 00:51–01:10 AWST（Australia/Perth, UTC+8）
- 本地克隆：`C:\Users\admin\pilot\FluentHub`（`git clone https://github.com/0x5bfa/FluentHub`；只读取，未推送；`origin` 仍指向上游，**未向其推送任何东西**）

## 1. 源与标识

| 项 | 值 |
|---|---|
| FluentHub commit | `eb08f0dbcbb43880ab34038b0cb6742f0c0deca5`（main，2026-09-20 01:42:29 +0900 = 2026-09-20 00:42:29 AWST，"Implement StateLabel"） |
| WASDK | `Microsoft.WindowsAppSDK` **2.0.1**（`src/FluentHub/FluentHub.csproj` PackageReference）；`WindowsAppSDKSelfContained=false`（依赖框架包） |
| 打包标识 | `Package.appxmanifest`：`Identity Name="49462fluenthub-uwp.FluentHub"`（与预期一致），Publisher `CN=4E3C0825-4D00-4ECC-89F8-528E9200B125`，Version `0.11.0.0`；MSIX（`EnableMsixTooling=true`），`runFullTrust` |
| 目标框架 | `net10.0-windows10.0.26100.0`，TargetPlatformMinVersion 10.0.17763.0；`PublishAot=true`、`SelfContained=true`、`TreatWarningsAsErrors=true`（Directory.Build.props） |
| NuGet 源（nuget.config） | nuget.org + `pkgs.dev.azure.com/dotnet/CommunityToolkit/_packaging/CommunityToolkit-Labs/nuget/v3/index.json`（CommunityToolkit Labs 公开 Azure Artifacts 源，匿名可读，未要求登录；仅映射 `CommunityToolkit.Labs.*`） |
| .NET SDK | 10.0.300（无 global.json SDK 钉选） |

## 2. 前置检查（只读）

| 项 | 结果 |
|---|---|
| `Get-AppxPackage *fluenthub*` | **无**（未装商店版 / 任何 FluentHub 包）→ 不停 |
| WindowsAppRuntime 2.x | 框架包 `Microsoft.WindowsAppRuntime.2` **2.5.1.0（x64）**、**2.6.5.0（x64/x86）**；`MicrosoftCorporationII.WinAppRuntime.Main.2` 2.6.5.0；`MicrosoftCorporationII.WinAppRuntime.Singleton` 8002.6.4.0；`Microsoft.WinAppRuntime.DDLM.2.5.1.0-x6`。**字面上没有 2.0.x 版本**；同一主版本族（`Microsoft.WindowsAppRuntime.2`）的更高次版本已在，按 MSIX MinVersion 规则可满足 2.0.x 依赖（未验证，因构建未过）。本票**未安装任何运行时** |
| 开发者模式 | `HKLM\…\AppModelUnlock\AllowDevelopmentWithoutDevLicense=1`（开）；只读读取 |
| 会话类型 | `$env:SESSIONNAME=Console`；`query session`：`>console admin ID 1 Active`（另有 `services` ID 0）；非 RDP |

## 3. 原样构建（未改任何源码 / 工程文件）

命令（在克隆根目录）：

```
dotnet build src\FluentHub\FluentHub.csproj -c Debug -p:Platform=x64
```

结果：**BUILD_EXIT=1**（00:52:52–01:10:19 AWST，restore 耗时 17.41 分钟），0 警告 1 错误：

```
C:\Users\admin\pilot\FluentHub\src\FluentHub\FluentHub.csproj : error NU1903: 错误形式的警告: 包 "Microsoft.DotNet.ILCompiler" 10.0.8 具有已知的 高 严重性漏洞，https://github.com/advisories/GHSA-p3jp-9v83-76w9
未能还原 C:\Users\admin\pilot\FluentHub\src\FluentHub\FluentHub.csproj (用时 17.41 分钟)。
```

另有一条下载失败信息（未计为错误）：

```
未能从“https://api.nuget.org/v3-flatcontainer/microsoft.netcore.app.runtime.win-arm64/10.0.8/microsoft.netcore.app.runtime.win-arm64.10.0.8.nupkg”下载包“Microsoft.NETCore.App.Runtime.win-arm64.10.0.8”。
Received an unexpected EOF or 0 bytes from the transport stream.
```

分析（不下修复）：`PublishAot=true` 使 restore 引入 `Microsoft.DotNet.ILCompiler` 10.0.8；NuGet 漏洞审计把它报为 NU1903 警告，`TreatWarningsAsErrors=true` 把它升级为错误，restore 失败。FluentHub、Core、Octokit、Octokit.Generators 其余三个工程 restore 成功。FluentHub 源码 `git status` 干净（仅 `obj/` 被忽略产物）。

## 4. 启动 / 包注册 / 截图

- **未启动**（无构建产物）；**未执行 `Add-AppxPackage -Register`**，用户级包注册未改；**无截图**。
- 若日后注册松散布局，清理命令为：`Get-AppxPackage -Name 49462fluenthub-uwp.FluentHub | Remove-AppxPackage`（用户级）。本次无需清理。

## 5. 结论与停下请示点

- **T4 基线 = FAIL（原样构建失败于 restore：NU1903 被 TreatWarningsAsErrors 升级为错误）**。按规则如实记录，不改代码、不改工程、不换参数重跑凑数。
- 因 T4 基线未 PASS，**Z5-T5（FluentHub 试点接入）按第二波派工前置顺序不启动**。
- 请示 pidan（任选其一或另定）：
  1. 允许用**命令行属性**（不改 FluentHub 任何文件）重做基线构建，例如 `-p:NuGetAudit=false` 或 `-p:WarningsNotAsErrors=NU1903`（必要时 `-p:RuntimeIdentifier=win-x64` 以避开 arm64 运行时包下载），并在报告中把它记为「非原样」偏差；
  2. 或在本地侧叉中做最小工程改动（同上属性写入本地 csproj，不推送、不提 PR）；
  3. 或维持 FAIL，换 B 备份（GetStoreApp）另议。
- 运行时：字面 2.0.x 未装（见第 2 节）；本票未装任何东西。如 pidan 认为必须字面 2.0.x，则同样需要请示（不得自行安装）。

## 6. 本地副作用

- 新建 `C:\Users\admin\pilot\FluentHub`（克隆，含 `obj/` restore 产物）。
- NuGet 用户缓存 `%USERPROFILE%\.nuget\packages` 新增 restore 下载的包（含 WASDK 2.0.1、ILCompiler 10.0.8 等）。
- 构建日志：`%TEMP%\z5_wave1\t4\fluenthub_build_debug_x64.txt`（UTF-16）、`fluenthub_build_exit.txt`。
- 无包注册、无计划任务、无证书、无系统设置改动、无安装。

---

## 7. 非原样重做（pidan 裁决后）· 2026-11-18 01:19–01:21 AWST — 结果：**FAIL（构建失败，未启动）**

> 本节为**非原样**构建记录；上文第 1–6 节原样 FAIL 记录保持不变。
> 依据：pidan 2026-11-18 01:18 AWST 原话「T4 用命令行属性重做基线（如 -p:NuGetAudit=false），不改 FluentHub 文件，记非原样；2.6.5 可替代 2.0.x」。
> 运行时：按该裁决，已装 `Microsoft.WindowsAppRuntime.2` **2.6.5** 视为替代字面 2.0.x（本次未走到运行阶段，未实测）。

### 7.1 尝试 A1（唯一一次）

命令（克隆根目录，FluentHub 任何文件未改）：

```
dotnet build src\FluentHub\FluentHub.csproj -c Debug -p:Platform=x64 -p:NuGetAudit=false
```

结果：**BUILD_EXIT=1**（01:19:43–01:20:12 AWST，已用时间 00:00:28.93），0 警告 3 错误。

- NU1903 已不再出现；restore 通过（包已在用户缓存，无新下载；arm64 运行时包下载失败信息本次未出现）。
- `Octokit.Generators`、`Octokit`、`FluentHub.Core` 三个工程编译成功；`FluentHub` 失败。
- 错误（去重后）：

```
EXEC : error : Could not read the Windows SDK's Platform.xml at C:\Program Files (x86)\Windows Kits\10\Platforms\UAP\10.0.26100.0\Platform.xml [...\src\FluentHub\FluentHub.csproj]
...\microsoft.windows.cswinrt\2.2.0\build\Microsoft.Windows.CsWinRT.targets(293,5): error MSB3073: 命令“cswinrt.exe @...\cswinrt.rsp”已退出，代码为 1。
...\microsoft.windowsappsdk.winui\2.0.12\buildTransitive\Microsoft.UI.Xaml.Markup.Compiler.interop.targets(845,9): error MSB3073: 命令“XamlCompiler.exe ...input.json ...output.json”已退出，代码为 1。
```

### 7.2 原因（只读核实，不下修复）

- 工程目标 `net10.0-windows10.0.26100.0` 需要本机 **Windows SDK 10.0.26100.0**（`Platforms\UAP\10.0.26100.0\Platform.xml`）。
- K93 `C:\Program Files (x86)\Windows Kits\10` 下 `Platforms\UAP`、`Include`、`References` 均只有 **10.0.18362.0**；无 26100。
- 安装 Windows SDK 属于系统级安装（需管理员 / UAC），按停止条件**未安装，停下请示**。
- 未尝试 `-p:WarningsNotAsErrors=NU1903`（NU1903 已由 A1 消除，无需）与 `-p:RuntimeIdentifier=win-x64`（与本错误无关）；未尝试改 TargetFramework/TargetPlatformVersion（会改变目标平台，超出本次裁决范围）。

### 7.3 启动 / 包注册 / 截图

- **未启动**；**未执行 `Add-AppxPackage -Register`**，用户级包注册未改；**无截图**；无计划任务。
- 清理命令（仅供日后注册后使用）：`Get-AppxPackage -Name 49462fluenthub-uwp.FluentHub | Remove-AppxPackage`。本次无需清理。

### 7.4 结论与请示

- **T4 非原样重做 = FAIL**（缺 Windows SDK 10.0.26100.0）。如实记录，不改 FluentHub 文件、不换参数凑数。
- T4 基线仍未 PASS → **Z5-T5 不启动**（第二波前置顺序）。
- 请示 pidan（任选或另定）：
  1. 由 pidan 本人在 K93 安装 Windows SDK 10.0.26100.0（系统级，需 UAC）后再按 A1 命令重跑；
  2. 允许再加命令行属性改目标平台（如 `-p:TargetFramework=net10.0-windows10.0.18362.0`），风险：FluentHub 可能使用 18362 之后的 API，结果与上游目标平台不同，偏差更大；
  3. 维持 FAIL，换备选应用另议。

### 7.5 本地副作用

- FluentHub 克隆 `git status` 干净（仅忽略的 `obj/`、`bin/` 产物增加：Octokit.Generators / Octokit / FluentHub.Core 的 Debug x64 输出，及 FluentHub `obj\...\win-x64\` 中间文件）。
- 日志：`%TEMP%\z5_wave1\t4\rebuild\a1_build.txt`（UTF-8）、`a1_exit.txt`。
- 无安装、无证书、无包注册、无系统设置改动。

---

## 8. 非原样重做 A1 第二次（pidan 安装 SDK 后）· 2026-11-18 01:35–01:40 AWST — 结果：**PASS（构建通过、启动到未登录主界面）**

> 本节为**非原样**构建记录（`-p:NuGetAudit=false`）；上文第 1–7 节保持不变（标题行的「FAIL」指第 3 节原样构建，最新结论以本节为准）。
> 依据：pidan 2026-11-18 01:25 AWST 原话「T4 我在 K93 装 Windows SDK 10.0.26100，装完用同一命令重跑」；pidan 随后本人告知「已安装」。SDK 由 pidan 本人安装，本票未装任何东西。
> 运行时：按 pidan 01:18 AWST 裁决，`Microsoft.WindowsAppRuntime.2` **2.6.5** 替代字面 2.0.x —— 本节已**实测**（见 8.3）。

### 8.1 前置（只读）

- `C:\Program Files (x86)\Windows Kits\10\Platforms\UAP\10.0.26100.0\Platform.xml` 存在；`Platforms\UAP` 与 `Include` 现为 `10.0.18362.0`、`10.0.26100.0`。
- FluentHub 克隆 HEAD `eb08f0dbcbb43880ab34038b0cb6742f0c0deca5`，`git status --porcelain` 为空（构建前、构建后均为空）。

### 8.2 构建（与 7.1 完全相同的命令，未加任何其他属性，未改任何文件）

```
dotnet build src\FluentHub\FluentHub.csproj -c Debug -p:Platform=x64 -p:NuGetAudit=false
```

结果：**BUILD_EXIT=0**（01:35:44–01:36:44 AWST，已用时间 00:00:59.88），**0 警告 0 错误**。restore「所有项目均是最新的」（无新下载）。

```
Octokit.Generators -> ...\src\Octokit.Generators\bin\x64\Debug\netstandard2.0\Octokit.Generators.dll
Octokit -> ...\src\Octokit\bin\x64\Debug\net10.0\Octokit.dll
FluentHub.Core -> ...\src\FluentHub.Core\bin\x64\Debug\net10.0\FluentHub.Core.dll
FluentHub -> ...\src\FluentHub\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\FluentHub.dll
```

输出目录 `src\FluentHub\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\` 含 `FluentHub.exe`、`AppxManifest.xml`、`resources.pri`（Debug 松散布局；Debug 下 `GenerateAppxPackageOnBuild` 不生效，未生成 .msix）。

### 8.3 包注册与运行时（用户级）

- 注册（开发者模式松散注册，用户级，无 UAC、无证书）：
  `Add-AppxPackage -Register <输出目录>\AppxManifest.xml` → 成功。
- 注册结果：`49462fluenthub-uwp.FluentHub_0.11.0.0_x64__tq8wa33sfftnm`，PFN `49462fluenthub-uwp.FluentHub_tq8wa33sfftnm`，`IsDevelopmentMode=True`，`SignatureKind=None`，InstallLocation = 上述构建输出目录。
- 清单依赖 `Microsoft.WindowsAppRuntime.2` MinVersion `2.0.1.0`；**实际解析依赖 = `Microsoft.WindowsAppRuntime.2_2.6.5.0_x64__8wekyb3d8bbwe`**。
- 进程加载模块实证（pid 4632）：`Microsoft.WindowsAppRuntime.dll`、`Microsoft.UI.Xaml.dll`、`Microsoft.UI.Xaml.Controls.dll`、`Microsoft.UI.Composition.OSSupport.dll`、`dcompi.dll`、`Microsoft.UI.Windowing.dll` 等全部来自 `C:\Program Files\WindowsApps\Microsoft.WindowsAppRuntime.2_2.6.5.0_x64__8wekyb3d8bbwe\`；`hostfxr.dll`/`coreclr.dll` 来自构建输出目录（SelfContained）。
- → **2.6.5 替代 2.0.x：实测可用**（构建引用 WASDK 2.0.1，运行于框架包 2.6.5）。
- **清理命令**（用户级，保留以供 T5 继续使用，本票未执行）：`Get-AppxPackage -Name 49462fluenthub-uwp.FluentHub | Remove-AppxPackage`

### 8.4 启动与截图

- 启动方式：计划任务 `schtasks /Create /TN z5_t4_launch ... /IT /RL LIMITED` + `/Run`，任务内 `explorer.exe shell:AppsFolder\49462fluenthub-uwp.FluentHub_tq8wa33sfftnm!App`；会话 1（Console）；任务已 `schtasks /Delete` 删除。
- 结果：01:37:55 AWST 进程启动，`Responding=True`，主窗口标题 `FluentHub`；显示左侧导航（Inbox / My pulls / Reviews / Assigned）、顶部搜索框，正文「Sign in to GitHub to view your activity.」—— 即**未登录主界面**；未出现必须登录才能继续的阻断，未点击登录、未输入任何凭据。
- 截图（仅该窗口区域，DWM 窗口边界 1424×748）：`pilot/fluenthub/t4_fluenthub_window.png`（sha256 `2E429645B3E468AC3BD4066230A2A72F0B3347B22E3F02DDFD675ACA11F7D0FD`）。
- 截图后用 `CloseMainWindow()` 正常关闭应用。

### 8.5 结论

- **T4 基线（非原样：`-p:NuGetAudit=false`；SDK 10.0.26100.0 由 pidan 安装；运行时 2.6.5 替代 2.0.x）= PASS**：构建 0 警告 0 错误；启动到未登录主界面。
- 原样构建（第 3 节）仍为 FAIL，不改写。
- 不含任何性能/时序结论；不采数；Success≠adopt。
- T4 PASS → 按第二波顺序可进入 Z5-T5（仅本地提交）。

### 8.6 本地副作用

- 用户级包注册：`49462fluenthub-uwp.FluentHub_0.11.0.0_x64__tq8wa33sfftnm`（保留；清理命令见 8.3）。
- FluentHub 克隆仅新增被忽略的 `bin/`、`obj/` 产物；`git status` 干净。
- 日志：`%TEMP%\z5_wave1\t4\rebuild2\build_a1.log`、`exit.txt`、`register.log`、`launch.log`、`shot.log`、`modules.txt`、截图原件 `fluenthub_t4_window.png`；启动/截图脚本 `t4_launch.ps1`、`winshot.ps1`。
- 计划任务已删除；无安装、无证书、无系统设置改动（SDK 安装为 pidan 本人操作）。
