# M2-Δv′ 结题中文包（notion_m2_delta_v_prime_pack.md）

> DECISION_AT=2026-10-26 10:50:13 Asia/Taipei  
> 权威结题：`docs/m2_delta_v_prime_result.md`  
> 闸门 LOCKED **勿改 hash**：`docs/m2_delta_v_prime_gate_LOCKED.md` GATE_BODY_SHA256=`06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c` @ 2026-10-26 09:05:37 AWST  
> APPROVED：pidan@2026-10-26 09:09:19 AWST  
> 真机：DESKTOP-K93N3P8（1 物理机；60 Hz）  
> 前序 M2-Δv `d2349ceb…09c6`：**未重跑**

## 结论

- **票级 = CLOSED FAIL**（非 INCONCLUSIVE；P1/P2 生效后可分胜负）。
- G4：`M2-Δv′ | gate=06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c @ 2026-10-26 09:05:37 AWST | approved=pidan@2026-10-26 09:09:19 AWST | sessions=3/3 FAIL | cells=t060 judicable FAIL (t300 mostly SKIP) | ctrl=baseP/IVel0F | ticket=FAIL | t156=rule-SKIP | P1=|a|σ≤thr;P2=thr≥Nv_h | 成功≠采用；不改composition.h；禁影子IV；≠PERF-CI；前序d2349ceb不改`
- **成功 ≠ 采用**；不解冻 / 不升级 B_v；不重开 S1 / R-3 / R-2；`composition.h` PRE=POST `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` **未改**；闸门 body/hash **未改**。
- **不重跑本 FAIL 闸门翻盘**；残差另开修 seed 票（见下一票）。
- **本票 ≠ PERF-CI**；PERF-CI 仍 HOLD `5b007dd4…282a`。

## FAIL 归因（写死）

| 层 | 事实 | 含义 |
|---|---|---|
| 闸门 | P1/P2 生效；t060{fwd,rev} 三会话均 judicable | 测量几何已可分胜负（相对 Δv INCONCLUSIVE 已解决） |
| 被测臂 | Arm-WGC-seed `wgc_pass` = 0–2/20（s1 2/0；s2 0/0；s3 1/0） | **绝对 Δv 臂差** → 格 FAIL → 票 FAIL（P4） |
| 正对照 | 三会话 `basePass=True` | 噪声门 / 估计器 OK；非测法假 FAIL |
| 负对照 | 三会话 IVel0 t060 fwd+rev FAIL | 对照成立；P5 下 t300* IVel0 跳过 |
| t300 | 多数 `Nv_h>thr` → SKIP；s2 t300rev 可判且 FAIL 5/20 | 辅证臂差；不单独定生死 |
| t156 | 规则 SKIP ACCEPT | 不挡 |
| 根因一句话 | H5 已锁 **速度比** seed=WGC；在绝对阈 `max(20,5%|v_ref|)` 下，**末两帧 WGC→InitialVelocity** 仍不足以过门 | **产品/seed 路径问题**，非闸门、非 composition.h、非影子 IV 违规 |

**禁止解读**：禁止把本 FAIL 归因回可判规则；禁止静默改 thr / 17/20 / 复活中位句；禁止重跑 `06467b82…805c` 翻盘。

## 证据摘要

| sess | pid | t060fwd | t060rev | basePass | IVel0 t060 both FAIL | 判定 |
|---|---|---|---|---|---|---|
| 1 | 48772 | FAIL 2/20 | FAIL 0/20 | T | T | FAIL_CANDIDATE |
| 2 | 54972 | FAIL 0/20 | FAIL 0/20 | T | T | FAIL_CANDIDATE（另 t300rev FAIL 5/20） |
| 3 | 56980 | FAIL 1/20 | FAIL 0/20 | T | T | FAIL_CANDIDATE |

产物：`artifacts/m2_delta_v_prime_*`；源码 `MotionSoftDemo/M2DeltaVPrimeProbe.cs`。

## 禁令复述

- 不重开 S1 / R-3 / R-2。
- 不解冻 / 不升级 B_v。
- 成功 ≠ 采用。
- 不改 `composition.h`（`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）。
- 禁影子 IV。
- **不改**已 LOCKED `06467b82…805c`；**不重跑本票翻盘**。
- 前序 `d2349ceb…09c6` 不改、不重跑。
- 旁路挂账不挡主线；本票也不清旁路。
- PERF-CI 不降级；本票产物 ≠ PERF-CI 证据。
- 不开 M3 真机。

## 挂账 / 残差

| 项 | 处置 |
|---|---|
| M2-Δv′（本票） | **CLOSED FAIL**；LOCKED 归档不改 |
| **M2-WGC-SEED-ABS** | **残差主票**（立刻开）——修 WGC→InitialVelocity 路径，目标服务绝对 Δv；禁影子 IV；不改 composition.h |
| M2-Δv″（绝对门复测） | **挂账**：须 seed 票 CLOSED 后再开新闸门版本；**禁止**用本 FAIL 闸门重跑翻盘 |
| M2-VEL-SMOOTH | **并入** seed 票设计空间（平滑/多帧 WGC 估速为候选手段，不另开并行真机） |
| M2-L | 继续挂账 |
| M2-REFRESH | 本轮不并行（M3/C5b） |
| M2-TOOLING | 挂账 |
| harness IVel0·t300dbl | 旁路不变 |
| M2-PERF-CI | 继续 HOLD |

## 下一票决断

**选 A = M2-WGC-SEED-ABS（修 WGC seed / InitialVelocity 路径）。**  
骨架 DRAFT：`docs/m2_wgc_seed_abs_gate_DRAFT.md`（**NOT LOCKED**；不授权采数）。详见 `docs/notion_m2_wgc_seed_abs_next.md`。

### 候选速判

| 选项 | 决断 | 一句理由 |
|---|---|---|
| **A 修 WGC seed 路径** | **采纳** | FAIL 根因在被测臂 IV；对照已证明闸门可分胜负 |
| B M2-L | 挂账 | 调查票；不挡，可随后 |
| C 多刷新率单机 | 本轮不并行 | M3/C5b |
| D VEL-SMOOTH 单独开 | **并入 A** | 平滑是 seed 手段之一，不另并行 |
| E TOOLING | 挂账 | 文档 |
| 重跑本 FAIL 闸门 | **否** | 禁翻盘；绝对复测须新版本闸门且在 seed 后 |

### 协调下一步一句

请立刻派 **Gemini**（WGC→IVel 候选路径：多帧/平滑/时戳对齐；禁影子 IV）与 **DeepSeek**（与 H5 calib=1.0 衔接、绝对 Δv 验收口径、G2 风险）补洞；Notion 收齐后可 LOCK 修订 → LOCK → 问 pidan APPROVED（单机；PERF-CI 仍 HOLD）。

— notion ai（M2-Δv′ 结题；ticket=CLOSED FAIL；下一票=M2-WGC-SEED-ABS）
