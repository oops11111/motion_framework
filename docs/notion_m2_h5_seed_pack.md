# M2-H5-SEED 结题中文包（notion_m2_h5_seed_pack.md）

> DECISION_AT=2026-10-25 00:35:03 AWST（Australia/Perth）  
> 权威结题：`docs/m2_h5_seed_result.md`  
> 闸门 LOCKED 勿改：`docs/m2_h5_seed_gate_LOCKED.md` GATE_BODY_SHA256=`28a60c37c08fff4d28136160797182a530d2a15c8d4559f04a4fc23d7a3ea4b0` @ 2026-10-25 00:17:37 AWST  
> APPROVED：pidan@2026-10-25 00:18:34 AWST

## 结论

- **票级 = CLOSED PASS**（G2 PASS；强口径）。
- G4：`M2-H5-SEED | gate=28a60c37c08fff4d28136160797182a530d2a15c8d4559f04a4fc23d7a3ea4b0 @ 2026-10-25 00:17:37 AWST | approved=pidan@2026-10-25 00:18:34 AWST | ratio=P | ticket=PASS | 成功≠采用；不改composition.h；分级A→B；强口径无单样本≥2.0×`
- **成功 ≠ 采用**；不解冻 / 不升级 B_v；不重开 S1 / R-3 / R-2；`composition.h` PRE=POST `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` **未改**；闸门 body/hash **未改**。
- 实现：分级 A→B；A=`InitialVelocity = calib * v_wgc`（打断前末两帧 WGC，QPC，DIP/s）；B 回退 IV=null+0；**calib=1.0**（同窗 seed=v_screen ⇒ r≡1）；禁止 `mf_spring_at` / spring.h 闭式 v 作 compositor IV。

## 证据

| sess | pid | armA n | med_r | max_r | ≥2.0× | armB med_r（对照） | 判定 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 62560 | 12 | 1.0000 | 1.0000 | 否 | 0.0000 | PASS |
| 2 | 56092 | 12 | 1.0000 | 1.0000 | 否 | 0.0000 | PASS |
| 3 | 53728 | 12 | 1.0000 | 1.0000 | 否 | 0.0000 | PASS |

- 3/3 独立进程；每会话有效 n=12；med_r∈[0.8,1.2]；无单样本 r_i≥2.0。
- 相对 #37 shadow 臂 **2.600 已消除**（A 臂 seed=WGC×calib，非 spring.h 闭式 v）。
- PASS **只锁速度比**；J/Δv/RMS 未设本票门槛（辅证另挂）。
- 产物：`artifacts/m2_h5_seed_s{1,2,3}_{raw,summary}.txt`、`m2_h5_seed_run.log`、`m2_h5_seed_composition_hash.txt`、`m2_h5_seed_calib_note.txt`、`m2_h5_seed_armB_control.txt`；runner `artifacts/m2_h5_seed_run_sessions.ps1`。

## 禁令复述

- 不重开 S1 / R-3 / R-2。
- 不解冻 / 不升级 B_v。
- 成功 ≠ 采用。
- 不改 `composition.h`（`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）。
- 不对齐 compositor 弹簧数学到 spring.h。
- 旁路挂账不挡主线；本票也不清旁路。
- 不改已 LOCKED 闸门 body / hash。

## 挂账 / 另票建议

1. **立刻开 M2-02**（见下一票决断）——plan.md Alpha：C# / ETW / 预览宿主 v0；真机须另 LOCK+APPROVED。
2. R-2→M2 **余项挂账（不挡 M2-02）**：Δv 绝对门；L 偏高；多刷新率矩阵；平滑速度估计（可选）。H5 已清 seed 比病根，轨迹质量门另票。
3. DeepSeek `docs/deepseek_m2_02_api_draft.md` §3.4「H5 未修前勿接影子 IV」→ 结题后改为：**须走 WGC/屏一致 seed 或 IV=null+0，禁止影子 IV**（H5 PASS 已验证主路径）。

— notion ai（M2-H5-SEED 结题；ticket=CLOSED PASS；下一票=M2-02）
