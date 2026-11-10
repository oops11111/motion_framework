using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MotionSoftDemo;

/// <summary>
/// Z4 r1 session bookkeeping. Not a Z3a dev smoke and not a collection by itself.
/// Criteria numbers follow docs/z4_etw_l1_smoke_gate_r1_LOCKED.md G2.
/// </summary>
public static class Z4SessionCore
{
    public const string Flag = "--z4-l1-session";
    public const string EnvVar = "MF_Z4_L1_SESSION";
    public const string ExpectedTipEnv = "MF_Z4_EXPECTED_TIP";
    public const string ProviderSpec = "FluidMotion-Motion:0xFFFFFFFFFFFFFFFF:5";
    public const string DefaultTraceDuration = "00:00:50";
    public const string ExpectedCompositionSha =
        "a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418";
    public const string GateBodySha =
        "acb89d1b0cb975e5d63ae2c6fc47adcbc74dddd959b63089b2546b692cadab14";

    // M2-02 dynTol, which the locked gate says to keep. Step limit is the gate's strict < 80 DIP.
    public const double BaseJumpDip = 2.0;
    public const double FrameSlackSec = 0.040;
    public const double MaxFrameStepDip = 80.0;
    public const double VelZeroFloor = 8.0;

    static int _entries;

    public static string? OutDirFromArgs(string[] args)
    {
        if (args == null) return null;
        for (int i = 0; i < args.Length; i++)
        {
            if (!string.Equals(args[i], Flag, StringComparison.OrdinalIgnoreCase)) continue;
            if (i + 1 < args.Length && args[i + 1].Length > 0 && args[i + 1][0] != '-')
                return args[i + 1];
            return "";
        }
        string? env = Environment.GetEnvironmentVariable(EnvVar);
        return string.IsNullOrWhiteSpace(env) ? null : env.Trim();
    }

    /// <summary>One process, one session. A second entry is INVALID.</summary>
    public static bool TryEnterSession(out string? invalidReason)
    {
        if (Interlocked.Increment(ref _entries) != 1)
        {
            invalidReason = "session_run_in_process_twice";
            return false;
        }
        invalidReason = null;
        return true;
    }

    public static double DynTol(double velocityBefore) =>
        Math.Max(BaseJumpDip, Math.Abs(velocityBefore) * FrameSlackSec + 1.0);

    public static Z4Continuity Evaluate(double before, double velocityBefore, double after, double velocityAfter, double target, IReadOnlyList<double> stepsAfter)
    {
        double jump = Math.Abs(after - before);
        double dyn = DynTol(velocityBefore);
        bool jumpOk = !double.IsNaN(before) && !double.IsNaN(after) && jump <= dyn;
        bool velOk = !(Math.Abs(velocityBefore) > VelZeroFloor && Math.Abs(velocityAfter) < 0.5);
        bool noTeleport = !(Math.Abs(before - target) > 10.0 && Math.Abs(after - target) < 1.0);
        double maxStep = 0;
        bool seqOk = true;
        double prev = after;
        if (stepsAfter != null)
        {
            foreach (double sx in stepsAfter)
            {
                double step = Math.Abs(sx - prev);
                if (step > maxStep) maxStep = step;
                if (!(step < MaxFrameStepDip)) seqOk = false;
                prev = sx;
            }
        }
        return new Z4Continuity(jumpOk && velOk && noTeleport && seqOk, jumpOk, velOk, noTeleport, seqOk, jump, dyn, maxStep);
    }

    public static string TraceArguments(int processId, string outputPath, string? duration = null)
    {
        string span = string.IsNullOrWhiteSpace(duration) ? DefaultTraceDuration : duration.Trim();
        return "collect --process-id " + processId.ToString(CultureInfo.InvariantCulture)
            + " --providers " + ProviderSpec
            + " --duration " + span
            + " --output \"" + outputPath + "\"";
    }

    public static string? FindDotnetTrace()
    {
        string? fromEnv = Environment.GetEnvironmentVariable("DOTNET_TRACE");
        if (!string.IsNullOrWhiteSpace(fromEnv) && File.Exists(fromEnv)) return fromEnv;
        string[] names = OperatingSystem.IsWindows()
            ? new[] { "dotnet-trace.exe", "dotnet-trace.cmd", "dotnet-trace" }
            : new[] { "dotnet-trace" };
        string? path = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(path))
        {
            foreach (string dir in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                foreach (string name in names)
                {
                    string candidate = Path.Combine(dir, name);
                    if (File.Exists(candidate)) return candidate;
                }
            }
        }
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        foreach (string name in names)
        {
            string candidate = Path.Combine(home, ".dotnet", "tools", name);
            if (File.Exists(candidate)) return candidate;
        }
        return null;
    }

    public static (string tip, string compositionSha) ReadBuildInfo(string baseDir)
    {
        string tip = "";
        string composition = "";
        string path = Path.Combine(baseDir ?? "", "z4_build_info.txt");
        if (File.Exists(path))
        {
            foreach (string raw in File.ReadAllLines(path))
            {
                int eq = raw.IndexOf('=');
                if (eq <= 0) continue;
                string key = raw[..eq].Trim();
                string value = raw[(eq + 1)..].Trim();
                if (key == "tip") tip = value;
                else if (key == "composition_sha256") composition = value;
            }
        }
        return (tip, composition);
    }

    public static string? FindCompositionHeader(string startDir)
    {
        var dir = new DirectoryInfo(string.IsNullOrWhiteSpace(startDir) ? Directory.GetCurrentDirectory() : startDir);
        for (int i = 0; i < 8 && dir != null; i++, dir = dir.Parent)
        {
            string candidate = Path.Combine(dir.FullName, "code", "composition.h");
            if (File.Exists(candidate)) return candidate;
        }
        return null;
    }

    public static string Sha256File(string path)
    {
        using var sha = SHA256.Create();
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(sha.ComputeHash(stream)).ToLowerInvariant();
    }

    public static string WriteEnv(string path, IReadOnlyDictionary<string, object?> values)
    {
        string json = JsonSerializer.Serialize(values, JsonOptions());
        File.WriteAllText(path, json + Environment.NewLine, new UTF8Encoding(false));
        return json;
    }

    public static string WriteVerdict(string path, IReadOnlyDictionary<string, object?> values)
    {
        string json = JsonSerializer.Serialize(values, JsonOptions());
        File.WriteAllText(path, json + Environment.NewLine, new UTF8Encoding(false));
        return json;
    }

    static JsonSerializerOptions JsonOptions() => new()
    {
        WriteIndented = true,
    };
}

public readonly record struct Z4Continuity(
    bool Ok,
    bool JumpOk,
    bool VelOk,
    bool NoTeleport,
    bool SeqOk,
    double Jump,
    double DynTol,
    double MaxStep);
