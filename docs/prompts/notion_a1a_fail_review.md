# T-A1a FAIL 同行评审 — Notion

## 共享事实（自包含，评审方可能看不到仓库文件）
- 票 T-A1a 为独立票；R-3 已 CLOSED；**不得重开 S1 / R-3**；success ≠ adopt（成功不等于采纳）。
- composition.h 未改动，sha256 = a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418
- Gate 已锁：docs/t_a1a_gate_LOCKED.md，hash 0750a8402d4118c98025e71863f79bafd689966c2565fa922d0674e8bcf89d1d @ 2026-10-24 22:27:54 AWST
- 已批准：docs/t_a1a_approved.md，pidan @ 2026-10-24 22:34:47 AWST
- corr = CONFIRM_F5_REFUTE_2X（常数偏移）；N_h = 0.41
- Workbuddy ONELINE：
  `T-A1a | gate=0750a840… | S1:A1a=F A1b=F A1c=P A2=P | S2:A1a=F A1b=P A1c=P A2=P | S3:A1a=F A1b=F A1c=P A2=P | A-only×3=0/3 | ticket=FAIL | 归因=修正后未稳态`
- A1a：21.8 / 33.3 通过；3.3 / 6.8 / 8.3 在全部 3 个 session 均 FAIL，RMS ≈ 1.22–1.38 > 0.41；A1c / A2 均 3/3 P；A1b 仅 S2 P；**非 INVALID**。
- 产物：artifacts/t_a1a_*；结果落点 docs/t_a1a_result.md（可能稍后出现）；离线复核 docs/t_a1a_offline_review.md；简报 docs/notion_a1a_tracker_brief.md

## 请回答
1) 是否同意票级 FAIL + 归因「修正后未稳态」？（同意/不同意 + 理由）
2) 下一步：关 T-A1a 挂账 / 新一轮 LOCK（改 OFFSET_METHOD 或网格）/ 其他？
3) 用一句话说明：3.3/6.8/8.3 仍 FAIL 而 21.8/33.3 通过，意味着什么？
4) 明确声明：不重开 S1、不改 composition.h。

## 交付（Notion）
请产出中文包 docs/notion_a1a_fail_pack.md，结构：
1. 结论（票级判定 + 归因 + 对 Q1–Q4 的回答）
2. 表（S1/S2/S3 × A1a/A1b/A1c/A2；A1a 分频点 RMS 对比 N_h=0.41）
3. 建议下一步（关挂账 / 新 LOCK 方案 / 其他）
注：Claude Free 处于空窗期；可附英文草稿，但不阻塞中文包。
