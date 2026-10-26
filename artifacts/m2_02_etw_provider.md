# M2-02 ETW provider (implementation-chosen; gate did not freeze GUID/ids)

Provider name: FluidMotion-Motion
Provider GUID: 8f3a2c1d-6b4e-4a91-9c7d-2e5f8a1b3c4d
Implementation: System.Diagnostics.Tracing.EventSource (MotionEtw.cs)

## Minimal set (6) — event name / id / level
| EventName | Id | Level | Notes |
|---|---|---|---|
| AnimationLifecycle | 1 | Informational | Created/Played/Cancelled |
| Retarget | 2 | Informational | handoff from/to/vel |
| FrameTick | 3 | Verbose | default off; enabled by MotionEtwMirror at accept |
| FrameTick (accept force) | 103 | Informational | remapped to name FrameTick in ledger |
| PresentStats | 4 | Informational | session-end placeholder OK |
| IdleTransition | 5 | Informational | active 0↔1 / pump on/off |
| TokenLoad | 6 | Informational | load ok/fail |

LoweringDecision: schema not required this ticket (not in min set).
NoiseProbe: out of min set (M2-01).

## Collection / WPA
- In-process EventListener ledger: artifacts/m2_02_s*_etw_events.txt + m2_02_s*_etw_names.txt (all 6 SEEN)
- EventPipe: artifacts/m2_02_eventpipe.nettrace (1483562 bytes) via `dotnet-trace collect -p <pid> --providers FluidMotion-Motion:0xFFFFFFFF:5 --duration 00:00:00:40`
- Open in PerfView / convert for WPA; provider name FluidMotion-Motion visible in collect banner
- logman ETL: Access denied (no admin) on this machine — EventPipe is the external capture path
