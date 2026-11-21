# F5 build environment root cause and remaining GUI gate

Recorded 2026-11-21 11:34 AWST. This adds current evidence; previous failed attempts remain unchanged. The F5 stage is still not accepted.

## Build repaired without another MSBuild property

Static PE/IL inspection of the already cached MSIX task found that `MrmSupportLibrary.GetLocation` calls `Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE")` at IL_002E and immediately invokes `ToLowerInvariant` at IL_0033 without a null guard. Both the net6.0 task used by `dotnet build` and the net472 task used by full-framework MSBuild contain this sequence. No third-party task code was loaded or executed for this inspection.

The local diagnostic process reports:

- ProcessArchitecture and OSArchitecture: X64.
- Process `PROCESSOR_ARCHITECTURE`: null.
- Existing machine `PROCESSOR_ARCHITECTURE`: AMD64 (read-only query).
- The existing VS2022 vcvars64 child environment also leaves that process variable null.

This explains why the vcvars-only attempt still failed. The task accepts a fallback MrmSupportLibraryPath argument but, when it finds an SDK, recomputes the library path using the environment variable. Passing a fallback property is therefore not established as a bypass for this null dereference. It was never tried.

Relevant target inspection: Packaging.targets selects net6.0 for MSBuildRuntimeType Core and net472 otherwise, then passes `MrmSupportLibraryPath` to the recipe task. Pri.targets adds `_GetMrmSupportLibraryPath` to its PRI generation dependency list; MrtCore.PriGen.targets has a separate `_GetMrtCoreToolsPath` path. It was not necessary to prove the fallback property was empty: the task recomputes the SDK path regardless of the fallback whenever a platform SDK is found. Changing task runtime, installing an SDK or adding that property was unnecessary for the observed build failure.

## Attempt 6: overall build PASS (dev-only), non-original

A **new child process only** copied the existing machine value `AMD64` into its process `PROCESSOR_ARCHITECTURE`. It ran the same already approved command in the isolated side fork:

```
dotnet build src/FluentHub/FluentHub.csproj -c Debug -p:Platform=x64 -p:WarningsNotAsErrors=NU1903
```

Result: **exit 0**, two NU1903 warnings, zero errors, pack hash verification OK, complete build including recipe generation. This is still **non-original** because it uses the user's F5-specific warning downgrade. No MrmSupportLibraryPath flag, fallback flag, dependency change, SDK install, package operation, registry write or persistent environment write occurred. The parent process still lacks the variable afterward, confirming that the environment repair was not persisted.

Evidence: `raw/build_attempt6_command.json`, `raw/build_attempt6_env_nonoriginal.log`, both environment probes, post-child environment check, and task IL extracts. Attempt 4/5 failures remain valid historical evidence, not replaced by this success. This is an observed property of this local execution session, not a claim that all Windows/MSIX environments lack this variable.

## New direct-launch attempt: GUI remains blocked

After the successful build, one new direct-exe diagnostic used the same standard child environment plus `FM_PILOT_F5=1`. It exited `0xe0434352`, with the same COMException `0x80040154 (REGDB_E_CLASSNOTREG)` during WindowsAppRuntime DeploymentInitializeOptions activation, before application startup. No GUI gate was exercised and no owned process remained. Direct-exe retries stopped here. Both the exe hash and stderr are preserved in new evidence files.

The generated manifest identifies `49462fluenthub-uwp.FluentHub`, version 0.11.0.0, x64, with the existing publisher and Microsoft.WindowsAppRuntime.2 >=2.0.1.0 dependency. The launch profile is `MsixPackage`, and no FluentHub package is currently registered. Compatible runtime framework versions already exist; these observations do not prove that installing a runtime is needed, nor guarantee that package registration alone resolves activation.

The next smallest testable step requires specific user authorization: register **only** the isolated output's generated AppxManifest.xml, launch and collect B/C/E without system animation changes, then remove only that newly registered FluentHub package. This is a proposed package-graph experiment, not a guaranteed fix. No scheduled task, certificate, developer-mode change, runtime installation, system setting change or additional build property is included. The implementation agent has not performed it.

Current gates: A frozen surface verified; source behavior previously passed 20 checks; host build now passes (dev-only, non-original); B/C/E remain pending; D remains unauthorized. Draft PR #10 must not merge until required gates and its latest-tip CI all pass.

## Reproducing the read-only inspection

`diagnostics/ReadTaskIl.cs` is an original, dependency-free static reader using the installed SDK's Reflection.Metadata and PEReader. To avoid generating build output inside the evidence tree, copy it to an ignored scratch directory such as `m0/out/f5-il-reader/Program.cs` and create a minimal net8.0 console csproj with ImplicitUsings and Nullable enabled there. Run it with the cached task DLL path and filter `MrmSupportLibrary`; `--environment` instead prints only the six relevant architecture fields. It never invokes the inspected assembly. The delivered task IL is limited to the relevant GetLocation method, with assembly hashes, rather than an unrelated decompilation dump.
