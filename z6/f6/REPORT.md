# Z6 F6 REPORT（未复核）

- 票：F6（handover r2 第三节 / 表 C）：入场动画可见性。宿主侧等首帧之后再 Play；只改侧叉；须重新注册包；录屏（只录当前窗口）或至少两张中间帧截图；日志证明首帧回调先于 Play；不计时。
- 分支：`wb/z6-f6`，**叠在 `wb/z6-f11` 上**，父提交 `c3fb8d926458ebb93c3bc675a13c5f25ab853719`（F11 提交；F11 自身叠在 F7 `6b9f4767`，F7 父为 main `7e2feb0e`）。叠加原因：定稿建议顺序 F11 在 F6 之前；本票沿用同一 FluentHub 侧叉（含 T5 与 F11 的 pack 路径 / 校验），构建仍走 F11 的 `FmL1VerifyPack`。
- worktree：`%USERPROFILE%\mf_z6\f6`（由侧克隆 `%USERPROFILE%\mf_z3a` 建立）。
- 侧叉：`%USERPROFILE%\pilot\FluentHub` @ `eb08f0dbcbb43880ab34038b0cb6742f0c0deca5`（T5 / F11 改动仍未提交到该仓库；本票只改 `src/FluentHub/Views/NavMotionPilot.cs`；FluentHub 源码不入库；未推送、未 fork、未登录、无上游 PR / 评论）。
- 许可原话（逐字）：
  - pidan，卡 t208s1016，2026-11-18 08:04 AWST：「F6 同 F11：批准非原样开关 -p:WarningsNotAsErrors=NU1903，不识别则退回 -p:NuGetAudit=false，仅限本票」
  - pidan，卡 t206s992，2026-11-18 07:01 AWST：「wb/z6-* 分支可以普通 push（常设许可）」
  - pidan，卡 t206s992，2026-11-18 07:01 AWST：「Z6 范围内允许用一次性计划任务跑 GUI，用完即删」
  - pidan，卡 t206s992，2026-11-18 07:01 AWST：「Z6 范围内松散包注册和卸载给总许可」
- 上述构建开关许可**仅限 F6**，不适用于 F2 / F5 或其他票。t208s1016 中另一句「F9a 允许安装新版 .NET SDK（Roslyn≥5.9）和 VS 2026 C++ 工作负载」与 F6 无关，本票未使用。

## 结论
- **PASS（非原样（构建开关：-p:WarningsNotAsErrors=NU1903））**。
- `-p:WarningsNotAsErrors=NU1903` 被识别：两次 `dotnet build` 均把 NU1903 报为警告，exit=0，0 错误。**未退回** `-p:NuGetAudit=false`，因此没有「退回前失败日志」可留。两次命令行原文见 `raw/build1/cmd.txt` 与 `raw/build2/cmd.txt`（均为 `dotnet build src\FluentHub\FluentHub.csproj -c Debug -p:Platform=x64 -p:WarningsNotAsErrors=NU1903`）。
- 日志：所有成功启动的 GUI 次都先出现 `F6 FIRST_FRAME callback (CompositionTarget.Rendering) before Play` 与 `F6 FIRST_FRAME enqueue Play ok=True`，之后才出现 `F6 PLAY after first frame` 与 ETW `state=Played`。
- 可见性：run4 在启动前即开始 DPI 感知抓取，事后只裁 FluentHub 当前窗口。`evidence/window/enter_start.png`（窗体已出现、内容尚未画出 / 透明度近 0）、`enter_mid.png`（左侧 Inbox / My pulls / Reviews / Assigned 仍被 offsetX 切在窗口左缘外，为入场中途）、`enter_settled.png`（条目已就位）。另有只含当前窗口的 `enter_window.gif`（由同一裁剪序列合成）。未拉长 token。不计时。

## 侧叉改动（`fluenthub_f6_NavMotionPilot.patch`，相对本票开始前的 T5+F11 侧叉 CS）
1. 构造里只 `Create` 入场 handle，**不在构造 / Loaded 时 Play**；日志：`F6 enter created (Play deferred)`，然后 `F6 waiting for first CompositionTarget.Rendering before Play`。
2. 已有 `CompositionTarget.Rendering` 钩子上：首次回调只记 `F6 FIRST_FRAME ... before Play`，再 `DispatcherQueue.TryEnqueue(PlayEnterAfterFirstFrame)`（按约定表 2，不在渲染 / sink 回调里直接 Play）。
3. `PlayEnterAfterFirstFrame` 内 `enter.Play()`，日志 `F6 PLAY after first frame`。hover 的 Play / Retarget 行为与 T5 相同，未改。
4. 额外诊断（偏差，见下）：同一 Rendering 钩子在 Play 之后异步 `RenderTargetBitmap` 截 NavigationView，写入包 LocalState `z6_f6_enter_XX.png`。这些图左侧为黑、不能当中间帧证据；可见性证据来自外部窗口裁剪，不依赖这组图。
- 改法由 `f6_patch_pilot.py` 完成（先写的 `f6_patch_pilot.ps1` 传输损坏，未采用，不入库）。
- 当前工作区 `NavMotionPilot.cs` 为 LF；本票开始前备份 `NavMotionPilot.cs.pre` 为 CRLF。入库 patch 按两边 LF 归一化后的语义 diff；路径已脱敏。sha 见 `patches.sha256`（含原始 / 入库两列，以及未入库源文件 raw 与归一化 sha）。当前 CS sha256 `22b966c8d5ccc813d4ecd2b539e79bb4e64be48c2659a9657db985420e9ce436`（不入库源码本身）。

## 构建
| 步骤 | 命令原文 | 结果 |
|---|---|---|
| build1（改 CS 后首次） | `dotnet build src\FluentHub\FluentHub.csproj -c Debug -p:Platform=x64 -p:WarningsNotAsErrors=NU1903` | exit=0，2 警告（NU1903，Microsoft.DotNet.ILCompiler 10.0.8）0 错误；`FmL1VerifyPack: OK`；开关被识别，未退回 |
| build2（补上 RenderTargetBitmap 诊断后） | 同上 | exit=0，2 警告 0 错误；`FmL1VerifyPack: OK` |

- 工作目录：`%USERPROFILE%\pilot\FluentHub`。输出见 `raw/build1/`、`raw/build2/`。
- .NET SDK 仍为机上已有版本（本票未安装 SDK / VS）。runtimeconfig 中 `System.Diagnostics.Tracing.EventSource.IsSupported=true`（沿用 F11/T5）。
- 产物路径：`src\FluentHub\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\`，清单 `AppxManifest.xml`。包名 `49462fluenthub-uwp.FluentHub_tq8wa33sfftnm!App`。

## GUI 与可见性（一次性 schtasks，用完即删）
松散包：`Add-AppxPackage -Register`（用户级，无证书，无提升）。启动：

```
schtasks /Create /TN z6_f6_launch /TR "explorer.exe shell:AppsFolder\49462fluenthub-uwp.FluentHub_tq8wa33sfftnm!App" /SC ONCE /ST 00:00 /IT /RL LIMITED /F
schtasks /Run /TN z6_f6_launch
schtasks /Delete /TN z6_f6_launch /F
```

各次 `/Delete` 后查询任务，任务已不存在。光标临时移到 (8,8) 以避免 hover，结束后复原。未登录 GitHub。

| 次 | 捕捉 | 结果 |
|---|---|---|
| attempt1 | ImageGrab，未开 DPI 感知，等窗口后再抓 | 区域错误 / 近乎空白；日志顺序正确。只入库日志，不入库那些图 |
| run2 | PrintWindow + `PW_RENDERFULLCONTENT` + `SetProcessDpiAwareness(2)` | 窗口约 1440×756 真实画面，但枚举到 hwnd 已约 2s、入场 320ms 已结束，只有静止帧。日志顺序正确 |
| run3 | 侧叉内对 NavigationView 做 RenderTargetBitmap | 得到 4 张 PNG，左侧导航为黑，条目不可见，不能当中间帧。日志顺序正确（含 `F6 ENTER_SHOT`） |
| run4 | 启动**前**即以 DPI 感知抓整桌面，事后按窗口边框裁成只含 FluentHub 的帧；合成窗口-only GIF | **可用可见性证据**：start / mid / settled + `enter_window.gif`。整桌面原图含其他窗口，**不入库**。日志顺序正确 |

run4（及 run3）运动日志要点（路径已脱敏，全文见 `raw/run4/z6_f6_motion.log`）：

- `00016 F6 FIRST_FRAME callback (CompositionTarget.Rendering) before Play`
- `00017 F6 FIRST_FRAME enqueue Play ok=True`
- `00020`–`00027` 四个 `demo.enter` `state=Played` 与对应 `F6 PLAY after first frame`
- `Dispose end handles=4 (enter=4, hover=0) sinkCalls=168`
- `ETW_SUMMARY distinctNames=4 names[AnimationLifecycle=16, FrameTick=22, IdleTransition=4, TokenLoad=1] lifecycle[Cancelled=4, Created=4, Played=4, Released=4]`

FrameTick **只计数**（22），入库日志中无 `deltaUs`。sinkCalls=168 说明入场动画确实跑过，不是空转。hover=0。

## composition.h 冻结
PRE（`raw/composition_pre.txt`，2026-11-18 08:09:10 +08:00）与 POST（`raw/composition_post.txt`，2026-11-18 08:23:07 +08:00）均为 `match=True`。

- blob_HEAD:code/composition.h = `fd0603c17136c067e89c71e56aef310f2f7e63f4`
- sha256_normalized_lf = `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`
- head 仍为父提交 `c3fb8d926458ebb93c3bc675a13c5f25ab853719`（本票不改 `composition.h` / `fm_capi.*` / 冻结面）。

## 入库
- `z6/f6/REPORT.md`、`f6_composition.ps1`、`f6_patch_pilot.py`、`f6_run.py`、`run4_desktop.py`
- `z6/f6/fluenthub_f6_NavMotionPilot.patch`、`z6/f6/patches.sha256`
- `z6/f6/raw/`：build1 / build2 的 cmd、times、build.log、BUILD.txt；attempt1 / run2 / run3 / run4 的运动日志与运行日志（路径已换为 `%USERPROFILE%`）；`composition_pre.txt` / `composition_post.txt`
- `z6/f6/evidence/window/`：`enter_start.png`、`enter_mid.png`、`enter_settled.png`、`enter_window.gif`（以及同内容的 `win_0023.png` / `win_0024.png` / `win_0025.png`）
- `z6/LEDGER_snapshot.md`
- 不含 FluentHub 源码，不含整桌面截图，不含 FrameTick 时间增量。

## 偏差
- 分支叠在 F11 而非直接从 main 开出（见上；定稿顺序与侧叉依赖）。
- 构建为经批准的非原样开关（结论已标明）；开关仅限 F6。
- 侧叉多写了 RenderTargetBitmap 诊断截图（条目区为黑，不能当中间帧）。未为方便捕捉而拉长 token。
- 可见性证据来自「先抓整桌面再裁当前窗口」，不是 OS 级「只录当前窗口」的原生录屏（本机无 ffmpeg / mss / dxcam）。入库只有裁好的窗口图和窗口-only GIF。
- 中间帧有效张数：一张明确中途（`enter_mid.png`），加上 start 与 settled，以及窗口-only GIF 作为录屏替代。
- `NavMotionPilot.cs` 行尾由 CRLF 变为 LF（patch 按 LF 归一化）。
- GUI 跑了 4 次才拿到可用中间画面；失败/不足的 attempt1、run2、run3 日志保留在 `raw/`。
- 未改 Files、未开始 F9a / F9b / F1 C / F2 / F5。

## 副作用与清理
- 侧叉 `NavMotionPilot.cs` 与 bin/obj 被本机构建更新（仍仅本地）。
- 松散包曾注册；证据收齐后 `Remove-AppxPackage` 卸载（总许可）。LocalState 仍可能留有 `z6_f6_motion.log` 与 `z6_f6_enter_*.png`（用户配置目录，未入库）。
- 一次性任务 `z6_f6_launch` 每次用完即删，结束后不存在。
- 光标曾短暂移到 (8,8) 并复原。
- `%TEMP%\z6\f6\` 下留有构建日志、attempt1/run2 帧、run4 整桌面原图（含其他窗口，仅本机 TEMP，不入库）。

## 停下点
- 无。本票范围内未出现定稿未覆盖、须另拍板的新系统级步骤。未花钱。F9a 安装与续跑留到本票普通 push 完成之后。
