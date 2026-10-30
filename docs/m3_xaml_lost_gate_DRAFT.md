# M3-XAML-LOST 门槛闸门（m3_xaml_lost_gate_DRAFT.md）

STATUS=DRAFT（**NOT LOCKED**；本文件**不授权采数**）
LOCKED_AT: （LOCK 时填）
GATE_BODY_SHA256: （LOCK 时算；约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希）

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> 起草：Claude，2026-10-30。来源：M3-RIVE-LOST-RECOVER 复核（`docs/m3_rive_lost_recover_result_claude_review.md`）中的新发现：三个会话、六次显卡驱动重启后，应用自己的 XAML 界面（页面标题、开关、按钮、提示文字、标题栏图标和标题）**全部消失且不再重画**，只有我们的 Rive 视觉在画。像素统计：标题区亮像素 1518 → 0，提示区 506 → 0；恢复后探针改过提示文字，也没有画出来。
> 这是一张**诊断票**：先弄清楚 XAML 不恢复是 WinUI 3 本身的问题，还是我们的代码引起的；不判 PASS/FAIL。
> 分支：建议 `claude/m3-xaml-lost`，从 `claude/m3-rive-lost` 拉出（复用 runner 的注入、自检和停止规则）。

## G0 硬边界
不改 `composition.h`；不重开 S1/R-3/R-2；禁影子 IV；不解冻 Bv；PERF-CI 仍 HOLD；不改 TDR 注册表和显卡驱动设置；未 LOCK 且 APPROVED_* 为空时禁止采数；本机 csproj 补丁不进仓库。

## G1 要回答的问题
1. **不加载 Rive、不碰任何合成图形设备**的普通 WinUI 3 窗口，在显卡驱动重启后，XAML 界面会不会恢复？
2. 如果不恢复，下面哪种操作能让它恢复：多等一会儿（30 s）；程序改变窗口大小（±1 px）；最小化再还原；整个页面重新设置一遍内容（`Content` 换新再换回）？
3. 如果普通窗口能恢复，而带 Rive 的窗口不能，那就是我们的代码（`SetRenderingDevice`、我们挂进 XAML 树的 SpriteVisual 或别的）引起的。

## G2 观测设计（每个会话；数字为建议值，见 N2）
- 探针 `--m3-xaml-lost-probe`：**不加载 m0rive.dll**，不建任何 `CompositionGraphicsDevice`。页面上保留原有的标题、开关、按钮，再加一个**每 250 ms 变一次的计数文字**（数字加上交替的色块），用来判断 XAML 是否还在出新画面。
- 流程：基线截图 → 请 runner 重启一次显卡驱动（与 r2 相同的 `pnputil /restart-device`、自检、间隔和停止规则）→ 驱动恢复后 +5 s、+15 s、+30 s 各截一张 → 依次尝试：改变窗口大小 ±1 px、最小化再还原、重新设置页面内容；每一步之后等 2 s 截一张。
- 每张截图，探针同时记录：计数文字当时应该显示的值、`CompositionTarget.Rendering` 在最近 1 s 内是否还在触发、窗口大小。我在复核时用像素统计判断“标题区、计数区是否有内容，计数是否与应显示值一致”。
- **对照组（N3）**：同一会话流程再用带 Rive 的探针跑一遍（即 RECOVER 的设置），这样“普通窗口”和“带 Rive 窗口”在同一天、同一驱动下直接比较。

## G3 待裁决（N1–N5，附建议）
| # | 问题 | 建议 | 理由 |
|---|---|---|---|
| **N1** | 先做这张，还是按 plan 先做 M3-FOLLOW（协调 next 的主推） | **先做 XAML 对照**；FOLLOW 的 DRAFT 可以同时起草，但真机排在后面 | 如果 WinUI 3 宿主在驱动重启后界面全空，试点应用会直接受影响，这比 FOLLOW 的延迟对比更要紧；这张票只需要一次很短的真机运行（约 15 分钟） |
| **N2** | 规模 | 普通窗口 3 个会话 × 每会话 1 次驱动重启；对照组（带 Rive）1 个会话 × 1 次；加自检共 5 次，≤ 8；间隔和冷却同 r2 | 3 次足以看出是否稳定复现；对照组只需要确认 RECOVER 里看到的现象还在 |
| **N3** | 要不要带 Rive 的对照组 | **要**，在同一次运行里跑 | 驱动、系统更新、显示设置每天都可能变，同一天对比才有说服力 |
| **N4** | 结果怎么用 | 记录型诊断票，标签只取：`XAML-RECOVERS`（普通窗口自己恢复）、`XAML-LOST-REMEDY-<操作>`（某个操作后恢复）、`XAML-LOST`（都不恢复）。若普通窗口也丢 → 判定为平台问题，下一步查 WinUI 已知问题并做宿主侧的规避（例如检测到设备丢失后主动执行有效的那个操作）；若普通窗口不丢而带 Rive 的丢 → 我们的代码有问题，下一步定位 | 现在还不知道该修谁，先让数据回答 |
| **N5** | RECOVER 结题里的事实问题，以及协调副本 | 照 L5、M5 的做法，在 `docs/m3_rive_lost_recover_result.md` 末尾附更正：闸门是 LOCKED `16202071…84ce` 且有 approved（结题写“DRAFT 未 LOCK、无 approved、建议补 LOCK 归档”，不需要补）；补上 XAML 界面消失的发现。正文和 PASS 结论不变。**另请协调侧先同步再写**（brief 已被旧副本覆盖三次） | 防止后面的人以为采数未授权，或漏掉 XAML 问题 |

## G4 实现计划（N 裁决后写，LOCK 前完成）
| 文件 | 改动 |
|---|---|
| `MotionSoftDemo/M3XamlLostProbe.cs`（新） | 计数文字、`CompositionTarget.Rendering` 监测、截图请求、四种恢复尝试；不引用 m0rive |
| `MotionSoftDemo/MainPage.xaml.cs` + `m3/mainpage_m3xaml_hook.patch` | 入口 `--m3-xaml-lost-probe` |
| `m3/run_m3_xaml_lost.ps1`（新） | 复用 `run_m3_lost.ps1` 的自检、注入、间隔和停止规则；按探针请求截图；最后跑 1 个带 Rive 的对照会话（调用已有的 `--m3-lost-probe`，只注入 1 次） |

## G5 禁令
不改 composition.h；不重开 S1/R-3/R-2；禁影子 IV；不改 TDR 注册表；本 DRAFT ≠ 授权采数；诊断结果 ≠ 采用。
