# API v0 freeze rules — **L1 FROZEN**

- **STATUS:** **API v0 L1 FROZEN** — pidan 「冻结 API v0 L1」, 2026-11-06 16:32:43 AWST (Australia/Perth)
- **Reaffirmed:** pidan 「确认」 2026-11-10 22:35 AWST (Australia/Perth); the freeze itself remains the 2026-11-06 LOCKED snapshot. Caveats unchanged: freeze = L1 contract only (`MotionSystem` / `AnimationHandle` / `MotionTokens`; RiveView / PointerFollow on separate tickets; no default 0.3·T; three tables + honest non-goals). Not merged to `main`; Success≠adopt.
- **Coding ticket (opened):** `docs/z3a_cabi_l1_ticket.md` — pidan 「继续，按建议进行」 2026-11-10 23:07 AWST. Implementation is limited to that ticket (branch `claude/z3a-cabi-l1`, MotionSoftDemo, C-ABI / software path). Opening the ticket ≠ merge to `main` ≠ adopt ≠ Z4 collection.
- **Frozen contract:** `docs/api_surface_m2_alpha_contract_LOCKED.md`, GATE_BODY_SHA256=`77416e8bbc700238b42a940a2f93d4466edec1ebd79352b007f86c267a663a0f`
- **Frozen surface:** `MotionSystem` / `AnimationHandle` / `MotionTokens` only (RiveView / PointerFollow parked on separate tickets; PACING r4 0.3·T is not an API default)
- **Changelog:** `docs/api_changelog.md`
- **Hard bounds:** Success≠adopt; no merge to `main`; composition.h untouched unless separately LOCKED; Y4 approval-before-collection

## When pidan replies 「冻结 API v0」

1. Mark `api_surface_m2_alpha_contract_DRAFT.md` → LOCKED snapshot (hash + timestamp) and point here.
2. **Add-only during pilot:** new members may be appended behind a version note; **rename/delete of frozen L1 members is gated** (requires new contract version + pidan ACK).
3. **LOCK contract ≠ merge-to-main.** Implementation stays on feature branch; Success≠adopt.
4. Implement **only** L1: `MotionSystem` / `AnimationHandle` / `MotionTokens` (+ matching C-ABI gaps listed as appendix). **Do not** pull in `RiveView` / `PointerFollow` under this freeze.
5. Do **not** encode PACING r4 `0.3·T` / tip `69094ee` as API defaults.
6. The separate 「开 API v0」 ticket is now open: `docs/z3a_cabi_l1_ticket.md` (2026-11-10 23:07 AWST). Work stays on `claude/z3a-cabi-l1`. Z3a rehearsal report is still outstanding and does not block writing code; Z4 collection still waits for the L1 build plus 「APPROVED Z4 r1」.

## Still not authorized by the freeze (or by opening the coding ticket)

- Merge to `main`; adopt.
- Z4 official data collection: the r1 gate is LOCKED (`docs/z4_etw_l1_smoke_gate_r1_LOCKED.md`) but APPROVED_* are empty; pidan must send 「APPROVED Z4 r1」 in chat, and the Z3a L1 build must already exist.
- Coding outside the Z3a ticket (including any FluentHub / Files / `RiveView` / `PointerFollow` / real compositor backend work).
