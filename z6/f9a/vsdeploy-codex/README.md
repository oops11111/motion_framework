# Reviewed GateB tool contract (not executed)

Default: `python z6/f9a/vsdeploy-codex/deploy_gateB.py` prints a dryrun plan and returns before OS/package/VS/activation calls. AST compile and PSParser checks passed; see raw/gateB-script-review. No runtime success is inferred.

Files: deploy_gateB.py (exact signed.exe wrapper/prePOST evidence/cleanup), ActivateNormally.ps1 (ordinary Windows activation, independently defaults dryrun), native_lifecycle.py (private read-only PID/path/package identity and ordinary WM_CLOSE, no kill). No Files source, new dependency or user setting is changed by preparing these tools.

Real execution is deliberately blocked pending TWO separately created current-conversation receipts, neither supplied/generated here:

1. Direct authority JSON: grant_kind exactly `F9a FilesDev selected-project deployment, ordinary startup, exact cleanup`; source exactly `Direct user response in current root conversation`; real user_verbatim and recorded_utc; manifest_sha256 bound to fresh GateA manifest. Existing one-flag/F5 permissions do not match this schema.
2. Primary instruction JSON: instruction_kind `Primary GateB execute`, real root_verbatim, and authority_sha256 equal to exact bytes of that new authority file. A file's existence alone is insufficient. The agent must have actually received both the direct grant and primary execution instruction; JSON is a persistent record, not permission manufactured by code.

Only AFTER receipt/review: `python .../deploy_gateB.py --execute --authority <new B receipt> --instruction <new primary instruction>`. The PS activation helper repeats grant/instruction validation and exact installed FilesDev App/empty-arguments binding before Add-Type/activation. No such execute command has run.

Execute preconditions recheck sourceHEAD/pin/clean, all bound inputs, signature, no VS/FilesDev/residualdata, declared existing framework publisher/version/status. WinPS5 serializes versions explicitly and architecture/status numerically. Full current-user package fullname PRE/AFTER_DEPLOY/POST snapshots remain ONLY in ignored attempt storage; publish delta/hash, not the full app inventory. Any observed non-FilesDev package delta stops activation, reports uncertain cause and does not uninstall those packages.

Normal close uses visible owned windows and a verified held process handle. Cleanup checks Files/Server candidate package identity: positively foreign/no-package/exited can be ignored; open/query failure is unknown and prevents removal, as does owned live. No force-close path. Finally catches cleanup/query errors and always writes result.json; only successful exact uninstall plus FilesDev POST absent sets cleanup_complete. Errors or state/identity uncertainty retain STOP/residue, never runtime PASS.

Original /Out/stdout/stderr, actual exit, package identity/process/window/close/source state and exceptions remain under a new ignored attempt. No B operation is allowed before new authority. Signed.exe support/success is to be tested, not guaranteed; GateA is ready for that user decision, not startup accepted.

Final review correction: normalize extension/protocol UserChoice snapshots (ordered extensions, case-insensitive ProgId, exact Hash). A delta is recorded as STOP, never automatically repaired. Wrapper returns nonzero after writing result for any incomplete/error/residue/state-delta outcome; actual deployment exit remains independently recorded. Latest syntax/defaultdryrun proof is raw/gateB-script-review-final. No B runtime was executed.

Main-page acceptance correction: visible HWND alone may be splash. Require read-only UIA SettingsButton automation ID under the exact verified PID/HWND, visible and enabled; no Name reads or interaction. Pin source references and hashes: MAINPAGE_MARKER.json. UIA helper runtime remains untested. On readiness failure, finally attempts normal WM_CLOSE using the verified held handle; retains original failure and records cleanup close outcome. Failed close still STOP/no force kill/no uninstall of live or uncertain processes.
