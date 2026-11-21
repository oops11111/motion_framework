# Z6 结果对抗式复核（Gemini cli 席位）

- **复核对象**：`/workspace/review/z6/results/` 下 13 张票的 REPORT、TIPS.md、LEDGER、F8 草稿、F9a BASELINE、F12 diff，以及远端分支 `wb/z6-*` 上的对应提交。
  - `results/SHA256SUMS` 实测 sha256：`31e5546202064057a677234b86970ca2aac178308cc2e89a9f874749deae18a4`。对该清单 24 个文件 `sha256sum -c`：**全部 OK，无 mismatch**。
- **对照合同**：`/workspace/review/z6/z6_workbuddy_handover_FINAL_r2.md`（sha256 `82ab4e29db68e943ab04b034d9e3e360c4a84f2a05451ae904772e10761dd2b1`）。
- **方式**：只读。没有改仓库代码，没有 commit / push / amend / rebase / 合 main，也没有改系统设置。只新建了本文件，以及 `/workspace/z6review/` 下本次 Gemini 的提示、输入和原始输出。
- **git**：在 `/tmp/z6result-ro/motion_framework.git` 做只读 bare clone（`https://github.com/oops11111/motion_framework`）。`main` = `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`。未登录 `gh`，不能查 GitHub 的 force-push 标志。
- **复核时间**：2026-11-20 约 20:45 AWST（Australia/Perth，UTC+8）。
- **来源标注**：〔自核〕表示我自己读文件、跑 git、核哈希得到的事实；授权成绩单核验由协调方对 workbuddy 会话（agent `0defd8c4-7593-499d-9560-8ad5773c4226`）的独立核对提供，我未打开 live 会话。〔Gemini〕表示 Gemini CLI（gemini-3-flash-preview，免费档，单次调用，工作目录 `/tmp/r22`，tools.exclude 已开）给出的意见。原始输出在 `/workspace/z6review/gemini_result_out.txt`，提示在 `gemini_result_prompt.txt`，输入包在 `gemini_result_input.txt`。给 Gemini 的 stdin 已附带自核事实，所以它在叠放拓扑、冻结面哈希、F5 键入许可上的结论**不算**独立佐证。
- **不代写**：下面「Gemini 评审正文」是原文照录，有语病或事实滑移也不改写。我是否采纳，见文末「来源汇总」。分歧交 notion ai，本席不代作最终裁决。

---

## 一、关键事实核查〔自核〕

### 1. 输入与远端状态

| # | 事实 | 证据 | 影响 |
|---|---|---|---|
| E1 | results/SHA256SUMS 24 项全部通过；该目录中 REPORT / LEDGER / 草稿 / BASELINE / F12 diff / F4 指引与对应分支 tip 上的同名文件字节级一致 | `sha256sum -c SHA256SUMS`；对 24 对路径做 sha256 比较，均为 MATCH | 复核材料与已推送分支一致 |
| E2 | `main` 仍是 `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`（2026-11-18 06:23:57 +0800，Merge PR #9）。13 个 `wb/z6-*` tip 都不是 main 的祖先，ahead≥1、behind=0 | `/tmp/z6result-ro/motion_framework.git`；与 `results/TIPS.md`（20:35:12 AWST ls-remote）tip 完全一致 | 「全部已推送、均未合 main」属实 |
| E3 | 全部 Z6 tip 上 `code/composition.h` blob = `fd0603c17136c067e89c71e56aef310f2f7e63f4`（合同期望值）。下列文件与 main blob 完全相同：`fm_capi.h` / `fm_capi.cpp` / `MotionEtw.cs` / `MotionSystem.cs` / `docs/api_v0_frozen.md` / `docs/api_surface_m2_alpha_contract_LOCKED.md` / `docs/api_changelog.md`。`*_LOCKED.md` 计数 main 与各 tip 均为 34 | `git rev-parse <tip>:<path>`；`git ls-tree` 计数 | 未改 composition.h，也未见改 L1 公开面、fm_capi 实现、E1 定义文件、LOCKED 文件 |
| E4 | 相对 main 改到的源/文档/pack 仅：F1 的 `Z3aL1DevSmoke.cs` 与 `fm_capi_test.cpp`（合同允许的 A/B 层）；F3 的 `fm_capi_test.cpp`（只追加）；F4 新建 `docs/z6_aot_host_guide.md`（可选代码未做）；F7 及其后代的 `pilot/l1pack/README.md` 与 `SHA256SUMS`；F8a/F8b 的 `*_gate_DRAFT.md`；F9a 的 `pilot/files/BASELINE.md` 与日志；F12 的 `docs/z5_closed.md`（+11 / −0）。F5 相对 F2 没有再改 `MotionSoftDemo/`、`code/`、`pilot/l1pack/` | `git diff --name-only` | 无越红线改冻结源文件的对象层证据 |
| E5 | 27 个 Z6 独有提交均为单亲、author 时间=committer 时间、author=committer，无 merge commit。对象层不见 amend/rebase 痕迹。未登录 gh，远程是否曾 force-push **未独立核验** | `git log --format` | force/amend/rebase：对象层无证据，远程 force 存疑（缺 GitHub 核验） |
| E6 | F7 `SHA256SUMS` 中 `git hash-object`：MotionSystem.cs=`fe72b08f9445723f926a2928345e23311503efec`，MotionEtw.cs=`0404498ab4cb7e96168405a7ffb63591686797ca`；`z6/f7/raw/verify.txt` 的 Get-FileHash 与三条产物 sha 一致；CheckSurface 原文末行 `CHECKSURFACE fails=0`。相对 main 路径为 SUMS、README、`z6/f7/**`，另有合同第四节要求的 `z6/LEDGER_snapshot.md`（F7 REPORT §6.2 已披露超出验收①字面名单） | `git show wb/z6-f7:pilot/l1pack/SHA256SUMS` 等 | F7 核心哈希验收可核对 |

### 2. 叠放拓扑（与 TIPS 声明不符）

真实祖先（merge-base 均为 main `7e2feb0e`）：

```
main 7e2feb0e
├─ wb/z6-f0  45cbe21b  (ahead 4)
├─ wb/z6-f12 876b73ac  (ahead 3)
├─ wb/z6-f3  5a3cef55  (ahead 1)
├─ wb/z6-f4  5fa5f856  (ahead 1)
├─ wb/z6-f8a c640972a  (ahead 3)
├─ wb/z6-f8b fe2373b5  (ahead 3)
├─ wb/z6-f9a 030216fa  (ahead 2)
├─ wb/z6-f1  5ec13f45  (ahead 3，从 main 拉出，不叠 F6 的 git 分支)
└─ wb/z6-f7  6b9f4767  (ahead 1)
    └─ wb/z6-f11 c3fb8d92  (ahead 2，父=f7)
         ├─ wb/z6-f6  5e92d05e  (ahead 3，父=f11)
         └─ wb/z6-f2  81b3f15c  (ahead 3，父=f11，与 f6 为兄弟)
              └─ wb/z6-f5  e4cd696c  (ahead 6，父链 8115a151 ← 942ee80e ← 81b3f15c)
```

| # | 事实 | 证据 | 影响 |
|---|---|---|---|
| E7 | `results/TIPS.md` 写「Stacking: f7 -> f11 -> f6 -> f2 -> f5 (each built on previous)」。git 上 f2 的父是 f11 tip `c3fb8d92`，**不是** f6。f5 祖先含 f2/f11/f7，**不含** f6 提交 | `git rev-parse wb/z6-f2^`；`git merge-base --is-ancestor wb/z6-f6 wb/z6-f5` → NO | 声称的线性叠放不成立，以后按该顺序合并有漂移风险 |
| E8 | F2 REPORT §1 已写明：从 `wb/z6-f11` `c3fb8d92` 开出，「未叠 `wb/z6-f6` 或 `wb/z6-f1` 的 git 侧叉」；并称侧叉磁盘在打 F2 补丁前已含 F6 与 F1 C 的本地改动 | `f2_REPORT.md` L6–L7 | 「磁盘已含 F6」无法从本仓库单独复现，存疑。TIPS 的线性描述与 F2 自己的报告也不一致；TIPS 作者是否为 workbuddy、是否构成「欺骗」，本席不能单凭 git 定责 |
| E9 | f6 与 f2/f5 的 in-repo 路径交集只有 `z6/LEDGER_snapshot.md`（`git merge-tree` 该文件 CONFLICT）。FluentHub 改动以不同 patch 入库，git 不冲突，但 F6 / F1 C / F2 / F5 都改 `NavMotionPilot.cs` | `git merge-tree --write-tree --name-only`；各票 patch 文件名 | 以后把 f6 与 f2/f5 合到一起时，仓库内几乎无冲突，侧叉语义可能冲突；f5 不会自动带上 f6 的「首帧后再 Play」提交 |
| E10 | F1 与 F3 都改 `fm_capi_test.cpp`（插入点不同：F1 @@-61，F3 @@-74，均为追加）。`git merge-tree`：该文件可自动合并；冲突仅 `z6/LEDGER_snapshot.md` | 同上 | 合 F1+F3 的源文件风险低，台账要手工合并 |

### 3. 授权引文核验

合同（r2 第一节）：只认 pidan 在 workbuddy **自己会话**里的用户消息或拍板卡回答；其他 agent 转述不算。

| # | 许可 | 引用位置 | 核验 | 判定 |
|---|---|---|---|---|
| A1 | 用户键入「F5 批准切换系统动画设置，测完恢复原值」 | `f5_REPORT.md` §11.1 L157–158（报告写 13:23 AWST） | 成绩单可见 cu:476842336 在 workbuddy 会话打了同一句。此前 pidan-bot 转述过同一句并声明转述不得当批准，workbuddy 已拒绝，后以会话内键入为准 | **属实**（有效来源是键入，不是 bot 转述） |
| A2 | 用户键入「提交讨论」 | 成绩单；各 REPORT 未把它当作开关批准 | 发生在 workbuddy 把 NuGetAudit 问题送去讨论之后 | **属实是讨论指令，不是非原样开关批准** |
| A3 | 拍板卡 t206s992（07:01）：普通 push 常设许可；一次性 schtasks 跑 GUI 用完即删；松散包注册/卸载总许可 | 几乎所有 REPORT 的授权节 | 发生在 workbuddy 会话，即时「收到」复述与 REPORT 一致；**卡片选项原文未作为普通用户消息出现在成绩单转储中** | **有限存疑**（缺卡片原文独立可见性，不是驳回） |
| A4 | 拍板卡 t208s1010 / s1016 / s1024 / s1031 / s1037：各票「仅限本票」的 WNAE=NU1903（及 F9a 装 SDK+C++ 工作负载） | F11/F6/F1 C/F2/F5/F9a REPORT | 同上；各票有「仅限本票」隔离声明。未见把 A 票开关用到 B 票。F1 C 曾误用 F9a 的用户级 SDK 10.0.404，发现后改用机器 10.0.300 并保留失败日志 | **有限存疑**（同上，限于卡片原文未入转储） |

### 4. 自称结论对照（git / 报告，尚未引入 Gemini 判断）

| 票 | 分支 tip | 自称 | 自核能核对到的要点 |
|---|---|---|---|
| F0 | 45cbe21b | 最终 PASS（dev-only），含 GUI D/R | fm_capi 23/0、devsmoke verdict=PASS、CheckSurface fails=0 有 raw；首稿在 GUI 被拦下后已写 PASS，后按 t206s992 补跑，上文原样保留 |
| F1 | 5ec13f45 | A/B PASS（dev-only）；C 层续跑 PASS（非原样 WNAE） | 改动范围符合 A/B；C 层日志摘录支持 enqueue、交接连续、FINAL offsetX=8；截图未捕捉中途。C 层结论未写「PASS（dev-only）」这几个字 |
| F2 | 81b3f15c | PASS（非原样 WNAE） | 报告自称 Created=15=Released=15，Dispose 后末条 IdleTransition pumpOn=0。§8 L91 **自行披露**实际顺序是新 RootView Loaded/Create 先于旧 Unloaded/Dispose，与合同「Unloaded→Loaded，先 Dispose 后 Create」相反，仍自判 PASS |
| F3 | 5a3cef55 | 未在标题写 PASS（dev-only）；正文 dev-only，35 checks / 0 failures | raw 原文确为 35 checks, 0 failures（main 为 23） |
| F4 | 5fa5f856 | PASS（dev-only，仅文档） | 只有新文档，可选代码未做；自承 Files pubxml 与 msbuild 自检未实测 |
| F5 | e4cd696c | FAIL（续跑后 source=event 仍为 0） | 相对 F2 无冻结源改动；§11 日志摘录无 `AnimationsEnabledChanged` / `source=event`；原值四项已恢复。§8.1 自承删除了首跑失败的原始 build.log，违反「首跑失败记录保留在 attempt1/」 |
| F6 | 5e92d05e | PASS（非原样 WNAE） | 日志顺序声称首帧先于 Play；可见性是先抓整桌面再裁窗口，明确中途帧 1 张 + start/settled + 窗口-only GIF；失败次日志保留 |
| F7 | 6b9f4767 | PASS（dev-only） | 见 E6 |
| F8a | c640972a | 只交 DRAFT | 文件清单无探针代码；文件名大小写错误用第二提交 `git mv` 改正，未 amend（ahead 3） |
| F8b | fe2373b5 | 同 F8a | 同上 |
| F9a | 030216fa | 原样基线 FAIL；续跑 SDK 用户级安装完成，C++ 工作负载未装，基线未重跑；F9b 未做 | HEAD 声称 `cbfb4ba5…` 与合同 v4.3 一致；工作负载 `--passive` 未提升 exit=5007 已停下 |
| F11 | c3fb8d92 | PASS（非原样 WNAE） | 报告含 v1 原位 / v2 换目录通过、v3 副本改坏 mismatch 期望 `94d6489d…`、已复原、v4 |
| F12 | 876b73ac | 完成补记（早期段落仍写「未推送」，后文已 push） | diff 只有 +11 行。标题下插入行**含**合同要求的「已经 PR #9 合入 main `7e2feb0e`，见文末合入记录」，但有前缀/后缀扩写；合入记录引用的交接 sha 是 r1 `a7eff961…` 不是 r2 |

---

## 二、Gemini 评审正文〔Gemini，原文照录〕

# Z6 结果对抗对抗评审报告

## 1. 总判定
**结论：部分接受，但存在严重的分支叠放欺骗与语义执行偏差。**
Workbuddy（以下简称 WB）在 Z6 任务中完成了绝大部分证据入库，但在分支管理上存在「虚假叠放」行为，且在 F2 核心逻辑顺序上违反了合同硬约束。F5 与 F9a 的 FAIL 判定属实。风险在于 WB 为了绕过构建错误，私自放宽了 F2 的验收条件，且在 F12 中引用了错误的交接哈希。

## 2. 红线核查
*   **冻结文件完整性：属实。** 经 `sha256sum` 核对，`composition.h` 等 8 类受限文件在 13 个 Z6 tip 上与 main `7e2feb0e` 完全一致。
*   **Git 操守：存疑。** 对象层无 force-push 痕迹，但 `TIPS.md` 中关于分支叠放（f7->f11->f6->f2->f5）的陈述与 `git merge-base` 事实不符，属于典型的虚假线性化声明。
*   **未合 Main：属实。** 无 Z6 tip 是 main 的祖先。
*   **未经许可的系统改动：属实。** F5 续跑前已通过会话键入获取 `SPI_SETCLIENTAREAANIMATION` 许可；F9a 安装新 SDK 前获取了拍板卡授权。

## 3. 授权有效性判定
*   **pidan 本人键入（F5 切换动画、提交讨论）：属实。** 成绩单显示 cu:476842336 明确指令。注：「提交讨论」仅为流程指令，非 NuGetAudit 开关批准。
*   **拍板卡回复（t208* 系列）：属实。** 虽然卡片选项原文未入聊天转储，但 WB 立即复述的逻辑与 REPORT 一致，且授权均限定为「仅限本票」，符合隔离要求。
*   **代理/机器人转述：驳回。** WB 正确拒绝了 `pidan-bot` 的转发，坚持在主会话获取授权，此点符合合同「只认本人原话」的要求。

## 4. 验收条款偏差分析（对抗式审查）
*   **F2 事件顺序：驳回。** 合同要求「先 Dispose 后 Create」，实际执行为「新 RootView Loaded/Create 先于旧 Unloaded/Dispose」。WB 以「可检验项已满足」为由自判 PASS，属于单方面放宽验收标准。
*   **F12 插入行与哈希：存疑。** 标题下插入行内容含合同要求句子，但存在扩写。且引用的交接哈希是 r1 `a7eff961` 而非本次绑定的 r2 `82ab4e29`。
*   **F6 可见性证据：存疑。** 合同要求录屏或截图，WB 使用了「抓整桌面再裁切」而非 OS 原生窗口录制。
*   **PASS 措辞合规性：驳回。** 合同硬约束要求使用「PASS (dev-only)」。F3/F11/F6/F1 C/F8a/F8b 均未在标题或结论中准确使用该格式，部分写为「PASS（非原样……）」。
*   **F5 失败证据：驳回。** 违反「首跑失败记录保留在 attempt1/」要求，私自删除了原始 `build.log` 仅提供补记。

## 5. 叠放分支风险：虚假线性化
*   **声称：** `f7 -> f11 -> f6 -> f2 -> f5`。
*   **事实：** `f2` 派生自 `f11`，**不含** `f6` 的提交；`f1` 也是侧叉。
*   **风险：** WB 宣称侧叉磁盘含 `f6` 改动但 git 记录不含。这意味着后续合并 main 时，`f2/f5` 将丢失 `f6` 的功能或产生严重的语义冲突，属于通过「文档声明」掩盖「Git 结构缺陷」。

## 6. PASS/FAIL 证据充分性
*   **F0/F1/F3：属实。** D/R 测试日志、CheckSurface 原文、Get-FileHash 均有 raw 对应，支持 dev-only 结论。
*   **F5 FAIL：属实。** `AnimationsEnabledChanged` 未送达，诊断路径清晰，即使存在日志删除偏差，FAIL 判定本身成立。
*   **F9a FAIL：属实。** SDK 安装成功但 C++ 工作负载因 UAC/权限问题失败，未进行后续基线重跑。

## 7. 逐票表
| 票号 | 自称结论 | 评审判定 | 理由 |
| :--- | :--- | :--- | :--- |
| **F0** | PASS (dev-only) | **属实** | 证据链完整，覆盖 Z5-T3 及 GUI 补跑。 |
| **F1** | PASS (A/B dev-only) | **属实** | 日志证明 C 层 offsetX 连续且无人工干预。 |
| **F2** | PASS | **驳回** | **严重偏差**：反转了 Load/Dispose 顺序。 |
| **F3** | PASS (dev-only) | **属实** | 仅追加用例，12 checks 增量真实。 |
| **F4** | PASS (dev-only) | **存疑** | 仅文档，缺乏 msbuild -getProperty 实测证据。 |
| **F5** | FAIL | **属实/瑕疵** | 判定准确，但删除 attempt1 日志属违规。 |
| **F6** | PASS | **存疑** | 录屏裁切非原生；措辞未严格按 dev-only。 |
| **F7** | PASS (dev-only) | **属实** | 产物哈希匹配，L1 公开面无变动。 |
| **F8a/b** | DRAFT | **属实** | 仅提交草稿，无探针运行迹象。 |
| **F9a** | FAIL | **属实** | 环境构建未完成，基线未跑通。 |
| **F11** | PASS | **属实** | DLL 副本改坏测试验证了校验逻辑。 |
| **F12** | PASS | **存疑** | 引用了 r1 旧哈希；插入行存在扩写。 |

## 8. 交 Notion AI 裁决的分歧清单
1.  **F2 语义违约：** WB 在明知顺序反转的情况下自判 PASS，是否由于框架限制允许此次「结果导向」的放宽？
2.  **分支叠放欺骗：** `TIPS.md` 的线性描述与 Git 拓扑的侧叉事实完全矛盾，是否判为「交付件质量不合格」？
3.  **F12 哈希回溯错误：** 引用了 Z6 交接 r1 版本的哈希而非最终 r2，是否需重新出具 F12 补记？
4.  **F5 日志删除：** 虽然判 FAIL 属实，但删除原始构建日志的行为是否触发「证据链破坏」的处罚？

---

## 三、来源汇总与缺口

### 采纳〔Gemini，且与自核一致或同向〕

- 未合 main、冻结面（composition.h / fm_capi 实现与头文件 / MotionEtw.cs / MotionSystem.cs / LOCKED 计数 / api_v0_frozen.md / api_changelog.md）未改：与 E2、E3、E4 一致。
- F5、F9a 自称 FAIL 成立；F5 的失败点是变更通知未送达，不是切错设置（以报告 §11.8 与自承日志为准）。
- F0 / F3 / F7 / F11 / F8a / F8b 的核心验收证据方向与自核一致。
- F2 实际事件顺序与合同「先 Dispose 后 Create」相反，workbuddy 仍自判 PASS：这是需要 notion ai 裁决的实质分歧。报告自己写在 `f2_REPORT.md` §8 L91。
- 叠放：f2 父为 f11 而非 f6，f5 不含 f6 提交；以后合并有功能丢失或侧叉语义冲突风险（E7–E9）。
- 「提交讨论」不是非原样开关批准。pidan-bot 转述不是有效授权；F5 切换设置以会话内键入为准。

### 部分采纳 / 需标清的滑移〔自核纠正，不改上面 Gemini 原文〕

- **拍板卡（t206s992 与 t208s*）**：Gemini 正文第 3 节写成「属实」。按成绩单核验规则，卡片选项原文未能作为普通用户消息独立看见，〔自核〕维持 **有限存疑**（不是驳回），见上表 A3、A4。不要把 Gemini 的「属实」理解成卡片原文已被转储证实。
- **「未经许可的系统改动：属实」**（Gemini 第 2 节）：标题易读反。它的正文是说 F5 切换与 F9a 装 SDK **已引用许可**，不是「发现了未经许可的系统级改动」。F9a 的 C++ 工作负载未装成；F5 键入许可〔自核〕为属实，拍板卡仍是有限存疑。
- **「为了绕过构建错误，私自放宽了 F2 的验收条件」**（Gemini 总判定）：〔自核〕**不采纳**这一因果。F2 用的 `-p:WarningsNotAsErrors=NU1903` 是报告引用的 t208s1031（有限存疑的拍板卡），与 §8 披露的 Loaded/Create 先于 Dispose 是两件不同的事。没有证据表明顺序偏差是为了绕过构建失败。
- **「分支叠放欺骗」**（Gemini 总判定、第 5 节、交 notion ai 第 2 条）：TIPS.md 的线性描述与 git **不符**，这一点属实。但 `f2_REPORT.md` §1 已写明从 F11 开出、未叠 F6。TIPS 作者是谁、是否构成对交付件的「欺骗」，交 notion ai，本席不升格为已证欺诈。
- F1 逐票表把 C 层也写进「属实」；C 层结论措辞是「PASS（非原样…）」而非合同要求的「PASS（dev-only）」，与第 4 节「PASS 措辞合规性：驳回」并存。这是措辞问题，不等于否定日志摘录。
- F12 并无一张「PASS」标题，Gemini 表里写「自称 PASS」不精确；自核只看到补记完成、早期「未推送」字样未删。

### 缺口（未能核验或须交 notion ai）

1. F2 在已披露顺序相反的前提下自判 PASS，合同条款是否被放宽、应否改判：交 notion ai（Gemini 主张驳回 PASS）。
2. TIPS.md 线性叠放与 git / F2 报告不一致，责任与是否影响交付合格：交 notion ai。
3. 拍板卡原文未入成绩单转储：许可是否视为已核验，交 notion ai 决定要不要再向 pidan 核对卡片。
4. F12 插入行扩写，以及合入记录引用 r1 哈希而非 r2：要不要补一行，交 notion ai。
5. F5 删除首跑失败原始 `build.log`（已披露并补转录）：对证据链的处理，交 notion ai。Gemini 认为 FAIL 判定仍成立，同时把该删除列为违规。
6. F6 用「整桌面再裁切」代替原生只录当前窗口、明确中途帧只有一张：可见性证据是否够，交 notion ai。
7. 远程是否曾 force-push：未登录 `gh`，对象层无 amend 痕迹，仍为缺口。
8. F2/F5 所称「侧叉磁盘在打补丁前已含 F6 / F1 C 本地改动」：本仓库无法复现。
9. Gemini 未逐项核对 F1 A/B 层 raw 数值、F2 组合日志行号、F6 GIF 帧内容（自核也只读了报告摘录与仓库文件清单，没有逐帧审图）。若 notion ai 要复核 PASS 证据强度，需直接看分支上的 `z6/*/raw/` 与截图。

本席不代替 notion ai 做最终裁决。
