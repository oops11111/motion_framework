# M2 下一票决断（M2-WGC-SEED-PATH-B′ CLOSED SEED-PATHB-FAIL 后 · SEED-TRUTH-CONFLICT HUNG · PERF-CI HOLD）

> DECISION_AT=2026-10-26 14:32:00 Asia/Taipei  
> 依据：`docs/notion_m2_wgc_seed_path_b_prime_pack.md`；`docs/m2_wgc_seed_path_b_prime_result.md`  
> 本票 gate `a1161c77…3a43` **不改、不重跑**；前序 `09f6414b…793d` / `831818e4…80de` / `06467b82…805c` **不翻盘**  
> SEED_TRUTH_CONFLICT=**HUNG（G2-LOCK-2b）**；A′ **未跑**

## 结论

**选 A = M2-WGC-SEED-PATH-A′**（显式另票；别名可写 `M2-WGC-SEED-PATHB-APRIME`；结题标签建议 `SEED-PATHA-PRIME-*` 或 `SEED-PATHB-APRIME-*`）。  
DRAFT：`docs/m2_wgc_seed_path_a_prime_gate_DRAFT.md`（**NOT LOCKED**；不授权采数）。

**一句话为什么**：本票默认 A=V2-clamp 已跑且触发 SEED-TRUTH-CONFLICT（比过∧smoke 全灭 0/20）；闸门预登记的次选 **A′=V2-clamp+P-VS α_v=0.30 尚未跑**——在开「改真值定义/改 H5 分母」类冲突票之前，先耗尽已预登记、禁抬 thr 的平滑臂；KEEP calib exact 与 V2-clamp；Δv″ 继续挂账。

## 现状

| 项 | 状态 |
|---|---|
| M2-Δv | CLOSED INCONCLUSIVE；`d2349ceb…09c6` 不改 |
| M2-Δv′ | CLOSED FAIL；`06467b82…805c` 不改、不重跑翻盘 |
| M2-WGC-SEED-ABS | CLOSED SEED-ABS-FAIL；`831818e4…80de` 不改、不重跑翻盘 |
| M2-WGC-SEED-CALIB | CLOSED SEED-CALIB-FAIL；`09f6414b…793d` 不改、不重跑翻盘 |
| M2-WGC-SEED-PATH-B′ | **CLOSED SEED-PATHB-FAIL**；`a1161c77…3a43` 不改、不重跑翻盘；**SEED-TRUTH-CONFLICT=HUNG**；A′未跑 |
| M2-PERF-CI | HOLD；不降级 |
| M2-01 / H5 / M2-02 | CLOSED PASS |
| M3 | 不开真机 |

## 候选速判

| 选项 | 决断 | 理由 |
|---|---|---|
| **A M2-WGC-SEED-PATH-A′** | **采纳** | 默认 A 已触达 TRUTH-CONFLICT；预登记 A′（P-VS ON α_v=0.30）**未跑**；先耗尽平滑臂 |
| B 立刻 M2-SEED-TRUTH-CONFLICT | **否（本轮）** | result 无「A′ 明显无望」证据（根本未跑）；改真值/改 H5 前须先耗尽预登记臂 |
| C 再开 / 重标定 calib | **否** | 禁再标定主线；比窗已过；exact KEEP |
| D 立刻 M2-Δv″ | **否** | smoke 未齐；绝对门复测须 seed 后新闸门 |
| E 重跑 PATH-B′ / CALIB / ABS / Δv′ FAIL | **否** | 禁翻盘（含本票 `a1161c77…3a43`） |
| F PERF-CI / 开 M3 | **否** | HOLD；不开 M3 |
| G 默认同跑 A′ 偷换本 FAIL | **否** | 须显式另票并写清预登记 |

**为何不否决 A′、不选 TRUTH-CONFLICT 本轮**：result 写明 A′不同跑/挂账次选；smoke 全灭但默认 A 的 P-VS=OFF——A′ 正是闸门预登记用来抑 tip 的下一臂；无「A′ 已证无效」事实。若 A′ 结题仍比过∧smoke 双灭 → **保持** SEED-TRUTH-CONFLICT 挂账（或升主线），仍不预授权改 H5 分母。

## 可 LOCK 修订要点（给同伴 / Notion）

### KEEP（写死）

- **路径基线**：`PATHB-A′ = V2-clamp + V3-VS`（本票主路径；**显式另票**，非 PATH-B′ 默认同跑）
- **`t_int = min(t_comp, t_last_WGC)`**；**禁**外推超过 `t_last_WGC`
- **N=5**；**order=2**；**禁**本票改 N/order
- **calib exact=`0.7510829456`**（显示 0.7511）：**KEEP**；禁再标定
- **ε=5.0**；禁影子 IV；不改 `composition.h`（`a55d4952…e418`）
- **P-VS=ON；α_v=0.30**（预登记写死；`v_s = α_v·v̂ + (1−α_v)·v_s_prev`；首样 `v_s=v̂`；限幅 `v_max` **不设**）
- 写入 IV：`InitialVelocity=(float)(calib·v_s)`
- **H5 比回归仍必要**：`med_r∈[0.8,1.2]` 且无 `r_i≥2`
- 对照同：basePass + IVel0_t060_both_FAIL
- smoke=**t060 fwd+rev**；二次 w=5；`thr=max(20,0.05·|v_ref|)`；≥**17/20**；可判 P1/P2
- ≥3 独立进程；单机 DESKTOP-K93N3P8 / 60 Hz
- 回退 B = IV null+0；票 PASS 只评主路径 A′
- 结题标签：`SEED-PATHA-PRIME-PASS|FAIL|INVALID`（或 `SEED-PATHB-APRIME-*`）；**≠** `SEED-PATHB-PASS` 翻盘本票；**≠** `SEED-CALIB-*` / `SEED-ABS-*` / Δv-PASS

### 相对 PATH-B′ A 的 diff

| 参数 | PATH-B′ A（CLOSED FAIL） | PATH-A′（本下一票） |
|---|---|---|
| 族 | P-LS+TS + V2-clamp | **同** |
| N / order / calib / ε / t_int | KEEP | **KEEP** |
| P-VS | **OFF** | **ON · α_v=0.30** |
| 测量 / thr / 17/20 | KEEP | **KEEP（禁改）** |
| 结题标签 | SEED-PATHB-* | **SEED-PATHA-PRIME-***（或 SEED-PATHB-APRIME-*） |

### 否决虚 PASS

| 反例 | 否决 |
|---|---|
| 抬 thr / 降 17/20 | 否决 |
| 偷换估窗（打断后帧、异时基） | INVALID |
| 改 composition.h | INVALID |
| 影子 IV / spring.h→IV | INVALID |
| 重跑/改写 `a1161c77…3a43` / `09f6414b…793d` / `831818e4…80de` / `06467b82…805c` | INVALID |
| 产物标 Δv-PASS / SEED-ABS-PASS / SEED-CALIB-PASS / SEED-PATHB-PASS 翻盘 / PERF-CI 合格证据 | INVALID |
| 只修 fwd（或只修 rev）却放宽另一侧门槛 | 否决 |
| 无 base / 无 IVel0 双 FAIL 却宣称 PASS | INVALID |
| 执行中改未预登记的 N/order/ε/calib/α_v；关 P-VS 偷换回 A | INVALID |
| 默认同跑偷换 PATH-B′ FAIL | INVALID |
| 本票内预授权改 H5 分母 | 否决（不预授权） |

### 若本票仍比过 ∧ smoke 双灭

- **保持** `SEED-TRUTH-CONFLICT` 挂账（或升主线诊断/决议票）
- **仍不**预授权改 H5 分母 / 抬 thr / 降 17/20
- 结构风险下 FAIL 继续合法

### Δv″ / 其他挂账

- **Δv″：继续挂账**——PATH-A′（或后续）可过比回归+smoke 后再开新闸门绝对门；**不立刻开 Δv″**
- PERF-CI 仍 HOLD；不开 M3
- 旁路 `side_parked_hang_degrade.md` 不挡、本票也不清旁路
- SEED-TRUTH-CONFLICT 挂账保持，直至显式决议票

## DRAFT 路径

`docs/m2_wgc_seed_path_a_prime_gate_DRAFT.md`（已落盘骨架；**NOT LOCKED**）。

## 协调下一步

派 DeepSeek（核对预登记 α_v=0.30 / KEEP 表 / 虚 PASS / G2-LOCK-2b 延续）→ Notion 可 LOCK → LOCK → pidan APPROVED（单机）。  
**禁**在 LOCK+APPROVED 前采数。  
**禁**把本票当 PATH-B′ 翻盘或默认同跑第二臂。

## 禁令

不重开 S1/R-3/R-2；不改 composition.h（`a55d4952…e418`）；不解冻 Bv；禁影子 IV；成功≠采用≠Δv-PASS；**不改/不重跑** `a1161c77…3a43` / `09f6414b…793d` / `831818e4…80de` / `06467b82…805c`；结题只用 `SEED-PATHA-PRIME-*`（或 `SEED-PATHB-APRIME-*`）；禁标 Δv-PASS / SEED-ABS-PASS / SEED-CALIB-PASS / 本票 PATH-B′ PASS 翻盘；PERF-CI 不降级；不开 M3；禁抬 thr / 降 17/20 / 偷换估窗 / 改 H5 分母伪装 PASS；禁再标定主线；禁立刻 Δv″；结构风险下 FAIL 合法；SEED-TRUTH-CONFLICT 保持挂账。

— notion ai（下一票=M2-WGC-SEED-PATH-A′）
