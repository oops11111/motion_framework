# Z3a · API v0 L1 C-ABI 编码票（z3a_cabi_l1_ticket.md）

- **TICKET**：Z3A-CABI-L1
- **STATUS**：OPEN · 已授权编码（仅本票范围）
- **AUTHORIZED_BY**：pidan
- **AUTHORIZED_AT**：2026-11-10 23:07 AWST（Australia/Perth）
- **授权原文**：pidan「继续，按建议进行」（在 Z1 B/C 已入席、E1–E4 已裁的前提下，按协调建议开 Z3a C-ABI 编码票，并准备 Z4 LOCK）
- **合同**：`docs/api_surface_m2_alpha_contract_LOCKED.md`（GATE_BODY_SHA256=`77416e8bbc700238b42a940a2f93d4466edec1ebd79352b007f86c267a663a0f`，LOCKED_AT=2026-11-06 16:32:43 AWST）
- **冻结规则**：`docs/api_v0_frozen.md`
- **ETW（E1）**：provider 名 `FluidMotion-Motion`；GUID `8f3a2c1d-6b4e-4a91-9c7d-2e5f8a1b3c4d`；事件名与事件 id 按 `docs/z4_etw_l1_smoke_gate_r1_LOCKED.md` 冻结；字段只许末尾追加
- **前置彩排**：`docs/workbuddy_z3a_rehearsal_dispatch.md`（旧名 `MotionEngine` 路径；不采数）。彩排结果仍待回报，**不阻断本票开写**；但 **Z4 真机采数**须本票的 L1 构建已经存在。
- **UPDATED**：2026-11-10 23:09:00 AWST

本文件即「另票开 API v0 实现」。冻结合同本身仍不授权合 `main`、不构成 adopt。

## 1. 范围（只做这些）

在现有软件后端 / C-ABI 桥上，实现冻结的 API v0 L1 三表，供 MotionSoftDemo 狗粮调用：

| 项 | 要求 |
|---|---|
| 公开面 | 仅 `MotionSystem` / `AnimationHandle` / `MotionTokens`（对齐 LOCKED 合同表 1–3） |
| 宿主 | 仅 **MotionSoftDemo** |
| 场景 | `Play` → `Retarget` → `Cancel`（含 `Release`、token 加载、减少动效钩子，与合同及 Z4 G1/G2 一致） |
| 后端 | 现有软件路径 + `fm_*` C-ABI（`code/capi/fm_capi.h` 已有 14 个导出）。缺的 L1 相关 C 导出按合同附录**只许新增** |
| ETW | 沿用 / 校准 `MotionSoftDemo/MotionEtw.cs` 至 E1：provider `FluidMotion-Motion`、GUID 与事件 id 不得改名或更换；探测改用 L1 名字 |
| 线程 | 遵守合同表 2（单 UI 线程；禁止在 sink/render 回调里调 `fm_*`） |

把现有狗粮包装（`MotionEngine` / `MotionAnimation` / `FmBridge` 或 `MotionBridge`）改到 L1 名字，或在其之上做一层公开包装并改探针引用。内部 P/Invoke 桥可保留，但**对外**不得并行再推 `MotionEngine` / `MotionAnimation`。

## 2. 明确不做（写死）

- 不改 `composition.h`。
- 不做真实 WinRT Composition 后端（诚实非目标，另票）。
- 不实现 `RiveView` / `PointerFollow`。
- 不把 PACING r4 `offset=0.3·T` / tip `69094ee` 写成 API 默认。
- 不合入 `main`。Success≠adopt。
- **本票不写 Z1 试点应用**（FluentHub / Files）的任何代码或侧叉集成；试点编码另票。
- **本票不采 Z4 真机数据。** 允许在 K93 上做构建、单测、以及开发用冒烟（验证能编过、API 对得上、harness 能打出事件）；这些结果**不算** Z4，不进 Z4 结题，不填 Z4 APPROVED。
- 不解冻 PERF-CI；不解冻 Bv；不重开 S1 / R-3 / R-2；禁影子 IV。
- 不新开 WinMD / 公开 WinMD 投影（Z3b，另票）。

## 3. 建议分支

- **新分支**：`claude/z3a-cabi-l1`
- **建议起点**：`dabd5b780818af2ee2a2dcde47f2fdf788b1011e`（`claude/m3-matrix` 在本票文档落地前的 tip）。若本票文档已先合入 `claude/m3-matrix`，则从**该文档提交**起分支，以便票文在编码分支上可见；不要从更旧的 tip 起，不要 force-push `claude/m3-matrix` 或 `main`。
- 编码只在 `claude/z3a-cabi-l1` 上进行；不把本票实现合进 `main`。

## 4. 文件布局（预期触达；以实现为准，不擅自扩大）

| 路径 | 角色 |
|---|---|
| `code/capi/fm_capi.h` / `fm_capi.cpp` | C-ABI；L1 已实现的 14 个导出保持兼容；缺项只许追加 |
| `code/capi/fm_capi_test.cpp` | C-ABI 回归，必须保持 PASS |
| `MotionSoftDemo/MotionEngine.cs`（及并列的 `MotionAnimation`） | 改为（或包装为）`MotionSystem` / `AnimationHandle` |
| `MotionSoftDemo/` 下 token 加载 | 落到 `MotionTokens`（或 `MotionSystem` 上与合同表 1 对得上的 Load 入口） |
| `MotionSoftDemo/MotionEtw.cs` | 保持 E1 冻结的 provider / GUID / 事件名与 id（现已是 `FluidMotion-Motion` + `8f3a2c1d-…3c4d`，id 1–6）；不得改名换 id |
| `MotionSoftDemo/M202Probe.cs`（或新的 Z3a L1 探针） | 冒烟改用 L1 名字；场景 Play→Retarget→Cancel |
| 内部桥（`FmBridge` / `MotionBridge` 等） | 可保留为非公开实现细节 |
| 测试 | 现有 `fm_capi_test` + 必要的 C# 单测 / 冒烟入口 |

不改 `composition.h`、不改 `m3/results_*`、不改已结题 artefacts。

## 5. 交付物

1. `claude/z3a-cabi-l1` 上可审查的实现（提交或补丁包），附 tip SHA。
2. MotionSoftDemo 能用 L1 名字走完 Play→Retarget→Cancel。
3. `fm_capi_test`（及新增单测）在 K93 上 PASS。
4. 开发用冒烟 harness：能发出 E1 冻结的事件名（至少 `AnimationLifecycle` / `Retarget` / `FrameTick` / `IdleTransition` / `TokenLoad`；最小集六名与 Z4 G2 ⑥ / M2-02 一致，含 `PresentStats`）。记录为开发日志，**标明 dev-only，非 Z4**。
5. `composition.h` PRE=POST 声明；未合 `main` 的说明。
6. 简短实现备忘（哪些文件改了、是否追加了 `fm_*`、公开面如何对表 1）。

## 6. 验收（本票 PASS ≠ Z4 PASS ≠ adopt）

本票验收在 K93 开发环境完成即可：

1. **能构建**：`fluidmotion.dll`（x64 MSVC，现有 `docs/api.md` §7.4 路径）+ MotionSoftDemo Debug/x64，0 个与本票相关的编译错误。
2. **API 对表**：公开 C# 面为 `MotionSystem` / `AnimationHandle` / `MotionTokens`，成员与 LOCKED 合同表 1 一致；不把已停车的 `RiveView` / `PointerFollow` 或 0.3·T 默认带进来。
3. **回归**：`fm_capi_test` PASS。
4. **冒烟 harness**：Play→Retarget（含中途打断）→Cancel 能跑通；harness 能打出 E1 事件名。此跑**不算** Z4 采数。
5. **硬边界**：`composition.h` 未改；无 FluentHub/Files 改动；无合 `main`。

未通过则停在本票，不进入 Z4 APPROVED 采数。

## 7. 谁做什么

| 角色 | 职责 |
|---|---|
| **Claude**（英文；网页或本地编码助手） | 对照 LOCKED 合同设计并**编写代码**（改名/包装、补 C-ABI 缺项、改探针与 ETW 对齐 E1、单测）。CloudAgent **不可用**，不经 CloudAgent 派工。产出提交或补丁，落在 `claude/z3a-cabi-l1`。派工见 `docs/claude_z3a_cabi_l1_dispatch.md`。 |
| **workbuddy**（DESKTOP-K93N3P8） | 取分支/补丁；在 K93 上构建、跑 `fm_capi_test` 与开发用冒烟；回报 SHA 与日志。不采 Z4、不填 APPROVED、不改 `composition.h`、不合 `main`。派工见 `docs/workbuddy_z3a_cabi_l1_dispatch.md`。 |
| **协调** | 只写本票与闸门文档；本包不写实现代码。 |
| **pidan** | 已授权开本票。Z4 采数仍须其在对话中发送「APPROVED Z4 r1」，且本票 L1 构建已存在。 |

## 8. 与 Z4 的关系

Z4 r1 闸门文本见 `docs/z4_etw_l1_smoke_gate_r1_LOCKED.md`（STATUS=LOCKED，APPROVED_* 为空）。**LOCK ≠ 采数。** 采数条件（同时满足）：(1) 本票 L1 构建已存在；(2) pidan 发送「APPROVED Z4 r1」且闸门 APPROVED_* 已填。本票在 K93 上的开发冒烟不得当作 Z4 会话。
