// Z4 r1 session tooling. One process, one session. Not Z3a dev smoke. Not PERF-CI.
// Unpackaged output is the requested directory. Packaged output is ApplicationData LocalState
// (a real path). The requested directory is only a mirror, and may be virtualized.
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.UI.Xaml.Media;

namespace MotionSoftDemo;

public sealed partial class MainPage
{
    const int AppModelErrorNoPackage = 15700;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    static extern int GetCurrentPackageFullName(ref int length, StringBuilder? packageFullName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    static extern int GetCurrentPackageFamilyName(ref int length, StringBuilder? packageFamilyName);

    async Task RunZ4L1Session(string outDirArg, bool devAlsoSet)
    {
        bool packaged = TryPackageFullName(out string packageFullName);
        string packageFamily = packaged ? TryPackageFamilyName() : "";
        string localStateError = "";
        string? localState = packaged ? TryLocalState(out localStateError) : null;
        string requested = string.IsNullOrWhiteSpace(outDirArg)
            ? Path.Combine(Path.GetTempPath(), "z4_missing_outdir")
            : Path.GetFullPath(outDirArg);
        string sessionDir = packaged && !string.IsNullOrWhiteSpace(localState)
            ? Z4SessionCore.ResolveSessionDirectory(requested, packaged: true, localState)
            : requested;
        try { Directory.CreateDirectory(sessionDir); } catch { }
        string logPath = Path.Combine(sessionDir, "session.log");
        var log = new StringBuilder();
        void Write(string line)
        {
            log.Append(line).Append('\n');
            try { Log(line); } catch { }
        }

        var invalid = new List<string>();
        if (!Z4SessionCore.TryEnterSession(out string? twice))
            invalid.Add(twice ?? "session_run_in_process_twice");
        if (devAlsoSet) invalid.Add("dev_mode_instead_of_z4");
        if (string.IsNullOrWhiteSpace(outDirArg)) invalid.Add("missing_outdir");
        if (packaged && string.IsNullOrWhiteSpace(localState)) invalid.Add("packaged_localstate_unavailable");

        Z4BuildInfo stamped = Z4SessionCore.ReadBuildInfo(AppContext.BaseDirectory);
        string? header = Z4SessionCore.FindCompositionHeader(AppContext.BaseDirectory)
            ?? Z4SessionCore.FindCompositionHeader(Directory.GetCurrentDirectory());
        string compositionSha = "";
        string compositionRaw = "";
        if (header != null)
        {
            try
            {
                Z4TextHash hashed = Z4SessionCore.HashTextFile(header);
                compositionSha = hashed.Normalized;
                compositionRaw = hashed.Raw;
            }
            catch (Exception ex) { Write("composition hash failed: " + ex.Message); }
        }
        else
        {
            compositionSha = stamped.CompositionSha.ToLowerInvariant();
            compositionRaw = stamped.CompositionShaRaw.ToLowerInvariant();
        }
        if (!string.Equals(compositionSha, Z4SessionCore.ExpectedCompositionSha, StringComparison.OrdinalIgnoreCase))
            invalid.Add("composition_h_mismatch");

        string tip = stamped.Tip;
        if (string.IsNullOrWhiteSpace(tip)) tip = GitTip() ?? "";
        if (string.IsNullOrWhiteSpace(tip)) invalid.Add("undeclared_tip");
        string? expectedTip = Environment.GetEnvironmentVariable(Z4SessionCore.ExpectedTipEnv);
        if (!string.IsNullOrWhiteSpace(expectedTip)
            && !string.Equals(expectedTip.Trim(), tip, StringComparison.OrdinalIgnoreCase))
            invalid.Add("tip_mismatch");

        Write("Z4=YES");
        Write("Z3A_DEV_ONLY=0");
        Write("PERF_CI=HOLD");
        Write("gate=docs/z4_etw_l1_smoke_gate_r1_LOCKED.md");
        Write("gate_body_sha256=" + Z4SessionCore.GateBodySha);
        Write("pid=" + Environment.ProcessId.ToString(CultureInfo.InvariantCulture));
        Write("machine=" + Environment.MachineName);
        Write("packaged=" + (packaged ? "1" : "0") + " package=" + packageFullName);
        Write("tip=" + tip);
        string virtualGuess = packaged
            ? (Z4SessionCore.PredictVirtualizedMirror(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                packageFamily,
                requested) ?? "")
            : "";
        Write("composition_sha256=" + compositionSha);
        Write("composition_sha256_raw=" + compositionRaw);
        Write("composition_header=" + (header ?? ""));
        Write("RESOLVED_OUTDIR=" + sessionDir);
        Write("REQUESTED_OUTDIR=" + requested);
        Write("TRACE_PATH=" + Path.Combine(sessionDir, "trace.nettrace"));
        if (!string.IsNullOrEmpty(localStateError)) Write("LOCALSTATE " + localStateError);
        Write("provider=FluidMotion-Motion runtime_guid=" + MotionEtw.Log.Guid
            + " frozen_guid=" + Z4TraceCheck.FrozenGuid
            + " match=" + (MotionEtw.Log.Guid == Z4TraceCheck.FrozenGuid)
            + " name_hash_guid=" + Z4TraceCheck.NameHashGuid(Z4TraceCheck.ProviderName));

        var env = new Dictionary<string, object?>
        {
            ["machine"] = Environment.MachineName,
            ["os"] = Environment.OSVersion.VersionString,
            ["packaged"] = packaged,
            ["package_full_name"] = packageFullName,
            ["package_family_name"] = packageFamily,
            ["process_id"] = Environment.ProcessId,
            ["tip"] = tip,
            ["composition_sha256"] = compositionSha,
            ["composition_sha256_raw"] = compositionRaw,
            ["runtime_eventsource_guid"] = MotionEtw.Log.Guid.ToString(),
            ["frozen_guid"] = Z4TraceCheck.FrozenGuid.ToString(),
            ["runtime_matches_frozen"] = MotionEtw.Log.Guid == Z4TraceCheck.FrozenGuid,
            ["name_hash_guid"] = Z4TraceCheck.NameHashGuid(Z4TraceCheck.ProviderName).ToString(),
            ["composition_expected"] = Z4SessionCore.ExpectedCompositionSha,
            ["composition_header"] = header ?? "",
            ["outdir"] = sessionDir,
            ["resolved_outdir"] = sessionDir,
            ["requested_outdir"] = requested,
            ["trace"] = Path.Combine(sessionDir, "trace.nettrace"),
            ["virtualized_mirror_guess"] = virtualGuess,
            ["mirror_note"] = "pending",
            ["argv"] = string.Join(" ", Environment.GetCommandLineArgs()),
            ["windows_package_type"] = packaged ? "msix" : "unpackaged",
            ["virtualization_note"] = packaged
                ? "Canonical files are in resolved_outdir (package LocalState). dotnet-trace writes trace.nettrace there; same user, no admin. A copy into requested_outdir may be virtualized to virtualized_mirror_guess. Collect with an external Copy-Item from resolved_outdir."
                : "Unpackaged writes are not virtualized. resolved_outdir is the requested directory.",
        };
        try { Z4SessionCore.WriteEnv(Path.Combine(sessionDir, "env.json"), env); }
        catch (Exception ex) { Write("env.json failed: " + ex.Message); invalid.Add("env_write_failed"); }

        bool g1 = false, g2 = false, g3 = false, g4 = false, g5 = false, g6 = false;
        int retargets = 0, jumpFails = 0, idleEvents = 0, crossHits = 0, framesToStop = -1, callbacksIn5s = -1;
        var sessionDtMs = new List<double>();
        var continuitySteps = new Z4FrameSteps();
        double probeMaxStep = 0;
        string badDiagnostic = "";
        var categories = new List<string>();
        bool cancelSeen = false, releaseContinued = false;
        string traceDetail = "not_started";
        var traceNames = new List<string>();
        string guidStatus = Z4TraceCheck.GuidAbsent;
        string guidInFile = "";
        string guidReason = "";
        string tracePath = Path.Combine(sessionDir, "trace.nettrace");

        if (invalid.Count == 0)
        {
            var traceLog = new List<string>();
            try
            {
                (bool startedTrace, traceDetail) = await StartTrace(tracePath, traceLog, Write);
                if (!startedTrace)
                {
                    invalid.Add("eventpipe_not_started");
                    Write("TRACE not attached (" + traceDetail + "); E1 scenario not started");
                }
                else
                {
                var seen = new HashSet<string>(StringComparer.Ordinal);
                using var mirror = new MotionEtwMirror(line =>
                {
                    Write(line);
                    if (!line.StartsWith("ETW_EVENT name=", StringComparison.Ordinal)) return;
                    int a = "ETW_EVENT name=".Length;
                    int b = line.IndexOf(' ', a);
                    string name = b < 0 ? line[a..] : line[a..b];
                    seen.Add(name);
                    if (name == "IdleTransition") idleEvents++;
                }, enableVerbose: true);
                _ = MotionEtw.Log;

                int owner = Environment.CurrentManagedThreadId;
                int resumeMismatches = 0;
                void NoteThread()
                {
                    if (Environment.CurrentManagedThreadId != owner) resumeMismatches++;
                }

                using var system = MotionSystem.Create();
                system.EmitFrameTickAccept = true;
                int renders = 0;
                bool pumpOn = false;
                EventHandler<object>? onRendering = null;
                MotionSystem? bound = system;
                long prevFrameTs = 0;
                double prevFrameX = double.NaN;
                int frameWatch = 0;
                bool inContinuity = false;
                var watchedFrames = new List<string>();
                onRendering = (_, __) =>
                {
                    renders++;
                    long ts = Stopwatch.GetTimestamp();
                    try { bound?.OnFrame(); }
                    catch (Exception ex) { Write("ONFRAME " + ex.Message); }
                    double dtMs = prevFrameTs == 0 ? 0 : (ts - prevFrameTs) * 1000.0 / Stopwatch.Frequency;
                    prevFrameTs = ts;
                    double x = double.NaN, vel = double.NaN, step = double.NaN;
                    try
                    {
                        if (bound?.Current(BoxId, "offsetX") is MotionSample sample)
                        {
                            x = sample.Value;
                            vel = sample.Velocity;
                        }
                    }
                    catch { }
                    if (!double.IsNaN(prevFrameX) && !double.IsNaN(x)) step = Math.Abs(x - prevFrameX);
                    if (dtMs > 0) sessionDtMs.Add(dtMs);
                    if (inContinuity)
                    {
                        int beforeFails = continuitySteps.FailCount;
                        continuitySteps.Observe(renders, dtMs, step);
                        if (continuitySteps.FailCount > beforeFails)
                        {
                            Write("FRAME_STEP n=" + renders.ToString(CultureInfo.InvariantCulture)
                                + " dt_ms=" + F(dtMs) + " x=" + F(x) + " vel=" + F(vel)
                                + " step=" + F(step) + " limit=" + F(Z4SessionCore.MaxFrameStepDip, 0));
                        }
                    }
                    if (frameWatch > 0)
                    {
                        watchedFrames.Add("FRAME n=" + renders.ToString(CultureInfo.InvariantCulture)
                            + " dt_ms=" + F(dtMs) + " x=" + F(x) + " vel=" + F(vel) + " step=" + F(step));
                        frameWatch--;
                    }
                    if (!double.IsNaN(x)) prevFrameX = x;
                };
                system.FramePumpChanged += on =>
                {
                    pumpOn = on;
                    if (onRendering == null) return;
                    if (on) CompositionTarget.Rendering += onRendering;
                    else CompositionTarget.Rendering -= onRendering;
                };
                system.ValueChanged += (t, prop, v) =>
                {
                    if (t != BoxId || _compMode) return;
                    try
                    {
                        if (prop == "offsetX") BoxTranslate.X = v;
                        else if (prop == "opacity") AnimBox.Opacity = v;
                    }
                    catch { }
                };
                try { SetMode(false, "z4-l1-session"); } catch { }

                string badJson = "{\"version\":1,\"bogus\":1}";
                File.WriteAllText(Path.Combine(sessionDir, "bad_tokens.json"), badJson, new UTF8Encoding(false));
                string? bad = system.LoadTokens(badJson, "bad_tokens.json");
                badDiagnostic = bad ?? "";
                bool pointer = bad != null && bad.Contains("/bogus", StringComparison.Ordinal) && bad.Contains(':');
                Write("BAD_TOKEN " + (bad ?? "<null>"));

                string tokenPath = Path.Combine(AppContext.BaseDirectory, "demo_motion.json");
                string? goodErr = new MotionTokens(system).Load(File.ReadAllText(tokenPath));
                Write("TOKENS " + (goodErr ?? "ok") + " path=" + tokenPath);
                NoteThread();

                var enter = system.CreateAnimation(BoxId, "demo.enter");
                enter.Play();
                for (int i = 0; i < 40 && system.IsRunning; i++)
                {
                    await Task.Delay(50);
                    NoteThread();
                }
                Write("ENTER id=" + enter.Id.ToString(CultureInfo.InvariantCulture) + " running=" + system.IsRunning);

                var spring = new SpringParams(0.32, 0.78);
                var animA = system.CreateAnimation(BoxId, null);
                animA.Play();
                bool started = false;
                async Task<bool> Retarget(AnimationHandle anim, double to, string tag, string category)
                {
                    // pollMaxStep is the Task.Delay sample sum. It does not decide ②.
                    // Every OnFrame while inContinuity does, including frames past this 8-line log cap.
                    watchedFrames.Clear();
                    frameWatch = 8;
                    var beforeSample = system.Current(BoxId, "offsetX");
                    double before = beforeSample is MotionSample bs ? bs.Value : double.NaN;
                    double velB = beforeSample is MotionSample bv ? bv.Velocity : double.NaN;
                    long mark = Stopwatch.GetTimestamp();
                    bool api = anim.Retarget("offsetX", to, spring);
                    retargets++;
                    var afterSample = system.Current(BoxId, "offsetX");
                    double after = afterSample is MotionSample af ? af.Value : double.NaN;
                    double velA = afterSample is MotionSample av ? av.Velocity : double.NaN;
                    var steps = new List<double>();
                    var polls = new List<string>();
                    double prevPoll = after;
                    void NotePoll(int i, double x, double v)
                    {
                        long now = Stopwatch.GetTimestamp();
                        double dtMs = (now - mark) * 1000.0 / Stopwatch.Frequency;
                        mark = now;
                        double step = double.IsNaN(x) || double.IsNaN(prevPoll) ? double.NaN : Math.Abs(x - prevPoll);
                        polls.Add("POLL tag=" + tag + " i=" + i.ToString(CultureInfo.InvariantCulture)
                            + " dt_ms=" + F(dtMs) + " x=" + F(x) + " vel=" + F(v) + " step=" + F(step));
                        if (!double.IsNaN(x)) prevPoll = x;
                    }
                    NotePoll(0, after, velA);
                    for (int i = 0; i < 5; i++)
                    {
                        await Task.Delay(16);
                        NoteThread();
                        var stepSample = system.Current(BoxId, "offsetX");
                        double x = stepSample is MotionSample ss ? ss.Value : double.NaN;
                        double v = stepSample is MotionSample sv ? sv.Velocity : double.NaN;
                        steps.Add(x);
                        NotePoll(i + 1, x, v);
                    }
                    frameWatch = 0;
                    Z4Continuity c = Z4SessionCore.Evaluate(before, velB, after, velA, to, steps);
                    if (c.MaxStep > probeMaxStep) probeMaxStep = c.MaxStep;
                    bool handoffOk = api && c.JumpOk && c.VelOk && c.NoTeleport;
                    if (!handoffOk) jumpFails++;
                    if (!categories.Contains(category)) categories.Add(category);
                    Write("RETARGET tag=" + tag + " cat=" + category + " api=" + api
                        + " before=" + F(before) + " after=" + F(after) + " to=" + F(to, 1)
                        + " velB=" + F(velB) + " velA=" + F(velA) + " velSource=current_before_retarget"
                        + " jump=" + F(c.Jump) + " dynTol=" + F(c.DynTol)
                        + " pollMaxStep=" + F(c.MaxStep) + " handoffOk=" + handoffOk);
                    foreach (string poll in polls) Write(poll);
                    foreach (string frame in watchedFrames) Write(frame);
                    return handoffOk;
                }

                inContinuity = true;
                started = await Retarget(animA, 240, "R0_start", "same_prop");
                await Task.Delay(120);
                NoteThread();
                await Retarget(animA, 40, "R1_mid_interrupt", "mid_interrupt");
                await Task.Delay(80);
                await Retarget(animA, 200, "R2_same_prop", "same_prop");
                await Task.Delay(80);
                await Retarget(animA, 80, "R3_same_prop", "same_prop");
                await Task.Delay(80);
                animA.Cancel();
                cancelSeen = true;
                var animB = system.CreateAnimation(BoxId, null);
                animB.Play();
                await Retarget(animB, 220, "R4_cross_anim", "cross_anim");
                await Task.Delay(80);
                await Retarget(animB, 30, "R5_mid_interrupt", "mid_interrupt");
                await Task.Delay(60);
                await Retarget(animB, 160, "R6_same_prop", "same_prop");
                await Task.Delay(80);
                NoteThread();
                inContinuity = false;
                jumpFails += continuitySteps.FailCount;
                Write("CONTINUITY_FRAMES observed=" + continuitySteps.Observed.ToString(CultureInfo.InvariantCulture)
                    + " max_frame_step_dip=" + F(continuitySteps.MaxStep)
                    + " dt_ms=" + F(continuitySteps.MaxDtMs)
                    + " index=" + continuitySteps.MaxIndex.ToString(CultureInfo.InvariantCulture)
                    + " frame_step_fails=" + continuitySteps.FailCount.ToString(CultureInfo.InvariantCulture)
                    + " frame_step_limit_dip=" + F(Z4SessionCore.MaxFrameStepDip, 0)
                    + " probe_max_step_dip=" + F(probeMaxStep));
                var beforeRelease = system.Current(BoxId, "offsetX");
                double rel0 = beforeRelease is MotionSample r0 ? r0.Value : double.NaN;
                animB.Release();
                bool releasedBlocked = !animB.Retarget("offsetX", 10, spring);
                await Task.Delay(100);
                NoteThread();
                var afterRelease = system.Current(BoxId, "offsetX");
                double rel1 = afterRelease is MotionSample r1 ? r1.Value : double.NaN;
                releaseContinued = releasedBlocked && (Math.Abs(rel1 - rel0) > 1e-3 || Math.Abs(rel1 - 160) < 1.0);
                Write("RELEASE blocked=" + releasedBlocked + " x0=" + F(rel0) + " x1=" + F(rel1));
                for (int i = 0; i < 40 && system.IsRunning; i++)
                {
                    await Task.Delay(50);
                    NoteThread();
                }

                system.SetReducedMotion(false);
                var reduceAnim = system.CreateAnimation(BoxId, null);
                reduceAnim.Play();
                bool reduceStart = reduceAnim.Retarget("offsetX", 90, spring);
                await Task.Delay(40);
                NoteThread();
                system.SetReducedMotion(true);
                await Task.Delay(80);
                NoteThread();
                var reducedNow = system.Current(BoxId, "offsetX");
                double reducedX = double.NaN, reducedVel = double.NaN;
                bool reducedSnap = false;
                if (reducedNow is MotionSample reducedSample)
                {
                    reducedX = reducedSample.Value;
                    reducedVel = reducedSample.Velocity;
                    reducedSnap = Math.Abs(reducedX - 90) < 1.5 && Math.Abs(reducedVel) < 1.0 && !system.IsRunning;
                }
                var reduceJump = system.CreateAnimation(BoxId, null);
                reduceJump.Play();
                bool reduceRetarget = reduceJump.Retarget("offsetX", 15, spring, 0);
                await Task.Delay(50);
                NoteThread();
                var jumpNow = system.Current(BoxId, "offsetX");
                bool reducedJump = false;
                if (jumpNow is MotionSample jumpSample)
                    reducedJump = Math.Abs(jumpSample.Value - 15) < 1.0 && Math.Abs(jumpSample.Velocity) < 1.0;
                Write("REDUCED start=" + reduceStart + " snap=" + reducedSnap + " x=" + F(reducedX) + " vel=" + F(reducedVel)
                    + " retarget=" + reduceRetarget + " jump=" + reducedJump + " running=" + system.IsRunning);
                g3 = reduceStart && reducedSnap && reduceRetarget && reducedJump;
                system.SetReducedMotion(false);
                reduceAnim.Cancel();
                reduceJump.Cancel();
                for (int i = 0; i < 20 && system.IsRunning; i++)
                {
                    await Task.Delay(50);
                    NoteThread();
                }

                int mark = renders;
                var idleWait = Stopwatch.StartNew();
                while (pumpOn && idleWait.ElapsedMilliseconds < 500)
                {
                    await Task.Delay(16);
                    NoteThread();
                }
                framesToStop = renders - mark;
                int quietMark = renders;
                await Task.Delay(5000);
                NoteThread();
                callbacksIn5s = renders - quietMark;
                g4 = !pumpOn && framesToStop <= 2 && callbacksIn5s == 0 && idleEvents >= 1;
                Write("IDLE pumpOn=" + pumpOn + " framesToStop=" + framesToStop.ToString(CultureInfo.InvariantCulture)
                    + " callbacksIn5s=" + callbacksIn5s.ToString(CultureInfo.InvariantCulture)
                    + " idleEvents=" + idleEvents.ToString(CultureInfo.InvariantCulture));

                MotionEtw.Log.PresentStats(system.SessionId, 0, 16.666);
                Write("MIRROR_NAMES " + string.Join(",", seen.OrderBy(n => n, StringComparer.Ordinal)) + " (not G2-6; G2-6 reads the nettrace)");
                crossHits = system.CrossThreadHits + resumeMismatches;
                g5 = crossHits == 0;
                bool categoriesOk = categories.Contains("mid_interrupt") && categories.Contains("same_prop") && categories.Contains("cross_anim");
                g1 = pointer && goodErr == null && enter.Id != 0 && started && retargets >= 5 && categoriesOk && cancelSeen && releaseContinued;
                g2 = jumpFails == 0 && retargets >= 5;
                Write("FRAME_SUMMARY " + FrameSummaryText(sessionDtMs, continuitySteps, probeMaxStep));
                Write("G2 g1=" + g1 + " g2=" + g2 + " g3=" + g3 + " g4=" + g4 + " g5=" + g5
                    + " retargets=" + retargets.ToString(CultureInfo.InvariantCulture)
                    + " jumpFails=" + jumpFails.ToString(CultureInfo.InvariantCulture)
                    + " cross=" + crossHits.ToString(CultureInfo.InvariantCulture));

                bound = null;
                system.Dispose();
                }
            }
            catch (Exception ex)
            {
                Write("FAIL exception " + ex.GetType().Name + ": " + ex.Message);
                Write(ex.ToString());
                g1 = g2 = g3 = g4 = g5 = false;
            }

            if (traceProcHeld != null)
            {
                try
                {
                    bool exited = await Task.Run(() => traceProcHeld.WaitForExit(100_000));
                    Write(exited ? "TRACE exit " + traceProcHeld.ExitCode.ToString(CultureInfo.InvariantCulture) : "TRACE wait timeout");
                    if (!exited) invalid.Add("eventpipe_timeout");
                    Write("TRACE_LINES " + traceLog.Count.ToString(CultureInfo.InvariantCulture));
                }
                catch (Exception ex) { Write("TRACE wait " + ex.Message); }
            }
            if (!File.Exists(tracePath)) invalid.Add("missing_trace");
            else
            {
                Z4TraceReport traceReport = Z4TraceCheck.Check(tracePath);
                Write(Z4TraceCheck.Format(traceReport).TrimEnd());
                traceNames.AddRange(Z4TraceCheck.RequiredNames.Where(n => traceReport.Names.TryGetValue(n, out bool ok) && ok));
                guidStatus = traceReport.GuidStatus;
                guidInFile = traceReport.GuidInFile;
                bool runtimeMatch = MotionEtw.Log.Guid == Z4TraceCheck.FrozenGuid;
                if (!runtimeMatch || traceReport.GuidStatus == Z4TraceCheck.GuidMismatchFrozen)
                    guidReason = Z4TraceCheck.GuidMismatchFrozen;
                g6 = traceReport.Pass && runtimeMatch && traceReport.GuidStatus != Z4TraceCheck.GuidMismatchFrozen;
                if (!g6) Write("G2_6 missing=" + string.Join(",", traceReport.MissingNames) + " guid_reason=" + guidReason + " guid_status=" + guidStatus);
            }
        }

        string status = invalid.Count > 0 ? "INVALID" : (g1 && g2 && g3 && g4 && g5 && g6) ? "PASS" : "FAIL";
        var verdict = new Dictionary<string, object?>
        {
            ["mode"] = "Z4",
            ["not_z3a_dev"] = true,
            ["perf_ci"] = "HOLD",
            ["gate"] = "docs/z4_etw_l1_smoke_gate_r1_LOCKED.md",
            ["gate_body_sha256"] = Z4SessionCore.GateBodySha,
            ["status"] = status,
            ["invalid_reasons"] = invalid,
            ["g2"] = new Dictionary<string, object?>
            {
                ["1_l1_sequence"] = new Dictionary<string, object?>
                {
                    ["pass"] = g1,
                    ["bad_token_diagnostic"] = badDiagnostic,
                    ["retargets"] = retargets,
                    ["categories"] = categories,
                    ["cancel"] = cancelSeen,
                    ["release_motion_continued"] = releaseContinued,
                },
                ["2_continuity"] = new Dictionary<string, object?>
                {
                    ["pass"] = g2,
                    ["jumpFails"] = jumpFails,
                    ["max_frame_step_dip"] = continuitySteps.MaxStep,
                    ["max_frame_step_dt_ms"] = continuitySteps.MaxDtMs,
                    ["max_frame_step_index"] = continuitySteps.MaxIndex,
                    ["frame_steps_observed"] = continuitySteps.Observed,
                    ["frame_step_fails"] = continuitySteps.FailCount,
                    ["frame_step_limit_dip"] = Z4SessionCore.MaxFrameStepDip,
                    ["probe_max_step_dip"] = probeMaxStep,
                    ["frame_dt_ms"] = FrameDtSummary(sessionDtMs),
                    ["dynTol"] = "max(2, |vel|*0.040+1)",
                },
                ["3_reduced_motion"] = new Dictionary<string, object?> { ["pass"] = g3 },
                ["4_idle"] = new Dictionary<string, object?>
                {
                    ["pass"] = g4,
                    ["frames_to_stop"] = framesToStop,
                    ["callbacks_in_5s"] = callbacksIn5s,
                    ["idle_transition_events"] = idleEvents,
                },
                ["5_thread"] = new Dictionary<string, object?> { ["pass"] = g5, ["cross_thread_hits"] = crossHits },
                ["6_etw"] = new Dictionary<string, object?>
                {
                    ["pass"] = g6,
                    ["source"] = "eventpipe",
                    ["trace"] = tracePath,
                    ["collector"] = traceDetail,
                    ["names_found"] = traceNames,
                    ["runtime_eventsource_guid"] = MotionEtw.Log.Guid.ToString(),
                    ["frozen_guid"] = Z4TraceCheck.FrozenGuid.ToString(),
                    ["runtime_matches_frozen"] = MotionEtw.Log.Guid == Z4TraceCheck.FrozenGuid,
                    ["name_hash_guid"] = Z4TraceCheck.NameHashGuid(Z4TraceCheck.ProviderName).ToString(),
                    ["trace_guid_status"] = guidStatus,
                    ["trace_guid"] = guidInFile,
                    ["guid_reason"] = guidReason,
                },
            },
        };
        try { Z4SessionCore.WriteVerdict(Path.Combine(sessionDir, "verdict.json"), verdict); }
        catch (Exception ex) { Write("verdict.json failed: " + ex.Message); }
        Write("Z4_SESSION status=" + status);
        Write("RESOLVED_OUTDIR=" + sessionDir);
        Write("REQUESTED_OUTDIR=" + requested);
        string mirrorNote = MirrorSession(sessionDir, requested, packaged);
        env["mirror_note"] = mirrorNote;
        Write("MIRROR " + mirrorNote);
        try { Z4SessionCore.WriteEnv(Path.Combine(sessionDir, "env.json"), env); }
        catch (Exception ex) { Write("env.json final failed: " + ex.Message); }
        try { File.WriteAllText(logPath, log.ToString(), new UTF8Encoding(false)); } catch { }
        if (packaged && !string.Equals(Path.GetFullPath(sessionDir), Path.GetFullPath(requested), StringComparison.OrdinalIgnoreCase))
        {
            try { Z4SessionCore.CopyDirectoryFiles(sessionDir, requested); }
            catch { }
        }
        FlushLog();
        try { CloseMainWindowSoon(); } catch { }
        await Task.CompletedTask;
    }

    Process? traceProcHeld;

    async Task<(bool started, string detail)> StartTrace(string tracePath, List<string> traceLog, Action<string> write)
    {
        string? exe = Z4SessionCore.FindDotnetTrace();
        if (exe == null) return (false, "dotnet-trace not found");
        string? duration = Environment.GetEnvironmentVariable("MF_Z4_TRACE_DURATION");
        var psi = new ProcessStartInfo
        {
            FileName = exe,
            Arguments = Z4SessionCore.TraceArguments(Environment.ProcessId, tracePath, duration),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        Process proc;
        try { proc = Process.Start(psi) ?? throw new InvalidOperationException("Process.Start returned null"); }
        catch (Exception ex) { return (false, ex.GetType().Name + ": " + ex.Message); }
        traceProcHeld = proc;
        void OnLine(string? line)
        {
            if (string.IsNullOrEmpty(line)) return;
            traceLog.Add(line);
            write("TRACE " + line);
        }
        proc.OutputDataReceived += (_, e) => OnLine(e.Data);
        proc.ErrorDataReceived += (_, e) => OnLine(e.Data);
        try
        {
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
        }
        catch (Exception ex)
        {
            write("TRACE read " + ex.Message);
        }
        // dotnet-trace 10 buffers its banner when stdout is not a TTY. The nettrace header is the attach signal.
        var deadline = DateTime.UtcNow.AddSeconds(20);
        long lastLength = -1;
        while (DateTime.UtcNow < deadline)
        {
            if (proc.HasExited) return (false, "dotnet-trace exited " + proc.ExitCode.ToString(CultureInfo.InvariantCulture));
            if (File.Exists(tracePath))
            {
                long length = 0;
                try { length = new FileInfo(tracePath).Length; } catch { }
                if (length >= 8)
                {
                    try
                    {
                        using var stream = new FileStream(tracePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        var magic = new byte[8];
                        if (stream.Read(magic, 0, 8) == 8 && Encoding.ASCII.GetString(magic) == "Nettrace")
                            return (true, length > lastLength && lastLength >= 0 ? "file_growing" : "nettrace_header");
                    }
                    catch { }
                }
                lastLength = length;
            }
            await Task.Delay(200);
        }
        if (proc.HasExited) return (false, "dotnet-trace exited " + proc.ExitCode.ToString(CultureInfo.InvariantCulture));
        if (!File.Exists(tracePath)) return (false, "process_alive_no_file");
        return (true, "process_alive_file_present");
    }

    static string MirrorSession(string resolved, string requested, bool packaged)
    {
        if (!packaged) return "unpackaged_requested_is_resolved";
        try
        {
            if (string.Equals(Path.GetFullPath(resolved), Path.GetFullPath(requested), StringComparison.OrdinalIgnoreCase))
                return "resolved_equals_requested";
            Z4SessionCore.CopyDirectoryFiles(resolved, requested);
            return "copied_to_requested_from_inside_package";
        }
        catch (Exception ex)
        {
            return "copy_failed: " + ex.GetType().Name + ": " + ex.Message;
        }
    }

    static string? TryLocalState(out string error)
    {
        error = "";
        try
        {
            string path = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            if (string.IsNullOrWhiteSpace(path))
            {
                error = "LocalFolder.Path empty";
                return null;
            }
            return path;
        }
        catch (Exception ex)
        {
            error = ex.GetType().Name + ": " + ex.Message;
            return null;
        }
    }

    static string TryPackageFamilyName()
    {
        try
        {
            int length = 0;
            int rc = GetCurrentPackageFamilyName(ref length, null);
            if (rc == AppModelErrorNoPackage || length <= 1) return "";
            var sb = new StringBuilder(length);
            rc = GetCurrentPackageFamilyName(ref length, sb);
            return rc == 0 ? sb.ToString() : "";
        }
        catch
        {
            return "";
        }
    }

    static Dictionary<string, object?> FrameDtSummary(List<double> dts)
    {
        if (dts.Count == 0)
        {
            return new Dictionary<string, object?>
            {
                ["count"] = 0,
                ["min"] = null,
                ["p50"] = null,
                ["p95"] = null,
                ["max"] = null,
                ["mean"] = null,
            };
        }
        var ordered = dts.OrderBy(v => v).ToList();
        double mean = dts.Average();
        return new Dictionary<string, object?>
        {
            ["count"] = ordered.Count,
            ["min"] = ordered[0],
            ["p50"] = Percentile(ordered, 0.50),
            ["p95"] = Percentile(ordered, 0.95),
            ["max"] = ordered[^1],
            ["mean"] = mean,
        };
    }

    static string FrameSummaryText(List<double> dts, Z4FrameSteps frames, double probeMaxStep)
    {
        if (dts.Count == 0)
            return "n=0 probe_max_step=" + probeMaxStep.ToString("F4", CultureInfo.InvariantCulture);
        var ordered = dts.OrderBy(v => v).ToList();
        string N(double v) => v.ToString("F3", CultureInfo.InvariantCulture);
        return "n=" + ordered.Count.ToString(CultureInfo.InvariantCulture)
            + " dt_ms_min=" + N(ordered[0])
            + " dt_ms_p50=" + N(Percentile(ordered, 0.50))
            + " dt_ms_p95=" + N(Percentile(ordered, 0.95))
            + " dt_ms_max=" + N(ordered[^1])
            + " dt_ms_mean=" + N(dts.Average())
            + " max_frame_step_dip=" + frames.MaxStep.ToString("F4", CultureInfo.InvariantCulture)
            + " max_frame_step_dt_ms=" + N(frames.MaxDtMs)
            + " max_frame_step_index=" + frames.MaxIndex.ToString(CultureInfo.InvariantCulture)
            + " frame_steps_observed=" + frames.Observed.ToString(CultureInfo.InvariantCulture)
            + " probe_max_step_dip=" + probeMaxStep.ToString("F4", CultureInfo.InvariantCulture);
    }

    static double Percentile(IReadOnlyList<double> ordered, double p)
    {
        if (ordered.Count == 0) return double.NaN;
        double idx = (ordered.Count - 1) * p;
        int i = (int)idx;
        double f = idx - i;
        if (i >= ordered.Count - 1) return ordered[^1];
        return ordered[i] * (1.0 - f) + ordered[i + 1] * f;
    }

    static bool TryPackageFullName(out string fullName)
    {
        fullName = "";
        try
        {
            int length = 0;
            int rc = GetCurrentPackageFullName(ref length, null);
            if (rc == AppModelErrorNoPackage || length <= 1) return false;
            var sb = new StringBuilder(length);
            rc = GetCurrentPackageFullName(ref length, sb);
            if (rc != 0) return false;
            fullName = sb.ToString();
            return fullName.Length > 0;
        }
        catch
        {
            return false;
        }
    }

    static string? GitTip()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "rev-parse HEAD",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            };
            using var proc = Process.Start(psi);
            if (proc == null) return null;
            string text = proc.StandardOutput.ReadToEnd().Trim();
            proc.WaitForExit(5000);
            return proc.ExitCode == 0 && text.Length >= 7 ? text : null;
        }
        catch
        {
            return null;
        }
    }
}
