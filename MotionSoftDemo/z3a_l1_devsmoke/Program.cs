// DEV-ONLY. Not Z4. Not PERF-CI. Success is not adopt.
using System.Text;
using MotionSoftDemo;

string json = args.Length > 0
    ? args[0]
    : Path.Combine(AppContext.BaseDirectory, "demo_motion.json");
using var stdout = new StreamWriter(Console.OpenStandardOutput(), new UTF8Encoding(false)) { AutoFlush = true };
int code = Z3aL1DevSmoke.Run(stdout, json);
return code;
