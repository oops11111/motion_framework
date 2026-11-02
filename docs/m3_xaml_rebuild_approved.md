# M3-XAML-REBUILD APPROVED 留证（m3_xaml_rebuild_approved.md）

> 本文件是闸门 `docs/m3_xaml_rebuild_gate_LOCKED.md` 的 APPROVED 留证，不修改 LOCKED 闸门的 body 或 hash。

## 引用的 LOCK（不变）
- 闸门路径：`docs/m3_xaml_rebuild_gate_LOCKED.md`
- STATUS=LOCKED
- LOCKED_AT=2026-10-30 17:35:36 AWST
- GATE_BODY_SHA256=3ea74c371ae983cebe480e1b4dea99e61aec92cfbe9646fb87b91b7a1d456fdc
- 哈希约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。
- 分支：`claude/m3-xaml-rebuild`；派工：`docs/workbuddy_m3_xaml_rebuild_dispatch.md`

## APPROVED 留证
- APPROVED_BY=pidan
- APPROVED_AT=2026-11-01 16:26:53 Asia/Shanghai（UTC+8）
- APPROVED_NOTE=会话明确两行「APPROVED M3-XAML-REBUILD」与「APPROVED M3-RIVE-FOLLOW」；授权 DryRun 之后的真机驱动重启采数（A/B on IJI1KJG，C on K93N3P8）

## 范围（写死）
| 字段 | 值 |
|---|---|
| 票 | M3-XAML-REBUILD：诊断票；W_pre / W_post 文字能否画 |
| 采数 | **是**：按派工单第 4 步 A/B/C；结果 `m3/results_xaml_rebuild/{a,b,c}/` |
| DryRun | 可先行；本 APPROVED 同时覆盖第 4 步真重启 |
| composition.h | 不改（`a55d4952…e418`） |
| 合 main / 采用 | **否**（Success≠adopt） |

## 禁令
同 LOCKED G0/G5；不为 R2 修部署；不改 TDR；PERF-CI HOLD。
