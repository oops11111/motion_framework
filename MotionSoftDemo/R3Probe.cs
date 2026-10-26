using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Composition;

namespace MotionSoftDemo;

/// <summary>
/// 派题 #48 (R3 / P12).
/// PART A: CompositionPropertySet-driven scalar springs (P0a–P0d) feasibility — expression on Translation.X reads a
///         property-set scalar; a ScalarNaturalMotionAnimation animates that scalar; TryGetScalar readback sampled on
///         the UI thread every ~8 ms.
/// PART B: E1 phase sweep — B1 (0->240) is started phase-free, then at 60 ms + phi a retarget is seeded either with
///         velocity only ("Bv", InitialValue unset) or explicitly ("Aex", InitialValue=shX/InitialVelocity=shV),
///         plus a no-retarget control ("base"). Vsync phase comes from a background DwmFlush() tick ring.
/// Programmatic Composition animations only — no SendInput / InputInjector / mouse / keyboard synthesis.
/// </summary>
public sealed partial class MainPage
{
    private async Task RunR3P12Probe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artPath = Path.Combine(@"C:\Users\admin\motion_framework\artifacts", "p12_48_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        const double rr = 0.32, zeta = 0.78;
        int tickStop = 0; Thread? tickThread = null;
        Both("# p12_48_raw.txt — 派题 #48 (R3): PART A P0a–P0d CompositionPropertySet scalar-spring feasibility (5 reps each) + PART B E1 explicit-seed phase sweep (Bv/Aex/base, phi {1,3,5,7,9,11,13,15} ms, b {0,120}, 12 reps); programmatic Composition animations only, NO synthetic input");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss}; spring r={F(rr, 2)} zeta={F(zeta, 2)}; QPC freq={Stopwatch.Frequency}; units: tSeed/tInt_m/phiAct/tickPer/lastTick/RB = ms; RAW = tArr_ms:xDip:tSrt_ms");
        try
        {
            if (MfProbe.mf_map_spring(rr, zeta, out var period, out var zetaC) != 1) throw new Exception("map");
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "r3-p12-probe");
            CompReset(0, "p12 start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            var calQ0 = Qpc; CompReset(37, "p12 cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "p12 cal end"); await Task.Delay(500);
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
            SpringScalarNaturalMotionAnimation Sp() { var a = _comp!.CreateSpringScalarAnimation(); a.Period = TimeSpan.FromSeconds(period); a.DampingRatio = (float)zetaC; return a; }
            List<Cap> CapsSince(long q0) => ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > q0).OrderBy(c => c.QpcArrive).ToList();
            string RawOf(List<Cap> caps, long q0) => "RAW " + string.Join(" ", caps.Select(c => $"{F(SecSince(q0, c.QpcArrive) * 1000, 2)}:{F((c.Cx - cx0) / ppd, 2)}:{F((c.T100ns + srtOff - q0) / 1e4, 2)}"));
            string Xs(List<Cap> caps)
            {
                if (caps.Count == 0) return "xFirst=NaN xLast=NaN xMin=NaN xMax=NaN";
                var xs = caps.Select(c => (c.Cx - cx0) / ppd).ToList();
                return $"xFirst={F(xs[0], 2)} xLast={F(xs[^1], 2)} xMin={F(xs.Min(), 2)} xMax={F(xs.Max(), 2)}";
            }
            // readback: TryGetScalar sampled on the UI thread every ~8 ms while the captures accumulate
            async Task<List<(double t, double[] v)>> Sample(long q0, int durMs, CompositionPropertySet ps, string[] keys)
            {
                var res = new List<(double, double[])>();
                while (SecSince(q0, Qpc) * 1000 < durMs)
                {
                    await Task.Delay(8);
                    double t = SecSince(q0, Qpc) * 1000;
                    var v = new double[keys.Length];
                    for (int i = 0; i < keys.Length; i++)
                    {
                        var st = ps.TryGetScalar(keys[i], out var f);
                        v[i] = st == CompositionGetValueStatus.Succeeded ? f : double.NaN;
                    }
                    res.Add((t, v));
                }
                return res;
            }
            void EmitRb(List<(double t, double[] v)> s, string[] keys)
            {
                foreach (var (t, v) in s)
                {
                    if (keys.Length == 1) Both($"RB {F(t, 1)}:{F(v[0], 3)}");
                    else Both($"RB {F(t, 1)}:" + string.Join(" ", keys.Select((kk, i) => kk + "=" + F(v[i], 3))));
                }
            }
            double LastOf(List<(double t, double[] v)> s, int i) => s.Count > 0 ? s[^1].v[i] : double.NaN;
            double MaxOf(List<(double t, double[] v)> s, int i) => s.Count > 0 ? s.Max(z => z.v[i]) : double.NaN;
            double MinOf(List<(double t, double[] v)> s, int i) => s.Count > 0 ? s.Min(z => z.v[i]) : double.NaN;
            string KeyStr(string k, float v, CompositionGetValueStatus st) => $"{k}={((st == CompositionGetValueStatus.Succeeded) ? F(v, 3) : st.ToString())}";

            // ================================================================ PART A
            Both("----- PART A: CompositionPropertySet scalar springs -----");

            // P0a: expression "ps.d" drives Translation.X; spring 0 -> 240 on ps.d
            for (int k = 0; k < 5; k++)
            {
                try
                {
                    var ps = _comp!.CreatePropertySet();
                    ps.InsertScalar("d", 0f);
                    var ea = _comp.CreateExpressionAnimation("ps.d");
                    ea.SetReferenceParameter("ps", ps);
                    _visual!.StartAnimation("Translation.X", ea);
                    await Task.Delay(60);
                    long q0 = Qpc;
                    var a = Sp(); a.InitialValue = 0f; a.InitialVelocity = 0f; a.FinalValue = 240f;
                    ps.StartAnimation("d", a);
                    long qAnim = Qpc;
                    var rb = await Sample(q0, 900, ps, new[] { "d" });
                    var caps = CapsSince(q0);
                    Both($"P0a {k} psSpring d:0->240 iv=0 caps={caps.Count} {Xs(caps)} rbN={rb.Count} rbLast={F(LastOf(rb, 0), 3)} rbMax={F(MaxOf(rb, 0), 3)} rbMin={F(MinOf(rb, 0), 3)} startQpc={qAnim} tStart={F(SecSince(q0, qAnim) * 1000, 3)} |xLast-rbLast|={F(Math.Abs((caps.Count > 0 ? (caps[^1].Cx - cx0) / ppd : double.NaN) - LastOf(rb, 0)), 2)}");
                    EmitRb(rb, new[] { "d" });
                    Both(RawOf(caps, q0));
                }
                catch (Exception ex) { Both($"P0a {k} THROW 0x{ex.HResult:X8} {ex.GetType().Name} {ex.Message}"); }
                _visual.StopAnimation("Translation.X"); CompReset(0, "P0a end");
                await Task.Delay(150);
            }
            Both("P0a DONE");

            // P0b: same wiring, spring 0 -> 0 with InitialVelocity 1500 (does it move or no-op?)
            for (int k = 0; k < 5; k++)
            {
                try
                {
                    var ps = _comp!.CreatePropertySet();
                    ps.InsertScalar("d", 0f);
                    var ea = _comp.CreateExpressionAnimation("ps.d");
                    ea.SetReferenceParameter("ps", ps);
                    _visual!.StartAnimation("Translation.X", ea);
                    await Task.Delay(60);
                    long q0 = Qpc;
                    var a = Sp(); a.InitialValue = 0f; a.InitialVelocity = 1500f; a.FinalValue = 0f;
                    ps.StartAnimation("d", a);
                    long qAnim = Qpc;
                    var rb = await Sample(q0, 900, ps, new[] { "d" });
                    var caps = CapsSince(q0);
                    double span = MaxOf(rb, 0) - MinOf(rb, 0);
                    Both($"P0b {k} psSpring d:0->0 iv=1500 caps={caps.Count} {Xs(caps)} rbN={rb.Count} rbLast={F(LastOf(rb, 0), 3)} rbMax={F(MaxOf(rb, 0), 3)} rbMin={F(MinOf(rb, 0), 3)} rbSpan={F(span, 3)} verdict={(span > 1.0 ? "MOVED" : "NOOP")} startQpc={qAnim} tStart={F(SecSince(q0, qAnim) * 1000, 3)}");
                    EmitRb(rb, new[] { "d" });
                    Both(RawOf(caps, q0));
                }
                catch (Exception ex) { Both($"P0b {k} THROW 0x{ex.HResult:X8} {ex.GetType().Name} {ex.Message}"); }
                _visual.StopAnimation("Translation.X"); CompReset(0, "P0b end");
                await Task.Delay(150);
            }
            Both("P0b DONE");

            // P0c (+P0d hold): composite "ps.b + ps.d1 + ps.d2", retargets at 60 ms / 156 ms
            var keys3 = new[] { "b", "d1", "d2" };
            for (int k = 0; k < 5; k++)
            {
                try
                {
                    var ps = _comp!.CreatePropertySet();
                    ps.InsertScalar("b", 0f); ps.InsertScalar("d1", 0f); ps.InsertScalar("d2", 0f);
                    var ea = _comp.CreateExpressionAnimation("ps.b + ps.d1 + ps.d2");
                    ea.SetReferenceParameter("ps", ps);
                    _visual!.StartAnimation("Translation.X", ea);
                    await Task.Delay(60);
                    long q0 = Qpc;
                    var ab = Sp(); ab.InitialValue = 0f; ab.InitialVelocity = 0f; ab.FinalValue = 240f;
                    ps.StartAnimation("b", ab);
                    long qB = Qpc;
                    while (SecSince(q0, Qpc) < 0.060 - 0.020) await Task.Delay(2);
                    while (SecSince(q0, Qpc) < 0.060) { }
                    var a1 = Sp(); a1.InitialValue = 0f; a1.InitialVelocity = 0f; a1.FinalValue = -240f;
                    ps.StartAnimation("d1", a1);
                    long qD1 = Qpc;
                    while (SecSince(q0, Qpc) < 0.156 - 0.020) await Task.Delay(2);
                    while (SecSince(q0, Qpc) < 0.156) { }
                    var a2 = Sp(); a2.InitialValue = 0f; a2.InitialVelocity = 0f; a2.FinalValue = 120f;
                    ps.StartAnimation("d2", a2);
                    long qD2 = Qpc;
                    var rb = await Sample(q0, 1200, ps, keys3);
                    var caps = CapsSince(q0);
                    Both($"P0c {k} composite b:0->240 d1:0->-240 d2:0->120 caps={caps.Count} {Xs(caps)} qB={qB} qD1={qD1} qD2={qD2} tB={F(SecSince(q0, qB) * 1000, 3)} tD1={F(SecSince(q0, qD1) * 1000, 3)} tD2={F(SecSince(q0, qD2) * 1000, 3)} rbN={rb.Count} rbB={F(LastOf(rb, 0), 3)} rbD1={F(LastOf(rb, 1), 3)} rbD2={F(LastOf(rb, 2), 3)}");
                    EmitRb(rb, keys3);
                    Both(RawOf(caps, q0));

                    // P0d: hold 1500 ms with no new commands, then 400 ms more of captures + readback (expect 120)
                    await Task.Delay(1500);
                    long qh = Qpc;
                    var rb2 = await Sample(qh, 400, ps, keys3);
                    var caps2 = CapsSince(qh);
                    double xHold = caps2.Count > 0 ? (caps2[^1].Cx - cx0) / ppd : double.NaN;
                    Both($"P0d {k} hold caps={caps2.Count} {Xs(caps2)} rbN={rb2.Count} rbB={F(LastOf(rb2, 0), 3)} rbD1={F(LastOf(rb2, 1), 3)} rbD2={F(LastOf(rb2, 2), 3)} sum={F(LastOf(rb2, 0) + LastOf(rb2, 1) + LastOf(rb2, 2), 3)} xHold={F(xHold, 2)} expect=120");
                    EmitRb(rb2, keys3);
                    Both(RawOf(caps2, qh));
                    ps.StopAnimation("b"); ps.StopAnimation("d1"); ps.StopAnimation("d2");
                    await Task.Delay(30);
                    var s1 = ps.TryGetScalar("b", out var vb); var s2 = ps.TryGetScalar("d1", out var v1); var s3 = ps.TryGetScalar("d2", out var v2);
                    var caps3 = CapsSince(qh);
                    Both($"P0d {k} afterStop {KeyStr("b", vb, s1)} {KeyStr("d1", v1, s2)} {KeyStr("d2", v2, s3)} xAfter={F((caps3.Count > 0 ? (caps3[^1].Cx - cx0) / ppd : double.NaN), 2)}");
                }
                catch (Exception ex) { Both($"P0c {k} THROW 0x{ex.HResult:X8} {ex.GetType().Name} {ex.Message}"); }
                _visual.StopAnimation("Translation.X"); CompReset(0, "P0c/P0d end");
                await Task.Delay(150);
            }
            Both("P0c DONE"); Both("P0d DONE");

            // ================================================================ PART B
            Both("----- PART B: E1 phase sweep (Bv / Aex / base) -----");
            const int RingN = 256;
            var ring = new long[RingN];
            int ringHead = 0, ringCount = 0;
            var ringLock = new object();
            tickThread = new Thread(() =>
            {
                while (Volatile.Read(ref tickStop) == 0)
                {
                    DwmFlush();
                    long q = Qpc;
                    lock (ringLock)
                    {
                        ring[ringHead] = q; ringHead = (ringHead + 1) % RingN;
                        if (ringCount < RingN) ringCount++;
                    }
                }
            })
            { IsBackground = true, Name = "p12-dwmflush" };
            tickThread.Start();
            await Task.Delay(250);

            (long last, double per) TickInfo()
            {
                lock (ringLock)
                {
                    if (ringCount == 0) return (0, Stopwatch.Frequency / 60.0);
                    long last = ring[(ringHead - 1 + RingN) % RingN];
                    int m = Math.Min(ringCount - 1, 64);
                    if (m <= 0) return (last, Stopwatch.Frequency / 60.0);
                    var iv = new double[m];
                    for (int i = 0; i < m; i++)
                    {
                        int i2 = (ringHead - 1 - i + RingN * 2) % RingN;
                        int i1 = (i2 - 1 + RingN) % RingN;
                        iv[i] = ring[i2] - ring[i1];
                    }
                    Array.Sort(iv);
                    return (last, iv[m / 2]);
                }
            }
            // next target = lastTick + k*period + phi, k smallest with target >= now + 0.3 ms; busy-spin to it
            (long target, long actual, long lastTick, double per) SpinToPhase(double phiMs)
            {
                var ti = TickInfo();
                long last = ti.last; double per = ti.per;
                if (per <= 0) per = Stopwatch.Frequency / 60.0;
                long ph = (long)Math.Round(phiMs / 1000.0 * Stopwatch.Frequency);
                long now = Qpc + (long)(0.0003 * Stopwatch.Frequency);
                long target = last + (long)(Math.Ceiling((now - last - ph) / per) * per) + ph;
                while (Qpc < target) { }
                return (target, Qpc, last, per);
            }
            async Task Trial(string arm, double b, double phiMs, int rep)
            {
                CompReset(0, "p12 trial"); await WaitNear(0); await Task.Delay(200);
                long qStart = Qpc;
                var b1 = Sp(); b1.InitialValue = 0f; b1.InitialVelocity = 0f; b1.FinalValue = 240f;
                _visual!.StartAnimation("Translation.X", b1);
                while (SecSince(qStart, Qpc) < 0.060 - 0.025) await Task.Delay(2);
                while (SecSince(qStart, Qpc) < 0.060) { }
                var sp = SpinToPhase(phiMs);
                long qSeed = sp.actual;
                double tSeed = SecSince(qStart, qSeed);
                MfProbe.mf_spring_at(rr, zeta, 0, 240, 0, tSeed, out var shX, out var shV);
                if (arm != "base")
                {
                    var a = Sp();
                    if (arm == "Aex") a.InitialValue = (float)shX;      // "Bv" leaves InitialValue unset
                    a.InitialVelocity = (float)shV;
                    a.FinalValue = (float)b;
                    _visual!.StartAnimation("Translation.X", a);
                }
                double tInt = SecSince(qStart, Qpc);                   // tInt_m: StartAnimation returned
                double phiAct = (qSeed - sp.lastTick) * 1000.0 / Stopwatch.Frequency;
                double tickPerMs = sp.per * 1000.0 / Stopwatch.Frequency;
                double lastRel = (sp.lastTick - qStart) * 1000.0 / Stopwatch.Frequency;
                while (SecSince(qStart, Qpc) < 0.700) await Task.Delay(10);
                var caps = CapsSince(qStart);
                Both($"TRIAL arm={arm} b={F(b, 0)} phi={F(phiMs, 0)} rep={rep} tSeed={F(tSeed * 1000, 3)} tInt_m={F(tInt * 1000, 3)} phiAct={F(phiAct, 3)} tickPer={F(tickPerMs, 3)} shX={F(shX, 3)} shV={F(shV, 2)} lastTick={F(lastRel, 3)} n={caps.Count}");
                Both(RawOf(caps, qStart));
            }
            double[] phis = { 1, 3, 5, 7, 9, 11, 13, 15 };
            double[] bs = { 0, 120 };
            string[] arms = { "Bv", "Aex", "base" };
            var rnd = new Random(20261048);
            for (int rep = 0; rep < 12; rep++)
            {
                var blocks = new List<(int pi, int bi)>();
                for (int pi = 0; pi < phis.Length; pi++) for (int bi = 0; bi < bs.Length; bi++) blocks.Add((pi, bi));
                var order = blocks.OrderBy(_ => rnd.Next()).ToList();
                for (int bl = 0; bl < order.Count; bl++)
                {
                    int rot = (rep + bl) % 3;
                    for (int j = 0; j < 3; j++)
                    {
                        var arm = arms[(j + rot) % 3];
                        await Trial(arm, bs[order[bl].bi], phis[order[bl].pi], rep);
                    }
                }
                FlushLog(); Both($"REP {rep} DONE");
            }
            Both("===== P12 RAW DONE =====");
            Volatile.Write(ref tickStop, 1);
            try { tickThread.Join(500); } catch { }
            art.Close(); try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex) { Both("P12 EXCEPTION " + ex); try { art.Close(); } catch { } try { MfProbe.mf_cap_stop(); } catch { } }
            finally
        {
            Volatile.Write(ref tickStop, 1);
            try { tickThread?.Join(300); } catch { }
            _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog(); try { CloseMainWindowSoon(); } catch { }
        }
    }

    /// <summary>
    /// 派题 #49 (R3 / E3): Sup overlay vs Bv(+base) phase-sweep comparison matrix.
    /// Sup: a CompositionPropertySet scalar "b" carries the base spring (0->240) and never stops; an overlay spring on
    ///      "d1" (FinalValue = bNew-240) is started at tInt+phi. Translation.X = expression "ps.b + ps.d1".
    /// Bv:  B1 (0->240) started phase-free on Translation.X, then at tInt+phi a retarget with InitialValue unset and
    ///      InitialVelocity = analytic shadow velocity shV, FinalValue = bNew.
    /// base: B1 (0->240) started; no retarget (timing-parity control).
    /// Vsync phase from a background DwmFlush() tick ring (identical to P12). Programmatic Composition animations only —
    /// no SendInput / InputInjector / mouse / keyboard synthesis.
    /// </summary>
    private async Task RunR3E3Probe()
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artPath = Path.Combine(@"C:\Users\admin\motion_framework\artifacts", "p13_49_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        const double rr = 0.32, zeta = 0.78;
        int tickStop = 0; Thread? tickThread = null;
        Both("# p13_49_raw.txt — 派题 #49 (R3 E3): Sup overlay vs Bv(+base) phase-sweep; tInt {60,156} ms; b_new {0,120}; phi {1,5,9,13} ms; arms {Sup,Bv,base}; programmatic Composition only, NO synthetic input");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss}; spring r={F(rr, 2)} zeta={F(zeta, 2)}; QPC freq={Stopwatch.Frequency}; units: tSeed/tInt_m/phiAct/tickPer/lastTick = ms; RAW = tArr_ms:xDip:tSrt_ms");
        try
        {
            if (MfProbe.mf_map_spring(rr, zeta, out var period, out var zetaC) != 1) throw new Exception("map");
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "r3-e3-probe");
            CompReset(0, "e3 start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            var calQ0 = Qpc; CompReset(37, "e3 cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "e3 cal end"); await Task.Delay(500);
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
            SpringScalarNaturalMotionAnimation Sp() { var a = _comp!.CreateSpringScalarAnimation(); a.Period = TimeSpan.FromSeconds(period); a.DampingRatio = (float)zetaC; return a; }
            List<Cap> CapsSince(long q0) => ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > q0).OrderBy(c => c.QpcArrive).ToList();
            string RawOf(List<Cap> caps, long q0) => "RAW " + string.Join(" ", caps.Select(c => $"{F(SecSince(q0, c.QpcArrive) * 1000, 2)}:{F((c.Cx - cx0) / ppd, 2)}:{F((c.T100ns + srtOff - q0) / 1e4, 2)}"));
            string Xs(List<Cap> caps)
            {
                if (caps.Count == 0) return "xFirst=NaN xLast=NaN xMin=NaN xMax=NaN";
                var xs = caps.Select(c => (c.Cx - cx0) / ppd).ToList();
                return $"xFirst={F(xs[0], 2)} xLast={F(xs[^1], 2)} xMin={F(xs.Min(), 2)} xMax={F(xs.Max(), 2)}";
            }

            const int RingN = 256;
            var ring = new long[RingN];
            int ringHead = 0, ringCount = 0;
            var ringLock = new object();
            tickThread = new Thread(() =>
            {
                while (Volatile.Read(ref tickStop) == 0)
                {
                    DwmFlush();
                    long q = Qpc;
                    lock (ringLock)
                    {
                        ring[ringHead] = q; ringHead = (ringHead + 1) % RingN;
                        if (ringCount < RingN) ringCount++;
                    }
                }
            })
            { IsBackground = true, Name = "e3-dwmflush" };
            tickThread.Start();
            await Task.Delay(250);

            (long last, double per) TickInfo()
            {
                lock (ringLock)
                {
                    if (ringCount == 0) return (0, Stopwatch.Frequency / 60.0);
                    long last = ring[(ringHead - 1 + RingN) % RingN];
                    int m = Math.Min(ringCount - 1, 64);
                    if (m <= 0) return (last, Stopwatch.Frequency / 60.0);
                    var iv = new double[m];
                    for (int i = 0; i < m; i++)
                    {
                        int i2 = (ringHead - 1 - i + RingN * 2) % RingN;
                        int i1 = (i2 - 1 + RingN) % RingN;
                        iv[i] = ring[i2] - ring[i1];
                    }
                    Array.Sort(iv);
                    return (last, iv[m / 2]);
                }
            }
            // next target = lastTick + k*period + phi, k smallest with target >= now + 0.3 ms; busy-spin to it
            (long target, long actual, long lastTick, double per) SpinToPhase(double phiMs)
            {
                var ti = TickInfo();
                long last = ti.last; double per = ti.per;
                if (per <= 0) per = Stopwatch.Frequency / 60.0;
                long ph = (long)Math.Round(phiMs / 1000.0 * Stopwatch.Frequency);
                long now = Qpc + (long)(0.0003 * Stopwatch.Frequency);
                long target = last + (long)(Math.Ceiling((now - last - ph) / per) * per) + ph;
                while (Qpc < target) { }
                return (target, Qpc, last, per);
            }

            async Task Trial(string arm, double bNew, double phiMs, double tIntMs, int rep)
            {
                CompReset(0, "e3 trial"); await WaitNear(0); await Task.Delay(200);
                long qStart = Qpc;
                long qSeed; double tSeed; double shX, shV;
                if (arm == "Sup")
                {
                    var ps = _comp!.CreatePropertySet();
                    ps.InsertScalar("b", 0f); ps.InsertScalar("d1", 0f);
                    var ea = _comp.CreateExpressionAnimation("ps.b + ps.d1");
                    ea.SetReferenceParameter("ps", ps);
                    _visual!.StartAnimation("Translation.X", ea);
                    var ab = Sp(); ab.InitialValue = 0f; ab.InitialVelocity = 0f; ab.FinalValue = 240f;
                    ps.StartAnimation("b", ab);                                  // base spring; do NOT stop "b" later
                    while (SecSince(qStart, Qpc) < tIntMs / 1000.0 - 0.025) await Task.Delay(2);
                    while (SecSince(qStart, Qpc) < tIntMs / 1000.0) { }
                    var sp = SpinToPhase(phiMs);
                    qSeed = sp.actual;
                    tSeed = SecSince(qStart, qSeed);
                    MfProbe.mf_spring_at(rr, zeta, 0, 240, 0, tSeed, out shX, out shV);   // log only; do NOT seed Sup
                    var d1 = Sp(); d1.InitialValue = 0f; d1.InitialVelocity = 0f; d1.FinalValue = (float)(bNew - 240);
                    ps.StartAnimation("d1", d1);                                  // overlay; no readback, no δ
                    double tInt_m = SecSince(qStart, Qpc);
                    double phiAct = (qSeed - sp.lastTick) * 1000.0 / Stopwatch.Frequency;
                    double tickPerMs = sp.per * 1000.0 / Stopwatch.Frequency;
                    double lastRel = (sp.lastTick - qStart) * 1000.0 / Stopwatch.Frequency;
                    while (SecSince(qStart, Qpc) < 0.700) await Task.Delay(10);
                    var caps = CapsSince(qStart);
                    Both($"TRIAL arm={arm} b={F(bNew, 0)} tInt={F(tIntMs, 0)} phi={F(phiMs, 0)} rep={rep} tSeed={F(tSeed * 1000, 3)} tInt_m={F(tInt_m * 1000, 3)} phiAct={F(phiAct, 3)} tickPer={F(tickPerMs, 3)} shX={F(shX, 3)} shV={F(shV, 2)} lastTick={F(lastRel, 3)} n={caps.Count} sup=1");
                    Both(RawOf(caps, qStart));
                    _visual.StopAnimation("Translation.X");
                    try { ps.StopAnimation("b"); ps.StopAnimation("d1"); } catch { }
                    CompReset(0, "e3 trial Sup end");
                }
                else if (arm == "Bv")
                {
                    var b1 = Sp(); b1.InitialValue = 0f; b1.InitialVelocity = 0f; b1.FinalValue = 240f;
                    _visual!.StartAnimation("Translation.X", b1);
                    while (SecSince(qStart, Qpc) < tIntMs / 1000.0 - 0.025) await Task.Delay(2);
                    while (SecSince(qStart, Qpc) < tIntMs / 1000.0) { }
                    var sp = SpinToPhase(phiMs);
                    qSeed = sp.actual;
                    tSeed = SecSince(qStart, qSeed);
                    MfProbe.mf_spring_at(rr, zeta, 0, 240, 0, tSeed, out shX, out shV);
                    var a = Sp();                                           // InitialValue NOT set (IV=null)
                    a.InitialVelocity = (float)shV; a.FinalValue = (float)bNew;
                    _visual!.StartAnimation("Translation.X", a);
                    double tInt_m = SecSince(qStart, Qpc);
                    double phiAct = (qSeed - sp.lastTick) * 1000.0 / Stopwatch.Frequency;
                    double tickPerMs = sp.per * 1000.0 / Stopwatch.Frequency;
                    double lastRel = (sp.lastTick - qStart) * 1000.0 / Stopwatch.Frequency;
                    while (SecSince(qStart, Qpc) < 0.700) await Task.Delay(10);
                    var caps = CapsSince(qStart);
                    Both($"TRIAL arm={arm} b={F(bNew, 0)} tInt={F(tIntMs, 0)} phi={F(phiMs, 0)} rep={rep} tSeed={F(tSeed * 1000, 3)} tInt_m={F(tInt_m * 1000, 3)} phiAct={F(phiAct, 3)} tickPer={F(tickPerMs, 3)} shX={F(shX, 3)} shV={F(shV, 2)} lastTick={F(lastRel, 3)} n={caps.Count}");
                    Both(RawOf(caps, qStart));
                    _visual.StopAnimation("Translation.X"); CompReset(0, "e3 trial Bv end");
                }
                else // "base"
                {
                    var b1 = Sp(); b1.InitialValue = 0f; b1.InitialVelocity = 0f; b1.FinalValue = 240f;
                    _visual!.StartAnimation("Translation.X", b1);
                    while (SecSince(qStart, Qpc) < tIntMs / 1000.0 - 0.025) await Task.Delay(2);
                    while (SecSince(qStart, Qpc) < tIntMs / 1000.0) { }
                    var sp = SpinToPhase(phiMs);                              // timing parity
                    qSeed = sp.actual;
                    tSeed = SecSince(qStart, qSeed);
                    MfProbe.mf_spring_at(rr, zeta, 0, 240, 0, tSeed, out shX, out shV);
                    double tInt_m = SecSince(qStart, Qpc);                    // right after SpinToPhase returns
                    double phiAct = (qSeed - sp.lastTick) * 1000.0 / Stopwatch.Frequency;
                    double tickPerMs = sp.per * 1000.0 / Stopwatch.Frequency;
                    double lastRel = (sp.lastTick - qStart) * 1000.0 / Stopwatch.Frequency;
                    while (SecSince(qStart, Qpc) < 0.700) await Task.Delay(10);
                    var caps = CapsSince(qStart);
                    Both($"TRIAL arm={arm} b={F(bNew, 0)} tInt={F(tIntMs, 0)} phi={F(phiMs, 0)} rep={rep} tSeed={F(tSeed * 1000, 3)} tInt_m={F(tInt_m * 1000, 3)} phiAct={F(phiAct, 3)} tickPer={F(tickPerMs, 3)} shX={F(shX, 3)} shV={F(shV, 2)} lastTick={F(lastRel, 3)} n={caps.Count}");
                    Both(RawOf(caps, qStart));
                    _visual.StopAnimation("Translation.X"); CompReset(0, "e3 trial base end");
                }
            }

            double[] phis = { 1, 5, 9, 13 };
            double[] bs = { 0, 120 };
            string[] arms = { "Sup", "Bv", "base" };
            var tIntSpec = new[] { (tInt: 60.0, reps: 8), (tInt: 156.0, reps: 4) };
            var rnd = new Random(20261049);
            int blockIndex = 0;
            foreach (var spec in tIntSpec)
            {
                for (int rep = 0; rep < spec.reps; rep++)
                {
                    var blocks = new List<(int pi, int bi)>();
                    for (int pi = 0; pi < phis.Length; pi++) for (int bi = 0; bi < bs.Length; bi++) blocks.Add((pi, bi));
                    var order = blocks.OrderBy(_ => rnd.Next()).ToList();
                    for (int bl = 0; bl < order.Count; bl++)
                    {
                        int rot = (rep + blockIndex) % 3;
                        for (int j = 0; j < 3; j++)
                        {
                            var arm = arms[(j + rot) % 3];
                            await Trial(arm, bs[order[bl].bi], phis[order[bl].pi], spec.tInt, rep);
                        }
                        blockIndex++;
                    }
                    FlushLog(); Both($"REP tInt={F(spec.tInt, 0)} rep={rep} DONE");
                }
            }
            Both("===== P13 RAW DONE =====");
            Volatile.Write(ref tickStop, 1);
            try { tickThread.Join(500); } catch { }
            art.Close(); try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex) { Both("E3 EXCEPTION " + ex); try { art.Close(); } catch { } try { MfProbe.mf_cap_stop(); } catch { } }
        finally
        {
            Volatile.Write(ref tickStop, 1);
            try { tickThread?.Join(300); } catch { }
            _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog(); try { CloseMainWindowSoon(); } catch { }
        }
    }

    /// <summary>
    /// 派题 #50 (R3 Sup-adv). Stage A: P0-fit gate (A1a seed-shift, A1b Δ-linearity, A1c superposition d1-on-d0,
    ///   A2 ε-path check on the unified PS expression). Stage B: Sup vs SupS2 (prebind-once / retarget-idle-slot) vs
    ///   Bv vs base phase-sweep matrix. Stage C (only if runC): S1 ε̂ sweep on SupS2 wiring (d1 seeded with
    ///   s(ε̂), s'(ε̂) at the interrupt). Notation: base + d0 (old spring still running) + d1 (step overlay) + d2.
    ///   Programmatic Composition animations only — no SendInput / InputInjector / mouse / keyboard synthesis.
    /// </summary>
    private async Task RunR350Probe(bool runC)
    {
        if (_suiteRunning || _comp is null || _visual is null) return;
        _suiteRunning = true; MeasureButton.IsEnabled = false;
        string artPath = Path.Combine(@"C:\Users\admin\motion_framework\artifacts", "p14_50_raw.txt");
        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        const double rr = 0.32, zeta = 0.78;
        int tickStop = 0; Thread? tickThread = null;
        Both("# t_a1a_raw.txt — T-A1a (门槛采数 A-only×3): Stage A focus-τ0 gate {21.8,3.3,6.8,8.3,33.3} + A1b origin=220 + A1c wide-ε 12 reps + A2 path=PS; Stage B not required (A-only; runner stops after STAGE A END); corr=CONFIRM_F5_REFUTE_2X; programmatic Composition only, NO synthetic input");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss}; spring r={F(rr, 2)} zeta={F(zeta, 2)}; runC={runC}; QPC freq={Stopwatch.Frequency}; units ms; RAW = tArr_ms:xDip:tSrt_ms");
        try
        {
            if (MfProbe.mf_map_spring(rr, zeta, out var period, out var zetaC) != 1) throw new Exception("map");
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            if (hwnd != IntPtr.Zero) { ShowWindow(hwnd, 9); SetForegroundWindow(hwnd); }
            await DwmFlushProbe(80);
            Both($"DwmFlushVerdict={_dwmFlushVerdict}");
            SetMode(true, "r3-50-probe");
            CompReset(0, "r3-50 start"); await Task.Delay(300);
            hwnd = Process.GetCurrentProcess().MainWindowHandle;
            Both($"WGC start hr=0x{MfProbe.mf_cap_start(hwnd):X8}");
            var calQ0 = Qpc; CompReset(37, "r3-50 cal"); await Task.Delay(400); var calQ1 = Qpc;
            CompReset(0, "r3-50 cal end"); await Task.Delay(500);
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
            SpringScalarNaturalMotionAnimation Sp() { var a = _comp!.CreateSpringScalarAnimation(); a.Period = TimeSpan.FromSeconds(period); a.DampingRatio = (float)zetaC; return a; }
            List<Cap> CapsSince(long q0) => ReadCaptures().Where(c => c.N > 50 && c.QpcArrive > q0).OrderBy(c => c.QpcArrive).ToList();
            string RawOf(List<Cap> caps, long q0) => "RAW " + string.Join(" ", caps.Select(c => $"{F(SecSince(q0, c.QpcArrive) * 1000, 2)}:{F((c.Cx - cx0) / ppd, 2)}:{F((c.T100ns + srtOff - q0) / 1e4, 2)}"));
            string Xs(List<Cap> caps)
            {
                if (caps.Count == 0) return "xFirst=NaN xLast=NaN xMin=NaN xMax=NaN";
                var xs = caps.Select(c => (c.Cx - cx0) / ppd).ToList();
                return $"xFirst={F(xs[0], 2)} xLast={F(xs[^1], 2)} xMin={F(xs.Min(), 2)} xMax={F(xs.Max(), 2)}";
            }

            const int RingN = 256;
            var ring = new long[RingN];
            int ringHead = 0, ringCount = 0;
            var ringLock = new object();
            tickThread = new Thread(() =>
            {
                while (Volatile.Read(ref tickStop) == 0)
                {
                    DwmFlush();
                    long q = Qpc;
                    lock (ringLock)
                    {
                        ring[ringHead] = q; ringHead = (ringHead + 1) % RingN;
                        if (ringCount < RingN) ringCount++;
                    }
                }
            })
            { IsBackground = true, Name = "r50-dwmflush" };
            tickThread.Start();
            await Task.Delay(250);

            (long last, double per) TickInfo()
            {
                lock (ringLock)
                {
                    if (ringCount == 0) return (0, Stopwatch.Frequency / 60.0);
                    long last = ring[(ringHead - 1 + RingN) % RingN];
                    int m = Math.Min(ringCount - 1, 64);
                    if (m <= 0) return (last, Stopwatch.Frequency / 60.0);
                    var iv = new double[m];
                    for (int i = 0; i < m; i++)
                    {
                        int i2 = (ringHead - 1 - i + RingN * 2) % RingN;
                        int i1 = (i2 - 1 + RingN) % RingN;
                        iv[i] = ring[i2] - ring[i1];
                    }
                    Array.Sort(iv);
                    return (last, iv[m / 2]);
                }
            }
            // next target = lastTick + k*period + phi, k smallest with target >= now + 0.3 ms; busy-spin to it
            (long target, long actual, long lastTick, double per) SpinToPhase(double phiMs)
            {
                var ti = TickInfo();
                long last = ti.last; double per = ti.per;
                if (per <= 0) per = Stopwatch.Frequency / 60.0;
                long ph = (long)Math.Round(phiMs / 1000.0 * Stopwatch.Frequency);
                long now = Qpc + (long)(0.0003 * Stopwatch.Frequency);
                long target = last + (long)(Math.Ceiling((now - last - ph) / per) * per) + ph;
                while (Qpc < target) { }
                return (target, Qpc, last, per);
            }

            // ================================================================ STAGE A
            Both("===== STAGE A START =====");

            // --- A1a seed-shift: start spring at analytic state s(τ0) instead of rest ---
            // T-A1a: focus gate set {21.8,3.3,6.8,8.3,33.3}, 5 reps each; no {0.0,16.7,50.0}
            double[] tau0s = { 21.8, 3.3, 6.8, 8.3, 33.3 };
            for (int ti = 0; ti < tau0s.Length; ti++)
            {
                double tau0Ms = tau0s[ti];
                for (int rep = 0; rep < 5; rep++)
                {
                    CompositionPropertySet? ps = null;
                    try
                    {
                        CompReset(0, "r50 A1a"); await WaitNear(0); await Task.Delay(150);
                        long qStart = Qpc;
                        ps = _comp!.CreatePropertySet();
                        ps.InsertScalar("d", 0f);
                        var ea = _comp.CreateExpressionAnimation("ps.d");
                        ea.SetReferenceParameter("ps", ps);
                        _visual!.StartAnimation("Translation.X", ea);
                        double amp = 240.0;
                        MfProbe.mf_spring_at(rr, zeta, 0, amp, 0, tau0Ms / 1000.0, out var iv, out var ivel);
                        var a = Sp(); a.InitialValue = (float)iv; a.InitialVelocity = (float)ivel; a.FinalValue = (float)amp;
                        ps.StartAnimation("d", a);
                        long qAnim = Qpc;
                        while (SecSince(qStart, Qpc) < 0.600) await Task.Delay(10);
                        var caps = CapsSince(qStart);
                        Both($"A1A tau0={F(tau0Ms, 1)} amp=240 rep={rep} iv={F(iv, 3)} ivel={F(ivel, 2)} tStart={F(SecSince(qStart, qAnim) * 1000, 3)} n={caps.Count} {Xs(caps)}");
                        Both(RawOf(caps, qStart));
                    }
                    catch (Exception ex) { Both($"A1A tau0={F(tau0Ms, 1)} rep={rep} THROW 0x{ex.HResult:X8} {ex.GetType().Name} {ex.Message}"); }
                    _visual!.StopAnimation("Translation.X");
                    try { ps?.StopAnimation("d"); } catch { }
                    CompReset(0, "r50 A1a end");
                    await Task.Delay(120);
                }
            }
            Both("A1A DONE");

            // --- A1b Δ linearity: #51 Part1 re-crop — negative Δ starts from origin=220 so −240 stays on-screen ---
            double[] amps = { 120, 240, -120, -240 };
            for (int ai = 0; ai < amps.Length; ai++)
            {
                double amp = amps[ai];
                double origin = amp < 0 ? 220.0 : 0.0;   // rest offset; RAW still vs cal cx0
                for (int rep = 0; rep < 5; rep++)
                {
                    CompositionPropertySet? ps = null;
                    try
                    {
                        CompReset(origin, "r50 A1b"); await WaitNear(origin, 6); await Task.Delay(150);
                        long qStart = Qpc;
                        ps = _comp!.CreatePropertySet();
                        ps.InsertScalar("d", 0f);
                        // Translation.X = origin + d; d: 0→amp keeps −Δ in crop when origin=220
                        var ea = _comp.CreateExpressionAnimation(origin == 0 ? "ps.d" : ("ps.d + " + origin.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                        ea.SetReferenceParameter("ps", ps);
                        _visual!.StartAnimation("Translation.X", ea);
                        var a = Sp(); a.InitialValue = 0f; a.InitialVelocity = 0f; a.FinalValue = (float)amp;
                        ps.StartAnimation("d", a);
                        long qAnim = Qpc;
                        while (SecSince(qStart, Qpc) < 0.600) await Task.Delay(10);
                        var caps = CapsSince(qStart);
                        Both($"A1B amp={F(amp, 0)} origin={F(origin, 0)} rep={rep} tStart={F(SecSince(qStart, qAnim) * 1000, 3)} n={caps.Count} {Xs(caps)}");
                        Both(RawOf(caps, qStart));
                    }
                    catch (Exception ex) { Both($"A1B amp={F(amp, 0)} origin={F(origin, 0)} rep={rep} THROW 0x{ex.HResult:X8} {ex.GetType().Name} {ex.Message}"); }
                    _visual!.StopAnimation("Translation.X");
                    try { ps?.StopAnimation("d"); } catch { }
                    CompReset(0, "r50 A1b end");
                    await Task.Delay(120);
                }
            }
            Both("A1B DONE");

            // --- A1c superposition: d1 step overlay on a running d0 (d0 NOT stopped); phase LOGGED only ---
            // #52 P1: 12 reps to correlate present time (tPresent0) vs frame phase (phiAct). No SpinToPhase here.
            for (int rep = 0; rep < 12; rep++)
            {
                CompositionPropertySet? ps = null;
                try
                {
                    CompReset(0, "r50 A1c"); await WaitNear(0); await Task.Delay(150);
                    long qStart = Qpc;
                    ps = _comp!.CreatePropertySet();
                    ps.InsertScalar("d0", 0f); ps.InsertScalar("d1", 0f);
                    var ea = _comp.CreateExpressionAnimation("ps.d0 + ps.d1");
                    ea.SetReferenceParameter("ps", ps);
                    _visual!.StartAnimation("Translation.X", ea);
                    var a0 = Sp(); a0.InitialValue = 0f; a0.InitialVelocity = 0f; a0.FinalValue = 240f;
                    ps.StartAnimation("d0", a0);                                    // do NOT stop d0
                    while (SecSince(qStart, Qpc) < 0.060 - 0.025) await Task.Delay(2);
                    while (SecSince(qStart, Qpc) < 0.060) { }
                    var tiD1 = TickInfo();                                           // frame tick at the d1 command
                    long lastTickD1 = tiD1.last; double perD1 = tiD1.per;
                    long qD1 = Qpc;
                    double tickPerMs = perD1 * 1000.0 / Stopwatch.Frequency;
                    double lastRel = (lastTickD1 - qStart) * 1000.0 / Stopwatch.Frequency;
                    double phiAct = (qD1 - lastTickD1) * 1000.0 / Stopwatch.Frequency;   // logged, not controlled
                    var a1 = Sp(); a1.InitialValue = 0f; a1.InitialVelocity = 0f; a1.FinalValue = -120f;
                    ps.StartAnimation("d1", a1);                                    // overlay on running d0
                    while (SecSince(qStart, Qpc) < 0.700) await Task.Delay(10);
                    var caps = CapsSince(qStart);
                    var capP0 = caps.FirstOrDefault(c => c.QpcArrive > qD1);         // first present after d1 command
                    double tPresent0 = capP0 != null ? SecSince(qStart, capP0.QpcArrive) * 1000 : double.NaN;
                    Both($"A1C tInt=60 d0amp=240 d1amp=-120 rep={rep} tD1={F(SecSince(qStart, qD1) * 1000, 3)} tickPer={F(tickPerMs, 3)} lastTick={F(lastRel, 3)} phiAct={F(phiAct, 3)} tPresent0={F(tPresent0, 3)} n={caps.Count} {Xs(caps)}");
                    Both(RawOf(caps, qStart));
                }
                catch (Exception ex) { Both($"A1C rep={rep} THROW 0x{ex.HResult:X8} {ex.GetType().Name} {ex.Message}"); }
                _visual!.StopAnimation("Translation.X");
                try { ps?.StopAnimation("d0"); ps?.StopAnimation("d1"); } catch { }
                CompReset(0, "r50 A1c end");
                await Task.Delay(120);
            }
            Both("A1C DONE");

            // --- A2 ε definition check: unified PS path (d0+d1), Sup-style interrupt at tInt=60 phi=5 bNew=0 ---
            Both("===== STAGE A2 START =====");
            for (int rep = 0; rep < 8; rep++)
            {
                CompositionPropertySet? ps = null;
                try
                {
                    CompReset(0, "r50 A2"); await WaitNear(0); await Task.Delay(150);
                    long qStart = Qpc;
                    ps = _comp!.CreatePropertySet();
                    ps.InsertScalar("d0", 0f); ps.InsertScalar("d1", 0f);
                    var ea = _comp.CreateExpressionAnimation("ps.d0 + ps.d1");
                    ea.SetReferenceParameter("ps", ps);
                    _visual!.StartAnimation("Translation.X", ea);
                    var a0 = Sp(); a0.InitialValue = 0f; a0.InitialVelocity = 0f; a0.FinalValue = 240f;
                    ps.StartAnimation("d0", a0);                                    // d0 never stopped
                    while (SecSince(qStart, Qpc) < 0.060 - 0.025) await Task.Delay(2);
                    while (SecSince(qStart, Qpc) < 0.060) { }
                    var sp = SpinToPhase(5);
                    long qSeed = sp.actual;
                    double tSeed = SecSince(qStart, qSeed);
                    MfProbe.mf_spring_at(rr, zeta, 0, 240, 0, tSeed, out var shX, out var shV);
                    var a1 = Sp(); a1.InitialValue = 0f; a1.InitialVelocity = 0f; a1.FinalValue = (float)(0 - 240);
                    ps.StartAnimation("d1", a1);
                    double tInt_m = SecSince(qStart, Qpc);
                    double phiAct = (qSeed - sp.lastTick) * 1000.0 / Stopwatch.Frequency;
                    double tickPerMs = sp.per * 1000.0 / Stopwatch.Frequency;
                    double lastRel = (sp.lastTick - qStart) * 1000.0 / Stopwatch.Frequency;
                    while (SecSince(qStart, Qpc) < 0.700) await Task.Delay(10);
                    var caps = CapsSince(qStart);
                    Both($"A2 path=PS tInt=60 phi=5 b=0 rep={rep} tSeed={F(tSeed * 1000, 3)} tInt_m={F(tInt_m * 1000, 3)} phiAct={F(phiAct, 3)} tickPer={F(tickPerMs, 3)} shX={F(shX, 3)} shV={F(shV, 2)} lastTick={F(lastRel, 3)} n={caps.Count}");
                    Both(RawOf(caps, qStart));
                }
                catch (Exception ex) { Both($"A2 rep={rep} THROW 0x{ex.HResult:X8} {ex.GetType().Name} {ex.Message}"); }
                _visual!.StopAnimation("Translation.X");
                try { ps?.StopAnimation("d0"); ps?.StopAnimation("d1"); } catch { }
                CompReset(0, "r50 A2 end");
                await Task.Delay(120);
            }
            Both("A2 DONE");
            Both("===== STAGE A END =====");

            // ================================================================ STAGE B
            Both("===== STAGE B START =====");

            async Task TrialB(string arm, double bNew, double phiMs, double tIntMs, int rep)
            {
                CompositionPropertySet? ps = null;
                try
                {
                    CompReset(0, "r50 B trial"); await WaitNear(0); await Task.Delay(200);
                    long qStart = Qpc;
                    long qSeed; double tSeed; double shX, shV;
                    if (arm == "Sup")
                    {
                        ps = _comp!.CreatePropertySet();
                        ps.InsertScalar("b", 0f); ps.InsertScalar("d1", 0f);
                        var ea = _comp.CreateExpressionAnimation("ps.b + ps.d1");
                        ea.SetReferenceParameter("ps", ps);
                        _visual!.StartAnimation("Translation.X", ea);
                        var ab = Sp(); ab.InitialValue = 0f; ab.InitialVelocity = 0f; ab.FinalValue = 240f;
                        ps.StartAnimation("b", ab);                              // base; never stop until cleanup
                        while (SecSince(qStart, Qpc) < tIntMs / 1000.0 - 0.025) await Task.Delay(2);
                        while (SecSince(qStart, Qpc) < tIntMs / 1000.0) { }
                        var sp = SpinToPhase(phiMs);
                        qSeed = sp.actual;
                        tSeed = SecSince(qStart, qSeed);
                        MfProbe.mf_spring_at(rr, zeta, 0, 240, 0, tSeed, out shX, out shV);   // log only
                        var d1 = Sp(); d1.InitialValue = 0f; d1.InitialVelocity = 0f; d1.FinalValue = (float)(bNew - 240);
                        ps.StartAnimation("d1", d1);
                        double tInt_m = SecSince(qStart, Qpc);
                        double phiAct = (qSeed - sp.lastTick) * 1000.0 / Stopwatch.Frequency;
                        double tickPerMs = sp.per * 1000.0 / Stopwatch.Frequency;
                        double lastRel = (sp.lastTick - qStart) * 1000.0 / Stopwatch.Frequency;
                        while (SecSince(qStart, Qpc) < 0.700) await Task.Delay(10);
                        var caps = CapsSince(qStart);
                        Both($"TRIAL arm={arm} b={F(bNew, 0)} tInt={F(tIntMs, 0)} phi={F(phiMs, 0)} rep={rep} tSeed={F(tSeed * 1000, 3)} tInt_m={F(tInt_m * 1000, 3)} phiAct={F(phiAct, 3)} tickPer={F(tickPerMs, 3)} shX={F(shX, 3)} shV={F(shV, 2)} lastTick={F(lastRel, 3)} n={caps.Count}");
                        Both(RawOf(caps, qStart));
                    }
                    else if (arm == "SupS2")
                    {
                        ps = _comp!.CreatePropertySet();
                        ps.InsertScalar("base", 0f); ps.InsertScalar("d0", 0f); ps.InsertScalar("d1", 0f); ps.InsertScalar("d2", 0f);
                        var ea = _comp.CreateExpressionAnimation("ps.base + ps.d0 + ps.d1 + ps.d2");
                        ea.SetReferenceParameter("ps", ps);
                        _visual!.StartAnimation("Translation.X", ea);            // bind ONCE
                        // base stays 0 (constant InsertScalar, never animated)
                        var a0 = Sp(); a0.InitialValue = 0f; a0.InitialVelocity = 0f; a0.FinalValue = 240f;
                        ps.StartAnimation("d0", a0);
                        while (SecSince(qStart, Qpc) < tIntMs / 1000.0 - 0.025) await Task.Delay(2);
                        while (SecSince(qStart, Qpc) < tIntMs / 1000.0) { }
                        var sp = SpinToPhase(phiMs);
                        qSeed = sp.actual;
                        tSeed = SecSince(qStart, qSeed);
                        MfProbe.mf_spring_at(rr, zeta, 0, 240, 0, tSeed, out shX, out shV);   // log only
                        // CRITICAL S2: do NOT CreateExpressionAnimation again; do NOT StartAnimation on Translation.X again
                        var d1 = Sp(); d1.InitialValue = 0f; d1.InitialVelocity = 0f; d1.FinalValue = (float)(bNew - 240);
                        ps.StartAnimation("d1", d1);                              // retarget only the idle slot
                        double tInt_m = SecSince(qStart, Qpc);
                        double phiAct = (qSeed - sp.lastTick) * 1000.0 / Stopwatch.Frequency;
                        double tickPerMs = sp.per * 1000.0 / Stopwatch.Frequency;
                        double lastRel = (sp.lastTick - qStart) * 1000.0 / Stopwatch.Frequency;
                        while (SecSince(qStart, Qpc) < 0.700) await Task.Delay(10);
                        var caps = CapsSince(qStart);
                        Both($"TRIAL arm={arm} b={F(bNew, 0)} tInt={F(tIntMs, 0)} phi={F(phiMs, 0)} rep={rep} tSeed={F(tSeed * 1000, 3)} tInt_m={F(tInt_m * 1000, 3)} phiAct={F(phiAct, 3)} tickPer={F(tickPerMs, 3)} shX={F(shX, 3)} shV={F(shV, 2)} lastTick={F(lastRel, 3)} n={caps.Count} s2=1");
                        Both(RawOf(caps, qStart));
                    }
                    else if (arm == "Bv")
                    {
                        var b1 = Sp(); b1.InitialValue = 0f; b1.InitialVelocity = 0f; b1.FinalValue = 240f;
                        _visual!.StartAnimation("Translation.X", b1);
                        while (SecSince(qStart, Qpc) < tIntMs / 1000.0 - 0.025) await Task.Delay(2);
                        while (SecSince(qStart, Qpc) < tIntMs / 1000.0) { }
                        var sp = SpinToPhase(phiMs);
                        qSeed = sp.actual;
                        tSeed = SecSince(qStart, qSeed);
                        MfProbe.mf_spring_at(rr, zeta, 0, 240, 0, tSeed, out shX, out shV);
                        var a = Sp();                                            // InitialValue NOT set (IV=null)
                        a.InitialVelocity = (float)shV; a.FinalValue = (float)bNew;
                        _visual!.StartAnimation("Translation.X", a);
                        double tInt_m = SecSince(qStart, Qpc);
                        double phiAct = (qSeed - sp.lastTick) * 1000.0 / Stopwatch.Frequency;
                        double tickPerMs = sp.per * 1000.0 / Stopwatch.Frequency;
                        double lastRel = (sp.lastTick - qStart) * 1000.0 / Stopwatch.Frequency;
                        while (SecSince(qStart, Qpc) < 0.700) await Task.Delay(10);
                        var caps = CapsSince(qStart);
                        Both($"TRIAL arm={arm} b={F(bNew, 0)} tInt={F(tIntMs, 0)} phi={F(phiMs, 0)} rep={rep} tSeed={F(tSeed * 1000, 3)} tInt_m={F(tInt_m * 1000, 3)} phiAct={F(phiAct, 3)} tickPer={F(tickPerMs, 3)} shX={F(shX, 3)} shV={F(shV, 2)} lastTick={F(lastRel, 3)} n={caps.Count}");
                        Both(RawOf(caps, qStart));
                    }
                    else // "base"
                    {
                        var b1 = Sp(); b1.InitialValue = 0f; b1.InitialVelocity = 0f; b1.FinalValue = 240f;
                        _visual!.StartAnimation("Translation.X", b1);
                        while (SecSince(qStart, Qpc) < tIntMs / 1000.0 - 0.025) await Task.Delay(2);
                        while (SecSince(qStart, Qpc) < tIntMs / 1000.0) { }
                        var sp = SpinToPhase(phiMs);                             // timing parity
                        qSeed = sp.actual;
                        tSeed = SecSince(qStart, qSeed);
                        MfProbe.mf_spring_at(rr, zeta, 0, 240, 0, tSeed, out shX, out shV);
                        double tInt_m = SecSince(qStart, Qpc);
                        double phiAct = (qSeed - sp.lastTick) * 1000.0 / Stopwatch.Frequency;
                        double tickPerMs = sp.per * 1000.0 / Stopwatch.Frequency;
                        double lastRel = (sp.lastTick - qStart) * 1000.0 / Stopwatch.Frequency;
                        while (SecSince(qStart, Qpc) < 0.700) await Task.Delay(10);
                        var caps = CapsSince(qStart);
                        Both($"TRIAL arm={arm} b={F(bNew, 0)} tInt={F(tIntMs, 0)} phi={F(phiMs, 0)} rep={rep} tSeed={F(tSeed * 1000, 3)} tInt_m={F(tInt_m * 1000, 3)} phiAct={F(phiAct, 3)} tickPer={F(tickPerMs, 3)} shX={F(shX, 3)} shV={F(shV, 2)} lastTick={F(lastRel, 3)} n={caps.Count}");
                        Both(RawOf(caps, qStart));
                    }
                }
                catch (Exception ex) { Both($"TRIAL arm={arm} b={F(bNew, 0)} tInt={F(tIntMs, 0)} phi={F(phiMs, 0)} rep={rep} THROW 0x{ex.HResult:X8} {ex.GetType().Name} {ex.Message}"); }
                // cleanup
                _visual!.StopAnimation("Translation.X");
                try { if (ps != null) { ps.StopAnimation("b"); ps.StopAnimation("base"); ps.StopAnimation("d0"); ps.StopAnimation("d1"); ps.StopAnimation("d2"); } } catch { }
                CompReset(0, "r50 B trial end");
                await Task.Delay(60);
            }

            double[] phisB = { 1, 5, 9, 13 };
            double[] bsB = { 0 };
            string[] armsB = { "Sup", "Bv", "base" };
            const double tIntB = 60.0;
            const int repsB = 2;
            var rndB = new Random(20261050);
            int blockIndexB = 0;
            for (int rep = 0; rep < repsB; rep++)
            {
                var blocks = new List<(int pi, int bi)>();
                for (int pi = 0; pi < phisB.Length; pi++) for (int bi = 0; bi < bsB.Length; bi++) blocks.Add((pi, bi));
                var order = blocks.OrderBy(_ => rndB.Next()).ToList();
                for (int bl = 0; bl < order.Count; bl++)
                {
                    int rot = (rep + blockIndexB) % armsB.Length;
                    for (int j = 0; j < armsB.Length; j++)
                    {
                        var arm = armsB[(j + rot) % armsB.Length];
                        await TrialB(arm, bsB[order[bl].bi], phisB[order[bl].pi], tIntB, rep);
                    }
                    blockIndexB++;
                }
                FlushLog(); Both($"REP B rep={rep} DONE");
            }
            Both("===== STAGE B END =====");
            Both("===== P14 AB DONE =====");

            // ================================================================ STAGE C
            if (!runC)
            {
                Both("===== STAGE C SKIPPED (pass --r3-50-full after A+B gate) =====");
                goto cleanup;
            }
            Both("===== STAGE C START =====");
            Both("LIVE_TERMS_CAP=3");   // d0+d1+d2; ship limitation; no 3-chain fold probe in this run

            // Stage C body is wrapped in a block so the !runC goto above does not skip local initializations.
            {
            async Task TrialC(string arm, double bNew, double phiMs, double tIntMs, int rep, string cellTag)
            {
                CompositionPropertySet? ps = null;
                try
                {
                    CompReset(0, "r50 C trial"); await WaitNear(0); await Task.Delay(200);
                    long qStart = Qpc;
                    long qSeed; double tSeed; double shX, shV;
                    if (arm.StartsWith("S1", StringComparison.Ordinal))
                    {
                        double epsHatMs = arm == "S1_0" ? 0 : (arm == "S1_7" ? 6.8 : 16.7);
                        double amp = bNew - 240.0;
                        ps = _comp!.CreatePropertySet();
                        ps.InsertScalar("base", 0f); ps.InsertScalar("d0", 0f); ps.InsertScalar("d1", 0f); ps.InsertScalar("d2", 0f);
                        var ea = _comp.CreateExpressionAnimation("ps.base + ps.d0 + ps.d1 + ps.d2");
                        ea.SetReferenceParameter("ps", ps);
                        _visual!.StartAnimation("Translation.X", ea);            // bind ONCE
                        var a0 = Sp(); a0.InitialValue = 0f; a0.InitialVelocity = 0f; a0.FinalValue = 240f;
                        ps.StartAnimation("d0", a0);
                        while (SecSince(qStart, Qpc) < tIntMs / 1000.0 - 0.025) await Task.Delay(2);
                        while (SecSince(qStart, Qpc) < tIntMs / 1000.0) { }
                        var sp = SpinToPhase(phiMs);
                        qSeed = sp.actual;
                        tSeed = SecSince(qStart, qSeed);
                        MfProbe.mf_spring_at(rr, zeta, 0, 240, 0, tSeed, out shX, out shV);   // shadow of base spring for log
                        // seed d1 at ε̂: analytic state s(ε̂) on the same step amplitude
                        MfProbe.mf_spring_at(rr, zeta, 0, amp, 0, epsHatMs / 1000.0, out var iv, out var ivel);
                        var d1 = Sp(); d1.InitialValue = (float)iv; d1.InitialVelocity = (float)ivel; d1.FinalValue = (float)amp;
                        ps.StartAnimation("d1", d1);                              // S2 discipline: idle slot only
                        double tInt_m = SecSince(qStart, Qpc);
                        double phiAct = (qSeed - sp.lastTick) * 1000.0 / Stopwatch.Frequency;
                        double tickPerMs = sp.per * 1000.0 / Stopwatch.Frequency;
                        double lastRel = (sp.lastTick - qStart) * 1000.0 / Stopwatch.Frequency;
                        while (SecSince(qStart, Qpc) < 0.700) await Task.Delay(10);
                        var caps = CapsSince(qStart);
                        Both($"TRIALC arm={arm} cell={cellTag} b={F(bNew, 0)} tInt={F(tIntMs, 0)} phi={F(phiMs, 0)} rep={rep} epsHat={F(epsHatMs, 1)} iv={F(iv, 3)} ivel={F(ivel, 2)} amp={F(amp, 0)} tSeed={F(tSeed * 1000, 3)} tInt_m={F(tInt_m * 1000, 3)} phiAct={F(phiAct, 3)} tickPer={F(tickPerMs, 3)} shX={F(shX, 3)} shV={F(shV, 2)} lastTick={F(lastRel, 3)} n={caps.Count} s2=1");
                        Both(RawOf(caps, qStart));
                    }
                    else // "base"
                    {
                        var b1 = Sp(); b1.InitialValue = 0f; b1.InitialVelocity = 0f; b1.FinalValue = 240f;
                        _visual!.StartAnimation("Translation.X", b1);
                        while (SecSince(qStart, Qpc) < tIntMs / 1000.0 - 0.025) await Task.Delay(2);
                        while (SecSince(qStart, Qpc) < tIntMs / 1000.0) { }
                        var sp = SpinToPhase(phiMs);                             // timing parity
                        qSeed = sp.actual;
                        tSeed = SecSince(qStart, qSeed);
                        MfProbe.mf_spring_at(rr, zeta, 0, 240, 0, tSeed, out shX, out shV);
                        double tInt_m = SecSince(qStart, Qpc);
                        double phiAct = (qSeed - sp.lastTick) * 1000.0 / Stopwatch.Frequency;
                        double tickPerMs = sp.per * 1000.0 / Stopwatch.Frequency;
                        double lastRel = (sp.lastTick - qStart) * 1000.0 / Stopwatch.Frequency;
                        while (SecSince(qStart, Qpc) < 0.700) await Task.Delay(10);
                        var caps = CapsSince(qStart);
                        Both($"TRIALC arm={arm} cell={cellTag} b={F(bNew, 0)} tInt={F(tIntMs, 0)} phi={F(phiMs, 0)} rep={rep} epsHat=NA iv=NA ivel=NA amp=NA tSeed={F(tSeed * 1000, 3)} tInt_m={F(tInt_m * 1000, 3)} phiAct={F(phiAct, 3)} tickPer={F(tickPerMs, 3)} shX={F(shX, 3)} shV={F(shV, 2)} lastTick={F(lastRel, 3)} n={caps.Count}");
                        Both(RawOf(caps, qStart));
                    }
                }
                catch (Exception ex) { Both($"TRIALC arm={arm} cell={cellTag} b={F(bNew, 0)} tInt={F(tIntMs, 0)} phi={F(phiMs, 0)} rep={rep} THROW 0x{ex.HResult:X8} {ex.GetType().Name} {ex.Message}"); }
                _visual!.StopAnimation("Translation.X");
                try { if (ps != null) { ps.StopAnimation("base"); ps.StopAnimation("d0"); ps.StopAnimation("d1"); ps.StopAnimation("d2"); } } catch { }
                CompReset(0, "r50 C trial end");
                await Task.Delay(60);
            }

            var cellsC = new[] {
                (tInt: 60.0, bNew: 0.0,   tag: "t060_b0"),
                (tInt: 60.0, bNew: 120.0, tag: "t060_b120"),
                (tInt: 60.0, bNew: 360.0, tag: "t060_rev"),
                (tInt: 40.0, bNew: 0.0,   tag: "t040_b0"),
            };
            string[] armsC = { "S1_0", "S1_7", "S1_16", "base" };
            double[] phisC = { 1, 5, 9, 13 };
            const int repsC = 4;
            var rndC = new Random(20261050 + 1);
            // shuffle cells × phi
            var cellPhi = new List<(int ci, int pi)>();
            for (int ci = 0; ci < cellsC.Length; ci++) for (int pi = 0; pi < phisC.Length; pi++) cellPhi.Add((ci, pi));
            var orderC = cellPhi.OrderBy(_ => rndC.Next()).ToList();
            int blockIndexC = 0;
            for (int rep = 0; rep < repsC; rep++)
            {
                for (int bl = 0; bl < orderC.Count; bl++)
                {
                    int rot = (rep + blockIndexC) % 4;
                    for (int j = 0; j < 4; j++)
                    {
                        var arm = armsC[(j + rot) % 4];
                        var cell = cellsC[orderC[bl].ci];
                        await TrialC(arm, cell.bNew, phisC[orderC[bl].pi], cell.tInt, rep, cell.tag);
                    }
                    blockIndexC++;
                }
                FlushLog(); Both($"REP C rep={rep} DONE");
            }
            Both("===== STAGE C END =====");
            Both("===== P14 RAW DONE =====");
            } // end Stage C block

        cleanup:
            Volatile.Write(ref tickStop, 1);
            try { tickThread.Join(500); } catch { }
            art.Close(); try { MfProbe.mf_cap_stop(); } catch { }
        }
        catch (Exception ex) { Both("R50 EXCEPTION " + ex); try { art.Close(); } catch { } try { MfProbe.mf_cap_stop(); } catch { } }
        finally
        {
            Volatile.Write(ref tickStop, 1);
            try { tickThread?.Join(300); } catch { }
            _suiteRunning = false; MeasureButton.IsEnabled = true; FlushLog(); try { CloseMainWindowSoon(); } catch { }
        }
    }
}

