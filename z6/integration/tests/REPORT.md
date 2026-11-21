# Stage 2A — local integration PASS (dev-only), CI/merge pending

2026-11-21 AWST. Branch `codex/z6-integration-tests`, base/main `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`, PR target **main**. The primary agent reviewed the plan and count-only test fixture. Original ticket labels and reports are preserved, not re-adjudicated; this stage records fresh deterministic native/C# integration acceptance. External Notion review is not a blanket prerequisite for these independent tickets (coord adjudication N9); F2 verdict changes and Z6 closure remain outside scope.

## Integrated sources and boundary

| Ticket | Preserved source tip | Ordinary source merge |
| --- | --- | --- |
| F0 | `45cbe21b96ac917c28635065b01206210aace738` | `9916a2d` |
| F3 | `5a3cef559e826a8450a252add7efd8659125a0e4` | `2ad406e` |
| F1 | `5ec13f459a7926012407b10cf263127e5a74ab73` | `aab9041` |

Only two existing test sources change: the combined native test and the existing devsmoke test file. Production C ABI/header/composition, MotionSystem, MotionEtw/schema, L1 project/public surface and all LOCKED documents remain unchanged. F2/F5 ancestry is excluded; F5's already accepted separate integration merge is only referenced in the ledger. Documentation/pack tickets, F9a and unopened F9b are not bundled here. No Z6 closure/LOCK or adopt decision.

F1/F3 native hunks merged without test conflict. The sole recurring conflict was shared ledger add/add. Every source snapshot is retained as its exact Git blob under `source_ledgers/`, with `-text` attributes preventing encoding/newline rewriting; the aggregate ledger retains the original F0 prefix and adds ASCII stage records. All 109 imported evidence files match their original source blobs (`source_receipts.json`). Old reports and raw outputs were not edited.

## Test-only fixture correction

The frozen production MotionEtwMirror formats all payloads, so the devsmoke now uses a private EventListener within its test file. It counts names/IDs, maps id 103 to FrameTick, and returns for FrameTick/PresentStats before any Payload access. Only IdleTransition's non-timing fields are read, retaining the existing Dispose assertion. All original and imported assertions remain. No EventSource/schema or public member changes were needed.

## Fresh Debug/Release verification

Canonical commands: `python z6/integration/tests/run_tests.py --configuration Debug` and `--configuration Release`. Each builds/runs the full existing net8 console test project, not a partial FluentHub/Files application build. Native commands match the established F0/F1 recipe; exact generated commands and exit codes are preserved in each configuration's receipt. No warning, audit, target framework or toolset override was used; the F5-only NU1903 grant was not reused.

| Gate | Debug | Release |
| --- | --- | --- |
| MSVC native test build/run | exit 0; **45 checks, 0 failures** | exit 0; **45 checks, 0 failures** |
| Fresh native DLL and copied DLL hash | verified | verified |
| Whole standalone managed build/run | exit 0; verdict PASS, fails=0; 68 ok lines | exit 0; verdict PASS, fails=0; 68 ok lines |
| Original/F1B deterministic cases | retained and executed | retained and executed |
| Restricted ETW output | FrameTick/PresentStats counts only, no payload | same |
| CheckSurface | 20 checks, fails=0; exit 0 | same |
| Full public dump comparison | exact equality after CRLF→LF only | same |

Both full public dumps have SHA256 `243c2e009252fa1d7d74b11c0c173a425a1804e1581157946882f852c8c23a34`, exactly matching the stored per-configuration Z5 baseline. No first-line or member-difference exemption was used. Native total is 23 base + 12 F3 + 10 F1 = 45. Both configurations passed on their first attempt; there was no failed test rerun or skipped assertion.

Evidence: `raw/Debug/`, `raw/Release/`, `raw/acceptance_summary.json`, and `raw/frozen_source_and_scope.json`. Each original command/result hash and source SHA snapshot applies to the original ignored output; provenance maps it to the delivered UTF-8/LF/path-sanitized copy. Ticket SHA256SUMS covers delivered bytes, including the opaque original source ledger archives. Files are bounded below 5MB. Original outputs remain under the ignored per-configuration `m0/out` attempt directories.

composition.h remains blob `fd0603c17136c067e89c71e56aef310f2f7e63f4`, normalized SHA256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`. Policy checks also verify 41 frozen files, original imported blobs and exact ledger archives.

## CI, review and limitations

New Windows CI has one policy/evidence job and a real Debug/Release native+managed+surface matrix. Current stage path restrictions use `--enforce-stage-scope` only for this named branch/PR, so later legitimate documentation or pack PRs are not rejected by this stage's path whitelist. Frozen-source/evidence checks and the regression matrix remain applicable generally. A pending or failed check is not acceptance; the primary agent owns final review and merge only after all checks pass on the exact head, then verifies the target/main push checks.

Fresh GUI runs were unnecessary and were not performed. F0 GUI and F1C FluentHub results remain historical imported evidence, not fresh claims. No package registration, tasks, SDK/runtime/toolchain installation, credential action, system setting or persistent environment change occurred. The standard missing architecture value was restored only in test subprocess environments from existing machine AMD64; stale test DLL/session overrides were cleared only in those child environments.

The native test's original manual-clock-derived frame/ms display is not a wall-clock observation or pass threshold. No PresentStats fields or FrameTick timing fields were consumed by the new fixture, no timing conclusions are made, PERF-CI stays HOLD, and Success does not mean adopt. Further independent documentation/pack integration needs its own scoped stage; F2/F5 main integration and closure remain unresolved separately.
