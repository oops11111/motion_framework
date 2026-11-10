# Z1 · Pilot app scoring table (DRAFT)

- **STATUS:** DRAFT · 2026-11-10 22:59 AWST (Australia/Perth)
- **Rule:** Produce scores; **do not pick the winner.** B/C seats below are **APPROVED BY PIDAN** (written, 2026-11-10 22:59 AWST: 「同意 B=FluentHub、C=Files，推上去」). Seated = fork-local only; no upstream PRs; Success≠adopt; **no Winner chosen**; pilot coding still requires a separate ticket. Coordinator does not select Winner. Backups remain backups (not seated).
- **Scope:** WinUI 3 only (`plan.md` §7). No WPF / UWP / Avalonia / Blazor-WPF as v1.0 pilot. Success≠adopt. Pilot is **fork-local only**; no upstream PRs.

## Nominations

| ID | Candidate | Seat | Notes |
|---|---|---|---|
| **A** | MotionSoftDemo | Internal dogfood / scaffold | Also Z3a dogfood host; 33/50; **not** the product-pilot winner; Success≠adopt |
| **B** | **0x5bfa/FluentHub** (**APPROVED BY PIDAN** · seated) | Real pilot seat | Friendly mid-size WinUI 3 GitHub client; MIT; lists/nav; fork-local only; no upstream PRs. Approved 2026-11-10 22:59 AWST. |
| **C** | **files-community/Files** (**APPROVED BY PIDAN** · seated) | Contrast | Heavy, active WinUI 3 file manager; MIT; list/nav stress; fork-local only; no upstream PRs. Approved 2026-11-10 22:59 AWST. |
| **B backup** | Gaoyifei1011/GetStoreApp | Real pilot (alt, **not seated**) | More recently active; MIT; WASDK 2.6.5; Store/WinGet lists; shell-extension / WindowsAPI ding on W3. Remains a backup. |
| **C backup** | bkaankose/Wino-Mail | Contrast (alt, **not seated**) | Heavy WinUI 3 mail client; **GPL-3.0** (fork-local only; no redistribute without GPL compliance). Remains a backup. |

## Scoring rubric (0–5 each; higher better)

| Criterion | Weight | What “5” means |
|---|---|---|
| W1 Team access / friendliness | 2 | Dedicated partner; fast feedback loop |
| W2 Real motion pain | 2 | Lists / navigation / onboarding pain today |
| W3 WinUI 3 fit | 2 | Pure WinUI 3; no HWND-island dependency for v1 path |
| W4 Telemetry / lab access | 1 | Can measure jank / adopt ETW |
| W5 Designer token ownership | 1 | Designers will author/edit motion tokens |
| W6 Size / risk | 1 | Mid-size; not critical-path business |
| W7 Contrast value | 1 | Exercises heavier surfaces (more weight for C) |

**Score** = Σ (weight × score). Max = 50.

## Table (fill scores; leave Winner blank)

Public-GitHub seats have **no dedicated partner and no designer writing our tokens**. W1 is therefore scored on fork-friendliness (permissive license, mid-size, buildable WinUI 3, no need for upstream), not as if a product team had signed on. W4/W5 stay conservative (local-fork ETW is possible; no shared lab / no token partner). A remains internal dogfood even if a public seat scores near it. Role ≠ highest raw score. **Winner left blank.** Seating B/C ≠ picking a Winner ≠ authorizing pilot coding (separate ticket).

| Criterion (w) | A MotionSoftDemo | B 0x5bfa/FluentHub (**APPROVED BY PIDAN**) | C files-community/Files (**APPROVED BY PIDAN**) |
|---|---|---|---|
| W1 Team (×2) | 5 (internal) | 3 (MIT, mid-size, forkable; no partner) | 2 (MIT, forkable, but huge; upstream signoff — we will not PR) |
| W2 Pain (×2) | 3 (demo pain) | 4 (repo/issue/PR/user lists + RootView / page nav) | 5 (file lists, folder nav, previews, layout transitions) |
| W3 WinUI 3 (×2) | 5 | 5 (`UseWinUI`; WASDK 2.0.1; no shell-extension project in tree) | 4 (main `Files.App` is `UseWinUI` + WASDK 2.6.5; also Open/Save dialog + App.Server / shell interop) |
| W4 Telemetry (×1) | 4 (lab probes exist) | 2 (can instrument a local fork; no shared lab) | 2 (same; no shared lab) |
| W5 Designer tokens (×1) | 2 (eng-owned today) | 1 (no designer partner for our tokens) | 1 (Fluent culture exists; no partner for our tokens) |
| W6 Size/risk (×1) | 5 (non-prod) | 5 (moderate ~44k KB repo; fork-local, non-critical) | 2 (large ~290k KB repo; integration risk) |
| W7 Contrast (×1) | 1 (not contrast) | 2 (real product surfaces; not the heavy seat) | 5 (the contrast seat) |
| **Weighted total** | **33** (prelim) | **34** (**APPROVED BY PIDAN** · seated) | **32** (**APPROVED BY PIDAN** · seated) |
| **Winner** | — | — | — |

A is scored as **internal dogfood / primary scaffold**, not as the product pilot winner.

### Backup scores (remain backups; not seated)

| Criterion (w) | B backup GetStoreApp | C backup Wino-Mail |
|---|---|---|
| W1 Team (×2) | 3 (MIT, active, forkable; no partner) | 2 (forkable locally; **GPL-3.0** is less friendly) |
| W2 Pain (×2) | 4 (AppList / Store / WinGet search / downloads / settings pages) | 5 (mail lists, conversation nav, compose) |
| W3 WinUI 3 (×2) | 3 (WASDK.WinUI 2.6.5; tree also has ShellExtension + WindowsAPI + WebView) | 4 (`UseWinUI` + WASDK on Windows; multi-platform repo; `CoreUWPContainerSetup.cs` exists) |
| W4 Telemetry (×1) | 2 | 2 |
| W5 Designer tokens (×1) | 1 | 1 |
| W6 Size/risk (×1) | 4 (larger than FluentHub, far below Files) | 3 (substantial mail client + GPL redistribution constraint) |
| W7 Contrast (×1) | 3 | 4 |
| **Weighted total** | **30** (backup, not seated) | **32** (backup, not seated) |

## Why these seats (**APPROVED BY PIDAN** 2026-11-10 22:59 AWST)

- **B = FluentHub** (https://github.com/0x5bfa/FluentHub) — **seated**: MIT; 1263 stars; last commit **2026-09-20 00:42 AWST** (`eb08f0dbcb`); release `0.10.0` on 2026-09-19 03:11 AWST; 40 commits since 2026-08-01. `net10.0-windows10.0.26100.0`, `UseWinUI`, `Microsoft.WindowsAppSDK` 2.0.1. Views include `RepositoryListView`, `RepositoryItemListView`, `IssueView`, `PullRequestView`, `InboxView`, `RootView` — list + page-transition UX in a mid-size tree. Quieter than GetStoreApp since mid-September, but a side fork is more stable and W3 is cleaner. No upstream involvement required.
- **C = Files** (https://github.com/files-community/Files) — **seated**: MIT (GitHub SPDX; tree also contains `LICENSE-MPL`); 46573 stars; last commit **2026-11-10 22:22 AWST** (`e40fa63825`); release `v4.3` on 2026-10-12 02:35 AWST; ≥100 commits since 2026-08-01. Canonical heavy WinUI 3 product (lists, navigation, previews). Some shell interop is acceptable on the contrast seat.
- **B backup = GetStoreApp** remains a backup (not seated), if FluentHub cannot be stood up in lab.
- **C backup = Wino-Mail** remains a backup (not seated), if Files is too large to stand up in lab. GPL-3.0: fork-local only.

Seated ≠ Winner. Pilot remains fork-local only; no upstream PRs; Success≠adopt. Pilot coding still requires a separate ticket.

## Verified candidates (GitHub API, 2026-11-10; times in AWST)

Stars / last commit are API values as of this write-up. “WinUI 3?” is from the default-branch csproj / README, not the marketing blurb.

| Repo | URL | License | Stars | Last commit (AWST) | Stack | Verdict |
|---|---|---|---|---|---|---|
| 0x5bfa/FluentHub | https://github.com/0x5bfa/FluentHub | MIT | 1263 | 2026-09-20 00:42 | WinUI 3 (`UseWinUI`, WASDK 2.0.1) | **B APPROVED BY PIDAN** (seated) |
| files-community/Files | https://github.com/files-community/Files | MIT (also LICENSE-MPL in tree) | 46573 | 2026-11-10 22:22 | WinUI 3 (`UseWinUI`, WASDK 2.6.5) + some shell interop | **C APPROVED BY PIDAN** (seated) |
| Gaoyifei1011/GetStoreApp | https://github.com/Gaoyifei1011/GetStoreApp | MIT | 1463 | 2026-11-07 16:35 | WinUI 3 (`Microsoft.WindowsAppSDK.WinUI` 2.6.5) + ShellExtension / WindowsAPI / WebView | **B backup** |
| bkaankose/Wino-Mail | https://github.com/bkaankose/Wino-Mail | GPL-3.0 | 1661 | 2026-11-10 22:06 | WinUI 3 on Windows (`UseWinUI`); also Android/iOS/macOS trees | **C backup** (GPL) |
| microsoft/WinUI-Gallery | https://github.com/microsoft/WinUI-Gallery | MIT | 3726 | 2026-11-09 02:26 | WinUI 3 + Windows App SDK (official control gallery) | Not a product app (same niche as A); reference only |
| Scighost/Starward | https://github.com/Scighost/Starward | MIT | 4580 | 2026-10-12 02:43 | WinUI 3 (`UseWinUI`, WASDK 1.8) + WebView2 / tray / game process | Mid-heavy MIT launcher; not seated (B already has a cleaner mid-size pick) |
| yoshiask/FluentStore | https://github.com/yoshiask/FluentStore | MPL-2.0 | 569 | 2026-02-20 03:39 (no commits since 2026-08-01; `pushed_at` 2026-10-11 is not a default-branch commit) | WinUI 3 (`UseWinUI`, WASDK 1.6) | Too quiet |
| IviriusCommunity/Rebound | https://github.com/IviriusCommunity/Rebound | MIT | 868 | 2026-11-10 18:40 | WinUI 3 (multiple small system-app rewrites, WASDK 2.6.5) | Fragmented multi-app; not a single product pilot |
| ntdevlabs/tiny11builder | https://github.com/ntdevlabs/tiny11builder | **No SPDX / no LICENSE** | 21848 | 2026-10-30 22:44 | WinUI 3 (`UseWinUI`, WASDK 2.5.1); wizard-style | License gap — not fork-safe |
| microsoft/PowerToys | https://github.com/microsoft/PowerToys | MIT | 140817 | 2026-11-10 20:34 | **Mixed**: Settings.UI is WinUI 3 (`UseWinUI`); rest Win32/WPF/modules | Not a pure WinUI 3 app; reject as B/C |
| Devolutions/UniGetUI (was marticliment/UniGetUI, redirected) | https://github.com/Devolutions/UniGetUI | MIT | 26973 | 2026-11-10 22:26 | **Avalonia** (`UniGetUI.Avalonia`; even `UniGetUI.Windows.slnx` hosts the Avalonia project; no WinUI csproj) | Reject — no longer WinUI 3 |
| DevToys-app/DevToys | https://github.com/DevToys-app/DevToys | MIT | 32243 | 2026-09-29 08:45 | **Blazor + WPF/WinForms** (`DevToys.Windows.csproj`: `UseWPF` + `UseWindowsForms` + `Microsoft.AspNetCore.Components.WebView.Wpf`) | Reject — v2 is not WinUI 3 |
| jenius-apps/ambie | https://github.com/jenius-apps/ambie | MIT | 2240 | 2026-11-03 12:44 | **UWP** (`AmbientSounds.Uwp`, `WINDOWS_UWP`, `Microsoft.NETCore.UniversalWindowsPlatform`) | Reject (WinUI 3 only) |
| jenius-apps/nightingale-rest-api-client | https://github.com/jenius-apps/nightingale-rest-api-client | MIT | 837 | 2026-07-02 14:28 | **UWP** (`WINDOWS_UWP`) | Reject |
| MarcAnt01/Fluent-Screen-Recorder | https://github.com/MarcAnt01/Fluent-Screen-Recorder | MIT | 565 | 2023-10-01 03:33 (push 2024-05-26) | **UWP** (README: “It is a UWP app”) | Reject; stale |
| ModernFlyouts-Community/ModernFlyouts | https://github.com/ModernFlyouts-Community/ModernFlyouts | MIT | 4020 | 2024-11-17 10:39 | Archived; UWP-era flyouts (not an active WinUI 3 product) | Reject |
| Pixeval/Pixeval | https://github.com/Pixeval/Pixeval | GPL-3.0 | 3196 | 2026-11-03 01:03 | **Avalonia** (`AvaloniaVersion` in Directory.Build.props) | Reject |
| microsoft/devhome | https://github.com/microsoft/devhome | MIT | 89 | 2025-02-22 03:21 | Archived | Reject |

Research: public GitHub REST API, 2026-11-10. Pilot = side fork only; Success≠adopt.
