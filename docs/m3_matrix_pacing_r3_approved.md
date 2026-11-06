# M3-MATRIX-PACING r3 APPROVED 留证（m3_matrix_pacing_r3_approved.md）

> 本文件是闸门 `docs/m3_matrix_pacing_gate_r3_LOCKED.md` 的 APPROVED 留证，不修改 LOCKED 闸门的 body 或 hash。r1、r2 的 approved 只对各自闸门有效。

## 引用的 LOCK（不变）
- 闸门路径：`docs/m3_matrix_pacing_gate_r3_LOCKED.md`
- STATUS=LOCKED
- LOCKED_AT=2026-11-06 11:14:15 AWST
- GATE_BODY_SHA256=90f9356bf2739ea2a95a56619fc73af561b0c34e4ebd097be05442f8c5719476（落盘前已复算，MATCH）
- 哈希约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。

## APPROVED 留证
- APPROVED_BY=pidan
- APPROVED_AT=2026-11-06 11:15:19 AWST（Australia/Perth，= UTC+8）
- APPROVED_NOTE=会话回复「APPROVED M3-MATRIX-PACING r3」，在 LOCK 90f9356b…9476 之后

## 范围（写死）
| 字段 | 值 |
|---|---|
| 票 | M3-MATRIX-PACING r3：DESKTOP-IJI1KJG，强制同拓扑 75 Hz@100% + 180 Hz@150% |
| 代码 | 分支 `claude/m3-matrix`（LOCK 时代码提交 `878892d`；需重建 m0rive.dll；出帧只等垂直同步，GPU 计时拆分） |
| 采数 | **是**：`m3/run_m3_matrix.ps1 -Sessions 3 -Pacing`，数据目录 `<ROOT>\artifacts_m3matrixpacing_r3`，结果提交到 `m3/results_matrix_pacing_r3/` |
| 判据 | r1 LOCKED G2，唯一改动：预算的 GPU 项只按 Rive 渲染判定，拷贝进表面单独记录（r3 G1/G2） |
| 落地 / 合入 main | **否**（结果评审后经 PR） |
| composition.h | 不改（LF `a55d4952…e418`） |

## 禁令
同 r1 G6；r1、r2、MATRIX 数据不改不混用；提交结果前先从 origin 同步，只新增结果文件；FAIL 不重跑凑数；成功 ≠ 采用；PERF-CI 仍 HOLD。
