# Gemini 初版方案（摘要，原文见 https://gemini.google.com/app/bc649f64a6516be3）
按四类技术栈分析：
1. Web（Electron/Tauri + React/Vue + CSS/Framer Motion/GSAP/Lottie-web/WebGL）：动效生态最丰富，上限高；内存占用大。
2. .NET 原生（WPF Storyboard/VSM；WinUI 3 Composition API，独立合成线程、Connected Animation、Acrylic/Mica；Lottie-Windows）：系统融合好、硬件加速；WPF 老旧，WinUI 3 生态尚在完善。
3. C++（Qt Quick/QML 动画与状态机；Win32 + DirectComposition）：性能天花板、内存低；门槛高、代码量大。
4. Flutter Desktop（AnimationController/Tween，Skia/Impeller，Rive）：跨平台一致、上限高；Windows 原生字体与系统集成需适配。
选型建议：重开发速度和复杂动效、团队懂前端选 Electron/Tauri；重原生风格选 WinUI 3/WPF；重性能和轻量选 Qt/C++。
