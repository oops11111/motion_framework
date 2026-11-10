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

    string argsLine = Z4SessionCore.TraceArguments(4242, @"C:\s\trace.nettrace");
    Ok(argsLine.Contains("FluidMotion-Motion:0xFFFFFFFFFFFFFFFF:5", StringComparison.Ordinal)
        && argsLine.Contains("--process-id 4242", StringComparison.Ordinal)
        && argsLine.Contains("--duration 00:00:50", StringComparison.Ordinal),
        "eventpipe arguments");

    string dir = Path.Combine(Path.GetTempPath(), "z4-check-" + Guid.NewGuid().ToString("n"));
    Directory.CreateDirectory(dir);
    try
    {
        string good = Path.Combine(dir, "good.nettrace");
        File.WriteAllBytes(good, Fixture(includePresent: true));
        var goodReport = Z4TraceCheck.Check(good);
        Ok(goodReport.Pass, "synthetic nettrace passes");
        string bad = Path.Combine(dir, "bad.nettrace");
        File.WriteAllBytes(bad, Fixture(includePresent: false));
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

static byte[] Fixture(bool includePresent)
{
    var blob = new List<byte>();
    blob.AddRange(Encoding.ASCII.GetBytes("Nettrace"));
    blob.AddRange(new byte[16]);
    blob.AddRange(Encoding.Unicode.GetBytes("FluidMotion-Motion"));
    blob.AddRange(Z4TraceCheck.ProviderGuid.ToByteArray());
    foreach (string name in Z4TraceCheck.RequiredNames)
    {
        if (!includePresent && name == "PresentStats") continue;
        blob.AddRange(Encoding.Unicode.GetBytes(name));
    }
    return blob.ToArray();
}
