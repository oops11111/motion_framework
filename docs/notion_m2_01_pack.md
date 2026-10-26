# M2-01 结题中文包（notion_m2_01_pack.md）

> DECISION_AT=2026-10-25 00:04:04 AWST（Australia/Perth）  
> 权威结题：`docs/m2_01_result.md`；闸门 LOCKED 勿改（hash `182f945c77b7103b8883be523742af8e6dcd8ca2d4232529a5fac347a168beb1`）  
> 执行澄清：`docs/m2_01_exec_clarification.md`（紧急决断 A）

## 结论

- **票级 = PASS**（G2 PASS ∧ G3 PASS）。
- G6：`M2-01 | gate=182f945c77b7103b8883be523742af8e6dcd8ca2d4232529a5fac347a168beb1 @ 2026-10-24 23:41:05 AWST | approved=pidan@2026-10-24 23:42:17 AWST | noise=P | shadow2p6=P | ticket=PASS | 成功≠采用；不解冻Bv；不改composition.h`
- **成功 ≠ 采用**；不解冻 B_v / 不重开 R-2；不重开 S1 / R-3；`composition.h` PRE=POST `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` **未改**。
- #30 运动探针**不参与** G2（与紧急 A / 执行澄清一致）；G2 只用本轮静止/零激励新采。

## 证据

### G2 底噪（PASS）
| sess | n | RMS(DIP) | 判定 |
| --- | --- | --- | --- |
| 1 | 127 | ~0（≤4e-14） | PASS |
| 2 | 128 | ~0 | PASS |
| 3 | 127 | 0 | PASS |

- 阈值：3/3 会话 RMS < 0.5 DIP；静止/零激励；WGC Translation.X 主副本；排除影子路径。
- 产物：`artifacts/m2_01_noise_s{1,2,3}_raw.txt` + summary；`artifacts/m2_01_g2_method_LOCKED.txt`；`artifacts/m2_01_noise_run.log`。
- 解读：静止下估计底噪≈0，与 #30 运动 RMS~1.3–1.6 量级分离，印证「运动≠静止」口径。

### G3 影子 2.6×（PASS）
- 排查序：H1→H5（Gemini；Notion 沿用、不否决）。
- **主因 = H5 Seed 源**：spring.h 闭式 v 作 compositor InitialVelocity，与屏上 NaturalMotion 轨迹 shape residual → 过加速；#37 ratio = 620.0/238.5 = **2.600**。
- H1–H4：均 NOT primary（见 `artifacts/m2_01_g3_h1h5_audit.txt`）。
- 证据：`artifacts/win_r2_p4_b2vel.txt` + `artifacts/m2_01_g3_h1h5_audit.txt`。
- **修复触及 composition.h？NO**。方向（另票）：WGC/屏一致速度 seed，或 IV=null+0 降级。

## 禁令复述

- 不重开 S1 / R-3。
- 不解冻 / 不升级 B_v；不重开 R-2。
- 成功 ≠ 采用。
- 不改 `composition.h`（`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）。
- 旁路挂账不挡主线；本票也不清旁路。
- 不改已 LOCKED 闸门 body / hash。

## 挂账 / 另票建议

1. **立刻开 H5 修复票**（见下文下一票决断 A）——不碰 `composition.h`。
2. M2-02（C#/ETW/预览宿主）文档继续预起草（已有 `docs/deepseek_m2_02_api_draft.md`），**真机须另 LOCK+APPROVED**；本轮不并行派测试。
3. 旁路停车场条目保持（T-A1a / R-3 / B_v / C5(b) 等）——不挡。

— notion ai（M2-01 结题；ticket=PASS；下一票=H5 修复）
