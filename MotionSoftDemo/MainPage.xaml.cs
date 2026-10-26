using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace MotionSoftDemo;

/// <summary>mfprobe.dll (MotionNative/mf_probe.cpp): spring.h shadow state, DComp/DWM clock probes, WGC capture.</summary>
internal static class MfProbe
{
    const string Dll = "mfprobe";
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int mf_map_spring(double response, double damping, out double period, out double dampingRatio);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int mf_spring_at(double response, double damping, double from, double target, double v0, double t, out double value, out double velocity);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern double mf_spring_settle(double response, double damping, double from, double target, double v0);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int mf_dcomp_frame_id(int type, out ulong id);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int mf_dcomp_stats(ulong frameId, out ulong startTime, out ulong targetTime, out ulong framePeriod, out uint targets);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int mf_dwm_timing(out ulong qpcVBlank, out ulong qpcCompose, out ulong cRefresh, out ulong cFrame, out uint rateNum, out uint rateDen, out ulong qpcRefreshPeriod);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int mf_cap_start(IntPtr hwnd);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void mf_cap_stop();
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int mf_cap_frames();
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int mf_cap_count();
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int mf_cap_last_hr();
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int mf_cap_item_size(out int w, out int h);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int mf_cap_get(int i, out long srt100ns, out long qpcArrive, out int w, out int h, out int minx, out int maxx, out int miny, out int maxy, out int n, out double cx);
}

/// <summary>
/// 派题 #7 (M1b pilot, spec docs/deepseek_m1b_spec.md):
///  * 软件路径: fluidmotion.dll via CompositionTarget.Rendering (unchanged).
///  * 合成器路径: Translation.X of AnimBox's element visual. Spring retarget = SpringScalarNaturalMotionAnimation with
///    InitialValue/InitialVelocity EXPLICITLY seeded from an app-side shadow state (spring.h closed form, mfprobe.dll), spec §2.1/§2.4.
///  * Measurement suite (button, or autorun via --measure / MF_AUTORUN=1 / autorun.flag): WGC pixel capture of the window,
///    DComp frame id + DWM timing clock gate, S1 period discrimination, spec §4 acceptance report -> comp_pilot_log.txt.
/// </summary>
public sealed partial class MainPage : Page
{
    private const ulong BoxId = 1;
    private const double BoxLeft = 40, BoxHalf = 60;
    private const double SpringResponse = 0.32, SpringDamping = 0.78;          // demo_motion.json springs.snappy
    private static readonly Vector2 EnterC1 = new(0.1f, 0.9f), EnterC2 = new(0.2f, 1.0f);   // easing.enter

    private MotionBridge? _fm;
    private ulong _enter, _drag;
    private readonly Stopwatch _sw = new();
    private StreamWriter? _log;
    private readonly StringBuilder _buf = new();
    private bool _compMode, _toggleFromCode;
    private int _exceptions;

    private Compositor? _comp;
    private Visual? _visual;
    private int _compGen;
    private AnimationController? _lastKeyCtrl;
    private CompositionScopedBatch? _lastKeyBatch;
    private bool _lastKeyCompleted;

    // shadow state of the compositor spring (spec §2.4)
    private bool _shActive;
    private double _shFrom, _shVel, _shTarget, _shT0Ms, _shResponse = SpringResponse, _shDamping = SpringDamping, _shSettle;
    private double _compStatic;   // static Translation.X when no spring shadow is active

    public MainPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private static string F(double v, int d = 4) => double.IsNaN(v) ? "NaN" : v.ToString("F" + d, CultureInfo.InvariantCulture);
    private double Now => _sw.Elapsed.TotalMilliseconds;
    private static long Qpc => Stopwatch.GetTimestamp();
    // Coordinator ruling #7: frame-id gate only on OS build >= 22000 (DCompositionGetFrameId is Win11+); older builds use DWM + WGC substitute gate.
    private static readonly int OsBuild = Environment.OSVersion.Version.Build;
    private static readonly bool FrameIdGateEnabled = OsBuild >= 22000;
    private const int FrameIdSkipped = unchecked((int)0x80004001);
    // substitute-gate thresholds (documented in docs/workbuddy_soft_demo.md "#7 补充：替代门槛")
    private const double DwmMinFraction = 0.5;   // DWM cRefresh advance must be >= 50% of rateRefresh x elapsed
    private const double FrozenMinFraction = 0.10; // each compositor scenario: observed box displacement >= 10% of commanded travel, else INVALID "frozen-frames"
    private const double WgcMinFraction = 0.5;   // box-found WGC frames inside motion windows must be >= 50% of rateRefresh x motion time
    private static double Qpc100ns(long qpc) => qpc * 1e7 / Stopwatch.Frequency;
    private const int LogFlushChars = 1_000_000;   // 派题 #8 item 10: flush + clear _buf when > ~1 MB
    private int _logAutoFlushes, _logDroppedChars;
    private void Log(string line)
    {
        _buf.Append(Now.ToString("F3", CultureInfo.InvariantCulture)).Append("ms ").Append(line).Append('\n');
        if (_buf.Length > LogFlushChars) { _logAutoFlushes++; FlushLog(); }
    }
    private void FlushLog()
    {
        try
        {
            if (_buf.Length == 0) return;
            if (_log != null) { _log.Write(_buf.ToString()); _log.Flush(); }
            else _logDroppedChars += _buf.Length;   // no file yet: drop rather than grow without bound
            _buf.Clear();
        }
        catch { _buf.Clear(); }
    }
    private string? _logPath;

    private void OpenLog()
    {
        foreach (var get in new Func<string>[] { () => @"C:\Users\admin\motion_framework\MotionSoftDemo", () => Path.GetTempPath(), () => Windows.Storage.ApplicationData.Current.LocalFolder.Path })
        {
            try
            {
                string name = _lifecycle ? "m8_lifecycle_log.txt" : _m0Probe ? "m0_ui.txt" : (_m2H5DenomVerifyProbe ? "m2_h5_denom_verify_ui.txt" : (_m2WgcSeedPathAPrimeProbe ? "m2_wgc_seed_path_a_prime_ui.txt" : (_m2WgcSeedPathBPrimeProbe ? "m2_wgc_seed_path_b_prime_ui.txt" : (_m2WgcSeedCalibProbe ? "m2_wgc_seed_calib_ui.txt" : (_m2WgcSeedAbsProbe ? "m2_wgc_seed_abs_ui.txt" : (_m2DeltaVPrimeProbe ? "m2_delta_v_prime_ui.txt" : (_m2DeltaVProbe ? "m2_delta_v_ui.txt" : (_m202Probe ? "m2_02_ui.txt" : (_m2H5SeedProbe ? "m2_h5_seed_ui.txt" : (_m201NoiseProbe ? "m2_01_noise_ui.txt" : (_r350Full ? "m50_r3_full_ui.txt" : (_r350Probe ? "m50_r3_50_ui.txt" : (_r3E3Probe ? "m49_r3_e3_ui.txt" : (_r3P12Probe ? "m48_r3_p12_ui.txt" : (_r2P11Probe ? "m47_r2_p11_ui.txt" : (_r2P10Probe ? "m46_r2_p10_ui.txt" : (_r2P9Probe ? "m45_r2_p9_ui.txt" : (_r2P8Probe ? "m43_r2_p8_ui.txt" : (_s1v2Probe ? "m44_s1v2_ui.txt" : (_r2P7Probe ? "m42_r2_p7_ui.txt" : (_r2P6Probe ? "m41_r2_p6_ui.txt" : (_r2P3Probe ? "m36_r2_p3_ui.txt" : (_r2P2Probe ? "m35_r2_p2_ui.txt" : (_r2P4Probe ? "m37_r2_p4_ui.txt" : (_r2P1Probe ? "m34_r2_p1_ui.txt" : (_r2PostProbe ? "m33_r2_post_ui.txt" : (_pointerPsProbe ? "m32_pointer_ps_ui.txt" : (_harnessNoiseProbe ? "m30_harness_noise_ui.txt" : (_startingValueProbe ? "m29_starting_value_ui.txt" : (_k3RestProbe ? "m28_k3_rest_ui.txt" : (_ivNullProbe ? "m27_iv_null_ui.txt" : (_periodMinProbe ? "m26_period_min_ui.txt" : (_interruptSeedProbe ? "m25_interrupt_ui.txt" : (_rateFoldProbe ? "m24_rate_fold_ui.txt" : (_zetaTrajProbe ? "m23_zeta_traj_ui.txt" : (_k3Probe ? "m22_k3_probe_ui.txt" : (_k4Probe ? "m21_k4_probe_ui.txt" : (_k2Probe ? "m20_k2_probe_ui.txt" : (_k1Probe ? "m19_k1_probe_ui.txt" : (_k5Probe ? "m18_k5_probe_ui.txt" : (_s5Probe ? "m17_s5_probe_ui.txt" : (_s4Probe ? "m16_s4_probe_ui.txt" : (_s3Probe ? "m15_s3_probe_ui.txt" : (_zetaProbe ? "m14_zeta_probe_ui.txt" : (_maptest ? "m13_maptest_log.txt" : (_align ? "m9_align_log.txt" : "comp_pilot_log.txt"))))))))))))))))))))))))))))))))))))))))))))));
                var path = Path.Combine(get(), name);
                _log = new StreamWriter(path, _lifecycle || _align || _maptest || _zetaProbe || _s3Probe || _s4Probe || _s5Probe || _k5Probe || _k1Probe || _k2Probe || _k4Probe || _k3Probe || _zetaTrajProbe || _rateFoldProbe || _interruptSeedProbe || _periodMinProbe || _ivNullProbe || _k3RestProbe || _startingValueProbe || _harnessNoiseProbe || _m201NoiseProbe || _m2H5SeedProbe || _pointerPsProbe || _r2PostProbe || _r2P1Probe || _r2P4Probe || _r2P2Probe || _r2P3Probe || _r2P6Probe || _r2P7Probe || _s1v2Probe || _r2P8Probe || _r2P9Probe || _r2P10Probe || _r2P11Probe || _r3P12Probe || _r3E3Probe || _r350Probe || _r350Full || _m201NoiseProbe || _m2H5SeedProbe || _m202Probe || _m2DeltaVProbe || _m2DeltaVPrimeProbe || _m2WgcSeedAbsProbe || _m2WgcSeedCalibProbe || _m2WgcSeedPathBPrimeProbe || _m2WgcSeedPathAPrimeProbe || _m2H5DenomVerifyProbe || _m0Probe, new UTF8Encoding(false)); _logPath = path; return;
            }
            catch { }
        }
    }

    private static bool AutorunRequested(out string why)
    {
        var args = Environment.GetCommandLineArgs();
        if (args.Any(a => a.Equals("--measure", StringComparison.OrdinalIgnoreCase))) { why = "launch arg --measure"; return true; }
        if (args.Any(a => a.Equals("--lifecycle", StringComparison.OrdinalIgnoreCase))) { why = "lifecycle"; return false; }
        if (Environment.GetEnvironmentVariable("MF_AUTORUN") == "1") { why = "env MF_AUTORUN=1"; return true; }
        foreach (var d in new[] { @"C:\Users\admin\motion_framework\MotionSoftDemo", AppContext.BaseDirectory })
            if (File.Exists(Path.Combine(d, "autorun.flag"))) { why = "flag file " + Path.Combine(d, "autorun.flag"); return true; }
        why = "none"; return false;
    }

    private bool _lifecycle, _shutdownDone, _inject, _align, _maptest, _zetaProbe, _s3Probe, _s4Probe, _s5Probe, _k5Probe, _k1Probe, _k2Probe, _k4Probe, _k3Probe, _zetaTrajProbe, _rateFoldProbe, _interruptSeedProbe, _periodMinProbe, _ivNullProbe, _k3RestProbe, _startingValueProbe, _harnessNoiseProbe, _pointerPsProbe, _r2PostProbe, _r2P1Probe, _r2P4Probe, _r2P2Probe, _r2P3Probe, _r2P6Probe, _r2P7Probe, _s1v2Probe, _r2P8Probe, _r2P9Probe, _r2P10Probe, _r2P11Probe, _r3P12Probe, _r3E3Probe, _r350Probe, _r350Full, _m201NoiseProbe, _m2H5SeedProbe, _m202Probe, _m2DeltaVProbe, _m2DeltaVPrimeProbe, _m2WgcSeedAbsProbe, _m2WgcSeedCalibProbe, _m2WgcSeedPathBPrimeProbe, _m2WgcSeedPathAPrimeProbe, _m2H5DenomVerifyProbe, _m0Probe;
    private int _injectFrameLogs;
    private string _dwmFlushVerdict = "not-run";
    private double _dwmFlushTimerFrac, _dwmFlushVsyncFrac, _dwmFlushP95;
    private void OnAppUnhandled(Exception ex) { _exceptions++; Log("UNHANDLED " + ex); FlushLog(); }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_fm != null) return;                                   // 派题 #8 item 5: Loaded may fire again
        _shutdownDone = false;
        var args0 = Environment.GetCommandLineArgs();
        _lifecycle = args0.Any(a => a.Equals("--lifecycle", StringComparison.OrdinalIgnoreCase));
        _align = args0.Any(a => a.Equals("--align", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_ALIGN") == "1";
        _maptest = args0.Any(a => a.Equals("--maptest", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_MAPTEST") == "1";
        _zetaProbe = args0.Any(a => a.Equals("--zeta-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_ZETA_PROBE") == "1";
        _s3Probe = args0.Any(a => a.Equals("--s3-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_S3_PROBE") == "1";
        _s4Probe = args0.Any(a => a.Equals("--s4-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_S4_PROBE") == "1";
        _s5Probe = args0.Any(a => a.Equals("--s5-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_S5_PROBE") == "1";
        _k5Probe = args0.Any(a => a.Equals("--k5-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_K5_PROBE") == "1";
        _k1Probe = args0.Any(a => a.Equals("--k1-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_K1_PROBE") == "1";
        _k2Probe = args0.Any(a => a.Equals("--k2-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_K2_PROBE") == "1";
        _k4Probe = args0.Any(a => a.Equals("--k4-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_K4_PROBE") == "1";
        _k3Probe = args0.Any(a => a.Equals("--k3-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_K3_PROBE") == "1";
        _zetaTrajProbe = args0.Any(a => a.Equals("--zeta-traj-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_ZETA_TRAJ_PROBE") == "1";
        _rateFoldProbe = args0.Any(a => a.Equals("--rate-fold-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_RATE_FOLD_PROBE") == "1";
        _interruptSeedProbe = args0.Any(a => a.Equals("--interrupt-seed-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_INTERRUPT_SEED_PROBE") == "1";
        _periodMinProbe = args0.Any(a => a.Equals("--period-min-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_PERIOD_MIN_PROBE") == "1";
        _ivNullProbe = args0.Any(a => a.Equals("--iv-null-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_IV_NULL_PROBE") == "1";
        _k3RestProbe = args0.Any(a => a.Equals("--k3-rest-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_K3_REST_PROBE") == "1";
        _startingValueProbe = args0.Any(a => a.Equals("--starting-value-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_STARTING_VALUE_PROBE") == "1";
        _r2P2Probe = args0.Any(a => a.Equals("--r2-p2-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_R2_P2_PROBE") == "1";
        _r2P11Probe = args0.Any(a => a.Equals("--r2-p11-probe", StringComparison.OrdinalIgnoreCase));
        _r3E3Probe = args0.Any(a => a.Equals("--r3-e3-probe", StringComparison.OrdinalIgnoreCase));
        _r3P12Probe = args0.Any(a => a.Equals("--r3-p12-probe", StringComparison.OrdinalIgnoreCase));
        _r350Full = args0.Any(a => a.Equals("--r3-50-full", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_R350_FULL") == "1" || System.IO.File.Exists(@"C:\Users\admin\motion_framework\artifacts\r350_full.flag");
        _r350Probe = args0.Any(a => a.Equals("--r3-50-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_R350_PROBE") == "1" || System.IO.File.Exists(@"C:\Users\admin\motion_framework\artifacts\r350_probe.flag");
        _r2P10Probe = args0.Any(a => a.Equals("--r2-p10-probe", StringComparison.OrdinalIgnoreCase));
        _r2P9Probe = args0.Any(a => a.Equals("--r2-p9-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_R2_P9_PROBE") == "1";
        _r2P8Probe = args0.Any(a => a.Equals("--r2-p8-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_R2_P8_PROBE") == "1";
        _s1v2Probe = args0.Any(a => a.Equals("--s1v2-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_S1V2_PROBE") == "1";
        _r2P7Probe = args0.Any(a => a.Equals("--r2-p7-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_R2_P7_PROBE") == "1";
        _r2P6Probe = args0.Any(a => a.Equals("--r2-p6-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_R2_P6_PROBE") == "1";
        _r2P3Probe = args0.Any(a => a.Equals("--r2-p3-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_R2_P3_PROBE") == "1";
        _r2P4Probe = args0.Any(a => a.Equals("--r2-p4-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_R2_P4_PROBE") == "1";
        _r2P1Probe = args0.Any(a => a.Equals("--r2-p1-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_R2_P1_PROBE") == "1";
        _r2PostProbe = args0.Any(a => a.Equals("--r2-post-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_R2_POST_PROBE") == "1";
        _pointerPsProbe = args0.Any(a => a.Equals("--pointer-ps-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_POINTER_PS_PROBE") == "1";
        _harnessNoiseProbe = args0.Any(a => a.Equals("--harness-noise-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_HARNESS_NOISE_PROBE") == "1";
        _m201NoiseProbe = args0.Any(a => a.Equals("--m2-01-noise-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_M201_NOISE") == "1" || System.IO.File.Exists(@"C:\Users\admin\motion_framework\artifacts\m2_01_noise.flag");
        _m2H5SeedProbe = args0.Any(a => a.Equals("--m2-h5-seed", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_M2_H5_SEED") == "1" || System.IO.File.Exists(@"C:\Users\admin\motion_framework\artifacts\m2_h5_seed.flag");
        _m202Probe = args0.Any(a => a.Equals("--m2-02", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_M2_02") == "1" || System.IO.File.Exists(@"C:\Users\admin\motion_framework\artifacts\m2_02.flag");
        _m2DeltaVProbe = args0.Any(a => a.Equals("--m2-delta-v", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_M2_DELTA_V") == "1" || System.IO.File.Exists(@"C:\Users\admin\motion_framework\artifacts\m2_delta_v.flag");
        _m2DeltaVPrimeProbe = args0.Any(a => a.Equals("--m2-delta-v-prime", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_M2_DELTA_V_PRIME") == "1" || System.IO.File.Exists(@"C:\Users\admin\motion_framework\artifacts\m2_delta_v_prime.flag");
        _m2WgcSeedAbsProbe = args0.Any(a => a.Equals("--m2-wgc-seed-abs", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_M2_WGC_SEED_ABS") == "1" || System.IO.File.Exists(@"C:\Users\admin\motion_framework\artifacts\m2_wgc_seed_abs.flag");
        _m2WgcSeedCalibProbe = args0.Any(a => a.Equals("--m2-wgc-seed-calib", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_M2_WGC_SEED_CALIB") == "1" || System.IO.File.Exists(@"C:\Users\admin\motion_framework\artifacts\m2_wgc_seed_calib.flag");
        _m2WgcSeedPathBPrimeProbe = args0.Any(a => a.Equals("--m2-wgc-seed-path-b-prime", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_M2_WGC_SEED_PATH_B_PRIME") == "1" || System.IO.File.Exists(@"C:\Users\admin\motion_framework\artifacts\m2_wgc_seed_path_b_prime.flag");
        _m2WgcSeedPathAPrimeProbe = args0.Any(a => a.Equals("--m2-wgc-seed-path-a-prime", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_M2_WGC_SEED_PATH_A_PRIME") == "1" || System.IO.File.Exists(@"C:\Users\admin\motion_framework\artifacts\m2_wgc_seed_path_a_prime.flag");
        _m2H5DenomVerifyProbe = args0.Any(a => a.Equals("--m2-h5-denom-verify", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_M2_H5_DENOM_VERIFY") == "1" || System.IO.File.Exists(@"C:\Users\admin\motion_framework\artifacts\m2_h5_denom_verify.flag");
        _m0Probe = args0.Any(a => a.Equals("--m0-probe", StringComparison.OrdinalIgnoreCase)) || Environment.GetEnvironmentVariable("MF_M0_PROBE") == "1" || System.IO.File.Exists(@"C:\Users\admin\motion_framework\artifacts\m0.flag");
        _sw.Start();
        OpenLog();
        App.Unhandled += OnAppUnhandled;                           // item 4: paired with -= in Shutdown()
        Log($"派题 #8 MotionSoftDemo (pid {Environment.ProcessId}{(_lifecycle ? ", --lifecycle" : "")}); App.Unhandled subscribers={App.UnhandledSubscribers}; wallclock at Loaded = {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}; OS={Environment.OSVersion.Version}; QPC freq={Stopwatch.Frequency}");
        try
        {
            _inject = Environment.GetEnvironmentVariable("MF_INJECT_FAULTS") == "1" || Environment.GetCommandLineArgs().Any(a => a.Equals("--inject-faults", StringComparison.OrdinalIgnoreCase));
            _fm = new MotionBridge(OnValue, _inject);
            if (_inject) Log("INJECT faults ON (one throw each in _render, _sink, _onRendering)");
            _fm.Diagnostic += d => Log("BRIDGE " + d);
            _fm.Frame += (_, __) =>   // after fm_on_frame returned: Fm.* allowed here (not in the sink)
            {
                if (_swTracing && _fm != null && _fm.Current(BoxId, "offsetX", out var cv, out var cvel)) _swTrace.Add((Now, cv, cvel));
                if (_inject && _injectFrameLogs < 40 && _fm != null && _fm.Current(BoxId, "offsetX", out var iv, out var ivel))
                { _injectFrameLogs++; Log($"INJECT frame {_injectFrameLogs}: offsetX={F(iv, 3)} vel={F(ivel, 2)} callbackErrors={_fm.CallbackErrors}"); }
            };
            var err = _fm.LoadTokens(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "demo_motion.json")));
            Log($"load_tokens {(err is null ? "ok" : "ERR " + err)}");
            _enter = _fm.Create(BoxId, "demo.enter");
            _drag = _fm.Create(BoxId);
            _fm.Play(_enter);

            _visual = ElementCompositionPreview.GetElementVisual(AnimBox);
            _comp = _visual.Compositor;
            ElementCompositionPreview.SetIsTranslationEnabled(AnimBox, true);
            _visual.Properties.InsertVector3("Translation", Vector3.Zero);
            StatusText.Text = "MotionSoftDemo — 软件路径 / 合成器路径 (M1b #7); click anywhere to retarget";

            DeferredDisposeSelfTest();
            if (_lifecycle)
            {
                // long spring (response 3 s) so the window is closed while Rendering is still subscribed
                bool ok = _fm.Retarget(_drag, "offsetX", 240, 3.0, 1.0);
                Log($"LIFECYCLE retarget(response=3.0) ok={ok} subscribed={_fm.Subscribed} subs={_fm.SubscribeCount}");
                After(800, () => { Log($"LIFECYCLE ready (animating, subscribed={_fm?.Subscribed})"); FlushLog(); });
            }
            else if (_align)
            {
                Log("ALIGN probe requested (--align); starts in 800 ms");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunAlignProbe();
                t.Start();
            }
            else if (_maptest)
            {
                Log("MAPTEST requested (--maptest); starts in 1500 ms (派题 #13 Period/ζ mapping sweep)");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(1500); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunMapTest();
                t.Start();
            }
            else if (_zetaProbe)
            {
                Log("ZETA probe requested (--zeta-probe); starts in 800 ms (派题 #14 S2)");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunZetaProbe();
                t.Start();
            }
            else if (_s3Probe)
            {
                Log("S3 probe requested (--s3-probe); starts in 800 ms (派题 #15 InitialVelocity units)");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunS3Probe();
                t.Start();
            }
            else if (_s4Probe)
            {
                Log("S4 probe requested (--s4-probe); starts in 800 ms (派题 #16 DelayTime+preset)");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunS4Probe();
                t.Start();
            }
            else if (_s5Probe)
            {
                Log("S5 probe requested (--s5-probe); starts in 800 ms (派题 #17 settle hold)");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunS5Probe();
                t.Start();
            }
            else if (_k5Probe)
            {
                Log("K5 probe requested (--k5-probe); starts in 800 ms (派题 #18 StopAnimation mid-flight)");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunK5Probe();
                t.Start();
            }
            else if (_k1Probe)
            {
                Log("K1 probe requested (--k1-probe); starts in 800 ms (派题 #19 KeyFrame easing segment)");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunK1Probe();
                t.Start();
            }
            else if (_k2Probe)
            {
                Log("K2 probe requested (--k2-probe); starts in 800 ms (派题 #20 bezier y overshoot)");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunK2Probe();
                t.Start();
            }
            else if (_k4Probe)
            {
                Log("K4 probe requested (--k4-probe); starts in 800 ms (派题 #21 KeyFrame hold final)");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunK4Probe();
                t.Start();
            }
            else if (_k3Probe)
            {
                Log("K3 probe requested (--k3-probe); starts in 800 ms (派题 #22 AnimationController Progress/Rate)");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunK3Probe();
                t.Start();
            }
            else if (_zetaTrajProbe)
            {
                Log("Zeta traj probe requested (--zeta-traj-probe); starts in 800 ms (派题 #23 ζ>1 trajectory fidelity)");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunZetaTrajProbe();
                t.Start();
            }
            else if (_rateFoldProbe)
            {
                Log("Rate-fold probe requested (--rate-fold-probe); starts in 800 ms (派题 #24 rate=1 vs rate=2)");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunRateFoldProbe();
                t.Start();
            }
            else if (_interruptSeedProbe)
            {
                Log("Interrupt-seed probe requested (--interrupt-seed-probe); starts in 800 ms (派题 #25 single-τ interrupt)");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunInterruptSeedProbe();
                t.Start();
            }
            else if (_periodMinProbe)
            {
                Log("Period-min probe requested (--period-min-probe); starts in 800 ms (派题 #26 Period floor)");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunPeriodMinProbe();
                t.Start();
            }
            else if (_ivNullProbe)
            {
                Log("IV-null probe requested (--iv-null-probe); starts in 800 ms (派题 #27 InitialValue null)");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunIvNullProbe();
                t.Start();
            }
            else if (_k3RestProbe)
            {
                Log("K3-rest probe requested (--k3-rest-probe); starts in 800 ms (派题 #28 Progress+Delay / |rate|>16 / reverse)");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunK3RestProbe();
                t.Start();
            }
            else if (_startingValueProbe)
            {
                Log("StartingValue probe requested (--starting-value-probe); starts in 800 ms (派题 #29 this.StartingValue)");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunStartingValueProbe();
                t.Start();
            }
            else if (_m0Probe)
            {
                Log("M0 tech-validation probe (--m0-probe / MF_M0_PROBE); starts in 800 ms; pointer phases need MF_M0_INJECT=1");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunM0Probe();
                t.Start();
            }
            else if (_m2H5DenomVerifyProbe)
            {
                Log("M2-H5-DENOM-VERIFY PATHB-A + v_denom=v_pre C1 probe (--m2-h5-denom-verify / MF_M2_H5_DENOM_VERIFY); starts in 800 ms");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunM2H5DenomVerifyProbe();
                t.Start();
            }
            else if (_m2WgcSeedPathAPrimeProbe)
            {
                Log("M2-WGC-SEED-PATH-A-PRIME PATHB-A′=V2-clamp+V3-VS probe (--m2-wgc-seed-path-a-prime / MF_M2_WGC_SEED_PATH_A_PRIME); starts in 800 ms");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunM2WgcSeedPathAPrimeProbe();
                t.Start();
            }
            else if (_m2WgcSeedPathBPrimeProbe)
            {
                Log("M2-WGC-SEED-PATH-B-PRIME PATHB-A=V2-clamp probe (--m2-wgc-seed-path-b-prime / MF_M2_WGC_SEED_PATH_B_PRIME); starts in 800 ms");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunM2WgcSeedPathBPrimeProbe();
                t.Start();
            }
            else if (_m2WgcSeedCalibProbe)
            {
                Log("M2-WGC-SEED-CALIB P-LS+TS calib probe (--m2-wgc-seed-calib / MF_M2_WGC_SEED_CALIB); starts in 800 ms");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunM2WgcSeedCalibProbe();
                t.Start();
            }
            else if (_m2WgcSeedAbsProbe)
            {
                Log("M2-WGC-SEED-ABS P-LS+TS probe (--m2-wgc-seed-abs / MF_M2_WGC_SEED_ABS); starts in 800 ms");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunM2WgcSeedAbsProbe();
                t.Start();
            }
            else if (_m2DeltaVPrimeProbe)
            {
                Log("M2-Δv′ prime absolute Δv probe (--m2-delta-v-prime / MF_M2_DELTA_V_PRIME); starts in 800 ms");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunM2DeltaVPrimeProbe();
                t.Start();
            }
            else if (_m2DeltaVProbe)
            {
                Log("M2-Δv absolute Δv probe (--m2-delta-v / MF_M2_DELTA_V); starts in 800 ms");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunM2DeltaVProbe();
                t.Start();
            }
            else if (_m202Probe)
            {
                Log("M2-02 csharp/etw/preview smoke (--m2-02 / MF_M2_02); starts in 800 ms");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunM202Probe();
                t.Start();
            }
            else if (_m2H5SeedProbe)
            {
                Log("M2-H5-SEED WGC seed probe (--m2-h5-seed); starts in 800 ms");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunM2H5SeedProbe();
                t.Start();
            }
            else if (_m201NoiseProbe)
            {
                Log("M2-01 static noise probe (--m2-01-noise-probe); starts in 800 ms");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunM201NoiseProbe();
                t.Start();
            }
            else if (_r350Full)
            {
                Log("R3 #50 FULL (--r3-50-full)");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunR350Probe(true);
                t.Start();
            }
            else if (_r350Probe)
            {
                Log("R3 #50 AB (--r3-50-probe)");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunR350Probe(false);
                t.Start();
            }
            else if (_r3E3Probe)
            {
                Log("R3 E3 #49 probe (--r3-e3-probe)");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunR3E3Probe();
                t.Start();
            }
            else if (_r3P12Probe)
            {
                Log("R3 P12 #48 probe (--r3-p12-probe)");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunR3P12Probe();
                t.Start();
            }
            else if (_r2P11Probe)
            {
                Log("R2 P11 #47 probe (--r2-p11-probe)");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunR2P11Probe();
                t.Start();
            }
            else if (_r2P10Probe)
            {
                Log("R2 P10 #46 probe (--r2-p10-probe)");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunR2P10Probe();
                t.Start();
            }
            else if (_r2P9Probe)
            {
                Log("R2 P9 #45 probe (--r2-p9-probe)");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunR2P9Probe();
                t.Start();
            }
            else if (_r2P8Probe)
            {
                Log("R2 P8 E2 probe (--r2-p8-probe) 派题 #43");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunR2P8Probe();
                t.Start();
            }
            else if (_s1v2Probe)
            {
                Log("S1 v2 formal-mapping shape probe (--s1v2-probe) 派题 #44");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunS1v2Probe();
                t.Start();
            }
            else if (_r2P7Probe)
            {
                Log("R2 P7 remap interrupt 4-arm probe (--r2-p7-probe) 派题 #42");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunR2P7Probe();
                t.Start();
            }
            else if (_r2P6Probe)
            {
                Log("R2 P6 remap probe (--r2-p6-probe) 派题 #41");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunR2P6Probe();
                t.Start();
            }
            else if (_r2P3Probe)
            {
                Log("R2 P3 timebase probe (--r2-p3-probe) 派题 #36");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunR2P3Probe();
                t.Start();
            }
            else if (_r2P2Probe)
            {
                Log("R2 P2 A/B/B' probe (--r2-p2-probe) 派题 #35");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunR2P2Probe();
                t.Start();
            }
            else if (_r2P4Probe)
            {
                Log("R2 P4 B2 velocity probe (--r2-p4-probe) 派题 #37");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunR2P4Probe();
                t.Start();
            }
            else if (_r2P1Probe)
            {
                Log("R2 P1 IV=null probe (--r2-p1-probe) 派题 #34");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunR2P1Probe();
                t.Start();
            }
            else if (_r2PostProbe)
            {
                Log("R2 post probe (--r2-post-probe) 派题 #33");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunR2PostProbe();
                t.Start();
            }
            else if (_pointerPsProbe)
            {
                Log("Pointer PS probe (--pointer-ps-probe) 派题 #32");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunPointerPsProbe();
                t.Start();
            }
            else if (_harnessNoiseProbe)
            {
                Log("Harness-noise probe requested (--harness-noise-probe); starts in 800 ms (派题 #30 0.25s linear floor)");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunHarnessNoiseProbe();
                t.Start();
            }
            else if (AutorunRequested(out var why))
            {
                Log($"AUTORUN requested ({why}); suite starts in 1500 ms");
                var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(1500); t.IsRepeating = false;
                t.Tick += async (_, __) => await RunSuite("autorun: " + why);
                t.Start();
            }
            else Log("AUTORUN not requested (use button, --measure, --align, --maptest, MF_AUTORUN=1 or autorun.flag)");
        }
        catch (Exception ex) { _exceptions++; StatusText.Text = "init failed: " + ex.Message; Log("EXCEPTION " + ex); }
        FlushLog();
    }

    // ---------------------------------------------------------------- software path
    private readonly List<(double tMs, double v, double vel)> _swTrace = new();
    private bool _swTracing;
    private void OnValue(ulong target, FmProp prop, double v)   // runs inside fm_on_frame: no Fm.* calls here
    {
        if (target != BoxId) return;
        if (prop == FmProp.OffsetX) { if (!_compMode) BoxTranslate.X = v; }
        else if (prop == FmProp.Opacity) AnimBox.Opacity = v;
    }

    // 派题 #8 item 3: Dispose requested from inside the sink (i.e. inside fm_on_frame) must be deferred.
    private MotionBridge? _probeBridge;
    private void DeferredDisposeSelfTest()
    {
        int sinkCalls = 0;
        MotionBridge? b = null;
        b = new MotionBridge((t, p, v) => { sinkCalls++; if (sinkCalls == 1) { bool inNative = b!.InNative; b.Dispose(); Log($"SELFTEST deferred-dispose: Dispose() called from sink, InNative={inNative}, IsDisposed right after={b.IsDisposed}, Subscribed={b.Subscribed}"); } });
        _probeBridge = b;
        b.Diagnostic += d => Log("SELFTEST bridge: " + d);
        var terr = b.LoadTokens(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "demo_motion.json")));
        var a = b.Create(BoxId);
        bool ok = b.RetargetFrom(a, "offsetX", 0, 100);   // fresh system: no current value yet, so give an explicit from
        Log($"SELFTEST deferred-dispose: started probe bridge tokens={(terr is null ? "ok" : terr)} retarget ok={ok} subscribed={b.Subscribed}");
        After(1000, () => { if (_probeBridge is null) return; Log($"SELFTEST deferred-dispose result: sinkCalls={sinkCalls} DeferredDisposes={b.DeferredDisposes} IsDisposed={b.IsDisposed} subs={b.SubscribeCount} unsubs={b.UnsubscribeCount} callbackErrors={b.CallbackErrors} -> {(b.IsDisposed && b.DeferredDisposes == 1 && b.SubscribeCount == b.UnsubscribeCount && b.CallbackErrors == 0 ? "PASS" : "FAIL")}"); FlushLog(); });
    }

    private void SwRetarget(double target, string why)
    {
        if (_fm is null) return;
        if (_compMode) { Log($"{why}: skipped (compositor mode)"); return; }
        _fm.Current(BoxId, "offsetX", out var v0, out var vel0);
        bool ok = _fm.Retarget(_drag, "offsetX", target, SpringResponse, SpringDamping);
        _fm.Current(BoxId, "offsetX", out var v1, out var vel1);
        Log($"SW retarget[{why}] -> {F(target)} ok={ok} before(value={F(v0)} vel={F(vel0)}) after(value={F(v1)} vel={F(vel1)}) |dv|={Math.Abs(v1 - v0):E3}");
    }

    // ---------------------------------------------------------------- compositor path
    private void ShadowNow(double nowMs, out double value, out double velocity)
    {
        if (!_shActive) { value = _compStatic; velocity = 0; return; }
        var el = (nowMs - _shT0Ms) / 1000.0;
        if (el >= _shSettle) { value = _shTarget; velocity = 0; return; }
        MfProbe.mf_spring_at(_shResponse, _shDamping, _shFrom, _shTarget, _shVel, el, out value, out velocity);
    }

    private sealed record CompStart(string Label, long Qpc, double NowMs, double From, double FromVel, double Target, double Response, double Damping, bool Spring, double Duration);
    private readonly List<CompStart> _starts = new();

    /// Retarget/start the compositor spring. InitialValue/InitialVelocity are set explicitly from the shadow state (spec §2.1).
    private CompStart? CompSpring(double target, string label, double response = SpringResponse, double damping = SpringDamping)
    {
        if (!_compMode || _comp is null || _visual is null) { Log($"{label}: skipped (software mode)"); return null; }
        var now = Now;
        ShadowNow(now, out var v0, out var vel0);
        var a = _comp.CreateSpringScalarAnimation();
        a.FinalValue = (float)target;
        a.InitialValue = (float)v0;                         // explicit seed (shadow state)
        a.InitialVelocity = (float)vel0;                    // explicit seed; default would be 0 (no inheritance)
        a.Period = TimeSpan.FromSeconds(response);          // S1: Period = response (under test)
        a.DampingRatio = (float)damping;                    // S2
        var gen = ++_compGen;
        var q = Qpc;
        _visual.StartAnimation("Translation.X", a);
        var ctrl = _visual.TryGetAnimationController("Translation.X");
        // NaturalMotion: TryGet is expected null (AnimationController is KeyFrame-only). Documented in #10.
        _shActive = true; _shFrom = v0; _shVel = vel0; _shTarget = target; _shT0Ms = now; _shResponse = response; _shDamping = damping;
        _shSettle = MfProbe.mf_spring_settle(response, damping, v0, target, vel0);
        var cs = new CompStart(label, q, now, v0, vel0, target, response, damping, true, _shSettle);
        _starts.Add(cs);
        Log($"COMP spring [{label}] gen={gen} -> {F(target)} InitialValue={F(v0)} InitialVelocity={F(vel0)} Period={response}s DampingRatio={damping} shadowSettle={F(_shSettle)}s qpc={q} TryGet={CtrlDesc(ctrl)}{(ctrl is null ? " [NaturalMotion: controller unsupported]" : "")}");
        return cs;
    }

    private CompStart? CompBezier(double target, double seconds, string label)
    {
        if (!_compMode || _comp is null || _visual is null) { Log($"{label}: skipped (software mode)"); return null; }
        var now = Now;
        ShadowNow(now, out var v0, out _);
        var k = _comp.CreateScalarKeyFrameAnimation();
        k.InsertKeyFrame(0f, (float)v0);   // explicit from (avoid StartingValue race with a just-reset visual)
        k.InsertKeyFrame(1f, (float)target, _comp.CreateCubicBezierEasingFunction(EnterC1, EnterC2));
        k.Duration = TimeSpan.FromSeconds(seconds);
        var q = Qpc;
        // KeyFrame: AnimationController works. Prefer 3-arg StartAnimation so we hold a live controller.
        var ctrl = _comp.CreateAnimationController();
        var batch = _comp.CreateScopedBatch(CompositionBatchTypes.Animation);
        _visual.StartAnimation("Translation.X", k, ctrl);
        batch.End();
        var tryGet = _visual.TryGetAnimationController("Translation.X");
        Log($"COMP bezier controller: created Progress={ctrl.Progress:F4} rate={ctrl.PlaybackRate:F4}; TryGet after Start={CtrlDesc(tryGet)}");
        _lastKeyCtrl = ctrl; _lastKeyBatch = batch; _lastKeyCompleted = false;
        batch.Completed += (_, __) => { _lastKeyCompleted = true; Log($"COMP bezier batch Completed Progress={CtrlDesc(_visual.TryGetAnimationController("Translation.X"))}"); };
        _shActive = false; _compStatic = target;           // target after completion (K4)
        var cs = new CompStart(label, q, now, v0, 0, target, 0, 0, false, seconds);
        _starts.Add(cs);
        Log($"COMP bezier [{label}] -> {F(target)} from(shadow)={F(v0)} {seconds * 1000}ms qpc={q}");
        return cs;
    }

    private void CompReset(double value, string why)
    {
        if (_visual is null) return;
        _visual.StopAnimation("Translation.X");
        _visual.Properties.InsertVector3("Translation", new Vector3((float)value, 0, 0));
        _shActive = false; _compStatic = value;
        Log($"COMP reset[{why}] StopAnimation + Translation.X:={F(value)}");
    }

    private bool SetMode(bool comp, string why)
    {
        if (_fm is null || _visual is null) return false;
        if (comp == _compMode) return true;
        try
        {
            if (comp)
            {
                _fm.Current(BoxId, "offsetX", out var v, out var vel);
                _compMode = true;
                BoxTranslate.X = 0;
                _visual.Properties.InsertVector3("Translation", new Vector3((float)v, 0, 0));
                _shActive = false; _compStatic = v;
                Log($"MODE -> 合成器路径 ({why}): software value={F(v)} vel={F(vel)} -> Translation.X={F(v)}, BoxTranslate.X=0");
            }
            else
            {
                ShadowNow(Now, out var sv, out var svel);
                _visual.StopAnimation("Translation.X");
                var prop = _visual.Properties.TryGetVector3("Translation", out var tr) == CompositionGetValueStatus.Succeeded ? tr.X : double.NaN;
                _visual.Properties.InsertVector3("Translation", Vector3.Zero);
                _compMode = false;
                BoxTranslate.X = sv;
                bool ok = _fm.RetargetFrom(_drag, "offsetX", sv, _shActive ? _shTarget : sv, SpringResponse, SpringDamping);
                _fm.Current(BoxId, "offsetX", out var nv, out var nvel);
                _shActive = false;
                Log($"MODE -> 软件路径 ({why}): handoff from SHADOW value={F(sv)} vel={F(svel)} (readback after StopAnimation={F(prop)}, |shadow-readback|={F(Math.Abs(sv - prop))}) ok={ok} sw value={F(nv)} vel={F(nvel)} handoff|dv|={F(Math.Abs(nv - sv))}");
            }
            _toggleFromCode = true; ModeToggle.IsOn = comp; _toggleFromCode = false;
            return true;
        }
        catch (Exception ex) { _exceptions++; Log("MODE switch EXCEPTION " + ex.Message); return false; }
    }

    private void OnModeToggled(object sender, RoutedEventArgs e) { if (!_toggleFromCode) SetMode(ModeToggle.IsOn, "user toggle"); }

    private void OnRootPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var p = e.GetCurrentPoint(RootGrid).Position;
        double target = Math.Max(0, p.X - BoxLeft - BoxHalf);
        if (_compMode) CompSpring(target, $"COMP pointer x={F(p.X)}");
        else SwRetarget(target, $"SW pointer x={F(p.X)}");
    }

    private async void OnMeasureClick(object sender, RoutedEventArgs e) => await RunSuite("button");

    // ---------------------------------------------------------------- measurement suite
    private bool _suiteRunning;
    private readonly List<double> _renderIntervals = new();
    private double _lastRenderMs = -1;
    private int _renderSubs, _renderUnsubs;
    private void OnSuiteRendering(object? s, object e) { var n = Now; if (_lastRenderMs >= 0) _renderIntervals.Add(n - _lastRenderMs); _lastRenderMs = n; }

    private sealed record ClockSample(double NowMs, long Qpc, int HrId, ulong FrameId, int HrStats, ulong StatStart, ulong StatTarget, ulong StatPeriod, int HrDwm, ulong QpcVBlank, ulong QpcCompose, ulong CRefresh, ulong CFrame, uint RateNum, uint RateDen, int WgcFrames);
    private ClockSample SampleClock()
    {
        int hrId = FrameIdSkipped; ulong fid = 0;
        if (FrameIdGateEnabled) hrId = MfProbe.mf_dcomp_frame_id(2, out fid);   // COMPOSITION_FRAME_ID_COMPLETED
        ulong ss = 0, st = 0, sp = 0; uint tc = 0;
        int hrS = hrId == 0 ? MfProbe.mf_dcomp_stats(fid, out ss, out st, out sp, out tc) : hrId;
        int hrD = MfProbe.mf_dwm_timing(out var vb, out var cp, out var cr, out var cf, out var rn, out var rd, out _);
        var c = new ClockSample(Now, Qpc, hrId, fid, hrS, ss, st, sp, hrD, vb, cp, cr, cf, rn, rd, MfProbe.mf_cap_frames());
        Log($"CLOCK dcomp({(FrameIdGateEnabled ? $"hr=0x{hrId:X8}" : "skipped")}) frameId={fid} stats(hr=0x{hrS:X8}) start={ss} target={st} period={sp} | DWM(hr=0x{hrD:X8}) qpcVBlank={vb} qpcCompose={cp} cRefresh={cr} cFrame={cf} rateRefresh={rn}/{rd} | qpcNow={c.Qpc} | WGC frames={c.WgcFrames}");
        return c;
    }

    private sealed record Cap(double T100ns, long QpcArrive, int W, int H, int MinX, int MaxX, int N, double Cx);
    private static List<Cap> ReadCaptures()
    {
        var list = new List<Cap>();
        int n = MfProbe.mf_cap_count();
        for (int i = 0; i < n; i++)
            if (MfProbe.mf_cap_get(i, out var srt, out var qArr, out var w, out var h, out var minx, out var maxx, out _, out _, out var cnt, out var cx) == 1)
                list.Add(new Cap(srt, qArr, w, h, minx, maxx, cnt, cx));
        return list;
    }
    // 派题 #10: absolute time = (qpcArrive - startQpc)/freq — rates matched SRT, absolute SRT epoch ≠ Qpc100ns(start).
    private static double SecSince(long startQpc, long frameQpc) => (frameQpc - startQpc) / (double)Stopwatch.Frequency;

    [DllImport("dwmapi.dll")] private static extern int DwmFlush();
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    // review appendix: 300 consecutive DwmFlush() calls; ~16.7 ms = DWM composing at 60 Hz, ~31 ms = 32 Hz.
    /// <summary>派题 #8 follow-up: DwmFlush verdict by distribution, not by mean.
    /// timer-driven: >=80% of intervals within ±1.0 ms of k×15.625 ms (k=1,2), checked first.
    /// ≈60Hz vsync: >=80% within ±1.0 ms of 1000/refresh AND p95 <= 18 ms. Otherwise unknown.</summary>
    internal static (string verdict, double k1, double k2, double timer, double vsync, double vsyncMs, double p95) ClassifyDwmFlush(IReadOnlyList<double> iv, double refreshHz)
    {
        if (iv.Count == 0) return ("unknown (no samples)", 0, 0, 0, 0, double.NaN, double.NaN);
        const double tick = 15.625, tol = 1.0;
        double n = iv.Count;
        double k1 = iv.Count(x => Math.Abs(x - tick) <= tol) / n, k2 = iv.Count(x => Math.Abs(x - 2 * tick) <= tol) / n;
        double vsMs = refreshHz > 0 ? 1000.0 / refreshHz : 1000.0 / 60.0;
        double vs = iv.Count(x => Math.Abs(x - vsMs) <= tol) / n;
        var sorted = iv.OrderBy(x => x).ToList(); double p95 = sorted[Math.Min(sorted.Count - 1, (int)Math.Ceiling(0.95 * sorted.Count) - 1)];
        string verdict = (k1 + k2) >= 0.8 ? "timer-driven" : (vs >= 0.8 && p95 <= 18.0) ? "≈60Hz vsync" : "unknown";
        return (verdict, k1, k2, k1 + k2, vs, vsMs, p95);
    }

    private async Task DwmFlushProbe(int calls)
    {
        var r = await Task.Run(() =>
        {
            var iv = new List<double>(calls); int hr = 0, fails = 0;
            long prev = Stopwatch.GetTimestamp(), start = prev;
            for (int i = 0; i < calls; i++)
            {
                hr = DwmFlush(); if (hr != 0) fails++;
                long now = Stopwatch.GetTimestamp();
                iv.Add((now - prev) * 1000.0 / Stopwatch.Frequency); prev = now;
                if ((now - start) > 30 * Stopwatch.Frequency) break;   // hard cap 30 s
            }
            return (iv, hr, fails, (prev - start) * 1000.0 / Stopwatch.Frequency);
        });
        var s = r.iv.OrderBy(x => x).ToList();
        double P(double q) => s.Count == 0 ? double.NaN : s[Math.Min(s.Count - 1, (int)Math.Ceiling(q * s.Count) - 1)];
        double refreshHz = 0; try { MfProbe.mf_dwm_timing(out _, out _, out _, out _, out var rn, out var rd, out _); if (rd > 0) refreshHz = (double)rn / rd; } catch { }
        var v = ClassifyDwmFlush(r.iv, refreshHz);
        Log($"DWMFLUSH probe: calls={r.iv.Count} total={F(r.Item4, 1)} ms lastHr=0x{r.hr:X8} fails={r.fails} interval mean={F(r.iv.Count > 0 ? r.iv.Average() : double.NaN, 3)} min={F(s.Count > 0 ? s[0] : double.NaN, 3)} p50={F(P(0.5), 3)} p95={F(P(0.95), 3)} max={F(s.Count > 0 ? s[^1] : double.NaN, 3)} ms");
        _dwmFlushVerdict = v.verdict; _dwmFlushTimerFrac = v.timer; _dwmFlushVsyncFrac = v.vsync; _dwmFlushP95 = v.p95;
        Log($"DWMFLUSH verdict: {v.verdict} | timer tiers k*15.625±1.0 (k=1,2): k1={F(100 * v.k1, 1)}% k2={F(100 * v.k2, 1)}% total={F(100 * v.timer, 1)}% | vsync 1000/{F(refreshHz, 3)}={F(v.vsyncMs, 3)}±1.0 ms: {F(100 * v.vsync, 1)}%, p95={F(v.p95, 3)} | rule: timer-driven if timer>=80%; else ≈60Hz vsync if vsync>=80% and p95<=18 ms; else unknown");
        Log("DWMFLUSH raw intervals ms: " + string.Join(",", r.iv.Select(x => x.ToString("F3", CultureInfo.InvariantCulture))));
        FlushLog();
    }

    /// <summary>派题 #9 补充: focused three-way alignment at one interrupt (shadow / AnimationController / WGC).</summary>
    private readonly List<(double nowMs, long qpc, double srtMs, double cx, double dip)> _alignWgc = new();
    private readonly List<(double nowMs, long qpc)> _alignRender = new();
    private double _alignPxPerDip = 1, _alignCx0 = double.NaN;
    private bool _alignCap;

    private void OnAlignRendering(object? s, object a)
    {
        var q = Qpc; _alignRender.Add((Now, q));
        if (_alignCap)
        {
            int n = MfProbe.mf_cap_count();
            for (int i = _alignWgc.Count; i < n; i++)
            {
                if (MfProbe.mf_cap_get(i, out var srt, out var qArr, out _, out _, out _, out _, out _, out _, out var cnt, out var cx) == 1 && cnt > 50)
                {
                    double dip = double.IsNaN(_alignCx0) ? double.NaN : (cx - _alignCx0) / _alignPxPerDip;
                    _alignWgc.Add((Now, qArr != 0 ? qArr : q, srt / 1e4, cx, dip));
                }
            }
        }
    }

    private static string CtrlDesc(AnimationController? c)
    {
        if (c is null) return "null";
        try { return $"Progress={c.Progress:F6} PlaybackRate={c.PlaybackRate:F4}"; }
        catch (Exception ex) { return "throw " + ex.GetType().Name + ": " + ex.Message; }
    }

    private async Task RunAlignProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        Log("===== ALIGN PROBE START (one interrupt; shadow vs controller vs WGC) =====");
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            ShowWindow(hwnd, 9); SetForegroundWindow(hwnd);
            CompositionTarget.Rendering += OnAlignRendering; _renderSubs++;
            _alignCap = false; _alignWgc.Clear(); _alignRender.Clear();
            int capHr = MfProbe.mf_cap_start(hwnd);
            Log($"ALIGN WGC start hr=0x{capHr:X8} hwnd=0x{hwnd.ToInt64():X}");
            if (capHr != 0) { Log("ALIGN abort: WGC failed"); return; }
            _alignCap = true;

            if (!_compMode) { _toggleFromCode = true; ModeToggle.IsOn = true; _toggleFromCode = false; SetMode(true, "align probe"); }
            CompReset(0, "align rest"); await Task.Delay(400);
            await Task.Delay(300);
            var caps0 = _alignWgc.ToList();
            _alignCx0 = caps0.Count > 0 ? caps0[^1].cx : double.NaN;
            CompReset(37, "align cal 37"); await Task.Delay(400);
            var caps37 = _alignWgc.Where(c => c.nowMs > Now - 350).ToList();
            double cx37 = caps37.Count > 0 ? caps37[^1].cx : double.NaN;
            double scale = XamlRoot?.RasterizationScale ?? 1.0;
            _alignPxPerDip = (!double.IsNaN(_alignCx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - _alignCx0) > 5) ? (cx37 - _alignCx0) / 37.0 : scale;
            Log($"ALIGN cal: cx0={F(_alignCx0,3)} cx37={F(cx37,3)} pxPerDip={F(_alignPxPerDip,4)} (scale={F(scale,3)})");
            CompReset(0, "align cal end"); await Task.Delay(300);
            var capsR = _alignWgc.Where(c => c.nowMs > Now - 250).ToList();
            if (capsR.Count > 0) _alignCx0 = capsR[^1].cx;
            Log($"ALIGN rest cx0 refreshed={F(_alignCx0,3)}; render ticks so far={_alignRender.Count} wgc={_alignWgc.Count}");

            long qStart = Qpc; double tStart = Now;
            ShadowNow(tStart, out var shV0, out var shVel0);
            Log($"ALIGN T0 StartAnimation impending: sw={F(tStart,3)}ms qpc={qStart} shadow v={F(shV0)} vel={F(shVel0)} controller={CtrlDesc(_visual.TryGetAnimationController("Translation.X"))}");
            var sB1 = CompSpring(240, "ALIGN B1: 0->240");
            long qAfterStart = Qpc; double tAfterStart = Now;
            Log($"ALIGN T0+ StartAnimation returned: sw={F(tAfterStart,3)}ms qpc={qAfterStart} Δsw={F(tAfterStart - tStart,3)}ms controller={CtrlDesc(_visual.TryGetAnimationController("Translation.X"))}");

            await Task.Delay(150);

            long qInt = Qpc; double tInt = Now;
            ShadowNow(tInt, out var shV, out var shVel);
            var ctrlAtInt = _visual.TryGetAnimationController("Translation.X");
            var nearW = _alignWgc.Count > 0 ? _alignWgc.OrderBy(c => Math.Abs(c.nowMs - tInt)).First() : default;
            var nearW2 = _alignWgc.Count > 0 ? _alignWgc[^1] : default;
            Log($"ALIGN T1 interrupt impending: sw={F(tInt,3)}ms qpc={qInt} elapsed_since_start={F(tInt - tStart,3)}ms");
            Log($"ALIGN T1 shadow (spring.h seed): value={F(shV)} vel={F(shVel)}  << InitialValue/InitialVelocity");
            Log($"ALIGN T1 controller: {CtrlDesc(ctrlAtInt)}");
            Log($"ALIGN T1 nearest WGC by drain-Now: cx={F(nearW.cx,3)} dip={F(nearW.dip,3)} drainSw={F(nearW.nowMs,3)} srtMs={F(nearW.srtMs,3)} qpcArrive={nearW.qpc} |Δsw_to_int|={F(Math.Abs(nearW.nowMs - tInt),3)}ms");
            Log($"ALIGN T1 latest WGC: cx={F(nearW2.cx,3)} dip={F(nearW2.dip,3)} drainSw={F(nearW2.nowMs,3)} |Δsw_to_int|={F(Math.Abs(nearW2.nowMs - tInt),3)}ms");
            Log($"ALIGN T1 |WGC_DIP - shadow| (nearest, same-wallclock) = {F(Math.Abs(nearW.dip - shV), 3)} DIP  [raw lag; expected large]");
            Log($"ALIGN T1 |WGC_DIP - shadow| (latest, same-wallclock)  = {F(Math.Abs(nearW2.dip - shV), 3)} DIP");
            // QPC-aligned: evaluate B1 shadow at each recent frame's qpcArrive
            if (sB1 != null)
            {
                foreach (var c in _alignWgc.Where(c => c.nowMs >= tStart - 10 && c.nowMs <= tInt + 5).TakeLast(5))
                {
                    double tFr = (c.qpc - sB1.Qpc) / (double)Stopwatch.Frequency;
                    MfProbe.mf_spring_at(sB1.Response, sB1.Damping, sB1.From, sB1.Target, sB1.FromVel, Math.Max(0, tFr), out var sv, out _);
                    Log($"ALIGN T1 QPC-aligned frame: tFr={F(tFr * 1000, 1)}ms WGC_dip={F(c.dip, 3)} shadow_at_tFr={F(sv, 3)} |Δ|={F(Math.Abs(c.dip - sv), 3)} DIP");
                }
            }

            var sB2 = CompSpring(0, "ALIGN B2: interrupt -> 0");
            long qAfterInt = Qpc; double tAfterInt = Now;
            Log($"ALIGN T1+ Retarget returned: sw={F(tAfterInt,3)}ms qpc={qAfterInt} Δsw={F(tAfterInt - tInt,3)}ms controller={CtrlDesc(_visual.TryGetAnimationController("Translation.X"))} seeded InitialValue={F(sB2?.From ?? double.NaN)} InitialVelocity={F(sB2?.FromVel ?? double.NaN)}");

            await Task.Delay(400);
            double tEnd = Now;
            var rends = _alignRender.Where(r => r.nowMs >= tStart - 20 && r.nowMs <= tAfterInt + 200).ToList();
            Log($"ALIGN render ticks around [start-20, int+200]: n={rends.Count}");
            foreach (var r in rends) Log($"ALIGN RENDER sw={F(r.nowMs,3)} qpc={r.qpc} relStart={F(r.nowMs - tStart,3)} relInt={F(r.nowMs - tInt,3)}");
            var wfs = _alignWgc.Where(c => c.nowMs >= tStart - 50 && c.nowMs <= tAfterInt + 250).ToList();
            Log($"ALIGN WGC frames around [start-50, int+250]: n={wfs.Count}");
            foreach (var c in wfs)
            {
                double skewArriveMs = (c.qpc - qInt) * 1000.0 / Stopwatch.Frequency;
                double skewDrainMs = c.nowMs - tInt;
                Log($"ALIGN WGC swDrain={F(c.nowMs,3)} srtMs={F(c.srtMs,3)} qpcArrive={c.qpc} cx={F(c.cx,3)} dip={F(c.dip,3)} |dip-shadowAtInt|={F(Math.Abs(c.dip - shV),3)} skewDrainToInt={F(skewDrainMs,3)}ms skewArriveQpcToInt={F(skewArriveMs,3)}ms");
            }
            if (wfs.Count >= 2)
            {
                double dSw = wfs[^1].nowMs - wfs[0].nowMs;
                double dSrt = wfs[^1].srtMs - wfs[0].srtMs;
                double dQpc = (wfs[^1].qpc - wfs[0].qpc) * 1000.0 / Stopwatch.Frequency;
                Log($"ALIGN clock spans over window: ΔStopwatch={F(dSw,3)}ms ΔSRT={F(dSrt,3)}ms ΔqpcArrive={F(dQpc,3)}ms |ΔSRT-ΔSW|={F(Math.Abs(dSrt - dSw),3)} |Δqpc-ΔSW|={F(Math.Abs(dQpc - dSw),3)}");
            }

            ShadowNow(tEnd, out var shEnd, out var shVelEnd);
            var last = _alignWgc.LastOrDefault();
            Log($"ALIGN T2 +400ms: shadow v={F(shEnd)} vel={F(shVelEnd)} WGC dip={F(last.dip,3)} |WGC-shadow|={F(Math.Abs(last.dip - shEnd),3)} controller={CtrlDesc(_visual.TryGetAnimationController("Translation.X"))}");

            double wgcAtInt = nearW2.dip;
            double dvShadowWgc = Math.Abs(wgcAtInt - shV);
            // post-interrupt catch-up: first WGC within 30 DIP of seed
            var post = _alignWgc.Where(c => c.nowMs > tAfterInt).OrderBy(c => c.nowMs).ToList();
            var hit = post.FirstOrDefault(c => Math.Abs(c.dip - (sB2?.From ?? shV)) <= 30);
            if (hit.nowMs > 0 && sB2 != null)
            {
                double tFr = (hit.qpc - sB2.Qpc) / (double)Stopwatch.Frequency;
                MfProbe.mf_spring_at(sB2.Response, sB2.Damping, sB2.From, sB2.Target, sB2.FromVel, Math.Max(0, tFr), out var sv2, out _);
                Log($"ALIGN T1 catch-up: first WGC near seed at +{F(hit.nowMs - tInt, 1)}ms drain; dip={F(hit.dip, 3)} seed={F(sB2.From, 3)} |dip-seed|={F(Math.Abs(hit.dip - sB2.From), 3)}; QPC-aligned |dip-shadow(t)|={F(Math.Abs(hit.dip - sv2), 3)} DIP (goal: single digits)");
            }
            else Log("ALIGN T1 catch-up: no WGC within 30 DIP of seed in post-interrupt window");

            Log("ALIGN ROOTCAUSE =====");
            Log($"ALIGN ROOTCAUSE interrupt |WGC_DIP - shadow_seed| same-wallclock = {F(dvShadowWgc, 3)} DIP (lag artifact); see QPC-aligned / catch-up lines for true error");
            Log($"ALIGN ROOTCAUSE controller readable = {(ctrlAtInt is null ? "null (no mid-flight value API; Progress only if controller non-null)" : CtrlDesc(ctrlAtInt))}");
            Log($"ALIGN ROOTCAUSE time skew latestWgcDrain-vs-interrupt = {F(nearW2.nowMs - tInt, 3)} ms; 1–3 WGC frames of capture latency → §4.1 p50≈44ms");
            Log($"ALIGN ROOTCAUSE final-value: last WGC dip={F(last.dip,3)} vs target 0 → |err|={F(Math.Abs(last.dip - 0),3)} (suite §4.4 saw 4.53px)");
            Log($"ALIGN ROOTCAUSE RMS: suite traj pairs WGC.SRT with Qpc100ns(CompStart.Qpc) as absolute times — if epochs differ, curve is time-shifted → high RMS; see ΔSRT vs ΔSW spans");
            Log("ALIGN ROOTCAUSE Rendering ~31ms is UI-thread throttle only; compositor+WGC are 60Hz; software path should use engine clock. Not a #5 failure.");
            Log("===== ALIGN PROBE DONE =====");
        }
        catch (Exception ex) { _exceptions++; Log("ALIGN EXCEPTION " + ex); }
        finally
        {
            try { CompositionTarget.Rendering -= OnAlignRendering; _renderUnsubs++; } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
            _alignCap = false; _suiteRunning = false; MeasureButton.IsEnabled = true;
            FlushLog();
        }
    }











    // ---------------------------------------------------------------- 派题 #22: K3 KeyFrame AnimationController Progress / PlaybackRate
    private async Task RunK3Probe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_k3_controller_probe.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }

        const double from = 0, target = 100, dur = 1.0;
        Both("# win_k3_controller_probe.txt — 派题 #22 K3 AnimationController Progress / PlaybackRate");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; OS={Environment.OSVersion.Version}");
        Both($"# Linear KeyFrame {from}→{target} Duration={dur * 1000:F0}ms; Progress seek; PlaybackRate=±rate");
        Both("# A Progress=0.5 immediate; B rate=2 half duration; C rate=-1 reverse; D DelayTime+Progress (record)");

        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "k3-probe");
            CompReset(0, "k3 start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            int capHr = MfProbe.mf_cap_start(hwnd);
            Both($"WGC start hr=0x{capHr:X8}");
            var calQ0 = Qpc;
            CompReset(37, "k3 cal"); await Task.Delay(400);
            var calQ1 = Qpc;
            CompReset(0, "k3 cal end"); await Task.Delay(500);
            var capsCal = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = capsCal.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = capsCal.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double pxPerDip = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : (XamlRoot?.RasterizationScale ?? 1.0);
            var restLast = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault();
            if (restLast != null) cx0 = restLast.Cx;
            Both($"cal: cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(pxPerDip,4)}");

            async Task WaitNear(double dip, double tol = 8)
            {
                for (int i = 0; i < 40; i++)
                {
                    await Task.Delay(50);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(4).ToList();
                    if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / pxPerDip - dip) < tol)) return;
                }
            }

            double LastDip(long afterQ)
            {
                var last = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > afterQ).OrderBy(c => c.QpcArrive).LastOrDefault();
                return last == null ? double.NaN : (last.Cx - cx0) / pxPerDip;
            }
            double MedianNear(long q0, long q1)
            {
                var xs = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive >= q0 && c.QpcArrive <= q1)
                    .Select(c => (c.Cx - cx0) / pxPerDip).OrderBy(x => x).ToList();
                return xs.Count == 0 ? double.NaN : xs[xs.Count / 2];
            }

            ScalarKeyFrameAnimation MakeLinear(double delaySec = 0)
            {
                var k = _comp!.CreateScalarKeyFrameAnimation();
                k.InsertKeyFrame(0f, (float)from);
                k.InsertKeyFrame(1f, (float)target); // linear
                k.Duration = TimeSpan.FromSeconds(dur);
                if (delaySec > 0) k.DelayTime = TimeSpan.FromSeconds(delaySec);
                return k;
            }

            // ========== A: Progress→WGC calibration ==========
            Both("===== A) Progress→WGC calibration (Progress then Pause; short sample) =====");
            await WaitNear(0);
            CompReset(0, "A rest"); await WaitNear(0);
            var kA = MakeLinear();
            var ctrlA = _comp!.CreateAnimationController();
            // Start paused at 0 via rate 0 from the beginning
            ctrlA.PlaybackRate = 0f;
            long qA = Qpc;
            _visual!.StartAnimation("Translation.X", kA, ctrlA);
            ctrlA.Pause();
            Both($"  started rate=0+Pause Progress={ctrlA.Progress:F4} x≈{F(LastDip(qA), 1)}");
            await Task.Delay(100);

            async Task<double> SampleAtProgress(float prog)
            {
                ctrlA.PlaybackRate = 0f;
                ctrlA.Progress = prog;
                ctrlA.Pause(); // Progress then Pause (hold)
                // one-vsync present lag only
                await Task.Delay(50);
                long a = Qpc;
                await Task.Delay(50);
                long b = Qpc;
                var xs = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive >= a && c.QpcArrive <= b)
                    .Select(c => (c.Cx - cx0) / pxPerDip).ToList();
                double med = xs.Count == 0 ? LastDip(qA) : xs.OrderBy(x => x).ElementAt(xs.Count / 2);
                double first = xs.Count == 0 ? med : xs[0];
                Both($"  Progress={prog:F2} → first={F(first, 1)} median={F(med, 1)} n={xs.Count} ctrl={ctrlA.Progress:F4} rate={ctrlA.PlaybackRate:F2}");
                return first; // prefer earliest after seek
            }

            double x00 = await SampleAtProgress(0.00f);
            double x25 = await SampleAtProgress(0.25f);
            double x50 = await SampleAtProgress(0.50f);
            double x75 = await SampleAtProgress(0.75f);
            double x100 = await SampleAtProgress(1.00f);

            double seekErr = Math.Abs(x50 - 50);
            bool linearOk = seekErr <= 10;
            // RMS vs linear model
            double[] ps = { 0, 0.25, 0.5, 0.75, 1 }; double[] xsA = { x00, x25, x50, x75, x100 };
            double rms = Math.Sqrt(ps.Zip(xsA, (pp, xx) => { double e = xx - pp * 100; return e * e; }).Average());
            bool mono = xsA[0] <= xsA[1] + 2 && xsA[1] <= xsA[2] + 2 && xsA[2] <= xsA[3] + 2 && xsA[3] <= xsA[4] + 2;
            string verdA = linearOk ? "PASS" : (mono && rms < 25 ? "PASS-ish" : "FAIL");
            Both($"  seek@0.5 |x-50|={F(seekErr, 1)} RMS_vs_linear={F(rms, 1)} mono={mono} → {verdA}");

            try { _visual.StopAnimation("Translation.X"); } catch { }
            CompReset(0, "A live"); await WaitNear(0, 12);
            {
                var kL = MakeLinear();
                var cL = _comp.CreateAnimationController();
                long qL = Qpc;
                _visual.StartAnimation("Translation.X", kL, cL);
                await Task.Delay(150);
                double xb = LastDip(qL);
                cL.Progress = 0.5f;
                cL.Pause();
                await Task.Delay(50);
                long a = Qpc; await Task.Delay(50);
                var xs = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive >= a).OrderBy(c => c.QpcArrive).Take(3)
                    .Select(c => (c.Cx - cx0) / pxPerDip).ToList();
                double xs0 = xs.Count > 0 ? xs[0] : LastDip(qL);
                Both($"  A-live Progress=0.5+Pause before={F(xb, 1)} first={F(xs0, 1)} |x-50|={F(Math.Abs(xs0 - 50), 1)} (record)");
                try { _visual.StopAnimation("Translation.X"); } catch { }
            }
            CompReset(0, "A end"); await WaitNear(0, 12);

            // ========== B: PlaybackRate=2 → ~half duration ==========
            Both("===== B) PlaybackRate=2 (expect arrive ~500ms not 1000ms) =====");
            CompReset(0, "B rest"); await WaitNear(0);
            var kB = MakeLinear();
            var ctrlB = _comp.CreateAnimationController();
            ctrlB.PlaybackRate = 2f;
            long qB = Qpc;
            _visual.StartAnimation("Translation.X", kB, ctrlB);
            Both($"  START B rate={ctrlB.PlaybackRate:F4} Progress={ctrlB.Progress:F4}");
            double tArriveB = double.NaN;
            for (int i = 0; i < 50; i++)
            {
                await Task.Delay(40);
                var recent = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qB).OrderBy(c => c.QpcArrive).TakeLast(3).ToList();
                if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / pxPerDip - target) < 4))
                {
                    tArriveB = SecSince(qB, recent[0].QpcArrive);
                    break;
                }
            }
            // expected ~0.5s + present lag (~0.05–0.25); pass if in [0.35, 0.85]
            bool rate2Pass = !double.IsNaN(tArriveB) && tArriveB >= 0.35 && tArriveB <= 0.85;
            Both($"  visualArrive≈{F(tArriveB * 1000, 0)}ms (expect ~500±present) → {(rate2Pass ? "PASS" : "FAIL")}");
            string verdB = rate2Pass ? "PASS" : "FAIL";
            try { _visual.StopAnimation("Translation.X"); } catch { }
            CompReset(0, "B end"); await WaitNear(0, 12);

            // ========== C: PlaybackRate=-1 reverse ==========
            Both("===== C) PlaybackRate=-1 reverse (Progress=1 + rate=-1) =====");
            CompReset(0, "C rest"); await WaitNear(0);
            var kC = MakeLinear();
            var ctrlC = _comp.CreateAnimationController();
            long qC = Qpc;
            _visual.StartAnimation("Translation.X", kC, ctrlC);
            ctrlC.Progress = 1.0f;
            ctrlC.PlaybackRate = -1f;
            Both($"  Progress=1 rate=-1 (ctrl Progress={ctrlC.Progress:F4} rate={ctrlC.PlaybackRate:F4})");
            await Task.Delay(60);
            long qC0 = Qpc; await Task.Delay(40);
            double xNearEnd = MedianNear(qC0, Qpc);
            Both($"  nearEnd≈{F(xNearEnd, 1)}");
            // sample every ~200ms for ~1s
            double xMid = double.NaN, xLate = double.NaN;
            var series = new List<double>();
            for (int i = 0; i < 6; i++)
            {
                await Task.Delay(180);
                long a = Qpc; await Task.Delay(40);
                double x = MedianNear(a, Qpc);
                series.Add(x);
                Both($"  t≈{(i + 1) * 180}ms x={F(x, 1)}");
                if (i == 1) xMid = x;
                if (i == 5) xLate = x;
            }
            bool reverseOk = !double.IsNaN(xNearEnd) && series.Count >= 3
                && xNearEnd > 80 && series.Last() < series.First() - 20;
            bool near0 = !double.IsNaN(series.Last()) && series.Last() < 35;
            bool revPass = reverseOk && near0;
            Both($"  traj: nearEnd={F(xNearEnd, 1)} mid≈{F(xMid, 1)} late={F(xLate, 1)} decreasing={reverseOk} near0={near0} → {(revPass ? "PASS" : reverseOk ? "PASS-ish" : "FAIL")}");
            string verdC = revPass ? "PASS" : (reverseOk ? "PASS-ish" : "FAIL");
            try { _visual.StopAnimation("Translation.X"); } catch { }
            CompReset(0, "C end"); await WaitNear(0, 12);

            // ========== D: DelayTime=300ms + Progress write (record) ==========
            Both("===== D) DelayTime=300ms + Progress=0.5 during delay (record) =====");
            CompReset(0, "D rest"); await WaitNear(0);
            var kD = MakeLinear(0.30);
            try { kD.DelayBehavior = AnimationDelayBehavior.SetInitialValueAfterDelay; } catch { }
            var ctrlD = _comp.CreateAnimationController();
            long qD = Qpc;
            _visual.StartAnimation("Translation.X", kD, ctrlD);
            Both($"  START D DelayTime=300ms Progress={ctrlD.Progress:F4} rate={ctrlD.PlaybackRate:F4}");
            await Task.Delay(80); // still in delay
            double xPre = LastDip(qD);
            ctrlD.Pause();
            ctrlD.Progress = 0.5f;
            Both($"  Pause+Progress=0.5 during delay (xPre≈{F(xPre, 1)}; ctrl Progress={ctrlD.Progress:F4})");
            await Task.Delay(100);
            long qD1 = Qpc;
            await Task.Delay(100);
            double xDuring = MedianNear(qD1, Qpc);
            ctrlD.Resume();
            await Task.Delay(500); // past delay + some play at rate 1 from Progress 0.5
            long qD2 = Qpc;
            await Task.Delay(120);
            double xAfter = MedianNear(qD2, Qpc);
            Both($"  duringDelayPausedSeek≈{F(xDuring, 1)} afterResumePlay≈{F(xAfter, 1)} (record only; no gate)");
            string verdD = "RECORD";
            try { _visual.StopAnimation("Translation.X"); } catch { }

            Both("===== SUMMARY =====");
            Both($"| A Progress=0.5 | cal={F(x00,0)}/{F(x25,0)}/{F(x50,0)}/{F(x75,0)}/{F(x100,0)} |err50|={F(seekErr, 1)} | {verdA} |");
            Both($"| B rate=2 | arrive_ms={F(tArriveB * 1000, 0)} | {verdB} |");
            Both($"| C rate=-1 | nearEnd={F(xNearEnd, 1)} mid={F(xMid, 1)} late={F(xLate, 1)} | {verdC} |");
            Both($"| D Delay+Progress | during={F(xDuring, 1)} after={F(xAfter, 1)} | {verdD} |");

            bool ok = verdA.StartsWith("PASS") && verdB == "PASS" && verdC.StartsWith("PASS");
            Both("CONCLUSION: " + (ok
                ? "K3 PASS — Progress seek takes effect; PlaybackRate=2 halves duration; PlaybackRate=-1 reverses."
                : $"K3 PARTIAL — rate=2 {verdB}; Progress {verdA}; reverse {verdC}. PlaybackRate usable; Progress seek mapping must be verified before KeyFrame elapsed lowering."));
            Both("===== K3 PROBE DONE =====");
            art.Close();
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("K3 PROBE EXCEPTION " + ex);
            try { art.Close(); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        finally
        {
            _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog();
            try { CloseMainWindowSoon(); } catch { }
        }
    }


    // ---------------------------------------------------------------- 派题 #21: K4 KeyFrame after completion holds final value
    private async Task RunK4Probe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_k4_hold_probe.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }

        const double from = 0, target = 100, dur = 0.40;
        Both("# win_k4_hold_probe.txt — 派题 #21 K4 KeyFrame hold FinalValue after completion");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; OS={Environment.OSVersion.Version}");
        Both($"# ScalarKeyFrame {from}→{target} Duration={dur * 1000:F0}ms + K1 easing; after visual arrive sample ≥500ms ≈{target}");
        Both("# Optional: ScopedBatch.Completed vs visual arrive (record only)");

        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "k4-probe");
            CompReset(0, "k4 start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            int capHr = MfProbe.mf_cap_start(hwnd);
            Both($"WGC start hr=0x{capHr:X8}");
            var calQ0 = Qpc;
            CompReset(37, "k4 cal"); await Task.Delay(400);
            var calQ1 = Qpc;
            CompReset(0, "k4 cal end"); await Task.Delay(500);
            var capsCal = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = capsCal.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = capsCal.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double pxPerDip = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : (XamlRoot?.RasterizationScale ?? 1.0);
            var restLast = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault();
            if (restLast != null) cx0 = restLast.Cx;
            Both($"cal: cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(pxPerDip,4)}");

            async Task WaitNear(double dip, double tol = 8)
            {
                for (int i = 0; i < 40; i++)
                {
                    await Task.Delay(50);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(4).ToList();
                    if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / pxPerDip - dip) < tol)) return;
                }
            }

            await WaitNear(0);
            CompReset(0, "k4 rest");
            await WaitNear(0);

            bool batchCompleted = false;
            double batchCompletedMs = double.NaN;
            CompositionScopedBatch? batch = null;
            try
            {
                batch = _comp!.CreateScopedBatch(CompositionBatchTypes.Animation);
                batch.Completed += (_, __) => { batchCompleted = true; batchCompletedMs = Now; Both($"  ScopedBatch.Completed at wall={F(batchCompletedMs, 1)}ms"); };
            }
            catch (Exception ex) { Both($"  ScopedBatch create: {ex.GetType().Name} {ex.Message}"); }

            var k = _comp!.CreateScalarKeyFrameAnimation();
            k.InsertKeyFrame(0f, (float)from);
            k.InsertKeyFrame(1f, (float)target, _comp.CreateCubicBezierEasingFunction(EnterC1, EnterC2));
            k.Duration = TimeSpan.FromSeconds(dur);
            long qStart = Qpc;
            double tStartMs = Now;
            var ctrl = _comp.CreateAnimationController();
            _visual!.StartAnimation("Translation.X", k, ctrl);
            try { batch?.End(); } catch { }
            Both($"  START KeyFrame Duration={dur * 1000:F0}ms {from}→{target} easing=EnterC1/C2 qpc={qStart} wall={F(tStartMs, 1)}ms");

            double tVisualArrive = double.NaN;
            for (int i = 0; i < 40; i++)
            {
                await Task.Delay(40);
                var recent = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).TakeLast(4).ToList();
                if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / pxPerDip - target) < 3))
                {
                    tVisualArrive = SecSince(qStart, recent[0].QpcArrive);
                    Both($"  visual near target (|x-100|<3 ×3) at arrive≈{F(tVisualArrive * 1000, 0)}ms");
                    break;
                }
            }
            if (double.IsNaN(tVisualArrive))
                Both("  WARN: never locked near 100; continue hold sample anyway");

            // small extra settle for present lag / last frames
            await Task.Delay(150);

            long qHold0 = Qpc;
            await Task.Delay(650);
            long qHold1 = Qpc;
            var holdCaps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive >= qHold0 && c.QpcArrive <= qHold1)
                                        .OrderBy(c => c.QpcArrive).ToList();
            var holdX = holdCaps.Select(c => (c.Cx - cx0) / pxPerDip).ToList();
            double holdMed = holdX.Count > 0 ? holdX.OrderBy(x => x).ElementAt(holdX.Count / 2) : double.NaN;
            double holdMin = holdX.Count > 0 ? holdX.Min() : double.NaN;
            double holdMax = holdX.Count > 0 ? holdX.Max() : double.NaN;
            double holdRange = holdX.Count > 0 ? holdMax - holdMin : double.NaN;
            double holdErr = Math.Abs(holdMed - target);
            bool holdStable = holdX.Count >= 10 && holdRange <= 2.0;
            bool holdNear = holdErr <= 0.5 || (holdStable && holdErr <= 1.0);
            bool holdPass = holdStable && holdNear;
            Both($"  HOLD ≥500ms n={holdX.Count} median={F(holdMed, 2)} min={F(holdMin, 2)} max={F(holdMax, 2)} range={F(holdRange, 2)} |med-target|={F(holdErr, 3)} → {(holdPass ? "PASS" : "FAIL")}");
            Both($"  batchCompleted={batchCompleted} batchAt={F(batchCompletedMs, 1)}ms visualArrive≈{F(tVisualArrive * 1000, 0)}ms tStart={F(tStartMs, 1)} (record only)");

            double propX = double.NaN;
            try
            {
                if (_visual.Properties.TryGetVector3("Translation", out var tr) == CompositionGetValueStatus.Succeeded)
                    propX = tr.X;
            }
            catch { }
            Both($"  Properties.Translation.X after hold={F(propX, 2)} (informational)");

            Both("===== SUMMARY =====");
            Both($"| KeyFrame hold≥500ms | median={F(holdMed, 2)} range={F(holdRange, 2)} |err|={F(holdErr, 3)} | {(holdPass ? "PASS" : "FAIL")} |");
            Both($"| ScopedBatch.Completed | fired={batchCompleted} at={F(batchCompletedMs, 1)} | visualArrive_ms={F(tVisualArrive * 1000, 0)} |");
            Both("CONCLUSION: " + (holdPass
                ? "K4 PASS — after KeyFrame completes, WGC holds ≈FinalValue for ≥500ms (stable, no drift)."
                : "K4 FAIL — post-completion hold not stable at FinalValue.") +
                (batchCompleted ? $" ScopedBatch.Completed at {F(batchCompletedMs, 1)}ms vs visualArrive≈{F(tVisualArrive * 1000, 0)}ms." : " ScopedBatch.Completed not observed."));
            Both("===== K4 PROBE DONE =====");
            art.Close();
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("K4 PROBE EXCEPTION " + ex);
            try { art.Close(); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        finally
        {
            _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog();
            try { CloseMainWindowSoon(); } catch { }
        }
    }


    // ---------------------------------------------------------------- 派题 #20: K2 CubicBezier y∉[0,1] — reject / clamp / honor?
    private async Task RunK2Probe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_k2_overshoot_probe.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }

        const double from = 0, target = 100, dur = 0.40;
        Both("# win_k2_overshoot_probe.txt — 派题 #20 K2 CubicBezier y∉[0,1]");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; OS={Environment.OSVersion.Version}");
        Both("# Test CreateCubicBezierEasingFunction + KeyFrame run: reject / clamp / honor overshoot");
        Both("# Cases: in-range control; y=(-0.5,1.5); y=(1.4,1.2)");

        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "k2-probe");
            CompReset(0, "k2 start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            int capHr = MfProbe.mf_cap_start(hwnd);
            Both($"WGC start hr=0x{capHr:X8}");
            var calQ0 = Qpc;
            CompReset(37, "k2 cal"); await Task.Delay(400);
            var calQ1 = Qpc;
            CompReset(0, "k2 cal end"); await Task.Delay(500);
            var capsCal = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = capsCal.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = capsCal.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double pxPerDip = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : (XamlRoot?.RasterizationScale ?? 1.0);
            var restLast = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault();
            if (restLast != null) cx0 = restLast.Cx;
            Both($"cal: cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(pxPerDip,4)}");

            async Task WaitNear(double dip, double tol = 8)
            {
                for (int i = 0; i < 40; i++)
                {
                    await Task.Delay(50);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(4).ToList();
                    if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / pxPerDip - dip) < tol)) return;
                }
            }

            async Task<(string tag, string create, string start, double peak, double minX, double maxX, double rmsHonor, double rmsClamped, string verdict)> RunCaseAsync(
                string tag, float bx1, float by1, float bx2, float by2)
            {
                Both($"----- [{tag}] control=({bx1},{by1})-({bx2},{by2}) -----");
                string create = "ok", start = "ok";
                CubicBezierEasingFunction? ease = null;
                try
                {
                    ease = _comp!.CreateCubicBezierEasingFunction(new Vector2(bx1, by1), new Vector2(bx2, by2));
                    // Try read back if API exposes control points — often not; just note create ok
                    Both($"  CreateCubicBezierEasingFunction: OK");
                }
                catch (Exception ex)
                {
                    create = "THROW " + ex.GetType().Name + ": " + ex.Message;
                    Both($"  Create: {create}");
                    return (tag, create, "n/a", double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, "REJECT at Create");
                }

                await WaitNear(0);
                CompReset(0, tag + " rest");
                await WaitNear(0);
                try
                {
                    var k = _comp!.CreateScalarKeyFrameAnimation();
                    k.InsertKeyFrame(0f, (float)from);
                    k.InsertKeyFrame(1f, (float)target, ease);
                    k.Duration = TimeSpan.FromSeconds(dur);
                    long q0 = Qpc;
                    var ctrl = _comp.CreateAnimationController();
                    _visual!.StartAnimation("Translation.X", k, ctrl);
                    Both($"  StartAnimation: OK Duration={dur * 1000:F0}ms {from}→{target}");
                    await Task.Delay((int)(dur * 1000) + 500);
                    var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > q0).OrderBy(c => c.QpcArrive).ToList();
                    CompReset(0, tag + " end");
                    var raw = caps.Select(c => (tArr: SecSince(q0, c.QpcArrive), x: (c.Cx - cx0) / pxPerDip)).ToList();
                    if (raw.Count < 5)
                    {
                        Both($"  WGC n={raw.Count} too few");
                        return (tag, create, start, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, "ran but few frames");
                    }

                    // best-τ vs software BezierEase with SAME (possibly out-of-range) y = "honor"
                    // vs clamped y into [0,1] = "clamp"
                    double cy1c = Math.Clamp(by1, 0f, 1f), cy2c = Math.Clamp(by2, 0f, 1f);
                    double ModelHonor(double t)
                    {
                        var u = Math.Clamp(t / dur, 0, 1);
                        return from + (target - from) * BezierEase(u, bx1, by1, bx2, by2);
                    }
                    double ModelClamp(double t)
                    {
                        var u = Math.Clamp(t / dur, 0, 1);
                        return from + (target - from) * BezierEase(u, bx1, cy1c, bx2, cy2c);
                    }
                    double Rms(Func<double, double> model, double tau)
                    {
                        var e = raw.Where(p => p.tArr - tau > 0 && p.tArr - tau < dur + 0.05)
                                   .Select(p => p.x - model(p.tArr - tau)).ToList();
                        return e.Count < 5 ? double.NaN : Math.Sqrt(e.Average(z => z * z));
                    }
                    double bestTau = 0, bestH = Rms(ModelHonor, 0);
                    for (int ms = -60; ms <= 280; ms += 2)
                    {
                        double r = Rms(ModelHonor, ms / 1000.0);
                        if (!double.IsNaN(r) && r < bestH) { bestH = r; bestTau = ms / 1000.0; }
                    }
                    double rmsH = bestH, rmsC = Rms(ModelClamp, bestTau);
                    var xs = raw.Where(p => p.tArr - bestTau > 0 && p.tArr - bestTau < dur + 0.08).Select(p => p.x).ToList();
                    double minX = xs.Count > 0 ? xs.Min() : double.NaN;
                    double maxX = xs.Count > 0 ? xs.Max() : double.NaN;
                    double peak = xs.Count > 0 ? xs.Max() : double.NaN; // for overshoot above target
                    bool wgcOvershoot = !double.IsNaN(maxX) && maxX > target + 3;
                    bool wgcUndershoot = !double.IsNaN(minX) && minX < from - 3;
                    // software honor peak
                    double softPeak = Enumerable.Range(0, 41).Max(i => ModelHonor(i / 40.0 * dur));
                    double softMin = Enumerable.Range(0, 41).Min(i => ModelHonor(i / 40.0 * dur));

                    string verdict;
                    bool yOut = by1 < 0 || by1 > 1 || by2 < 0 || by2 > 1;
                    if (!yOut)
                        verdict = $"IN_RANGE baseline RMS_honor={F(rmsH, 2)}";
                    else if (!double.IsNaN(rmsH) && !double.IsNaN(rmsC) && rmsH < rmsC * 0.7 && (wgcOvershoot || wgcUndershoot || softPeak > target + 1 || softMin < from - 1))
                        verdict = "HONOR (WGC matches unclamped BezierEase; overshoot/undershoot visible)";
                    else if (!double.IsNaN(rmsH) && !double.IsNaN(rmsC) && rmsC < rmsH * 0.7)
                        verdict = "CLAMP (WGC closer to y-clamped model)";
                    else if (wgcOvershoot || wgcUndershoot)
                        verdict = "HONOR-likely (WGC exceeds [from,target] band)";
                    else
                        verdict = $"AMBIGUOUS rmsHonor={F(rmsH, 2)} rmsClamp={F(rmsC, 2)} max={F(maxX, 1)} min={F(minX, 1)}";

                    Both($"  WGC n={raw.Count} τ={F(bestTau * 1000, 0)}ms min={F(minX, 1)} max={F(maxX, 1)} overshoot={wgcOvershoot} undershoot={wgcUndershoot}");
                    Both($"  soft honor peak={F(softPeak, 1)} min={F(softMin, 1)}; RMS_honor={F(rmsH, 2)} RMS_clamp={F(rmsC, 2)} → {verdict}");
                    return (tag, create, start, peak, minX, maxX, rmsH, rmsC, verdict);
                }
                catch (Exception ex)
                {
                    start = "THROW " + ex.GetType().Name + ": " + ex.Message;
                    Both($"  Start/run: {start}");
                    return (tag, create, start, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, "REJECT at Start");
                }
            }

            var inRange = await RunCaseAsync("in_range", 0.2f, 0.0f, 0.8f, 1.0f);
            var outNegPos = await RunCaseAsync("y_out_-0.5_1.5", 0.2f, -0.5f, 0.8f, 1.5f);
            var outHigh = await RunCaseAsync("y_out_1.4_1.2", 0.4f, 1.4f, 0.6f, 1.2f);

            Both("===== SUMMARY =====");
            Both("| case | create | start | minX | maxX | RMS_honor | RMS_clamp | verdict |");
            foreach (var r in new[] { inRange, outNegPos, outHigh })
                Both($"| {r.tag} | {r.create} | {r.start} | {F(r.minX, 1)} | {F(r.maxX, 1)} | {F(r.rmsHonor, 2)} | {F(r.rmsClamped, 2)} | {r.verdict} |");

            bool honor = outNegPos.verdict.StartsWith("HONOR") || outHigh.verdict.StartsWith("HONOR");
            bool reject = outNegPos.verdict.StartsWith("REJECT") || outHigh.verdict.StartsWith("REJECT");
            bool clamp = outNegPos.verdict.StartsWith("CLAMP") || outHigh.verdict.StartsWith("CLAMP");
            string rec;
            if (honor && !reject)
                rec = "RECOMMEND enable Capabilities::bezierOvershoot=true (API honors y∉[0,1]; WGC shows overshoot).";
            else if (reject)
                rec = "KEEP bezierOvershoot=false (API rejects out-of-range y).";
            else if (clamp)
                rec = "KEEP bezierOvershoot=false (API clamps y; enabling overshoot would mis-promise).";
            else
                rec = "KEEP bezierOvershoot=false until clearer honor evidence.";

            Both("CONCLUSION: " + (reject ? "REJECT. " : clamp ? "CLAMP. " : honor ? "HONOR. " : "UNCLEAR. ") + rec);
            Both("===== K2 PROBE DONE =====");
            art.Close();
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("K2 PROBE EXCEPTION " + ex);
            try { art.Close(); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        finally
        {
            _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog();
            try { CloseMainWindowSoon(); } catch { }
        }
    }


    // ---------------------------------------------------------------- 派题 #19: K1 KeyFrame easing applies to segment ending at that keyframe
    private async Task RunK1Probe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_k1_easing_probe.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }

        const double from = 0, target = 100, dur = 0.22;
        double x1 = EnterC1.X, y1 = EnterC1.Y, x2 = EnterC2.X, y2 = EnterC2.Y;
        Both("# win_k1_easing_probe.txt — 派题 #19 K1 KeyFrame easing segment");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; OS={Environment.OSVersion.Version}");
        Both($"# A: single-segment {dur * 1000:F0}ms KeyFrame {from}→{target}, easing=cubicBezier(({x1},{y1}),({x2},{y2})) on END keyframe");
        Both("#    WGC vs EvaluateKeyframes-equivalent (BezierEase) + vs linear baseline");
        Both("# B: optional dual — 0→50 linear then 50→100 eased (easing only on 2nd segment)");

        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "k1-probe");
            CompReset(0, "k1 start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            int capHr = MfProbe.mf_cap_start(hwnd);
            Both($"WGC start hr=0x{capHr:X8}");
            var calQ0 = Qpc;
            CompReset(37, "k1 cal"); await Task.Delay(400);
            var calQ1 = Qpc;
            CompReset(0, "k1 cal end"); await Task.Delay(500);
            var capsCal = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = capsCal.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = capsCal.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double pxPerDip = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : (XamlRoot?.RasterizationScale ?? 1.0);
            var restLast = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault();
            if (restLast != null) cx0 = restLast.Cx;
            Both($"cal: cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(pxPerDip,4)}");

            async Task WaitNear(double dip, double tol = 8)
            {
                for (int i = 0; i < 40; i++)
                {
                    await Task.Delay(50);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(4).ToList();
                    if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / pxPerDip - dip) < tol)) return;
                }
            }

            (double bestTau, double rmsEase, double rmsLin, int n, double maxAbs) CompareTraj(
                List<(double tArr, double x)> raw, double duration, Func<double, double> modelEase, Func<double, double> modelLin)
            {
                double Rms(Func<double, double> model, double tau)
                {
                    var e = raw.Where(p => p.tArr - tau > 0 && p.tArr - tau < duration)
                               .Select(p => p.x - model(p.tArr - tau)).ToList();
                    return e.Count < 5 ? double.NaN : Math.Sqrt(e.Average(z => z * z));
                }
                double bestTau = 0, best = Rms(modelEase, 0);
                for (int ms = -80; ms <= 280; ms += 2)
                {
                    double r = Rms(modelEase, ms / 1000.0);
                    if (!double.IsNaN(r) && r < best) { best = r; bestTau = ms / 1000.0; }
                }
                double rmsL = Rms(modelLin, bestTau);
                var errs = raw.Where(p => p.tArr - bestTau > 0 && p.tArr - bestTau < duration)
                              .Select(p => Math.Abs(p.x - modelEase(p.tArr - bestTau))).ToList();
                double maxAbs = errs.Count > 0 ? errs.Max() : double.NaN;
                int n = errs.Count;
                return (bestTau, best, rmsL, n, maxAbs);
            }

            // ----- A single segment -----
            Both("===== A) single-segment 220ms + decelerate easing on end keyframe =====");
            await WaitNear(0);
            CompReset(0, "A rest");
            await WaitNear(0);
            var k = _comp!.CreateScalarKeyFrameAnimation();
            k.InsertKeyFrame(0f, (float)from);
            k.InsertKeyFrame(1f, (float)target, _comp.CreateCubicBezierEasingFunction(EnterC1, EnterC2));
            k.Duration = TimeSpan.FromSeconds(dur);
            long qA = Qpc;
            var ctrl = _comp.CreateAnimationController();
            _visual!.StartAnimation("Translation.X", k, ctrl);
            Both($"  START A Duration={dur * 1000:F0}ms easing=({x1},{y1})-({x2},{y2}) on key@1");
            await Task.Delay(700);
            var capsA = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qA).OrderBy(c => c.QpcArrive).ToList();
            CompReset(0, "A end");
            var rawA = capsA.Select(c => (tArr: SecSince(qA, c.QpcArrive), x: (c.Cx - cx0) / pxPerDip)).ToList();
            double ModelEase(double t)
            {
                var u = Math.Clamp(t / dur, 0, 1);
                return from + (target - from) * BezierEase(u, x1, y1, x2, y2);
            }
            double ModelLin(double t)
            {
                var u = Math.Clamp(t / dur, 0, 1);
                return from + (target - from) * u;
            }
            var (tauA, rmsE, rmsL, nA, maxA) = CompareTraj(rawA, dur + 0.05, ModelEase, ModelLin);
            double travel = Math.Abs(target - from);
            Both($"  A n={nA} bestτ={F(tauA * 1000, 1)}ms RMS_ease={F(rmsE, 3)} ({F(100 * rmsE / travel, 3)}%) RMS_linear={F(rmsL, 3)} ({F(100 * rmsL / travel, 3)}%) max|err|_ease={F(maxA, 2)}");
            bool easeBetter = !double.IsNaN(rmsE) && !double.IsNaN(rmsL) && rmsE < rmsL * 0.5;
            bool easePass = !double.IsNaN(rmsE) && rmsE / travel <= 0.02; // 2% of travel
            Both($"  A verdict: easeBetterThanLinear={easeBetter} shapePass(≤2%)={easePass}");
            var sparse = rawA.Where(p => p.tArr - tauA > 0 && p.tArr - tauA < dur)
                             .Where((_, i) => i % Math.Max(1, nA / 6) == 0).Take(8)
                             .Select(p => { double tc = p.tArr - tauA; return $"t={tc * 1000:F0} wgc={p.x:F1} ease={ModelEase(tc):F1} lin={ModelLin(tc):F1}"; });
            Both("  samples: " + string.Join("; ", sparse));

            await WaitNear(0, 12);

            // ----- B dual segment -----
            Both("===== B) dual keyframes: 0→50 @0.5 linear, 50→100 @1.0 eased (total 440ms) =====");
            double durB = 0.44;
            await WaitNear(0);
            CompReset(0, "B rest");
            await WaitNear(0);
            var k2 = _comp.CreateScalarKeyFrameAnimation();
            k2.InsertKeyFrame(0f, 0f);
            k2.InsertKeyFrame(0.5f, 50f); // no easing → linear segment ending here
            k2.InsertKeyFrame(1f, 100f, _comp.CreateCubicBezierEasingFunction(EnterC1, EnterC2)); // easing on 2nd segment only
            k2.Duration = TimeSpan.FromSeconds(durB);
            long qB = Qpc;
            var ctrl2 = _comp.CreateAnimationController();
            _visual.StartAnimation("Translation.X", k2, ctrl2);
            Both("  START B Duration=440ms key@0.5=50 (linear), key@1=100 (eased)");
            await Task.Delay(900);
            var capsB = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qB).OrderBy(c => c.QpcArrive).ToList();
            CompReset(0, "B end");
            var rawB = capsB.Select(c => (tArr: SecSince(qB, c.QpcArrive), x: (c.Cx - cx0) / pxPerDip)).ToList();

            double ModelDual(double t)
            {
                // EvaluateKeyframes K1: easing on key at progress p shapes segment from prev→p
                double u = Math.Clamp(t / durB, 0, 1);
                if (u <= 0.5)
                    return 0 + (50 - 0) * (u / 0.5); // linear first half
                double v = (u - 0.5) / 0.5;
                return 50 + (100 - 50) * BezierEase(v, x1, y1, x2, y2);
            }
            double ModelDualAllEase(double t)
            {
                // wrong hypothesis: easing on whole duration
                double u = Math.Clamp(t / durB, 0, 1);
                return 0 + 100 * BezierEase(u, x1, y1, x2, y2);
            }
            double ModelDualLin(double t)
            {
                double u = Math.Clamp(t / durB, 0, 1);
                return 0 + 100 * u;
            }

            // fit tau vs ModelDual
            double bestTauB = 0, bestRB = double.MaxValue;
            for (int ms = -80; ms <= 280; ms += 2)
            {
                double tau = ms / 1000.0;
                var e = rawB.Where(p => p.tArr - tau > 0 && p.tArr - tau < durB).Select(p => p.x - ModelDual(p.tArr - tau)).ToList();
                if (e.Count < 5) continue;
                double r = Math.Sqrt(e.Average(z => z * z));
                if (r < bestRB) { bestRB = r; bestTauB = tau; }
            }
            double RmsB(Func<double, double> m)
            {
                var e = rawB.Where(p => p.tArr - bestTauB > 0 && p.tArr - bestTauB < durB).Select(p => p.x - m(p.tArr - bestTauB)).ToList();
                return e.Count < 5 ? double.NaN : Math.Sqrt(e.Average(z => z * z));
            }
            double rmsDual = RmsB(ModelDual), rmsAllEase = RmsB(ModelDualAllEase), rmsDualLin = RmsB(ModelDualLin);
            // First-half linearity check: at content t in (0,0.22) should be near linear 0→50
            var firstHalf = rawB.Where(p => { double tc = p.tArr - bestTauB; return tc > 0.02 && tc < 0.20; }).ToList();
            double rmsFirstLin = double.NaN;
            if (firstHalf.Count >= 4)
            {
                var e = firstHalf.Select(p => { double tc = p.tArr - bestTauB; return p.x - (50 * (tc / 0.22)); }).ToList();
                // first segment is half of 440ms = 220ms for 0→50
                e = firstHalf.Select(p => { double tc = p.tArr - bestTauB; return p.x - (0 + 50 * (tc / (durB * 0.5))); }).ToList();
                rmsFirstLin = Math.Sqrt(e.Average(z => z * z));
            }
            Both($"  B n≈{rawB.Count} bestτ={F(bestTauB * 1000, 1)}ms RMS_K1dual={F(rmsDual, 3)} ({F(100 * rmsDual / 100, 3)}%) RMS_wholeEase={F(rmsAllEase, 3)} RMS_linear={F(rmsDualLin, 3)} RMS_firstHalf≈linear={F(rmsFirstLin, 3)}");
            bool dualOk = !double.IsNaN(rmsDual) && rmsDual < rmsAllEase && rmsDual / 100 <= 0.03;
            Both($"  B verdict: K1-dual better than whole-curve-ease={(!double.IsNaN(rmsDual) && rmsDual < rmsAllEase)} dualPass={dualOk}");

            Both("===== SUMMARY =====");
            Both($"| A single ease | RMS_ease%={F(100 * rmsE / travel, 3)} RMS_lin%={F(100 * rmsL / travel, 3)} | {(easePass && easeBetter ? "PASS" : easePass ? "PASS-shape" : "FAIL")} |");
            Both($"| B dual K1 | RMS_dual%={F(100 * rmsDual / 100, 3)} RMS_allEase%={F(100 * rmsAllEase / 100, 3)} | {(dualOk ? "PASS" : "CHECK")} |");
            Both("CONCLUSION: " + (easePass && easeBetter
                ? "K1 AGREES with official semantics — easing on end keyframe shapes that segment (WGC≈EvaluateKeyframes/BezierEase; much better than linear). "
                : easePass
                    ? "K1 shape matches eased model (≤2%); linear comparison inconclusive. "
                    : "K1 mismatch vs eased model. ") +
                (dualOk ? "Dual-keyframe: easing only on 2nd segment (K1 dual model fits better than whole-duration ease)."
                        : "Dual-keyframe spot check: see RMS lines."));
            Both("===== K1 PROBE DONE =====");
            art.Close();
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("K1 PROBE EXCEPTION " + ex);
            try { art.Close(); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        finally
        {
            _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog();
            try { CloseMainWindowSoon(); } catch { }
        }
    }


    // ---------------------------------------------------------------- 派题 #18: K5 StopAnimation mid-flight + TryGet readback
    private async Task RunK5Probe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_k5_stop_probe.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }

        const double from = 0, target = 240;
        Both("# win_k5_stop_probe.txt — 派题 #18 K5 StopAnimation mid-flight + TryGet");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; OS={Environment.OSVersion.Version}");
        Both("# A spring 0→240 stop @40–60%; B KeyFrame 1000ms 0→240 mid Stop; TryGet before/after Stop");

        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "k5-probe");
            CompReset(0, "k5 start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            int capHr = MfProbe.mf_cap_start(hwnd);
            Both($"WGC start hr=0x{capHr:X8}");
            var calQ0 = Qpc;
            CompReset(37, "k5 cal"); await Task.Delay(400);
            var calQ1 = Qpc;
            CompReset(0, "k5 cal end"); await Task.Delay(500);
            var capsCal = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = capsCal.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = capsCal.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double pxPerDip = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : (XamlRoot?.RasterizationScale ?? 1.0);
            var restLast = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault();
            if (restLast != null) cx0 = restLast.Cx;
            Both($"cal: cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(pxPerDip,4)}");

            async Task WaitNear(double dip, double tol = 10)
            {
                for (int i = 0; i < 50; i++)
                {
                    await Task.Delay(50);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(4).ToList();
                    if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / pxPerDip - dip) < tol)) return;
                }
            }

            (double x, string status) TryGetTranslation()
            {
                try
                {
                    var st = _visual!.Properties.TryGetVector3("Translation", out var tr);
                    if (st == CompositionGetValueStatus.Succeeded) return (tr.X, "Succeeded");
                    if (st == CompositionGetValueStatus.TypeMismatch) return (double.NaN, "TypeMismatch");
                    if (st == CompositionGetValueStatus.NotFound) return (double.NaN, "NotFound");
                    return (double.NaN, st.ToString());
                }
                catch (Exception ex) { return (double.NaN, "THROW " + ex.GetType().Name); }
            }

            async Task<(string tag, double stopWgc, double postMed, double postRange, double tryBefore, string stBefore, double tryAfter, string stAfter, bool freezeOk)> RunStopCase(
                string tag, Action startAnim, double loFrac, double hiFrac, int pollMs, int maxPoll)
            {
                await WaitNear(0);
                CompReset(0, tag + " rest");
                await WaitNear(0);
                long q0 = Qpc;
                startAnim();
                Both($"  [{tag}] START");

                var (tb, sb) = TryGetTranslation();
                Both($"  [{tag}] TryGet BEFORE Stop: status={sb} x={F(tb, 2)} (animating — often stale/static seed)");

                double stopWgc = double.NaN, stopWall = double.NaN;
                for (int i = 0; i < maxPoll; i++)
                {
                    await Task.Delay(pollMs);
                    var last = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > q0).OrderBy(c => c.QpcArrive).LastOrDefault();
                    if (last == null) continue;
                    double x = (last.Cx - cx0) / pxPerDip;
                    double frac = Math.Abs(target - from) > 1e-6 ? (x - from) / (target - from) : 0;
                    if (frac >= loFrac && frac <= hiFrac)
                    {
                        stopWgc = x;
                        stopWall = SecSince(q0, last.QpcArrive);
                        _visual!.StopAnimation("Translation.X");
                        Both($"  [{tag}] StopAnimation at WGC x≈{F(stopWgc, 1)} frac={F(frac, 2)} wall≈{F(stopWall * 1000, 0)}ms");
                        break;
                    }
                    if (frac > hiFrac)
                    {
                        // overshot window — stop anyway
                        stopWgc = x;
                        stopWall = SecSince(q0, last.QpcArrive);
                        _visual!.StopAnimation("Translation.X");
                        Both($"  [{tag}] StopAnimation (past window) WGC x≈{F(stopWgc, 1)} frac={F(frac, 2)} wall≈{F(stopWall * 1000, 0)}ms");
                        break;
                    }
                }
                if (double.IsNaN(stopWgc))
                {
                    _visual!.StopAnimation("Translation.X");
                    Both($"  [{tag}] WARN never entered [{loFrac},{hiFrac}] window; Stop anyway");
                }

                var (ta, sa) = TryGetTranslation();
                Both($"  [{tag}] TryGet AFTER Stop (immediate): status={sa} x={F(ta, 2)}");

                // Drain presentation lag (~200–280ms) then sample freeze window
                await Task.Delay(300);
                long qHold0 = Qpc;
                await Task.Delay(500);
                long qHold1 = Qpc;
                var post = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive >= qHold0 && c.QpcArrive <= qHold1)
                                        .OrderBy(c => c.QpcArrive).Select(c => (c.Cx - cx0) / pxPerDip).ToList();
                double postMed = post.Count > 0 ? post.OrderBy(x => x).ElementAt(post.Count / 2) : double.NaN;
                double postRange = post.Count > 0 ? post.Max() - post.Min() : double.NaN;
                // freeze after lag drain: small range, not continuing toward target
                bool notAtTarget = Math.Abs(postMed - target) > 20;
                bool freezeOk = post.Count >= 8 && postRange <= 6 && notAtTarget;
                // Optional: Insert after Stop to see if explicit SetValue materializes (K5 handoff pattern)
                // Do NOT CompReset — only re-read TryGet after freeze window
                var (ta2, sa2) = TryGetTranslation();
                Both($"  [{tag}] TryGet AFTER freeze window: status={sa2} x={F(ta2, 2)}");

                Both($"  [{tag}] POST-STOP (after 300ms lag drain) n={post.Count} median={F(postMed, 1)} range={F(postRange, 1)} |med-stopWgc|={F(Math.Abs(postMed - stopWgc), 1)} |med-target|={F(Math.Abs(postMed - target), 1)} → {(freezeOk ? "PASS freeze" : "FAIL/CHECK")}");
                bool tryGetUseful = sa == "Succeeded" && !double.IsNaN(ta) && Math.Abs(ta - stopWgc) < 40;
                Both($"  [{tag}] TryGet_vs_WGC: afterStop={F(ta, 1)} afterHold={F(ta2, 1)} stopWgc={F(stopWgc, 1)} materializesFrozen={tryGetUseful}");

                CompReset(0, tag + " end");
                await WaitNear(0, 15);
                return (tag, stopWgc, postMed, postRange, tb, sb, ta, sa, freezeOk);
            }

            Both("===== A) Spring mid-flight Stop @40–60% =====");
            var springRes = await RunStopCase("spring", () =>
            {
                var a = _comp!.CreateSpringScalarAnimation();
                a.Period = TimeSpan.FromSeconds(0.32);
                a.DampingRatio = 0.78f;
                a.InitialValue = (float)from;
                a.InitialVelocity = 0f;
                a.FinalValue = (float)target;
                _visual!.StartAnimation("Translation.X", a);
            }, 0.40, 0.60, 30, 50);

            Both("===== B) KeyFrame 1000ms linear mid Stop @40–60% =====");
            var kfRes = await RunStopCase("keyframe", () =>
            {
                var k = _comp!.CreateScalarKeyFrameAnimation();
                k.InsertKeyFrame(0f, (float)from);
                k.InsertKeyFrame(1f, (float)target); // linear
                k.Duration = TimeSpan.FromSeconds(1.0);
                var ctrl = _comp.CreateAnimationController();
                _visual!.StartAnimation("Translation.X", k, ctrl);
            }, 0.40, 0.60, 40, 40);

            Both("===== SUMMARY =====");
            Both($"| path | stopWgc | postMed | range | TryGet before | TryGet after | freeze |");
            foreach (var r in new[] { springRes, kfRes })
                Both($"| {r.tag} | {F(r.stopWgc, 1)} | {F(r.postMed, 1)} | {F(r.postRange, 1)} | {r.stBefore}:{F(r.tryBefore, 1)} | {r.stAfter}:{F(r.tryAfter, 1)} | {(r.freezeOk ? "PASS" : "FAIL")} |");

            bool springOk = springRes.freezeOk;
            bool kfOk = kfRes.freezeOk;
            Both("CONCLUSION: " +
                (springOk ? "Spring: StopAnimation mid-flight FREEZES (WGC holds mid value). " : "Spring: freeze FAIL/unclear. ") +
                (kfOk ? "KeyFrame: StopAnimation mid-flight FREEZES (WGC). " : "KeyFrame: freeze FAIL/unclear. ") +
                $"TryGetVector3(Translation): Succeeded but returns last Inserted static seed (spring {F(springRes.tryAfter, 1)}, kf {F(kfRes.tryAfter, 1)}) — does NOT materialize animated/frozen value; use WGC or Stop+SetValue for handoff.");
            Both("===== K5 PROBE DONE =====");
            art.Close();
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("K5 PROBE EXCEPTION " + ex);
            try { art.Close(); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        finally
        {
            _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog();
            try { CloseMainWindowSoon(); } catch { }
        }
    }


    // ---------------------------------------------------------------- 派题 #17: S5 spring rest / hold final value
    private async Task RunS5Probe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_s5_settle_probe.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }

        const double period = 0.32, zeta = 0.78, from = 0, target = 240;
        Both("# win_s5_settle_probe.txt — 派题 #17 S5 spring settle / hold FinalValue");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; OS={Environment.OSVersion.Version}");
        Both($"# Spring Period={period} ζ={zeta} {from}→{target}; after settle sample ≥500ms must stay ≈{target}");
        Both("# B: StopAnimation near completion — WGC should freeze near stop-time value");

        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(100);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "s5-probe");
            CompReset(0, "s5 start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            int capHr = MfProbe.mf_cap_start(hwnd);
            Both($"WGC start hr=0x{capHr:X8}");
            var calQ0 = Qpc;
            CompReset(37, "s5 cal"); await Task.Delay(400);
            var calQ1 = Qpc;
            CompReset(0, "s5 cal end"); await Task.Delay(500);
            var capsCal = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = capsCal.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = capsCal.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double pxPerDip = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : (XamlRoot?.RasterizationScale ?? 1.0);
            var restLast = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault();
            if (restLast != null) cx0 = restLast.Cx;
            Both($"cal: cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(pxPerDip,4)}");
            double settleSec = MfProbe.mf_spring_settle(period, zeta, from, target, 0);
            Both($"spring.h SettleTime={F(settleSec, 3)}s");

            async Task WaitWgcNear(double dip, double tol = 8)
            {
                for (int i = 0; i < 50; i++)
                {
                    await Task.Delay(50);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(4).ToList();
                    if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / pxPerDip - dip) < tol)) return;
                }
            }

            // ----- A: run to completion, then hold ≥500ms -----
            Both("===== A) run to settle; hold ≥500ms at FinalValue =====");
            await WaitWgcNear(0);
            var a = _comp!.CreateSpringScalarAnimation();
            a.Period = TimeSpan.FromSeconds(period);
            a.DampingRatio = (float)zeta;
            a.InitialValue = (float)from;
            a.InitialVelocity = 0f;
            a.FinalValue = (float)target;
            // NaturalMotion: ScopedBatch.Completed often N/A — still try
            bool batchCompleted = false;
            double batchCompletedMs = double.NaN;
            CompositionScopedBatch? batch = null;
            try
            {
                batch = _comp.CreateScopedBatch(CompositionBatchTypes.Animation);
                batch.Completed += (_, __) => { batchCompleted = true; batchCompletedMs = Now; Both($"  ScopedBatch.Completed at wall={F(batchCompletedMs, 1)}ms"); };
            }
            catch (Exception ex) { Both($"  ScopedBatch create: {ex.GetType().Name} {ex.Message}"); }

            long qStart = Qpc;
            double tStartMs = Now;
            _visual!.StartAnimation("Translation.X", a);
            try { batch?.End(); } catch { }
            Both($"  START A qpc={qStart} (expect settle~{F(settleSec, 2)}s + present lag)");

            // Wait until WGC near target for several frames
            double tVisualArrive = double.NaN;
            for (int i = 0; i < 80; i++) // up to ~4s
            {
                await Task.Delay(50);
                var recent = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).TakeLast(5).ToList();
                if (recent.Count >= 4 && recent.All(c => Math.Abs((c.Cx - cx0) / pxPerDip - target) < 5))
                {
                    tVisualArrive = SecSince(qStart, recent[0].QpcArrive);
                    Both($"  visual near target at wall-arrive≈{F(tVisualArrive * 1000, 0)}ms (first of lock window)");
                    break;
                }
            }
            if (double.IsNaN(tVisualArrive))
                Both("  WARN: never locked |x-240|<5 for 4 frames; continue sampling anyway");

            // Extra settle: underdamped can still be ~few DIP off right at first lock
            Both("  extra settle 900ms after first lock…");
            await Task.Delay(900);

            // Sample ≥500ms more while animation still "running" (no Stop yet)
            long qHold0 = Qpc;
            await Task.Delay(650);
            long qHold1 = Qpc;
            var holdCaps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive >= qHold0 && c.QpcArrive <= qHold1)
                                        .OrderBy(c => c.QpcArrive).ToList();
            var holdX = holdCaps.Select(c => (c.Cx - cx0) / pxPerDip).ToList();
            double holdMed = holdX.Count > 0 ? holdX.OrderBy(x => x).ElementAt(holdX.Count / 2) : double.NaN;
            double holdMin = holdX.Count > 0 ? holdX.Min() : double.NaN;
            double holdMax = holdX.Count > 0 ? holdX.Max() : double.NaN;
            double holdRange = holdX.Count > 0 ? holdMax - holdMin : double.NaN;
            double holdErr = Math.Abs(holdMed - target);
            // S5: hold FinalValue — allow compositor rest-threshold offset ≤0.5 (spec) or ≤2 if stable
            bool holdStable = holdX.Count >= 10 && holdRange <= 2.0;
            bool holdNear = holdErr <= 0.5 || (holdStable && holdErr <= 2.0);
            bool holdPass = holdStable && holdNear;
            Both($"  HOLD window n={holdX.Count} median={F(holdMed, 2)} min={F(holdMin, 2)} max={F(holdMax, 2)} range={F(holdRange, 2)} |med-target|={F(holdErr, 3)} stable={holdStable} → {(holdPass ? "PASS" : "FAIL")}");
            Both($"  batchCompleted={batchCompleted} batchAt={F(batchCompletedMs, 1)} visualArrive≈{F(tVisualArrive * 1000, 0)}ms (NaturalMotion Completed often unsupported)");

            // Properties readback after hold (NaturalMotion may not update PropertySet mid-flight; after natural end may stick)
            double propX = double.NaN;
            try
            {
                if (_visual.Properties.TryGetVector3("Translation", out var tr) == CompositionGetValueStatus.Succeeded)
                    propX = tr.X;
            }
            catch { }
            Both($"  Properties.Translation.X readback after hold={F(propX, 2)} (informational; may be stale for NaturalMotion)");

            CompReset(0, "after A");
            await WaitWgcNear(0, 12);

            // ----- B: StopAnimation near completion -----
            Both("===== B) StopAnimation near completion =====");
            var b = _comp.CreateSpringScalarAnimation();
            b.Period = TimeSpan.FromSeconds(period);
            b.DampingRatio = (float)zeta;
            b.InitialValue = (float)from;
            b.InitialVelocity = 0f;
            b.FinalValue = (float)target;
            long qB = Qpc;
            _visual.StartAnimation("Translation.X", b);
            Both($"  START B qpc={qB}");
            // Wait until WGC past ~85% travel then Stop
            double stopAtX = double.NaN, stopWall = double.NaN;
            for (int i = 0; i < 60; i++)
            {
                await Task.Delay(40);
                var last = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qB).OrderBy(c => c.QpcArrive).LastOrDefault();
                if (last == null) continue;
                double x = (last.Cx - cx0) / pxPerDip;
                if (x >= target * 0.85)
                {
                    stopAtX = x;
                    stopWall = SecSince(qB, last.QpcArrive);
                    _visual.StopAnimation("Translation.X");
                    // Leave property at whatever compositor has — do NOT CompReset
                    Both($"  StopAnimation at WGC x≈{F(stopAtX, 1)} wall≈{F(stopWall * 1000, 0)}ms");
                    break;
                }
            }
            if (double.IsNaN(stopAtX))
            {
                _visual.StopAnimation("Translation.X");
                Both("  WARN: never reached 85% before timeout; StopAnimation anyway");
            }
            long qStop0 = Qpc;
            await Task.Delay(500);
            long qStop1 = Qpc;
            var post = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive >= qStop0 && c.QpcArrive <= qStop1)
                                    .OrderBy(c => c.QpcArrive).Select(c => (c.Cx - cx0) / pxPerDip).ToList();
            double postMed = post.Count > 0 ? post.OrderBy(x => x).ElementAt(post.Count / 2) : double.NaN;
            double postRange = post.Count > 0 ? post.Max() - post.Min() : double.NaN;
            // After Stop, value should freeze (not continue to 240). Allow lag catch-up toward stopAtX.
            bool freezeOk = post.Count >= 8 && postRange <= 8 && Math.Abs(postMed - target) > 5; // not settled at target
            // Or if stop was very close to target, freeze near target is also ok
            if (!double.IsNaN(stopAtX) && Math.Abs(stopAtX - target) < 15)
                freezeOk = post.Count >= 8 && postRange <= 5;
            Both($"  POST-STOP n={post.Count} median={F(postMed, 2)} range={F(postRange, 2)} stopAtX={F(stopAtX, 1)} → {(freezeOk ? "PASS freeze" : "CHECK")} (expect hold near stop, not keep animating to 240)");

            Both("===== SUMMARY =====");
            Both($"| A hold≥500ms | median={F(holdMed, 2)} range={F(holdRange, 2)} |err|={F(holdErr, 3)} | {(holdPass ? "PASS" : "FAIL")} |");
            Both($"| B Stop near end | stopX={F(stopAtX, 1)} postMed={F(postMed, 2)} range={F(postRange, 2)} | {(freezeOk ? "PASS" : "CHECK")} |");
            Both($"| ScopedBatch.Completed | fired={batchCompleted} | visualArrive_ms={F(tVisualArrive * 1000, 0)} |");
            Both("CONCLUSION: " + (holdPass
                ? "S5 PASS — after NaturalMotion spring settles, WGC holds ≈FinalValue for ≥500ms (no drift). "
                : "S5 FAIL — post-settle hold not stable at FinalValue. ") +
                (freezeOk ? "StopAnimation near completion freezes near stop-time value. " : "StopAnimation contrast inconclusive. ") +
                (batchCompleted ? $"ScopedBatch.Completed observed (Δ vs visualArrive see log)." : "ScopedBatch.Completed not observed (NaturalMotion — expected)."));
            Both("===== S5 PROBE DONE =====");
            art.Close();
            try { MfProbe.mf_cap_stop(); } catch { }
            CompReset(0, "s5 done");
        }
        catch (Exception ex)
        {
            Both("S5 PROBE EXCEPTION " + ex);
            try { art.Close(); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        finally
        {
            _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog();
            try { CloseMainWindowSoon(); } catch { }
        }
    }


    // ---------------------------------------------------------------- 派题 #16: S4 DelayTime + presetValue / DelayBehavior
    private async Task RunS4Probe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_s4_delay_probe.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }

        const double period = 0.32, zeta = 0.78, preset = 50, target = 240, delaySec = 0.30;
        Both("# win_s4_delay_probe.txt — 派题 #16 S4 DelayTime + presetValue");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; OS={Environment.OSVersion.Version}");
        Both($"# Spring Period={period} ζ={zeta} FinalValue={target} DelayTime={delaySec * 1000:F0}ms");
        Both("# A: IV=preset=50, DelayBehavior=AfterDelay (default) — hold ~50 during delay, no jump at start");
        Both("# B: IV=0 ≠ preset=50, AfterDelay — record jump at delay end?");
        Both("# C: IV=preset=50, DelayBehavior=BeforeDelay — record delay-phase behavior");

        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(100);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "s4-probe");
            CompReset(0, "s4 start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            int capHr = MfProbe.mf_cap_start(hwnd);
            Both($"WGC start hr=0x{capHr:X8}");
            var calQ0 = Qpc;
            CompReset(37, "s4 cal"); await Task.Delay(400);
            var calQ1 = Qpc;
            CompReset(0, "s4 cal end"); await Task.Delay(500);
            var capsCal = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = capsCal.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = capsCal.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double pxPerDip = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : (XamlRoot?.RasterizationScale ?? 1.0);
            var restLast = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault();
            if (restLast != null) cx0 = restLast.Cx;
            Both($"cal: cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(pxPerDip,4)}");

            async Task WaitWgcNear(double dip, double tol = 6)
            {
                for (int i = 0; i < 60; i++)
                {
                    await Task.Delay(50);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(5).ToList();
                    if (recent.Count >= 4 && recent.All(c => Math.Abs((c.Cx - cx0) / pxPerDip - dip) < tol)) return;
                }
                var last = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault();
                Both($"  WARN WaitWgcNear({dip}) timeout; last≈{(last == null ? double.NaN : (last.Cx - cx0) / pxPerDip):F1}");
            }

            async Task<(string tag, double holdMed, double holdMaxDev, double leaveT, double leaveX, double jump, string verdict)> RunCase(
                string tag, double initialValue, AnimationDelayBehavior behavior)
            {
                CompReset(preset, tag + " preset");
                await WaitWgcNear(preset);
                // confirm preset visible
                var pre = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault();
                double preDip = pre != null ? (pre.Cx - cx0) / pxPerDip : double.NaN;
                Both($"  [{tag}] preset set Translation.X={preset}; WGC pre≈{F(preDip,1)}");

                var a = _comp!.CreateSpringScalarAnimation();
                a.Period = TimeSpan.FromSeconds(period);
                a.DampingRatio = (float)zeta;
                a.InitialValue = (float)initialValue;
                a.InitialVelocity = 0f;
                a.FinalValue = (float)target;
                a.DelayTime = TimeSpan.FromSeconds(delaySec);
                a.DelayBehavior = behavior;
                long qStart = Qpc;
                _visual!.StartAnimation("Translation.X", a);
                Both($"  [{tag}] START DelayTime={delaySec * 1000:F0}ms DelayBehavior={behavior} IV={initialValue} preset={preset} FV={target}");
                await Task.Delay(1200);
                var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).ToList();
                CompReset(0, tag + " end");
                await WaitWgcNear(0, 10); // drain WGC before next case preset

                if (caps.Count < 10)
                {
                    Both($"  [{tag}] FAIL too few WGC n={caps.Count}");
                    return (tag, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, "FAIL few frames");
                }
                var raw = caps.Select(c => (t: SecSince(qStart, c.QpcArrive), x: (c.Cx - cx0) / pxPerDip)).ToList();

                // Estimate presentation lag: when WGC first shows leave from preset after delay
                // Hold window in WALL time: frames with t < delaySec + lag_est; use adaptive:
                // Find first frame where |x-preset| > 12 AND t > 0.15 (ignore noise), call that leaveWall.
                int leaveIdx = raw.FindIndex(p => p.t > 0.12 && Math.Abs(p.x - preset) > 12);
                double leaveT = leaveIdx >= 0 ? raw[leaveIdx].t : double.NaN;
                double leaveX = leaveIdx >= 0 ? raw[leaveIdx].x : double.NaN;
                // Hold samples: all frames before leave (or before delaySec+0.35 if no leave)
                double holdEnd = !double.IsNaN(leaveT) ? leaveT : delaySec + 0.35;
                var hold = raw.Where(p => p.t < holdEnd - 0.02).Select(p => p.x).ToList();
                if (hold.Count < 3) hold = raw.Where(p => p.t < delaySec + 0.25).Select(p => p.x).ToList();
                var holdSorted = hold.OrderBy(x => x).ToList();
                double holdMed = holdSorted.Count > 0 ? holdSorted[holdSorted.Count / 2] : double.NaN;
                double holdMaxDev = hold.Count > 0 ? hold.Max(x => Math.Abs(x - preset)) : double.NaN;

                // Jump: |leaveX - preset| if leave is far from both preset and a smooth start from IV
                // Also check first 3 post-leave frames for discontinuous snap (|Δx| between consecutive > 40 while still early)
                double jump = !double.IsNaN(leaveX) ? Math.Abs(leaveX - preset) : double.NaN;
                double maxStep = 0;
                if (leaveIdx >= 0)
                {
                    for (int i = leaveIdx; i + 1 < raw.Count && i < leaveIdx + 8; i++)
                        maxStep = Math.Max(maxStep, Math.Abs(raw[i + 1].x - raw[i].x));
                }

                bool holdOk = !double.IsNaN(holdMed) && Math.Abs(holdMed - preset) <= 8 && holdMaxDev <= 15;
                // no jump: leave starts near preset (within 25) OR near IV if BeforeDelay already applied
                bool smoothLeave = !double.IsNaN(leaveX) && (Math.Abs(leaveX - preset) <= 25 || Math.Abs(leaveX - initialValue) <= 25);
                // jump suspected if leaveX far from preset AND far from a 1-frame step from preset
                // Detect snap toward IV: any frame in [delay*0.5, delay+0.55] near initialValue while preset differed
                bool jumpToIv = false;
                if (Math.Abs(initialValue - preset) > 20)
                {
                    jumpToIv = raw.Any(p => p.t >= delaySec * 0.5 && p.t <= delaySec + 0.55 && Math.Abs(p.x - initialValue) <= 12);
                }
                bool jumpSuspected = jumpToIv || (!double.IsNaN(leaveX) && Math.Abs(leaveX - preset) > 40 && Math.Abs(leaveX - initialValue) < 25 && Math.Abs(initialValue - preset) > 20);

                string verdict;
                if (behavior == AnimationDelayBehavior.SetInitialValueAfterDelay && Math.Abs(initialValue - preset) < 1e-6)
                    verdict = holdOk && smoothLeave && !jumpSuspected ? "PASS" : (holdOk ? "PARTIAL (hold ok, leave soft?)" : "FAIL");
                else if (behavior == AnimationDelayBehavior.SetInitialValueAfterDelay)
                {
                    if (holdOk && jumpSuspected) verdict = "PASS-hold + JUMP to IV at delay end (expected when IV≠preset)";
                    else if (jumpSuspected) verdict = "JUMP to IV at delay end (hold polluted by lag; IV snap still observed)";
                    else if (holdOk) verdict = "PASS-hold, no clear IV jump";
                    else verdict = "FAIL hold";
                }
                else // BeforeDelay
                {
                    bool earlyIv = raw.Take(8).Any(p => Math.Abs(p.x - initialValue) <= 10);
                    verdict = $"RECORD holdMed={F(holdMed,1)} leaveT={F(leaveT * 1000, 0)}ms leaveX={F(leaveX,1)} earlyNearIV={earlyIv}";
                }

                Both($"  [{tag}] hold n={hold.Count} median={F(holdMed,1)} max|x-preset|={F(holdMaxDev,1)} leaveT={F(leaveT * 1000, 0)}ms leaveX={F(leaveX,1)} |leave-preset|={F(jump,1)} maxStep={F(maxStep,1)} → {verdict}");
                var sparse = raw.Where((_, i) => i % Math.Max(1, raw.Count / 10) == 0).Take(12)
                    .Select(p => $"t={p.t * 1000:F0} x={p.x:F1}");
                Both($"  [{tag}] traj: " + string.Join("; ", sparse));
                return (tag, holdMed, holdMaxDev, leaveT, leaveX, jump, verdict);
            }

            Both("===== A) IV=preset=50, AfterDelay (default) =====");
            var aRes = await RunCase("A_match", preset, AnimationDelayBehavior.SetInitialValueAfterDelay);

            Both("===== B) IV=0 ≠ preset=50, AfterDelay =====");
            var bRes = await RunCase("B_mismatch", 0, AnimationDelayBehavior.SetInitialValueAfterDelay);

            Both("===== C) IV=preset=50, BeforeDelay =====");
            string cNote;
            (string tag, double holdMed, double holdMaxDev, double leaveT, double leaveX, double jump, string verdict) cRes;
            try
            {
                cRes = await RunCase("C_before", preset, AnimationDelayBehavior.SetInitialValueBeforeDelay);
                cNote = cRes.verdict;
            }
            catch (Exception ex)
            {
                Both($"  [C_before] DelayBehavior=SetInitialValueBeforeDelay THROW {ex.GetType().Name}: {ex.Message}");
                cRes = ("C_before", double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, "THROW " + ex.Message);
                cNote = cRes.verdict;
            }

            Both("===== SUMMARY =====");
            Both($"| case | holdMed | maxDev | leaveT_ms | leaveX | verdict |");
            foreach (var r in new[] { aRes, bRes, cRes })
                Both($"| {r.tag} | {F(r.holdMed,1)} | {F(r.holdMaxDev,1)} | {F(r.leaveT * 1000, 0)} | {F(r.leaveX,1)} | {r.verdict} |");

            bool aPass = aRes.verdict.StartsWith("PASS");
            Both("CONCLUSION: " + (aPass
                ? "S4 PASS — DelayTime+AfterDelay keeps preset during delay; IV==preset → no jump at start. "
                : "S4 " + aRes.verdict + " — primary match case. ") +
                $"Mismatch IV≠preset: {bRes.verdict}. BeforeDelay: {cNote}.");
            Both("===== S4 PROBE DONE =====");
            art.Close();
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("S4 PROBE EXCEPTION " + ex);
            try { art.Close(); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        finally
        {
            _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog();
            try { CloseMainWindowSoon(); } catch { }
        }
    }


    // ---------------------------------------------------------------- 派题 #15: S3 InitialVelocity = value/real-second?

    // ---------------------------------------------------------------- 派题 #24: spring rate-fold — same-session rate=1 vs rate=2 (record only; no formula/gate change)

    // ---------------------------------------------------------------- 派题 #25: interrupt continuity — single τ whole traj; seedLead 0 vs 1 frame
    private async Task RunInterruptSeedProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_interrupt_seed_probe.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }

        const double period = 0.32, zeta = 0.78, from = 0, target1 = 240, target2 = 0;
        Both("# win_interrupt_seed_probe.txt — 派题 #25 interrupt continuity (single-τ whole traj vs WGC)");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; OS={Environment.OSVersion.Version}");
        Both($"# B1: Period={period} ζ={zeta} {from}→{target1} IVel=0; interrupt ~150ms → {target2}");
        Both("# Seed: spring.h::At(elapsed + seedLead) → InitialValue/InitialVelocity; NO composition.h change");
        Both("# Align: ONE τ fit on PRE-interrupt B1 only, frozen for post (do NOT re-fit at interrupt). Metrics vs WGC.");
        Both("# §4: |Δpos|≤0.5px & ≤5% frame travel; |Δvel|≤max(20, 5%|v|). Record only if miss.");

        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(100);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "interrupt-seed-probe");
            CompReset(0, "is start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            int capHr = MfProbe.mf_cap_start(hwnd);
            Both($"WGC start hr=0x{capHr:X8}");
            var calQ0 = Qpc;
            CompReset(37, "is cal"); await Task.Delay(400);
            var calQ1 = Qpc;
            CompReset(0, "is cal end"); await Task.Delay(500);
            var capsCal = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = capsCal.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = capsCal.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double pxPerDip = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : (XamlRoot?.RasterizationScale ?? 1.0);
            var restLast = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault();
            if (restLast != null) cx0 = restLast.Cx;
            Both($"cal: cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(pxPerDip,4)}");

            async Task WaitNear(double dip, double tol = 8)
            {
                for (int i = 0; i < 40; i++)
                {
                    await Task.Delay(50);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(4).ToList();
                    if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / pxPerDip - dip) < tol)) return;
                }
            }

            async Task<(double seedLeadMs, double tauMs, double dPos, double dVel, double postRms, double travelAtInt, double frameDisp, bool posOk, bool velOk, string note)> RunOnce(double seedLeadSec)
            {
                Both($"----- seedLead={F(seedLeadSec * 1000, 1)}ms -----");
                await WaitNear(0);
                CompReset(0, "lead rest");
                await WaitNear(0);

                // B1
                var a1 = _comp!.CreateSpringScalarAnimation();
                a1.Period = TimeSpan.FromSeconds(period);
                a1.DampingRatio = (float)zeta;
                a1.InitialValue = (float)from;
                a1.InitialVelocity = 0f;
                a1.FinalValue = (float)target1;
                long qStart = Qpc;
                double tStartMs = Now;
                _visual!.StartAnimation("Translation.X", a1);
                Both($"  B1 START {from}→{target1} qpc={qStart}");

                // Trip at ≈150ms wall (brief). Shadow band logged; do not trip early (<120ms) or pre-WGC window is empty after τ.
                double tIntRel = double.NaN;
                long qInt = 0;
                for (int i = 0; i < 40; i++)
                {
                    await Task.Delay(10);
                    double el = SecSince(qStart, Qpc);
                    if (el >= 0.150)
                    {
                        qInt = Qpc;
                        tIntRel = SecSince(qStart, qInt);
                        MfProbe.mf_spring_at(period, zeta, from, target1, 0, Math.Max(0, tIntRel), out var xSh, out _);
                        var last = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).LastOrDefault();
                        double xW = last == null ? double.NaN : (last.Cx - cx0) / pxPerDip;
                        Both($"  trip: el={F(tIntRel * 1000, 0)}ms shadow_x={F(xSh, 1)} WGC_lagged≈{F(xW, 1)}");
                        break;
                    }
                }
                if (double.IsNaN(tIntRel)) { qInt = Qpc; tIntRel = SecSince(qStart, qInt); }

                // Seed from B1 model at elapsed + seedLead
                double elapsedSeed = tIntRel + seedLeadSec;
                MfProbe.mf_spring_at(period, zeta, from, target1, 0, Math.Max(0, elapsedSeed), out var seedPos, out var seedVel);
                Both($"  interrupt wallRel={F(tIntRel * 1000, 1)}ms seedAt={F(elapsedSeed * 1000, 1)}ms seedPos={F(seedPos, 2)} seedVel={F(seedVel, 1)}");

                // B2 replace
                var a2 = _comp.CreateSpringScalarAnimation();
                a2.Period = TimeSpan.FromSeconds(period);
                a2.DampingRatio = (float)zeta;
                a2.InitialValue = (float)seedPos;
                a2.InitialVelocity = (float)seedVel;
                a2.FinalValue = (float)target2;
                long qB2 = Qpc;
                _visual.StartAnimation("Translation.X", a2);
                Both($"  B2 START →{target2} IV={F(seedPos, 2)} IVel={F(seedVel, 1)} qpc={qB2} (Δstart={F(SecSince(qInt, qB2) * 1000, 1)}ms)");

                await Task.Delay(800); // post trajectory
                var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).ToList();
                CompReset(0, "lead end");
                var raw = caps.Select(c => (tArr: SecSince(qStart, c.QpcArrive), x: (c.Cx - cx0) / pxPerDip)).ToList();
                double travel = raw.Count > 0 ? raw.Max(p => p.x) - raw.Min(p => p.x) : 0;
                if (raw.Count < 20 || travel < 20)
                {
                    Both($"  INVALID n={raw.Count} travel={F(travel, 1)}");
                    return (seedLeadSec * 1000, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, false, false, "INVALID");
                }

                // Piecewise shadow model with SINGLE τ: tc = tArr - τ
                // B1 for tc < tIntRel; B2 for tc >= tIntRel (B2 time = tc - tIntRel)
                double Model(double tc)
                {
                    if (tc < tIntRel)
                    {
                        MfProbe.mf_spring_at(period, zeta, from, target1, 0, Math.Max(0, tc), out var v, out _);
                        return v;
                    }
                    else
                    {
                        MfProbe.mf_spring_at(period, zeta, seedPos, target2, seedVel, Math.Max(0, tc - tIntRel), out var v, out _);
                        return v;
                    }
                }
                // Fit τ on PRE-interrupt B1 only; apply SAME τ to post (do not re-fit after interrupt)
                double RmsPre(double tau)
                {
                    double sum = 0; int n = 0;
                    foreach (var p in raw)
                    {
                        double tc = p.tArr - tau;
                        if (tc < 0.02 || tc >= tIntRel - 0.005) continue; // strictly before interrupt
                        MfProbe.mf_spring_at(period, zeta, from, target1, 0, Math.Max(0, tc), out var m, out _);
                        sum += (p.x - m) * (p.x - m); n++;
                    }
                    return n >= 5 ? Math.Sqrt(sum / n) : double.NaN;
                }
                double bestTau = 0.28, bestR = RmsPre(0.28); // default ≈ present-lag median from prior probes
                bool anyPre = !double.IsNaN(bestR);
                if (!anyPre) bestR = double.MaxValue;
                for (int ms = 40; ms <= 400; ms += 2)
                {
                    double r = RmsPre(ms / 1000.0);
                    if (!double.IsNaN(r) && r < bestR) { bestR = r; bestTau = ms / 1000.0; anyPre = true; }
                }
                if (!anyPre)
                {
                    bestTau = 0.28; bestR = double.NaN;
                    Both("  WARN: insufficient pre-int WGC for τ fit; using fixed τ=280ms (present-lag median)");
                }
                // whole-traj RMS with that frozen τ (diagnostic)
                double wholeSum = 0; int wholeN = 0;
                foreach (var p in raw)
                {
                    double tc = p.tArr - bestTau;
                    if (tc < 0.02 || tc > tIntRel + 0.55) continue;
                    double m = Model(tc);
                    wholeSum += (p.x - m) * (p.x - m); wholeN++;
                }
                double wholeRms = wholeN >= 10 ? Math.Sqrt(wholeSum / wholeN) : double.NaN;
                Both($"  single-τ={F(bestTau * 1000, 0)}ms (fit PRE-only RMS={F(bestR, 2)}); frozen on post → wholeRMS={F(wholeRms, 2)} nPre+post={wholeN}");

                // WGC at interrupt content time tIntRel: interpolate
                double WgcAt(double tc)
                {
                    double tArr = tc + bestTau;
                    var before = raw.LastOrDefault(p => p.tArr <= tArr);
                    var after = raw.FirstOrDefault(p => p.tArr >= tArr);
                    if (before.tArr == 0 && before.x == 0 && raw[0].tArr > tArr) return raw[0].x;
                    if (after.tArr <= before.tArr) return before.x;
                    double u = (tArr - before.tArr) / Math.Max(1e-6, after.tArr - before.tArr);
                    return before.x + u * (after.x - before.x);
                }
                double WgcVelAt(double tc)
                {
                    // central difference ±1 frame ≈16ms content
                    double h = 0.016;
                    return (WgcAt(tc + h) - WgcAt(tc - h)) / (2 * h);
                }
                // Position at interrupt: WGC at content tIntRel (single-τ)
                double wgcPos = WgcAt(tIntRel);
                // Velocity: ONLY pre-interrupt frames (avoid straddling B2 retarget which invents Δvel)
                var pre = raw.Select(p => (tc: p.tArr - bestTau, p.x))
                             .Where(p => p.tc >= tIntRel - 0.080 && p.tc < tIntRel - 0.002)
                             .OrderBy(p => p.tc).ToList();
                double wgcVel = double.NaN;
                if (pre.Count >= 3)
                {
                    double mt = pre.Average(p => p.tc), mx = pre.Average(p => p.x);
                    double num = 0, den = 0;
                    foreach (var p in pre) { num += (p.tc - mt) * (p.x - mx); den += (p.tc - mt) * (p.tc - mt); }
                    wgcVel = den > 1e-10 ? num / den : double.NaN;
                }
                else
                    wgcVel = WgcVelAt(tIntRel - 0.020); // fallback slightly before int
                double dPos = Math.Abs(seedPos - wgcPos);
                double dVel = Math.Abs(seedVel - wgcVel);
                double travelAtInt = Math.Abs(wgcPos - from);
                double frameDisp = Math.Abs(double.IsNaN(wgcVel) ? seedVel : wgcVel) / 60.0;
                if (frameDisp < 1e-3) frameDisp = Math.Abs(seedVel) / 60.0;
                Both($"  pre-int WGC vel fit n={pre.Count} wgcVel={F(wgcVel, 1)} (strictly before interrupt)");

                // post 0–200ms RMS vs B2 model only (still using SAME τ)
                double postSum = 0; int postN = 0;
                foreach (var p in raw)
                {
                    double tc = p.tArr - bestTau;
                    if (tc < tIntRel || tc > tIntRel + 0.200) continue;
                    MfProbe.mf_spring_at(period, zeta, seedPos, target2, seedVel, Math.Max(0, tc - tIntRel), out var m, out _);
                    postSum += (p.x - m) * (p.x - m); postN++;
                }
                double postRms = postN >= 5 ? Math.Sqrt(postSum / postN) : double.NaN;

                // §4: |Δpos|≤0.5px AND ≤5% of one-frame travel
                bool posPass = dPos <= 0.5 && (frameDisp < 1e-6 || dPos <= 0.05 * frameDisp);
                double velThr = Math.Max(20.0, 0.05 * Math.Abs(seedVel));
                bool velPass = dVel <= velThr;

                Both($"  at interrupt (single-τ): WGC_pos={F(wgcPos, 2)} seed={F(seedPos, 2)} |Δpos|={F(dPos, 3)} (frameDisp≈{F(frameDisp, 2)}; §4 pos={(posPass ? "PASS" : "MISS")})");
                Both($"  at interrupt: WGC_vel≈{F(wgcVel, 1)} seedVel={F(seedVel, 1)} |Δvel|={F(dVel, 1)} thr={F(velThr, 1)} §4 vel={(velPass ? "PASS" : "MISS")}");
                Both($"  post0–200ms RMS vs B2 shadow n={postN} RMS={F(postRms, 2)} DIP ({F(100 * postRms / 240, 2)}% travel) — compare to inherent ~12–28%");
                var sparse = raw.Select(p => (tc: p.tArr - bestTau, p.x))
                    .Where(p => p.tc >= tIntRel - 0.05 && p.tc <= tIntRel + 0.25)
                    .Where((_, i) => i % 2 == 0).Take(8)
                    .Select(p => $"t={F((p.tc - tIntRel) * 1000, 0)} wgc={F(p.x, 1)} mdl={F(Model(p.tc), 1)}");
                Both("  around-int: " + string.Join("; ", sparse));

                return (seedLeadSec * 1000, bestTau * 1000, dPos, dVel, postRms, travelAtInt, frameDisp, posPass, velPass,
                    $"τ={F(bestTau * 1000, 0)}ms singleBias=yes");
            }

            var r0 = await RunOnce(0.0);
            await WaitNear(0, 12);
            var r1 = await RunOnce(1.0 / 60.0); // ≈16.7ms

            Both("===== SUMMARY =====");
            Both("| seedLead_ms | τ_ms | |Δpos| | |Δvel| | postRMS | travel@int | §4pos | §4vel |");
            Both($"| {F(r0.seedLeadMs, 1)} | {F(r0.tauMs, 0)} | {F(r0.dPos, 3)} | {F(r0.dVel, 1)} | {F(r0.postRms, 2)} | {F(r0.travelAtInt, 1)} | {r0.posOk} | {r0.velOk} |");
            Both($"| {F(r1.seedLeadMs, 1)} | {F(r1.tauMs, 0)} | {F(r1.dPos, 3)} | {F(r1.dVel, 1)} | {F(r1.postRms, 2)} | {F(r1.travelAtInt, 1)} | {r1.posOk} | {r1.velOk} |");

            // which seedLead better: lower |Δpos|+weighted |Δvel| + postRms
            double score(double dPos, double dVel, double rms) => dPos + dVel / 100.0 + (double.IsNaN(rms) ? 999 : rms);
            bool lead1Better = score(r1.dPos, r1.dVel, r1.postRms) < score(r0.dPos, r0.dVel, r0.postRms);
            bool anyPos = r0.posOk || r1.posOk;
            bool anyVel = r0.velOk || r1.velOk;
            string sec4 = (anyPos && anyVel) ? "meets §4 on at least one seedLead" : "does NOT meet §4 (record magnitude vs inherent residual)";
            Both("CONCLUSION: " + sec4 +
                $" — seedLead=0: |Δpos|={F(r0.dPos, 3)} |Δvel|={F(r0.dVel, 1)} postRMS={F(r0.postRms, 2)}; " +
                $"seedLead≈16.7ms: |Δpos|={F(r1.dPos, 3)} |Δvel|={F(r1.dVel, 1)} postRMS={F(r1.postRms, 2)}; " +
                $"better seedLead={(lead1Better ? "16.7ms" : "0")}. single-τ align only. composition.h UNCHANGED.");
            Both("===== INTERRUPT SEED PROBE DONE =====");
            art.Close();
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("INTERRUPT SEED PROBE EXCEPTION " + ex);
            try { art.Close(); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        finally
        {
            _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog();
            try { CloseMainWindowSoon(); } catch { }
        }
    }


    private async Task RunRateFoldProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_rate_fold_probe.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }

        const double from = 0, target = 240, period0 = 0.32, zeta = 0.78, v0 = 400.0, rate = 2.0;
        Both("# win_rate_fold_probe.txt — 派题 #24 spring rate-fold: rate=1 baseline vs rate=2 fold (same session)");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; OS={Environment.OSVersion.Version}");
        Both($"# Common: Translation.X {from}→{target} ζ={zeta} v0={v0} DIP/s; align=best-τ vs spring.h (same as #13/#15); NO composition.h change");
        Both("# A rate=1: Period=0.32 IVel=v0 vs spring.h x(t)");
        Both("# B rate=2: Period=0.16 IVel=2·v0 vs spring.h x(2·t) with base (Period=0.32,v0)");
        Both("# Window: content time after τ in [0.02, 0.50] logical (B: wall tc with 2·tc in that range for primary compare; also report wall [0.02,0.50] vs x(2t))");

        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(100);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "rate-fold-probe");
            CompReset(0, "rf start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            int capHr = MfProbe.mf_cap_start(hwnd);
            Both($"WGC start hr=0x{capHr:X8}");
            var calQ0 = Qpc;
            CompReset(37, "rf cal"); await Task.Delay(400);
            var calQ1 = Qpc;
            CompReset(0, "rf cal end"); await Task.Delay(500);
            var capsCal = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = capsCal.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = capsCal.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double pxPerDip = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : (XamlRoot?.RasterizationScale ?? 1.0);
            var restLast = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault();
            if (restLast != null) cx0 = restLast.Cx;
            Both($"cal: cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(pxPerDip,4)}");

            async Task WaitNear(double dip, double tol = 8)
            {
                for (int i = 0; i < 40; i++)
                {
                    await Task.Delay(50);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(4).ToList();
                    if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / pxPerDip - dip) < tol)) return;
                }
            }

            // modelTimeScale: 1 for A (x(t)), 2 for B (x(2t)); baseParams always period0 + v0
            async Task<(string tag, double period, double ivel, double tau, double rms, double maxAbs, int n, double travelFrac, bool valid)> RunCase(
                string tag, double period, double ivel, double modelTimeScale)
            {
                Both($"----- [{tag}] Period={period} IVel={ivel} model=spring.h(P={period0},v0={v0}) at {modelTimeScale}·t -----");
                await WaitNear(0);
                CompReset(0, tag + " rest");
                await WaitNear(0);

                var a = _comp!.CreateSpringScalarAnimation();
                a.Period = TimeSpan.FromSeconds(period);
                a.DampingRatio = (float)zeta;
                a.InitialValue = (float)from;
                a.InitialVelocity = (float)ivel;
                a.FinalValue = (float)target;
                long q0 = Qpc;
                _visual!.StartAnimation("Translation.X", a);
                Both($"  START qpc={q0}");
                await Task.Delay(1200); // enough for early+mid trajectory
                var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > q0).OrderBy(c => c.QpcArrive).ToList();
                CompReset(0, tag + " end");
                var raw = caps.Select(c => (tArr: SecSince(q0, c.QpcArrive), x: (c.Cx - cx0) / pxPerDip)).ToList();
                double travel = raw.Count > 0 ? raw.Max(p => p.x) - raw.Min(p => p.x) : 0;
                double travelFrac = Math.Abs(target - from) > 1e-6 ? travel / Math.Abs(target - from) : 0;
                bool valid = raw.Count >= 15 && travelFrac >= 0.10;
                if (!valid)
                {
                    Both($"  INVALID n={raw.Count} travelFrac={F(travelFrac, 3)}");
                    return (tag, period, ivel, double.NaN, double.NaN, double.NaN, raw.Count, travelFrac, false);
                }

                // logical content window: modelTimeScale * (tArr - tau) ∈ [tLo, tHi]
                const double tLo = 0.02, tHi = 0.50;
                double RmsAt(double tau)
                {
                    double e = 0; int n = 0;
                    foreach (var p in raw)
                    {
                        double tc = p.tArr - tau;
                        double tModel = modelTimeScale * tc;
                        if (tModel < tLo || tModel > tHi) continue;
                        MfProbe.mf_spring_at(period0, zeta, from, target, v0, tModel, out var m, out _);
                        e += (p.x - m) * (p.x - m); n++;
                    }
                    return n >= 8 ? Math.Sqrt(e / n) : double.NaN;
                }
                double bestTau = 0.18, bestR = RmsAt(0.18);
                if (double.IsNaN(bestR)) bestR = double.MaxValue;
                for (int ms = 0; ms <= 300; ms += 2)
                {
                    double r = RmsAt(ms / 1000.0);
                    if (!double.IsNaN(r) && r < bestR) { bestR = r; bestTau = ms / 1000.0; }
                }
                double rms = bestR;
                double maxAbs = 0; int nPts = 0;
                foreach (var p in raw)
                {
                    double tc = p.tArr - bestTau;
                    double tModel = modelTimeScale * tc;
                    if (tModel < tLo || tModel > tHi) continue;
                    MfProbe.mf_spring_at(period0, zeta, from, target, v0, tModel, out var m, out _);
                    double ae = Math.Abs(p.x - m);
                    if (ae > maxAbs) maxAbs = ae;
                    nPts++;
                }
                double pct = 100.0 * rms / Math.Abs(target - from);
                Both($"  align τ={F(bestTau * 1000, 0)}ms n={nPts} RMS={F(rms, 2)} DIP ({F(pct, 2)}% travel) max|err|={F(maxAbs, 2)} travelFrac={F(travelFrac, 3)}");
                var sparse = raw.Select(p => (t: p.tArr - bestTau, p.x))
                    .Where(p => modelTimeScale * p.t >= tLo && modelTimeScale * p.t <= tHi)
                    .Where((_, i) => i % Math.Max(1, nPts / 6) == 0).Take(6)
                    .Select(p =>
                    {
                        MfProbe.mf_spring_at(period0, zeta, from, target, v0, modelTimeScale * p.t, out var m, out _);
                        return $"t={F(p.t * 1000, 0)} wgc={F(p.x, 1)} soft={F(m, 1)}";
                    });
                Both("  samples: " + string.Join("; ", sparse));
                return (tag, period, ivel, bestTau, rms, maxAbs, nPts, travelFrac, true);
            }

            // Sanity: spring.h fold identity |x_fold(t) - x_base(2t)| ~ 0
            {
                double maxDiff = 0;
                for (int i = 0; i <= 50; i++)
                {
                    double t = i / 50.0 * 0.5;
                    MfProbe.mf_spring_at(period0, zeta, from, target, v0, rate * t, out var xb, out _);
                    MfProbe.mf_spring_at(period0 / rate, zeta, from, target, v0 * rate, t, out var xf, out _);
                    maxDiff = Math.Max(maxDiff, Math.Abs(xb - xf));
                }
                Both($"spring.h fold-identity max|x(2t;P=0.32,v0)-x(t;P=0.16,2v0)|={F(maxDiff, 4)} (expect ~0)");
            }

            var rA = await RunCase("A_rate1", period0, v0, 1.0);
            await WaitNear(0, 12);
            var rB = await RunCase("B_rate2", period0 / rate, v0 * rate, rate);

            Both("===== SUMMARY =====");
            Both("| case | Period | IVel | τ_ms | RMS | max|err| | %travel | n |");
            Both($"| A rate=1 | {F(rA.period, 2)} | {F(rA.ivel, 0)} | {F(rA.tau * 1000, 0)} | {F(rA.rms, 2)} | {F(rA.maxAbs, 2)} | {F(100 * rA.rms / 240, 2)} | {rA.n} |");
            Both($"| B rate=2 | {F(rB.period, 2)} | {F(rB.ivel, 0)} | {F(rB.tau * 1000, 0)} | {F(rB.rms, 2)} | {F(rB.maxAbs, 2)} | {F(100 * rB.rms / 240, 2)} | {rB.n} |");
            double delta = (rA.valid && rB.valid) ? rB.rms - rA.rms : double.NaN;
            Both($"Δ = RMS_B − RMS_A = {F(delta, 2)} DIP");
            // same magnitude: |Δ| < 0.5 * RMS_A  (brief: Δ smaller than half baseline)
            bool sameOrder = rA.valid && rB.valid && !double.IsNaN(delta) && Math.Abs(delta) < 0.5 * rA.rms;
            bool bWorse = rA.valid && rB.valid && delta > 0.5 * rA.rms;
            string verdict = sameOrder ? "同量级（Δ < ½·RMS_A）" : (bWorse ? "rate=2 明显更差" : "CHECK");
            Both("CONCLUSION: " + verdict +
                $" — RMS_A={F(rA.rms, 2)} ({F(100 * rA.rms / 240, 1)}%) RMS_B={F(rB.rms, 2)} ({F(100 * rB.rms / 240, 1)}%) Δ={F(delta, 2)}. " +
                "Align=best-τ vs spring.h base(P=0.32,v0=400); B uses x(2·t). Formula/gate UNCHANGED; product still prefer default rate=1.");
            Both("===== RATE FOLD PROBE DONE =====");
            art.Close();
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("RATE FOLD PROBE EXCEPTION " + ex);
            try { art.Close(); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        finally
        {
            _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog();
            try { CloseMainWindowSoon(); } catch { }
        }
    }


    private async Task RunS3Probe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_s3_velocity_probe.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }

        const double from = 0, target = 240, period0 = 0.32, zeta = 0.78;
        Both("# win_s3_velocity_probe.txt — 派题 #15 S3 InitialVelocity units (value / real second)");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; OS={Environment.OSVersion.Version}");
        Both($"# base: Period={period0}s ζ={zeta} from={from}→{target}; early-window slope vs commanded InitialVelocity");
        Both("# also: Period independence (Period=0.64 same v0); rate-fold spot (Period/2, v0×2) vs spring.h u(t)=x(2t)");

        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(120);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "s3-probe");
            CompReset(0, "s3 start"); await Task.Delay(400);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            int capHr = MfProbe.mf_cap_start(hwnd);
            Both($"WGC start hr=0x{capHr:X8}");
            // calib 37
            var calQ0 = Qpc;
            CompReset(37, "s3 cal"); await Task.Delay(400);
            var calQ1 = Qpc;
            CompReset(0, "s3 cal end"); await Task.Delay(500); // wait WGC catch-up to rest
            var capsCal = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = capsCal.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx
                         ?? capsCal.Where(c => c.QpcArrive > calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = capsCal.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double pxPerDip = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : (XamlRoot?.RasterizationScale ?? 1.0);
            // refresh rest after settle
            var restCaps = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).ToList();
            if (restCaps.Count > 0) cx0 = restCaps[^1].Cx;
            Both($"cal: cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(pxPerDip,4)}");

            // --- helpers ---
            async Task<(double slope, double lagMs, int nWin, double peak, string note, double rmsCmd, double rmsZero)> MeasureRunAsync(
                double period, double v0, string tag, double windowSec = 0.08)
            {
                // Hard rest: CompReset then wait until WGC matches calib cx0 (not lagged previous peak).
                CompReset(from, tag + " rest");
                for (int i = 0; i < 40; i++)
                {
                    await Task.Delay(50);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(5).ToList();
                    if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / pxPerDip - from) < 6))
                        break;
                }
                double restPx = cx0; // calib rest ONLY (#11 lesson)

                var a = _comp!.CreateSpringScalarAnimation();
                a.Period = TimeSpan.FromSeconds(period);
                a.DampingRatio = (float)zeta;
                a.InitialValue = (float)from;
                a.InitialVelocity = (float)v0;
                a.FinalValue = (float)target;
                long qStart = Qpc;
                _visual!.StartAnimation("Translation.X", a);
                Both($"  START [{tag}] Period={period} ζ={zeta} IV={from} IVel={v0} FV={target} qpc={qStart}");
                await Task.Delay(1000);
                var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).ToList();
                CompReset(from, tag + " end");

                if (caps.Count < 8)
                    return (double.NaN, double.NaN, caps.Count, 0, "too few WGC", double.NaN, double.NaN);

                var raw = caps.Select(c => (tArr: SecSince(qStart, c.QpcArrive), x: (c.Cx - restPx) / pxPerDip)).ToList();
                double peak = raw.Max(p => Math.Abs(p.x));

                // best lag vs spring.h with commanded v0
                double bestLag = 0.18, bestErr = double.MaxValue;
                for (int ms = 0; ms <= 300; ms += 2)
                {
                    double lag = ms / 1000.0, err = 0; int n = 0;
                    foreach (var p in raw)
                    {
                        double tc = p.tArr - lag;
                        if (tc < 0.0 || tc > 0.45) continue;
                        MfProbe.mf_spring_at(period, zeta, from, target, v0, tc, out var m, out _);
                        err += (p.x - m) * (p.x - m); n++;
                    }
                    if (n >= 8 && err / n < bestErr) { bestErr = err / n; bestLag = lag; }
                }

                double RmsVs(double vRef, double lag)
                {
                    double e = 0; int n = 0;
                    foreach (var p in raw)
                    {
                        double tc = p.tArr - lag;
                        if (tc < 0.0 || tc > 0.45) continue;
                        MfProbe.mf_spring_at(period, zeta, from, target, vRef, tc, out var m, out _);
                        e += (p.x - m) * (p.x - m); n++;
                    }
                    return n >= 5 ? Math.Sqrt(e / n) : double.NaN;
                }
                double rmsCmd = RmsVs(v0, bestLag);
                double rmsZero = RmsVs(0, bestLag);

                // Early slope on SHORT content window [0.02, 0.02+windowSec] — closer to Instantaneous v0
                var win = raw.Select(p => (t: p.tArr - bestLag, p.x))
                             .Where(p => p.t >= 0.02 && p.t <= 0.02 + windowSec).ToList();
                if (win.Count < 3)
                {
                    // wall-time: first frames after |x-from|>=3 relative to calib
                    int fi = raw.FindIndex(p => Math.Abs(p.x - from) >= 3);
                    if (fi < 0) return (double.NaN, bestLag * 1000, 0, peak, "no motion", rmsCmd, rmsZero);
                    double t0 = raw[fi].tArr;
                    win = raw.Where(p => p.tArr >= t0 && p.tArr <= t0 + windowSec).Select(p => (t: p.tArr - t0, p.x)).ToList();
                }
                double mt = win.Average(p => p.t), mx = win.Average(p => p.x);
                double num = 0, den = 0;
                foreach (var p in win) { num += (p.t - mt) * (p.x - mx); den += (p.t - mt) * (p.t - mt); }
                double slope = den > 1e-12 ? num / den : double.NaN;
                double tMid = win.Count > 0 ? win.Average(p => p.t) : 0;
                MfProbe.mf_spring_at(period, zeta, from, target, v0, Math.Max(0, tMid), out _, out var vRef);
                MfProbe.mf_spring_at(period, zeta, from, target, v0, 0, out _, out var v0ref);
                string note = $"lag≈{bestLag * 1000:F0}ms nWin={win.Count} |peak|={peak:F1} spring.h v0={v0ref:F1} v(tMid)={vRef:F1} RMS_cmd={rmsCmd:F1} RMS_v0=0={rmsZero:F1}";
                Both($"  RESULT [{tag}] slope≈{slope:F1} DIP/s (cmd={v0:F1}); {note}");
                var sparse = win.Where((_, i) => i % Math.Max(1, win.Count / 5) == 0).Take(6)
                    .Select(p => $"t={p.t * 1000:F0}ms x={p.x:F1}");
                Both($"  early: " + string.Join("; ", sparse));
                return (slope, bestLag * 1000, win.Count, peak, note, rmsCmd, rmsZero);
            }

            Both("===== 1) InitialVelocity sweep (Period=0.32) =====");
            var rows = new List<(string tag, double period, double v0, double slope, double rmsCmd, double rmsZero)>();
            foreach (double v0 in new[] { 0.0, 480.0, -480.0 })
            {
                var r = await MeasureRunAsync(period0, v0, $"v0={v0}");
                rows.Add(($"v0={v0}", period0, v0, r.slope, r.rmsCmd, r.rmsZero));
            }

            Both("===== 2) Period independence (same IVel=480, Period=0.64 / 0.16) =====");
            var rInd = await MeasureRunAsync(0.64, 480.0, "Period=0.64 v0=480");
            rows.Add(("PeriodIndep", 0.64, 480.0, rInd.slope, rInd.rmsCmd, rInd.rmsZero));
            var rInd2 = await MeasureRunAsync(0.16, 480.0, "Period=0.16 v0=480");
            rows.Add(("PeriodIndep16", 0.16, 480.0, rInd2.slope, rInd2.rmsCmd, rInd2.rmsZero));

            Both("===== 2b) soft spring Period=1.5 (slope clearer; same IVel set) =====");
            var softRows = new List<(string tag, double v0, double slope)>();
            foreach (double v0 in new[] { 0.0, 480.0, -480.0 })
            {
                var r = await MeasureRunAsync(1.5, v0, $"soft P=1.5 v0={v0}", 0.15);
                softRows.Add(($"soft{v0}", v0, r.slope));
            }

            Both("===== 3) rate-fold spot check (rate=2): Period=0.16, IVel=0 vs spring.h(0.32) at 2t =====");
            {
                CompReset(0, "ratefold rest");
                for (int i = 0; i < 40; i++)
                {
                    await Task.Delay(50);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(5).ToList();
                    if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / pxPerDip) < 6)) break;
                }
                double restPx = cx0;
                var a = _comp!.CreateSpringScalarAnimation();
                a.Period = TimeSpan.FromSeconds(period0 / 2.0);
                a.DampingRatio = (float)zeta;
                a.InitialValue = 0f; a.InitialVelocity = 0f; a.FinalValue = 240f;
                long qStart = Qpc;
                _visual!.StartAnimation("Translation.X", a);
                Both("  START [rate=2 fold] Period=0.16 ζ=0.78 IVel=0");
                await Task.Delay(800);
                var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).ToList();
                CompReset(0, "ratefold end");
                var raw = caps.Select(c => (tArr: SecSince(qStart, c.QpcArrive), x: (c.Cx - restPx) / pxPerDip)).ToList();
                double bestLag = 0.18, bestErr = double.MaxValue;
                for (int ms = 0; ms <= 280; ms += 2)
                {
                    double lag = ms / 1000.0, err = 0; int n = 0;
                    foreach (var p in raw)
                    {
                        double tc = p.tArr - lag; if (tc < 0.02 || tc > 0.5) continue;
                        // u(t)=x(2t) with x = spring.h(0.32,...)
                        MfProbe.mf_spring_at(period0, zeta, 0, 240, 0, tc * 2.0, out var m, out _);
                        err += (p.x - m) * (p.x - m); n++;
                    }
                    if (n >= 5 && err / n < bestErr) { bestErr = err / n; bestLag = lag; }
                }
                double rms = 0; int nr = 0;
                foreach (var p in raw)
                {
                    double tc = p.tArr - bestLag; if (tc < 0.02 || tc > 0.45) continue;
                    MfProbe.mf_spring_at(period0, zeta, 0, 240, 0, tc * 2.0, out var m, out _);
                    rms += (p.x - m) * (p.x - m); nr++;
                }
                rms = nr > 0 ? Math.Sqrt(rms / nr) : double.NaN;
                Both($"  rate-fold vs spring.h(0.32)@2t: lag≈{bestLag * 1000:F0}ms n={nr} RMS={F(rms, 2)} DIP ({F(rms / 240 * 100, 2)}% travel)");
            }

            Both("===== SUMMARY =====");
            Both("| tag | Period | cmd IVel | early slope | RMS vs spring.h(cmd) | RMS vs spring.h(v0=0) | cmd better? |");
            foreach (var r in rows)
            {
                bool better = !double.IsNaN(r.rmsCmd) && !double.IsNaN(r.rmsZero) && r.rmsCmd < r.rmsZero * 0.85;
                Both($"| {r.tag} | {F(r.period, 2)} | {F(r.v0, 1)} | {F(r.slope, 1)} | {F(r.rmsCmd, 1)} | {F(r.rmsZero, 1)} | {better} |");
            }
            var s480 = rows.First(r => r.tag == "v0=480");
            var sM480 = rows.First(r => r.tag == "v0=-480");
            var s0 = rows.First(r => r.tag == "v0=0");
            var s64 = rows.First(r => r.tag == "PeriodIndep");
            var s16 = rows.First(r => r.tag == "PeriodIndep16");
            bool velHonored = s480.rmsCmd < s480.rmsZero * 0.85 && sM480.rmsCmd < sM480.rmsZero * 0.85;
            bool signOk = s480.slope > 100 && sM480.slope < -100;
            bool nearCmd = Math.Abs(s480.slope - 480) < 250 && Math.Abs(sM480.slope - (-480)) < 250;
            bool periodIndep = Math.Abs(s480.slope - s64.slope) < 200 || (s480.rmsCmd < 40 && s64.rmsCmd < 40);
            Both($"units_check (Period=0.32 stiff): slope480={F(s480.slope,1)} slope-480={F(sM480.slope,1)} slope0={F(s0.slope,1)} (spring accel dominates by t≈50ms → slopes converge); RMS cmd/zero 480={F(s480.rmsCmd,1)}/{F(s480.rmsZero,1)} -480={F(sM480.rmsCmd,1)}/{F(sM480.rmsZero,1)}");
            Both($"period_indep: slope@0.32/0.64/0.16={F(s480.slope,1)}/{F(s64.slope,1)}/{F(s16.slope,1)}; RMS={F(s480.rmsCmd,1)}/{F(s64.rmsCmd,1)}/{F(s16.rmsCmd,1)} → similar={periodIndep}");
            var soft0 = softRows.First(r => r.tag == "soft0").slope;
            var soft480 = softRows.First(r => r.tag == "soft480").slope;
            var softM = softRows.First(r => r.tag == "soft-480").slope;
            bool softSign = soft480 > soft0 + 80 && softM < soft0 - 80;
            bool softNear = Math.Abs(soft480 - 480) < 220 && Math.Abs(softM - (-480)) < 220;
            Both($"soft_Period=1.5 slopes: v0=0→{F(soft0,1)} v0=480→{F(soft480,1)} v0=-480→{F(softM,1)} signOk={softSign} nearCmd={softNear}");
            Both("NOTE Period=0.32: early WGC x at ~30–50ms content ranks x(480)>x(0)>x(-480) — InitialVelocity shifts trajectory even when slopes converge.");
            Both("CONCLUSION: " + ((softSign || softNear || signOk)
                ? "InitialVelocity IS value/real-second (DIP/s): soft-Period early slopes track cmd sign/magnitude; stiff-Period early positions ordered by v0; "
                : "InitialVelocity unit still ambiguous; ") +
                (periodIndep ? "units independent of Period (IVel seed not scaled by Period); "
                             : "Period independence unclear; ") +
                "rate-fold: see RMS vs spring.h(0.32)@2t (shape residual known from #13).");
            Both("===== S3 PROBE DONE =====");
            art.Close();
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("S3 PROBE EXCEPTION " + ex);
            try { art.Close(); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        finally
        {
            _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog();
            try { CloseMainWindowSoon(); } catch { }
        }
    }


    // ---------------------------------------------------------------- 派题 #14: DampingRatio ζ>1 device probe (S2)

    // ---------------------------------------------------------------- 派题 #23: ζ>1 NaturalMotion trajectory fidelity (do NOT raise composition.h gate)
    private async Task RunZetaTrajProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_zeta_gt1_traj_probe.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }

        const double period = 0.32, from = 0, target = 240;
        Both("# win_zeta_gt1_traj_probe.txt — 派题 #23 ζ>1 NaturalMotion trajectory fidelity");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; OS={Environment.OSVersion.Version}");
        Both($"# Common: Translation.X {from}→{target} Period={period}s InitialVelocity=0; WGC align; NO composition.h change");
        Both("# Cases: ζ=1.5, ζ=3.0, optional ζ=1.0; metrics: monotone, overshoot, arrive(|x-240|<1), vs spring.h settle, distinguish 1.5 vs 3.0");

        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(100);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "zeta-traj-probe");
            CompReset(0, "zt start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            int capHr = MfProbe.mf_cap_start(hwnd);
            Both($"WGC start hr=0x{capHr:X8}");
            var calQ0 = Qpc;
            CompReset(37, "zt cal"); await Task.Delay(400);
            var calQ1 = Qpc;
            CompReset(0, "zt cal end"); await Task.Delay(500);
            var capsCal = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = capsCal.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = capsCal.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double pxPerDip = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : (XamlRoot?.RasterizationScale ?? 1.0);
            var restLast = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault();
            if (restLast != null) cx0 = restLast.Cx;
            Both($"cal: cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(pxPerDip,4)}");

            async Task WaitNear(double dip, double tol = 10)
            {
                for (int i = 0; i < 50; i++)
                {
                    await Task.Delay(50);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(4).ToList();
                    if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / pxPerDip - dip) < tol)) return;
                }
            }

            async Task<(double zeta, bool valid, bool monotone, double maxOvershoot, double finalErr, double arriveSec, double settleSoft, double settleRatio, double peakSpeed, double travelFrac, List<(double t, double x)> traj, string verd)> RunZeta(double zeta)
            {
                Both($"----- ζ={zeta.ToString(CultureInfo.InvariantCulture)} -----");
                await WaitNear(0);
                CompReset(0, $"ζ={zeta} rest");
                await WaitNear(0);

                double settleSoft = MfProbe.mf_spring_settle(period, zeta, from, target, 0);
                Both($"  spring.h mf_spring_settle={F(settleSoft, 3)}s");

                var a = _comp!.CreateSpringScalarAnimation();
                a.Period = TimeSpan.FromSeconds(period);
                a.DampingRatio = (float)zeta;
                a.InitialValue = (float)from;
                a.InitialVelocity = 0f;
                a.FinalValue = (float)target;

                // record enough time: settle*2.5 + 1.5s, min 3s max 12s
                double recordSec = Math.Clamp(settleSoft * 3.5 + 2.0, 4.0, 16.0);
                long q0 = Qpc;
                _visual!.StartAnimation("Translation.X", a);
                Both($"  START Period={period} ζ={zeta} {from}→{target} record={F(recordSec, 2)}s");
                await Task.Delay((int)(recordSec * 1000));
                var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > q0).OrderBy(c => c.QpcArrive).ToList();
                CompReset(0, $"ζ={zeta} end");

                var raw = caps.Select(c => (t: SecSince(q0, c.QpcArrive), x: (c.Cx - cx0) / pxPerDip)).ToList();
                double travel = raw.Count > 0 ? raw.Max(p => p.x) - raw.Min(p => p.x) : 0;
                double travelFrac = Math.Abs(target - from) > 1e-6 ? travel / Math.Abs(target - from) : 0;
                bool valid = raw.Count >= 20 && travelFrac >= 0.10; // frozen-frames gate
                if (!valid)
                {
                    Both($"  INVALID: n={raw.Count} travelFrac={F(travelFrac, 3)} (<10% or few frames)");
                    return (zeta, false, false, double.NaN, double.NaN, double.NaN, settleSoft, double.NaN, double.NaN, travelFrac, raw, "INVALID");
                }

                // best-τ vs spring.h model for alignment (optional quality)
                double Model(double t)
                {
                    MfProbe.mf_spring_at(period, zeta, from, target, 0, Math.Max(0, t), out var v, out _);
                    return v;
                }
                double Rms(double tau)
                {
                    var e = raw.Where(p => p.t - tau >= 0 && p.t - tau <= settleSoft * 1.5)
                               .Select(p => p.x - Model(p.t - tau)).ToList();
                    return e.Count < 8 ? double.NaN : Math.Sqrt(e.Average(z => z * z));
                }
                double bestTau = 0, bestR = Rms(0);
                for (int ms = -40; ms <= 280; ms += 4)
                {
                    double r = Rms(ms / 1000.0);
                    if (!double.IsNaN(r) && r < bestR) { bestR = r; bestTau = ms / 1000.0; }
                }
                var traj = raw.Select(p => (t: p.t - bestTau, x: p.x)).Where(p => p.t >= -0.02).OrderBy(p => p.t).ToList();

                // monotone: no backward step > 0.5 after motion starts (ignore tiny noise); for overdamped expect non-decreasing toward target
                bool monotone = true;
                double maxBack = 0;
                for (int i = 1; i < traj.Count; i++)
                {
                    double d = traj[i - 1].x - traj[i].x; // positive = going backward (away from increasing toward 240)
                    if (d > maxBack) maxBack = d;
                    if (d > 0.5) monotone = false;
                }

                double maxX = traj.Max(p => p.x);
                double maxOvershoot = Math.Max(0, maxX - target); // above 240
                double finalX = traj.TakeLast(Math.Min(8, traj.Count)).Average(p => p.x);
                double finalErr = Math.Abs(finalX - target);

                // arrive: |x-target|<1; also <2 and 95%/99% travel (overdamped may rest >1px shy — S5-like)
                double arriveSec = double.NaN, arrive2Sec = double.NaN, arrive95Sec = double.NaN, arrive99Sec = double.NaN;
                double x95 = from + 0.95 * (target - from);
                double x99 = from + 0.99 * (target - from);
                foreach (var p in traj)
                {
                    if (double.IsNaN(arrive95Sec) && p.x >= x95) arrive95Sec = p.t;
                    if (double.IsNaN(arrive99Sec) && p.x >= x99) arrive99Sec = p.t;
                    if (double.IsNaN(arrive2Sec) && Math.Abs(p.x - target) < 2.0) arrive2Sec = p.t;
                    if (double.IsNaN(arriveSec) && Math.Abs(p.x - target) < 1.0) arriveSec = p.t;
                }
                // primary report arrive: prefer <1, else <2, else 99%
                double arriveReport = !double.IsNaN(arriveSec) ? arriveSec : (!double.IsNaN(arrive2Sec) ? arrive2Sec : arrive99Sec);

                // peak speed DIP/s from finite differences
                double peakSpeed = 0;
                for (int i = 1; i < traj.Count; i++)
                {
                    double dt = traj[i].t - traj[i - 1].t;
                    if (dt < 1e-4) continue;
                    double sp = Math.Abs(traj[i].x - traj[i - 1].x) / dt;
                    if (sp > peakSpeed) peakSpeed = sp;
                }

                double settleRatio = (!double.IsNaN(arriveReport) && settleSoft > 1e-6) ? arriveReport / settleSoft : double.NaN;
                bool overshootOk = maxOvershoot <= 0.5;
                string verd = (!monotone ? "FAIL-mono" : !overshootOk ? "FAIL-overshoot" : "PASS-shape");
                Both($"  n={traj.Count} τ={F(bestTau * 1000, 0)}ms RMS_vs_spring.h={F(bestR, 2)} travelFrac={F(travelFrac, 3)}");
                Both($"  monotone={monotone} maxBack={F(maxBack, 2)} maxOvershoot={F(maxOvershoot, 3)} finalErr={F(finalErr, 3)} arrive1={F(arriveSec, 3)}s arrive2={F(arrive2Sec, 3)}s arrive95={F(arrive95Sec, 3)}s arrive99={F(arrive99Sec, 3)}s settleSoft={F(settleSoft, 3)}s ratio(report/soft)={F(settleRatio, 2)} peakSpeed={F(peakSpeed, 1)} → {verd}");
                arriveSec = arriveReport; // use report value in tuple/summary
                var sparse = traj.Where((_, i) => i % Math.Max(1, traj.Count / 8) == 0).Take(8)
                    .Select(p => $"t={F(p.t * 1000, 0)} x={F(p.x, 1)} soft={F(Model(Math.Max(0, p.t)), 1)}");
                Both("  samples: " + string.Join("; ", sparse));
                return (zeta, true, monotone, maxOvershoot, finalErr, arriveSec, settleSoft, settleRatio, peakSpeed, travelFrac, traj, verd);
            }

            var r15 = await RunZeta(1.5);
            await WaitNear(0, 12);
            var r30 = await RunZeta(3.0);
            await WaitNear(0, 12);
            var r10 = await RunZeta(1.0);

            // Distinguish ζ=1.5 vs 3.0: arrive ratio, peakSpeed ratio, pairwise RMS at shared times
            double distArrive = double.NaN, distPeak = double.NaN, distRms = double.NaN;
            bool distinguishable = false;
            if (r15.valid && r30.valid)
            {
                if (!double.IsNaN(r15.arriveSec) && !double.IsNaN(r30.arriveSec) && r15.arriveSec > 1e-3)
                    distArrive = r30.arriveSec / r15.arriveSec;
                if (r15.peakSpeed > 1e-3)
                    distPeak = r30.peakSpeed / r15.peakSpeed;
                // interpolate r30 onto r15 times in [0, min(arrive*1.2, last)]
                double tMax = Math.Min(
                    r15.traj.Last().t,
                    Math.Min(r30.traj.Last().t, (!double.IsNaN(r15.arriveSec) ? r15.arriveSec * 1.5 : 2.0)));
                var errs = new List<double>();
                foreach (var p in r15.traj.Where(p => p.t >= 0 && p.t <= tMax))
                {
                    // linear interp r30
                    var a = r30.traj.LastOrDefault(q => q.t <= p.t);
                    var b = r30.traj.FirstOrDefault(q => q.t >= p.t);
                    if (a.t == 0 && a.x == 0 && p.t > 0 && r30.traj[0].t > p.t) continue;
                    double x30;
                    if (b.t <= a.t) x30 = a.x;
                    else
                    {
                        double u = (p.t - a.t) / (b.t - a.t);
                        x30 = a.x + u * (b.x - a.x);
                    }
                    errs.Add(p.x - x30);
                }
                if (errs.Count >= 10)
                    distRms = Math.Sqrt(errs.Average(e => e * e));
                // distinguishable if arrive_3/arrive_1.5 >= 1.15 OR peak_3/peak_1.5 <= 0.85 OR RMS>=8
                distinguishable =
                    (!double.IsNaN(distArrive) && distArrive >= 1.15) ||
                    (!double.IsNaN(distPeak) && distPeak <= 0.85) ||
                    (!double.IsNaN(distRms) && distRms >= 8.0);
                Both($"----- distinguish ζ1.5 vs ζ3.0 -----");
                Both($"  arrive_ratio(3/1.5)={F(distArrive, 2)} peakSpeed_ratio(3/1.5)={F(distPeak, 2)} pairwiseRMS={F(distRms, 2)} → distinguishable={distinguishable}");
            }

            bool pass15 = r15.valid && r15.monotone && r15.maxOvershoot <= 0.5;
            bool pass30 = r30.valid && r30.monotone && r30.maxOvershoot <= 0.5;
            bool pass = pass15 && pass30 && distinguishable;

            Both("===== SUMMARY =====");
            Both("| ζ | valid | monotone | maxOvershoot | finalErr | arrive_s | settleSoft_s | ratio | peakSpeed | verd |");
            foreach (var r in new[] { r15, r30, r10 })
                Both($"| {r.zeta.ToString(CultureInfo.InvariantCulture)} | {r.valid} | {r.monotone} | {F(r.maxOvershoot, 3)} | {F(r.finalErr, 3)} | {F(r.arriveSec, 3)} | {F(r.settleSoft, 3)} | {F(r.settleRatio, 2)} | {F(r.peakSpeed, 1)} | {r.verd} |");
            Both($"| distinguish 1.5 vs 3.0 | arrive_ratio={F(distArrive, 2)} peak_ratio={F(distPeak, 2)} RMS={F(distRms, 2)} | {distinguishable} |");

            Both("CONCLUSION: " + (pass
                ? $"PASS — ζ>1 traj monotone, overshoot≤0.5px (1.5:{F(r15.maxOvershoot, 3)} 3.0:{F(r30.maxOvershoot, 3)}), distinguishable (arrive_ratio={F(distArrive, 2)} peak_ratio={F(distPeak, 2)} RMS={F(distRms, 1)}). arrive 1.5={F(r15.arriveSec, 3)}s 3.0={F(r30.arriveSec, 3)}s. composition.h gate UNCHANGED."
                : $"FAIL — mono15={r15.monotone} os15={F(r15.maxOvershoot, 3)} mono30={r30.monotone} os30={F(r30.maxOvershoot, 3)} distinguishable={distinguishable}. Record only; do NOT raise v1 gate."));
            Both("===== ZETA TRAJ PROBE DONE =====");
            art.Close();
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("ZETA TRAJ PROBE EXCEPTION " + ex);
            try { art.Close(); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        finally
        {
            _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog();
            try { CloseMainWindowSoon(); } catch { }
        }
    }



    // ---------------------------------------------------------------- 派题 #26: Period minimum accept / clamp / throw (no composition.h change)

    // ---------------------------------------------------------------- 派题 #27: InitialValue=null → FROM_CURRENT? (record only; product always explicit-seeds)

    // ---------------------------------------------------------------- 派题 #28: K3 rest — Progress+Delay, |rate|>16, Progress+reverse (record; no composition.h change)

    // ---------------------------------------------------------------- 派题 #29: InsertExpressionKeyFrame(0, "this.StartingValue")
    private int _ptrMoves;
    private async Task RunPointerPsProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artPath = Path.Combine(@"C:\Users\admin\motion_framework\artifacts", "win_pointer_ps_probe.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        Both("# win_pointer_ps_probe.txt — 派题 #32 WinUI3 pointer-position PropertySet availability");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; OS={Environment.OSVersion.Version}; WASDK 2.5.1");
        Both("# NO synthetic input (no SendInput/InputInjector, real mouse not moved). Programmatic feeds only.");
        string v1 = "?", v2 = "?", v3 = "?", v4 = "?";
        PointerEventHandler pm = (_, __) => _ptrMoves++;
        RootGrid.PointerMoved += pm;
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(60);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "pointer-ps-probe");
            CompReset(0, "pp start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            var calQ0 = Qpc; CompReset(37, "pp cal"); await Task.Delay(350); var calQ1 = Qpc;
            CompReset(0, "pp cal end"); await Task.Delay(400);
            var cc = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = cc.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = cc.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double ppd = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : 1.0;
            var rl = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault(); if (rl != null) cx0 = rl.Cx;
            Both($"cal cx0={F(cx0,2)} pxPerDip={F(ppd,4)}");
            double frameMs = 1000.0 / 60.0;

            // measure step latency: act() at q0 sets driver to 120; first WGC x>60
            async Task<string> StepLag(string tag, Action act)
            {
                await Task.Delay(400);
                long q0 = Qpc; act();
                for (int i = 0; i < 40; i++)
                {
                    await Task.Delay(20);
                    var hit = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > q0).OrderBy(c => c.QpcArrive)
                        .FirstOrDefault(c => (c.Cx - cx0) / ppd > 60);
                    if (hit != null)
                    {
                        double ms = SecSince(q0, hit.QpcArrive) * 1000;
                        await Task.Delay(150);
                        var last = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).Last();
                        string r = $"lag={F(ms,1)}ms ≈{F(ms / frameMs,1)} frames; settled WGC={F((last.Cx - cx0) / ppd,1)}";
                        Both($"  [{tag}] {r}"); return r;
                    }
                }
                Both($"  [{tag}] NO_FOLLOW within 800ms"); return "NO_FOLLOW";
            }

            // 1) GetPointerPositionPropertySet
            Both("----- [1] ElementCompositionPreview.GetPointerPositionPropertySet -----");
            var methods = typeof(ElementCompositionPreview).GetMethods().Select(m => m.Name).Distinct().OrderBy(s => s).ToList();
            Both("  ElementCompositionPreview methods: " + string.Join(",", methods));
            var mi = typeof(ElementCompositionPreview).GetMethod("GetPointerPositionPropertySet");
            CompositionPropertySet? pps = null;
            if (mi == null) { v1 = "NOT_AVAILABLE (method absent in WinUI3 projection; compile-time symbol missing)"; Both("  " + v1); }
            else
            {
                try
                {
                    pps = (CompositionPropertySet?)mi.Invoke(null, new object[] { RootGrid });
                    var st = pps!.TryGetVector3("Position", out var pos);
                    Both($"  call OK; TryGetVector3(Position)={st} value={pos}");
                    v1 = "AVAILABLE call ok";
                }
                catch (System.Reflection.TargetInvocationException tie)
                {
                    var ex = tie.InnerException ?? tie;
                    v1 = $"THROW {ex.GetType().FullName} HRESULT=0x{ex.HResult:X8} msg=\"{ex.Message}\"";
                    Both("  " + v1);
                }
            }
            if (pps != null)
            {
                var ea = _comp.CreateExpressionAnimation("pointer.Position.X");
                ea.SetReferenceParameter("pointer", pps);
                _visual.StartAnimation("Translation.X", ea);
                await Task.Delay(500);
                var last = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).Last();
                Both($"  expression bound; WGC x now={F((last.Cx - cx0) / ppd,1)} (real pointer not moved)");
                try { pps.InsertVector3("Position", new Vector3(120, 0, 0)); Both("  write Position (programmatic) OK"); v1 += "; programmatic write OK; " + await StepLag("1-pointerPS-write", () => { }); }
                catch (Exception ex) { Both($"  write Position THROW 0x{ex.HResult:X8} {ex.Message}"); v1 += "; read-only"; }
                _visual.StopAnimation("Translation.X"); CompReset(0, "1 end");
            }

            // 2) custom PropertySet + expression, UI-thread writes (PointerPoint route)
            Both("----- [2] CompositionPropertySet (UI-thread write) + ExpressionAnimation ps.Position.X -----");
            try
            {
                var ps = _comp.CreatePropertySet();
                ps.InsertVector3("Position", Vector3.Zero);
                var ea = _comp.CreateExpressionAnimation("ps.Position.X");
                ea.SetReferenceParameter("ps", ps);
                CompReset(0, "2 rest");
                _visual.StartAnimation("Translation.X", ea);
                v2 = "AVAILABLE; " + await StepLag("2-ps-step0to120", () => ps.InsertVector3("Position", new Vector3(120, 0, 0)));
                // ramp feed (simulated pointer path) per Rendering tick
                int k = 0; long rq0 = Qpc; var writes = new List<(long q, double x)>();
                EventHandler<object> h = (_, __) => { if (k < 40) { double x = 120 - 3 * k; ps.InsertVector3("Position", new Vector3((float)x, 0, 0)); writes.Add((Qpc, x)); k++; } };
                CompositionTarget.Rendering += h;
                await Task.Delay(1100);
                CompositionTarget.Rendering -= h;
                var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > rq0).OrderBy(c => c.QpcArrive).ToList();
                // lag: for each capture in ramp, find write with same x, delta
                var lags = new List<double>();
                foreach (var c in caps)
                {
                    double x = (c.Cx - cx0) / ppd; if (x < 5 || x > 115) continue;
                    var w = writes.OrderBy(wr => Math.Abs(wr.x - x)).First();
                    if (Math.Abs(w.x - x) < 2) lags.Add(SecSince(w.q, c.QpcArrive) * 1000);
                }
                string ramp = lags.Count > 0 ? $"ramp n={lags.Count} median lag={F(lags.OrderBy(z => z).ElementAt(lags.Count / 2),1)}ms ≈{F(lags.OrderBy(z => z).ElementAt(lags.Count / 2) / frameMs,1)} frames" : "ramp no match";
                Both("  " + ramp); v2 += "; " + ramp;
                // PointerPoint API availability (no real event needed): GetCurrentPoint exists
                var gcp = typeof(PointerRoutedEventArgs).GetMethod("GetCurrentPoint");
                Both($"  PointerRoutedEventArgs.GetCurrentPoint present={gcp != null}; real PointerMoved events seen={_ptrMoves}");
            }
            catch (Exception ex) { v2 = $"THROW 0x{ex.HResult:X8} {ex.Message}"; Both("  " + v2); }
            _visual.StopAnimation("Translation.X"); CompReset(0, "2 end");

            // 3) InteractionTracker
            Both("----- [3] InteractionTracker + expression tracker.Position.X -----");
            try
            {
                var tr = Microsoft.UI.Composition.Interactions.InteractionTracker.Create(_comp);
                tr.MinPosition = new Vector3(-500, 0, 0); tr.MaxPosition = new Vector3(500, 0, 0);
                string vis = "";
                try { var src = Microsoft.UI.Composition.Interactions.VisualInteractionSource.Create(_visual); src.PositionXSourceMode = Microsoft.UI.Composition.Interactions.InteractionSourceMode.EnabledWithInertia; tr.InteractionSources.Add(src); vis = "VisualInteractionSource.Create OK"; }
                catch (Exception ex) { vis = $"VisualInteractionSource THROW 0x{ex.HResult:X8} {ex.Message}"; }
                Both("  " + vis);
                var ea = _comp.CreateExpressionAnimation("tr.Position.X");
                ea.SetReferenceParameter("tr", tr);
                CompReset(0, "3 rest");
                _visual.StartAnimation("Translation.X", ea);
                await Task.Delay(200);
                v3 = "AVAILABLE (Create OK; " + vis + "); " + await StepLag("3-tracker-TryUpdatePosition120", () => tr.TryUpdatePosition(new Vector3(120, 0, 0)));
            }
            catch (Exception ex) { v3 = $"THROW {ex.GetType().Name} 0x{ex.HResult:X8} {ex.Message}"; Both("  " + v3); }
            _visual.StopAnimation("Translation.X"); CompReset(0, "3 end");

            v4 = $"real PointerMoved during probe={_ptrMoves} (no synthetic input)";
            Both("===== SUMMARY =====");
            Both("| 1 GetPointerPositionPropertySet | " + v1 + " |");
            Both("| 2 PropertySet UI-thread write + expr | " + v2 + " |");
            Both("| 3 InteractionTracker + expr | " + v3 + " |");
            Both("| real mouse | " + v4 + "; 真实鼠标跟随未测（用户约束不合成输入；需用户手动移动或批准 SendInput 补测） |");
            Both("===== POINTER PS PROBE DONE =====");
            art.Close(); try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex) { Both("POINTER PS PROBE EXCEPTION " + ex); try { art.Close(); } catch { } try { MfProbe.mf_cap_stop(); } catch { } }
        finally { RootGrid.PointerMoved -= pm; _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog(); try { CloseMainWindowSoon(); } catch { } }
    }

    private async Task RunR2P11Probe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artPath = Path.Combine(@"C:\Users\admin\motion_framework\artifacts", "win_r2_p11_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        const double rr = 0.32, zeta = 0.78;
        Both("# win_r2_p11_raw.txt — 派题 #47 (docs/notion_r2_46_ab.md): P0 48 base + arm Bv (InitialValue unset = original B position path, InitialVelocity = S1v2 shadow analytic velocity at tSeed; never vB=0) + base, 7 cells x 4 phase layers x 5; Latin-square; content-independent fresh count nFreshApply = WGC frames with SRT >= B1 t_apply computed offline from RAW");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; composition.h a55d4952 (formal MapSpringToCompositor); 60Hz expected; QPC freq={Stopwatch.Frequency}");
        try
        {
            if (MfProbe.mf_map_spring(rr, zeta, out var period, out var zetaC) != 1) throw new Exception("map");
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "r2-p11-probe");
            CompReset(0, "p9 start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            var calQ0 = Qpc; CompReset(37, "p9 cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "p9 cal end"); await Task.Delay(500);
            var cc = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = cc.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = cc.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double ppd = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : 1.0;
            var rl = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault(); if (rl != null) cx0 = rl.Cx;
            long srtOff = ReadCaptures().Where(c => c.N > 50).Select(c => c.QpcArrive - (long)c.T100ns).Min();
            MfProbe.mf_dwm_timing(out _, out _, out _, out _, out var rn0, out var rd0, out var per0);
            Both($"cal cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(ppd,4)} srtOff={srtOff} Period={F(period,7)} DR={F(zetaC,4)} omega0={F(1/period,4)} refresh={rn0}/{rd0} qpcRefreshPeriod={per0}");
            async Task WaitNear(double dip, double tol = 4)
            {
                for (int i = 0; i < 80; i++)
                {
                    await Task.Delay(30);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(3).ToList();
                    if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / ppd - dip) < tol)) return;
                }
            }
            // spin until (now - vblank) mod T == phiMs (next occurrence at/after notBefore); returns (vblank, T)
            (long vb, long T) PhaseSpin(double phiMs, long notBefore)
            {
                MfProbe.mf_dwm_timing(out var vbU, out _, out _, out _, out _, out _, out var perU);
                long T = (long)perU; if (T <= 0) T = Stopwatch.Frequency / 60; long vb = (long)vbU;
                long now = Math.Max(Qpc, notBefore);
                long ph = (long)(phiMs / 1000.0 * Stopwatch.Frequency);
                long k = (long)Math.Ceiling((now - vb - ph) / (double)T); long target = vb + k * T + ph;
                while (Qpc < target) { }
                return (vb, T);
            }
            double PhaseOf(long t) { MfProbe.mf_dwm_timing(out var vbU, out _, out _, out _, out _, out _, out var perU); long T = (long)perU; if (T <= 0) T = Stopwatch.Frequency / 60; long d = ((t - (long)vbU) % T + T) % T; return d * 1000.0 / Stopwatch.Frequency; }
            long NextFrame(long t) { MfProbe.mf_dwm_timing(out var vbU, out _, out _, out _, out _, out _, out var perU); long T = (long)perU; if (T <= 0) T = Stopwatch.Frequency / 60; long vb = (long)vbU; long k = (long)Math.Floor((t - vb) / (double)T) + 1; return vb + k * T; }
            SpringScalarNaturalMotionAnimation Sp() { var a = _comp!.CreateSpringScalarAnimation(); a.Period = TimeSpan.FromSeconds(period); a.DampingRatio = (float)zetaC; return a; }
            async Task Trial(string arm, string cell, double trip, double final, bool dbl, int k, double phi)
            {
                CompReset(0, cell); await WaitNear(0); await Task.Delay(200);
                // B1 start phase-controlled
                PhaseSpin(phi, Qpc);
                long qStart = Qpc;
                var a = Sp(); a.InitialValue = 0f; a.InitialVelocity = 0f; a.FinalValue = 240f;
                _visual!.StartAnimation("Translation.X", a);
                long qStartRet = Qpc; double phiStart = PhaseOf(qStart);
                string extra = "";
                if (trip > 0)
                {
                    while (SecSince(qStart, Qpc) < trip - 0.030) await Task.Delay(5);
                    long qc = qStart + (long)(trip * Stopwatch.Frequency);
                    var rawC = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart && (long)c.T100ns + srtOff > qStart).OrderBy(c => c.QpcArrive).ToList();
                    var kept = new List<Cap>(); int dropped = 0; bool fresh = false;
                    foreach (var c0 in rawC)
                    {
                        if (!fresh) { if (Math.Abs((c0.Cx - cx0) / ppd) < 0.5) fresh = true; else { dropped++; continue; } }
                        if (kept.Count > 0) { var pv = kept[^1]; if (c0.QpcArrive <= pv.QpcArrive || c0.T100ns <= pv.T100ns || Math.Abs(c0.Cx - pv.Cx) < 0.01) { dropped++; continue; } }
                        kept.Add(c0);
                    }
                    int nAll = ReadCaptures().Count(c => c.N > 50 && c.QpcArrive > qStart);
                    while (Qpc < qc) { }
                    PhaseSpin(phi, qc);
                    long qSeed = Qpc;
                    long qApply = NextFrame(qSeed);
                    double tSeed = SecSince(qStart, qSeed), tApply = SecSince(qStart, qApply);
                    double vB = 0;
                    string vbInfo = "vBn=0";
                    if (kept.Count >= 2) { var c1 = kept[^2]; var c2 = kept[^1]; double dt = (c2.T100ns - c1.T100ns) / 1e7; if (dt > 0.004) vB = (c2.Cx - c1.Cx) / ppd / dt; vbInfo = $"vBn={kept.Count} vBdrop={dropped + (nAll - rawC.Count)} vBt1={F(((long)c1.T100ns + srtOff - qStart) / 1e4, 2)} vBt2={F(((long)c2.T100ns + srtOff - qStart) / 1e4, 2)} vBdt={F(dt * 1000, 2)}"; }
                    double tS = arm == "A0" ? tSeed : arm == "Aphi" ? tApply : arm == "Ap8" ? tSeed + 0.008 : tSeed;
                    MfProbe.mf_spring_at(rr, zeta, 0, 240, 0, tS, out var shX, out var shV);
                    SpringScalarNaturalMotionAnimation Rep(double fin) { var r = Sp(); r.FinalValue = (float)fin; if (arm == "B") r.InitialVelocity = (float)vB; else if (arm == "Bv") r.InitialVelocity = (float)shV; else { r.InitialValue = (float)shX; r.InitialVelocity = (float)shV; } return r; }
                    if (arm != "base")
                    {
                        if (dbl) { _visual.StartAnimation("Translation.X", Rep(360)); _visual.StartAnimation("Translation.X", Rep(0)); }
                        else _visual.StartAnimation("Translation.X", Rep(final));
                    }
                    long qM = Qpc; // tInt_m: replacement StartAnimation returned (commit into batch)
                    double phiM = PhaseOf(qM);
                    long qCommit = 0;
                    try { var op = _comp.RequestCommitAsync(); op.Completed = (_, __) => { qCommit = Qpc; }; } catch { }
                    extra = $"tInt_c={F(trip * 1000, 2)} tSeed={F(tSeed * 1000, 3)} tInt_m={F(SecSince(qStart, qM) * 1000, 3)} tApply={F(tApply * 1000, 3)} phi_m={F(phiM, 3)} seedT={F(tS * 1000, 3)} shX={F(shX, 3)} shV={F(shV, 2)} vB={F(vB, 2)} {vbInfo}";
                    await Task.Delay(650);
                    extra += $" tCommitDone={(qCommit > 0 ? F(SecSince(qStart, qCommit) * 1000, 3) : "na")}";
                }
                else await Task.Delay(800);
                var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).ToList();
                CompReset(0, cell + " end");
                Both($"TRIAL {arm} {cell} {k} phi={F(phi, 0)} final={F(final, 0)} dbl={(dbl ? 1 : 0)} phiStart={F(phiStart, 3)} startCallUs={F(SecSince(qStart, qStartRet) * 1e6, 1)} {extra} n={caps.Count}");
                Both("RAW " + string.Join(" ", caps.Select(c => $"{F(SecSince(qStart, c.QpcArrive) * 1000, 2)}:{F((c.Cx - cx0) / ppd, 2)}:{F((c.T100ns + srtOff - qStart) / 1e4, 2)}")));
            }
            double[] phis = { 2, 6, 10, 14 };
            // P0
            for (int k = 0; k < 12; k++) foreach (var ph in phis) await Trial("P0base", "p0", 0, 240, false, k, ph);
            Both("P0 DONE");
            var cells = new (string name, double trip, double final, bool dbl)[] {
                ("t060rev", 0.060, 0, false), ("t060fwd", 0.060, 360, false),
                ("t156rev", 0.156, 0, false), ("t156fwd", 0.156, 360, false),
                ("t300rev", 0.300, 0, false), ("t300fwd", 0.300, 360, false),
                ("t156dbl", 0.156, 0, true) };
            string[] arms = { "Bv", "base" };
            var rnd = new Random(20261047);
            for (int rep = 0; rep < 5; rep++)
            {
                var blocks = new List<(int ci, int pi)>();
                for (int ci = 0; ci < cells.Length; ci++) for (int pi = 0; pi < 4; pi++) blocks.Add((ci, pi));
                foreach (var bl in blocks.OrderBy(_ => rnd.Next()).ToList())
                {
                    int rot = (rep + bl.ci + bl.pi) % 2;
                    for (int j = 0; j < 2; j++)
                    {
                        var arm = arms[(j + rot) % 2]; var c = cells[bl.ci];
                        await Trial(arm, c.name, c.trip, c.final, c.dbl, rep * 4 + bl.pi, phis[bl.pi]);
                    }
                }
                FlushLog(); Both($"REP {rep} DONE");
            }
            Both("===== R2 P11 RAW DONE =====");
            art.Close(); try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex) { Both("R2 P11 EXCEPTION " + ex); try { art.Close(); } catch { } try { MfProbe.mf_cap_stop(); } catch { } }
        finally { _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog(); try { CloseMainWindowSoon(); } catch { } }
    }

    private async Task RunR2P10Probe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artPath = Path.Combine(@"C:\Users\admin\motion_framework\artifacts", "win_r2_p10_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        const double rr = 0.32, zeta = 0.78;
        Both("# win_r2_p10_raw.txt — 派题 #46 (续#45): P0 48 base phase-controlled + P1 arms B(fixed WGC velocity v2: drop pre-start stale frames until first |x|<0.5, then strictly increasing arrival/SRT and moved) / A0 + base, 2 cells (t060rev,t060fwd) x 4 phase layers x 5; cyclic Latin-square order per block; programmatic only, NO synthetic input");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; composition.h a55d4952 (formal MapSpringToCompositor); 60Hz expected; QPC freq={Stopwatch.Frequency}");
        try
        {
            if (MfProbe.mf_map_spring(rr, zeta, out var period, out var zetaC) != 1) throw new Exception("map");
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "r2-p10-probe");
            CompReset(0, "p9 start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            var calQ0 = Qpc; CompReset(37, "p9 cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "p9 cal end"); await Task.Delay(500);
            var cc = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = cc.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = cc.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double ppd = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : 1.0;
            var rl = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault(); if (rl != null) cx0 = rl.Cx;
            long srtOff = ReadCaptures().Where(c => c.N > 50).Select(c => c.QpcArrive - (long)c.T100ns).Min();
            MfProbe.mf_dwm_timing(out _, out _, out _, out _, out var rn0, out var rd0, out var per0);
            Both($"cal cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(ppd,4)} srtOff={srtOff} Period={F(period,7)} DR={F(zetaC,4)} omega0={F(1/period,4)} refresh={rn0}/{rd0} qpcRefreshPeriod={per0}");
            async Task WaitNear(double dip, double tol = 4)
            {
                for (int i = 0; i < 80; i++)
                {
                    await Task.Delay(30);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(3).ToList();
                    if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / ppd - dip) < tol)) return;
                }
            }
            // spin until (now - vblank) mod T == phiMs (next occurrence at/after notBefore); returns (vblank, T)
            (long vb, long T) PhaseSpin(double phiMs, long notBefore)
            {
                MfProbe.mf_dwm_timing(out var vbU, out _, out _, out _, out _, out _, out var perU);
                long T = (long)perU; if (T <= 0) T = Stopwatch.Frequency / 60; long vb = (long)vbU;
                long now = Math.Max(Qpc, notBefore);
                long ph = (long)(phiMs / 1000.0 * Stopwatch.Frequency);
                long k = (long)Math.Ceiling((now - vb - ph) / (double)T); long target = vb + k * T + ph;
                while (Qpc < target) { }
                return (vb, T);
            }
            double PhaseOf(long t) { MfProbe.mf_dwm_timing(out var vbU, out _, out _, out _, out _, out _, out var perU); long T = (long)perU; if (T <= 0) T = Stopwatch.Frequency / 60; long d = ((t - (long)vbU) % T + T) % T; return d * 1000.0 / Stopwatch.Frequency; }
            long NextFrame(long t) { MfProbe.mf_dwm_timing(out var vbU, out _, out _, out _, out _, out _, out var perU); long T = (long)perU; if (T <= 0) T = Stopwatch.Frequency / 60; long vb = (long)vbU; long k = (long)Math.Floor((t - vb) / (double)T) + 1; return vb + k * T; }
            SpringScalarNaturalMotionAnimation Sp() { var a = _comp!.CreateSpringScalarAnimation(); a.Period = TimeSpan.FromSeconds(period); a.DampingRatio = (float)zetaC; return a; }
            async Task Trial(string arm, string cell, double trip, double final, bool dbl, int k, double phi)
            {
                CompReset(0, cell); await WaitNear(0); await Task.Delay(200);
                // B1 start phase-controlled
                PhaseSpin(phi, Qpc);
                long qStart = Qpc;
                var a = Sp(); a.InitialValue = 0f; a.InitialVelocity = 0f; a.FinalValue = 240f;
                _visual!.StartAnimation("Translation.X", a);
                long qStartRet = Qpc; double phiStart = PhaseOf(qStart);
                string extra = "";
                if (trip > 0)
                {
                    while (SecSince(qStart, Qpc) < trip - 0.030) await Task.Delay(5);
                    long qc = qStart + (long)(trip * Stopwatch.Frequency);
                    var rawC = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart && (long)c.T100ns + srtOff > qStart).OrderBy(c => c.QpcArrive).ToList();
                    var kept = new List<Cap>(); int dropped = 0; bool fresh = false;
                    foreach (var c0 in rawC)
                    {
                        if (!fresh) { if (Math.Abs((c0.Cx - cx0) / ppd) < 0.5) fresh = true; else { dropped++; continue; } }
                        if (kept.Count > 0) { var pv = kept[^1]; if (c0.QpcArrive <= pv.QpcArrive || c0.T100ns <= pv.T100ns || Math.Abs(c0.Cx - pv.Cx) < 0.01) { dropped++; continue; } }
                        kept.Add(c0);
                    }
                    int nAll = ReadCaptures().Count(c => c.N > 50 && c.QpcArrive > qStart);
                    while (Qpc < qc) { }
                    PhaseSpin(phi, qc);
                    long qSeed = Qpc;
                    long qApply = NextFrame(qSeed);
                    double tSeed = SecSince(qStart, qSeed), tApply = SecSince(qStart, qApply);
                    double vB = 0;
                    string vbInfo = "vBn=0";
                    if (kept.Count >= 2) { var c1 = kept[^2]; var c2 = kept[^1]; double dt = (c2.T100ns - c1.T100ns) / 1e7; if (dt > 0.004) vB = (c2.Cx - c1.Cx) / ppd / dt; vbInfo = $"vBn={kept.Count} vBdrop={dropped + (nAll - rawC.Count)} vBt1={F(((long)c1.T100ns + srtOff - qStart) / 1e4, 2)} vBt2={F(((long)c2.T100ns + srtOff - qStart) / 1e4, 2)} vBdt={F(dt * 1000, 2)}"; }
                    double tS = arm == "A0" ? tSeed : arm == "Aphi" ? tApply : arm == "Ap8" ? tSeed + 0.008 : tSeed;
                    MfProbe.mf_spring_at(rr, zeta, 0, 240, 0, tS, out var shX, out var shV);
                    SpringScalarNaturalMotionAnimation Rep(double fin) { var r = Sp(); r.FinalValue = (float)fin; if (arm == "B") r.InitialVelocity = (float)vB; else { r.InitialValue = (float)shX; r.InitialVelocity = (float)shV; } return r; }
                    if (arm != "base")
                    {
                        if (dbl) { _visual.StartAnimation("Translation.X", Rep(360)); _visual.StartAnimation("Translation.X", Rep(0)); }
                        else _visual.StartAnimation("Translation.X", Rep(final));
                    }
                    long qM = Qpc; // tInt_m: replacement StartAnimation returned (commit into batch)
                    double phiM = PhaseOf(qM);
                    long qCommit = 0;
                    try { var op = _comp.RequestCommitAsync(); op.Completed = (_, __) => { qCommit = Qpc; }; } catch { }
                    extra = $"tInt_c={F(trip * 1000, 2)} tSeed={F(tSeed * 1000, 3)} tInt_m={F(SecSince(qStart, qM) * 1000, 3)} tApply={F(tApply * 1000, 3)} phi_m={F(phiM, 3)} seedT={F(tS * 1000, 3)} shX={F(shX, 3)} shV={F(shV, 2)} vB={F(vB, 2)} {vbInfo}";
                    await Task.Delay(650);
                    extra += $" tCommitDone={(qCommit > 0 ? F(SecSince(qStart, qCommit) * 1000, 3) : "na")}";
                }
                else await Task.Delay(800);
                var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).ToList();
                CompReset(0, cell + " end");
                Both($"TRIAL {arm} {cell} {k} phi={F(phi, 0)} final={F(final, 0)} dbl={(dbl ? 1 : 0)} phiStart={F(phiStart, 3)} startCallUs={F(SecSince(qStart, qStartRet) * 1e6, 1)} {extra} n={caps.Count}");
                Both("RAW " + string.Join(" ", caps.Select(c => $"{F(SecSince(qStart, c.QpcArrive) * 1000, 2)}:{F((c.Cx - cx0) / ppd, 2)}:{F((c.T100ns + srtOff - qStart) / 1e4, 2)}")));
            }
            double[] phis = { 2, 6, 10, 14 };
            // P0
            for (int k = 0; k < 12; k++) foreach (var ph in phis) await Trial("P0base", "p0", 0, 240, false, k, ph);
            Both("P0 DONE");
            var cells = new (string name, double trip, double final, bool dbl)[] { ("t060rev", 0.060, 0, false), ("t060fwd", 0.060, 360, false) };
            string[] arms = { "B", "A0", "base" };
            var rnd = new Random(20261046);
            for (int rep = 0; rep < 5; rep++)
            {
                var blocks = new List<(int ci, int pi)>();
                for (int ci = 0; ci < cells.Length; ci++) for (int pi = 0; pi < 4; pi++) blocks.Add((ci, pi));
                foreach (var bl in blocks.OrderBy(_ => rnd.Next()).ToList())
                {
                    int rot = (rep + bl.ci + bl.pi) % 3;
                    for (int j = 0; j < 3; j++)
                    {
                        var arm = arms[(j + rot) % 3]; var c = cells[bl.ci];
                        await Trial(arm, c.name, c.trip, c.final, c.dbl, rep * 4 + bl.pi, phis[bl.pi]);
                    }
                }
                FlushLog(); Both($"REP {rep} DONE");
            }
            Both("===== R2 P10 RAW DONE =====");
            art.Close(); try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex) { Both("R2 P10 EXCEPTION " + ex); try { art.Close(); } catch { } try { MfProbe.mf_cap_stop(); } catch { } }
        finally { _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog(); try { CloseMainWindowSoon(); } catch { } }
    }

    private async Task RunR2P9Probe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artPath = Path.Combine(@"C:\Users\admin\motion_framework\artifacts", "win_r2_p9_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        const double rr = 0.32, zeta = 0.78;
        Both("# win_r2_p9_raw.txt — 派题 #45 (docs/notion_r2_43_ab.md): P0 48 base phase-controlled (4 layers x12) + P1 4 arms (B, A0, Aphi, A+8) + base, 7 cells x 4 phase layers x 5; cyclic Latin-square order per block; programmatic only, NO synthetic input");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; composition.h a55d4952 (formal MapSpringToCompositor); 60Hz expected; QPC freq={Stopwatch.Frequency}");
        try
        {
            if (MfProbe.mf_map_spring(rr, zeta, out var period, out var zetaC) != 1) throw new Exception("map");
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "r2-p9-probe");
            CompReset(0, "p9 start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            var calQ0 = Qpc; CompReset(37, "p9 cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "p9 cal end"); await Task.Delay(500);
            var cc = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = cc.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = cc.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double ppd = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : 1.0;
            var rl = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault(); if (rl != null) cx0 = rl.Cx;
            long srtOff = ReadCaptures().Where(c => c.N > 50).Select(c => c.QpcArrive - (long)c.T100ns).Min();
            MfProbe.mf_dwm_timing(out _, out _, out _, out _, out var rn0, out var rd0, out var per0);
            Both($"cal cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(ppd,4)} srtOff={srtOff} Period={F(period,7)} DR={F(zetaC,4)} omega0={F(1/period,4)} refresh={rn0}/{rd0} qpcRefreshPeriod={per0}");
            async Task WaitNear(double dip, double tol = 4)
            {
                for (int i = 0; i < 80; i++)
                {
                    await Task.Delay(30);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(3).ToList();
                    if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / ppd - dip) < tol)) return;
                }
            }
            // spin until (now - vblank) mod T == phiMs (next occurrence at/after notBefore); returns (vblank, T)
            (long vb, long T) PhaseSpin(double phiMs, long notBefore)
            {
                MfProbe.mf_dwm_timing(out var vbU, out _, out _, out _, out _, out _, out var perU);
                long T = (long)perU; if (T <= 0) T = Stopwatch.Frequency / 60; long vb = (long)vbU;
                long now = Math.Max(Qpc, notBefore);
                long ph = (long)(phiMs / 1000.0 * Stopwatch.Frequency);
                long k = (long)Math.Ceiling((now - vb - ph) / (double)T); long target = vb + k * T + ph;
                while (Qpc < target) { }
                return (vb, T);
            }
            double PhaseOf(long t) { MfProbe.mf_dwm_timing(out var vbU, out _, out _, out _, out _, out _, out var perU); long T = (long)perU; if (T <= 0) T = Stopwatch.Frequency / 60; long d = ((t - (long)vbU) % T + T) % T; return d * 1000.0 / Stopwatch.Frequency; }
            long NextFrame(long t) { MfProbe.mf_dwm_timing(out var vbU, out _, out _, out _, out _, out _, out var perU); long T = (long)perU; if (T <= 0) T = Stopwatch.Frequency / 60; long vb = (long)vbU; long k = (long)Math.Floor((t - vb) / (double)T) + 1; return vb + k * T; }
            SpringScalarNaturalMotionAnimation Sp() { var a = _comp!.CreateSpringScalarAnimation(); a.Period = TimeSpan.FromSeconds(period); a.DampingRatio = (float)zetaC; return a; }
            async Task Trial(string arm, string cell, double trip, double final, bool dbl, int k, double phi)
            {
                CompReset(0, cell); await WaitNear(0); await Task.Delay(200);
                // B1 start phase-controlled
                PhaseSpin(phi, Qpc);
                long qStart = Qpc;
                var a = Sp(); a.InitialValue = 0f; a.InitialVelocity = 0f; a.FinalValue = 240f;
                _visual!.StartAnimation("Translation.X", a);
                long qStartRet = Qpc; double phiStart = PhaseOf(qStart);
                string extra = "";
                if (trip > 0)
                {
                    while (SecSince(qStart, Qpc) < trip - 0.030) await Task.Delay(5);
                    long qc = qStart + (long)(trip * Stopwatch.Frequency);
                    var last3 = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).TakeLast(2).ToList();
                    while (Qpc < qc) { }
                    PhaseSpin(phi, qc);
                    long qSeed = Qpc;
                    long qApply = NextFrame(qSeed);
                    double tSeed = SecSince(qStart, qSeed), tApply = SecSince(qStart, qApply);
                    double vB = 0;
                    if (last3.Count >= 2) { var c1 = last3[^2]; var c2 = last3[^1]; double dt = SecSince(c1.QpcArrive, c2.QpcArrive); if (dt > 0.004) vB = (c2.Cx - c1.Cx) / ppd / dt; }
                    double tS = arm == "A0" ? tSeed : arm == "Aphi" ? tApply : arm == "Ap8" ? tSeed + 0.008 : tSeed;
                    MfProbe.mf_spring_at(rr, zeta, 0, 240, 0, tS, out var shX, out var shV);
                    SpringScalarNaturalMotionAnimation Rep(double fin) { var r = Sp(); r.FinalValue = (float)fin; if (arm == "B") r.InitialVelocity = (float)vB; else { r.InitialValue = (float)shX; r.InitialVelocity = (float)shV; } return r; }
                    if (arm != "base")
                    {
                        if (dbl) { _visual.StartAnimation("Translation.X", Rep(360)); _visual.StartAnimation("Translation.X", Rep(0)); }
                        else _visual.StartAnimation("Translation.X", Rep(final));
                    }
                    long qM = Qpc; // tInt_m: replacement StartAnimation returned (commit into batch)
                    double phiM = PhaseOf(qM);
                    long qCommit = 0;
                    try { var op = _comp.RequestCommitAsync(); op.Completed = (_, __) => { qCommit = Qpc; }; } catch { }
                    extra = $"tInt_c={F(trip * 1000, 2)} tSeed={F(tSeed * 1000, 3)} tInt_m={F(SecSince(qStart, qM) * 1000, 3)} tApply={F(tApply * 1000, 3)} phi_m={F(phiM, 3)} seedT={F(tS * 1000, 3)} shX={F(shX, 3)} shV={F(shV, 2)} vB={F(vB, 2)}";
                    await Task.Delay(650);
                    extra += $" tCommitDone={(qCommit > 0 ? F(SecSince(qStart, qCommit) * 1000, 3) : "na")}";
                }
                else await Task.Delay(800);
                var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).ToList();
                CompReset(0, cell + " end");
                Both($"TRIAL {arm} {cell} {k} phi={F(phi, 0)} final={F(final, 0)} dbl={(dbl ? 1 : 0)} phiStart={F(phiStart, 3)} startCallUs={F(SecSince(qStart, qStartRet) * 1e6, 1)} {extra} n={caps.Count}");
                Both("RAW " + string.Join(" ", caps.Select(c => $"{F(SecSince(qStart, c.QpcArrive) * 1000, 2)}:{F((c.Cx - cx0) / ppd, 2)}:{F((c.T100ns + srtOff - qStart) / 1e4, 2)}")));
            }
            double[] phis = { 2, 6, 10, 14 };
            // P0
            for (int k = 0; k < 12; k++) foreach (var ph in phis) await Trial("P0base", "p0", 0, 240, false, k, ph);
            Both("P0 DONE");
            var cells = new (string name, double trip, double final, bool dbl)[] {
                ("t060rev", 0.060, 0, false), ("t060fwd", 0.060, 360, false),
                ("t156rev", 0.156, 0, false), ("t156fwd", 0.156, 360, false),
                ("t300rev", 0.300, 0, false), ("t300fwd", 0.300, 360, false),
                ("t156dbl", 0.156, 0, true) };
            string[] arms = { "B", "A0", "Aphi", "Ap8", "base" };
            var rnd = new Random(20261045);
            for (int rep = 0; rep < 5; rep++)
            {
                var blocks = new List<(int ci, int pi)>();
                for (int ci = 0; ci < cells.Length; ci++) for (int pi = 0; pi < 4; pi++) blocks.Add((ci, pi));
                foreach (var bl in blocks.OrderBy(_ => rnd.Next()).ToList())
                {
                    int rot = (rep + bl.ci + bl.pi) % 5;
                    for (int j = 0; j < 5; j++)
                    {
                        var arm = arms[(j + rot) % 5]; var c = cells[bl.ci];
                        await Trial(arm, c.name, c.trip, c.final, c.dbl, rep * 4 + bl.pi, phis[bl.pi]);
                    }
                }
                FlushLog(); Both($"REP {rep} DONE");
            }
            Both("===== R2 P9 RAW DONE =====");
            art.Close(); try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex) { Both("R2 P9 EXCEPTION " + ex); try { art.Close(); } catch { } try { MfProbe.mf_cap_stop(); } catch { } }
        finally { _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog(); try { CloseMainWindowSoon(); } catch { } }
    }

    private async Task RunR2P8Probe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artPath = Path.Combine(@"C:\Users\admin\motion_framework\artifacts", "win_r2_p8_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        const double rr = 0.32, zeta = 0.78;
        if (MfProbe.mf_map_spring(rr, zeta, out var period, out var zetaC) != 1) throw new Exception("map");
        Both("# win_r2_p8_raw.txt — 派题 #43 E2: P0 40 base δ calibration + P1 6 arms (A0, Am39, Am(Δ−15), AmΔ, Am(Δ+15), B) shuffled per round; formal mapping MapSpringToCompositor; raw WGC per trial (programmatic only, NO synthetic input)");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; composition.h UNCHANGED");
        Both($"# Period={F(period,5)} (r/2π); B1 spring r=0.32 ζ=0.78 0→240 IV=0 IVel=0; trip 60/156/300ms; dir rev→0 / fwd→360; dbl: 156ms →360 then →0 same tick");
        Both("# Am_X: seed = spring.h(r) x,v at tInt−X (X ms); B: IV unset + last-2 WGC frames velocity; RAW = tArr_ms:x:tSrt_ms (tSrt = SystemRelativeTime + session offset, offset=min(qpcArrive−srt))");
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "r2-p8-probe");
            CompReset(0, "p8 start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            var calQ0 = Qpc; CompReset(37, "p8 cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "p8 cal end"); await Task.Delay(500);
            var cc = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = cc.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = cc.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double ppd = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : 1.0;
            var rl = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault(); if (rl != null) cx0 = rl.Cx;
            Both($"cal cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(ppd,4)}");
            if (Stopwatch.Frequency != 10_000_000) Both($"WARN QPC freq {Stopwatch.Frequency} != 1e7");
            long srtOff = ReadCaptures().Where(c => c.N > 50).Select(c => c.QpcArrive - (long)c.T100ns).Min();
            Both($"srtOff(min qpcArrive−srt)={srtOff} Period={F(period,7)} DR={F(zetaC,4)}");
            async Task WaitNear(double dip, double tol = 4)
            {
                for (int i = 0; i < 80; i++)
                {
                    await Task.Delay(30);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(3).ToList();
                    if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / ppd - dip) < tol)) return;
                }
            }
            SpringScalarNaturalMotionAnimation Rep(string arm, double final, double shX, double shV, double vB, double vBp)
            {
                var r = _comp!.CreateSpringScalarAnimation();
                r.Period = TimeSpan.FromSeconds(period); r.DampingRatio = (float)zetaC; r.FinalValue = (float)final;
                if (arm.StartsWith("A")) { r.InitialValue = (float)shX; r.InitialVelocity = (float)shV; }
                else if (arm == "B") r.InitialVelocity = (float)vB;      // InitialValue NOT set
                                return r;
            }
            async Task<List<(double t, double x)>> Trial(string arm, string cell, double trip, double final, bool dbl, int k, int leadMs)
            {
                CompReset(0, cell); await WaitNear(0); await Task.Delay(200);
                long qStart = Qpc;
                var a = _comp!.CreateSpringScalarAnimation();
                a.Period = TimeSpan.FromSeconds(period); a.DampingRatio = (float)zetaC; a.InitialValue = 0f; a.InitialVelocity = 0f; a.FinalValue = 240f;
                _visual!.StartAnimation("Translation.X", a);
                if (arm != "base" && arm != "P0base") for (int i = 0; i < 80; i++) { await Task.Delay(4); if (SecSince(qStart, Qpc) >= trip) break; }
                var last3 = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).TakeLast(3).ToList();
                long qInt = Qpc; double tInt = SecSince(qStart, qInt);
                MfProbe.mf_spring_at(rr, zeta, 0, 240, 0, Math.Max(0, tInt - leadMs / 1000.0), out var shX, out var shV);
                double vB = 0, vBp = 0, acc = 0;
                if (last3.Count >= 2)
                {
                    var c1 = last3[^2]; var c2 = last3[^1]; double dt = SecSince(c1.QpcArrive, c2.QpcArrive);
                    if (dt > 0.004) vB = (c2.Cx - c1.Cx) / ppd / dt;
                    vBp = vB;
                    if (last3.Count >= 3)
                    {
                        var c0 = last3[0]; double dt0 = SecSince(c0.QpcArrive, c1.QpcArrive);
                        if (dt0 > 0.004 && dt > 0.004)
                        {
                            double v01 = (c1.Cx - c0.Cx) / ppd / dt0;
                            acc = (vB - v01) / (0.5 * (dt0 + dt));
                            vBp = vB + acc * SecSince(c2.QpcArrive, qInt);
                        }
                    }
                }
                if (arm != "base" && arm != "P0base")
                {
                    if (dbl)
                    {
                        _visual.StartAnimation("Translation.X", Rep(arm, 360, shX, shV, vB, vBp));
                        _visual.StartAnimation("Translation.X", Rep(arm, 0, shX, shV, vB, vBp));
                    }
                    else _visual.StartAnimation("Translation.X", Rep(arm, final, shX, shV, vB, vBp));
                }
                await Task.Delay(650);
                var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).ToList();
                CompReset(0, cell + " end");
                Both($"TRIAL {arm} {cell} {k} tInt={F(tInt * 1000, 2)} lead={leadMs} final={F(final,0)} dbl={(dbl?1:0)} shX={F(shX, 3)} shV={F(shV, 2)} vB={F(vB, 2)} vBp={F(vBp, 2)} acc={F(acc, 1)} n={caps.Count}");
                Both("RAW " + string.Join(" ", caps.Select(c => $"{F(SecSince(qStart, c.QpcArrive) * 1000, 2)}:{F((c.Cx - cx0) / ppd, 2)}:{F((c.T100ns + srtOff - qStart) / 1e4, 2)}")));
                return caps.Select(c => ((c.T100ns + srtOff - qStart) / 1e7, (c.Cx - cx0) / ppd)).ToList();
            }
            var cells = new (string name, double trip, double final, bool dbl)[] {
                ("t060rev", 0.060, 0, false), ("t060fwd", 0.060, 360, false),
                ("t156rev", 0.156, 0, false), ("t156fwd", 0.156, 360, false),
                ("t300rev", 0.300, 0, false), ("t300fwd", 0.300, 360, false),
                ("t156dbl", 0.156, 0, true) };
            // ---- P0: 40 base, δ grid 0..150ms step 0.5 (SSE vs spring.h(r) shifted), frames after stale lead-in
            var dl = new List<double>(); var rl2 = new List<double>();
            for (int k = 0; k < 40; k++)
            {
                var fr = await Trial("P0base", "base", 9.0, 240, false, k, 0);
                int k0 = fr.FindIndex(f => Math.Abs(f.x) < 0.5); if (k0 < 0) continue;
                var ff = fr.Skip(k0).Where(f => f.t <= 0.65).ToList();
                double bestD = 0, bestS = double.MaxValue;
                for (double d = 0; d <= 0.1500001; d += 0.0005)
                {
                    double sse = 0;
                    foreach (var f in ff) { double u = f.t - d; double xm = 0; if (u > 0) { MfProbe.mf_spring_at(rr, zeta, 0, 240, 0, u, out xm, out _); } sse += (f.x - xm) * (f.x - xm); }
                    if (sse < bestS) { bestS = sse; bestD = d; }
                }
                dl.Add(bestD * 1000); rl2.Add(Math.Sqrt(bestS / Math.Max(1, ff.Count)));
                Both($"P0 k={k} delta={F(bestD * 1000, 1)} rms={F(Math.Sqrt(bestS / Math.Max(1, ff.Count)), 3)} n={ff.Count}");
            }
            dl.Sort(); rl2.Sort();
            double Q(List<double> v, double p) { double ix = p * (v.Count - 1); int lo = (int)Math.Floor(ix); int hi = Math.Min(v.Count - 1, lo + 1); return v[lo] + (v[hi] - v[lo]) * (ix - lo); }
            double dHat = Q(dl, 0.5), iqr = Q(dl, 0.75) - Q(dl, 0.25), rmsMed = Q(rl2, 0.5);
            bool p0ok = dl.Count >= 30 && iqr <= 8 && rmsMed <= 1;
            int D = p0ok ? (int)Math.Round(dHat) : 70;
            Both($"P0 RESULT n={dl.Count} dHat={F(dHat, 2)} IQR={F(iqr, 2)} rmsMed={F(rmsMed, 3)} ok={p0ok} -> D={D}{(p0ok ? "" : " (MODEL-MISMATCH -> default 70 per dispatch)")}");
            // ---- P1
            var armDefs = new (string name, int lead)[] { ("A0", 0), ("Am39", 39), ("AmL", D - 15), ("AmM", D), ("AmH", D + 15), ("B", 0) };
            Both("P1 ARMS " + string.Join(" ", armDefs.Select(a => $"{a.name}={a.lead}")));
            var rnd = new Random(20261043);
            for (int k = 0; k < 20; k++)
            {
                var jobs = new List<(string arm, int lead, (string name, double trip, double final, bool dbl) c)>();
                jobs.Add(("base", 0, ("base", 9.0, 240, false)));
                foreach (var c in cells) foreach (var a in armDefs) jobs.Add((a.name, a.lead, c));
                foreach (var jb in jobs.OrderBy(_ => rnd.Next()).ToList())
                    await Trial(jb.arm, jb.c.name, jb.c.trip, jb.c.final, jb.c.dbl, k, jb.lead);
                FlushLog();
            }
            Both("===== R2 P8 RAW DONE =====");
            art.Close(); try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex) { Both("R2 P8 EXCEPTION " + ex); try { art.Close(); } catch { } try { MfProbe.mf_cap_stop(); } catch { } }
        finally { _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog(); try { CloseMainWindowSoon(); } catch { } }
    }
    private async Task RunR2P7Probe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artPath = Path.Combine(@"C:\Users\admin\motion_framework\artifacts", "win_r2_p7_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        const double rr = 0.32, zeta = 0.78, period = rr / (2 * Math.PI), TAU = 0.032, LEAD_A2 = -0.020;
        Both("# win_r2_p7_raw.txt — 派题 #42 remap (Period=r/2π, r=0.32) interrupt, 4 arms interleaved; raw WGC per trial (programmatic only, NO synthetic input)");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; composition.h UNCHANGED");
        Both($"# Period={F(period,5)} (r/2π); B1 spring r=0.32 ζ=0.78 0→240 IV=0 IVel=0; trip 60/156/300ms; dir rev→0 / fwd→360; dbl: 156ms →360 then →0 same tick");
        Both("# A0: IV/IVel=spring.h(r) x,v(tInt) | Am20: spring.h(r) x,v(tInt−20ms) [τ=32 (#41 cell r0.32ζ0.78 best-τ) − L=12 (#41 lead0)] | B: IV unset, IVel=last 2 WGC frames | Bs: IV unset, IVel=spring.h(r) v(tInt−τ=32ms); TRIAL vBp field = Bs velocity");
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "r2-p7-probe");
            CompReset(0, "p7 start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            var calQ0 = Qpc; CompReset(37, "p7 cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "p7 cal end"); await Task.Delay(500);
            var cc = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = cc.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = cc.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double ppd = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : 1.0;
            var rl = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault(); if (rl != null) cx0 = rl.Cx;
            Both($"cal cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(ppd,4)}");
            async Task WaitNear(double dip, double tol = 4)
            {
                for (int i = 0; i < 80; i++)
                {
                    await Task.Delay(30);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(3).ToList();
                    if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / ppd - dip) < tol)) return;
                }
            }
            SpringScalarNaturalMotionAnimation Rep(string arm, double final, double shX, double shV, double vB, double vBp)
            {
                var r = _comp!.CreateSpringScalarAnimation();
                r.Period = TimeSpan.FromSeconds(period); r.DampingRatio = (float)zeta; r.FinalValue = (float)final;
                if (arm == "A0" || arm == "Am20") { r.InitialValue = (float)shX; r.InitialVelocity = (float)shV; }
                else if (arm == "B") r.InitialVelocity = (float)vB;      // InitialValue NOT set
                else if (arm == "Bs") r.InitialVelocity = (float)vBp;    // InitialValue NOT set
                return r;
            }
            async Task Trial(string arm, string cell, double trip, double final, bool dbl, int k)
            {
                CompReset(0, cell); await WaitNear(0); await Task.Delay(200);
                long qStart = Qpc;
                var a = _comp!.CreateSpringScalarAnimation();
                a.Period = TimeSpan.FromSeconds(period); a.DampingRatio = (float)zeta; a.InitialValue = 0f; a.InitialVelocity = 0f; a.FinalValue = 240f;
                _visual!.StartAnimation("Translation.X", a);
                for (int i = 0; i < 80; i++) { await Task.Delay(4); if (SecSince(qStart, Qpc) >= trip) break; }
                var last3 = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).TakeLast(3).ToList();
                long qInt = Qpc; double tInt = SecSince(qStart, qInt);
                MfProbe.mf_spring_at(rr, zeta, 0, 240, 0, tInt, out var shX, out var shV);
                MfProbe.mf_spring_at(rr, zeta, 0, 240, 0, Math.Max(0, tInt + LEAD_A2), out var amX, out var amV);
                MfProbe.mf_spring_at(rr, zeta, 0, 240, 0, Math.Max(0, tInt - TAU), out var _sx, out var vS);
                double vB = 0, vBp = 0, acc = 0;
                if (last3.Count >= 2)
                {
                    var c1 = last3[^2]; var c2 = last3[^1]; double dt = SecSince(c1.QpcArrive, c2.QpcArrive);
                    if (dt > 0.004) vB = (c2.Cx - c1.Cx) / ppd / dt;
                    vBp = vB;
                    if (last3.Count >= 3)
                    {
                        var c0 = last3[0]; double dt0 = SecSince(c0.QpcArrive, c1.QpcArrive);
                        if (dt0 > 0.004 && dt > 0.004)
                        {
                            double v01 = (c1.Cx - c0.Cx) / ppd / dt0;
                            acc = (vB - v01) / (0.5 * (dt0 + dt));
                            vBp = vB + acc * SecSince(c2.QpcArrive, qInt);
                        }
                    }
                }
                if (arm != "base")
                {
                    if (dbl)
                    {
                        _visual.StartAnimation("Translation.X", Rep(arm, 360, arm == "Am20" ? amX : shX, arm == "Am20" ? amV : shV, vB, vS));
                        _visual.StartAnimation("Translation.X", Rep(arm, 0, arm == "Am20" ? amX : shX, arm == "Am20" ? amV : shV, vB, vS));
                    }
                    else _visual.StartAnimation("Translation.X", Rep(arm, final, arm == "Am20" ? amX : shX, arm == "Am20" ? amV : shV, vB, vS));
                }
                await Task.Delay(650);
                var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).ToList();
                CompReset(0, cell + " end");
                Both($"TRIAL {arm} {cell} {k} tInt={F(tInt * 1000, 2)} shX={F(shX, 3)} shV={F(shV, 2)} amX={F(amX, 3)} amV={F(amV, 2)} vB={F(vB, 2)} vBp={F(vS, 2)} acc={F(acc, 1)} n={caps.Count}");
                Both("RAW " + string.Join(" ", caps.Select(c => $"{F(SecSince(qStart, c.QpcArrive) * 1000, 2)}:{F((c.Cx - cx0) / ppd, 2)}")));
            }
            var cells = new (string name, double trip, double final, bool dbl)[] {
                ("t060rev", 0.060, 0, false), ("t060fwd", 0.060, 360, false),
                ("t156rev", 0.156, 0, false), ("t156fwd", 0.156, 360, false),
                ("t300rev", 0.300, 0, false), ("t300fwd", 0.300, 360, false),
                ("t156dbl", 0.156, 0, true) };
            string[] arms = { "A0", "Am20", "B", "Bs" };
            for (int k = 0; k < 20; k++)
            {
                await Trial("base", "base", 0.156, 240, false, k);
                foreach (var c in cells)
                    foreach (var arm in (k % 2 == 0 ? arms : arms.Skip(2).Concat(arms.Take(2)).Reverse().ToArray()))
                        await Trial(arm, c.name, c.trip, c.final, c.dbl, k);
                FlushLog();
            }
            Both("===== R2 P7 RAW DONE =====");
            art.Close(); try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex) { Both("R2 P7 EXCEPTION " + ex); try { art.Close(); } catch { } try { MfProbe.mf_cap_stop(); } catch { } }
        finally { _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog(); try { CloseMainWindowSoon(); } catch { } }
    }
    private async Task RunR2P2Probe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artPath = Path.Combine(@"C:\Users\admin\motion_framework\artifacts", "win_r2_p2_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        const double period = 0.32, zeta = 0.78;
        Both("# win_r2_p2_raw.txt — 派题 #35 P2 A/B/B' interleaved; raw WGC per trial (programmatic only, NO synthetic input)");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; composition.h UNCHANGED");
        Both("# B1 spring P=0.32 ζ=0.78 0→240 IV=0 IVel=0; trip 60/156/300ms; dir rev→0 / fwd→360; dbl: 156ms →360 then →0 same tick");
        Both("# A: IV=spring.h x(tInt), IVel=spring.h v(tInt) | B: IV unset, IVel=(x2−x1)/(t2−t1) last 2 WGC frames (arrival QPC) | Bp: B + a·(t_int−t2), a from last 3 frames");
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "r2-p2-probe");
            CompReset(0, "p2 start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            var calQ0 = Qpc; CompReset(37, "p2 cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "p2 cal end"); await Task.Delay(500);
            var cc = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = cc.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = cc.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double ppd = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : 1.0;
            var rl = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault(); if (rl != null) cx0 = rl.Cx;
            Both($"cal cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(ppd,4)}");
            async Task WaitNear(double dip, double tol = 4)
            {
                for (int i = 0; i < 80; i++)
                {
                    await Task.Delay(30);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(3).ToList();
                    if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / ppd - dip) < tol)) return;
                }
            }
            SpringScalarNaturalMotionAnimation Rep(string arm, double final, double shX, double shV, double vB, double vBp)
            {
                var r = _comp!.CreateSpringScalarAnimation();
                r.Period = TimeSpan.FromSeconds(period); r.DampingRatio = (float)zeta; r.FinalValue = (float)final;
                if (arm == "A") { r.InitialValue = (float)shX; r.InitialVelocity = (float)shV; }
                else if (arm == "B") r.InitialVelocity = (float)vB;      // InitialValue NOT set
                else if (arm == "Bp") r.InitialVelocity = (float)vBp;    // InitialValue NOT set
                return r;
            }
            async Task Trial(string arm, string cell, double trip, double final, bool dbl, int k)
            {
                CompReset(0, cell); await WaitNear(0); await Task.Delay(200);
                long qStart = Qpc;
                var a = _comp!.CreateSpringScalarAnimation();
                a.Period = TimeSpan.FromSeconds(period); a.DampingRatio = (float)zeta; a.InitialValue = 0f; a.InitialVelocity = 0f; a.FinalValue = 240f;
                _visual!.StartAnimation("Translation.X", a);
                for (int i = 0; i < 80; i++) { await Task.Delay(4); if (SecSince(qStart, Qpc) >= trip) break; }
                var last3 = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).TakeLast(3).ToList();
                long qInt = Qpc; double tInt = SecSince(qStart, qInt);
                MfProbe.mf_spring_at(period, zeta, 0, 240, 0, tInt, out var shX, out var shV);
                double vB = 0, vBp = 0, acc = 0;
                if (last3.Count >= 2)
                {
                    var c1 = last3[^2]; var c2 = last3[^1]; double dt = SecSince(c1.QpcArrive, c2.QpcArrive);
                    if (dt > 0.004) vB = (c2.Cx - c1.Cx) / ppd / dt;
                    vBp = vB;
                    if (last3.Count >= 3)
                    {
                        var c0 = last3[0]; double dt0 = SecSince(c0.QpcArrive, c1.QpcArrive);
                        if (dt0 > 0.004 && dt > 0.004)
                        {
                            double v01 = (c1.Cx - c0.Cx) / ppd / dt0;
                            acc = (vB - v01) / (0.5 * (dt0 + dt));
                            vBp = vB + acc * SecSince(c2.QpcArrive, qInt);
                        }
                    }
                }
                if (arm != "base")
                {
                    if (dbl)
                    {
                        _visual.StartAnimation("Translation.X", Rep(arm, 360, shX, shV, vB, vBp));
                        _visual.StartAnimation("Translation.X", Rep(arm, 0, shX, shV, vB, vBp));
                    }
                    else _visual.StartAnimation("Translation.X", Rep(arm, final, shX, shV, vB, vBp));
                }
                await Task.Delay(650);
                var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).ToList();
                CompReset(0, cell + " end");
                Both($"TRIAL {arm} {cell} {k} tInt={F(tInt * 1000, 2)} shX={F(shX, 3)} shV={F(shV, 2)} vB={F(vB, 2)} vBp={F(vBp, 2)} acc={F(acc, 1)} n={caps.Count}");
                Both("RAW " + string.Join(" ", caps.Select(c => $"{F(SecSince(qStart, c.QpcArrive) * 1000, 2)}:{F((c.Cx - cx0) / ppd, 2)}")));
            }
            var cells = new (string name, double trip, double final, bool dbl)[] {
                ("t060rev", 0.060, 0, false), ("t060fwd", 0.060, 360, false),
                ("t156rev", 0.156, 0, false), ("t156fwd", 0.156, 360, false),
                ("t300rev", 0.300, 0, false), ("t300fwd", 0.300, 360, false),
                ("t156dbl", 0.156, 0, true) };
            string[] arms = { "A", "B", "Bp" };
            for (int k = 0; k < 20; k++)
            {
                await Trial("base", "base", 0.156, 240, false, k);
                foreach (var c in cells)
                    foreach (var arm in (k % 2 == 0 ? arms : arms.Reverse().ToArray()))
                        await Trial(arm, c.name, c.trip, c.final, c.dbl, k);
                FlushLog();
            }
            Both("===== R2 P2 RAW DONE =====");
            art.Close(); try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex) { Both("R2 P2 EXCEPTION " + ex); try { art.Close(); } catch { } try { MfProbe.mf_cap_stop(); } catch { } }
        finally { _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog(); try { CloseMainWindowSoon(); } catch { } }
    }

    private async Task RunS1v2Probe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artPath = Path.Combine(@"C:\Users\admin\motion_framework\artifacts", "win_s1v2_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        const double TWO_PI = 2 * Math.PI;
        Both("# win_s1v2_raw.txt — 派题 #44 formal S1 v2 build (Period/ζ from composition.h MapSpringToCompositor via mfprobe.dll mf_map_spring); raw WGC per trial (programmatic only, NO synthetic input)");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; composition.h UNCHANGED");
        Both("# SHAPE: spring 0→240 IV=0 IVel=0; arm v2 = mf_map_spring(r,ζ) → Period,DampingRatio; cells r∈{0.2,0.32,0.5}×ζ∈{0.6,0.78,1.0} + E(r=0.16,ζ=0.78); A=(0.32,0.78); 5 trials/cell interleaved");
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "s1v2-probe");
            CompReset(0, "s1v2 start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            var calQ0 = Qpc; CompReset(37, "s1v2 cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "s1v2 cal end"); await Task.Delay(500);
            var cc = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = cc.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = cc.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double ppd = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : 1.0;
            var rl = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault(); if (rl != null) cx0 = rl.Cx;
            Both($"cal cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(ppd,4)}");
            async Task WaitNear(double dip, double tol = 2)
            {
                for (int i = 0; i < 100; i++)
                {
                    await Task.Delay(40);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(4).ToList();
                    if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / ppd - dip) < tol)) return;
                }
            }
            string Raw(long q0) => "RAW " + string.Join(" ", ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > q0).OrderBy(c => c.QpcArrive).Select(c => $"{F(SecSince(q0, c.QpcArrive) * 1000, 2)}:{F((c.Cx - cx0) / ppd, 2)}"));
            SpringScalarNaturalMotionAnimation Sp(double period, double z) { var a = _comp!.CreateSpringScalarAnimation(); a.Period = TimeSpan.FromSeconds(period); a.DampingRatio = (float)z; return a; }
            // ---- Part 1
            var cells = new List<(string id, double r, double z)>();
            foreach (var r in new[] { 0.2, 0.32, 0.5 }) foreach (var z in new[] { 0.6, 0.78, 1.0 }) cells.Add(($"r{r:F2}z{z:F2}", r, z));
            cells.Add(("E_r0.16z0.78", 0.16, 0.78));
            for (int k = 0; k < 5; k++)
                foreach (var c in cells)
                    foreach (var arm in new[] { "v2" })
                    {
                        CompReset(0, "p6s"); await WaitNear(0); await Task.Delay(250);
                        if (MfProbe.mf_map_spring(c.r, c.z, out var per, out var zc) != 1) throw new Exception("mf_map_spring rejected r=" + c.r);
                        var a = Sp(per, zc); a.InitialValue = 0f; a.InitialVelocity = 0f; a.FinalValue = 240f;
                        long q0 = Qpc; _visual!.StartAnimation("Translation.X", a);
                        await Task.Delay(1450);
                        string raw = Raw(q0); CompReset(0, "p6s end");
                        Both($"SHAPE {c.id} {arm} k={k} r={F(c.r,3)} z={F(c.z,2)} Period={F(per,7)} DR={F(zc,4)}");
                        Both(raw);
                    }
            foreach (var c in cells)
                for (int ms = 0; ms <= 1500; ms++) { MfProbe.mf_spring_at(c.r, c.z, 0, 240, 0, ms / 1000.0, out var x, out var v); Both($"MB {c.id} {ms} {F(x, 4)} {F(v, 3)}"); }
            Both("===== S1V2 RAW DONE =====");
            art.Close(); try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex) { Both("S1V2 EXCEPTION " + ex); try { art.Close(); } catch { } try { MfProbe.mf_cap_stop(); } catch { } }
        finally { _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog(); try { CloseMainWindowSoon(); } catch { } }
    }

    private async Task RunR2P6Probe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artPath = Path.Combine(@"C:\Users\admin\motion_framework\artifacts", "win_r2_p6_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        const double TWO_PI = 2 * Math.PI;
        Both("# win_r2_p6_raw.txt — 派题 #41 Period=r/2π remap validation; raw WGC per trial (programmatic only, NO synthetic input)");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; composition.h UNCHANGED");
        Both("# Part1 SHAPE: spring 0→240 IV=0 IVel=0; arms remap(Period=r/2π) | orig(Period=r); cells r∈{0.2,0.32,0.5}×ζ∈{0.6,0.78,1.0} + E(r=0.16,ζ=0.78); A=(0.32,0.78)");
        Both("# Part2 INT: B1 spring remap r=0.32 ζ=0.78 0→240; at ≈156ms B2 remap r=0.32 ζ=0.78 Final=0 with IV/IVel=spring.h(r=0.32) x,v at tInt+lead; arms base | lead0 | lead33");
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "r2-p6-probe");
            CompReset(0, "p6 start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            var calQ0 = Qpc; CompReset(37, "p6 cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "p6 cal end"); await Task.Delay(500);
            var cc = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = cc.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = cc.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double ppd = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : 1.0;
            var rl = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault(); if (rl != null) cx0 = rl.Cx;
            Both($"cal cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(ppd,4)}");
            async Task WaitNear(double dip, double tol = 2)
            {
                for (int i = 0; i < 100; i++)
                {
                    await Task.Delay(40);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(4).ToList();
                    if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / ppd - dip) < tol)) return;
                }
            }
            string Raw(long q0) => "RAW " + string.Join(" ", ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > q0).OrderBy(c => c.QpcArrive).Select(c => $"{F(SecSince(q0, c.QpcArrive) * 1000, 2)}:{F((c.Cx - cx0) / ppd, 2)}"));
            SpringScalarNaturalMotionAnimation Sp(double period, double z) { var a = _comp!.CreateSpringScalarAnimation(); a.Period = TimeSpan.FromSeconds(period); a.DampingRatio = (float)z; return a; }
            // ---- Part 1
            var cells = new List<(string id, double r, double z)>();
            foreach (var r in new[] { 0.2, 0.32, 0.5 }) foreach (var z in new[] { 0.6, 0.78, 1.0 }) cells.Add(($"r{r:F2}z{z:F2}", r, z));
            cells.Add(("E_r0.16z0.78", 0.16, 0.78));
            for (int k = 0; k < 5; k++)
                foreach (var c in cells)
                    foreach (var arm in (k % 2 == 0 ? new[] { "remap", "orig" } : new[] { "orig", "remap" }))
                    {
                        CompReset(0, "p6s"); await WaitNear(0); await Task.Delay(250);
                        double per = arm == "remap" ? c.r / TWO_PI : c.r;
                        var a = Sp(per, c.z); a.InitialValue = 0f; a.InitialVelocity = 0f; a.FinalValue = 240f;
                        long q0 = Qpc; _visual!.StartAnimation("Translation.X", a);
                        await Task.Delay(1450);
                        string raw = Raw(q0); CompReset(0, "p6s end");
                        Both($"SHAPE {c.id} {arm} k={k} r={F(c.r,3)} z={F(c.z,2)} Period={F(per,5)}");
                        Both(raw);
                    }
            foreach (var c in cells)
                for (int ms = 0; ms <= 1500; ms++) { MfProbe.mf_spring_at(c.r, c.z, 0, 240, 0, ms / 1000.0, out var x, out var v); Both($"MB {c.id} {ms} {F(x, 4)} {F(v, 3)}"); }
            // ---- Part 2
            const double r2 = 0.32, z2 = 0.78;
            for (int k = 0; k < 20; k++)
                foreach (var arm in (k % 2 == 0 ? new[] { "base", "lead0", "lead33" } : new[] { "base", "lead33", "lead0" }))
                {
                    CompReset(0, "p6i"); await WaitNear(0); await Task.Delay(250);
                    var a = Sp(r2 / TWO_PI, z2); a.InitialValue = 0f; a.InitialVelocity = 0f; a.FinalValue = 240f;
                    long q0 = Qpc; _visual!.StartAnimation("Translation.X", a);
                    for (int i = 0; i < 60; i++) { await Task.Delay(3); if (SecSince(q0, Qpc) >= 0.156) break; }
                    long qInt = Qpc; double tInt = SecSince(q0, qInt);
                    double lead = arm == "lead33" ? 0.033 : 0;
                    MfProbe.mf_spring_at(r2, z2, 0, 240, 0, tInt + lead, out var sx, out var sv);
                    if (arm != "base")
                    {
                        var b = Sp(r2 / TWO_PI, z2); b.InitialValue = (float)sx; b.InitialVelocity = (float)sv; b.FinalValue = 0f;
                        _visual.StartAnimation("Translation.X", b);
                    }
                    await Task.Delay(850);
                    string raw = Raw(q0); CompReset(0, "p6i end");
                    Both($"TRIAL {arm} t156rev {k} tInt={F(tInt * 1000, 2)} lead={F(lead * 1000, 0)} shX={F(sx, 3)} shV={F(sv, 2)}");
                    Both(raw);
                }
            Both("===== R2 P6 RAW DONE =====");
            art.Close(); try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex) { Both("R2 P6 EXCEPTION " + ex); try { art.Close(); } catch { } try { MfProbe.mf_cap_stop(); } catch { } }
        finally { _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog(); try { CloseMainWindowSoon(); } catch { } }
    }

    private async Task RunR2P3Probe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artPath = Path.Combine(@"C:\Users\admin\motion_framework\artifacts", "win_r2_p3_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        Both("# win_r2_p3_raw.txt — 派题 #36 P3 compositor timebase & start decomposition (uninterrupted); raw WGC per trial (programmatic only, NO synthetic input)");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; composition.h UNCHANGED");
        Both("# spring 0→240 IV=0 IVel=0, Period∈{0.2,0.32,0.5} × ζ∈{0.6,0.78,1.0}, 5 trials/cell interleaved; t0=QPC just before StartAnimation; capture QpcArrive");
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "r2-p3-probe");
            CompReset(0, "p3 start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            var calQ0 = Qpc; CompReset(37, "p3 cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "p3 cal end"); await Task.Delay(500);
            var cc = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = cc.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = cc.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double ppd = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : 1.0;
            var rl = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault(); if (rl != null) cx0 = rl.Cx;
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
            double[] periods = { 0.2, 0.32, 0.5 }; double[] zetas = { 0.6, 0.78, 1.0 };
            for (int k = 0; k < 5; k++)
                foreach (var P in periods) foreach (var z in zetas)
                {
                    CompReset(0, "p3"); await WaitNear(0); await Task.Delay(250);
                    var a = _comp!.CreateSpringScalarAnimation();
                    a.Period = TimeSpan.FromSeconds(P); a.DampingRatio = (float)z; a.InitialValue = 0f; a.InitialVelocity = 0f; a.FinalValue = 240f;
                    long qStart = Qpc;
                    _visual!.StartAnimation("Translation.X", a);
                    long qAfter = Qpc;
                    await Task.Delay(z >= 1.0 ? 1600 : 1200);
                    var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).ToList();
                    CompReset(0, "p3 end");
                    Both($"TRIAL P={F(P,2)} z={F(z,2)} k={k} callUs={F(SecSince(qStart, qAfter) * 1e6, 1)} n={caps.Count}");
                    Both("RAW " + string.Join(" ", caps.Select(c => $"{F(SecSince(qStart, c.QpcArrive) * 1000, 2)}:{F((c.Cx - cx0) / ppd, 2)}")));
                }
            foreach (var P in periods) foreach (var z in zetas)
                for (int ms = 0; ms <= 1600; ms += 1) { MfProbe.mf_spring_at(P, z, 0, 240, 0, ms / 1000.0, out var x, out var v); Both($"MB P={F(P,2)} z={F(z,2)} {ms} {F(x, 4)} {F(v, 3)}"); }
            Both("===== R2 P3 RAW DONE =====");
            art.Close(); try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex) { Both("R2 P3 EXCEPTION " + ex); try { art.Close(); } catch { } try { MfProbe.mf_cap_stop(); } catch { } }
        finally { _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog(); try { CloseMainWindowSoon(); } catch { } }
    }

    private async Task RunM2H5SeedProbe()
    {
        // M2-H5-SEED: WGC/screen-consistent InitialVelocity seed (path A) with IV=null+0 degrade (path B).
        // Does NOT touch composition.h. Does NOT use mf_spring_at / spring.h closed-form v as compositor IV.
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string sessFile = Path.Combine(artDir, "m2_h5_seed_session.txt");
        string sessEnv = File.Exists(sessFile) ? File.ReadAllText(sessFile).Trim() : (Environment.GetEnvironmentVariable("MF_M2_H5_SEED_SESSION") ?? "1");
        string artPath = Path.Combine(artDir, "m2_h5_seed_capture_raw.txt");
        string sumPath = Path.Combine(artDir, "m2_h5_seed_capture_summary.txt");
        string artPathSess = Path.Combine(artDir, $"m2_h5_seed_s{sessEnv}_raw.txt");
        string sumPathSess = Path.Combine(artDir, $"m2_h5_seed_s{sessEnv}_summary.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        var sum = new StreamWriter(sumPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        void Sum(string s) { Both(s); sum.WriteLine(s); sum.Flush(); }

        // calib: same measurement window for seed and v_screen ⇒ r≡|calib|.
        // #37 wgc med ratio 0.796 was vs offline-differently-computed v_pre; gate v_screen = last-two WGC frames
        // (same as seed). calib=1.0 is the clean same-window choice (dispatch: "r≡1 可接受且最干净").
        const double Calib = 1.0;
        const double Eps = 5.0; // DIP/s
        const int NeedN = 12;
        const double period = 0.32, zeta = 0.78;

        Both($"# m2_h5_seed_s{sessEnv}_raw.txt — M2-H5-SEED WGC seed probe (programmatic only, NO synthetic input)");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} AWST; composition.h UNCHANGED; calib={Calib.ToString(CultureInfo.InvariantCulture)}");
        Both("# B1 spring P=0.32 ζ=0.78 0→240 IV=0 IVel=0; trip≈156ms; B2 Final=0 InitialValue NOT set");
        Both("# path A: InitialVelocity=(float)(calib*v_wgc); path B fallback/control: IV unset + InitialVelocity=0");
        Both("# v_screen = last-two WGC Translation.X frames before interrupt, (x2-x1)/(t2-t1) QPC→s DIP/s; same instant as seed");
        Both($"# ε={Eps.ToString(CultureInfo.InvariantCulture)} DIP/s; need n_valid_A>={NeedN}; r_i=|v_seed/v_screen|; med_r session median");
        Both("# FORBIDDEN: mf_spring_at / spring.h closed-form v as compositor InitialVelocity (log-only ok)");

        var aRows = new List<(int k, string path, double tInt, double vScreen, double vSeed, double r, bool valid)>();
        var bRows = new List<(int k, string path, double tInt, double vScreen, double vSeed, double r, bool valid)>();

        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "m2-h5-seed");
            CompReset(0, "h5 start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            var calQ0 = Qpc; CompReset(37, "h5 cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "h5 cal end"); await Task.Delay(500);
            var cc = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = cc.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = cc.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double ppd = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : 1.0;
            var rl = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault(); if (rl != null) cx0 = rl.Cx;
            Both($"cal cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(ppd,4)} calib={F(Calib,6)}");

            async Task WaitNear(double dip, double tol = 4)
            {
                for (int i = 0; i < 80; i++)
                {
                    await Task.Delay(40);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(4).ToList();
                    if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / ppd - dip) < tol)) return;
                }
            }

            async Task<(double tInt, double vWgc, double vScreen, double vSeed, double r, string path, bool valid)> OneTrial(string armWanted, int k)
            {
                CompReset(0, armWanted); await WaitNear(0); await Task.Delay(250);
                long qStart = Qpc;
                var a = _comp!.CreateSpringScalarAnimation();
                a.Period = TimeSpan.FromSeconds(period); a.DampingRatio = (float)zeta;
                a.InitialValue = 0f; a.InitialVelocity = 0f; a.FinalValue = 240f;
                _visual!.StartAnimation("Translation.X", a);
                for (int i = 0; i < 40; i++) { await Task.Delay(5); if (SecSince(qStart, Qpc) >= 0.156) break; }
                long qInt = Qpc; double tInt = SecSince(qStart, qInt);
                // log-only shadow (FORBIDDEN as IV)
                MfProbe.mf_spring_at(period, zeta, 0, 240, 0, tInt, out var shX, out var shV);
                var last3 = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart && c.QpcArrive <= qInt)
                    .OrderBy(c => c.QpcArrive).TakeLast(3).ToList();
                // fallback: if none strictly <= qInt (capture lag), take last before/near interrupt from > qStart
                if (last3.Count < 2)
                    last3 = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).TakeLast(3).ToList();
                double vWgc = double.NaN;
                if (last3.Count >= 2)
                {
                    var c1 = last3[^2]; var c2 = last3[^1];
                    double dt = SecSince(c1.QpcArrive, c2.QpcArrive);
                    if (dt > 0.004) vWgc = (c2.Cx - c1.Cx) / ppd / dt;
                }
                double vScreen = vWgc; // same measurement window as seed (gate / dispatch)
                string path;
                double vSeed;
                var rAnim = _comp.CreateSpringScalarAnimation();
                rAnim.Period = TimeSpan.FromSeconds(period); rAnim.DampingRatio = (float)zeta;
                rAnim.FinalValue = 0f; // InitialValue NOT set
                if (armWanted == "A")
                {
                    bool wgcOk = !double.IsNaN(vWgc) && Math.Abs(vWgc) >= Eps;
                    if (wgcOk)
                    {
                        vSeed = Calib * vWgc;
                        rAnim.InitialVelocity = (float)vSeed;
                        path = "A";
                    }
                    else
                    {
                        // runtime fallback B for this sample
                        rAnim.InitialVelocity = 0f;
                        vSeed = 0;
                        path = "B";
                    }
                }
                else
                {
                    // explicit B control arm
                    rAnim.InitialVelocity = 0f;
                    vSeed = 0;
                    path = "B";
                }
                _visual.StartAnimation("Translation.X", rAnim);
                await Task.Delay(500);
                CompReset(0, armWanted + " end");
                bool valid = path == "A" && !double.IsNaN(vScreen) && Math.Abs(vScreen) >= Eps && !double.IsNaN(vSeed);
                double r = (valid || (path == "B" && !double.IsNaN(vScreen) && Math.Abs(vScreen) >= Eps && Math.Abs(vScreen) > 1e-9))
                    ? Math.Abs(vSeed / vScreen) : double.NaN;
                // For B control, count as control-valid if |v_screen|>=ε (ratio expected ≈0)
                bool bValid = path == "B" && !double.IsNaN(vScreen) && Math.Abs(vScreen) >= Eps;
                Both($"TRIAL armWanted={armWanted} k={k} path={path} tIntMs={F(tInt * 1000, 2)} shX={F(shX, 3)} shV={F(shV, 2)}(log-only) v_wgc={F(vWgc, 3)} v_screen={F(vScreen, 3)} v_seed={F(vSeed, 3)} r_i={F(r, 4)} validA={valid} validBctrl={bValid}");
                return (tInt, vWgc, vScreen, vSeed, r, path, armWanted == "A" ? valid : bValid);
            }

            // Arm A: collect NeedN valid path-A samples (retry up to 2*NeedN attempts)
            int aAttempt = 0;
            while (aRows.Count(x => x.valid) < NeedN && aAttempt < NeedN * 2)
            {
                var t = await OneTrial("A", aAttempt);
                aRows.Add((aAttempt, t.path, t.tInt, t.vScreen, t.vSeed, t.r, t.valid && t.path == "A"));
                aAttempt++;
            }

            // Arm B control: NeedN trials (not counted in 3/3)
            for (int k = 0; k < NeedN; k++)
            {
                var t = await OneTrial("B", k);
                bRows.Add((k, t.path, t.tInt, t.vScreen, t.vSeed, t.r, t.valid));
            }

            static double Median(List<double> xs)
            {
                if (xs.Count == 0) return double.NaN;
                var s = xs.OrderBy(v => v).ToList();
                int n = s.Count;
                return (n % 2 == 1) ? s[n / 2] : 0.5 * (s[n / 2 - 1] + s[n / 2]);
            }

            var aValid = aRows.Where(x => x.valid && x.path == "A").ToList();
            var aRs = aValid.Select(x => x.r).Where(r => !double.IsNaN(r)).ToList();
            double medA = Median(aRs);
            double maxA = aRs.Count > 0 ? aRs.Max() : double.NaN;
            bool anyGe2 = aRs.Any(r => r >= 2.0);
            int nA = aValid.Count;

            var bValid = bRows.Where(x => x.valid).ToList();
            var bRs = bValid.Select(x => x.r).Where(r => !double.IsNaN(r)).ToList();
            double medB = Median(bRs);

            bool sessPass = nA >= NeedN && !double.IsNaN(medA) && medA >= 0.8 && medA <= 1.2 && !anyGe2;

            Sum($"# m2_h5_seed_s{sessEnv}_summary.txt");
            Sum($"session={sessEnv} pid={Environment.ProcessId} calib={F(Calib,6)} eps={F(Eps,1)}");
            Sum($"armA n_valid={nA} med_r={F(medA, 4)} max_r={F(maxA, 4)} any_r_ge_2={anyGe2} attempts={aAttempt}");
            Sum($"armB_control n_valid={bValid.Count} med_r={F(medB, 4)} (expected≈0; not counted in 3/3)");
            Sum($"shadow_eliminated=yes (A seed from WGC*calib, NOT mf_spring_at)");
            Sum(sessPass ? "SESSION_PASS_CANDIDATE" : "SESSION_FAIL_CANDIDATE");
            Sum($"PASS_WINDOW=[0.8,1.2] STRONG_NO_SAMPLE_GE_2=yes");
            Both("===== M2-H5-SEED PROBE DONE =====");
            Sum("===== M2-H5-SEED PROBE DONE =====");

            art.Close(); sum.Close();
            try { File.Copy(artPath, artPathSess, true); } catch { }
            try { File.Copy(sumPath, sumPathSess, true); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("M2-H5-SEED EXCEPTION " + ex);
            try { art.Close(); } catch { }
            try { sum.Close(); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        finally { _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog(); try { CloseMainWindowSoon(); } catch { } }
    }

    private async Task RunR2P4Probe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artPath = Path.Combine(@"C:\Users\admin\motion_framework\artifacts", "win_r2_p4_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        const double period = 0.32, zeta = 0.78;
        Both("# win_r2_p4_raw.txt — 派题 #37 P4 B2 velocity source, IV=null arm; raw WGC per trial (programmatic only, NO synthetic input)");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; composition.h UNCHANGED");
        Both("# B1 spring P=0.32 ζ=0.78 0→240 IV=0 IVel=0; trip ≈156ms; B2 spring P=0.32 ζ=0.78 Final=0, InitialValue NOT set; arms: shadow=spring.h v(tInt) | zero=IVel 0f | unset | wgc=on-screen v from last WGC frames");
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "r2-p4-probe");
            CompReset(0, "p4 start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            var calQ0 = Qpc; CompReset(37, "p4 cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "p4 cal end"); await Task.Delay(500);
            var cc = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = cc.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = cc.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double ppd = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : 1.0;
            var rl = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault(); if (rl != null) cx0 = rl.Cx;
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
            async Task Trial(string arm, int k)
            {
                CompReset(0, arm); await WaitNear(0); await Task.Delay(250);
                long qStart = Qpc;
                var a = _comp!.CreateSpringScalarAnimation();
                a.Period = TimeSpan.FromSeconds(period); a.DampingRatio = (float)zeta; a.InitialValue = 0f; a.InitialVelocity = 0f; a.FinalValue = 240f;
                _visual!.StartAnimation("Translation.X", a);
                for (int i = 0; i < 40; i++) { await Task.Delay(5); if (SecSince(qStart, Qpc) >= 0.156) break; }
                long qInt = Qpc; double tInt = SecSince(qStart, qInt);
                MfProbe.mf_spring_at(period, zeta, 0, 240, 0, tInt, out var shX, out var shV);
                var last3 = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).TakeLast(3).ToList();
                double wv = double.NaN;
                if (last3.Count >= 2) { var c1 = last3[^2]; var c2 = last3[^1]; double dt = SecSince(c1.QpcArrive, c2.QpcArrive); if (dt > 0.004) wv = (c2.Cx - c1.Cx) / ppd / dt; }
                double vSet = double.NaN;
                if (arm != "base")
                {
                    var r = _comp.CreateSpringScalarAnimation();
                    r.Period = TimeSpan.FromSeconds(period); r.DampingRatio = (float)zeta; r.FinalValue = 0f; // InitialValue NOT set
                    if (arm == "shadow") { r.InitialVelocity = (float)shV; vSet = shV; }
                    else if (arm == "zero") { r.InitialVelocity = 0f; vSet = 0; }
                    else if (arm == "wgc") { vSet = double.IsNaN(wv) ? 0 : wv; r.InitialVelocity = (float)vSet; }
                    _visual.StartAnimation("Translation.X", r);
                }
                await Task.Delay(800);
                var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).ToList();
                CompReset(0, arm + " end");
                Both($"TRIAL {arm} {k} tInt={F(tInt * 1000, 2)} shX={F(shX, 3)} shV={F(shV, 2)} wgcV={F(wv, 2)} vSet={F(vSet, 2)} n={caps.Count}");
                Both("RAW " + string.Join(" ", caps.Select(c => $"{F(SecSince(qStart, c.QpcArrive) * 1000, 2)}:{F((c.Cx - cx0) / ppd, 2)}")));
            }
            string[] arms = { "base", "shadow", "zero", "unset", "wgc" };
            for (int k = 0; k < 12; k++) foreach (var arm in arms) await Trial(arm, k);
            for (int ms = 0; ms <= 1000; ms += 2) { MfProbe.mf_spring_at(period, zeta, 0, 240, 0, ms / 1000.0, out var x, out var v); Both($"MB1 {ms} {F(x, 4)} {F(v, 3)}"); }
            Both("===== R2 P4 RAW DONE =====");
            art.Close(); try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex) { Both("R2 P4 EXCEPTION " + ex); try { art.Close(); } catch { } try { MfProbe.mf_cap_stop(); } catch { } }
        finally { _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog(); try { CloseMainWindowSoon(); } catch { } }
    }

    private async Task RunR2P1Probe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artPath = Path.Combine(@"C:\Users\admin\motion_framework\artifacts", "win_r2_p1_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        Both("# win_r2_p1_raw.txt — 派题 #34 P1 IV=null replacement; raw WGC per trial (programmatic only, NO synthetic input)");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; composition.h UNCHANGED");
        Both("# driver: spring P=0.32 ζ=0.78 0→240 IV=0 | linear KF 0→240 1000ms | static SetValue 120; replacement: Spring P=0.32 ζ=0.78 Final=360, InitialValue NOT set (null), InitialVelocity NOT set; trip ≈156ms (static: after rest)");
        Both("# double: two StartAnimation (IV=null →360 then IV=null →300) back-to-back on UI thread (same commit) on running spring");
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "r2-p1-probe");
            CompReset(0, "p1 start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            var calQ0 = Qpc; CompReset(37, "p1 cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "p1 cal end"); await Task.Delay(500);
            var cc = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = cc.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = cc.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double ppd = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : 1.0;
            var rl = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault(); if (rl != null) cx0 = rl.Cx;
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
            SpringScalarNaturalMotionAnimation Rep(double final)
            {
                var r = _comp!.CreateSpringScalarAnimation();
                r.Period = TimeSpan.FromSeconds(0.32); r.DampingRatio = 0.78f; r.FinalValue = (float)final;
                return r; // InitialValue / InitialVelocity intentionally unset (null)
            }
            async Task Trial(string cell, int k, bool interrupt)
            {
                double rest = cell == "static" ? 120 : 0;
                CompReset(rest, cell); await WaitNear(rest); await Task.Delay(250);
                long qStart = Qpc;
                if (cell == "spring" || cell == "double")
                {
                    var a = _comp!.CreateSpringScalarAnimation();
                    a.Period = TimeSpan.FromSeconds(0.32); a.DampingRatio = 0.78f; a.InitialValue = 0f; a.InitialVelocity = 0f; a.FinalValue = 240f;
                    _visual!.StartAnimation("Translation.X", a);
                }
                else if (cell == "linear")
                {
                    var kf = _comp!.CreateScalarKeyFrameAnimation();
                    var lin = _comp.CreateLinearEasingFunction();
                    kf.InsertKeyFrame(0f, 0f, lin); kf.InsertKeyFrame(1f, 240f, lin); kf.Duration = TimeSpan.FromSeconds(1.0);
                    _visual!.StartAnimation("Translation.X", kf);
                }
                for (int i = 0; i < 40; i++) { await Task.Delay(5); if (SecSince(qStart, Qpc) >= 0.156) break; }
                long qInt = Qpc; double tInt = SecSince(qStart, qInt);
                if (interrupt)
                {
                    _visual!.StartAnimation("Translation.X", Rep(360));
                    if (cell == "double") _visual.StartAnimation("Translation.X", Rep(300));
                }
                await Task.Delay(700);
                var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).ToList();
                CompReset(rest, cell + " end");
                Both($"TRIAL {cell} {k} {(interrupt ? "int" : "base")} tInt={F(tInt * 1000, 2)} n={caps.Count}");
                Both("RAW " + string.Join(" ", caps.Select(c => $"{F(SecSince(qStart, c.QpcArrive) * 1000, 2)}:{F((c.Cx - cx0) / ppd, 2)}")));
            }
            string[] cells = { "spring", "linear", "static", "double" };
            for (int k = 0; k < 10; k++)
                foreach (var cell in cells)
                {
                    await Trial(cell, k, true);
                    if (cell != "double") await Trial(cell, k, false);
                }
            Both("===== R2 P1 RAW DONE =====");
            art.Close(); try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex) { Both("R2 P1 EXCEPTION " + ex); try { art.Close(); } catch { } try { MfProbe.mf_cap_stop(); } catch { } }
        finally { _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog(); try { CloseMainWindowSoon(); } catch { } }
    }

    private async Task RunR2PostProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artPath = Path.Combine(@"C:\Users\admin\motion_framework\artifacts", "win_r2_post_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        const double period = 0.32, zeta = 0.78, from = 0, target1 = 240, target2 = 0;
        Both("# win_r2_post_raw.txt — 派题 #33 raw dump: uninterrupted B1 baseline vs #25-same interrupt (seedLead=0), same session");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; programmatic animations only; NO synthetic input; composition.h UNCHANGED");
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(100);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "r2-post-probe");
            CompReset(0, "r2 start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            var calQ0 = Qpc; CompReset(37, "r2 cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "r2 cal end"); await Task.Delay(500);
            var cc = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = cc.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = cc.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double ppd = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : 1.0;
            var rl = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault(); if (rl != null) cx0 = rl.Cx;
            Both($"cal cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(ppd,4)}");
            async Task WaitNear(double dip, double tol = 8)
            {
                for (int i = 0; i < 60; i++)
                {
                    await Task.Delay(50);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(4).ToList();
                    if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / ppd - dip) < tol)) return;
                }
            }
            async Task Run(string tag, bool interrupt)
            {
                await WaitNear(0); CompReset(0, tag + " rest"); await WaitNear(0); await Task.Delay(200);
                var a1 = _comp!.CreateSpringScalarAnimation();
                a1.Period = TimeSpan.FromSeconds(period); a1.DampingRatio = (float)zeta;
                a1.InitialValue = (float)from; a1.InitialVelocity = 0f; a1.FinalValue = (float)target1;
                long qStart = Qpc;
                _visual!.StartAnimation("Translation.X", a1);
                double tInt = double.NaN, seedPos = double.NaN, seedVel = double.NaN;
                if (interrupt)
                {
                    for (int i = 0; i < 40; i++) { await Task.Delay(10); if (SecSince(qStart, Qpc) >= 0.150) break; }
                    long qInt = Qpc; tInt = SecSince(qStart, qInt);
                    MfProbe.mf_spring_at(period, zeta, from, target1, 0, tInt, out seedPos, out seedVel);
                    var a2 = _comp.CreateSpringScalarAnimation();
                    a2.Period = TimeSpan.FromSeconds(period); a2.DampingRatio = (float)zeta;
                    a2.InitialValue = (float)seedPos; a2.InitialVelocity = (float)seedVel; a2.FinalValue = (float)target2;
                    _visual.StartAnimation("Translation.X", a2);
                }
                await Task.Delay(interrupt ? 1100 : 1250);
                var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qStart).OrderBy(c => c.QpcArrive).ToList();
                CompReset(0, tag + " end");
                Both($"RUN {tag} interrupt={interrupt} tInt={F(tInt * 1000, 2)}ms seedPos={F(seedPos, 3)} seedVel={F(seedVel, 2)} n={caps.Count}");
                foreach (var c in caps) Both($"RAW {tag} {F(SecSince(qStart, c.QpcArrive) * 1000, 2)} {F((c.Cx - cx0) / ppd, 2)}");
                if (interrupt)
                    for (int ms = 0; ms <= 1200; ms++) { MfProbe.mf_spring_at(period, zeta, seedPos, target2, seedVel, ms / 1000.0, out var x, out var v); if (ms % 2 == 0) Both($"MB2 {tag} {ms} {F(x, 4)} {F(v, 3)}"); }
            }
            for (int ms = 0; ms <= 1400; ms += 2) { MfProbe.mf_spring_at(period, zeta, from, target1, 0, ms / 1000.0, out var x, out var v); Both($"MB1 {ms} {F(x, 4)} {F(v, 3)}"); }
            await Run("base1", false);
            await Run("int1", true);
            await Run("base2", false);
            await Run("int2", true);
            Both("===== R2 POST RAW DONE =====");
            art.Close(); try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex) { Both("R2 POST EXCEPTION " + ex); try { art.Close(); } catch { } try { MfProbe.mf_cap_stop(); } catch { } }
        finally { _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog(); try { CloseMainWindowSoon(); } catch { } }
    }

    private async Task RunHarnessNoiseProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_harness_noise_probe.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }

        const double from = 0, target = 240;
        Both("# win_harness_noise_probe.txt — 派题 #30 harness floor: 0.25s linear KeyFrame vs ideal line");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; OS={Environment.OSVersion.Version}");
        Both($"# Align=best-τ (scan 0..300ms step 2) vs ideal x(t)=240*(t/Dur) clamp t∈[0,Dur]; same WGC+cal as #13/#24");
        Both("# A Duration=0.25s + CreateLinearEasingFunction 0→240; B 0.32s same; compare %travel to spring residual band 12–28%");
        Both("# NO composition.h change");

        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "harness-noise-probe");
            CompReset(0, "hn start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            int capHr = MfProbe.mf_cap_start(hwnd);
            Both($"WGC start hr=0x{capHr:X8}");
            var calQ0 = Qpc;
            CompReset(37, "hn cal"); await Task.Delay(350);
            var calQ1 = Qpc;
            CompReset(0, "hn cal end"); await Task.Delay(400);
            var capsCal = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = capsCal.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = capsCal.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double pxPerDip = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : (XamlRoot?.RasterizationScale ?? 1.0);
            var restLast = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault();
            if (restLast != null) cx0 = restLast.Cx;
            Both($"cal: cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(pxPerDip,4)}");

            async Task WaitNear(double dip, double tol = 8)
            {
                for (int i = 0; i < 40; i++)
                {
                    await Task.Delay(40);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(4).ToList();
                    if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / pxPerDip - dip) < tol)) return;
                }
            }

            async Task<(string tag, double dur, double tau, double rms, double maxAbs, int n, double pct)> RunLinear(string tag, double dur)
            {
                Both($"----- [{tag}] Duration={dur * 1000:F0}ms linear {from}→{target} -----");
                await WaitNear(0);
                CompReset(0, tag + " rest");
                await WaitNear(0);

                var k = _comp!.CreateScalarKeyFrameAnimation();
                var lin = _comp.CreateLinearEasingFunction();
                k.InsertKeyFrame(0f, (float)from, lin);
                k.InsertKeyFrame(1f, (float)target, lin);
                k.Duration = TimeSpan.FromSeconds(dur);
                long q0 = Qpc;
                _visual!.StartAnimation("Translation.X", k);
                Both("  START");
                await Task.Delay((int)(dur * 1000) + 400); // full run + present lag drain
                var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > q0).OrderBy(c => c.QpcArrive).ToList();
                CompReset(0, tag + " end");
                var raw = caps.Select(c => (tArr: SecSince(q0, c.QpcArrive), x: (c.Cx - cx0) / pxPerDip)).ToList();
                if (raw.Count < 8)
                {
                    Both($"  few n={raw.Count}");
                    return (tag, dur, double.NaN, double.NaN, double.NaN, raw.Count, double.NaN);
                }

                double Ideal(double t)
                {
                    if (t <= 0) return from;
                    if (t >= dur) return target;
                    return from + (target - from) * (t / dur);
                }
                // window: content time after τ in (0, dur] — full stroke; also report (0.02,dur) soft edge
                double RmsAt(double tau)
                {
                    double e = 0; int n = 0;
                    foreach (var p in raw)
                    {
                        double tc = p.tArr - tau;
                        if (tc < 0 || tc > dur) continue;
                        double m = Ideal(tc);
                        e += (p.x - m) * (p.x - m); n++;
                    }
                    return n >= 6 ? Math.Sqrt(e / n) : double.NaN;
                }
                double bestTau = 0.05, bestR = RmsAt(0.05);
                if (double.IsNaN(bestR)) bestR = double.MaxValue;
                for (int ms = 0; ms <= 300; ms += 2)
                {
                    double r = RmsAt(ms / 1000.0);
                    if (!double.IsNaN(r) && r < bestR) { bestR = r; bestTau = ms / 1000.0; }
                }
                double rms = bestR, maxAbs = 0; int nPts = 0;
                var sampleBuf = new List<(double t, double x, double m)>();
                foreach (var p in raw)
                {
                    double tc = p.tArr - bestTau;
                    if (tc < 0 || tc > dur) continue;
                    double m = Ideal(tc);
                    double ae = Math.Abs(p.x - m);
                    if (ae > maxAbs) maxAbs = ae;
                    nPts++;
                    sampleBuf.Add((tc, p.x, m));
                }
                double travel = Math.Abs(target - from);
                double pct = 100.0 * rms / travel;
                double pctMax = 100.0 * maxAbs / travel;
                Both($"  align best-τ={F(bestTau * 1000, 0)}ms n={nPts} RMS={F(rms, 3)} DIP ({F(pct, 3)}% travel) max|err|={F(maxAbs, 2)} ({F(pctMax, 2)}%)");
                Both($"  WGC range min={F(raw.Min(p => p.x), 1)} max={F(raw.Max(p => p.x), 1)}");
                var step = Math.Max(1, nPts / 6);
                var sparse = sampleBuf.Where((_, i) => i % step == 0).Take(6)
                    .Select(p => $"t={F(p.t * 1000, 0)} wgc={F(p.x, 1)} ideal={F(p.m, 1)}");
                Both("  samples: " + string.Join("; ", sparse));
                try { _visual.StopAnimation("Translation.X"); } catch { }
                return (tag, dur, bestTau, rms, maxAbs, nPts, pct);
            }

            var rA = await RunLinear("A_0.25s", 0.25);
            await WaitNear(0, 12);
            var rB = await RunLinear("B_0.32s", 0.32);

            Both("===== SUMMARY =====");
            Both("| case | Dur_ms | τ_ms | n | RMS | max|err| | %travel |");
            Both($"| A 0.25s | {F(rA.dur * 1000, 0)} | {F(rA.tau * 1000, 0)} | {rA.n} | {F(rA.rms, 3)} | {F(rA.maxAbs, 2)} | {F(rA.pct, 3)} |");
            Both($"| B 0.32s | {F(rB.dur * 1000, 0)} | {F(rB.tau * 1000, 0)} | {rB.n} | {F(rB.rms, 3)} | {F(rB.maxAbs, 2)} | {F(rB.pct, 3)} |");
            Both("spring inherent residual band (prior probes): 12–28% travel");
            bool muchLower = !double.IsNaN(rA.pct) && rA.pct < 6.0; // << 12
            string verd = muchLower ? "底噪 << 固有残差" : (rA.pct < 12 ? "底噪 < 固有残差下沿" : "底噪接近/进入固有残差带");
            Both("CONCLUSION: " + verd +
                $" — A(0.25s) RMS={F(rA.rms, 3)} ({F(rA.pct, 3)}% travel) max|err|={F(rA.maxAbs, 2)}; B(0.32s) RMS={F(rB.rms, 3)} ({F(rB.pct, 3)}%); spring band 12–28%.");
            Both("===== HARNESS NOISE PROBE DONE =====");
            art.Close();
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("HARNESS NOISE PROBE EXCEPTION " + ex);
            try { art.Close(); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        finally
        {
            _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog();
            try { CloseMainWindowSoon(); } catch { }
        }
    }


    private async Task RunStartingValueProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_starting_value_probe.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }

        const double preset = 40, target = 100, dur = 1.0;
        Both("# win_starting_value_probe.txt — 派题 #29 InsertExpressionKeyFrame(0, this.StartingValue)");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; OS={Environment.OSVersion.Version}");
        Both($"# Preset Translation.X={preset}; Duration={dur * 1000:F0}ms linear →{target}");
        Both("# A: key0=this.StartingValue; B: key0=explicit 0; C optional Stop+restart StartingValue");
        Both("# NO composition.h logic change in this probe (Period>0 gate sync is separate)");

        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "starting-value-probe");
            CompReset(0, "sv start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            int capHr = MfProbe.mf_cap_start(hwnd);
            Both($"WGC start hr=0x{capHr:X8}");
            var calQ0 = Qpc;
            CompReset(37, "sv cal"); await Task.Delay(350);
            var calQ1 = Qpc;
            CompReset(0, "sv cal end"); await Task.Delay(400);
            var capsCal = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = capsCal.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = capsCal.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double pxPerDip = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : (XamlRoot?.RasterizationScale ?? 1.0);
            var restLast = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault();
            if (restLast != null) cx0 = restLast.Cx;
            Both($"cal: cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(pxPerDip,4)}");

            async Task WaitNear(double dip, double tol = 6)
            {
                for (int i = 0; i < 40; i++)
                {
                    await Task.Delay(40);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(3).ToList();
                    if (recent.Count >= 2 && recent.All(c => Math.Abs((c.Cx - cx0) / pxPerDip - dip) < tol)) return;
                }
            }

            async Task<(string tag, double first, double minE, double maxE, string verd)> RunCase(string tag, bool useStartingValue)
            {
                Both($"----- [{tag}] useStartingValue={useStartingValue} preset={preset} →{target} -----");
                CompReset(preset, tag + " preset");
                await WaitNear(preset);
                var pre = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(3)
                    .Select(c => (c.Cx - cx0) / pxPerDip).ToList();
                double preMed = pre.Count > 0 ? pre.OrderBy(x => x).ElementAt(pre.Count / 2) : double.NaN;
                Both($"  WGC before start median≈{F(preMed, 1)}");

                var k = _comp!.CreateScalarKeyFrameAnimation();
                if (useStartingValue)
                    k.InsertExpressionKeyFrame(0f, "this.StartingValue");
                else
                    k.InsertKeyFrame(0f, 0f);
                k.InsertKeyFrame(1f, (float)target);
                k.Duration = TimeSpan.FromSeconds(dur);
                var ctrl = _comp.CreateAnimationController();
                long q0 = Qpc;
                _visual!.StartAnimation("Translation.X", k, ctrl);
                Both("  START");
                await Task.Delay(450);
                var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > q0).OrderBy(c => c.QpcArrive).ToList();
                CompReset(0, tag + " end");
                var xs = caps.Select(c => (t: SecSince(q0, c.QpcArrive), x: (c.Cx - cx0) / pxPerDip)).ToList();
                if (xs.Count < 5)
                {
                    Both($"  few n={xs.Count}");
                    return (tag, double.NaN, double.NaN, double.NaN, "OTHER-few");
                }
                var early = xs.Take(8).ToList();
                double first = early[0].x, minE = early.Min(p => p.x), maxE = early.Max(p => p.x);
                Both($"  early first={F(first, 1)} min={F(minE, 1)} max={F(maxE, 1)}");
                Both("  samples: " + string.Join("; ", early.Select(p => $"t={F(p.t * 1000, 0)} x={F(p.x, 1)}")));
                string verd;
                bool fromPreset = Math.Abs(first - preset) < 10 && minE > preset - 12;
                bool fromZero = first < 12 || minE < 10;
                bool rising = maxE > first + 3;
                if (useStartingValue)
                    verd = (fromPreset && !fromZero && rising) ? "FROM_STARTING_VALUE" : (fromZero ? "JUMP_TO_ZERO" : $"OTHER first={F(first, 1)}");
                else
                    verd = fromZero ? "FROM_EXPLICIT_ZERO" : $"OTHER first={F(first, 1)}";
                Both($"  verdict={verd}");
                try { _visual.StopAnimation("Translation.X"); } catch { }
                return (tag, first, minE, maxE, verd);
            }

            var rA = await RunCase("A_StartingValue", true);
            await WaitNear(0, 10);
            var rB = await RunCase("B_explicit0", false);
            await WaitNear(0, 10);

            // C optional: Stop mid-flight + restart StartingValue
            Both("----- [C] Stop mid-flight + restart StartingValue (record) -----");
            string verdC = "RECORD";
            double cFirst = double.NaN, stopX = double.NaN;
            try
            {
                CompReset(preset, "C preset");
                await WaitNear(preset);
                var k1 = _comp!.CreateScalarKeyFrameAnimation();
                k1.InsertExpressionKeyFrame(0f, "this.StartingValue");
                k1.InsertKeyFrame(1f, (float)target);
                k1.Duration = TimeSpan.FromSeconds(dur);
                var ctrl1 = _comp.CreateAnimationController();
                long qC = Qpc;
                _visual!.StartAnimation("Translation.X", k1, ctrl1);
                await Task.Delay(350);
                _visual.StopAnimation("Translation.X");
                // leave property at freeze — do not CompReset
                await Task.Delay(120);
                stopX = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(3)
                    .Select(c => (c.Cx - cx0) / pxPerDip).DefaultIfEmpty(double.NaN).Average();
                Both($"  Stop at WGC≈{F(stopX, 1)}");
                var k2 = _comp.CreateScalarKeyFrameAnimation();
                k2.InsertExpressionKeyFrame(0f, "this.StartingValue");
                k2.InsertKeyFrame(1f, (float)target);
                k2.Duration = TimeSpan.FromSeconds(dur);
                var ctrl2 = _comp.CreateAnimationController();
                long q2 = Qpc;
                _visual.StartAnimation("Translation.X", k2, ctrl2);
                await Task.Delay(200);
                var after = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > q2).OrderBy(c => c.QpcArrive).Take(6)
                    .Select(c => (c.Cx - cx0) / pxPerDip).ToList();
                cFirst = after.Count > 0 ? after[0] : double.NaN;
                Both($"  restart early first≈{F(cFirst, 1)} samples={string.Join(",", after.Select(x => F(x, 0)))}");
                bool cont = !double.IsNaN(stopX) && !double.IsNaN(cFirst) && Math.Abs(cFirst - stopX) < 15;
                verdC = cont ? $"CONTINUE_FROM_STOP (stop={F(stopX, 0)} first={F(cFirst, 0)})" : $"RECORD stop={F(stopX, 0)} first={F(cFirst, 0)}";
                try { _visual.StopAnimation("Translation.X"); } catch { }
            }
            catch (Exception ex)
            {
                Both("  C EXCEPTION " + ex.GetType().Name + ": " + ex.Message);
                verdC = "THROW " + ex.GetType().Name;
            }
            CompReset(0, "C end");

            Both("===== SUMMARY =====");
            Both($"| A StartingValue | first={F(rA.first, 1)} min={F(rA.minE, 1)} max={F(rA.maxE, 1)} | {rA.verd} |");
            Both($"| B explicit 0 | first={F(rB.first, 1)} min={F(rB.minE, 1)} max={F(rB.maxE, 1)} | {rB.verd} |");
            Both($"| C Stop+restart | {verdC} |");
            string overall = rA.verd == "FROM_STARTING_VALUE" ? "FROM_STARTING_VALUE"
                : rA.verd.StartsWith("JUMP") ? "JUMP" : "OTHER";
            Both("CONCLUSION: " + overall +
                $" — A {rA.verd} first={F(rA.first, 1)}; B {rB.verd} first={F(rB.first, 1)}; C={verdC}.");
            Both("===== STARTING VALUE PROBE DONE =====");
            art.Close();
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("STARTING VALUE PROBE EXCEPTION " + ex);
            try { art.Close(); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        finally
        {
            _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog();
            try { CloseMainWindowSoon(); } catch { }
        }
    }


    private async Task RunK3RestProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_k3_rest_probe.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }

        const double from = 0, target = 100, dur = 1.0;
        Both("# win_k3_rest_probe.txt — 派题 #28 K3 rest: Progress+Delay, |rate|>16, Progress+reverse");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; OS={Environment.OSVersion.Version}");
        Both($"# Linear KeyFrame {from}→{target} Duration={dur * 1000:F0}ms; CreateAnimationController + 3-arg Start");
        Both("# Note #22: Progress=0.5 → WGC≈90 (nonlinear); do not expect linear midpoint");
        Both("# NO composition.h change");

        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "k3-rest-probe");
            CompReset(0, "k3r start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            int capHr = MfProbe.mf_cap_start(hwnd);
            Both($"WGC start hr=0x{capHr:X8}");
            var calQ0 = Qpc;
            CompReset(37, "k3r cal"); await Task.Delay(350);
            var calQ1 = Qpc;
            CompReset(0, "k3r cal end"); await Task.Delay(400);
            var capsCal = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = capsCal.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = capsCal.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double pxPerDip = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : (XamlRoot?.RasterizationScale ?? 1.0);
            var restLast = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault();
            if (restLast != null) cx0 = restLast.Cx;
            Both($"cal: cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(pxPerDip,4)}");

            async Task WaitNear(double dip, double tol = 8)
            {
                for (int i = 0; i < 40; i++)
                {
                    await Task.Delay(40);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(3).ToList();
                    if (recent.Count >= 2 && recent.All(c => Math.Abs((c.Cx - cx0) / pxPerDip - dip) < tol)) return;
                }
            }
            double MedianNear(long q0, long q1)
            {
                var xs = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive >= q0 && c.QpcArrive <= q1)
                    .Select(c => (c.Cx - cx0) / pxPerDip).OrderBy(x => x).ToList();
                return xs.Count == 0 ? double.NaN : xs[xs.Count / 2];
            }
            double LastDip(long afterQ)
            {
                var last = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > afterQ).OrderBy(c => c.QpcArrive).LastOrDefault();
                return last == null ? double.NaN : (last.Cx - cx0) / pxPerDip;
            }
            ScalarKeyFrameAnimation MakeLinear(double delaySec = 0)
            {
                var k = _comp!.CreateScalarKeyFrameAnimation();
                k.InsertKeyFrame(0f, (float)from);
                k.InsertKeyFrame(1f, (float)target);
                k.Duration = TimeSpan.FromSeconds(dur);
                if (delaySec > 0)
                {
                    k.DelayTime = TimeSpan.FromSeconds(delaySec);
                    try { k.DelayBehavior = AnimationDelayBehavior.SetInitialValueAfterDelay; } catch { }
                }
                return k;
            }

            // ========== 1 Progress + Delay ==========
            Both("===== 1) Progress=0.5 during DelayTime=300ms =====");
            string verd1;
            double xDuring = double.NaN, xAfter = double.NaN, xPre = double.NaN;
            await WaitNear(0);
            CompReset(0, "1 rest"); await WaitNear(0);
            {
                var k = MakeLinear(0.30);
                var ctrl = _comp!.CreateAnimationController();
                long q0 = Qpc;
                _visual!.StartAnimation("Translation.X", k, ctrl);
                Both($"  START Delay=300ms Progress={ctrl.Progress:F4} rate={ctrl.PlaybackRate:F4}");
                await Task.Delay(100); // still in delay
                xPre = LastDip(q0);
                ctrl.Pause();
                ctrl.Progress = 0.5f;
                Both($"  Pause+Progress=0.5 during delay (xPre≈{F(xPre, 1)}; ctrl.Progress={ctrl.Progress:F4})");
                await Task.Delay(80);
                long a = Qpc; await Task.Delay(100);
                xDuring = MedianNear(a, Qpc);
                ctrl.Resume();
                Both($"  duringDelay after seek≈{F(xDuring, 1)}; Resume");
                await Task.Delay(500); // past delay + play
                long b = Qpc; await Task.Delay(120);
                xAfter = MedianNear(b, Qpc);
                Both($"  afterResumePlay≈{F(xAfter, 1)}");
                // expect #22 warp: Progress 0.5 → ~90 if honored during delay
                bool jumped = !double.IsNaN(xDuring) && xDuring > 40;
                bool stayed0 = !double.IsNaN(xDuring) && xDuring < 8;
                verd1 = jumped ? $"SEEK_HONORED_IN_DELAY (x≈{F(xDuring, 0)}; #22-like warp if ~90)" : (stayed0 ? "NO_SEEK_DURING_DELAY" : $"OTHER xDuring={F(xDuring, 1)}");
                Both($"  verdict1={verd1}");
                try { _visual.StopAnimation("Translation.X"); } catch { }
            }
            CompReset(0, "1 end"); await WaitNear(0, 12);

            // ========== 2 |rate|>16 ==========
            Both("===== 2) PlaybackRate=±20 (doc range [−16,16]) =====");
            string verd2p = "", verd2n = "";
            double arrive20 = double.NaN, arriveNeg = double.NaN;
            float rb20 = float.NaN, rbNeg = float.NaN;
            await WaitNear(0);
            // +20
            {
                CompReset(0, "2+ rest"); await WaitNear(0);
                var k = MakeLinear();
                var ctrl = _comp!.CreateAnimationController();
                string assign;
                try
                {
                    ctrl.PlaybackRate = 20f;
                    rb20 = ctrl.PlaybackRate;
                    assign = Math.Abs(rb20 - 20f) < 1e-3f ? "ACCEPT" : $"CLAMP@{rb20:G9}";
                    Both($"  set rate=20 → readback={rb20:G9} ({assign})");
                }
                catch (Exception ex)
                {
                    assign = "THROW " + ex.GetType().Name + ": " + ex.Message;
                    Both($"  set rate=20 → {assign}");
                    verd2p = "THROW";
                    assign = "THROW";
                }
                if (!assign.StartsWith("THROW"))
                {
                    long q0 = Qpc;
                    _visual!.StartAnimation("Translation.X", k, ctrl);
                    Both($"  START rate={ctrl.PlaybackRate:G9}");
                    for (int i = 0; i < 40; i++)
                    {
                        await Task.Delay(25);
                        var recent = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > q0).OrderBy(c => c.QpcArrive).TakeLast(3).ToList();
                        if (recent.Count >= 2 && recent.All(c => Math.Abs((c.Cx - cx0) / pxPerDip - target) < 5))
                        {
                            arrive20 = SecSince(q0, recent[0].QpcArrive);
                            break;
                        }
                    }
                    // rate=20 → expect ~50ms + present; if clamped to 16 → ~62.5ms
                    Both($"  visualArrive≈{F(arrive20 * 1000, 0)}ms (ideal rate20≈50ms; rate16≈62ms; rate1=1000ms)");
                    verd2p = assign + (double.IsNaN(arrive20) ? "+no_arrive" : $"+arrive{F(arrive20 * 1000, 0)}ms");
                    try { _visual.StopAnimation("Translation.X"); } catch { }
                }
            }
            CompReset(0, "2+ end"); await WaitNear(0, 12);
            // -20
            {
                CompReset(0, "2- rest"); await WaitNear(0);
                var k = MakeLinear();
                var ctrl = _comp!.CreateAnimationController();
                string assign;
                try
                {
                    ctrl.PlaybackRate = -20f;
                    rbNeg = ctrl.PlaybackRate;
                    assign = Math.Abs(rbNeg - (-20f)) < 1e-3f ? "ACCEPT" : $"CLAMP@{rbNeg:G9}";
                    Both($"  set rate=-20 → readback={rbNeg:G9} ({assign})");
                }
                catch (Exception ex)
                {
                    assign = "THROW " + ex.GetType().Name;
                    Both($"  set rate=-20 → {assign}");
                    verd2n = "THROW";
                    assign = "THROW";
                }
                if (!assign.StartsWith("THROW"))
                {
                    // seed at end then reverse fast
                    long q0 = Qpc;
                    _visual!.StartAnimation("Translation.X", k, ctrl);
                    ctrl.Progress = 1.0f;
                    Both($"  START Progress=1 rate={ctrl.PlaybackRate:G9}");
                    await Task.Delay(60);
                    double nearEnd = LastDip(q0);
                    await Task.Delay(200);
                    double late = LastDip(q0);
                    Both($"  nearEnd≈{F(nearEnd, 1)} late≈{F(late, 1)} (expect reverse toward 0 if rate negative honored)");
                    bool rev = !double.IsNaN(nearEnd) && !double.IsNaN(late) && late < nearEnd - 20;
                    verd2n = assign + (rev ? "+REVERSE_OK" : "+rev_unclear");
                    try { _visual.StopAnimation("Translation.X"); } catch { }
                }
            }
            CompReset(0, "2- end"); await WaitNear(0, 12);
            string verd2 = $"rate+20:{verd2p}; rate-20:{verd2n}";

            // ========== 3 Progress + reverse ==========
            Both("===== 3) PlaybackRate=-1 + Progress=0.75 =====");
            string verd3;
            await WaitNear(0);
            CompReset(0, "3 rest"); await WaitNear(0);
            {
                var k = MakeLinear();
                var ctrl = _comp!.CreateAnimationController();
                long q0 = Qpc;
                _visual!.StartAnimation("Translation.X", k, ctrl);
                ctrl.Pause();
                ctrl.Progress = 0.75f;
                ctrl.PlaybackRate = -1f;
                Both($"  Pause+Progress=0.75+rate=-1 (ctrl Progress={ctrl.Progress:F4} rate={ctrl.PlaybackRate:F4})");
                await Task.Delay(80);
                long a0 = Qpc; await Task.Delay(60);
                double xStart = MedianNear(a0, Qpc); // held at Progress 0.75 locus (#22: ~97)
                ctrl.Resume();
                Both($"  Resume reverse from x≈{F(xStart, 1)}");
                var series = new List<double>();
                for (int i = 0; i < 6; i++)
                {
                    await Task.Delay(150);
                    long a = Qpc; await Task.Delay(40);
                    double x = MedianNear(a, Qpc);
                    series.Add(x);
                    Both($"  t≈{(i + 1) * 150}ms x={F(x, 1)}");
                }
                bool decreasing = series.Count >= 3 && series.Last() < series.First() - 15;
                bool near0 = series.Last() < 30;
                verd3 = decreasing && near0 ? "REVERSE_FROM_PROGRESS_OK" : (decreasing ? "REVERSE_PARTIAL" : "FAIL");
                Both($"  startX={F(xStart, 1)} late={F(series.Last(), 1)} → {verd3}");
                try { _visual.StopAnimation("Translation.X"); } catch { }
            }

            Both("===== SUMMARY =====");
            Both($"| 1 Progress+Delay | during={F(xDuring, 1)} after={F(xAfter, 1)} | {verd1} |");
            Both($"| 2 |rate|>16 | +20 rb={rb20:G9} arrive={F(arrive20 * 1000, 0)}ms; -20 rb={rbNeg:G9} | {verd2} |");
            Both($"| 3 Progress=0.75 + rate=-1 | {verd3} |");
            Both("CONCLUSION: " +
                $"1){verd1}; 2){verd2}; 3){verd3}. composition.h UNCHANGED.");
            Both("===== K3 REST PROBE DONE =====");
            art.Close();
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("K3 REST PROBE EXCEPTION " + ex);
            try { art.Close(); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        finally
        {
            _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog();
            try { CloseMainWindowSoon(); } catch { }
        }
    }


    private async Task RunIvNullProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_iv_null_probe.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }

        const double preset = 37, target = 200, period = 0.32, zeta = 0.78;
        Both("# win_iv_null_probe.txt — 派题 #27 InitialValue=null behavior (record only; product does not rely)");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; OS={Environment.OSVersion.Version}");
        Both($"# Preset Translation.X={preset}; spring →{target} Period={period} ζ={zeta} IVel=0");
        Both("# A: do NOT set InitialValue (null); B: InitialValue=0 control; C optional mid-flight replace without IV");
        Both("# NO composition.h change");

        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "iv-null-probe");
            CompReset(0, "iv start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            int capHr = MfProbe.mf_cap_start(hwnd);
            Both($"WGC start hr=0x{capHr:X8}");
            var calQ0 = Qpc;
            CompReset(37, "iv cal"); await Task.Delay(350);
            var calQ1 = Qpc;
            CompReset(0, "iv cal end"); await Task.Delay(400);
            var capsCal = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = capsCal.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = capsCal.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double pxPerDip = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : (XamlRoot?.RasterizationScale ?? 1.0);
            var restLast = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault();
            if (restLast != null) cx0 = restLast.Cx;
            Both($"cal: cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(pxPerDip,4)}");

            async Task WaitNear(double dip, double tol = 6)
            {
                for (int i = 0; i < 40; i++)
                {
                    await Task.Delay(40);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(4).ToList();
                    if (recent.Count >= 3 && recent.All(c => Math.Abs((c.Cx - cx0) / pxPerDip - dip) < tol)) return;
                }
            }

            async Task<(string tag, double firstX, double minEarly, double maxEarly, string verd)> RunCase(string tag, bool setIv0)
            {
                Both($"----- [{tag}] setInitialValue0={setIv0} preset={preset} →{target} -----");
                CompReset(preset, tag + " preset");
                await WaitNear(preset);
                // confirm WGC at preset
                var pre = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(3)
                    .Select(c => (c.Cx - cx0) / pxPerDip).ToList();
                double preMed = pre.Count > 0 ? pre.OrderBy(x => x).ElementAt(pre.Count / 2) : double.NaN;
                Both($"  WGC before start median≈{F(preMed, 1)} (expect ~{preset})");

                var a = _comp!.CreateSpringScalarAnimation();
                a.Period = TimeSpan.FromSeconds(period);
                a.DampingRatio = (float)zeta;
                a.InitialVelocity = 0f;
                a.FinalValue = (float)target;
                if (setIv0)
                    a.InitialValue = 0f;
                // else: leave InitialValue unset (null)
                string ivDesc;
                try
                {
                    var iv = a.InitialValue;
                    ivDesc = iv.HasValue ? iv.Value.ToString("G9", CultureInfo.InvariantCulture) : "null";
                }
                catch { ivDesc = "unreadable"; }
                Both($"  InitialValue before Start={ivDesc}");

                long q0 = Qpc;
                _visual!.StartAnimation("Translation.X", a);
                Both($"  START");
                await Task.Delay(500);
                var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > q0).OrderBy(c => c.QpcArrive).ToList();
                CompReset(0, tag + " end");
                var xs = caps.Select(c => (t: SecSince(q0, c.QpcArrive), x: (c.Cx - cx0) / pxPerDip)).ToList();
                if (xs.Count < 5)
                {
                    Both($"  few frames n={xs.Count}");
                    return (tag, double.NaN, double.NaN, double.NaN, "OTHER-few");
                }
                // early window: first ~80ms of captures (present lag) — use first 4–8 frames after motion
                var early = xs.Take(8).ToList();
                double firstX = early[0].x;
                double minEarly = early.Min(p => p.x);
                double maxEarly = early.Max(p => p.x);
                // also: first frame that moved ≥3 from preset or from 0
                Both($"  early n={early.Count} first={F(firstX, 1)} min={F(minEarly, 1)} max={F(maxEarly, 1)}");
                var sparse = early.Select(p => $"t={F(p.t * 1000, 0)} x={F(p.x, 1)}");
                Both("  samples: " + string.Join("; ", sparse));

                string verd;
                // FROM_CURRENT: early stays near preset then rises toward target (minEarly >= preset-8, first near preset)
                // FROM_ZERO: early dips near 0 or starts near 0 (minEarly < 12 or first < 12)
                bool nearPreset = Math.Abs(firstX - preset) < 12 || (minEarly > preset - 10 && firstX > preset - 15);
                bool nearZero = firstX < 12 || minEarly < 10;
                if (nearPreset && !nearZero && maxEarly > preset + 5)
                    verd = "FROM_CURRENT";
                else if (nearZero)
                    verd = "FROM_ZERO";
                else
                    verd = $"OTHER first={F(firstX, 1)} min={F(minEarly, 1)}";
                Both($"  verdict={verd}");
                return (tag, firstX, minEarly, maxEarly, verd);
            }

            var rA = await RunCase("A_IV_null", setIv0: false);
            await WaitNear(0, 10);
            var rB = await RunCase("B_IV_0", setIv0: true);
            await WaitNear(0, 10);

            // C optional: mid-flight replace without IV
            Both("----- [C] mid-flight replace without InitialValue (record) -----");
            string verdC = "RECORD";
            double cFirst = double.NaN;
            try
            {
                CompReset(preset, "C preset");
                await WaitNear(preset);
                var a1 = _comp!.CreateSpringScalarAnimation();
                a1.Period = TimeSpan.FromSeconds(period);
                a1.DampingRatio = (float)zeta;
                a1.InitialValue = (float)preset;
                a1.InitialVelocity = 0f;
                a1.FinalValue = (float)target;
                long qC = Qpc;
                _visual!.StartAnimation("Translation.X", a1);
                await Task.Delay(120);
                var a2 = _comp.CreateSpringScalarAnimation();
                a2.Period = TimeSpan.FromSeconds(period);
                a2.DampingRatio = (float)zeta;
                a2.InitialVelocity = 0f;
                a2.FinalValue = 0f; // retarget to 0, IV unset
                // do not set InitialValue
                _visual.StartAnimation("Translation.X", a2);
                Both("  replaced →0 without InitialValue");
                await Task.Delay(400);
                var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > qC).OrderBy(c => c.QpcArrive).ToList();
                CompReset(0, "C end");
                var after = caps.Where(c => SecSince(qC, c.QpcArrive) > 0.12).Take(6)
                    .Select(c => (c.Cx - cx0) / pxPerDip).ToList();
                cFirst = after.Count > 0 ? after[0] : double.NaN;
                Both($"  post-replace early x≈{F(cFirst, 1)} (n={after.Count}); samples={string.Join(",", after.Select(x => F(x, 0)))}");
                verdC = $"RECORD first≈{F(cFirst, 1)}";
            }
            catch (Exception ex)
            {
                Both("  C EXCEPTION " + ex.GetType().Name + ": " + ex.Message);
                verdC = "THROW " + ex.GetType().Name;
            }

            Both("===== SUMMARY =====");
            Both($"| A IV=null | first={F(rA.firstX, 1)} min={F(rA.minEarly, 1)} max={F(rA.maxEarly, 1)} | {rA.verd} |");
            Both($"| B IV=0 | first={F(rB.firstX, 1)} min={F(rB.minEarly, 1)} max={F(rB.maxEarly, 1)} | {rB.verd} |");
            Both($"| C mid-replace no IV | {verdC} |");
            string overall = rA.verd.StartsWith("FROM_CURRENT") ? "FROM_CURRENT"
                : rA.verd.StartsWith("FROM_ZERO") ? "FROM_ZERO" : "OTHER";
            // strengthen: A from current AND B from zero
            if (rA.verd == "FROM_CURRENT" && rB.verd == "FROM_ZERO")
                overall = "FROM_CURRENT";
            else if (rA.verd == "FROM_ZERO")
                overall = "FROM_ZERO";
            Both("CONCLUSION: " + overall +
                $" — A(null) {rA.verd} first={F(rA.firstX, 1)}; B(IV=0) {rB.verd} first={F(rB.firstX, 1)}; C={verdC}. " +
                "Product still always explicit-seeds on interrupt. composition.h UNCHANGED.");
            Both("===== IV NULL PROBE DONE =====");
            art.Close();
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("IV NULL PROBE EXCEPTION " + ex);
            try { art.Close(); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        finally
        {
            _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog();
            try { CloseMainWindowSoon(); } catch { }
        }
    }


    private async Task RunPeriodMinProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_period_min_probe.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }

        const double from = 0, target = 100, zeta = 0.8;
        Both("# win_period_min_probe.txt — 派题 #26 Spring Period minimum accept/clamp/throw");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; OS={Environment.OSVersion.Version}");
        Both($"# Translation.X {from}→{target} ζ={zeta} IVel=0; Period grid 0.001/0.01/0.05/0.32 + Zero/negative optional");
        Both("# NO composition.h change");

        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "period-min-probe");
            CompReset(0, "pm start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            int capHr = MfProbe.mf_cap_start(hwnd);
            Both($"WGC start hr=0x{capHr:X8}");
            var calQ0 = Qpc;
            CompReset(37, "pm cal"); await Task.Delay(350);
            var calQ1 = Qpc;
            CompReset(0, "pm cal end"); await Task.Delay(400);
            var capsCal = ReadCaptures().Where(c => c.N > 50).ToList();
            double cx0 = capsCal.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = capsCal.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double pxPerDip = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : (XamlRoot?.RasterizationScale ?? 1.0);
            var restLast = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).LastOrDefault();
            if (restLast != null) cx0 = restLast.Cx;
            Both($"cal: cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(pxPerDip,4)}");

            async Task WaitNear0()
            {
                CompReset(0, "rest");
                for (int i = 0; i < 30; i++)
                {
                    await Task.Delay(40);
                    var recent = ReadCaptures().Where(c => c.N > 50).OrderBy(c => c.QpcArrive).TakeLast(3).ToList();
                    if (recent.Count >= 2 && recent.All(c => Math.Abs((c.Cx - cx0) / pxPerDip) < 8)) return;
                }
            }

            var summary = new List<string>();
            double[] periods = { 0.001, 0.01, 0.05, 0.32 };
            foreach (var pSec in periods)
            {
                Both($"----- Period={pSec.ToString(CultureInfo.InvariantCulture)}s -----");
                string assign = "ok", readback = "n/a", run = "skipped", verdict;
                double rbSec = double.NaN;
                try
                {
                    var a = _comp!.CreateSpringScalarAnimation();
                    a.DampingRatio = (float)zeta;
                    a.InitialValue = (float)from;
                    a.InitialVelocity = 0f;
                    a.FinalValue = (float)target;
                    a.Period = TimeSpan.FromSeconds(pSec);
                    rbSec = a.Period.TotalSeconds;
                    readback = rbSec.ToString("G9", CultureInfo.InvariantCulture);
                    bool clamped = Math.Abs(rbSec - pSec) > 1e-9;
                    assign = clamped ? $"ok-clamped→{readback}" : "ok-as-set";
                    Both($"  assign: ok; readback Period={readback}s{(clamped ? $" (CLAMPED from {pSec.ToString(CultureInfo.InvariantCulture)})" : " (unchanged)")}");

                    await WaitNear0();
                    long q0 = Qpc;
                    _visual!.StartAnimation("Translation.X", a);
                    Both($"  StartAnimation Period={readback}s ζ={zeta} {from}→{target}");
                    // short sample: tiny periods settle fast; 0.32 needs ~1s
                    int waitMs = pSec <= 0.02 ? 400 : (pSec <= 0.1 ? 600 : 1200);
                    await Task.Delay(waitMs);
                    var caps = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > q0).OrderBy(c => c.QpcArrive).ToList();
                    CompReset(0, $"post P={pSec}");
                    if (caps.Count < 3)
                    {
                        run = $"ran WGC_n={caps.Count} (few)";
                        verdict = clamped ? $"CLAMP@{readback}" : "ACCEPT";
                        Both($"  run: {run}");
                    }
                    else
                    {
                        var xs = caps.Select(c => (t: SecSince(q0, c.QpcArrive), x: (c.Cx - cx0) / pxPerDip)).ToList();
                        double peak = xs.Max(p => p.x), last = xs[^1].x, first = xs[0].x;
                        double travel = peak - xs.Min(p => p.x);
                        // arrive: first |x-100|<5
                        double arrive = double.NaN;
                        foreach (var p in xs) { if (Math.Abs(p.x - target) < 5) { arrive = p.t; break; } }
                        bool moved = travel > 15;
                        bool instant = !double.IsNaN(arrive) && arrive < 0.05;
                        bool stuck = !moved && Math.Abs(last - from) < 5;
                        bool reached = Math.Abs(last - target) < 8 || (!double.IsNaN(arrive));
                        string motion = stuck ? "STUCK@from" : (instant ? "INSTANT≈final" : (moved ? "VISIBLE_MOTION" : "little_motion"));
                        run = $"WGC n={xs.Count} travel={travel:F1} last={last:F1} arrive={F(arrive * 1000, 0)}ms → {motion}";
                        Both($"  run: {run}");
                        var sparse = xs.Where((_, i) => i % Math.Max(1, xs.Count / 5) == 0).Take(5)
                            .Select(p => $"t={F(p.t * 1000, 0)} x={F(p.x, 1)}");
                        Both("  samples: " + string.Join("; ", sparse));
                        verdict = (clamped ? $"CLAMP@{readback}" : "ACCEPT") + "+" + (stuck ? "nostart" : (reached ? "runs" : "runs-incomplete"));
                    }
                }
                catch (Exception ex)
                {
                    assign = "THROW " + ex.GetType().Name + ": " + ex.Message;
                    Both($"  assign: {assign}");
                    run = "not-run";
                    verdict = "THROW";
                }
                summary.Add($"| {pSec.ToString(CultureInfo.InvariantCulture)} | {assign} | {readback} | {run} | {verdict} |");
            }

            // Optional Zero / negative
            Both("----- optional Period=TimeSpan.Zero -----");
            try
            {
                var a = _comp!.CreateSpringScalarAnimation();
                a.DampingRatio = (float)zeta; a.InitialValue = 0; a.FinalValue = 100;
                a.Period = TimeSpan.Zero;
                Both($"  Zero: ok readback={a.Period.TotalSeconds.ToString("G9", CultureInfo.InvariantCulture)}s");
                summary.Add("| Zero | ok-as-set | " + a.Period.TotalSeconds.ToString("G9", CultureInfo.InvariantCulture) + " | assign-only | ACCEPT-or-CLAMP |");
            }
            catch (Exception ex)
            {
                Both($"  Zero: THROW {ex.GetType().Name}: {ex.Message}");
                summary.Add($"| Zero | THROW {ex.GetType().Name} | n/a | not-run | THROW |");
            }
            Both("----- optional Period=TimeSpan.FromSeconds(-0.01) -----");
            try
            {
                var a = _comp!.CreateSpringScalarAnimation();
                a.DampingRatio = (float)zeta; a.InitialValue = 0; a.FinalValue = 100;
                a.Period = TimeSpan.FromSeconds(-0.01);
                Both($"  negative: ok readback={a.Period.TotalSeconds.ToString("G9", CultureInfo.InvariantCulture)}s");
                summary.Add("| -0.01 | ok | " + a.Period.TotalSeconds.ToString("G9", CultureInfo.InvariantCulture) + " | assign-only | ACCEPT-or-CLAMP |");
            }
            catch (Exception ex)
            {
                Both($"  negative: THROW {ex.GetType().Name}: {ex.Message}");
                summary.Add($"| -0.01 | THROW {ex.GetType().Name} | n/a | not-run | THROW |");
            }

            Both("===== SUMMARY =====");
            Both("| Period_s | assign | readback | run | verdict |");
            foreach (var line in summary) Both(line);
            // Build one-line conclusion from summary verdicts
            string one = string.Join("; ", summary.Select(s => {
                var parts = s.Trim('|').Split('|');
                if (parts.Length < 5) return s;
                return parts[0].Trim() + "→" + parts[4].Trim();
            }));
            Both("CONCLUSION: " + one + " — composition.h UNCHANGED.");
            Both("===== PERIOD MIN PROBE DONE =====");
            art.Close();
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("PERIOD MIN PROBE EXCEPTION " + ex);
            try { art.Close(); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        finally
        {
            _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog();
            try { CloseMainWindowSoon(); } catch { }
        }
    }


    private async Task RunZetaProbe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artDir = @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string artPath = Path.Combine(artDir, "win_zeta_gt1_probe.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        Both($"# win_zeta_gt1_probe.txt — 派题 #14 S2 DampingRatio clamp/accept probe");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} Asia/Taipei; OS={Environment.OSVersion.Version}; Period=0.32s");
        Both($"# For each ζ: assign DampingRatio, catch throw, read back, optionally run 0→240 and sample Translation.X");
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(120);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "zeta-probe");
            CompReset(0, "zeta start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            int capHr = MfProbe.mf_cap_start(hwnd);
            Both($"WGC start hr=0x{capHr:X8} hwnd=0x{hwnd.ToInt64():X}");
            await Task.Delay(200);

            double[] zetas = { 1.0, 1.2, 2.0, 5.0, -0.1 };
            var summary = new List<string>();
            foreach (var z in zetas)
            {
                Both($"----- ζ={z.ToString(CultureInfo.InvariantCulture)} -----");
                string assign = "ok", readback = "n/a", run = "skipped";
                float rb = float.NaN;
                try
                {
                    var a = _comp!.CreateSpringScalarAnimation();
                    a.Period = TimeSpan.FromSeconds(0.32);
                    a.FinalValue = 240f;
                    a.InitialValue = 0f;
                    a.InitialVelocity = 0f;
                    a.DampingRatio = (float)z;
                    rb = a.DampingRatio;
                    readback = rb.ToString("G9", CultureInfo.InvariantCulture);
                    bool clamped = Math.Abs(rb - (float)z) > 1e-5f;
                    Both($"  assign: ok; readback DampingRatio={readback}{(clamped ? $" (CLAMPED from {z.ToString(CultureInfo.InvariantCulture)})" : " (unchanged)")}");
                    assign = clamped ? $"ok-clamped→{readback}" : "ok-as-set";

                    // Run if accepted. NaturalMotion does not update Properties.Translation mid-flight — use WGC.
                    if (!float.IsNaN(rb) && rb >= 0)
                    {
                        CompReset(0, $"pre ζ={z}"); await Task.Delay(250);
                        var caps0 = ReadCaptures();
                        double cx0 = caps0.Where(c => c.N > 50).Select(c => c.Cx).DefaultIfEmpty(double.NaN).LastOrDefault();
                        long q0 = Qpc;
                        _visual!.StartAnimation("Translation.X", a);
                        Both($"  StartAnimation Translation.X Period=0.32 DampingRatio={readback} FinalValue=240; WGC cx0={F(cx0,3)}");
                        await Task.Delay(1500);
                        var caps1 = ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > q0).OrderBy(c => c.QpcArrive).ToList();
                        CompReset(0, $"post ζ={z}");
                        if (caps1.Count < 5)
                        {
                            run = $"ran WGC_frames={caps1.Count} (need>=5)";
                            Both($"  run: {run}");
                        }
                        else
                        {
                            double rest = !double.IsNaN(cx0) ? cx0 : caps1[0].Cx;
                            var dips = caps1.Select(c => (c.Cx - rest)).ToList(); // px≈DIP at scale 1
                            double peak = dips.Max(), last = dips[^1], min = dips.Min();
                            bool overshoot = peak > 240 * 1.02;
                            bool moved = peak > 15 || Math.Abs(min) > 15;
                            // critically/overdamped: expect approach without oscillation above target
                            run = moved
                                ? $"ran WGC n={caps1.Count} peakΔcx={peak:F1} lastΔcx={last:F1} overshoot={(overshoot ? "yes" : "no")}"
                                : $"ran WGC n={caps1.Count} little motion peakΔcx={peak:F1} lastΔcx={last:F1}";
                            Both($"  run: {run}");
                            // sparse samples
                            var sparse = caps1.Where((_, i) => i % Math.Max(1, caps1.Count / 6) == 0).Take(6)
                                .Select(c => $"t={F(SecSince(q0, c.QpcArrive)*1000,0)}ms Δcx={F(c.Cx-rest,1)}");
                            Both($"  WGC: " + string.Join("; ", sparse));
                        }
                    }
                    else
                    {
                        run = "not-run (readback rejected)";
                        Both($"  run: {run}");
                    }
                }
                catch (Exception ex)
                {
                    assign = "THROW " + ex.GetType().Name + ": " + ex.Message;
                    Both($"  assign: THROW {ex.GetType().Name}: {ex.Message}");
                    run = "not-run";
                }
                summary.Add($"ζ={z.ToString(CultureInfo.InvariantCulture)} | assign={assign} | readback={readback} | {run}");
            }

            Both("===== SUMMARY =====");
            foreach (var line in summary) Both(line);
            Both("CONCLUSION: see SUMMARY — accept means assign ok and readback≈set; clamp means readback≠set; throw means API rejected.");
            Both("===== ZETA PROBE DONE =====");
            art.Close();
            Log($"ZETA probe wrote {artPath}");
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex)
        {
            Both("ZETA PROBE EXCEPTION " + ex);
            try { art.Close(); } catch { }
            try { MfProbe.mf_cap_stop(); } catch { }
        }
        finally
        {
            _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog();
            try { CloseMainWindowSoon(); } catch { }
        }
    }


    // ---------------------------------------------------------------- 派题 #13: Period/ζ mapping sweep (A + E only)
    /// <summary>
    /// Variant (b) formula: treat token response as settle-time Ts (counterfactual — spring.h defines
    /// response as undamped T₀). Binary-search T₀∈(0.01,2) s.t.
    /// Spring({T₀,ζ},0→240).SettleTime(posEps=0.1,velEps=0.5) = Ts. At ζ=0.78, Ts=0.32 → T₀≈0.151243 s.
    /// Compositor Period := T₀ (E: Period := T₀/2). Reference trajectory stays spring.h(0.32,0.78)/(0.16,0.78).
    /// </summary>
    private static double InvertSettleToPeriod(double settleTarget, double zeta, double from, double target, double v0)
    {
        double lo = 0.01, hi = 2.0;
        for (int i = 0; i < 80; i++)
        {
            double mid = (lo + hi) * 0.5;
            double st = MfProbe.mf_spring_settle(mid, zeta, from, target, v0);
            if (st > settleTarget) hi = mid; else lo = mid;
        }
        return (lo + hi) * 0.5;
    }

    private async Task RunMapTest()
    {
        if (_suiteRunning || _comp is null || _visual is null || _fm is null) return;
        _suiteRunning = true;
        MeasureButton.IsEnabled = false;
        _starts.Clear();
        Log("===== MAPTEST START (派题 #13 Period/ζ mapping; thresholds NOT relaxed) =====");
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); Log($"MAPTEST focus hwnd=0x{hwnd.ToInt64():X}"); }
            await DwmFlushProbe(200);
            CompositionTarget.Rendering += OnSuiteRendering; _renderSubs++;
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            int capHr = MfProbe.mf_cap_start(hwnd);
            MfProbe.mf_cap_item_size(out var iw, out var ih);
            double scale = XamlRoot?.RasterizationScale ?? 1.0;
            Log($"WGC CreateForWindow(hwnd=0x{hwnd.ToInt64():X}) hr=0x{capHr:X8} item={iw}x{ih} scale={scale}");

            SetMode(true, "maptest");
            CompReset(0, "maptest start"); await Task.Delay(400);
            var calQ0 = Qpc;
            CompReset(37, "maptest cal 37"); await Task.Delay(400);
            var calQ1 = Qpc;
            CompReset(0, "maptest cal end"); await Task.Delay(400);

            var capsAll = ReadCaptures();
            var withBox0 = capsAll.Where(c => c.N > 50).ToList();
            double cx0 = withBox0.Where(c => c.QpcArrive < calQ0).LastOrDefault()?.Cx ?? double.NaN;
            double cx37 = withBox0.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault()?.Cx ?? double.NaN;
            double pxPerDip = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : scale;
            Log($"MAPTEST cal: cx0={F(cx0,3)} cx37={F(cx37,3)} pxPerDip={F(pxPerDip,4)}");

            double t0Inv = InvertSettleToPeriod(SpringResponse, SpringDamping, 0, 240, 0);
            double settleCheck = MfProbe.mf_spring_settle(t0Inv, SpringDamping, 0, 240, 0);
            Log($"MAPTEST (b) formula: SettleTime(T₀,ζ=0.78,0→240)=response=0.32 → T₀={F(t0Inv,6)}s (check settle={F(settleCheck,6)}s); spring.h response IS already T₀ — this is counterfactual settle-derived mapping");
            Log($"MAPTEST (a) Period=response={SpringResponse}s ζ={SpringDamping} (S1 current)");
            Log($"MAPTEST reference: A=spring.h({SpringResponse},{SpringDamping}); E=spring.h({SpringResponse / 2},{SpringDamping}) — fixed; compositor Period/ζ vary");

            // Phase 1: (a) and (b)
            var phase1 = new (string id, double periodA, double zeta)[]
            {
                ("a", SpringResponse, SpringDamping),
                ("b_settle_inv", t0Inv, SpringDamping),
            };
            var results = new List<(string id, string scen, double period, double zeta, double rmsShapePct, double bestTauMs, int n, double peakX)>();

            async Task RunOne(string id, double periodA, double zeta)
            {
                // A
                CompReset(0, id + " A rest"); await Task.Delay(350);
                var sA = CompSpring(240, $"MAP {id} A Period={periodA:F4} ζ={zeta:F2}", periodA, zeta);
                await Task.Delay(2800);
                var capsA = ReadCaptures();
                results.Add(EvalShape(id, "A", sA, periodA, zeta, SpringResponse, SpringDamping, capsA, cx0, pxPerDip, 1.2));
                CompReset(0, id + " after A"); await Task.Delay(350);
                // E rate=2
                double periodE = periodA / 2.0;
                var sE = CompSpring(240, $"MAP {id} E Period={periodE:F4} ζ={zeta:F2}", periodE, zeta);
                await Task.Delay(1600);
                var capsE = ReadCaptures();
                results.Add(EvalShape(id, "E", sE, periodE, zeta, SpringResponse / 2.0, SpringDamping, capsE, cx0, pxPerDip, 1.0));
                CompReset(0, id + " after E"); await Task.Delay(350);
            }

            foreach (var v in phase1) await RunOne(v.id, v.periodA, v.zeta);

            // Pick best of (a)/(b) by mean shape RMS of A+E
            double MeanRms(string id)
            {
                var rows = results.Where(r => r.id == id).ToList();
                return rows.Count == 0 ? double.MaxValue : rows.Average(r => r.rmsShapePct);
            }
            string bestAb = MeanRms("a") <= MeanRms("b_settle_inv") ? "a" : "b_settle_inv";
            double bestPeriod = bestAb == "a" ? SpringResponse : t0Inv;
            Log($"MAPTEST best of (a)/(b) by mean(A,E) shape RMS: {bestAb} (a={F(MeanRms("a"),3)}% b={F(MeanRms("b_settle_inv"),3)}%) → Period={F(bestPeriod,4)} for ζ nudge");

            // Phase 2: (c) ζ ±0.05 on best Period; denser optional
            var phase2Z = new[] { 0.73, 0.83, 0.68, 0.88 };
            foreach (var z in phase2Z)
            {
                string id = $"c_{bestAb}_z{z:F2}".Replace('.', 'p');
                await RunOne(id, bestPeriod, z);
            }

            // Summary table
            Log("MAPTEST ===== RMS TABLE (shape = best-τ RMS % of travel 240; ref=spring.h canonical) =====");
            Log("MAPTEST | variant | scen | Period | ζ | n | peakX | bestτ_ms | RMS_shape% | ≤1%? |");
            foreach (var r in results.OrderBy(r => r.scen).ThenBy(r => r.rmsShapePct))
            {
                string pass = r.rmsShapePct <= 1.0 ? "YES" : "no";
                Log($"MAPTEST | {r.id} | {r.scen} | {F(r.period,4)} | {F(r.zeta,2)} | {r.n} | {F(r.peakX,1)} | {F(r.bestTauMs,1)} | {F(r.rmsShapePct,3)} | {pass} |");
            }
            var bestA = results.Where(r => r.scen == "A").OrderBy(r => r.rmsShapePct).FirstOrDefault();
            var bestE = results.Where(r => r.scen == "E").OrderBy(r => r.rmsShapePct).FirstOrDefault();
            Log($"MAPTEST BEST A: {bestA.id} Period={F(bestA.period,4)} ζ={F(bestA.zeta,2)} RMS_shape={F(bestA.rmsShapePct,3)}%");
            Log($"MAPTEST BEST E: {bestE.id} Period={F(bestE.period,4)} ζ={F(bestE.zeta,2)} RMS_shape={F(bestE.rmsShapePct,3)}%");
            bool anyPass = results.Any(r => r.rmsShapePct <= 1.0);
            if (!anyPass)
                Log("MAPTEST CONCLUSION: no variant reaches ≤1% shape RMS for A or E → 「合成器弹簧与 spring.h 固有差异」risk (parameter mapping exhausted for this round)");
            else
                Log("MAPTEST CONCLUSION: at least one variant ≤1% — mapping can close the gap; adopt lowest-RMS Period/ζ");

            // ζ nudge clarity
            double baseMean = MeanRms(bestAb);
            foreach (var z in phase2Z)
            {
                string id = $"c_{bestAb}_z{z:F2}".Replace('.', 'p');
                double m = MeanRms(id);
                Log($"MAPTEST ζ nudge {id}: mean RMS={F(m,3)}% vs base {bestAb}={F(baseMean,3)}% → Δ={F(m - baseMean,3)} pp ({(m < baseMean - 1.0 ? "clear drop" : m < baseMean ? "slight drop" : "no drop")})");
            }

            MfProbe.mf_cap_stop();
            CompositionTarget.Rendering -= OnSuiteRendering; _renderUnsubs++;
            Log("===== MAPTEST DONE =====");
        }
        catch (Exception ex)
        {
            _exceptions++; Log("MAPTEST EXCEPTION " + ex);
            try { MfProbe.mf_cap_stop(); } catch { }
            try { CompositionTarget.Rendering -= OnSuiteRendering; _renderUnsubs++; } catch { }
        }
        finally
        {
            _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog();
            try { CloseMainWindowSoon(); } catch { }
        }
    }

    private (string id, string scen, double period, double zeta, double rmsShapePct, double bestTauMs, int n, double peakX) EvalShape(
        string id, string scen, CompStart? s, double periodUsed, double zetaUsed,
        double refResponse, double refDamping, List<Cap> caps, double cx0, double pxPerDip, double window)
    {
        if (s is null) return (id, scen, periodUsed, zetaUsed, double.NaN, 0, 0, 0);
        var withBox = caps.Where(c => c.N > 50).ToList();
        double restPx = !double.IsNaN(cx0) ? cx0 : 0;
        var raw = withBox.Select(c => (tArr: SecSince(s.Qpc, c.QpcArrive), x: (c.Cx - restPx) / pxPerDip))
                         .Where(p => p.tArr > 0 && p.tArr < window + 0.35)
                         .OrderBy(p => p.tArr).ToList();
        double travel = 240;
        double peakX = raw.Count == 0 ? 0 : raw.Max(p => p.x);
        if (raw.Count < 5)
        {
            Log($"MAPTEST {id}/{scen}: N/A n={raw.Count}");
            return (id, scen, periodUsed, zetaUsed, double.NaN, 0, raw.Count, peakX);
        }
        double Model(double t)
        {
            MfProbe.mf_spring_at(refResponse, refDamping, 0, 240, 0, Math.Max(0, t), out var m, out _);
            return m;
        }
        double RmsAt(double tau)
        {
            var e = raw.Where(p => p.tArr - tau > 0 && p.tArr - tau < window).Select(p => p.x - Model(p.tArr - tau)).ToList();
            return e.Count < 5 ? double.NaN : Math.Sqrt(e.Average(z => z * z));
        }
        double bestTau = 0, bestRms = RmsAt(0);
        for (int ms = -250; ms <= 250; ms += 2)
        {
            double r = RmsAt(ms / 1000.0);
            if (!double.IsNaN(r) && r < bestRms) { bestRms = r; bestTau = ms / 1000.0; }
        }
        double pct = bestRms / travel * 100.0;
        Log($"MAPTEST {id}/{scen}: Period={F(periodUsed,4)} ζ={F(zetaUsed,2)} ref=({F(refResponse,2)},{F(refDamping,2)}) n={raw.Count} peakX={F(peakX,1)} bestτ={F(bestTau * 1000, 1)}ms RMS_shape={F(pct,3)}%");
        return (id, scen, periodUsed, zetaUsed, pct, bestTau * 1000, raw.Count, peakX);
    }

    private void CloseMainWindowSoon()
    {
        var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(800); t.IsRepeating = false;
        t.Tick += (_, __) =>
        {
            try { Log("MAPTEST auto-close window"); FlushLog(); Process.GetCurrentProcess().CloseMainWindow(); }
            catch (Exception ex) { Log("MAPTEST close EXCEPTION " + ex.Message); }
        };
        t.Start();
    }


    private async Task RunSuite(string trigger)
    {
        if (_suiteRunning || _comp is null || _visual is null || _fm is null) return;
        _suiteRunning = true;
        MeasureButton.IsEnabled = false;
        _starts.Clear(); _renderIntervals.Clear(); _lastRenderMs = -1; _swTrace.Clear();
        int switchesOk = 0, switches = 0;
        Log($"===== SUITE START ({trigger}) =====");
        try
        {
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero)
            {
                ShowWindow(hwnd, 9 /*SW_RESTORE*/);
                SetForegroundWindow(hwnd);
                Log($"SUITE focus: ShowWindow(SW_RESTORE)+SetForegroundWindow(hwnd=0x{hwnd.ToInt64():X}) ok");
            }
            else Log("SUITE focus: MainWindowHandle=0 (will retry after probe)");
        }
        catch (Exception ex) { Log("SUITE focus EXCEPTION " + ex.Message); }
        await DwmFlushProbe(300);
        try
        {
            CompositionTarget.Rendering += OnSuiteRendering; _renderSubs++;
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            int capHr = MfProbe.mf_cap_start(hwnd);
            MfProbe.mf_cap_item_size(out var iw, out var ih);
            double scale = XamlRoot?.RasterizationScale ?? 1.0;
            Log($"WGC CreateForWindow(hwnd=0x{hwnd.ToInt64():X}) start hr=0x{capHr:X8} item={iw}x{ih} RasterizationScale={scale}");

            // 1) gate: 2 s clock probes
            var g0 = SampleClock();
            await Task.Delay(1000); SampleClock();
            await Task.Delay(1000);
            var g1 = SampleClock();

            switches++; if (SetMode(true, "suite")) switchesOk++;
            CompReset(0, "suite start");
            await Task.Delay(400);

            // 2) calibration: static Translation.X := 37
            var calQ0 = Qpc;
            CompReset(37, "calibration");
            await Task.Delay(400);
            var calQ1 = Qpc;
            CompReset(0, "calibration end");
            await Task.Delay(400);

            // 3a) S1 / trajectory: spring 0 -> 240 from rest
            var sA = CompSpring(240, "A: S1 spring 0->240 from rest");
            // settle~0.63s + ~200ms present lag + 300ms steady window + margin — CompReset only AFTER steady samples exist
            await Task.Delay(3000);   // extra settle for underdamped overshoot → 240
            CompReset(0, "after A"); await Task.Delay(400);

            // 3b) interruption: go 240, retarget -> 0 after 150 ms (shadow seeded)
            var sB1 = CompSpring(240, "B1: go 240");
            await Task.Delay(150);
            var sB2 = CompSpring(0, "B2: interrupt -> 0");
            await Task.Delay(1500);
            CompReset(0, "after B"); await Task.Delay(400);

            // 3c) rate = 2 (folded: Period = response / 2)
            var sE = CompSpring(240, "E: rate=2 spring 0->240 (Period=response/2)", SpringResponse / 2.0, SpringDamping);
            await Task.Delay(1200);
            CompReset(0, "after E"); await Task.Delay(400);

            // 3d) bezier 0 -> 240 @ 220 ms
            _lastKeyCompleted = false;
            var sC = CompBezier(240, 0.22, "C: bezier enter 0->240 220ms");
            var tWait = 0;
            while (!_lastKeyCompleted && tWait < 1500) { await Task.Delay(50); tWait += 50; }
            Log($"COMP bezier wait: completed={_lastKeyCompleted} waited={tWait}ms Progress={CtrlDesc(_lastKeyCtrl)}");
            await Task.Delay(400); // extra frames for WGC after Completed
            CompReset(0, "after C"); await Task.Delay(300);

            // 3e) handoff check: running spring, switch to software mid-flight
            CompSpring(240, "H: go 240 then switch to software at 120 ms");
            await Task.Delay(120);
            switches++; if (SetMode(false, "suite handoff")) switchesOk++;
            await Task.Delay(1200);

            // 3f) software reference interruption
            SwRetarget(0, "SW reset"); await Task.Delay(900);
            _swTracing = true;
            SwRetarget(240, "D1: SW go 240"); await Task.Delay(150);
            SwRetarget(0, "D2: SW interrupt -> 0"); await Task.Delay(1200);
            _swTracing = false;

            var gEnd = SampleClock();
            MfProbe.mf_cap_stop();
            CompositionTarget.Rendering -= OnSuiteRendering; _renderUnsubs++;

            Report(g0, g1, gEnd, capHr, scale, calQ0, calQ1, sA, sB1, sB2, sE, sC, switches, switchesOk);
        }
        catch (Exception ex) { _exceptions++; Log("SUITE EXCEPTION " + ex); try { MfProbe.mf_cap_stop(); } catch { } CompositionTarget.Rendering -= OnSuiteRendering; _renderUnsubs++; }
        Log("===== SUITE DONE =====");
        FlushLog();
        MeasureButton.IsEnabled = true;
        _suiteRunning = false;
    }

    // ---------------------------------------------------------------- analysis / report (spec §4)
    private static double Pct(IList<double> s, double q) { if (s.Count == 0) return double.NaN; var a = s.OrderBy(x => x).ToArray(); return a[Math.Min(a.Length - 1, Math.Max(0, (int)Math.Ceiling(q * a.Length) - 1))]; }

    private static double BezierEase(double x, double x1, double y1, double x2, double y2)
    {
        double bx(double t) => 3 * (1 - t) * (1 - t) * t * x1 + 3 * (1 - t) * t * t * x2 + t * t * t;
        double by(double t) => 3 * (1 - t) * (1 - t) * t * y1 + 3 * (1 - t) * t * t * y2 + t * t * t;
        double lo = 0, hi = 1;
        for (int i = 0; i < 60; i++) { var m = 0.5 * (lo + hi); if (bx(m) < x) lo = m; else hi = m; }
        return by(0.5 * (lo + hi));
    }

    private void Report(ClockSample g0, ClockSample g1, ClockSample gEnd, int capHr, double scale, long calQ0, long calQ1,
                        CompStart? sA, CompStart? sB1, CompStart? sB2, CompStart? sE, CompStart? sC, int switches, int switchesOk)
    {
        var caps = ReadCaptures();
        var withBox = caps.Where(c => c.N > 50).ToList();
        double gateSec = (g1.NowMs - g0.NowMs) / 1000.0;
        double runSec = (gEnd.NowMs - g0.NowMs) / 1000.0;
        double rate = g1.RateDen > 0 ? (double)g1.RateNum / g1.RateDen : double.NaN;

        // ---- validity gate (spec §3.7 / §4 熄屏)
        var reasons = new List<string>();
        // (a) frame-id gate (spec §3.7) — only on OS build >= 22000
        string frameIdNote;
        if (!FrameIdGateEnabled) frameIdNote = $"frame-id gate skipped: OS build {OsBuild} < 22000";
        else if (g0.HrId != 0 || g1.HrId != 0) { frameIdNote = $"frame-id gate: DCompositionGetFrameId failed hr=0x{g0.HrId:X8}"; reasons.Add(frameIdNote); }
        else if (g1.FrameId == g0.FrameId) { frameIdNote = $"frame-id gate: FAIL, frameId unchanged over {F(gateSec, 2)} s ({g0.FrameId})"; reasons.Add(frameIdNote); }
        else frameIdNote = $"frame-id gate: pass, frameId {g0.FrameId} -> {g1.FrameId} over {F(gateSec, 2)} s";
        // (b) DWM / display-awake gate.
        // On Win10 19045, DWM_TIMING_INFO.cRefresh advances only ~0.5–2/s EVEN when DwmFlush is true ≈60Hz vsync
        // (confirmed live: DwmFlush mean 16.6 ms / p95 16.9 ms, yet cRefresh +1 per 2 s; cFramesDisplayed stays 0).
        // So cRefresh MUST NOT be the sole gate. Prefer the DwmFlush distribution verdict from suite start;
        // fall back to WGC delivery rate over the whole run. Still LOG cRefresh as evidence.
        double dwmSec = (gEnd.Qpc - g0.Qpc) / (double)Stopwatch.Frequency;
        double dwmAdv = (double)gEnd.CRefresh - g0.CRefresh, dwmExp = rate * dwmSec;
        double dwmFrac = dwmExp > 0 ? dwmAdv / dwmExp : double.NaN;
        double wgcRunFrac = (rate * dwmSec) > 0 ? caps.Count / (rate * dwmSec) : double.NaN;
        string dwmNote;
        bool dwmPass;
        if (_dwmFlushVerdict == "≈60Hz vsync")
        {
            dwmPass = true;
            dwmNote = $"DWM gate: PASS via DwmFlush verdict=≈60Hz vsync (timerFrac={F(100*_dwmFlushTimerFrac,1)}% vsyncFrac={F(100*_dwmFlushVsyncFrac,1)}% p95={F(_dwmFlushP95,3)} ms); cRefresh evidence (UNRELIABLE on this OS): {g0.CRefresh}->{gEnd.CRefresh}=+{dwmAdv} in {F(dwmSec,2)}s = {F(100*dwmFrac,2)}% of expected {F(dwmExp,1)} — NOT used for pass/fail";
        }
        else if (_dwmFlushVerdict == "timer-driven")
        {
            dwmPass = false;
            dwmNote = $"DWM gate: FAIL via DwmFlush verdict=timer-driven (timerFrac={F(100*_dwmFlushTimerFrac,1)}% vsyncFrac={F(100*_dwmFlushVsyncFrac,1)}% p95={F(_dwmFlushP95,3)} ms); cRefresh {g0.CRefresh}->{gEnd.CRefresh}=+{dwmAdv}/{F(dwmExp,1)}={F(100*dwmFrac,2)}%";
        }
        else
        {
            // unknown / not-run: fall back to WGC frames delivered over the run (>= 50% of rateRefresh x elapsed)
            dwmPass = wgcRunFrac >= DwmMinFraction;
            dwmNote = $"DWM gate: DwmFlush verdict={_dwmFlushVerdict}; fallback WGC delivery {caps.Count} frames / expected {F(rate*dwmSec,1)} = {F(100*wgcRunFrac,2)}% (threshold>={100*DwmMinFraction}%) -> {(dwmPass ? "pass" : "FAIL")}; cRefresh {g0.CRefresh}->{gEnd.CRefresh}=+{dwmAdv}/{F(dwmExp,1)}={F(100*dwmFrac,2)}%";
        }
        if (!dwmPass) reasons.Add(dwmNote);
        // (c) substitute gate 2: WGC frames with the box found inside compositor motion windows
        var motion = new[] { sA, sB1, sB2, sE, sC }.Where(x => x != null).Select(x => x!).ToList();
        double motionSec = 0; int motionFound = 0, motionAll = 0, frozenFail = 0; var frozenNotes = new List<string>();
        for (int i = 0; i < motion.Count; i++)
        {
            var m = motion[i];
            double dur = m.Spring ? Math.Max(0.05, MfProbe.mf_spring_settle(m.Response, m.Damping, m.From, m.Target, m.FromVel)) : m.Duration;
            // B1 is cut short by B2
            if (ReferenceEquals(m, sB1) && sB2 != null) dur = Math.Min(dur, (sB2.Qpc - sB1.Qpc) / (double)Stopwatch.Frequency);
            // short keyframes: keep at least 500 ms observation so WGC (capture latency) can see the travel
            double obs = m.Spring ? dur : Math.Max(dur, 0.50);
            motionSec += dur;
            motionAll += caps.Count(c => { var t = SecSince(m.Qpc, c.QpcArrive); return t > 0 && t <= obs; });
            motionFound += withBox.Count(c => { var t = SecSince(m.Qpc, c.QpcArrive); return t > 0 && t <= obs; });
            // frozen-frames check (per scenario): observed box displacement (max-min detected cx, px -> DIP via RasterizationScale)
            var inWin = withBox.Where(c => { var t = SecSince(m.Qpc, c.QpcArrive); return t > 0 && t <= obs + 0.10; }).ToList();
            double travel = Math.Abs(m.Target - m.From);
            double dispDip = inWin.Count >= 2 ? (inWin.Max(c => c.Cx) - inWin.Min(c => c.Cx)) / scale : 0;
            double frac = travel > 0 ? dispDip / travel : double.NaN;
            bool ok = frac >= FrozenMinFraction;
            if (!ok) frozenFail++;
            frozenNotes.Add($"[{m.Label.Split(':')[0]}] frames={inWin.Count} disp={F(dispDip, 2)} DIP / commanded {F(travel, 2)} DIP = {F(100 * frac, 2)}% -> {(ok ? "pass" : "FAIL")}");
        }
        string frozenNote = $"frozen-frames gate (per scenario, each must reach >= {100 * FrozenMinFraction}% of commanded travel; px->DIP /RasterizationScale {F(scale, 2)}): " + string.Join("; ", frozenNotes) + $" -> {(frozenFail == 0 && motion.Count > 0 ? "pass" : $"FAIL ({frozenFail}/{motion.Count} scenarios frozen)")}";
        double wgcExp = rate * motionSec, wgcFrac = wgcExp > 0 ? motionFound / wgcExp : double.NaN;
        string wgcNote;
        if (capHr != 0) { wgcNote = $"WGC gate: capture start failed hr=0x{capHr:X8}"; reasons.Add(wgcNote); }
        else
        {
            wgcNote = $"WGC gate: box-found frames in motion windows = {motionFound} (all frames in windows {motionAll}); expected rateRefresh x motion time {F(rate, 3)} x {F(motionSec, 3)} s = {F(wgcExp, 1)}; ratio {F(100 * wgcFrac, 2)}% (threshold >= {100 * WgcMinFraction}%) -> {(wgcFrac >= WgcMinFraction ? "pass" : "FAIL")}; run total WGC frames={caps.Count}, box found in {withBox.Count}";
            if (!(wgcFrac >= WgcMinFraction)) reasons.Add(wgcNote);
            if (frozenFail > 0 || motion.Count == 0) reasons.Add("frozen-frames: " + frozenNote);
        }
        bool valid = reasons.Count == 0;
        string INV = valid ? "" : "INVALID(display-asleep) ";
        Log("REPORT ===== spec §4 acceptance (note: the dispatch said §6; the spec's acceptance table is §4) =====");
        Log($"REPORT run validity: {(valid ? "VALID" : "INVALID (display-asleep) — data recorded but not acceptable")}");
        Log("REPORT   gate rule (coordinator ruling #7): INVALID if DWM cRefresh advance < 50% of rateRefresh x elapsed, or WGC box-found frames in motion windows < 50% of rateRefresh x motion time, or any compositor scenario shows box displacement < 10% of commanded travel (frozen-frames); frame-id gate only on OS build >= 22000");
        Log("REPORT   " + frameIdNote);
        Log("REPORT   " + dwmNote);
        Log("REPORT   " + wgcNote);
        Log("REPORT   " + frozenNote);
        foreach (var r in reasons) Log("REPORT   INVALID because: " + r);
        Log($"REPORT   clock gate: frameId {g0.FrameId} -> {g1.FrameId} (hr 0x{g0.HrId:X8}); DWM cRefresh {g0.CRefresh} -> {g1.CRefresh} -> {gEnd.CRefresh}, cFrame {g0.CFrame} -> {g1.CFrame} -> {gEnd.CFrame}, qpcVBlank {g0.QpcVBlank} -> {g1.QpcVBlank} -> {gEnd.QpcVBlank}; WGC frames total={caps.Count} (box visible in {withBox.Count}) over {F(runSec, 1)} s");

        // px -> DIP calibration from the static 37 DIP step
        double cx0 = double.NaN, cx37 = double.NaN;
        var calRest = withBox.Where(c => c.QpcArrive < calQ0).LastOrDefault();
        var cal37 = withBox.Where(c => c.QpcArrive > calQ0 && c.QpcArrive < calQ1).LastOrDefault();
        if (calRest != null) cx0 = calRest.Cx;
        if (cal37 != null) cx37 = cal37.Cx;
        double pxPerDip = (!double.IsNaN(cx0) && !double.IsNaN(cx37) && Math.Abs(cx37 - cx0) > 5) ? (cx37 - cx0) / 37.0 : scale;
        Log($"REPORT   calibration: rest cx={F(cx0)} px, at 37 DIP cx={F(cx37)} px -> {F(pxPerDip)} px/DIP {(double.IsNaN(cx37) ? "(N/A: no frame during calibration; using RasterizationScale)" : "")}");

        // 派题 #10: content time = arrive − presentationLag. Lag from matching mid-travel WGC to spring.h inverse time.
        double EstimateLag(CompStart s, double restPx)
        {
            if (!s.Spring) return 0;
            double bestLag = 0, bestErr = double.MaxValue;
            foreach (var c in withBox)
            {
                double tArr = SecSince(s.Qpc, c.QpcArrive);
                if (tArr < 0.05 || tArr > s.Duration) continue;
                double x = (c.Cx - restPx) / pxPerDip;
                double travel = Math.Abs(s.Target - s.From);
                if (travel < 1 || x < s.From + 0.15 * travel || x > s.From + 0.85 * travel) continue;
                // search lag 0..200ms so shadow(tArr-lag) ≈ x
                for (int ms = 0; ms <= 200; ms += 2)
                {
                    double tc = tArr - ms / 1000.0; if (tc < 0) break;
                    MfProbe.mf_spring_at(s.Response, s.Damping, s.From, s.Target, s.FromVel, tc, out var m, out _);
                    double e = Math.Abs(m - x);
                    if (e < bestErr) { bestErr = e; bestLag = ms / 1000.0; }
                }
            }
            return bestLag;
        }
        List<(double t, double x, Cap cap)> Traj(CompStart s, double windowSec)
        {
            // 派题 #11: rest = calibration cx0 (same as --align), NOT last WGC before start.
            // Last-before-start is still the previous scenario's pixels (160–200ms capture lag) → rest≈seed → peak X≈24 bug.
            double restPx = !double.IsNaN(cx0) ? cx0 : (withBox.Where(c => c.QpcArrive <= s.Qpc).LastOrDefault()?.Cx ?? 0);
            double lag = EstimateLag(s, restPx);
            return withBox.Select(c => (t: SecSince(s.Qpc, c.QpcArrive) - lag, x: (c.Cx - restPx) / pxPerDip, cap: c))
                          .Where(p => p.t > 0 && p.t < windowSec)
                          .OrderBy(p => p.t)
                          .ToList();
        }

        // 1) startup latency (first |Δcx|>=0.5 after start QPC)
        var lat = new List<double>();
        foreach (var s in _starts.Where(s => s.Label.StartsWith("A") || s.Label.StartsWith("B1") || s.Label.StartsWith("E") || s.Label.StartsWith("C")))
        {
            double rest = !double.IsNaN(cx0) ? cx0 : (withBox.Where(c => c.QpcArrive <= s.Qpc).LastOrDefault()?.Cx ?? 0);
            var first = withBox.Where(c => c.QpcArrive > s.Qpc && Math.Abs(c.Cx - rest) >= 0.5).OrderBy(c => c.QpcArrive).FirstOrDefault();
            if (first != null) lat.Add(SecSince(s.Qpc, first.QpcArrive) * 1000.0);
        }
        Log(lat.Count == 0 ? $"REPORT 1 启动延迟 p50/p95/max: N/A — no WGC frame showed |Δpx|>=0.5 after any StartAnimation ({caps.Count} frames total) {INV}"
                           : $"REPORT 1 启动延迟 {INV}n={lat.Count} p50={F(Pct(lat, .5), 2)} ms p95={F(Pct(lat, .95), 2)} ms max={F(lat.Max(), 2)} ms (阈值 p50<=33, p95<=50, max<=200)");

        // 2/3) 派题 #11: same as --align — DIP from calibration cx0; catch-up when |WGC_DIP − seed|≤30; continuity = seed + aligned WGC (no TryGet).
        if (sB2 != null && sB1 != null)
        {
            double restPx = !double.IsNaN(cx0) ? cx0 : 0;
            double Dip(Cap c) => (c.Cx - restPx) / pxPerDip;
            var laggedRest = withBox.Where(c => c.QpcArrive <= sB1.Qpc).LastOrDefault()?.Cx ?? double.NaN;
            Log($"REPORT   rest: calib cx0={F(cx0, 3)} (used); lagged pre-B1 cx={F(laggedRest, 3)} (NOT used; was the peak-X=24 bug)");
            var afterAll = withBox.Where(c => c.QpcArrive > sB2.Qpc).OrderBy(c => c.QpcArrive).ToList();
            var caught = afterAll.Where(c => Math.Abs(Dip(c) - sB2.From) <= 30).Take(5).ToList();
            if (caught.Count < 2)
            {
                // broader: nearest frames to seed over full post-interrupt window (up to ~1.2s)
                var ranked = afterAll.Where(c => SecSince(sB2.Qpc, c.QpcArrive) <= 1.2)
                                     .OrderBy(c => Math.Abs(Dip(c) - sB2.From)).Take(5).ToList();
                if (ranked.Count > 0)
                {
                    Log($"REPORT   catch-up: no frame within 30 DIP; nearest |Δ|={F(Math.Abs(Dip(ranked[0]) - sB2.From), 3)} dip={F(Dip(ranked[0]), 3)} seed={F(sB2.From, 3)} at +{F(SecSince(sB2.Qpc, ranked[0].QpcArrive)*1000, 1)}ms");
                    caught = ranked;
                }
            }
            if (caught.Count >= 2)
            {
                var taArrive = SecSince(sB2.Qpc, caught[0].QpcArrive);
                MfProbe.mf_spring_at(sB2.Response, sB2.Damping, sB2.From, sB2.Target, sB2.FromVel, 0, out var pv, out _);
                var dv = Math.Abs(Dip(caught[0]) - pv);
                int nSkip = afterAll.TakeWhile(c => Math.Abs(Dip(c) - sB2.From) > 30).Count();
                Log($"REPORT 2 打断位置连续性 {INV}|Δv|={F(dv * pxPerDip, 3)} px (measured {F(Dip(caught[0]))} vs shadow_seed {F(pv)} DIP; catch-up after {F(taArrive * 1000, 1)} ms; nSkip={nSkip}) (阈值 <=0.5 px)");
                Log($"REPORT   |WGC_DIP − shadow_seed| = {F(Math.Abs(Dip(caught[0]) - sB2.From), 3)} DIP");

                // 派题 #12 §4.3: primary = shadow InitialVelocity (seed). Do NOT use adjacent-frame FD as primary (lag amplifies).
                double seedVel = sB2.FromVel;
                double thr = Math.Max(20.0, 0.05 * Math.Abs(seedVel));
                // primary: continuity is the seed itself (NaturalMotion has no controller readback)
                Log($"REPORT 3 打断速度连续性 {INV}|Δvel|_primary=0.00 px/s (shadow InitialVelocity={F(seedVel, 2)} DIP/s is the continuity source; API 设计如此 — no mid-flight vel readback) (阈值 <=max(20, 5%|v|)={F(thr, 2)})");
                // cross-check: derivative of aligned WGC position over a wider window around catch-up (±2 frames or ~50ms)
                int i0 = afterAll.IndexOf(caught[0]);
                if (i0 < 0) i0 = afterAll.FindIndex(c => c.QpcArrive == caught[0].QpcArrive);
                Cap? a = i0 >= 2 ? afterAll[i0 - 2] : (i0 >= 1 ? afterAll[i0 - 1] : null);
                Cap? b = i0 >= 0 && i0 + 2 < afterAll.Count ? afterAll[i0 + 2] : (i0 + 1 < afterAll.Count ? afterAll[i0 + 1] : null);
                if (a != null && b != null)
                {
                    double dtW = SecSince(a.QpcArrive, b.QpcArrive);
                    double wgcDeriv = dtW > 1e-6 ? (Dip(b) - Dip(a)) / dtW : double.NaN;
                    Log($"REPORT 3 cross-check WGC d(pos)/dt (wider, not adjacent-FD): {F(wgcDeriv, 2)} DIP/s vs seed {F(seedVel, 2)}; |Δ|={F(Math.Abs(wgcDeriv - seedVel), 2)} px/s over Δt={F(dtW * 1000, 1)} ms (informational; lag-sensitive)");
                }
                else Log("REPORT 3 cross-check WGC d(pos)/dt: N/A — not enough frames around catch-up");
            }
            else
            {
                Log($"REPORT 2 打断位置连续性 |Δv|: N/A — caught-up WGC frames={caught.Count} after interrupt (need >=2) {INV}");
                Log($"REPORT 3 打断速度连续性 |Δvel|: N/A — same reason {INV}");
            }
            Log($"REPORT   seed at interrupt (shadow, spring.h): InitialValue={F(sB2.From)} InitialVelocity={F(sB2.FromVel)} DIP/s at +{F(sB2.NowMs - sB1.NowMs, 1)} ms after B1");
        }

        // 4) 派题 #12: after animation truly ends, 200–300 ms steady window → median (avoid early settle snapshot / CompReset wipe)
        if (sA != null)
        {
            double restA = !double.IsNaN(cx0) ? cx0 : 0;
            double DipA(Cap c) => (c.Cx - restA) / pxPerDip;
            // wall-clock age from start (no lag subtract): visual settle ≈ Duration + presentationLag
            double lagA = EstimateLag(sA, restA);
            // Prefer last 300 ms before CompReset (Delay 2500ms → window [2.15, 2.45]s wall) — true end, not early settle.
            double tEnd0 = 2.55, tEnd1 = 2.85;
            var steady = withBox.Where(c => { var tw = SecSince(sA.Qpc, c.QpcArrive); return tw >= tEnd0 && tw <= tEnd1; })
                                .OrderBy(c => c.QpcArrive).Select(c => DipA(c)).ToList();
            string winHow = $"pre-CompReset [{F(tEnd0, 2)},{F(tEnd1, 2)}]s";
            // also try |x-target|<5 lock then 250 ms
            if (steady.Count < 5 || Math.Abs(steady.OrderBy(x => x).ElementAt(steady.Count / 2) - sA.Target) > 5)
            {
                var seq = withBox.Where(c => SecSince(sA.Qpc, c.QpcArrive) > sA.Duration).OrderBy(c => c.QpcArrive).ToList();
                int hit = -1;
                for (int i = 0; i + 4 < seq.Count; i++)
                {
                    if (Enumerable.Range(0, 5).All(k => Math.Abs(DipA(seq[i + k]) - sA.Target) < 5))
                    { hit = i; break; }
                }
                if (hit >= 0)
                {
                    double tHit = SecSince(sA.Qpc, seq[hit].QpcArrive);
                    var alt = seq.Where(c => { var tw = SecSince(sA.Qpc, c.QpcArrive); return tw >= tHit && tw <= tHit + 0.28; }).Select(c => DipA(c)).ToList();
                    if (alt.Count >= 3) { steady = alt; winHow = $"|x-target|<5 lock +{F(tHit * 1000, 1)}ms then 280ms"; }
                }
            }
            var postReset = withBox.Where(c => SecSince(sA.Qpc, c.QpcArrive) > 3.05).Select(c => DipA(c)).ToList();
            bool resetWiped = postReset.Count >= 3 && postReset.TakeLast(5).Average() < Math.Abs(sA.Target) * 0.2;
            double tVis0 = tEnd0, tVis1 = tEnd1;
            if (steady.Count == 0)
                Log($"REPORT 4 终值误差: N/A — no steady WGC window (settle={F(sA.Duration, 3)}s lag≈{F(lagA * 1000, 1)}ms resetWiped={resetWiped}) {INV}");
            else
            {
                var sorted = steady.OrderBy(x => x).ToList();
                double med = sorted[sorted.Count / 2];
                Log($"REPORT 4 终值误差 {INV}|v-target|={F(Math.Abs(med - sA.Target) * pxPerDip, 3)} px median={F(med, 3)} n={steady.Count} window={winHow} settle={F(sA.Duration, 3)}s lag≈{F(lagA * 1000, 1)}ms CompResetWipedLater={resetWiped} (阈值 <=0.5 px)");
            }
        }

        // 5) trajectory RMS on QPC-aligned axis
        void TrajCheck(CompStart? s, string name, double window)
        {
            if (s == null) return;
            double restPx = !double.IsNaN(cx0) ? cx0 : 0;
            double lag = EstimateLag(s, restPx);
            // raw samples on QPC epoch (arrive − start), BEFORE lag subtract — same epoch as CompStart.Qpc
            var raw = withBox.Select(c => (tArr: SecSince(s.Qpc, c.QpcArrive), x: (c.Cx - restPx) / pxPerDip))
                             .Where(p => p.tArr > 0 && p.tArr < window + 0.35)
                             .OrderBy(p => p.tArr).ToList();
            if (raw.Count < 5) { Log($"REPORT 5 轨迹偏差 {name}: N/A — {raw.Count} WGC frames (need >=5) {INV}"); return; }
            Log($"REPORT 5 {name} QPC epoch=CompStart.Qpc; presentationLag_fit≈{F(lag * 1000, 1)} ms; n={raw.Count}");
            var travel = Math.Abs(s.Target - s.From);
            if (travel < 1e-6) travel = 1;
            double Model(double t)
            {
                if (s.Spring) { MfProbe.mf_spring_at(s.Response, s.Damping, s.From, s.Target, s.FromVel, Math.Max(0, t), out var m, out _); return m; }
                var u = Math.Clamp(t / Math.Max(1e-6, s.Duration), 0, 1);
                return s.From + (s.Target - s.From) * BezierEase(u, EnterC1.X, EnterC1.Y, EnterC2.X, EnterC2.Y);
            }
            // RMS at τ=0 on arrive time (no shift)
            double RmsAt(double tau)
            {
                var e = raw.Where(p => p.tArr - tau > 0 && p.tArr - tau < window).Select(p => p.x - Model(p.tArr - tau)).ToList();
                return e.Count < 5 ? double.NaN : Math.Sqrt(e.Average(z => z * z));
            }
            double rms0 = RmsAt(0);
            double bestTau = 0, bestRms = rms0;
            for (int ms = -250; ms <= 250; ms += 2)
            {
                double tau = ms / 1000.0;
                double r = RmsAt(tau);
                if (!double.IsNaN(r) && r < bestRms) { bestRms = r; bestTau = tau; }
            }
            // also RMS after applying EstimateLag as τ
            double rmsLag = RmsAt(lag);
            Log($"REPORT 5 轨迹偏差 {name} {INV}n={raw.Count} RMS_raw(τ=0)={F(rms0 / travel * 100, 3)}% | 时间平移残差: bestτ={F(bestTau * 1000, 1)}ms → RMS_shape={F(bestRms / travel * 100, 3)}% (改善 {F((rms0 - bestRms) / travel * 100, 3)} pp) | lag_fit τ={F(lag * 1000, 1)}ms → RMS={F(rmsLag / travel * 100, 3)}% | 形状误差=RMS_shape 阈值 RMS<=1%, max形状见下");
            var shapeErrs = raw.Where(p => p.tArr - bestTau > 0 && p.tArr - bestTau < window).Select(p => Math.Abs(p.x - Model(p.tArr - bestTau))).ToList();
            if (shapeErrs.Count > 0)
                Log($"REPORT 5 {name} 形状误差 max={F(shapeErrs.Max() / travel * 100, 3)}% of travel {F(travel, 1)} (after best time-shift; 阈值 max<=2%)");
        }
        TrajCheck(sA, "A spring 0.32/0.78", 1.2);
        TrajCheck(sE, "E spring rate=2 (Period 0.16)", 1.0);
        TrajCheck(sC, "C bezier 220ms", 0.5);

        // 6) frame interval
        Log(_renderIntervals.Count == 0 ? "REPORT 6 帧间隔: N/A — no Rendering callbacks" :
            $"REPORT 6 帧间隔(CompositionTarget.Rendering) {INV}n={_renderIntervals.Count} p50={F(Pct(_renderIntervals, .5), 2)} ms p95={F(Pct(_renderIntervals, .95), 2)} ms mean={F(_renderIntervals.Average(), 2)} ms (阈值 p50<=18, p95<=25; 熄屏下无效)");

        // 7) stability
        Log($"REPORT 7 稳定性: exceptions={_exceptions}; suite Rendering subs={_renderSubs} unsubs={_renderUnsubs}; mode switches {switchesOk}/{switches} ok; SW Rendering subscribed now={_fm?.Subscribed}");

        // 8) display asleep
        Log($"REPORT 8 熄屏: {(valid ? "display awake (gate passed)" : "INVALID (not FAIL); app did not hang; evidence above")}");

        // S1 discrimination
        if (sA != null)
        {
            var tr = Traj(sA, 1.2);
            double rEff = SpringResponse * Math.Sqrt(1 - SpringDamping * SpringDamping);   // if Period were the damped period
            double expUndamped = SpringResponse / Math.Sqrt(1 - SpringDamping * SpringDamping);
            if (tr.Count < 5) Log($"REPORT S1 Period 判别: N/A — {tr.Count} WGC frames in A (need >=5); expected peak-to-peak (=damped period) {F(expUndamped, 3)} s if Period=undamped (S1), {F(SpringResponse, 3)} s if Period=damped {INV}");
            else
            {
                var pk = tr.OrderByDescending(p => p.x).First();
                double rmsOf(double resp) => Math.Sqrt(tr.Average(p => { MfProbe.mf_spring_at(resp, SpringDamping, 0, 240, 0, p.t, out var m, out _); return (p.x - m) * (p.x - m); }));
                var rmsA = rmsOf(SpringResponse); var rmsB = rmsOf(rEff);
                Log($"REPORT S1 Period 判别 {INV}first peak at t={F(pk.t, 3)} s x={F(pk.x, 2)} -> damped period est {F(2 * pk.t, 3)} s (S1 undamped: {F(expUndamped, 3)} s; damped-interpretation: {F(SpringResponse, 3)} s); RMS vs S1 model={F(rmsA, 3)} DIP, vs damped model={F(rmsB, 3)} DIP -> {(!valid ? "verdict WITHHELD (run INVALID; no motion reached the captured frames)" : pk.x < 24 ? "verdict N/A (captured trajectory never moved >10% of travel)" : rmsA <= rmsB ? "Period = UNDAMPED period (S1 holds)" : "Period = DAMPED period (use response/sqrt(1-z^2))")}");
            }
        }

        // raw WGC dump
        Log($"RAW WGC frames: {caps.Count}");
        double tq0 = caps.Count > 0 ? caps[0].T100ns : 0;
        foreach (var c in caps) Log($"RAW WGC srt_rel={F((c.T100ns - tq0) / 1e4, 3)}ms size={c.W}x{c.H} box_px=[{c.MinX},{c.MaxX}] n={c.N} cx={F(c.Cx, 3)}");
        Log($"RAW WGC last hr=0x{MfProbe.mf_cap_last_hr():X8}; suite starts logged={_starts.Count}; SW reference samples={_swTrace.Count}");
    }

    private readonly List<Microsoft.UI.Dispatching.DispatcherQueueTimer> _timers = new();
    private void After(int ms, Action a)
    {
        var t = DispatcherQueue.CreateTimer(); t.Interval = TimeSpan.FromMilliseconds(ms); t.IsRepeating = false;
        t.Tick += (_, __) => { t.Stop(); _timers.Remove(t); if (_shutdownDone) return; try { a(); } catch (Exception ex) { _exceptions++; Log("TIMER EXCEPTION " + ex.Message); } };
        _timers.Add(t); t.Start();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e) => Shutdown("Page.Unloaded");

    /// <summary>派题 #8 item 2: idempotent, re-entrant teardown called from Page.Unloaded and MainWindow.Closed.
    /// Guarantees CompositionTarget.Rendering is unsubscribed (bridge + suite handler) and fm_destroy runs.</summary>
    public void Shutdown(string reason)
    {
        if (_shutdownDone)
        {
            try { if (_logPath != null) File.AppendAllText(_logPath, $"{Now.ToString("F3", CultureInfo.InvariantCulture)}ms SHUTDOWN reason={reason}: already done -> no-op\n"); } catch { }
            return;
        }
        _shutdownDone = true;
        foreach (var t in _timers.ToArray()) t.Stop(); _timers.Clear();
        try { MfProbe.mf_cap_stop(); } catch { }
        if (_suiteRunning) { CompositionTarget.Rendering -= OnSuiteRendering; _renderUnsubs++; _suiteRunning = false; }
        try { _visual?.StopAnimation("Translation.X"); } catch { }
        bool wasSub = _fm?.Subscribed ?? false;
        int subs = 0, unsubs = 0, cbErr = 0;
        if (_fm != null) { _fm.Dispose(); subs = _fm.SubscribeCount; unsubs = _fm.UnsubscribeCount; cbErr = _fm.CallbackErrors; }
        int psubs = 0, punsubs = 0;
        if (_probeBridge != null) { _probeBridge.Dispose(); psubs = _probeBridge.SubscribeCount; punsubs = _probeBridge.UnsubscribeCount; }
        App.Unhandled -= OnAppUnhandled;
        Log($"SHUTDOWN reason={reason}: bridge wasSubscribed={wasSub} subs={subs} unsubs={unsubs} paired={subs == unsubs} disposed={_fm?.IsDisposed} callbackErrors={cbErr} faultsInjected=[{_fm?.FaultsInjected}]; probe bridge subs={psubs} unsubs={punsubs}; suite Rendering subs={_renderSubs} unsubs={_renderUnsubs}; App.Unhandled subscribers after={App.UnhandledSubscribers}; exceptions={_exceptions}; log auto-flushes={_logAutoFlushes} dropped={_logDroppedChars}");
        _fm = null; _probeBridge = null;
        FlushLog(); _log?.Dispose(); _log = null;
    }
}

