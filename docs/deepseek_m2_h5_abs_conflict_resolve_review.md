# DeepSeek · M2-H5-ABS-CONFLICT-RESOLVE 可 LOCK 补洞（比过 smoke 灭 · 处置菜单）

- 时间：2026-10-26（Asia/Taipei, UTC+8）
- 状态：**文档 only**；NOT LOCKED；**禁采数**
- **出处说明**：**Harness 空回代拟**；对照 `docs/m2_h5_abs_conflict_resolve_gate_DRAFT.md`、`docs/m2_h5_denom_verify_result.md`（H5-DENOM-VERIFY-FAIL）、`docs/notion_m2_h5_denom_verify_pack.md` / `next.md`（主推 C 挂账；次选 D=本票）起草。
- composition.h sha 锁：`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（未改、不改）
- **声明**：VERIFY-FAIL 归档保留；本补洞 ≠ 授权采数；成功 ≠ 把 FAIL 改 PASS ≠ Δv-PASS。

## 禁令复述（文首）

不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；禁抬 thr / 降 17/20；禁立刻 Δv″；禁翻盘八 hash（五 FAIL + TRUTH `276b97f0…4f69` + DENOM `d891df9f…33c7` + VERIFY `fd00a060…889b`）；禁翻 C1 exact=`1.3358225694150683`；禁 LOCK 前采数；≠PERF-CI；成功≠采用；结题主用 `H5-ABS-RESOLVE-*`。

---

## 0) 已证事实（议题前提 · 写死）

| 项 | 值 |
|---|---|
| VERIFY | CLOSED **H5-DENOM-VERIFY-FAIL**；gate `fd00a060…889b` |
| 分母 | KEEP **`v_denom ≡ v_pre`** |
| calib | C1 exact=**`1.3358225694150683`**（n=120；≠0.7511） |
| 路径 | PATHB-A；P-VS=OFF；A′ 未开 |
| H5 比 | **三项齐**（med_r≈0.99–1.16；无 r_i≥2） |
| smoke | **三项皆 ≪17/20**（s1 10/8；s2 0/9；s3 12/9）；pass_sessions=**0/3** |
| 对照 | KEEP |
| 含义 | 分母同源 + C1 后**相对比可过、绝对 smoke 仍系统性灭**；与 SEED-TRUTH 核心矛盾**未解**；非「只差再跑一遍」 |

---

## 1) 处置菜单（可 LOCK）

### 推荐主决议 **R2 = 挂账确认**

与 Notion next **主推 C** 对齐：显式确认「比–绝对冲突」挂账旁路；保留 `H5-DENOM-VERIFY-FAIL`；主线不因本 FAIL 强行再采 / 再 VERIFY；Δv″ / PERF-CI 继续 HOLD；**不开采数**。

| ID | 选项 | 决断 | 说明 |
|---|---|---|---|
| **R2** | **挂账确认** | **推荐主决议** | 旁路写死；不挫主线；无新假设不虚构再跑 |
| **R1** | **STOP** | 可选（等价强归档） | 停止在现行 thr/17/20 下继续追 H5 绝对齐；与 R2 同向，措辞更硬 |
| **R3** | **另开绝对门定义票** | 次选（须另闸门） | 仅当有**具体新假设**（绝对 smoke 的定义/几何/估点——**仍禁抬 thr / 禁降 17/20**）；本票只决议「开/不开」占位，**本票不开采数** |
| R4a | 抬 thr / 降 17/20 | **否决** | 硬禁；不治矛盾 |
| R4b | 立刻 Δv″ | **否决** | 比+smoke 未齐 |
| R4c | 重跑 VERIFY / 翻八 hash / 把 FAIL 当 PASS | **否决** | 禁翻盘 |
| R4d | 默认同跑 A′ / 另批 calib 式追 smoke | **否决** | next 已排除；比已齐，改式易钓鱼；SEED A′ 已耗尽 |
| R4e | 翻 C1 exact / 静默 0.7511 | **否决** | 禁翻本次 C1 |
| R4f | 默认可落地 / 改 composition.h / 影子 IV / 开 M3 / 解冻 Bv | **否决** | 硬禁 |

**一句话**：主决议 **R2 挂账确认**（可与 R1 同批声明「STOP+挂账」）；R3 仅在 Notion/协调另有绝对门定义假设时启用占位。

---

## 2) mode / 采数 / KEEP

| 项 | 写死 |
|---|---|
| mode | **doc-only** |
| 验证采数 | **否** |
| 落地 | **否** |
| v_denom≡v_pre | **KEEP**（本票不改定义） |
| C1 exact | **KEEP 不翻** `1.3358225694150683` |
| thr / 17/20 | **KEEP**；禁抬 / 禁降 |
| composition.h | **KEEP** `a55d4952…e418` |
| 影子 IV | **禁** |
| Δv″ | **继续挂账**；禁立刻开 |
| PERF-CI | **HOLD** |

---

## 3) 八 hash 不翻盘（全文）

1. PATH-A′ FAIL `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362`
2. PATH-B′ FAIL `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`
3. SEED-CALIB FAIL `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`
4. SEED-ABS FAIL `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`
5. Δv′ FAIL `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`
6. TRUTH gate `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69`
7. DENOM gate `d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7`
8. VERIFY gate `fd00a06058b239abfe03a52175463a52741718318ce99416a4ce762cf037889b`

---

## 4) 虚 PASS / INVALID 否决表

| 行为 | 处置 |
|---|---|
| 抬 thr / 降 17/20 救 VERIFY | INVALID |
| 立刻 Δv″ | INVALID |
| 重跑/改写八 hash；把 VERIFY-FAIL 标 PASS | INVALID |
| 翻 C1 exact / 静默 0.7511 | INVALID |
| 未 LOCK+APPROVED 采数；把 DRAFT 当授权 | INVALID |
| 主决议落在 R4* | INVALID |
| 改 composition.h；影子 IV；开 M3；解冻 Bv | INVALID |
| 产物作 PERF-CI 合格证据 | INVALID |

---

## 5) 结题标签 / 产物

- 结局 ∈ {`H5-ABS-RESOLVE-PASS`, `H5-ABS-RESOLVE-FAIL`, `H5-ABS-RESOLVE-INVALID`, `H5-ABS-RESOLVE-STOP`}
- **PASS** = doc-only 菜单完备且主决议 ∈ {R1,R2,R3}；**≠** VERIFY-PASS 改写；**≠** Δv-PASS
- 前缀：`m2_h5_abs_conflict_resolve_*` only
- 闸门：`docs/m2_h5_abs_conflict_resolve_gate_DRAFT.md`（本补洞配套）

---

## 6) 可 LOCK？

### **是 — 可 LOCK**

议题（比过 smoke 灭）、菜单 R1/R2/R3 + R4 否决、推荐 **R2**、mode=doc-only、八 hash、禁抬 thr、禁立刻 Δv″、禁翻 C1、标签与产物前缀均已写死。

### 最小修订点（LOCK 粘贴）

1. APPROVED：mode=doc-only；主决议=**R2**（或 R1 / R3）；采数=否；禁抬 thr；八 hash 不翻盘；禁立刻 Δv″；禁翻 C1。  
2. 正文粘贴 §0 已证事实 + §1 菜单。  
3. 结题 `H5-ABS-RESOLVE-*`；明确 ≠ 把 VERIFY-FAIL 改 PASS。

### G4 ONELINE（建议）

`M2-H5-ABS-CONFLICT-RESOLVE | gate=<hash> @ <LOCKED_AT Asia/Taipei> | approved=<who>@<when> | mode=doc-only | ticket=H5-ABS-RESOLVE-* | menu=R2 | 比过smoke灭已证；禁抬thr；八hash不翻盘；禁立刻Δv″；禁翻C1；成功≠VERIFY-PASS改写≠Δv-PASS；≠PERF-CI；不授权采数`

---

## 回执摘要（给协调）

| 项 | 值 |
|---|---|
| DRAFT | `docs/m2_h5_abs_conflict_resolve_gate_DRAFT.md` |
| 补洞 | `docs/deepseek_m2_h5_abs_conflict_resolve_review.md` |
| 可 LOCK | **是** |
| 推荐 menu | **R2 挂账确认**（对齐 Notion 主推 C） |

— DeepSeek网页版（M2-H5-ABS-CONFLICT-RESOLVE 补洞；Harness 空回代拟；文档 only）
