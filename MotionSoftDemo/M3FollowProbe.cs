// M3-RIVE-FOLLOW probe (docs/m3_rive_follow_gate_LOCKED.md). Four ways for a part of a
// Rive animation to follow the pointer, measured with M0's method (SendInput steps and
// drags, WGC tracking of the AnimBox colour):
//   A  in the Rive surface, position handed over by the UI thread (m0r_set_marker)
//   B  in the Rive surface, cursor read by the render thread itself (m0r_set_follow_cursor)
//   C  split-out visual driven by the pointer property-set expression (M0 "expr")
//   D  split-out visual written by the UI thread (M0 "ui")
// Only one object in the tracked colour is visible at a time. The seam between a split
// part and the rest of Rive is the drag-lag difference C - A (as M0-2's misalignment).
// composition.h is not used or touched.
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
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using WinRT;

namespace MotionSoftDemo;

internal static class M3FollowNative
{
    [DllImport("m0rive")] internal static extern int m0r_set_follow_cursor(IntPtr hwnd, int originX, int originY, uint w, uint h, byte r, byte g, byte b);
}

public sealed partial class MainPage
{
    const int M3FollowSteps = 40;  // per path: 20 forward + 20 backward (P3)

    private async Task RunM3FollowProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true;
        try { MeasureButton.IsEnabled = false; } catch { }

        string artDir = Environment.GetEnvironmentVariable("MF_M0_ARTDIR") ?? @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string sess = Environment.GetEnvironmentVariable("MF_M3_SESSION") ?? "1";
        string rivPath = Environment.GetEnvironmentVariable("MF_M0_RIV") ?? Path.Combine(artDir, "m0_assets", "Knight_square_2.riv");
        string pre = Path.Combine(artDir, $"m3follow_s{sess}");
        var raw = new StreamWriter(pre + "_raw.txt", false, new UTF8Encoding(false));
        var sum = new StreamWriter(pre + "_summary.txt", false, new UTF8Encoding(false));
        bool verdictWritten = false;  // the runner waits for a verdict= line
        void Raw(string s) { Log(s); raw.WriteLine(s); raw.Flush(); }
        void Kv(string k, object v) { string s = $"{k}={Convert.ToString(v, CultureInfo.InvariantCulture)}"; Raw("KV " + s); sum.WriteLine(s); sum.Flush(); if (k == "verdict") verdictWritten = true; }
        static string Hx(int hr) => "0x" + hr.ToString("X8", CultureInfo.InvariantCulture);

        SpriteVisual? sprite = null;
        bool created = false, capturing = false;
        double prevOpacity = AnimBox.Opacity;
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            ShowWindow(hwnd, 9); SetForegroundWindow(hwnd);
            double scale = XamlRoot?.RasterizationScale ?? 1.0;
            double refreshHz = 0;
            try { MfProbe.mf_dwm_timing(out _, out _, out _, out _, out var rn, out var rd, out _); if (rd > 0) refreshHz = (double)rn / rd; } catch { }
            double T = refreshHz > 0 ? 1000.0 / refreshHz : double.NaN;
            Kv("probe", "M3-FOLLOW v1");
            Kv("session", sess);
            Kv("pid", Environment.ProcessId);
            Kv("host", Environment.MachineName);
            Kv("os", Environment.OSVersion.VersionString);
            Kv("scale", F(scale, 3));
            Kv("refresh_hz", F(refreshHz, 3));
            if (!_compMode) { _toggleFromCode = true; ModeToggle.IsOn = true; _toggleFromCode = false; SetMode(true, "m3 follow"); }
            CompReset(0, "m3 follow rest");

            // Rive surface over AnimBox, as in M0.
            int hr = M0Native.m0r_create(rivPath, M0SurfacePx, M0SurfacePx, out var dev);
            Kv("create_hr", Hx(hr));
            if (hr < 0) { Kv("verdict", "INVALID_CREATE"); return; }
            created = true;
            var gdHr = _comp.As<IM0CompositorInterop>().CreateGraphicsDevice(dev, out var gdPtr);
            Marshal.Release(dev);
            if (gdHr < 0) { Kv("graphics_device_hr", Hx(gdHr)); Kv("verdict", "INVALID_GRAPHICS_DEVICE"); return; }
            var gd = CompositionGraphicsDevice.FromAbi(gdPtr);
            Marshal.Release(gdPtr);
            var surface = gd.CreateDrawingSurface2(new Windows.Graphics.SizeInt32 { Width = (int)M0SurfacePx, Height = (int)M0SurfacePx },
                DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
            hr = M0Native.m0r_attach_surface(((IWinRTObject)surface).NativeObject.ThisPtr);
            Kv("surface_interop_qi_hr", Hx(hr));
            if (hr < 0) { Kv("verdict", "INVALID_SURFACE_INTEROP"); return; }
            var boxAt = AnimBox.TransformToVisual(RootGrid).TransformPoint(new Windows.Foundation.Point(0, 0));
            sprite = _comp.CreateSpriteVisual();
            sprite.Size = new Vector2((float)(M0SurfacePx / scale), (float)(M0SurfacePx / scale));
            sprite.Offset = new Vector3((float)boxAt.X, (float)boxAt.Y, 0);
            sprite.Brush = _comp.CreateSurfaceBrush(surface);
            ElementCompositionPreview.SetElementChildVisual(RootGrid, sprite);
            Kv("start_hr", Hx(M0Native.m0r_start()));

            // Geometry (M0PointerPhases): cursor rests on AnimBox's centre; steps and drags run along +x.
            var pt = new M0Native.POINT();
            M0Native.ClientToScreen(hwnd, ref pt);
            var boxCentre = AnimBox.TransformToVisual(null).TransformPoint(new Windows.Foundation.Point(AnimBox.ActualWidth / 2, AnimBox.ActualHeight / 2));
            var boxClient = AnimBox.TransformToVisual(null).TransformPoint(new Windows.Foundation.Point(0, 0));
            int sx = pt.X + (int)Math.Round(boxCentre.X * scale), sy = pt.Y + (int)Math.Round(boxCentre.Y * scale);
            var rootAt = RootGrid.TransformToVisual(null).TransformPoint(new Windows.Foundation.Point(0, 0));
            double x0Dip = boxCentre.X - rootAt.X;
            int stepPx = (int)Math.Round(M0StepDip * scale);
            uint bw = (uint)Math.Round(AnimBox.ActualWidth * scale), bh = (uint)Math.Round(AnimBox.ActualHeight * scale);
            var color = M0BoxColor();
            Kv("start_screen", $"{sx},{sy}");
            Kv("step_px", stepPx);
            Kv("block_px", $"{bw}x{bh}");
            Kv("box_color", color is { } c0 ? $"{c0.R},{c0.G},{c0.B}" : "unknown");
            if (color is not { } c) { Kv("verdict", "INVALID_UNKNOWN_BOX_COLOR"); return; }

            int capHr = MfProbe.mf_cap_start(hwnd);
            Kv("wgc_start_hr", Hx(capHr));
            if (capHr != 0) { Kv("verdict", "INVALID_WGC"); return; }
            capturing = true;

            var pps = ElementCompositionPreview.GetPointerPositionPropertySet(RootGrid);
            M0Native.MoveCursor(sx - 2, sy); await Task.Delay(100);
            M0Native.MoveCursor(sx, sy); await Task.Delay(300);
            double xUiDip = x0Dip;
            if (pps.TryGetVector3("Position", out var ppsRest) == CompositionGetValueStatus.Succeeded && ppsRest.X != 0) x0Dip = ppsRest.X;
            Kv("x0_used_dip", F(x0Dip, 2));
            ExpressionAnimation Follow()
            {
                var e = _comp!.CreateExpressionAnimation($"pps.Position.X - {x0Dip.ToString("R", CultureInfo.InvariantCulture)}");
                e.SetReferenceParameter("pps", pps);
                return e;
            }

            // Path A: UI thread -> m0r_set_marker -> next Rive frame. Surface px of the cursor:
            // (RootGrid DIP - sprite offset) * scale; the block is centred on it.
            PointerEventHandler onMoveA = (s, e) =>
            {
                var p = e.GetCurrentPoint(RootGrid).Position;
                int cx = (int)Math.Round((p.X - boxAt.X) * scale), cy = (int)Math.Round((p.Y - boxAt.Y) * scale);
                M0Native.m0r_set_marker((uint)Math.Max(0, cx - (int)bw / 2), (uint)Math.Max(0, cy - (int)bh / 2), bw, bh, c.R, c.G, c.B);
            };
            PointerEventHandler onMoveD = (s, e) =>
            {
                var p = e.GetCurrentPoint(RootGrid).Position;
                _visual!.Properties.InsertVector3("Translation", new Vector3((float)(p.X - xUiDip), 0, 0));
            };
            void SurfaceOn() { AnimBox.Opacity = 0; sprite!.IsVisible = true; }
            void SurfaceOff() { M0Native.m0r_set_marker(0, 0, 0, 0, 0, 0, 0); M3FollowNative.m0r_set_follow_cursor(IntPtr.Zero, 0, 0, 0, 0, 0, 0, 0); AnimBox.Opacity = prevOpacity; }
            var paths = new List<(string name, Action setup, Action teardown)>
            {
                ("A_surface_ui", () => { SurfaceOn(); M0Native.m0r_set_marker(0, 0, bw, bh, c.R, c.G, c.B); RootGrid.PointerMoved += onMoveA; },
                                 () => { RootGrid.PointerMoved -= onMoveA; SurfaceOff(); }),
                ("B_surface_latch", () => { SurfaceOn(); M3FollowNative.m0r_set_follow_cursor(hwnd, (int)Math.Round(boxClient.X * scale), (int)Math.Round(boxClient.Y * scale), bw, bh, c.R, c.G, c.B); },
                                    () => SurfaceOff()),
                ("C_split_expr", () => { sprite.IsVisible = false; _visual!.StartAnimation("Translation.X", Follow()); },
                                 () => { CompReset(0, "m3 follow C end"); sprite.IsVisible = true; }),
                ("D_split_ui", () => { sprite.IsVisible = false; _visual!.StopAnimation("Translation.X"); RootGrid.PointerMoved += onMoveD; },
                               () => { RootGrid.PointerMoved -= onMoveD; CompReset(0, "m3 follow D end"); sprite.IsVisible = true; }),
            };

            var stepP50 = new Dictionary<string, double>();
            var dragP50 = new Dictionary<string, double>();
            var hitOk = new Dictionary<string, bool>();
            bool budgetOk = true;
            double Med(IEnumerable<double> v) { var o = v.Where(x => !double.IsNaN(x)).OrderBy(x => x).ToList(); return o.Count == 0 ? double.NaN : o[(o.Count - 1) / 2]; }
            foreach (var (name, setup, teardown) in paths)
            {
                M0Native.m0r_reset_stats();
                var lat = await M0RunSteps(name, sx, sy, stepPx, setup, teardown, Kv, Raw, M3FollowSteps, "follow");
                var lags = await M0RunDrags("drag_" + name, sx, sy, stepPx, setup, teardown, Kv, Raw, "follow");
                M0Native.m0r_get_stats(out var st);
                stepP50[name] = Med(lat);
                dragP50[name] = Med(lags);
                int hits = lat.Count(v => !double.IsNaN(v));
                int fwd = lat.Where((v, i) => i % 2 == 0 && !double.IsNaN(v)).Count();
                hitOk[name] = fwd >= M3FollowSteps / 4 && hits - fwd >= M3FollowSteps / 4;  // at least half of each direction
                bool b = st.CpuP95 <= 0.35 * T && st.GpuP95 <= 0.35 * T && st.GpuSamplesDropped <= 0.10 * Math.Max(1, st.Frames);
                budgetOk &= b;
                Kv($"follow.{name}.cpu_p95_frac_T", F(st.CpuP95 / T, 3));
                Kv($"follow.{name}.gpu_p95_frac_T", F(st.GpuP95 / T, 3));
                Kv($"follow.{name}.draw_failures", st.BeginDrawFailures);
                Kv($"follow.{name}.budget_ok", b ? 1 : 0);
            }

            // G2 rules (P6), in frames of T.
            double aStep = stepP50["A_surface_ui"], bStep = stepP50["B_surface_latch"], cStep = stepP50["C_split_expr"];
            double seam = dragP50["C_split_expr"] - dragP50["A_surface_ui"];
            Kv("rule.c_vs_a_ms", F(aStep - cStep, 2));
            Kv("rule.c_vs_a_frames", F((aStep - cStep) / T, 2));
            Kv("rule.split_default", (aStep - cStep) >= T ? 1 : 0);
            Kv("rule.b_vs_a_ms", F(aStep - bStep, 2));
            Kv("rule.b_vs_a_frames", F((aStep - bStep) / T, 2));
            Kv("rule.latch_default", (aStep - bStep) >= 0.5 * T ? 1 : 0);
            Kv("rule.seam_ms", F(seam, 2));
            Kv("rule.seam_frames", F(seam / T, 2));
            Kv("rule.seam_ok", Math.Abs(seam) <= T ? 1 : 0);
            Kv("budget_ok", budgetOk ? 1 : 0);
            Kv("verdict", hitOk.Values.All(v => v) ? "RECORDED" : "INVALID_FEW_STEPS");
        }
        catch (Exception ex)
        {
            Kv("exception", ex.GetType().Name + ": " + ex.Message.Replace('\n', ' '));
            Kv("verdict", "INVALID_EXCEPTION");
        }
        finally
        {
            if (!verdictWritten) Kv("verdict", "INVALID_INCOMPLETE");
            if (capturing) MfProbe.mf_cap_stop();
            AnimBox.Opacity = prevOpacity;
            if (created)
            {
                M0Native.m0r_stop();
                M0Native.m0r_destroy();
            }
            if (sprite is not null) ElementCompositionPreview.SetElementChildVisual(RootGrid, null);
            raw.Dispose(); sum.Dispose();
            FlushLog();
            _suiteRunning = false;
            try { MeasureButton.IsEnabled = true; } catch { }
        }
    }
}
