// Module 7 / MF-02 — K3 explicit-linear-easing probe (follow-up to
// 0040/0041).
//
// 0040 FAILed 0/9 and 0041 FAILed 0/6. Offline analysis of BOTH
// raw files shows the same thing: the animation CLOCKS are
// honored (P/R settle at 0.5s, V at 1.0s), but the value mapping
// follows ONE warp function g of clock progress in every cell,
// with or without a controller -- 0041 cell N (NO controller,
// no easing) fits the 0040-fitted g to RMS 2.47 dips at tau
// 16ms, i.e. inside the 3-dip gate, while the linear model
// misses by 47 dips. K1/K2 keyframes carried EXPLICIT easings
// and fit their easing models; no earlier probe ran a
// no-easing keyframe trajectory with gates. So the remaining
// question is narrow: does an EXPLICIT linear easing restore
// linearity, or is the warp applied regardless of easing?
//
// This probe has two cells, same linear keyframes 0 -> 240
// over 1.0s, 3 trials each, no controller in either:
//   L: end keyframe carries an explicit LinearEasingFunction
//      (Compositor.CreateLinearEasingFunction, 3-arg
//      InsertKeyFrame, the K1/K2 pattern). Model: linear.
//      If L is linear (RMS <= 3), the warp is the DEFAULT
//      evaluation of no-easing keyframes in this path, and
//      the planner must attach explicit linear easing;
//      EvaluateKeyframes' "no easing = linear" needs a
//      probe-driven correction.
//   N: no easing (exact repeat of 0041 cell N). Model:
//      linear (expected to FAIL with the g shape, as in
//      0041). Reproducibility anchor and within-run control.
// Gates per trial (S3 thresholds): RMS <= 3 dips, tau
// searched +-100ms, end value within 2 dips, Duration
// readback. The 0040/0041 FAILs are NOT re-judged here.
// Flag: --k3-ease-probe / MF_K3_EASE_PROBE=1.
// Log: m61_k3_ease_ui.txt. Artifact: win_k3_ease_raw.txt.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;

namespace MotionSoftDemo;

public sealed partial class MainPage
{
    private async Task RunK3EaseProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = DevLog.Directory();
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_k3_ease_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        Both("# win_k3_ease_raw.txt — MF-02 K3 explicit-linear-easing probe (0040/0041 follow-up)");
        Both("# wall=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "; OS=" + Environment.OSVersion.Version);
        Both("# CELL linear 0->240 duration=1.0s; L: explicit LinearEasingFunction; N: no easing; trials=3 observe=2.0s; no controller");
        Both("# PREDICT both cells: linear 0->240 over 1.0s (planner SimulateKeyframes, differenced to trial zero)");
        Both("# VERDICT rule: readbacks match, zero stable by capture polling, end value within 2 dips, RMS<=3 dips (tau +-100ms, best by model); no-calibration => INVALID");
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(120);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "k3-ease-probe");
            CompReset(0, "k3-ease start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            // Settle AFTER capture start so pre-calibration captures exist
            // (patch 0023 lesson).
            CompReset(0, "k3-ease cal settle"); await Task.Delay(400);
            // px/dip calibration, same as MAPTEST/S1/S2/S3/S4/S5/K2/K1/K3: 0 -> 37 dips,
            // medians of the last few captures in each window (patch 0026).
            var calQ0 = Qpc; CompReset(37, "k3-ease cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "k3-ease cal end"); await Task.Delay(500);
            var cc = ReadCaptures().Where(c => c.N > 50).ToList();
            var preCal = cc.Where(c => c.QpcArrive < calQ0).Select(c => c.Cx).ToList();
            var midCal = cc.Where(c => c.QpcArrive >= calQ0 && c.QpcArrive < calQ1).Select(c => c.Cx).ToList();
            double cx0 = preCal.Count > 0 ? MedianS1(preCal.Skip(Math.Max(0, preCal.Count - 5)).ToList()) : double.NaN;
            double cx37 = midCal.Count > 0 ? MedianS1(midCal.Skip(Math.Max(0, midCal.Count - 5)).ToList()) : double.NaN;
            double pxPerDip = (double.IsNaN(cx0) || double.IsNaN(cx37)) ? double.NaN : (cx37 - cx0) / 37.0;
            Both($"cal cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(pxPerDip,4)} nPre={preCal.Count} nMid={midCal.Count}");

            int passes = 0, total = 0;
            if (double.IsNaN(cx0) || pxPerDip <= 0)
            {
                Both("K3 EASE VERDICT=INVALID reason=no-calibration");
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
                var cells = new[] { "L", "N" };
                foreach (var cell in cells)
                {
                    for (int trial = 0; trial < 3; trial++)
                    {
                        total++;
                        CompReset(0, $"k3-ease {cell} t={trial} zero");
                        long qZero = Qpc;
                        var (zeroDip, zeroPx, zeroWait, zeroStable) = await PollStable(qZero);
                        Both($"ZERO cell={cell} trial={trial} waitedMs={zeroWait} zero={F(zeroDip, 2)} zeroPx={F(zeroPx, 2)} stable={zeroStable}");
                        if (!zeroStable || double.IsNaN(zeroDip))
                        {
                            Both($"TRIAL cell={cell} trial={trial} zero={F(zeroDip, 2)} FAIL reason=zero-not-stable");
                            continue;
                        }
                        try
                        {
                            var k = _comp!.CreateScalarKeyFrameAnimation();
                            k.InsertKeyFrame(0f, 0f);
                            if (cell == "L")
                            {
                                var linearEase = _comp!.CreateLinearEasingFunction();
                                k.InsertKeyFrame(1f, 240f, linearEase);
                            }
                            else
                            {
                                k.InsertKeyFrame(1f, 240f);
                            }
                            k.Duration = TimeSpan.FromSeconds(1.0);
                            TimeSpan readDur = k.Duration;
                            string assignNote = $"assign ok readback durationMs={F(readDur.TotalMilliseconds, 0)}";
                            long q0 = Qpc;
                            _visual!.StartAnimation("Translation.X", k);
                            await Task.Delay(2000);
                            long qEnd = Qpc;
                            CompReset(0, $"k3-ease {cell} t={trial} end");
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
                            double expectEnd = zeroDip + 240;
                            double Model(double t) => zeroDip + 240 * Math.Clamp(t, 0, 1);
                            double Rms(double tau)
                            {
                                var e = samples.Where(p => p.t - tau > 0 && p.t - tau < 1.05)
                                    .Select(p => p.x - Model(p.t - tau)).ToList();
                                return e.Count < 5 ? double.NaN : Math.Sqrt(e.Average(z => z * z));
                            }
                            // K1 lesson: a NaN first RMS must not
                            // poison the search; start from +Inf.
                            double bestTau = 0, bestRms = double.PositiveInfinity;
                            bool found = false;
                            for (int ms = -100; ms <= 100; ms++)
                            {
                                double r = Rms(ms / 1000.0);
                                if (!double.IsNaN(r) && r < bestRms) { bestRms = r; bestTau = ms / 1000.0; found = true; }
                            }
                            if (!found)
                            {
                                Both($"TRIAL cell={cell} trial={trial} {assignNote} zero={F(zeroDip, 2)} n={samples.Count} FAIL reason=no-rms-samples");
                                continue;
                            }
                            bool ok = Math.Abs(settled - expectEnd) <= 2.0 && bestRms <= 3.0;
                            Both($"TRIAL cell={cell} trial={trial} {assignNote} zero={F(zeroDip, 2)} settled={F(settled, 2)} expectEnd={F(expectEnd, 2)} tau={F(bestTau * 1000, 0)}ms rms={F(bestRms, 2)} {(ok ? "PASS" : "FAIL")}");
                            if (ok) passes++;
                        }
                        catch (Exception ex)
                        {
                            Both($"TRIAL cell={cell} trial={trial} assign THROW {ex.GetType().Name}: {ex.Message} FAIL reason=trial-throw");
                            try { _visual!.StopAnimation("Translation.X"); } catch { }
                            try { CompReset(0, $"k3-ease {cell} t={trial} throw-end"); await Task.Delay(300); } catch { }
                        }
                    }
                }
                Both($"K3 EASE VERDICT={(passes == total ? "PASS" : "FAIL")} passes={passes}/{total}");
            }
            Both("===== K3 EASE DONE =====");
            art.Close();
            Log($"K3 ease probe wrote {artPath}");
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("K3 EASE EXCEPTION " + ex);
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
