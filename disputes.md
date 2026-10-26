# 分歧点专题讨论结论（2026-10-19）

参与讨论：Gemini（https://gemini.google.com/app/bc649f64a6516be3）和 Claude（https://claude.ai/chat/5b0701a6-cbce-4ee5-a392-87e8d3c95643），共两轮。Claude 原文见 `/workspace/claude_motion_reply4.md` 和 `claude_motion_reply5.md`，Gemini 第 1 轮摘要见 `disputes_r1_gemini.md`。
说明：文中所有人周数和市场占比都是两个模型的估计，未经核实。

## 1. HWND（WPF/Win32）宿主是否首发
- **共识**：受支持的完整版 `EmbedToHwnd`（包括 XAML Islands、可与 WPF 内容重叠混合、完整命中测试和 DPI 适配）不首发，推迟到 V1.5。首发以 WinUI 3 为一等宿主，主接口是 `GetCompositionVisual()`。双方都承认没有可靠的市场占比数据（Gemini 的 70-80% / 15-20% 是估计），改为调查首批 5-10 家接入方各自用什么宿主。
- **人周差异**：属于范围不同。Gemini 的 18-26 人周算的是工业级完整封装，Claude 后来把精简预览版从 3-5 人周上调到 5-8 人周，并承认漏算了 MFC/ATL/WinForms 的兼容问题。Airspace（HWND 区域无法与重叠的 WPF 内容混合）在精简方案里无法解决，只能写成硬限制。
- **最后一轮双方立场交叉**：Gemini 转而接受“首发提供预览版 HWND 宿主”；Claude 则改口认为带缺陷的预览 API 会带来支持义务，主张首发只提供一个**不受支持的参考宿主示例（附源码）**，约 1-2 人周，不提供 API。
- **裁决建议**：采纳 Claude 的最终版本。首发附带一个不受支持的 HWND 参考宿主示例，不提供 `EmbedToHwnd` 接口，正式 API 推迟到 V1.5，是否做由接入方调查结果决定。理由：成本最低、不产生 API 承诺，又能让 WPF/Win32 团队先试用并收集真实需求。

## 2. 帧耗时红线与测量
- **共识**：各阶段按流水线并行建模，每个阶段各自不能超过刷新间隔，不能加总分配（Gemini 的加总分配合计约 8.75ms，已经超过 8.3ms，已被纠正）。预算按刷新间隔的比例来定，以适配 60/120/240Hz。
  - UI 线程中框架自身的部分约占 12%，120Hz 下稳态 ≤1ms，动画启动时 ≤2ms（Gemini 的结论写作 ≤1.5ms）。
  - Rive 渲染线程约占 35%，≤3ms；GPU 上本框架内容约占 35%，≤3ms。
  - 8.3ms 是每个阶段的失败红线，不是可以用满的预算。
- **测量**：用 ETW/WPA 加自定义 TraceLogging 事件测各线程耗时；合成器动画无法在进程内计时，要靠 DWM/DXGI 的 ETW 事件、GPUView 和 PresentMon；GPU 部分用 PIX；`IDCompositionFrameStatistics` 只适用于 DComp 承载的路径；关键场景至少用一次外部拍摄（高速相机或采集卡）校验。CI 必须跑在带真实 GPU 的专用物理机上，每个场景至少 1000 帧、跑多次。
- **剩余分歧**：P95 门禁是用绝对值还是相对值。Gemini 最终只采用“相对基线回退超过 10%”；Claude 主张在固定参考机上同时卡绝对值（如 ≤2ms）和相对回退，理由是只看相对值会放过缓慢累积的变慢。
- **裁决建议**：两项都卡。P95 同时检查绝对上限和相对回退；P99 以每千帧丢帧率（超过 1.5 倍刷新间隔记为丢帧）作为门禁，初期先只告警、不阻断；P50 只用于看趋势。

## 3. Rive 跟随指针的延迟
- **共识**：无法普遍消除。首选把跟随部件拆成由 ExpressionAnimation 驱动的合成器 visual，其次在渲染线程最后时刻再读取指针位置（late latching，配合最大帧延迟 1 和 waitable 对象）、预测指针位置、按 `GetPointerInfoHistory` 的时间戳推进 Rive；小表面可以在 UI 线程同步绘制，但需要显式开关。Rive 内部依赖指针的变形写成设计约束，装饰性跟随动效可以加轻微弹簧阻尼来掩盖延迟。
- **已纠正的说法**：
  - `GetPointerPositionPropertySet` 给出的是相对某个元素的指针位置，不是系统全局位置，而且是 XAML 宿主专属 API，精简 HWND 宿主里用不了。
  - `InteractionTracker` 只用于平移、缩放等手势操作，不能跟踪鼠标悬停。
  - 鼠标悬停跟随即使走合成器，Gemini 也承认仍有约 0.5-1.5 帧的采样延迟，此前说的“0 帧”“小于 0.5 帧”都不成立。
  - Claude 认为 composition swapchain 不支持 `Present(0, ALLOW_TEARING)`，即使支持也无法低于 DWM 的合成节奏，需要一个小测试确认。
- **裁决建议**：写成设计约束，同时提供分级缓解指南。不承诺任何具体帧数，实际延迟作为技术验证的交付指标实测。

## 待技术验证确认的事项
WinUI 3 中 `GetPointerPositionPropertySet` 是否可用；composition swapchain 是否支持 ALLOW_TEARING；实测 Rive 跟随延迟；在参考机上标定各阶段预算。
