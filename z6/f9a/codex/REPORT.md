# F9a stage3A: VS2026 C++ workload installation evidence

Local environment acceptance: PASS, independently rechecked by primary. Repository evidence CI/merge: pending. Historical Files baseline FAIL/5007 and all78 original source files remain exact; no new Files baseline build has run. Original verdicts are not re-adjudicated. Overall project is incomplete.

User directly authorized: “VS 2026 C++ 工作负载可以由你安装”. Source/current-conversation scope and recording time are in AUTHORIZATION.json and PLAN.md; this overrides only the older installer actor restriction. No F2/NU1903/package/F9b grant is inferred.

Branch codex/z6-f9a, base main122f7b4edc7698e742658532e3e1a83010f39f17. Plan949f007 preceded installation; ordinary merge8dd1871 preserves F9a source030216fad26d2a2e0d3d3d9ef7edf057d66b07bc. Base and source ledger archives are exact; root ledger retains entire base prefix and appends prior stage acceptance and current provenance.

## Actual installation, not launcher-only success

Attempt20261121T083326Z used existing signed Microsoft Installer/setup.exe with the exact reviewed command: modify only %ProgramFiles%/Microsoft Visual Studio/18/Community, channel VisualStudio.18.Release, add Microsoft.VisualStudio.Workload.NativeDesktop;includeRecommended, quiet/norestart. Start-Process RunAs succeeded; agent did not interact with UAC or credentials. No second installer command, automatic restart, force-close, paid SKU/account operation, optional components or VS2022 modify command.

Initial launcher PID47336 exited0 at08:34:04Z, while workers34504/57604 continued: that launcher return was explicitly NOT acceptance. Worker34504 (hex86c8) installed recommended components, including VC14.51 and Windows SDK10.0.26100. The SDK child exited0/restartingNo at08:54:53Z. Actual final worker log reports Completed product modification at08:56:20Z and Closing installer exit code0/ExitCode0 at08:56:26Z. All setup/SDK processes were absent at final postflight08:56:57Z. Read-only watch reached its loop bound; it did not terminate installation. No restart was required or performed.

## Read-only final verification

Same instance bf096482 remains Community18.6.11822.322, isComplete=true/isRebootRequired=false. Simultaneous vswhere requires NativeDesktop and Component.VC.Tools.x86.x64 returned this instance. v145 x64 Toolset.props exists (SHA45927a0697ed83cc6e5f62bae0c9c9865a486c788471cfe095b5f0228159776e), vcvars64 exists. MSVC directory14.51.36231 contains real cl/link/lib files: cl FileVersion19.51.36246.0, link/lib14.51.36246.0; exact file hashes and paths are in postflight.json. MSBuild remains18.6.3.22110. No hello/compiler test/native regression or Files build was executed.

User SDK10.0.404 remains present; its .version hash7c640ddf62826144982bfd8f9e9cf5d888e0b4e4a673822f6647c39c51cd3bf4 matches the during-installation snapshot and lastwrite remains2026-10-29. Machine SDK list remains8.0.428/9.0.321/10.0.300. VS2022BuildTools retains version17.14.37328.6/isComplete=true; no command targeted it. Recommended shared dependency installations are recorded in installer logs rather than claiming all shared runtime bytes unchanged.

## Evidence and limits

raw/install-20261121T083326Z contains sanitized command/preflight/started/launcher-exit/watch/diagnostic/postflight/result records, complete sanitized main logs where small, bounded head/tail main excerpts where over threshold,268-file original-log inventory and last3line package excerpts. All complete original bytes are retained in ignored m0/out/f9a-install-20261121T083326Z/original-dd-logs (including the SDK logs exactly linked by package140). The index records original byte counts/SHA256 and whether published content is complete or an excerpt; no original log was deleted. New repository files are under5MB. SHA256SUMS covers every stage artifact except itself; `python z6/f9a/codex/verify.py` checks coverage/hashes,78 exact original files, two ledger archives, base prefix, local result consistency and bounded repository scope. CI checks record integrity, not a reproduction of installation on the user's machine.

The final errors log contains7 lines: at08:56:20Z Error0x80070005, Could not sync DCAT registration, access denied to HKLM/SOFTWARE/Microsoft/Windows NT/CurrentVersion/Update/TargetingInfo/PersistentDynamicInstalled. This appeared during finalization after the earlier empty-errors monitoring snapshots. Installer exit0 and workload/component/tool readiness passed, but DCAT registration sync failed; downstream impact was not assessed. No registry change, repair or reinstall was attempted. The original error log is retained.

NGEN emitted a System.Design warning that Microsoft.Data.SqlClient could not be loaded. NGEN tasks and final installer returned0; final instance/component/tool checks passed. This warning is retained; no repair, added dependency or install retry was attempted.

Production/test/pack source/schema/LOCKED and earlier integration tools/manifests are unchanged. Existing regression CI runs naturally; no local native/C# regression rerun. No package registration, tasks, system animation change, credentials handling or F9b work. F2 remains pending direct adjudication/its own permissions. Stage3B only has a read-only Plan draft: isolated original Files cbfb4ba54a89632fbceb82ea94ada60a9bae3737 baseline after separate primary approval, with session-only user SDK selection and no special build overrides. It has NOT started.

Whitespace gate is limited to new authored files (stage Plan/report/authorization/scripts/receipts/manifest/workflow). Full base-to-head diff includes immutable historical logs with pre-existing trailing whitespace; those bytes are preserved and no whole-PR whitespace PASS is claimed.

Next: primary reviews final evidence/diff and all applicable CI, merges only when green and verifies main postmerge CI. Environment readiness does not turn the old baseline into PASS; new baseline results need their own attempt.
