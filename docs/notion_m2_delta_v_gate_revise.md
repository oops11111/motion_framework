# M2-Δv 闸门补洞决断（第二刀 · 可 LOCK · 并入 Claude INTERIM）

> DECISION_AT=2026-10-25 23:33:39 AWST（Australia/Perth）  
> **刀次：第二刀**（应协调 STOP：第一刀 DECISION_AT=23:29:52 / mtime≈23:30 仍写「Claude 未落盘」，与已落盘 INTERIM `docs/claude_m2_delta_v_review.md` @ 23:29:27 冲突 → 按预留「实质门槛冲突另开 revise」本刀覆盖）  
> 依据：Gemini / DeepSeek / **Claude INTERIM NEEDS-REVISE** / `docs/notion_m2_parallel_next.md`  
> 完整可 LOCK 正文：`docs/m2_delta_v_gate_DRAFT.md`（本刀覆盖；NOT LOCKED）  
> **不授权采数**；单机即可。  
> Claude Free→2026-10-26 16:20 AWST；明日 FINAL 仅对照。  
> **本刀无挂账装 LOCK**：下列冲突项全部 **采纳写死**；无一推旁路。

## 协调 STOP 实质冲突 · 逐条（= Claude 洞）

| STOP# | 冲突 | 第二刀决断 |
|---|---|---|
| 1 | 速度估计器未定义 | **采纳**：每侧二次 LS，**w=5**，外推到 t_s；`v_ref,i=v_pre(t_s−)`；禁两帧差分 / 禁 naive 线性窗 |
| 2 | Nv_h 测法不够 | **采纳**：须在**未打断 base、同估计器、同内容时刻、打断臂之前**测；`Nv_h_cell`=该格 base Δv 的 **P95**（不再写「harness 给出」） |
| 3 | 会话 ≥3 vs ≥2 | **采纳 ≥3**；每会话须过；禁池化 / 为翻盘重跑 |
| 4 | 删「中位≤阈中位」 | **采纳删除**；格 PASS 只留 ≥17/20 + 报 r_i |
| 5 | 臂+对照 / IV_NULL0 踩雷 | **采纳**：被测=`Arm-WGC-seed`；正对照 base 须 PASS；负对照 **IVel=0** 在 t060 fwd/rev 须 FAIL（**非 PASS 候选**）；`IV_NULL0` **仅**负对照变体，**禁止**当被测合法 seed |
| 6 | t_s 写死 | **采纳**：`t_s`=窗 `W=[τ_{k_c−1},τ_{k_c}]` 内 `|x_post−x_pre|` 最小点（QPC+φ）；报 σ_ts |
| 7 | t300 dbl 定义 + vs #47 | **采纳**：dbl 取**第二次**切换 `t_s2`；vs #47 四格 = **有意 +dbl**；票 PASS **5/5** |
| 8 | 试验有效性 + 预登记 | **采纳**：G0 / 亮屏 / crop / 无丢帧；`hang_cap` 默认提案 2；采数前 script+param **prereg_hash** |

## 保留（三家一致）

- 绝对阈 **KEEP** `max(20 DIP/s, 0.05·|v_ref|)`
- t156 SKIP = **规则导出 ACCEPT**（可判规则失败 → SKIP；禁硬标签覆盖）
- PERF-CI HOLD `5b007dd4…282a` **不改不降级**；本票 ≠ PERF-CI
- 禁令：不重开 S1/R-3/R-2；不改 composition.h `a55d4952…e418`；禁影子 IV；不解冻 Bv；成功≠采用；不开 M3 真机

## 可判规则（写死，非标签）

可判 ⇔ `thr ≥ 2·Nv_h_cell` ∧ `|a(t_s)|·σ_ts ≤ thr/2`。  
预期 t156=timing-bound SKIP；t300=floor-bound 可判。规则与命名冲突 → **升闸门版本**。

## 票结局

`Δv-PASS` / `Δv-FAIL` / `Δv-INCONCLUSIVE`（每会话 5/5 + 对照行为）。

## 挂账清单

**无。** 本刀不把任何 Claude 实质洞推到 `side_parked_hang_degrade.md`。

## 相对第一刀（23:29:52）变更摘要

第一刀（Gemini+DeepSeek only）缺：曲率修正估计器、base-P95 Nv_h、≥3 会话、删中位句、臂/对照与 IVel 记号、t_s 操作定义、dbl/t_s2、G0/crop/prereg。第二刀全部写死进 DRAFT。

## 结论

**可 LOCK。**（第二刀正文已覆盖 DRAFT；请按本刀 DECISION_AT / 文件 mtime 哈希，**勿**再用 23:29:52 稿。）

请协调侧 LOCK → 问 pidan APPROVED（单机；提醒 PERF-CI 仍需第二台）。

— notion ai（M2-Δv 第二刀 · 应 STOP）
