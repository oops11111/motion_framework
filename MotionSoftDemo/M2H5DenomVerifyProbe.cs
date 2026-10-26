// M2-H5-DENOM-VERIFY probe: PATHB-A=V2-clamp — new denom v_denom≡v_pre + C1 calib median(v_denom/v_hat).
// Gate: fd00a06058b239abfe03a52175463a52741718318ce99416a4ce762cf037889b @ 2026-10-26 16:09:00 Asia/Taipei
// Approved: pidan@2026-10-26 16:12:48 Australia/Perth
// Labels: H5-DENOM-VERIFY-* only. FORBIDDEN: Δv-PASS / SEED-*-PASS / SEED-TRUTH-PASS / rewrite H5-DENOM-PASS as landed / shadow IV / composition.h edit / silent 0.7511 / raise thr / steal-open A′ / flip seven hashes.
// Phase-Calib: calib_probe=1.0 expose v_hat; collect v_denom/v_hat; NO score freeze yet.
// Phase-Score: frozen calib_new; IV=(float)(calib_new·v̂); H5 r_i=|v_seed/v_denom|; smoke ≥17/20; ctrl KEEP.
// v_screen (last-two WGC) = aux only; NOT gate denom.
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
    const double M2HdvPeriod = 0.32;
    const double M2HdvZeta = 0.78;
    // FORBIDDEN silent 0.7510829456 — calib comes from phase file / env only
    const double M2HdvCalibProbe = 1.0; // Phase-Calib only; expose v̂
    const int M2HdvSeedN = 5;
    const int M2HdvMeasW = 5;
    const int M2HdvNeedN = 20;
    const int M2HdvHangCap = 2;
    const double M2HdvFloorThr = 20.0;
    const double M2HdvRelThr = 0.05;
    const double M2HdvEps = 5.0;
    const double M2HdvFrameSec = 1.0 / 60.0;
    const double M2HdvMedRLo = 0.8;
    const double M2HdvMedRHi = 1.2;
    const double M2HdvRiCap = 2.0;

    private async Task RunM2H5DenomVerifyProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true;
        try { MeasureButton.IsEnabled = false; } catch { }

        string artDir = @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string sessFile = Path.Combine(artDir, "m2_h5_denom_verify_session.txt");
        string sessEnv = File.Exists(sessFile) ? File.ReadAllText(sessFile).Trim() : (Environment.GetEnvironmentVariable("MF_M2_H5_DENOM_VERIFY_SESSION") ?? "1");
        if (string.IsNullOrWhiteSpace(sessEnv)) sessEnv = "1";

        string phaseFile = Path.Combine(artDir, "m2_h5_denom_verify_phase.txt");
        string phaseEnv = File.Exists(phaseFile) ? File.ReadAllText(phaseFile).Trim() : (Environment.GetEnvironmentVariable("MF_M2_H5_DENOM_VERIFY_PHASE") ?? "calib");
        bool isCalib = phaseEnv.Equals("calib", StringComparison.OrdinalIgnoreCase);
        bool isScore = phaseEnv.Equals("score", StringComparison.OrdinalIgnoreCase);
        if (!isCalib && !isScore) { isCalib = true; phaseEnv = "calib"; }

        double calibUsed;
        string calibSource;
        if (isCalib)
        {
            calibUsed = M2HdvCalibProbe;
            calibSource = "calib_probe=1.0";
        }
        else
        {
            string frozenFile = Path.Combine(artDir, "m2_h5_denom_verify_calib_frozen.txt");
            string frozenEnv = Environment.GetEnvironmentVariable("MF_M2_H5_DENOM_VERIFY_CALIB") ?? "";
            string frozenTxt = File.Exists(frozenFile) ? File.ReadAllText(frozenFile).Trim() : frozenEnv.Trim();
            if (string.IsNullOrWhiteSpace(frozenTxt) || !double.TryParse(frozenTxt, NumberStyles.Float, CultureInfo.InvariantCulture, out calibUsed)
                || !double.IsFinite(calibUsed) || calibUsed <= 0)
            {
                // hard fail — never fall back to 0.7511
                string failPath = Path.Combine(artDir, $"m2_h5_denom_verify_s{sessEnv}_summary.txt");
                File.WriteAllText(failPath, $"# INVALID missing frozen calib_new for Phase-Score\nSESSION_INVALID\n===== M2-H5-DENOM-VERIFY PROBE DONE =====\n", new UTF8Encoding(false));
                File.WriteAllText(Path.Combine(artDir, "m2_h5_denom_verify_capture_summary.txt"), File.ReadAllText(failPath), new UTF8Encoding(false));
                _suiteRunning = false;
                try { MeasureButton.IsEnabled = true; } catch { }
                FlushLog();
                try { CloseMainWindowSoon(); } catch { }
                return;
            }
            // reject silent old calib
            if (Math.Abs(calibUsed - 0.7510829456) < 1e-12)
            {
                string failPath = Path.Combine(artDir, $"m2_h5_denom_verify_s{sessEnv}_summary.txt");
                File.WriteAllText(failPath, $"# INVALID forbidden silent calib=0.7510829456\nSESSION_INVALID\n===== M2-H5-DENOM-VERIFY PROBE DONE =====\n", new UTF8Encoding(false));
                File.WriteAllText(Path.Combine(artDir, "m2_h5_denom_verify_capture_summary.txt"), File.ReadAllText(failPath), new UTF8Encoding(false));
                _suiteRunning = false;
                try { MeasureButton.IsEnabled = true; } catch { }
                FlushLog();
                try { CloseMainWindowSoon(); } catch { }
                return;
            }
            calibSource = "frozen_calib_new";
        }

        string preregPath = Path.Combine(artDir, "m2_h5_denom_verify_prereg.txt");
        string preregHash = File.Exists(preregPath) ? Sha256FileHdv(preregPath) : "MISSING_PREREG";

        string artPath = Path.Combine(artDir, "m2_h5_denom_verify_capture_raw.txt");
        string sumPath = Path.Combine(artDir, "m2_h5_denom_verify_capture_summary.txt");
        string artPathSess = Path.Combine(artDir, $"m2_h5_denom_verify_s{sessEnv}_raw.txt");
        string sumPathSess = Path.Combine(artDir, $"m2_h5_denom_verify_s{sessEnv}_summary.txt");
        string calibSampPath = Path.Combine(artDir, $"m2_h5_denom_verify_calib_s{sessEnv}_samples.txt");

        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        var sum = new StreamWriter(sumPath, false, new UTF8Encoding(false));
        var calibSamp = isCalib ? new StreamWriter(calibSampPath, false, new UTF8Encoding(false)) : null;
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        void Sum(string s) { Both(s); sum.WriteLine(s); sum.Flush(); }

        string host = Environment.MachineName;
        int pid = Environment.ProcessId;
        Both($"# m2_h5_denom_verify_s{sessEnv}_raw.txt — M2-H5-DENOM-VERIFY PATHB-A + v_denom≡v_pre + C1");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Australia/Perth; hostname={host}; pid={pid}; sess={sessEnv}; phase={phaseEnv}");
        Both($"# gate=fd00a06058b239abfe03a52175463a52741718318ce99416a4ce762cf037889b LOCKED_AT=2026-10-26 16:09:00 Asia/Taipei");
        Both($"# approved=pidan@2026-10-26 16:12:48 Australia/Perth");
        Both($"# prereg_hash={preregHash}");
        Both($"# path_A=PATHB-A=V2-clamp N={M2HdvSeedN} order=2 t_int=min(t_comp,t_last_WGC) calib={calibUsed:R} calib_source={calibSource} eps={M2HdvEps} P-VS=OFF Aprime=OFF");
        Both($"# path_B=IV=null+0; ticket PASS evaluates A only");
        Both($"# measure=quad_LS w={M2HdvMeasW}; thr=max({M2HdvFloorThr},{M2HdvRelThr}*|v_ref|); hang_cap={M2HdvHangCap}");
        Both("# judicable P1=|a|*σ_ts<=thr; P2=thr>=Nv_h_cell; IVel0 only t060 fwd+rev");
        Both("# arms=Arm-WGC-seed|base|IVel0; smoke_cells=t060{fwd,rev}; t156=rule-SKIP; t300*=optional-not-run");
        Both("# H5 ratio GATE: r_i=|v_seed/v_denom|; v_denom≡v_pre (quad LS w=5 same t_s); med_r∈[0.8,1.2]; no r_i≥2.0");
        Both("# H5 aux only: r_2F=|v_seed/v_screen|; |v_denom-v_screen|; v_screen=last-two WGC NOT gate denom");
        Both("# C1: calib_new=median(v_denom/v_hat); FORBIDDEN median(v_screen/v_hat); FORBIDDEN silent 0.7511");
        Both("# FORBIDDEN: shadow IV; composition.h edit; Δv-PASS; SEED-*-PASS; SEED-TRUTH-PASS; rewrite H5-DENOM-PASS landed; flip f17871f1/a1161c77/09f6414b/831818e4/06467b82/276b97f0/d891df9f; PERF-CI; thaw Bv; reopen S1/R-3/R-2; raise thr; steal-open A′; immediate Δv″");
        Both("# labels=H5-DENOM-VERIFY-* only; 成功≠落地采用≠Δv-PASS; ≠PERF-CI; 落地=否");

        if (calibSamp != null)
        {
            calibSamp.WriteLine($"# m2_h5_denom_verify_calib_s{sessEnv}_samples.txt — C1 samples v_denom/v_hat");
            calibSamp.WriteLine($"# sess={sessEnv} pid={pid} phase=calib calib_probe={M2HdvCalibProbe:R}");
            calibSamp.WriteLine("# columns: cell k v_hat v_denom ratio=v_denom/v_hat v_screen ivel_set shadow_v_log t_int t_comp valid");
            calibSamp.Flush();
        }

        var cells = new (string name, double trip, double final, bool dbl, bool core)[]
        {
            ("t060fwd", 0.060, 360, false, true),
            ("t060rev", 0.060, 0, false, true),
        };
        string[] armsOrder = { "base", "Arm-WGC-seed", "IVel0" };

        var rows = new List<TrialRowHdv>();
        var hangByCell = new Dictionary<string, int>(StringComparer.Ordinal);
        var calibRatios = new List<double>();

        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "m2-h5-denom-verify");
            CompReset(0, "m2hdv start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");

            var calQ0 = Qpc; CompReset(37, "m2hdv cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "m2hdv cal end"); await Task.Delay(500);
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
                    int valid = 0, attempt = 0, maxAttempt = M2HdvNeedN * 3;
                    while (valid < M2HdvNeedN && attempt < maxAttempt)
                    {
                        var row = await OneTrialHdv(arm, cell.name, cell.trip, cell.final, cell.dbl, attempt, cx0, ppd, calibUsed, WaitNear, Both);
                        rows.Add(row);
                        attempt++;
                        if (row.Hang)
                        {
                            hangByCell[cell.name] = hangByCell[cell.name] + 1;
                            Both($"HANG cell={cell.name} arm={arm} k={attempt - 1} hang_n={hangByCell[cell.name]}");
                            if (hangByCell[cell.name] > M2HdvHangCap)
                            {
                                Both($"INVALID hang_cap exceeded cell={cell.name} hang={hangByCell[cell.name]}");
                                goto DONE_EARLY;
                            }
                            continue;
                        }
                        if (row.Valid) valid++;

                        // C1 sample: Arm-WGC-seed path A, finite v_denom & v_hat, v_hat≠0, |v_hat|≥ε
                        if (isCalib && arm == "Arm-WGC-seed" && row.SeedPath == "A" && row.Valid
                            && double.IsFinite(row.VHat) && row.VHat != 0 && Math.Abs(row.VHat) >= M2HdvEps
                            && double.IsFinite(row.VDenom) && row.VDenom != 0)
                        {
                            double ratio = row.VDenom / row.VHat;
                            if (double.IsFinite(ratio))
                            {
                                calibRatios.Add(ratio);
                                calibSamp?.WriteLine($"cell={row.Cell} k={row.K} v_hat={row.VHat:R} v_denom={row.VDenom:R} ratio={ratio:R} v_screen={row.VScreen:R} ivel_set={row.IVelSet:R} shadow_v_log={row.ShadowV:R} t_int={row.TInt:R} t_comp={row.TComp:R} valid=1");
                                calibSamp?.Flush();
                            }
                        }
                    }
                    Both($"CELL_ARM_DONE cell={cell.name} arm={arm} valid={valid}/{M2HdvNeedN} attempts={attempt}");
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
                nvh[cell.name] = PercentileHdv(baseDvs, 0.95);
                var sts = rows.Where(r => r.Arm == "base" && r.Cell == cell.name && r.Valid && !double.IsNaN(r.SigmaTs))
                    .Select(r => r.SigmaTs).ToList();
                sigmaTs[cell.name] = sts.Count > 0 ? sts.Average() : M2HdvFrameSec;
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
                Both($"ADJ arm={r.Arm} cell={r.Cell} k={r.K} phi={F(r.Phi,0)} t_s={F(r.Ts * 1000, 3)}ms v_pre={F(r.VPre,3)} v_denom={F(r.VDenom,3)} v_post={F(r.VPost,3)} thr={F(r.Thr,3)} Δv={F(r.Dv,3)} r={F(r.R,4)} Nv_h={F(r.NvH,3)} jud={r.Judicable} pass={r.PassThr} reason={reason} ivel={F(r.IVelSet,3)} v_hat={F(r.VHat,3)} v_screen={F(r.VScreen,3)} r_i={F(r.Ri,4)} r_2F={F(r.R2F,4)} |v_denom-v_screen|={F(r.AbsDenomScreen,3)} seed_path={r.SeedPath} pathb_variant={r.PathbVariant} t_int={F(r.TInt*1000,3)} t_last_WGC={F(r.TLastWgc*1000,3)} t_comp={F(r.TComp*1000,3)} clamped={r.TIntClamped} shadow_v_log={F(r.ShadowV,2)}");
            }

            // H5 ratio: r_i = |v_seed/v_denom|; v_denom≡v_pre; INVALID if denom 0/nonfinite
            var ratioRows = rows.Where(r => r.Arm == "Arm-WGC-seed" && r.SeedPath == "A" && r.Valid
                && !double.IsNaN(r.Ri) && !double.IsInfinity(r.Ri)).ToList();
            var ratios = ratioRows.Select(r => r.Ri).OrderBy(x => x).ToList();
            double medR = MedianHdv(ratios);
            double maxR = ratios.Count > 0 ? ratios.Max() : double.NaN;
            bool anyRiGe2 = ratios.Any(x => x >= M2HdvRiCap);
            bool ratioOk = ratios.Count > 0 && !double.IsNaN(medR) && medR >= M2HdvMedRLo && medR <= M2HdvMedRHi && !anyRiGe2;
            int nPathA = rows.Count(r => r.Arm == "Arm-WGC-seed" && r.SeedPath == "A");
            int nPathB = rows.Count(r => r.Arm == "Arm-WGC-seed" && r.SeedPath == "B");
            bool pathAUsed = nPathA > 0;

            // no-shadow-IV evidence: at least one cell ivel_set = calib*v_hat ≠ shadow_v_log
            var evidence = rows.FirstOrDefault(r => r.Arm == "Arm-WGC-seed" && r.SeedPath == "A" && r.Valid
                && double.IsFinite(r.IVelSet) && double.IsFinite(r.ShadowV) && Math.Abs(r.IVelSet - r.ShadowV) > 1.0);
            if (evidence != null)
                Both($"NO_SHADOW_IV_EVIDENCE cell={evidence.Cell} k={evidence.K} ivel_set={evidence.IVelSet:R} (=calib*v_hat={calibUsed:R}*{evidence.VHat:R}) ≠ shadow_v_log={evidence.ShadowV:R}");

            Sum($"# m2_h5_denom_verify_s{sessEnv}_summary.txt");
            Sum($"session={sessEnv} pid={pid} hostname={host} phase={phaseEnv} prereg_hash={preregHash} calib={calibUsed:R} calib_source={calibSource} seed_N={M2HdvSeedN} meas_w={M2HdvMeasW} hang_cap={M2HdvHangCap} eps={M2HdvEps}");
            Sum($"path_declare=PATHB-A=V2-clamp N={M2HdvSeedN} order=2 t_int=min(t_comp,t_last_WGC) calib_exact={calibUsed:R} P-VS=OFF Aprime=OFF");
            Sum($"pathb_variant=PATHB-A");
            Sum($"composition_expected=a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418");
            int nClamp = rows.Count(r => r.Arm == "Arm-WGC-seed" && r.TIntClamped);
            Sum($"H5_ratio denom=v_pre n={ratios.Count} med_r={F(medR,4)} max_r={F(maxR,4)} any_ri_ge2={anyRiGe2} ratioOk={ratioOk} pathA_n={nPathA} pathB_n={nPathB}");
            Sum($"t_int_clamp n_clamped={nClamp} rule=min(t_comp,t_last_WGC) pathb_variant=PATHB-A P-VS=OFF");
            if (isCalib)
            {
                double medCal = MedianHdv(calibRatios);
                Sum($"C1_calib_samples n={calibRatios.Count} median_v_denom_over_v_hat={medCal:R} (session-local; aggregate across sessions for freeze)");
            }

            bool allCorePass = true;
            bool basePass = true;
            bool ivel0FailOk = true;
            var cellVerdicts = new List<string>();

            foreach (var cell in cells)
            {
                double nv = nvh.GetValueOrDefault(cell.name, double.NaN);
                double st = sigmaTs.GetValueOrDefault(cell.name, double.NaN);
                double aa = aAbs.GetValueOrDefault(cell.name, double.NaN);

                var wgcValid = rows.Where(r => r.Arm == "Arm-WGC-seed" && r.Cell == cell.name && r.Valid && r.SeedPath == "A").Take(M2HdvNeedN).ToList();
                double thrMed = wgcValid.Count > 0 ? MedianHdv(wgcValid.Select(x => x.Thr).ToList()) : M2HdvFloorThr;
                bool cellJud = !double.IsNaN(nv) && thrMed >= nv && !double.IsNaN(aa) && !double.IsNaN(st) && aa * st <= thrMed;

                int nPass = wgcValid.Count(r => r.PassThr && !r.FailDetermined);
                bool cellPass = cellJud && wgcValid.Count >= M2HdvNeedN && nPass >= 17;
                bool cellSkip = !cellJud;

                var baseValid = rows.Where(r => r.Arm == "base" && r.Cell == cell.name && r.Valid).Take(M2HdvNeedN).ToList();
                int basePassN = baseValid.Count(r => r.Dv <= r.Thr);
                bool baseCellPass = baseValid.Count >= M2HdvNeedN && basePassN >= 17;

                var ivelValid = rows.Where(r => r.Arm == "IVel0" && r.Cell == cell.name && r.Valid).Take(M2HdvNeedN).ToList();
                int ivelPassN = ivelValid.Count(r => r.PassThr && !r.FailDetermined);
                bool ivelCellFail = ivelValid.Count >= M2HdvNeedN && ivelPassN < 17;

                string verdict;
                if (cellSkip)
                {
                    verdict = "H5-DENOM-VERIFY-SKIP";
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

            bool sessInvalid = hangByCell.Values.Any(h => h > M2HdvHangCap);
            // too_few_frames heavy → treat as INVALID candidate for replacement
            int tooFew = rows.Count(r => r.InvalidReason == "too_few_frames");
            if (tooFew > 30) { Both($"INVALID too_few_frames_heavy n={tooFew}"); sessInvalid = true; }

            string sessVerdict;
            if (sessInvalid) sessVerdict = "SESSION_INVALID";
            else if (isCalib)
            {
                // calib phase: session ok if we got enough C1 samples; smoke not scored for PASS
                sessVerdict = calibRatios.Count >= 10 ? "SESSION_CALIB_OK" : "SESSION_CALIB_SPARSE";
            }
            else if (!pathAUsed) sessVerdict = "SESSION_FAIL_CANDIDATE";
            else if (allCorePass && basePass && ivel0FailOk && ratioOk && pathAUsed) sessVerdict = "SESSION_PASS_CANDIDATE";
            else if (cellVerdicts.Any(v => v.Contains("FAIL")) || !ratioOk || !basePass || !ivel0FailOk) sessVerdict = "SESSION_FAIL_CANDIDATE";
            else sessVerdict = "SESSION_INCONCLUSIVE_CANDIDATE";

            Sum($"ctrl basePass={basePass} IVel0_t060_both_FAIL={ivel0FailOk} pathAUsed={pathAUsed} ratioOk={ratioOk}");
            Sum($"cells={string.Join(",", cellVerdicts)}");
            Sum($"label_note=H5-DENOM-VERIFY only; NOT Δv-PASS/SEED-*-PASS/SEED-TRUTH-PASS; seven hashes not flipped; Aprime_not_run; land=NO; silent_0.7511=FORBIDDEN");
            Sum(sessVerdict);
            Sum("===== M2-H5-DENOM-VERIFY PROBE DONE =====");
            Both("===== M2-H5-DENOM-VERIFY PROBE DONE =====");

            art.Close(); sum.Close();
            try { calibSamp?.Close(); } catch { }
            try { File.Copy(artPath, artPathSess, true); } catch { }
            try { File.Copy(sumPath, sumPathSess, true); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("M2-H5-DENOM-VERIFY EXCEPTION " + ex);
            try { art.Close(); } catch { }
            try { sum.Close(); } catch { }
            try { calibSamp?.Close(); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        finally { _suiteRunning = false; try { MeasureButton.IsEnabled = true; } catch { } FlushLog(); try { CloseMainWindowSoon(); } catch { } }
    }

    private async Task<TrialRowHdv> OneTrialHdv(
        string arm, string cell, double trip, double final, bool dbl, int k,
        double cx0, double ppd, double calib, Func<double, double, Task> WaitNear, Action<string> Both)
    {
        var row = new TrialRowHdv { Arm = arm, Cell = cell, K = k, SeedPath = arm == "Arm-WGC-seed" ? "?" : (arm == "IVel0" ? "IVel0" : "base"), Phi = 0 };
        try
        {
            CompReset(0, cell); await WaitNear(0, 4); await Task.Delay(200);
            long qStart = Qpc;
            var a = _comp!.CreateSpringScalarAnimation();
            a.Period = TimeSpan.FromSeconds(M2HdvPeriod); a.DampingRatio = (float)M2HdvZeta;
            a.InitialValue = 0f; a.InitialVelocity = 0f; a.FinalValue = 240f;
            _visual!.StartAnimation("Translation.X", a);

            MfProbe.mf_spring_at(M2HdvPeriod, M2HdvZeta, 0, 240, 0, trip, out var shXTarget, out var shVTarget);
            double xTargetAbs = Math.Abs(shXTarget);
            double catchDeadline = trip + 3.0 * M2HdvFrameSec;
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

            long qComp = Qpc;
            double tComp = SecSince(qStart, qComp);
            row.Phi = tComp;
            row.TComp = tComp;
            row.PathbVariant = "PATHB-A";

            var preCaps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart && c.QpcArrive <= qComp)
                .OrderBy(c => c.QpcArrive).ToList();
            if (preCaps.Count < M2HdvSeedN)
                preCaps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).TakeLast(Math.Max(8, M2HdvSeedN)).ToList();

            // v_screen = last-two WGC — AUX only
            double vScreen = double.NaN;
            if (preCaps.Count >= 2)
            {
                var c1 = preCaps[^2]; var c2 = preCaps[^1];
                double dt = SecSince(c1.QpcArrive, c2.QpcArrive);
                if (dt > 0.004) vScreen = (c2.Cx - c1.Cx) / ppd / dt;
            }
            row.VScreen = vScreen;

            // PATHB-A: N=5 quadratic LS @ t_int=min(t_comp, t_last_WGC)
            double vHat = double.NaN;
            string pathFail = "";
            var seedCaps = preCaps.TakeLast(M2HdvSeedN).ToList();
            bool mono = true;
            for (int i = 1; i < seedCaps.Count; i++)
            {
                if (seedCaps[i].QpcArrive <= seedCaps[i - 1].QpcArrive) { mono = false; break; }
            }
            long qLastWgc = seedCaps.Count > 0 ? seedCaps[^1].QpcArrive : qComp;
            long qInt = qComp <= qLastWgc ? qComp : qLastWgc;
            double tLastWgc = SecSince(qStart, qLastWgc);
            double tInt = SecSince(qStart, qInt);
            row.TLastWgc = tLastWgc;
            row.TInt = tInt;
            row.TIntClamped = qComp > qLastWgc;
            if (seedCaps.Count < M2HdvSeedN) pathFail = "seed_frames<N";
            else if (!mono) pathFail = "ts_not_mono";
            else
            {
                var seedFrames = seedCaps
                    .Select(c => (t: SecSince(qStart, c.QpcArrive), x: (c.Cx - cx0) / ppd, q: c.QpcArrive))
                    .ToList();
                var (vLs, aLs) = QuadVelAtHdv(seedFrames, tInt);
                if (double.IsNaN(vLs) || double.IsInfinity(vLs)) pathFail = "fit_nonfinite";
                else if (Math.Abs(vLs) < M2HdvEps) pathFail = "below_eps";
                else { vHat = vLs; row.SeedAbsA = aLs; }
            }
            row.VHat = vHat;
            row.VWgc = !double.IsNaN(vHat) ? vHat : vScreen;

            MfProbe.mf_spring_at(M2HdvPeriod, M2HdvZeta, 0, 240, 0, tComp, out var shX, out var shV);
            row.ShadowV = shV;

            long qSwitch1 = 0, qSwitch2 = 0;
            long qNextChange = long.MaxValue;

            if (arm == "base")
            {
                row.IVelSet = double.NaN;
                row.SeedPath = "base";
                double commitSkew = Math.Max(M2HdvFrameSec, (M2HdvMeasW + 0.5) * M2HdvFrameSec - trip);
                while (SecSince(qStart, Qpc) < trip + commitSkew) { /* spin */ }
                qSwitch1 = Qpc;
                await Task.Delay((int)Math.Max(400, (0.450 - (trip + commitSkew)) * 1000));
            }
            else
            {
                var rAnim = _comp.CreateSpringScalarAnimation();
                rAnim.Period = TimeSpan.FromSeconds(M2HdvPeriod); rAnim.DampingRatio = (float)M2HdvZeta;
                rAnim.FinalValue = dbl ? 360f : (float)final;
                if (arm == "Arm-WGC-seed")
                {
                    if (!double.IsNaN(vHat) && string.IsNullOrEmpty(pathFail))
                    {
                        row.IVelSet = calib * vHat;
                        rAnim.InitialVelocity = (float)row.IVelSet;
                        row.SeedPath = "A";
                    }
                    else
                    {
                        row.IVelSet = 0;
                        rAnim.InitialVelocity = 0f;
                        row.SeedPath = "B";
                        row.PathBReason = pathFail;
                    }
                    // r_i deferred until v_denom(=v_pre) known; aux r_2F from v_screen now
                    if (!double.IsNaN(row.IVelSet) && !double.IsNaN(vScreen) && Math.Abs(vScreen) > 1e-6)
                        row.R2F = Math.Abs(row.IVelSet / vScreen);
                    else
                        row.R2F = double.NaN;
                }
                else // IVel0
                {
                    row.IVelSet = 0;
                    rAnim.InitialVelocity = 0f;
                    row.SeedPath = "IVel0";
                }
                _visual.StartAnimation("Translation.X", rAnim);
                long qCommit = Qpc;
                while (SecSince(qCommit, Qpc) < M2HdvFrameSec * 0.85) { /* spin */ }
                qSwitch1 = Qpc;

                if (dbl)
                {
                    await Task.Delay(80);
                    var r2 = _comp.CreateSpringScalarAnimation();
                    r2.Period = TimeSpan.FromSeconds(M2HdvPeriod); r2.DampingRatio = (float)M2HdvZeta;
                    r2.FinalValue = (float)final;
                    if (arm == "Arm-WGC-seed" && row.SeedPath == "A")
                    {
                        var mid = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qSwitch1).OrderBy(c => c.QpcArrive).TakeLast(M2HdvSeedN).ToList();
                        double v2 = row.IVelSet;
                        if (mid.Count >= M2HdvSeedN)
                        {
                            long qNow = Qpc;
                            double tNow = SecSince(qStart, qNow);
                            var midFrames = mid.Select(c => (t: SecSince(qStart, c.QpcArrive), x: (c.Cx - cx0) / ppd, q: c.QpcArrive)).ToList();
                            var (vLs2, _) = QuadVelAtHdv(midFrames, tNow);
                            if (!double.IsNaN(vLs2) && Math.Abs(vLs2) >= M2HdvEps) v2 = calib * vLs2;
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
                    while (SecSince(qC2, Qpc) < M2HdvFrameSec * 0.85) { /* spin */ }
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

            if (caps.Count < M2HdvMeasW * 2 + 2)
            {
                row.Valid = false; row.InvalidReason = "too_few_frames"; row.Hang = caps.Count < 4;
                Both($"TRIAL arm={arm} cell={cell} k={k} INVALID too_few_frames n={caps.Count} seed_path={row.SeedPath}");
                CompReset(0, cell + " end");
                return row;
            }

            long qSw = (dbl && qSwitch2 > 0) ? qSwitch2 : qSwitch1;
            if (qSw <= 0) qSw = qComp;
            double tSwNom = SecSince(qStart, qSw);
            double tLo = tSwNom - M2HdvFrameSec;
            double tHi = tSwNom + M2HdvFrameSec;
            if (tLo < 0) tLo = 0;

            double bestTs = 0.5 * (tLo + tHi);
            double bestGap = double.PositiveInfinity;
            var inWin = caps.Where(c => c.t >= tLo - M2HdvFrameSec && c.t <= tHi + M2HdvFrameSec).ToList();
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
            row.SigmaTs = M2HdvFrameSec / Math.Sqrt(12.0);

            double tNext = (dbl && qSwitch2 > 0 && qSw == qSwitch1)
                ? SecSince(qStart, qSwitch2)
                : (qNextChange < long.MaxValue ? SecSince(qStart, qNextChange) : double.PositiveInfinity);

            var pre = caps.Where(c => c.t < bestTs).OrderBy(c => c.t).TakeLast(M2HdvMeasW).ToList();
            var post = caps.Where(c => c.t > bestTs && c.t < tNext - 1e-4).OrderBy(c => c.t).Take(M2HdvMeasW).ToList();

            if (pre.Count < M2HdvMeasW || post.Count < M2HdvMeasW)
            {
                row.Valid = false; row.InvalidReason = $"insuff_w pre={pre.Count} post={post.Count}";
                Both($"TRIAL arm={arm} cell={cell} k={k} INVALID insuff_w pre={pre.Count} post={post.Count} t_s={F(bestTs*1000,2)} seed_path={row.SeedPath}");
                CompReset(0, cell + " end");
                return row;
            }

            var (vPre, aPre) = QuadVelAtHdv(pre, bestTs);
            var (vPost, aPost) = QuadVelAtHdv(post, bestTs);
            row.VPre = vPre; row.VPost = vPost;
            // KEEP: v_denom ≡ v_pre (quad LS w=5 same estimate point)
            row.VDenom = vPre;
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
            row.Thr = Math.Max(M2HdvFloorThr, M2HdvRelThr * Math.Abs(vRef));
            row.R = row.Thr > 0 ? row.Dv / row.Thr : double.NaN;
            row.Valid = !double.IsNaN(row.Dv) && !double.IsNaN(row.Thr) && !double.IsNaN(vPre) && !double.IsNaN(vPost);
            if (!row.Valid) row.InvalidReason = "nan_estimate";

            // GATE ratio: r_i = |v_seed/v_denom|; INVALID sample if denom 0/nonfinite
            if (arm == "Arm-WGC-seed" && row.SeedPath == "A" && row.Valid)
            {
                if (!double.IsFinite(row.VDenom) || row.VDenom == 0)
                {
                    row.Ri = double.NaN; // sample INVALID for ratio
                }
                else if (double.IsFinite(row.IVelSet))
                {
                    row.Ri = Math.Abs(row.IVelSet / row.VDenom);
                }
                else row.Ri = double.NaN;
                if (double.IsFinite(row.VDenom) && double.IsFinite(vScreen))
                    row.AbsDenomScreen = Math.Abs(row.VDenom - vScreen);
            }

            Both($"TRIAL arm={arm} cell={cell} k={k} phi={F(tComp*1000,2)} t_s={F(bestTs*1000,3)} v_pre={F(vPre,3)} v_denom={F(row.VDenom,3)} v_post={F(vPost,3)} thr={F(row.Thr,3)} Δv={F(row.Dv,3)} r={F(row.R,4)} valid={row.Valid} ivel_set={F(row.IVelSet,3)} v_hat={F(vHat,3)} v_screen={F(vScreen,3)} r_i={F(row.Ri,4)} r_2F={F(row.R2F,4)} seed_path={row.SeedPath} pathb_variant={row.PathbVariant} t_int={F(tInt*1000,3)} t_last_WGC={F(tLastWgc*1000,3)} t_comp={F(tComp*1000,3)} t_int_clamped={row.TIntClamped} pathB={row.PathBReason} shadow_v_log={F(shV,2)} |a|={F(row.AbsA,2)} σ_ts={F(row.SigmaTs,5)} pre_n={pre.Count} post_n={post.Count}");

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

    private static (double v, double accel) QuadVelAtHdv(List<(double t, double x, long q)> frames, double t0)
    {
        int n = frames.Count;
        if (n < 3) return (double.NaN, double.NaN);
        double scale = M2HdvFrameSec;
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
        if (!Solve3Hdv(A, rhs, out var coeff)) return (double.NaN, double.NaN);
        double v = coeff[1] / scale;
        double accel = 2.0 * coeff[2] / (scale * scale);
        if (double.IsNaN(v) || double.IsInfinity(v) || Math.Abs(v) > 1e6) return (double.NaN, double.NaN);
        if (double.IsNaN(accel) || double.IsInfinity(accel) || Math.Abs(accel) > 1e8) return (double.NaN, double.NaN);
        return (v, accel);
    }

    private static bool Solve3Hdv(double[,] A, double[] b, out double[] x)
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

    private static double PercentileHdv(List<double> xs, double p)
    {
        if (xs == null || xs.Count == 0) return double.NaN;
        var s = xs.OrderBy(v => v).ToList();
        double idx = p * (s.Count - 1);
        int i = (int)Math.Floor(idx);
        double f = idx - i;
        if (i + 1 >= s.Count) return s[^1];
        return s[i] * (1 - f) + s[i + 1] * f;
    }

    private static double MedianHdv(List<double> xs)
    {
        if (xs == null || xs.Count == 0) return double.NaN;
        var s = xs.OrderBy(v => v).ToList();
        int n = s.Count;
        return (n % 2 == 1) ? s[n / 2] : 0.5 * (s[n / 2 - 1] + s[n / 2]);
    }

    private static string Sha256FileHdv(string path)
    {
        using var fs = File.OpenRead(path);
        var hash = System.Security.Cryptography.SHA256.HashData(fs);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private sealed class TrialRowHdv
    {
        public string Arm = "";
        public string Cell = "";
        public int K;
        public double Phi, Ts, VPre, VPost, VDenom, Thr, Dv, R, VWgc, VScreen, VHat, Ri, R2F, AbsDenomScreen, IVelSet, ShadowV, SigmaTs, AbsA, NvH, SeedAbsA;
        public double TInt, TLastWgc, TComp;
        public bool TIntClamped;
        public bool Valid, Hang, Judicable, PassThr, FailDetermined;
        public string SeedPath = "";
        public string PathBReason = "";
        public string PathbVariant = "PATHB-A";
        public string InvalidReason = "";
    }
}
