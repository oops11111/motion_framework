// M2-Δv′ (prime) probe: absolute Δv gate (LOCKED 06467b82…805c).
// Arms: Arm-WGC-seed (tested), base (pos ctrl), IVel=0 (neg ctrl t060 fwd+rev ONLY).
// Estimator: quadratic LS w=5; thr=max(20, 0.05*|v_ref|) KEEP.
// Judicable P1 |a|·σ_ts ≤ thr (no /2); P2 thr ≥ Nv_h_cell (no 2×).
// IVel0 skipped on t156/t300 (P5 harness bypass). NO shadow IV. ≠ PERF-CI.
// Prior gate d2349ceb…09c6 NOT re-run; artifacts use m2_delta_v_prime_* prefix.
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
    const double M2DvpPeriod = 0.32;
    const double M2DvpZeta = 0.78;
    const double M2DvpCalib = 1.0;
    const int M2DvpW = 5;
    const int M2DvpNeedN = 20;
    const int M2DvpHangCap = 2;
    const double M2DvpFloorThr = 20.0; // DIP/s
    const double M2DvpRelThr = 0.05;
    const double M2DvpFrameSec = 1.0 / 60.0;

    private async Task RunM2DeltaVPrimeProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true;
        try { MeasureButton.IsEnabled = false; } catch { }

        string artDir = @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string sessFile = Path.Combine(artDir, "m2_delta_v_prime_session.txt");
        string sessEnv = File.Exists(sessFile) ? File.ReadAllText(sessFile).Trim() : (Environment.GetEnvironmentVariable("MF_M2_DELTA_V_PRIME_SESSION") ?? "1");
        if (string.IsNullOrWhiteSpace(sessEnv)) sessEnv = "1";

        string preregPath = Path.Combine(artDir, "m2_delta_v_prime_prereg.txt");
        string preregHash = File.Exists(preregPath) ? Sha256FilePrime(preregPath) : "MISSING_PREREG";

        string artPath = Path.Combine(artDir, "m2_delta_v_prime_capture_raw.txt");
        string sumPath = Path.Combine(artDir, "m2_delta_v_prime_capture_summary.txt");
        string artPathSess = Path.Combine(artDir, $"m2_delta_v_prime_s{sessEnv}_raw.txt");
        string sumPathSess = Path.Combine(artDir, $"m2_delta_v_prime_s{sessEnv}_summary.txt");

        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        var sum = new StreamWriter(sumPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        void Sum(string s) { Both(s); sum.WriteLine(s); sum.Flush(); }

        string host = Environment.MachineName;
        int pid = Environment.ProcessId;
        Both($"# m2_delta_v_prime_s{sessEnv}_raw.txt — M2-Δv′ absolute Δv gate (prime)");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} AWST; hostname={host}; pid={pid}; sess={sessEnv}");
        Both($"# gate=06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c LOCKED_AT=2026-10-26 09:05:37 AWST");
        Both($"# approved=pidan@2026-10-26 09:09:19 AWST");
        Both($"# prereg_hash={preregHash}");
        Both($"# estimator=quad_LS w={M2DvpW}; thr=max({M2DvpFloorThr},{M2DvpRelThr}*|v_ref|); hang_cap={M2DvpHangCap}; calib={M2DvpCalib}");
        Both("# judicable P1=|a|*σ_ts<=thr (no /2); P2=thr>=Nv_h_cell (no 2x); IVel0 only t060 fwd+rev");
        Both("# arms=Arm-WGC-seed|base|IVel0; cells=t060{fwd,rev}+t156{fwd,rev}+t300{fwd,rev,dbl}");
        Both("# FORBIDDEN: shadow IV / mf_spring_at→InitialVelocity; two-frame Δv; relative-A; composition.h edit; PERF-CI claim");
        Both("# seed_path=WGC for Arm-WGC-seed; base=uninterrupted; IVel0=InitialVelocity=0");

        // cell def: name, trip_s, final, dbl, core?
        var cells = new (string name, double trip, double final, bool dbl, bool core)[]
        {
            ("t060fwd", 0.060, 360, false, true),
            ("t060rev", 0.060, 0, false, true),
            ("t156fwd", 0.156, 360, false, false),
            ("t156rev", 0.156, 0, false, false),
            ("t300fwd", 0.300, 360, false, true),
            ("t300rev", 0.300, 0, false, true),
            ("t300dbl", 0.300, 0, true, true),
        };
        string[] armsOrder = { "base", "Arm-WGC-seed", "IVel0" }; // base FIRST → Nv_h before interrupt arms

        var rows = new List<TrialRowPrime>();
        int hangCountCell = 0; // tracked per cell below via dict
        var hangByCell = new Dictionary<string, int>(StringComparer.Ordinal);

        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "m2-delta-v-prime");
            CompReset(0, "m2dvp start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");

            var calQ0 = Qpc; CompReset(37, "m2dvp cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "m2dvp cal end"); await Task.Delay(500);
            var cc = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = cc.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = cc.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double ppd = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : 1.0;
            var rl = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault();
            if (rl != null) cx0 = rl.Cx;
            Both($"cal cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(ppd,4)}");

            async Task WaitNear(double dip, double tol = 4)
            {
                for (int i = 0; i < 80; i++)
                {
                    await Task.Delay(40);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(4).ToList();
                    if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / ppd - dip) < tol)) return;
                }
            }

            foreach (var cell in cells)
            {
                hangByCell[cell.name] = 0;
                // per-cell: run base first (NeedN), then WGC, then IVel0
                foreach (var arm in armsOrder)
                {
                    // P5: IVel0 only on t060 fwd+rev (neg ctrl). Skip t156/t300 entirely (crash bypass ≠ INVALID).
                    if (arm == "IVel0" && cell.name != "t060fwd" && cell.name != "t060rev")
                    {
                        Both($"CELL_ARM_SKIP cell={cell.name} arm=IVel0 reason=P5_IVel0_only_t060");
                        continue;
                    }
                    int valid = 0, attempt = 0, maxAttempt = M2DvpNeedN * 3;
                    while (valid < M2DvpNeedN && attempt < maxAttempt)
                    {
                        var row = await OneTrialPrime(arm, cell.name, cell.trip, cell.final, cell.dbl, attempt, cx0, ppd, WaitNear, Both);
                        rows.Add(row);
                        attempt++;
                        if (row.Hang)
                        {
                            hangByCell[cell.name] = hangByCell[cell.name] + 1;
                            Both($"HANG cell={cell.name} arm={arm} k={attempt - 1} hang_n={hangByCell[cell.name]}");
                            if (hangByCell[cell.name] > M2DvpHangCap)
                            {
                                Both($"INVALID hang_cap exceeded cell={cell.name} hang={hangByCell[cell.name]}");
                                goto DONE_EARLY;
                            }
                            continue;
                        }
                        if (row.Valid) valid++;
                    }
                    Both($"CELL_ARM_DONE cell={cell.name} arm={arm} valid={valid}/{M2DvpNeedN} attempts={attempt}");
                }
            }

        DONE_EARLY:
            // ---- Nv_h_cell from base rows (P95 of Δv) ----
            var nvh = new Dictionary<string, double>(StringComparer.Ordinal);
            var sigmaTs = new Dictionary<string, double>(StringComparer.Ordinal);
            var aAbs = new Dictionary<string, double>(StringComparer.Ordinal);
            foreach (var cell in cells)
            {
                var baseDvs = rows.Where(r => r.Arm == "base" && r.Cell == cell.name && r.Valid && !double.IsNaN(r.Dv))
                    .Select(r => r.Dv).OrderBy(x => x).ToList();
                nvh[cell.name] = PercentilePrime(baseDvs, 0.95);
                var sts = rows.Where(r => r.Arm == "base" && r.Cell == cell.name && r.Valid && !double.IsNaN(r.SigmaTs))
                    .Select(r => r.SigmaTs).ToList();
                sigmaTs[cell.name] = sts.Count > 0 ? sts.Average() : M2DvpFrameSec;
                var aa = rows.Where(r => r.Arm == "base" && r.Cell == cell.name && r.Valid && !double.IsNaN(r.AbsA))
                    .Select(r => r.AbsA).ToList();
                aAbs[cell.name] = aa.Count > 0 ? aa.Average() : double.NaN;
                Both($"NVH cell={cell.name} Nv_h_P95={F(nvh[cell.name], 3)} n_base={baseDvs.Count} sigma_ts={F(sigmaTs[cell.name], 5)} |a|={F(aAbs[cell.name], 2)}");
            }

            // annotate adjudicate per row now that Nv_h known
            foreach (var r in rows)
            {
                if (!r.Valid || double.IsNaN(r.Dv)) continue;
                double nv = nvh.GetValueOrDefault(r.Cell, double.NaN);
                r.NvH = nv;
                // median thr from valid trials of same arm/cell for adjudicate floor
                double thrMed = rows.Where(x => x.Arm == r.Arm && x.Cell == r.Cell && x.Valid && !double.IsNaN(x.Thr))
                    .Select(x => x.Thr).DefaultIfEmpty(M2DvpFloorThr).OrderBy(x => x).Skip(Math.Max(0, rows.Count(x => x.Arm == r.Arm && x.Cell == r.Cell && x.Valid) / 2)).FirstOrDefault();
                // use sample thr for pass; for judicability use thr floor vs Nv_h
                double thrFloor = M2DvpFloorThr; // conservative for judicability when |v_ref| small
                // per gate prime: thr >= Nv_h AND |a|*σ_ts <= thr (P1/P2)
                // use sample thr_i for the second clause relative to that sample
                bool judicable = !double.IsNaN(nv) && r.Thr >= nv
                    && !double.IsNaN(aAbs.GetValueOrDefault(r.Cell)) && !double.IsNaN(sigmaTs.GetValueOrDefault(r.Cell))
                    && aAbs[r.Cell] * sigmaTs[r.Cell] <= r.Thr; // P1 |a|σ≤thr; P2 thr≥Nv_h
                // t156: expected timing-bound SKIP via rule (not hard label) — if rule says judicable we still report, but gate expects SKIP
                r.Judicable = judicable;
                r.PassThr = r.Dv <= r.Thr && (double.IsNaN(nv) || nv <= r.Thr);
                // FAIL-determined: CI lower > thr+Nv_h — single sample proxy: Dv > thr+nv
                r.FailDetermined = !double.IsNaN(nv) && r.Dv > r.Thr + nv;
            }

            // rewrite annotated trial lines for clarity
            Both("# ---- ADJUDICATED ----");
            foreach (var r in rows)
            {
                string reason = !r.Valid ? (r.Hang ? "hang" : r.InvalidReason)
                    : (!r.Judicable ? "rule-SKIP" : (r.PassThr ? "pass" : (r.FailDetermined ? "FAIL-determined" : "fail-thr")));
                Both($"ADJ arm={r.Arm} cell={r.Cell} k={r.K} phi={F(r.Phi,0)} t_s={F(r.Ts * 1000, 3)}ms v_pre={F(r.VPre,3)} v_post={F(r.VPost,3)} thr={F(r.Thr,3)} Δv={F(r.Dv,3)} r={F(r.R,4)} Nv_h={F(r.NvH,3)} jud={r.Judicable} pass={r.PassThr} reason={reason} ivel={F(r.IVelSet,3)} v_wgc={F(r.VWgc,3)} seed_path={r.SeedPath} shadow_v_log={F(r.ShadowV,2)}");
            }

            // ---- session summary ----
            Sum($"# m2_delta_v_prime_s{sessEnv}_summary.txt");
            Sum($"session={sessEnv} pid={pid} hostname={host} prereg_hash={preregHash} calib={F(M2DvpCalib,6)} w={M2DvpW} hang_cap={M2DvpHangCap}");
            Sum($"composition_expected=a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418");

            bool allCorePass = true;
            bool basePass = true;
            bool ivel0FailOk = true;
            var cellVerdicts = new List<string>();

            foreach (var cell in cells)
            {
                double nv = nvh.GetValueOrDefault(cell.name, double.NaN);
                double st = sigmaTs.GetValueOrDefault(cell.name, double.NaN);
                double aa = aAbs.GetValueOrDefault(cell.name, double.NaN);

                // judicability at cell level using floor thr and median thr of WGC arm
                var wgcValid = rows.Where(r => r.Arm == "Arm-WGC-seed" && r.Cell == cell.name && r.Valid).Take(M2DvpNeedN).ToList();
                double thrMed = wgcValid.Count > 0 ? MedianPrime(wgcValid.Select(x => x.Thr).ToList()) : M2DvpFloorThr;
                bool cellJud = !double.IsNaN(nv) && thrMed >= nv && !double.IsNaN(aa) && !double.IsNaN(st) && aa * st <= thrMed; // P1/P2 prime

                // force t156 through rule: if somehow judicable, still report numbers; gate says expected SKIP
                string role = cell.core ? "core" : "aux-t156";

                int nOk = wgcValid.Count(r => r.Judicable && r.PassThr && !r.FailDetermined);
                int nJ = wgcValid.Count(r => r.Judicable);
                // For PASS count: among first NeedN valid, those with Δv≤thr (and Nv_h≤thr). If not judicable → SKIP
                int nPass = wgcValid.Count(r => r.PassThr && !r.FailDetermined);
                bool cellPass = cellJud && wgcValid.Count >= M2DvpNeedN && nPass >= 17;
                bool cellSkip = !cellJud;

                // base arm for this cell
                var baseValid = rows.Where(r => r.Arm == "base" && r.Cell == cell.name && r.Valid).Take(M2DvpNeedN).ToList();
                int basePassN = baseValid.Count(r => r.Dv <= r.Thr);
                bool baseCellPass = baseValid.Count >= M2DvpNeedN && basePassN >= 17;

                // IVel0 on t060 only for neg ctrl
                var ivelValid = rows.Where(r => r.Arm == "IVel0" && r.Cell == cell.name && r.Valid).Take(M2DvpNeedN).ToList();
                int ivelPassN = ivelValid.Count(r => r.PassThr && !r.FailDetermined);
                bool ivelCellFail = ivelValid.Count >= M2DvpNeedN && ivelPassN < 17; // FAIL = not PASS

                string verdict;
                if (cellSkip || (!cell.core && !cellJud))
                {
                    verdict = "Δv-SKIP";
                    if (!cell.core) { /* aux OK */ }
                    else { allCorePass = false; } // core must be judicable
                }
                else if (cellPass)
                {
                    verdict = "PASS";
                }
                else
                {
                    verdict = "FAIL";
                    if (cell.core) allCorePass = false;
                }

                if (cell.core && cellJud && !baseCellPass) { basePass = false; } // only judicable core
                if ((cell.name == "t060fwd" || cell.name == "t060rev"))
                {
                    // neg ctrl must FAIL (not PASS)
                    if (!ivelCellFail) ivel0FailOk = false;
                }

                Sum($"CELL cell={cell.name} role={role} Nv_h={F(nv,3)} thrMed={F(thrMed,3)} |a|*σ={F(aa * st, 3)} jud={cellJud} wgc_n={wgcValid.Count} wgc_pass={nPass}/20 base_pass={basePassN}/{baseValid.Count} IVel0_pass={ivelPassN}/{ivelValid.Count} verdict={verdict}");
                cellVerdicts.Add($"{cell.name}:{verdict}");
            }

            bool sessInvalid = hangByCell.Values.Any(h => h > M2DvpHangCap);
            string sessVerdict;
            if (sessInvalid) sessVerdict = "SESSION_INVALID";
            else if (allCorePass && basePass && ivel0FailOk) sessVerdict = "SESSION_PASS_CANDIDATE";
            else if (!allCorePass && cellVerdicts.Any(v => v.Contains("FAIL"))) sessVerdict = "SESSION_FAIL_CANDIDATE";
            else sessVerdict = "SESSION_INCONCLUSIVE_CANDIDATE";

            Sum($"ctrl basePass={basePass} IVel0_t060_both_FAIL={ivel0FailOk}");
            Sum($"cells={string.Join(",", cellVerdicts)}");
            Sum(sessVerdict);
            Sum("===== M2-DELTA-V-PRIME PROBE DONE =====");
            Both("===== M2-DELTA-V-PRIME PROBE DONE =====");

            art.Close(); sum.Close();
            try { File.Copy(artPath, artPathSess, true); } catch { }
            try { File.Copy(sumPath, sumPathSess, true); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("M2-DELTA-V-PRIME EXCEPTION " + ex);
            try { art.Close(); } catch { }
            try { sum.Close(); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        finally { _suiteRunning = false; try { MeasureButton.IsEnabled = true; } catch { } FlushLog(); try { CloseMainWindowSoon(); } catch { } }
    }

    private async Task<TrialRowPrime> OneTrialPrime(
        string arm, string cell, double trip, double final, bool dbl, int k,
        double cx0, double ppd, Func<double, double, Task> WaitNear, Action<string> Both)
    {
        var row = new TrialRowPrime { Arm = arm, Cell = cell, K = k, SeedPath = arm == "Arm-WGC-seed" ? "WGC" : (arm == "IVel0" ? "IVel0" : "base"), Phi = 0 };
        try
        {
            CompReset(0, cell); await WaitNear(0, 4); await Task.Delay(200);
            long qStart = Qpc;
            var a = _comp!.CreateSpringScalarAnimation();
            a.Period = TimeSpan.FromSeconds(M2DvpPeriod); a.DampingRatio = (float)M2DvpZeta;
            a.InitialValue = 0f; a.InitialVelocity = 0f; a.FinalValue = 240f;
            _visual!.StartAnimation("Translation.X", a);

            // Content trip at `trip`, with WGC phase catch-up: compositor/WGC lag ~1–3 frames means a
            // pure wall-clock wait at t060 leaves the screen near rest (v_wgc≈0) and starves w=5.
            // Wait until wall>=trip AND WGC |x| has reached ~shadow_x(trip) (log-only shadow as timing
            // oracle — NOT written to InitialVelocity). Caps wall at trip+3 frames.
            MfProbe.mf_spring_at(M2DvpPeriod, M2DvpZeta, 0, 240, 0, trip, out var shXTarget, out var shVTarget);
            double xTargetAbs = Math.Abs(shXTarget);
            double catchDeadline = trip + 3.0 * M2DvpFrameSec;
            while (true)
            {
                double tw = SecSince(qStart, Qpc);
                if (tw >= catchDeadline) break;
                if (tw >= trip)
                {
                    var recent = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).TakeLast(3).ToList();
                    if (recent.Count >= 1)
                    {
                        double xNow = Math.Abs((recent[^1].Cx - cx0) / ppd);
                        // require visual to have reached ≥70% of shadow travel at content φ
                        if (xTargetAbs < 1.0 || xNow >= 0.70 * xTargetAbs) break;
                    }
                }
                else if (tw < trip - 0.020) await Task.Delay(5);
                // else spin
            }

            long qTrip = Qpc;
            double tTrip = SecSince(qStart, qTrip);
            row.Phi = tTrip; // wall content time (may be trip..trip+3frames)

            // WGC last-two for seed (and log)
            var preCaps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart && c.QpcArrive <= qTrip)
                .OrderBy(c => c.QpcArrive).ToList();
            if (preCaps.Count < 2)
                preCaps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).TakeLast(8).ToList();

            double vWgc = double.NaN;
            if (preCaps.Count >= 2)
            {
                var c1 = preCaps[^2]; var c2 = preCaps[^1];
                double dt = SecSince(c1.QpcArrive, c2.QpcArrive);
                if (dt > 0.004) vWgc = (c2.Cx - c1.Cx) / ppd / dt;
            }
            row.VWgc = vWgc;

            // log-only shadow at actual wall trip (FORBIDDEN as IV)
            MfProbe.mf_spring_at(M2DvpPeriod, M2DvpZeta, 0, 240, 0, tTrip, out var shX, out var shV);
            row.ShadowV = shV;

            long qSwitch1 = 0, qSwitch2 = 0;
            long qNextChange = long.MaxValue;

            if (arm == "base")
            {
                // uninterrupted — no StartAnimation replacement
                row.IVelSet = double.NaN;
                row.SeedPath = "base";
                // Align nominal switch with interrupt-arm commit latency (~1 frame after content
                // trip) so estimator W sits where interrupt t_s would land; keeps ≥w pre frames
                // at 60 Hz for t060. Content φ still recorded as row.Phi = tTrip.
                double commitSkew = Math.Max(M2DvpFrameSec, (M2DvpW + 0.5) * M2DvpFrameSec - trip); // ensure ≥w pre frames at 60Hz
                while (SecSince(qStart, Qpc) < trip + commitSkew) { /* spin */ }
                qSwitch1 = Qpc;
                await Task.Delay((int)Math.Max(400, (0.450 - (trip + commitSkew)) * 1000));
            }
            else
            {
                var rAnim = _comp.CreateSpringScalarAnimation();
                rAnim.Period = TimeSpan.FromSeconds(M2DvpPeriod); rAnim.DampingRatio = (float)M2DvpZeta;
                rAnim.FinalValue = dbl ? 360f : (float)final;
                // InitialValue NOT set (WGC/screen-consistent position via compositor current)
                if (arm == "Arm-WGC-seed")
                {
                    if (double.IsNaN(vWgc))
                    {
                        row.Valid = false; row.InvalidReason = "no_v_wgc"; row.Hang = false;
                        Both($"TRIAL arm={arm} cell={cell} k={k} INVALID no_v_wgc");
                        CompReset(0, cell + " abort");
                        return row;
                    }
                    row.IVelSet = M2DvpCalib * vWgc;
                    rAnim.InitialVelocity = (float)row.IVelSet;
                    row.SeedPath = "WGC";
                }
                else // IVel0
                {
                    row.IVelSet = 0;
                    rAnim.InitialVelocity = 0f;
                    row.SeedPath = "IVel0";
                }
                _visual.StartAnimation("Translation.X", rAnim);
                // Yield ~1 frame so W centers on apply (tApply), giving ≥w pre frames at 60Hz/t060
                long qCommit = Qpc;
                while (SecSince(qCommit, Qpc) < M2DvpFrameSec * 0.85) { /* spin to apply */ }
                qSwitch1 = Qpc;

                if (dbl)
                {
                    await Task.Delay(80); // brief then second switch
                    var r2 = _comp.CreateSpringScalarAnimation();
                    r2.Period = TimeSpan.FromSeconds(M2DvpPeriod); r2.DampingRatio = (float)M2DvpZeta;
                    r2.FinalValue = (float)final; // 0
                    // re-seed from latest WGC for continuity of path A; still WGC not shadow
                    var mid = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qSwitch1).OrderBy(c => c.QpcArrive).TakeLast(3).ToList();
                    double v2 = row.IVelSet;
                    if (arm == "Arm-WGC-seed" && mid.Count >= 2)
                    {
                        double dt2 = SecSince(mid[^2].QpcArrive, mid[^1].QpcArrive);
                        if (dt2 > 0.004) v2 = M2DvpCalib * (mid[^1].Cx - mid[^2].Cx) / ppd / dt2;
                        r2.InitialVelocity = (float)v2;
                        row.IVelSet = v2; // report last set
                    }
                    else
                    {
                        r2.InitialVelocity = arm == "IVel0" ? 0f : (float)row.IVelSet;
                    }
                    _visual.StartAnimation("Translation.X", r2);
                    long qC2 = Qpc;
                    while (SecSince(qC2, Qpc) < M2DvpFrameSec * 0.85) { /* spin */ }
                    qSwitch2 = Qpc;
                    qNextChange = long.MaxValue;
                }
                await Task.Delay(450);
            }

            // Collect captures for estimation
            long qEnd = Qpc;
            var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart && c.QpcArrive < qEnd)
                .OrderBy(c => c.QpcArrive)
                .Select(c => (t: SecSince(qStart, c.QpcArrive), x: (c.Cx - cx0) / ppd, q: c.QpcArrive))
                .ToList();

            if (caps.Count < M2DvpW * 2 + 2)
            {
                row.Valid = false; row.InvalidReason = "too_few_frames"; row.Hang = caps.Count < 4;
                Both($"TRIAL arm={arm} cell={cell} k={k} INVALID too_few_frames n={caps.Count}");
                CompReset(0, cell + " end");
                return row;
            }

            // Determine switch window and t_s
            // W = [τ_{kc-1}, τ_kc]: for interrupt use [qSwitch - 1 frame, qSwitch]; for dbl use second switch
            long qSw = (dbl && qSwitch2 > 0) ? qSwitch2 : qSwitch1;
            if (qSw <= 0) qSw = qTrip;
            double tSwNom = SecSince(qStart, qSw);
            // W ≈ [τ_{kc-1}, τ_kc]: one-frame neighborhood around commit-aligned switch
            double tLo = tSwNom - M2DvpFrameSec;
            double tHi = tSwNom + M2DvpFrameSec;
            if (tLo < 0) tLo = 0;

            // Find t_s minimizing |x_post - x_pre| in W (gate). For base (no real switch) fall back to W midpoint if flat.
            double bestTs = 0.5 * (tLo + tHi);
            double bestGap = double.PositiveInfinity;
            var inWin = caps.Where(c => c.t >= tLo - M2DvpFrameSec && c.t <= tHi + M2DvpFrameSec).ToList();
            for (int i = 1; i < inWin.Count; i++)
            {
                double gap = Math.Abs(inWin[i].x - inWin[i - 1].x);
                double tsCand = 0.5 * (inWin[i].t + inWin[i - 1].t);
                if (tsCand < tLo || tsCand > tHi) continue;
                if (gap < bestGap) { bestGap = gap; bestTs = tsCand; }
            }
            if (arm == "base" && (double.IsInfinity(bestGap) || inWin.Count < 2))
                bestTs = 0.5 * (tLo + tHi);
            row.Ts = bestTs;
            row.SigmaTs = M2DvpFrameSec / Math.Sqrt(12.0); // uniform within 1 frame timing uncertainty

            // pre: last w frames with t < t_s; post: first w with t > t_s and before next change
            double tNext = (dbl && qSwitch2 > 0 && qSw == qSwitch1)
                ? SecSince(qStart, qSwitch2)
                : (qNextChange < long.MaxValue ? SecSince(qStart, qNextChange) : double.PositiveInfinity);
            // for dbl evaluated at t_s2, next change = none

            var pre = caps.Where(c => c.t < bestTs).OrderBy(c => c.t).TakeLast(M2DvpW).ToList();
            var post = caps.Where(c => c.t > bestTs && c.t < tNext - 1e-4).OrderBy(c => c.t).Take(M2DvpW).ToList();

            if (pre.Count < M2DvpW || post.Count < M2DvpW)
            {
                row.Valid = false; row.InvalidReason = $"insuff_w pre={pre.Count} post={post.Count}";
                Both($"TRIAL arm={arm} cell={cell} k={k} INVALID insuff_w pre={pre.Count} post={post.Count} t_s={F(bestTs*1000,2)}");
                CompReset(0, cell + " end");
                return row;
            }

            var (vPre, aPre) = QuadVelAtPrime(pre, bestTs);
            var (vPost, aPost) = QuadVelAtPrime(post, bestTs);
            row.VPre = vPre; row.VPost = vPost;
            row.AbsA = Math.Abs(aPre); // from pre side (base/accel for judicability)
            if (row.AbsA > 50000 || Math.Abs(vPre) > 8000 || Math.Abs(vPost) > 8000)
            {
                row.Valid = false; row.InvalidReason = "num_unstable";
                Both($"TRIAL arm={arm} cell={cell} k={k} INVALID num_unstable |a|={F(row.AbsA,1)} v_pre={F(vPre,1)} v_post={F(vPost,1)}");
                CompReset(0, cell + " end");
                return row;
            }
            row.Dv = Math.Abs(vPost - vPre);
            double vRef = vPre;
            row.Thr = Math.Max(M2DvpFloorThr, M2DvpRelThr * Math.Abs(vRef));
            row.R = row.Thr > 0 ? row.Dv / row.Thr : double.NaN;
            row.Valid = !double.IsNaN(row.Dv) && !double.IsNaN(row.Thr) && !double.IsNaN(vPre) && !double.IsNaN(vPost);
            if (!row.Valid) row.InvalidReason = "nan_estimate";

            Both($"TRIAL arm={arm} cell={cell} k={k} phi={F(tTrip*1000,2)} t_s={F(bestTs*1000,3)} v_pre={F(vPre,3)} v_post={F(vPost,3)} thr={F(row.Thr,3)} Δv={F(row.Dv,3)} r={F(row.R,4)} valid={row.Valid} ivel_set={F(row.IVelSet,3)} v_wgc={F(vWgc,3)} seed_path={row.SeedPath} shadow_v_log={F(shV,2)} |a|={F(row.AbsA,2)} σ_ts={F(row.SigmaTs,5)} pre_n={pre.Count} post_n={post.Count} dbl={(dbl?1:0)}");

            CompReset(0, cell + " end");
            return row;
        }
        catch (Exception ex)
        {
            row.Valid = false; row.Hang = true; row.InvalidReason = "exception:" + ex.GetType().Name;
            Both($"TRIAL arm={arm} cell={cell} k={k} EXCEPTION {ex.Message}");
            try { CompReset(0, cell + " ex"); } catch { }
            return row;
        }
    }

    /// <summary>Quadratic LS: x(t)=a + b*(t-t0) + c*(t-t0)^2; return (v=b, accel=2c) at t0.
    /// Time is scaled to frame units for Vandermonde conditioning at 60 Hz.</summary>
    private static (double v, double accel) QuadVelAtPrime(List<(double t, double x, long q)> frames, double t0)
    {
        int n = frames.Count;
        if (n < 3) return (double.NaN, double.NaN);
        double scale = M2DvpFrameSec; // ~1/60; u = (t-t0)/scale ∈ frames
        if (scale < 1e-6) scale = 1.0 / 60.0;
        double s0 = 0, s1 = 0, s2 = 0, s3 = 0, s4 = 0;
        double sy = 0, sty = 0, st2y = 0;
        foreach (var f in frames)
        {
            double u = (f.t - t0) / scale;
            double u2 = u * u, u3 = u2 * u, u4 = u2 * u2;
            s0 += 1; s1 += u; s2 += u2; s3 += u3; s4 += u4;
            sy += f.x; sty += u * f.x; st2y += u2 * f.x;
        }
        double[,] A = { { s0, s1, s2 }, { s1, s2, s3 }, { s2, s3, s4 } };
        double[] rhs = { sy, sty, st2y };
        if (!Solve3Prime(A, rhs, out var coeff)) return (double.NaN, double.NaN);
        // x = a + b_u*u + c_u*u^2; u=(t-t0)/scale ⇒ dx/dt = (b_u + 2 c_u u)/scale; at t0: b_u/scale
        double v = coeff[1] / scale;
        double accel = 2.0 * coeff[2] / (scale * scale);
        if (double.IsNaN(v) || double.IsInfinity(v) || Math.Abs(v) > 1e6) return (double.NaN, double.NaN);
        if (double.IsNaN(accel) || double.IsInfinity(accel) || Math.Abs(accel) > 1e8) return (double.NaN, double.NaN);
        return (v, accel);
    }

    private static bool Solve3Prime(double[,] A, double[] b, out double[] x)
    {
        x = new double[3];
        double[,] M = (double[,])A.Clone();
        double[] y = (double[])b.Clone();
        for (int col = 0; col < 3; col++)
        {
            int piv = col;
            for (int r = col + 1; r < 3; r++) if (Math.Abs(M[r, col]) > Math.Abs(M[piv, col])) piv = r;
            if (Math.Abs(M[piv, col]) < 1e-14) return false;
            if (piv != col) { for (int c = 0; c < 3; c++) { var tmp = M[col, c]; M[col, c] = M[piv, c]; M[piv, c] = tmp; } var ty = y[col]; y[col] = y[piv]; y[piv] = ty; }
            double div = M[col, col];
            for (int c = col; c < 3; c++) M[col, c] /= div;
            y[col] /= div;
            for (int r = 0; r < 3; r++)
            {
                if (r == col) continue;
                double f = M[r, col];
                for (int c = col; c < 3; c++) M[r, c] -= f * M[col, c];
                y[r] -= f * y[col];
            }
        }
        x[0] = y[0]; x[1] = y[1]; x[2] = y[2];
        return true;
    }

    private static double PercentilePrime(List<double> xs, double p)
    {
        if (xs == null || xs.Count == 0) return double.NaN;
        var s = xs.OrderBy(v => v).ToList();
        double idx = p * (s.Count - 1);
        int i = (int)Math.Floor(idx);
        double f = idx - i;
        if (i + 1 >= s.Count) return s[^1];
        return s[i] * (1 - f) + s[i + 1] * f;
    }

    private static double MedianPrime(List<double> xs)
    {
        if (xs == null || xs.Count == 0) return double.NaN;
        var s = xs.OrderBy(v => v).ToList();
        int n = s.Count;
        return (n % 2 == 1) ? s[n / 2] : 0.5 * (s[n / 2 - 1] + s[n / 2]);
    }

    private static string Sha256FilePrime(string path)
    {
        using var fs = File.OpenRead(path);
        var hash = System.Security.Cryptography.SHA256.HashData(fs);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private sealed class TrialRowPrime
    {
        public string Arm = "";
        public string Cell = "";
        public int K;
        public double Phi, Ts, VPre, VPost, Thr, Dv, R, VWgc, IVelSet, ShadowV, SigmaTs, AbsA, NvH;
        public bool Valid, Hang, Judicable, PassThr, FailDetermined;
        public string SeedPath = "";
        public string InvalidReason = "";
    }
}





