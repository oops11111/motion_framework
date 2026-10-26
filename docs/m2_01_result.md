# M2-01 结题（m2_01_result.md）

- WALL: 2026-10-24 23:59:33 AWST (Australia/Perth, UTC+8)
- GATE: `docs/m2_01_gate_LOCKED.md` GATE_BODY_SHA256=`182f945c77b7103b8883be523742af8e6dcd8ca2d4232529a5fac347a168beb1` @ LOCKED_AT=2026-10-24 23:41:05 AWST
- APPROVED: pidan@2026-10-24 23:42:17 AWST（`docs/m2_01_approved.md`；**未改** LOCKED body/hash）
- Notion 决断=A 继续跑；本文件仅写执行澄清与判定，不改闸门。
- composition.h PRE=POST=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（未改）
- 禁令重申：不重开 S1/R-3；不解冻 Bv/R-2；不升级 Bv；成功≠采用；M2-02 deferred；无合成输入；无 gate-extra 补测。

---

## 执行澄清（写入结题，不改闸门）

### C1. G2 口径 vs #30
- **G2 只用本轮静止/零激励新采**（`artifacts/m2_01_noise_s{1,2,3}_raw.txt`，wall 2026-10-24 23:58–23:59 AWST）。
- **#30 运动探针**（`artifacts/win_harness_noise_probe.txt` 线性 KF RMS/max|err|）**不参与** G2 PASS/FAIL，不混用、不入中位。
- 依据：闸门 G2「静止/零激励」+ `artifacts/m2_01_g2_method_LOCKED.txt` + DeepSeek `docs/deepseek_m2_01_noise.md` §2.6/§3（运动口径 ≠ 静止底噪）。

### C2. 指标 / 静止定义 / 单位·几何 / n / 会话（闸门写死 + 缺口澄清）
| 项 | 闸门/方法锁写死 | 本轮执行澄清（对照 deepseek） |
|---|---|---|
| 静止定义 | CompReset(rest) + **不** StartAnimation | rest=120 DIP；零激励；WGC 捕获 Translation.X 主副本（排除影子路径，deepseek §2.7） |
| 指标 | 位置 RMS（DIP） | `RMS = sqrt(mean((x_i - mean(x))^2))`；会话得分=该会话单一 RMS（不对窗再取中位）；与 method_LOCKED 一致。闸门「会话中位」在单指标/会话下即该 RMS。 |
| 单位·几何 | DIP；标定写死 | pxPerDip 由 cx0/cx37 cal（本轮三会话均为 1.0000≈100%）；x 经 rest-reanchor 使静止期望≈120 |
| n | ≥60 有效帧 | 剔除启动后前 5 帧暖机；本轮 n=127/128/127 |
| 会话 | ≥3 独立进程 | 3/3；各写独立 raw；runner=`artifacts/m2_01_run_noise_sessions.ps1` |
| PASS | 3/3 < 0.5 DIP | 本轮 RMS ≈ 0（浮点尘埃 ≤4e-14；range=0）→ 3/3 PASS |

### C3. 仍禁
- 不改 `composition.h`；不重开 S1/R-3；不解冻 Bv。

---

## G2 Harness 底噪 — PASS

| sess | n | mean(DIP) | RMS(DIP) | max\|dev\| | range | pxPerDip | verdict |
|------|---|-----------|----------|-----------|-------|----------|---------|
| 1 | 127 | 120.0 | ~0 (4.26e-14) | ~0 | 0 | 1.0 | SESSION_PASS_CANDIDATE |
| 2 | 128 | 120.0 | ~0 (1.42e-14) | ~0 | 0 | 1.0 | SESSION_PASS_CANDIDATE |
| 3 | 127 | 120.0 | 0 | 0 | 0 | 1.0 | SESSION_PASS_CANDIDATE |

- 证据：`artifacts/m2_01_noise_s1_raw.txt` / `s2_raw.txt` / `s3_raw.txt` + 对应 `*_summary.txt` + `artifacts/m2_01_noise_run.log`
- 方法锁：`artifacts/m2_01_g2_method_LOCKED.txt`
- 解读（对照 deepseek）：静止零激励下 WGC 质心在本机 100% DPI 上帧间恒定 → 纯估计底噪下界≈0 DIP，远低于 0.5；与 #30 运动 RMS~1.3–1.6（含速度×时间误差）量级分离，印证口径选择正确。
- **G2 = PASS**（3/3 RMS < 0.5 DIP；n≥60；hash 相符；非 #30）

---

## G3 影子 2.6× 根因 — PASS

- 顺序：H1→H5（Gemini）；五条全保留（Notion）。
- 审计：`artifacts/m2_01_g3_h1h5_audit.txt`
- #37 复算：shadow arm med_vSet=620.0 / med_v_pre=238.5 → **ratio=2.600**；证据 `artifacts/win_r2_p4_b2vel.txt`
- 判定：
  - H1 属性路径 — NOT primary（Translation.X 正确）
  - H2 时基/单位 — NOT primary（非 1000×；观测≈2.6×）
  - H3 坐标系 — NOT primary（同 DIP）
  - H4 DIP/缩放 — NOT primary（pxPerDip=1.0 仍见 2.6×）
  - **H5 Seed 源 — PRIMARY**：用 spring.h 闭式 v(tInt) 作 compositor InitialVelocity，与屏上 NaturalMotion 轨迹存在已知 shape residual → 过加速（最差臂）
- 单一主因：seed 源脱节（H5）
- 证据路径：`artifacts/win_r2_p4_b2vel.txt`（+ raw / soft_demo #37）
- **修复是否触及 composition.h？NO**（方向：WGC/屏一致速度 seed，或 IV=null+0 降级；另票。对齐 compositor spring 数学到 spring.h 才可能触 composition.h，出本票范围）
- **G3 = PASS**

---

## G4 票级

- 票级 PASS ⇔ G2 PASS ∧ G3 PASS → **PASS**
- 成功 ≠ 采用；不解冻 Bv；不重开 R-2/S1/R-3；不改 composition.h

---

## Harness / 产物清单

- `artifacts/m2_01_g2_method_LOCKED.txt`
- `artifacts/m2_01_g3_h1h5_audit.txt`
- `artifacts/m2_01_run_noise_sessions.ps1`
- `artifacts/m2_01_noise_run.log`
- `artifacts/m2_01_noise_s1_raw.txt` / `s1_summary.txt`
- `artifacts/m2_01_noise_s2_raw.txt` / `s2_summary.txt`
- `artifacts/m2_01_noise_s3_raw.txt` / `s3_summary.txt`
- `artifacts/m2_01_noise_capture_raw.txt` / `capture_summary.txt`（末会话固定捕获副本）
- `artifacts/m2_01_build.txt` / `m2_01_build2.txt`
- `artifacts/m2_01_harness_ref_snip.txt`
- 探针改动（仅 demo，非 composition.h）：`MotionSoftDemo/R3Probe.cs`（RunM201NoiseProbe）、`MainPage.xaml.cs`（flag/env/--m2-01-noise-probe 接线）
- 文档：`docs/deepseek_m2_01_noise.md`（分析澄清参照）、本文件

---

## G6 ONELINE

M2-01 | gate=182f945c77b7103b8883be523742af8e6dcd8ca2d4232529a5fac347a168beb1 @ 2026-10-24 23:41:05 AWST | approved=pidan@2026-10-24 23:42:17 AWST | noise=P | shadow2p6=P | ticket=PASS | 成功≠采用；不解冻Bv；不改composition.h