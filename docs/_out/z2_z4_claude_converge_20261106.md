# Z2–Z4 converge · Claude English (2026-11-06 Asia/Shanghai)

**Status.** Discussion-only. **Do not** implement C# API v0. **Do not** invent recovered Z2–Z4 text. Starting point = `docs/z2_z4_DRAFT.md` (sha256 7522031a…). Hard bounds unchanged: WinUI 3 only; Success≠adopt; no `main` merge; no `composition.h` change unless separately LOCKED; no sampling without APPROVED; PERF-CI HOLD; Y4 `docs/COORDINATION.md`.

Current facts accepted: M0–M3 tech validation largely done on MotionSoftDemo; M3-MATRIX-PACING **r4 CLOSED PASS** on `claude/m3-matrix` tip `907da1ee` (offset=0.3·T; Success≠adopt; not on main); r4 adopt / budget SUPERSEDED still open (pidan skipped); Z1 deferred; API v0 paused.

## 1) Accept / revise scopes + exits

| Slice | Verdict | Scope (kept / revised) | Exit |
|---|---|---|---|
| **Z1** | **Keep deferred** | Nominate 1–3 WinUI 3 pilots per `plan.md` §7; no WPF v1.0 pilot | pidan names apps **or** keeps deferred |
| **Z2** | **Accept with one tighten** | Public C#/WinMD **L1 contract, doc-only**: create/load tokens → Play → Retarget (mid-flight, same-property, cross-animation) → Cancel; dispatcher/thread rules; reduced-motion hook; non-goals (no EmbedToHwnd, no WPF). **Tighten:** contract must name the **dogfood call shapes** that Z3 may temporarily satisfy via C-ABI bridge **without** mixing public WinMD and C-ABI in the shipped surface. Align `docs/api.md`. | Short LOCK-ready contract under `docs/`; coordinator ACK; **no** impl commit required |
| **Z3** | **Accept with one tighten** | MotionSoftDemo = first consumer scaffold. May start on current C-ABI/Composition path using Z2 call shapes. **Tighten:** dogfood must record tip/DLL/composition hash + seed path; Retarget≥5 with continuity check; **must not** cite r4 PASS as API adopt. | Reproducible runbook on lab DESKTOP: Play / Retarget≥5 / Cancel; lab files only; Success≠adopt |
| **Z4** | **Accept with one tighten** | Minimal ETW set + L1 smoke / ticket PASS rule for “C# API usable” (sessions, cells, INVALID). **Tighten:** gate **must not** demote PERF-CI to single-machine green; **must not** unfreeze Bv or reopen S1/R-3; sampling only after LOCK+APPROVED. | `*_gate_DRAFT.md` reviewed; LOCK+APPROVED before any new sampling |

**Net:** keep the three slices; add the three tightens above into the next DRAFT revision.

## 2) Calendar order · parallelization · ESTIMATE person-days

Assumes ~1 FTE on API/docs path (plan’s C#/WinUI seat) plus light coordinator review; **ESTIMATE** only (re-baseline after Z2 lock).

| Order | Work | Parallel with | ESTIMATE |
|---|---|---|---|
| **Day 0** | pidan confirm/revise this converge + Z2–Z4 DRAFT | — | 0 (decision) |
| **Wave A** | **Z2** contract draft → review → LOCK-ready | **Z4** gate DRAFT skeleton (events + PASS/INVALID) | Z2: **3–5 pd**; Z4 draft: **2–3 pd** |
| **Wave B** (after Z2 call shapes stable enough) | **Z3** dogfood runbook on C-ABI/Composition | Z4 refine to match Z2 names | Z3: **4–6 pd** |
| **Gate** | pidan agrees Z2 LOCK-ready + Z4 DRAFT; Z3 dogfood green on lab | — | decision |
| **Only then** | C# API v0 coding (WinMD projection / L1 impl) | Z3 retarget host to WinMD; Z4 LOCK+APPROVED before formal smoke | **out of scope this turn** (plan task 7 was 5–7 person-**weeks** — do not start) |

**Parallelization rule:** Z2 ∥ Z4 from day 1; Z3 starts as soon as Z2 has frozen **method names + semantics** (full LOCK not required to begin C-ABI dogfood); WinMD coding waits on Z2 LOCK-ready **and** pidan greenlight.

**Against plan.md calendar (ESTIMATE mapping):** this Z2–Z4 packet is a **pre-M2-Alpha doc+dogfood wedge**, not the whole M2 (~week 16) or first pilot (~week 22). Roughly **9–14 person-days** calendar ~1.5–3 weeks wall-clock at 1 FTE before API v0 coding is allowed to start.

## 3) Must finish before C# API v0 coding starts

Hard checklist (all required):

1. pidan **explicit agree** on revised Z2–Z4 DRAFT (this converge or successor).
2. **Z2** LOCK-ready L1 contract in `docs/` (call shapes, threads, non-goals, WinMD≠C-ABI).
3. **Z4** gate DRAFT exists and is reviewed (even if not yet LOCKED); PASS/INVALID rules named.
4. **Z3** dogfood runbook exists; at least one lab rehearsal on MotionSoftDemo showing Play/Retarget≥5/Cancel continuity on current path (C-ABI OK).
5. Written reminder in the kickoff: Success≠adopt; no `main`; composition.h frozen; PERF-CI HOLD; Y4 approval-before-collection for any later smoke sampling.
6. **Not required before coding:** Z1 real product pilots; r4 adopt decision; PERF-CI unfreeze; RENDER/BUDGET DRAFT SUPERSEDED formalization.

## 4) Open risks vs current M3 hang items

| Risk | Link to M3 hangs | Mitigation |
|---|---|---|
| **API work silently treated as adopt / main-merge pressure** after r4 PASS | r4 adopt / WP offset=0.3·T still open (pidan skipped) | Keep Success≠adopt on every Z3/Z4 artifact; API v0 stays on feature branch; no main |
| **Mixing r4 pacing WP into API semantics** | Conditional adopt of tip `69094ee` unresolved | Z2 contract must not depend on offset=0.3·T; dogfood records tip hash but API contract stays tip-agnostic |
| **Budget DRAFT ambiguity leaks into Z4** | RENDER-BUDGET + BUDGET DRAFT = hang/SUPERSEDED candidates | Z4 L1 smoke is **API continuity / ETW hit**, not frame-budget matrix; do not fold M3 budget gates into Z4 |
| **PERF-CI HOLD vs “API usable” headline** | PERF-CI still HOLD (≥2 machines) | Z4 forbids single-machine PERF-CI green; M2 Alpha perf-CI remains a **later** ticket |
| **Z3 dogfood on C-ABI forever** | Public surface is WinMD (plan §1 / task 7) | Z2 names bridge as temporary; Z3 exit allows C-ABI now but kickoff for v0 coding requires WinMD path plan |
| **Sampling without APPROVED** | Y4 rule 3 | Z4 LOCK+APPROVED before any formal L1 smoke campaign |
| **Coordinator thrash while Notion/Gemini revise** | Same brief in CN to peers | Freeze one DRAFT tip; only pidan/coordinator merge peer diffs |

## Ask of coordinator / pidan

1. Confirm the three **tightens** (Z2 dogfood call-shape note; Z3 tip/hash logging; Z4 no-PERF-CI-demotion).
2. Confirm **Wave A→B→Gate→only then API v0** order.
3. Keep **r4 adopt / budget SUPERSEDED** on the M3 hang track — **orthogonal** to Z2–Z4; do not block Z2 doc work on those answers.

API v0 remains **paused** until pidan agrees.
