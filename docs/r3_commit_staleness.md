# R-3：commit / 位置陈旧（运行记录）

> 状态：**CLOSED**（2026-10-24 Australia/Perth）— 闸门触发（A v2×3=2/3）→ STOP S1；Bv 降级兜底（非合格基线）；A1a 跟踪转出  
> 技术 ID 保持英文；本页为 R-3 滚动记录，不改 R-2 结题史。

## 开题原因

R-2（#47）本机 FAIL 结题后，根因归因已由「R-1 形状残差衍生」**被取代**：S1 v2 映射下形状残差仅 0.06–0.21%，B_v 跳变符合 **J ∝ |v| × commit 陈旧（约 1 帧）**。应另开 **R-3**，R-2 历史与 §4 门控不变。设计讨论见 `/workspace/claude_r2pos_reply.md`；短 stub 见 [claude_r3_design_review.md](./claude_r3_design_review.md)。

- **失效旧假设（设计口径）**：常量 `seedLead` 模型过时；相位依赖 δ̂(φ)=c−φ 才是候选。#25 数字在映射修复之前，**作废**。
- **降级现状**：M1b 仍用 B_v（不设 InitialValue + 影子解析速度），标注「不满足 R-2」；R-3 负责决定是否用叠加 / 相位种子等取代。

## #48（P0 + E1）一行

P0a–d 屏幕侧全 PASS（PS 读回对冲量项 NOOP，以 WGC 为准）。E1 576/576：**未验证**锯齿 δ̂(φ)=c−φ；Bv 的 J/|v| ≈ **12–13 ms 近恒定**（相位模型消不掉 commit 陈旧）。产物：`artifacts/p12_48.txt`；探针 `--r3-p12-probe`。详见 `docs/workbuddy_soft_demo.md` ## 派题 #48。

## #49（E3）叠加 vs Bv

真机矩阵 **288/288** 齐、解析 **287/288**。`composition.h` sha256  
`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` **未改**。

臂：
- **Sup**：additive 叠加 Expression `b + d1`（基弹簧不停；打断时在 `d1` 上步进弹簧 IV=0、FV=b_new−240、IVel=0）
- **Bv**：#47/#48 同款（+base 不打断对照）

### 主结论表（tInt=60，全 φ×b）

| 臂 | n | J 中位 | Δv_fd 中位 | ε 中位 |
| --- | --- | --- | --- | --- |
| Sup | 64 | 1.55 | 130.7 | 21.8 ms |
| Bv | 63 | 25.86 | 716.8 | 28.2 ms |
| base | 64 | ~0.73（噪声） | — | 0 |

门控（报告用）：
- **J**：PASS（Sup ≈ Bv/17，近噪声）
- **Δv_fd**：相对 PASS（Sup ≪ Bv）
- **ε**：FAIL（21.8 > 16.7 ms ≈ 1 帧）；稳健子集 ε ≈ 22.2 ms
- ε vs φ（t060）：φ=1 ≈ 30 ms，φ=13 ≈ 11–17 ms（相位相关启动滞后）

**裁决：不建议采用叠加。** 失败模式 = **响应延迟**（非一帧回跳）。E2 未跑。

产物：`artifacts/p13_49_raw.txt`、`p13_49.txt`、`win_r2_p13_e3.py`、`p13_49_codebuddy_{prompt,out}.txt`；Win `R3Probe.RunR3E3Probe` + `MainPage --r3-e3-probe`；`docs/workbuddy_soft_demo.md` ## 派题 #49。

## #50（Sup-adv 修订稿整包）

**ONELINE**：闸门 A=FAIL（本 full raw；对照 AB-only A=PASS）；S2后ε≈6.2 ms（扫描沿用 AB 的 6.8）；最佳ε̂=6.8 ms；J_vis门=YES；分流=记录 C / **R-3 仍 OPEN / 不采用**。

- **运行**：full raw wall=2026-10-24 19:08:33 AWST（UTC+8，`--r3-50-full` / `MF_R350_FULL`），A+B+C 齐；对照 AB-only wall≈18:45:48 AWST（C 未跑）。
- **阶段 A（本 raw）总闸门 FAIL**：A1a@τ0=21.8 中位 RMS=1.823 > N_h=0.41 → FAIL（其余 τ0 PASS）；A1b FAIL（+amp RMS 2.0–2.6；负 amp 出画 offscreen，早窗配对=False）；A1c FAIL（rep1–3 偶发坏 trial，RMS≈1.77、ε_fit=-5.0）；A2 path=PS PASS（ε 中位 9.0 ms，n=8）。
- **AB-only 对照**：A1a/A1b/A1c/A2 全 PASS，阶段 A=PASS；Sup/SupS2 ε 中位均 6.8 ms，ε̂ 候选 {0, 6.8, 16.7}。
- **阶段 B（本 raw）OK**：TRIAL 192/192；Sup ε 中位 7.0、SupS2 6.2，|Δ|=0.8 → S2 后明显变化 NO；Bv J 中位 16.09（Sup 0.26，base 0.07）。
- **阶段 C**：TRIALC 256/256；ε̂ 实际扫描 {0, 6.8, 16.7}（按 AB 重定档，非本 raw 候选 6.2）。出货门 `J ≤ d_frame + N_h_eff`（#47；禁止 ≤2×Sup）三档 × 四 cell 全 PASS → YES；最佳ε̂=**6.8**（J 中位 0.53–1.97）。
  - 有符号 e：ε̂=6.8 偏超前（φ 分档 0.8 / 4.8 / 8.3 / 11.8 ms）；ε̂=0 偏滞后（-13…-1）；ε̂=16.7 大超前≈21.7。fit/hold-out 一致（6.5 / 6.3）。
- **异常**：同日 full 会话阶段 A 与 AB-only 不一致（A1a τ0=21.8、A1c 坏 trial），A 结果不稳定，不得把单次 A 当最终闸门。
- **J_vis 过门 ≠ 采用 Sup-adv**。站立规则：闸门（A 稳定 PASS）与 Claude 复核齐备前，禁止采用任何 R-3 变体（成功≠采用）。
- `composition.h` sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` 未改；未改 E2。

产物：`artifacts/p14_50_raw.txt`、`p14_50.txt`、`p14_50_ab.txt`；`/workspace/r3/p50.py`；`docs/workbuddy_soft_demo.md` ## 派题 #50；Win `--r3-50-full` / `MF_R350_FULL`。

## #51（A 稳定性 + Part1/Part2）

**ONELINE**：A×3=0/3；Part1有效超前=δ0=39.5ms eff−τ0中位未饱和≈+2.7；A1a=FAIL[3.3,6.8,8.3,33.3]；A1b=PASS；−5伪FAIL双模{+6.5,−10}；c×3=14.00/13.89/13.90（均≈14，无≈14↔≈30跳）；卡点=A子项轮换+ε双模+A1a细网格非倍数FAIL；**NO-ADOPT**；composition.h sha256 a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418 未改。不比 #49 跨会话；同会话臂交错含 Sup/Bv/base；S2 未复活。

### A×3
| sess | A1a | A1b | A1c | A2 | A总 | 失败子项 |
| --- | --- | --- | --- | --- | --- | --- |
| a1 | PASS | FAIL | FAIL | PASS | FAIL | A1b early-pair(−Δ); A1c 4/5 ε_fit=−5 |
| a2 | FAIL | PASS | PASS | PASS | FAIL | A1a τ0=50 RMS中位=0.557 |
| a3 | PASS | PASS | FAIL | PASS | FAIL | A1c 3/5 ε_fit=−5 |

→ 可复现不稳定（子项轮换），非单次偶发。

### Part1
- #50 raw 放宽 τ 边界：A1c 的 ε_fit=−5 为撞界伪 FAIL；放宽后 ε 双模 {+6.5, −10}。
- A1a 细网格：有效超前 δ0=39.5 ms；未饱和 eff−τ0 中位 ≈+2.7；FAIL 格 [3.3, 6.8, 8.3, 33.3]（含非帧倍数与 33.3）。
- A1b origin=220 重裁剪：−Δ 不出画 → PASS。

### Part2
- 三会话 c≈14.00 / 13.89 / 13.90；ε≈14−φ；无 ≈14↔≈30 跳。
- S2 |Δε|：c1/c2=0.2、c3=1.8（离群，不复活 S2）。
- 同会话臂交错含 Sup/Bv/base；不与 #49 跨会话比较。

**裁决：NO-ADOPT / R-3 仍 OPEN。** 卡点=A 子项轮换 + ε 双模 + A1a 细网格非倍数 FAIL。`composition.h` sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` 未改。

产物：`artifacts/p15_51.txt`、`artifacts/p15_51_part1.txt`；`docs/workbuddy_soft_demo.md` ## 派题 #51；派题 [workbuddy_next_r3_51.md](./workbuddy_next_r3_51.md)。

## #52（P1+P2+P3 · STOP S1）

**ONELINE**：`#52: P1=分开(pooled; a1分不开); A v2×3=2/3; P3=F5确认; ≈30c=无; 硬停=A×3<3/3→STOP S1; sha OK`

### P3（离线，先于采数）
- **CONFIRM_F5_REFUTE_2X**：med(residual|unsat)=+2.70；SSE_2x/SSE_const=14.5×（Finding 5 确认；2× 超前假说证伪）。

### P2 — A v2×3（闸门 LOCKED_AT=2026-10-24 20:33:42 AWST，先于采数）
| sess | A1a | A1b | A1c | A2 | A |
| --- | --- | --- | --- | --- | --- |
| a1 | PASS | PASS | PASS | PASS | **PASS** |
| a2 | PASS | PASS | PASS | PASS | **PASS** |
| a3 | **FAIL** τ0=21.8 RMS中位=0.781 | PASS | PASS | PASS | **FAIL** |

- A1b / A1c / A2 均 3/3；A1c 无撞界（0 INVALID）。**A v2×3 = 2/3 < 3/3 → 硬停 STOP S1。**

### P1 — 帧相位 vs ε
- tickPer 实测 ≈16.66 ms；pooled n=36 **分开**（purity=0.83）；a1 单会话分不开；双模 {+6.5, −10} 再现。路径不稳健，不作 ship 依据。

### 其他
- ≈30 c：无。未复活 S2；未与 #49 横比。

**裁决：STOP S1 / S1 NO-ADOPT。** 建议下轮评审关闭 R-3，保留 Bv 为降级兜底。`composition.h` sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` PRE/POST OK 未改。

产物：`artifacts/p16_52.txt`（汇总）、`p16_52_p3.txt`、`p16_52_a_v2_gates.txt`、`p16_52_sha.txt`、`p16_52_p1_pooled.txt`、`p16_52_a{1,2,3}.txt` + `_raw.txt`；`docs/workbuddy_soft_demo.md` ## 派题 #52；派题 [workbuddy_next_r3_52.md](./workbuddy_next_r3_52.md)。


## 结题（CLOSED）

用户确认（2026-10-24 Australia/Perth）：**关 R-3（按 DeepSeek 三条挂账）**。Notion / Gemini / DeepSeek #52 评审皆同意关闭。S1 = **STOP / NO-ADOPT**。`composition.h` sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` 未改。

### 三条挂账（必须流传）

1. **关闭理由**：闸门触发（A v2×3=**2/3** < 3/3）→ STOP S1；**非**数据复审结论。
2. **Bv**：降级兜底、**非合格基线**；随 Bv 挂账：P1（a1 分不开；pooled purity=0.83）、P3（CONFIRM_F5_REFUTE_2X；med_res_unsat=+2.70；SSE_2x/SSE_const=14.5×）、≈30c=无、S2 未复活、不比 #49；**不作 ship 依据**。
3. **A1a 跟踪转出**：a3 @ τ0=21.8、RMS 中位=0.781；**不随 R-3 关闭蒸发**。
   - 状态（2026-10-24 23:12:41 AWST）：T-A1a = **CLOSED FAIL 挂账**（票级 FAIL G4-2 修正后未稳态；A-only×3=0/3），见 [t_a1a_closed.md](./t_a1a_closed.md) / [t_a1a_result.md](./t_a1a_result.md)；**R-3 保持 CLOSED，不重开**；挂账（DeepSeek 补洞四条 + Notion 独立票）随 closed 文件流传，不蒸发。

依据：`artifacts/p16_52.txt`；[notion_r3_52_pack.md](./notion_r3_52_pack.md)；[gemini_r3_52_opinion.md](./gemini_r3_52_opinion.md)；[deepseek_r3_52_opinion.md](./deepseek_r3_52_opinion.md)；前置硬停 [claude_r3_51_review.md](./claude_r3_51_review.md)；**结题英文 FINAL（Notion 顶替 Claude）** [claude_r3_52_close.md](./claude_r3_52_close.md)；brief [notion_brief.md](./notion_brief.md)。Claude Free 对照可保留，**不阻塞**关闭。

## 状态与下一步

| 项 | 值 |
| --- | --- |
| R-3 状态 | **CLOSED**（2026-10-24 Australia/Perth）；STOP S1；Bv 降级兜底（非合格基线）；A1a 跟踪转出；见上 ## 结题（CLOSED） |
| 叠加 E3 | 已否（ε） |
| 常量 seedLead / #25 | 设计上 obsolete |
| 重设计草稿 | [notion_r3_redesign.md](./notion_r3_redesign.md)（已按 Claude REVISE 修订，2026-10-24） |
| Claude 复核 | #50：[claude_r3_50_review.md](./claude_r3_50_review.md)。#51 fallback：[claude_r3_51_review.md](./claude_r3_51_review.md)。**#52 结题 FINAL（Notion AI 顶替 Claude）**：[claude_r3_52_close.md](./claude_r3_52_close.md)（R-3 CLOSED；S1 STOP；三条挂账）——非 Claude 亲撰 |
| Claude Free | **已恢复**（例行 06:50 Perth 已跑完并删除）。#51 Claude 亲撰 FINAL 已覆盖 [claude_r3_51_review.md](./claude_r3_51_review.md)；Notion 兜底仍见 `/workspace/notion_r3_51_review_reply.md` + [notion_r3_51_pack.md](./notion_r3_51_pack.md)。**仅对照，不重开 R-3** |
| 关键改点 | 敏感度一阶；落地 hop；出货门 J_vis；S2 前置；#50 扩 P0-fit / ε̂ 扫描 / 多 cell |
| #50 | **已完成**：A=FAIL（AB-only A=PASS，不一致）；B OK；C J_vis=YES，最佳ε̂=6.8；`artifacts/p14_50.txt` |
| 分流 | 记录 C 数据 / **不采用**（J_vis 过门 ≠ 采用） |
| 下一刀 | **无**（R-3 已结题 CLOSED）。挂账见 ## 结题（CLOSED） |
| #52 | **已完成**（STOP S1）：P3=CONFIRM_F5_REFUTE_2X；P1 pooled 分开（a1 分不开）；A v2×3=2/3（a3 A1a τ0=21.8 FAIL）；≈30c 无；sha OK；派题 [workbuddy_next_r3_52.md](./workbuddy_next_r3_52.md)；soft_demo ## 派题 #52 |
| #51 | **已完成**（NO-ADOPT）：A×3=0/3；c≈14×3；见上 ## #51、`artifacts/p15_51.txt`、`p15_51_part1.txt`；派题 [workbuddy_next_r3_51.md](./workbuddy_next_r3_51.md)；soft_demo [workbuddy_soft_demo.md](./workbuddy_soft_demo.md) ## 派题 #51 |
| Gemini 意见 | #50/#51 已到。[**#52 已到**](./gemini_r3_52_opinion.md)（同意 STOP S1；同意关 R-3 留 Bv） |
| Notion #51 包 | **已定稿（Notion AI 兜底）**：[notion_r3_51_pack.md](./notion_r3_51_pack.md)；英文权威 [claude_r3_51_review.md](./claude_r3_51_review.md)；结论 NO-ADOPT |
| Notion #52 包 | **已到**：[notion_r3_52_pack.md](./notion_r3_52_pack.md)；建议关 R-3 留 Bv；S1 STOP / NO-ADOPT |
| Notion #50 包 | **已按 Claude 定稿覆盖**：[notion_r3_50_pack.md](./notion_r3_50_pack.md) / [notion_r3_50_pack_claude.md](./notion_r3_50_pack_claude.md) |
| DeepSeek 意见 | #50/#51 已到。[**#52 已到**](./deepseek_r3_52_opinion.md)（同意 STOP S1；同意关 R-3+Bv，附闸门触发理由/Bv 挂账/A1a 跟踪转出） |
| 评审汇总 | **#52 三方同意关闭已执行**（Notion+Gemini+DeepSeek）。关闭理由=闸门触发；Bv 挂账；A1a 跟踪转出。Claude Free 对照可至 2026-10-25 06:50 Perth，不阻塞关闭 |

R-3 已 CLOSED。勿改 `composition.h`。Bv 为降级兜底（非合格基线）；A1a 跟踪项已 **CLOSED FAIL 挂账**（2026-10-24 23:12:41 AWST；见 [t_a1a_closed.md](./t_a1a_closed.md)），不重开 R-3/S1。E3 原样不重跑。

> **旁路指针（2026-10-24 23:35:47 AWST）**：挂账 / Bv 降级停放于 [side_parked_hang_degrade.md](./side_parked_hang_degrade.md)；**不阻塞 M2**；R-3 保持 CLOSED，不重开。

### Claude #51 对照 FINAL（事后归档 · 2026-10-25 06:56 AWST）

Claude Free 例行已跑完并删除。亲撰 FINAL 落盘：
- [claude_r3_51_review.md](./claude_r3_51_review.md)（5629 B；覆盖先前 Notion 兜底同路径副本）
- `/workspace/claude_r3_51_review_reply.md`

Notion #51 兜底原文仍保留（未丢）：
- `/workspace/notion_r3_51_review_reply.md`
- [notion_r3_51_pack.md](./notion_r3_51_pack.md)

**处置**：仅作对照归档。R-3 仍 **CLOSED**；**不重开 S1/R-3**；**不派 Claude 文中「#52」建议**（#52 已于关题前跑完并触发 STOP S1）。文中「R-3 stays OPEN」相对结题状态为过时口径，以 ## 结题（CLOSED）为准。composition.h 未改。主线仍为 M2-PERF-CI（HOLD，等第二台物理机）。

