# Stage 2C: pilot pack metadata and historical provenance integration

Recorded 2026-11-21 AWST. Primary reviewed scope and approved this Plan before implementation. Source of acceptance: coord/z6-codex-handover at 9c28dcc6a97bda7d552ddf2a6cc6f12347063751, docs/z6_coord. N9 permits independent integration with disclosure rather than a universal external-review gate. No external review performed by this stage; original ticket verdicts remain unchanged. The user's continuous-project-delivery instruction (verbatim in ../tests/PLAN.md) authorizes continuing implementation/ordinary commit/push/PR, primary diff/CI review and merge only after all applicable CI passes. No system/install/non-original build flag permission is inferred.

## Scope, dependencies and exact sources

Base main 8c5cc6f31ca2be796986afd901fc1bb1098712ca; branch codex/z6-integration-pack; one PR targeting main. Stage2B is accepted: PR12, head6191d9db816d56c3d8d34cbb76f4791341c4e4c9, merge8c5cc6f31ca2be796986afd901fc1bb1098712ca; pushdoc44881562575, PRdoc44881582615, PRregression44881582621 and postmerge main44881810494 all applicable checks success, independently checked by primary.

Ordinary merge order preserves the actual dependency chain:

| Ticket | Exact tip | New source scope |
| --- | --- | --- |
| F7 | 6b9f4767829add919cb6e626f45208f2ecf59614 | pilot/l1pack README/SHA metadata plus25 historical files |
| F11 | c3fb8d926458ebb93c3bc675a13c5f25ab853719 |24 historical patch/report/script/raw files |
| F6 | 5e92d05ea3cf7c943dd3a79e97223db10c8d74e0 |45 historical patch/report/script/raw/window-evidence files |

All97 source paths are precisely two metadata files,94 historical files under z6/f7, z6/f11, z6/f6, and shared ledger. Metadata exact final blobs: README5f56d54b8c56ad1a807d9474b4c2ada9948df804, SHA256SUMS438f933fa27691a4569428f48856e89168f3b1fd. No pack source, production/test/schema/public surface/LOCKED changes; exclude F2/F5 main ancestry, closure and F9. Stage2A/2B tools/manifests and existing regression workflow remain unchanged. No executions of imported scripts or historical task/package commands.

## Deliverables and ledger

Under z6/integration/pack: Plan, exact source receipts, base+three source ledger archives, branch-scoped provenance checker, local reference inspection runner, raw evidence, report and SHA256SUMS. Archive ledgers -text exact; preserve the entire base-main ledger prefix and append stage2B acceptance/stage2C records. Only ledger conflicts are expected; no rewriting old reports/verdicts/raw. New branch-scoped metadata/provenance CI must not permanently freeze future legitimate pack rebuilds.

## Local reference verification (not a new pack build)

Read only the existing reference cache %USERPROFILE%/mf_z6/f7/pilot/l1pack/bin/x64/Release/net8.0. All three files already matched proposed F7 SUMS during Plan inspection:

- FluidMotion.L1.dll SHA25694d6489d9bec289732ea3e4a8540b052e576a7182bae3f3367a3e72f04e10dc2
- fluidmotion.dll SHA256add4857dead413f8211941381c6aa8f73c546f3ecae1774aced79c464719577d
- demo_motion.json SHA2568d5db5775acad5db0c04cbc3c2a214bb7309670c304b71ff40860757973c410e

`python z6/integration/pack/inspect_reference.py`: recheck reference hashes, copy into a new ignored m0/out attempt and verify copied bytes; copy the repository's original SurfaceDump_Program.cs/SurfaceDump.csproj.txt to a separate ignored tool directory and ordinary `dotnet build <tool.csproj> -c Release`. Run that existing reflection-only tool against copied managed DLL; require CheckSurface fails=0 and full output bytes equal pilot/fluenthub/surface_l1pack.txt. No MotionSystem function, native entrypoint, ETW listener or GUI invocation; no pack rebuild or source mutation. If any baseline difference (including only first line), stop and show primary evidence rather than silently normalize. Log sanitized commands/exits and hashes, new raw each file under5MB. Do not modify or Git-operate the external workbuddy cache.

`python z6/integration/pack/verify.py`: validate exact imported blobs, four ledger snapshots/full prefix, metadata declarations and current frozen source Git hashes, stage scope/noLOCK/source freeze, own manifest coverage/hash. Local cache/API evidence is separate from CI provenance validation. Do not repeat local native/C# functional regressions or GUI.

## CI gates, acceptance and stop conditions

New policy runs on this stage branch push and this PR only; verifies source/evidence/metadata/provenance/manifest, not unavailable cloud cache. Existing PR policy plus Debug/Release regression3 naturally run unchanged; postmerge main regression3 must pass. Typically push policy1 plus PR policy1/regression3; primary checks every applicable run rather than fixed count. Primary merges only after final diff/receipts and all CI success; implementation agent never merges.

Fresh stage acceptance = exact metadata/history integration plus matching local reference hashes and reflection API. It is not a current-main pack rebuild, new F6 GUI run, F11 relocation/corruption run or original F7 re-adjudication. Stop on base/tip drift, unexpected non-ledger conflict, source/frozen changes, missing/mismatched cache, API diff, or need for install/system operation/new override flag; preserve evidence and report instead of widening scope. Overall project remains incomplete.
