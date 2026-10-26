# 分歧第 1 轮：Gemini 立场摘要（原文在旧对话中）
1. EmbedToHwnd：同意后置到 V1.5，首发只做 WinUI 3 + GetCompositionVisual()。估算 18-26 人周，另加每年 4-6 人周维护（估计）。WPF/Win32 存量占 70-80%、WinUI 3 占 15-20%（均为行业估计，未核实）。
2. 帧耗时：认为 8.3ms 红线和 2ms 目标合理。分配：宿主 UI 3.0ms、引擎 CPU 1.5-2.0ms、合成 2.0ms、GPU 2.0ms。测量用 ETW/WPA、IDCompositionFrameStatistics、PresentMon/PIX。CI 以 P95 引擎单帧 ≤2ms 作为阻断门槛，P99 只做监控。
3. Rive 跟随延迟：无法降到零，可缩到 0.5 帧以内。首选把跟随部件拆成由 ExpressionAnimation 驱动的合成器 visual（自称 0 帧，未核实）；其次是指针预测、按输入时间戳同步推进 Rive；保底是设计上加弹性阻尼。
