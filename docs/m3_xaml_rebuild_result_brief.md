# M3-XAML-REBUILD 结题简报（协调侧）

STATUS: DIAGNOSIS_PASS（有条件；R4=KNOWN_LIMIT；成功 ≠ 采用；采用=有条件验证结论；**不合 main**）
TICKET: XAML-REBUILD-PASS
CHECKED_AT: 2026-11-01 17:55:00 Asia/Shanghai
BRANCH: claude/m3-xaml-rebuild
COMMIT: d00ffd0f5be2f11f7b5b4e30f287c7d0b16b9e9f
TIP_READ: 24ccce6c3e8d618c704c27c8e675ae039c26599c
TIP_AFTER_PUSH: 同
GATE: `docs/m3_xaml_rebuild_gate_LOCKED.md`；GATE_BODY_SHA256=`3ea74c371ae983cebe480e1b4dea99e61aec92cfbe9646fb87b91b7a1d456fdc`（**MATCH**）
APPROVED: pidan @ 2026-11-01 16:26:53 Asia/Shanghai（`docs/m3_xaml_rebuild_approved.md`）
HOSTS: A/B=IJI1KJG；C=K93N3P8
composition.h: PRE=POST CRLF=33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97 LF=a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418 edited=False
ADOPT: 有条件采用验证结论（KNOWN_LIMIT／重启应用；非宿主重建窗口充分方案）；不合 main
PERF-CI: HOLD
RESULTS: m3/results_xaml_rebuild/ a71+b25+c25=121

## ONELINE
M3-XAML-REBUILD | tip_read=24ccce6c…599c | commit=d00ffd0f…9e9f @ claude/m3-xaml-rebuild | A unpackaged×3 RECORDED | B packaged×1 RECORDED packaged=1 | C packaged×1 RECORDED | TEXT A/B: W_pre=LOST W_post=LOST（进程级）| C: main=OK；W_*=INCONCLUSIVE | R4=KNOWN_LIMIT | ticket=XAML-REBUILD-PASS | gate MATCH 3ea74c37…6fdc | composition 未改 | 有条件采用；不合main | Success≠adopt | PERF-CI HOLD

## TEXT 摘要
| 组 | W_pre | W_post | 备注 |
|---|---|---|---|
| A IJI unpackaged×3 | TEXT-LOST（base=OK） | TEXT-LOST | 主数据；进程级 |
| B IJI packaged×1 | TEXT-LOST（base=OK） | TEXT-LOST | 打包同向 |
| C K93 packaged×1 | INCONCLUSIVE | INCONCLUSIVE | 主窗 TEXT-OK；DPI 截窗挂账 |

## R4
W_post 不能画字（A/B）→ **KNOWN_LIMIT**（建议重启应用）；**宿主重建窗口不充分**；Q3 报告交 pidan。

## 下一票（一句话）
主推 **M3-RIVE-FOLLOW**（已 APPROVED SendInput LOCK `285e39dc…aab3`；**已派 workbuddy**）— **待命/并行，勿阻拦**；**本简报不授权新采数**。

## 禁令核对
不改 composition.h；不合 main；Success≠adopt；PERF-CI HOLD；不翻 CLOSED；不阻拦 workbuddy；本结题不采数。
