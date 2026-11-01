# M3-RIVE-FOLLOW APPROVED 留证（m3_rive_follow_approved.md）

> 本文件是闸门 `docs/m3_rive_follow_gate_LOCKED.md` 的 APPROVED 留证，不修改 LOCKED 闸门的 body 或 hash。
> 补记：pidan 在 2026-11-01 16:26:53（Asia/Shanghai）的同一条消息中同时批准了 M3-XAML-REBUILD 和 M3-RIVE-FOLLOW（见 `claude/m3-xaml-rebuild` 分支 `docs/m3_xaml_rebuild_approved.md` 的 APPROVED_NOTE）；当时只落了 REBUILD 的留证，本文件补上 FOLLOW 的。本票尚未采数。

## 引用的 LOCK（不变）
- 闸门路径：`docs/m3_rive_follow_gate_LOCKED.md`
- STATUS=LOCKED
- LOCKED_AT=2026-10-30 17:40:39 AWST
- GATE_BODY_SHA256=285e39dc9c1100f92ddddc5560a3631b94129a9b56c561c9803c8cbee57eaab3（落盘前已复算，MATCH）
- 哈希约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。

## APPROVED 留证
- APPROVED_BY=pidan
- APPROVED_AT=2026-11-01 16:26:53 Asia/Shanghai（UTC+8）
- APPROVED_NOTE=同一条消息中的「APPROVED M3-RIVE-FOLLOW」，在 LOCK 285e39dc…eaab3 之后

## 范围（写死）
| 字段 | 值 |
|---|---|
| 票 | M3-RIVE-FOLLOW：DESKTOP-IJI1KJG，75Hz，缩放 100% |
| 代码 | 分支 `claude/m3-follow`（LOCK 时代码提交 `b9e6f32`；需重建 m0rive.dll） |
| 采数 | **是**：`m3/run_m3_follow.ps1 -Sessions 3`，数据目录 `<ROOT>\artifacts_m3follow`，结果提交到 `m3/results_follow/` |
| 合成输入 | `SendInput` 仅限本票，只移动光标、不点击（P4） |
| 判读 | 按 LOCKED G2 / P6：`split_default`、`latch_default`、`seam_ok`（记录型）；任一路径正向或反向检出不足一半 → INVALID |
| 落地 / 合入 main | **否**（结果评审后经 PR） |
| composition.h | 不改（LF `a55d4952…e418`） |

## 禁令
不重开 S1/R-3/R-2；禁影子 IV；构建失败不改代码硬凑；提交结果前先从 origin 同步，只新增结果文件；运行时应用窗口保持在最前、不被遮挡；成功 ≠ 采用；PERF-CI 仍 HOLD。
