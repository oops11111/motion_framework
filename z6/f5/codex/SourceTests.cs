using System.Collections.Concurrent;
using FluentHub.Views;

static class SourceTests
{
    private static int checks;
    private static void Check(bool value, string name)
    {
        if (!value) throw new Exception(name);
        Console.WriteLine("PASS " + name);
        checks++;
    }

    public static void Main()
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException("Windows named-event tests");
        var real = new ReducedMotionState(true, false);
        var seen = new List<(bool, string)>();
        Action<bool, string> old = (enabled, source) => seen.Add((enabled, source));
        real.Changed += old;
        real.Reread(true, "wm");
        Check(seen.Count == 0, "unchanged WM reread does not notify");
        real.Reread(false, "wm");
        real.Reread(false, "event");
        Check(seen.SequenceEqual(new[] { (false, "wm") }), "changed WM and duplicate event deduplicate");
        real.ToggleFake();
        Check(!real.AnimationsEnabled, "real source ignores fake trigger");
        real.Changed -= old;
        int newSubscriber = 0;
        real.Changed += (_, _) => newSubscriber++;
        real.Reread(true, "activated");
        Check(seen.Count == 1 && newSubscriber == 1, "replacement subscriber does not retain old subscriber");
        real.Dispose();
        real.Reread(false, "wm");
        Check(newSubscriber == 1 && real.AnimationsEnabled, "disposed state suppresses late callbacks");

        var fake = new ReducedMotionState(true, true);
        int uiThread = Environment.CurrentManagedThreadId;
        var changes = new List<bool>();
        fake.Changed += (enabled, source) =>
        {
            Check(Environment.CurrentManagedThreadId == uiThread && source == "fake", "fake notification on queued UI thread");
            changes.Add(enabled);
        };
        fake.Reread(false, "wm");
        Check(fake.AnimationsEnabled && changes.Count == 0, "OS reread cannot overwrite fake mode");

        var queue = new ConcurrentQueue<Action>();
        using var queued = new AutoResetEvent(false);
        using var rejected = new AutoResetEvent(false);
        using var source = new FakeReducedMotionSource(action =>
        {
            queue.Enqueue(action);
            queued.Set();
            return true;
        }, fake, _ => { });
        using var signal = EventWaitHandle.OpenExisting(@"Local\FM_PILOT_F5_TOGGLE");
        void Receive()
        {
            signal.Set();
            Check(queued.WaitOne(TimeSpan.FromSeconds(10)), "named event reaches actual FakeReducedMotionSource");
            Check(queue.TryDequeue(out var action), "fake callback enqueued");
            Check(changes.Count == (fake.AnimationsEnabled ? 0 : 1), "wait thread has not applied state");
            action!();
        }
        Receive();
        fake.Reread(true, "activated");
        Check(!fake.AnimationsEnabled, "activation cannot reset fake state");
        Receive();
        Check(changes.SequenceEqual(new[] { false, true }), "fake flip and restore");
        signal.Set();
        Check(queued.WaitOne(TimeSpan.FromSeconds(10)), "queued callback prepared before disposal");
        Check(queue.TryDequeue(out var late), "late fake callback captured");
        source.Dispose();
        source.Dispose();
        late!();
        Check(changes.Count == 2, "queued fake callback is harmless after source disposal");
        fake.Dispose();

        using var rejectedSource = new FakeReducedMotionSource(_ => false, new ReducedMotionState(true, true), line =>
        {
            if (line.Contains("ok=False")) rejected.Set();
        });
        signal.Set();
        Check(rejected.WaitOne(TimeSpan.FromSeconds(10)), "enqueue rejection is observable");
        Console.WriteLine($"PASS (dev-only) checks={checks}; no OS setting writes; no GUI acceptance claimed");
    }
}
