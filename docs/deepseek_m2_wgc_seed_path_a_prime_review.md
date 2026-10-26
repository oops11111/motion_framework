# DeepSeek · M2-WGC-SEED-PATH-A′ 可 LOCK 补洞

- 时间：2026-10-26（Asia/Taipei, UTC+8）
- 状态：**文档 only**；NOT LOCKED；**禁采数**
- **出处说明**：**Harness 空回代拟**；对照 `docs/m2_wgc_seed_path_a_prime_gate_DRAFT.md`、`docs/notion_m2_wgc_seed_path_b_prime_pack.md`、`docs/notion_m2_wgc_seed_path_b_prime_next.md`、`docs/m2_wgc_seed_path_b_prime_result.md`、`docs/deepseek_m2_wgc_seed_path_b_prime_design.md` §2.2 独立核对。
- composition.h sha 锁：`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（未改、不改）

## 禁令复述（文首）

不重开 S1/R-3/R-2；不改 composition.h；不解冻 Bv；禁影子 IV；禁翻盘 `a1161c77…3a43` / `09f6414b…793d` / `831818e4…80de` / `06467b82…805c`；禁立刻 Δv″；禁 LOCK 前采数；≠PERF-CI；成功≠采用；禁标 `Δv-PASS` / `SEED-ABS-PASS` / `SEED-CALIB-PASS` / `SEED-PATHB-PASS`；结题只用 **`SEED-PATHA-PRIME-*`**（勿用 `SEED-PATHB-*` 翻盘本票）；KEEP calib；禁再标定；禁改 H5 分母预授权。

---

## 1) 核对预登记（主路径 A′）

| 项 | 须写死 | 核对 |
|---|---|---|
| 变体 ID | **`PATHB-A′ = V2-clamp + V3-VS`** | ✅ 与 DRAFT G1 / 前序 design §2.2 一致 |
| 估速 | 打断前末 N=5 · 二次 LS · `v̂=b+2c·t_int` | ✅ |
| `t_int` | `min(t_compositor_interrupt_QPC, t_last_WGC)`；**禁**外推过 `t_last_WGC`；禁打断后帧；同 QPC | ✅ |
| **P-VS** | **ON**（本票主路径必须开；关 = 偷换回 A → INVALID） | ✅ |
| **α_v** | **`0.30` 写死**；`v_s = α_v·v̂ + (1−α_v)·v_s_prev` | ✅ |
| 首样 | **`v_s = v̂`** | ✅ |
| 限幅 `v_max` | **不设** | ✅ |
| 写入 IV | **`InitialVelocity = (float)(calib · v_s)`** | ✅ |
| **ε 判据** | **`|v̂|<ε` → 回退 B，判在 VS 之前**（对平滑前的 `v̂`，不是对 `v_s`） | ✅ DRAFT 已写「VS 前判」；LOCK 须保持此句 |
| 回退 B | IV=null+0；票 PASS **只评 A′** | ✅ |
| 显式另票 | **禁止**默认同跑偷换 PATH-B′ FAIL | ✅ |

**prereg 建议必含字段**：`pathb_variant=PATHB-A′`、`P-VS=ON`、`α_v=0.30`、`calib_exact=0.7510829456`、`t_int` 钳制规则、`eps_before_vs=true`。

---

## 2) KEEP 表确认

| 参数 | 值 | 本票 |
|---|---|---|
| N | **5** | **KEEP**；禁改 |
| order | **2** | **KEEP**；禁改 |
| calib | exact=`**0.7510829456**`（显示 0.7511） | **KEEP**；**禁再标定**；执行中改 = INVALID |
| ε | **5.0** DIP/s | **KEEP**；VS **前**判 |
| `t_int` | `min(t_comp, t_last_WGC)` | **KEEP**（继承 PATH-B′ A） |
| thr | `max(20, 0.05·\|v_ref\|)` | **KEEP**；禁抬 |
| 格 PASS | ≥**17/20** | **KEEP**；禁放宽 |
| 测量窗 | 二次 LS **w=5** | **KEEP** |
| H5 分母 | **2F `v_screen`**（末两帧 WGC） | **KEEP**；**禁**本票预授权改分母 |
| H5 窗 | `med_r∈[0.8,1.2]` ∧ 无 `r_i≥2` | **KEEP**；必要，不得 smoke 豁免 |
| 对照 | basePass + IVel0_t060_both_FAIL | KEEP |
| composition.h | `a55d4952…e418` | 不改 |
| 机器 | DESKTOP-K93N3P8 · 60 Hz · ≥3 会话 | KEEP |

---

## 3) 相对 PATH-B′ A 的 diff（仅 P-VS）

| 参数 | PATH-B′ A（CLOSED `SEED-PATHB-FAIL`） | **PATH-A′（本票）** |
|---|---|---|
| 族 | P-LS+TS + V2-clamp | **同** |
| N / order / calib / ε / t_int | 5 / 2 / exact 0.7510829456 / 5.0 / clamp | **全部 KEEP** |
| **P-VS** | **OFF** | **ON** |
| **α_v** | — | **0.30** |
| IV 公式 | `(float)(calib·v̂)` | **`(float)(calib·v_s)`** |
| 测量 / thr / 17/20 / H5@2F | KEEP | **KEEP** |
| 结题标签 | `SEED-PATHB-*` | **`SEED-PATHA-PRIME-*`** |
| 前序闸门 | — | **不翻盘** `a1161c77…3a43` 等四门 |

**唯一实质性 diff = 启用 P-VS（α_v=0.30）及 IV 改用 `v_s`。** 无其它估窗/尺度变更。

---

## 4) 虚 PASS / INVALID 否决表

| 行为 | 处置 |
|---|---|
| 抬 thr / 降 ≥17/20 | 否决 / INVALID |
| 偷换估窗（打断后帧、异时基、外推过 `t_last_WGC`） | INVALID |
| **关 P-VS / 偷换回 PATHB-A** | **INVALID** |
| **执行中改 α_v（≠0.30）** / 改 N·order·ε·calib | INVALID |
| 改 H5 分母（本票预授权） | 否决 / INVALID |
| 影子 IV / spring.h → compositor IV | INVALID |
| 改 composition.h | INVALID |
| 重跑/宣称通过四闸门 `a1161c77…3a43` / `09f6414b…793d` / `831818e4…80de` / `06467b82…805c` | INVALID |
| 产物标 `Δv-PASS` / `SEED-ABS-PASS` / `SEED-CALIB-PASS` / **`SEED-PATHB-PASS`** | INVALID |
| 只修一侧却放宽另一侧门槛；V4 单侧默认 | 否决 / INVALID |
| 无 base / 无 IVel0 双 FAIL 却宣称 PASS | INVALID |
| offline 当结题；跨会话池化；FAIL 后补测拼凑；默认同跑偷换 PATH-B′ | INVALID |
| 产物作 PERF-CI；开 M3；解冻 Bv；立刻 Δv″ | INVALID |

---

## 5) G2-LOCK-2b 延续 + 结构风险

- PATH-B′ 真机：H5 比过（med_r≈0.81–0.95）∧ t060 smoke **0/20**（fwd×3、rev 灭/SKIP）→ **`SEED-TRUTH-CONFLICT=HUNG`**；`|ivel−v_pre|≈50+` 仍在。  
- A′ **未跑**；本票正是耗尽预登记平滑臂。  
- **若本票仍比过 ∧ smoke 双灭** → 结局 = **`SEED-PATHA-PRIME-FAIL`**；**保持** `SEED-TRUTH-CONFLICT` 挂账（或升主线）；**仍不**预授权改 H5 分母；**禁**抬 thr / 降 17/20 / 再标定主线；结构风险下 **FAIL 合法**。  
- Δv″：**继续挂账**；仅当比过 ∧ t060 fwd+rev 格 PASS 后另开；**不立刻开**。  
- **比窗裕度提醒（非挡 LOCK）**：PATH-B′ s2/s3 med_r≈0.805–0.806，已贴下沿；α_v=0.30 可能进一步欠 seed → 出窗则本票 FAIL/看 INVALID 条款，**不得**因此改 calib 或改分母救票。

---

## 6) 可 LOCK？

### **是 — 可 LOCK**

DRAFT 已写死：主路径 A′、α_v=0.30、KEEP 全表、ε 在 VS 前判、四闸门不翻盘、G2-LOCK-2b 延续、虚 PASS、结构风险。与前序 design §2.2 对齐；**无硬缺口挡 LOCK**。

### 最小修订点（建议 Notion LOCK 时一并粘贴；非「不可 LOCK」）

1. **结题标签主用 `SEED-PATHA-PRIME-*`**；若保留别名 `SEED-PATHB-APRIME-*`，正文须再写一句「**禁止**写作 `SEED-PATHB-PASS`」。  
2. prereg / 每会话产物显式字段：`α_v`、`v_s`（或等价）、`eps_before_vs=true`、`pathb_variant=PATHB-A′`。  
3. 文首复述：PATH-B′ med_r 贴下沿 → A′ 出窗风险；出窗≠可改 calib/H5。

---

## 7) 结题标签

- **推荐**：`SEED-PATHA-PRIME-PASS` / `SEED-PATHA-PRIME-FAIL` / `SEED-PATHA-PRIME-INVALID`  
- **禁止**：`SEED-PATHB-PASS`（翻盘 PATH-B′）；`SEED-CALIB-*` / `SEED-ABS-*` / `Δv-PASS`  
- 产物前缀：`m2_wgc_seed_path_a_prime_*`（或 `m2_wgc_seed_pathb_aprime_*`）；**不得**冒充 `m2_wgc_seed_path_b_prime_*` 翻盘包

---

## 明确声明

成功 ≠ 采用 ≠ Δv-PASS；不改 composition.h；禁影子 IV；≠PERF-CI（HOLD）；不开 M3；Δv″ 与 SEED-TRUTH-CONFLICT 继续挂账；LOCK+APPROVED 前禁采数。

— DeepSeek网页版（M2-WGC-SEED-PATH-A′ 可 LOCK 补洞；Harness 空回代拟；文档 only）
