# M2-WGC-SEED-ABS 闸门补洞决断（可 LOCK）

> DECISION_AT=2026-10-26 10:59:25 Asia/Taipei  
> 依据：`docs/deepseek_m2_wgc_seed_abs_review.md`；`docs/deepseek_m2_wgc_seed_abs_path_design.md`（顶替 Gemini；pidan 已批）；`docs/m2_wgc_seed_abs_gate_DRAFT.md`  
> 前序 FAIL gate `06467b82…805c` **不改、不重跑**；composition.h **不改**  
> LOCKED：`docs/m2_wgc_seed_abs_gate_LOCKED.md`

## 逐条

| 项 | 决断 |
|---|---|
| 默认路径 A | **采纳** P-LS+TS：末 N=5 二次 LS @ `t_int`；calib=1.0；ε=5.0 |
| 回退 B | IV=null+0；票 PASS 只评 A |
| P-VS | 默认关 |
| 影子 / composition.h | **否决** |
| H5 比 | 回归门槛 med_r∈[0.8,1.2] 且无 r_i≥2.0；calib KEEP 1.0 |
| smoke | t060 fwd+rev；工具链二次 w=5 + thr + P1/P2；≥17/20；base + IVel0 |
| 翻盘 | **禁**重跑/改写 `06467b82…805c`；结题标签 SEED-ABS-* 非 Δv-PASS |
| Δv″ | 挂账（新闸门） |
| Gemini | 地区限制；路径由 DeepSeek 顶替（pidan 已批） |

## 结论

**可 LOCK。**

— notion ai
