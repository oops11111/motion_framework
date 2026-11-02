# 外部问题报告草稿：显卡驱动重启后 WinUI 3 应用的 XAML 内容不再绘制

> 状态：**草稿，未提交到任何外部渠道**（pidan T2：由 pidan 决定由谁、在哪里提交——Windows 反馈中心和/或 AMD）。
> 不含任何内部人名、机器名之外的私人信息；提交前请再核对一遍附件。

---

## Title
WinUI 3 (WinAppSDK 2.5.1): after a display-driver restart, XAML stops rasterizing text and shaped elements process-wide on Windows 11 build 26300 + AMD RX 7800 XT; Windows 10 + NVIDIA recovers

## Summary
After the display driver is restarted while a WinUI 3 app is running (`pnputil /restart-device <GPU instance id>`, equivalent to a driver update or a driver reset), every XAML element that needs XAML's own rasterization (TextBlock text, rounded Borders, Buttons, ToggleSwitch, the title bar icon and caption) disappears and is never drawn again. XAML is still producing frames: `CompositionTarget.Rendering` keeps firing (~75/s at 75 Hz), and a plain rectangular Border with a SolidColorBrush keeps updating its colour. The loss is process-wide: a second window that was already open, and a brand-new `Window` created after the restart, are both entirely black. Resizing, minimise/restore and replacing `Content` do not help. Restarting the app restores the UI.

The same build on a Windows 10 machine with an NVIDIA GPU recovers fully after the same restart (verified that the app's D3D11 device was actually removed: `DXGI_ERROR_DEVICE_REMOVED`).

## Environment
| | Affected | Not affected |
|---|---|---|
| OS | Windows 11, build 26300 (Insider/preview) | Windows 10 22H2, build 19045 |
| GPU / driver | AMD Radeon RX 7800 XT, driver 32.0.12033.1030 | NVIDIA GeForce GTX 970, driver 32.0.15.8278 |
| Display | 1280×1024 @ 75 Hz, 100 % | 1920×1080 @ 60 Hz, 125 % |
| App | WinAppSDK 2.5.1, .NET 10, x64 Debug; both unpackaged self-contained and MSIX-packaged | same build, MSIX-packaged |

## Steps to reproduce
1. Create a WinUI 3 desktop app (WinAppSDK 2.5.1) whose window shows a TextBlock, a Button, a Border with `CornerRadius="16"` and a plain rectangular Border with a SolidColorBrush. Update the TextBlock and the rectangle's colour every 250 ms from a DispatcherQueueTimer.
2. Optionally open a second `Window` with similar content.
3. Run the app. In an elevated PowerShell, restart the display adapter:
   `pnputil /restart-device "<instance id from Get-PnpDevice -Class Display>"`
4. Wait for the screen to come back (about 3 s), then observe the windows for 30 s. Optionally create another new `Window` with the same content.

## Expected
All XAML content is redrawn after the device is restored, as on Windows 10 + NVIDIA.

## Actual (Windows 11 26300 + AMD)
- Text, rounded Borders, Buttons and ToggleSwitch are not drawn in any window of the process, including windows created after the restart; the title bar icon and caption are gone too.
- The plain rectangular Border keeps updating, `CompositionTarget.Rendering` keeps firing, and the TextBlock's text keeps changing in the visual tree.
- Waiting 30 s, resizing by 1 px, minimise + restore, and setting `Content` to null and back do not restore anything.
- Reproduced in 3/3 unpackaged sessions, 1/1 packaged session, and 4/4 earlier sessions (identical with and without our own Direct3D/composition content in the window).

## Notes
- Our own D3D11 content (a `CompositionDrawingSurface` fed by a separate D3D11 device; recovered with `ICompositionGraphicsDeviceInterop::SetRenderingDevice`) recovers on both machines within 2–5 s, so the compositor itself is working.
- Screenshots and logs are available on request (`m3/results_xaml_lost/`, `m3/results_xaml_rebuild/`, `m3/results_xaml_k93/` in our repository; check them for private content before attaching).
- We cannot yet tell whether the Windows build or the AMD driver is responsible; we have no Windows 11 release-build + AMD or Windows 11 26300 + NVIDIA machine to separate them.
