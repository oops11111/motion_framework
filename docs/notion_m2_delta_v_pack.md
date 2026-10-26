# M2-Δv 结题中文包（notion_m2_delta_v_pack.md）

> DECISION_AT=2026-10-26 08:59:30 AWST（Australia/Perth）  
> 权威结题：`docs/m2_delta_v_result.md`  
> 闸门 LOCKED **勿改 hash**：`docs/m2_delta_v_gate_LOCKED.md` GATE_BODY_SHA256=`d2349ceb0d37fdf332a4f00cb2c1f1217fe6856d0f3e5222ffaf82b8b15509c6` @ 2026-10-25 23:33:05 AWST  
> APPROVED：pidan@2026-10-26 06:21:36 AWST（`docs/m2_delta_v_approved.md`）  
> 真机：DESKTOP-K93N3P8（1 物理机；60 Hz）；Binary=MotionSoftDemo

## 结论

- **票级 = CLOSED INCONCLUSIVE**（非 PASS / 非 FAIL）。
- G4：`M2-Δv | gate=d2349ceb0d37fdf332a4f00cb2c1f1217fe6856d0f3e5222ffaf82b8b15509c6 @ 2026-10-25 23:33:05 AWST | approved=pidan@2026-10-26 06:21:36 AWST | sessions=3/3 INCONCLUSIVE | cells=0/5 judicable(all rule-SKIP) | ctrl=baseF/IVel0Fok | ticket=INCONCLUSIVE | t156=rule-SKIP | 成功≠采用；不改composition.h；禁影子IV；≠PERF-CI`
- **成功 ≠ 采用**；不解冻 / 不升级 B_v；不重开 S1 / R-3 / R-2；`composition.h` PRE=POST `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` **未改**；闸门 body/hash **未改**。
- **本票 ≠ PERF-CI**；PERF-CI 仍 HOLD `5b007dd4…282a`（须 ≥2 物理机）。
- **不重跑本 LOCKED 闸门**翻盘；残差升 **M2-Δv′**（新闸门版本）。

## INCONCLUSIVE 归因（写死）

| 层 | 事实 | 含义 |
|---|---|---|
| 会话 | 3/3 = `SESSION_INCONCLUSIVE_CANDIDATE` | 票结局只能是 INCONCLUSIVE |
| 可判核心 5 格 | 每会话全部 `Δv-SKIP`（0/5 judicable） | SKIP≠PASS；不构成 FAIL 票 |
| t060{fwd,rev} | `|a|·σ_ts > thr/2`（|a|σ≈10.5–10.9；thr/2=10；σ_ts=1/60/√12） | **timing-bound 可判失败** |
| t300{fwd,rev,dbl} | `thrMed < 2·Nv_h`（thr≈20 floor；Nv_h P95≈18–37） | **floor-bound 可判失败**（与设计预期「t300 可判」冲突） |
| t156 | 规则导出 SKIP | 辅证；不挡；ACCEPT 保留 |
| 正对照 base | `basePass=False`（部分格 base_pass<17/20 或同可判失败） | 正对照未成立 → 挡 PASS |
| 负对照 IVel=0 | t060 fwd **与** rev 三会话均 FAIL | **对照成立**（非 PASS 候选路径 OK） |
| 被测臂 WGC | 可采样本上 wgc_pass 多为 0–4/20（Δv 常 ≫ thr） | **格不可判 → 不记 Δv-FAIL**；升版后若可判，预期更接近 FAIL 而非虚 PASS |
| 探针 | 每会话在 IVel0·t300dbl 中途崩；核心格（含 t300dbl 的 base+WGC）齐 | **非票内负对照要求**；offline adj 留证；挂 harness 稳定旁路 |

**根因一句话**：绝对阈公式 KEEP `max(20, 5%|v_ref|)` 与可判规则 `thr≥2·Nv_h ∧ |a|σ≤thr/2` 在 60 Hz / 本估计器几何下**互斥**——核心格无法进入可判，票无法区分 PASS/FAIL → INCONCLUSIVE。属**闸门版本问题**，非 composition.h / 影子 IV / 采数程序违规。

## 证据摘要

| sess | appPID | offline | 核心格 | basePass | IVel0 t060 both FAIL | 判定 |
|---|---|---|---|---|---|---|
| 1 | 59588 | 1 | 5× SKIP | F | T | INCONCLUSIVE_CANDIDATE |
| 2 | 49404 | 1 | 5× SKIP | F | T | INCONCLUSIVE_CANDIDATE |
| 3 | 24020 | 1 | 5× SKIP | F | T | INCONCLUSIVE_CANDIDATE |

产物：`artifacts/m2_delta_v_*`（含 `session_matrix`、`s{1,2,3}_{raw,summary}`、`prereg`+hash、`run_sessions.ps1`、`offline_adj.ps1`、`run.log`、`composition_hash.txt`）；源码 `MotionSoftDemo/M2DeltaVProbe.cs`。

## 禁令复述

- 不重开 S1 / R-3 / R-2。
- 不解冻 / 不升级 B_v。
- 成功 ≠ 采用。
- 不改 `composition.h`（`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）。
- 禁影子 IV。
- **不改**已 LOCKED `d2349ceb…09c6` body/hash（残差另开 Δv′）。
- 旁路挂账不挡主线；本票也不清旁路。
- PERF-CI 不降级为单机合格；本票产物 ≠ PERF-CI 证据。
- 不开 M3 真机。

## 升闸门 / 挂账 / 残差

| 项 | 处置 |
|---|---|
| M2-Δv（本票） | **CLOSED INCONCLUSIVE**；LOCKED 归档不改 |
| **M2-Δv′** | **残差主票**（立刻开；见下一票决断 A）——只改可判几何 / 对照与 harness 稳定；**KEEP** 绝对阈 `max(20,5%|v_ref|)`；t156 规则导出 SKIP ACCEPT 保留 |
| M2-L | 继续挂账（不挡） |
| M2-REFRESH | 本轮仍不并行（属 M3 / C5b） |
| M2-VEL-SMOOTH | 挂账可选（估计器已二次 w=5；平滑不优先于可判修复） |
| M2-TOOLING | 挂账（文档票） |
| IVel0·t300dbl 崩 | **旁路**：harness/探针稳定；不挡 Δv′ 闸门设计 |
| M2-PERF-CI | 继续 HOLD 待第二台物理机 |

## 下一票决断

**选 A = M2-Δv′（升版绝对门闸门）。** 骨架 DRAFT：`docs/m2_delta_v_prime_gate_DRAFT.md`（**NOT LOCKED**；不授权采数）。

### 候选速判

| 选项 | 决断 | 一句理由 |
|---|---|---|
| **A M2-Δv′** | **采纳** | 绝对门仍是 R-2→M2 明文余项；本票 INCONCLUSIVE 因可判几何，不升版则门永久空转 |
| B M2-L | 挂账 | 调查票；可跟在可判门可分胜负之后 |
| C 多刷新率单机 | **本轮不并行** | 仍易与 M3/C5b/PERF-CI 纠缠 |
| D M2-VEL-SMOOTH | 挂账 | 可选；不优先于可判修复 |
| E M2-TOOLING | 挂账 | 文档票不挡质量门 |
| F 重跑本 LOCKED | **否** | 同几何必再 INCONCLUSIVE；禁静默改 LOCKED |

### 协调下一步一句

请立刻派 **Gemini**（可判规则 vs thr/Nv_h/|a|σ 几何修订提案，KEEP 阈公式）与 **DeepSeek**（G2 风险：防虚 PASS、base 正对照可达成性、IVel0 崩旁路）补洞；Notion 收齐后覆盖可 LOCK 修订 → LOCK → 问 pidan APPROVED（**单机即可**；提醒 PERF-CI 仍 HOLD）。

— notion ai（M2-Δv 结题；ticket=CLOSED INCONCLUSIVE；下一票=M2-Δv′）
