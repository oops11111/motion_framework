timestamp: 2026-10-19 18:28:46 +0800 Asia/Taipei
chat URL: https://claude.ai/chat/5b0701a6-cbce-4ee5-a392-87e8d3c95643
prompt=docs-diff follow-up (api.md + usage.md + MotionSoftDemo plug-in notes)
status: complete — Free plan still available; full reply captured
account: miitwo.e@gmail.com (Free)
artifacts: fm_capi.h, fm_capi.cpp, fm_capi_test.cpp, demo_motion.json, csharp_pinvoke_sketch.cs
  (in /workspace/claude_docs_diff_artifacts/)

---

# Claude reply — docs diff + MotionSoftDemo plug-in

Chat: https://claude.ai/chat/5b0701a6-cbce-4ee5-a392-87e8d3c95643
Account: pidan (Free). Model shown: Sonnet 5.5 Medium.
Sent with attachments api.md and usage.md.
Generation finished. No free-limit / reset banner.

---

Tracing timeline, spring, and token usage through each example.

You’ve uploaded the real api.md and usage.md. Only usage.md came through inline, so I’ll read api.md from disk and diff both against the current headers by script and by hand.

Ran 2 commands

Two overloaded names (Curve::Bezier, MotionSystem::Create) can’t go through decltype directly. I’ll resolve them with explicit casts, which still fail to compile on any signature mismatch.

Ran 7 commands, shared files

The IDispatcher signatures in api.md and usage.md have not drifted from the headers. The new bezier accessors are the only code API missing from the docs. Both documents were written before module 6 and before the spring-threshold fix, so their test counts and a few behaviour notes are stale. usage.md also has two compile errors in its own snippets.

I checked this mechanically, not by eye. I compiled static_asserts on every signature api.md declares against the current headers, and I compiled the usage.md snippets as written. The new C-ABI shim and its test passed under ASan and UBSan on Linux, but nothing was built on Windows or with C# here.

1) Drift list

Signatures that match (no drift)

IDispatcher: Submit(const ControllerSnapshot&), Withdraw(ControllerId, EndReason) and Apply(TargetId, const SampleMap&) match api.md §5.2 exactly. The field types in ControllerSnapshot and SpringOverrideInfo match too, as does MakeInstantTimeline.
Everything else in api.md: all other api.md signatures also match: Spring, CubicBezier, Track, Timeline, Player, Curve, TokenSet, AnimationHandle and MotionSystem. The APIs it lists as absent (SnapToEnd, MotionSystem::Advance) are indeed absent.

Drift in the docs

#	Severity	Finding	Evidence	Fix
1	Medium	api.md §2.1 lacks the new bezier accessors.	The header has X1(), Y1(), X2(), Y2() and IsLinear() (static-asserted). api.md lists only the constructor, IsValid, Ease and Slope.	Add the five accessors, noting that the compositor planner reads them.
2	Medium	Track::SpringTo duration is described as SettleTime.	Settling is now span-relative (SpringSettleTime, SpringPosEps(span), SpringVelEps(span)). A 0.96→1 scale spring now lasts over 0.3 s instead of about 0.06 s.	Change to “duration = SpringSettleTime(spring, from, target)”. Add the three helpers to §3.2 and say kSpringPosEps and kSpringVelEps are now upper caps.
3	Medium	Neither document mentions composition.h.	api.md lists its authoritative sources without it, and §5.2 and §6 say the Composition dispatcher is “planned”. In fact the planner, CompositionDispatcher, ICapabilityProbe and ICompositorBackend exist and pass 2,063 checks against a simulated compositor. The real WinRT backend is still absent.	Add a section for composition.h marked scaffold. Reword “planned” to “planner and dispatcher implemented (simulation only); no WinRT backend”.
4	Medium	Test counts are stale.	Current: timeline 422 (docs say 419), tokens 743 (docs say 617), controller 236 (same). Composition adds 2,063.	Update the counts in api.md §8 and usage.md §6, or replace them with a pointer to the last run.
5	Low	Withdraw and Apply descriptions are looser than the behaviour.	Withdraw(Completed) also fires when a controller merely goes idle, for example on a paused pose. Apply runs only on ticks where something is active or dirty.	See the paste-ready text in the previous reply, item D2.
6	Low	The docs say code/ while the workspace folder is motion/.	api.md and usage.md refer to code/*.h and code/motion.json.	Confirm your local layout. If the folder was renamed, update the references.

Bugs in usage.md itself

§1.1 does not compile. ManualClock clock; at global scope collides with the C library’s clock() that <chrono> and <fstream> pull in. g++ gives “redeclared as different kind of entity”; MSVC will probably do the same. Rename it to gClock or appClock everywhere in the examples.
§1.2 does not compile. std::cerr needs #include <iostream>, which the snippet omits.
§2 contradicts §3.3. Curve::FromToken("emphasized") works, but it returns the placeholder curve from bezier.h, not the one in the designer’s token file. Use tokens->Easing("emphasized") in the example.

With the two compile errors fixed, every other snippet compiles against the current headers.

2) Plugging into MotionSoftDemo

Blocker: MotionSoftDemo is C#, and software_dispatcher.h is a C++ header. C# cannot include it, and no native library is wired yet. The thinnest bridge is a C-ABI shim exposing SoftwareDispatcher and FramePump to P/Invoke. I wrote and tested it:

File	State
fm_capi.h, fm_capi.cpp	14 functions over MotionSystem, SoftwareDispatcher and FramePump; builds as a shared library
fm_capi_test.cpp	23 checks through the C ABI only, ASan and UBSan clean
demo_motion.json	Two transitions, demo.enter and demo.exit, animating opacity (bezier) and offsetX (spring), with reduced variants
csharp_pinvoke_sketch.cs	MotionBridge class and MainWindow wiring; uncompiled sketch

What the shim does:

Frame source: your 16 ms DispatcherQueueTimer stays. The engine now starts and stops it through the render callback, so it stops when nothing animates. The timer’s Tick only calls fm_on_frame.
Time: the engine reads its own steady clock, so timer jitter does not change the motion. That is the real difference from the sine tick.
Values: the sink callback receives opacity and offsetX every frame. C# writes Border.Opacity and TranslateTransform.X from it, and must never call fm_* inside the callback.
Waking: every call that can start or change motion wakes the pump inside the shim. fm_animation_play, fm_animation_retarget and fm_set_reduced_motion all do this, which settles the earlier gap about waking after SetReducedMotion.

What the test confirmed:

demo.enter settles in 69 frames (575 ms) and ends exactly on opacity 1 and offset 0.
The pump stops itself, and nothing is written while idle.
A retarget mid-flight carries value and velocity with no jump.
A mid-flight reduced-motion switch lands on the final values one frame later.
Bad token files return the diagnostics.
Destroying the system with animations still running is clean.

Windows-side requirements and risks (unverified):

A native toolchain is the real unknown. The DLL must be x64 and built with MSVC Build Tools (cl /std:c++17 /EHsc /O2 /LD fm_capi.cpp /Fe:fluidmotion.dll) or MinGW-w64. You said no winget or admin was needed so far, and the demo builds with dotnet alone. If neither compiler is available, that is the first thing to settle.
Fallback without a compiler: generate per-frame value tables from this Linux-tested engine for the two transitions, and have the C# demo replay them. That checks the wiring and visuals but not the engine on Windows. I can produce those tables if you want them.
Delegate lifetime: the two callback delegates must stay referenced for as long as the native system lives, as the sketch does. The garbage collector would otherwise crash the app on the next callback.
Same-thread rule: the DispatcherQueueTimer ticks on the UI thread, so all calls stay on one thread as long as buttons call the shim from their handlers.
Packaging: whether the packaged-app deployment copies fluidmotion.dll beside the exe as shown in the sketch’s csproj line has not been checked.

What this does not do: it does not exercise composition.h or any compositor lowering. This is stage A from the earlier plan. Stage B still needs the real backend and the answers to the three open questions.

Artifacts from this reply:
- /home/box/Downloads/fm_capi.h
- /home/box/Downloads/fm_capi.cpp
- /home/box/Downloads/fm_capi_test.cpp
- /home/box/Downloads/demo_motion.json
- /home/box/Downloads/csharp_pinvoke_sketch.cs
