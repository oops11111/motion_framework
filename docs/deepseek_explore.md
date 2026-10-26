# DeepSeek Harness exploration — FluidMotion (post-patch review)

- **timestamp (Asia/Taipei):** 2026-10-19 10:24:22 Asia/Taipei (UTC+8)
- **note:** 对照上一轮 P1–P8 补丁复核 + WinUI3 小演示路线 + plan.md 里程碑压缩 + 3 个开放问题；目录 `C:\Users\admin\Temp\motion_framework_review\`（api.md/usage.md/plan.md/disputes.md/头文件/motion.json）
- **source:** DeepSeek Harness local web (HTTP RPC + WebSocket session/follow)
- **harness URL (auth token entry):** `http://127.0.0.1:27921/?token=84QouN-mJd_-dYdtJudJY646QbGFUozsAwrFHXwznCo`
- **RPC base actually used:** `http://127.0.0.1:54300/` (cookie authority / same process as :27921)
- **session id:** `session-65556d04-54b4-4b82-9a17-8b41fa1b23a2`
- **model:** deepseek-account / deepseek-v4-pro / reasoningEffort=max
- **prompt accepted:** yes
- **turn status:** completed (turn/end; replyChars=88557; frameCount=31280)

## User prompt

请阅读目录 C:\Users\admin\Temp\motion_framework_review\ 中更新后的 api.md、usage.md，以及 plan.md、disputes.md 与头文件。用中文完成下列四部分，写清楚、可执行：

1) **补丁复核**：对照上一轮建议的 P1–P8，逐条说明「已到位 / 部分到位 / 未到位 / 仍有漏网」。若有新漏网，给出可粘贴修订片段。
2) **WinUI3 可演示小样本技术路线**：从现有纯逻辑核心（spring/bezier/timeline/tokens/controller）走到一个可运行的 WinUI3 小演示，给出分阶段路线、每阶段交付物、主要技术风险与缓解。
3) **对照 plan.md 里程碑**：哪些可以压缩、哪些不能压缩（说明理由，引用里程碑编号如 M0–M5）。
4) **开放问题**：提出恰好 3 个供 Gemini / Claude / DeepSeek 下一轮讨论的开放问题（尖锐、可裁决）。

【上一轮 P1–P8 摘要，供你对照当前目录文件是否已改到位】
P1: usage.md 第3–6行测试口径（估计 + 本机无编译器）
P2: usage.md §5 清单第4、6条改为 Create(TargetId,...) 与 AnimationHandle::Retarget
P3: usage.md §6 限制一览补 HWND→V1.5、Rive不承诺帧数、帧预算流水线
P4: usage.md §4 C# 伪代码补时钟注释
P5: api.md §6 追加 EmbedToHwnd/HWND → V1.5
P6: api.md §8 末尾补「估计/本机未编译」说明
P7: api.md §1.1 SpringParams 措辞改为「由 Spring 构造时钳制」
P8: 扁平目录相对路径/随附 engineering.md 或改引用

请先用工具实际阅读目录内文件，再回答；不要凭旧印象。


## Assistant reply (verbatim)

Now I have the previous round's full P1–P8 text. Key facts established:

1. Previous round (P1): recommended usage.md line 3-6 mention "本机尚无 C++ 编译器、尚未本地编译验证". Current usage.md now says "本机曾用 g++ 复验通过，以本机最近一次跑测为准". And plan.md line 68 now says "早期本机一度无编译器；其后已安装 g++ 并复验 timeline 419 / tokens 617 / controller 236". So the docs now claim a NEW fact: g++ was installed and re-verified. On THIS machine, g++ is not found on PATH nor in common roots. So the claim "本机曾用 g++ 复验通过" is currently unverifiable and, by my check, g++ isn't even present now. This is a significant finding for P1/P6 review: the fix replaced "no compiler" with "re-verified with g++" — a stronger claim that (a) cannot be reproduced from this directory (no tests, no logs), (b) contradicts my check that g++ isn't installed on this machine (no g++.exe anywhere in common locations, not on PATH).

Wait — careful. The machine where "本机" refers — the review is happening on C:\Users\admin\Temp. The docs say 本机 (this machine). My check: g++ not on PATH; no g++.exe in C:\msys64, C:\mingw64, C:\mingw32, C:\TDM-GCC-64, C:\Program Files, C:\ProgramData\chocolatey, C:\tools, C:\Strawberry (only the ones that exist were searched — C:\Program Files may be huge but the search returned nothing). The command exited 1 because `Get-Command clang++/cl` failed (no output). Actually the exit code 1 came from the last Get-Command failing. Let me be careful: the output shows "no g++.exe found in common roots" then "----" then exit code 1 (because Get-Command cl failed and returned non-zero, which propagated). So clang++, cl, cmake, ninja — none found on PATH either. So this machine currently has no C++ compiler toolchain on PATH. g++ could be somewhere unusual but we can't verify; the docs' claim of "本机曾用 g++ 复验通过" is not corroborated by any artifact in the directory (no test sources, no build logs, no results file).

This is a strong, concrete finding: P1/P6 got a wording fix but the new wording asserts an even stronger claim (本机复验通过) that is unverifiable in this directory and inconsistent with the current machine state (no compiler found). I should report it precisely: "已到位一半" or "部分到位" — the 估计 markers are in place, but "本机曾用 g++ 复验通过" is a new unverifiable claim. And plan.md §8 says it as fact ("本机 419 项全过" etc. — line 68: "本机传 motion.json 参数时 617 项全过", "本机 236 项全过", "本机 419 项全过" in §8 lines 67).

Let me re-check plan.md line 67-68:
- Line 67: "...已交付 → code/timeline.h（本机 419 项全过）；...code/tokens.h + motion.json（本机传 motion.json 参数时 617 项全过）；...code/controller.h（本机 236 项全过）；5）重定向控制器；..."
- Line 68: "spring.h 已经应用 Claude 自查发现的两处修复... **早期本机一度无编译器；其后已安装 g++ 并复验 timeline 419 / tokens 617 / controller 236。**"

So plan.md §8 has both "本机 X 项全过" claims AND the note about installing g++. Duplicate "5）" is still there (line 67: "5）可打断控制器与重定向，已交付 → code/controller.h（本机 236 项全过）；5）重定向控制器；") — the duplicate numbering leak from the previous round is still present. Previous round also flagged plan.md §4 rows 3/4/5 lacking "已交付" markers vs §8. Let me check plan.md §4 table now: rows 3 (时间线 4-6 人周, Claude), 4 (Token 3-4), 5 (控制器 4-6) — no "已交付初版" markers. Rows 1 and 2 have "**Claude**（已交付初版）". So still inconsistent. These are "新漏网/旧漏网仍在" items beyond P1-P8.

Also, previous round's minor precision items in api.md §5.4 and §5.7 (in-flight includes Paused; Reverse zero-duration also completes immediately): let me check current api.md:
- api.md §5.4 line 468: "on→true：对每个活跃控制器 SnapToEnd（in-flight 含 Playing/Paused：正向到终点、反向到 0，并 Completed）" — FIXED (includes Playing/Paused and 反向到 0). ✓
- api.md §5.7 line 516: "零时长时间线在 Play / Reverse 时立即完成（跳过 markers）" — FIXED (mentions Reverse). ✓

Good — those two were also fixed beyond the P1-P8 list.

Now current P-by-P status with line evidence:

P1 (usage.md 第3–6行): Current lines 3-6:
```
> **前提**：核心在 `code/*.h`（header-only C++17，命名空间 `motion`）。  
> **测试状态**：timeline 419 / tokens 617 / controller 236 项在实现方 Linux 环境通过（**估计**）；本机曾用 g++ 复验通过，以本机最近一次跑测为准。  
> **未接线**：Windows 合成器 / WinUI / C# 绑定 — **规划中**；下文 C# 仅为「规划伪代码」。  
> 估计与待真机项单独标明。
```
Verdict: 部分到位。The 估计 marker and "待真机项单独标明" are in. But instead of P1's recommended "本机尚无编译器、尚未本地编译验证", it now asserts "本机曾用 g++ 复验通过" — a stronger claim with zero artifacts in the directory (no test sources/logs) and no compiler currently discoverable on this machine. The claim may be true (maybe g++ was installed and removed, or it's elsewhere), but as a deliverable it's unverifiable. Recommend either attach test sources + result log, or revert to "本机未复验" wording. Patch snippet to provide.

P2 (usage.md §5 清单第4、6条): Current lines 276, 278:
- "4. `MotionSystem::Create(TargetId, tokens, transition)` → 设回调 → `Play`。" ✓ exact recommended text.
- "6. 交互打断用 `AnimationHandle::Retarget`（勿直接依赖 `Player::Retarget`，见 api.md §3.6 / §7.1）；取消用 `AnimationHandle::Cancel`。" ✓ exact.
Verdict: 已到位.

P3 (usage.md §6): Current lines 290-292 contain HWND→V1.5, Rive 不承诺帧数, 帧预算流水线 rows, all with disputes.md/plan.md references. ✓ Also row 287 test status now says "实现方环境 419/617/236 项通过（估计）；本机 g++ 曾复验通过，以最近一次跑测为准". And 闲置 0% CPU row includes "≤0.1% 为 Gemini 口径". Verdict: 已到位 (for the three required rows). Note: P3's recommended row said "本机无编译器，尚未本地复验" — again the shipped version asserts g++ 复验 instead. Same caveat as P1.

P4 (usage.md §4 C# 伪代码补时钟注释): Current lines 256-257:
```
// var system = MotionSystem.Create(dispatcher);   // 规划：C++ 侧为 MotionSystem(IClock&, IDispatcher*)，
//                                                 // 投影需自带/注入时钟，此处仅示意，勿当作最终签名
```
✓ exact recommended text. Verdict: 已到位.

P5 (api.md §6 追加 EmbedToHwnd/HWND → V1.5): Current api.md line 535: "| `EmbedToHwnd` / HWND 宿主正式 API | **V1.5**；首发仅随附不受支持的参考示例（`disputes.md` §1） |" ✓. Verdict: 已到位.

P6 (api.md §8 末尾补「估计/本机未编译」): Current api.md line 560: "> 用例数来自实现方 Linux 环境与本机 g++ 复验；上表为**估计**，以本机最近一次跑测为准。详见 [`deepseek_review.md`](./deepseek_review.md)。" Verdict: 部分到位 — "估计" present, but instead of "本机未编译" it claims "本机 g++ 复验" (same issue as P1); AND the "详见 deepseek_review.md" link points to a file that does not exist in this directory. Also §8 heading now says "（估计，本机 Linux）" which is good.

P7 (api.md §1.1 SpringParams 措辞): Current lines 35-36: "由 Spring 构造时钳制为 >= 1e-4" / "由 Spring 构造时钳制为 >= 0（负值会发散）" ✓ exact. Verdict: 已到位 (and verified against spring.h:22-23).

P8 (扁平目录相对路径 / 随附 engineering.md): Current state:
- usage.md:3 "核心在 `code/*.h`" — still says code/; files are flat.
- usage.md:8 "架构背景见 [`engineering.md`](./engineering.md)" — file absent; and mixed style (./ vs ../).
- usage.md:267 "Rive 指针跟随延迟见下方限制表与 `../disputes.md`" — ../disputes.md points outside this dir.
- usage.md:296: `./engineering.md`, `../plan.md`, `../summary.md`, `../disputes.md`, `./deepseek_review.md` — 3 of 5 broken paths + 3 missing files.
- api.md:3 "本文档只描述 `code/` 下已实现的 C++17 头文件" — code/ not present.
- api.md:286 "`docs/engineering.md` / 计划中明确" — docs/ not present.
- api.md:358 "示例文件：`code/motion.json`" — code/ not present (motion.json is flat here).
- api.md:560 "`./deepseek_review.md`" — absent.
- plan.md:73-78 文档索引 all `docs/...` — absent.
Verdict: 未到位. Provide patchable snippets.

New leaks beyond P1–P8 (仍有漏网):
N1. plan.md 测试口径仍与 usage/api 不一致: plan.md §8 lines 67-68 state "本机 419 项全过" / "本机传 motion.json 参数时 617 项全过" / "本机 236 项全过" as fact, and claim "其后已安装 g++ 并复验" — no 估计 qualifier in §8, contradicting plan.md's own line 3-4 (所有估计) and usage.md/api.md's 估计 wording; plus no test artifacts anywhere. Also plan.md:68 "本机 236 项全过" vs api.md §8/usage.md §6 table numbers 419/617/236 — consistent numbers, but the claim strength differs. Patch: add "（估计，无随附测试产物）" or cite the log.

N2. plan.md §8 duplicate numbering "5）...5）重定向控制器" still present (line 67), and §4 table rows 3/4/5 still lack "已交付初版" markers that rows 1/2 have. Previous round flagged both; not fixed. Patch: fix numbering and add markers.

N3. usage.md:4/usage.md:287/api.md:560 claim "本机曾用 g++ 复验通过" — new unverifiable claim replacing the honest "本机无编译器". My machine check: no g++/clang/cl on PATH, no g++.exe under common roots. If the docs are to be the deliverable, either (a) 随附测试源码 + 跑测日志/命令, or (b) 改回 "本机未复验". Patch provided.

N4. instant 规则实现两遍: controller.h:111 MakeInstantTimeline (free function, documented in api.md §5.2) vs tokens.h:707 Compiler::MakeInstant (static, undocumented) — same semantics duplicated; docs only mention one. Minor; suggest api.md note that both exist, or unify. Actually the header comment says "kept separate so tokens.h is unchanged" — intentional. Documented in api.md §5.2: "与 token 的 "instant" 同规则；独立函数以免改 tokens.h" — that does mention it. So it's documented. Not a leak, skip or mention as observation only.

N5. usage.md:4 "本机曾用 g++ 复验通过，以本机最近一次跑测为准" — "最近一次跑测" implies a re-run is possible; no test harness shipped. Same as N3; fold together.

N6. Check api.md §0 line 15: "已实现 | 头文件中有定义，Linux 单元测试覆盖（估计，以本机跑测为准）" — has 估计 ✓ fine.

N7. disputes.md line 15: "UI 线程中框架自身的部分约占 12%，120Hz 下稳态 ≤1ms，动画启动时 ≤2ms（Gemini 的结论写作 ≤1.5ms）" — internal inconsistency: 12% of 8.33ms ≈ 1.0ms, fine; "Gemini 的结论写作 ≤1.5ms" vs "启动时 ≤2ms" — the parenthetical seems to say Gemini's number was 1.5ms while the adopted text says 2ms. That's a pre-existing wrinkle in disputes.md; usage.md §6 doesn't repeat these absolute numbers (it says 每阶段单独 ≤T) so no new leak introduced. Skip (disputes.md wasn't in scope of P1–P8 modifications; and it's "结论写作" vs adopted — arguably intentional recording of disagreement).

N8. api.md §5.2 ControllerSnapshot 与 header: api.md lists fields id/target/timeline/position/rate/reversed/playing/loops/clockTime/overrides/ownedProperties — header matches ✓.

N9. api.md §3.2: "Curve::Bezier(double...) // x 钳制到 [0,1]" ✓.

N10. One more possible new leak: usage.md §1.2 code `auto tokens = TokenSet::Parse(buf.str(), errors); if (!tokens) {...}` — Parse returns optional<TokenSet>; `if (!tokens)` ✓; then `tokens->Duration("normal")` ✓ (uses ->; line 65 `tokens->Duration`). ✓ correct.

N11. usage.md line 79: `sys.Create(kCard, *tokens, "card.enter")` ✓.

N12. usage.md §1.3 line 97: `std::make_shared<const Timeline>(Parallel({...}))` — Parallel returns NodePtr; Timeline(NodePtr) ✓. `Animate(Track("opacity", 0.0).To(1.0, 0.2, Curve::Bezier(0.1, 0.9, 0.2, 1.0)))` — To returns Track&, Animate takes Track by value ✓.

N13. api.md §4.3 durations range [0,600] ✓ tokens.h kMaxDuration=600 ✓.

N14. api.md §5.6: "终态（Cancelled / Superseded）会清空用户回调" ✓ matches Terminate.

N15. usage.md §1.6 line 145-149 comments ✓.

Now — should I also verify compilation? No compiler available on this machine. I can note that as the concrete fact backing the review: cannot re-verify locally, which strengthens the argument about "本机复验" wording. (The bundled workspace dependencies might include compilers? load_workspace_dependencies mentions Node.js/pnpm, not C++ compilers. Not worth checking for g++.)

One more check on usage.md line 4 vs plan.md line 68: plan.md says g++ installed & re-verified. usage.md says "本机曾用 g++ 复验通过". These are consistent with each other but both unverifiable. api.md §8 says "本机 g++ 复验". OK.

Also check usage.md §6 row 1: "实现方环境 419/617/236 项通过（估计）；本机 g++ 曾复验通过，以最近一次跑测为准" — same issue.

Also note disputes.md line 4: "文中所有人周数和市场占比都是两个模型的估计，未经核实" — good practice; plan.md line 3 similar.

Now Part 2 — WinUI3 demo roadmap. Concrete plan:

总体策略: 复用现有纯逻辑核心的全部能力（软件路径 + IDispatcher 缝），分 4 个阶段，总 2-3 人周；不做 WinMD/C# 投影（那是任务 7 的 5-7 人周），演示用 C ABI 桥（extern "C" 薄封装）或纯 C++/WinRT 工程。

阶段 0（0.5 天）: 环境验收 — Windows SDK + C++/WinRT VS 模板 + 可编译的空白 WinUI 3 应用。风险: SDK/工具链版本; 缓解: 用 VS2022 官方 WinUI3 template.
Wait — but本机无编译器 (no cl/g++ found). For the demo roadmap this is about the engineering team, fine. Actually I can mention: 前置条件是装 Visual Studio 2022 + Windows App SDK workloads.

阶段 1（1 天）: 纯逻辑 golden demo（不依赖 UI）: 控制台程序 + 记录型 Dispatcher（把每帧 Apply 落 CSV），跑 motion.json 的 card.enter / button.press / list.stagger-in + Retarget 打断，输出 golden CSV。交付物: demo_core.exe + golden.csv + README。作用: 把 419/617/236 之外做一条端到端「可看」的数据流，验证 token→controller 全链，并可作性能冒烟。风险: 无（纯头文件）。

阶段 2（2-3 天）: WinUI3 软件路径演示 app:
- C++/WinRT 工程（或 C# app + C++ DLL with C ABI）。我建议两条选一，推荐 C++/WinRT 单工程（避免投影问题）或者 C# + C++/CLI? No — C++/CLI is dead; use C# + C ABI DLL via [LibraryImport]. 实际上更简单: C++/WinRT blank app, 直接 #include 头文件（header-only），用 DispatcherQueueTimer 或 CompositionTarget.Rendering 驱动 Tick。
- UI: 一个卡片（enter/exit）、一个按钮（press 弹簧）、一个列表（stagger）、一个可拖拽小方块（Retarget 跟手 + 松手弹簧回落）。
- IDispatcher::Apply 写 XAML 属性: Translation (offsetX/Y 需映射), Opacity, Scale (ScaleTransform 或 UIElement.Scale property — WinUI3 UIElement 有 Scale/Translation 吗? UIElement has CompositeMode, and there's UIElement.Translation (Vector3) via TransitionCollection... In WinUI 3 (UWP XAML), UIElement has Scale and Translation properties (the "composition" properties on UIElement: UIElement.Scale, UIElement.Translation, UIElement.Rotation, UIElement.CenterPoint — yes, these exist on UIElement in UWP/WinUI). So Apply 直接把 samples 写到 UIElement.Scale = new Vector3(s,s,s), Translation.Y, Opacity.
- clock: SteadyClock + DispatcherQueueTimer(60Hz) 或 CompositionTarget.Rendering（跟随 vsync, 120Hz 屏自动 120Hz）。
- 演示点: 打断保留速度（拖拽中触发 enter 再点 exit）、reduced motion 开关（UISettings.AnimationsEnabled 挂钩）。
交付物: 可运行 app + 一段录屏 + README（如何构建运行）。
风险: C++/WinRT 模板构建复杂度、UIElement 属性映射（offsetY→Translation.Y 单位 DIP ✓）、计时器抖动。缓解: 用 CompositionTarget.Rendering 而非计时器；属性映射表硬编码 3-4 个属性。

阶段 3（3-5 天）: 合成器下放「演示级」实现（mini-CompositionDispatcher）:
- 实现 IDispatcher::Submit: 对 Timeline 的 PlacedTrack 列表 lowering——Linear/Bezier 段 → CompositionObject.StartAnimation(CubicBezierEasingFunction + KeyFrameAnimation); Spring 段 → SpringVector3NaturalMotionAnimation (或 SpringScalarNaturalMotionAnimation — 实际是 ScalarNaturalMotionAnimation/Vector3...)。WinUI 的 Compositor 有 CreateScalarKeyFrameAnimation, CreateCubicBezierEasingFunction, CreateSpringVector3NaturalMotionAnimation, CreateSpringScalarNaturalMotionAnimation。
- Retarget override → ExpressionAnimation 或直接重发 natural motion animation。
- 演示: 开关「软件 / 合成器」对比，显示 UI 线程帧耗时与 CPU。
- 这正是 M0/M1 要真机验证的参数对应问题，demo 就是验证脚手架。
交付物: hybrid demo + 参数对照表(Composition spring 参数 vs SpringParams response/damping 映射) + 已知差距清单。
风险: ①参数映射无把握（Period→response, DampingRatio→damping 需要换算公式: 无阻尼周期 T = 2π/ω0, ω0=√(k/m); Composition 的 SpringVector3NaturalMotionAnimation 参数是 Period 和 DampingRatio — Period 应该直接对应 response (无阻尼周期), DampingRatio 对应 damping。这需要真机验证); ②bezier y 超出 [0,1]（emphasized 的 y2=1.0 是边界内; 但 y1/y2 可超界时 CubicBezierEasingFunction 是否接受 — disputes 里已列待验证）; ③所有权切换（last-wins）在合成器侧的 Withdraw/接管顺序; ④合成器动画与 Tick 双写冲突（lowered 属性 Apply 要忽略 — IDispatcher 注释已说明 backend 可忽略）。缓解: 每动画可回退软件路径; 只在单一属性上试点。

阶段 4（1-2 天）: 设计师闭环演示: motion.json 热重载（FileSystemWatcher → Parse → 重建 TokenSet，验证 Diagnostic 报错展示）、reduced motion 绑定系统开关、演示打包（视频 + 说明）。
交付物: 录屏、demo README、开放问题清单。

总时长: 约 1.5-2 人周（不含 M0 真机验证结论依赖）。

主要技术风险与缓解汇总表:
1. 工具链: C++/WinRT 工程搭建/构建失败率高 → 用官方模板; 或退路: C# + C ABI DLL（.h 打包进 DLL, extern "C" API ~10 个函数）。
2. 合成器参数对应（Period/DampingRatio ↔ response/damping; CubicBezierEasingFunction 对 y 超 [0,1]）→ 阶段 3 做成对照实验, 记录数据, 不放进 1.0 承诺; 这正是 disputes 待验证清单。
3. 120Hz 演示环境（演示机需 120Hz 屏 + 可变刷新率）→ 提前预定硬件; 软件路径在 60Hz 也能演示。
4. 属性所有权 last-wins 在合成器侧的状态机复杂度 → demo 只演示单 target 少数属性; 完整状态机留给模块 6。
5. Tick 与渲染相位: CompositionTarget.Rendering 回调在 UI 线程、vsync 前 → 保证 Apply 同帧生效; 跨线程（渲染线程）不做。
6. 无编译器现状（本机核查: PATH 无 g++/clang/cl）→ 先装 VS2022 Build Tools + Windows SDK; 或委托有环境的队友。

Deliverables per phase 明确列出。

Part 3 — plan.md 里程碑压缩分析:

M0（第 2 周，4-6 人周）: **不可压缩**。理由: 它是全计划唯一的 go/no-go 门（"作出是否继续的决定，并重新核算估计"）；其结论（Rive 进合成表面、ALLOW_TEARING、指针属性集可用性、实测延迟）直接决定任务 6/8/9 的方案与「Skia 备选 +4-8 周」是否触发；且 4-6 人周里大部分是硬件/驱动/实测等待时间，非纯编码可并行摊薄。压缩 M0 = 用未验证假设押注后续 30 周。
M1（第 10 周）: **可压缩（部分）**。理由: M1 验收两半——「数学核心、时间线、token 无界面跑通单测」已由已交付初版（任务 1-5）覆盖大半；剩「分发器降级 + 真机验证弹簧参数对应」。压缩点: 任务 1-5 的收尾/评审可从关键路径上拿掉（已成事实），分发器（任务 6）与 M0 结论并行开工（降级规则 Claude 写、集成人等 M0）。不可压部分: 「真机验证 SpringVectorNaturalMotionAnimation 参数对应」依赖 M0 硬件与结论，无法提前。预计可回收 2-3 周，但里程碑日期建议不动、把回收时间转给 M2 的风险缓冲——因为 M1 是后续所有模块的地基，压缩它换来的日期承诺会被真机项吃回。
M2（第 16 周）: **可微压缩（1-2 周）**。理由: ETW 事件定义（任务 10, Claude 写定义）可与任务 6 并行; C# 投影样板（任务 7）的接口面在任务 4/5 完成后即稳定，可提前开工; 预览宿主 v0 的验收可砍到「token 热重载 2 秒内上屏」最小集（Gemini 1.5 秒要求留到 M4）。不可压: 性能 CI ≥2 台物理机（采购、装机、基线采集有硬性日历时间）；预览宿主/Figma 插件 8-10 人周本身是 M2 最大块，压缩它只能砍范围不能砍时间。
M3（第 22 周）: **不可压缩**。理由: Rive 宿主 8-12 人周是全计划风险最高、最不可并行摊薄的任务（渲染线程、DPI 重光栅化、设备丢失均需逐项踩坑）；试点提前到 22 周接入已是采纳 Gemini 意见后的激进值; 混合刷新率/DPI 矩阵实测需要多台真机排队。任何压缩都直接转化为 M4/M5 的返工或试点 1 的延期。
M4（第 28 周）: **不可压缩**。理由: P95/P99 达标验证依赖性能 CI 持续运行数周的基线数据（"至少 1000 帧、跑多次"），瓶颈是日历时间不是人周; 无障碍检查需外部评审 0.2 人排期; HWND 参考宿主冒烟（任务 14）虽只 1-2 人周但依赖任务 6。压缩 M4 的常见错误是把「绝对上限 + 相对回退」双门禁改成单门禁——disputes.md §2 已裁决双卡，改即违约。
M5（第 32-34 周）: **不可压缩（作为缓冲反而要守住）**。理由: 试点 2 是「更重或更老的应用」作对照，联调时长不可预测; 「无未关闭 P0/P1」是硬门槛; 且 Skia 备选（+4-8 周）如果触发, M5 的 32-34 周窗口是唯一吸收点。压缩 M5 等于取消全计划唯一的缓冲。

总结: 可压缩 = M1（数学核心已交付带来的事实提前）+ M2（范围裁剪式微压）; 不可压缩 = M0/M3/M4/M5（外部依赖/日历时间/质量门槛）。并给出一条「压缩后如何用」的建议: 把 M1/M2 回收的 3-5 周计入风险缓冲, 而不是直接前移 M3-M5 承诺日期。

Part 4 — 3 open questions (尖锐、可裁决):

Q1 (测试证据门槛): 「本机曾用 g++ 复验通过」三文档已写入, 但本目录没有任何测试源码、跑测命令或结果日志, 且经查当前机器 PATH 与常见安装位置均无 g++。请裁决: (a) 测试源码与日志必须随交付物入库, 否则「已实现」标注降级为「已交付初版、待复验」; (b) 维持现状、以实现方 Linux 结果为唯一证据, 本机复验声明删除。二选一, 下一轮后所有文档口径统一。

Q2 (M1 重定义与提前): 数学核心任务 1-5 已交付初版, M1 剩余实质只有分发器 + 合成器参数真机验证。请裁决: 是否把 M1 拆为「M1a 纯逻辑（判定: 已过）」与「M1b 合成器真机对应」, 并让任务 6 的分发器降级规则与 M0 并行开工, 把 M2 的 ETW/C# 投影提前——从而把 M2 压缩 1-2 周? 还是坚持 Claude 的串行结构（M0→分发器→Rive→指针→试点）, 压缩收益全部转为缓冲而不动承诺日期?

Q3 (演示准入标准): WinUI3 小演示是否允许第一阶段纯软件路径（UI 线程 Tick + Apply 写 UIElement.Scale/Translation/Opacity）即视为「可演示」, 还是必须包含至少一条合成器下放（KeyFrame/SpringNaturalMotion）才算? 若必须下放, 第一个试点动画选哪个（建议: button.press 的 springTo 或 card.enter 的 opacity bezier）, 且 Period/DampingRatio ↔ response/damping 的换算公式谁来先给一版（Gemini 给 WinUI 文档依据, Claude 给推导, DeepSeek 复核）?

These 3 are sharp and decidable. Good.

Now let me also double-check line numbers for citations in current files:

usage.md:
- L3: 前提 code/*.h
- L4: 测试状态 line
- L5: 未接线
- L8: engineering.md link
- L267: ../disputes.md
- L276: §5 item 4
- L278: §5 item 6
- L287-294: §6 table rows
- L296: 更多架构与里程碑 links

api.md:
- L3: code/ 范围
- L15: 已实现 估计
- L35-36: SpringParams comments (P7) — actual lines: 35 "double response = 0.35;  // 无阻尼周期（秒）；由 Spring 构造时钳制为 >= 1e-4" and 36 "double damping  = 0.8;   // 阻尼比；由 Spring 构造时钳制为 >= 0（负值会发散）"
- L286: docs/engineering.md
- L358: code/motion.json
- L468: SetReducedMotion comment (in-flight 含 Playing/Paused... 反向到 0) — fixed
- L516: §5.7 零时长 Play/Reverse — fixed
- L535: EmbedToHwnd row
- L560: §8 末尾 note

plan.md:
- L67: §8 已交付 list with duplicate 5)
- L68: g++ 安装与复验 claim
- L73-78: 文档索引 docs/ links
- §4 rows 1-5: rows 1,2 have 已交付初版; rows 3,4,5 don't.

disputes.md: L10 裁决; L13-16 帧预算; L28 Rive 不承诺帧数.

spring.h: L22-23 clamp (P7 verification).
controller.h: L293-302 Create overloads; L111 MakeInstantTimeline.
timeline.h: L415-426 Player::Retarget.
tokens.h: L282 Parse; L724 errors.clear().

For new-leak patches, I'll give concise paste-able snippets:

Patch for N1/N3 (plan.md L67-68 + usage.md L4 + api.md L560 口径统一). Provide one replacement for usage.md L4:

```markdown
> **测试状态**：timeline 419 / tokens 617 / controller 236 项为实现方 Linux 环境报告数（**估计，未随附测试源码与日志**）；「本机 g++ 复验」声明当前无法在本目录复现（目录内无测试产物，本机 PATH 亦无 g++），在随附测试源码与跑测日志前，本机状态按「未复验」标注。
```

And plan.md L68 replacement:

```markdown
- `spring.h` 已经应用 Claude 自查发现的两处修复：特大阻尼比时数值精度丢失（改用两根之积计算）、阻尼比下限钳制为 0。**早期本机一度无编译器；其后声称已安装 g++ 并复验 timeline 419 / tokens 617 / controller 236，但测试源码与日志未随附，本机复验状态待证实。**
```

api.md L560 replacement:

```markdown
> 用例数来自实现方 Linux 环境；「本机 g++ 复验」无随附测试产物可核验。上表为**估计**，以随附测试源码在本机实跑为准（当前本机无编译器，未复验）。
```

Hmm — but careful: I shouldn't overclaim that the machine never had g++. I verified: PATH 无 g++/clang/cl/cmake/ninja; common roots 无 g++.exe. That's current state. The doc says "曾用 g++ 复验" (past tense). It's possible someone installed g++ earlier and removed it. My finding: 当前无法复现/无法核验 — that's accurate and fair. State it that way.

Patch for N2 (plan.md duplicate numbering):

```
5）可打断控制器与重定向，已交付 → `code/controller.h`（本机 236 项全过）；6）重定向控制器；7）能力分发器的降级规则；...
```
Wait the current numbering: "5）可打断控制器与重定向，已交付 → code/controller.h（本机 236 项全过）；5）重定向控制器；6）能力分发器的降级规则；7）ETW 事件定义；8）测试工具与 CI 分析脚本；9）技术验证脚手架" — so renumber: 5）可打断控制器与重定向（已交付）；6）能力分发器的降级规则；7）ETW 事件定义；8）测试工具与 CI 分析脚本；9）技术验证脚手架。And remove the duplicate "5）重定向控制器；" (retarget is already covered by item 5). Patch:

```markdown
5）可打断控制器与重定向，已交付 → `code/controller.h`；6）能力分发器的降级规则；7）ETW 事件定义；8）测试工具与 CI 分析脚本；9）技术验证脚手架（由人运行并解读结果）。
```

And §4 rows 3/4/5: add 「**Claude**（已交付初版）」:

```markdown
| 3 | 时间线、轨道、关键帧图，时钟抽象 | 1 | 4-6 | **Claude**（已交付初版） |
| 4 | Token schema、校验器、加载器 | 1、3 | 3-4 | **Claude**（已交付初版） |
| 5 | 可打断控制器、句柄、重定向 | 1、3、6 | 4-6 | **Claude**（已交付初版），人评审 |
```
Wait row 5's dependency includes 6 (能力分发器)? "5 | 可打断控制器、句柄、重定向 | 1、3、6" — controller depends on 6? The controller.h includes tokens.h/timeline.h only; the dispatcher seam is in controller.h but module 6 plugs into it. Dependency "6" on row 5 looks odd but that's plan.md's table; not my business to fix unless flagging. Actually interesting: row 5 depends on 6 (能力分发器) yet controller was delivered without it. I could note this dependency edge as questionable — controller.h already defines IDispatcher seam without module 6, so row 5's dependency on 6 seems reversed (6 depends on 5's seam). This is a genuine plan-level observation worth one line in part 3: 任务 5 的依赖列写「1、3、6」但 controller.h 已交付且只依赖 timeline/tokens——依赖边 5→6 应改为 6→5。Good catch, include as a small new leak / plan inconsistency.

Patch for P8 (usage.md L8, L267, L296; api.md L3, L286, L358, L560; plan.md L73-78):

usage.md L8:
```markdown
配套 API 清单见 [`api.md`](./api.md)。架构背景见 `engineering.md`（**未随附本目录**）。
```

usage.md L267:
```markdown
真实接线 **待真机验证**（DWM / `Windows.UI.Composition` 参数对应关系）。Rive 指针跟随延迟见下方限制表与 [`disputes.md`](./disputes.md)。
```

usage.md L296:
```markdown
更多架构与里程碑：[`engineering.md`](./engineering.md)（未随附）、[`plan.md`](./plan.md)、[`summary.md`](./summary.md)（未随附）、[`disputes.md`](./disputes.md)、[`deepseek_review.md`](./deepseek_review.md)（未随附）。
```

api.md L3:
```markdown
> **范围**：本文档只描述随附头文件（`code/` 或本目录，header-only C++17）中已实现的公开接口。
```
Better: "本文档只描述已实现的 C++17 头文件公开接口（本目录内的 `*.h`，原仓库位于 `code/`）。"

api.md L286: `docs/engineering.md` → `engineering.md（未随附本目录）`.
api.md L358: `code/motion.json` → `motion.json（本目录随附）`.
api.md L560: deepseek_review.md → 标注未随附（与 P6 patch 合并）。
plan.md 文档索引: docs/... → ./... 或标注未随附。

OK. Also double-check one subtle thing for P2: usage.md §5 item 4 says "MotionSystem::Create(TargetId, tokens, transition)" — the actual param is string_view transition; fine as shorthand.

Also check the claim in api.md §8 heading "（估计，本机 Linux）" — "本机 Linux"? The machine is Windows (C:\Users\admin...). "本机 Linux" is odd phrasing — probably means "Linux（本机）". Previous round heading was "## 8. 测试覆盖（估计，本机 Linux）" — hmm, was that there before? Previous round cited api.md §8 第 548 行「估计，以本机跑测为准」; the heading presumably said something similar. The phrase "本机 Linux" on a Windows machine is contradictory — this machine is Windows, so "本机 Linux" cannot be right; tests ran on Linux (WSL? or the implementer's Linux). Minor new leak: api.md §8 标题「（估计，本机 Linux）」把「本机」和「Linux」并置，与当前 Windows 主机矛盾，建议改为「（估计，Linux 环境）」。Nice catch — include as small item.

Also usage.md §6 row 1: "实现方环境 419/617/236 项通过（估计）；本机 g++ 曾复验通过，以最近一次跑测为准" — same issue; covered.

Now, also confirm P3's new rows vs previous recommendation: previous P3 recommended 帧预算 row text "每阶段单独 ≤1/T" — current shipped "每阶段单独 ≤T" (corrected, better). ✓ good.

One more check on usage.md §6 row 帧预算: "UI≈12%、Rive 渲染≈35%、GPU≈35%，每阶段单独 ≤T" — plan says 12% of T for UI at 120Hz steady ≤1ms. Consistent.

Also double-check api.md L468 for the SetReducedMotion comment (was a previous-round minor fix): current text "on→true：对每个活跃控制器 SnapToEnd（in-flight 含 Playing/Paused：正向到终点、反向到 0，并 Completed）" ✓ fixed. And api.md §5.7 L516 "零时长时间线在 Play / Reverse 时立即完成（跳过 markers）" ✓ fixed. Both beyond-P1-P8 fixes landed. I'll mention them briefly as "上轮两项次要精度项也已顺带修好".

Now let me also sanity check the claim in usage.md L4 "以本机最近一次跑测为准" — "最近一次" implies a most-recent run exists; with no logs, unfalsifiable. Covered.

Also: should I double check whether there's an actual tests folder somewhere, like C:\Users\admin\Temp\motion_framework_review has no tests. Confirmed by directory listing: only 10 files. ✓

Also the review directory has no engineering.md/summary.md/deepseek_review.md. ✓

One more verification: is there maybe a "docs" subfolder? Directory listing showed only files, no dirs (Get-ChildItem -Recurse -File shows files only). Let me not bother; the flat listing implies no subdirectories with files. Actually -Recurse -File would show files in subdirs; none appeared, so no subdirs with files. ✓

Now structure the final answer in Chinese with 4 parts. It should be thorough but organized. Include:

0. 一句话结论 + 我做了什么核查（读了 10 个文件、对比上一轮 dsh_motion_reply_only.md、本机编译器核查）。

1. 补丁复核: P1–P8 table with verdicts + evidence (line numbers), then 新漏网 (N1–N5) with paste-able patches.

2. WinUI3 demo route: phases 0-4 with deliverables, risks & mitigations, total estimate.

3. plan.md milestones M0–M5: compressible or not with reasons.

4. Exactly 3 open questions.

Length: this is a dense review; aim for comprehensive but not bloated. Use tables where useful. Cite line numbers. All Chinese.

Let me draft the P1-P8 table verdicts precisely:

- P1 部分到位: usage.md:4 已有「（估计）」与「以本机最近一次跑测为准」，且 §6 与 api.md §8 同步；但原建议的「本机无编译器/未复验」被改写成更强的「本机曾用 g++ 复验通过」——本目录无任何测试源码/日志佐证，且本机当前 PATH 与常见位置查无 g++。建议随附测试产物或改回「未复验」口径。
- P2 已到位: usage.md:276、278 与建议文本逐字一致。
- P3 已到位: usage.md:290-292 三行齐全（HWND→V1.5、Rive 不承诺帧数、帧预算流水线每阶段 ≤T），引用 disputes/plan 正确。
- P4 已到位: usage.md:256-257 注释含「IClock&」与「投影需自带/注入时钟」。
- P5 已到位: api.md:535 有 EmbedToHwnd/HWND → V1.5 行。
- P6 部分到位: api.md:560 有「估计」字样，但同样把「本机未编译」替换为「本机 g++ 复验」的新断言；且「详见 deepseek_review.md」指向不存在的文件。
- P7 已到位: api.md:35-36 两处均为「由 Spring 构造时钳制」，与 spring.h:22-23 实现一致。
- P8 未到位: usage.md:3/8/267/296、api.md:3/286/358/560、plan.md:73-78 的路径仍混用 code/、docs/、./、../，engineering.md/summary.md/deepseek_review.md 未随附。

新漏网:
- N1 (口径残留): plan.md:67-68「本机 419/617/236 项全过」「其后已安装 g++ 并复验」无「估计」限定，与 plan.md:3「所有人周…都是估计」及 usage/api 的估计口径冲突；本目录无测试产物。Patch.
- N2 (上轮遗留未修): plan.md:67 重复「5）」编号；§4 任务表第 3/4/5 行缺「已交付初版」（第 1/2 行有）。
- N3 (上轮遗留已修确认): api.md:468 与 api.md:516 的两处精度问题已修好（提一下）。
- N4 (依赖边写反): plan.md §4 任务 5 依赖列「1、3、6」，但 controller.h 已交付且只依赖 timeline/tokens；IDispatcher 缝在 controller.h 内、模块 6 反依赖它。依赖边应 6→5 而非 5→6。
- N5 (小措辞): api.md §8 标题「（估计，本机 Linux）」与当前 Windows 主机矛盾。
- N6: usage.md:4「本机曾用 g++ 复验通过」+ §6 同句 — 合并进 N1/P1。

Patches for N1/N2/N4/N5/P8/P6 — provide compact snippets.

For part 3, remember to cite M0–M5.

For part 4, exactly 3 questions.

One consideration: the user asked "若有新漏网，给出可粘贴修订片段" — provide patches for new leaks. I'll provide patches for: usage.md L4 (test wording), plan.md L67-68 (duplicate numbering + claim), plan.md §4 rows 3-5 + dependency 5→6, P8 link fixes, api.md §8 heading + L560. Keep them compact.

Also part 2 must be "分阶段路线、每阶段交付物、主要技术风险与缓解" — I'll do a table per phase + risk table.

Also note in part 2: the demo must avoid the WinMD/C# 投影 (5-7 人周) — the route uses C ABI shim or pure C++/WinRT; that's the key architectural choice making it "小样本可演示" cheaply. Also emphasize software path is fully supported today (usage.md §1), compositor path is demo-level mini lowering.

Word budget: it's a heavy review; the user expects thorough. I'll aim for a well-structured answer, moderately long. Use markdown tables and code blocks. All Chinese. Avoid excessive fluff.

Let me write the final answer now. I'll cite file paths with line numbers. Note instructions about linking files: I should link file mentions. The instruction says outside commands/code blocks, link every mention of an existing file to its full path relative to working directory. That would be heavy for this many mentions. The rule: "Outside commands, configuration expressions, and code blocks, link every mention of an existing file". Hmm. That's for the GUI. Given the volume of file mentions, I'll link key ones (first mention of each file) with relative paths like [usage.md](<usage.md>). Working directory is C:\Users\admin\Temp\motion_framework_review. Relative paths from workspace root. I'll use relative links without angle brackets except where needed. Keep it reasonable: link first occurrence of each file, and use plain code-format for line refs like `usage.md:4` — hmm, but then "usage.md" inside backticks is a code span, allowed unlinked? The instruction says outside code blocks link every mention; code spans presumably exempt. To be safe and useful, I'll link first mentions and keep line refs in backticks (code spans). That should be acceptable.

Let me also decide the P1 verdict wording: The user asked "已到位 / 部分到位 / 未到位 / 仍有漏网". For P1: 部分到位 (标记已加，但新断言比原问题更激进，且无佐证). P6: 部分到位. P8: 未到位. Others: 已到位.

One more sanity check on P1: the previous recommendation text (from dsh_motion_reply_only.md) was: "测试状态：timeline 419 / tokens 617 / controller 236 项在实现方 Linux 环境通过（估计）；plan.md §8 注明本机尚无 C++ 编译器、尚未本地编译验证，以本机复测为准。" Current: "timeline 419 / tokens 617 / controller 236 项在实现方 Linux 环境通过（**估计**）；本机曾用 g++ 复验通过，以本机最近一次跑测为准。" So yes: 估计 kept, but "本机无编译器/未验证" → "本机曾用 g++ 复验通过". The direction changed. Since plan.md now claims g++ was installed, the docs are internally consistent with plan.md but the underlying claim is unverifiable — I checked and found no g++ now. So "部分到位" with a new-evidence gap is the right verdict, plus note the option that g++ may have existed transiently — but as a deliverable it must be reproducible. Good.

Also I should double check plan.md line 68: "早期本机一度无编译器；其后已安装 g++ 并复验 timeline 419 / tokens 617 / controller 236。" — yes. This appears to be a NEW claim added in the update (previous round plan.md:68 said "这台电脑上没有 C++ 编译器，代码尚未编译或测试"). So the update flipped the claim. My verification: no g++ now. Also note plan.md:67 says "本机 419 项全过" for timeline in §8 — wait, actually line 67 says "3）时间线和时钟，已交付 → code/timeline.h（本机 419 项全过）" and "4）...tokens.h + motion.json（本机传 motion.json 参数时 617 项全过）" and "5）...controller.h（本机 236 项全过）". Right.

In the final answer, for the P1 verification I'll state: 经查本机 PATH 无 g++/clang++/cl/cmake/ninja，常见安装目录亦无 g++.exe；目录内无测试源码/日志 → 「本机复验」无法重现。Hedged: 不排除曾短暂安装后卸载，但交付物形态下不可核验。

Now write it. Structure:

# 总览（几行）
- 核查方法说明。

## 1) 补丁复核 P1–P8
Table + details + 新漏网 N1-N5 + patches.

## 2) WinUI3 可演示小样本技术路线
Phase table + risks.

## 3) 对照 plan.md 里程碑 M0–M5 压缩分析
Table + reasoning.

## 4) 开放问题（恰好 3 个）

Keep patches compact. Let me write.

For patch N1 (usage.md L4 replacement) — full replacement of line 4:

```markdown
> **测试状态**：timeline 419 / tokens 617 / controller 236 项为实现方 Linux 环境报告数（**估计**）。本目录未随附测试源码与跑测日志；本机当前 PATH 无 g++（「曾复验」无法重现）。在测试随附前，本机复验状态标注为**未复验**。
```

plan.md L68 replacement:

```markdown
- `spring.h` 已经应用 Claude 自查发现的两处修复：特大阻尼比时数值精度丢失（改用两根之积计算）、阻尼比下限钳制为 0。**早期本机一度无编译器；其后声称安装 g++ 并复验 timeline 419 / tokens 617 / controller 236，但测试源码与日志未随附（当前机器 PATH 亦无 g++），此复验声明待证实。**
```

api.md L560 replacement:

```markdown
> 用例数来自实现方 Linux 环境；「本机 g++ 复验」无随附测试产物可核验（当前本机无编译器）。上表为**估计**；建议随附测试源码与跑测命令，以本机实跑为准。deepseek_review.md 未随附本目录。
```

plan.md L67 renumber patch:

```markdown
5）可打断控制器与重定向，已交付 → `code/controller.h`；6）能力分发器的降级规则；7）ETW 事件定义；8）测试工具与 CI 分析脚本；9）技术验证脚手架（由人运行并解读结果）。
```

plan.md §4 rows patch:

```markdown
| 3 | 时间线、轨道、关键帧图，时钟抽象 | 1 | 4-6 | **Claude**（已交付初版） |
| 4 | Token schema、校验器、加载器 | 1、3 | 3-4 | **Claude**（已交付初版） |
| 5 | 可打断控制器、句柄、重定向 | 1、3 | 4-6 | **Claude**（已交付初版），人评审 |
```
(把依赖「1、3、6」改为「1、3」— note as part of N4: controller.h 只依赖 timeline/tokens；「6」依赖边反了。)

api.md §8 heading patch: 「## 8. 测试覆盖（估计，Linux 环境）」.

P8 patches as above.

Also for api.md L3: keep simple:

```markdown
> **范围**：本文档只描述随附头文件（本目录 `*.h`，原仓库路径 `code/`）已实现的 C++17 公开接口。
```

api.md L286: "`docs/engineering.md`" → "`engineering.md`（未随附本目录）".
api.md L358: "示例文件：`code/motion.json`" → "示例文件：[`motion.json`](./motion.json)（本目录随附）".

usage.md L267: "`../disputes.md`" → "`./disputes.md`"（即 [`disputes.md`](./disputes.md)）.
usage.md L296 rewrite as above.

OK. Now also for part 2 details, let me flesh out:

阶段 0（0.5 天）：工具链与模板
- 交付物: 可构建的空白 WinUI 3 工程（官方 C++/WinRT 或 C# 模板），VS2022 + Windows SDK + Windows App SDK 验证脚本。
- 风险: 模板/SDK 版本组合问题；缓解: 固定 Windows App SDK 版本（1.5/1.6），CI 用 GitHub Actions windows-latest。

阶段 1（0.5-1 天）：纯逻辑 golden demo（不碰 UI）
- demo_core: ManualClock + 记录型 Dispatcher（Apply 写 CSV/JSONL）+ motion.json 四场景（card.enter/exit、button.press、list.stagger-in、拖拽 Retarget 序列），输出 golden 文件与 P95 Tick 耗时。
- 交付物: demo_core.exe、golden 输出、构建脚本（-std=c++17 一行命令）。
- 风险: 基本无；只验证 token→controller 全链 + 本机构建可行性（**当前本机无编译器，需先装**）。

阶段 2（2-3 天）：WinUI3 软件路径 app
- 工程选型 A（推荐）: C++/WinRT 单工程直接 #include 五个头文件（header-only，无投影问题）；选型 B（退路）: C# app + C ABI DLL（extern "C" 约 10 个函数封装 MotionSystem）。
- 时钟与帧: SteadyClock；驱动用 CompositionTarget.Rendering（跟随 vsync，120Hz 屏自动 120Hz）或 DispatcherQueueTimer(16ms)。
- Apply 映射: opacity→Opacity；offsetY→Translation.Y；scale→Scale（Vector3 均匀）；offsetX→Translation.X。IDispatcher::Apply 里写 UIElement 属性。
- 演示交互: 卡片 enter/exit（tokens）、按钮 press（springTo inheritVelocity）、列表 stagger、拖拽方块 Retarget 跟手 + 松手回弹（保留速度）；右下角 reduced motion 开关（绑定 UISettings.AnimationsEnabled）。
- 交付物: 可运行 app、操作脚本、60/120Hz 录屏。
- 风险: C++/WinRT 模板构建细节多 → 退路 C#+C ABI；UIElement.Scale/Translation 属性在 WinUI3 存在（composition 属性）→ 若版本不支持退到 RenderTransform 或 ScaleTransform；时钟抖动 → CompositionTarget.Rendering。

阶段 3（3-5 天）：mini-CompositionDispatcher（下放试点）
- IDispatcher::Submit lowering: Track 的 Linear/Bezier 段 → Compositor.CreateScalarKeyFrameAnimation + CreateCubicBezierEasingFunction；Spring 段 → CreateSpringScalarNaturalMotionAnimation；Retarget → 重发 natural motion（或 ExpressionAnimation 接管）；Withdraw → StopAnimationGroup。
- 选 1 个属性试点（建议 button.press 的 scale 弹簧），做软件/合成器双路径开关与参数对照实验（Period/DampingRatio ↔ response/damping、CubicBezierEasingFunction 对 y∉[0,1] 的接受度——正是 disputes.md 待验证清单）。
- 交付物: hybrid demo、参数对照数据表、M0/M1 验证脚手架雏形、已知差距清单。
- 风险: 参数映射无把握（这是计划里「必须由人真机验证」项）→ 阶段 3 定位为实验而非承诺；所有权 last-wins 在合成器侧的状态机复杂度 → 只做单 target 单属性；lowered 与 Apply 双写 → IDispatcher 注释已定义 Apply 可忽略 lowered 属性。

阶段 4（1-2 天）：设计师闭环与打包
- motion.json 热重载（FileSystemWatcher→Parse→错误浮层）、reduced motion 跟随系统开关、README + 录屏 + 给三模型的观察问题清单。
- 交付物: 演示包。
- 风险: 低。

总投入: 6-10 人日 ≈ 1.5-2 人周（不含装工具链与等硬件）。
关键决策: 不做 WinMD/C# 投影（任务 7 的 5-7 人周）——演示走 C++/WinRT 直连或 C ABI 桥；不做 Rive（任务 8）——演示只用 XAML 元素。

风险总表:
1. 工具链（本机现无编译器；C++/WinRT 模板脆弱）→ 先装 VS2022/Windows SDK；退路 C ABI。
2. 合成器参数对应（Period/DampingRatio、bezier y 超界）→ 做成对照实验输出数据，正是 M0 待验证项。
3. 120Hz 演示硬件 → 提前预定；软件路径 60Hz 兜底。
4. 下放状态机（last-wins/Withdraw）→ 试点单属性；完整实现留模块 6。
5. 帧驱动与 UI 线程 → 全部走 UI 线程 + CompositionTarget.Rendering，不做跨线程（与头文件线程规则一致）。

Part 3 table:

| 里程碑 | 可否压缩 | 理由 |
M0 不可压缩: go/no-go 门 + 硬件实测日历时间 + Skia 备选触发点。
M1 可压缩（部分）: 任务1-5已交付初版使「无界面跑通单测」过半达成；分发器规则可与 M0 并行；但「真机验证弹簧参数对应」依赖 M0 不可前移。
M2 可微压缩（1-2周）: ETW 定义与 C# 投影样板可提前并行；预览宿主 v0 砍到热重载最小集；但性能 CI ≥2 台物理机采购/基线是日历时间。
M3 不可压缩: Rive 宿主 8-12 人周风险最高；试点 22 周已激进；混合刷新率/DPI 矩阵需真机排队。
M4 不可压缩: P95/P99 达标需要数周基线数据（1000 帧×多次）；无障碍外部评审排期；双门禁是 disputes 裁决不可改单。
M5 不可压缩: 试点 2 联调不可预测 + P0/P1 清零门槛 + Skia 备选(+4-8周)唯一吸收点。

结论段: 唯一真实的「压缩」来自已交付事实（M1 内任务 1-5），建议把回收人周转为 M2-M5 缓冲而非前移承诺日期；另指出依赖边 5→6 写反这一处计划级错误。

Part 4 questions — write them crisp, each with 裁决选项.

Q1: 测试证据门槛（裁决式）
Q2: M1 拆分与并行开工（裁决式）
Q3: 演示准入标准 + 第一个下放试点（裁决式）

Format each: 背景一句话 + 选项 A/B + 需要谁给什么（Gemini 查 WinUI 文档、Claude 给推导、DeepSeek 复核）— for Q3 mention division of labor.

Now write the final response. Keep it structured, Chinese. Include the table for P1-P8 and per-item evidence. Reasonable length — this is a big deliverable, so length is justified, but I'll keep prose tight.
# 四部分审阅答复（基于对 10 个文件的逐行重读 + 上一轮原文比对 + 本机实证核查）

**核查方式说明**：我重读了 [api.md](<api.md>)、[usage.md](<usage.md>)、[plan.md](<plan.md>)、[disputes.md](<disputes.md>) 与五个头文件（spring/bezier/timeline/tokens/controller）+ motion.json；调出上一轮建议原文（`C:\Users\admin\Temp\dsh_motion_reply_only.md`）逐条比对；并实测核查：**本机 PATH 无 g++/clang++/cl/cmake/ninja，常见安装目录亦无 g++.exe**；目录内**无任何测试源码、跑测日志**；engineering.md / summary.md / deepseek_review.md **均不存在**。

---

## 1) 补丁复核 P1–P8

| # | 判定 | 证据与说明 |
|---|------|-----------|
| P1 | **部分到位** | `usage.md:4` 已加「（**估计**）」与「以本机最近一次跑测为准」，但上一轮建议的「本机尚无编译器、尚未本地验证」被**改写成更强的断言**「本机曾用 g++ 复验通过」。本目录无任何测试产物可佐证，且本机当前查无 g++（不排除曾短暂安装后卸载，但交付物形态下不可核验）。 |
| P2 | **已到位** | `usage.md:276` 已为 `MotionSystem::Create(TargetId, tokens, transition)`；`usage.md:278` 已为 `AnimationHandle::Retarget`，与上轮建议逐字一致。 |
| P3 | **已到位** | `usage.md:290-292` 三行齐全：HWND→V1.5（disputes §1）、Rive「不承诺帧数」+实测为交付指标（disputes §3）、帧预算「每阶段单独 ≤T、8.3ms 是红线不是可加总预算」（disputes §2）。且「≤1/T」已订正为「≤T」。 |
| P4 | **已到位** | `usage.md:256-257` 注释已含 `MotionSystem(IClock&, IDispatcher*)` 与「投影需自带/注入时钟」。 |
| P5 | **已到位** | `api.md:535` 有「`EmbedToHwnd` / HWND 宿主正式 API → **V1.5**；首发仅随附不受支持的参考示例」。 |
| P6 | **部分到位** | `api.md:560` 有「估计」字样，但同样把「本机未编译」替换为「本机 g++ 复验」新断言，且「详见 deepseek_review.md」指向**不存在的文件**。 |
| P7 | **已到位** | `api.md:35-36` 两处均为「由 Spring 构造时钳制」，与 `spring.h:22-23` 实现一致（钳制发生在 `Spring` 构造器，`Params()` 返回钳制后值）。 |
| P8 | **未到位** | 路径仍混用：`usage.md:3`「code/*.h」、`usage.md:8` `./engineering.md`（不存在）、`usage.md:267` `../disputes.md`（指向目录外）、`usage.md:296` `../plan.md` `../summary.md`、`api.md:3/286/358` `code/`、`docs/engineering.md`、`code/motion.json`、`plan.md:73-78` 文档索引全为 `docs/...`。engineering/summary/deepseek_review 未随附。 |

**顺带确认**：上一轮提出的两处 api.md 精度项（§5.4「in-flight 含 Playing/Paused、反向到 0」、§5.7「零时长在 Play/Reverse 立即完成」）也已在 `api.md:468`、`api.md:516` 修好。

### 新漏网（可粘贴修订片段）

**N1 —「本机 g++ 复验」是新断言且零证据（P1/P6 的残留，最要紧）**
`plan.md:67`「本机 419/617/236 项全过」、`plan.md:68`「其后已安装 g++ 并复验」为事实口吻，与 `plan.md:3`「所有人周和工期都是估计」及 usage/api 的估计口径冲突。修订：

```markdown
<!-- usage.md 第 4 行替换为 -->
> **测试状态**：timeline 419 / tokens 617 / controller 236 项为实现方 Linux 环境报告数（**估计**）。本目录未随附测试源码与跑测日志，本机当前 PATH 亦无 g++，「本机曾复验」无法重现；随附测试前本机状态按**未复验**标注。

<!-- plan.md 第 68 行结尾替换为 -->
- ……**早期本机一度无编译器；其后声称安装 g++ 并复验 419/617/236，但测试源码与日志未随附（当前机器 PATH 无 g++），该复验声明待证实。**

<!-- api.md 第 560 行替换为 -->
> 用例数来自实现方 Linux 环境；「本机 g++ 复验」无随附测试产物可核验（本机当前无编译器）。上表为**估计**；建议随附测试源码与跑测命令，以本机实跑为准。`deepseek_review.md` 未随附本目录。
```

**N2 — plan.md §8 重复编号与 §4 表格缺标注（上轮已指出、仍未修）**

```markdown
<!-- plan.md 第 67 行：删掉重复的「5）重定向控制器」，后续顺延 -->
5）可打断控制器与重定向，已交付 → `code/controller.h`；6）能力分发器的降级规则；7）ETW 事件定义；8）测试工具与 CI 分析脚本；9）技术验证脚手架（由人运行并解读结果）。

<!-- plan.md §4 表格第 3/4/5 行补「已交付初版」，与第 1/2 行及 §8 对齐 -->
| 3 | 时间线、轨道、关键帧图，时钟抽象 | 1 | 4-6 | **Claude**（已交付初版） |
| 4 | Token schema、校验器、加载器 | 1、3 | 3-4 | **Claude**（已交付初版） |
| 5 | 可打断控制器、句柄、重定向 | 1、3 | 4-6 | **Claude**（已交付初版），人评审 |
```

**N3 — plan.md 依赖边写反**：任务 5 的依赖列「1、3、**6**」，但 controller.h 已交付且只依赖 timeline/tokens，`IDispatcher` 缝定义在 controller.h 内，是模块 6 反依赖任务 5。应改为依赖「1、3」，并在任务 6 的依赖列加「5」。（上表已顺手改。）

**N4 — api.md §8 标题自相矛盾**：「测试覆盖（估计，**本机 Linux**）」把「本机」（Windows 主机）与「Linux」并置。改为「## 8. 测试覆盖（估计，Linux 环境）」。

**N5 — P8 的具体修订**：

```markdown
<!-- usage.md 第 8 行 -->
配套 API 清单见 [`api.md`](./api.md)。架构背景见 `engineering.md`（**未随附本目录**）。

<!-- usage.md 第 267 行 -->
真实接线 **待真机验证**（DWM / `Windows.UI.Composition` 参数对应关系）。Rive 指针跟随延迟见下方限制表与 [`disputes.md`](./disputes.md)。

<!-- usage.md 第 296 行 -->
更多架构与里程碑：[`plan.md`](./plan.md)、[`disputes.md`](./disputes.md)；未随附：`engineering.md`、`summary.md`、`deepseek_review.md`。

<!-- api.md 第 3 行 -->
> **范围**：本文档只描述随附头文件（本目录 `*.h`，原仓库路径 `code/`）中已实现的 C++17 公开接口。

<!-- api.md 第 286 行：`docs/engineering.md` → `engineering.md`（未随附本目录）
     api.md 第 358 行：`code/motion.json` → [`motion.json`](./motion.json)（本目录随附）
     plan.md 第 73-78 行文档索引：`docs/...` → `./...`，不存在者标注「未随附」 -->
```

---

## 2) WinUI3 可演示小样本技术路线

**核心决策：绕过 WinMD/C# 投影（任务 7 的 5–7 人周）与 Rive（任务 8 的 8–12 人周）。** 现有核心已完整支持「软件路径」（usage.md §1：自供时钟 + IDispatcher::Apply 写回 UI），演示先 100% 走这条路；下放只做一个「演示级」单属性试点。总投入约 **6–10 人日（1.5–2 人周）**，不含装工具链。

| 阶段 | 内容 | 交付物 | 主要风险 → 缓解 |
|---|---|---|---|
| **0（0.5 天）工具链** | 装 VS2022 + Windows SDK + Windows App SDK；用官方模板建空白 WinUI3 工程并跑通 | 可构建空白工程 + 版本固定说明 | 模板/SDK 版本组合易碎 → 固定 Windows App SDK 版本；**当前本机无任何编译器，此步为硬前置** |
| **1（0.5–1 天）纯逻辑 golden demo** | 控制台程序：ManualClock + 记录型 Dispatcher（Apply 落 CSV/JSONL），跑 motion.json 四场景（card.enter/exit、button.press、list.stagger-in、拖拽 Retarget 序列），输出 golden 文件与 Tick 耗时 | demo_core.exe + golden 输出 + `g++ -std=c++17` 一行构建脚本 | 几乎无风险；顺带验证 token→controller 全链，并成为 P1/P6「本机复验」的证据来源 |
| **2（2–3 天）软件路径 app** | 方案 A（推荐）：C++/WinRT 单工程直接 `#include` 五个头文件（header-only，无投影问题）；方案 B（退路）：C# app + C ABI DLL（`extern "C"` 约 10 个函数封装 MotionSystem）。帧驱动用 `CompositionTarget.Rendering`（跟 vsync，120Hz 屏自动 120Hz）；Apply 映射：opacity→Opacity、offsetY→Translation.Y、scale→Scale(Vector3)、offsetX→Translation.X。交互：卡片进/出、按钮弹簧按压（inheritVelocity）、列表 stagger、**拖拽方块 Retarget 跟手 + 松手速度保留回弹**；reduced motion 挂钩 `UISettings.AnimationsEnabled` | 可运行 app + 操作脚本 + 60/120Hz 录屏 | C++/WinRT 模板细节多 → 退路 C ABI；`UIElement.Scale/Translation` 属性若版本不支持 → 退到 RenderTransform；时钟抖动 → 用 Rendering 而非计时器 |
| **3（3–5 天）mini-CompositionDispatcher（下放试点）** | 实现 `IDispatcher::Submit` lowering：Linear/Bezier 段 → `CreateScalarKeyFrameAnimation` + `CreateCubicBezierEasingFunction`；Spring 段 → `CreateSpringScalarNaturalMotionAnimation`；Retarget → 重发 natural motion 或 ExpressionAnimation；Withdraw → StopAnimationGroup。**只试点 1 个属性**（建议 button.press 的 scale 弹簧），做软件/合成器双路径开关与参数对照实验（Period/DampingRatio ↔ response/damping、bezier y∉[0,1] 是否被接受——正是 disputes.md 待验证清单） | hybrid demo + 参数对照数据表 + M0/M1 验证脚手架雏形 + 已知差距清单 | 参数映射无把握（plan.md 明列「必须由人真机验证」）→ 本阶段定位为**实验**而非承诺；last-wins 在合成器侧的状态机复杂度 → 只做单 target 单属性，完整状态机留模块 6；lowered 与 Apply 双写 → IDispatcher 注释已约定 Apply 可忽略 lowered 属性 |
| **4（1–2 天）设计师闭环** | motion.json 热重载（FileSystemWatcher→Parse→Diagnostic 错误浮层）、reduced motion 跟随系统开关、README + 录屏 + 观察问题清单 | 演示包 | 低 |

**风险总表**：① 工具链（本机无编译器、C++/WinRT 脆弱）——先装工具链，C ABI 为退路；② 合成器参数对应——阶段 3 输出对照数据、不进 1.0 承诺；③ 120Hz 演示硬件——提前预定，软件路径 60Hz 兜底；④ 线程——全部走 UI 线程 + Rendering 回调，与头文件「单 dispatcher 线程」规则一致，不做跨线程下放。

---

## 3) 对照 plan.md 里程碑的压缩分析

| 里程碑 | 能否压缩 | 理由（引用编号与内容） |
|---|---|---|
| **M0（第 2 周，4–6 人周）** | **不能** | 全计划唯一的 go/no-go 门（「作出是否继续的决定，并重新核算估计」）。Rive 进合成表面、ALLOW_TEARING、指针属性集、实测延迟的结论直接决定任务 6/8/9 方案，以及 **Skia 备选（+4–8 周）是否触发**；其中大量是硬件/驱动/实测等待，非编码可摊薄。压缩 M0 = 用未验证假设押注后续 30 周。 |
| **M1（第 10 周）** | **可压缩（部分）** | 验收两半：①「数学核心、时间线、token 无界面跑通单测」——任务 1–5 已交付初版，此半**已成事实**，收尾/评审可从关键路径拿掉；②「分发器降级 + 真机验证弹簧参数对应」——分发器降级规则（Claude 写）可与 M0 并行开工。**不可压**：真机验证 `SpringVectorNaturalMotionAnimation` 参数对应依赖 M0 硬件与结论。预计回收 2–3 周。 |
| **M2（第 16 周）** | **可微压缩（1–2 周）** | ETW 事件定义（任务 10）可与任务 6 并行；C# 投影样板（任务 7）在任务 4/5 完成后接口面已稳，可提前；预览宿主 v0 验收可砍到「token 热重载 2 秒内上屏」最小集（Gemini 的 1.5 秒留到 M4）。**不可压**：性能 CI ≥2 台物理机（采购、装机、基线采集是日历时间）；Figma 插件 8–10 人周只能砍范围。 |
| **M3（第 22 周）** | **不能** | Rive 宿主 8–12 人周是全计划风险最高项（渲染线程、DPI 重光栅化、设备丢失逐项踩坑），几乎不可并行摊薄；试点提前到 22 周接入**已是采纳 Gemini 意见后的激进值**；混合刷新率/DPI 矩阵实测需多台真机排队。再压缩只会把成本转移成 M4/M5 返工。 |
| **M4（第 28 周）** | **不能** | P95/P99 达标依赖性能 CI 数周基线（「每场景至少 1000 帧、跑多次」），瓶颈是日历时间；无障碍需外部评审 0.2 人排期；且「绝对上限 + 相对回退」**双门禁是 disputes.md §2 的裁决**，压缩时改单门禁即违约。 |
| **M5（第 32–34 周）** | **不能（作为缓冲反而要守住）** | 试点 2 是「更重或更老的应用」作对照，联调时长不可预测；「无未关闭 P0/P1」是硬门槛；Skia 备选（+4–8 周）若触发，32–34 周窗口是**唯一吸收点**。压缩 M5 等于取消全计划唯一的缓冲。 |

**结论**：唯一真实的压缩来自**已交付事实**（M1 内的任务 1–5），而非重新排期。建议：把 M1/M2 回收的 3–5 周**转作 M3–M5 的风险缓冲**，而不是前移承诺日期。另有一处计划级错误需先修：**任务 5 的依赖边「1、3、6」写反**（见 N3），按现依赖图「分发器→控制器」无法解释 controller.h 已交付的事实。

---

## 4) 开放问题（恰好 3 个，供 Gemini / Claude / DeepSeek 下一轮裁决）

**Q1（测试证据门槛，裁决式）**：三份文档现已写入「本机曾用 g++ 复验通过 419/617/236」，但目录内没有任何测试源码、跑测命令或结果日志，且当前机器查无 g++。请裁决二选一：
- **A**：测试源码与跑测日志必须随交付物入库，入库前「已实现」标注一律降级为「已交付初版、待复验」；
- **B**：维持现状，以实现方 Linux 结果作为唯一证据，并**删除**全部「本机复验」字样。
无论选哪个，下一轮起 usage.md / api.md / plan.md 三处口径必须统一（当前 plan.md §8 仍是事实口吻）。

**Q2（M1 拆分与并行开工）**：任务 1–5 已交付初版后，M1 剩余实质只有「分发器 + 合成器参数真机验证」。请裁决：是否把 M1 拆为「M1a 纯逻辑（判定：已过）」与「M1b 合成器真机对应」，并让任务 6 的降级规则与 M0 **并行**开工、ETW/C# 投影样板提前，从而压缩 M2 1–2 周？还是坚持 Claude 的串行结构（M0→分发器→Rive→指针→试点），把压缩收益全部转作缓冲、不动承诺日期？须明确：并行开工会不会让分发器在 M0 结论未定时返工（例如 ALLOW_TEARING 不可用时整套 lowering 策略作废）？

**Q3（演示准入标准）**：WinUI3 小演示的第一阶段若只有**纯软件路径**（UI 线程 Tick + Apply 写 `UIElement.Scale/Translation/Opacity`），算不算「可演示」？请裁决：
- **A**：软件路径即可，下放试点降级为「阶段 3 可选」；
- **B**：必须含至少一条合成器下放才可对外演示。
若选 B，请同时指定：第一个下放试点动画（button.press 的 springTo vs card.enter 的 opacity bezier），并分工给出 `Period/DampingRatio ↔ response/damping` 的换算公式初版——Gemini 给出 WinUI 文档依据（`SpringVector3NaturalMotionAnimation`/`SpringScalarNaturalMotionAnimation` 参数语义）、Claude 给推导、DeepSeek 复核——该公式同时就是 M1b 的验证脚本输入。
Now I have the previous round's full P1–P8 text. Key facts established:

1. Previous round (P1): recommended usage.md line 3-6 mention "本机尚无 C++ 编译器、尚未本地编译验证". Current usage.md now says "本机曾用 g++ 复验通过，以本机最近一次跑测为准". And plan.md line 68 now says "早期本机一度无编译器；其后已安装 g++ 并复验 timeline 419 / tokens 617 / controller 236". So the docs now claim a NEW fact: g++ was installed and re-verified. On THIS machine, g++ is not found on PATH nor in common roots. So the claim "本机曾用 g++ 复验通过" is currently unverifiable and, by my check, g++ isn't even present now. This is a significant finding for P1/P6 review: the fix replaced "no compiler" with "re-verified with g++" — a stronger claim that (a) cannot be reproduced from this directory (no tests, no logs), (b) contradicts my check that g++ isn't installed on this machine (no g++.exe anywhere in common locations, not on PATH).

Wait — careful. The machine where "本机" refers — the review is happening on C:\Users\admin\Temp. The docs say 本机 (this machine). My check: g++ not on PATH; no g++.exe in C:\msys64, C:\mingw64, C:\mingw32, C:\TDM-GCC-64, C:\Program Files, C:\ProgramData\chocolatey, C:\tools, C:\Strawberry (only the ones that exist were searched — C:\Program Files may be huge but the search returned nothing). The command exited 1 because `Get-Command clang++/cl` failed (no output). Actually the exit code 1 came from the last Get-Command failing. Let me be careful: the output shows "no g++.exe found in common roots" then "----" then exit code 1 (because Get-Command cl failed and returned non-zero, which propagated). So clang++, cl, cmake, ninja — none found on PATH either. So this machine currently has no C++ compiler toolchain on PATH. g++ could be somewhere unusual but we can't verify; the docs' claim of "本机曾用 g++ 复验通过" is not corroborated by any artifact in the directory (no test sources, no build logs, no results file).

This is a strong, concrete finding: P1/P6 got a wording fix but the new wording asserts an even stronger claim (本机复验通过) that is unverifiable in this directory and inconsistent with the current machine state (no compiler found). I should report it precisely: "已到位一半" or "部分到位" — the 估计 markers are in place, but "本机曾用 g++ 复验通过" is a new unverifiable claim. And plan.md §8 says it as fact ("本机 419 项全过" etc. — line 68: "本机传 motion.json 参数时 617 项全过", "本机 236 项全过", "本机 419 项全过" in §8 lines 67).

Let me re-check plan.md line 67-68:
- Line 67: "...已交付 → code/timeline.h（本机 419 项全过）；...code/tokens.h + motion.json（本机传 motion.json 参数时 617 项全过）；...code/controller.h（本机 236 项全过）；5）重定向控制器；..."
- Line 68: "spring.h 已经应用 Claude 自查发现的两处修复... **早期本机一度无编译器；其后已安装 g++ 并复验 timeline 419 / tokens 617 / controller 236。**"

So plan.md §8 has both "本机 X 项全过" claims AND the note about installing g++. Duplicate "5）" is still there (line 67: "5）可打断控制器与重定向，已交付 → code/controller.h（本机 236 项全过）；5）重定向控制器；") — the duplicate numbering leak from the previous round is still present. Previous round also flagged plan.md §4 rows 3/4/5 lacking "已交付" markers vs §8. Let me check plan.md §4 table now: rows 3 (时间线 4-6 人周, Claude), 4 (Token 3-4), 5 (控制器 4-6) — no "已交付初版" markers. Rows 1 and 2 have "**Claude**（已交付初版）". So still inconsistent. These are "新漏网/旧漏网仍在" items beyond P1-P8.

Also, previous round's minor precision items in api.md §5.4 and §5.7 (in-flight includes Paused; Reverse zero-duration also completes immediately): let me check current api.md:
- api.md §5.4 line 468: "on→true：对每个活跃控制器 SnapToEnd（in-flight 含 Playing/Paused：正向到终点、反向到 0，并 Completed）" — FIXED (includes Playing/Paused and 反向到 0). ✓
- api.md §5.7 line 516: "零时长时间线在 Play / Reverse 时立即完成（跳过 markers）" — FIXED (mentions Reverse). ✓

Good — those two were also fixed beyond the P1-P8 list.

Now current P-by-P status with line evidence:

P1 (usage.md 第3–6行): Current lines 3-6:
```
> **前提**：核心在 `code/*.h`（header-only C++17，命名空间 `motion`）。  
> **测试状态**：timeline 419 / tokens 617 / controller 236 项在实现方 Linux 环境通过（**估计**）；本机曾用 g++ 复验通过，以本机最近一次跑测为准。  
> **未接线**：Windows 合成器 / WinUI / C# 绑定 — **规划中**；下文 C# 仅为「规划伪代码」。  
> 估计与待真机项单独标明。
```
Verdict: 部分到位。The 估计 marker and "待真机项单独标明" are in. But instead of P1's recommended "本机尚无编译器、尚未本地编译验证", it now asserts "本机曾用 g++ 复验通过" — a stronger claim with zero artifacts in the directory (no test sources/logs) and no compiler currently discoverable on this machine. The claim may be true (maybe g++ was installed and removed, or it's elsewhere), but as a deliverable it's unverifiable. Recommend either attach test sources + result log, or revert to "本机未复验" wording. Patch snippet to provide.

P2 (usage.md §5 清单第4、6条): Current lines 276, 278:
- "4. `MotionSystem::Create(TargetId, tokens, transition)` → 设回调 → `Play`。" ✓ exact recommended text.
- "6. 交互打断用 `AnimationHandle::Retarget`（勿直接依赖 `Player::Retarget`，见 api.md §3.6 / §7.1）；取消用 `AnimationHandle::Cancel`。" ✓ exact.
Verdict: 已到位.

P3 (usage.md §6): Current lines 290-292 contain HWND→V1.5, Rive 不承诺帧数, 帧预算流水线 rows, all with disputes.md/plan.md references. ✓ Also row 287 test status now says "实现方环境 419/617/236 项通过（估计）；本机 g++ 曾复验通过，以最近一次跑测为准". And 闲置 0% CPU row includes "≤0.1% 为 Gemini 口径". Verdict: 已到位 (for the three required rows). Note: P3's recommended row said "本机无编译器，尚未本地复验" — again the shipped version asserts g++ 复验 instead. Same caveat as P1.

P4 (usage.md §4 C# 伪代码补时钟注释): Current lines 256-257:
```
// var system = MotionSystem.Create(dispatcher);   // 规划：C++ 侧为 MotionSystem(IClock&, IDispatcher*)，
//                                                 // 投影需自带/注入时钟，此处仅示意，勿当作最终签名
```
✓ exact recommended text. Verdict: 已到位.

P5 (api.md §6 追加 EmbedToHwnd/HWND → V1.5): Current api.md line 535: "| `EmbedToHwnd` / HWND 宿主正式 API | **V1.5**；首发仅随附不受支持的参考示例（`disputes.md` §1） |" ✓. Verdict: 已到位.

P6 (api.md §8 末尾补「估计/本机未编译」): Current api.md line 560: "> 用例数来自实现方 Linux 环境与本机 g++ 复验；上表为**估计**，以本机最近一次跑测为准。详见 [`deepseek_review.md`](./deepseek_review.md)。" Verdict: 部分到位 — "估计" present, but instead of "本机未编译" it claims "本机 g++ 复验" (same issue as P1); AND the "详见 deepseek_review.md" link points to a file that does not exist in this directory. Also §8 heading now says "（估计，本机 Linux）" which is good.

P7 (api.md §1.1 SpringParams 措辞): Current lines 35-36: "由 Spring 构造时钳制为 >= 1e-4" / "由 Spring 构造时钳制为 >= 0（负值会发散）" ✓ exact. Verdict: 已到位 (and verified against spring.h:22-23).

P8 (扁平目录相对路径 / 随附 engineering.md): Current state:
- usage.md:3 "核心在 `code/*.h`" — still says code/; files are flat.
- usage.md:8 "架构背景见 [`engineering.md`](./engineering.md)" — file absent; and mixed style (./ vs ../).
- usage.md:267 "Rive 指针跟随延迟见下方限制表与 `../disputes.md`" — ../disputes.md points outside this dir.
- usage.md:296: `./engineering.md`, `../plan.md`, `../summary.md`, `../disputes.md`, `./deepseek_review.md` — 3 of 5 broken paths + 3 missing files.
- api.md:3 "本文档只描述 `code/` 下已实现的 C++17 头文件" — code/ not present.
- api.md:286 "`docs/engineering.md` / 计划中明确" — docs/ not present.
- api.md:358 "示例文件：`code/motion.json`" — code/ not present (motion.json is flat here).
- api.md:560 "`./deepseek_review.md`" — absent.
- plan.md:73-78 文档索引 all `docs/...` — absent.
Verdict: 未到位. Provide patchable snippets.

New leaks beyond P1–P8 (仍有漏网):
N1. plan.md 测试口径仍与 usage/api 不一致: plan.md §8 lines 67-68 state "本机 419 项全过" / "本机传 motion.json 参数时 617 项全过" / "本机 236 项全过" as fact, and claim "其后已安装 g++ 并复验" — no 估计 qualifier in §8, contradicting plan.md's own line 3-4 (所有估计) and usage.md/api.md's 估计 wording; plus no test artifacts anywhere. Also plan.md:68 "本机 236 项全过" vs api.md §8/usage.md §6 table numbers 419/617/236 — consistent numbers, but the claim strength differs. Patch: add "（估计，无随附测试产物）" or cite the log.

N2. plan.md §8 duplicate numbering "5）...5）重定向控制器" still present (line 67), and §4 table rows 3/4/5 still lack "已交付初版" markers that rows 1/2 have. Previous round flagged both; not fixed. Patch: fix numbering and add markers.

N3. usage.md:4/usage.md:287/api.md:560 claim "本机曾用 g++ 复验通过" — new unverifiable claim replacing the honest "本机无编译器". My machine check: no g++/clang/cl on PATH, no g++.exe under common roots. If the docs are to be the deliverable, either (a) 随附测试源码 + 跑测日志/命令, or (b) 改回 "本机未复验". Patch provided.

N4. instant 规则实现两遍: controller.h:111 MakeInstantTimeline (free function, documented in api.md §5.2) vs tokens.h:707 Compiler::MakeInstant (static, undocumented) — same semantics duplicated; docs only mention one. Minor; suggest api.md note that both exist, or unify. Actually the header comment says "kept separate so tokens.h is unchanged" — intentional. Documented in api.md §5.2: "与 token 的 "instant" 同规则；独立函数以免改 tokens.h" — that does mention it. So it's documented. Not a leak, skip or mention as observation only.

N5. usage.md:4 "本机曾用 g++ 复验通过，以本机最近一次跑测为准" — "最近一次跑测" implies a re-run is possible; no test harness shipped. Same as N3; fold together.

N6. Check api.md §0 line 15: "已实现 | 头文件中有定义，Linux 单元测试覆盖（估计，以本机跑测为准）" — has 估计 ✓ fine.

N7. disputes.md line 15: "UI 线程中框架自身的部分约占 12%，120Hz 下稳态 ≤1ms，动画启动时 ≤2ms（Gemini 的结论写作 ≤1.5ms）" — internal inconsistency: 12% of 8.33ms ≈ 1.0ms, fine; "Gemini 的结论写作 ≤1.5ms" vs "启动时 ≤2ms" — the parenthetical seems to say Gemini's number was 1.5ms while the adopted text says 2ms. That's a pre-existing wrinkle in disputes.md; usage.md §6 doesn't repeat these absolute numbers (it says 每阶段单独 ≤T) so no new leak introduced. Skip (disputes.md wasn't in scope of P1–P8 modifications; and it's "结论写作" vs adopted — arguably intentional recording of disagreement).

N8. api.md §5.2 ControllerSnapshot 与 header: api.md lists fields id/target/timeline/position/rate/reversed/playing/loops/clockTime/overrides/ownedProperties — header matches ✓.

N9. api.md §3.2: "Curve::Bezier(double...) // x 钳制到 [0,1]" ✓.

N10. One more possible new leak: usage.md §1.2 code `auto tokens = TokenSet::Parse(buf.str(), errors); if (!tokens) {...}` — Parse returns optional<TokenSet>; `if (!tokens)` ✓; then `tokens->Duration("normal")` ✓ (uses ->; line 65 `tokens->Duration`). ✓ correct.

N11. usage.md line 79: `sys.Create(kCard, *tokens, "card.enter")` ✓.

N12. usage.md §1.3 line 97: `std::make_shared<const Timeline>(Parallel({...}))` — Parallel returns NodePtr; Timeline(NodePtr) ✓. `Animate(Track("opacity", 0.0).To(1.0, 0.2, Curve::Bezier(0.1, 0.9, 0.2, 1.0)))` — To returns Track&, Animate takes Track by value ✓.

N13. api.md §4.3 durations range [0,600] ✓ tokens.h kMaxDuration=600 ✓.

N14. api.md §5.6: "终态（Cancelled / Superseded）会清空用户回调" ✓ matches Terminate.

N15. usage.md §1.6 line 145-149 comments ✓.

Now — should I also verify compilation? No compiler available on this machine. I can note that as the concrete fact backing the review: cannot re-verify locally, which strengthens the argument about "本机复验" wording. (The bundled workspace dependencies might include compilers? load_workspace_dependencies mentions Node.js/pnpm, not C++ compilers. Not worth checking for g++.)

One more check on usage.md line 4 vs plan.md line 68: plan.md says g++ installed & re-verified. usage.md says "本机曾用 g++ 复验通过". These are consistent with each other but both unverifiable. api.md §8 says "本机 g++ 复验". OK.

Also check usage.md §6 row 1: "实现方环境 419/617/236 项通过（估计）；本机 g++ 曾复验通过，以最近一次跑测为准" — same issue.

Also note disputes.md line 4: "文中所有人周数和市场占比都是两个模型的估计，未经核实" — good practice; plan.md line 3 similar.

Now Part 2 — WinUI3 demo roadmap. Concrete plan:

总体策略: 复用现有纯逻辑核心的全部能力（软件路径 + IDispatcher 缝），分 4 个阶段，总 2-3 人周；不做 WinMD/C# 投影（那是任务 7 的 5-7 人周），演示用 C ABI 桥（extern "C" 薄封装）或纯 C++/WinRT 工程。

阶段 0（0.5 天）: 环境验收 — Windows SDK + C++/WinRT VS 模板 + 可编译的空白 WinUI 3 应用。风险: SDK/工具链版本; 缓解: 用 VS2022 官方 WinUI3 template.
Wait — but本机无编译器 (no cl/g++ found). For the demo roadmap this is about the engineering team, fine. Actually I can mention: 前置条件是装 Visual Studio 2022 + Windows App SDK workloads.

阶段 1（1 天）: 纯逻辑 golden demo（不依赖 UI）: 控制台程序 + 记录型 Dispatcher（把每帧 Apply 落 CSV），跑 motion.json 的 card.enter / button.press / list.stagger-in + Retarget 打断，输出 golden CSV。交付物: demo_core.exe + golden.csv + README。作用: 把 419/617/236 之外做一条端到端「可看」的数据流，验证 token→controller 全链，并可作性能冒烟。风险: 无（纯头文件）。

阶段 2（2-3 天）: WinUI3 软件路径演示 app:
- C++/WinRT 工程（或 C# app + C++ DLL with C ABI）。我建议两条选一，推荐 C++/WinRT 单工程（避免投影问题）或者 C# + C++/CLI? No — C++/CLI is dead; use C# + C ABI DLL via [LibraryImport]. 实际上更简单: C++/WinRT blank app, 直接 #include 头文件（header-only），用 DispatcherQueueTimer 或 CompositionTarget.Rendering 驱动 Tick。
- UI: 一个卡片（enter/exit）、一个按钮（press 弹簧）、一个列表（stagger）、一个可拖拽小方块（Retarget 跟手 + 松手弹簧回落）。
- IDispatcher::Apply 写 XAML 属性: Translation (offsetX/Y 需映射), Opacity, Scale (ScaleTransform 或 UIElement.Scale property — WinUI3 UIElement 有 Scale/Translation 吗? UIElement has CompositeMode, and there's UIElement.Translation (Vector3) via TransitionCollection... In WinUI 3 (UWP XAML), UIElement has Scale and Translation properties (the "composition" properties on UIElement: UIElement.Scale, UIElement.Translation, UIElement.Rotation, UIElement.CenterPoint — yes, these exist on UIElement in UWP/WinUI). So Apply 直接把 samples 写到 UIElement.Scale = new Vector3(s,s,s), Translation.Y, Opacity.
- clock: SteadyClock + DispatcherQueueTimer(60Hz) 或 CompositionTarget.Rendering（跟随 vsync, 120Hz 屏自动 120Hz）。
- 演示点: 打断保留速度（拖拽中触发 enter 再点 exit）、reduced motion 开关（UISettings.AnimationsEnabled 挂钩）。
交付物: 可运行 app + 一段录屏 + README（如何构建运行）。
风险: C++/WinRT 模板构建复杂度、UIElement 属性映射（offsetY→Translation.Y 单位 DIP ✓）、计时器抖动。缓解: 用 CompositionTarget.Rendering 而非计时器；属性映射表硬编码 3-4 个属性。

阶段 3（3-5 天）: 合成器下放「演示级」实现（mini-CompositionDispatcher）:
- 实现 IDispatcher::Submit: 对 Timeline 的 PlacedTrack 列表 lowering——Linear/Bezier 段 → CompositionObject.StartAnimation(CubicBezierEasingFunction + KeyFrameAnimation); Spring 段 → SpringVector3NaturalMotionAnimation (或 SpringScalarNaturalMotionAnimation — 实际是 ScalarNaturalMotionAnimation/Vector3...)。WinUI 的 Compositor 有 CreateScalarKeyFrameAnimation, CreateCubicBezierEasingFunction, CreateSpringVector3NaturalMotionAnimation, CreateSpringScalarNaturalMotionAnimation。
- Retarget override → ExpressionAnimation 或直接重发 natural motion animation。
- 演示: 开关「软件 / 合成器」对比，显示 UI 线程帧耗时与 CPU。
- 这正是 M0/M1 要真机验证的参数对应问题，demo 就是验证脚手架。
交付物: hybrid demo + 参数对照表(Composition spring 参数 vs SpringParams response/damping 映射) + 已知差距清单。
风险: ①参数映射无把握（Period→response, DampingRatio→damping 需要换算公式: 无阻尼周期 T = 2π/ω0, ω0=√(k/m); Composition 的 SpringVector3NaturalMotionAnimation 参数是 Period 和 DampingRatio — Period 应该直接对应 response (无阻尼周期), DampingRatio 对应 damping。这需要真机验证); ②bezier y 超出 [0,1]（emphasized 的 y2=1.0 是边界内; 但 y1/y2 可超界时 CubicBezierEasingFunction 是否接受 — disputes 里已列待验证）; ③所有权切换（last-wins）在合成器侧的 Withdraw/接管顺序; ④合成器动画与 Tick 双写冲突（lowered 属性 Apply 要忽略 — IDispatcher 注释已说明 backend 可忽略）。缓解: 每动画可回退软件路径; 只在单一属性上试点。

阶段 4（1-2 天）: 设计师闭环演示: motion.json 热重载（FileSystemWatcher → Parse → 重建 TokenSet，验证 Diagnostic 报错展示）、reduced motion 绑定系统开关、演示打包（视频 + 说明）。
交付物: 录屏、demo README、开放问题清单。

总时长: 约 1.5-2 人周（不含 M0 真机验证结论依赖）。

主要技术风险与缓解汇总表:
1. 工具链: C++/WinRT 工程搭建/构建失败率高 → 用官方模板; 或退路: C# + C ABI DLL（.h 打包进 DLL, extern "C" API ~10 个函数）。
2. 合成器参数对应（Period/DampingRatio ↔ response/damping; CubicBezierEasingFunction 对 y 超 [0,1]）→ 阶段 3 做成对照实验, 记录数据, 不放进 1.0 承诺; 这正是 disputes 待验证清单。
3. 120Hz 演示环境（演示机需 120Hz 屏 + 可变刷新率）→ 提前预定硬件; 软件路径在 60Hz 也能演示。
4. 属性所有权 last-wins 在合成器侧的状态机复杂度 → demo 只演示单 target 少数属性; 完整状态机留给模块 6。
5. Tick 与渲染相位: CompositionTarget.Rendering 回调在 UI 线程、vsync 前 → 保证 Apply 同帧生效; 跨线程（渲染线程）不做。
6. 无编译器现状（本机核查: PATH 无 g++/clang/cl）→ 先装 VS2022 Build Tools + Windows SDK; 或委托有环境的队友。

Deliverables per phase 明确列出。

Part 3 — plan.md 里程碑压缩分析:

M0（第 2 周，4-6 人周）: **不可压缩**。理由: 它是全计划唯一的 go/no-go 门（"作出是否继续的决定，并重新核算估计"）；其结论（Rive 进合成表面、ALLOW_TEARING、指针属性集可用性、实测延迟）直接决定任务 6/8/9 的方案与「Skia 备选 +4-8 周」是否触发；且 4-6 人周里大部分是硬件/驱动/实测等待时间，非纯编码可并行摊薄。压缩 M0 = 用未验证假设押注后续 30 周。
M1（第 10 周）: **可压缩（部分）**。理由: M1 验收两半——「数学核心、时间线、token 无界面跑通单测」已由已交付初版（任务 1-5）覆盖大半；剩「分发器降级 + 真机验证弹簧参数对应」。压缩点: 任务 1-5 的收尾/评审可从关键路径上拿掉（已成事实），分发器（任务 6）与 M0 结论并行开工（降级规则 Claude 写、集成人等 M0）。不可压部分: 「真机验证 SpringVectorNaturalMotionAnimation 参数对应」依赖 M0 硬件与结论，无法提前。预计可回收 2-3 周，但里程碑日期建议不动、把回收时间转给 M2 的风险缓冲——因为 M1 是后续所有模块的地基，压缩它换来的日期承诺会被真机项吃回。
M2（第 16 周）: **可微压缩（1-2 周）**。理由: ETW 事件定义（任务 10, Claude 写定义）可与任务 6 并行; C# 投影样板（任务 7）的接口面在任务 4/5 完成后即稳定，可提前开工; 预览宿主 v0 的验收可砍到「token 热重载 2 秒内上屏」最小集（Gemini 1.5 秒要求留到 M4）。不可压: 性能 CI ≥2 台物理机（采购、装机、基线采集有硬性日历时间）；预览宿主/Figma 插件 8-10 人周本身是 M2 最大块，压缩它只能砍范围不能砍时间。
M3（第 22 周）: **不可压缩**。理由: Rive 宿主 8-12 人周是全计划风险最高、最不可并行摊薄的任务（渲染线程、DPI 重光栅化、设备丢失均需逐项踩坑）；试点提前到 22 周接入已是采纳 Gemini 意见后的激进值; 混合刷新率/DPI 矩阵实测需要多台真机排队。任何压缩都直接转化为 M4/M5 的返工或试点 1 的延期。
M4（第 28 周）: **不可压缩**。理由: P95/P99 达标验证依赖性能 CI 持续运行数周的基线数据（"至少 1000 帧、跑多次"），瓶颈是日历时间不是人周; 无障碍检查需外部评审 0.2 人排期; HWND 参考宿主冒烟（任务 14）虽只 1-2 人周但依赖任务 6。压缩 M4 的常见错误是把「绝对上限 + 相对回退」双门禁改成单门禁——disputes.md §2 已裁决双卡，改即违约。
M5（第 32-34 周）: **不可压缩（作为缓冲反而要守住）**。理由: 试点 2 是「更重或更老的应用」作对照，联调时长不可预测; 「无未关闭 P0/P1」是硬门槛; 且 Skia 备选（+4-8 周）如果触发, M5 的 32-34 周窗口是唯一吸收点。压缩 M5 等于取消全计划唯一的缓冲。

总结: 可压缩 = M1（数学核心已交付带来的事实提前）+ M2（范围裁剪式微压）; 不可压缩 = M0/M3/M4/M5（外部依赖/日历时间/质量门槛）。并给出一条「压缩后如何用」的建议: 把 M1/M2 回收的 3-5 周计入风险缓冲, 而不是直接前移 M3-M5 承诺日期。

Part 4 — 3 open questions (尖锐、可裁决):

Q1 (测试证据门槛): 「本机曾用 g++ 复验通过」三文档已写入, 但本目录没有任何测试源码、跑测命令或结果日志, 且经查当前机器 PATH 与常见安装位置均无 g++。请裁决: (a) 测试源码与日志必须随交付物入库, 否则「已实现」标注降级为「已交付初版、待复验」; (b) 维持现状、以实现方 Linux 结果为唯一证据, 本机复验声明删除。二选一, 下一轮后所有文档口径统一。

Q2 (M1 重定义与提前): 数学核心任务 1-5 已交付初版, M1 剩余实质只有分发器 + 合成器参数真机验证。请裁决: 是否把 M1 拆为「M1a 纯逻辑（判定: 已过）」与「M1b 合成器真机对应」, 并让任务 6 的分发器降级规则与 M0 并行开工, 把 M2 的 ETW/C# 投影提前——从而把 M2 压缩 1-2 周? 还是坚持 Claude 的串行结构（M0→分发器→Rive→指针→试点）, 压缩收益全部转为缓冲而不动承诺日期?

Q3 (演示准入标准): WinUI3 小演示是否允许第一阶段纯软件路径（UI 线程 Tick + Apply 写 UIElement.Scale/Translation/Opacity）即视为「可演示」, 还是必须包含至少一条合成器下放（KeyFrame/SpringNaturalMotion）才算? 若必须下放, 第一个试点动画选哪个（建议: button.press 的 springTo 或 card.enter 的 opacity bezier）, 且 Period/DampingRatio ↔ response/damping 的换算公式谁来先给一版（Gemini 给 WinUI 文档依据, Claude 给推导, DeepSeek 复核）?

These 3 are sharp and decidable. Good.

Now let me also double-check line numbers for citations in current files:

usage.md:
- L3: 前提 code/*.h
- L4: 测试状态 line
- L5: 未接线
- L8: engineering.md link
- L267: ../disputes.md
- L276: §5 item 4
- L278: §5 item 6
- L287-294: §6 table rows
- L296: 更多架构与里程碑 links

api.md:
- L3: code/ 范围
- L15: 已实现 估计
- L35-36: SpringParams comments (P7) — actual lines: 35 "double response = 0.35;  // 无阻尼周期（秒）；由 Spring 构造时钳制为 >= 1e-4" and 36 "double damping  = 0.8;   // 阻尼比；由 Spring 构造时钳制为 >= 0（负值会发散）"
- L286: docs/engineering.md
- L358: code/motion.json
- L468: SetReducedMotion comment (in-flight 含 Playing/Paused... 反向到 0) — fixed
- L516: §5.7 零时长 Play/Reverse — fixed
- L535: EmbedToHwnd row
- L560: §8 末尾 note

plan.md:
- L67: §8 已交付 list with duplicate 5)
- L68: g++ 安装与复验 claim
- L73-78: 文档索引 docs/ links
- §4 rows 1-5: rows 1,2 have 已交付初版; rows 3,4,5 don't.

disputes.md: L10 裁决; L13-16 帧预算; L28 Rive 不承诺帧数.

spring.h: L22-23 clamp (P7 verification).
controller.h: L293-302 Create overloads; L111 MakeInstantTimeline.
timeline.h: L415-426 Player::Retarget.
tokens.h: L282 Parse; L724 errors.clear().

For new-leak patches, I'll give concise paste-able snippets:

Patch for N1/N3 (plan.md L67-68 + usage.md L4 + api.md L560 口径统一). Provide one replacement for usage.md L4:

```markdown
> **测试状态**：timeline 419 / tokens 617 / controller 236 项为实现方 Linux 环境报告数（**估计，未随附测试源码与日志**）；「本机 g++ 复验」声明当前无法在本目录复现（目录内无测试产物，本机 PATH 亦无 g++），在随附测试源码与跑测日志前，本机状态按「未复验」标注。
```

And plan.md L68 replacement:

```markdown
- `spring.h` 已经应用 Claude 自查发现的两处修复：特大阻尼比时数值精度丢失（改用两根之积计算）、阻尼比下限钳制为 0。**早期本机一度无编译器；其后声称已安装 g++ 并复验 timeline 419 / tokens 617 / controller 236，但测试源码与日志未随附，本机复验状态待证实。**
```

api.md L560 replacement:

```markdown
> 用例数来自实现方 Linux 环境；「本机 g++ 复验」无随附测试产物可核验。上表为**估计**，以随附测试源码在本机实跑为准（当前本机无编译器，未复验）。
```

Hmm — but careful: I shouldn't overclaim that the machine never had g++. I verified: PATH 无 g++/clang/cl/cmake/ninja; common roots 无 g++.exe. That's current state. The doc says "曾用 g++ 复验" (past tense). It's possible someone installed g++ earlier and removed it. My finding: 当前无法复现/无法核验 — that's accurate and fair. State it that way.

Patch for N2 (plan.md duplicate numbering):

```
5）可打断控制器与重定向，已交付 → `code/controller.h`（本机 236 项全过）；6）重定向控制器；7）能力分发器的降级规则；...
```
Wait the current numbering: "5）可打断控制器与重定向，已交付 → code/controller.h（本机 236 项全过）；5）重定向控制器；6）能力分发器的降级规则；7）ETW 事件定义；8）测试工具与 CI 分析脚本；9）技术验证脚手架" — so renumber: 5）可打断控制器与重定向（已交付）；6）能力分发器的降级规则；7）ETW 事件定义；8）测试工具与 CI 分析脚本；9）技术验证脚手架。And remove the duplicate "5）重定向控制器；" (retarget is already covered by item 5). Patch:

```markdown
5）可打断控制器与重定向，已交付 → `code/controller.h`；6）能力分发器的降级规则；7）ETW 事件定义；8）测试工具与 CI 分析脚本；9）技术验证脚手架（由人运行并解读结果）。
```

And §4 rows 3/4/5: add 「**Claude**（已交付初版）」:

```markdown
| 3 | 时间线、轨道、关键帧图，时钟抽象 | 1 | 4-6 | **Claude**（已交付初版） |
| 4 | Token schema、校验器、加载器 | 1、3 | 3-4 | **Claude**（已交付初版） |
| 5 | 可打断控制器、句柄、重定向 | 1、3、6 | 4-6 | **Claude**（已交付初版），人评审 |
```
Wait row 5's dependency includes 6 (能力分发器)? "5 | 可打断控制器、句柄、重定向 | 1、3、6" — controller depends on 6? The controller.h includes tokens.h/timeline.h only; the dispatcher seam is in controller.h but module 6 plugs into it. Dependency "6" on row 5 looks odd but that's plan.md's table; not my business to fix unless flagging. Actually interesting: row 5 depends on 6 (能力分发器) yet controller was delivered without it. I could note this dependency edge as questionable — controller.h already defines IDispatcher seam without module 6, so row 5's dependency on 6 seems reversed (6 depends on 5's seam). This is a genuine plan-level observation worth one line in part 3: 任务 5 的依赖列写「1、3、6」但 controller.h 已交付且只依赖 timeline/tokens——依赖边 5→6 应改为 6→5。Good catch, include as a small new leak / plan inconsistency.

Patch for P8 (usage.md L8, L267, L296; api.md L3, L286, L358, L560; plan.md L73-78):

usage.md L8:
```markdown
配套 API 清单见 [`api.md`](./api.md)。架构背景见 `engineering.md`（**未随附本目录**）。
```

usage.md L267:
```markdown
真实接线 **待真机验证**（DWM / `Windows.UI.Composition` 参数对应关系）。Rive 指针跟随延迟见下方限制表与 [`disputes.md`](./disputes.md)。
```

usage.md L296:
```markdown
更多架构与里程碑：[`engineering.md`](./engineering.md)（未随附）、[`plan.md`](./plan.md)、[`summary.md`](./summary.md)（未随附）、[`disputes.md`](./disputes.md)、[`deepseek_review.md`](./deepseek_review.md)（未随附）。
```

api.md L3:
```markdown
> **范围**：本文档只描述随附头文件（`code/` 或本目录，header-only C++17）中已实现的公开接口。
```
Better: "本文档只描述已实现的 C++17 头文件公开接口（本目录内的 `*.h`，原仓库位于 `code/`）。"

api.md L286: `docs/engineering.md` → `engineering.md（未随附本目录）`.
api.md L358: `code/motion.json` → `motion.json（本目录随附）`.
api.md L560: deepseek_review.md → 标注未随附（与 P6 patch 合并）。
plan.md 文档索引: docs/... → ./... 或标注未随附。

OK. Also double-check one subtle thing for P2: usage.md §5 item 4 says "MotionSystem::Create(TargetId, tokens, transition)" — the actual param is string_view transition; fine as shorthand.

Also check the claim in api.md §8 heading "（估计，本机 Linux）" — "本机 Linux"? The machine is Windows (C:\Users\admin...). "本机 Linux" is odd phrasing — probably means "Linux（本机）". Previous round heading was "## 8. 测试覆盖（估计，本机 Linux）" — hmm, was that there before? Previous round cited api.md §8 第 548 行「估计，以本机跑测为准」; the heading presumably said something similar. The phrase "本机 Linux" on a Windows machine is contradictory — this machine is Windows, so "本机 Linux" cannot be right; tests ran on Linux (WSL? or the implementer's Linux). Minor new leak: api.md §8 标题「（估计，本机 Linux）」把「本机」和「Linux」并置，与当前 Windows 主机矛盾，建议改为「（估计，Linux 环境）」。Nice catch — include as small item.

Also usage.md §6 row 1: "实现方环境 419/617/236 项通过（估计）；本机 g++ 曾复验通过，以最近一次跑测为准" — same issue; covered.

Now, also confirm P3's new rows vs previous recommendation: previous P3 recommended 帧预算 row text "每阶段单独 ≤1/T" — current shipped "每阶段单独 ≤T" (corrected, better). ✓ good.

One more check on usage.md §6 row 帧预算: "UI≈12%、Rive 渲染≈35%、GPU≈35%，每阶段单独 ≤T" — plan says 12% of T for UI at 120Hz steady ≤1ms. Consistent.

Also double-check api.md L468 for the SetReducedMotion comment (was a previous-round minor fix): current text "on→true：对每个活跃控制器 SnapToEnd（in-flight 含 Playing/Paused：正向到终点、反向到 0，并 Completed）" ✓ fixed. And api.md §5.7 L516 "零时长时间线在 Play / Reverse 时立即完成（跳过 markers）" ✓ fixed. Both beyond-P1-P8 fixes landed. I'll mention them briefly as "上轮两项次要精度项也已顺带修好".

Now let me also sanity check the claim in usage.md L4 "以本机最近一次跑测为准" — "最近一次" implies a most-recent run exists; with no logs, unfalsifiable. Covered.

Also: should I double check whether there's an actual tests folder somewhere, like C:\Users\admin\Temp\motion_framework_review has no tests. Confirmed by directory listing: only 10 files. ✓

Also the review directory has no engineering.md/summary.md/deepseek_review.md. ✓

One more verification: is there maybe a "docs" subfolder? Directory listing showed only files, no dirs (Get-ChildItem -Recurse -File shows files only). Let me not bother; the flat listing implies no subdirectories with files. Actually -Recurse -File would show files in subdirs; none appeared, so no subdirs with files. ✓

Now structure the final answer in Chinese with 4 parts. It should be thorough but organized. Include:

0. 一句话结论 + 我做了什么核查（读了 10 个文件、对比上一轮 dsh_motion_reply_only.md、本机编译器核查）。

1. 补丁复核: P1–P8 table with verdicts + evidence (line numbers), then 新漏网 (N1–N5) with paste-able patches.

2. WinUI3 demo route: phases 0-4 with deliverables, risks & mitigations, total estimate.

3. plan.md milestones M0–M5: compressible or not with reasons.

4. Exactly 3 open questions.

Length: this is a dense review; aim for comprehensive but not bloated. Use tables where useful. Cite line numbers. All Chinese.

Let me draft the P1-P8 table verdicts precisely:

- P1 部分到位: usage.md:4 已有「（估计）」与「以本机最近一次跑测为准」，且 §6 与 api.md §8 同步；但原建议的「本机无编译器/未复验」被改写成更强的「本机曾用 g++ 复验通过」——本目录无任何测试源码/日志佐证，且本机当前 PATH 与常见位置查无 g++。建议随附测试产物或改回「未复验」口径。
- P2 已到位: usage.md:276、278 与建议文本逐字一致。
- P3 已到位: usage.md:290-292 三行齐全（HWND→V1.5、Rive 不承诺帧数、帧预算流水线每阶段 ≤T），引用 disputes/plan 正确。
- P4 已到位: usage.md:256-257 注释含「IClock&」与「投影需自带/注入时钟」。
- P5 已到位: api.md:535 有 EmbedToHwnd/HWND → V1.5 行。
- P6 部分到位: api.md:560 有「估计」字样，但同样把「本机未编译」替换为「本机 g++ 复验」的新断言；且「详见 deepseek_review.md」指向不存在的文件。
- P7 已到位: api.md:35-36 两处均为「由 Spring 构造时钳制」，与 spring.h:22-23 实现一致。
- P8 未到位: usage.md:3/8/267/296、api.md:3/286/358/560、plan.md:73-78 的路径仍混用 code/、docs/、./、../，engineering.md/summary.md/deepseek_review.md 未随附。

新漏网:
- N1 (口径残留): plan.md:67-68「本机 419/617/236 项全过」「其后已安装 g++ 并复验」无「估计」限定，与 plan.md:3「所有人周…都是估计」及 usage/api 的估计口径冲突；本目录无测试产物。Patch.
- N2 (上轮遗留未修): plan.md:67 重复「5）」编号；§4 任务表第 3/4/5 行缺「已交付初版」（第 1/2 行有）。
- N3 (上轮遗留已修确认): api.md:468 与 api.md:516 的两处精度问题已修好（提一下）。
- N4 (依赖边写反): plan.md §4 任务 5 依赖列「1、3、6」，但 controller.h 已交付且只依赖 timeline/tokens；IDispatcher 缝在 controller.h 内、模块 6 反依赖它。依赖边应 6→5 而非 5→6。
- N5 (小措辞): api.md §8 标题「（估计，本机 Linux）」与当前 Windows 主机矛盾。
- N6: usage.md:4「本机曾用 g++ 复验通过」+ §6 同句 — 合并进 N1/P1。

Patches for N1/N2/N4/N5/P8/P6 — provide compact snippets.

For part 3, remember to cite M0–M5.

For part 4, exactly 3 questions.

One consideration: the user asked "若有新漏网，给出可粘贴修订片段" — provide patches for new leaks. I'll provide patches for: usage.md L4 (test wording), plan.md L67-68 (duplicate numbering + claim), plan.md §4 rows 3-5 + dependency 5→6, P8 link fixes, api.md §8 heading + L560. Keep them compact.

Also part 2 must be "分阶段路线、每阶段交付物、主要技术风险与缓解" — I'll do a table per phase + risk table.

Also note in part 2: the demo must avoid the WinMD/C# 投影 (5-7 人周) — the route uses C ABI shim or pure C++/WinRT; that's the key architectural choice making it "小样本可演示" cheaply. Also emphasize software path is fully supported today (usage.md §1), compositor path is demo-level mini lowering.

Word budget: it's a heavy review; the user expects thorough. I'll aim for a well-structured answer, moderately long. Use markdown tables and code blocks. All Chinese. Avoid excessive fluff.

Let me write the final answer now. I'll cite file paths with line numbers. Note instructions about linking files: I should link file mentions. The instruction says outside commands/code blocks, link every mention of an existing file to its full path relative to working directory. That would be heavy for this many mentions. The rule: "Outside commands, configuration expressions, and code blocks, link every mention of an existing file". Hmm. That's for the GUI. Given the volume of file mentions, I'll link key ones (first mention of each file) with relative paths like [usage.md](<usage.md>). Working directory is C:\Users\admin\Temp\motion_framework_review. Relative paths from workspace root. I'll use relative links without angle brackets except where needed. Keep it reasonable: link first occurrence of each file, and use plain code-format for line refs like `usage.md:4` — hmm, but then "usage.md" inside backticks is a code span, allowed unlinked? The instruction says outside code blocks link every mention; code spans presumably exempt. To be safe and useful, I'll link first mentions and keep line refs in backticks (code spans). That should be acceptable.

Let me also decide the P1 verdict wording: The user asked "已到位 / 部分到位 / 未到位 / 仍有漏网". For P1: 部分到位 (标记已加，但新断言比原问题更激进，且无佐证). P6: 部分到位. P8: 未到位. Others: 已到位.

One more sanity check on P1: the previous recommendation text (from dsh_motion_reply_only.md) was: "测试状态：timeline 419 / tokens 617 / controller 236 项在实现方 Linux 环境通过（估计）；plan.md §8 注明本机尚无 C++ 编译器、尚未本地编译验证，以本机复测为准。" Current: "timeline 419 / tokens 617 / controller 236 项在实现方 Linux 环境通过（**估计**）；本机曾用 g++ 复验通过，以本机最近一次跑测为准。" So yes: 估计 kept, but "本机无编译器/未验证" → "本机曾用 g++ 复验通过". The direction changed. Since plan.md now claims g++ was installed, the docs are internally consistent with plan.md but the underlying claim is unverifiable — I checked and found no g++ now. So "部分到位" with a new-evidence gap is the right verdict, plus note the option that g++ may have existed transiently — but as a deliverable it must be reproducible. Good.

Also I should double check plan.md line 68: "早期本机一度无编译器；其后已安装 g++ 并复验 timeline 419 / tokens 617 / controller 236。" — yes. This appears to be a NEW claim added in the update (previous round plan.md:68 said "这台电脑上没有 C++ 编译器，代码尚未编译或测试"). So the update flipped the claim. My verification: no g++ now. Also note plan.md:67 says "本机 419 项全过" for timeline in §8 — wait, actually line 67 says "3）时间线和时钟，已交付 → code/timeline.h（本机 419 项全过）" and "4）...tokens.h + motion.json（本机传 motion.json 参数时 617 项全过）" and "5）...controller.h（本机 236 项全过）". Right.

In the final answer, for the P1 verification I'll state: 经查本机 PATH 无 g++/clang++/cl/cmake/ninja，常见安装目录亦无 g++.exe；目录内无测试源码/日志 → 「本机复验」无法重现。Hedged: 不排除曾短暂安装后卸载，但交付物形态下不可核验。

Now write it. Structure:

# 总览（几行）
- 核查方法说明。

## 1) 补丁复核 P1–P8
Table + details + 新漏网 N1-N5 + patches.

## 2) WinUI3 可演示小样本技术路线
Phase table + risks.

## 3) 对照 plan.md 里程碑 M0–M5 压缩分析
Table + reasoning.

## 4) 开放问题（恰好 3 个）

Keep patches compact. Let me write.

For patch N1 (usage.md L4 replacement) — full replacement of line 4:

```markdown
> **测试状态**：timeline 419 / tokens 617 / controller 236 项为实现方 Linux 环境报告数（**估计**）。本目录未随附测试源码与跑测日志；本机当前 PATH 无 g++（「曾复验」无法重现）。在测试随附前，本机复验状态标注为**未复验**。
```

plan.md L68 replacement:

```markdown
- `spring.h` 已经应用 Claude 自查发现的两处修复：特大阻尼比时数值精度丢失（改用两根之积计算）、阻尼比下限钳制为 0。**早期本机一度无编译器；其后声称安装 g++ 并复验 timeline 419 / tokens 617 / controller 236，但测试源码与日志未随附（当前机器 PATH 亦无 g++），此复验声明待证实。**
```

api.md L560 replacement:

```markdown
> 用例数来自实现方 Linux 环境；「本机 g++ 复验」无随附测试产物可核验（当前本机无编译器）。上表为**估计**；建议随附测试源码与跑测命令，以本机实跑为准。deepseek_review.md 未随附本目录。
```

plan.md L67 renumber patch:

```markdown
5）可打断控制器与重定向，已交付 → `code/controller.h`；6）能力分发器的降级规则；7）ETW 事件定义；8）测试工具与 CI 分析脚本；9）技术验证脚手架（由人运行并解读结果）。
```

plan.md §4 rows patch:

```markdown
| 3 | 时间线、轨道、关键帧图，时钟抽象 | 1 | 4-6 | **Claude**（已交付初版） |
| 4 | Token schema、校验器、加载器 | 1、3 | 3-4 | **Claude**（已交付初版） |
| 5 | 可打断控制器、句柄、重定向 | 1、3 | 4-6 | **Claude**（已交付初版），人评审 |
```
(把依赖「1、3、6」改为「1、3」— note as part of N4: controller.h 只依赖 timeline/tokens；「6」依赖边反了。)

api.md §8 heading patch: 「## 8. 测试覆盖（估计，Linux 环境）」.

P8 patches as above.

Also for api.md L3: keep simple:

```markdown
> **范围**：本文档只描述随附头文件（本目录 `*.h`，原仓库路径 `code/`）已实现的 C++17 公开接口。
```

api.md L286: "`docs/engineering.md`" → "`engineering.md`（未随附本目录）".
api.md L358: "示例文件：`code/motion.json`" → "示例文件：[`motion.json`](./motion.json)（本目录随附）".

usage.md L267: "`../disputes.md`" → "`./disputes.md`"（即 [`disputes.md`](./disputes.md)）.
usage.md L296 rewrite as above.

OK. Now also for part 2 details, let me flesh out:

阶段 0（0.5 天）：工具链与模板
- 交付物: 可构建的空白 WinUI 3 工程（官方 C++/WinRT 或 C# 模板），VS2022 + Windows SDK + Windows App SDK 验证脚本。
- 风险: 模板/SDK 版本组合问题；缓解: 固定 Windows App SDK 版本（1.5/1.6），CI 用 GitHub Actions windows-latest。

阶段 1（0.5-1 天）：纯逻辑 golden demo（不碰 UI）
- demo_core: ManualClock + 记录型 Dispatcher（Apply 写 CSV/JSONL）+ motion.json 四场景（card.enter/exit、button.press、list.stagger-in、拖拽 Retarget 序列），输出 golden 文件与 P95 Tick 耗时。
- 交付物: demo_core.exe、golden 输出、构建脚本（-std=c++17 一行命令）。
- 风险: 基本无；只验证 token→controller 全链 + 本机构建可行性（**当前本机无编译器，需先装**）。

阶段 2（2-3 天）：WinUI3 软件路径 app
- 工程选型 A（推荐）: C++/WinRT 单工程直接 #include 五个头文件（header-only，无投影问题）；选型 B（退路）: C# app + C ABI DLL（extern "C" 约 10 个函数封装 MotionSystem）。
- 时钟与帧: SteadyClock；驱动用 CompositionTarget.Rendering（跟随 vsync，120Hz 屏自动 120Hz）或 DispatcherQueueTimer(16ms)。
- Apply 映射: opacity→Opacity；offsetY→Translation.Y；scale→Scale（Vector3 均匀）；offsetX→Translation.X。IDispatcher::Apply 里写 UIElement 属性。
- 演示交互: 卡片 enter/exit（tokens）、按钮 press（springTo inheritVelocity）、列表 stagger、拖拽方块 Retarget 跟手 + 松手回弹（保留速度）；右下角 reduced motion 开关（绑定 UISettings.AnimationsEnabled）。
- 交付物: 可运行 app、操作脚本、60/120Hz 录屏。
- 风险: C++/WinRT 模板构建细节多 → 退路 C#+C ABI；UIElement.Scale/Translation 属性在 WinUI3 存在（composition 属性）→ 若版本不支持退到 RenderTransform 或 ScaleTransform；时钟抖动 → CompositionTarget.Rendering。

阶段 3（3-5 天）：mini-CompositionDispatcher（下放试点）
- IDispatcher::Submit lowering: Track 的 Linear/Bezier 段 → Compositor.CreateScalarKeyFrameAnimation + CreateCubicBezierEasingFunction；Spring 段 → CreateSpringScalarNaturalMotionAnimation；Retarget → 重发 natural motion（或 ExpressionAnimation 接管）；Withdraw → StopAnimationGroup。
- 选 1 个属性试点（建议 button.press 的 scale 弹簧），做软件/合成器双路径开关与参数对照实验（Period/DampingRatio ↔ response/damping、CubicBezierEasingFunction 对 y∉[0,1] 的接受度——正是 disputes.md 待验证清单）。
- 交付物: hybrid demo、参数对照数据表、M0/M1 验证脚手架雏形、已知差距清单。
- 风险: 参数映射无把握（这是计划里「必须由人真机验证」项）→ 阶段 3 定位为实验而非承诺；所有权 last-wins 在合成器侧的状态机复杂度 → 只做单 target 单属性；lowered 与 Apply 双写 → IDispatcher 注释已定义 Apply 可忽略 lowered 属性。

阶段 4（1-2 天）：设计师闭环与打包
- motion.json 热重载（FileSystemWatcher→Parse→错误浮层）、reduced motion 跟随系统开关、README + 录屏 + 给三模型的观察问题清单。
- 交付物: 演示包。
- 风险: 低。

总投入: 6-10 人日 ≈ 1.5-2 人周（不含装工具链与等硬件）。
关键决策: 不做 WinMD/C# 投影（任务 7 的 5-7 人周）——演示走 C++/WinRT 直连或 C ABI 桥；不做 Rive（任务 8）——演示只用 XAML 元素。

风险总表:
1. 工具链（本机现无编译器；C++/WinRT 模板脆弱）→ 先装 VS2022/Windows SDK；退路 C ABI。
2. 合成器参数对应（Period/DampingRatio、bezier y 超界）→ 做成对照实验输出数据，正是 M0 待验证项。
3. 120Hz 演示硬件 → 提前预定；软件路径 60Hz 兜底。
4. 下放状态机（last-wins/Withdraw）→ 试点单属性；完整实现留模块 6。
5. 帧驱动与 UI 线程 → 全部走 UI 线程 + CompositionTarget.Rendering，不做跨线程（与头文件线程规则一致）。

Part 3 table:

| 里程碑 | 可否压缩 | 理由 |
M0 不可压缩: go/no-go 门 + 硬件实测日历时间 + Skia 备选触发点。
M1 可压缩（部分）: 任务1-5已交付初版使「无界面跑通单测」过半达成；分发器规则可与 M0 并行；但「真机验证弹簧参数对应」依赖 M0 不可前移。
M2 可微压缩（1-2周）: ETW 定义与 C# 投影样板可提前并行；预览宿主 v0 砍到热重载最小集；但性能 CI ≥2 台物理机采购/基线是日历时间。
M3 不可压缩: Rive 宿主 8-12 人周风险最高；试点 22 周已激进；混合刷新率/DPI 矩阵需真机排队。
M4 不可压缩: P95/P99 达标需要数周基线数据（1000 帧×多次）；无障碍外部评审排期；双门禁是 disputes 裁决不可改单。
M5 不可压缩: 试点 2 联调不可预测 + P0/P1 清零门槛 + Skia 备选(+4-8周)唯一吸收点。

结论段: 唯一真实的「压缩」来自已交付事实（M1 内任务 1-5），建议把回收人周转为 M2-M5 缓冲而非前移承诺日期；另指出依赖边 5→6 写反这一处计划级错误。

Part 4 questions — write them crisp, each with 裁决选项.

Q1: 测试证据门槛（裁决式）
Q2: M1 拆分与并行开工（裁决式）
Q3: 演示准入标准 + 第一个下放试点（裁决式）

Format each: 背景一句话 + 选项 A/B + 需要谁给什么（Gemini 查 WinUI 文档、Claude 给推导、DeepSeek 复核）— for Q3 mention division of labor.

Now write the final response. Keep it structured, Chinese. Include the table for P1-P8 and per-item evidence. Reasonable length — this is a big deliverable, so length is justified, but I'll keep prose tight.
# 四部分审阅答复（基于对 10 个文件的逐行重读 + 上一轮原文比对 + 本机实证核查）

**核查方式说明**：我重读了 [api.md](<api.md>)、[usage.md](<usage.md>)、[plan.md](<plan.md>)、[disputes.md](<disputes.md>) 与五个头文件（spring/bezier/timeline/tokens/controller）+ motion.json；调出上一轮建议原文（`C:\Users\admin\Temp\dsh_motion_reply_only.md`）逐条比对；并实测核查：**本机 PATH 无 g++/clang++/cl/cmake/ninja，常见安装目录亦无 g++.exe**；目录内**无任何测试源码、跑测日志**；engineering.md / summary.md / deepseek_review.md **均不存在**。

---

## 1) 补丁复核 P1–P8

| # | 判定 | 证据与说明 |
|---|------|-----------|
| P1 | **部分到位** | `usage.md:4` 已加「（**估计**）」与「以本机最近一次跑测为准」，但上一轮建议的「本机尚无编译器、尚未本地验证」被**改写成更强的断言**「本机曾用 g++ 复验通过」。本目录无任何测试产物可佐证，且本机当前查无 g++（不排除曾短暂安装后卸载，但交付物形态下不可核验）。 |
| P2 | **已到位** | `usage.md:276` 已为 `MotionSystem::Create(TargetId, tokens, transition)`；`usage.md:278` 已为 `AnimationHandle::Retarget`，与上轮建议逐字一致。 |
| P3 | **已到位** | `usage.md:290-292` 三行齐全：HWND→V1.5（disputes §1）、Rive「不承诺帧数」+实测为交付指标（disputes §3）、帧预算「每阶段单独 ≤T、8.3ms 是红线不是可加总预算」（disputes §2）。且「≤1/T」已订正为「≤T」。 |
| P4 | **已到位** | `usage.md:256-257` 注释已含 `MotionSystem(IClock&, IDispatcher*)` 与「投影需自带/注入时钟」。 |
| P5 | **已到位** | `api.md:535` 有「`EmbedToHwnd` / HWND 宿主正式 API → **V1.5**；首发仅随附不受支持的参考示例」。 |
| P6 | **部分到位** | `api.md:560` 有「估计」字样，但同样把「本机未编译」替换为「本机 g++ 复验」新断言，且「详见 deepseek_review.md」指向**不存在的文件**。 |
| P7 | **已到位** | `api.md:35-36` 两处均为「由 Spring 构造时钳制」，与 `spring.h:22-23` 实现一致（钳制发生在 `Spring` 构造器，`Params()` 返回钳制后值）。 |
| P8 | **未到位** | 路径仍混用：`usage.md:3`「code/*.h」、`usage.md:8` `./engineering.md`（不存在）、`usage.md:267` `../disputes.md`（指向目录外）、`usage.md:296` `../plan.md` `../summary.md`、`api.md:3/286/358` `code/`、`docs/engineering.md`、`code/motion.json`、`plan.md:73-78` 文档索引全为 `docs/...`。engineering/summary/deepseek_review 未随附。 |

**顺带确认**：上一轮提出的两处 api.md 精度项（§5.4「in-flight 含 Playing/Paused、反向到 0」、§5.7「零时长在 Play/Reverse 立即完成」）也已在 `api.md:468`、`api.md:516` 修好。

### 新漏网（可粘贴修订片段）

**N1 —「本机 g++ 复验」是新断言且零证据（P1/P6 的残留，最要紧）**
`plan.md:67`「本机 419/617/236 项全过」、`plan.md:68`「其后已安装 g++ 并复验」为事实口吻，与 `plan.md:3`「所有人周和工期都是估计」及 usage/api 的估计口径冲突。修订：

```markdown
<!-- usage.md 第 4 行替换为 -->
> **测试状态**：timeline 419 / tokens 617 / controller 236 项为实现方 Linux 环境报告数（**估计**）。本目录未随附测试源码与跑测日志，本机当前 PATH 亦无 g++，「本机曾复验」无法重现；随附测试前本机状态按**未复验**标注。

<!-- plan.md 第 68 行结尾替换为 -->
- ……**早期本机一度无编译器；其后声称安装 g++ 并复验 419/617/236，但测试源码与日志未随附（当前机器 PATH 无 g++），该复验声明待证实。**

<!-- api.md 第 560 行替换为 -->
> 用例数来自实现方 Linux 环境；「本机 g++ 复验」无随附测试产物可核验（本机当前无编译器）。上表为**估计**；建议随附测试源码与跑测命令，以本机实跑为准。`deepseek_review.md` 未随附本目录。
```

**N2 — plan.md §8 重复编号与 §4 表格缺标注（上轮已指出、仍未修）**

```markdown
<!-- plan.md 第 67 行：删掉重复的「5）重定向控制器」，后续顺延 -->
5）可打断控制器与重定向，已交付 → `code/controller.h`；6）能力分发器的降级规则；7）ETW 事件定义；8）测试工具与 CI 分析脚本；9）技术验证脚手架（由人运行并解读结果）。

<!-- plan.md §4 表格第 3/4/5 行补「已交付初版」，与第 1/2 行及 §8 对齐 -->
| 3 | 时间线、轨道、关键帧图，时钟抽象 | 1 | 4-6 | **Claude**（已交付初版） |
| 4 | Token schema、校验器、加载器 | 1、3 | 3-4 | **Claude**（已交付初版） |
| 5 | 可打断控制器、句柄、重定向 | 1、3 | 4-6 | **Claude**（已交付初版），人评审 |
```

**N3 — plan.md 依赖边写反**：任务 5 的依赖列「1、3、**6**」，但 controller.h 已交付且只依赖 timeline/tokens，`IDispatcher` 缝定义在 controller.h 内，是模块 6 反依赖任务 5。应改为依赖「1、3」，并在任务 6 的依赖列加「5」。（上表已顺手改。）

**N4 — api.md §8 标题自相矛盾**：「测试覆盖（估计，**本机 Linux**）」把「本机」（Windows 主机）与「Linux」并置。改为「## 8. 测试覆盖（估计，Linux 环境）」。

**N5 — P8 的具体修订**：

```markdown
<!-- usage.md 第 8 行 -->
配套 API 清单见 [`api.md`](./api.md)。架构背景见 `engineering.md`（**未随附本目录**）。

<!-- usage.md 第 267 行 -->
真实接线 **待真机验证**（DWM / `Windows.UI.Composition` 参数对应关系）。Rive 指针跟随延迟见下方限制表与 [`disputes.md`](./disputes.md)。

<!-- usage.md 第 296 行 -->
更多架构与里程碑：[`plan.md`](./plan.md)、[`disputes.md`](./disputes.md)；未随附：`engineering.md`、`summary.md`、`deepseek_review.md`。

<!-- api.md 第 3 行 -->
> **范围**：本文档只描述随附头文件（本目录 `*.h`，原仓库路径 `code/`）中已实现的 C++17 公开接口。

<!-- api.md 第 286 行：`docs/engineering.md` → `engineering.md`（未随附本目录）
     api.md 第 358 行：`code/motion.json` → [`motion.json`](./motion.json)（本目录随附）
     plan.md 第 73-78 行文档索引：`docs/...` → `./...`，不存在者标注「未随附」 -->
```

---

## 2) WinUI3 可演示小样本技术路线

**核心决策：绕过 WinMD/C# 投影（任务 7 的 5–7 人周）与 Rive（任务 8 的 8–12 人周）。** 现有核心已完整支持「软件路径」（usage.md §1：自供时钟 + IDispatcher::Apply 写回 UI），演示先 100% 走这条路；下放只做一个「演示级」单属性试点。总投入约 **6–10 人日（1.5–2 人周）**，不含装工具链。

| 阶段 | 内容 | 交付物 | 主要风险 → 缓解 |
|---|---|---|---|
| **0（0.5 天）工具链** | 装 VS2022 + Windows SDK + Windows App SDK；用官方模板建空白 WinUI3 工程并跑通 | 可构建空白工程 + 版本固定说明 | 模板/SDK 版本组合易碎 → 固定 Windows App SDK 版本；**当前本机无任何编译器，此步为硬前置** |
| **1（0.5–1 天）纯逻辑 golden demo** | 控制台程序：ManualClock + 记录型 Dispatcher（Apply 落 CSV/JSONL），跑 motion.json 四场景（card.enter/exit、button.press、list.stagger-in、拖拽 Retarget 序列），输出 golden 文件与 Tick 耗时 | demo_core.exe + golden 输出 + `g++ -std=c++17` 一行构建脚本 | 几乎无风险；顺带验证 token→controller 全链，并成为 P1/P6「本机复验」的证据来源 |
| **2（2–3 天）软件路径 app** | 方案 A（推荐）：C++/WinRT 单工程直接 `#include` 五个头文件（header-only，无投影问题）；方案 B（退路）：C# app + C ABI DLL（`extern "C"` 约 10 个函数封装 MotionSystem）。帧驱动用 `CompositionTarget.Rendering`（跟 vsync，120Hz 屏自动 120Hz）；Apply 映射：opacity→Opacity、offsetY→Translation.Y、scale→Scale(Vector3)、offsetX→Translation.X。交互：卡片进/出、按钮弹簧按压（inheritVelocity）、列表 stagger、**拖拽方块 Retarget 跟手 + 松手速度保留回弹**；reduced motion 挂钩 `UISettings.AnimationsEnabled` | 可运行 app + 操作脚本 + 60/120Hz 录屏 | C++/WinRT 模板细节多 → 退路 C ABI；`UIElement.Scale/Translation` 属性若版本不支持 → 退到 RenderTransform；时钟抖动 → 用 Rendering 而非计时器 |
| **3（3–5 天）mini-CompositionDispatcher（下放试点）** | 实现 `IDispatcher::Submit` lowering：Linear/Bezier 段 → `CreateScalarKeyFrameAnimation` + `CreateCubicBezierEasingFunction`；Spring 段 → `CreateSpringScalarNaturalMotionAnimation`；Retarget → 重发 natural motion 或 ExpressionAnimation；Withdraw → StopAnimationGroup。**只试点 1 个属性**（建议 button.press 的 scale 弹簧），做软件/合成器双路径开关与参数对照实验（Period/DampingRatio ↔ response/damping、bezier y∉[0,1] 是否被接受——正是 disputes.md 待验证清单） | hybrid demo + 参数对照数据表 + M0/M1 验证脚手架雏形 + 已知差距清单 | 参数映射无把握（plan.md 明列「必须由人真机验证」）→ 本阶段定位为**实验**而非承诺；last-wins 在合成器侧的状态机复杂度 → 只做单 target 单属性，完整状态机留模块 6；lowered 与 Apply 双写 → IDispatcher 注释已约定 Apply 可忽略 lowered 属性 |
| **4（1–2 天）设计师闭环** | motion.json 热重载（FileSystemWatcher→Parse→Diagnostic 错误浮层）、reduced motion 跟随系统开关、README + 录屏 + 观察问题清单 | 演示包 | 低 |

**风险总表**：① 工具链（本机无编译器、C++/WinRT 脆弱）——先装工具链，C ABI 为退路；② 合成器参数对应——阶段 3 输出对照数据、不进 1.0 承诺；③ 120Hz 演示硬件——提前预定，软件路径 60Hz 兜底；④ 线程——全部走 UI 线程 + Rendering 回调，与头文件「单 dispatcher 线程」规则一致，不做跨线程下放。

---

## 3) 对照 plan.md 里程碑的压缩分析

| 里程碑 | 能否压缩 | 理由（引用编号与内容） |
|---|---|---|
| **M0（第 2 周，4–6 人周）** | **不能** | 全计划唯一的 go/no-go 门（「作出是否继续的决定，并重新核算估计」）。Rive 进合成表面、ALLOW_TEARING、指针属性集、实测延迟的结论直接决定任务 6/8/9 方案，以及 **Skia 备选（+4–8 周）是否触发**；其中大量是硬件/驱动/实测等待，非编码可摊薄。压缩 M0 = 用未验证假设押注后续 30 周。 |
| **M1（第 10 周）** | **可压缩（部分）** | 验收两半：①「数学核心、时间线、token 无界面跑通单测」——任务 1–5 已交付初版，此半**已成事实**，收尾/评审可从关键路径拿掉；②「分发器降级 + 真机验证弹簧参数对应」——分发器降级规则（Claude 写）可与 M0 并行开工。**不可压**：真机验证 `SpringVectorNaturalMotionAnimation` 参数对应依赖 M0 硬件与结论。预计回收 2–3 周。 |
| **M2（第 16 周）** | **可微压缩（1–2 周）** | ETW 事件定义（任务 10）可与任务 6 并行；C# 投影样板（任务 7）在任务 4/5 完成后接口面已稳，可提前；预览宿主 v0 验收可砍到「token 热重载 2 秒内上屏」最小集（Gemini 的 1.5 秒留到 M4）。**不可压**：性能 CI ≥2 台物理机（采购、装机、基线采集是日历时间）；Figma 插件 8–10 人周只能砍范围。 |
| **M3（第 22 周）** | **不能** | Rive 宿主 8–12 人周是全计划风险最高项（渲染线程、DPI 重光栅化、设备丢失逐项踩坑），几乎不可并行摊薄；试点提前到 22 周接入**已是采纳 Gemini 意见后的激进值**；混合刷新率/DPI 矩阵实测需多台真机排队。再压缩只会把成本转移成 M4/M5 返工。 |
| **M4（第 28 周）** | **不能** | P95/P99 达标依赖性能 CI 数周基线（「每场景至少 1000 帧、跑多次」），瓶颈是日历时间；无障碍需外部评审 0.2 人排期；且「绝对上限 + 相对回退」**双门禁是 disputes.md §2 的裁决**，压缩时改单门禁即违约。 |
| **M5（第 32–34 周）** | **不能（作为缓冲反而要守住）** | 试点 2 是「更重或更老的应用」作对照，联调时长不可预测；「无未关闭 P0/P1」是硬门槛；Skia 备选（+4–8 周）若触发，32–34 周窗口是**唯一吸收点**。压缩 M5 等于取消全计划唯一的缓冲。 |

**结论**：唯一真实的压缩来自**已交付事实**（M1 内的任务 1–5），而非重新排期。建议：把 M1/M2 回收的 3–5 周**转作 M3–M5 的风险缓冲**，而不是前移承诺日期。另有一处计划级错误需先修：**任务 5 的依赖边「1、3、6」写反**（见 N3），按现依赖图「分发器→控制器」无法解释 controller.h 已交付的事实。

---

## 4) 开放问题（恰好 3 个，供 Gemini / Claude / DeepSeek 下一轮裁决）

**Q1（测试证据门槛，裁决式）**：三份文档现已写入「本机曾用 g++ 复验通过 419/617/236」，但目录内没有任何测试源码、跑测命令或结果日志，且当前机器查无 g++。请裁决二选一：
- **A**：测试源码与跑测日志必须随交付物入库，入库前「已实现」标注一律降级为「已交付初版、待复验」；
- **B**：维持现状，以实现方 Linux 结果作为唯一证据，并**删除**全部「本机复验」字样。
无论选哪个，下一轮起 usage.md / api.md / plan.md 三处口径必须统一（当前 plan.md §8 仍是事实口吻）。

**Q2（M1 拆分与并行开工）**：任务 1–5 已交付初版后，M1 剩余实质只有「分发器 + 合成器参数真机验证」。请裁决：是否把 M1 拆为「M1a 纯逻辑（判定：已过）」与「M1b 合成器真机对应」，并让任务 6 的降级规则与 M0 **并行**开工、ETW/C# 投影样板提前，从而压缩 M2 1–2 周？还是坚持 Claude 的串行结构（M0→分发器→Rive→指针→试点），把压缩收益全部转作缓冲、不动承诺日期？须明确：并行开工会不会让分发器在 M0 结论未定时返工（例如 ALLOW_TEARING 不可用时整套 lowering 策略作废）？

**Q3（演示准入标准）**：WinUI3 小演示的第一阶段若只有**纯软件路径**（UI 线程 Tick + Apply 写 `UIElement.Scale/Translation/Opacity`），算不算「可演示」？请裁决：
- **A**：软件路径即可，下放试点降级为「阶段 3 可选」；
- **B**：必须含至少一条合成器下放才可对外演示。
若选 B，请同时指定：第一个下放试点动画（button.press 的 springTo vs card.enter 的 opacity bezier），并分工给出 `Period/DampingRatio ↔ response/damping` 的换算公式初版——Gemini 给出 WinUI 文档依据（`SpringVector3NaturalMotionAnimation`/`SpringScalarNaturalMotionAnimation` 参数语义）、Claude 给推导、DeepSeek 复核——该公式同时就是 M1b 的验证脚本输入。
