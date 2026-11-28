// Module 7 / MF-02 — K1 keyframe-easing-segment probe.
//
// K1 claim (docs/module7-plan.md, unverified): "a keyframe's
// easing shapes the segment ENDING at that keyframe. Probe:
// two-segment keyframe animation, sample both segments." The
// planner implements exactly this (composition.h
// EvaluateKeyframes: the easing of `b`, the keyframe ending the
// segment, shapes the segment). No Windows report for the old
// #19 probe (RunK1Probe, --k1-probe) is on file, so the claim
// has no target-OS evidence.
//
// What #19 does NOT establish: single-sample calibration with a
// RasterizationScale fallback and no INVALID guard; absolute
// cx0-scale values (the S4 zero-bias lesson); 220/440ms durations
// (13/26 frames); easing taken from UI fields; a rival model
// (easing over the whole duration) that is not the real
// alternative, and a verdict that never requires beating the
// linear model; percentage-of-travel gates instead of dips; no
// readbacks; no passes=N/N verdict line.
//
// This probe compares the two readings directly, S3-style, with
// the S1/S2/S3 discipline plus the S4 lessons: median
// calibration, per-trial differenced measurement (models relative
// to the trial's own rest baseline, so any cx0 additive bias
// cancels), tau search, and a stated tolerance. Two segments,
// 0 -> 120 -> 240 over 1.0s. Keyframe @0.5 carries easeIn
// (0.42,0)-(1,1); keyframe @1.0 carries easeOut (0,0)-(0.58,1);
// keyframe @0 carries no easing. ENDING reading: segment 1
// follows easeIn, segment 2 follows easeOut. STARTING reading:
// segment 1 follows linear (keyframe @0 has no easing), segment
// 2 follows easeIn. The readings differ by up to 44 dips
// (segment means 15/30 dips), so the rival comparison is
// decisive. Gates per trial (S3 thresholds): RMS vs ENDING <= 3
// dips and RMS vs STARTING >= 10 dips, with tau searched
// +-100ms in 1ms steps (best tau by the ENDING model). The
// models are continuous at the segment boundary (both give 120
// at u=0.5), so one combined RMS over both segments is clean.
// Duration is read back. Control-point readback is deliberately
// NOT attempted (no repo code reads those properties; the
// trajectory comparison subsumes it).
// Flag: --k1-segment-probe / MF_K1_SEGMENT_PROBE=1.
// Log: m58_k1_segment_ui.txt. Artifact: win_k1_segment_raw.txt.

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
    private async Task RunK1SegmentProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = DevLog.Directory();
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_k1_segment_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        Both("# win_k1_segment_raw.txt — MF-02 K1 keyframe-easing-segment probe");
        Both("# wall=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "; OS=" + Environment.OSVersion.Version);
        Both("# CELL keys (0.0,0,none) (0.5,120,easeIn 0.42,0-1,1) (1.0,240,easeOut 0,0-0.58,1) duration=1.0s trials=3 observe=2.0s");
        Both("# PREDICT ENDING: seg1 easeIn, seg2 easeOut; STARTING (rival): seg1 linear, seg2 easeIn; max model gap 44.3 dips");
        Both("# VERDICT rule: readbacks match, zero stable by capture polling, |settled-zero-240|<=2 dips, RMS-ending<=3 dips, RMS-starting>=10 dips (tau +-100ms, best by ENDING); no-calibration => INVALID");
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(120);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "k1-segment-probe");
            CompReset(0, "k1-segment start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            // Settle AFTER capture start so pre-calibration captures exist
            // (patch 0023 lesson).
            CompReset(0, "k1-segment cal settle"); await Task.Delay(400);
            // px/dip calibration, same as MAPTEST/S1/S2/S3/S4/S5/K2: 0 -> 37 dips,
            // medians of the last few captures in each window (patch 0026).
            var calQ0 = Qpc; CompReset(37, "k1-segment cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "k1-segment cal end"); await Task.Delay(500);
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
                Both("K1 SEGMENT VERDICT=INVALID reason=no-calibration");
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
                double ModelEnding(double u, double zero)
                {
                    if (u <= 0.5) return zero + 120 * BezierEase(u / 0.5, 0.42, 0.0, 1.0, 1.0);
                    return zero + 120 + 120 * BezierEase((u - 0.5) / 0.5, 0.0, 0.0, 0.58, 1.0);
                }
                double ModelStarting(double u, double zero)
                {
                    if (u <= 0.5) return zero + 120 * (u / 0.5);
                    return zero + 120 + 120 * BezierEase((u - 0.5) / 0.5, 0.42, 0.0, 1.0, 1.0);
                }
                for (int trial = 0; trial < 3; trial++)
                {
                    total++;
                    CompReset(0, $"k1-segment t={trial} zero");
                    long qZero = Qpc;
                    var (zeroDip, zeroPx, zeroWait, zeroStable) = await PollStable(qZero);
                    Both($"ZERO trial={trial} waitedMs={zeroWait} zero={F(zeroDip, 2)} zeroPx={F(zeroPx, 2)} stable={zeroStable}");
                    if (!zeroStable || double.IsNaN(zeroDip))
                    {
                        Both($"TRIAL trial={trial} zero={F(zeroDip, 2)} FAIL reason=zero-not-stable");
                        continue;
                    }
                    TimeSpan readDur = TimeSpan.MinValue;
                    try
                    {
                        var easeIn = _comp!.CreateCubicBezierEasingFunction(new Vector2(0.42f, 0f), new Vector2(1f, 1f));
                        var easeOut = _comp!.CreateCubicBezierEasingFunction(new Vector2(0f, 0f), new Vector2(0.58f, 1f));
                        var k = _comp!.CreateScalarKeyFrameAnimation();
                        k.InsertKeyFrame(0f, 0f);
                        k.InsertKeyFrame(0.5f, 120f, easeIn);
                        k.InsertKeyFrame(1f, 240f, easeOut);
                        k.Duration = TimeSpan.FromSeconds(1.0);
                        readDur = k.Duration;
                        string assignNote = $"assign ok readback durationMs={F(readDur.TotalMilliseconds, 0)}";
                        long q0 = Qpc;
                        _visual!.StartAnimation("Translation.X", k);
                        await Task.Delay(2000);
                        long qEnd = Qpc;
                        CompReset(0, $"k1-segment t={trial} end");
                        // Let the stop present before the next
                        // trial's zero poll (S4 0037 lesson).
                        await Task.Delay(300);
                        if (readDur != TimeSpan.FromSeconds(1.0))
                        {
                            Both($"TRIAL trial={trial} {assignNote} FAIL reason=readback-mismatch");
                            continue;
                        }
                        var samples = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive >= q0 && c.QpcArrive <= qEnd)
                            .Select(c => (t: SecSince(q0, c.QpcArrive), x: (c.Cx - cx0) / pxPerDip))
                            .OrderBy(p => p.t).ToList();
                        string rawLine = samples.Count > 0
                            ? string.Join(" ", samples.Select(p => $"{F(p.t * 1000, 0)}:{F(p.x, 2)}"))
                            : "(no samples)";
                        Both($"RAW trial={trial} n={samples.Count} zero={F(zeroDip, 2)} {rawLine}");
                        var settleSet = samples.Where(p => p.t >= 1.5).Select(p => p.x).ToList();
                        if (settleSet.Count < 3)
                        {
                            Both($"TRIAL trial={trial} {assignNote} zero={F(zeroDip, 2)} n={samples.Count} FAIL reason=no-settle-samples");
                            continue;
                        }
                        double settled = MedianS1(settleSet);
                        double span = settled - zeroDip;
                        double Rms(Func<double, double> model, double tau)
                        {
                            var e = samples.Where(p => p.t - tau > 0 && p.t - tau < 1.05)
                                .Select(p => p.x - model(p.t - tau)).ToList();
                            return e.Count < 5 ? double.NaN : Math.Sqrt(e.Average(z => z * z));
                        }
                        double bestTau = 0, bestEnd = Rms(t => ModelEnding(t, zeroDip), 0);
                        for (int ms = -100; ms <= 100; ms++)
                        {
                            double r = Rms(t => ModelEnding(t, zeroDip), ms / 1000.0);
                            if (!double.IsNaN(r) && r < bestEnd) { bestEnd = r; bestTau = ms / 1000.0; }
                        }
                        double rmsStart = Rms(t => ModelStarting(t, zeroDip), bestTau);
                        // Per-segment diagnostic RMS at the best tau.
                        double SegRms(Func<double, double> model, double tLo, double tHi)
                        {
                            var e = samples.Where(p => { double tc = p.t - bestTau; return tc > tLo && tc <= tHi; })
                                .Select(p => p.x - model(p.t - bestTau)).ToList();
                            return e.Count < 3 ? double.NaN : Math.Sqrt(e.Average(z => z * z));
                        }
                        double rmsEndSeg1 = SegRms(t => ModelEnding(t, zeroDip), 0, 0.5);
                        double rmsEndSeg2 = SegRms(t => ModelEnding(t, zeroDip), 0.5, 1.0);
                        bool ok = Math.Abs(span - 240) <= 2.0 && bestEnd <= 3.0 && rmsStart >= 10.0;
                        Both($"TRIAL trial={trial} {assignNote} zero={F(zeroDip, 2)} settled={F(settled, 2)} span={F(span, 2)} tau={F(bestTau * 1000, 0)}ms rmsEnding={F(bestEnd, 2)} rmsEndingSeg1={F(rmsEndSeg1, 2)} rmsEndingSeg2={F(rmsEndSeg2, 2)} rmsStarting={F(rmsStart, 2)} {(ok ? "PASS" : "FAIL")}");
                        if (ok) passes++;
                    }
                    catch (Exception ex)
                    {
                        Both($"TRIAL trial={trial} assign THROW {ex.GetType().Name}: {ex.Message} FAIL reason=trial-throw");
                        try { CompReset(0, $"k1-segment t={trial} throw-end"); await Task.Delay(300); } catch { }
                    }
                }
                Both($"K1 SEGMENT VERDICT={(passes == total ? "PASS" : "FAIL")} passes={passes}/{total}");
            }
            Both("===== K1 SEGMENT DONE =====");
            art.Close();
            Log($"K1 segment probe wrote {artPath}");
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("K1 SEGMENT EXCEPTION " + ex);
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
