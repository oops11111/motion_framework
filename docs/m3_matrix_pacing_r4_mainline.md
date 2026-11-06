# M3-MATRIX-PACING r4 mainline 注记

WRITTEN_AT: 2026-11-06 11:58:00 AWST（Australia/Perth（AWST，UTC+8））
STATUS: **不合 main**；**不自动采用**（Success≠adopt；有条件采用须 pidan 另拍板）；本文件仅为结题旁注，**不是**合入授权。

| 项 | 值 |
|---|---|
| 票 | M3-MATRIX-PACING **r4** CLOSED **RIVE-MATRIX-PACING-PASS** |
| 结果 | 全判据绿：pacing/hitch/size/budget/stability=1 |
| GPU | gpu_render_p95_frac_T@180Hz≈0.040（远低于 0.35·T；对照 r3≈1.05–1.08） |
| 出帧 | 69094ee：vblank 后再等 0.3·T；pacing_source=vblank；offset_fraction=0.30；probe v4 |
| branch | claude/m3-matrix |
| tip / results | `e94600332307e4482cf0ff91d4e16d3b60deefc5` / `6b7a290864ef58ff8b908f8d46532e449c8e4530`；dir=`m3/results_matrix_pacing_r4/` |
| composition.h | 未触 |
| PERF-CI | HOLD |
| r1 / r2 / r3 / MATRIX | 保持 CLOSED FAIL；**禁回改**；**禁用 r4 洗票** |

**禁止**：把本注记解读为可合 main、可自动 adopt、可解冻 PERF-CI、可回改 r1/r2/r3/MATRIX、可合并前序 FAIL 为 PASS、可自动 LOCK 下一票。

— notion ai
