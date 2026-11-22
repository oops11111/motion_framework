# F5 continuation after specific build authorization

Recorded 2026-11-21 09:55:26 AWST (recording time, not asserted message send time).

User's direct response in the current root conversation, forwarded as the current-stage task to the continuing implementation agent:

> F5 批准仅本票使用 -p:WarningsNotAsErrors=NU1903，标记非原样

Question item identifier: `["request_user_input_async","call_fe4ebc0a949b4b1d860b2214b3898e0b",0]`.

This authorizes only F5's named warning downgrade. It does not authorize fallback flags, dependency upgrades, package registration, scheduled tasks, system settings changes or installation. A new attempt uses:

```
dotnet build src/FluentHub/FluentHub.csproj -c Debug -p:Platform=x64 -p:WarningsNotAsErrors=NU1903
```

This is a **non-original build**. Previous original-build failure and evidence remain intact.

Clarification to the first report: initial SetReducedMotion is applied synchronously by the **NavMotionPilot constructor**, on its UI thread before it creates/plays enter handles. MainWindow constructs and owns the shared source. The first report's shorter description must not be read as calling SetReducedMotion from MainWindow itself.

Initial read-only Windows PowerShell 5 query `Get-AppxPackage '*fluenthub*'` returned no package. No package was registered or removed. GUI gates B/C/E remain pending until actual host execution evidence exists. Green source CI alone does not accept those gates.

## Results recorded 2026-11-21 10:02 AWST

- Attempt 4, with the specifically approved warning downgrade, produced FluentHub.dll after C# compilation and XAML generation; the **overall build failed, exit 1**, during `WinAppSdkGenerateAppxPackageRecipe`, APPX0002/MSB4018, `MrmSupportLibrary.GetLocation` NullReferenceException. No new source compilation errors were emitted. This is not a build PASS. `raw/build_attempt4_nonoriginal.log` retains the full sanitized output.
- Attempt 5 used the same approved command after entering the already installed VS 2022 vcvars64 environment. It again produced the application assembly, then failed with the same recipe exception, exit 1. No additional MSBuild property or dependency change was used. `raw/build_attempt5_vcvars_nonoriginal.log` is separate evidence.
- A direct execution probe of the new exe, with Fake selected only in the child environment, exited `0xe0434352`. Its stderr identifies COMException `0x80040154 (REGDB_E_CLASSNOTREG)` while activating WindowsAppRuntime DeploymentInitializeOptions, before application startup. No owned GUI remained running. The helper's console printer subsequently hit a GBK/UTF-8 display encoding exception; the original stderr bytes and child exit had already been captured. The delivered stderr is decoded from GBK to UTF-8 and explicitly labeled. This probe is not B/C/E evidence.
- The built manifest requires Microsoft.WindowsAppRuntime.2 >=2.0.1.0. Read-only package enumeration finds 2.5.1.0, 2.6.5.0 and 2.7.0.0 installed, including x64 packages. The failure does not establish that runtime installation is missing, nor that registering FluentHub alone would solve activation. No runtime or package changes were attempted.
- Artifact runtimeconfig has `System.Diagnostics.Tracing.EventSource.IsSupported=true`. Artifact SHA256 values and the failed overall build exit are recorded together in `raw/host_artifact_attempt4.json`; successful compilation is not conflated with successful build or launch.
- Existing SDK file `%ProgramFiles(x86)%/Windows Kits/10/bin/10.0.26100.0/x64/MrmSupport.dll` was verified to exist and have PE machine 0x8664 (x64). Both registry views point KitsRoot10 at the existing Windows Kits installation. The installed MSIX target explicitly consumes the `MrmSupportLibraryPath` property in its recipe task. `raw/mrm_support_existing_sdk.json` records hash and file size. A minimal **unverified candidate** is binding that existing file with `-p:MrmSupportLibraryPath=...`; it has not been executed because it requires a new specific authorization. No SDK install is proposed from this evidence.
- `verify.py` now checks SHA256SUMS hashes, duplicate/path constraints and exact coverage of delivery files. It continues to claim patch syntax and actual extracted state/Fake tests only; CI does not verify the pre-existing FluentHub file bodies or GUI. The local original-sidefork PRE comparison and patch roundtrip remain separate recorded evidence.
- Previous tip `ccefd895fe7812a22303a6d229d051da8e2af126` source CI run `44852713507` completed successfully, as inspected by the primary agent. Updated commit CI must be checked separately.

Current acceptance: frozen surface and source behavior remain verified; host overall build and launch are blocked; required B/C/E remain pending; optional D is not authorized. PR #10 stays Draft and must not merge. The next action is primary-agent review of these preserved results and a specific decision about the existing-SDK property binding. Do not add that flag, register packages or change system settings without authorization.
