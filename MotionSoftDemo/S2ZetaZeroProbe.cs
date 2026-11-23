// Module 7 / MF-02 — S2 edge probe: DampingRatio = 0 degeneration.
//
// This is NOT plan S3 (InitialVelocity units, still open). It is the
// undamped edge of the S2 mapping: DampingRatio = 0 must degenerate to
// the undamped spring formula. spring.h covers zeta = 0 in its
// underdamped branch with no special-casing (a = 0, wd = w0), and
// MapSpringToCompositor passes damping through unchanged
// (dampingScale = 1.0), so the prediction is:
//
//   x(t) = 240 * (1 - cos(w0 t)), w0 = 2*pi/response, response = 0.5 s
//   first peak 480 dips at t = 0.25 s, period 0.5 s, no decay, troughs
//   back at the rest level. spring.h SettleTime returns its cap for
//   undamped springs ("never settle").
//
// The probe measures, for 3 trials of a 0 -> 240 spring from rest at
// zeta = 0, Period = r/(2*pi) (the S1-verified mapping): the readback
// of DampingRatio, the peak-to-peak period, the first-peak value, the
// last/first peak ratio (no decay), and the trough level — via the same
// WGC capture + 0/37-dip median calibration and per-trial rest baseline
// as the S2 probe (patch 0026). If the API throws on 0 or clamps the
// readback, that is the finding: the trial FAILs with the reason
// logged, and no oscillation data is claimed for it.
// 0028 revision: the first Windows round (REPORT-cc9f10dd) FAILed 0/3
// on a measurement artifact — the per-trial rest baseline. An undamped
// predecessor never settles, so the pre-start captures of trials 1-2
// were mid-flight (rest ~419px) while their absolute oscillation
// extremes matched trial 0 exactly. The baseline is now the
// calibration rest for all trials (trial 0's rest equalled cx0), and
// the RAW line also logs the raw max sample so a peak shortfall can be
// told apart from estimator bias. Gates unchanged.
//
// Flag: --s2-zero-damping-probe / MF_S2_ZERO_DAMPING_PROBE=1.
// Log: m53_s2_zero_damping_ui.txt.
// Artifact: win_s2_zero_damping_raw.txt in the same art dir as the log.

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
    private async Task RunS2ZetaZeroProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = DevLog.Directory();
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_s2_zero_damping_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        Both("# win_s2_zero_damping_raw.txt — MF-02 S2 edge: DampingRatio=0 undamped probe");
        Both("# wall=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "; OS=" + Environment.OSVersion.Version);
        Both("# CELL r=0.5000 Period=0.0795775 (S1 mapping Period=r/2pi) travel 0->240 IV=0 zeta=0 trials=3 record=2.6s");
        Both("# PREDICT period=0.5000s firstPeak=480.00 trough=0.00 no-decay lastPeak>=0.95*firstPeak tol period=+/-5.0% values=+/-2.0 dips");
        Both("# VERDICT rule: readback=0, >=3 peaks, median interval within 5% of 0.5s, first peak within 2 dips of 480, last/first>=0.95, |min|<=2 dips; no-calibration => INVALID");
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(120);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "s2-zero-damping-probe");
            CompReset(0, "s2-zero start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            // Settle AFTER capture start so pre-calibration captures exist
            // (patch 0023 lesson: calQ0 taken immediately after mf_cap_start
            // leaves no capture with QpcArrive < calQ0 and cx0 comes out NaN).
            CompReset(0, "s2-zero cal settle"); await Task.Delay(400);
            // px/dip calibration, same as MAPTEST/S1/S2: 0 -> 37 dips,
            // medians of the last few captures in each window (patch 0026).
            var calQ0 = Qpc; CompReset(37, "s2-zero cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "s2-zero cal end"); await Task.Delay(500);
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
                Both("S2 ZERO DAMPING VERDICT=INVALID reason=no-calibration");
            }
            else
            {
                for (int trial = 0; trial < 3; trial++)
                {
                    total++;
                    CompReset(0, $"s2-zero t={trial} rest"); await Task.Delay(250);
                    // Baseline is the CALIBRATION rest (cx0), NOT a
                    // per-trial pre-start baseline: the previous trial's
                    // undamped spring never settles, so pre-start captures
                    // are mid-flight, not rest. First round proved it:
                    // trials 1-2 read rest ~419px (mid-flight) while the
                    // absolute oscillation extremes matched trial 0
                    // exactly; trial 0's own rest (127.68) equalled cx0
                    // (127.700). The 0026 per-trial lesson does not
                    // transfer to a spring that never rests.
                    double rest = cx0;
                    long q0 = Qpc;
                    string assignNote;
                    float readback = float.NaN;
                    try
                    {
                        var a = _comp!.CreateSpringScalarAnimation();
                        a.Period = TimeSpan.FromSeconds(0.5 / (2 * Math.PI));
                        a.DampingRatio = 0f;
                        a.FinalValue = 240f;
                        a.InitialValue = 0f;
                        a.InitialVelocity = 0f;
                        readback = a.DampingRatio;
                        assignNote = $"assign ok readback={readback.ToString("G9", CultureInfo.InvariantCulture)}";
                        _visual!.StartAnimation("Translation.X", a);
                    }
                    catch (Exception ex)
                    {
                        assignNote = $"assign THROW {ex.GetType().Name}: {ex.Message}";
                    }
                    Both($"TRIAL trial={trial} {assignNote} rest={F(rest, 2)}");
                    if (float.IsNaN(readback) || readback != 0f)
                    {
                        Both($"TRIAL trial={trial} FAIL reason={(float.IsNaN(readback) ? "assign-throw" : $"readback-clamped-to-{readback.ToString("G9", CultureInfo.InvariantCulture)}")}");
                        continue;
                    }
                    await Task.Delay(2600);
                    CompReset(0, $"s2-zero t={trial} end");
                    var samples = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive >= q0)
                        .Select(c => (t: SecSince(q0, c.QpcArrive), x: (c.Cx - rest) / pxPerDip))
                        .OrderBy(p => p.t).ToList();
                    var peaks = S2ZeroPeaks(samples);
                    double rawPeak = samples.Count > 0 ? samples.Max(p => p.x) : double.NaN;
                    double min = samples.Count > 0 ? samples.Min(p => p.x) : double.NaN;
                    var intervals = new List<double>();
                    for (int i = 1; i < peaks.Count; i++) intervals.Add(peaks[i].t - peaks[i - 1].t);
                    double medianInterval = intervals.Count > 0 ? MedianS1(intervals) : double.NaN;
                    string rawLine = samples.Count > 0
                        ? string.Join(" ", samples.Select(p => $"{F(p.t * 1000, 0)}:{F(p.x, 2)}"))
                        : "(no samples)";
                    Both($"RAW trial={trial} n={samples.Count} rest={F(rest, 2)} peaks={peaks.Count} rawPeak={F(rawPeak, 2)} firstPeak={(peaks.Count > 0 ? F(peaks[0].x, 2) : "NaN")} lastPeak={(peaks.Count > 0 ? F(peaks[^1].x, 2) : "NaN")} medianInterval={F(medianInterval, 4)} min={F(min, 2)} {rawLine}");
                    bool ok = peaks.Count >= 3
                        && !double.IsNaN(medianInterval) && Math.Abs(medianInterval - 0.5) <= 0.025
                        && Math.Abs(peaks[0].x - 480.0) <= 2.0
                        && peaks[^1].x >= 0.95 * peaks[0].x
                        && !double.IsNaN(min) && Math.Abs(min) <= 2.0;
                    Both($"TRIAL trial={trial} peaks={peaks.Count} medianInterval={F(medianInterval, 4)} firstPeak={(peaks.Count > 0 ? F(peaks[0].x, 2) : "NaN")} lastFirst={(peaks.Count > 0 ? F(peaks[^1].x / peaks[0].x, 3) : "NaN")} min={F(min, 2)} {(ok ? "PASS" : "FAIL")}");
                    if (ok) passes++;
                }
                Both($"S2 ZERO DAMPING VERDICT={(passes == total ? "PASS" : "FAIL")} passes={passes}/{total}");
            }
            Both("===== S2 ZERO DAMPING DONE =====");
            art.Close();
            Log($"S2 zero-damping probe wrote {artPath}");
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("S2 ZERO DAMPING EXCEPTION " + ex);
            try { art.Close(); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        finally
        {
            _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog();
            try { CloseMainWindowSoon(); } catch { }
        }
    }

    // Peak (time, parabolic-vertex value) pairs: a sample is a peak when
    // it exceeds both neighbours and stands at least 50 dips above the
    // minima within 0.15 s on either side (true peaks are ~480 dips
    // above the ~0 troughs; the floor rejects capture noise).
    private static List<(double t, double x)> S2ZeroPeaks(List<(double t, double x)> s)
    {
        var peaks = new List<(double t, double x)>();
        const double window = 0.15, minProminence = 50.0;
        for (int i = 1; i + 1 < s.Count; i++)
        {
            if (!(s[i].x > s[i - 1].x && s[i].x >= s[i + 1].x)) continue;
            double leftMin = s[i].x, rightMin = s[i].x;
            for (int j = i - 1; j >= 0 && s[i].t - s[j].t <= window; j--) leftMin = Math.Min(leftMin, s[j].x);
            for (int j = i + 1; j < s.Count && s[j].t - s[i].t <= window; j++) rightMin = Math.Min(rightMin, s[j].x);
            if (s[i].x - Math.Max(leftMin, rightMin) < minProminence) continue;
            double h = (s[i + 1].t - s[i - 1].t) / 2;
            double denom = s[i - 1].x - 2 * s[i].x + s[i + 1].x;
            double offset = denom == 0 ? 0 : h * (s[i - 1].x - s[i + 1].x) / (2 * denom);
            double t = s[i].t + Math.Clamp(offset, -h, h);
            double v = denom >= 0 ? s[i].x : s[i].x - (s[i - 1].x - s[i + 1].x) * (s[i - 1].x - s[i + 1].x) / (8 * denom);
            peaks.Add((t, v));
        }
        return peaks;
    }
}
