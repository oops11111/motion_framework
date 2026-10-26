The user is asking me to write a Chinese technical review document for a "动效框架" (motion effects framework) project. This is a pure document-writing task based solely on the provided fact material. The task says:

- Deliver full text for coordination to write to `docs/deepseek_m2_h5_seed_review.md`
- Structure: 元信息 (meta info), 块 1 (judgment criteria review against DRAFT G2), 块 2 (call site inventory), optional third section
- Strict requirements: Simplified Chinese, conclusion first, tables allowed, quote measured numbers verbatim, no fabrication

Let me check the working directory context. Current working directory is "C:\Users\admin\Temp". The task says "交付全文，供协调落盘 docs/deepseek_m2_h5_seed_review.md" — deliver the full text for coordination to write to the file. So my main deliverable is the Markdown text in my response. The user says "请输出完整 Markdown 正文（从一级标题开始），不要包在代码围栏里。"

So the key deliverable is the full Markdown text output in my final response, NOT wrapped in code fences. Should I also write the file? The task says "交付全文，供协调落盘" — deliver the full text, for coordination to write to disk. This suggests the user wants the text delivered; the actual writing is coordinated (perhaps another agent writes it). The instruction "请输出完整 Markdown 正文（从一级标题开始），不要包在代码围栏里" is explicit: output the full Markdown body starting from level-1 heading, not in a code fence.

Should I write the file myself? The working directory is C:\Users\admin\Temp, which is not a repo containing docs/. There's no indication the repo is here. The task explicitly says "交付全文，供协调落盘" — meaning my delivery is the text, and the coordination (落盘) happens elsewhere. Given the doc must go to `docs/deepseek_m2_h5_seed_review.md` which doesn't exist in this Temp directory, and the task structure is a "review document" delivery, I'll output the full Markdown as my answer. That's what's asked: "请输出完整 Markdown 正文（从一级标题开始），不要包在代码围栏里。"

Also, is there a team here? I'm the Team Lead. The user's message is direct: write the document. No need to spawn teammates. Just deliver.

Let me check the requirements carefully:

### 元信息
- 票：M2-H5-SEED 判据复核 + 调用点清单（文档 only，DRAFT 闸门未 LOCK，本交付不授权采数）
- 依据文件列表
- composition.h 当前 sha256（材料已给，须原样写出）: `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`
- 硬禁重申

依据文件列表 from the material:
- docs/m2_h5_seed_gate_DRAFT.md (闸门 DRAFT)
- docs/m2_01_result.md (M2-01 结题)
- artifacts/m2_01_g3_h1h5_audit.txt (G3 审计)
- artifacts/win_r2_p4_b2vel.txt (#37 解读摘要)
- 仓库静态扫描（调用点清单，本机 sha 已核）

### 块 1 — 判据复核（对照 DRAFT G2）
Must answer each:
1) PASS 定义「3/3 会话中位 |v_seed/v_screen| ∈ [0.8,1.2] 且无会话 ≥2.0×；相对 #37 的 2.600 已消除」是否够硬？缺口？
2) 建议 LOCK 前写死：n（打断次数或帧）、会话独立性、INVALID 边界（仍用 spring.h 影子 v 等）——给出可粘贴的写死草案
3) 是否需要保留 RMS/位置辅证，还是只锁速度比？给出明确推荐与理由（对照 #37：shadow 臂速度比 2.6 与 J_vis/Δv_vis 同向变差；wgc 臂速度比≈0.8 但仍有 J/Δv 未达标——说明速度比是必要非充分）

Block 1 must first give 总评：有条件同意 / 不同意 / 需补洞（choose one), then list gaps.

Analysis of 1):
PASS 定义 strength:
- Positive: uses per-session median (robust to outliers), ratio window [0.8,1.2] matches wgc arm observation (0.796 from C/… wait, wgc ratio=0.796 which is below 0.8! Interesting). Let me check: wgc med_vSet=183.5 / med_v_pre=230.5 → ratio = 183.5/230.5 = 0.7961... The material says "wgc 臂速度比≈0.8" and in the audit "ratio=0.796". Hmm, but the PASS window is [0.8, 1.2]. wgc at 0.796 would technically be below 0.8! This is a subtle point worth noting: the best arm (#37 wgc) had ratio=0.796, marginally below the proposed lower bound 0.8. That's a potential gap worth flagging: the PASS window lower bound 0.8 excludes the empirically best observed arm (0.796). Actually wait — is that a real gap? Let me be careful. The ratio definition: |v_seed/v_screen|. For wgc, v_seed was the WGC screen velocity from the last two frames, and v_screen was presumably the actual on-screen velocity after retarget. So 0.796 means the seed slightly underestimates the screen velocity (approximately 0.8, "wgc 臂速度比≈0.8" per D). Since PASS requires ∈[0.8,1.2], the observed best arm at 0.796 sits just outside. I should flag this as a boundary consistency gap: either the threshold needs justification or the wgc-based seed needs a slight calibration. This is a legitimate observation from the given numbers. I must be careful not to overstate: material says "wgc 臂速度比≈0.8 但仍有 J/Δv 未达标——说明速度比是必要非充分". The user themselves frames 0.796 as ≈0.8. So flagging the boundary tension is fair and valuable.

- "无会话 ≥2.0×" caps worst-case — shadow's 2.600 is well above 2.0, so it eliminates the #37 shadow pathology. Good.
- 3/3 sessions with median in window: requires consistency across sessions. Good — guards against cherry-picking.
- Relative to #37: 2.600 eliminated.

Gaps (缺口):
- n not fixed in DRAFT (G2 says "每会话 n 按 LOCK 写死（建议≥12 次打断或沿用 #37 臂对照结构）" — it's DRAFT, must be locked).
- 会话独立性 not defined (what counts as a session; same process? restart? different arms?)
- INVALID boundary partially listed but needs lock: "仍用 spring.h 影子 v" listed; but also "INVALID" for median computation when v_screen=0 → ratio division by zero (zero arm ratio=0.000 when vSet=0; unset arm ratio=nan). What if v_screen ≈ 0 (e.g., interrupt at rest)? Need a guard for near-zero denominator. Material doesn't explicitly give that, but the zero/unset arms show ratio=0.000/nan values, so the ratio metric itself has degenerate behavior at v=0. I can flag this as a gap requiring a written-down boundary (e.g., exclude windows where |v_screen| < ε, or predefine handling). But careful: I must not invent specific numbers not given. I can propose the draft includes a threshold to be chosen, without inventing the value. In the draft I give, I should leave placeholders like `<ε>` or define per LOCK. Actually, giving a paste-able draft with placeholders is acceptable — the user asked for "可粘贴的写死草案". I should avoid inventing hard numbers not in the material; use the material's numbers (≥12, 3/3, [0.8,1.2], ≥2.0×) and leave placeholders where the material doesn't specify.
- PASS uses median but the per-session "无会话≥2.0×" is on what statistic? "无会话 ≥2.0×" — is it median or any single sample? Ambiguous. Should lock: no session's median ≥2.0×, and optionally no single sample ≥2.0×. Flag as ambiguity.
- Denominator issue: |v_seed/v_screen| — which is the reference? v_screen is the measured on-screen velocity. How is v_screen measured? Not specified in G2 (only "屏上 NaturalMotion 轨迹" from B). Measurement window (before/after interrupt, frame window) not locked.
- What about the "滞后补偿" mentioned in D ("并论及滞后补偿，属实现细节，本票复核不必冻结")? The task says "本票复核不必冻结" — so I can mention it's out of scope but note the ratio window must tolerate measurement lag; keep brief.
- FAIL definition: "样本足未达 PASS" — needs n and session count locked, and retry/backfill rules ("追加补测拼凑" is INVALID per material). Also "样本足" depends on locked n.
- The ratio lower bound 0.8 vs wgc 0.796 tension (as above).

Total assessment: 需补洞 (needs holes filled) or 有条件同意? The user says "块 1 先给总评：有条件同意 / 不同意 / 需补洞（择一）". Given DRAFT is explicitly NOT LOCKED and n/会话独立性/INVALID are not yet written down, the honest verdict is 需补洞 — the direction is right (speed-ratio core is necessary), the numbers are anchored to #37 (2.600 vs 0.796), but the LOCK must first write down n, session independence, INVALID boundaries, and the near-zero denominator handling before it can be executed. Hmm, or 有条件同意 — "agree on condition that holes are filled before LOCK". Both plausible. Let me pick 需补洞 since the draft literally leaves n/会话独立性/INVALID as "LOCK 写死" placeholders — i.e., holes exist in the current DRAFT text itself. Then list the gaps and the draft. Actually "有条件同意" would also fit: agree with conditions = fill the holes. The strongest and most precise: 需补洞. The PASS definition itself is directionally sound but the current DRAFT cannot be executed as-is because the core sampling parameters are unresolved. I'll go with 需补洞.

For 2) — paste-able draft. Let me draft:

```
G2-LOCK 写死草案（建议并入 DRAFT G2 后 LOCK）：

1. 样本量：每会话 n=12 次打断/重定向（沿用 #37 臂对照结构：n=12/臂）。
2. 会话定义与独立性：会话 = 一次独立运行的探针场景（进程级重启或场景完整重建，臂间互不共享弹簧状态）；有效会话 ≥3，且 3 个通过会话须来自 3 次独立运行。
3. 臂结构：至少保留 {修复臂, zero/IV=null+0 降级臂} 对照；可沿用 #37 {base,shadow,zero,unset,wgc} 结构。
4. 指标：每会话对 n 次打断逐一计算 r_i = |v_seed,i / v_screen,i|，取会话中位 med_r。
   - 分母退化：|v_screen,i| < ε 的样本点单独处理——（ε 与处理方式由 LOCK 时写死，例如剔除并记录，或并入 near-rest 规则；本复核不代定数值）。
5. PASS：3/3 会话 med_r ∈ [0.8, 1.2] 且 3 个会话的 med_r 均 < 2.0（相对 #37 shadow 2.600 已消除）。
6. FAIL：有效样本数足（n 与会话数达标）但未达 PASS。
7. INVALID（任一即整票 INVALID，不允许补测拼凑）：
   a. composition.h sha256 偏离 a55d...418；
   b. 未授权采数；
   c. seed 仍来自 spring.h 影子闭式 v（即 seed 赋值语句仍取 mf_spring_at/spring.h v(t)）；
   d. 指标口径偏离本 G2-LOCK（改中位为均值、改臂、改窗口、改分母等）；
   e. 会话数 <3 或任一会话有效 n < 12；
   f. 追加补测拼凑（FAIL 后回填数据）。
```

Wait — regarding "无会话 ≥2.0×" — I should be careful about 5: original says "3/3 中位∈[0.8,1.2] 且无会话≥2.0×". If med_r ∈ [0.8,1.2] already, then med_r < 2.0 automatically. So "无会话≥2.0×" presumably refers to something else — perhaps any session whose median ≥2.0×, or any single sample ≥2.0×. Since [0.8,1.2] implies <2.0 for medians, the "无会话≥2.0×" clause only adds meaning if it applies to individual samples or to sessions that miss the window. Actually reread: "PASS：3/3 中位∈[0.8,1.2] 且无会话≥2.0×". Hmm, the most sensible reading: it's a redundant-but-explicit guard, or it applies to raw samples. Since the draft is ambiguous, my review should flag this ambiguity and my locked draft should disambiguate. I'll write: 每会话任一单样本 r_i ≥ 2.0 即该会话计未达标？ That would be very strict. Alternatively: "无会话中位 ≥2.0×（防御性冗余条款；在 [0.8,1.2] 下自动满足）"。Better: flag ambiguity in the gap list, and in the draft make it explicit as applying to single samples: "且 3 会话中均无单样本 ≥2.0×（若采纳较严口径）或（若按冗余口径）删除该条"。Hmm, that's wishy-washy. Let me just propose one concrete locked wording and note the alternative. The user asked for a paste-able draft — I should give one decisive draft, and separately note the ambiguity and my choice.

Actually, thinking about it more: shadow arm had med ratio 2.600. The clause "无会话≥2.0×" — for a session-level statistic. If medians all in [0.8,1.2], then no session has median ≥2.0 trivially. The clause is then either (a) redundant belt-and-braces, or (b) intended as a per-sample bound. I'll note this in gaps, and in the draft write it as a per-sample guard with a session-level redundancy note. Let me pick: "任一会话任一单样本 ≥2.0× → 该会话未达标（若判定过严，LOCK 时可降为会话中位口径并保留 [0.8,1.2] 主判据）"。Hmm. To keep it decisive: I'll draft the stricter single-sample version and mark the softer reading as the fallback. Actually simpler and defensible: keep the draft's original intent as session-level and note it's redundant with [0.8,1.2]; recommend clarifying to per-sample bound. In my draft I'll write it explicitly as per-sample, since that's the only reading that adds information.

Regarding ε: the material doesn't give a number. I'll use placeholder "ε（LOCK 时写死，建议按 v_screen 测量噪声量级取定）" — no invented value. Also v_seed=0 cases: zero arm ratio=0.000 — with IV=0 seed, |0/v_screen| = 0, which is "well-calibrated ratio" but terrible outcome? Wait no — zero arm had ratio 0.000 (vSet=0), and D says "zero/unset 近似，IV 不设≡0". And wgc was best. So ratio=0 is NOT a pass even though it's in a sense "not over-accelerated"? Interesting: ratio metric punishes zero-seed (0 is outside [0.8,1.2]), correctly, because zero seed gives wrong velocity magnitude (though direction of error differs from shadow's over-acceleration). Also unset→nan: ratio undefined when no seed at all. So the metric handles these. Good — the ratio window [0.8,1.2] requires the seed to actually match screen velocity, not merely avoid overshoot. That's a strength I can note.

Also note: #37 shadow J_vis median=5.30, Δv_vis median=272, peak median=100.3 (worst); wgc J_vis=2.75, Δv_vis=64, peak=41.5 (best but still not reaching J≤0.5 / Δv≤20). Use these numbers verbatim in section 3.

For 3) — recommendation: 保留 RMS/位置辅证 as secondary gating, but only lock the speed ratio as the PASS criterion (speed ratio necessary, not sufficient). Reasoning:
- Speed ratio is necessary: shadow 2.6 with worst J/Δv; wgc 0.796 with best J/Δv — same-direction worsening shows ratio tracks the failure mode (over-acceleration → seed speed mismatch).
- But not sufficient: wgc ratio ≈0.8 yet J_vis=2.75, Δv_vis=64 still fail J≤0.5 / Δv≤20. So ratio passing alone doesn't guarantee visual quality targets.
- Recommendation: 主判据锁速度比（本票范围：seed 路径修复，判据直击 seed 问题）；RMS/位置与 J/Δv 列为辅证观测（记录不作 PASS/FAIL 门槛），或挂到后续票（如 WGC 滞后补偿、更精细 IV 估计算法）。Avoid double-locking: if we lock both speed ratio AND J/Δv≤0.5/20, then the fix would fail even in the best observed case (wgc 41.5 peak), making the gate unsatisfiable by the current fix scope — that would be wrong gating. Since the fix scope is "seed 路径", the PASS should test the seed property (speed ratio); the visual residual J/Δv belongs to the broader trajectory-quality gate. This is a strong, well-grounded recommendation. The user explicitly framed: "wgc 臂速度比≈0.8 但仍有 J/Δv 未达标——说明速度比是必要非充分" — so they want this exact argumentation.

Careful nuance: the user says "wgc 臂速度比≈0.8" — material numbers: 183.5/230.5=0.796. I should quote the actual 0.796 and can say "≈0.8". Fine.

### 块 2 — 调用点清单
1) All call sites in repo feeding spring.h / closed-form v to compositor InitialVelocity: path + symbol-level (method name / arm name / assignment statement level).
2) Layers allowed to change this ticket: demo / harness / controller; explicitly which files may move, which forbidden.
3) Reiterate composition.h sha must keep the given value; composition.h's PlanOverride/PlanGraph using spring.h::At to fill CompositorSpring.fromVelocity is FORBIDDEN to change this ticket, only register as 挂账/出票外.

From E1 (allowed to change):
- win_softdemo_src/MainPage.xaml.cs (and live 同源 MotionSoftDemo/MainPage.xaml.cs):
  - CompStartSpring (~L519+): `a.InitialValue=(float)v0; a.InitialVelocity=(float)vel0` — v0/vel0 from shadow state (comment says spring.h)
  - RunAlignProbe: interrupt uses shadow as InitialValue/InitialVelocity
  - RunInterruptSeedProbe: `MfProbe.mf_spring_at(... out seedPos, out seedVel)` → `a2.InitialVelocity=(float)seedVel`
  - RunRateFoldProbe: `a.InitialVelocity=(float)ivel` (ivel from mf_spring_at)
  - RunR2P11Probe: arm `Bv` → `r.InitialVelocity=(float)shV`; arm A 类 → shX+shV (mf_spring_at at tS)
  - RunR2P10Probe / RunR2P9Probe: same pattern, A/B arms shV
  - RunR2P8Probe: arm name starting with A → InitialValue=shX, InitialVelocity=shV
  - RunR2P7Probe: arm A0/Am20 → shX+shV
  - RunR2P2Probe: arm A → shX+shV
  - RunR2P4Probe (ticket source #37): arm `shadow` → `r.InitialVelocity=(float)shV; vSet=shV`; arms={base,shadow,zero,unset,wgc}; evidence source
  - RunR2PostProbe: interrupt path `a2.InitialVelocity=(float)seedVel`
  - Suite RunSuite interrupt segment: shadow seeded retarget (comment "shadow seeded")
- MotionSoftDemo/R3Probe.cs (and win_softdemo_src/R3Probe.cs copy):
  - RunR3P12Probe / Trial: arms `Bv`/`Aex` → `a.InitialVelocity=(float)shV` (mf_spring_at at tSeed); Aex also InitialValue=shX
  - RunR3E3Probe: arm `Bv` → InitialValue unset + InitialVelocity=shV
  - RunR350Probe: arm `Bv` → same; plus d1 overlay `InitialVelocity=(float)ivel` (ivel from mf_spring_at, ε̂ sweep param)
  - Note: some mf_spring_at annotated `// log only; do NOT seed` — these are not seed call sites; mark "仅日志" in the inventory.
- Bridge: `MfProbe.mf_spring_at` ← mf_probe.cpp / fm_capi export, internally calls spring.h closed-form; when changing seed strategy, change the caller assignment, not spring.h itself.

E2 controller layer (allowed to touch "seed 路径" semantics with caution):
- code/controller.h: `Controller::Retarget` takes over value/velocity from current owner (software path spring state). This is AppThread/software controller semantics, NOT directly writing WinRT `SpringScalarNaturalMotionAnimation.InitialVelocity`. If this ticket wants to solidify "no shadow IV" into controller API contract, may touch docs + controller contract; but must NOT use the chance to change composition.h.

E3 forbidden (register only):
- code/composition.h: PlanOverride (~L304–313) `sp.At(elapsed)` → `pp.spring.fromVelocity = st.velocity`; PlanGraph spring segment (~L357–368) similar. This is the production lowering path to CompositorSpring, forbidden to change this ticket; aligning compositor math to spring.h or changing that seed is out-of-ticket (出票外).
- code/spring.h: pure math, not a "feed compositor IV" call site; don't touch unless separate ticket.
- Freezes/bans: 不重开 S1/R-3; 不解冻 Bv; 成功≠采用.

E4 table: may move vs forbidden.

### 可选第三段
One incremental sentence for docs/deepseek_m2_02_api_draft.md: 「H5 未修前 C# 投影勿接影子 IV」— place it in a section. Suggestion: §3.4 or §3.5. The material F says: "请给一句增量警告位置建议（例如 §3.4 或 §3.5）". So I give the paste-able sentence and say which section. I need to pick one. Without seeing the doc, I'd suggest §3.4 (假设与警告 / 未验证清单 area) or §3.5. Since I can't read the file (it's not in the workspace — C:\Users\admin\Temp), I'll recommend §3.4 或 §3.5 with a reason: place near 合成器路径接续假设/未验证项. Let me phrase: 建议放 §3.4（合成器路径接续/未验证假设处）；若该节为纯接续假设则放 §3.5（风险与禁忌清单处）。Since I must give one decisive suggestion, I'll recommend §3.4 and note fallback §3.5. Actually the user said "例如 §3.4 或 §3.5" — they want a concrete suggestion. I'll say: 建议置于 §3.4；若该节已满或专述接续假设，则 §3.5。Hmm, keep it simple: "建议放 §3.4（若该节是合成器接续/未验证假设），否则 §3.5" — fine.

The exact sentence to paste: 「H5 未修前 C# 投影勿接影子 IV」— the user says "可直接粘贴" that sentence. Maybe I can expand slightly but they gave the exact sentence in quotes. I'll provide that sentence plus a one-line rationale for the doc context. Careful: "给 ... 的增量一句（可直接粘贴）：「H5 未修前 C# 投影勿接影子 IV」" — so the deliverable sentence is exactly that. I can add a lead-in like "建议在 §3.4 末尾追加一句：" then the sentence. Maybe expand to "H5 未修前 C# 投影勿接影子 IV（spring.h 闭式 v 作 InitialVelocity，见 M2-H5-SEED 复核）" — but adding extra text beyond the quoted sentence risks overstepping. The user said the incremental sentence is that quoted one; I can give it plus optional annotation. I'll give the sentence verbatim and separately note a suggested parenthetical reference, clearly marked as optional. Better to keep exactly what they gave, and mention in prose where it goes.

### 元信息 content
- 票: "M2-H5-SEED 判据复核 + 调用点清单（文档 only，DRAFT 闸门未 LOCK，本交付不授权采数）"
- 依据文件列表:
  - docs/m2_h5_seed_gate_DRAFT.md（闸门 DRAFT，STATUS=DRAFT NOT LOCKED）
  - docs/m2_01_result.md（M2-01 结题，G3 PRIMARY=H5 Seed 源）
  - artifacts/m2_01_g3_h1h5_audit.txt（G3 审计，四臂数字）
  - artifacts/win_r2_p4_b2vel.txt（#37 解读摘要/证据）
  - 仓库静态扫描（E 调用点清单，本机 sha 已核）
- composition.h 当前 sha256（原样写出）: a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418
- 硬禁重申: 不改 composition.h（sha 锁死为上述值）；不重开 S1/R-3；不解冻/不升级 Bv；不重开 R-2；成功≠采用；本票只改 seed 路径（demo/harness/控制器），不对齐 compositor 弹簧数学到 spring.h；未授权不采数。

The user says "硬禁重申" — restate the hard bans. From A G0: 不重开 S1/R-3；不解冻/不升级 Bv；不重开 R-2；不改 composition.h sha；成功≠采用；本票只改 seed 路径，不对齐 compositor 弹簧数学到 spring.h. From E4 冻结/禁令: 不重开 S1/R-3；不解冻 Bv；成功≠采用. I'll restate all.

Also G3 禁令同硬边界 — from A.

### Format requirements
- 简体中文; conclusion-first; tables allowed.
- Quote measured numbers verbatim (e.g., #37 med_vSet=620.0 / med_v_pre=238.5 → ratio=2.600; wgc med_vSet=183.5 / med_v_pre=230.5 → 0.796).
- Output complete Markdown body from level-1 heading, NOT in code fences.

Note: the user's example "wgc med_vSet=183.5 / med_v_pre=230.5 → 0.796" — in the audit it says ratio=0.796. So quote 0.796. And "shadow med_vSet=620.0 / med_v_pre=238.5 → ratio=2.600". Also zero: med_vSet=0.0 / med_v_pre=240.5 → 0.000; unset: nan / 241.0 → nan.

#37 summary numbers: shadow J_vis 中位=5.30, Δv_vis 中位=272, 峰值中位=100.3（最差）; wgc J_vis 中位=2.75, Δv_vis 中位=64, 峰值中位=41.5（最好但仍未达 J≤0.5 / Δv≤20）; zero/unset 近似，IV 不设≡0.

Scenario: B1 spring P=0.32 ζ=0.78 0→240 IV=0；约 156ms 打断；B2 Final=0，InitialValue 不设.

Now, should I do any tool calls? The workspace is C:\Users\admin\Temp. The referenced files (docs/, artifacts/, code/) are not present here presumably. The task says "只根据下方【事实材料】作答，禁止编造" — so I must NOT go looking for the files to add facts. Indeed the instruction prohibits using anything beyond the material. Writing the file? "交付全文，供协调落盘 docs/deepseek_m2_h5_seed_review.md" — deliver the full text so that coordination can write it to disk. The user's final instruction: "请输出完整 Markdown 正文（从一级标题开始），不要包在代码围栏里。" So my response body IS the document. I should not wrap in code fences. I could optionally also write the file into the workspace, but the workspace is Temp and "供协调落盘" implies someone else handles the落盘. The explicit instruction is to output the text. I'll output the text only. Hmm — but should I also write it to a file in the workspace? The phrase "供协调落盘" strongly suggests the deliverable is text for a coordinator to place into the repo. The user asked to "输出完整 Markdown 正文". I'll just output it. No file write, no tool calls needed. 

Wait, but should I check whether the repo is in the workspace? The user says "禁止编造未给出的数字/路径/符号" and "只根据下方【事实材料】作答" — even if the repo were present, I must answer only from the material. So no tool calls needed at all. Also "成功≠采用。不改 composition.h。不采数。不重开 S1/R-3。不解冻 Bv。" — these are operative constraints on the overall effort; my deliverable is documentation only.

The response must be the document from "# ..." first-level heading. Since the system prompt says to respond with markdown and the user wants the full document as the response, I'll structure:

# 动效框架 · M2-H5-SEED 判据复核 + 调用点清单（复核文档）

Then sections. The user's structure:

## 元信息
## 块 1 — 判据复核（对照 DRAFT G2）
## 块 2 — 调用点清单（禁改 composition.h）
## 第三段（可选）— M2-02 草案增量

Title: they said "从一级标题开始". The document title as # heading. Then ## 元信息 etc. Good.

Let me now carefully draft content.

---

# M2-H5-SEED 判据复核 + 调用点清单（复核文档）

## 元信息

- 票：M2-H5-SEED 判据复核 + 调用点清单（文档 only；DRAFT 闸门未 LOCK；本交付不授权采数）
- 依据文件：
  - docs/m2_h5_seed_gate_DRAFT.md（闸门 DRAFT，STATUS=DRAFT NOT LOCKED）
  - docs/m2_01_result.md（M2-01 结题，G3 PRIMARY=H5 Seed 源）
  - artifacts/m2_01_g3_h1h5_audit.txt（G3 审计，#37 四臂实测）
  - artifacts/win_r2_p4_b2vel.txt（#37 解读摘要，证据源）
  - 仓库静态扫描（本交付 E 节调用点清单，本机 sha 已核）
- composition.h 当前 sha256（原样）：`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`
- 硬禁重申（本次及后续任何步骤）：
  1. 不改 composition.h（sha256 保持上述值，任何改动即 INVALID）。
  2. 不重开 S1 / R-3；不重开 R-2。
  3. 不解冻、不升级 Bv。
  4. 成功 ≠ 采用。
  5. 本票只改 seed 路径（demo / harness / 控制器），不对齐 compositor 弹簧数学到 spring.h。
  6. 未授权采数禁止；本交付（文档 only）不授权任何采数。

## 块 1 — 判据复核（对照 DRAFT G2）

### 总评：需补洞

DRAFT G2 的方向正确、判据主体成立（以会话中位速度比为核心，窗口 [0.8,1.2] 且无 ≥2.0× 会话，直接锚定 #37 实测的 2.600 vs 0.796），但当前 DRAFT 文本中 n、会话独立性、INVALID 边界均为「LOCK 写死」占位，分母退化（v_screen≈0）无处理规则，「无会话 ≥2.0×」的统计口径有歧义。按现状不可执行，必须补洞并 LOCK 后才能采数。

Wait — "需补洞" chosen. Good.

#### 1) PASS 定义是否够硬

PASS 定义原文：「3/3 会话中位 |v_seed/v_screen| ∈ [0.8,1.2] 且无会话 ≥2.0×；相对 #37 的 2.600 已消除」。

够硬的部分：
- 会话中位：对单次打断的异常值稳健，比均值口径更适合打断探针。
- [0.8,1.2]：下限 0.8 意味着 seed 必须真实匹配屏上速度，仅「不超调」不达标——zero 臂 ratio=0.000 直接被排除，判据因此同时排除了「影子过加速」与「欠 seed」两类错误。
- 3/3：要求全部会话通过，禁止挑会话。
- ≥2.0× 上限与 #37 锚定：shadow 臂 med_vSet=620.0 / med_v_pre=238.5 → ratio=2.600 是已知病态，2.0× 上限把它与正常区隔开，判据与已知失败模式一一对应。

缺口：
- G1: n 未写死（DRAFT 仅「建议≥12 次打断或沿用 #37 臂对照结构」），样本量不足时「样本足」无定义，PASS/FAIL 无法判定。
- G2: 「会话」的独立性无定义：同一进程内重复跑 3 次算不算 3 个会话？臂间共享弹簧状态与否？不写死则 3/3 可被同一次运行拼出。
- G3: 分母退化：v_screen≈0 时比值发散/无定义（unset 臂 ratio=nan、zero 臂 ratio=0.000 已在 #37 出现），DRAFT 没有对 |v_screen| 过小样本的处理规则；不写死则这类样本既可被剔除也可被计入，口径可漂移。
- G4: 「无会话 ≥2.0×」统计口径歧义：若指会话中位，则该条与「中位∈[0.8,1.2]」冗余；若指单样本，则约束强得多。必须二选一写死。
- G5: 0.8 下限与 #37 最优臂的边界摩擦：wgc med_vSet=183.5 / med_v_pre=230.5 → ratio=0.796，是四臂中唯一贴近窗口的（且 J/Δv 最优），却恰好低于 0.8。窗口下限若不做 0.796 级别的灵敏度说明，修复方案可能被门槛卡在最优解之下。需在 LOCK 时明确：接受 0.8 下限并预期 wgc 类 seed 需小幅校准，或将下限调整并说明理由。
- G6: v_screen 的测量定义（哪几帧、打断前/后）未写死；D 提到滞后补偿属实现细节可不必冻结，但测量窗口本身属于判据口径，必须写死。
- G7: FAIL 里「样本足」依赖未锁定的 n 与会话数；「追加补测拼凑」已被 DRAFT 列为 INVALID，但「样本足」必须先有数字才能判断是否拼凑。

Hmm wait, G5 — should I be careful? "wgc 臂速度比≈0.8" is stated in material D and user's prompt. 0.796 < 0.8. This is a genuine observation. The user's prompt for block 1 question 1 asks "是否够硬？缺口？" — I can include this. But I must not overreach: I can say the boundary tension exists and LOCK should decide. Yes, include as gap. It's derived purely from given numbers.

Also careful about "INVALID 边界（仍用 spring.h 影子 v 等）" in question 2 — the draft covers in question 2. Fine.

#### 2) LOCK 前写死草案（可粘贴）

I'll present as a block that can be pasted into the DRAFT. Since the whole answer can't be in a code fence... The user said the Markdown body shouldn't be wrapped in code fences — that refers to the whole deliverable. Inside, I can use a fenced code block for the paste-able draft? Hmm. "请输出完整 Markdown 正文（从一级标题开始），不要包在代码围栏里" — the deliverable as a whole shouldn't be in a code fence. Using an internal code fence for a paste-able G2-LOCK clause is legitimate Markdown and improves copy-paste fidelity. But risk: the coordinator pasting the whole doc might then have embedded fences. I think an internal fenced block for the "可粘贴的写死草案" is acceptable and actually desirable; the top-level instruction was about not wrapping the entire answer in a fence. I'll use a fenced block for the paste-able draft, and maybe also a quote style for the M2-02 sentence. Actually, for safety and to be maximally paste-able, I'll use a fenced block with a language-less fence for the draft text. That's standard.

Draft content:

```
【G2-LOCK 写死草案（建议原样并入 DRAFT G2 后 LOCK）】

G2-LOCK-1 样本量：每会话 n=12 次打断/重定向（沿用 #37 臂对照结构，n=12/臂）；有效样本 = 打断后 seed 与屏上速度均可测得且 |v_screen| ≥ ε 的样本（ε 由 LOCK 时按测量噪声写死）。
G2-LOCK-2 会话定义与独立性：会话 = 一次进程级独立运行、场景完整重建、臂间不共享弹簧状态；有效会话数 ≥3，且计入 3/3 的会话必须来自 3 次不同运行（同一进程重复 3 轮只计 1 个会话）。
G2-LOCK-3 臂结构：至少 {修复臂, IV=null+0 降级臂}；沿用 #37 {base, shadow, zero, unset, wgc} 结构时保留臂名与 #37 同义。
G2-LOCK-4 指标：逐样本 r_i = |v_seed,i / v_screen,i|，每会话取中位 med_r；v_screen 测量窗口在 LOCK 时写死（打断前后帧窗）。
G2-LOCK-5 PASS：3/3 会话 med_r ∈ [0.8,1.2]；且任一会话任一单样本 r_i < 2.0（无单样本 ≥2.0×，此为「无会话 ≥2.0×」的强口径；若 LOCK 决定取弱口径，则本款改为「3/3 会话 med_r < 2.0」，与主判据冗余，仅作防御）。
G2-LOCK-6 FAIL：有效样本足（会话数 ≥3 且每会话有效 n ≥ 12）但未达 PASS。
G2-LOCK-7 INVALID（任一即整票 INVALID，不允许补测拼凑）：
  a. composition.h sha256 ≠ a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418；
  b. 未授权采数；
  c. seed 仍来自 spring.h 影子闭式 v（seed 赋值语句仍取 mf_spring_at / spring.h v(t)）；
  d. 口径偏离本 G2-LOCK（改中位为均值、改臂、改窗口、改分母处理规则等）；
  e. 会话数 <3 或任一会话有效 n <12；
  f. FAIL 后追加补测回填拼凑。
G2-LOCK-8 0.8 下限说明：wgc 臂 ratio=0.796 为 #37 实测最优且贴近下限；LOCK 需明确——保留 0.8 下限（预期 wgc 类 seed 需小幅校准）或调整下限，二者择一，不得在执行中变口径。
```

Hmm, G2-LOCK-5's strong interpretation — is that too strict? Original DRAFT: "PASS：3/3 中位∈[0.8,1.2] 且无会话≥2.0×". My draft picks the strong reading (any single sample ≥2.0 → fail). I flag the weak alternative inline. That's reasonable and honest.

Wait, is "任一单样本 ≥2.0×" too strict given #37 shadow had median 2.6 — individual samples would certainly exceed. For the fix arm, we'd want no extreme samples. It's a defensible strong reading. I'll keep with the inline alternative.

Also ε — I say "ε 由 LOCK 时按测量噪声写死" — no invented number. Good.

#### 3) RMS/位置辅证：推荐保留为辅证观测，PASS 只锁速度比

明确推荐：速度比作唯一 PASS 判据；RMS/位置与 J/Δv 保留为记录性辅证（观测并入库，不设 PASS/FAIL 门槛），不并入本票 PASS。

理由（对照 #37）：
- 速度比是必要的：shadow 臂 med_vSet=620.0 / med_v_pre=238.5 → ratio=2.600，同时 J_vis 中位=5.30、Δv_vis 中位=272、峰值中位=100.3，四项同向最差；wgc 臂 med_vSet=183.5 / med_v_pre=230.5 → ratio=0.796，同时 J_vis 中位=2.75、Δv_vis 中位=64、峰值中位=41.5，四项同向最好。速度比与视觉残余同向变差，说明它抓得住本票的病根（seed 速度与屏上速度失配）。
- 速度比非充分：wgc 臂 ratio≈0.8（0.796）已是四臂最优，但 J_vis 中位=2.75、Δv_vis 中位=64 仍不达标（目标 J≤0.5 / Δv≤20）。若把 J/Δv 也锁进本票 PASS，则 #37 实测最优臂也过不了，本票（只改 seed 路径）将无解——把轨迹质量门槛放进 seed 修复票是错误分层。
- 结论：本票 PASS 只锁速度比（判 seed 是否修好）；J/Δv 与 RMS/位置留作辅证记录，其达标挂账到轨迹质量类后续票（如 WGC 滞后补偿等）。zero/unset 臂的对照意义继续保留：zero 臂 ratio=0.000、unset 臂 ratio=nan，证明 [0.8,1.2] 窗口能区分「不设 seed」与「正确 seed」，辅证价值不减。

Hmm wait, one nuance: is it right to say "本票将无解" — the material says "最优估计是 WGC 末两帧差分（并论及滞后补偿，属实现细节）" and fix directions: WGC/屏一致速度 seed 或 IV=null+0. If J/Δv gates were included, wgc arm fails them (2.75 > 0.5). The fix ticket could still try harder (lag compensation) to improve J/Δv... but lag compensation is implementation detail explicitly out of scope for this review. The clean argument: 本票范围是 seed 路径；J/Δv 门槛涉及轨迹残余与滞后补偿，超范围；锁入 PASS 会使命题（seed 修复）不可判定。I'll phrase carefully: "若同时锁 J/Δv，则本票 PASS 依赖滞后补偿等非 seed 因素，判据不再针对 seed 修复命题" — that's the precise reasoning. Avoid overclaiming "无解" absolutely. Good.

Also mention: 位置/RMS 辅证 worth keeping because shadow arm's failure shows both velocity ratio and positional residual; and material D's wgc still-failing J/Δv shows sufficiency gap. My recommendation stands: 保留为记录，不锁.

#### 块 2 — 调用点清单（禁改 composition.h）

##### 2.1 仓库内「spring.h / 闭式 v → compositor InitialVelocity」调用点（符号级）

Path A: win_softdemo_src/MainPage.xaml.cs（真机 live 同源常在 MotionSoftDemo/MainPage.xaml.cs；仓库箱内副本在 win_softdemo_src）

| 方法（约行号） | 臂/路径 | 赋值语句级细节 |
|---|---|---|
| CompStartSpring（约 L519+） | 通用启动 | `a.InitialValue=(float)v0; a.InitialVelocity=(float)vel0`（v0/vel0 来自影子状态，注释写明 spring.h） |
| RunAlignProbe | 打断路径 | 打断时用 shadow 作 InitialValue / InitialVelocity |
| RunInterruptSeedProbe | 打断路径 | `MfProbe.mf_spring_at(... out seedPos, out seedVel)` → `a2.InitialVelocity=(float)seedVel` |
| RunRateFoldProbe | 通用 | `a.InitialVelocity=(float)ivel`（ivel 自 mf_spring_at） |
| RunR2P11Probe | 臂 Bv | `r.InitialVelocity=(float)shV`；臂 A 类 → shX+shV（mf_spring_at at tS） |
| RunR2P10Probe | 臂 A/B | 同模式，shV |
| RunR2P9Probe | 臂 A/B | 同模式，shV |
| RunR2P8Probe | 臂名以 A 开头 | InitialValue=shX, InitialVelocity=shV |
| RunR2P7Probe | 臂 A0 / Am20 | shX+shV |
| RunR2P2Probe | 臂 A | shX+shV |
| RunR2P4Probe（派题 #37） | 臂 shadow | `r.InitialVelocity=(float)shV; vSet=shV`；arms={base, shadow, zero, unset, wgc}；证据源 |
| RunR2PostProbe | interrupt 路径 | `a2.InitialVelocity=(float)seedVel` |
| RunSuite 打断段 | shadow seeded retarget | 注释「shadow seeded」 |

Path B: MotionSoftDemo/R3Probe.cs（及 win_softdemo_src/R3Probe.cs 副本）

| 方法 / Trial | 臂/路径 | 赋值语句级细节 |
|---|---|---|
| RunR3P12Probe / Trial | 臂 Bv / Aex | `a.InitialVelocity=(float)shV`（mf_spring_at at tSeed）；Aex 另设 InitialValue=shX |
| RunR3E3Probe | 臂 Bv | InitialValue unset + InitialVelocity=shV |
| RunR350Probe | 臂 Bv | 同上；另有 d1 overlay `InitialVelocity=(float)ivel`（ivel 自 mf_spring_at，ε̂ 扫参） |

备注：
- 若干 `mf_spring_at` 标注 `// log only; do NOT seed` —— 这些不是 seed 调用点，清单中标记「仅日志」，不属于修复面。
- 桥接：`MfProbe.mf_spring_at` ← mf_probe.cpp / fm_capi 导出，内部调 spring.h 闭式。改 seed 策略改的是调用方赋值，不是 spring.h 本身。

##### 2.2 控制器层（本票可触及「seed 路径」语义，须谨慎）

- code/controller.h：`Controller::Retarget` 从当前 owner 的 value/velocity 交接（软件路径 spring 状态）。这是 AppThread/软件控制器语义，不是直接写 WinRT `SpringScalarNaturalMotionAnimation.InitialVelocity`。若本票要把「禁止影子 IV」固化到控制器 API 契约，可动文档+控制器契约；但不得借此改 composition.h。

##### 2.3 禁止本票改动（只登记）

- code/composition.h：`PlanOverride`（约 L304–313）`sp.At(elapsed)` → `pp.spring.fromVelocity = st.velocity`；`PlanGraph` 弹簧段（约 L357–368）同类。这是 lowering 到 CompositorSpring 的产品路径，本票禁止改；对齐 compositor 数学到 spring.h 或改该 seed 出票外。
- code/spring.h：纯数学，不是「喂给 compositor IV」的调用点；勿改除非另票。
- 冻结/禁令：不重开 S1/R-3；不解冻 Bv；成功≠采用。

##### 2.4 本票可能动 vs 禁止动（表）

| 层 | 文件 | 本票 |
|---|---|---|
| demo/harness（可能动） | MotionSoftDemo/R3Probe.cs、win_softdemo_src/R3Probe.cs、win_softdemo_src/MainPage.xaml.cs（及真机 MotionSoftDemo/MainPage.xaml.cs）、相关 harness 脚本/探针接线 | 允许改（仅 seed 赋值路径） |
| 控制器（有条件） | code/controller.h | 仅限 seed 契约/文档级，且不触及 composition.h |
| 文档（可能动） | 本票相关文档 | 允许 |
| 禁止动 | code/composition.h（sha 锁死）、LOCKED 闸门/结果文件本体、DRAFT 闸门本体（本交付不改 DRAFT 文件）、spring.h 数学对齐类改动、采数 | 禁止 |

##### 2.5 sha 与挂账重申

- composition.h sha256 必须保持给定值：`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（本机已核，PRE=POST）。
- composition.h 内 PlanOverride / PlanGraph 用 spring.h::At 填 CompositorSpring.fromVelocity 的路径本票禁止改，只登记为挂账 / 出票外。

## 第三段（可选）— M2-02 草案增量

给 docs/deepseek_m2_02_api_draft.md 的增量一句（可直接粘贴）：

> H5 未修前 C# 投影勿接影子 IV。

放置位置建议：§3.4（若该节承载合成器路径接续假设/未验证项，则追加至该节末尾）；若 §3.4 专述接续假设而不含风险清单，则放 §3.5。一句话理由（供落盘时附注，不必进正文）：H5 PRIMARY（spring.h 闭式 v 作 compositor InitialVelocity → 过加速）未修前，C# 投影若接入影子 IV 会把同一病根带进投影层。

Hmm, the user asked "给 docs/deepseek_m2_02_api_draft.md 的增量一句（可直接粘贴）：「H5 未修前 C# 投影勿接影子 IV」。说明放在哪一节。" So the deliverable is the sentence + section placement. Good.

Let me double check all numbers quoted:

- sha256: a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418 ✓
- shadow: n=12, med_vSet=620.0, med_v_pre=238.5, ratio=2.600 ✓
- zero: n=12, med_vSet=0.0, med_v_pre=240.5, ratio=0.000 ✓
- unset: n=12, med_vSet=nan, med_v_pre=241.0, ratio=nan ✓
- wgc: n=12, med_vSet=183.5, med_v_pre=230.5, ratio=0.796 ✓
- #37 J/Δv: shadow J_vis 中位=5.30, Δv_vis 中位=272, 峰值中位=100.3（最差）✓; wgc J_vis 中位=2.75, Δv_vis 中位=64, 峰值中位=41.5（最好但仍未达 J≤0.5 / Δv≤20）✓
- Scenario: B1 spring P=0.32 ζ=0.78 0→240 IV=0; ~156ms 打断; B2 Final=0, InitialValue 不设 ✓
- M2-01: G3 PRIMARY = H5 Seed 源 ✓
- composition.h PRE=POST ✓

Do not invent: line numbers — the material gives "约 L519+", "约 L304–313", "约 L357–368". Use "约" as given.

"臂 A 类" in R2P11: "臂 `Bv` → `r.InitialVelocity=(float)shV`；臂 A 类 → shX+shV（mf_spring_at at tS）" — keep.

R2P8: "臂名以 A 开头 → InitialValue=shX, InitialVelocity=shV" ✓.

E2: "Controller::Retarget 从当前 owner 的 value/velocity 交接（软件路径 spring 状态）" ✓.

Now about 块 1 question 1's answer structure: "是否够硬？缺口？" — I'll structure: 够硬的部分 (bullet) / 缺口 (numbered G1..G7). Good, conclusion first: 够硬但未闭口? The overall verdict already given at top. Within 1), I can lead with "够硬的核心 + 缺口如下".

One more check — the user says "块 1 先给总评：有条件同意 / 不同意 / 需补洞（择一），再列缺口". So block 1 starts with 总评. Then questions 1,2,3. My 总评 = 需补洞. Should I use exactly one of the three labels? "需补洞" yes. I might also briefly note "若按有条件同意口径，也可表述为：同意判据方向，条件是 LOCK 前补洞" — better to keep it crisp: pick 需补洞, explain why it's not "有条件同意" (the holes are inside the DRAFT text itself). Actually let me just pick 需补洞 with one-line justification. Keep it clean.

Also should the doc mention "本交付不授权采数" and "成功≠采用" — yes in 元信息 硬禁.

Length: The doc should be complete but not bloated. Let me write final markdown carefully with tables.

One concern: the user says "不要输出与交付无关的寒暄" — so no preamble before the doc. The response starts directly with the # heading.

Also "从一级标题开始" — first line is `# ...`. The document title. Let me title it: `# M2-H5-SEED 判据复核 + 调用点清单（文档 only）`. The meta info says 票: M2-H5-SEED 判据复核 + 调用点清单. Good.

Let me also reconsider whether to include "本文档定位" line — meta covers it.

Regarding G2 draft original line "G1：禁止 spring.h 影子闭式速度作 compositor InitialVelocity；主路径=WGC/屏一致速度 seed；或降级 IV=null+0（LOCK 写死默认/分级）" — should my draft include the seed 主路径/降级 default? The user's question 2 asks specifically for n、会话独立性、INVALID 边界写死. I can optionally add a G2-LOCK-0 clause about seed source: 主路径=WGC/屏一致速度 seed，降级=IV=null+0，禁止影子 v. That aligns with G1. I'll add it as G2-LOCK-0 since INVALID item (c) references seed source. Good idea — makes the draft complete for INVALID c.

Also in the draft, "每会话 n=12" — the material suggests ≥12 or follow #37 structure. I choose 12 (material's #37 used n=12 per arm; DRAFT suggests ≥12). Write "n=12（≥12 亦可，但必须写死单一值；#37 结构即 n=12）". Keep decisive: n=12.

Also INVALID: "改 composition.h" per DRAFT. Included as (a). Also "未授权采数" (b). "仍用 spring.h 影子 v" (c). "口径偏离" (d). "追加补测拼凑" (f). Good.

Now — one thing about the 缺口 numbering: G0/G1/G2/G3 are already used in the DRAFT (G0 hard bans, G1 seed source ban, G2 scenarios, G3 bans). To avoid confusion with DRAFT's own clause numbers, I should NOT reuse G1/G2/G3 for my gap list! Use 缺口 A/B/C or 「缺 1」... Let me use "缺口-1 ... 缺口-7" or just "缺口 1)". I'll use 「缺口 1」…「缺口 7」.

Also in my LOCK draft, prefixing clauses G2-LOCK-1 etc. is fine (distinct namespace).

For the 3) recommendation, the user wants "明确推荐与理由". I'll write 推荐：… then 理由 1（必要）、2（非充分）、3（分层）. 

Let me also double-check: "zero/unset 近似，IV 不设≡0" — from D. I reference in 3): zero arm ratio 0.000 / unset nan demonstrates window distinguishes. Careful: does [0.8,1.2] window "distinguish 不设 seed 与 正确 seed"? zero→0.000 fails window; unset→nan fails window. Yes.

But wait — is zero arm "不设≡0" the IV=null+0 downgrade? G1's downgrade is "IV=null+0". In #37, zero arm = 0f and unset = 不设, and D says they're approximately equivalent, IV 不设≡0. So the downgrade path IV=null+0 would land at ratio 0.000/outside window → would FAIL the PASS window. Hmm! That's a subtle but important consistency point: if G1 allows "降级 IV=null+0（LOCK 写死默认/分级）" as a fallback, but the PASS window [0.8,1.2] necessarily fails for zero-seed (ratio=0.000), then the downgrade path can never PASS this gate. Is that a contradiction to flag?

Think: G1 says 主路径=WGC/屏一致速度 seed；或降级 IV=null+0. If the main path achieves ratio∈[0.8,1.2], PASS. The downgrade is a fallback when main path unavailable. If a session uses the downgrade, its ratio would be ~0 → session fails → ticket FAIL. So the downgrade is a product fallback, not a PASS pathway. The gate measures the main path. That's coherent: the gate is about whether the fix (main path) works; downgrade exists for product robustness but can't "pass" the gate. Worth one line in gaps? It's more of a semantic clarification: 判据只考核主路径；降级臂仅作对照。I could add to the LOCK draft: "降级臂（IV=null+0）仅作对照登记，不计入 3/3 会话"。That's a good, derived-from-material clarification. But careful — is it "编造"? It's a logical consequence of the given numbers (zero ratio 0.000 is outside [0.8,1.2]) plus G1 text. I'll add it as a clarification clause in the draft with justification note. This is genuinely valuable for coordination. Add G2-LOCK-3 refinement: 臂结构至少 {修复臂, IV=null+0 降级臂}；降级臂仅作对照与 INVALID 证明，不计入 3/3 PASS 会话（zero 臂 ratio=0.000、unset 臂 ratio=nan 均落于窗口外，降级路径自身无法满足 [0.8,1.2]）。That's solid.

Hmm, but hold on: is it right that downgrade "无法满足"? If v_screen were also ~0 (rest), ratio 0/0 undefined. If v_screen > 0, |0/v| = 0 ∉ [0.8,1.2]. So yes, zero-seed always fails the window (except degenerate). Safe to state with the caveat "按 [0.8,1.2] 窗口". Good.

Also G2-LOCK-4 v_screen measurement — keep one line, no invented frame counts.

I think there's one more subtle gap: "相对 #37 的 2.600 已消除" — the PASS mentions this but it's descriptive, not an operational criterion; operationalized via the 2.0× bound. Fine, covered.

Also 缺口 about 会话间臂配置：3/3 会话是同一臂重复 3 次，还是每臂 1 次？"会话≥3" per DRAFT — with arms {fix, zero...}, the 3 sessions must presumably be the fix arm across 3 runs (plus control arms each session). My G2-LOCK-2/3 covers: 计入 3/3 的会话来自修复臂的 3 次独立运行，对照臂随会话跑。Hmm, is that in the material? DRAFT G2: "场景约定打断/重定向探针；会话≥3；每会话 n 按 LOCK 写死（建议≥12 次打断或沿用 #37 臂对照结构）。指标…PASS：3/3 中位∈…"。So 3 sessions each with n interrupts; the arms structure optional (#37 style). To keep the draft faithful without over-inventing, I'll say: 3/3 判定的 3 个会话须针对修复路径（修复臂）各独立运行；对照臂可随同会话采集，仅作对照。This is an operational necessity, flagged as "本复核给出的写死建议" not as material fact. The user asked me to produce the draft — it's my recommended wording, clearly presented as such. OK.

Now, also the user says in block 2 question 2: "本票允许改的层：demo / harness / 控制器；明确哪些文件可能动、哪些禁止动" — E4 table covers it.

Block 2 question 3: "重申 composition.h sha 必须保持给定值；composition.h 内 PlanOverride/PlanGraph 用 spring.h::At 填 CompositorSpring.fromVelocity 的路径本票禁止改，只登记为挂账/出票外" — covered in 2.5. Note material says PlanOverride uses `sp.At(elapsed)` → `pp.spring.fromVelocity = st.velocity` (from E3). Hmm interesting: E3 says "PlanOverride（约 L304–313）`sp.At(elapsed)` → `pp.spring.fromVelocity = st.velocity`" — wait the user's block-2 item 3 says "用 spring.h::At 填 CompositorSpring.fromVelocity". The material E3 says "`sp.At(elapsed)` → `pp.spring.fromVelocity = st.velocity`". I'll quote E3's form (sp.At(elapsed), st.velocity) and can mention "spring.h::At 填 CompositorSpring.fromVelocity" per the user's phrasing. To be safe I'll mirror E3 exactly: `sp.At(elapsed)` → `pp.spring.fromVelocity = st.velocity`. And PlanGraph 弹簧段（约 L357–368）同类. Good.

Now write the final document. Also ensure: 简体中文, tables, no fabrications, no fluff. The response should be ONLY the document (no lead-in sentence? "不要输出与交付无关的寒暄" — I'll start directly with the heading).

Let me also double check the 元信息 票 line: "票：M2-H5-SEED 判据复核 + 调用点清单（文档 only，DRAFT 闸门未 LOCK，本交付不授权采数）" — reproduce.

Structure headings:
# M2-H5-SEED 判据复核 + 调用点清单
## 元信息
## 块 1 — 判据复核（对照 DRAFT G2）
### 总评：需补洞
### 1) PASS 定义复核
### 2) LOCK 前写死草案（可粘贴）
### 3) RMS/位置辅证建议
## 块 2 — 调用点清单（禁改 composition.h）
### 2.1 调用点清单（路径 + 符号级）
### 2.2 控制器层
### 2.3 禁止改动（只登记）
### 2.4 本票可能动 vs 禁止动
### 2.5 sha 保持与挂账
## 第三段（可选）— M2-02 API 草案增量

Let me write it fully now.

Careful with quotes of numbers: "#37 复算：shadow arm med_vSet=620.0 / med_v_pre=238.5 → ratio=2.600" — the user's example formatting uses "med_vSet=620.0 / med_v_pre=238.5 → ratio=2.600". I'll use that.

J_vis numbers: "shadow J_vis 中位=5.30 Δv_vis 中位=272 峰值中位=100.3（最差）". "wgc J_vis 中位=2.75 Δv_vis 中位=64 峰值中位=41.5（最好但仍未达 J≤0.5 / Δv≤20）".

Scenario quote: "B1 spring P=0.32 ζ=0.78 0→240 IV=0；约 156ms 打断；B2 Final=0，InitialValue 不设" — use in block 1 §1 context for anchoring.

Compose now. Also the WGC lag compensation note: "D 指出最优估计是 WGC 末两帧差分（并论及滞后补偿，属实现细节，本票复核不必冻结）" — mention in §3 or §1 as out-of-scope note. Put in §3 理由 3 or in gaps as G6 note. I'll add a line in 3): 滞后补偿按 D 属实现细节，本复核不冻结，但速度比窗口须容忍测量滞后（故 [0.8,1.2] 而非点值 1.0）— hmm, is that interpretation safe? [0.8,1.2] tolerance rationale — the material doesn't state why the window was chosen. I should not invent the rationale. Just note: 滞后补偿属实现细节不冻结，但 LOCK 时需保证 v_screen 测量窗口与 seed 时刻定义一致，否则比值天然含滞后偏差。That's a fair operational note without inventing rationale for the 0.8/1.2 choice. Keep it in G6.

Let me finalize wording for gap 5 (0.796 vs 0.8 boundary). "wgc med_vSet=183.5 / med_v_pre=230.5 → ratio=0.796" — this is the ratio the metric computes (183.5/230.5 = 0.7961). Given. "恰好低于 0.8 下限" — true. I'll present it as: LOCK 前需明确 0.8 下限与实测最优 0.796 的边界关系（或调下限，或声明 wgc 类 seed 需校准）；避免修复臂落在 0.796–0.8 区间时被机械判 FAIL。Good.

Draft the document now, being careful with markdown. Use code fence for the paste-able draft. Inside the fence, no backticks issues.

One more consideration: should the whole answer be just the markdown? Yes. No final "note" outside. The user said don't output unrelated pleasantries.

Also check: "块 1 先给总评…再列缺口" — my §1 (PASS 复核) contains 够硬部分 + 缺口 list. The 总评 comes first. Good.

Draft final:

---

# M2-H5-SEED 判据复核 + 调用点清单

## 元信息

| 项 | 内容 |
|---|---|
| 票 | M2-H5-SEED 判据复核 + 调用点清单（文档 only；DRAFT 闸门未 LOCK；本交付不授权采数） |
| 依据文件 | docs/m2_h5_seed_gate_DRAFT.md（闸门 DRAFT，STATUS=DRAFT NOT LOCKED）；docs/m2_01_result.md（M2-01 结题）；artifacts/m2_01_g3_h1h5_audit.txt（G3 审计）；artifacts/win_r2_p4_b2vel.txt（#37 解读/证据）；仓库静态扫描（E 节调用点清单，本机 sha 已核） |
| composition.h 当前 sha256 | `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` |

硬禁重申：
1. 不改 composition.h，sha256 必须保持 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（PRE=POST，本机已核）。
2. 不重开 S1 / R-3；不重开 R-2。
3. 不解冻、不升级 Bv。
4. 成功 ≠ 采用。
5. 本票只改 seed 路径（demo / harness / 控制器），不对齐 compositor 弹簧数学到 spring.h。
6. 未授权采数禁止；本交付（文档 only）不授权任何采数，不得因本文档而开始采数。

## 块 1 — 判据复核（对照 DRAFT G2）

### 总评：需补洞

...

Good. Now write everything out. I'll aim for a complete but tight document.

For §2) draft fence:

```
G2-LOCK-0（seed 源）主路径 = WGC/屏一致速度 seed；降级 = IV=null+0（LOCK 写死默认/分级）；禁止 spring.h 影子闭式 v 作 compositor InitialVelocity。
G2-LOCK-1（样本量）每会话 n=12 次打断/重定向（沿用 #37 臂对照结构 n=12/臂）；有效样本 = seed 与 v_screen 均可测得且 |v_screen| ≥ ε（ε 由 LOCK 按测量噪声写死，本复核不代定数值）。
G2-LOCK-2（会话独立性）会话 = 一次进程级独立运行、场景完整重建、臂间不共享弹簧状态；计入 3/3 的 3 个会话必须来自 3 次不同运行（同一进程重复 3 轮只计 1 个会话），且均针对修复路径。
G2-LOCK-3（臂结构）至少 {修复臂, IV=null+0 降级臂}；降级臂仅作对照登记，不计入 3/3（zero 臂 ratio=0.000、unset 臂 ratio=nan，落于 [0.8,1.2] 之外，降级路径自身无法满足窗口）。
G2-LOCK-4（指标口径）逐样本 r_i = |v_seed,i / v_screen,i|，会话中位 med_r；v_screen 测量窗口与 seed 时刻定义在 LOCK 写死，两定义必须一致（滞后补偿属实现细节不冻结，但不得造成口径漂移）。
G2-LOCK-5（PASS）3/3 会话 med_r ∈ [0.8,1.2]，且任一单样本 r_i < 2.0（即无单样本 ≥2.0×，相对 #37 shadow 2.600 已消除）；若 LOCK 选择弱口径则改为「无会话 med_r ≥2.0×」（与主判据冗余，仅防御）。
G2-LOCK-6（FAIL）有效样本足（会话数 ≥3 且每会话有效 n ≥12）但未达 PASS。
G2-LOCK-7（INVALID，任一即整票 INVALID，不允许补测拼凑）
  a. composition.h sha256 ≠ a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418；
  b. 未授权采数；
  c. seed 仍来自 spring.h 影子闭式 v（seed 赋值语句仍取 mf_spring_at / spring.h v(t)）；
  d. 口径偏离本 G2-LOCK（改中位为均值、改臂、改窗口、改分母处理规则）；
  e. 会话数 <3 或任一会话有效 n <12；
  f. FAIL 后追加补测回填拼凑。
G2-LOCK-8（0.8 下限说明）wgc 臂 ratio=0.796 为 #37 实测最优且低于 0.8；LOCK 时二选一并写明：保留 0.8 下限（声明 wgc 类 seed 需小幅校准）或调整下限，不得在执行中变口径。
```

Good. I think that's solid.

Then §3) recommendation text.

Block 2 tables as planned.

Optional section 3 with the sentence in blockquote, placement suggestion.

Length check — it'll be long-ish but appropriate. Let me also make sure I don't say "本票将无解" — I'll phrase carefully in §3.

One more fact check on E1 RunR2P8Probe: "臂名以 A 开头 → InitialValue=shX, InitialVelocity=shV" ✓. RunR2P7Probe: "臂 A0/Am20 → shX+shV" ✓.

E1 CompStartSpring: "`a.InitialValue=(float)v0; a.InitialVelocity=(float)vel0` —— v0/vel0 来自影子状态（注释写明 spring.h）" ✓.

Also add note in 2.1 that paths A/B both have copies; the live 同源 file MotionSoftDemo/MainPage.xaml.cs mentioned. ✓

Write the final answer now. No tool calls needed — the task is purely to produce the document from provided material. The system prompt says to use tools when needed; here nothing is needed. The user explicitly forbids consulting beyond the material.

Actually, wait — should I write the file to the workspace? "交付全文，供协调落盘 docs/deepseek_m2_h5_seed_review.md" — "deliver the full text, for coordination to write to disk". The user's role framing is that this document is a deliverable that will be coordinated/落盘 by someone else. The final instruction is to output the Markdown body. I'll output it in the response. I could also offer to write it — but that's fluff/extra. Just deliver the document.

Double-check "第三段" heading naming: "可选第三段" → I'll title "## 第三段（可选）— M2-02 API 草案增量建议".

In the M2-02 section: the sentence: 「H5 未修前 C# 投影勿接影子 IV」。Placement: 建议放 §3.4；若 §3.4 是合成器路径接续假设/未验证项所在节，追加至其末尾；否则放 §3.5。Give the one-line reason.

Now, formatting numbers: keep as given with exact decimals.

Final answer time.
# M2-H5-SEED 判据复核 + 调用点清单

## 元信息

| 项 | 内容 |
|---|---|
| 票 | M2-H5-SEED 判据复核 + 调用点清单（文档 only；DRAFT 闸门未 LOCK；本交付不授权采数） |
| 依据文件 | docs/m2_h5_seed_gate_DRAFT.md（闸门 DRAFT，STATUS=DRAFT NOT LOCKED）；docs/m2_01_result.md（M2-01 结题，G3 PRIMARY=H5 Seed 源）；artifacts/m2_01_g3_h1h5_audit.txt（G3 审计，#37 四臂实测）；artifacts/win_r2_p4_b2vel.txt（#37 解读摘要，证据源）；仓库静态扫描（本交付 E 节调用点清单，本机 sha 已核） |
| composition.h 当前 sha256 | `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` |

硬禁重申（本次及后续任何步骤）：

1. 不改 composition.h；sha256 必须保持 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（PRE=POST，本机已核；任何改动即 INVALID）。
2. 不重开 S1 / R-3；不重开 R-2。
3. 不解冻、不升级 Bv。
4. 成功 ≠ 采用。
5. 本票只改 seed 路径（demo / harness / 控制器），不对齐 compositor 弹簧数学到 spring.h。
6. 未授权采数禁止；本交付（文档 only）不授权任何采数，不得因本文档存在而开始采数。

## 块 1 — 判据复核（对照 DRAFT G2）

### 总评：需补洞

DRAFT G2 的判据方向正确、主体成立：以会话中位速度比为核心，窗口 [0.8,1.2] 且无 ≥2.0× 会话，直接锚定 #37 实测的 2.600 与 0.796 两个已知极值，能同时排除「影子过加速」与「欠 seed / 不设 seed」两类错误。但当前 DRAFT 文本中 n、会话独立性、INVALID 边界均为「LOCK 写死」占位，分母退化（v_screen≈0）无处理规则，「无会话 ≥2.0×」的统计口径有歧义，0.8 下限与实测最优 0.796 存在边界摩擦。按现状不可执行，必须在 LOCK 前补齐缺口；补洞方案见下方第 2) 条写死草案。

### 1) PASS 定义是否够硬

PASS 定义原文：「3/3 会话中位 |v_seed/v_screen| ∈ [0.8,1.2] 且无会话 ≥2.0×；相对 #37 的 2.600 已消除」。

**够硬的部分：**

- 会话中位：对单次打断的异常值稳健，比均值更适合打断探针。
- [0.8,1.2] 窗口：下限 0.8 要求 seed 真实匹配屏上速度，仅「不超调」不达标——zero 臂 ratio=0.000、unset 臂 ratio=nan 均被窗口排除，判据因此同时防住「过 seed」与「欠 seed」。
- 3/3：全部会话通过才 PASS，禁止挑会话。
- ≥2.0× 上限与 #37 锚定：shadow 臂 med_vSet=620.0 / med_v_pre=238.5 → ratio=2.600 是已知病态，2.0× 上限将其与正常区隔开；判据与已定位失败模式一一对应。

**缺口：**

- 缺口 1（n 未写死）：DRAFT 仅「建议≥12 次打断或沿用 #37 臂对照结构」。「样本足」无数字则 FAIL 判定无据，也无法识别「补测拼凑」。
- 缺口 2（会话独立性无定义）：同一进程重复跑 3 轮算 1 个还是 3 个会话？臂间是否共享弹簧状态？不写死则 3/3 可由同一次运行拼出，失去「3 个会话」的意义。
- 缺口 3（分母退化无规则）：v_screen≈0 时比值发散或无定义（#37 已出现 unset 臂 ratio=nan、zero 臂 ratio=0.000）。DRAFT 未规定 |v_screen| 过小样本的处理方式，口径可漂移。
- 缺口 4（「无会话 ≥2.0×」口径歧义）：若指会话中位，则该条与「中位∈[0.8,1.2]」冗余；若指单样本，约束强度完全不同。必须二选一写死（草案取强口径，见下）。
- 缺口 5（0.8 下限与实测最优的边界摩擦）：wgc med_vSet=183.5 / med_v_pre=230.5 → ratio=0.796，是 #37 四臂中唯一贴近窗口且 J/Δv 最优的臂，却恰好低于 0.8。不写死边界说明，修复臂落在 0.796–0.8 区间时会被机械判 FAIL。
- 缺口 6（v_screen 测量口径未写死）：测量取哪几帧、打断前还是打断后，DRAFT 未写死。滞后补偿按 D 属实现细节本复核不冻结，但测量窗口本身是判据口径的一部分，且必须与 seed 时刻定义一致，否则比值天然含滞后偏差。
- 缺口 7（FAIL 判定依赖未锁定参数）：「样本足」取决于未锁定的 n 与会话数，FAIL 判据因此暂不可判定。

### 2) LOCK 前写死草案（可粘贴）

```text
G2-LOCK-0（seed 源）：主路径 = WGC/屏一致速度 seed；降级 = IV=null+0（LOCK 写死默认/分级）；
  禁止 spring.h 影子闭式 v 作 compositor InitialVelocity。
G2-LOCK-1（样本量）：每会话 n=12 次打断/重定向（沿用 #37 臂对照结构 n=12/臂）；
  有效样本 = seed 与 v_screen 均可测得且 |v_screen| ≥ ε（ε 由 LOCK 按测量噪声写死，本复核不代定数值）。
G2-LOCK-2（会话独立性）：会话 = 一次进程级独立运行、场景完整重建、臂间不共享弹簧状态；
  计入 3/3 的 3 个会话必须来自 3 次不同运行（同一进程重复 3 轮只计 1 个会话），且均针对修复路径。
G2-LOCK-3（臂结构）：至少 {修复臂, IV=null+0 降级臂}；降级臂仅作对照登记，不计入 3/3
  （zero 臂 ratio=0.000、unset 臂 ratio=nan，均落于 [0.8,1.2] 之外，降级路径自身无法满足窗口）。
G2-LOCK-4（指标口径）：逐样本 r_i = |v_seed,i / v_screen,i|，会话中位 med_r；
  v_screen 测量窗口与 seed 时刻定义在 LOCK 时写死，两者必须一致（滞后补偿属实现细节，不冻结但不得造成口径漂移）。
G2-LOCK-5（PASS）：3/3 会话 med_r ∈ [0.8,1.2]，且任一单样本 r_i < 2.0（即无单样本 ≥2.0×，
  相对 #37 shadow 2.600 已消除）；若 LOCK 选择弱口径，则改为「无会话 med_r ≥ 2.0×」（与主判据冗余，仅作防御）。
G2-LOCK-6（FAIL）：有效样本足（会话数 ≥3 且每会话有效 n ≥12）但未达 PASS。
G2-LOCK-7（INVALID，任一即整票 INVALID，不允许补测拼凑）：
  a. composition.h sha256 ≠ a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418；
  b. 未授权采数；
  c. seed 仍来自 spring.h 影子闭式 v（seed 赋值语句仍取 mf_spring_at / spring.h v(t)）；
  d. 口径偏离本 G2-LOCK（改中位为均值、改臂、改窗口、改分母处理规则等）；
  e. 会话数 <3 或任一会话有效 n <12；
  f. FAIL 后追加补测回填拼凑。
G2-LOCK-8（0.8 下限说明）：wgc 臂 ratio=0.796 为 #37 实测最优且低于 0.8；
  LOCK 时二选一并写明：保留 0.8 下限（声明 wgc 类 seed 需小幅校准），或调整下限；不得在执行中变口径。
```

### 3) RMS/位置辅证：保留为辅证观测，PASS 只锁速度比

**明确推荐：速度比作唯一 PASS 判据；RMS/位置与 J/Δv 保留为记录性辅证（观测入库，不设 PASS/FAIL 门槛），不并入本票 PASS。**

理由（对照 #37）：

- 速度比是**必要**的：shadow 臂 med_vSet=620.0 / med_v_pre=238.5 → ratio=2.600，同时 J_vis 中位=5.30、Δv_vis 中位=272、峰值中位=100.3，四项同向最差；wgc 臂 med_vSet=183.5 / med_v_pre=230.5 → ratio=0.796，同时 J_vis 中位=2.75、Δv_vis 中位=64、峰值中位=41.5，四项同向最好。速度比与视觉残余同向变差，说明它抓得住本票病根（seed 速度与屏上速度失配）。
- 速度比**非充分**：wgc 臂 ratio=0.796（≈0.8）已是四臂最优，但 J_vis 中位=2.75、Δv_vis 中位=64 仍未达 J≤0.5 / Δv≤20。若把 J/Δv 也锁进本票 PASS，则 #37 实测最优臂亦不达标，PASS 将依赖滞后补偿等非 seed 因素，判据不再针对「seed 路径修复」这一命题。
- 分层结论：本票 PASS 只锁速度比（判 seed 是否修好）；J/Δv 与 RMS/位置留作辅证，其达标挂账到轨迹质量类后续票（如 WGC 滞后补偿）。同时保留 zero/unset 对照臂：zero 臂 ratio=0.000、unset 臂 ratio=nan 证明 [0.8,1.2] 窗口能区分「不设 seed」与「正确 seed」，辅证价值不减。

## 块 2 — 调用点清单（禁改 composition.h）

### 2.1 仓库内「spring.h / 闭式 v → compositor InitialVelocity」调用点（符号级）

路径 A：`win_softdemo_src/MainPage.xaml.cs`（真机 live 同源常在 MotionSoftDemo/MainPage.xaml.cs；仓库箱内副本在 win_softdemo_src）

| 方法（约行号） | 臂/路径 | 赋值语句级细节 |
|---|---|---|
| CompStartSpring（约 L519+） | 通用启动 | `a.InitialValue=(float)v0; a.InitialVelocity=(float)vel0`（v0/vel0 来自影子状态，注释写明 spring.h） |
| RunAlignProbe | 打断路径 | 打断时用 shadow 作 InitialValue / InitialVelocity |
| RunInterruptSeedProbe | 打断路径 | `MfProbe.mf_spring_at(... out seedPos, out seedVel)` → `a2.InitialVelocity=(float)seedVel` |
| RunRateFoldProbe | 通用 | `a.InitialVelocity=(float)ivel`（ivel 自 mf_spring_at） |
| RunR2P11Probe | 臂 Bv | `r.InitialVelocity=(float)shV`；臂 A 类 → shX+shV（mf_spring_at at tS） |
| RunR2P10Probe | 臂 A/B | 同模式，shV |
| RunR2P9Probe | 臂 A/B | 同模式，shV |
| RunR2P8Probe | 臂名以 A 开头 | InitialValue=shX, InitialVelocity=shV |
| RunR2P7Probe | 臂 A0 / Am20 | shX+shV |
| RunR2P2Probe | 臂 A | shX+shV |
| **RunR2P4Probe（派题 #37）** | 臂 shadow | `r.InitialVelocity=(float)shV; vSet=shV`；arms={base, shadow, zero, unset, wgc}；证据源 |
| RunR2PostProbe | interrupt 路径 | `a2.InitialVelocity=(float)seedVel` |
| RunSuite 打断段 | shadow seeded retarget | 注释「shadow seeded」 |

路径 B：`MotionSoftDemo/R3Probe.cs`（及 win_softdemo_src/R3Probe.cs 副本）

| 方法 / Trial | 臂/路径 | 赋值语句级细节 |
|---|---|---|
| RunR3P12Probe / Trial | 臂 Bv / Aex | `a.InitialVelocity=(float)shV`（mf_spring_at at tSeed）；Aex 另设 InitialValue=shX |
| RunR3E3Probe | 臂 Bv | InitialValue unset + InitialVelocity=shV |
| RunR350Probe | 臂 Bv | 同上；另有 d1 overlay `InitialVelocity=(float)ivel`（ivel 自 mf_spring_at，ε̂ 扫参） |

备注：

- 若干 `mf_spring_at` 标注 `// log only; do NOT seed`——这些不是 seed 调用点，清单中标记「仅日志」，不属于修复面。
- 桥接：`MfProbe.mf_spring_at` ← mf_probe.cpp / fm_capi 导出，内部调 spring.h 闭式。改 seed 策略时改的是**调用方赋值**，不是 spring.h 本身。

### 2.2 控制器层（本票可触及「seed 路径」语义，须谨慎）

- `code/controller.h`：`Controller::Retarget` 从当前 owner 的 value/velocity 交接（软件路径 spring 状态）。这是 AppThread/软件控制器语义，**不是**直接写 WinRT `SpringScalarNaturalMotionAnimation.InitialVelocity`。若本票要把「禁止影子 IV」固化到控制器 API 契约，可动文档 + 控制器契约；但**不得**借机改 composition.h。

### 2.3 禁止本票改动（只登记）

- `code/composition.h`：`PlanOverride`（约 L304–313）`sp.At(elapsed)` → `pp.spring.fromVelocity = st.velocity`；`PlanGraph` 弹簧段（约 L357–368）同类。这是 lowering 到 CompositorSpring 的产品路径，本票禁止改；对齐 compositor 数学到 spring.h 或改该 seed 出票外。
- `code/spring.h`：纯数学，不是「喂给 compositor IV」的调用点；勿改，除非另票。
- 冻结/禁令：不重开 S1/R-3；不解冻 Bv；成功≠采用。

### 2.4 本票可能动 vs 禁止动

| 层 | 文件 | 本票 |
|---|---|---|
| demo / harness（可能动） | MotionSoftDemo/R3Probe.cs、win_softdemo_src/R3Probe.cs、win_softdemo_src/MainPage.xaml.cs（及真机 MotionSoftDemo/MainPage.xaml.cs）、相关 harness 脚本/探针接线 | 允许改（仅 seed 赋值路径） |
| 控制器（有条件） | code/controller.h | 仅限 seed 契约/文档级，且不触及 composition.h |
| 文档（可能动） | 本票相关文档 | 允许 |
| 禁止动 | code/composition.h（sha 锁死）、LOCKED 闸门/结果文件本体、DRAFT 闸门本体（本交付不改 DRAFT 文件）、spring.h 数学对齐类改动、采数 | 禁止 |

### 2.5 sha 保持与挂账重申

- composition.h sha256 必须保持给定值：`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（本机已核，PRE=POST）。
- composition.h 内 `PlanOverride` / `PlanGraph` 用 spring.h::At 填 CompositorSpring.fromVelocity 的路径**本票禁止改**，只登记为挂账 / 出票外。

## 第三段（可选）— M2-02 API 草案增量建议

给 `docs/deepseek_m2_02_api_draft.md` 的增量一句（可直接粘贴）：

> H5 未修前 C# 投影勿接影子 IV。

放置位置建议：**§3.4**。若 §3.4 是「合成器路径接续假设/未验证项」所在节，追加至该节末尾；若该节专述接续假设而无风险清单，则放 §3.5。附注理由（供落盘参考，不必进正文）：H5 PRIMARY（spring.h 闭式 v 作 compositor InitialVelocity → 过加速）修复前，C# 投影若接入影子 IV，会把同一病根带进投影层。
The user is asking me to write a Chinese technical review document for a "动效框架" (motion effects framework) project. This is a pure document-writing task based solely on the provided fact material. The task says:

- Deliver full text for coordination to write to `docs/deepseek_m2_h5_seed_review.md`
- Structure: 元信息 (meta info), 块 1 (judgment criteria review against DRAFT G2), 块 2 (call site inventory), optional third section
- Strict requirements: Simplified Chinese, conclusion first, tables allowed, quote measured numbers verbatim, no fabrication

Let me check the working directory context. Current working directory is "C:\Users\admin\Temp". The task says "交付全文，供协调落盘 docs/deepseek_m2_h5_seed_review.md" — deliver the full text for coordination to write to the file. So my main deliverable is the Markdown text in my response. The user says "请输出完整 Markdown 正文（从一级标题开始），不要包在代码围栏里。"

So the key deliverable is the full Markdown text output in my final response, NOT wrapped in code fences. Should I also write the file? The task says "交付全文，供协调落盘" — deliver the full text, for coordination to write to disk. This suggests the user wants the text delivered; the actual writing is coordinated (perhaps another agent writes it). The instruction "请输出完整 Markdown 正文（从一级标题开始），不要包在代码围栏里" is explicit: output the full Markdown body starting from level-1 heading, not in a code fence.

Should I write the file myself? The working directory is C:\Users\admin\Temp, which is not a repo containing docs/. There's no indication the repo is here. The task explicitly says "交付全文，供协调落盘" — meaning my delivery is the text, and the coordination (落盘) happens elsewhere. Given the doc must go to `docs/deepseek_m2_h5_seed_review.md` which doesn't exist in this Temp directory, and the task structure is a "review document" delivery, I'll output the full Markdown as my answer. That's what's asked: "请输出完整 Markdown 正文（从一级标题开始），不要包在代码围栏里。"

Also, is there a team here? I'm the Team Lead. The user's message is direct: write the document. No need to spawn teammates. Just deliver.

Let me check the requirements carefully:

### 元信息
- 票：M2-H5-SEED 判据复核 + 调用点清单（文档 only，DRAFT 闸门未 LOCK，本交付不授权采数）
- 依据文件列表
- composition.h 当前 sha256（材料已给，须原样写出）: `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`
- 硬禁重申

依据文件列表 from the material:
- docs/m2_h5_seed_gate_DRAFT.md (闸门 DRAFT)
- docs/m2_01_result.md (M2-01 结题)
- artifacts/m2_01_g3_h1h5_audit.txt (G3 审计)
- artifacts/win_r2_p4_b2vel.txt (#37 解读摘要)
- 仓库静态扫描（调用点清单，本机 sha 已核）

### 块 1 — 判据复核（对照 DRAFT G2）
Must answer each:
1) PASS 定义「3/3 会话中位 |v_seed/v_screen| ∈ [0.8,1.2] 且无会话 ≥2.0×；相对 #37 的 2.600 已消除」是否够硬？缺口？
2) 建议 LOCK 前写死：n（打断次数或帧）、会话独立性、INVALID 边界（仍用 spring.h 影子 v 等）——给出可粘贴的写死草案
3) 是否需要保留 RMS/位置辅证，还是只锁速度比？给出明确推荐与理由（对照 #37：shadow 臂速度比 2.6 与 J_vis/Δv_vis 同向变差；wgc 臂速度比≈0.8 但仍有 J/Δv 未达标——说明速度比是必要非充分）

Block 1 must first give 总评：有条件同意 / 不同意 / 需补洞（choose one), then list gaps.

Analysis of 1):
PASS 定义 strength:
- Positive: uses per-session median (robust to outliers), ratio window [0.8,1.2] matches wgc arm observation (0.796 from C/… wait, wgc ratio=0.796 which is below 0.8! Interesting). Let me check: wgc med_vSet=183.5 / med_v_pre=230.5 → ratio = 183.5/230.5 = 0.7961... The material says "wgc 臂速度比≈0.8" and in the audit "ratio=0.796". Hmm, but the PASS window is [0.8, 1.2]. wgc at 0.796 would technically be below 0.8! This is a subtle point worth noting: the best arm (#37 wgc) had ratio=0.796, marginally below the proposed lower bound 0.8. That's a potential gap worth flagging: the PASS window lower bound 0.8 excludes the empirically best observed arm (0.796). Actually wait — is that a real gap? Let me be careful. The ratio definition: |v_seed/v_screen|. For wgc, v_seed was the WGC screen velocity from the last two frames, and v_screen was presumably the actual on-screen velocity after retarget. So 0.796 means the seed slightly underestimates the screen velocity (approximately 0.8, "wgc 臂速度比≈0.8" per D). Since PASS requires ∈[0.8,1.2], the observed best arm at 0.796 sits just outside. I should flag this as a boundary consistency gap: either the threshold needs justification or the wgc-based seed needs a slight calibration. This is a legitimate observation from the given numbers. I must be careful not to overstate: material says "wgc 臂速度比≈0.8 但仍有 J/Δv 未达标——说明速度比是必要非充分". The user themselves frames 0.796 as ≈0.8. So flagging the boundary tension is fair and valuable.

- "无会话 ≥2.0×" caps worst-case — shadow's 2.600 is well above 2.0, so it eliminates the #37 shadow pathology. Good.
- 3/3 sessions with median in window: requires consistency across sessions. Good — guards against cherry-picking.
- Relative to #37: 2.600 eliminated.

Gaps (缺口):
- n not fixed in DRAFT (G2 says "每会话 n 按 LOCK 写死（建议≥12 次打断或沿用 #37 臂对照结构）" — it's DRAFT, must be locked).
- 会话独立性 not defined (what counts as a session; same process? restart? different arms?)
- INVALID boundary partially listed but needs lock: "仍用 spring.h 影子 v" listed; but also "INVALID" for median computation when v_screen=0 → ratio division by zero (zero arm ratio=0.000 when vSet=0; unset arm ratio=nan). What if v_screen ≈ 0 (e.g., interrupt at rest)? Need a guard for near-zero denominator. Material doesn't explicitly give that, but the zero/unset arms show ratio=0.000/nan values, so the ratio metric itself has degenerate behavior at v=0. I can flag this as a gap requiring a written-down boundary (e.g., exclude windows where |v_screen| < ε, or predefine handling). But careful: I must not invent specific numbers not given. I can propose the draft includes a threshold to be chosen, without inventing the value. In the draft I give, I should leave placeholders like `<ε>` or define per LOCK. Actually, giving a paste-able draft with placeholders is acceptable — the user asked for "可粘贴的写死草案". I should avoid inventing hard numbers not in the material; use the material's numbers (≥12, 3/3, [0.8,1.2], ≥2.0×) and leave placeholders where the material doesn't specify.
- PASS uses median but the per-session "无会话≥2.0×" is on what statistic? "无会话 ≥2.0×" — is it median or any single sample? Ambiguous. Should lock: no session's median ≥2.0×, and optionally no single sample ≥2.0×. Flag as ambiguity.
- Denominator issue: |v_seed/v_screen| — which is the reference? v_screen is the measured on-screen velocity. How is v_screen measured? Not specified in G2 (only "屏上 NaturalMotion 轨迹" from B). Measurement window (before/after interrupt, frame window) not locked.
- What about the "滞后补偿" mentioned in D ("并论及滞后补偿，属实现细节，本票复核不必冻结")? The task says "本票复核不必冻结" — so I can mention it's out of scope but note the ratio window must tolerate measurement lag; keep brief.
- FAIL definition: "样本足未达 PASS" — needs n and session count locked, and retry/backfill rules ("追加补测拼凑" is INVALID per material). Also "样本足" depends on locked n.
- The ratio lower bound 0.8 vs wgc 0.796 tension (as above).

Total assessment: 需补洞 (needs holes filled) or 有条件同意? The user says "块 1 先给总评：有条件同意 / 不同意 / 需补洞（择一）". Given DRAFT is explicitly NOT LOCKED and n/会话独立性/INVALID are not yet written down, the honest verdict is 需补洞 — the direction is right (speed-ratio core is necessary), the numbers are anchored to #37 (2.600 vs 0.796), but the LOCK must first write down n, session independence, INVALID boundaries, and the near-zero denominator handling before it can be executed. Hmm, or 有条件同意 — "agree on condition that holes are filled before LOCK". Both plausible. Let me pick 需补洞 since the draft literally leaves n/会话独立性/INVALID as "LOCK 写死" placeholders — i.e., holes exist in the current DRAFT text itself. Then list the gaps and the draft. Actually "有条件同意" would also fit: agree with conditions = fill the holes. The strongest and most precise: 需补洞. The PASS definition itself is directionally sound but the current DRAFT cannot be executed as-is because the core sampling parameters are unresolved. I'll go with 需补洞.

For 2) — paste-able draft. Let me draft:

```
G2-LOCK 写死草案（建议并入 DRAFT G2 后 LOCK）：

1. 样本量：每会话 n=12 次打断/重定向（沿用 #37 臂对照结构：n=12/臂）。
2. 会话定义与独立性：会话 = 一次独立运行的探针场景（进程级重启或场景完整重建，臂间互不共享弹簧状态）；有效会话 ≥3，且 3 个通过会话须来自 3 次独立运行。
3. 臂结构：至少保留 {修复臂, zero/IV=null+0 降级臂} 对照；可沿用 #37 {base,shadow,zero,unset,wgc} 结构。
4. 指标：每会话对 n 次打断逐一计算 r_i = |v_seed,i / v_screen,i|，取会话中位 med_r。
   - 分母退化：|v_screen,i| < ε 的样本点单独处理——（ε 与处理方式由 LOCK 时写死，例如剔除并记录，或并入 near-rest 规则；本复核不代定数值）。
5. PASS：3/3 会话 med_r ∈ [0.8, 1.2] 且 3 个会话的 med_r 均 < 2.0（相对 #37 shadow 2.600 已消除）。
6. FAIL：有效样本数足（n 与会话数达标）但未达 PASS。
7. INVALID（任一即整票 INVALID，不允许补测拼凑）：
   a. composition.h sha256 偏离 a55d...418；
   b. 未授权采数；
   c. seed 仍来自 spring.h 影子闭式 v（即 seed 赋值语句仍取 mf_spring_at/spring.h v(t)）；
   d. 指标口径偏离本 G2-LOCK（改中位为均值、改臂、改窗口、改分母等）；
   e. 会话数 <3 或任一会话有效 n < 12；
   f. 追加补测拼凑（FAIL 后回填数据）。
```

Wait — regarding "无会话 ≥2.0×" — I should be careful about 5: original says "3/3 中位∈[0.8,1.2] 且无会话≥2.0×". If med_r ∈ [0.8,1.2] already, then med_r < 2.0 automatically. So "无会话≥2.0×" presumably refers to something else — perhaps any session whose median ≥2.0×, or any single sample ≥2.0×. Since [0.8,1.2] implies <2.0 for medians, the "无会话≥2.0×" clause only adds meaning if it applies to individual samples or to sessions that miss the window. Actually reread: "PASS：3/3 中位∈[0.8,1.2] 且无会话≥2.0×". Hmm, the most sensible reading: it's a redundant-but-explicit guard, or it applies to raw samples. Since the draft is ambiguous, my review should flag this ambiguity and my locked draft should disambiguate. I'll write: 每会话任一单样本 r_i ≥ 2.0 即该会话计未达标？ That would be very strict. Alternatively: "无会话中位 ≥2.0×（防御性冗余条款；在 [0.8,1.2] 下自动满足）"。Better: flag ambiguity in the gap list, and in the draft make it explicit as applying to single samples: "且 3 会话中均无单样本 ≥2.0×（若采纳较严口径）或（若按冗余口径）删除该条"。Hmm, that's wishy-washy. Let me just propose one concrete locked wording and note the alternative. The user asked for a paste-able draft — I should give one decisive draft, and separately note the ambiguity and my choice.

Actually, thinking about it more: shadow arm had med ratio 2.600. The clause "无会话≥2.0×" — for a session-level statistic. If medians all in [0.8,1.2], then no session has median ≥2.0 trivially. The clause is then either (a) redundant belt-and-braces, or (b) intended as a per-sample bound. I'll note this in gaps, and in the draft write it as a per-sample guard with a session-level redundancy note. Let me pick: "任一会话任一单样本 ≥2.0× → 该会话未达标（若判定过严，LOCK 时可降为会话中位口径并保留 [0.8,1.2] 主判据）"。Hmm. To keep it decisive: I'll draft the stricter single-sample version and mark the softer reading as the fallback. Actually simpler and defensible: keep the draft's original intent as session-level and note it's redundant with [0.8,1.2]; recommend clarifying to per-sample bound. In my draft I'll write it explicitly as per-sample, since that's the only reading that adds information.

Regarding ε: the material doesn't give a number. I'll use placeholder "ε（LOCK 时写死，建议按 v_screen 测量噪声量级取定）" — no invented value. Also v_seed=0 cases: zero arm ratio=0.000 — with IV=0 seed, |0/v_screen| = 0, which is "well-calibrated ratio" but terrible outcome? Wait no — zero arm had ratio 0.000 (vSet=0), and D says "zero/unset 近似，IV 不设≡0". And wgc was best. So ratio=0 is NOT a pass even though it's in a sense "not over-accelerated"? Interesting: ratio metric punishes zero-seed (0 is outside [0.8,1.2]), correctly, because zero seed gives wrong velocity magnitude (though direction of error differs from shadow's over-acceleration). Also unset→nan: ratio undefined when no seed at all. So the metric handles these. Good — the ratio window [0.8,1.2] requires the seed to actually match screen velocity, not merely avoid overshoot. That's a strength I can note.

Also note: #37 shadow J_vis median=5.30, Δv_vis median=272, peak median=100.3 (worst); wgc J_vis=2.75, Δv_vis=64, peak=41.5 (best but still not reaching J≤0.5 / Δv≤20). Use these numbers verbatim in section 3.

For 3) — recommendation: 保留 RMS/位置辅证 as secondary gating, but only lock the speed ratio as the PASS criterion (speed ratio necessary, not sufficient). Reasoning:
- Speed ratio is necessary: shadow 2.6 with worst J/Δv; wgc 0.796 with best J/Δv — same-direction worsening shows ratio tracks the failure mode (over-acceleration → seed speed mismatch).
- But not sufficient: wgc ratio ≈0.8 yet J_vis=2.75, Δv_vis=64 still fail J≤0.5 / Δv≤20. So ratio passing alone doesn't guarantee visual quality targets.
- Recommendation: 主判据锁速度比（本票范围：seed 路径修复，判据直击 seed 问题）；RMS/位置与 J/Δv 列为辅证观测（记录不作 PASS/FAIL 门槛），或挂到后续票（如 WGC 滞后补偿、更精细 IV 估计算法）。Avoid double-locking: if we lock both speed ratio AND J/Δv≤0.5/20, then the fix would fail even in the best observed case (wgc 41.5 peak), making the gate unsatisfiable by the current fix scope — that would be wrong gating. Since the fix scope is "seed 路径", the PASS should test the seed property (speed ratio); the visual residual J/Δv belongs to the broader trajectory-quality gate. This is a strong, well-grounded recommendation. The user explicitly framed: "wgc 臂速度比≈0.8 但仍有 J/Δv 未达标——说明速度比是必要非充分" — so they want this exact argumentation.

Careful nuance: the user says "wgc 臂速度比≈0.8" — material numbers: 183.5/230.5=0.796. I should quote the actual 0.796 and can say "≈0.8". Fine.

### 块 2 — 调用点清单
1) All call sites in repo feeding spring.h / closed-form v to compositor InitialVelocity: path + symbol-level (method name / arm name / assignment statement level).
2) Layers allowed to change this ticket: demo / harness / controller; explicitly which files may move, which forbidden.
3) Reiterate composition.h sha must keep the given value; composition.h's PlanOverride/PlanGraph using spring.h::At to fill CompositorSpring.fromVelocity is FORBIDDEN to change this ticket, only register as 挂账/出票外.

From E1 (allowed to change):
- win_softdemo_src/MainPage.xaml.cs (and live 同源 MotionSoftDemo/MainPage.xaml.cs):
  - CompStartSpring (~L519+): `a.InitialValue=(float)v0; a.InitialVelocity=(float)vel0` — v0/vel0 from shadow state (comment says spring.h)
  - RunAlignProbe: interrupt uses shadow as InitialValue/InitialVelocity
  - RunInterruptSeedProbe: `MfProbe.mf_spring_at(... out seedPos, out seedVel)` → `a2.InitialVelocity=(float)seedVel`
  - RunRateFoldProbe: `a.InitialVelocity=(float)ivel` (ivel from mf_spring_at)
  - RunR2P11Probe: arm `Bv` → `r.InitialVelocity=(float)shV`; arm A 类 → shX+shV (mf_spring_at at tS)
  - RunR2P10Probe / RunR2P9Probe: same pattern, A/B arms shV
  - RunR2P8Probe: arm name starting with A → InitialValue=shX, InitialVelocity=shV
  - RunR2P7Probe: arm A0/Am20 → shX+shV
  - RunR2P2Probe: arm A → shX+shV
  - RunR2P4Probe (ticket source #37): arm `shadow` → `r.InitialVelocity=(float)shV; vSet=shV`; arms={base,shadow,zero,unset,wgc}; evidence source
  - RunR2PostProbe: interrupt path `a2.InitialVelocity=(float)seedVel`
  - Suite RunSuite interrupt segment: shadow seeded retarget (comment "shadow seeded")
- MotionSoftDemo/R3Probe.cs (and win_softdemo_src/R3Probe.cs copy):
  - RunR3P12Probe / Trial: arms `Bv`/`Aex` → `a.InitialVelocity=(float)shV` (mf_spring_at at tSeed); Aex also InitialValue=shX
  - RunR3E3Probe: arm `Bv` → InitialValue unset + InitialVelocity=shV
  - RunR350Probe: arm `Bv` → same; plus d1 overlay `InitialVelocity=(float)ivel` (ivel from mf_spring_at, ε̂ sweep param)
  - Note: some mf_spring_at annotated `// log only; do NOT seed` — these are not seed call sites; mark "仅日志" in the inventory.
- Bridge: `MfProbe.mf_spring_at` ← mf_probe.cpp / fm_capi export, internally calls spring.h closed-form; when changing seed strategy, change the caller assignment, not spring.h itself.

E2 controller layer (allowed to touch "seed 路径" semantics with caution):
- code/controller.h: `Controller::Retarget` takes over value/velocity from current owner (software path spring state). This is AppThread/software controller semantics, NOT directly writing WinRT `SpringScalarNaturalMotionAnimation.InitialVelocity`. If this ticket wants to solidify "no shadow IV" into controller API contract, may touch docs + controller contract; but must NOT use the chance to change composition.h.

E3 forbidden (register only):
- code/composition.h: PlanOverride (~L304–313) `sp.At(elapsed)` → `pp.spring.fromVelocity = st.velocity`; PlanGraph spring segment (~L357–368) similar. This is the production lowering path to CompositorSpring, forbidden to change this ticket; aligning compositor math to spring.h or changing that seed is out-of-ticket (出票外).
- code/spring.h: pure math, not a "feed compositor IV" call site; don't touch unless separate ticket.
- Freezes/bans: 不重开 S1/R-3; 不解冻 Bv; 成功≠采用.

E4 table: may move vs forbidden.

### 可选第三段
One incremental sentence for docs/deepseek_m2_02_api_draft.md: 「H5 未修前 C# 投影勿接影子 IV」— place it in a section. Suggestion: §3.4 or §3.5. The material F says: "请给一句增量警告位置建议（例如 §3.4 或 §3.5）". So I give the paste-able sentence and say which section. I need to pick one. Without seeing the doc, I'd suggest §3.4 (假设与警告 / 未验证清单 area) or §3.5. Since I can't read the file (it's not in the workspace — C:\Users\admin\Temp), I'll recommend §3.4 或 §3.5 with a reason: place near 合成器路径接续假设/未验证项. Let me phrase: 建议放 §3.4（合成器路径接续/未验证假设处）；若该节为纯接续假设则放 §3.5（风险与禁忌清单处）。Since I must give one decisive suggestion, I'll recommend §3.4 and note fallback §3.5. Actually the user said "例如 §3.4 或 §3.5" — they want a concrete suggestion. I'll say: 建议置于 §3.4；若该节已满或专述接续假设，则 §3.5。Hmm, keep it simple: "建议放 §3.4（若该节是合成器接续/未验证假设），否则 §3.5" — fine.

The exact sentence to paste: 「H5 未修前 C# 投影勿接影子 IV」— the user says "可直接粘贴" that sentence. Maybe I can expand slightly but they gave the exact sentence in quotes. I'll provide that sentence plus a one-line rationale for the doc context. Careful: "给 ... 的增量一句（可直接粘贴）：「H5 未修前 C# 投影勿接影子 IV」" — so the deliverable sentence is exactly that. I can add a lead-in like "建议在 §3.4 末尾追加一句：" then the sentence. Maybe expand to "H5 未修前 C# 投影勿接影子 IV（spring.h 闭式 v 作 InitialVelocity，见 M2-H5-SEED 复核）" — but adding extra text beyond the quoted sentence risks overstepping. The user said the incremental sentence is that quoted one; I can give it plus optional annotation. I'll give the sentence verbatim and separately note a suggested parenthetical reference, clearly marked as optional. Better to keep exactly what they gave, and mention in prose where it goes.

### 元信息 content
- 票: "M2-H5-SEED 判据复核 + 调用点清单（文档 only，DRAFT 闸门未 LOCK，本交付不授权采数）"
- 依据文件列表:
  - docs/m2_h5_seed_gate_DRAFT.md（闸门 DRAFT，STATUS=DRAFT NOT LOCKED）
  - docs/m2_01_result.md（M2-01 结题，G3 PRIMARY=H5 Seed 源）
  - artifacts/m2_01_g3_h1h5_audit.txt（G3 审计，四臂数字）
  - artifacts/win_r2_p4_b2vel.txt（#37 解读摘要/证据）
  - 仓库静态扫描（E 调用点清单，本机 sha 已核）
- composition.h 当前 sha256（原样写出）: a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418
- 硬禁重申: 不改 composition.h（sha 锁死为上述值）；不重开 S1/R-3；不解冻/不升级 Bv；不重开 R-2；成功≠采用；本票只改 seed 路径（demo/harness/控制器），不对齐 compositor 弹簧数学到 spring.h；未授权不采数。

The user says "硬禁重申" — restate the hard bans. From A G0: 不重开 S1/R-3；不解冻/不升级 Bv；不重开 R-2；不改 composition.h sha；成功≠采用；本票只改 seed 路径，不对齐 compositor 弹簧数学到 spring.h. From E4 冻结/禁令: 不重开 S1/R-3；不解冻 Bv；成功≠采用. I'll restate all.

Also G3 禁令同硬边界 — from A.

### Format requirements
- 简体中文; conclusion-first; tables allowed.
- Quote measured numbers verbatim (e.g., #37 med_vSet=620.0 / med_v_pre=238.5 → ratio=2.600; wgc med_vSet=183.5 / med_v_pre=230.5 → 0.796).
- Output complete Markdown body from level-1 heading, NOT in code fences.

Note: the user's example "wgc med_vSet=183.5 / med_v_pre=230.5 → 0.796" — in the audit it says ratio=0.796. So quote 0.796. And "shadow med_vSet=620.0 / med_v_pre=238.5 → ratio=2.600". Also zero: med_vSet=0.0 / med_v_pre=240.5 → 0.000; unset: nan / 241.0 → nan.

#37 summary numbers: shadow J_vis 中位=5.30, Δv_vis 中位=272, 峰值中位=100.3（最差）; wgc J_vis 中位=2.75, Δv_vis 中位=64, 峰值中位=41.5（最好但仍未达 J≤0.5 / Δv≤20）; zero/unset 近似，IV 不设≡0.

Scenario: B1 spring P=0.32 ζ=0.78 0→240 IV=0；约 156ms 打断；B2 Final=0，InitialValue 不设.

Now, should I do any tool calls? The workspace is C:\Users\admin\Temp. The referenced files (docs/, artifacts/, code/) are not present here presumably. The task says "只根据下方【事实材料】作答，禁止编造" — so I must NOT go looking for the files to add facts. Indeed the instruction prohibits using anything beyond the material. Writing the file? "交付全文，供协调落盘 docs/deepseek_m2_h5_seed_review.md" — deliver the full text so that coordination can write it to disk. The user's final instruction: "请输出完整 Markdown 正文（从一级标题开始），不要包在代码围栏里。" So my response body IS the document. I should not wrap in code fences. I could optionally also write the file into the workspace, but the workspace is Temp and "供协调落盘" implies someone else handles the落盘. The explicit instruction is to output the text. I'll output the text only. Hmm — but should I also write it to a file in the workspace? The phrase "供协调落盘" strongly suggests the deliverable is text for a coordinator to place into the repo. The user asked to "输出完整 Markdown 正文". I'll just output it. No file write, no tool calls needed. 

Wait, but should I check whether the repo is in the workspace? The user says "禁止编造未给出的数字/路径/符号" and "只根据下方【事实材料】作答" — even if the repo were present, I must answer only from the material. So no tool calls needed at all. Also "成功≠采用。不改 composition.h。不采数。不重开 S1/R-3。不解冻 Bv。" — these are operative constraints on the overall effort; my deliverable is documentation only.

The response must be the document from "# ..." first-level heading. Since the system prompt says to respond with markdown and the user wants the full document as the response, I'll structure:

# 动效框架 · M2-H5-SEED 判据复核 + 调用点清单（复核文档）

Then sections. The user's structure:

## 元信息
## 块 1 — 判据复核（对照 DRAFT G2）
## 块 2 — 调用点清单（禁改 composition.h）
## 第三段（可选）— M2-02 草案增量

Title: they said "从一级标题开始". The document title as # heading. Then ## 元信息 etc. Good.

Let me now carefully draft content.

---

# M2-H5-SEED 判据复核 + 调用点清单（复核文档）

## 元信息

- 票：M2-H5-SEED 判据复核 + 调用点清单（文档 only；DRAFT 闸门未 LOCK；本交付不授权采数）
- 依据文件：
  - docs/m2_h5_seed_gate_DRAFT.md（闸门 DRAFT，STATUS=DRAFT NOT LOCKED）
  - docs/m2_01_result.md（M2-01 结题，G3 PRIMARY=H5 Seed 源）
  - artifacts/m2_01_g3_h1h5_audit.txt（G3 审计，#37 四臂实测）
  - artifacts/win_r2_p4_b2vel.txt（#37 解读摘要，证据源）
  - 仓库静态扫描（本交付 E 节调用点清单，本机 sha 已核）
- composition.h 当前 sha256（原样）：`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`
- 硬禁重申（本次及后续任何步骤）：
  1. 不改 composition.h（sha256 保持上述值，任何改动即 INVALID）。
  2. 不重开 S1 / R-3；不重开 R-2。
  3. 不解冻、不升级 Bv。
  4. 成功 ≠ 采用。
  5. 本票只改 seed 路径（demo / harness / 控制器），不对齐 compositor 弹簧数学到 spring.h。
  6. 未授权采数禁止；本交付（文档 only）不授权任何采数。

## 块 1 — 判据复核（对照 DRAFT G2）

### 总评：需补洞

DRAFT G2 的方向正确、判据主体成立（以会话中位速度比为核心，窗口 [0.8,1.2] 且无 ≥2.0× 会话，直接锚定 #37 实测的 2.600 vs 0.796），但当前 DRAFT 文本中 n、会话独立性、INVALID 边界均为「LOCK 写死」占位，分母退化（v_screen≈0）无处理规则，「无会话 ≥2.0×」的统计口径有歧义。按现状不可执行，必须补洞并 LOCK 后才能采数。

Wait — "需补洞" chosen. Good.

#### 1) PASS 定义是否够硬

PASS 定义原文：「3/3 会话中位 |v_seed/v_screen| ∈ [0.8,1.2] 且无会话 ≥2.0×；相对 #37 的 2.600 已消除」。

够硬的部分：
- 会话中位：对单次打断的异常值稳健，比均值口径更适合打断探针。
- [0.8,1.2]：下限 0.8 意味着 seed 必须真实匹配屏上速度，仅「不超调」不达标——zero 臂 ratio=0.000 直接被排除，判据因此同时排除了「影子过加速」与「欠 seed」两类错误。
- 3/3：要求全部会话通过，禁止挑会话。
- ≥2.0× 上限与 #37 锚定：shadow 臂 med_vSet=620.0 / med_v_pre=238.5 → ratio=2.600 是已知病态，2.0× 上限把它与正常区隔开，判据与已知失败模式一一对应。

缺口：
- G1: n 未写死（DRAFT 仅「建议≥12 次打断或沿用 #37 臂对照结构」），样本量不足时「样本足」无定义，PASS/FAIL 无法判定。
- G2: 「会话」的独立性无定义：同一进程内重复跑 3 次算不算 3 个会话？臂间共享弹簧状态与否？不写死则 3/3 可被同一次运行拼出。
- G3: 分母退化：v_screen≈0 时比值发散/无定义（unset 臂 ratio=nan、zero 臂 ratio=0.000 已在 #37 出现），DRAFT 没有对 |v_screen| 过小样本的处理规则；不写死则这类样本既可被剔除也可被计入，口径可漂移。
- G4: 「无会话 ≥2.0×」统计口径歧义：若指会话中位，则该条与「中位∈[0.8,1.2]」冗余；若指单样本，则约束强得多。必须二选一写死。
- G5: 0.8 下限与 #37 最优臂的边界摩擦：wgc med_vSet=183.5 / med_v_pre=230.5 → ratio=0.796，是四臂中唯一贴近窗口的（且 J/Δv 最优），却恰好低于 0.8。窗口下限若不做 0.796 级别的灵敏度说明，修复方案可能被门槛卡在最优解之下。需在 LOCK 时明确：接受 0.8 下限并预期 wgc 类 seed 需小幅校准，或将下限调整并说明理由。
- G6: v_screen 的测量定义（哪几帧、打断前/后）未写死；D 提到滞后补偿属实现细节可不必冻结，但测量窗口本身属于判据口径，必须写死。
- G7: FAIL 里「样本足」依赖未锁定的 n 与会话数；「追加补测拼凑」已被 DRAFT 列为 INVALID，但「样本足」必须先有数字才能判断是否拼凑。

Hmm wait, G5 — should I be careful? "wgc 臂速度比≈0.8" is stated in material D and user's prompt. 0.796 < 0.8. This is a genuine observation. The user's prompt for block 1 question 1 asks "是否够硬？缺口？" — I can include this. But I must not overreach: I can say the boundary tension exists and LOCK should decide. Yes, include as gap. It's derived purely from given numbers.

Also careful about "INVALID 边界（仍用 spring.h 影子 v 等）" in question 2 — the draft covers in question 2. Fine.

#### 2) LOCK 前写死草案（可粘贴）

I'll present as a block that can be pasted into the DRAFT. Since the whole answer can't be in a code fence... The user said the Markdown body shouldn't be wrapped in code fences — that refers to the whole deliverable. Inside, I can use a fenced code block for the paste-able draft? Hmm. "请输出完整 Markdown 正文（从一级标题开始），不要包在代码围栏里" — the deliverable as a whole shouldn't be in a code fence. Using an internal code fence for a paste-able G2-LOCK clause is legitimate Markdown and improves copy-paste fidelity. But risk: the coordinator pasting the whole doc might then have embedded fences. I think an internal fenced block for the "可粘贴的写死草案" is acceptable and actually desirable; the top-level instruction was about not wrapping the entire answer in a fence. I'll use a fenced block for the paste-able draft, and maybe also a quote style for the M2-02 sentence. Actually, for safety and to be maximally paste-able, I'll use a fenced block with a language-less fence for the draft text. That's standard.

Draft content:

```
【G2-LOCK 写死草案（建议原样并入 DRAFT G2 后 LOCK）】

G2-LOCK-1 样本量：每会话 n=12 次打断/重定向（沿用 #37 臂对照结构，n=12/臂）；有效样本 = 打断后 seed 与屏上速度均可测得且 |v_screen| ≥ ε 的样本（ε 由 LOCK 时按测量噪声写死）。
G2-LOCK-2 会话定义与独立性：会话 = 一次进程级独立运行、场景完整重建、臂间不共享弹簧状态；有效会话数 ≥3，且计入 3/3 的会话必须来自 3 次不同运行（同一进程重复 3 轮只计 1 个会话）。
G2-LOCK-3 臂结构：至少 {修复臂, IV=null+0 降级臂}；沿用 #37 {base, shadow, zero, unset, wgc} 结构时保留臂名与 #37 同义。
G2-LOCK-4 指标：逐样本 r_i = |v_seed,i / v_screen,i|，每会话取中位 med_r；v_screen 测量窗口在 LOCK 时写死（打断前后帧窗）。
G2-LOCK-5 PASS：3/3 会话 med_r ∈ [0.8,1.2]；且任一会话任一单样本 r_i < 2.0（无单样本 ≥2.0×，此为「无会话 ≥2.0×」的强口径；若 LOCK 决定取弱口径，则本款改为「3/3 会话 med_r < 2.0」，与主判据冗余，仅作防御）。
G2-LOCK-6 FAIL：有效样本足（会话数 ≥3 且每会话有效 n ≥ 12）但未达 PASS。
G2-LOCK-7 INVALID（任一即整票 INVALID，不允许补测拼凑）：
  a. composition.h sha256 ≠ a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418；
  b. 未授权采数；
  c. seed 仍来自 spring.h 影子闭式 v（seed 赋值语句仍取 mf_spring_at / spring.h v(t)）；
  d. 口径偏离本 G2-LOCK（改中位为均值、改臂、改窗口、改分母处理规则等）；
  e. 会话数 <3 或任一会话有效 n <12；
  f. FAIL 后追加补测回填拼凑。
G2-LOCK-8 0.8 下限说明：wgc 臂 ratio=0.796 为 #37 实测最优且贴近下限；LOCK 需明确——保留 0.8 下限（预期 wgc 类 seed 需小幅校准）或调整下限，二者择一，不得在执行中变口径。
```

Hmm, G2-LOCK-5's strong interpretation — is that too strict? Original DRAFT: "PASS：3/3 中位∈[0.8,1.2] 且无会话≥2.0×". My draft picks the strong reading (any single sample ≥2.0 → fail). I flag the weak alternative inline. That's reasonable and honest.

Wait, is "任一单样本 ≥2.0×" too strict given #37 shadow had median 2.6 — individual samples would certainly exceed. For the fix arm, we'd want no extreme samples. It's a defensible strong reading. I'll keep with the inline alternative.

Also ε — I say "ε 由 LOCK 时按测量噪声写死" — no invented number. Good.

#### 3) RMS/位置辅证：推荐保留为辅证观测，PASS 只锁速度比

明确推荐：速度比作唯一 PASS 判据；RMS/位置与 J/Δv 保留为记录性辅证（观测并入库，不设 PASS/FAIL 门槛），不并入本票 PASS。

理由（对照 #37）：
- 速度比是必要的：shadow 臂 med_vSet=620.0 / med_v_pre=238.5 → ratio=2.600，同时 J_vis 中位=5.30、Δv_vis 中位=272、峰值中位=100.3，四项同向最差；wgc 臂 med_vSet=183.5 / med_v_pre=230.5 → ratio=0.796，同时 J_vis 中位=2.75、Δv_vis 中位=64、峰值中位=41.5，四项同向最好。速度比与视觉残余同向变差，说明它抓得住本票的病根（seed 速度与屏上速度失配）。
- 速度比非充分：wgc 臂 ratio≈0.8（0.796）已是四臂最优，但 J_vis 中位=2.75、Δv_vis 中位=64 仍不达标（目标 J≤0.5 / Δv≤20）。若把 J/Δv 也锁进本票 PASS，则 #37 实测最优臂也过不了，本票（只改 seed 路径）将无解——把轨迹质量门槛放进 seed 修复票是错误分层。
- 结论：本票 PASS 只锁速度比（判 seed 是否修好）；J/Δv 与 RMS/位置留作辅证记录，其达标挂账到轨迹质量类后续票（如 WGC 滞后补偿等）。zero/unset 臂的对照意义继续保留：zero 臂 ratio=0.000、unset 臂 ratio=nan，证明 [0.8,1.2] 窗口能区分「不设 seed」与「正确 seed」，辅证价值不减。

Hmm wait, one nuance: is it right to say "本票将无解" — the material says "最优估计是 WGC 末两帧差分（并论及滞后补偿，属实现细节）" and fix directions: WGC/屏一致速度 seed 或 IV=null+0. If J/Δv gates were included, wgc arm fails them (2.75 > 0.5). The fix ticket could still try harder (lag compensation) to improve J/Δv... but lag compensation is implementation detail explicitly out of scope for this review. The clean argument: 本票范围是 seed 路径；J/Δv 门槛涉及轨迹残余与滞后补偿，超范围；锁入 PASS 会使命题（seed 修复）不可判定。I'll phrase carefully: "若同时锁 J/Δv，则本票 PASS 依赖滞后补偿等非 seed 因素，判据不再针对 seed 修复命题" — that's the precise reasoning. Avoid overclaiming "无解" absolutely. Good.

Also mention: 位置/RMS 辅证 worth keeping because shadow arm's failure shows both velocity ratio and positional residual; and material D's wgc still-failing J/Δv shows sufficiency gap. My recommendation stands: 保留为记录，不锁.

#### 块 2 — 调用点清单（禁改 composition.h）

##### 2.1 仓库内「spring.h / 闭式 v → compositor InitialVelocity」调用点（符号级）

Path A: win_softdemo_src/MainPage.xaml.cs（真机 live 同源常在 MotionSoftDemo/MainPage.xaml.cs；仓库箱内副本在 win_softdemo_src）

| 方法（约行号） | 臂/路径 | 赋值语句级细节 |
|---|---|---|
| CompStartSpring（约 L519+） | 通用启动 | `a.InitialValue=(float)v0; a.InitialVelocity=(float)vel0`（v0/vel0 来自影子状态，注释写明 spring.h） |
| RunAlignProbe | 打断路径 | 打断时用 shadow 作 InitialValue / InitialVelocity |
| RunInterruptSeedProbe | 打断路径 | `MfProbe.mf_spring_at(... out seedPos, out seedVel)` → `a2.InitialVelocity=(float)seedVel` |
| RunRateFoldProbe | 通用 | `a.InitialVelocity=(float)ivel`（ivel 自 mf_spring_at） |
| RunR2P11Probe | 臂 Bv | `r.InitialVelocity=(float)shV`；臂 A 类 → shX+shV（mf_spring_at at tS） |
| RunR2P10Probe | 臂 A/B | 同模式，shV |
| RunR2P9Probe | 臂 A/B | 同模式，shV |
| RunR2P8Probe | 臂名以 A 开头 | InitialValue=shX, InitialVelocity=shV |
| RunR2P7Probe | 臂 A0 / Am20 | shX+shV |
| RunR2P2Probe | 臂 A | shX+shV |
| RunR2P4Probe（派题 #37） | 臂 shadow | `r.InitialVelocity=(float)shV; vSet=shV`；arms={base, shadow, zero, unset, wgc}；证据源 |
| RunR2PostProbe | interrupt 路径 | `a2.InitialVelocity=(float)seedVel` |
| RunSuite 打断段 | shadow seeded retarget | 注释「shadow seeded」 |

Path B: MotionSoftDemo/R3Probe.cs（及 win_softdemo_src/R3Probe.cs 副本）

| 方法 / Trial | 臂/路径 | 赋值语句级细节 |
|---|---|---|
| RunR3P12Probe / Trial | 臂 Bv / Aex | `a.InitialVelocity=(float)shV`（mf_spring_at at tSeed）；Aex 另设 InitialValue=shX |
| RunR3E3Probe | 臂 Bv | InitialValue unset + InitialVelocity=shV |
| RunR350Probe | 臂 Bv | 同上；另有 d1 overlay `InitialVelocity=(float)ivel`（ivel 自 mf_spring_at，ε̂ 扫参） |

备注：
- 若干 `mf_spring_at` 标注 `// log only; do NOT seed` —— 这些不是 seed 调用点，清单中标记「仅日志」，不属于修复面。
- 桥接：`MfProbe.mf_spring_at` ← mf_probe.cpp / fm_capi 导出，内部调 spring.h 闭式。改 seed 策略改的是调用方赋值，不是 spring.h 本身。

##### 2.2 控制器层（本票可触及「seed 路径」语义，须谨慎）

- code/controller.h：`Controller::Retarget` 从当前 owner 的 value/velocity 交接（软件路径 spring 状态）。这是 AppThread/软件控制器语义，不是直接写 WinRT `SpringScalarNaturalMotionAnimation.InitialVelocity`。若本票要把「禁止影子 IV」固化到控制器 API 契约，可动文档+控制器契约；但不得借此改 composition.h。

##### 2.3 禁止本票改动（只登记）

- code/composition.h：`PlanOverride`（约 L304–313）`sp.At(elapsed)` → `pp.spring.fromVelocity = st.velocity`；`PlanGraph` 弹簧段（约 L357–368）同类。这是 lowering 到 CompositorSpring 的产品路径，本票禁止改；对齐 compositor 数学到 spring.h 或改该 seed 出票外。
- code/spring.h：纯数学，不是「喂给 compositor IV」的调用点；勿改除非另票。
- 冻结/禁令：不重开 S1/R-3；不解冻 Bv；成功≠采用。

##### 2.4 本票可能动 vs 禁止动（表）

| 层 | 文件 | 本票 |
|---|---|---|
| demo/harness（可能动） | MotionSoftDemo/R3Probe.cs、win_softdemo_src/R3Probe.cs、win_softdemo_src/MainPage.xaml.cs（及真机 MotionSoftDemo/MainPage.xaml.cs）、相关 harness 脚本/探针接线 | 允许改（仅 seed 赋值路径） |
| 控制器（有条件） | code/controller.h | 仅限 seed 契约/文档级，且不触及 composition.h |
| 文档（可能动） | 本票相关文档 | 允许 |
| 禁止动 | code/composition.h（sha 锁死）、LOCKED 闸门/结果文件本体、DRAFT 闸门本体（本交付不改 DRAFT 文件）、spring.h 数学对齐类改动、采数 | 禁止 |

##### 2.5 sha 与挂账重申

- composition.h sha256 必须保持给定值：`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（本机已核，PRE=POST）。
- composition.h 内 PlanOverride / PlanGraph 用 spring.h::At 填 CompositorSpring.fromVelocity 的路径本票禁止改，只登记为挂账 / 出票外。

## 第三段（可选）— M2-02 草案增量

给 docs/deepseek_m2_02_api_draft.md 的增量一句（可直接粘贴）：

> H5 未修前 C# 投影勿接影子 IV。

放置位置建议：§3.4（若该节承载合成器路径接续假设/未验证项，则追加至该节末尾）；若 §3.4 专述接续假设而不含风险清单，则放 §3.5。一句话理由（供落盘时附注，不必进正文）：H5 PRIMARY（spring.h 闭式 v 作 compositor InitialVelocity → 过加速）未修前，C# 投影若接入影子 IV 会把同一病根带进投影层。

Hmm, the user asked "给 docs/deepseek_m2_02_api_draft.md 的增量一句（可直接粘贴）：「H5 未修前 C# 投影勿接影子 IV」。说明放在哪一节。" So the deliverable is the sentence + section placement. Good.

Let me double check all numbers quoted:

- sha256: a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418 ✓
- shadow: n=12, med_vSet=620.0, med_v_pre=238.5, ratio=2.600 ✓
- zero: n=12, med_vSet=0.0, med_v_pre=240.5, ratio=0.000 ✓
- unset: n=12, med_vSet=nan, med_v_pre=241.0, ratio=nan ✓
- wgc: n=12, med_vSet=183.5, med_v_pre=230.5, ratio=0.796 ✓
- #37 J/Δv: shadow J_vis 中位=5.30, Δv_vis 中位=272, 峰值中位=100.3（最差）✓; wgc J_vis 中位=2.75, Δv_vis 中位=64, 峰值中位=41.5（最好但仍未达 J≤0.5 / Δv≤20）✓
- Scenario: B1 spring P=0.32 ζ=0.78 0→240 IV=0; ~156ms 打断; B2 Final=0, InitialValue 不设 ✓
- M2-01: G3 PRIMARY = H5 Seed 源 ✓
- composition.h PRE=POST ✓

Do not invent: line numbers — the material gives "约 L519+", "约 L304–313", "约 L357–368". Use "约" as given.

"臂 A 类" in R2P11: "臂 `Bv` → `r.InitialVelocity=(float)shV`；臂 A 类 → shX+shV（mf_spring_at at tS）" — keep.

R2P8: "臂名以 A 开头 → InitialValue=shX, InitialVelocity=shV" ✓.

E2: "Controller::Retarget 从当前 owner 的 value/velocity 交接（软件路径 spring 状态）" ✓.

Now about 块 1 question 1's answer structure: "是否够硬？缺口？" — I'll structure: 够硬的部分 (bullet) / 缺口 (numbered G1..G7). Good, conclusion first: 够硬但未闭口? The overall verdict already given at top. Within 1), I can lead with "够硬的核心 + 缺口如下".

One more check — the user says "块 1 先给总评：有条件同意 / 不同意 / 需补洞（择一），再列缺口". So block 1 starts with 总评. Then questions 1,2,3. My 总评 = 需补洞. Should I use exactly one of the three labels? "需补洞" yes. I might also briefly note "若按有条件同意口径，也可表述为：同意判据方向，条件是 LOCK 前补洞" — better to keep it crisp: pick 需补洞, explain why it's not "有条件同意" (the holes are inside the DRAFT text itself). Actually let me just pick 需补洞 with one-line justification. Keep it clean.

Also should the doc mention "本交付不授权采数" and "成功≠采用" — yes in 元信息 硬禁.

Length: The doc should be complete but not bloated. Let me write final markdown carefully with tables.

One concern: the user says "不要输出与交付无关的寒暄" — so no preamble before the doc. The response starts directly with the # heading.

Also "从一级标题开始" — first line is `# ...`. The document title. Let me title it: `# M2-H5-SEED 判据复核 + 调用点清单（文档 only）`. The meta info says 票: M2-H5-SEED 判据复核 + 调用点清单. Good.

Let me also reconsider whether to include "本文档定位" line — meta covers it.

Regarding G2 draft original line "G1：禁止 spring.h 影子闭式速度作 compositor InitialVelocity；主路径=WGC/屏一致速度 seed；或降级 IV=null+0（LOCK 写死默认/分级）" — should my draft include the seed 主路径/降级 default? The user's question 2 asks specifically for n、会话独立性、INVALID 边界写死. I can optionally add a G2-LOCK-0 clause about seed source: 主路径=WGC/屏一致速度 seed，降级=IV=null+0，禁止影子 v. That aligns with G1. I'll add it as G2-LOCK-0 since INVALID item (c) references seed source. Good idea — makes the draft complete for INVALID c.

Also in the draft, "每会话 n=12" — the material suggests ≥12 or follow #37 structure. I choose 12 (material's #37 used n=12 per arm; DRAFT suggests ≥12). Write "n=12（≥12 亦可，但必须写死单一值；#37 结构即 n=12）". Keep decisive: n=12.

Also INVALID: "改 composition.h" per DRAFT. Included as (a). Also "未授权采数" (b). "仍用 spring.h 影子 v" (c). "口径偏离" (d). "追加补测拼凑" (f). Good.

Now — one thing about the 缺口 numbering: G0/G1/G2/G3 are already used in the DRAFT (G0 hard bans, G1 seed source ban, G2 scenarios, G3 bans). To avoid confusion with DRAFT's own clause numbers, I should NOT reuse G1/G2/G3 for my gap list! Use 缺口 A/B/C or 「缺 1」... Let me use "缺口-1 ... 缺口-7" or just "缺口 1)". I'll use 「缺口 1」…「缺口 7」.

Also in my LOCK draft, prefixing clauses G2-LOCK-1 etc. is fine (distinct namespace).

For the 3) recommendation, the user wants "明确推荐与理由". I'll write 推荐：… then 理由 1（必要）、2（非充分）、3（分层）. 

Let me also double-check: "zero/unset 近似，IV 不设≡0" — from D. I reference in 3): zero arm ratio 0.000 / unset nan demonstrates window distinguishes. Careful: does [0.8,1.2] window "distinguish 不设 seed 与 正确 seed"? zero→0.000 fails window; unset→nan fails window. Yes.

But wait — is zero arm "不设≡0" the IV=null+0 downgrade? G1's downgrade is "IV=null+0". In #37, zero arm = 0f and unset = 不设, and D says they're approximately equivalent, IV 不设≡0. So the downgrade path IV=null+0 would land at ratio 0.000/outside window → would FAIL the PASS window. Hmm! That's a subtle but important consistency point: if G1 allows "降级 IV=null+0（LOCK 写死默认/分级）" as a fallback, but the PASS window [0.8,1.2] necessarily fails for zero-seed (ratio=0.000), then the downgrade path can never PASS this gate. Is that a contradiction to flag?

Think: G1 says 主路径=WGC/屏一致速度 seed；或降级 IV=null+0. If the main path achieves ratio∈[0.8,1.2], PASS. The downgrade is a fallback when main path unavailable. If a session uses the downgrade, its ratio would be ~0 → session fails → ticket FAIL. So the downgrade is a product fallback, not a PASS pathway. The gate measures the main path. That's coherent: the gate is about whether the fix (main path) works; downgrade exists for product robustness but can't "pass" the gate. Worth one line in gaps? It's more of a semantic clarification: 判据只考核主路径；降级臂仅作对照。I could add to the LOCK draft: "降级臂（IV=null+0）仅作对照登记，不计入 3/3 会话"。That's a good, derived-from-material clarification. But careful — is it "编造"? It's a logical consequence of the given numbers (zero ratio 0.000 is outside [0.8,1.2]) plus G1 text. I'll add it as a clarification clause in the draft with justification note. This is genuinely valuable for coordination. Add G2-LOCK-3 refinement: 臂结构至少 {修复臂, IV=null+0 降级臂}；降级臂仅作对照与 INVALID 证明，不计入 3/3 PASS 会话（zero 臂 ratio=0.000、unset 臂 ratio=nan 均落于窗口外，降级路径自身无法满足 [0.8,1.2]）。That's solid.

Hmm, but hold on: is it right that downgrade "无法满足"? If v_screen were also ~0 (rest), ratio 0/0 undefined. If v_screen > 0, |0/v| = 0 ∉ [0.8,1.2]. So yes, zero-seed always fails the window (except degenerate). Safe to state with the caveat "按 [0.8,1.2] 窗口". Good.

Also G2-LOCK-4 v_screen measurement — keep one line, no invented frame counts.

I think there's one more subtle gap: "相对 #37 的 2.600 已消除" — the PASS mentions this but it's descriptive, not an operational criterion; operationalized via the 2.0× bound. Fine, covered.

Also 缺口 about 会话间臂配置：3/3 会话是同一臂重复 3 次，还是每臂 1 次？"会话≥3" per DRAFT — with arms {fix, zero...}, the 3 sessions must presumably be the fix arm across 3 runs (plus control arms each session). My G2-LOCK-2/3 covers: 计入 3/3 的会话来自修复臂的 3 次独立运行，对照臂随会话跑。Hmm, is that in the material? DRAFT G2: "场景约定打断/重定向探针；会话≥3；每会话 n 按 LOCK 写死（建议≥12 次打断或沿用 #37 臂对照结构）。指标…PASS：3/3 中位∈…"。So 3 sessions each with n interrupts; the arms structure optional (#37 style). To keep the draft faithful without over-inventing, I'll say: 3/3 判定的 3 个会话须针对修复路径（修复臂）各独立运行；对照臂可随同会话采集，仅作对照。This is an operational necessity, flagged as "本复核给出的写死建议" not as material fact. The user asked me to produce the draft — it's my recommended wording, clearly presented as such. OK.

Now, also the user says in block 2 question 2: "本票允许改的层：demo / harness / 控制器；明确哪些文件可能动、哪些禁止动" — E4 table covers it.

Block 2 question 3: "重申 composition.h sha 必须保持给定值；composition.h 内 PlanOverride/PlanGraph 用 spring.h::At 填 CompositorSpring.fromVelocity 的路径本票禁止改，只登记为挂账/出票外" — covered in 2.5. Note material says PlanOverride uses `sp.At(elapsed)` → `pp.spring.fromVelocity = st.velocity` (from E3). Hmm interesting: E3 says "PlanOverride（约 L304–313）`sp.At(elapsed)` → `pp.spring.fromVelocity = st.velocity`" — wait the user's block-2 item 3 says "用 spring.h::At 填 CompositorSpring.fromVelocity". The material E3 says "`sp.At(elapsed)` → `pp.spring.fromVelocity = st.velocity`". I'll quote E3's form (sp.At(elapsed), st.velocity) and can mention "spring.h::At 填 CompositorSpring.fromVelocity" per the user's phrasing. To be safe I'll mirror E3 exactly: `sp.At(elapsed)` → `pp.spring.fromVelocity = st.velocity`. And PlanGraph 弹簧段（约 L357–368）同类. Good.

Now write the final document. Also ensure: 简体中文, tables, no fabrications, no fluff. The response should be ONLY the document (no lead-in sentence? "不要输出与交付无关的寒暄" — I'll start directly with the heading).

Let me also double check the 元信息 票 line: "票：M2-H5-SEED 判据复核 + 调用点清单（文档 only，DRAFT 闸门未 LOCK，本交付不授权采数）" — reproduce.

Structure headings:
# M2-H5-SEED 判据复核 + 调用点清单
## 元信息
## 块 1 — 判据复核（对照 DRAFT G2）
### 总评：需补洞
### 1) PASS 定义复核
### 2) LOCK 前写死草案（可粘贴）
### 3) RMS/位置辅证建议
## 块 2 — 调用点清单（禁改 composition.h）
### 2.1 调用点清单（路径 + 符号级）
### 2.2 控制器层
### 2.3 禁止改动（只登记）
### 2.4 本票可能动 vs 禁止动
### 2.5 sha 保持与挂账
## 第三段（可选）— M2-02 API 草案增量

Let me write it fully now.

Careful with quotes of numbers: "#37 复算：shadow arm med_vSet=620.0 / med_v_pre=238.5 → ratio=2.600" — the user's example formatting uses "med_vSet=620.0 / med_v_pre=238.5 → ratio=2.600". I'll use that.

J_vis numbers: "shadow J_vis 中位=5.30 Δv_vis 中位=272 峰值中位=100.3（最差）". "wgc J_vis 中位=2.75 Δv_vis 中位=64 峰值中位=41.5（最好但仍未达 J≤0.5 / Δv≤20）".

Scenario quote: "B1 spring P=0.32 ζ=0.78 0→240 IV=0；约 156ms 打断；B2 Final=0，InitialValue 不设" — use in block 1 §1 context for anchoring.

Compose now. Also the WGC lag compensation note: "D 指出最优估计是 WGC 末两帧差分（并论及滞后补偿，属实现细节，本票复核不必冻结）" — mention in §3 or §1 as out-of-scope note. Put in §3 理由 3 or in gaps as G6 note. I'll add a line in 3): 滞后补偿按 D 属实现细节，本复核不冻结，但速度比窗口须容忍测量滞后（故 [0.8,1.2] 而非点值 1.0）— hmm, is that interpretation safe? [0.8,1.2] tolerance rationale — the material doesn't state why the window was chosen. I should not invent the rationale. Just note: 滞后补偿属实现细节不冻结，但 LOCK 时需保证 v_screen 测量窗口与 seed 时刻定义一致，否则比值天然含滞后偏差。That's a fair operational note without inventing rationale for the 0.8/1.2 choice. Keep it in G6.

Let me finalize wording for gap 5 (0.796 vs 0.8 boundary). "wgc med_vSet=183.5 / med_v_pre=230.5 → ratio=0.796" — this is the ratio the metric computes (183.5/230.5 = 0.7961). Given. "恰好低于 0.8 下限" — true. I'll present it as: LOCK 前需明确 0.8 下限与实测最优 0.796 的边界关系（或调下限，或声明 wgc 类 seed 需校准）；避免修复臂落在 0.796–0.8 区间时被机械判 FAIL。Good.

Draft the document now, being careful with markdown. Use code fence for the paste-able draft. Inside the fence, no backticks issues.

One more consideration: should the whole answer be just the markdown? Yes. No final "note" outside. The user said don't output unrelated pleasantries.

Also check: "块 1 先给总评…再列缺口" — my §1 (PASS 复核) contains 够硬部分 + 缺口 list. The 总评 comes first. Good.

Draft final:

---

# M2-H5-SEED 判据复核 + 调用点清单

## 元信息

| 项 | 内容 |
|---|---|
| 票 | M2-H5-SEED 判据复核 + 调用点清单（文档 only；DRAFT 闸门未 LOCK；本交付不授权采数） |
| 依据文件 | docs/m2_h5_seed_gate_DRAFT.md（闸门 DRAFT，STATUS=DRAFT NOT LOCKED）；docs/m2_01_result.md（M2-01 结题）；artifacts/m2_01_g3_h1h5_audit.txt（G3 审计）；artifacts/win_r2_p4_b2vel.txt（#37 解读/证据）；仓库静态扫描（E 节调用点清单，本机 sha 已核） |
| composition.h 当前 sha256 | `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` |

硬禁重申：
1. 不改 composition.h，sha256 必须保持 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（PRE=POST，本机已核）。
2. 不重开 S1 / R-3；不重开 R-2。
3. 不解冻、不升级 Bv。
4. 成功 ≠ 采用。
5. 本票只改 seed 路径（demo / harness / 控制器），不对齐 compositor 弹簧数学到 spring.h。
6. 未授权采数禁止；本交付（文档 only）不授权任何采数，不得因本文档而开始采数。

## 块 1 — 判据复核（对照 DRAFT G2）

### 总评：需补洞

...

Good. Now write everything out. I'll aim for a complete but tight document.

For §2) draft fence:

```
G2-LOCK-0（seed 源）主路径 = WGC/屏一致速度 seed；降级 = IV=null+0（LOCK 写死默认/分级）；禁止 spring.h 影子闭式 v 作 compositor InitialVelocity。
G2-LOCK-1（样本量）每会话 n=12 次打断/重定向（沿用 #37 臂对照结构 n=12/臂）；有效样本 = seed 与 v_screen 均可测得且 |v_screen| ≥ ε（ε 由 LOCK 按测量噪声写死，本复核不代定数值）。
G2-LOCK-2（会话独立性）会话 = 一次进程级独立运行、场景完整重建、臂间不共享弹簧状态；计入 3/3 的 3 个会话必须来自 3 次不同运行（同一进程重复 3 轮只计 1 个会话），且均针对修复路径。
G2-LOCK-3（臂结构）至少 {修复臂, IV=null+0 降级臂}；降级臂仅作对照登记，不计入 3/3（zero 臂 ratio=0.000、unset 臂 ratio=nan，落于 [0.8,1.2] 之外，降级路径自身无法满足窗口）。
G2-LOCK-4（指标口径）逐样本 r_i = |v_seed,i / v_screen,i|，会话中位 med_r；v_screen 测量窗口与 seed 时刻定义在 LOCK 写死，两定义必须一致（滞后补偿属实现细节不冻结，但不得造成口径漂移）。
G2-LOCK-5（PASS）3/3 会话 med_r ∈ [0.8,1.2]，且任一单样本 r_i < 2.0（即无单样本 ≥2.0×，相对 #37 shadow 2.600 已消除）；若 LOCK 选择弱口径则改为「无会话 med_r ≥2.0×」（与主判据冗余，仅防御）。
G2-LOCK-6（FAIL）有效样本足（会话数 ≥3 且每会话有效 n ≥12）但未达 PASS。
G2-LOCK-7（INVALID，任一即整票 INVALID，不允许补测拼凑）
  a. composition.h sha256 ≠ a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418；
  b. 未授权采数；
  c. seed 仍来自 spring.h 影子闭式 v（seed 赋值语句仍取 mf_spring_at / spring.h v(t)）；
  d. 口径偏离本 G2-LOCK（改中位为均值、改臂、改窗口、改分母处理规则）；
  e. 会话数 <3 或任一会话有效 n <12；
  f. FAIL 后追加补测回填拼凑。
G2-LOCK-8（0.8 下限说明）wgc 臂 ratio=0.796 为 #37 实测最优且低于 0.8；LOCK 时二选一并写明：保留 0.8 下限（声明 wgc 类 seed 需小幅校准）或调整下限，不得在执行中变口径。
```

Good. I think that's solid.

Then §3) recommendation text.

Block 2 tables as planned.

Optional section 3 with the sentence in blockquote, placement suggestion.

Length check — it'll be long-ish but appropriate. Let me also make sure I don't say "本票将无解" — I'll phrase carefully in §3.

One more fact check on E1 RunR2P8Probe: "臂名以 A 开头 → InitialValue=shX, InitialVelocity=shV" ✓. RunR2P7Probe: "臂 A0/Am20 → shX+shV" ✓.

E1 CompStartSpring: "`a.InitialValue=(float)v0; a.InitialVelocity=(float)vel0` —— v0/vel0 来自影子状态（注释写明 spring.h）" ✓.

Also add note in 2.1 that paths A/B both have copies; the live 同源 file MotionSoftDemo/MainPage.xaml.cs mentioned. ✓

Write the final answer now. No tool calls needed — the task is purely to produce the document from provided material. The system prompt says to use tools when needed; here nothing is needed. The user explicitly forbids consulting beyond the material.

Actually, wait — should I write the file to the workspace? "交付全文，供协调落盘 docs/deepseek_m2_h5_seed_review.md" — "deliver the full text, for coordination to write to disk". The user's role framing is that this document is a deliverable that will be coordinated/落盘 by someone else. The final instruction is to output the Markdown body. I'll output it in the response. I could also offer to write it — but that's fluff/extra. Just deliver the document.

Double-check "第三段" heading naming: "可选第三段" → I'll title "## 第三段（可选）— M2-02 API 草案增量建议".

In the M2-02 section: the sentence: 「H5 未修前 C# 投影勿接影子 IV」。Placement: 建议放 §3.4；若 §3.4 是合成器路径接续假设/未验证项所在节，追加至其末尾；否则放 §3.5。Give the one-line reason.

Now, formatting numbers: keep as given with exact decimals.

Final answer time.
# M2-H5-SEED 判据复核 + 调用点清单

## 元信息

| 项 | 内容 |
|---|---|
| 票 | M2-H5-SEED 判据复核 + 调用点清单（文档 only；DRAFT 闸门未 LOCK；本交付不授权采数） |
| 依据文件 | docs/m2_h5_seed_gate_DRAFT.md（闸门 DRAFT，STATUS=DRAFT NOT LOCKED）；docs/m2_01_result.md（M2-01 结题，G3 PRIMARY=H5 Seed 源）；artifacts/m2_01_g3_h1h5_audit.txt（G3 审计，#37 四臂实测）；artifacts/win_r2_p4_b2vel.txt（#37 解读摘要，证据源）；仓库静态扫描（本交付 E 节调用点清单，本机 sha 已核） |
| composition.h 当前 sha256 | `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` |

硬禁重申（本次及后续任何步骤）：

1. 不改 composition.h；sha256 必须保持 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（PRE=POST，本机已核；任何改动即 INVALID）。
2. 不重开 S1 / R-3；不重开 R-2。
3. 不解冻、不升级 Bv。
4. 成功 ≠ 采用。
5. 本票只改 seed 路径（demo / harness / 控制器），不对齐 compositor 弹簧数学到 spring.h。
6. 未授权采数禁止；本交付（文档 only）不授权任何采数，不得因本文档存在而开始采数。

## 块 1 — 判据复核（对照 DRAFT G2）

### 总评：需补洞

DRAFT G2 的判据方向正确、主体成立：以会话中位速度比为核心，窗口 [0.8,1.2] 且无 ≥2.0× 会话，直接锚定 #37 实测的 2.600 与 0.796 两个已知极值，能同时排除「影子过加速」与「欠 seed / 不设 seed」两类错误。但当前 DRAFT 文本中 n、会话独立性、INVALID 边界均为「LOCK 写死」占位，分母退化（v_screen≈0）无处理规则，「无会话 ≥2.0×」的统计口径有歧义，0.8 下限与实测最优 0.796 存在边界摩擦。按现状不可执行，必须在 LOCK 前补齐缺口；补洞方案见下方第 2) 条写死草案。

### 1) PASS 定义是否够硬

PASS 定义原文：「3/3 会话中位 |v_seed/v_screen| ∈ [0.8,1.2] 且无会话 ≥2.0×；相对 #37 的 2.600 已消除」。

**够硬的部分：**

- 会话中位：对单次打断的异常值稳健，比均值更适合打断探针。
- [0.8,1.2] 窗口：下限 0.8 要求 seed 真实匹配屏上速度，仅「不超调」不达标——zero 臂 ratio=0.000、unset 臂 ratio=nan 均被窗口排除，判据因此同时防住「过 seed」与「欠 seed」。
- 3/3：全部会话通过才 PASS，禁止挑会话。
- ≥2.0× 上限与 #37 锚定：shadow 臂 med_vSet=620.0 / med_v_pre=238.5 → ratio=2.600 是已知病态，2.0× 上限将其与正常区隔开；判据与已定位失败模式一一对应。

**缺口：**

- 缺口 1（n 未写死）：DRAFT 仅「建议≥12 次打断或沿用 #37 臂对照结构」。「样本足」无数字则 FAIL 判定无据，也无法识别「补测拼凑」。
- 缺口 2（会话独立性无定义）：同一进程重复跑 3 轮算 1 个还是 3 个会话？臂间是否共享弹簧状态？不写死则 3/3 可由同一次运行拼出，失去「3 个会话」的意义。
- 缺口 3（分母退化无规则）：v_screen≈0 时比值发散或无定义（#37 已出现 unset 臂 ratio=nan、zero 臂 ratio=0.000）。DRAFT 未规定 |v_screen| 过小样本的处理方式，口径可漂移。
- 缺口 4（「无会话 ≥2.0×」口径歧义）：若指会话中位，则该条与「中位∈[0.8,1.2]」冗余；若指单样本，约束强度完全不同。必须二选一写死（草案取强口径，见下）。
- 缺口 5（0.8 下限与实测最优的边界摩擦）：wgc med_vSet=183.5 / med_v_pre=230.5 → ratio=0.796，是 #37 四臂中唯一贴近窗口且 J/Δv 最优的臂，却恰好低于 0.8。不写死边界说明，修复臂落在 0.796–0.8 区间时会被机械判 FAIL。
- 缺口 6（v_screen 测量口径未写死）：测量取哪几帧、打断前还是打断后，DRAFT 未写死。滞后补偿按 D 属实现细节本复核不冻结，但测量窗口本身是判据口径的一部分，且必须与 seed 时刻定义一致，否则比值天然含滞后偏差。
- 缺口 7（FAIL 判定依赖未锁定参数）：「样本足」取决于未锁定的 n 与会话数，FAIL 判据因此暂不可判定。

### 2) LOCK 前写死草案（可粘贴）

```text
G2-LOCK-0（seed 源）：主路径 = WGC/屏一致速度 seed；降级 = IV=null+0（LOCK 写死默认/分级）；
  禁止 spring.h 影子闭式 v 作 compositor InitialVelocity。
G2-LOCK-1（样本量）：每会话 n=12 次打断/重定向（沿用 #37 臂对照结构 n=12/臂）；
  有效样本 = seed 与 v_screen 均可测得且 |v_screen| ≥ ε（ε 由 LOCK 按测量噪声写死，本复核不代定数值）。
G2-LOCK-2（会话独立性）：会话 = 一次进程级独立运行、场景完整重建、臂间不共享弹簧状态；
  计入 3/3 的 3 个会话必须来自 3 次不同运行（同一进程重复 3 轮只计 1 个会话），且均针对修复路径。
G2-LOCK-3（臂结构）：至少 {修复臂, IV=null+0 降级臂}；降级臂仅作对照登记，不计入 3/3
  （zero 臂 ratio=0.000、unset 臂 ratio=nan，均落于 [0.8,1.2] 之外，降级路径自身无法满足窗口）。
G2-LOCK-4（指标口径）：逐样本 r_i = |v_seed,i / v_screen,i|，会话中位 med_r；
  v_screen 测量窗口与 seed 时刻定义在 LOCK 时写死，两者必须一致（滞后补偿属实现细节，不冻结但不得造成口径漂移）。
G2-LOCK-5（PASS）：3/3 会话 med_r ∈ [0.8,1.2]，且任一单样本 r_i < 2.0（即无单样本 ≥2.0×，
  相对 #37 shadow 2.600 已消除）；若 LOCK 选择弱口径，则改为「无会话 med_r ≥ 2.0×」（与主判据冗余，仅作防御）。
G2-LOCK-6（FAIL）：有效样本足（会话数 ≥3 且每会话有效 n ≥12）但未达 PASS。
G2-LOCK-7（INVALID，任一即整票 INVALID，不允许补测拼凑）：
  a. composition.h sha256 ≠ a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418；
  b. 未授权采数；
  c. seed 仍来自 spring.h 影子闭式 v（seed 赋值语句仍取 mf_spring_at / spring.h v(t)）；
  d. 口径偏离本 G2-LOCK（改中位为均值、改臂、改窗口、改分母处理规则等）；
  e. 会话数 <3 或任一会话有效 n <12；
  f. FAIL 后追加补测回填拼凑。
G2-LOCK-8（0.8 下限说明）：wgc 臂 ratio=0.796 为 #37 实测最优且低于 0.8；
  LOCK 时二选一并写明：保留 0.8 下限（声明 wgc 类 seed 需小幅校准），或调整下限；不得在执行中变口径。
```

### 3) RMS/位置辅证：保留为辅证观测，PASS 只锁速度比

**明确推荐：速度比作唯一 PASS 判据；RMS/位置与 J/Δv 保留为记录性辅证（观测入库，不设 PASS/FAIL 门槛），不并入本票 PASS。**

理由（对照 #37）：

- 速度比是**必要**的：shadow 臂 med_vSet=620.0 / med_v_pre=238.5 → ratio=2.600，同时 J_vis 中位=5.30、Δv_vis 中位=272、峰值中位=100.3，四项同向最差；wgc 臂 med_vSet=183.5 / med_v_pre=230.5 → ratio=0.796，同时 J_vis 中位=2.75、Δv_vis 中位=64、峰值中位=41.5，四项同向最好。速度比与视觉残余同向变差，说明它抓得住本票病根（seed 速度与屏上速度失配）。
- 速度比**非充分**：wgc 臂 ratio=0.796（≈0.8）已是四臂最优，但 J_vis 中位=2.75、Δv_vis 中位=64 仍未达 J≤0.5 / Δv≤20。若把 J/Δv 也锁进本票 PASS，则 #37 实测最优臂亦不达标，PASS 将依赖滞后补偿等非 seed 因素，判据不再针对「seed 路径修复」这一命题。
- 分层结论：本票 PASS 只锁速度比（判 seed 是否修好）；J/Δv 与 RMS/位置留作辅证，其达标挂账到轨迹质量类后续票（如 WGC 滞后补偿）。同时保留 zero/unset 对照臂：zero 臂 ratio=0.000、unset 臂 ratio=nan 证明 [0.8,1.2] 窗口能区分「不设 seed」与「正确 seed」，辅证价值不减。

## 块 2 — 调用点清单（禁改 composition.h）

### 2.1 仓库内「spring.h / 闭式 v → compositor InitialVelocity」调用点（符号级）

路径 A：`win_softdemo_src/MainPage.xaml.cs`（真机 live 同源常在 MotionSoftDemo/MainPage.xaml.cs；仓库箱内副本在 win_softdemo_src）

| 方法（约行号） | 臂/路径 | 赋值语句级细节 |
|---|---|---|
| CompStartSpring（约 L519+） | 通用启动 | `a.InitialValue=(float)v0; a.InitialVelocity=(float)vel0`（v0/vel0 来自影子状态，注释写明 spring.h） |
| RunAlignProbe | 打断路径 | 打断时用 shadow 作 InitialValue / InitialVelocity |
| RunInterruptSeedProbe | 打断路径 | `MfProbe.mf_spring_at(... out seedPos, out seedVel)` → `a2.InitialVelocity=(float)seedVel` |
| RunRateFoldProbe | 通用 | `a.InitialVelocity=(float)ivel`（ivel 自 mf_spring_at） |
| RunR2P11Probe | 臂 Bv | `r.InitialVelocity=(float)shV`；臂 A 类 → shX+shV（mf_spring_at at tS） |
| RunR2P10Probe | 臂 A/B | 同模式，shV |
| RunR2P9Probe | 臂 A/B | 同模式，shV |
| RunR2P8Probe | 臂名以 A 开头 | InitialValue=shX, InitialVelocity=shV |
| RunR2P7Probe | 臂 A0 / Am20 | shX+shV |
| RunR2P2Probe | 臂 A | shX+shV |
| **RunR2P4Probe（派题 #37）** | 臂 shadow | `r.InitialVelocity=(float)shV; vSet=shV`；arms={base, shadow, zero, unset, wgc}；证据源 |
| RunR2PostProbe | interrupt 路径 | `a2.InitialVelocity=(float)seedVel` |
| RunSuite 打断段 | shadow seeded retarget | 注释「shadow seeded」 |

路径 B：`MotionSoftDemo/R3Probe.cs`（及 win_softdemo_src/R3Probe.cs 副本）

| 方法 / Trial | 臂/路径 | 赋值语句级细节 |
|---|---|---|
| RunR3P12Probe / Trial | 臂 Bv / Aex | `a.InitialVelocity=(float)shV`（mf_spring_at at tSeed）；Aex 另设 InitialValue=shX |
| RunR3E3Probe | 臂 Bv | InitialValue unset + InitialVelocity=shV |
| RunR350Probe | 臂 Bv | 同上；另有 d1 overlay `InitialVelocity=(float)ivel`（ivel 自 mf_spring_at，ε̂ 扫参） |

备注：

- 若干 `mf_spring_at` 标注 `// log only; do NOT seed`——这些不是 seed 调用点，清单中标记「仅日志」，不属于修复面。
- 桥接：`MfProbe.mf_spring_at` ← mf_probe.cpp / fm_capi 导出，内部调 spring.h 闭式。改 seed 策略时改的是**调用方赋值**，不是 spring.h 本身。

### 2.2 控制器层（本票可触及「seed 路径」语义，须谨慎）

- `code/controller.h`：`Controller::Retarget` 从当前 owner 的 value/velocity 交接（软件路径 spring 状态）。这是 AppThread/软件控制器语义，**不是**直接写 WinRT `SpringScalarNaturalMotionAnimation.InitialVelocity`。若本票要把「禁止影子 IV」固化到控制器 API 契约，可动文档 + 控制器契约；但**不得**借机改 composition.h。

### 2.3 禁止本票改动（只登记）

- `code/composition.h`：`PlanOverride`（约 L304–313）`sp.At(elapsed)` → `pp.spring.fromVelocity = st.velocity`；`PlanGraph` 弹簧段（约 L357–368）同类。这是 lowering 到 CompositorSpring 的产品路径，本票禁止改；对齐 compositor 数学到 spring.h 或改该 seed 出票外。
- `code/spring.h`：纯数学，不是「喂给 compositor IV」的调用点；勿改，除非另票。
- 冻结/禁令：不重开 S1/R-3；不解冻 Bv；成功≠采用。

### 2.4 本票可能动 vs 禁止动

| 层 | 文件 | 本票 |
|---|---|---|
| demo / harness（可能动） | MotionSoftDemo/R3Probe.cs、win_softdemo_src/R3Probe.cs、win_softdemo_src/MainPage.xaml.cs（及真机 MotionSoftDemo/MainPage.xaml.cs）、相关 harness 脚本/探针接线 | 允许改（仅 seed 赋值路径） |
| 控制器（有条件） | code/controller.h | 仅限 seed 契约/文档级，且不触及 composition.h |
| 文档（可能动） | 本票相关文档 | 允许 |
| 禁止动 | code/composition.h（sha 锁死）、LOCKED 闸门/结果文件本体、DRAFT 闸门本体（本交付不改 DRAFT 文件）、spring.h 数学对齐类改动、采数 | 禁止 |

### 2.5 sha 保持与挂账重申

- composition.h sha256 必须保持给定值：`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（本机已核，PRE=POST）。
- composition.h 内 `PlanOverride` / `PlanGraph` 用 spring.h::At 填 CompositorSpring.fromVelocity 的路径**本票禁止改**，只登记为挂账 / 出票外。

## 第三段（可选）— M2-02 API 草案增量建议

给 `docs/deepseek_m2_02_api_draft.md` 的增量一句（可直接粘贴）：

> H5 未修前 C# 投影勿接影子 IV。

放置位置建议：**§3.4**。若 §3.4 是「合成器路径接续假设/未验证项」所在节，追加至该节末尾；若该节专述接续假设而无风险清单，则放 §3.5。附注理由（供落盘参考，不必进正文）：H5 PRIMARY（spring.h 闭式 v 作 compositor InitialVelocity → 过加速）修复前，C# 投影若接入影子 IV，会把同一病根带进投影层。