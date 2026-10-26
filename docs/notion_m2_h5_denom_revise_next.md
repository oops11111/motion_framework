# M2 下一票决断（M2-H5-DENOM-REVISE CLOSED H5-DENOM-PASS · mode=doc-only · PERF-CI HOLD）

> DECISION_AT=2026-10-26 16:02:00 Asia/Taipei  
> 依据：`docs/notion_m2_h5_denom_revise_pack.md`；`docs/m2_h5_denom_revise_result.md`  
> 本 DENOM gate `d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7` **不改、不重跑**  
> 六 hash：TRUTH `276b97f0…4f69` / PATH-A′ `f17871f1…2362` / PATH-B′ `a1161c77…3a43` / CALIB `09f6414b…793d` / ABS `831818e4…80de` / Δv′ `06467b82…805c` **不翻盘**  
> mode 本票已 CLOSED doc-only；`H5-DENOM-PASS` ≠ 已落地采用 ≠ Δv-PASS

## 结论

**选 A = M2-H5-DENOM-VERIFY**（等价名可 `M2-H5-DENOM-CALIB`；本决断主用 **VERIFY**）。  
DRAFT：`docs/m2_h5_denom_verify_gate_DRAFT.md`（**NOT LOCKED**；**本 next 不授权采数**）。

**一句话为什么**：契约已 doc-only PASS（`v_denom≡v_pre`、比–绝对同源、门槛 KEEP、calib 另批规则写死）；下一步才是「**验证采数 + 新分母下另批 calib**」真机票；Δv″ 继续挂账到验证过比+smoke 后再开。

## 现状

| 项 | 状态 |
|---|---|
| M2-H5-DENOM-REVISE | **CLOSED H5-DENOM-PASS**；mode=doc-only；gate `d891df9f…33c7` 不改 |
| M2-SEED-TRUTH-CONFLICT | CLOSED SEED-TRUTH-PASS；menu=M1；gate `276b97f0…4f69` 不翻盘 |
| M2-WGC-SEED-PATH-A′ | CLOSED SEED-PATHA-PRIME-FAIL；`f17871f1…2362` 不翻盘 |
| M2-WGC-SEED-PATH-B′ | CLOSED SEED-PATHB-FAIL；`a1161c77…3a43` 不翻盘 |
| M2-WGC-SEED-CALIB | CLOSED SEED-CALIB-FAIL；`09f6414b…793d` 不翻盘 |
| M2-WGC-SEED-ABS | CLOSED SEED-ABS-FAIL；`831818e4…80de` 不翻盘 |
| M2-Δv′ | CLOSED FAIL；`06467b82…805c` 不翻盘 |
| M2-Δv″ | 继续挂账；**不立刻开** |
| M2-PERF-CI | **HOLD**；不降级 |
| M3 | 不开真机 |

## 候选速判

| 选项 | 决断 | 理由 |
|---|---|---|
| **A M2-H5-DENOM-VERIFY** | **采纳** | 契约已 PASS；须新闸门下验证采数 + 另批 calib |
| B 立刻 Δv″ | **否** | 禁立刻；须验证过比+smoke 后再论 |
| C 默认可落地 / 无新闸门改分母 | **否** | doc-only PASS ≠ 落地；须新 LOCK+APPROVED 显式字段 |
| D 静默沿用旧 calib=0.7511 | **否** | 禁静默 0.7511；旧值钉 2F |
| E 抬 thr / 降 17/20 | **否** | 禁抬 thr；不治定义 |
| F 翻盘六 hash / 本 DENOM | **否** | 禁翻盘 |
| G PERF-CI / 开 M3 | **否** | HOLD；不开 M3 |
| H 解冻 Bv / 重开 S1/R-3/R-2 | **否** | 硬禁 |

## 可 LOCK 要点（给同伴 / Notion · 写死）

须**新闸门**；APPROVED 须**显式**：

1. **验证采数=是**（写死目的、格表、会话数；推荐单机；≥3 独立进程）。  
2. **落地=否**（或另字段单独写死；默认本 VERIFY 不落地启用）。  
3. **calib=新常量或标定式**（推荐 `median(v_denom/v_hat)` 或写死常量表）；**禁静默 0.7511**。  
4. **KEEP `v_denom≡v_pre`**（二次 LS · w=5 · 同 QPC · 同 `t_int`）；旧 2F 不作门控分母。  
5. **禁抬 thr**；禁降 17/20。  
6. **六 + 本 DENOM hash 不翻盘**（五 FAIL + TRUTH `276b97f0…4f69` + DENOM `d891df9f…33c7`）。  
7. **单机**；**PERF-CI HOLD**；不开 M3；不解冻 Bv；不重开 S1/R-3/R-2；不改 composition.h（`a55d4952…e418`）；禁影子 IV；禁立刻 Δv″。  
8. 结题标签建议 `H5-DENOM-VERIFY-*`（或等价）；**≠** Δv-PASS ≠ 六 hash 翻盘 ≠ 默认可落地。

### 否决虚 PASS / 偷换

| 反例 | 否决 |
|---|---|
| 立刻 Δv″ | INVALID / 否决 |
| 默认可落地（无显式落地=是） | INVALID |
| 静默沿用旧 calib=0.7511 | INVALID |
| 翻盘六 hash / 本 DENOM hash | INVALID |
| 未 LOCK+APPROVED 即采数或落地 | INVALID |
| 抬 thr / 降 17/20 | INVALID |
| 把本 DRAFT 当已授权采数 | 否决 |
| 开 M3；解冻 Bv；改 composition.h；影子 IV | INVALID |

## DRAFT 路径

`docs/m2_h5_denom_verify_gate_DRAFT.md`（已落盘骨架；**NOT LOCKED**；**不授权采数**）。

## 协调下一步

派同伴按 DRAFT 补洞（验证采数范围 / 新 calib 标定式 / KEEP v_denom≡v_pre / 禁抬 thr / 六+DENOM 不翻盘 / 落地默认否 / 单机 / PERF-CI HOLD）→ Notion 可 LOCK → LOCK → pidan APPROVED（须显式：验证采数=是；落地=否或另字段；calib=新常量或标定式）。  
**禁**在 LOCK+APPROVED 前采数或改分母落地。  
**禁**把 doc-only `H5-DENOM-PASS` 误读成已落地采用 / Δv-PASS / 可立刻 Δv″。

## 禁令

不重开 S1/R-3/R-2；不改 composition.h（`a55d4952…e418`）；不解冻 Bv；禁影子 IV；成功≠已落地采用≠Δv-PASS；**不改/不重跑** `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362` / `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43` / `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d` / `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de` / `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c` / TRUTH `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69` / 本 DENOM `d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7`；PERF-CI HOLD；不开 M3；禁抬 thr；禁立刻 Δv″；禁静默 0.7511；本 next **不**授权采数。

— notion ai（下一票=M2-H5-DENOM-VERIFY）
