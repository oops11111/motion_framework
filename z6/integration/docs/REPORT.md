# Stage 2B documentation integration report

Local documentation gates: PASS. CI and main merge: pending primary-agent review. This is fresh acceptance of exact imports only; original tickets were not re-adjudicated. Primary reviewed scope; no external review performed by this stage. Overall project remains incomplete.

Base main: 4e128e735edf788da0b9fa9e11f0d5e2511e5585. Branch: codex/z6-integration-docs. Target: main. Plan commit 74cbce7; ordinary source merges F12 b0e190b, F4 c999215, F8a 7a9afbe, F8b c8c57d4. Full source pins and per-file SHA256/Git blobs are in source_receipts.json. Each source tip is preserved as an ancestor.

Imported four canonical documents and nineteen original report/script/raw files exactly. Only shared ledger conflicts required resolution. All five archived ledgers match their exact source blobs, including the complete base-main ledger da6459237b5140abcb38fa3f050765f3d3572a81. Aggregate ledger preserves that entire prefix and appends stage records, including PR11 merge and post-main CI44880677014 success; stage2A REPORT and manifest are unchanged.

`python z6/integration/docs/verify.py --output z6/integration/docs/raw/validation.json` passed source hashes, ledger prefix, six actual local Markdown link targets, scope and F8 DRAFT/blank approval/no issued lock checks. The raw result records the pre-seal validation (manifest_sealed=false); SHA256SUMS is subsequently sealed and final `python z6/integration/docs/verify.py` verifies its complete coverage and hashes. No absolute workstation paths were added to authored stage records; exact legacy source ledger archives retain their original bytes and historical wording.

No production/test/LOCKED/schema/public surface change, no stage2A artifact change. F12's historical wording stays verbatim; F4's AOT guide adds no optional code. Both F8 documents remain DRAFT/NOT LOCKED with approval and lock placeholders; their historical instructions/references are documentation, not new probe or measurement activity. No docs/z6_closed.md, F2/F5 main integration, pack chain or F9 work.

The only new workflow is a branch-scoped documentation import policy: this branch push runs docs1; PR main runs docs1 plus unchanged regression policy/Debug/Release3; postmerge main runs the existing regression3. All actually applicable runs must succeed before primary merge/acceptance. The fixed source whitelist does not run for future unrelated PR branches. No local native/C# rerun, application/GUI execution, system change, dependency install or special build flag was used in this documentation stage. Existing regression CI provides the normal code checks independently of local doc acceptance.

Next: primary reviews final diff, exact receipts/freeze and all applicable CI, merges to main only when green, then verifies main push CI. Remaining F2/closure/F5-main/pack and gated work are separate stages with their own dependencies.
