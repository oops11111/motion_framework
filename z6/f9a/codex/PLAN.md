# Stage 3A: F9a environment prerequisites

Status: implementing. Primary reviewed this bounded Plan before implementation. Base main122f7b4edc7698e742658532e3e1a83010f39f17; branch codex/z6-f9a; target main via reviewed PR. Stage2C accepted by primary: PR13 merge122f7b4edc7698e742658532e3e1a83010f39f17, head db208be6da7442b9105efbd767a6a5ccf8c6d710; all five head checks and post-main run44883079762 three jobs success.

## Current authorization and boundary

Direct user response in current root conversation, verbatim:

> VS 2026 C++ 工作负载可以由你安装

Recorded by primary 2026-11-21 16:26:38 AWST, recording time not send time; message id unavailable. This supersedes the older actor-only installation restriction solely for the existing VS2026 C++ workload and recommended dependencies. It does not authorize F2 adjudication/NU1903/temporary packages, F9b, install account changes, payment, restart, force-close or arbitrary components. User's continuing delivery instruction remains recorded verbatim in z6/integration/tests/PLAN.md. Original F9a FAIL/attempt1 remains unchanged.

## Dependency and approved operation

Existing instance bf096482, %ProgramFiles%/Microsoft Visual Studio/18/Community, VS18.6.2, channel VisualStudio.18.Release. Fresh preflight verifies identity, signature Valid/Microsoft Corporation for existing Installer/setup.exe, no devenv/setup/vs_installer process, non-admin session1. VS2022BuildTools remains untouched; SDK10.0.404 retained. No new bootstrapper.

Exact command: `%ProgramFiles(x86)%/Microsoft Visual Studio/Installer/setup.exe modify --installPath "%ProgramFiles%/Microsoft Visual Studio/18/Community" --channelId VisualStudio.18.Release --add "Microsoft.VisualStudio.Workload.NativeDesktop;includeRecommended" --quiet --norestart`.

Use signed installer Start-Process -Verb RunAs -WindowStyle Hidden -PassThru; user confirms UAC personally. Execution sandbox escalation is not OS elevation. No --wait/force/updateall/includeOptional/remove/global-includeRecommended; no automatic repeat of denied/cancelled UAC. Official parameter source reviewed by primary: https://learn.microsoft.com/en-us/visualstudio/install/use-command-line-parameters-to-install-visual-studio?view=visualstudio .

## Deliverables and evidence

Preserve old wb/z6-f9a030216fad26d2a2e0d3d3d9ef7edf057d66b07bc source via ordinary merge and exact ledger snapshot; all original attempt1/raw/FAIL untouched. New namespace holds authority, preflight, one installation attempt, command/PID/UTC times/exit and associated new dd logs. Originals retained in ignored attempt storage; sanitized new repository artifacts each under5MB, large logs indexed/bounded excerpts explicitly labeled if needed. Main ledger entire prefix preserved with append-only new records.

## Verification and acceptance

After installer completion read only vswhere same instance: NativeDesktop and VC.Tools.x86.x64 present, isComplete=true; validate v145 props and MSVC tool paths and versions. Verify SDK10.0.404 still exists, no reinstall. Read true installer exit plus isRebootRequired:0 success vs3010 pending restart vs740/elevation/5007 etc distinct. Never restart;3010 or rebootRequired is not ready. No hello/native/Files baseline build in3A. Stage3B baseline needs a separate reviewed Plan and only original command; no F5/F2 special flag reuse.

Cloud CI cannot prove machine installation. PR documentation/evidence policy plus existing regression CI provide provenance and repository regression, separately labeled. Primary reviews all applicable CI and main postmerge checks; implementation agent never merges.

Stop on UAC cancel/denial, competing setup/VS, account/network credential prompt, additional install/update scope, required restart or other unexpected failure. Preserve evidence and report, never force-close/bypass/expand. Environment completion does not adjudicate F9a historical FAIL or complete overall project.
