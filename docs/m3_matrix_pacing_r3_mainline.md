# M3-MATRIX-PACING r3 mainline 注记

WRITTEN_AT: 2026-11-06 11:26:00 AWST（Australia/Perth（AWST，UTC+8））
STATUS: **不合 main**；**不采用**；本文件仅为结题旁注，**不是**合入授权。

| 项 | 值 |
|---|---|
| 票 | M3-MATRIX-PACING **r3** CLOSED **RIVE-MATRIX-PACING-FAIL** |
| 主因 | budget_ok=0（METRICS_FAIL）；gpu_render_p95_frac_T@180Hz≈1.05–1.08 超 0.35·T |
| pacing | pacing_ok=1（相对 r2 旁证；不抵消 FAIL） |
| 出帧 / GPU | 878892d：只等 WaitForVBlank；拆分 render/copy；预算按 render；pacing_source=vblank；probe v3 |
| gpu_copy | ≈0.008 ms（可忽略） |
| branch | claude/m3-matrix |
| tip / results | `82eacb0b59e1bab7ee809b4b62c8a100545b4073` / `39cdcf42e7758555b223c8058cc8a2e9e3793aff`；dir=`m3/results_matrix_pacing_r3/` |
| composition.h | 未触 |
| PERF-CI | HOLD |
| r1 / r2 / MATRIX | 保持 CLOSED FAIL；**禁回改**；**禁合并洗票** |

**禁止**：把本注记解读为可合 main、可 adopt、可解冻 PERF-CI、可回改 r1/r2/MATRIX、可合并 r1+r2+r3 为 PASS、可自动 LOCK 下一票。

— notion ai
