# M3-XAML-LOST 结果复核（Claude）

> 依据：`m3/results_xaml_lost/`（提交 `efce219`，72 个文件）、`docs/m3_xaml_lost_gate_LOCKED.md`、`docs/m3_xaml_lost_approved.md`、代码 `4c04450`。协调结题 `docs/m3_xaml_lost_result.md` 标为 `XAML-LOST-PASS`，本复核**不同意这个标签**（见下）。

## 授权核验
| 项 | 结果 |
|---|---|
| 闸门 | `docs/m3_xaml_lost_gate_LOCKED.md`，GATE_BODY_SHA256=`69a52e2de66153348bff5197dbb037dd2650c26055b834173abde6ae6a1e1f55`，在 `efce219` 上复算 MATCH |
| approved | `docs/m3_xaml_lost_approved.md`（pidan @ 2026-10-30 13:42:56 AWST），早于采数（16:53）。协调结题写“本地无闸门、无 approved、按用户指令采数”，又是本地副本过旧 |
| 执行方提交 | 只新增 `m3/results_xaml_lost/`，没改 `docs/` |
| composition.h | PRE=POST |
| 注入 | 自检 OK（1.6 s）+ 4 次会话注入 = 5 次；全部 `pnputil` 退出码 0、30 s 内回到 OK |

## 结论：`XAML-LOST`，且与 Rive 无关（平台问题）
按 LOCKED 的 N4，本票只取 `XAML-RECOVERS` / `XAML-LOST-REMEDY-<操作>` / `XAML-LOST` 三种标签之一。数据给出的是 **`XAML-LOST`**：普通窗口（不加载 Rive、不建任何合成图形设备）3/3 会话在驱动重启后界面内容丢失，等 30 s、改窗口大小、最小化再还原、重设页面内容都没有恢复。带 Rive 的会话（s4）表现完全一样。所以**问题不是我们的代码引起的**，是 WinUI 3 在这套环境下没有从显卡驱动重启中恢复。

协调结题的 `XAML-LOST-PASS` 是按 s4 里 Rive 的恢复（`recovered=1`，2004.58 ms）判的，没有看 XAML 截图；Rive 恢复在 RECOVER 票已经验证过，不是本票的问题。

## 像素统计（四个会话一致）
| 观测点 | 标题区亮像素 | 计数文字区亮像素 | AnimBox 蓝色像素 | 纯色方块 |
|---|---|---|---|---|
| base（重启前） | 2020 | 263（s4：153） | 约 14010（s4 被 Rive 遮住：0） | 正常 |
| t05 / t15 / t30 | 0 | 0 | 0 | **仍在红绿交替** |
| resize / minrestore / content | 0 | 0 | 0 | **仍在红绿交替** |

- **XAML 没有停：** 每个观测点 `CompositionTarget.Rendering` 都是 75–76 次/秒，计数器一直在走（s1：7 → 37 → 75 → 133 → 177），纯色 `Border`（只有颜色画刷）也一直在按计数换颜色。
- **丢的是需要 XAML 自己光栅化的内容：** 所有文字（标题、计数数字、开关和按钮的文字）、圆角的 AnimBox、开关和按钮本身、标题栏的图标和标题。重启后新设置的文字（计数器每 250 ms 改一次）和“重设页面内容”后重新创建的元素同样画不出来。
- **推断（未证实）：** XAML 内部用来把文字、圆角等光栅化成表面的那个图形设备，在驱动重启后没有被重建；纯色矩形不需要这种表面，所以不受影响。这是整个进程层面的问题，换元素、重排版都救不回来。

## 环境限定
WinAppSDK 2.5.1；**unpackaged + WindowsAppSDKSelfContained**（执行方本机的 csproj 补丁）；Windows 11 **build 26300（预览通道）**；AMD RX 7800 XT，驱动 32.0.12033.1030；注入方式是 `pnputil /restart-device`。还不知道换成打包部署、正式版 Windows、其他驱动或真实 TDR 时是否一样。网上检索没有找到 microsoft-ui-xaml 里与此完全对应的已知问题记录（只找到 Win2D 关于设备丢失的通用说明：丢失后必须新建设备并重建所有图形资源）。

## 对框架的影响
- Rive 部分已能自恢复（RECOVER）；但宿主应用的 XAML 界面在驱动更新或驱动重启后会“只剩纯色块和 Rive”，这会直接影响试点。框架本身修不了 XAML，只能**检测到设备丢失后在宿主层面补救**（例如重建窗口或提示重启应用），或等 WinUI 修复。
- 建议的下一步见 brief 和会话中的决策项（Q1–Q4）。
