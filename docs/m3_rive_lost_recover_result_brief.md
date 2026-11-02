# M3-RIVE-LOST-RECOVER 结题简报（协调侧）

STATUS: METRICS_PASS（有条件；成功 ≠ 采用；采用=有条件验证结论；**不合 main**）
TICKET: RIVE-LOST-RECOVER-PASS
CHECKED_AT: 2026-10-30 12:10:00 Asia/Shanghai
BRANCH: claude/m3-rive-lost
COMMIT: 68ce59d3cea4c9e8ec155015207113ce718a7ab6
TIP_READ: c954696470849f8f466d3c5d7f7dd57159cd8cbd
TIP_AFTER_PUSH: 同 commit
GATE: `docs/m3_rive_lost_recover_gate_DRAFT.md`（**DRAFT 未 LOCK**；未改）；本地**无** approved
AUTH_NOTE: **按用户指令采数 / 闸门未 LOCK**（协调转述：pidan 跳过开放问题拍板，直接下令 4 步采数；协调侧视该指令为本次采数授权；结题不虚构批准文件）
HOST: 惯例 DESKTOP-IJI1KJG；selftest=pnputil exit0 ~3.3s；elevated=True
DLL_SHA256: d3aef01a0e410c30a147588cbc3dab6be18f0fdf50ba2674d1223e9ca1f0b840
composition.h: PRE=POST CRLF=33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97 LF=a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418 edited=False
ADOPT: 有条件采用验证结论；不合 main
PERF-CI: HOLD
vs r1/r2: r1=INCONCLUSIVE；r2=FAIL（abort）→ 本票 recover_ok / abort=none / 3×RECORDED — **不翻盘 r1/r2**

## ONELINE
M3-RIVE-LOST-RECOVER | tip_read=c954696470849f8f466d3c5d7f7dd57159cd8cbd | commit=68ce59d3cea4c9e8ec155015207113ce718a7ab6 @ claude/m3-rive-lost | tip_after_push=同 commit | BUILD_OK | dll_sha256=d3aef01a0e410c30a147588cbc3dab6be18f0fdf50ba2674d1223e9ca1f0b840 | SIM_SMOKE=PASS（SIM_RECORDED recover_ok=1 recover_max_ms=756.94 probe=M3-LOST v3）| selftest=OK（pnputil exit0 ~3.3s）| elevated=True | s1/s2/s3=RECORDED/RECORDED/RECORDED | lost_detected=2/2/2 | recovered=2/2/2 | recover_ok=1/1/1 | recover_max_ms=2319.99/2206.54/2160.70 | rive_abort=none | budget_ok/stability_ok=1/1 | frames_missed=331/347,341/342,340/331 | mem_delta_mb=-23/-25/-25 | injections_ok=1/1/1 | composition PRE=POST CRLF=33d826c7… LF=a55d4952…e418 edited=False | results=m3/results_lost_recover/（31 files）| gate=DRAFT未LOCK | 按用户指令采数 | ticket=RIVE-LOST-RECOVER-PASS | METRICS_PASS有条件 | 有条件采用；不合main | Success≠adopt | PERF-CI HOLD

## 闸门对照（DRAFT G2）
| 判据 | 结果 |
|---|---|
| 会话 ≥3 | 3×RECORDED → 满足 |
| ① 保活 / N_inj=2 | lost_detected/recovered=2/2/2；rive_abort=none → **有条件 PASS**；injections_ok=1/1/1 字段挂账 |
| ② 恢复成功 | recover_ok=1/1/1；recovered=2/2/2；abort=none → **PASS** |
| ③ 稳态帧 ≥300 | 字段=`frames_missed`（对值≥300）；本地无 results → **挂账；勿盲升** |
| ④ 预算/稳定性 | budget_ok/stability_ok=1/1 → **PASS** |
| ⑤ abort 字段 | none → N/A |
| ⑥ 恢复耗时 | ~2.2s（只记录；超参考 500ms 不挡） |
| ⑦ 内存 Δ | −23/−25/−25（只记录） |
| SIM_SMOKE | PASS（**仅 SIM**） |

**ticket：RIVE-LOST-RECOVER-PASS（METRICS_PASS 有条件）** — 相对 r2 恢复已成立；③/injections 挂账。

## 下一票（一句话）
主推 **M3-RIVE-FOLLOW DRAFT（文档）**；建议补 RECOVER LOCK 归档（≠采数授权）；FOLLOW/MATRIX 不再因恢复未成立硬挡，但仍须各自 LOCK+APPROVED；**本简报不授权采数**。

## 禁令核对
不改 composition.h；不重开 S1/R-3/R-2；成功≠采用；不合 main；PERF-CI HOLD；不翻盘 r1/r2；不改 PROBE；不把 DRAFT 写成 LOCKED；不把 SIM_SMOKE 当真机唯一 PASS；不伪造 approved。

## 更正附录（pidan 裁决 XAML-LOST N5，2026-10-30；正文不改，PASS 结论不变）
- **闸门已 LOCK 且 APPROVED，不需要“补 LOCK 归档”**：`docs/m3_rive_lost_recover_gate_LOCKED.md`，GATE_BODY_SHA256=`16202071fd08d091d0a0d825eda2297297ae19227d72a6cf981d4ed8b25484ce`（LOCKED_AT 2026-10-30 11:31:47 AWST；在 `68ce59d` 上复算 MATCH）；`docs/m3_rive_lost_recover_approved.md`（pidan @ 2026-10-30 11:47:20 AWST，早于 11:52:46 开始采数）。正文“DRAFT 未 LOCK、无 approved、按用户指令采数”是协调侧本地副本过旧所致。
- **补充发现（正文遗漏）**：三个会话、六次驱动重启后，应用自己的 XAML 界面（标题、开关、按钮、提示文字、标题栏图标和标题）全部消失且不再重画，只有 Rive 视觉在画（标题区亮像素 1518 → 0，提示区 506 → 0）。不在本票判据内，但会影响试点接入；诊断票 `docs/m3_xaml_lost_gate_*.md`。详见 `docs/m3_rive_lost_recover_result_claude_review.md`。
- r1、r2 的更正见各自结题末尾附录（r1 = INVALID；r2 对应 r2 LOCKED `dc0770c1…9d10`）。
