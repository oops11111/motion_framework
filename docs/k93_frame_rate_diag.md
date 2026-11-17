# K93N3P8 回调半刷新率诊断（只读）· Z5 T6

> 只读诊断；不做新计时测量；不改任何系统设置；不采数；PERF-CI HOLD；Success≠adopt；不合 main。
> 「K93 现在」与「Z4 采数时」分开记录，**不得拿现在的状态推当时的结论**。本文只列假设，不下结论。

- 机台：DESKTOP-K93N3P8
- 派工：`workbuddy_z5_wave1_dispatch.md` T6（分支 `wb/z5-t6`，基线 main `2e57bff384c771cc972840ca98a1f0bb92aaeb98`）
- 采集时间：2026-11-18 00:48 AWST（Australia/Perth, UTC+8）；方法：只读 PowerShell 5（CIM / user32 EnumDisplayDevices·EnumDisplaySettings·GetSystemMetrics·SystemParametersInfo 读取 / `powercfg /getactivescheme` / `query session` / WinRT `UISettings` / HKLM 只读读值），原始输出见附录。

## 1. K93 现在（2026-11-18 00:48 AWST 读取的事实）

| 项 | 值 |
|---|---|
| OS | Windows 10 企业版 10.0.19045 |
| 会话类型 | **控制台**：`SESSIONNAME=Console`；`query session` 仅 `console`（admin, ID 1, Active）与 `services`；`GetSystemMetrics(SM_REMOTESESSION)=0`（非 RDP） |
| 显示适配器 | **NVIDIA GeForce GTX 970**（物理 PCI `VEN_10DE&DEV_13C2`），驱动 32.0.15.8278（2026-06-25），Status OK |
| 虚拟 / 软件适配器 | `Get-PnpDevice -Class Display` 只有 GTX 970 一项；**未见** Microsoft Basic Display Adapter / Microsoft Remote Display Adapter / Hyper-V Video |
| 显示器 | 1 台：Samsung `SAM08A2`（WmiMonitorID 名称 S19B150，2012 年），Generic PnP Monitor |
| 当前模式 | `\\.\DISPLAY1`（primary，唯一 attached）**1920×1080 @ 60 Hz**，32 bpp；Win32_VideoController `CurrentRefreshRate=60`（Min/Max 56/75） |
| 显示器列出的刷新率 | 59.79, 59.94, 60, 60.32, 63.75, 67, 70, 70.07, 72.2, 72.81, 75, 75.03 Hz（EDID 源模式表） |
| 电源计划 | `e9a42b02-d5df-448d-aa00-03f14749eb61`（卓越性能 / Ultimate Performance） |
| 硬件加速 GPU 调度 | `HKLM\…\GraphicsDrivers\HwSchMode=1`（关） |
| UISettings.AnimationsEnabled | **True**（WinRT 投影只读）；`SPI_GETCLIENTAREAANIMATION=1` |

## 2. Z4 采数时（只引用 `z4/results_etw_l1_smoke_r1/K93N3P8/` 已有记录，不重测）

| 会话 | 形态 | 记录的 OS | 帧间隔 `frame_dt_ms`（n / min / p50 / p95 / max / mean，ms） | `max_frame_step_dt_ms` | 结论（原记录） |
|---|---|---|---|---|---|
| s2 | 打包（`windows_package_type=msix`，pid 38324） | Microsoft Windows NT 10.0.19045.0 | 90 / 25.123 / 31.320 / 32.832 / 79.317 / 32.440 | 31.3512 | PASS |
| s4 | 非打包（独立构建，pid 38872） | 同上 | 90 / 16.725 / 31.237 / 33.191 / 78.909 / 31.956 | 31.0939 | PASS |
| s5 | 非打包（与 s4 同一输出，pid 21744） | 同上 | 88 / 26.323 / 31.275 / 32.238 / 77.902 / 32.180 | 30.6259 | PASS |

来源：各会话 `verdict.json` 的 `g2.2_continuity.frame_dt_ms`、`session.log` 的 `FRAME_SUMMARY` 行、`env.json`。s1 / s3 为 INVALID（启动即 `0x80040154`，无帧数据）。

**Z4 记录中没有**：显示器 / 刷新率、显示适配器型号、电源计划、会话类型（控制台 / RDP）、`UISettings.AnimationsEnabled`。这些在采数时的值**未知**，不能用上面第 1 节的现值回填。（`session.log` 的 `REDUCED …` 行是减少动效用例本身，不是系统设置读数。）`docs/z4_etw_l1_smoke_r1_closed.md` 已注明帧间隔仅为旁证，非 PERF-CI、不构成帧预算判定。

## 3. 假设清单（只列，不下结论）

| # | 假设 | 现有事实对它的关系（仅指向，不判定） | 需要什么才能区分（本票不做） |
|---|---|---|---|
| H1 | 显示器刷新率本身约 30 Hz（或采数时模式不同） | 现在 60 Hz；采数时未记录 | 采数时的模式记录 |
| H2 | 显示适配器为虚拟 / 软件渲染（Microsoft Remote Display Adapter、Basic Display、Hyper-V Video），回调被限速 | 现在只见物理 GTX 970；采数时未记录 | 采数时的适配器记录 |
| H3 | 会话是 RDP 或远程桌面类会话，DWM 合成节流 | 现在是控制台、`SM_REMOTESESSION=0`；采数时未记录（启动走 `schtasks /IT`，交互会话类型未入档） | 采数时的会话记录 |
| H4 | `CompositionTarget.Rendering` / OnFrame 回调在该机隔一个 vblank 触发一次（应用侧每帧耗时 > 16.7 ms 或 UI 线程被占用导致错过 vblank） | p50 ≈ 31.2–31.3 ms ≈ 2 × 16.67 ms 的形态与此一致，但也与 H1–H3 一致 | 单独开票的计时探针（本票禁止） |
| H5 | 窗口未获前台 / 被遮挡 / 最小化或电源 / 节能策略导致 DWM 降频 | 现电源计划为卓越性能；采数时前台状态与电源计划未记录 | 采数时的窗口与电源记录 |
| H6 | Debug 构建 + EventPipe（dotnet-trace）/ EventListener Verbose FrameTick 的开销使每帧超过一个 vblank | 采数时均为 Debug 且开着 trace；未做对照 | 对照测量（本票禁止） |
| H7 | 驱动 / 硬件加速 GPU 调度（HwSchMode）或 Win10 19045 的 DWM 行为差异（IJI1KJG 为 Win11，其 `max_frame_step_dt_ms` ≈ 5.6 ms，见 `docs/z4_etw_l1_smoke_r1_closed.md`） | 现 HwSchMode=1（关）；采数时未记录 | 采数时记录 + 对照 |
| H8 | 系统动画设置（`UISettings.AnimationsEnabled` / 客户区动画）影响回调频率 | 现在 True / 1；采数时未记录 | 采数时记录 |

## 4. 边界

- 本票未做任何新计时测量，未改电源 / 显示 / 系统设置，未安装任何东西。
- 本文件不改变 Z4 r1 结论（仍绑定 fe59c622，CLOSED PASS），也不构成 PERF-CI 或帧预算判定。

## 附录 · 只读采集原始输出（2026-11-18 00:48 AWST）

```text
# T6 read-only facts, collected 2026-11-18 00:48:47 (Australia/Perth UTC+8) on DESKTOP-K93N3P8
## OS
Caption=Microsoft Windows 10 企业版 Version=10.0.19045 Build=19045
## Session
SESSIONNAME=Console
query session:
   SESSIONNAME       USERNAME                 ID  STATE   TYPE        DEVICE 
   services                                    0  Disc                        
  >console           admin                     1  Active                      
GetSystemMetrics(SM_REMOTESESSION=0x1000):
  SM_REMOTESESSION=0
## Display devices (EnumDisplayDevices + EnumDisplaySettings ENUM_CURRENT_SETTINGS)
  [0] \\.\DISPLAY1 'NVIDIA GeForce GTX 970' attached=True primary=True flags=0x5 id=PCI\VEN_10DE&DEV_13C2&SUBSYS_13C210DE&REV_A1 mode=1920x1080@60Hz bpp=32
      monitor[0] 'Generic PnP Monitor' flags=0x3 id=MONITOR\SAM08A2\{4d36e96e-e325-11ce-bfc1-08002be10318}\0005
  [1] \\.\DISPLAY2 'NVIDIA GeForce GTX 970' attached=False primary=False flags=0x0 id=PCI\VEN_10DE&DEV_13C2&SUBSYS_13C210DE&REV_A1
  [2] \\.\DISPLAY3 'NVIDIA GeForce GTX 970' attached=False primary=False flags=0x0 id=PCI\VEN_10DE&DEV_13C2&SUBSYS_13C210DE&REV_A1
  [3] \\.\DISPLAY4 'NVIDIA GeForce GTX 970' attached=False primary=False flags=0x0 id=PCI\VEN_10DE&DEV_13C2&SUBSYS_13C210DE&REV_A1
## Win32_VideoController
  Name=NVIDIA GeForce GTX 970 | Compat=NVIDIA | Driver=32.0.15.8278 (06/25/2026 08:00:00) | Status=OK | CurrentRefreshRate=60 | Min/Max=56/75 | Mode=1920 x 1080 x 4294967296 种颜色 | PNP=PCI\VEN_10DE&DEV_13C2&SUBSYS_13C210DE&REV_A1\4&31024D2F&0&0018 | AdapterRAM=4293918720
## Win32_DesktopMonitor
  Name=通用即插即用监视器 | Type=通用即插即用监视器 | Mfr=(标准监视器类型) | PNP=DISPLAY\SAM08A2\5&1B8A367B&3&UID12545 | Status=OK | Avail=8
## WmiMonitorID (root\wmi)
  Instance=DISPLAY\SAM08A2\5&1b8a367b&3&UID12545_0 Mfr=SAM Name=S19B150 Active=True Year=2012
## WmiMonitorListedSupportedSourceModes max refresh (root\wmi)
  DISPLAY\SAM08A2\5&1b8a367b&3&UID12545_0 refresh rates listed: 59.79, 59.94, 60, 60.32, 63.75, 67, 70, 70.07, 72.2, 72.81, 75, 75.03
## Power
  电源方案 GUID: e9a42b02-d5df-448d-aa00-03f14749eb61  (卓越性能)
## UISettings.AnimationsEnabled (WinRT projection, PS5)
  AnimationsEnabled=True
  SPI_GETCLIENTAREAANIMATION=1
## Hardware-accelerated GPU scheduling (HKLM read-only)
  HwSchMode=1 (2=on,1=off)
## Remote Display adapter PnP presence (Get-PnpDevice -Class Display)
  OK | NVIDIA GeForce GTX 970 | PCI\VEN_10DE&DEV_13C2&SUBSYS_13C210DE&REV_A1\4&31024D2F&0&0018
```
