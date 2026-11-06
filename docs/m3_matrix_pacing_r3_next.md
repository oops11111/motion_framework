# M3 下一票决断（M3-MATRIX-PACING r3 CLOSED RIVE-MATRIX-PACING-FAIL · pacing_ok=1 · budget_ok=0 · 不采用 · 不合 main · PERF-CI HOLD）

> DECISION_AT=2026-11-06 11:26:00 AWST（Australia/Perth（AWST，UTC+8））  
> 依据：`docs/m3_matrix_pacing_r3_close_pack.md`；`docs/m3_matrix_pacing_r3_result.md`；gate MATCH `90f9356bf2739ea2a95a56619fc73af561b0c34e4ebd097be05442f8c5719476`  
> tip/results：`82eacb0b59e1bab7ee809b4b62c8a100545b4073` / `39cdcf42e7758555b223c8058cc8a2e9e3793aff` @ `claude/m3-matrix`  
> **硬禁**：不改 composition.h；不合 main；Success≠adopt；PERF-CI HOLD；不翻 CLOSED；**不回改 MATRIX / PACING r1 / r2**；**本 next 不授权新采数/真机**；**勿擅自 LOCK**；**禁合并 r1+r2+r3 洗票**

## 结论

**主推：新开（或改写）专吃 r3 `budget_ok=0` / `gpu_render_p95_frac_T@180Hz≈1.05–1.08` 残差的纯文档闸门 DRAFT — STATUS=DRAFT·NOT LOCKED；APPROVED_* 空；不授权真机；不派 workbuddy。**

建议票族二选一（供 pidan 拍板）：  
- **A（建议）**：`M3-MATRIX-RENDER-BUDGET` / `M3-MATRIX-GPU-RENDER` 新 DRAFT（刀口写死：render 区间 vs 0.35·T @180Hz；copy 可忽略旁证）  
- **B**：重开 / 升优先级已落盘 `docs/m3_matrix_budget_gate_DRAFT.md`（由「挂账/对照」改回 LOCK 候选路径；须书面纳入 r3 拆分口径）

**pacing 残差：** r3 `pacing_ok=1` + `pacing_source=vblank` + p95≪T → **相对 r2 的 pacing 残差大体可挂账闭合**（doc-only 对照即可）；**不**因此把本票或三轮升 PASS。

**对照 r1/r2（禁洗票）：**  
- r1：pacing✓ budget✗（合计 GPU 含等 DWM）  
- r2：pacing✗ budget✓（vblank+dwmflush）  
- r3：pacing✓ budget✗ —— 拆分后 gpu_copy≈0，失败在 **render**  
三票各自 CLOSED FAIL；禁止拼成 PASS。

**一句话为什么：** vblank-only 路径下节奏已绿，但按 render 计的预算在 180Hz 仍超 0.35·T；应用文档先消化 **render 预算残差** 与相对 BUDGET DRAFT 的关系，而不是自动 LOCK、采数或洗票。

## 现状

| 项 | 状态 |
|---|---|
| PACING **r3** | **CLOSED FAIL**；pacing_ok=1；budget_ok=0；不采用 |
| PACING **r2** | **CLOSED FAIL**；pacing_ok=0；budget_ok=1；**禁回改** |
| PACING **r1** | **CLOSED FAIL**；pacing_ok=1；budget_ok=0；**禁回改** |
| MATRIX | **CLOSED FAIL**；**禁回改** |
| M3-MATRIX-BUDGET DRAFT | 已落盘 NOT LOCKED；曾因 r2 budget_ok=1 降为挂账；**r3 再显 budget 残差（render）→ 优先级待重排** |
| PACING-R3 DRAFT | `docs/m3_matrix_pacing_r3_gate_DRAFT.md` 曾为续票入口；本轮已 CLOSED；**勿擅自再 LOCK 同票** |
| composition.h / PERF-CI | 冻结 / **HOLD** |

## 候选速判

| 选项 | 决断 | 理由 |
|---|---|---|
| **A 主推：RENDER-BUDGET / GPU-RENDER 新 DRAFT** | **建议采纳** | 吃 r3 budget_ok=0；刀口对准 gpu_render@180Hz |
| **B 重排并可能 LOCK 路径回 BUDGET DRAFT** | **开放次选** | 已有文档；须纳入 r3 拆分口径；**仍勿擅自 LOCK** |
| C 自动 LOCK / 立刻采数 | **否** | 勿擅自 LOCK；本 next 不授权真机 |
| D 合并 r1+r2+r3 改判 PASS | **否** | 各有硬 FAIL；禁洗票 |
| E 回改 r1 / r2 / MATRIX | **否** | 硬禁 |
| F 合 main / 解冻 PERF-CI | **否** | 硬禁 |
| G 仅 doc-only 挂账 pacing「已闭合」且不再开预算票 | **开放** | 须 pidan 明示；建议仍有预算残差文档入口 |
| H 把 r2 budget_ok=1 写成「预算已过」 | **否** | 被 r3 budget_ok=0 + render 证据否定作整线结论 |

## 开放问题（供 **pidan** 拍板；**勿擅自 LOCK**）

1. **下一票票名**：`M3-MATRIX-RENDER-BUDGET` / `M3-MATRIX-GPU-RENDER` / 仍用 `M3-MATRIX-BUDGET` / 其它？  
2. **与 `docs/m3_matrix_budget_gate_DRAFT.md` 关系**：继续挂账对照 / SUPERSEDED 由新 RENDER DRAFT 取代 / 升回 LOCK 候选并改写纳入 r3 拆分？  
3. **预算门槛**：保持 **0.35·T** 硬门槛（相对 180Hz T≈5.56ms），还是书面调整？r3 `gpu_render_p95_frac_T≈1.05–1.08` 如何解释（真超 / 探针归因 / 混刷双列）？  
4. **计费口径**：是否写死「预算只计 render、不计 copy / 不计 WaitForVBlank」为唯一合法？copy≈0 是否升硬旁证？  
5. **探针**：v3 + `pacing_source=vblank` 是否成为后续预算票合法基线？是否仍要求 seg MATCH？  
6. **复测**：强制同拓扑 IJI？≥3？结果目录建议 `m3/results_matrix_budget/` 或 `m3/results_matrix_render_budget/`（**勿覆写** r1/r2/r3/matrix）？  
7. **pacing 是否再测**：只读旁证（r3 pacing_ok=1）还是硬复测？建议默认只读旁证。  
8. **FAIL 照交**：是否仍禁自动重跑？  
9. **标签前缀**：`RIVE-MATRIX-BUDGET-*` / `RIVE-MATRIX-RENDER-*` / 仍挂 `RIVE-MATRIX-PACING-*`？  
10. **是否允许 doc-only 挂账**（对照 H5 ABS）而不采数？  
11. **APPROVED_***：任何新 DRAFT 必须 **空**；LOCK≠采数；本 next **不**填 APPROVED。

## 协调下一步

1. 归档 r3 CLOSED FAIL / 不采用 / 不合 main。  
2. 请 pidan 拍板开放问题与相对 BUDGET DRAFT / 新 RENDER DRAFT 的优先级。  
3. 若采纳 A 或 B：指定文档助手写/改 DRAFT（**NOT LOCKED**；APPROVED_* 空）。  
4. 推送结题 docs tar → 协调另派 workbuddy/K93（本 next **不**派真机）。  
5. 禁合 main；禁改 composition.h；**禁擅自 LOCK**；**禁回改 r1/r2/MATRIX**；**禁洗票**。

## 确认（硬边界）

- 不改 composition.h（CRLF `33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97` / LF `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）  
- PERF-CI HOLD；不合 main；Success≠adopt  
- **本 next 不授权新采数/真机**；**勿擅自 LOCK**；任何新 DRAFT 的 APPROVED_* **空**  
- **不回改** CLOSED MATRIX / PACING r1 / r2 / 本 r3 标签  
- **不**把 r1+r2+r3 合并改 PASS

— notion ai（下一票建议=RENDER-BUDGET / 重排 BUDGET DRAFT；pacing 残差可挂账；勿擅自 LOCK；r3=CLOSED FAIL；Australia/Perth（AWST，UTC+8））
