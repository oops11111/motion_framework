# F9a stage3B original Files build continuation

Fresh original upstream CI command route: PASS (build only). Primary review and repository CI/main merge are pending. This does not adjudicate or rewrite the original F9a FAIL, nor demonstrate package activation, GUI/runtime behavior or AOT-ETW compatibility. Overall project remains incomplete; F9b is not open.

Base main3078ae0e9f8792a03f45e09dd4fce5c6fda1ca7e, branch codex/z6-f9a-baseline, target main. PLAN commit02a00fabc37912acf94cd142a5ceadfe589e4553 was reviewed before execution; EXECUTION.md records primary approval and stage3A post-CI44887910560 success. All3A files/manifests/workflow and all original baseline/evidence remain unchanged. Base ledger is archived exact; aggregate ledger retains the whole prefix and appends current facts.

## Attempts and actual results

- 20261121T090913Z-3a2a9200-run1: STOP before checkout/restore/build. Local Git clone PID79260 exited128 with Git sh.exe fatal signal-pipe Win32 error5 under execution sandbox. Complete original log/result retained. This was not a failed Files compile. No source or dependency change followed.
- 20261121T090938Z-a08ae53c-run1: new isolated no-hardlink local clone after removing the process/filesystem execution sandbox, without OS elevation or build-property changes. Clone82356 and checkout73528 exit0. Detached Files HEADcbfb4ba54a89632fbceb82ea94ada60a9bae3737; exact source tree/key-file receipts and full clean PRE/POST are identical. Original %USERPROFILE%/pilot/Files was read only.
- Existing user dotnet --version returned10.0.404. Only build-child DOTNET_ROOT/PATH-prefix plus missing AMD64 restoration changed; no persistent environment or compiler override. Toolchain file versions/hashes are independently recorded, including SDK Roslyn5.9 and VS bundled Roslyn5.6; file presence does NOT establish which compiler assembly was loaded.
- Exact upstream CI Restore PID32600: exit0 at09:10:12Z. Exact Build PID87316: exit0 at09:14:13Z. Full argv, start/endUTC, actual exits, original/published hashes and source receipts are in the successful attempt's result.json and source_pre/source_post.json. Quiet output contains87 warning occurrences (CS0067=5, CS0612=6, CS0618=75, CsWinRT1034=1), no error-ID lines; raw stdout/stderr and process exits are authoritative. No warnings were suppressed.
- Run2 was NOT run because run1 passed. No same-command retries after build; the pre-build sandbox failure remains separate.

Original commands, with paths only substituted:

```text
MSBuild.exe Files.slnx -t:Restore -p:Platform=x64 -p:Configuration=Debug -p:PublishReadyToRun=true -v:quiet
MSBuild.exe src/Files.App/Files.App.csproj -t:Build -p:Configuration=Debug -p:Platform=x64 -p:AppxBundle=Never -p:GenerateAppxPackageOnBuild=false -v:quiet
```

## Generated-artifact binding and scope

Hash-only receipts in artifacts.json bind output from the successful clone (binaries remain ignored):

| Output | Bytes | SHA256 |
| --- | ---: | --- |
| Files.dll |6747136|dd4591739f29f6029860206e0da84ff628d5e94a499e9f2e270121d8621d0f79|
| Files.exe |587776|e5e8765c4fb1a69429c919b508e99b5e49773eb173b91cee58a2a8e3bd3d30b3|
| AppxManifest.xml |22767|ad4ee744e9e3b82875b656f3da15754003d673987b978c7c411af4d7c7c890b4|

These files were not executed, registered, published or deployed. Build0 does not prove NativeAOT publish or E1/runtime compatibility. Source pin, original source tree and project/dependency/profile/TFM/toolset remain unchanged. No Files source or generated binary is added to this repository. No package/certificate/task/install/restart/system-animation action; no F2/F5 main integration or F9b. Stage3A DCAT sync error/NGEN warning remain disclosed in unchanged3A evidence; no repair was attempted.

Complete original clone/build outputs remain in each ignored m0/out/f9a-baseline-<attempt> directory; new raw logs are explicitly sanitized complete text (or would be labeled bounded excerpts if large), with original byte count/SHA and decoded encoding. New files are each under5MB. Original and sanitized hashes differ by documented path/newline redaction, not by error removal.

`python z6/f9a/baseline-codex/verify.py` validates original argv, child SDK selection, attempt status/exits, source PRE/POST equality, artifact-receipt bindings, preserved ledger prefix/scope and sealed stage manifest. New-authored whitespace check excludes immutable old ledger archives; no whole-history whitespace cleanup. CI runs this branch-scoped integrity gate plus the unchanged real native/C# regression matrix. Cloud CI does not independently build Files or access workstation outputs; the new local original command result is a separate gate. No local repository regression was repeated for these evidence-only changes.

Next: primary reviews raw/results/diff and all applicable head CI, merges only after green, then verifies main postmerge CI. Any application activation/deployment or next F9b work needs its own explicit scope/authorization; current build success does not grant it.
