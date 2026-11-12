# M3-MATRIX-PACING r3 结题 brief

WRITTEN_AT: 2026-11-06 11:26:00 AWST（Australia/Perth（AWST，UTC+8））
TICKET: **RIVE-MATRIX-PACING-FAIL**（CLOSED；**r3**）
METRICS: METRICS_FAIL — **budget_ok=0**；pacing_ok=1；hitch/size/stability=1
ADOPT: 不采用；不合 main；Success≠adopt；PERF-CI HOLD
gate: MATCH `90f9356bf2739ea2a95a56619fc73af561b0c34e4ebd097be05442f8c5719476` @ 2026-11-06 11:14:15 AWST
approved: pidan @2026-11-06 11:15:19 AWST；approved_in_tree=yes
tip / results: `82eacb0b59e1bab7ee809b4b62c8a100545b4073` / `39cdcf42e7758555b223c8058cc8a2e9e3793aff` @ claude/m3-matrix；dir=`m3/results_matrix_pacing_r3/`
dll: `81cbdbbb571110303ae8f9a2b95f6b7284176ac9995d2f9c4b81be0ff60e9f6b` BUILD_OK
host/display: DESKTOP-IJI1KJG；75Hz@100% + 180Hz@150%
sessions: 3×RECORDED；scale_changes=4
pacing_p95_ms: primary=0.060/0.065/0.064；external=0.111/0.107/0.114
probe: M3-MATRIX-PACING v3；pacing_source=vblank（三会话 MATCH）
gpu: render_p95_frac_T@180Hz≈1.05–1.08；copy_p95_ms≈0.008（可忽略）
composition.h: PRE=POST 未改
备注: 代码 878892d（只等 WaitForVBlank；GPU 拆分 render/copy；预算按 render）；FAIL 照交未重跑；未动 r1/r2/matrix
对照: r1=pacing✓/budget✗（合计 GPU 含等 DWM）；r2=pacing✗/budget✓（vblank+dwmflush）；r3=pacing✓/budget✗（render 超 0.35·T）；**均 FAIL；禁合并洗票；禁回改 r1/r2/MATRIX**
next: 见 `docs/m3_matrix_pacing_r3_next.md`（**勿擅自 LOCK**）
