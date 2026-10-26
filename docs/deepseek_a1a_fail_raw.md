Let me understand the task. This is a short review (短评审) for ticket T-A1a, which is a FAIL review based on real machine (真机) data. The user asks me to answer Q1-Q4 in a short opinion (≤300 characters? Actually ≤300字, meaning 300 Chinese characters) in Simplified Chinese, without inventing numbers.

This appears to be a decision review task. The user is asking me as a reviewer to:
1) Agree or disagree with ticket-level FAIL + attribution "修正后未稳态" (post-correction not stable)
2) Next step: close T-A1a with pending note / new LOCK round (change OFFSET_METHOD or grid) with hole-filling conditions / other?
3) One sentence: what does it mean that 3.3/6.8/8.3 still FAIL while 21.8/33.3 pass?
4) Explicitly declare: no reopening S1, no changing composition.h.

Let me check the facts:
- Gate G4 attribution priority (hit and stop):
  1. Ticket-level INVALID → new LOCK round
  2. Post-correction A-only ×3 < 3/3 → "修正后未稳态", ticket-level FAIL (takes priority over "不可复现")
  3. A-only ×3 = 3/3 → ticket-level PASS (≠ reopen S1, ≠ adopt)

- This round's outcome: A-only ×3 = 0/3 → G4-2 → ticket=FAIL, attribution=修正后未稳态.

Wait, let me check: A-only ×3 = 0/3. In the session table, column A: S1=F, S2=F, S3=F. So A = A1a AND A1b AND A1c AND A2 presumably all pass. Since A1a fails in all three sessions, A fails in all three. So A-only×3=0/3. The gate says A-only×3 < 3/3 → "修正后未稳态", FAIL.

Hmm, but wait — is this the right attribution? Let me think carefully. The grid shows:
- τ0 = 21.8: PASS×3
- τ0 = 3.3: FAIL×3 (RMS ~1.22-1.25)
- τ0 = 6.8: FAIL×3 (RMS ~1.32-1.33)
- τ0 = 8.3: FAIL×3 (RMS ~1.37-1.38)
- τ0 = 33.3: PASS×3

So the A1a metric (RMS_gate median) fails for small τ0 values (3.3, 6.8, 8.3) but passes for 21.8 and 33.3.

The attribution "修正后未稳态" (post-correction not stable) — is that consistent? The G4-2 rule maps A-only×3 < 3/3 to this attribution. Since the gate is LOCKED and the priority rules are set, mechanically the result is ticket FAIL with attribution 修正后未稳态.

The question asks whether I agree. Let me think about the substance: the RMS_gate for small τ0 is high (1.22-1.38 > N_h=0.41) and consistent across sessions, while large τ0 passes. This suggests the correction (CONFIRM_F5_REFUTE_2X, constant offset) handles the large τ0 case but not small τ0. Actually, this suggests a systematic pattern: correction insufficient for small τ0 — i.e., the corrected method isn't stable across the τ0 range. So "修正后未稳态" (post-correction not in steady state / not stable) fits, though one could argue it's actually "修正覆盖不足" (correction coverage insufficient) or a systematic τ0-dependent residual. But the gate attribution is fixed: A-only×3=0/3 → G4-2 → 修正后未稳态. Since the gate is LOCKED and this is a short review, I should agree mechanically per the gate, noting the grid pattern is consistent with the attribution (failures are reproducible across sessions, and specifically localized to small τ0 — consistent, reproducible failure, not noise).

Q1: Agree. Reasons: A-only×3=0/3 hits G4-2 directly (priority 2), which mandates ticket FAIL with attribution 修正后未稳态, and it takes priority over "不可复现" (irreproducible). Also the failure is not noise: small τ0 cells fail consistently across all 3 sessions (RMS 1.22–1.38), which is a stable failure pattern, not instability per se... Hmm, actually "修正后未稳态" literally means "not stabilized after correction". The fact that it's consistent across sessions — high consistency — could suggest it's a systematic residual rather than instability. But the gate defines the attribution by rule, and the rule maps A-only<3/3 to that label. Within a ≤300字 answer, I should agree with the FAIL but perhaps note that the data suggest a systematic τ0-dependent residual (small τ0 under-corrected), which is compatible with "修正后未稳态" as gate-defined; the distinction matters for the next round's hole-filling conditions.

Actually, let me be careful: the user asks "是否同意票级 FAIL + 归因「修正后未稳态」？（同意/不同意 + 理由）". I should give a clear yes/no. The mechanical gate answer is yes. Reasons: (1) gate rule hit-stop: A-only×3=0/3 < 3/3 → G4-2 → FAIL/修正后未稳态, priority over 不可复现; (2) small-τ0 cells fail 3/3 with RMS 1.22–1.38 > N_h=0.41, large-τ0 pass 3/3 — a reproducible, τ0-dependent residual, so it's not a fluke; labeling as "修正后未稳态" is gate-consistent.

Q2: Next step. Options: close T-A1a with pending note (关 T-A1a 挂账) vs new LOCK round (改 OFFSET_METHOD 或网格) with hole-filling conditions, or other. 

Given success ≠ adopt and R-3 closed and no reopening S1, my recommendation: close T-A1a with the FAIL recorded (挂账: FAIL + 归因=修正后未稳态, 数据留存), and optionally open a new ticket for a new LOCK round ONLY if a fix proposal exists — the hole-filling conditions (补洞条件): (a) explain why small τ0 has residual ~1.2-1.4 (OFFSET_METHOD's constant offset insufficient for small τ0, i.e., residual grows as τ0 decreases); (b) propose concrete change (e.g., τ0-dependent correction or revised OFFSET_METHOD/网格 covering 3.3-8.3 range); (c) verify PRE/POST composition.h unchanged and pass the same gate protocol; (d) demonstrate corrected small-τ0 cells ≤ N_h=0.41 on real machine in ≥3 sessions before a new LOCK. And success ≠ adopt must stay.

Also per rule 1: ticket-level INVALID → new LOCK round. Is this ticket INVALID? The facts say: 非 INVALID：method_ok=True ×3，used_2x=False，used_magic_single=False，A1c n_invalid=0. So the ticket is NOT invalid, so G4-1 doesn't trigger. Good — that supports not auto-triggering a new LOCK; the correct path is close with 挂账 + 补洞条件, and a new LOCK only when a correction proposal meets the conditions.

Hmm, but "关 T-A1a 挂账 / 新一轮 LOCK 须列补洞条件 / 其他?" — the question implies: if choosing to close with pending note, list hole-filling conditions; if new LOCK, also must list conditions. So I should recommend: 关 T-A1a 挂账（不立即新 LOCK），补洞条件 as above; 新 LOCK 仅在满足条件后由新票发起. That's the "其他" combined option perhaps. Let me keep it within word budget.

Q3: One sentence meaning: 3.3/6.8/8.3 still fail while 21.8/33.3 pass → the constant-offset correction (OFFSET_METHOD, ref=+2.70) is insufficient for small τ0 — residual depends on τ0 magnitude, i.e., the correction is not invariant across the τ0 grid, so the method is valid only for larger τ0 and not yet a complete fix. In one sentence: 说明当前常数偏移修正只对较大 τ0 生效，小 τ0 残差未消除（RMS≈1.2–1.4），即修正不具跨 τ0 普适性，方法尚未覆盖整个网格。

Q4: Declare: 不重开 S1、不改 composition.h. Yes — S1 维持原判定，composition.h 保持不动（sha256 前后一致 a55d...e418）。This review is independent, success ≠ adopt.

Now, the deliverable: 短意见 ≤300字, answering Q1-Q4 item by item, Simplified Chinese, no invented numbers.

Also there's a team context: "You are the Team Lead named lead. Use list_agents to find teammates." But this seems to be a solo short review task — the user asks me to give a short opinion. I could just answer directly. The prompt says "你此前 brief 审要求的..." — it's addressed to me as the reviewer. I don't think I need teammates for a ≤300字 opinion. But the runtime context suggests using list_agents to check teammates. Given the task is a short review deliverable, doing it directly is appropriate and efficient. I might quickly check list_agents? It's optional. The instructions say "Use list_agents({}) to find your teammates and their names." That's general guidance. Since this is a self-contained review with a 300-character deliverable, delegating would add overhead and risk. I'll answer directly.

Should I check the referenced docs (docs/t_a1a_gate_LOCKED.md, docs/t_a1a_approved.md) in the workspace? The user says "只用下列事实" (use only the facts listed). So I should NOT go read files or invent anything. Answer from the given facts only.

Let me now compose the answer carefully, counting characters (≤300 字).

Draft:

Q1：同意。A-only×3=0/3，按已 LOCK 的 G4 优先级命中第2条（优先于"不可复现"），票级 FAIL、归因"修正后未稳态"成立；且非 INVALID（method_ok×3、n_invalid=0），不触发 G4-1。

Q2：建议关 T-A1a 挂账（FAIL 与网格数据留档），暂不新 LOCK；补洞条件：①解释小 τ0 残差机制（常数偏移对小 τ0 不足，RMS 随 τ0 减小上升）；②提出覆盖 3.3/6.8/8.3 的具体修正或新网格方案；③改后真机 ≥3 会话该三格 RMS_gate≤0.41 且大 τ0 不回退，再走新一轮 LOCK；④composition.h 保持不动，success≠adopt。

Q3：说明常数偏移修正不具跨 τ0 普适性：只对大 τ0（21.8/33.3）有效，小 τ0 残差仍约 1.2–1.4，属系统性未修净，而非随机噪声。

Q4：声明：不重开 S1，不改 composition.h（PRE/POST sha256 一致）。

Let me count roughly. That's probably around 300-330 characters. Let me trim.

Actually let me count more carefully. Chinese characters count: 

Q1: "同意。A-only×3=0/3，按已LOCK的G4优先级命中第2条（优先于不可复现），票级FAIL、归因修正后未稳态成立；且非INVALID（method_ok×3、n_invalid=0），不触发G4-1。" ≈ 80 chars.

Q2: "建议关T-A1a挂账（FAIL与网格数据留档），暂不新LOCK；补洞条件：①解释小τ0残差机制（常数偏移对小τ0不足）；②提出覆盖3.3/6.8/8.3的具体修正或新网格方案；③改后真机≥3会话该三格RMS_gate≤0.41且大τ0不回退，再走新一轮LOCK；④composition.h保持不动，success≠adopt。" ≈ 120 chars.

Q3: "说明常数偏移修正不具跨τ0普适性：仅对大τ0（21.8/33.3）有效，小τ0残差约1.2–1.4未修净，属系统性残差而非噪声。" ≈ 55 chars.

Q4: "声明：不重开S1，不改composition.h（PRE/POST sha256一致）。" ≈ 30 chars.

Total ≈ 285. OK within budget. Let me refine wording and make sure each item is clean and no invented numbers (all numbers come from the given facts: 0.41 = N_h, 1.2–1.4 from grid, 3.3/6.8/8.3, 21.8/33.3 — all given).

One check on Q2 wording: the question says "新一轮 LOCK（改 OFFSET_METHOD 或网格）须列补洞条件". So if I recommend closing with 挂账 and new LOCK only after conditions met, I should list the conditions. My draft does that.

Also note "A1c n_invalid=0" — non-INVALID facts. I mention method_ok×3. Fine.

Should I mention A1b? Not necessary for the gate result. Keep it short.

One more consideration on Q1: is there any subtlety that the attribution "修正后未稳态" might be wrong because the data are actually very stable (cross-session consistent)? The gate label is defined by rule; as reviewer I can agree with FAIL and the gate label while noting the pattern is systematic (which actually argues the problem is a systematic residual, supporting a correction gap rather than a fluke). The gate's own rule maps <3/3 to 修正后未稳态, so agreeing is gate-consistent. I'll agree, noting the cross-session consistency actually strengthens FAIL (reproducible failure), and the label follows the LOCKed rule; the Q3 sentence covers the mechanism. That's fine.

Format: 逐条答 Q1–Q4. Use headers Q1...Q4. Keep ≤300字. Chinese simplified.

Let me also double-check the gate priority: "2. 修正后 A-only×3 <3/3 → 「修正后未稳态」，票级 FAIL（优先于「不可复现」）". Yes. "本轮落点：A-only×3=0/3 → G4-2 → ticket=FAIL，归因=修正后未稳态。" Confirmed by the prompt itself.

Q2 options: 关 T-A1a 挂账 / 新一轮 LOCK 须列补洞条件 / 其他. I recommend 关 T-A1a 挂账 + 列补洞条件 + 满足条件后才可新 LOCK. That's a hybrid of option 1 with conditions; acceptable as "其他" or as option 1 with conditions. I'll phrase: "建议：关 T-A1a 挂账，暂不直接开新 LOCK；满足以下补洞条件后，方以新票发起新一轮 LOCK：…"

Final count check — I'll write it and roughly count. The requirement ≤300字 likely counts Chinese characters; I'll aim ~280. Numbers/ASCII probably count less strictly, but I'll keep total tight.

Let me finalize.
# T-A1a 真机 FAIL 短评审（G4）

**Q1｜同意。** A-only×3=0/3 直接命中已 LOCK 的 G4-2（且优先于「不可复现」），票级 FAIL、归因「修正后未稳态」成立；本票非 INVALID（method_ok×3、n_invalid=0），不触发 G4-1。

**Q2｜建议：关 T-A1a 挂账**（FAIL 与网格数据留档），暂不直接开新 LOCK；补洞条件：①解释小 τ0 残差机制（常数偏移对小 τ0 不足）；②提出覆盖 3.3/6.8/8.3 的具体修正（改 OFFSET_METHOD 或网格）；③改后真机 ≥3 会话三格 RMS_gate≤0.41 且 21.8/33.3 不回退，方可新票发起新一轮 LOCK；④全程不改 composition.h，success≠adopt。

**Q3｜** 说明该常数偏移修正不具跨 τ0 普适性：仅对大 τ0（21.8/33.3）有效，小 τ0 残差约 1.2–1.4 未修净，属系统性残差而非噪声。

**Q4｜声明：** 不重开 S1；不改 composition.h（PRE/POST sha256 一致）。
Let me understand the task. This is a short review (短评审) for ticket T-A1a, which is a FAIL review based on real machine (真机) data. The user asks me to answer Q1-Q4 in a short opinion (≤300 characters? Actually ≤300字, meaning 300 Chinese characters) in Simplified Chinese, without inventing numbers.

This appears to be a decision review task. The user is asking me as a reviewer to:
1) Agree or disagree with ticket-level FAIL + attribution "修正后未稳态" (post-correction not stable)
2) Next step: close T-A1a with pending note / new LOCK round (change OFFSET_METHOD or grid) with hole-filling conditions / other?
3) One sentence: what does it mean that 3.3/6.8/8.3 still FAIL while 21.8/33.3 pass?
4) Explicitly declare: no reopening S1, no changing composition.h.

Let me check the facts:
- Gate G4 attribution priority (hit and stop):
  1. Ticket-level INVALID → new LOCK round
  2. Post-correction A-only ×3 < 3/3 → "修正后未稳态", ticket-level FAIL (takes priority over "不可复现")
  3. A-only ×3 = 3/3 → ticket-level PASS (≠ reopen S1, ≠ adopt)

- This round's outcome: A-only ×3 = 0/3 → G4-2 → ticket=FAIL, attribution=修正后未稳态.

Wait, let me check: A-only ×3 = 0/3. In the session table, column A: S1=F, S2=F, S3=F. So A = A1a AND A1b AND A1c AND A2 presumably all pass. Since A1a fails in all three sessions, A fails in all three. So A-only×3=0/3. The gate says A-only×3 < 3/3 → "修正后未稳态", FAIL.

Hmm, but wait — is this the right attribution? Let me think carefully. The grid shows:
- τ0 = 21.8: PASS×3
- τ0 = 3.3: FAIL×3 (RMS ~1.22-1.25)
- τ0 = 6.8: FAIL×3 (RMS ~1.32-1.33)
- τ0 = 8.3: FAIL×3 (RMS ~1.37-1.38)
- τ0 = 33.3: PASS×3

So the A1a metric (RMS_gate median) fails for small τ0 values (3.3, 6.8, 8.3) but passes for 21.8 and 33.3.

The attribution "修正后未稳态" (post-correction not stable) — is that consistent? The G4-2 rule maps A-only×3 < 3/3 to this attribution. Since the gate is LOCKED and the priority rules are set, mechanically the result is ticket FAIL with attribution 修正后未稳态.

The question asks whether I agree. Let me think about the substance: the RMS_gate for small τ0 is high (1.22-1.38 > N_h=0.41) and consistent across sessions, while large τ0 passes. This suggests the correction (CONFIRM_F5_REFUTE_2X, constant offset) handles the large τ0 case but not small τ0. Actually, this suggests a systematic pattern: correction insufficient for small τ0 — i.e., the corrected method isn't stable across the τ0 range. So "修正后未稳态" (post-correction not in steady state / not stable) fits, though one could argue it's actually "修正覆盖不足" (correction coverage insufficient) or a systematic τ0-dependent residual. But the gate attribution is fixed: A-only×3=0/3 → G4-2 → 修正后未稳态. Since the gate is LOCKED and this is a short review, I should agree mechanically per the gate, noting the grid pattern is consistent with the attribution (failures are reproducible across sessions, and specifically localized to small τ0 — consistent, reproducible failure, not noise).

Q1: Agree. Reasons: A-only×3=0/3 hits G4-2 directly (priority 2), which mandates ticket FAIL with attribution 修正后未稳态, and it takes priority over "不可复现" (irreproducible). Also the failure is not noise: small τ0 cells fail consistently across all 3 sessions (RMS 1.22–1.38), which is a stable failure pattern, not instability per se... Hmm, actually "修正后未稳态" literally means "not stabilized after correction". The fact that it's consistent across sessions — high consistency — could suggest it's a systematic residual rather than instability. But the gate defines the attribution by rule, and the rule maps A-only<3/3 to that label. Within a ≤300字 answer, I should agree with the FAIL but perhaps note that the data suggest a systematic τ0-dependent residual (small τ0 under-corrected), which is compatible with "修正后未稳态" as gate-defined; the distinction matters for the next round's hole-filling conditions.

Actually, let me be careful: the user asks "是否同意票级 FAIL + 归因「修正后未稳态」？（同意/不同意 + 理由）". I should give a clear yes/no. The mechanical gate answer is yes. Reasons: (1) gate rule hit-stop: A-only×3=0/3 < 3/3 → G4-2 → FAIL/修正后未稳态, priority over 不可复现; (2) small-τ0 cells fail 3/3 with RMS 1.22–1.38 > N_h=0.41, large-τ0 pass 3/3 — a reproducible, τ0-dependent residual, so it's not a fluke; labeling as "修正后未稳态" is gate-consistent.

Q2: Next step. Options: close T-A1a with pending note (关 T-A1a 挂账) vs new LOCK round (改 OFFSET_METHOD 或网格) with hole-filling conditions, or other. 

Given success ≠ adopt and R-3 closed and no reopening S1, my recommendation: close T-A1a with the FAIL recorded (挂账: FAIL + 归因=修正后未稳态, 数据留存), and optionally open a new ticket for a new LOCK round ONLY if a fix proposal exists — the hole-filling conditions (补洞条件): (a) explain why small τ0 has residual ~1.2-1.4 (OFFSET_METHOD's constant offset insufficient for small τ0, i.e., residual grows as τ0 decreases); (b) propose concrete change (e.g., τ0-dependent correction or revised OFFSET_METHOD/网格 covering 3.3-8.3 range); (c) verify PRE/POST composition.h unchanged and pass the same gate protocol; (d) demonstrate corrected small-τ0 cells ≤ N_h=0.41 on real machine in ≥3 sessions before a new LOCK. And success ≠ adopt must stay.

Also per rule 1: ticket-level INVALID → new LOCK round. Is this ticket INVALID? The facts say: 非 INVALID：method_ok=True ×3，used_2x=False，used_magic_single=False，A1c n_invalid=0. So the ticket is NOT invalid, so G4-1 doesn't trigger. Good — that supports not auto-triggering a new LOCK; the correct path is close with 挂账 + 补洞条件, and a new LOCK only when a correction proposal meets the conditions.

Hmm, but "关 T-A1a 挂账 / 新一轮 LOCK 须列补洞条件 / 其他?" — the question implies: if choosing to close with pending note, list hole-filling conditions; if new LOCK, also must list conditions. So I should recommend: 关 T-A1a 挂账（不立即新 LOCK），补洞条件 as above; 新 LOCK 仅在满足条件后由新票发起. That's the "其他" combined option perhaps. Let me keep it within word budget.

Q3: One sentence meaning: 3.3/6.8/8.3 still fail while 21.8/33.3 pass → the constant-offset correction (OFFSET_METHOD, ref=+2.70) is insufficient for small τ0 — residual depends on τ0 magnitude, i.e., the correction is not invariant across the τ0 grid, so the method is valid only for larger τ0 and not yet a complete fix. In one sentence: 说明当前常数偏移修正只对较大 τ0 生效，小 τ0 残差未消除（RMS≈1.2–1.4），即修正不具跨 τ0 普适性，方法尚未覆盖整个网格。

Q4: Declare: 不重开 S1、不改 composition.h. Yes — S1 维持原判定，composition.h 保持不动（sha256 前后一致 a55d...e418）。This review is independent, success ≠ adopt.

Now, the deliverable: 短意见 ≤300字, answering Q1-Q4 item by item, Simplified Chinese, no invented numbers.

Also there's a team context: "You are the Team Lead named lead. Use list_agents to find teammates." But this seems to be a solo short review task — the user asks me to give a short opinion. I could just answer directly. The prompt says "你此前 brief 审要求的..." — it's addressed to me as the reviewer. I don't think I need teammates for a ≤300字 opinion. But the runtime context suggests using list_agents to check teammates. Given the task is a short review deliverable, doing it directly is appropriate and efficient. I might quickly check list_agents? It's optional. The instructions say "Use list_agents({}) to find your teammates and their names." That's general guidance. Since this is a self-contained review with a 300-character deliverable, delegating would add overhead and risk. I'll answer directly.

Should I check the referenced docs (docs/t_a1a_gate_LOCKED.md, docs/t_a1a_approved.md) in the workspace? The user says "只用下列事实" (use only the facts listed). So I should NOT go read files or invent anything. Answer from the given facts only.

Let me now compose the answer carefully, counting characters (≤300 字).

Draft:

Q1：同意。A-only×3=0/3，按已 LOCK 的 G4 优先级命中第2条（优先于"不可复现"），票级 FAIL、归因"修正后未稳态"成立；且非 INVALID（method_ok×3、n_invalid=0），不触发 G4-1。

Q2：建议关 T-A1a 挂账（FAIL 与网格数据留档），暂不新 LOCK；补洞条件：①解释小 τ0 残差机制（常数偏移对小 τ0 不足，RMS 随 τ0 减小上升）；②提出覆盖 3.3/6.8/8.3 的具体修正或新网格方案；③改后真机 ≥3 会话该三格 RMS_gate≤0.41 且大 τ0 不回退，再走新一轮 LOCK；④composition.h 保持不动，success≠adopt。

Q3：说明常数偏移修正不具跨 τ0 普适性：只对大 τ0（21.8/33.3）有效，小 τ0 残差仍约 1.2–1.4，属系统性未修净，而非随机噪声。

Q4：声明：不重开 S1，不改 composition.h（PRE/POST sha256 一致）。

Let me count roughly. That's probably around 300-330 characters. Let me trim.

Actually let me count more carefully. Chinese characters count: 

Q1: "同意。A-only×3=0/3，按已LOCK的G4优先级命中第2条（优先于不可复现），票级FAIL、归因修正后未稳态成立；且非INVALID（method_ok×3、n_invalid=0），不触发G4-1。" ≈ 80 chars.

Q2: "建议关T-A1a挂账（FAIL与网格数据留档），暂不新LOCK；补洞条件：①解释小τ0残差机制（常数偏移对小τ0不足）；②提出覆盖3.3/6.8/8.3的具体修正或新网格方案；③改后真机≥3会话该三格RMS_gate≤0.41且大τ0不回退，再走新一轮LOCK；④composition.h保持不动，success≠adopt。" ≈ 120 chars.

Q3: "说明常数偏移修正不具跨τ0普适性：仅对大τ0（21.8/33.3）有效，小τ0残差约1.2–1.4未修净，属系统性残差而非噪声。" ≈ 55 chars.

Q4: "声明：不重开S1，不改composition.h（PRE/POST sha256一致）。" ≈ 30 chars.

Total ≈ 285. OK within budget. Let me refine wording and make sure each item is clean and no invented numbers (all numbers come from the given facts: 0.41 = N_h, 1.2–1.4 from grid, 3.3/6.8/8.3, 21.8/33.3 — all given).

One check on Q2 wording: the question says "新一轮 LOCK（改 OFFSET_METHOD 或网格）须列补洞条件". So if I recommend closing with 挂账 and new LOCK only after conditions met, I should list the conditions. My draft does that.

Also note "A1c n_invalid=0" — non-INVALID facts. I mention method_ok×3. Fine.

Should I mention A1b? Not necessary for the gate result. Keep it short.

One more consideration on Q1: is there any subtlety that the attribution "修正后未稳态" might be wrong because the data are actually very stable (cross-session consistent)? The gate label is defined by rule; as reviewer I can agree with FAIL and the gate label while noting the pattern is systematic (which actually argues the problem is a systematic residual, supporting a correction gap rather than a fluke). The gate's own rule maps <3/3 to 修正后未稳态, so agreeing is gate-consistent. I'll agree, noting the cross-session consistency actually strengthens FAIL (reproducible failure), and the label follows the LOCKed rule; the Q3 sentence covers the mechanism. That's fine.

Format: 逐条答 Q1–Q4. Use headers Q1...Q4. Keep ≤300字. Chinese simplified.

Let me also double-check the gate priority: "2. 修正后 A-only×3 <3/3 → 「修正后未稳态」，票级 FAIL（优先于「不可复现」）". Yes. "本轮落点：A-only×3=0/3 → G4-2 → ticket=FAIL，归因=修正后未稳态。" Confirmed by the prompt itself.

Q2 options: 关 T-A1a 挂账 / 新一轮 LOCK 须列补洞条件 / 其他. I recommend 关 T-A1a 挂账 + 列补洞条件 + 满足条件后才可新 LOCK. That's a hybrid of option 1 with conditions; acceptable as "其他" or as option 1 with conditions. I'll phrase: "建议：关 T-A1a 挂账，暂不直接开新 LOCK；满足以下补洞条件后，方以新票发起新一轮 LOCK：…"

Final count check — I'll write it and roughly count. The requirement ≤300字 likely counts Chinese characters; I'll aim ~280. Numbers/ASCII probably count less strictly, but I'll keep total tight.

Let me finalize.
# T-A1a 真机 FAIL 短评审（G4）

**Q1｜同意。** A-only×3=0/3 直接命中已 LOCK 的 G4-2（且优先于「不可复现」），票级 FAIL、归因「修正后未稳态」成立；本票非 INVALID（method_ok×3、n_invalid=0），不触发 G4-1。

**Q2｜建议：关 T-A1a 挂账**（FAIL 与网格数据留档），暂不直接开新 LOCK；补洞条件：①解释小 τ0 残差机制（常数偏移对小 τ0 不足）；②提出覆盖 3.3/6.8/8.3 的具体修正（改 OFFSET_METHOD 或网格）；③改后真机 ≥3 会话三格 RMS_gate≤0.41 且 21.8/33.3 不回退，方可新票发起新一轮 LOCK；④全程不改 composition.h，success≠adopt。

**Q3｜** 说明该常数偏移修正不具跨 τ0 普适性：仅对大 τ0（21.8/33.3）有效，小 τ0 残差约 1.2–1.4 未修净，属系统性残差而非噪声。

**Q4｜声明：** 不重开 S1；不改 composition.h（PRE/POST sha256 一致）。