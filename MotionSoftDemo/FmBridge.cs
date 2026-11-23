// FmBridge.cs: P/Invoke over code/capi/fm_capi.h (fluidmotion.dll), following
// code/capi/csharp_pinvoke_sketch.cs. Frame source = CompositionTarget.Rendering (vsync-paced).
//
// LEGACY (MF-04): this bridge is demo-only and frozen. The canonical L1 surface is
// MotionSystem.cs (FluidMotion.L1); new code must use MotionSystem. Since the MF-04
// migration (2026-11-23) no in-repo code calls MotionBridge: MainPage.xaml.cs and
// M202Probe.cs run on MotionSystem. MotionBridge stays only as a frozen legacy surface
// for out-of-tree demo copies. Do not add features here; parity fixes only.
// 派题 #8 hardening (review docs/claude_m1b_reply.md items 1,3,6,7,8,9):
//  - every callback (_sink, _render, _onRendering) catches all exceptions: nothing crosses P/Invoke
//  - _inNative counter around every fm_* call; Dispose() inside a native call is deferred via
//    DispatcherQueue.TryEnqueue so fm_destroy never overlaps fm_on_frame
//  - Debug.Assert(_queue.HasThreadAccess) on every public member (engine is not thread-safe)
//  - Dispose clears Frame / SubscriptionChanged / Diagnostic
//  - err marshalled as [Out] byte[]; property dispatch by UTF-8 byte compare (no per-frame string)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;

namespace MotionSoftDemo;

internal static class Fm
{
    const string Dll = "fluidmotion";
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void SinkFn(ulong target, IntPtr property, double value, IntPtr user);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void RenderFn(int on, IntPtr user);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr fm_create(SinkFn s, RenderFn r, IntPtr user);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr fm_create_manual(SinkFn s, RenderFn r, IntPtr user);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void fm_manual_advance(IntPtr sys, double seconds);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void fm_destroy(IntPtr sys);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int fm_load_tokens(IntPtr sys, [MarshalAs(UnmanagedType.LPUTF8Str)] string json, [Out] byte[] err, int cap);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void fm_set_reduced_motion(IntPtr sys, int on);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern ulong fm_animation_create(IntPtr sys, ulong target, [MarshalAs(UnmanagedType.LPUTF8Str)] string? transition);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void fm_animation_play(IntPtr sys, ulong anim);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void fm_animation_cancel(IntPtr sys, ulong anim);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void fm_animation_release(IntPtr sys, ulong anim);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int fm_animation_retarget(IntPtr sys, ulong anim, [MarshalAs(UnmanagedType.LPUTF8Str)] string prop, double target, double response, double damping, int hasFrom, double from);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void fm_on_frame(IntPtr sys);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int fm_is_running(IntPtr sys);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int fm_current(IntPtr sys, ulong target, [MarshalAs(UnmanagedType.LPUTF8Str)] string prop, out double value, out double velocity);
}

/// <summary>Property ids dispatched from the native sink without allocating a string.
/// Values are append-only: do not renumber. Other means the property is
/// not one of the mapped names below; callers must not treat it as a specific property.</summary>
public enum FmProp { Other = 0, OffsetX = 1, Opacity = 2, OffsetY = 3, Scale = 4, ScaleX = 5, ScaleY = 6, Rotation = 7 }

/// <summary>Legacy demo-only bridge over <c>fm_capi.h</c>. Use <c>MotionSystem</c> (FluidMotion.L1) for new code.</summary>
[Obsolete("Legacy demo bridge. Use MotionSystem (FluidMotion.L1) for new code; MotionBridge is frozen except for parity fixes.")]
public sealed class MotionBridge : IDisposable
{
    readonly Fm.SinkFn _sink;        // keep delegates alive for the native side's lifetime
    readonly Fm.RenderFn _render;
    readonly EventHandler<object> _onRendering;
    readonly DispatcherQueue _queue;
    readonly Action<ulong, FmProp, double> _onValue;
    IntPtr _sys;
    bool _subscribed, _disposed, _disposePending;
    int _inNative;
    readonly HashSet<ulong> _live = new();  // handles created here and not yet released

    // Demo-chosen retarget spring (matches the demo's feel, not a framework default).
    // Production code should pass an explicit SpringParams via MotionSystem.Retarget.
    public const double DefaultRetargetResponse = 0.32;
    public const double DefaultRetargetDamping = 0.78;

    public event Action<TimeSpan, double>? Frame;   // after each fm_on_frame: (RenderingTime, fm_on_frame cost ms)
    public event Action<bool>? SubscriptionChanged;
    public event Action<string>? Diagnostic;        // callback exceptions, deferred dispose, ...
    public int SubscribeCount { get; private set; }
    public int UnsubscribeCount { get; private set; }
    public int CallbackErrors { get; private set; }
    public string FaultsInjected => _inject ? $"sink={_faultSink} render={_faultRender} rendering={_faultRendering}" : "off";
    public int DeferredDisposes { get; private set; }
    public Exception? LastError { get; private set; }
    public bool IsDisposed => _disposed;
    public bool InNative => _inNative > 0;

    // 派题 #8 follow-up: debug fault injection (MF_INJECT_FAULTS=1 / --inject-faults). One throw per callback, once each,
    // AFTER the callback's real work, so the engine state is unaffected and we only test the catch path.
    readonly bool _inject;
    bool _faultSink, _faultRender, _faultRendering;
    int _sinkCalls, _frames;
    public sealed class InjectedFaultException(string where) : Exception($"injected fault in {where}");

    public MotionBridge(Action<ulong, FmProp, double> onValue, bool injectFaults = false)
    {
        _inject = injectFaults;
        _queue = DispatcherQueue.GetForCurrentThread() ?? throw new InvalidOperationException("MotionBridge must be created on a UI (DispatcherQueue) thread");
        _onValue = onValue;
        _onRendering = (_, a) =>
        {
            try
            {
                if (_sys == IntPtr.Zero || _disposed) return;
                var rt = (a as RenderingEventArgs)?.RenderingTime ?? TimeSpan.Zero;
                long t0 = Stopwatch.GetTimestamp();
                _inNative++;
                try { Fm.fm_on_frame(_sys); } finally { ExitNative(); }
                double costMs = (Stopwatch.GetTimestamp() - t0) * 1000.0 / Stopwatch.Frequency;
                if (!_disposed) Frame?.Invoke(rt, costMs);
                if (_inject && !_faultRendering && ++_frames == 10) { _faultRendering = true; throw new InjectedFaultException("_onRendering (frame 10)"); }
            }
            catch (Exception ex) { Fault("rendering", ex); }
        };
        _sink = (t, p, v, u) =>   // runs inside fm_on_frame: never call Fm.* in here, never throw
        {
            try
            {
                if (!_disposed) _onValue(t, Classify(p), v);
                if (_inject && !_faultSink && ++_sinkCalls == 5) { _faultSink = true; throw new InjectedFaultException("_sink (call 5)"); }
            }
            catch (Exception ex) { Fault("sink", ex); }
        };
        _render = (on, u) =>      // runs inside fm_* calls: never throw
        {
            try
            {
                SetSubscribed(on != 0);
                if (_inject && !_faultRender && on != 0) { _faultRender = true; throw new InjectedFaultException("_render (first on=1)"); }
            }
            catch (Exception ex) { Fault("render", ex); }
        };
        _sys = Fm.fm_create(_sink, _render, IntPtr.Zero);
        if (_sys == IntPtr.Zero) throw new InvalidOperationException("fm_create failed");
    }

    // UTF-8 byte comparison against u8 literals: no allocation per frame/property.
    static unsafe FmProp Classify(IntPtr p)
    {
        if (p == IntPtr.Zero) return FmProp.Other;
        var name = MemoryMarshal.CreateReadOnlySpanFromNullTerminated((byte*)p);
        if (name.SequenceEqual("offsetX"u8)) return FmProp.OffsetX;
        if (name.SequenceEqual("offsetY"u8)) return FmProp.OffsetY;
        if (name.SequenceEqual("opacity"u8)) return FmProp.Opacity;
        if (name.SequenceEqual("scale"u8)) return FmProp.Scale;
        if (name.SequenceEqual("scaleX"u8)) return FmProp.ScaleX;
        if (name.SequenceEqual("scaleY"u8)) return FmProp.ScaleY;
        if (name.SequenceEqual("rotation"u8)) return FmProp.Rotation;
        return FmProp.Other;
    }

    void Fault(string where, Exception ex)
    {
        CallbackErrors++; LastError = ex;
        try { Diagnostic?.Invoke($"callback exception in {where}: {ex.GetType().Name}: {ex.Message}"); } catch { }
    }

    void ExitNative()
    {
        _inNative--;
        // the deferred Dispose itself is queued (TryEnqueue) when it was requested; nothing to do here
    }

    // Same rule as MotionSystem: the UI thread is enforced in every build, not only Debug.
    void AssertUi()
    {
        if (!_queue.HasThreadAccess)
            throw new InvalidOperationException("MotionBridge must be used from its UI thread");
    }

    void SetSubscribed(bool on)
    {
        if (on == _subscribed) return;       // idempotent: never double-subscribe
        if (on && _disposed) return;
        if (on) { CompositionTarget.Rendering += _onRendering; SubscribeCount++; }
        else { CompositionTarget.Rendering -= _onRendering; UnsubscribeCount++; }
        _subscribed = on;
        try { SubscriptionChanged?.Invoke(on); } catch (Exception ex) { Fault("SubscriptionChanged", ex); }
    }

    // every fm_* call goes through these so _inNative is exact
    T Call<T>(Func<IntPtr, T> f, T dflt)
    {
        AssertUi();
        if (_sys == IntPtr.Zero || _disposed) return dflt;
        _inNative++;
        try { return f(_sys); } finally { ExitNative(); }
    }
    void Call(Action<IntPtr> f)
    {
        AssertUi();
        if (_sys == IntPtr.Zero || _disposed) return;
        _inNative++;
        try { f(_sys); } finally { ExitNative(); }
    }

    public bool Subscribed { get { AssertUi(); return _subscribed; } }
    public string? LoadTokens(string json)
    {
        var err = new byte[4096];  // same cap as MotionSystem; longer diagnostics truncate, not split
        int rc = Call(s => Fm.fm_load_tokens(s, json, err, err.Length), 1);
        if (rc == 0) return null;
        int n = Array.IndexOf(err, (byte)0); if (n < 0) n = err.Length;
        return System.Text.Encoding.UTF8.GetString(err, 0, n);
    }
    public ulong Create(ulong target, string? transition = null)
    {
        ulong id = Call(s => Fm.fm_animation_create(s, target, transition), 0UL);
        if (id != 0) _live.Add(id);
        return id;
    }
    public void Play(ulong anim) => Call(s => Fm.fm_animation_play(s, anim));
    public void Cancel(ulong anim) => Call(s => Fm.fm_animation_cancel(s, anim));
    /// <summary>Drops this bridge's handle entry; running motion keeps its controller and
    /// finishes normally. After <c>Release</c> the id must not be used again. Do not call
    /// from a sink callback. Returns false when the id was never created here or was
    /// already released.</summary>
    public bool Release(ulong anim)
    {
        bool released = false;
        Call(s => { if (_live.Remove(anim)) { Fm.fm_animation_release(s, anim); released = true; } });
        return released;
    }
    public bool Retarget(ulong anim, string prop, double target, double response = DefaultRetargetResponse, double damping = DefaultRetargetDamping)
        => Call(s => Fm.fm_animation_retarget(s, anim, prop, target, response, damping, 0, 0.0), 0) != 0;
    public bool RetargetFrom(ulong anim, string prop, double from, double target, double response = DefaultRetargetResponse, double damping = DefaultRetargetDamping)
        => Call(s => Fm.fm_animation_retarget(s, anim, prop, target, response, damping, 1, from), 0) != 0;
    public bool Current(ulong target, string prop, out double value, out double velocity)
    {
        double v = 0, vel = 0;
        bool ok = Call(s => Fm.fm_current(s, target, prop, out v, out vel), 0) != 0;
        value = v; velocity = vel; return ok;
    }
    public bool Running => Call(s => Fm.fm_is_running(s), 0) != 0;
    public void SetReducedMotion(bool on) => Call(s => Fm.fm_set_reduced_motion(s, on ? 1 : 0));

    /// <summary>Idempotent. If called while a native call is on the stack (e.g. from a sink callback),
    /// Rendering is unsubscribed immediately and fm_destroy is deferred to the dispatcher queue.</summary>
    public void Dispose()
    {
        AssertUi();
        if (_disposed) return;
        try { SetSubscribed(false); } catch (Exception ex) { Fault("dispose-unsubscribe", ex); }
        if (_inNative > 0)
        {
            if (!_disposePending)
            {
                _disposePending = true; DeferredDisposes++;
                try { Diagnostic?.Invoke($"Dispose requested inside native call (depth {_inNative}); fm_destroy deferred via DispatcherQueue.TryEnqueue"); } catch { }
            }
            if (!_queue.TryEnqueue(Dispose)) { try { Diagnostic?.Invoke("TryEnqueue failed; fm_destroy skipped (queue shut down)"); } catch { } }
            return;
        }
        // Release every handle this bridge created before fm_destroy drops the rest.
        // fm_animation_release only drops our handle entry; running motion finishes normally.
        // (_inNative == 0 on this path; the deferred path re-runs Dispose and releases here.)
        if (_sys != IntPtr.Zero) { foreach (ulong anim in _live) Fm.fm_animation_release(_sys, anim); _live.Clear(); }
        _disposed = true;
        if (_sys != IntPtr.Zero) { Fm.fm_destroy(_sys); _sys = IntPtr.Zero; }
        Frame = null; SubscriptionChanged = null; Diagnostic = null;
    }
}
