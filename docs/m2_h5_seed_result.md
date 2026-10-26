# M2-H5-SEED 结题（m2_h5_seed_result.md）

- WALL: 2026-10-25 00:32:13 AWST (Australia/Perth, UTC+8)
- GATE: `docs/m2_h5_seed_gate_LOCKED.md` GATE_BODY_SHA256=`28a60c37c08fff4d28136160797182a530d2a15c8d4559f04a4fc23d7a3ea4b0` @ LOCKED_AT=2026-10-25 00:17:37 AWST（**未改** body/hash）
- APPROVED: pidan@2026-10-25 00:18:34 AWST（`docs/m2_h5_seed_approved.md`）
- composition.h PRE=POST=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（未改）
- 禁令重申：不重开 S1/R-3/R-2；不解冻/升级 Bv；成功≠采用；不对齐 compositor 弹簧数学到 spring.h；无合成输入；FAIL 后未补测。

---

## 实现摘要

- 探针：`MotionSoftDemo/MainPage.xaml.cs` → `RunM2H5SeedProbe`；flag `--m2-h5-seed` / env `MF_M2_H5_SEED=1` / `artifacts/m2_h5_seed.flag`
- 同步副本：`artifacts/win_softdemo_src/MainPage.xaml.cs`
- 分级 A→B：
  - **A**：`InitialVelocity = (float)(calib * v_wgc)`；`v_wgc` = 打断前末两帧 WGC Translation.X，`(x2-x1)/(t2-t1)`，QPC→秒，DIP/s
  - **B 回退**：`|v_wgc|<ε` 或 NaN 时该次 `InitialVelocity=0`（IV unset + IVel=0），path=B，不计 A 的 n
  - **B 对照臂**：显式 12 次 IV=null+0，仅登记，不计入 3/3
- **禁止**：`mf_spring_at` / spring.h 闭式 v 作 compositor IV（仅 log-only 记录 shV）
- calib：**1.0**（写死常量）。同测量窗令 seed=v_screen ⇒ r≡1；#37 wgc 0.796 乃相对离线另算 v_pre 的系统偏低，本闸门 v_screen 定义下不适用 1/0.796。详见 `artifacts/m2_h5_seed_calib_note.txt`
- 口径：窗口 [0.8,1.2]；ε=5.0 DIP/s；n=12；中位；强口径无单样本 ≥2.0×

---

## G2 判定 — PASS

| sess | pid | armA n_valid | med_r | max_r | any_r≥2.0 | armB med_r (对照) | verdict |
|------|-----|--------------|-------|-------|-----------|-------------------|---------|
| 1 | 62560 | 12 | 1.0000 | 1.0000 | False | 0.0000 | SESSION_PASS_CANDIDATE |
| 2 | 56092 | 12 | 1.0000 | 1.0000 | False | 0.0000 | SESSION_PASS_CANDIDATE |
| 3 | 53728 | 12 | 1.0000 | 1.0000 | False | 0.0000 | SESSION_PASS_CANDIDATE |

- 3/3 独立进程；每会话有效 n=12；med_r∈[0.8,1.2]；无单样本 r_i≥2.0
- 相对 #37 shadow 臂 2.600：**已消除**（A 臂 seed 来自 WGC*calib，非 spring.h 闭式 v；log 中 shV≈475–646 仅辅证）
- 证据：`artifacts/m2_h5_seed_s{1,2,3}_{raw,summary}.txt` + `artifacts/m2_h5_seed_run.log` + `artifacts/m2_h5_seed_composition_hash.txt`
- runner：`artifacts/m2_h5_seed_run_sessions.ps1`

**G2 = PASS**（强口径）

---

## G3 禁令重申

不重开 S1/R-3；不改 composition.h；成功≠采用；不解冻/升级 Bv；不对齐 compositor 弹簧数学到 spring.h；旁路挂账不挡、本票也不清旁路。

---

## G4 ONELINE

M2-H5-SEED | gate=28a60c37c08fff4d28136160797182a530d2a15c8d4559f04a4fc23d7a3ea4b0 @ 2026-10-25 00:17:37 AWST | approved=pidan@2026-10-25 00:18:34 AWST | ratio=P | ticket=PASS | 成功≠采用；不改composition.h；分级A→B；强口径无单样本≥2.0×