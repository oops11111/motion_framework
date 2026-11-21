# F9a stage3C1: read-only deployment preflight

Evidence completeness/scope: local PASS. Startup: NOT READY/NOT TESTED — current appbin is not a verified deployment layout. This is not a normal-VS deployment failure, upstream bug adjudication, complete F9a acceptance or F9b permission. Primary diff/CI/main merge remain pending.

Base main425165e112c271dc76706e5ff2d5354b1dfc9120; branch codex/z6-f9a-deployment-preflight. PLANc79c8634364ace1f25ff9d6bd24ff0c61e08c941 was reviewed before implementation. Stage3B PR15 and mainpost44889148641 were accepted by primary. All previous3A/3B source/artifacts/manifests/workflows and old verdicts are unchanged; base ledger archived exact and aggregate retains its complete prefix plus append-only records.

One successful scanner invocation read only the existing successful3B Files clone at cbfb4ba54a89632fbceb82ea94ada60a9bae3737, its generated manifest/recipe and referenced SDK targets. SourcePRE==POST, no tracked/untracked changes. A source-string syntax issue was corrected before execution; no failed scan or target operation occurred. Complete original JSON remains ignored under m0/out/f9a-deployment-preflight-20261121-final; sanitized published JSON and both byte hashes are in raw/20261121-readonly/receipt.json. No upstream/SDK source text or full recipe/manifest is copied into this repository. Native-helper PostBuild/xcopy hash/blob/line references were separately captured by a bounded read-only query, not silently claimed as part of the first scanner snapshot; its sidecar is retained.

## Proved current-output and source facts

- Generated manifest SHAad4ee744e9e3b82875b656f3da15754003d673987b978c7c411af4d7c7c890b4: FilesDev4.3.0.0x64/CN=Files, App->Files.exe. Structured output records dependencies, capabilities/extensions and all declared executable/server path checks. It does not exercise those capabilities.
- App recipe SHA12e12ed3d1f60a365e9abca14fa8f45293422ada0e05d324a170ec33b8691e5c:1300 mapped files, all referenced sources exist; no Files.App.Server/ subtree or cross-recipe AdditionalPackageReference/ProjectReference/OptionalProjectRecipeFiles item. Its declared LayoutDir points to target output/AppX, not an already proven final package; AppX does not yet exist.
- Server was compiled separately: Files.App.Server.exe SHA c39f5fd212e91de45cc4887b2e90ad4cde68bf44fd975228fc261b9a865a9c05 exists in its own bin; no server appxrecipe found in that exact output directory. Manifest's server path is absent beneath current appbin. This establishes current path/mapping facts, not what every VS deployment later constructs.
- App.csproj146-150 copies Server.winmd and168-174 BuildFilesAppServer builds Server; it is not a Server ProjectReference. Files.slnx includes native helper BuildDependency and app/server Deploy entries. Separate helper vcxproj references show PostBuild xcopy into app assets. None of those targets or helpers was run in this stage. Inherited/evaluated project deployment properties were not inferred from missing XML attributes.

## Active target mechanism, not an executable recommendation

Existing app obj nuget.g.targets imports MSIX1.7.251221100; server obj imports lack MSIX packaging. Exact import-file hashes and lists are stored. Referenced Microsoft.Windows.SDK.BuildTools.MSIX.Packaging.targets SHAad6a90582d0bf3f452bc057eecd7ddf7368b4337199dfce240e6382fc8992458. The active file has these concrete line references:

| Lines | Confirmed mechanism / boundary |
| --- | --- |
|158 /201|Recipe LayoutDir defaults TargetDir/AppX; DIFFERENT AppxLayoutDir defaults IntermediateOutputPath/PackageLayout. Missing appbin subtree cannot directly stand for missing final VS package.|
|1814-1838,1848-1880|_CreatePackageLayout_CalculateInputsAndOutputs consumes existing AppxPackagePayload; _CreatePackageLayout removes AppxLayoutDir, makes directory and copies, possibly hardlinks. It has no dependency chain preparing a complete payload, and is not a proved standalone VS AppX layout target.|
|1718-1808|Its normal parent bundle chain also creates packages/bundles and other artifacts; not a pure layout-only safe command.|
|1650-1665,4394-4405|_GenerateAppxPackage can Build/create packages/clean signing artifacts, and enables IncludePublishItemsOutputGroup; GenerateAppxPackageOnBuild=true also enables publish. Do not propose either as a harmless layout-only route.|
|3508-3513,4566-4608|Recipe generation consumes AppxPackagePayload; OptionalProjectRecipeFiles comes from BundleMappingRecipes (optional-package path), not proof of automatic Server merge. Current recipe contains no such reference.|

Literal target/task structure is recorded without evaluating/invoking targets. No active-file evidence proves that normal VS deployment automatically merges Server; nor does it disprove a VS project-system mechanism outside these files. No safe complete standard layout-only command has been established. We did not continue VS black-box research or guess/run a command. Legacy Microsoft.AppXPackage.Targets is not treated as the active file merely because older documentation uses that name.

Pinned README38-40 points to the [official build guide](https://files.community/docs/contributing/building-from-source); the current page describes opening the solution and using Files.App as startup in Debug/x64. That page is dynamic, not a pinned cbfb deployment contract. Pinned CONTRIBUTING mentions registering a debug app for interaction tests but supplies no Server merge CLI; its Files.Package wording is not treated as proof of a current packaging project.

Pinned AGENTS Build section documents `msbuild -restore Files.slnx -p:Configuration=Debug -p:Platform=x64 -v:quiet -clp:ErrorsOnly`. This is a potential original full-solution route for a SEPARATE reviewed stage; it was NOT run here and does not itself prove Server layout or imply Deploy. No manual payload assembly, property override, manifest edit, signing or registration is recommended by this evidence stage.

## Acceptance and limits

`python z6/f9a/deployment-preflight/verify.py` checks complete stage manifest, structured receipt consistency/source pin/cleanliness, source/SDK reference hashes, base ledger exact archive/prefix and scope. Cloud CI checks stored evidence integrity, not workstation SDK inspection, layout validity or application launch. Existing actual repository regression runs naturally; no local native/C# repetition for this read-only stage. New branch-scoped policy does not constrain future legitimate build/deployment routes.

All authored new files sanitized/under5MB; original JSON remains intact. No MSBuild, layout copy/remove, registration/Deploy/AppxInstall, certificate/task/install/UAC, application activation or source change. No F2/F5, closure, runtime/performance collection or F9b. Any deployment, non-original route, package registration and startup needs its own reviewed Plan and required direct user permission; current output observation does not grant it. Overall project is incomplete.

Next: primary reviews this bounded evidence and all applicable CI before main merge/post-CI; choose a separate original full-solution or deployment route only afterward. Startup has not been accepted.
