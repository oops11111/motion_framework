# FluidMotion C# API v0（api_v0_DRAFT.md）

STATUS=DRAFT（**待 pidan 评审后冻结**；冻结后改名为 `docs/api_v0_frozen.md`）

> 起草：Claude，2026-11-06（pidan：WP4 的 Z2–Z4 先定，按建议）。依据：`docs/m3_wp4_pilot_DRAFT.md`（Z2–Z4）、`docs/m3_plan_DRAFT.md` F4（按未来 WinMD 的形状冻结 C# API，底层先走 C ABI）、`docs/api.md`（现有 C++ 接口和 `fm_capi.h`）、`docs/m3_wp1_rive_host_requirements.md`（H1–H9）、`docs/m3_wp2_follow_guidelines.md`。
> 形状文件：`code/csharp/FluidMotion/FluidMotion.Api.v0.cs`（只有签名，不含实现；对照 WinAppSDK 2.5.1 编译 0 错误 0 警告）。

## 1. 形状规则（为了日后换成 WinMD 时调用处不变）
- 公开类型只有：sealed class、enum、只含公开字段的 struct；
- 不公开泛型类型；集合用 `IReadOnlyList<T>`（投影为 `IVectorView<T>`）；
- 不用只靠参数类型区分的重载；可选参数拆成单独的方法（例如 `Retarget` / `RetargetFrom`）；
- 事件用 `TypedEventHandler<TSender, TArgs>`；释放用 `IDisposable`（投影为 `IClosable`）；
- **所有成员都在创建它的 UI 线程上调用**（与 `fm_capi.h` 规则一致）。

## 2. 类型一览与底层映射
| 类型 / 成员 | 作用 | 底层 | 现状 |
|---|---|---|---|
| `MotionSystem(DispatcherQueue)` | 绑定一个 UI 线程的引擎；只在有动画时出帧 | `fm_create` + 帧源（`DispatcherQueueTimer`，由 `fm_render_fn` 开关） | 可直接实现 |
| `MotionSystem.LoadTokens(json)` → `TokenLoadResult` | 加载 `motion.json`（schema v1，严格） | `fm_load_tokens` | 可直接实现 |
| `MotionSystem.ReducedMotion` | 减弱动效；打开时运行中的动画吸附到终点 | `fm_set_reduced_motion`（读取需新增） | 写：可实现；读：**需新增 C ABI** |
| `MotionSystem.CreateAnimation(target, transition)` | 按 token 过渡创建动画；过渡不存在返回 null | `fm_animation_create` | 可直接实现 |
| `MotionSystem.CreateRetargetOnly(target)` | 空动画，只用于 Retarget | `fm_animation_create(…, NULL)` | 可直接实现 |
| `MotionSystem.TryGetCurrent(target, property, out value, out velocity)` | 当前值和速度 | `fm_current` | 可直接实现 |
| `MotionSystem.ActiveCount` | 活跃动画数 | `MotionSystem::ActiveCount` | **需新增 C ABI** |
| `MotionTarget.ForElement(UIElement)` | 被驱动的元素；v0 属性：`opacity`、`translateX/Y`、`scaleX/Y`、`rotation` | sink 回调写入元素的合成 visual（软件路径） | 可直接实现；合成器路径（模块 7）之后替换，对调用方透明 |
| `AnimationHandle.Play / Cancel` | | `fm_animation_play / cancel` | 可直接实现 |
| `AnimationHandle.Retarget / RetargetFrom` | 保留速度的打断（试点期 Bv 行为，F5） | `fm_animation_retarget` | 可直接实现 |
| `AnimationHandle.State` | | `AnimationHandle::State` | **需新增 C ABI** |
| `AnimationHandle.Ended`（`AnimationEndReason`） | 结束事件 | `AnimationHandle::OnEnd` | **需新增 C ABI**（回调，UI 线程派发） |
| `AnimationHandle.Dispose` | 放下句柄，运动继续 | `fm_animation_release` | 可直接实现 |
| `PointerFollow.Attach(follower, area, options)` | 拆出的部件由合成器跟随指针（WP2 约束 1–2） | 指针属性集 + `ExpressionAnimation` | 跟随：可直接实现；`SmoothingSeconds`（弹簧阻尼）：**实现方式待定**，参数先冻结 |
| `RiveView`（`Source`、`StateMachine`、`AutoPlay`、`Play`、`Pause`） | 承载 .riv 的 XAML 控件，内置 H1–H6 | `m0rive`（产品化） | 需按 H1–H9 产品化 |
| `RiveView.SetBooleanInput / SetNumberInput / FireTrigger` | 状态机输入 | Rive `StateMachineInstance` | **需新增 m0rive 接口** |
| `RiveView.DeviceLost / DeviceRecovered` | 设备丢失与恢复通知（H5） | `m0r_get_lost` 改为回调 | **需新增 m0rive 接口** |

## 3. v0 刻意不包含的（之后只能新增，见第 4 节）
`Pause / Seek / Reverse / SetRate / SetLoops`、markers、时间线直接构造（不经 token）、HWND 宿主（V1.5）、交换链路径（F1 可选）、诊断计时接口（保留为内部接口）。

## 4. 冻结规则（Z3）
1. pidan 评审通过后，本文档改名为 `docs/api_v0_frozen.md`，形状文件打标签 `api-v0`。
2. 试点期间**只允许新增**（新类型、新成员、新枚举值放在末尾）。**改名、删除、改签名、改语义**都要走闸门（DRAFT → LOCK → APPROVED），并通知试点团队。
3. 每次变更记在 `docs/api_changelog.md`（日期、变更、原因、是否影响试点代码）。
4. 标为“需新增 C ABI / m0rive 接口”的成员，签名已冻结，实现完成前调用会抛 `NotSupportedException`，并在文档中注明。

## 5. 试点指标（Z4）
| 指标（plan.md §7） | 怎么采 |
|---|---|
| 60 / 75 Hz 下的丢帧率、P95 / P99 帧耗时 | 应用内：DComp 帧统计（`DCompositionGetStatistics`，同 PERF-CI 的 PresentStats 口径）；Rive：渲染线程间隔和预算（`m0r_get_intervals`、`m0r_get_gpu_split`，M3 口径） |
| CPU、GPU、内存、续航、包体积、启动时间、闲置开销 | 应用遥测 + PresentMon / WPA（PERF-CI 口径）；闲置判定沿用 PERF-CI r2 草案（≥ 60 s 无 present、CPU/GPU ≤ 0.1%） |
| 设计修改到上屏的时间、替换掉的代码行数、token 复用率 | 试点团队记录 + 代码统计 |
| 崩溃率、无障碍缺陷数 | 应用遥测、缺陷系统；减弱动效开关必须生效（`ReducedMotion`） |
| 设计师和开发者满意度 | 问卷（试点结束时） |

## 6. 下一步
1. pidan 评审本草案 → 冻结（第 4 节）。
2. 实现“可直接实现”的成员，补 C ABI（`ActiveCount`、`ReducedMotion` 读取、`State`、`Ended` 回调）并加单测；`fm_capi_test` 回归。
3. `RiveView` 按 H1–H9 产品化 `m0rive`（含状态机输入和设备丢失回调），用现有 M3 探针回归。
4. Z1：pidan 提名候选应用后出评分表。
