# F5 continuation acceptance — PASS (dev-only), local gates A/B/C/E

Recorded 2026-11-21 13:12 AWST. Final implementation is `fluenthub_f5_window_source_v3.patch`; v1/v2 patches and all failed attempts remain evidence. This is a new repair/acceptance continuation, **not** a rewrite of the original `z6/f5/REPORT.md` FAIL. Primary-agent review and latest-tip CI remain required before merging this PR to `codex/z6-integration`. No main merge, adopt decision, timing gate, LOCK or Z6 closure is claimed.

## Final delivered behavior and artifact binding

The v3 patch changes six host files only: MainWindow and NavMotionPilot, App.xaml.cs, and three new internal reduced-motion source/state classes. The window owns one monitor/debounce source across both RootViews. Initial application is synchronous in the NavMotionPilot constructor before Play; later changes queue onto the UI thread. Any WM_SETTINGCHANGE is accepted for a 100 ms reread debounce; unchanged values only log. WinRT event and activation rereads remain secondary paths.

Normal activation with exactly one extra process argument `--fm-pilot-f5=1` sets **only that app process's** FM_PILOT_F5=1 before window construction. Recognition requires `Environment.GetCommandLineArgs().Length == 2` and ordinal full-string equality. The existing environment-variable entry remains available. No-argument ordinary activation uses the real source. No arbitrary argument contents are logged.

Build attempt 8 completed **exit 0**, two NU1903 warnings, zero errors, using the specifically approved **non-original** `-p:WarningsNotAsErrors=NU1903` plus the already diagnosed child-only AMD64 environment repair. No extra MSBuild property was used. `raw/host_artifact_attempt8_v3.json` binds the actual build to:

| Artifact | SHA256 |
| --- | --- |
| FluentHub.dll (managed implementation) | `9be938e614b636034840223429e0bdd60cc21342823fb31c0eb5859471008b7a` |
| FluentHub.exe (unchanged apphost shim) | `7b0688b0e9504779d4af26390ce776339eda3ff850f9fa35dddeffa14ca55a4c` |
| AppxManifest.xml | `fe5dc89b96997208936d7ad3263254a047ef604c696d1ce8be40b5a4bdd243f4` |

Both final GUI runs independently record that same managed DLL hash. Runtimeconfig EventSource.IsSupported remains true. Six-file patch application and normalized-byte roundtrip match, with separate v2/v3 records. Frozen code, L1/pack, LOCKED documents and original sidefork source remain unchanged (`raw/frozen_surface_v3.json`).

## Final acceptance: GUI attempt 5, ordinary activation

Authoritative local result: `raw/gui_attempt5/result.json`, complete actual application logs and `raw/gui_final_acceptance_summary.json`. Both modes used normal Windows.Launch activation through IApplicationActivationManager; neither used the restricted diagnostic token or a hidden app launch.

| Gate | Evidence and result |
| --- | --- |
| A frozen surface | composition blob `fd0603c17136c067e89c71e56aef310f2f7e63f4`; normalized SHA `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`; protected diff empty; no main merge |
| B Fake flip/restore | OrdinaryFake PID 62252; visible host and verified package/exe identity. Exact command-line flag log precedes Fake source construction. Two actual `SetReducedMotion` calls: True with AnimationsEnabled=False, then False with AnimationsEnabled=True restored. Both thread=2, matching initial UI application. One Fake source survives both subscriber cycles |
| C real hook and message | Ordinary PID 69204; visible top-level HWND `0xfc11ea`, one installed WindowMessageMonitor. Synchronous injected 0x001A with unrelated wParam=0x006D and null lParam is logged; debounce reread reports unchanged True; **zero** new `source=wm` SetReducedMotion calls. API return/last_error and HWND/PID are recorded |
| E lifecycle | Each run attached cycles 1 and 2, detached both, disposed one window source and exited 0. Real run removed its one hook. Existing F2 ordering is preserved, not rejudged |
| D optional real OS switch | Not executed or authorized. No claim of real OS-broadcast setting-change validation |

Four read-only settings matched before/after each final mode and after cleanup:

| Setting | PRE = POST |
| --- | --- |
| UISettings.AnimationsEnabled | True |
| SPI_GETCLIENTAREAANIMATION | True |
| MinAnimate | `1` |
| UserPreferencesMask | `9E3E078012000000` |

Both final hosts closed normally, exit 0. Exact temporary package `49462fluenthub-uwp.FluentHub_0.11.0.0_x64__tq8wa33sfftnm` was removed; remaining package list is empty. No runtime/framework package was removed. No certificate, developer-mode, UAC, task, system animation or persistent environment change occurred. Observation watchdogs only prevent hanging automation; they are not performance criteria. PresentStats fields and FrameTick timing fields were not used.

## All GUI attempts, retained separately

| Attempt | Actual outcome | Cleanup |
| --- | --- | --- |
| 1, package-context Fake diagnostic | **Not accepted**: no pilot log in either known location and no observed subscriber cycles; owned host 87000 remained running. This does not establish an F5 OS-source failure. The earlier .NET MainWindowHandle=0 was not evidence of no HWND: later native enumeration found a FluentHub WinUI HWND | See the cleanup continuation below |
| 1 cleanup continuation | WM_CLOSE failed. Before a necessary cleanup exception to the prepared no-force plan, PID/start time/exact executable/hash/package were reverified while holding a handle. Only this self-created host was terminated, exit marker 0xf5000001. Its wrapper was already absent; no unrelated shell or process was terminated | Exact package removed and four POST settings unchanged; full proof under `raw/gui_attempt1/` |
| 2, ordinary Real, v1 | **Not accepted**: startup, visible window, both RootViews and hook worked, but test harness PostMessage injection returned 1159 (message requires synchronous delivery). This was an injection-tool defect | Normal host closure and exact package removal; settings unchanged |
| 3, ordinary Real, v1 | **PASS C/E only**, not whole F5: SendMessageTimeout injection, unchanged reread, one hook and two subscriber lifetimes verified | Normal exit, exact removal, settings unchanged |
| 4, ordinary Fake requested, v2 | **Not accepted**: new host flag wrongly read WinUI LaunchActivatedEventArgs.Arguments, so actual source stayed real; named Fake event absent. This was an implementation defect in the added activation entry, not an OS setting failure | Normal closure, exact removal, settings unchanged |
| 5, ordinary Fake and ordinary Real, v3 | **PASS (dev-only) B/C/E**, fully bound to managed DLL hash above | Both exit 0, exact removal, all four settings unchanged |

Microsoft documents that WinUI desktop LaunchActivatedEventArgs.Arguments is empty and recommends Environment.GetCommandLineArgs. The v3 correction follows that API behavior; the v2 failure and hashes are retained rather than hidden. [Microsoft API documentation](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.launchactivatedeventargs.arguments?view=windows-app-sdk-2.0).

Original ignored attempt directories remain local. Delivered copies use UTF-8/LF and machine-path placeholders; each `evidence_provenance.json` maps original hashes to delivered hashes. The renamed source SHA snapshot describes original unredacted driver output and can predate cleanup additions; the ticket-level SHA256SUMS covers every delivered file. Existing LocalState/TEMP logs were read/copied before new writes; no unique application log was discarded.

## Specific package-operation authorization

User's direct message in the current root conversation, verbatim:

> 授权

It replied to the pending F5 temporary loose-package registration, B/C/E package-context and ordinary activation, and exact cleanup question. Root recorded this at **2026-11-21 12:50:34 AWST** (recording time, not asserted send time). Message ID is unavailable; the record explicitly says so instead of inventing a tool identifier. The per-attempt authorization copies bind the current conversation, manifest SHA, exact identity, scope and this original text. It did not open F9b or authorize any workload/runtime installation or real system animation change. The separately recorded F5-only NU1903 build approval remains the only non-original build switch used.

## Next integration action

Primary agent reviews final v3 diff, evidence hashes, frozen surface and all latest-tip CI checks. Only then merge PR #10 to `codex/z6-integration`. Main integration still waits for outstanding Notion/F2 decisions. F8a/F8b remain DRAFT; PERF-CI remains HOLD; no Z6 closure document is created here.
