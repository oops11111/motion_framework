# 探索轮汇总（2026-10-19）

参与：Gemini（`gemini_explore.md`）、DeepSeek（`deepseek_explore.md`）、Claude（额度至 13:10，仅有部分 `composition.h`）。
协作名单（2026-10-19 起）：Gemini / Claude / DeepSeek / **workbuddy cli**（Windows 本机 CLI/代码）。

## 1. 文档再评结论
- Gemini 头文件版 api/usage 签名基本正确；DeepSeek P2–P5、P7 已到位。
- **证据口径**：DeepSeek 在 Windows 审阅机上找不到 g++，质疑「本机复验」；协调 bot 所在 Linux box 已有 g++，并已把复跑日志存入 `artifacts/linux_gtest_log.txt`（建议采纳开放问题 Q1 的方案 A）。
- Gemini 探索稿中的 `IDispatcher` 方法名与 `controller.h` 不符，以头文件 / `composition.h` 为准。
- P8（扁平目录断链）对完整仓库布局影响较小；审阅机拷贝目录需随附 engineering 等文件。

## 2. 代码进展
| 模块 | 状态 |
|---|---|
| spring/bezier/timeline/tokens/controller | Linux 单测通过（见 artifacts 日志） |
| composition.h（模块 6 半成品） | 已入 `code/`；冒烟可链；缺测试；Claude 13:10 后续写 |
| bezier 访问器 | 为配合 composition 已补 `X1/Y1/X2/Y2/IsLinear` |

## 3. MVP / 路线对比
| | Gemini | DeepSeek |
|---|---|---|
| 路径 | WinUI3Dispatcher + CompositionAnimator + C# 绑定 | 跳过 WinMD/Rive：控制台 golden → 软件路径 WinUI3 → 单属性 CompositionDispatcher → 热重载 |
| 工期感 | M0 5 日切片 | 演示约 6–10 人日 |
| Skia 触发 | 设备丢失 / 持续视觉错位 | （同方向，详见原文） |

**建议**：先做 DeepSeek 的阶段 0–2（golden + 软件路径演示），并行等 Claude 补完 composition 测试；合成器单属性试点对齐 Gemini Day2。

## 4. 里程碑
DeepSeek：仅可部分压缩 M1（逻辑已交付），微压 M2；M0/M3–M5 不可压；回收周数进缓冲。与 Claude 修订的 30–34 周结构一致。

## 5. 开放问题
**来自 DeepSeek**
1. 测试证据门槛：A 入库测试+日志，否则降级「待复验」 / B 删除本机复验字样。→ **已裁决：A**（用户 2026-10-19；证据见 `artifacts/linux_gtest_log.txt`）。
2. 是否拆 M1a（纯逻辑已过）/ M1b（合成器真机），任务 6 与 M0 并行？→ **已裁决：拆分并行**（用户 2026-10-19）。
3. 「仅软件路径」是否算可演示，还是必须先有合成器 lowering？→ **已裁决：软件路径即可算可演示**（用户 2026-10-19）。

**来自 Gemini**
1. C++/C# 跨语言生命周期与回调泄漏
2. 后台最小化时 Tick 是否停、恢复如何校准时钟
3. 跨屏刷新率/DPI 突变时弹簧数值稳定性

## 5b. Windows golden（workbuddy｜2026-10-19）
- 源码同步：`C:\Users\admin\motion_framework\code`（自 `artifacts/code_sync.tar.gz`）
- MSVC `cl` 复验：timeline 419 / tokens 617 / controller 236，0 failures；见 `artifacts/win_cl_golden_log.txt`
- 与 Linux `artifacts/linux_gtest_log.txt` 数字一致 → Q1=A 双侧证据齐
- 本机有 Windows App Runtime；缺 nuget `microsoft.windowsappsdk` 开发包（WinUI3 软件路径下一步）

## 5c. WinUI3 软件路径脚手架（workbuddy｜2026-10-19）
- 项目：`C:\Users\admin\motion_framework\MotionSoftDemo\`
- WASDK NuGet **2.5.1** + 模板 `winui`；`dotnet build` 0 警告；窗口可启动（假 Tick 16ms）
- 详见 `docs/workbuddy_soft_demo.md` → **Q3 软件路径演示门槛已落地（假 Tick）**；下一步可接真实 C++ Tick / 等 Claude composition

## 6. 下一步
1. §5 DeepSeek 三问已全裁决：Q1=A，Q2=拆并行，Q3=软件路径可演示。
2. 13:10 后 Claude 续写 composition 测试 + explore。
3. 三方互评开放问题，收敛进 `plan.md`。
