# Z4 ETW + L1 smoke r1 APPROVED 留证（z4_etw_l1_smoke_r1_approved.md）

> 本文件是闸门 `docs/z4_etw_l1_smoke_gate_r1_LOCKED.md` 的 APPROVED 留证，**不修改** LOCKED 闸门的 body 或 hash（闸门内 APPROVED_* 占位行计入 GATE_BODY_SHA256，填入会破坏哈希，故 LOCKED 文件保持原样不动）。

## 引用的 LOCK（不变）

- 闸门路径：`docs/z4_etw_l1_smoke_gate_r1_LOCKED.md`
- STATUS=LOCKED
- LOCKED_AT=2026-11-10 23:09:00 AWST（Australia/Perth）
- GATE_BODY_SHA256=acb89d1b0cb975e5d63ae2c6fc47adcbc74dddd959b63089b2546b692cadab14（落盘前已复算，MATCH）
- 哈希约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。

## APPROVED 留证

- APPROVED_BY=pidan
- APPROVED_AT=2026-11-10 23:16 AWST（Australia/Perth，= UTC+8）
- APPROVED_PHRASE=会话原文「APPROVED Z4 r1」（一字不差）
- APPROVED_NOTE=pidan 于 2026-11-10 23:16 AWST 在对话中发送「APPROVED Z4 r1」，在 LOCK `acb89d1b0cb975e5d63ae2c6fc47adcbc74dddd959b63089b2546b692cadab14` 之后。本留证满足闸门对「APPROVED Z4 r1」口头/书面批准的要求；LOCKED 文件内的 APPROVED_* 行按惯例保持空白。

## 采数仍阻断（写死）

本 APPROVED **不等于立即授权采数**。闸门要求两项同时满足，目前只满足第 (1) 项：

1. pidan 已发送「APPROVED Z4 r1」，且本留证已记录 — **已满足**。
2. Z3a C-ABI L1 构建已存在，并通过 `docs/z3a_cabi_l1_ticket.md` 的验收 — **尚未满足**。分支 `claude/z3a-cabi-l1` **尚不存在**。在该分支落地、K93 构建通过票内验收（能编过、API 对得上冻结 L1 三表、`fm_capi_test` PASS、开发冒烟能打出 E1 事件、`composition.h` PRE=POST）之前，**禁止**按本闸门采数。把 Z3a 票内的开发冒烟当作 Z4 会话 = INVALID。

采数开始前还须在回报中**声明采数所用代码 tip**（应为 `claude/z3a-cabi-l1` 上已通过验收的提交 SHA）。未声明 tip、声明与实际不符、或使用影子 / 未声明 tip = INVALID。

## 范围（写死）

| 字段 | 值 |
|---|---|
| 票 | Z4 ETW + L1 smoke r1：C# API usable（G2 ①–⑥）；非 PERF-CI |
| 宿主 | 仅 MotionSoftDemo（E4）；经现有软件 / C-ABI 桥；探针为 L1 名字。不在 FluentHub / Files 内跑 |
| 代码 | 分支 `claude/z3a-cabi-l1`（尚不存在）。采数 tip 必须在开跑前声明，且须已通过 Z3a 票验收 |
| 机台 / 会话（E2） | 仅 K93N3P8（Win10）与 IJI1KJG（Win11）；每台 ≥ 3 个独立进程会话；unpackaged 与打包各至少 1 个。不扩机台 |
| 采集（E3） | EventPipe（无需管理员）为主路径；`logman` ETL→WPA 仅为有管理员时的可选对照，不是 PASS 必要条件 |
| 结果目录 | 闸门正文未单列目录名。本留证指定采数结果提交到 **`z4/results_etw_l1_smoke_r1/`**；工作副本可用 `<ROOT>\artifacts_z4_etw_l1_smoke_r1`。不得覆写 `artifacts/m2_02_*`、`m3/results_*` 或其他已结题目录 |
| ETW（E1） | provider `FluidMotion-Motion`；GUID `8f3a2c1d-6b4e-4a91-9c7d-2e5f8a1b3c4d`；事件名与 id 冻结 |
| 落地 / 合入 main | **否**。Success≠adopt |
| composition.h | 不改 |

## 禁令

同闸门 G0 / G5：不合 main；不改 `composition.h`；PERF-CI 仍 HOLD；不解冻 Bv；不重开 S1 / R-3 / R-2；禁影子 IV；不把 0.3·T 写成 API 默认；L1 smoke PASS ≠ PERF-CI ≠ 采用。FAIL 不重跑凑数。Z3a L1 构建未通过验收前不得采数。
