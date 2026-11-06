# M3-MATRIX APPROVED 留证（m3_matrix_approved.md）

> 本文件是闸门 `docs/m3_matrix_gate_LOCKED.md` 的 APPROVED 留证，不修改 LOCKED 闸门的 body 或 hash。

## 引用的 LOCK（不变）
- 闸门路径：`docs/m3_matrix_gate_LOCKED.md`
- STATUS=LOCKED
- LOCKED_AT=2026-11-02 09:34:33 AWST
- GATE_BODY_SHA256=d8c21d0304931c42d805ceef35fccf4df50b5f2931fd916b81a13eea5db1963f（落盘前已复算，MATCH）
- 哈希约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。

## APPROVED 留证
- APPROVED_BY=pidan
- APPROVED_AT=2026-11-05 14:41:43 AWST（Australia/Perth）（留证落盘时刻；pidan 的「APPROVED M3-MATRIX」消息在此之前，会话期间工作进程重启过一次）
- APPROVED_NOTE=会话回复「APPROVED M3-MATRIX」，在 LOCK d8c21d03…963f 之后

## 范围（写死）
| 字段 | 值 |
|---|---|
| 票 | M3-MATRIX：DESKTOP-IJI1KJG（75 Hz，100%）+ 一块外接显示器（60 Hz 或 ≥120 Hz；125% 或 150%），恰好两块（X1–X3） |
| 代码 | 分支 `claude/m3-matrix`（LOCK 时代码提交 `fe81db8`；需重建 m0rive.dll） |
| 采数 | **是**：`m3/run_m3_matrix.ps1 -Sessions 3`，数据目录 `<ROOT>\artifacts_m3mx`，结果提交到 `m3/results_matrix/` |
| 输入 | 探针自己移动窗口；**不注入指针、不重启驱动、不需要管理员**（X4） |
| 判据 | 按 LOCKED G2：①–⑥ 每会话满足即 PASS；⑦只记录；不是恰好两块显示器 → BLOCKED_HARDWARE |
| 落地 / 合入 main | **否**（结果评审后经 PR） |
| composition.h | 不改（LF `a55d4952…e418`） |

## 禁令
不重开 S1/R-3/R-2；禁影子 IV；构建失败不改代码硬凑（记 BLOCKED_TOOLCHAIN）；提交结果前先从 origin 同步，只新增结果文件；FAIL 不重跑凑数；成功 ≠ 采用；PERF-CI 仍 HOLD。
