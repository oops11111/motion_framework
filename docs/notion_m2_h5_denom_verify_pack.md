# M2-H5-DENOM-VERIFY 结题中文包（notion_m2_h5_denom_verify_pack.md）

> DECISION_AT=2026-10-26 17:27:00 Asia/Taipei  
> 权威结题：`docs/m2_h5_denom_verify_result.md`（WRITTEN_AT=2026-10-26 17:24:08 Australia/Perth）  
> 闸门 LOCKED **勿改 hash**：`docs/m2_h5_denom_verify_gate_LOCKED.md` GATE_BODY_SHA256=`fd00a06058b239abfe03a52175463a52741718318ce99416a4ce762cf037889b` @ 2026-10-26 16:09:00 Asia/Taipei（结题复算 **MATCH**）  
> APPROVED（来源：`docs/m2_h5_denom_verify_approved.md` + result 正文；协调亦声称同字段——**结题包据文件/result 记入；不把代理声称当作亲自核实的用户批准事实去超出结题范围采数/改码**）：pidan@2026-10-26 16:12:48 Australia/Perth  
> 显式字段（approved / result）：验证采数=**是**；落地=**否**；calib=**C1**；路径=**PATHB-A**；KEEP v_denom≡v_pre；禁抬 thr；七 hash 不翻盘；禁立刻 Δv″  
> 前序 PATH-A′ FAIL `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362`：**未重跑、未翻盘**  
> 前序 PATH-B′ FAIL `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`：**未重跑、未翻盘**  
> 前序 SEED-CALIB FAIL `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`：**未重跑、未翻盘**  
> 前序 SEED-ABS FAIL `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`：**未重跑、未翻盘**  
> 前序 Δv′ FAIL `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`：**未重跑、未翻盘**  
> 前序 TRUTH gate `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69`：**未重跑、未翻盘**  
> 前序 DENOM gate `d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7`：**未重跑、未翻盘**  
> **本 VERIFY gate `fd00a060…889b`：结题后不改 body/hash；不重跑翻盘**  
> 旁路：`docs/side_parked_hang_degrade.md`（不挡、本票也不清）

## 结论

- **票级 = CLOSED**；标签 **`H5-DENOM-VERIFY-FAIL`**；mode=**verify**（验证采数=是；落地=否）。
- G4 ONELINE（原文照抄 result）：  
  `M2-H5-DENOM-VERIFY | gate=fd00a06058b239abfe03a52175463a52741718318ce99416a4ce762cf037889b @ 2026-10-26 16:09:00 Asia/Taipei | approved=pidan@2026-10-26 16:12:48 Australia/Perth | mode=verify | ticket=H5-DENOM-VERIFY-FAIL | KEEP v_denom≡v_pre；path=PATHB-A；calib=C1 exact=1.3358225694150683；禁静默0.7511；验证采数=是；落地=否；七hash不翻盘；禁抬thr；成功≠落地采用≠Δv-PASS；≠PERF-CI；禁立刻Δv″；单机 DESKTOP-K93N3P8`
- **声明**：成功 ≠ 落地采用 ≠ Δv-PASS；**本票未落地**；**未解冻 Δv″**；≠PERF-CI；禁立刻 Δv″。
- **FAIL 缺口**：H5 **比三项齐**（med_r∈[0.8,1.2] 且无 r_i≥2.0）但 **smoke ≥17/20 三项皆未达** → `pass_sessions=0/3` → ticket=`H5-DENOM-VERIFY-FAIL`。
- 路径：**PATHB-A**（V2-clamp · N=5 · order=2 · **P-VS=OFF** · **A′=OFF**；执行中未偷开）。
- calib：**C1** exact=`1.3358225694150683`（n=120 from 3 cold calib；**≠0.7510829456 / ≠0.7511**）。
- 对照 **KEEP**：三项皆 basePass ∧ IVel0_t060_both_FAIL。
- `composition.h` PRE=POST=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` **未改**；本闸门 body/hash **未改**。
- **本票 ≠ PERF-CI**；PERF-CI 仍 HOLD `5b007dd4…282a`；不开 M3；禁影子 IV；不解冻 Bv；不重开 S1/R-3/R-2。
- **七 + VERIFY hash 不翻盘**（见下全文）；禁把 FAIL 当 PASS。

## 授权核验

| 项 | 值 |
|---|---|
| approved 文件 | 存在：`docs/m2_h5_denom_verify_approved.md`（来源文件；协调亦声称同值） |
| APPROVED_BY / AT | pidan / 2026-10-26 16:12:48 Australia/Perth（原文） |
| 验证采数 | **是**（已执行；≥3 独立进程；单机 DESKTOP-K93N3P8） |
| 落地 | **否**（本票未落地） |
| calib | **C1 = median(v_denom/v_hat)** → 冻结 exact=`1.3358225694150683` |
| 路径 | **PATHB-A**；P-VS=OFF；A′=OFF |
| KEEP | v_denom≡v_pre；禁抬 thr/降 17/20；七 hash 不翻盘；禁立刻 Δv″ |
| gate hash | `fd00a06058b239abfe03a52175463a52741718318ce99416a4ce762cf037889b` @ 16:09:00 Asia/Taipei；复算 **MATCH**；**勿改** |

## 验证事实摘要

### Phase-Calib → 冻结 C1

| sess | n | session median(v_denom/v_hat) |
|------|---|-------------------------------|
| 1 | 40 | 1.3385438573753232 |
| 2 | 40 | 1.345761262050146 |
| 3 | 40 | 1.2074588251150606 |

- **frozen calib_new** = median of all 120 = **`1.3358225694150683`**
- 禁静默 0.7511；计分窗未再改 calib

### Phase-Score（frozen C1；PATHB-A）

| sess | med_r | ratioOk | smoke fwd | smoke rev | ctrl | verdict |
|------|-------|---------|-----------|-----------|------|---------|
| 1 | 1.1622 | True | 10/20 | 8/20 | basePass∧IVel0_both_FAIL | FAIL_CANDIDATE |
| 2 | 0.9911 | True | 0/20 (SKIP jud=False) | 9/20 | basePass∧IVel0_both_FAIL | FAIL_CANDIDATE |
| 3 | 1.1642 | True | 12/20 | 9/20 | basePass∧IVel0_both_FAIL | FAIL_CANDIDATE |

- **pass_sessions = 0/3**
- **比过、smoke 灭**：相对比可齐，绝对 smoke 系统性 ≪17/20（未抬 thr；未降 17/20）
- 与 SEED-TRUTH-CONFLICT 核心矛盾（比 vs 绝对）在「分母同源 + C1」后**仍未解**

## 事实表

| 项 | 值 |
|---|---|
| TICKET / STATUS | `H5-DENOM-VERIFY-FAIL` / **CLOSED** |
| mode | **verify**（采数=是；落地=否） |
| gate | `fd00a06058b239abfe03a52175463a52741718318ce99416a4ce762cf037889b` @ 2026-10-26 16:09:00 Asia/Taipei |
| approved | pidan@2026-10-26 16:12:48 Australia/Perth（来源：approved 文件 + result） |
| path | **PATHB-A**；P-VS=**OFF**；A′=**OFF** |
| calib C1 exact | **`1.3358225694150683`**（n=120；≠0.7511） |
| pass_sessions | **0/3** |
| H5 比 | 三项齐（med_r≈0.99–1.16） |
| smoke | 三项皆 ≪17/20 |
| 对照 | KEEP basePass∧IVel0_both_FAIL |
| 落地 | **否**（未落地） |
| Δv″ | **未解冻** |
| composition.h | 未触 `a55d4952…e418` |
| PERF-CI | HOLD `5b007dd4…282a` |

## 七 + VERIFY hash 不翻盘（全文）

- PATH-A′ `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362`：**不改、不重跑、不宣称通过**
- PATH-B′ `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`：**不改、不重跑、不宣称通过**
- SEED-CALIB `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`：**不改、不重跑、不宣称通过**
- SEED-ABS `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`：**不改、不重跑、不宣称通过**
- Δv′ `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`：**不改、不重跑、不宣称通过**
- TRUTH `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69`：**不改、不重跑、不宣称通过**
- DENOM `d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7`：**不改、不重跑、不宣称通过**
- 本 VERIFY gate `fd00a06058b239abfe03a52175463a52741718318ce99416a4ce762cf037889b`：**不改 body/hash**（复算 MATCH）；**不重跑翻盘**
- 结题标签只用 `H5-DENOM-VERIFY-*`；**禁止**写 `Δv-PASS` / 前序 `SEED-*-PASS` / `SEED-TRUTH-PASS` / 重写 `H5-DENOM-PASS` 为已落地；**禁把 FAIL 当 PASS**

## 产物清单

- `docs/m2_h5_denom_verify_result.md`（权威结题）
- `docs/notion_m2_h5_denom_verify_pack.md`（本包）
- `docs/notion_m2_h5_denom_verify_next.md`
- `docs/m2_h5_denom_verify_approved.md` / `docs/m2_h5_denom_verify_gate_LOCKED.md`（只读引用）
- 标定/计分产物前缀 only：`artifacts/m2_h5_denom_verify_*` / `m2_h5_denom_verify_calib_*`
- 探针：`MotionSoftDemo/M2H5DenomVerifyProbe.cs`；bak=`MainPage.xaml.cs.bak_h5denom_verify`

## 禁令复述

- 不重开 S1 / R-3 / R-2；不解冻 Bv；不改 composition.h；禁影子 IV
- 成功 ≠ 落地采用 ≠ Δv-PASS；**本票未落地**；**未解冻 Δv″**
- **不改/不重跑**七 hash 全文；不改本 VERIFY gate；禁重跑翻盘
- PERF-CI 不降级；不开 M3；禁立刻 Δv″
- 禁抬 thr / 降 17/20；禁静默 0.7511；禁翻本次 C1 exact
- 禁把 FAIL 当 PASS；禁默认可落地
- KEEP `v_denom≡v_pre`；PATHB-A；P-VS=OFF；A′ 本票未开

## 挂账 / 残差

| 项 | 处置 |
|---|---|
| M2-H5-DENOM-VERIFY（本票） | **CLOSED H5-DENOM-VERIFY-FAIL**；mode=verify；LOCKED 归档不改 |
| 比 vs 绝对 | **未解**（分母同源+C1 后比过、smoke 仍灭）→ 见 next |
| M2-Δv″ | **继续挂账**；禁立刻 |
| M2-PERF-CI | 继续 HOLD |
| M3 | 不开真机 |
| 七 + VERIFY hash 归档 | 保持；不翻盘 |
| 本 VERIFY hash | 归档；下一票亦不翻盘 |

## 下一票决断

详见 `docs/notion_m2_h5_denom_verify_next.md`。  
**主推 C = 挂账旁路**；次选 D = 协调决议票（doc-only）。禁抬 thr；禁立刻 Δv″；禁翻盘七+VERIFY hash；PERF-CI HOLD。

— notion ai（M2-H5-DENOM-VERIFY 结题；ticket=CLOSED H5-DENOM-VERIFY-FAIL；mode=verify；落地=否；未解冻 Δv″；gate MATCH 不改）
