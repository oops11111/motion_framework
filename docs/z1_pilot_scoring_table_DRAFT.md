# Z1 · Pilot app scoring table (DRAFT)

- **STATUS:** DRAFT · 2026-11-06 15:25 AWST (Australia/Perth)
- **Rule:** Produce scores; **do not pick the winner.** pidan fills real names for B/C.
- **Scope:** WinUI 3 only (`plan.md` §7). No WPF as v1.0 pilot. Success≠adopt.

## Nominations

| ID | Candidate | Seat | Notes |
|---|---|---|---|
| **A** | MotionSoftDemo | Primary / internal | Also Z3a dogfood host; Success≠adopt |
| **B** | *(friendly mid-size WinUI 3 product — pidan to name)* | Real pilot seat | Real motion pain (lists/nav/onboarding); willing design partner |
| **C** | *(heavier/older WinUI 3 — pidan to name; optional if only A+B)* | Contrast | Stresses edges; optional |

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

| Criterion (w) | A MotionSoftDemo | B *(name)* | C *(name)* |
|---|---|---|---|
| W1 Team (×2) | 5 (internal) | _ | _ |
| W2 Pain (×2) | 3 (demo pain) | _ | _ |
| W3 WinUI 3 (×2) | 5 | _ | _ |
| W4 Telemetry (×1) | 4 (lab probes exist) | _ | _ |
| W5 Designer tokens (×1) | 2 (eng-owned today) | _ | _ |
| W6 Size/risk (×1) | 5 (non-prod) | _ | _ |
| W7 Contrast (×1) | 1 (not contrast) | _ | _ |
| **Weighted total** | **33** (prelim) | _ | _ |
| **Winner** | — | — | — |

A is scored as **internal dogfood / primary scaffold**, not as the product pilot winner. B/C await pidan names + scores.
