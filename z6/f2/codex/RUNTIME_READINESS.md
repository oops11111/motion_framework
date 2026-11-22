# F2 runtime tools ready for review

PREPARATION PASS; runtime NOT EXECUTED. No F2 package-operation grant has been received. The authority context remains pending and execute refuses it.

Build binding: FluentHub.dll 428fe0ba893c45df426da289685cc687caf2e5e216d9a2e4506e38d7eeab4981; manifest fe5dc89b96997208936d7ad3263254a047ef604c696d1ce8be40b5a4bdd243f4; identity 49462fluenthub-uwp.FluentHub, 0.11.0.0, x64, original publisher CN=4E3C0825-4D00-4ECC-89F8-528E9200B125. The existing appbin is bound by a 284-file inventory, layout SHA d5c6885a1f8099066e5902ba2d978f9be9fb53e659578de57861415e996df3da. No new build or manual layout copy occurred.

Final runtime binding SHA: 168ae7bf73d784d04a4f988868f9dc7d95b28c7b95764ae1265a10afaaad8f46. Seven tools are individually hashed. Source pin/ten files, full layout set/bytes/hashes, manifest, root containment and every parent reparse gate are checked before execution.

Actual final checks: four Python AST parses PASS; three WinPS5 parser checks PASS; default dryrun exit0; real WinPS5 ValidateOnly exit0 with full authority guard and COM compilation, returning before activation/package query/registration/UIA/input. No state file was created. Evidence: raw/runtime_preparation_final/.

Proposed execution, only after the new direct F2 package grant and separately bound primary instruction:
`python z6/f2/codex/run_runtime.py --execute --instruction z6/f2/codex/PRIMARY_RUNTIME_EXECUTE.json`

The sole registration command is `Add-AppxPackage -Register <bound appbin/AppxManifest.xml>` without extra flags. Ordinary activation is exact !App with --fm-pilot-f5=1 for Fake and empty arguments for Real. No screenshot, UIA, pointer movement or account input is needed.

Read-only prechecks require package/data/process/TEMP-log absence and correct installed framework. Exact owned PID/path/package is held for observations and normal WM_CLOSE. Each mode's full original log is copied and verified after exit and before the next mode; redacted publication has a distinct hash. TEMP fallback, if newly created by this attempt, is retained as evidence and explicitly reported as a residual; no old TEMP log is overwritten or deleted.

Cleanup always saves a terminal result. Partial registration is captured by exact identity/path; unknown/live process prevents uninstall. Removal rechecks the sole exact package identity/location/fullname immediately before `Remove-AppxPackage -Package <newly captured fullname>`. No runtime/framework/foreign package is removed, and no process is force killed. Package deltas and four read-only animation settings are compared after cleanup.

The requested new user scope remains only this F2 temporary package register, ordinary Fake/Real lifecycle/F5 regression, normal close and exact cleanup. The old F5/Files package grants are not reused. No closure, LOCKED or historic verdict changes.
