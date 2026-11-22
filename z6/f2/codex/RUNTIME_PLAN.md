# F2 stage 3 runtime preparation

Status: PREPARATION ONLY. F2 package-operation permission is pending; no registration, activation, real UIA, ETW app observation or input has occurred.

Dependency: accepted stage2 commit 3048050, v2 ten-source patch and x64 output. Build is NON-ORIGINAL (the two specifically approved properties); old failures and all historic evidence remain immutable.

Scope: only this 49462fluenthub-uwp.FluentHub 0.11.0.0 x64 identity / original publisher. No new host code/build flags, source changes, system animation changes, tasks, certificates, components, account operations or force kill. No performance sampling or restricted payload reads.

Layout: RUNTIME_LAYOUT.json binds all 284 existing app-output files. No recipe was emitted; the complete appbin has the declared executable/manifest/resources. Static presence is preparation evidence, not proof registration will succeed. A new exact-byte workspace copy may be used if needed; no synthetic files, reindex or modified manifest.

Tools: run_runtime.py (default dryrun), ActivateNormally.ps1 (default dryrun / real WinPS5 ValidateOnly), RuntimeGuard.ps1, native_lifecycle.py and ReadAnimationSettings.ps1. Runtime binding covers authority context, all tools, layout inventory, source10, host DLL, manifest and patch. ValidateOnly verifies the full guard and compiles COM code, then returns before activation or package/system queries.

Execute requires a new direct F2-specific package grant AND a separately bound primary execution instruction. Existing F5/Files grants are not accepted. Review-ready requested scope: temporary Add-AppxPackage -Register exact manifest; ordinary Fake then Real; normal close; exact newly owned package removal. Existing same-name package/data/process or unknown identity stops before mutation.

Prechecks: full source/layout hashes, manifest identity/publisher/version/x64, framework publisher/minimum/status, exact package and residual data absence, no matching host process, read-only four animation settings and all package fullname snapshot. No runtime dependency installation.

Fake: ordinary COM activation with existing exact --fm-pilot-f5=1 argument. Hold verified PID/path/full package identity. Observe two actual RootViews. Signal existing process named event twice only; require opposite fake state then restoration, initial/apply same UI thread and no OS reread override.

Real: ordinary COM activation with empty args. Verify one window-owned hook across two RootViews. Send only WM_SETTINGCHANGE 0x1A/wParam0x6D/null via bounded SendMessageTimeoutW to verified owned HWND; require sourcewm unchanged and no additional SetReducedMotion. No setting is changed.

Both: assert old Unloaded/Dispose and listener active0 precede new Create/listener active1, exactly two subscribers/listeners, no overlap, per-cycle Created==Released>0, final Idle active0/pump0 and clean normal exit. Totals alone do not establish ordering. Only safe names/lifecycle/Idle evidence is consumed.

Each mode finally attempts normal WM_CLOSE on a verified held identity. After exit, BEFORE the next activation and BEFORE uninstall, copy FULL z6_f2_motion.log original bytes to a mode-specific immutable ignored file and record SHA. The source path may be package LocalState or TEMP fallback; ambiguous fresh logs stop. Preserve any prior logs without deletion. Fake capture completes before Real starts. Published evidence is a separately redacted copy with separate SHA, never described as original bytes.

Failure: retain the first error and all available evidence, no automatic registration retry. Always save final result. A failed assertion still permits safe cleanup after original log preservation; an existing unreadable log, live owned process, unknown process identity or partial-registration ambiguity stops uninstall. Never remove framework/foreign package or force-close processes.

Post: no owned/unknown live process, exact newly created fullname removal, exact package/data absence, no unexpected package delta, four settings unchanged, source/layout unchanged. Any unexpected difference is reported, never repaired automatically.

Acceptance: actual Fake/Real ordered lifecycle plus mandatory F5 B/C/E regression, full original logs preserved before overwrite/removal, ownership and cleanup closed. CI tests source logic/hash/scope only; GUI remains a separately reviewed real-run gate. Root reviews concrete tools and final evidence, tracks all CI, merges only after all required gates pass. No closure/LOCKED change.
