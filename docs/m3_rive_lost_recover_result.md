# M3-RIVE-LOST-RECOVER 结题结果

STATUS: CLOSED
TICKET: RIVE-LOST-RECOVER-PASS
METRICS: METRICS_PASS（有条件；见「③ frames_missed」与「injections_ok」挂账）
ADOPT: 有条件采用（验证结论 / 实验分支证据；**不合 main**；非整 tip 默认可合入主线）
CSPROJ_BACKPORT: 否（不回灌主线）
WRITTEN_AT: 2026-10-30 12:10:00 Asia/Shanghai
GATE: `docs/m3_rive_lost_recover_gate_DRAFT.md`（STATUS=**DRAFT**；**未 LOCK**；本结题**未改** DRAFT body；**无** GATE_BODY_SHA256）
GATE_BODY_SHA256: （无；DRAFT 写死 LOCK 时再算；本地**无** `docs/m3_rive_lost_recover_gate_LOCKED.md`）
LOCKED_AT: （无）
APPROVED: 本地**无** `docs/m3_rive_lost_recover_approved.md`（本结题**不虚构**批准文件；采数口径见下「授权 / 闸门」）
BRANCH: claude/m3-rive-lost
COMMIT: 68ce59d3cea4c9e8ec155015207113ce718a7ab6
TIP_READ: c954696470849f8f466d3c5d7f7dd57159cd8cbd
TIP_AFTER_PUSH: 同 commit `68ce59d3cea4c9e8ec155015207113ce718a7ab6`
HOST: （ONELINE 未单列；与 r1/r2/DPI 同机惯例 DESKTOP-IJI1KJG；selftest=`pnputil` exit0 ~3.3s；elevated=True）
composition.h: PRE=POST；CRLF=`33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97`；LF=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`；edited=False（本地 `code/composition.h` 复算 MATCH）
RESULTS_DIR: `m3/results_lost_recover/`（ONELINE：31 files incl s1–s3 `_stderr.txt`；**本根本地无**该目录可读矩阵 → 以 ONELINE 为准）
DLL_SHA256: `d3aef01a0e410c30a147588cbc3dab6be18f0fdf50ba2674d1223e9ca1f0b840`

## 授权 / 闸门核验摘要（写死）

| 项 | 结果 |
|---|---|
| RECOVER 闸门 | **仍为 DRAFT**：`docs/m3_rive_lost_recover_gate_DRAFT.md`；**未 LOCK** |
| GATE_BODY_SHA256 / LOCKED | **无** |
| approved 文件 | **本地不存在** `docs/m3_rive_lost_recover_approved.md`（及同类） |
| 采数授权口径 | **按用户指令采数 / 闸门未 LOCK**。据**协调转述**：pidan 跳过开放问题拍板，直接下令 4 步采数；协调侧视该指令为本次采数授权。本结题据协调转述 + **已发生结果落盘**归档；**不**把代理声称写成「结题助手亲自核实的用户批准文件」——本地无 approved 路径可引用 |
| 本结题任务 | **不采数、不派 workbuddy、不重跑**；仅据 ONELINE + 本地文档结题 |
| composition.h | **未改** PRE=POST；本地 LF/CRLF 与 ONELINE 全 hash **一致** |
| PERF-CI | **仍 HOLD** |
| r1 / r2 CLOSED | **不改写**（r1=`RIVE-LOST-INCONCLUSIVE`；r2=`RIVE-LOST-FAIL`） |
| PROBE LOCKED | **不改** body/hash `7afbcd980d464abdeb835f059e05089edbf1d8f1112efb3e1d1330bd386e6b54` |

> **Success ≠ adopt**；成功 ≠ 默认可合入 main；本票 **不合 main**。

## workbuddy ONELINE（核实后；**字段勿改写**）

```
M3-RIVE-LOST-RECOVER | tip_read=c954696470849f8f466d3c5d7f7dd57159cd8cbd | commit=68ce59d3cea4c9e8ec155015207113ce718a7ab6 @ claude/m3-rive-lost | tip_after_push=同 commit | BUILD_OK | dll_sha256=d3aef01a0e410c30a147588cbc3dab6be18f0fdf50ba2674d1223e9ca1f0b840 | SIM_SMOKE=PASS（SIM_RECORDED recover_ok=1 recover_max_ms=756.94 probe=M3-LOST v3）| selftest=OK（pnputil exit0 ~3.3s）| elevated=True | s1/s2/s3=RECORDED/RECORDED/RECORDED | lost_detected=2/2/2 | recovered=2/2/2 | recover_ok=1/1/1 | recover_max_ms=2319.99/2206.54/2160.70 | rive_abort=none | budget_ok/stability_ok=1/1 | frames_missed=331/347,341/342,340/331 | mem_delta_mb=-23/-25/-25 | injections_ok=1/1/1 | composition PRE=POST CRLF=33d826c7… LF=a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418 edited=False | results=m3/results_lost_recover/（31 files incl s1–s3 _stderr.txt）| gate=DRAFT未LOCK | 按用户指令采数 | ticket=RIVE-LOST-RECOVER-PASS | METRICS_PASS有条件 | 有条件采用验证结论；不合main | Success≠adopt | PERF-CI HOLD
```

## 与 r1 / r2 对照（写死）

| 项 | r1（CLOSED INCONCLUSIVE） | r2（CLOSED FAIL） | 本票 RECOVER（CLOSED PASS 有条件） |
|---|---|---|---|
| tip / commit | `82a53cce…02694d` | commit=`382683cf…1a04ec`；tip_read=`0aea9d17…012807` | tip_read=`c9546964…8cbd`；commit=`68ce59d3…7ab6` |
| sessions / 结局 | 1/3；CRASHED_NO_VERDICT | 3/3；FAIL_ABORT | **3×RECORDED** |
| lost_detected | N/A | 1/1/1 | **2/2/2** |
| recover_ok / recovered | N/A | N/A（无 recovery） | **recover_ok=1/1/1**；**recovered=2/2/2** |
| rive_abort | （未进入） | **1** | **none** |
| recover_max_ms | N/A | N/A | **2319.99 / 2206.54 / 2160.70**（只记录；超参考 ≤500ms） |
| injections_ok | N/A | 1/1/1（目标 2 只成 1；首注后死） | **1/1/1**（字段同形；本票 lost_detected=2 → 见挂账） |
| 票标签 | RIVE-LOST-INCONCLUSIVE | RIVE-LOST-FAIL | **RIVE-LOST-RECOVER-PASS** |

> **勿翻盘 r1 / r2**；PROBE LOCKED **不改**。探测能力继续以 r2 `lost_detected=1/1/1` 为前置实证；本票刀口（abort/重建/二次可达）相对 r2 **已越过**。  
> **勿把 SIM_SMOKE PASS 当成真机 PASS**（SIM recover_max_ms=756.94；真机 ~2.2s）。

## 票结局声明

- 真机：s1/s2/s3=**RECORDED**；**lost_detected=2/2/2**；**recovered=2/2/2**；**recover_ok=1/1/1**；**rive_abort=none**；**budget_ok/stability_ok=1/1** → 相对 r2 FAIL_ABORT，**恢复路径成立**。
- 票标签 **`RIVE-LOST-RECOVER-PASS`**；STATUS=**CLOSED**；协调对照 **METRICS_PASS（有条件）**。
- **有条件点（诚实）**：
  1. **闸门未 LOCK**；采数依「按用户指令 / 协调转述授权」；本地**无** approved 文件。
  2. **③ 稳态帧**：ONELINE 字段名为 **`frames_missed`**（非 DRAFT 模板 `frames_post`）；数值对 331/347、341/342、340/331 均 ≥300，且 `stability_ok=1`，**疑似**各恢复后稳态段帧数，但本根本地**无** `m3/results_lost_recover/` 可读澄清 → **禁止**盲把「missed」等同 PASS 帧；**挂账复核**。
  3. **① / injections_ok**：DRAFT 要求 N_inj=2 且二次注入可达。ONELINE `injections_ok=1/1/1` 未展开为「2/2」；但 `lost_detected=2/2/2` + `recovered=2/2/2` + `rive_abort=none` + RECORDED **强支持**二次注入已可达、每会话检出 2 次。结题**有条件视①齐**；挂账：results 复核 injections 计数字段语义（勿与 r2「1/2 做成」同形误读）。
- **恢复耗时**：~2.2s **超过**参考 ≤500ms → DRAFT ⑥ **只记录**，**不单凭此判 FAIL**。
- **内存 Δ**：−23/−25/−25 MB（减少）→ ⑦ 只记录；参考 ≤50MB 内。
- **明确声明**：**成功 ≠ 采用 ≠ 默认可合入 main ≠ 授权改 composition.h ≠ PERF-CI ≠ 解冻 Bv ≠ 翻盘 M2 ≠ 重开 S1/R-3/R-2 ≠ 翻盘 r1/r2 ≠ 改 PROBE LOCKED**。
- **是否采用 = 有条件采用**：采用「本票验证结论」与实验分支恢复路径证据（commit `68ce59d3…` / tip_read `c9546964…` + ONELINE 所述 `m3/results_lost_recover/`）；**不合 main**；整 tip **不**视为可直接合入主线；合入须另走 PR 评审。
- **composition.h 未改**；**PERF-CI 仍 HOLD**。

## METRICS 核实（对照 DRAFT G2；本地无 results 矩阵）

| 判据 | DRAFT 门槛 | ONELINE | 结果 |
|---|---|---|---|
| 可裁决会话 | ≥3 | s1/s2/s3=RECORDED | **满足** |
| ① 保活 / N_inj=2 / 二次可达 | 首注后存活；N_inj=2 | lost_detected=2/2/2；recovered=2/2/2；rive_abort=none；injections_ok=1/1/1（字段未展开 2/2） | **有条件 PASS**（见挂账） |
| ② recover_ok | 每次检出后重建成功 | recover_ok=1/1/1；recovered=2/2/2；rive_abort=none | **PASS** |
| ③ 恢复后稳态帧 | 每段 ≥300 | 字段名=`frames_missed`；对值均 ≥300；本地无 results 澄清 | **有条件 / 挂账**（勿盲升） |
| ④ 预算 / 稳定性 | PASS | budget_ok/stability_ok=1/1 | **PASS**（据 ONELINE） |
| ⑤ abort 根因 | 失败路径硬要求 | rive_abort=none → N/A | **N/A（成功路径）** |
| ⑥ 恢复耗时 | 只记录；参考 ≤500ms | 2319.99 / 2206.54 / 2160.70 | **记录**（超参考；不挡） |
| ⑦ 内存 Δ | 只记录；参考 ≤50MB | −23 / −25 / −25 | **记录** |
| BUILD / SIM / selftest / elevated | 前置 | BUILD_OK；SIM_SMOKE=PASS；selftest=OK；elevated=True | 前置 OK；**SIM 不升真机** |

**ticket 判据：METRICS_PASS（有条件）** → 标签 **`RIVE-LOST-RECOVER-PASS`**（①③有条件挂账；②④据 ONELINE PASS；⑥⑦只记录）。

## 采用决断（写死）

| 项 | 决断 | 理由 |
|---|---|---|
| 票标签 | **RIVE-LOST-RECOVER-PASS** | 相对 r2 abort 已越过；recover_ok / RECORDED / abort=none |
| METRICS | **METRICS_PASS（有条件）** | ③ frames_missed 语义 + injections_ok 字段形挂账 |
| 是否采用 | **有条件采用验证结论** | 成功≠采用；**不合 main**；须 PR |
| composition.h | **未改** PRE=POST 全 hash | 硬禁；edited=False |
| csproj 回灌主线 | **否** | 沿用 DPI/r1/r2 边界 |
| PERF-CI | **HOLD** | 硬禁 |
| 闸门 LOCK 补档 | **建议协调另办**（本结题不改 DRAFT→LOCKED） | 已采数事后归档 / 开放问题仍未拍板入 LOCK |
| 翻盘 r1/r2 / 改 PROBE | **否** | 硬禁 |

## 挂账 / 残差

| 项 | 处置 |
|---|---|
| 闸门仍 DRAFT / 无 approved 文件 | **写死**：按用户指令采数；协调转述授权；本地无 approved 路径 |
| `frames_missed` vs `frames_post` | **挂账**：对照 `m3/results_lost_recover/`（本根暂不可读）澄清是否稳态段帧；未澄清前不得盲升 ③ |
| `injections_ok=1/1/1` vs N_inj=2 | **有条件齐**（凭 lost_detected/recovered=2）；挂账复核计数语义 |
| recover_max_ms ≈2.2s | **记录**；超参考 500ms；不升 FAIL；可作 FOLLOW/性能观察项 |
| mem_delta 负值 | **记录**；不挡 |
| SIM_SMOKE PASS | **仅 SIM**；不升真机唯一依据 |
| Lab / Demo csproj | **不回灌主线** |
| M2 旁路 / VERIFY-FAIL | **不翻盘** |
| PERF-CI | **仍 HOLD** |
| r1/r2/PROBE | **不改** |
| FOLLOW / MATRIX | 见 next：恢复验证有条件成立后**可开 DRAFT**；**本结题不授权采数** |
| 下一张 | 见 `docs/notion_m3_rive_lost_recover_next.md` |

## 标签边界

- 本票标签仅 `RIVE-LOST-RECOVER-*`；**PASS ≠ 默认可合入 main ≠ ship ≠ 闸门已 LOCK**
- 不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；不改 composition.h
- 不改 r1/r2 CLOSED；不改 PROBE LOCKED；不把本 DRAFT 写成 LOCKED
- 不把 SIM_SMOKE 当真机唯一 PASS；不立刻 Δv″；不抬 thr；不开无关 M2 翻盘
- 产物前缀：`m3_rive_lost_recover_*` / `notion_m3_rive_lost_recover_*`；结果目录 `m3/results_lost_recover/`（勿覆写 r1/r2/probe/dpi）

## 写结

- written_at: 2026-10-30 12:10:00 Asia/Shanghai
- notion ai（动效框架执行助手）结题；**本任务未采数、未派 workbuddy、未改 composition.h、未改 r1/r2 CLOSED、未改 PROBE LOCKED、未改 RECOVER DRAFT body、未改 ONELINE 事实字段**；依据协调 ONELINE + RECOVER DRAFT + r1/r2/DPI 结题样式对照

## 更正附录（pidan 裁决 XAML-LOST N5，2026-10-30；正文不改，PASS 结论不变）
- **闸门已 LOCK 且 APPROVED，不需要“补 LOCK 归档”**：`docs/m3_rive_lost_recover_gate_LOCKED.md`，GATE_BODY_SHA256=`16202071fd08d091d0a0d825eda2297297ae19227d72a6cf981d4ed8b25484ce`（LOCKED_AT 2026-10-30 11:31:47 AWST；在 `68ce59d` 上复算 MATCH）；`docs/m3_rive_lost_recover_approved.md`（pidan @ 2026-10-30 11:47:20 AWST，早于 11:52:46 开始采数）。正文“DRAFT 未 LOCK、无 approved、按用户指令采数”是协调侧本地副本过旧所致。
- **补充发现（正文遗漏）**：三个会话、六次驱动重启后，应用自己的 XAML 界面（标题、开关、按钮、提示文字、标题栏图标和标题）全部消失且不再重画，只有 Rive 视觉在画（标题区亮像素 1518 → 0，提示区 506 → 0）。不在本票判据内，但会影响试点接入；诊断票 `docs/m3_xaml_lost_gate_*.md`。详见 `docs/m3_rive_lost_recover_result_claude_review.md`。
- r1、r2 的更正见各自结题末尾附录（r1 = INVALID；r2 对应 r2 LOCKED `dc0770c1…9d10`）。
