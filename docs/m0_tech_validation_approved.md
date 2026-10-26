# M0-A APPROVED 留证（m0_tech_validation_approved.md）

> 本文件是闸门 `docs/m0_tech_validation_gate_LOCKED.md` 的 APPROVED 留证，不修改 LOCKED 闸门的 body 或 hash。

## 引用的 LOCK（不变）
- 闸门路径：`docs/m0_tech_validation_gate_LOCKED.md`
- STATUS=LOCKED
- LOCKED_AT=2026-10-26 21:38:45 AWST
- GATE_BODY_SHA256=942d6965dae1aa41f3b853923d08a11aac9714d60018888a5c486bfde5809768（落盘前已复算，MATCH）
- 哈希约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。

## APPROVED 留证
- APPROVED_BY=pidan
- APPROVED_AT=2026-10-26 21:43:10 AWST（Australia/Perth）
- APPROVED_NOTE=会话回复「APPROVED M0-A」，在 LOCK 942d6965…9768 之后；此前回复「D1-D7按建议」

## 范围（写死）
| 字段 | 值 |
|---|---|
| 票 | M0-A（60Hz 本机 DESKTOP-K93N3P8） |
| 采数 | **是**：`m0/run_m0.ps1 -Inject -Sessions 3`（PresentMon 可选） |
| D1 SendInput | **允许，仅限 M0** |
| D2 | M0-A 现在做；M0-B（≥120Hz、Win11）另票 |
| D3–D7 | 按 LOCKED G3 |
| 落地 / 合入 main | **否**（结果评审后另行决定） |
| composition.h | 不改（`a55d4952…e418`） |
| PERF-CI | 仍 HOLD |

## 禁令
不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；构建失败不改参数硬凑（记 BLOCKED_TOOLCHAIN）；成功 ≠ 采用。
