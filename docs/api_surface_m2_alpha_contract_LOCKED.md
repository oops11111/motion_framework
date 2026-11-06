# M2 Alpha · Public L1 API surface contract (LOCKED · API v0 L1 frozen)

STATUS=LOCKED
LOCKED_AT: 2026-11-06 16:32:43 AWST（Australia/Perth）
GATE_BODY_SHA256: 77416e8bbc700238b42a940a2f93d4466edec1ebd79352b007f86c267a663a0f
> GATE_BODY_SHA256 convention: SHA-256 of the whole file excluding the line starting with GATE_BODY_SHA256; LOCKED_AT is included. Body not to be edited after this line is written.

FROZEN_BY: pidan（会话回复「冻结 API v0 L1」，2026-11-06 16:32:43 AWST）

- **Drafted:** 2026-11-06 15:25 AWST (Australia/Perth); revised 2026-11-06 (implemented-vs-missing appendix)
- **DRAFT:** `docs/api_surface_m2_alpha_contract_DRAFT.md` (kept, marked SUPERSEDED_BY_LOCKED)
- **Source:** coordinator change list (pidan forwarded) reviewing `m3_wp4_pilot_DRAFT` Z2 + `deepseek_m2_02_api_draft` §3 L1 + `fm_capi` / `docs/api.md`
- **Authoring bot:** Claude 网页 (Wave A / Z2 doc)
- **Hard bounds:** Success≠adopt; no merge to `main`; no `composition.h` change unless separately LOCKED; no sampling without APPROVED; PERF-CI HOLD; Y4 `docs/COORDINATION.md`
- **Related:** `docs/Z_SERIES_CONVERGE.md` (unique Z master); freeze rules → `docs/api_v0_frozen.md` (freeze declared)

## L1 freeze scope (names unified)

**In scope for M2 Alpha L1 (pilot-facing):**

| Type | Role |
|---|---|
| `MotionSystem` | Create / destroy; token load; reduced-motion; frame tick binding |
| `AnimationHandle` | `Play` → `Retarget` → `Cancel` (+ `Release` as handle drop) |
| `MotionTokens` | Load from `motion.json` / token JSON (maps to `fm_load_tokens`) |

**Parked on separate tickets (NOT L1 freeze):**

| Type | Note |
|---|---|
| `RiveView` | H1–H8 (DPI, device-lost, per-monitor pacing, …) — separate ticket |
| `PointerFollow` | WP2 follow split / expression visuals — separate ticket |

**Name unify:** drop parallel `MotionEngine` / `MotionAnimation` from DeepSeek L1 sketch; C# dogfood may temporarily wrap C-ABI under these three names. Public WinMD ≠ C-ABI (bridge OK behind the names).

**Do NOT bake into API defaults:** PACING r4 `offset=0.3·T`, tip `69094ee`, or any WP adopt decision (orthogonal M3 hang).

## Gate before coding

Wave A (this Z2 contract ∥ Z4 ETW/L1 smoke DRAFT) + **Z3a** C-ABI dry-run on MotionSoftDemo → **separate ticket** before any API v0 implementation. This file alone does **not** authorize coding.

---

## Table 1 — L1 ↔ C++ / `fm_capi` map

| L1 (C# shape, DRAFT) | C++ (`controller.h` / `api.md` §5) | C-ABI (`code/capi/fm_capi.h`, 14 exports) | Status |
|---|---|---|---|
| `MotionSystem.Create()` | `MotionSystem(clock, dispatcher?)` | `fm_create` / `fm_create_manual` | **implemented** (software path) |
| `MotionSystem.Dispose()` / destroy | `~MotionSystem` | `fm_destroy` | **implemented** |
| `MotionTokens.Load(json)` / `MotionSystem.LoadTokens` | `TokenSet::Parse` via create-from-tokens | `fm_load_tokens` | **implemented** |
| `MotionSystem.SetReducedMotion(on)` | `MotionSystem::SetReducedMotion` | `fm_set_reduced_motion` | **implemented** |
| `MotionSystem.IsRunning` | (pump / ActiveCount) | `fm_is_running` | **implemented** |
| `MotionSystem.OnFrame` / host binds frame source | `MotionSystem::Tick` | `fm_on_frame` | **implemented** |
| `MotionSystem.Current(target, property)` | `MotionSystem::Current` | `fm_current` | **implemented** (last Tick sample) |
| `MotionSystem.CreateAnimation(target, transition?)` | `MotionSystem::Create(...)` | `fm_animation_create` | **implemented** |
| `AnimationHandle.Play()` | `AnimationHandle::Play` | `fm_animation_play` | **implemented** |
| `AnimationHandle.Cancel()` | `AnimationHandle::Cancel` | `fm_animation_cancel` | **implemented** |
| `AnimationHandle.Release()` | drop handle; anim may finish | `fm_animation_release` | **implemented** |
| `AnimationHandle.Retarget(property, target, spring, from?)` | `AnimationHandle::Retarget` | `fm_animation_retarget` (+ `hasFrom`/`from`) | **implemented** |
| WinMD projection of the above | — | — | **missing** (planned; not in tree) |
| Real WinRT Composition backend | `ICompositorBackend` / lowering | — | **missing** (honest non-goal for this DRAFT) |
| `RiveView` / `PointerFollow` | — | — | **parked** (other tickets) |

### Appendix — implemented vs missing C-interface members (inventory only)

**Present today (14):** `fm_create`, `fm_create_manual`, `fm_manual_advance`, `fm_destroy`, `fm_load_tokens`, `fm_set_reduced_motion`, `fm_animation_create`, `fm_animation_play`, `fm_animation_cancel`, `fm_animation_release`, `fm_animation_retarget`, `fm_on_frame`, `fm_is_running`, `fm_current`.

**Missing for a future public WinMD L1 (not freeze blockers for C-ABI dogfood):** WinRT/WinMD types; compositor-backed dispatcher; any RiveView/PointerFollow exports; formal `SnapToEnd` public API (reduced-motion uses internal snap).

**Implemented in C++ but not exported through the C ABI (outside the L1 freeze; add later as add-only members if the pilot needs them):** `AnimationHandle::State` (no C getter), `AnimationHandle::OnEnd` (no C callback → no C# end event), `MotionSystem::ActiveCount` (only `fm_is_running`), reading the reduced-motion flag (setter only), `Pause / Seek / Reverse / SetRate / SetLoops`, markers. Listing them here is an inventory, not part of the freeze (revision 2026-11-06, Claude).

---

## Table 2 — Thread-affinity hard sentences

1. **Single UI dispatcher thread:** all `MotionSystem` / `AnimationHandle` / `fm_*` calls run on one UI thread; the types are **not** thread-safe (`api.md` / header comments).
2. **No `fm_*` inside sink/render callbacks:** `fm_sink_fn` / `fm_render_fn` (and C# equivalents) must not call back into the API.
3. **Construction affinity:** the thread that creates `MotionSystem` / `fm_create` is the owning UI thread; do not hop.
4. **Frame pump:** software path uses host frame callback → `Tick` / `fm_on_frame`; idle (`ActiveCount==0`) must unsubscribe / `render(0)`.
5. **Delegates/callbacks stay alive** for the lifetime of the system (store as fields); clearing on terminal states breaks cycles as in `api.md` §5.

---

## Table 3 — Honest non-goals (this DRAFT)

| Non-goal | Statement |
|---|---|
| Real WinRT Composition backend | **Not present.** Software / C-ABI bridge is OK for Z3a dogfood. |
| WinMD == C-ABI | **Forbidden mix in the public story.** Same *names* may wrap C-ABI temporarily; shipped public WinMD is a later projection. |
| EmbedToHwnd / WPF / XAML Islands | Out of v1.0 L1; HWND remains unsupported reference only. |
| RiveView / PointerFollow in L1 freeze | Parked; separate tickets. |
| PACING r4 / 0.3·T as API default | **Not** an API default; M3 product decision stays orthogonal. |
| PERF-CI single-machine green | L1 smoke ≠ PERF-CI; PERF-CI remains HOLD (≥2 machines). |
| Success = adopt / merge `main` | **Never.** LOCK/freeze of this contract ≠ merge-to-main. |

---

## Status of this file

**L1 FROZEN** by pidan 「冻结 API v0 L1」. Changes follow `docs/api_v0_frozen.md` (add-only during the pilot; rename / delete / signature or semantic change is gated). **Freezing ≠ merge to `main` ≠ adopt. Coding still requires the separate 「开 API v0」 ticket** (after Z3a dry run).
