# T-A1a 门槛 brief（notion_a1a_tracker_brief.md）

> DeepSeek 补洞修订；bak=notion_a1a_tracker_brief.md.bak_pre_ds_patch
> 状态：**CLOSED**（2026-10-24 23:12:41 AWST Australia/Perth）— FAIL 挂账保留；见 §11 / [t_a1a_closed.md](./t_a1a_closed.md)
> **本 brief 不授权采数。** 必须同时满足 APPROVED 和闸门 LOCKED，才可以派 workbuddy。
> 闸门文件：`docs/t_a1a_gate_DRAFT.md`（待协调侧 LOCK 后改名为 `docs/t_a1a_gate_LOCKED.md`）

---

## §0 状态与硬边界（全文保留，不得删改）

- 不重开 R-3；不复活 S1 出货；不升级 Bv；不作 ship；不改 composition.h（sha256 a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418）；不比 #49；不复活 S2。
- A1a 通过 ≠ 重开 S1 / ≠ 采用 Sup-adv；成功 ≠ 采用。
- 本批产出仍不授权采数。须 APPROVED + 闸门 LOCKED 后才可派 workbuddy。LOCKED 由协调侧稍后填写时间戳和 hash，本批只交 DRAFT。

## §1 已知挂账（只引用，不改写）

- #52 a3：A1a @ τ0=21.8，RMS 中位=0.781，FAIL；A1b/A1c/A2=3/3；A v2×3=2/3 → STOP S1。
- #51：A1a FAIL 点 [3.3, 6.8, 8.3, 33.3]；δ0=39.5；未饱和 eff−τ0 中位≈+2.7。
- P3：CONFIRM_F5_REFUTE_2X（med_res_unsat=+2.70；SSE_2x/SSE_const=14.5×），即常数偏移优于 2×。
- N_h=0.41：沿用 #52 A v2（来源：#47 t060rev / #52 闸门文件）。N_h 只用作 A1a/A1b/A1c 的 RMS 中位阈值，**不适用于 A2**（A2 判据是 path=PS）。

## §2 目标

- 只做门槛判定：按 §4.2 写死的修正口径，判断 A1a 在焦点范围内是否达到「修正后稳态」，以及 A-only×3 能否达到 3/3。
- 不做采用决策。A1a 通过不等于重开 S1，也不等于采用 Sup-adv；成功不等于采用。

## §3 范围（焦点网格）

- 主焦点：τ0=21.8。
- 复测 #51 的 FAIL 子集：τ0 ∈ [3.3, 6.8, 8.3, 33.3]。
- 不扩大到上面没给定的网格。#52 原网格中的 {0.0, 16.7, 50.0} 本票不采；原网格定义只在 §4.3 作参照引用。
- 是否先走 analysis-only 复读，见 `t_a1a_offline_review.md`。

## §4 判据

### §4.1 总则
- 会话 A PASS ⇔ A1a ∧ A1b ∧ A1c ∧ A2 全部 PASS。
- A-only×3 PASS ⇔ 3/3 会话 PASS。
- 各单项阈值都沿用 #52 A v2。A1a 只把焦点范围扩展到 T-A1a（§3），单项阈值不变。

### §4.2 修正口径（LOCK 前写死，不留条件式）
- 采用与 P3 一致的口径：**CONFIRM_F5_REFUTE_2X**，即常数偏移优于 2×。
- 判定只按常数偏移口径进行，不提供 2× 口径，也不设「如果采用修正」之类的分支。
- 偏移量引用 P3 / #51 的挂账记录（med_res_unsat=+2.70；未饱和 eff−τ0 中位≈+2.7）。本 brief 不新设数值；闸门 `OFFSET_SOURCE` 写死为：P3 med_res_unsat=+2.70；#51 未饱和 eff−τ0 中位≈+2.7（artifacts/p16_52_p3.txt / p15_51_part1；CONFIRM_F5_REFUTE_2X）。

### §4.3 A1a（焦点扩展于 T-A1a，单项阈值沿用 #52 A v2）
- #52 原文（参照）：τ0 ∈ {0.0, 16.7, 21.8, 33.3, 50.0}；每格 ≥5；RMS 中位 ≤ N_h=0.41；会话 PASS ⇔ 5 格全部 PASS。
- T-A1a 会话判据：范围 τ0 ∈ {21.8, 3.3, 6.8, 8.3, 33.3}。每格 ≥5 reps，且每格 RMS 中位 ≤0.41（修正口径见 §4.2）。
  - PASS：范围内每格都满足。
  - FAIL：任一格 RMS 中位 >0.41（且该格 reps ≥5）。
  - INVALID：任一格 reps <5，或者偏离了 §4.2 的口径。

### §4.4 A1b / A1c / A2（逐项沿用 #52 A v2）

**A1b（沿用 #52 A v2）**
- amp ∈ {+120, +240, −120, −240}；每档 ≥5。
- 负幅度的 Translation 起点 origin=220；正幅度 origin=0。
- 每档 PASS：非出画，且 RMS 中位 ≤ N_h=0.41。
- 出画：amp<0，且 |x_last−(origin+amp)| 过大或样本过少（沿用 #51 Part1 定义，本 brief 不新设阈值）。
- 会话 A1b PASS ⇔ 4 档全部 PASS；任一档出画或 RMS 中位 >0.41 → FAIL。
- 任一档 <5 → INVALID。

**A1c（沿用 #52 A v2）**
- ε ∈ [−40, +40] ms，步长 0.5。ε 撞到边界端点 → 该 trial 记 INVALID。
- 会话中有任一 INVALID trial → A1c=INVALID，不得记 PASS 或 FAIL。
- 双模参数：μ+=+6.5，μ−=−10.0，R=4.0。
- A1c PASS 需同时满足：
  - OTHER 计数=0；
  - 有效 trial ≥5；
  - RMS 中位 ≤0.41；
  - 两模都非空时 |med+−med−| ∈ [14.0, 19.0]；只有单模非空也允许。
- 在没有 INVALID trial 的前提下，上面任一条不满足 → FAIL。
- 禁止用全体 ε 的中位掩盖双模。

**A2（沿用 #52 A v2）**
- path=PS；≥4 trials。
- 闸门判据：全部 trial 的 path=PS。**不是** RMS ≤0.41，N_h 不适用于 A2。
- PASS：≥4 trials 且全部 path=PS。
- FAIL：任一 trial 的 path≠PS。
- INVALID：trials <4。

### §4.5 归因优先级（票级）
按下面的顺序判定，命中即停：
1. 票级 INVALID：任一会话有任一单项 INVALID、流程违规（闸门未 LOCKED 或未 APPROVED 就采数、hash 不符、追加或补测）。走 §4.6。
2. 修正后 A-only×3 <3/3 → 一律归因为 **「修正后未稳态」**，并判票级 FAIL。这条**优先于「不可复现」**，不得改记为不可复现。
3. A-only×3 = 3/3 → 票级 PASS。PASS ≠ 重开 S1，≠ 采用 Sup-adv。

### §4.6 票级 INVALID 处置
- 票级 INVALID 必须开新一轮 LOCK（新时间戳 + 新 body hash）。
- 不得在原闸门上追加；不得补测拼凑；原闸门和原数据只能作废留档。

## §5 闸门流程
1. 本批交付 `docs/t_a1a_gate_DRAFT.md`（STATUS=DRAFT，NOT LOCKED）。
2. 协调侧审阅后填写 APPROVED 留证（§6）。
3. 协调侧 LOCK：填写 LOCKED_AT / GATE_BODY_SHA256，并改名为 `docs/t_a1a_gate_LOCKED.md`。
4. 只有 APPROVED 且 LOCKED 之后，才可以派 workbuddy 采数。

## §6 APPROVED 留证
- APPROVED 的内容和时间戳必须落盘，写在闸门文件或结题文件里。
- 字段：APPROVED_BY / APPROVED_AT / APPROVED_NOTE。本批 DRAFT 只留占位。
- 没有落盘留证的 APPROVED 视同未 APPROVED。

## §7 硬停
- 闸门不是 LOCKED，或者没有 APPROVED 留证 → 不得采数。
- composition.h 的 sha256 与 §0 不符 → 硬停。
- 任何动作触及 R-3 / S1 出货 / Bv 升级 / ship / #49 比较 / S2 → 硬停。
- 发现追加、补测拼凑或口径偏离 §4.2 → 硬停，按 §4.6 处理。

## §8 交付与 ONELINE
- 交付物：本 brief、`t_a1a_offline_review.md`（analysis-only）、`docs/t_a1a_gate_DRAFT.md`。
- ONELINE 模板见闸门 DRAFT。
- 状态：~~OPEN~~ → **CLOSED**（2026-10-24 23:12:41 AWST）；不授权采数（票已结）。

## §9 闸门 LOCK 状态（协调侧追加，2026-10-24 Australia/Perth）
- 闸门已 LOCKED：`docs/t_a1a_gate_LOCKED.md`
- LOCKED_AT=2026-10-24 22:27:54 AWST
- GATE_BODY_SHA256=0750a8402d4118c98025e71863f79bafd689966c2565fa922d0674e8bcf89d1d
- （LOCK 当时）APPROVED_* 占位仍空、闸门 body 未填 → 当时不授权采数；DRAFT 文件保留未改。随后见下一行。
- APPROVED 已落盘（结题文件 `docs/t_a1a_approved.md`）：APPROVED_BY=pidan；APPROVED_AT=2026-10-24 22:34:47 AWST；引用 GATE_BODY_SHA256=0750a8402d4118c98025e71863f79bafd689966c2565fa922d0674e8bcf89d1d / LOCKED_AT=2026-10-24 22:27:54 AWST（闸门 body 未改，哈希不变）。现已授权派 workbuddy（仅 T-A1a）。

## §10 真机结果（2026-10-24 23:05 AWST）
- 结果：**ticket=FAIL**（G4-2，非 INVALID），A-only×3=0/3，归因=修正后未稳态；详见 [`t_a1a_result.md`](./t_a1a_result.md)。
- A1a：τ0=21.8/33.3 PASS；3.3/6.8/8.3 三会话 FAIL（RMS_gate 1.22–1.38 ＞ 0.41）。A1c/A2 3/3 PASS；A1b 仅 S2 PASS。
- 闸门/哈希 PRE+POST OK；composition.h 未改。成功≠采用；不重开 S1/R-3。

## §11 结题（CLOSED）

用户决策（2026-10-24 23:12:41 AWST Australia/Perth）：**关 T-A1a（挂账保留）**。

- **ticket = FAIL**（G4-2）；A-only×3=0/3；归因=**修正后未稳态**。
- Peer consensus：Gemini + Notion + DeepSeek 皆同意 FAIL + 修正后未稳态 + 关票挂账。
- **不重开 S1**；**不重开 R-3**（保持 CLOSED）；不改 `composition.h`（sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）。
- 结题文件：[t_a1a_closed.md](./t_a1a_closed.md)；真机结果：[t_a1a_result.md](./t_a1a_result.md)。

### 挂账 ledger（显式子弹；未来票拾取，不复活本票）

- **DeepSeek 补洞四条**（新 LOCK 须）：
  1. 小 τ0 残差机制
  2. 具体修正
  3. ≥3 会话三格过，且 21.8/33.3 不回退
  4. 不改 `composition.h`
- **Notion 备注**：验残差–τ0 相关须**另开独立票**（不挂回 T-A1a）。

