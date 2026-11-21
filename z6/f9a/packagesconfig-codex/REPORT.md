# F9a stage3C3: authorized NON-ORIGINAL packages.config restore/build

Local result: PASS (NON-ORIGINAL build only). User approved exactly one additional property; this does not convert prior original full-solution FAIL into PASS. Earlier3B original app-build PASS,3C2 original failure and all prior evidence remain immutable. Startup/deployment/runtime are NOT TESTED; F9a is not complete and F9b is not open. Primary CI/diff/main merge review is pending.

Base main81543c86fc67801aa2de00f44d8b267c2ac09ee9; branch codex/z6-f9a-packagesconfig. PLAN7169c2e68f39aa38baf48c999839fde26bf25914/AUTHORIZATION.json were reviewed before explicit primary execution instruction. Verbatim direct user grant and recording time/source are preserved, with no fabricated message/send time. PR17 original failure evidence/postmain44891508566 were accepted first. No prior-stage source/manifests/workflows changed; base ledger exact archive/full-prefix append preserves the sequence.

## Single authorized command and result

Fresh no-hardlink local clone of Files cbfb4ba54a89632fbceb82ea94ada60a9bae3737, detached/clean; no old binaries reused. Clone81960/checkout77856 exited0, existing user dotnet --version10.0.404 exited0. Only child DOTNET_ROOT/PATH-prefix and missing AMD64 restoration changed. No source/TFM/toolset/compiler/dependency-version override, no persistent environment change.

```text
MSBuild.exe -restore Files.slnx -p:Configuration=Debug -p:Platform=x64 -v:quiet -clp:ErrorsOnly -p:RestorePackagesConfig=true
```

The existing VS18Community MSBuild/Current/Bin/amd64 executable ran this once, PID83272,10:30:47Z to10:36:10Z, exit0. Entire route is NON-ORIGINAL. No separate restore, manual package copy, import fix, extra property/target, warning/audit override or retry followed. ErrorsOnly is the upstream logging parameter; the short error-only log is not proof of zero warnings. Tool file versions/hashes are recorded without inferring which compiler assembly was loaded.

## Exact declared-version restore proof

All tracked packages.config inventory still has only src/Files.App.Launcher/packages.config, unchanged Gitblobf3255c9617199ec636a01fcc01d93269be1b1f7f and SHA4e8ba66b03f978177ae68a0b6fd8758c3a517acee6b67cb7193afd0c4a9b56d5. Full sourcePRE==POST/HEAD/tree/key receipts/statusclean are identical.

Read-only post-build .nupkg-contained nuspec inspection confirms:

| Package | Declared and actual version | NUPKG SHA256 |
| --- | --- | --- |
|Microsoft.Windows.CppWinRT|3.0.260818.1|a8993608ae9263a8288e1a1d9ee2684a5632f4acc59e1e89ddd86b3b889a7367|
|Microsoft.Windows.ImplementationLibrary|1.0.260126.7|7d67bc71dd0edc342db44ea0334ac9b6a1776514f81694b0b5b3c7ac9d608e42|

restored_package_receipts.json records actual nuspec ID/version/contentSHA and archive SHA, unchanged config identity and three real import file hashes: CppWinRT.props3306e33..., WIL.targets1628336..., CppWinRT.targets24c007e.... All imports exist at their declared version paths. NuGet's normal fresh-clone ignored packages/cache writes were authorized; no package upgrade or source repair occurred. No SDK/ATL/VS/runtime installation or credential/account change.

## Generated outputs and deployment boundary

Hash-only receipts bind this build; binaries stay ignored and were not activated/registered:

- Files.dll6747136bytes, SHAc4dcb8a473ff1a9a5680efa33e1a6b7fac7309c1fce6098d967449d2ec2fd873.
- Files.exe587776bytes, SHAe5e8765c4fb1a69429c919b508e99b5e49773eb173b91cee58a2a8e3bd3d30b3.
- AppxManifest.xml22767bytes, SHAad4ee744e9e3b82875b656f3da15754003d673987b978c7c411af4d7c7c890b4.
- Recipe440056bytes, SHA5d5310419232afa868f3f453603cce472d28ec07d411071f52b7dc10d7de565e.
- Server.exe exists separately, same SHAc39f5fd212e91de45cc4887b2e90ad4cde68bf44fd975228fc261b9a865a9c05. Launcher plus Open/Save32/64 helpers now exist with hashes recorded.

The new recipe has1308 mapped files and the five native helper mappings. It still does not map a Files.App.Server/ subtree; server path beneath current appbin is absent, and AppX/intermediate PackageLayout remain absent. These are current output observations, not proof that normal VS deployment fails or an upstream bug. No manual layout, deployment/registration, signing/certificate creation/import, app/test execution, task/UAC/system-animation change, F2/F5 main integration or F9b action. Build success is not NativeAOT publish, E1/runtime or startup acceptance.

## Evidence and CI

Complete original logs remain in ignored m0/out/f9a-packagesconfig-20261121T103038Z-444beec5-packagesconfig; published logs are complete sanitized text with original/published hashes, exact argv/env/PID/UTC/exit and encoding receipts. Config/source/nuspec/import/artifact metadata were read only after natural exit. New files each under5MB, workstation paths replaced with placeholders, no third-party source/binaries committed.

`python z6/f9a/packagesconfig-codex/verify.py` validates verbatim authority, one-property delta/NON-ORIGINAL label, source/config/version/import receipts, exact exit0, own manifest and bounded diff/base ledger prefix. Its CI validates stored evidence, not an independent cloud Files build or cache inspection. Existing actual repository regression CI remains unchanged; no manual local native/C# repetition for evidence publication. Fixed stage policy is branch-scoped so future legitimate stages are not constrained.

Next: primary reviews actual local package/artifact receipts, all applicable CI and diff before ordinary main merge/post-CI. Any next verified deployment layout or ordinary startup requires a separate reviewed Plan and direct package permission; this single-property grant does not authorize it. Overall project remains incomplete.
