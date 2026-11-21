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
