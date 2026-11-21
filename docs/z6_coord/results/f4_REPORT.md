# Z6 F4 · AOT 宿主接入指引（文档）— PASS（dev-only，仅文档）

> **未复核**：本报告随 `wb/z6-f4` 推送，尚未经评审复核。dev-only，不采数，不计时；PERF-CI HOLD；Success≠adopt。

## 1. 基线与标识
- 分支 `wb/z6-f4`，worktree `%USERPROFILE%\mf_z6\f4`，从 main `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a` 拉出（实际 base sha 即此；父提交 = 7e2feb0e）。
- 依据：`z6_workbuddy_handover_FINAL_r2.md`（sha256 `82ab4e29db68e943ab04b034d9e3e360c4a84f2a05451ae904772e10761dd2b1`）第三节 F4、第四节「宿主开了 PublishAot」一条；已知限制 K-AOT-ETW（`docs/z5_closed.md` 第 9 节）。
- 时间：2026-11-18 07:14（worktree）、07:22–07:25 AWST（Australia/Perth, UTC+8）。

## 2. 授权留痕（逐字）
- pidan 拍板卡 **t206s992**，2026-11-18 07:01 AWST，workbuddy cli 会话：
  - 「wb/z6-* 分支可以普通 push（常设许可）」—— 本分支的普通 push 依据此条。
  - 「Z6 范围内允许用一次性计划任务跑 GUI，用完即删」—— 本票未用。
  - 「Z6 范围内松散包注册和卸载给总许可」—— 本票未用。
- 未获许可、本票未做：合 main、开/合 PR、tag、删远端分支、LOCK、APPROVED、其他系统级步骤、非原样构建开关。

## 3. 改动（`git diff --name-only 7e2feb0e..HEAD`）
| 文件 | 说明 |
|---|---|
| `docs/z6_aot_host_guide.md` | 新建。宿主侧接入指引：`<EventSourceSupport>true</EventSourceSupport>`；原因（PublishAot 使 SDK 写入 `EventSource.IsSupported=false`，Debug build 也一样）；从输出目录 `*.runtimeconfig.json` 查 `System.Diagnostics.Tracing.EventSource.IsSupported` 的自检方法（含 `dotnet msbuild -getProperty`）；通过 pubxml/PublishProfile 间接引入 PublishAot 的情况（Files）；引用 T5 run1/run2 证据（全部用相对路径） |
| `z6/f4/**` | 本报告、composition 脚本、原始输出 |
| `z6/LEDGER_snapshot.md` | 台账快照（定稿第四节要求） |

- **可选代码未做，留待 pidan 或评审决定。** 没有改 `MotionSystem.cs`，也没有改 `MotionEtw.cs`、`fm_capi.h/.cpp`、`composition.h`、公开面、E1、`docs/api_changelog.md`、任何 `*_LOCKED.md`、`docs/api_v0_frozen.md`。因此 F7/F11 不受本票影响。
- 没有构建或运行任何东西（纯文档票）。

## 4. 引用核对（原始输出 `z6/f4/raw/cited_files.txt`）
指引里引用的库内文件都在 7e2feb0e 上，逐个记录了 `git ls-files` 结果和 blob sha：`pilot/fluenthub/T5_REPORT.md`、`BASELINE.md`、`z5_t5_motion_run1_etw_off.log`、`z5_t5_motion_run2.log`、`fluenthub_t5_side_fork.patch`、`docs/z5_closed.md`。还摘录了支撑结论的原文行：run1 的 `ETW_SUMMARY distinctNames=0`、run2 的 `ETW_SUMMARY distinctNames=5 …`、patch 中 `EventSourceSupport` 行、z5_closed.md 第 9 节 K-AOT-ETW。

## 5. composition.h
- PRE 和 POST：blob `fd0603c17136c067e89c71e56aef310f2f7e63f4`，归一化 sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`，`match=True`。原始输出 `z6/f4/raw/composition_pre.txt`、`composition_post.txt`。

## 6. 偏差与未验证项
1. Files 的 pubxml/PublishProfile 情况取自 Z6 定稿 F9a 的记载，**本票没有在本地克隆或核实 Files**（F9a 未做）。指引中已注明以 F9a 基线为准。
2. 现有证据只覆盖 `dotnet build`（JIT）输出；NativeAOT 发布产物下 E1 能否被外部 ETW 会话收到**没有证据**，已在指引第 6 节写明。
3. `dotnet msbuild -getProperty` 的自检写法在本票**没有实际运行**（纯文档票，不构建），runtimeconfig 检查是主要依据。
4. `z6/LEDGER_snapshot.md` 在 `z6/f4/**` 之外，按定稿第四节要求入库。
5. 既有证据 `pilot/fluenthub/z5_t5_motion_run*.log` 末行含本机绝对路径（Z5 时入库）。按「已有证据不可改写」，本票没有改动它们，仅引用。

## 7. 副作用与清理
- 新 worktree `%USERPROFILE%\mf_z6\f4`、本地及远端分支 `wb/z6-f4`（删远端分支须另行请示）。
- `%TEMP%\z6\f4\`、`%TEMP%\z6\f4src\`：证据已入库，可删。无安装、注册表、包注册、计划任务、系统设置改动。

## 8. 停下点
- 无。可选代码等 pidan 或评审决定；若要做，须排在 F7 之前，按 T3 标准做。
