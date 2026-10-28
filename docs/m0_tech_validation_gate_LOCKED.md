# M0 技术验证门槛闸门（m0_tech_validation_gate_LOCKED.md）

STATUS=LOCKED
LOCKED_AT: 2026-10-26 21:38:45 AWST（Australia/Perth）
GATE_BODY_SHA256: 942d6965dae1aa41f3b853923d08a11aac9714d60018888a5c486bfde5809768
> GATE_BODY_SHA256 计算约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。本行写入后不得再改 body。

**本 LOCK 不授权采数。** 须另有 approved 留证（APPROVED_* 非空）后，才可派 workbuddy 采数。构建（build_m0.ps1）不采数，可先行。

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> 起草：Claude（Claude Code 接手会话），2026-10-26；DRAFT：`docs/m0_tech_validation_gate_DRAFT.md`。依据：plan.md §5 M0 验收门槛；summary.md §三 实施门槛；disputes.md「待技术验证确认的事项」；评估报告建议 1。
> 分支：`claude/m0-tech-validation`。真机结果只提交到这个分支，结果评审通过后再合入 main。
> 前序：指针属性集 #32（`docs/workbuddy_soft_demo.md` 派题 #32）为 PARTIAL：API 可调用、表达式可绑定；因当时不允许合成输入，真实指针跟随延迟没有测。

## G0 硬边界
- 不改 `composition.h`（`a55d4952…e418`）。M0 探针不调用弹簧、重定向或影子 IV 相关代码。
- 不重开 S1、R-3、R-2；不解冻 Bv；PERF-CI 仍 HOLD；Δv″ 仍挂账。
- 未 LOCK 且 APPROVED_* 为空时禁止采数。
- 成功 ≠ 采用。M0 PASS 只表示 Rive 路径可以继续投入，不构成发版依据。

## G1 验证项

| 项 | 内容 | 本机（60Hz）可做 | 判据类型 |
|---|---|---|---|
| **M0-1a** | Rive（rive-runtime D3D11 渲染器）渲染进 `CompositionDrawingSurface`：先画到独立的 B8G8R8A8 纹理，再拷进表面图集 | 是 | **PASS/FAIL** |
| **M0-1b** | 同样内容走合成交换链（`ICompositorSwapChainInterop::CreateCompositionSurfaceForSwapChain`） | 是 | 记录 |
| **M0-2** | 拖动错位：元素（AnimBox）和 Rive 表面用同一条指针表达式跟随，比较两者相对光标轨迹的滞后差 | 60Hz 预演；**120Hz 需第二台机器** | 60Hz 记录；120Hz 时 PASS/FAIL |
| **M0-3** | 指针属性集真实跟随（补 #32 未测部分） | 是（需 D1） | 记录 |
| **M0-4** | 合成交换链能否用 `ALLOW_TEARING`：CheckFeatureSupport、创建、Present 的 HRESULT，以及 PresentMon 的 PresentMode | 是 | 记录 |
| **M0-5** | 实测管线延迟：注入光标 → WGC 首帧越过半程。分两条路径比较：表达式路径和 UI 线程写入路径 | 是（需 D1） | 记录 |

## G2 判据
- **G2-M0-1（M0-1a PASS 的全部条件）**：
  1. 创建设备、创建 CompositionGraphicsDevice、表面 interop QI、BeginDraw/EndDraw 的 HRESULT 全部成功；
  2. 每个会话 M0-1a 阶段至少 1000 帧，`draw_failures=0`，结束时 `device_removed_hr=0`；
  3. 渲染线程 CPU P95 ≤ 0.35·T，GPU P95 ≤ 0.35·T（T 为实测刷新间隔；对应 plan.md 的渲染线程和 GPU 预算）；
  4. 截图（`m0_s*_screen_m0_1a.png`）中表面区域确实显示 Rive 内容，由协调对照参考图 `m0_s*_rive_ref_t0500.bgra` 目视确认并写入结题。
- **G2-M0-2（60Hz 预演，只记录）**：报告 `misalign_ms` 和 `misalign_px_at_drag_speed`。建议的 120Hz 正式门槛（M0-B 时 LOCK）：两条路径滞后中位数之差的绝对值 ≤ 2 ms，即落在同一个合成帧内。
- **G2-M0-345（只记录）**：M0-3/5 报告每条路径的 `steps_hit`、延迟 P50/P95，以及时钟偏移检查值；M0-4 报告全部 HRESULT、present 节奏，有 PresentMon 时报告 PresentMode。
- **会话**：≥3 个独立进程（与前序票一致），另外跑 1 次 M0-4。
- **INVALID**：缺产物；hash 不符；未授权采数；改 composition.h；构建工具链失败（记为 `BLOCKED_TOOLCHAIN`，不算 FAIL）。
- **Go / No-go**：M0-1a PASS，且 M0-2（60Hz）没有出现层间错位 → Rive 路径继续，M0-B 在 120Hz 上复核。M0-1a 因 interop 或渲染器本身不可行而 FAIL → 按 summary.md 启用 Skia 备选方案，工期 +4–8 周，交协调裁决。

## G3 裁决（协调 2026-10-26：D1–D7 全部按建议）

| # | 问题 | 裁决（= 原建议） | 理由 |
|---|---|---|---|
| **D1** | 是否允许用 `SendInput` 合成指针输入（M0-2、M0-3、M0-5） | **允许，仅限 M0**；另可加一次人工拖动作为对照 | 不注入就测不了真实指针延迟（#32 正是卡在这里）。SendInput 进入与真实鼠标相同的系统输入队列，只少了 USB/HID 这一段（约 1–8 ms），报告中注明“注入→WGC” |
| **D2** | 只有 60Hz 机器，120Hz 的要求怎么处理 | **拆成 M0-A 和 M0-B**：M0-A 现在在本机做 M0-1、M0-3/4/5，以及 M0-2 的 60Hz 预演；M0-B 等第二台 ≥120Hz、Win11 的机器到位后，复测 M0-2 和 M0-4，并试 Win11 的 CompositionTexture 路径 | 不让第二台机器阻塞 Rive 可行性的判断；120Hz 的结论仍以 M0-B 为准 |
| **D3** | Rive 怎么接入 | **rive-runtime C++ D3D11 渲染器**，钉在 `runtime-v0.1.545`（commit `2a043c31`，2026-10-10，MIT）。主路径是“独立纹理 + 拷进 CompositionDrawingSurface”，交换链是第二路径 | Rive 要求渲染目标与纹理同尺寸，而绘图表面返回的是图集加偏移，所以只能先离屏渲染再拷贝；拷贝不能跨 RGBA/BGRA，离屏纹理必须是 B8G8R8A8 |
| **D4** | 实验室机器的构建工具链 | 用 MSVC（`--toolset=msc`），**不装 clang-cl**（安装需要管理员）；需要 Git for Windows 的 bash，并允许访问 github.com（Rive 构建脚本会自动下载 premake） | 本机没有管理员权限（logman 被拒）。如果构建失败，记为 BLOCKED_TOOLCHAIN，不判 FAIL |
| **D5** | M0-1a 的预算门槛用 0.35·T 是否合适 | **采用**（与 plan.md 的渲染线程和 GPU 预算一致） | 60Hz 下为 5.8 ms；不另造新数字 |
| **D6** | M0-2 正式门槛（120Hz） | 两条路径滞后中位数之差 ≤ 2 ms（M0-B 时 LOCK） | 两层都由合成器同帧驱动时，差值应接近 0；2 ms 能容纳 WGC 时间戳的抖动，又远小于 120Hz 的一帧（8.3 ms） |
| **D7** | 流程 | 真机采数照旧走 DRAFT → LOCK → APPROVED；代码改动和真机结果都在 `claude/m0-tech-validation` 分支上，评审后合入 | 与现有规则一致；按评估报告的建议，纯代码部分不另开闸门 |

## G4 交付物（已在分支上）

| 文件 | 内容 | 本地验证情况 |
|---|---|---|
| `m0/native/m0_tear_probe.cpp` | M0-4 原生探针：Win32、D3D11、DXGI、DirectComposition | MinGW 交叉编译并链接通过，无警告 |
| `m0/native/m0rive/m0rive.{h,cpp}` | M0-1 Rive 表面/交换链 DLL（C ABI）：渲染线程、CPU/GPU 计时、M0-2 标记块、参考图导出 | 在钉住的 Rive 头文件下（含默认的 text/layout/canvas 宏）用 MinGW 做语法检查，通过；**未链接**（需要在 Windows 上构建 Rive） |
| `MotionSoftDemo/M0Probe.cs` + `MainPage.xaml.cs` 加 11 行入口 | WinUI 宿主：interop、表面、交换链、SendInput、WGC 采样 | 用桩替代 MainPage 其余部分，对照 WinAppSDK 2.5.1 编译通过，0 错误 0 警告；**未在 Windows 上运行** |
| `m0/build_m0.ps1` / `m0/run_m0.ps1` | 构建脚本 / 会话运行脚本 | **未做语法检查**（本环境没有 PowerShell），已人工复查 |

已在本地核实、写进代码的事实：
- WinAppSDK 2.5.1（InteractiveExperiences 2.1.9）的 interop IID 与系统 `Windows.UI.Composition` 的**不同**：ICompositorInterop = `FAB19398-…`，ICompositorSwapChainInterop = `FC084699-…`（交换链方法在这个接口上），ICompositionDrawingSurfaceInterop = `2D6355C2-…`。
- Rive D3D11 要求特性级别 11_1；GTX 970 满足。
- 测试素材用 rive-runtime 仓库里的 `Knight_square_2.riv`（sha256 `cec7ff27…ddb8`），由构建脚本取出并校验，不提交到本仓库。

## G5 禁令
不改 composition.h；不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；本 DRAFT ≠ 授权采数；M0 PASS ≠ 采用；PERF-CI 仍 HOLD。

## G6 ONELINE 模板
M0-A | gate=<hash> @ <LOCKED_AT> | approved=<who>@<when> | D1=<是/否> | sessions=<n> | m0_1a=<PASS/FAIL/BLOCKED_TOOLCHAIN> | m0_1b/m0_2@60/m0_3/m0_4/m0_5=RECORDED | go=<Rive继续/Skia备选/待定> | 不改composition.h；成功≠采用
