# M3-MATRIX-PACING r2 结题 brief

WRITTEN_AT: 2026-11-06 11:08:00 AWST（Australia/Perth（AWST，UTC+8））
TICKET: **RIVE-MATRIX-PACING-FAIL**（CLOSED；**r2**）
METRICS: METRICS_FAIL — **pacing_ok=0**；budget_ok=1；hitch/size/stability=1
ADOPT: 不采用；不合 main；Success≠adopt；PERF-CI HOLD
gate: MATCH `a1fa4590285f52948ecf51bc9c32629d4d2c3bc42efc7f5bb4e4922bd2be2883` @ 2026-11-06 10:57:03 AWST
approved: pidan @2026-11-06 10:57:53 AWST；approved_in_tree=yes
tip / results: `27ad82395e3eed94c40ad99feba6aed46eeca715` / `e3feb5cde7441e1855727dd0468275a6254e56bc` @ claude/m3-matrix；dir=`m3/results_matrix_pacing_r2/`
dll: `2c76816f29e7812616188bd25d4c4565ef601a047d2b66988df0bb950ce8d6a6` BUILD_OK
host/display: DESKTOP-IJI1KJG；75Hz@100% + 180Hz@150%
sessions: 3×RECORDED；scale_changes=4
pacing_p95_ms: primary=5.164/5.273/5.208；external=6.682/6.686/6.496
probe: M3-MATRIX-PACING v2；pacing_source=vblank+dwmflush（seg0–4 MATCH）
composition.h: PRE=POST 未改
备注: WaitForVBlank→DwmFlush（`53cedde`）；FAIL 照交未重跑；未动 r1/matrix results
对照 r1: r1=pacing1/budget0；r2=pacing0/budget1；**均 FAIL；不翻 r1**
next: 见 `docs/m3_matrix_pacing_r2_next.md`（**勿擅自 LOCK**）
