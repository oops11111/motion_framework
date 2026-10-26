# M2-01 门槛闸门（DRAFT · NOT LOCKED）

STATUS=DRAFT
DRAFT_AT: 2026-10-24 23:39:28 AWST
GATE_BODY_SHA256: （LOCK 时由协调侧填写）
APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

**本 DRAFT 不授权采数。** 须 STATUS=LOCKED 且 APPROVED_* 非空后，才可派 workbuddy。
**本票结论不解冻 B_v / 不重开 R-2 / 不重开 S1 / R-3。**

---

## G0 硬边界
- 不重开 S1 / R-3（CLOSED / STOP / NO-ADOPT）。
- 不改 `composition.h`（sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）。
- 成功 ≠ 采用；B_v ≠ ship 基线；不升级 Bv；不比 #49；不复活 S2。
- 旁路挂账（`docs/side_parked_hang_degrade.md`）不阻塞本票，本票也不清旁路挂账。
- 本票 PASS ≠ 解冻 B_v；≠ 重开 R-2。

## G1 票目标（写死）
1. **Harness 底噪**：在约定探针下，位置底噪中位 < **0.5 DIP**（定义见 G2）。
2. **影子速度 2.6× 根因**：对「影子追踪速度 ≈ 屏幕 2.6×」给出可复核的根因结论（单一主因 + 证据指针），并给出可操作的修复方向（仍不改 `composition.h`；修复代码若需另票）。

两项目标须在同一 LOCK 下分别判；票级 PASS ⇔ 两项均 PASS。

## G2 Harness 底噪判据
- 探针：静止 / 零激励（或闸门 LOCK 时写死的等价静止场景）；会话 ≥3；每会话有效样本按 LOCK 附件写死（建议 n≥60 帧或既有 harness 惯例，LOCK 时二选一并写死）。
- 指标：位置误差 RMS 或等价 harness 底噪量的**会话中位**（单位 DIP）；LOCK 时写死用 RMS 还是既有 harness 口径，不得混用。
- **PASS**：3/3 会话中位均 < 0.5 DIP。
- **FAIL**：任一会话中位 ≥ 0.5 DIP（且样本充足）。
- **INVALID**：样本不足、口径偏离 LOCK、未 LOCKED/未 APPROVED 即采数、`composition.h` hash 不符、追加补测拼凑。

## G3 影子 2.6× 根因判据
- 参照挂账：#37 影子速度约为屏幕 2.6×；A 臂可见跳变约 100–207 DIP（只引用，不改写）。
- **PASS**：同时满足：
  - 根因陈述可被独立复读（指出缺陷环节：seed 源 / 坐标系 / 时间基 / 合成器属性路径等之一为主因）；
  - 至少一份真机或日志证据支持该主因（路径写入结题文件）；
  - 明确「修复是否触及 `composition.h`」——若必须改则本票只能记根因 PASS + 修复另票；本票仍禁改 `composition.h`。
- **FAIL**：无法定位主因，或证据互相矛盾且无裁决，或仅有猜测无证据。
- **INVALID**：把「不解冻 B_v / 不重开 R-2」写成已解冻；或借本票改 `composition.h`；或流程违规（同 G2 INVALID）。

## G4 票级汇总
- 票级 **PASS** ⇔ G2 PASS ∧ G3 PASS。
- 票级 **FAIL** ⇔ 非 INVALID，且 G2 或 G3 为 FAIL。
- 票级 **INVALID** ⇔ 任一子项 INVALID 或 G0 违规 → 数据作废，须新一轮 LOCK（新时间戳 + 新 hash），不得在本闸门追加。

## G5 禁令（结题必须重申）
- 不重开 S1 / R-3；不改 `composition.h`；成功 ≠ 采用；结论不解冻 B_v / 不重开 R-2；不升级 Bv；旁路挂账不挡主线、本票也不清旁路。

## G6 ONELINE 模板
M2-01 | gate=<hash> @ <LOCKED_AT> | approved=<who>@<when> | noise=<P/F/I> | shadow2p6=<P/F/I> | ticket=<PASS/FAIL/INVALID> | 成功≠采用；不解冻Bv；不改composition.h
