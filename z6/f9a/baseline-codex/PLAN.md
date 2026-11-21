# F9a stage3B: isolated original Files baseline continuation

Status: PLAN ONLY. Primary requested this concrete Plan commit; clone/build implementation waits for explicit primary execution instruction after stage3A main post-CI succeeds. No Files clone, build, package registration or new installation is performed by preparing this Plan.

## Acceptance source, dependency and authority

Base main3078ae0e9f8792a03f45e09dd4fce5c6fda1ca7e; branch codex/z6-f9a-baseline; eventual PR target main. Stage3A PR14 merged as that SHA from reviewed headfba233e688e396e7d26e8e9bcbcfd091ba00b376, exact tree8f8142ab15bddba64284dafec775a8af7dfd5edf. All five head checks passed; main post-CI44887910560 was running at Plan creation and is a hard prerequisite before implementation.

Source of task/acceptance: coord/z6-codex-handover9c28dcc6a97bda7d552ddf2a6cc6f12347063751, original F9a source030216fad26d2a2e0d3d3d9ef7edf057d66b07bc REPORT/Files BASELINE and their explicit continuation: same Files v4.3, first original upstream CI command, necessary original T4 command next. User's continuing project-delivery instruction is preserved verbatim in z6/integration/tests/PLAN.md. Primary reviews this Plan and owns diff/CI/main merge; implementation agent implements/tests/commits/pushes/opens PR only after execution instruction. User's separate VS workload grant is recorded in z6/f9a/codex/AUTHORIZATION.json and is not a general install/flag/package permission.

Prerequisites: same VS2026 bf096482 complete/noReboot with NativeDesktop+VC.Tools.x86.x64, real v145/cl present; existing user SDK10.0.404/Roslyn5.9. Stage3A final installer exit0 and these gates passed, while DCAT registration synchronization Error0x80070005 and NGEN System.Design/Microsoft.Data.SqlClient warning remain disclosed, not repaired. Do not modify any3A file, manifest or workflow. No reinstallation or system PATH change.

## Scope and isolated source

Only new stage plan/runner/provenance checker/evidence/report/SHA256SUMS under z6/f9a/baseline-codex plus .github/workflows/z6-f9a-baseline.yml and append-only aggregate ledger (archive complete current-main ledger prefix first). Existing original BASELINE/REPORT/attempt1/5007/raw and3A archives stay byte-exact. No production/test/pack/L1/public surface/schema/LOCKED changes, third-party writes, F2 adjudication, F5 integration, closure, F9b, source fixes or dependency changes.

Files source pin: cbfb4ba54a89632fbceb82ea94ada60a9bae3737 (v4.3). Existing %USERPROFILE%/pilot/Files is read only. Before cloning record its HEAD/status and exact Git blobs of global.json, solution/app/project/publish profile/Directory.Build.props/Directory.Packages.props/NuGet sources and upstream CI; fail if the pin/worktree unexpectedly changed. Use a newly named ignored m0/out/f9a-baseline-<UTC>-<id>/run1 directory, `git clone --no-hardlinks --no-checkout "%USERPROFILE%/pilot/Files" <new-source>`, then `git -C <new-source> checkout --detach cbfb4ba54a89632fbceb82ea94ada60a9bae3737`. This uses local Git objects without changing the original working tree or contacting/writing the upstream repository. Verify detached HEAD, clean tracked/untracked status and key source blobs; no old bin/obj is copied. If clone/submodule/LFS state cannot reproduce the exact pin without additional tooling or credentials, stop.

Only build-child environment sets DOTNET_ROOT=%USERPROFILE%/.dotnet and prepends that existing directory to PATH. Restore missing PROCESSOR_ARCHITECTURE=AMD64 only in child from known x64 host; record original/actual values. No setx/User/Machine environment mutation. Record actual user dotnet version/info, SDK Roslyn file version, VS18 MSBuild file version and source metadata. Do not set compiler paths/MSBuildSDKsPath/SDK resolver overrides. No WarningsNotAsErrors/NuGetAudit/toolset/TFM/profile/AOT/recipe bypass or other extra build properties. Preserve NuGet warnings/errors as output.

## Exact original commands and stage gates

Run1 uses existing %ProgramFiles%/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/amd64/MSBuild.exe, cwd fresh run1 source. Substitute only paths, leaving the original flags intact:

```text
MSBuild.exe Files.slnx -t:Restore -p:Platform=x64 -p:Configuration=Debug -p:PublishReadyToRun=true -v:quiet
MSBuild.exe src/Files.App/Files.App.csproj -t:Build -p:Configuration=Debug -p:Platform=x64 -p:AppxBundle=Never -p:GenerateAppxPackageOnBuild=false -v:quiet
```

Run build only if restore succeeds. Capture separate command/child-env/version/UTC/exit/full stdoutstderr and source PRE/POST receipts. Success is restore0 and build0 with no source modifications. MSBuild may still use its bundled Roslyn5.6 even when SDK404 is selected; record the actual result rather than changing compiler paths to force success.

If run1 passes, do not run run2 to accumulate evidence. If run1 fails on an actual toolchain/build error where the documented T4 alternative is applicable, retain its full failure then use a second entirely fresh clone of the SAME source pin under run2 (no deletion/clean of run1 or old user tree), with the same child-only user SDK environment:

```text
%USERPROFILE%/.dotnet/dotnet.exe build src/Files.App/Files.App.csproj -c Debug -p:Platform=x64
```

Run2 is one different original command, not a retry of the same failure. Do not run it after restore authentication/network/system prerequisite failure or to bypass a new explicit stop condition. If run2 fails, preserve it and stop with exact error. Never loop either command or switch flags. Report run1 and run2 separately even if run2 passes; do not erase or relabel run1 failure. Build success alone does not prove package deployment/GUI/runtime/AOT-ETW compatibility. A later runtime gate would need a separate Plan and any package authorization.

## Deliverables, validation and CI

Versioned fresh attempt directory per command route in ignored storage; complete original logs preserved. Publish sanitized command/environment/version/source/status/exits and bounded build log evidence, full logs when under5MB or explicit index/SHA/bounded excerpts otherwise. No full Files source or generated binaries in this repository. Hash produced app assembly/manifest if present to bind evidence without starting them. Record no-source-change POST, repository freeze/public-source identity, current main ledger entire-prefix retention and stage manifest complete coverage. New report appends current facts in its own namespace and never changes historical verdicts.

Local verification: exact clone/source pin+key blobs+clean source PRE/POST, original flags/child SDK selection, actual process exit and error evidence, artifact hash bindings, immutable old3A/original evidence, scope/frozen source and new manifest. Do not rerun this repository's local native/C# regression or GUI for documentation-only result publication. New branch-scoped CI validates provenance/flags/scope/evidence integrity, NOT reproduction of local Files build. Existing PR regression policy/Debug/Release naturally run unchanged. Fixed stage whitelist only current stage branch/head; future unrelated PRs are not frozen by it.

Acceptance requires primary review of local raw and per-route result, all applicable PR/head checks green, ordinary main merge and postmerge main CI green. Environment readiness and repository CI alone are not a Files build PASS. Baseline may legitimately remain FAIL with preserved current evidence if original commands still fail. Implementation agent never merges.

## Stop conditions and next dependency

Stop before implementation if main/source pin changes or3A post-CI is not green. Stop during work for source/props/dependency/profile changes, unexplained dirty clone, credentials or rate/endpoint failures, additional install/restart/UAC/cert/driver/package registration requirement, any proposed new build override, or frozen/LOCKED/schema changes. Preserve original outputs, report evidence and ask primary for the narrow decision; no automated repair or widening scope. No F9b without user's explicit open decision. F2 remains pending its own user adjudication/permissions; this stage does not imply them.
