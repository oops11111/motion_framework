# workbuddy 派单：M2-01（自包含）

派单时间：2026-10-24 23:42:17 AWST
执行环境：Windows DESKTOP，经 CodeBuddy 执行。

## 授权
- 闸门：`docs/m2_01_gate_LOCKED.md`（STATUS=LOCKED，LOCKED_AT=2026-10-24 23:41:05 AWST，GATE_BODY_SHA256=182f945c77b7103b8883be523742af8e6dcd8ca2d4232529a5fac347a168beb1）
- 批准：`docs/m2_01_approved.md`（APPROVED_BY=pidan，APPROVED_AT=2026-10-24 23:42:17 AWST）
- **严格逐字遵循 LOCKED 闸门**；不得修改闸门 body / hash；不得在本闸门追加补测（违规 → INVALID，须新 LOCK）。

## 范围（仅 M2-01）
1. **Harness 底噪（G2）**：静止/零激励探针；会话 ≥3；每会话样本数与指标口径（RMS 或既有 harness 口径）按闸门写死，开工前在结题文件写明所选口径，不得混用。PASS ⇔ 3/3 会话中位 < 0.5 DIP；任一 ≥0.5 → FAIL；样本不足/口径偏离/hash 不符/拼凑补测 → INVALID。
2. **影子速度 2.6× 根因（G3）**：参照 #37（影子≈屏幕 2.6×；A 臂跳变约 100–207 DIP，只引用）。须给出单一主因（seed 源/坐标系/时间基/合成器属性路径之一）+ 至少一份真机或日志证据路径 + 明确修复是否触及 composition.h（若必须改 → 仅记根因 PASS，修复另票）。
3. **票级（G4）**：PASS ⇔ G2 PASS ∧ G3 PASS；任一 INVALID → 票 INVALID。
- 不做 M2-02（deferred）。

## 硬禁令（G0/G5）
- 不改 `composition.h`；开工前与结题时各校验一次 sha256 = `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`，不符即停并报 INVALID。
- 不重开 S1 / R-3；不解冻 B_v；不重开 R-2；不升级 Bv；不比 #49；不复活 S2。
- 成功 ≠ 采用；旁路挂账不阻塞、也不清。

## 产出
- 全部写入 `artifacts/m2_01_*`（原始数据、日志、会话汇总、composition.h hash 校验记录、根因证据）。
- 结题文件：`docs/m2_01_result.md`，含 G2/G3/G4 判定、证据路径、禁令重申、以及 ONELINE：
  `M2-01 | gate=182f945c77b7103b8883be523742af8e6dcd8ca2d4232529a5fac347a168beb1 @ 2026-10-24 23:41:05 AWST | approved=pidan@2026-10-24 23:42:17 AWST | noise=<P/F/I> | shadow2p6=<P/F/I> | ticket=<PASS/FAIL/INVALID> | 成功≠采用；不解冻Bv；不改composition.h`
