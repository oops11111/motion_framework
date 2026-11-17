# API changelog（api_changelog.md）

> Rules: `docs/api_v0_frozen.md`. During the pilot only additions are allowed; rename / delete / signature or semantic changes go through a gate and are listed here with the gate reference.

| Date | Change | Reason | Affects pilot code? | Reference |
|---|---|---|---|---|
| 2026-11-06 16:32:43 AWST | **API v0 L1 frozen**: `MotionSystem`, `AnimationHandle`, `MotionTokens` as in the contract tables | pidan 「冻结 API v0 L1」 | — (baseline) | `docs/api_surface_m2_alpha_contract_LOCKED.md` `77416e8b…` |
| 2026-11-18 AWST | **登记（非变更）**：L1 的 C# 形态包含 `MotionSystem.ValueChanged`、`MotionSystem.FramePumpChanged` 事件，`SpringParams`、`MotionSample` 结构，`AnimationHandle.Id`（`MotionSoftDemo/MotionSystem.cs`）。这是合同 Table 1（L1 ↔ C++ / `fm_capi`）的 C# 形态，**非新能力，非冻结外新增**；无改名 / 删除 / 签名或语义变化。 | Z5 T2 文档补记（Claude / notion ai 评审条件） | 否 | `docs/api_surface_m2_alpha_contract_LOCKED.md` Table 1；`docs/z3a_cabi_l1_ticket.md` |
| 2026-11-18 AWST | **已知限制登记（非变更）**：`PresentStats` 当前为占位值（dropped=0, intervalMs=16.666），不是测量值；试点遥测不得读其值；真实 present 统计须另开票。E1 名字 / id / 字段 / GUID 不变。 | Z5 T2 文档补记 | 否 | `docs/api_v0_frozen.md`「已知限制」 |
