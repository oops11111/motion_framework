// Module 7 / MF-02 — S5 rest-threshold probe (plan S5, still open).
//
// S5 claim (docs/module7-plan.md): "the compositor spring stops at its
// own rest threshold; ours snaps to target at `SpringSettleTime`. Probe:
// record the end-of-motion delta between the two and document the
// expected difference; do not force them to match."
//
// What already exists does NOT cover this. 派题 #17 (RunS5Probe,
// --s5-probe) checks only that WGC holds near FinalValue for >=500ms
// after a 900ms extra settle, plus a manual StopAnimation freeze check:
// it never timestamps the natural freeze, never measures the frozen
// offset, and never compares either against SpringSettleTime (it logs
// the number but never uses it). It has no Windows report on file.
// This probe measures the compositor side directly: the time (from
// motion onset) at which WGC stops changing, the frozen value, and the
// value at the model's settle time; the deltas against the model are
// RECORDED, not forced to match.
//
// "Ours snaps" code basis (read, not re-proved here):
// composition.h PlanOverride returns HoldAt(target) once elapsed >=
// SpringSettleTime; timeline.h sizes settle with span-scaled epsilons
// (0.1% of span position, 0.5%/s velocity, floored/capped). For the
// 240-dip span used here both clamp to spring.h's defaults (0.1 dips,
// 0.5 dips/s), so the planner's SpringSettleTime equals spring.h
// SettleTime(defaults) for these cells. mf_spring_settle is the
// probe-DLL export wrapping spring.h SettleTime (DllImport signature
// takes response; its wrapper source is not in this repo); the probe
// logs its value per cell for post-hoc comparison. Note: #17 passed
// its Period (0.32) as the response argument, so its logged SettleTime
// is for a different (faster) spring and cannot be reused.
//
// "Freeze" honesty limit: WGC cannot distinguish the compositor
// stopping internally from creeping below observation resolution.
// Freeze here means NO OBSERVABLE MOTION: 15 consecutive captures
// (250ms at 60Hz) spanning <= 0.1 dips. The frozen value, freeze time,
// and post-freeze tail range are all logged; a later move shows up in
// the tail range instead of being hidden.
// Start latency is handled with the S3 lesson: times are anchored on
// motion onset (first sample >= 1 dip), not on q0 (start latency
// measured 32-47ms in the S3 second round).
// 0032 fixes (first round was build-blocked, REPORT-4c8407d2):
// FinalValue is float?, not float (CS0266 at the readback); the
// observation window is cut at qEnd so post-reset samples cannot fake
// a freeze; the freeze scan starts at the onset index, so tFreeze
// lies in [0, 6s) by construction.
// Cells: zeta in {0.78, 0.25}, response 0.5s, Period = r/(2*pi) (the
// S1-verified mapping), 0 -> 240, IV = 0, 3 trials each.
// Flag: --s5-rest-probe / MF_S5_REST_PROBE=1. Log: m55_s5_rest_ui.txt.
// Artifact: win_s5_rest_raw.txt in the same art dir as the log.

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
    private async Task RunS5RestProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = DevLog.Directory();
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_s5_rest_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        Both("# win_s5_rest_raw.txt — MF-02 S5 compositor rest-threshold probe");
        Both("# wall=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "; OS=" + Environment.OSVersion.Version);
        Both("# CELL r=0.5000 Period=0.0795775 (S1 mapping) travel 0->240 IV=0 zeta={0.78,0.25} trials=3 observe=6.0s");
        Both("# PREDICT model=mf_spring_settle(r,zeta,0,240,0) (spring.h SettleTime defaults 0.1 dips/0.5 dips/s = planner span epsilons at span 240); freeze=15 consecutive captures range<=0.1 dips; onset anchor=first sample >=1 dip");
        Both("# VERDICT rule: readbacks match, onset found, freeze within 6.0s, |xFreeze-target|<=10 dips (sanity bound only); dT/dX are RECORDED, not gated; no-calibration => INVALID");
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(120);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "s5-rest-probe");
            CompReset(0, "s5-rest start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            // Settle AFTER capture start so pre-calibration captures exist
            // (patch 0023 lesson).
            CompReset(0, "s5-rest cal settle"); await Task.Delay(400);
            // px/dip calibration, same as MAPTEST/S1/S2/S3: 0 -> 37 dips,
            // medians of the last few captures in each window (patch 0026).
            var calQ0 = Qpc; CompReset(37, "s5-rest cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "s5-rest cal end"); await Task.Delay(500);
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
                Both("S5 REST VERDICT=INVALID reason=no-calibration");
            }
            else
            {
                double[] zetas = { 0.78, 0.25 };
                foreach (double zeta in zetas)
                {
                    double tSettle = MfProbe.mf_spring_settle(0.5, zeta, 0, 240, 0);
                    Both($"CELL zeta={zeta} settleModel={F(tSettle, 3)}s");
                    for (int trial = 0; trial < 3; trial++)
                    {
                        total++;
                        CompReset(0, $"s5-rest zeta={zeta} t={trial} rest"); await Task.Delay(250);
                        long q0 = Qpc;
                        string assignNote;
                        float readZeta = float.NaN; float? readFinal = null;
                        try
                        {
                            var a = _comp!.CreateSpringScalarAnimation();
                            a.Period = TimeSpan.FromSeconds(0.5 / (2 * Math.PI));
                            a.DampingRatio = (float)zeta;
                            a.FinalValue = 240f;
                            a.InitialValue = 0f;
                            a.InitialVelocity = 0f;
                            readZeta = a.DampingRatio;
                            readFinal = a.FinalValue;
                            assignNote = $"assign ok readback zeta={readZeta.ToString("G9", CultureInfo.InvariantCulture)} final={readFinal?.ToString("G9", CultureInfo.InvariantCulture)}";
                            _visual!.StartAnimation("Translation.X", a);
                        }
                        catch (Exception ex)
                        {
                            assignNote = $"assign THROW {ex.GetType().Name}: {ex.Message}";
                        }
                        await Task.Delay(6000);
                        long qEnd = Qpc;
                        CompReset(0, $"s5-rest zeta={zeta} t={trial} end");
                        if (float.IsNaN(readZeta) || readZeta != (float)zeta || !readFinal.HasValue || readFinal.Value != 240f)
                        {
                            Both($"TRIAL zeta={zeta} trial={trial} {assignNote} FAIL reason={(assignNote.Contains("THROW") ? "assign-throw" : "readback-mismatch")}");
                            continue;
                        }
                        if (!(tSettle > 0))
                        {
                            Both($"TRIAL zeta={zeta} trial={trial} {assignNote} FAIL reason=bad-settle-model");
                            continue;
                        }
                        var samples = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive >= q0 && c.QpcArrive <= qEnd)
                            .Select(c => (qpc: c.QpcArrive, x: (c.Cx - cx0) / pxPerDip))
                            .OrderBy(p => p.qpc).ToList();
                        // Motion onset anchor: first sample >= 1 dip.
                        int onsetIdx = samples.FindIndex(p => p.x >= 1.0);
                        if (onsetIdx < 0)
                        {
                            Both($"TRIAL zeta={zeta} trial={trial} {assignNote} n={samples.Count} FAIL reason=no-onset");
                            continue;
                        }
                        var onset = samples[onsetIdx];
                        // Freeze: first run of 15 consecutive captures
                        // spanning <= 0.1 dips (no observable motion),
                        // searched from the onset index on.
                        int freezeEnd = -1;
                        for (int i = onsetIdx + 14; i < samples.Count; i++)
                        {
                            var win = samples.Skip(i - 14).Take(15).Select(p => p.x).ToList();
                            if (win.Max() - win.Min() <= 0.1) { freezeEnd = i; break; }
                        }
                        string rawLine = samples.Count > 0
                            ? string.Join(" ", samples.Select(p => $"{F(SecSince(q0, p.qpc) * 1000, 0)}:{F(p.x, 2)}"))
                            : "(no samples)";
                        Both($"RAW zeta={zeta} trial={trial} n={samples.Count} {rawLine}");
                        if (freezeEnd < 0)
                        {
                            Both($"TRIAL zeta={zeta} trial={trial} {assignNote} settleModel={F(tSettle, 3)}s n={samples.Count} FAIL reason=no-freeze");
                            continue;
                        }
                        var freezeRun = samples.Skip(freezeEnd - 14).Take(15).ToList();
                        double xFreeze = MedianS1(freezeRun.Select(p => p.x).ToList());
                        double tFreeze = SecSince(onset.qpc, freezeRun[0].qpc);
                        double dT = tFreeze - tSettle;
                        double dX = xFreeze - 240.0;
                        // Value nearest the model's settle time (onset-anchored).
                        var atSettle = samples.OrderBy(p => Math.Abs(SecSince(onset.qpc, p.qpc) - tSettle)).First();
                        double xAtSettle = atSettle.x;
                        // Post-freeze tail: does the value move again?
                        double tailRange = 0;
                        foreach (var p in samples.Skip(freezeEnd + 1)) tailRange = Math.Max(tailRange, Math.Abs(p.x - xFreeze));
                        bool ok = Math.Abs(dX) <= 10.0;
                        Both($"TRIAL zeta={zeta} trial={trial} {assignNote} settleModel={F(tSettle, 3)}s tFreeze={F(tFreeze, 3)}s xFreeze={F(xFreeze, 2)} dT={F(dT, 3)}s dX={F(dX, 2)} xAtSettle={F(xAtSettle, 2)} tailRange={F(tailRange, 2)} {(ok ? "PASS" : "FAIL")}");
                        if (ok) passes++;
                    }
                }
                Both($"S5 REST VERDICT={(passes == total ? "PASS" : "FAIL")} passes={passes}/{total}");
            }
            Both("===== S5 REST DONE =====");
            art.Close();
            Log($"S5 rest probe wrote {artPath}");
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("S5 REST EXCEPTION " + ex);
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
