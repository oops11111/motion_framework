// Module 7 / MF-02 — S3 InitialVelocity probe (plan S3, still open).
//
// S3 claim (docs/module7-plan.md): `InitialVelocity` is in value-units
// per real second. Probe: start a compositor spring with a known initial
// velocity and compare the first sampled values with spring.h `At(t)`.
//
// What already exists does NOT cover this. M2DeltaVProbe /
// M2DeltaVPrimeProbe use nonzero InitialVelocity only as a RETARGET
// seed (velocity measured from a running WGC trace, testing retarget
// continuity), and forbid feeding mf_spring_at velocities into the seed
// (circular there). No probe starts a FRESH spring with a known IV and
// checks the units against the model. MAPTEST/S1V2/S1/S2 all run IV=0.
//
// This probe runs fresh 0 -> 240 springs at zeta = 0.78, response 0.5 s
// (Period = r/(2*pi), the S1-verified mapping) with IV in {0, +600,
// -600} dips/s, 3 trials each, and compares the first 200 ms of WGC
// samples against the native spring.h model via mf_spring_at (the same
// code the plan cites). Start latency is handled the way the K2 probe
// does it: a tau search over +-100 ms in 1 ms steps minimizing the RMS
// deviation from the true-IV model; the verdict then requires the
// true-IV RMS <= 3 dips AND, for nonzero cells, the rival IV=0 model
// (same tau, same samples) to miss by >= 10 dips, so the data must
// positively prefer the stated units over "IV ignored". The IV=0 cell
// is the control: it checks the machinery with true == rival.
// Calibration is the median 0/37-dip procedure with the calibration
// rest as the baseline (patches 0023/0026/0028: the springs here settle,
// and the per-trial lesson of 0026 does not apply to a never-settling
// predecessor — not the case here; the calibration rest is stable).
// Flag: --s3-iv-probe / MF_S3_IV_PROBE=1. Log: m54_s3_iv_ui.txt.
// Artifact: win_s3_iv_raw.txt in the same art dir as the log.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace MotionSoftDemo;

public sealed partial class MainPage
{
    private async Task RunS3IvProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = DevLog.Directory();
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_s3_iv_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        Both("# win_s3_iv_raw.txt — MF-02 S3 InitialVelocity units probe");
        Both("# wall=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "; OS=" + Environment.OSVersion.Version);
        Both("# CELL r=0.5000 zeta=0.78 Period=0.0795775 (S1 mapping) travel 0->240 IV={0,+600,-600} dips/s trials=3 window=0.20s");
        Both("# PREDICT model=mf_spring_at(r,zeta,0,240,IV,t); tau search +-100ms/1ms; tol rmsTrue<=3.0 dips; nonzero cells rival(IV=0)>=10 dips");
        Both("# VERDICT rule: readback=IV, >=5 window samples, rmsTrue<=3 dips, rival>=10 dips (nonzero cells); no-calibration => INVALID; TAU-BOUNDARY-HIT logged when bestTau hits +-100ms (observation only)");
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(120);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "s3-iv-probe");
            CompReset(0, "s3-iv start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            // Settle AFTER capture start so pre-calibration captures exist
            // (patch 0023 lesson).
            CompReset(0, "s3-iv cal settle"); await Task.Delay(400);
            // px/dip calibration, same as MAPTEST/S1/S2: 0 -> 37 dips,
            // medians of the last few captures in each window (patch 0026).
            var calQ0 = Qpc; CompReset(37, "s3-iv cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "s3-iv cal end"); await Task.Delay(500);
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
                Both("S3 IV VERDICT=INVALID reason=no-calibration");
            }
            else
            {
                double[] ivs = { 0, 600, -600 };
                foreach (double iv in ivs)
                {
                    for (int trial = 0; trial < 3; trial++)
                    {
                        total++;
                        CompReset(0, $"s3-iv IV={iv} t={trial} rest"); await Task.Delay(250);
                        long q0 = Qpc;
                        string assignNote;
                        float readback = float.NaN;
                        try
                        {
                            var a = _comp!.CreateSpringScalarAnimation();
                            a.Period = TimeSpan.FromSeconds(0.5 / (2 * Math.PI));
                            a.DampingRatio = 0.78f;
                            a.FinalValue = 240f;
                            a.InitialValue = 0f;
                            a.InitialVelocity = (float)iv;
                            readback = a.InitialVelocity;
                            assignNote = $"assign ok readback={readback.ToString("G9", CultureInfo.InvariantCulture)}";
                            _visual!.StartAnimation("Translation.X", a);
                        }
                        catch (Exception ex)
                        {
                            assignNote = $"assign THROW {ex.GetType().Name}: {ex.Message}";
                        }
                        await Task.Delay(1200);
                        CompReset(0, $"s3-iv IV={iv} t={trial} end");
                        if (float.IsNaN(readback) || readback != (float)iv)
                        {
                            Both($"TRIAL IV={iv} trial={trial} {assignNote} FAIL reason={(float.IsNaN(readback) ? "assign-throw" : "readback-mismatch")}");
                            continue;
                        }
                        var samples = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive >= q0)
                            .Select(c => (t: SecSince(q0, c.QpcArrive), x: (c.Cx - cx0) / pxPerDip))
                            .OrderBy(p => p.t).ToList();
                        // Tau search: minimize RMS vs the true-IV model over
                        // the first 200 ms; samples before the true start
                        // (t - tau < 0) are excluded. Range widened by 0030
                        // (+-33ms to +-100ms): the first round clipped at
                        // +33ms in 7/9 trials, so the window could not absorb
                        // the start latency. Gate and tolerance unchanged; a
                        // boundary hit is logged, not judged.
                        double bestTau = 0, bestRms = double.NaN;
                        int bestMs = 0;
                        List<(double t, double x)> bestSet = new();
                        for (int ms = -100; ms <= 100; ms++)
                        {
                            double tau = ms / 1000.0;
                            var set = samples.Where(p => p.t - tau >= 0.001 && p.t - tau <= 0.20).ToList();
                            if (set.Count < 5) continue;
                            double rms = S3IvRms(set, tau, iv);
                            if (double.IsNaN(rms)) continue;
                            if (double.IsNaN(bestRms) || rms < bestRms) { bestRms = rms; bestTau = tau; bestMs = ms; bestSet = set; }
                        }
                        if (Math.Abs(bestMs) >= 100)
                            Both($"TAU-BOUNDARY-HIT IV={iv} trial={trial} tau={F(bestTau * 1000, 0)}ms rmsTrue={F(bestRms, 2)}");
                        double rmsRival = double.NaN;
                        if (iv != 0 && bestSet.Count >= 5) rmsRival = S3IvRms(bestSet, bestTau, 0);
                        string rawLine = samples.Count > 0
                            ? string.Join(" ", samples.Select(p => $"{F(p.t * 1000, 0)}:{F(p.x, 2)}"))
                            : "(no samples)";
                        Both($"RAW IV={iv} trial={trial} n={samples.Count} nWin={bestSet.Count} tau={F(bestTau * 1000, 0)}ms rmsTrue={F(bestRms, 2)} rmsRival={F(rmsRival, 2)} {rawLine}");
                        bool ok = bestSet.Count >= 5 && !double.IsNaN(bestRms) && bestRms <= 3.0
                            && (iv == 0 || (!double.IsNaN(rmsRival) && rmsRival >= 10.0));
                        Both($"TRIAL IV={iv} trial={trial} {assignNote} tau={F(bestTau * 1000, 0)}ms rmsTrue={F(bestRms, 2)} rmsRival={F(rmsRival, 2)} {(ok ? "PASS" : "FAIL")}");
                        if (ok) passes++;
                    }
                }
                Both($"S3 IV VERDICT={(passes == total ? "PASS" : "FAIL")} passes={passes}/{total}");
            }
            Both("===== S3 IV DONE =====");
            art.Close();
            Log($"S3 IV probe wrote {artPath}");
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("S3 IV EXCEPTION " + ex);
            try { art.Close(); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        finally
        {
            _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog();
            try { CloseMainWindowSoon(); } catch { }
        }
    }

    // RMS deviation of measured samples from the native spring.h model
    // at the given initial velocity, with start-time offset tau.
    private static double S3IvRms(List<(double t, double x)> set, double tau, double iv)
    {
        double ss = 0;
        foreach (var p in set)
        {
            MfProbe.mf_spring_at(0.5, 0.78, 0, 240, iv, p.t - tau, out double model, out _);
            double d = p.x - model;
            ss += d * d;
        }
        return Math.Sqrt(ss / set.Count);
    }
}
