# M2-WGC-SEED-PATH-B′ APPROVED 留证（结题文件）

> 本文件是闸门 `docs/m2_wgc_seed_path_b_prime_gate_LOCKED.md` 的 APPROVED 留证。不修改 LOCKED 闸门 body / hash（APPROVED 占位行计入哈希，填入会破坏哈希；故 LOCKED 文件保持原样不动）。

## 引用的 LOCK（不变）
- 闸门路径：`docs/m2_wgc_seed_path_b_prime_gate_LOCKED.md`
- STATUS=LOCKED
- LOCKED_AT=2026-10-26 13:49:20 Asia/Taipei
- GATE_BODY_SHA256=a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43
- 哈希约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。
- 修订审计：`docs/notion_m2_wgc_seed_path_b_prime_gate_revise.md`
- 同伴稿：`docs/deepseek_m2_wgc_seed_path_b_prime_design.md`
- 前序不改、不翻盘：SEED-CALIB `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`；SEED-ABS `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`；Δv′ `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`。

## APPROVED 留证
- APPROVED_BY=pidan
- APPROVED_AT=2026-10-26 13:51:27 Asia/Taipei
- APPROVED_NOTE=widget reply「APPROVED」（t248s1120）after LOCK a1161c77…3a43；LOCKED_AT=2026-10-26 13:49:20 Asia/Taipei；A=V2-clamp；单机合法；PERF-CI 仍 HOLD

## 授权含义
- 本留证 + LOCKED 闸门同时成立 → **授权派 workbuddy 按闸门验收**（仅 M2-WGC-SEED-PATH-B′：默认 A=PATHB-A=V2-clamp；KEEP calib exact=0.7510829456；A′ 不同跑；1 物理机 DESKTOP-K93N3P8）。
- 不重开 S1 / R-3 / R-2；不解冻 / 不升级 B_v；不改 composition.h（sha256 a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418）；禁止影子 IV；成功 ≠ 采用；结题标签只用 `SEED-PATHB-*`；禁标 Δv-PASS / SEED-ABS-PASS / SEED-CALIB-PASS；禁翻盘三 FAIL 闸门；结构风险下 FAIL 合法、禁抬 thr；A′/Δv″/SEED-TRUTH-CONFLICT 挂账；本票 ≠ PERF-CI（仍 HOLD `5b007dd4…282a`）。
