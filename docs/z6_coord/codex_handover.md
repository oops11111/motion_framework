# Z6 交接提示词（交给 Codex CLI）

- 日期：2026-11-21（Australia/Perth，UTC+8，下文时间均为此区）
- 发自：协调 bot「动效框架」
- 收件：codex cli（bot id `76d762d9-ef0a-49bd-967d-6f046a24cec1`）
- 执行机：pidan 的 Windows PC `DESKTOP-K93N3P8`（machineId `3951c05a-6a80-446d-898c-2ed8af24baa8`），只走命令行
- 依据：workbuddy 已推送的 13 张票 REPORT / TIPS / LEDGER、交接定稿 `z6_workbuddy_handover_FINAL_r2.md`（sha256 `82ab4e29db68e943ab04b034d9e3e360c4a84f2a05451ae904772e10761dd2b1`）、Gemini cli 结果复核、Gemini 网页版 F5 修法。材料见第 9 节。
- 本文是 Codex 接手 Z6 开发的自包含提示词。仓库里已有 `*_LOCKED.md`、`docs/api_v0_frozen.md`、`docs/z5_closed.md` 的约束继续有效；冲突时以更严的一方为准。

你从 workbuddy 接手「Windows 桌面端动效框架」Z6 的后续开发。是否向协调 bot 汇报、或直接向 pidan 汇报，由你自己决定（与 pidan 此前给 workbuddy 的规则相同）。需要评审或对照时，可以找协调 bot 组织。notion ai 裁决评审分歧。

---

## 1. 仓库与取码

- 仓库：https://github.com/oops11111/motion_framework
- `main` = `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`（tree `d61f0138f06b27d6f184cc63b04df2ab84ab945d`；2026-11-18 06:23:57 +0800，Merge PR #9，parents `2e57bff3` + `f71ad831`）。Z5 已合入 main。合入 ≠ adopt，不选 Winner。

**在 K93 上取码（必须遵守）：**

- 在**新的兄弟目录**里 clone / 建 worktree，例如 `C:\Users\admin\mf_codex`（其下再 `worktree add` 各票目录）。
- **禁止**在 `C:\Users\admin\motion_framework` 里操作：它不是 git 仓库。
- **禁止**打扰 workbuddy 已有 worktree（`C:\Users\admin\mf_z6\…`、侧克隆 `C:\Users\admin\mf_z3a` 等）以及 `C:\Users\admin\mf_z6_dispatch`。
- 第三方仓（FluentHub、Files）只在本地另目录克隆。不 fork、不推送、不提 issue/PR/评论、不 star、不登录任何第三方账号。

开工前先 `git ls-remote origin refs/heads/main refs/heads/wb/z6-*`，核对第 3、4 节的 tip；若 tip 已变，停下报告，不要假定下文仍准。

---

## 2. 当前状态（2026-11-21 自核）

- Z5 已由 PR #9 合入 main `7e2feb0e`。Z6 的 13 个分支 `wb/z6-{f0,f1,f2,f3,f4,f5,f6,f7,f8a,f8b,f9a,f11,f12}` 均于 2026-11-18 推送，**全部未合 main**（ahead≥1、behind=0）。2026-11-21 再次 `git ls-remote`，tip 与 2026-11-20 20:35:12 AWST 的 `TIPS.md` 一致。
- 成绩是 workbuddy 自报、且标明「未复核」。Gemini cli 已做对抗复核；notion ai 的结果复核稿 `z6_result_review_notion.md` 和结题草稿 `z6_closed_DRAFT_notion.md` **尚未落盘**。在裁决出来之前，不要改判已推送的票，也不要写 `docs/z6_closed.md`。
- 开发环境验证 ≠ 计时数据。报告里不得出现计时判据或帧率结论。`PresentStats` 是占位值，不得读其字段。`FrameTick` 只计数，不落 `deltaUs` 数值。

---

## 3. 真实分支拓扑（勿用 TIPS.md 的叠放句）

`results/TIPS.md` 写「Stacking: f7 -> f11 -> f6 -> f2 -> f5 (each built on previous)」。这是错的。2026-11-21 在只读 bare clone 上用 `git rev-parse` / `git merge-base --is-ancestor` 自核，与 Gemini cli 复核一致：

```
main 7e2feb0e
├─ wb/z6-f0   45cbe21b  (ahead 4，直接从 main 拉出)
├─ wb/z6-f12  876b73ac  (ahead 3)
├─ wb/z6-f3   5a3cef55  (ahead 1)
├─ wb/z6-f4   5fa5f856  (ahead 1)
├─ wb/z6-f8a  c640972a  (ahead 3)
├─ wb/z6-f8b  fe2373b5  (ahead 3)
├─ wb/z6-f9a  030216fa  (ahead 2)
├─ wb/z6-f1   5ec13f45  (ahead 3，从 main 拉出，不叠 F6)
└─ wb/z6-f7   6b9f4767  (ahead 1)
    └─ wb/z6-f11  c3fb8d92  (ahead 2，父=f7)
         ├─ wb/z6-f6   5e92d05e  (ahead 3，父=f11)
         └─ wb/z6-f2   81b3f15c  (ahead 3，父=f11，与 f6 为兄弟)
              └─ wb/z6-f5   e4cd696c  (ahead 6；父链 8115a151 ← 942ee80e ← 81b3f15c)
```

要点：`f2` 的父是 `f11` tip `c3fb8d92`，**不是** `f6`。`f5` 祖先含 `f2` / `f11` / `f7`，**不含** `f6`。以后若合 main，不可按 TIPS 的线性顺序把 `f6` 当成 `f2`/`f5` 的父，否则会漂移。`f1` 也不叠在 `f6` 上。

全部 Z6 tip 上 `code/composition.h` blob 应为 `fd0603c17136c067e89c71e56aef310f2f7e63f4`（与 main 相同）。每张票 PRE/POST 都要记。

---

## 4. 十三张票（tip 与自报结论）

| 票 | tip（完整 sha） | 自报结论（均标未复核） |
|---|---|---|
| F0 | `45cbe21b96ac917c28635065b01206210aace738` | PASS（dev-only），含 GUI `--z3a-l1-dev` Debug/Release |
| F12 | `876b73ac752338fab09b225e227cfde77c1c0c63` | 已追加合入记录，并在标题下插入「已 PR #9 合入 main `7e2feb0e`」一行。报告未用 PASS 标题；文中早期「未推送」字样未删，但分支已推 |
| F3 | `5a3cef559e826a8450a252add7efd8659125a0e4` | PASS（dev-only） |
| F1 | `5ec13f459a7926012407b10cf263127e5a74ab73` | A/B 层 PASS（dev-only）；C 层续跑后 PASS（非原样：`-p:WarningsNotAsErrors=NU1903`） |
| F4 | `5fa5f856bc54632becbc365bb2453a71af4b0c29` | PASS（dev-only，仅文档 `docs/z6_aot_host_guide.md`）。可选 `#if DEBUG` 代码未做 |
| F7 | `6b9f4767829add919cb6e626f45208f2ecf59614` | PASS（dev-only）。L1 pack 已按 main 重打 |
| F11 | `c3fb8d926458ebb93c3bc675a13c5f25ab853719` | PASS（非原样：`-p:WarningsNotAsErrors=NU1903`） |
| F6 | `5e92d05ea3cf7c943dd3a79e97223db10c8d74e0` | PASS（非原样：`-p:WarningsNotAsErrors=NU1903`） |
| F2 | `81b3f15cdf44b8207fa843bb819bf01416973316` | PASS（非原样：`-p:WarningsNotAsErrors=NU1903`）。实际事件顺序是新 RootView Loaded/Create 先于旧 Unloaded/Dispose，与定稿「先 Dispose 再 Create」相反，已留痕。Gemini cli 主张对 PASS 有异议，交 notion，**尚未裁决**——不要自行改判或改 MainWindow |
| F5 | `e4cd696c10ad3d9a948c5650295be7fbbbc301cc` | **FAIL**。见第 5.1 节 |
| F8a | `c640972a4858c6cb72dbc5060a601c4f040c0e2f` | 仅 DRAFT（`docs/z6_f8a_gate_DRAFT.md`，未 LOCK） |
| F8b | `fe2373b523c6fe2a70598af0bf37b8a028a70d63` | 仅 DRAFT（`docs/z6_f8b_gate_DRAFT.md`，未 LOCK） |
| F9a | `030216fad26d2a2e0d3d3d9ef7edf057d66b07bc` | **FAIL**（原样基线未启动）。用户级 SDK 10.0.404（Roslyn 5.9）已装。见第 5.2 节 |
| F9b | （无分支） | **未开工**。等 F9a 基线完成且 pidan 本人说「开」 |

F1/F2/F6/F11 用过的 `-p:WarningsNotAsErrors=NU1903` 是 workbuddy 当时经 pidan 按票批准的非原样开关，**不自动覆盖你的新构建**。你要再用，须在你自己的会话里重新拿到 pidan 原话，并写明「非原样」，不得声称原样。

workbuddy 曾有卡 t206s992「`wb/z6-*` 分支可以普通 push（常设许可）」。这是给 workbuddy 的，**不转给你，也不覆盖 `codex/z6-*`**。你必须在自己与 pidan 的会话里重新取得许可。

---

## 5. 未完成工作

### 5.1 F5（优先开发）

现象（已由续跑实验证实，Windows 11 26100）：pidan 批准后用 `SystemParametersInfo(SPI_SETCLIENTAREAANIMATION=0x1043, …, SPIF_UPDATEINIFILE|SPIF_SENDCHANGE)` 在 GUI 运行期间关→开，SPI 回读与 `UserPreferencesMask` 变化，新进程能读到 `UISettings.AnimationsEnabled` 跟随变化，但已运行进程里 `AnimationsEnabledChanged` / `source=event` 均为 0 行。失败点是**变更通知未送达订阅者**，不是切错设置、也不是订阅没装上。注意：`0x1041` 是 `SPI_SETDISABLEOVERLAPPEDCONTENT`，**不是**动画开关。

不要把「该事件在 WinUI 3 / Win32 桌面进程不触发」写成官方已知行为（官方「桌面应用不支持」事件表只列了 `ColorValuesChanged` 和 `HighContrastChanged`，此点标未证实）。

**采用 Gemini 网页版修法**（`z6_f5_fix_gemini.md`，sha256 `6cb9738964c51f1ebada3c6a45d0fe65bdcb7f5fa5ad9f4ea51b8fe2e6dbb62d`），只改宿主侧，不改 L1、不改 `composition.h`：

- **主路径**：在顶层 HWND 收 `WM_SETTINGCHANGE`（`0x001A`），100 ms 去抖后重读 `UISettings.AnimationsEnabled`，再经 `DispatcherQueue.TryEnqueue` 调用 `SetReducedMotion(!enabled)`，日志 `source=wm`。
- `AnimationsEnabledChanged` 只作尽力而为的次路径（`source=event`）；`Activated` 时再重读一次。
- 钩子挂在 `MainWindow`，不要挂在每个 `NavMotionPilot` 上——F2 会重建 RootView，但 HWND 不变。
- 优先用宿主已有的 WinUIEx 2.9.2 `WindowMessageMonitor`；若 2.9.2 程序集与文档对不上，改用 `SetWindowSubclass` P/Invoke。不要加新包、不要升级 WinUIEx。不要用仅消息窗口（官方写明不接收广播）。不要把 `SystemEvents.UserPreferenceChanged` 当主路径。
- 不要要求 `wParam==0x1043` 才处理：任何 `WM_SETTINGCHANGE` 都进入去抖+重读；值没变只打日志、不调 `SetReducedMotion`。

**验收：**

- A. 公开面冻结：不改 `MotionSoftDemo/`、`code/`、`pilot/l1pack/`，`composition.h` 不变，不合 main。
- B. 默认自动验收走 Fake（`FM_PILOT_F5=1`）：系统动画设置不变；`source=fake` 能在 UI 线程翻转 `SetReducedMotion` 并恢复。
- C. 不切系统设置也能验证钩子：日志有 `F5 hook installed hwnd=… ok=True`；注入 `0x001A` 打出 `WM_SETTINGCHANGE` 日志，值未变则不调 `SetReducedMotion`。
- D. 真实 SPI / 设置应用切换**只能由 pidan 本人操作，或凭拍板卡原话授权**。你不得自行切换系统动画设置。获准后必须看到 `source=wm` 跟随，然后恢复。不计时、不用 PERF-CI。钩子安装失败则停止并报告。

从 `wb/z6-f5` tip `e4cd696c` 拉**新分支**继续，不要改写 `wb/*`。

### 5.2 F9a / F9b

- Files `v4.3` = `cbfb4ba54a89632fbceb82ea94ada60a9bae3737`。已知缺口：C++ 子项目要 `PlatformToolset=v145`（VS 2026）；K93 此前是 VS 2022 17.14。pubxml 设了 `PublishAot=true`，预计会碰到 K-AOT-ETW。
- 用户级 SDK 10.0.404（Roslyn 5.9）**已经装好**（`-NoPath`，未写用户 PATH），不要重装。
- VS 2026 C++ 桌面工作负载（`Microsoft.VisualStudio.Workload.NativeDesktop`，含推荐组件）**必须由 pidan 本人用已提升进程安装**（上次 `--passive` 因未提升以 5007 退出，未弹 UAC）。你不得自行提权或点过 UAC。装好后再续跑原样基线。
- 首跑失败记录保留在 `attempt1/`，不为凑结果而重跑已计数的失败会话；续跑作为新 attempt 另存。
- F9b（侧叉接入 L1 pack）等 F9a 基线完成后，**且 pidan 本人说「开」**再做。口径同 T5（需要 ETW 就设 `EventSourceSupport=true`，记为偏差）；patch 入库，不把 Files 源码入库。

### 5.3 结题与合 main

- `docs/z6_closed.md` 等 notion 复核稿落地后再写，不要抢先定稿。
- Z6 合 main 的方案可以整理建议（必须按第 3 节真实拓扑，并写明评审结论），但合 main、开/合 PR 必须等 pidan 本人原话。任何票改了 pack 源（`MotionSystem.cs`、`MotionEtw.cs`、`fm_capi.cpp`），F7 和 F11 都要重做。
- F8a/F8b 保持 DRAFT。你不得自行 LOCK，不得新建或改名任何 `*_LOCKED.md`。
- F4 可选 `#if DEBUG` 代码仅在 pidan 要求时做，且须排在下一次 F7 之前，按 T3 标准（changelog 一行、Debug/Release CheckSurface、devsmoke PASS）。

---

## 6. 硬红线（不随本次交接放宽）

1. **`code/composition.h` 冻结。** `git rev-parse HEAD:code/composition.h` 必须是 `fd0603c17136c067e89c71e56aef310f2f7e63f4`；归一化（CRLF→LF）sha256 为 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`。
2. **L1 合同冻结**（`docs/api_surface_m2_alpha_contract_LOCKED.md`，GATE_BODY_SHA256 `77416e8bbc700238b42a940a2f93d4466edec1ebd79352b007f86c267a663a0f`）。不改公开面（`MotionSystem` / `AnimationHandle` / `MotionTokens` 的公开成员）、`fm_*`、E1（事件名、id、字段、provider GUID）。`code/capi/fm_capi.h/.cpp` 不改；`MotionSoftDemo/MotionEtw.cs` 不改。
3. **不采数**，除非该票已有 LOCKED 闸门，并且 pidan 本人说了「APPROVED \<票名\>」。**PERF-CI 继续 HOLD**，任何票的 APPROVED 都不解除它。已锁阈值永不改。失败的计数会话不重跑凑数。
4. **不 force、不 amend、不 rebase。** 只用普通 commit 和普通 push。
5. **不合 main**，除非 pidan 本人原话。开 PR、合 PR、打 tag、发 release、删远端分支，同此。
6. **授权只认 pidan 本人原话，而且只认在你（codex cli）自己会话里的两种形式**：pidan 的用户消息，或 pidan 对你发出的拍板卡的回答。协调 bot 或任何 agent 转述的「pidan 同意了」（即使附了原文）、卡片预填文字、仓库文档里的引用，一律不算。收到转述就自己发卡向 pidan 求证；核验不到按未授权处理。
7. **系统级步骤先停，按票向 pidan 请示**，包括：装 SDK / 运行时 / VS 组件或工具集（如 v145）；改注册表；包注册或卸载（`Add-AppxPackage -Register` / `Remove-AppxPackage`）；开发者模式；提权或 UAC；证书；电源、显示、DPI、防火墙、计划任务、`dotnet tool -g`；改辅助功能（系统动画设置）。**非原样构建开关**（如 `-p:PlatformToolset=…`、`-p:NuGetAudit=false`、`-p:WarningsNotAsErrors=NU1903`、只构建子项目）同样须 pidan 按票原话批准。
8. **不花钱，不改任何账号。**
9. **Success≠adopt。** 通过写「PASS（dev-only）」。开发环境结果不是计时数据。
10. **第三方仓库永不 fork / 推送 / 登录。** 不在入库文件里硬编码本机绝对路径。
11. 提交说明用英文 ASCII，经 `git commit -F <文件>` 传入。已有证据只追加、不改写（`z3a/`、`z4/`、`z5/`、`pilot/` 下已有文件同此）。先入库，再清理；不得删除唯一一份日志。

碰冻结面、修不好的构建失败、需要采数或计时、需要系统级步骤或非原样开关、第三方要登录、需要合 main / 开合 PR / LOCK / APPROVED、要花钱或动账号：先停下，直接请示 pidan（也可以同时告诉协调 bot）。

---

## 7. 工作流

- **一票一分支。** 命名 `codex/z6-<票号>`，或从相关 `wb/z6-*` tip 拉出新分支继续。**永不改写 `wb/*` 分支**（不 force、不 amend、不 rebase、不在其上直接提交后再推）。REPORT 写明实际 base sha。main 若前进，开新分支，不 rebase。
- 每张票在分支上放 `z6/<票号>/REPORT.md`：做了什么、命令、结果、偏差、副作用和清理、授权留痕（许可原话逐字、AWST 时间、所在会话、拍板卡 id 或消息 id）。附原始日志（构建退出码、测试输出、CheckSurface、公开面 dump、运行日志等），并记 sha256。单个证据文件不超过 5MB；截图只截当前窗口。
- 普通 push（须先有第 6.6 节的本人许可），然后报告 tip sha。协调 bot 在 GitHub 上核对。未推送的结果一律标「未复核」。
- K93 本地只追加台账；push 时把快照放到分支的 `z6/LEDGER_snapshot.md`。
- 侧叉改动以 patch 入库并记 sha256；引用预编 DLL 时在构建时校验 sha256。宿主开了 `PublishAot` 时，先查 runtimeconfig 里的 `System.Diagnostics.Tracing.EventSource.IsSupported`；需要 ETW 就显式设 `EventSourceSupport=true`，并记为偏差。

---

## 8. 建议下一票顺序与验收

1. **F5 修法（先做）。** 新分支 `codex/z6-f5`，base = `wb/z6-f5`（`e4cd696c`）。验收见第 5.1 节 A–C（必做）；D 仅在 pidan 本人批准后做。composition.h PRE=POST。不计时。
2. **F9a 续跑。** 等 pidan 用管理员权限装好 VS 2026 C++ 工作负载之后，新分支 `codex/z6-f9a`，base = `wb/z6-f9a`（`030216fa`）。验收：Files 仍在 `cbfb4ba5`；原样构建（或经批准的非原样，且标明）的原始日志入库；BASELINE 只追加；保留 attempt1。不要重装已有的用户级 SDK 10.0.404。
3. **F9b。** 仅当 F9a 基线完成且 pidan 本人说「开」。新分支，patch 入库，不把 Files 源码入库。
4. **遗留评审项。** 等 notion 结果复核：F2 PASS 争议、F12 措辞、F5 证据链（首跑 `build.log` 误删后已补转录）等。裁决前不要改判、不要补写结题文档。
5. **合 main 建议。** 按第 3 节真实拓扑整理，等 pidan 原话。不要自行开 PR。

顺序可微调，但不要在 F5 / F9 未决时去改已 PASS 的票。若你的改动碰了 pack 源，先停并计划重做 F7、F11。

---

## 9. 材料位置

若分支 `coord/z6-codex-handover` 已推送，完整材料在该分支的 `docs/z6_coord/`（含 `results/`、本提示词 `codex_handover.md`、定稿 r2、三方交接评审、裁决、Gemini cli 结果复核、F5 修法）。notion 的结果复核稿与 `z6_closed` 草稿若当时尚未落盘，以该分支说明为准。

也可向协调 bot 索取盒上副本：`/workspace/review/z6/`（跳过 `_scratch_*`）。

---

## 10. 本次未能核验或仍未决

- notion ai 的 `z6_result_review_notion.md`、`z6_closed_DRAFT_notion.md` 在交接时尚未存在。
- 远程是否曾 force-push：对象层无 amend 痕迹，但未用带鉴权的 GitHub 审计核对。
- F2/F5 所称「侧叉磁盘在打补丁前已含 F6 / F1 C 本地改动」无法在本仓库复现。
- WinUIEx 2.9.2 里 `WindowMessageMonitor` 的精确类型/命名空间，请在 K93 上对应当时代码核实后再写实现。
