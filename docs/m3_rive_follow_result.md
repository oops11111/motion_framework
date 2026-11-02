# M3-RIVE-FOLLOW 结题结果

STATUS: CLOSED
TICKET: RIVE-FOLLOW-PASS
METRICS: METRICS_PASS
ADOPT: 有条件采用（WP2 设计约束结论 / 实验分支证据；非整 tip 默认可合入主线；**不合 main**）
WRITTEN_AT: 2026-11-02 08:11:19 Australia/Perth（AWST）
GATE: docs/m3_rive_follow_gate_LOCKED.md
GATE_BODY_SHA256: 285e39dc9c1100f92ddddc5560a3631b94129a9b56c561c9803c8cbee57eaab3
LOCKED_AT: 2026-10-30 17:40:39 AWST（Australia/Perth）
APPROVED: docs/m3_rive_follow_approved.md — pidan@2026-11-01 16:26:53 Asia/Shanghai
BRANCH: claude/m3-follow
COMMIT: 0fb0760d0897f378e82b0242a803d18afcbf9e4f
TIP: 0fb0760d0897f378e82b0242a803d18afcbf9e4f（叠在远端 9715164b0cd7f1ca4f1effe0974a31dcca031bc9 上；IJI 直推）
HOST: DESKTOP-IJI1KJG；75Hz；scale=1.000
BUILD: m0rive BUILD_OK；m0rive.dll sha256=`733c5cb3a0397a1fdd560b67ba53adddf0dc5fc10eb10e5b4395ae99871f4aa4`；demo Debug OK（follow patch 手工路径剥离后打上）
composition.h: PRE=POST CRLF=`33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97`（run.log）；LF 惯例 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`；edited=False
RESULTS: m3/results_follow/（9 files：8×m3follow_* + build_m0.log）
PERF-CI: HOLD
Success≠adopt: 是

## 授权核验摘要

| 项 | 结果 |
|---|---|
| approved 文件 | 存在：`docs/m3_rive_follow_approved.md` |
| APPROVED_BY / AT | pidan / **2026-11-01 16:26:53 Asia/Shanghai** |
| 采数 | **是**（派工 `docs/workbuddy_m3_rive_follow_dispatch.md`；SendInput P4 仅移光标） |
| 落地 / 合入 main | APPROVED 写死 **否**；结题维持不合 main |
| 指针喂入 | 现行 LOCK **SendInput**（非 PropertySet bak） |
| GATE_BODY_SHA256 | 期望 `285e39dc9c1100f92ddddc5560a3631b94129a9b56c561c9803c8cbee57eaab3` |
| 结题复算 hash | **MATCH**（排除 GATE_BODY_SHA256 行后全文 SHA-256）；**未改** LOCKED body/hash |
| PropertySet bak | `docs/m3_rive_follow_gate_LOCKED.md.bak_propertyset_0f85a952` — **勿当现行** |
| composition.h | **不改**；run.log PRE=POST CRLF `33d826c7…6e97` |
| 本结题任务 | **纯文档结题 ≠ 采数**；未派其它 agent；未合 main |

## G0 / G2 / G3 硬门槛对照

| 门槛 | 闸门口径 | 核实 | 结果 |
|---|---|---|---|
| G0 composition.h | 不改 | PRE=POST `33d826c7…6e97`；edited=False | **守** |
| G0 合 main / Success≠adopt | 成功≠采用≠默认可合入 | 结题不合 main；有条件采用仅设计约束 | **守** |
| G0 PERF-CI | HOLD | 仍 HOLD | **守** |
| G0 / G3 会话数 | ≥3（P3） | sessions=3 | **PASS** |
| 路径覆盖 | A/B/C/D 全测（P2） | 四路径均有 p50 / steps_hit / budget | **PASS** |
| 检出步数 | 正向+反向；不足一半→INVALID | 全路径 `steps_hit=40/40`、`fwd_bwd=20/20` | **PASS**（非 INVALID） |
| 预算 | CPU/GPU P95 ≤0.35·T | 各路径 frac_T≪0.35；`budget_ok=1/1/1` | **PASS** |
| 稳定性（字段） | draw_failures 等 | 全路径 `draw_failures=0` | **PASS** |
| bind_ok | **本闸门未定义硬字段** | summary 无 `bind_ok`；不臆造 INVALID | **N/A**（不升 INVALID） |
| 延迟绝对值 | **记录型**对比+写入约束；**不**升硬对齐 M0 12–45 | A/B/C/D p50≈40–67ms；已记录 | **记录**（不挡 PASS） |
| 规则 C vs A | ≥1 帧 → split_default | c_vs_a_frames=1.80/1.89/2.00；`split_default=1/1/1` | **成立** |
| 规则 B vs A | ≥0.5 帧 → latch_default | b_vs_a_frames=0.09/0.20/0.20；`latch_default=0/0/0` | **不成立（正确记录）** |
| 规则接缝 | ≤1 帧 → seam_ok；否则写弹簧阻尼约束 | seam_frames=-1.93/-1.93/-1.95；`seam_ok=0/0/0` | **接缝不可接受→约束成立** |
| INVALID 总览 | 缺产物 / hash 不符 / 未授权 / 改 composition.h / 检出不足半 | 9 产物齐；hash MATCH；已 APPROVED；composition 未改；检出满 | **非 INVALID** |

## workbuddy ONELINE（核实后）

```
M3-RIVE-FOLLOW | commit=0fb0760d0897f378e82b0242a803d18afcbf9e4f @ claude/m3-follow | tip_parent=9715164b0cd7f1ca4f1effe0974a31dcca031bc9 | build=m0rive BUILD_OK dll_sha256=733c5cb3…4aa4 + demo Debug OK | sessions=3 | verdict=RECORDED/RECORDED/RECORDED | A/B/C/D p50_ms=s1 64.72/63.48/40.78/54.62；s2 65.84/63.18/40.70/54.52；s3 67.16/64.53/40.47/54.13 | c_vs_a_frames=1.80/1.89/2.00 | b_vs_a_frames=0.09/0.20/0.20 | seam_frames=-1.93/-1.93/-1.95 | budget_ok=1/1/1 | split_default=1/1/1 latch_default=0/0/0 seam_ok=0/0/0 | composition.h PRE=POST 33d826c7…6e97 | gate=285e39dc…aab3 MATCH | approved=pidan@2026-11-01 16:26:53 Asia/Shanghai | ticket=RIVE-FOLLOW-PASS | METRICS_PASS | 有条件采用 WP2 设计约束；不合 main | Success≠adopt | PERF-CI HOLD
```

## 会话事实表（读 `m3follow_session_matrix.txt` + s{1,2,3}_summary）

| 会话 | pid | verdict | A p50 | B p50 | C p50 | D p50 | c_vs_a_fr | b_vs_a_fr | seam_fr | split | latch | seam_ok | budget |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| s1 | 18324 | RECORDED | 64.72 | 63.48 | 40.78 | 54.62 | 1.80 | 0.09 | -1.93 | 1 | 0 | 0 | 1 |
| s2 | 4868 | RECORDED | 65.84 | 63.18 | 40.70 | 54.52 | 1.89 | 0.20 | -1.93 | 1 | 0 | 0 | 1 |
| s3 | 13872 | RECORDED | 67.16 | 64.53 | 40.47 | 54.13 | 2.00 | 0.20 | -1.95 | 1 | 0 | 0 | 1 |

- 跑机：`DESKTOP-IJI1KJG`；refresh≈75.03Hz；WGC/start HR 全 0。
- 拖动样本：各路径 ≥404（≥200 门槛远超）。
- 产物：无独立 `.json`；矩阵为 CSV 头行 + 3 数据行（`m3follow_session_matrix.txt`）。

## WP2 设计约束裁决（写入；Success≠代码采用）

| 规则 | 结论 | 写入约束 |
|---|---|---|
| C（拆出合成器 visual / expr）相对 A | **定为跟随部件默认做法**（≥1 帧） | 跟随部件默认拆成合成器表达式驱动的 visual |
| B（渲染线程最后时刻读光标）相对 A | **不**定为表面内默认（≪0.5 帧） | 表面内仍以 A 类 UI 转交为基线；B 不升默认 |
| 拖动接缝（C_lag−A_lag） | **不可接受**（≈−1.93…−1.95 帧，\|seam\|>1） | 拆出部件须加轻微弹簧阻尼掩盖与 Rive 表面内容的错位（disputes §3） |
| 延迟绝对值 | 记录对比 | C≈40–41ms；D≈54ms；A≈65–67ms；B≈63–65ms（75Hz）；**不**硬对齐 M0 12–45 |

## 采用决断（写死）

| 项 | 决断 | 理由 |
|---|---|---|
| 票标签 | **RIVE-FOLLOW-PASS** | 3×RECORDED；预算/检出/产物齐；规则可裁决；METRICS_PASS |
| METRICS | **METRICS_PASS** | 非 INVALID；延迟不单列硬 FAIL |
| 是否采用 | **有条件采用** | 采用 WP2 **设计约束结论**与实验分支 evidence；**不合 main** |
| 采用范围 | 设计约束 + `claude/m3-follow` @ `0fb0760d…` + `m3/results_follow/` | 非整 tip 无评审合入产品主线 |
| 不采用 | tip 默认可合入 main；解冻 PERF-CI；改 composition.h；把 Success 当 ship | Success≠adopt |
| composition.h | **未改** | 硬禁 |
| PERF-CI | **HOLD** | 硬禁 |

## 挂账 / 残差

| 项 | 处置 |
|---|---|
| 绝对延迟 > M0 expr 参考带 | **记录**；闸门记录型；不升 FAIL |
| seam_ok=0 | **设计约束挂账**：弹簧阻尼；非 METRICS FAIL |
| latch_default=0 | **记录**：B 增益不足；不升表面内默认 |
| follow patch 手工路径 | workbuddy 已记；本机 csproj 不进仓 |
| 无独立 json | 矩阵 CSV 文本已够；不补采 |
| M3-XAML-K93 | **已 APPROVED 且 K93 在跑** — 见 next；本结题不阻拦、不重复授权 |
| CLOSED 旧票 | **不翻** |
| PropertySet bak | 非现行 |

## 标签边界

- 本票标签仅 `RIVE-FOLLOW-*`；**METRICS_PASS ≠ 默认可合入 main ≠ ship**
- 不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；不改 composition.h
- 不翻 CLOSED；不阻拦 K93；本结题 **≠ 采数令**
- 产物前缀：`m3_rive_follow_*` / `m3follow_*` / `notion_m3_rive_follow_*`

## 写结

- written_at: 2026-11-02 08:11:19 Australia/Perth（AWST）
- notion ai（动效框架执行助手）结题；**本任务未采数、未派其它 agent**；依据 LOCK 复算 MATCH + APPROVED + GitHub `claude/m3-follow` @ `0fb0760d…` 的 `m3/results_follow/`（9 files）+ workbuddy ONELINE 核对
