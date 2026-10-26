# M2-02 Result

STATUS: PASS
WRITTEN_AT: 2026-10-25 01:29:16 AWST
MACHINE: DESKTOP-K93N3P8 (machineId=3951c05a-6a80-446d-898c-2ed8af24baa8)
BINARY: MotionSoftDemo (x64 Debug)

## G4 ONELINE
M2-02 | gate=ce147d212d31910b84c6c8e5566fbf50e740efcaadb27747cde827db2ec0a180 @ 2026-10-25 00:42:43 AWST | approved=pidan@2026-10-25 01:11:43 AWST | csharp=P | etw=P | preview=P | ticket=PASS | 成功≠采用；不改composition.h；禁影子IV；perfCI另票

## Composition.h
- PRE = a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418
- POST = a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418
- PRE_EQ_POST = True
- composition.h **未改**

## Gate / Approval (unmodified)
- docs/m2_02_gate_LOCKED.md STATUS=LOCKED
- LOCKED_AT = 2026-10-25 00:42:43 AWST
- GATE_BODY_SHA256 = ce147d212d31910b84c6c8e5566fbf50e740efcaadb27747cde827db2ec0a180
- docs/m2_02_approved.md APPROVED_BY=pidan @ 2026-10-25 01:11:43 AWST

## G1-1 / csharp = PASS
- L1: MotionEngine / MotionEtw / M202Probe; FmBridge Retarget (software path; 禁影子 IV)
- Create → LoadTokens(demo_motion.json) → Play → Retarget ≥5 (mid-interrupt / same-prop / cross-anim each ≥1) → Cancel
- Continuity: dynTol from last-tick Current + noTeleport + frame step < 80 DIP; jumpFails=0 all sessions
- Play / Cancel / Retarget all hit (playHits≥3, cancelHits≥3, retargetHits≥7, retargetOk=6)

## G1-2 / etw = PASS
- Provider: FluidMotion-Motion GUID 8f3a2c1d-6b4e-4a91-9c7d-2e5f8a1b3c4d (EventSource)
- Min-6 seen each session: AnimationLifecycle, Retarget, FrameTick, PresentStats, IdleTransition, TokenLoad
- FrameTick default Verbose-off; acceptance enables via MotionEtwMirror
- Evidence: m2_02_s*_etw_names.txt / m2_02_s*_etw_events.txt; EventPipe m2_02_eventpipe.nettrace (~1.4MB, provider FluidMotion-Motion in collect banner)
- logman ETL: Access denied (no admin) — EventPipe + in-process ledger used

## G1-3 / preview = PASS
- MotionSoftDemo preview-host v0 path inside M202Probe: start → import demo_motion.json → select token → load → play → observe; PREVIEW_VERDICT=PASS no_visual_crash
- Artifacts: m2_02_s*_preview.txt

## G2-LOCK-1 sessions (≥3 independent processes)
| sess | appPID | sessionId | csharp | etw | preview | verdict |
|------|--------|-----------|--------|-----|---------|---------|
| 1 | 10404 | 0x28A40DF285D5 | P | P | P | PASS_CANDIDATE |
| 2 | 62428 | 0xF3DC0DF2E9DE | P | P | P | PASS_CANDIDATE |
| 3 | 12912 | 0x32700DF34ACB | P | P | P | PASS_CANDIDATE |

Matrix: artifacts/m2_02_session_matrix.txt; runner: artifacts/m2_02_run_sessions.ps1
Run log: artifacts/m2_02_run.log (2026-10-25 01:25:50–01:27:06 AWST)

## Hard bans respected
- composition.h untouched (PRE=POST)
- gate body/hash untouched
- 禁影子 IV (FmBridge Retarget from current)
- S1/R-3/R-2 not reopened; Bv not unfrozen
- perf CI multi-machine = 另票 (not claimed)

## Key sources (Win)
- MotionSoftDemo/MotionEtw.cs
- MotionSoftDemo/MotionEngine.cs
- MotionSoftDemo/M202Probe.cs
- MotionSoftDemo/MainPage.xaml.cs (dispatch MF_M2_02 / m2_02.flag)
- artifacts/m2_02_run_sessions.ps1
- artifacts/m2_02_etw_provider.md
- artifacts/m2_02_composition_hash.txt
