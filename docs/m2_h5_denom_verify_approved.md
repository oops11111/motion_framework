# M2-H5-DENOM-VERIFY 批准记录

APPROVED_BY: pidan
APPROVED_AT: 2026-10-26 16:12:48 Australia/Perth
GATE: docs/m2_h5_denom_verify_gate_LOCKED.md
GATE_BODY_SHA256: fd00a06058b239abfe03a52175463a52741718318ce99416a4ce762cf037889b
LOCKED_AT: 2026-10-26 16:09:00 Asia/Taipei

## 显式字段（widget t263s1211）

| 字段 | 值 |
|---|---|
| 验证采数 | **是**（t060 fwd+rev + H5 比；≥3 独立进程；单机 DESKTOP-K93N3P8；≠PERF-CI） |
| 落地 | **否** |
| calib | **C1 = median(v_denom/v_hat)**（Phase-Calib→冻结→Phase-Score；禁静默 0.7511） |
| 路径 | **PATHB-A**（V2-clamp；P-VS=OFF）；A′ 默认不启用 |
| KEEP | v_denom≡v_pre；禁抬 thr/降 17/20；七 hash 不翻盘；禁立刻 Δv″ |

原文：`APPROVED 验证采数=是；落地=否；calib=C1=median(v_denom/v_hat)；路径=PATHB-A`

本批准授权 workbuddy 按 LOCKED 执行验证采数；**不**授权落地启用；**不**解冻 PERF-CI；**不**立刻开 Δv″。
