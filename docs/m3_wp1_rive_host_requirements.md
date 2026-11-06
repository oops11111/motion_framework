# WP1 Rive 宿主产品化要求（m3_wp1_rive_host_requirements.md）

> 状态：**已采用**（pidan Y1，2026-11-06）。把 M3 各票的真机结论收拢成产品化 Rive 宿主（由 `m0rive` 原型做成引擎模块）必须具备的行为。数字均为 DESKTOP-IJI1KJG（Win11 26300，RX 7800 XT）上的实测；其他环境以 M3-MATRIX 后续票补充。

| # | 要求 | 依据 |
|---|---|---|
| H1 | **合成绘制表面为默认渲染路径**（交换链为可选） | F1；M0-A/M0-B |
| H2 | **首帧预热**：首次显示前在离屏先画到非空为止（Rive 着色器异步编译） | M0-A 参考图全空 |
| H3 | **DPI 变化时重新光栅化**：`XamlRoot.RasterizationScale` 变化 → 表面 Resize → 按新尺寸渲染；逻辑尺寸不变 | M3-RIVE-DPI PASS（≤ 13.3 ms） |
| H4 | **设备丢失自动恢复**：渲染线程检测 `DXGI_ERROR_DEVICE_REMOVED` 等；丢失期间不调用 Rive；只在硬件适配器上建新设备，用检查返回值的小资源验证并稳定 500 ms 后才交给 Rive；`ICompositionGraphicsDeviceInterop::SetRenderingDevice`；重新导入 .riv（动画从头播放）；恢复耗时上限 5 s | M3-RIVE-LOST-RECOVER PASS（2.1–2.3 s） |
| H5 | **设备丢失时通知宿主**（事件或回调），由宿主决定是否提示重启应用 | M3-XAML-* 诊断：部分环境下 XAML 自身不恢复（`docs/m3_known_limits.md`） |
| H6 | **默认按窗口所在显示器的垂直同步出帧，并在垂直同步后约 0.3·T 再提交**（`WaitForVBlank` + 高精度定时器；动画时钟按垂直同步时刻）。窗口换屏或设备替换时重新查找输出；找不到时退回 `DwmFlush` | M3-MATRIX FAIL（`DwmFlush` 跟随最快的屏）→ M3-MATRIX-PACING r4 PASS |
| H7 | **跟随指针的部件按 WP2 设计约束处理**（松耦合部件拆成表达式驱动的 visual + 弹簧阻尼；严格对齐的部件留在 Rive 里） | M3-RIVE-FOLLOW PASS；`docs/m3_wp2_follow_guidelines.md` |
| H8 | **预算**：渲染线程 CPU P95、Rive 渲染 GPU P95 ≤ 0.35·T（T 为窗口所在屏的帧周期）；拷贝进表面单独统计 | plan.md；PACING r3/r4 |
| H9 | **Rive 本身的容错补丁**（`VERIFY_OK` 失败不应 `abort()`；`onMap` 检查返回值）：产品化待办，需要以 `--with-exceptions` 重编 Rive | K7、M3 |

**测试接口保留**：`m0r_get_intervals`、`m0r_get_gpu_split`、`m0r_get_pacing*`、`m0r_get_lost`、`m0r_simulate_lost` 等诊断接口在产品模块中以内部 / 调试接口保留，供后续回归。
