# F9a stage3C4: isolated VS CLI deployment baseline — gates A/B

Status: PLAN ONLY. Primary requested this commit for review; do not implement even gateA until explicitly instructed. GateB is NOT authorized by the existing restore/build grant. No devenv execution (including /?), VS UI, Deploy/register/start or new build occurs at Plan preparation.

## Dependency, authority and scope

Base main92c498fbcf8ea7216d54104886a722d34d5099fb freshly verified; branch codex/z6-f9a-vsdeploy; eventual target main. Primary merged3C3 PR18 from reviewed head32bf48a5cc7f65557e62c9769c3a3646befcd74b, identical tree990ca593d65b9c6e72744b623a33fbf54a7a6a3f. Postmain44895678100 completed success and primary accepted3C3. Earlier original full-solution FAIL and authorized NON-ORIGINAL build PASS remain separate and immutable.

User's continuing delivery instruction is preserved verbatim in z6/integration/tests/PLAN.md. Source task: coord9c28dcc6a97bda7d552ddf2a6cc6f12347063751 section3 F9a/MSIX permission boundary. Existing direct grant, verbatim:

> 批准 F9a 仅本票增加 -p:RestorePackagesConfig=true，按原版本还原并构建，标记“非原样”。

Source: direct user message in current root conversation, messageID unavailable. Primary recorded2026-11-21 10:27:54UTC /18:27:54AWST (recording, not send time), already preserved in z6/f9a/packagesconfig-codex/AUTHORIZATION.json. It authorizes gateA's necessary new preparation build for the same F9a ticket, not deployment. No new user grant is fabricated or presumed for gateB.

Allowed repository scope: z6/f9a/vsdeploy-codex/**, .github/workflows/z6-f9a-vsdeploy.yml, append-only z6/LEDGER_snapshot.md with exact opaque whole-base archive. All old stages/files/manifests/workflows immutable. No production/test/L1/pack/schema/public surface/LOCKED changes, F2/F5 integration, closure or F9b. One continuing implementation agent; primary owns diff/CI/main merge and post-CI. No self-merge.

## GateA: independent preparation and read-only readiness

Dependencies: this Plan approved plus explicit primary A instruction. Preserve sealed3C3 build outputs by using a new no-hardlink local clone from read-only %USERPROFILE%/pilot/Files, fixed cbfb4ba54a89632fbceb82ea94ada60a9bae3737. No old bin/obj copy and no modifications to3C3 source/output. Record source tree/HEAD/status and all declared packages.config/key source PRE. GateA rebuilding is necessary isolated deployment preparation, not retrying a failed build or replacing prior evidence.

Use existing VS18Community MSBuild/Current/Bin/amd64/MSBuild.exe exactly:

```text
MSBuild.exe -restore Files.slnx -p:Configuration=Debug -p:Platform=x64 -v:quiet -clp:ErrorsOnly -p:RestorePackagesConfig=true
```

Entire preparation route NON-ORIGINAL. Only child DOTNET_ROOT/PATH-prefix use existing SDK10.0.404, plus restoring missing standard AMD64. No new flag/property, source/package upgrade, independent restore, manual package copy, compiler override, signing/TFM/toolset/audit/WNAE change. Observe original stdout/stderr/exit and source POST; new error stops, no repair/retry. Verify all tracked packages.config exact (currently only Launcher, CppWinRT3.0.260818.1/WIL1.0.260126.7), actual nuspec IDs/versions, imported-file hashes and generated artifact/manifest/recipe bindings, no tracked changes. Build outputs remain ignored; no app/test execution.

Read-only preflight (without invoking devenv): existing %ProgramFiles%/Microsoft Visual Studio/18/Community/Common7/IDE/devenv.com file existence, Authenticode Valid/Microsoft signer and SHA/version; no existing devenv process. Use WinPS5 exact Get-AppxPackage -Name FilesDev and framework dependency query. ANY existing FilesDev package or pre-existing residual %LOCALAPPDATA%/Packages/FilesDev_* data path means STOP: do not reuse, remove or alter it. Record names/existence only, avoid reading personal data. Verify x64 Microsoft.WindowsAppRuntime.2>=2.5.1.0 already installed; no new framework install. Bind generated IdentityFilesDev/PublisherCN=Files/Version4.3.0.0/archx64/ApplicationApp/Files.exe and dependencies/capabilities/extensions, and source app dependency graph: test projects are not Files.App dependencies. Do not execute /? or probe deployment support.

A deliverables: exact command/PID/UTC/exit/log/source/package/artifact receipts and concrete readiness report. Startup/deploy remains NOT TESTED. If A is not ready, preserve evidence and report without asking a vague B approval. A artifacts can be reviewed/committed for a concrete later B request; no B execution until direct grant is recorded and primary confirms that gate.

## GateB: conditional selected-project deployment, normal activation and cleanup

Dependencies: A ready and primary review; then user's NEW direct grant specifically for FilesDev single-project deployment, ordinary first-window startup and exact own-package cleanup, with source/manifest/exe hashes shown. Record verbatim grant/current-conversation source/recording time and unavailable messageID honestly before execution. Existing F9a one-property grant and prior FluentHub/F5 permissions do not authorize B. Immediately repeat the mutable package/data/process/dependency preconditions before B; changed state stops.

Primary reviewed Microsoft's [devenv CLI switch reference](https://learn.microsoft.com/en-us/visualstudio/ide/reference/devenv-command-line-switches?view=visualstudio), [Deploy](https://learn.microsoft.com/en-us/visualstudio/ide/reference/deploy-devenv-exe?view=visualstudio) and [Project](https://learn.microsoft.com/en-us/visualstudio/ide/reference/project-devenv-exe?view=visualstudio): devenv.com supplies stdout/stderr, /Deploy is CLI without displaying IDE, and /Project selects one project. Combined with the source dependency graph excluding tests, this is a conditional single-app test candidate rather than unrestricted solution deployment. Do not invent a requirement to prove every VS runtime side effect before proposing the test. Actual project support/success is still tested, not assumed.

Exact candidate command after B grant, fresh log file, no /Run or extra properties:

```text
%ProgramFiles%/Microsoft Visual Studio/18/Community/Common7/IDE/devenv.com <fresh Files.slnx> /Deploy "Debug|x64" /Project "src/Files.App/Files.App.csproj" /ProjectConfig "Debug|x64" /Out <fresh ignored deploy.log>
```

Capture command/PID/start/end/exit/stdoutstderr and /Out original bytes. /Deploy can build/deploy through the normal project system; this is a different route from MSBuild build-only, preserving A's PRE binding and recording all POST changes/outputs. Do not inject RestorePackagesConfig via extra devenv syntax/env/source; A already restored the pinned inputs. No /Run, no UI clicks, no /Rebuild/fallback/deploy-all/manual-layout/Add-Appx substitute. If project unsupported, certificate/permission/component/account requirement or new error appears, record actual exit and STOP; do not install/create certs, log in, elevate/click UAC, start an IDE UI, add flags or reverse engineer VS internals.

Post-deploy query exact FilesDev, verify identity/version/publisher/arch and capture actual PackageFullName/FamilyName/InstallLocation; never invent publisher-id. Unexpected other package deployment is reported concretely without wildcard removal or silent extra authorization. Record package/artifact/recipe/layout/source state; normal VS Server merge is determined from actual results, not an advance promise. No upstream bug declaration from a single route failure.

If deploy succeeded under grant, activate normally with ApplicationActivationManager.ActivateApplication(capturedPFN + !App, empty args), record returned PID/starttime/path/package identity, visible top-level HWND and first ordinary window. No diagnostic package token, custom app flags, test execution or browsing/editing/copying/deleting user files. Do not change defaults/associations, enable startup, invoke registry scripts, log in, click updates/consent or switch OS animation/DPI/power.

Disclose registration effects: protocol files-dev, files-dev.exe alias, archive associations, declared updateTask/disabled startupTask and WinRT classes are package-managed metadata. Normal first launch may read Home/recent/clipboard/network and create new package LocalState/settings/log/cache; do not promise zero persistence. Capture relevant package-managed/default/startup PRE/POST read only; no manual repair of user settings. App-window-only screenshot only if needed, avoid personal filenames/whole desktop.

Ordinary shutdown: WM_CLOSE on identity-verified own HWND, wait main/owned Server exit; ordinary app Quit if applicable. Reverify PID/starttime/exe/package to avoid reuse/unrelated processes. If owned process hangs, report exact ownership/state first; no broad kill, Explorer termination or automatic force-close. Cleanup finally targets ONLY newly captured exact FilesDev PackageFullName after owned processes exit: WinPS5 Remove-AppxPackage -Package <that fullname>. Never remove existing runtime, other package/user data or wildcard FilesDev. If cleanup cannot safely complete, report residue promptly; do not conceal it. Preserve logs before removal, verify FilesDev absent and recorded POST state; existing package/data was a STOP precondition, not cleanup material.

## Verification, CI and acceptance

Per gate/attempt original logs retained in new ignored directories; publish sanitized raw plus original/published SHA and bounded-excerpt labels if needed, each file<5MB. No Files/SDK sources or binaries in repo. Exact whole-base ledger archive/prefix retained. Stage verifier checks authority/scope/manifest, A exact one-property/source/version/artifact receipts, and B only when granted/executed: actual deployment/activation/identity/window/exit/precise cleanup and PRE/POST. Pending/not-executed B is never PASS. Source tracked mutation is FAIL and preserved, not repaired.

New workflow runs only this branch/head and validates stored evidence plus stage scope; it does not reproduce local VS deployment. Existing actual native/C# regression jobs remain unchanged; no unnecessary local reruns. Primary reviews all applicable head checks/diff before ordinary main merge and verifies post-main CI. GateA evidence/build acceptance is distinct from B runtime/startup and whole F9a completion. If B fails/blocks, report that accurately rather than shrinking acceptance to build-only.

## Stop boundaries

Main/source/hash drift, existing FilesDev or residual data, competing devenv, invalid signature, missing dependency, source/config change, certificate/UAC/credentials/component/task/install demand, unexpected package identity or errors require evidence and stop. No extra build flags, independent restore, package source/manifest edits, manual layout or scope creep. GateB waits for direct user grant; current instruction is Plan only, then primary may authorize A separately. Overall project remains incomplete.
