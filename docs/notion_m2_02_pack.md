# M2-02 结题中文包（notion_m2_02_pack.md）

> DECISION_AT=2026-10-25 01:31:27 AWST（Australia/Perth）  
> 权威结题：`docs/m2_02_result.md`  
> 闸门 LOCKED 勿改：`docs/m2_02_gate_LOCKED.md` GATE_BODY_SHA256=`ce147d212d31910b84c6c8e5566fbf50e740efcaadb27747cde827db2ec0a180` @ 2026-10-25 00:42:43 AWST  
> APPROVED：pidan@2026-10-25 01:11:43 AWST  
> 真机：DESKTOP-K93N3P8；Binary=MotionSoftDemo x64 Debug

## 结论

- **票级 = CLOSED PASS**（csharp=P ∧ etw=P ∧ preview=P）。
- G4：`M2-02 | gate=ce147d212d31910b84c6c8e5566fbf50e740efcaadb27747cde827db2ec0a180 @ 2026-10-25 00:42:43 AWST | approved=pidan@2026-10-25 01:11:43 AWST | csharp=P | etw=P | preview=P | ticket=PASS | 成功≠采用；不改composition.h；禁影子IV；perfCI另票`
- **成功 ≠ 采用**；不解冻 / 不升级 B_v；不重开 S1 / R-3 / R-2；`composition.h` PRE=POST `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` **未改**；闸门 body/hash **未改**。
- **性能 CI ≥2 机未申领**（按闸门 G2-LOCK-8 另票）。
- 禁影子 IV：FmBridge Retarget 从当前状态接续，未接 spring.h 影子 IV。

## 证据

| sess | appPID | sessionId | csharp | etw | preview | 判定 |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 10404 | 0x28A40DF285D5 | P | P | P | PASS |
| 2 | 62428 | 0xF3DC0DF2E9DE | P | P | P | PASS |
| 3 | 12912 | 0x32700DF34ACB | P | P | P | PASS |

### csharp=PASS
- L1：`MotionEngine` / `MotionEtw` / `M202Probe`；Create → LoadTokens(`demo_motion.json`) → Play → Retarget ≥5（中途打断 / 同属性改目标 / 跨动画切换各 ≥1）→ Cancel。
- 连续性：jumpFails=0；Play/Cancel/Retarget 均命中。

### etw=PASS
- Provider：`FluidMotion-Motion`；最小集 6 事件每会话 SEEN：AnimationLifecycle / Retarget / FrameTick / PresentStats / IdleTransition / TokenLoad。
- 证据：`artifacts/m2_02_s*_etw_*.txt`；`artifacts/m2_02_eventpipe.nettrace`（logman ETL Access denied → EventPipe + in-proc ledger）。

### preview=PASS
- 宿主内路径：`demo_motion.json` → 选 token → Load → Play；PREVIEW_VERDICT=PASS，无视觉崩溃。

产物：`artifacts/m2_02_*`（含 `m2_02_session_matrix.txt`、`m2_02_run_sessions.ps1`、`m2_02_run.log`、`m2_02_composition_hash.txt`、`m2_02_etw_provider.md`）。

## 禁令复述

- 不重开 S1 / R-3 / R-2。
- 不解冻 / 不升级 B_v。
- 成功 ≠ 采用。
- 不改 `composition.h`（`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）。
- 禁止影子 IV（须 WGC/屏一致 seed 或 IV=null+0）。
- 性能 CI ≥2 机另票（本票未申）。
- 旁路挂账不挡主线；本票也不清旁路。
- 不改已 LOCKED 闸门 body / hash。

## 挂账 / 另票建议

1. **立刻开 M2-PERF-CI**（见下一票决断）——收口 plan.md M2 Alpha 最后一项「性能 CI ≥2 物理机」。
2. R-2→M2 余项继续挂账（不挡）：Δv 绝对门；L 偏高；多刷新率矩阵；平滑速度估计（可选）。
3. M3（Rive + 首个试点）等 M2 Alpha 收口后再起步。

— notion ai（M2-02 结题；ticket=CLOSED PASS；下一票=M2-PERF-CI）
