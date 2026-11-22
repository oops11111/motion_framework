# Z6 formal closure delivery plan

Base: fresh origin/main eb09e63f3068a98fb011709132365e81d825c23c; coord pin9c28dcc6a97bda7d552ddf2a6cc6f12347063751; branch codex/z6-closure.
Acceptance source: current direct user answer and scoped question in AUTHORIZATION.json, accepted technical draft SHA0a0ef2ce22248e797aa40eb9793f3c3efefe6435b28baf5d28c632e5493e6cab, primary review and verified PR11–22/main-post chain.
User authorization chooses technical draft + primary review instead of the missing Notion result/closed drafts. It does not assert those external files exist or grant a Notion verdict.
Operating contract: sole continuing implementation agent implements/checks/ordinary commits/push/PR; primary reviews diff, all applicable CI, merge and main post-CI. No force/amend/rebase or wb history edits.
Candidate write allowlist after primary release: new docs/z6_closed.md; z6/closure/codex/**; z6/LEDGER_snapshot.md append only. Propose one minimal .github/workflows/z6-closure.yml branch-scoped documentation policy, if accepted with this Plan; existing workflows unchanged.
Never modify old reports/raw or frozen production/public/pack/E1/composition/LOCKED/APPROVED files. No new GUI/host/native/private tests, build, ETW, screenshots, settings, package/system/account actions or external messaging.

## Stage 1 — pin, authorization and plan (current gate)
Dependencies: clean latest main, existing accepted14-ticket/9-gap draft and primary review, direct user answer.
Scope/deliverables: only this PLAN.md and AUTHORIZATION.json; verify draft SHA and baseline; no canonical closure write.
Validation: git status/rev-parse/fetch; SHA256 of ignored accepted draft; exact quotation/question ID/recording-vs-send-time distinction.
Acceptance/CI: primary accepts Plan and bounded file scope; no CI run is created by this local gate.
Stop: changed main/draft, unresolved authority mismatch or unexpected working changes; report instead of guessing.

## Stage 2 — seal review sources and prepare the canonical record
Dependencies: Stage1 primary release; no repeat user approval for the already authorized closure/ordinary delivery workflow.
Scope: exact archive of accepted technical draft, matrix/status, source/link/hash receipts, primary API_PROVENANCE/HEAD_CHECKS_PROVENANCE/F6_VISUAL_REVIEW/REMOTE_HISTORY_LIMIT and necessary F1 summary; exact main ledger base archive.
Deliverables: new docs/z6_closed.md and immutable source_archive/**; review-source provenance/index and SHA256SUMS in z6/closure/codex; append ledger stage reference preserving its entire main prefix.
Archive originals as exact bytes (declare source_archive/** -text before copying); accepted draft archive must retain its exact SHA. Do not regenerate or rename sources as Notion output.
Canonical content: current bounded functional closure/dev-only evidence; all13 historical tickets plus F9b; exact PR/head/merge/mainCI links; nine-gap dispositions and the direct user-selected replacement basis.
Retain explicit limits: oldF2 reversed-order dispute/oldPASS text; oldF5 FAIL and missing original capture bytes; original cards/remote history/old disk-prehistory visibility; F12 historical r1 reference; F6 desktop-crop method/one dedicated midPNG.
Also retain F8a/b DRAFT/no probe/noLOCK; optionalF5D not performed; Files manual layout/NON-ORIGINAL routes and entry_position_changed=false; environment NGEN/DCAT warning/error history; PERF-CI HOLD and Success!=adopt.
Validation commands: git show/ls-tree/rev-parse/diff; Python stdlib SHA/manifest/local-link/source-ref checks; git diff --check for authored files. Verify archive byte hashes and pinned target existence only, without re-running old tests or GUI.
Acceptance/CI: primary can review a concrete complete document and receipts; every closure claim maps to existing accepted evidence and stated limits. Historical reports/raw remain untouched.
Stop: unresolvable citation, source/hash mismatch, new claim requiring runtime/system proof, or scope beyond the allowlist. No silent omission, evidence reconstruction or historical rejudgment.

## Stage 3 — documentation verification and reviewable PR
Dependencies: Stage2 document/source sealing; primary reviews actual diff/wording.
Deliverables: minimal verify_closure.py and (if scope accepted) branch-scoped doc policy; documented local verification result; ordinary English ASCII Conventional Commit via git commit -F; normal push and one PR to main.
Validation: python z6/closure/codex/verify_closure.py --enforce-stage-scope; check all newfiles<5MB, exact archive/SHA coverage, 14-ticket/9-gap references, ledger baseprefix, unchanged protected tree and no new LOCKED/APPROVED/probe files; check authored diff whitespace.
CI: new documentation job validates scope/evidence only and is limited to this closure branch; existing applicable policy and Debug/Release regression run naturally. Do not manually repeat those tests or change existing workflow gates.
Acceptance: local documentation checks pass, review contains exact head/diff/PR/source hashes and limitations; no claim that documentation CI reproduced GUI or historical authorization cards.
Stop: any local/CI failure is investigated on the same branch within this documentation scope; a needed production/system/flag change is outside this closure ticket.

## Stage 4 — primary integration and handoff
Dependencies: primary final diff review and every actually applicable head check green; no merge while pending/failing.
Deliverable: primary ordinary merge to main, verify merge/tree equals reviewed head and target ref; follow existing main post-CI to success.
Validation/acceptance: authenticated PR/check/run records plus Git tree equality; report canonical document/commit/PR/main-post links, retained limits and the explicit conclusion basis.
Only after these pass may this closure stage be called accepted. Implementation agent never merges and does not close the overall goal prematurely.
Stop: head/base/tree drift, non-green CI, or any unmet mandatory delivery condition; preserve evidence and let primary resolve.

Current status: Plan/auth only. No canonical document, archive, ledger append, workflow, commit or PR has been created in this stage. Await primary release of Stage2 implementation.
