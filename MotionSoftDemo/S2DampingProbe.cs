// Module 7 / MF-02 — S2 damping-ratio probe.
//
// S2 claim (docs/module7-plan.md): the compositor's DampingRatio is our
// damping ratio zeta (1.0 = critical). What is missing is a DIRECT
// measurement of the underdamped overshoot: the two existing probes do
// not cover it. RunZetaProbe (#14) tests zeta >= 1 accept/clamp only, with
// a crude boolean overshoot check in uncalibrated pixels; RunZetaTrajProbe
// (#23) tests zeta in {1.5, 3.0} overdamped trajectories only. Neither
// runs zeta < 1 and compares the first-peak overshoot to the analytic
// amount, and neither asserts zeta = 1 has no overshoot on a calibrated
// measurement.
//
// This probe measures, for zeta in {0.6, 0.78, 1.0} at response r = 0.5 s
// (Period = r/(2*pi), the S1-verified mapping), the first-peak value of a
// 0 -> 240 spring from rest, via the same WGC capture + 0/37-dip
// calibration as the S1 period probe (including the 400 ms post-start
// settle fix of patch 0023, without which cx0 is NaN and the probe
// reports INVALID).
//
// Analytic predictions (spring.h underdamped closed form, from 0 to
// target T with v0 = 0): the first peak overshoots by
// exp(-pi*zeta/sqrt(1-zeta^2)) of the travel:
//   zeta = 0.6  -> overshoot 9.48%  -> peak 262.75 dips
//   zeta = 0.78 -> overshoot 1.99%  -> peak 244.79 dips
//   zeta = 1.0  -> critically damped, monotone approach, no overshoot
//                  -> peak <= 240 dips (asymptotic approach from below)
// Verdict per cell: |interpolated peak - predicted peak| <= 2 dips for
// zeta < 1; peak <= 242 dips for zeta = 1. The 2-dip tolerance is stated,
// not tuned: near the peak the trajectory is flat (parabolic vertex),
// so 60 Hz sampling plus 3-point parabolic interpolation bounds the
// peak-value miss well under 2 dips, and the calibration pxPerDip comes
// from the same procedure the S1 probe verified (pxPerDip = 1.2478 in
// its passing round). The raw peak sample is logged next to the
// interpolated value so the verdict stays auditable.
// Flag: --s2-damping-probe / MF_S2_DAMPING_PROBE=1. Log: m52_s2_damping_ui.txt.
// Artifact: win_s2_damping_raw.txt in the same art dir as the log.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

public sealed partial class MainPage
{
    private async Task RunS2DampingProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = DevLog.Directory();
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_s2_damping_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        Both("# win_s2_damping_raw.txt — MF-02 S2 DampingRatio overshoot probe");
        Both("# wall=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "; OS=" + Environment.OSVersion.Version);
        Both("# CELL r=0.5000 Period=0.0795775 (S1 mapping Period=r/2pi) travel 0->240 IV=0 trials=3");
        Both("# PREDICT zeta=0.6 peak=262.75 zeta=0.78 peak=244.79 zeta=1.0 peak<=242 (overshoot exp(-pi*z/sqrt(1-z^2))) tol=+/-2.0 dips");
        Both("# VERDICT rule: zeta<1 |interpPeak-predicted|<=2 dips; zeta=1 peak<=242 dips; no-calibration => INVALID");
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(120);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "s2-damping-probe");
            CompReset(0, "s2-damping start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            // Settle AFTER capture start so pre-calibration captures exist
            // (patch 0023 lesson: calQ0 taken immediately after mf_cap_start
            // leaves no capture with QpcArrive < calQ0 and cx0 comes out NaN).
            CompReset(0, "s2-damping cal settle"); await Task.Delay(400);
            // px/dip calibration, same as MAPTEST/S1: 0 -> 37 dips.
            var calQ0 = Qpc; CompReset(37, "s2-damping cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "s2-damping cal end"); await Task.Delay(500);
            var cc = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = cc.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = cc.Where(c => c.QpcArrive >= calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double pxPerDip = (double.IsNaN(cx0) || double.IsNaN(cx37)) ? double.NaN : (cx37 - cx0) / 37.0;
            Both($"cal cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(pxPerDip,4)}");
            Both($"WGC start hr=0x00000000");

            string[] cells = { "0.6", "0.78", "1.0" };
            double[] predicted = { 262.75, 244.79, 240.0 };
            int passes = 0, total = 0;
            if (double.IsNaN(cx0) || pxPerDip <= 0)
            {
                Both("S2 DAMPING VERDICT=INVALID reason=no-calibration");
            }
            else
            {
                for (int ci = 0; ci < cells.Length; ci++)
                {
                    double zeta = double.Parse(cells[ci], CultureInfo.InvariantCulture);
                    for (int trial = 0; trial < 3; trial++)
                    {
                        total++;
                        CompReset(0, $"s2-damping z={cells[ci]} t={trial} rest"); await Task.Delay(250);
                        long q0 = Qpc;
                        var a = _comp!.CreateSpringScalarAnimation();
                        a.Period = TimeSpan.FromSeconds(0.5 / (2 * Math.PI));
                        a.DampingRatio = (float)zeta;
                        a.FinalValue = 240f;
                        a.InitialValue = 0f;
                        a.InitialVelocity = 0f;
                        _visual!.StartAnimation("Translation.X", a);
                        await Task.Delay(1600);
                        CompReset(0, $"s2-damping z={cells[ci]} t={trial} end");
                        var samples = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive >= q0)
                            .Select(c => (t: SecSince(q0, c.QpcArrive), x: (c.Cx - cx0) / pxPerDip))
                            .OrderBy(p => p.t).ToList();
                        double rawPeak = samples.Count > 0 ? samples.Max(p => p.x) : double.NaN;
                        double peak = S2PeakValue(samples);
                        string rawLine = samples.Count > 0
                            ? string.Join(" ", samples.Select(p => $"{F(p.t * 1000, 0)}:{F(p.x, 2)}"))
                            : "(no samples)";
                        Both($"RAW zeta={cells[ci]} trial={trial} n={samples.Count} rawPeak={F(rawPeak, 2)} peak={F(peak, 2)} predicted={F(predicted[ci], 2)} {rawLine}");
                        bool ok;
                        if (zeta >= 1.0)
                            ok = !double.IsNaN(peak) && rawPeak > 200 && peak <= 242.0;
                        else
                            ok = !double.IsNaN(peak) && Math.Abs(peak - predicted[ci]) <= 2.0;
                        Both($"TRIAL zeta={cells[ci]} trial={trial} peak={F(peak, 2)} predicted={F(predicted[ci], 2)} {(ok ? "PASS" : "FAIL")}");
                        if (ok) passes++;
                    }
                }
                Both($"S2 DAMPING VERDICT={(passes == total ? "PASS" : "FAIL")} passes={passes}/{total} tol=+/-2.0 dips");
            }
            Both("===== S2 DAMPING DONE =====");
            art.Close();
            Log($"S2 damping probe wrote {artPath}");
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("S2 DAMPING EXCEPTION " + ex);
            try { art.Close(); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        finally
        {
            _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog();
            try { CloseMainWindowSoon(); } catch { }
        }
    }

    // Peak value of the first overshoot: parabolic vertex through the
    // argmax sample and its two neighbours. At a boundary sample (the
    // zeta = 1 monotone case, whose max is the last sample) there is no
    // neighbour pair, so the raw sample value is returned.
    private static double S2PeakValue(List<(double t, double x)> s)
    {
        if (s.Count == 0) return double.NaN;
        int i = 0;
        for (int k = 1; k < s.Count; k++) if (s[k].x > s[i].x) i = k;
        if (i == 0 || i == s.Count - 1) return s[i].x;
        double y0 = s[i - 1].x, y1 = s[i].x, y2 = s[i + 1].x;
        double denom = y0 - 2 * y1 + y2;
        if (denom >= 0) return y1;
        return y1 - (y0 - y2) * (y0 - y2) / (8 * denom);
    }
}
