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
    // Name plus the frozen GUID. EventPipe enables either identity. The attribute GUID is what EventSource.Guid returns.
    public const string ProviderSpec =
        "FluidMotion-Motion:0xFFFFFFFFFFFFFFFF:5,8f3a2c1d-6b4e-4a91-9c7d-2e5f8a1b3c4d:0xFFFFFFFFFFFFFFFF:5";
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

    public static Z4BuildInfo ReadBuildInfo(string baseDir)
    {
        string tip = "";
        string composition = "";
        string rawHash = "";
        string path = Path.Combine(baseDir ?? "", "z4_build_info.txt");
        if (File.Exists(path))
        {
            foreach (string raw in File.ReadAllLines(path))
            {
                int eq = raw.IndexOf('=');
                if (eq <= 0) continue;
                string key = raw[..eq].Trim();
                string value = raw[(eq + 1)..].Trim().Trim('\r');
                if (key == "tip") tip = value;
                else if (key == "composition_sha256") composition = value;
                else if (key == "composition_sha256_raw") rawHash = value;
            }
        }
        return new Z4BuildInfo(tip, composition, rawHash);
    }

    /// <summary>
    /// SHA-256 of the file bytes, and of those bytes after CR/CRLF are turned into LF.
    /// Git autocrlf changes the working-tree bytes and not the blob. PASS compares the normalized digest.
    /// The git tip is a commit id from <c>git rev-parse</c>, not a file hash, so line endings do not apply to it.
    /// </summary>
    public static Z4TextHash HashTextFile(string path)
    {
        byte[] raw = File.ReadAllBytes(path);
        return HashTextBytes(raw);
    }

    public static Z4TextHash HashTextBytes(byte[] raw)
    {
        raw ??= Array.Empty<byte>();
        return new Z4TextHash(Sha256(raw), Sha256(NormalizeNewlines(raw)));
    }

    public static byte[] NormalizeNewlines(byte[] raw)
    {
        if (raw == null || raw.Length == 0) return Array.Empty<byte>();
        var lf = new byte[raw.Length];
        int n = 0;
        for (int i = 0; i < raw.Length; i++)
        {
            byte b = raw[i];
            if (b == (byte)'\r')
            {
                lf[n++] = (byte)'\n';
                if (i + 1 < raw.Length && raw[i + 1] == (byte)'\n') i++;
            }
            else lf[n++] = b;
        }
        if (n == lf.Length) return lf;
        var trimmed = new byte[n];
        Buffer.BlockCopy(lf, 0, trimmed, 0, n);
        return trimmed;
    }

    /// <summary>Last two path segments, so a packaged LocalState folder keeps machine/session.</summary>
    public static string SessionLeaf(string requestedPath)
    {
        string trimmed = (requestedPath ?? "").Trim().TrimEnd('\\', '/');
        if (trimmed.Length == 0) return "session";
        int slash = Math.Max(trimmed.LastIndexOf('\\'), trimmed.LastIndexOf('/'));
        string leaf = slash >= 0 ? trimmed[(slash + 1)..] : trimmed;
        if (leaf.Length == 0) leaf = "session";
        string rest = slash >= 0 ? trimmed[..slash] : "";
        int slash2 = Math.Max(rest.LastIndexOf('\\'), rest.LastIndexOf('/'));
        string parent = slash2 >= 0 ? rest[(slash2 + 1)..] : rest;
        if (parent.Length == 0) return leaf;
        return parent + Path.DirectorySeparatorChar + leaf;
    }

    /// <summary>
    /// Unpackaged: the requested directory. Packaged: ApplicationData LocalState, which is a real
    /// user-writable path (not the virtualized view of the repo).
    /// </summary>
    public static string ResolveSessionDirectory(string requestedPath, bool packaged, string? localState)
    {
        if (!packaged || string.IsNullOrWhiteSpace(localState))
            return requestedPath ?? "";
        return Path.Combine(localState, "z4", "results_etw_l1_smoke_r1", SessionLeaf(requestedPath ?? ""));
    }

    /// <summary>
    /// Where a packaged write under the user profile is stored on disk:
    /// %LOCALAPPDATA%\Packages\&lt;PackageFamilyName&gt;\LocalCache\Local\&lt;rest of profile path&gt;.
    /// </summary>
    public static string? PredictVirtualizedMirror(string localAppData, string userProfile, string packageFamilyName, string requestedPath)
    {
        if (string.IsNullOrWhiteSpace(localAppData) || string.IsNullOrWhiteSpace(userProfile) || string.IsNullOrWhiteSpace(packageFamilyName))
            return null;
        string profile = userProfile.Trim().TrimEnd('\\', '/');
        string req = (requestedPath ?? "").Trim();
        if (req.Length < profile.Length || !req.StartsWith(profile, StringComparison.OrdinalIgnoreCase)) return null;
        if (req.Length > profile.Length && req[profile.Length] != '\\' && req[profile.Length] != '/') return null;
        string rest = req.Length > profile.Length ? req[(profile.Length + 1)..] : "";
        char sep = localAppData.Contains('\\') ? '\\' : '/';
        string root = localAppData.Trim().TrimEnd('\\', '/')
            + sep + "Packages" + sep + packageFamilyName + sep + "LocalCache" + sep + "Local";
        if (rest.Length == 0) return root;
        return root + sep + rest.Replace('\\', sep).Replace('/', sep);
    }

    public static void CopyDirectoryFiles(string source, string dest)
    {
        Directory.CreateDirectory(dest);
        foreach (string file in Directory.GetFiles(source))
        {
            string target = Path.Combine(dest, Path.GetFileName(file));
            if (string.Equals(Path.GetFullPath(file), Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase)) continue;
            File.Copy(file, target, overwrite: true);
        }
    }

    static string Sha256(byte[] data)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(data)).ToLowerInvariant();
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

    public static string Sha256File(string path) => HashTextFile(path).Raw;

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

public readonly record struct Z4BuildInfo(string Tip, string CompositionSha, string CompositionShaRaw);

public readonly record struct Z4TextHash(string Raw, string Normalized);

public readonly record struct Z4Continuity(
    bool Ok,
    bool JumpOk,
    bool VelOk,
    bool NoTeleport,
    bool SeqOk,
    double Jump,
    double DynTol,
    double MaxStep);
