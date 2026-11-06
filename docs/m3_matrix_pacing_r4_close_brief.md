# M3-MATRIX-PACING r4 结题 brief

WRITTEN_AT: 2026-11-06 11:58:00 AWST（Australia/Perth（AWST，UTC+8））
TICKET: **RIVE-MATRIX-PACING-PASS**（CLOSED；**r4**）
METRICS: METRICS_PASS — **全判据绿**：pacing_ok=1；hitch/size/budget/stability=1
ADOPT: **不自动采用**；不合 main；Success≠adopt；有条件采用须 pidan 另拍板；PERF-CI HOLD
gate: MATCH `81fc74ebba270feb818646cd5f5c8c90fd2d3812c3447c642ed126413108934c` @ 2026-11-06 11:27:50 AWST
approved: pidan @2026-11-06 11:36:53 AWST（`docs/m3_matrix_pacing_r4_approved.md`）
tip / results: `e94600332307e4482cf0ff91d4e16d3b60deefc5` / `6b7a290864ef58ff8b908f8d46532e449c8e4530` @ claude/m3-matrix；dir=`m3/results_matrix_pacing_r4/`
dll: `29daae326b5ee3903b8e2ed1352372825007065de92f966db66310746ad16026` BUILD_OK
host/display: DESKTOP-IJI1KJG；75Hz@100% + 180Hz@150%
sessions: 3×RECORDED；scale_changes=4
pacing_p95_ms: primary≈13.38–13.39；external≈5.63–5.69
probe: M3-MATRIX-PACING v4；pacing_source=vblank；pacing_offset_fraction=0.30（三会话 MATCH）
pacing_offset_last_ms: @75Hz≈4.06–4.52（over_T≈0.304–0.339）；@180Hz≈1.79–2.19（over_T≈0.322–0.394）
gpu: render_p95_frac_T@180Hz≈**0.040**（远低于 0.35·T；对照 r3≈1.05–1.08）
composition.h: PRE=POST 未改
备注: 代码 **69094ee**（vblank 后再等 0.3·T）；未动 r1/r2/r3/matrix；禁洗票
对照: r1=pacing✓/budget✗；r2=pacing✗/budget✓；r3=pacing✓/budget✗（render）；r4=**全绿**（分立 tip；**不**翻前序 FAIL）
next: 见 `docs/m3_matrix_pacing_r4_next.md`（归档 PASS；RENDER-BUDGET/BUDGET DRAFT 挂账或 SUPERSEDED **候选**；**勿擅自 LOCK**；不授权新采数）
