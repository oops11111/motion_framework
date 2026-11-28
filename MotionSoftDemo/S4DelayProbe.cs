// Module 7 / MF-02 — S4 DelayTime probe (plan S4, still open).
//
// S4 claim (docs/module7-plan.md): "an animation may start with a
// `DelayTime` and the property keeps its preset value until the delay
// elapses. Probe: sample during the delay window." composition.h
// documents the same time model: "delay: real seconds until it starts
// advancing (presetValue is set immediately)".
//
// What already exists does NOT cover this to the probe standard.
// 派题 #16 (RunS4Probe, --s4-probe) samples three DelayBehavior arms
// with loose gates (hold 8/15 dips, leave threshold 12 dips), single-
// sample calibration with a RasterizationScale fallback, no control
// cell, no readbacks, no comparison against an expected onset time,
// and Period passed raw as 0.32s (the pre-S1 reading; the S1 mapping
// is Period = response/(2*pi)). It has no Windows report on file.
// This probe measures the plan claim directly, with the S1/S2/S3
// discipline: median calibration, a DelayTime=0 control cell that
// measures the baseline call latency, and a stated onset tolerance.
//
// Design. Preset = InitialValue = 50 dips (nonzero, so "holds preset"
// is distinguishable from "never started"), target 240, zeta = 0.78,
// response 0.5s, Period = r/(2*pi) (S1 mapping), IV = 0,
// DelayBehavior = SetInitialValueAfterDelay set explicitly and read
// back (the default is not under test; BeforeDelay and the IV!=preset
// jump are #16 arms B/C and out of scope here). Cells: delay in
// {0 (control), 0.5s}, 3 trials each; controls run first and their
// median onset latency is the baseline the delay cells are judged
// against. Two claims, both gated:
//   HOLD: pre-onset samples stay within 0.5 dips of the trial's own
//     preset baseline (S2 noise ~0.2 dips; the per-trial baseline,
//     taken after CompReset(preset), is immune to the S5 open
//     calibration-rest question; preset placement itself is gated
//     separately: |baseline - 50| <= 2 dips on the calibrated scale).
//   ONSET: delay-cell onset minus control-median latency minus delay
//     within +-50ms (3 frames: latency jitter measured 32-47ms in the
//     S3 second round, plus 1-frame onset granularity).
// Onset = first sample deviating > 1.0 dip from the trial baseline
// (first-frame motion from rest is ~4 dips for this spring, well
// above noise). InitialValue is not read back: its nullability is
// not established from repo code (only writes exist); its value is
// confirmed by the hold measurement itself.
// 0034 fixes (first round was build-blocked, REPORT-0c003de5):
// AnimationDelayBehavior lives in Microsoft.UI.Composition, which
// this file did not import (CS0246); and a control trial now
// contributes its onset to the latency baseline only if it passed
// the hold gate, so a hold-failing control cannot pollute medianL0.
// Flag: --s4-delay-probe / MF_S4_DELAY_PROBE=1. Log: m56_s4_delay_ui.txt.
// Artifact: win_s4_delay_raw.txt in the same art dir as the log.

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
    private async Task RunS4DelayProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = DevLog.Directory();
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_s4_delay_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        Both("# win_s4_delay_raw.txt — MF-02 S4 DelayTime preset-hold probe");
        Both("# wall=" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "; OS=" + Environment.OSVersion.Version);
        Both("# CELL r=0.5000 Period=0.0795775 (S1 mapping) zeta=0.78 preset=InitialValue=50 InitialVelocity=0 target=240 DelayBehavior=AfterDelay delay={0,0.5}s trials=3 observe=1.5s");
        Both("# PREDICT hold: pre-onset samples within 0.5 dips of trial preset baseline; onset: delay-cell onset - control-median latency - delay within +-50ms; onset=first sample >1.0 dip from trial baseline");
        Both("# VERDICT rule: readbacks match, |trialBaseline-50|<=2 dips, holdMaxDev<=0.5 dips (>=3 pre-onset samples for delay cells, >=1 for controls), onset found, delay-cell |dOnset|<=50ms; no-calibration => INVALID");
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(120);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "s4-delay-probe");
            CompReset(0, "s4-delay start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            // Settle AFTER capture start so pre-calibration captures exist
            // (patch 0023 lesson).
            CompReset(0, "s4-delay cal settle"); await Task.Delay(400);
            // px/dip calibration, same as MAPTEST/S1/S2/S3/S5: 0 -> 37 dips,
            // medians of the last few captures in each window (patch 0026).
            var calQ0 = Qpc; CompReset(37, "s4-delay cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "s4-delay cal end"); await Task.Delay(500);
            var cc = ReadCaptures().Where(c => c.N > 50).ToList();
            var preCal = cc.Where(c => c.QpcArrive < calQ0).Select(c => c.Cx).ToList();
            var midCal = cc.Where(c => c.QpcArrive >= calQ0 && c.QpcArrive < calQ1).Select(c => c.Cx).ToList();
            double cx0 = preCal.Count > 0 ? MedianS1(preCal.Skip(Math.Max(0, preCal.Count - 5)).ToList()) : double.NaN;
            double cx37 = midCal.Count > 0 ? MedianS1(midCal.Skip(Math.Max(0, midCal.Count - 5)).ToList()) : double.NaN;
            double pxPerDip = (double.IsNaN(cx0) || double.IsNaN(cx37)) ? double.NaN : (cx37 - cx0) / 37.0;
            Both($"cal cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(pxPerDip,4)} nPre={preCal.Count} nMid={midCal.Count}");

            int passes = 0, total = 0;
            var controlOnsets = new List<double>();
            if (double.IsNaN(cx0) || pxPerDip <= 0)
            {
                Both("S4 DELAY VERDICT=INVALID reason=no-calibration");
            }
            else
            {
                double[] delays = { 0, 0.5 };
                foreach (double delaySec in delays)
                {
                    double? medianL0 = controlOnsets.Count > 0 ? MedianS1(controlOnsets) : null;
                    if (delaySec > 0) Both($"CONTROL medianL0={F(medianL0 ?? double.NaN, 3)}s n={controlOnsets.Count}");
                    for (int trial = 0; trial < 3; trial++)
                    {
                        total++;
                        CompReset(50, $"s4-delay D={delaySec} t={trial} preset");
                        long qPre = Qpc;
                        await Task.Delay(300);
                        long q0 = Qpc;
                        string assignNote;
                        float readZeta = float.NaN; float? readFinal = null;
                        TimeSpan readDelay = TimeSpan.MinValue;
                        AnimationDelayBehavior readBehavior = (AnimationDelayBehavior)(-1);
                        try
                        {
                            var a = _comp!.CreateSpringScalarAnimation();
                            a.Period = TimeSpan.FromSeconds(0.5 / (2 * Math.PI));
                            a.DampingRatio = 0.78f;
                            a.FinalValue = 240f;
                            a.InitialValue = 50f;
                            a.InitialVelocity = 0f;
                            a.DelayTime = TimeSpan.FromSeconds(delaySec);
                            a.DelayBehavior = AnimationDelayBehavior.SetInitialValueAfterDelay;
                            readZeta = a.DampingRatio;
                            readFinal = a.FinalValue;
                            readDelay = a.DelayTime;
                            readBehavior = a.DelayBehavior;
                            assignNote = $"assign ok readback zeta={readZeta.ToString("G9", CultureInfo.InvariantCulture)} final={readFinal?.ToString("G9", CultureInfo.InvariantCulture)} delayMs={F(readDelay.TotalMilliseconds, 0)} behavior={readBehavior}";
                            _visual!.StartAnimation("Translation.X", a);
                        }
                        catch (Exception ex)
                        {
                            assignNote = $"assign THROW {ex.GetType().Name}: {ex.Message}";
                        }
                        await Task.Delay(1500);
                        long qEnd = Qpc;
                        CompReset(0, $"s4-delay D={delaySec} t={trial} end");
                        if (float.IsNaN(readZeta) || readZeta != 0.78f || !readFinal.HasValue || readFinal.Value != 240f
                            || readDelay != TimeSpan.FromSeconds(delaySec) || readBehavior != AnimationDelayBehavior.SetInitialValueAfterDelay)
                        {
                            Both($"TRIAL delay={delaySec} trial={trial} {assignNote} FAIL reason={(assignNote.Contains("THROW") ? "assign-throw" : "readback-mismatch")}");
                            continue;
                        }
                        // Trial preset baseline: last 5 captures after
                        // CompReset(preset), before StartAnimation.
                        var preCaps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive >= qPre && c.QpcArrive < q0)
                            .Select(c => (c.Cx - cx0) / pxPerDip).ToList();
                        double trialBaseline = preCaps.Count > 0
                            ? MedianS1(preCaps.Skip(Math.Max(0, preCaps.Count - 5)).ToList()) : double.NaN;
                        var samples = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive >= q0 && c.QpcArrive <= qEnd)
                            .Select(c => (t: SecSince(q0, c.QpcArrive), x: (c.Cx - cx0) / pxPerDip))
                            .OrderBy(p => p.t).ToList();
                        string rawLine = samples.Count > 0
                            ? string.Join(" ", samples.Select(p => $"{F(p.t * 1000, 0)}:{F(p.x, 2)}"))
                            : "(no samples)";
                        Both($"RAW delay={delaySec} trial={trial} n={samples.Count} baseline={F(trialBaseline, 2)} {rawLine}");
                        if (double.IsNaN(trialBaseline) || Math.Abs(trialBaseline - 50) > 2.0)
                        {
                            Both($"TRIAL delay={delaySec} trial={trial} {assignNote} baseline={F(trialBaseline, 2)} FAIL reason=preset-placement");
                            continue;
                        }
                        int onsetIdx = samples.FindIndex(p => Math.Abs(p.x - trialBaseline) > 1.0);
                        if (onsetIdx < 0)
                        {
                            Both($"TRIAL delay={delaySec} trial={trial} {assignNote} baseline={F(trialBaseline, 2)} n={samples.Count} FAIL reason=no-onset");
                            continue;
                        }
                        double tOnset = samples[onsetIdx].t;
                        var holdSet = samples.Take(onsetIdx).ToList();
                        double holdMaxDev = holdSet.Count > 0 ? holdSet.Max(p => Math.Abs(p.x - trialBaseline)) : double.NaN;
                        int needHold = delaySec > 0 ? 3 : 1;
                        bool holdOk = holdSet.Count >= needHold && !double.IsNaN(holdMaxDev) && holdMaxDev <= 0.5;
                        double? dOnset = null;
                        bool onsetOk = true;
                        if (delaySec > 0)
                        {
                            if (!medianL0.HasValue)
                            {
                                Both($"TRIAL delay={delaySec} trial={trial} {assignNote} baseline={F(trialBaseline, 2)} tOnset={F(tOnset, 3)}s FAIL reason=no-control-baseline");
                                continue;
                            }
                            dOnset = tOnset - medianL0.Value - delaySec;
                            onsetOk = Math.Abs(dOnset.Value) <= 0.05;
                        }
                        else if (holdOk)
                        {
                            // Only a control trial that held its preset
                            // may contribute its onset to the baseline.
                            controlOnsets.Add(tOnset);
                        }
                        bool ok = holdOk && onsetOk;
                        Both($"TRIAL delay={delaySec} trial={trial} {assignNote} baseline={F(trialBaseline, 2)} nHold={holdSet.Count} holdMaxDev={F(holdMaxDev, 2)} tOnset={F(tOnset, 3)}s dOnset={(dOnset.HasValue ? F(dOnset.Value * 1000, 0) + "ms" : "n/a")} {(ok ? "PASS" : "FAIL")}");
                        if (ok) passes++;
                    }
                }
                Both($"S4 DELAY VERDICT={(passes == total ? "PASS" : "FAIL")} passes={passes}/{total}");
            }
            Both("===== S4 DELAY DONE =====");
            art.Close();
            Log($"S4 delay probe wrote {artPath}");
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("S4 DELAY EXCEPTION " + ex);
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
