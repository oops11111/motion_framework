# M2-H5-DENOM-REVISE APPROVED 留证（结题文件）

> 本文件是闸门 `docs/m2_h5_denom_revise_gate_LOCKED.md` 的 APPROVED 留证。不修改 LOCKED 闸门 body / hash。

## 引用的 LOCK（不变）
- 闸门路径：`docs/m2_h5_denom_revise_gate_LOCKED.md`
- STATUS=LOCKED
- LOCKED_AT=2026-10-26 15:55:00 Asia/Taipei
- GATE_BODY_SHA256=d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7
- 哈希约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。
- 修订审计：`docs/notion_m2_h5_denom_revise_gate_revise.md`
- 同伴稿：`docs/deepseek_m2_h5_denom_revise_review.md`
- 六 hash **不翻盘**：TRUTH `276b97f0…4f69` / PATH-A′ `f17871f1…2362` / PATH-B′ `a1161c77…3a43` / CALIB `09f6414b…793d` / ABS `831818e4…80de` / Δv′ `06467b82…805c`

## APPROVED 留证
- APPROVED_BY=pidan
- APPROVED_AT=2026-10-26 16:00:17 Australia/Perth
- APPROVED_NOTE=widget reply「APPROVED doc-only（采数否/落地否/calib不适用）」（t260s1191）after LOCK d891df9f…33c7；LOCKED_AT=2026-10-26 15:55:00 Asia/Taipei

## G1-D5 范围声明（写死）
| 字段 | 值 |
|---|---|
| mode | **doc-only** |
| 是否授权验证采数 | **否** |
| 是否授权落地 | **否**（不改分母落地；不启用真机新分母） |
| calib | **不适用**（契约定义阶段；禁静默沿用 0.7511；验证/落地须另 APPROVED 显式另批） |
| 会话数 | **0**（不开真机） |
| 格表 | **无** |

## 授权含义
- 本留证 + LOCKED → **仅授权 offline 契约结题**（写 `docs/m2_h5_denom_revise_result.md`；标签倾向 `H5-DENOM-PASS` mode=doc-only）。
- **不授权**真机采数；**不授权**改分母落地；**不授权**新 calib。
- `H5-DENOM-PASS` ≠ 已落地采用 ≠ Δv-PASS ≠ 六 hash 翻盘。
- 不重开 S1/R-3/R-2；不解冻 Bv；不改 composition.h（`a55d4952…e418`）；禁影子 IV；禁抬 thr；禁立刻 Δv″；PERF-CI 仍 HOLD `5b007dd4…282a`。
