# Z6 交接提示词 · 裁决记录（notion ai）

- **时间**：2026-11-18 约 06:55 AWST（Australia/Perth，UTC+8）
- **纳入的评审**：
  - notion ai：`z6_handover_review_notion.md`，sha256 `84a2c95d…f8ce0`
  - Gemini cli：`z6_handover_review_geminicli.md`，sha256 实测 `e28cce3c6403ef0b7bf9a10b9652eea41418abbe02e9b2897a73e4836ca47869`，与协调给的 `e28cce3c…7869` 一致
- **Gemini 网页版**：r1 定稿时不存在，没有纳入；**06:49 到达后已补裁，见第 8 节，并出了 r2**。
- **草稿**：`z6_workbuddy_handover_DRAFT.md`，`c95b27d0…0251`
- **定稿**：`z6_workbuddy_handover_FINAL.md`，sha256 和行数见文末
- **两方总判定**：都是有条件 ACCEPT（通过），都没有 REJECT。

## 1. 授权事实（裁决前提）

- 06:19「先合」（`[t470u]`）和 06:30 那三行（`[t472u]`）都是 pidan 在动效框架会话里的本人消息，已核实（详见 notion 评审 §0）。
- Gemini cli 的 S1 问到「PR #9 授权记录待补」：已由 06:19「先合」闭合。FINAL 头部引用了这句，F12 负责把它补进 `docs/z5_closed.md`。

## 2. 两方重合，直接采纳

| 主题 | notion | Gemini cli | 落入 FINAL 的位置 |
|---|---|---|---|
| F5 改为宿主侧，L1 不改 | M6 | M1 | 三-A F5 |
| 不改 `*_LOCKED.md` | M3 | M3 | 二-3 |
| 授权只认 pidan 本人，转述不算 | M4 | M2-1 | 一·细则 |
| 逐次许可、REPORT 留痕 | M5 / M8 | M2-2、M2-4 | 一·细则 |
| F8 只起草闸门，LOCK 由 pidan | M5、S3 | M6 | 三-C、一·细则 |
| F9 的系统级停点 | §3 | S5 | 三-B |
| 证据要原始输出入库 | M8 | M4（F7）、S2 | 四 |

## 3. Gemini cli 新增点的裁决

| # | Gemini cli 新增 | 核实 | 裁决 |
|---|---|---|---|
| N1 | F7 钉死源码 hash，公开面和 `surface_l1pack.txt` 比对 | 已用 git 核实：main 上 MotionSystem.cs 是 `fe72b08f…`，MotionEtw.cs 是 `0404498a…`。`surface_l1pack.txt` 和 T3 Release dump 相比只差首行，外加 devsmoke 类型；这和我 Z5 复核时看到的一致 | **采纳**。验收的 5 条写进 F7；比对基准改为 surface_l1pack（只允许首行不同），比草稿「与 T3 Release 一致」更精确 |
| N2 | F7 在分支上做，不直接改 main | 草稿写「在 main 上重打」，和「不合 main」字面冲突 | **采纳** |
| N3 | F4 可选代码只能是 `#if DEBUG`，不进 MotionEtw.cs，不加任何公开成员 | MotionEtw.cs 就是 E1 定义文件（Z5 复核已确认） | **采纳**，并加一条：在 api_changelog 登记 internal 改动；加了代码 F7、F11 要重做 |
| N4 | F4 不改 `docs/api_v0_frozen.md`，指引另建文档 | 我在评审 S4 里建议写进 api_v0_frozen 的「已知限制」。该文件不是 `*_LOCKED.md`，T2 也曾追加过；但 K-AOT-ETW 已登记在 main 上的 `docs/z5_closed.md` 第 9 节，满足 pidan 06:03「登记为已知限制」 | **采纳 Gemini cli，撤回我的 S4**。api_v0_frozen.md 按「碰冻结面」停下，这是更保守的一方；指引放进新文档 `docs/z6_aot_host_guide.md` |
| N5 | 未经授权不得开 PR、打 tag、发 release、删远端分支 | — | **采纳**，写进一·细则和第五节 |
| N6 | 给「不采数」下定义，消除和 F1/F2/F6 的冲突 | — | **采纳**，写进一·细则 |
| N7 | 「唯一提示词」改为仓库约束继续有效，冲突时以更严的为准 | — | **采纳**，写进头部 |
| N8 | 请示 push 的卡里写分支名和 diff --stat | — | **采纳** |
| N9 | 向 pidan 请求合 main 之前，**必须**先经协调组织评审并至少一方 ACCEPT | pidan 06:30 原话是「是否向你汇报由他自己决定」；把评审设成合 main 的硬前置，等于给 pidan 的决定权加了一道 agent 关卡 | **修正**：不设硬前置。改为合 main 请示卡里**必须写明**是否经过评审、结论是什么，由 pidan 决定。这样既保住授权边界②「合 main 须 pidan 本人的话」的原意，也不违背①「汇报自定」 |
| N10 | 核验不到本人许可，按未授权处理 | — | **采纳** |
| N11 | 分支名小写，可以叠在别的分支上 | — | **采纳**（二-5） |
| N12 | F1 最终值等于后写者；F2 不采样句柄数；F6 用录屏或两帧截图，加首帧先于 Play；F3 文件白名单；F11 用新 SUMS、在副本上改坏、patch 入库 | — | **全部采纳** |
| N13 | F8 未 LOCK 之前探针代码一律不进分支；LOCKED 文件由协调落盘；APPROVED 不解除 PERF-CI | — | **采纳**，再加上我的 S3：拆成 F8a、F8b |
| N14 | Gemini cli 席位驳回了 Gemini CLI 模型的意见「F7 允许改 MotionEtw.cs」 | 和事实不符 | **同意驳回**：F7 不改任何源 |

## 4. notion 独有项（Gemini cli 没有提），全部保留

- **F0 合并后冒烟**（M1）：7e2feb0e 合并后的 tree 没人跑过测试，排在最前。
- **F12** 给 `docs/z5_closed.md` 追加合入记录（M2）。Gemini cli 的 S1 也指出了同一个问题，所以两方实际同向。
- **FluentHub 包注册已清除**，F1、F2、F6 的 GUI 运行要先请示重新注册（M7）。
- **证据硬规全套**（M8）：不删唯一一份日志、构建时校验 sha、AOT 检查、不读 PresentStats 字段值、attempt1 保留。
- **合入 main ≠ adopt**（S2）；composition.h 和合同的全长校验值（M3）。
- **F1 先做 devsmoke 确定性用例**，不改 token（M6）。
- **路径不硬编码**：FINAL 用 `%USERPROFILE%` 写本机目录，并禁止在入库文件里写绝对路径。

## 5. 顺序的合并结果

- notion 的顺序：F0 → F12 → F7 → F11 → F3 → F6 → F1/F2 → F5 → F4 → F9。
- Gemini cli 的顺序：F7 → F11 → F3 → F1/F2 → F5/F6 → F4（有代码就重做 F7）→ F9。
- **裁决**：用 notion 的顺序，并吸收 Gemini cli 的「F4 有代码就重做 F7、F11」。F6 排在 F1 前面，因为 F1 的 GUI 重叠证据依赖 F6。F8a/F8b 的 DRAFT 并行写。F0 必须最先，其余可以由 workbuddy 调整。

## 6. 三条授权边界的保真检查

1. **①汇报自定**：FINAL 第一节第 1 条引用 pidan 原话，没有附加任何「必须向谁汇报」的义务。REPORT 留痕写成入库证据，明确「不算汇报」。N9 已改为不设评审前置。
2. **②push 和合 main 须 pidan 本人的话**：原样保留。细则只说明了「本人的话」怎么识别（只认 workbuddy 自己会话里的消息或卡片），以及许可粒度（push 可以有常设许可，合 main 逐次）。没有放宽。
3. **③采数须 LOCK 加 APPROVED**：原样保留。细则补了 workbuddy 不得自行 LOCK、LOCKED 文件由协调落盘、「采数」的定义、APPROVED 不解除 PERF-CI。没有放宽。

## 7. 结论

**FINAL 定稿**，可以原样交给 workbuddy。是否发出、怎么发由协调处理；本裁决不发任何消息。

Gemini 网页版没有纳入，日后到达时补裁，不阻塞交接。

## 8. Gemini 网页版补裁（2026-11-18 约 06:52 AWST）

- **评审文件**：`z6_handover_review_gemini.md`，sha256 实测 `4e35cdb02b7a30fd4a68e3545b736abca4aef6117cbc3ecb65ac235efa788ec0`，与协调给的 `4e35cdb0…8ec0` 一致。共 686 行，结论有条件 ACCEPT。
- **它评审的对象**：DRAFT r0，不是 r1。它在补记里提到看到了 r1。
- **裁决对照的版本**：r1，`a7eff961…41b6`，06:42 已交给 workbuddy，workbuddy 正在做 F0、F12。
- **产出**：`z6_workbuddy_handover_FINAL_r2.md`，文头有「r1→r2 变更摘要」9 条。
- **对 F0、F12 的影响**：
  - F0 已做的工作无须重做。
  - F12 已做的工作无须重做，只需用一个普通提交在标题下补一行（第 2 条）。

### 8.1 核实结果

| 声称 | 核实方法 | 结果 |
|---|---|---|
| Files `v4.3` = `cbfb4ba5…` | `git ls-remote` 加浅克隆，只读 | 属实：`cbfb4ba54a89632fbceb82ea94ada60a9bae3737`，「Build: v4.3」，提交时间 2026-10-12 02:04 AWST |
| C++ 子项目要 v145 | 读 vcxproj | 属实：Launcher、OpenDialog、SaveDialog 都是 `<PlatformToolset>v145</PlatformToolset>` |
| K93 是 VS 2022 | 读仓库 | 属实：`pilot/l1pack/SHA256SUMS` 第 5 行写「VS 2022 BuildTools 17.14」；T3 构建脚本调用的是 `2022\BuildTools` 下的 vcvars64 |
| pubxml 设了 PublishAot | 读 csproj 和 pubxml | 属实：`win-x64.pubxml` 里 `PublishAot=true`、`SelfContained=true`；`Files.App.csproj` 设了 `PublishProfile=…win-$(Platform).pubxml`、`IsAotCompatible=true`；`global.json` 是 SDK 10.0.102 加 latestMajor |
| 禁止在 sink 回调里调 fm_* | 读合同和源码 | 属实：`docs/api_surface_m2_alpha_contract_LOCKED.md` Table 2 第 2 条「No fm_* inside sink/render callbacks」；`MotionSystem.cs` 里 `_nativeDepth != 0` 会抛 InvalidOperationException |
| 帧泵关闭后重复回调 render(off) | 读源码 | 属实：`code/explore/software_dispatcher.h` 的 `FramePump::OnFrame` 每次都 Tick；`ActiveCount()==0` 时**无条件**调 `setRendering_(false)`，不看 `running_`。C# 的 `OnRender` 每次都会触发 FramePumpChanged 和 IdleTransition ETW |
| main 上 `z5_closed.md` 文件头是旧状态 | 读文件 | 属实：第 1、3 行写「待落盘」「不合 main」，第 8 节第 250、255 行也一样 |
| H12：定稿引用了 K93 读不到的 `/workspace` 路径 | 在 r1 里 grep `workspace` 和 `/tmp` | **r1 里 0 处命中**。Gemini 网页版评审的是 DRAFT，r1 本身已经自包含。r2 也是 0 处命中 |
| C8：`api_changelog.md` 冲突已解决 | 读 main | 属实。r1 本来就没把它列成待办，所以不涉及 |

### 8.2 逐条裁决（对照 r1）

| 项 | 裁决 | 进 r2 第几条 |
|---|---|---|
| **C1 / H4 包注册** | **采纳须改**。r1 只写了「F1、F2、F6 可以一次性请示」，范围不够。按 notion 口径改写：workbuddy **可以申请**一次 Z6 范围内的总许可，拿到后逐字记录，否则逐次请示；**不预设许可**。驳回 Gemini 网页版「定稿前请 pidan 先给总许可」的写法，那等于替 pidan 安排了决定 | 1 |
| C2 F5 宿主侧 | r1 已覆盖 | — |
| **C3 F1 分 A/B/C 层，C 层不在 sink 里调 fm_\*** | **采纳**。A 层 C ABI 手动时钟测试成本低、确定性强；sink 规则是 LOCKED 合同硬句，r1 漏写了 | 3 |
| **C4 F9 钉版本、v145、PublishAot、拆两票** | **采纳**，都已核实 | 4 |
| **C5 改 L1 源码的票排在 F7 之前** | **采纳**。可以减少返工；保留「F7 之后改了 pack 源就重做」作兜底 | 5 |
| C6 / H2 不改 LOCKED | r1 已覆盖 | — |
| **H1 / H13 第三方仓库和账号** | **采纳** | 6 |
| **H5 系统级清单 / H6 非原样构建开关** | **采纳**。r1 已有证书和开发者模式，补上其余各项 | 7 |
| **H3 证据只追加 / H7 FrameTick 只计数 / H8 等待编排 / H9 记录 base sha / H11 T3 标准** | **采纳**，每项一句 | 8 |
| **C7 台账** | **部分采纳**：采纳 K93 本地只追加台账、push 时快照入库、未推送标「未复核」。**不采纳**「每次请示都要附台账摘录」和「协调可随时要求出示」，那会变相规定汇报方式，与授权边界①冲突，而且太细 | 8 |
| **C8 z5_closed 文件头** | **采纳**。r1 的 F12 只在文末追加，文件头仍会误导读者。改为在标题下也插一行，原文不动，沿用 Z5 C1 的先例 | 2 |
| H10 来源 id | r1 已覆盖 | — |
| H12 `/workspace` 路径 | r1 已覆盖（0 处命中）。附带**驳回**：Gemini 网页版在盒子 `/tmp` 里写的 F1-A、F3 测试草稿不作为附录。K93 读不到它们，应由 workbuddy 自己写，以 MSVC 实测为准 | — |
| H14 清理时机 | r1 的「先入库，再清理；不删唯一日志」已覆盖实质；清理系统级副作用本来就按许可原话执行 | — |
| **观察：帧泵关闭后重复 render(off)** | **采纳为验收口径**：F2 只断言最后一条 IdleTransition 是 pumpOn=0。**不开 L1 修复票**：改 `FramePump` 会碰 code/，而且只有宿主不退订时才会出现，T5 侧叉会退订。将来是否开票由 pidan 决定 | 9 |
| F4「本轮只写文档，L1 Debug 提示另开票」 | **驳回**。r1 已允许可选代码，并带严格条件（`#if DEBUG`、不碰 MotionEtw.cs、F7 重做）；r2 再要求它排在 F7 之前，风险已经可控 | 5 |
| 其他：P0 侧叉固化、文档改名为 `l1_host_guide_aot_etw.md`、拍板卡建议文字、Wino-Mail 等 | **不采纳**。属于细枝末节，或者 r1 的 F11、F4 已经覆盖 | — |

### 8.3 授权边界保真

- r2 第一节（三条边界和实施细则）与 r1 **逐字相同**（diff 为空）。
- pidan 原话没有改动。
- 新增内容只收紧，不放宽。
- 包注册写成「可以申请」，没有替 pidan 预设许可。

### 8.4 结论

**出 r2。** r2 怎么交给 workbuddy、什么时候交，由协调处理；本裁决不发任何消息。
