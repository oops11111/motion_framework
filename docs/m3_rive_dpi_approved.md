# M3-RIVE-DPI APPROVED 留证（m3_rive_dpi_approved.md）

> 本文件是闸门 `docs/m3_rive_dpi_gate_LOCKED.md` 的 APPROVED 留证，不修改 LOCKED 闸门的 body 或 hash。

## 引用的 LOCK（不变）
- 闸门路径：`docs/m3_rive_dpi_gate_LOCKED.md`
- STATUS=LOCKED
- LOCKED_AT=2026-10-28 20:39:58 AWST
- GATE_BODY_SHA256=92e6b4e9fd1014faf74714eb6cb20fc7e998169dc1171785bb8ea43f68d97bf5（落盘前已复算，MATCH）
- 哈希约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。

## APPROVED 留证
- APPROVED_BY=pidan
- APPROVED_AT=2026-10-28 21:28:50 AWST（Australia/Perth）
- APPROVED_NOTE=会话回复「APPROVED M3-RIVE-DPI」，在 LOCK 92e6b4e9…97bf5 之后

## 范围（写死）
| 字段 | 值 |
|---|---|
| 票 | M3-RIVE-DPI：DESKTOP-IJI1KJG（G3），合成表面路径（F1） |
| 采数 | **是**：`m3/run_m3_dpi.ps1 -Sessions 3`，数据目录 `<ROOT>\artifacts_m3dpi`，结果提交到 `m3/results_dpi/` |
| 缩放切换 | 操作员按探针提示，在“设置 → 显示 → 缩放”里手动切换 100% ↔ 125%，每会话 4 次（G1/G2） |
| 指针注入 | **否**（G4） |
| 判据 | 按 LOCKED G2：①②④⑤⑥ 每会话满足即 PASS；③（重新光栅化延迟）、⑦（内存）只记录 |
| 落地 / 合入 main | **否**（结果评审后经 PR 合入） |
| composition.h | 不改（`a55d4952…e418`） |

## 禁令
不重开 S1/R-3/R-2；禁影子 IV；构建失败不改 Rive 参数硬凑（记 BLOCKED_TOOLCHAIN）；成功 ≠ 采用；PERF-CI 仍 HOLD。
