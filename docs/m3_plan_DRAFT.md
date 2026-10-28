# M3 规划（m3_plan_DRAFT.md）

STATUS=DRAFT（规划文档，**不授权采数**；其中每张真机票仍须单独走 DRAFT → LOCK → APPROVED）
起草：Claude（Claude Code 接手会话），2026-10-28。依据：plan.md §4–§5（M3 = 第 22 周“Rive 层 + 首个试点”）；`docs/m0_tech_validation_result.md`；`docs/m0b_win11_r3_result.md`；评估报告 `docs/assessment/fluidmotion_assessment.html`（在 `claude/m2-perf-ci-tooling` 分支）。

## 1. 进入 M3 时的状态
| 里程碑 | 状态 | 说明 |
|---|---|---|
| M0 技术验证 | **已关闭** | M0-A（Win10 / GTX 970 / 60Hz）PASS；M0-B r3（Win11 / RX 7800 XT / 75Hz）PASS。Rive 路径可行，不启用 Skia 备选方案 |
| M1 核心 | 基本达成 | 纯逻辑单测全绿；弹簧映射真机 RMS ≤0.2%；分发器（`composition.h`）冻结 |
| M1b 打断续接 R-2 | 本机 FAIL，挂账 | 降级为 B_v（只保证位置连续）；速度连续的绝对门没有通过（7 张票） |
| M2 Alpha | 部分 | M2-02 PASS（C# API、ETW、预览宿主 v0）；**PERF-CI HOLD**：还需要一台 GPU 档不同的物理机，闸门 Q1/Q2/Q7 待裁决；WinMD 投影未做（现在走 C ABI + P/Invoke） |

## 2. M3 验收门槛（plan.md，原文）
1. Rive 表面支持 DPI 变化和设备丢失恢复；
2. 跟随部件拆分完成；
3. 通过混合刷新率测试矩阵；
4. 试点 1 开始接入。

## 3. 工作包
| WP | 内容 | plan 任务 | 依赖 | 负责 | 真机票 | 估计（人周，plan） |
|---|---|---|---|---|---|---|
| **WP1 Rive 宿主产品化** | 把 `m0rive` 原型做成引擎模块：独立渲染线程、表面/交换链两条路径、**首帧预热**、**DPI 变化时重新光栅化**（`RasterizationScale` 变化 → 表面 Resize → 按新尺寸渲染）、**设备丢失恢复**（`DXGI_ERROR_DEVICE_REMOVED` → 重建 D3D 设备和 Rive 上下文 → `ICompositionGraphicsDeviceInterop::SetRenderingDevice`） | 8 | M0 ✓ | 人（D3D/Rive），Claude 写骨架和单测 | M3-RIVE-DPI、M3-RIVE-LOST | 8–12 |
| **WP2 跟随部件与指针管线** | 跟随指针的部件拆成由合成器表达式驱动的 visual（M0 实测：表达式路径比 UI 线程快 12–45 ms）；Rive 内部依赖指针的变形写成设计约束，并给出缓解指南（disputes.md §3） | 9 | WP1 | 人 | M3-FOLLOW | 3–5 |
| **WP3 混合刷新率与 DPI 矩阵** | plan.md §6：60+75Hz、60+144Hz 混合；100%/150%/200% 缩放混合；动画中途跨屏拖动；动态刷新率 | 6、8 | WP1 | 人 + 实验室 | M3-MATRIX | （含在 13） |
| **WP4 试点 1 接入** | 按 plan.md §7 选应用；接入期间冻结公开 API 的形状 | 7、15 | WP1、F4 | 人 | 试点报告（非闸门票） | 6–8（含文档） |
| **WP5 M2 遗留项** | ① PERF-CI（第二台机器 + Q1/Q2/Q7）；② WinMD 投影；③ 在 Windows 端接上减少动效（`UISettings.AnimationsEnabled`）；④ R-2 产品决策（F5） | 7、12、13 | — | 人 / Claude | M2-PERF-CI | 见 plan |

**关键路径**：WP1（DPI 和设备丢失）→ WP2 → WP3；WP4 的选型可以和 WP1 并行，但真正接入要等 WP1 和 F4。

## 4. M0 带进来的事项（写入对应 WP）
| 事项 | 证据 | 归入 |
|---|---|---|
| 首帧卡顿：Win10 / NVIDIA 上约 440 ms，Win11 / AMD 上约 1.4 ms | M0-A、M0-B r3 | WP1（F3） |
| 表面路径 CPU P95 ≈ 0.08–0.10·T，交换链 ≈ 0.14–0.18·T | M0-B r3 | WP1（F1） |
| 合成交换链 + ALLOW_TEARING 在 Win11 上为 Hardware: Independent Flip | M0-B r3 PresentMon | WP1（F2） |
| WGC 在 75Hz 下只有约 37 fps；后续测量要增加采样次数 | M0-B r2 | WP3 测量设计 |
| runner：执行方本地改版没有提交；PresentMon 固定 60 秒 | M0-B r3 | 工具收尾（本分支第二项） |

## 5. 硬件需求
| 需求 | 用途 | 现状 |
|---|---|---|
| 一台机器接两块刷新率不同的显示器（如 60Hz + ≥120/144Hz） | WP3 | 没有 |
| 一台机器可以设 150%/200% 缩放 | WP1 DPI 票、WP3 | 两台现有机器都可以改缩放（需确认） |
| 有管理员权限 | 设备丢失注入（`dxcap -forcetdr` 等）、PresentMon | DESKTOP-IJI1KJG 可提权 |
| 一台集显或低端笔记本 | PERF-CI 第二台机器（GPU 档须不同于两台独显机） | 没有 |

## 6. 需要裁决的事项（附建议）
| # | 问题 | 建议 | 理由 |
|---|---|---|---|
| **F1** | Rive 默认走哪条路径 | **合成表面为默认**，交换链作为可选项（大面积、高频内容） | 表面路径 CPU 约为交换链的一半，DPI 和合成都交给系统；交换链保留给需要 independent flip 的场景 |
| **F2** | 是否默认启用撕裂 | **默认关闭**，只对低延迟跟随场景开放 opt-in | 框架目标是流畅、合规的同步动画；撕裂的收益只在极低延迟场景 |
| **F3** | 首帧预热策略 | **首次显示前先离屏渲染若干帧**（保持异步编译），超时则显示占位 | 不阻塞 UI 线程；兼顾 Win10 / NVIDIA 上 440 ms 的情况 |
| **F4** | 试点 1 绑定哪个 API | **现在就按未来 WinMD 的形状冻结 C# API**，试点绑定这个 API；底层先走 C ABI，WinMD 之后替换，对试点透明 | 不让 WinMD（5–7 人周）阻塞试点；试点代码不需要随之返工 |
| **F5** | 打断续接（R-2）怎么处理 | **试点期按 B_v 发布**（位置连续），“改目标时速度可能不连续”写进设计约束；另请设计师在真机上盲测，决定是否还要追速度连续 | 同一方向已经连续 7 张真机票没有通过；按评估报告建议 4 处理 |
| **F6** | M3 的流程 | 真机采数票照旧走 DRAFT → LOCK → APPROVED；纯代码、纯文档改动走 PR 评审 | 与 M0 一致，减少文档负担 |

## 7. 建议的 M3 票序
1. **M3-RIVE-DPI**：表面在 100% → 150% → 100% 切换时重新光栅化（M0 两台机器任选一台）。
2. **M3-RIVE-LOST**：设备丢失注入后自动恢复，恢复时间和内存增长都在记录内（需要管理员权限）。
3. **M3-FOLLOW**：跟随部件拆分，与 Rive 内部指针变形的延迟对比。
4. **M3-MATRIX**：混合刷新率与 DPI 矩阵（需要双显示器）。
5. 试点 1 选型并行进行；接入在 1、2 完成后开始。

## 8. 禁令（沿用）
不改 `composition.h`（分发器冻结；若 M3 需要改，另开闸门）；不重开 S1/R-3/R-2；不解冻 Bv（F5 裁决前）；禁影子 IV；成功 ≠ 采用；PERF-CI 在第二台机器到位前仍 HOLD。
