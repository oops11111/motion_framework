# M2-Δv APPROVED 留证（结题文件）

> 本文件是闸门 `docs/m2_delta_v_gate_LOCKED.md` 的 APPROVED 留证。不修改 LOCKED 闸门 body / hash（APPROVED 占位行计入哈希，填入会破坏哈希；故 LOCKED 文件保持原样不动）。

## 引用的 LOCK（不变）
- 闸门路径：`docs/m2_delta_v_gate_LOCKED.md`
- STATUS=LOCKED
- LOCKED_AT=2026-10-25 23:33:05 AWST
- GATE_BODY_SHA256=d2349ceb0d37fdf332a4f00cb2c1f1217fe6856d0f3e5222ffaf82b8b15509c6
- 哈希约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。
- 第二刀审计：`docs/notion_m2_delta_v_gate_revise.md`（DECISION_AT=2026-10-25 23:33:39 AWST）与 LOCKED 同实质；**不要求重 LOCK / 不改 hash**。

## APPROVED 留证
- APPROVED_BY=pidan
- APPROVED_AT=2026-10-26 06:21:36 AWST
- APPROVED_NOTE=widget reply「APPROVED」（t228s999）after LOCK d2349ceb…09c6；LOCKED_AT=2026-10-25 23:33:05 AWST；单机合法；PERF-CI 仍 HOLD

## 授权含义
- 本留证 + LOCKED 闸门同时成立 → **授权派 workbuddy 按闸门验收**（仅 M2-Δv：绝对 Δv 门；1 物理机 DESKTOP-K93N3P8）。
- 不重开 S1 / R-3 / R-2；不解冻 / 不升级 B_v；不改 composition.h（sha256 a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418）；禁止影子 IV；成功 ≠ 采用；本票 ≠ PERF-CI（PERF-CI 仍 HOLD `5b007dd4…282a`，须 ≥2 物理机）。
