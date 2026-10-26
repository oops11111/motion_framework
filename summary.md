# Windows 桌面端动效框架技术栈：推荐方案

参与讨论：Gemini（会话 https://gemini.google.com/app/bc649f64a6516be3）和 Claude（会话 https://claude.ai/chat/5b0701a6-cbce-4ee5-a392-87e8d3c95643），共两轮。
各轮原文：`round0_gemini.md`（Gemini 初版摘要），`/workspace/claude_motion_reply.md`、`claude_motion_reply2.md`、`claude_motion_reply3.md`（Claude 各轮回复）。

## 一、推荐技术栈（首选）
- **执行层**：WinUI 3 / Windows.UI.Composition。常规过渡和微交互（平移、缩放、旋转、透明度、裁剪）编译成合成器动画，在 DWM 合成线程上运行，宿主主线程卡住时动效也不受影响。
- **引擎**：用 C++/WinRT 编写，对外只暴露 WinRT 组件（WinMD），通过它提供 C# 接口。不用 Rust，C ABI 只在 Rive/Skia 内部边界使用。
- **动画模型**：声明式时间线，支持顺序、并行、交错播放、标记和拖动进度；兼容 CSS 的三次贝塞尔曲线加命名缓动 token；闭式解弹簧，中途被打断改目标时保留当前速度；交互组件用状态机描述；所有动画默认可打断，并返回可暂停、跳转、改目标的句柄。
- **复杂矢量**：Rive 渲染到 CompositionDrawingSurface 上。这个表面的位移、缩放、淡入淡出一律由合成器属性（共享的 CompositionPropertySet）驱动。
- **设计到代码**：设计师在 Figma 插件里产出 JSON motion tokens（时长、缓动、弹簧预设、过渡规格），通过 PR 评审；Rive 负责交互插画；Lottie 只作为简单素材的导入通道。配套热重载预览、CI 校验 token 格式、黄金帧回归测试。
- **高 DPI 与刷新率**：采用 Per-Monitor v2 DPI，动效尺寸以 DIP 为单位，静止时对齐像素网格；所有计算基于真实时间戳，不写死 16.7ms；跟踪 VRR 和动态刷新率变化。
- **无障碍**：调用方必须提供减少动效时的替代方案，并跟随系统的动画开关设置。
- **宿主接口**：主接口是 `GetCompositionVisual()`。首发附带一个不受支持的 HWND 参考宿主示例（附源码，约 1-2 人周，不提供 API）；受支持的 `EmbedToHwnd` 推迟到 V1.5，是否做取决于首批接入方调查。
- **性能**：各阶段按流水线并行建模，预算按刷新间隔的比例来定：UI 线程中框架部分约 12%（120Hz 下 ≤1ms，动画启动时 ≤2ms），Rive 渲染线程约 35%，GPU 约 35%；8.3ms 是每个阶段的失败红线。CI 在专用物理机上运行：P95 同时检查绝对上限和相对基线回退；P99 以每千帧丢帧率作为门禁，初期只告警。

## 二、备选方案
引擎、token 和 API 都不变，渲染换成 DirectComposition visual 内的 D3D swapchain 加 Skia，由独立线程绘制全部内容。
- **触发条件**：Rive/D3D 技术验证没通过，或者 WinUI 3 不够稳定。
- **得失**：渲染管线完全自己掌控，不存在两层渲染之间的帧不同步，黄金帧测试也更精确，还保留了跨平台的可能；代价是失去合成线程的天然保障，要自己负责帧节奏，内存开销也略高。

## 三、实施门槛
1. 先用两周做技术验证：把 Rive 渲染进 CompositionDrawingSurface，再试 swapchain 路径，由一名熟悉 D3D11/12 的工程师负责。
2. 在 120Hz 下跑“拖动含 Rive 组件”的黄金帧测试，确认不出现画面错位。
3. 两项都通过后才全面投入。没通过就把 Rive 改走 Skia 备选路径，token 和引擎不受影响。

## 四、分歧点与处理
| 议题 | Gemini | Claude | 结论 |
|---|---|---|---|
| 初版思路 | 按团队背景在 Electron/Tauri、WinUI 3/WPF、Qt、Flutter 中挑选 | 要做的是可嵌入的动效框架，不是选整套 App UI 框架 | Gemini 已接受，改为原生 Composition 方案 |
| Web 动效“上限最高” | 初版这么认为 | 只有 transform/opacity 走合成器，JS 动画库跑在主线程，负载高时会卡 | Gemini 接受 |
| 技术栈复杂度 | C++/WinRT、Rust、C ABI 混用风险很高 | 接受，去掉 Rust，公开接口只保留 WinMD | 采纳 Claude 的缓解办法 |
| 两层渲染帧不同步 | 拖动时可能差 1 到 2 帧 | 表面的运动统一由合成器驱动，可以消除；Rive 内部内容跟随指针仍有约 1 帧延迟，写进文档作为设计约束 | 达成一致，但这一点仍待测试验证 |
| Rive 在 Windows 上的集成成熟度 | 文档少，需要很强的 D3D 能力 | 接受，无法确认现状，先做验证 | 列为第一道门槛 |
| 8.3ms 帧预算 | 作为预算 | 只是红线，目标约 2ms | 采纳 Claude 的说法 |
| HWND 宿主（WPF/Win32） | 最终接受首发提供预览版宿主 | 首发只给不受支持的参考宿主示例 | 裁决：首发只给参考示例，正式 API 推迟到 V1.5，详见 disputes.md |
| Rive 跟随延迟 | 最初称可以做到 0 帧或小于 0.5 帧 | 无法普遍消除 | 写成设计约束并提供缓解指南，延迟以实测为准，详见 disputes.md |

## 五、理由
- 两方最终首选一致；只有 Composition 能把常规动效放到合成线程上，最符合“120fps、低占用”的要求。
- Electron/Tauri、Flutter 和 Qt 都属于整套 App UI 框架，难以嵌入别的应用。Web 动效只有少数属性能走硬件加速。Qt 还涉及许可、原生观感和无障碍方面的成本。
- Claude 提醒：两个模型容易互相认同，结论是否成立要看技术验证和画面错位测试的结果，而不是看双方是否一致。


> 分歧点专题讨论（HWND 宿主、帧耗时红线与测量、Rive 延迟）的详细过程见 `disputes.md`。

## 文档索引
- [工程文档](docs/engineering.md)
- [公开 API](docs/api.md)
- [使用与接入](docs/usage.md)
- [DeepSeek 文档审阅](docs/deepseek_review.md)
- [探索轮汇总](docs/explore_round.md)
- [Gemini 探索](docs/gemini_explore.md)
- [DeepSeek 探索](docs/deepseek_explore.md)
- [分歧结论](disputes.md)
- [实施计划](plan.md) / [方案摘要](summary.md)
