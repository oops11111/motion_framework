# M2-H5-ABS-CONFLICT-RESOLVE APPROVED 留证（结题文件）

> 本文件是闸门 `docs/m2_h5_abs_conflict_resolve_gate_LOCKED.md` 的 APPROVED 留证。不修改 LOCKED 闸门 body / hash。

## 引用的 LOCK（不变）
- 闸门路径：`docs/m2_h5_abs_conflict_resolve_gate_LOCKED.md`
- STATUS=LOCKED
- LOCKED_AT=2026-10-26 18:30:00 Asia/Taipei
- GATE_BODY_SHA256=e96bf6238ace42ce2edcfe4191ef5ec45fa53eefca94fcf777f2b52c17425ade
- 哈希约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。
- 修订审计：`docs/notion_m2_h5_abs_conflict_resolve_gate_revise.md`
- 同伴稿：`docs/deepseek_m2_h5_abs_conflict_resolve_review.md`
- 八 hash **不翻盘**：PATH-A′ `f17871f1…2362` / PATH-B′ `a1161c77…3a43` / CALIB `09f6414b…793d` / ABS `831818e4…80de` / Δv′ `06467b82…805c` / TRUTH `276b97f0…4f69` / DENOM `d891df9f…33c7` / VERIFY `fd00a060…889b`

## APPROVED 留证
- APPROVED_BY=pidan
- APPROVED_AT=2026-10-26 20:08:54 Australia/Perth
- APPROVED_NOTE=widget reply「APPROVED mode=doc-only；采数=否；主决议=R2挂账确认」（t269s1242）after LOCK e96bf623…5ade；LOCKED_AT=2026-10-26 18:30:00 Asia/Taipei

## G1 范围声明（写死）
| 字段 | 值 |
|---|---|
| mode | **doc-only** |
| 主决议 menu | **R2 挂账确认**（本批未同选 R1 STOP） |
| 验证采数 | **否** |
| 落地 | **否** |
| 禁抬 thr / 降 17/20 | **是** |
| 八 hash 不翻盘 | **是** |
| 禁立刻 Δv″ | **是** |
| 禁翻 C1 exact | **是**（KEEP `1.3358225694150683`） |
| PERF-CI | **HOLD** `5b007dd4…282a` |
| 会话数 | **0**（不开真机） |
| 格表 | **无** |

## 授权含义
- 本留证 + LOCKED → **仅授权 offline 决议结题**（写 `docs/m2_h5_abs_conflict_resolve_result.md`；结题包/下一票；标签倾向 `H5-ABS-RESOLVE-PASS` mode=doc-only；主决议=R2）。
- **不授权**真机采数；**不授权**抬 thr / 降 17/20；**不授权**立刻 Δv″；**不授权**翻 C1；**不授权**把 `H5-DENOM-VERIFY-FAIL` 改写成 PASS。
- `H5-ABS-RESOLVE-PASS` ≠ VERIFY-PASS 改写 ≠ Δv-PASS ≠ 八 hash 翻盘 ≠ 默认可落地。
- 不重开 S1/R-3/R-2；不解冻 Bv；不改 composition.h（`a55d4952…e418`）；禁影子 IV；PERF-CI 仍 HOLD。
- R3 仅占位；本批不开绝对门定义票；R4* 否决。
