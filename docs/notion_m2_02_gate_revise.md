# M2-02 闸门补洞决断（可 LOCK）

> DECISION_AT=2026-10-25 00:41:37 AWST（Australia/Perth）  
> 依据：`docs/gemini_m2_02_design.md`、`docs/deepseek_m2_02_review.md`、`docs/deepseek_m2_02_api_draft.md`（§3.4 已禁影子 IV）、`docs/m2_02_gate_DRAFT.md`（旧）  
> 完整可 LOCK 正文：`docs/m2_02_gate_DRAFT.md`（本决断后覆盖）  
> **不授权采数**；须 LOCKED + APPROVED 后才可派 workbuddy。

## 拍板

| # | 缺口 | 决断 |
|---|---|---|
| 1 | G1 三项验收句 | 采纳 Gemini smoke + DeepSeek 子目标写死（见闸门 G1） |
| 2 | G2 LOCK-1…9 | **采纳 DeepSeek G2-LOCK-1…9**；预览/C# 步骤嵌入 Gemini smoke |
| 3 | ETW 最小集 | **冻结 6 项**：AnimationLifecycle / Retarget / FrameTick / PresentStats / IdleTransition / TokenLoad；LoweringDecision 降级（schema 保留、不进判分）；NoiseProbe 移出 |
| 4 | 性能 CI ≥2 机 | **另票**（不进本票 PASS） |
| 5 | 禁令 | 禁影子 IV；不改 composition.h `a55d4952…e418`；成功≠采用；不重开 S1/R-3/R-2；不解冻/升级 Bv |

## 结论

**可 LOCK。** 请协调侧立刻写 hash + LOCKED_AT，并向 pidan 要 APPROVED。真机仍等 APPROVED。

— notion ai（M2-02 闸门补洞决断）
