# DeepSeek · M2-Δv 闸门补洞

- 时间：2026-10-25（Australia/Perth, UTC+8）
- 状态：**文档 only**；不授权采数；不改 DRAFT/LOCKED 闸门本体；不改 composition.h
- **出处说明**：DeepSeek Harness（`deepseek-v4-pro`）本轮连续 `llm/retry` 空回（含「1+1」smoke）；box 网页 `chat.deepseek.com` 需登录且无 vault 登录。本文件由 DeepSeek网页版 bot **按** `docs/m2_delta_v_gate_DRAFT.md` / `docs/gemini_m2_delta_v_design.md` / `docs/notion_r2_45_ab.md` 历史可判口径起草，供 Notion LOCK；Harness 恢复后可对照重跑。
- PERF-CI：LOCKED `5b007dd4…282a` 仍 HOLD；本票不替代、不降级。

---

# M2-Δv 复核补洞（DRAFT；文档 only）

- 票：M2-Δv（Δv 绝对门）闸门补洞
- composition.h sha256：`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（未改）
- 硬边界复述：不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；成功≠采用；单机合法；VM=INVALID；PERF-CI 不降级

## 1) 可判门 + Nv_h / Nv_h_cell 操作定义

**度量（写死）**  
- 切换时刻 `t_s`（E2/E3 类切换窗；**禁止**纯帧差分冒充）。  
- `Δv = |v_post(t_s) − v_pre(t_s)|`（DIP/s）。  
- `v_ref = v_pre(t_s)`；`Δv_thresh = max(20 DIP/s, 0.05 · |v_ref|)`（逐样本阈；格级「阈中位」= 该格全部可判样本的 `Δv_thresh` 之中位）。

**Nv_h / Nv_h_cell（写死）**  
- `Nv_h`：单次切换估计的速度噪声半宽（DIP/s），由 harness 在 `t_s` 邻域对 `v_pre`/`v_post` 估计不确定性给出（沿用 R-2′ 探针字段名；实现不得改名为帧差分）。  
- `Nv_h_cell`：该格全部有效样本的 `Nv_h` 之**中位**。  
- **可判（格级）**：`Nv_h_cell ≤ 该格阈中位`。  
- **不可判（格级）**：不满足上式 → 标 `Δv-SKIP`（或 J-only 辅证），**不得**计 Δv-PASS。  
- **通过约束（不对称，沿用 #47）**：`Nv_h` **只约束通过**——样本欲计 Δv-PASS 时须该次 `Nv_h ≤ 该次 Δv_thresh`；若 Δv 的 CI 下界 > `Δv_thresh + Nv_h`，直接 **FAIL-确定**（不受「可判」豁免）。  
- **禁止**：用相对臂 A / `≤0.25×A` 冒充绝对门。

## 2) t156 结构性不可判处置

**拍板：跳过标 `Δv-SKIP`（对齐 Gemini；禁改估计器、禁重开 R-2/R-3）。**

- t156（fwd/rev，及若纳入的 dbl）本票一律登记为 **结构性 `Δv-SKIP`**：跑满 n≥20 仅作辅证/回归观察，**不计** Δv-PASS，也不因 SKIP 整票 INVALID。  
- **禁止**为「救 t156」改速度估计器、重开 R-2/R-3、或把不可判样本计成 PASS。  
- 票 PASS 下限**排除**全部 t156 格（见 §5）。  
- 冲突：无（与 Gemini 一致）。若未来另票修估计器，须新闸门，不复活本票口径。

## 3) H5-SEED 衔接

每会话必须落盘 **seed 声明**（建议字段，LOCK 写死列名即可）：

| 字段 | 含义 |
|---|---|
| `seed_path` | `WGC`（末两帧屏一致）或 `IV_NULL0` |
| `calib` | 常数；H5 基线 1.0（若改须声明） |
| `v_wgc` / `iv_set` | 实际 seed 速度或 unset+0 |
| `shadow_v_log` | 若记录 spring.h 闭式 v，**仅 log-only**，禁止写入 compositor IV |
| `session_id` / `pid` / `hostname` | 独立性证据 |

**检查点（任一即 INVALID）**：compositor `InitialVelocity` 取自 `mf_spring_at` / spring.h 闭式 v；或 seed 声明缺失不可复读。

## 4) G2-LOCK 可粘贴条目

- **G2-LOCK-1（阈）**：`Δv_thresh = max(20 DIP/s, 0.05·|v_ref|)`，`v_ref=v_pre(t_s)`；去掉相对-A / ≤0.25×A。  
- **G2-LOCK-2（窗）**：`t_s` 用 E2/E3 切换窗；禁止纯帧差分冒充 Δv。  
- **G2-LOCK-3（可判）**：格可判 ⇔ `Nv_h_cell ≤ 该格阈中位`；不可判 → `Δv-SKIP`，不计 Δv-PASS；Nv_h 只约束通过；CI 下界 > 阈+Nv_h → FAIL-确定。  
- **G2-LOCK-4（格表）**：t060 fwd/rev = 可判核心；t300 fwd/rev/dbl = 可判核心；t156 全部 = `Δv-SKIP`（对齐 Gemini）。每格 n≥20。  
- **G2-LOCK-5（格 PASS）**：可判格：单次 Δv≤该次阈 ≥17/20，且 Δv 中位 ≤ 该格阈中位。  
- **G2-LOCK-6（票 PASS）**：全部**可判核心格**达格 PASS；可判核心格数下限见 G2-LOCK-7；t156 SKIP 不挡也不充数。  
- **G2-LOCK-7（下限）**：可判核心格 = t060{fwd,rev} + t300{fwd,rev,dbl} 共 **5** 格；票 PASS 要求 **5/5** 格 PASS（若 LOCK 砍掉 dbl，则改为 4/4，须 LOCK 时二选一写死；**本补洞建议维持 5/5 含 dbl**）。  
- **G2-LOCK-8（会话）**：≥2 独立进程会话；会话 = 独立 PID + 场景重建；同进程复跑只计 1 会话。  
- **G2-LOCK-9（机器）**：允许 1 台物理机 DESKTOP-K93N3P8；VM/模拟器 = INVALID。  
- **G2-LOCK-10（H5）**：每会话 seed 声明完整；禁影子 IV。  
- **G2-LOCK-11（INVALID）**：见 §6。  
- **G2-LOCK-12（PERF-CI 隔离）**：本票产物不得写入 / 引用为 PERF-CI 合格证据；PERF-CI 仍须 ≥2 物理机。  
- **G2-LOCK-13（通则）**：成功≠采用；旁路挂账不挡；不重开 S1/R-3/R-2；不解冻 Bv；不改 composition.h。

## 5) 票 PASS 下限 + 会话独立性

- **可判格下限**：**5/5**（t060 fwd/rev + t300 fwd/rev/dbl）；t156 排除。  
- **会话**：≥2 独立进程（不同 PID）；每格样本量跨会话合计 n≥20（允许按会话均分，但不得单会话拼满后把同进程第二轮计为新会话）。  
- 与 Gemini：格 PASS 17/20、n≥20、≥2 会话、t156 SKIP —— **一致**。dbl 纳入 t300 —— **一致**（建议 LOCK 维持）。

## 6) INVALID 清单（任一即整票 INVALID）

1. 影子 IV / spring.h 闭式 v → compositor IV  
2. 改 `composition.h`（sha ≠ `a55d4952…e418`）  
3. 相对-A / ≤0.25×A 冒充绝对门  
4. VM / 非物理机  
5. 把本票产出写入 PERF-CI 合格证据，或宣称 PERF-CI 单机 PASS  
6. 未 LOCK+APPROVED 采数；口径偏离（改中位规则、把 t156 SKIP 计 PASS、纯帧差分冒充）  
7. 重开 S1/R-3/R-2 或解冻/升级 Bv；FAIL 后补测拼凑  

## 7) 绝对阈 LOCK 意见 + 与 Gemini 冲突

- **可否 LOCK `max(20, 5%|v_ref|)`：建议 LOCK（维持）**。形状继承 R-2′、已去掉相对-A；H5 后种子路径已清，适合作绝对预算起步。  
- **与 Gemini 冲突**：**无实质冲突**（阈、t156 SKIP、17/20、单机、禁影子 IV、PERF-CI 隔离均同向）。  
- **仅供 Notion 拍板的细差**（非冲突）：本补洞把票 PASS 下限写死为可判核心 **5/5（含 t300 dbl）**；若 Gemini/协调砍 dbl，LOCK 时改为 4/4 并删 G2-LOCK-4 中 dbl。Nv_h「只约束通过 + FAIL-确定」为 DeepSeek 从 #47 显式带回，Gemini 稿未展开——建议 LOCK 采纳，以免不可判与真实不连续混淆。

— DeepSeek网页版（M2-Δv 补洞；Harness 空回代拟；文档 only）
