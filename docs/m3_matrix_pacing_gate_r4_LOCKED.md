# M3-MATRIX-PACING 门槛闸门 r4（m3_matrix_pacing_gate_r4_LOCKED.md）

STATUS=LOCKED
LOCKED_AT: 2026-11-06 11:27:50 AWST（Australia/Perth）
GATE_BODY_SHA256: 81fc74ebba270feb818646cd5f5c8c90fd2d3812c3447c642ed126413108934c
> GATE_BODY_SHA256 计算约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。本行写入后不得再改 body。

**本 LOCK 不授权采数。** 须另有 approved 留证（APPROVED_* 非空）后才可采数；构建可先行。

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> 起草：Claude，2026-11-06（pidan：「W2 as recommended」）。DRAFT：`docs/m3_matrix_pacing_gate_r4_DRAFT.md`。取代 r3 `docs/m3_matrix_pacing_gate_r3_LOCKED.md`（`90f9356b…9476`；r3 结论 FAIL：节奏完美，180 Hz 屏上 Rive 渲染段的 GPU 时间约 1.06·T，见 `docs/m3_matrix_pacing_r3_result_claude_review.md`）。
> 分支：`claude/m3-matrix`。结果目录 **`m3/results_matrix_pacing_r4/`**（不覆写 r1–r3、MATRIX 的结果目录）。

## G0 硬边界
沿用 r1 G0 全部条款（不改 composition.h；不重开 S1/R-3/R-2；禁影子 IV；PERF-CI 仍 HOLD；不解冻 Bv；未 LOCK 且 APPROVED_* 为空时禁止采数；成功 ≠ 采用 ≠ 默认可合入 main；不翻 CLOSED MATRIX / r1–r3；强制同拓扑 DESKTOP-IJI1KJG 75 Hz@100% + 180 Hz@150%）。

## G1 / G2：与 r3 完全相同
沿用 r1 的全部判据，以及 r3 对 ④ 预算 GPU 项的定义（只按 Rive 渲染的 GPU 时间判定；拷贝进表面单独记录；合计照旧记录）。不改一字。

## G3 唯一改动：出帧时机加偏移（pidan W2 裁决）
| # | 内容 | 理由 |
|---|---|---|
| **R4-1** | 在窗口所在屏的垂直同步之后，渲染线程用高精度定时器再等 **0.3·T**（T 为该屏刷新周期，取自显示设置）才开始画。动画时间、帧间隔记录仍以垂直同步时刻为准；CPU 帧耗时从等待结束开始计。偏移固定为 0.3，不在采数中调整 | r3 中在 180 Hz 屏的垂直同步时刻提交的工作排在 DWM 合成后面约一帧；推迟到合成窗口之后提交，预期回到 MATRIX 时的 GPU 时间（约 0.04·T），节奏保持 r3 的水平 |

**判读补充**：若 180 Hz 屏上 Rive 渲染 GPU 时间仍约 1·T，则说明偏移无效，按 W2 的约定转 W1（采用 r3 的行为并写成已知限制，由 pidan 决定）。

## G4 实现（已写，未在真机运行；代码 = 分支 `claude/m3-matrix` 提交 `69094ee`）
| 文件 | 改动 | 本地验证 |
|---|---|---|
| `m0/native/m0rive/m0rive.{h,cpp}` | 查找输出时一并取该屏刷新率；垂直同步后按 `CREATE_WAITABLE_TIMER_HIGH_RESOLUTION` 定时器等 `offset·T`（不支持时用普通可等待定时器）；新增 `m0r_set_pacing_offset`、`m0r_get_pacing_offset`；CPU 帧耗时改为从偏移结束计 | MinGW 对照钉住的 Rive 头文件：0 警告 0 错误 |
| `MotionSoftDemo/M3MatrixProbe.cs` | `probe=M3-MATRIX-PACING v4`；设置并记录 `pacing_offset_fraction=0.30`；每段记录实际偏移 `pacing_offset_last_ms` 和 `/T` | 对照 WinAppSDK 2.5.1 编译 0 个 C# 错误 |
| runner、MainPage | 不变（`run_m3_matrix.ps1 -Pacing`） | — |

## G5 禁令
同 r1 G6；r1–r3、MATRIX 数据不改不混用；本 DRAFT ≠ 授权采数。
