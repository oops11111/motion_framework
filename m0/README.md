# M0 技术验证

闸门：`docs/m0_tech_validation_gate_DRAFT.md`。LOCK 并 APPROVED 之前不得在真机上运行 `run_m0.ps1`。

## 组成

| 路径 | 作用 |
|---|---|
| `native/m0_tear_probe.cpp` | M0-4：合成交换链和 HWND 交换链分别测 `ALLOW_TEARING` |
| `native/m0rive/` | M0-1：Rive（D3D11）渲染进 WinUI 3 合成表面或合成交换链；M0-2 的标记块 |
| `../MotionSoftDemo/M0Probe.cs` | WinUI 宿主：M0-1a、M0-1b、M0-2、M0-3/5（`--m0-probe` 或 `MF_M0_PROBE=1`） |
| `build_m0.ps1` | 拉取钉住版本的 rive-runtime 并构建，再构建 DLL 和探针，暂存测试素材 |
| `run_m0.ps1` | 先跑 M0-4，再跑 N 个独立的 MotionSoftDemo 会话，并截图 |

## 在实验室机器上

```powershell
# 1. 构建（需要 VS 2022 C++ 工具、Windows SDK、Git for Windows、github.com 网络）
powershell -ExecutionPolicy Bypass -File m0\build_m0.ps1 -DemoBinDir <MotionSoftDemo.exe 所在目录>
# 2. 按平常方式重新构建 MotionSoftDemo（已加入 M0Probe.cs）
# 3. 采数（闸门 APPROVED 后）；只有 D1 获批才加 -Inject
powershell -ExecutionPolicy Bypass -File m0\run_m0.ps1 -DemoExe <...\MotionSoftDemo.exe> -Inject -Sessions 3 [-PresentMon <...\PresentMon.exe>]
```

启动方式：`-LaunchMode Package`（默认）用 `dotnet run` 加 MSIX 启动配置（M0-A 时的 DESKTOP-K93N3P8）；`-LaunchMode Exe` 直接启动 `-DemoExe`，通过环境变量和 `--m0-probe --m0-inject` 传参（M0-B r3 时的 DESKTOP-IJI1KJG，那台机器没有打包启动配置，用的是 Debug 版的输出）。传了 `-PresentMon` 时，PresentMon 会跟随撕裂探针进程退出（`--terminate_on_proc_exit`），runner 再等它写完 CSV，不再固定 60 秒。这个参数来自 PresentMon 2.x 的命令行，第一次使用时请确认 `m0_run.log` 里的 `PresentMon rows=` 不为 0。

产物都写到 `C:\Users\admin\motion_framework\artifacts`，包括：`m0_s{n}_summary.txt`（key=value）、`m0_s{n}_raw.txt`、`m0_s{n}_screen_m0_1a.png`、`m0_s{n}_rive_ref_t0500.bgra`、`m0_session_matrix.txt`、`m0_tear\m0_tear_summary.txt`、`m0_tear\m0_tear_frames.csv`，以及可选的 `m0_tear\presentmon.csv`、`m0_run.log`。

## 已知限制
- `m0rive.dll` 只在 Linux 上对着 Rive 头文件做过语法检查，没有链接过。第一次在 Windows 上构建时，最可能出问题的是 Rive 依赖库的链接列表（脚本会链接 `tests\out\release\*.lib` 下的全部库）。
- 构建 DLL 时必须使用 Rive 的默认构建设置（/MT、`WITH_RIVE_TEXT`、`WITH_RIVE_LAYOUT`、`RIVE_CANVAS`）。改动 Rive 构建参数后，必须同步修改 `build_m0.ps1` 中的宏定义，否则会出现运行时内存布局不一致。
- WGC 跟踪的是 AnimBox 的颜色（mfprobe 原生库）。M0-2 让 Rive 表面里画一个同色标记块，并在测量期间隐藏 AnimBox。
- 延迟的计时起点是 `SendInput` 被调用的时刻，没有包含鼠标硬件本身的延迟。
