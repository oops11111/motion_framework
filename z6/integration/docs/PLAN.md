# Stage 2B: independent documentation imports

Status: implementing; primary agent reviewed this bounded Plan before implementation on 2026-11-21 AWST. Acceptance source: coordination branch 9c28dcc6a97bda7d552ddf2a6cc6f12347063751, docs/z6_coord. N9 does not require external review before every main merge. Original ticket conclusions are not re-adjudicated; this stage accepts exact documentation integration only.

The current user's continuous-project-delivery instruction remains the authority: one continuing implementation agent implements/tests/commits/opens PR; primary agent reviews diff, follows CI and merges only after all applicable CI succeeds. The complete original instruction is preserved verbatim in ../tests/PLAN.md; no new system permission is inferred. Primary reviewed scope; no external review performed in this stage.

## Dependency, scope and source pins

Base and merge target: main at 4e128e735edf788da0b9fa9e11f0d5e2511e5585. Branch: codex/z6-integration-docs. One doc-only PR, ordinary merges in this order:

| Source | Exact SHA | Imported scope |
| --- | --- | --- |
| F12 | 876b73ac752338fab09b225e227cfde77c1c0c63 | docs/z5_closed.md historical append and z6/f12 evidence |
| F4 | 5fa5f856bc54632becbc365bb2453a71af4b0c29 | docs/z6_aot_host_guide.md and z6/f4 evidence |
| F8a | c640972a4858c6cb72dbc5060a601c4f040c0e2f | docs/z6_f8a_gate_DRAFT.md and z6/f8a evidence |
| F8b | fe2373b523c6fe2a70598af0bf37b8a028a70d63 | docs/z6_f8b_gate_DRAFT.md and z6/f8b evidence |

All four source tips diverge at old main 7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a. Read-only three-way inspection found only shared ledger conflicts. F8a references F4's guide, hence this group. Four canonical docs and nineteen original evidence files are exact imports. F12's historical phrasing remains unchanged. F4 adds no optional code. F8 remains DRAFT with blank approval/lock placeholders; no probes, threshold decisions or lock issuance.

No production/test source, stage2A tooling/manifest, LOCKED file, pack chain, F2/F5 main ancestry, F9a/F9b or docs/z6_closed.md changes. Stage2A's fresh native/C# acceptance remains separate; no new GUI/performance claims.

## Deliverables and conflict resolution

PLAN, exact source receipts, small branch-scoped checker, validation evidence and report under z6/integration/docs; one branch-specific doc policy workflow. Preserve all original reports/raw blobs. Archive four source ledgers plus entire base main ledger as opaque -text exact blobs. Resolve shared ledger by retaining the whole base blob da6459237b5140abcb38fa3f050765f3d3572a81 as prefix and appending new records, including stage2A PR11 merge/post-main-CI acceptance. Never delete sole evidence or rewrite verdicts.

## Verification and CI gates

Run once locally after import: `python z6/integration/docs/verify.py --output z6/integration/docs/raw/validation.json`. Check exact canonical/evidence/source-ledger blobs and SHA256, base ledger prefix, changed-file scope/frozen source, actual local Markdown link targets, explicit F8 DRAFT and blank approval/lock fields, no new LOCKED/probe/closure. Own manifest SHA256SUMS must cover stage deliverables (excluding itself) and validate. Link checks cover actual Markdown links, not historical inline command/path examples.

Do not execute documentation build/probe recipes or rerun local native/C# tests. Existing regression workflow is unchanged and naturally runs its policy plus real Debug/Release matrix on this PR (three jobs); docs policy runs on this branch push and this PR only. Typical head checks: push docs1 plus PR docs1/regression3; root checks all actually applicable jobs rather than relying on counts. After merge existing main push regression3 must pass. Fixed stage-import rules are branch-scoped and do not freeze future legitimate DRAFT revisions.

## Acceptance and stop conditions

Accept only exact imports, scope/hash/link/DRAFT checks passing, primary diff review, all applicable CI green, ordinary main merge and postmerge CI green. Implementation agent never merges. Stop for changed base/source pins, non-ledger substantive conflicts, broken actual links requiring source edits, proposed original verdict/approval changes, source/system/install/credential needs or new special build flags. Report evidence rather than widening scope. F2 and closure remain unresolved independent work; overall project is not complete.
