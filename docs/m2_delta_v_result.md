# M2-Δv 结题

STATUS=CLOSED
TICKET=Δv-INCONCLUSIVE
WALL=2026-10-26 08:56 AWST
MACHINE=DESKTOP-K93N3P8 (1 physical; 60Hz)

## Gate / Auth
- gate=`docs/m2_delta_v_gate_LOCKED.md`
- GATE_BODY_SHA256=d2349ceb0d37fdf332a4f00cb2c1f1217fe6856d0f3e5222ffaf82b8b15509c6
- LOCKED_AT=2026-10-25 23:33:05 AWST
- APPROVED_BY=pidan APPROVED_AT=2026-10-26 06:21:36 AWST (`docs/m2_delta_v_approved.md`)
- prereg_hash=267c6b781712fcde6b368bdb7dd716a6f8e359bfc22ccdca93d7c248e4bfc025

## Verdict
- **ticket=Δv-INCONCLUSIVE**
- 3/3 独立进程会话均为 SESSION_INCONCLUSIVE_CANDIDATE
- 可判核心 5 格在每会话均因可判规则失败 → **Δv-SKIP**（SKIP≠PASS；不挡票也不构成 PASS）
  - t060{fwd,rev}: 常见 `|a|·σ_ts > thr/2`（σ_ts=1/60/√12）
  - t300{fwd,rev,dbl}: 常见 `thrMed < 2·Nv_h_cell`（floor thr=20；Nv_h_P95 多在 18–37）
- 正对照 base：**未**达 base PASS（部分格 base_pass<17/20 或同可判失败）→ `ctrl basePass=False`
- 负对照 IVel=0：三会话 t060 fwd **与** rev 均 FAIL（IVel0_pass≪17）→ `IVel0_t060_both_FAIL=True`（对照成立）
- t156{fwd,rev}: 规则导出 Δv-SKIP（辅证；不挡票）
- Arm-WGC-seed 在可采样本上 Δv 常 ≫ thr（wgc_pass 多为 0–4/20），但因格不可判，不记 Δv-FAIL 票结局
- 每会话探针在 IVel0·t300dbl 中途崩溃；核心格（含 t300dbl 的 base+WGC）数据齐；IVel0·t300dbl 非票内负对照要求；offline adjudicate 留证

## Sessions
见 `artifacts/m2_delta_v_session_matrix.txt` 与 `artifacts/m2_delta_v_s{1,2,3}_{raw,summary}.txt`。

| sess | verdict | note |
|---|---|---|
| 1 | INCONCLUSIVE_CANDIDATE | offline; crash IVel0 t300dbl |
| 2 | INCONCLUSIVE_CANDIDATE | offline; crash IVel0 t300dbl |
| 3 | INCONCLUSIVE_CANDIDATE | offline; crash IVel0 t300dbl |

## composition.h
- PRE=POST=a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418
- 未改

## 禁令重申
- 禁影子 IV（shadow_v_log 仅日志；InitialVelocity 来自 WGC 末两帧或 0）
- 不重开 S1/R-3/R-2；不解冻 B_v
- 成功≠采用；本票 ≠ PERF-CI（产物不作 PERF-CI 合格证据）
- 未改闸门 body/hash；未跨会话池化

## Artifacts
- `artifacts/m2_delta_v_prereg.txt` + `m2_delta_v_prereg_hash.txt`
- `artifacts/m2_delta_v_run_sessions.ps1` + `m2_delta_v_offline_adj.ps1` + `m2_delta_v_run.log`
- `artifacts/m2_delta_v_s{1,2,3}_{raw,summary}.txt`
- `artifacts/m2_delta_v_session_matrix.txt` + `m2_delta_v_composition_hash.txt`
- 源码 `MotionSoftDemo/M2DeltaVProbe.cs`（RunM2DeltaVProbe；二次 LS w=5）

## G4 ONELINE
M2-Δv | gate=d2349ceb0d37fdf332a4f00cb2c1f1217fe6856d0f3e5222ffaf82b8b15509c6 @ 2026-10-25 23:33:05 AWST | approved=pidan@2026-10-26 06:21:36 AWST | sessions=3/3 INCONCLUSIVE | cells=0/5 judicable(all rule-SKIP) | ctrl=baseF/IVel0Fok | ticket=INCONCLUSIVE | t156=rule-SKIP | 成功≠采用；不改composition.h；禁影子IV；≠PERF-CI
