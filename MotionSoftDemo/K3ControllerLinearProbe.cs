// Module 7 / MF-02 — K3 controller probe, second run with explicit
// linear easing (follow-up to 0040/0041/0042).
//
// 0040 FAILed 0/9 on trajectory RMS. 0041 showed the warp is
// NOT controller-specific (no-controller cell N fit the same
// warp g). 0042 showed the warp IS easing-specific: explicit
// LinearEasingFunction restores linearity (L 3/3 PASS, RMS
// 1.23-1.24), no-easing keyframes stay warped (N 0/3). So the
// 0040 FAIL is explained by the no-easing default evaluation,
// NOT by the controller: the clocks were honored all along.
// What 0040 never tested is controller semantics with LINEAR
// keyframes. This probe repeats 0040's P/R/V/S cells exactly,
// with an explicit LinearEasingFunction on the end keyframe
// (the 0042 pattern) in every cell including S. If P/R/V pass
// their planner models here, K3 closes: Progress/rate/reverse
// are honored per SimulateKeyframes. S is record-only as in
// 0040: with linear easing, a paused seek to 0.5 should show
// ~118 (linear midpoint), not the ~214 warp; if S still shows
// ~214, the paused-seek warp is a SEPARATE mechanism.
// Cells (linear keyframes 0 -> 240, 1.0s, 3 trials each):
//   P: Progress=0.5, rate=1 (120 -> 240 over 0.5s, then hold)
//   R: Progress=0, rate=2 (0 -> 240 over 0.5s)
//   V: Progress=1, rate=-1 (240 -> 0 over 1.0s)
//   S: paused seek record-only (rate=0 + Pause, Progress=0.5)
// Controller Progress/PlaybackRate are set BEFORE
// StartAnimation, read back before AND immediately after
// start (a change FAILs with reason=controller-reset,
// distinguishable from a trajectory failure); Duration
// readback gated; fresh-zero differencing per trial (S4
// 0036 method); tau +-100ms best-by-model, RMS <= 3 dips
// (S3 thresholds). The 0040/0041/0042 FAILs are NOT
// re-judged here.
// Flag: --k3-controller-linear-probe /
// MF_K3_CONTROLLER_LINEAR_PROBE=1.
// Log: m62_k3_controller_linear_ui.txt.
// Artifact: win_k3_controller_linear_raw.txt.

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
    private async Task RunK3ControllerLinearProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = DevLog.Directory();
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_k3_controller_linear_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        Both("# win_k3_controller_linear_raw.txt — MF-02 K3 controller Progress/rate/reverse probe");
        Both("# wall=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "; OS=" + Environment.OSVersion.Version);
        Both("# CELL linear 0->240 duration=1.0s; P: Progress=0.5 rate=1; R: Progress=0 rate=2; V: Progress=1 rate=-1; S: paused seek 0.5 record-only; trials=3 observe=2.0s");
        Both("# PREDICT P: 120->240 over 0.5s then hold; R: 0->240 over 0.5s; V: 240->0 over 1.0s (planner SimulateKeyframes, differenced to trial zero)");
        Both("# VERDICT rule: readbacks match (Duration + controller pre/post start), zero stable by capture polling, end value within 2 dips, RMS<=3 dips (tau +-100ms, best by model); no-calibration => INVALID");
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(120);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "k3-controller-linear-probe");
            CompReset(0, "k3-controller start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            // Settle AFTER capture start so pre-calibration captures exist
            // (patch 0023 lesson).
            CompReset(0, "k3-controller cal settle"); await Task.Delay(400);
            // px/dip calibration, same as MAPTEST/S1/S2/S3/S4/S5/K2/K1: 0 -> 37 dips,
            // medians of the last few captures in each window (patch 0026).
            var calQ0 = Qpc; CompReset(37, "k3-controller cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "k3-controller cal end"); await Task.Delay(500);
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
                Both("K3 CONTROLLER LINEAR VERDICT=INVALID reason=no-calibration");
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
                var cells = new[] { ("P", 0.5f, 1f), ("R", 0.0f, 2f), ("V", 1.0f, -1f) };
                foreach (var (cell, prog, rate) in cells)
                {
                    for (int trial = 0; trial < 3; trial++)
                    {
                        total++;
                        CompReset(0, $"k3-controller {cell} t={trial} zero");
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
                            var linearEase = _comp!.CreateLinearEasingFunction();
                            k.InsertKeyFrame(1f, 240f, linearEase);
                            k.Duration = TimeSpan.FromSeconds(1.0);
                            var ctrl = _comp!.CreateAnimationController();
                            ctrl.Progress = prog;
                            ctrl.PlaybackRate = rate;
                            float preProg = ctrl.Progress, preRate = ctrl.PlaybackRate;
                            TimeSpan readDur = k.Duration;
                            string assignNote = $"assign ok readback durationMs={F(readDur.TotalMilliseconds, 0)} preProgress={F(preProg, 4)} preRate={F(preRate, 4)}";
                            long q0 = Qpc;
                            _visual!.StartAnimation("Translation.X", k, ctrl);
                            float postProg = ctrl.Progress, postRate = ctrl.PlaybackRate;
                            assignNote += $" postProgress={F(postProg, 4)} postRate={F(postRate, 4)}";
                            await Task.Delay(2000);
                            long qEnd = Qpc;
                            CompReset(0, $"k3-controller {cell} t={trial} end");
                            // Let the stop present before the next
                            // trial's zero poll (S4 0037 lesson).
                            await Task.Delay(300);
                            if (readDur != TimeSpan.FromSeconds(1.0) || Math.Abs(preProg - prog) > 1e-6 || Math.Abs(preRate - rate) > 1e-6)
                            {
                                Both($"TRIAL cell={cell} trial={trial} {assignNote} FAIL reason=readback-mismatch");
                                continue;
                            }
                            if (Math.Abs(postProg - prog) > 1e-6 || Math.Abs(postRate - rate) > 1e-6)
                            {
                                Both($"TRIAL cell={cell} trial={trial} {assignNote} FAIL reason=controller-reset");
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
                            double expectEnd = cell == "V" ? zeroDip : zeroDip + 240;
                            double Model(double t)
                            {
                                if (cell == "P") return zeroDip + 120 + 120 * Math.Clamp(t / 0.5, 0, 1);
                                if (cell == "R") return zeroDip + 240 * Math.Clamp(2 * t, 0, 1);
                                return zeroDip + 240 * (1 - Math.Clamp(t, 0, 1));
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
                            try { CompReset(0, $"k3-controller {cell} t={trial} throw-end"); await Task.Delay(300); } catch { }
                        }
                    }
                }
                // Cell S: paused seek, record-only (#22 sequencing,
                // #22 warp noted; no gate).
                {
                    CompReset(0, "k3-controller S zero");
                    long qZero = Qpc;
                    var (zeroDip, zeroPx, zeroWait, zeroStable) = await PollStable(qZero);
                    Both($"ZERO cell=S waitedMs={zeroWait} zero={F(zeroDip, 2)} zeroPx={F(zeroPx, 2)} stable={zeroStable}");
                    try
                    {
                        var k = _comp!.CreateScalarKeyFrameAnimation();
                        k.InsertKeyFrame(0f, 0f);
                        var linearEase = _comp!.CreateLinearEasingFunction();
                        k.InsertKeyFrame(1f, 240f, linearEase);
                        k.Duration = TimeSpan.FromSeconds(1.0);
                        var ctrl = _comp!.CreateAnimationController();
                        ctrl.PlaybackRate = 0f;
                        _visual!.StartAnimation("Translation.X", k, ctrl);
                        ctrl.Pause();
                        await Task.Delay(100);
                        ctrl.Progress = 0.5f;
                        ctrl.Pause();
                        await Task.Delay(100);
                        long a = Qpc;
                        await Task.Delay(150);
                        long b = Qpc;
                        var xs = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive >= a && c.QpcArrive <= b)
                            .Select(c => (c.Cx - cx0) / pxPerDip).ToList();
                        double med = xs.Count > 0 ? MedianS1(xs) : double.NaN;
                        Both($"SEEK cell=S record-only progress={F(ctrl.Progress, 4)} rate={F(ctrl.PlaybackRate, 2)} zero={F(zeroDip, 2)} heldMedian={F(med, 2)} n={xs.Count} (linear prediction {F(zeroDip + 120, 2)}; #22 warp noted, no gate)");
                        try { _visual!.StopAnimation("Translation.X"); } catch { }
                        CompReset(0, "k3-controller S end");
                        await Task.Delay(300);
                    }
                    catch (Exception ex)
                    {
                        Both($"SEEK cell=S THROW {ex.GetType().Name}: {ex.Message} (record-only)");
                        try { _visual!.StopAnimation("Translation.X"); } catch { }
                    }
                }
                Both($"K3 CONTROLLER LINEAR VERDICT={(passes == total ? "PASS" : "FAIL")} passes={passes}/{total}");
            }
            Both("===== K3 CONTROLLER LINEAR DONE =====");
            art.Close();
            Log($"K3 controller probe wrote {artPath}");
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("K3 CONTROLLER LINEAR EXCEPTION " + ex);
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
