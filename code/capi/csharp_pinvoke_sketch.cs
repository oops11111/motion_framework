// mvp/csharp_pinvoke_sketch.cs
// *** UNCOMPILED, UNVERIFIED SKETCH *** Written against the described MotionSoftDemo (WinUI 3, C#,
// DispatcherQueueTimer @16 ms, TranslateTransform.X + Border.Opacity). Not built on Windows here.
// Native side: mvp/fm_capi.h/.cpp (tested on Linux under ASan/UBSan). Build it x64 and copy
// fluidmotion.dll next to the exe (csproj: <None Include="..\native\fluidmotion.dll"
// CopyToOutputDirectory="PreserveNewest" />).
using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Dispatching;

namespace MotionSoftDemo
{
    internal static class Fm
    {
        const string Dll = "fluidmotion";
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void SinkFn(ulong target, IntPtr property, double value, IntPtr user);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] internal delegate void RenderFn(int on, IntPtr user);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr fm_create(SinkFn s, RenderFn r, IntPtr user);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void fm_destroy(IntPtr sys);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int fm_load_tokens(IntPtr sys, [MarshalAs(UnmanagedType.LPUTF8Str)] string json, byte[] err, int cap);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void fm_set_reduced_motion(IntPtr sys, int on);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern ulong fm_animation_create(IntPtr sys, ulong target, [MarshalAs(UnmanagedType.LPUTF8Str)] string? transition);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void fm_animation_play(IntPtr sys, ulong anim);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void fm_animation_cancel(IntPtr sys, ulong anim);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int fm_animation_retarget(IntPtr sys, ulong anim, [MarshalAs(UnmanagedType.LPUTF8Str)] string prop, double target, double response, double damping, int hasFrom, double from);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void fm_on_frame(IntPtr sys);
    }

    /// Replaces the fake sine tick. The 16 ms DispatcherQueueTimer stays as the frame SOURCE, but now it
    /// is started/stopped by the engine (idle means idle) and only forwards the tick; time comes from the
    /// engine's own steady clock, so timer jitter does not change the motion.
    public sealed class MotionBridge : IDisposable
    {
        readonly Fm.SinkFn _sink;        // keep delegates alive for the native side's lifetime
        readonly Fm.RenderFn _render;
        readonly DispatcherQueueTimer _timer;
        readonly IntPtr _sys;

        public MotionBridge(DispatcherQueue queue, Action<ulong, string, double> onValue)
        {
            _timer = queue.CreateTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(16);
            _timer.IsRepeating = true;
            _timer.Tick += (_, __) => Fm.fm_on_frame(_sys);                   // UI thread
            _sink = (t, p, v, u) => onValue(t, Marshal.PtrToStringUTF8(p)!, v);   // never call Fm.* in here
            _render = (on, u) => { if (on != 0) _timer.Start(); else _timer.Stop(); };
            _sys = Fm.fm_create(_sink, _render, IntPtr.Zero);
        }

        public string? LoadTokens(string json)
        {
            var err = new byte[1024];
            return Fm.fm_load_tokens(_sys, json, err, err.Length) == 0 ? null : System.Text.Encoding.UTF8.GetString(err).TrimEnd('\0');
        }
        public ulong Create(ulong target, string? transition = null) => Fm.fm_animation_create(_sys, target, transition);
        public void Play(ulong anim) => Fm.fm_animation_play(_sys, anim);
        public bool Retarget(ulong anim, string prop, double target, double response = 0.32, double damping = 0.78)
            => Fm.fm_animation_retarget(_sys, anim, prop, target, response, damping, 0, 0.0) != 0;
        public void SetReducedMotion(bool on) => Fm.fm_set_reduced_motion(_sys, on ? 1 : 0);
        public void Dispose() { _timer.Stop(); Fm.fm_destroy(_sys); }
    }
}

// MainWindow wiring (sketch):
//   _bridge = new MotionBridge(DispatcherQueue, (target, prop, v) => {
//       if (prop == "opacity") box.Opacity = v;            // Border.Opacity
//       else if (prop == "offsetX") xform.X = v;           // TranslateTransform.X
//   });
//   _bridge.LoadTokens(File.ReadAllText("demo_motion.json"));   // copy mvp/demo_motion.json to the output dir
//   _enter = _bridge.Create(1, "demo.enter");  _drag = _bridge.Create(1);
//   button "Enter":  _bridge.Play(_enter);
//   button "Move":   _bridge.Retarget(_drag, "offsetX", moved ? 0 : 240);   // interruptible, velocity carries
//   toggle "Reduce": _bridge.SetReducedMotion(isOn);
