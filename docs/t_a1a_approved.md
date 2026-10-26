# T-A1a APPROVED 留证（结题文件）

> 本文件是闸门 `docs/t_a1a_gate_LOCKED.md` 约定的 **结题文件** 留证。
> 不修改 LOCKED 闸门 body（GATE_BODY_SHA256 计入 APPROVED 占位行；填入会破坏哈希）。
> 闸门原文约定：「APPROVED 的内容和时间戳必须在本文件或结题文件落盘」。

## 引用的 LOCK（不变）

- 闸门路径：`docs/t_a1a_gate_LOCKED.md`
- STATUS=LOCKED
- LOCKED_AT=2026-10-24 22:27:54 AWST
- GATE_BODY_SHA256=0750a8402d4118c98025e71863f79bafd689966c2565fa922d0674e8bcf89d1d
- 哈希约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。本留证 **不** 改写该 body。

## APPROVED 留证

- APPROVED_BY=pidan
- APPROVED_AT=2026-10-24 22:34:47 AWST
- APPROVED_NOTE=widget reply「APPROVED，派 workbuddy」after LOCK 0750a8402d4118c98025e71863f79bafd689966c2565fa922d0674e8bcf89d1d；LOCKED_AT=2026-10-24 22:27:54 AWST

## 授权含义

- 本留证 + LOCKED 闸门同时成立 → **授权派 workbuddy 按闸门采数**（仅 T-A1a）。
- 不清 A1a 挂账；不重开 S1 / R-3；不升级 Bv；不作 ship；不改 composition.h（sha256 a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418）；成功 ≠ 采用。

- COLLECTION_DONE（追加，2026-10-24 23:05 AWST）：workbuddy 采数完成，ticket=FAIL（G4-2，归因=修正后未稳态，A-only×3=0/3）；见 `docs/t_a1a_result.md`。上方 APPROVED 字段未改。
