// S1 period probe (module 7, MF-02). Measures the oscillation period of a
// compositor spring DIRECTLY and compares it with the S1-v2 prediction.
//
// spring.h: `response` is the UNDAMPED period; w0 = 2*pi/response; the
// observable damped period is T_d = response / sqrt(1 - zeta^2).
// composition.h MapSpringToCompositor (S1 v2): Period = response / (2*pi),
// DampingRatio = zeta, i.e. the compositor reads Period as 1/omega0.
// Rival reading (Period taken as the undamped period): with the same Period
// property value, T_rival = Period / sqrt(1 - zeta^2) = T_d / (2*pi).
// The two predictions differ by 2*pi, so the data picks one decisively.
//
// What already exists: shape evidence only. MAPTEST (派题 #13) compares
// trajectory shape RMS; S1V2 (派题 #44) compares WGC traces against the
// spring.h model. Neither extracts the peak-to-peak period or logs a
// verdict. This probe does that one thing: peak times from WGC captures,
// inter-peak intervals only (never the absolute start time, so
// StartAnimation latency cannot bias it), 3-point parabolic interpolation
// for sub-sample peak timing, verdict in the log itself.
//
// No change to composition.h, fm_capi, MotionSystem, or the planner.
// Programmatic only, NO synthetic input. Follows the MAPTEST/S1V2 capture
// pattern (WGC via mfprobe.dll, 0->37 dip calibration, N>50 filter).
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionSoftDemo;

public sealed partial class MainPage
{
    // S1 cell under test (spring.h units). zeta low enough for 3+ visible
    // peaks before the decay envelope (~1% at 1.5 s) kills the oscillation.
    private const double S1PeriodResponse = 0.5;
    private const double S1PeriodZeta = 0.25;
    private const int S1PeriodTrials = 3;
    private const double S1PeriodTolerance = 0.05; // +/-5% of predicted damped period

    private async Task RunS1PeriodProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artPath = Path.Combine(DevLog.Directory(), "win_s1_period_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }

        double period = S1PeriodResponse / (2 * Math.PI);
        double tPred = S1PeriodResponse / Math.Sqrt(1 - S1PeriodZeta * S1PeriodZeta);
        double tRival = period / Math.Sqrt(1 - S1PeriodZeta * S1PeriodZeta);
        Both("# win_s1_period_raw.txt — module 7 S1 period probe: measured compositor oscillation period vs r/(2*pi) mapping (programmatic only, NO synthetic input)");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss}; OS={Environment.OSVersion.Version}; QPC freq={Stopwatch.Frequency}; composition.h UNCHANGED");
        Both($"# CELL r={F(S1PeriodResponse, 4)} zeta={F(S1PeriodZeta, 2)} Period={F(period, 7)} DampingRatio={F(S1PeriodZeta, 2)} travel 0->240 IV=0 IVel=0 trials={S1PeriodTrials}");
        Both($"# PREDICT S1 T_d=r/sqrt(1-z^2)={F(tPred, 4)}s rival T={F(tRival, 4)}s tol=+/-{F(S1PeriodTolerance * 100, 1)}% (WGC quantum ~16.7ms ~3.2% of T_d; 3-point parabolic peak interpolation)");
        Both("# VERDICT rule: PASS iff every trial finds >=3 peaks and |median interval - T_d|/T_d <= tol; INVALID iff capture/calibration insufficient; else FAIL");
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "s1-period-probe");
            CompReset(0, "s1-period start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            // px/dip calibration, same as MAPTEST/S1V2: 0 -> 37 dips.
            var calQ0 = Qpc; CompReset(37, "s1-period cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "s1-period cal end"); await Task.Delay(500);
            var cc = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = cc.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = cc.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double pxPerDip = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : (XamlRoot?.RasterizationScale ?? 1.0);
            Both($"cal cx0={F(cx0, 3)} cx37={F(cx37, 3)} pxPerDip={F(pxPerDip, 4)}");
            if (double.IsNaN(cx0) || pxPerDip <= 0)
            {
                Both("S1 PERIOD VERDICT=INVALID reason=no-calibration");
            }
            else
            {
                async Task WaitNear(double dip, double tol = 2)
                {
                    for (int i = 0; i < 100; i++)
                    {
                        await Task.Delay(40);
                        var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(4).ToList();
                        if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / pxPerDip - dip) < tol)) return;
                    }
                }
                int passes = 0;
                for (int k = 0; k < S1PeriodTrials; k++)
                {
                    CompReset(0, "s1-period trial"); await WaitNear(0); await Task.Delay(250);
                    var a = _comp!.CreateSpringScalarAnimation();
                    a.Period = TimeSpan.FromSeconds(period); a.DampingRatio = (float)S1PeriodZeta;
                    a.InitialValue = 0f; a.InitialVelocity = 0f; a.FinalValue = 240f;
                    long q0 = Qpc; _visual!.StartAnimation("Translation.X", a);
                    await Task.Delay(1600);
                    var samples = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > q0).OrderBy(c => c.QpcArrive)
                        .Select(c => (t: SecSince(q0, c.QpcArrive), x: (c.Cx - cx0) / pxPerDip)).ToList();
                    CompReset(0, "s1-period trial end");
                    Both("RAW " + string.Join(" ", samples.Select(s => $"{F(s.t * 1000, 2)}:{F(s.x, 2)}")));
                    var (peaks, intervals) = ExtractPeriods(samples);
                    Both($"TRIAL k={k} n={samples.Count} peaks={peaks.Count} peakTimes={string.Join(",", peaks.Select(p => F(p, 4)))} intervals={string.Join(",", intervals.Select(v => F(v, 4)))}");
                    if (peaks.Count < 3 || intervals.Count < 2)
                    {
                        Both($"TRIAL k={k} VERDICT=INVALID reason=insufficient-peaks peaks={peaks.Count}");
                        continue;
                    }
                    double median = MedianS1(intervals);
                    double err = Math.Abs(median - tPred) / tPred;
                    bool pass = err <= S1PeriodTolerance;
                    if (pass) passes++;
                    Both($"TRIAL k={k} VERDICT={(pass ? "PASS" : "FAIL")} medianInterval={F(median, 4)}s predicted={F(tPred, 4)}s rival={F(tRival, 4)}s err={F(err * 100, 2)}%");
                }
                Both($"S1 PERIOD VERDICT={(passes == S1PeriodTrials ? "PASS" : "FAIL")} passes={passes}/{S1PeriodTrials} predicted={F(tPred, 4)}s rival={F(tRival, 4)}s tol=+/-{F(S1PeriodTolerance * 100, 1)}%");
            }
            Both("===== S1 PERIOD DONE =====");
            art.Close(); try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex) { Both("S1 PERIOD EXCEPTION " + ex); try { art.Close(); } catch { } try { MfProbe.mf_cap_stop(); } catch { } }
        finally { _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog(); try { CloseMainWindowSoon(); } catch { } }
    }

    // Peak-to-peak intervals from (time, dip) samples. A peak is a strict
    // local maximum with >= 4 dips prominence over the 0.35 s windows each
    // side; its time is the vertex of the parabola through its 3 samples.
    // Returns peak times and the intervals between consecutive peaks.
    private static (List<double> peaks, List<double> intervals) ExtractPeriods(List<(double t, double x)> s)
    {
        var peaks = new List<double>();
        const double window = 0.35;
        const double minProminence = 4.0;
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
            peaks.Add(s[i].t + Math.Clamp(offset, -h, h));
        }
        var intervals = new List<double>();
        for (int i = 1; i < peaks.Count; i++) intervals.Add(peaks[i] - peaks[i - 1]);
        return (peaks, intervals);
    }

    // Suffixed per the repo convention (MedianPrime/MedianHdv/MedianWsa):
    // a plain Median(List<double>) already exists on this partial class in
    // M2DeltaVProbe.cs; reusing that name is CS0111.
    private static double MedianS1(List<double> v)
    {
        var s = v.OrderBy(x => x).ToList();
        int n = s.Count;
        return n % 2 == 1 ? s[n / 2] : (s[n / 2 - 1] + s[n / 2]) / 2;
    }
}
