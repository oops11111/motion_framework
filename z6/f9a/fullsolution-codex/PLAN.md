# F9a stage3C2: original upstream full-solution Build

Status: PLAN ONLY. Primary reviewed the ignored draft and authorized this Plan commit, but implementation still waits for explicit primary confirmation of3C1 main post-CI success and execution instruction. No new clone/build/target/deploy operation has run. This is an original documented full-solution build, not a manual package-layout repair or a promise of Server mapping/startup success.

## Dependency and source

Base main8e68cf81ef10bd0623651949e6a98ddef4f175ca freshly verified; branch codex/z6-f9a-fullsolution; eventual target main.3C1 PR16 merged by primary as that SHA, tree2db80f72af655c6417b5f68f21e58d49268bd38a equals reviewed head0eb69f17282a6d75a49550fe42dea394de3a099b. Main postrun44890658744 was running at Plan creation and is a hard prerequisite before implementation. Existing3A installation and3B original app-build evidence remain immutable; retain3C1 preflight facts/unknowns. Files stays cbfb4ba54a89632fbceb82ea94ada60a9bae3737 (v4.3). Pinned AGENTS.md Build section explicitly documents this command:

`msbuild -restore Files.slnx -p:Configuration=Debug -p:Platform=x64 -v:quiet -clp:ErrorsOnly`

Substitute only existing VS18Community MSBuild/Current/Bin/amd64/MSBuild.exe full path. Do not add -t, GenerateAppxPackageOnBuild, RestorePackagesConfig, signing suppression, toolset/TFM/audit overrides or project exclusions. Default target is Build; no Deploy/AppxInstall/Publish/Test command. Whole solution includes native helper dependencies and test projects; builds their outputs but does not execute tests/app by request. Original app-project-only route already passed and is not rerun as a retry.

## Isolation and environment

Fresh newly named ignored local no-hardlink clone from read-only %USERPROFILE%/pilot/Files, no old bin/obj copy; detached exact pin. Use required process/filesystem sandbox escalation for Git's signal pipe if necessary, not OS elevation. Source original remains read only. Record HEAD/tree/status/key source hashes PRE/POST; a tracked mutation or unexplained untracked source file is FAIL, retained and not reverted. Native PostBuild xcopy to generated ignored Assets output is an expected build artifact, not permission to alter tracked source.

Only child DOTNET_ROOT/PATH-prefix use existing user SDK10.0.404; standard missing AMD64 restoration. Record actual MSBuild/userSDK/tool versions without inferring actual loaded compiler from file existence. No persistent PATH/system/env changes. Check relevant inherited build override/certificate/upload environment presence without dumping secrets; unexpected overrides or credentials cause STOP, not silent clearing.

## Static default packaging/signing review (read only, not evaluated graph)

Current referenced MSIX.Packaging.targets1.7.251221100 lines150-153: GenerateAppxPackageOnBuild defaults false; _GenerateMsixPackage becomes true for explicit GenerateAppxPackageOnBuild=true, PublishAppxPackage=true or Sparse, otherwise defaults false. Lines1690-1693 gate GenerateMsixPackage by that flag. Line243 defaults GenerateTemporaryStoreCertificate=false. Line439 enables signing when a specified certificate key file exists; line576 gates SignAppx task on AppxPackageSigningEnabled. App pubxml does set PublishAot/SelfContained=true but that is not itself a Publish target invocation;3C1 lines4394-4400 distinguish F5 layout from publishing. App csproj declares AppxBundleAlways, but no new override is introduced here.

These literal defaults support an ordinary Build route rather than a Deploy request; they do not prove all evaluated conditional hooks. Do not force flags to make the command safe/succeed. If any effective need for signing/certificate creation/install, Deploy/registration, extra component/SDK/ATL, auth/upload or target change emerges, stop and preserve its error. No certificate creation/import, registration, app/test execution, manual layout assembly, installer or system task is authorized. Do not run _GenerateAppxPackage, GenerateMsixPackage or layout internals as fallback.

## Deliverables and acceptance

Deliver only z6/f9a/fullsolution-codex/**, .github/workflows/z6-f9a-fullsolution.yml and append-only z6/LEDGER_snapshot.md plus exact opaque base ledger archive. One independently reviewed PR to main; no existing stage artifacts/manifests/workflows are edited. User continuing delivery authorization is preserved verbatim in z6/integration/tests/PLAN.md; primary confirmed this pinned original build is within existing F9a authorization. This does not grant any additional component, new build-property route, certificate or deployment action. Preserve original full stdoutstderr and result per fresh attempt, sanitized evidence under5MB per file or explicit bounded excerpts+original index; record exact command/env/PID/start/end/exit. Do not add diagnostic build flags. Source PRE/POST must be clean/unchanged; bind generated outputs, native helpers and any recipe/layout via hashes only. A read-only postscan may compare3C1 observed mappings, never assert untested VS behavior.

Success means exact documented full-solution command exit0 and unchanged source, not deployment or startup. Failure (including missing NuGet/ATL/certificate/compiler/test-project dependency) is recorded as-is; no further restore target/property or new install, no rerun to accumulate success. If command emits package files within ignored output, record them; never sign/install/launch them manually or invoke generated install scripts.

Repository CI remains provenance-only for this external local build plus existing real repository regressions. All current3A/3B/3C1 artifacts/manifests/workflows unchanged, complete base ledger prefix preserved with append-only records, own sealed manifest and bounded branch-scoped policy. Primary reviews all applicable CI/main merge/post-CI; implementation agent never merges.

F9b remains unopened, F2 adjudication/permissions separate, no production/L1/pack/LOCKED changes or closure. Any later package registration/ordinary startup needs separate direct user permission and a reviewed verified layout plan. This Plan commit does not start execution; await primary post-CI confirmation and explicit implementation instruction.
