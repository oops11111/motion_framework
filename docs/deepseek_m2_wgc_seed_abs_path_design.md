# DeepSeek · M2-WGC-SEED-ABS 路径设计（WGC→IVel｜顶替 Gemini）

- 时间：2026-10-26（Asia/Taipei, UTC+8）
- 状态：**文档 only**；NOT LOCKED；不授权采数
- **出处说明**：pidan 决定 Gemini 地区限制由本 bot 补路径设计。Harness 未通；按 `/workspace/gemini_m2_wgc_seed_abs_prompt.md`、`docs/m2_wgc_seed_abs_gate_DRAFT.md`、`docs/deepseek_m2_wgc_seed_abs_review.md`、`docs/m2_delta_v_prime_result.md` 起草。验收/H5/风险仍以 **review** 为准；本文专责 **路径选型**。
- 交叉引用：`docs/deepseek_m2_wgc_seed_abs_review.md`

---

## 0) 问题一句 + 真机锚点

H5 A（**末两帧**差分 × calib=1.0）令 `med_r≡1.0` PASS，但 Δv′ 在同一 WGC→IVel 下 t060 `wgc_pass`=0–2/20 → **CLOSED FAIL**。正对照 base PASS、负对照 IVel0 FAIL ⇒ 测法/阈可分胜负；残差在 **seed 估法相对绝对连续测量过粗**，非 composition.h、非影子 IV、非 thr 公式。

数字（DESKTOP-K93N3P8·60Hz·Δv′ s1–s3；不另编）：s1 2/0；s2 0/0；s3 1/0（fwd/rev）；thr=`max(20,0.05·|v_ref|)`；测量侧二次 LS **w=5**。

---

## 1) 候选路径清单

凡路径均须：**输入 = WGC/屏一致位置序列**（Translation.X + QPC）；**输出 = compositor InitialVelocity（DIP/s）**；**calib 默认 1.0**。下表「预期方向」为机制推理，**非**未测数字。

| ID | 怎么估 v | 预登记参数 | 优 | 缺 | 相对 P-2F 基线 |
|---|---|---|---|---|---|
| **P-2F**（基线） | 打断前末两帧 `(x2−x1)/(t2−t1)` | 无（H5 已锁） | 实现最简；同窗比≡1 | 高频量化/抖动直接进 IV；与 Δv′ 二次 w=5 **几何不一致** | 基线；绝对已 FAIL |
| **P-MF** | 末 N 帧一阶差分再滑动平均，或对逐帧差分做 EMA | `N∈{3,5,7}` 或 `α∈(0,1]`；窗口「打断前」 | 抑抖；实现轻 | 相位滞后；N/α 过大 → 欠 seed，威胁 H5 比窗 | 预期降 IV 噪声；滞后风险须比回归把关 |
| **P-LS** | 打断前末 N 帧位置对时间 **一次或二次 LS**，在打断瞬间 `t_int` 取 `v̂(t_int)`（一次：斜率；二次：`b+2c·t_int`） | `N`（建议 5）；`order∈{1,2}`；`t_int` 定义（见 P-TS） | 与 Δv′ **测量侧二次 w=5** 同族；外推到同一瞬间，机制上最对齐「绝对连续」判据 | 实现重于 2F；N 不足时二次不稳；须防用测量窗冒充 seed 窗却偷偷吃 spring 状态 | **优先推荐族**；预期改善绝对 Δv，比须回归 |
| **P-TS** | 不改估速公式，只把 WGC 样本时戳与 compositor 打断时刻对齐（插值/外推到同一 `t_int`） | `t_int` 源（compositor 回调 QPC / 探针标记）；是否线性插值位置后再求导 | 修接线滞后；可与任一估速叠加 | 单独不够（2F+TS 仍可能抖） | **必选叠加层**，非独立银弹 |
| **P-VS**（VEL-SMOOTH 并入） | 先得 `v_raw`（2F 或 LS），再对速度做 EMA/`min(·)` 限幅后 seed | `α_v` 或限幅 `v_max`（若用）；须预登记 | Notion 已并入本票；抑尖峰 | 限幅易欠 seed；另开并行真机 **禁止**（已并入） | 作 P-LS/P-MF **可选后处理**，默认关 |
| **P-LS+TS**（组合） | P-LS 在 P-TS 对齐的 `t_int` 上取值 | 同 P-LS + P-TS | 估法+接线双修 | 参数面略大 | **推荐默认**（见 §3） |
| ~~P-SH~~ | spring.h / `mf_spring_at` 闭式 v | — | — | 影子 IV | **否决**（§2） |

**不采纳为默认的变体（可挂账）**：自适应 N、按 `|a|` 切换阶数——LOCK 前参数面过大；本票先固定 N/order。

---

## 2) 否决影子路径

- **否决**：`mf_spring_at` / spring.h 闭式 `v(t)` → compositor `InitialVelocity`。  
  **理由**：H5 已证明该路径制造过加速（#37 shadow med 比 2.600）；本票修的是 **屏一致源上的估法**，不得借「绝对连续」复活影子 IV。`shadow_v_log` 仅 log-only。
- **否决**：任何「对齐 compositor 弹簧数学到 spring.h」以取得 IV 的来源（含控制器契约把影子 v 写进 WinRT IV）。
- **否决**：改 `composition.h` 以「修好」seed（sha 锁 `a55d4952…e418`）。

---

## 3) 推荐默认路径 + 回退（继承 H5 A→B）

### 3.1 拍板

| 级 | 路径 | 条件 |
|---|---|---|
| **主路径 A（默认）** | **P-LS+TS**：打断前末 **N=5** 帧 WGC `(t_k,x_k)`，**二次** LS，在 compositor 打断时刻 **`t_int`（QPC）** 取 `v̂=b+2c·t_int`；`InitialVelocity = (float)(calib · v̂)`，**calib=1.0** | 有效帧数 ≥5；拟合有限；`|v̂|≥ε`；时戳单调 |
| **回退 B** | `IV=null + InitialVelocity=0`（path=B） | A 失败：帧不足 / NaN / 非单调 / `|v̂|<ε` / 取 WGC 失败 |
| **票 PASS** | **只评 A** | 全程 B 的会话不计 PASS（对照可登记） |

**预登记参数（LOCK 写死；执行中改 = INVALID）**

| 参数 | 值 |
|---|---|
| `N` | **5**（与 Δv′ 测量 w=5 同宽，便于机制对齐；**seed 窗 ≠ 改测量估计器**） |
| `order` | **2**（二次） |
| `t_int` | 探针/compositor 记录的打断 QPC；WGC 样本用同一 QPC 原点 |
| `calib` | **1.0**（改须另预登记+比窗证明） |
| `ε` | **5.0 DIP/s**（继承 H5） |
| `P-VS` | **默认关**；若开启须另锁 `α_v` 并不破比窗 |
| 单位 | DIP/s；禁止两帧差分冒充本默认（P-2F 仅基线对照臂） |

### 3.2 与 H5 med_r

- **绝对 Δv smoke = 本票主验收**（见 §4）。  
- **速度比仍报且作回归门槛**：计入 PASS 的会话须 `med_r∈[0.8,1.2]` 且无 `r_i≥2.0`（详见 review §1）。  
- 注意：A 改为 LS 后，若辅证比的分母仍用末两帧 `v_screen`，则 `r=|v̂_LS/v_2F|` **不必≡1**——这是预期；回归门槛仍约束「别拧飞」。可选另报 `r_ls`（分母同用 LS 窗）作辅证，**不**替代 H5 窗口。

### 3.3 为何默认 P-LS+TS 而非单 P-MF

1. Δv′ 已用二次 w=5 证明 **可判且 base PASS**——测量几何可信；seed 仍用 2F → 与判决器几何不一致，是最干净的残差假说。  
2. P-TS 单独不治抖；与 LS 叠加成本低。  
3. P-MF/P-VS 可作失败后的次选实验，但不作本票 LOCK 默认（少一层调参）。

---

## 4) 绝对 Δv smoke（本票验收；非翻盘）

沿用 review §2，此处压缩为路径侧可粘贴条款：

- **格**：至少 **t060 fwd + t060 rev**（必测）；t300* 可选辅证；t156 规则 SKIP。  
- **测量工具**（非闸门翻盘）：二次 LS **w=5** 外推到 `t_s`；`thr_i=max(20, 0.05·|v_ref,i|)`；可判 `|a|σ≤thr ∧ thr≥Nv_h`；PASS 样本 `Nv_h≤thr_i`；≥**17/20**。  
- **会话**：≥3 独立 PID；**basePass=True**；**IVel0 t060 fwd+rev 均 FAIL**（P5：t300* IVel0 不进票）。  
- **基线**：相对 Δv′ 真机 t060 wgc 0–2/20 → 本票须 ≥17/20；可引用 Δv′ **产物数字**，**禁止**重跑 / 改写 gate `06467b82…805c`。  
- **结题标签**：`ticket=SEED-ABS-PASS|FAIL`；**禁止**写 `Δv-PASS`。  
- **成功 ≠ 采用**；本票 FAIL **不**重跑翻盘；完整绝对门复测挂 **M2-Δv″**（新闸门）。

---

## 5) LOCK 粘贴句（可直接进闸门「WGC→IVel 路径」）

> **默认 A**：打断前末 N=5 帧 WGC 位置对 QPC 时间做二次 LS，在 compositor 打断时刻 `t_int` 取 `v̂`，`InitialVelocity=calib·v̂`（calib=1.0）；须时戳同源对齐（P-TS）。**回退 B**：失败则 IV=null+0。ε=5.0 DIP/s。票 PASS 只评 A。  
> **禁**：`mf_spring_at` / spring.h 闭式 v → compositor IV；禁改 composition.h；禁为过门改 thr。  
> **验收**：缩小绝对 Δv smoke（至少 t060 fwd+rev；测量侧可引用二次 w=5 与 thr）；**不**重跑 `06467b82…805c`；H5 比作回归门槛；成功≠采用；≠Δv PASS。

---

## 6) 明确声明（照抄）

- 严禁修改 `composition.h`（sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）
- 严禁影子 IV；严禁解冻 Bv / 严禁重开 S1/R-3/R-2
- 成功 ≠ 采用；闸门仍为 DRAFT（本票绝对不授权采数）
- 本票 ≠ PERF-CI（仍 HOLD `5b007dd4…282a`）；不开 M3 真机
- 前序 Δv′ gate `06467b82…805c` **不改、不重跑翻盘**；前序 Δv `d2349ceb…09c6` **不改**
- 已交验收/风险稿保留：`docs/deepseek_m2_wgc_seed_abs_review.md`

— DeepSeek网页版（M2-WGC-SEED-ABS 路径设计；顶替 Gemini；Harness 未通代拟；文档 only）
