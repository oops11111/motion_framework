using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace MotionSoftDemo;

/// <summary>
/// Reads EventPipe metadata the way a nettrace actually stores it:
/// uint32 metadata id, UTF-16Z provider name, uint32 event id, UTF-16Z event name.
/// That record has no provider-GUID field. A GUID is reported only when its bytes are in the file.
/// </summary>
public static class Z4TraceCheck
{
    public const string Magic = "Nettrace";
    public const string ProviderName = "FluidMotion-Motion";
    public static readonly Guid FrozenGuid = new("8f3a2c1d-6b4e-4a91-9c7d-2e5f8a1b3c4d");
    public static readonly Guid ProviderGuid = FrozenGuid;

    public const string GuidMatchFrozen = "match_frozen";
    public const string GuidMismatchFrozen = "guid_mismatch_frozen";
    public const string GuidAbsent = "absent_from_metadata";

    public static readonly string[] RequiredNames =
    {
        "AnimationLifecycle",
        "Retarget",
        "FrameTick",
        "PresentStats",
        "IdleTransition",
        "TokenLoad",
    };

    /// <summary>
    /// GUID EventSource.GetGuid returns when the type has a name and no Guid attribute.
    /// MotionEtw sets the Guid attribute, so this is not the value EventSource.Guid emits.
    /// </summary>
    public static Guid NameHashGuid(string name)
    {
        byte[] ns =
        {
            0x48, 0x2C, 0x2D, 0xB2, 0xC3, 0x90, 0x47, 0xC8,
            0x87, 0xF8, 0x1A, 0x15, 0xBF, 0xC1, 0x30, 0xFB,
        };
        byte[] nameBytes = Encoding.BigEndianUnicode.GetBytes((name ?? "").ToUpperInvariant());
        byte[] source = new byte[ns.Length + nameBytes.Length];
        Buffer.BlockCopy(ns, 0, source, 0, ns.Length);
        Buffer.BlockCopy(nameBytes, 0, source, ns.Length, nameBytes.Length);
        byte[] hash = SHA1.HashData(source);
        hash[7] = (byte)((hash[7] & 0x0F) | 0x50);
        var guidBytes = new byte[16];
        Buffer.BlockCopy(hash, 0, guidBytes, 0, 16);
        return new Guid(guidBytes);
    }

    public static Z4TraceReport Check(string path)
    {
        var report = new Z4TraceReport { Path = path ?? "" };
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            report.Error = "trace file missing";
            report.GuidStatus = GuidAbsent;
            return report;
        }
        byte[] data;
        try { data = File.ReadAllBytes(path); }
        catch (Exception ex)
        {
            report.Error = ex.GetType().Name + ": " + ex.Message;
            report.GuidStatus = GuidAbsent;
            return report;
        }
        return CheckBytes(data, path);
    }

    public static Z4TraceReport CheckBytes(byte[] data, string path = "")
    {
        var report = new Z4TraceReport { Path = path, Bytes = data?.LongLength ?? 0 };
        if (data == null || data.Length < Magic.Length || Encoding.ASCII.GetString(data, 0, Magic.Length) != Magic)
        {
            report.Error = "not a nettrace (missing Nettrace magic)";
            report.GuidStatus = GuidAbsent;
            return report;
        }
        report.MagicOk = true;
        var metadataGuids = new List<Guid>();
        ParseMetadata(data, report);
        ParseV6Metadata(data, report, metadataGuids);
        report.ProviderNameFound = report.Events.Any(e => e.Provider == ProviderName);
        foreach (string name in RequiredNames)
            report.Names[name] = report.Events.Any(e => e.Provider == ProviderName && e.EventName == name);
        ClassifyGuid(data, report, metadataGuids);
        report.Pass = report.MagicOk && report.ProviderNameFound
            && RequiredNames.All(n => report.Names[n])
            && report.GuidStatus != GuidMismatchFrozen;
        return report;
    }

    public static string Format(Z4TraceReport report)
    {
        var sb = new StringBuilder();
        sb.Append("trace=").AppendLine(report.Path);
        sb.Append("bytes=").Append(report.Bytes.ToString(CultureInfo.InvariantCulture));
        sb.Append(" magic=").Append(report.MagicOk ? "ok" : "BAD");
        sb.Append(" provider=").Append(report.ProviderNameFound ? "ok" : "MISSING");
        sb.Append(" guid=").Append(report.GuidStatus);
        if (!string.IsNullOrEmpty(report.GuidInFile)) sb.Append(" file=").Append(report.GuidInFile);
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

    static void ParseMetadata(byte[] data, Z4TraceReport report)
    {
        byte[] marker = Encoding.Unicode.GetBytes(ProviderName);
        int from = 0;
        while (from <= data.Length - marker.Length - 2)
        {
            int at = IndexOf(data, marker, from);
            if (at < 0) break;
            from = at + 2;
            if (at + marker.Length + 1 >= data.Length) break;
            if (data[at + marker.Length] != 0 || data[at + marker.Length + 1] != 0) continue;
            int cursor = at + marker.Length + 2;
            if (cursor + 4 > data.Length) continue;
            int eventId = BitConverter.ToInt32(data, cursor);
            cursor += 4;
            if (!TryReadUtf16Z(data, ref cursor, out string eventName)) continue;
            if (eventId < 0 || eventId > 0xFFFF || eventName.Length == 0) continue;
            report.Events.Add(new Z4TraceEvent(ProviderName, eventId, eventName));
        }
    }

    /// <summary>
    /// NetTrace V6 metadata row: length-prefixed UTF-8 names, then optional metadata.
    /// Kind 7 is the provider GUID (16 bytes, .NET Guid layout). The runtime's older
    /// FastSerialization metadata (parsed above) has no GUID field.
    /// </summary>
    static void ParseV6Metadata(byte[] data, Z4TraceReport report, List<Guid> guids)
    {
        byte[] name = Encoding.UTF8.GetBytes(ProviderName);
        if (name.Length == 0 || name.Length > 127) return;
        for (int i = 1; i <= data.Length - name.Length; i++)
        {
            if (data[i - 1] != name.Length) continue;
            if (IndexOf(data, name, i) != i) continue;
            int cursor = i + name.Length;
            if (!TryReadVarUInt(data, ref cursor, out uint eventId) || eventId > 0xFFFF) continue;
            if (!TryReadUtf8String(data, ref cursor, data.Length, out string eventName)) continue;
            if (cursor + 2 > data.Length) continue;
            int fieldCount = BitConverter.ToUInt16(data, cursor);
            cursor += 2;
            if (fieldCount > 64) continue;
            bool ok = true;
            for (int f = 0; f < fieldCount && ok; f++)
            {
                if (cursor + 2 > data.Length) { ok = false; break; }
                int fieldSize = BitConverter.ToUInt16(data, cursor);
                cursor += 2;
                if (fieldSize > 4096 || cursor + fieldSize > data.Length) { ok = false; break; }
                cursor += fieldSize;
            }
            if (!ok || cursor + 2 > data.Length) continue;
            int optSize = BitConverter.ToUInt16(data, cursor);
            cursor += 2;
            if (optSize > 4096 || cursor + optSize > data.Length) continue;
            int optEnd = cursor + optSize;
            Guid? rowGuid = null;
            while (cursor < optEnd && ok)
            {
                byte kind = data[cursor++];
                switch (kind)
                {
                    case 1:
                    case 8:
                    case 9:
                        if (cursor >= optEnd) ok = false;
                        else cursor++;
                        break;
                    case 3:
                        if (cursor + 8 > optEnd) ok = false;
                        else cursor += 8;
                        break;
                    case 4:
                    case 5:
                        if (!TryReadUtf8String(data, ref cursor, optEnd, out _)) ok = false;
                        break;
                    case 6:
                        if (!TryReadUtf8String(data, ref cursor, optEnd, out _)
                            || !TryReadUtf8String(data, ref cursor, optEnd, out _)) ok = false;
                        break;
                    case 7:
                        if (cursor + 16 > optEnd) ok = false;
                        else
                        {
                            var raw = new byte[16];
                            Buffer.BlockCopy(data, cursor, raw, 0, 16);
                            rowGuid = new Guid(raw);
                            cursor += 16;
                        }
                        break;
                    default:
                        ok = false;
                        break;
                }
            }
            if (!ok || cursor != optEnd) continue;
            report.Events.Add(new Z4TraceEvent(ProviderName, (int)eventId, eventName));
            if (rowGuid.HasValue) guids.Add(rowGuid.Value);
        }
    }

    static void ClassifyGuid(byte[] data, Z4TraceReport report, List<Guid> fromMetadata)
    {
        if (fromMetadata.Count > 0)
        {
            bool allFrozen = fromMetadata.TrueForAll(g => g == FrozenGuid);
            report.GuidFound = true;
            report.GuidInFile = (allFrozen ? FrozenGuid : fromMetadata.Find(g => g != FrozenGuid)).ToString();
            report.GuidStatus = allFrozen ? GuidMatchFrozen : GuidMismatchFrozen;
            return;
        }
        Guid nameHash = NameHashGuid(ProviderName);
        bool frozen = IndexOf(data, FrozenGuid.ToByteArray(), 0) >= 0
            || IndexOf(data, GuidBigEndian(FrozenGuid), 0) >= 0;
        bool hashed = IndexOf(data, nameHash.ToByteArray(), 0) >= 0
            || IndexOf(data, GuidBigEndian(nameHash), 0) >= 0;
        if (frozen)
        {
            report.GuidFound = true;
            report.GuidInFile = FrozenGuid.ToString();
            report.GuidStatus = GuidMatchFrozen;
            return;
        }
        if (hashed)
        {
            report.GuidFound = true;
            report.GuidInFile = nameHash.ToString();
            report.GuidStatus = GuidMismatchFrozen;
            return;
        }
        report.GuidFound = false;
        report.GuidInFile = "";
        report.GuidStatus = GuidAbsent;
    }

    static byte[] GuidBigEndian(Guid guid)
    {
        byte[] mixed = guid.ToByteArray();
        return new byte[]
        {
            mixed[3], mixed[2], mixed[1], mixed[0],
            mixed[5], mixed[4],
            mixed[7], mixed[6],
            mixed[8], mixed[9], mixed[10], mixed[11], mixed[12], mixed[13], mixed[14], mixed[15],
        };
    }

    static bool TryReadVarUInt(byte[] data, ref int cursor, out uint value)
    {
        value = 0;
        int shift = 0;
        for (int i = 0; i < 5; i++)
        {
            if (cursor >= data.Length) return false;
            byte b = data[cursor++];
            value |= (uint)(b & 0x7F) << shift;
            if ((b & 0x80) == 0) return true;
            shift += 7;
        }
        return false;
    }

    static bool TryReadUtf8String(byte[] data, ref int cursor, int limit, out string text)
    {
        text = "";
        if (!TryReadVarUInt(data, ref cursor, out uint len)) return false;
        if (len == 0 || len > 200 || cursor + len > data.Length || cursor + len > limit) return false;
        text = Encoding.UTF8.GetString(data, cursor, (int)len);
        cursor += (int)len;
        return text.All(c => c >= 32 && c < 127);
    }

    static bool TryReadUtf16Z(byte[] data, ref int cursor, out string text)
    {
        text = "";
        int start = cursor;
        var chars = new StringBuilder();
        while (cursor + 1 < data.Length)
        {
            if (data[cursor] == 0 && data[cursor + 1] == 0)
            {
                cursor += 2;
                text = chars.ToString();
                return cursor > start + 2 && text.All(c => c >= 32 && c < 127);
            }
            if (data[cursor + 1] != 0) return false;
            chars.Append((char)data[cursor]);
            cursor += 2;
            if (chars.Length > 200) return false;
        }
        return false;
    }

    static int IndexOf(byte[] data, byte[] needle, int start)
    {
        if (needle.Length == 0 || data.Length < needle.Length) return -1;
        for (int i = Math.Max(0, start); i <= data.Length - needle.Length; i++)
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

public sealed class Z4TraceEvent
{
    public Z4TraceEvent(string provider, int eventId, string eventName)
    {
        Provider = provider;
        EventId = eventId;
        EventName = eventName;
    }

    public string Provider { get; }
    public int EventId { get; }
    public string EventName { get; }
}

public sealed class Z4TraceReport
{
    public string Path { get; set; } = "";
    public long Bytes { get; set; }
    public bool MagicOk { get; set; }
    public bool ProviderNameFound { get; set; }
    public bool GuidFound { get; set; }
    public string GuidStatus { get; set; } = Z4TraceCheck.GuidAbsent;
    public string GuidInFile { get; set; } = "";
    public List<Z4TraceEvent> Events { get; } = new();
    public Dictionary<string, bool> Names { get; } = new(StringComparer.Ordinal);
    public bool Pass { get; set; }
    public string? Error { get; set; }

    public IReadOnlyList<string> MissingNames =>
        Z4TraceCheck.RequiredNames.Where(n => !Names.TryGetValue(n, out bool ok) || !ok).ToList();
}
