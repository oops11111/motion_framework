# R-3 #50 中文包（notion ai，2026-10-24 Asia/Perth）

> **覆盖注记（Claude 定稿后）：** 权威复核见 `docs/claude_r3_50_review.md`。**Verdict：常量 ε̂ S1 = NO-ADOPT；S1 程序 = REVISE；R-3 仍 OPEN。** §4 已改为「Claude 已定稿」；§3 #51 已按 Claude 条件扩成 Part1（有效超前）+ Part2（c×3）；**S2 移出关键路径**；S1-φ / 多 retarget **暂缓**。Claude Free 撞限至页面约 **6:50 AM**，暂勿再派 Claude。workbuddy 已按扩订续跑 #51。
>
> **ONELINE（#50 实测，不变）：** 闸门 A=本 full raw **FAIL**（同日 AB-only A=**PASS**）；S2 后 ε=**6.2 ms**；扫描用 AB 的 ε̂∈{**0, 6.8, 16.7**}；最佳 ε̂=**6.8**；J_vis=**YES**（三档×四 cell）；分流=**记录 C / R-3 仍 OPEN / 不采用**。`composition.h` sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` **未改**。
>
> 权威数字出处：`artifacts/p14_50.txt`、`p14_50_ab.txt`、`docs/r3_commit_staleness.md` ## #50；定稿：`docs/claude_r3_50_review.md`。成功 ≠ 采用。硬约束：不改 composition.h；不合成输入。

## 1. #50 一页摘要

### 1.1 门控表

| 阶段 | 本 full raw（wall≈19:08 AWST） | 同日 AB-only（wall≈18:45，无 C） | 备注 |
| --- | --- | --- | --- |
| **A 总闸门** | **FAIL** | **PASS** | 同日不一致 → A 未稳，单次 A 不得作终裁 |
| A1a 种子平移 | FAIL（τ0=21.8 中位 RMS=1.823 > N_h=0.41；其余 τ0 PASS） | PASS（含 τ0=21.8） | full 偶发坏格 |
| A1b Δ 线性 | FAIL（+amp RMS 2.0–2.6；负 amp 出画，早窗配对=False） | PASS（负 amp 仍出画，但早窗配对=True → 格门 PASS） | 负 amp 出画是 WGC/画面边，非产品路径结论 |
| A1c 叠加 | FAIL（rep1–3 坏 trial，RMS≈1.77、ε_fit=-5.0） | PASS（中位过门；仅 rep4 单次坏） | 偶发坏 trial |
| A2 ε 路径 | PASS（path=PS，ε 中位 9.0 ms，n=8） | PASS（同） | d0/d1 同路径可测 |
| **B Sup+S2** | OK（192/192） | OK（192/192） | 见下 |
| Sup ε 中位 | 7.0 ms | 6.8 ms | |
| SupS2 ε 中位 | **6.2 ms** | 6.8 ms | \|Δ\| vs Sup = **0.8 ms** → **S2 未明显改 ε** |
| Bv J 中位 | 16.09 | 16.01 | 对照；Bv 仍降级史 |
| **C S1 主矩阵** | 256/256；ε̂ 扫描 **{0, 6.8, 16.7}**（按 AB 重定，非本 raw 候选 6.2） | （未跑） | LIVE_TERMS_CAP=3 |
| **J_vis 出货门** `J ≤ d_frame + N_h_eff`（#47；禁 ≤2×Sup） | **YES**（三档×四 cell 全 PASS） | — | 最佳 ε̂=**6.8**（J 中位约 0.53–1.97） |

**有符号 e（ε̂=6.8，偏超前）：** φ 分档中位约 **0.8 / 4.8 / 8.3 / 11.8 ms**。ε̂=0 偏滞后（约 −13…−1）；ε̂=16.7 大超前≈21.7。fit/hold-out 一致（6.5 / 6.3）。

### 1.2 异常（必须写清）

1. **A 不稳定：** 同日 full 与 AB-only 阶段 A 结论相反。full 的 A1a@21.8、A1c 偶发坏 trial、负 amp 出画导致 A=FAIL；AB-only A=PASS。不得把单次 full A 当最终闸门。
2. **S2 收益弱：** Sup→SupS2 \|Δε\|=0.8 ms，标定口径「S2 后明显变化」= **NO**；扫描仍沿用 AB 档 {0, 6.8, 16.7}。
3. **J_vis 过门 ≠ 采用：** C 数据可记录；站立规则要求 **A 稳定 PASS + Claude 复核齐备** 前禁止采用任何 R-3 变体。

### 1.3 不采用理由（本包草案）

- conditional GO 的前置闸门 A **未稳定通过**（会话间翻牌）。
- S2 作为 S1 前置的 ε 重标定收益 **未成立**（未明显改 ε）。
- 虽最佳 ε̂=6.8 过 J_vis，且有符号 e 仍系统偏超前，**成功 ≠ 采用**；R-3 保持 **OPEN**，不改 composition.h，不宣布 Sup-adv / S1 入产品。

---

## 2. 给 Claude 复核的中文提纲（已答）

对照：`docs/claude_r3_design_review.md`（设计侧 REVISE）+ `docs/notion_r3_redesign.md`。

**Claude #50 定稿已到**（`docs/claude_r3_50_review.md`）：**NO-ADOPT**（常量 ε̂）；程序 **REVISE**；#51 **GO（有条件）**。下表保留作对照清单。

| # | 对照点 | #50 实测 | 请你判定 |
| --- | --- | --- | --- |
| 1 | conditional GO：A 闸门过才开主矩阵 | full A=FAIL，但 C 已齐；AB-only A=PASS | A 翻牌时，C 的 J_vis=YES 能否算数？是否强制 A×N 稳定后再谈采用？ |
| 2 | S2 前置 → 按新 ε 重定 ε̂ | \|Δε\|=0.8，**未明显变化**；扫描用 AB 的 6.8 非本 raw 6.2 | S2 前置是否仍必要？ε̂ 网格是否改？ |
| 3 | 出货门改为 J_vis≤d_frame+N_h_eff；废 ≤2×Sup | 三档×四 cell **全 YES**；最佳 6.8 | 门是否过松？是否加有符号 e / hop 硬顶？ |
| 4 | 残差对 e **一阶**；落地 hop 写代价 | ε̂=6.8 时 e 分档 0.8–11.8 ms **偏超前**；ε̂=16.7 e≈21.7 | 常量 ε̂ 是否够用？要否 φ 相关 / 封顶一帧？ |
| 5 | 多 cell + φ hold-out | 四 cell；fit/hold-out 6.5/6.3 一致 | 是否足以支持「可标定」结论？ |
| 6 | 成功 ≠ 采用；R-3 OPEN | 本包默认 **不采用** | 确认 NO-ADOPT 或给出 REVISE 条件 |

**请勿：** 改 composition.h；把「再跑一遍 E3」当主方案；用合成输入；把 J_vis=YES 直接写成 ADOPT。

---

## 3. #51 brief（Claude 条件；覆盖原「仅 A×3」草稿）

> 原三选一草稿作废为历史；以 Claude FINAL「#51: GO, with these conditions」为准。workbuddy 已按扩订续跑。

### Part 1 — A 稳定性 + **有效超前**标定（关键路径）

1. **放宽 τ 重拟合**：对已存 raw 帧放宽 τ 搜索界，核对 A1c 的 `ε_fit=−5` 是否撞界伪失败。
2. **A1a 细网格**：τ0 含帧倍数与非倍数；每值 ≥5 次重复；除形状 RMS 外，报告 **拟合平移 − 命令 τ0（effective advance）**。
3. **A1b 重裁**：保证 −Δ 不出画（修正 WGC crop），使反号重定向可测。

### Part 2 — **c×3** 会话稳定性

- 至少 **3** 个独立会话（或负载态）重复 Sup 的 ε(φ) 分档；同会话交错 Bv / base。
- 看 c 是否在 ≈14 与 ≈30（约差一帧）之间翻牌；无运行时 c 读回前，常量 ε̂ / ε̂(φ) 均不可出货。

### 移出 / 暂缓

| 项 | 处置 | 理由（Claude） |
| --- | --- | --- |
| **S2** | **移出关键路径** | Sup≈SupS2，\|Δε\|=0.8 ms；bind 不是滞后源 |
| **S1-φ** | **暂缓** | 仅当 Part1 解开 advance 因子且 Part2 显示 c 稳定或可估后 |
| **多 retarget / LIVE_TERMS** | **暂缓** | 同上；先不烧机时 |
| **Bv** | 维持已降级出货兜底 | R-2 结题史不变 |

### 硬停（#51 前）

- 不改 composition.h；不改出货配置；禁止仅凭 J_vis 宣称采用。
- 确认 e / ε / J / Δv_fd / resid 一行定义。
- 同会话交错臂（必含 Sup、Bv、base）；记录 G0 与亮屏态；**同会话数字互比，勿与 #49 混比。**

### 原候选对照（归档）

- ~~仅 A×3~~ → 并入 Part1+Part2（Part2 仍要 ×3 会话，但测的是 c/ε(φ)，不是裸 A 闸门）。
- ~~多 retarget 优先~~ → 暂缓。
- ~~R-3 整段冻结~~ → 不采纳；#51 GO（有条件）。

---

## 4. NO-ADOPT — **Claude 已定稿：NO-ADOPT**

> **状态标签：`Claude 已定稿：NO-ADOPT`**（取代原「待 Claude 定稿」）。全文权威：`docs/claude_r3_50_review.md` FINAL。

### 裁决（定稿）

**常量 ε̂ 的 S1 / Sup-adv：NO-ADOPT。** S1 **程序**改为 **REVISE**（见 §3 #51 条件）。R-3 保持 **OPEN**。不改 `composition.h`。不把 #50 的 J_vis=YES 写成产品采用。Bv 仍为已降级出货兜底。

### FINAL 要点摘录

1. **J_vis 不能排序候选**：三档 ε̂（含 ε̂=0）与四 cell 全过门；裸 Sup J=0.26 也过；ε̂=16.7 时 resid P90≈6.4 仍过门 → 「无落地硬折」≠「S1 优于 Sup」。
2. **常量 ε̂=6.8 无净精度收益**：ε̂=0 时 mean \|e\|≈7.0（derived）；ε̂=6.8 时≈6.4（derived），把滞后换成高 φ 超前（超前更危险）。
3. **种子有效超前疑似 ≈2× 命令值**（hypothesis；e 定义待确认）：设计 e=ε̂−ε 与报告值差约一个 ε̂。
4. **ε 跨会话不稳**：#49 Sup ε 中位 21.8 → #50 的 7.0（约 3.1×）；φ 分档暗示 c 约差一帧。无 c 读回前不可出货常量/相位 ε̂。
5. **阶段 A 阻断采用**：A1a 仅非帧倍数 τ0=21.8 成簇 FAIL；A1c 3/5 坏且 ε_fit=−5；A1b −Δ 出画无效、+Δ FAIL；同日 AB-only PASS vs full FAIL → A 不稳即挡住采用（即使 J_vis=YES）。
6. **S2 不必要**：\|Δε\|=0.8 ms，移出关键路径。

### 禁止项（定稿一致）

不改 composition.h；不合成输入；不宣布采用任何 R-3 变体；不用「≤2×Sup」作出货门；不以 J_vis 单独采用；暂勿再派 Claude（Free 撞限至页面约 6:50 AM）。

---

## 附录：路径

| 项 | 路径 |
| --- | --- |
| full 摘要 | `artifacts/p14_50.txt` |
| AB-only | `artifacts/p14_50_ab.txt` |
| raw | `artifacts/p14_50_raw.txt` |
| 运行记录 | `docs/r3_commit_staleness.md` ## #50 |
| 修订规格 | `docs/notion_r3_redesign.md` |
| 设计侧 REVISE | `docs/claude_r3_design_review.md` |
| **#50 Claude 定稿** | `docs/claude_r3_50_review.md` |
| 本包备份（定稿前） | `docs/notion_r3_50_pack.md.bak_pre_claude_50` |
| 探针/脚本 | `/workspace/r3/p50.py`；Win `--r3-50-full` |

— notion ai（#50 数字未改写；§3/§4 按 Claude 定稿覆盖）
