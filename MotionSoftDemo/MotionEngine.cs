// M2-02 L1 thin wrap over MotionBridge (software path). Emits MotionEtw events.
// Retarget uses FmBridge from-current-state (hasFrom=0) — NO shadow IV / NO compositor InitialVelocity.
using System;
using System.Diagnostics;

namespace MotionSoftDemo;

public sealed class MotionEngine : IDisposable
{
    readonly MotionBridge _bridge;
    readonly ulong _sessionId;
    bool _disposed;
    bool _wasRunning;
    int _frameTicks;
    long _lastFrameTs;

    public ulong SessionId => _sessionId;
    public MotionBridge Bridge => _bridge;
    public bool IsRunning => _bridge.Running;
    public bool EmitFrameTickAccept { get; set; }

    public MotionEngine(MotionBridge bridge, ulong sessionId)
    {
        _bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
        _sessionId = sessionId;
        _bridge.SubscriptionChanged += OnSubscriptionChanged;
        _bridge.Frame += OnFrame;
        _lastFrameTs = Stopwatch.GetTimestamp();
    }

    public static ulong NewSessionId()
    {
        // Independent process + fresh engine => unique sessionId (pid << 32 | tick low).
        ulong pid = (uint)Environment.ProcessId;
        ulong tick = (uint)Environment.TickCount64;
        return (pid << 32) | tick;
    }

    void OnSubscriptionChanged(bool on)
    {
        uint active = on ? 1u : 0u;
        MotionEtw.Log.IdleTransition(_sessionId, active, (byte)(on ? 1 : 0));
        _wasRunning = on;
    }

    void OnFrame(TimeSpan rt, double costMs)
    {
        _frameTicks++;
        long now = Stopwatch.GetTimestamp();
        uint deltaUs = (uint)Math.Max(0, (now - _lastFrameTs) * 1_000_000L / Stopwatch.Frequency);
        _lastFrameTs = now;
        uint engineUs = (uint)Math.Max(0, costMs * 1000.0);
        uint active = _bridge.Running ? 1u : 0u;
        // Verbose path (default off unless listener enables Verbose)
        MotionEtw.Log.FrameTick(_sessionId, deltaUs, engineUs, active);
        if (EmitFrameTickAccept && (_frameTicks % 8 == 1))
            MotionEtw.Log.FrameTickAccept(_sessionId, deltaUs, engineUs, active);
    }

    public string? LoadTokens(string json, string name = "demo_motion.json")
    {
        long t0 = Stopwatch.GetTimestamp();
        var err = _bridge.LoadTokens(json);
        uint parseUs = (uint)Math.Max(0, (Stopwatch.GetTimestamp() - t0) * 1_000_000L / Stopwatch.Frequency);
        uint problems = err == null ? 0u : 1u;
        MotionEtw.Log.TokenLoad(_sessionId, name, problems, parseUs);
        return err;
    }

    public MotionAnimation Create(ulong target, string? transition = null)
    {
        ulong id = _bridge.Create(target, transition);
        MotionEtw.Log.AnimationLifecycle(_sessionId, id, target, transition ?? "", "Created");
        return new MotionAnimation(this, id, target, transition);
    }

    public void EmitPresentStats(uint dropped = 0, double intervalMs = 0)
        => MotionEtw.Log.PresentStats(_sessionId, dropped, intervalMs);

    /// PERF-CI r2 Q4: emit measured stats (PresentStatsSampler) instead of placeholders.
    internal void EmitPresentStats(PresentStatsSampler.Result r)
        => MotionEtw.Log.PresentStats(_sessionId, r.Dropped, r.IntervalMs);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { _bridge.SubscriptionChanged -= OnSubscriptionChanged; } catch { }
        try { _bridge.Frame -= OnFrame; } catch { }
        // Bridge lifetime owned by MainPage; do not dispose it here.
    }
}

public sealed class MotionAnimation
{
    readonly MotionEngine _engine;
    public ulong AnimId { get; }
    public ulong Target { get; }
    public string? Transition { get; }
    bool _played;

    internal MotionAnimation(MotionEngine engine, ulong animId, ulong target, string? transition)
    {
        _engine = engine;
        AnimId = animId;
        Target = target;
        Transition = transition;
    }

    public void Play()
    {
        _engine.Bridge.Play(AnimId);
        _played = true;
        MotionEtw.Log.AnimationLifecycle(_engine.SessionId, AnimId, Target, Transition ?? "", "Played");
    }

    public void Cancel()
    {
        _engine.Bridge.Cancel(AnimId);
        MotionEtw.Log.AnimationLifecycle(_engine.SessionId, AnimId, Target, Transition ?? "", "Cancelled");
    }

    /// <summary>
    /// Software-path retarget from current engine state (hasFrom=0). Forbidden: shadow IV / compositor IV seed.
    /// </summary>
    public bool RetargetTo(string property, double target, double response = 0.32, double damping = 0.78, double? from = null)
    {
        double cur = double.NaN, vel = double.NaN;
        _engine.Bridge.Current(Target, property, out cur, out vel);
        bool ok;
        byte hasFrom;
        double fromVal;
        if (from.HasValue)
        {
            hasFrom = 1;
            fromVal = from.Value;
            ok = _engine.Bridge.RetargetFrom(AnimId, property, fromVal, target, response, damping);
        }
        else
        {
            hasFrom = 0;
            fromVal = cur;
            ok = _engine.Bridge.Retarget(AnimId, property, target, response, damping);
        }
        MotionEtw.Log.Retarget(_engine.SessionId, AnimId, property, fromVal, target, response, damping, hasFrom, vel);
        return ok;
    }
}
