# Z6 continuous delivery plan

Recorded 2026-11-21 09:45 AWST, current Codex conversation. Status: active, not accepted.

## Baseline and boundaries

- Repository `oops11111/motion_framework`; main `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`.
- F5 implementation branch `codex/z6-f5`, actual base `e4cd696c10ad3d9a948c5650295be7fbbbc301cc`.
- All thirteen remote Z6 tips matched the handover at discovery; no open issues or PRs were returned. No existing Actions workflow was present.
- Acceptance source: supplied `codex_handover.md` sections 5.1, 6-8 and `coord/z6-codex-handover:docs/z6_coord/z6_f5_fix_gemini.md` sections 3.2-3.5.
- Freeze `code/`, `MotionSoftDemo/`, `pilot/l1pack/`, all LOCKED files and existing evidence. No force, amend or rebase. No timing collection; PERF-CI HOLD; Success does not imply adopt.
- One continuing implementation agent owns implementation, targeted tests, ordinary English Conventional Commit commits, push and one PR. The primary agent owns review, CI diagnosis and merge. No merge while any applicable CI or required local gate is pending.

## Stages

| Stage | Dependencies and scope | Deliverables and verification | CI and merge target | Stop conditions |
| --- | --- | --- | --- | --- |
| 1 F5 host repair | Base above; preserve existing local FluentHub changes in an isolated ignored copy; host-only WM_SETTINGCHANGE source, event/Activated secondary paths and Fake | Host patch, hashes, report, reproducible tests and PR. Run `python z6/f5/codex/verify.py`; source tests documented with implementation; ordinary `dotnet build src/FluentHub/FluentHub.csproj -c Debug -p:Platform=x64` in isolated side fork. A freeze, B Fake flip/restore with unchanged system settings, C hook plus unchanged-value message injection, E RootView lifecycle and shutdown evidence required separately | Add applicable non-performance patch/freeze/behavior CI. PR to `codex/z6-integration` initially at F5 base, avoiding implicit acceptance of F2 ancestors. Main waits for stage 2 | Frozen source change, hook failure, credentials, system-level operation, unapproved build flags or irreparable build failure. Optional D real setting switch is skipped without explicit authorization |
| 2 review and integration | Stage 1 plus outstanding Notion result review and F2 ruling; review all proposed ancestors using actual topology, not TIPS stacking | Integration proposal and focused PRs; `git merge-base --is-ancestor`, per-ticket diffs/hashes, applicable local tests; do not prewrite Z6 closure | Every proposed PR CI and local acceptance green; main only after unresolved review decisions land | Missing `z6_result_review_notion.md` / `z6_closed_DRAFT_notion.md`, disputed F2 disposition or unsupported evidence. No unilateral verdict changes |
| 3 F9a continuation | User installs VS 2026 NativeDesktop workload; fixed Files `cbfb4ba54a89632fbceb82ea94ada60a9bae3737`; existing SDK 10.0.404 retained | Separate branch from `030216fad26d2a2e0d3d3d9ef7edf057d66b07bc`, new attempt logs with original exact build command and exit; append baseline; preserve attempt1 | Applicable CI plus actual original baseline build/run evidence; main through reviewed PR | Missing v145/workload, credentials or system-level steps; no installation, substitutions, or retries to erase failed counted sessions |
| 4 F9b and closure | F9a baseline complete, user explicitly says “开”, review decisions available | Host-side L1 patch and evidence without Files source; runtimeconfig ETW check; final closure only after decisions | Applicable local acceptance and CI all green; main through reviewed PR | No F9b opening, missing review, frozen source change; F8a/F8b stay DRAFT and PERF-CI stays HOLD |

## F5 design and acceptance

MainWindow owns the source for its HWND lifetime; RootView recreation only replaces subscribers. The real source reads UISettings after any WM_SETTINGCHANGE, with a resettable single-shot 100 ms UI timer. WinUIEx 2.9.2 cached XML exposes `WinUIEx.Messaging.WindowMessageMonitor` and its Window/IntPtr constructors, WindowMessageReceived event and IDisposable. Do not narrow wParam. Retain guarded WinRT event and Activated rereads. Changed values travel via DispatcherQueue.TryEnqueue before SetReducedMotion. Unchanged rereads log only.

Fake mode is selected only by `FM_PILOT_F5=1`, never writes system settings, and cannot be overwritten by OS rereads. It must flip and restore on the UI thread. Test source and unsubscribe/queued-after-dispose behavior without substituting those tests for real WinUI hook/GUI acceptance. New evidence files each remain below 5 MB and contain no hardcoded machine paths. A report must explicitly distinguish local tests, CI and device evidence.

System animation switching (optional D), SDK/workload installation, package registration/unregistration, scheduled tasks, elevation and non-original build switches require specific user authorization. Existing workbuddy grants are not reused. If blocked, record exact command, exit, sanitized raw log and next required action; continue independent work only.

## User authorization (verbatim)

Source: user messages in this current conversation on 2026-11-21 AWST; message IDs unavailable to implementation agent. Recorded at the time above, not asserted as the send time.

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

The next user message supplied the full `continuous-project-delivery` SKILL.md. Its Plan, implementation pipeline, acceptance and persistent execution instructions are applied. The authorization covers this repository's ordinary delivery workflow, not system-level operations or non-original build flags. Only the primary agent may merge after review and complete acceptance.
