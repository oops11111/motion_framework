# FluentHub 基线 · K93N3P8（Z5 T4）— 结果：**FAIL（原样构建失败，未启动）**

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
