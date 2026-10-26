# DeepSeek 对 T-A1a 真机 FAIL 的短评审

- 时间：2026-10-24 23:10:27 Australia/Perth (UTC+8)（会话约 23:09:47–23:10:27）
- session：`session-5cc2c1f8-ee78-4090-b31f-7e1d6e1acdb9`
- provider / model：`deepseek-account` / `deepseek-v4-pro`（reasoningEffort=max）
- Harness：RPC `:59449`；composition.h 未改
- 依据：闸门 `docs/t_a1a_gate_LOCKED.md`（0750a840…f89d1d）、`docs/t_a1a_approved.md`、`docs/t_a1a_result.md` / ONELINE

---

# T-A1a 真机 FAIL 短评审（G4）

**Q1｜同意。** A-only×3=0/3 直接命中已 LOCK 的 G4-2（且优先于「不可复现」），票级 FAIL、归因「修正后未稳态」成立；本票非 INVALID（method_ok×3、n_invalid=0），不触发 G4-1。

**Q2｜建议：关 T-A1a 挂账**（FAIL 与网格数据留档），暂不直接开新 LOCK；补洞条件：①解释小 τ0 残差机制（常数偏移对小 τ0 不足）；②提出覆盖 3.3/6.8/8.3 的具体修正（改 OFFSET_METHOD 或网格）；③改后真机 ≥3 会话三格 RMS_gate≤0.41 且 21.8/33.3 不回退，方可新票发起新一轮 LOCK；④全程不改 composition.h，success≠adopt。

**Q3｜** 说明该常数偏移修正不具跨 τ0 普适性：仅对大 τ0（21.8/33.3）有效，小 τ0 残差约 1.2–1.4 未修净，属系统性残差而非噪声。

**Q4｜声明：** 不重开 S1；不改 composition.h（PRE/POST sha256 一致）。

