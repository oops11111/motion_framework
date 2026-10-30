// M3-XAML-LOST probe (docs/m3_xaml_lost_gate_LOCKED.md). Diagnostic: does the app's own
// XAML UI come back after the display driver restarts (pnputil /restart-device, run by
// the runner when asked)? A counter and a colour block change every 250 ms, so two
// screenshots 1 s apart show whether XAML still produces new frames. After the restart
// the probe takes screenshots at +5/+15/+30 s, then tries four remedies in turn (resize
// by 1 px, minimise and restore, reset the page content), with screenshots after each.
// Plain sessions load no Rive and create no composition graphics device; with
// --m3-xaml-with-rive (N3 comparison) the Rive surface of M3-RIVE-LOST-RECOVER runs too.
// composition.h is not used or touched.
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.DirectX;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using WinRT;

namespace MotionSoftDemo;

internal static class M3XamlNative
{
    [StructLayout(LayoutKind.Sequential)] internal struct RECT { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] internal static extern bool GetWindowRect(IntPtr hwnd, out RECT r);
    [DllImport("user32.dll")] internal static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int cx, int cy, uint flags);
    internal const uint SWP_NOMOVE = 0x0002, SWP_NOZORDER = 0x0004, SWP_NOACTIVATE = 0x0010;
}

public sealed partial class MainPage
{
    const int M3XamlTickMs = 250;
    const int M3XamlWaitRestartMs = 300000;  // runner spacing and cooldown fit in 5 min

    private async Task RunM3XamlLostProbe(bool withRive)
    {
        if (_suiteRunning || _comp is null || XamlRoot is null) return;
        _suiteRunning = true;
        try { MeasureButton.IsEnabled = false; } catch { }

        string artDir = Environment.GetEnvironmentVariable("MF_M0_ARTDIR") ?? @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string sess = Environment.GetEnvironmentVariable("MF_M3_SESSION") ?? "1";
        string pre = Path.Combine(artDir, $"m3xaml_s{sess}");
        var raw = new StreamWriter(pre + "_raw.txt", false, new UTF8Encoding(false));
        // Shared for writing: with Rive, the DLL's abort handler appends to this file.
        var sum = new StreamWriter(new FileStream(pre + "_summary.txt", FileMode.Create, FileAccess.Write, FileShare.ReadWrite),
                                   new UTF8Encoding(false));
        bool verdictWritten = false;  // the runner waits for a verdict= line
        void Raw(string s) { Log(s); raw.WriteLine(s); raw.Flush(); }
        void Kv(string k, object v) { string s = $"{k}={Convert.ToString(v, CultureInfo.InvariantCulture)}"; Raw("KV " + s); sum.WriteLine(s); sum.Flush(); if (k == "verdict") verdictWritten = true; }
        static string Hx(int hr) => "0x" + hr.ToString("X8", CultureInfo.InvariantCulture);

        TextBlock? counter = null;
        Border? block = null;
        DispatcherQueueTimer? ticker = null;
        SpriteVisual? sprite = null;
        CompositionGraphicsDevice? gd = null;
        bool riveCreated = false;
        long ticks = 0, renderingEvents = 0;
        var sw = Stopwatch.StartNew();
        var renderTimes = new System.Collections.Generic.Queue<double>();
        void OnRendering(object? s, object e)
        {
            renderingEvents++;
            double now = sw.Elapsed.TotalMilliseconds;
            renderTimes.Enqueue(now);
            while (renderTimes.Count > 0 && now - renderTimes.Peek() > 1000) renderTimes.Dequeue();
        }
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            ShowWindow(hwnd, 9); SetForegroundWindow(hwnd);
            Kv("probe", "M3-XAML v1");
            Kv("with_rive", withRive ? 1 : 0);
            Kv("session", sess);
            Kv("pid", Environment.ProcessId);
            Kv("host", Environment.MachineName);
            Kv("os", Environment.OSVersion.VersionString);
            Kv("scale", F(XamlRoot.RasterizationScale, 3));

            // Counter (right of where the Rive surface sits) and a colour block that flips each tick.
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(480, 200, 24, 0),
                                         VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left };
            counter = new TextBlock { FontSize = 40, Text = "0", Width = 200 };
            block = new Border { Width = 80, Height = 80, Background = new SolidColorBrush(Colors.Red) };
            panel.Children.Add(counter);
            panel.Children.Add(block);
            RootGrid.Children.Add(panel);
            var red = new SolidColorBrush(Colors.Red);
            var green = new SolidColorBrush(Colors.LimeGreen);
            ticker = DispatcherQueue.CreateTimer();
            ticker.Interval = TimeSpan.FromMilliseconds(M3XamlTickMs);
            ticker.IsRepeating = true;
            ticker.Tick += (_, __) => { ticks++; counter.Text = ticks.ToString(CultureInfo.InvariantCulture); block.Background = (ticks & 1) == 0 ? red : green; };
            ticker.Start();
            CompositionTarget.Rendering += OnRendering;

            if (withRive)
            {
                // Same setup as M3LostProbe (RECOVER), so the comparison runs the code under suspicion.
                string rivPath = Environment.GetEnvironmentVariable("MF_M0_RIV") ?? Path.Combine(artDir, "m0_assets", "Knight_square_2.riv");
                uint px = (uint)Math.Round(400 * XamlRoot.RasterizationScale);
                int hr = M0Native.m0r_create(rivPath, px, px, out var dev);
                Kv("rive.create_hr", Hx(hr));
                if (hr < 0) { Kv("verdict", "INVALID_CREATE"); return; }
                riveCreated = true;
                M3LostNative.m0r_set_abort_note(pre + "_summary.txt");
                var gdHr = _comp.As<IM0CompositorInterop>().CreateGraphicsDevice(dev, out var gdPtr);
                Marshal.Release(dev);
                if (gdHr < 0) { Kv("rive.graphics_device_hr", Hx(gdHr)); Kv("verdict", "INVALID_GRAPHICS_DEVICE"); return; }
                gd = CompositionGraphicsDevice.FromAbi(gdPtr);
                Marshal.Release(gdPtr);
                Kv("rive.graphics_device_interop_qi_hr", Hx(M3LostNative.m0r_attach_graphics_device(((IWinRTObject)gd).NativeObject.ThisPtr)));
                var surface = gd.CreateDrawingSurface2(new Windows.Graphics.SizeInt32 { Width = (int)px, Height = (int)px },
                    DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
                Kv("rive.surface_interop_qi_hr", Hx(M0Native.m0r_attach_surface(((IWinRTObject)surface).NativeObject.ThisPtr)));
                var boxAt = AnimBox.TransformToVisual(RootGrid).TransformPoint(new Windows.Foundation.Point(0, 0));
                sprite = _comp.CreateSpriteVisual();
                sprite.Size = new Vector2(400, 400);
                sprite.Offset = new Vector3((float)boxAt.X, (float)boxAt.Y, 0);
                sprite.Brush = _comp.CreateSurfaceBrush(surface);
                ElementCompositionPreview.SetElementChildVisual(RootGrid, sprite);
                Kv("rive.start_hr", Hx(M0Native.m0r_start()));
            }

            // Two screenshots 1 s apart per observation point; the counter must differ if XAML is alive.
            async Task Obs(string name)
            {
                M3XamlNative.GetWindowRect(hwnd, out var r);
                Kv($"obs.{name}.t_ms", F(sw.Elapsed.TotalMilliseconds, 0));
                Kv($"obs.{name}.ticks", ticks);
                Kv($"obs.{name}.rendering_events_1s", renderTimes.Count);
                Kv($"obs.{name}.window", $"{r.Right - r.Left}x{r.Bottom - r.Top}");
                foreach (var part in new[] { "a", "b" })
                {
                    string shot = $"{pre}_{name}_{part}.png";
                    File.WriteAllText($"{pre}_{name}_{part}.shotreq", shot);
                    for (int i = 0; i < 50 && !File.Exists(shot); i++) await Task.Delay(100);
                    Kv($"obs.{name}.{part}", File.Exists(shot) ? Path.GetFileName(shot) : "MISSING");
                    if (part == "a") await Task.Delay(1000);
                }
            }

            await Task.Delay(2000);
            await Obs("base");

            File.WriteAllText($"{pre}_inject1.req", DateTime.Now.ToString("o", CultureInfo.InvariantCulture));
            var wait = Stopwatch.StartNew();
            string done = $"{pre}_inject1.done", failed = $"{pre}_inject1.failed";
            while (!File.Exists(done) && !File.Exists(failed) && wait.ElapsedMilliseconds < M3XamlWaitRestartMs) await Task.Delay(100);
            if (!File.Exists(done)) { Kv("inject1.done", 0); Kv("verdict", File.Exists(failed) ? "STOPPED_INJECT_FAILED" : "INVALID_NO_RESTART"); return; }
            Kv("inject1.done", 1);
            var since = Stopwatch.StartNew();  // adapter back to OK (the runner writes .done then)
            foreach (var (name, atMs) in new[] { ("t05", 5000), ("t15", 15000), ("t30", 30000) })
            {
                while (since.ElapsedMilliseconds < atMs) await Task.Delay(50);
                await Obs(name);
            }
            if (withRive)
            {
                var ev = new M3LostNative.LostEvent[4];
                M3LostNative.m0r_get_lost(out var det, out var rec, out var lostNow, ev, (uint)ev.Length);
                Kv("rive.lost_detected", det);
                Kv("rive.recovered", rec);
                Kv("rive.lost_now", lostNow);
                if (rec > 0) Kv("rive.recover_ms", F(ev[rec - 1].RecoverMs, 2));
            }

            // Remedies, in order (G1 question 2).
            M3XamlNative.GetWindowRect(hwnd, out var wr);
            int w = wr.Right - wr.Left, h = wr.Bottom - wr.Top;
            const uint flags = M3XamlNative.SWP_NOMOVE | M3XamlNative.SWP_NOZORDER | M3XamlNative.SWP_NOACTIVATE;
            M3XamlNative.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, w + 1, h + 1, flags);
            await Task.Delay(300);
            M3XamlNative.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, w, h, flags);
            await Task.Delay(2000);
            await Obs("resize");

            ShowWindow(hwnd, 6);  // SW_MINIMIZE
            await Task.Delay(1000);
            ShowWindow(hwnd, 9);  // SW_RESTORE
            SetForegroundWindow(hwnd);
            await Task.Delay(2000);
            await Obs("minrestore");

            var content = Content;
            Content = null;
            await Task.Delay(200);
            Content = content;
            await Task.Delay(2000);
            await Obs("content");

            Kv("rendering_events_total", renderingEvents);
            Kv("verdict", "RECORDED");
        }
        catch (Exception ex)
        {
            Kv("exception", ex.GetType().Name + ": " + ex.Message.Replace('\n', ' '));
            Kv("verdict", "INVALID_EXCEPTION");
        }
        finally
        {
            if (!verdictWritten) Kv("verdict", "INVALID_INCOMPLETE");
            CompositionTarget.Rendering -= OnRendering;
            ticker?.Stop();
            if (riveCreated)
            {
                M0Native.m0r_stop();
                M0Native.m0r_destroy();
            }
            if (sprite is not null) ElementCompositionPreview.SetElementChildVisual(RootGrid, null);
            GC.KeepAlive(gd);
            raw.Dispose(); sum.Dispose();
            FlushLog();
            _suiteRunning = false;
            try { MeasureButton.IsEnabled = true; } catch { }
        }
    }
}
