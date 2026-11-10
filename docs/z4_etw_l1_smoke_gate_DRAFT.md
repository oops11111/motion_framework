# Z4 · ETW + L1 smoke 门槛闸门（z4_etw_l1_smoke_gate_DRAFT.md）

STATUS=DRAFT（**NOT LOCKED**；不授权采数；不授权编码）
LOCKED_AT: （LOCK 时填）
GATE_BODY_SHA256: （LOCK 时算；约定同其他闸门）

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> 起草：Claude，2026-11-06。Wave A 中 Z2 的配套文件（`docs/Z_SERIES_CONVERGE.md` 待办“Wave A 还差 Z4 ETW/L1 smoke DRAFT”）。依据：`docs/api_surface_m2_alpha_contract_DRAFT.md`（L1 = `MotionSystem` / `AnimationHandle` / `MotionTokens`，三张表）、`docs/m2_02_gate_LOCKED.md` 与 `docs/m2_02_result.md`（M2-02 已在 K93N3P8 上跑通 L1 smoke 与 ETW 最小 6 事件，当时名字是 `MotionEngine`）、`MotionSoftDemo/MotionEtw.cs`。
> 定位：L1 冻结并实现之后、交给试点之前的真机冒烟。**不是 PERF-CI**（不判帧预算）；与 M3 的 Rive 预算无关；不涉及 `RiveView` / `PointerFollow`。
> **pidan 再确认**：2026-11-10 22:35 AWST（Australia/Perth）书面确认本 DRAFT 路径与四条 INVALID / 边界句。**仍 NOT LOCKED**；**不授权采数 / 真机**。
> **E1–E4 裁决**：2026-11-10 22:56 AWST，pidan 书面委托「E1–E4 你来定」，由协调按 G3 既有建议作保守、不扩张裁决（见下表「裁决」列）。**本裁决 ≠ LOCK ≠ APPROVED ≠ 授权采数 / 编码**；闸门仍须 pidan 书面 LOCK + APPROVED 后才能采数。

## G0 硬边界
Success ≠ adopt；不合 main；不改 `composition.h`；无 LOCK + APPROVED 不采数；PERF-CI 仍 HOLD；不解冻 Bv；不重开 S1 / R-3 / R-2；禁影子 IV；Y4（`docs/COORDINATION.md`）；不把 PACING r4 的 0.3·T 写成 API 默认。

## G1 要验证什么
1. 冻结后的 L1 名字（`MotionSystem` / `AnimationHandle` / `MotionTokens`）经 C ABI 桥在真机上端到端可用，行为与 M2-02 相同：可打断、从当前实时状态接续、无跳变。
2. 减少动效钩子生效：打开时运行中的动画吸附到终点，`Retarget` 直接跳到目标。
3. 空闲即空闲：没有动画时帧源停止（`fm_render_fn(0)`），不再有帧回调。
4. ETW provider 在真机上可采集，事件名和字段可进 WPA；试点遥测以此为准。

## G2 判据（每个会话）
| 判据 | 门槛 | 来源 |
|---|---|---|
| ① L1 序列 | `MotionSystem` 创建 → `MotionTokens` 加载（含 1 次故意出错的 token 文件，诊断带 JSON-Pointer 路径）→ `CreateAnimation` → `Play` → `Retarget` ≥ 5 次（播放中途打断、同属性改目标、跨动画切换各 ≥ 1）→ `Cancel` → `Release`（句柄放下后运动继续到结束） | 契约表 1；M2-02 G2-LOCK-2 |
| ② 连续性 | 每次 `Retarget` 从上一帧 `Current` 接续：首帧位置差 ≤ 动态容差（沿用 M2-02 `dynTol`）、无瞬移、单帧步长 < 80 DIP；`jumpFails = 0` | M2-02 |
| ③ 减少动效 | 动画运行中打开 → 下一帧到终点并结束；打开状态下 `Retarget` 一帧到目标、速度 0 | 契约表 2；api.md §5.7 |
| ④ 空闲 | 所有动画结束后 ≤ 2 帧内帧源停止，此后 5 s 内帧回调 0 次；`IdleTransition` 事件 ≥ 1 | plan.md 闲置测试 |
| ⑤ 线程 | 所有调用在创建 `MotionSystem` 的 UI 线程上；探针断言命中 0 次越线程调用 | 契约表 2 |
| ⑥ ETW | provider 打开时 `AnimationLifecycle`、`Retarget`、`FrameTick`、`IdleTransition`、`TokenLoad` 各 ≥ 1；6 个事件名都能在 WPA（或 EventPipe 采集结果）中看到 | M2-02 G2-LOCK-3 |

- **会话**：每台机器 ≥ 3 个独立进程。
- **PASS** ⇔ ①–⑥ 每个会话都满足（即“C# API usable”的 L1 smoke PASS）。**FAIL**：已进入可裁决采数且任一条不满足。
- **INVALID**（写死，任一即 INVALID）：
  1. 未 LOCK 或未 APPROVED 就采数；
  2. 改动 `composition.h`（PRE ≠ POST）；
  3. 使用影子 IV，或采数所用的代码 tip 未在回报中声明 / 与 LOCK 记录不符；
  4. 把 PERF-CI HOLD 当作已通过（例如在本票结论中声称满足帧预算或 PERF-CI）。
  另：缺产物、闸门 hash 不符，同样 INVALID。
- **边界（写死）**：L1 smoke PASS ≠ PERF-CI；不折进 M3 的 Rive 预算判定；不解冻 Bv；不重开 S1 / R-3 / R-2；任何新采数仍须本闸门 LOCK + APPROVED。

## G3 E1–E4（原待裁决；2026-11-10 22:56 AWST 协调已裁，仍待 LOCK）
下表「建议」为 2026-11-06 起草原文，保留不改。「裁决」为协调按 pidan 委托于 2026-11-10 22:56 AWST 写下的正式裁定：采纳原建议，不扩机台、不增事件、不改 GUID、不解冻 PERF-CI、不把本票开进试点应用或真实合成器后端。

| # | 问题 | 建议（起草） | 理由 | 裁决（协调 · 2026-11-10 22:56 AWST） |
|---|---|---|---|---|
| **E1** | ETW 事件名、provider 名和 GUID 要不要随 L1 一起冻结 | **冻结事件名、provider 名（`FluidMotion-Motion`）和 GUID（`8f3a2c1d-6b4e-4a91-9c7d-2e5f8a1b3c4d`）**；字段只许在末尾追加；事件 id 冻结 | 试点遥测和 WPA 配置会依赖它们；M2-02 闸门当时写明“不冻结”，试点前需要定下来 | **采纳建议。** 事件名 / provider 名 `FluidMotion-Motion` / GUID `8f3a2c1d-6b4e-4a91-9c7d-2e5f8a1b3c4d` / 事件 id 随本 DRAFT 文本冻结，供日后 LOCK 引用；字段只许在末尾追加，不得改名、删字段、换 GUID。本冻结是闸门合同文字，**不是** LOCK 本文件，也**不**授权采数或改 `composition.h`。 |
| **E2** | 在哪些机器上跑 | **K93N3P8（Win10）和 IJI1KJG（Win11）各 3 个会话**，unpackaged 和打包各至少 1 个会话 | 试点应用多为打包部署；两种系统都要覆盖 | **采纳建议，不扩机台。** 仅 K93N3P8（Win10）与 IJI1KJG（Win11）；每台 ≥ 3 个独立进程会话；unpackaged 与打包各至少 1 个会话。未点名的机器不得采数。 |
| **E3** | ETW 怎么采 | **EventPipe（不需要管理员）为主**；有管理员权限时另用 `logman` 采 ETL 进 WPA 作为对照 | M2-02 在 K93 上 `logman` 被拒；两条路都留 | **采纳建议。** EventPipe（无需管理员）为唯一主路径；`logman` ETL→WPA 仅为有管理员时的可选对照，**不**作为 PASS 必要条件（K93 上 `logman` 曾被拒）。 |
| **E4** | 由谁、在哪个宿主上跑 | **Z3a：MotionSoftDemo 作为狗粮宿主**，经现有软件 / C ABI 桥跑 Play → Retarget → Cancel，探针改用 L1 名字（`MotionEngine` 改名）；不在试点应用里跑；真实 WinRT 合成器后端不是本票目标（pidan 2026-11-06：C-interface 先通） | 与契约“Z3a C-ABI 狗粮”一致；试点应用尚未选定（Z1） | **采纳建议。** 宿主仅 MotionSoftDemo；经现有软件 / C-ABI 桥跑 Play→Retarget→Cancel；探针改用 L1 名字。不在 Z1 试点应用内跑本闸门；真实 WinRT Composition 后端仍为诚实非目标。本裁决不授权编码（编码仍须另票「开 API v0」）。 |

## G4 前置与顺序
1. pidan 同意 Z2 契约修订稿并冻结 L1（`docs/api_v0_frozen.md` 规则）。
2. 另票“开 API v0 实现”：Claude 把 MotionSoftDemo 中的 `MotionEngine` / `FmBridge` 包装改成 L1 名字，补 `fm_*` 中缺的 C ABI（如契约附录所列），单测 + `fm_capi_test` 回归。
3. Z3a 彩排一次（不采数）。
4. 本闸门 E1–E4 **已裁决**（2026-11-10 22:56 AWST）→ 仍须 pidan 书面 **LOCK + APPROVED** → 才可采数。裁决本身 ≠ LOCK。

## G5 禁令
同 G0；本 DRAFT ≠ 授权采数 ≠ 授权编码；L1 smoke PASS ≠ PERF-CI ≠ 采用。
