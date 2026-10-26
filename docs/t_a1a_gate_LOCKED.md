# T-A1a 闸门（t_a1a_gate_LOCKED.md）

STATUS=LOCKED

LOCKED_AT: 2026-10-24 22:27:54 AWST
GATE_BODY_SHA256: 0750a8402d4118c98025e71863f79bafd689966c2565fa922d0674e8bcf89d1d
> GATE_BODY_SHA256 计算约定（与 artifacts/p16_52_a_v2_gates.txt 一致）：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。本行写入后不得再改 body。

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿
APPROVED_NOTE: ＿＿＿＿
> APPROVED 的内容和时间戳必须在本文件或结题文件落盘；没有落盘视同未 APPROVED。**本 LOCK 未授权采数；APPROVED_* 仍空。**

OFFSET_SOURCE: P3 med_res_unsat=+2.70；#51 未饱和 eff−τ0 中位≈+2.7（artifacts/p16_52_p3.txt / p15_51_part1；CONFIRM_F5_REFUTE_2X）
OFFSET_METHOD（估计方法写死，非冻结单一魔术数）:
- 残差定义沿用 P3：residual = fitted_shift − τ0 = (δ0 − δ_rest) − τ0；δ0(τ0=0)=39.5 ms。
- 未饱和子集：δ_rest>0.5 且 τ0>0（τ0=0 为参考点，不进残差中位）。
- 常数偏移量 = median(residual | 未饱和子集)，引用挂账 med_res_unsat=+2.70 / ≈+2.7 作对照，**不**另发明数值。
- 单点 residual 可散（P3 未饱和范围约 −2.3…+9.5）；判定用中位法，不得改用单点魔术数或改回 2×。
- 偏离本方法 → INVALID（见 G2 / G5）。

**本 LOCK 不授权采数。只有 APPROVED + LOCKED 之后才可以派 workbuddy。**
**本 LOCK 不清 A1a；不重开 S1 / R-3；不升级 Bv；不作 ship；成功 ≠ 采用。**

---

## G0 硬边界
- 不重开 R-3；不复活 S1 出货；不升级 Bv；不作 ship；不改 composition.h（sha256 a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418）；不比 #49；不复活 S2。
- A1a 通过 ≠ 重开 S1 / ≠ 采用 Sup-adv；成功 ≠ 采用。
- 本闸门 LOCK ≠ 清 A1a 挂账；≠ 授权采数（须 APPROVED_* 落盘非空）。

## G1 总则
- 会话 A PASS ⇔ A1a ∧ A1b ∧ A1c ∧ A2 全部 PASS。
- A-only×3 PASS ⇔ 3/3。
- 单项阈值沿用 #52 A v2。N_h=0.41（来源：#47 t060rev / #52 闸门文件）只用于 A1a/A1b/A1c 的 RMS 中位，不用于 A2。

## G2 修正口径（写死）
- 采用 CONFIRM_F5_REFUTE_2X：常数偏移优于 2×（P3：med_res_unsat=+2.70；SSE_2x/SSE_const=14.5×）。
- 只按常数偏移口径判定，无条件分支，无 2× 备选。偏移量按上方 OFFSET_METHOD 估计；偏离本口径 / 本方法 → INVALID。

## G3 单项判据

### A1a（焦点扩展于 T-A1a，单项阈值沿用 #52 A v2）
- #52 原文参照网格：τ0 ∈ {0.0, 16.7, 21.8, 33.3, 50.0}；每格 ≥5；RMS 中位 ≤0.41；5 格全部 PASS。
- 本票焦点网格（写死）：τ0 ∈ {21.8, 3.3, 6.8, 8.3, 33.3}，不扩大。
  - 说明：#51 FAIL 点 {3.3, 6.8, 8.3} **不在** #52 A1a 网格内；#52 唯一记录的 A1a FAIL 格为 21.8；33.3 两边共有。两次 FAIL 格位不重合，本票同时覆盖。
  - #52 原网格中的 {0.0, 16.7, 50.0} 本票不采。
- PASS：每格 ≥5 reps，且每格 RMS 中位 ≤0.41（修正口径见 G2 / OFFSET_METHOD）。
- FAIL：任一格 RMS 中位 >0.41。
- INVALID：任一格 <5 reps，或者口径偏离 G2。

### A1b（沿用 #52 A v2）
- amp ∈ {+120, +240, −120, −240}；每档 ≥5。
- origin：负幅度=220，正幅度=0。
- 每档 PASS：非出画，且 RMS 中位 ≤0.41。
- 出画：amp<0，且 |x_last−(origin+amp)| 过大或样本过少（沿用 #51 Part1）。
- PASS ⇔ 4 档全部 PASS；FAIL：任一档出画或 RMS 中位 >0.41；INVALID：任一档 <5。

### A1c（沿用 #52 A v2）
- ε ∈ [−40, +40] ms，步长 0.5。ε 撞界端点 → trial INVALID。
- 会话中有任一 INVALID trial → A1c=INVALID（不得记 PASS 或 FAIL）。
- 双模：μ+=+6.5，μ−=−10.0，R=4.0。
- PASS 需同时满足：
  - OTHER=0；
  - 有效 trial ≥5；
  - RMS 中位 ≤0.41；
  - 两模都非空时 |med+−med−| ∈ [14.0, 19.0]；只有单模非空也允许。
- 在没有 INVALID trial 时，上面任一条不满足 → FAIL。
- 禁止用全体 ε 的中位掩盖双模。

### A2（沿用 #52 A v2）
- path=PS；≥4 trials。
- PASS：全部 path=PS（不是 RMS ≤0.41）。
- FAIL：任一 path≠PS。
- INVALID：trials <4。

## G4 归因优先级（票级，按顺序，命中即停）
1. 票级 INVALID：任一会话有任一单项 INVALID，或者出现流程违规（未 LOCKED 或未 APPROVED 就采数、hash 不符、追加、补测）。
2. 修正后 A-only×3 <3/3 → 「修正后未稳态」，判票级 FAIL。优先于「不可复现」。
3. A-only×3 = 3/3 → 票级 PASS（≠ 重开 S1，≠ 采用 Sup-adv，成功 ≠ 采用）。

## G5 票级 INVALID → 新一轮 LOCK
- 必须开新闸门：新 LOCKED_AT + 新 GATE_BODY_SHA256。
- 不得在本闸门追加；不得补测拼凑。本轮数据作废留档。

## G6 硬停
- STATUS≠LOCKED，或 APPROVED_* 为空 → 不得采数，不得派 workbuddy。
- composition.h 的 sha256 ≠ a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418 → 硬停。
- 触及 R-3 / S1 出货 / Bv 升级 / ship / #49 比较 / S2 → 硬停。
- 闸门 body 的 hash 与 GATE_BODY_SHA256 不符 → 硬停，按 G5 处理。

## G7 ONELINE 模板
T-A1a | gate=<GATE_BODY_SHA256> @ <LOCKED_AT> | approved=<APPROVED_BY>@<APPROVED_AT> | corr=CONFIRM_F5_REFUTE_2X | S1:A1a=<P/F/I> A1b=<P/F/I> A1c=<P/F/I> A2=<P/F/I> | S2:… | S3:… | A-only×3=<k>/3 | ticket=<PASS/FAIL/INVALID> | 归因=<修正后未稳态/—> | 成功≠采用；不重开 S1
