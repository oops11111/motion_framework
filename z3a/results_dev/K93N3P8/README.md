# Z3a 验收事后补证 · K93N3P8（Z5 T1）

> **事后补证，非同期记录；构建输入等价于采数 tip fe59c622；dev-only，不计 Z4 会话；不能证明 2026-11-11 06:38 AWST 前的同期状态。**

- 派工：`C:\Users\admin\mf_z5_dispatch\workbuddy_z5_wave1_dispatch.md`（sha256 `75c9fceba0987d66d70e15fc73065bea13f01bdff060c595fff3164e1af80c99`）T1
- 机台：DESKTOP-K93N3P8（Win10 19045，控制台会话 `console`，用户 admin）
- 分支 / 基线：`wb/z5-t1` @ main `2e57bff384c771cc972840ca98a1f0bb92aaeb98`；worktree `C:\Users\admin\mf_z5\t1`
- 执行时间：2026-11-18 00:41–00:45 AWST（Australia/Perth, UTC+8）
- 边界：composition.h 冻结（未改）；L1 公开面 / fm_* / E1 未改；不采数；PERF-CI HOLD；Success≠adopt；不合 main；不改任何 *_LOCKED.md。

## 1. 树等价（`tree_equivalence.txt`）

- `fe59c622:code` = `2e57bff:code` = `62005db224ae97453322940a76dfacf831b54944`
- `fe59c622:MotionSoftDemo` = `2e57bff:MotionSoftDemo` = `e7561c15868331f2b83bf1a0a2cb01e232786485`
- `fe59c622:tools` = `2e57bff:tools` = `799982f4ee1f81b7a7329da5bc449bd9eb8633e6`
- `git diff --name-status fe59c622 2e57bff`：45 条，**全部**在 `docs/`（6 条）与 `z4/`（39 条）下；docs/ 与 z4/ 之外（含根目录 Directory.Build.props、global.json、nuget.config 等）**无任何改动**（主线根目录本就没有这三个文件）。

## 2. 重编与测试（全部在 worktree 内，独立输出目录，未碰松散布局与包注册）

| 步骤 | 命令 / 位置 | 结果 |
|---|---|---|
| fluidmotion.dll | `cl /nologo /std:c++17 /EHsc /O2 /W3 /LD /I code\capi /I code /I code\explore code\capi\fm_capi.cpp /Fo:MotionNative\ /Fe:MotionNative\fluidmotion.dll`（VS 2022 BuildTools 17.14 vcvars64） | exit 0 |
| fm_capi_test | `cl ... code\capi\fm_capi_test.cpp code\capi\fm_capi.cpp`，运行 `fm_capi_test.exe code\capi\demo_motion.json` | exit 0；**`23 checks, 0 failures`**（`demo.enter: 69 frames (575 ms), 138 sink calls`） |
| MotionSoftDemo（非打包，Debug x64） | `dotnet restore/build`，OutputPath=`_z5_unpackaged_build\`，BaseIntermediateOutputPath / MSBuildProjectExtensionsPath=`_z5_unpackaged_obj\`，沿用 Z4 的 exclude_default_obj.props/.targets（字节相同，sha 见 Z4 BUILD_NOTES，副本在本目录） | restore exit 0；build exit 0（14 warnings，0 errors）；无 AppxManifest.xml / resources.pri |
| `--z3a-l1-dev` 冒烟 | `schtasks /IT /RL LIMITED` 启动 `_z5_unpackaged_build\MotionSoftDemo.exe --z3a-l1-dev`，`MF_M0_ARTDIR=%TEMP%\z5_wave1\t1\gui_logs`；跑完即删任务 | EXE_EXIT=0；**`Z3A_DEV_ONLY verdict=PASS fails=0 not_z4=1 perf_ci=HOLD`** |

E1 六事件名齐：`ETW_SEEN [AnimationLifecycle,FrameTick,IdleTransition,PresentStats,Retarget,TokenLoad]`（`z3a_l1_dev_smoke.txt` 中六行 `ok E1 event …`）。

UI 日志按 `MF_M0_ARTDIR` 落在 `%TEMP%\z5_wave1\t1\gui_logs\`（已复制到本目录）；本次**未**写入 `C:\Users\admin\motion_framework\MotionSoftDemo\`。

## 3. composition.h

| | 归一化（CRLF→LF）sha256 | 落盘原始 sha256 |
|---|---|---|
| PRE | `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` | `33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97` |
| POST | `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` | `33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97` |

PRE = POST = 期望值。

## 4. 二进制 sha256 与 Z4 记录对比（`hashes.txt`）

| 文件 | 本次（2e57bff worktree） | Z4 r1 K93 s4/s5 BUILD_NOTES（fe59c622，2026-11-11） | 结论 |
|---|---|---|---|
| fluidmotion.dll | `b0c7f91163eb0d41b32f015c9064bbc74e38248557009be44bd525fa10288f6d`（438784） | `0efac99e8326bfa705a35518ff148ba5a568ff2acfafddce7199e2e9de8e0dea`（438784） | 不一致；长度相同。记为工具链差异（MSVC 链接时间戳等非确定性），**不算失败** |
| MotionSoftDemo.exe | `f3c796c6c1d229de4076fd46a44a81f71f71c8804cece6a618c95b53335b6194`（163840） | `50b00fef…fd82`（163840） | 不一致；记为工具链/构建路径差异 |
| MotionSoftDemo.dll | `3ac3bba7314090274d81b77c750440fabd2d5e31c4721c80576e33b920a9432b`（1314816） | `03ffd553…4c48`（1315328） | 不一致（长度差 512 字节）；构建目录不同（`mf_z3a` vs `mf_z5\t1`，路径嵌入 PDB/调试信息）与 tip 戳不同所致，记为工具链/路径差异 |
| fm_capi_test.exe | `036eff8ea1642da20c831c1a7d3dabfd99071b0d8552880c8f4afa858291e2dc`（581632） | 未记录 | — |
| z4_build_info.txt | `d226f469…a27f`：`tip=2e57bff…`，composition 两哈希同上 | `c7169179…f3d7`：`tip=fe59c622…` | tip 行按构建时 HEAD 写入，预期不同；composition 两哈希一致 |

## 5. 如实记录的偏差

- 第一次原生构建（00:41:22–00:41:34 AWST）的 `native_build.log` 因 cmd 的 `echo X=0>>file` 被解释成句柄重定向，各 exit code 行为空；产物与输出正常（`23 checks, 0 failures`）。修正日志写法后于 00:42:04–00:42:16 AWST 完整重跑一次（同样 23/0），本目录以重跑为准；第一次的原始日志原样保留在 `attempt1_logging_bug/`。这不是为凑结果的重跑：两次结果相同，且没有改任何代码。
- cl / dotnet 输出含系统中文本地化文字，按原始字节保存，可能显示为乱码。
- `MotionNative\`、`_z5_unpackaged_build\`、`_z5_unpackaged_obj\` 为本地构建产物，未提交。

## 6. 文件清单

`tree_equivalence.txt` · `hashes.txt` · `native_build.log` · `vcvars64_out.txt` · `cl_fluidmotion.txt` · `cl_fm_capi_test.txt` · `fm_capi_test_output.txt` · `dotnet_build.log` · `dotnet_restore_out.txt` · `dotnet_build_out.txt` · `launcher.log` · `z3a_l1_dev_smoke.txt` · `z3a_l1_dev_ui.txt` · `composition_pre.txt` · `composition_post.txt` · `t1_build.cmd` · `t1_dotnet.cmd` · `t1_run.cmd` · `exclude_default_obj.props` · `exclude_default_obj.targets` · `attempt1_logging_bug/`
