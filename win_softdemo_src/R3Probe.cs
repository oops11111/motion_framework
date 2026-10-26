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
}
