# workbuddy 派题 — T-A1a（门槛采数）

**状态：READY TO RUN**（APPROVED + LOCKED 已齐）
**时区：Australia/Perth（AWST）**

## 权威与引用（必须 verbatim 遵守）

- 闸门（唯一权威）：`docs/t_a1a_gate_LOCKED.md`
  - STATUS=LOCKED
  - LOCKED_AT=**2026-10-24 22:27:54 AWST**
  - GATE_BODY_SHA256=**0750a8402d4118c98025e71863f79bafd689966c2565fa922d0674e8bcf89d1d**
- APPROVED 留证（结题文件，不改闸门 body）：`docs/t_a1a_approved.md`
  - APPROVED_BY=pidan
  - APPROVED_AT 见该文件
  - APPROVED_NOTE=widget reply「APPROVED，派 workbuddy」after LOCK 0750a840…；引用 LOCKED_AT
- 门槛 brief（背景）：`docs/notion_a1a_tracker_brief.md`
- 离线复读（analysis-only，非采数指令）：`docs/t_a1a_offline_review.md`

**歧义时以 LOCKED 闸门原文为准。** 开工前核验闸门 body 哈希（排除以 `GATE_BODY_SHA256` 开头的那一行）必须等于上列值；不符 → 硬停，按 G5 开新闸门，不得采数。

## 票范围（只做 T-A1a）

- **只做票 T-A1a**：按闸门判定「修正后稳态」与 A-only×3 能否 3/3。
- **不做**：重开 R-3；复活 S1 出货；升级 Bv；ship；改 `composition.h`；比 #49；复活 S2；把 PASS 解读为「采用 Sup-adv」。
- **成功 ≠ 采用**。A1a 通过 ≠ 重开 S1 ≠ 采用。
- R-3 已 CLOSED；**禁止**重开 S1 / R-3。

## 机器与入口

- 在 Windows **DESKTOP-K93N3P8** 上经 **CodeBuddy CLI** 执行。
- 运行前后各核一次 `code/composition.h` sha256，必须保持：
  `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`
  不符 → 硬停（G6）。

## G0 / G6 硬边界（摘抄闸门）

- 不重开 R-3；不复活 S1 出货；不升级 Bv；不作 ship；**不改 composition.h**；不比 #49；不复活 S2。
- STATUS≠LOCKED，或无 APPROVED 留证 → 不得采数。
- 闸门 body hash ≠ GATE_BODY_SHA256 → 硬停，按 G5。
- 触及 R-3 / S1 出货 / Bv 升级 / ship / #49 / S2 → 硬停。

## 总则（G1）

- 会话 A PASS ⇔ **A1a ∧ A1b ∧ A1c ∧ A2** 全部 PASS。
- A-only×3 PASS ⇔ **3/3**。
- 单项阈值沿用 #52 A v2。**N_h=0.41**（来源：#47 t060rev / #52 闸门）**只用于 A1a/A1b/A1c 的 RMS 中位，不用于 A2**。

## 修正口径 OFFSET_METHOD（G2；写死，勿发明）

采用 **CONFIRM_F5_REFUTE_2X**：常数偏移优于 2×。只按常数偏移判定；无条件分支；无 2× 备选。

估计方法（闸门原文）：

1. 残差定义沿用 P3：`residual = fitted_shift − τ0 = (δ0 − δ_rest) − τ0`；`δ0(τ0=0)=39.5 ms`。
2. 未饱和子集：`δ_rest>0.5` 且 `τ0>0`（τ0=0 为参考点，不进残差中位）。
3. 常数偏移量 = `median(residual | 未饱和子集)`；对照挂账 `med_res_unsat=+2.70` / ≈+2.7（`artifacts/p16_52_p3.txt` / `p15_51_part1`），**不**另发明数值。
4. 单点 residual 可散（P3 未饱和约 −2.3…+9.5）；判定用中位法，不得改用单点魔术数或改回 2×。
5. 偏离本方法 → **INVALID**（G2 / G5）。

OFFSET_SOURCE：P3 med_res_unsat=+2.70；#51 未饱和 eff−τ0 中位≈+2.7。

## 单项判据（G3）

### A1a（本票焦点）

- 焦点网格（写死，不扩大）：τ0 ∈ **{21.8, 3.3, 6.8, 8.3, 33.3}**
  - 不采 #52 原网格中的 {0.0, 16.7, 50.0}
- 每格 ≥5 reps；每格 RMS 中位 ≤ **0.41**（修正口径见上）
- PASS：5 格全部满足；FAIL：任一格 RMS 中位 >0.41；INVALID：任一格 <5 或口径偏离 G2

### A1b（沿用 #52 A v2）

- amp ∈ {+120, +240, −120, −240}；每档 ≥5
- origin：负幅度=220，正幅度=0
- 每档 PASS：非出画，且 RMS 中位 ≤0.41
- 出画：amp<0，且 |x_last−(origin+amp)| 过大或样本过少（沿用 #51 Part1）
- PASS ⇔ 4 档全部 PASS；FAIL：任一档出画或 RMS 中位 >0.41；INVALID：任一档 <5

### A1c（沿用 #52 A v2）

- ε ∈ [−40, +40] ms，步长 0.5；ε 撞界端点 → trial INVALID
- 会话任一 INVALID trial → A1c=INVALID（不得记 PASS/FAIL）
- 双模：μ+=+6.5，μ−=−10.0，R=4.0
- PASS 需同时：OTHER=0；有效 trial ≥5；RMS 中位 ≤0.41；两模都非空时 |med+−med−| ∈ [14.0, 19.0]（单模非空亦允许）
- 无 INVALID trial 时任一条不满足 → FAIL
- 禁止用全体 ε 中位掩盖双模

### A2（沿用 #52 A v2）

- path=PS；≥4 trials
- PASS：全部 path=PS（**不是** RMS ≤0.41）
- FAIL：任一 path≠PS；INVALID：trials <4

## 归因与票级（G4 / G5）

按顺序，命中即停：

1. 票级 **INVALID**：任一会话有任一单项 INVALID，或流程违规（未 LOCKED/未 APPROVED 就采数、hash 不符、追加、补测）→ 必须开**新**闸门（新 LOCKED_AT + 新 GATE_BODY_SHA256）；**不得**在本闸门追加；不得补测拼凑；本轮数据作废留档。
2. 修正后 A-only×3 <3/3 → 「**修正后未稳态**」，票级 FAIL（优先于「不可复现」）。
3. A-only×3 = 3/3 → 票级 PASS（≠ 重开 S1，≠ 采用，成功 ≠ 采用）。

**INVALID 即停**：命中 INVALID 规则立刻停采、留档、回报协调侧；不要自行改闸门或补测。

## 显式禁止

- ❌ 编辑 / 改动 `composition.h`（sha 必须保持 a55d4952…e418）
- ❌ 重开 S1 出货 / 重开 R-3
- ❌ 把票级 PASS 写成「采用 Sup-adv」或任何 adopt 主张
- ❌ 偏离 CONFIRM_F5_REFUTE_2X / OFFSET_METHOD（改用 2× 或单点魔术数）
- ❌ 扩大 A1a 焦点网格
- ❌ 在本 LOCKED 闸门上追加条文或补测拼凑
- ❌ 未核闸门 hash / composition.h sha 就开始采数

## 产物（写到 `artifacts/`，文件名清晰）

建议命名（可按会话拆分，但前缀统一）：

- `artifacts/t_a1a_sha_pre.txt` / `artifacts/t_a1a_sha_post.txt` — composition.h 与闸门 hash 核验
- `artifacts/t_a1a_s1.txt` / `t_a1a_s2.txt` / `t_a1a_s3.txt` — 每会话 A1a/A1b/A1c/A2 明细与 P/F/I
- `artifacts/t_a1a_a1a_grid.txt` — 焦点 5 格每格 reps、RMS 中位、偏移估计过程
- `artifacts/t_a1a_summary.txt` — 票级汇总 + ONELINE
- 原始 trial 日志按既有 harness 习惯落盘，并在 summary 里列出路径

## 回报协调侧格式（G7 ONELINE）

按闸门模板一行回报：

```
T-A1a | gate=0750a8402d4118c98025e71863f79bafd689966c2565fa922d0674e8bcf89d1d @ 2026-10-24 22:27:54 AWST | approved=pidan@<APPROVED_AT> | corr=CONFIRM_F5_REFUTE_2X | S1:A1a=<P/F/I> A1b=<P/F/I> A1c=<P/F/I> A2=<P/F/I> | S2:… | S3:… | A-only×3=<k>/3 | ticket=<PASS/FAIL/INVALID> | 归因=<修正后未稳态/—> | 成功≠采用；不重开 S1
```

另附：composition.h sha 前后一致确认；闸门 hash 核验确认；产物路径列表。若 INVALID，说明触发的 G4/G5 条款，**停止**，等待新闸门。
