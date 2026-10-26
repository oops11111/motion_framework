# M2-Δv′ 结题

STATUS=CLOSED
TICKET=Δv-FAIL
WALL=2026-10-26 10:45 AWST
MACHINE=DESKTOP-K93N3P8 (1 physical; 60Hz)

## Gate / Auth
- gate=`docs/m2_delta_v_prime_gate_LOCKED.md`
- GATE_BODY_SHA256=06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c
- LOCKED_AT=2026-10-26 09:05:37 AWST
- APPROVED_BY=pidan APPROVED_AT=2026-10-26 09:09:19 AWST
- prereg_hash=7bfaa379ba238c1b409aaec2ace07a6749cce55a6edc57494687c1681b5a0496
- 前序 M2-Δv gate `d2349ceb…09c6` **未重跑**

## Verdict
- **ticket=Δv-FAIL**（G2-LOCK-9：可判核心格臂差 → FAIL；禁标 INCONCLUSIVE）
- 3/3 独立进程均为 `SESSION_FAIL_CANDIDATE`
- P1/P2 升版生效：t060{fwd,rev} 三会话均 **judicable=True**，但 Arm-WGC-seed `wgc_pass` 为 0–2/20 → 格·会话 **FAIL**
- t300 多数因 `Nv_h > thr` 仍 rule-SKIP（s2 t300rev 一度可判且 FAIL 5/20）
- 正对照 base：三会话 `basePass=True`
- 负对照 IVel=0：三会话 t060 fwd+rev 均 FAIL（`IVel0_t060_both_FAIL=True`）；IVel0·t300* 按 P5 跳过（`CELL_ARM_SKIP … P5_IVel0_only_t060`）
- t156：规则 SKIP ACCEPT

## Sessions
| sess | verdict | t060fwd | t060rev | notes |
|---|---|---|---|---|
| 1 | FAIL_CANDIDATE | FAIL 2/20 | FAIL 0/20 | t300 all SKIP |
| 2 | FAIL_CANDIDATE | FAIL 0/20 | FAIL 0/20 | t300rev also FAIL 5/20 |
| 3 | FAIL_CANDIDATE | FAIL 1/20 | FAIL 0/20 | t300 all SKIP |

详见 `artifacts/m2_delta_v_prime_session_matrix.txt` 与 `s{1,2,3}_{raw,summary}.txt`。

## composition.h
- PRE=POST=a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418
- 未改

## 禁令重申
- 禁影子 IV（shadow_v_log 仅日志）
- 不重开 S1/R-3/R-2；不解冻 B_v
- 成功≠采用；≠PERF-CI；未改闸门；未池化；FAIL 不翻盘

## Artifacts
- `artifacts/m2_delta_v_prime_prereg.txt` + `_prereg_hash.txt`
- `artifacts/m2_delta_v_prime_run_sessions.ps1` + `_offline_adj.ps1` + `_run.log`
- `artifacts/m2_delta_v_prime_s{1,2,3}_{raw,summary}.txt`
- `artifacts/m2_delta_v_prime_session_matrix.txt` + `_composition_hash.txt`
- 源码 `MotionSoftDemo/M2DeltaVPrimeProbe.cs`

## G4 ONELINE
M2-Δv′ | gate=06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c @ 2026-10-26 09:05:37 AWST | approved=pidan@2026-10-26 09:09:19 AWST | sessions=3/3 FAIL | cells=t060 judicable FAIL (t300 mostly SKIP) | ctrl=baseP/IVel0F | ticket=FAIL | t156=rule-SKIP | P1=|a|σ≤thr;P2=thr≥Nv_h | 成功≠采用；不改composition.h；禁影子IV；≠PERF-CI；前序d2349ceb不改
