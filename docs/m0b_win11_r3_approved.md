# M0-B r3 APPROVED 留证（m0b_win11_r3_approved.md）

> 本文件是闸门 `docs/m0b_win11_gate_r3_LOCKED.md` 的 APPROVED 留证，不修改 LOCKED 闸门的 body 或 hash。r2 的 approved（`docs/m0b_win11_approved.md`）只适用于 r2，不延续到 r3。

## 引用的 LOCK（不变）
- 闸门路径：`docs/m0b_win11_gate_r3_LOCKED.md`（r3；取代 r2 `fd3acfc7…505c`）
- STATUS=LOCKED
- LOCKED_AT=2026-10-28 14:53:18 AWST
- GATE_BODY_SHA256=741729784693893dedb6190b5d2768617045554ae90a58dfe00fd52855f48ede（落盘前已复算，MATCH）
- 哈希约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。

## APPROVED 留证
- APPROVED_BY=pidan
- APPROVED_AT=2026-10-28 15:26:25 AWST（Australia/Perth）
- APPROVED_NOTE=会话回复「APPROVED M0-B r3，提权 + PresentMon」，在 r3 LOCK 74172978…8ede 之后

## 范围（写死）
| 字段 | 值 |
|---|---|
| 票 | M0-B r3：同一台 DESKTOP-IJI1KJG（SMBIOS UUID `03FF0210-04E0-057F-E006-E90700080009`），用探针 `M0 v3` 整票重跑 |
| 采数 | **是**：`m0/run_m0.ps1 -Inject -Sessions 3 -ArtDir <ROOT>\artifacts_r3` |
| 提权 / PresentMon | **是**：在提权 shell 中运行，并传 `-PresentMon`（2.x）。如果实际无法提权或 PresentMon 失败，照常跑完并在回报中写明，M0-4 仍只有 API 层结论 |
| SendInput | 允许，仅限 M0 |
| 判据 | 按 r3（与 r2 相同）：M0-2 \|misalign_ms\| ≤ 2 ms，且每条路径每个会话样本 ≥ 200；M0-1a 按 G2-M0-1 ①–④；GPU 丢弃样本超过帧数 10% 时 GPU P95 无效 |
| 数据 | 提交到 `m0/results_m0b_r3/`；不动、不混用 `m0/results_m0b/` |
| 落地 / 合入 main | **否**（结果评审后经 PR 合入） |
| composition.h | 不改（`a55d4952…e418`） |
| PERF-CI | 仍 HOLD |

## 禁令
不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；构建失败不改 Rive 参数硬凑（记 BLOCKED_TOOLCHAIN）；不做补测拼接；成功 ≠ 采用。
