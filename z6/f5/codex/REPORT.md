# F5 host repair continuation — implementation prepared, host acceptance blocked

2026-11-21 AWST. Not accepted; not a replacement for the existing F5 FAIL report. Unreviewed until primary-agent review. No main merge, no timing data, PERF-CI HOLD, Success does not imply adopt.

## Baseline and change

- Implementation branch `codex/z6-f5`, base `e4cd696c10ad3d9a948c5650295be7fbbbc301cc`.
- PR target `codex/z6-integration` starts at that same base. This does not accept inherited F2/F11/F7 results or merge them into main.
- Isolated local side fork under ignored `m0/out/f5-fluenthub`, upstream fixed at `eb08f0dbcbb43880ab34038b0cb6742f0c0deca5`. The three existing original-sidefork work files were copied without modifying their original location. NavMotionPilot PRE hash matches the supplied F5 post hash `d0b111f56e2a809dedd566186e559cd1f1f2a8895e4a51667ee2344f67fec065`.
- Only `MainWindow.cs`, `NavMotionPilot.cs` and three new internal host source files change in the incremental patch. RootView recreation order is unchanged. No third-party source tree is committed; only the patch is delivered.
- Window owns one WinUIEx 2.9.2 WindowMessageMonitor and single-shot 100 ms DispatcherQueueTimer. Every WM_SETTINGCHANGE schedules a reread regardless of wParam. Changed values notify the host subscriber, which queues SetReducedMotion onto its dispatcher. Unchanged rereads only log. WinRT event is version-guarded and secondary; Activated also rereads.
- MainWindow initial source value is applied synchronously on the constructor's UI thread before Play. Fake mode `FM_PILOT_F5=1` uses the actual `Local\FM_PILOT_F5_TOGGLE` named auto-reset event; its background wait only queues UI work. OS rereads cannot overwrite Fake state. Queued callbacks check disposal. No periodic polling and no system setting writes.
- lParam is logged as an address, not dereferenced: injected diagnostic messages may carry null or arbitrary addresses. Neither the monitor nor its callback consumes the message.

## Verification and evidence

| Gate | Authoritative status | Evidence |
| --- | --- | --- |
| A frozen surface | PASS (dev-only), local | `raw/frozen_surface.json`: PRE=POST blob and normalized SHA; delivery verifier scope; no changed frozen paths |
| Patch applies to recorded F5 sidefork baseline | PASS (dev-only), local | `git apply --check --directory=m0/out/f5-patchcheck ...`; patch application reproduced all five final file hashes; `raw/patch_roundtrip.json` |
| Source behavior | PASS (dev-only), local, 20 checks | `python z6/f5/codex/verify.py --run-source-tests`; `raw/source_tests_attempt4.log`; actual patch-extracted ReducedMotionState and FakeReducedMotionSource, not reimplementations |
| Original FluentHub host build | BLOCKED before compilation, exit 1 | `raw/build_attempt3_original.log`: NU1903, Microsoft.DotNet.ILCompiler 10.0.8 advisory GHSA-p3jp-9v83-76w9. No warning suppression or dependency upgrade performed |
| B Fake to actual MotionSystem in host and unchanged OS snapshots | PENDING | Independent source test proves queue/fake logic only; not claimed as host GUI acceptance |
| C actual WinUI HWND hook and injected message | PENDING | Full host build must succeed first; no hook-install log yet |
| E actual RootView recreation and host shutdown | PENDING | Tests cover subscriber replacement and queued-after-disposal logic only; real host lifecycle still required |
| D optional real setting switch | SKIPPED, not authorized | No SPI SET, registry writes or settings changes |
| Remote CI | PENDING until run is observed | `.github/workflows/z6-f5.yml`, Windows runner, freeze/patch/actual source tests. This CI does not claim full FluentHub build or GUI acceptance |

Local source tests check unchanged-value suppression, duplicate WM/event deduplication, real source ignoring Fake requests, subscriber replacement, disposal, Fake isolation from OS rereads, real named-event delivery into a simulated UI queue, state mutation only on that queue's thread, flip/restore, queued callbacks after disposal, and enqueue rejection. Timeout waits only prevent hanging tests; they are not performance acceptance thresholds.

## Attempts, environment and deviations

1. Local-path git clone failed because Git's sh could not create a signal pipe under the execution sandbox. Anonymous public upstream clone succeeded; fixed commit and copied work files were verified. No account or third-party write operation occurred.
2. Host build attempt 1 used the original command `dotnet build src/FluentHub/FluentHub.csproj -c Debug -p:Platform=x64`, exit 1 during restore, no error text. Its console transcript is preserved separately and explicitly labeled; no redirected log existed.
3. Attempt 2 repeated with diagnostic verbosity to diagnose the sandbox failure, retaining the diagnostic log. Restore-graph MSBuild tasks failed without a definitive application error. Diagnostic MSBuild build-task durations are incidental tool output, not application timing measurements or gate data.
4. Attempt 3 ran the same original build command outside the execution sandbox after automatic permission review; this was process/filesystem access, not Windows elevation, installation or a changed build switch. Restore returned the explicit NU1903 blocker above. No further full-host build was attempted without specific user approval of a non-original switch.
5. Independent test attempts 1/2 hit execution-sandbox directory-write denial; source remained unchanged. Attempt 3 exposed CA1416 in the new test harness because OpenExisting is Windows-only; the harness received an explicit OperatingSystem.IsWindows guard. Attempt 4 passed all 20 checks. The failed compile log and passing log are both preserved.

## Limits and exact next actions

The whole F5 stage is not complete and must not merge, even if source CI is green. Primary agent must review the incremental patch and CI, then resolve the exact host-build blocker with the user. If a non-original NU1903 switch is approved, record its verbatim authorization and run a new attempt; do not overwrite these logs. GUI package registration, task creation, system setting changes and installation remain separately unauthorized. Prefer an existing runnable package if available without system changes. Hook failure stops the real path rather than silently accepting partial operation.

Original sidefork sources were left unchanged; the isolated copy and ignored build/test output remain for diagnosis. No package registration/unregistration, scheduled task, SDK install, system animation change, third-party push, force/amend/rebase, LOCK operation or main merge was performed. See PLAN.md for current-conversation ordinary delivery authorization and remaining stage dependencies.
