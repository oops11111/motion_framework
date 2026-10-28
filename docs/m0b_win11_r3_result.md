# M0-B r3 结题结果（m0b_win11_r3_result.md）

STATUS: CLOSED
TICKET: **M0-B-PASS**（M0-2 PASS 3/3；M0-1a PASS 3/3）→ 按闸门 G2，**M0 关闭**，Rive 路径进入 M3 规划
WRITTEN_AT: 2026-10-28 17:11:40 AWST（Australia/Perth）
GATE: `docs/m0b_win11_gate_r3_LOCKED.md`，GATE_BODY_SHA256=`741729784693893dedb6190b5d2768617045554ae90a58dfe00fd52855f48ede`（结题时复算：MATCH）
APPROVED: `docs/m0b_win11_r3_approved.md`，pidan @ 2026-10-28 15:26:25 AWST（提权 + PresentMon）
DATA: 分支 `claude/m0b-win11-run` 的 commit `c3491a87`（提交者 Cursor Agent，2026-10-28 09:04 UTC），目录 `m0/results_m0b_r3/`。采数时代码为 `6c2b9c17`；三个会话的 summary 都是 `probe=M0 v3`，并带有 `gpu_samples_dropped` 字段，确认用的是 v3 的 DLL 和探针
MACHINE: DESKTOP-IJI1KJG，SMBIOS UUID `03FF0210-04E0-057F-E006-E90700080009`（与 r2 是同一台）；Windows 11 build 26200；AMD Radeon RX 7800 XT；1280×1024，75.03Hz；缩放 100%；**已提权**
composition.h: 运行前后 sha256 均为 `a55d4952…e418`，未改。r2 的数据（`m0/results_m0b/`）没有改动，也没有与 r3 混用。

## G6 ONELINE
```
M0-B r3 | gate=74172978…8ede @ 2026-10-28 14:53:18 AWST | approved=pidan@2026-10-28 15:26:25 AWST | host=DESKTOP-IJI1KJG build=26200 refresh=75 elevated=是 | sessions=3 | m0_2=PASS misalign_ms=-0.04/0.06/-0.06 samples≥404 | m0_1a=PASS cpu_p95≤0.096·T gpu_p95=0.014·T dropped=0 | m0_4 PresentMode(comp_tear)=Hardware: Independent Flip | 不改composition.h；成功≠采用
```

## 执行偏离（不影响判据）
- **本地 runner**：运行日志写的是 `M0 run LAB` 和 `LAB launch`，与仓库里 `run_m0.ps1` 的输出不同，说明执行方用了一个**没有提交**的本地改版 runner。据 `host_meta.txt`：Release 版和非打包启动报 `REGDB_E_CLASSNOTREG`，主机上也没有打包启动的配置，所以改为直接启动 Debug 版 exe（`bin\x64\Debug\…\MotionSoftDemo.exe --m0-probe --m0-inject`）。runner 只负责启动进程、截图和收集文件，测量逻辑都在探针（v3）里；会话矩阵、截图和 summary 都齐全。**建议**以后把本地 runner 提交到仓库，或把 Debug 直接启动的方式并回 `run_m0.ps1`。
- **PresentMon 只覆盖了前 60 秒**：runner 给 PresentMon 固定 `--timed 60`，而不带标志的 sync0 arm 被节流到每秒约 174 次，30000 帧就要约 173 秒。结果是 comp_tear 和 comp_sync0 都抓到了，**hwnd_tear 没抓到**。HWND 只是对照组，不影响合成交换链的结论。以后 runner 应改为按探针实际运行时长来设 PresentMon 时长。
- **Debug 构建**：MotionSoftDemo 是 Debug 版，但 Rive DLL（`m0rive.dll`）是按 Rive 默认设置以 Release 编译的，CPU/GPU 计时都在 DLL 的渲染线程里测，不受 C# 构建配置影响。

## M0-2 拖动错位（≥75Hz 正式判定）→ **PASS**
| | s1 | s2 | s3 | 门槛 |
|---|---|---|---|---|
| \|misalign_ms\| | 0.04 | 0.06 | 0.06 | ≤ 2 ms ✓ |
| 样本数（元素 / 表面） | 405 / 408 | 405 / 404 | 406 / 406 | ≥ 200 ✓ |
| 元素 / 表面滞后 P50 | 30.45 / 30.41 ms | 30.46 / 30.53 ms | 30.52 / 30.46 ms | （记录） |
| 按拖动速度折算 | 0.02 px | 0.03 px | 0.03 px | （记录） |

元素和 Rive 表面由同一条合成器表达式驱动，在 75Hz 下两层同帧移动，错位约 0.05 ms（不到 0.03 px）。与 M0-A（60Hz）和 r2 的结论一致。

## M0-1a Win11 回归 → **PASS**
| 判据 | s1 | s2 | s3 | 结论 |
|---|---|---|---|---|
| ① 全部 HRESULT | 0 | 0 | 0 | ✓ |
| ② 帧数 / 失败 / 设备丢失 | 1502 / 0 / 0 | 1501 / 0 / 0 | 1500 / 0 / 0 | ✓ |
| ③ CPU P95 | 1.285 ms（0.096·T） | 1.091 ms（0.082·T） | 1.089 ms（0.082·T） | ✓（≤0.35·T = 4.66 ms） |
| ③ GPU P95 | 0.194 ms（0.015·T） | 0.192 ms（0.014·T） | 0.192 ms（0.014·T） | ✓ |
| ③ GPU 丢弃样本 | 0 | 0 | 0 | ✓（≤10%） |
| ④ 参考图非空，内容与截图一致 | 100% 不透明 | 同左 | 同左 | ✓（场景、角色、构图一致；配色是夜景对白天，原因是动画时间不同，与 r2 结论相同） |

- 首帧 CPU 最大值为 1.36–1.58 ms，这台机器上没有首帧卡顿。
- GPU 计时修复已生效：上一轮的回绕值（1.8×10¹⁴ ms）消失了，丢弃样本为 0。

## M0-1b 交换链（RECORDED）
帧数 1500/1501/1501，0 失败；CPU P95 2.36 / 1.92 / 2.37 ms（0.14–0.18·T）；GPU P95 约 0.22 ms，丢弃样本 0。交换链路径的 CPU 耗时约为表面路径的两倍，但仍远低于预算。

## M0-3 / M0-5 指针延迟（RECORDED）
| 路径 | s1 P50 | s2 P50 | s3 P50 | 检出 |
|---|---|---|---|---|
| 表达式（合成器） | 40.64 ms | 39.87 ms | 38.68 ms | 20/20 |
| UI 线程 | 52.59 ms | 54.97 ms | 55.53 ms | 20/20 |

表达式路径快约 13–17 ms，与 r2 一致。

## M0-4 ALLOW_TEARING（RECORDED，**首次拿到屏幕层面的证据**）
| arm | 被 PresentMon 记录的帧数 | PresentFlags / AllowsTearing | PresentMode |
|---|---|---|---|
| comp_tear（合成交换链 + ALLOW_TEARING） | 4071 | 512（ALLOW_TEARING）/ 1 | **全部 Hardware: Independent Flip** |
| comp_sync0（合成交换链，不带标志） | 9013 | 0 / 0 | Hardware: Independent Flip 4100；Hardware Composed: Independent Flip 4100；Composed: Flip 813 |
| hwnd_tear | 未记录（超出 PresentMon 的 60 秒窗口） | — | — |

- 结论：在 Win11 26200、RX 7800 XT 上，**合成交换链带 `ALLOW_TEARING` 时，以 independent flip 方式上屏，并且 `AllowsTearing=1`**，即屏幕上确实可以撕裂，不经过 DWM 合成节奏。这推翻了 disputes.md 中“composition swapchain 不支持 ALLOW_TEARING”的判断，而且不只是 API 层面。
- present 速率：带标志约 10,274 次/秒，不带标志约 174 次/秒，与 r2 的观察一致。
- 是否在产品中使用撕裂，属于 M3 的设计选择（闸门 E2），本票只做记录。

## 结论
- **M0-B-PASS**：M0-2 和 M0-1a 都按 r3 判据通过。按闸门 G2，**M0 关闭**，Rive 路径进入 M3 规划，不启用 Skia 备选方案。
- M0 汇总：M0-A（Win10/NVIDIA/60Hz）PASS + M0-B r3（Win11/AMD/75Hz）PASS。两套硬件、两个 Windows 版本上，Rive 都能渲染进 WinUI 3 合成表面，渲染预算余量很大，与合成器驱动的元素同帧移动。
- 按 plan.md（已改为 75Hz），M0 的验收门槛“75Hz 拖动错位测试结论”已经给出。

## 带进 M3 的事项
1. 首帧预热：M0-A 的 Win10/NVIDIA 上首帧约 440 ms，Win11/AMD 上没有这个问题。M3 仍需预热（着色器同步编译或先离屏渲染）。
2. 表面路径与交换链路径：交换链的 CPU 耗时约为表面路径的两倍，但能用 independent flip 和撕裂；按需要选择。
3. 撕裂：Win11 上 independent flip 可以做到，是否启用由设计决定。
4. 测量工具：本地 runner 要并回仓库；PresentMon 时长要跟着探针实际运行时长走；WGC 在 75Hz 下只有约 37 fps，以后的测量需要相应增加采样次数。
5. 尚未覆盖：混合刷新率与 DPI 矩阵、设备丢失恢复、DPI 变化时重新光栅化（plan.md M3 项）。

## 禁令重申
不改 composition.h；不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；成功 ≠ 采用；PERF-CI 仍 HOLD；r1/r2 闸门和 r2 结论不改。
