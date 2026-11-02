# M3-XAML-LOST APPROVED 留证（m3_xaml_lost_approved.md）

> 本文件是闸门 `docs/m3_xaml_lost_gate_LOCKED.md` 的 APPROVED 留证，不修改 LOCKED 闸门的 body 或 hash。

## 引用的 LOCK（不变）
- 闸门路径：`docs/m3_xaml_lost_gate_LOCKED.md`
- STATUS=LOCKED
- LOCKED_AT=2026-10-30 13:36:05 AWST
- GATE_BODY_SHA256=69a52e2de66153348bff5197dbb037dd2650c26055b834173abde6ae6a1e1f55（落盘前已复算，MATCH）
- 哈希约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。

## APPROVED 留证
- APPROVED_BY=pidan
- APPROVED_AT=2026-10-30 13:42:56 AWST（Australia/Perth）
- APPROVED_NOTE=会话回复「APPROVED M3-XAML-LOST」，在 LOCK 69a52e2d…1f55 之后

## 范围（写死）
| 字段 | 值 |
|---|---|
| 票 | M3-XAML-LOST（诊断票）：DESKTOP-IJI1KJG |
| 代码 | 分支 `claude/m3-xaml-lost`（LOCK 时代码提交 `4c04450`；m0rive.dll 沿用 RECOVER 构建） |
| 采数 | **是**：管理员 PowerShell 运行 `m3/run_m3_xaml_lost.ps1`（默认 3 个普通会话 + 1 个带 Rive 会话），数据目录 `<ROOT>\artifacts_m3xaml`，结果提交到 `m3/results_xaml_lost/` |
| 注入 | `pnputil /restart-device <显卡实例 ID>`；自检 1 次 + 每会话 1 次 = 5 次；间隔 ≥ 45 s，会话前冷却 ≥ 120 s，全程 ≤ 8 次 |
| 走流程 | `-DryRun` 可先跑；数据不提交、不计入 |
| 停止条件 | 蓝屏、显示器 30 s 内没恢复、驱动报错弹窗、显卡状态 30 s 内没回到 OK、runner 打出 `STOP:` → 立即停止，当天不重试 |
| 判读 | 记录型（N4）：`XAML-RECOVERS` / `XAML-LOST-REMEDY-<操作>` / `XAML-LOST`；不判 PASS/FAIL |
| 落地 / 合入 main | **否** |
| composition.h | 不改（LF `a55d4952…e418`） |
| TDR 注册表 / 驱动设置 | 不改 |

## 禁令
不重开 S1/R-3/R-2；禁影子 IV；不改 TDR 注册表；构建失败不改代码硬凑（记 BLOCKED_TOOLCHAIN）；提交结果前先从 origin 同步，只新增结果文件；诊断结果 ≠ 采用；PERF-CI 仍 HOLD。
