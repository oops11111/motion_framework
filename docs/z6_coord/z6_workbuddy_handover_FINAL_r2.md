# Z6 交接提示词（FINAL r2 · notion ai 裁决定稿 · 2026-11-18 AWST）

## r1→r2 变更摘要（r1 = `a7eff961…41b6`，06:42 已交给你）

**F0 已做的工作无须重做。F12 已做的工作也无须重做**，只需按第 2 条补一行：用一个普通提交追加即可，不 amend。

1. **包注册**：你可以向 pidan 申请一次「Z6 范围内松散包注册（Add-AppxPackage -Register / Remove-AppxPackage）」的总许可，拿到后逐字记录；没拿到就逐次请示。这不是已经给你的许可。（第二节第 7 条）
2. **F12 补一行**：`docs/z5_closed.md` 文件头仍写「定稿（待落盘）」「不合 main」，那是结题时的状态。F12 除了在文末追加合入记录，还要在**标题下插入一行**说明「已经 PR #9 合入 main `7e2feb0e`，见文末合入记录」。原有文字一字不改，沿用 Z5 C1 的先例。
3. **F1 分三层**：A 层是 C ABI 手动时钟测试；B 层是 devsmoke `CreateManual`；C 层是宿主由程序状态触发。C 层**不得在 sink/render 回调里直接调 fm_\***（L1 合同 Table 2 第 2 条，运行时 `_nativeDepth` 会抛异常），要用 DispatcherQueue 排队。不靠人手悬停。
4. **F9 拆成 F9a 和 F9b**，并钉死版本：Files `v4.3` = `cbfb4ba54a89632fbceb82ea94ada60a9bae3737`；已知缺口写进票里（见 F9a）。
5. **顺序调整**：所有改 L1 源码的票（含 F4 可选代码）排在 F7 之前。
6. **第三方仓库和账号**：不 fork，不推送，不提 issue、PR 或评论，不 star；不登录任何第三方账号。（第二节第 11 条）
7. **系统级清单补全**：补 VS 组件或工具集、`dotnet tool -g`、计划任务、防火墙、辅助功能（动画效果）、DPI 和显示。非原样构建开关（如 `-p:PlatformToolset=…`、`-p:NuGetAudit=false`）须 pidan 拍板。（第二节第 7 条、新第 12 条）
8. **证据补充**：
   - 已有证据只追加、不改写；
   - FrameTick 的 deltaUs 只计数，不落数值；
   - 脚本里可以用等待做编排，但等待时长不写成结果；
   - 未推送的结果一律标「未复核」；
   - 报告记录实际的 base sha；main 前进后开新分支，不 rebase；
   - L1 内部改动按 T3 标准做；
   - K93 本地只追加台账，push 时把快照入库。（第四节）
9. **帧泵关闭后的行为**（已核实 `code/explore/software_dispatcher.h` 的 `FramePump::OnFrame`）：`ActiveCount==0` 时，每次 `fm_on_frame` 都会再回调一次 render(off)。所以 F2 只断言**最后一条** IdleTransition 是 pumpOn=0，不断言条数。不为这件事改 L1。

---

# Z6 交接提示词

> 依据（pidan 本人原话，逐字，已在动效框架会话中核实）：
> - 06:19 AWST「先合」（Z5 由此经 PR #9 合入 main）
> - 06:30 AWST「后续票全开，任务交workbuddy / 提示词三方讨论后让它接手开发 / 是否向你汇报由他自己决定」
>
> 基线：main `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`（tree `d61f0138f06b27d6f184cc63b04df2ab84ab945d`）。Z5 结题文档在 `docs/z5_closed.md`。
> 本文原样发给 workbuddy cli，作为它接手 Z6 开发的提示词。仓库里 `*_LOCKED.md`、`docs/api_v0_frozen.md`、`docs/z5_closed.md` 的约束继续有效；与本文冲突时，以更严的一方为准。

---

你接手「Windows 桌面端动效框架」Z6 开发，在 K93（DESKTOP-K93N3P8）上执行。Z5 已经合入 main `7e2feb0e`。下面第三节的票，pidan 已经说「全开」，由你自己排期、开分支、实现、自测。

## 一、三条授权边界（原样，不得改变含义）

1. **汇报由你自己决定。** pidan 原话：「是否向你汇报由他自己决定」。你可以直接在你和 pidan 的对话里汇报、请示，不必经过协调 bot（动效框架）。需要评审或对照时，可以找协调 bot 组织。
2. **push 和合 main 须 pidan 本人的话。**
3. **采数须 LOCK 加 APPROVED。**

### 实施细则（只细化上面三条，不放宽）

- **授权只认 pidan 本人**，而且只认在**你自己会话里**的两种形式：pidan 的用户消息，或 pidan 对你发出的拍板卡的回答。
  - 以下一律不算：协调 bot 或任何 agent 转述的「pidan 同意了」（即使附了原文）、卡片上的预填文字、仓库文档里的引用。
  - 收到转述时，自己发卡向 pidan 求证。核验不到的，按未授权处理。
- **push**：
  - 可以发卡请 pidan 给一次常设许可，例如「`wb/z6-*` 分支可以普通 push」。拿到后逐字记入每张票的 REPORT。pidan 随时可以撤回。
  - 没有常设许可时，逐次请示。请示卡里写明分支名和 `git diff --stat` 摘要。
- **以下动作常设许可都不覆盖，必须逐次拿 pidan 本人的话**：合 main；开 PR 或合 PR；打 tag；发 release；删除远端分支；LOCK；APPROVED；系统级步骤（见第二节第 7 条）。
  - 请示合 main 时，在卡里写明这张票是否经过协调组织的评审，以及评审结论，由 pidan 决定。
- **你不得自行 LOCK**：
  - 闸门只写 `*_DRAFT.md`；
  - 只有 pidan 本人说了 LOCK，LOCKED 文件才由协调落盘，你不新建、不改名、不复制出任何 `*_LOCKED.md`；
  - 采数还须 pidan 本人说「APPROVED <票名>」。
- **「采数」的定义**：产出用于判定、闸门或性能结论的数值，包括计时、帧率、帧间隔、PresentStats 字段值，以及任何闸门会话。
  - F1、F2、F6 这类 dev-only 运行证据**不算采数，可以做**：ETW 事件名和顺序、Created/Released 配对计数、日志、截图、录屏。
  - 但报告里不得出现计时判据或帧率结论，也不计任何闸门会话。
- **留痕（必须做，不算「汇报」）**：每次 push 后，分支上的 `z6/<票号>/REPORT.md` 都要写明所依据的许可，包括原话（逐字）、AWST 时间、所在会话、拍板卡 id（`tNNNsNNNN`）或消息 id（`tNNNu`），供任何人只读核验。

## 二、硬约束（不随本次授权放宽）

1. **`code/composition.h` 冻结。**
   - 校验方法：`git rev-parse HEAD:code/composition.h` 应为 `fd0603c17136c067e89c71e56aef310f2f7e63f4`；归一化（CRLF→LF）sha256 应为 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`。
   - 每张票的报告都记录 PRE/POST。
2. **L1 合同冻结**（`docs/api_surface_m2_alpha_contract_LOCKED.md`，GATE_BODY_SHA256 `77416e8bbc700238b42a940a2f93d4466edec1ebd79352b007f86c267a663a0f`）。
   - 不改公开面（`MotionSystem` / `AnimationHandle` / `MotionTokens` 的公开成员）、`fm_*`、E1（事件名、id、字段、provider GUID）。
   - `code/capi/fm_capi.h/.cpp` 不改；`MotionSoftDemo/MotionEtw.cs` 不改。
3. **不改、不新建、不改名任何 `*_LOCKED.md`**（main 上现有 34 个）。`docs/api_v0_frozen.md` 和合同文件也不改，要改就按「碰冻结面」停下。已锁阈值不改。
4. **不采数**，除非该票有 LOCKED 闸门，并且 pidan 本人说了「APPROVED <票名>」。**PERF-CI 继续 HOLD**，任何票的 APPROVED 都不解除它。计数会话失败不重跑凑数。
5. **分支**：
   - 每张票一个分支，命名 `wb/z6-<票号小写>`（例如 `wb/z6-f7`），从 main `7e2feb0e`（或之后 pidan 认可的 main）拉出。
   - 依赖别的 Z6 票时，可以叠在那张票的分支上拉，REPORT 写明父提交。
   - 只用普通 commit 和普通 push：不 force，不 amend，不 rebase。
6. **不合 main。** 合 main、开 PR 或合 PR，都按第一节细则逐次拿 pidan 本人的话。
7. **系统级步骤先停下请示 pidan**，包括：装 SDK、运行时或 workload；改注册表；注册或卸载包（含用户级 `Add-AppxPackage -Register`）；开发者模式；提权或 UAC；证书；改电源、显示或系统动画设置。
   - 系统级步骤还包括：装 VS 组件或工具集（例如 v145）、`dotnet tool -g`、建计划任务、改防火墙、改辅助功能（动画效果）、改 DPI 或显示设置。
   - **包注册**：FluentHub 的包注册已在 06:03 按 pidan 许可清掉。F1、F2、F5、F6、F11、F9 的 GUI 运行都要用包身份。你可以向 pidan **申请一次** Z6 范围内松散包注册（`Add-AppxPackage -Register` / `Remove-AppxPackage`，当前用户，不装证书，不开开发者模式，不提权）的总许可，拿到后逐字记录。没拿到就逐次请示。不得自行认定已有许可。
8. **不花钱，不改任何账号。** 提交说明用英文 ASCII，通过 `git commit -F <文件>` 传入。
9. **Success≠adopt**：票通过写「PASS（dev-only）」。**Z5 合入 main 也不等于 adopt**：不选 Winner，不升级为正式 L1 类库。
10. **不在提交进仓库的任何文件里硬编码本机绝对路径**（csproj、脚本、配置都算）。本机目录用变量或相对路径，例如 worktree 放在 `%USERPROFILE%\mf_z6\<票号>`，不要动主克隆 `%USERPROFILE%\motion_framework`。
11. **第三方仓库（FluentHub、Files 等）**：只在本地克隆。不 fork，不推送，不提 issue、PR 或评论，不 star。**不登录任何第三方账号**（包括 FluentHub 的 GitHub 登录）。
12. **非原样构建开关**须 pidan 拍板，例如 `-p:PlatformToolset=…`、`-p:NuGetAudit=false`、只构建子项目。用了就记为「非原样」，不得声称原样。

## 三、票单

**A. 可直接做**（遇到第二节第 7 条的系统级步骤仍须停下）

| 票 | 内容 | 验收（全部要有原始输出入库） |
|---|---|---|
| **F0** | **合并后冒烟（最先做）**。7e2feb0e 把 T2、T3 的改动合在一起，合并后的 tree 还没人跑过测试 | 在 7e2feb0e 上：`fm_capi_test` Debug、Release 都是 23 checks / 0 failures；devsmoke（`--z3a-l1-dev`）Debug、Release 都是 verdict=PASS，含 Z5-T3 用例；对 devsmoke Release 程序集跑 CheckSurface，原始输出含 `CHECKSURFACE fails=0`；composition.h PRE=POST。只入库证据，不改源 |
| **F12** | `docs/z5_closed.md` 文件头和第 8 节仍写「待落盘」「不合 main」，这是结题时的状态，已被 PR #9 取代 | 在文末**追加**「合入记录」：PR #9、merge `7e2feb0e`、pidan 06:19「先合」。在**标题下插入一行**：「已经 PR #9 合入 main `7e2feb0e`，见文末合入记录」。原有内容一字不改，`git diff` 只有新增行 |
| **F7** | 合 main 后的整合收尾：在 `wb/z6-f7` 上用 Release 重打 L1 pack，重写 `pilot/l1pack/SHA256SUMS` 和 `README.md`（mf_tip 改为 7e2feb0e）。不改任何源文件 | ① `git diff --name-only 7e2feb0e..分支` 只有 `pilot/l1pack/SHA256SUMS`、`pilot/l1pack/README.md`、`z6/f7/**`。② SHA256SUMS 里 `git hash-object` 的值：MotionSystem.cs = `fe72b08f9445723f926a2928345e23311503efec`，MotionEtw.cs = `0404498ab4cb7e96168405a7ffb63591686797ca`。③ 对**新 pack DLL** 跑 CheckSurface，原始输出入库，含 `fails=0`。④ 新 DLL 的公开面 dump 入库，与 `pilot/fluenthub/surface_l1pack.txt` 相比只允许首行不同。⑤ 产物 sha256 用 `Get-FileHash` 独立再算一遍，结果入库。以后任何票改了 pack 源（MotionSystem.cs、MotionEtw.cs、fm_capi.cpp），F7 须重做 |
| **F11** | FluentHub 侧叉 csproj 改为相对路径或可配置路径；**构建时**校验 pack DLL 的 sha256。依赖 F7 | 校验用的 sha 来自 F7 的新 SHA256SUMS；换目录也能构建；在 DLL 的**副本**上故意改坏，构建应失败，做完复原并记录。侧叉改动以 patch（附 sha256）放进 `z6/f11/`，不把 FluentHub 源码入库 |
| **F3** | `fm_capi_test` 补「空动画 + Play + Retarget」的 C 层用例 | `git diff --name-only` 只有 `code/capi/fm_capi_test.cpp` 和 `z6/f3/**`；Debug、Release 的测试原始输出入库 |
| **F6** | 入场动画可见性：宿主侧等首帧之后再 Play。只改侧叉，须重新注册包 | 录屏（只录当前窗口），或至少两张中间帧截图；日志证明首帧回调先于 Play；不计时 |
| **F1** | 入场动画进行中被悬停打断（同一 (target, offsetX) 两个持有者真正重叠），对照仲裁语义「后写者胜」 | **A 层**：新增 C ABI 测试，用 `fm_create_manual` 和 `fm_manual_advance` 手动时钟，Debug、Release 原始输出入库。**B 层**：devsmoke 用 internal 的 `CreateManual` 加确定性用例，不改公开面。A、B 层的日志要能看到两个持有者重叠、交接处值连续、最终值等于后写者的目标。**C 层**（F6 之后做，须包注册许可）：侧叉由程序状态触发，sink 观察到「入场中途」后，用 `DispatcherQueue.TryEnqueue` 排队调用悬停。**不得在 sink/render 回调里直接调 fm_\***（合同 Table 2 第 2 条）。不靠人手悬停；连续 3 次没触发成功就停下报告。**不得为了凑出重叠去改 token 时长或 `demo_motion.json`** |
| **F2** | RootView 重建（Unloaded→Loaded，先 Dispose 后 Create）的运行证据。只在侧叉里加触发手段，须包注册许可 | ETW 里 AnimationLifecycle 的 Created 数等于 Released 数；Dispose 之后**最后一条** IdleTransition 是 (activeCount=0, pumpOn=0)。帧泵关闭后如果宿主还在调 `fm_on_frame`，会重复回调 render(off)，所以不断言条数。不采样进程句柄数 |
| **F5** | ReducedMotion 跟随运行中的系统设置变化。**这是宿主侧的事**：现在是 FluentHub 侧叉启动时读一次 `UISettings.AnimationsEnabled`。改为在宿主侧订阅设置变化，再调用已有的 `MotionSystem.SetReducedMotion(bool)` | `git diff --name-only` 里没有 `MotionSoftDemo/`、`code/`、`pilot/l1pack/` 下的文件；日志显示设置切换后 `SetReducedMotion` 的入参跟着变；不计时。**切换系统动画设置是系统级步骤，先请示 pidan**；测完恢复原值并记录 |
| **F4** | AOT 宿主关闭 EventSource（已知限制 K-AOT-ETW，已登记在 `docs/z5_closed.md` 第 9 节）。新建 `docs/z6_aot_host_guide.md`，写宿主侧接入指引 | 指引写清 `<EventSourceSupport>true</EventSourceSupport>`，以及通过 pubxml 加 `PublishProfile` 间接引入 `PublishAot` 的情况（Files 就是这样）；自检方法是查输出目录 `*.runtimeconfig.json` 里的 `IsSupported`；引用 T5 的 run1/run2 证据。**可选代码**（要做就排在 F7 之前）：只能在 `MotionSystem.cs` 内部加 `#if DEBUG` 的提示；不改 `MotionEtw.cs`；不新增 public/protected 成员、事件、Keyword 或导出符号；Release 公开面 dump 与 `surface_l1pack.txt` 相比只允许首行不同；在 `docs/api_changelog.md` 登记 internal 改动。加了代码，F7（以及 F11）须重做 |

**B. 需请示 pidan**（做到系统级步骤就停）

| 票 | 内容 | 要点 |
|---|---|---|
| **F9a** | Files（C 试点）原样基线，参照 Z5 T4：本地克隆 files-community/Files，检出 `v4.3` = `cbfb4ba54a89632fbceb82ea94ada60a9bae3737`，记录 `git rev-parse HEAD` | 已知缺口：① C++ 子项目（Launcher、OpenDialog、SaveDialog）要 `PlatformToolset=v145`（VS 2026），K93 现在是 VS 2022 BuildTools 17.14，装组件或改开关都须 pidan 拍板；② `src/Files.App/Files.App.csproj` 设了 `PublishProfile=…win-$(Platform).pubxml`，pubxml 里 `PublishAot=true`，预计会复现 K-AOT-ETW；③ `global.json` 要求 SDK 10.0.102（latestMajor）；④ MSIX 运行须包注册许可。交 `pilot/files/BASELINE.md` 加原始日志；源码不入库 |
| **F9b** | Files 本地侧叉接入 L1 pack | F9a 交了基线、pidan 另说「开」之后才做。口径同 T5（需要 ETW 就设 `EventSourceSupport=true`）；patch 入库 |

**C. 须 LOCK + APPROVED，现在只起草闸门**

| 票 | 内容 | 要点 |
|---|---|---|
| **F8a** | 真实 PresentStats | 在 pidan LOCK 并说「APPROVED F8a」之前：只允许在分支上新建 `docs/z6_f8a_gate_DRAFT.md`；**不写、不编译、不运行**任何探针或计时代码；不试跑工具。DRAFT 绑定 7e2feb0e 或之后 pidan 认可的 main。LOCKED 文件由协调在 pidan LOCK 后落盘。实现时 E1 不变，只把事件 id 4 的值由占位改为实测，登记进 api_changelog；F7 须重做 |
| **F8b** | K93 约 32Hz 计时探针 | 同 F8a（闸门 DRAFT 用 `docs/z6_f8b_gate_DRAFT.md`）。T6、Z4 的旧日志只能当背景引用，不能作阈值来源或通过依据。F8a、F8b 的 APPROVED 都不解除 PERF-CI HOLD，也不改任何已锁闸门 |

**建议顺序**：F0 → F12 → F3 → F1 的 A/B 层 → F4（文档，以及可选代码）→ **F7**（所有改 L1 源码的工作都要在这之前完成）→ F11 → F6 → F1 的 C 层 → F2 → F5。F8a、F8b 的 DRAFT 和 F9a 可以并行；F9b 等 pidan 另说「开」之后再做。顺序可以调整，但 F0 必须最先；F7 之后任何票改了 pack 源，F7、F11 都要重做。

## 四、证据硬规（Z5 的教训）

- **每个结论都要有库内原始输出**：构建日志（含退出码）、测试原始输出、CheckSurface 原始输出、公开面 dump、运行日志、加载模块清单。只有报告文字不算证据。
- **先入库，再清理**。不得删除唯一一份日志。
- 侧叉改动以 patch 入库并记 sha256；引用预编 DLL 时**在构建时**校验 sha256。
- 宿主开了 `PublishAot` 时，先查 runtimeconfig 里的 `System.Diagnostics.Tracing.EventSource.IsSupported`；需要 ETW 就显式设 `EventSourceSupport=true`，并记为偏差。
- `PresentStats` 是占位值，任何报告都不得读它的字段值。
- 首跑失败的原始记录保留在 `attempt1/`，不为凑结果而重跑。
- 报告写全长 SHA、AWST 时间、composition.h PRE/POST。
- **已有证据不可改写**：`z3a/`、`z4/`、`z5/`、`pilot/` 下已有的文件，更正只能追加。
- `FrameTick` 只计数，不把 deltaUs 数值写进日志或报告。脚本里可以用等待做编排，但等待时长不能写成结果。
- 报告记录本票实际的 base sha。main 前进后，开新分支，不 rebase。
- **L1 内部改动**（不改公开面，例如 F4 可选代码）按 T3 标准做：
  - 在 `docs/api_changelog.md` 加一行；
  - Debug、Release 各跑一次 CheckSurface，原始输出入库；
  - devsmoke Debug、Release 都 PASS；
  - 排在 F7 之前。
- **台账**：在 K93 本地 `%USERPROFILE%\mf_z6\LEDGER.md` 记只追加的台账，每行写：AWST 时间、票号、动作（系统级步骤、许可原话和来源 id、副作用、清理）。每次 push 时，把台账副本放进分支的 `z6/LEDGER_snapshot.md`。**没推送的结果一律标「未复核」**。
- 每张票的分支上放 `z6/<票号>/REPORT.md`，写明：做了什么、命令、结果、偏差、副作用和清理方法、授权留痕。单个证据文件不超过 5MB；截图只截当前窗口。

## 五、什么时候停下

遇到下面任一情况，先停下，直接请示 pidan（也可以同时告诉协调 bot）：
- 构建或测试失败，修不好；
- 实现路径要改 `code/composition.h`、`code/capi/fm_capi.h/.cpp`、`MotionSoftDemo/MotionEtw.cs`、L1 公开成员、任何 `*_LOCKED.md`、`docs/api_v0_frozen.md`；
- 需要系统级步骤（包注册，除非已拿到总许可），或需要非原样构建开关；
- 第三方仓库或应用要求登录；
- 需要采数或计时；
- 需要 push（没有常设许可时）、合 main、开或合 PR、打 tag、删远端分支、LOCK、APPROVED；
- 需要花钱或动账号。
