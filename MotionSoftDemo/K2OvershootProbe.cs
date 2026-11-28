// Module 7 / MF-02 — K2 bezier-overshoot re-verification probe.
//
// K2 claim (docs/module7-plan.md): "`CubicBezierEasingFunction`
// honors control-point y outside [0,1] (`bezierOvershoot = true`).
// Probe: overshoot easing on the target OS; if it does not
// overshoot, `bezierOvershoot` must become probe-driven, not a
// constant." composition.h records K2 as verified (派题 #20) and
// carries `Capabilities::bezierOvershoot = true` as a constant; the
// plan preamble resolves the contradiction by re-verifying S1 and
// K2 on the target OS with evidence. No Windows report for #20 is
// on file, so the constant currently rests on an unevidenced claim.
//
// What #20 (RunK2Probe, --k2-probe) does NOT establish: single-
// sample calibration with a RasterizationScale fallback and no
// INVALID guard; absolute cx0-scale min/max (the S4 zero-bias
// lesson: cx0 reads up to ~3.6px high, varying run to run); a
// convoluted RMS honor-vs-clamp comparison with a 3-dip overshoot
// bar and no comparison of the measured peak against the analytic
// bezier peak; 0.40s duration (24 frames); no readbacks; verdict
// logic that can call AMBIGUOUS. This probe measures the claim
// directly, with the S1/S2/S3 discipline plus the S4 lessons:
// median calibration, per-trial differenced measurement (peak and
// trough relative to the trial's own rest and settled values, so
// any cx0 additive bias cancels), a stated tolerance, and an
// in-range control cell with the SAME x control points, so the
// only difference is the y values.
//
// Design. Span 0 -> 240 dips, duration 1.0s. Test easing
// (0.2,-0.5)-(0.8,1.5): analytic peak +19.43 dips, trough -19.43
// dips (dense y(t) sampling; x(t) is strictly increasing, min
// dx/dt 0.6, so the easing attains max y(t)). Control easing
// (0.2,0)-(0.8,1): analytic peak/trough exactly 0. The probe
// recomputes both predictions at runtime by the same dense
// sampling and logs them; the header states the values computed
// offline. Gates (one rule, both cells): |overshoot - predOver|
// <= 2 dips and |undershoot - predUnder| <= 2 dips, where
// overshoot = peak - settled and undershoot = zero - trough, all
// differenced within the trial. Span is validated separately:
// |settled - zero - 240| <= 2 dips. Control-point readback is
// deliberately NOT attempted: no repo code reads those properties,
// and the behavioral peak comparison subsumes it (creation-time
// clamping and evaluation-time clamping both show as CLAMP);
// Duration is read back. If creation throws for out-of-range y,
// that is the REJECT outcome and the trial FAILs as create-throw.
// The CONCLUSION line (HONOR / CLAMP / REJECT / UNCLEAR) answers
// the plan's branch directly; it never overrides the VERDICT.
// Flag: --k2-overshoot-probe / MF_K2_OVERSHOOT_PROBE=1.
// Log: m57_k2_overshoot_ui.txt. Artifact: win_k2_overshoot_raw.txt.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;

namespace MotionSoftDemo;

public sealed partial class MainPage
{
    private async Task RunK2OvershootProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = DevLog.Directory();
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_k2_overshoot_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        Both("# win_k2_overshoot_raw.txt — MF-02 K2 bezier-overshoot re-verification probe");
        Both("# wall=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "; OS=" + Environment.OSVersion.Version);
        Both("# CELL span 0->240 duration=1.0s test=(0.2,-0.5)-(0.8,1.5) control=(0.2,0)-(0.8,1) trials=3 observe=2.0s");
        Both("# PREDICT test: overshoot=+19.43 dips, undershoot=+19.43 dips (analytic bezier peak/trough, dense y(t) sampling); control: 0.00/0.00");
        Both("# VERDICT rule: readbacks match, zero stable by capture polling, |settled-zero-240|<=2 dips, |overshoot-predOver|<=2 dips, |undershoot-predUnder|<=2 dips, onset found; no-calibration => INVALID");
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(120);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "k2-overshoot-probe");
            CompReset(0, "k2-overshoot start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            // Settle AFTER capture start so pre-calibration captures exist
            // (patch 0023 lesson).
            CompReset(0, "k2-overshoot cal settle"); await Task.Delay(400);
            // px/dip calibration, same as MAPTEST/S1/S2/S3/S4/S5: 0 -> 37 dips,
            // medians of the last few captures in each window (patch 0026).
            var calQ0 = Qpc; CompReset(37, "k2-overshoot cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "k2-overshoot cal end"); await Task.Delay(500);
            var cc = ReadCaptures().Where(c => c.N > 50).ToList();
            var preCal = cc.Where(c => c.QpcArrive < calQ0).Select(c => c.Cx).ToList();
            var midCal = cc.Where(c => c.QpcArrive >= calQ0 && c.QpcArrive < calQ1).Select(c => c.Cx).ToList();
            double cx0 = preCal.Count > 0 ? MedianS1(preCal.Skip(Math.Max(0, preCal.Count - 5)).ToList()) : double.NaN;
            double cx37 = midCal.Count > 0 ? MedianS1(midCal.Skip(Math.Max(0, midCal.Count - 5)).ToList()) : double.NaN;
            double pxPerDip = (double.IsNaN(cx0) || double.IsNaN(cx37)) ? double.NaN : (cx37 - cx0) / 37.0;
            Both($"cal cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(pxPerDip,4)} nPre={preCal.Count} nMid={midCal.Count}");

            int passes = 0, total = 0;
            bool anyCreateThrow = false;
            var testOvers = new List<double>();
            var testUnders = new List<double>();
            if (double.IsNaN(cx0) || pxPerDip <= 0)
            {
                Both("K2 OVERSHOOT VERDICT=INVALID reason=no-calibration");
            }
            else
            {
                // Poll-until-stable helper (S4 0036/0037): median of
                // the last 5 captures after the reset, requiring 3
                // consecutive FRESH medians (newest capture
                // advancing) spanning <= 0.5 dips.
                async Task<(double medDip, double medPx, int waitedMs, bool stable)> PollStable(long qAfterReset)
                {
                    long t0 = Stopwatch.GetTimestamp();
                    var recent = new List<double>();
                    long lastNewest = -1;
                    double lastMedDip = double.NaN, lastMedPx = double.NaN;
                    while (true)
                    {
                        await Task.Delay(50);
                        var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive >= qAfterReset).OrderBy(c => c.QpcArrive).TakeLast(5).ToList();
                        if (caps.Count == 5 && caps[caps.Count - 1].QpcArrive > lastNewest)
                        {
                            lastNewest = caps[caps.Count - 1].QpcArrive;
                            lastMedDip = MedianS1(caps.Select(c => (c.Cx - cx0) / pxPerDip).ToList());
                            lastMedPx = MedianS1(caps.Select(c => c.Cx).ToList());
                            recent.Add(lastMedDip);
                            if (recent.Count > 3) recent.RemoveAt(0);
                            if (recent.Count == 3 && recent.Max() - recent.Min() <= 0.5)
                                return (lastMedDip, lastMedPx, (int)((Stopwatch.GetTimestamp() - t0) * 1000 / Stopwatch.Frequency), true);
                        }
                        if ((Stopwatch.GetTimestamp() - t0) * 1000 / Stopwatch.Frequency >= 3000)
                            return (lastMedDip, lastMedPx, (int)((Stopwatch.GetTimestamp() - t0) * 1000 / Stopwatch.Frequency), false);
                    }
                }
                // Analytic bezier peak/trough by dense sampling of
                // y(t). x(t) is strictly increasing for these control
                // points, so the easing attains max/min y(t).
                (double predOver, double predUnder) BezierPrediction(double x1, double y1, double x2, double y2)
                {
                    double maxY = double.NegativeInfinity, minY = double.PositiveInfinity;
                    for (int i = 0; i <= 20000; i++)
                    {
                        double t = i / 20000.0, u = 1 - t;
                        double y = 3 * u * u * t * y1 + 3 * u * t * t * y2 + t * t * t;
                        if (y > maxY) maxY = y;
                        if (y < minY) minY = y;
                    }
                    return ((maxY - 1) * 240.0, -minY * 240.0);
                }
                var cells = new[] { ("control", 0.2f, 0.0f, 0.8f, 1.0f), ("test", 0.2f, -0.5f, 0.8f, 1.5f) };
                foreach (var (cell, bx1, by1, bx2, by2) in cells)
                {
                    var (predOver, predUnder) = BezierPrediction(bx1, by1, bx2, by2);
                    Both($"CELL {cell} easing=({bx1},{by1})-({bx2},{by2}) predOver={F(predOver, 2)} predUnder={F(predUnder, 2)}");
                    for (int trial = 0; trial < 3; trial++)
                    {
                        total++;
                        CompReset(0, $"k2-overshoot {cell} t={trial} zero");
                        long qZero = Qpc;
                        var (zeroDip, zeroPx, zeroWait, zeroStable) = await PollStable(qZero);
                        Both($"ZERO cell={cell} trial={trial} waitedMs={zeroWait} zero={F(zeroDip, 2)} zeroPx={F(zeroPx, 2)} stable={zeroStable}");
                        if (!zeroStable || double.IsNaN(zeroDip))
                        {
                            Both($"TRIAL cell={cell} trial={trial} zero={F(zeroDip, 2)} FAIL reason=zero-not-stable");
                            continue;
                        }
                        string assignNote;
                        TimeSpan readDur = TimeSpan.MinValue;
                        bool created = false;
                        try
                        {
                            var ease = _comp!.CreateCubicBezierEasingFunction(new Vector2(bx1, by1), new Vector2(bx2, by2));
                            var k = _comp!.CreateScalarKeyFrameAnimation();
                            k.InsertKeyFrame(0f, 0f);
                            k.InsertKeyFrame(1f, 240f, ease);
                            k.Duration = TimeSpan.FromSeconds(1.0);
                            readDur = k.Duration;
                            created = true;
                            assignNote = $"assign ok readback durationMs={F(readDur.TotalMilliseconds, 0)}";
                            long q0 = Qpc;
                            _visual!.StartAnimation("Translation.X", k);
                            await Task.Delay(2000);
                            long qEnd = Qpc;
                            CompReset(0, $"k2-overshoot {cell} t={trial} end");
                            // Let the stop present before the next
                            // trial's zero poll (S4 0037 lesson).
                            await Task.Delay(300);
                            if (readDur != TimeSpan.FromSeconds(1.0))
                            {
                                Both($"TRIAL cell={cell} trial={trial} {assignNote} FAIL reason=readback-mismatch");
                                continue;
                            }
                            var samples = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive >= q0 && c.QpcArrive <= qEnd)
                                .Select(c => (t: SecSince(q0, c.QpcArrive), x: (c.Cx - cx0) / pxPerDip))
                                .OrderBy(p => p.t).ToList();
                            string rawLine = samples.Count > 0
                                ? string.Join(" ", samples.Select(p => $"{F(p.t * 1000, 0)}:{F(p.x, 2)}"))
                                : "(no samples)";
                            Both($"RAW cell={cell} trial={trial} n={samples.Count} zero={F(zeroDip, 2)} {rawLine}");
                            var settleSet = samples.Where(p => p.t >= 1.5).Select(p => p.x).ToList();
                            if (settleSet.Count < 3)
                            {
                                Both($"TRIAL cell={cell} trial={trial} {assignNote} zero={F(zeroDip, 2)} n={samples.Count} FAIL reason=no-settle-samples");
                                continue;
                            }
                            double settled = MedianS1(settleSet);
                            double span = settled - zeroDip;
                            int onsetIdx = samples.FindIndex(p => Math.Abs(p.x - zeroDip) > 1.0);
                            if (onsetIdx < 0)
                            {
                                Both($"TRIAL cell={cell} trial={trial} {assignNote} zero={F(zeroDip, 2)} settled={F(settled, 2)} n={samples.Count} FAIL reason=no-onset");
                                continue;
                            }
                            var window = samples.Skip(onsetIdx).Where(p => p.t <= 1.3).ToList();
                            if (window.Count < 3)
                            {
                                Both($"TRIAL cell={cell} trial={trial} {assignNote} zero={F(zeroDip, 2)} settled={F(settled, 2)} FAIL reason=window-too-few");
                                continue;
                            }
                            double peak = window.Max(p => p.x);
                            double trough = window.Min(p => p.x);
                            double overshoot = peak - settled;
                            double undershoot = zeroDip - trough;
                            bool ok = Math.Abs(span - 240) <= 2.0
                                && Math.Abs(overshoot - predOver) <= 2.0
                                && Math.Abs(undershoot - predUnder) <= 2.0;
                            if (cell == "test") { testOvers.Add(overshoot); testUnders.Add(undershoot); }
                            Both($"TRIAL cell={cell} trial={trial} {assignNote} zero={F(zeroDip, 2)} settled={F(settled, 2)} span={F(span, 2)} peak={F(peak, 2)} trough={F(trough, 2)} overshoot={F(overshoot, 2)} undershoot={F(undershoot, 2)} predOver={F(predOver, 2)} predUnder={F(predUnder, 2)} {(ok ? "PASS" : "FAIL")}");
                            if (ok) passes++;
                        }
                        catch (Exception ex)
                        {
                            if (!created) anyCreateThrow = true;
                            Both($"TRIAL cell={cell} trial={trial} assign THROW {ex.GetType().Name}: {ex.Message} FAIL reason={(created ? "trial-throw" : "create-throw")}");
                            try { CompReset(0, $"k2-overshoot {cell} t={trial} throw-end"); await Task.Delay(300); } catch { }
                        }
                    }
                }
                Both($"K2 OVERSHOOT VERDICT={(passes == total ? "PASS" : "FAIL")} passes={passes}/{total}");
                string conclusion;
                if (anyCreateThrow) conclusion = "REJECT (creation threw for out-of-range y; bezierOvershoot must become probe-driven, not a constant)";
                else if (testOvers.Count == 3 && testUnders.Count == 3 && testOvers.Average() <= 2.0 && testUnders.Average() <= 2.0) conclusion = "CLAMP (peaks match y-clamped curve; bezierOvershoot must become probe-driven, not a constant)";
                else if (passes == total) conclusion = "HONOR (bezierOvershoot=true stands)";
                else conclusion = "UNCLEAR (see TRIAL lines; bezierOvershoot stays probe-driven until clearer evidence)";
                Both("CONCLUSION: " + conclusion);
            }
            Both("===== K2 OVERSHOOT DONE =====");
            art.Close();
            Log($"K2 overshoot probe wrote {artPath}");
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("K2 OVERSHOOT EXCEPTION " + ex);
            try { art.Close(); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        finally
        {
            _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog();
            try { CloseMainWindowSoon(); } catch { }
        }
    }
}
