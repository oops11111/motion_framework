// M3-MATRIX probe (docs/m3_matrix_gate_LOCKED.md). Two monitors with different refresh
// rates and scales: the probe moves its own window between them while the Rive surface
// keeps animating (no pointer injection), and measures per monitor segment the render-loop
// pacing, frame gaps, CPU/GPU budget and the surface's re-rasterisation at the new scale,
// plus the longest render-loop gap while crossing. composition.h is not used or touched.
// --m3-matrix-pacing (docs/m3_matrix_pacing_gate_LOCKED.md) runs the same flow with the
// render loop paced to the window's monitor (m0r_set_pacing_window), starts each steady
// segment only after the gate's transient window (>= 2.0 s and >= 120 frames after the
// move or scale change, whichever ends later), and judges pacing per monitor column:
// P95(|dt - T|) <= 0.25 T and share(|dt - T| > 0.5 T) <= 5 %.
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

internal static class M3MatrixNative
{
    [DllImport("m0rive")] internal static extern int m0r_get_intervals([Out] double[] buf, uint capacity, out uint count);
    [DllImport("m0rive")] internal static extern int m0r_set_pacing_window(IntPtr hwnd);
    [DllImport("m0rive")] internal static extern int m0r_get_pacing(out int vblank, out IntPtr monitor, out uint lookups);
    [DllImport("m0rive")] internal static extern int m0r_set_pacing_offset(double fraction);
    [DllImport("m0rive")] internal static extern int m0r_get_pacing_offset(out double lastMs);
    [DllImport("m0rive")] internal static extern int m0r_get_gpu_split(out double renderP50, out double renderP95, out double copyP50, out double copyP95);

    [StructLayout(LayoutKind.Sequential)] internal struct RECT { public int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct MONITORINFOEX
    {
        public int cbSize; public RECT rcMonitor, rcWork; public uint dwFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string szDevice;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmDeviceName;
        public ushort dmSpecVersion, dmDriverVersion, dmSize, dmDriverExtra;
        public uint dmFields;
        public int dmPositionX, dmPositionY;
        public uint dmDisplayOrientation, dmDisplayFixedOutput;
        public short dmColor, dmDuplex, dmYResolution, dmTTOption, dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmFormName;
        public ushort dmLogPixels;
        public uint dmBitsPerPel, dmPelsWidth, dmPelsHeight, dmDisplayFlags, dmDisplayFrequency;
        public uint dmICMMethod, dmICMIntent, dmMediaType, dmDitherType, dmReserved1, dmReserved2, dmPanningWidth, dmPanningHeight;
    }
    internal delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdc, ref RECT rc, IntPtr data);
    [DllImport("user32.dll")] internal static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr clip, MonitorEnumProc proc, IntPtr data);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] internal static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX info);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] internal static extern bool EnumDisplaySettings(string device, int mode, ref DEVMODE dm);
    [DllImport("user32.dll")] internal static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint flags);
    [DllImport("user32.dll")] internal static extern bool GetWindowRect(IntPtr hwnd, out RECT r);
    [DllImport("user32.dll")] internal static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int cx, int cy, uint flags);
    [DllImport("shcore.dll")] internal static extern int GetDpiForMonitor(IntPtr hMonitor, int type, out uint dpiX, out uint dpiY);
    internal const uint SWP_NOSIZE = 0x0001, SWP_NOZORDER = 0x0004, SWP_NOACTIVATE = 0x0010;

    internal sealed record Monitor(IntPtr Handle, string Device, RECT Bounds, RECT Work, double Hz, double Scale, bool Primary);

    internal static List<Monitor> Monitors()
    {
        var list = new List<Monitor>();
        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr h, IntPtr hdcUnused, ref RECT rcUnused, IntPtr dataUnused) =>
        {
            var mi = new MONITORINFOEX { cbSize = Marshal.SizeOf<MONITORINFOEX>() };
            if (!GetMonitorInfo(h, ref mi)) return true;
            var dm = new DEVMODE { dmSize = (ushort)Marshal.SizeOf<DEVMODE>() };
            double hz = EnumDisplaySettings(mi.szDevice, -1, ref dm) ? dm.dmDisplayFrequency : 0;  // ENUM_CURRENT_SETTINGS
            double scale = GetDpiForMonitor(h, 0, out var dx, out _) == 0 ? dx / 96.0 : double.NaN;  // MDT_EFFECTIVE_DPI
            list.Add(new Monitor(h, mi.szDevice, mi.rcMonitor, mi.rcWork, hz, scale, (mi.dwFlags & 1) != 0));
            return true;
        }, IntPtr.Zero);
        return list;
    }
}

public sealed partial class MainPage
{
    const double M3MxDip = 400;          // Rive surface edge in DIP
    const int M3MxSegmentMs = 8000;      // >= 480 frames per segment at 60 Hz
    const int M3MxCrossings = 4;         // drag, jump, drag, jump (G2)
    const int M3MxDragStepPx = 40;       // animated move: 40 px per 16 ms (~2.5 px/ms, a brisk drag)

    private async Task RunM3MatrixProbe(bool pacingMode = false)
    {
        if (_suiteRunning || _comp is null || XamlRoot is null) return;
        _suiteRunning = true;
        try { MeasureButton.IsEnabled = false; } catch { }

        // Arguments first: the lab found the runner's environment did not reach the app (M3-MATRIX lab note 1).
        string artDir = M3Arg("--m3-artdir") ?? Environment.GetEnvironmentVariable("MF_M0_ARTDIR") ?? @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string sess = M3Arg("--m3-session") ?? Environment.GetEnvironmentVariable("MF_M3_SESSION") ?? "1";
        string rivPath = Environment.GetEnvironmentVariable("MF_M0_RIV") ?? Path.Combine(artDir, "m0_assets", "Knight_square_2.riv");
        string pre = Path.Combine(artDir, pacingMode ? $"m3matrixpacing_s{sess}" : $"m3mx_s{sess}");
        var raw = new StreamWriter(pre + "_raw.txt", false, new UTF8Encoding(false));
        var sum = new StreamWriter(new FileStream(pre + "_summary.txt", FileMode.Create, FileAccess.Write, FileShare.ReadWrite), new UTF8Encoding(false));
        bool verdictWritten = false;  // the runner waits for a verdict= line
        void Raw(string s) { Log(s); raw.WriteLine(s); raw.Flush(); }
        void Kv(string k, object v) { string s = $"{k}={Convert.ToString(v, CultureInfo.InvariantCulture)}"; Raw("KV " + s); sum.WriteLine(s); sum.Flush(); if (k == "verdict") verdictWritten = true; }
        static string Hx(int hr) => "0x" + hr.ToString("X8", CultureInfo.InvariantCulture);
        static uint Px(double scale) => (uint)Math.Round(M3MxDip * scale);

        SpriteVisual? sprite = null;
        TextBlock? prompt = null;
        bool created = false;
        var scaleEvents = new List<(double ms, double scale)>();
        var sw = Stopwatch.StartNew();
        double lastScale = XamlRoot.RasterizationScale;
        void OnXamlRootChanged(XamlRoot r, XamlRootChangedEventArgs a)
        {
            double sc = r.RasterizationScale;
            if (Math.Abs(sc - lastScale) < 1e-6) return;
            lastScale = sc;
            lock (scaleEvents) scaleEvents.Add((sw.Elapsed.TotalMilliseconds, sc));
            M3Native.m0r_resize(Px(sc), Px(sc));  // same path as M3-RIVE-DPI
        }
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            ShowWindow(hwnd, 9); SetForegroundWindow(hwnd);
            Kv("probe", pacingMode ? "M3-MATRIX-PACING v4" : "M3-MATRIX v1");
            Kv("session", sess);
            Kv("pid", Environment.ProcessId);
            Kv("host", Environment.MachineName);
            Kv("os", Environment.OSVersion.VersionString);
            var mons = M3MatrixNative.Monitors();
            Kv("monitors", mons.Count);
            for (int i = 0; i < mons.Count; i++)
            {
                var m = mons[i];
                Kv($"monitor{i}", $"{m.Device} {m.Bounds.Right - m.Bounds.Left}x{m.Bounds.Bottom - m.Bounds.Top}@{m.Bounds.Left},{m.Bounds.Top} hz={F(m.Hz, 0)} scale={F(m.Scale, 2)} primary={(m.Primary ? 1 : 0)}");
            }
            if (mons.Count != 2) { Kv("verdict", "BLOCKED_HARDWARE"); return; }  // X1: exactly two monitors
            var home = mons.First(m => m.Primary);
            var other = mons.First(m => !m.Primary);
            Kv("mixed_refresh", Math.Abs(home.Hz - other.Hz) >= 1 ? 1 : 0);
            Kv("mixed_scale", Math.Abs(home.Scale - other.Scale) >= 0.01 ? 1 : 0);

            // Rive surface (as M3-RIVE-DPI); sized for the monitor the window starts on.
            double scale0 = XamlRoot.RasterizationScale;
            int hr = M0Native.m0r_create(rivPath, Px(scale0), Px(scale0), out var dev);
            Kv("create_hr", Hx(hr));
            if (hr < 0) { Kv("verdict", "INVALID_CREATE"); return; }
            created = true;
            M3LostNative.m0r_set_abort_note(pre + "_summary.txt");
            var gdHr = _comp.As<IM0CompositorInterop>().CreateGraphicsDevice(dev, out var gdPtr);
            Marshal.Release(dev);
            if (gdHr < 0) { Kv("graphics_device_hr", Hx(gdHr)); Kv("verdict", "INVALID_GRAPHICS_DEVICE"); return; }
            var gd = CompositionGraphicsDevice.FromAbi(gdPtr);
            Marshal.Release(gdPtr);
            M3LostNative.m0r_attach_graphics_device(((IWinRTObject)gd).NativeObject.ThisPtr);
            var surface = gd.CreateDrawingSurface2(new Windows.Graphics.SizeInt32 { Width = (int)Px(scale0), Height = (int)Px(scale0) },
                DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
            hr = M0Native.m0r_attach_surface(((IWinRTObject)surface).NativeObject.ThisPtr);
            if (hr < 0) { Kv("surface_interop_qi_hr", Hx(hr)); Kv("verdict", "INVALID_SURFACE_INTEROP"); return; }
            var boxAt = AnimBox.TransformToVisual(RootGrid).TransformPoint(new Windows.Foundation.Point(0, 0));
            sprite = _comp.CreateSpriteVisual();
            sprite.Size = new Vector2((float)M3MxDip, (float)M3MxDip);
            sprite.Offset = new Vector3((float)boxAt.X, (float)boxAt.Y, 0);
            sprite.Brush = _comp.CreateSurfaceBrush(surface);
            ElementCompositionPreview.SetElementChildVisual(RootGrid, sprite);
            prompt = new TextBlock { FontSize = 24, Margin = new Thickness(24, 200, 24, 0), TextWrapping = TextWrapping.Wrap, HorizontalAlignment = HorizontalAlignment.Right };
            RootGrid.Children.Add(prompt);
            XamlRoot.Changed += OnXamlRootChanged;
            Kv("start_hr", Hx(M0Native.m0r_start()));
            if (pacingMode)
            {
                Kv("pacing_window_hr", Hx(M3MatrixNative.m0r_set_pacing_window(hwnd)));
                Kv("pacing_offset_fraction", "0.30");  // r4 (W2): draw 0.3 T after the vblank
                Kv("pacing_offset_hr", Hx(M3MatrixNative.m0r_set_pacing_offset(0.30)));
            }
            // Steady-state |dt - T| samples per monitor column (pacing mode).
            var colDev = new Dictionary<bool, List<double>> { [true] = new(), [false] = new() };
            var colIv = new Dictionary<bool, List<double>> { [true] = new(), [false] = new() };
            double lastEventMs = 0;

            // Window placement helpers: top-left of a monitor's work area (+40 px margin).
            (int x, int y) Spot(M3MatrixNative.Monitor m) => (m.Work.Left + 40, m.Work.Top + 40);
            const uint flags = M3MatrixNative.SWP_NOSIZE | M3MatrixNative.SWP_NOZORDER | M3MatrixNative.SWP_NOACTIVATE;
            var startSpot = Spot(home);
            M3MatrixNative.SetWindowPos(hwnd, IntPtr.Zero, startSpot.x, startSpot.y, 0, 0, flags);
            await Task.Delay(1500);

            var buf = new double[20000];
            List<double> Intervals() { M3MatrixNative.m0r_get_intervals(buf, (uint)buf.Length, out var n); return buf.Take((int)Math.Min(n, (uint)buf.Length)).ToList(); }
            double Pct(List<double> v, double q) { var o = v.OrderBy(x => x).ToList(); return o.Count == 0 ? double.NaN : o[Math.Min(o.Count - 1, Math.Max(0, (int)Math.Ceiling(q * o.Count) - 1))]; }
            bool pacingOk = true, budgetOk = true, stableOk = true, sizeOk = true, hitchOk = true;
            async Task Segment(int k, M3MatrixNative.Monitor m)
            {
                var cur = M3MatrixNative.MonitorFromWindow(hwnd, 2);  // MONITOR_DEFAULTTONEAREST
                double T = m.Hz > 0 ? 1000.0 / m.Hz : double.NaN;
                prompt!.Text = $"M3-MATRIX 第 {k} 段：{(m.Primary ? "主" : "副")}显示器 {F(m.Hz, 0)}Hz / {F(m.Scale * 100, 0)}%";
                await Task.Delay(500);   // let a pending re-raster land before measuring
                if (pacingMode)
                {
                    // G2 ⑦: steady samples start only after the transient window.
                    double settle = Math.Max(2000, 120 * T);
                    double wait = lastEventMs + settle - sw.Elapsed.TotalMilliseconds;
                    if (wait > 0) await Task.Delay((int)Math.Ceiling(wait));
                }
                M0Native.m0r_reset_stats();
                ulong fid0 = 0; try { MfProbe.mf_dcomp_frame_id(2, out fid0); } catch { }
                await Task.Delay(M3MxSegmentMs);
                ulong fid1 = 0, period = 0; try { MfProbe.mf_dcomp_frame_id(2, out fid1); MfProbe.mf_dcomp_stats(fid1, out _, out _, out period, out _); } catch { }
                M0Native.m0r_get_stats(out var st);
                var iv = Intervals();
                double p50 = Pct(iv, 0.5);
                int drops = iv.Count(x => x > 1.5 * T);
                string p = $"seg{k}";
                Kv(p + ".monitor", m.Primary ? "primary" : "secondary");
                Kv(p + ".on_expected_monitor", cur == m.Handle ? 1 : 0);
                Kv(p + ".hz", F(m.Hz, 2));
                Kv(p + ".scale", F(XamlRoot!.RasterizationScale, 3));
                Kv(p + ".frames", st.Frames);
                Kv(p + ".draw_failures", st.BeginDrawFailures);
                Kv(p + ".interval_p50_ms", F(p50, 3));
                Kv(p + ".interval_p50_over_T", F(p50 / T, 3));
                Kv(p + ".interval_max_ms", F(iv.Count == 0 ? double.NaN : iv.Max(), 2));
                Kv(p + ".drops_over_1_5T", drops);
                Kv(p + ".drop_rate", F(iv.Count == 0 ? double.NaN : (double)drops / iv.Count, 4));
                Kv(p + ".cpu_p95_frac_T", F(st.CpuP95 / T, 3));
                Kv(p + ".gpu_p95_frac_T", F(st.GpuP95 / T, 3));
                Kv(p + ".gpu_samples_dropped", st.GpuSamplesDropped);
                Kv(p + ".dcomp_frames", fid1 >= fid0 ? (fid1 - fid0).ToString(CultureInfo.InvariantCulture) : "NaN");   // record only
                Kv(p + ".dcomp_frame_period_raw", period);                                                               // record only (units as reported)
                if (pacingMode)
                {
                    var devs = iv.Select(x => Math.Abs(x - T)).ToList();
                    colDev[m.Primary].AddRange(devs);
                    colIv[m.Primary].AddRange(iv);
                    M3MatrixNative.m0r_get_pacing(out var vb, out var pmon, out var looks);
                    Kv(p + ".pacing_source", vb == 1 ? "vblank" : "dwmflush");
                    Kv(p + ".pacing_monitor_matches", pmon == m.Handle ? 1 : 0);
                    Kv(p + ".interval_p95_ms", F(Pct(iv, 0.95), 3));
                    Kv(p + ".dev_p95_ms", F(Pct(devs, 0.95), 3));
                    Kv(p + ".dev_over_half_T_share", F(devs.Count == 0 ? double.NaN : devs.Count(x => x > 0.5 * T) / (double)devs.Count, 4));
                    Kv(p + ".pacing_lookups", looks);
                    M3MatrixNative.m0r_get_pacing_offset(out var offMs);
                    Kv(p + ".pacing_offset_last_ms", F(offMs, 3));
                    Kv(p + ".pacing_offset_last_over_T", F(offMs / T, 3));
                    pacingOk &= cur == m.Handle;
                }
                else
                    pacingOk &= cur == m.Handle && Math.Abs(p50 / T - 1) <= 0.10 && drops <= 0.01 * Math.Max(1, iv.Count);
                M3MatrixNative.m0r_get_gpu_split(out var gr50, out var gr95, out var gc50, out var gc95);
                Kv(p + ".gpu_render_p95_frac_T", F(gr95 / T, 3));
                Kv(p + ".gpu_copy_p50_ms", F(gc50, 3));
                Kv(p + ".gpu_copy_p95_ms", F(gc95, 3));
                Kv(p + ".gpu_copy_p95_frac_T", F(gc95 / T, 3));
                // PACING r3 (V1): in pacing mode the GPU budget covers Rive's rendering only; the
                // copy into the surface, which can wait for DWM, is recorded above, not judged.
                double gpuJudged = pacingMode ? gr95 : st.GpuP95;
                budgetOk &= st.CpuP95 <= 0.35 * T && gpuJudged <= 0.35 * T && st.GpuSamplesDropped <= 0.10 * Math.Max(1, st.Frames);
                stableOk &= st.BeginDrawFailures == 0 && st.Frames >= 300;
                // Surface size must follow the monitor's scale (M3-RIVE-DPI path).
                M3Native.m0r_get_resize(out _, out _, out _, out var aw, out var ah, out _);
                uint want = Px(XamlRoot.RasterizationScale);
                bool sOk = (aw == 0 && want == Px(scale0)) || (aw == want && ah == want);
                Kv(p + ".surface_px", aw == 0 ? $"{Px(scale0)} (initial)" : $"{aw}x{ah}");
                Kv(p + ".size_ok", sOk ? 1 : 0);
                sizeOk &= sOk;
                string shot = $"{pre}_seg{k}.png";
                File.WriteAllText($"{pre}_seg{k}.shotreq", shot);
                for (int i = 0; i < 50 && !File.Exists(shot); i++) await Task.Delay(100);
                Kv(p + ".screenshot", File.Exists(shot) ? Path.GetFileName(shot) : "MISSING");
            }

            // Crossing: animated move ("drag") or a single jump; the longest render-loop
            // interval from the start of the move until 1 s after it is the hitch.
            async Task Cross(int k, M3MatrixNative.Monitor to, bool animated)
            {
                prompt!.Text = $"M3-MATRIX 跨屏 {k}/{M3MxCrossings}（{(animated ? "拖动" : "跳转")}）";
                int seen; lock (scaleEvents) seen = scaleEvents.Count;
                M0Native.m0r_reset_stats();
                M3MatrixNative.GetWindowRect(hwnd, out var r);
                var target = Spot(to);
                var t0 = sw.Elapsed.TotalMilliseconds;
                if (animated)
                {
                    int steps = Math.Max(1, (int)Math.Ceiling(Math.Abs(target.x - r.Left) / (double)M3MxDragStepPx));
                    for (int i = 1; i <= steps; i++)
                    {
                        int x = r.Left + (int)Math.Round((target.x - r.Left) * (double)i / steps);
                        int y = r.Top + (int)Math.Round((target.y - r.Top) * (double)i / steps);
                        M3MatrixNative.SetWindowPos(hwnd, IntPtr.Zero, x, y, 0, 0, flags);
                        await Task.Delay(16);
                    }
                }
                else M3MatrixNative.SetWindowPos(hwnd, IntPtr.Zero, target.x, target.y, 0, 0, flags);
                var moveMs = sw.Elapsed.TotalMilliseconds - t0;
                await Task.Delay(1000);
                lock (scaleEvents) lastEventMs = Math.Max(t0 + moveMs, scaleEvents.Count > seen ? scaleEvents[^1].ms : 0);
                var iv = Intervals();
                double gap = iv.Count == 0 ? double.NaN : iv.Max();
                double scaleMs = double.NaN;
                lock (scaleEvents) if (scaleEvents.Count > seen) scaleMs = scaleEvents[seen].ms - t0;
                M3Native.m0r_get_resize(out _, out var lastRe, out _, out _, out _, out var rhr);
                string p = $"cross{k}";
                Kv(p + ".kind", animated ? "drag" : "jump");
                Kv(p + ".to", to.Primary ? "primary" : "secondary");
                Kv(p + ".move_ms", F(moveMs, 1));
                Kv(p + ".max_interval_ms", F(gap, 2));
                Kv(p + ".scale_change_ms", F(scaleMs, 1));
                Kv(p + ".reraster_last_ms", F(lastRe, 2));
                Kv(p + ".resize_hr", Hx(rhr));
                Kv(p + ".draw_failures", M0Native.m0r_get_stats(out var cst) >= 0 ? cst.BeginDrawFailures : 0);
                hitchOk &= gap <= 100;   // G2 ③: no visible stall
            }

            await Segment(0, home);
            var mon = new[] { other, home, other, home };
            for (int k = 1; k <= M3MxCrossings; k++)
            {
                await Cross(k, mon[k - 1], animated: k % 2 == 1);
                await Segment(k, mon[k - 1]);
            }
            if (pacingMode)
            {
                // G2 ②: each column passes on its steady samples; pacing_ok = both columns.
                foreach (var (primary, m) in new[] { (true, home), (false, other) })
                {
                    double T = 1000.0 / m.Hz;
                    var cdev = colDev[primary];
                    string c = primary ? "col_primary" : "col_external";
                    double p95 = Pct(cdev, 0.95), share = cdev.Count == 0 ? double.NaN : cdev.Count(x => x > 0.5 * T) / (double)cdev.Count;
                    bool ok = cdev.Count > 0 && p95 <= 0.25 * T && share <= 0.05;
                    Kv(c + ".hz", F(m.Hz, 2));
                    Kv(c + ".samples", cdev.Count);
                    Kv(c + ".interval_p50_ms", F(Pct(colIv[primary], 0.5), 3));
                    Kv(c + ".interval_p95_ms", F(Pct(colIv[primary], 0.95), 3));
                    Kv(c + ".interval_max_ms", F(colIv[primary].Count == 0 ? double.NaN : colIv[primary].Max(), 3));
                    Kv(c + ".dev_p95_ms", F(p95, 3));
                    Kv(c + ".dev_p95_over_T", F(p95 / T, 3));
                    Kv(c + ".dev_over_half_T_share", F(share, 4));
                    Kv(c + ".ok", ok ? 1 : 0);
                    pacingOk &= ok;
                }
            }
            Kv("pacing_ok", pacingOk ? 1 : 0);
            Kv("budget_ok", budgetOk ? 1 : 0);
            Kv("stability_ok", stableOk ? 1 : 0);
            Kv("size_ok", sizeOk ? 1 : 0);
            Kv("hitch_ok", hitchOk ? 1 : 0);
            lock (scaleEvents) Kv("scale_changes", scaleEvents.Count);
            prompt!.Text = "M3-MATRIX 本会话完成。";
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
            try { XamlRoot!.Changed -= OnXamlRootChanged; } catch { }
            if (created) { M0Native.m0r_stop(); M0Native.m0r_destroy(); }
            if (sprite is not null) ElementCompositionPreview.SetElementChildVisual(RootGrid, null);
            if (prompt is not null) RootGrid.Children.Remove(prompt);
            raw.Dispose(); sum.Dispose();
            FlushLog();
            _suiteRunning = false;
            try { MeasureButton.IsEnabled = true; } catch { }
        }
    }
}
