# P1 — formal internal component candidate

Base: MF main11b3378fb20498675165f0b52275aa8d23ec2ccd after approved P0 PR24; branch codex/everystage-component. Acceptance source is the current user's formal EveryStage internal-delivery request and primary's P1 narrow task, not any Z6 waiver.

## Scope and deliverables

New src/FluidMotion.L1 project links the unchanged MotionSystem.cs and MotionEtw.cs; PackageId/assembly FluidMotion.L1, namespace MotionSoftDemo, net8.0, managed AnyCPU with only a win-x64 native asset. Version0.1.0 is candidate/unsealed/unpublished. New tools/delivery entry/guard and tests/delivery independent consumer, this P1 record and branch CI are the only implementation areas. Old pilot, product/public/native/E1/composition source and all historical records remain untouched.

One `python tools/delivery/build_component.py [--output <new-directory>]` entry builds native Release /O2 /DNDEBUG /MT using existing MSVC, validates PE x64/imports, packs standard runtimes/win-x64/native plus managed/token/README/provenance, and returns a JSON manifest. A missing native is never optional. No installation, publication, global DLL/cache or old F7 binary dependency.

Static CRT is an explicit new formal-build configuration, not a source/API edit. Actual dumpbin /dependents and /headers are preserved; dependency acceptance rejects VC/UCRT runtime imports and unreviewed DLLs. This checks the built asset rather than assuming self-contained .NET includes VC runtime. The new native hash is recorded, not compared to historical compiler output hashes.

## Acceptance and dependency order

1. Validate source boundary/pin and candidate provenance; dirty local outputs retain base SHA+dirty/input hashes. CI uses exact committed PR head with full Git history; the future EveryStage checkout must pin the accepted merged P1 commit and also fetch-depth0.
2. Build/pack once into a unique non-existing directory. Manifest maps package members/SHA, source commit and input inventory, SDK/compiler/native configuration. The package includes no invented public license; its internal use statement does not purport to grant unrelated third-party rights.
3. Run actual dotnet pack/MSBuild gates for missing native, unsupported RID and a copied native fixture with incorrect PE machine; require the specific failure messages. No production native bytes are edited.
4. In a second temporary directory/job-local cache, restore exact `[0.1.0]` through source mapping/local feed, build/run a package-only consumer. Assert actual create/play/target64/rest0/release/dispose/pump teardown and loaded native/managed output identity. Compare native/managed/token and restored nupkg bytes to the same newly built package.
5. Run unchanged CheckSurface code/full dump against actual consumer managed output. Compare complete normalized bytes with pilot/fluenthub/surface_l1pack.txt, preserve raw and normalized hashes. Then existing native/managed Debug and Release regressions must pass.
6. Ordinary commit/push/PRmain. Primary reviews actual diff, all applicable CI, merge and main post-CI. No candidate is called a sealed final release before P3/P4; final EveryStage consumes the new source/entry rather than a historical cached DLL.

Commands are the entry above, plus `python z6/integration/tests/run_tests.py --configuration Debug` and `--configuration Release`. There are no added audit/WNAE/toolset/TFM waivers or new tools installed. Existing source tests/workflows are retained; new component CI is additive and branch/path bounded.

Stop on frozen/public/source mismatch, missing/wrong native asset, dependency/import surprises, exact local-feed identity failure, actual API failure or non-green checks. Preserve failures/logs; no workaround changes to frozen source, dependency upgrade, system install or unapproved fallback.

## Current evidence handling and limits

Actual build/test output lives under unique ignored m0/out/component-* and z6-integration-tests-* directories; P1 evidence records identify attempts, raw hashes and sanitized published copies. No old attempt is overwritten. A first candidate may be dirty; source-clean CI rebuilt artifacts provide committed provenance without a loop of committing generated binaries.

First candidate completed package/three negative gates/14 actual consumer checks/CheckSurface0 and full raw surface equality. Its standalone metadata query `cl /Bv` without a source emitted D8003 while the real compile passed. That original query log is retained; the final tool instead puts /Bv on the actual compile, with failures never ignored. This does not change native source or conceal an unsuccessful compiler invocation.

No EveryStage business code or installer was changed/executed. Subsequent P2 must account for newly observed existing ProgramData/Program Files EveryStage directories: copying the real application EXE would not isolate its hardcoded data/network/instance behavior. Real application/installer verification should use disposable CI; any local isolated-control harness must be labeled as a harness, never ordinary full-application validation. No attempt to alter all application data paths is included here.

## Local results (no repeated GUI or installation)

Final component attempt `component-20261122T070504Z-0fec0dba`: PASS candidate; native/pack/restore/consumer/surface commands all0, three expected MSBuild negatives each nonzero with its specific missing/RID/machine reason. The independent consumer completed15 public/native identity and lifecycle checks; managed/native/token outputs and restored nupkg match package members/bytes. PE isAMD64 DLL; actual imports onlyKERNEL32.dll; /MT does not require an omitted VC/UCRT DLL. Input hashes and source HEAD remained unchanged during that attempt.

Package SHA256 `c972242cb05f608bff34684129a116b38a3d1f090fba16599eb5962219c8e4cf`; nativeSHA `5868f372eeaa46cf737e4116f67395f19fdfbb060fc82e4a858e02aec4327a8e`; managedSHA `d53f27d3bdc0ca8bf54f0bc64130c28735d25a9f9aaaa8a7b3155d16617f1e1d`; tokenSHA `8d5db5775acad5db0c04cbc3c2a214bb7309670c304b71ff40860757973c410e`. Raw and LF-normalized full API dumps both equal frozen baselineSHA `7b67b77bce6b36dc58cc870a17fe1b5588df3c6e526b39ca5a3e36d5280bc5cc`, CheckSurface0.

Existing Debug attempt `077acf41` and Release `0ab6f52a` each passed all six native/managed/surface commands, native45/0 and full devsmoke verdictPASS/API. Run once per configuration, not repeated after success. No event payload listener was added to the new consumer; no FrameTick/PresentStats fields were read or performance claim made.

[P1 evidence receipts](P1_EVIDENCE/RECEIPTS.json) map exact ignored originals to separately redacted UTF8 publication, including both component candidates, the initial metadata query limitation, three actual negative failures, final consumer/surface, and D/R logs. Decoding replacement counts are explicit; published files are not described as raw-byte originals. No nupkg/native binary is committed. Original candidates/logs remain ignored and intact.

Local candidates accurately bind base11b3378 plusdirty=true/input hashes. They do not claim the final submitted implementation commit. New exact-head clean CI will rebuild a candidate bound to the real implementation SHA; actual cross-repo application consumption uses the subsequently accepted merged commit. CI rebuilt native/package hashes may differ and are recorded separately.

Candidate/unsealed metadata describes this build's state. In P3/P4 an external internal-delivery manifest may explicitly attest/seal the same exact bytes with provenance and hash; that later state relationship must be documented instead of pretending the build-time candidate field was silently changed. No sealing/publication implementation is added in P1.
