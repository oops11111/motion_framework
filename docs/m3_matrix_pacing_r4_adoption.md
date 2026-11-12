# M3-MATRIX-PACING r4 采用决断（m3_matrix_pacing_r4_adoption.md）

> pidan，2026-11-06。依据：`docs/m3_matrix_pacing_r4_result.md`（协调结题 RIVE-MATRIX-PACING-PASS）、`docs/m3_matrix_pacing_r4_result_claude_review.md`、`docs/m3_wp1_rive_host_requirements.md`（Y1 H6）。

| 项 | 决断 |
|---|---|
| 是否采用 | **有条件采用（WP）** |
| 采用内容 | 代码 `69094ee`：Rive 渲染线程按窗口所在显示器的垂直同步出帧（`m0r_set_pacing_window`），垂直同步后约 **0.3·T** 再提交（`m0r_set_pacing_offset`，高精度定时器），动画时钟按垂直同步时刻 |
| 范围 | **实验分支 + 产品化 Rive 宿主模块默认开启**（对齐 Y1 H6）；产品化模块默认开启另票实现 |
| 不包括 | **不写进 API v0 L1 的默认行为**（L1 只有 MotionSystem / AnimationHandle / MotionTokens）；**不合 main**，除非 pidan 另令；不改写 MATRIX、PACING r1–r3 的 CLOSED 结论 |
| 预算判定口径 | 按 PACING r3/r4 的定义：GPU 预算只计 Rive 渲染段，拷贝进表面单独记录（pidan V1） |
| 预算残差草案 | `m3_matrix_budget_gate_DRAFT.md`、`m3_matrix_render_budget_gate_DRAFT.md` **SUPERSEDED**：r4 已 `budget_ok=1`、180 Hz 屏 Rive 渲染 GPU ≈ 0.040·T，r3 残差已消；不再 LOCK。本次（2026-11-10）已把这两份文件纳入仓库，并在文首加 SUPERSEDED 横幅（正文保留不删；文首前 sha：budget `49bbc9757feeade1f5b571c317e01be2c9d14fb9e464fa98438e5f31dba54745`，render-budget `e10bf9e5bf2944f13faec600a2db1de914d214a55025714ecc921fb9550b5dd1`）；勿再 LOCK、勿再采数 |

## 再确认（2026-11-10 22:35 AWST）

pidan 于 2026-11-10 22:35 AWST（Australia/Perth）对协调建议回复「确认」：本文件 2026-11-06 的有条件采用（WP）仍然有效。范围仍是**实验分支 + 产品化 Rive 宿主模块默认开启**；**不合 main**；**不写进 API v0 L1 默认**（0.3·T / tip `69094ee` 非 API 默认）。PASS 闸门 `81fc74ebba270feb818646cd5f5c8c90fd2d3812c3447c642ed126413108934c`；结果目录 `m3/results_matrix_pacing_r4/`。两份预算 DRAFT 正式 **SUPERSEDED**（横幅见各文件文首）。PERF-CI 仍 HOLD。不以本确认授权新采数、LOCK 或合 main。

命名说明：未另开 `m3_matrix_pacing_r4_conditional_adopt.md`；以本文件为唯一采用记录。`docs/m3_matrix_pacing_r4_next.md` 保留为当时 next 快照，文首已加指针。
