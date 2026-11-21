# Z6 F1 · A 层 + B 层：入场被悬停打断（同一 (target, offsetX) 两个持有者重叠，后写者胜）— PASS（dev-only）

> **未复核**：本报告随 `wb/z6-f1` 推送，尚未经评审复核。dev-only，不采数，不计时，不计任何闸门会话；PERF-CI HOLD；Success≠adopt。C 层（宿主由程序状态触发）按定稿排在 F6 之后，本次不做。

## 1. 基线与标识
- 分支 `wb/z6-f1`，worktree `%USERPROFILE%\mf_z6\f1`，从 main `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a` 拉出（实际 base sha 即此；未叠 F3，父提交 = 7e2feb0e）。
- 依据：`z6_workbuddy_handover_FINAL_r2.md`（sha256 `82ab4e29db68e943ab04b034d9e3e360c4a84f2a05451ae904772e10761dd2b1`）第三节 F1 验收、r1→r2 变更摘要第 3 条。
- 时间：2026-11-18 07:14–07:20 AWST（Australia/Perth, UTC+8）。

## 2. 授权留痕（逐字）
- pidan 拍板卡 **t206s992**，2026-11-18 07:01 AWST，workbuddy cli 会话：
  - 「wb/z6-* 分支可以普通 push（常设许可）」—— 本分支的普通 push 依据此条。
  - 「Z6 范围内允许用一次性计划任务跑 GUI，用完即删」—— 本票未用。
  - 「Z6 范围内松散包注册和卸载给总许可」—— 本票未用。
- 未获许可、本票未做：合 main、开/合 PR、tag、删远端分支、LOCK、APPROVED、其他系统级步骤、非原样构建开关。

## 3. 改动（`git diff --name-only 7e2feb0e..HEAD`）
| 文件 | 说明 |
|---|---|
| `code/capi/fm_capi_test.cpp` | A 层：新增一个块（39 行，10 个 CHECK），插在「reduced motion」段之前；独立的 `fm_create_manual` 系统，只用 `fm_manual_advance` 手动时钟 |
| `MotionSoftDemo/Z3aL1DevSmoke.cs` | B 层：devsmoke **测试代码**（非 L1 源），新增一个块（50 行，8 个 `Ok`），插在 PresentStats/E1 汇总之前；用 internal `MotionSystem.CreateManual()` 手动步进 |
| `z6/f1/**` | 脚本、原始输出、本报告 |
| `z6/LEDGER_snapshot.md` | 台账快照（定稿第四节要求） |

未改：`code/composition.h`、`code/capi/fm_capi.h/.cpp`、`MotionSoftDemo/MotionEtw.cs`、`MotionSoftDemo/MotionSystem.cs`（L1 源）、L1 公开面、fm_*、E1、`code/capi/demo_motion.json`、token 时长、任何 `*_LOCKED.md`、`docs/api_v0_frozen.md`。冻结文件 `git diff --name-only 7e2feb0e -- …` 为空（exit 0）。源码改动全文：`z6/f1/raw/f1_source.diff.txt`。

### 3.1 用例设计（A、B 两层相同）
1. target 11：持有者 1 = `demo.enter`（opacity 0→1 时长轨，offsetX −80→0 弹簧）；持有者 2 = 空动画（`transition = null`，模拟悬停）。
2. 播放 enter，手动推进 6 帧（每帧 1/120 s 手动时钟），断言 enter 仍在途：offsetX ∈ (−80, 0)、速度 > 0、opacity ∈ (0, 1)、帧泵开。
3. 悬停 Play，然后 `Retarget("offsetX", 40, snappy)`，**不带 from**（后写者）。断言 Retarget 起点 = enter 的当前值和速度（差 < 1e-9）：交接处值、速度连续。
4. 再推进 6 帧：每一帧 enter 的 opacity 仍在上升，同时 hover 的 offsetX 在变化（两个持有者同帧写入 = 真正重叠）。断言 6/6。
5. 跑到帧泵自停：offsetX == 40（后写者目标），opacity == 1（enter 仍完成自己持有的属性）。
- 不改 token 时长、不改 demo_motion.json；snappy = demo_motion.json `springs.snappy`（0.32, 0.78）。

## 4. 命令
- `z6/f1/f1_build.cmd`：vcvars64 → `cl` 编 `fluidmotion.dll`（/O2 /LD）→ fm_capi_test Release（`/O2 /DNDEBUG /MT`）与 Debug（`/Od /Zi /RTC1 /D_DEBUG /MTd`），与 Z5-T3/F0 同参 → 运行两者 → `dotnet build Z3aL1DevSmoke.csproj -c Debug|Release --no-incremental` → 运行控制台 devsmoke。产物都在 `%TEMP%\z6\f1\`，不写入 worktree。
- `z6/f1/f1_composition.ps1`（与 F0/F3 相同）记录 PRE/POST；`z6/f1/f1_collect.ps1` 收集原始输出。
- 原样构建，无非原样开关。

## 5. 结果（原始输出在 `z6/f1/raw/`）
| 项 | Debug | Release |
|---|---|---|
| cl 退出码（fluidmotion / fm_capi_test） | 0 | 0 / 0 |
| fm_capi_test | **33 checks, 0 failures**（原 23 + 新 10），exit 0 | **33 checks, 0 failures**，exit 0 |
| devsmoke 构建 | 0 警告 0 错误，exit 0 | 0 警告 0 错误，exit 0 |
| devsmoke 控制台 | `Z3A_DEV_ONLY verdict=PASS fails=0 not_z4=1 perf_ci=HOLD`，68 行 ok，含 `Z5T3_DISPOSE_IDLE emitted=1 pumpOffCallbacks=1` 和 8 个 F1B ok | 同左 |

### 5.1 日志摘录（两配置、两层数值一致）
```
F1A_HOLDER1 enter=1 target=11 property=offsetX in_flight value=-57.123109 vel=672.792543 opacity=0.826891887
F1A_RETARGET_FROM hover=2 target=11 property=offsetX from=-57.123109 vel=672.792543 to=40
F1A_OVERLAP frames=6 enter_opacity_advancing_and_hover_offsetX_moving=6
F1A_FINAL offsetX=40 opacity=1 expected_offsetX=40 (later writer target)
```
devsmoke（B 层）同样有 `F1B_HOLDER1 / F1B_RETARGET_FROM / F1B_OVERLAP / F1B_FINAL`。E1 的 Retarget 事件也给出了同一起点：`ETW_EVENT name=Retarget id=2 … payload=<session>,2,offsetX,-57.12310899257264,40,0.32,0.78,0,672.7925425430902`（hasFrom=0；from = enter 的在途值；速度沿用）。
- 上面这些是手动时钟下的位置和速度，不是计时数据。

### 5.2 composition.h
- PRE（07:17）和 POST（07:18）：blob `fd0603c17136c067e89c71e56aef310f2f7e63f4`，归一化 sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`，`match=True`。PRE=POST。

## 6. 偏差
1. **attempt1**：07:17 第一次调用 `f1_build.cmd` 时，我误用了 PowerShell 的 `&` 串联，命令被放进后台作业，随调用结束被终止，停在 VCVARS 之前（`build.log` 只有 BUILD_START 和 HEAD）。这**不是测试失败**，是调用方式错误。按规保留原始记录到 `z6/f1/attempt1/`（`build.log`、`vcvars64_out.txt`），然后以正确方式调用一次（07:18），即第 5 节结果。
2. **路径替换**：入库副本里本机用户目录换成 `%USERPROFILE%`；**FrameTick**：devsmoke 两份日志各 310 行含 FrameTick，其中 308 个 `payload=` 改为 `[omitted]`（另 2 行是 `ok E1 event FrameTick` 和 `ETW_SEEN`，无 payload）。原件留在 `%TEMP%\z6\f1\`；逐文件的原始和入库 sha256 见 `raw/redaction_manifest.txt`。
3. fm_capi_test 输出首行 `demo.enter: 69 frames (575 ms)` 是**原有用例**按手动时钟算出的帧数×(1000/120)，不是计时，不据此做结论。
4. devsmoke 的改动在 `MotionSoftDemo/Z3aL1DevSmoke.cs`（devsmoke 测试代码，非 L1 源，按指示列出）。WinUI 版 MotionSoftDemo 也编译这个文件，本票**没有重建 WinUI 工程，也没有跑 GUI**（未用计划任务）。
5. `z6/LEDGER_snapshot.md` 在 `z6/f1/**` 之外，按定稿第四节要求入库。

## 7. 副作用与清理
- 新 worktree `%USERPROFILE%\mf_z6\f1`、本地及远端分支 `wb/z6-f1`（删远端分支须另行请示）。
- `%TEMP%\z6\f1\`（含 attempt1\、native\、capi_*、ds_*、ds_obj_*）和 `%TEMP%\z6\f1src\`：证据已入库，可删。
- NuGet 缓存（devsmoke restore）。无安装、注册表、包注册、计划任务、系统设置改动。

## 8. 停下点
- 无。C 层留待 F6 之后（须包注册许可，已有 t206s992 总许可，届时再记）。

---

## C 层（2026-11-18 08:34–08:39 AWST，未复核）— **已停下，未跑 GUI**

- 依据：handover r2 第三节 F1 C（F6 之后）：侧叉由程序状态触发；sink 观察到入场中途后，用 `DispatcherQueue.TryEnqueue` 排队调用悬停；**不得在 sink/render 回调里直接调 fm_\***（合同 Table 2 第 2 条）；不靠人手悬停；连续 3 次没触发成功就停下报告；不得为凑重叠改 token 时长或 `demo_motion.json`。
- 分支：仍在同一支 `wb/z6-f1` 上**追加提交**（不 amend）。定稿把 A/B/C 写在同一张 F1 票下，未要求另开分支。父提交为本票 A/B tip `cf7e4b9a4cf1898726111d9953a565ddd397a5f5`（其父为 main `7e2feb0e`）。
- worktree：`%USERPROFILE%\mf_z6\f1`。
- 许可原话：
  - pidan，卡 t206s992，2026-11-18 07:01 AWST：「wb/z6-* 分支可以普通 push（常设许可）」
  - 本段未用到包注册 / schtasks（GUI 未跑）。t208s1016 的 F6/F9a 开关与安装许可**不适用于本层**（F6 开关「仅限本票」）。
- 侧叉：`%USERPROFILE%\pilot\FluentHub` @ `eb08f0dbcbb43880ab34038b0cb6742f0c0deca5`（本地已有 T5 / F11 / F6 改动，均未推送到该仓库）。本层只改 `src/FluentHub/Views/NavMotionPilot.cs`；源码不入库；未 fork、未登录、无上游 PR。未改 `demo_motion.json`、未改 token 时长。未改 A/B 源文件（`code/capi/fm_capi_test.cpp`、`MotionSoftDemo/Z3aL1DevSmoke.cs`），本层未重跑 A/B。

### 侧叉改动（`fluenthub_f1c_NavMotionPilot.patch`，相对 F6 完成后的 CS）
1. 保持 F6：等首帧后再 `DispatcherQueue.TryEnqueue` Play 入场。
2. `OnValueChanged`（sink）在 target=1 的 `offsetX` 处于 (−80, 0) 时只**观察**并 `TryEnqueue(PlayHoverAfterMidFlight)`，日志 `F1C SINK mid-flight … (enqueue hover; no fm_* in this callback)`。不在此回调里 `Create` / `Play` / `Retarget` / 其它 fm_*。
3. `PlayHoverAfterMidFlight` 在 DispatcherQueue 上走与 T5 相同的悬停路径：空动画 + Play + `Retarget(offsetX, 8, snappy)`，不带 from。日志写明 `not in sink/render`。
4. sink 继续记 `F1C_OVERLAP`（opacity 仍在 (0,1) 且 offsetX 未到 8）和 Dispose 前的 `F1C_FINAL`（后写者目标 8）。
5. 指针 Enter/Leave 在本层被忽略并记日志，保证「不靠人手悬停」。
6. 运动日志改名为 `z6_f1c_motion.log`。
- 当前 CS sha256 `c54461d39f95bd0034a15e9d17fadbc3ee34fa56dc249d07f16e9101d4fc6c79`（不入库源码）。patch sha 见 `patches_f1c.sha256`。改法由 `f1c_patch.py` 完成。未改冻结文件。

### 构建（原样命令，无额外开关）
| 步骤 | 命令原文 | 结果 |
|---|---|---|
| 第一次（会话 PATH 仍指向 F9a 装的用户级 SDK） | `dotnet build src\FluentHub\FluentHub.csproj -c Debug -p:Platform=x64` | SDK **10.0.404**，exit=0，0 警告 0 错误，`FmL1VerifyPack: OK`（08:36:16–08:38:34 AWST）。**不是**本票要采用的默认工具链，只作记录 |
| 第二次（显式机器默认） | `"%ProgramFiles%\dotnet\dotnet.exe" build src\FluentHub\FluentHub.csproj -c Debug -p:Platform=x64` | SDK **10.0.300**，**exit=1**：restore 把 NU1903（`Microsoft.DotNet.ILCompiler` 10.0.8）当作错误。日志见 `raw/c/build_sdk300/` |

- **未**使用 `-p:WarningsNotAsErrors=NU1903` 或 `-p:NuGetAudit=false`（F11/F6 的许可仅限那两票）。
- **未**用 `--no-restore` 或其他规避。
- **未**注册包、未跑 GUI、未用 schtasks。C 层「3 次触发失败再停」的运行门槛未走到。

### composition.h 与冻结面
- PRE `raw/composition_pre_c.txt`（08:34:47 +08:00）与 POST `raw/composition_post_c.txt`（08:39:10 +08:00）：`match=True`，blob `fd0603c17136c067e89c71e56aef310f2f7e63f4`，normalized `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`，head 仍为 `cf7e4b9a`。PRE=POST。
- `git diff --name-only HEAD -- code/composition.h code/capi/fm_capi.h code/capi/fm_capi.cpp MotionSoftDemo/MotionEtw.cs docs/api_v0_frozen.md` 为空。

### 结论
- C 层**代码已写在侧叉**，但 **GUI / 重叠证据未跑**。
- 机器默认 SDK 10.0.300 原样构建失败（NU1903）。继续需要任一步（对本票批准与 F6 相同的非原样开关，或批准用已安装的用户级 SDK 10.0.404 原样命令构建并跑 GUI）都须 **pidan 拍板**。本层停在这里。

### 入库（本追加提交）
- `z6/f1/REPORT.md`（本段）、`f1c_patch.py`、`fluenthub_f1c_NavMotionPilot.patch`、`patches_f1c.sha256`
- `z6/f1/raw/composition_pre_c.txt`、`composition_post_c.txt`、`raw/c/build_sdk404/**`、`raw/c/build_sdk300/**`
- `z6/LEDGER_snapshot.md`
- 不含 FluentHub 源码，无 GUI 日志 / 截图。

### 偏差
- 第一次构建误用了 F9a 装在 `%USERPROFILE%\.dotnet` 的 SDK 10.0.404（本会话此前为 F9a 前置过 PATH / `DOTNET_ROOT`）。发现后立刻用 `%ProgramFiles%\dotnet\dotnet.exe`（10.0.300）重跑并保留失败日志。
- 侧叉 bin 产物停留在 10.0.404 那次成功构建；未用其启动应用。

### 副作用与清理
- 侧叉 `NavMotionPilot.cs` 已本地修改（相对 F6）；10.0.404 构建更新了侧叉 bin/obj。
- 无包注册、无计划任务、无系统设置改动、未花钱。用户级 SDK 目录是 F9a 留下的，本层未再安装。

### 停下点
- **停在这里**，等 pidan 在下列之一上拍板后再续跑 C 层 GUI（同一侧叉，不改 token / demo_motion.json）：
  1. 批准本票使用非原样开关 `-p:WarningsNotAsErrors=NU1903`（不识别则退回 `-p:NuGetAudit=false`），用机器 SDK 10.0.300 构建；或
  2. 批准本票用已安装的官方用户级 SDK 10.0.404，原样命令 `dotnet build src\FluentHub\FluentHub.csproj -c Debug -p:Platform=x64`（无额外开关）构建并跑 GUI。

---

## C 层续跑（2026-11-18 08:50–08:53 AWST，未复核）

- 许可原话（pidan，卡 t208s1024，2026-11-18 08:49 AWST，workbuddy cli 会话）：「F1 C 批准非原样开关 -p:WarningsNotAsErrors=NU1903，不识别则退回 -p:NuGetAudit=false，用机器 SDK 10.0.300 构建并跑 GUI，仅限本票」
- 上述开关许可**仅限 F1 C**，不适用于 F2 / F5 / F9a 或其他票。
- 常设许可（pidan，卡 t206s992，2026-11-18 07:01 AWST）：「wb/z6-* 分支可以普通 push（常设许可）」；「Z6 范围内允许用一次性计划任务跑 GUI，用完即删」；「Z6 范围内松散包注册和卸载给总许可」。
- 本追加提交不 amend `c4061a2d`。侧叉 CS 相对上一笔未再改。未碰 F9a 安装、F9b、F2、F5。未花钱。

### 结论
- **PASS（非原样（构建开关：-p:WarningsNotAsErrors=NU1903））**。
- 全程使用机器默认 `%ProgramFiles%\dotnet\dotnet.exe`（SDK **10.0.300**），未把用户级 10.0.404 放进 PATH。`DOTNET_ROOT=%ProgramFiles%\dotnet`，`where dotnet` 只有机器路径。
- `-p:WarningsNotAsErrors=NU1903` **被识别**：restore 把 NU1903 报为警告，exit=0，2 警告 0 错误，`FmL1VerifyPack: OK`。**未退回** `-p:NuGetAudit=false`，因此没有「退回前失败日志」可留（上一笔已有无开关的 10.0.300 失败日志 `raw/c/build_sdk300/`）。命令原文见 `raw/c2/build_wnae/cmd.txt`：

```
"%ProgramFiles%\dotnet\dotnet.exe" build src\FluentHub\FluentHub.csproj -c Debug -p:Platform=x64 -p:WarningsNotAsErrors=NU1903
```

- GUI 第一次即触发成功（无须连续 3 次失败再停）。日志证明：sink 先见入场中途并只入队；Play/Retarget 发生在 DispatcherQueue 回调（不在 sink/render）；交接处 offsetX 连续；入场 opacity 仍在上升的同时 hover 的 offsetX 在走向 8；最终 offsetX=8（后写者目标）、opacity=1。不靠人手悬停。未改 token / `demo_motion.json`。不计时。

### GUI（一次性 schtasks，用完即删；松散包用完卸载）
- 注册：`Add-AppxPackage -Register` 用户级，清单 `…\win-x64\AppxManifest.xml`，exit=0。包名 `49462fluenthub-uwp.FluentHub_tq8wa33sfftnm!App`。
- 启动：`schtasks /Create /TN z6_f1c_launch /TR "explorer.exe shell:AppsFolder\49462fluenthub-uwp.FluentHub_tq8wa33sfftnm!App" /SC ONCE /ST 00:00 /IT /RL LIMITED /F` 然后 `/Run`，结束后 `/Delete`（随后查询 exit=1，任务已不存在）。
- 光标临时移到 (8,8) 并复原。未登录 GitHub。runtimeconfig 中 `System.Diagnostics.Tracing.EventSource.IsSupported=true`。
- 运动日志要点（全文 `raw/c2/run/z6_f1c_motion.log`，路径已脱敏）：
  - `00017`–`00018` F6 首帧回调后入队 Play 入场（仍不在 Rendering 里直接 Play）
  - `00029` `F1C SINK mid-flight target=1 property=offsetX value=-76.64838521930064 (enqueue hover; no fm_* in this callback)`
  - `00030` `F1C SINK enqueue hover ok=True`
  - `00033` ETW Retarget `anim=5 prop=offsetX from=-76.64838521930064 to=8 hasFrom=0 vel=383.9144757886635`（from 与 sink 观察值相同，交接连续；hasFrom=0）
  - `00034` `F1C PLAY hover after mid-flight enqueue (not in sink/render) … retarget(offsetX=8) ok=True mid_observed=-76.64838521930064`
  - `00035`–`00046` `F1C_OVERLAP` n=1..12：opacity 从 0.48 升到 0.997，同时 offsetX 从 −68.5 走向 4.92（两持有者同帧重叠）。共 `overlapSinkNotes=15`（只详记前 12 条）
  - `00053` `F1C_FINAL offsetX=8 opacity=1 … expected_offsetX=8 (later writer target)`
  - `Dispose end handles=5 (enter=4, hover=1) sinkCalls=160`
  - `ETW_SUMMARY … names[AnimationLifecycle=20, FrameTick=21, IdleTransition=4, Retarget=1, TokenLoad=1] lifecycle[Cancelled=5, Created=5, Played=5, Released=5]`
- FrameTick **只计数**（21），入库日志无 `deltaUs`。日志中无 pointer-enter/leave 忽略行，说明没有人手悬停事件。
- 窗口-only 截图（PrintWindow，只截 FluentHub）：`evidence/window/enter_early.png`（窗体已出现、内容近空）、`settled.png`（条目已就位）。PrintWindow 枚举到窗口时入场/悬停短动画已基本结束，8px 悬停位移在静帧上不明显；重叠与后写者胜以日志为准，不以截图计时或作为唯一证据。

### composition.h 与冻结面
- PRE `raw/composition_pre_c2.txt`（08:50:15 +08:00）与 POST `raw/composition_post_c2.txt`（08:53:01 +08:00）：`match=True`，blob `fd0603c17136c067e89c71e56aef310f2f7e63f4`，normalized `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`，head 仍为 `c4061a2d`。冻结 `git diff --name-only` 为空。

### 入库（本追加提交）
- `z6/f1/REPORT.md`（本段）、`f1c_run.py`
- `z6/f1/raw/c2/build_wnae/**`、`raw/c2/run/**`、`raw/composition_pre_c2.txt`、`composition_post_c2.txt`
- `z6/f1/evidence/window/enter_early.png`、`settled.png`
- `z6/LEDGER_snapshot.md`
- 不含 FluentHub 源码。侧叉 CS 相对上一笔无新 diff。

### 偏差
- 构建为经批准的非原样开关（结论已标明），仅限 F1 C。
- 窗口截图未能捕捉 8px 悬停中途（枚举 hwnd 偏晚），不以截图代替日志。
- 本层只跑了 FluentHub Debug GUI，未重跑 A/B 的 `fm_capi_test` / devsmoke（那些源文件本层未改）。
- F6 遗留的 RenderTargetBitmap 诊断仍会写 `z6_f6_enter_*.png` 到 LocalState（左栏仍可能为黑）；不是本层证据。

### 副作用与清理
- 松散包已注册并在取证后 `Remove-AppxPackage` 卸载。任务 `z6_f1c_launch` 用完即删。光标已复原。
- 侧叉 bin 被 10.0.300 + 本开关的构建更新（仅本地）。`%TEMP%\z6\f1c\` 本机日志。LocalState 可能仍有 `z6_f1c_motion.log` 与 F6 诊断 PNG。
- 无系统设置改动，未登录，未花钱。

### 停下点
- 无。C 层验收在本续跑完成。
