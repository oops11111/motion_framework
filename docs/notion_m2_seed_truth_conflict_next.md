# M2 下一票决断（M2-SEED-TRUTH-CONFLICT CLOSED SEED-TRUTH-PASS · menu=M1 · PERF-CI HOLD）

> DECISION_AT=2026-10-26 15:49:00 Asia/Taipei  
> 依据：`docs/notion_m2_seed_truth_conflict_pack.md`；`docs/m2_seed_truth_conflict_result.md`  
> 本 TRUTH gate `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69` **不改、不重跑**  
> 五 FAIL `f17871f1…2362` / `a1161c77…3a43` / `09f6414b…793d` / `831818e4…80de` / `06467b82…805c` **不翻盘**  
> mode 本票已 CLOSED offline-resolve；勾选 M1 ≠ 已改 H5

## 结论

**选 A = M2-H5-DENOM-REVISE**。  
DRAFT：`docs/m2_h5_denom_revise_gate_DRAFT.md`（**NOT LOCKED**；**本 next 不授权采数**）。

**一句话为什么**：TRUTH 票已 offline 决议勾选 M1——残差是 **2F 比真值 vs 绝对 `v_pre`/smoke** 的定义冲突；下一步须另票写死新分母 / 估窗契约使比–绝对同源，**禁**抬 thr、**禁**翻盘五 FAIL + 本 TRUTH gate。

## 现状

| 项 | 状态 |
|---|---|
| M2-SEED-TRUTH-CONFLICT | **CLOSED SEED-TRUTH-PASS**；menu=M1；gate `276b97f0…4f69` 不改 |
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
| **A M2-H5-DENOM-REVISE** | **采纳** | M1 已勾选；须另票 LOCK 新分母定义后才可执行 |
| B 立刻执行改分母（无新闸门） | **否** | 勾选 M1 ≠ 授权执行；须新 LOCK+APPROVED |
| C 抬 thr / 降 17/20 | **否** | 禁抬 thr；不治定义冲突 |
| D 改 smoke 参照（M2） | **否** | TRUTH 已否决 M2 作主决议 |
| E 新估法族 / 再跑 α（M3） | **否** | 平滑臂已耗尽；TRUTH 否决 M3 |
| F 立刻 Δv″ | **否** | 禁立刻；须新分母路径可过比+smoke 后再论 |
| G 重跑五 FAIL / 本 TRUTH | **否** | 禁翻盘 |
| H PERF-CI / 开 M3 | **否** | HOLD；不开 M3 |
| I STOP（M4 事后改选） | **否** | 本票已 CLOSED PASS；无强制 STOP |

## 可 LOCK 要点（给同伴 / Notion）

1. **新分母定义须 LOCK 后另批**：写死新分母 / 估窗契约、与 t060 / `v_pre` **比–绝对同源**规则、回归门槛。
2. **禁抬 thr**；禁降 17/20 伪装 PASS。
3. **禁翻盘**：五 FAIL 全文 + 本 TRUTH gate `276b97f0…4f69`；禁标 `Δv-PASS` / 前序 `SEED-*-PASS` 翻盘。
4. **默认仍可能需真机**验证新契约，但 **本 next / 本 DRAFT 不授权采数**；采数须该票 LOCK+APPROVED **显式写死范围**。
5. **PERF-CI HOLD**；不开 M3；不解冻 Bv；不重开 S1/R-3/R-2；不改 composition.h（`a55d4952…e418`）；禁影子 IV；禁立刻 Δv″。
6. 结题标签建议 `H5-DENOM-*`（或等价）；**≠** 五 FAIL / TRUTH 翻盘。

### 否决虚 PASS / 偷换

| 反例 | 否决 |
|---|---|
| 未 LOCK+APPROVED 即改分母落地 | INVALID |
| 抬 thr / 降 17/20 救票 | INVALID |
| 重跑/改写五 FAIL 或本 TRUTH gate | INVALID |
| 产物标 Δv-PASS / 前序 SEED-*-PASS / TRUTH 翻盘 | INVALID |
| 未授权采数却派真机 | INVALID |
| 立刻开 Δv″；开 M3；解冻 Bv；改 composition.h；影子 IV | INVALID |
| 把本 DRAFT 当已授权执行 | 否决 |

## DRAFT 路径

`docs/m2_h5_denom_revise_gate_DRAFT.md`（已落盘骨架；**NOT LOCKED**；**不授权采数**）。

## 协调下一步

派同伴按 DRAFT 补洞（新分母定义边界 / 比–绝对同源 / 禁抬 thr / 五+TRUTH 不翻盘 / 默认采数须另批）→ Notion 可 LOCK → LOCK → pidan APPROVED。  
**禁**在 LOCK+APPROVED 前改分母落地或真机采数。  
**禁**把 TRUTH PASS 误读成已改 H5 / Δv-PASS。

## 禁令

不重开 S1/R-3/R-2；不改 composition.h（`a55d4952…e418`）；不解冻 Bv；禁影子 IV；成功≠采用≠Δv-PASS≠已改H5（TRUTH）；**不改/不重跑** `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362` / `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43` / `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d` / `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de` / `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c` / 本 TRUTH `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69`；PERF-CI HOLD；不开 M3；禁抬 thr；禁立刻 Δv″；本 next **不**授权采数。

— notion ai（下一票=M2-H5-DENOM-REVISE）
