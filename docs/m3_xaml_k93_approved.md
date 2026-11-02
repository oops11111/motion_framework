# M3-XAML-K93 APPROVED 留证（m3_xaml_k93_approved.md）

> 本文件是闸门 `docs/m3_xaml_k93_gate_LOCKED.md` 的 APPROVED 留证，不修改 LOCKED 闸门的 body 或 hash。

## 引用的 LOCK（不变）
- 闸门路径：`docs/m3_xaml_k93_gate_LOCKED.md`
- STATUS=LOCKED
- LOCKED_AT=2026-11-01 18:37:27 AWST
- GATE_BODY_SHA256=22fb8b92d8703619215ffaae3f0d5d9efea003951bb44b42d790d488ac28b657（落盘前已复算，MATCH）
- 哈希约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。

## APPROVED 留证
- APPROVED_BY=pidan
- APPROVED_AT=2026-11-01 20:58:26 AWST（Australia/Perth）
- APPROVED_NOTE=会话回复「APPROVED M3-XAML-K93」，在 LOCK 22fb8b92…b657 之后

## 范围（写死）
| 字段 | 值 |
|---|---|
| 票 | M3-XAML-K93（诊断票）：DESKTOP-K93N3P8 |
| 代码 | 分支 `claude/m3-xaml-rebuild`（S1/S2 改动 `104e5f9`；需在该机构建带设备丢失恢复的 m0rive.dll） |
| 采数 | **是**：`m3/run_m3_xaml_lost.ps1 -Rebuild -PlainSessions 0 -RiveSessions 1`（启动方式沿用上次 C 组），数据目录 `<ROOT>\artifacts_m3xaml_k93`，结果提交到 `m3/results_xaml_k93/` |
| 注入 | `pnputil /restart-device`；自检 1 次 + 会话 1 次 = 2 次 |
| 停止条件 | 蓝屏、显示器 30 s 内没恢复、驱动报错弹窗、显卡状态 30 s 内没回到 OK、runner 打出 `STOP:` → 立即停止，当天不重试 |
| 判读 | 按 LOCKED G2 的三行表（以 `rive.lost_detected` 为前提） |
| 落地 / 合入 main | **否** |
| composition.h | 不改（LF `a55d4952…e418`） |

## 禁令
不重开 S1/R-3/R-2；禁影子 IV；不改 TDR 注册表；构建失败不改代码硬凑（记 BLOCKED_TOOLCHAIN）；提交结果前先从 origin 同步，只新增结果文件；开始前关闭聊天软件并打开“请勿打扰”；诊断结果 ≠ 采用；PERF-CI 仍 HOLD。
