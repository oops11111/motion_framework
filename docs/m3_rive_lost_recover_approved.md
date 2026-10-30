# M3-RIVE-LOST-RECOVER APPROVED 留证（m3_rive_lost_recover_approved.md）

> 本文件是闸门 `docs/m3_rive_lost_recover_gate_LOCKED.md` 的 APPROVED 留证，不修改 LOCKED 闸门的 body 或 hash。r1、r2 的 approved 只对各自闸门有效。

## 引用的 LOCK（不变）
- 闸门路径：`docs/m3_rive_lost_recover_gate_LOCKED.md`
- STATUS=LOCKED
- LOCKED_AT=2026-10-30 11:31:47 AWST
- GATE_BODY_SHA256=16202071fd08d091d0a0d825eda2297297ae19227d72a6cf981d4ed8b25484ce（落盘前已复算，MATCH）
- 哈希约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。

## APPROVED 留证
- APPROVED_BY=pidan
- APPROVED_AT=2026-10-30 11:47:20 AWST（Australia/Perth）
- APPROVED_NOTE=会话回复「APPROVED M3-RIVE-LOST-RECOVER」，在 LOCK 16202071…84ce 之后

## 范围（写死）
| 字段 | 值 |
|---|---|
| 票 | M3-RIVE-LOST-RECOVER：DESKTOP-IJI1KJG，合成表面路径（F1） |
| 代码 | 分支 `claude/m3-rive-lost`（LOCK 时代码提交 `90c4180`；**需重建 m0rive.dll**） |
| 采数 | **是**：管理员 PowerShell 运行 `m3/run_m3_lost.ps1 -Sessions 3`，数据目录 `<ROOT>\artifacts_m3lost_recover`，结果（含 `_stderr.txt`）提交到 `m3/results_lost_recover/` |
| 注入 | 同 r2：`pnputil /restart-device <显卡实例 ID>`；自检 1 次；每会话 2 次，间隔 ≥ 45 s，会话前冷却 ≥ 120 s，全程 ≤ 8 次 |
| 模拟冒烟 | `-Simulate` 先跑 1 个会话；数据不提交、不计入 |
| 停止条件 | 蓝屏、显示器 30 s 内没恢复、驱动报错弹窗、显卡状态 30 s 内没回到 OK、runner 打出 `STOP:` → 立即停止，当天不重试 |
| abort | `FAIL_ABORT` 判 FAIL，照常交付（含 stderr 与 `abort_stage`），不重跑凑数 |
| 判据 | 沿用 r2（r1 G2）：①②③④⑤ 每会话满足即 PASS；⑥⑦只记录 |
| Rive | 不改（M3） |
| 落地 / 合入 main | **否**（结果评审后经 PR 合入） |
| composition.h | 不改（LF `a55d4952…e418`） |
| TDR 注册表 / 驱动设置 | 不改 |

## 禁令
不重开 S1/R-3/R-2；禁影子 IV；不改 TDR 注册表；构建失败不改代码硬凑（记 BLOCKED_TOOLCHAIN）；r1、r2 数据不改不混用；提交结果前先从 origin 同步，只新增结果文件；成功 ≠ 采用；PERF-CI 仍 HOLD。
