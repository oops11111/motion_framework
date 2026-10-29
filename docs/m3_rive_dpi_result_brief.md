# M3-RIVE-DPI 结题简报（协调侧）

STATUS: METRICS_PASS（成功 ≠ 采用；采用/下一票交 Notion）
CHECKED_AT: 2026-10-29 Asia/Shanghai
BRANCH: claude/m3-rive-dpi
TIP: 67decd39b3824b390740461a356def44b4e47614
GATE: 92e6b4e9…97bf5 @ 2026-10-28 20:39:58 AWST；approved 齐
HOST: DESKTOP-IJI1KJG；scales 100↔125
composition.h: PRE=POST a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418

## ONELINE
M3-RIVE-DPI | commit=67decd39b3824b390740461a356def44b4e47614 | build=BUILD_OK | sessions=3 | switches=4/4/4 | size_ok=1/1/1 | reraster_max_ms=12.31/11.16/13.33 | budget_ok/stability_ok=1/1|1/1|1/1 | mem_delta_mb=2/0/2 | composition.h PRE=POST a55d4952…e418

## 闸门对照（G2）
| 判据 | 结果 |
|---|---|
| ① 切换≥4 | 4/4/4 PASS |
| ② 尺寸 0px 误差 | size_ok=1/1/1；400↔500 PASS |
| ③ 重光栅 P95≤100ms | max 12.31/11.16/13.33（只记录） |
| ④ 稳定性 | draw_failures=0；device_removed=0；每段帧≥586 PASS |
| ⑤ 预算 | CPU/GPU P95 frac_T 均 ≪0.35；gpu_dropped=0 PASS |
| ⑥ 清晰度 | 协调目视：100%/125% 窗口截图与 ref（400²/500²）均为清晰矢量重绘，非模糊缩放 → PASS |
| ⑦ 内存 | delta 2/0/2 MB ≤20（只记录） |

**ticket 判据 PASS**（①②④⑤⑥ 全会话满足）。

## Lab 注记
首轮×3 INVALID_NO_SUMMARY：REGDB_E_CLASSNOTREG（DeploymentManager AutoInitialize）。
lab 修（非改 Rive）：MotionSoftDemo.csproj 加 `WindowsPackageType=None` + `WindowsAppSDKSelfContained=true` 后重跑。tip 仅含 `m3/results_dpi/` + lab note；csproj 是否回灌分支待采用决断时处理。

## 禁令核对
不改 composition.h；不重开 S1/R-3/R-2；成功≠采用。
