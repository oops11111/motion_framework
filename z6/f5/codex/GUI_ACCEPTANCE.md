# Prepared F5 B/C/E CLI acceptance — not executed

2026-11-21 AWST. The runner defaults to **read-only dry-run**. Package registration and GUI execution require the current user's specific authorization; no such authorization is inferred from this file or from a previous agent's records.

## Fixed inputs and concrete actions

- Manifest: `m0/out/f5-fluenthub/src/FluentHub/bin/x64/Debug/net10.0-windows10.0.26100.0/win-x64/AppxManifest.xml` under this repository.
- Manifest SHA256: `fe5dc89b96997208936d7ad3263254a047ef604c696d1ce8be40b5a4bdd243f4`.
- Exe SHA256: `7b0688b0e9504779d4af26390ce776339eda3ff850f9fa35dddeffa14ca55a4c`.
- Only identity `49462fluenthub-uwp.FluentHub`, version 0.11.0.0, x64, publisher `CN=4E3C0825-4D00-4ECC-89F8-528E9200B125`; any existing package of that exact name stops the runner before registration.
- Execute mode registers only that loose generated manifest with `Add-AppxPackage -Register`. It records the resulting exact PackageFullName, checks publisher/version/architecture/install location, then runs the prepared probes. It does not install dependencies, certificates or SDKs, change developer mode or settings, create tasks, invoke UAC, or set user/machine environment.
- Cleanup uses `Remove-AppxPackage -Package <that recorded exact PackageFullName>` only if ownership still matches and no recorded owned host remains running. Unknown process or package ownership stops removal and reports cleanup pending. Existing runtime/framework packages are never removal targets.

## Three deliberately separate launches

1. **Fake package-context diagnostic:** the existing WinPS5 `Invoke-CommandInDesktopPackage -PreventBreakaway` starts `LaunchHostInPackage.ps1`. That fresh diagnostic process sets AMD64 and Fake only in its process environment and starts the exact new exe. The script checks the wrapper identity, then the driver independently checks the host's process executable path and package full name. It waits for both RootView cycles and old-subscriber disposal, sends two signals to the real Local named event, verifies two actual host SetReducedMotion calls with inverse bool mapping, restore and UI thread identity, then closes its owned PID's windows and checks clean exit/disposal.
2. **Real package-context diagnostic:** same context launcher with Fake removed from that process only. It verifies one top-level HWND hook across RootView recreation, posts WM_SETTINGCHANGE with deliberately unrelated wParam 0x006D and null lParam, then requires an unchanged reread and no new source=wm SetReducedMotion call. It closes the owned process and checks both subscriber disposals and one hook removal.
3. **Ordinary Real activation:** `ActivateHostNormally.ps1` uses IApplicationActivationManager's normal Windows.Launch contract with only AO_NOERRORUI, obtaining the exact process ID. It does not enable package debugging or design mode. The same real-source hook/injection/lifecycle checks apply. Fake is not required for this launch. Results identify this separately from the diagnostic token.

Microsoft documents the package-context cmdlet as a diagnostic process with package identity and a token that differs from ordinary activation; its behavior is not guaranteed to duplicate a normal app process. The runner therefore keeps those results separate and adds ordinary Real activation rather than claiming the diagnostic proves standard launching. [Invoke-CommandInDesktopPackage](https://learn.microsoft.com/en-us/powershell/module/appx/invoke-commandindesktoppackage?view=windowsserver2025-ps). The activation API returns the PID and supports disabling its error dialog with AO_NOERRORUI. [ActivateApplication](https://learn.microsoft.com/en-us/windows/win32/api/shobjidl_core/nf-shobjidl_core-iapplicationactivationmanager-activateapplication).

Before and after each run, and after cleanup, read only: UISettings.AnimationsEnabled, SPI_GETCLIENTAREAANIMATION, MinAnimate and UserPreferencesMask. Any mismatch stops acceptance. No real setting switch is part of this script. Observation timeouts prevent hanging automation; no elapsed time is a performance gate.

## Reviewable commands

Already run, exit 0, with no package or GUI mutation:

```
python z6/f5/codex/gui_acceptance.py --manifest m0/out/f5-fluenthub/src/FluentHub/bin/x64/Debug/net10.0-windows10.0.26100.0/win-x64/AppxManifest.xml --dry-run
```

Python parsing and WinPS5 parsing of all three helpers passed. This validates syntax and read-only preflight, not the unexecuted launch/cleanup branches. The dry-run snapshot was AnimationsEnabled=True, SPI=True, MinAnimate="1", mask="9E3E078012000000". `raw/gui_preflight_dryrun.json` preserves the sanitized result.

**Only after** the actual user reply is verified and recorded by the primary agent:

```
python z6/f5/codex/gui_acceptance.py --manifest m0/out/f5-fluenthub/src/FluentHub/bin/x64/Debug/net10.0-windows10.0.26100.0/win-x64/AppxManifest.xml --execute --authorization-file m0/out/f5-gui-authorization.json
```

The authorization JSON must contain the current conversation ID, the exact identity and manifest hash above, scope `F5 temporary registration, package-context diagnostics, ordinary Real activation, exact cleanup`, source `direct user response in current root conversation`, verbatim `user_text`, the real `question_item_id`, and the AWST recording time. An arbitrary old file or a generated record alone is not permission. No approval template with a prefilled approval statement is supplied.

Each invocation creates a new ignored attempt directory under `m0/out`. Existing LocalState logs are copied read-only to per-mode prior-log snapshots before application startup; the driver never truncates or deletes them. Newly written app logs are captured even on failed validation, alongside state/stdout/stderr, settings snapshots, result JSON and SHA256SUMS. Application log sequence counters are interpreted separately for each new process, never spliced into old evidence. Only current owned process windows receive WM_CLOSE; there is no forced process kill or screenshot capture.

After an authorized run, preserve originals in that ignored attempt directory, append sanitized bounded evidence under this ticket, and have the primary agent review actual B/C/E logs and current-tip CI. Do not merge based on this preparation or dry-run.

## Final execution update (2026-11-21 13:12 AWST)

The user subsequently authorized the pending scoped package operations. Actual evidence and the final acceptance continuation are in `GUI_RESULTS.md`. The original preparation above is retained as history.

The diagnostic Fake launch was inconclusive and is not retried. The final default is **OrdinaryFake + Ordinary**, both normal Windows.Launch activations. OrdinaryFake passes only `--fm-pilot-f5=1`; final v3 host code recognizes exactly one extra process argument, sets only that process's FM_PILOT_F5 environment variable, and uses the same Fake source. Ordinary has no flag and uses the real source. Legacy Fake/Real diagnostic modes remain for historical reproducibility but are not missing acceptance gates. `--launch-modes Ordinary` can select only C/E and is explicitly not a complete B/C/E acceptance.

WM_SETTINGCHANGE diagnostic delivery now uses synchronous **SendMessageTimeoutW**, not PostMessageW: the earlier asynchronous attempt returned ERROR_MESSAGE_SYNC_ONLY. It still uses the exact owned top-level HWND, message 0x001A, unrelated wParam 0x006D and null lParam. Return, last error and message result are preserved; no setting is changed. Both known log locations (package LocalState and TEMP fallback) are checked, with prior copies preserved and ambiguity rejected. Normal app visibility and the managed FluentHub.dll hash are recorded separately from the unchanged exe shim.

The first hung diagnostic required one carefully verified manual cleanup exception to the original no-force plan, limited to its own PID/start time/exe/package with a held process handle. That failure and termination are retained, not described as a normal exit. The final two ordinary runs exited normally and required no force. All package removals targeted only the exact newly registered package, and four OS settings remained unchanged.
