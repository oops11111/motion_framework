using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using MotionSoftDemo;

int checks = 0;
void Check(bool condition, string label)
{
    if (!condition) throw new InvalidOperationException(label);
    checks++;
    Console.WriteLine("PASS " + label);
}

string native = Path.Combine(AppContext.BaseDirectory, "fluidmotion.dll");
Check(File.Exists(native), "native asset copied from package");
string tokens = Path.Combine(AppContext.BaseDirectory, "demo_motion.json");
Check(File.Exists(tokens), "sample tokens copied from package");
var system = MotionSystem.Create();
int sinkCalls = 0;
double lastHeight = double.NaN;
var pump = new List<bool>();
system.ValueChanged += (target, property, value) =>
{
    if (target == 41 && property == "height")
    {
        sinkCalls++;
        lastHeight = value;
    }
};
system.FramePumpChanged += on => pump.Add(on);
Check(new MotionTokens(system).Load(File.ReadAllText(tokens)) is null, "actual token loading");
var handle = system.CreateAnimation(41);
Check(handle.Id != 0, "native animation created");
handle.Play();
Check(handle.Retarget("height", 64, new SpringParams(0.03, 1), from: 0), "public retarget accepted");
void Drain()
{
    var watchdog = Stopwatch.StartNew();
    while (system.IsRunning && watchdog.Elapsed < TimeSpan.FromSeconds(5))
    {
        system.OnFrame();
        Thread.Sleep(1);
    }
    Check(!system.IsRunning, "frame pump reaches idle (watchdog only)");
}
Drain();
Check(system.Current(41, "height") is { } sample && Math.Abs(sample.Value - 64) < 0.01,
    "actual native target reached");
Check(sinkCalls > 0 && Math.Abs(lastHeight - 64) < 0.01, "sink received final target");
Check(handle.Retarget("height", 0, new SpringParams(0.03, 1)), "from-null rest retarget");
Drain();
Check(system.Current(41, "height") is { } rest && Math.Abs(rest.Value) < 0.01,
    "actual native rest reached");
handle.Cancel();
handle.Release();
handle.Release();
Check(!handle.Retarget("height", 20, new SpringParams(0.03, 1)), "released handle rejects reuse");
system.Dispose();
system.Dispose();
Check(!system.IsRunning && pump.Contains(true) && pump.Last() == false,
    "actual release/dispose and pump teardown");
using var process = Process.GetCurrentProcess();
var module = process.Modules.Cast<ProcessModule>().Single(m =>
    string.Equals(m.ModuleName, "fluidmotion.dll", StringComparison.OrdinalIgnoreCase));
Check(Path.GetFullPath(module.FileName).Equals(Path.GetFullPath(native), StringComparison.OrdinalIgnoreCase),
    "loaded module is isolated consumer native output");
Check(Path.GetFullPath(typeof(MotionSystem).Assembly.Location).Equals(
    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "FluidMotion.L1.dll")), StringComparison.OrdinalIgnoreCase),
    "managed assembly loaded from isolated package consumer");
Console.WriteLine(JsonSerializer.Serialize(new
{
    checks,
    native_sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(module.FileName))).ToLowerInvariant(),
    managed_sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(MotionSystem).Assembly.Location))).ToLowerInvariant(),
    token_sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(tokens))).ToLowerInvariant(),
    package_only = true,
    performance_claim = false,
    event_payloads_read = false
}));
