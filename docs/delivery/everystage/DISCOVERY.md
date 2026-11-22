# EveryStage 内部接入与打包：P0只读发现

## 固定来源与合并目标

- MF：`350f81ecb605886d968b95c55ea9a9ade4ff43d8`（已结Z6的main）；正式组件/框架工作合并到MF `main`。
- 用户指定[安装包artifact](https://github.com/oops11111/EveryStage/actions/runs/44977550323/artifacts/14736098352)。主代理connector只读核run成功且每step成功；artifact名EveryStage-Setup-win-x64、约62MB、未过期，artifact digest `76614f31aa34372c15f0de26d69f818f89c3b5814967b24a0aa980b4c7eaad94`。这是artifact归档digest，不冒充内层EXE SHA或Authenticode验证。
- EveryStage实际source为`drop-prewarm` / `7a40a8d38d6d1d91786e17d5265037b0d4bd7ddf`。隔离clone本次独立核到最新tip相同；默认分支`claude/new-project-qrnmf8` / `78475afbfe77a8c02b91201986b3eae26ba651f6`是其祖先，left/right为0/23。因此应用PR目标为`drop-prewarm`，不因默认分支不同而丢弃用户产物的23个后续提交。后续每阶段fresh检查，变化先报告。
- 唯一只读clone：`%WORKSPACE%/m0/out/everystage-p0-532e677a80344dafb178229f14c38adf`，detached用户指定commit且clean；未下载、执行或安装artifact。

## 仓库与构建约束

MF根、已检查父层及tracked子目录没有AGENTS.md；EveryStage clone、其src子树及本workspace的m0/out父层也未发现AGENTS.md。本次只读未发现额外适用AGENTS，不把.claude技能包许可证当作应用许可证。

EveryStage `Directory.Build.props`统一版本0.9.1。Caster是WinForms `net8.0-windows10.0.19041.0`，Terminal是WinForms `net8.0-windows10.0.22621.0`、最低19041；两者明确win-x64。SharedUi是net8.0-windows19041。没有必要更改TFM/RID来接入现有net8封装。

现CI逐项目restore、Release -warnaserror build，运行CoreSelfTests、WindowsSelfTests，发布DEV-ONLY ZIP、Caster截图和NSIS Setup；当前没有安装/卸载验收。不要将已生成installer当成已验证安装成功。

可用原命令（本次均未执行）：

```powershell
# MF已有真实Debug/Release回归，后续组件阶段按变更需要运行
python z6/integration/tests/run_tests.py --configuration Debug
python z6/integration/tests/run_tests.py --configuration Release
# EveryStage按现CI循环所有src项目，保留原参数
Get-ChildItem src -Recurse -Filter *.csproj | Sort-Object FullName | ForEach-Object { dotnet restore $_.FullName --nologo; if ($LASTEXITCODE) { throw 'restore failed' } }
Get-ChildItem src -Recurse -Filter *.csproj | Sort-Object FullName | ForEach-Object { dotnet build $_.FullName --no-restore --configuration Release --nologo -warnaserror; if ($LASTEXITCODE) { throw 'build failed' } }
dotnet run --project src/Tests/EveryStage.CoreSelfTests/EveryStage.CoreSelfTests.csproj --no-build --configuration Release
dotnet run --project src/Tests/EveryStage.WindowsSelfTests/EveryStage.WindowsSelfTests.csproj --no-build --configuration Release
# 仅构建产物；P3审脚本后优先在一次性Windows CI runner执行
./scripts/Build-ReleasePackages.ps1
./scripts/Build-Installer.ps1
```

每条失败都留实际exit/log，不承袭Z6单票WNAE/Platform/RestorePackagesConfig等例外。原CI的已有NU1701项目配置不冒称本轮新批准的任意waiver。

## 现pilot到正式组件的缺口

`pilot/l1pack/FluidMotion.L1.csproj`仅链接MotionSystem.cs/MotionEtw.cs、net8.0；README及注释明确pilot-only/不分发。native复制使用Exists条件，缺件可能静默；没有正式版本/包内容/RID native资产/包来源与独立消费验收。不能把旧缓存预编DLL或DEV-ONLY目录改名交付。

本轮用户已明确要求自有应用接入和正式内部组件/可安装包；计划新增独立`src/FluidMotion.L1`，保留旧pilot与所有历史档案不动。生产MotionSystem/MotionEtw/fm/composition及公开namespace/API保持原样，正式链接点在新计划中明确，不借此重写旧约束或源码。

P1需构建native并按win-x64资产正确入包、缺件硬失败、源commit/工具链/包内容/版本/SHA记录，独立临时目录仅用新nupkg restore消费和实际native加载/公开面验证。新编译native hash可能不同于旧F7，记录新产物hash而非强求旧二进制hash或修改旧SUMS。

## 最小真实UI接入点

Terminal `UI/MusicDrawer.cs`现有背景音乐歌单抽屉：15ms WinForms Timer、DateTime.UtcNow、180ms cubic滑动；bitmap单图过渡渲染已经存在。P2候选仅把这一已有开/关/反向动画换成正式组件驱动，不新增效果、不改投屏/音视频/文档管线。

保留Open/Close/Toggle/Esc/收起和bitmap绘制语义。必要时只处理该控件OnParentChanged匿名Resize订阅的卸载/重挂，以确保组件生命周期无残留；不借机广泛清理。UI线程创建与Dispose、外层timer OnFrame、native回调只Apply/排队、不重入、反向from:null连续交接均需实际覆盖。

FluentHub/Files仅提供已验设计参考；不再作为本轮目标，也不操作其上游。其Fake/named-event/EntryReplay/自动重建/截图/诊断日志不是正式产品功能，不照搬。

## 安装、签名、依赖与内部交付

`Build-Installer.ps1`发布Terminal/Caster self-contained win-x64并调用NSIS，显式`-DUNSIGNED=1`；当前0.9.1安装格式为NSIS EXE。未下载内层artifact，本次不声称直接检查其签名；源路由明确未签名，内部交付如实标未签名，不伪造签名/安装证书/更改信任。

旧installer请求admin、机器级ProgramFiles/HKLM autorun、firewall规则、ACL，以及按进程名taskkill（含/F）。因此绝不在用户机器运行它。P3首先在一次性GitHub Windows runner、无既有EveryStage进程/注册/用户数据的单独job验证安装→产物/注册/启动范围→正常关闭→卸载/残留。原silent/component选项需先审实际脚本，不凭空发明参数。若需调整安装安全范围，另列实际diff审查。

`Build-ReleasePackages.ps1`的framework-dependent ZIP及部署PS1明确DEV-ONLY，不能改名冒充正式可安装包。正式内部交付是Setup EXE＋正式组件nupkg/必要说明和SHA，不公开NuGet或GitHub Release。

现MF没有tracked产品LICENSE/NOTICE/COPYING；EveryStage发现的技能包LICENSE不是产品许可。不要凭空生成MIT等授权。已存在应用依赖包括NAudio2.2.1、Vortice.MediaFoundation3.6.2、PdfiumViewer2.13.0/旧native2018.4.8.256、Microsoft.CSharp4.7.0；正式包需从实际还原包核license/notice/转分发内容并随包保留必要第三方声明，不在P0臆断许可条件。Office/WPS功能是现应用依赖，未授权安装或登录。

本机SDK已由主代理只读核为8.0.428/9.0.321/10.0.300；makensis不在PATH，两个标准ProgramFiles目录也未见该EXE。无需为P0或组件先本机安装NSIS；既有一次性CI已安装NSIS来构建。构建/签名/安装任何新增系统需要须按后续明确计划处理，不沿用Z6许可。

## Pinned source links

- [EveryStage: Directory.Build.props](https://github.com/oops11111/EveryStage/blob/7a40a8d38d6d1d91786e17d5265037b0d4bd7ddf/Directory.Build.props)
- [EveryStage: src/Caster/EveryStage.Caster/EveryStage.Caster.csproj](https://github.com/oops11111/EveryStage/blob/7a40a8d38d6d1d91786e17d5265037b0d4bd7ddf/src/Caster/EveryStage.Caster/EveryStage.Caster.csproj)
- [EveryStage: src/Terminal/EveryStage.Terminal/EveryStage.Terminal.csproj](https://github.com/oops11111/EveryStage/blob/7a40a8d38d6d1d91786e17d5265037b0d4bd7ddf/src/Terminal/EveryStage.Terminal/EveryStage.Terminal.csproj)
- [EveryStage: src/Shared/EveryStage.SharedUi/EveryStage.SharedUi.csproj](https://github.com/oops11111/EveryStage/blob/7a40a8d38d6d1d91786e17d5265037b0d4bd7ddf/src/Shared/EveryStage.SharedUi/EveryStage.SharedUi.csproj)
- [EveryStage: .github/workflows/ci.yml](https://github.com/oops11111/EveryStage/blob/7a40a8d38d6d1d91786e17d5265037b0d4bd7ddf/.github/workflows/ci.yml)
- [EveryStage: scripts/Build-ReleasePackages.ps1](https://github.com/oops11111/EveryStage/blob/7a40a8d38d6d1d91786e17d5265037b0d4bd7ddf/scripts/Build-ReleasePackages.ps1)
- [EveryStage: scripts/Build-Installer.ps1](https://github.com/oops11111/EveryStage/blob/7a40a8d38d6d1d91786e17d5265037b0d4bd7ddf/scripts/Build-Installer.ps1)
- [EveryStage: deployment/installer/EveryStage.nsi](https://github.com/oops11111/EveryStage/blob/7a40a8d38d6d1d91786e17d5265037b0d4bd7ddf/deployment/installer/EveryStage.nsi)
- [EveryStage: src/Terminal/EveryStage.Terminal/UI/MusicDrawer.cs](https://github.com/oops11111/EveryStage/blob/7a40a8d38d6d1d91786e17d5265037b0d4bd7ddf/src/Terminal/EveryStage.Terminal/UI/MusicDrawer.cs)
- [motion_framework: pilot/l1pack/FluidMotion.L1.csproj](https://github.com/oops11111/motion_framework/blob/350f81ecb605886d968b95c55ea9a9ade4ff43d8/pilot/l1pack/FluidMotion.L1.csproj)
- [motion_framework: MotionSoftDemo/MotionSystem.cs](https://github.com/oops11111/motion_framework/blob/350f81ecb605886d968b95c55ea9a9ade4ff43d8/MotionSoftDemo/MotionSystem.cs)
- [motion_framework: .github/workflows/z6-integration-tests.yml](https://github.com/oops11111/motion_framework/blob/350f81ecb605886d968b95c55ea9a9ade4ff43d8/.github/workflows/z6-integration-tests.yml)
- [motion_framework: z6/integration/tests/run_tests.py](https://github.com/oops11111/motion_framework/blob/350f81ecb605886d968b95c55ea9a9ade4ff43d8/z6/integration/tests/run_tests.py)

[Stage plan](PLAN.md) | [Source/hash receipts](SOURCES.json)
