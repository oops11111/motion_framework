# Z6 F2 · RootView 重建（Dispose 后泵应关闭；Created 数等于 Released 数）— PASS（非原样（构建开关：-p:WarningsNotAsErrors=NU1903））

> **未复核**：本报告随 `wb/z6-f2` 推送，尚未经评审复核。dev-only，不采数，不计时，不计任何闸门会话；PERF-CI HOLD；Success≠adopt。本票未改 L1。

## 1. 基线与标识
- 分支 `wb/z6-f2`，worktree `%USERPROFILE%\mf_z6\f2`，从 `wb/z6-f11` `c3fb8d926458ebb93c3bc675a13c5f25ab853719` 开出（**叠在 F11 上**，与 F6 同一 FluentHub 侧叉 / F11 包路径；**未叠** `wb/z6-f6` 或 `wb/z6-f1` 的 git 侧叉）。父提交 = F11 tip。
- 侧叉磁盘状态在 F2 补丁之前已含 F6（首帧后再 Play）与 F1 C（sink 中途 DispatcherQueue 入队悬停）的本地改动；F2 补丁相对该 `pre_f2` 备份追加。
- 依据：`z6_workbuddy_handover_FINAL_r2.md`（sha256 `82ab4e29db68e943ab04b034d9e3e360c4a84f2a05451ae904772e10761dd2b1`）F2 行：RootView 重建（Unloaded→Loaded，先 Dispose 再 Create），触发只写在侧叉；ETW AnimationLifecycle Created 数等于 Released 数；Dispose 之后只断言最后一个 IdleTransition 为 (activeCount=0, pumpOn=0)，不断言 IdleTransition 条数（宿主若仍调用 fm_on_frame，泵关可重复 render(off)）；不采样进程句柄数；本票不改 L1。
- 时间：2026-11-18 09:03–09:09 AWST（Australia/Perth, UTC+8）。

## 2. 授权留痕（逐字）
- pidan 拍板卡 **t208s1031**，2026-11-18 08:59 AWST，workbuddy 会话本人原话（**仅限 F2 / 仅限本票**）：
  - 「F2 同 F11：批准非原样开关 -p:WarningsNotAsErrors=NU1903，不识别则退回 -p:NuGetAudit=false，用机器 SDK 10.0.300 构建，仅限本票」
- pidan 拍板卡 **t206s992**，2026-11-18 07:01 AWST，workbuddy cli 会话：
  - 「wb/z6-* 分支可以普通 push（常设许可）」—— 本分支的普通 push 依据此条。
  - 「Z6 范围内允许用一次性计划任务跑 GUI，用完即删」—— 本票使用一次性任务 `z6_f2_launch`，用完已删。
  - 「Z6 范围内松散包注册和卸载给总许可」—— 本票注册后已卸载。
- 上述非原样构建开关许可**仅限 F2**，不沿用 F11 / F6 / F1 C 的仅限本票批准，也不适用于其他票。
- 未获许可、本票未做：合 main、开/合 PR、tag、删远端分支、LOCK、APPROVED、force/amend/rebase/reset --hard、花钱、F9a 安装、F9b、F5。

## 3. 改动（侧叉 patch+sha 入库，源码不入库）
仓库内相对父提交 F11 仅新增 `z6/f2/**` 与更新 `z6/LEDGER_snapshot.md`。未改 `code/composition.h`、`code/capi/fm_capi.h/.cpp`、`MotionSoftDemo/MotionEtw.cs`、公开面、E1、任何 `*_LOCKED.md`、`docs/api_v0_frozen.md`。冻结 diff 为空（见 `z6/f2/raw/frozen_diff.txt`）。

侧叉（`%USERPROFILE%\pilot\FluentHub`，仍停在 `eb08f0dbcbb43880ab34038b0cb6742f0c0deca5`，源码不入库）：

| 文件 | pre_f2 sha256 | post_f2 sha256 | 说明 |
|---|---|---|---|
| `NavMotionPilot.cs` | `c54461d39f95bd0034a15e9d17fadbc3ee34fa56dc249d07f16e9101d4fc6c79` | `80a50698d07cd4c42afd5823c916ef5d6321bf1e83414a59e51288668dc73e21` | 见 patch |
| `RootView.xaml.cs` | `3b7b2ec7fe7781ed484ddb76c78278cf1464be134b3aac26e7cc8441b58fa8c5` | `364e9d65a6d6508c3417a34fe98b6a16cae7314d74ccd1f28b94ebf6ab144184` | 见 patch |
| `MainWindow.cs` | `61c7c6a1131787c72aa6e267da277646982ad53a2664c4e3cf225eb50e26fa19` | 同左（未改） | 沿用已有 `InitializeApplication(null, forceReload: true)` |

- `fluenthub_f2_NavMotionPilot.patch` sha256 `bf8748df33969a6f5a0e954d8a96e15c6b8a937f21ad2bb4a8ccc299daaf32a8`
- `fluenthub_f2_RootView.patch` sha256 `6c7d74b6b6aa888731833d2cc40fad7e5793bdeca103484fa19537523ef6de2a`
- 摘要也在 `z6/f2/patches_f2.sha256`。

侧叉行为（触发只在侧叉，不在 sink/render 调 fm_*）：
- 静态共享 `PilotLog`（进程内同一缓冲，写入 `z6_f2_motion.log`），避免第二轮 RootView 覆盖第一轮证据。
- 周期计数 `s_f2Cycle`；仅 cycle=1 在首轮 enter Play 之后用 `DispatcherQueue.TryEnqueue(RequestF2Rebuild)` 一次，调用 `MainWindow.Instance.InitializeApplication(null, forceReload: true)`。
- RootView Loaded 记「Create NavMotionPilot」后 `??= new NavMotionPilot`；Unloaded 记「Dispose then null」后 Dispose 并置空。
- F6 首帧再 Play、F1 C 程序态悬停保持不变。FrameTick 只计数，不入库 deltaUs。

## 4. 命令
必须使用机器默认 `"%ProgramFiles%\dotnet\dotnet.exe"`（10.0.300），不把用户级 10.0.404 放入 PATH。本票会话：`DOTNET_ROOT=%ProgramFiles%\dotnet`，PATH 去掉 `\.dotnet`；`where.exe dotnet` 仅 `%ProgramFiles%\dotnet\dotnet.exe`；`dotnet --version` = 10.0.300。

先试批准开关（命令行原文，已入库 `z6/f2/raw/build_wnae/cmd.txt`）：

```
"%ProgramFiles%\dotnet\dotnet.exe" build src\FluentHub\FluentHub.csproj -c Debug -p:Platform=x64 -p:WarningsNotAsErrors=NU1903
```

- 时间：2026-11-18 09:04:35–09:05:32 +08:00，exit=0，用时约 56.6 s。
- 开关**被识别**：NU1903 仅为警告（Microsoft.DotNet.ILCompiler 10.0.8），0 个错误，2 个警告。未退回 `-p:NuGetAudit=false`（无失败日志需要保留）。
- FmL1VerifyPack: OK（与 F11 校验一致）。
- 完整日志：`z6/f2/raw/build_wnae/build.log`、`env.txt`、`times.txt`。

GUI：Windows PowerShell 5.1 `Add-AppxPackage -Register` 注册松散包；一次性 `schtasks /Create /TN z6_f2_launch /TR "explorer.exe shell:AppsFolder\49462fluenthub-uwp.FluentHub_tq8wa33sfftnm!App" /SC ONCE /ST 00:00 /IT /RL LIMITED /F`，随后 `/Run`；光标移到 (8,8)；PrintWindow 只截标题为 FluentHub 的当前窗口；轮询共享日志至 cycle=2 入场结束再 CloseMainWindow；`schtasks /Delete /F`（随后 `/Query` exit=1，任务已不存在）；复制 `z6_f2_motion.log`；`Remove-AppxPackage` 卸载。脚本：`z6/f2/f2_run.py`。原始输出：`z6/f2/raw/run/`。

## 5. 结果
GUI 一次跑完（09:07:13–09:07:19 AWST）：重建触发、cycle=1 Dispose、cycle=2 PILOT / PLAY、关闭后 cycle=2 Dispose 均进入日志。

### 5.1 断言（组合日志 `z6/f2/raw/run/z6_f2_motion.log`）
- AnimationLifecycle **Created=15，Released=15，相等**（Played=15，Cancelled=15）。重叠窗口内两个 EventListener 各写一行，按组合日志原样计数。不断言 IdleTransition 条数（事件 12 条）。
- cycle=1 Dispose 窗口（L84–L109）内最后一条 IdleTransition：**L107 `activeCount=0 pumpOn=0`**（L106 同值，双监听重复）。
- cycle=2 Dispose 窗口（L137–L150）内没有新的 IdleTransition（泵在关闭前已停）。**组合日志最后一条 IdleTransition 事件为 L133 `activeCount=0 pumpOn=0`**。
- FrameTick 只出现在 ETW_SUMMARY 计数中（第一轮摘要 FrameTick=3，第二轮=22）；无 deltaUs / 帧间隔载荷。
- 未采样进程句柄数。未改 L1。

### 5.2 事件顺序（见偏差）
日志顺序：L31 `F2 REBUILD begin` → L32 新 RootView Loaded / L33 `PILOT start cycle=2` → … → L82 旧 RootView Unloaded → L84 `F2 Dispose begin cycle=1` → L109 `F2 Dispose end cycle=1`。即 **Loaded/Create 发生在 Unloaded/Dispose 之前**（`InitializeApplication(forceReload=true)` / 内容替换的实际顺序）。Unloaded 与 Dispose 均已发生，未另发明触发源，未改 MainWindow。重叠期间两条 ETW 监听同时写同一提供者，故部分事件成对出现；组合计数仍 Created=Released。

### 5.3 composition.h
PRE 2026-11-18 09:03:55 +08:00 与 POST 2026-11-18 09:09:18 +08:00：head 仍为 `c3fb8d92…`，blob `fd0603c17136c067e89c71e56aef310f2f7e63f4`，normalized `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`，**match=True**，冻结 diff 为空。

### 5.4 窗口截图（仅当前窗口）
`z6/f2/evidence/window/`：PrintWindow 共 13 张（`win_00_appear.png` … `win_12_cycle2_settled.png`）。未入库整桌面，未入库 LocalState 里侧叉自打的 `z6_f6_enter_*.png`。

## 6. 结论
**PASS（非原样（构建开关：-p:WarningsNotAsErrors=NU1903））**。开关仅限 F2。

## 7. 入库
- `z6/f2/REPORT.md`、`f2_patch.py`、`f2_run.py`、`f2_composition.ps1`
- `fluenthub_f2_NavMotionPilot.patch`、`fluenthub_f2_RootView.patch`、`patches_f2.sha256`
- `z6/f2/raw/`（构建命令与日志、GUI 日志、composition PRE/POST、冻结 diff、分析、脱敏清单）
- `z6/f2/evidence/window/`
- `z6/LEDGER_snapshot.md`
- 不含 FluentHub 源码，不含整桌面截图，不含 FrameTick 时间增量。

## 8. 偏差
- 分支叠在 F11 而非直接从 main 开出（与 F6 同一侧叉依赖；未叠 F6/F1 的 git 分支）。
- 构建为经批准的非原样开关（结论已标明）；开关仅限 F2；未使用退回开关。
- `forceReload` 实际事件顺序是新 RootView Loaded/Create 先于旧 RootView Unloaded/Dispose，与定稿表述的「Unloaded→Loaded，先 Dispose 再 Create」相反。本票不改 MainWindow（上游已有 forceReload）。可检验项（Created=Released；Dispose 后最末 IdleTransition pumpOn=0；Unloaded 已发生）均满足，故仍判 PASS，并在此留痕。
- 上述顺序导致两轮 MotionSystem / EventListener 短时并存，组合日志在重叠段出现成对 ETW 行；按组合日志计数。
- 重建在首轮 Play 之后立即入队，cycle=1 入场/悬停未跑完即被 Dispose（cycle=1 `F1C_FINAL offsetX=-20.65`）；cycle=2 跑完，`F1C_FINAL offsetX=8`。F2 不验收 F1 C 数值。
- cycle=1 一帧异步 `F6 ENTER_SHOT n=0` 在 Dispose 后失败（ArgumentException），随后 cycle=2 截图成功；那些 PNG 未入库。
- `NavMotionPilot.cs` / patch 按 LF 归一化。
- 未改 Files，未做 F9a 安装、F9b、F5。

## 9. 副作用与清理
- 侧叉 `NavMotionPilot.cs`、`RootView.xaml.cs` 与 bin/obj 被本机构建更新（仍仅本地）。MainWindow.cs 未改。
- 松散包曾注册；证据收齐后 `Remove-AppxPackage` 卸载（t206s992 总许可）。LocalState 仍可能留有 `z6_f2_motion.log` 与 `z6_f6_enter_*.png`（用户配置目录，未入库）。
- 一次性任务 `z6_f2_launch` 用完即删，结束后不存在。
- 光标曾短暂移到 (8,8) 并复原。
- `%TEMP%\z6\f2\` 下留有补丁脚本、构建与 GUI 原件（仅本机 TEMP）。

## 10. 停下点
- 无新的须另拍板的系统级步骤。未花钱。事件顺序偏差已按所用 API 如实记录，未改 MainWindow、未另写触发。F9a 安装、F9b、F5 未做。