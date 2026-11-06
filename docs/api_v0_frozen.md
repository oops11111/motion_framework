# API v0 freeze rules (DRAFT — freeze NOT declared)

- **STATUS:** Rules document only · **API v0 is NOT frozen** as of 2026-11-06 15:25 AWST (Australia/Perth)
- **Contract under review:** `docs/api_surface_m2_alpha_contract_DRAFT.md`
- **Hard bounds:** Success≠adopt; no merge to `main`; composition.h untouched unless separately LOCKED; Y4 approval-before-collection

## When pidan replies 「冻结 API v0」

1. Mark `api_surface_m2_alpha_contract_DRAFT.md` → LOCKED snapshot (hash + timestamp) and point here.
2. **Add-only during pilot:** new members may be appended behind a version note; **rename/delete of frozen L1 members is gated** (requires new contract version + pidan ACK).
3. **LOCK contract ≠ merge-to-main.** Implementation stays on feature branch; Success≠adopt.
4. Implement **only** L1: `MotionSystem` / `AnimationHandle` / `MotionTokens` (+ matching C-ABI gaps listed as appendix). **Do not** pull in `RiveView` / `PointerFollow` under this freeze.
5. Do **not** encode PACING r4 `0.3·T` / tip `69094ee` as API defaults.
6. Coding still requires the separate 「开 API v0」 ticket after Wave A + Z3a dry-run (see `docs/Z_SERIES_CONVERGE.md`).

## Until freeze

- Review/change-list iterations allowed on the DRAFT contract.
- No implementation claim; no 「API v0 frozen」 badge in mainline logs.
