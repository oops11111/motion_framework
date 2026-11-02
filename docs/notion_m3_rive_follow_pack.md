# M3-RIVE-FOLLOW 结题中文包（notion_m3_rive_follow_pack.md）

> DECISION_AT=2026-11-02 08:11:19 Australia/Perth（AWST）  
> 权威结题：`docs/m3_rive_follow_result.md`  
> 闸门：`docs/m3_rive_follow_gate_LOCKED.md`；GATE_BODY_SHA256=`285e39dc9c1100f92ddddc5560a3631b94129a9b56c561c9803c8cbee57eaab3`（结题复算 **MATCH**；**SendInput 现行**）  
> APPROVED：`docs/m3_rive_follow_approved.md` — pidan @ 2026-11-01 16:26:53 Asia/Shanghai  
> 计划：`docs/m3_plan_DRAFT.md` WP2 / §7 第 3 张票  
> 前序：DPI PASS；LOST r1/r2/RECOVER；XAML-LOST；XAML-REBUILD=KNOWN_LIMIT（**均勿翻**）  
> 分支 tip/commit：`0fb0760d0897f378e82b0242a803d18afcbf9e4f` @ **`claude/m3-follow`**（parent `9715164…`；IJI 直推）  
> composition.h PRE=POST CRLF=`33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97`；edited=False  
> PropertySet bak `0f85a952…`：**勿当现行**

## 结论

- **票级 = CLOSED**；标签 **`RIVE-FOLLOW-PASS`**；协调对照 **METRICS_PASS**。
- **是否采用 = 有条件采用**（WP2 **设计约束结论** / 实验分支 evidence；**不合 main**）。
- **Success≠adopt**；**PERF-CI HOLD**；composition.h **未触**。
- ONELINE（结题）：  
  `M3-RIVE-FOLLOW | commit=0fb0760d…9e4f @ claude/m3-follow | gate=285e39dc…aab3 MATCH | sessions=3 RECORDED | A/B/C/D p50≈65/63/41/54 | c_vs_a=1.80/1.89/2.00 | b_vs_a=0.09/0.20/0.20 | seam=-1.93/-1.93/-1.95 | budget_ok=1/1/1 | split_default=1 latch_default=0 seam_ok=0 | ticket=RIVE-FOLLOW-PASS | METRICS_PASS | 有条件采用设计约束；不合main | PERF-CI HOLD`

## 口径（METRICS_PASS；采用有条件）

| 选项 | 决断 | 理由 |
|---|---|---|
| RIVE-FOLLOW-FAIL | **否** | 非采数失败；3×RECORDED；检出满；预算过 |
| RIVE-FOLLOW-INCONCLUSIVE / INVALID | **否** | 产物齐；hash MATCH；已 APPROVED；非 INVALID_FEW_STEPS；bind_ok 非本闸硬字段 |
| **RIVE-FOLLOW-PASS** | **是** | G2 可裁决；METRICS_PASS；延迟记录型不挡 |
| 采用 tip 默认可合入 main | **否** | Success≠adopt；不合 main |
| 采用 WP2 设计约束 | **有条件是** | C=默认拆分；B不升默认；接缝须弹簧阻尼 |
| 延迟硬 FAIL（对齐 M0 12–45） | **否** | 现行 SendInput LOCK：延迟可裁决对比+记录，不升硬对齐 |
| 翻 CLOSED / 阻拦 K93 | **否** | 硬禁 |

### 有条件采用 vs 不采用（写清）

| 采用什么 | 不采用什么 |
|---|---|
| WP2 设计约束：跟随部件 **默认拆合成器 visual（C）** | 整 tip / 分支 **默认可合入 main** |
| 表面内 **不**把 B（渲染线程 latch）升为默认 | 把 Success 写成 ship / 解冻 PERF-CI |
| 接缝约束：拆出部件 **加轻微弹簧阻尼**（seam_ok=0） | 改 composition.h；合 main；回灌本机 csproj |
| 实验证据：`0fb0760d…` + `m3/results_follow/`（9 files） | 本 pack **授权新采数**；阻拦 `M3-XAML-K93` |

## 授权核验

| 项 | 值 |
|---|---|
| LOCKED | SendInput `285e39dc…aab3` @ 2026-10-30 17:40:39 AWST；复算 **MATCH** |
| approved | pidan 2026-11-01 16:26:53 Asia/Shanghai |
| 本结题采数 | **否**（纯文档；真机已由 workbuddy 交付） |
| tip / commit | `0fb0760d0897f378e82b0242a803d18afcbf9e4f` |
| dll | `733c5cb3a0397a1fdd560b67ba53adddf0dc5fc10eb10e5b4395ae99871f4aa4` BUILD_OK |

## 事实表

| 项 | 值 |
|---|---|
| TICKET / STATUS | `RIVE-FOLLOW-PASS` / **CLOSED** |
| METRICS | **METRICS_PASS**（成功≠采用） |
| 采用 | **有条件采用**（设计约束）；不合 main |
| sessions / verdict | 3 / RECORDED×3 |
| A/B/C/D p50_ms | s1 64.72/63.48/40.78/54.62；s2 65.84/63.18/40.70/54.52；s3 67.16/64.53/40.47/54.13 |
| c_vs_a / b_vs_a / seam_frames | 1.80/1.89/2.00；0.09/0.20/0.20；-1.93/-1.93/-1.95 |
| rules | split_default=1；latch_default=0；seam_ok=0 |
| budget_ok | 1/1/1；draw_failures=0 |
| results | `m3/results_follow/` **9** files |
| composition.h | 未触；PRE=POST |
| PERF-CI | HOLD |

## 产物清单

- `docs/m3_rive_follow_result.md`（权威结题）
- `docs/m3_rive_follow_result_brief.md`
- `docs/notion_m3_rive_follow_pack.md`（本包）
- `docs/notion_m3_rive_follow_next.md`
- brief 一行：`docs/m2_mainline_brief.md`
- 只读结果：`m3/results_follow/`（勿改）
- 只读闸门/批准/派工：LOCKED / approved / `workbuddy_m3_rive_follow_dispatch.md`
- 产物前缀 only：`m3_rive_follow_*` / `m3follow_*` / `notion_m3_rive_follow_*`

## 禁令复述

- 不改 composition.h；不合 main；Success≠adopt；PERF-CI HOLD
- 不翻 CLOSED 旧票；不阻拦 `M3-XAML-K93`；本 pack/next **不授权新采数**
- PropertySet bak 非现行；不改 LOCKED body/hash

## 挂账 / 残差

| 项 | 处置 |
|---|---|
| seam_ok=0 | 设计约束：弹簧阻尼 |
| latch_default=0 | 记录；B 不升默认 |
| 绝对延迟记录 | 不升硬 FAIL |
| 下一张 | 见 next → **M3-XAML-K93 已派/在跑 — 待命并行** |

## 下一票决断

详见 `docs/notion_m3_rive_follow_next.md`。  
**主推：待命/并行 `M3-XAML-K93`（已 APPROVED；K93 真机进行中）— 勿重复授权采数；勿阻拦。** MATRIX 仍排 FOLLOW 路径结题后再议。

— notion ai（M3-RIVE-FOLLOW 结题；CLOSED PASS；METRICS_PASS；有条件采用设计约束；不合 main）
