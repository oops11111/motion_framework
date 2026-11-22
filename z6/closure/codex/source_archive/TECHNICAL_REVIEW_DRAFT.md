# Z6 技术结果复核草稿（建议，非裁决，非 canonical closure）

基线 main `eb09e63f3068a98fb011709132365e81d825c23c`；coord `9c28dcc6a97bda7d552ddf2a6cc6f12347063751`。仅ignored只读技术材料；不修改历史PASS/FAIL、原日志、TIPS、LOCKED/APPROVED或正式结题文件。没有重测、GUI、系统操作、外部消息或账号动作。

来源强度分开：①本次Git/文件/选定安全summary直接核对；②已落main的原始/续跑证据；③主代理已独立核查的GitHub API、实际日志/图像与cleanup事实。`API_PROVENANCE.json`来自主代理authenticated connector，只用于PR/CI事实，不是Notion裁决。F6视觉复核由主代理实际完成，详见 F6_VISUAL_REVIEW.json；本实现代理未重复看图或运行GUI。

## 逐票矩阵（13原票 + F9b）

|票|依赖/边界|历史声明（不改）|当前证据|证据强度|限制/仍需决定|原报告→新证据；main链|
|---|---|---|---|---|---|---|
|F0|原main7e2合入后的冒烟；冻结源/API|原票最终PASS(dev-only)，含GUI Debug/Release补跑|PR11导入原证据；集成native45 checks、完整net8 devsmoke、公开面Debug/Release新验收通过|原raw+集成新测试强；GUI是历史证据|未对latest main重新跑F0 GUI，不把CI视作新GUI|[原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f0/REPORT.md) → [当前证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/integration/tests/REPORT.md)；PR11|
|F1|A/B直接基于main7e2；独立Git分支不叠F6/F11；C宿主验收才排F6后|A/B PASS(dev-only)；C非原样续跑PASS|本次只读核原A/B四份raw：D/R native33/0；A/B在途值-57.123109、起点同值、overlap6/6、FINAL40/opacity1；PR11新合并矩阵45/0|A/B原数值及fresh integration强；C为原日志/补丁证据|C旧措辞/视觉中途未截图保留；新F2保留F1C代码，不宣称重做全部F1C视觉验收|[原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f1/REPORT.md) → [当前证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/integration/tests/REPORT.md)；PR11|
|F2|F11/F7；与F6是兄弟，不是后代；F5继承F2|原票称PASS但自己披露Create早于Dispose|PR22独立新修复：seq44 Unloaded→61 DisposeEND→63 listener0→64 ACK→66 Create→67 Loaded→69 listener1；Fake每cycle5=5/4=4，Real5=5/5=5；finalIdle0/0，正常退出及精确清理|新严格合同证据强：实际原日志+source/patch+root独立复核|不追认或改写旧PASS；新build仅F2已批准WNAE+Platform=x64，NON-ORIGINAL；无系统动画切换|[原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f2/REPORT.md) → [当前证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f2/codex/REPORT.md)；PR22|
|F3|原native基线；白名单仅testsource|正文dev-only，35 checks/0fail，标题格式争议|PR11合并F1/F3后D/R native45/0、C#完整devsmoke/API均通过；真实生产面无改|原raw及新D/R CI强|措辞问题与技术通过分开；不重命名旧报告|[原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f3/REPORT.md) → [当前证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/integration/tests/REPORT.md)；PR11|
|F4|冻结AOT/ETW已知限制；无可选代码请求|PASS(dev-only)，只文档；Files属性自检未实测|PR12 exact导入AOT指引与link检查；后续Files实际runtimeconfig/EventSource证据独立在F9a/b|文档来源/链接强；泛化教程未逐宿主执行|不把后续Files特例当原F4全部命令验证；未做DEBUG可选代码|[原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f4/REPORT.md) → [当前证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/integration/docs/REPORT.md)；PR12|
|F5|原F2侧叉；窗口级source；冻结pack|原FAIL(source=event0)，首失败build.log删除后补转录|PR10先入integration；PR22入main并新回归Fake翻转恢复/UI线程2、Real一次hook跨RootView/WM0x6D unchanged不重复Set/hook移除/清理|新A/B/C/E证据强；旧删除日志链仍缺|原FAIL/补转录保留，不能用新run重造旧原件；可选D系统动画切换未做，也非必做|[原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f5/REPORT.md) → [当前证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f2/codex/REPORT.md)；PR22|
|F6|F7→F11；仅宿主patch/历史窗口证据|原非原样WNAE PASS，firstframe后Play、窗口PNG/GIF|PR13 exact导入原票及元数据；新F2baseline保留首帧后Play设计；root已亲看start/mid/settled与GIF全部8帧：空白→左缘裁断→若干不同接近就位位置→完整文字图标|来源/patch及现有定性视觉内容已直接核|原全桌面后裁切方式仍如实披露；只有1张独立命名midPNG，GIF是独立序列，不等于2张dedicated midPNG；需结题依据明确接受这些偏差或保持限制|[原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f6/REPORT.md) → [当前证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/integration/pack/REPORT.md)；PR13|
|F7|固定main源码及公开面基线；仅README/SUMS更新|原PASS(dev-only)，参考三产物SHA与CheckSurface0|PR13 exact metadata+96证据imports；原cache三SHA/复制后SHA和完整surface逐字节复核；F9b及F2实际输出再匹配|参考hash/API链强|非latest main重建pack；原native二进制未因审计重执行；禁止为凑hash改SUMS|[原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f7/REPORT.md) → [当前证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/integration/pack/REPORT.md)；PR13|
|F8a|用户未来LOCK/APPROVED决定；禁止probe|只交DRAFT|PR12 exact导入gate草稿、空审批、NoLOCK/NoProbe范围验证|DRAFT交付/冻结边界强|不是性能闸门已开；PERF-CI HOLD，不补probe或LOCK|[原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f8a/REPORT.md) → [当前证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/integration/docs/REPORT.md)；PR12|
|F8b|同F8a；两个草稿独立|只交DRAFT|PR12 exact导入、来源/链接/无审批验证|同F8a|不是APPROVED；不把DRAFT措辞视为PASS性能结果|[原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f8b/REPORT.md) → [当前证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/integration/docs/REPORT.md)；PR12|
|F9a|Files cbfb4ba5固定；VS2026 NativeDesktop；同票系统/flags许可|原样FAIL、安装5007，原未开F9b|PR14环境补齐；15原appbuildPASS；16只读preflight；17原fullsolution失败缺WIL；18批准RestorePackagesConfig原版本NON-ORIGINAL fullbuildPASS；19 VS Deploy0但未注册STOP；20手工development layout普通主页SettingsButton/normalclose/exactcleanupPASS|各失败/成功分轨，日志/权限/包清理链强|仍非原样fullsolution/VS部署成功；安装exit0含NGEN与DCAT同步warning/error；无repair/上游bug结论|[原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f9a/REPORT.md) → [当前证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f9a/devlayout-codex/REPORT_M_CONTINUATION.md)；PR14–20|
|F11|F7 reference pack；默认引用与SUMS先验|原WNAE PASS：原位/搬迁/副本corruption mismatch/复原|PR13 exact导入历史patch/raw；当前三SHA、source/API链复核；后续宿主build实际packguard通过|原搬迁/破坏记录可核；新hash/API强|未重跑原搬迁/corruption GUI；不能把nativeCI替代这类历史验收|[原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f11/REPORT.md) → [当前证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/integration/pack/REPORT.md)；PR13|
|F12|PR9已合main7e2的历史事实/用户先合许可|补记完成；标题扩写、r1hash及旧未推送措辞争议|PR12 exact导入+11行/-0；PR9 merge/tree/parents可核；原记录r1引用保留，本次不声称独立取得r1授权原文；后续PR/main链可核|历史合入事实与bytes强；措辞裁决未作|是否追加澄清由用户/真实结果评审决定；不把格式/版本标注争议升级为功能失败，也不悄改成r2冒充历史引用|[原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f12/REPORT.md) → [当前证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/integration/docs/REPORT.md)；PR12|
|F9b|F9a手工layout基线+用户直接开票及专属flags/runtime范围；冻结L1pack|原交接未开；随后用户明确开票|PR21四文件patch/17私测、pack3SHA+完整API、实际模块metadata与TokenLoad；普通主页；root看图接受opacity入场+8DIPhover/rest；5类safeevents，Created=Released3，finalIdle0/0，normalclose/exactcleanup|实际构建/图像/模块/生命周期/清理证据强|NON-ORIGINAL+manual layout；entry_position_changed=false，不声称token offsetX=-80DIP位移；无perf/adopt/原样VS成功|[F9a前置原报告](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f9a/REPORT.md) → [当前证据](https://github.com/oops11111/motion_framework/blob/eb09e63f3068a98fb011709132365e81d825c23c/z6/f9b/codex/REPORT.md)；PR21|

## 原结果复核九项缺口：逐项处置建议

以下编号严格对应[原Gemini结果复核「三／缺口」](https://github.com/oops11111/motion_framework/blob/9c28dcc6a97bda7d552ddf2a6cc6f12347063751/docs/z6_coord/z6_result_review_geminicli.md#L166)，不是把四个分歧摘要当成全部缺口。建议不代作Notion/用户裁决，也不指控责任。

|原缺口|已可证的事实/新证据|仍未闭合|建议给用户/真实结果评审的具体处置|
|---|---|---|---|
|1 F2顺序反转仍判PASS|旧报告自行披露反序；新PR22按原合同修复，两mode逐cycle严格释放后创建、active1→0→1，日志与root独立复核通过|新PASS不追认旧时的合同满足，也不是对旧票改判|明确记「旧结果保留原文并列出合同偏差；现修复独立PASS(dev-only)」；无需再接受顺序例外或重跑|
|2 TIPS线性叠放与Git/F2报告不符|原F2父F11、与F6兄弟；全13源tip现在都已为main祖先，PR11–22有精确merge/ledger归档链|作者责任/当时意图不能凭拓扑认定；原TIPS文字未修|附真实拓扑勘误与合入链，保留旧TIPS；交付可用性按当前树/新证据判断，不作欺诈断言|
|3 旧拍板卡原文未入转储|原review A3/A4自身仅「有限存疑」；新工作流/系统/flags逐票有本会话直接授权receipt|本次未打开/复核旧live卡；当前grant不追溯证明旧卡选项|保留「旧卡原文未独立可见」来源限制；若用户认为必要再提供真实卡原文，否则明确接受该历史可见性限制，不伪造追认|
|4 F12扩写及r1hash|现存+11/-0和PR9合入事实可核；实际§11的r1引用用于历史授权来源，r2是后续最终规范；exact原文随PR12保留|是否需改措辞/补r2澄清属于材料决策|建议仅在新结论中澄清「原F12引用r1；r2是本轮交接依据」，不把r1替换成r2冒充历史；本稿仅建议新结论澄清，未修改原补记字句|
|5 F5首失败原build.log删除|旧自承删除/补转录和原FAIL仍在；新F5/F2构建/运行/失败全保留新attempt，补丁与CI链独立|当时原bytes缺失不能由新run恢复，现有材料缺原bytes的限制继续披露|将其列为现有交付缺原bytes、转录不能核验原始字节的历史限制；不以新PASS替代原失败日志、不施加臆测处罚；用户/真实评审决定该限制是否阻挡结题|
|6 F6桌面后裁切/单中途帧|原报告明确采集方式及原窗口PNG/GIF；PR13 exact导入；主代理已实际亲看3PNG和GIF全部8帧，见F6_VISUAL_REVIEW.json|现有定性视觉内容已核；原capture方式并非原生window capture、单独midPNG只有1张仍是限制|记录可见空白→左clip→接近就位→完整图标文字；采集方式/交付形式偏差仍需结题依据明确接受或保持限制，不重GUI|
|7 远程是否force-push|当前Git对象/refs与普通PR11–22合并、CI完整；未见由此推出历史force证据|API_PROVENANCE只核merged/head/merge/main runs；root一次只读repo/events查询被connector400拒绝（非allowed public endpoint），未取得事件数据|明确保留「历史远程force-push未独立审计」；不可把当前正常合并链当成已证明从未force；保持历史remote更新不完整unknown，不推断有/无违规；不索凭据、绕接口、反复重试或改账号|
|8 旧侧叉磁盘已有F6/F1C不可复现|PR22给出新隔离pin、明确F5 v3来源8文件hash、十源最终patch/严格byte roundtrip和真实运行；当前合成宿主可复现|不能回到旧磁盘时间点证明当时内容/来源|接受新重现链作为当前交付证明，旧磁盘前史仍标不可独立复现；不宣称Git ancestry自动携带外部工作区变更|
|9 原F1A/B数值、F2行号、F6GIF未逐项核|本次F1_AB_SUMMARY直接选取四raw的A/B安全summary：nativeD/R33/0、A/B一致在途/交接、overlap6/6、FINAL40；PR11新D/R45/0；F2旧反序与PR22新seq链由root实核；F6图像由root单独复核|F6现有图像内容由root实核，但capture方式/独立midPNG数量限制保留；本次不读取FrameTick/PresentStats字段，不假称旧review已做这些|将每个子项证据单列，不用一句「CI绿」概括GUI/历史数值；F6原GIF8帧现已纳入实际覆盖，仍列明采集/交付例外|

## 合入/CI链：事实与结果裁决分离

主代理新增只读核验见 [HEAD_CHECKS_PROVENANCE.json](HEAD_CHECKS_PROVENANCE.json)：PR11/22各6项success，PR12–21各5项success；PR15–20另有environment-evidence skipped，旧 `.github/workflows/z6-f9a.yml` 的branch guard明确仅适用codex/z6-f9a，因此该skip是非适用项。全部观测checkruns均在merged_at之前完成且无非绿适用项；12个main post-run全部success。此记录只证明所观测PR/CI时序与适用范围，不构成历史remote更新/从未force-push的证明，也不代替Notion结果裁决。

全部13个原wb tip已是main祖先。main first-parent PR11–22完整；主代理API收据逐项核 merged/head/merge 与各main post-run completed/success。链接如下，PR21 post应为44911508536（44903228845属于PR20）。CI没有复现本机GUI/包安装，不能替代局部真实验收。

|PR|head|merge/main|main post-CI|
|---|---|---|---|
|[#11](https://github.com/oops11111/motion_framework/pull/11)|`7f13ef46e31f51b59908f4457590c33aebb068bb`|`4e128e735edf788da0b9fa9e11f0d5e2511e5585`|[run 44880677014](https://github.com/oops11111/motion_framework/actions/runs/44880677014) success|
|[#12](https://github.com/oops11111/motion_framework/pull/12)|`6191d9db816d56c3d8d34cbb76f4791341c4e4c9`|`8c5cc6f31ca2be796986afd901fc1bb1098712ca`|[run 44881810494](https://github.com/oops11111/motion_framework/actions/runs/44881810494) success|
|[#13](https://github.com/oops11111/motion_framework/pull/13)|`db208be6da7442b9105efbd767a6a5ccf8c6d710`|`122f7b4edc7698e742658532e3e1a83010f39f17`|[run 44883079762](https://github.com/oops11111/motion_framework/actions/runs/44883079762) success|
|[#14](https://github.com/oops11111/motion_framework/pull/14)|`fba233e688e396e7d26e8e9bcbcfd091ba00b376`|`3078ae0e9f8792a03f45e09dd4fce5c6fda1ca7e`|[run 44887910560](https://github.com/oops11111/motion_framework/actions/runs/44887910560) success|
|[#15](https://github.com/oops11111/motion_framework/pull/15)|`cecc685923824a106f2faf537fddd53dcf2795d9`|`425165e112c271dc76706e5ff2d5354b1dfc9120`|[run 44889148641](https://github.com/oops11111/motion_framework/actions/runs/44889148641) success|
|[#16](https://github.com/oops11111/motion_framework/pull/16)|`0eb69f17282a6d75a49550fe42dea394de3a099b`|`8e68cf81ef10bd0623651949e6a98ddef4f175ca`|[run 44890658744](https://github.com/oops11111/motion_framework/actions/runs/44890658744) success|
|[#17](https://github.com/oops11111/motion_framework/pull/17)|`1074cbea2295cae9eadd9bc4617b9e57433d7e40`|`81543c86fc67801aa2de00f44d8b267c2ac09ee9`|[run 44891508566](https://github.com/oops11111/motion_framework/actions/runs/44891508566) success|
|[#18](https://github.com/oops11111/motion_framework/pull/18)|`32bf48a5cc7f65557e62c9769c3a3646befcd74b`|`92c498fbcf8ea7216d54104886a722d34d5099fb`|[run 44895678100](https://github.com/oops11111/motion_framework/actions/runs/44895678100) success|
|[#19](https://github.com/oops11111/motion_framework/pull/19)|`579154af5b2b34bdd444742b6d336cb4b282598f`|`0fc838c9624b4a23c99bbc57f3d5d1c9bf8c2510`|[run 44900488544](https://github.com/oops11111/motion_framework/actions/runs/44900488544) success|
|[#20](https://github.com/oops11111/motion_framework/pull/20)|`a04cfbc5d4c231fc11070ed59f428787b8ee662a`|`05830996127cd40d32f3718a2ae253583d092f36`|[run 44903228845](https://github.com/oops11111/motion_framework/actions/runs/44903228845) success|
|[#21](https://github.com/oops11111/motion_framework/pull/21)|`eef9be49e3491ad794edad7148cf089e410f75d9`|`d2aac2dc355d0412aa5fc3ede212b79faf4459b9`|[run 44911508536](https://github.com/oops11111/motion_framework/actions/runs/44911508536) success|
|[#22](https://github.com/oops11111/motion_framework/pull/22)|`51076afefe4ca5848fc80a4d97f9ec48708b00f1`|`eb09e63f3068a98fb011709132365e81d825c23c`|[run 44978158079](https://github.com/oops11111/motion_framework/actions/runs/44978158079) success|

## 精确剩余材料和收尾选择（未实施）

coord `9c28dcc6a97bda7d552ddf2a6cc6f12347063751`仍缺 `docs/z6_coord/z6_result_review_notion.md` 和 `docs/z6_coord/z6_closed_DRAFT_notion.md`。[handover adjudication](https://github.com/oops11111/motion_framework/blob/9c28dcc6a97bda7d552ddf2a6cc6f12347063751/docs/z6_coord/z6_handover_adjudication_notion.md)是交接规则裁决，不是结果裁决；N9不把外部评审设为main硬门，不能由此虚构结果稿或自动准许正式结题。

建议让用户在两个具体依据间决定：

1. 提供真实Notion结果复核和结题草稿；按真实材料逐项对照本九gap证据，保留分歧，之后再提出canonical closure的最小Plan。
2. 明确以本技术草稿 + 主代理独立复核作为本轮结果依据，接受上表仍披露的历史限制，并指定是否允许代替缺失材料；然后另Plan正式结题文档/CI/PR范围。不得给该材料冒名「Notion评审」。

这两项都是后续决策，当前没有获得替代缺稿/正式结题的指令。本草稿不预写closed草稿、不改旧票结论、不解除PERF-CI、不产生LOCK/APPROVED。F6现有定性视觉内容已直接核，采集/交付形式限制仍保留；历史卡/force审计/缺失旧日志bytes/旧磁盘前史/F12澄清是明确的材料与来源限制，而非继续无改动重测的理由。
