// M2-WGC-SEED-PATH-B′ probe: PATHB-A=V2-clamp — P-LS+TS seed (N=5 quad LS @ t_int=min(t_comp,t_last_WGC)) + absolute Δv smoke (t060 only).
// Gate: a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43 @ 2026-10-26 13:49:20 Asia/Taipei
// Approved: pidan@2026-10-26 13:51:27 Asia/Taipei
// Labels: SEED-PATHB-* only. FORBIDDEN: Δv-PASS / SEED-ABS-PASS / SEED-CALIB-PASS / re-run 09f6414b…793d / 831818e4…80de / 06467b82…805c / shadow IV / composition.h edit.
// Measurement tool (KEEP): quad LS w=5; thr=max(20,0.05*|v_ref|); P1 |a|σ≤thr; P2 thr≥Nv_h; ≥17/20.
// Seed PATHB-A: last N=5 WGC (t,x) quad LS → v̂=b+2c·t_int; t_int=min(t_compositor_interrupt_QPC, t_last_WGC); FORBIDDEN extrapolate past t_last_WGC.
// IV=(float)(calib·v̂); calib=0.7510829456 (display 0.7511) KEEP; ε=5.0; P-VS=OFF. A′ (P-VS α_v=0.30) NOT run this ticket.
// Fallback B: IV=null+0. Ticket PASS evaluates A only.
// H5 ratio regression: med_r∈[0.8,1.2] AND no r_i≥2.0; v_screen=last-two WGC (H5 denom).
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
    const double M2WspPeriod = 0.32;
    const double M2WspZeta = 0.78;
    const double M2WspCalib = 0.7510829456;  // exact; display 0.7511; KEEP; ABS 1.0 retired; CALIB FAIL not re-run
    const int M2WspSeedN = 5;          // seed window N=5 (P-LS)
    const int M2WspMeasW = 5;          // measurement estimator w=5
    const int M2WspNeedN = 20;
    const int M2WspHangCap = 2;
    const double M2WspFloorThr = 20.0;  // DIP/s
    const double M2WspRelThr = 0.05;
    const double M2WspEps = 5.0;        // DIP/s; |v̂|<ε → path B
    const double M2WspFrameSec = 1.0 / 60.0;
    const double M2WspMedRLo = 0.8;
    const double M2WspMedRHi = 1.2;
    const double M2WspRiCap = 2.0;

    private async Task RunM2WgcSeedPathBPrimeProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true;
        try { MeasureButton.IsEnabled = false; } catch { }

        string artDir = @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string sessFile = Path.Combine(artDir, "m2_wgc_seed_path_b_prime_session.txt");
        string sessEnv = File.Exists(sessFile) ? File.ReadAllText(sessFile).Trim() : (Environment.GetEnvironmentVariable("MF_M2_WGC_SEED_PATH_B_PRIME_SESSION") ?? "1");
        if (string.IsNullOrWhiteSpace(sessEnv)) sessEnv = "1";

        string preregPath = Path.Combine(artDir, "m2_wgc_seed_path_b_prime_prereg.txt");
        string preregHash = File.Exists(preregPath) ? Sha256FileWsp(preregPath) : "MISSING_PREREG";

        string artPath = Path.Combine(artDir, "m2_wgc_seed_path_b_prime_capture_raw.txt");
        string sumPath = Path.Combine(artDir, "m2_wgc_seed_path_b_prime_capture_summary.txt");
        string artPathSess = Path.Combine(artDir, $"m2_wgc_seed_path_b_prime_s{sessEnv}_raw.txt");
        string sumPathSess = Path.Combine(artDir, $"m2_wgc_seed_path_b_prime_s{sessEnv}_summary.txt");

        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        var sum = new StreamWriter(sumPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        void Sum(string s) { Both(s); sum.WriteLine(s); sum.Flush(); }

        string host = Environment.MachineName;
        int pid = Environment.ProcessId;
        Both($"# m2_wgc_seed_path_b_prime_s{sessEnv}_raw.txt — M2-WGC-SEED-PATH-B-PRIME P-LS+TS + t060 smoke");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; hostname={host}; pid={pid}; sess={sessEnv}");
        Both($"# gate=a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43 LOCKED_AT=2026-10-26 13:49:20 Asia/Taipei");
        Both($"# approved=pidan@2026-10-26 13:51:27 Asia/Taipei");
        Both($"# prereg_hash={preregHash}");
        Both($"# path_A=PATHB-A=V2-clamp N={M2WspSeedN} order=2 t_int=min(t_comp,t_last_WGC) calib={M2WspCalib} eps={M2WspEps} P-VS=OFF");
        Both($"# path_B=IV=null+0; ticket PASS evaluates A only");
        Both($"# measure=quad_LS w={M2WspMeasW}; thr=max({M2WspFloorThr},{M2WspRelThr}*|v_ref|); hang_cap={M2WspHangCap}");
        Both("# judicable P1=|a|*σ_ts<=thr; P2=thr>=Nv_h_cell; IVel0 only t060 fwd+rev");
        Both("# arms=Arm-WGC-seed|base|IVel0; smoke_cells=t060{fwd,rev}; t156=rule-SKIP; t300*=optional-not-run");
        Both("# H5 ratio: r_i=|v_seed/v_screen|; v_screen=last-two WGC; med_r∈[0.8,1.2]; no r_i≥2.0");
        Both("# FORBIDDEN: shadow IV; composition.h edit; Δv-PASS; SEED-ABS-PASS; SEED-CALIB-PASS; re-run 09f6414b/831818e4/06467b82; PERF-CI; thaw Bv; reopen S1/R-3/R-2; raise thr; A→A′ swap");
        Both("# labels=SEED-PATHB-* only; 成功≠采用≠Δv-PASS≠SEED-ABS-PASS≠SEED-CALIB-PASS; ≠PERF-CI; SEED-TRUTH-CONFLICT if ratioOk but t060 smoke dual FAIL");

        // smoke-only cells (gate: t060 required; t156 SKIP; t300 optional — not run this ticket)
        var cells = new (string name, double trip, double final, bool dbl, bool core)[]
        {
            ("t060fwd", 0.060, 360, false, true),
            ("t060rev", 0.060, 0, false, true),
        };
        string[] armsOrder = { "base", "Arm-WGC-seed", "IVel0" };

        var rows = new List<TrialRowWsp>();
        var hangByCell = new Dictionary<string, int>(StringComparer.Ordinal);

        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "m2-wgc-seed-path-b-prime");
            CompReset(0, "m2wsp start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");

            var calQ0 = Qpc; CompReset(37, "m2wsp cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "m2wsp cal end"); await Task.Delay(500);
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
                foreach (var arm in armsOrder)
                {
                    if (arm == "IVel0" && cell.name != "t060fwd" && cell.name != "t060rev")
                    {
                        Both($"CELL_ARM_SKIP cell={cell.name} arm=IVel0 reason=P5_IVel0_only_t060");
                        continue;
                    }
                    int valid = 0, attempt = 0, maxAttempt = M2WspNeedN * 3;
                    while (valid < M2WspNeedN && attempt < maxAttempt)
                    {
                        var row = await OneTrialWsp(arm, cell.name, cell.trip, cell.final, cell.dbl, attempt, cx0, ppd, WaitNear, Both);
                        rows.Add(row);
                        attempt++;
                        if (row.Hang)
                        {
                            hangByCell[cell.name] = hangByCell[cell.name] + 1;
                            Both($"HANG cell={cell.name} arm={arm} k={attempt - 1} hang_n={hangByCell[cell.name]}");
                            if (hangByCell[cell.name] > M2WspHangCap)
                            {
                                Both($"INVALID hang_cap exceeded cell={cell.name} hang={hangByCell[cell.name]}");
                                goto DONE_EARLY;
                            }
                            continue;
                        }
                        if (row.Valid) valid++;
                    }
                    Both($"CELL_ARM_DONE cell={cell.name} arm={arm} valid={valid}/{M2WspNeedN} attempts={attempt}");
                }
            }

        DONE_EARLY:
            var nvh = new Dictionary<string, double>(StringComparer.Ordinal);
            var sigmaTs = new Dictionary<string, double>(StringComparer.Ordinal);
            var aAbs = new Dictionary<string, double>(StringComparer.Ordinal);
            foreach (var cell in cells)
            {
                var baseDvs = rows.Where(r => r.Arm == "base" && r.Cell == cell.name && r.Valid && !double.IsNaN(r.Dv))
                    .Select(r => r.Dv).OrderBy(x => x).ToList();
                nvh[cell.name] = PercentileWsp(baseDvs, 0.95);
                var sts = rows.Where(r => r.Arm == "base" && r.Cell == cell.name && r.Valid && !double.IsNaN(r.SigmaTs))
                    .Select(r => r.SigmaTs).ToList();
                sigmaTs[cell.name] = sts.Count > 0 ? sts.Average() : M2WspFrameSec;
                var aa = rows.Where(r => r.Arm == "base" && r.Cell == cell.name && r.Valid && !double.IsNaN(r.AbsA))
                    .Select(r => r.AbsA).ToList();
                aAbs[cell.name] = aa.Count > 0 ? aa.Average() : double.NaN;
                Both($"NVH cell={cell.name} Nv_h_P95={F(nvh[cell.name], 3)} n_base={baseDvs.Count} sigma_ts={F(sigmaTs[cell.name], 5)} |a|={F(aAbs[cell.name], 2)}");
            }

            foreach (var r in rows)
            {
                if (!r.Valid || double.IsNaN(r.Dv)) continue;
                double nv = nvh.GetValueOrDefault(r.Cell, double.NaN);
                r.NvH = nv;
                bool judicable = !double.IsNaN(nv) && r.Thr >= nv
                    && !double.IsNaN(aAbs.GetValueOrDefault(r.Cell)) && !double.IsNaN(sigmaTs.GetValueOrDefault(r.Cell))
                    && aAbs[r.Cell] * sigmaTs[r.Cell] <= r.Thr;
                r.Judicable = judicable;
                r.PassThr = r.Dv <= r.Thr && (double.IsNaN(nv) || nv <= r.Thr);
                r.FailDetermined = !double.IsNaN(nv) && r.Dv > r.Thr + nv;
            }

            Both("# ---- ADJUDICATED ----");
            foreach (var r in rows)
            {
                string reason = !r.Valid ? (r.Hang ? "hang" : r.InvalidReason)
                    : (!r.Judicable ? "rule-SKIP" : (r.PassThr ? "pass" : (r.FailDetermined ? "FAIL-determined" : "fail-thr")));
                Both($"ADJ arm={r.Arm} cell={r.Cell} k={r.K} phi={F(r.Phi,0)} t_s={F(r.Ts * 1000, 3)}ms v_pre={F(r.VPre,3)} v_post={F(r.VPost,3)} thr={F(r.Thr,3)} Δv={F(r.Dv,3)} r={F(r.R,4)} Nv_h={F(r.NvH,3)} jud={r.Judicable} pass={r.PassThr} reason={reason} ivel={F(r.IVelSet,3)} v_wgc={F(r.VWgc,3)} v_screen={F(r.VScreen,3)} r_i={F(r.Ri,4)} seed_path={r.SeedPath} pathb_variant={r.PathbVariant} t_int={F(r.TInt*1000,3)} t_last_WGC={F(r.TLastWgc*1000,3)} t_comp={F(r.TComp*1000,3)} clamped={r.TIntClamped} shadow_v_log={F(r.ShadowV,2)}");
            }

            // H5 ratio regression over path-A Arm-WGC-seed trials with finite r_i
            var ratioRows = rows.Where(r => r.Arm == "Arm-WGC-seed" && r.SeedPath == "A" && r.Valid && !double.IsNaN(r.Ri) && !double.IsInfinity(r.Ri)).ToList();
            var ratios = ratioRows.Select(r => r.Ri).OrderBy(x => x).ToList();
            double medR = MedianWsp(ratios);
            double maxR = ratios.Count > 0 ? ratios.Max() : double.NaN;
            bool anyRiGe2 = ratios.Any(x => x >= M2WspRiCap);
            bool ratioOk = ratios.Count > 0 && !double.IsNaN(medR) && medR >= M2WspMedRLo && medR <= M2WspMedRHi && !anyRiGe2;
            int nPathA = rows.Count(r => r.Arm == "Arm-WGC-seed" && r.SeedPath == "A");
            int nPathB = rows.Count(r => r.Arm == "Arm-WGC-seed" && r.SeedPath == "B");
            bool pathAUsed = nPathA > 0;

            Sum($"# m2_wgc_seed_path_b_prime_s{sessEnv}_summary.txt");
            Sum($"session={sessEnv} pid={pid} hostname={host} prereg_hash={preregHash} calib={F(M2WspCalib,6)} seed_N={M2WspSeedN} meas_w={M2WspMeasW} hang_cap={M2WspHangCap} eps={M2WspEps}");
            Sum($"path_declare=PATHB-A=V2-clamp N={M2WspSeedN} order=2 t_int=min(t_comp,t_last_WGC) calib_exact={M2WspCalib:R} calib_display=0.7511 P-VS=OFF");
            Sum($"pathb_variant=PATHB-A");
            Sum($"composition_expected=a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418");
            int nClamp = rows.Count(r => r.Arm == "Arm-WGC-seed" && r.TIntClamped);
            Sum($"H5_ratio n={ratios.Count} med_r={F(medR,4)} max_r={F(maxR,4)} any_ri_ge2={anyRiGe2} ratioOk={ratioOk} pathA_n={nPathA} pathB_n={nPathB}");
            Sum($"t_int_clamp n_clamped={nClamp} rule=min(t_comp,t_last_WGC) pathb_variant=PATHB-A P-VS=OFF");

            bool allCorePass = true;
            bool basePass = true;
            bool ivel0FailOk = true;
            var cellVerdicts = new List<string>();

            foreach (var cell in cells)
            {
                double nv = nvh.GetValueOrDefault(cell.name, double.NaN);
                double st = sigmaTs.GetValueOrDefault(cell.name, double.NaN);
                double aa = aAbs.GetValueOrDefault(cell.name, double.NaN);

                var wgcValid = rows.Where(r => r.Arm == "Arm-WGC-seed" && r.Cell == cell.name && r.Valid && r.SeedPath == "A").Take(M2WspNeedN).ToList();
                // if path-A valid < NeedN, still report with whatever we have (may FAIL)
                if (wgcValid.Count < M2WspNeedN)
                {
                    // also accept Valid path-A first; then any Valid WGC (incl B) only for logging — PASS needs A
                    wgcValid = rows.Where(r => r.Arm == "Arm-WGC-seed" && r.Cell == cell.name && r.Valid && r.SeedPath == "A").Take(M2WspNeedN).ToList();
                }
                double thrMed = wgcValid.Count > 0 ? MedianWsp(wgcValid.Select(x => x.Thr).ToList()) : M2WspFloorThr;
                bool cellJud = !double.IsNaN(nv) && thrMed >= nv && !double.IsNaN(aa) && !double.IsNaN(st) && aa * st <= thrMed;

                int nPass = wgcValid.Count(r => r.PassThr && !r.FailDetermined);
                bool cellPass = cellJud && wgcValid.Count >= M2WspNeedN && nPass >= 17;
                bool cellSkip = !cellJud;

                var baseValid = rows.Where(r => r.Arm == "base" && r.Cell == cell.name && r.Valid).Take(M2WspNeedN).ToList();
                int basePassN = baseValid.Count(r => r.Dv <= r.Thr);
                bool baseCellPass = baseValid.Count >= M2WspNeedN && basePassN >= 17;

                var ivelValid = rows.Where(r => r.Arm == "IVel0" && r.Cell == cell.name && r.Valid).Take(M2WspNeedN).ToList();
                int ivelPassN = ivelValid.Count(r => r.PassThr && !r.FailDetermined);
                bool ivelCellFail = ivelValid.Count >= M2WspNeedN && ivelPassN < 17;

                string verdict;
                if (cellSkip)
                {
                    verdict = "SEED-PATHB-SKIP";
                    if (cell.core) allCorePass = false;
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

                if (cell.core && cellJud && !baseCellPass) basePass = false;
                if (cell.name == "t060fwd" || cell.name == "t060rev")
                {
                    if (!ivelCellFail) ivel0FailOk = false;
                }

                Sum($"CELL cell={cell.name} role=core Nv_h={F(nv,3)} thrMed={F(thrMed,3)} |a|*σ={F(aa * st, 3)} jud={cellJud} wgc_n={wgcValid.Count} wgc_pass={nPass}/20 base_pass={basePassN}/{baseValid.Count} IVel0_pass={ivelPassN}/{ivelValid.Count} verdict={verdict}");
                cellVerdicts.Add($"{cell.name}:{verdict}");
            }

            bool sessInvalid = hangByCell.Values.Any(h => h > M2WspHangCap);
            string sessVerdict;
            if (sessInvalid) sessVerdict = "SESSION_INVALID";
            else if (!pathAUsed) sessVerdict = "SESSION_FAIL_CANDIDATE"; // all-B not PASS
            else if (allCorePass && basePass && ivel0FailOk && ratioOk && pathAUsed) sessVerdict = "SESSION_PASS_CANDIDATE";
            else if (cellVerdicts.Any(v => v.Contains("FAIL")) || !ratioOk || !basePass || !ivel0FailOk) sessVerdict = "SESSION_FAIL_CANDIDATE";
            else sessVerdict = "SESSION_INCONCLUSIVE_CANDIDATE";

            Sum($"ctrl basePass={basePass} IVel0_t060_both_FAIL={ivel0FailOk} pathAUsed={pathAUsed} ratioOk={ratioOk}");
            Sum($"cells={string.Join(",", cellVerdicts)}");
            Sum($"label_note=SEED-PATHB only; NOT Δv-PASS/SEED-ABS-PASS/SEED-CALIB-PASS; prior 09f6414b/831818e4/06467b82 not re-run; Aprime_not_run");
            Sum(sessVerdict);
            Sum("===== M2-WGC-SEED-PATH-B-PRIME PROBE DONE =====");
            Both("===== M2-WGC-SEED-PATH-B-PRIME PROBE DONE =====");

            art.Close(); sum.Close();
            try { File.Copy(artPath, artPathSess, true); } catch { }
            try { File.Copy(sumPath, sumPathSess, true); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("M2-WGC-SEED-PATH-B-PRIME EXCEPTION " + ex);
            try { art.Close(); } catch { }
            try { sum.Close(); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        finally { _suiteRunning = false; try { MeasureButton.IsEnabled = true; } catch { } FlushLog(); try { CloseMainWindowSoon(); } catch { } }
    }

    private async Task<TrialRowWsp> OneTrialWsp(
        string arm, string cell, double trip, double final, bool dbl, int k,
        double cx0, double ppd, Func<double, double, Task> WaitNear, Action<string> Both)
    {
        var row = new TrialRowWsp { Arm = arm, Cell = cell, K = k, SeedPath = arm == "Arm-WGC-seed" ? "?" : (arm == "IVel0" ? "IVel0" : "base"), Phi = 0 };
        try
        {
            CompReset(0, cell); await WaitNear(0, 4); await Task.Delay(200);
            long qStart = Qpc;
            var a = _comp!.CreateSpringScalarAnimation();
            a.Period = TimeSpan.FromSeconds(M2WspPeriod); a.DampingRatio = (float)M2WspZeta;
            a.InitialValue = 0f; a.InitialVelocity = 0f; a.FinalValue = 240f;
            _visual!.StartAnimation("Translation.X", a);

            // phase catch-up (shadow log-only timing oracle — NOT written to InitialVelocity)
            MfProbe.mf_spring_at(M2WspPeriod, M2WspZeta, 0, 240, 0, trip, out var shXTarget, out var shVTarget);
            double xTargetAbs = Math.Abs(shXTarget);
            double catchDeadline = trip + 3.0 * M2WspFrameSec;
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
                        if (xTargetAbs < 1.0 || xNow >= 0.70 * xTargetAbs) break;
                    }
                }
                else if (tw < trip - 0.020) await Task.Delay(5);
            }

            long qComp = Qpc; // t_compositor_interrupt_QPC (same origin as WGC)
            double tComp = SecSince(qStart, qComp);
            row.Phi = tComp;
            row.TComp = tComp;
            row.PathbVariant = "PATHB-A";

            var preCaps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart && c.QpcArrive <= qComp)
                .OrderBy(c => c.QpcArrive).ToList();
            if (preCaps.Count < M2WspSeedN)
                preCaps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).TakeLast(Math.Max(8, M2WspSeedN)).ToList();

            // v_screen = last-two WGC (H5 denominator) — always computed for ratio
            double vScreen = double.NaN;
            if (preCaps.Count >= 2)
            {
                var c1 = preCaps[^2]; var c2 = preCaps[^1];
                double dt = SecSince(c1.QpcArrive, c2.QpcArrive);
                if (dt > 0.004) vScreen = (c2.Cx - c1.Cx) / ppd / dt;
            }
            row.VScreen = vScreen;
            row.VWgc = vScreen; // keep field for log compatibility; seed uses LS below

            // ---- PATHB-A: N=5 quadratic LS @ t_int=min(t_comp, t_last_WGC); FORBIDDEN extrapolate past t_last_WGC ----
            double vHat = double.NaN;
            string pathFail = "";
            var seedCaps = preCaps.TakeLast(M2WspSeedN).ToList();
            bool mono = true;
            for (int i = 1; i < seedCaps.Count; i++)
            {
                if (seedCaps[i].QpcArrive <= seedCaps[i - 1].QpcArrive) { mono = false; break; }
            }
            long qLastWgc = seedCaps.Count > 0 ? seedCaps[^1].QpcArrive : qComp;
            long qInt = qComp <= qLastWgc ? qComp : qLastWgc; // clamp: min(t_comp, t_last_WGC)
            double tLastWgc = SecSince(qStart, qLastWgc);
            double tInt = SecSince(qStart, qInt);
            row.TLastWgc = tLastWgc;
            row.TInt = tInt;
            row.TIntClamped = qComp > qLastWgc;
            if (seedCaps.Count < M2WspSeedN) pathFail = "seed_frames<N";
            else if (!mono) pathFail = "ts_not_mono";
            else
            {
                var seedFrames = seedCaps
                    .Select(c => (t: SecSince(qStart, c.QpcArrive), x: (c.Cx - cx0) / ppd, q: c.QpcArrive))
                    .ToList();
                var (vLs, aLs) = QuadVelAtWsp(seedFrames, tInt); // evaluate at clamped t_int
                if (double.IsNaN(vLs) || double.IsInfinity(vLs)) pathFail = "fit_nonfinite";
                else if (Math.Abs(vLs) < M2WspEps) pathFail = "below_eps";
                else { vHat = vLs; row.SeedAbsA = aLs; }
            }
            row.VWgc = !double.IsNaN(vHat) ? vHat : vScreen; // report seed estimate as v_wgc for A

            // log-only shadow at wall trip (FORBIDDEN as IV)
            MfProbe.mf_spring_at(M2WspPeriod, M2WspZeta, 0, 240, 0, tComp, out var shX, out var shV);
            row.ShadowV = shV;

            long qSwitch1 = 0, qSwitch2 = 0;
            long qNextChange = long.MaxValue;

            if (arm == "base")
            {
                row.IVelSet = double.NaN;
                row.SeedPath = "base";
                double commitSkew = Math.Max(M2WspFrameSec, (M2WspMeasW + 0.5) * M2WspFrameSec - trip);
                while (SecSince(qStart, Qpc) < trip + commitSkew) { /* spin */ }
                qSwitch1 = Qpc;
                await Task.Delay((int)Math.Max(400, (0.450 - (trip + commitSkew)) * 1000));
            }
            else
            {
                var rAnim = _comp.CreateSpringScalarAnimation();
                rAnim.Period = TimeSpan.FromSeconds(M2WspPeriod); rAnim.DampingRatio = (float)M2WspZeta;
                rAnim.FinalValue = dbl ? 360f : (float)final;
                if (arm == "Arm-WGC-seed")
                {
                    if (!double.IsNaN(vHat) && string.IsNullOrEmpty(pathFail))
                    {
                        row.IVelSet = M2WspCalib * vHat;
                        rAnim.InitialVelocity = (float)row.IVelSet;
                        row.SeedPath = "A";
                    }
                    else
                    {
                        // Path B fallback
                        row.IVelSet = 0;
                        // IV=null+0: leave InitialVelocity at default 0; do NOT set from shadow
                        rAnim.InitialVelocity = 0f;
                        row.SeedPath = "B";
                        row.PathBReason = pathFail;
                    }
                    if (!double.IsNaN(row.IVelSet) && !double.IsNaN(vScreen) && Math.Abs(vScreen) > 1e-6)
                        row.Ri = Math.Abs(row.IVelSet / vScreen);
                    else
                        row.Ri = double.NaN;
                }
                else // IVel0
                {
                    row.IVelSet = 0;
                    rAnim.InitialVelocity = 0f;
                    row.SeedPath = "IVel0";
                }
                _visual.StartAnimation("Translation.X", rAnim);
                long qCommit = Qpc;
                while (SecSince(qCommit, Qpc) < M2WspFrameSec * 0.85) { /* spin to apply */ }
                qSwitch1 = Qpc;

                if (dbl)
                {
                    await Task.Delay(80);
                    var r2 = _comp.CreateSpringScalarAnimation();
                    r2.Period = TimeSpan.FromSeconds(M2WspPeriod); r2.DampingRatio = (float)M2WspZeta;
                    r2.FinalValue = (float)final;
                    if (arm == "Arm-WGC-seed" && row.SeedPath == "A")
                    {
                        var mid = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qSwitch1).OrderBy(c => c.QpcArrive).TakeLast(M2WspSeedN).ToList();
                        double v2 = row.IVelSet;
                        if (mid.Count >= M2WspSeedN)
                        {
                            long qNow = Qpc;
                            double tNow = SecSince(qStart, qNow);
                            var midFrames = mid.Select(c => (t: SecSince(qStart, c.QpcArrive), x: (c.Cx - cx0) / ppd, q: c.QpcArrive)).ToList();
                            var (vLs2, _) = QuadVelAtWsp(midFrames, tNow);
                            if (!double.IsNaN(vLs2) && Math.Abs(vLs2) >= M2WspEps) v2 = M2WspCalib * vLs2;
                        }
                        r2.InitialVelocity = (float)v2;
                        row.IVelSet = v2;
                    }
                    else
                    {
                        r2.InitialVelocity = arm == "IVel0" ? 0f : (float)row.IVelSet;
                    }
                    _visual.StartAnimation("Translation.X", r2);
                    long qC2 = Qpc;
                    while (SecSince(qC2, Qpc) < M2WspFrameSec * 0.85) { /* spin */ }
                    qSwitch2 = Qpc;
                    qNextChange = long.MaxValue;
                }
                await Task.Delay(450);
            }

            long qEnd = Qpc;
            var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart && c.QpcArrive < qEnd)
                .OrderBy(c => c.QpcArrive)
                .Select(c => (t: SecSince(qStart, c.QpcArrive), x: (c.Cx - cx0) / ppd, q: c.QpcArrive))
                .ToList();

            if (caps.Count < M2WspMeasW * 2 + 2)
            {
                row.Valid = false; row.InvalidReason = "too_few_frames"; row.Hang = caps.Count < 4;
                Both($"TRIAL arm={arm} cell={cell} k={k} INVALID too_few_frames n={caps.Count} seed_path={row.SeedPath}");
                CompReset(0, cell + " end");
                return row;
            }

            long qSw = (dbl && qSwitch2 > 0) ? qSwitch2 : qSwitch1;
            if (qSw <= 0) qSw = qComp;
            double tSwNom = SecSince(qStart, qSw);
            double tLo = tSwNom - M2WspFrameSec;
            double tHi = tSwNom + M2WspFrameSec;
            if (tLo < 0) tLo = 0;

            double bestTs = 0.5 * (tLo + tHi);
            double bestGap = double.PositiveInfinity;
            var inWin = caps.Where(c => c.t >= tLo - M2WspFrameSec && c.t <= tHi + M2WspFrameSec).ToList();
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
            row.SigmaTs = M2WspFrameSec / Math.Sqrt(12.0);

            double tNext = (dbl && qSwitch2 > 0 && qSw == qSwitch1)
                ? SecSince(qStart, qSwitch2)
                : (qNextChange < long.MaxValue ? SecSince(qStart, qNextChange) : double.PositiveInfinity);

            var pre = caps.Where(c => c.t < bestTs).OrderBy(c => c.t).TakeLast(M2WspMeasW).ToList();
            var post = caps.Where(c => c.t > bestTs && c.t < tNext - 1e-4).OrderBy(c => c.t).Take(M2WspMeasW).ToList();

            if (pre.Count < M2WspMeasW || post.Count < M2WspMeasW)
            {
                row.Valid = false; row.InvalidReason = $"insuff_w pre={pre.Count} post={post.Count}";
                Both($"TRIAL arm={arm} cell={cell} k={k} INVALID insuff_w pre={pre.Count} post={post.Count} t_s={F(bestTs*1000,2)} seed_path={row.SeedPath}");
                CompReset(0, cell + " end");
                return row;
            }

            var (vPre, aPre) = QuadVelAtWsp(pre, bestTs);
            var (vPost, aPost) = QuadVelAtWsp(post, bestTs);
            row.VPre = vPre; row.VPost = vPost;
            row.AbsA = Math.Abs(aPre);
            if (row.AbsA > 50000 || Math.Abs(vPre) > 8000 || Math.Abs(vPost) > 8000)
            {
                row.Valid = false; row.InvalidReason = "num_unstable";
                Both($"TRIAL arm={arm} cell={cell} k={k} INVALID num_unstable |a|={F(row.AbsA,1)} v_pre={F(vPre,1)} v_post={F(vPost,1)}");
                CompReset(0, cell + " end");
                return row;
            }
            row.Dv = Math.Abs(vPost - vPre);
            double vRef = vPre;
            row.Thr = Math.Max(M2WspFloorThr, M2WspRelThr * Math.Abs(vRef));
            row.R = row.Thr > 0 ? row.Dv / row.Thr : double.NaN;
            row.Valid = !double.IsNaN(row.Dv) && !double.IsNaN(row.Thr) && !double.IsNaN(vPre) && !double.IsNaN(vPost);
            if (!row.Valid) row.InvalidReason = "nan_estimate";

            Both($"TRIAL arm={arm} cell={cell} k={k} phi={F(tComp*1000,2)} t_s={F(bestTs*1000,3)} v_pre={F(vPre,3)} v_post={F(vPost,3)} thr={F(row.Thr,3)} Δv={F(row.Dv,3)} r={F(row.R,4)} valid={row.Valid} ivel_set={F(row.IVelSet,3)} v_hat={F(vHat,3)} v_screen={F(vScreen,3)} r_i={F(row.Ri,4)} seed_path={row.SeedPath} pathb_variant={row.PathbVariant} t_int={F(tInt*1000,3)} t_last_WGC={F(tLastWgc*1000,3)} t_comp={F(tComp*1000,3)} t_int_clamped={row.TIntClamped} pathB={row.PathBReason} shadow_v_log={F(shV,2)} |a|={F(row.AbsA,2)} σ_ts={F(row.SigmaTs,5)} pre_n={pre.Count} post_n={post.Count}");

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
    /// Time scaled to frame units for Vandermonde conditioning at 60 Hz.</summary>
    private static (double v, double accel) QuadVelAtWsp(List<(double t, double x, long q)> frames, double t0)
    {
        int n = frames.Count;
        if (n < 3) return (double.NaN, double.NaN);
        double scale = M2WspFrameSec;
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
        if (!Solve3Wsp(A, rhs, out var coeff)) return (double.NaN, double.NaN);
        double v = coeff[1] / scale;
        double accel = 2.0 * coeff[2] / (scale * scale);
        if (double.IsNaN(v) || double.IsInfinity(v) || Math.Abs(v) > 1e6) return (double.NaN, double.NaN);
        if (double.IsNaN(accel) || double.IsInfinity(accel) || Math.Abs(accel) > 1e8) return (double.NaN, double.NaN);
        return (v, accel);
    }

    private static bool Solve3Wsp(double[,] A, double[] b, out double[] x)
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

    private static double PercentileWsp(List<double> xs, double p)
    {
        if (xs == null || xs.Count == 0) return double.NaN;
        var s = xs.OrderBy(v => v).ToList();
        double idx = p * (s.Count - 1);
        int i = (int)Math.Floor(idx);
        double f = idx - i;
        if (i + 1 >= s.Count) return s[^1];
        return s[i] * (1 - f) + s[i + 1] * f;
    }

    private static double MedianWsp(List<double> xs)
    {
        if (xs == null || xs.Count == 0) return double.NaN;
        var s = xs.OrderBy(v => v).ToList();
        int n = s.Count;
        return (n % 2 == 1) ? s[n / 2] : 0.5 * (s[n / 2 - 1] + s[n / 2]);
    }

    private static string Sha256FileWsp(string path)
    {
        using var fs = File.OpenRead(path);
        var hash = System.Security.Cryptography.SHA256.HashData(fs);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private sealed class TrialRowWsp
    {
        public string Arm = "";
        public string Cell = "";
        public int K;
        public double Phi, Ts, VPre, VPost, Thr, Dv, R, VWgc, VScreen, Ri, IVelSet, ShadowV, SigmaTs, AbsA, NvH, SeedAbsA;
        public double TInt, TLastWgc, TComp;
        public bool TIntClamped;
        public bool Valid, Hang, Judicable, PassThr, FailDetermined;
        public string SeedPath = "";
        public string PathBReason = "";
        public string PathbVariant = "PATHB-A";
        public string InvalidReason = "";
    }
}
