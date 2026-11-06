# M3-MATRIX-PACING 实现说明（Claude）

> 对应闸门：`docs/m3_matrix_pacing_gate_LOCKED.md`（notion ai 锁定，GATE_BODY_SHA256=`2543e424cd71eea2959fe942ce0bfe7d7118ea6ee5c28b31da7902bce904812c`，复算 MATCH）。闸门 G5 把实现列为“待写”；本文件说明已写的实现，**不修改闸门**。代码 = 分支 `claude/m3-matrix` 提交 `248a2d4`。
> **需要 pidan 在批准采数时一并确认**：闸门 G5 只列了“统计 / 探针字段 / runner”，没有写渲染器改动；但不改渲染节奏，本票必然 FAIL（见 `docs/m3_matrix_result_claude_review.md`）。下面第 1 项是渲染器改动。

## 1. 修复：按窗口所在显示器的垂直同步出帧（`m0/native/m0rive`）
- 新增 `m0r_set_pacing_window(hwnd)`：设置后，渲染线程每帧不再 `DwmFlush`，而是对**窗口当前所在显示器**对应的 DXGI 输出调用 `IDXGIOutput::WaitForVBlank`。窗口换屏（`MonitorFromWindow` 变化）或设备被替换（设备丢失恢复）时重新查找输出。找不到对应输出（例如该屏不在我们的显卡上）时退回 `DwmFlush`。
- 新增 `m0r_get_pacing`：报告当前是按垂直同步还是 `DwmFlush`、对应的显示器、查找次数，探针写进 summary。
- **默认行为不变**：不调用 `m0r_set_pacing_window` 时仍用 `DwmFlush`（其他所有探针不受影响）。
- 预期：75 Hz 屏上循环间隔 ≈ 13.3 ms，180 Hz 屏上 ≈ 5.56 ms；每帧在该屏垂直同步后立刻画并提交，由 DWM 在下一次合成时取走。

## 2. 测量（按闸门 G1/G2）
- 探针 `--m3-matrix-pacing`（`M3MatrixProbe.cs` 的 pacing 模式）：与 MATRIX 相同的流程（5 段、4 次跨屏，拖动和跳转交替），打开上面的垂直同步出帧。
- **瞬态豁免窗（G2 ⑦）**：每段的稳态采样从“跨屏移动结束或缩放变化完成（取较晚）之后 max(2.0 s, 120·T)”才开始；跨屏期间的样本只进跨屏记录。
- **列级判读（G2 ②）**：按“主显示器列（75 Hz）”和“外接显示器列（180 Hz）”汇总各自所有稳态段的样本；每列 `P95(|Δt − T|) ≤ 0.25·T` 且 `|Δt − T| > 0.5·T` 的占比 ≤ 5% 才算过；`pacing_ok` = 两列都过，且每段窗口确实在预期显示器上。
- **分布（G2 ⑤）**：summary 写出每段和每列的间隔 P50 / P95 / 最大值、偏差 P95、超半周期占比、出帧方式（`vblank` / `dwmflush`）。时间戳来自渲染线程的 `steady_clock`（QPC），不依赖会被回收的托管定时器。
- **前序字段（G2 ④）**：hitch / size / budget / stability 照旧计算并写出。
- **文件和目录**：前缀 `m3matrixpacing_`；结果目录 `m3/results_matrix_pacing/`；不碰 `m3/results_matrix/`。

## 3. 测试工具修正（MATRIX 执行方备注的两处）
- runner 用参数 `--m3-artdir=`、`--m3-session=` 传数据目录和会话号（探针优先读参数）。
- M3 各探针的启动定时器保存在 `MainPage` 字段里，不再只由局部变量持有。

## 4. 本地验证
MinGW 对照钉住的 Rive 头文件：0 警告 0 错误。C# 对照 WinAppSDK 2.5.1 编译 0 个 C# 错误。PowerShell 未运行（本环境没有），人工复查。`WaitForVBlank` 在窗口化应用里的实际节奏只能在真机上验证。
