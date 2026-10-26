# M2-02 APPROVED 留证（结题文件）

> 本文件是闸门 `docs/m2_02_gate_LOCKED.md` 的 APPROVED 留证。不修改 LOCKED 闸门 body / hash（APPROVED 占位行计入哈希，填入会破坏哈希；故 LOCKED 文件保持原样不动）。

## 引用的 LOCK（不变）
- 闸门路径：`docs/m2_02_gate_LOCKED.md`
- STATUS=LOCKED
- LOCKED_AT=2026-10-25 00:42:43 AWST
- GATE_BODY_SHA256=ce147d212d31910b84c6c8e5566fbf50e740efcaadb27747cde827db2ec0a180
- 哈希约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。

## APPROVED 留证
- APPROVED_BY=pidan
- APPROVED_AT=2026-10-25 01:11:43 AWST
- APPROVED_NOTE=widget reply「APPROVED」after LOCK ce147d21…a180；LOCKED_AT=2026-10-25 00:42:43 AWST

## 授权含义
- 本留证 + LOCKED 闸门同时成立 → **授权派 workbuddy 按闸门验收**（仅 M2-02：C# / ETW / 预览宿主 v0）。
- 不重开 S1 / R-3 / R-2；不解冻 / 不升级 B_v；不改 composition.h（sha256 a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418）；禁止影子 IV；成功 ≠ 采用；性能 CI ≥2 机另票。
