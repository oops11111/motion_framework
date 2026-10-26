基于目前的文档体系（`api.md`、`usage.md`、`engineering.md` 等）与已落地的算法模块（`spring`、`bezier`、`timeline`、`tokens`、`controller`），以下是针对你提出的四个维度的深度审视与系统规划：

---

### 一、 现有文档体系（API / Usage / Spec）的模糊与矛盾点审视

1. **`Timeline` 与 `MotionSystem` 的职责交集边界模糊**
* *模糊处*：`timeline.h` 负责复杂的结构化编排（Sequence/Parallel/Stagger），而 `controller.h` 中的 `MotionSystem` 也负责管理属性动画。两者的底层驱动关系在文档中没有明确说明：`MotionSystem` 是直接调用 `Spring`，还是将 Token 编译为 `Timeline` 后再交由 `IDispatcher` 下发？
* *修正建议*：明确 `MotionSystem` 为**高层交互控制器**（处理瞬时抢占、重定向与属性所有权），其底层在遇到复杂组合动效时，可选择将 `Token` 编译为 `Timeline` 并通过 `IDispatcher` 批量分发。


2. **`IDispatcher` 接口与 Windows 异步特性的契约缺失**
* *矛盾处*：`IDispatcher::DispatchAnimationStart` 传入了 `AnimationSpec`，但在 Windows.UI.Composition 中，很多动画属性的启动是异步的（如创建 Visual、绑定 Animation）。接口中缺乏错误码返回或异步回调机制。
* *修正建议*：在 `IDispatcher` 中增加 HRESULT 或错误状态反馈，明确其在跨语言（C++ 到 WinMD/C#）调用时的异常边界。



---

### 二、 WinUI 3 + Composition 真机落地的最小可行切片 (MVP) 文件与接口设计

要在真实 WinUI 3 环境中跑通第一个弹簧动画，首发 MVP 切片不需要庞大的架构，只需以下核心文件与接口：

```text
/src/mvp/
  ├── WinUI3Dispatcher.h/.cpp       # 实现 IDispatcher 接口，对接 Compositor
  ├── CompositionAnimator.h/.cpp    # 绑定 WinUI 3 UIElement 与 MotionSystem
  └── MotionPlugin.cs               # C# 端入口与 WinMD 互操作绑定

```

#### 核心接口定义：

1. **`WinUI3Dispatcher` (实现 `motion::IDispatcher`)**：
```cpp
class WinUI3Dispatcher : public motion::IDispatcher {
public:
    explicit WinUI3Dispatcher(winrt::Windows::UI::Composition::Compositor compositor);
    void DispatchPropertyUpdate(uint64_t target_id, motion::PropertyType prop, float value) override;
    void DispatchAnimationStart(uint64_t target_id, motion::PropertyType prop, const motion::AnimationSpec& spec) override;
    void DispatchAnimationCancel(uint64_t target_id, motion::PropertyType prop) override;
private:
    winrt::Windows::UI::Composition::Compositor m_compositor;
};

```


2. **`CompositionAnimator` (桥接层)**：
负责将 C# 传入的 `UIElement` 转换为底层 `winrt::Windows::UI::Composition::Visual`，并管理其关联的 `MotionSystem` 生命周期。

---

### 三、 技术验证 M0 逐日清单与 Skia 备选触发条件

#### 📅 M0 (第 2 周) 逐日技术验证清单：

* **Day 1**：搭建 WinUI 3 (Windows App SDK) 空白项目，配置 C++/WinRT 静态库依赖，成功链接已测通的 `spring.h` 与 `controller.h`。
* **Day 2**：实现 `WinUI3Dispatcher`，验证能否通过 C++ 代码操控 WinUI 3 按钮的 `Scale` 属性。
* **Day 3**：集成 Rive C++ Runtime，测试在 WinUI 3 页面中创建一个 `CompositionDrawingSurface` 并成功渲染一个基础 `.riv` 矢量动画。
* **Day 4**：编写 `GetPointerPositionPropertySet` 测试用例，测试鼠标悬停时 `ExpressionAnimation` 的响应延迟。
* **Day 5**：开启 DXGI `ALLOW_TEARING` 标志，在支持 VRR（如 G-Sync/FreeSync）的显示器上进行快速拖拽测试，肉眼与工具观察是否存在画面撕裂。

#### 🛑 Skia + DComp Swapchain 备选方案触发条件：

如果在 M0 阶段出现以下任意一种情况，**立即放弃纯 Windows.UI.Composition 路径，切入备选方案（Option B: Skia + DComp Swapchain）**：

1. **驱动兼容与设备丢失**：在测试机上集成 Rive C++ Runtime 到 `CompositionDrawingSurface` 时，频繁触发 `DXGI_ERROR_DEVICE_REMOVED` 或 D3D11 互操作渲染崩溃，且官方无解。
2. **合成表面与矢量撕裂**：由于 WinUI 3 内部 SwapChain 呈现管线限制，Rive 独立渲染线程与 DWM 合成线程之间出现了无法通过 `ALLOW_TEARING` 或 `WaitForVBlank` 消除的持续性视觉闪烁与微小错位（Visual Jitter）。

---

### 四、 3 个值得继续讨论的开放问题

1. **跨语言内存生命周期与循环引用（C++/WinRT 与 C# GC 的交界）**：
当 C# 业务层持有 `AnimationHandle`（内部弱引用 C++ 的 `MotionSystem`），而 C++ 引擎通过 `IDispatcher` 回调 C# 对象时，如何彻底避免闭包捕获导致的跨语言内存泄漏（Memory Leak）或悬挂指针（Dangling Pointer）？
2. **后台最小化与生命周期休眠策略**：
当 WinUI 3 应用被用户最小化或切到后台时，系统独立合成器（DWM）虽然会暂停非必要渲染，但引擎内部的弹簧/时间线 Tick 应该如何响应？是否需要主动暂停计算以将后台 CPU 占用降为绝对的 0%？窗口恢复时如何校准时间戳？
3. **异构高刷与多屏 DPI 动态迁移时的物理量重校准**：
当一个带有弹簧惯性的动效窗口被用户从 4K 144Hz 屏幕（缩放 150%）拖拽到 1080P 60Hz 屏幕（缩放 100%）时，时间步长 `dt` 的剧烈突变（如从 6.9ms 跳变为 16.6ms）是否会导致弹簧物理方程产生瞬间的数值振荡或速度跳跃？