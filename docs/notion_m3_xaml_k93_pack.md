# M3-XAML-K93 结题中文包（notion_m3_xaml_k93_pack.md）

> DECISION_AT=2026-11-02 09:20:00 Australia/Perth（AWST）  
> 权威结题：`docs/m3_xaml_k93_result.md`  
> 闸门：`docs/m3_xaml_k93_gate_LOCKED.md`；GATE_BODY_SHA256=`22fb8b92d8703619215ffaae3f0d5d9efea003951bb44b42d790d488ac28b657`（协调/workbuddy + tip 旁证 **MATCH**；**本地未见 LOCK 文件**）  
> APPROVED：原 APPROVED **仍有效**（tip 旁证 `docs/m3_xaml_k93_approved.md` — pidan @ 2026-11-01 20:58:26 AWST）；Decision C **不重新批采数**  
> Decision C：`docs/notion_m3_xaml_k93_toolchain_decision.md` — **IJI_BYPASS_COPY**  
> 计划：`docs/m3_plan_DRAFT.md` §7 / WP1 机差旁证（K93 + Rive）  
> 前序：XAML-LOST / XAML-REBUILD=KNOWN_LIMIT / FOLLOW=CLOSED PASS（**均勿翻**）  
> 分支：tip_read=`cbc5fdb285570fde7d71fb7f3782d02f1663f8fd`；commit=`bfd76c3660b157ca3c903bb9f2afae07339c6f55` @ **`claude/m3-xaml-rebuild`**  
> composition.h PRE=POST CRLF=`33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97` LF=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` edited=False  

## 结论

- **票级 = CLOSED**；标签 **`XAML-K93-PASS`**；协调对照 **METRICS_PASS（有条件）**。
- **是否采用 = 有条件采用验证结论**（成功 ≠ 采用；**不合 main**）。
- **有条件要点（必须写清）**：
  1. `build_path=IJI_BYPASS_COPY`；`k93_native=BLOCKED_TOOLCHAIN` — IJI 旁路是 **Decision C 明确路径**，**不算作弊**；采用时标明「**构建路径非 K93 原生**」。
  2. sessions=1×RECORDED；dryrun=RECORDED；`lost_detected=1 recovered=1 lost_now=0 rive_abort=none` → **探测+恢复在 K93 packaged+with_rive 上成立**；`recover_ms=4504.25` **按闸门记录**（不升硬 FAIL）。
  3. `W_pre=1440x756` / `W_post=1153x605` — results/ONELINE **无 TEXT-OK/TEXT-LOST 目视字段** → **缺文字裁决**（如实挂账；本 pack **不**代目视升格、**不**授权复采）。
- **Success≠adopt**；**PERF-CI HOLD**；composition.h **未触**。
- ONELINE（结题）：  
  `M3-XAML-K93 | tip_read=cbc5fdb2…f8fd | commit=bfd76c36…6f55 @ claude/m3-xaml-rebuild | build_path=IJI_BYPASS_COPY | k93_native=BLOCKED_TOOLCHAIN | gate MATCH 22fb8b92…b657 | dryrun RECORDED | s1 RECORDED with_rive=1 packaged=1 inject_ok=1 shots=20 | lost=1 recovered=1 lost_now=0 recover_ms=4504.25 abort=none reason=0x887A0005 | W_pre=1440x756 W_post=1153x605（缺TEXT字段）| ticket=XAML-K93-PASS | METRICS_PASS有条件 | 有条件采用；不合main | Success≠adopt | PERF-CI HOLD`

## 口径（METRICS_PASS 有条件；Success≠adopt）

| 选项 | 决断 | 理由 |
|---|---|---|
| XAML-K93-FAIL | **否** | 非采数失败；dryrun+session RECORDED；lost+recover 成立；abort=none |
| XAML-K93-INCONCLUSIVE | **否（票级）** | 非「未进入裁决」；探测/恢复可裁决；TEXT 缺字段仅挂账条件 |
| **XAML-K93-PASS** | **是（有条件 METRICS）** | G1 丢失检测已答；恢复成立；旁路构建/缺 TEXT 为条件 |
| 采用 tip 默认可合入 main | **否** | Success≠adopt；不合 main |
| 采用验证结论（lost+recover） | **有条件是** | 须标明 IJI_BYPASS_COPY；缺 TEXT 不升「XAML 必能画字」 |
| 把 IJI 旁路当作弊 / 作废票 | **否** | Decision C 明示立刻执行 |
| 翻 CLOSED / 解冻 PERF-CI | **否** | 硬禁 |

### 有条件采用 vs 不采用（写清）

| 采用什么 | 不采用什么 |
|---|---|
| 验证结论：K93 packaged+with_rive 上 **lost_detected+recovered** 成立（abort=none；reason=0x887A0005） | 整 tip / 分支 **默认可合入 main** |
| 实验证据：commit `bfd76c36…` + `m3/results_xaml_k93/` 26 files；dll `70ed8d14…` | 把旁路写成 **K93 原生工具链已修好** |
| Decision C 路径记录（IJI→K93 拷 dll） | 把缺 TEXT 字段写成已目视 TEXT-OK/LOST |
| — | PERF-CI 解冻；本 pack **授权新采数**；翻 FOLLOW/REBUILD |

## 授权核验

| 项 | 值 |
|---|---|
| LOCKED | tip 旁证 `m3_xaml_k93_gate_LOCKED.md`；hash **MATCH** `22fb8b92…b657`；**本机 box 未见 LOCK 文件** |
| approved | 原 APPROVED 仍有效；tip 旁证 pidan 2026-11-01 20:58:26 AWST |
| Decision C | HIT；不重新批采数 |
| 本结题采数 | **否** |
| tip_read / commit | `cbc5fdb2…f8fd` / `bfd76c36…6f55` |

## 事实表

| 项 | 值 |
|---|---|
| TICKET / STATUS | `XAML-K93-PASS` / **CLOSED** |
| METRICS | **METRICS_PASS（有条件）** |
| 采用 | **有条件采用验证结论**；不合 main；**构建路径非 K93 原生** |
| build_path / k93_native | **IJI_BYPASS_COPY** / **BLOCKED_TOOLCHAIN** |
| dll_sha256 | `70ed8d14ff0e73793d36dd6799e8972f9877cd652eee54401f35b444de6da15f` |
| dryrun / sessions | RECORDED / 1×RECORDED（with_rive=1 packaged=1 inject_ok=1 shots=20） |
| rive lost/recover | lost_detected=1；recovered=1；lost_now=0；recover_ms=4504.25；abort=none；reason=0x887A0005 |
| W_pre / W_post | **1440×756** / **1153×605**；**缺 TEXT-OK/LOST 目视字段** |
| results | `m3/results_xaml_k93/` **26** files |
| composition.h | 未触；PRE=POST |
| PERF-CI | HOLD |

## 产物清单

- `docs/m3_xaml_k93_result.md`（权威结题）
- `docs/m3_xaml_k93_result_brief.md`
- `docs/notion_m3_xaml_k93_pack.md`（本包）
- `docs/notion_m3_xaml_k93_next.md`
- brief 一行：`docs/m2_mainline_brief.md`
- Decision C（已有）：`docs/notion_m3_xaml_k93_toolchain_decision.md`
- 只读结果：`m3/results_xaml_k93/`（勿改；本根文本子集 + commit 全量）
- 产物前缀 only：`m3_xaml_k93_*` / `notion_m3_xaml_k93_*`

## 禁令复述

- 不改 composition.h；不合 main；Success≠adopt；PERF-CI HOLD
- 不翻 CLOSED 旧票；本 pack/next **不授权新采数**
- 工具链 A/B 挂账 **不挡本票**；不为凑原生编译改代码硬过本票

## 挂账 / 残差

| 项 | 处置 |
|---|---|
| 工具链 A（钉/降 DirectX-Headers） | **挂账**；另开文档/工具链票 |
| 工具链 B（升/对齐 Windows SDK） | **挂账**；另开文档/工具链票 |
| TEXT 目视缺字段 | 挂账；≠采数令 |
| recover_ms≈4.5s | 记录 |
| 下一张 | 见 next → **MATRIX DRAFT（文档）**；勿重复授权采数 |

## 下一票决断

详见 `docs/notion_m3_xaml_k93_next.md`。  
**主推：`M3-MATRIX` DRAFT（纯文档）— 不授权真机；工具链 A/B 挂账并行文档；勿重复授权采数。**

— notion ai（M3-XAML-K93 结题；CLOSED PASS；METRICS_PASS 有条件；有条件采用；不合 main；IJI_BYPASS_COPY）
