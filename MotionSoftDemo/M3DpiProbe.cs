// M3-RIVE-DPI probe (docs/m3_rive_dpi_gate_LOCKED.md). The Rive surface (M3 F1:
// drawing-surface path) keeps a fixed logical size while the operator switches the
// display scale 100% <-> 125% when prompted (G1/G2). On every XamlRoot scale change
// the surface is re-rasterised at round(logical * scale) pixels via m0r_resize.
// No pointer injection (G4). composition.h is not used or touched.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.DirectX;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using WinRT;

namespace MotionSoftDemo;

internal static class M3Native
{
    const string Dll = "m0rive";
    [DllImport(Dll)] internal static extern int m0r_resize(uint w, uint h);
    [DllImport(Dll)] internal static extern int m0r_get_resize(out uint count, out double lastMs, out double maxMs,
                                                             out uint w, out uint h, out int lastHr);
}

public sealed partial class MainPage
{
    const double M3LogicalDip = 400;       // surface edge in DIP, constant across scales
    const int M3SegmentMs = 8000;          // >= 300 frames per scale segment at 60-75 Hz
    const int M3SwitchTimeoutMs = 180000;  // operator has 3 min per switch
    const int M3Switches = 4;              // 100 -> 125 -> 100 -> 125 -> 100 (G2)

    private async Task RunM3DpiProbe()
    {
        if (_suiteRunning || _comp is null || XamlRoot is null) return;
        _suiteRunning = true;
        try { MeasureButton.IsEnabled = false; } catch { }

        string artDir = Environment.GetEnvironmentVariable("MF_M0_ARTDIR") ?? @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string sess = Environment.GetEnvironmentVariable("MF_M3_SESSION") ?? "1";
        string rivPath = Environment.GetEnvironmentVariable("MF_M0_RIV") ?? Path.Combine(artDir, "m0_assets", "Knight_square_2.riv");
        string pre = Path.Combine(artDir, $"m3dpi_s{sess}");
        var raw = new StreamWriter(pre + "_raw.txt", false, new UTF8Encoding(false));
        var sum = new StreamWriter(pre + "_summary.txt", false, new UTF8Encoding(false));
        bool verdictWritten = false;  // the runner waits for a verdict= line
        void Raw(string s) { Log(s); raw.WriteLine(s); raw.Flush(); }
        void Kv(string k, object v) { string s = $"{k}={Convert.ToString(v, CultureInfo.InvariantCulture)}"; Raw("KV " + s); sum.WriteLine(s); sum.Flush(); if (k == "verdict") verdictWritten = true; }
        static string Hx(int hr) => "0x" + hr.ToString("X8", CultureInfo.InvariantCulture);
        static uint Px(double scale) => (uint)Math.Round(M3LogicalDip * scale);

        SpriteVisual? sprite = null;
        TextBlock? prompt = null;
        bool created = false;
        var scaleChanged = new List<(double ms, double scale)>();
        var sw = Stopwatch.StartNew();
        double lastScale = XamlRoot.RasterizationScale;
        void OnXamlRootChanged(XamlRoot r, XamlRootChangedEventArgs a)
        {
            double sc = r.RasterizationScale;
            if (Math.Abs(sc - lastScale) < 1e-6) return;
            lastScale = sc;
            lock (scaleChanged) scaleChanged.Add((sw.Elapsed.TotalMilliseconds, sc));
        }
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            ShowWindow(hwnd, 9); SetForegroundWindow(hwnd);
            double scale0 = XamlRoot.RasterizationScale;
            double refreshHz = 0;
            try { MfProbe.mf_dwm_timing(out _, out _, out _, out _, out var rn, out var rd, out _); if (rd > 0) refreshHz = (double)rn / rd; } catch { }
            double T = refreshHz > 0 ? 1000.0 / refreshHz : double.NaN;
            Kv("probe", "M3-DPI v1");
            Kv("session", sess);
            Kv("pid", Environment.ProcessId);
            Kv("host", Environment.MachineName);
            Kv("os", Environment.OSVersion.VersionString);
            Kv("refresh_hz", F(refreshHz, 3));
            Kv("scale0", F(scale0, 3));
            Kv("logical_dip", M3LogicalDip);

            int hr = M0Native.m0r_create(rivPath, Px(scale0), Px(scale0), out var dev);
            Kv("create_hr", Hx(hr));
            if (hr < 0) { Kv("verdict", "INVALID_CREATE"); return; }
            created = true;
            var gdHr = _comp.As<IM0CompositorInterop>().CreateGraphicsDevice(dev, out var gdPtr);
            Marshal.Release(dev);
            Kv("graphics_device_hr", Hx(gdHr));
            if (gdHr < 0) { Kv("verdict", "INVALID_GRAPHICS_DEVICE"); return; }
            var gd = CompositionGraphicsDevice.FromAbi(gdPtr);
            Marshal.Release(gdPtr);
            var surface = gd.CreateDrawingSurface2(new Windows.Graphics.SizeInt32 { Width = (int)Px(scale0), Height = (int)Px(scale0) },
                DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
            hr = M0Native.m0r_attach_surface(((IWinRTObject)surface).NativeObject.ThisPtr);
            Kv("surface_interop_qi_hr", Hx(hr));
            if (hr < 0) { Kv("verdict", "INVALID_SURFACE_INTEROP"); return; }

            var boxAt = AnimBox.TransformToVisual(RootGrid).TransformPoint(new Windows.Foundation.Point(0, 0));
            sprite = _comp.CreateSpriteVisual();
            sprite.Size = new Vector2((float)M3LogicalDip, (float)M3LogicalDip);   // DIP; pixels follow the scale
            sprite.Offset = new Vector3((float)boxAt.X, (float)boxAt.Y, 0);
            sprite.Brush = _comp.CreateSurfaceBrush(surface);
            ElementCompositionPreview.SetElementChildVisual(RootGrid, sprite);
            prompt = new TextBlock { FontSize = 28, Margin = new Thickness(24, 200, 24, 0), TextWrapping = TextWrapping.Wrap };
            RootGrid.Children.Add(prompt);
            XamlRoot.Changed += OnXamlRootChanged;
            Kv("start_hr", Hx(M0Native.m0r_start()));

            var segs = new List<(double scale, M0Native.Stats st, long privMb)>();
            async Task Segment(int k, double scale)
            {
                prompt!.Text = $"M3-DPI 第 {k} 段：缩放 {scale:P0}，请勿操作（约 {M3SegmentMs / 1000} 秒）";
                M0Native.m0r_reset_stats();
                await Task.Delay(M3SegmentMs);
                M0Native.m0r_get_stats(out var st);
                var proc = Process.GetCurrentProcess(); proc.Refresh();
                long privMb = proc.PrivateMemorySize64 / (1024 * 1024);
                segs.Add((scale, st, privMb));
                string p = $"seg{k}";
                Kv(p + ".scale", F(scale, 3));
                Kv(p + ".frames", st.Frames);
                Kv(p + ".draw_failures", st.BeginDrawFailures);
                Kv(p + ".cpu_p95_ms", F(st.CpuP95, 3));
                Kv(p + ".cpu_p95_frac_T", F(st.CpuP95 / T, 3));
                Kv(p + ".gpu_p95_ms", F(st.GpuP95, 3));
                Kv(p + ".gpu_p95_frac_T", F(st.GpuP95 / T, 3));
                Kv(p + ".gpu_samples_dropped", st.GpuSamplesDropped);
                Kv(p + ".private_mb", privMb);
                // Window screenshot (runner) and a reference rendered at this pixel size.
                string shot = $"{pre}_seg{k}.png";
                File.WriteAllText($"{pre}_seg{k}.shotreq", shot);
                for (int i = 0; i < 50 && !File.Exists(shot); i++) await Task.Delay(100);
                Kv(p + ".screenshot", File.Exists(shot) ? Path.GetFileName(shot) : "MISSING");
                M0Native.m0r_stop();
                Kv(p + ".reference_hr", Hx(M0Native.m0r_write_reference($"{pre}_seg{k}_ref.bgra", 0.5)));
                M0Native.m0r_start();
            }

            await Segment(0, scale0);
            double cur = scale0;
            for (int k = 1; k <= M3Switches; k++)
            {
                double want = Math.Abs(cur - 1.0) < 1e-6 ? 1.25 : 1.0;
                prompt!.Text = $"请把显示缩放改为 {want:P0}（设置 → 显示 → 缩放）。第 {k}/{M3Switches} 次切换";
                int seen; lock (scaleChanged) seen = scaleChanged.Count;
                var t0 = sw.Elapsed.TotalMilliseconds;
                while (sw.Elapsed.TotalMilliseconds - t0 < M3SwitchTimeoutMs)
                {
                    lock (scaleChanged) if (scaleChanged.Count > seen) break;
                    await Task.Delay(50);
                }
                (double ms, double scale) ev;
                lock (scaleChanged)
                {
                    if (scaleChanged.Count <= seen) { Kv($"switch{k}.detected", 0); Kv("verdict", "INVALID_NO_SWITCH"); return; }
                    ev = scaleChanged[^1];
                }
                M3Native.m0r_get_resize(out var before, out _, out _, out _, out _, out _);
                uint px = Px(ev.scale);
                int rhr = M3Native.m0r_resize(px, px);
                uint cnt = before, aw = 0, ah = 0; double lastMs = double.NaN; int lhr = 0;
                for (int i = 0; i < 100 && cnt == before; i++)
                {
                    await Task.Delay(20);
                    M3Native.m0r_get_resize(out cnt, out lastMs, out _, out aw, out ah, out lhr);
                }
                double eventToCall = sw.Elapsed.TotalMilliseconds - ev.ms;  // XAML event -> m0r_resize call (UI thread)
                Kv($"switch{k}.detected", 1);
                Kv($"switch{k}.want", F(want, 3));
                Kv($"switch{k}.got", F(ev.scale, 3));
                Kv($"switch{k}.resize_hr", Hx(rhr));
                Kv($"switch{k}.resize_applied_hr", Hx(lhr));
                Kv($"switch{k}.expected_px", px);
                Kv($"switch{k}.applied_px", $"{aw}x{ah}");
                Kv($"switch{k}.size_ok", cnt > before && aw == px && ah == px ? 1 : 0);
                Kv($"switch{k}.reraster_ms", F(cnt > before ? lastMs : double.NaN, 2));
                Kv($"switch{k}.event_to_resize_call_ms", F(eventToCall, 2));
                cur = ev.scale;
                await Segment(k, cur);
            }

            // Summary for the gate (G2 ①②③⑤⑦; ④ from segment counters; ⑥ is a visual check).
            M3Native.m0r_get_resize(out var nres, out _, out var maxMs, out _, out _, out _);
            Kv("switches", M3Switches);
            Kv("reraster_count", nres);
            Kv("reraster_max_ms", F(maxMs, 2));
            bool budget = segs.All(s => s.st.CpuP95 <= 0.35 * T && s.st.GpuP95 <= 0.35 * T
                                        && s.st.GpuSamplesDropped <= 0.10 * Math.Max(1, s.st.Frames));
            bool stable = segs.All(s => s.st.BeginDrawFailures == 0 && s.st.Frames >= 300);
            Kv("budget_ok", budget ? 1 : 0);
            Kv("stability_ok", stable ? 1 : 0);
            var at100 = segs.Where(s => Math.Abs(s.scale - 1.0) < 1e-6).ToList();
            Kv("private_mb_delta_100", at100.Count >= 2 ? (at100[^1].privMb - at100[0].privMb).ToString(CultureInfo.InvariantCulture) : "NaN");
            prompt!.Text = "M3-DPI 完成，可以把缩放恢复为 100%。";
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
            try { XamlRoot.Changed -= OnXamlRootChanged; } catch { }
            if (created)
            {
                M0Native.m0r_stop();
                M0Native.m0r_get_stats(out var fin);
                Kv("device_removed_hr", "0x" + fin.DeviceRemovedHr.ToString("X8", CultureInfo.InvariantCulture));
                M0Native.m0r_destroy();
            }
            if (sprite is not null) ElementCompositionPreview.SetElementChildVisual(RootGrid, null);
            if (prompt is not null) RootGrid.Children.Remove(prompt);
            raw.Dispose(); sum.Dispose();
            FlushLog();
            _suiteRunning = false;
            try { MeasureButton.IsEnabled = true; } catch { }
        }
    }
}
