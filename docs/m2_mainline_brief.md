# M2 主线 brief（骨架）

> LANDED_AT=2026-10-24 23:35:47 AWST（Australia/Perth）  
> 用户决策：挂账/降级旁路停放；**M2–M5 继续**。  
> **STATUS: M2-01 SELECTED / gate LOCKED** — `docs/m2_01_gate_LOCKED.md`；GATE_BODY_SHA256=`182f945c77b7103b8883be523742af8e6dcd8ca2d4232529a5fac347a168beb1`；LOCKED_AT=2026-10-24 23:41:05 AWST。M2-02 deferred（仅文档预起草，不派真机）。Gemini/DeepSeek 文档任务 in flight。**APPROVED_* 仍空 → 勿派 workbuddy / 勿采数。**

## Goal

- **M2 Alpha ~wk16**（plan.md）：C# API 可用（可打断重定向）；ETW 可用；预览宿主 v0；性能 CI 至少在 2 台物理机上运行。
- 承接 **R-2 → M2 目标**（notion_brief §4.1）：
  - harness 底噪降到 **< 0.5 DIP**
  - 影子追踪合成器 **2.6×** 根因查清
  - Δv 绝对门；L 偏高（B 40–45 ms vs A 20–29 ms）；多刷新率矩阵；平滑速度估计（可选）

## Candidate tickets（M2-01 已选；M2-02 延期真机）

| ID | 摘要 |
| --- | --- |
| **M2-01** | harness 底噪 < 0.5 DIP + 影子速度 2.6× 根因（**不改 `composition.h`**） |
| **M2-02** | C# 投影 + ETW + 预览宿主 v0（plan.md M2 闸门） |

## plan.md vs engineering.md

- **plan.md** M2/M3/M4 与 **engineering.md** 交付物列表**不完全对齐**（例：plan M2=C#/ETW/previewer/perf CI；engineering M2=Figma/Token/WinMD/热重载；engineering 把分发器/试点放到 M3，plan 把 Rive+试点放到 M3、perf/试点完成放到 M4）。
- **闸门 / 验收以 plan.md 为准**；**交付物清单可参考 engineering.md**，冲突时记差异、不静默改 plan。

## Hard bans（来自挂账旁路）

- **不重开 S1 / R-3**
- **不改 `composition.h`**（挂账工作亦然）
- **B_v ≠ ship 基线**；成功 ≠ 采用
- 挂账条目见 [side_parked_hang_degrade.md](./side_parked_hang_degrade.md)——**不阻塞**本主线

## STATUS

**M2-01 SELECTED / gate LOCKED**（Australia/Perth）

- Gate path: `docs/m2_01_gate_LOCKED.md`
- GATE_BODY_SHA256: `182f945c77b7103b8883be523742af8e6dcd8ca2d4232529a5fac347a168beb1`
- LOCKED_AT: 2026-10-24 23:41:05 AWST
- APPROVED_*: empty（本 LOCK **不**授权采数；勿派 workbuddy）
- **M2-02 deferred** for live test（文档预起草可；不派真机 / 不采数）
- Gemini / DeepSeek：文档任务 in flight（2.6× 假设与实验设计 / 底噪分解与 G2 复核；可选 M2-02 文档草图）
- DRAFT 保留：`docs/m2_01_gate_DRAFT.md`（未改）
- 禁令不变：不重开 S1 / R-3；不改 `composition.h`
- STATUS（追加，2026-10-24 23:42:17 AWST）：M2-01 APPROVED by pidan（见 `docs/m2_01_approved.md`；gate 182f945c…beb1 @ 2026-10-24 23:41:05 AWST）→ dispatching workbuddy（`docs/workbuddy_m2_01_dispatch.md`）。禁令不变。
- CLARIFICATION（追加，2026-10-24 23:50:02 AWST）：Notion 紧急决定 A → 继续 run，不作废 LOCKED gate 182f945c…beb1；DeepSeek 噪声文档仅作结果撰写的执行澄清；#30 不参与 G2（仅用本轮 static/zero-drive）；禁令不变。见 `docs/m2_01_exec_clarification.md`。

- **2026-10-25 00:02 AWST — M2-01 ticket PASS**（G2 PASS + G3 PASS）
  - GATE_BODY_SHA256 unchanged: 182f945c77b7103b8883be523742af8e6dcd8ca2d4232529a5fac347a168beb1
  - composition.h PRE=POST a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418 unchanged
  - G3 primary = H5 Seed source (spring.h closed-form v→IV); fix touches composition.h? NO (separate ticket)
  - result: docs/m2_01_result.md; artifacts/m2_01_*
  - bans unchanged: no reopen S1/R-3; no unfreeze Bv; success≠adopt; no composition.h change
  - next: Notion 结题包 + 决断下一票 in flight; M2-02 still deferred for live until Notion decides

- **2026-10-25 00:05:02 AWST — M2-01 CLOSED**
  - M2-01 CLOSED PASS（Notion 结题包 docs/notion_m2_01_pack.md；权威 docs/m2_01_result.md）
  - next ticket = M2-H5-SEED（Notion 决断 A：先修 H5 seed，不碰 composition.h；M2-02 live still deferred）
  - H5 gate DRAFT: docs/m2_h5_seed_gate_DRAFT.md（NOT LOCKED；不授权采数）
  - peers in flight: Gemini=WGC seed vs IV=null+0；DeepSeek=判据复核+调用点清单 + continue m2_02 draft
  - bans unchanged

- **H5 gate LOCKED**（2026-10-25 00:17:37 AWST）：`docs/m2_h5_seed_gate_LOCKED.md`；GATE_BODY_SHA256=`28a60c37c08fff4d28136160797182a530d2a15c8d4559f04a4fc23d7a3ea4b0`；LOCKED_AT=2026-10-25 00:17:37 AWST。**NOT APPROVED yet**（APPROVED_* 空）；APPROVED 前不派 workbuddy、不采数。禁令不变：不重开 S1/R-3/R-2；不解冻/升级 B_v；不改 composition.h；成功≠采用。

- STATUS（追加，2026-10-25 00:18:34 AWST）：M2-H5-SEED APPROVED by pidan @ 2026-10-25 00:18:34 AWST（见 `docs/m2_h5_seed_approved.md`；gate 28a60c37…a4b0 @ 2026-10-25 00:17:37 AWST）→ dispatching workbuddy（`docs/workbuddy_m2_h5_seed_dispatch.md`）。gate hash 不变；禁令不变。

- **2026-10-25 00:32:13 AWST — M2-H5-SEED ticket PASS**（G2 PASS；强口径）
  - GATE_BODY_SHA256 unchanged: 28a60c37c08fff4d28136160797182a530d2a15c8d4559f04a4fc23d7a3ea4b0 @ 2026-10-25 00:17:37 AWST
  - approved=pidan@2026-10-25 00:18:34 AWST
  - composition.h PRE=POST a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418 unchanged
  - 3/3 med_r=1.0000/1.0000/1.0000（各 n=12）；max r_i=1.0；calib=1.0；armB 对照 med_r=0；shadow 2.600 已消除
  - result: docs/m2_h5_seed_result.md；artifacts/m2_h5_seed_*
  - bans unchanged: no reopen S1/R-3/R-2; no unfreeze Bv; success≠adopt; no composition.h change
  - next: Notion 结题包 + 决断下一票 in flight（协调侧决断后立刻派）；workbuddy 待命；M2-02 live still deferred until Notion decides

- **2026-10-25 00:35:03 AWST — M2-H5-SEED CLOSED PASS**
  - Notion 结题包：docs/notion_m2_h5_seed_pack.md；权威 docs/m2_h5_seed_result.md
  - GATE 28a60c37…a4b0 @ 00:17:37 未改；composition.h a55d4952…e418 未改
  - next ticket = **M2-02**（Notion 决断 A）：plan.md M2 Alpha — C# API（可打断重定向）+ ETW + 预览宿主 v0；禁影子 IV（承接 H5）
  - M2-02 DRAFT：docs/m2_02_gate_DRAFT.md（NOT LOCKED；不授权采数）
  - M2-02 live：**仍延期至 LOCK+APPROVED**；本轮只派文档/设计/闸门补洞
  - 余项挂账（不挡）：Δv 绝对门 / L 偏高 / 多刷新率矩阵 / 平滑速度估计
  - peers：Gemini=预览宿主 v0 验收一步 + C# 打断 smoke；DeepSeek=ETW 最小集 + G2 补洞 + 修订 deepseek_m2_02_api_draft §3.4；Notion 收齐后可 LOCK 修订；workbuddy 待命
  - bans unchanged

- **M2-02 gate LOCKED**（2026-10-25 00:42:43 AWST）：`docs/m2_02_gate_LOCKED.md`；GATE_BODY_SHA256=`ce147d212d31910b84c6c8e5566fbf50e740efcaadb27747cde827db2ec0a180`；LOCKED_AT=2026-10-25 00:42:43 AWST。**NOT APPROVED yet**（APPROVED_* 空）；APPROVED 前不派 workbuddy、不采数。性能 CI ≥2 机另票。禁令不变：不重开 S1/R-3/R-2；不解冻/升级 B_v；不改 composition.h；禁影子 IV；成功≠采用。

- STATUS（追加，2026-10-25 01:11:43 AWST）：M2-02 APPROVED by pidan @ 2026-10-25 01:11:43 AWST（见 `docs/m2_02_approved.md`；gate ce147d21…a180 @ 2026-10-25 00:42:43 AWST）→ dispatching workbuddy（`docs/workbuddy_m2_02_dispatch.md`）。gate hash 不变；禁令不变；perf CI ≥2 机另票。

- **2026-10-25 01:30:56 AWST — M2-02 ticket PASS**（csharp=P ∧ etw=P ∧ preview=P；G2 PASS）
  - GATE_BODY_SHA256 unchanged: ce147d212d31910b84c6c8e5566fbf50e740efcaadb27747cde827db2ec0a180 @ 2026-10-25 00:42:43 AWST
  - approved=pidan@2026-10-25 01:11:43 AWST
  - composition.h PRE=POST a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418 unchanged
  - 3/3 独立进程；禁影子 IV；perf CI ≥2 机另票（未申）
  - result: docs/m2_02_result.md；artifacts/m2_02_*
  - bans unchanged
  - next: Notion 结题包 + 决断下一票 in flight；workbuddy 待命

- **2026-10-25 01:32:23 AWST — M2-02 CLOSED PASS**
  - Notion 结题包：docs/notion_m2_02_pack.md；权威 docs/m2_02_result.md
  - GATE ce147d21…a180 @ 00:42:43 未改；composition.h a55d4952…e418 未改
  - next ticket = **M2-PERF-CI**（Notion 决断 A）：plan.md M2 Alpha 性能 CI ≥2 物理机 + P95/P99；复用 PresentStats
  - DRAFT：docs/m2_perf_ci_gate_DRAFT.md（NOT LOCKED；不授权采数）
  - 真机：须 LOCK+APPROVED；**必须 ≥2 物理机**（1 机 INVALID）；M3 等本票收口后再起步
  - 余项挂账不挡：Δv / L偏高 / 多刷新率 / 平滑速度
  - peers：Gemini=场景清单+帧数/轮次+P95/P99；DeepSeek=PresentStats/PresentMon 对账+机器矩阵/INVALID（含 EventPipe vs logman）；Notion 收齐后可 LOCK；workbuddy 待命（需第二台物理机）
  - bans unchanged

- **M2-PERF-CI gate LOCKED**（2026-10-25 01:37:05 AWST）：`docs/m2_perf_ci_gate_LOCKED.md`；GATE_BODY_SHA256=`5b007dd43ba351dc0985a52d1745485954d149575feeca622791275005cd282a`；LOCKED_AT=2026-10-25 01:37:05 AWST。**NOT APPROVED yet**（APPROVED_* 空）；APPROVED 前不派 workbuddy、不采数。**须 ≥2 物理机**（已知 DESKTOP-K93N3P8；第二台待确认）。禁令不变。
- **2026-10-25 06:56 AWST — M2-PERF-CI HOLD**（pidan widget t217s948）：第二台物理机未就绪；gate `5b007dd4…282a` @ 01:37:05 **仍 LOCKED、未 APPROVED**；**不派 workbuddy**。就绪后重要 APPROVED。
- 旁路归档（同刻）：Claude #51 对照 FINAL 已落盘（见 docs/r3_commit_staleness.md / side_parked）；**不重开 R-3/S1**；主线不变。

- **2026-10-25 23:13 AWST — pidan：先不派第二台真机，继续**
  - M2-PERF-CI 保持 HOLD / LOCKED `5b007dd4…282a` @ 01:37:05；**不 APPROVED、不派双机真机**
  - 主线不停：交 Notion 决断「HOLD 下可并行的下一票」（候选：Δv 绝对门 / L 偏高 / 多刷新率（单机）/ 平滑速度；或 engineering M2 工具链文档票；**不开 M3 真机**；不重开 S1/R-3）
  - Gemini/DeepSeek 同步出方案与闸门风险；Notion 拍板后立刻 LOCK 流程
  - bans unchanged

- **2026-10-25 23:16 AWST — 并行下一票 = M2-Δv（Notion 决断 A）**
  - 决断：`docs/notion_m2_parallel_next.md`；DRAFT：`docs/m2_delta_v_gate_DRAFT.md`（NOT LOCKED；不授权采数）
  - 绝对门提案：`Δv_thresh=max(20 DIP/s, 0.05·|v_ref|)`；去相对臂 A；禁影子 IV；**1 物理机合法**
  - PERF-CI 仍 HOLD / LOCKED `5b007dd4…282a`（不改、不单机冒充）
  - B/D/E 挂账；C 多刷新本轮不并行；不开 M3 真机
  - peers in flight：Gemini=格表+绝对阈；DeepSeek=可判/Nv_h/H5+G2-LOCK；收齐后 Notion 可 LOCK 修订
  - bans unchanged

- **M2-Δv gate LOCKED**（2026-10-25 23:33:05 AWST）：`docs/m2_delta_v_gate_LOCKED.md`；GATE_BODY_SHA256=`d2349ceb0d37fdf332a4f00cb2c1f1217fe6856d0f3e5222ffaf82b8b15509c6`；LOCKED_AT=2026-10-25 23:33:05 AWST。**NOT APPROVED yet**（APPROVED_* 空）；APPROVED 前不派 workbuddy、不采数。**单机合法**（DESKTOP-K93N3P8）；并入 Claude INTERIM（估计器二次 w=5、Nv_h base P95、≥3 会话、对照臂等）。PERF-CI 仍 HOLD `5b007dd4…282a`（不改、不单机冒充）。禁令不变。

- STATUS（追加，2026-10-26 06:21:36 AWST）：M2-Δv APPROVED by pidan @ 2026-10-26 06:21:36 AWST（见 `docs/m2_delta_v_approved.md`；gate d2349ceb…09c6 @ 2026-10-25 23:33:05 AWST）→ dispatching workbuddy（`docs/workbuddy_m2_delta_v_dispatch.md`）。单机 DESKTOP-K93N3P8；gate hash 不变；PERF-CI 仍 HOLD `5b007dd4…282a`；禁令不变。

- **2026-10-26 08:56 AWST — M2-Δv ticket INCONCLUSIVE**（真机 DESKTOP-K93N3P8；workbuddy ONELINE）
  - GATE_BODY_SHA256 unchanged: d2349ceb0d37fdf332a4f00cb2c1f1217fe6856d0f3e5222ffaf82b8b15509c6 @ 2026-10-25 23:33:05 AWST
  - approved=pidan@2026-10-26 06:21:36 AWST
  - composition.h PRE=POST a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418 unchanged
  - 3/3 INCONCLUSIVE_CANDIDATE；核心 5 格全 rule-SKIP（t060: |a|·σ_ts>thr/2；t300: thr<2·Nv_h）；base 未 PASS；IVel0 t060 fwd+rev FAIL（负对照成立）；t156=rule-SKIP
  - result: docs/m2_delta_v_result.md；artifacts/m2_delta_v_*
  - bans unchanged；≠PERF-CI；PERF-CI 仍 HOLD `5b007dd4…282a`
  - next: Notion 结题包 + 决断下一票 in flight；workbuddy 待命

- **2026-10-26 08:59:30 AWST — M2-Δv CLOSED INCONCLUSIVE**
  - Notion 结题包：docs/notion_m2_delta_v_pack.md；权威 docs/m2_delta_v_result.md
  - GATE d2349ceb…09c6 @ 23:33:05 未改；composition.h a55d4952…e418 未改
  - 3/3 INCONCLUSIVE；0/5 核心格可判（全 rule-SKIP）；ctrl=baseF/IVel0Fok
  - next ticket = **M2-Δv′**（Notion 决断 A）：升版可判几何；KEEP thr=max(20,5%|v_ref|)；t156 SKIP ACCEPT
  - DRAFT：docs/m2_delta_v_prime_gate_DRAFT.md（NOT LOCKED；不授权采数）；决断 docs/notion_m2_delta_v_next.md
  - peers：Gemini=可判几何修订；DeepSeek=虚 PASS/base/harness 旁路；Notion 收齐后可 LOCK
  - PERF-CI 仍 HOLD 5b007dd4…282a；bans unchanged

- **M2-Δv′ gate LOCKED**（2026-10-26 09:05:37 AWST）：`docs/m2_delta_v_prime_gate_LOCKED.md`；GATE_BODY_SHA256=`06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`；LOCKED_AT=2026-10-26 09:05:37 AWST。**NOT APPROVED yet**（APPROVED_* 空）；APPROVED 前不派 workbuddy、不采数。升版可判：P1 `|a|σ≤thr`；P2 `thr≥Nv_h`；KEEP thr/w=5/t156 SKIP；前序 d2349ceb…09c6 **不改**。PERF-CI 仍 HOLD。禁令不变。

- STATUS（追加，2026-10-26 09:09:19 AWST）：M2-Δv′ APPROVED by pidan @ 2026-10-26 09:09:19 AWST（见 `docs/m2_delta_v_prime_approved.md`；gate 06467b82…805c @ 2026-10-26 09:05:37 AWST）→ dispatching workbuddy（`docs/workbuddy_m2_delta_v_prime_dispatch.md`）。单机 DESKTOP-K93N3P8；gate hash 不变；PERF-CI 仍 HOLD `5b007dd4…282a`；禁令不变。

- **2026-10-26 10:45 AWST — M2-Δv′ ticket FAIL**（真机 DESKTOP-K93N3P8；workbuddy ONELINE）
  - GATE_BODY_SHA256 unchanged: 06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c @ 2026-10-26 09:05:37 AWST
  - approved=pidan@2026-10-26 09:09:19 AWST
  - composition.h PRE=POST a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418 unchanged
  - 3/3 FAIL；t060 可判但 WGC 0–2/20 → 格 FAIL；ctrl=baseP/IVel0F；t300 多 SKIP；前序 d2349ceb 未重跑
  - result: docs/m2_delta_v_prime_result.md；artifacts/m2_delta_v_prime_*
  - bans unchanged；≠PERF-CI；PERF-CI 仍 HOLD `5b007dd4…282a`
  - next: Notion 结题包 + 决断下一票 in flight；workbuddy 待命

- **2026-10-26 10:50:13 Asia/Taipei — M2-Δv′ CLOSED FAIL**
  - Notion 结题包：docs/notion_m2_delta_v_prime_pack.md；权威 docs/m2_delta_v_prime_result.md
  - GATE 06467b82…805c @ 09:05:37 未改；composition.h a55d4952…e418 未改；不重跑翻盘
  - 3/3 FAIL；t060 judicable；WGC 0–2/20；ctrl=baseP/IVel0F；t300 多 SKIP
  - next ticket = **M2-WGC-SEED-ABS**（Notion 决断 A）：修 WGC→InitialVelocity；禁影子 IV；VEL-SMOOTH 并入
  - DRAFT：docs/m2_wgc_seed_abs_gate_DRAFT.md（NOT LOCKED）；决断 docs/notion_m2_wgc_seed_abs_next.md
  - peers：Gemini=WGC→IVel 路径；DeepSeek=H5 衔接/验收/风险；Notion 收齐后可 LOCK
  - PERF-CI 仍 HOLD；bans unchanged

- **M2-WGC-SEED-ABS gate LOCKED**（2026-10-26 10:59:25 Asia/Taipei）：`docs/m2_wgc_seed_abs_gate_LOCKED.md`；GATE_BODY_SHA256=`831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`。**NOT APPROVED yet**；APPROVED 前不派 workbuddy。默认 A=P-LS+TS N=5；smoke=t060；H5 比回归；禁翻盘 06467b82…805c；PERF-CI 仍 HOLD。禁令不变。

- STATUS（追加，2026-10-26 11:15:50 Asia/Taipei）：M2-WGC-SEED-ABS APPROVED by pidan @ 2026-10-26 11:15:50 Asia/Taipei（见 `docs/m2_wgc_seed_abs_approved.md`；gate 831818e4…80de @ 2026-10-26 10:59:25 Asia/Taipei）→ dispatching workbuddy（`docs/workbuddy_m2_wgc_seed_abs_dispatch.md`）。单机 DESKTOP-K93N3P8；gate hash 不变；PERF-CI 仍 HOLD `5b007dd4…282a`；禁令不变；结题标签 SEED-ABS-*；禁翻盘 06467b82…805c。

- **2026-10-26 11:55:10 Asia/Taipei — M2-WGC-SEED-ABS CLOSED SEED-ABS-FAIL**
  - 权威：docs/m2_wgc_seed_abs_result.md；gate `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de` @ 2026-10-26 10:59:25 Asia/Taipei **未改**
  - approved=pidan@2026-10-26 11:15:50；path=P-LS+TS/N=5；pass_sessions=0/3
  - H5 比：三会话 med_r≈1.33∉[0.8,1.2]（无 r_i≥2）；t060fwd 8–11/20、t060rev 0–1/20；ctrl=baseP/IVel0F；composition.h a55d4952…e418 未改；无影子IV；禁翻盘 06467b82…805c
  - next: Notion 结题包 + 决断下一票 in flight；workbuddy 待命
  - PERF-CI 仍 HOLD `5b007dd4…282a`；bans unchanged

- **2026-10-26 11:59:29 Asia/Taipei — Notion 结题包 + 下一票 = M2-WGC-SEED-CALIB**
  - pack：docs/notion_m2_wgc_seed_abs_pack.md；next：docs/notion_m2_wgc_seed_abs_next.md
  - DRAFT（NOT LOCKED）：docs/m2_wgc_seed_calib_gate_DRAFT.md — KEEP P-LS+TS/N=5；只标定 calib；Δv″/PATH-B′ 挂账
  - peers：DeepSeek=标定精确值+比窗预期+虚 PASS；Notion 收齐后可 LOCK
  - 粗算（待 DeepSeek 复核）：ABS pathA n=120 median(v_screen/v_seed)≈0.7511
  - PERF-CI 仍 HOLD；禁翻盘 831818e4…80de / 06467b82…805c；禁 LOCK+APPROVED 前采数

- STATUS（追加，2026-10-26 12:01:49 Asia/Taipei）：DeepSeek M2-WGC-SEED-CALIB 补洞已落盘 `docs/deepseek_m2_wgc_seed_calib_review.md` — calib=**0.7511**（n=120）；可 LOCK；rev 可能触发 PATH-B′ 挂账。已交 Notion 可 LOCK→LOCK。禁采数至 APPROVED。

- **M2-WGC-SEED-CALIB gate LOCKED**（2026-10-26 12:03:33 Asia/Taipei）：`docs/m2_wgc_seed_calib_gate_LOCKED.md`；GATE_BODY_SHA256=`09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`。**NOT APPROVED yet**；APPROVED 前不派 workbuddy。calib=0.7511；KEEP P-LS+TS；禁翻盘 831818e4…80de / 06467b82…805c；PATH-B′/Δv″ 挂账；PERF-CI 仍 HOLD。禁令不变。

- STATUS（追加，2026-10-26 13:04:53 Asia/Taipei）：M2-WGC-SEED-CALIB APPROVED by pidan @ 2026-10-26 13:04:53 Asia/Taipei（见 `docs/m2_wgc_seed_calib_approved.md`；gate 09f6414b…793d @ 2026-10-26 12:03:33 Asia/Taipei）→ dispatching workbuddy（`docs/workbuddy_m2_wgc_seed_calib_dispatch.md`）。calib=0.7511；单机 DESKTOP-K93N3P8；gate hash 不变；PERF-CI 仍 HOLD `5b007dd4…282a`；禁令不变。

- **2026-10-26 13:40:59 Asia/Taipei — M2-WGC-SEED-CALIB CLOSED SEED-CALIB-FAIL（PATH-B′ 挂账）**
  - 权威：docs/m2_wgc_seed_calib_result.md；gate `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d` @ 2026-10-26 12:03:33 Asia/Taipei **未改**
  - approved=pidan@2026-10-26 13:04:53；calib exact=0.7510829456；pass_sessions=0/3
  - H5 比：**过**（med_r≈0.99–1.00）；t060 smoke：**FAIL**（fwd 1–3/20；rev 0/20）→ G2-LOCK-2b **PATH-B′ 已挂**（未抬 thr/放宽17/20/改N）
  - ctrl=baseP/IVel0F；composition.h a55d4952…e418 未改；无影子IV；禁翻盘 831818e4…80de / 06467b82…805c
  - next: Notion 结题包 + 决断下一票 in flight；workbuddy 待命
  - PERF-CI 仍 HOLD `5b007dd4…282a`；bans unchanged

- **2026-10-26 13:43:58 Asia/Taipei — Notion 结题包 + 下一票 = M2-WGC-SEED-PATH-B′**
  - pack：docs/notion_m2_wgc_seed_calib_pack.md；next：docs/notion_m2_wgc_seed_calib_next.md
  - DRAFT（NOT LOCKED）：docs/m2_wgc_seed_path_b_prime_gate_DRAFT.md — KEEP calib exact=0.7510829456；变体 V1–V4 估窗/TS/P-VS/方向性；标签 SEED-PATHB-*；Δv″ 挂账
  - peers：DeepSeek=变体择一或 A/A′ 分级+预登记+虚 PASS；Notion 收齐后可 LOCK
  - PERF-CI 仍 HOLD；禁翻盘 09f6414b…793d / 831818e4…80de / 06467b82…805c；禁 LOCK+APPROVED 前采数

- STATUS（追加，2026-10-26 13:48:00 Asia/Taipei）：DeepSeek PATH-B′ 设计已落盘 `docs/deepseek_m2_wgc_seed_path_b_prime_design.md` — 默认 **A=V2-clamp**（t_int=min(t_comp,t_last_WGC)；KEEP N=5/calib exact）；A′=P-VS α=0.30 挂账次选；可 LOCK。已交 Notion 可 LOCK→LOCK。禁采数至 APPROVED。

- **M2-WGC-SEED-PATH-B′ gate LOCKED**（2026-10-26 13:49:20 Asia/Taipei）：`docs/m2_wgc_seed_path_b_prime_gate_LOCKED.md`；GATE_BODY_SHA256=`a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`。**NOT APPROVED yet**；APPROVED 前不派 workbuddy。默认 A=V2-clamp；KEEP calib exact；A′ 挂账次选；PERF-CI 仍 HOLD。禁令不变。

- STATUS（追加，2026-10-26 13:51:27 Asia/Taipei）：M2-WGC-SEED-PATH-B′ APPROVED by pidan @ 2026-10-26 13:51:27 Asia/Taipei（见 `docs/m2_wgc_seed_path_b_prime_approved.md`；gate a1161c77…3a43 @ 2026-10-26 13:49:20 Asia/Taipei）→ dispatching workbuddy（`docs/workbuddy_m2_wgc_seed_path_b_prime_dispatch.md`）。A=V2-clamp；单机 DESKTOP-K93N3P8；gate hash 不变；PERF-CI 仍 HOLD `5b007dd4…282a`；禁令不变。

- **2026-10-26 14:30:10 Asia/Taipei — M2-WGC-SEED-PATH-B′ CLOSED SEED-PATHB-FAIL + SEED-TRUTH-CONFLICT 挂账**
  - 权威：docs/m2_wgc_seed_path_b_prime_result.md；gate `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43` @ 2026-10-26 13:49:20 Asia/Taipei **未改**
  - approved=pidan@2026-10-26 13:51:27；path=PATHB-A=V2-clamp；calib exact=0.7510829456 KEEP；P-VS=OFF；pass_sessions=0/3
  - H5 比：**过**（med_r≈0.81–0.95）；t060 smoke：**全灭**（wgc_pass≈0/20）→ G2-LOCK-2b **SEED-TRUTH-CONFLICT 已挂**（未抬 thr/放宽17/20/再标定/改N）
  - t_int 钳制落实（n_clamped=40×3）；ctrl=baseP/IVel0F；A′未跑；composition.h a55d4952…e418 未改；无影子IV
  - 禁翻盘 09f6414b…793d / 831818e4…80de / 06467b82…805c；Δv″继续挂账
  - next: Notion 结题包 + 决断下一票 in flight；workbuddy 待命
  - PERF-CI 仍 HOLD `5b007dd4…282a`；bans unchanged

- **2026-10-26 14:35:24 Asia/Taipei — Notion 结题包 + 下一票 = M2-WGC-SEED-PATH-A′**
  - pack：docs/notion_m2_wgc_seed_path_b_prime_pack.md；next：docs/notion_m2_wgc_seed_path_b_prime_next.md
  - DRAFT（NOT LOCKED）：docs/m2_wgc_seed_path_a_prime_gate_DRAFT.md — KEEP V2-clamp + calib exact；**P-VS=ON α_v=0.30**；标签 SEED-PATHA-PRIME-*；SEED-TRUTH-CONFLICT 保持挂账；Δv″ 挂账
  - peers：DeepSeek=核对预登记 α_v / KEEP / 虚 PASS / G2-LOCK-2b 延续；Notion 收齐后可 LOCK
  - 否决：立刻 TRUTH-CONFLICT 主线 / 再标定 / 立刻 Δv″ / 翻盘四 FAIL（含 a1161c77…3a43）
  - PERF-CI 仍 HOLD；禁 LOCK+APPROVED 前采数

- STATUS（追加，2026-10-26 14:37:17 Asia/Taipei）：DeepSeek M2-WGC-SEED-PATH-A′ 补洞已落盘 `docs/deepseek_m2_wgc_seed_path_a_prime_review.md` — **可 LOCK**（α_v=0.30；KEEP 全表；无硬缺口）。已交 Notion 可 LOCK→LOCK。禁采数至 APPROVED。

- **M2-WGC-SEED-PATH-A′ gate LOCKED**（2026-10-26 14:40:41 Asia/Taipei）：`docs/m2_wgc_seed_path_a_prime_gate_LOCKED.md`；GATE_BODY_SHA256=`f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362`；LOCKED_AT=2026-10-26 14:38:15 Asia/Taipei（hash 复算 MATCH）。**NOT APPROVED yet**；APPROVED 前不派 workbuddy。默认 A′=V2-clamp+P-VS α_v=0.30；KEEP calib exact；标签 SEED-PATHA-PRIME-*；G2-LOCK-2b 延续；禁翻盘四 FAIL；PERF-CI 仍 HOLD。禁令不变。

- STATUS（追加，2026-10-26 14:45:22 Asia/Taipei）：M2-WGC-SEED-PATH-A′ APPROVED by pidan @ 2026-10-26 14:45:22 Asia/Taipei（见 `docs/m2_wgc_seed_path_a_prime_approved.md`；gate f17871f1…2362 @ 2026-10-26 14:38:15 Asia/Taipei）→ dispatching workbuddy（`docs/workbuddy_m2_wgc_seed_path_a_prime_dispatch.md`）。A′=V2-clamp+P-VS α_v=0.30；单机 DESKTOP-K93N3P8；gate hash 不变；PERF-CI 仍 HOLD `5b007dd4…282a`；禁令不变。

- NOTE（2026-10-26 14:48:39 Asia/Taipei）：Gemini 出口恢复后补交 `docs/gemini_m2_wgc_seed_abs_design.md`（M2-WGC-SEED-ABS 路径设计；否决影子 IV；推 P-LS+N=5±P-VS）。**ABS 已 CLOSED SEED-ABS-FAIL**（gate `831818e4…80de`）— **不重开、不翻盘、不据此改 LOCK**；稿归档对照用。主线仍等 PATH-A′ ONELINE（gate `f17871f1…2362` 真机中）。

- **2026-10-26 15:35:43 Australia/Perth — M2-WGC-SEED-PATH-A′ CLOSED SEED-PATHA-PRIME-FAIL（SEED-TRUTH-CONFLICT 保持挂账）**
  - 权威：docs/m2_wgc_seed_path_a_prime_result.md；gate `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362` @ 2026-10-26 14:38:15 Asia/Taipei **未改**
  - approved=pidan@2026-10-26 14:45:22；path=PATHB-A′=V2-clamp+V3-VS α_v=0.30；pass_sessions=0/3（s3 INVALID hang_cap 已替换）
  - H5：s1 ratioOk=False（max_r=6.60）；s2/s3 比过但 t060 smoke 全灭 → **SEED-TRUTH-CONFLICT 保持**
  - ctrl=baseP/IVel0F；未抬 thr/改 calib/改分母/开Δv″；composition.h a55d4952…e418 未改；无影子IV
  - 禁翻盘 a1161c77…3a43 / 09f6414b…793d / 831818e4…80de / 06467b82…805c
  - next: Notion 结题包 + 决断下一票 in flight；workbuddy 待命
  - PERF-CI 仍 HOLD `5b007dd4…282a`；bans unchanged

- **2026-10-26 15:40:44 Australia/Perth — Notion 结题包 + 下一票 = M2-SEED-TRUTH-CONFLICT**
  - pack：docs/notion_m2_wgc_seed_path_a_prime_pack.md；next：docs/notion_m2_wgc_seed_path_a_prime_next.md
  - DRAFT（NOT LOCKED）：docs/m2_seed_truth_conflict_gate_DRAFT.md — 诊断/决议；标签 SEED-TRUTH-*；默认不开真机采数；菜单 M1–M5 每项另票；不预授权改分母/抬 thr
  - peers：DeepSeek=冲突陈述边界 / 处置菜单「每项另票」/ 五 FAIL 不翻盘 / 默认不开采数；Notion 收齐后可 LOCK
  - 否决：再跑 α / 再标定 / 立刻 Δv″ / 翻盘五 FAIL
  - PERF-CI 仍 HOLD；禁 LOCK+APPROVED 前采数

- STATUS（追加，2026-10-26 15:42:13 Australia/Perth）：DeepSeek M2-SEED-TRUTH-CONFLICT 补洞已落盘 `docs/deepseek_m2_seed_truth_conflict_review.md` — **可 LOCK**；推荐主决议 **M1**（授权另票改 H5 分母/比–绝对同源；本票不执行）；次选 M4 STOP。已交 Notion 可 LOCK→LOCK。禁采数至 APPROVED（默认仍不开）。

- **M2-SEED-TRUTH-CONFLICT gate LOCKED**（2026-10-26 15:46:07 Australia/Perth）：`docs/m2_seed_truth_conflict_gate_LOCKED.md`；GATE_BODY_SHA256=`276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69`；LOCKED_AT=2026-10-26 15:43:30 Asia/Taipei（hash 复算 MATCH）。**NOT APPROVED yet**；决议票；主决议倾向 M1（另票 `M2-H5-DENOM-REVISE`）；次选 M4 STOP；默认不开采数；PERF-CI 仍 HOLD。禁令不变。

- STATUS（追加，2026-10-26 15:47:34 Australia/Perth）：M2-SEED-TRUTH-CONFLICT APPROVED by pidan @ 2026-10-26 15:47:34 Australia/Perth（见 `docs/m2_seed_truth_conflict_approved.md`；gate 276b97f0…4f69 @ 2026-10-26 15:43:30 Asia/Taipei）→ 派 Notion offline 决议结题（默认勾选 **M1**；**不开采数**）。gate hash 不变；PERF-CI 仍 HOLD；禁令不变。

- **2026-10-26 15:51:51 Australia/Perth — M2-SEED-TRUTH-CONFLICT CLOSED SEED-TRUTH-PASS（menu=M1）**
  - 权威：docs/m2_seed_truth_conflict_result.md；gate `276b97f0…4f69` **未改**；mode=offline-resolve；**未采数**；未执行改分母
  - pack：docs/notion_m2_seed_truth_conflict_pack.md；next：docs/notion_m2_seed_truth_conflict_next.md
  - next ticket = **M2-H5-DENOM-REVISE**；DRAFT（NOT LOCKED）：docs/m2_h5_denom_revise_gate_DRAFT.md
  - peers：DeepSeek=新分母定义/比–绝对同源/禁抬 thr/六 hash 不翻盘；Notion 收齐后可 LOCK
  - SEED-TRUTH-PASS ≠ 已改 H5 ≠ 采用 ≠ Δv-PASS；PERF-CI 仍 HOLD；禁 LOCK 前采数/改分母落地

- STATUS（追加，2026-10-26 15:53:42 Australia/Perth）：DeepSeek M2-H5-DENOM-REVISE 补洞已落盘 `docs/deepseek_m2_h5_denom_revise_review.md` — **可 LOCK**；推荐 **v_denom≡v_pre**（同窗 w=5）；废旧 2F 作门控分母；验证须另批 calib。已交 Notion 可 LOCK→LOCK。禁采数/禁改分母落地至 APPROVED。

- **M2-H5-DENOM-REVISE gate LOCKED**（2026-10-26 15:57:00 Australia/Perth）：`docs/m2_h5_denom_revise_gate_LOCKED.md`；GATE_BODY_SHA256=`d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7`；LOCKED_AT=2026-10-26 15:55:00 Asia/Taipei（hash 复算 MATCH）。**NOT APPROVED yet**；v_denom≡v_pre；默认不开采数/不落地；calib 须另批；PERF-CI 仍 HOLD。

- STATUS（追加，2026-10-26 16:00:17 Australia/Perth）：M2-H5-DENOM-REVISE APPROVED by pidan @ 2026-10-26 16:00:17 Australia/Perth（见 `docs/m2_h5_denom_revise_approved.md`；gate d891df9f…33c7 @ 2026-10-26 15:55:00 Asia/Taipei）→ **mode=doc-only**（采数否/落地否/calib不适用）→ 派 Notion offline 契约结题。gate hash 不变；PERF-CI 仍 HOLD；禁令不变。

- **2026-10-26 16:04:48 Australia/Perth — M2-H5-DENOM-REVISE CLOSED H5-DENOM-PASS（mode=doc-only）**
  - 权威：docs/m2_h5_denom_revise_result.md；gate `d891df9f…33c7` **未改**；未采数、未落地、calib 未批
  - pack：docs/notion_m2_h5_denom_revise_pack.md；next：docs/notion_m2_h5_denom_revise_next.md
  - next ticket = **M2-H5-DENOM-VERIFY**；DRAFT（NOT LOCKED）：docs/m2_h5_denom_verify_gate_DRAFT.md
  - peers：DeepSeek=验证采数范围/新 calib 标定式/落地默认否/七 hash 不翻盘；Notion 收齐后可 LOCK
  - H5-DENOM-PASS ≠ 已落地采用 ≠ Δv-PASS；Δv″ 继续挂账；PERF-CI 仍 HOLD；禁 LOCK 前采数

- **2026-10-26 16:07:22 Australia/Perth — DeepSeek M2-H5-DENOM-VERIFY 补洞齐 → Notion 可 LOCK**
  - 落盘：docs/deepseek_m2_h5_denom_verify_review.md；可 LOCK=是；Harness 空回代拟
  - calib=C1 median(v_denom/v_hat)；路径=PATHB-A（P-VS=OFF）；落地=否；验证采数 t060+H5 ≥3 进程单机
  - 已派 Notion 可 LOCK→LOCK；禁 LOCK 前采数；PERF-CI 仍 HOLD

- **2026-10-26 16:12:19 Australia/Perth — M2-H5-DENOM-VERIFY LOCKED；等 pidan APPROVED**
  - LOCKED：docs/m2_h5_denom_verify_gate_LOCKED.md；GATE_BODY_SHA256=`fd00a060…889b` @ 2026-10-26 16:09:00 Asia/Taipei；复算 MATCH
  - revise：docs/notion_m2_h5_denom_verify_gate_revise.md；DeepSeek 无实质分歧
  - 写死：采数=是（t060+H5≥3单机）；落地=否；calib=C1；PATHB-A；KEEP v_denom≡v_pre；七 hash 不翻盘
  - APPROVED_* 空 → 禁采数；PERF-CI 仍 HOLD

- **2026-10-26 16:12:48 Australia/Perth — pidan APPROVED M2-H5-DENOM-VERIFY**
  - approved：docs/m2_h5_denom_verify_approved.md；gate `fd00a060…889b`
  - 显式：采数=是；落地=否；calib=C1；PATHB-A
  - 派 workbuddy 单机真机；PERF-CI 仍 HOLD

- **2026-10-26 16:31:53 Australia/Perth — Claude 区域不可用挂账（M2-Δv English FINAL）**
  - 非 Free 额度；302 app-unavailable-in-region；INTERIM only；禁 INTERIM→LOCK
  - 不挡主线 VERIFY；routine 已删；待 egress 或交 notion ai 顶替英文定稿

- **2026-10-26 17:25:49 Australia/Perth — M2-H5-DENOM-VERIFY CLOSED H5-DENOM-VERIFY-FAIL（单机）**
  - result：docs/m2_h5_denom_verify_result.md；gate `fd00a060…889b` MATCH；calib C1 exact=`1.3358225694150683`（n=120）
  - pass_sessions=0/3：H5 比三项齐（med_r≈0.99–1.16）但 smoke 均≪17/20 → FAIL；ctrl KEEP；composition.h 未改；A′未开
  - 七 hash 不翻盘；落地=否；≠PERF-CI；禁立刻 Δv″
  - 下一步：Notion 结题包 + 下一票决断；PERF-CI 仍 HOLD

- **2026-10-26 17:29:24 Australia/Perth — Notion 下一票=挂账旁路 C（VERIFY FAIL 归档）**
  - pack/next 已落盘；采纳 C；A′/另 calib=否；次选 D 暂不派
  - VERIFY gate `fd00a060…889b` 不翻盘；C1 exact=`1.3358…` 保留；Δv″/PERF-CI 继续 HOLD
  - 挂账写入 docs/side_parked_hang_degrade.md；不挡主线；无新假设不重开 VERIFY

- **2026-10-26 18:27:16 Australia/Perth — 用户「继续」→ 派次选 D=M2-H5-ABS-CONFLICT-RESOLVE**
  - 已派 DeepSeek 写 DRAFT `docs/m2_h5_abs_conflict_resolve_gate_DRAFT.md` + review（doc-only；禁采数）
  - 挂账 C 保留；PERF-CI HOLD；禁立刻 Δv″

- **2026-10-26 18:28:58 Australia/Perth — DeepSeek ABS-CONFLICT-RESOLVE 补洞齐 → Notion 可 LOCK**
  - DRAFT + review 已落盘；可 LOCK=是；推荐 menu=R2 挂账确认；mode=doc-only

- **2026-10-26 18:31:45 Australia/Perth — M2-H5-ABS-CONFLICT-RESOLVE LOCKED；等 pidan APPROVED**
  - LOCKED：docs/m2_h5_abs_conflict_resolve_gate_LOCKED.md；hash=`e96bf623…5ade` @ 18:30:00 Asia/Taipei MATCH
  - 写死：doc-only；采数=否；主决议 R2；八 hash 不翻盘；PERF-CI HOLD

- STATUS（追加，2026-10-26 20:08:54 Australia/Perth）：M2-H5-ABS-CONFLICT-RESOLVE APPROVED by pidan @ 2026-10-26 20:08:54 Australia/Perth（见 `docs/m2_h5_abs_conflict_resolve_approved.md`；gate e96bf623…5ade @ 2026-10-26 18:30:00 Asia/Taipei）→ **mode=doc-only**；采数=**否**；主决议=**R2 挂账确认** → 派 Notion offline 决议结题（result/pack/next；标签 H5-ABS-RESOLVE-*）。gate hash 不变；PERF-CI 仍 HOLD；禁令不变。

- **2026-10-26 20:10:00 Asia/Taipei — M2-H5-ABS-CONFLICT-RESOLVE CLOSED H5-ABS-RESOLVE-PASS（menu=R2；doc-only）**
  - 权威：docs/m2_h5_abs_conflict_resolve_result.md；gate `e96bf6238ace42ce2edcfe4191ef5ec45fa53eefca94fcf777f2b52c17425ade` @ 2026-10-26 18:30:00 Asia/Taipei **未改**（复算 MATCH）；approved=pidan@2026-10-26 20:08:54 Australia/Perth
  - pack：docs/notion_m2_h5_abs_conflict_resolve_pack.md；next：docs/notion_m2_h5_abs_conflict_resolve_next.md
  - mode=**doc-only**；主决议=**R2 挂账确认**（未同选 R1；R3 占位；R4* 否决）；**未采数**；未抬 thr；未立刻 Δv″；未翻 C1
  - H5-ABS-RESOLVE-PASS ≠ VERIFY-PASS 改写 ≠ Δv-PASS ≠ 落地；`H5-DENOM-VERIFY-FAIL` 仍 FAIL 归档；八 hash 不翻盘
  - next：**无新票；主线旁路确认后待命**；Δv″ / PERF-CI 继续 HOLD；composition.h 未触

- **2026-10-26 — 协调点名 M2-PERF-CI-TOOLING（plan.md §8 第 8 项；offline-tooling；采数=否）**
  - Claude 接手会话落盘：`tools/perf_ci/perf_ci_grade.py` + 单测 29/29 + `tools/perf_ci/README.md`；DRAFT `docs/m2_perf_ci_tooling_gate_DRAFT.md`（未 LOCK）
  - 实现 LOCKED PERF-CI 闸门时发现开放问题 Q1–Q8，其中 Q1（Idle 与 ≥1000 帧冲突）、Q2（替代路径 P95≤T 无法满足）、Q7（基线不存在）会阻断首轮 PERF-CI，须在第二台机器就绪前裁决
  - PERF-CI `5b007dd4…282a` 不改，仍 HOLD；composition.h 未触；引擎单测复验全绿（timeline 422 / tokens 743 / controller 236 / composition 2305 / capi 23 / soft-demo 10）

- **2026-10-26 — 协调「按建议 1 开始做 M0」→ 开 M0-A DRAFT（分支 `claude/m0-tech-validation`）**
  - DRAFT：`docs/m0_tech_validation_gate_DRAFT.md`（未 LOCK；不授权采数）；待裁决 D1–D7（D1 = 是否允许 SendInput）
  - 代码：`m0/`（tear 探针、Rive 表面 DLL、构建和运行脚本）+ `MotionSoftDemo/M0Probe.cs`（MainPage 加 11 行入口）；composition.h 未触
  - 本地核实：WinAppSDK 2.5.1 的合成 interop IID 与系统版不同，已按头文件写入；Rive 钉 `runtime-v0.1.545`

- **2026-10-26 21:38:45 AWST — M0-A LOCKED（D1–D7 按建议）**
  - LOCKED：`docs/m0_tech_validation_gate_LOCKED.md`，hash=`942d6965dae1aa41f3b853923d08a11aac9714d60018888a5c486bfde5809768`
  - 派工单：`docs/workbuddy_m0_dispatch.md`（构建可先行；采数等 approved 留证）；MainPage 补丁 `m0/mainpage_m0_hook.patch`

- **2026-10-26 21:43:10 AWST — M0-A APPROVED by pidan → 派 workbuddy**
  - approved：`docs/m0_tech_validation_approved.md`；gate `942d6965…9768` 不变；采数=是（-Inject，3 会话）；派工单 `docs/workbuddy_m0_dispatch.md`

- **2026-10-26 23:50:32 AWST — M0-A CLOSED M0-A-PASS（3 会话，D1 注入）**
  - 结果：`docs/m0_tech_validation_result.md`；数据 `m0/results/`（commit fac309a5）；gate `942d6965…9768` 复算 MATCH；composition.h PRE=POST
  - M0-1a PASS 3/3（CPU P95 ≈0.03·T，GPU P95 0.04–0.19·T）；M0-2@60Hz 层间错位 ≤0.19 ms；表达式跟随比 UI 线程快约 45 ms；合成交换链接受 ALLOW_TEARING（API 层）
  - 偏离：参考图全空（异步着色器编译，探针缺陷，已修）→ 第④条改以截图确认，待协调确认
  - Go：Rive 路径继续；M0-B 等第二台机器（≥120Hz、Win11、有管理员权限）

- **2026-10-27 — M0-A 合入 main（PR #1）；开 M0-B DRAFT（分支 `claude/m0b-win11`）**
  - M0-A 第④条偏离：pidan 以「开pr合并」确认
  - M0-B DRAFT：`docs/m0b_win11_gate_DRAFT.md`（Win11 复核：M0-2 在 ≥120Hz 下正式判定、M0-1a 回归、M0-4 用 PresentMon）；待裁决 E1–E4；不授权采数

- **2026-10-27 00:19:21 AWST — M0-B LOCKED（E1–E4 按建议）**
  - LOCKED：`docs/m0b_win11_gate_LOCKED.md`，hash=`5e1bbb6d517babf6f04a94ee9af93cc17f208f5ea9e5bdcf7ac47c8cd49df203`
  - 分支 `claude/m0b-win11-run`（从 main `513dd5e` 拉出）；派工单 `docs/workbuddy_m0b_dispatch.md`（预检和构建可先做；采数等 approved）

- **2026-10-28 09:16:57 AWST — M0-B 闸门 r2 LOCKED（pidan：「修改120hz为75hz」）**
  - r2：`docs/m0b_win11_gate_r2_LOCKED.md`，hash=`fd3acfc787564137532cc52adb64066427a88b159f0c6b4312dfd9dece40505c`；取代 r1 `5e1bbb6d…f203`（r1 未 APPROVED、未采数，文件不改）
  - M0-2 判定所需刷新率改为 ≥75Hz，门槛 2 ms 不变；与 plan.md「120Hz 拖动错位测试」的偏离已记录，120Hz 结论转 M3 混合刷新率矩阵
  - 派工单已改指向 r2；采数仍等 approved

- **2026-10-28 09:18:00 AWST — M0-B APPROVED by pidan → 派 workbuddy**
  - approved：`docs/m0b_win11_approved.md`；gate r2 `fd3acfc7…505c` 不变；采数=是（-Inject，3 会话）；派工单 `docs/workbuddy_m0b_dispatch.md`

- **2026-10-28 09:36:54 AWST — plan.md 的 120Hz 全部改为 75Hz（pidan）**
  - 共 5 处：目标“60/75Hz 以上保持流畅”；M-0 任务与 M0 验收门槛“75Hz 拖动错位测试”；多屏矩阵“60+75Hz、60+144Hz 混合”；试点指标“60/75Hz 下”
  - 因此 M0-B r2（`fd3acfc7…505c`）的 ≥75Hz 判定与 plan.md 一致；r2 中“与 plan.md 的偏离 / 120Hz 结论转 M3”一条已不再适用（r2 已 LOCK，正文不改，以本条为准）
  - summary.md、disputes.md 中“120Hz 下 ≤1ms”是按比例换算的预算示例，未改

- **2026-10-28 12:01:34 AWST — M0-B CLOSED M0-B-INCONCLUSIVE（DESKTOP-IJI1KJG，Win11 26200，RX 7800 XT，75Hz，未提权）**
  - 结果：`docs/m0b_win11_result.md`；数据 `m0/results_m0b/`（commit 6096c411）；gate r2 `fd3acfc7…505c` 复算 MATCH；composition.h PRE=POST
  - M0-2：错位 0.06/0.08/0.01 ms（≤2 ms），但每条路径样本只有 161–163（要求 ≥200）→ INCONCLUSIVE
  - M0-1a：①②④ 和 CPU（≤0.073·T）都满足；GPU P95 因探针时间戳读取 bug（AMD 上触发）无效 → INCONCLUSIVE
  - 已修：探针 v3（GPU 查询读取、拖动 16→40 次）；建议出 r3 并整票重跑，待 pidan 决定

- **2026-10-28 14:53:18 AWST — M0-B 闸门 r3 LOCKED（pidan：「按建议出 r3」）**
  - r3：`docs/m0b_win11_gate_r3_LOCKED.md`，hash=`741729784693893dedb6190b5d2768617045554ae90a58dfe00fd52855f48ede`；取代 r2 `fd3acfc7…505c`（r2 文件不改，r2 结论 INCONCLUSIVE 保留）
  - 判据不变；要求探针 M0 v3、同一台 DESKTOP-IJI1KJG 整票重跑 3 个会话、结果放 `m0/results_m0b_r3/`、不混用 r2 数据；GPU 丢弃样本超过 10% 时 GPU P95 视为无效
  - 派工单：`docs/workbuddy_m0b_r3_dispatch.md`；采数等 r3 approved

- **2026-10-28 15:26:25 AWST — M0-B r3 APPROVED by pidan（提权 + PresentMon）→ 采数**
  - approved：`docs/m0b_win11_r3_approved.md`；gate r3 `74172978…8ede` 不变；派工单 `docs/workbuddy_m0b_r3_dispatch.md`；数据进 `m0/results_m0b_r3/`

- **2026-10-28 17:11:40 AWST — M0-B r3 CLOSED M0-B-PASS → M0 关闭**
  - 结果：`docs/m0b_win11_r3_result.md`；数据 `m0/results_m0b_r3/`（commit c3491a87）；gate r3 `74172978…8ede` 复算 MATCH；composition.h PRE=POST
  - M0-2 PASS：错位 −0.04/0.06/−0.06 ms，每条路径样本 404–408；M0-1a PASS：CPU P95 ≤0.096·T，GPU P95 0.014·T，丢弃 0，参考图非空且一致
  - M0-4：合成交换链 + ALLOW_TEARING 在 Win11/AMD 上为 Hardware: Independent Flip，AllowsTearing=1（屏幕层面可撕裂）
  - 偏离：执行方用了未提交的本地 runner（直接启动 Debug exe）；PresentMon 只覆盖 60 s，没抓到 hwnd 对照组。均不影响判据
  - Go：Rive 路径进入 M3 规划；Skia 备选不启用

- **2026-10-28 — M3 规划 DRAFT + M0 工具收尾（分支 `claude/m3-plan`）**
  - `docs/m3_plan_DRAFT.md`：WP1–WP5、票序、硬件需求；待裁决 F1–F6
  - `m0/run_m0.ps1`：新增 `-LaunchMode Exe`（并回 M0-B r3 执行方的直接启动方式）；PresentMon 改为随探针进程退出；探针也接受 `--m0-inject` 参数

- **2026-10-28 20:26:23 AWST — M3 规划 F1–F6 按建议裁决（pidan）**
  - F1 默认走合成表面，交换链可选；F2 撕裂默认关；F3 首次显示前离屏预热；F4 按未来 WinMD 的形状冻结 C# API，试点绑定它；F5 试点期按 B_v 发布，速度不连续写进设计约束，另请设计师盲测；F6 真机票保留闸门，代码和文档走 PR
  - 记录在 `docs/m3_plan_DRAFT.md` §6；PERF-CI 的 Q1–Q8 仍待裁决（PR #5 未合并）

- **2026-10-28 — PR #4 合入 main（M3 规划，F1–F6 已裁决；runner 收尾）；开 M3-RIVE-DPI DRAFT**
  - DRAFT：`docs/m3_rive_dpi_gate_DRAFT.md`（分支 `claude/m3-rive-dpi`）；待裁决 G1–G4；代码在裁决后写
  - 注意：两台测试机都是 1280×1024，Windows 通常只提供 100%/125% 缩放；150%/200% 留给 M3-MATRIX

- **2026-10-28 20:31:11 AWST — PERF-CI Q1–Q8 按建议裁决（pidan）→ 合入 PR #5**
  - 裁决记录在 `docs/m2_perf_ci_tooling_gate_DRAFT.md`；Q1/Q2/Q3/Q7 改变判据，写入新版 PERF-CI 闸门 r2（另开 DRAFT），原 LOCKED `5b007dd4…282a` 不改
  - 采数前提：Q4 真实 PresentStats、Q6 WPA 导出口径；PERF-CI 仍 HOLD（缺 GPU 档不同的第二台机器）

- **2026-10-28 20:39:58 AWST — M3-RIVE-DPI LOCKED（G1–G4 按建议）**
  - LOCKED：`docs/m3_rive_dpi_gate_LOCKED.md`，hash=`92e6b4e9fd1014faf74714eb6cb20fc7e998169dc1171785bb8ea43f68d97bf5`
  - 代码：`m0r_resize`/`m0r_get_resize`、`M3DpiProbe.cs`（`--m3-dpi-probe`）、`m3/run_m3_dpi.ps1`、`m3/mainpage_m3_hook.patch`；派工单 `docs/workbuddy_m3_rive_dpi_dispatch.md`；采数等 approved
  - PERF-CI r2：H1–H3 按建议裁决（Idle 0.1%；第三台机器由 pidan 安排；Q4 由 Claude 实现，Q6 由懂 WPA 的人做）

- **2026-10-28 21:28:50 AWST — M3-RIVE-DPI APPROVED by pidan → 派 workbuddy**
  - approved：`docs/m3_rive_dpi_approved.md`；gate `92e6b4e9…97bf5` 不变；采数=是（3 会话，操作员手动切换 100%↔125%）；派工单 `docs/workbuddy_m3_rive_dpi_dispatch.md`；结果进 `m3/results_dpi/`

- **2026-10-29 11:42:31 AWST — M3-RIVE-DPI：Claude 独立复核，与权威结题一致**
  - 权威结题：`docs/m3_rive_dpi_result.md`（notion ai，RIVE-DPI-PASS，有条件采用，csproj 不回灌主线）
  - 复核：`docs/m3_rive_dpi_result_claude_review.md`；补充 ⑥ 像素宽度核对（125% 时 Rive 区域 500 px，清晰重绘）、125% 截图被截断、每次切换的延迟 0.65–13.33 ms

- **2026-10-29 11:54:00 AWST — PR #6（DPI 代码进 main）已开，待 pidan 评审；M3-RIVE-LOST DRAFT 起草**
  - PR：https://github.com/oops11111/motion_framework/pull/6（`claude/m3-rive-dpi` → main，未合并；不含 csproj 本机补丁）
  - DRAFT：`docs/m3_rive_lost_gate_DRAFT.md`（分支 `claude/m3-rive-lost`，从 DPI 分支拉出）；K1–K6 待裁决；未 LOCK，不授权采数

- **2026-10-29 12:30:02 AWST — M3-RIVE-LOST：K1–K6 按建议裁决，实现已写；新增 K7 待裁决**
  - 实现：`m0rive` 设备丢失检测与自动恢复（SetRenderingDevice）、`M3LostProbe.cs`、`m3/run_m3_lost.ps1`（强制注入间隔与次数上限）、`m3/mainpage_m3lost_hook.patch`
  - K7：钉住版本的 Rive D3D11 后端在设备丢失时可能崩溃（`onMap` 不检查 Map 返回值，`VERIFY_OK` 直接 abort）；建议本票不改 Rive，崩溃判 FAIL 并另开补丁票
  - K7 裁决后 LOCK；仍不授权采数

- **2026-10-29 12:31:48 AWST — M3-RIVE-LOST LOCKED（K1–K7 全部按建议）**
  - LOCKED：`docs/m3_rive_lost_gate_LOCKED.md`，GATE_BODY_SHA256=`753c0a6c82f06c81cf741eecba278b926127771befa2a51b515a925a7873f27d`（已复算 MATCH）；代码 = `claude/m3-rive-lost` `23e08de`
  - 派工单草稿：`docs/workbuddy_m3_rive_lost_dispatch.md`（第 1–3 步构建和模拟冒烟可先做；第 4 步真实注入等 approved）
  - **未 APPROVED，不授权采数**

- **2026-10-29 12:48:46 AWST — M3-RIVE-LOST APPROVED（pidan）**
  - approved：`docs/m3_rive_lost_approved.md`；gate `753c0a6c…f27d` 不变；采数=是（3 会话 × 2 次 `dxcap -forcetdr`，先模拟冒烟）；派工单 `docs/workbuddy_m3_rive_lost_dispatch.md`；结果进 `m3/results_lost/`

- **2026-10-29 13:46:44 AWST — PR #6 合并（pidan 同意）：M3-RIVE-DPI 代码进 main**
  - main `c2a1c7f`；不含 csproj 本机补丁；composition.h 未改
  - `claude/m3-rive-lost` 基于已合并的 DPI 分支，之后的 LOST PR 只含恢复代码

- **2026-10-29 — 恢复本 brief（Claude）**：协调提交 `5a8825f` 基于旧的本地副本（`/workspace/motion_framework`，初始导入 `137c52e`），覆盖掉了 2026-10-26 至 2026-10-29 13:46 的 30 余条记录。已按 `1110e81` 的版本恢复，协调新增的条目原样保留在下面。
  - 协调副本带入的历史条目（原文）：
    STATUS（追加，2026-10-26 20:49:27 Australia/Perth）：用户指示 **开发停止、后续任务停止**。协调侧已停五方派工；取消未决（含 PERF-CI HOLD 等待）；**不再派新票/真机/结题**，直至用户再开。GitHub 已推 [oops11111/motion_framework](https://github.com/oops11111/motion_framework) `137c52e`；本地 `/workspace/motion_framework` 保留。
    - 这条是 2026-10-26 的旧状态；之后 pidan 已恢复开发（“接收开发项目，继续”），本条不再有效。

- **2026-10-29 10:00:00 Asia/Shanghai — M3-RIVE-DPI CLOSED RIVE-DPI-PASS（METRICS_PASS；有条件采用）**
  - 权威：docs/m3_rive_dpi_result.md；pack：docs/notion_m3_rive_dpi_pack.md；next：docs/notion_m3_rive_dpi_next.md
  - gate `92e6b4e9fd1014faf74714eb6cb20fc7e998169dc1171785bb8ea43f68d97bf5` @ 2026-10-28 20:39:58 AWST **MATCH 未改**；approved=pidan@2026-10-28 21:28:50 AWST
  - tip `67decd39…e47614` @ claude/m3-rive-dpi；sessions=3；switches=4/4/4；有条件采用；csproj 不回灌主线
  - composition.h 未触 `a55d4952…e418`；PERF-CI 仍 HOLD；下一票曾主推 M3-RIVE-LOST

- **2026-10-29 17:45:00 Asia/Shanghai — M3-RIVE-LOST CLOSED RIVE-LOST-INCONCLUSIVE（非 METRICS_PASS；未采用）**
  - 权威：docs/m3_rive_lost_result.md；pack：docs/notion_m3_rive_lost_pack.md；next：docs/notion_m3_rive_lost_next.md；brief：docs/m3_rive_lost_result_brief.md
  - 闸门：本地仅 docs/m3_rive_lost_gate_DRAFT.md（**无 LOCKED / 无 GATE_BODY_SHA256**）；DRAFT **未改**
  - tip `82a53ccee2dd7ed6456c635421e70eb18902694d` @ claude/m3-rive-lost；host=DESKTOP-IJI1KJG；BUILD_OK；SIM_SMOKE=PASS；elevated=True；dxcap=OK
  - sessions=1/3；s1=CRASHED_NO_VERDICT；stop=K5 inject1 not detected within 30s（无后续注入；**当日不重跑**）；lost/recover/memΔ=N/A；private_mb_seg0=179；event4101=0
  - composition.h PRE=POST CRLF=`33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97` LF=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` edited=False
  - 成功≠采用；未采用；PERF-CI 仍 HOLD；不重开 S1/R-3/R-2
  - next：**新 DRAFT 修探针/检测窗/注入探测**（须 LOCK+APPROVED 才采数）；禁当日重跑；禁插队 FOLLOW/MATRIX 真机

- **2026-10-29 — M3-RIVE-LOST r1：Claude 复核 + r2 DRAFT**
  - 复核：`docs/m3_rive_lost_result_claude_review.md`。同意非 PASS、未采用；更正三处事实：① 闸门已 LOCK（`753c0a6c…f27d`，tip 上复算 MATCH）且 APPROVED；② 应用没有崩溃，`CRASHED_NO_VERDICT` 是 runner 先结束进程再判断造成的误标；③ `dxcap -forcetdr` 退出码 0 但没有产生 TDR（0 条 4101），注入未生效 → 按 LOCKED G2 属 INVALID
  - r2 DRAFT：`docs/m3_rive_lost_gate_r2_DRAFT.md`；L1–L5 待裁决（建议改用 `pnputil /restart-device` 重启显卡驱动、采数前注入自检、修 runner 标注）；不授权采数

- **2026-10-29 18:13:07 AWST — M3-RIVE-LOST r2 LOCKED（L1–L5 按建议）**
  - LOCKED：`docs/m3_rive_lost_gate_r2_LOCKED.md`，GATE_BODY_SHA256=`dc0770c1f6a6819a5f6cffeb6bea2ef828b93045ec8e75f39910ee9537d29d10`（复算 MATCH）；代码 `05f7576`
  - 注入改为 `pnputil /restart-device`；采数前自检；runner 崩溃标注已修；DLL 不变
  - r1 标签按 L5 更正为 `RIVE-LOST-INVALID`（附录写在 `docs/m3_rive_lost_result.md` 末尾，正文不改）
  - 派工单：`docs/workbuddy_m3_rive_lost_r2_dispatch.md`；**未 APPROVED，不授权采数**

- **2026-10-29 18:15:58 AWST — M3-RIVE-LOST r2 APPROVED（pidan）**
  - approved：`docs/m3_rive_lost_r2_approved.md`；gate r2 `dc0770c1…9d10` 不变；采数=是（自检 1 次 + 3 会话 × 2 次 `pnputil /restart-device`，先模拟冒烟）；派工单 `docs/workbuddy_m3_rive_lost_r2_dispatch.md`；结果进 `m3/results_lost_r2/`

- **2026-10-30 — 再次恢复本 brief（Claude）**：协调提交 `a67b142` 又基于旧的本地副本写入，覆盖了 2026-10-29 恢复后的记录。已按 `0aea9d1` 的版本恢复，协调新增的 r2 结题条目原样保留在下面。**协调侧需要先从 origin 同步（`git pull origin claude/m3-rive-lost`）再写文档。**

- **2026-10-30 08:50:00 Asia/Shanghai — M3-RIVE-LOST r2 CLOSED RIVE-LOST-FAIL（非 METRICS_PASS；未采用）**
  - 权威：docs/m3_rive_lost_r2_result.md；pack：docs/notion_m3_rive_lost_r2_pack.md；next：docs/notion_m3_rive_lost_r2_next.md；brief：docs/m3_rive_lost_r2_result_brief.md
  - 对照 r1：r1 仍 CLOSED `RIVE-LOST-INCONCLUSIVE`（**不翻盘**）；r2=探测成立但恢复 abort → **FAIL**
  - PROBE gate `7afbcd980d464abdeb835f059e05089edbf1d8f1112efb3e1d1330bd386e6b54` @ 2026-10-29 17:49:40 Asia/Taipei **未改**；LOST DRAFT **未改**；本地无 approved（本结题只归档 ONELINE）
  - commit=`382683cfa369f789a8f926ee7c0eba0a581a04ec`；tip_read=`0aea9d172b28065b453abb3b635eccb5e1012807` @ claude/m3-rive-lost
  - BUILD_OK（DLL reuse BD6F4827…）；SIM_SMOKE=PASS（仅 SIM；recover_ok=1 ≈221/210；probe=M3-LOST v2）；selftest=OK（pnputil RX7800XT ~3.3s）；elevated=True
  - sessions=3/3；s1/s2/s3=FAIL_ABORT rive_abort=1；lost_detected=1/1/1；recover_ok/recover_max_ms/memΔ=N/A（无 recovery）；injections_ok=1/1/1（每会话 2 次只成 1）；event4101=0；bugcheck=0
  - results=`m3/results_lost_r2/`（r1 未触）；K7 finding delivered no re-run
  - composition.h PRE=POST CRLF=`33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97` LF=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` edited=False
  - 成功≠采用；未采用；PERF-CI 仍 HOLD；不重开 S1/R-3/R-2；禁当日重跑；禁插队 FOLLOW/MATRIX
  - next：**新 DRAFT `M3-RIVE-LOST-RECOVER`**（专攻 abort/重建）；PROBE 探测能力已由 r2 实证（挂账关闭探针残差 / 保留前置）；须 LOCK+APPROVED 才采数

- **2026-10-30 — M3-RIVE-LOST r2：Claude 复核 + RECOVER DRAFT**
  - 复核：`docs/m3_rive_lost_r2_result_claude_review.md`。同意 FAIL、未采用；注入已生效（自检 OK，3/3 检测到丢失）；abort 发生在恢复过程中（检测后不再画帧），Rive 的 `VERIFY_OK` 全在建资源路径上，最可能是驱动重启期间就在新设备上建 Rive 对象
  - 更正：本次对应 r2 LOCKED `dc0770c1…9d10` 且有 approved；权威结题引用的“PROBE 闸门 7afbcd98”不在仓库
  - RECOVER DRAFT：`docs/m3_rive_lost_recover_gate_DRAFT.md`；M1–M5 待裁决（诊断、恢复前确认新设备可用、本票暂不改 Rive）；不授权采数

- **2026-10-30 11:31:47 AWST — M3-RIVE-LOST-RECOVER LOCKED（M1–M5 按建议）**
  - LOCKED：`docs/m3_rive_lost_recover_gate_LOCKED.md`，GATE_BODY_SHA256=`16202071fd08d091d0a0d825eda2297297ae19227d72a6cf981d4ed8b25484ce`（复算 MATCH）；代码 `90c4180`
  - 恢复前先确认新设备可用（硬件适配器、检查过返回值的小资源、稳定 500 ms）；abort 时记录恢复阶段并保存 stderr；本票不改 Rive
  - r2 结题已附更正（M5）；派工单 `docs/workbuddy_m3_rive_lost_recover_dispatch.md`（需重建 DLL）；**未 APPROVED，不授权采数**

- **2026-10-30 11:47:20 AWST — M3-RIVE-LOST-RECOVER APPROVED（pidan）**
  - approved：`docs/m3_rive_lost_recover_approved.md`；gate `16202071…84ce` 不变；采数=是（重建 DLL；模拟冒烟 → 自检 → 3 会话 × 2 次驱动重启）；派工单 `docs/workbuddy_m3_rive_lost_recover_dispatch.md`；结果进 `m3/results_lost_recover/`

- **2026-10-30 — M3-RIVE-LOST-RECOVER 真机结果：Claude 复核 → 判据全部满足（RIVE-LOST-PASS，待协调结题和采用决断）**
  - 数据 `m3/results_lost_recover/`（`68ce59d`）；gate `16202071…84ce` 复算 MATCH；approved 在先；composition.h PRE=POST
  - 3 会话 × 2 次驱动重启：6/6 检测、6/6 恢复（2.06–2.32 s ≤ 5 s），0 abort；恢复后段 600–601 帧、0 失败、CPU ≤ 0.096·T、GPU 0.013·T；内存 −23～−25 MB，旧设备引用全为 0
  - r2 abort 的原因已证实：驱动重启初期没有硬件适配器（DXGI_ERROR_NOT_FOUND），r2 在不可用设备上建了 Rive 对象
  - **新发现**：每次驱动重启后应用自身的 XAML 界面全部消失且不再重画（Rive 视觉正常）；疑似 WinUI 3 问题，未证实；建议下一票做无 Rive 的对照
  - 复核：`docs/m3_rive_lost_recover_result_claude_review.md`

- **2026-10-30 — 第三次恢复本 brief（Claude）**：协调提交 `94886fb` 仍基于旧的本地副本，覆盖了 `cb5a2f1` 之前的记录。已恢复，协调新增的 RECOVER 结题条目原样保留在下面。协调副本仍未从 origin 同步，所以它的结题里写“闸门未 LOCK、无 approved”，也没有看到 XAML 界面消失的发现。

- **2026-10-30 12:10:00 Asia/Shanghai — M3-RIVE-LOST-RECOVER CLOSED RIVE-LOST-RECOVER-PASS（METRICS_PASS 有条件；有条件采用验证结论；不合 main）**
  - 权威：docs/m3_rive_lost_recover_result.md；pack：docs/notion_m3_rive_lost_recover_pack.md；next：docs/notion_m3_rive_lost_recover_next.md；brief：docs/m3_rive_lost_recover_result_brief.md
  - 闸门：docs/m3_rive_lost_recover_gate_DRAFT.md（**仍 DRAFT；未 LOCK**；本结题未改）；本地**无** approved
  - 采数口径：**按用户指令采数 / 闸门未 LOCK**（协调转述：pidan 跳过开放问题拍板，直接下令 4 步采数；协调侧视该指令为本次采数授权；结题不虚构批准文件）
  - tip_read=`c954696470849f8f466d3c5d7f7dd57159cd8cbd`；commit=`68ce59d3cea4c9e8ec155015207113ce718a7ab6`；tip_after_push=同 commit @ claude/m3-rive-lost
  - BUILD_OK；dll_sha256=`d3aef01a0e410c30a147588cbc3dab6be18f0fdf50ba2674d1223e9ca1f0b840`；SIM_SMOKE=PASS（仅 SIM；recover_ok=1；756.94；probe=M3-LOST v3）；selftest=OK（pnputil ~3.3s）；elevated=True
  - s1/s2/s3=RECORDED/RECORDED/RECORDED；lost_detected=2/2/2；recovered=2/2/2；recover_ok=1/1/1；recover_max_ms=2319.99/2206.54/2160.70（只记录；超参考 500ms 不挡）；rive_abort=none；budget_ok/stability_ok=1/1；frames_missed=331/347,341/342,340/331（**挂账**：勿盲当 frames_post）；mem_delta_mb=-23/-25/-25；injections_ok=1/1/1（有条件齐；凭 lost_detected=2）
  - results=`m3/results_lost_recover/`（ONELINE 31 files；本根本地无矩阵可读）
  - composition.h PRE=POST CRLF=`33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97` LF=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` edited=False
  - 对照：r1 仍 INCONCLUSIVE、r2 仍 FAIL（**不翻盘**）；PROBE `7afbcd98…6b54` **不改**；相对 r2 abort → 本票恢复成立
  - 成功≠采用；有条件采用验证结论；**不合 main**；PERF-CI 仍 HOLD；不重开 S1/R-3/R-2
  - next：**M3-RIVE-FOLLOW 新 DRAFT（文档）**；建议补 RECOVER LOCK 归档（≠采数授权）；FOLLOW/MATRIX 不再因恢复未成立硬挡，但仍须各自 LOCK+APPROVED；**不授权采数**

- **2026-10-30 — M3-XAML-LOST DRAFT（诊断票）**
  - `docs/m3_xaml_lost_gate_DRAFT.md`：弄清驱动重启后 XAML 界面不恢复是 WinUI 3 本身的问题还是我们的代码；普通窗口 vs 带 Rive 窗口同日对照，并尝试 4 种恢复操作；N1–N5 待裁决（建议先于 FOLLOW 真机）；不授权采数

- **2026-10-30 13:36:05 AWST — M3-XAML-LOST LOCKED（N1–N5 按建议）**
  - LOCKED：`docs/m3_xaml_lost_gate_LOCKED.md`，GATE_BODY_SHA256=`69a52e2de66153348bff5197dbb037dd2650c26055b834173abde6ae6a1e1f55`（复算 MATCH）；分支 `claude/m3-xaml-lost`，代码 `4c04450`
  - 普通窗口 3 会话 + 带 Rive 1 会话，各重启驱动 1 次，加自检共 5 次；观测 +5/+15/+30 s 和三种恢复操作
  - RECOVER 结题已附更正（N5）；派工单 `docs/workbuddy_m3_xaml_lost_dispatch.md`；**未 APPROVED，不授权采数**

- **2026-10-30 13:42:56 AWST — M3-XAML-LOST APPROVED（pidan）**
  - approved：`docs/m3_xaml_lost_approved.md`；gate `69a52e2d…1f55` 不变；采数=是（走流程 → 自检 → 3 普通 + 1 带 Rive，各重启驱动 1 次）；派工单 `docs/workbuddy_m3_xaml_lost_dispatch.md`；结果进 `m3/results_xaml_lost/`（分支 `claude/m3-xaml-lost`）
