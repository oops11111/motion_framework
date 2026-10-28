// M0 technical-validation probe (docs/m0_tech_validation_gate_DRAFT.md).
//   M0-1a Rive -> CompositionDrawingSurface      (m0rive.dll, ICompositorInterop)
//   M0-1b Rive -> composition swapchain          (ICompositorSwapChainInterop)
//   M0-2  drag misalignment, surface vs element  (same pointer expression; WGC)
//   M0-3/5 pointer -> screen latency             (pointer PropertySet expression vs UI-thread write; WGC)
// M0-2 / M0-3 / M0-5 move the real cursor with SendInput and run only when
// MF_M0_INJECT=1 (gate decision D1). Without it they are logged as SKIP.
// composition.h is not used or touched; no shadow IV; no spring/retarget code.
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
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using WinRT;

namespace MotionSoftDemo;

internal static class M0Native
{
    const string Dll = "m0rive";

    [StructLayout(LayoutKind.Sequential)]
    internal struct Stats
    {
        public ulong Frames, BeginDrawFailures;
        public double CpuP50, CpuP95, CpuMax, GpuP50, GpuP95, IntervalP50, IntervalP95;
        public int LastHr, InteropQiHr, OffscreenHasUav, DeviceRemovedHr;
        public ulong GpuSamplesDropped;
    }

    [DllImport(Dll, CharSet = CharSet.Unicode)] internal static extern int m0r_create(string rivPath, uint w, uint h, out IntPtr device);
    [DllImport(Dll)] internal static extern int m0r_attach_surface(IntPtr surfaceUnknown);
    [DllImport(Dll)] internal static extern int m0r_create_swapchain(out IntPtr swapchain);
    [DllImport(Dll)] internal static extern int m0r_start();
    [DllImport(Dll)] internal static extern int m0r_set_marker(uint x, uint y, uint w, uint h, byte r, byte g, byte b);
    [DllImport(Dll)] internal static extern int m0r_stop();
    [DllImport(Dll)] internal static extern int m0r_get_stats(out Stats s);
    [DllImport(Dll, CharSet = CharSet.Unicode)] internal static extern int m0r_write_reference(string path, double tSec);
    [DllImport(Dll)] internal static extern int m0r_reset_stats();
    [DllImport(Dll)] internal static extern void m0r_destroy();

    // SendInput (x64 layout: 4-byte type, 4 padding, 32-byte MOUSEINPUT union arm).
    [StructLayout(LayoutKind.Sequential)]
    internal struct MOUSEINPUT { public int dx, dy; public uint mouseData, dwFlags, time; public IntPtr dwExtraInfo; }
    [StructLayout(LayoutKind.Sequential)]
    internal struct INPUT { public uint type; public MOUSEINPUT mi; }
    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT { public int X, Y; }
    [DllImport("user32.dll", SetLastError = true)] internal static extern uint SendInput(uint n, INPUT[] inputs, int size);
    [DllImport("user32.dll")] internal static extern bool ClientToScreen(IntPtr hwnd, ref POINT p);
    [DllImport("user32.dll")] internal static extern bool GetCursorPos(out POINT p);
    [DllImport("user32.dll")] internal static extern int GetSystemMetrics(int index);

    /// Moves the cursor to absolute screen pixel (x, y) on the primary monitor.
    internal static uint MoveCursor(int x, int y)
    {
        int w = GetSystemMetrics(0), h = GetSystemMetrics(1);  // SM_CXSCREEN, SM_CYSCREEN
        var i = new INPUT
        {
            type = 0,  // INPUT_MOUSE
            mi = new MOUSEINPUT
            {
                dx = (int)Math.Round(x * 65535.0 / Math.Max(1, w - 1)),
                dy = (int)Math.Round(y * 65535.0 / Math.Max(1, h - 1)),
                dwFlags = 0x0001 | 0x8000,  // MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE
            },
        };
        return SendInput(1, new[] { i }, Marshal.SizeOf<INPUT>());
    }
}

// Interop interfaces of Microsoft.UI.Composition (not Windows.UI.Composition).
// IIDs and vtable order from Microsoft.UI.Composition.Interop.h in
// Microsoft.WindowsAppSDK.InteractiveExperiences 2.1.9 (WindowsAppSDK 2.5.1).
[ComImport, Guid("FAB19398-6D19-4D8A-B752-8F096C396069"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IM0CompositorInterop
{
    [PreserveSig] int CreateGraphicsDevice(IntPtr renderingDevice, out IntPtr result);
}

[ComImport, Guid("FC084699-67D8-40E1-ADE7-08901D84FFDA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IM0CompositorSwapChainInterop
{
    [PreserveSig] int CreateGraphicsDevice(IntPtr renderingDevice, out IntPtr result);  // inherited slot
    [PreserveSig] int CreateCompositionSurfaceForHandle(IntPtr swapChainHandle, out IntPtr result);
    [PreserveSig] int CreateCompositionSurfaceForSwapChain(IntPtr swapChain, out IntPtr result);
}

public sealed partial class MainPage
{
    const uint M0SurfacePx = 400;          // Rive surface edge, physical px
    const int M0PhaseMs = 20000;           // >= 1000 frames at 60 Hz
    const int M0Steps = 20;                // latency steps per path
    const int M0StepDip = 120;             // step / drag amplitude
    const int M0StepGapMs = 450;
    const int M0Drags = 40;                // drags per M0-2 path; M0-B saw ~10 usable WGC frames per drag (WGC ~37 fps at 75 Hz), so 16 gave ~160 < 200 samples
    const int M0DragMs = 300;              // one drag of M0StepDip at constant speed

    private async Task RunM0Probe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true;
        try { MeasureButton.IsEnabled = false; } catch { }

        string artDir = Environment.GetEnvironmentVariable("MF_M0_ARTDIR") ?? @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string sess = Environment.GetEnvironmentVariable("MF_M0_SESSION") ?? "1";
        string rivPath = Environment.GetEnvironmentVariable("MF_M0_RIV") ?? Path.Combine(artDir, "m0_assets", "Knight_square_2.riv");
        bool inject = Environment.GetEnvironmentVariable("MF_M0_INJECT") == "1";
        var raw = new StreamWriter(Path.Combine(artDir, $"m0_s{sess}_raw.txt"), false, new UTF8Encoding(false));
        var sum = new StreamWriter(Path.Combine(artDir, $"m0_s{sess}_summary.txt"), false, new UTF8Encoding(false));
        void Raw(string s) { Log(s); raw.WriteLine(s); raw.Flush(); }
        bool verdictWritten = false;  // run_m0.ps1 waits for a verdict= line
        void Kv(string k, object v) { string s = $"{k}={Convert.ToString(v, CultureInfo.InvariantCulture)}"; Raw("KV " + s); sum.WriteLine(s); sum.Flush(); if (k == "verdict") verdictWritten = true; }
        static string Hx(int hr) => "0x" + hr.ToString("X8", CultureInfo.InvariantCulture);

        SpriteVisual? sprite = null;
        bool created = false;
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            ShowWindow(hwnd, 9); SetForegroundWindow(hwnd);
            double scale = XamlRoot?.RasterizationScale ?? 1.0;
            double refreshHz = 0;
            try { MfProbe.mf_dwm_timing(out _, out _, out _, out _, out var rn, out var rd, out _); if (rd > 0) refreshHz = (double)rn / rd; } catch { }
            Kv("probe", "M0 v3");
            Kv("session", sess);
            Kv("pid", Environment.ProcessId);
            Kv("host", Environment.MachineName);
            Kv("os", Environment.OSVersion.VersionString);
            Kv("rasterization_scale", F(scale, 3));
            Kv("refresh_hz", F(refreshHz, 3));
            Kv("riv", rivPath);
            Kv("inject_approved", inject ? 1 : 0);
            if (!_compMode) { _toggleFromCode = true; ModeToggle.IsOn = true; _toggleFromCode = false; SetMode(true, "m0 probe"); }
            CompReset(0, "m0 rest");

            // ---- M0-1a: Rive -> CompositionDrawingSurface ----------------------------
            int hr = M0Native.m0r_create(rivPath, M0SurfacePx, M0SurfacePx, out var dev);
            Kv("m0_1.create_hr", Hx(hr));
            if (hr < 0) { Kv("m0_1.verdict", "FAIL_CREATE"); Kv("verdict", "M0_1_FAIL_CREATE"); return; }
            created = true;
            hr = M0Native.m0r_write_reference(Path.Combine(artDir, $"m0_s{sess}_rive_ref_t0500.bgra"), 0.5);
            Kv("m0_1.reference_hr", Hx(hr));

            var compInterop = _comp.As<IM0CompositorInterop>();
            hr = compInterop.CreateGraphicsDevice(dev, out var gdPtr);
            Marshal.Release(dev);
            Kv("m0_1.create_graphics_device_hr", Hx(hr));
            if (hr < 0) { Kv("m0_1.verdict", "FAIL_GRAPHICS_DEVICE"); Kv("verdict", "M0_1_FAIL_GRAPHICS_DEVICE"); return; }
            var gd = CompositionGraphicsDevice.FromAbi(gdPtr);
            Marshal.Release(gdPtr);
            var surface = gd.CreateDrawingSurface2(new Windows.Graphics.SizeInt32 { Width = (int)M0SurfacePx, Height = (int)M0SurfacePx },
                DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
            hr = M0Native.m0r_attach_surface(((IWinRTObject)surface).NativeObject.ThisPtr);
            Kv("m0_1.surface_interop_qi_hr", Hx(hr));
            if (hr < 0) { Kv("m0_1.verdict", "FAIL_SURFACE_INTEROP"); Kv("verdict", "M0_1_FAIL_SURFACE_INTEROP"); return; }

            // Place the surface where AnimBox rests, so M0-2 compares like with like.
            var boxAt = AnimBox.TransformToVisual(RootGrid).TransformPoint(new Windows.Foundation.Point(0, 0));
            sprite = _comp.CreateSpriteVisual();
            sprite.Size = new Vector2((float)(M0SurfacePx / scale), (float)(M0SurfacePx / scale));
            sprite.Offset = new Vector3((float)boxAt.X, (float)boxAt.Y, 0);
            sprite.Brush = _comp.CreateSurfaceBrush(surface);
            ElementCompositionPreview.SetElementChildVisual(RootGrid, sprite);

            Kv("m0_1a.start_hr", Hx(M0Native.m0r_start()));
            await Task.Delay(M0PhaseMs);
            M0Native.m0r_get_stats(out var st);
            M0ReportStats(Kv, "m0_1a", st, refreshHz);

            // ---- M0-2: drag misalignment, element vs content inside the Rive surface ----
            // ---- M0-3/5: pointer -> screen latency (expression vs UI-thread write) ----
            if (!inject)
            {
                Kv("m0_2.verdict", "SKIP_NO_INJECT_APPROVAL");
                Kv("m0_35.verdict", "SKIP_NO_INJECT_APPROVAL");
            }
            else
            {
                await M0PointerPhases(hwnd, scale, sprite, Kv, Raw);
            }

            // ---- M0-1b: Rive -> composition swapchain ----------------------------------
            M0Native.m0r_stop();
            M0Native.m0r_set_marker(0, 0, 0, 0, 0, 0, 0);
            hr = M0Native.m0r_create_swapchain(out var sc);
            Kv("m0_1b.create_swapchain_hr", Hx(hr));
            if (hr >= 0)
            {
                var scInterop = _comp.As<IM0CompositorSwapChainInterop>();
                hr = scInterop.CreateCompositionSurfaceForSwapChain(sc, out var csPtr);
                Marshal.Release(sc);
                Kv("m0_1b.surface_for_swapchain_hr", Hx(hr));
                if (hr >= 0)
                {
                    var cs = MarshalInterface<ICompositionSurface>.FromAbi(csPtr);
                    Marshal.Release(csPtr);
                    sprite.Brush = _comp.CreateSurfaceBrush(cs);
                    M0Native.m0r_reset_stats();  // M0-B: report the swapchain phase on its own
                    Kv("m0_1b.start_hr", Hx(M0Native.m0r_start()));
                    await Task.Delay(M0PhaseMs);
                    M0Native.m0r_get_stats(out var st2);
                    M0ReportStats(Kv, "m0_1b", st2, refreshHz);
                }
            }
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
            if (created) { M0Native.m0r_stop(); M0Native.m0r_get_stats(out var fin); Kv("device_removed_hr", "0x" + fin.DeviceRemovedHr.ToString("X8", CultureInfo.InvariantCulture)); M0Native.m0r_destroy(); }
            if (sprite is not null) ElementCompositionPreview.SetElementChildVisual(RootGrid, null);
            CompReset(0, "m0 end");
            raw.Dispose(); sum.Dispose();
            FlushLog();
            _suiteRunning = false;
            try { MeasureButton.IsEnabled = true; } catch { }
        }
    }

    private static void M0ReportStats(Action<string, object> kv, string p, M0Native.Stats s, double refreshHz)
    {
        double T = refreshHz > 0 ? 1000.0 / refreshHz : double.NaN;
        kv(p + ".frames", s.Frames);
        kv(p + ".draw_failures", s.BeginDrawFailures);
        kv(p + ".cpu_p50_ms", F(s.CpuP50, 3));
        kv(p + ".cpu_p95_ms", F(s.CpuP95, 3));
        kv(p + ".cpu_max_ms", F(s.CpuMax, 3));
        kv(p + ".cpu_p95_frac_T", F(s.CpuP95 / T, 3));    // plan budget: render thread <= 0.35
        kv(p + ".gpu_p50_ms", F(s.GpuP50, 3));
        kv(p + ".gpu_p95_ms", F(s.GpuP95, 3));
        kv(p + ".gpu_p95_frac_T", F(s.GpuP95 / T, 3));    // plan budget: GPU <= 0.35
        kv(p + ".gpu_samples_dropped", s.GpuSamplesDropped);
        kv(p + ".interval_p50_ms", F(s.IntervalP50, 3));
        kv(p + ".interval_p95_ms", F(s.IntervalP95, 3));
        kv(p + ".last_hr", "0x" + s.LastHr.ToString("X8", CultureInfo.InvariantCulture));
        kv(p + ".offscreen_uav", s.OffscreenHasUav);
    }

    /// Reads AnimBox's solid colour: the WGC tracker (mfprobe) follows that colour.
    private Windows.UI.Color? M0BoxColor()
    {
        Brush? b = (object)AnimBox switch
        {
            Microsoft.UI.Xaml.Shapes.Shape s => s.Fill,
            Border bd => bd.Background,
            Panel pn => pn.Background,
            Control c => c.Background,
            _ => null,
        };
        return (b as SolidColorBrush)?.Color;
    }

    private async Task M0PointerPhases(IntPtr hwnd, double scale, SpriteVisual sprite, Action<string, object> Kv, Action<string> Raw)
    {
        // Geometry: screen px of AnimBox's centre at rest; pointer steps run along +x.
        var pt = new M0Native.POINT();
        M0Native.ClientToScreen(hwnd, ref pt);
        var boxAt = AnimBox.TransformToVisual(null).TransformPoint(new Windows.Foundation.Point(AnimBox.ActualWidth / 2, AnimBox.ActualHeight / 2));
        int sx = pt.X + (int)Math.Round(boxAt.X * scale), sy = pt.Y + (int)Math.Round(boxAt.Y * scale);
        var rootAt = RootGrid.TransformToVisual(null).TransformPoint(new Windows.Foundation.Point(0, 0));
        double x0Dip = boxAt.X - rootAt.X;   // pointer x relative to RootGrid at rest
        int stepPx = (int)Math.Round(M0StepDip * scale);
        Kv("m0_ptr.start_screen", $"{sx},{sy}");
        Kv("m0_ptr.step_px", stepPx);

        var color = M0BoxColor();
        Kv("m0_ptr.box_color", color is { } c0 ? $"{c0.R},{c0.G},{c0.B}" : "unknown");

        int capHr = MfProbe.mf_cap_start(hwnd);
        Kv("m0_ptr.wgc_start_hr", "0x" + capHr.ToString("X8", CultureInfo.InvariantCulture));
        if (capHr != 0) { Kv("m0_2.verdict", "INVALID_WGC"); Kv("m0_35.verdict", "INVALID_WGC"); return; }
        try
        {
            var pps = ElementCompositionPreview.GetPointerPositionPropertySet(RootGrid);
            // M0-A found the computed x0 off by ~70 px (expression rest cx 30 vs 100), so
            // the pointer set's origin is not RootGrid's TransformToVisual origin. Park the
            // cursor at rest and take x0 from the set itself when it reports a value.
            M0Native.MoveCursor(sx - 2, sy); await Task.Delay(100);
            M0Native.MoveCursor(sx, sy); await Task.Delay(300);
            double xUiDip = x0Dip;  // the UI path reads GetCurrentPoint(RootGrid); computed x0 was right there
            var ppsStatus = pps.TryGetVector3("Position", out var ppsRest);
            Kv("m0_ptr.x0_computed_dip", F(x0Dip, 2));
            Kv("m0_ptr.pps_rest", $"{ppsStatus}:{F(ppsRest.X, 2)},{F(ppsRest.Y, 2)}");
            if (ppsStatus == CompositionGetValueStatus.Succeeded && ppsRest.X != 0) x0Dip = ppsRest.X;
            Kv("m0_ptr.x0_used_dip", F(x0Dip, 2));
            ExpressionAnimation Follow()
            {
                var e = _comp!.CreateExpressionAnimation($"pps.Position.X - {x0Dip.ToString("R", CultureInfo.InvariantCulture)}");
                e.SetReferenceParameter("pps", pps);
                return e;
            }

            // Path list: (name, setup, teardown). Every path runs the same injected steps.
            double uiX0 = xUiDip;
            PointerEventHandler onMove = (s, e) =>
            {
                var p = e.GetCurrentPoint(RootGrid).Position;
                _visual!.Properties.InsertVector3("Translation", new Vector3((float)(p.X - uiX0), 0, 0));
            };
            var paths = new List<(string name, Action setup, Action teardown)>
            {
                ("expr", () => _visual!.StartAnimation("Translation.X", Follow()), () => CompReset(0, "m0 expr end")),
                ("ui", () => { _visual!.StopAnimation("Translation.X"); RootGrid.PointerMoved += onMove; }, () => { RootGrid.PointerMoved -= onMove; CompReset(0, "m0 ui end"); }),
            };
            // The Rive sprite sits over AnimBox; hide it while AnimBox is the tracked object.
            sprite.IsVisible = false;
            foreach (var (name, setup, teardown) in paths)
                await M0RunSteps(name, sx, sy, stepPx, setup, teardown, Kv, Raw);

            // M0-2: constant-speed drags. Element (AnimBox via expression) first, then the
            // Rive surface on the same expression carrying a marker in AnimBox's colour
            // while AnimBox is hidden, so the tracker sees one object at a time. Each path
            // is scored by its time lag behind the injected cursor trajectory.
            if (color is { } c)
            {
                var lagExpr = await M0RunDrags("drag_expr", sx, sy, stepPx,
                    () => _visual!.StartAnimation("Translation.X", Follow()), () => CompReset(0, "m0 drag expr end"), Kv, Raw);
                double prevOpacity = AnimBox.Opacity;
                int mhr = M0Native.m0r_set_marker(0, 0, (uint)Math.Round(AnimBox.ActualWidth * scale), (uint)Math.Round(AnimBox.ActualHeight * scale), c.R, c.G, c.B);
                Kv("m0_2.marker_hr", "0x" + mhr.ToString("X8", CultureInfo.InvariantCulture));
                var baseOffset = sprite.Offset;
                var lagSurface = await M0RunDrags("drag_surface", sx, sy, stepPx,
                    () => { AnimBox.Opacity = 0; sprite.IsVisible = true; var e = Follow(); e.Expression = $"{baseOffset.X.ToString("R", CultureInfo.InvariantCulture)} + " + e.Expression; sprite.StartAnimation("Offset.X", e); },
                    () => { sprite.StopAnimation("Offset.X"); sprite.Offset = baseOffset; sprite.IsVisible = true; AnimBox.Opacity = prevOpacity; M0Native.m0r_set_marker(0, 0, 0, 0, 0, 0, 0); },
                    Kv, Raw);
                double Med(List<double> v) { var o = v.OrderBy(x => x).ToList(); return o.Count == 0 ? double.NaN : o[(o.Count - 1) / 2]; }
                double dLag = Med(lagSurface) - Med(lagExpr);
                double pxPerMs = stepPx / (double)M0DragMs;
                Kv("m0_2.lag_expr_p50_ms", F(Med(lagExpr), 2));
                Kv("m0_2.lag_surface_p50_ms", F(Med(lagSurface), 2));
                Kv("m0_2.misalign_ms", F(dLag, 2));
                Kv("m0_2.misalign_px_at_drag_speed", F(Math.Abs(dLag) * pxPerMs, 2));
                Kv("m0_2.drag_speed_px_per_ms", F(pxPerMs, 3));
                Kv("m0_2.verdict", lagExpr.Count >= 20 && lagSurface.Count >= 20 ? "RECORDED" : "INCONCLUSIVE_FEW_SAMPLES");
            }
            else Kv("m0_2.verdict", "INVALID_UNKNOWN_BOX_COLOR");
            Kv("m0_35.verdict", "RECORDED");
        }
        finally { MfProbe.mf_cap_stop(); sprite.IsVisible = true; }
    }

    /// Injects M0Steps alternating +step/-step jumps and records, per step, the time
    /// from injection to the first WGC frame past the half-step point (M0-3/M0-5).
    private async Task M0RunSteps(string name, int sx, int sy, int stepPx,
        Action setup, Action teardown, Action<string, object> Kv, Action<string> Raw)
    {
        double qpcMs(long q) => q * 1000.0 / Stopwatch.Frequency;
        var lat = new List<double>();
        M0Native.MoveCursor(sx, sy);
        await Task.Delay(300);
        setup();
        await Task.Delay(300);
        int seen = MfProbe.mf_cap_count();
        double cxRest = double.NaN;
        for (int i = Math.Max(0, seen - 5); i < seen; i++)
            if (MfProbe.mf_cap_get(i, out _, out _, out _, out _, out _, out _, out _, out _, out var n, out var cx) == 1 && n > 50) cxRest = cx;
        Raw($"M0 {name}: rest cx={F(cxRest, 2)}");
        double offsetMin = double.MaxValue;  // min(qpcArrive - srt): sanity check of the shared clock
        for (int k = 0; k < M0Steps; k++)
        {
            bool fwd = k % 2 == 0;
            int x = sx + (fwd ? stepPx : 0);
            long tInj = Stopwatch.GetTimestamp();
            uint sent = M0Native.MoveCursor(x, sy);
            await Task.Delay(M0StepGapMs);
            int n2 = MfProbe.mf_cap_count();
            double target = cxRest + (fwd ? stepPx : 0), from = cxRest + (fwd ? 0 : stepPx);
            double half = (target + from) / 2, hit = double.NaN;
            for (int i = seen; i < n2; i++)
            {
                if (MfProbe.mf_cap_get(i, out var srt, out var qArr, out _, out _, out var minx, out var maxx, out _, out _, out var cnt, out var cx) != 1) continue;
                double srtMs = srt / 1e4, rel = srtMs - qpcMs(tInj);
                // Per-frame trace (M0-A: backward steps of the expression path were never
                // detected; this shows whether the box returned, was clipped, or vanished).
                Raw($"M0F {name} step={k} rel_ms={F(rel, 2)} cx={F(cx, 2)} minx={minx} maxx={maxx} n={cnt}");
                if (cnt <= 50) continue;
                if (qArr != 0) offsetMin = Math.Min(offsetMin, qpcMs(qArr) - srtMs);
                if (double.IsNaN(hit) && rel > 0 && (fwd ? cx >= half : cx <= half)) hit = rel;
            }
            seen = n2;
            lat.Add(hit);
            Raw($"M0 {name} step={k} dir={(fwd ? "+" : "-")} sent={sent} latency_ms={F(hit, 2)}");
        }
        teardown();
        var ok = lat.Where(v => !double.IsNaN(v)).OrderBy(v => v).ToList();
        double P(double q) => ok.Count == 0 ? double.NaN : ok[Math.Min(ok.Count - 1, (int)Math.Ceiling(q * ok.Count) - 1)];
        Kv($"m0_35.{name}.steps_hit", $"{ok.Count}/{M0Steps}");
        Kv($"m0_35.{name}.latency_p50_ms", F(P(0.5), 2));
        Kv($"m0_35.{name}.latency_p95_ms", F(P(0.95), 2));
        Kv($"m0_35.{name}.clock_offset_min_ms", F(offsetMin == double.MaxValue ? double.NaN : offsetMin, 3));
    }

    /// M0-2: injects M0Drags constant-speed drags (alternating direction) from a
    /// dedicated thread at ~1 kHz, then for every WGC frame inside a drag computes
    /// how long ago the cursor was where the tracked object is now (ms).
    private async Task<List<double>> M0RunDrags(string name, int sx, int sy, int stepPx,
        Action setup, Action teardown, Action<string, object> Kv, Action<string> Raw)
    {
        double qpcMs(long q) => q * 1000.0 / Stopwatch.Frequency;
        var lags = new List<double>();
        M0Native.MoveCursor(sx, sy);
        await Task.Delay(300);
        setup();
        await Task.Delay(300);
        int seen = MfProbe.mf_cap_count();
        double cxRest = double.NaN;
        for (int i = Math.Max(0, seen - 5); i < seen; i++)
            if (MfProbe.mf_cap_get(i, out _, out _, out _, out _, out _, out _, out _, out _, out var n, out var cx) == 1 && n > 50) cxRest = cx;
        for (int k = 0; k < M0Drags; k++)
        {
            bool fwd = k % 2 == 0;
            // Cursor trajectory as (qpc ms, cursor offset px from rest), recorded per move.
            var traj = await Task.Run(() =>
            {
                var t = new List<(double ms, double px)>();
                long t0 = Stopwatch.GetTimestamp(), dur = M0DragMs * Stopwatch.Frequency / 1000;
                int last = int.MinValue;
                for (long now = t0; now - t0 <= dur; now = Stopwatch.GetTimestamp())
                {
                    double f = (double)(now - t0) / dur;
                    int off = (int)Math.Round((fwd ? f : 1 - f) * stepPx);
                    if (off != last) { M0Native.MoveCursor(sx + off, sy); t.Add((qpcMs(Stopwatch.GetTimestamp()), off)); last = off; }
                    System.Threading.Thread.SpinWait(200);
                }
                return t;
            });
            await Task.Delay(250);
            int n2 = MfProbe.mf_cap_count();
            int used = 0;
            for (int i = seen; i < n2; i++)
            {
                if (MfProbe.mf_cap_get(i, out var srt, out _, out _, out _, out _, out _, out _, out _, out var cnt, out var cx) != 1 || cnt <= 50) continue;
                double pos = cx - cxRest, srtMs = srt / 1e4;
                if (pos <= 0.05 * stepPx || pos >= 0.95 * stepPx) continue;   // only the moving part
                // Latest cursor sample at or before this frame's time with that offset.
                int j = traj.FindLastIndex(p => p.ms <= srtMs && (fwd ? p.px <= pos : p.px >= pos));
                if (j < 0) continue;
                lags.Add(srtMs - traj[j].ms);
                used++;
            }
            seen = n2;
            Raw($"M0 {name} drag={k} dir={(fwd ? "+" : "-")} moves={traj.Count} frames_used={used}");
        }
        teardown();
        var o = lags.OrderBy(v => v).ToList();
        double P(double q) => o.Count == 0 ? double.NaN : o[Math.Min(o.Count - 1, (int)Math.Ceiling(q * o.Count) - 1)];
        Kv($"m0_2.{name}.samples", o.Count);
        Kv($"m0_2.{name}.lag_p50_ms", F(P(0.5), 2));
        Kv($"m0_2.{name}.lag_p95_ms", F(P(0.95), 2));
        return lags;
    }
}
