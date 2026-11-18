# Z6 F5 · ReducedMotion 跟随运行中的系统设置变化 — 未完成（停下：真实切换需 pidan 批准）

> **未复核**：本报告随 `wb/z6-f5` 推送，尚未经评审复核。dev-only，不采数，不计时，不计任何闸门会话；PERF-CI HOLD；Success≠adopt。
> **结论**：宿主侧代码已改为订阅 `UISettings.AnimationsEnabledChanged` 并在 UI 线程调用 `MotionSystem.SetReducedMotion(bool)`；机器 SDK 10.0.300 + 批准开关构建 exit=0；GUI 一次跑通，日志显示订阅生效、初始值已应用、入队路径已在 UI 线程执行。
> **但验收第 2 项「日志显示设置切换后 `SetReducedMotion` 的入参跟着变」未达成**：切换系统动画设置属系统级步骤（定稿第二节第 7 条），本票许可未覆盖，故按定稿第五节停下，未自行切换。

## 1. 基线与标识

- 分支 `wb/z6-f5`，worktree `%USERPROFILE%\mf_z6\f5`。
- **父提交** = `81b3f15cdf44b8207fa843bb819bf01416973316`（`wb/z6-f2` tip）。**叠在 F2 上**：F5 复用 F11 的 pack 路径与构建时 sha 校验、F6 的「首帧后再 Play」、F2 的共享日志与 RootView 重建触发，均为同一份 FluentHub 侧叉。开工前 `git ls-remote origin refs/heads/main` = `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`（未前进，无需另开）。
- 侧叉：`%USERPROFILE%\pilot\FluentHub`，仍停在 `eb08f0dbcbb43880ab34038b0cb6742f0c0deca5`；源码不入库。
- 依据：`z6_workbuddy_handover_FINAL_r2.md`，sha256 `82ab4e29db68e943ab04b034d9e3e360c4a84f2a05451ae904772e10761dd2b1`（核对一致）。
- 时间：2026-11-18 09:53–10:04 AWST（Australia/Perth, UTC+8）。

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

## 10. 停下点

- **停下**：验收第 2 项需要切换系统动画设置（系统级步骤），本票无许可。等 pidan 本人批准后续跑（见第 6 节）。
- 其余无新增须拍板项；未花钱；未碰冻结面。
