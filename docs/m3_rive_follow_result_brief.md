# M3-RIVE-FOLLOW 结题简报（协调侧）

STATUS: METRICS_PASS（成功 ≠ 采用；采用=有条件 WP2 设计约束；**不合 main**）
TICKET: RIVE-FOLLOW-PASS
CHECKED_AT: 2026-11-02 08:11:19 Australia/Perth（AWST）
BRANCH: claude/m3-follow
COMMIT: 0fb0760d0897f378e82b0242a803d18afcbf9e4f
TIP: 同 commit（parent 9715164b0cd7f1ca4f1effe0974a31dcca031bc9；IJI 直推）
GATE: `docs/m3_rive_follow_gate_LOCKED.md`；GATE_BODY_SHA256=`285e39dc9c1100f92ddddc5560a3631b94129a9b56c561c9803c8cbee57eaab3`（结题复算 **MATCH**；SendInput 现行；PropertySet bak 勿用）
APPROVED: `docs/m3_rive_follow_approved.md` — pidan@2026-11-01 16:26:53 Asia/Shanghai
HOST: DESKTOP-IJI1KJG；75Hz
BUILD: m0rive BUILD_OK；dll_sha256=`733c5cb3a0397a1fdd560b67ba53adddf0dc5fc10eb10e5b4395ae99871f4aa4`；demo Debug OK
composition.h: PRE=POST CRLF=`33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97`；edited=False
ADOPT: 有条件采用 WP2 设计约束；不合 main
PERF-CI: HOLD

## ONELINE
M3-RIVE-FOLLOW | commit=0fb0760d0897f378e82b0242a803d18afcbf9e4f @ claude/m3-follow | build=m0rive BUILD_OK 733c5cb3… + demo Debug OK | sessions=3 | verdict=RECORDED/RECORDED/RECORDED | A/B/C/D p50_ms=s1 64.72/63.48/40.78/54.62；s2 65.84/63.18/40.70/54.52；s3 67.16/64.53/40.47/54.13 | c_vs_a_frames=1.80/1.89/2.00 | b_vs_a_frames=0.09/0.20/0.20 | seam_frames=-1.93/-1.93/-1.95 | budget_ok=1/1/1 | split_default=1 latch_default=0 seam_ok=0 | composition PRE=POST | gate=285e39dc…aab3 MATCH | ticket=RIVE-FOLLOW-PASS | METRICS_PASS | 有条件采用设计约束；不合main | Success≠adopt | PERF-CI HOLD

## 闸门对照（SendInput LOCK G2）
| 判据 | 结果 |
|---|---|
| 会话 ≥3 | 3×RECORDED → **PASS** |
| 检出 / INVALID_FEW_STEPS | 全路径 40/40（20/20）→ **PASS** |
| 预算 | budget_ok=1/1/1；draw_failures=0 → **PASS** |
| 延迟绝对值 | 记录型；不硬对齐 M0 12–45 → **不挡** |
| C≥A+1帧 | 1.80/1.89/2.00 → split_default=1 → **C 默认** |
| B≥A+0.5帧 | 0.09/0.20/0.20 → latch_default=0 → **B 不升默认** |
| 接缝 ≤1帧 | \|seam\|≈1.93–1.95 → seam_ok=0 → **须弹簧阻尼约束** |
| bind_ok | 闸门无此硬字段 → **N/A** |
| composition.h | 未改 → **守** |

**ticket：RIVE-FOLLOW-PASS（METRICS_PASS）**

## 下一票（一句话）
主推 **待命/并行已 APPROVED 的 `M3-XAML-K93`（K93 真机进行中）**；勿重复授权采数、勿阻拦；MATRIX 仍排 FOLLOW 结题后议；**本简报不授权采数**。

## 禁令核对
不改 composition.h；不合 main；Success≠adopt；PERF-CI HOLD；不翻 CLOSED；不阻拦 K93；PropertySet bak 非现行；纯文档结题≠采数。
