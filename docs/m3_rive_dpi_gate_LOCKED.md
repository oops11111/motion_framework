# M3-RIVE-DPI 门槛闸门（m3_rive_dpi_gate_LOCKED.md）

STATUS=LOCKED
LOCKED_AT: 2026-10-28 20:39:58 AWST（Australia/Perth）
GATE_BODY_SHA256: 92e6b4e9fd1014faf74714eb6cb20fc7e998169dc1171785bb8ea43f68d97bf5
> GATE_BODY_SHA256 计算约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。本行写入后不得再改 body。

**本 LOCK 不授权采数。** 须另有 approved 留证（APPROVED_* 非空）后才可采数；构建不采数，可先行。

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> 起草：Claude（Claude Code 接手会话），2026-10-28。依据：`docs/m3_plan_DRAFT.md` §7 第 1 张票（WP1，plan 任务 8）；M3 验收门槛第 1 条“Rive 表面支持 DPI 变化”。F1 已裁决：默认走合成表面，所以本票只测表面路径。
> DRAFT：`docs/m3_rive_dpi_gate_DRAFT.md`。分支：`claude/m3-rive-dpi`（从 main `f74f6e3` 拉出，已合入 main `27c060b`）。真机结果只提交到这个分支，评审后经 PR 合入。

## G0 硬边界
- 不改 `composition.h`；不重开 S1/R-3/R-2；禁影子 IV；PERF-CI 仍 HOLD。
- 未 LOCK 且 APPROVED_* 为空时禁止采数。成功 ≠ 采用。

## G1 要验证什么
Rive 内容所在的合成表面，在窗口所在显示器的缩放比例变化时（`XamlRoot.RasterizationScale` 变化）：
1. 表面的像素尺寸改为“逻辑尺寸 × 新缩放”，Rive 按新像素尺寸重新渲染（清晰度不变），逻辑尺寸（DIP）保持不变；
2. 渲染不中断、不出错、设备不丢失；
3. 切换前后渲染预算仍满足 plan.md（渲染线程 ≤0.35·T、GPU ≤0.35·T）；
4. 来回切换不泄漏内存。

## G2 判据（每个会话）
| 判据 | 门槛 |
|---|---|
| ① 切换次数 | 每个会话至少 4 次缩放切换（100% → 125% → 100% → 125% → 100%），每次都被探针捕获（`XamlRoot.Changed` 中的新缩放值） |
| ② 尺寸正确 | 每次切换后，表面像素尺寸 = round(逻辑尺寸 × 新缩放)，误差 0 px |
| ③ 重新光栅化延迟 | 从探针收到缩放变化，到渲染线程完成第一帧新尺寸的渲染，P95 ≤ 100 ms（**记录**；这是用户能看到模糊或错尺寸的时间窗） |
| ④ 稳定性 | `draw_failures=0`；`device_removed_hr=0`；每个缩放段帧数 ≥ 300 |
| ⑤ 预算 | 每个缩放段 CPU P95 ≤ 0.35·T，GPU P95 ≤ 0.35·T，GPU 丢弃样本 ≤ 10% |
| ⑥ 清晰度 | 每次切换后的窗口截图与按新像素尺寸导出的参考图目视一致，且不是放大或缩小后的模糊图（协调目视确认，写入结题） |
| ⑦ 内存 | 最后回到 100% 后，进程专用内存比第一次 100% 段末尾多出 ≤ 20 MB（**记录**；超出就列为 M3 待查项，不判 FAIL） |

- **会话**：≥3 个独立进程。
- **PASS** ⇔ ①②④⑤⑥ 每个会话都满足；③⑦只记录。
- **INVALID**：缺产物；hash 不符；未授权采数；改 composition.h；缩放没有真正发生（探针没捕获到变化）。

## G3 裁决（pidan：G1–G4 全部按建议）
| # | 问题 | 裁决（= 原建议） | 理由 |
|---|---|---|---|
| **G1** | 怎样触发缩放变化 | **由操作员按探针提示，在“设置 → 显示 → 缩放”里手动切换**；探针按 `XamlRoot.Changed` 记录时刻，不依赖操作员手速 | 单显示器上没有正式的编程接口（只有未公开的 `DisplayConfigSetDeviceInfo` 用法，不建议依赖）；把窗口拖到另一块缩放不同的显示器属于 M3-MATRIX |
| **G2** | 测哪些缩放比例 | **100% ↔ 125%** | 两台测试机都是 1280×1024，Windows 在这个分辨率下通常只提供 100% 和 125%（150% 会让有效高度低于 720）。自定义缩放需要注销，不能实时切换。150%/200% 留给有更高分辨率显示器的 M3-MATRIX |
| **G3** | 用哪台机器 | **DESKTOP-IJI1KJG**（Win11、可提权、M0-B 的那台） | 已有完整的构建和运行环境；Win11 是主要目标平台 |
| **G4** | 操作员手动操作会不会影响“不碰鼠标键盘”的约定 | 本票**不注入指针**（不需要 SendInput）；操作员只在提示出现时切换缩放，其余时间不碰 | 本票只测渲染和尺寸，不测指针 |

## G4 实现（已写，未在真机运行）
| 文件 | 内容 | 本地验证 |
|---|---|---|
| `m0/native/m0rive/m0rive.{h,cpp}` | 新增 `m0r_resize(w, h)`：UI 线程只登记请求，渲染线程在下一帧开头执行表面 interop `Resize`、重建离屏纹理、Rive 渲染目标和标记 RTV；渲染线程未运行时直接在调用线程执行。新增 `m0r_get_resize`，报告次数、最近一次和最大的“请求 → 新尺寸首帧完成”耗时、实际尺寸和 HRESULT。交换链路径返回 E_NOTIMPL（F1） | MinGW 对照钉住的 Rive 头文件（含默认宏和 `-Wshadow`）做语法检查，通过 |
| `MotionSoftDemo/M3DpiProbe.cs`（`--m3-dpi-probe`） | 表面逻辑尺寸固定为 400 DIP；在窗口内提示操作员切换缩放；监听 `XamlRoot.Changed`，换算像素尺寸并调用 `m0r_resize`；分 5 个缩放段统计帧数、失败、CPU/GPU P95、GPU 丢弃样本、进程专用内存；每段请 runner 截窗口图，并导出该尺寸下的参考图 | 对照 WinAppSDK 2.5.1 编译，0 错误 |
| `MotionSoftDemo/MainPage.xaml.cs` +11 行；`m3/mainpage_m3_hook.patch` | 入口 | 补丁能打到 main 上的 MainPage |
| `m3/run_m3_dpi.ps1` | 按会话启动应用；响应探针的截图请求（只截应用窗口）；等待 summary；输出会话矩阵 | 未做语法检查（本环境没有 PowerShell），已人工复查 |

## G5 禁令
不改 composition.h；不重开 S1/R-3/R-2；禁影子 IV；本 DRAFT ≠ 授权采数；成功 ≠ 采用；PERF-CI 仍 HOLD。

## G6 ONELINE 模板
M3-RIVE-DPI | gate=<hash> @ <LOCKED_AT> | approved=<who>@<when> | host=<名> scales=<100↔125> | sessions=<n> | switches=<n/会话> | size_ok=<是/否> | reraster_p95_ms=<…> | budget=<PASS/FAIL> | sharp=<目视结论> | mem_delta_mb=<…> | ticket=<PASS/FAIL/INVALID> | 不改composition.h；成功≠采用
