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

- **2026-10-30 — 第四次恢复本 brief（Claude）**：协调提交 `d5d8300` 仍基于旧的本地副本，覆盖了 `bb61eae` 之前的记录。已恢复，协调新增的 XAML-LOST 结题条目原样保留在下面。

- **2026-10-30 17:05:00 Asia/Shanghai — M3-XAML-LOST CLOSED XAML-LOST-PASS（METRICS_PASS 有条件；有条件采用验证结论；不合 main）**
  - 权威：docs/m3_xaml_lost_result.md；pack：docs/notion_m3_xaml_lost_pack.md；next：docs/notion_m3_xaml_lost_next.md；brief：docs/m3_xaml_lost_result_brief.md
  - 闸门：**本地无 XAML-LOST LOCKED**（无 DRAFT/LOCKED/APPROVED 可引用）；采数口径=**按用户指令采数**（pidan 2026-10-30 16:43 四步直接下令；无 APPROVED 文件）
  - tip_read=`bb61eae8ca52ee4be6d0e9d36f0bebd62c45e012`；commit=`efce2195904ad9acf77fafb2ea5c0d8e4d81f556`；tip_after_push=同 commit @ **claude/m3-xaml-lost**（tip 曾误写 claude/m3-rive-lost）
  - BUILD_OK；dll 复用 RECOVER `d3aef01a0e410c30a147588cbc3dab6be18f0fdf50ba2674d1223e9ca1f0b840`；DRYRUN=PASS；selftest=OK（~1.6s）；push=IJI1KJG direct
  - s1/s2/s3=RECORDED plain inject_ok=1；s4=RECORDED with_rive=1 inject_ok=1 lost_detected=1 recovered=1 recover_ms=2004.58 rive_abort=none
  - results=`m3/results_xaml_lost/`（ONELINE 72 files；本根本地无矩阵可读）
  - composition.h PRE=POST CRLF=`33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97` LF=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` edited=False
  - 成功≠采用；有条件采用验证结论；**不合 main**；PERF-CI 仍 HOLD；不重开 S1/R-3/R-2；不翻盘 r1/r2/RECOVER
  - next：**M3-RIVE-FOLLOW**（已 LOCKED hash `0f85a95277dd279e51085a42cd491b55db332ba49077055d045f794fc6b30a13`；APPROVED 空；等 pidan）；**本 next 不授权采数**

- **2026-10-30 — M3-XAML-LOST：Claude 复核 → `XAML-LOST`（不同意协调的 `XAML-LOST-PASS`）**
  - 普通窗口（无 Rive）3/3 与带 Rive 1/1 表现一致：驱动重启后所有文字、圆角元素、开关、按钮、标题栏图标和标题消失，等待、改大小、最小化还原、重设内容都不恢复；XAML 仍在出帧（Rendering 75–76/s，纯色块持续变色）→ 平台问题，不是我们的代码
  - 闸门 `69a52e2d…1f55` 复算 MATCH、approved 在先（协调结题“无闸门、无 approved”是旧副本所致）
  - 复核：`docs/m3_xaml_lost_result_claude_review.md`；下一步待 pidan 决定（Q1–Q4）

- **2026-10-30 — Q1–Q4 按建议（pidan）**
  - Q1：XAML-LOST 结题已附更正（标签 `XAML-LOST`，平台问题；闸门 LOCKED 且 APPROVED）
  - Q2：后续诊断 DRAFT `docs/m3_xaml_rebuild_gate_DRAFT.md`（分支 `claude/m3-xaml-rebuild`）：重启前已开的窗口、重启后新建的窗口能否画文字；打包部署是否一样；R1–R4 待裁决；不授权采数
  - Q3：有最小复现后，由 pidan 决定是否向 microsoft-ui-xaml 报告
  - Q4：FOLLOW 与此无关，可并行；P1–P6 仍待裁决

- **2026-10-30 17:35:36 AWST — M3-XAML-REBUILD LOCKED（R1–R4 按建议；K93N3P8 有管理员权限，纳入）**
  - LOCKED：`docs/m3_xaml_rebuild_gate_LOCKED.md`，GATE_BODY_SHA256=`3ea74c371ae983cebe480e1b4dea99e61aec92cfbe9646fb87b91b7a1d456fdc`（复算 MATCH）；分支 `claude/m3-xaml-rebuild`，代码 `12c720d`
  - A：IJI1KJG unpackaged 3 会话；B：IJI1KJG 打包 1 会话（启动不了记 BLOCKED_PACKAGED）；C：K93N3P8 1 会话
  - 派工单 `docs/workbuddy_m3_xaml_rebuild_dispatch.md`；**未 APPROVED，不授权采数**
  - P1–P6（FOLLOW）已按建议裁决，实现在 `claude/m3-follow`

- **2026-11-01 — 第五次恢复本 brief（Claude）**：协调提交 `0ef414e` 这次已经对照了正确的闸门和 approved，但 brief 仍被整体改写，丢了 164 行历史。已按 `0ef414e~1` 恢复，协调新增的两条原样保留在下面。

- **2026-11-01 16:26:53 Asia/Shanghai — 双票 APPROVED（XAML-REBUILD + RIVE-FOLLOW）**（文档归档；本行≠派 workbuddy）
  - pidan 明文：`APPROVED M3-XAML-REBUILD` + `APPROVED M3-RIVE-FOLLOW`
  - 留证：`docs/m3_xaml_rebuild_approved.md`（LOCK `3ea74c37…6fdc` @ `claude/m3-xaml-rebuild`）；`docs/m3_rive_follow_approved.md`（LOCK `285e39dc…aab3` @ `claude/m3-follow`，SendInput P4）
  - **FOLLOW 以 SendInput 版 LOCK `285e39dc9c1100f92ddddc5560a3631b94129a9b56c561c9803c8cbee57eaab3` 为准**（`docs/m3_rive_follow_gate_LOCKED.md`）
  - 归档：PropertySet LOCK `0f85a95277dd279e51085a42cd491b55db332ba49077055d045f794fc6b30a13` **已由** SendInput `285e39dc…aab3` **取代**；SUPERSEDED 落点=`docs/m3_rive_follow_gate_LOCKED.md.bak_propertyset_0f85a952`（PropertySet 原文未再单独保留；bak 不删）
  - 派工单顶部已标 APPROVED 生效（只读确认）：`docs/workbuddy_m3_xaml_rebuild_dispatch.md`、`docs/workbuddy_m3_rive_follow_dispatch.md`
  - composition.h 未改 `a55d4952…e418`；Success≠adopt；不合 main；**PERF-CI 仍 HOLD**

- **2026-11-01 17:55:00 Asia/Shanghai — M3-XAML-REBUILD CLOSED XAML-REBUILD-PASS（DIAGNOSIS_PASS 有条件；R4=KNOWN_LIMIT；有条件采用验证结论；不合 main）**
  - 权威：docs/m3_xaml_rebuild_result.md；pack：docs/notion_m3_xaml_rebuild_pack.md；next：docs/notion_m3_xaml_rebuild_next.md；brief：docs/m3_xaml_rebuild_result_brief.md
  - gate MATCH `3ea74c371ae983cebe480e1b4dea99e61aec92cfbe9646fb87b91b7a1d456fdc`；APPROVED pidan @ 2026-11-01 16:26:53 Asia/Shanghai
  - tip_read=`24ccce6c3e8d618c704c27c8e675ae039c26599c`；commit=`d00ffd0f5be2f11f7b5b4e30f287c7d0b16b9e9f` @ claude/m3-xaml-rebuild；results=`m3/results_xaml_rebuild/` 121 files
  - TEXT：A/B W_pre=TEXT-LOST、W_post=TEXT-LOST（进程级；打包同向）；C main=TEXT-OK、W_*=INCONCLUSIVE（DPI 截窗挂账）
  - R4=**KNOWN_LIMIT**（建议重启应用；宿主重建窗口不充分）；Success≠adopt；composition.h 未触；PERF-CI HOLD
  - next：**M3-RIVE-FOLLOW 已 APPROVED（SendInput `285e39dc…aab3`）且已派 — 待命/并行；本结题不授权新采数、不阻拦 workbuddy**

- **2026-11-01 — M3-XAML-REBUILD：Claude 复核（同意协调的逐窗口判读）**
  - IJI1KJG：进程级丢失（W_post 也全黑），打包同样 → 已知限制、建议重启应用
  - K93N3P8：主窗口完整，但该会话不带 Rive，无法证明设备确实丢失；需带 Rive 复测
  - runner 截图两处缺陷（被遮挡、不感知 DPI）；B 的 7 张截图拍到了执行方的私人聊天和通知，待 pidan 决定是否删除
  - 复核：`docs/m3_xaml_rebuild_result_claude_review.md`

- **2026-11-01 18:37:27 AWST — S1–S4 按建议（pidan）；M3-XAML-K93 LOCKED**
  - S1：`m3/run_m3_xaml_lost.ps1` 截图改为 `PrintWindow`（按窗口取内容，不受遮挡）+ 每显示器 DPI 感知；打包启动时复制 `m0rive.dll` 进 AppX（`104e5f9`）
  - S2：`docs/m3_xaml_k93_gate_LOCKED.md`，GATE_BODY_SHA256=`22fb8b92d8703619215ffaae3f0d5d9efea003951bb44b42d790d488ac28b657`（复算 MATCH）；K93N3P8 带 Rive 1 会话；派工单 `docs/workbuddy_m3_xaml_k93_dispatch.md`；**未 APPROVED，不授权采数**
  - S3：B 组 7 张拍到私人内容的截图已从分支最新版本删除（历史未改写），见 `m3/results_xaml_rebuild/b/REMOVED.md`
  - S4：对外报告等 K93 结果；已知限制草稿 `docs/m3_known_limits.md`

- **2026-11-01 20:58:26 AWST — M3-XAML-K93 APPROVED（pidan）**
  - approved：`docs/m3_xaml_k93_approved.md`；gate `22fb8b92…b657` 不变；采数=是（K93N3P8，自检 + 1 会话带 Rive）；派工单 `docs/workbuddy_m3_xaml_k93_dispatch.md`；结果进 `m3/results_xaml_k93/`

- **2026-11-02 — 再次恢复本 brief（Claude）**：协调提交 `58cc1a4` 又整体改写了 brief（−200 行）。已按 `58cc1a4~1` 恢复，协调新增的条目原样保留在下面。**请协调侧只在 brief 末尾追加，不要整体重写。**

- **2026-11-02 08:11:19 Australia/Perth — M3-RIVE-FOLLOW CLOSED RIVE-FOLLOW-PASS（METRICS_PASS；有条件采用 WP2 设计约束；不合 main）**
  - 权威：docs/m3_rive_follow_result.md；pack：docs/notion_m3_rive_follow_pack.md；next：docs/notion_m3_rive_follow_next.md；brief：docs/m3_rive_follow_result_brief.md
  - gate MATCH `285e39dc9c1100f92ddddc5560a3631b94129a9b56c561c9803c8cbee57eaab3`（SendInput 现行；PropertySet bak 勿用）；APPROVED pidan @ 2026-11-01 16:26:53 Asia/Shanghai
  - commit/tip=`0fb0760d0897f378e82b0242a803d18afcbf9e4f` @ claude/m3-follow（parent 9715164…；IJI 直推）
  - build=m0rive BUILD_OK dll_sha256=`733c5cb3a0397a1fdd560b67ba53adddf0dc5fc10eb10e5b4395ae99871f4aa4` + demo Debug OK
  - s1/s2/s3=RECORDED/RECORDED/RECORDED；A/B/C/D p50_ms=64.72/63.48/40.78/54.62｜65.84/63.18/40.70/54.52｜67.16/64.53/40.47/54.13
  - c_vs_a_frames=1.80/1.89/2.00（split_default=1）；b_vs_a_frames=0.09/0.20/0.20（latch_default=0）；seam_frames=-1.93/-1.93/-1.95（seam_ok=0→弹簧阻尼约束）；budget_ok=1/1/1
  - results=`m3/results_follow/`（9 files）；composition.h PRE=POST CRLF=`33d826c7…6e97` edited=False
  - 成功≠采用；有条件采用设计约束（C默认拆分；B不升默认；接缝阻尼）；**不合 main**；PERF-CI 仍 HOLD
  - next：**M3-XAML-K93 已 APPROVED 且 K93 在跑 — 待命/并行；本结题不授权新采数、不阻拦**

- **2026-11-02 09:20:00 Australia/Perth — M3-XAML-K93 CLOSED XAML-K93-PASS（METRICS_PASS 有条件；有条件采用验证结论；构建路径非 K93 原生；不合 main）**
  - 权威：docs/m3_xaml_k93_result.md；pack：docs/notion_m3_xaml_k93_pack.md；next：docs/notion_m3_xaml_k93_next.md；brief：docs/m3_xaml_k93_result_brief.md
  - gate MATCH `22fb8b92d8703619215ffaae3f0d5d9efea003951bb44b42d790d488ac28b657`（协调/workbuddy + tip 旁证；**本地未见 LOCK 文件**）；decision_doc=HIT Decision C；原 APPROVED 仍有效
  - tip_read=`cbc5fdb285570fde7d71fb7f3782d02f1663f8fd`；commit=`bfd76c3660b157ca3c903bb9f2afae07339c6f55` @ claude/m3-xaml-rebuild；results=`m3/results_xaml_k93/` 26 files
  - build_path=**IJI_BYPASS_COPY**；k93_native=**BLOCKED_TOOLCHAIN**；dll_sha256=`70ed8d14ff0e73793d36dd6799e8972f9877cd652eee54401f35b444de6da15f`
  - dryrun=RECORDED；sessions=1×RECORDED（with_rive=1 packaged=1 inject_ok=1 shots=20）；lost_detected=1 recovered=1 lost_now=0 recover_ms=4504.25 rive_abort=none reason=0x887A0005
  - W_pre=1440x756 W_post=1153x605（**缺 TEXT-OK/LOST 目视字段**）；composition.h PRE=POST 未触；Success≠adopt；PERF-CI HOLD
  - 工具链 A/B **挂账不挡本票**；next：**M3-MATRIX DRAFT（纯文档）— 不授权真机**；本结题不授权新采数

- **2026-11-02 — M3-XAML-K93：Claude 复核 → XAML 在 K93N3P8 上能恢复（问题限于 IJI1KJG 环境）**
  - Rive 证实设备确实丢失（`0x887A0005`，恢复 4.50 s）；主窗口、W_pre、W_post 全部 TEXT-OK，计数器持续前进
  - 结论：XAML-LOST 是 Windows 11 预览版 26300 和/或 AMD 驱动的问题，不是 WinUI 3 普遍问题；按 S4 向反馈中心 / AMD 报告（由 pidan 决定）
  - 构建在 IJI1KJG 完成后复制到 K93（协调 Decision C），代码与锁定提交一致，记为执行偏离
  - 复核：`docs/m3_xaml_k93_result_claude_review.md`；已知限制已更新：`docs/m3_known_limits.md`

- **2026-10-30 — M3-RIVE-FOLLOW DRAFT（plan §7 第 3 张）**
  - `docs/m3_rive_follow_gate_DRAFT.md`（分支 `claude/m3-follow`，从 `claude/m3-xaml-lost` 拉出）：四条跟随路径（表面内 UI 线程转交 / 表面内渲染线程读取 / 拆出 visual 表达式驱动 / 拆出 visual UI 线程写入）的指针延迟和拆分接缝；P1–P6 待裁决；真机排在 XAML-LOST 之后（N1）；不授权采数

- **2026-10-30 17:40:39 AWST — M3-RIVE-FOLLOW LOCKED（P1–P6 按建议）**
  - LOCKED：`docs/m3_rive_follow_gate_LOCKED.md`，GATE_BODY_SHA256=`285e39dc9c1100f92ddddc5560a3631b94129a9b56c561c9803c8cbee57eaab3`（复算 MATCH）；分支 `claude/m3-follow`（含 XAML-REBUILD 代码），代码 `b9e6f32`
  - 四条跟随路径 × 3 会话，每路径 40 次阶跃 + 40 次拖动；需重建 m0rive.dll；`SendInput` 仅限本票
  - 派工单 `docs/workbuddy_m3_rive_follow_dispatch.md`（建议在 XAML-REBUILD 之后做）；**未 APPROVED，不授权采数**

- **2026-11-01 — M3-RIVE-FOLLOW APPROVED 留证补记**
  - pidan 在 2026-11-01 16:26:53（UTC+8）同时批准了 XAML-REBUILD 和 RIVE-FOLLOW；当时只落了 REBUILD 的留证，现补 `docs/m3_rive_follow_approved.md`；gate `285e39dc…eaab3` 复算 MATCH；FOLLOW 尚未采数

- **2026-11-02 — 再次恢复本 brief（Claude）**：协调提交 `c4cf6be` 又整体改写了 brief（−194 行）。已按 `c4cf6be~1` 恢复，协调新增的条目原样保留在下面。**请协调侧只在 brief 末尾追加，不要整体重写。**

- **2026-11-02 — M3-RIVE-FOLLOW：Claude 复核（同意 RIVE-FOLLOW-PASS 的数字和规则判读）**
  - C（拆出 + 表达式）40.5–40.8 ms，比 A（表面内）快 1.8–2.0 帧；B（渲染线程读光标）只比 A 快 0.1–0.2 帧；接缝 −1.93 帧（拆出的部件领先 Rive 主体约 26 ms）
  - 解释：表面内多出的约 2 帧来自绘制表面管线本身，不在 UI 线程；WP2 设计约束草稿写在复核里（松耦合部件拆出 + 弹簧阻尼，严格对齐的部件留在 Rive 里）
  - 流程：FOLLOW 与 REBUILD 的 approved 留证都是采数之后才落盘（批准本身在先）；以后应先落留证
  - 复核：`docs/m3_rive_follow_result_claude_review.md`

- **2026-11-02 — T1–T4 按建议（pidan；M3-MATRIX 的开放问题交 Claude 定）**
  - T1：WP2 指针跟随设计约束已采用：`docs/m3_wp2_follow_guidelines.md`
  - T2：外部问题报告草稿 `docs/m3_xaml_device_lost_report_draft.md`（英文，未提交到任何外部渠道；由谁在哪里提交由 pidan 决定）
  - T3：`claude/m3-follow` 已合入 XAML-REBUILD / K93 的最新内容，开 PR 到 main（不合并，待评审）
  - T4：M3-MATRIX DRAFT 另开分支

- **2026-11-02 — PR #7 已开（M3 LOST / XAML 诊断 / FOLLOW 代码进 main），待 pidan 评审，未合并**
  - https://github.com/oops11111/motion_framework/pull/7（`claude/m3-follow` → main；不含 csproj 本机补丁；composition.h 未改）

- **2026-11-02 09:34:33 AWST — M3-MATRIX LOCKED（开放问题 X1–X7 由 Claude 代定，pidan 授权）**
  - LOCKED：`docs/m3_matrix_gate_LOCKED.md`，GATE_BODY_SHA256=`d8c21d0304931c42d805ceef35fccf4df50b5f2931fd916b81a13eea5db1963f`（复算 MATCH）；分支 `claude/m3-matrix`，代码 `fe81db8`
  - IJI1KJG（75 Hz，100%）+ 一块外接显示器（60 Hz，125% 或 150%）；探针自己移动窗口跨屏，测每屏节奏、跨屏卡顿、重新光栅化、预算；144 Hz、200%、DRR、RDP、GPU 切换、热插拔留作后续
  - 派工单 `docs/workbuddy_m3_matrix_dispatch.md`；**未 APPROVED，不授权采数**

- **2026-11-05 14:41:43 AWST — M3-MATRIX APPROVED（pidan）**
  - approved：`docs/m3_matrix_approved.md`；gate `d8c21d03…963f` 不变；采数=是（IJI1KJG + 一块外接显示器，3 会话）；派工单 `docs/workbuddy_m3_matrix_dispatch.md`；结果进 `m3/results_matrix/`

- **2026-11-06 — 再次恢复本 brief（Claude）**：协调提交 `feaec91` 又整体改写了 brief（−262 行）。已按 `feaec91~1` 恢复，协调新增的条目原样保留在下面。

- **2026-11-02 09:20:00 Asia/Shanghai — M3-MATRIX DRAFT 已落（纯文档；NOT LOCKED；不授权真机）+ 工具链 A/B 挂账**
  - DRAFT：`docs/m3_matrix_gate_DRAFT.md`（NOT LOCKED；APPROVED_* 空；LOCK≠采数；不派 workbuddy）
  - 结果目录建议：`m3/results_matrix/`；标签建议 `RIVE-MATRIX-*`（备选 `M3-MATRIX-*` → LOCK）
  - 工具链 A（钉/降 DirectX-Headers）/ B（升/对齐 Win SDK）→ 挂账 `docs/side_parked_hang_degrade.md`；源自 K93 `BLOCKED_TOOLCHAIN`；Decision C 旁路已结题；**不挡主线**
  - 依据 next：`docs/notion_m3_xaml_k93_next.md`；FOLLOW/XAML/LOST 线只读消费；不翻 CLOSED
  - composition.h 未触；Success≠adopt；不合 main；**PERF-CI 仍 HOLD**；**本行不授权真机/采数**

- **2026-11-06 09:45:00 Asia/Shanghai — M3-MATRIX CLOSED RIVE-MATRIX-FAIL（pacing_ok=0；不采用；不合 main）**
  - 权威：docs/m3_matrix_result.md；pack：docs/notion_m3_matrix_pack.md；next：docs/notion_m3_matrix_next.md；brief：docs/m3_matrix_result_brief.md
  - gate MATCH `d8c21d0304931c42d805ceef35fccf4df50b5f2931fd916b81a13eea5db1963f`（workbuddy；**本机未见 LOCK 文件**；不重算 body）
  - tip_read=`9fe1ffe850162b9eb374d4552bba49462894e4df`；commit=`c0ef236fff3a501bef002fff7db42398f686ed89` @ claude/m3-matrix；dll=`0d7044c7ed9b6fb494ae73194f5775a8c7bd50e713abf8008f9e25d3a67fb6da` BUILD_OK；push=OK
  - display：Philips17S4 1280x1024@75Hz/100% + VSC-VX2758-2K-PRO 2560x1440@180Hz/150%
  - sessions=3×RECORDED；pacing_ok=**0**；hitch_ok=1；size_ok=1；budget_ok=1；stability_ok=1；scale_changes=4
  - composition.h PRE=POST `33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97`（LF `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` 未改）；results=`m3/results_matrix/`
  - lab备注：UseShellExecute=false→MF_M0_ARTDIR；probe After()；pacing 失败未重跑（未入库）
  - Success≠adopt；**不采用**；不合 main；PERF-CI 仍 HOLD
  - next：**建议 M3-MATRIX-PACING 新 DRAFT（纯文档；NOT LOCK；不授权真机）— 勿自动 LOCK**；本结题不授权新采数

- **2026-11-06 09:50:00 Asia/Shanghai — M3-MATRIX-PACING DRAFT 已落（纯文档；NOT LOCKED；不授权真机）**
  - DRAFT：`docs/m3_matrix_pacing_gate_DRAFT.md`（sha256=`569695033907ec981b1fb7dddd54571f7f7991228ad370442be8402c172ef427`；APPROVED_* 空；LOCK≠采数；不派 workbuddy；**勿擅自 LOCK**）
  - 依据：`docs/notion_m3_matrix_next.md`；消化 MATRIX CLOSED `RIVE-MATRIX-FAIL` 残差 `pacing_ok=0`（hitch/size/budget/stability=1；3×RECORDED；scale_changes=4；双屏 75Hz@100%+180Hz@150%）
  - 结果目录建议：`m3/results_matrix_pacing/`（勿覆写 `m3/results_matrix/`）；标签建议 `RIVE-MATRIX-PACING-*`
  - 开放问题：pacing 定义/硬vs记录型/混刷双列/同拓扑复测/hitch 分立/计时器/是否禁改判 MATRIX — 供 pidan 拍板
  - composition.h 未触；Success≠adopt；不合 main；**PERF-CI 仍 HOLD**；**本行不授权真机/采数**；**不暗示可采数**

- **2026-11-06 09:55:00 Asia/Shanghai（UTC+8） — M3-MATRIX-PACING 已 LOCK（LOCK≠采数；不授权真机；APPROVED_* 空）**
  - LOCKED：`docs/m3_matrix_pacing_gate_LOCKED.md`；GATE_BODY_SHA256=`2543e424cd71eea2959fe942ce0bfe7d7118ea6ee5c28b31da7902bce904812c`；LOCKED_AT=2026-11-06 09:55:00 Asia/Shanghai（UTC+8）
  - DRAFT 保留并 SUPERSEDED：`docs/m3_matrix_pacing_gate_DRAFT.md`（原 sha `569695033907ec981b1fb7dddd54571f7f7991228ad370442be8402c172ef427`）
  - 写死：硬 pacing；混刷双列均过；瞬态 N=2.0s/M=120；强制 IJI 同拓扑；≥3；只测 pacing；dir=`m3/results_matrix_pacing/`；hitch 分立；After()/QPC；GC→INVALID；禁回改 CLOSED MATRIX；标签 `RIVE-MATRIX-PACING-*`
  - **不派 workbuddy**；**不暗示可采数**；composition.h 未触；Success≠adopt；不合 main；**PERF-CI 仍 HOLD**

- **2026-11-06 — M3-MATRIX：Claude 复核（同意 RIVE-MATRIX-FAIL）+ PACING 实现**
  - 原因：混合刷新率下 DWM 按最快的屏（180 Hz）合成，Rive 渲染线程用 `DwmFlush` 跟着 180 Hz 走，在 75 Hz 屏上也每秒画约 178 帧；跨屏、尺寸、预算、稳定都通过
  - 协调锁定的 M3-MATRIX-PACING（`2543e424…812c`）只规定判读，没有修复；不改代码重跑必然 FAIL
  - 已实现（`248a2d4`）：`m0r_set_pacing_window` 让渲染线程按窗口所在屏的垂直同步出帧（默认行为不变）；探针 pacing 模式按闸门 G2 做双列判读；修正 MATRIX 执行方备注的两处测试工具缺陷
  - 复核 `docs/m3_matrix_result_claude_review.md`；实现说明 `docs/m3_matrix_pacing_impl_claude.md`；派工单 `docs/workbuddy_m3_matrix_pacing_dispatch.md`；**未 APPROVED，不授权采数**；批准时需确认渲染器改动在本票范围内

- **2026-11-06 — M3-MATRIX-PACING 真机结果：Claude 复核 → pacing 修好（3/3 `pacing_ok=1`），但按 G2 ④ 判 FAIL（预算回退）**
  - 75 Hz 列 13.33 ms、180 Hz 列 5.556 ms，偏差 P95 0.11–0.23 ms，0 超长；跨屏最长 13.6 ms
  - 180 Hz 屏上 GPU P95 从 0.041·T 升到约 1.06·T（MATRIX 同屏对照），三会话一致 → `budget_ok=0`；推断为垂直同步时与 DWM 读取同一表面的冲突（GPU 在等待，不是工作量增加）
  - 建议 r2：`WaitForVBlank` 之后再 `DwmFlush` 再画；同一套判据复测
  - 复核：`docs/m3_matrix_pacing_result_claude_review.md`

- **2026-11-06 10:57:03 AWST — M3-MATRIX-PACING r2 LOCKED（pidan：按建议出 r2）**
  - LOCKED：`docs/m3_matrix_pacing_gate_r2_LOCKED.md`，GATE_BODY_SHA256=`a1fa4590285f52948ecf51bc9c32629d4d2c3bc42efc7f5bb4e4922bd2be2883`（复算 MATCH）；代码 `53cedde`
  - 判据与 r1 完全相同；唯一改动：垂直同步之后再 `DwmFlush` 再画
  - 派工单 `docs/workbuddy_m3_matrix_pacing_r2_dispatch.md`；结果进 `m3/results_matrix_pacing_r2/`；**未 APPROVED，不授权采数**

- **2026-11-06 — 再次合并本 brief（Claude）**：协调提交 `1f22889` 又整体改写了 brief（−265 行）。合并时保留完整历史，协调新增的条目原样附在下面。

- **2026-11-06 10:55:00 Asia/Shanghai — M3-MATRIX-PACING CLOSED RIVE-MATRIX-PACING-FAIL（纯文档结题；不采用；不合 main）**
  - result：`docs/m3_matrix_pacing_result.md` sha=`55154fcd358b36c52045b9c11d2fc910c9409093df3fcf139a09631db1477199`
  - pack/brief/next/mainline：close_pack=`4b393057c04a…`；next 主推 **M3-MATRIX-BUDGET DRAFT**（勿擅自 LOCK）
  - gate MATCH `2543e424cd71eea2959fe942ce0bfe7d7118ea6ee5c28b31da7902bce904812c`；approved pidan@2026-11-06 10:40:00 Asia/Shanghai（UTC+8）；tip `fd05c6c058022c233f1cd896338e03d665c43d0f`；results `7d5f545c7a3af0b7410421006a3683ce64c93f07` @ claude/m3-matrix
  - **budget_ok=0**；pacing_ok=1（节奏修复已验证）；hitch/size/stability=1；3×RECORDED；dir=`m3/results_matrix_pacing/`
  - composition.h 未触；Success≠adopt；**PERF-CI 仍 HOLD**；不授权新采数/真机；禁回改 CLOSED MATRIX

- **2026-11-06 10:57:53 AWST — M3-MATRIX-PACING r2 APPROVED（pidan）**
  - approved：`docs/m3_matrix_pacing_r2_approved.md`；gate r2 `a1fa4590…2883` 不变；采数=是（同拓扑，3 会话，`-Pacing`）；派工单 `docs/workbuddy_m3_matrix_pacing_r2_dispatch.md`；结果进 `m3/results_matrix_pacing_r2/`

- **2026-11-06 — 再次恢复本 brief（Claude）**：协调提交 `cfa4252` 又整体改写了 brief（−283 行）。已按 `cfa4252~1` 恢复，协调新增的条目原样保留在下面。

- **2026-11-06 11:00:00 Asia/Shanghai — M3-MATRIX-BUDGET DRAFT 已落（纯文档；NOT LOCKED；不授权真机）**
  - DRAFT：`docs/m3_matrix_budget_gate_DRAFT.md`（sha256=`49bbc9757feeade1f5b571c317e01be2c9d14fb9e464fa98438e5f31dba54745`；APPROVED_* 空；LOCK≠采数；不派 workbuddy；**勿擅自 LOCK**）
  - 依据：`docs/m3_matrix_pacing_next.md`；消化 PACING CLOSED `RIVE-MATRIX-PACING-FAIL` 残差 `budget_ok=0`（pacing_ok=1 已验证；禁回改 CLOSED）
  - 结果目录建议：`m3/results_matrix_budget/`；标签建议 `RIVE-MATRIX-BUDGET-*`
  - 开放问题：票名/T与双列/硬vs放宽/同拓扑/与pacing边界/节奏修复必含/FAIL照交/标签/doc-only旁路 — 供 pidan 拍板
  - composition.h 未触；Success≠adopt；不合 main；**PERF-CI 仍 HOLD**；**本行不授权真机/采数**；**不暗示可采数**

- **2026-11-06 11:08:00 AWST — M3-MATRIX-PACING r2 CLOSED RIVE-MATRIX-PACING-FAIL（纯文档结题；不采用；不合 main；Australia/Perth（AWST，UTC+8））**
  - result：`docs/m3_matrix_pacing_r2_result.md` sha=`fda913e489c0b04e5badc986b18bb475f88736a4fc2aa6048930835f844cb589`
  - pack/brief/next/mainline 已落；next 主推 **PACING 残差新 DRAFT**；重排相对 BUDGET DRAFT 优先级；**勿擅自 LOCK**
  - gate MATCH `a1fa4590285f52948ecf51bc9c32629d4d2c3bc42efc7f5bb4e4922bd2be2883` @ 2026-11-06 10:57:03 AWST；approved pidan@2026-11-06 10:57:53 AWST；tip `27ad82395e3eed94c40ad99feba6aed46eeca715`；results `e3feb5cde7441e1855727dd0468275a6254e56bc`；dir=`m3/results_matrix_pacing_r2/`
  - **pacing_ok=0**；budget_ok=1；hitch/size/stability=1；probe v2；pacing_source=vblank+dwmflush；WaitForVBlank→DwmFlush（`53cedde`）
  - 对照 r1：pacing1/budget0 vs r2 pacing0/budget1；**均 FAIL；禁回改 r1/MATRIX**
  - composition.h 未触；Success≠adopt；**PERF-CI 仍 HOLD**；不授权新采数/真机

- **2026-11-06 — M3-MATRIX-PACING r2：Claude 复核（同意 FAIL）**
  - r2 预算恢复（180 Hz 屏 GPU 0.07·T），但节奏坏了：75 Hz 列偏差 P95 5.1–5.3 ms（门槛 3.33），180 Hz 列约一半帧被跳过（122 fps）；原因是 `DwmFlush` 在垂直同步之后的等待长短不一，且常常越过下一次垂直同步
  - 结论：出帧必须只锚定垂直同步（r1 的做法）；r1 的预算失败来自拷贝时等 DWM 读表面，需要把 GPU 计时拆开才能判断
  - 复核：`docs/m3_matrix_pacing_r2_result_claude_review.md`；下一步待 pidan 决定（V1–V3）

- **2026-11-06 11:14:15 AWST — M3-MATRIX-PACING r3 LOCKED（pidan：V1 按建议）**
  - LOCKED：`docs/m3_matrix_pacing_gate_r3_LOCKED.md`，GATE_BODY_SHA256=`90f9356bf2739ea2a95a56619fc73af561b0c34e4ebd097be05442f8c5719476`（复算 MATCH）；代码 `878892d`
  - 出帧时机回到 r1（只等垂直同步）；GPU 计时拆成 Rive 渲染 / 拷贝进表面；预算的 GPU 项只按渲染判定，拷贝（含等 DWM）单独记录；其余判据同 r1
  - 派工单 `docs/workbuddy_m3_matrix_pacing_r3_dispatch.md`；结果进 `m3/results_matrix_pacing_r3/`；**未 APPROVED，不授权采数**

- **2026-11-06 11:15:19 AWST — M3-MATRIX-PACING r3 APPROVED（pidan）**
  - approved：`docs/m3_matrix_pacing_r3_approved.md`；gate r3 `90f9356b…9476` 不变；采数=是（同拓扑，3 会话，`-Pacing`）；派工单 `docs/workbuddy_m3_matrix_pacing_r3_dispatch.md`；结果进 `m3/results_matrix_pacing_r3/`

- **2026-11-06 — M3-MATRIX-PACING r3：Claude 复核 → 节奏完美，预算 FAIL；r2 复核中的原因推断不成立**
  - 75 Hz / 180 Hz 两列偏差 P95 0.06–0.11 ms，0 超长；拷贝进表面只要 6–8 µs
  - 180 Hz 屏上多出的约一帧 GPU 时间在“Rive 渲染”段（1.05–1.08·T），不是拷贝；75 Hz 屏同样工作只要 0.018·T
  - 推断：垂直同步时刻提交的工作排在 DWM 合成（可能含等待翻转）后面；未证实，需 GPUView / PIX
  - 复核：`docs/m3_matrix_pacing_r3_result_claude_review.md`；下一步待 pidan 决定（W1–W3）

- **2026-11-06 11:27:50 AWST — M3-MATRIX-PACING r4 LOCKED（pidan：W2 按建议）**
  - LOCKED：`docs/m3_matrix_pacing_gate_r4_LOCKED.md`，GATE_BODY_SHA256=`81fc74ebba270feb818646cd5f5c8c90fd2d3812c3447c642ed126413108934c`（复算 MATCH）；代码 `69094ee`
  - 判据同 r3；唯一改动：垂直同步后再等 0.3·T 才画（动画时间与帧间隔仍按垂直同步）；若 180 Hz 屏渲染 GPU 仍约 1·T 则转 W1
  - 派工单 `docs/workbuddy_m3_matrix_pacing_r4_dispatch.md`；结果进 `m3/results_matrix_pacing_r4/`；**未 APPROVED，不授权采数**

- **2026-11-06 — 再次恢复本 brief（Claude）**：协调提交 `57d53d4` 又整体改写了 brief（−310 行）。已按 `57d53d4~1` 恢复，协调新增的条目原样保留在下面。

- **2026-11-06 11:10:00 AWST — M3-MATRIX-PACING-R3 DRAFT 已落（纯文档；NOT LOCKED；不授权真机；Australia/Perth（AWST，UTC+8））**
  - DRAFT：`docs/m3_matrix_pacing_r3_gate_DRAFT.md`（sha256=`d0155830dded6833f4cde5623a01429d81dadbe656925429e215c99563e8838f`；APPROVED_* 空；**勿擅自 LOCK**）
  - 依据：`docs/m3_matrix_pacing_r2_next.md` P1；消化 r2 `pacing_ok=0`（budget_ok=1 旁证；禁回改 r1/r2/MATRIX）
  - BUDGET DRAFT（`docs/m3_matrix_budget_gate_DRAFT.md`）**挂账/对照**；本票专吃 pacing
  - 建议 dir=`m3/results_matrix_pacing_r3/`；标签建议 `RIVE-MATRIX-PACING-*`；票名暂用 R3（开放 VBLANK 等）
  - 开放问题：票名/BUDGET关系/门槛vs r2 p95/探针source/复测/budget旁证/FAIL照交/标签/doc-only — 供 pidan 拍板
  - composition.h 未触；Success≠adopt；不合 main；**PERF-CI 仍 HOLD**；**不暗示可采数**

- **2026-11-06 11:26:00 AWST — M3-MATRIX-PACING r3 CLOSED RIVE-MATRIX-PACING-FAIL（纯文档结题；不采用；不合 main；Australia/Perth（AWST，UTC+8））**
  - result：`docs/m3_matrix_pacing_r3_result.md`；pack/brief/next/mainline 已落；next 主推 **RENDER-BUDGET / GPU-RENDER 新 DRAFT**（或重排 `docs/m3_matrix_budget_gate_DRAFT.md`）；pacing 残差可挂账；**勿擅自 LOCK**
  - gate MATCH `90f9356bf2739ea2a95a56619fc73af561b0c34e4ebd097be05442f8c5719476` @ 2026-11-06 11:14:15 AWST；approved pidan@2026-11-06 11:15:19 AWST；tip `82eacb0b59e1bab7ee809b4b62c8a100545b4073`；results `39cdcf42e7758555b223c8058cc8a2e9e3793aff`；dir=`m3/results_matrix_pacing_r3/`；push_exit=0
  - **budget_ok=0**；pacing_ok=1；hitch/size/stability=1；probe v3；pacing_source=vblank；pacing_p95_ms primary=0.060/0.065/0.064；external=0.111/0.107/0.114
  - gpu_render_p95_frac_T(180Hz seg1/3)≈1.05–1.08（超 0.35·T）；gpu_copy_p95_ms≈0.008（可忽略）；代码 878892d（只等 WaitForVBlank；GPU 拆分 render/copy；预算按 render）
  - 对照：r1 pacing✓/budget✗；r2 pacing✗/budget✓；r3 pacing✓/budget✗（失败在 render 非 copy）；**三票均 FAIL；禁合并洗票；禁回改 r1/r2/MATRIX**
  - composition.h PRE=POST `33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97`（LF `a55d4952…e418`）未触；Success≠adopt；**PERF-CI 仍 HOLD**；不授权新采数/真机；FAIL 照交未重跑

- **2026-11-06 11:36:53 AWST — M3-MATRIX-PACING r4 APPROVED（pidan）**
  - approved：`docs/m3_matrix_pacing_r4_approved.md`；gate r4 `81fc74eb…934c` 不变；采数=是（同拓扑，3 会话，`-Pacing`）；派工单 `docs/workbuddy_m3_matrix_pacing_r4_dispatch.md`；结果进 `m3/results_matrix_pacing_r4/`

- **2026-11-06 — 再次恢复本 brief（Claude）**：协调提交 `907da1e` 又整体改写了 brief（−316 行）。已按 `907da1e~1` 恢复，协调新增的条目原样保留在下面。

- **2026-11-06 11:30:10 AWST — M3-MATRIX-RENDER-BUDGET DRAFT 已落（纯文档；NOT LOCKED；不授权真机；Australia/Perth（AWST，UTC+8））**
  - DRAFT：`docs/m3_matrix_render_budget_gate_DRAFT.md`（sha256=`e10bf9e5bf2944f13faec600a2db1de914d214a55025714ecc921fb9550b5dd1`；APPROVED_* 空；**勿擅自 LOCK**；LOCK≠采数；不派 workbuddy）
  - 依据：`docs/m3_matrix_pacing_r3_next.md` 主推 A；消化 r3 CLOSED `RIVE-MATRIX-PACING-FAIL` 残差 `budget_ok=0` / **gpu_render_p95_frac_T@180Hz≈1.05–1.08**（gpu_copy≈0.008；pacing_ok=1 旁证；禁回改 CLOSED MATRIX/r1/r2/r3；禁合并洗票）
  - 既有 `docs/m3_matrix_budget_gate_DRAFT.md` **挂账/对照或待 SUPERSEDED**；本票专吃 **render 预算残差**；pacing 相对 r2 可挂账闭合（只读旁证）
  - 建议 dir=`m3/results_matrix_render_budget/`（勿覆写 r1/r2/r3/matrix/budget）；标签建议 `RIVE-MATRIX-RENDER-*`；票名暂用 RENDER-BUDGET（开放 GPU-RENDER）
  - 开放问题：票名/BUDGET关系/0.35·T硬vs调整/只计render/探针v3+vblank/复测拓扑dir/pacing旁证/FAIL照交/标签/doc-only/APPROVED_*空 — 供 pidan 拍板
  - composition.h 未触；Success≠adopt；不合 main；**PERF-CI 仍 HOLD**；**本行不授权真机/采数**；**不暗示可采数**

- **2026-11-06 11:58:00 AWST — M3-MATRIX-PACING r4 CLOSED RIVE-MATRIX-PACING-PASS（纯文档结题；Success≠adopt；不合 main；Australia/Perth（AWST，UTC+8））**
  - result：`docs/m3_matrix_pacing_r4_result.md`；pack/brief/next/mainline 已落；next 主推 **归档 r4 PASS**；**挂账/SUPERSEDED 候选** RENDER-BUDGET + BUDGET DRAFT；有条件采用 WP（offset=0.3·T）须 pidan；**勿擅自 LOCK**；不授权新采数
  - gate MATCH `81fc74ebba270feb818646cd5f5c8c90fd2d3812c3447c642ed126413108934c` @ 2026-11-06 11:27:50 AWST；approved pidan@2026-11-06 11:36:53 AWST（`docs/m3_matrix_pacing_r4_approved.md`）；tip `e94600332307e4482cf0ff91d4e16d3b60deefc5`；results `6b7a290864ef58ff8b908f8d46532e449c8e4530`；dir=`m3/results_matrix_pacing_r4/`
  - **全判据绿**：pacing_ok=1；hitch=1；size=1；budget_ok=1；stability=1；sessions=3×RECORDED；scale_changes=4；probe v4；pacing_source=vblank；pacing_offset_fraction=0.30
  - pacing_p95_ms primary≈13.38–13.39；external≈5.63–5.69；offset_last_ms @75Hz≈4.06–4.52 / @180Hz≈1.79–2.19
  - **gpu_render_p95_frac_T@180Hz≈0.040**（远低于 0.35·T；对照 r3≈1.05–1.08）；代码 **69094ee**（vblank 后再等 0.3·T）
  - 对照：r1 pacing✓/budget✗；r2 pacing✗/budget✓；r3 pacing✓/budget✗；r4 **全绿**（分立 tip；**禁洗票；禁回改 r1/r2/r3/MATRIX**）
  - composition.h PRE=POST `33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97`（LF `a55d4952…e418`）未触；Success≠adopt；**PERF-CI 仍 HOLD**；不授权新采数/真机

- **2026-11-06 — M3-MATRIX-PACING r4：Claude 复核 → PASS（全部判据 3/3）**
  - 两列偏差 P95 < 0.2 ms；180 Hz 屏 Rive 渲染 GPU 0.040·T（r3 约 1.06·T）；CPU ≤ 0.122·T；跨屏 ≤ 13.5 ms
  - 做法：按窗口所在屏的垂直同步出帧，垂直同步后约 0.3·T 再提交；建议写进 WP1 产品化作为多屏默认节奏（是否默认开启由 pidan 决定）
  - 复核：`docs/m3_matrix_pacing_r4_result_claude_review.md`

- **2026-11-06 — Y1–Y4 按建议（pidan）**
  - Y1：WP1 Rive 宿主产品化要求已采用：`docs/m3_wp1_rive_host_requirements.md`（H6：默认按窗口所在屏的垂直同步出帧，垂直同步后约 0.3·T 再提交）
  - Y2：PR #7 已合入 main（`db5de87`）；MATRIX / PACING 另开 PR
  - Y3：M3 Rive 层在现有硬件上完成，推迟项与下一步见 `docs/m3_status.md`；WP4 启动草案 `docs/m3_wp4_pilot_DRAFT.md`（Z1 需要 pidan 提名候选应用）
  - Y4：协作约定 `docs/COORDINATION.md`（先同步再写；brief 只追加；先落 approved 再采数）

- **2026-11-06 — Wave A：Z4 ETW + L1 smoke DRAFT（Claude）**
  - `docs/z4_etw_l1_smoke_gate_DRAFT.md`：L1 冻结名字经 C ABI 在真机端到端冒烟（序列、连续性、减少动效、空闲、线程、ETW 六条）；E1–E4 待裁决；不是 PERF-CI
  - 我在 `claude/m3-wp4-api` 上的 API v0 草案由协调的 Z2 契约取代，仅作参考

- **2026-11-06 — pidan 意见落实（Claude）**
  - 删除个人邮箱：`docs/collab_roster.md`、`claude_docs_diff.md`、`claude_explore.md`、`api_surface_m2_alpha_contract_DRAFT.md`（本分支最新版本；main 和旧分支、git 历史中仍有）
  - Z2 契约小修订（implemented vs missing 附录）；**待 pidan「冻结 API v0 L1」**，冻结后仍须另票编码
  - Z4 DRAFT 写死四条 INVALID（未 LOCK/APPROVED 采数、改 composition.h、影子 IV / 未声明 tip、把 PERF-CI HOLD 当绿）和边界句
  - 后端路径：C-ABI 先通（Z3a）；真实合成器后端另排
  - r4 有条件采用（实验分支 + 产品化模块默认开启；不进 L1 默认；不合 main）；两份预算 DRAFT SUPERSEDED：`docs/m3_matrix_pacing_r4_adoption.md`
  - Z1：待 pidan 填 B 的真实应用名

- **2026-11-06 16:32:43 AWST — API v0 L1 冻结（pidan「冻结 API v0 L1」）**
  - LOCKED：`docs/api_surface_m2_alpha_contract_LOCKED.md`，GATE_BODY_SHA256=`77416e8bbc700238b42a940a2f93d4466edec1ebd79352b007f86c267a663a0f`（复算 MATCH）；冻结面 = MotionSystem / AnimationHandle / MotionTokens
  - 规则 `docs/api_v0_frozen.md`（试点期只许新增；改名 / 删除 / 改签名或语义走闸门）；变更日志 `docs/api_changelog.md`
  - 冻结 ≠ 合 main ≠ adopt；**编码仍须另票「开 API v0」**（在 Z3a 彩排之后）；Z4 采数仍须 LOCK + APPROVED

- **2026-11-06 — Wave B：Z3a 彩排派工单（Claude）**
  - `docs/workbuddy_z3a_rehearsal_dispatch.md`：K93N3P8 上用现有 C ABI 桥和 M2-02 探针跑 Play → Retarget → Cancel 与 ETW 6 事件；**彩排不采数、不提交**，runner 会覆盖入库的 `artifacts/m2_02_*`，须先备份后还原；只回一行
  - 彩排通过后，由 pidan 开“开 API v0”另票

- **2026-11-10 22:35 AWST — pidan 书面「确认」五条裁定（再确认 2026-11-06 已落口径；协调落盘）**
  - L1 合同冻结仍有效（LOCKED `77416e8b…3a0f`）；冻结 ≠ 合 main ≠ adopt；编码仍须另票
  - Z4 DRAFT 路径与四条 INVALID 再确认；**仍 NOT LOCKED**；E1–E4 仍待裁决；L1 smoke ≠ PERF-CI
  - 后端：C-interface / Z3a 先通；真实 WinRT Composition 后端仍为诚实非目标
  - r4 条件采用 WP（`69094ee` / 0.3·T；实验分支 + 产品化模块默认开启；不合 main；非 API v0 默认）
  - 两份预算 DRAFT 正式 SUPERSEDED 并入仓加横幅（文首前 sha `49bbc975…` / `e10bf9e5…`）；勿再 LOCK / 采数；PERF-CI 仍 HOLD
  - Z1：B/C 真名仍待 pidan；A=MotionSoftDemo 只作内部狗粮（33/50），不当试点赢家
  - 主文档：`docs/Z_SERIES_CONVERGE.md`；采用记录：`docs/m3_matrix_pacing_r4_adoption.md`

- **2026-11-12 — Z3a + Z4 r1 并入 `claude/m3-matrix`（Claude；pidan「先合并，1和2放到下一步」）**
  - `claude/z3a-cabi-l1` 已 merge（no-ff）进 `claude/m3-matrix`：Z3a C-ABI L1 实现（tip `fe59c622`）、Z4 r1 结果（`z4/results_etw_l1_smoke_r1/`）、结题 CLOSED `Z4-ETW-L1-SMOKE-PASS`、Claude 复核 `docs/z3a_z4_r1_claude_review.md`
  - 合并后在 `claude/m3-matrix` 上复算：闸门 `acb89d1b…ab14`、合同 `77416e8b…3a0f` MATCH；composition.h `a55d4952…e418` 未改
  - **未合 main**；Success≠adopt；PERF-CI HOLD
  - 下一步（待 pidan 开）：(1) workbuddy 补交 K93 上 Z3a 验收记录（`fm_capi_test` + devsmoke，dev-only，放 `z3a/results_dev/`）；(2) Z1 试点编码票（FluentHub 先行），引用 Z4 时须写明 PresentStats 为占位值
