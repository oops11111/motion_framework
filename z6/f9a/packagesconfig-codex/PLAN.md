# F9a stage3C3: authorized packages.config restore opt-in (NON-ORIGINAL)

Status: PLAN ONLY. Direct user approval for this ONE property has been received and recorded in AUTHORIZATION.json. Primary requested this Plan commit and will explicitly instruct execution after reviewing it. No new clone, restore, target or build has run. Technical documentation supports the mechanism but is not the source of permission.

## Problem and evidence

The exact pinned upstream full-solution command exited1 in attempt20261121T094221Z-e33cb21d-fullsolution. Launcher.vcxproj145 lacks packages/Microsoft.Windows.ImplementationLibrary.1.0.260126.7/build/native/Microsoft.Windows.ImplementationLibrary.targets; source PRE/POST was clean. Do not replace that original failure or earlier3B original app-Build PASS.

Files pin remains cbfb4ba54a89632fbceb82ea94ada60a9bae3737. Read-only inventory of ALL tracked packages.config files finds one: src/Files.App.Launcher/packages.config, Gitblobf3255c9617199ec636a01fcc01d93269be1b1f7f/SHA4e8ba66b03f978177ae68a0b6fd8758c3a517acee6b67cb7193afd0c4a9b56d5. It declares Microsoft.Windows.CppWinRT3.0.260818.1 and Microsoft.Windows.ImplementationLibrary1.0.260126.7, targetFrameworknative. Full structured inventory is in declared_packages.json beside this draft; do not upgrade or edit it.

Microsoft's [NuGet MSBuild reference](https://learn.microsoft.com/en-us/nuget/reference/msbuild-targets#restoring-packagereference-and-packagesconfig-projects-with-msbuild) documents packages.config opt-in support on MSBuild16.5+, unavailable through dotnet.exe. Its combined-command guidance uses -restore with Build's separate evaluations, not -t:restore,build. This explains the narrow candidate but does not guarantee a successful full build or deployment.

## Exact proposed command and prerequisites

Base main81543c86fc67801aa2de00f44d8b267c2ac09ee9 freshly verified; branch codex/z6-f9a-packagesconfig; eventual PR target main. Primary accepted PR17 failure evidence and verified postmain44891508566 three jobs success. Allowed repository scope: z6/f9a/packagesconfig-codex/**, .github/workflows/z6-f9a-packagesconfig.yml and append-only aggregate ledger plus exact opaque base ledger archive. Existing stages/manifests/workflows are immutable. The user's continuing delivery instruction remains preserved verbatim in z6/integration/tests/PLAN.md. Primary reviews diff/all applicable CI and owns merge; implementation agent never merges. Existing stages/source/manifests immutable. Fresh no-hardlink local clone from read-only original Files at the same pin, new ignored attempt; no reuse of the failed clone or generated outputs. Record original source HEAD/tree/status, every tracked packages.config and key props/projects PRE.

Using the existing VS18Community MSBuild/Current/Bin/amd64/MSBuild.exe, only this ONE additional property compared with the documented upstream command:

```text
MSBuild.exe -restore Files.slnx -p:Configuration=Debug -p:Platform=x64 -v:quiet -clp:ErrorsOnly -p:RestorePackagesConfig=true
```

Mark the entire attempt NON-ORIGINAL even if it passes. Only child DOTNET_ROOT/PATH-prefix select existing SDK10.0.404 and restore missing AMD64; no persistent environment change or compiler override. No separate nuget/dotnet restore, target switch, project exclusions, RestoreIgnoreFailedSources, NuGetAudit/WNAE/TFM/toolset/signing/GenerateAppxPackage properties, source patch or manual package copying.

Expected authorized filesystem effects: NuGet writes the fresh clone's ignored repository packages directory and normal NuGet cache/obj/bin according to the current NuGet config. Restoring precisely declared package versions is NOT an upgrade or a VS/runtime component installation. Inventory package directories/nuspec versions and relevant imported props/targets SHA afterward; verify versions match declarations and all packages.config source bytes/Git blobs remain unchanged. No restore sources or account configuration change. Any network/auth prompt, package unavailability or mismatched version stops without fallback or credentials handling.

## Result, source and side-effect gates

One process only, record exact argv/env/PID/UTC/exit/full stdoutstderr and original/published hashes. ErrorsOnly is original console logging, not a claim of zero warnings. Source POST clean/head/tree/key hashes and all packages.config identical; tracked changes are FAIL and retained, not repaired/reverted. Expected ignored package/bin/obj and native PostBuild assets are recorded separately.

If code0, report NON-ORIGINAL full-solution Build PASS only, bind produced app/server/native/helper/manifest/recipe/layout existence and hashes through a read-only postscan. This does not establish standard VS layout/Server merge, signing/registration, startup/runtime/AOT-ETW or F9b readiness. If nonzero, preserve exact new error and stop; do not stack further flags or run another build.

Static MSIX defaults from3C2 review remain packaging-on-Buildfalse/temporary-certfalse, but do not use overrides to enforce success. Certificate/signing permission need, extra SDK/ATL/component install, Deploy/AppxInstall/registration, app/test execution, task/UAC/system animation action or source change is outside this candidate and must stop. No token/account/pay action. No installer or certificate command is authorized by this property grant.

## Delivery and acceptance

One independently reviewed stage PR after primary implementation instruction, with verbatim current user decision/time-source, exact declaration inventory, original raw failure continuity, source/restore receipts, report, base ledger whole-prefix archive/append and own sealed manifest. New branch-scoped provenance CI plus existing repository regressions, clearly not a cloud Files build. Primary reviews all applicable checks/diff and main merge/post-CI. No manual repository native/C# regression repeat for evidence publication, no self-merge.

Current direct user authorization, verbatim: “批准 F9a 仅本票增加 -p:RestorePackagesConfig=true，按原版本还原并构建，标记“非原样”。” Source: latest direct user message in current root conversation; message ID unavailable. Primary recorded2026-11-21 10:27:54UTC /18:27:54AWST, recording time not send time. This covers the single property and declared-version restore/build only, not registration, optional components, F2/F9b, certificates or other flags. Implementation waits only for primary review/execution instruction; do not ask user to approve the same flag again.
