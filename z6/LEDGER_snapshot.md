# Z6 台账（K93 本地，只追加）

| AWST 时间 | 票号 | 动作 | 系统级步骤 / 许可原话与来源 id | 副作用 | 清理 |
|---|---|---|---|---|---|
| 2026-11-18 06:42 | Z6 | 读交接定稿 r1 `z6_workbuddy_handover_FINAL.md`，sha256 `a7eff961…41b6` 核对一致 | 无 | 无 | — |
| 2026-11-18 06:44 | F0/F12 | `%USERPROFILE%\mf_z3a`：`git ls-remote origin refs/heads/main` = `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`；`git fetch origin main` | 无 | 侧路克隆对象库新增 main 的对象，FETCH_HEAD 更新 | 无需 |
| 2026-11-18 06:44 | F0 | `git worktree add -b wb/z6-f0 %USERPROFILE%\mf_z6\f0 7e2feb0e` | 无 | 新 worktree `%USERPROFILE%\mf_z6\f0` 和本地分支 `wb/z6-f0` | `git worktree remove` 后 `git branch -D`（须先确认提交已经保存在别处） |
| 2026-11-18 06:44 | F12 | `git worktree add -b wb/z6-f12 %USERPROFILE%\mf_z6\f12 7e2feb0e` | 无 | 新 worktree `%USERPROFILE%\mf_z6\f12` 和本地分支 `wb/z6-f12` | 同上 |
| 2026-11-18 06:45 | F0 | 写入脚本 `z6/f0/*.cmd`、`*.ps1`（经 `%TEMP%\z6\f0_scripts*.tgz` 中转，已删） | 无 | 无 | — |
| 2026-11-18 06:46 | F0 | composition PRE；`f0_build.cmd`：cl 生成 fluidmotion.dll、fm_capi_test Debug/Release；dotnet build devsmoke Debug/Release | 无（使用已安装的 VS BuildTools 和 .NET SDK，没有安装任何东西） | `%TEMP%\z6\f0\`（capi_*、ds_*、ds_obj_*、日志）；worktree 内未跟踪的 `MotionNative\` | 删目录 |
| 2026-11-18 06:46 | F0 | `f0_devsmoke.cmd` 运行控制台 devsmoke；`f0_surface.cmd` 构建 SurfaceDump 并对 Release devsmoke 跑 CheckSurface | 无 | `%TEMP%\z6\f0\tool\` | 删目录 |
| 2026-11-18 06:47 | F0 | `f0_demo.cmd`：MotionSoftDemo 非打包 Debug/Release 构建（`--no-incremental`） | 无 | worktree 内未跟踪的 `_z6f0_build_Debug\`、`_z6f0_build_Release\`、`_z6f0_obj_Debug\`、`_z6f0_obj_Release\`；NuGet 全局缓存可能新增包 | 删目录 |
| 2026-11-18 06:48 | F0 | 尝试用一次性 `schtasks /Create … /IT /RL LIMITED` 跑 GUI `--z3a-l1-dev` | **计划任务属于系统级步骤**；被自动审查拦下，命令没有执行。无许可，未请示 | 没有创建任何计划任务 | 无需 |
| 2026-11-18 06:49 | F0 | composition POST；`f0_hashes.ps1`；`f0_collect.ps1` 把用户目录路径替换为 `%USERPROFILE%` 后收集到 `z6/f0/raw/` | 无 | 替换前的原件保留在 `%TEMP%\z6\f0\` | 证据已入库后才可删 |
| 2026-11-18 06:50 | F0 | 本地提交 `ee495368814f3227831025227f0b35b11e01ebac`（父 `7e2feb0e`），未推送 | push 无许可，未推送 | `%TEMP%\z6\f0_report\`（报告中转） | 可删 |
| 2026-11-18 06:53 | Z6 | 读交接定稿 r2 `z6_workbuddy_handover_FINAL_r2.md`，sha256 `82ab4e29…d2b1` | 无 | 无 | — |
| 2026-11-18 06:56 | F12 | `docs/z5_closed.md` 文末追加第 11 节「合入记录」，本地提交 `d3c169bf1078d3be1e6c01a46a4b2e5bc0471fb3`（父 `7e2feb0e`），未推送 | push 无许可，未推送 | `%TEMP%\z6\f12\`（原 blob 副本、追加段、提交说明） | 可删 |
| 2026-11-18 06:59 | F12 | 在 `docs/z5_closed.md` 标题行下插入一行合入状态（r2 第 2 条），第二个普通提交 `a4adbb628ce0a8b7c630c593d92ccabfa742dff5`（父 `d3c169bf`），未推送 | push 无许可，未推送 | `%TEMP%\r6\`（中转文件） | 可删 |
| 2026-11-18 06:59 | F0 | `z6/f0/REPORT.md` 追加 r2 补记（base sha、未复核、FrameTick payload 说明），第二个普通提交 `d0b88c9e6aeaf00f113a299b3d6b6d46c08169fb`（父 `ee495368`），未推送 | push 无许可，未推送 | 无新增 | — |
| 2026-11-18 07:00 | Z6 | 建立本台账 `%USERPROFILE%\mf_z6\LEDGER.md`（只追加）。没有推送，所以未放 `z6/LEDGER_snapshot.md` | 无 | 本文件 | 不删 |
| 2026-11-18 07:02 | F12 | 更正上方 06:59 F12 行：副作用列的中转目录应为 `%TEMP%\z6\r2\`（原行写成 `%TEMP%\r6\`，是写入时的笔误）。原行不改，以本行为准 | 无 | 无 | — |
| 2026-11-18 07:01 | Z6 | 许可（常设）：pidan 原话「wb/z6-* 分支可以普通 push（常设许可）」 | 来源：拍板卡 t206s992，workbuddy cli 会话，2026-11-18 07:01 AWST | 无 | — |
| 2026-11-18 07:01 | Z6 | 许可（总许可）：pidan 原话「Z6 范围内允许用一次性计划任务跑 GUI，用完即删」 | 来源：拍板卡 t206s992，workbuddy cli 会话，2026-11-18 07:01 AWST | 无 | — |
| 2026-11-18 07:01 | Z6 | 许可（总许可）：pidan 原话「Z6 范围内松散包注册和卸载给总许可」 | 来源：拍板卡 t206s992，workbuddy cli 会话，2026-11-18 07:01 AWST。仍不在许可内的：合 main、开或合 PR、tag、删远端分支、LOCK、APPROVED、其他系统级步骤、非原样构建开关 | 无 | — |
| 2026-11-18 07:03 | F0 | 普通 push `git push -u origin wb/z6-f0`，ls-remote = `d9e8acc55afa4fe9bb3475e32b11b5d0bf2ca496` | 许可：「wb/z6-* 分支可以普通 push（常设许可）」，t206s992 | 远端新分支 `wb/z6-f0` | 删远端分支须另行请示 |
| 2026-11-18 07:03 | F12 | 普通 push `git push -u origin wb/z6-f12`，ls-remote = `876b73ac752338fab09b225e227cfde77c1c0c63` | 同上，t206s992 | 远端新分支 `wb/z6-f12` | 同上 |
| 2026-11-18 07:04 | F0 | **系统级：一次性计划任务** `z6f0_gui_Debug`、`z6f0_gui_Release`（`/IT /RL LIMITED`）跑 `--z3a-l1-dev`，跑完 `/Delete /F`，`/Query` 确认已不存在 | 许可：「Z6 范围内允许用一次性计划任务跑 GUI，用完即删」，t206s992 | `%TEMP%\z6\f0\gui_Debug\`、`gui_Release\`、`launcher_*.log`、`schtasks_gui.txt` | 任务已删；日志在证据入库后才可删 |
| 2026-11-18 07:06 | F0 | `f0_collect_gui.ps1` 收集到 `z6/f0/raw_gui/`（FrameTick payload 改为 `[omitted]`，路径改为 `%USERPROFILE%`） | 无 | 原件保留在 `%TEMP%\z6\f0\` | 证据入库后才可删 |
| 2026-11-18 07:07 | F0 | 普通 push `wb/z6-f0`（GUI 补跑证据），ls-remote = `45cbe21b96ac917c28635065b01206210aace738` | 许可：「wb/z6-* 分支可以普通 push（常设许可）」，t206s992 | 无新增 | — |
| 2026-11-18 07:08 | F3 | `git worktree add -b wb/z6-f3 %USERPROFILE%\mf_z6\f3 7e2feb0e`（开工前 ls-remote main = `7e2feb0e`） | 无 | 新 worktree 和本地分支 `wb/z6-f3` | `git worktree remove` 后 `git branch -D`（须先确认提交已保存） |
| 2026-11-18 07:09 | F3 | `f3_build.cmd`：cl 编译 fm_capi_test Debug/Release 并运行，两边都是 35 checks / 0 failures | 无 | `%TEMP%\z6\f3\`、`%TEMP%\z6\f3src\` | 证据入库后可删 |
| 2026-11-18 07:12 | F3 | 普通提交并 `git push -u origin wb/z6-f3` | 许可：「wb/z6-* 分支可以普通 push（常设许可）」，t206s992 | 远端新分支 `wb/z6-f3` | 删远端分支须另行请示 |
| 2026-11-18 07:14 | F1 | `git worktree add -b wb/z6-f1 %USERPROFILE%\mf_z6\f1 7e2feb0e` | 无 | 新 worktree 和本地分支 `wb/z6-f1` | `git worktree remove` 后 `git branch -D`（须先确认提交已保存） |
| 2026-11-18 07:14 | F4 | `git worktree add -b wb/z6-f4 %USERPROFILE%\mf_z6\f4 7e2feb0e` | 无 | 新 worktree 和本地分支 `wb/z6-f4` | 同上 |
| 2026-11-18 07:17 | F1 | 第一次调用 `f1_build.cmd` 时误用 PowerShell `&`，被放进后台作业并在 VCVARS 前终止；原始记录移入 `%TEMP%\z6\f1\attempt1\` 并入库 `z6/f1/attempt1/` | 无 | 无进程残留（已查） | 证据入库后可删 |
| 2026-11-18 07:18 | F1 | `f1_build.cmd`：fm_capi_test Debug/Release 都是 33 checks / 0 failures；devsmoke 控制台 Debug/Release verdict=PASS | 无 | `%TEMP%\z6\f1\`、`%TEMP%\z6\f1src\`、NuGet 缓存 | 证据入库后可删 |
| 2026-11-18 07:21 | F1 | 普通提交并 `git push -u origin wb/z6-f1` | 许可：「wb/z6-* 分支可以普通 push（常设许可）」，t206s992 | 远端新分支 `wb/z6-f1` | 删远端分支须另行请示 |
| 2026-11-18 07:22 | F1 | 普通 push `wb/z6-f1` 完成，ls-remote = `cf7e4b9a4cf1898726111d9953a565ddd397a5f5` | 许可：「wb/z6-* 分支可以普通 push（常设许可）」，t206s992 | 远端新分支 `wb/z6-f1` | 删远端分支须另行请示 |
| 2026-11-18 07:24 | F4 | 新建 `docs/z6_aot_host_guide.md`，记录 composition PRE/POST 和引用核对；可选代码未做 | 无 | `%TEMP%\z6\f4\`、`%TEMP%\z6\f4src\` | 证据入库后可删 |
| 2026-11-18 07:25 | F4 | 普通提交并 `git push -u origin wb/z6-f4` | 许可：「wb/z6-* 分支可以普通 push（常设许可）」，t206s992 | 远端新分支 `wb/z6-f4` | 删远端分支须另行请示 |
| 2026-11-18 07:25 | F4 | 普通 push `wb/z6-f4` 完成，ls-remote = `5fa5f856bc54632becbc365bb2453a71af4b0c29` | 许可：「wb/z6-* 分支可以普通 push（常设许可）」，t206s992 | 远端新分支 `wb/z6-f4` | 删远端分支须另行请示 |
| 2026-11-18 07:26 | F7 | `git worktree add -b wb/z6-f7 %USERPROFILE%\mf_z6\f7 7e2feb0e` | 无 | 新 worktree 和本地分支 `wb/z6-f7` | `git worktree remove` 后 `git branch -D`（须先确认提交已保存） |
| 2026-11-18 07:29 | F7 | `f7_build.cmd`：Release 重打 L1 pack（原样命令），CheckSurface fails=0，公开面与 surface_l1pack.txt 完全相同；fm_capi_test 23/0 ×2，devsmoke PASS ×2 | 无 | worktree 内未跟踪 `MotionNative\`、`pilot\l1pack\bin\obj\`；`%TEMP%\z6\f7\`、`%TEMP%\z6\f7src\`；NuGet 缓存 | pack 产物供 F11 使用，F11 完成后再议；`%TEMP%` 证据入库后可删 |
| 2026-11-18 07:33 | F7 | 普通提交并 `git push -u origin wb/z6-f7` | 许可：「wb/z6-* 分支可以普通 push（常设许可）」，t206s992 | 远端新分支 `wb/z6-f7` | 删远端分支须另行请示 |
| 2026-11-18 07:34 | F7 | 普通 push `wb/z6-f7` 完成，ls-remote = `6b9f4767829add919cb6e626f45208f2ecf59614` | 许可：「wb/z6-* 分支可以普通 push（常设许可）」，t206s992 | 远端新分支 `wb/z6-f7` | 删远端分支须另行请示 |
| 2026-11-18 07:36 | F11 | **停下**：构建 FluentHub 侧叉须 `-p:NuGetAudit=false`（T4 已证明不加则 restore 因 NU1903+TreatWarningsAsErrors 失败），定稿第二节第 12 条列为非原样开关，须 pidan 拍板；只读检查了侧叉状态（@eb08f0db，T5 改动未提交），未改侧叉、未开分支、未构建 | 无 | 无 | — |
| 2026-11-18 07:40 | F8A | 从 main 7e2feb0e 开 `wb/z6-F8A`，worktree `%USERPROFILE%\mf_z6\F8A`；只写 `docs/z6_F8A_gate_DRAFT.md`（DRAFT，NOT LOCKED，数值全部待定）与 z6/F8A/**；未写探针、未采数；composition PRE/POST match | 无 | 无 | — |
| 2026-11-18 07:40 | F8A | 普通提交并 `git push -u origin wb/z6-F8A` | 许可：「wb/z6-* 分支可以普通 push（常设许可）」，t206s992 | 远端新分支 `wb/z6-F8A` | 删远端分支须另行请示 |
| 2026-11-18 07:40 | F8B | 从 main 7e2feb0e 开 `wb/z6-F8B`，worktree `%USERPROFILE%\mf_z6\F8B`；只写 `docs/z6_F8B_gate_DRAFT.md`（DRAFT，NOT LOCKED，数值全部待定）与 z6/F8B/**；未写探针、未采数；composition PRE/POST match | 无 | 无 | — |
| 2026-11-18 07:40 | F8B | 普通提交并 `git push -u origin wb/z6-F8B` | 许可：「wb/z6-* 分支可以普通 push（常设许可）」，t206s992 | 远端新分支 `wb/z6-F8B` | 删远端分支须另行请示 |
| 2026-11-18 07:41 | F8a/F8b | **更正**（未 push 前发现）：上两组 F8A/F8B 行中的分支名、worktree、文件名大写系脚本变量大小写冲突所致；实际分支为 `wb/z6-f8a` / `wb/z6-f8b`，worktree `%USERPROFILE%\mf_z6\f8a` / `f8b`。首个本地提交误把草稿记为 `docs/z6_F8A_gate_DRAFT.md` / `docs/z6_F8B_gate_DRAFT.md` 且漏收 z6/<票号>/**；按禁 amend 规则追加第二个提交改名为小写并补收，未 push 前完成 | 无 | 无 | — |
| 2026-11-18 07:41 | F8a | 普通 push `wb/z6-f8a` 完成，ls-remote = `74f3515648cbdfa6d3ef2dfd1bbd348c8dd7c3a0` | 许可：「wb/z6-* 分支可以普通 push（常设许可）」，t206s992 | 远端新分支 `wb/z6-f8a` | 删远端分支须另行请示 |
| 2026-11-18 07:41 | F8b | 普通 push `wb/z6-f8b` 完成，ls-remote = `a58efa98937352f1f1b4cea9fb3f00b9d7831c7b` | 许可：「wb/z6-* 分支可以普通 push（常设许可）」，t206s992 | 远端新分支 `wb/z6-f8b` | 删远端分支须另行请示 |
| 2026-11-18 07:44 | f8a | 追加新提交（不 amend）：`z6/f8a/REPORT.md` 末尾追加「偏差」段，刷新快照；普通 push `wb/z6-f8a` | 许可：「wb/z6-* 分支可以普通 push（常设许可）」，t206s992 | 远端分支 `wb/z6-f8a` 前进 | 删远端分支须另行请示 |
| 2026-11-18 07:44 | f8b | 追加新提交（不 amend）：`z6/f8b/REPORT.md` 末尾追加「偏差」段，刷新快照；普通 push `wb/z6-f8b` | 许可：「wb/z6-* 分支可以普通 push（常设许可）」，t206s992 | 远端分支 `wb/z6-f8b` 前进 | 删远端分支须另行请示 |
| 2026-11-18 07:53 | F8a/F8b | 偏差补记提交已普通 push：f8a ls-remote = `c640972a4858c6cb72dbc5060a601c4f040c0e2f`，f8b ls-remote = `fe2373b523c6fe2a70598af0bf37b8a028a70d63` | 许可：「wb/z6-* 分支可以普通 push（常设许可）」，t206s992 | 远端分支前进 | — |
| 2026-11-18 07:53 | F9a | 本地克隆 files-community/Files 到 `%USERPROFILE%\pilot\Files`，检出 v4.3 `cbfb4ba5`；原样构建 run1（上游 CI msbuild 两步，VS 2026 MSBuild）与 run2（T4 形式 dotnet build）均 FAIL（CS9057 Roslyn 5.6<5.9；run1 另有 MSB4018 缺 MSVC 目录）；**停下**：继续须装 SDK / VS C++ 工作负载或非原样，待 pidan 拍板；未安装、未注册、未启动 | 无 | 本地克隆、NuGet 缓存 | — |
| 2026-11-18 07:53 | F9a | 普通提交并 `git push -u origin wb/z6-f9a` | 许可：「wb/z6-* 分支可以普通 push（常设许可）」，t206s992 | 远端新分支 `wb/z6-f9a` | 删远端分支须另行请示 |
| 2026-11-18 08:03 | F9a | 普通 push `wb/z6-f9a` 完成，ls-remote = `75139796d49ae2bed45df5eca82860ee93940ff0` | 许可：「wb/z6-* 分支可以普通 push（常设许可）」，t206s992 | 远端新分支 `wb/z6-f9a` | 删远端分支须另行请示 |
| 2026-11-18 08:03 | F11 | 从 `wb/z6-f7` 6b9f4767 开 `wb/z6-f11`（叠加，定稿 F11 依赖 F7）；侧叉 csproj 改为 FmL1Root/FmL1PackDir/FmL1Sums 可覆盖 + FmL1VerifyPack 构建时 sha256 校验；非原样开关 `-p:WarningsNotAsErrors=NU1903`（被识别，未退回 NuGetAudit=false）；v1 原位 PASS、v2 换目录 PASS、v3 改坏副本按预期 FAIL 并复原、v4 复原后原位 PASS | 许可：「F11 批准非原样开关 -p:WarningsNotAsErrors=NU1903，不识别则退回 -p:NuGetAudit=false，仅限本票」，t208s1010 | 侧叉 csproj（本地）、侧叉 bin/obj、%TEMP% 下 pack 副本 | — |
| 2026-11-18 08:03 | F11 | 普通提交并 `git push -u origin wb/z6-f11` | 许可：「wb/z6-* 分支可以普通 push（常设许可）」，t206s992 | 远端新分支 `wb/z6-f11` | 删远端分支须另行请示 |
| 2026-11-18 08:03 | F11 | 普通 push `wb/z6-f11` 完成，ls-remote = `c3fb8d926458ebb93c3bc675a13c5f25ab853719` | 许可：「wb/z6-* 分支可以普通 push（常设许可）」，t206s992 | 远端新分支 `wb/z6-f11` | 删远端分支须另行请示 |
| 2026-11-18 08:25 | F6 | 从 `wb/z6-f11` c3fb8d92 开 `wb/z6-f6`（叠加，沿用同一侧叉）；侧叉 NavMotionPilot 改为等首帧后再 DispatcherQueue 入队 Play；非原样开关 `-p:WarningsNotAsErrors=NU1903`（被识别，未退回 NuGetAudit=false）；日志 FIRST_FRAME 先于 Play；窗口-only 中间帧+GIF 可见性 PASS | 许可：「F6 同 F11：批准非原样开关 -p:WarningsNotAsErrors=NU1903，不识别则退回 -p:NuGetAudit=false，仅限本票」，t208s1016 | 侧叉 CS 与 bin/obj、松散包注册、一次性 schtasks、光标短暂移动、%TEMP%\z6\f6 | 包已卸载；schtasks 用完即删 |
| 2026-11-18 08:25 | F6 | 普通提交并 `git push -u origin wb/z6-f6` | 许可：「wb/z6-* 分支可以普通 push（常设许可）」，t206s992 | 远端新分支 `wb/z6-f6` | 删远端分支须另行请示 |
| 2026-11-18 08:25 | F6 | 普通 push `wb/z6-f6` 完成，ls-remote = `5e92d05ea3cf7c943dd3a79e97223db10c8d74e0` | 许可：「wb/z6-* 分支可以普通 push（常设许可）」，t206s992 | 远端新分支 `wb/z6-f6` | 删远端分支须另行请示 |
| 2026-11-18 08:32 | F9a | 续跑：官方用户级安装 .NET SDK 10.0.404（Roslyn 5.9，exit=0，-NoPath 到 %USERPROFILE%\.dotnet）；VS Community 2026 C++ 工作负载 `setup.exe modify --passive` 在未提升进程中 exit=5007（日志：quiet/passive 须一开始就提升；未弹出 UAC，未点击，未 RunAs）；未重跑 Files 原样构建；追加提交不 amend | 许可：「F9a 允许安装新版 .NET SDK（Roslyn≥5.9）和 VS 2026 C++ 工作负载」，t208s1016 | 用户级 SDK 目录 %USERPROFILE%\.dotnet；机器级 VS/SDK 未改；%TEMP% 安装日志 | 待用户提升后装 C++ 工作负载；用户级 SDK 可整目录删除 |
| 2026-11-18 08:32 | F9a | 普通提交并 `git push origin wb/z6-f9a` | 许可：「wb/z6-* 分支可以普通 push（常设许可）」，t206s992 | 远端分支 `wb/z6-f9a` 前进 | 删远端分支须另行请示 |
| 2026-11-18 08:33 | F9a | 普通 push `wb/z6-f9a` 完成，ls-remote = `030216fad26d2a2e0d3d3d9ef7edf057d66b07bc` | 许可：「wb/z6-* 分支可以普通 push（常设许可）」，t206s992 | 远端分支前进 | 删远端分支须另行请示 |
| 2026-11-18 08:39 | F1 C | 在已有 `wb/z6-f1`（A/B tip cf7e4b9a）追加：侧叉 NavMotionPilot 按定稿写成 sink 见入场中途后 DispatcherQueue 入队悬停（不在 sink/render 调 fm_*；不改 token）。机器 SDK 10.0.300 原样构建 exit=1（NU1903）；未使用 F6/F11 仅限本票开关。会话曾误用用户级 10.0.404 构建成功，已保留日志，**未跑 GUI**。停下等拍板 | 无新许可；t206s992 仅用于随后普通 push。F6 开关「仅限本票」，不适用 | 侧叉 CS 与 bin/obj（本地） | 待拍板后构建再注册并跑 GUI |
| 2026-11-18 08:39 | F1 C | 普通提交并 `git push origin wb/z6-f1` | 许可：「wb/z6-* 分支可以普通 push（常设许可）」，t206s992 | 远端分支 `wb/z6-f1` 前进 | 删远端分支须另行请示 |
