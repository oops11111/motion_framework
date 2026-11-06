# M3-MATRIX-PACING 门槛闸门 r2（m3_matrix_pacing_gate_r2_LOCKED.md）

STATUS=LOCKED
LOCKED_AT: 2026-11-06 10:57:03 AWST（Australia/Perth）
GATE_BODY_SHA256: a1fa4590285f52948ecf51bc9c32629d4d2c3bc42efc7f5bb4e4922bd2be2883
> GATE_BODY_SHA256 计算约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。本行写入后不得再改 body。

**本 LOCK 不授权采数。** 须另有 approved 留证（APPROVED_* 非空）后才可采数；构建可先行。

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> 起草：Claude，2026-11-06（pidan：「按建议出 PACING r2」）。DRAFT：`docs/m3_matrix_pacing_gate_r2_DRAFT.md`。取代 r1 `docs/m3_matrix_pacing_gate_LOCKED.md`（notion ai 锁定，`2543e424…812c`；r1 文件不改，r1 结论按复核 `docs/m3_matrix_pacing_result_claude_review.md`：pacing 3/3 通过，但按 r1 G2 ④ 因 180 Hz 屏上 GPU 预算回退判 FAIL）。
> 分支：`claude/m3-matrix`。结果目录 **`m3/results_matrix_pacing_r2/`**（不覆写 `m3/results_matrix/`、`m3/results_matrix_pacing/`）。

## G0 硬边界
沿用 r1 G0 全部条款（不改 composition.h；不重开 S1/R-3/R-2；禁影子 IV；PERF-CI 仍 HOLD；不解冻 Bv；未 LOCK 且 APPROVED_* 为空时禁止采数；成功 ≠ 采用 ≠ 默认可合入 main；不翻 CLOSED MATRIX；强制同拓扑 DESKTOP-IJI1KJG 75 Hz@100% + 180 Hz@150%）。

## G1 / G2：与 r1 完全相同
要回答的问题（r1 G1）、判据 ①–⑦（r1 G2，含双列 pacing 硬判、瞬态豁免窗 N = 2.0 s ∧ M = 120 帧、④ 前序已过字段系统性回退到 0 即 FAIL、⑤ 分布与计时、⑥ 目录隔离）、PASS / FAIL / INVALID / INCONCLUSIVE 口径、标签 `RIVE-MATRIX-PACING-*`，**全部沿用 r1 LOCKED，不改一字**。

## G3 唯一改动：出帧时机（pidan 已按建议裁决）
| # | 内容 | 理由 |
|---|---|---|
| **R2-1** | 渲染线程在窗口所在屏的垂直同步（`IDXGIOutput::WaitForVBlank`）之后，**再等一次 DWM 合成完成（`DwmFlush`）**，然后才画。其余（按窗口换屏/设备替换重新查找输出、找不到输出时退回 `DwmFlush`、默认行为不变）与 r1 相同 | r1 在垂直同步一到就画，与 DWM 读取同一绘制表面冲突，180 Hz 屏上 GPU 时间约 1.06·T；先等合成结束就回到 MATRIX 时的时机（GPU 0.041·T），同时仍每个垂直同步只出一帧 |

## G4 实现（已写，未在真机运行；代码 = 分支 `claude/m3-matrix` 提交 `53cedde`）
| 文件 | 改动 | 本地验证 |
|---|---|---|
| `m0/native/m0rive/m0rive.cpp` | `WaitFrame`：`WaitForVBlank` 成功后调用 `DwmFlush` | MinGW 对照钉住的 Rive 头文件：0 警告 0 错误 |
| `MotionSoftDemo/M3MatrixProbe.cs` | `probe=M3-MATRIX-PACING v2`；`pacing_source` 记为 `vblank+dwmflush` | 对照 WinAppSDK 2.5.1 编译 0 个 C# 错误 |
| runner、MainPage | 不变（`run_m3_matrix.ps1 -Pacing`；MainPage 补丁同 r1） | — |

## G5 禁令
同 r1 G6；r1 数据不改不删，不与 r2 混用；本 DRAFT ≠ 授权采数。
