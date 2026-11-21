# Z6 stage 2A — independent deterministic test integration

Recorded 2026-11-21 15:09 AWST, current Codex conversation. Scope reviewed and approved by the primary agent before implementation. Status: implementing; original ticket verdicts are not re-adjudicated. This stage produces fresh native/C# integration acceptance, not GUI or timing acceptance.

## Authority and corrected dependencies

Source of acceptance: `coord/z6-codex-handover` at `9c28dcc6a97bda7d552ddf2a6cc6f12347063751`, especially `docs/z6_coord/z6_handover_adjudication_notion.md` section 3 N9 and handover sections 2/4/5.3. N9 explicitly rejects making external review a universal main-merge prerequisite; the user decides with disclosure of whether review happened and its result. The earlier F5 PLAN's blanket Notion dependency for all integration was too broad. This independent stage can proceed under the user's current delivery authorization and primary-agent review. F2 verdict changes and `docs/z6_closed.md` still wait for the relevant outstanding decisions; this stage does neither.

Source: user's initial current-conversation instruction, verbatim:

> 使用 $continuous-project-delivery。
>
> 目标：后续开发测试完全由你接手
>
> 要求：
> - 对齐最新提交，先 Plan，再实施，不要跳过阶段拆解；
> - 每阶段说明依赖、范围、交付物、测试和验收标准；
> - 一个持续子代理负责实现、测试、提交和开 PR；
> - 主代理负责审查 diff、跟踪 CI、处理失败和合并；
> - CI 未全部通过前不要合并；
> - 每阶段完成后汇报提交、PR、验收证据和下一步；
> - 遇到凭据、真机或外部服务阻塞时报告证据，不扩大范围。

The user later pointed back to the coordination branch as the authoritative handover. The primary agent reviewed this stage's source whitelist and verification plan. Ordinary local commits, push and PR are covered; merge is the primary agent's responsibility only after its review and all CI gates pass. This is not new permission for system operations, package registration, installation, non-original warning/audit/toolset flags, or F9b. The F5-only NU1903 permission is not reused.

## Exact stage and source whitelist

- Branch: `codex/z6-integration-tests`.
- Base/main freshly checked before branch creation: `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`.
- Merge target: **main**, through one independently reviewable PR. No automatic/self merge.
- Ordinary source merge order, preserving each source tip as an ancestor:

| Ticket | Exact tip | Scope brought into this stage |
| --- | --- | --- |
| F0 | `45cbe21b96ac917c28635065b01206210aace738` | Existing post-merge test/GUI evidence only, `z6/f0/**` and source ledger |
| F3 | `5a3cef559e826a8450a252add7efd8659125a0e4` | Independent 12-check C ABI empty-animation/Play/Retarget case in `code/capi/fm_capi_test.cpp`, original evidence |
| F1 | `5ec13f459a7926012407b10cf263127e5a74ab73` | Ten native overlap checks and managed deterministic overlap block, original A/B/C evidence and host patch |

All three source branches start directly at the pinned main and do not contain F2/F5 ancestry. Fresh read-only three-input merge-tree inspection found the F1/F3 native test hunks merge without conflict; the only conflict is the add/add shared `z6/LEDGER_snapshot.md`. Native expected integrated total is **45 checks = 23 baseline + 12 F3 + 10 F1**.

Production/schema/LOCKED diff is empty on each source. Two existing **test** source files may change: `code/capi/fm_capi_test.cpp` and `MotionSoftDemo/Z3aL1DevSmoke.cs`. Additional new files are limited to this stage's plan/tools/evidence/ledger archives and `.github/workflows/z6-integration-tests.yml`.

Do not merge F2, F5 integration ancestry, F9a's failed baseline, or unopened F9b. F12/F4/F8a/F8b documentation and F7→F11→F6 pack/sidefork evidence belong to later independent stages; F8 stays DRAFT. No production source, public L1 surface, composition.h, fm_capi.h/.cpp, MotionEtw.cs/schema, LOCKED file or closure document changes.

## Evidence and ledger merge resolution

Before resolving any shared-ledger conflict, store each exact source Git blob under `source_ledgers/<ticket>-<tip>.md`, recording Git blob and SHA256. Attributes mark these archives `-text` to prevent newline/encoding rewriting. Existing raw ticket evidence remains byte-for-byte as delivered; historical payload-bearing F0 evidence is retained and never parsed for timing claims.

The aggregate ledger preserves the F0 blob as its unchanged prefix and appends ASCII stage records and links to exact F1/F3 snapshots. It does not replace old rows, invent authorization, alter PASS/FAIL wording or delete the sole evidence copy. Record F5's already accepted separate integration merge `15d4108fd56eb661c1a46c6552ffe74a0318ff28` as a reference only, not an ancestor imported into main.

## Necessary test-only logging fixture repair

The current devsmoke instantiates frozen `MotionEtwMirror`, whose payload join reads every field. New runs must not read/log FrameTick or PresentStats payload values. Only within `Z3aL1DevSmoke.cs`, replace this test's listener with a private count-only EventListener:

- Count EventId/Name, mapping id 103/FrameTickAccept to FrameTick.
- Return for FrameTick and PresentStats **before any Payload getter, Count or enumeration**.
- Other events log names/counts only; only IdleTransition may read its non-timing payload to preserve the existing Dispose assertion unchanged.
- Do not remove/change any test's pass condition or touch MotionEtw.cs, E1 definitions or public members. Private fixture members must not alter public dumps.

## Verification and commands

Use the already installed x64 MSVC vcvars environment and .NET SDKs. Generated outputs use a fresh ignored `m0/out/z6-integration-tests-<attempt>` directory. A standard missing PROCESSOR_ARCHITECTURE may be repaired only in the test child environment from the existing machine AMD64 value; no setx or User/Machine environment writes. Clear only test-child FM_FLUIDMOTION_PATH/MF_Z3A_L1_DEV/MF_Z4_L1_SESSION to avoid stale overrides. Native DLL copies are SHA-verified; never load a historical unverified binary.

1. Compile native C ABI tests with the established F0/F1 recipe:

```
cl /nologo /std:c++17 /EHsc /O2 /DNDEBUG /MT /W3 /I code/capi /I code /I code/explore code/capi/fm_capi_test.cpp code/capi/fm_capi.cpp /Fo:<Release-output>/ /Fe:<Release-output>/fm_capi_test.exe
cl /nologo /std:c++17 /EHsc /Od /Zi /RTC1 /D_DEBUG /MTd /W3 /I code/capi /I code /I code/explore code/capi/fm_capi_test.cpp code/capi/fm_capi.cpp /Fo:<Debug-output>/ /Fd:<Debug-output>/ /Fe:<Debug-output>/fm_capi_test.exe
<each-output>/fm_capi_test.exe code/capi/demo_motion.json
```

Both configurations must exit 0 and report 45 checks, 0 failures. The original manual-clock-derived frame/ms display is not a wall-clock measurement or acceptance threshold.

2. Build a fresh native DLL with the unchanged F0 `/LD /O2` recipe, record its SHA, copy it to each runner output and verify the copy. Build and run the **whole existing standalone net8 test project**, in each configuration:

```
cl /nologo /std:c++17 /EHsc /O2 /W3 /LD /I code/capi /I code /I code/explore code/capi/fm_capi.cpp /Fo:<native-output>/ /Fe:<native-output>/fluidmotion.dll
dotnet build MotionSoftDemo/z3a_l1_devsmoke/Z3aL1DevSmoke.csproj -c Debug --no-incremental -o <attempt>/ds_Debug
dotnet build MotionSoftDemo/z3a_l1_devsmoke/Z3aL1DevSmoke.csproj -c Release --no-incremental -o <attempt>/ds_Release
<each-output>/z3a_l1_devsmoke.exe code/capi/demo_motion.json
```

Require both exit 0, `Z3A_DEV_ONLY verdict=PASS fails=0`, all original and imported F1B assertions, six E1 event names and count-only restricted events. This is a complete existing test target, not a substitute claim about FluentHub/Files application baselines. Standard output directory options do not suppress diagnostics or change project targets.

3. Copy the existing `z5/t3_results_dev/K93N3P8/SurfaceDump_Program.cs` and its `SurfaceDump.csproj.txt` to the ignored tool directory, build the unchanged net10 tool, and run it on **both newly built** devsmoke assemblies. Require CheckSurface fails=0 and full LF-normalized dump equality to `surface_ds_Debug.txt` / `surface_ds_Release.txt` from that same existing baseline directory. Only a documented first-line filename/path difference may be normalized; no public member difference is allowed.

4. Check frozen Git blobs and normalized composition SHA PRE/POST; ensure all production source/schema/LOCKED blobs equal base. Verify all imported raw files against pinned source blobs, exact source ledger archives, evidence hashes, scope whitelist, each new evidence file <5MB, and no hardcoded machine paths in newly generated artifacts.

## CI gates and acceptance

New Windows-hosted CI runs a Debug/Release matrix executing the same native build/run, whole managed runner build/run, DLL-copy hash and surface checks. A separate policy/evidence gate checks source freeze, source ancestry/import integrity, ledger archives, bounded scope and evidence manifest. No PERF-CI or performance collection. Preserve logs and job handles; a pending gate is not pass. Every applicable check must be green on the exact PR head before primary-agent merge.

No fresh GUI is required because production/GUI-host source is unchanged; F0 GUI and F1C host evidence are imported as historical, not claimed as newly validated or re-adjudicated. Pack sources remain unchanged, so this stage does not rebuild/adopt a new pilot pack or trigger F7/F11 source-change reruns.

Deliverables: merged source provenance, two test source changes, repeatable targeted scripts/CI, Debug/Release native and managed logs, both CheckSurface/full dumps, immutable original evidence, frozen/API/hash records, append-only ledger, stage report and one PR.

Stop and report evidence if main/source tips drift, a conflict affects anything beyond the identified ledger/test scope, a test/public dump/frozen check fails without an in-scope test-fixture fix, or progress requires production/frozen/schema changes, credentials, installations, new warning/audit/toolset flags, real application package/GUI operations, or timing collection. Preserve failed attempts; never silently skip a failed test. No force/amend/rebase.
