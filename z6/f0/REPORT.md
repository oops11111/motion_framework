# Z6 F0 · 合并后冒烟（K93N3P8，dev-only）

> 只入库证据，不改任何源文件。composition.h 冻结；L1 公开面 / fm_* / E1 未改；不采数（本报告不含任何计时或帧率结论）；不计任何闸门会话；PERF-CI HOLD；Success≠adopt（合入 main ≠ adopt，不选 Winner）；不合 main；不改任何 *_LOCKED.md。

- 依据：`z6_workbuddy_handover_FINAL.md`（sha256 `a7eff96135c6189edfffcd51a29892638897382af61dbc6a20d32fd9787541b6`，111 行）第三节 F0。
- 机台：DESKTOP-K93N3P8，用户 admin。
- 分支 `wb/z6-f0`，起点 main `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`（tree `d61f0138f06b27d6f184cc63b04df2ab84ab945d`；parents `2e57bff384c771cc972840ca98a1f0bb92aaeb98` + `f71ad831a7cd86beed05a6ae23755bf5b14ece0b`）。开工前 `git ls-remote origin refs/heads/main` = `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`。
- worktree：`%USERPROFILE%\mf_z6\f0`（由侧路克隆 `%USERPROFILE%\mf_z3a` 执行 `git worktree add`；主克隆 `%USERPROFILE%\motion_framework` 未碰）。
- 时间：2026-11-18 06:46–06:50 AWST（Australia/Perth, UTC+8）。

## 1. 做法（原样复用 Z5-T1 / Z5-T3 的命令）

脚本均在本目录，路径由脚本位置和 `%TEMP%` 推出，不含本机绝对路径。中间产物在 `%TEMP%\z6\f0\`。

| 步骤 | 脚本 | 内容 |
|---|---|---|
| composition PRE | `f0_composition.ps1 -Label PRE` | `git rev-parse HEAD:code/composition.h`、落盘原始 sha256、CRLF→LF 归一化 sha256 |
| 原生 + fm_capi_test + devsmoke 构建 | `f0_build.cmd` | VS 2022 BuildTools vcvars64；`cl ... /LD code\capi\fm_capi.cpp` → `MotionNative\fluidmotion.dll`；fm_capi_test Release（`/O2 /DNDEBUG /MT`）与 Debug（`/Od /Zi /RTC1 /D_DEBUG /MTd`），各跑一次 `fm_capi_test.exe code\capi\demo_motion.json`；`dotnet build MotionSoftDemo\z3a_l1_devsmoke\Z3aL1DevSmoke.csproj -c Debug/Release --no-incremental`，输出与 obj 都在 `%TEMP%\z6\f0\`，再复制 fluidmotion.dll |
| devsmoke 运行 | `f0_devsmoke.cmd` | 清空 `FM_FLUIDMOTION_PATH`、`MF_Z3A_L1_DEV`、`MF_Z4_L1_SESSION` 后运行 `z3a_l1_devsmoke.exe`（Debug、Release 各一次） |
| CheckSurface | `f0_surface.cmd` | 用库内 Z5-T3 工具源码 `z5/t3_results_dev/K93N3P8/SurfaceDump_Program.cs` + `SurfaceDump.csproj.txt` 在 `%TEMP%` 构建，对 **Release devsmoke 程序集** `ds_Release\z3a_l1_devsmoke.dll` 跑 |
| MotionSoftDemo（WinUI，非打包）构建 | `f0_demo.cmd` | 与 Z5-T3 `t3_dotnet_demo.cmd` 相同参数，加 `--no-incremental`；exclude_default_obj.props/.targets 用库内 Z5-T1 副本 `z3a/results_dev/K93N3P8/` |
| GUI `--z3a-l1-dev` 运行 | `f0_gui.cmd` | **未运行**，见第 3 节偏差 1 |
| composition POST、哈希、收集 | `f0_composition.ps1 -Label POST`、`f0_hashes.ps1`、`f0_collect.ps1` | 收集到 `raw/` |

## 2. 结果

| 项 | Debug | Release | 原始输出 |
|---|---|---|---|
| fluidmotion.dll 构建 | — | cl exit 0 | `raw/build.log`、`raw/cl_fluidmotion.txt` |
| fm_capi_test 构建 / 运行 | cl exit 0 / run exit 0，**`23 checks, 0 failures`** | cl exit 0 / run exit 0，**`23 checks, 0 failures`** | `raw/fm_capi_test_debug_output.txt`、`raw/fm_capi_test_release_output.txt`、`raw/cl_fm_capi_test_*.txt` |
| devsmoke（Z3aL1DevSmoke 控制台）构建 | exit 0，0 警告 0 错误 | exit 0，0 警告 0 错误 | `raw/dotnet_devsmoke_*.txt` |
| devsmoke 运行 | exit 0，**`Z3A_DEV_ONLY verdict=PASS fails=0 not_z4=1 perf_ci=HOLD`** | exit 0，**同左** | `raw/devsmoke_console_Debug.txt`、`raw/devsmoke_console_Release.txt`、`raw/devsmoke_run.log` |
| 含 Z5-T3 用例（空动画 Play + Retarget 0→120，泵开着时 Dispose） | 4 条 `ok Z5-T3 ...` + `Z5T3_DISPOSE_IDLE emitted=1 pumpOffCallbacks=1` | 同左 | 同上 |
| E1 六事件名 | `ETW_SEEN [AnimationLifecycle,FrameTick,IdleTransition,PresentStats,Retarget,TokenLoad]` | 同左 | 同上 |
| CheckSurface（Release devsmoke 程序集） | — | 20 项 ok，**`CHECKSURFACE fails=0`**，exit 0 | `raw/surface_ds_Release.checksurface.txt`、`raw/surface_ds_Release.stdout.txt`、公开面 dump `raw/surface_ds_Release.txt`、`raw/surface.log` |
| MotionSoftDemo 非打包构建 | restore 0 / build 0，14 警告 0 错误 | restore 0 / build 0，19 警告 0 错误 | `raw/demo_build.log`、`raw/demo_build_*.txt`、`raw/demo_restore_*.txt` |

两个 devsmoke 输出里 `FAIL` 开头的行数均为 0。产物 sha256 见 `raw/hashes.txt`（两份 `z4_build_info.txt` 均为 `tip=7e2feb0e…`，composition 两个哈希与下表一致）。

fm_capi_test 输出里的 `demo.enter: 69 frames (575 ms), 138 sink calls` 是测试程序自身的固定输出行，本报告不据此作任何计时结论。

**结论：F0 PASS（dev-only）**，范围限于上表所列各项；GUI `--z3a-l1-dev` 运行未做（偏差 1）。

## 3. composition.h

| | `HEAD:code/composition.h` | 归一化（CRLF→LF）sha256 | 落盘原始 sha256 | 记录 |
|---|---|---|---|---|
| PRE（06:46:04 AWST） | `fd0603c17136c067e89c71e56aef310f2f7e63f4` | `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` | `33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97` | `raw/composition_pre.txt` |
| POST（06:49:17 AWST） | `fd0603c17136c067e89c71e56aef310f2f7e63f4` | `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` | `33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97` | `raw/composition_post.txt` |

PRE = POST = 期望值。

## 4. 如实记录的偏差

1. **GUI `MotionSoftDemo.exe --z3a-l1-dev` 未运行。** 按 Z5-T1/T3 的做法要用一次性 `schtasks /Create ... /IT /RL LIMITED` 在交互会话里启动。本次执行该命令前被自动安全审查拦下，理由是「计划任务属于系统级任务注册，按硬约束应停下回报」。我没有绕开，也没有创建任何计划任务。devsmoke 的 Debug、Release 两个 verdict 来自控制台 runner `Z3aL1DevSmoke`，跑的是同一份 `Z3aL1DevSmoke.cs`、`MotionSystem.cs`、`MotionEtw.cs`；GUI 宿主只做了构建（Debug、Release 都通过）。要不要补跑 GUI，等 pidan 决定。
2. **日志里的本机路径已替换。** 按「入库文件不得含本机绝对路径」，`f0_collect.ps1` 把各文件里的用户目录路径按字节替换成字面 `%USERPROFILE%`，其他字节不动。替换前后的 sha256、大小、替换次数见 `raw/redaction_manifest.txt`；替换前的原件留在 `%TEMP%\z6\f0\`，未删除。
3. `f0_collect.ps1` 第一次执行时，函数名 `H` 和 PowerShell 内置别名冲突，报错退出（只写出了 `raw/build.log`）。改名后重跑收集。这次只是重新拷贝，没有重新构建或测试。
4. 构建输出里的系统中文本地化文字按原始字节保存（UTF-8），用其他代码页查看可能显示为乱码。
5. Debug 构建一律加了 `--no-incremental`（Release 也加了），避免 Z5-T3 遇到的「旧二进制」问题。这是对 T3 命令唯一的参数改动。

## 5. 副作用与清理方法

- 新 worktree 和本地分支：`%USERPROFILE%\mf_z6\f0`（`wb/z6-f0`）。清理方法：`git -C %USERPROFILE%\mf_z3a worktree remove %USERPROFILE%\mf_z6\f0`，然后 `git branch -D wb/z6-f0`。须先确认 commit 已经保存在别处，否则不要删。
- worktree 内未跟踪的构建产物：`MotionNative\`、`_z6f0_build_Debug\`、`_z6f0_build_Release\`、`_z6f0_obj_Debug\`、`_z6f0_obj_Release\`，未提交，删目录即可。
- `%TEMP%\z6\f0\`：中间产物、devsmoke 和 SurfaceDump 构建输出、替换前的原始日志。证据已入库，可以整目录删除；删除前确认本提交已经保存。
- NuGet 全局缓存可能多出还原包（与 Z5 相同的包）。
- 没有注册包，没有改注册表、证书或系统设置，没有建计划任务，没有提权。

## 6. 授权留痕

push 尚未获 pidan 本人许可，本分支仅本地提交。

## 7. 补记（2026-11-18 AWST，按交接定稿 r2；第二个普通提交，不改上文）

- 交接定稿已升级为 `z6_workbuddy_handover_FINAL_r2.md`（sha256 `82ab4e29db68e943ab04b034d9e3e360c4a84f2a05451ae904772e10761dd2b1`）。按 r2，F0 已做的工作不重做。
- **本票实际 base sha**：`7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`。第一次提交 `ee495368814f3227831025227f0b35b11e01ebac`，父提交即 base。
- **状态：未复核**。结果只在本地提交，没有推送。
- **r2 第四节「FrameTick 只计数，不落 deltaUs 数值」**：`raw/devsmoke_console_Debug.txt`、`raw/devsmoke_console_Release.txt` 是 devsmoke 的原样 stdout。其中 `ETW_EVENT name=FrameTick ... payload=...` 行（两份合计 474 行）带着 devsmoke 自身打印的 payload 字段，与 Z5-T3 已入库的同类输出一样。这两份文件是 r1 下产出并提交的，本报告和上文都没有读取或引用这些字段的数值，也不据此作结论。按「已做的不重做」和「证据不改写」，原样保留不删改；之后是否要对这类原始输出另作处理，由 pidan 或协调决定。
- 台账：本票动作已记入 K93 本地 `%USERPROFILE%\mf_z6\LEDGER.md`（只追加）。本次不推送，所以没有放 `z6/LEDGER_snapshot.md`。

## 授权留痕（追加，2026-11-18 AWST）

上文「push 尚未获 pidan 本人许可」写于许可之前，原样保留。之后 pidan 本人在拍板卡 **t206s992**（workbuddy cli 会话，2026-11-18 07:01 AWST）上答复，原话逐字如下：

- 「wb/z6-* 分支可以普通 push（常设许可）」
- 「Z6 范围内允许用一次性计划任务跑 GUI，用完即删」
- 「Z6 范围内松散包注册和卸载给总许可」

本分支据第一条做普通 push（`git push -u origin <分支>`），不 force、不 amend、不 rebase。合 main、开或合 PR、tag、删远端分支、LOCK、APPROVED、其他系统级步骤、非原样构建开关，都**不在**上述许可内。台账副本见 `z6/LEDGER_snapshot.md`（取自 K93 本地 `%USERPROFILE%\mf_z6\LEDGER.md`，取于本次提交时）。推送前结果状态为「未复核」。
