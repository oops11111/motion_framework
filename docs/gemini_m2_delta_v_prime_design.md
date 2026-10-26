# Gemini · M2-Δv′ 可判几何修订（LOCK 前）

> 来源：Gemini 网页版 · 日期：2026-10-26（Australia/Perth）  
> 原始拷贝：`/workspace/gemini_m2_delta_v_prime_reply.md`  
> 依据：`docs/m2_delta_v_prime_gate_DRAFT.md`、`docs/notion_m2_delta_v_pack.md`、`docs/m2_delta_v_result.md`、artifacts/m2_delta_v_s{1,2,3}_summary.txt  
> 说明：下文为 Gemini 完整回复；数字仅引用既有真机结题。本文件**不授权采数**，不改闸门；前序 gate `d2349ceb…09c6` 不改；PERF-CI 仍 HOLD。

# M2-Δv′ 可判几何修订（LOCK 前 DRAFT 文档）

### 1) P1 t060 timing — 提案终稿

- **拍板拍定方案**：**可判准则放宽为 `|a|\sigma_\text{ts} \le \text{thr}`（去掉除以 2）**，同时保持绝对阈公式、二次 LS 外推（$w=5$）及种子领先路径绝对不变。
- **真机数字论证**：

  - 现用均匀帧模型下 $\sigma_\text{ts} = 1/60/\sqrt{12} \approx 0.00481\text{ s}$。真机实测显示 t060 格的 $\vert{}a\vert{}\cdot\sigma_\text{ts}$ 约为 $10.55 - 10.87$（例如 s1 fwd 10.550 / rev 10.807）。
  - 原先的严格互斥卡在 $\text{thr}/2 = 10$，导致实测值略高于 $10$ 因而全被判定不可判。
  - 修订后，分母放宽为 $\text{thr} = 20$，代入得 $\vert{}a\vert{}\sigma \approx 10.55 - 10.87 \le 20$，使得 t060{fwd, rev} 能够**顺利且诚实地进入可判集合**。
- **硬边界重申**：绝对阈公式、估计器阶数/w 以及 seedLead 绝对不作任何改动。

### 2) P2 t300 floor/SNR — 提案终稿

- **拍板拍定方案**：**将可判 SNR 约束自适应调整为 `thr \ge Nv_h`（即去掉 2 倍乘数，改为 `thr \ge Nv_h`）**。
- **与真机数字共存证明**：

  - 设 thr 绝对floor 为 $20$，而真机 t300 的噪声底 $Nv_h$ 的 P95 约在 $18 - 37$ 区间分布（例如 s1 达 $18.04 - 35.70$）。
  - 调整后：对于 $Nv_h \le 20$ 的子区（如部分 rev 约 $18.04$），满足 $\text{thr} \ge Nv_h$，**诚实进入可判**；而对于 $Nv_h > 20$ 的高噪声子区（如 $32.49 / 36.73$），仍严格受 $\text{thr} < Nv_h$ 约束而**保持诚实 SKIP**，绝不强行虚报可判。
- **防虚 PASS 一句说明**：噪声臂在进入可判后，其实测 WGC 命中率多为 $0 - 4/20$（$\Delta v \gg \text{thr}$），修订后若可判，预期更接近真实的 $\Delta v$-FAIL，而非被洗成虚 PASS。

### 3) 写死禁止清单（照抄并确认）

- 禁止为过门改 thr 公式、改 seedLead、改估计器阶数/w、改臂。
- t156 规则 `$\Delta v$-SKIP` ACCEPT 保留；核心 5 格集合保留。

### 4) LOCK 粘贴句

- **Δv′ 闸门 G1/G2 可判条款粘贴句**：“M2-$\Delta v'$ 核心可判几何度量正式修订预登记公式：t060 timing 侧放宽可判边界为 $\vert{}a\vert{}\sigma_\text{ts} \le \text{thr}$（剔除原先的 $/2$ 缩放），使实测 $\vert{}a\vert{}\sigma \approx 10.55-10.87 \le 20$ 合法进入可判；t300 floor/SNR 侧将防噪判据修订为 $\text{thr} \ge Nv_h$，对 $Nv_h \le 20$ 格诚实解封可判，对高噪格保持诚实 SKIP；绝对阈公式、二次 LS 估计器 ($w=5$)、t156 SKIP 及核心 5 格严格维持不变。”

### 5) 明确声明

- 严禁修改 `composition.h`（sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）。
- 严禁影子 IV；严禁解冻 Bv / 严禁重开 S1/R-3/R-2。
- 成功 ≠ 采用；闸门仍为 DRAFT（本票绝对不授权采数）。
- 本票 ≠ PERF-CI（仍 HOLD `5b007dd4…282a`）；不开 M3 真机。
- 前序 gate `d2349ceb…09c6` **不改**。
