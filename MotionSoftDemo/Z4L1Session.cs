// Z4 r1 session tooling. One process, one session. Not Z3a dev smoke. Not PERF-CI.
// Output is only the directory passed to --z4-l1-session / MF_Z4_L1_SESSION.
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

    async Task RunZ4L1Session(string outDirArg, bool devAlsoSet)
    {
        string sessionDir = string.IsNullOrWhiteSpace(outDirArg)
            ? Path.Combine(Path.GetTempPath(), "z4_missing_outdir")
            : Path.GetFullPath(outDirArg);
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

        bool packaged = TryPackageFullName(out string packageFullName);
        var (stampedTip, stampedSha) = Z4SessionCore.ReadBuildInfo(AppContext.BaseDirectory);
        string? header = Z4SessionCore.FindCompositionHeader(AppContext.BaseDirectory)
            ?? Z4SessionCore.FindCompositionHeader(Directory.GetCurrentDirectory());
        string compositionSha = "";
        if (header != null)
        {
            try { compositionSha = Z4SessionCore.Sha256File(header); }
            catch (Exception ex) { Write("composition hash failed: " + ex.Message); }
        }
        else if (!string.IsNullOrWhiteSpace(stampedSha))
            compositionSha = stampedSha.ToLowerInvariant();
        if (!string.Equals(compositionSha, Z4SessionCore.ExpectedCompositionSha, StringComparison.OrdinalIgnoreCase))
            invalid.Add("composition_h_mismatch");

        string tip = stampedTip;
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
        Write("composition_sha256=" + compositionSha);
        Write("composition_header=" + (header ?? ""));
        Write("outdir=" + sessionDir);
        Write("provider=FluidMotion-Motion guid=8f3a2c1d-6b4e-4a91-9c7d-2e5f8a1b3c4d");

        var env = new Dictionary<string, object?>
        {
            ["machine"] = Environment.MachineName,
            ["os"] = Environment.OSVersion.VersionString,
            ["packaged"] = packaged,
            ["package_full_name"] = packageFullName,
            ["process_id"] = Environment.ProcessId,
            ["tip"] = tip,
            ["composition_sha256"] = compositionSha,
            ["composition_expected"] = Z4SessionCore.ExpectedCompositionSha,
            ["composition_header"] = header ?? "",
            ["outdir"] = sessionDir,
            ["argv"] = string.Join(" ", Environment.GetCommandLineArgs()),
            ["windows_package_type"] = packaged ? "msix" : "unpackaged",
            ["virtualization_note"] = packaged
                ? "Manifest excludes %USERPROFILE%\\motion_framework\\z4 and %LOCALAPPDATA%\\MotionSoftDemoZ4 from file-system write virtualization. Other paths may appear under %LOCALAPPDATA%\\Packages\\<package>\\LocalCache."
                : "Unpackaged writes are not virtualized.",
        };
        try { Z4SessionCore.WriteEnv(Path.Combine(sessionDir, "env.json"), env); }
        catch (Exception ex) { Write("env.json failed: " + ex.Message); invalid.Add("env_write_failed"); }

        bool g1 = false, g2 = false, g3 = false, g4 = false, g5 = false, g6 = false;
        int retargets = 0, jumpFails = 0, idleEvents = 0, crossHits = 0, framesToStop = -1, callbacksIn5s = -1;
        string badDiagnostic = "";
        var categories = new List<string>();
        bool cancelSeen = false, releaseContinued = false;
        string traceDetail = "not_started";
        var traceNames = new List<string>();
        string tracePath = Path.Combine(sessionDir, "trace.nettrace");

        if (invalid.Count == 0)
        {
            var traceLog = new List<string>();
            try
            {
                (bool startedTrace, traceDetail) = await StartTrace(tracePath, traceLog, Write);
                if (!startedTrace) invalid.Add("eventpipe_not_started");
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
                onRendering = (_, __) =>
                {
                    renders++;
                    try { bound?.OnFrame(); }
                    catch (Exception ex) { Write("ONFRAME " + ex.Message); }
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
                    var beforeSample = system.Current(BoxId, "offsetX");
                    double before = beforeSample is MotionSample bs ? bs.Value : double.NaN;
                    double velB = beforeSample is MotionSample bv ? bv.Velocity : double.NaN;
                    bool api = anim.Retarget("offsetX", to, spring);
                    retargets++;
                    var afterSample = system.Current(BoxId, "offsetX");
                    double after = afterSample is MotionSample af ? af.Value : double.NaN;
                    double velA = afterSample is MotionSample av ? av.Velocity : double.NaN;
                    var steps = new List<double>();
                    for (int i = 0; i < 5; i++)
                    {
                        await Task.Delay(16);
                        NoteThread();
                        var stepSample = system.Current(BoxId, "offsetX");
                        steps.Add(stepSample is MotionSample ss ? ss.Value : double.NaN);
                    }
                    Z4Continuity c = Z4SessionCore.Evaluate(before, velB, after, velA, to, steps);
                    if (!api || !c.Ok) jumpFails++;
                    if (!categories.Contains(category)) categories.Add(category);
                    Write("RETARGET tag=" + tag + " cat=" + category + " api=" + api
                        + " before=" + F(before) + " after=" + F(after) + " to=" + F(to, 1)
                        + " jump=" + F(c.Jump) + " dynTol=" + F(c.DynTol)
                        + " maxStep=" + F(c.MaxStep) + " ok=" + c.Ok);
                    return api && c.Ok;
                }

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
                Write("G2 g1=" + g1 + " g2=" + g2 + " g3=" + g3 + " g4=" + g4 + " g5=" + g5
                    + " retargets=" + retargets.ToString(CultureInfo.InvariantCulture)
                    + " jumpFails=" + jumpFails.ToString(CultureInfo.InvariantCulture)
                    + " cross=" + crossHits.ToString(CultureInfo.InvariantCulture));

                bound = null;
                system.Dispose();
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
                g6 = traceReport.Pass;
                if (!traceReport.Pass) Write("G2_6 missing=" + string.Join(",", traceReport.MissingNames));
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
                    ["max_frame_step_dip"] = Z4SessionCore.MaxFrameStepDip,
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
                },
            },
        };
        try { Z4SessionCore.WriteVerdict(Path.Combine(sessionDir, "verdict.json"), verdict); }
        catch (Exception ex) { Write("verdict.json failed: " + ex.Message); }
        Write("Z4_SESSION status=" + status);
        try { File.WriteAllText(logPath, log.ToString(), new UTF8Encoding(false)); } catch { }
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
        var ready = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        void OnLine(string? line)
        {
            if (string.IsNullOrEmpty(line)) return;
            traceLog.Add(line);
            write("TRACE " + line);
            if (line.Contains("Recording", StringComparison.OrdinalIgnoreCase))
                ready.TrySetResult(true);
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
        Task finished = await Task.WhenAny(ready.Task, Task.Delay(20000));
        if (proc.HasExited) return (false, "dotnet-trace exited " + proc.ExitCode.ToString(CultureInfo.InvariantCulture));
        return (true, finished == ready.Task ? "recording" : "started_without_ready_line");
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
