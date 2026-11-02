# M3-XAML-LOST 结题简报（协调侧）

STATUS: METRICS_PASS（有条件；成功 ≠ 采用；采用=有条件验证结论；**不合 main**）
TICKET: XAML-LOST-PASS
CHECKED_AT: 2026-10-30 17:05:00 Asia/Shanghai
BRANCH: claude/m3-xaml-lost（tip 曾误写 claude/m3-rive-lost；实际名=claude/m3-xaml-lost）
COMMIT: efce2195904ad9acf77fafb2ea5c0d8e4d81f556
TIP_READ: bb61eae8ca52ee4be6d0e9d36f0bebd62c45e012
TIP_AFTER_PUSH: 同 commit efce2195…
GATE: **本地无 XAML-LOST LOCKED**（无 DRAFT/LOCKED/APPROVED 文件可引用）
AUTH_NOTE: **按用户指令采数 / 闸门未 LOCK**（pidan 2026-10-30 16:43 四步直接下令；无 APPROVED 文件；站立规则：闸门未 LOCK≠采数时按用户指令可结题；结题不虚构批准/LOCKED）
HOST: DESKTOP-IJI1KJG；push=IJI1KJG direct；DRYRUN=PASS；selftest=OK ~1.6s
DLL_SHA256: d3aef01a0e410c30a147588cbc3dab6be18f0fdf50ba2674d1223e9ca1f0b840（复用 RECOVER）
composition.h: PRE=POST CRLF=33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97 LF=a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418 edited=False
ADOPT: 有条件采用验证结论；不合 main
PERF-CI: HOLD
vs RIVE-LOST 族: r2=FAIL（abort）；RECOVER=PASS 有条件 → 本票 s4 with_rive lost_detected=1 recovered=1 abort=none — **不翻盘**前序

## ONELINE
M3-XAML-LOST | tip_read=bb61eae8ca52ee4be6d0e9d36f0bebd62c45e012 | commit=efce2195904ad9acf77fafb2ea5c0d8e4d81f556 @ claude/m3-xaml-lost | tip_after_push=efce2195… | BUILD_OK（dll 复用 RECOVER d3aef01a…b840）| DRYRUN=PASS | selftest=OK（~1.6s）| s1/s2/s3=RECORDED plain inject_ok=1 | s4=RECORDED with_rive=1 inject_ok=1 lost_detected=1 recovered=1 recover_ms=2004.58 rive_abort=none | composition PRE=POST CRLF=33d826c7… LF=a55d4952…e418 | results=m3/results_xaml_lost/（72 files）| push=IJI1KJG direct | gate=本地无XAML-LOST LOCKED | 按用户指令采数 | ticket=XAML-LOST-PASS | METRICS_PASS有条件 | 有条件采用；不合main | Success≠adopt | PERF-CI HOLD

## 证据对照
| 项 | 结果 |
|---|---|
| s1–s3 plain | RECORDED；inject_ok=1 → **PASS** |
| s4 with_rive | RECORDED；lost_detected=1；recovered=1；rive_abort=none → **探测+恢复成立** |
| with_rive 会话数 | n=1 → **有条件挂账** |
| recover_ms | 2004.58（只记录；不挡） |
| BUILD / DRYRUN / selftest | OK / PASS / OK |
| composition.h | 未触；LF a55d4952…e418 |

**ticket：XAML-LOST-PASS（METRICS_PASS 有条件）** — with_rive 探测+恢复成立；闸门缺失与 n=1 挂账。

## 下一票（一句话）
主推 **M3-RIVE-FOLLOW**（已 LOCKED hash `0f85a95277dd279e51085a42cd491b55db332ba49077055d045f794fc6b30a13`；**APPROVED 空**；等 pidan）；**本简报不授权采数**。

## 禁令核对
不改 composition.h；不重开 S1/R-3/R-2；成功≠采用；不合 main；PERF-CI HOLD；不翻盘 r1/r2/RECOVER；不改 PROBE/FOLLOW LOCKED；不虚构 XAML-LOST LOCKED/APPROVED；不把 DRYRUN 当真机唯一 PASS。

## 更正附录（pidan 裁决 Q1，2026-10-30；正文不改）
- **标签按 LOCKED N4 更正为 `XAML-LOST`（平台问题，与 Rive 无关）**，不是 `XAML-LOST-PASS`。本票是诊断票，只取 `XAML-RECOVERS` / `XAML-LOST-REMEDY-<操作>` / `XAML-LOST`。正文按 s4 中 Rive 的恢复判定，但 Rive 恢复已在 RECOVER 票验证；本票要回答的是 XAML 界面：普通窗口 3/3、带 Rive 1/1 在驱动重启后丢失全部文字、圆角元素、开关、按钮和标题栏内容，等待、改大小、最小化还原、重设内容都不恢复，而 XAML 仍在出帧。
- **闸门已 LOCK 且 APPROVED**：`docs/m3_xaml_lost_gate_LOCKED.md`，GATE_BODY_SHA256=`69a52e2de66153348bff5197dbb037dd2650c26055b834173abde6ae6a1e1f55`（LOCKED_AT 2026-10-30 13:36:05 AWST；在 `efce219` 上复算 MATCH）；`docs/m3_xaml_lost_approved.md`（pidan @ 2026-10-30 13:42:56 AWST，早于 16:53 开始采数）。正文“本地无闸门、无 approved、按用户指令采数”是协调侧本地副本过旧所致。
- 详见 `docs/m3_xaml_lost_result_claude_review.md`；后续诊断 `docs/m3_xaml_rebuild_gate_DRAFT.md`。
