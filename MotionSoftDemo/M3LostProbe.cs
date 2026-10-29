// M3-RIVE-LOST probe (docs/m3_rive_lost_gate_r2_LOCKED.md). The Rive surface (M3 F1:
// drawing-surface path) renders at a fixed size while the runner restarts the display
// driver (pnputil /restart-device, r2 L1) when the probe asks for it. The DLL's
// render thread detects the loss and recovers on its own (new device, Rive context,
// re-imported .riv, SetRenderingDevice); the probe only records what it reports and
// measures a segment after each recovery. --m3-lost-sim raises the loss in-process
// instead (local smoke test only; its verdict is SIM_RECORDED and never counts).
// No pointer injection. composition.h is not used or touched.
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
using WinRT;

namespace MotionSoftDemo;

internal static class M3LostNative
{
    const string Dll = "m0rive";

    [StructLayout(LayoutKind.Sequential)]
    internal struct LostEvent
    {
        public int RemovedReason, Simulated, RecoverHr;
        public uint Attempts;
        public double RecoverMs, GapMs;
        public ulong FailuresDuring;
        public uint OldDeviceRefs, SehCaught;
    }

    [DllImport(Dll)] internal static extern int m0r_attach_graphics_device(IntPtr graphicsDeviceUnknown);
    [DllImport(Dll)] internal static extern int m0r_get_lost(out uint detected, out uint recovered, out int lostNow,
                                                           [Out] LostEvent[] events, uint capacity);
    [DllImport(Dll)] internal static extern int m0r_simulate_lost();
    [DllImport(Dll, CharSet = CharSet.Unicode)] internal static extern int m0r_set_abort_note(string path);
}

public sealed partial class MainPage
{
    const double M3LostDip = 400;            // surface edge in DIP
    const int M3LostSegmentMs = 8000;        // >= 300 frames per segment at 60-75 Hz
    const int M3LostInjections = 2;          // per session (K4)
    const int M3LostWaitInjectMs = 300000;   // runner spacing (>= 45 s) and session cooldown fit in 5 min
    const int M3LostWaitRecoverMs = 30000;   // keep waiting past the 5 s ceiling to log what happened
    const double M3LostCeilingMs = 5000;     // G2 ②: a slower recovery fails
    const int M3LostSettleMs = 500;          // after recovery, before the measured segment

    private async Task RunM3LostProbe(bool simulate)
    {
        if (_suiteRunning || _comp is null || XamlRoot is null) return;
        _suiteRunning = true;
        try { MeasureButton.IsEnabled = false; } catch { }

        string artDir = Environment.GetEnvironmentVariable("MF_M0_ARTDIR") ?? @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string sess = Environment.GetEnvironmentVariable("MF_M3_SESSION") ?? "1";
        string rivPath = Environment.GetEnvironmentVariable("MF_M0_RIV") ?? Path.Combine(artDir, "m0_assets", "Knight_square_2.riv");
        string pre = Path.Combine(artDir, simulate ? $"m3lostsim_s{sess}" : $"m3lost_s{sess}");
        var raw = new StreamWriter(pre + "_raw.txt", false, new UTF8Encoding(false));
        // Shared for writing: the DLL's abort handler appends to this file (m0r_set_abort_note).
        var sum = new StreamWriter(new FileStream(pre + "_summary.txt", FileMode.Create, FileAccess.Write, FileShare.ReadWrite),
                                   new UTF8Encoding(false));
        bool verdictWritten = false;  // the runner waits for a verdict= line
        void Raw(string s) { Log(s); raw.WriteLine(s); raw.Flush(); }
        void Kv(string k, object v) { string s = $"{k}={Convert.ToString(v, CultureInfo.InvariantCulture)}"; Raw("KV " + s); sum.WriteLine(s); sum.Flush(); if (k == "verdict") verdictWritten = true; }
        static string Hx(int hr) => "0x" + hr.ToString("X8", CultureInfo.InvariantCulture);

        SpriteVisual? sprite = null;
        TextBlock? prompt = null;
        CompositionGraphicsDevice? gd = null;
        bool created = false;
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            ShowWindow(hwnd, 9); SetForegroundWindow(hwnd);
            double scale = XamlRoot.RasterizationScale;
            uint px = (uint)Math.Round(M3LostDip * scale);
            double refreshHz = 0;
            try { MfProbe.mf_dwm_timing(out _, out _, out _, out _, out var rn, out var rd, out _); if (rd > 0) refreshHz = (double)rn / rd; } catch { }
            double T = refreshHz > 0 ? 1000.0 / refreshHz : double.NaN;
            Kv("probe", "M3-LOST v2");
            Kv("mode", simulate ? "simulated" : "pnputil-restart");
            Kv("session", sess);
            Kv("pid", Environment.ProcessId);
            Kv("host", Environment.MachineName);
            Kv("os", Environment.OSVersion.VersionString);
            Kv("refresh_hz", F(refreshHz, 3));
            Kv("scale", F(scale, 3));
            Kv("surface_px", px);
            Kv("resume_mode", "restart");  // K6

            int hr = M0Native.m0r_create(rivPath, px, px, out var dev);
            Kv("create_hr", Hx(hr));
            if (hr < 0) { Kv("verdict", "INVALID_CREATE"); return; }
            created = true;
            // Rive aborts the process on a failed D3D call; the DLL then appends the verdict here.
            M3LostNative.m0r_set_abort_note(pre + "_summary.txt");
            var gdHr = _comp.As<IM0CompositorInterop>().CreateGraphicsDevice(dev, out var gdPtr);
            Marshal.Release(dev);
            Kv("graphics_device_hr", Hx(gdHr));
            if (gdHr < 0) { Kv("verdict", "INVALID_GRAPHICS_DEVICE"); return; }
            gd = CompositionGraphicsDevice.FromAbi(gdPtr);
            Marshal.Release(gdPtr);
            hr = M3LostNative.m0r_attach_graphics_device(((IWinRTObject)gd).NativeObject.ThisPtr);
            Kv("graphics_device_interop_qi_hr", Hx(hr));
            if (hr < 0) { Kv("verdict", "INVALID_GRAPHICS_DEVICE_INTEROP"); return; }
            var surface = gd.CreateDrawingSurface2(new Windows.Graphics.SizeInt32 { Width = (int)px, Height = (int)px },
                DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
            hr = M0Native.m0r_attach_surface(((IWinRTObject)surface).NativeObject.ThisPtr);
            Kv("surface_interop_qi_hr", Hx(hr));
            if (hr < 0) { Kv("verdict", "INVALID_SURFACE_INTEROP"); return; }

            var boxAt = AnimBox.TransformToVisual(RootGrid).TransformPoint(new Windows.Foundation.Point(0, 0));
            sprite = _comp.CreateSpriteVisual();
            sprite.Size = new Vector2((float)M3LostDip, (float)M3LostDip);
            sprite.Offset = new Vector3((float)boxAt.X, (float)boxAt.Y, 0);
            sprite.Brush = _comp.CreateSurfaceBrush(surface);
            ElementCompositionPreview.SetElementChildVisual(RootGrid, sprite);
            prompt = new TextBlock { FontSize = 28, Margin = new Thickness(24, 200, 24, 0), TextWrapping = TextWrapping.Wrap };
            RootGrid.Children.Add(prompt);
            Kv("start_hr", Hx(M0Native.m0r_start()));

            var segs = new List<(M0Native.Stats st, long privMb)>();
            async Task Segment(int k)
            {
                prompt!.Text = $"M3-LOST 第 {k} 段：请勿操作（约 {M3LostSegmentMs / 1000} 秒）";
                M0Native.m0r_reset_stats();
                await Task.Delay(M3LostSegmentMs);
                M0Native.m0r_get_stats(out var st);
                var proc = Process.GetCurrentProcess(); proc.Refresh();
                long privMb = proc.PrivateMemorySize64 / (1024 * 1024);
                segs.Add((st, privMb));
                string p = $"seg{k}";
                Kv(p + ".frames", st.Frames);
                Kv(p + ".draw_failures", st.BeginDrawFailures);
                Kv(p + ".device_removed_hr", Hx(st.DeviceRemovedHr));
                Kv(p + ".cpu_p95_ms", F(st.CpuP95, 3));
                Kv(p + ".cpu_p95_frac_T", F(st.CpuP95 / T, 3));
                Kv(p + ".gpu_p95_ms", F(st.GpuP95, 3));
                Kv(p + ".gpu_p95_frac_T", F(st.GpuP95 / T, 3));
                Kv(p + ".gpu_samples_dropped", st.GpuSamplesDropped);
                Kv(p + ".private_mb", privMb);
                // Window screenshot (runner) and a reference rendered on the current device.
                string shot = $"{pre}_seg{k}.png";
                File.WriteAllText($"{pre}_seg{k}.shotreq", shot);
                for (int i = 0; i < 50 && !File.Exists(shot); i++) await Task.Delay(100);
                Kv(p + ".screenshot", File.Exists(shot) ? Path.GetFileName(shot) : "MISSING");
                M0Native.m0r_stop();
                Kv(p + ".reference_hr", Hx(M0Native.m0r_write_reference($"{pre}_seg{k}_ref.bgra", 0.5)));
                M0Native.m0r_start();
            }

            await Segment(0);
            var events = new M3LostNative.LostEvent[16];
            var recoverMs = new List<double>();
            var missed = new List<long>();
            bool allRecovered = true;
            for (int k = 1; k <= M3LostInjections; k++)
            {
                M3LostNative.m0r_get_lost(out var det0, out var rec0, out _, events, 0);
                var sw = Stopwatch.StartNew();
                if (simulate)
                {
                    prompt!.Text = $"M3-LOST 模拟第 {k}/{M3LostInjections} 次设备丢失";
                    await Task.Delay(1000);
                    Kv($"inject{k}.simulate_hr", Hx(M3LostNative.m0r_simulate_lost()));
                }
                else
                {
                    prompt!.Text = $"等待第 {k}/{M3LostInjections} 次注入（runner 会重启显卡驱动，屏幕会黑几秒）。请勿操作。";
                    File.WriteAllText($"{pre}_inject{k}.req", DateTime.Now.ToString("o", CultureInfo.InvariantCulture));
                }
                uint det = det0, rec = rec0; int lostNow = 0;
                while (det == det0 && sw.ElapsedMilliseconds < M3LostWaitInjectMs)
                {
                    await Task.Delay(20);
                    M3LostNative.m0r_get_lost(out det, out rec, out lostNow, events, 0);
                }
                if (det == det0)
                {
                    Kv($"inject{k}.detected", 0);
                    Kv("verdict", "INVALID_NO_LOSS");
                    return;
                }
                Kv($"inject{k}.detected", 1);  // the runner reads this line
                var sinceDetect = Stopwatch.StartNew();
                while (rec == rec0 && sinceDetect.ElapsedMilliseconds < M3LostWaitRecoverMs)
                {
                    await Task.Delay(20);
                    M3LostNative.m0r_get_lost(out det, out rec, out lostNow, events, (uint)events.Length);
                }
                M3LostNative.m0r_get_lost(out det, out rec, out lostNow, events, (uint)events.Length);
                string p = $"inject{k}";
                Kv(p + ".losses", det - det0);
                Kv(p + ".recovered", rec > rec0 ? 1 : 0);
                if (rec == rec0 || rec > events.Length)
                {
                    allRecovered = false;
                    Kv(p + ".recover_ok", 0);
                    Kv(p + ".lost_now", lostNow);
                    break;  // no device to measure on; the summary below still gets written
                }
                var ev = events[rec - 1];
                double framesMissed = T > 0 ? Math.Max(0, Math.Round(ev.GapMs / T) - 1) : double.NaN;
                bool ok = ev.RecoverMs <= M3LostCeilingMs;
                allRecovered &= ok;
                recoverMs.Add(ev.RecoverMs);
                missed.Add((long)framesMissed);
                Kv(p + ".removed_reason", Hx(ev.RemovedReason));
                Kv(p + ".simulated", ev.Simulated);
                Kv(p + ".recover_hr", Hx(ev.RecoverHr));
                Kv(p + ".attempts", ev.Attempts);
                Kv(p + ".recover_ms", F(ev.RecoverMs, 2));
                Kv(p + ".gap_ms", F(ev.GapMs, 2));
                Kv(p + ".frames_missed", F(framesMissed, 0));
                Kv(p + ".failures_during", ev.FailuresDuring);
                Kv(p + ".old_device_refs", ev.OldDeviceRefs);
                Kv(p + ".seh_caught", ev.SehCaught);
                Kv(p + ".recover_ok", ok ? 1 : 0);
                await Task.Delay(M3LostSettleMs);
                await Segment(k);
            }

            // Summary for the gate (G2 ①②③④⑥⑦; ⑤ is a visual check of the screenshots).
            M3LostNative.m0r_get_lost(out var detTotal, out var recTotal, out _, events, 0);
            Kv("injections", M3LostInjections);
            Kv("lost_detected", detTotal);
            Kv("recovered", recTotal);
            Kv("recover_ok", allRecovered && recoverMs.Count == M3LostInjections ? 1 : 0);
            Kv("recover_max_ms", recoverMs.Count > 0 ? F(recoverMs.Max(), 2) : "NaN");
            Kv("frames_missed", string.Join("/", missed));
            var after = segs.Skip(1).ToList();
            bool budget = after.Count > 0 && after.All(s => s.st.CpuP95 <= 0.35 * T && s.st.GpuP95 <= 0.35 * T
                                                           && s.st.GpuSamplesDropped <= 0.10 * Math.Max(1, s.st.Frames));
            bool stable = after.Count > 0 && after.All(s => s.st.BeginDrawFailures == 0 && s.st.DeviceRemovedHr == 0 && s.st.Frames >= 300);
            Kv("budget_ok", budget ? 1 : 0);
            Kv("stability_ok", stable ? 1 : 0);
            Kv("private_mb_delta", segs.Count >= 2 ? (segs[^1].privMb - segs[0].privMb).ToString(CultureInfo.InvariantCulture) : "NaN");
            prompt!.Text = "M3-LOST 本会话完成。";
            Kv("verdict", simulate ? "SIM_RECORDED" : "RECORDED");
        }
        catch (Exception ex)
        {
            Kv("exception", ex.GetType().Name + ": " + ex.Message.Replace('\n', ' '));
            Kv("verdict", "INVALID_EXCEPTION");
        }
        finally
        {
            if (!verdictWritten) Kv("verdict", "INVALID_INCOMPLETE");
            if (created)
            {
                M0Native.m0r_stop();
                M0Native.m0r_destroy();
            }
            if (sprite is not null) ElementCompositionPreview.SetElementChildVisual(RootGrid, null);
            if (prompt is not null) RootGrid.Children.Remove(prompt);
            GC.KeepAlive(gd);
            raw.Dispose(); sum.Dispose();
            FlushLog();
            _suiteRunning = false;
            try { MeasureButton.IsEnabled = true; } catch { }
        }
    }
}
