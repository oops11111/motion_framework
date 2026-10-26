> **状态：已完成（2026-10-24）** — 闸门 A=FAIL（AB-only A=PASS）; S2后ε=6.2ms; 最佳ε̂=6.8ms; J_vis门=YES; 分流=记录 C / R-3 仍 OPEN / 不采用。详见 `artifacts/p14_50.txt`、`docs/r3_commit_staleness.md` ## #50。

# 派题 #50：R-3 Sup-adv（修订稿整包）

> 权威规格：`docs/notion_r3_redesign.md`（Claude REVISE 后）+ `docs/claude_r3_design_review.md`  
> 运行记录：`docs/r3_commit_staleness.md`  
> 对照：#48 `artifacts/p12_48.txt`；#49 `artifacts/p13_49.txt`

## 一行目标
conditional GO：先扩 **P0-fit** + **ε 定义检查**；通过后跑 **Sup+S2 → 标定 ε̂ → S1 ε̂ 扫描主矩阵**。不改 composition.h；无合成输入；WGC 为准。

## 硬约束
- 仅程序化 Composition；禁合成鼠标/键盘。
- composition.h sha256 保持 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（或报告若已变）。
- 记法：`base + d0 + d1`（d0=旧弹簧仍跑，d1=步进叠加项）。
- 禁止用「J/Δv ≤ 2×同批 Sup」作 S1 出货门。
- S1-φ 若做：相位源必须是产品可估（如 DwmFlush），**不能**用 harness 日志 φ。
- 成功 ≠ 采用；R-3 保持 OPEN。

## 阶段 A — 闸门（任一 FAIL → 停主矩阵，写分流）

### A1 扩 P0-fit
1. **种子平移**：若干 τ0，IV=s(τ0)、IVel=s′(τ0)；WGC vs 静止启动曲线平移 τ0；残差 ≤ #47 逐格 N_h。
2. **Δ 线性**：正负号、不同幅度。
3. **叠加**：d1 在运行中的 d0 上，Expression `base+d0+d1`，屏幕≈闭式和。
4. 门槛一律用 #47 N_h，不造新常量。

### A2 ε 定义检查
打断前 d0 与 d1 **同一** PS/Expression 路径；否则先统一路径再测 ε。

## 阶段 B — Sup+S2（S1 标定前置）
- **S2**：初始化预绑定常驻 Expression `base+Σdᵢ`（PS 槽）；重定向时不新建/重绑 Expression，只对空闲槽启弹簧。
- 臂至少：Bv / Sup（E3 原样）/ Sup+S2（无时间提前）。
- 测 ε（及 J、Δv）；S2 后若 ε 中位/分布相对 Sup 明显变化，**按新 ε 重定**后续 ε̂ 扫描取值（默认候选 {0, 16.7, 21.8} ms 可替换）。

## 阶段 C — S1 主矩阵（仅 A+B 闸门过）
- **ε̂ 扫描**：至少 {0(=Sup), 16.7, 21.8}（或 B 阶段重定后的三档）；S1 种子 IV=s(ε̂)、IVel=s′(ε̂)。
- **多 cell**：t060 + 不同速度 + 反号 Δ；每格有不打断 baseline。
- **φ**：桶样本量够；拟合批 vs hold-out；交错随机臂。
- **多 retarget**：3 链折叠 **或** 明确写出 live terms 上限（出货限制）。
- **门控/报告**：
  - 出货门（proposed）：`J_vis ≤ d_frame + N_h_eff`（#47）
  - 主指标：相对理想重定向轨迹的有符号位置/速度残差（落地帧及随后若干帧）
  - 有符号 e：median/P90/φ 分档；超前与滞后分列
  - Sup 时代 ε≤16.7 仅对照
  - 逐 trial 原始行：`φ, ε, e, J, Δv, N_h_eff`
  - composition.h sha256；derived/proposed/实测标注清楚

## 产物
- `artifacts/p14_50_*.txt`（raw + summary）、相关 py/探针源
- `docs/workbuddy_soft_demo.md` ## 派题 #50
- Win：探针入口（如 `--r3-50-probe`），CodeBuddy 可用

## 回报一行
闸门 PASS/FAIL；S2 后 ε；最佳 ε̂；是否有任一 ε̂ 过 J_vis 门；分流建议。CodeBuddy 用完待命。
