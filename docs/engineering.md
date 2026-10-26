# Windows 桌面端动效框架 (FluidMotion Engine) 工程规范与架构设计文档

> 公开接口与接入步骤（以 `code/` 头文件为准）：[`api.md`](./api.md)、[`usage.md`](./usage.md)。

---

## 一、 项目概述与目标 / 非目标

### 1.1 项目概述
`FluidMotion Engine` 是专为 Windows 桌面端打造的高性能、低功耗、嵌入式动效与微交互引擎。它通过将复杂的物理弹簧与贝塞尔时间线编译为系统的 `Windows.UI.Composition` 动画，并深度集成 Rive 矢量渲染，实现设计资产（Figma）到原生高帧率渲染的全链路打通。

### 1.2 核心目标
*   **极致流畅**：完美支持 120Hz/240Hz 高刷屏及 VRR（可变刷新率），主线程零阻塞（动画运行在 DWM 独立合成线程）。
*   **超低资源消耗**：应用闲置（无动画播放）时，动效引擎对 CPU/GPU 的额外占用严格趋近于 $0\%$ ($\le 0.1\%$ *(估计)*)。
*   **设计开发一体化**：通过 Figma 插件导出 Motion Tokens，实现设计改动至本地预览的极速闭环。
*   **开发友好**：核心由 C++/WinRT 实现，对外暴露标准的 WinMD / C# 绑定及 C ABI。

### 1.3 明确非目标 (Non-Goals)
*   **不做跨平台 UI 渲染**：100% 深度绑定 Windows 桌面底层生态（DWM / DirectComposition）。
*   **首发不提供正式 WPF 支持**：仅提供基于 `CreateDesktopWindowTarget` 的裸 HWND 参考宿主示例，不承担复杂的 WPF Airspace 与无障碍树适配。
*   **不自研 2D 光栅化引擎**：复杂矢量交互插画完全依赖 Rive C++ Runtime。

---

## 二、 架构总览


```

[ Figma 设计端 ] ──► (Motion Tokens JSON)
│
▼
[ 编译与分发管线 ] ──► Capabilities Dispatcher (能力检测与分发 / IDispatcher)
│
┌─────────────────┴─────────────────┐ (硬件支持与平台判定)
▼                                   ▼
[ 现代 WinUI 3 路径 ]               [ 降级兼容路径 (Fallback) ]
├── Windows.UI.Composition          └── UI 线程定时器 / 软件重绘 *(待真机验证)*
├── 闭式弹簧 / 贝塞尔编译
└── Rive 渲染至 CompositionSurface

```

### 2.1 核心组件分层
1.  **Core Math & Timeline Engine (C++20)**：纯数学计算层，负责弹簧闭式解、贝塞尔求值、时间线状态机及重定向。
2.  **Capabilities Dispatcher (能力分发器 / `IDispatcher`)**：检测宿主环境的 Windows 编译版本与显卡驱动支持情况，自动选择渲染后端（首选 `Windows.UI.Composition`，必要时降级到 Skia + DComp Swapchain 或 UI 线程软件 Timer）。
3.  **Token & Compiler Pipeline**：将设计端产出的 `motion.json` 编译为运行时可直接下放给合成器的动画属性。

---

## 三、 模块说明与目录约定

### 3.1 源码目录结构
```text
/src
  ├── core/               # C++ 核心动效引擎 (已完成单机纯逻辑测试)
  │     ├── spring.h      # 已实现：闭式弹簧物理模型
  │     ├── bezier.h      # 已实现：贝塞尔曲线与求值
  │     ├── timeline.h    # 已实现：声明式时间线与状态机 (本机 419 项全过)
  │     ├── tokens.h      # 已实现：Motion Token 解析与结构定义 (本机 617 项全过)
  │     └── controller.h  # 已实现：控制器、AnimationHandle、MotionSystem 与 IDispatcher (本机 236 项全过)
  ├── rendering/          # 待开发：渲染管线 (Composition Surface & Rive 绑定)
  ├── bindings/           # 待开发：WinMD / C# 互操作层
  └── host/               # 待开发：WinUI 3 试点宿主与 HWND 参考示例
/tools
  └── figma-plugin/       # Figma Motion Token 导出插件
/tests                    # 单元测试与性能基准套件

```

### 3.2 当前代码落地状态

* **已实现并验证模块**：`spring.h`、`bezier.h`、`timeline.h`、`tokens.h` 以及 `controller.h`（包含 `AnimationHandle`、`MotionSystem`、`IDispatcher` 接口）。
* **本机测试覆盖 (Linux 环境)**：
* `timeline.h` 测试项：419 个，0 失败。
* `tokens.h` 测试项：617 个，0 失败。
* `controller.h` 测试项：236 个，0 失败。


* **工程架构演进变更**：`Player::Retarget` 接口已被显式**弃用（Deprecated）**，重定向逻辑完全统一收拢至 `MotionSystem` 控制器中管辖。

---

## 四、 控制器与重定向架构 (增补)

### 4.1 控制器与句柄 API 说明 (`controller.h`)

动效系统采用 **`MotionSystem` 主控制器 + 轻量级 `AnimationHandle**` 的设计，解耦动画触发与生命周期管理。

```cpp
namespace FluidMotion {

// 抽象分发器接口，作为 CPU 动效系统与下层合成器/渲染管线的解耦桥梁
class IDispatcher {
public:
    virtual ~IDispatcher() = default;
    virtual void DispatchPropertyUpdate(uint64_t target_id, PropertyType prop, float value) = 0;
    virtual void DispatchAnimationStart(uint64_t target_id, PropertyType prop, const AnimationSpec& spec) = 0;
    virtual void DispatchAnimationCancel(uint64_t target_id, PropertyType prop) = 0;
};

// 动画生命周期控制句柄
class AnimationHandle {
public:
    AnimationHandle(uint64_t handle_id, std::weak_ptr<MotionSystem> system);
    
    void Cancel();                   // 取消当前动画
    bool IsActive() const;           // 查询动画是否仍在运行
    void SnapToEnd();                // 立即吸附到终点（用于无障碍或强行终止）
private:
    uint64_t m_handle_id;
    std::weak_ptr<MotionSystem> m_system;
};

// 全局/局部动效调度系统
class MotionSystem : public std::enable_shared_from_this<MotionSystem> {
public:
    explicit MotionSystem(std::unique_ptr<IDispatcher> dispatcher);

    // 播放指定属性动画，返回控制句柄
    AnimationHandle AnimateProperty(
        uint64_t target_element_id, 
        PropertyType property, 
        float target_value, 
        const MotionToken& token
    );

    // 设置系统无障碍：减少动效 (Reduced Motion)
    void SetReducedMotionEnabled(bool enabled);

    // 引擎 Tick 推进（用于非合成器/降级 Timer 场景）
    void Advance(float dt_seconds);
};

} // namespace FluidMotion

```

### 4.2 属性所有权与重定向规则 (Property Ownership & Retargeting)

为保障多来源微交互并发触发时的视觉一致性，系统制定了以下硬性规则：

1. **最后胜出原则 (Last-Wins Ownership)**：
* 每个 UI 元素的指定属性（如 `Element_A.Opacity` 或 `Element_A.ScaleX`）在同一时刻**有且仅有一个**活跃动画拥有所有权。
* 当对同一个 `(target_element_id, property)` 启动新动画时，旧动画句柄（`AnimationHandle`）会被自动失效（Invalidated），所有权抢占转移至新动画。


2. **保留速度重定向 (Velocity-Preserving Retargeting)**：
* 旧的 `Player::Retarget()` 接口因易导致状态不同步已被完全废弃。
* 当所有权转移发生时，`MotionSystem` 会自动捕获旧动画在中断瞬间的**当前实际数值 ($x_0$)** 与**瞬时速度 ($v_0$)**。
* 如果新 Token 为弹簧动画（Spring），系统采用闭式弹簧公式直接以 $(x_0, v_0)$ 为初始状态无缝过度到新的目标值，无需经过 CPU 逐帧重算，避免视觉上的速度突变或拉伸跳变。



### 4.3 减少动效 (Reduced Motion) 无障碍行为

当响应 Windows 系统无障碍开关 `UISettings.AnimationsEnabled == false` 或调用 `SetReducedMotionEnabled(true)` 时：

* **中途吸附 (Snap-to-End)**：系统不会触发长达数百毫秒的位移/缩放过渡，所有新发起的动画均会在第 1 个 Tick 内直接调用 `SnapToEnd()`，瞬间到达终点值。
* **状态保持**：对于运行中的动画，句柄会收到中断通知并立即将最终 Target 提交至下层 `IDispatcher`，确保业务状态与 UI 最终呈现完全一致。

### 4.4 `IDispatcher` 与下层合成器的衔接设计

`IDispatcher` 是 C++ 算法引擎与底层渲染平台的隔离边界：

```
                             ┌─── WinUI3Dispatcher ──► Windows.UI.Composition (合成器硬件动画)
                             │
[ MotionSystem ] ──► IDispatcher ──┼─── DCompDispatcher ───► DirectComposition (HWND 裸宿主)
                             │
                             └─── SoftwareDispatcher ─► UI 线程 Timer 降级 (Fallback)

```

* **合成器路径 (Composition Path)**：`DispatchAnimationStart` 会将弹簧/贝塞尔 Token 编译为 `KeyFrameAnimation` 或 `ExpressionAnimation` 并挂载至 DWM 合成 Visual 上，此后 CPU 停止逐帧计算。
* **降级/软渲染路径 (Software Path)**：若能力分发器检测到系统环境不支持硬件合成器，`SoftwareDispatcher` 将接管更新，在 CPU 侧通过 `MotionSystem::Advance(dt)` 逐帧计算并驱动 UI 线程重绘。

---

## 五、 API 使用示例

### 5.1 C++ 核心求值示例 (`spring.h`)

```cpp
#include "core/spring.h"

// 初始化一个临界阻尼弹簧 (stiffness = 300.0, damping = 25.0)
FluidMotion::SpringSolver spring(300.0f, 25.0f);

// 在某一帧计算当前位移与瞬时速度
float current_value = 0.0f;
float current_velocity = 0.0f;
float target_value = 100.0f;
float dt = 0.0083f; // 120Hz 单帧时间

spring.Update(current_value, current_velocity, target_value, dt);

```

### 5.2 C# 伪代码示例 (在 WinUI 3 中使用)

```csharp
using FluidMotion.Engine;

// 在 WinUI 3 页面中初始化动效控制器
var animator = new MotionAnimator(element);

// 加载从 Figma 编译的 Token 动画
animator.ApplyToken("card-expand-smooth");

// 触发带有阻尼重定向的交互动画 (自动处理 Last-Wins 所有权抢占)
AnimationHandle handle = animator.StartAnimation(
    targetScale: 1.05f, 
    currentVelocity: existingVelocity
);

// 如有需要，中途手动强行吸附到终点
if (userRequiresAccessibility) {
    handle.SnapToEnd();
}

```

---

## 六、 Motion.json Schema 说明

Figma 导出的 `motion.json` 结构规范如下：

```json
{
  "$schema": "[https://fluidmotion.dev/schemas/motion-token.v1.json](https://fluidmotion.dev/schemas/motion-token.v1.json)",
  "version": "1.0.0",
  "tokens": {
    "card-expand-smooth": {
      "type": "spring",
      "properties": {
        "stiffness": 250.0,
        "damping": 22.0,
        "mass": 1.0
      },
      "easing": {
        "type": "cubic-bezier",
        "points": [0.25, 1.0, 0.5, 1.0]
      },
      "durationMs": 350
    }
  }
}

```

---

## 七、 性能与测试要求

### 7.1 帧预算分配 (基于刷新率比例)

* **UI / 宿主业务线程**：$\le 12\%$ 周期预算（120Hz 下稳态 $\le 1.0\text{ms}$，启动 $\le 2.0\text{ms}$）*(估计)*。
* **Rive 渲染与合成线程**：$\le 35\%$ 周期预算（120Hz 下 $\le 3.0\text{ms}$）*(估计)*。
* **GPU 硬件渲染**：$\le 35\%$ 周期预算（120Hz 下 $\le 3.0\text{ms}$）*(估计)*。

### 7.2 测试与验证门槛

* **Linux 算法单机测试 (已达成)**：`timeline` (419 passing), `tokens` (617 passing), `controller` (236 passing)。
* **专用物理机 CI (待集成)**：所有性能指标在固定的 Windows 物理测试机（固定 CPU/GPU 频率）上运行。
* **门槛标准**：
* **P95 回退门禁**：相比 Main 分支基线，CPU 计算耗时相对回退 $> 10\%$ 时自动拦截 PR。
* **P99 丢帧率**：每千帧丢帧数（Frame Duration $> 1.5 \times \text{Interval}$）必须 $< 1.0$。
* **视觉回归**：黄金帧自动化比对，SSIM $> 0.99$。



---

## 八、 技术验证与实施清单 (Milestones)

本项目计划总周期约 **30–34 周**，由约 **6 名全职人员**推进，划分为 6 个阶段：

* **M0 (第 2 周)**：完成 PoC 验证（Rive 接入合成表面、`GetPointerPositionPropertySet` 可用性、`ALLOW_TEARING` 撕裂测试）。
* **M1 (第 10 周)**：核心引擎完成（C++ 数学库、弹簧闭式解、时间线与重定向控制器落地，Linux 侧单机测试通过）。
* **M2 (第 16 周)**：工具链与语言绑定打通（Figma 插件、Token 编译器、WinMD/C# 封装、桌面热重载 Previewer）。
* **M3 (第 22 周)**：能力分发器（`IDispatcher`）、合成器降级机制接入，并**接入第一个友好 WinUI 3 应用试点**。
* **M4 (第 28 周)**：性能 CI 部署、双应用试点验证（接入第二个较重/较老的 WinUI 3 应用，不包含正式 WPF 支持）。
* **M5 (第 32–34 周)**：1.0 GA 正式版发布与长稳压测。

---

## 九、 已知限制与待真机验证项 (更新)

1. **Windows DWM / 合成器对接待验证**：当前 `spring.h`、`bezier.h`、`timeline.h`、`tokens.h` 与 `controller.h` 均已在 Linux 单机环境完成纯逻辑测试（累计 1272 tests 全通过），**尚未在真实的 Windows DWM 环境与 `Windows.UI.Composition` API 进行挂载对接** *(待真机验证)*。
2. **指针跟随采样延迟**：普通鼠标指针（PointerMove）受限于系统 Win32 消息分发与 DWM 采样机制，物理上存在 **0.5 ~ 1.5 帧的采样延迟** *(待真机验证)*，复杂变形需配合设计端阻尼（Damping）掩盖。
3. **HWND 裸宿主限制**：首发提供的 HWND 参考宿主仅适用于无遮挡的矩形装饰区域，不支持复杂的 Airspace 混合嵌套及 UIA 无障碍读屏树。
4. **硬件降级与 Timer 精度**：当能力分发器回退至 `SoftwareDispatcher` 时，在 Windows 下依赖高精度 Timer（如 `timeBeginPeriod` 或 `CreateTimerQueueTimer`）的 CPU 波动与功耗表现 *(待真机验证)*。
