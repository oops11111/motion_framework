# M0-B Win11 复核门槛闸门（m0b_win11_gate_LOCKED.md）

STATUS=LOCKED
LOCKED_AT: 2026-10-27 00:19:21 AWST（Australia/Perth）
GATE_BODY_SHA256: 5e1bbb6d517babf6f04a94ee9af93cc17f208f5ea9e5bdcf7ac47c8cd49df203
> GATE_BODY_SHA256 计算约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。本行写入后不得再改 body。

**本 LOCK 不授权采数。** 须另有 approved 留证（APPROVED_* 非空）后，才可派 workbuddy 采数。构建不采数，可先行。

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> 起草：Claude（Claude Code 接手会话），2026-10-27。触发：pidan「M0-B开新分支在win11上跑」。
> 前序：M0-A CLOSED M0-A-PASS（`docs/m0_tech_validation_result.md`，gate `942d6965…9768`，已合入 main，PR #1）。M0-A gate 不改。
> DRAFT：`docs/m0b_win11_gate_DRAFT.md`（经 PR #2 合入 main）。
> 分支：`claude/m0b-win11-run`（从 main `513dd5e` 拉出）。真机结果只提交到这个分支，评审通过后经 PR 合入 main。

## G0 硬边界
- 不改 `composition.h`（`a55d4952…e418`）。不重开 S1、R-3、R-2；不解冻 Bv；禁影子 IV；PERF-CI 仍 HOLD；Δv″ 仍挂账。
- 未 LOCK 且 APPROVED_* 为空时禁止采数。成功 ≠ 采用。

## G1 目标机器（写死）
| 要求 | 用途 | 不满足时 |
|---|---|---|
| Windows 11（build ≥ 22000） | 本票前提 | 整票 INVALID |
| 显示器刷新率 ≥ 120Hz（runner 记录 `refresh_hz`） | M0-2 正式判定 | M0-2 只记录，**不判定**，并另找机器 |
| 提权 shell，或用户属于 Performance Log Users 组 | M0-4 的 PresentMon | M0-4 仍只有 API 层结论 |
| 物理机；machineId 取 SMBIOS UUID，且与 DESKTOP-K93N3P8（`03000200-…0009`）不同 | 换机复核 | 整票 INVALID |

## G2 验证项与判据
| 项 | 内容 | 判据 |
|---|---|---|
| **M0-2** | 拖动错位，在 ≥120Hz 下正式判定 | **PASS** ⇔ 3 个会话的 `\|m0_2.misalign_ms\|` 都 ≤ 2 ms，且每条路径每个会话的样本数 ≥ 200（M0-A D6） |
| **M0-1a** | Win11 回归 | **PASS** ⇔ M0-A 的 G2-M0-1 第①–③条全部满足，且第④条按原文执行：参考图非空，截图中表面区域显示的 Rive 内容与参考图一致（修复后的参考图导出在此一并验证） |
| M0-1b | 交换链阶段单独统计（`m0r_reset_stats`） | 记录：帧数、失败数、CPU/GPU P95 |
| M0-3/5 | 用修正后的 x0 重测延迟；步进逐帧记录 | 记录；表达式路径 `steps_hit` 若仍不足 20/20，用 `M0F` 行给出原因 |
| **M0-4** | 撕裂 | 记录：全部 HRESULT，以及每个 arm 的 PresentMon `PresentMode`。`comp_tear` 出现 independent flip 类模式，才算“屏幕上确实可撕裂” |
| M0-1c | CompositionTexture 路径 | **不做**：WinAppSDK 2.5.1 的公开投影里没有 CompositionTexture 类型（只有 interop 头文件里的 `ICompositorInterop2`）。等 SDK 公开后另票 |

- **会话**：≥3 个独立进程，带 `-Inject`（M0-A 的 D1 延用到 M0-B）。
- **INVALID**：缺产物；hash 不符；未授权采数；改 composition.h；机器不满足 G1 第 1 或第 4 行；构建工具链失败记为 BLOCKED_TOOLCHAIN。
- **票级结论**：M0-2 PASS 且 M0-1a PASS → **M0-B-PASS**，M0 关闭，Rive 路径进入 M3 规划。M0-2 FAIL → 列出错位数值，交协调裁决是否改用 Skia 备选方案。

## G3 裁决（pidan：E1–E4 全部按建议）
| # | 问题 | 裁决（= 原建议） |
|---|---|---|
| **E1** | Win11 机器达不到 120Hz 怎么办 | 仍然跑，M0-1a 回归、M0-4、M0-3/5 照常有效；M0-2 只记录。M0 要等找到 ≥120Hz 的屏幕（外接显示器也可以）才能关闭 |
| **E2** | M0-4 只记录、不判定，是否合适 | 合适。plan.md 只要求“记录 ALLOW_TEARING 测试结果”；是否需要撕裂，属于 M3 的设计选择 |
| **E3** | 这台机器能不能顺便当 PERF-CI 的第二台 | 本票不做。如果它的 GPU 档和 GTX 970 不同（集显或低端笔记本），可以另开 PERF-CI 票，但要先裁决 PERF-CI 闸门里的 Q1、Q2、Q7（`docs/m2_perf_ci_tooling_gate_DRAFT.md`） |
| **E4** | 截图范围 | 只截应用窗口（runner 已改）。M0-A 截的是整个桌面，已有截图是否裁剪由 pidan 决定 |

## G4 本分支相对 M0-A 的改动
| 文件 | 改动 | 本地验证 |
|---|---|---|
| `m0/native/m0rive/m0rive.{h,cpp}` | 新增 `m0r_reset_stats`；参考图导出改为等着色器编译后再读回（M0-A 结题后已改） | MinGW 语法检查通过（含 Rive 默认宏和 `-Wshadow`） |
| `MotionSoftDemo/M0Probe.cs` | 版本改为 `M0 v2`；M0-1b 单独统计；artifact 目录可用 `MF_M0_ARTDIR` 指定；x0 修正和逐帧记录（M0-A 结题后已改） | 对照 WinAppSDK 2.5.1 编译，0 错误 |
| `m0/run_m0.ps1` | 新增 `-DemoProj`、`-TearFramesPerArm`（默认 30000，每个 arm 约 5 秒，留给 PresentMon 采集）；向应用传入 artifact 目录；记录 OS build、是否提权、GPU 和刷新率；截图只截应用窗口；会话矩阵加入 refresh_hz、m0_1b_frames、misalign_ms | 未做语法检查（本环境没有 PowerShell），已人工复查 |

## G5 禁令
不改 composition.h；不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；本 DRAFT ≠ 授权采数；成功 ≠ 采用；PERF-CI 仍 HOLD；M0-A gate 和结论不改。

## G6 ONELINE 模板
M0-B | gate=<hash> @ <LOCKED_AT> | approved=<who>@<when> | host=<hostname> build=<n> refresh=<Hz> elevated=<是/否> | sessions=<n> | m0_2=<PASS/FAIL/RECORDED(<120Hz)> misalign_ms=<s1/s2/s3> | m0_1a=<PASS/FAIL> | m0_4 PresentMode(comp_tear)=<…> | 不改composition.h；成功≠采用
