# Internal component build interface

Prerequisites: Windows x64, existing MSVC x64 tools discoverable by vswhere, .NET SDK capable of net8.0, Python 3 and Git. No tool is installed by this entry. Both this repository and any future EveryStage `external/motion_framework` checkout must use **fetch-depth: 0**: the entry compares frozen sources to accepted base `11b3378fb20498675165f0b52275aa8d23ec2ccd`.

```powershell
python tools/delivery/build_component.py
# Or select a NEW output directory; existing directories are rejected, never deleted.
python tools/delivery/build_component.py --output <new-isolated-directory>
```

The final stdout line is `RESULT_MANIFEST=<absolute-path-to-result.json>`. Nonzero exit means a failed command/verification; original logs and the terminal JSON are preserved. Inspect `status`, `commands`, `package.path/sha256`, `source_commit/source_dirty`, input hashes, native imports/CRT, consumer output identities and full surface comparison. Input hashes are a provenance inventory, not a claim that every listed file is a compiler dependency.

The entry builds Release native using /MT, inspects PE and dumpbin imports, packs a new local feed, executes actual missing-native/wrong-RID/wrong-machine MSBuild failures with reason assertions, then creates a second isolated consumer/cache. The consumer uses exact `[0.1.0]`; source mapping allows FluidMotion.L1 only from this local feed. Microsoft framework acquisition may use the official NuGet source; no component fallback to a public source is allowed. Native, managed and token consumer-output hashes must equal nupkg members, and the restored nupkg bytes must equal the newly built feed file. Actual public API/native behavior plus module/assembly identity are exercised without event listeners or GUI.

The unchanged existing SurfaceDump/CheckSurface code is compiled as a net8.0 inspection tool, then the complete surface is compared after CRLF-to-LF normalization; raw and normalized hashes are both recorded. Existing public baseline/pilot files are not edited. No timing/performance acceptance is performed; the consumer's watchdog is only a hung-test stop.

`0.1.0` is an **internal candidate**, not a sealed or published release. Dirty local builds identify their base commit plus `source_dirty=true` and input hashes. The CI checkout is the exact PR head (or pushed commit), with a clean tree, so its rebuilt candidate binds the commit that actually contains this new project and scripts. Future EveryStage must checkout the accepted merged P1 commit, not a floating branch or this earlier base. No binary byte identity across separate native builds is assumed. The application's pipeline should build once and consume/deliver that same nupkg; final bytes/version are sealed only in later P3/P4.

No installer, global native DLL, FmL1Root, old F7 SHA, public feed/release, signing, system settings, extra waiver or EveryStage code is used. The component's public API/namespace and production source remain frozen. See docs/delivery/everystage/P1_COMPONENT.md for scope and candidate evidence.
