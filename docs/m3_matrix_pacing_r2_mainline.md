# M3-MATRIX-PACING r2 mainline 注记

WRITTEN_AT: 2026-11-06 11:08:00 AWST（Australia/Perth（AWST，UTC+8））
STATUS: **不合 main**；**不采用**；本文件仅为结题旁注，**不是**合入授权。

| 项 | 值 |
|---|---|
| 票 | M3-MATRIX-PACING **r2** CLOSED **RIVE-MATRIX-PACING-FAIL** |
| 主因 | pacing_ok=0（METRICS_FAIL） |
| budget | budget_ok=1（相对 r1 旁证；不抵消 FAIL） |
| 出帧 | WaitForVBlank→DwmFlush（`53cedde`）；pacing_source=vblank+dwmflush |
| branch | claude/m3-matrix |
| tip / results | `27ad82395e3eed94c40ad99feba6aed46eeca715` / `e3feb5cde7441e1855727dd0468275a6254e56bc`；dir=`m3/results_matrix_pacing_r2/` |
| composition.h | 未触 |
| PERF-CI | HOLD |
| r1 / MATRIX | 保持 CLOSED FAIL；**禁回改** |

**禁止**：把本注记解读为可合 main、可 adopt、可解冻 PERF-CI、可回改 r1/MATRIX、可合并 r1+r2 为 PASS。

— notion ai
