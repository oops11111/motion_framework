// WinUI entry for the Z3A-CABI-L1 dev smoke. DEV-ONLY. Not a Z4 session. Not PERF-CI.
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MotionSoftDemo;

public sealed partial class MainPage
{
    private async Task RunZ3aL1DevSmoke()
    {
        string logPath = Path.Combine(DevLog.Directory(), "z3a_l1_dev_smoke.txt");
        string jsonPath = Path.Combine(AppContext.BaseDirectory, "demo_motion.json");
        int code;
        using (var buffer = new StringWriter())
        {
            code = Z3aL1DevSmoke.Run(buffer, jsonPath);
            string text = buffer.ToString();
            File.WriteAllText(logPath, text, new UTF8Encoding(false));
            foreach (string line in text.Split('\n'))
            {
                if (line.Length > 0) Log(line);
            }
        }
        Log($"Z3A_L1_DEV_SMOKE file={logPath} code={code} not_z4=1 perf_ci=HOLD");
        FlushLog();
        try { CloseMainWindowSoon(); } catch { }
        await Task.CompletedTask;
    }
}
