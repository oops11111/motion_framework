# DeepSeek 对 R-3 #50 的门控短意见（2026-10-24）

依据：`artifacts/p14_50.txt`；`docs/r3_commit_staleness.md` ## #50。

**结论：否决采用 Sup-adv；只记录 C 数据；R-3 保持 OPEN。**

## 1) C 过门但 A 不稳 → 否决采用
- 阶段 C 出货门 J_vis=YES（最佳 ε̂=6.8 ms），但本 full raw 阶段 A 总闸门 FAIL（A1a/A1b/A1c=FAIL，A2=PASS），与同日 AB-only 的 A=PASS 不一致。
- A 不稳定即前置闸门不成立，C 结果无合法前提。**只记录 C，不采用；R-3 OPEN。**

## 2) S2 |Δε|=0.8 不削弱前置
- 阶段 B：Sup ε 中位 7.0、SupS2 6.2，|Δ|=0.8 ms，S2 后变化不明显。
- 这不构成取消 S2 前置的理由；**降级为「必跑、ε 重定按阈值」**：S2 每次必跑，仅当 |Δε| 超阈值时才重定 ε̂ 档位，否则沿用。

## 3) #51 最小充分集 = A-only ×3
每跑须同时满足（N_h=0.41，沿用 #47，不引入新常数）：
- **A1a 种子平移**：全部 τ0（含 21.8）中位 RMS ≤ N_h，无 FAIL。
- **A1b Δ 线性**：±amp 均 PASS；无 offscreen，早窗配对=True。
- **A1c 叠加 (d0+d1)**：rep 内无坏 trial（无 RMS≈1.77 / ε_fit=-5.0 类离群）。
- **A2 ε 定义检查**：path=PS PASS，ε 中位与 Sup 口径一致。

**三跑皆稳 PASS** 才讨论采用 / 重跑 C；任一跑 FAIL → 先查 A 不稳根因，不进 C。
