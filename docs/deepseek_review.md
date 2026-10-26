# DeepSeek Harness review — FluidMotion docs (Gemini-refreshed)

- **timestamp (Asia/Taipei):** 2026-10-19 10:10:49 Asia/Taipei (UTC+8)
- **note:** 基于 Gemini 刷新后版本的 `api.md` / `usage.md`（目录 `C:\Users\admin\Temp\motion_framework_review\`，含头文件、disputes.md、plan.md、motion.json）
- **source:** DeepSeek Harness local web (HTTP RPC + WebSocket session/follow)
- **harness URL (auth token entry):** `http://127.0.0.1:27921/?token=84QouN-mJd_-dYdtJudJY646QbGFUozsAwrFHXwznCo`
- **RPC base actually used:** `http://127.0.0.1:54300/` (cookie authority / same process as :27921)
- **session id:** `session-b3192c18-c79f-440f-b0d7-50297c29f4e7`
- **model:** deepseek-account / deepseek-v4-pro / reasoningEffort=max
- **prompt accepted:** yes
- **turn status:** completed (turn/end; replyChars=21469)

## User prompt

【说明】请基于目录 C:\Users\admin\Temp\motion_framework_review\ 中「Gemini 刚刷新后」的最新 api.md / usage.md 进行审阅（不要使用旧版文档印象）。同目录还有头文件 controller.h、tokens.h、timeline.h、spring.h、bezier.h，以及 disputes.md、plan.md、motion.json。请先读取这些文件再回答。用中文，尽量具体到章节号与符号名。

请按以下结构输出：

1) 与头文件不符的签名/行为（务必核对：TokenSet::Parse 返回 optional；MotionSystem::Create 需要 TargetId；Player::Retarget 应用层已弃用倾向；AnimationHandle 无公开 SnapToEnd；命名空间是 motion 不是 FluidMotion）
2) 文档遗漏的公开类型/方法
3) usage.md 示例是否逻辑上可编译（指出错误行与改法；注意 C# 伪代码里 Create 是否漏了 TargetId；检查清单写 MotionSystem.Retarget 是否应改为 AnimationHandle::Retarget）
4) 已知约束是否写清：HWND→V1.5、Rive 延迟、帧预算流水线、Linux-only 测试
5) 与 disputes.md / plan.md / motion.json 的不一致处（范围、里程碑、命名、API 决策等）
6) 明确修改建议（可附 api.md / usage.md 的可粘贴修订片段或补丁段落）

覆盖重点：遗漏公开接口、错误示例/签名、与 disputes/plan 不一致处。

## Assistant reply (verbatim)

The subagent isn't tracked as a waitable peer. Its report will arrive asynchronously. I'll proceed to write the final answer based on my own thorough verification (I've read every file fully and cross-checked line by line). If the subagent later reports discrepancies, I can adjust in a follow-up turn.

Let me now write the comprehensive Chinese review following the user's requested structure 1-6.

Key line-number citations I've verified:
- api.md:310 TokenSet::Parse optional ✓ / tokens.h:282, 723-732 (errors.clear() at 724).
- api.md:470-477 Create overloads / controller.h:293-302.
- api.md:279-286 Player::Retarget + 状态说明 / timeline.h:415-426 (no [[deprecated]]).
- api.md:455 无 SnapToEnd / controller.h:228-271 (AnimationHandle), 666 (detail::Controller::SnapToEnd), 284-290 (SetReducedMotion).
- api.md:4, 534 namespace motion.
- usage.md:259 C# Create has targetId; usage.md:256 Create(dispatcher) no clock.
- usage.md:277 bare Retarget; no "MotionSystem.Retarget" anywhere (grep confirmed).
- plan.md:67 vs plan.md:68 contradiction; plan.md:67 duplicate "5）".
- plan.md §4 rows 3/4/5 lack 已交付 vs §8 claims delivered.
- usage.md:4 "已验证" vs usage.md:286 "估计" and api.md:15, 548-555 hedged.
- api.md/usage.md missing HWND→V1.5 (disputes.md:10), 帧预算 (disputes.md:13-19, plan.md:9), Rive 延迟 (disputes.md:23-28). Grep confirmed api.md only has "Rive → CompositionSurface | 规划中" (line 530).
- motion.json consistent with schema (api.md §4.3).
- usage.md §1.1-§3 snippets compile-verified.
- Broken cross-refs: usage.md:8 engineering.md, usage.md:293 engineering.md/../plan.md/../summary.md, api.md:286 docs/engineering.md, api.md:3 code/, api.md:358 code/motion.json — flattened dir.

Structure the answer:

# 总体结论
Then sections 1-6.

Section 1: 与头文件不符的签名/行为 — 5 个重点核对（全部已修复/一致），然后 3 个次要精度问题。给出逐项结论表格。

Section 2: 遗漏公开接口 — 结论：api.md 覆盖完整；列出准遗漏 4 项（次要）。

Section 3: usage.md 可编译性 — 结论可编译；核对表；C# 两行；清单两条。

Section 4: 已知约束 — 4 项逐项打勾/缺失，给出缺失处。

Section 5: 与 disputes/plan/motion.json 不一致 — 列出 6 项。

Section 6: 修改建议 — 6 个可粘贴 patch + 说明。

Write it concisely but concretely. Include the patches in code blocks. Use section numbers and symbol names. All in Chinese.
子代理的复核结果尚未回传（其报告会异步送达，届时我再核对），但以下结论均来自我对 10 个文件逐行比对，并已用 grep 复核关键行号。以下是完整审阅。

---

# 审阅结论：「Gemini 刚刷新后」api.md / usage.md（2026-10-19 09:57）

**总体评价**：两个文档相对头文件**已基本准确**，用户点名的五个疑点（Parse 返回 optional、Create 需要 TargetId、Player::Retarget 弃用倾向、无公开 SnapToEnd、命名空间 motion）**均已修复并写对**。剩余问题集中在：①测试状态的「已验证」口径与 plan.md 矛盾；②disputes.md 的三条裁决（HWND→V1.5、帧预算流水线、Rive 延迟不承诺帧数）未写入两个文档；③少量措辞精度与引用路径问题。usage.md 全部 C++ 示例可编译。

---

## 1) 与头文件不符的签名/行为

先给用户点名的五项核对结论（全部通过）：

| # | 疑点 | 核对结果 | 证据 |
|---|------|----------|------|
| 1 | `TokenSet::Parse` 返回 optional | ✅ 已写对 | api.md §4.2（第 310 行）`static std::optional<TokenSet> Parse(std::string_view, std::vector<Diagnostic>&)`，与 tokens.h:282 一致；「先 clear errors」也与 tokens.h:724 `errors.clear()` 一致 |
| 2 | `MotionSystem::Create` 需要 TargetId | ✅ 已写对 | api.md §5.4（第 470–477 行）两个重载首参均为 `TargetId target`，与 controller.h:293–302 一致 |
| 3 | `Player::Retarget` 应用层已弃用倾向 | ✅ 已写对 | api.md §3.6 第 277–286 行标「【底层 / 应用层弃用倾向】」并明确「头文件**未**写 `[[deprecated]]`」，与 timeline.h:415–426 完全一致（无弃用属性）；签名（第 279–280 行）逐参数吻合 |
| 4 | `AnimationHandle` 无公开 SnapToEnd | ✅ 已写对 | api.md §5.3 第 455 行、§6 第 531 行、§7 第 543 行三处声明；controller.h:228–271 的 `AnimationHandle` 确实无此方法（`detail::Controller::SnapToEnd` 在 controller.h:666，仅由 `SetReducedMotion` controller.h:284–290 调用） |
| 5 | 命名空间是 motion 不是 FluidMotion | ✅ 已写对 | api.md 第 4 行「命名空间：全部类型与函数位于 `motion`」、§6 第 534 行「namespace `FluidMotion` \| 代码为 `motion`」；五个头文件均为 `namespace motion` |

**仍未修正的次要精度问题（非签名错误）：**

- **§1.1（api.md 第 35–36 行）**：`SpringParams` 字段注释写「构造时钳制为 >= 1e-4 / >= 0」，实际钳制发生在 `Spring` 的**构造函数**（spring.h:22–23），`SpringParams` 本身不钳制。建议把「构造时」改为「由 `Spring` 构造时」。
- **§5.4（api.md 第 468 行）**：「播放中的 Seek 到终点并 Completed」→ 代码中 in-flight 包括 **Playing 与 Paused**（controller.h:196 `InFlight()`），且反向播放时 Seek 到 0（controller.h:673）。建议改「in-flight（Playing/Paused）」。
- **§5.7（api.md 第 516 行）**：「零时长时间线在 `Play` 时立即完成」与头文件注释（controller.h:36）一致，但代码上 `AfterStart()` 是 `Play`/`Reverse` 共用（controller.h:500–514），`Reverse` 零时长线同样立即 Completed。可加半句说明。

## 2) 文档遗漏的公开类型/方法

**结论：api.md §1–§5 对五个头文件的公开面覆盖完整，无实质性遗漏**（已逐类清点：spring.h 3 个类型全列；bezier.h 的 `CubicBezier`/`EasingToken`/`kEasingTokens`/`FindEasing` 全列；timeline.h 的时钟、`Curve`、`Track`、节点、`Timeline`、`Player` 全列；tokens.h 的 `Diagnostic`/`TokenSet` 全列；controller.h 的 `TargetId`/枚举/`IDispatcher`/`MakeInstantTimeline`/`AnimationHandle`/`MotionSystem` 全列，`detail::Controller` 按内部实现处理）。

可指出的「准遗漏」（均为次要，建议顺手补一句）：

1. **`ManualClock::Now()` / `FixedStepClock::Now()`**：api.md §3.1 只写测试辅助方法，未注明 `Now()` 继承自 `IClock`（timeline.h:47、58）。
2. **`GroupNode` 构造器与 `TrackNode`/`DelayNode`/`MarkerNode` 构造器**：api.md §3.4 只列工厂函数与类型名，未列原始构造器（timeline.h:209–248）。工厂已覆盖用法，不算错误。
3. **`FixedStepClock::Frame()`/`Step()` 返回类型**：api.md §3.1 未标注（`std::int64_t` / `double`，timeline.h:60–61）。
4. usage.md 作为速查未列 `Player::Rate()`、`Timeline::Root()`、`Track::Segments()`、`Curve::FromSpring()`、`Spring::SettleTimeScan()` 等——速查不算遗漏，但建议在 §2 加一行「完整清单见 api.md」。

## 3) usage.md 示例是否逻辑上可编译

**结论：§1–§3 的全部 C++ 片段均可编译、签名全部对上**（重点核验：§1.1 `SoftDispatcher` 三个 override 签名与 IDispatcher 一致；§1.3 `Track("opacity",0.0).To(...)` 返回 `Track&` 作为 `Animate` 按值形参成立；`SpringParams{0.3,0.8}` 聚合初始化成立；`sys.Create(kCard)` 走默认实参 `full=nullptr, reduced=nullptr` 成立；§3.2 `h.Retarget("x", 1, {})` 的 `{}` 可构造 `SpringParams` 成立；§1.7 `Retarget(..., 0.0)` 的 `0.0` 隐式转 `std::optional<double>` 成立）。

针对用户两个具体检查点：

- **C# 伪代码 `Create` 是否漏了 TargetId**：**没漏**。usage.md 第 259 行 `AnimationHandle h = system.Create(targetId, tokens, "card.enter");` 已带 `targetId`，与 C++ `Create(TargetId, const TokenSet&, string_view)` 对应。✅
- **C# 第 256 行（新发现的小问题）**：`var system = MotionSystem.Create(dispatcher);` 与 C++ 构造器 `MotionSystem(const IClock&, IDispatcher* = nullptr)` 相比**缺时钟参数**。规划投影若自带内部时钟则应在注释中说明，否则应写 `Create(clock, dispatcher)`。
- **清单写的是不是 `MotionSystem.Retarget`**：全文 grep 确认**没有**「MotionSystem.Retarget」字样（`MotionSystem` 类也无此方法，controller.h:275–381）。第 277 行第 6 条写的是裸 `Retarget`——由于 `Player::Retarget` 与 `AnimationHandle::Retarget` 并存（api.md §7.1），建议改为显式 `AnimationHandle::Retarget`。另第 275 行第 4 条 `MotionSystem::Create` 建议写成 `MotionSystem::Create(TargetId, tokens, transition)`。

## 4) 已知约束是否写清（逐项）

| 约束 | api.md | usage.md | 结论 |
|------|--------|----------|------|
| HWND → V1.5（disputes.md §1 裁决） | ❌ 无任何提及（grep 0 命中） | ❌ 无 | **缺失**，应补 |
| Rive 指针延迟（disputes.md §3：不承诺帧数、实测为准） | ❌ 仅 §6「Rive → CompositionSurface \| 规划中」 | ⚠️ 仅第 266 行「指针延迟等见 engineering.md」 | **未写清**（engineering.md 本目录中不存在，无法核验） |
| 帧预算流水线（disputes.md §2：12%/35%/35%、每阶段单独红线、8.3ms 不可加总） | ❌ 无 | ⚠️ 仅「闲置 0% CPU」一行 | **缺失** |
| Linux-only 测试 | ⚠️ §8 有「估计，本机 Linux」 | ⚠️ §6 有，但第 4 行写「**已验证**：Linux 单元测试通过」无估计限定 | **口径矛盾**，且未提 plan.md「本机无编译器」 |

## 5) 与 disputes.md / plan.md / motion.json 的不一致处

1. **测试状态矛盾（最重要）**：plan.md 第 67 行称 timeline/tokens/controller「本机 419 / 617 / 236 项全过」，而第 68 行又称「**这台电脑上没有 C++ 编译器，代码尚未编译或测试**」——plan.md 自身两句话冲突。usage.md 第 4 行「已验证：Linux 单元测试通过」采信了前者且无「估计」限定，与 usage.md §6（第 286 行「估计以本地最近一次运行为准」）和 api.md §8（第 548 行「估计，以本机跑测为准」）口径不一致。**api.md 的表述是正确的**；usage.md 第 4 行需要改。
2. **HWND 裁决未反映**：disputes.md 第 10 行最终裁决「首发仅不受支持的参考宿主示例，`EmbedToHwnd` 正式 API 推迟 V1.5」、plan.md 第 8/40/51 行（M-14 参考示例 1–2 人周、M4 冒烟测试）均未进入 api.md §6 或 usage.md §6。
3. **帧预算未反映**：disputes.md 第 13–16 行（流水线并行建模、每阶段单独 ≤ 刷新间隔、8.3ms 是红线不是预算）与 plan.md 第 9 行（12%/35%/35% P95、P99 丢帧率）在两文档中无对应条目。
4. **Rive 延迟结论未反映**：disputes.md 第 28 行「不承诺任何具体帧数，实测为交付指标」未写入；usage.md 第 266 行只给了「见 engineering.md」的空引用（该文件不在本目录）。
5. **plan.md 内部瑕疵（顺带指出）**：§8 第 67 行出现两个「5）」编号；§4 任务表第 3/4/5 行未标「已交付初版」，与 §8 的「已交付」矛盾。
6. **motion.json 与 schema 一致**：✅ 无矛盾——api.md §4.3 的严格校验规则与 motion.json 全部吻合（四条 transition 均有 `reduced`；`default→normal`、`enter/exit→decelerate/accelerate`、`press→snappy` 别名链合法；`0ms`/`100ms` 等在 `[0,600]s` 内；`button.press` 的 `inheritVelocity` 合法）。
7. **引用路径（扁平目录问题）**：api.md 第 3 行「`code/` 下」、第 358 行「`code/motion.json`」、usage.md 第 3 行「`code/*.h`」、第 8/293 行「`engineering.md`」「`../plan.md`」「`../summary.md`」、api.md 第 286 行「`docs/engineering.md`」按仓库 docs/ 布局写；在本扁平目录中头文件与 motion.json 在根、engineering.md/summary.md 不存在、`../plan.md` 指向目录外。若该目录即交付物，路径全部断裂，且两文档引 engineering.md 佐证的「Player::Retarget 弃用」「指针延迟」无法核验。

## 6) 明确修改建议（可粘贴修订片段）

**P1. usage.md 第 3–6 行**（修复测试口径）：

```markdown
> **前提**：核心在 `code/*.h`（header-only C++17，命名空间 `motion`）。
> **测试状态**：timeline 419 / tokens 617 / controller 236 项在实现方 Linux 环境通过（**估计**）；plan.md §8 注明本机尚无 C++ 编译器、尚未本地编译验证，以本机复测为准。
> **未接线**：Windows 合成器 / WinUI / C# 绑定 — **规划中**；下文 C# 仅为「规划伪代码」。
> 估计与待真机项单独标明。
```

**P2. usage.md §5 清单第 4、6 条**（第 275、277 行）：

```markdown
4. `MotionSystem::Create(TargetId, tokens, transition)` → 设回调 → `Play`。
...
6. 交互打断用 `AnimationHandle::Retarget`（勿直接依赖 `Player::Retarget`，见 api.md §3.6 / §7.1）；取消用 `AnimationHandle::Cancel`。
```

**P3. usage.md §6 限制一览**（补齐三项缺失约束）：

```markdown
| 项 | 状态 |
|----|------|
| Linux 单测（core） | 实现方环境 419/617/236 项通过（估计）；**本机无编译器，尚未本地复验**（plan.md §8） |
| Windows Composition 下放 | **未接线，待真机验证** |
| C# / WinMD | **规划中 / 未实现** |
| HWND / WPF 宿主 | 首发仅**不受支持的参考示例**；`EmbedToHwnd` 正式 API 推迟 **V1.5**（disputes.md §1） |
| Rive 表面 | **规划中**；指针跟随延迟无法普遍消除：写为设计约束 + 分级缓解，**不承诺帧数**，实测为交付指标（disputes.md §3） |
| 帧预算（流水线） | 按刷新间隔 T 比例：UI 线程约 12%、Rive 渲染约 35%、GPU 约 35%，**每阶段单独** ≤1/T；8.3ms 是 120Hz 每阶段红线，不是可加总预算（disputes.md §2、plan.md §1） |
| 公开 `SnapToEnd` 句柄 API | **无**；用 `SetReducedMotion` |
| 闲置 0% CPU（合成器路径） | **估计目标**，真机未测（目标：0 计时器、CPU/GPU ≈0，Gemini 口径 ≤0.1%，plan.md §1） |
```

**P4. usage.md §4 C# 伪代码（第 256–259 行）**，补时钟与 TargetId 注释：

```csharp
// var system = MotionSystem.Create(dispatcher);   // 规划：C++ 侧为 MotionSystem(IClock&, IDispatcher*)，
//                                                 // 投影需自带/注入时钟，此处仅示意，勿当作最终签名
// var tokens = TokenSet.LoadFromFile("motion.json");  // 规划：对应 C++ TokenSet::Parse(json, errors)
// AnimationHandle h = system.Create(targetId, tokens, "card.enter");  // TargetId 必传（与 C++ 一致）
```

**P5. api.md §6 表格**（第 526–534 行区域）追加一行：

```markdown
| `EmbedToHwnd` / HWND 宿主正式 API | **V1.5**；首发仅随附不受支持的参考示例（disputes.md §1） |
```

**P6. api.md §8 末尾**（第 556 行后）追加：

```markdown
> 用例数来自实现方（Claude）的 Linux 环境；plan.md §8 同时注明本机没有 C++ 编译器、代码尚未在本机编译或测试。上表为**估计**，以本机跑测为准。
```

**P7. api.md §1.1（第 35–36 行）措辞**：

```cpp
struct SpringParams {
    double response = 0.35;  // 无阻尼周期（秒）；由 Spring 构造时钳制为 >= 1e-4
    double damping  = 0.8;   // 阻尼比；由 Spring 构造时钳制为 >= 0（负值会发散）
};
```

**P8.（建议）**若本目录即交付形态，请把 api.md/usage.md 中的 `code/*.h`、`code/motion.json`、`docs/engineering.md`、`../plan.md`、`../summary.md` 改为同目录相对引用，或随附 engineering.md/summary.md；否则其中被引证的两处结论（`Player::Retarget` 弃用依据、指针延迟说明）处于无源状态。

---

补充说明：我另启动了一个独立复核子代理逐条对抗性核验上述 13 项结论（含一次「五头文件公开面是否有 api.md 未覆盖项」的独立扫描）；其报告送达后如有出入我会立刻更正。
The subagent isn't tracked as a waitable peer. Its report will arrive asynchronously. I'll proceed to write the final answer based on my own thorough verification (I've read every file fully and cross-checked line by line). If the subagent later reports discrepancies, I can adjust in a follow-up turn.

Let me now write the comprehensive Chinese review following the user's requested structure 1-6.

Key line-number citations I've verified:
- api.md:310 TokenSet::Parse optional ✓ / tokens.h:282, 723-732 (errors.clear() at 724).
- api.md:470-477 Create overloads / controller.h:293-302.
- api.md:279-286 Player::Retarget + 状态说明 / timeline.h:415-426 (no [[deprecated]]).
- api.md:455 无 SnapToEnd / controller.h:228-271 (AnimationHandle), 666 (detail::Controller::SnapToEnd), 284-290 (SetReducedMotion).
- api.md:4, 534 namespace motion.
- usage.md:259 C# Create has targetId; usage.md:256 Create(dispatcher) no clock.
- usage.md:277 bare Retarget; no "MotionSystem.Retarget" anywhere (grep confirmed).
- plan.md:67 vs plan.md:68 contradiction; plan.md:67 duplicate "5）".
- plan.md §4 rows 3/4/5 lack 已交付 vs §8 claims delivered.
- usage.md:4 "已验证" vs usage.md:286 "估计" and api.md:15, 548-555 hedged.
- api.md/usage.md missing HWND→V1.5 (disputes.md:10), 帧预算 (disputes.md:13-19, plan.md:9), Rive 延迟 (disputes.md:23-28). Grep confirmed api.md only has "Rive → CompositionSurface | 规划中" (line 530).
- motion.json consistent with schema (api.md §4.3).
- usage.md §1.1-§3 snippets compile-verified.
- Broken cross-refs: usage.md:8 engineering.md, usage.md:293 engineering.md/../plan.md/../summary.md, api.md:286 docs/engineering.md, api.md:3 code/, api.md:358 code/motion.json — flattened dir.

Structure the answer:

# 总体结论
Then sections 1-6.

Section 1: 与头文件不符的签名/行为 — 5 个重点核对（全部已修复/一致），然后 3 个次要精度问题。给出逐项结论表格。

Section 2: 遗漏公开接口 — 结论：api.md 覆盖完整；列出准遗漏 4 项（次要）。

Section 3: usage.md 可编译性 — 结论可编译；核对表；C# 两行；清单两条。

Section 4: 已知约束 — 4 项逐项打勾/缺失，给出缺失处。

Section 5: 与 disputes/plan/motion.json 不一致 — 列出 6 项。

Section 6: 修改建议 — 6 个可粘贴 patch + 说明。

Write it concisely but concretely. Include the patches in code blocks. Use section numbers and symbol names. All in Chinese.
子代理的复核结果尚未回传（其报告会异步送达，届时我再核对），但以下结论均来自我对 10 个文件逐行比对，并已用 grep 复核关键行号。以下是完整审阅。

---

# 审阅结论：「Gemini 刚刷新后」api.md / usage.md（2026-10-19 09:57）

**总体评价**：两个文档相对头文件**已基本准确**，用户点名的五个疑点（Parse 返回 optional、Create 需要 TargetId、Player::Retarget 弃用倾向、无公开 SnapToEnd、命名空间 motion）**均已修复并写对**。剩余问题集中在：①测试状态的「已验证」口径与 plan.md 矛盾；②disputes.md 的三条裁决（HWND→V1.5、帧预算流水线、Rive 延迟不承诺帧数）未写入两个文档；③少量措辞精度与引用路径问题。usage.md 全部 C++ 示例可编译。

---

## 1) 与头文件不符的签名/行为

先给用户点名的五项核对结论（全部通过）：

| # | 疑点 | 核对结果 | 证据 |
|---|------|----------|------|
| 1 | `TokenSet::Parse` 返回 optional | ✅ 已写对 | api.md §4.2（第 310 行）`static std::optional<TokenSet> Parse(std::string_view, std::vector<Diagnostic>&)`，与 tokens.h:282 一致；「先 clear errors」也与 tokens.h:724 `errors.clear()` 一致 |
| 2 | `MotionSystem::Create` 需要 TargetId | ✅ 已写对 | api.md §5.4（第 470–477 行）两个重载首参均为 `TargetId target`，与 controller.h:293–302 一致 |
| 3 | `Player::Retarget` 应用层已弃用倾向 | ✅ 已写对 | api.md §3.6 第 277–286 行标「【底层 / 应用层弃用倾向】」并明确「头文件**未**写 `[[deprecated]]`」，与 timeline.h:415–426 完全一致（无弃用属性）；签名（第 279–280 行）逐参数吻合 |
| 4 | `AnimationHandle` 无公开 SnapToEnd | ✅ 已写对 | api.md §5.3 第 455 行、§6 第 531 行、§7 第 543 行三处声明；controller.h:228–271 的 `AnimationHandle` 确实无此方法（`detail::Controller::SnapToEnd` 在 controller.h:666，仅由 `SetReducedMotion` controller.h:284–290 调用） |
| 5 | 命名空间是 motion 不是 FluidMotion | ✅ 已写对 | api.md 第 4 行「命名空间：全部类型与函数位于 `motion`」、§6 第 534 行「namespace `FluidMotion` \| 代码为 `motion`」；五个头文件均为 `namespace motion` |

**仍未修正的次要精度问题（非签名错误）：**

- **§1.1（api.md 第 35–36 行）**：`SpringParams` 字段注释写「构造时钳制为 >= 1e-4 / >= 0」，实际钳制发生在 `Spring` 的**构造函数**（spring.h:22–23），`SpringParams` 本身不钳制。建议把「构造时」改为「由 `Spring` 构造时」。
- **§5.4（api.md 第 468 行）**：「播放中的 Seek 到终点并 Completed」→ 代码中 in-flight 包括 **Playing 与 Paused**（controller.h:196 `InFlight()`），且反向播放时 Seek 到 0（controller.h:673）。建议改「in-flight（Playing/Paused）」。
- **§5.7（api.md 第 516 行）**：「零时长时间线在 `Play` 时立即完成」与头文件注释（controller.h:36）一致，但代码上 `AfterStart()` 是 `Play`/`Reverse` 共用（controller.h:500–514），`Reverse` 零时长线同样立即 Completed。可加半句说明。

## 2) 文档遗漏的公开类型/方法

**结论：api.md §1–§5 对五个头文件的公开面覆盖完整，无实质性遗漏**（已逐类清点：spring.h 3 个类型全列；bezier.h 的 `CubicBezier`/`EasingToken`/`kEasingTokens`/`FindEasing` 全列；timeline.h 的时钟、`Curve`、`Track`、节点、`Timeline`、`Player` 全列；tokens.h 的 `Diagnostic`/`TokenSet` 全列；controller.h 的 `TargetId`/枚举/`IDispatcher`/`MakeInstantTimeline`/`AnimationHandle`/`MotionSystem` 全列，`detail::Controller` 按内部实现处理）。

可指出的「准遗漏」（均为次要，建议顺手补一句）：

1. **`ManualClock::Now()` / `FixedStepClock::Now()`**：api.md §3.1 只写测试辅助方法，未注明 `Now()` 继承自 `IClock`（timeline.h:47、58）。
2. **`GroupNode` 构造器与 `TrackNode`/`DelayNode`/`MarkerNode` 构造器**：api.md §3.4 只列工厂函数与类型名，未列原始构造器（timeline.h:209–248）。工厂已覆盖用法，不算错误。
3. **`FixedStepClock::Frame()`/`Step()` 返回类型**：api.md §3.1 未标注（`std::int64_t` / `double`，timeline.h:60–61）。
4. usage.md 作为速查未列 `Player::Rate()`、`Timeline::Root()`、`Track::Segments()`、`Curve::FromSpring()`、`Spring::SettleTimeScan()` 等——速查不算遗漏，但建议在 §2 加一行「完整清单见 api.md」。

## 3) usage.md 示例是否逻辑上可编译

**结论：§1–§3 的全部 C++ 片段均可编译、签名全部对上**（重点核验：§1.1 `SoftDispatcher` 三个 override 签名与 IDispatcher 一致；§1.3 `Track("opacity",0.0).To(...)` 返回 `Track&` 作为 `Animate` 按值形参成立；`SpringParams{0.3,0.8}` 聚合初始化成立；`sys.Create(kCard)` 走默认实参 `full=nullptr, reduced=nullptr` 成立；§3.2 `h.Retarget("x", 1, {})` 的 `{}` 可构造 `SpringParams` 成立；§1.7 `Retarget(..., 0.0)` 的 `0.0` 隐式转 `std::optional<double>` 成立）。

针对用户两个具体检查点：

- **C# 伪代码 `Create` 是否漏了 TargetId**：**没漏**。usage.md 第 259 行 `AnimationHandle h = system.Create(targetId, tokens, "card.enter");` 已带 `targetId`，与 C++ `Create(TargetId, const TokenSet&, string_view)` 对应。✅
- **C# 第 256 行（新发现的小问题）**：`var system = MotionSystem.Create(dispatcher);` 与 C++ 构造器 `MotionSystem(const IClock&, IDispatcher* = nullptr)` 相比**缺时钟参数**。规划投影若自带内部时钟则应在注释中说明，否则应写 `Create(clock, dispatcher)`。
- **清单写的是不是 `MotionSystem.Retarget`**：全文 grep 确认**没有**「MotionSystem.Retarget」字样（`MotionSystem` 类也无此方法，controller.h:275–381）。第 277 行第 6 条写的是裸 `Retarget`——由于 `Player::Retarget` 与 `AnimationHandle::Retarget` 并存（api.md §7.1），建议改为显式 `AnimationHandle::Retarget`。另第 275 行第 4 条 `MotionSystem::Create` 建议写成 `MotionSystem::Create(TargetId, tokens, transition)`。

## 4) 已知约束是否写清（逐项）

| 约束 | api.md | usage.md | 结论 |
|------|--------|----------|------|
| HWND → V1.5（disputes.md §1 裁决） | ❌ 无任何提及（grep 0 命中） | ❌ 无 | **缺失**，应补 |
| Rive 指针延迟（disputes.md §3：不承诺帧数、实测为准） | ❌ 仅 §6「Rive → CompositionSurface \| 规划中」 | ⚠️ 仅第 266 行「指针延迟等见 engineering.md」 | **未写清**（engineering.md 本目录中不存在，无法核验） |
| 帧预算流水线（disputes.md §2：12%/35%/35%、每阶段单独红线、8.3ms 不可加总） | ❌ 无 | ⚠️ 仅「闲置 0% CPU」一行 | **缺失** |
| Linux-only 测试 | ⚠️ §8 有「估计，本机 Linux」 | ⚠️ §6 有，但第 4 行写「**已验证**：Linux 单元测试通过」无估计限定 | **口径矛盾**，且未提 plan.md「本机无编译器」 |

## 5) 与 disputes.md / plan.md / motion.json 的不一致处

1. **测试状态矛盾（最重要）**：plan.md 第 67 行称 timeline/tokens/controller「本机 419 / 617 / 236 项全过」，而第 68 行又称「**这台电脑上没有 C++ 编译器，代码尚未编译或测试**」——plan.md 自身两句话冲突。usage.md 第 4 行「已验证：Linux 单元测试通过」采信了前者且无「估计」限定，与 usage.md §6（第 286 行「估计以本地最近一次运行为准」）和 api.md §8（第 548 行「估计，以本机跑测为准」）口径不一致。**api.md 的表述是正确的**；usage.md 第 4 行需要改。
2. **HWND 裁决未反映**：disputes.md 第 10 行最终裁决「首发仅不受支持的参考宿主示例，`EmbedToHwnd` 正式 API 推迟 V1.5」、plan.md 第 8/40/51 行（M-14 参考示例 1–2 人周、M4 冒烟测试）均未进入 api.md §6 或 usage.md §6。
3. **帧预算未反映**：disputes.md 第 13–16 行（流水线并行建模、每阶段单独 ≤ 刷新间隔、8.3ms 是红线不是预算）与 plan.md 第 9 行（12%/35%/35% P95、P99 丢帧率）在两文档中无对应条目。
4. **Rive 延迟结论未反映**：disputes.md 第 28 行「不承诺任何具体帧数，实测为交付指标」未写入；usage.md 第 266 行只给了「见 engineering.md」的空引用（该文件不在本目录）。
5. **plan.md 内部瑕疵（顺带指出）**：§8 第 67 行出现两个「5）」编号；§4 任务表第 3/4/5 行未标「已交付初版」，与 §8 的「已交付」矛盾。
6. **motion.json 与 schema 一致**：✅ 无矛盾——api.md §4.3 的严格校验规则与 motion.json 全部吻合（四条 transition 均有 `reduced`；`default→normal`、`enter/exit→decelerate/accelerate`、`press→snappy` 别名链合法；`0ms`/`100ms` 等在 `[0,600]s` 内；`button.press` 的 `inheritVelocity` 合法）。
7. **引用路径（扁平目录问题）**：api.md 第 3 行「`code/` 下」、第 358 行「`code/motion.json`」、usage.md 第 3 行「`code/*.h`」、第 8/293 行「`engineering.md`」「`../plan.md`」「`../summary.md`」、api.md 第 286 行「`docs/engineering.md`」按仓库 docs/ 布局写；在本扁平目录中头文件与 motion.json 在根、engineering.md/summary.md 不存在、`../plan.md` 指向目录外。若该目录即交付物，路径全部断裂，且两文档引 engineering.md 佐证的「Player::Retarget 弃用」「指针延迟」无法核验。

## 6) 明确修改建议（可粘贴修订片段）

**P1. usage.md 第 3–6 行**（修复测试口径）：

```markdown
> **前提**：核心在 `code/*.h`（header-only C++17，命名空间 `motion`）。
> **测试状态**：timeline 419 / tokens 617 / controller 236 项在实现方 Linux 环境通过（**估计**）；plan.md §8 注明本机尚无 C++ 编译器、尚未本地编译验证，以本机复测为准。
> **未接线**：Windows 合成器 / WinUI / C# 绑定 — **规划中**；下文 C# 仅为「规划伪代码」。
> 估计与待真机项单独标明。
```

**P2. usage.md §5 清单第 4、6 条**（第 275、277 行）：

```markdown
4. `MotionSystem::Create(TargetId, tokens, transition)` → 设回调 → `Play`。
...
6. 交互打断用 `AnimationHandle::Retarget`（勿直接依赖 `Player::Retarget`，见 api.md §3.6 / §7.1）；取消用 `AnimationHandle::Cancel`。
```

**P3. usage.md §6 限制一览**（补齐三项缺失约束）：

```markdown
| 项 | 状态 |
|----|------|
| Linux 单测（core） | 实现方环境 419/617/236 项通过（估计）；**本机无编译器，尚未本地复验**（plan.md §8） |
| Windows Composition 下放 | **未接线，待真机验证** |
| C# / WinMD | **规划中 / 未实现** |
| HWND / WPF 宿主 | 首发仅**不受支持的参考示例**；`EmbedToHwnd` 正式 API 推迟 **V1.5**（disputes.md §1） |
| Rive 表面 | **规划中**；指针跟随延迟无法普遍消除：写为设计约束 + 分级缓解，**不承诺帧数**，实测为交付指标（disputes.md §3） |
| 帧预算（流水线） | 按刷新间隔 T 比例：UI 线程约 12%、Rive 渲染约 35%、GPU 约 35%，**每阶段单独** ≤1/T；8.3ms 是 120Hz 每阶段红线，不是可加总预算（disputes.md §2、plan.md §1） |
| 公开 `SnapToEnd` 句柄 API | **无**；用 `SetReducedMotion` |
| 闲置 0% CPU（合成器路径） | **估计目标**，真机未测（目标：0 计时器、CPU/GPU ≈0，Gemini 口径 ≤0.1%，plan.md §1） |
```

**P4. usage.md §4 C# 伪代码（第 256–259 行）**，补时钟与 TargetId 注释：

```csharp
// var system = MotionSystem.Create(dispatcher);   // 规划：C++ 侧为 MotionSystem(IClock&, IDispatcher*)，
//                                                 // 投影需自带/注入时钟，此处仅示意，勿当作最终签名
// var tokens = TokenSet.LoadFromFile("motion.json");  // 规划：对应 C++ TokenSet::Parse(json, errors)
// AnimationHandle h = system.Create(targetId, tokens, "card.enter");  // TargetId 必传（与 C++ 一致）
```

**P5. api.md §6 表格**（第 526–534 行区域）追加一行：

```markdown
| `EmbedToHwnd` / HWND 宿主正式 API | **V1.5**；首发仅随附不受支持的参考示例（disputes.md §1） |
```

**P6. api.md §8 末尾**（第 556 行后）追加：

```markdown
> 用例数来自实现方（Claude）的 Linux 环境；plan.md §8 同时注明本机没有 C++ 编译器、代码尚未在本机编译或测试。上表为**估计**，以本机跑测为准。
```

**P7. api.md §1.1（第 35–36 行）措辞**：

```cpp
struct SpringParams {
    double response = 0.35;  // 无阻尼周期（秒）；由 Spring 构造时钳制为 >= 1e-4
    double damping  = 0.8;   // 阻尼比；由 Spring 构造时钳制为 >= 0（负值会发散）
};
```

**P8.（建议）**若本目录即交付形态，请把 api.md/usage.md 中的 `code/*.h`、`code/motion.json`、`docs/engineering.md`、`../plan.md`、`../summary.md` 改为同目录相对引用，或随附 engineering.md/summary.md；否则其中被引证的两处结论（`Player::Retarget` 弃用依据、指针延迟说明）处于无源状态。

---

补充说明：我另启动了一个独立复核子代理逐条对抗性核验上述 13 项结论（含一次「五头文件公开面是否有 api.md 未覆盖项」的独立扫描）；其报告送达后如有出入我会立刻更正。
