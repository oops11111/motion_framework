# Z6 F5 · ReducedMotion 跟随运行中的系统设置变化 — FAIL（续跑后 source=event 仍为 0）

> **续跑结论（2026-11-18 13:23–13:30 AWST）**：pidan 批准后已用 `SystemParametersInfo(SPI_SETCLIENTAREAANIMATION)` 在 GUI 运行期间做了关→开两个方向的真实切换（SPI 回读 1→0→1，`UserPreferencesMask` 0x12↔0x10），原值已恢复且复核一致。但日志里 `F5 AnimationsEnabledChanged` 与 `source=event` **仍为 0 行**，`events=0`。诊断证明切换的确实是 `UISettings.AnimationsEnabled` 背后的同一个设置（关闭后新进程读到 `False`），因此失败点是变更通知没有送达应用内的订阅者，不是切错设置。**验收第 2 项仍未达成，本票判 FAIL。** 详见第 11 节。

> **未复核**：本报告随 `wb/z6-f5` 推送，尚未经评审复核。dev-only，不采数，不计时，不计任何闸门会话；PERF-CI HOLD；Success≠adopt。
> **首跑结论（2026-11-18 10:07，已被第 11 节取代）**：宿主侧代码已改为订阅 `UISettings.AnimationsEnabledChanged` 并在 UI 线程调用 `MotionSystem.SetReducedMotion(bool)`；机器 SDK 10.0.300 + 批准开关构建 exit=0；GUI 一次跑通，日志显示订阅生效、初始值已应用、入队路径已在 UI 线程执行。
> **但验收第 2 项「日志显示设置切换后 `SetReducedMotion` 的入参跟着变」未达成**：切换系统动画设置属系统级步骤（定稿第二节第 7 条），首跑许可未覆盖，故按定稿第五节停下，未自行切换。**续跑已获批并执行，结果见第 11 节（仍为 0 行，判 FAIL）。**

## 1. 基线与标识

- 分支 `wb/z6-f5`，worktree `%USERPROFILE%\mf_z6\f5`。
- **父提交** = `81b3f15cdf44b8207fa843bb819bf01416973316`（`wb/z6-f2` tip）。**叠在 F2 上**：F5 复用 F11 的 pack 路径与构建时 sha 校验、F6 的「首帧后再 Play」、F2 的共享日志与 RootView 重建触发，均为同一份 FluentHub 侧叉。开工前 `git ls-remote origin refs/heads/main` = `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`（未前进，无需另开）。
- 侧叉：`%USERPROFILE%\pilot\FluentHub`，仍停在 `eb08f0dbcbb43880ab34038b0cb6742f0c0deca5`；源码不入库。
- 依据：`z6_workbuddy_handover_FINAL_r2.md`，sha256 `82ab4e29db68e943ab04b034d9e3e360c4a84f2a05451ae904772e10761dd2b1`（核对一致）。
- 时间：2026-11-18 09:53–10:07 AWST（Australia/Perth, UTC+8）。

## 2. 授权留痕（逐字）

- pidan 拍板卡 **t208s1037**，2026-11-18 09:39 AWST（**仅限本票**）：
  - 「F5 同 F11：批准非原样开关 -p:WarningsNotAsErrors=NU1903，不识别则退回 -p:NuGetAudit=false，用机器 SDK 10.0.300 构建，仅限本票」
- pidan 拍板卡 **t206s992**，2026-11-18 07:01 AWST（常设）：
  - 「wb/z6-* 分支可以普通 push（常设许可）」
  - 「Z6 范围内允许用一次性计划任务跑 GUI，用完即删」
  - 「Z6 范围内松散包注册和卸载给总许可」
- 上述非原样开关许可**仅限 F5**，不沿用 F11 / F6 / F2 / F1 C 的仅限本票批准，也不适用于其他票。
- 未获许可、本票未做：切换系统动画设置（系统级步骤）、合 main、开/合 PR、tag、删远端分支、LOCK、APPROVED、force/amend/rebase/reset --hard、花钱、F9a 安装、F9b。
- 未登录任何第三方账号；未 fork、未提 issue/PR/评论、未 star。

## 3. 改动

仓库内相对父提交只新增 `z6/f5/**` 并更新 `z6/LEDGER_snapshot.md`。**没有** `MotionSoftDemo/`、`code/`、`pilot/l1pack/` 下的文件（见 `z6/f5/raw/frozen_diff.txt` 与 `git diff --name-only`）。

侧叉（`%USERPROFILE%\pilot\FluentHub`，源码不入库）：

| 文件 | pre_f5 sha256 | post_f5 sha256 | 说明 |
|---|---|---|---|
| `src/FluentHub/Views/NavMotionPilot.cs` | `80a50698d07cd4c42afd5823c916ef5d6321bf1e83414a59e51288668dc73e21`（= F2 post） | `d0b111f56e2a809dedd566186e559cd1f1f2a8895e4a51667ee2344f67fec065` | 见 patch |

- `z6/f5/fluenthub_f5_NavMotionPilot.patch` sha256 `a936192a8e7bf5c8b57dd3abd706891635ba11b3711a803d9d5ec0491dfa5661`（摘要另见 `z6/f5/patches.sha256`）。
- 改动内容：
  1. 删除启动时的一次性 `new UISettings().AnimationsEnabled` 读取；
  2. 新增 `_uiSettings` 字段，订阅 `AnimationsEnabledChanged`；订阅前用 `OperatingSystem.IsWindowsVersionAtLeast(10,0,19041)` 守卫（该事件需 19041，项目最低版本 17763；只加守卫，**未抬项目最低版本、未加任何构建开关**），订阅/退订包在 `[SupportedOSPlatform("windows10.0.19041.0")]` 的小方法里；
  3. 事件回调里**只读值** `sender.AnimationsEnabled`，然后用 `DispatcherQueue.TryEnqueue` 排队调用 `ApplyReducedMotion`；**不在事件线程直接调 `fm_*`**（`MotionSystem` 线程亲和，见合同 Table 2；`MotionSystem.Affinity()` 会抛 `InvalidOperationException`）；
  4. `ApplyReducedMotion` 在 UI 线程调用已有的 `MotionSystem.SetReducedMotion(!animationsEnabled)`，并记录入参、来源、前值与线程号；
  5. `Dispose` 里退订，避免 F2 重建后旧实例残留订阅；
  6. 一个 **dev-only 自检**：构造末尾用当前值再入队一次同一路径（值不变、不改状态），仅证明 DispatcherQueue 交接可用，**不是**系统设置切换的证据。

## 4. 命令

机器 SDK：`DOTNET_ROOT=%ProgramFiles%\dotnet`，PATH 去掉 `\.dotnet`；`dotnet --version` = **10.0.300**；`where.exe dotnet` 只有 `%ProgramFiles%\dotnet\dotnet.exe`（`z6/f5/raw/build_wnae/env.txt`）。

先试批准开关（命令行原文，`z6/f5/raw/build_wnae/cmd.txt`）：

```
"%ProgramFiles%\dotnet\dotnet.exe" build src\FluentHub\FluentHub.csproj -c Debug -p:Platform=x64 -p:WarningsNotAsErrors=NU1903
```

- 开关**被识别**：NU1903（Microsoft.DotNet.ILCompiler 10.0.8）只作为警告出现，未退回 `-p:NuGetAudit=false`。
- **attempt1 失败**：`error CA1416`×2 —— `UISettings.AnimationsEnabledChanged` 需 Windows 10.0.19041.0，项目声明 17763.0。原始 `build.log` 被我重跑前误删（见偏差 1），失败内容以控制台逐字转录保存在 `z6/f5/raw/attempt1/console_capture.txt`。
- 修法（只改侧叉）：加版本守卫 + `[SupportedOSPlatform]` 包装，未动最低版本、未加开关。
- **最终构建**：2026-11-18 10:03:24–10:03:50 +08:00，`exit=0`，2 警告 / 0 错误，`FmL1VerifyPack: OK`（`FluidMotion.L1.dll` = `94d6489d…`，与 F7 `SHA256SUMS` 一致）。日志 `z6/f5/raw/build_wnae/build.log`（UTF-8，可读）。

GUI（`z6/f5/f5_run.py`，原始输出 `z6/f5/raw/run/`）：
1. `Add-AppxPackage -Register`（松散包，t206s992 总许可）；
2. 光标移到 (8,8)；
3. 一次性任务 `schtasks /Create /TN z6_f5_launch /TR "explorer.exe shell:AppsFolder\49462fluenthub-uwp.FluentHub_tq8wa33sfftnm!App" /SC ONCE /ST 00:00 /IT /RL LIMITED /F`，随后 `/Run`；
4. PrintWindow 只截标题为 `FluentHub` 的当前窗口；
5. 等 F5 行与入场 PLAY 后停 2.5 s 收尾，`CloseMainWindow`；
6. `schtasks /Delete /F`，`/Query` 返回 1（不存在）；
7. `Remove-AppxPackage` 卸载。

## 5. 结果

### 5.1 GUI 运行（2026-11-18 10:03:56–10:04:04 AWST，一次跑通）

日志 `z6/f5/raw/run/z6_f5_motion.log`（162 行）中的 F5 行：

```
00006 F5 subscribed UISettings.AnimationsEnabledChanged=True; initial AnimationsEnabled=True -> SetReducedMotion(False)
00008 F5 initial SetReducedMotion(False) applied on UI thread thread=2
00009 F5 selftest enqueue ok=True (same value, no state change)
00023 F5 SetReducedMotion(False) source=selftest animationsEnabled=True previous=True thread=2
00025 F5 SetReducedMotion(False) applied; last_animationsEnabled=True
...
00114 F5 unsubscribed AnimationsEnabledChanged events=0
```

- 订阅已装上（`AnimationsEnabledChanged=True`）；本机当前 `AnimationsEnabled=True`，因此启动值映射为 `SetReducedMotion(False)`，在 UI 线程（thread=2）执行。
- 入队路径可用：`selftest` 经 `DispatcherQueue` 在 thread=2 执行，入参 `False`、前值 `True`，无跨线程异常。
- 侧叉含 F2 行为，运行中发生一次 RootView 重建（cycle=2），两轮各自订阅并在 Dispose 退订（`events=0` ×2），未出现重复订阅残留。
- `source=event` 行 **0 条**（`z6/f5/raw/analysis.txt`）：本票**没有**切换系统设置，因此真实事件链路未验证。
- 未计时；FrameTick 只出现在 `ETW_SUMMARY` 计数里（`FrameTick=3` / `FrameTick=23`），无 `deltaUs` 载荷。

### 5.2 验收对照

| 验收项 | 结果 |
|---|---|
| `git diff --name-only` 不含 `MotionSoftDemo/`、`code/`、`pilot/l1pack/` | **满足** |
| 日志显示设置切换后 `SetReducedMotion` 入参跟着变 | **未达成**（未切换系统设置） |
| 不计时 | 满足 |
| 测完恢复原值并记录 | N/A（未切换，原值即 `AnimationsEnabled=True`） |

### 5.3 窗口截图（只截当前窗口）

`z6/f5/raw/run/frames/`：`win_00_appear.png` … `win_08_settled.png` 共 9 张，均为 PrintWindow 当前窗口，无整桌面。

### 5.4 冻结面

- `git diff --name-only HEAD -- <frozen>`：exit=0，空；`git diff --name-only 81b3f15c -- <frozen>`：exit=0，空（`z6/f5/raw/frozen_diff.txt`）。
- composition.h：PRE 2026-11-18 09:54:49 +08:00 / POST 10:04:08 +08:00，head 均为 `81b3f15c…`，blob `fd0603c17136c067e89c71e56aef310f2f7e63f4`，归一化 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`，**match=True**（`z6/f5/raw/composition_pre.txt`、`composition_post.txt`）。

## 6. 结论

**未完成（停下）**：代码与构建已就绪（dev-only，非原样开关 `-p:WarningsNotAsErrors=NU1903`），运行证据证明订阅生效、初始值映射正确、入队到 UI 线程的路径可用；但**验收要求的「真实切换系统动画设置后入参跟着变」没有做**，因为那是系统级步骤，本票没有相应许可。

**续跑所需**：pidan 本人一句「F5 批准切换系统动画设置，测完恢复原值」即可。续跑只需在同一套脚本里，GUI 运行期间切换一次系统「动画效果」开关（两个方向），观察 `F5 AnimationsEnabledChanged ... source=event` 行与 `SetReducedMotion` 入参，然后恢复原值并记录。无需改代码、无需重新构建。

（2026-11-18 13:23 已获该许可并完成续跑：切换确实生效且已恢复，但 `source=event` 仍为 0 行，结论 FAIL，见第 11 节。）

## 7. 入库

- `z6/f5/REPORT.md`、`f5_patch.py`、`f5_build.ps1`、`f5_run.py`、`f5_collect.ps1`、`f5_composition.ps1`
- `fluenthub_f5_NavMotionPilot.patch`、`patches.sha256`
- `z6/f5/raw/`：`build_wnae/`（cmd、env、times、build.log）、`attempt1/console_capture.txt`、`attempt2_console_gbk/`、`run/`（含 9 张窗口截图与两版日志）、`composition_pre/post.txt`、`frozen_diff.txt`、`analysis.txt`、`redaction_manifest.txt`
- `z6/LEDGER_snapshot.md`
- 不含 FluentHub 源码，不含整桌面截图，不含 FrameTick 时间增量；日志中本机路径已替换为 `%USERPROFILE%` / `%ProgramFiles%`。

## 8. 偏差

1. **attempt1 的原始构建日志被我删了**：首跑失败后我在重跑前删掉了整个 `%TEMP%\z6\f5\build_wnae`，原始 `build.log` 丢失，违反「首跑失败记录保留在 attempt1/」。已用 CLI 控制台逐字转录补记在 `z6/f5/raw/attempt1/console_capture.txt`，并在文件头写明是重建件、不是原始日志。这是本票最实质的证据瑕疵。
2. **首次成功构建的日志乱码**：控制台代码页 936 使 `dotnet build` 的中文输出被按 UTF-8 解码，`build.log` 中文成乱码（ASCII 部分完好，exit=0）。已保留为 `z6/f5/raw/attempt2_console_gbk/`，并在修好 `[Console]::OutputEncoding` 后重跑得到可读日志 `z6/f5/raw/build_wnae/`；为让该日志包含一次完整编译（而非增量空跑），重跑前只改过 `NavMotionPilot.cs` 的 mtime。
3. **patch 生成后又改了一次**：最初版本在文件头多加一行注释，导致 patch 的 hunk 把文件头那行带本机绝对路径的注释带进仓库（违反第二节第 10 条）。我删掉这行注释性改动（功能代码不变），重新生成 patch，因此 patch sha 与 post 文件 sha 为最终值 `a936192a…` / `d0b111f5…`；随后重跑构建与 GUI（10:03–10:04），仓库里的 patch 已无任何本机绝对路径。
4. **构建/GUI 顺序**：最终构建 10:03:24–10:03:50，GUI 10:03:56–10:04:04，即运行的是最终构建产物；更早三次构建（09:58、10:00、10:01 前）与两次运行（09:59、10:01）被覆盖重跑（同一功能代码，差异仅为注释行与 mtime，非为凑结果）。
5. **分支叠在 F2 而非 main**：F5 需要 F11 的 pack 与 F6/F2 同一份侧叉，故父提交取 `wb/z6-f2` tip `81b3f15c`。
6. **侧叉保留了 F2 的 RootView 重建触发**，所以本次日志出现 cycle=1/cycle=2 两轮。F5 断言只取 F5 行，不受影响；但两轮各订阅一次，日志里 `F5 subscribed` 出现两次属预期。
7. **`AnimationsEnabledChanged` 需要 19041**，项目最低 17763：加版本守卫后编译通过。K93 是 Windows 11 26100，守卫为真，事件可用。
8. **验收未完成**（见第 6 节），故本票不判 PASS。

## 9. 副作用与清理

- 侧叉 `NavMotionPilot.cs` 与 `bin/obj` 被本机构建更新（仍仅本地，未提交、未推送）。
- 松散包曾注册；证据收齐后 `Remove-AppxPackage` 卸载，`analysis.txt` 记录 `package still registered after run=0`。
- 一次性任务 `z6_f5_launch` 用完即删，`/Query` 返回 1（不存在）。
- 光标曾短暂移到 (8,8)，随后复原到 (565,546)。
- `%TEMP%\z6\f5\` 下留有补丁脚本、构建与 GUI 原件（仅本机 TEMP）。
- **未改**任何系统动画设置、未改注册表、未装任何 SDK/组件、未提权、未动证书。

## 10. 停下点（首跑）

- **停下**：验收第 2 项需要切换系统动画设置（系统级步骤），本票无许可。等 pidan 本人批准后续跑（见第 6 节）。
- 其余无新增须拍板项；未花钱；未碰冻结面。

## 11. 续跑：系统动画切换（2026-11-18 13:23–13:30 AWST）— FAIL

### 11.1 授权留痕（逐字）

- pidan 本人，2026-11-18 13:23 AWST，workbuddy cli 会话原话：
  - 「F5 批准切换系统动画设置，测完恢复原值」
- 沿用常设许可（t206s992）：`wb/z6-*` 普通 push；Z6 内一次性计划任务跑 GUI 用完即删；Z6 内松散包注册/卸载。
- 本次**未重新构建**，因此未使用 t208s1037 的构建开关许可；未提权、未弹 UAC、未改 HKLM。

### 11.2 PRE（2026-11-18 13:25:35–13:25:43 +08:00）

- HEAD = `8115a15155471adbd9bbc006a6cfb715b66d991a`；`git status --porcelain` 为空（仅本轮新建的证据目录除外）。
- `code/composition.h`：blob `fd0603c17136c067e89c71e56aef310f2f7e63f4`，LF 归一 sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`，`match=True`（`z6/f5/raw/run2/composition_pre.txt`、`pre_git.txt`）。
- 系统动画设置原值（`z6/f5/raw/run2/settings_pre.txt`）：

| 来源 | 原值 |
|---|---|
| `Windows.UI.ViewManagement.UISettings.AnimationsEnabled` | `True` |
| `SystemParametersInfo(SPI_GETCLIENTAREAANIMATION=0x1042)` | ok=1, value=`1` |
| `HKCU\Control Panel\Desktop\WindowMetrics` → `MinAnimate` | `1` |
| `HKCU\Control Panel\Desktop` → `UserPreferencesMask` | `9E 3E 07 80 12 00 00 00` |

- 上一轮卸载后包 `LocalState` 不存在，故本轮共享日志从空开始（不是追加）。

### 11.3 复用产物（未重建）

| 项 | 值 |
|---|---|
| `AppxManifest.xml` | 2026-11-18 10:03，未改动 |
| `src/FluentHub/Views/NavMotionPilot.cs` sha256 | `d0b111f56e2a809dedd566186e559cd1f1f2a8895e4a51667ee2344f67fec065`（= 首跑 post） |
| `FluidMotion.L1.dll` sha256 | `94d6489d9bec289732ea3e4a8540b052e576a7182bae3f3367a3e72f04e10dc2`（与 F7 `SHA256SUMS` 一致） |

未改任何代码，未重新构建。

### 11.4 流程与命令

脚本 `z6/f5/f5b_run.py`（沿用 `f5_run.py` 的注册 / 一次性任务 / PrintWindow / 清理流程，新增切换与回读），原始输出 `z6/f5/raw/run2/`：

1. `Add-AppxPackage -Register -Path <AppxManifest>`（松散包，t206s992 总许可）；
2. 光标移到 (8,8)（保存原位置）；
3. `schtasks /Create /TN z6_f5b_launch /TR "explorer.exe shell:AppsFolder\49462fluenthub-uwp.FluentHub_tq8wa33sfftnm!App" /SC ONCE /ST 00:00 /IT /RL LIMITED /F` → `/Run`（交互式、LIMITED，不提权）；
4. 等窗口出现、`F5 subscribed` 出现 2 次（F2 重建两轮）、`F6 PLAY after first frame`，再静置 3 s，记基线（127 行）；
5. 关动画：`SystemParametersInfoW(SPI_SETCLIENTAREAANIMATION=0x1043, 0, FALSE, SPIF_UPDATEINIFILE|SPIF_SENDCHANGE)`，回读 SPI 与 HKCU，等 12 s 找 `source=event`；
6. 开动画：同 API 传 TRUE，回读，等 12 s；
7. 恢复原值（=TRUE）并复核 SPI / `MinAnimate` / `UserPreferencesMask`；
8. `CloseMainWindow` → 等 `Dispose end`；`schtasks /Delete` + `/Query`（返回 1）；光标复原；复制日志；`Remove-AppxPackage`。

### 11.5 切换时间线与观测（AWST）

| 时刻 | 动作 | SPI 回读 | `UserPreferencesMask` | `MinAnimate` | 日志新增 |
|---|---|---|---|---|---|
| 13:26:37.344 | 请求关动画（基线 127 行） | — | `…12` | 1 | — |
| 13:26:38.750 | 关完成，mode=byval，ret=1 | `0` | `9E 3E 07 80 10 00 00 00` | 1 | 0 |
| 13:26:50.840 | 关后等 12.09 s | — | — | — | **0 行**（`log_after_off.txt` 为空） |
| 13:26:50.894 | 请求开动画 | — | — | — | — |
| 13:26:51.833 | 开完成，mode=byval，ret=1 | `1` | `9E 3E 07 80 12 00 00 00` | 1 | 0 |
| 13:27:03.925 | 开后等 12.09 s | — | — | — | **0 行**（`log_after_on.txt` 为空） |
| 13:27:03.979 | 恢复原值请求（original=1） | — | — | — | — |
| 13:27:06.008 | 恢复完成 | `1` | `9E 3E 07 80 12 00 00 00` | 1 | — |
| 13:27:06.986 | 关窗后 `Dispose end` | — | — | — | 关闭段 35 行 |

- 两次切换期间应用**一行日志都没有产生**（关后 12 s、开后 12 s 均为 0 行），说明应用处于空闲，不是被日志刷屏掩盖。
- `MinAnimate` 全程为 `1` 未变；变化的是 `UserPreferencesMask` 第 5 字节（`0x12`↔`0x10`，即 0x02 位），与「动画效果」开关一致。

### 11.6 日志摘录（`z6/f5/raw/run2/z6_f5b_motion.log`，162 行，F5 共 12 行）

```
00006 F5 subscribed UISettings.AnimationsEnabledChanged=True; initial AnimationsEnabled=True -> SetReducedMotion(False)
00008 F5 initial SetReducedMotion(False) applied on UI thread thread=2
00009 F5 selftest enqueue ok=True (same value, no state change)
00023 F5 SetReducedMotion(False) source=selftest animationsEnabled=True previous=True thread=2
00025 F5 SetReducedMotion(False) applied; last_animationsEnabled=True
00042 F5 subscribed UISettings.AnimationsEnabledChanged=True; initial AnimationsEnabled=True -> SetReducedMotion(False)
00045 F5 initial SetReducedMotion(False) applied on UI thread thread=2
00046 F5 selftest enqueue ok=True (same value, no state change)
00067 F5 SetReducedMotion(False) source=selftest animationsEnabled=True previous=True thread=2
00070 F5 SetReducedMotion(False) applied; last_animationsEnabled=True
00114 F5 unsubscribed AnimationsEnabledChanged events=0
00158 F5 unsubscribed AnimationsEnabledChanged events=0
```

- `F5 AnimationsEnabledChanged` 行：**0**；`source=event` 行：**0**；两个实例 Dispose 时 `events=0`。
- 关→开两个方向都没有任何回调：既没有 `SetReducedMotion(True) source=event`，也没有 `SetReducedMotion(False) source=event`。
- 订阅本身是活的：两轮各订阅一次，cycle=2 的订阅（00042）早于切换（13:26:37），且应用一直存活到 13:27:06 关窗。
- 不计时；`deltaUs` 行 0；`FrameTick` 只出现在 `ETW_SUMMARY` 计数里。

### 11.7 原值与恢复值

| 项 | 原值 | 恢复后 | 一致 |
|---|---|---|---|
| `UISettings.AnimationsEnabled` | `True` | `True` | 是 |
| `SPI_GETCLIENTAREAANIMATION` | `1` | `1` | 是 |
| `MinAnimate` | `1` | `1` | 是 |
| `UserPreferencesMask` | `9E 3E 07 80 12 00 00 00` | `9E 3E 07 80 12 00 00 00` | 是 |

`matches_original=True`（`z6/f5/raw/run2/settings_post_script.txt`、`run.log` 13:27:06 行）。

### 11.8 失败原因分析

诊断（`z6/f5/raw/run2/diag/diagnostic.txt`）：

1. 想用 Windows PowerShell 5.1 直接订阅 WinRT 事件做对照，失败：「Windows PowerShell 无法订阅 Windows RT 事件」；脚本在订阅处终止，**未执行任何 SPI 写入**，未改动设置。
2. 改为手工隔离测试（每次读取都起新进程，避开进程内缓存）：
   - 基线：SPI=`1`，`UISettings.AnimationsEnabled=True`；
   - SPI 置 0 后：回读 `0`，**新进程**读到 `UISettings.AnimationsEnabled=False`，`UserPreferencesMask=…10`；
   - SPI 置 1 后：回读 `1`，新进程读到 `True`，`UserPreferencesMask=…12`；
   - 收尾复核与原值完全一致，FluentHub 进程数 0。

结论：切换的确实是 `UISettings.AnimationsEnabled` 背后的同一个设置，**排除了「切错设置」**。已排除的其他可能：

- 订阅未装上：两轮都打出 `F5 subscribed …=True`，且 cycle=2 的订阅早于切换；
- 应用已退出 / 订阅实例已被 Dispose：窗口在，日志连续，`Dispose` 只在关窗后出现；
- 线程亲和问题：回调在 `TryEnqueue` 之前就应打印 `F5 AnimationsEnabledChanged …`，一行都没有，说明回调根本没进；
- 事件被日志量掩盖：切换窗口内日志 0 行。

因此剩下的解释集中在**变更通知未送达**：`SPI_SETCLIENTAREAANIMATION + SPIF_SENDCHANGE` 在本机（Windows 11 26100）改变了设置值，但 `UISettings.AnimationsEnabledChanged` 没有向该 WinUI 3 桌面进程投递回调。可能是该事件只随系统设置代理（设置应用的切换路径）发出，也可能是 WinUI 3 桌面进程（无 CoreWindow）下 WinRT 的这条通知链路未接通。**本票未改代码、未换 API、未重跑凑结果**，按硬约束如实判 FAIL。

### 11.9 验收对照（更新）

| 验收项 | 结果 |
|---|---|
| `git diff --name-only` 不含 `MotionSoftDemo/`、`code/`、`pilot/l1pack/` | **满足** |
| 日志显示设置切换后 `SetReducedMotion` 入参跟着变 | **未达成（FAIL）**：设置已真实切换并回读确认，`source=event` 仍 0 行 |
| 不计时 | 满足 |
| 测完恢复原值并记录 | **满足**：四项均与原值一致 |

### 11.10 清理

- `Remove-AppxPackage` 已卸载；`analysis.txt` 记录 `package still registered after run=0`；FluentHub 进程数 0。
- 一次性任务 `z6_f5b_launch` 用完即删，`/Query` 返回 1（不存在）。
- 光标曾移到 (8,8)，已复原到 (1479,0)（`run.log` 记录 saved=(1479,0)）。
- 窗口截图 5 张（`z6/f5/raw/run2/frames/`），均为 PrintWindow 当前窗口，无整桌面。
- 冻结面 diff 为空（`z6/f5/raw/run2/frozen_diff.txt`）。
- 未改代码、未重建、未提权、未动 HKLM、未装组件、未登录第三方账号。

### 11.11 本次偏差

1. **额外做了两轮 SPI 切换用于诊断**（13:28–13:29）：在 pidan 许可的「切换 + 恢复」范围内，已恢复并复核；记录在 `diag/diagnostic.txt`。
2. **`Register-ObjectEvent` 订阅 WinRT 事件失败**：PowerShell 限制，未改动设置，仅作诊断尝试记录。
3. **`schtasks_*.txt` 为空**：脚本用 `text=True` 读 `schtasks` 中文输出时抛 `UnicodeDecodeError`（控制台代码页 936），只影响被丢弃的 stdout 副本；命令本身 exit code 正常（create=0、run=0、delete=0、query=1），均在 `run.log` 里。
4. **未重新构建**：按要求复用首跑产物，故本轮不涉及构建开关。
5. **诊断的第 2 步（SPI 值 vs 新进程 UISettings）为手工逐条执行**，未逐条打时间戳，只有顺序与结果记录。
