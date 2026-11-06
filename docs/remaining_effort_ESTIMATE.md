# 剩余开发总量估计（对照 plan.md）

- **STATUS**：ESTIMATE · 非闸门 · 不授权采数 / 不授权合 main
- **UPDATED**：2026-11-06 15:28 Asia/Shanghai
- **依据**：`plan.md` §4–§5；`docs/m3_plan_DRAFT.md`；`docs/m3_status.md`（分支 tip）；`docs/Z_SERIES_CONVERGE.md`；`docs/api.md`
- **口径**：Success≠adopt；实验室 PASS ≠ 产品采用；旁路挂账（R-2 速度连续等）**不计入**主线余量

## 一句话

相对原目标（**6 人全职、约 30–34 周**；模块表中值约 **84 人周** + 试点联调），当前主线**剩余约 50–80 人周**（估计）。若 4–6 人全力，日历约 **12–18 周**到 1.0；现有少人 + 多 bot 节奏日历更长。

## 进度对照（里程碑）

| 里程碑 | plan 周 | 现状（2026-11-06） |
|---|---|---|
| M0 技术验证 | 2 | **已关闭**（M0-A / M0-B r3 PASS；未走 Skia） |
| M1 核心 | 10 | **基本达成**（单测绿；分发器模拟路径；composition.h 冻结） |
| M2 Alpha | 16 | **部分**：C-ABI+P/Invoke 软件路径有；WinMD / 真实 Composition / PERF-CI / 完整 ETW+浮层 / 预览·Figma 仍开 |
| M3 Rive + 试点 1 | 22 | **Rive 层（现有硬件）基本完成**；试点 1 **未开始** |
| M4 Beta | 28 | 未开 |
| M5 1.0 | 32–34 | 未开 |

M3 Rive 层细节：DPI / LOST / FOLLOW PASS；MATRIX→PACING r4 PASS（75+180）；推迟格（200%/DRR/RDP/GPU 切换/热插拔）等硬件。

## 已基本吃掉（估计）

- M-0；模块 1–5（弹簧 / 贝塞尔 / 时间线 / token / 控制器）
- 模块 6 模拟分发器 + 规划器（**无**真实 WinRT 后端）
- 模块 8/9 实验室原型（`m0rive` + 真机票）
- 大量闸门 / 结题文档

## 剩余主线（人周 · 估计）

| 块 | 余量 | 备注 |
|---|---|---|
| 真实 Composition 后端（模块 6 残） | 4–6 | 诚实非目标，但 1.0 要 |
| WinMD / C# 投影（7） | 5–7 | `api_surface_m2_alpha_contract_DRAFT` 已有；**未冻结、未编码** |
| Rive 宿主产品化（8 残 / WP1） | 3–5 | H1–H9 要求已采用，代码仍原型 |
| 跟随残差（9） | 0–2 | FOLLOW PASS；产品化边角 |
| ETW + 调试浮层（10） | 2–4 | Z4 DRAFT 仍缺 |
| 预览宿主 + Figma（11） | 7–10 | 最大块之一 |
| 减少动效 / 画质分级（12） | 3–4 | |
| PERF-CI + 黄金帧实验室（13） | 5–8 | 仍 HOLD（缺第二档 GPU 机） |
| HWND 参考宿主（14） | 1–2 | |
| 文档 / 接入支持（15） | 4–6 | |
| 试点 1 接入（WP4） | 6–8 | Z1 未点名 |
| 试点 2 + M4/M5 收尾 | 6–10 | |
| 推迟矩阵格 | 2–4 | 等硬件 |

**合计**：约 **50–80 人周**。

## 卡点

1. Z1 试点点名（B/C 真实应用名）
2. API v0：合同修订 / 「冻结」+ **另票**才编码
3. 真实 WinRT Composition 后端
4. PERF-CI 第二台物理机
5. 预览宿主 + Figma

## 不改的硬边界

Success≠adopt；不合 `main`（除非 pidan 另令）；不改 `composition.h`（除非另 LOCK）；无 APPROVED 不采数；PERF-CI HOLD；Y4。
