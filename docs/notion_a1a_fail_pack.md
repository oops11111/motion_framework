# T-A1a 真机 FAIL 评审中文包（notion_a1a_fail_pack.md）

> 来源：Notion AI Opus 5.5；全文见 `/workspace/notion_a1a_fail_reply.md`
> 落盘：2026-10-24 AWST（Australia/Perth）
> 权威数据：`docs/t_a1a_result.md` / `artifacts/t_a1a_*`；闸门 `docs/t_a1a_gate_LOCKED.md`

### 结论

- **票级结果**：ticket=FAIL；A-only×3 = 0/3；归因为修正后未稳态（G4-2）。
- **非 INVALID**：三个会话都是 method_ok=True，used_2x=False，used_magic_single=False，A1c 的 n_invalid=0。G4-1 未命中，G5 未触发。
- **参数**：corr=CONFIRM_F5_REFUTE_2X；N_h=0.41。OFFSET_METHOD 为 residual=(δ0−δ_rest)−τ0，δ0=39.5，k=median(residual|未饱和)，ref=+2.70。
- **闸门**：docs/t_a1a_gate_LOCKED.md，hash=0750a8402d4118c98025e71863f79bafd689966c2565fa922d0674e8bcf89d1d，LOCKED 于 2026-10-24 22:27:54 AWST。
- **审批**：APPROVED，pidan，2026-10-24 22:34:47 AWST，结题留证 docs/t_a1a_approved.md。
- **composition.h**：PRE/POST match=True，未修改。
- **权威来源**：docs/t_a1a_result.md 和 artifacts/t_a1a_*。

### 会话表

| sess | k_est | A1a | A1b | A1c | A2 | A |
| --- | --- | --- | --- | --- | --- | --- |
| S1 | +3.70 | F | F（−120@220，RMS 1.685） | P（RMS_med 0.221） | P（PS，n=8） | F |
| S2 | +2.70 | F | P | P（0.233） | P（PS，n=8） | F |
| S3 | +3.45 | F | F（+240，RMS 1.995） | P（0.227） | P（PS，n=8） | F |

A1c 和 A2 都是 3/3 P；A1b 只有 S2 是 P；A1a 三个会话都是 F，所以 A 为 0/3。

### A1a 网格摘要

RMS_gate 中位值，每格 n=5，N_h=0.41：

| τ0 | S1 | S2 | S3 | cell |
| --- | --- | --- | --- | --- |
| 3.3 | 1.220 | 1.251 | 1.246 | FAIL×3 |
| 6.8 | 1.320 | 1.323 | 1.328 | FAIL×3 |
| 8.3 | 1.384 | 1.373 | 1.371 | FAIL×3 |
| 21.8 | 0.191 | 0.202 | 0.191 | PASS×3 |
| 33.3 | 0.131 | 0.123 | 0.127 | PASS×3 |
- **小 τ0 系统性 FAIL**：3.3、6.8、8.3 三格的 RMS 约为 1.22–1.38，三个会话之间差异约 ±0.03，并且随 τ0 单调上升。
- **大 τ0 修正有效**：21.8 和 33.3 两格修正后 PASS×3。
- **解读**：常数偏移修正对大 τ0 有效，对小 τ0 无效，提示残差中有与 τ0 相关的项。

### 建议下一步

- **结论：关票挂账。** T-A1a 按 FAIL 结题，闸门维持 LOCKED，不在本票名下重跑。
- **不新开 LOCK 的理由**：
    1. 本票不是 INVALID，G5 未触发。
    2. 看到结果后再改 OFFSET 方法或删减网格，属于事后调参或挑选数据，会破坏预注册效力。
- **挂账项（只记录假设，不采新数）**：
    - (a) 小 τ0 残差可能与 τ0 相关，或者处于饱和、非线性区。
    - (b) k_est 偏离 ref（S1 +3.70，S3 +3.45）可能与 A1b 的 F 相关。
- **后续验证方式**：以上假设如需验证，必须另开独立票，单独写闸门并 LOCK、单独审批。

### 禁令

- 不重开 R-3（已 CLOSED）。
- 不重开 S1。
- 不修改 composition.h（sha256 a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418）。
- 成功≠采用：后续任何新票 PASS 都不等于自动采用。
- 不升级 Bv。
- 不回改 docs/t_a1a_gate_LOCKED.md（hash 0750a8402d4118c98025e71863f79bafd689966c2565fa922d0674e8bcf89d1d）。
- 本包只做整理和结论，不采新数。

— notion ai（T-A1a FAIL 评审；ticket=FAIL；归因=修正后未稳态；成功≠采用）
