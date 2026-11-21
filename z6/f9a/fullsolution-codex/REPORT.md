# F9a stage3C2 original full-solution build — FAIL

Original documented full-solution Build: FAIL/BLOCKED. Repository evidence/scope validation is a separate gate; CI/primary merge review is pending. Source remained unchanged. This failure does not replace the earlier3B app-project Build PASS or the historical F9a results. Startup is NOT TESTED; F9a is not complete and F9b is not open.

Base main8e68cf81ef10bd0623651949e6a98ddef4f175ca; branch codex/z6-f9a-fullsolution. PLANf81b954959bea242f1ea83a8890b750a4d97fbcb preceded execution; EXECUTION.md records primary approval and3C1 mainpost44890658744 success. All existing stage artifacts/manifests/workflows remain immutable; full base ledger archived exact and aggregate retains entire prefix with appended current facts.

## One exact original command

Fresh no-hardlink isolated local clone at Files cbfb4ba54a89632fbceb82ea94ada60a9bae3737, detached/clean. Clone81228/checkout60272 exited0. Existing user dotnet --version returned10.0.404. Only child DOTNET_ROOT/PATH-prefix and missing AMD64 restoration changed; no persistent environment, source or compiler override. Relevant inherited certificate/upload/build overrides were checked absent without dumping values.

The pinned AGENTS command, substituting only existing VS18 MSBuild full path, ran once:

```text
MSBuild.exe -restore Files.slnx -p:Configuration=Debug -p:Platform=x64 -v:quiet -clp:ErrorsOnly
```

PID78948 started09:42:30Z and exited1 at09:44:59Z. Complete original stdout/stderr remains in ignored m0/out/f9a-fullsolution-20261121T094221Z-e33cb21d-fullsolution/build.original.log; sanitized complete text and original/published hashes/bytes are recorded in the attempt result. ErrorsOnly is the upstream logging parameter: absence of warning output is NOT evidence that no warnings occurred. Compiler/tool file versions are recorded, without claiming which compiler assembly was actually loaded.

## Exact error and stop

Files.App.Launcher.vcxproj(145,5) reports that a referenced NuGet package is not present. Missing file relative to source:

`packages/Microsoft.Windows.ImplementationLibrary.1.0.260126.7/build/native/Microsoft.Windows.ImplementationLibrary.targets`

Read-only existence check confirms absent. package_stop.json binds the Launcher project SHA/Git blob, relevant source line numbers and its unchanged packages.config declared versions (WIL1.0.260126.7 and CppWinRT3.0.260818.1). Original -restore did not provide this repository-level packages.config payload. This is the observed missing-package stop, not a judgment that every upstream VS restore route fails.

No RestorePackagesConfig property, separate NuGet restore, import/source fix, package copy, alternate target, install or rerun followed. No certificate creation/import, Deploy/registration, application/test execution or F9b action. Any next restore route is separate and needs its own reviewed Plan and required user non-original-route decision; root research is not user authorization.

## Source and partial-output observations

SourcePRE==POST for full clean HEAD/tree/key receipts, no tracked/untracked change; source pin remains exact. Native PostBuild outputs went to ignored assets only. The one failed solution still produced separately compiled Server.exe SHA c39f5fd212e91de45cc4887b2e90ad4cde68bf44fd975228fc261b9a865a9c05 and four OpenDialog/SaveDialog32/64 DLLs; their hashes are recorded, not executed. Launcher binary remains absent. The main app Files.dll/exe/AppxManifest/recipe, AppX and intermediate PackageLayout paths were absent in this NEW clone after failure. These are current clone observations, not changes to the successful3B outputs and not proof about normal VS deployment.

Artifact/layout observations and source/cfg receipts were obtained read only after the sole command. No manual layout was created. Building solution test projects is not running tests; no test or app activation target was requested. All generated binaries remain ignored, with hashes only in the repository.

## Evidence gate and limits

`python z6/f9a/fullsolution-codex/verify.py` validates exact original argv/SDK environment, actual exit1/error/provenance, source PRE/POST, partial-output and package-stop receipts, base ledger prefix, complete stage manifest and bounded diff. Local original BuildFAIL is deliberately retained; an evidence-integrity PASS is not a build PASS. New-authored whitespace check excludes opaque archives. New files sanitized and each under5MB.

Branch-scoped workflow verifies stored evidence/provenance, not a cloud Files build or machine dependency fix. Existing real repository regression CI remains unchanged; no manual local native/C# regression repetition. Primary reviews all applicable CI/diff, merges evidence only after green and checks main post-CI. No production/test/L1/pack/schema/public surface/LOCKED or earlier-stage change, no F2/F5 integration/closure. Overall project is incomplete.

Next: primary review this failure evidence; then separately decide the narrowly scoped packages.config restore candidate. Do not execute new flags while this stage is under review.
