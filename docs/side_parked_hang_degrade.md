# 旁路停车场：挂账 / 降级（side/parked-hang-degrade）

> DECISION_AT=2026-10-24 23:35:47 AWST（Australia/Perth）  
> 用户决策：挂账和降级另开分支，**M2–M5 继续**。  
> 建议未来 git 分支名：`side/parked-hang-degrade`（仓库当前**不是** git；仅命名约定）。

## 硬约束（全文适用）

- 下列条目 **不阻塞** M2–M5 主线。
- **不重开 S1 / R-3**（保持 CLOSED / STOP / NO-ADOPT）。
- **不改 `composition.h`**（sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）——挂账工作亦然。
- **成功 ≠ 采用**（NO-ADOPT）；过门 / 改善 ≠ ship。

## 停放条目

- **R-2 C5(b) 第二台机器**（换刷新率补测 t156dbl n≥60、t060rev；未完成；不阻塞主线）
- **R-3 CLOSED 挂账**（Bv 降级兜底；P1 a1 分不开 / pooled purity=0.83；P3 = CONFIRM_F5_REFUTE_2X med_res=+2.70；关闭理由=闸门触发 A v2×3=2/3，非数据复审）——见 [r3_commit_staleness.md](./r3_commit_staleness.md)
- **T-A1a CLOSED FAIL 挂账**（DeepSeek 补洞四条件须同满；Notion：残差–τ0 相关须另开独立票，不复活本票）——见 [t_a1a_closed.md](./t_a1a_closed.md)
- **B_v 降级路径**：M1b 兜底实现，标注「不满足 R-2」；**非合格基线**；**不作发版 / ship 依据**；不升级、不与 #49 横比、不复活 S2

## 主线指针

主线 = M2 → M5。见 [m2_mainline_brief.md](./m2_mainline_brief.md)；分流说明见 [notion_brief.md](./notion_brief.md)「主线 / 旁路分流」。

## M2 余项挂账（不挡 M2-02 · 2026-10-25 00:35 AWST）

Notion 结题 H5 后挂账、不并行真机：
- Δv 绝对门
- L 偏高（B 40–45 ms vs A 20–29 ms）
- 多刷新率矩阵
- 平滑速度估计（可选）

H5 已清 seed 比病根；轨迹质量门另票。主线 = M2-02。

## Claude #51 对照（归档 · 2026-10-25 06:56 AWST）

Claude Free 例行交付 #51 FINAL → [claude_r3_51_review.md](./claude_r3_51_review.md)。**不进主线、不重开 R-3/S1、不派文中 #52。** Notion 兜底保留于 `/workspace/notion_r3_51_review_reply.md` 与 [notion_r3_51_pack.md](./notion_r3_51_pack.md)。详见 [r3_commit_staleness.md](./r3_commit_staleness.md) ## 结题。

## M2-Δv CLOSED 挂账（2026-10-26 08:59:30 AWST）

- **M2-Δv CLOSED INCONCLUSIVE**（gate `d2349ceb…09c6` 不改 hash；真机 3/3 全核心格 rule-SKIP）。
- **残差主票 = M2-Δv′**（可判几何升版；KEEP 绝对阈；见 `docs/notion_m2_delta_v_pack.md` + `docs/m2_delta_v_prime_gate_DRAFT.md`）。
- IVel0·t300dbl 探针崩 → harness 稳定旁路（不挡 Δv′）。
- 其余挂账不变：L 偏高；多刷新率→M3/C5b；VEL-SMOOTH 可选；TOOLING；PERF-CI HOLD。

## M2-Δv′ CLOSED FAIL 挂账（2026-10-26 10:50:13 Asia/Taipei）

- **M2-Δv′ CLOSED FAIL**（gate `06467b82…805c` 不改、**不重跑翻盘**；t060 可判后 WGC 0–2/20；ctrl=baseP/IVel0F）。
- **残差主票 = M2-WGC-SEED-ABS**（修 WGC→IVel；禁影子 IV；不改 composition.h；见 `docs/notion_m2_delta_v_prime_pack.md`）。
- M2-Δv″（绝对门复测）挂账：须 seed 后新闸门版本。
- VEL-SMOOTH 并入 seed 设计空间；L / REFRESH / TOOLING / PERF-CI HOLD 不变。

## M2-WGC-SEED-PATH-B′ CLOSED 挂账（2026-10-26 14:30:10 Asia/Taipei）

- **M2-WGC-SEED-PATH-B′ CLOSED SEED-PATHB-FAIL**（gate `a1161c77…3a43` 不改、**不重跑翻盘**；A=V2-clamp；H5 比过 ∧ t060 smoke 全灭）。
- **SEED-TRUTH-CONFLICT 挂账（G2-LOCK-2b）**：2F 比真值 vs 绝对 `v_pre` 冲突；**禁**再开 calib 废止票当主线；**禁**抬 thr；**不**预授权改 H5 分母。
- **A′（V2-clamp+P-VS α_v=0.30）**继续挂账次选（本票未同跑）；启用须 Notion 显式另票。
- **M2-Δv″**继续挂账：须 seed 路径比回归过 ∧ t060 fwd+rev smoke 格 PASS 后另开新闸门；**不立刻开**。
- 禁翻盘三闸门 `09f6414b…793d` / `831818e4…80de` / `06467b82…805c`；composition.h 未改；PERF-CI HOLD；L / REFRESH / TOOLING 不变。
- 见 `docs/m2_wgc_seed_path_b_prime_result.md`；主线下一票交 Notion 决断。

## M2-WGC-SEED-PATH-A′ CLOSED 挂账（2026-10-26 15:35:43 Australia/Perth）

- **M2-WGC-SEED-PATH-A′ CLOSED SEED-PATHA-PRIME-FAIL**（gate `f17871f1…2362` 不改、**不重跑翻盘**；A′=V2-clamp+P-VS α_v=0.30；s1 比出窗；s2/s3 比过∧smoke 全灭）。
- **SEED-TRUTH-CONFLICT 保持挂账**：预登记平滑臂已耗尽；**禁**抬 thr；**不**预授权改 H5 分母；**禁**再标定主线。
- **M2-Δv″**继续挂账（smoke 未齐；不立刻开）。
- 禁翻盘四闸门含本票与 PATH-B′/CALIB/ABS/Δv′；composition.h 未改；PERF-CI HOLD。
- 见 `docs/m2_wgc_seed_path_a_prime_result.md`；主线下一票交 Notion 决断（可能升 SEED-TRUTH-CONFLICT 主线诊断）。

## M2-SEED-TRUTH-CONFLICT CLOSED（2026-10-26 15:51:51 Australia/Perth）

- **CLOSED SEED-TRUTH-PASS**（gate `276b97f0…4f69` 不改；menu=**M1**；offline；未采数）。
- PASS ≠ 已改 H5；执行改分母 → 主线下一票 **M2-H5-DENOM-REVISE**（另 LOCK）。
- 五 FAIL + 本 TRUTH 不翻盘；Δv″ 继续挂账；PERF-CI HOLD。

## 2026-10-26 16:31:53 Australia/Perth — Claude M2-Δv English FINAL merge · 区域不可用挂账

- **票/任务**：M2-Δv English FINAL merge DeepSeek — **未完成**
- **原因**：非 Free 额度。打开 claude.ai 相关 chat → 302 → `claude.com/app-unavailable-in-region`（仅部分地区可用；无 reset 时间；无 Upgrade）
- **Egress**：Chrome 报 CN（郑州）；经 `127.0.0.1:8791` 仍 302
- **产物**：`/workspace/claude_m2_delta_v_reply.md`、`docs/claude_m2_delta_v_review.md` 仍为 **INTERIM only**；无 DeepSeek merge；无新 FINAL；**禁仅凭 INTERIM 宣称 LOCK**
- **Routine**「Claude Free 恢复 · M2-Δv DeepSeek 合并终审」已删（本跑后）
- **处置**：主线不挡（当前 M2-H5-DENOM-VERIFY）；待非屏蔽 egress 再派同 chat + DeepSeek ADDENDUM；空窗/不可用期英文定稿可交 notion ai 顶替（不阻塞结题）
- **Δv″**：继续挂账至 VERIFY 比+smoke 齐后再论

## 2026-10-26 17:29:24 Australia/Perth — M2-H5-DENOM-VERIFY CLOSED FAIL · 挂账旁路（Notion 选 C）

- **票**：M2-H5-DENOM-VERIFY = CLOSED **H5-DENOM-VERIFY-FAIL**
- **gate**：`fd00a06058b239abfe03a52175463a52741718318ce99416a4ce762cf037889b` @ 2026-10-26 16:09:00 Asia/Taipei — **不改、不重跑翻盘**
- **result**：`docs/m2_h5_denom_verify_result.md`；pack：`docs/notion_m2_h5_denom_verify_pack.md`；next：`docs/notion_m2_h5_denom_verify_next.md`
- **已证**：C1 exact=`1.3358225694150683`（n=120）；pass_sessions=0/3；H5 比三项齐（med_r≈0.99–1.16）但 smoke 均≪17/20；对照 KEEP；composition.h 未改；落地=否；A′未开
- **矛盾**：分母同源+C1 后「比过、绝对 smoke 仍灭」—与 SEED-TRUTH 核心未解
- **Notion 决断**：主推 **C=挂账旁路**（采纳）；A′/另 calib=否；次选 D=ABS-CONFLICT-RESOLVE 暂不派
- **继续挂账**：Δv″；PERF-CI HOLD；禁抬 thr；禁立刻 Δv″；禁把 FAIL 当 PASS；七+VERIFY hash 不翻盘
- **不挡主线**：归档本 FAIL；无新假设不虚构再跑 VERIFY

## 2026-10-26 20:10:00 Asia/Taipei — M2-H5-ABS-CONFLICT-RESOLVE CLOSED PASS/R2 · 比–绝对冲突旁路已确认

- **票**：M2-H5-ABS-CONFLICT-RESOLVE = CLOSED **H5-ABS-RESOLVE-PASS**；menu=**R2 挂账确认**；mode=**doc-only**
- **gate**：`e96bf6238ace42ce2edcfe4191ef5ec45fa53eefca94fcf777f2b52c17425ade` @ 2026-10-26 18:30:00 Asia/Taipei — **不改 body/hash**（结题复算 MATCH）
- **APPROVED**：`docs/m2_h5_abs_conflict_resolve_approved.md` — pidan@2026-10-26 20:08:54 Australia/Perth；widget t269s1242
- **产物**：result `docs/m2_h5_abs_conflict_resolve_result.md`；pack `docs/notion_m2_h5_abs_conflict_resolve_pack.md`；next `docs/notion_m2_h5_abs_conflict_resolve_next.md`
- **写死**：采数=**否**（未采数）；未同选 R1；R3 仅占位；R4* 否决；禁抬 thr；八 hash 不翻盘；禁立刻 Δv″；禁翻 C1
- **挂账含义（R2 已确认）**：显式旁路「比–绝对冲突」；保留 **`H5-DENOM-VERIFY-FAIL` 仍 FAIL 归档**（不改写 PASS）；Δv″ / PERF-CI 继续 HOLD
- **下一票**：无新票；主线旁路确认后待命；无新假设不虚构再 VERIFY / 立刻 Δv″
- **不挡主线**：挂账旁路已确认；VERIFY FAIL 挂账保留
