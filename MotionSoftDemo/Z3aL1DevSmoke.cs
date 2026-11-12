// Z3A-CABI-L1 dev-only smoke. NOT a Z4 session. NOT PERF-CI. NOT adopt.
// Play → Retarget (mid-play interrupt, same property, cross-animation) → Cancel,
// plus Release, token load (including one JSON-Pointer failure), and reduced motion.
// Emits frozen E1 names: AnimationLifecycle, Retarget, FrameTick, PresentStats, IdleTransition, TokenLoad.
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MotionSoftDemo;

public static class Z3aL1DevSmoke
{
    // demo_motion.json springs.snappy (320ms, damping 0.78). Not a pacing offset.
    static readonly SpringParams Snappy = new(0.32, 0.78);

    public static int Run(TextWriter log, string tokenJsonPath)
    {
        if (log == null) throw new ArgumentNullException(nameof(log));
        int fails = 0;
        void Ok(bool cond, string msg)
        {
            if (cond) log.WriteLine("ok " + msg);
            else { fails++; log.WriteLine("FAIL " + msg); }
        }

        log.WriteLine("Z3A_DEV_ONLY=1");
        log.WriteLine("Z4=NO");
        log.WriteLine("PERF_CI=HOLD");
        log.WriteLine("provider=FluidMotion-Motion guid=8f3a2c1d-6b4e-4a91-9c7d-2e5f8a1b3c4d");
        log.WriteLine("note=dev smoke only; does not fill Z4 APPROVED and is not a Z4 session");

        try
        {
            CheckSurface(Ok);
            if (!File.Exists(tokenJsonPath))
            {
                Ok(false, "token json missing: " + tokenJsonPath);
                return Finish(log, fails);
            }
            string json = File.ReadAllText(tokenJsonPath);

            var seen = new HashSet<string>(StringComparer.Ordinal);
            using var mirror = new MotionEtwMirror(line =>
            {
                log.WriteLine(line);
                if (!line.StartsWith("ETW_EVENT name=", StringComparison.Ordinal)) return;
                int a = "ETW_EVENT name=".Length;
                int b = line.IndexOf(' ', a);
                seen.Add(b < 0 ? line[a..] : line[a..b]);
            }, enableVerbose: true);

            _ = MotionEtw.Log;
            Ok(MotionEtw.Log.Name == "FluidMotion-Motion", "provider name");
            Ok(MotionEtw.Log.Guid == MotionEtw.ProviderGuid, "provider guid");
            Ok(MotionEtw.IdAnimationLifecycle == 1 && MotionEtw.IdRetarget == 2 && MotionEtw.IdFrameTick == 3
                && MotionEtw.IdPresentStats == 4 && MotionEtw.IdIdleTransition == 5 && MotionEtw.IdTokenLoad == 6,
                "event ids 1-6");

            using (var real = MotionSystem.Create())
            {
                real.EmitFrameTickAccept = true;
                string? bad = real.LoadTokens("{\"version\":1,\"bogus\":1}", "bad_tokens.json");
                Ok(bad != null && bad.Contains("/bogus", StringComparison.Ordinal) && bad.Contains(':'),
                    "public Create + bad token diagnostic: " + (bad ?? "<null>"));
                string? good = new MotionTokens(real).Load(json);
                Ok(good == null, "MotionTokens.Load on fm_create system: " + (good ?? "ok"));
                real.Dispose();
                real.Dispose();
            }

            using var sys = MotionSystem.CreateManual();
            sys.EmitFrameTickAccept = true;
            Ok(!sys.IsRunning, "manual system idle before play");

            string? bad2 = new MotionTokens(sys).Load("{\"version\":1,\"bogus\":1}");
            Ok(bad2 != null && bad2.Contains("/bogus:", StringComparison.Ordinal),
                "MotionTokens.Load error has JSON-Pointer: " + (bad2 ?? "<null>").Replace('\n', '|'));

            string? loaded = sys.LoadTokens(json, "demo_motion.json");
            Ok(loaded == null, "MotionSystem.LoadTokens demo_motion.json: " + (loaded ?? "ok"));

            string? badAfter = sys.LoadTokens("{\"version\":1,\"bogus\":1}", "bad_tokens.json");
            Ok(badAfter != null && badAfter.Contains("/bogus", StringComparison.Ordinal), "bad load does not claim success");
            var still = sys.CreateAnimation(1, "demo.enter");
            Ok(still.Id != 0, "tokens remain after a rejected load");

            Ok(sys.CreateAnimation(1, "no.such").Id == 0, "unknown transition returns an empty handle");

            still.Play();
            int enterFrames = Pump(sys, 1000);
            Ok(enterFrames > 20 && enterFrames < 200 && !sys.IsRunning, "demo.enter ran then idled frames=" + enterFrames.ToString(CultureInfo.InvariantCulture));
            var enterEnd = sys.Current(1, "offsetX");
            var enterOp = sys.Current(1, "opacity");
            Ok(enterEnd is { Value: 0 } && enterOp is { Value: 1 },
                "demo.enter ends at offsetX=0 opacity=1");

            var animA = sys.CreateAnimation(1, null);
            Ok(animA.Id != 0, "CreateAnimation empty (retarget-only)");
            animA.Play();
            Ok(animA.Retarget("offsetX", 240, Snappy), "start retarget 240");
            Step(sys, 12);
            var mid = sys.Current(1, "offsetX");
            Ok(mid is { } midSample && midSample.Value > 0 && midSample.Velocity > 0,
                "mid-play sample value=" + Fmt(mid?.Value) + " vel=" + Fmt(mid?.Velocity));

            double v1 = mid?.Value ?? double.NaN;
            double vel1 = mid?.Velocity ?? double.NaN;
            Ok(animA.Retarget("offsetX", 40, Snappy), "mid-play interrupt retarget 40");
            var handoff = sys.Current(1, "offsetX");
            Ok(handoff is { } handoffSample
                && Math.Abs(handoffSample.Value - v1) < 1e-9
                && Math.Abs(handoffSample.Velocity - vel1) < 1e-9,
                "interrupt keeps value and velocity (no shadow IV)");

            Ok(animA.Retarget("offsetX", 200, Snappy), "same-property retarget 200");
            Step(sys, 8);
            Ok(animA.Retarget("offsetX", 80, Snappy), "same-property retarget 80");
            Step(sys, 8);

            animA.Cancel();
            var animB = sys.CreateAnimation(1, null);
            animB.Play();
            var cross = sys.Current(1, "offsetX");
            Ok(animB.Retarget("offsetX", 220, Snappy), "cross-animation retarget 220");
            var crossAfter = sys.Current(1, "offsetX");
            Ok(cross is { } crossSample && crossAfter is { } crossNow
                && Math.Abs(crossNow.Value - crossSample.Value) < 1e-6
                && Math.Abs(crossNow.Velocity - crossSample.Velocity) < 1e-6,
                "cross-animation handoff has no jump");
            Step(sys, 8);
            Ok(animB.Retarget("offsetX", 30, Snappy), "second mid-play interrupt");
            Step(sys, 6);
            Ok(animB.Retarget("offsetX", 160, Snappy), "sixth retarget");

            var beforeRelease = sys.Current(1, "offsetX");
            animB.Release();
            Ok(!animB.Retarget("offsetX", 10, Snappy), "Retarget after Release fails");
            Step(sys, 10);
            var afterRelease = sys.Current(1, "offsetX");
            Ok(beforeRelease is { } releasedFrom && afterRelease is { } releasedTo
                && (Math.Abs(releasedTo.Value - releasedFrom.Value) > 1e-4
                    || Math.Abs(releasedTo.Value - 160) < 1e-3),
                "Release drops the handle and motion continues value " + Fmt(beforeRelease?.Value) + " -> " + Fmt(afterRelease?.Value));
            Pump(sys, 1000);
            Ok(!sys.IsRunning, "idle after released motion finishes");

            var snap = sys.CreateAnimation(4, "demo.enter");
            snap.Play();
            Step(sys, 6);
            var snapMid = sys.Current(4, "opacity");
            Ok(snapMid is { } snapMidSample && snapMidSample.Value < 1.0, "reduced-motion setup is still in flight opacity=" + Fmt(snapMid?.Value));
            sys.SetReducedMotion(true);
            int snapFrames = Pump(sys, 10);
            var snapOp = sys.Current(4, "opacity");
            var snapX = sys.Current(4, "offsetX");
            Ok(!sys.IsRunning && snapOp is { Value: 1 } && snapX is { Value: 0 },
                "SetReducedMotion snaps to end frames=" + snapFrames.ToString(CultureInfo.InvariantCulture)
                + " opacity=" + Fmt(snapOp?.Value) + " offsetX=" + Fmt(snapX?.Value));

            var jump = sys.CreateAnimation(5, null);
            jump.Play();
            Ok(jump.Retarget("offsetX", 50, Snappy, from: 0), "retarget while reduced motion is on");
            Step(sys, 2);
            var jumped = sys.Current(5, "offsetX");
            Ok(jumped is { } jumpedSample && Math.Abs(jumpedSample.Value - 50) < 1e-6 && Math.Abs(jumpedSample.Velocity) < 1e-6,
                "reduced-motion Retarget lands on target with velocity 0 got " + Fmt(jumped?.Value) + " vel=" + Fmt(jumped?.Velocity));
            jump.Cancel();
            if (sys.IsRunning) Pump(sys, 5);
            sys.SetReducedMotion(false);

            MotionEtw.Log.PresentStats(sys.SessionId, 0, 16.666);
            string[] required = { "AnimationLifecycle", "Retarget", "FrameTick", "PresentStats", "IdleTransition", "TokenLoad" };
            foreach (string name in required)
                Ok(seen.Contains(name), "E1 event " + name);
            log.WriteLine("ETW_SEEN [" + string.Join(",", seen.OrderBy(n => n, StringComparer.Ordinal)) + "]");
        }
        catch (Exception ex)
        {
            fails++;
            log.WriteLine("FAIL exception " + ex.GetType().Name + ": " + ex.Message);
            log.WriteLine(ex.ToString());
        }

        return Finish(log, fails);
    }

    static void CheckSurface(Action<bool, string> ok)
    {
        var asm = typeof(MotionSystem).Assembly;
        string[] forbidden = { "MotionEngine", "MotionAnimation", "RiveView", "PointerFollow" };
        foreach (string name in forbidden)
        {
            bool found = asm.GetTypes().Any(t => t.Name == name || t.FullName == "MotionSoftDemo." + name);
            ok(!found, "no public-or-otherwise type " + name);
        }

        ok(HasMethod(typeof(MotionSystem), "Create", Type.EmptyTypes), "MotionSystem.Create()");
        ok(HasMethod(typeof(MotionSystem), "Dispose", Type.EmptyTypes), "MotionSystem.Dispose()");
        ok(HasMethod(typeof(MotionSystem), "LoadTokens", new[] { typeof(string) }), "MotionSystem.LoadTokens(json)");
        ok(HasMethod(typeof(MotionSystem), "SetReducedMotion", new[] { typeof(bool) }), "MotionSystem.SetReducedMotion(on)");
        ok(typeof(MotionSystem).GetProperty("IsRunning")?.CanRead == true, "MotionSystem.IsRunning");
        ok(HasMethod(typeof(MotionSystem), "OnFrame", Type.EmptyTypes), "MotionSystem.OnFrame()");
        ok(HasMethod(typeof(MotionSystem), "Current", new[] { typeof(ulong), typeof(string) }), "MotionSystem.Current(target, property)");
        ok(HasMethod(typeof(MotionSystem), "CreateAnimation", new[] { typeof(ulong), typeof(string) }), "MotionSystem.CreateAnimation(target, transition?)");
        ok(HasMethod(typeof(AnimationHandle), "Play", Type.EmptyTypes), "AnimationHandle.Play()");
        ok(HasMethod(typeof(AnimationHandle), "Cancel", Type.EmptyTypes), "AnimationHandle.Cancel()");
        ok(HasMethod(typeof(AnimationHandle), "Release", Type.EmptyTypes), "AnimationHandle.Release()");
        var retarget = typeof(AnimationHandle).GetMethod("Retarget");
        ok(retarget != null, "AnimationHandle.Retarget");
        if (retarget != null)
        {
            var ps = retarget.GetParameters();
            ok(ps.Length == 4
                && ps[0].Name == "property" && ps[0].ParameterType == typeof(string)
                && ps[1].Name == "target" && ps[1].ParameterType == typeof(double)
                && ps[2].Name == "spring" && ps[2].ParameterType == typeof(SpringParams)
                && ps[3].Name == "from" && ps[3].ParameterType == typeof(double?)
                && ps[3].HasDefaultValue && ps[3].DefaultValue == null,
                "Retarget(property, target, spring, from?)");
        }
        ok(HasMethod(typeof(MotionTokens), "Load", new[] { typeof(string) }), "MotionTokens.Load(json)");
        ok(!HasDefaultPointThree(typeof(MotionSystem)) && !HasDefaultPointThree(typeof(AnimationHandle)) && !HasDefaultPointThree(typeof(MotionTokens)),
            "no 0.3 default on L1 methods");
    }

    static bool HasMethod(Type type, string name, Type[] args) => type.GetMethod(name, args) != null;

    static bool HasDefaultPointThree(Type type)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        foreach (var method in type.GetMethods(flags))
        {
            foreach (var p in method.GetParameters())
            {
                if (!p.HasDefaultValue || p.DefaultValue is not double d) continue;
                if (Math.Abs(d - 0.3) < 1e-12) return true;
            }
        }
        return false;
    }

    static void Step(MotionSystem sys, int frames)
    {
        for (int i = 0; i < frames; i++)
        {
            sys.Advance(1.0 / 120.0);
            sys.OnFrame();
        }
    }

    static int Pump(MotionSystem sys, int max)
    {
        int n = 0;
        while (sys.IsRunning && n < max)
        {
            sys.Advance(1.0 / 120.0);
            sys.OnFrame();
            n++;
        }
        return n;
    }

    static string Fmt(double? v) => v.HasValue ? v.Value.ToString("G9", CultureInfo.InvariantCulture) : "null";

    static int Finish(TextWriter log, int fails)
    {
        log.WriteLine("Z3A_DEV_ONLY verdict=" + (fails == 0 ? "PASS" : "FAIL") + " fails=" + fails.ToString(CultureInfo.InvariantCulture) + " not_z4=1 perf_ci=HOLD");
        return fails == 0 ? 0 : 1;
    }
}
