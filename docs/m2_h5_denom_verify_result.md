# M2-H5-DENOM-VERIFY Result

- **ticket**: `H5-DENOM-VERIFY-FAIL`
- **mode**: verify（验证采数=是；落地=否）
- **gate**: `fd00a06058b239abfe03a52175463a52741718318ce99416a4ce762cf037889b` @ 2026-10-26 16:09:00 Asia/Taipei — body sha match=True（开工前+结题复核）
- **approved**: pidan @ 2026-10-26 16:12:48 Australia/Perth
- **path**: PATHB-A = V2-clamp · N=5 · order=2 · P-VS=OFF · A′=OFF（执行中未偷开）
- **KEEP denom**: `v_denom ≡ v_pre`（二次 LS · w=5 · 同 QPC · 同 `t_int=min(t_comp,t_last_WGC)`）
- **calib**: C1=`median(v_denom/v_hat)` exact=`1.3358225694150683`（n=120 from 3 cold calib sessions；≠0.7510829456 / ≠0.7511）
- **composition.h**: PRE=POST=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（未改）
- **machine**: DESKTOP-K93N3P8

## G4 ONELINE

```
M2-H5-DENOM-VERIFY | gate=fd00a06058b239abfe03a52175463a52741718318ce99416a4ce762cf037889b @ 2026-10-26 16:09:00 Asia/Taipei | approved=pidan@2026-10-26 16:12:48 Australia/Perth | mode=verify | ticket=H5-DENOM-VERIFY-FAIL | KEEP v_denom≡v_pre；path=PATHB-A；calib=C1 exact=1.3358225694150683；禁静默0.7511；验证采数=是；落地=否；七hash不翻盘；禁抬thr；成功≠落地采用≠Δv-PASS；≠PERF-CI；禁立刻Δv″；单机 DESKTOP-K93N3P8
```

## Phase-Calib

| sess | appPID | n | session median(v_denom/v_hat) | notes |
|------|--------|---|-------------------------------|-------|
| 1 | 20964 | 40 | 1.3385438573753232 | calib_probe=1.0；SESSION_CALIB_OK |
| 2 | 43908 | 40 | 1.345761262050146 | SESSION_CALIB_OK |
| 3 | 28160 | 40 | 1.2074588251150606 | SESSION_CALIB_OK |

- **frozen calib_new** = median of all 120 ratios = `1.3358225694150683`
- 落盘：`artifacts/m2_h5_denom_verify_calib_frozen.txt` / `_calib_aggregate.txt` / `_calib_all_ratios.txt` / `_calib_s{1,2,3}_*`

## Phase-Score（frozen calib；禁再改）

| sess | appPID | med_r | ratioOk | smoke fwd | smoke rev | ctrl | verdict |
|------|--------|-------|---------|-----------|-----------|------|---------|
| 1 | 62180 | 1.1622 | True | 10/20 | 8/20 | basePass∧IVel0_both_FAIL | FAIL_CANDIDATE |
| 2 | 50644 | 0.9911 | True | 0/20 (SKIP jud=False) | 9/20 | basePass∧IVel0_both_FAIL | FAIL_CANDIDATE |
| 3 | 61368 | 1.1642 | True | 12/20 | 9/20 | basePass∧IVel0_both_FAIL | FAIL_CANDIDATE |

- pass_sessions = **0/3**
- H5 比三项齐：med_r∈[0.8,1.2] 且无 r_i≥2.0（denom=v_pre）
- smoke ≥17/20：**三项皆未达**（未抬 thr；未降 17/20）
- 对照 KEEP：三项皆 basePass ∧ IVel0_t060_both_FAIL

## 缺口（FAIL 原因）

- 门控齐备中 **smoke≥17/20** 全会话失败 → ticket=`H5-DENOM-VERIFY-FAIL`
- 非 INVALID（每会话有限样 n=40、无 hang_cap/too_few_frames 替换需求）
- 非 PASS（smoke 缺口）

## 例证：无影子 IV

- score s1 Arm-WGC-seed t060fwd k=0：`ivel_set=204.339` = `calib_new·v_hat`（1.3358225694150683×152.969）；`shadow_v_log=1359.06` ≠ ivel_set

## 七 hash 不翻盘声明

未改写、未重开、未宣称翻盘：

- PATH-A′ `f17871f1cd13d5be0a10ff1ac6707d96696bd5e13c8b8f3fbc398fef9f9b2362`
- PATH-B′ `a1161c77c5d9ca262d9371f2cab9c3ecd5e7c4138de6fc9ad58c3e579eea3a43`
- CALIB `09f6414bd22748133b2bf7354c49188e8af6708154ea534a3ebad1fb3e0f793d`
- ABS `831818e41a1fc445cdf763f97b506ad573a1efd9676d23cd3d106904c87180de`
- Δv′ `06467b8292f30239bb824a5379901fcaeefd2e0a79ba3442995bda84d9ae805c`
- TRUTH `276b97f07ba23da2f8e1be07212f1cf2ca9c367d64fb5a5d1881215963704f69`
- DENOM `d891df9fe8156427b0ffd9d0fbc63d46f7c7f694c70d921bde30f18ed32833c7`

## 标签边界

- 本票标签仅 `H5-DENOM-VERIFY-*`；**成功≠落地采用≠Δv-PASS**；≠PERF-CI；禁立刻 Δv″；落地=否
- 产物前缀 only `m2_h5_denom_verify_*` / `m2_h5_denom_verify_calib_*`

## 探针

- `MotionSoftDemo/M2H5DenomVerifyProbe.cs`（H5 分母=v_pre；C1；禁 0.7511；PATHB-A）
- MainPage 接入 flag/OpenLog/dispatcher；bak=`MainPage.xaml.cs.bak_h5denom_verify`

## 写结

- written_at: 2026-10-26 17:24:08 Australia/Perth
- pidan / DESKTOP-K93N3P8 / workbuddy executor
