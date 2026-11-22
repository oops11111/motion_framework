# F2 x64 build continuation

PASS: authorized solution restore/build, complete output public API comparison, source and frozen-surface checks. Runtime remains NOT EXECUTED.

The user's F2-only Platform=x64 approval is recorded verbatim in AUTHORIZATION_PLATFORM_X64.json. This route is NON-ORIGINAL and uses exactly the two approved properties: WarningsNotAsErrors=NU1903 and Platform=x64. Machine SDK 10.0.300 and PATH/DOTNET_ROOT remain unchanged; only missing child PROCESSOR_ARCHITECTURE is set to AMD64.

Restore PID 22156 exited 0; solution Debug build PID 60936 exited 0, with two NU1903 warnings and no errors. Actual output is x64 / win-x64, net10.0-windows10.0.26100.0. No source change was needed. Previous original restore and ARM64 build failures remain preserved.

Actual FluentHub.dll SHA256: 428fe0ba893c45df426da289685cc687caf2e5e216d9a2e4506e38d7eeab4981. Manifest SHA256: fe5dc89b96997208936d7ad3263254a047ef604c696d1ce8be40b5a4bdd243f4, identity 49462fluenthub-uwp.FluentHub / 0.11.0.0 / x64, original Publisher unchanged. Runtime configuration has EventSource.IsSupported=true, JSON reflection=false and DynamicCodeSupported=false; this is not a NativeAOT publish claim.

All three output pack hashes match the frozen reference. Existing reflection-only SurfaceDump ran against the actual output FluidMotion.L1.dll: CheckSurface fails=0 and the complete dump equals the canonical baseline byte-for-byte (SHA256 7b67b77bce6b36dc58cc870a17fe1b5588df3c6e526b39ca5a3e36d5280bc5cc). No motion functionality/native callback/app was invoked.

All ten host source hashes still match v2. Twelve frozen source/metadata/API blobs match the main base. Existing 18 private repair tests and 20 F5 source tests were not repeated because no source logic changed.

Raw sanitized output, original captured byte hashes, actual argv/PIDs/exits, artifacts, runtime configuration, full surface/checksurface and freeze/source receipts are in raw/20261122T093319AWST-x64/. Original captures remain in the ignored attempt directory.

Stage 3 preparation may now proceed for review. Registration, ordinary activation, UIA and real/Fake lifecycle acceptance remain separate, unexecuted gates and require the specific package authorization after concrete script review.
