// Z3A-CABI-L1 public surface: MotionSystem / AnimationHandle / MotionTokens.
// Software path only, over the existing 14 fm_* exports (code/capi/fm_capi.h).
// No new native exports. No WinRT Composition backend. No RiveView / PointerFollow.
// Spring arguments are caller-supplied. Nothing here defaults to PACING r4 0.3·T.
// P/Invoke stays internal. Thread rule (contract Table 2): one UI thread; no fm_* from sink/render.
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace MotionSoftDemo;

/// <summary>Undamped period (seconds) and damping ratio. Maps to <c>fm_animation_retarget</c> response/damping.</summary>
public readonly struct SpringParams
{
    public double Response { get; }
    public double Damping { get; }

    public SpringParams(double response, double damping)
    {
        if (!(response > 0)) throw new ArgumentOutOfRangeException(nameof(response));
        if (!(damping >= 0)) throw new ArgumentOutOfRangeException(nameof(damping));
        Response = response;
        Damping = damping;
    }
}

/// <summary>Last <c>Tick</c> sample (<c>fm_current</c>). Null from <see cref="MotionSystem.Current"/> means no value.</summary>
public readonly struct MotionSample
{
    public double Value { get; }
    public double Velocity { get; }

    public MotionSample(double value, double velocity)
    {
        Value = value;
        Velocity = velocity;
    }
}

/// <summary>
/// L1 system (contract Table 1). <see cref="Create"/> → <c>fm_create</c>; <see cref="Dispose"/> → <c>fm_destroy</c>.
/// The host binds a frame source to <see cref="FramePumpChanged"/> and calls <see cref="OnFrame"/> from it.
/// </summary>
public sealed class MotionSystem : IDisposable
{
    readonly FmAbi.SinkFn _sink;
    readonly FmAbi.RenderFn _render;
    readonly int _ownerThread;
    readonly ulong _sessionId;
    IntPtr _sys;
    int _nativeDepth;
    bool _disposeRequested;
    bool _disposed;
    bool _pumpOn;
    int _frameTicks;
    long _lastFrameTs;

    /// <summary>ETW session correlation. Not a Table 1 motion operation.</summary>
    internal ulong SessionId => _sessionId;

    /// <summary>Also emit frozen event id 103 (name remapped to FrameTick) while a listener is attached.</summary>
    internal bool EmitFrameTickAccept { get; set; }

    /// <summary>fm_sink_fn projection. Do not call back into the API from this handler (Table 2).</summary>
    public event Action<ulong, string, double>? ValueChanged;

    /// <summary>
    /// Host frame-source bind (Table 1 OnFrame row). true: start calling <see cref="OnFrame"/>;
    /// false: idle, unsubscribe. Do not call back into the API from this handler (Table 2).
    /// </summary>
    public event Action<bool>? FramePumpChanged;

    static MotionSystem()
    {
        string? path = Environment.GetEnvironmentVariable("FM_FLUIDMOTION_PATH");
        if (string.IsNullOrEmpty(path)) return;
        try
        {
            NativeLibrary.SetDllImportResolver(typeof(MotionSystem).Assembly, (name, _, _) =>
            {
                if (!string.Equals(name, "fluidmotion", StringComparison.Ordinal)) return IntPtr.Zero;
                return NativeLibrary.Load(path);
            });
        }
        catch (InvalidOperationException)
        {
            // Another resolver already owns this assembly; default probing still applies.
        }
    }

    MotionSystem(bool manual)
    {
        _ownerThread = Environment.CurrentManagedThreadId;
        _sessionId = NewSessionId();
        _lastFrameTs = Stopwatch.GetTimestamp();
        _sink = OnSink;
        _render = OnRender;
        _sys = manual
            ? FmAbi.fm_create_manual(_sink, _render, IntPtr.Zero)
            : FmAbi.fm_create(_sink, _render, IntPtr.Zero);
        if (_sys == IntPtr.Zero) throw new InvalidOperationException(manual ? "fm_create_manual failed" : "fm_create failed");
    }

    /// <summary>Table 1 <c>MotionSystem.Create()</c> → <c>fm_create</c> (steady clock).</summary>
    public static MotionSystem Create() => new(manual: false);

    /// <summary>Dev/test clock (<c>fm_create_manual</c>). Not part of the public L1 surface.</summary>
    internal static MotionSystem CreateManual() => new(manual: true);

    internal static ulong NewSessionId()
    {
        ulong pid = (uint)Environment.ProcessId;
        ulong tick = (uint)Environment.TickCount64;
        return (pid << 32) | tick;
    }

    /// <summary>Table 1 <c>MotionSystem.LoadTokens</c> → <c>fm_load_tokens</c>. Null = success; otherwise JSON-Pointer diagnostics.</summary>
    public string? LoadTokens(string json) => LoadTokens(json, "motion.json");

    internal string? LoadTokens(string json, string name)
    {
        Affinity();
        if (json == null) throw new ArgumentNullException(nameof(json));
        if (string.IsNullOrEmpty(name)) name = "motion.json";
        var err = new byte[4096];
        long t0 = Stopwatch.GetTimestamp();
        int rc = Call(s => FmAbi.fm_load_tokens(s, json, err, err.Length), 1);
        uint parseUs = (uint)Math.Max(0, (Stopwatch.GetTimestamp() - t0) * 1_000_000L / Stopwatch.Frequency);
        uint problems = rc <= 0 ? 0u : (uint)rc;
        MotionEtw.Log.TokenLoad(_sessionId, name, problems, parseUs);
        if (rc == 0) return null;
        int n = Array.IndexOf(err, (byte)0);
        if (n < 0) n = err.Length;
        return Encoding.UTF8.GetString(err, 0, n);
    }

    /// <summary>Table 1 <c>MotionSystem.SetReducedMotion(on)</c> → <c>fm_set_reduced_motion</c>.</summary>
    public void SetReducedMotion(bool on)
    {
        Affinity();
        Call(s => FmAbi.fm_set_reduced_motion(s, on ? 1 : 0));
    }

    /// <summary>Table 1 <c>MotionSystem.IsRunning</c> → <c>fm_is_running</c>.</summary>
    public bool IsRunning
    {
        get
        {
            Affinity();
            return Call(s => FmAbi.fm_is_running(s), 0) != 0;
        }
    }

    /// <summary>Table 1 <c>MotionSystem.OnFrame</c> → <c>fm_on_frame</c>. Call from the host frame source, never from a sink.</summary>
    public void OnFrame()
    {
        Affinity();
        if (_nativeDepth != 0)
            throw new InvalidOperationException("MotionSystem.OnFrame must not be called from a sink or render callback.");
        if (_disposed || _sys == IntPtr.Zero) return;
        long t0 = Stopwatch.GetTimestamp();
        _nativeDepth++;
        try { FmAbi.fm_on_frame(_sys); }
        finally { ExitNative(); }
        if (_disposed) return;
        _frameTicks++;
        long now = Stopwatch.GetTimestamp();
        uint deltaUs = (uint)Math.Max(0, (now - _lastFrameTs) * 1_000_000L / Stopwatch.Frequency);
        _lastFrameTs = now;
        double costMs = (now - t0) * 1000.0 / Stopwatch.Frequency;
        uint engineUs = (uint)Math.Max(0, costMs * 1000.0);
        uint active = IsRunning ? 1u : 0u;
        MotionEtw.Log.FrameTick(_sessionId, deltaUs, engineUs, active);
        if (EmitFrameTickAccept && (_frameTicks % 8 == 1))
            MotionEtw.Log.FrameTickAccept(_sessionId, deltaUs, engineUs, active);
    }

    /// <summary>Dev/test only (<c>fm_manual_advance</c>). No effect on a <see cref="Create"/> system.</summary>
    internal void Advance(double seconds)
    {
        Affinity();
        if (!(seconds > 0)) return;
        Call(s => FmAbi.fm_manual_advance(s, seconds));
    }

    /// <summary>Table 1 <c>MotionSystem.Current(target, property)</c> → <c>fm_current</c> (last Tick sample, else rest).</summary>
    public MotionSample? Current(ulong target, string property)
    {
        Affinity();
        if (property == null) throw new ArgumentNullException(nameof(property));
        double value = 0, velocity = 0;
        bool ok = Call(s => FmAbi.fm_current(s, target, property, out value, out velocity), 0) != 0;
        return ok ? new MotionSample(value, velocity) : null;
    }

    /// <summary>Table 1 <c>MotionSystem.CreateAnimation(target, transition?)</c> → <c>fm_animation_create</c>.</summary>
    public AnimationHandle CreateAnimation(ulong target, string? transition = null)
    {
        Affinity();
        ulong id = Call(s => FmAbi.fm_animation_create(s, target, transition), 0UL);
        if (id != 0)
            MotionEtw.Log.AnimationLifecycle(_sessionId, id, target, transition ?? "", "Created");
        return new AnimationHandle(this, id, target, transition);
    }

    /// <summary>Table 1 <c>MotionSystem.Dispose()</c> → <c>fm_destroy</c>. Deferred if called from a sink/render callback.</summary>
    public void Dispose()
    {
        Affinity();
        if (_disposed || _disposeRequested) return;
        _disposeRequested = true;
        if (_nativeDepth > 0) return;
        DestroyCore();
    }

    internal bool PlayRaw(ulong anim)
    {
        if (anim == 0) return false;
        Call(s => FmAbi.fm_animation_play(s, anim));
        return true;
    }

    internal bool CancelRaw(ulong anim)
    {
        if (anim == 0) return false;
        Call(s => FmAbi.fm_animation_cancel(s, anim));
        return true;
    }

    internal bool ReleaseRaw(ulong anim)
    {
        if (anim == 0) return false;
        Call(s => FmAbi.fm_animation_release(s, anim));
        return true;
    }

    internal bool RetargetRaw(ulong anim, string property, double target, SpringParams spring, int hasFrom, double from)
    {
        if (anim == 0) return false;
        return Call(s => FmAbi.fm_animation_retarget(s, anim, property, target, spring.Response, spring.Damping, hasFrom, from), 0) != 0;
    }

    void OnSink(ulong target, IntPtr property, double value, IntPtr user)
    {
        try
        {
            if (_disposed) return;
            string name = Marshal.PtrToStringUTF8(property) ?? "";
            ValueChanged?.Invoke(target, name, value);
        }
        catch
        {
            // Never let a managed exception cross the ABI.
        }
    }

    void OnRender(int on, IntPtr user)
    {
        try
        {
            _pumpOn = on != 0;
            FramePumpChanged?.Invoke(_pumpOn);
            if (!_disposed)
                MotionEtw.Log.IdleTransition(_sessionId, _pumpOn ? 1u : 0u, (byte)(_pumpOn ? 1 : 0));
        }
        catch
        {
        }
    }

    void Affinity()
    {
        if (Environment.CurrentManagedThreadId != _ownerThread)
            throw new InvalidOperationException("MotionSystem is bound to the thread that created it (Table 2).");
    }

    T Call<T>(Func<IntPtr, T> f, T fallback)
    {
        Affinity();
        if (_nativeDepth != 0)
            throw new InvalidOperationException("fm_* must not be called from a sink or render callback.");
        if (_disposed || _sys == IntPtr.Zero) return fallback;
        _nativeDepth++;
        try { return f(_sys); }
        finally { ExitNative(); }
    }

    void Call(Action<IntPtr> f)
    {
        Affinity();
        if (_nativeDepth != 0)
            throw new InvalidOperationException("fm_* must not be called from a sink or render callback.");
        if (_disposed || _sys == IntPtr.Zero) return;
        _nativeDepth++;
        try { f(_sys); }
        finally { ExitNative(); }
    }

    void ExitNative()
    {
        _nativeDepth--;
        if (_nativeDepth == 0 && _disposeRequested && !_disposed) DestroyCore();
    }

    void DestroyCore()
    {
        if (_disposed) return;
        _disposed = true;
        _disposeRequested = true;
        var pump = FramePumpChanged;
        FramePumpChanged = null;
        ValueChanged = null;
        if (_pumpOn)
        {
            _pumpOn = false;
            try { pump?.Invoke(false); } catch { }
        }
        if (_sys != IntPtr.Zero)
        {
            IntPtr sys = _sys;
            _sys = IntPtr.Zero;
            try { FmAbi.fm_destroy(sys); } catch { }
        }
    }

    internal static class FmAbi
    {
        const string Dll = "fluidmotion";

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void SinkFn(ulong target, IntPtr property, double value, IntPtr user);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void RenderFn(int on, IntPtr user);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr fm_create(SinkFn sink, RenderFn render, IntPtr user);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr fm_create_manual(SinkFn sink, RenderFn render, IntPtr user);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void fm_manual_advance(IntPtr sys, double seconds);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void fm_destroy(IntPtr sys);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int fm_load_tokens(IntPtr sys, [MarshalAs(UnmanagedType.LPUTF8Str)] string json, [Out] byte[] err, int errCap);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void fm_set_reduced_motion(IntPtr sys, int on);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern ulong fm_animation_create(IntPtr sys, ulong target, [MarshalAs(UnmanagedType.LPUTF8Str)] string? transition);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void fm_animation_play(IntPtr sys, ulong anim);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void fm_animation_cancel(IntPtr sys, ulong anim);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void fm_animation_release(IntPtr sys, ulong anim);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int fm_animation_retarget(IntPtr sys, ulong anim, [MarshalAs(UnmanagedType.LPUTF8Str)] string property, double target, double response, double damping, int hasFrom, double from);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void fm_on_frame(IntPtr sys);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int fm_is_running(IntPtr sys);

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int fm_current(IntPtr sys, ulong target, [MarshalAs(UnmanagedType.LPUTF8Str)] string property, out double value, out double velocity);
    }
}

/// <summary>L1 handle (contract Table 1): Play → Retarget → Cancel, plus Release.</summary>
public sealed class AnimationHandle
{
    readonly MotionSystem _system;
    bool _released;

    internal AnimationHandle(MotionSystem system, ulong id, ulong target, string? transition)
    {
        _system = system;
        Id = id;
        Target = target;
        Transition = transition;
    }

    public ulong Id { get; }
    internal ulong Target { get; }
    internal string? Transition { get; }

    /// <summary>Table 1 <c>AnimationHandle.Play()</c> → <c>fm_animation_play</c>.</summary>
    public void Play()
    {
        if (_released || Id == 0) return;
        if (!_system.PlayRaw(Id)) return;
        MotionEtw.Log.AnimationLifecycle(_system.SessionId, Id, Target, Transition ?? "", "Played");
    }

    /// <summary>Table 1 <c>AnimationHandle.Cancel()</c> → <c>fm_animation_cancel</c>.</summary>
    public void Cancel()
    {
        if (_released || Id == 0) return;
        if (!_system.CancelRaw(Id)) return;
        MotionEtw.Log.AnimationLifecycle(_system.SessionId, Id, Target, Transition ?? "", "Cancelled");
    }

    /// <summary>Table 1 <c>AnimationHandle.Release()</c> → <c>fm_animation_release</c>. Drops this handle; motion may finish.</summary>
    public void Release()
    {
        if (_released || Id == 0) return;
        _released = true;
        if (!_system.ReleaseRaw(Id)) return;
        MotionEtw.Log.AnimationLifecycle(_system.SessionId, Id, Target, Transition ?? "", "Released");
    }

    /// <summary>
    /// Table 1 <c>AnimationHandle.Retarget(property, target, spring, from?)</c> → <c>fm_animation_retarget</c>.
    /// <paramref name="from"/> null keeps the current value and velocity (hasFrom = 0). No shadow initial velocity.
    /// </summary>
    public bool Retarget(string property, double target, SpringParams spring, double? from = null)
    {
        if (property == null) throw new ArgumentNullException(nameof(property));
        if (_released || Id == 0) return false;
        MotionSample? sample = _system.Current(Target, property);
        int hasFrom;
        double fromValue;
        if (from.HasValue)
        {
            hasFrom = 1;
            fromValue = from.Value;
        }
        else
        {
            hasFrom = 0;
            fromValue = 0;
        }
        bool ok = _system.RetargetRaw(Id, property, target, spring, hasFrom, fromValue);
        double reportedFrom = from ?? sample?.Value ?? double.NaN;
        double velocity = sample?.Velocity ?? double.NaN;
        MotionEtw.Log.Retarget(_system.SessionId, Id, property, reportedFrom, target, spring.Response, spring.Damping, (byte)hasFrom, velocity);
        return ok;
    }
}

/// <summary>L1 token load (contract Table 1). <see cref="Load"/> → <c>fm_load_tokens</c> on the bound system.</summary>
public sealed class MotionTokens
{
    readonly MotionSystem _system;

    public MotionTokens(MotionSystem system)
    {
        _system = system ?? throw new ArgumentNullException(nameof(system));
    }

    /// <summary>Table 1 <c>MotionTokens.Load(json)</c>. Null = success; otherwise JSON-Pointer diagnostics, one per line.</summary>
    public string? Load(string json) => _system.LoadTokens(json);
}
