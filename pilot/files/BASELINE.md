# Files 基线 · K93N3P8（Z6 F9a）— 结果：**FAIL（原样构建失败，未启动）**（未复核）

> composition.h 冻结；L1 公开面 / fm_* / E1 不改；不采数（dev-only，不计任何闸门会话）；PERF-CI HOLD；Success≠adopt；不合 main；不 force/amend/rebase；不改任何 *_LOCKED.md；系统级改动、安装 SDK / 工作负载 / VS 组件、非原样构建开关一律停下请示 pidan。
> 本分支只放报告与日志，不提交 Files 源码；本地克隆不 fork、不推送、不提 issue / PR / 评论、不登录任何第三方账号。

- 依据：`z6_workbuddy_handover_FINAL_r2.md`（sha256 `82ab4e29db68e943ab04b034d9e3e360c4a84f2a05451ae904772e10761dd2b1`）第三节 F9a，参照 Z5 T4（`pilot/fluenthub/BASELINE.md`）。
- 时间：2026-11-18 07:45–07:50 AWST（Australia/Perth, UTC+8）。
- 本地克隆：`%USERPROFILE%\pilot\Files`（`git clone https://github.com/files-community/Files`；`origin` 仍指向上游，未推送任何东西）。

## 1. 源与标识

| 项 | 值 |
|---|---|
| Files commit | `git rev-parse HEAD` = `cbfb4ba54a89632fbceb82ea94ada60a9bae3737`（`git describe --tags` = `v4.3`；"Build: v4.3"，2026-10-11 14:04:55 -0400 = 2026-10-12 02:04:55 AWST），detached HEAD |
| 解决方案 / 应用工程 | `Files.slnx`；`src/Files.App/Files.App.csproj` |
| 目标框架 | `net10.0-windows10.0.26100.0`（Directory.Build.props），MinimalWindowsVersion 10.0.19041.0；`WarningsAsErrors=nullable`（未设 TreatWarningsAsErrors） |
| WASDK | `Microsoft.WindowsAppSDK` **2.5.1**（Directory.Packages.props）；CsWinRT 2.3.1；Microsoft.Windows.SDK.BuildTools 10.0.28000.2705 |
| 打包标识 | `src/Files.App/Package.appxmanifest`：Identity Name `FilesDev`，Publisher `CN=Files`，Version `4.3.0.0`，Application Id `App`；MSIX（`EnableMsixTooling=true`），`runFullTrust` 等 |
| PublishProfile / AOT（已知缺口②） | `Files.App.csproj` 第 6 行 `PublishProfile=Properties\PublishProfiles\win-$(Platform).pubxml`；`win-x64.pubxml`：`PublishAot=true`、`SelfContained=true`。预计复现 K-AOT-ETW（本票未构建成功，未验证） |
| C++ 子项目（已知缺口①） | `Files.App.Launcher`、`Files.App.OpenDialog`(+Win32)、`Files.App.SaveDialog`(+Win32) 的 vcxproj 均为 `PlatformToolset=v145` |
| global.json（已知缺口③） | `sdk.version=10.0.102`，`rollForward=latestMajor` → K93 实际解析为 SDK **10.0.300**（`dotnet --version`） |
| NuGet 源 | nuget.org、仓内 `src/Files.App/Assets/Libraries/`（本地目录）、CommunityToolkit Labs 公开 Azure Artifacts 源（匿名，未要求登录） |
| 上游 CI 构建方式 | `.github/workflows/ci.yml`：`runs-on: windows-2025-vs2026`；先 `msbuild Files.slnx -t:Restore …`，再 `msbuild Files.App.csproj -t:Build …`（见第 3 节） |

## 2. 前置检查（只读）

| 项 | 结果 |
|---|---|
| `Get-AppxPackage *Files*` | 无 Files 包（只有 WindowsAppRuntime 框架包） |
| WindowsAppRuntime 2.x | `Microsoft.WindowsAppRuntime.2` 2.5.1.0（x64）、2.6.5.0（x64/x86）已在；本票未安装任何运行时 |
| .NET SDK | 8.0.428、9.0.321、10.0.300 |
| VS / MSBuild | VS 生成工具 2022 17.14.37328.6（含 VC 工具集 v143）；**VS Community 2026 18.6.11822.322**（MSBuild 18.6.3；工作负载只有 ManagedDesktop、CoreEditor，**无 C++ 工具**：`VC\Tools\MSVC` 不存在，v180 下无 PlatformToolsets）。定稿写 K93 只有 VS 2022 生成工具，与实际不符，记为事实差异 |
| Windows SDK | 10.0.18362.0、10.0.26100.0 |
| 开发者模式 | `AllowDevelopmentWithoutDevLicense=1`（只读读取） |
| 会话 | `SESSIONNAME=Console` |

## 3. 原样构建（未改任何源码 / 工程文件；无额外开关）

### run1：上游 CI 命令（Debug / x64，CI 矩阵中非 Release 一格的两步），用 VS 2026 的 MSBuild 18.6.3

```
msbuild %SOLUTION_PATH% -t:Restore -p:Platform=x64 -p:Configuration=Debug -p:PublishReadyToRun=true -v:quiet
msbuild %APP_PROJECT_PATH% -t:Build -p:Configuration=Debug -p:Platform=x64 -p:AppxBundle=Never -p:GenerateAppxPackageOnBuild=false -v:quiet
```

- Restore：**exit=0**（07:47:19–07:48:24 AWST）。警告：5 个 vcxproj `NU1503`（跳过还原）；`Files.App.csproj` `NU1903`（`Microsoft.DotNet.ILCompiler` 10.0.8 高危漏洞，此处为警告，未阻断）。
- Build：**exit=1**（07:48:48 AWST）。错误：
  1. `CSC : error CS9057`：分析器 `Files.Core.SourceGenerator.dll` 引用编译器版本 5.9.0.0，比当前运行的 5.6.0.0 新（`Files.App.CsWin32.csproj`）。源生成器引用 `Microsoft.CodeAnalysis.CSharp` 5.9.0（Directory.Packages.props）。
  2. `error MSB4018`：WinUI 2.3.9 `Microsoft.UI.Xaml.Markup.Compiler.interop.targets(295)` 的 `GetLatestMSVCVersion` 任务找不到 `%ProgramFiles%\Microsoft Visual Studio\18\Community\VC\Tools\MSVC`（`Files.App.Controls.csproj`）。

### run2：参照 Z5 T4 的命令形式（`git clean -xdf` 清掉 run1 产物后）

```
dotnet build src\Files.App\Files.App.csproj -c Debug -p:Platform=x64
```

- **exit=1**（07:49:19–07:50:14 AWST），5 警告 1 错误：同样的 `CS9057`（SDK 10.0.300 自带 Roslyn `Microsoft.CodeAnalysis.dll` 文件版本 5.600.26.23102，低于源生成器要求的 5.9）。

### 结论

- **原样基线 = FAIL**（构建未过，未打包、未注册、未启动；无截图）。
- 新发现缺口（定稿未列）：**⑤ Roslyn 版本**：K93 上的 SDK 10.0.300 与 MSBuild 18.6.3 都带 Roslyn 5.6，Files v4.3 的源生成器需要 5.9；**⑥ WinUI XAML 编译器需要 VS 的 MSVC 工具目录**（MSBuild 路线）。
- 定稿已知缺口：① v145 本次未走到（C++ 子项目不在 `Files.App.csproj` 的构建链里，CI 也只构建应用工程；打包 / 运行时是否需要待定）；② PublishAot 未验证；③ global.json 已由 latestMajor 解析到 10.0.300，未阻断；④ 包注册未走到。
- 继续需要的任一步（装更新的 .NET SDK、给 VS 2026 装 C++ 工作负载、改 VS 2022 / 2026 组件、改包版本或加开关）都是安装或非原样，**须 pidan 拍板**，本票停在这里。

## 4. 本地副作用

- 新增本地克隆 `%USERPROFILE%\pilot\Files`（@cbfb4ba5，detached），`git status` 干净（run2 后的 bin/obj 被忽略）。
- NuGet 全局缓存新增 Files 依赖包。
- 无安装、无证书、无包注册、无系统设置改动、无计划任务。

## 5. 日志

`pilot/files/f9a_logs/`：`clone.log`、`cmd.txt`、`restore.log`、`build.log`、`times.txt`（run1），`cmd2.txt`、`build2.log`、`times2.txt`（run2），`MANIFEST.txt`（原始 sha256 与入库 sha256）。入库前把用户目录替换为 `%USERPROFILE%`，`C:\Program Files (x86)` / `C:\Program Files` 替换为 `%ProgramFiles(x86)%` / `%ProgramFiles%`（`z6/f9a/f9a_collect.ps1`）。

---

## 续跑（2026-11-18 08:25–08:31 AWST）— 结果：**未完成重跑**（未复核）

许可（pidan，卡 t208s1016，08:04 AWST）：「F9a 允许安装新版 .NET SDK（Roslyn≥5.9）和 VS 2026 C++ 工作负载」。不花钱，不升级付费版。

| 项 | 安装前 | 安装后（SDK 完成、VS 未装） |
|---|---|---|
| 机器级 SDK | 8.0.428、9.0.321、10.0.300 | 不变 |
| 用户级 SDK | 无 | **10.0.404** 于 `%USERPROFILE%\.dotnet`（官方脚本，`-NoPath`） |
| Roslyn（所用编译器） | 10.0.300 → 5.6.0（FileVersion 5.600.26.23102） | 用户级 10.0.404 → **5.9.0**（FileVersion 5.900.26.53019） |
| VS Community 2026 | 18.6.11822.322，ManagedDesktop+CoreEditor | 不变 |
| `VC\Tools\MSVC` | 不存在 | 仍不存在 |

- SDK 安装 exit=0。命令与日志：`pilot/files/f9a_logs/rerun/` 与 `z6/f9a/raw/rerun/sdk/`。
- VS C++ 工作负载：`setup.exe modify … NativeDesktop --includeRecommended --passive --norestart --noUpdateInstaller` 在未提升进程中 **exit=5007**。日志：`Commands with --quiet or --passive should be run elevated from the beginning.` **未弹出 UAC**（未点击）。详见 REPORT 续跑段。
- **未重跑** run1 / run2（须等 C++ 工作负载装完）。Files HEAD 仍为 `cbfb4ba54a89632fbceb82ea94ada60a9bae3737`，工作区干净。上一节原样基线结论仍为 **FAIL（未启动）**。
