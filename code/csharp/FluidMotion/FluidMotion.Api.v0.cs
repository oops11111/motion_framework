// FluidMotion public C# API v0 — SHAPE ONLY (docs/api_v0_DRAFT.md). WP4 / F4: frozen in the
// shape a future WinMD projection will have, so pilot code binds once and the WinMD swap later
// is invisible to it. Underneath it runs on the C ABI (code/capi/fm_capi.h) and the Rive host
// DLL (m0/native/m0rive). Members marked [C-ABI+] need a C ABI addition before they work.
// Bodies are placeholders; nothing here is implemented yet.
//
// WinRT shape rules followed (so the WinMD swap does not change call sites):
//  - public types are sealed classes, enums, or structs of public fields only;
//  - no public generic types; collections as IReadOnlyList<T> (projects to IVectorView<T>);
//  - no overloads that differ only by parameter type; optional values are separate methods;
//  - events use TypedEventHandler<TSender, TArgs>; disposal through IDisposable (IClosable);
//  - every member is called on the UI thread that created the MotionSystem / RiveView.
using System;
using System.Collections.Generic;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace FluidMotion
{
    /// <summary>State of an animation (controller.h AnimationState).</summary>
    public enum AnimationState { Idle = 0, Playing = 1, Paused = 2, Finished = 3, Cancelled = 4, Superseded = 5 }

    /// <summary>Why an animation ended (controller.h EndReason).</summary>
    public enum AnimationEndReason { Completed = 0, Cancelled = 1, Superseded = 2 }

    /// <summary>Spring used by retargeting (spring.h SpringParams; Period = response / 2π on the compositor, S1/A1).</summary>
    public struct SpringSpec
    {
        public double Response;      // undamped period in seconds (>= 1e-4)
        public double DampingRatio;  // >= 0
    }

    /// <summary>Result of loading a token file (tokens.h Diagnostic list).</summary>
    public sealed class TokenLoadResult
    {
        public bool Succeeded => throw new NotImplementedException();
        /// <summary>One problem per entry, with a JSON-Pointer path (fm_load_tokens err).</summary>
        public IReadOnlyList<string> Diagnostics => throw new NotImplementedException();
    }

    /// <summary>
    /// The element an animation drives. v0 properties: "opacity", "translateX", "translateY",
    /// "scaleX", "scaleY", "rotation" (degrees), applied to the element's composition visual.
    /// </summary>
    public sealed class MotionTarget
    {
        public static MotionTarget ForElement(UIElement element) => throw new NotImplementedException();
        public UIElement Element => throw new NotImplementedException();
    }

    /// <summary>Engine instance bound to one UI thread (controller.h MotionSystem; fm_create).</summary>
    public sealed class MotionSystem : IDisposable
    {
        /// <summary>Creates the engine on <paramref name="dispatcherQueue"/>'s thread; frames run only while something animates.</summary>
        public MotionSystem(DispatcherQueue dispatcherQueue) => throw new NotImplementedException();

        /// <summary>Loads a motion.json (schema v1, strict). Replaces previously loaded tokens on success.</summary>
        public TokenLoadResult LoadTokens(string json) => throw new NotImplementedException();

        /// <summary>Reduced motion; turning it on snaps running animations to their end (controller.h §5.7).</summary>
        public bool ReducedMotion { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>Animation for a token transition; null when the transition does not exist.</summary>
        public AnimationHandle? CreateAnimation(MotionTarget target, string transition) => throw new NotImplementedException();

        /// <summary>Empty animation, for Retarget only.</summary>
        public AnimationHandle CreateRetargetOnly(MotionTarget target) => throw new NotImplementedException();

        /// <summary>Current value and velocity of a property (owner's live value, else rest value).</summary>
        public bool TryGetCurrent(MotionTarget target, string property, out double value, out double velocity) => throw new NotImplementedException();

        /// <summary>Number of active animations. [C-ABI+]</summary>
        public int ActiveCount => throw new NotImplementedException();

        public void Dispose() => throw new NotImplementedException();
    }

    /// <summary>One animation (controller.h AnimationHandle). Dispose drops this handle; the motion keeps running (fm_animation_release).</summary>
    public sealed class AnimationHandle : IDisposable
    {
        public void Play() => throw new NotImplementedException();
        public void Cancel() => throw new NotImplementedException();

        /// <summary>Velocity-preserving interruption to <paramref name="target"/> (Bv behaviour, F5). False on failure.</summary>
        public bool Retarget(string property, double target, SpringSpec spring) => throw new NotImplementedException();

        /// <summary>Retarget with an explicit start value, used only when nothing drives the property and it has no rest value.</summary>
        public bool RetargetFrom(string property, double target, SpringSpec spring, double from) => throw new NotImplementedException();

        /// <summary>[C-ABI+]</summary>
        public AnimationState State => throw new NotImplementedException();

        /// <summary>Raised once when the animation ends. [C-ABI+]</summary>
        public event TypedEventHandler<AnimationHandle, AnimationEndedEventArgs>? Ended;

        public void Dispose() => throw new NotImplementedException();

        internal void SuppressUnusedEventWarning() => Ended?.Invoke(this, null!);
    }

    public sealed class AnimationEndedEventArgs
    {
        public AnimationEndReason Reason => throw new NotImplementedException();
    }

    /// <summary>Options for PointerFollow (WP2 guidelines, docs/m3_wp2_follow_guidelines.md).</summary>
    public struct PointerFollowOptions
    {
        public bool FollowX;
        public bool FollowY;
        public double OffsetX;       // DIP added to the pointer position
        public double OffsetY;
        public double SmoothingSeconds;  // spring time constant hiding the ~2-frame seam; 0 = none (default ~0.027 s at 75 Hz)
    }

    /// <summary>
    /// Makes a split-out part follow the pointer on the compositor (pointer property set +
    /// ExpressionAnimation): ~2 frames faster than inside a Rive surface (M3-RIVE-FOLLOW).
    /// Use only for parts loosely coupled to the Rive body (WP2 rule 2/3).
    /// </summary>
    public sealed class PointerFollow : IDisposable
    {
        public static PointerFollow Attach(UIElement follower, UIElement pointerArea, PointerFollowOptions options) => throw new NotImplementedException();
        public void Dispose() => throw new NotImplementedException();
    }
}

namespace FluidMotion.Rive
{
    /// <summary>Raised when the GPU device behind the view is lost (H4/H5).</summary>
    public sealed class RiveDeviceLostEventArgs
    {
        /// <summary>DXGI removal reason HRESULT.</summary>
        public int Reason => throw new NotImplementedException();
    }

    /// <summary>Raised after automatic recovery from a device loss (H4).</summary>
    public sealed class RiveDeviceRecoveredEventArgs
    {
        public double RecoveryMilliseconds => throw new NotImplementedException();
        /// <summary>
        /// Recovery restarts the animation from t = 0 (K6). On some systems XAML itself does not
        /// redraw after a driver restart (L-XAML-1); the host may want to offer an app restart.
        /// </summary>
        public bool AnimationRestarted => throw new NotImplementedException();
    }

    /// <summary>
    /// XAML control hosting a Rive file on a composition surface, with the WP1 host requirements
    /// built in (docs/m3_wp1_rive_host_requirements.md): first-frame warm-up (H2), re-rasterisation
    /// on DPI change (H3), device-loss recovery (H4) with notification (H5), per-monitor pacing
    /// with the post-vblank offset (H6).
    /// </summary>
    public sealed class RiveView : Control
    {
        /// <summary>Path or ms-appx URI of the .riv file.</summary>
        public string Source { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        /// <summary>State machine to run; empty = the artboard's default (else animation 0).</summary>
        public string StateMachine { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool AutoPlay { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Play() => throw new NotImplementedException();
        public void Pause() => throw new NotImplementedException();

        /// <summary>State machine inputs. [C-ABI+ in m0rive]</summary>
        public void SetBooleanInput(string name, bool value) => throw new NotImplementedException();
        public void SetNumberInput(string name, double value) => throw new NotImplementedException();
        public void FireTrigger(string name) => throw new NotImplementedException();

        public event TypedEventHandler<RiveView, RiveDeviceLostEventArgs>? DeviceLost;
        public event TypedEventHandler<RiveView, RiveDeviceRecoveredEventArgs>? DeviceRecovered;

        internal void SuppressUnusedEventWarnings()
        {
            DeviceLost?.Invoke(this, null!);
            DeviceRecovered?.Invoke(this, null!);
        }
    }
}
