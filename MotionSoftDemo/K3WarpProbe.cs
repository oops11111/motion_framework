// Module 7 / MF-02 — K3 warp-isolation probe (follow-up to 0040).
//
// 0040 (K3 controller, REPORT-50e3ce09) FAILed 0/9: readbacks and
// end values all passed, but every playing trajectory missed the
// planner's SimulateKeyframes model with RMS 24-50 dips and all
// best-tau values on the +-100ms boundary. Offline analysis of
// win_k3_controller_raw.txt (9 trials x 120 samples) shows the
// clocks are honored -- P settles at 0.5s, R at 0.5s, V at 1.0s --
// but the VALUE mapping is warped by ONE function g of clock
// progress, the same in all three cells: g(0.5)=0.90 (the #22
// paused-seek warp, reproduced on the differenced scale),
// g(0.17)=0.30, g(0.33)=0.75, g(0.67)=0.95. g fits a cubic bezier
// with control points (0.406,0.442),(0.001,0.960) to RMS 0.004
// (a DESCRIPTION of the shape, not a mechanism). The mechanism is
// NOT established: the warp could come from the controller path,
// or from no-easing keyframe evaluation in general. No earlier
// probe discriminates them: K1/K2 keyframes carried explicit
// easings, and #22's no-easing keyframes all used controllers.
//
// This probe isolates the cause with two cells, same linear
// keyframes 0 -> 240 over 1.0s, 3 trials each:
//   N: NO controller (2-arg StartAnimation, the K1/K2 path).
//      Model: linear. If N is linear (RMS <= 3), the warp is
//      controller-specific. If N shows the same g, no-easing
//      keyframes are warped by default in this path.
//   P: controller, Progress=0.5, rate=1 (exact repeat of 0040
//      cell P). Model: 120 -> 240 over 0.5s. Reproducibility
//      anchor and within-run comparison with N.
// Gates per trial (S3 thresholds): RMS <= 3 dips, tau searched
// +-100ms, end value within 2 dips, Duration readback. The 0040
// FAIL is NOT re-judged here; this probe only isolates the warp.
// Flag: --k3-warp-probe / MF_K3_WARP_PROBE=1.
// Log: m60_k3_warp_ui.txt. Artifact: win_k3_warp_raw.txt.

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
    private async Task RunK3WarpProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = DevLog.Directory();
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_k3_warp_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        Both("# win_k3_warp_raw.txt — MF-02 K3 warp-isolation probe (0040 follow-up)");
        Both("# wall=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "; OS=" + Environment.OSVersion.Version);
        Both("# CELL linear 0->240 duration=1.0s; N: no controller; P: controller Progress=0.5 rate=1; trials=3 observe=2.0s");
        Both("# PREDICT N: linear 0->240 over 1.0s; P: 120->240 over 0.5s then hold (planner SimulateKeyframes, differenced to trial zero)");
        Both("# VERDICT rule: readbacks match, zero stable by capture polling, end value within 2 dips, RMS<=3 dips (tau +-100ms, best by model); no-calibration => INVALID");
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(120);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "k3-warp-probe");
            CompReset(0, "k3-warp start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            // Settle AFTER capture start so pre-calibration captures exist
            // (patch 0023 lesson).
            CompReset(0, "k3-warp cal settle"); await Task.Delay(400);
            // px/dip calibration, same as MAPTEST/S1/S2/S3/S4/S5/K2/K1/K3: 0 -> 37 dips,
            // medians of the last few captures in each window (patch 0026).
            var calQ0 = Qpc; CompReset(37, "k3-warp cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "k3-warp cal end"); await Task.Delay(500);
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
                Both("K3 WARP VERDICT=INVALID reason=no-calibration");
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
                var cells = new[] { "N", "P" };
                foreach (var cell in cells)
                {
                    for (int trial = 0; trial < 3; trial++)
                    {
                        total++;
                        CompReset(0, $"k3-warp {cell} t={trial} zero");
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
                            k.InsertKeyFrame(1f, 240f);
                            k.Duration = TimeSpan.FromSeconds(1.0);
                            TimeSpan readDur = k.Duration;
                            string assignNote = $"assign ok readback durationMs={F(readDur.TotalMilliseconds, 0)}";
                            long q0 = Qpc;
                            if (cell == "N")
                            {
                                _visual!.StartAnimation("Translation.X", k);
                            }
                            else
                            {
                                var ctrl = _comp!.CreateAnimationController();
                                ctrl.Progress = 0.5f;
                                ctrl.PlaybackRate = 1f;
                                float preProg = ctrl.Progress, preRate = ctrl.PlaybackRate;
                                _visual!.StartAnimation("Translation.X", k, ctrl);
                                float postProg = ctrl.Progress, postRate = ctrl.PlaybackRate;
                                assignNote += $" preProgress={F(preProg, 4)} preRate={F(preRate, 4)} postProgress={F(postProg, 4)} postRate={F(postRate, 4)}";
                                if (Math.Abs(preProg - 0.5f) > 1e-6 || Math.Abs(preRate - 1f) > 1e-6
                                    || Math.Abs(postProg - 0.5f) > 1e-6 || Math.Abs(postRate - 1f) > 1e-6)
                                {
                                    Both($"TRIAL cell={cell} trial={trial} {assignNote} FAIL reason=readback-mismatch");
                                    await Task.Delay(2000);
                                    CompReset(0, $"k3-warp {cell} t={trial} end");
                                    await Task.Delay(300);
                                    continue;
                                }
                            }
                            await Task.Delay(2000);
                            long qEnd = Qpc;
                            CompReset(0, $"k3-warp {cell} t={trial} end");
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
                            double Model(double t)
                            {
                                if (cell == "N") return zeroDip + 240 * Math.Clamp(t, 0, 1);
                                return zeroDip + 120 + 120 * Math.Clamp(t / 0.5, 0, 1);
                            }
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
                            try { CompReset(0, $"k3-warp {cell} t={trial} throw-end"); await Task.Delay(300); } catch { }
                        }
                    }
                }
                Both($"K3 WARP VERDICT={(passes == total ? "PASS" : "FAIL")} passes={passes}/{total}");
            }
            Both("===== K3 WARP DONE =====");
            art.Close();
            Log($"K3 warp probe wrote {artPath}");
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("K3 WARP EXCEPTION " + ex);
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
