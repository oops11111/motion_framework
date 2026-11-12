# M3 下一票决断（M3-MATRIX-PACING r4 CLOSED RIVE-MATRIX-PACING-PASS · 全判据绿 · Success≠adopt · 不合 main · PERF-CI HOLD）

> **已裁定（pidan）**：2026-11-06 条件采用 WP + 两份预算 DRAFT **SUPERSEDED**；2026-11-10 22:35 AWST 书面再确认。权威记录：`docs/m3_matrix_pacing_r4_adoption.md`、`docs/Z_SERIES_CONVERGE.md`。下文开放问题 1 / 3 / 4 / 5 已关闭；**不合 main**（问题 2 默认仍否）；PERF-CI **仍 HOLD**。本文保留为当时 next 快照，不以本文作现行权威。

> DECISION_AT=2026-11-06 11:58:00 AWST（Australia/Perth（AWST，UTC+8））  
> 依据：`docs/m3_matrix_pacing_r4_close_pack.md`；`docs/m3_matrix_pacing_r4_result.md`；gate MATCH `81fc74ebba270feb818646cd5f5c8c90fd2d3812c3447c642ed126413108934c`  
> tip/results：`e94600332307e4482cf0ff91d4e16d3b60deefc5` / `6b7a290864ef58ff8b908f8d46532e449c8e4530` @ `claude/m3-matrix`  
> 代码基线：`69094ee`（vblank 后再等 0.3·T）；gpu_render_p95_frac_T@180Hz≈0.040  
> **硬禁**：不改 composition.h；不合 main；Success≠adopt；PERF-CI HOLD；不翻 CLOSED；**不回改 MATRIX / PACING r1 / r2 / r3**；**本 next 不授权新采数/真机**；**勿擅自 LOCK**；**禁用 r4 洗前序 FAIL**；**禁合并 r1+r2+r3 改 PASS**

## 结论

**主推：归档 r4 CLOSED `RIVE-MATRIX-PACING-PASS`（全判据绿）；默认不 adopt / 不合 main；有条件采用 WP（offset=0.3·T / tip `69094ee`）须 **pidan 另拍板**；将 `docs/m3_matrix_render_budget_gate_DRAFT.md` 与 `docs/m3_matrix_budget_gate_DRAFT.md` 标为 **挂账或 SUPERSEDED 候选**（因 r4 `budget_ok=1` 已实证消掉 r3 render 残差）— **开放给 pidan**；**勿擅自 LOCK**；**不改写 DRAFT 正文为 LOCK**；**不授权新采数**。**

**一句话为什么：** r4 在分立 tip（vblank + 0.3·T offset）下全绿且 render 预算远低于 0.35·T，故主线应归档 PASS 并消化「是否条件采用 / 是否挂账预算 DRAFT」，而不是自动合 main、解冻 PERF-CI、翻前序 FAIL 或再开采数。

## 现状

| 项 | 状态 |
|---|---|
| PACING **r4** | **CLOSED PASS**；全判据绿；Success≠adopt；不合 main |
| PACING **r3** | **CLOSED FAIL**；pacing_ok=1；budget_ok=0（render）；**禁回改**；残差被 r4 tip **实证旁证清除**（不翻标签） |
| PACING **r2** | **CLOSED FAIL**；pacing_ok=0；budget_ok=1；**禁回改** |
| PACING **r1** | **CLOSED FAIL**；pacing_ok=1；budget_ok=0；**禁回改** |
| MATRIX | **CLOSED FAIL**；**禁回改** |
| RENDER-BUDGET DRAFT | `docs/m3_matrix_render_budget_gate_DRAFT.md` NOT LOCKED；**建议挂账/SUPERSEDED 候选** |
| BUDGET DRAFT | `docs/m3_matrix_budget_gate_DRAFT.md` NOT LOCKED；**建议挂账/SUPERSEDED 候选** |
| composition.h / PERF-CI | 冻结 / **HOLD**（建议仍 HOLD） |

## 候选速判

| 选项 | 决断 | 理由 |
|---|---|---|
| **A 主推：归档 r4 PASS；默认不 adopt** | **建议采纳** | 全绿可归档；Success≠adopt |
| **B 有条件采用 WP（offset=0.3·T）** | **开放** | 须 pidan 书面拍板；仍不合 main 除非另令 |
| **C 挂账 / SUPERSEDED 候选 RENDER-BUDGET + BUDGET DRAFT** | **建议** | r4 budget_ok=1 实证；**勿擅自 LOCK** |
| D 解冻 PERF-CI | **否（建议仍 HOLD）** | 开放给 pidan；默认 HOLD |
| E 合 main / 改 composition.h | **否** | 硬禁 |
| F 翻 CLOSED r1/r2/r3/MATRIX 或洗票 | **否** | 硬禁 |
| G 自动再开采数 / LOCK 新闸门 | **否** | 本 next 不授权；勿擅自 LOCK |
| H follow-up：GPUView/PIX 验证 DWM 归因 | **开放** | doc-only 规划可；不授权真机除非另批 |

## 开放问题（供 **pidan** 拍板；**勿擅自 LOCK**）

1. **是否条件采用 WP**：采用 tip `69094ee` / `pacing_offset_fraction=0.30` 作为工作路径？范围（仅实验分支 / 文档采用 / 其它）？  
2. **是否合 main**：默认 **否**；若要合，须另令且仍遵守 composition.h 冻结与 Success≠adopt 边界。  
3. **RENDER-BUDGET DRAFT**：挂账对照 / 正式 SUPERSEDED / 仍保留并行？  
4. **BUDGET DRAFT**：同上。  
5. **PERF-CI**：是否解冻？**建议仍 HOLD**。  
6. **follow-up**：是否开 GPUView/PIX 验证 DWM / offset 归因的纯文档票？（**不**默认授权真机）  
7. **标签**：确认终裁 **`RIVE-MATRIX-PACING-PASS`**（本结题已按此写）？  
8. **前序 FAIL**：确认 **不翻** r1/r2/r3/MATRIX？  
9. **APPROVED_***：任何新 DRAFT/LOCK 路径 APPROVED_* 须空或另签；本 next **不**填。  
10. **是否允许 doc-only 挂账** 而不再开预算采数票？

## 协调下一步

1. 归档 r4 CLOSED PASS / Success≠adopt / 不合 main。  
2. 请 pidan 拍板：条件采用 WP？DRAFT 挂账/SUPERSEDED？PERF-CI 是否仍 HOLD？  
3. 推送结题 docs tar → 协调另派（本 next **不**派真机 / workbuddy 采数）。  
4. 禁合 main；禁改 composition.h；**禁擅自 LOCK**；**禁回改 r1/r2/r3/MATRIX**；**禁洗票**。

## 确认（硬边界）

- 不改 composition.h（CRLF `33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97` / LF `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）  
- PERF-CI HOLD；不合 main；Success≠adopt  
- **本 next 不授权新采数/真机**；**勿擅自 LOCK**  
- **不回改** CLOSED MATRIX / PACING r1 / r2 / r3 / 本 r4 标签  
- **不**把 r1+r2+r3 合并改 PASS；**不**用 r4 洗前序 FAIL

— notion ai（下一票建议=归档 r4 PASS；挂账/SUPERSEDED 候选 RENDER-BUDGET+BUDGET DRAFT；有条件采用须 pidan；勿擅自 LOCK；Australia/Perth（AWST，UTC+8））
