using System.Diagnostics.Tracing;
using System.Text;
using MotionSoftDemo;

if (args.Length == 1 && args[0] == "--self-test")
    return SelfTest();

if (args.Length != 1)
{
    Console.Error.WriteLine("usage: z4_check_trace <session.nettrace>");
    Console.Error.WriteLine("       z4_check_trace --self-test");
    return 2;
}

Z4TraceReport report = Z4TraceCheck.Check(args[0]);
Console.Write(Z4TraceCheck.Format(report));
return report.Pass ? 0 : 1;

static int SelfTest()
{
    int fails = 0;
    void Ok(bool cond, string msg)
    {
        Console.WriteLine((cond ? "ok " : "FAIL ") + msg);
        if (!cond) fails++;
    }

    string? missing = Z4SessionCore.OutDirFromArgs(new[] { "app" });
    Ok(missing == null, "no flag means no session");
    Ok(Z4SessionCore.OutDirFromArgs(new[] { "app", "--z4-l1-session", @"C:\out\s1" }) == @"C:\out\s1", "flag takes the next argument");
    Ok(Z4SessionCore.OutDirFromArgs(new[] { "app", "--z4-l1-session" }) == "", "flag without a directory is empty");
    Ok(Z4SessionCore.TryEnterSession(out string? why1) && why1 == null, "first session entry");
    Ok(!Z4SessionCore.TryEnterSession(out string? why2) && why2 == "session_run_in_process_twice", "second entry is INVALID");

    var pass = Z4SessionCore.Evaluate(10, 20, 10.4, 19, 80, new[] { 12.0, 14.0, 16.0 });
    Ok(pass.Ok && pass.Jump <= pass.DynTol, "small handoff passes dynTol");
    var teleport = Z4SessionCore.Evaluate(10, 5, 80, 0, 80, new[] { 80.0 });
    Ok(!teleport.Ok && !teleport.NoTeleport, "snap to target is a jump fail");
    var bigStep = Z4SessionCore.Evaluate(10, 1, 10.2, 1, 40, new[] { 90.2 });
    Ok(!bigStep.Ok && !bigStep.SeqOk && bigStep.MaxStep >= 80, "step of 80 DIP fails");
    var under = Z4SessionCore.Evaluate(0, 0, 0, 0, 10, new[] { 79.9 });
    Ok(under.SeqOk && under.MaxStep < 80, "step under 80 DIP passes");
    const double u1Vel = 17.467030932093856;
    Ok(Math.Abs(Z4SessionCore.DynTol(u1Vel) - 2.0) < 1e-9 && Math.Abs(Z4SessionCore.DynTol(0) - 2.0) < 1e-9,
        "vel 17.467 and vel 0 both floor dynTol at 2");
    double p0 = SpringPos(0);
    double p33 = SpringPos(0.033);
    double p66 = SpringPos(0.066);
    var frames = new Z4FrameSteps();
    frames.Observe(1, 33, Math.Abs(p33 - p0));
    frames.Observe(2, 33, Math.Abs(p66 - p33));
    Ok(frames.SeqOk && frames.MaxStep > 50 && frames.MaxStep < 60 && frames.Observed == 2,
        "consecutive 33 ms OnFrame steps pass the locked 80");
    var limit = new Z4FrameSteps();
    limit.Observe(1, 33, 79.9);
    limit.Observe(2, 33, 80.0);
    Ok(limit.FailCount == 1 && !limit.SeqOk && limit.MaxStep == 80 && limit.MaxDtMs == 33,
        "an OnFrame step of 80 fails and keeps its dt");
    var many = new Z4FrameSteps();
    for (int i = 1; i <= 12; i++) many.Observe(i, 31, 10);
    many.Observe(13, 31, 90);
    Ok(many.Observed == 13 && many.FailCount == 1 && many.MaxStep == 90 && many.MaxIndex == 13,
        "frame steps past the 8-line log cap still count");
    var collapsed = Z4SessionCore.Evaluate(p0, u1Vel, p0, u1Vel, 40, new[] { p66 });
    Ok(!collapsed.SeqOk && collapsed.MaxStep >= 80 && collapsed.MaxStep < 90,
        "a 66 ms poll sum is diagnostic and is not the ② step");

    string argsLine = Z4SessionCore.TraceArguments(4242, @"C:\s\trace.nettrace");
    Ok(argsLine.Contains("FluidMotion-Motion:0xFFFFFFFFFFFFFFFF:5", StringComparison.Ordinal)
        && argsLine.Contains("8f3a2c1d-6b4e-4a91-9c7d-2e5f8a1b3c4d:0xFFFFFFFFFFFFFFFF:5", StringComparison.Ordinal)
        && argsLine.Contains("--process-id 4242", StringComparison.Ordinal)
        && argsLine.Contains("--duration 00:00:50", StringComparison.Ordinal),
        "eventpipe arguments enable name and frozen GUID");

    string dir = Path.Combine(Path.GetTempPath(), "z4-check-" + Guid.NewGuid().ToString("n"));
    Directory.CreateDirectory(dir);
    try
    {
        string good = Path.Combine(dir, "good.nettrace");
        File.WriteAllBytes(good, V5Fixture(includePresent: true));
        var goodReport = Z4TraceCheck.Check(good);
        Ok(goodReport.Pass && goodReport.GuidStatus == Z4TraceCheck.GuidAbsent, "metadata without a GUID field is not guid=MISSING");
        Guid nameHash = Z4TraceCheck.NameHashGuid(Z4TraceCheck.ProviderName);
        string withGuid = Path.Combine(dir, "guid.nettrace");
        File.WriteAllBytes(withGuid, V6Fixture(includePresent: true, providerGuid: Z4TraceCheck.FrozenGuid, decoy: nameHash));
        var guidReport = Z4TraceCheck.Check(withGuid);
        Ok(guidReport.Pass && guidReport.GuidStatus == Z4TraceCheck.GuidMatchFrozen
            && guidReport.GuidInFile == Z4TraceCheck.FrozenGuid.ToString(),
            "ProviderGuid in V6 metadata matches frozen, ignoring a decoy name-hash");
        string mismatch = Path.Combine(dir, "mismatch.nettrace");
        File.WriteAllBytes(mismatch, V6Fixture(includePresent: true, providerGuid: nameHash, decoy: Z4TraceCheck.FrozenGuid));
        var mismatchReport = Z4TraceCheck.Check(mismatch);
        Ok(!mismatchReport.Pass && mismatchReport.GuidStatus == Z4TraceCheck.GuidMismatchFrozen
            && mismatchReport.GuidInFile == nameHash.ToString(),
            "name-hash ProviderGuid in metadata is guid_mismatch_frozen");
        string bad = Path.Combine(dir, "bad.nettrace");
        File.WriteAllBytes(bad, V5Fixture(includePresent: false));
        var badReport = Z4TraceCheck.Check(bad);
        Ok(!badReport.Pass && badReport.MissingNames.Contains("PresentStats"), "missing PresentStats fails");
        string text = Path.Combine(dir, "dev.txt");
        File.WriteAllText(text, "AnimationLifecycle Retarget FrameTick PresentStats IdleTransition TokenLoad");
        Ok(!Z4TraceCheck.Check(text).Pass, "a dev log is not a nettrace");

        string? header = Z4SessionCore.FindCompositionHeader(AppContext.BaseDirectory)
            ?? Z4SessionCore.FindCompositionHeader(Directory.GetCurrentDirectory());
        Ok(header != null, "found composition.h");
        if (header != null)
        {
            Z4TextHash lf = Z4SessionCore.HashTextFile(header);
            Ok(lf.Normalized == Z4SessionCore.ExpectedCompositionSha, "LF normalized hash is a55d4952");
            Ok(lf.Raw == Z4SessionCore.ExpectedCompositionSha, "git blob is LF so raw matches");
            byte[] crlf = ToCrlf(File.ReadAllBytes(header));
            string crlfPath = Path.Combine(dir, "composition.h");
            File.WriteAllBytes(crlfPath, crlf);
            Z4TextHash cr = Z4SessionCore.HashTextFile(crlfPath);
            Ok(cr.Normalized == Z4SessionCore.ExpectedCompositionSha, "CRLF normalized hash is a55d4952");
            Ok(cr.Raw == "33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97", "CRLF raw hash is the autocrlf working-tree digest");
        }

        string leaf = Z4SessionCore.SessionLeaf(@"C:\repo\z4\results_etw_l1_smoke_r1\K93N3P8\s3-packaged");
        Ok(leaf.Contains("K93N3P8") && leaf.Contains("s3-packaged"), "session leaf keeps machine and session");
        string resolved = Z4SessionCore.ResolveSessionDirectory(
            @"C:\repo\z4\results_etw_l1_smoke_r1\K93N3P8\s3-packaged",
            packaged: true,
            @"C:\Users\admin\AppData\Local\Packages\MotionSoftDemo_abc\LocalState");
        Ok(resolved.Contains("LocalState") && resolved.Contains("K93N3P8") && resolved.Contains("s3-packaged"),
            "packaged session dir is under LocalState");
        string? virt = Z4SessionCore.PredictVirtualizedMirror(
            @"C:\Users\admin\AppData\Local",
            @"C:\Users\admin",
            "MotionSoftDemo_abc",
            @"C:\Users\admin\motion_framework\z4\results_etw_l1_smoke_r1\K93N3P8\s3-packaged");
        Ok(virt != null && virt.Contains(@"Packages\MotionSoftDemo_abc\LocalCache\Local\motion_framework"),
            "virtualized mirror guess");
        Ok(Z4TraceCheck.NameHashGuid("FluidMotion-Motion") == new Guid("367c0a5e-9a10-5a09-16fb-80d0a4f845d8"),
            "name-hash GUID");
        Ok(Z4GuidProbe.Log.Guid == Z4TraceCheck.FrozenGuid
            && EventSource.GetGuid(typeof(Z4GuidProbe)) == Z4TraceCheck.FrozenGuid,
            "EventSource.GetGuid returns the Guid attribute");
        Ok(Z4NameProbe.Log.Guid == new Guid("367c0a5e-9a10-5a09-16fb-80d0a4f845d8"),
            "name-only EventSource.Guid is the name hash");
    }
    finally
    {
        try { Directory.Delete(dir, true); } catch { }
    }

    Console.WriteLine(fails == 0 ? "SELF_TEST PASS" : "SELF_TEST FAIL " + fails.ToString());
    return fails == 0 ? 0 : 1;
}

static byte[] ToCrlf(byte[] lf)
{
    var list = new List<byte>(lf.Length + 64);
    foreach (byte b in lf)
    {
        if (b == (byte)'\n') list.Add((byte)'\r');
        list.Add(b);
    }
    return list.ToArray();
}

static double SpringPos(double t)
{
    const double response = 0.32, z = 0.78, target = 40.0, from = 244.71180310234942, v0 = 17.467030932093856;
    double w0 = 2.0 * Math.PI / response;
    double a = z * w0;
    double wd = w0 * Math.Sqrt(1.0 - z * z);
    double x0 = from - target;
    double b = (v0 + a * x0) / wd;
    double e = Math.Exp(-a * t);
    return target + e * (x0 * Math.Cos(wd * t) + b * Math.Sin(wd * t));
}

static byte[] V5Fixture(bool includePresent)
{
    var blob = new List<byte>();
    blob.AddRange(Encoding.ASCII.GetBytes("Nettrace"));
    blob.AddRange(new byte[16]);
    int meta = 1;
    int id = 1;
    foreach (string name in Z4TraceCheck.RequiredNames)
    {
        if (!includePresent && name == "PresentStats") continue;
        blob.AddRange(BitConverter.GetBytes(meta++));
        blob.AddRange(Encoding.Unicode.GetBytes(Z4TraceCheck.ProviderName));
        blob.Add(0);
        blob.Add(0);
        blob.AddRange(BitConverter.GetBytes(id++));
        blob.AddRange(Encoding.Unicode.GetBytes(name));
        blob.Add(0);
        blob.Add(0);
    }
    return blob.ToArray();
}

static byte[] V6Fixture(bool includePresent, Guid providerGuid, Guid decoy)
{
    var blob = new List<byte>();
    blob.AddRange(Encoding.ASCII.GetBytes("Nettrace"));
    blob.AddRange(new byte[16]);
    blob.AddRange(decoy.ToByteArray());
    int id = 1;
    foreach (string name in Z4TraceCheck.RequiredNames)
    {
        if (!includePresent && name == "PresentStats") continue;
        var row = new List<byte> { (byte)id };
        row.Add((byte)Z4TraceCheck.ProviderName.Length);
        row.AddRange(Encoding.UTF8.GetBytes(Z4TraceCheck.ProviderName));
        row.Add((byte)id);
        row.Add((byte)name.Length);
        row.AddRange(Encoding.UTF8.GetBytes(name));
        row.Add(0);
        row.Add(0);
        row.Add(17);
        row.Add(0);
        row.Add(7);
        row.AddRange(providerGuid.ToByteArray());
        blob.AddRange(BitConverter.GetBytes((ushort)row.Count));
        blob.AddRange(row);
        id++;
    }
    return blob.ToArray();
}

[EventSource(Name = "FluidMotion-Motion", Guid = "8f3a2c1d-6b4e-4a91-9c7d-2e5f8a1b3c4d")]
sealed class Z4GuidProbe : EventSource
{
    public static readonly Z4GuidProbe Log = new();
}

[EventSource(Name = "FluidMotion-Motion")]
sealed class Z4NameProbe : EventSource
{
    public static readonly Z4NameProbe Log = new();
}
