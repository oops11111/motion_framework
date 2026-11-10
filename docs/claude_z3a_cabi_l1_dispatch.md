# Claude dispatch — Z3a C-ABI L1 implementation

- **TICKET:** Z3A-CABI-L1
- **AUTHORITY:** pidan, 2026-11-10 23:07 AWST (Australia/Perth), 「继续，按建议进行」
- **CONTRACT:** `docs/api_surface_m2_alpha_contract_LOCKED.md` (GATE_BODY_SHA256=`77416e8bbc700238b42a940a2f93d4466edec1ebd79352b007f86c267a663a0f`)
- **RULES:** `docs/api_v0_frozen.md`
- **TICKET TEXT (normative, Chinese):** `docs/z3a_cabi_l1_ticket.md`
- **Z4:** `docs/z4_etw_l1_smoke_gate_r1_LOCKED.md` — LOCKED, not APPROVED; this ticket does **not** collect Z4 data
- **UPDATED:** 2026-11-10 23:09:00 AWST

CloudAgent is **unavailable**. You design and write the code. workbuddy on DESKTOP-K93N3P8 will build and run; do not assume a cloud builder.

## Goal

Implement the frozen API v0 L1 surface — `MotionSystem` / `AnimationHandle` / `MotionTokens` — as a C# API over the **existing software backend / C-ABI bridge**. Host: **MotionSoftDemo** only. Scenario: **Play → Retarget → Cancel** (plus `Release`, token load, reduced-motion hook, matching the locked tables and Z4 G1/G2 behaviour).

Rename or wrap the current dogfood types (`MotionEngine` / `MotionAnimation` / `MotionBridge` or `FmBridge`) so the public names match L1. Keep the P/Invoke bridge private. Do not keep a parallel public `MotionEngine` / `MotionAnimation` surface.

## ETW (E1, frozen)

Provider name `FluidMotion-Motion`, GUID `8f3a2c1d-6b4e-4a91-9c7d-2e5f8a1b3c4d`, event names and event ids as in `MotionSoftDemo/MotionEtw.cs` today (ids 1–6: `AnimationLifecycle`, `Retarget`, `FrameTick`, `PresentStats`, `IdleTransition`, `TokenLoad`). **Do not rename the provider, change the GUID, or renumber events.** Fields may only be appended at the end. Point the smoke harness at the L1 type names.

## Branch

- Proposed branch: `claude/z3a-cabi-l1`
- Proposed base: `dabd5b780818af2ee2a2dcde47f2fdf788b1011e` (tip of `claude/m3-matrix` before / unless the ticket docs have already landed). If the docs commit (Z3a ticket + Z4 r1 LOCK) is already on `claude/m3-matrix`, branch from **that** commit so the ticket text is on the branch.
- Do not force-push `claude/m3-matrix` or `main`. Do not merge this work to `main`.

## Files you may touch

- `code/capi/fm_capi.h`, `fm_capi.cpp`, `fm_capi_test.cpp` — add-only for any missing L1 C exports listed in the contract appendix; keep the existing 14 exports compatible
- `MotionSoftDemo/MotionEngine.cs` (and `MotionAnimation`) → `MotionSystem` / `AnimationHandle`
- Token load → `MotionTokens` (or the Table 1 Load entry on `MotionSystem`)
- `MotionSoftDemo/MotionEtw.cs` — align/confirm E1; do not change frozen names/ids
- `MotionSoftDemo/M202Probe.cs` or a new Z3a L1 probe — Play→Retarget→Cancel under L1 names
- Internal bridge files (`FmBridge` / `MotionBridge`) as needed
- Tests: `fm_capi_test` plus any C# unit tests required for acceptance

## Hard stops

- Do **not** change `composition.h`.
- Do **not** implement a real WinRT Composition backend.
- Do **not** implement `RiveView` or `PointerFollow`.
- Do **not** make PACING r4 `offset=0.3·T` / tip `69094ee` an API default.
- Do **not** merge to `main`. Success≠adopt.
- Do **not** write any code in the Z1 pilot apps (FluentHub, Files) or their forks under this ticket.
- Do **not** perform Z4 official data collection. A K93 build + unit test + **dev-only** smoke (events emitted) is in scope; label it dev-only. It does **not** count as Z4 and does not fill APPROVED_*.
- Do not unfreeze PERF-CI, unfreeze Bv, reopen S1/R-3/R-2, or use a shadow IV.

## Acceptance (this ticket ≠ Z4 PASS)

1. `fluidmotion.dll` (x64 MSVC, `docs/api.md` §7.4) and MotionSoftDemo Debug/x64 build cleanly.
2. Public C# surface matches the locked L1 tables.
3. `fm_capi_test` PASSes.
4. Smoke harness runs Play→Retarget (including mid-play interrupt)→Cancel and emits the frozen E1 event names.
5. `composition.h` PRE=POST; no FluentHub/Files diffs; not merged to `main`.

Deliver a reviewable commit or patch on `claude/z3a-cabi-l1` plus a short note (files changed, any added `fm_*`, how Table 1 is met) and the tip SHA. workbuddy will build and run on K93 (`docs/workbuddy_z3a_cabi_l1_dispatch.md`).
