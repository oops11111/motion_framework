# Z6 交接提示词对抗式复核（Gemini cli 席位）

- **复核对象**：`/workspace/review/z6/z6_workbuddy_handover_DRAFT.md`（DRAFT r0，48 行）
  - 实测 sha256：`c95b27d08421a1ac1de054de08fbea787daa78f5f2f91f43bbbd357231650251`，与给定的 `c95b27d0…0251` **一致**。
- **对照材料**：
  - `/workspace/review/z5/z5_closed.md`（sha256 `f82dbd08…1b31`），与 main `7e2feb0e` 上的 `docs/z5_closed.md` 逐字节一致。
  - 仓库 main `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`（tree `d61f0138`；PR #9 合并于 2026-11-18 06:23:57 AWST）。复核前做了只读 `git fetch`，`origin/main` 仍是这个提交。
- **方式**：只读。没有改仓库代码，没有 push。
- **复核时间**：2026-11-18 约 06:45 AWST（Australia/Perth，UTC+8）。
- **来源标注**：〔自核〕表示我自己读文件、跑 git 核出来的；〔Gemini〕表示 Gemini CLI（gemini-3-flash-preview，免费档，单次调用）给出的意见，并注明我是否采纳。原始输出在 `/workspace/z6review/gemini_out.txt`。注意：给 Gemini 的提示里附带了我自核得到的 F5 事实，所以它在 F5 上的结论**不算**独立佐证。

---

## 总判定：**有条件 ACCEPT**

草稿第二节把 Z5 的硬约束基本照搬过来了，而且 push 也要逐次拿 pidan 本人许可，比 Z5 更严。但有三处会让 workbuddy **在字面上合规，实际却越界或卡死**：

1. **F5 的类型标错了**：它是宿主侧的事，却被标成「L1 内部」，按字面执行就会去改 L1 公开面所在的文件。
2. **「汇报自选」之后，授权的留痕和核验点没有了**：没有任何人负责核实「pidan 本人许可」。
3. **第二节第 2 条「不采数」与 F1、F2、F6 的「运行证据」字面冲突**；同时漏掉了「不改任何 *_LOCKED.md」。

下面 M1–M7 改完即可定稿，不需要再开一轮复核；notion ai 抽查 M1–M3 即可。

| 问题 | 判定 |
|---|---|
| ① 硬约束是否被放宽 | **有条件 ACCEPT**：没有显式放宽；有 3 处漏项或歧义会造成事实上的放宽（M3、M4、M7） |
| ② 「汇报由 workbuddy 自己决定」与授权链 | **有条件 ACCEPT**：授权主体没变（仍是 pidan 本人），但缺留痕、核验和兜底（M2） |
| ③ 各票验收是否可检查、是否误触冻结面 | **有条件 ACCEPT**：F5 **须改**（M1）；F7、F4 须收紧（M4、M5）；F2、F6、F11、F9 建议改（S 项） |
| ④ F8 边界 | **有条件 ACCEPT**：方向对，但「起草」的范围、何时算 LOCK、谁落盘 LOCKED、与 PERF-CI 和 E1 的关系都没写（M6） |

---

## 一、关键事实核查〔自核〕

| # | 事实 | 证据（main `7e2feb0e`） | 影响 |
|---|---|---|---|
| E1 | L1 里**没有**任何读取系统动画设置的代码。L1 只有公开方法 `MotionSystem.SetReducedMotion(bool)` → `fm_set_reduced_motion` | `MotionSoftDemo/MotionSystem.cs:147-150`；`code/capi/fm_capi.cpp:73`；`code/controller.h:283-284`；全仓 grep `UISettings\|SystemParametersInfo` 在 L1 源码中为 0 命中 | F5 不是「L1 内部」 |
| E2 | 「启动时读一次」在 **FluentHub 宿主侧叉**：`SetReducedMotion(!UISettings.AnimationsEnabled)` | `pilot/fluenthub/T5_REPORT.md:35`、`:57` | F5 的正确落点是宿主侧 |
| E3 | 要在 L1 里实现跟随系统设置，只能改 `MotionSystem.cs`（公开面所在文件）、`fm_capi.cpp`（草稿第 17 行已冻结），或 `controller.h`（`fm_*` 语义） | 同上 | 按草稿字面做 F5，必然碰冻结面或高风险面 |
| E4 | pack 只链接编译 `MotionSystem.cs` 和 `MotionEtw.cs`。`MotionEtw.cs` 就是 E1 的 EventSource 定义（Name、GUID、事件 id 1–6、103） | `pilot/l1pack/FluidMotion.L1.csproj`；`MotionSoftDemo/MotionEtw.cs:13-82` | F4 的可选代码、F8 的「真实 PresentStats」只要动这两个文件，就会让 pack 失效，并贴近 E1 |
| E5 | d618140b（T3）到 7e2feb0e 之间，pack 源只有 `MotionEtw.cs` 变了（+4 行注释，T2 `f5bdf96`）。`MotionSystem.cs` 的 hash-object 仍是 `fe72b08f…`；`MotionEtw.cs` 由 `565111f0…` 变为 `0404498a…` | `git diff --stat d618140b 7e2feb0e -- <pack 源>`；`git hash-object` | F7 的验收可以写成精确的哈希比对 |
| E6 | `pilot/fluenthub/surface_l1pack.txt`（pack 公开面）和 `z5/t3_results_dev/K93N3P8/surface_ds_Release.txt`（T3 Release，devsmoke 程序集）相比，差别只有首行程序集名，外加后者多出的 `Z3aL1DevSmoke` 类型（2 行） | `diff` 实测 | 草稿第 29 行「与 T3 Release 一致」**字面上做不到**，须写明比对基准和允许的差异 |
| E7 | T3 已在 main：`d618140b` 是 `7e2feb0e` 的祖先 | `git merge-base --is-ancestor` | 「新闸门绑定含 T3 的 tip」已满足，F8 的 DRAFT 应绑定 `7e2feb0e` 或之后的提交 |
| E8 | main 上的 `docs/z5_closed.md` 仍写着「不合 main」「唯一待办：…推到新分支 wb/z5-closed」，而且第 6 节授权链只到 06:03，**没有 PR #9 合 main 的授权记录** | `docs/z5_closed.md:3、250、255`；第 6 节表格 | 草稿第 4 行把「Z5 已合入」当作前提，却没有引用合并授权，授权链在这里有一个未登记的节点（见 S1） |

---

## 二、问题 ① 硬约束是否被放宽：**有条件 ACCEPT**

〔自核〕逐条对照 Z5 硬边界（`z5_closed.md` 第 6–10 行）：

| Z5 硬边界 | 草稿 | 结论 |
|---|---|---|
| composition.h 冻结；L1 合同 `77416e8b…3a0f`；公开面、fm_*、E1 不改 | 第 17 行 | 保留。但 F5、F4 的票面写法与它冲突（见问题 ③） |
| 无 LOCK+APPROVED 不采数；PERF-CI HOLD | 第 18 行 | 保留。但「不采数」没定义，与 F1、F2、F6 字面冲突（M7） |
| **不改任何 *_LOCKED.md** | **缺失**；只有「已锁阈值不改」 | **漏项**（M3） |
| 不 force、不 amend、不 rebase | 第 19 行 | 保留 |
| 不合 main；合 main 须 pidan 口令 | 第 20 行 | 保留 |
| push 须许可 | 第 21 行 | 比 Z5 更严（逐次许可，转述不算），好 |
| 系统级改动先请示 | 第 22 行 | 保留。但 F9 可能需要的装 SDK、注册 MSIX、开开发者模式没有点名（S5） |
| Success≠adopt | 第 24 行 | 保留 |

另外两处会造成事实上的放宽：
- **第 5 行「唯一提示词」**：如果它是唯一依据，仓库里 `*_LOCKED.md`、`docs/api_v0_frozen.md`、`docs/z5_closed.md` 的约束就可能被 workbuddy 理解为「不在提示词里就不适用」。须补一条兜底（M3）。
- **没提开 PR、打 tag、发 release、删远端分支**：这些都是对外可见、接近合 main 的动作。按第 20、21 行字面并不禁止（M3）。
- 〔Gemini〕认为第 11 行「自己排期、实现」权力过大，缺对实现路径的预审。**部分采纳**：不加预审（pidan 说了全开），改为在第五节加「实现路径要碰第 17 行所列文件即停」（并入 M3）。
- 〔Gemini〕称 F7 允许修改 `MotionEtw.cs`。**不采纳**：F7 不改源文件，只是基于 T2 已合入的注释改动重打 pack（E5）。

## 三、问题 ② 「汇报由 workbuddy 自己决定」与授权链：**有条件 ACCEPT**

〔自核〕pidan 的原话「是否向你汇报由他自己决定」里，「你」是协调 bot。它放开的是**向协调汇报**这一条通道，并没有放开**向 pidan 请示**，也没有放开硬约束。草稿第 13–14 行本身没错，问题在于 Z5 授权链能闭合，靠的是 notion ai 在**两个会话**里逐条核实到 ① 级（`z5_closed.md` 第 6 节）。Z6 之后，workbuddy 可以只在自己和 pidan 的会话里请示，于是：
- 没有人负责核实「pidan 本人许可」。workbuddy 自己引用、自己执行，属于自证。
- 第 44 行要求逐字照录原话和时间，但**没有要求写拍板卡 id 或消息 id**，事后无法定位核验。
- 没有规定协调或评审方什么时候介入，结果就是 push 之后、合 main 之前都可能没人看过。
- 没有写明哪些消息不算 pidan 本人（agent 转述、协调卡片上的预填文字、仓库文档里的引用）。

**补法**（不违背 pidan 原话：进度汇报仍然自选；授权和留痕是硬性的）：
1. 授权人唯一：**pidan 本人**。判定口径沿用 `z5_closed.md` 第 6 节：用户本人消息，或用户对拍板卡的回答。
2. 凡属第五节的停点动作（push、合 main、开 PR、碰冻结面、系统级改动、采数或计时、LOCK），都必须拿到 pidan 本人逐次许可。许可的范围以原话为准，不外推到别的分支或别的票。
3. **留痕是硬性的**：REPORT 里写原话、AWST 时间、所在会话（workbuddy 或协调）、拍板卡 id（`tNNNsNNNN`）或消息 id（`tNNNu`）。
4. 进度要不要告诉协调，由 workbuddy 自己定。但**每次 push 之后**，必须让分支上的 REPORT 包含上面的留痕，以便协调或 notion ai 随时只读核验，这不算「汇报」。**任何合 main 的请求**提交给 pidan 之前，必须先经协调组织的评审，三方中至少一方给出 ACCEPT，或有条件 ACCEPT 且条件已关闭。
5. 核验结论低于 ① 级（只有 workbuddy 转述）的许可，视为未授权；已经 push 的，由 pidan 决定是否保留。

〔Gemini〕建议「push 前在 pidan 面前发布含 diff stat 的意图声明」。**采纳**，并入 M2 第 3 点：拍板卡里写明分支名和 `git diff --stat`。

## 四、问题 ③ 各票验收与冻结面：**有条件 ACCEPT**

| 票 | 可检查 | 误触冻结面风险 | 结论 |
|---|---|---|---|
| F7 | 否：「SHA256SUMS 与产物一致」是自证（重算后必然一致）；「与 T3 Release 一致」字面做不到（E6）；「在 main 上重打」与第 19、20 行「独立分支、不合 main」字面冲突 | 低（只要不改源）；但如果 F4 或 F8 后来改了 pack 源，新 pack 又会失效 | **须改**（M4） |
| F11 | 基本可检查；但它依赖 F7 的新 SUMS，没写清楚；侧叉不入库，交付物是什么也没写；「故意改坏 DLL」要在副本上做，做完复原 | 无 | 建议改（S3） |
| F3 | 可检查，但应给出文件白名单 | 无（白名单内） | 建议改（S2） |
| F1 | 可检查（日志里能看到两个持有者重叠、Retarget 起点、最终值 = 后写者目标） | 无 | 小改（S2） |
| F2 | 「不泄漏句柄」如果靠采样句柄数，就是计时或采数口径，还会和第 18 行冲突 | 无 | 建议改（S2） |
| **F5** | 「切换系统设置后行为跟着变」需要改系统设置，本身就是第 22 行的停点，写法却是「如果要改」 | **高**：E1–E3。标成「L1 内部」会引导 workbuddy 改 `MotionSystem.cs`、`fm_capi.cpp` 或 `controller.h` | **须改**（M1） |
| F6 | 一张截图证明不了「动画可见」 | 无（宿主侧） | 建议改（S2） |
| **F4** | 指引部分可检查；可选代码部分只写了「公开面不变」，没写 E1 不变、放在哪个文件、能不能加事件 | **中**：落在 `MotionEtw.cs` 就是 E1 定义文件；新增事件或 Keyword 就是 E1 变更；任何 L1 源改动都会让 F7 的 pack 失效；写进 `docs/api_v0_frozen.md` 属于改冻结文档 | **须改**（M5） |
| F9 | 「先交基线报告」可检查；但 Files 的构建和运行很可能要装 SDK 或 workload、注册 MSIX、开开发者模式，草稿没有点名这是停点 | 无（侧叉只在本地） | 建议改（S5） |
| F8 | 见问题 ④ | — | **须改**（M6） |

〔Gemini〕F5 应限定在宿主侧：与自核一致，但这条不是独立佐证（见文首说明）。〔Gemini〕F4 限定为「只加 Debug 断言，不加成员或导出符号」：**采纳**，并入 M5。

## 五、问题 ④ F8 边界：**有条件 ACCEPT**

〔自核〕第 38 行「只起草闸门 DRAFT…之前不采数、不计时」方向正确，但还缺几样：
1. **「起草」包括什么**：能不能写探针代码？能不能编译？能不能「试跑一下看工具通不通」？试跑就是采数。
2. **「之前」指什么之前**：要写明是「LOCK **和** 该票的 APPROVED 都到位之前」。
3. **LOCKED 文件由谁落盘**：workbuddy 不能自己把 DRAFT 改名或复制成 `*_LOCKED.md`。
4. **与 PERF-CI 的关系**：F8 拿到 APPROVED 不等于 PERF-CI 解除 HOLD；也不得改 `docs/m2_perf_ci_gate_LOCKED.md` 等已锁文件。
5. **与 E1 的关系**：「真实 PresentStats」就是要让 E1 事件 id 4 输出实测值。E1 的名字、id、字段、GUID 一律不变，只换值的来源；值的语义变化（占位 → 实测）须登记进 `docs/api_changelog.md`，而且只能在 LOCK 之后做。改 `MotionEtw.cs` 或 `MotionSystem.cs` 也会让 pack 失效。
6. **已有数据**：T6 和 Z4 的旧日志可以当背景引用，但**不能**当 DRAFT 的通过依据或阈值来源。
7. 绑定提交：含 T3 的 tip（`7e2feb0e` 或之后 pidan 认可的 main）。

〔Gemini〕认为在 APPROVED 之前应禁止写 `QueryPerformanceCounter` 或 `PresentStats` 相关代码。**采纳并加严**：不只是不写，未 LOCK 之前探针代码一律不进分支。

---

## 六、修改建议（原文位置 + 替换文字）

### 必改（M1–M7，定稿前必须落实）

**M1｜F5 改为宿主侧｜第三节票单 第 34 行**〔自核；Gemini 同向〕
- 原文：
  `| F5 | ReducedMotion 跟随运行中系统设置的变化（现在只在启动时读一次） | L1 内部 | 公开面不变；切换系统设置后行为跟着变。如果要改系统设置，先请示 pidan |`
- 替换为：
  `| F5 | ReducedMotion 跟随运行中系统设置的变化。现在由 FluentHub 宿主侧叉在启动时读一次 UISettings.AnimationsEnabled，再调 SetReducedMotion（见 pilot/fluenthub/T5_REPORT.md 第 35 行）。只在宿主侧订阅设置变化，然后调用已有的公开方法 MotionSystem.SetReducedMotion(bool)；不改 MotionSoftDemo/*.cs、code/**、pilot/l1pack/** | 宿主侧（本地侧叉） | git diff --name-only 7e2feb0e..分支 里没有 MotionSoftDemo/、code/、pilot/l1pack/ 下的文件；运行日志里能看到设置切换后 SetReducedMotion 的入参跟着变，切换后新 Play 的动画行为也跟着变（不计时）。切换系统动画设置属于系统级改动，验收前必须先请示 pidan，测完恢复原值并在 REPORT 里记录恢复结果 |`

**M2｜补授权链｜第一节 第 13–14 行之后（第 14 行末尾）新增一段**〔自核；第 3 点采纳 Gemini〕
- 在第 14 行后插入：
  ```
  汇报方式自选，不影响第二节的任何一条，也不影响第五节的停点。授权按下面的规则：
  1. 唯一授权人是 pidan 本人：用户本人消息，或用户对拍板卡的回答。agent 转述、协调卡片上的预填文字、仓库文档里的引用都不算。
  2. 第五节列出的每一类动作（push、开 PR、合 main、碰冻结面、系统级改动、采数或计时、LOCK）都要逐次拿到许可。许可的范围以原话为准，不外推到别的分支或别的票。
  3. 请示 push 的拍板卡里写明分支名和 git diff --stat 摘要。
  4. 每次 push 后，分支上的 z6/<票号>/REPORT.md 必须写明：许可原话（逐字）、AWST 时间、所在会话、拍板卡 id（tNNNsNNNN）或消息 id（tNNNu），供协调或 notion ai 只读核验。这一步不算「汇报」，必须做。
  5. 向 pidan 请求合 main 之前，必须先经协调组织的评审，至少一方给出 ACCEPT，或有条件 ACCEPT 且条件已关闭。
  6. 核验不到 pidan 本人的许可，按未授权处理。
  ```

**M3｜补硬约束漏项｜第二节，第 23 行之后新增第 9、10 条；同时改第 5 行**〔自核〕
- 第二节新增：
  ```
  9. 不改、不新建、不改名任何 *_LOCKED.md；LOCKED 文件只能在 pidan LOCK 之后由协调落盘。docs/api_v0_frozen.md、L1 合同文件不改；要改，按碰冻结面处理，先停。
  10. 不开 PR，不打 tag，不发 release，不删远端分支；需要时按 push 同等处理，先拿 pidan 本人许可。
  ```
- 第 5 行原文：`> 本文定稿后原样发给 workbuddy cli，作为它接手 Z6 开发的唯一提示词。`
- 替换为：`> 本文定稿后原样发给 workbuddy cli，作为它接手 Z6 开发的提示词。仓库内 *_LOCKED.md、docs/api_v0_frozen.md、docs/z5_closed.md 的约束继续有效，与本文冲突时以更严的一方为准。`
- 第五节第 48 行开头「构建或测试失败修不好、必须动冻结面、」改为：
  `构建或测试失败修不好、实现路径需要改 code/composition.h、code/capi/fm_capi.h/.cpp、MotionSoftDemo/MotionSystem.cs 的公开成员或 MotionSoftDemo/MotionEtw.cs、需要改 *_LOCKED.md、`

**M4｜F7 验收改为可检查｜第三节 第 29 行**〔自核〕
- 原文：`| F7 | 合 main 后的整合收尾：在 main 上重打 L1 pack，…新 pack 绑定 \`7e2feb0e\` | 打包 | 构建 0 错误；SHA256SUMS 与产物一致；公开面 dump 与 T3 Release 一致 |`
- 替换为：
  `| F7 | 合 main 后的整合收尾：在分支 wb/z6-f7（从 main 7e2feb0e 拉出，不是直接改 main）上用 Release 重打 L1 pack，重写 pilot/l1pack/SHA256SUMS 和 README（mf_tip 改为 7e2feb0e）；不改任何源文件 | 打包 | ① git diff --name-only 7e2feb0e..分支 只有 pilot/l1pack/SHA256SUMS、pilot/l1pack/README.md、z6/f7/**；② SHA256SUMS 里的 git hash-object 行：MotionSystem.cs = fe72b08f…、MotionEtw.cs = 0404498a…，与 7e2feb0e 一致；③ 对新 FluidMotion.L1.dll 跑 CheckSurface，原始输出入库，含「CHECKSURFACE fails=0」；④ 公开面 dump 与 pilot/fluenthub/surface_l1pack.txt 相比，只允许首行不同（与 T3 Release 的 surface_ds_Release.txt 相比，只多出 Z3aL1DevSmoke 类型）；⑤ 产物 sha256 由独立命令（certutil 或 Get-FileHash）再算一遍，结果入库。之后如果有任何票改了 pack 源（MotionSystem.cs、MotionEtw.cs、fm_capi.cpp），F7 作废，须重做 |`

**M5｜F4 可选代码收紧｜第三节 第 36 行**〔自核；采纳 Gemini〕
- 原文：`| F4 | …可选在 L1 侧检测到 EventSource 关闭时给 Debug 提示 | 文档、可选代码 | 指引写清 \`EventSourceSupport=true\`；如果加了检测，公开面不变 |`
- 替换为：
  `| F4 | AOT 宿主关闭 EventSource（已知限制 K-AOT-ETW）：新建 docs/z6_aot_host_guide.md，写宿主侧接入指引；可选：在 MotionSoftDemo/MotionSystem.cs 内部加一个 #if DEBUG 的 Debug.WriteLine 提示 | 文档、可选代码 | 指引写清 <EventSourceSupport>true</EventSourceSupport>，并引用 T5 run1/run2 证据；不改 docs/api_v0_frozen.md。可选代码：不改 MotionEtw.cs；不新增 public/protected 成员、事件、Keyword、导出符号；Release 构建产物的公开面 dump 与 surface_l1pack.txt 相比只允许首行不同；完成后 F7 作废，须重做 |`

**M6｜F8 边界写清楚｜第三节 第 38 行**〔自核；部分采纳 Gemini〕
- 原文：`| F8 | 真实 PresentStats；K93 约 32Hz 的计时探针 | **须 LOCK** | 你只起草闸门 DRAFT，交协调组织评审并请 pidan LOCK 和 APPROVED。之前不采数、不计时 |`
- 替换为：
  `| F8 | 真实 PresentStats；K93 约 32Hz 的计时探针 | **须 LOCK** | 在 pidan LOCK 并说出「APPROVED F8」之前，只允许在分支上新建 docs/z6_f8_gate_DRAFT.md，不写、不编译、不运行任何探针或计时代码，不采数，不跑工具试跑。DRAFT 绑定 7e2feb0e（或之后 pidan 认可的 main）；T6、Z4 的旧日志只能当背景引用，不作阈值来源或通过依据。LOCKED 文件由协调在 pidan LOCK 后落盘，你不自建。实现时 E1（名字、id、字段、GUID）一律不变，只把事件 id 4 的值由占位改为实测，并登记进 docs/api_changelog.md；改 pack 源后 F7 须重做。F8 的 APPROVED 不解除 PERF-CI HOLD，也不改任何已锁闸门 |`

**M7｜定义「不采数」，消除与 F1、F2、F6 的冲突｜第二节 第 18 行末尾追加**〔自核〕
- 在第 18 行末尾追加：
  `本条所说的「采数、计时」，指产出用于判定、闸门或性能结论的数值。F1、F2、F6 这类 dev-only 运行证据（ETW 事件名和顺序、配对计数、日志、截图、录屏）可以做，但报告里不得出现任何计时判据或帧率结论，也不计任何闸门会话。`

### 建议改（S 项，不阻塞定稿）

- **S1｜第 4 行**〔自核〕：「基线」一行后补上 PR #9 合 main 所依据的 pidan 原话（时间、会话、卡片 id）。如果协调核验不到，就写「合并由 pidan 本人在 GitHub 完成（06:23:57 AWST），授权记录待补」。另外，main 上 `docs/z5_closed.md` 仍写着「不合 main」「唯一待办」（E8），建议另开一张小文档票补一句合并后的状态，或者在提示词里注明「z5_closed 第 8 节的『不合 main』『唯一待办』已被 PR #9 取代」，免得 workbuddy 误读。
- **S2｜第 31–33、35 行**〔自核〕：
  - F3（第 31 行）验收改为「`git diff --name-only` 只有 `code/capi/fm_capi_test.cpp` 和 `z6/f3/**`；Debug、Release 的测试输出原文入库」。
  - F1（第 32 行）验收末尾加「日志里能看到最终值等于后写者的目标」。
  - F2（第 33 行）「不泄漏句柄」改为「ETW 里 AnimationLifecycle 的 Created 和 Released 数量相等，Dispose 后能看到 IdleTransition（activeCount=0, pumpOn=0）；不采样进程句柄数」。
  - F6（第 35 行）「截图或录屏能证明入场动画可见」改为「录屏（只录当前窗口），或至少两张中间帧截图；外加日志证明首帧回调先于 Play；不计时」。
- **S3｜第 30 行 F11**〔自核〕：验收里加三点：校验用的 sha256 来自 F7 新的 SHA256SUMS（F11 排在 F7 之后）；侧叉改动以 patch 文件放进 `z6/f11/`，不把 FluentHub 源码入库；「故意改坏」只在 DLL 副本上做，做完复原并记录。
- **S4｜第 19 行**〔自核〕：分支名统一用小写 `wb/z6-<票号小写>`（例如 `wb/z6-f7`）。允许叠在另一张 Z6 票的分支上拉出（例如 F11 叠在 F7 上），但要在 REPORT 里写明父提交。
- **S5｜第 37 行 F9**〔自核〕：末尾加「Files 的构建和运行如果需要装 SDK 或 workload、注册 MSIX、开开发者模式、提权，都属于第二节第 6 条的系统级改动，先停；源码克隆到 `C:\Users\admin\pilot\Files`，不入库」。
- **S6｜第 40 行顺序**〔自核〕：改为「F7 → F11 → F3 → F1/F2 → F5/F6 → F4 → F9；F4 如果加了可选代码，完成后重做 F7（若 F11 已完成，一并复核）」。

---

## 七、来源汇总

- **自核**：E1–E8 全部事实；M1–M7、S1–S6 的位置和替换文字；对四个问题的判定。
- **Gemini CLI**（`/workspace/z6review/gemini_out.txt`，一次调用成功）：
  - 采纳：push 前的意图声明（并入 M2 第 3 点）；F4 只加 Debug 提示，不加成员或导出符号（并入 M5）；F8 在 APPROVED 之前禁止计时代码（并入 M6，已加严）。
  - 部分采纳：第 11 行权力过大（不加预审，改为加停点，并入 M3）。
  - 不采纳：「F7 允许修改 MotionEtw.cs」，与事实不符（E5）。
  - 同向但不算独立佐证：F5 改为宿主侧（提示里已附带我核到的事实）。
