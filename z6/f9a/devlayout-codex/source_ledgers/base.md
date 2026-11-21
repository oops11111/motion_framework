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


## Codex stage 2A append-only integration records

- 2026-11-21 15:12 AWST: imported pinned F3 5a3cef559e826a8450a252add7efd8659125a0e4; incoming exact ledger is archived at integration/tests/source_ledgers/f3-5a3cef559e826a8450a252add7efd8659125a0e4.md. Existing ticket verdicts unchanged.
- Primary reviewed this independent F0/F3/F1 scope under the current user continuous-delivery instruction; details in integration/tests/PLAN.md. No system or non-original-build authorization inferred.
- Prior F5 acceptance reference only: PR10 merged to codex/z6-integration at 15d4108fd56eb661c1a46c6552ffe74a0318ff28. That F2/F5 ancestry is not part of this stage or main.

- 2026-11-21 15:13 AWST: imported pinned F1 5ec13f459a7926012407b10cf263127e5a74ab73; incoming exact ledger is archived at integration/tests/source_ledgers/f1-5ec13f459a7926012407b10cf263127e5a74ab73.md. Native F1/F3 blocks merged without test conflict; historical GUI C evidence imported only, not newly judged.

- 2026-11-21 15:24:53 +0800: Stage 2A fresh local integration PASS(dev-only): native 45/0 Debug+Release; complete net8 devsmoke PASS both; both full surfaces equal original baseline. No GUI rerun, timing, production/schema change or new build override. Evidence: integration/tests/REPORT.md; primary review and exact-head CI required before main merge.


## Stage 2A accepted on main (append-only integration record)

PR #11 merged as 4e128e735edf788da0b9fa9e11f0d5e2511e5585; tested head 7f13ef46e31f51b59908f4457590c33aebb068bb. Head push 44880416531 and PR 44880461075 all applicable checks success. Postmerge main run 44880677014 policy/Debug/Release all success. Primary accepted deterministic native/C# integration; original reports remain unchanged.

## Stage 2B documentation integration (implementing)

Original source verdicts retained. Canonical docs and reports/raw imported exact; source ledger snapshots under z6/integration/docs/source_ledgers. F8 remains DRAFT; no approval, lock, probe, or closure issued.

- F12: ordinary source merge 876b73ac752338fab09b225e227cfde77c1c0c63; exact ledger snapshot preserved.

- F4: ordinary source merge 5fa5f856bc54632becbc365bb2453a71af4b0c29; exact ledger snapshot preserved.

- F8a: ordinary source merge c640972a4858c6cb72dbc5060a601c4f040c0e2f; exact ledger snapshot preserved.

- F8b: ordinary source merge fe2373b523c6fe2a70598af0bf37b8a028a70d63; exact ledger snapshot preserved.


## Stage 2B accepted on main (append-only record)

PR #12 head6191d9db816d56c3d8d34cbb76f4791341c4e4c9 merged as8c5cc6f31ca2be796986afd901fc1bb1098712ca. Pushdoc44881562575, PRdoc44881582615 and PRregression44881582621 all applicable checks success; postmerge main44881810494 policy/Debug/Release all success. Primary accepted exact documentation integration; original verdicts unchanged.

## Stage 2C pilot metadata provenance integration (implementing)

Exact historical evidence imported; source ledger snapshots at z6/integration/pack/source_ledgers. Local reference inspection is separate from original F7/F11/F6 acceptance, not a current-main pack rebuild or fresh host GUI/relocation/corruption run.

- F7: ordinary source merge 6b9f4767829add919cb6e626f45208f2ecf59614; exact source ledger archived.

- F11: ordinary source merge c3fb8d926458ebb93c3bc675a13c5f25ab853719; exact source ledger archived.

- F6: ordinary source merge 5e92d05ea3cf7c943dd3a79e97223db10c8d74e0; exact source ledger archived.


## Stage 2C accepted and F9a stage3A authorized (append-only)

PR13 head db208be6da7442b9105efbd767a6a5ccf8c6d710 merged122f7b4edc7698e742658532e3e1a83010f39f17; head five checks and main post44883079762 policy/Debug/Release success, primary accepted metadata/history integration and local reference inspection.

F9a original source030216fad26d2a2e0d3d3d9ef7edf057d66b07bc imported unchanged with historical FAIL/attempt1 preserved. Source/base ledgers archived z6/f9a/codex/source_ledgers. Current user authorized only VS2026 C++ workload installation; stage3A evidence pending, no new baseline or F2/F9b authorization.


## F9a stage3A accepted; stage3B original build evidence (append-only)

PR14 merged3078ae0e9f8792a03f45e09dd4fce5c6fda1ca7e; headfba233e688e396e7d26e8e9bcbcfd091ba00b376 all five checks and mainpost44887910560 three jobs success, independently accepted by primary. Final DCAT sync error and NGEN warning remain disclosed, not repaired.

Stage3B attempt20261121T090938Z-a08ae53c-run1: original Files pin cbfb4ba54a89632fbceb82ea94ada60a9bae3737, fresh no-hardlink clone, original upstream CI Restore0 and Build0, sourcePRE=POST/clean. Initial sandbox clone STOP20261121T090913Z-3a2a9200-run1 retained; it never reached build. Run2 omitted because run1 passed. Binary/manifest hashes only, no Files install/launch/package/F9b/F2 action. Current evidence z6/f9a/baseline-codex; primary review/CI pending, original historical FAIL unchanged.


## F9a3B accepted;3C1 read-only deployment evidence (append-only)

PR15 merged425165e112c271dc76706e5ff2d5354b1dfc9120 fromcecc685923824a106f2faf537fddd53dcf2795d9; five applicable head checks and mainpost44889148641 policy/Debug/Release success, accepted by primary. Original build PASS only, no launch.

3C1 scanner collected current manifest/recipe/source/SDK metadata read only; source clean/pincbfb4ba54a89632fbceb82ea94ada60a9bae3737 unchanged. Current appbin is not a verified deployment layout; Server compiled but not mapped in this recipe, AppX and intermediate PackageLayout differ. Normal VS auto merge remains unproven, no upstream bug decision or startup PASS. Evidence z6/f9a/deployment-preflight. No build/target/layout/register/launch/F9b action. Primary review/CI pending.


## F9a3C1 accepted;3C2 original full-solution Build FAIL (append-only)

PR16 merged8e68cf81ef10bd0623651949e6a98ddef4f175ca from0eb69f17282a6d75a49550fe42dea394de3a099b; head checks and mainpost44890658744 success, primary accepted read-only preflight evidence only. Startup remains not tested.

3C2 exact upstream AGENTS fullsolution attempt20261121T094221Z-e33cb21d-fullsolution at Files cbfb4ba54a89632fbceb82ea94ada60a9bae3737: Build exit1, Launcher.vcxproj145 missing repository packages/Microsoft.Windows.ImplementationLibrary.1.0.260126.7/build/native/Microsoft.Windows.ImplementationLibrary.targets. SourcePRE=POST/clean. Stopped without RestorePackagesConfig/extra restore/import repair/retry. Evidence z6/f9a/fullsolution-codex; original3B app-build PASS remains distinct, no startup or F9b conclusion. Repository evidence CI/primary review pending.


## F9a3C2 failure evidence accepted;3C3 authorized NON-ORIGINAL Build PASS

PR17 merged81543c86fc67801aa2de00f44d8b267c2ac09ee9 from1074cbea2295cae9eadd9bc4617b9e57433d7e40; mainpost44891508566 all3 success. Original fullsolution BuildFAIL remains unchanged.

User specifically approved only RestorePackagesConfig=true for F9a with NON-ORIGINAL label. Attempt20261121T103038Z-444beec5-packagesconfig: sameFilescbfb4ba54a89632fbceb82ea94ada60a9bae3737/freshclone/oneextra property, Build0, sourcePRE=POST/clean. Actual WIL1.0.260126.7 and CppWinRT3.0.260818.1 nuspec/config/import hashes verified exact, no upgrade/source repair/retry. Evidence z6/f9a/packagesconfig-codex. No deployment/startup/F2/F9b action; repository CI/primary review pending.


## F9a3C3 accepted;3C4 selected-project deployment STOP

PR18 merged92c498fbcf8ea7216d54104886a722d34d5099fb; mainpost44895678100 all3 success, primary accepted NON-ORIGINAL build only.

3C4 GateA fresh preparation Build0 with authorized RestorePackagesConfig=true; source clean. User separately approved selected FilesDev deployment/main-page startup/exact cleanup. Precheck autoload STOP retained, no deployment then; explicit existing WinPS5 Security module import fixed harness precheck. Unique actual signed VS18 /Deploy selected Files.App attempt20261121T113424Z-2ea2b1b4 exited0 but FilesDev remained absent and output reported build only. Wrapper exit1/STOP; startup/UIA/close not executed, no package created or removed, package inventory and UserChoice unchanged, source clean. No fallback/new flags/system repairs. Evidence z6/f9a/vsdeploy-codex; F9a startup NOTREADY, historical results unchanged; primary review/CI pending.
