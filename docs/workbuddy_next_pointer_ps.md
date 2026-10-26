# 派题 #32 · WinUI 3 指针位置 PropertySet 可用性（真机）

## 目标
回答待验证项：WinUI 3（WASDK 2.5.1，MotionSoftDemo）里能否拿到指针位置的 CompositionPropertySet，供表达式动画跟手用。

## 要测
1. `Microsoft.UI.Xaml.Hosting.ElementCompositionPreview.GetPointerPositionPropertySet(UIElement)` 是否存在、能否调用（编译 + 运行）。记录 HRESULT/异常原文。
2. 若可用：用 ExpressionAnimation（如 `pointer.Position.X`）驱动一个 Visual 的 Translation.X；鼠标移动后用 WGC 或读回确认 Visual 跟随，量粗略延迟（帧数即可）。
3. 若不可用：列出可替代的 API（例如 `InteractionTracker`、`PointerPoint` + UI 线程写 PropertySet），各写一句是否在 WinUI 3 可用（实测或编译验证，不要猜）。
4. 鼠标需真人移动时：改用 `SendInput` 合成移动；做不到就写明。

## 交付
- `artifacts/win_pointer_ps_probe.txt`
- `docs/workbuddy_soft_demo.md` 加 ## 派题 #32
- 不改 composition.h；不花钱
- 回一行中文结论：AVAILABLE / NOT_AVAILABLE / PARTIAL + 关键证据
