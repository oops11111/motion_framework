# Gemini · M2-WGC-SEED-ABS · WGC→IVel 路径提案（LOCK 前）

> 来源：Gemini 网页版 · 日期：2026-10-26（Asia/Taipei）  
> 原始拷贝：`/workspace/gemini_m2_wgc_seed_abs_reply.md`  
> 依据：`docs/m2_wgc_seed_abs_gate_DRAFT.md`、`docs/notion_m2_wgc_seed_abs_next.md`、`docs/notion_m2_delta_v_prime_pack.md`、`docs/m2_delta_v_prime_result.md`、artifacts/m2_delta_v_prime_s{1,2,3}_summary.txt、对照 H5 `docs/m2_h5_seed_result.md`  
> 说明：下文为 Gemini 完整回复；数字仅引用既有真机结题。本文件**不授权采数**，不改闸门；Δv′ gate `06467b82…805c` 不改不重跑翻盘；PERF-CI 仍 HOLD。

# M2-WGC-SEED-ABS · WGC→InitialVelocity 路径提案（LOCK 前 DRAFT 文档）

### 1) 候选路径清单

- **P-2F（现状 H5 A）**：

  - *怎么估 v*：打断前末两帧 WGC Translation.X 的 $(x_2 - x_1)/(t_2 - t_1)$，QPC 转换为秒，单位 DIP/s。
  - *预登记参数*：$\text{calib} = 1.0$。
  - *优缺点*：逻辑极简、无额外滞后；但对高频抖动敏感，导致真机实测 `wgc_pass` 偏低（s1 2/20、s2 0/20、s3 1/20）。
  - *改善方向*：作为本次绝对 $\Delta v$ 验收的基线对照。
- **P-MF（多帧平滑）**：

  - *怎么估 v*：对打断前末 $N$ 帧速度采用滑动平均（MA）或指数加权移动平均（EWMA）。
  - *预登记参数*：$N = 5$ 或平滑系数 $\alpha = 0.3$。
  - *优缺点*：能有效滤除单帧偶发高频毛刺；缺点是引入微小相位滞后。
  - *预期改善*：相比 P-2F 降低速度突变，提升 $\Delta v$ 绝对门命中率。
- **P-LS（最小二乘外推）**：

  - *怎么估 v*：对打断前末 $N$ 帧进行一次或二次 LS 拟合，外推至打断瞬间 $t_s$。
  - *预登记参数*：阶数 = 2（二次 LS），帧数 $N = 5$（与 Δv′ 估计器 $w=5$ 口径对齐，但接线点在 seed IV）。
  - *优缺点*：契合系统内部估计器数学模型，趋势捕捉能力强；对底层时戳离散较敏感。
  - *预期改善*：平滑性与趋势预判平衡最佳，有望大幅提升绝对阈命中率。
- **P-TS（时戳对齐）**：

  - *怎么估 v*：将 WGC 采样时戳与 compositor 实际打断时刻进行线性插值对齐到同一 $t$。
  - *预登记参数*：时间插值容差 $\varepsilon \le 1.5 \text{ ms}$。
  - *优缺点*：根源上消除采样不同步造成的瞬时差分误差；计算实现复杂度较高。
  - *预期改善*：消除时序错位带来的系统性偏差。
- **P-VS（VEL-SMOOTH 并入）**：

  - *怎么估 v*：对 $v_{\text{wgc}}$ 实施后置二次约束平滑后再向下游 compositor 输出 InitialVelocity。
  - *预登记参数*：限幅率 $\Delta v_{\text{max}} \le 1500 \text{ DIP/s}^2$。
  - *优缺点*：与本票任务流完美并行；确保输出速度绝对平滑。
  - *预期改善*：防止突变速度直接透传进弹簧初始态。

### 2) 否决影子路径

- 否决：`mf_spring_at` / `spring.h` 闭式 $v \to \text{compositor IV}$。
- 否决：任何「对齐 compositor 弹簧数学到 `spring.h`」的 IV 来源。

### 3) 推荐默认 + 回退

- **默认路径**：采用 **P-LS（二次 LS 外推， $N=5$）+ P-VS（限幅限速平滑）** 作为主路径，严格继承 H5 **A→B** 精神。若在主路径试算样本中因偶发抖动导致绝对 $\Delta v$ 不达标，则**条件回退至 B 方案（$\text{IV} = 0$）**。
- **预登记参数**：$N = 5$，阶数 = 2，$\text{calib} = 1.0$。
- **与 H5 med\_r 关系句**：本票依然保留速度比（med\_r $\equiv 1.0$）作为辅助验证参考，但**绝对 $\Delta v$ 命中率（$\Delta v \le \text{thr}$）作为主验收指标**。

### 4) 绝对 Δv smoke 建议（本票验收，非翻盘）

- **验收网格**：至少覆盖 **t060 fwd + rev**；沿用 Δv′ 估计器（二次 $w=5$）与绝对阈公式 $\text{thr} = \max(20, 0.05 \cdot \vert{}v_{\text{ref}}\vert{})$。
- **范围红线**：绝不写成「重跑 FAIL 闸门 `06467b82…805c` 翻盘」；本票定义为**新票缩小 smoke 验证**（评估 P-LS 相对 H5 末两帧基线的改善幅度；会话数 $\ge 3$，且严格继承原有的 base 及 IVel0 负对照验证）。
- **核心原则**：成功 $\neq$ 采用；FAIL 不重跑本票翻盘。

### 5) LOCK 粘贴句

- **WGC→IVel 路径粘贴句**：“M2-WGC-SEED-ABS 闸门 WGC→IVel 默认采用 P-LS 路径（二次 LS 外推， $N=5$）并级联 P-VS 平滑限幅，预登记 $\text{calib} = 1.0$；主路径失败时条件回退至 B 方案（$\text{IV} = 0$）；严禁任何形式的影子 IV 与 compositor 弹簧数学对齐。”

### 6) 明确声明

- 严禁修改 `composition.h`（sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）。
- 严禁影子 IV；严禁解冻 Bv / 严禁重开 S1/R-3/R-2。
- 成功 ≠ 采用；闸门仍为 DRAFT（本票绝对不授权采数）。
- 本票 $\neq$ PERF-CI（仍 HOLD `5b007dd4…282a`）；不开 M3 真机。
- 前序 Δv′ gate `06467b82…805c` **不改、不重跑翻盘**；前序 Δv gate `d2349ceb…09c6` **不改**。
