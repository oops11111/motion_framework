// M2-02: FluidMotion ETW provider (minimal set of 6 event names).
// GUID / event ids / payload layout are implementation-chosen (gate does not freeze them).
// FrameTick is Verbose (default off); enable at accept time via EventListener / MF_M2_02.
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;

namespace MotionSoftDemo;

[EventSource(Name = "FluidMotion-Motion", Guid = "8f3a2c1d-6b4e-4a91-9c7d-2e5f8a1b3c4d")]
public sealed class MotionEtw : EventSource
{
    public static readonly MotionEtw Log = new();

    // Stable provider GUID (docs/artifacts will record it). Not frozen by gate.
    public static readonly Guid ProviderGuid = new("8f3a2c1d-6b4e-4a91-9c7d-2e5f8a1b3c4d");

    MotionEtw() { }

    public const int IdAnimationLifecycle = 1;
    public const int IdRetarget = 2;
    public const int IdFrameTick = 3;
    public const int IdPresentStats = 4;
    public const int IdIdleTransition = 5;
    public const int IdTokenLoad = 6;

    [Event(IdAnimationLifecycle, Level = EventLevel.Informational, Opcode = EventOpcode.Info,
        Message = "AnimationLifecycle session={0} anim={1} target={2} transition={3} state={4}")]
    public void AnimationLifecycle(ulong sessionId, ulong animId, ulong target, string transition, string state)
    {
        if (IsEnabled()) WriteEvent(IdAnimationLifecycle, sessionId, animId, target, transition ?? "", state ?? "");
    }

    [Event(IdRetarget, Level = EventLevel.Informational,
        Message = "Retarget session={0} anim={1} prop={2} from={3} to={4} response={5} damping={6} hasFrom={7} vel={8}")]
    public void Retarget(ulong sessionId, ulong animId, string property, double from, double to,
        double response, double damping, byte hasFrom, double velocityAtHandoff)
    {
        if (IsEnabled()) WriteEvent(IdRetarget, sessionId, animId, property ?? "", from, to, response, damping, hasFrom, velocityAtHandoff);
    }

    [Event(IdFrameTick, Level = EventLevel.Verbose,
        Message = "FrameTick session={0} deltaUs={1} engineTickUs={2} activeCount={3}")]
    public void FrameTick(ulong sessionId, uint deltaUs, uint engineTickUs, uint activeCount)
    {
        if (IsEnabled(EventLevel.Verbose, EventKeywords.All))
            WriteEvent(IdFrameTick, sessionId, deltaUs, engineTickUs, activeCount);
    }

    /// <summary>Force-emit FrameTick at Information during M2-02 accept (Verbose may be filtered by default tools).</summary>
    [Event(103, Level = EventLevel.Informational,
        Message = "FrameTick session={0} deltaUs={1} engineTickUs={2} activeCount={3}")]
    public void FrameTickAccept(ulong sessionId, uint deltaUs, uint engineTickUs, uint activeCount)
    {
        // Event name for WPA/ledger is remapped to "FrameTick" by MotionEtwMirror.
        if (IsEnabled()) WriteEvent(103, sessionId, deltaUs, engineTickUs, activeCount);
    }

    [Event(IdPresentStats, Level = EventLevel.Informational,
        Message = "PresentStats session={0} dropped={1} intervalMs={2}")]
    public void PresentStats(ulong sessionId, uint dropped, double intervalMs)
    {
        if (IsEnabled()) WriteEvent(IdPresentStats, sessionId, dropped, intervalMs);
    }

    [Event(IdIdleTransition, Level = EventLevel.Informational,
        Message = "IdleTransition session={0} activeCount={1} pumpOn={2}")]
    public void IdleTransition(ulong sessionId, uint activeCount, byte pumpOn)
    {
        if (IsEnabled()) WriteEvent(IdIdleTransition, sessionId, activeCount, pumpOn);
    }

    [Event(IdTokenLoad, Level = EventLevel.Informational,
        Message = "TokenLoad session={0} name={1} problemCount={2} parseUs={3}")]
    public void TokenLoad(ulong sessionId, string name, uint problemCount, uint parseUs)
    {
        if (IsEnabled()) WriteEvent(IdTokenLoad, sessionId, name ?? "", problemCount, parseUs);
    }
}

/// <summary>
/// In-process EventListener used by M2-02 smoke to (a) enable Verbose FrameTick and
/// (b) mirror every minimal-set event name into a text ledger for provider proof / WPA cross-check.
/// NOTE: EventListener base ctor invokes OnEventSourceCreated before derived ctor body runs —
/// so _sink may be null; we queue matching sources and Enable after assignment.
/// </summary>
public sealed class MotionEtwMirror : EventListener
{
    Action<string>? _sink;
    bool _enableVerbose;
    readonly List<EventSource> _pending = new();
    bool _ready;

    public MotionEtwMirror(Action<string> sink, bool enableVerbose = true)
    {
        _enableVerbose = enableVerbose;
        _sink = sink ?? throw new ArgumentNullException(nameof(sink));
        _ready = true;
        // Enable any sources observed during base ctor.
        foreach (var es in _pending.ToArray())
            TryEnable(es);
        _pending.Clear();
        // Also explicitly enable our provider if already constructed.
        TryEnable(MotionEtw.Log);
    }

    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource == null) return;
        if (eventSource.Name != "FluidMotion-Motion" && eventSource.Guid != MotionEtw.ProviderGuid) return;
        if (!_ready || _sink == null)
        {
            _pending.Add(eventSource);
            return;
        }
        TryEnable(eventSource);
    }

    void TryEnable(EventSource eventSource)
    {
        try
        {
            var level = _enableVerbose ? EventLevel.Verbose : EventLevel.Informational;
            EnableEvents(eventSource, level, EventKeywords.All);
            _sink?.Invoke($"ETW_LISTENER enabled provider={eventSource.Name} guid={eventSource.Guid} level={level}");
        }
        catch (Exception ex)
        {
            _sink?.Invoke($"ETW_LISTENER enable FAILED {ex.GetType().Name}: {ex.Message}");
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs e)
    {
        if (_sink == null) return;
        if (e.EventSource?.Name != "FluidMotion-Motion") return;
        string name = e.EventName ?? ("Event" + e.EventId);
        // Normalize accept-force FrameTick (id 103) and method name "FrameTickAccept" to frozen event name.
        if (e.EventId == 103 || string.Equals(name, "FrameTickAccept", StringComparison.Ordinal))
            name = "FrameTick";
        string payload = e.Payload == null ? "" : string.Join(",", e.Payload.Select(p => p?.ToString() ?? ""));
        _sink($"ETW_EVENT name={name} id={e.EventId} level={e.Level} payload={payload}");
    }
}
