using System.Globalization;
using System.Text;

namespace MotionSoftDemo;

/// <summary>
/// Checks an EventPipe nettrace for the frozen E1 provider and the six event names.
/// Names are matched as UTF-16 or UTF-8 metadata strings. This is not a WPA parser.
/// </summary>
public static class Z4TraceCheck
{
    public const string Magic = "Nettrace";
    public const string ProviderName = "FluidMotion-Motion";
    public static readonly Guid ProviderGuid = new("8f3a2c1d-6b4e-4a91-9c7d-2e5f8a1b3c4d");

    public static readonly string[] RequiredNames =
    {
        "AnimationLifecycle",
        "Retarget",
        "FrameTick",
        "PresentStats",
        "IdleTransition",
        "TokenLoad",
    };

    public static Z4TraceReport Check(string path)
    {
        var report = new Z4TraceReport { Path = path };
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            report.Error = "trace file missing";
            return report;
        }
        byte[] data;
        try { data = File.ReadAllBytes(path); }
        catch (Exception ex)
        {
            report.Error = ex.GetType().Name + ": " + ex.Message;
            return report;
        }
        report.Bytes = data.Length;
        if (data.Length < Magic.Length || Encoding.ASCII.GetString(data, 0, Magic.Length) != Magic)
        {
            report.Error = "not a nettrace (missing Nettrace magic)";
            return report;
        }
        report.MagicOk = true;
        report.ProviderNameFound = ContainsText(data, ProviderName);
        report.GuidFound = IndexOf(data, ProviderGuid.ToByteArray()) >= 0;
        foreach (string name in RequiredNames)
            report.Names[name] = ContainsText(data, name);
        report.Pass = report.MagicOk && report.ProviderNameFound && report.GuidFound
            && RequiredNames.All(n => report.Names[n]);
        return report;
    }

    public static string Format(Z4TraceReport report)
    {
        var sb = new StringBuilder();
        sb.Append("trace=").AppendLine(report.Path);
        sb.Append("bytes=").Append(report.Bytes.ToString(CultureInfo.InvariantCulture));
        sb.Append(" magic=").Append(report.MagicOk ? "ok" : "BAD");
        sb.Append(" provider=").Append(report.ProviderNameFound ? "ok" : "MISSING");
        sb.Append(" guid=").Append(report.GuidFound ? "ok" : "MISSING");
        sb.AppendLine();
        foreach (string name in RequiredNames)
        {
            bool found = report.Names.TryGetValue(name, out bool ok) && ok;
            sb.Append(found ? "ok " : "MISSING ").AppendLine(name);
        }
        if (!string.IsNullOrEmpty(report.Error)) sb.Append("error=").AppendLine(report.Error);
        sb.Append("verdict=").AppendLine(report.Pass ? "PASS" : "FAIL");
        return sb.ToString();
    }

    static bool ContainsText(byte[] data, string text) =>
        IndexOf(data, Encoding.Unicode.GetBytes(text)) >= 0
        || IndexOf(data, Encoding.UTF8.GetBytes(text)) >= 0;

    static int IndexOf(byte[] data, byte[] needle)
    {
        if (needle.Length == 0 || data.Length < needle.Length) return -1;
        for (int i = 0; i <= data.Length - needle.Length; i++)
        {
            int j = 0;
            for (; j < needle.Length; j++)
            {
                if (data[i + j] != needle[j]) break;
            }
            if (j == needle.Length) return i;
        }
        return -1;
    }
}

public sealed class Z4TraceReport
{
    public string Path { get; set; } = "";
    public long Bytes { get; set; }
    public bool MagicOk { get; set; }
    public bool ProviderNameFound { get; set; }
    public bool GuidFound { get; set; }
    public Dictionary<string, bool> Names { get; } = new(StringComparer.Ordinal);
    public bool Pass { get; set; }
    public string? Error { get; set; }

    public IReadOnlyList<string> MissingNames =>
        Z4TraceCheck.RequiredNames.Where(n => !Names.TryGetValue(n, out bool ok) || !ok).ToList();
}
