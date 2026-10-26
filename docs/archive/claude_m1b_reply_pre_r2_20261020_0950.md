> **PARTIAL** — Reply 1 is complete (Continue clicked 4:30 AM Oct 20, finished ~4:35 AM, spec downloaded). Reply 2: the follow-up below was SENT once at ~4:37 AM, but Claude paused on the Free limit after only a preamble and two web searches. Clicking Continue at ~4:40 AM failed ("Failed to send" / Retry not clicked). Limit text: "Your free messages return at 9:30 AM." / "Limits will reset at 9:30 AM." Upgrade / See Pro plan were NOT clicked.

## Reply 1 — continued turn (after Continue, complete, copied verbatim via Copy button)

Research is done. Next I'll write the spec file.两件事都做完了。先说明依据：上传的 `spring_1.h`、`composition_1.h` 与我手里的版本完全一致，`fm_capi_1.h` 只差一行路径注释，所以下面的审查针对的就是你们正在跑的代码。Composition 的 API 事实我查了 Microsoft Learn，没有凭记忆。C# 侧我只读了代码，没有在 Windows 上编译或运行。

## 【1】C# 侧审查（按严重程度）

| # | 严重度 | 位置 | 问题 | 修法 |
|---|---|---|---|---|
| 1 | 高 | `FmBridge.cs`：`_sink`、`_render` 两个 lambda，以及 `_onRendering` | 托管异常传出原生栈会直接崩进程，不会被 `fm_on_frame` 里的 `catch(...)` 接住。sink 里写 `BoxTranslate.X` / `AnimBox.Opacity` 在元素拆除后可能抛 COMException，这一路没有任何保护 | 三处都包 `try { … } catch (Exception ex) { _lastError = ex; }`，不要让异常越过 P/Invoke 边界 |
| 2 | 中高 | `MainPage` 的 `Unloaded`（`OnUnloaded`） | WinUI 3 关闭窗口时不保证触发页面的 `Unloaded`。若没触发，静态事件 `CompositionTarget.Rendering` 仍持有 `_onRendering`，进而持有 bridge 和页面，原生系统不会销毁 | 在 `MainWindow` 里加 `Closed += (_, __) => page.Shutdown();`，`OnUnloaded` 和 `Closed` 都调同一个幂等的 `Shutdown()` |
| 3 | 中 | `MotionBridge.Dispose()` | 若 `Dispose` 发生在原生调用栈里（例如回调里触发页面卸载），`fm_destroy` 会销毁正在执行的 `fm_on_frame`，造成释放后使用 | 加计数 `_inNative`，每个 `Fm.*` 调用前后 ++/--；`Dispose` 时若 `_inNative>0` 就 `DispatcherQueue.TryEnqueue(Dispose)` 延后 |
| 4 | 中 | `MainPage` 构造函数：`Application.Current.UnhandledException += …` | 订阅静态事件的 lambda 捕获 `this`，从不退订。页面每次重建都会累加一个处理器并让旧页面永远不被回收 | 移到 `App` 里订阅一次，或在 `OnUnloaded` 里保存并退订 |
| 5 | 中 | `MainPage.OnLoaded` | `Loaded` 可能重复触发（页面重新进入树）。第二次会再 `new MotionBridge` 并覆盖 `_log`，旧句柄泄漏 | 开头 `if (_fm != null) return;` |
| 6 | 中 | `MotionBridge` 所有公有方法 | 引擎非线程安全，目前靠调用方自觉都在 UI 线程。`After` 定时器和指针事件当前都在 UI 线程，所以今天没问题，但没有保护 | 构造时记 `DispatcherQueue.GetForCurrentThread()`，每个公有方法首行 `Debug.Assert(_queue.HasThreadAccess)` |
| 7 | 低 | `MotionBridge.Dispose()` | `Frame`、`SubscriptionChanged` 事件在 `Dispose` 后仍引用页面 | `Dispose` 末尾置 `Frame = null; SubscriptionChanged = null;` |
| 8 | 低 | `fm_load_tokens` 的 `byte[] err` | blittable 数组会被固定，原生写入当前可见，但没有写 `[Out]`，依赖隐含行为 | 声明改为 `[Out] byte[] err` |
| 9 | 低 | `_sink` 里的 `Marshal.PtrToStringUTF8(p)!` | 每帧每属性分配一个字符串 | 先用字节比较（`"offsetX"u8`、`"opacity"u8`）分派，不分配 |
| 10 | 低 | `MainPage._buf` | 每帧日志追加到 `StringBuilder`，直到 `LogStats` / 卸载才写盘，长时间运行会无界增长（约 12 KB/s） | 超过 1 MB 就 `FlushLog()` |
| 11 | 低 | 构建 `fluidmotion.dll` | `cl` 默认 `/MD`，目标机需要 VC++ 运行库；打包部署时可能缺 | 演示 DLL 用 `/MT`（静态运行库）；同时把 `strncpy` 换成 `strncpy_s(err, cap, s, _TRUNCATE)` 以消除 C4996 |

**已核对、没有问题的几点：**
- **委托生命周期：** `_sink` 和 `_render` 作为字段持有，直到页面释放 bridge，不会被 GC 回收。
- **重入规则：** sink 与 render 回调里没有调用 `Fm.*`，符合“回调里不得调用 `fm_*`”。`Frame` 事件里的 `Running` 在 `fm_on_frame` 返回之后，合规。
- **回调线程：** 都在 UI 线程。
- **退订时机：** `fm_on_frame` 内部通过 render 回调退订 `Rendering` 是在事件处理中间退订。WinRT 事件按快照分发，应当安全，你们 3 次订阅 / 3 次退订的日志也证明了这一点。
- **销毁时的回调：** `fm_destroy` 期间原生侧不会再调用 sink 或 render。
- **自检定时器：** `After` 的一次性定时器在 `OnUnloaded` 里停掉并清空，`RetargetLogged` 也检查了 `_fm is null`，没有竞态。

**附注：32 fps 问题，不用动鼠标的判别探针。** 推测显示器休眠，但还没证实。用 `DwmFlush()`（user32 / dwmapi）连续阻塞调用 300 次，测相邻返回间隔：约 16.7 ms 说明 DWM 在 60 Hz 合成，约 31 ms 说明 DWM 本身只出 32 Hz。这个探针和 XAML 无关，能把“显示器休眠”与“Rendering 事件本身的问题”分开。32 ms 恰好是 2×15.625 ms 的系统时钟节拍，更像定时器驱动出帧，所以这项应放在 M1b 一切测量之前。

## 【2】`claude_m1b_spec.md`

规格共 138 行，中文，每条事实标 **[文档]**、**[推断]** 或 **[未验证]**。几个你们需要直接用的结论：

1. **Period 的含义文档没写。**
   - Microsoft 文档只写“弹簧完成一次振荡所需时间”，没有说是阻尼周期还是无阻尼周期，`InitialVelocity` 的单位也没写。
   - 文档推荐偏移用 ζ=0.8、Period=50 ms，示例代码却用 0.5 s 和 0.07 s，相差 10 倍。
   - 所以换算公式给了三个互斥假设：H1 `P = T₀`（当前代码）、H2 `P = T₀/√(1−ζ²)`、H3 `P = T₀/(2π)`。其中 H3 是我的推断：50 ms 对应 T₀≈314 ms，比 H1 的 20 Hz 更像常见响应时间，但不能当结论。
   - 判别靠两次捕获：ζ=0 时量振荡周期，ζ=0.5 时量波峰间隔，另附对数衰减校验 ζ。
   - `SpringMapping::ToCompositor` 只改这一处。

2. **打断不跳变，推荐“混合”方案。**
   - 不设 `InitialValue`，文档说缺省取当前值，所以位置对屏幕上的实际值天然连续；`InitialVelocity` 用引擎速度。
   - 前提是“缺省值取动画中的当前值，而非最后一次 UI 设置值”，需用 T-C1 测试确认。
   - 后备方案是引擎播种加前瞻：把旧弹簧外推约一帧后再设为初值，补偿提交延迟造成的约 `v·δ` 跳变。
   - 我没找到同步回读动画值的 API，假设不可用。

3. **试点默认映射要改。** `StaticCapabilityProbe` 把 `offsetX` 映射为 `"Offset.X"`，对 XAML 元素大概率被布局覆盖。试点应改用 `"Translation.X"`，并先调用 `ElementCompositionPreview.SetIsTranslationEnabled`，这个 API 在 WASDK 里有文档。

4. **规格里有一个前置代码任务。** `Capabilities` 需要加 `partialStart`、`playbackRate`、`reverse` 开关（目前不存在），试点后端全设 false，这样 SetRate、暂停后恢复等场景会回退到应用线程，而不是下发错误动画。

5. **接口增量。** 新增 C ABI：`fm_backend` 回调结构、`fm_caps`、`fm_create_composition`，全部 blittable，C# 侧实现四个回调。

6. **九条量化判据：** M-1 弹簧映射拟合、M-2 时长与收尾、M-3 keyframe 精度、M-4 打断连续性、M-5 前瞻补偿、M-6 UI 线程卡 200 ms 的卸载证明、M-7 释放与泄漏、M-8 reduced motion、M-9 初速度单位。阈值是提议值，M1b-0 之后可调。

7. **不宣称零 CPU。** 引擎仍要每帧 `Tick` 做完成记账；规格里写明了，并把 `NextWakeTime()` 列为后续项。

**我没能验证的：**
- `InitialValueExpression` 这个成员在我检索到的类成员列表里没看到，可能是文档示例写错或需要特定 WASDK 版本，需在 2.5.1 里编译确认。
- `CubicBezier` 的 y 控制点在 [0,1] 之外是否有效，文档沉默。
- `StopAnimation` 之后值留在哪里，文档未明确。
- 上述内容都在规格第 5 节的 M1b-0 里作为“只测不接引擎”的第一步，约 2–3 天。

## Reply 2 — follow-up (PARTIAL)

SENT (~4:37 AM Oct 20):

补充背景（workbuddy 试点实测，屏幕可能处于休眠）：ExpressionAnimation + CompositionPropertySet 的 TryGetScalar 只返回静态值（全是 0），关键帧动画约 14–30 ms 就报告完成。请在 spec 里补一节：可靠地读取飞行中数值 / 测量合成器路径的方法（如 Windows.Graphics.Capture 截帧、DComp 帧统计、animation capture 等），并解释这两个现象的可能原因。如已写出 spec 文件，请给出更新后的完整版。

Claude reply so far (UI status lines: "Compiling a full updated specification with new measurement sections", "Searched the web: CompositionPropertySet TryGetScalar animated value not updated expression an…", plus a second web search), then "Paused when your free usage ran out. Continue to pick up where Claude stopped.":

我先核对几个关键 API 的文档说法，再回答成因，因为成因判断依赖这些语义。
