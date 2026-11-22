# EveryStage 接入与正式内部交付计划

## 目标、授权与固定基线

本轮用户要求目标应用接入和正式打包发布，已选择自有应用、内部交付（本地可安装应用包＋组件包），并指定EveryStage安装artifact。这里是需求/派工范围摘要，不冒充用户说过额外逐字引文。不开公开NuGet/GitHub Release、不花钱或改账号、不用Z6逐票系统/非原样flag许可。旧pilot-only限制可在本轮明确批准的新正式项目范围迁移，历史pilot、Z6档案及冻结生产/API仍保留。

MF基线`350f81ecb605886d968b95c55ea9a9ade4ff43d8`，目标`main`；应用基线`7a40a8d38d6d1d91786e17d5265037b0d4bd7ddf`，目标`drop-prewarm`。应用默认分支虽为claude/new-project-qrnmf8，但它落后23提交，用户artifact来自drop-prewarm。所有阶段开工前fresh核基线；漂移先报告，不擅改目标或rebase。只ordinary commit/push，无force/amend，唯一持续实现代理做实现/测试/PR，主代理审diff/CI、处理失败和merge/main或target-post。

## P0 — 来源/计划交付（本PR，仅文档）

依赖：用户目标、渠道与artifact明确；两个repo可只读取得。
范围：MF `docs/delivery/everystage/**`；不改业务/pack/旧证据，不下载执行artifact、不运行安装脚本。
交付：本Plan、DISCOVERY.md及少量pinned source/hash收据，记录AGENTS、TFM/RID、CI、包/签名及MusicDrawer最小接入点。
验证：Git ref/祖先/clean核查、UTF8文档/本地链接、固定source文件hash、diff whitespace/scope；只引用已通过CI，不新跑行为测试。
验收/CI：来源与边界可审；P0文档PR现有适用checks全green，主代理merge MFmain并跟mainpost；不以文档检查宣称程序验收。
停止：pin变更/源证据不符/权限或需要系统步骤；不扩大调查为安装或业务实施。

## P1 — 新正式组件（MF，后续窄任务后实施）

依赖：P0 merge与post绿；主代理明确P1文件范围/版本决策。无须迁移旧pilot目录。
范围候选：新增 `src/FluidMotion.L1/FluidMotion.L1.csproj`、组件构建/打包脚本与必要包内容/文档/CI；链接现有MotionSystem.cs/MotionEtw.cs，不改变源码、namespace、公开API、fm、E1、composition或旧SUMS。
交付：适配已证实.NET8 Windows x64消费者的正式组件nupkg（managed与runtimes/win-x64/native资产、必要token/说明/来源/第三方notice）、新产物SHA清单/源commit/工具链记录。正式版本/PackageId在P1实际项目与交付清单中固定，不取latest/旧缓存。
构建候选：先现有vcvars64下以repo原native recipe构建独立fluidmotion.dll，再 `dotnet pack src/FluidMotion.L1/FluidMotion.L1.csproj -c Release -o <isolated-local-feed>`。具体native/asset接线由新P1脚本定义并审，不运行Z6 waiver或从用户缓存复制假产物。
验证：native缺件/错RID硬失败；包内容与架构检查；在第二个全新独立目录、job-local NuGet cache中只从新local feed精确版本restore、build并实际调用公开API/native；CheckSurface+full dump与冻结基线比对；按代码变更运行已有MF Debug/Release回归。
CI门：正式包生产→独立消费验证→native/managed/API/Frozen检查全绿。旧试点CI及证据保持，不删除20/17/18等既有行为用例。
验收：新组件不依赖旧FmL1Root/SUMS/手工全局DLL；native及托管在消费输出中真实匹配该次新包SHA。新编译native可能不等旧F7SHA，记录来源和新hash，不改旧记录凑数。
停止：公开面/冻结源码需改、实际license/资产缺口、外部凭据或新增工具安装需求；不得静默放宽缺件检查/升依赖/另加构建例外。

## P2 — EveryStage现有MusicDrawer最小接入

依赖：P1正式组件合并且MF post绿；固定该MF提交与组件精确版本；应用drop-prewarm基线再次核对。
跨仓消费：EveryStage CI以明确MF commit只读checkout源码到隔离external目录，使用MF已审构建接口生成job-local feed；应用精确PackageReference消费。NuGet source mapping把该PackageId仅映射到本地feed，禁止同名公共源回退。job-local包缓存与来源清单避免旧同版本缓存误用。
同一应用pipeline只构建一次供本轮消费/最终组件交付的nupkg，后续job下载同workflow产物并核SHA；不跨公开仓提交vendor二进制、不公开发布包、不依赖跨repo私密artifact权限或新token。MF CI自验与最终应用包各记实际构建hash，不能把源相同误称二进制恒等。正式对外于本地交付的固定版本一经封存不得换bytes，变化应另定版本。
范围候选：Terminal csproj/还原配置、MusicDrawer.cs与必要private WinForms adapter/Tests/CI；不动Caster捕获、投屏、音视频、发现/配对协议或文档渲染业务。
用户动作：已有「歌单」开→收起/Esc关；在途再点反向；完成后再开；抽屉打开时宿主Resize；关闭宿主。仍用原bitmap单图绘制，保持原最终高度/可用性，不能新增另一套用户效果。
交付：正式组件驱动现有高度动画；外层UI Timer调用OnFrame，idle停泵；native callback只Apply/队列而不重入；同handle Retarget(from:null)连续反向。必要时只修MusicDrawer parent Resize解绑/重挂和Dispose资源释放，不泛清理。
验证命令：DISCOVERY中现EveryStage逐项目restore/Release -warnaserror及Core/Windows selftests，加入真正链接该adapter/生命周期的定向测试；本机仅隔离目录普通启动用于小范围UI观察，具体数据目录/启动flags/权限先审，不能假装旧Z6包许可适用。
通过标准：开/关/快速反向无跳跃/卡住、末态和原行为一致；UI线程正确；Disposed/stale事件无回流；原visual/bitmap与parent订阅/handle释放、idle停泵；不用sink计数代替可见动作验收、不做perf或PresentStats字段读取。
CI门：现有完整CI保留，加正式组件来源/版本/hash/output资产/adapter测试；root审实际diff和local UI证据，全green后合EveryStage drop-prewarm并跟目标branchpost。
停止：需额外效果/公开API/媒体链路改动、真实设备/Office/账号不可用、新flag/安装动作；先留具体证据，不扩范围。

## P3 — 正式NSIS可安装包及一次性安装/卸载验收

依赖：P2目标branch已验、包版本/组件来源冻结；现安装脚本的实际mutation/参数与cleanup先审。
范围候选：EveryStage原Build-Installer.ps1/NSIS与最小安全验收CI、包内容/license/manifest说明；不将DEV-ONLY ZIP改名作为正式包。应用版本仍由Directory.Build.props单点控制，正式交付新版本由本阶段Plan实际确定。
交付：self-contained win-x64 NSIS Setup EXE＋本轮实际消费的正式组件nupkg＋SHA/sourcecommit/toolchain/dependency/notice/安装卸载说明。签名实际未签名（现UNSIGNED=1），明确内部未签名属性；不伪造签名/买证书/导入信任。若后续要求签名，作为真实凭据/服务需求另决策。
构建命令：已有 `./scripts/Build-Installer.ps1`（.NET8 publish＋makensis）；所需NSIS优先用既有一次性Windows CI环境，不自动在用户机器安装。Build-ReleasePackages的ZIP保留其DEV-ONLY定位。
验收位置：独立一次性GitHub Windows runner job，预置无EveryStage进程、安装/注册、数据；使用真实已审silent/component选项，不臆造开关。精确检查安装路径/两组件选择/版本/本次来源DLL和native、快捷方式、HKLM/autorun、firewall/ACL；测试后正常关闭、卸载及残留记录，异常先证据不扩大杀进程范围。现按进程名forcekill的旧脚本绝不在用户机器执行。
CI门：构建包→签名状态/hash/内容→一次性安装/卸载真实验收；失败不能只凭makensis/build0当PASS。不要求WPS/Office登录或新增硬件安装来凑通过，未知/不可覆盖功能明示。
停止：runner外既有程序/资料可能被影响、安装器新scope/证书/账号/费用需求、需要真实用户机提权/firewall变更；交root决定，不沿旧授权继续。

## P4 — 内部本地交付与关闭本轮范围

依赖：P1–P3实际差异已审、所有适用CI与MFmain/EveryStagedrop-prewarm post均green；安装包和组件来自同一封存交付链。
交付：本地可安装Setup EXE、组件nupkg、内部交付manifest/SHA/源commit/版本/RID/签名实际状态/依赖限制/回滚卸载说明；通过GitHub Actions artifact或本地目录交付，非公开NuGet/GitHub Release，不自动安装用户机。
验证：下载后只读核artifact归档digest与每内层文件SHA，不能把artifact zip digest当EXE hash；核实际组件→应用publish→Setup内资产来源一致。正式版本相同却bytes改变先停，不能悄替。
验收：root给出两个repo exactmerge/CI/交付文件与校验命令，用户能取得一致的两类内部产物；已知未签名/Office功能与未做性能结论如实交接。无需为补材料重跑已过行为。
停止：任何未绿必需gate、无法追到实际源/包/安装器或交付缺件；不得提前称正式交付完成。

## 全程权限与报告契约

用户本轮需求是验收来源，不新建无必要issue。P0只计划文档；后续每阶段由root给窄任务后再改对应repo。正常checkout/build/测试/普通commit/push/PR按明确阶段推进；root负责all-green merge/post。每阶段报告branch/fullSHA、文件、真实测试/产物hash、PR/run、limitations与下一步。所需新增系统/证书/账号/费用/外部服务或未知上游源码操作均按实际证据停，不继承Z6例外。

[Read-only discovery](DISCOVERY.md) | [Pinned source receipts](SOURCES.json)
