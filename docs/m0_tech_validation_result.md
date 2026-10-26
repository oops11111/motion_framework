# M0-A 结题结果（m0_tech_validation_result.md）

STATUS: CLOSED
TICKET: M0-A-PASS（M0-1a PASS 3/3；其余各项 RECORDED；G2-M0-1 第④条以截图确认）
DEVIATION_ACK: pidan 于 2026-10-26 23:59:30 AWST 指示「M0-B开新分支在win11上跑，开pr合并」，据此视为接受第④条改以截图确认
WRITTEN_AT: 2026-10-26 23:50:32 AWST（Australia/Perth）
GATE: `docs/m0_tech_validation_gate_LOCKED.md`，GATE_BODY_SHA256=`942d6965dae1aa41f3b853923d08a11aac9714d60018888a5c486bfde5809768`（结题时复算：MATCH）
APPROVED: `docs/m0_tech_validation_approved.md`，pidan @ 2026-10-26 21:43:10 AWST
DATA: 分支 `claude/m0-tech-validation` 的 commit `fac309a5`（workbuddy，2026-10-26 23:42 +0800），目录 `m0/results/`
MACHINE: DESKTOP-K93N3P8，SMBIOS UUID `03000200-0400-0500-0006-000700080009`；Win10 19045；GTX 970；1280×1024，60.020Hz；缩放 100%
composition.h: 运行前后 sha256 均为 `a55d4952…e418`，未改

## G6 ONELINE
```
M0-A | gate=942d6965…9768 @ 2026-10-26 21:38:45 AWST | approved=pidan@2026-10-26 21:43:10 AWST | D1=是 | sessions=3 | m0_1a=PASS(3/3) | m0_1b/m0_2@60/m0_3/m0_4/m0_5=RECORDED | go=Rive继续（M0-B 120Hz 复核） | 不改composition.h；成功≠采用
```

## 执行偏离（runner / 构建，不涉及判据）
- `build_m0.ps1`（workbuddy 修改）：只改了工具链环境，包括 MSVC 源文件按 UTF-8 编译（`-utf-8`，避免中文代码页下 C4819 被 /WX 升级为错误）、优先使用 Rive 自带的新版 DirectX-Headers（本机 SDK 18362 过旧）、加链 `dxguid.lib`、为 MSYS 构建配 PATH 和 python 垫片。**Rive 的特性宏、/MT、`m0rive.cpp` 都没改**，符合 D4。
- `run_m0.ps1`（workbuddy 修改）：本机 MotionSoftDemo 是 MSIX 打包应用，改为用 `dotnet run --launch-profile` 启动；会话号和注入开关通过 launchSettings 和 flag 文件传入；每个会话校验 summary 中的 `session=` 与实际会话号一致。测量逻辑没有改动。
- PresentMon 进程启动了，但没有产出 CSV（多半是没有管理员权限）。因此 M0-4 没有 PresentMode 数据。

## 结果

### M0-1a Rive → CompositionDrawingSurface（PASS 3/3）
| 判据 | s1 | s2 | s3 | 门槛 |
|---|---|---|---|---|
| ① create / graphics device / interop QI / start 的 HRESULT | 全 0 | 全 0 | 全 0 | 全部成功 |
| ② 帧数 | 1176 | 1175 | 1174 | ≥1000 |
| ② draw_failures / device_removed | 0 / 0 | 0 / 0 | 0 / 0 | 0 / 0 |
| ③ 渲染线程 CPU P95 | 0.550 ms（0.033·T） | 0.531 ms（0.032·T） | 0.529 ms（0.032·T） | ≤0.35·T（5.83 ms） |
| ③ GPU P95 | 2.610 ms（0.157·T） | 3.133 ms（0.188·T） | 0.655 ms（0.039·T） | ≤0.35·T |
| 循环间隔 P50 / P95 | 16.65 / 17.12 ms | 16.66 / 17.13 ms | 16.66 / 17.08 ms | （记录） |
| ④ 截图中表面区域是否显示 Rive 内容 | 是 | 是 | 是 | 目视确认 |

**第④条的偏离（须协调确认）**：闸门要求对照参考图 `m0_s*_rive_ref_t0500.bgra` 目视确认，但三张参考图全为 0（800 KB 全透明）。原因是探针缺陷，不是渲染失败：参考图在第一帧就导出，而 Rive 默认 `ShaderCompilationMode::allowAsynchronous`，着色器还没编好时会跳过绘制。实时表面渲染正常，三张截图里都能看到 `Knight_square_2.riv` 的骑士场景，位置也正确。本结题改为直接以截图确认第④条。参考图导出已修复（预热后重试，最长约 2 秒），M0-B 复核时一并验证。

**风险（记录）**：三个会话的 CPU 最大值都是约 436–464 ms，出现在首帧，推测是首次使用时的着色器编译或资源分配。M3 正式接入时必须预热（启动时同步编译，或先离屏渲染几帧再显示），否则 Rive 内容第一次出现时会卡一下。

### M0-1b Rive → 合成交换链（RECORDED）
三个会话的 `create_swapchain`、`CreateCompositionSurfaceForSwapChain`、`start` 的 HRESULT 均为 0，`draw_failures=0`，设备未丢失。统计是累计值（包含 M0-1a 和指针阶段，约 4702–4707 帧），所以交换链阶段单独的帧数和耗时无法拆出来。**路径可用**；M0-B 需要拆开统计。

### M0-2 拖动错位（60Hz 预演，RECORDED）
| | s1 | s2 | s3 |
|---|---|---|---|
| 元素（AnimBox）滞后 P50 | 37.58 ms | 37.39 ms | 37.27 ms |
| Rive 表面滞后 P50 | 37.48 ms | 37.40 ms | 37.46 ms |
| 差值 | −0.10 ms | +0.01 ms | +0.19 ms |
| 按拖动速度折算（0.4 px/ms） | 0.04 px | 0.00 px | 0.08 px |
| 样本数（元素 / 表面） | 260 / 258 | — | — |

两层由同一条合成器表达式驱动，60Hz 下**没有层间错位**（最大差 0.19 ms，远小于建议的 120Hz 门槛 2 ms）。120Hz 的结论以 M0-B 为准。

### M0-3 / M0-5 指针 → 屏幕延迟（RECORDED）
| 路径 | s1 P50 / P95 | s2 P50 / P95 | s3 P50 / P95 | 检出步数 |
|---|---|---|---|---|
| 指针属性集表达式（合成器） | 24.65 / 42.12 ms | 24.18 / 41.74 ms | 20.54 / 41.72 ms | **10/20**（只检出正向步） |
| UI 线程写 Translation | 66.31 / 78.05 ms | 68.51 / 78.39 ms | 69.44 / 78.47 ms | 20/20 |

- **结论**：在 60Hz 本机上，指针跟随走合成器表达式比走 UI 线程快约 45 ms（约 2.7 帧）。这补全了 #32 没测的真实指针跟随，并与“UI 线程只有约 32Hz”一致。
- **测量缺陷 1**：表达式路径的 10 个反向步一个都没检出，原因未明；而同一路径在拖动测试中两个方向都跟踪正常。已加逐帧记录（`M0F` 行），M0-B 时可以查清。
- **测量缺陷 2**：表达式路径的静止位置比 UI 路径偏左约 70 px（cx 30 对 100），说明指针属性集的坐标原点和 `TransformToVisual(RootGrid)` 不同。已改为静止时直接读取属性集的值作为 x0；UI 路径的 x0 保持不变。
- **时钟**：`min(qpcArrive − SystemRelativeTime)` 在所有路径上约为 −15.2 至 −15.8 ms，说明 WGC 的 SystemRelativeTime 比帧到达进程的时刻晚约一帧，更像是该帧的目标显示时间。因此绝对延迟按“注入 → 目标显示时刻”理解，两条路径之间的相对比较不受影响。

### M0-4 ALLOW_TEARING（RECORDED）
- `CheckFeatureSupport(ALLOW_TEARING)=1`。合成交换链带 `ALLOW_TEARING` 标志**创建成功**；`Present(0, ALLOW_TEARING)` 1200 次全部返回 S_OK。HWND 对照组同样成功。
- 这推翻了 disputes.md 中 Claude 早先“composition swapchain 不支持 Present(0, ALLOW_TEARING)”的判断，但仅限 **API 层**。
- 屏幕上是否真的发生撕裂（是否进入 independent flip）**未知**：PresentMon 没有输出；`GetFrameStatistics` 返回 `0x887A000B`（DXGI_ERROR_FRAME_STATISTICS_DISJOINT）；带不带标志的 present 速率都在每秒 5000–6000 次，无法区分。M0-B 需要在有管理员权限的机器上用 PresentMon 复测。

## Go / No-go
M0-1a PASS，且 M0-2（60Hz）没有层间错位 → **Rive 路径继续，不启用 Skia 备选方案**。M0-B（≥120Hz、Win11、有管理员权限的第二台机器）复核 M0-2、M0-4，并试 CompositionTexture 路径。

## 本次修复（分支上，未在真机运行）
- `m0rive.cpp`：`m0r_write_reference` 先预热，重复渲染同一时刻，直到出现非透明像素（最长约 2 秒）；仍为空则返回 S_FALSE，不写文件。
- `M0Probe.cs`：表达式路径的 x0 改为读取属性集的静止值（记录计算值、读数和最终采用值）；步进测试逐帧记录 cx、minx、maxx 和像素数。

## 禁令重申
不改 composition.h；不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；成功 ≠ 采用；PERF-CI 仍 HOLD；结果合入 main 需另行决定。
