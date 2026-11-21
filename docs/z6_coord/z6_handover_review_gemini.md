# Z6 交接提示词评审（Gemini网页版 · 三方讨论之一）

- **评审对象**：`/workspace/review/z6/z6_workbuddy_handover_DRAFT.md`（DRAFT r0，48 行，5216 字节）
- **哈希核对**：实际 sha256 = `c95b27d08421a1ac1de054de08fbea787daa78f5f2f91f43bbbd357231650251`，和协调给的前缀 `c95b27d0…` 和后缀 `…0251` **一致**。
- **评审人**：Gemini网页版 bot（为 pidan 撰写），2026-11-18 AWST（Australia/Perth，UTC+8）。
- **依据**：pidan 2026-11-18 06:30 AWST 原话（由协调 bot 转述，本评审未独立核实原文）：「后续票全开，任务交workbuddy / 提示词三方讨论后让它接手开发 / 是否向你汇报由他自己决定」。
- **我核对过的材料**（只读）：
  - `/workspace/review/z5/z5_closed.md`：第 3、4、5、6、9、10 节。
  - `/workspace/mf_main_ro`：`git fetch` 后 detached 检出 `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`，只读，没有推送。
  - `/workspace/fluenthub_ro`：`eb08f0d`。
  - files-community/Files：GitHub API、`git ls-remote`、raw 文件。
  - 我之前写的 `/workspace/motion_framework/docs/_out/z5_dev_plan_gemini_refine_20261118.md`。
- **我实际跑过的**：在盒子上（Linux，zig 0.17.1 自带 clang，`-fsanitize=address,undefined`）编译并运行了下文 F1-A、F3 两个**提议中的新测试文件**，原有的 `fm_capi_test` 也重跑了一遍（23/0）。这些只在盒子 `/tmp` 里，不入库，**不代表 K93 MSVC 的结果**。K93 上的 PowerShell 命令都没有实际跑过。
- **只新建本文件**，没有改动其他任何文件。

---

## 0. 总结论：**有条件 ACCEPT**

草稿方向对，硬约束主体完整，可以作为 workbuddy 接手 Z6 的基础。但有 8 个问题要先改，否则 workbuddy 会被卡住，或者交出没法复核的结果。下列条件全部落实后可以定稿发出：

| # | 条件（每条一句） | 对应修改建议 |
|---|---|---|
| **C1** | 每次运行 FluentHub 或 Files 都要**松散注册包**，草稿把这件事列为须请示的系统级改动，却没有安排**事先的统一许可**。不补这一条，F1、F2、F5、F6、F11、F9 的运行部分全部会卡住。要在定稿前请 pidan 用拍板卡给一次 Z6 期间的总许可，或者写明逐次请示。 | M5、M9 |
| **C2** | **F5 归类错了**：「启动时只读一次」读的是宿主侧叉 `NavMotionPilot` 构造函数里的 `UISettings.AnimationsEnabled`，不在 L1 里。L1 只有 `SetReducedMotion(bool)`，而且是纯 net8.0，没有 WinRT。F5 要改成「宿主侧」，L1 保持不动。 | M12 |
| **C3** | **F1 要写明确定性的方法和分层验收**：A 层 C ABI 手动时钟，B 层 devsmoke `CreateManual`（internal），C 层宿主事件驱动触发。不允许靠人手悬停碰运气。 | M11 |
| **C4** | **F9 要钉死版本**：Files `v4.3`（`cbfb4ba54a89632fbceb82ea94ada60a9bae3737`），并且把已知的工具链缺口写进票里：C++ 子项目要 **v145（VS 2026）** 工具集，pubxml 里有 `PublishAot=true`。基线、侧叉分成两张票。 | M15 |
| **C5** | **顺序要调**：先做 L1 侧和测试侧（F3、F1-A、F4 只写文档），再做 F7 重打包，然后 F11，最后做宿主侧的叠加修改（F6 → F1-C → F2 → F5）。草稿顺序是 F7 在前；如果之后 F4 的可选代码动了 L1，就得再打一次包。 | M16 |
| **C6** | **补硬约束漏洞**：第三方仓库只本地、不提 PR、不推 fork、不登录账号；不改 `*_LOCKED*`；已有证据文件只能追加；非原样构建开关要 pidan 拍板；证书、计划任务、VS 组件、dotnet workload 都算系统级；FrameTick 只计数，不记 deltaUs。 | M3、M4、M5、M6、M7 |
| **C7** | **补最低留痕**，同时不推翻「是否向协调汇报由他自己决定」：K93 本地写一份只追加的台账；所有停止点和系统级动作都要在给 pidan 的请示里附台账摘录；没推送的结果一律标「未复核」。 | M2 |
| **C8** | **事实修正**：F7 的②（`api_changelog.md` 冲突）在 `7e2feb0e` 里已经解决，三行都在。main 上 `docs/z5_closed.md` 的文件头仍写「待落盘 / 不合 main」，属于结题时的状态，要在提示词里说明，避免 workbuddy 误读。 | M1、M10 |

没有 REJECT 理由。硬约束的骨架（composition.h、L1 合同、LOCK+APPROVED、PERF-CI HOLD、Success≠adopt、不合 main、不 force/amend/rebase、push 要 pidan 本人许可）都在，而且写得清楚。

---

## 1. 我核实到的事实（供三方对照）

| 事实 | 结果 | 证据 |
|---|---|---|
| main 当前 tip | `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`，即 "Merge pull request #9 from oops11111/wb/z5-merge"，tree `d61f0138`，时间 2026-11-18 06:23:57 AWST | `git fetch` 后 `git rev-parse origin/main` |
| `code/composition.h` blob | `fd0603c17136c067e89c71e56aef310f2f7e63f4`，和 Z5 记录相同 | `git rev-parse 7e2feb0e:code/composition.h` |
| `code/` 相对 2e57bff 有无变化 | **无**（PR #9 的 diff 只涉及 MotionSoftDemo/、docs/、pilot/、z3a/、z5/） | `git diff --stat 2e57bff 7e2feb0e` |
| `MotionSystem.cs` blob | `fe72b08f…`，和 pack SHA256SUMS 记录**一致** | 同上 |
| `MotionEtw.cs` blob | `0404498a…`；pack SHA256SUMS 记的是 `565111f0…`，**不一致**（T2 只加了注释） | `pilot/l1pack/SHA256SUMS` |
| `docs/api_changelog.md` | T2 的两行和 T3 的一行在 main 上**都在**，所以 F7 的②已经解决 | `git diff 2e57bff 7e2feb0e -- docs/api_changelog.md` |
| main 上的 `docs/z5_closed.md` | 和 `/workspace/review/z5/z5_closed.md` **逐字相同**；文件头仍写「定稿（待落盘）… 不合 main」 | `diff` 输出为空 |
| 合 main 的授权原话 | `z5_closed.md` 第 6 节只核到 06:03，没有合并口令的原文 | **待核实**（建议在 Z6 提示词里补引） |
| L1 有无系统 ReducedMotion 读取 | **无**。全仓（pilot/ 之外）grep `UISettings`、`AnimationsEnabled` 都没有命中；读取只在 FluentHub 侧叉 `NavMotionPilot` 构造函数里 | `fluenthub_t5_side_fork.patch` |
| L1 的测试时钟 | `MotionSystem.CreateManual()` 和 `Advance(double)` 都是 **internal**；C ABI 有 `fm_create_manual` 和 `fm_manual_advance` | `MotionSystem.cs:116-117, 187-193`；`fm_capi.h:24-25` |
| 仲裁语义 | `controller.h:610-614`：Retarget 先从别的持有者 `owner->Advance(); cur = owner->ValueOf(prop)` 取种子值，再认领属性，上一个持有者随即失去该属性 | 源码 |
| FluentHub RootView 重建入口 | 已有现成路径 `MainWindow.InitializeApplication(null, forceReload: true)`，会 new 一个新的 RootView 并替换 Content | `fluenthub_ro/src/FluentHub/Views/MainWindow.cs:37-47` |
| FluentHub 运行是否必须打包 | **是**。MainWindow 构造里调用了 `Windows.ApplicationModel.Package.Current.InstalledLocation`，没有包身份会抛异常 | `MainWindow.cs:25` |

**盒子上的探针结果**（clang + ASan/UBSan，没有 MSVC）：
- `fm_capi_test` 原样：`demo.enter: 69 frames (575 ms)`，**23 checks, 0 failures**。
- F3 提议文件：**6 checks, 0 failures**。
- F1-A 提议文件：**8 checks, 0 failures**。
  - 入场中途：`offsetX=-57.123109 vel=672.792543 opacity=0.826892`。
  - 交接：`offsetX` 和速度**差值都是 0**。
  - 结束：`offsetX=8`（悬停句柄胜出），`opacity=1`（入场句柄仍持有 opacity）。
- **附带观察**（只记录，不判缺陷）：帧泵关闭后，宿主如果继续调 `fm_on_frame`，**每一帧都会再回调一次 render(off)**（探针里 off 计数 1→2→3）。C# 侧会因此重复触发 `FramePumpChanged(false)` 和 `IdleTransition` ETW。T5 侧叉是在 off 时退订 Rendering，所以一般不会碰到。但 F2、F3 如果要断言 `IdleTransition` 的**精确条数**，就会变脆弱。建议只断言「最后一条是 pumpOn=0」和 「≥1」，并请三方决定要不要单独记一张票（暂称 F12）。修复属于 `code/` 内部，本轮不动。

---

## 2. 票单细化（第三节）

### 2.0 通用约定（所有票共用；PowerShell 5，不用 `&&`）

**路径约定**
- `$Repo` 是 Z5 建 worktree 时用的那个旁路克隆，路径**待 workbuddy 核实**，例如 `C:\Users\admin\mf_z3a`。**不能**是 `C:\Users\admin\motion_framework`。
- 每张票一个 worktree：`C:\Users\admin\mf_z6\<票号>`。临时日志放 `%TEMP%\z6\<票号>`。

```powershell
# ---- 每张票开头（P-1）----
$ErrorActionPreference = 'Stop'
$Base = '7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a'
$Repo = 'C:\Users\admin\mf_z3a'          # 待核实：Z5 worktree 的母克隆
$Tk   = 'f3'                              # 票号，小写
$WT   = "C:\Users\admin\mf_z6\$Tk"
$L    = "$env:TEMP\z6\$Tk"
New-Item -ItemType Directory -Force -Path $L | Out-Null
git -C $Repo fetch origin
if ($LASTEXITCODE -ne 0) { throw 'fetch failed' }
git -C $Repo worktree add -b "wb/z6-$Tk" $WT $Base
if ($LASTEXITCODE -ne 0) { throw 'worktree add failed' }
git -C $WT rev-parse HEAD | Out-File -Encoding ascii "$L\base_sha.txt"

function Write-Utf8NoBom([string]$Path, [string]$Text) {
  [IO.File]::WriteAllText($Path, $Text, (New-Object System.Text.UTF8Encoding($false)))
}
```

```powershell
# ---- 每张票结尾（P-2）：冻结面守卫，期望三行全部为空或 OK ----
git -C $WT diff --name-only $Base -- code/composition.h code/capi/fm_capi.h code/capi/fm_capi.cpp ':(glob)**/*_LOCKED*' | Out-File -Encoding ascii "$L\frozen_diff.txt"
if ((Get-Item "$L\frozen_diff.txt").Length -ne 0) { throw 'FROZEN TOUCHED - STOP' }
$cb = git -C $WT rev-parse HEAD:code/composition.h
if ($cb -ne 'fd0603c17136c067e89c71e56aef310f2f7e63f4') { throw 'composition.h blob changed - STOP' }
git -C $WT diff --stat $Base | Out-File -Encoding ascii "$L\diffstat.txt"
```

**提交**：提交说明写成英文 ASCII，先用 `Write-Utf8NoBom "$L\msg.txt" '...'` 写文件，再 `git -C $WT commit -F "$L\msg.txt"`。

**编码注意**：PowerShell 5 的 `Out-File` 和 `>` 默认写 UTF-16 LE。REPORT.md 这类中文文件**必须**用 `Write-Utf8NoBom` 写，否则 GitHub 上会显示乱码。

**REPORT.md 固定头**（每张票都要有）：
```
- ticket: Z6-<票号>   base: 7e2feb0e…（全量）   branch: wb/z6-<票号>   local tip: <sha>   pushed: yes(<sha>)/no
- 性质: dev-only；不采数、不计时；Success≠adopt；结论写 PASS（dev-only）/FAIL
- 冻结面守卫: frozen_diff 空；composition.h blob fd0603c1…
- 授权引用: 逐字原话 + 时间（AWST）+ 来源（拍板卡 id tNNNsNNNN 或本人消息 id）
- 副作用与清理: …（台账行号）
```

**api_changelog 写法**：各票**不直接改** `docs/api_changelog.md`，把要加的行写进 `z6/<票号>/CHANGELOG_ROW.md`，整合时再合并。原因是 F7 的②已经证明，多个分支往同一张表末尾追加行一定会冲突。

---

### 2.1 P0 · 准备（不写代码；建议在 F 票之前做一次）

**步骤**
1. 核对 `$Repo` 的路径，确认 `origin/main` 是 `7e2feb0e`。
2. **FluentHub 侧叉固化**：`C:\Users\admin\pilot\FluentHub` 里现在还放着**未提交**的 T5 改动和 bin/obj（`z5_closed.md` 第 10 节）。先在侧叉里建本地分支 `z5-t5-sidefork`，**只做本地 commit**（不推、不建 fork），然后核对：
   ```powershell
   git -C C:\Users\admin\pilot\FluentHub diff eb08f0dbcbb43880ab34038b0cb6742f0c0deca5 > "$env:TEMP\z6\p0\t5_now.patch"
   ```
   核对口径：以 `pilot/fluenthub/fluenthub_t5_side_fork.patch`（sha256 `79b00ba2…`）为准，比的是**补丁内容**而不是文件哈希，因为换行和 index 行可能不同。比不上就停下，说明差异在哪。
   之后每张宿主票都在侧叉里**叠一个本地分支**：`z6-f11`、`z6-f6`、`z6-f1c`、`z6-f2`、`z6-f5`。每张票交两份 patch：本票增量，以及相对 `eb08f0db` 的累计 patch。
3. **一次性向 pidan 请示**（拍板卡，原话照录入台账），建议卡片文字：
   > 「Z6 期间，workbuddy 可以在 K93 上用当前用户对 FluentHub 侧叉（以及 F9 基线通过后的 Files）做松散包注册（Add-AppxPackage -Register），每张票结束后 Remove-AppxPackage，并在台账登记？（不装证书、不改开发者模式、不提权）」
   - 如果 pidan 不同意：F1-C、F2、F5、F6、F11 的「运行」部分全部停在「编过」这一步，只交构建证据。
4. 建台账 `C:\Users\admin\mf_z6\LEDGER.md`，只追加（见第 4 节）。

**验收**：`p0/REPORT.md` 写清三件事：侧叉 patch 是否吻合（是/否加差异摘要）、包注册许可的原话（或「未获许可」）、台账首行。

**停止条件**：侧叉内容和 T5 补丁对不上；开发者模式关着（**不要自己打开**，这是系统设置）。

---

### 2.2 F3 · C 层「空动画 + Play + Retarget」用例（测试 · 无依赖 · 建议最先做）

**改哪些文件**：**新增** `code/capi/fm_capi_empty_anim_test.cpp`。**不改**原有的 `fm_capi_test.cpp`，这样历史基线「23 checks」保持可比，也不会和 F1-A 冲突。

**用例要点**（盒子上 clang + ASan/UBSan 已跑通，6/0）：
- 新 target 9：`fm_animation_create(s, 9, nullptr)`，然后 `play`。
- `retarget(..., hasFrom=0)` 必须返回 **0**：没有现值、也没给 from，会被拒绝。这个语义 T3 实测过。
- `retarget(..., hasFrom=1, from=0)` 必须返回 **1**，并且 `fm_is_running` 为真。
- 一直跑到停：最后的 `offsetX == 120.0`，并且 `renderOff ≥ 1`。
- `release` 之后 `destroy`。

**K93 命令**（照搬 T3 的编译口径）：
```powershell
cmd /c "call ""C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvars64.bat"" >nul && cd /d $WT && cl /nologo /std:c++17 /EHsc /O2 /DNDEBUG /MT /W3 /I code\capi /I code /I code\explore code\capi\fm_capi_empty_anim_test.cpp code\capi\fm_capi.cpp /Fo:$L\rel\ /Fe:$L\rel\f3.exe" > "$L\cl_rel.txt" 2>&1
```
- 注意：`&&` 只出现在 cmd.exe 的字符串里，PowerShell 5 本身不支持 `&&`。
- Debug 版同理，参数用 `/Od /Zi /RTC1 /D_DEBUG /MTd`，编译前先 `New-Item -ItemType Directory $L\rel,$L\dbg`。
- 运行：`& "$L\rel\f3.exe" code\capi\demo_motion.json`，记 `$LASTEXITCODE`。
- 同时重跑原有的 `fm_capi_test`，确认没被影响。

**验收（可核对）**
- `f3.exe` 的 Debug 和 Release 都输出 `6 checks, 0 failures`，退出码 0。如果 MSVC 下条数不同，就如实记录。
- 原有 `fm_capi_test` 的 Debug 和 Release 仍是 `23 checks, 0 failures`。
- `git diff --name-only $Base` **只有**新测试文件加上 `z6/f3/*`。
- P-2 守卫通过。

**回报**：REPORT.md，加两份编译日志和两份运行输出。

**风险**：「只改测试文件」和 Z5 不变量「`code/` 无 diff」字面上冲突，所以不变量要改写成「`code/` 的 diff 仅限新增的 `*_test.cpp`」（见 M11）。

**停止条件**：要让用例通过必须改 `fm_capi.*` 或 `code/*.h`，说明行为和预期不符，这时停下，不改产品代码，报告实际行为。

---

### 2.3 F1 · 仲裁「后写者胜」的运行证据（运行证据，不计时）——**要确定性，分三层**

> 核心思路：**不要**靠人手或者 `SendInput` 碰巧在入场动画的约 575 ms 里悬停。重叠要**由程序状态触发**。

**A 层（必做，决定性证据）：C ABI 加手动时钟**
- 新增 `code/capi/fm_capi_overlap_test.cpp`（盒子上已跑通，8/0）。步骤：
  1. A = `create(1, "demo.enter")`，`play(A)`。
  2. 按 1/120 s 推进 6 帧。断言 `-80 < offsetX < 0`，`vel > 0`，`0 < opacity < 1`。
  3. B = `create(1, nullptr)`，`play(B)`，`retarget(B, "offsetX", 8, 0.32, 0.78, hasFrom=0)` 返回 1。
  4. 断言交接**无跳变**：`fm_current` 读出的值和速度跟交接前相差 `< 1e-9`，也就是种子值确实来自另一个持有者 A。
  5. 再推进 1 帧，断言 `opacity < 1`：A 仍然持有 opacity，两个持有者确实同时在跑。
  6. 跑到停，断言 `offsetX == 8.0`（B 胜出；如果 A 还持有，会停在 0），并且 `opacity == 1.0`。
- 编译和运行命令同 F3。验收：Debug 和 Release 都是 `8 checks, 0 failures`。

**B 层（建议做）：C# devsmoke，走 internal 的 `CreateManual()`**
- 在 `MotionSoftDemo/Z3aL1DevSmoke.cs` 里加一个必测用例 `Z6F1_OVERLAP`，流程和 A 层相同，但通过 C# 公开面的 `CreateAnimation`、`Play`、`Retarget` 调用，用 `Advance(1.0/120)` 加 `OnFrame()` 推进，同时挂一个 EventListener。
- 输出一行：
  ```
  Z6F1_OVERLAP retargetFrom=<v> inRange=<bool> velAtHandoff=<v> endOffsetX=8 endOpacity=1 enterCancelled=0
  ```
- 判据：
  - ETW `Retarget` 事件里 `property=offsetX`，`from` 在 (-80, 0) 之间，`hasFrom=0`，`velocityAtHandoff > 0`。
  - 交接点之后、结束之前，入场句柄**没有** `Cancelled`。
- `Z3aL1DevSmoke.cs` 是测试代码，不进 pack（pack 只链接 MotionSystem.cs 和 MotionEtw.cs），T3 就改过它。CheckSurface 对比时要排除 `Z3aL1DevSmoke` 类型本身，口径和 T5-0 相同。

**C 层（宿主证据，FluentHub 侧叉，本地）：事件驱动触发**
- 在侧叉 `NavMotionPilot` 里加测试钩子，只有环境变量 `FM_PILOT_F1=1` 时才生效，默认关。逻辑如下：
  1. 在 `OnValueChanged` 里，当 `target==1`、`property=="offsetX"`、`-60 < value < -5` 并且还没触发过时，**不能在 sink 里直接调 fm_\***（违反 Table 2 线程规则，`_nativeDepth` 会拦）。只用 `DispatcherQueue.GetForCurrentThread().TryEnqueue(DispatcherQueuePriority.Low, ...)` 排一个回调。
  2. 回调里调用和 `OnItemPointerEntered` **同一个**函数，要先把它重构成 `HoverIn(ulong target)`，指针事件和钩子都调它。
  3. 回调执行时先读 `_system.Current(1, "offsetX")`。如果已经 ≥ 0（入场已结束），记 `F1 MISSED`，这一次运行作废，重跑。属于 dev-only 运行，不是闸门会话，所以重跑不算凑数。
  4. 记一行：
     ```
     F1 trigger enterAnim=<id> hoverAnim=<id> offsetX_at_trigger=<v> opacity_at_trigger=<v>
     ```
- 判据（全部来自日志，可以 grep）：
  - (a) `F1 trigger` 行里 `-80 < offsetX_at_trigger < 0`，并且 `opacity_at_trigger < 1`。
  - (b) 随后有一条 ETW `Retarget`：`animId` 等于 hoverAnim，`property=offsetX`，`from` 在 (-80, 0)，`velocityAtHandoff ≠ 0`。
  - (c) 之后 `ValueChanged(1, offsetX)` 的最后一个值是 8，不是 0。
  - (d) `ValueChanged(1, opacity)` 在 trigger 之后仍然继续变化，直到 1。
  - (e) 退出时 `Created == Released`。
- **注意**：C# 的 `Retarget` ETW 字段 `from` 取自 `Current()`，也就是**上一帧的采样值**；原生侧种子值取的是 `owner->Advance()` 之后的当前值。真实时钟下两者可能有小差，所以 C 层**不比较精确相等**。精确相等只在 A 层断言。
- 可选：再用 `SendInput` 真实悬停做一次（D7 已经证明 `SetCursorPos` 不行）。只作佐证，不计入判据。

**验收汇总**：A 层 8/0（Debug 和 Release），这一条是**必须**。B 层 `Z6F1_OVERLAP` 全部字段满足。C 层 (a) 到 (e) 满足。
- REPORT.md 结论写「仲裁语义经 A/B 层确定性验证、C 层宿主运行佐证（dev-only）」。
- **不写**「仲裁已采用」，也不写任何时延数字。

**改哪些文件**
- mf 分支 `wb/z6-f1`：`code/capi/fm_capi_overlap_test.cpp`（新增）、`MotionSoftDemo/Z3aL1DevSmoke.cs`（只加测试用例）、`z6/f1/*`（报告、日志、侧叉增量 patch）。
- 侧叉本地分支 `z6-f1c`：`Views/NavMotionPilot.cs`。

**风险和停止条件**
- B 层改动导致 devsmoke 程序集的公开面 dump 出现除 `Z3aL1DevSmoke` 以外的差异：停。
- C 层连续 3 次 `F1 MISSED`：停，报告，不改 token 时长凑数。如果要换慢速 token，见 F6。

---

### 2.4 F4 · AOT 宿主 EventSource 指引（**建议本轮只写文档**）

**改哪些文件**：新增 `docs/l1_host_guide_aot_etw.md`，在 `z6/f4/CHANGELOG_ROW.md` 里写一行「已知限制 K-AOT-ETW 宿主指引」。

**文档要写清楚**：
- 触发条件：`PublishAot=true`。注意 Files 是**通过 pubxml 加 `PublishProfile` 属性**间接引入的，见 F9。
- 后果：runtimeconfig 里出现 `System.Diagnostics.Tracing.EventSource.IsSupported=false`。
- 宿主侧开关：`<EventSourceSupport>true</EventSourceSupport>`。
- 自检方法：在输出目录的 `*.runtimeconfig.json` 里 `Select-String IsSupported`。
- 证据：引用 T5 的 run1 和 run2 两份日志。

**验收**：
- 文档里有上面五项。
- `Select-String` 示例命令可以在 FluentHub T5 输出目录复现：开关打开前是 false，打开后是 true，**或者键不存在**。这一条待 workbuddy 实测，确认 true 时是写入 true 还是不写这个键。

**为什么不做「L1 侧 Debug 提示」**：那要改 `MotionSystem.cs`（虽然是 internal），会让 F7 刚打好的 pack 失效。如果三方坚持要做，就排在 F7 **之前**，并且照 T3 的标准：changelog 行、Debug/Release 各跑一次 CheckSurface 并把原始输出入库、devsmoke PASS。

---

### 2.5 F8 · 闸门 DRAFT（只起草，不跑任何东西）

**改哪些文件**：`docs/z6_f8_present_stats_gate_DRAFT.md`。文件头写 `STATUS: DRAFT（未 LOCK）`。

**内容至少包括**：
- 假设：H1–H8 照 T6 的写法。
- 测量方法和工具。
- 会话定义。
- 预先锁定的阈值和失败口径，并写明「失败不重跑凑数」。
- 绑定的 tip：要含 T3（d618140b 之后），推荐绑定 F7 的 tip。
- 和 PERF-CI 的关系：保持 HOLD。
- K93 的显示和电源状态只能**只读记录**。

**硬性**：不跑任何探针，包括「干跑」「预采」「试一下」，也不建计时脚本的可执行版本。
- 验收：分支 diff 只有这份 DRAFT 加 `z6/f8/REPORT.md`。

---

### 2.6 F7 · 在 main 内容上重打 L1 pack（依赖：F4 如有代码改动须先完成；建议 F3、F1-A 之后）

**草稿要修正的地方**：
- F7 的②（`api_changelog` 冲突）已经在 `7e2feb0e` 里解决（三行都在），本票只做①。
- 「新 pack 绑定 7e2feb0e」要写准确：pack 从 `wb/z6-f7` 的 worktree 构建，这个分支的 `code/`、`MotionSoftDemo/MotionSystem.cs`、`MotionEtw.cs` 和 main 完全相同。

**步骤**
```powershell
# P-1 完成后
cmd /c "call ""C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvars64.bat"" >nul && cd /d $WT && mkdir MotionNative 2>nul & cl /nologo /std:c++17 /EHsc /O2 /W3 /LD /I code\capi /I code /I code\explore code\capi\fm_capi.cpp /Fo:MotionNative\ /Fe:MotionNative\fluidmotion.dll" > "$L\cl_native.txt" 2>&1
dotnet build "$WT\pilot\l1pack\FluidMotion.L1.csproj" -c Release --no-incremental > "$L\pack_build.txt" 2>&1
$exit = $LASTEXITCODE
$out = "$WT\pilot\l1pack\bin\x64\Release\net8.0"    # 待核实：以实际输出目录为准（T5-0 记录为 bin\x64\Release\net8.0）
Get-FileHash -Algorithm SHA256 "$out\FluidMotion.L1.dll","$out\fluidmotion.dll","$out\demo_motion.json" | Format-List | Out-File -Encoding ascii "$L\hashes.txt"
git -C $WT hash-object MotionSoftDemo/MotionSystem.cs MotionSoftDemo/MotionEtw.cs | Out-File -Encoding ascii "$L\hash_object.txt"
```
- 公开面：用 `z5/t3_results_dev/K93N3P8/SurfaceDump_Program.cs` 和 `SurfaceDump.csproj.txt` 在 `%TEMP%` 下重建工具，对**新 pack DLL** 同时跑 dump 和 CheckSurface，原始输出**入库**。这样也补上了 T5-0 时 pack DLL 没有 CheckSurface 原始输出的缺口。
- 更新 `pilot/l1pack/SHA256SUMS`。文件头的 `mf_tip` 写 F7 分支的 tip，并注明「source identical to main 7e2feb0e」；`git hash-object` 一节更新为 `fe72b08f…` 和 `0404498a…`。

**验收（可核对）**
- `cl_native.txt` 里没有 error，`pack_build.txt` 里有 `0 Error(s)`，`$exit` 为 0。
- 新 SHA256SUMS 里的三条哈希和 `Get-FileHash` 的输出逐字一致。
- 新 pack 的 dump 和 `pilot/fluenthub/surface_l1pack.txt`（sha256 `7b67b77b…c5cc`）**逐字相同**：`git diff --no-index --exit-code` 返回 0。CheckSurface 输出 `CHECKSURFACE fails=0`。
- **预期**两个 DLL 的哈希和旧值**不同**：PDB 校验和与 cl 时间戳都会变，这不算失败。`demo_motion.json` 的哈希应当**不变**，仍是 `8d5db577…`。

**风险**：如果 dump 有差异，说明不只是注释变了，要停下来上报。

**停止条件**：构建失败；dump 不一致；需要装任何 SDK。

---

### 2.7 F11 · 侧叉 csproj 可配置路径和构建时 sha 校验（依赖：F7）

**改哪些文件**：侧叉本地分支 `z6-f11` 里的 `src/FluentHub/FluentHub.csproj`。mf 分支上放 `z6/f11/fluenthub_f11.patch` 和报告。

**做法（宿主侧 MSBuild）**
- `FmL1PackDir` 可以从 `-p:` 或环境变量 `FM_L1_PACK_DIR` 传入；默认值是 `$(MSBuildThisFileDirectory)..\..\..\fm_l1pack\`，相对路径。
- 三个期望哈希写成属性 `FmL1DllSha256`、`FmNativeSha256`、`FmTokensSha256`，取值来自 F7 的 SHA256SUMS。
- 在 `BeforeTargets="ResolveAssemblyReferences"` 上挂一个 Target：
  - 用内置的 `GetFileHash` 任务（`Algorithm="SHA256"`）算三个文件的哈希。注意这个任务输出的是**大写**十六进制，比较时要统一大小写。
  - 文件不存在时报 `<Error Code="FMPACK001" .../>`，哈希不一致时报 `<Error Code="FMPACK002" Text="... expected X got Y"/>`。

**验收（三次构建，可 grep）**
1. 用默认路径构建：0 错误。
2. 把 pack 复制到 `C:\Users\admin\mf_z6\packcopy\`，构建时加 `-p:FmL1PackDir=C:\Users\admin\mf_z6\packcopy\`：0 错误。
3. 对复制件追加 1 个字节：`[IO.File]::AppendAllText("...\packcopy\demo_motion.json", " ")`。再构建，必须失败，日志里要有 `FMPACK002`。
4. 删掉复制件里的 `fluidmotion.dll` 再构建，必须失败，日志里要有 `FMPACK001`。
- 第 4 步之后删除 packcopy，并在台账登记。
- 构建命令沿用 T4 的非原样口径，包括 `-p:NuGetAudit=false`，这是 pidan 01:18、01:19 批过的。REPORT 里逐字引用原话。

**风险**：FluentHub 的 `TreatWarningsAsErrors=true`，新加的 Target 里**只能用 Error，不要用 Warning**，否则可能误伤正常构建。

**停止条件**：要改 FluentHub 的 `Directory.Build.props` 或全局配置；要注册包但没有 P0 许可。

---

### 2.8 F6 · 入场动画可见：等首帧再 Play（宿主侧；依赖 F11）

**做法**（侧叉本地分支 `z6-f6`）
- 构造函数里**只做三件事**：建系统、在入场起点**同步**写入初值（opacity=0，`TranslateTransform.X=-80`）避免闪烁、订阅一次 `CompositionTarget.Rendering`。
- 在**第 2 次** Rendering 回调里才 `CreateAnimation` 并 `Play`，然后退订这个一次性的回调。
  - 为什么是第 2 次：第 1 次回调在首帧合成之前触发；第 2 次触发时，至少已经提交过 1 帧。这个是启发式判断，在报告里如实写明。
- 记三行日志：`F6 rendering#1 seq=…`、`F6 rendering#2 seq=… -> enter Play`，以及第一条入场 `ValueChanged` 的 seq。

**画面证据（不计时）**
- 默认 token 下入场约 575 ms（盒子上 120 Hz 手动时钟测得），在 K93 约 32 Hz 的回调下大概只有十几帧，截到中间状态要靠运气。
- 建议：**只在拍可见性证据时**加载侧叉自带的 `pilot_slow_motion.json`。它放在侧叉目录里，**不进 pack**，不影响 SHA256SUMS。把 `normal` 改成 2000 ms，`snappy` 的 response 改成 1.5 s，只通过环境变量 `FM_PILOT_SLOW=1` 启用。
- 拍「入场中途」的当前窗口截图，至少 1 张能看到菜单项半透明或者处于偏移状态。日志里要有 `tokens=pilot_slow_motion.json`。
- 再用默认 token 跑 1 次，只验证日志里的顺序。

**验收（可核对）**
- 日志里 `rendering#2 seq` < `enter Play seq` < 第一条入场 `ValueChanged` 的 seq。
- 至少 1 张慢速 token 下的中途截图，只截当前窗口，不超过 5 MB。
- `Created == Released`。
- 报告里注明「慢速 token 仅为可见性取证，不是产品值，也不是计时判据」。

**停止条件**：截图需要改显示设置或者 DPI。

---

### 2.9 F2 · RootView 重建（Unloaded→Loaded）运行证据（宿主侧；依赖 F6）

**草稿遗漏的风险**：草稿写「先 Dispose 后 Create」，但 WinUI 里替换 `Content` 时，**新元素的 Loaded 可能先于旧元素的 Unloaded**。旧元素的 Unloaded 是异步的，这一点要在 K93 实测，**待核实**。所以「先后顺序」**不能当成验收判据**，只记录实际观察到的顺序。

**做法**（侧叉本地分支 `z6-f2`）
- 测试钩子：环境变量 `FM_PILOT_F2_CYCLES=N`，默认关。用 `DispatcherQueueTimer` 每隔一段时间调用一次 FluentHub 自带的 `MainWindow.Instance.InitializeApplication(null, forceReload: true)`，它会 new 新的 RootView，旧的随之 Unloaded，一共 N=10 次。
  - 定时器间隔只负责编排，不当作结果。
- 再加一种：同一个实例 `Content = null` 之后又 `Content = 同一实例`，用来验证 `_motionPilot ??=` 加 Unloaded 置空这条路径，跑 3 次。
- 每个 `NavMotionPilot` 实例都记 `sessionId`。`Dispose end` 那一行已经有 handles 计数。

**验收（可核对）**
- 每个 sessionId 的 ETW `Created` 数等于 `Released` 数。
- 每个 sessionId 最后一条 `IdleTransition` 都是 pumpOn=0。只断言「最后一条」，原因见第 1 节附带观察。
- 关窗之后，日志里 `PILOT start` 的行数等于 `Dispose begin` 的行数。按上面的次数，预期都是 1 + 10 + 3 = 14；如果实际次数不同，以日志为准，两者相等即可。
- 进程退出时，日志里**没有**未释放的句柄行。
- 可选诊断（不是闸门、不计时）：第 1 次和第 10 次重建后各记一次 `Get-Process -Name FluentHub | Select HandleCount`，只记录，不判定。

**停止条件**：出现两个 MotionSystem 同时存活，并且导致异常，比如跨线程或 `ObjectDisposed`。这时停下报告，不在 L1 里修。

---

### 2.10 F5 · ReducedMotion 跟随系统设置变化（**改归类为宿主侧**；依赖 F2）

**事实**：L1 没有读系统设置，读取发生在侧叉 `NavMotionPilot` 的构造函数里：`new UISettings().AnimationsEnabled`，然后 `SetReducedMotion(!enabled)`。如果要 L1 自己去监听，就得把 pack 的 TFM 从 `net8.0` 改成 Windows TFM 并引入 WinRT，这是重大改动，**不建议**。

**做法**（侧叉本地分支 `z6-f5`；L1 公开面和 L1 源码都不动）
- 抽一个侧叉内部接口 `IReducedMotionSource { bool Current; event Action<bool> Changed; }`，提供两个实现：
  - `UISettingsSource`：订阅 `UISettings.AnimationsEnabledChanged`。这个事件 Windows 10 2004（19041）起提供，事件参数不带数据，要重新读 `AnimationsEnabled`。**这个事件在后台线程触发**，必须 `DispatcherQueue.TryEnqueue` 回 UI 线程再调 `SetReducedMotion`，否则会触发 L1 的 `Affinity` 异常（Table 2 规则）。另外 `UISettings` 实例要保存成字段，防止被 GC 回收后事件丢失。
  - `FakeSource`：只有环境变量 `FM_PILOT_F5=1` 时启用，用一个隐藏快捷键（比如 Ctrl+Alt+Shift+R，用 `SendInput` 驱动）或者事件驱动脚本翻转取值。
- 兜底方案（**待核实**是否需要）：如果 K93 上 WinUI 3 桌面应用收不到 `AnimationsEnabledChanged`，就在窗口 `Activated` 时重读一次，或者用子类化处理 `WM_SETTINGCHANGE`（`SPI_SETCLIENTAREAANIMATION`）。

**不改系统设置的测试方法（主证据）**
1. C 层：已有的 `fm_capi_test` 第 64–69 行已经覆盖「飞行中切到 reduced，下一帧跳到终值」，F5 引用它即可。
2. 宿主层：设 `FM_PILOT_F5=1`。在入场或悬停进行中翻转 Fake 取值，日志要出现：
   ```
   RM change source=fake animationsEnabled=False -> SetReducedMotion(True) seq=…
   ```
   之后**下一帧**的 `ValueChanged`：offsetX 是终值（入场时切换应为 0；悬停时切换预期为 8，**待核实**，依据是 `controller.h:76` 注释里提到的 reduced-motion snap），opacity 是 1，并且随后出现 pumpOn=0。再翻回 False，新的悬停恢复成弹簧插值，即连续出现中间值。
3. 真实路径的接线证据：日志里有 `UISettingsSource subscribed`，以及启动时读到的值。**不切换**系统设置。

**真实系统切换（可选，只能 pidan 本人来）**：如果三方要真实证据，由 pidan **亲自**在 K93 的「设置 > 辅助功能 > 视觉效果 > 动画效果」里切一次再切回，workbuddy 只看日志。或者 workbuddy 先拿到 pidan 拍板卡原话「允许 workbuddy 在 K93 切换动画效果并恢复」，再自己操作。**不允许**用 `SystemParametersInfo` 去写设置，那也是系统级改动。

**验收**：主证据第 1、2、3 条都满足。报告写「真实系统切换：未做 / 由 pidan 本人执行（原话）」。

**停止条件**：要改 L1 源码；要改系统设置但没有许可。

---

### 2.11 F9 · Files（C 试点）——**拆成 F9a 基线和 F9b 侧叉**

见第 3.3 节。F9a 可以和其他票并行。F9b 要等 F9a 报告经三方看过、pidan 另外说「开」才开始，原因是 Files 的体量和风险都比 FluentHub 高一级。

---

## 3. 三个重点问题的结论

### 3.1 F1：怎样**稳定**地制造重叠

**结论**：用三层方法，全部不需要改 L1 公开面。
- **A 层**：C ABI 的 `fm_create_manual` 加 `fm_manual_advance`，逐帧精确控制时间。盒子上已验证：交接时值和速度的差都是 0，`offsetX` 最终停在 8，`opacity` 最终停在 1。
- **B 层**：devsmoke 走 internal 的 `CreateManual()`，取 ETW `Retarget.from` 作证据。
- **C 层**：宿主用 sink 观察到「入场中途」之后，通过 `DispatcherQueue` 排队调用同一个 `HoverIn`。由程序状态触发，不靠人手时机；没触发成功就作废重跑。

几点说明：
- 「可控时钟」只存在于 internal 和 C ABI 测试入口，这本来就是设计好的 dev/test 入口（`MotionSystem.cs:116`、`fm_capi.h:24`），**不用新增任何东西**。
- 不建议的做法：
  - 拉长 `demo_motion.json` 的入场时长。这会改 pack 内容，SHA256SUMS 随之变化。
  - 用 `Start-Sleep` 卡时间去悬停。在 32 Hz 的机器上不稳定。
  - 在 `ValueChanged` 里直接调 `Retarget`。违反 Table 2 线程规则，会被 `_nativeDepth` 拦下。

### 3.2 F5：怎样不改系统设置就能测试

**结论**：
- 先把 F5 **改归类为宿主侧**。系统设置读取在侧叉里，L1 只有 `SetReducedMotion`。
- 宿主侧做一个可替换的来源接口：真实来源用 `UISettings.AnimationsEnabledChanged` 并封送回 UI 线程，测试来源用 Fake，靠环境变量加隐藏快捷键或事件驱动触发。
- 主证据由三部分组成：C 层已有用例、宿主层 Fake 翻转后下一帧跳到终值、真实来源的订阅日志。这三部分**都不碰系统设置**。
- 真实系统切换列为可选，只能由 pidan 本人操作，或者凭 pidan 拍板卡的原话授权。

### 3.3 F9：Files 用哪个仓库和版本

**结论：files-community/Files，标签 `v4.3`，commit `cbfb4ba54a89632fbceb82ea94ada60a9bae3737`。** 这是 GitHub 最新的稳定 release：发布于 2026-10-12 02:35 AWST，commit 时间 2026-10-12 02:04 AWST，`prerelease=false`。

| 项 | v4.3（推荐） | main HEAD `3fbc10d0…`（2026-11-18 06:24 AWST 推送） | v4.2.9 `99951c66…` |
|---|---|---|---|
| Microsoft.WindowsAppSDK | **2.5.1** | 2.6.5 | 2.4.0 |
| Microsoft.Windows.SDK.BuildTools | **10.0.28000.2705** | 10.0.28000.2957 | 10.0.28000.2526 |
| CsWinRT | 2.3.1 | 2.3.1 | 2.2.0 |
| global.json .NET SDK | 10.0.102（`rollForward: latestMajor`） | **10.0.403** | 10.0.102 |
| TFM | net10.0-windows10.0.26100.0，最低 19041 | 同 | 同 |
| C++ 子项目工具集 | **v145（VS 2026）** | 待核实（大概率相同） | 待核实 |
| 发布配置 | `win-x64.pubxml`：`PublishAot=true`、`SelfContained=true`；csproj 里设了 `PublishProfile`、`IsAotCompatible=true` | 待核实 | 待核实 |
| 许可 | MIT（树里另有 LICENSE-MPL） | 同 | 同 |

**推荐 v4.3 的理由**：
1. 这是稳定标签，不会漂移。main 每天都在变，`z1_pilot_scoring_table_DRAFT.md` 里记的「WASDK 2.6.5」就来自 11-10 那天的 main `e40fa63825`，**和 v4.3 不一致**。建议更正评分表，或者在 F9 报告里注明。
2. v4.3 用的 WASDK **2.5.1** 和 SDK.BuildTools **10.0.28000.2705**，跟 MotionSoftDemo 的 csproj **完全相同**。K93 已经用这套工具链构建并运行过 devsmoke GUI，可以少一类变量。
3. .NET SDK 10.0.102 加 `latestMajor`，比 main 要求的 10.0.403 宽松。K93 装的是哪个 .NET 10 SDK **待核实**：`dotnet --list-sdks`。

**已知缺口（写进 F9a 票里，基线时如实记录，不要自己修）**
- **G-F9-1 · C++ 工具集 v145**：
  - `Files.App.Launcher`、`OpenDialog`、`SaveDialog` 这几个 vcxproj 都要 `PlatformToolset=v145`，也就是 VS 2026。K93 的记录是 VS 2022 BuildTools 17.14（v143），**原样构建 `Files.slnx` 预计会失败**（待核实）。
  - 处理办法要请 pidan 三选一：
    - (a) pidan 本人安装 VS 2026 BuildTools 的 C++ 组件。这是系统级操作。
    - (b) 非原样构建：只构建 `src/Files.App/Files.App.csproj`。slnx 里的 `BuildDependency` 只在构建整个解决方案时生效；单独构建 csproj 时 Launcher.exe 可能缺失，影响有多大待核实。
    - (c) 非原样构建：加 `-p:PlatformToolset=v143`。不改文件，但 C++ 代码可能编不过。
  - 照 T4 的先例，原样构建的 FAIL 记录要保留。
- **G-F9-2 · `.slnx` 格式**：MSBuild 17.14 能不能直接构建 `.slnx`，**待核实**。不行的话就退回 (b)。
- **G-F9-3 · AOT 关闭 EventSource**：csproj 设了 `PublishProfile=...\win-$(Platform).pubxml`，**构建时也会导入**这个 pubxml，于是 `PublishAot=true` 生效，很可能再次出现 K-AOT-ETW（待核实，查输出目录里 `Files.runtimeconfig.json` 的 `IsSupported`）。F9b 侧叉要加 `EventSourceSupport=true`，口径同 T5。
- **G-F9-4 · 必须打包运行**：Files 是 MSIX 工程（`EnableMsixTooling`、`Package.appxmanifest`），还带一个进程外 COM 服务器 `Files.App.Server`。运行要松散注册（P0 许可），可能还要注册 COM 服务器，**待核实**。**不导入**任何证书。CI 里的签名用的是自签证书，本地**不要**照搬。
- **G-F9-5 · 遥测**：`AppLifecycleHelper.ConfigureSentry()` 用的 DSN 是占位符 `"sentry.secret"`，由 CI 注入真实值。本地构建预期**不会**往外发崩溃报告，待核实运行时的行为。报告里记一笔。
- **G-F9-6 · 体量**：要求不并行构建（Files 的 AGENTS.md 原话「Do not run build commands in parallel」），构建命令加 `-v:quiet -clp:ErrorsOnly`，日志控制在 5 MB 以内。

**F9a 基线步骤（K93，PowerShell 5）**
```powershell
$ErrorActionPreference = 'Stop'
$F = 'C:\Users\admin\pilot\Files'          # 只本地；不 fork、不推、不提 PR/issue
$L = "$env:TEMP\z6\f9a"; New-Item -ItemType Directory -Force -Path $L | Out-Null
git clone --no-tags --filter=blob:none https://github.com/files-community/Files $F
if ($LASTEXITCODE -ne 0) { throw 'clone failed' }
git -C $F fetch origin tag v4.3 --no-tags
git -C $F checkout --detach cbfb4ba54a89632fbceb82ea94ada60a9bae3737
git -C $F rev-parse HEAD | Out-File -Encoding ascii "$L\files_head.txt"   # 期望 cbfb4ba5…
dotnet --list-sdks | Out-File -Encoding ascii "$L\sdks.txt"
& "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -all -format json | Out-File -Encoding utf8 "$L\vswhere.json"
# 原样（A0）：Developer PowerShell 或 vcvars 环境下
msbuild -restore "$F\Files.slnx" -p:Configuration=Debug -p:Platform=x64 -v:quiet -clp:ErrorsOnly > "$L\a0_build.txt" 2>&1
"A0_EXIT=$LASTEXITCODE" | Out-File -Encoding ascii -Append "$L\a0_build.txt"
```
- A0 失败就停下，把 G-F9-1 的三个选项交给 pidan 拍板。拿到原话之后再做 A1（非原样），并在 REPORT 里逐字引用原话。
- 构建通过之后，运行要先有 P0 许可（松散注册）。截一张当前窗口的主界面截图。

**F9a 验收**
- `files_head.txt` 等于 `cbfb4ba5…`。
- 有 SDK 清单和 vswhere 清单。
- A0 的结果：PASS 或 FAIL 加首个错误摘录。
- 如果做了 A1：标「非原样」，写清非原样的点，引用授权原话。
- 截图（如果能运行）。
- 写明 `runtimeconfig` 里 `IsSupported` 的取值。
- 交付件 `pilot/files/BASELINE.md` 加截图。
- `C:\Users\admin\pilot\Files` 下 `git status --porcelain` 必须为空，即**不改 Files 的任何文件**。

**F9b（另开票）建议接入点**：`src/Files.App.Controls/Sidebar/SidebarItem*`，即侧栏项。它和 FluentHub 的 NavigationView 菜单项最接近，适合对照。
- 优先不改 Files.App.Controls，只在 `Files.App` 侧通过视觉树把 handler 挂到 `SidebarItem` 上。
- 消费 F7 pack，带 F11 的哈希校验。
- 用 `EventSourceSupport=true`。

**备选候选**（只在 Files 因工具链完全走不通、且 pidan 同意换席时才考虑）：`bkaankose/Wino-Mail`。这是 Z1 已经登记的备份，GPL-3.0，只做侧叉。它的最新标签、WASDK 版本我这次**没查，待核实**。C 席是 pidan 2026-11-10 22:59 AWST 书面批准的 Files，**换席要 pidan 本人决定**。

---

## 4. 硬约束漏洞（逐条）与可见性缺口

### 4.1 漏洞清单

| # | 漏洞（一句话） | 影响 | 补法 |
|---|---|---|---|
| H1 | **第三方仓库规则没写进硬约束**：只有 F9 一行提到「侧叉只留本地」，FluentHub 侧叉没有明确约束，也没禁止在 GitHub 上提 issue、评论、star 或 fork | 可能有对外可见的动作 | M6 |
| H2 | **没写「不改 `*_LOCKED*`」**：Z5 的硬边界里有这一条，草稿漏了。仓库里有 10 个以上 LOCKED 文件，还包括 `artifacts/` 下的 `.txt` | 合同和闸门文件可能被误改 | M3 |
| H3 | **证据不可变没写**：`z3a/`、`z4/`、`z5/`、`pilot/` 下已有的证据和报告，更正只能追加（照 C1、C2 的先例），不能改写 | 历史证据可能被覆盖 | M7 |
| H4 | **包注册既算系统级，又没有安排许可**：FluentHub 的 `MainWindow` 必须有包身份才能运行，Files 也是 MSIX。按草稿第 6 条，每次运行都要停下请示 | 宿主票全部卡住，或者 workbuddy 为了推进自行放宽口径 | M5，P0 一次性拍板卡 |
| H5 | **系统级清单不全**：漏了导入证书（Files 的 CI 用自签证书）、计划任务（T3 用 `schtasks /IT` 跑 GUI）、安装 VS 组件或工作负载（v145）、`dotnet workload`、`dotnet tool -g`、开发者模式、防火墙，以及辅助功能里的动画效果、DPI、显示设置 | 可能出现灰色地带的系统改动 | M5 |
| H6 | **「非原样构建开关」没有规则**：T4 的先例是 `-p:NuGetAudit=false` 由 pidan 拍板，F9 很可能要 `-p:PlatformToolset=v143` 或者只构建 csproj | workbuddy 可能自己加开关，同时宣称「原样」 | M7 |
| H7 | **「不采数」和日志里的 FrameTick deltaUs 有灰区**：EventListener 默认 Verbose，会收到 FrameTick 的 deltaUs。T5 的做法是只计数 | 日志里可能留下帧间隔数据，被人当成「采数」 | M4 |
| H8 | **「不计时」和脚本里的等待有灰区**：`Start-Sleep` 或定时器用来编排是必要的，要说明可以用，但不能把时长写成结果。还要明确禁止 F8 的「干跑」「预采」 | 两种方向的误读都可能出现 | M4 |
| H9 | **基线写「或之后 pidan 认可的 main」太模糊**：没要求记录实际的 base sha，也没说 main 前进之后怎么办（禁止 rebase，只能开新分支） | 证据绑定不清楚 | M8 |
| H10 | **授权引用格式不够**：草稿要求「逐字照录原话和时间」，但没要求写**来源 id**（拍板卡 `tNNNsNNNN` 或消息 id）。如果 workbuddy 不向协调汇报，第三方就只能凭这个 id 核实 | 授权链没法复核 | M9 |
| H11 | **L1 内部改动的边界没定**：草稿把 F5 写成「L1 内部」，F4 允许可选代码，但没规定 L1 内部改动要满足 T3 的标准，也没提这类改动会让 pack 失效 | pack 和证据可能对不上 | M3、M12、M16 |
| H12 | **提示词自称「唯一提示词」，却引用了 workbuddy 读不到的路径**：workbuddy 在 K93 上，看不到盒子的 `/workspace/review/...`；它能读的只有 GitHub 公开仓库和 K93 本地文件。本评审里的细化内容也到不了它手里 | 细化步骤落不了地 | M19 |
| H13 | **账号**：草稿写了「不改任何账号」，但没写「**不登录**任何第三方账号」。FluentHub 的主功能要 GitHub 登录，workbuddy 可能想登录后测列表 | 涉及凭据 | M6 |
| H14 | **清理时机没有规定**：草稿要求写「副作用和清理方法」，但没说由谁决定、什么时候清。Z5 的先例是 pidan 在 06:03 拍板卡里决定，而清理之后日志就永久丢失了 | 证据可能被提前删除 | M17 |

### 4.2 「是否向协调汇报由 workbuddy 自己决定」带来的可见性缺口

**不推翻 pidan 的决定**：workbuddy 可以不向协调汇报，直接对 pidan 汇报或请示。但会出现三个缺口：
1. 协调和三方不知道进度。只要没推送，就没有任何可以复核的东西。
2. 授权链只存在于 workbuddy 和 pidan 的对话里。
3. 停止点、副作用、清理这些信息没有统一的落点。

**建议最低留痕**（只规定「留下什么」，不规定「向谁汇报」）：
- **L-1 · K93 本地台账** `C:\Users\admin\mf_z6\LEDGER.md`，只追加，用 UTF-8 写。每条一行：
  ```
  时间(AWST) | 票 | 事件(start/stop-condition/system-change/pkg-register/pkg-remove/push/cleanup/done) | 摘要 | 授权来源id或"-"
  ```
- **L-2 · 请示随附台账**：workbuddy 本来就要为停止条件、系统级改动、push、合 main 去请示 pidan。请示消息里附上相关的台账行，不增加汇报对象。
- **L-3 · 票结束时二选一**：(a) 拿到 pidan 许可后普通 push，REPORT 里写远端 tip；(b) 告诉 pidan「仅本地，未推送」。两种情况都要写进台账。
- **L-4 · 复核口径**：协调和三方只对**已推送**的分支做复核。没推送的票一律标「未复核」，不能进入任何结题文档。
- **L-5 · 协调可以只读 fetch**：协调可以随时 `git fetch` 只读查看远端 `wb/z6-*`。这不算 workbuddy 汇报，也不需要它配合。
- **L-6 · 台账入库**：每次 push 时，把台账的当前副本放进该分支的 `z6/LEDGER_snapshot.md`。只追加，不改历史。

---

## 5. 逐条修改建议（原文位置 + 原文 + 建议替换）

> 行号指 DRAFT r0（sha256 `c95b27d0…0251`）的行号。

**M1 · 第 4 行（基线说明）**
- 原文：「> 基线：main `7e2feb0e`（PR #9 合入 Z5，tree `d61f0138`）。结题文档 `docs/z5_closed.md`（第 5 节后续票）。」
- 替换为：
  「> 基线：main `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`（PR #9 于 2026-11-18 06:23:57 AWST 合入 Z5，tree `d61f0138`；合并授权原话：〔待协调补引 pidan 原话、时间和来源 id〕）。结题文档 `docs/z5_closed.md` 第 5 节列出后续票。注意：该文件头写的『待落盘 / 不合 main』是结题当时的状态，现在 main 已经包含 Z5。F7 第②项（`api_changelog.md` 冲突）已在 `7e2feb0e` 里解决，三行都保留了。」

**M2 · 第一节，第 13–14 行（汇报）**
- 原文：「pidan 的原话是「是否向你汇报由他自己决定」。你可以直接在你和 pidan 的对话里汇报、请示，不必经过协调 bot（动效框架）。」
- 保留原文，在后面追加：
  「不过，不管你向谁汇报，以下留痕都必须做：① 在 K93 写只追加的台账 `C:\Users\admin\mf_z6\LEDGER.md`（格式见第四节）；② 请示 pidan 时附上相关台账行；③ 每张票结束时，要么经 pidan 许可普通 push，要么告诉 pidan『仅本地，未推送』；④ 没推送的结果一律算『未复核』，不能进入结题；⑤ 协调可以随时只读 fetch 远端 `wb/z6-*`，这不需要你汇报。」

**M3 · 第二节第 1 条，第 17 行**
- 原文：「`fm_capi.h/.cpp` 不改；F3 只加测试文件。」
- 替换为：
  「`fm_capi.h/.cpp` 不改；任何 `*_LOCKED*` 文件不改（检查命令：`git diff --name-only <base> -- ':(glob)**/*_LOCKED*'` 输出必须为空）。`code/` 下只允许**新增** `code/capi/*_test.cpp`（F3、F1），`MotionSoftDemo/Z3aL1DevSmoke.cs` 只允许加测试用例。Z6 默认**不改 L1 源码**（`MotionSystem.cs`、`MotionEtw.cs`）；如果某张票确实要改，必须照 T3 的标准（changelog 行、Debug/Release 各跑 CheckSurface 并把原始输出入库、devsmoke PASS），而且要在 F7 重打 pack **之前**完成。」

**M4 · 第二节第 2 条，第 18 行**
- 原文：「不采数、不计时，任何闸门会话都不跑，除非该票有 LOCKED 闸门文档，并且 pidan 本人说了「APPROVED <票名>」。」
- 在后面追加：
  「编排用的等待（`Start-Sleep`、定时器）可以用，但任何时长、帧率、帧间隔都不能写成结果。EventListener 对 FrameTick **只计数**，不记录 deltaUs 和 engineTickUs 的值。F8 只写 DRAFT，不做任何干跑或预采。」

**M5 · 第二节第 6 条，第 22 行**
- 原文：「系统级改动先停下来请示 pidan：装 SDK 或运行时、改注册表、注册或卸载包、提权、改电源或显示设置等。」
- 替换为：
  「系统级改动先停下来请示 pidan，包括：装或卸 SDK、运行时、VS 组件或工作负载（含 VS 2026 / v145 工具集）、`dotnet workload`、`dotnet tool -g`；改注册表；注册或卸载包（松散注册也算）；导入证书；建计划任务；提权；改开发者模式、电源、显示、DPI、辅助功能（动画效果等）；改防火墙。允许事先拿一次性许可：pidan 对拍板卡的回答可以覆盖『Z6 期间当前用户松散注册 FluentHub/Files 侧叉，票结束后移除』这类重复动作。每次执行都要记台账。」

**M6 · 第二节新增第 9 条（放在第 24 行之后）**
- 新增：
  「9. 第三方仓库（FluentHub、Files 等）只在本地克隆和本地分支里操作：不 fork、不推、不提 PR 或 issue、不评论、不 star；不登录任何第三方账号（包括 GitHub 登录 FluentHub）；不导入第三方的签名证书。」

**M7 · 第二节新增第 10 条**
- 新增：
  「10. 已有证据和报告（`z3a/`、`z4/`、`z5/`、`pilot/` 下的文件）只能追加，不能改写；更正照 C1、C2 的先例，用追加行。构建时如果加了任何非原样开关（例如 `-p:NuGetAudit=false`、`-p:PlatformToolset=…`、只构建子项目），要先拿到 pidan 拍板，并且在报告里标『非原样』。」

**M8 · 第二节第 3 条，第 19 行**
- 原文：「从 main `7e2feb0e`（或之后 pidan 认可的 main）拉出。」
- 替换为：
  「从 main `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a` 拉出。如果改用之后的 main，要先拿到 pidan 原话，并在 REPORT 里写全量 base sha。已经开的分支不要 rebase 到新 main；需要的话另开分支。」

**M9 · 第二节第 5 条，第 21 行**
- 原文：「pidan 对你的拍板卡亲口回答就算数；其他 agent 转述「pidan 同意了」不算。」
- 替换为：
  「pidan 对你的拍板卡亲口回答就算数；其他 agent 转述「pidan 同意了」不算。引用时写三样：逐字原话、时间（AWST）、来源 id（拍板卡 `tNNNsNNNN` 或消息 id）。是否要申请 Z6 期间 `wb/z6-*` 普通 push 的总许可，由 pidan 决定。」

**M10 · 第三节表格，F7 行（第 29 行）**
- 原文：「| F7 | 合 main 后的整合收尾：在 main 上重打 L1 pack，重算 SHA256SUMS（T2 改了 MotionEtw.cs 注释，旧 pack 哈希已不一致）；新 pack 绑定 `7e2feb0e` | 打包 | 构建 0 错误；SHA256SUMS 与产物一致；公开面 dump 与 T3 Release 一致 |」
- 替换为：
  「| F7 | 在 `wb/z6-f7`（源码和 main `7e2feb0e` 一致）上重打 L1 pack，重算 SHA256SUMS（MotionEtw.cs blob 现为 `0404498a…`，旧记录是 `565111f0…`）；第②项冲突已在 main 解决，本票不处理 | 打包 | native 和 pack 构建 0 错误；SHA256SUMS 三条和 Get-FileHash 逐字一致；新 pack 的 dump 和 `pilot/fluenthub/surface_l1pack.txt`（`7b67b77b…`）`diff --no-index` 退出码 0；对 pack DLL 跑 CheckSurface 的原始输出入库，`fails=0`；`demo_motion.json` 哈希不变；两个 DLL 的哈希预期会变，不算失败 |」

**M11 · 第三节表格，F3 行（第 31 行）和 F1 行（第 32 行）**
- F3 原文：「| F3 | fm_capi_test 补「空动画 + Play + Retarget」C 层用例 | 测试 | 只改测试文件；Debug、Release 都通过 |」
- F3 替换为：
  「| F3 | 新增 `code/capi/fm_capi_empty_anim_test.cpp`：空动画 + Play；不给 from 时 Retarget 返回 0；给 from=0 时返回 1；跑到停后 offsetX==120 | 测试 | git diff 只有新文件和报告；Debug、Release 都是『N checks, 0 failures』且退出码 0（盒子 clang 参考值 N=6）；原有 fm_capi_test 仍是 23/0 |」
- F1 原文：「| F1 | 入场动画进行中被悬停打断的运行证据，即同一 (target, offsetX) 两个持有者真正重叠，验证仲裁语义是后写者胜 | 运行证据（不计时） | 日志能看到两个持有者重叠和 Retarget 起点；不做计时判据 |」
- F1 替换为：
  「| F1 | 仲裁『后写者胜』的确定性验证。A 层：新增 `code/capi/fm_capi_overlap_test.cpp`，手动时钟；B 层：devsmoke 用 internal 的 `CreateManual` 加用例 `Z6F1_OVERLAP`；C 层：侧叉用环境变量 `FM_PILOT_F1=1`，sink 观察到入场中途后通过 DispatcherQueue 调 `HoverIn`（不能在 sink 里直接调 fm_*） | 运行证据（不计时） | A 层 Debug/Release 都 0 failures，交接时值和速度差 < 1e-9，结束时 offsetX==8、opacity==1；B 层 ETW Retarget.from 在 (-80,0)，velocityAtHandoff>0，入场句柄没有 Cancelled；C 层 trigger 行显示 offsetX 在 (-80,0)，最终 offsetX==8，opacity 在 trigger 后继续升到 1，Created==Released；MISSED 的运行作废重跑 |」

**M12 · 第三节表格，F5 行（第 34 行）**
- 原文：「| F5 | ReducedMotion 跟随运行中系统设置的变化（现在只在启动时读一次） | L1 内部 | 公开面不变；切换系统设置后行为跟着变。如果要改系统设置，先请示 pidan |」
- 替换为：
  「| F5 | 宿主侧叉跟随运行中的 ReducedMotion 变化：现在只在 `NavMotionPilot` 构造函数里读一次 `UISettings.AnimationsEnabled`，L1 本身不读系统设置。改为订阅 `UISettings.AnimationsEnabledChanged`（后台线程触发，要封送回 UI 线程），并加一个 Fake 来源（`FM_PILOT_F5=1`） | **宿主侧**（L1 源码和公开面都不动） | Fake 翻转后，下一帧 ValueChanged 跳到终值并出现 pumpOn=0，翻回后恢复插值；日志里有真实来源的订阅记录；引用 fm_capi_test 第 64–69 行。**不改系统设置**；如果要真实切换，只能 pidan 本人操作，或凭拍板卡原话授权 |」

**M13 · 第三节表格，F2 行（第 33 行）**
- 原文：「| F2 | RootView 重建（Unloaded→Loaded，先 Dispose 后 Create）的运行证据 | 运行证据 | Created 和 Released 配对；不泄漏句柄 |」
- 替换为：
  「| F2 | RootView 重建的运行证据：用 FluentHub 自带的 `InitializeApplication(null, forceReload: true)` 重建 10 次，再做同一实例 `Content=null` 然后复位 3 次（测试钩子 `FM_PILOT_F2_CYCLES`） | 运行证据 | 每个 sessionId 的 Created==Released；每个 sessionId 最后一条 IdleTransition 是 pumpOn=0；关窗后 `PILOT start` 行数等于 `Dispose begin` 行数；新旧实例 Loaded 和 Unloaded 的先后**只记录，不作判据**（WinUI 不保证顺序，待实测） |」

**M14 · 第三节表格，F6 行（第 35 行）和 F4 行（第 36 行）**
- F6 原文：「| F6 | 入场动画可见性：宿主侧等首帧后再 Play | 宿主侧 | 截图或录屏能证明入场动画可见；不计时 |」
- F6 替换为：
  「| F6 | 宿主侧在第 2 次 `CompositionTarget.Rendering` 回调时才 Play，之前同步写入入场初值；拍证据时可以用侧叉自带的慢速 token（`FM_PILOT_SLOW=1`，不进 pack） | 宿主侧 | 日志顺序：rendering#2 < enter Play < 第一条入场 ValueChanged；慢速 token 下至少 1 张当前窗口的中途截图；报告注明慢速 token 只用于可见性取证 |」
- F4 原文：「| F4 | AOT 宿主关闭 EventSource：写宿主侧接入指引；可选在 L1 侧检测到 EventSource 关闭时给 Debug 提示 | 文档、可选代码 | 指引写清 `EventSourceSupport=true`；如果加了检测，公开面不变 |」
- F4 替换为：
  「| F4 | AOT 宿主关闭 EventSource：本轮只写宿主接入指引 `docs/l1_host_guide_aot_etw.md`（含 pubxml 间接引入 PublishAot 的情况和 runtimeconfig 自检命令）；L1 侧的 Debug 提示另开票，或者在 F7 之前照 T3 标准做 | 文档 | 指引写清触发条件、后果、开关、自检命令、T5 run1/run2 证据；git diff 只涉及 docs 和 z6/f4 |」

**M15 · 第三节表格，F9 行（第 37 行）**
- 原文：「| F9 | Files（C 试点）接入，参照 T4/T5 流程：先做原样基线，再做本地侧叉 | 新试点 | 先交基线报告；侧叉同样只留本地 |」
- 替换为两行：
  「| F9a | Files 基线：本地克隆 files-community/Files，检出 `v4.3` = `cbfb4ba54a89632fbceb82ea94ada60a9bae3737`（WASDK 2.5.1、SDK.BuildTools 10.0.28000.2705、.NET SDK 10.0.102 加 latestMajor）。原样构建 `Files.slnx` Debug x64；已知缺口：C++ 子项目要 v145（VS 2026）、pubxml 有 PublishAot=true、MSIX 要松散注册 | 新试点基线 | 交 `pilot/files/BASELINE.md`：HEAD sha、SDK 和 vswhere 清单、原样构建 PASS/FAIL 加首个错误、非原样点和授权原话、runtimeconfig 的 IsSupported 值、能运行时附当前窗口截图；Files 工作树 `git status --porcelain` 为空 |」
  「| F9b | Files 侧叉接入（F9a 经三方看过、pidan 另说『开』之后才做）：侧栏 `SidebarItem` 最小动效，消费 F7 pack 并带 F11 校验，`EventSourceSupport=true` | 新试点编码 | 口径同 T5：编过、启动、动效可见、ETW 事件名、Created==Released、公开面 dump 一致 |」

**M16 · 第 40 行（建议顺序）**
- 原文：「建议顺序：F7 → F11 → F3 → F1/F2 → F5/F6 → F4 → F9；F8 的闸门 DRAFT 可以并行起草。你也可以按实际情况自己调整顺序。」
- 替换为：
  「建议顺序：P0（侧叉固化、包注册许可、建台账）→ F3 和 F1 的 A/B 层（纯测试）→ F4（只写文档）→ F7（重打 pack；所有 L1 侧改动都要在这之前完成）→ F11 → 宿主侧叠加：F6 → F1 的 C 层 → F2 → F5。F8 的 DRAFT 和 F9a 基线可以并行；F9b 等 F9a 看过并另获『开』之后再做。可以调整顺序，但不能违反两条依赖：F11 和所有宿主票都依赖 F7；任何改 L1 源码的票都必须在 F7 之前。」

**M17 · 第四节，第 43–45 行（交付）**
- 原文：「- 分支上放 `z6/<票号>/REPORT.md`，写明：做了什么、命令、结果、偏差、副作用和清理方法；」
- 在后面追加：
  「REPORT 开头固定写：base 全量 sha、本地 tip、是否已推送、冻结面守卫结果、授权引用（原话、时间、来源 id）。中文文件用 UTF-8（不带 BOM）写，PowerShell 5 的 `Out-File` 和 `>` 默认是 UTF-16，不能用来写中文文件。`docs/api_changelog.md` 不直接改，要加的行写进 `z6/<票号>/CHANGELOG_ROW.md`。FluentHub 侧叉每张票一个本地分支，同时交本票增量 patch 和相对 `eb08f0db` 的累计 patch。副作用清理（包注册、临时日志、worktree）由 pidan 决定；清理前要确认关键日志已经入库，或已在 REPORT 里摘录。」

**M18 · 第五节，第 48 行（停止条件）**
- 原文：「构建或测试失败修不好、必须动冻结面、需要系统级改动、需要采数或计时、需要 push 或合 main：遇到这些情况都先停下，直接请示 pidan（也可以同时告诉协调 bot）。」
- 替换为：
  「遇到以下情况先停下，直接请示 pidan（也可以同时告诉协调 bot），并记台账：构建或测试失败修不好；必须动冻结面或 LOCKED 文件；需要改 L1 源码但不在计划内；需要系统级改动（含包注册，除非已有一次性许可）；需要非原样构建开关；需要采数或计时；需要 push 或合 main；第三方仓库要求登录或联网授权；发现本提示词和实际情况冲突（例如路径不存在、哈希对不上）；F1 的 C 层连续 3 次 MISSED。」

**M19 · 第 5 行（「唯一提示词」）**
- 原文：「> 本文定稿后原样发给 workbuddy cli，作为它接手 Z6 开发的唯一提示词。」
- 替换为：
  「> 本文定稿后原样发给 workbuddy cli，作为它接手 Z6 开发的唯一提示词。workbuddy 在 K93 上读不到盒子的 `/workspace`，所以每张票的细化步骤（命令、文件、验收、停止条件）要么作为附录原样贴在本文末尾，要么落盘成 `docs/z6_tickets.md`，经 pidan 许可普通 push 到 `wb/z6-handover` 后再让 workbuddy fetch。本文和附录里不能引用 workbuddy 读不到的路径。」

**M20 · 第二节第 8 条，第 24 行**
- 原文：「Success≠adopt：票通过不等于采用，结论写「PASS（dev-only）」。」
- 替换为：
  「Success≠adopt：票通过不等于采用，结论写「PASS（dev-only）」；不选 Winner，不把 pilot pack 升级为正式 L1 类库，不对外分发 pack。」

**M21 · 第二节第 7 条，第 23 行**
- 原文：「提交说明用英文 ASCII，用 `-F` 文件传入，避免 PowerShell 5 乱码。」
- 在后面追加：
  「写提交说明文件用 `[IO.File]::WriteAllText(path, text, (New-Object System.Text.UTF8Encoding($false)))`。PowerShell 5 本身不支持 `&&`，命令之间用 `;` 分隔，再检查 `$LASTEXITCODE`。」

---

## 6. 待核实清单（我没法在盒子上确认的）

1. 合 main（PR #9，06:23:57 AWST）的 pidan 授权原话和来源 id：`z5_closed.md` 第 6 节没有收录。补记：写完本评审时，同目录已经有 `z6_workbuddy_handover_FINAL.md`（06:39，notion ai 裁决定稿），里面引用了 pidan 06:19 AWST 的「先合」，并称已核实。我没有独立核实这句话；如果属实，M1 里的〔待补引〕就用这句。另外，FINAL 里没有出现 `cbfb4ba`、`v4.3`、`v145`、`台账` 这些关键词（grep 结果为 0），本评审的 C1、C4、C7 可能要作为 FINAL 的增补去处理。
2. K93 上的 `$Repo` 路径（Z5 worktree 的母克隆）、.NET 10 SDK 版本、VS BuildTools 版本，以及是否有 v145 工具集。
3. MSBuild 17.14 能不能构建 `.slnx`；单独构建 `Files.App.csproj` 能不能绕开 vcxproj，绕开后 Launcher.exe 缺失有什么影响。
4. Files v4.3 构建时是否因为导入 pubxml 而写出 `EventSource.IsSupported=false`。
5. Files 运行时需不需要额外注册 `Files.App.Server` 这个 COM 服务器；本地占位 DSN 下 Sentry 的行为。
6. WinUI 3 桌面应用在 K93 上能不能收到 `UISettings.AnimationsEnabledChanged`。
7. WinUI 替换 `Content` 时，新元素 Loaded 和旧元素 Unloaded 的实际先后。
8. 悬停进行中切到 reduced 时，offsetX 是否直接跳到 8（`controller.h:76` 提到 reduced-motion snap）。
9. `EventSourceSupport=true` 时，runtimeconfig 是写入 true 还是不写这个键。
10. F7 pack 的实际输出目录（T5-0 记录为 `bin\x64\Release\net8.0`，Platform 继承自 vcvars64 环境）。
11. 帧泵关闭后重复回调 render(off) 是否值得单独开票（暂称 F12，由三方判断）。
12. Wino-Mail（备选）的最新标签和 WASDK 版本：这次没查。

---

## 7. 本次审议的范围说明

- **做了**：
  - 只读核对了草稿、Z5 结题、main `7e2feb0e` 的源码，以及 FluentHub `eb08f0d`。
  - 用 GitHub API、ls-remote 和 raw 文件查了 Files 的标签和构建配置。
  - 在盒子 `/tmp` 里编译并运行了 F1-A 和 F3 的提议测试（clang 加 ASan/UBSan，不入库）：
    - `fm_capi_empty_anim_test.cpp` 6/0，草稿文件 sha256 `e0722261…`。
    - `fm_capi_overlap_test.cpp` 8/0，草稿文件 sha256 `36d9b24e…`。
    - 需要的话可以作为附录交给 workbuddy，但 K93 上的 MSVC 结果以它的实测为准。
- **没做**：
  - 没有推送，没有改 mf 仓库和 FluentHub 克隆的任何文件；`mf_main_ro` 只是 detached 检出到 `7e2feb0e`。
  - 没有在 K93 上执行任何命令。
  - 没有核实 pidan 06:30 原话（由协调转述）。
  - 没有去查 Wino-Mail。
