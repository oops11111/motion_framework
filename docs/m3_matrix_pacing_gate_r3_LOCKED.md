# M3-MATRIX-PACING 门槛闸门 r3（m3_matrix_pacing_gate_r3_LOCKED.md）

STATUS=LOCKED
LOCKED_AT: 2026-11-06 11:14:15 AWST（Australia/Perth）
GATE_BODY_SHA256: 90f9356bf2739ea2a95a56619fc73af561b0c34e4ebd097be05442f8c5719476
> GATE_BODY_SHA256 计算约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。本行写入后不得再改 body。

**本 LOCK 不授权采数。** 须另有 approved 留证（APPROVED_* 非空）后才可采数；构建可先行。

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> 起草：Claude，2026-11-06（pidan：「V1 as recommended」）。DRAFT：`docs/m3_matrix_pacing_gate_r3_DRAFT.md`。取代 r2 `docs/m3_matrix_pacing_gate_r2_LOCKED.md`（`a1fa4590…2883`；r2 结论 FAIL：预算恢复但节奏坏，见 `docs/m3_matrix_pacing_r2_result_claude_review.md`）。r1（`2543e424…812c`，notion ai 锁定）结论 FAIL：节奏完美但 180 Hz 屏 GPU 计时约 1.06·T。
> 分支：`claude/m3-matrix`。结果目录 **`m3/results_matrix_pacing_r3/`**（不覆写 r1、r2、MATRIX 的结果目录）。

## G0 硬边界
沿用 r1 G0 全部条款（不改 composition.h；不重开 S1/R-3/R-2；禁影子 IV；PERF-CI 仍 HOLD；不解冻 Bv；未 LOCK 且 APPROVED_* 为空时禁止采数；成功 ≠ 采用 ≠ 默认可合入 main；不翻 CLOSED MATRIX / r1 / r2；强制同拓扑 DESKTOP-IJI1KJG 75 Hz@100% + 180 Hz@150%）。

## G1 / G2：沿用 r1，只改一处定义
- 要回答的问题、判据 ①–⑦、瞬态豁免窗、双列 pacing 硬判、④ 前序已过字段回退即 FAIL、⑤ 分布与计时、⑥ 目录隔离、标签 `RIVE-MATRIX-PACING-*`：**全部沿用 r1 LOCKED**。
- **唯一改动（pidan V1 裁决）——④ 中“预算”的 GPU 项的定义**：GPU P95 ≤ 0.35·T 只针对 **Rive 渲染**（从一帧开始到 Rive 提交完成的 GPU 时间戳差）。**拷贝进合成绘制表面**这一步（其中包含等 DWM 读完同一表面的时间）**单独记录、不参与判定**；同时照旧记录两者合计的 GPU 时间，便于与 MATRIX、r1、r2 对照。
- 理由：plan.md 的预算约束的是我们自己的渲染工作；r1 中 180 Hz 屏上约 1.06·T 的 GPU 时间主要是在等 DWM，不是渲染工作（Rive 本身约 0.2 ms）。这个等待的后果——新帧在高刷屏上可能晚一次合成上屏（约 +5.6 ms 延迟）——作为记录项写入结题和已知限制，不再混进预算判定。

## G3 实现（已写，未在真机运行；代码 = 分支 `claude/m3-matrix` 提交 `878892d`）
| 文件 | 改动 | 本地验证 |
|---|---|---|
| `m0/native/m0rive/m0rive.{h,cpp}` | 出帧时机回到 r1：只等窗口所在屏的 `WaitForVBlank`（去掉 r2 的 `DwmFlush`）；在 Rive 提交之后、开始拷贝之前加第三个 GPU 时间戳；新增 `m0r_get_gpu_split`（渲染 / 拷贝各自的 P50、P95）；`m0r_stats` 的 `gpu_*` 仍是合计，含义不变 | MinGW 对照钉住的 Rive 头文件：0 警告 0 错误 |
| `MotionSoftDemo/M3MatrixProbe.cs` | `probe=M3-MATRIX-PACING v3`；`pacing_source=vblank`；每段写出 `gpu_render_p95_frac_T`、`gpu_copy_p50_ms`、`gpu_copy_p95_ms`、`gpu_copy_p95_frac_T`，以及原有的合计 `gpu_p95_frac_T`；pacing 模式下预算按渲染 GPU 判定 | 对照 WinAppSDK 2.5.1 编译 0 个 C# 错误 |
| runner、MainPage | 不变（`run_m3_matrix.ps1 -Pacing`） | — |

## G4 禁令
同 r1 G6；r1、r2、MATRIX 数据不改不混用；本 DRAFT ≠ 授权采数。
