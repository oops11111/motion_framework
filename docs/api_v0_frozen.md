# API v0 freeze rules — **L1 FROZEN**

- **STATUS:** **API v0 L1 FROZEN** — pidan 「冻结 API v0 L1」, 2026-11-06 16:32:43 AWST (Australia/Perth)
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
6. Coding still requires the separate 「开 API v0」 ticket after Wave A + Z3a dry-run (see `docs/Z_SERIES_CONVERGE.md`).

## Still not authorized by the freeze

- Coding: needs the separate 「开 API v0」 ticket after Wave A + Z3a dry run.
- Merge to `main`; adopt; any data collection (Z4 still needs LOCK + APPROVED).
