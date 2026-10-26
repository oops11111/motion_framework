# workbuddy 派单：M2-Δv（自包含）

派单时间：2026-10-26 06:21:36 AWST
执行环境：Windows DESKTOP-K93N3P8，经 CodeBuddy 执行。**1 物理机合法**。

## 授权
- 闸门：`docs/m2_delta_v_gate_LOCKED.md`（STATUS=LOCKED，LOCKED_AT=2026-10-25 23:33:05 AWST，GATE_BODY_SHA256=d2349ceb0d37fdf332a4f00cb2c1f1217fe6856d0f3e5222ffaf82b8b15509c6）
- 批准：`docs/m2_delta_v_approved.md`（APPROVED_BY=pidan，APPROVED_AT=2026-10-26 06:21:36 AWST）
- **严格逐字遵循 LOCKED 闸门**；不得修改闸门 body / hash；不得在本闸门下追加补测（违规 → INVALID，须新 LOCK）。
- 第二刀 revise 仅审计：`docs/notion_m2_delta_v_gate_revise.md`；以 LOCKED 正文为准。

## 范围（仅 M2-Δv，依 LOCKED G0–G4）

### 臂 · 格 · 对照（G1）
- **被测臂** `Arm-WGC-seed`：InitialValue=WGC/屏一致；InitialVelocity=打断前末两帧 WGC（DIP/s，QPC）。
- **正对照** `base`：同内容时刻未打断；同估计器；须 Δv-PASS。
- **负对照** `IVel=0`：InitialVelocity=0；t060 fwd **与** rev 须 Δv-FAIL（非 PASS 候选）。
- **可判核心 5 格**：t060{fwd,rev} + t300{fwd,rev,dbl}；每格每会话 n=20 有效；≥17/20 达 `Δv_i ≤ thr_i`。
- t156{fwd,rev}：规则导出 Δv-SKIP 候选（辅证；SKIP≠PASS；不挡票）。
- dbl：Δv / v_ref 取**第二次**切换 `t_s2`。

### 主判据（G2 写死要点）
- `thr_i = max(20 DIP/s, 0.05 · |v_ref,i|)`，`v_ref = v_pre(t_s−)`；**禁止**相对-A / ≤0.25×A。
- 估计器：每侧邻接 `t_s` 的 **w=5** 帧**二次** LS，外推到 `t_s`；`Δv = |v_post − v_pre|`；**禁止**两帧差分。
- `t_s` = 窗 `W=[τ_{kc−1}, τ_{kc}]` 内 `|x_post − x_pre|` 最小时刻（QPC）。
- `Nv_h_cell` = 未打断 base、同估计器、打断前测得的 Δv **P95**；PASS 样本须 `Nv_h ≤ thr_i`。
- 可判 ⇔ `thr ≥ 2·Nv_h_cell` ∧ `|a|·σ_ts ≤ thr/2`；否则 SKIP。
- ≥**3** 独立进程会话；每会话均须达票内要求；禁跨会话池化；FAIL 会话不替换翻盘。
- 采数前预登记脚本+参数并哈希（`prereg_hash`）；`hang_cap` 默认 2。
- 票结局：Δv-PASS / FAIL / INCONCLUSIVE / INVALID（见 LOCKED G2-LOCK-9/11）。

### 机器 / 环境（G0）
- 1 台物理机 DESKTOP-K93N3P8；60 Hz；亮屏；G0 过；VM/模拟器 = INVALID。
- **本票 ≠ PERF-CI**；产物不得作 PERF-CI 合格证据。

## 硬禁令（G0/G3）
- 不改 `composition.h`；开工前 PRE 与结题 POST 各校验 sha256 = `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`，不符即停并报 INVALID。
- 不重开 S1 / R-3 / R-2；不解冻 / 不升级 B_v。
- **禁止影子 IV**（spring.h 闭式 v → compositor InitialVelocity）。
- 成功 ≠ 采用；旁路挂账不挡本票，本票也不清旁路。
- 不开 M3 真机；不把本票产出写入 PERF-CI。

## 产出
- 全部写入 `artifacts/m2_delta_v_*`（per-trial：φ, t_s, v_pre, v_post, thr, Δv, r, validity；SKIP 理由；对照；session meta；composition hash；prereg_hash）。
- 结题文件：`docs/m2_delta_v_result.md`，含判定、证据路径、禁令重申，以及 ONELINE（G4）：
  `M2-Δv | gate=d2349ceb0d37fdf332a4f00cb2c1f1217fe6856d0f3e5222ffaf82b8b15509c6 @ 2026-10-25 23:33:05 AWST | approved=pidan@2026-10-26 06:21:36 AWST | sessions=3/3|… | cells=5/5|… | ctrl=baseP/IVel0F | ticket=<PASS/FAIL/INCONCLUSIVE/INVALID> | t156=rule-SKIP | 成功≠采用；不改composition.h；禁影子IV；≠PERF-CI`

## 参考
- `docs/m2_delta_v_gate_LOCKED.md`
- `docs/m2_delta_v_approved.md`
- `docs/notion_m2_delta_v_gate_revise.md`
- `docs/gemini_m2_delta_v_design.md`
- `docs/deepseek_m2_delta_v_review.md`
- `docs/claude_m2_delta_v_review.md`
