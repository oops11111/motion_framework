# M3-XAML-K93 门槛闸门（m3_xaml_k93_gate_LOCKED.md）

STATUS=LOCKED
LOCKED_AT: 2026-11-01 18:37:27 AWST（Australia/Perth）
GATE_BODY_SHA256: 22fb8b92d8703619215ffaae3f0d5d9efea003951bb44b42d790d488ac28b657
> GATE_BODY_SHA256 计算约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。本行写入后不得再改 body。

**本 LOCK 不授权采数。** 须另有 approved 留证（APPROVED_* 非空）后才可采数；构建和 `-DryRun` 走流程不采数，可先行。

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> 起草：Claude，2026-11-01。DRAFT：`docs/m3_xaml_k93_gate_DRAFT.md`。来源：M3-XAML-REBUILD 复核（`docs/m3_xaml_rebuild_result_claude_review.md`）补充 1，pidan 裁决 S1–S4（S2 = 在 K93N3P8 上带 Rive、用修好的截图方式再跑 1 个短会话）。范围已由 S2 裁定，本票没有另外的待裁决项。
> 分支：`claude/m3-xaml-rebuild`。结果目录 `m3/results_xaml_k93/`。

## G0 硬边界
同 XAML-REBUILD：不改 `composition.h`；不重开 S1/R-3/R-2；禁影子 IV；不解冻 Bv；PERF-CI 仍 HOLD；不改 TDR 注册表和驱动设置；未 LOCK 且 APPROVED_* 为空时禁止采数；注入沿用 r2 的 `pnputil /restart-device`、自检、间隔、上限和停止规则。

## G1 要回答的问题
REBUILD 的 C 组里，K93N3P8 的主窗口在驱动重启后界面完整，但那个会话不加载 Rive，无法证明应用的图形设备真的丢失了。本票回答：
1. 在 K93N3P8 上，`pnputil /restart-device` 是否让应用的设备丢失？——由 Rive 的检测给出（`rive.lost_detected`、`rive.removed_reason`）。
2. 如果丢失了，XAML（主窗口、W_pre、W_post）是否仍能画文字？

## G2 设计与判读
- DESKTOP-K93N3P8（Win10 19045，GTX 970，60Hz，缩放 125%），按该机能用的方式启动（REBUILD C 组为打包启动），**1 个会话**：`--m3-xaml-rebuild --m3-xaml-with-rive`（主窗口里有 Rive 表面，外加 W_pre、W_post），驱动重启 1 次；加自检共 2 次。
- 截图用 S1 修好的方式：`PrintWindow(PW_RENDERFULLCONTENT)` 按窗口取内容，runner 进程按每显示器感知 DPI。
- 判读（复核时由 Claude 做）：
  | `rive.lost_detected` | XAML 文字 | 结论 |
  |---|---|---|
  | 1 | 三个窗口都有 | **XAML 在这台机器上能恢复** → 问题限于 IJI1KJG 这类环境（Win11 预览版 26300 或 AMD 驱动），按 S4 向 Windows 反馈中心或 AMD 报告 |
  | 1 | 没有 | 与 IJI1KJG 相同 → 问题更普遍，按 S4 考虑向 WinUI 报告 |
  | 0 | — | 这台机器上的驱动重启没有让应用设备丢失，C 组结论不成立；记 `INCONCLUSIVE_NO_LOSS`，换注入方式另议 |

## G3 实现
沿用 `claude/m3-xaml-rebuild` 的代码，S1/S2 改动在提交 `104e5f9`：runner 截图改为 `PrintWindow` + 每显示器 DPI 感知；打包启动时把 `m0rive.dll` 复制进 AppX 目录；探针在 rebuild + Rive 模式下记录 Rive 的丢失检测。K93N3P8 需要构建带设备丢失恢复的 `m0rive.dll`（RECOVER 版本，`m0/build_m0.ps1`）。

## G4 禁令
同 XAML-REBUILD；本 DRAFT ≠ 授权采数；诊断结果 ≠ 采用。
