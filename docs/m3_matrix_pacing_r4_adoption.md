# M3-MATRIX-PACING r4 采用决断（m3_matrix_pacing_r4_adoption.md）

> pidan，2026-11-06。依据：`docs/m3_matrix_pacing_r4_result.md`（协调结题 RIVE-MATRIX-PACING-PASS）、`docs/m3_matrix_pacing_r4_result_claude_review.md`、`docs/m3_wp1_rive_host_requirements.md`（Y1 H6）。

| 项 | 决断 |
|---|---|
| 是否采用 | **有条件采用（WP）** |
| 采用内容 | 代码 `69094ee`：Rive 渲染线程按窗口所在显示器的垂直同步出帧（`m0r_set_pacing_window`），垂直同步后约 **0.3·T** 再提交（`m0r_set_pacing_offset`，高精度定时器），动画时钟按垂直同步时刻 |
| 范围 | **实验分支 + 产品化 Rive 宿主模块默认开启**（对齐 Y1 H6）；产品化模块默认开启另票实现 |
| 不包括 | **不写进 API v0 L1 的默认行为**（L1 只有 MotionSystem / AnimationHandle / MotionTokens）；**不合 main**，除非 pidan 另令；不改写 MATRIX、PACING r1–r3 的 CLOSED 结论 |
| 预算判定口径 | 按 PACING r3/r4 的定义：GPU 预算只计 Rive 渲染段，拷贝进表面单独记录（pidan V1） |
| 预算残差草案 | `m3_matrix_budget_gate_DRAFT.md`、`m3_matrix_render_budget_gate_DRAFT.md` **SUPERSEDED**：r4 已 `budget_ok=1`、180 Hz 屏 Rive 渲染 GPU ≈ 0.040·T，r3 残差已消；不再 LOCK。注：这两份文件不在仓库中（只在协调侧本地），协调提交时须在文件头加 SUPERSEDED 标记 |
