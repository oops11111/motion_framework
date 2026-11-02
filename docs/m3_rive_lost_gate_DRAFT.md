# M3-RIVE-LOST 门槛闸门（m3_rive_lost_gate_DRAFT.md）

> **SUPERSEDED_BY_LOCKED**：`docs/m3_rive_lost_gate_LOCKED.md`
> GATE_BODY_SHA256=`753c0a6c82f06c81cf741eecba278b926127771befa2a51b515a925a7873f27d`
> LOCKED_AT=2026-10-29 12:31:48 AWST
> 本 DRAFT 保留不删；以 LOCKED 为准。

STATUS=DRAFT（**NOT LOCKED**；本文件**不授权采数**）
LOCKED_AT: （LOCK 时填）
GATE_BODY_SHA256: （LOCK 时算；约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希）

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> 起草：Claude（Claude Code 接手会话），2026-10-29。依据：`docs/m3_plan_DRAFT.md` §7 第 2 张票（WP1，plan 任务 8）；M3 验收门槛第 1 条“Rive 表面支持……设备丢失恢复”；`docs/notion_m3_rive_dpi_next.md`（主推 M3-RIVE-LOST）。
> 分支：`claude/m3-rive-lost`，从 `claude/m3-rive-dpi`（`2bb1f14`）拉出。原因：设备丢失后的重建要复用 DPI 票的重建路径（`ApplyResize` 中重建离屏纹理、Rive 渲染目标和标记 RTV）。DPI 代码进 main 走 PR #6 评审，与本票流程分开。

## G0 硬边界
- 不改 `composition.h`；不重开 S1/R-3/R-2；禁影子 IV；不解冻 Bv；PERF-CI 仍 HOLD。
- 未 LOCK 且 APPROVED_* 为空时禁止采数。成功 ≠ 采用 ≠ 默认可合入 main。
- **不改系统的 TDR 注册表项**（`TdrDelay`、`TdrLimitCount` 等），也不改显卡驱动设置。
- DPI 票中执行方本机的 csproj 补丁（`WindowsPackageType=None` + `WindowsAppSDKSelfContained=true`）：派工单会**单独**写明这是本机启动的前置条件，**不进仓库，不回灌主线**。

## G1 要验证什么
Rive 渲染所用的 D3D11 设备被移除后（真实 GPU 超时恢复，即 TDR）：
1. 渲染线程自己发现设备已丢失（`DXGI_ERROR_DEVICE_REMOVED` / `DEVICE_RESET`，来自 `BeginDraw`、Rive flush 或每帧检查的 `GetDeviceRemovedReason`）；
2. 不重启进程，自动完成恢复：新建 D3D11 设备（特性级别 11_1）→ 新建 Rive 渲染上下文 → 从内存中的 .riv 字节重新导入 → 重建离屏纹理和渲染目标 → 通过 `ICompositionGraphicsDeviceInterop::SetRenderingDevice` 把新设备交给原来的 `CompositionGraphicsDevice`；原有合成表面和 visual 保持不变，只是重画内容；
3. 恢复后画面正确、渲染预算仍满足 plan.md（渲染线程 ≤0.35·T、GPU ≤0.35·T）；
4. 多次丢失后不泄漏（旧设备和旧 Rive 上下文确实释放了）。

## G2 判据（每个会话；数字为建议值，见 K2）
| 判据 | 门槛 |
|---|---|
| ① 注入生效 | 每个会话 K4 规定次数的注入，每次都被渲染线程检测到（`lost_detected` 次数 = 注入次数，带 removed reason HRESULT） |
| ② 自动恢复 | 每次丢失都恢复成功（`recover_ok`），进程不重启；单次恢复耗时（检测到丢失 → 新设备上第一帧提交成功）**≤ 5000 ms**，超过判 FAIL；最大值和 P95 **记录**，参考目标 ≤ 1000 ms。Rive 的着色器是异步编译的，所以“第一帧提交成功”时画面可能还是空的；画面是否正确由 ⑤ 的截图判断 |
| ③ 恢复后稳定 | 每次恢复后的段（恢复后先等 500 ms 再开始，段长 8 s）：`draw_failures=0`（丢失期间的失败单独计数，不算在内）；`device_removed_hr=0`；帧数 ≥ 300 |
| ④ 恢复后预算 | 每次恢复后的段：CPU P95 ≤ 0.35·T，GPU P95 ≤ 0.35·T，GPU 丢弃样本 ≤ 10% |
| ⑤ 画面正确 | 每次恢复后的窗口截图不是空白，且与新设备上导出的参考图目视一致（协调目视确认，写入结题） |
| ⑥ 丢帧窗口 | 丢失前最后一帧到恢复后第一帧的间隔 `gap_ms`，换算为缺失帧数 = round(gap_ms / T) − 1（**记录**） |
| ⑦ 内存 | 最后一段末尾的进程专用内存，比第一次注入前的段末尾多出 ≤ 50 MB（**记录**；超出列为 M3 待查项，不判 FAIL）；同时记录我们最后一次 Release 旧设备后剩下的引用数（**记录**；非 0 可能是合成器还没放手，持续增长才算泄漏线索） |

- **会话**：≥3 个独立进程（K4）。
- **PASS** ⇔ ①②③④⑤ 每个会话都满足；⑥⑦只记录。
- **FAIL**（不是 INVALID）：进程在注入后崩溃或被 Rive `abort()`（summary 里出现 `verdict=FAIL_ABORT`，或矩阵里是 `CRASHED_NO_VERDICT`）；恢复超过 5 s；恢复后段不满足 ③④。这些都是本票要找的结论，见 K7。
- **INVALID**：缺产物；hash 不符；未授权采数；改 composition.h；注入命令执行了但探针 30 s 内没检测到丢失（该次作废，runner 停止后续注入）；系统蓝屏或显示器 30 s 内没恢复（`INVALID_SYSTEM`，当天停止，见 K5）；用 `--m3-lost-sim` 跑出的数据（`SIM_RECORDED`）。

## G3 裁决（pidan：K1–K6 全部按建议；K7 待裁决）
| # | 问题 | 裁决（= 原建议） | 理由 |
|---|---|---|---|
| **K1** | 怎样让设备丢失 | **以真实 TDR 为准**：管理员运行 `dxcap -forcetdr`（Windows“图形工具”可选功能自带）。另外在探针里留一个**模拟丢失**开关（`--m3-lost-sim`：渲染线程把当前设备当作已丢失，走完整的恢复路径），只用于本地冒烟和操作员熟悉流程，**不计入判据** | D3D11 没有像 D3D12 `RemoveDevice` 那样的进程内接口；只有真实 TDR 能同时测到驱动、DWM 和合成器三方的行为。模拟开关便宜、可重复，能在采数前把代码路径先跑一遍 |
| **K2** | 门槛取值 | 如 G2：恢复成功是硬判据；单次恢复上限 5 s，P95 只记录（参考目标 1 s）；恢复后预算沿用 DPI 票的 0.35·T；内存 50 MB 只记录 | plan 写的是“恢复时间和内存增长在记录内”，所以只给一个防止挂死的上限。TDR 本身让整个显示停顿 1–3 s，用户看到的停顿主要由系统决定；内存上限比 DPI 票（20 MB）宽，因为要重建整个设备和 Rive 上下文 |
| **K3** | 用哪台机器 | **DESKTOP-IJI1KJG**（Win11、可提权、RX 7800 XT，M0-B 和 DPI 票的那台）。采数前确认 `dxcap -?` 能运行；没有就在“设置 → 可选功能”装“图形工具” | 唯一能提权的机器；构建和运行环境现成 |
| **K4** | 注入次数和间隔 | **3 个会话 × 每会话 2 次注入**；同一会话两次注入间隔 ≥ 45 s；会话之间冷却 ≥ 120 s；全天合计 ≤ 8 次 | Windows 默认 60 s 内 TDR 超过 5 次会直接蓝屏（`TdrLimitCount`/`TdrLimitTime`），间隔 45 s 保证 60 s 窗口内最多 2 次；每会话 2 次能测到“恢复后再丢一次”的情形（旧对象没清干净时常在第二次暴露） |
| **K5** | 整机风险怎么控制 | 采数前关掉其他用 GPU 的程序，保存手头工作；runner 每次注入前检查距离上次注入 ≥ 45 s，否则拒绝执行；出现蓝屏、显示器 30 s 内没恢复、或驱动报错弹窗，立即停止，记 `INVALID_SYSTEM`，当天不重试，回报事件查看器“系统”日志里的 4101（显示驱动已停止响应并已恢复）和 BugCheck 记录 | TDR 会重置整块 GPU，影响机器上所有程序；由 runner 控制间隔，比依赖操作员手速可靠 |
| **K6** | 恢复后 Rive 动画从哪里接着播 | **允许从头播放**（新状态机从 t=0 开始），记录 `resume_mode=restart`；“恢复到丢失前的播放位置”记为 WP1 产品化待办，不在本票判据内 | 新导入的状态机没有可靠的“跳到某时刻”接口，用一次大步长 `advance` 补时间，不同 .riv 的结果不一定一致；本票验证的是“能恢复并画对”，播放连续性留给产品层 |
| **K7**（待裁决） | 钉住版本的 Rive D3D11 后端在设备丢失时不安全：`render_context_d3d_impl.cpp` 的 `onMap` 不检查 `Map` 的返回值就写入 `pData`；`d3d.hpp` 的 `VERIFY_OK` 遇到任何失败的 D3D 调用都直接 `abort()`。一帧正在绘制时设备被移除，进程可能崩溃。要不要先给 Rive 打补丁？ | **建议：本票不改 Rive**，只在我们这一侧防护：① 每帧调用 Rive 前检查设备状态，丢失期间完全不调 Rive；② MSVC 下用 SEH 只捕获“设备已移除时发生的访问冲突”，当作一次丢失处理，其他异常照常崩溃；③ Rive 一旦 `abort()`，DLL 在 summary 里追加 `verdict=FAIL_ABORT`。真机上若仍崩溃，本票判 FAIL 并留下证据，下一张票专门做 Rive 补丁（`onMap` 检查返回值、`VERIFY_OK` 改为报错不退出），同时向 Rive 上游报告 | 本票的目的就是看钉住的 Rive 能不能扛住真实的设备丢失；先打补丁会把这个结论藏起来。我们这一侧的防护把崩溃窗口缩小到“一帧 CPU 时间内恰好发生 TDR”，大约每次注入 10% 以内（CPU P95 ≤ 0.1·T 时），6 次注入里出现一次的可能不小，所以要先说清楚：崩溃 = FAIL（有价值的结论），不是 INVALID |

## G4 实现（已写，未在真机运行）
| 文件 | 内容 | 本地验证 |
|---|---|---|
| `m0/native/m0rive/m0rive.{h,cpp}` | 新增 `m0r_attach_graphics_device`（QI `ICompositionGraphicsDeviceInterop` 4AFA8030-BC70-4B0C-B1C7-6E69F933DC83）、`m0r_get_lost`、`m0r_simulate_lost`（K1 模拟开关）、`m0r_set_abort_note`（K7 ③）。渲染线程每帧开头检查 `GetDeviceRemovedReason`（以及上一帧返回的 `DEVICE_REMOVED/RESET/HUNG`）；丢失期间不调 Rive，每 ≥ 50 ms 尝试一次 `RecoverDevice`：新建设备 → `SetRenderingDevice` → 释放旧的 Rive 对象、纹理、视图、查询和上下文 → Release 旧设备并记录剩余引用 → 新 Rive 上下文、从内存重新导入 .riv（K6：从头播放）→ 按当前尺寸重建离屏纹理和渲染目标。新设备上第一帧提交成功时记一条事件（removed reason、恢复耗时、`gap_ms`、丢失期间的空转次数、尝试次数、SEH 捕获次数）。GPU 计时查询在设备已移除时不再等待（否则会把检测推迟 2 s）。`m0r_create` 改为复用同一组建设备 / 建 Rive 的函数，行为不变。交换链路径不做恢复（F1） | MinGW 对照钉住的 Rive 头文件（`-Wall -Wextra -Wshadow`）语法检查：本文件 0 警告 0 错误。SEH 部分只在 MSVC 下编译，MinGW 查不到，已人工复查 |
| `MotionSoftDemo/M3LostProbe.cs`（`--m3-lost-probe`；`--m3-lost-sim` 为模拟） | 表面 400 DIP；先测基线段 seg0；每次注入：写 `m3lost_s<n>_inject<k>.req` 请 runner 执行 `dxcap -forcetdr` → 等检测（最多 5 min）→ 写 `inject<k>.detected=1` → 等恢复（最多 30 s，超过 5 s 判 `recover_ok=0`）→ 等 500 ms → 8 s 统计段、截图、在新设备上导出参考图。summary 汇总 `lost_detected`、`recover_ok`、`recover_max_ms`、`frames_missed`、`budget_ok`、`stability_ok`、`private_mb_delta`。模拟模式的文件前缀是 `m3lostsim_`，verdict 是 `SIM_RECORDED` | 对照 WinAppSDK 2.5.1 编译，0 个 C# 错误 |
| `MotionSoftDemo/MainPage.xaml.cs` +12 行；`m3/mainpage_m3lost_hook.patch` | 入口（CRLF 不变）。补丁是在**已打 DPI 补丁**的 MainPage 上做的 | 补丁由 `git diff` 生成 |
| `m3/run_m3_lost.ps1` | 检查管理员权限和 `dxcap.exe`；记录 TDR 注册表值（只读）；按会话启动应用；响应截图请求；响应注入请求：强制两次注入间隔 ≥ 45 s、会话之间 ≥ 120 s、全程 ≤ 8 次；注入后 30 s 内探针没报告检测就停止全部注入；每会话收集系统日志 4101 事件；全程结束时统计 BugCheck 事件；输出会话矩阵。`-Simulate` 跑模拟模式（不需要管理员和 dxcap） | 本环境没有 PowerShell，只做人工复查 |

## G5 禁令
不改 composition.h；不重开 S1/R-3/R-2；禁影子 IV；不解冻 Bv；不改 TDR 注册表；本 DRAFT ≠ 授权采数；成功 ≠ 采用；PERF-CI 仍 HOLD。

## G6 ONELINE 模板
M3-RIVE-LOST | gate=<hash> @ <LOCKED_AT> | approved=<who>@<when> | host=<名> | inject=dxcap-forcetdr | sessions=<n> | lost_detected=<s1/s2/s3> | recover_ok=<s1/s2/s3> | recover_max_ms=<…> | budget_ok/stability_ok=<…> | sharp=<目视结论> | frames_missed=<…> | mem_delta_mb=<…> | event4101=<…> | ticket=<PASS/FAIL/INVALID> | 不改composition.h；成功≠采用
