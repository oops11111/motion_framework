# F2 repair checkpoint

Status: implementation and private logic checks ready for review; host build and runtime NOT ACCEPTED.

The ordinary merge is 6d46424815d6072798a58de87d119b1c2971bd53. Main ledger prefix and both source ledgers were preserved. The isolated source is pinned to upstream eb08f0dbcbb43880ab34038b0cb6742f0c0deca5, then populated only with the recorded accepted F5 source baseline.

Root content is detached before the old RootView Unloaded handler disposes the pilot. Only its disposal ACK queues creation. The actual private state rejects pending/closed/stale/inline requests and catches queue/create failures. Listener disposal completes before this ACK. Async screenshots and queued rebuilds reject disposed lifetimes.

PilotEventCounts guards restricted IDs/names before its lazy payload getter. Other events only count names except lifecycle state and Idle active/pump fields. Listener ownership logs create/dispose active counts.

Actual helper tests: 18 checks PASS, including throwing payload getters, FrameTickAccept alias, actual IdleTransition ID 5 and failure paths. Existing F5 actual source behavior: 20 checks PASS after CI scope compatibility adjustment. Neither establishes GUI acceptance.

Original AGENTS command `dotnet restore FluentHub.slnx` on the default SDK 10.0.300, with only missing child PROCESSOR_ARCHITECTURE restored to AMD64, exited 1: NU1903 Microsoft.DotNet.ILCompiler 10.0.8, GHSA-p3jp-9v83-76w9. No build command or waiver was run. Earlier sandbox captures exited 1 silently; the unsandboxed same command supplied the actual error. Captured UTF-8 output already contains replacement characters; original capture bytes remain in ignored storage and their hash is recorded. No Chinese text reconstruction is claimed.

v1 roundtrip exposed four inherited LF-only sources. The four were normalized to required CRLF without logic changes. The normalized git patch bytes remain identical; all ten v2 source files are byte-equal to the clean applied tree. v1 failure is retained.

The preview patch is partial for review; fluenthub_f2_repair_v2.patch is the full ten-file upstream-based artifact. No old PASS/FAIL, logs, frozen source, API, closure, LOCKED or APPROVED status was changed. No GUI, package registration or system setting operations occurred.

Next: await the F2-specific NU1903 decision; then only the approved host build route. Stage 3 requires review of concrete runtime scripts and its separate package scope before execution.
