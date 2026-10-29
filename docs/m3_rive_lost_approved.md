# M3-RIVE-LOST APPROVED 留证（m3_rive_lost_approved.md）

> 本文件是闸门 `docs/m3_rive_lost_gate_LOCKED.md` 的 APPROVED 留证，不修改 LOCKED 闸门的 body 或 hash。

## 引用的 LOCK（不变）
- 闸门路径：`docs/m3_rive_lost_gate_LOCKED.md`
- STATUS=LOCKED
- LOCKED_AT=2026-10-29 12:31:48 AWST
- GATE_BODY_SHA256=753c0a6c82f06c81cf741eecba278b926127771befa2a51b515a925a7873f27d（落盘前已复算，MATCH）
- 哈希约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。

## APPROVED 留证
- APPROVED_BY=pidan
- APPROVED_AT=2026-10-29 12:48:46 AWST（Australia/Perth）
- APPROVED_NOTE=会话回复「APPROVED M3-RIVE-LOST」，在 LOCK 753c0a6c…f27d 之后

## 范围（写死）
| 字段 | 值 |
|---|---|
| 票 | M3-RIVE-LOST：DESKTOP-IJI1KJG（K3），合成表面路径（F1） |
| 代码 | 分支 `claude/m3-rive-lost`（LOCK 时代码提交 `23e08de`） |
| 采数 | **是**：管理员 PowerShell 运行 `m3/run_m3_lost.ps1 -Sessions 3`，数据目录 `<ROOT>\artifacts_m3lost`，结果提交到 `m3/results_lost/` |
| 注入 | 真实 TDR：`dxcap -forcetdr`（K1）；每会话 2 次，间隔 ≥ 45 s，会话间 ≥ 120 s，全程 ≤ 8 次，由 runner 强制（K4/K5） |
| 模拟冒烟 | `-Simulate` 可先跑 1 个会话；数据不提交、不计入（K1） |
| 停止条件 | 蓝屏、显示器 30 s 内没恢复、驱动报错弹窗、runner 打出 `STOP:` → 立即停止，记 `INVALID_SYSTEM`，当天不重试（K5） |
| 崩溃 | `FAIL_ABORT` / `CRASHED_NO_VERDICT` 判 FAIL，照常交付，不重跑凑数（K7） |
| 判据 | 按 LOCKED G2：①②③④⑤ 每会话满足即 PASS；⑥（丢帧窗口）、⑦（内存、旧设备引用）只记录 |
| 落地 / 合入 main | **否**（结果评审后经 PR 合入） |
| composition.h | 不改（`a55d4952…e418`） |
| TDR 注册表 | 不改 |

## 禁令
不重开 S1/R-3/R-2；禁影子 IV；不改 TDR 注册表；构建失败不改 Rive 硬凑（记 BLOCKED_TOOLCHAIN）；成功 ≠ 采用；PERF-CI 仍 HOLD。
