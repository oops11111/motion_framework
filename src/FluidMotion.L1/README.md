# FluidMotion.L1 0.1.0 — internal candidate

.NET 8 managed API with a Windows x64 native asset. This is the new formal component project; historical pilot projects and evidence remain unchanged. Package/assembly name is FluidMotion.L1 and public namespace remains MotionSoftDemo.

Build through `python tools/delivery/build_component.py`. It builds native Release with static MSVC CRT (/MT), checks PE/imports, packs once, and verifies an independent local-feed consumer. Never substitute historical cache DLLs. `provenance.json` records source/toolchain/native hashes; the external delivery manifest additionally hashes the final nupkg.

Consume this exact candidate version from the generated local feed with package source mapping and an isolated NuGet cache. The application must target win-x64; the native DLL is under runtimes/win-x64/native. The included demo_motion.json is sample vocabulary, not a new product animation requirement.

Create/use/dispose MotionSystem on its owning UI thread. Bind the host frame source outside callbacks and call OnFrame while requested. Native sink/render callbacks must not reenter motion APIs; queue work instead. Dispose animation handles and detach UI sources on owner shutdown. Release ignores the Debug-only FM_FLUIDMOTION_PATH override.

Internal delivery only, candidate not sealed/published; no public license is invented here. Existing source rights and any applicable dependency notices remain authoritative. No third-party managed PackageReference is introduced by this component. It does not bundle a .NET runtime; the application's deployment supplies .NET 8. Static CRT imports are verified against the actual built DLL, not presumed from the command.

No public NuGet/GitHub Release, signing, performance/adoption or API expansion is implied. PERF-CI remains HOLD. Existing MotionEtwMirror is a frozen API; do not use it to read restricted FrameTick/PresentStats payloads.
