# motion_framework / FluidMotion

Windows 桌面端动效框架：声明式时间线 + 闭式解弹簧 + 三次贝塞尔缓动 + motion token，
优先下放到合成器，降级走应用线程软件计算。目标宿主是 WinUI 3。

> 状态（以代码为准，勿按旧文档推断）：核心数学、时间线、token、控制器与 lowering
> 规划器已实现，真实 `Windows.UI.Composition` 后端不存在，Rive、WinMD 投影、Figma
> 插件、预览宿主均未实现，性能 CI 继续 HOLD。`FluidMotion.L1 0.1.0` 是内部候选包，
> 未发布、未签名、不代表产品采用。详见 [docs/api.md](docs/api.md) 与
> [docs/z6_closed.md](docs/z6_closed.md) 的边界声明。

## 已实现 / 未实现

已实现（`code/`，C++17 头文件，命名空间 `motion`）：

- `spring.h`：闭式解阻尼弹簧，O(1) settle 时间上界。
- `bezier.h`：CSS 兼容三次贝塞尔求值与斜率。
- `timeline.h`：时钟、轨道、组合节点（sequence / parallel / stagger / marker）、播放器。
- `tokens.h`：motion token schema v1，严格校验器与加载器，reduced 变体必需。
- `controller.h`：`MotionSystem`、`AnimationHandle`、last-wins 属性所有权、保留速度重定向、`IDispatcher`。
- `composition.h`：能力探测、lowering 规划器、`CompositionDispatcher`，仅在模拟后端验证。
- `code/capi/fm_capi.h`：14 个 C 导出函数，软件路径，供 C# P/Invoke 使用。
- `src/FluidMotion.L1`：.NET 8 内部候选包，`MotionSystem` / `AnimationHandle` / `MotionTokens`（见 `MotionSoftDemo/MotionSystem.cs`）。

未实现（计划中有，`code/` 里没有）：真实 WinRT 合成器后端（模块 7）、Rive 渲染到
`CompositionDrawingSurface`、WinMD / C# 投影、`EmbedToHwnd` 正式 API、Figma 插件、
热重载预览宿主。`docs/api.md` 第 8 节有完整清单。

## 入口文档

- 公开 API 与接入：[docs/api.md](docs/api.md)（权威来源是 `code/` 下的头文件）。
- 使用与接入：[docs/usage.md](docs/usage.md)。
- 工程总览：[docs/engineering.md](docs/engineering.md)。
- 立项与实施计划：[plan.md](plan.md)；方案摘要：[summary.md](summary.md)；分歧结论：[disputes.md](disputes.md)。
- Z6 结题边界（dev-only）：[docs/z6_closed.md](docs/z6_closed.md)。

## 快速开始（只读 / 试算）

C++ 核心是纯头文件、无 OS 依赖，可直接包含 `code/` 下的头文件试算，例如：

```cpp
#include "spring.h"
#include "timeline.h"

motion::Spring spring(motion::SpringParams{0.35, 0.8}, /*from=*/0.0, /*target=*/100.0);
motion::SpringState state = spring.At(0.1);  // t 为自 (re)start 起的秒数
```

C-ABI 软件路径的构建命令见 `code/capi/fm_capi.cpp` 文件头注释（Linux 用 g++，
Windows 用 MSVC `cl`，均需加 `-I code/explore`）。L1 候选包的构建见
`src/FluidMotion.L1/README.md`：通过 `python tools/delivery/build_component.py`
新编原生 DLL 并打包验证，禁止用历史缓存 DLL。

## 线程与单位

- `Timeline` 构造后不可变、可共享；`Player` / `MotionSystem` / `AnimationHandle`
  须在同一条 UI dispatcher 线程上操作，头文件均非线程安全。
- 时间用秒（`double`），数值单位为 DIP 或调用方约定的任意标量。
- 全部动画默认可打断；应用层重定向走 `AnimationHandle::Retarget`，不要直接用
  `Player::Retarget`。

## Token 示例

`code/motion.json` 是 schema v1 示例，全部数值为占位、设计师拥有最终词表。
顶层键为 `version`（仅支持 `1`）、`durations`、`easing`、`springs`、`transitions`；
每个 transition 必须有 `root` 与 `reduced`（`reduced` 可为 `"instant"`）。
完整 schema 见 [docs/api.md](docs/api.md) 第 4 节与 `code/tokens.h` 文件头注释。
