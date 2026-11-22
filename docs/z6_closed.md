# Z6 结题记录（限定功能范围，dev-only）

## 结题依据与边界

用户在当前会话对完整拍板问题直接回复：**「采用技术草稿与主代理复核，继续正式结题」**。记录时刻为2026-11-22 11:06:38 AWST（03:06:38 UTC，非消息发送时刻）；完整question item标识与范围见[授权记录](../z6/closure/codex/AUTHORIZATION.json)。用户明确采用技术草稿及主代理独立复核，接受其中披露的历史限制，作为缺失两份Notion结果材料的替代结题依据。

`docs/z6_coord/z6_result_review_notion.md` 和 `docs/z6_coord/z6_closed_DRAFT_notion.md` 在协调分支9c28dcc6a97bda7d552ddf2a6cc6f12347063751中仍未提供；本记录不冒称取得Notion评审，也不创建伪Notion文件。既有handover裁决仅裁决交接规范，N9不构成结果裁决。本轮不再把等待这两份材料列为阻塞：其依据替代已经用户明确决定。

本轮关闭的是下表限定的开发验证/文档交付范围。**PASS均为dev-only；Success≠adopt；PERF-CI继续HOLD。** 结题不表示性能合格、产品采用、NativeAOT publish成功或面向一般用户的全面验证；任何未来采数/阈值/性能票另走授权与闸门。

本次文档基于已验main `eb09e63f3068a98fb011709132365e81d825c23c`。生产L1/fm/E1/composition与公开面保持冻结；本轮未改pack源、旧报告/raw、现有LOCKED/APPROVED。F1/F3所改测试代码属于已验集成范围，不是生产公开面变化。本次正式文档仍经主代理审diff、全部适用CI绿色、普通合入main及main post-CI核验后完成交付流程。

## 逐票范围与结论

13张原票与后续明确开票的F9b均有证据路径；不将新验收倒写为历史旧票的原判定。

|票|本轮范围内状态|证据与保留限制|
|---|---|---|
|F0|原冒烟证据保留；集成回归通过|[历史原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f0/REPORT.md) → [已验续接/集成证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/integration/tests/REPORT.md)；未对latest main重新跑F0 GUI，不把CI视作新GUI|
|F1|A/B确定性及集成回归通过；C原证据保留|[历史原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f1/REPORT.md) → [已验续接/集成证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/integration/tests/REPORT.md)；C旧措辞/视觉中途未截图保留；新F2保留F1C代码，不宣称重做全部F1C视觉验收|
|F2|新严格生命周期修复通过|[历史原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f2/REPORT.md) → [已验续接/集成证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f2/codex/REPORT.md)；不追认或改写旧PASS；新build仅F2已批准WNAE+Platform=x64，NON-ORIGINAL；无系统动画切换|
|F3|native测试整合通过|[历史原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f3/REPORT.md) → [已验续接/集成证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/integration/tests/REPORT.md)；措辞问题与技术通过分开；不重命名旧报告|
|F4|文档交付完成；可选代码未做|[历史原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f4/REPORT.md) → [已验续接/集成证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/integration/docs/REPORT.md)；不把后续Files特例当原F4全部命令验证；未做DEBUG可选代码|
|F5|新宿主A/B/C/E通过；原FAIL保留|[历史原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f5/REPORT.md) → [已验续接/集成证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f2/codex/REPORT.md)；原FAIL/补转录保留，不能用新run重造旧原件；可选D系统动画切换未做，也非必做|
|F6|原视觉内容已定性复核；形式偏差保留|[历史原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f6/REPORT.md) → [已验续接/集成证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/integration/pack/REPORT.md)；原全桌面后裁切非原生窗口capture；独立midPNG仅1张，GIF是单独序列；本次依用户决定保留为已披露偏差|
|F7|元数据/参考hash/完整API复核通过|[历史原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f7/REPORT.md) → [已验续接/集成证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/integration/pack/REPORT.md)；非latest main重建pack；原native二进制未因审计重执行；禁止为凑hash改SUMS|
|F8a|DRAFT交付完成|[历史原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f8a/REPORT.md) → [已验续接/集成证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/integration/docs/REPORT.md)；不是性能闸门已开；PERF-CI HOLD，不补probe或LOCK|
|F8b|DRAFT交付完成|[历史原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f8b/REPORT.md) → [已验续接/集成证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/integration/docs/REPORT.md)；不是APPROVED；不把DRAFT措辞视为PASS性能结果|
|F9a|批准的非原样构建/手工布局主页验收通过|[历史原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f9a/REPORT.md) → [已验续接/集成证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f9a/devlayout-codex/REPORT_M_CONTINUATION.md)；不代表原样fullsolution或VS部署成功；安装exit0含NGEN与DCAT同步warning/error；无repair/上游bug结论|
|F11|历史搬迁/破坏证据保留；当前hash/API核对通过|[历史原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f11/REPORT.md) → [已验续接/集成证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/integration/pack/REPORT.md)；未重跑原搬迁/corruption GUI；不能把nativeCI替代这类历史验收|
|F12|历史合入补记完成；原引用措辞保留|[历史原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f12/REPORT.md) → [已验续接/集成证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/integration/docs/REPORT.md)；§11 r1为历史授权来源，r2为后续规范；本次在结论中澄清、原补记不改；不升级功能失败或悄改旧文|
|F9b|批准范围的L1宿主试点通过|[F9a前置原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f9a/REPORT.md) → [已验续接/集成证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f9b/codex/REPORT.md)；NON-ORIGINAL+manual layout；entry_position_changed=false，不声称token offsetX=-80DIP位移；无perf/adopt/原样VS成功|

F8a/F8b的本轮终态就是DRAFT交付：没有probe、LOCK或APPROVED，不是待补的本轮性能验收。F4可选DEBUG代码没有请求，也未实施。F5真实切换系统动画的可选D未执行，不是本轮尾项；必需Fake/钩子/生命周期与清理均有独立新证据，系统动画设置未切换。

## 关键技术证据及不被扩大的含义

- 集成PR11：native Debug/Release各45 checks/0 failures、完整net8 devsmoke及完整公开面检查通过。原F1 A/B raw的两配置33/0、在途交接值一致、6/6 overlap与最终后写者目标40均直接复核。没有为了结题重跑已验测试或GUI。
- F2/F5 PR22：新修复实际顺序为旧Unloaded→Dispose结束→旧listener active0→ACK→排队新Create→新Loaded→新listener active1。Fake逐cycle Created/Released为5/5、4/4，Real为5/5、5/5，最终Idle active0/pump0；不以合并总数替代无重叠证明。Fake线程2翻转并恢复，Real跨RootView仅一次窗口hook，WM_SETTINGCHANGE0x1A/wParam0x6D无变化重读不重复Set，正常退出和精确卸载完成。每mode FULL原日志在下次启动/卸载前保存；发布脱敏副本与原bytes有不同SHA记录。
- F7/F11：既有参考三产物SHA、源码来源、SUMS及完整public surface链核对；完整dump逐字节相等、CheckSurface0。参考检查不是重建当前pack，也不是新跑历史搬迁/破坏GUI。生产冻结面未为合入改变。
- F9a：原环境5007、原样fullsolution缺WIL失败、VS selected-project Deploy0但未出现包的STOP均保留。后续原版本RestorePackagesConfig=true获本票批准，NON-ORIGINAL fullsolution build通过；手工development layout普通MainPage/SettingsButton就绪、正常关闭及精确清理通过。它不是原样/VS部署成功，也不宣称上游bug。VS安装最终exit0/instance complete，但NGEN提示和DCAT注册同步0x80070005错误仍如实保留，未repair。
- F9b：只按用户开票范围以冻结L1pack接入Files私有宿主；NON-ORIGINAL构建+手工布局。root亲看Settings区域图接受opacity入场及8DIP hover/rest，真实5类安全事件、Created=Released3、末Idle0/0、模块metadata/TokenLoad和正常关闭/精确清理均有证据。`entry_position_changed=false`：不声称token offsetX=-80 DIP的入场负向位移被证明；FrameTick仅计数不读载荷；PresentStats所有字段不读，N/A，不作时间/FPS结论。

## 原结果复核九项缺口的结题处置

用户选择保留并归档以下历史限制，作为当前结题依据的一部分；没有把旧报告改判、把新run当旧原件，或把来源可见性缺口推断成责任/违规。

|原缺口|当前处置与限制|
|---|---|
|1 F2原反序PASS争议|旧报告原文及披露保留；本轮按原Dispose-before-Create合同完成独立新修复验收，不追认旧顺序满足合同。|
|2 TIPS线性叠放不符|保留旧TIPS，记录真实拓扑：F2父F11、与F6为兄弟。现13个原wb tips全部为main祖先；按真实PR/merge/归档来源整合，不从拓扑推断作者意图。|
|3 旧拍板卡未独立可见|原review仅有限存疑的A3/A4来源限制保留；本次没有取得旧live卡原文。当前直接授权不追溯证明旧卡选项。|
|4 F12扩写/r1引用|原§11的r1引用是历史授权来源，r2是后续最终规范；PR9 merge/tree/parents可核，但本次不声称独立取得r1授权原文。仅在本结论中澄清，不改原补记或把格式争议升级为功能失败。|
|5 F5旧build.log删除|原报告披露删除；现有交付缺原bytes，转录不能核验原始字节。本轮新F2/F5是独立新证据，不修复或替代旧capture链；不作全世界不可恢复的断言。|
|6 F6可见性与采集形式|主代理亲看start/mid/settled PNG及全部8个GIF帧，定性内容为blank→左clip→若干不同接近就位位置→完整图标文字，所见提交图仅应用window。原全桌面抓后裁切不是原生window capture；独立命名midPNG仍只有1张，GIF是独立序列，不假称交付2张dedicated midPNG。|
|7 历史远程force-push未知|当前对象/PR/CI链可证；历史remote更新未完整审计。一次repo/events查询被connector400拒绝，未取得事件数据；不推断有或无违规，不以当前ordinary push证明历史从未force。|
|8 旧侧叉磁盘前史|旧时磁盘已含F6/F1C的说法仍不能由本仓独立回溯。本轮隔离pin、明确baseline文件hash、最终完整patch/byte roundtrip和实际验收建立当前可复现链，不反向证明旧磁盘。|
|9 原数值/行号/GIF核验不足|本轮直接读F1 A/B四raw的安全summary；root实核F2原反序及新严格seq链，并看F6原图/8GIF帧。逐项补强当前证据覆盖，同时保留F6采集/交付形式限制，不声称旧review当时已做这些检查。|

详细14票/9gap矩阵、来源和primary原收据已[exact归档](../z6/closure/codex/SOURCE_INDEX.json)。技术稿SHA为`0a0ef2ce22248e797aa40eb9793f3c3efefe6435b28baf5d28c632e5493e6cab`。档内「ignored／授权pending／未结题」是用户决定之前的历史快照；本次AUTH仅取代授权/结题依据状态，不改其原字节，更不把其变为Notion结果稿。

## main合入与CI证据链

[API收据](../z6/closure/codex/source_archive/API_PROVENANCE.json)与[head checks收据](../z6/closure/codex/source_archive/HEAD_CHECKS_PROVENANCE.json)由主代理只读核验。PR11/22各6个成功head checks；PR12–21各5个成功；PR15–20另有旧workflow branch guard解释的非适用environment-evidence skipped。全部观测适用checkruns完成不晚于merge；12个对应main post-run全部success。此记录证明所观测PR/CI范围和时序，不是历史force审计，也不能替代实际本机GUI。

|PR|merge/main|main post-CI|
|---|---|---|
|[#11](https://github.com/oops11111/motion_framework/pull/11)|`4e128e735edf788da0b9fa9e11f0d5e2511e5585`|[run 44880677014](https://github.com/oops11111/motion_framework/actions/runs/44880677014) success|
|[#12](https://github.com/oops11111/motion_framework/pull/12)|`8c5cc6f31ca2be796986afd901fc1bb1098712ca`|[run 44881810494](https://github.com/oops11111/motion_framework/actions/runs/44881810494) success|
|[#13](https://github.com/oops11111/motion_framework/pull/13)|`122f7b4edc7698e742658532e3e1a83010f39f17`|[run 44883079762](https://github.com/oops11111/motion_framework/actions/runs/44883079762) success|
|[#14](https://github.com/oops11111/motion_framework/pull/14)|`3078ae0e9f8792a03f45e09dd4fce5c6fda1ca7e`|[run 44887910560](https://github.com/oops11111/motion_framework/actions/runs/44887910560) success|
|[#15](https://github.com/oops11111/motion_framework/pull/15)|`425165e112c271dc76706e5ff2d5354b1dfc9120`|[run 44889148641](https://github.com/oops11111/motion_framework/actions/runs/44889148641) success|
|[#16](https://github.com/oops11111/motion_framework/pull/16)|`8e68cf81ef10bd0623651949e6a98ddef4f175ca`|[run 44890658744](https://github.com/oops11111/motion_framework/actions/runs/44890658744) success|
|[#17](https://github.com/oops11111/motion_framework/pull/17)|`81543c86fc67801aa2de00f44d8b267c2ac09ee9`|[run 44891508566](https://github.com/oops11111/motion_framework/actions/runs/44891508566) success|
|[#18](https://github.com/oops11111/motion_framework/pull/18)|`92c498fbcf8ea7216d54104886a722d34d5099fb`|[run 44895678100](https://github.com/oops11111/motion_framework/actions/runs/44895678100) success|
|[#19](https://github.com/oops11111/motion_framework/pull/19)|`0fc838c9624b4a23c99bbc57f3d5d1c9bf8c2510`|[run 44900488544](https://github.com/oops11111/motion_framework/actions/runs/44900488544) success|
|[#20](https://github.com/oops11111/motion_framework/pull/20)|`05830996127cd40d32f3718a2ae253583d092f36`|[run 44903228845](https://github.com/oops11111/motion_framework/actions/runs/44903228845) success|
|[#21](https://github.com/oops11111/motion_framework/pull/21)|`d2aac2dc355d0412aa5fc3ede212b79faf4459b9`|[run 44911508536](https://github.com/oops11111/motion_framework/actions/runs/44911508536) success|
|[#22](https://github.com/oops11111/motion_framework/pull/22)|`eb09e63f3068a98fb011709132365e81d825c23c`|[run 44978158079](https://github.com/oops11111/motion_framework/actions/runs/44978158079) success|

PR21 post为44911508536，44903228845属于PR20。后续本次文档PR的head/merge/main-post事实以其PR和主代理最终交付记录为准；不伪填尚未发生的merge SHA。

本记录不遗留等待Notion、可选F5D或F8性能探针的本轮工作项。已披露的历史/环境/采集形式/非原样限制随结题保留；未来采用、采数、性能闸门或范围扩展均另票决定。
