# M0-B APPROVED 留证（m0b_win11_approved.md）

> 本文件是闸门 `docs/m0b_win11_gate_r2_LOCKED.md` 的 APPROVED 留证，不修改 LOCKED 闸门的 body 或 hash。

## 引用的 LOCK（不变）
- 闸门路径：`docs/m0b_win11_gate_r2_LOCKED.md`（r2；取代 r1 `5e1bbb6d…f203`）
- STATUS=LOCKED
- LOCKED_AT=2026-10-28 09:16:57 AWST
- GATE_BODY_SHA256=fd3acfc787564137532cc52adb64066427a88b159f0c6b4312dfd9dece40505c（落盘前已复算，MATCH）
- 哈希约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。

## APPROVED 留证
- APPROVED_BY=pidan
- APPROVED_AT=2026-10-28 09:18:00 AWST（Australia/Perth）
- APPROVED_NOTE=会话回复「APPROVED M0-B」，在 r2 LOCK fd3acfc7…505c 之后

## 范围（写死）
| 字段 | 值 |
|---|---|
| 票 | M0-B（Win11，build ≥ 22000，机器与 DESKTOP-K93N3P8 不同） |
| 采数 | **是**：`m0/run_m0.ps1 -Inject -Sessions 3`，有管理员权限时加 `-PresentMon` |
| SendInput | 允许，仅限 M0（沿用 M0-A 的 D1） |
| M0-2 正式判定 | 刷新率 ≥75Hz 时判定，门槛 \|misalign_ms\| ≤ 2 ms；<75Hz 时只记录（E1） |
| E1–E4 | 按 r2 G3 |
| 落地 / 合入 main | **否**（结果评审后经 PR 合入） |
| composition.h | 不改（`a55d4952…e418`） |
| PERF-CI | 仍 HOLD |

## 禁令
不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；构建失败不改 Rive 参数硬凑（记 BLOCKED_TOOLCHAIN）；成功 ≠ 采用。
