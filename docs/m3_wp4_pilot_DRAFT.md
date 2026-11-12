# WP4 试点 1 启动草案（m3_wp4_pilot_DRAFT.md）

STATUS=DRAFT（规划文档，非真机闸门；不授权采数）

> 起草：Claude，2026-11-06（pidan Y3：Rive 层视为完成，下一步启动 WP4）。依据：plan.md §7（试点选择与指标）、`docs/m3_plan_DRAFT.md` WP4 与 F4（按未来 WinMD 的形状冻结 C# API，试点绑定它；底层先走 C ABI）、`docs/api.md`（现有 C++ 公开接口与 `fm_capi.h`）、`docs/m3_wp1_rive_host_requirements.md`。

## 1. 要定下来的事（Z1–Z4，附建议）
| # | 问题 | 建议 | 理由 |
|---|---|---|---|
| **Z1** | 选哪个应用做试点 1 | **需要 pidan 提名候选应用**（Claude 无法从仓库得知团队和产品）。按 plan.md §7 的标准打分：团队配合度高、中等规模、确有动效痛点（列表、导航、引导页）、带遥测、WinUI 3、非关键业务、设计师愿意写 token。请给出 1–3 个候选，Claude 出评分表 | 选型是 WP4 的前置 |
| **Z2** | 冻结哪些 C# API（F4） | **v0 只冻结试点需要的最小面**：`MotionSystem`（创建 / 销毁、帧驱动绑定）、`AnimationHandle`（播放 / 改目标 / 取消，Bv 行为）、`MotionTokens`（从 `motion.json` 加载）、`RiveView`（XAML 控件，内含 H1–H8：DPI、设备丢失恢复与通知、按屏节奏）、`PointerFollow` 辅助（按 WP2 约束把部件拆成表达式 visual）。形状按 WinRT 规则写（无重载歧义、无泛型公开类型、异步用 `IAsyncOperation` 形状），底层仍走 C ABI | 不让 WinMD 阻塞试点，接入代码之后不用返工 |
| **Z3** | API 冻结怎么管 | 写 `docs/api_v0_frozen.md` + C# 接口文件；试点期间只允许**新增**，改名或删除要走闸门；每次变更记 API 变更日志 | 试点方不被频繁改动打断 |
| **Z4** | 试点指标怎么采 | 沿用 plan.md §7 指标；帧时间和丢帧复用 M3 的探针口径（渲染线程间隔、DComp 帧统计、Rive 渲染 GPU），设计和开发效率指标用问卷 | 与 M3 数据可比 |

## 2. 建议的顺序
1. pidan 提名候选（Z1）→ Claude 出评分表 → pidan 选定。
2. Claude 起草 `api_v0` 的 C# 接口（Z2）和冻结规则（Z3）→ pidan 评审后冻结。
3. Claude 把 `m0rive` 原型按 H1–H9 做成 `RiveView` 背后的模块（含诊断接口），单测 + 现有 M3 探针回归。
4. 试点团队接入；按 Z4 采集指标；出试点报告（WP4 产出不是闸门票）。
