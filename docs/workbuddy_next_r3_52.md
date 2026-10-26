# workbuddy 派题 — R-3 #52（整包 P1+P2+P3）

**状态：DONE 2026-10-24**（整包 P1+P2+P3 已执行）。结果：A v2×3=2/3 → **STOP S1**；S1 NO-ADOPT。**R-3 已 CLOSED**（用户确认 2026-10-24，按 DeepSeek 三条挂账）。汇总 [`artifacts/p16_52.txt`](../artifacts/p16_52.txt)；soft_demo [workbuddy_soft_demo.md](./workbuddy_soft_demo.md) ## 派题 #52；结题 [r3_commit_staleness.md ## 结题（CLOSED）](./r3_commit_staleness.md)。
权威：[claude_r3_51_review.md](./claude_r3_51_review.md)（Notion AI fallback FINAL）。硬停与 P1/P2/P3 定义以该 FINAL 为准；本文如有歧义，以 FINAL 为准。

## 不变量
- `code/composition.h` sha256 必须保持 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`。**不得改 composition.h**；仅允许 harness/日志改动。运行前后各核一次 sha 并写入产物。

## 顺序建议
1. **P3**（离线分析现有 `artifacts/p15_51*` 细网格）可与 **P1 准备**（harness 加帧/present 时间戳日志）并行。
2. **P2 须先写死 A v2 闸门**（提交/落盘带时间戳）**再采数**。
3. 采数：**同一会话内交错** Sup / Bv / base。

## P1 — 帧相位日志
- 每个 trial 记录帧/present 时间戳，旁路记录 ε_fit。
- 检验：帧相位是否预测 ε 双模 {+6.5, −10}。给出分离判据与结果（分开 / 分不开）。
- 注：60 Hz 帧周期在 #51 是假设 [H]；本轮请实测帧周期。

## P2 — A v2（采数前写死）
- **A1b**：crop origin = 220。
- **A1c**：放宽 ε 搜索界；任何拟合**撞界 = INVALID（非 FAIL）**；写明**双模显式判据**。
- **A1a / A2**：不变。
- 然后**新鲜 A×3**，PASS 要求 **3/3**。

## P3 — A1a 残差 vs τ0
- 在现有细网格（`artifacts/p15_51*.txt`）上做 per-point residual vs τ0（表/图），**确认或撤回 Finding 5（2× 增益）**。仅分析，不采新数。注意 τ0≈3.3 处 2× 与常数偏移不可区分，需说明未饱和 τ0 分布。

## 硬停（逐条抄 FINAL）
- If composition.h sha256 differs from a55d4952…e418, the run is INVALID.
- If any fit hits its search bound, the run is INVALID and must not be read as PASS/FAIL.
- No cross-session or cross-run comparison with #49 numbers.
- If P1 frame phase does not separate the ε modes: STOP the S1 frame-phase path.
- If A v2 ×3 is below 3/3: STOP S1. Recommend closing R-3 on Bv (degraded) at the next review.
- If a ≈30 c mode appears in any session: the toggle concern is confirmed again. STOP S1 and stay on Bv.
- No gate may be changed after data collection to fit observed data.
- S2 is not to be revived inside #52.

## 产物预期
- `artifacts/p16_52*.txt`（含 sha 前后核验、写死的 A v2 闸门原文+时间戳、P1 逐 trial 帧相位/ε 表、P2 A×3 结果、P3 残差 vs τ0）。
- `docs/workbuddy_soft_demo.md` 新增 `## 派题 #52` 小节。
- 一行 **ONELINE** 总结（如：`#52: P1=分开/分不开; A v2×3=k/3; P3=F5确认/撤回; ≈30c=有/无; 硬停=…; sha OK`）。
