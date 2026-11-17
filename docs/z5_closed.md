# Z5 结题（定稿 · 待落盘为 docs/z5_closed.md · 未 LOCK）

- **STATUS**：定稿（待落盘）。只表示 Z5 本轮执行已复核完毕，**不是** LOCK，**不是** adopt，**不合 main**。
- **起草**：notion ai，2026-11-18 02:10 AWST（Australia/Perth，UTC+8）；02:20 AWST 更新第 6、7 节（授权链核实）；02:45 AWST 并入 Gemini cli 对照复核，并修正判定和措辞（第 7 节）；06:10 AWST 定稿：核实 C1/C2 已推送，新增第 9 节「已知限制」、第 10 节「本地副作用（清理后）」，授权链补 06:03 四句。
- **复核依据**：只读核对 GitHub 上的 `wb/z5-*` 分支和库内证据。详细意见见 `z5_result_review_notion.md`。
- **硬边界（不变）**：
  - Success≠adopt；不合 main，合 main 须 pidan 明确口令。
  - composition.h 冻结；L1 合同 `77416e8b…3a0f` 冻结，L1 公开面、fm_*、E1 都不改。
  - 无 LOCK+APPROVED 不采数；**PERF-CI HOLD**。
  - 不改任何 *_LOCKED.md；不 force/amend/rebase。
- **性质**：本轮全部是 dev-only，不计任何闸门会话，没有任何计时判据；PresentStats 记 N/A，它是占位值。

## 1. 基线与分支

- 基线：main @ `2e57bff384c771cc972840ca98a1f0bb92aaeb98`。2026-11-18 06:10 AWST 再次 `git fetch` 核对：`origin/main` 仍是这个提交，下表 7 个分支都没合入。

| 项 | 分支 | 远端 tip（全量） | 父提交 | 判定 |
|---|---|---|---|---|
| T1 Z3a 验收事后补证 | wb/z5-t1 | `865c458d07dee8aded04b904db3bfe0f845d7aa4` | 2e57bff | **ACCEPT** |
| T2 文档补记 | wb/z5-t2 | `f5bdf96648e2a800f126ceb3279a0df42e9d47fd` | 2e57bff | **ACCEPT** |
| T3 L1 内部小修 | wb/z5-t3 | `d618140b4aaaec1c1a61e42c57e35ea3004f54f5` | 2e57bff | **ACCEPT** |
| T4 FluentHub 基线（非原样） | wb/z5-t4 | **`b592b6c80d052a5c013ee8dad6c2cbff3df40506`**（C1 更正提交）；结果提交 `6d8c8e5c7e5afefface668e1bc53245f823a0c2d` | 6d8c8e5 ← b0af25d ← bec36ac ← 2e57bff | **有条件 ACCEPT** |
| T5-0 L1 试点打包 | wb/z5-t5-pack | `0a166e1822c83d1d3e8dc9f314a0f74e07d0dbb2` | d618140（T3 tip） | **有条件 ACCEPT**（02:45 由 ACCEPT 下调：库内没有对 pack DLL 跑 CheckSurface 的原始输出，措辞降级） |
| T5 FluentHub NavigationView 试点 | wb/z5-t5 | **`c59edddc4bc175128dfa6751abeed8fbb2b6670c`**（C2 更正提交）；结果提交 `3809757673564f7b9702921adbd9e4fd9071197e` | 3809757 ← 0a166e1（T5-0 tip） | **有条件 ACCEPT** |
| T6 K93 回调半刷新率诊断 | wb/z5-t6 | `635c774fedfa664471c7276fdef0af6abbe87780` | 2e57bff | **ACCEPT** |

- 执行顺序符合评审要求：T1 → T3 → T5-0 → T5，T2、T4、T6 并行。
  - T1 提交于 00:45，T3 于 01:07，T5-0 于 01:14，T5 于 01:55（AWST）。
  - T5-0 从含 T3 的 tip 用 Release 构建。
  - T5 基于 T5-0，只消费预编 DLL。
- **C1/C2 更正提交**（06:05 AWST，按 pidan 06:03 许可推送，06:10 AWST notion ai 用 `git fetch` + `git diff` 核实）：
  - **C1 `b592b6c8`**：父提交 6d8c8e5c；只改 `pilot/fluenthub/BASELINE.md`，+1/-0；在标题行下插入「最新结论」一行，原文未删，内容和第 7 节 C1 文本逐字一致。
  - **C2 `c59edddc`**：父提交 3809757；只改 `pilot/fluenthub/T5_REPORT.md`，+2/-0（更正句加一个空行）；位置在第 8 节末、「## 9. 本地副作用」之前，原文未删，内容和第 7 节 C2 文本逐字一致。
  - 两个更正提交都只改报告文字，不改证据，也不影响判定。

## 2. 不变量核实结果

| 不变量 | 结果 | 证据 |
|---|---|---|
| composition.h 不变 | 通过 | 7 个分支的 `code/composition.h` blob 都是 `fd0603c1…`，和 2e57bff 相同；归一化（CRLF→LF）sha256 是 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`。T1 和 T3 的 PRE/POST 记录也一致 |
| fm_* / C-ABI 不变 | 通过 | 7 个分支的 `code/` 都没有 diff |
| L1 公开面不变 | 通过 | T3 的 Debug/Release 公开面 dump 前后逐字相同，`CHECKSURFACE fails=0`；T5 的 pack DLL 和 FluentHub 输出目录副本 dump 逐字相同（sha256 `7b67b77b…c5cc`），且和 T3 Release devsmoke 程序集 dump 相比只少 devsmoke 自身的 `Z3aL1DevSmoke` 类型。注意：库内**没有**对 pack DLL 的 CheckSurface 原始输出，「20/20」只见于报告文字 |
| E1（事件名、id、字段、GUID）不变 | 通过 | MotionEtw.cs 只在 T2 改了注释；T3 只多一次发射，schema 不变 |
| L1 合同 `77416e8b…3a0f` 冻结 | 通过 | 7 个分支的 *_LOCKED.md diff 条数都是 0 |
| PresentStats | N/A | 占位值，T2 已登记为已知限制 |
| PERF-CI | HOLD | 没有任何计时判据 |
| 不合 main | 通过 | `origin/main` = 2e57bff |

## 3. 各项结论与证据

### T1 · ACCEPT（事后补证）
- 树等价：`fe59c622` 和 `2e57bff` 的 `code/`、`MotionSoftDemo/`、`tools/` 三个 tree 相同；全树 diff 只在 `docs/`（6 条）和 `z4/`（39 条）。
- 测试：`fm_capi_test` 23 checks / 0 failures；`--z3a-l1-dev` verdict=PASS fails=0，六个 E1 事件都在。
- 证据目录：`z3a/results_dev/K93N3P8/`（README、tree_equivalence.txt、hashes.txt 等）。
- **补注**：`exclude_default_obj.props/.targets` 的仓库副本是 247/306 字节，Z4 K93 s4 BUILD_NOTES 记的是 248/307 字节，sha 也不同，疑为末尾换行。README 写的「字节相同」未能由库内证据证实。这两个文件只用来排除 obj 目录，不影响树等价和测试结论。
- 口径：**事后补证，非同期记录**，不能证明 2026-11-11 06:38 AWST 之前的状态；Z4 r1 结论不升级也不降级，仍绑定 `fe59c622`。

### T2 · ACCEPT
- 改了 4 个文件：`api_v0_frozen.md`（PresentStats 已知限制）、`api_changelog.md`（C# 形态登记和已知限制）、`MotionEtw.cs`（只改注释）、`Z_SERIES_CONVERGE.md`（闸门口径说明）。
- `Z_SERIES_CONVERGE.md` 的 FILE_SHA256 `3e3f5047…2af7` 经复算一致。UPDATED 行随之更新，属于文件头约定，不算越界，也已披露。

### T3 · ACCEPT
- `FM_FLUIDMOTION_PATH` 只在 Debug 读取（`#if DEBUG`）。`envvar_diag_*` 实测证明 Debug 读、Release 忽略。
- `DestroyCore` 在帧泵开着时关泵，补发一次 `IdleTransition(0,0)`。新的必测用例在 Debug 和 Release 下都是 `emitted=1 pumpOffCallbacks=1`。
- Debug 和 Release 的 fm_capi_test 都是 23/0，devsmoke 控制台和 GUI 都 PASS，公开面 diff 为空。
- 新用例把测试前置改成 `from: 0`，只影响新用例；`from=null` 遇到没有现值的属性会被拒，这是 fm_capi_test 里已有的语义，不削弱测试。
- 证据目录：`z5/t3_results_dev/K93N3P8/`。
- **Release 行为变化（须知）**：自 T3 起，**Release 构建不再读取 `FM_FLUIDMOTION_PATH`**，只走默认 DLL 探测（`fluidmotion.dll` 须和程序集放在一起）；Debug 行为不变。
  - 这是有意的安全修复，分支内 changelog 已登记「是（仅内部行为：Release 不再读环境变量…）」。
  - 我在 T3 tip 上 `git grep` 过，z4/ 下没有任何 Release+该变量的用法。
  - 今后凡是依赖这个变量的 Release 复跑都会失效。
- 今后任何新闸门须绑定**包含 `d618140b` 的 tip**（即 d618140b 或其后继），并在闸门正文写明具体 SHA。Z4 r1 仍绑定 fe59c622。

### T4 · 有条件 ACCEPT（**非原样**基线）
- **FluentHub**：`eb08f0dbcbb43880ab34038b0cb6742f0c0deca5`。工程本身设了 `PublishAot=true`、`SelfContained=true`、`TreatWarningsAsErrors=true`（Directory.Build.props）。
- **原样构建 FAIL**：NU1903（ILCompiler 10.0.8 安全公告）被当成错误。这条记录保持不变。
- **非原样三点**：
  - ① 命令行加 `-p:NuGetAudit=false`，不改 FluentHub 任何文件。
  - ② Windows SDK 10.0.26100 由 pidan 本人安装，属于系统级操作。
  - ③ 运行时实际解析到 `Microsoft.WindowsAppRuntime.2` **2.6.5**，代替清单里的 2.0.x，加载模块清单据报告，日志未入库。
- **结果**：
  - **库内证据**：截图 `pilot/fluenthub/t4_fluenthub_window.png` 显示应用启动到未登录主界面。
  - **只见于 BASELINE 报告文字、日志没有入库**（日志原在 K93 的 `%TEMP%\z5_wave1\t4\rebuild2\`，已按 pidan 06:03 许可删除，库外没有副本）：Debug x64 构建「0 警告 0 错误」、用户级 DevMode 松散注册、运行时实际解析到 2.6.5（加载模块清单）。
  - 结题对这几项的表述是「**据 workbuddy 报告**」，不当作已由库内日志证实。日志已删，所以这几项**永久只有报告文字作证**。
- **没有做 publish，也没有生成 AOT 产物**。
- **注意**：`BASELINE.md` 首行仍写「结果：FAIL」，它指第 3 节的原样构建，**最新结论以第 8 节为准**。已由 C1 `b592b6c8` 在标题下补充最新结论行（按 pidan 06:03 许可推送）。
- **本地副作用**：用户级包注册已按 pidan 06:03 许可清除（据 workbuddy 报告，见第 10 节）。

### T5-0 · ACCEPT
- **新增文件**：只有 `pilot/l1pack/{FluidMotion.L1.csproj, README.md, SHA256SUMS}`。链接编译的两个源文件和 T3 tree 一致：MotionSystem.cs `fe72b08f…`，MotionEtw.cs `565111f0…`。
- **namespace / AssemblyName**：namespace 仍是 MotionSoftDemo，没有设 AssemblyName。
- **构建和验收**：
  - Release 构建，依据是 csproj/README/SHA256SUMS 的声明。
  - **公开面**：pack DLL 的 dump（`pilot/fluenthub/surface_l1pack.txt`）和 T3 Release devsmoke 程序集的 dump 相比，只少 `Z3aL1DevSmoke` 自身类型；而后者的 CheckSurface 原始输出（`z5/t3_results_dev/K93N3P8/surface_ds_Release.checksurface.txt`）是 fails=0。
  - 结题只写「**公开面 dump 与已通过 CheckSurface 的 T3 Release 程序集一致**」，**不写「对 pack DLL 跑 CheckSurface 20/20」**，后者只见于 README 文字，没有原始输出入库。
- **标注**：pilot-only，不对外分发，是 devsmoke 之外唯一的链接编译点。
- **SHA256SUMS**：
  - FluidMotion.L1.dll `2e2af911d484c3068d1a59b42a81637f153409370d9a3ecae7dd8714a0b29865`
  - fluidmotion.dll `b39d58ef612f7dd2cd89cc4373e02954fb02e93723cae7632cf55c018d1e6996`
  - demo_motion.json `8d5db5775acad5db0c04cbc3c2a214bb7309670c304b71ff40860757973c410e`
- **局限**：二进制不入库，「确为 Release 构建」依据的是构建记录。

### T5 · 有条件 ACCEPT
- **侧叉**：FluentHub 本地侧叉 @ `eb08f0db`，不推 fork，不提上游。补丁 `pilot/fluenthub/fluenthub_t5_side_fork.patch`，sha256 `79b00ba22f8d45a0999a0a48e59be8b490608f29cbfdf041359e7c670b92e6ab`。改动是 csproj（HintPath 引用 pack DLL、`EventSourceSupport=true`）、`RootView.xaml.cs` +5 行、新增 `Views/NavMotionPilot.cs`。
- **行为**：
  - Loaded 时 Play 入场动画，带防重入。
  - 悬停时 Retarget offsetX=8；移开（Exited/Canceled/CaptureLost）时对**同一句柄** Retarget 回 0。
  - Unloaded 或 Closed 时对每个句柄先 Cancel 再 Release，然后 Dispose，有 `_disposed` 标志。
  - FramePumpChanged 时订阅或退订 `CompositionTarget.Rendering`。
- **验收**：编过（据报告：0 警告 0 错误，构建日志 build1/build2.log 原在 K93 的 `%TEMP%\z5_wave2\t5\`，未入库，已按 pidan 06:03 许可删除；库内间接证据是运行日志和截图）；启动；悬停动效可见，截图只截当前窗口；ETW 有 5 个事件名（AnimationLifecycle 28、Retarget 8、IdleTransition 8、TokenLoad 1、FrameTick 只计数 85）；**Created=Played=Cancelled=Released=7**，Created=Released，没有泄漏；SurfaceDump diff 为空。日志 `z5_t5_motion_run2.log`。
- **环境**：控制台会话，`UISettings.AnimationsEnabled=True`，因此 `SetReducedMotion(False)`。
- **仲裁**：入场和悬停都驱动 offsetX。语义只是**从 controller.h 读得**（后写者胜），本次运行中两个持有者**没有真正重叠**（每次悬停时入场都已结束）。**结题不写「仲裁已验证」**（F1）。
- **侧叉构建配置**：csproj 用绝对路径 `FmL1PackDir=C:\Users\admin\mf_z5\t5pack\…` 引用 DLL，构建时**不校验** sha256。sha 是引用前人工核对的（报告第 1 节三者 MATCH）。仅限本地 pilot，记 F11。
- **报告措辞**：`T5_REPORT.md` §8 写「仅本地提交，未 push（未获 pidan 推送许可）」已过时。实际情况：02:02 pidan 本人拍板卡「wb/z5-t5 可以普通 push，推完交 Claude 和 notion 复核并结题」之后已普通 push。已由 C2 `c59edddc` 在第 8 节末追加更正（按 pidan 06:03 许可推送）。
- 结题条件见第 4 节的偏差和第 5 节的后续票。这些都不阻塞。

### T6 · ACCEPT
- `docs/k93_frame_rate_diag.md`。「K93 现在」和「Z4 采数时」分开记录。
- 现在：控制台会话、GTX 970 物理卡、1920×1080@60Hz、AnimationsEnabled=True、HwSchMode=1。
- Z4 采数时的帧间隔 p50 约 31.2–31.3 ms，只作旁证。H1–H8 只列假设，不下结论，没有新的计时测量。

## 4. 偏差（如实记录，都不阻塞）

| # | 项 | 偏差 | 处理 |
|---|---|---|---|
| D1 | T1 | 首跑原生构建日志因 cmd 重定向写法丢了退出码 | 代码没改，完整重跑一次（同样 23/0）；原日志保留在 `attempt1_logging_bug/` |
| D2 | T1 | 二进制 sha256 和 Z4 记录不同 | 按工具链/构建路径差异记录，不算失败 |
| D3 | T3 | 新用例首跑 4 条 FAIL，原因是测试前置 `from=null` 被拒 | 测试改为 `from: 0`，产品代码没动；首跑结果保留在 `attempt1_testbug/` |
| D4 | T3 | GUI Debug 首跑用的是旧二进制（mtime 保留，增量没重编） | `--no-incremental` 重跑后 PASS，日志已入库 |
| D5 | T4 | 非原样：`-p:NuGetAudit=false`；SDK 由 pidan 安装；运行时 2.6.5 代替 2.0.x | 已记录；原样构建的 FAIL 记录保留 |
| D6 | T5 | FluentHub 的 `PublishAot=true` 让 runtimeconfig 关掉了 EventSource，run1 的 ETW 为 0 | 侧叉 csproj 加 `EventSourceSupport=true`，仅本地；run1 日志 `z5_t5_motion_run1_etw_off.log` 已入库 |
| D7 | T5 | `SetCursorPos` 不触发 WinUI 指针事件 | GUI 测试脚本改用 `SendInput`，应用代码没动 |
| D8 | T5 | 入场动画在窗口首帧前基本跑完 | 入场只有 ETW 和 sink 计数作证据，没有画面证据；不做任何计时判断 |
| D9 | T5 报告措辞 | §8「仅本地提交，未 push」已过时（02:02 pidan 本人许可后已推送）；报告时间段写到 02:00，但提交时间是 01:55:30 | §8 已由 C2 `c59edddc` 更正；时间段出入以本结题文档为准 |

## 5. 未覆盖项与后续票（都不阻塞本轮结题；每张后续票都须另行开票，开票前不执行）

| 票 | 内容 | 不阻塞的理由 |
|---|---|---|
| F1 | 入场进行中被悬停打断（同一 (target, offsetX) 两个持有者重叠）的运行证据；**仲裁未经运行验证** | 语义只是从 controller.h 读得（后写者胜；Retarget 起点优先级：自身值 → 其他持有者 → rest → from）；本轮只是 pilot，结题不作「已验证」论断 |
| F2 | RootView 重建（Unloaded→Loaded，先 Dispose 后 Create）的运行证据 | 代码路径已就位，缺的是宿主侧实测 |
| F3 | 在 fm_capi_test 里补「空动画 + Play + Retarget」的 C 层用例（只改测试文件，不动 fm_capi.h/.cpp） | devsmoke 第 103–106 行、T3 新用例和 T5 运行时都已走过这条路径 |
| F4 | ~~AOT 宿主（PublishAot）默认关闭 EventSource~~ → **已登记为已知限制**（第 9 节，pidan 06:03） | 宿主侧指引或 L1 侧检测提示如果要做，须另行开票 |
| F5 | ReducedMotion 跟随系统设置在运行中的变化（目前只在启动时读一次） | 本轮实测 True，验收不受影响 |
| F6 | 入场动画的可见性（例如等首帧再 Play），属于宿主侧改进 | 验收本来就没有计时判据 |
| F7 | 整合须知：① 各分支互相独立；T2 改了 MotionEtw.cs 注释，blob 会变，和 pack 的 SHA256SUMS 不一致。② **`docs/api_changelog.md` 冲突**：T2 和 T3 都在同一张表末尾追加行，`git merge-tree --write-tree origin/wb/z5-t2 origin/wb/z5-t3` 实测 `CONFLICT (content)`，退出码 1；t2 和 t5、t2 和 t5-pack 同样冲突；t2+t4、t1+t3、t3+t6 无冲突 | 只在将来整合时处理：须 pidan 合并口令；冲突是两边都追加表行，解决办法是三行全部保留、按时间排序；之后重打 pack、重算哈希，新闸门绑定包含 d618140b 的 tip |
| F11 | 侧叉 csproj 改为相对路径或可配置路径，构建时校验 pack DLL 的 sha256 | 仅限本地 pilot；本轮已人工核对 sha |
| F8 | 真实 PresentStats；K93 约 32Hz 的计时探针 | 须带 LOCK 的票；本轮禁止计时 |
| F9 | Files（C 试点） | 本轮不做，另行开票 |
| F10 | ~~清理本地副作用~~ → **已按 pidan 06:03 处置**（第 10 节）：包注册和 %TEMP% 日志已清（据 workbuddy 报告），worktree 保留 | 剩下的 worktree 和侧叉何时清理，由 pidan 日后决定 |

## 6. 授权链（逐字引用；2026-11-18 02:20 AWST 已由 notion ai 在用户对话记录中逐条核实）

**来源分级**
- **①**：notion ai 已在用户对话记录中核实，确为 pidan 本人原话或本人对拍板卡的回答。
- **②**：协调称亲见，notion ai 未能核实。
- **③**：只有 workbuddy 转述。

本表全部为 ①，没有 ② 或 ③（06:10 AWST 补核 06:03 四句，同为 ①）。

**核实方法**：用 ReadTranscript / SearchTranscripts 只读查看两个会话：协调 bot「动效框架」（933e43b0-87b8-49ae-a202-1979d2126a42）和 workbuddy cli（0defd8c4-7593-499d-9560-8ad5773c4226）。判定依据：
- 用户本人消息：role=user，speakerId=`cu:476842336`，或带 `[Sent from machine 7aed10fb-…]` 的 user_query。
- 拍板卡回答：user_query 里带 `[Answering your question tNNNsNNNN: "…"]`。
- agent 发来的消息（speaker 为 agent:…）不算用户本人。

| 时间（AWST，2026-11-18） | 原文（逐字；换行用 / 表示） | 形式与位置 | 分级 |
|---|---|---|---|
| 00:15 | 「检查进度，/ 检查Claude恢复情况 / 规划开发任务交workbuddy / 任务细节交Gemini，Claude和notion讨论通过后执行」 | 动效框架会话中的用户本人消息（`[t451u]`，speakerId cu:476842336；SearchTranscripts 位置 2001） | ① |
| 00:46 | 「开 Z5-T3、Z5-T5 编码票」 | 动效框架会话中的拍板卡回答（`t453s2018`，卡片问题「开 Z5-T3（L1 内部小修）和 Z5-T5（FluentHub 试点接入）两张编码票吗？」） | ① |
| 00:54 | 「可以，Z5 四个分支做完就普通 push」 | workbuddy 会话中的拍板卡回答（`t195s943`，卡片问题「Z5 四项做完本地 commit 后，可以把 wb/z5-t1/t2/t4/t6 四个新分支普通 push 到远程吗？（不合 main、不 force）」） | ① |
| 01:18 | 「T4 用命令行属性重做基线（如 -p:NuGetAudit=false），不改 FluentHub 文件，记非原样；2.6.5 可替代 2.0.x」 | workbuddy 会话中的拍板卡回答（`t196s953`，卡片问题「T4 FluentHub 基线下一步怎么走？」）；同一分钟 pidan 还发了「建议？」（`[t197u]`） | ① |
| 01:19 | 「T4 用命令行参数忽略 NU1903 重做基线，不改文件，标非原样；运行时用 2.6.5」 / 「wb/z5-t3、wb/z5-t5-pack 可以普通 push」 | 动效框架会话中的拍板卡回答（`t456s2032`，标题「Z5 两个拍板」，下设两问：「T4 FluentHub 编不过，怎么处理？」「第二波 T3、T5-0 分支能推吗？」） | ① |
| 01:25 | 「T4 我在 K93 装 Windows SDK 10.0.26100，装完用同一命令重跑」 | 动效框架会话中的拍板卡回答（`t458s2042`，卡片问题「T4 缺 Windows SDK 10.0.26100，怎么办？」） | ① |
| 01:34 | 「已安装」 | workbuddy 会话中的用户本人消息（`[t200u]`） | ① |
| 02:02 | 「wb/z5-t5 可以普通 push，推完交 Claude 和 notion 复核并结题」 | 动效框架会话中的拍板卡回答（`t459s2049`，卡片问题「wb/z5-t5 能普通 push 吗？」） | ① |
| 02:08 | 「额度用完就交Gemini cli」 | 动效框架会话中的用户本人消息（`[t462u]`） | ① |
| 06:03 | 「C1、C2 更正可以追加提交并普通 push 到 wb/z5-t4、wb/z5-t5」 | 动效框架会话中的拍板卡回答（`t466s2079`「Z5 结题拍板」第 1 问「workbuddy 在 wb/z5-t4、wb/z5-t5 各追加一个更正提交并普通 push？」）；ReadTranscript 全量页 positions 5160–5212 | ① |
| 06:03 | 「Z5 结题文档可以落盘并普通 push 到新分支，不合 main」 | 同一张卡 `t466s2079` 第 2 问「结题文档 docs/z5_closed.md 落盘并推到新分支（不合 main）？」 | ① |
| 06:03 | 「AOT 宿主默认关 EventSource 登记为已知限制」 | 同一张卡 `t466s2079` 第 3 问「「AOT 宿主默认关 EventSource」登记为已知限制？」 | ① |
| 06:03 | 「K93 清掉 FluentHub 包注册和 %TEMP% 日志，worktree 保留」 | 同一张卡 `t466s2079` 第 4 问「K93 上的包注册、工作目录和临时日志？」 | ① |

**T4 的两版文字（并列，两版都是 pidan 本人的拍板卡回答，分别回答两个 bot 各自的卡片，内容一致）**
- 01:18，回答 workbuddy 的卡片：「T4 用命令行属性重做基线（如 -p:NuGetAudit=false），不改 FluentHub 文件，记非原样；2.6.5 可替代 2.0.x」
- 01:19，回答协调的卡片：「T4 用命令行参数忽略 NU1903 重做基线，不改文件，标非原样；运行时用 2.6.5」

**几点说明**
- 拍板卡回答是 pidan 对 agent 所拟卡片的点选或填写，按本人决定计。
- 00:15 原文是分行写的，库内派工单引用时改成了逗号连写，字词没有变。
- 库内各文档（wave1/wave2 派工单、T3 分支的 api_changelog、l1pack/README、T5_REPORT、BASELINE §7/§8）对上述原话的引用，经核对和原文一致。

**授权链缺口：已关闭**。06:03 四句另外授权了 C1/C2 推送、结题落盘到新分支、已知限制登记和 K93 清理。T3 和 T5 编码票（00:46）、各分支推送（00:54、01:19、02:02）、T4 的系统级步骤（01:18、01:19、01:25、01:34）、结题指令（02:02）都已核实为 pidan 本人。

注意：授权链关闭**不等于** LOCK、adopt 或合并。本文件不 LOCK。落盘为 `docs/z5_closed.md` 并普通 push 到新分支，已获 pidan 06:03 许可；合 main 仍须 pidan 明确口令。

## 7. Gemini cli 对照（2026-11-18 02:45 AWST 并入）

- **依据**：pidan 2026-11-18 02:08 AWST 本人消息「额度用完就交Gemini cli」（第 6 节）。Claude 额度用尽，约 13:30 AWST 恢复。
- **Gemini cli 判定**（经协调转述）：T1、T6 ACCEPT；T2、T3、T5-0、T4、T5 有条件；无 REJECT。
- **核实方式**：notion ai 在 `/workspace/mf_main_ro` 用 git 逐条独立核实，下表的裁决以核实结果为准（分歧由 notion ai 裁决）。

| # | Gemini 意见 | notion ai 核实 | 裁决 |
|---|---|---|---|
| G1 | T2 改了 4 个文件，含 `api_v0_frozen.md` | 属实（我原先也是这么记的）。`api_v0_frozen.md` **不是 LOCKED 文件**：仓库里的 LOCKED 文件都以 `_LOCKED.md` 结尾，冻结合同是 `api_surface_m2_alpha_contract_LOCKED.md`（`77416e8b…`），后者没改；`api_v0_frozen.md` 是「冻结规则」说明文档。T2 只在文末**追加**「已知限制」一节（PresentStats 占位），在计划的 T2 文件清单之内 | **不越界**。T2 维持 ACCEPT |
| G2 | T2 和 T3 同改 api_changelog.md 的同一位置，会冲突 | **属实**：merge-tree 实测冲突（见 F7） | 采纳，写入整合须知 F7。这不是 T2 本身的缺陷（各分支按计划各自独立，合并另需口令），所以 T2 不下调 |
| G3 | T3 的 `#if DEBUG` 是 Release 行为变化，结题须写明；新闸门绑定 d618140b | 属实。分支内 changelog 已登记这一行为变化 | **采纳措辞**：第 3 节 T3 已写明 Release 行为变化。绑定口径改为「包含 d618140b 的 tip，闸门正文写明具体 SHA」（只绑 d618140b 一个提交过窄）。T3 维持 ACCEPT，因为分支内已披露，只需在结题文档里写明 |
| G4 | T5-0/T5「CheckSurface 20/20 / fails=0」在分支内没有对 pack DLL 的原始输出 | **属实**：`git grep` 只在 l1pack/README 和 T5_REPORT 的文字里找到；`surface_l1pack.txt` 是 47 行公开面 dump，不含 CHECKSURFACE 行。我先前写的「CheckSurface 20/20」依据的是 README 文字，**更正** | **降级措辞，不补采**：改为「pack DLL 公开面 dump 与 T3 Release 程序集一致（只少 Z3aL1DevSmoke），后者 CheckSurface 原始输出 fails=0」。**T5-0 由 ACCEPT 下调为有条件 ACCEPT** |
| G5 | T4「0 警 0 错」「运行时 2.6.5」只见于报告文字，日志未入库；BASELINE.md 第 1 行仍写 FAIL | 属实：wb/z5-t4 只有 BASELINE.md 和截图两个文件 | 前半**降级措辞**（「据 workbuddy 报告」，截图是唯一库内证据）。后半**须追加提交 C1**：首行会误导后续读者 |
| G6 | T5 §8「未 push」须更正并写明授权；侧叉 csproj 硬编码绝对路径、构建时不校验 sha；两个持有者真正重叠没跑到，不得写「仲裁已验证」 | 都属实（补丁里 `FmL1PackDir` 是绝对路径，没有校验步骤；run2 日志里每次悬停时入场都已结束） | §8 **须追加提交 C2**（误导读者，并补写授权）；路径和 sha 校验记**后续票 F11**；「仲裁已验证」**不写**，F1 改为「仲裁未经运行验证」 |
| G7 | T1 的 exclude_default_obj.props/.targets 仓库副本各少 1 字节 | **属实**：仓库副本 247/306 字节，Z4 K93 s4 BUILD_NOTES 记录的是 248/307 字节、sha `2fcd67a7…`/`994fae02…`；副本 sha `9e548c9c…`/`4b67a243…`，结尾只有一个 `\n`。T1 README 写的「字节相同」**没有库内证据支撑** | **只写文档**：这两个文件只用于排除 obj 目录，不影响树等价（tree 哈希）和测试结论。结题注明「副本与 Z4 记录相差 1 字节（疑为末尾换行），『字节相同』未能证实」。T1 维持 ACCEPT，不要求追加提交 |
| G8 | 可选：T4/T5 关键日志摘要入库 | — | **可选，不要求**：降级措辞已经足够 |

**须 workbuddy 分支追加提交的（已按 pidan 06:03 拍板卡「C1、C2 更正可以追加提交并普通 push 到 wb/z5-t4、wb/z5-t5」推送：C1 → `b592b6c80d052a5c013ee8dad6c2cbff3df40506`，C2 → `c59edddc4bc175128dfa6751abeed8fbb2b6670c`；普通 commit + 普通 push，没有 amend/force/rebase；06:10 AWST notion ai 核实内容和下文逐字一致）**
- **C1 · wb/z5-t4**：在 `pilot/fluenthub/BASELINE.md` 标题行下插入一行状态说明，原文不删：
  「> 最新结论（2026-11-18 01:40 AWST，第 8 节）：**非原样基线 PASS**（`-p:NuGetAudit=false`；SDK 10.0.26100 由 pidan 本人安装；运行时 2.6.5 替代 2.0.x）。标题行的 FAIL 只指第 3 节原样构建。构建/运行日志在 K93 `%TEMP%`，未入库。」
- **C2 · wb/z5-t5**：在 `pilot/fluenthub/T5_REPORT.md` 第 8 节末尾追加更正，原文不删：
  「更正（2026-11-18）：pidan 02:02 AWST 拍板卡「wb/z5-t5 可以普通 push，推完交 Claude 和 notion 复核并结题」之后，本分支已普通 push（远端 `3809757…`）。第 4.4 节「两边 CHECKSURFACE fails=0」没有对 pack DLL 的原始输出入库，以公开面 dump 一致为准。」

**只写进结题文档、不改分支的**：G1、G2、G3、G4、G5 前半、G6 的路径/sha 和仲裁两点、G7、G8。

**Claude 对照**：Claude 恢复后如有结果复核，同样只作对照。它在**计划**阶段提的条件已逐条落实：
- T1 全树 name-status；
- T2 写成裁决口径；
- T3 先 grep、必测用例、changelog 三点；
- T4 如实写注册情况和清理方法、记录会话类型；
- T5-0 头注释和 hash-object；
- T5 同一句柄 Retarget、Dispose 里 Cancel+Release、处理 Canceled 和 CaptureLost、`_disposed`、EventListener 缓存和缓冲上限、不写计时结论；
- T6 分两节、加适配器假设。

## 9. 已知限制（按 pidan 2026-11-18 06:03 AWST 拍板卡「AOT 宿主默认关 EventSource 登记为已知限制」登记）

- **K-AOT-ETW · AOT 宿主默认关闭 EventSource**
  - 宿主工程设了 `PublishAot=true` 时，.NET SDK 会在 runtimeconfig 里写入 `System.Diagnostics.Tracing.EventSource.IsSupported=false`，Debug build 也一样。
  - 后果：L1 的 E1 ETW 事件（provider `FluidMotion-Motion`，GUID `8f3a2c1d-6b4e-4a91-9c7d-2e5f8a1b3c4d`）**在这类宿主里不发出**，除非宿主显式设 `<EventSourceSupport>true</EventSourceSupport>`。
  - 证据：T5 run1 在侧叉加这个开关之前 `ETW_SUMMARY distinctNames=0`（`pilot/fluenthub/z5_t5_motion_run1_etw_off.log`）；加了之后 run2 出现 5 个事件名（`z5_t5_motion_run2.log`）。T5 侧叉 csproj 就是这样设置的，仅限本地 pilot。
  - 本条是**登记**，不改 L1 公开面、fm_*、E1，也不改任何 LOCKED 文件。如果要把它写进 `docs/api_v0_frozen.md` 的「已知限制」或做宿主指引，随落盘或另票处理。

## 10. 本地副作用（清理后状态；据 workbuddy 报告，notion ai 未独立核实）

按 pidan 06:03 拍板卡「K93 清掉 FluentHub 包注册和 %TEMP% 日志，worktree 保留」：

- **包注册**：用户级包 `49462fluenthub-uwp.FluentHub_0.11.0.0_x64__tq8wa33sfftnm` 已 `Remove-AppxPackage`，`Get-AppxPackage` 查无。包 LocalState 里的 `z5_t5_motion*.log` 随包一起移除。
- **临时日志**：已删除 `%TEMP%\z5_wave1`（51 个文件）和 `%TEMP%\z5_wave2`（315 个文件）。
- **保留**：worktree `C:\Users\admin\mf_z5\*` 全部保留；FluentHub 本地侧叉 `C:\Users\admin\pilot\FluentHub` 未动（含未提交的侧叉改动和 bin/obj）。
- **证据后果**：C1 里写的「构建/运行日志在 K93 `%TEMP%`，未入库」，这些日志已按 pidan 批准删除，**库外没有副本**，库内只剩报告和截图。因此 T4/T5 的「0 警告 0 错误」「运行时 2.6.5（加载模块清单）」以及 T5 的构建日志结论，**永久只有报告文字作证**。T5 的 ETW 运行日志（run1/run2）和截图早已入库，不受影响。

## 8. 结论

- Z5 七项：T1、T2、T3、T6 **ACCEPT**；T4、T5-0、T5 **有条件 ACCEPT**（T5-0 于 02:45 下调），条件即第 3 节的降级措辞、第 4 节的偏差记录、第 5 节的后续票；第 7 节的追加提交 C1/C2 已按 pidan 06:03 许可推送（C1 `b592b6c8`、C2 `c59edddc`）；无 REJECT，无阻塞项。
- **Success≠adopt**：本结题不构成 adopt，不选 Winner，不升级为正式 L1 类库。
- **不合 main**：7 个分支都留在 `wb/z5-*`，合并须 pidan 明确口令。
- **PERF-CI HOLD**；PresentStats N/A；不采数；**不 LOCK**。
- 授权链已核实为 pidan 本人，缺口关闭（第 6 节，含 06:03 四句）。
- 已知限制 K-AOT-ETW 已登记（第 9 节）；K93 清理按 06:03 许可完成（第 10 节，据 workbuddy 报告）。
- 第 5 节的后续票不是本轮待办，须各自另行开票。
- **唯一待办**：协调按 pidan 06:03 许可，把本文件落盘为 `docs/z5_closed.md`，推到新分支 `wb/z5-closed`（自 main 2e57bff），**不合 main**。

## 11. 合入记录（Z6 F12 追加，2026-11-18 AWST）

- Z5 已通过 **PR #9** 以 merge commit 合入 main：
  - merge `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`（提交说明「Merge pull request #9 from oops11111/wb/z5-merge」）；
  - parents `2e57bff384c771cc972840ca98a1f0bb92aaeb98`（合入前的 main）+ `f71ad831a7cd86beed05a6ae23755bf5b14ece0b`（集成分支 `wb/z5-merge` 的 tip）；
  - tree `d61f0138f06b27d6f184cc63b04df2ab84ab945d`。
- 依据：pidan 2026-11-18 06:19 AWST 本人原话「先合」（逐字见 Z6 交接定稿 `z6_workbuddy_handover_FINAL.md`，sha256 `a7eff96135c6189edfffcd51a29892638897382af61dbc6a20d32fd9787541b6`）。
- 本文前文中「待落盘」「不合 main」「7 个分支都留在 `wb/z5-*`，合并须 pidan 明确口令」「唯一待办：……不合 main」等表述，写的是合入前的状态，**已被本次合入取代**。前文原样保留，一字未改。
- **Success≠adopt 仍然有效**：合入 main ≠ adopt，不选 Winner，不升级为正式 L1 类库。PERF-CI 继续 HOLD；不采数；不 LOCK。
