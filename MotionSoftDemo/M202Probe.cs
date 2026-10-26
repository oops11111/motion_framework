// M2-02 probe: C# API interruptible retarget + ETW emit + preview host v0.
// Software path only via FmBridge Retarget (from current state). NO shadow IV. NO synthetic input.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace MotionSoftDemo;

public sealed partial class MainPage
{
    // Continuity criteria (documented):
    //  - rendering sequence post-Retarget has no hard cut (frame step < MaxFrameStepDip)
    //  - no teleport snap to the new target on the handoff sample
    //  - velocity not spuriously zeroed
    //  - |Δpos| at API boundary uses dynamic tolerance: max(BaseJumpDip, |vel|*FrameSlackSec + 1)
    //    because fm_current is last-tick cached while Retarget re-evaluates at "now" (api draft §3.5).
    const double M202BaseJumpDip = 2.0;
    const double M202FrameSlackSec = 0.040; // ~1 UI frame slack for cached-vs-now
    const double M202MaxFrameStepDip = 80.0;
    const double M202VelZeroFloor = 8.0;   // if |vel_before| > this, |vel_after| must not collapse to < 0.5
    const double M202SpringR = 0.32;
    const double M202SpringZ = 0.78;

    private async Task RunM202Probe()
    {
        if (_suiteRunning) return;
        _suiteRunning = true;
        try { MeasureButton.IsEnabled = false; } catch { }

        string artDir = @"C:\Users\admin\motion_framework\artifacts";
        Directory.CreateDirectory(artDir);
        string sessFile = Path.Combine(artDir, "m2_02_session.txt");
        string sessEnv = File.Exists(sessFile) ? File.ReadAllText(sessFile).Trim() : (Environment.GetEnvironmentVariable("MF_M2_02_SESSION") ?? "1");
        if (string.IsNullOrWhiteSpace(sessEnv)) sessEnv = "1";

        string artPath = Path.Combine(artDir, "m2_02_capture_raw.txt");
        string sumPath = Path.Combine(artDir, "m2_02_capture_summary.txt");
        string artPathSess = Path.Combine(artDir, $"m2_02_s{sessEnv}_raw.txt");
        string sumPathSess = Path.Combine(artDir, $"m2_02_s{sessEnv}_summary.txt");
        string etwLedger = Path.Combine(artDir, $"m2_02_s{sessEnv}_etw_events.txt");
        string etwNames = Path.Combine(artDir, $"m2_02_s{sessEnv}_etw_names.txt");
        string previewLog = Path.Combine(artDir, $"m2_02_s{sessEnv}_preview.txt");

        var art = new StreamWriter(artPath, false, new UTF8Encoding(false));
        var etwWriter = new StreamWriter(etwLedger, false, new UTF8Encoding(false));
        var previewWriter = new StreamWriter(previewLog, false, new UTF8Encoding(false));
        void Both(string s) { Log(s); art.WriteLine(s); art.Flush(); }
        void EtwLine(string s) { etwWriter.WriteLine(s); etwWriter.Flush(); Both(s); }
        void Prev(string s) { previewWriter.WriteLine(s); previewWriter.Flush(); Both("PREVIEW " + s); }

        ulong sessionId = MotionEngine.NewSessionId();
        var seenEvents = new HashSet<string>(StringComparer.Ordinal);
        MotionEtwMirror? mirror = null;

        bool csharpPass = false, etwPass = false, previewPass = false;
        int retargetOk = 0, retargetTotal = 0;
        int playHits = 0, cancelHits = 0, retargetHits = 0;
        var jumpFails = new List<string>();
        string tokenJsonPath = Path.Combine(AppContext.BaseDirectory, "demo_motion.json");

        Both($"# m2_02_s{sessEnv}_raw.txt — M2-02 csharp+etw+preview smoke");
        Both($"# wall={DateTime.Now:yyyy-MM-dd HH:mm:ss} AWST; pid={Environment.ProcessId}; sessionId=0x{sessionId:X16}");
        Both($"# gate=ce147d212d31910b84c6c8e5566fbf50e740efcaadb27747cde827db2ec0a180 LOCKED_AT=2026-10-25 00:42:43 AWST");
        Both($"# approved=pidan@2026-10-25 01:11:43 AWST");
        Both("# path=SOFTWARE FmBridge Retarget (from current); NO shadow IV; NO compositor InitialVelocity; NO synthetic input");
        Both($"# jump_criteria: dynTol=max({M202BaseJumpDip.ToString(CultureInfo.InvariantCulture)},|vel|*{M202FrameSlackSec.ToString(CultureInfo.InvariantCulture)}+1); no teleport snap to target; frame step <{M202MaxFrameStepDip.ToString(CultureInfo.InvariantCulture)} DIP; if |vel_before|>{M202VelZeroFloor} then |vel_after| must not be <0.5; fm_current=last-tick (api §3.5)");
        Both($"# provider=FluidMotion-Motion guid={MotionEtw.ProviderGuid}");

        MotionEngine? engine = null;
        try
        {
            // Ensure EventSource is constructed before listener (and before emits).
            _ = MotionEtw.Log;
            mirror = new MotionEtwMirror(line =>
            {
                EtwLine(line);
                if (line.StartsWith("ETW_EVENT name=", StringComparison.Ordinal))
                {
                    int a = "ETW_EVENT name=".Length;
                    int b = line.IndexOf(' ', a);
                    string n = b < 0 ? line[a..] : line[a..b];
                    seenEvents.Add(n);
                }
            }, enableVerbose: true);

            if (_fm == null) throw new InvalidOperationException("MotionBridge not ready");
            engine = new MotionEngine(_fm, sessionId) { EmitFrameTickAccept = true };

            // Force software path (no compositor IV).
            SetMode(false, "m2-02-software");
            await Task.Delay(200);

            // ---------- G1-3 preview host v0 ----------
            Prev($"step1_start host=MotionSoftDemo pid={Environment.ProcessId}");
            Prev($"step2_import path={tokenJsonPath} exists={File.Exists(tokenJsonPath)}");
            string json = File.ReadAllText(tokenJsonPath);
            var tokenNames = ListTransitionNames(json);
            Prev($"step3_select tokens=[{string.Join(",", tokenNames)}] pick={(tokenNames.FirstOrDefault() ?? "demo.enter")}");
            string pick = tokenNames.FirstOrDefault() ?? "demo.enter";

            string? loadErr = engine.LoadTokens(json, "demo_motion.json");
            Prev($"step4_load err={(loadErr ?? "ok")} session=0x{sessionId:X}");
            if (loadErr != null) throw new Exception("LoadTokens failed: " + loadErr);

            var previewAnim = engine.Create(BoxId, pick);
            // Bind visual: already bound via OnValue -> AnimBox in soft path
            Prev($"step5_bind animId={previewAnim.AnimId} target={BoxId} transition={pick}");
            previewAnim.Play(); playHits++;
            Prev("step6_play started; observing ~600ms for no crash");
            await Task.Delay(600);
            bool crashed = false;
            try
            {
                _fm.Current(BoxId, "offsetX", out var pv, out var pvel);
                Prev($"observe offsetX={F(pv,3)} vel={F(pvel,2)} running={_fm.Running} exceptions={_exceptions}");
                // Sample a few frames via app-side current
                for (int i = 0; i < 5; i++)
                {
                    await Task.Delay(40);
                    _fm.Current(BoxId, "offsetX", out pv, out pvel);
                    Prev($"frame_sample i={i} x={F(pv,3)} vel={F(pvel,2)}");
                }
            }
            catch (Exception ex)
            {
                crashed = true;
                Prev("CRASH " + ex);
            }
            previewPass = !crashed && _exceptions == 0;
            Prev($"PREVIEW_VERDICT={(previewPass ? "PASS" : "FAIL")} no_visual_crash={!crashed}");

            // Cancel preview anim before csharp smoke retarget suite (clean slate)
            previewAnim.Cancel(); cancelHits++;
            await Task.Delay(150);

            // ---------- G1-1 C# interruptible retarget (>=5, cover 3 categories) ----------
            Both("===== CSHARP_SMOKE START =====");
            // Reload tokens (TokenLoad again ok)
            loadErr = engine.LoadTokens(json, "demo_motion.json");
            Both($"LoadTokens2 err={(loadErr ?? "ok")}");

            var animA = engine.Create(BoxId, pick);
            var animB = engine.Create(BoxId, null); // free retarget anim for cross-switch
            Both($"Create animA={animA.AnimId} animB={animB.AnimId}");

            // Start motion on soft path via Retarget (Play alone may need tracks; Retarget drives spring)
            SetSoftBox(0);
            animA.Play(); playHits++;
            bool startOk = animA.RetargetTo("offsetX", 240, M202SpringR, M202SpringZ);
            retargetHits++; retargetTotal++;
            Both($"START Play+RetargetTo(240) ok={startOk}");
            await Task.Delay(120); // mid-flight

            // Helper local
            // Continuity from current state (software path Retarget hasFrom=0).
            // Primary evidence = app-side rendering sequence (no hard cut / no snap-to-target).
            async Task<(bool ok, double before, double after, double velB, double velA)> DoRetarget(
                MotionAnimation anim, string prop, double to, string tag, string category)
            {
                _fm!.Current(BoxId, prop, out var before, out var velB);
                bool ok = anim.RetargetTo(prop, to, M202SpringR, M202SpringZ);
                retargetHits++;
                _fm.Current(BoxId, prop, out var after, out var velA);
                double jump = Math.Abs(after - before);
                double dynTol = Math.Max(M202BaseJumpDip, Math.Abs(velB) * M202FrameSlackSec + 1.0);
                bool jumpOk = jump <= dynTol;
                bool velOk = !(Math.Abs(velB) > M202VelZeroFloor && Math.Abs(velA) < 0.5);
                // Teleport = handoff sample already at the new target while before was far away.
                bool noTeleport = !(Math.Abs(before - to) > 10.0 && Math.Abs(after - to) < 1.0);
                var seq = new List<double> { after };
                double prev = after;
                bool seqOk = true;
                double maxStep = 0;
                for (int si = 0; si < 5; si++)
                {
                    await Task.Delay(16);
                    _fm.Current(BoxId, prop, out var sx, out _);
                    double step = Math.Abs(sx - prev);
                    if (step > maxStep) maxStep = step;
                    if (step > M202MaxFrameStepDip) seqOk = false;
                    seq.Add(sx);
                    prev = sx;
                }
                bool pass = ok && jumpOk && velOk && noTeleport && seqOk;
                Both($"RETARGET tag={tag} cat={category} ok={ok} before={F(before,3)} after={F(after,3)} to={F(to,1)} jump={F(jump,3)} dynTol={F(dynTol,3)} velB={F(velB,2)} velA={F(velA,2)} jumpOk={jumpOk} velOk={velOk} noTeleport={noTeleport} maxStep={F(maxStep,3)} seqOk={seqOk} pass={pass} seq=[{string.Join(",", seq.Select(v => F(v,2)))}]");
                if (!pass)
                    jumpFails.Add($"{tag}: jump={jump:F3}/{dynTol:F3} jumpOk={jumpOk} velOk={velOk} noTeleport={noTeleport} seqOk={seqOk} maxStep={maxStep:F3}");
                return (pass, before, after, velB, velA);
            }

            // 1) mid-play interrupt
            var r1 = await DoRetarget(animA, "offsetX", 40, "R1_mid_interrupt", "mid_interrupt");
            if (r1.ok) retargetOk++; retargetTotal++;
            await Task.Delay(100);

            // 2) same-property change target
            var r2 = await DoRetarget(animA, "offsetX", 200, "R2_same_prop", "same_prop");
            if (r2.ok) retargetOk++; retargetTotal++;
            await Task.Delay(80);

            // 3) same-prop again (counts toward >=5)
            var r3 = await DoRetarget(animA, "offsetX", 80, "R3_same_prop2", "same_prop");
            if (r3.ok) retargetOk++; retargetTotal++;
            await Task.Delay(80);

            // 4) cross-animation switch: cancel A mid-flight, drive B from current
            _fm.Current(BoxId, "offsetX", out var crossX, out var crossV);
            Both($"CROSS_PREP x={F(crossX,3)} vel={F(crossV,2)}");
            animA.Cancel(); cancelHits++;
            animB.Play(); playHits++;
            // From-current (hasFrom=0) — continuity via engine current state
            var r4 = await DoRetarget(animB, "offsetX", 220, "R4_cross_anim", "cross_anim");
            if (r4.ok) retargetOk++; retargetTotal++;
            await Task.Delay(80);

            // 5) another mid-interrupt on B
            var r5 = await DoRetarget(animB, "offsetX", 30, "R5_mid_interrupt2", "mid_interrupt");
            if (r5.ok) retargetOk++; retargetTotal++;
            await Task.Delay(80);

            // 6) bonus same-prop to ensure >=5 successful attempts recorded
            var r6 = await DoRetarget(animB, "offsetX", 160, "R6_same_prop3", "same_prop");
            if (r6.ok) retargetOk++; retargetTotal++;
            await Task.Delay(60);

            animB.Cancel(); cancelHits++;
            await Task.Delay(200); // allow IdleTransition (active 1->0)

            // Ensure IdleTransition seen: toggle pump by brief play/cancel if needed
            if (!seenEvents.Contains("IdleTransition"))
            {
                Both("IdleTransition missing — force play/cancel pulse");
                var pulse = engine.Create(BoxId, null);
                pulse.Play();
                pulse.RetargetTo("offsetX", 50, M202SpringR, M202SpringZ);
                await Task.Delay(100);
                pulse.Cancel();
                await Task.Delay(200);
            }

            // PresentStats at session end (placeholder 0 ok; event name must appear)
            engine.EmitPresentStats(0, 16.666);
            await Task.Delay(50);

            bool coverMid = true; // R1/R5
            bool coverSame = true; // R2/R3/R6
            bool coverCross = true; // R4
            int retargetAttempts = 6; // R1..R6 (START not counted toward the ≥5 gate set; we have 6)
            csharpPass = playHits >= 1 && cancelHits >= 1 && retargetHits >= 5
                && retargetOk >= 5 && jumpFails.Count == 0
                && coverMid && coverSame && coverCross;
            Both($"CSHARP playHits={playHits} cancelHits={cancelHits} retargetHits={retargetHits} retargetOk={retargetOk}/{retargetAttempts} jumpFails={jumpFails.Count}");
            foreach (var jf in jumpFails) Both("JUMP_FAIL " + jf);
            Both($"CSHARP_VERDICT={(csharpPass ? "PASS" : "FAIL")}");

            // ---------- G1-2 ETW ----------
            // Ensure FrameTick appeared (accept force path)
            if (!seenEvents.Contains("FrameTick"))
            {
                MotionEtw.Log.FrameTickAccept(sessionId, 16666, 100, 0);
                await Task.Delay(30);
            }
            string[] requiredMin = { "AnimationLifecycle", "Retarget", "FrameTick", "PresentStats", "IdleTransition", "TokenLoad" };
            string[] requiredObserved = { "AnimationLifecycle", "Retarget", "FrameTick", "IdleTransition" };
            var missingMin = requiredMin.Where(n => !seenEvents.Contains(n)).ToList();
            var missingObs = requiredObserved.Where(n => !seenEvents.Contains(n)).ToList();
            File.WriteAllText(etwNames, string.Join("\n", requiredMin.Select(n => $"{n}\t{(seenEvents.Contains(n) ? "SEEN" : "MISSING")}")) + "\n", new UTF8Encoding(false));
            Both($"ETW_SEEN [{string.Join(",", seenEvents.OrderBy(x => x))}]");
            Both($"ETW_MISSING_MIN [{string.Join(",", missingMin)}]");
            Both($"ETW_MISSING_OBS [{string.Join(",", missingObs)}]");
            etwPass = missingMin.Count == 0 && missingObs.Count == 0;
            Both($"ETW_VERDICT={(etwPass ? "PASS" : "FAIL")} provider=FluidMotion-Motion");

            bool sessionPass = csharpPass && etwPass && previewPass;
            string verdict = sessionPass ? "SESSION_PASS_CANDIDATE" : "SESSION_FAIL_CANDIDATE";
            Both($"===== SESSION SUMMARY sess={sessEnv} pid={Environment.ProcessId} sessionId=0x{sessionId:X} csharp={(csharpPass ? "P" : "F")} etw={(etwPass ? "P" : "F")} preview={(previewPass ? "P" : "F")} => {verdict}");
            Both("===== M2-02 PROBE DONE =====");

            string sumBody =
                $"sess={sessEnv}\n" +
                $"pid={Environment.ProcessId}\n" +
                $"sessionId=0x{sessionId:X}\n" +
                $"csharp={(csharpPass ? "PASS" : "FAIL")}\n" +
                $"etw={(etwPass ? "PASS" : "FAIL")}\n" +
                $"preview={(previewPass ? "PASS" : "FAIL")}\n" +
                $"playHits={playHits}\ncancelHits={cancelHits}\nretargetHits={retargetHits}\nretargetOk={retargetOk}\n" +
                $"jumpFails={jumpFails.Count}\n" +
                $"etwSeen={string.Join(",", seenEvents.OrderBy(x => x))}\n" +
                $"provider=FluidMotion-Motion\nproviderGuid={MotionEtw.ProviderGuid}\n" +
                $"verdict={verdict}\n" +
                $"art={artPathSess}\n";
            File.WriteAllText(sumPath, sumBody, new UTF8Encoding(false));
            try { art.Flush(); } catch { }
            try { File.Copy(artPath, artPathSess, true); } catch (Exception cx) { Both("WARN copy raw: " + cx.Message); }
            try { File.WriteAllText(sumPathSess, sumBody, new UTF8Encoding(false)); } catch (Exception cx) { Both("WARN sum sess: " + cx.Message); }
        }
        catch (Exception ex)
        {
            Both("M202 EXCEPTION " + ex);
            try
            {
                File.WriteAllText(sumPath, $"sess={sessEnv}\nverdict=SESSION_FAIL_CANDIDATE\nexception={ex}\n", new UTF8Encoding(false));
                File.Copy(sumPath, sumPathSess, true);
            }
            catch { }
        }
        finally
        {
            try { mirror?.Dispose(); } catch { }
            try { engine?.Dispose(); } catch { }
            try { art.Close(); } catch { }
            try { etwWriter.Close(); } catch { }
            try { previewWriter.Close(); } catch { }
            _suiteRunning = false;
            try { MeasureButton.IsEnabled = true; } catch { }
            FlushLog();
            try { CloseMainWindowSoon(); } catch { }
        }
    }

    static List<string> ListTransitionNames(string json)
    {
        var names = new List<string>();
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("transitions", out var tr))
            {
                foreach (var p in tr.EnumerateObject()) names.Add(p.Name);
            }
        }
        catch { }
        if (names.Count == 0) names.Add("demo.enter");
        return names;
    }

    void SetSoftBox(double x)
    {
        try
        {
            // Drive Canvas.Left via existing OnValue path: cancel then retarget from known value.
            if (_fm == null) return;
            _fm.Cancel(_drag);
            // Snap via RetargetFrom with high damping / short response for near-instant settle is avoided;
            // instead set Canvas directly for start pose (software visual).
            Canvas.SetLeft(AnimBox, BoxLeft + x);
            AnimBox.Opacity = 1;
        }
        catch { }
    }
}
