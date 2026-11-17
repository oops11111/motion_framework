# API changelog（api_changelog.md）

> Rules: `docs/api_v0_frozen.md`. During the pilot only additions are allowed; rename / delete / signature or semantic changes go through a gate and are listed here with the gate reference.

| Date | Change | Reason | Affects pilot code? | Reference |
|---|---|---|---|---|
| 2026-11-06 16:32:43 AWST | **API v0 L1 frozen**: `MotionSystem`, `AnimationHandle`, `MotionTokens` as in the contract tables | pidan 「冻结 API v0 L1」 | — (baseline) | `docs/api_surface_m2_alpha_contract_LOCKED.md` `77416e8b…` |
| 2026-11-18 AWST | **internal fix（Z5-T3，非合同变更）**：① `FM_FLUIDMOTION_PATH` 环境变量**只在 Debug 构建读取**（`#if DEBUG`），Release 忽略；② `MotionSystem.Dispose` 在帧泵开着时关泵，并**多发一次 `IdleTransition`**（activeCount=0, pumpOn=0）。公开面、`fm_*`、E1 名字 / id / 字段 / GUID **不变**；`fm_capi.h/.cpp`、`composition.h` 不动。**Z4 r1 结论仍绑定 `fe59c622`**；今后任何新闸门须绑定**含 T3 的 tip**。 | pidan 2026-11-18 00:46 AWST「开 Z5-T3、Z5-T5 编码票」；Claude / notion ai 评审条件 | 是（仅内部行为：Release 不再读环境变量；Dispose 多一条 IdleTransition） | 分支 `wb/z5-t3`；`MotionSoftDemo/MotionSystem.cs`、`MotionSoftDemo/Z3aL1DevSmoke.cs`（新增必测用例：帧泵开着时 Dispose 须见 IdleTransition） |
