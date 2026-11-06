# M3-MATRIX-PACING r2 APPROVED 留证（m3_matrix_pacing_r2_approved.md）

> 本文件是闸门 `docs/m3_matrix_pacing_gate_r2_LOCKED.md` 的 APPROVED 留证，不修改 LOCKED 闸门的 body 或 hash。r1 的 approved（`docs/m3_matrix_pacing_approved.md`）只对 r1 有效。

## 引用的 LOCK（不变）
- 闸门路径：`docs/m3_matrix_pacing_gate_r2_LOCKED.md`
- STATUS=LOCKED
- LOCKED_AT=2026-11-06 10:57:03 AWST
- GATE_BODY_SHA256=a1fa4590285f52948ecf51bc9c32629d4d2c3bc42efc7f5bb4e4922bd2be2883（落盘前已复算，MATCH）
- 哈希约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。

## APPROVED 留证
- APPROVED_BY=pidan
- APPROVED_AT=2026-11-06 10:57:53 AWST（Australia/Perth，= UTC+8）
- APPROVED_NOTE=会话回复「APPROVED M3-MATRIX-PACING r2」，在 LOCK a1fa4590…2883 之后

## 范围（写死）
| 字段 | 值 |
|---|---|
| 票 | M3-MATRIX-PACING r2：DESKTOP-IJI1KJG，强制同拓扑 75 Hz@100% + 180 Hz@150% |
| 代码 | 分支 `claude/m3-matrix`（LOCK 时代码提交 `53cedde`；需重建 m0rive.dll；包含渲染器出帧时机改动：垂直同步之后再 `DwmFlush`） |
| 采数 | **是**：`m3/run_m3_matrix.ps1 -Sessions 3 -Pacing`，数据目录 `<ROOT>\artifacts_m3matrixpacing_r2`，结果提交到 `m3/results_matrix_pacing_r2/` |
| 判据 | 与 r1 LOCKED G2 完全相同（含 ④ 前序已过字段回退即 FAIL） |
| 落地 / 合入 main | **否**（结果评审后经 PR） |
| composition.h | 不改（LF `a55d4952…e418`） |

## 禁令
同 r1 G6；r1、MATRIX 数据不改不混用；提交结果前先从 origin 同步，只新增结果文件；FAIL 不重跑凑数；成功 ≠ 采用；PERF-CI 仍 HOLD。
