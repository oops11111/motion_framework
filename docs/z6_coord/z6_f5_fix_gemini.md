# Z6 F5 修法细化：运行中跟随系统动画设置（Gemini）

- 日期：2026-11-20（Australia/Perth，UTC+8）
- 角色：Gemini 侧方案（只写方案，不改代码、不改仓库、不改系统设置、不 LOCK、不推送）
- 依据实验：`/workspace/review/z6/results/f5_REPORT.md` 第 11 节（续跑 2026-11-18 13:23–13:30 AWST，Windows 11 26100）
- 先前意见：`z6_handover_review_gemini.md` §2.10 / §3.2（可切换来源 + Fake；当时把 `WM_SETTINGCHANGE` 写成兜底，现改为真实来源的主路径）

## 结论（推荐选项）

1. **不能把「`UISettings.AnimationsEnabledChanged` 在 WinUI 3 / Win32 桌面进程里不触发」写成已有官方定论。** 官方「桌面应用不支持」事件表只列了 `UISettings.ColorValuesChanged` 和 `AccessibilitySettings.HighContrastChanged`，**没有**列 `AnimationsEnabledChanged`。因此「这是已知官方行为」标 **未证实**。
2. **本宿主上的现象是实验证实的，不是猜测。** F5 续跑已用 `SystemParametersInfo(SPI_SETCLIENTAREAANIMATION=0x1043, …, SPIF_UPDATEINIFILE|SPIF_SENDCHANGE)` 在 GUI 运行期间关→开，SPI 回读与 `UserPreferencesMask` 第 5 字节 `0x12↔0x10` 均变化，新进程读到 `UISettings.AnimationsEnabled=False/True`，但已运行进程 `F5 AnimationsEnabledChanged` 与 `source=event` 均为 0 行、`events=0`。失败点是**变更通知未送达订阅者**，不是切错设置、不是订阅没装上。
3. **推荐宿主侧真实来源改为「顶层 HWND 收 `WM_SETTINGCHANGE`（0x001A），去抖后重读 `UISettings.AnimationsEnabled`，再在 UI 线程调用 `MotionSystem.SetReducedMotion(!animationsEnabled)`」。** 这与官方辅助功能文档对「客户区动画」参数的建议一致：收到 `WM_SETTINGCHANGE` 后用 `SystemParametersInfo`（此处等价地重读已在用的 `UISettings.AnimationsEnabled`）确认当前值。`AnimationsEnabledChanged` 只作为尽力而为的次路径保留并打日志。默认自动验收继续走 **Fake 来源**，**不**让 workbuddy 自行切换系统动画设置。真实 SPI / 设置应用切换只能由 pidan 本人操作或凭拍板卡原话授权。
4. 硬约束不变：不改 L1 公开面、不改 `composition.h`、只改 FluentHub 侧叉；Success≠adopt；不合 main；不 force/amend；第三方仓只留本地。PERF-CI 仍为 HOLD，本票不计时、不用耗时做通过/失败标准。

---

## §1 这个事件在 WinUI 3 / Win32 桌面进程里不触发，是已知行为吗？

**短答：** 就 `AnimationsEnabledChanged` 这一条而言，**不是已有可公开核实的官方「已知不支持 / 已知不触发」陈述**，标 **未证实**。可核实的是下面三类事实：官方对相邻 API 的限制、官方对客户区动画应用该怎么听变化、以及 F5 在本宿主上的实验。

### 1.1 官方文档实际写了什么（已打开原文）

| 陈述 | 来源 | 对 F5 的含义 |
|---|---|---|
| `AnimationsEnabled`：只说明「获取用户界面是否启用动画」，不说明底层对应哪一个 SPI | [UISettings.AnimationsEnabled](https://learn.microsoft.com/en-us/uwp/api/windows.ui.viewmanagement.uisettings.animationsenabled?view=winrt-28000)（2026-11-20 取回） | 属性可读；**官方未写**它等于 `SPI_GETCLIENTAREAANIMATION`。映射见 1.3，实验已证实、官方映射仍标未证实 |
| `AnimationsEnabledChanged`：「在 `AnimationsEnabled` 属性的值更改时发生」。引入于 Windows 10 2004 / 19041，事件参数不带新值 | [UISettings.AnimationsEnabledChanged](https://learn.microsoft.com/en-us/uwp/api/windows.ui.viewmanagement.uisettings.animationsenabledchanged?view=winrt-28000)；类页版本历史同此（2026-11-20 取回） | 文档假定值变就会发生；**没有**写桌面 / 未打包 / 无 CoreWindow 下会不触发，也**没有**写它由 `SPI_SETCLIENTAREAANIMATION` 驱动 |
| 桌面应用「不支持的事件」：`UISettings` 仅列 `ColorValuesChanged`；`AccessibilitySettings` 仅列 `HighContrastChanged`。并写明：除非另有说明，这些类的其余成员在桌面应用中受支持 | [WinRT APIs not supported in desktop apps](https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/winrt-api-desktop-app-support)「Unsupported members / Events」表（2026-11-20 取回） | **不能**依据此表宣称 `AnimationsEnabledChanged` 官方不支持。此表也说明：同类事件在桌面应用里出问题，并不是没有先例 |
| 组合层裁剪指南要求应用「侦听并响应」`UISettings.AnimationsEnabled`，设置路径写为 Settings > Ease of Access > Display。示例代码只在构造时读一次属性，**没有**订阅 `AnimationsEnabledChanged`。对比同一页对透明效果的示例，则同时订阅了 `AdvancedEffectsEnabledChanged` | [Composition tailoring for WinUI apps](https://learn.microsoft.com/en-us/windows/apps/develop/composition/composition-tailoring)（2026-11-20 取回） | 官方动画示例是「启动时读一次」。这与 F5 改订阅之前的宿主行为一致，但**不能**单独当成「事件必定不触发」的证明 |
| 客户区动画是辅助功能参数之一。用户更改这些参数时，控制面板会发送 `WM_SETTINGCHANGE`；应用应响应此消息并用 `SystemParametersInfo` 判断当前状态 | [Accessibility parameters](https://learn.microsoft.com/en-us/windows/win32/winauto/accessibility-parameters)（2026-11-20 取回） | 这是 **Win32 官方推荐的变化通知路径**，不依赖 WinRT 事件 |
| `SPI_GETCLIENTAREAANIMATION = 0x1042`，`SPI_SETCLIENTAREAANIMATION = 0x1043`。`pvParam` 为 `BOOL`（TRUE 启用客户区动画及瞬时效果，FALSE 关闭）。**注意：`0x1041` 是 `SPI_SETDISABLEOVERLAPPEDCONTENT`，不是动画开关。** `SPI_GETANIMATION = 0x0048` / `SPI_SETANIMATION = 0x0049` 走的是 `ANIMATIONINFO`，是另一套「与用户操作相关的动画效果」（典型对应最小化/还原等窗口动画，F5 中的 `MinAnimate`） | [SystemParametersInfoW](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-systemparametersinfow)；[Client area animation parameter](https://learn.microsoft.com/en-us/windows/win32/winauto/client-area-animation)（2026-11-20 取回） | 常设常量以此为准。协调方草稿里的「0x1041？」应更正为 **0x1043（SET）/ 0x1042（GET）** |
| `fWinIni` 含 `SPIF_UPDATEINIFILE`（写入用户配置）和 `SPIF_SENDCHANGE`（更新配置后向所有顶层窗口广播 `WM_SETTINGCHANGE`；`SPIF_SENDWININICHANGE` 同义） | 同上 SystemParametersInfoW「fWinIni」 | F5 续跑用的正是 `SPIF_UPDATEINIFILE\|SPIF_SENDCHANGE`，按文档应当已经广播了 `WM_SETTINGCHANGE` |
| 系统因 `SystemParametersInfo` 发送 `WM_SETTINGCHANGE`（`0x001A`）时，`wParam` 为当时的 `uiAction`；`lParam` 可能指向区域名字符串，**通常并不指出具体是哪一个参数**，也可能为 NULL。收到后一般应重新加载应用用到的系统参数。该消息发给**所有顶层窗口**的 `WindowProc`；不能直接 `SendMessage` 给某个窗口，应 `HWND_BROADCAST` | [WM_SETTINGCHANGE](https://learn.microsoft.com/en-us/windows/win32/winmsg/wm-settingchange)（2026-11-20 取回） | 宿主应挂钩**顶层** HWND。不要用 `wParam == 0x1043` 作为唯一过滤器（设置应用路径是否也用这个 `uiAction`，未证实） |
| 仅消息窗口（`HWND_MESSAGE`）「does not receive broadcast messages」 | [Window Features · Message-Only Windows](https://learn.microsoft.com/en-us/windows/win32/winmsg/window-features)（2026-11-20 取回） | **禁止**用 message-only 窗口收这条广播 |
| WinUI 3 取 HWND：`WinRT.Interop.WindowNative.GetWindowHandle(window)` | [Retrieve a window handle (HWND)](https://learn.microsoft.com/en-us/windows/apps/develop/ui/retrieve-hwnd)（2026-11-20 取回） | 挂钩点用官方互操作即可 |
| `SetWindowSubclass` / `RemoveWindowSubclass`：按「回调函数 + 子类 ID」安装/拆除；**不能跨线程子类化**；回调委托必须钉住，防止被 GC | [SetWindowSubclass](https://learn.microsoft.com/en-us/windows/win32/api/commctrl/nf-commctrl-setwindowsubclass)、[RemoveWindowSubclass](https://learn.microsoft.com/en-us/windows/win32/api/commctrl/nf-commctrl-removewindowsubclass)（2026-11-20 取回） | 若不用现成 WinUIEx 监视器，就按此 API 在 UI 线程安装 |

官方文档**没有**写：该事件只在打包应用、只在设置 UI 切换、或只在存在 `CoreWindow` 时触发。这些机制性解释一律标 **未证实**。F5 报告 11.8 提出的两种可能（只走设置应用代理 / 无 CoreWindow 时 WinRT 通知链路未接通）可以保留为假说，不能写成已证实原因。

### 1.2 已核实的 F5 实验（本机宿主）

摘自 `f5_REPORT.md` §11，Windows 11 26100，FluentHub 侧叉松散包注册（`Add-AppxPackage -Register`），未重新构建，复用首跑产物。`NavMotionPilot` 已订阅 `UISettings.AnimationsEnabledChanged`，回调里先打 `F5 AnimationsEnabledChanged` 再 `DispatcherQueue.TryEnqueue` 调 `ApplyReducedMotion`。

已排除：

- 切错设置：关闭后**新进程**读到 `UISettings.AnimationsEnabled=False`，打开后为 `True`；`SPI_GETCLIENTAREAANIMATION` 同步为 0/1。
- 订阅未装上：两轮（含 F2 重建）都有 `F5 subscribed UISettings.AnimationsEnabledChanged=True`；cycle=2 的订阅早于切换。
- 进程已退出或实例已 Dispose：窗口仍在，`Dispose` 只在关窗后出现；两次 Dispose 都是 `events=0`。
- 线程亲和导致「进了回调但没打到 SetReducedMotion」：回调在入队前就应打 `F5 AnimationsEnabledChanged`，一行都没有。
- `MinAnimate` / `SPI_SETANIMATION` 被误切：`MinAnimate` 全程为 `1` 未变；变的是 `UserPreferencesMask` 第 5 字节 `0x12↔0x10`。
- 原值未恢复：四项回读与原值一致。

因此：在本 WinUI 3 桌面宿主（有松散包身份、无 UWP `CoreWindow` 模型）里，**用 SPI + `SPIF_SENDCHANGE` 改变了 `AnimationsEnabled` 所反映的那个设置，但该事件没有向已运行进程投递回调。** 这足以否定「只订这个事件就能满足 F5 验收第 2 项」，但还不足以把它升级为全平台官方已知缺陷。

本次**没有**用设置应用（「辅助功能 → 视觉效果 → 动画效果」或文档里的 Ease of Access > Display）做对照，因此「设置 UI 路径会不会触发该事件」标 **未证实**。修法不能假定「改用设置应用就能让 WinRT 事件复活」。

### 1.3 `AnimationsEnabled` 与 SPI 的对应

- **官方直接映射：未证实。** 已打开的 `AnimationsEnabled` 文档没有点名 `SPI_GETCLIENTAREAANIMATION`。
- **实验映射：已证实（K93，Windows 11 26100）。** `SPI_SETCLIENTAREAANIMATION` 之后，新进程的 `UISettings.AnimationsEnabled` 与 `SPI_GETCLIENTAREAANIMATION` 同向变化；`MinAnimate` 不变。
- 宿主谓词应继续明确写为：

  `reducedMotion = !uiSettings.AnimationsEnabled`

  不要改用 `SPI_GETANIMATION` / `MinAnimate`，也不要在未另做实验之前把两者 OR 到一起。可选：重读时顺手读一次 `SPI_GETCLIENTAREAANIMATION=0x1042`，若与 `AnimationsEnabled` 不一致只打警告，**仍以 `AnimationsEnabled` 为准**（与现有宿主行为一致）。

### 1.4 相邻公开报告（不是本事件的直接证明）

以下均已打开原文，只说明「WinRT 设置类事件在 WinUI 3 桌面 / 打包进程里不触发、业界改听 `WM_SETTINGCHANGE`」是相邻事实，**不能**等同于 `AnimationsEnabledChanged` 的官方说明。

- [microsoft-ui-xaml#9372](https://github.com/microsoft/microsoft-ui-xaml/issues/9372)（2026-11-20 取回）：`UISettings.ColorValuesChanged` 在 WinUI 3 中有不触发的报告。讨论中建议处理 `WM_SETTINGCHANGE`；有评论称在 Windows 10 上打包应用只能靠 `WM_SETTINGCHANGE` / `WM_DWMCOLORIZATIONCOLORCHANGED`，未打包有时可以收到 WinRT 事件。官方已把该事件列为桌面应用不支持。
- [WindowsAppSDK#4006](https://github.com/microsoft/WindowsAppSDK/issues/4006)（检索命中，本轮已打开 issue 9372 中的交叉引用与摘要；4006 正文与 9372 讨论一致指向「文档写了 ColorValuesChanged 在桌面应用不支持」）：打包 WinUI 3 在 Windows 10 上该事件不触发，UWP 无此问题。
- [WindowsAppSDK#3158](https://github.com/microsoft/WindowsAppSDK/issues/3158)（2026-11-20 取回）：`Microsoft.Win32.SystemEvents.UserPreferenceChanged` 在 WinUI 3 打包应用里常常不触发。建议改用 `SetWindowSubclass` 处理 `WM_SETTINGCHANGE`（示例：`WinRT.Interop.WindowNative.GetWindowHandle` + `SetWindowSubclass`）。另有评论指根因是 `DispatcherQueueSynchronizationContext` 未实现 `SynchronizationContext.Send`。**因此不要把 `SystemEvents` 当作本票主路径。**
- [Microsoft Q&A 878567](https://learn.microsoft.com/en-us/answers/questions/878567/winrt-getting-callbacks-to-win32-application)（2026-11-20 取回）：`AccessibilitySettings.HighContrastChanged` 有报告在 Win32 桌面应用不回调、仅打包/UWP 能收到。该事件已被官方列为桌面应用不支持。

在 microsoft-ui-xaml / WindowsAppSDK / CsWinRT 仓库检索中，**没有**找到专门声明「`AnimationsEnabledChanged` 在 WinUI 3 桌面进程不触发」的官方 issue 或文档句。此「无专门 issue」本身不是证明，只说明不能虚构编号。

### 1.5 §1 小结

| 命题 | 判定 |
|---|---|
| `AnimationsEnabledChanged` 被官方列为桌面应用不支持 | **未证实**（官方表未列入） |
| 该事件在所有 WinUI 3 / Win32 桌面进程、或所有松散包宿主中都不触发 | **未证实**（只有本宿主这一次实验） |
| 该事件只在设置 UI 切换时触发、SPI 路径不会触发 | **未证实** |
| 需要 `CoreWindow` / 特定消息泵，事件才能到 | **未证实**（官方未写；桌面应用本就不支持 `CoreWindow`） |
| 本 FluentHub 侧叉 + 松散包 + SPI+SENDCHANGE 路径上，已运行进程收不到该事件 | **已证实**（F5 §11） |
| 同一设置能被新进程的 `UISettings.AnimationsEnabled` 读到 | **已证实** |
| 客户区动画变化的官方 Win32 通知方式是 `WM_SETTINGCHANGE` + 重读参数 | **已证实**（辅助功能参数文档） |

修法结论：不要再把 F5 的真实来源押在这个 WinRT 事件上；改走官方 Win32 通知路径。事件订阅可以留作旁路，但不能当验收主证据。

---

## §2 替代方案比较

比较对象都只改宿主侧。共用前提：

- 谓词：`SetReducedMotion(!uiSettings.AnimationsEnabled)`，必须在 UI 线程调用（L1 `MotionSystem` 线程亲和，合同 Table 2；在错误线程会 `InvalidOperationException`）。
- `UISettings` 实例必须做成字段，防止被 GC 后订阅丢失（若仍保留事件旁路）。
- F2 会 `MainWindow.InitializeApplication(..., forceReload: true)` 重建 `RootView` / `NavMotionPilot`，但 **`MainWindow` 与其 HWND 保持同一实例**（见 `fluenthub_ro` 的 `src/FluentHub/Views/MainWindow.cs`）。挂钩必须挂在窗口生命周期上，不能每次 `NavMotionPilot` 构造都 `SetWindowSubclass` 一次却不拆除。
- 默认自动测试不得切换系统动画设置；Fake 来源是强制的。真实 SPI 只在 pidan 批准下做一次可选验收。

### 2.1 方案 A：顶层窗口处理 `WM_SETTINGCHANGE`，然后重读 `UISettings`

**怎么接（WinUI 3 C#，FluentHub 这类宿主）：**

1. 用官方互操作取顶层 HWND：`WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.Instance)`。若构造当下句柄为 0，等到第一次 `Activated` 再装（待 workbuddy 在侧叉上核实 HWND 就绪时机）。
2. 挂钩优先复用侧叉**已有**依赖：`FluentHub.csproj`（基线提交 `eb08f0dbcbb43880ab34038b0cb6742f0c0deca5`）已引用 `WinUIEx` **2.9.2**，`MainWindow` 已是 `WindowEx`。NuGet 2.9.2 功能列表含 “Windows Message Monitor”（https://www.nuget.org/packages/WinUIEx/2.9.2 ，2026-11-20 取回）。用法示例来自当前文档（GitHub `main`，同日取回），**精确类型名/命名空间以 2.9.2 程序集为准（待核实）**，形态为：

   ```csharp
   // 命名空间待 workbuddy 对 2.9.2 核实（文档示例未写 using；源码路径暗示 WinUIEx.Messaging）
   _monitor = new WindowMessageMonitor(MainWindow.Instance);
   _monitor.WindowMessageReceived += OnWindowMessageReceived;
   ```

   在处理函数里看 `e.Message.MessageId == 0x001A`（`WM_SETTINGCHANGE`），记录 `WParam` / `LParam`（`LParam != 0` 时 `Marshal.PtrToStringUni`）。
3. 若 2.9.2 里没有该类型或签名对不上：**不要加新包、不要升 WinUIEx**。改为在 UI 线程 `SetWindowSubclass`（`comctl32`），`SUBCLASSPROC` 委托必须保存在字段里；处理完调用 `DefSubclassProc`；关闭时 `RemoveWindowSubclass`。官方警告：不可跨线程子类化。
4. **不要**创建 `HWND_MESSAGE` 仅消息窗口来收广播，官方写明它不接收广播。
5. 收到 `0x001A` 后**不要**在 WndProc 里直接调 `SetReducedMotion`（重入风险）。安排 UI 线程去抖，到点后重读 `UISettings.AnimationsEnabled`，仅当与上次已应用值不同时再 `DispatcherQueue.TryEnqueue` → `ApplyReducedMotion`。
6. 过滤器：默认**不要**要求 `wParam == 0x1043` 才处理。原因：官方说 `lParam` 通常不指明具体参数；设置应用是否以 `uiAction=SPI_SETCLIENTAREAANIMATION` 广播，**未证实**。正确做法是：任何 `WM_SETTINGCHANGE` 都进入「去抖 + 重读」，值没变就只打诊断日志、不调 `SetReducedMotion`。`wParam == 0x1043` 可作为「高可信、可额外打标签」的信息，不能当唯一入口。

**线程模型：** `WM_SETTINGCHANGE` 进入创建该 HWND 的线程（即 UI 线程）的窗口过程。去抖定时器应使用 `Microsoft.UI.Dispatching.DispatcherQueue`（`MainWindow.DispatcherQueue` 或 `DispatcherQueue.GetForCurrentThread()`）。`SetReducedMotion` 只在入队后的 UI 回调里调用。

**去抖：** 系统或 `SystemParametersInfo` 可能连续广播。建议 **100 ms** 的 `DispatcherQueueTimer`（单次、可重置）：每次 `0x001A` 都重启定时器，到期只重读一次。100 ms 是实现参数，**不是**通过/失败时限。值未变则不算一次 ReducedMotion 变化。

**开销：** 空闲时为零周期成本（无轮询）。无关的 `WM_SETTINGCHANGE`（主题、区域、策略等）只会重置一个短定时器并做一次廉价属性读取。可忽略。

**测试：**

- 强制路径：Fake 来源翻转（见 §3），不碰系统设置。
- 挂钩接线（仍不改系统设置）：安装成功打 `F5 hook installed hwnd=…`；dev-only 向**同一套处理函数**注入一次「假装收到 0x001A」（或 `PostMessage` 到已挂钩 HWND）。若系统值未变，应看到 `F5 WM_SETTINGCHANGE …` 日志，且 **不**出现新的 `SetReducedMotion`。这只证明消息进了处理函数，不证明操作系统广播能到达。
- 可选真实路径：仅 pidan 批准。用与 F5 续跑相同的 SPI 标志，或由 pidan 在设置里切换。期望已运行进程打出 `WM_SETTINGCHANGE` 且 `SetReducedMotion` 入参跟随重读值；测完恢复原值。

**失败模式：**

- HWND 为 0 或挂钩装到子窗口：收不到顶层广播。
- 仅消息窗口：收不到广播。
- 只过滤 `wParam==0x1043`：可能漏掉设置应用路径（未证实）。
- 每个 `NavMotionPilot` 生命周期都重复子类化、F2 重建后拆错或叠两层。
- 委托被 GC：`SetWindowSubclass` 回调丢失。
- 在 WndProc 里直接碰 L1：亲和或重入。
- 打包 / 松散包 / 未打包：`WM_SETTINGCHANGE` 是 Win32 顶层广播，按文档与包身份无关；**实际送达仍要以本机可选验收为准**。
- 多窗口：本宿主只有一个 `MainWindow.Instance`，只挂钩这一扇。

### 2.2 方案 B：定时轮询 `UISettings.AnimationsEnabled`

**怎么接：** `DispatcherQueueTimer` 周期（例如 1–2 s，若要用应在代码里写成命名常量）重读属性，值变则 `SetReducedMotion`。不需要 HWND，也不依赖 WinRT 事件。

**线程模型：** 定时器本身就在 UI 队列上，封送简单。

**去抖：** 周期本身就是上限；不必再去抖。突发写入最多延迟一个周期。

**开销：** 应用存活期间一直跑。单次读取很便宜，但不如消息驱动干净，也给「动画已关」的用户引入固定唤醒。

**测试：** Fake 仍是自动测试主路径。轮询本身很难在不改系统设置的情况下证明「跟 OS 走」——除非另做 pidan 批准的真实切换，且要等一个周期，时序更差。

**失败模式：** 默认延迟大（用户关动画后仍可能再播最多一个周期）；把周期缩短又增加唤醒。不能代替「变化到达」的直接证据。可作为最后兜底，不推荐当主路径。

### 2.3 方案 C：A + B 组合（消息 + 慢轮询）

消息驱动为主，再加很慢的轮询（例如 10 s）或仅在漏消息时补救。线程与去抖同 A，再加上常驻定时器。开销略高于纯 A。测试仍以 Fake 为主。

**评价：** 能覆盖「广播丢失」的假说，但在尚未证明 `WM_SETTINGCHANGE` 会丢的情况下，给运行时增加固定成本。本票**不**把常驻轮询放进默认推荐。若可选真实切换证明挂钩已收到广播，就不必默认轮询。

### 2.4 方案 D（推荐附属，不是独立主路径）：窗口 `Activated` 时重读一次

`MainWindow.Activated`（或等价）在激活时重读 `AnimationsEnabled`，值变则走同一条 `ApplyReducedMotion`。线程已在 UI。无去抖需求。开销仅在激活。可接住「应用在后台时漏了广播、用户切回来才需要跟上」的情况，延迟要到下次激活，可接受。测试：不改系统设置时，激活不应误触发 `SetReducedMotion`（值不变）。

**推荐把它当作方案 A 的低成本第三路径，而不是替代 A。**

### 2.5 已排除或降级的选项

| 选项 | 结论 |
|---|---|
| 继续只订 `AnimationsEnabledChanged` | 本宿主已 FAIL，不能再当真实来源主路径 |
| `Microsoft.Win32.SystemEvents.UserPreferenceChanged` | 相邻 issue #3158 表明 WinUI 3 上不可靠；不作为主路径 |
| 把读取/监听放进 L1 | L1 是纯 net8.0、无 WinRT（先前 C2 / F5 归类）；禁止为 F5 改 TFM 或公开面 |
| message-only 窗口 | 官方：不接收广播 |
| 升 WinUIEx / 加新 NuGet / 改系统设置来「打通」事件 | 超出本票；要 pidan 另批 |

### 2.6 对比表

| | A. `WM_SETTINGCHANGE` + 重读 | B. 定时轮询 | C. A+B | A + Activated 重读（推荐） |
|---|---|---|---|---|
| 接线 | 顶层 HWND 子类 / WindowMessageMonitor | `DispatcherQueueTimer` | 两者都要 | A + `Activated` |
| 线程 | WndProc 已在 UI，仍入队再调 L1 | UI 定时器 | 同左 | 同 A |
| 去抖 | 重置 100 ms 再重读 | 周期即上限 | 同 A | 同 A；Activated 值不变则跳过 |
| 运行开销 | 仅在系统广播时 | 常驻 | 常驻 + 消息 | 广播时 + 激活时 |
| 不改系统设置如何测 | Fake + 注入 0x001A 打日志 | 只能测 Fake / 或改设置 | 同左 | 同 A |
| 主要风险 | HWND/挂钩错误；过滤过严 | 延迟、常驻唤醒 | 复杂 | 与 A 相同，另多一条激活日志 |
| 本票态度 | **真实来源主路径** | 不默认 | 不默认 | **推荐组合** |

---

## §3 推荐方案：workbuddy 可照做的步骤与验收

推荐：**宿主侧可切换来源 + 真实来源 = `WM_SETTINGCHANGE` 主路径 + `AnimationsEnabledChanged` 尽力而为旁路 + `Activated` 重读；测试默认 Fake；L1 / `composition.h` 不动。**

### 3.1 改哪里（侧叉，源码不入库）

工作区与基线（来自 F5 报告，路径若有变则待核实）：

- 侧叉：`%USERPROFILE%\pilot\FluentHub`，上游基线 `eb08f0dbcbb43880ab34038b0cb6742f0c0deca5`。
- 本票应叠在 **F5 之后** 的 `NavMotionPilot` 上（F5 报告 post sha256 `d0b111f56e2a809dedd566186e559cd1f1f2a8895e4a51667ee2344f67fec065`，补丁 `z6/f5/fluenthub_f5_NavMotionPilot.patch`），不要从本箱里的 T5-only 副本（`/workspace/z5/t5/NavMotionPilot.cs` 仍是「构造时读一次」）开改。
- 仓库内只应新增/更新 `z6/f5/**` 一类证据与台账（与 F5 相同约定）；`git diff --name-only` 不得出现 `MotionSoftDemo/`、`code/`、`pilot/l1pack/`。

建议文件（新文件名可按侧叉习惯微调，但职责不要拆散）：

| 路径 | 动作 |
|---|---|
| `src/FluentHub/Views/IReducedMotionSource.cs`（新） | 侧叉内部接口：`bool AnimationsEnabled { get; }`，`event Action<bool> Changed;`（或等价）。不要放到 L1 |
| `src/FluentHub/Views/OsReducedMotionSource.cs`（新） | 真实来源：持有 `UISettings`、窗口挂钩、去抖、Activated 重读、事件旁路 |
| `src/FluentHub/Views/FakeReducedMotionSource.cs`（新） | 仅 `FM_PILOT_F5=1`（或与先前约定相同的开关）时驱动 `SetReducedMotion` |
| `src/FluentHub/Views/MainWindow.cs` | 窗口级持有真实来源（或工厂），保证 F2 重建 RootView 时 **不**重新子类化、也不提前 Dispose 挂钩 |
| `src/FluentHub/Views/NavMotionPilot.cs` | 不再把「只订 WinRT 事件」当唯一来源；改为订阅 `IReducedMotionSource`，在 UI 线程 `ApplyReducedMotion`；`Dispose` 退订 |
| `src/FluentHub/Views/RootView.xaml.cs` | 已有 `_motionPilot ??= new NavMotionPilot(NavigationView)`（T5 补丁）。构造时可把来源传进去，或让 `NavMotionPilot` 向 `MainWindow` 取。F2 重建必须先 Dispose 再 new |

**不要改：** `composition.h`、L1 源、`pilot/l1pack` 工程、FluentHub 的 `TargetFramework` / 最低 OS 版本、包引用版本、系统设置、证书、开发者模式。

### 3.2 真实来源的行为（逐步）

1. **工厂：** 若环境变量 `FM_PILOT_F5=1`，`NavMotionPilot` 使用 `FakeReducedMotionSource` 作为**驱动 `SetReducedMotion` 的来源**。真实挂钩仍建议在非 Fake 运行里安装；Fake 运行里是否同时装挂钩仅用于诊断，**Fake 翻转不得被 OS 重读覆盖**（否则自动测试会被系统值抢掉）。推荐：Fake 模式下不把 OS 重读接到 `SetReducedMotion`，最多打 `F5 os reread ignored because fake=1`。
2. **启动：** 读 `UISettings.AnimationsEnabled`（与现在相同），在 UI 线程 `SetReducedMotion(!enabled)`，日志须带 `source=initial`（或沿用现有 `subscribed` / `initial` 格式，但要能区分来源）。
3. **安装挂钩（非 Fake 或诊断模式）：**
   - 取 `hwnd = WindowNative.GetWindowHandle(MainWindow.Instance)`，打 `F5 hook installed hwnd=0x… method=WindowMessageMonitor|SetWindowSubclass ok=True/False`。
   - `ok=False` 或 `hwnd=0`：**停下本条真实来源**，不要改系统设置重试，写入报告请 pidan 看。Fake 路径仍应可测。
   - 只装一次，与 `MainWindow` 同寿；`Closed` 时拆除并 Dispose monitor / `RemoveWindowSubclass`。
4. **收到 `WM_SETTINGCHANGE`（0x001A）：** 立即打一行（即使稍后值不变）：

   `F5 WM_SETTINGCHANGE wParam=0x… lParam="…" thread=…`

   重置 100 ms 去抖。到期后在 UI 队列重读 `UISettings.AnimationsEnabled`（可选交叉读 `SPI_GETCLIENTAREAANIMATION=0x1042`）。若与 `_lastAppliedAnimationsEnabled` 相同，打 `F5 reread unchanged animationsEnabled=…` 并返回。若不同：

   `F5 SetReducedMotion(True|False) source=wm animationsEnabled=… previous=… thread=…`

   然后入队调用已有 `ApplyReducedMotion`（F5 已把入队和亲和处理写在侧叉里，复用，不要在事件/WndProc 线程直接调 `fm_*`）。
5. **旁路 `AnimationsEnabledChanged`：** 若 OS ≥ 19041，可保留 F5 已加的版本守卫与订阅。回调同样只读值、入队。日志用 `source=event`。在本宿主上预期仍可能是 0 次；**有则记，无则不算失败**（真实路径验收看 `source=wm` 或 Fake）。`Dispose` 必须退订（F2 重建）。
6. **`Activated` 重读：** 值变则 `source=activated`，同样入队。值不变不打 Set 行（可打一条 debug）。
7. **计数：** 分别累计 `wm=`, `event=`, `activated=`, `fake=`，在退订/Dispose 时打印（替代目前只有 `events=0`）。

### 3.3 Fake 来源（默认自动验收，强制）

与先前 Gemini 意见一致，并收紧：

- 仅当 `FM_PILOT_F5=1` 时启用，作为 `SetReducedMotion` 的驱动来源。
- 翻转方式（实现至少一种，推荐两种都做）：
  1. 窗口内快捷键，例如 `Ctrl+Alt+Shift+R`（不要与系统或应用已有快捷键冲突，冲突则改并记入报告）。
  2. 命名事件，例如 `Local\FM_PILOT_F5_TOGGLE`（PowerShell 5 可 `EventWaitHandle` 打开并 `Set`），避免必须 `SendInput`。
- 翻转只改 Fake 的当前值，调用同一条 UI 线程 `ApplyReducedMotion`，日志：

  `F5 SetReducedMotion(True|False) source=fake animationsEnabled=… previous=… thread=…`

- **禁止** Fake 模式写 `SystemParametersInfo`、改 HKCU、改辅助功能设置。
- 可选：引用已有 C 层 `fm_capi_test` 中「飞行中切到 reduced，下一帧跳到终值」的用例作为 L1 行为旁证；宿主层只需证明 Fake 翻转到达 `SetReducedMotion`。不要把耗时当闸门。

### 3.4 验收条款（可检查）

**A. 冻结面（必过）**

- `git diff --name-only` 不含 `MotionSoftDemo/`、`code/`、`pilot/l1pack/`。
- `code/composition.h` 与冻结哈希一致（F5 续跑用过的核对方式可沿用）。
- 未合 main、未开/合 PR、未 force/amend、未 LOCK、未给第三方仓提 PR 或公开 fork。

**B. Fake 路径（必过，且不得改系统动画设置）**

- 运行前记录 `UISettings.AnimationsEnabled`、`SPI_GETCLIENTAREAANIMATION`、`MinAnimate`、`UserPreferencesMask`，跑完复核四项均未变。
- `FM_PILOT_F5=1` 启动后，翻转 Fake（快捷键或命名事件）：日志出现 `source=fake`，`SetReducedMotion` 入参随 Fake 值改变；再翻回，入参恢复。
- 须能看出是 UI 线程（沿用 F5 的 `thread=` 日志）。
- 不计时；无 `deltaUs` 通过标准；PERF-CI HOLD。

**C. 挂钩接线（必过，仍不改系统设置）**

- 非 Fake 的一次 GUI 启动（可用同一构建，关掉 `FM_PILOT_F5` 或另跑）日志含 `F5 hook installed hwnd=… ok=True`。
- dev-only 注入 `WM_SETTINGCHANGE`（或直接调处理函数）后，出现 `F5 WM_SETTINGCHANGE …`；系统值未变时**不应**出现新的 `source=wm` `SetReducedMotion`。
- 若 `ok=False`：本条真实来源未完成，停在报告里，不自行改系统设置或改清单重试。

**D. 可选真实切换（非默认；无 pidan 书面批准则跳过，不因此把整票打成实现失败）**

- 仅当 pidan 本人操作，或拍板卡原话明确批准「切换系统动画设置，测完恢复原值」。
- 建议仍用已验证的 SPI：`SPI_SETCLIENTAREAANIMATION=0x1043`，`uiParam=0`，`pvParam=BOOL`，`fWinIni=SPIF_UPDATEINIFILE|SPIF_SENDCHANGE`。关再开（或相反），每步回读。
- 通过标准（已运行进程）：出现 `F5 WM_SETTINGCHANGE`；随后 `source=wm`（或去抖后的重读来源）的 `SetReducedMotion` 入参与新的 `AnimationsEnabled` 一致。`source=event` 有没有都可以，记下来即可。
- 测完必须恢复原值并留下与 F5 §11.7 同类的对照表。
- workbuddy **不得**在没有上述批准时执行这一步。

**E. 回归**

- F2 重建（两轮 `NavMotionPilot`）后：旧实例已退订，新实例重新订阅同一窗口来源；不得重复安装未拆除的子类回调。
- 关窗后挂钩拆除，进程能正常退出（沿用计划任务用完即删、松散包按已有总许可卸载的流程，若许可仍有效）。

### 3.5 停止条件与风险

停下并写报告、不要改代码硬闯：

- 挂钩安装失败或 HWND 一直为 0。
- 为通过验收需要改 L1、抬 TFM、加新包、升 WinUIEx、改系统设置、注册证书、开开发者模式。
- 真实切换未获批准，却把「没做真实切换」写成实现缺陷（应写成「可选 D 未做」）。
- 真实切换获批但广播仍未进入已挂钩窗口：记 FAIL（真实路径），保留 Fake 与挂钩安装证据，**不要**擅自改用设置应用反复开关或改 HKLM；把现象交回审议/pidan。
- 发现 `AnimationsEnabled` 与 `SPI_GETCLIENTAREAANIMATION` 在本机不再同向：停下，不要悄悄改谓词。

已知风险：设置应用路径的 `wParam`/`lParam` 形态未证实；WinRT 事件旁路在本宿主可能继续为 0；`WindowMessageMonitor` 的精确 API 以 2.9.2 程序集为准；侧叉当前若尚未包含 F5 补丁，需先对上 sha 再改。

### 3.6 必须向 pidan 请示的事项

- **可选验收 D**：运行中切换系统动画设置并恢复。无批准就跳过。
- 松散包注册/卸载、一次性计划任务跑 GUI：仅在已有总许可仍有效时使用，并逐字引用；否则再请示。
- 不得自行：改辅助功能/显示/动画设置、装证书、开开发者模式、提权、改 HKLM、合 main、对 FluentHub / Files 上游提 PR 或公开 fork。

### 3.7 明确不做

- 不改 L1 公开面与 `composition.h`。
- 不把耗时、帧预算、PERF-CI 当作本票通过条件。
- 不把 Success 当成 adopt，不合 main。
- 不把「WinRT 事件在本宿主为 0」扩写成微软已承认的通用缺陷。

---

## 来源（本轮实际取回，访问日期 2026-11-20）

1. https://learn.microsoft.com/en-us/uwp/api/windows.ui.viewmanagement.uisettings.animationsenabled?view=winrt-28000
2. https://learn.microsoft.com/en-us/uwp/api/windows.ui.viewmanagement.uisettings.animationsenabledchanged?view=winrt-28000
3. https://learn.microsoft.com/en-us/uwp/api/windows.ui.viewmanagement.uisettings?view=winrt-28000
4. https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/winrt-api-desktop-app-support
5. https://learn.microsoft.com/en-us/windows/apps/develop/composition/composition-tailoring
6. https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-systemparametersinfow
7. https://learn.microsoft.com/en-us/windows/win32/winauto/client-area-animation
8. https://learn.microsoft.com/en-us/windows/win32/winauto/accessibility-parameters
9. https://learn.microsoft.com/en-us/windows/win32/winmsg/wm-settingchange
10. https://learn.microsoft.com/en-us/windows/win32/winmsg/window-features
11. https://learn.microsoft.com/en-us/windows/apps/develop/ui/retrieve-hwnd
12. https://learn.microsoft.com/en-us/windows/win32/api/commctrl/nf-commctrl-setwindowsubclass
13. https://learn.microsoft.com/en-us/windows/win32/api/commctrl/nf-commctrl-removewindowsubclass
14. https://github.com/microsoft/microsoft-ui-xaml/issues/9372
15. https://github.com/microsoft/WindowsAppSDK/issues/3158
16. https://learn.microsoft.com/en-us/answers/questions/878567/winrt-getting-callbacks-to-win32-application
17. https://www.nuget.org/packages/WinUIEx/2.9.2
18. https://raw.githubusercontent.com/dotMorten/WinUIEx/main/docs/concepts/WindowMessageMonitor.md （当前 `main` 文档，不等于已逐行核对 2.9.2 标签源码）
19. 本地：`/workspace/review/z6/results/f5_REPORT.md` §11；`/workspace/review/z6/z6_handover_review_gemini.md` §2.10；`/workspace/fluenthub_ro` @ `eb08f0dbcbb43880ab34038b0cb6742f0c0deca5` 的 `MainWindow.cs`、`RootView.xaml.cs`、`FluentHub.csproj`

检索中出现但本轮**未把正文当已核实引用**的条目（故不充当「已证实」来源）：WindowsAppSDK#4006 的独立正文页；microsoft-ui-xaml 的 `DXamlCore.cpp`（短哈希页面未取到源码）；WinUI 源码里 `SharedHelpers::IsAnimationsEnabled` 的实现文件。

---

## 未证实 / 限制

- 「`AnimationsEnabledChanged` 在 WinUI 3 / Win32 桌面进程里不触发」不是官方已写明的一般结论。
- `UISettings.AnimationsEnabled` 与 `SPI_GETCLIENTAREAANIMATION` 的**官方**等价关系未在已打开文档中写明（本机实验已证实同向）。
- 设置应用切换（相对 SPI）会不会投递 WinRT 事件、其 `WM_SETTINGCHANGE` 的 `wParam`/`lParam` 形态如何。
- 事件失败的根因（设置代理 / 无 CoreWindow / 打包身份 / CSWinRT / 消息泵）未证实。
- `WindowMessageMonitor` 在 WinUIEx 2.9.2 中的精确类型名、命名空间与构造函数：功能列表已写有该功能，API 示例取自当前 `main` 文档，需在 K93 对照 2.9.2 程序集。
- 本箱没有 F5 之后的 `NavMotionPilot` 源码（只在 K93 侧叉 / F5 补丁里）；插入点以 F5 报告与 T5/F5 路径为准，实施前核对 sha256。
- 当前 Windows 11 设置里该开关的精确中文路径（官方组合文档仍写 Ease of Access > Display）未在本轮打开设置页核对。
