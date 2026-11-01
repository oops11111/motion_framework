# M3-XAML-REBUILD 结果复核（Claude）

> 依据：`m3/results_xaml_rebuild/{a,b,c}/`（提交 `d00ffd0`）、`docs/m3_xaml_rebuild_gate_LOCKED.md`、`docs/m3_xaml_rebuild_approved.md`、代码 `12c720d`。协调结题 `docs/m3_xaml_rebuild_result.md`（`XAML-REBUILD-PASS`，R4 = KNOWN_LIMIT）这次对照了正确的闸门和 approved，**逐窗口的判读与本复核一致**。下面只补三点。

## 授权核验
闸门 `3ea74c37…56fdc` 在 `d00ffd0` 上复算 MATCH；approved（pidan @ 2026-11-01 16:26:53 UTC+8）早于采数（C 17:30:16、A 17:30:30、B 17:45:44，均 UTC+8）；composition.h PRE=POST；全部注入 `pnputil` 退出码 0，显卡 30 s 内回到 OK（K93 自检 12.5 s，其余 1.9–3.3 s）。

## 结论（同意协调）
- **A（IJI1KJG，unpackaged，3/3）**：重启后主窗口只剩纯色块（亮像素正好 6400 = 80×80 色块），**W_pre 和 W_post 全黑**（亮像素 0）——连按钮、圆角块都没有。重启后新建的窗口同样画不出任何东西，所以丢失是**整个进程**的，“检测到设备丢失后重建窗口”不能补救。
- **B（IJI1KJG，打包）**：t05 时主窗口和 W_pre 同样为空 → 打包部署**一样**。t10、t30 的截图被其他窗口遮挡，不可用（见下）。
- **C（K93N3P8，Win10 19045 / GTX 970，打包）**：主窗口在 t05、t30 **完整**（标题、开关、按钮、AnimBox，计数器 9 → 41 → 139）。W_pre / W_post 截图位置错了，无法判读。
- R4 结论：在 IJI1KJG 这类环境里写成**已知限制，建议重启应用**。

## 补充 1：C 的“恢复”还不能确定是 WinUI 恢复了
C 是普通会话，不加载 Rive，所以**没有直接证据表明这次驱动重启真的让应用的设备丢失了**。主窗口一直正常，有两种可能：WinUI 在这台机器上能正确恢复；或者这台机器上 `pnputil /restart-device` 没有让应用的图形设备失效（例如驱动以不同方式重启）。要区分，需要在 K93N3P8 上带 Rive 跑一次（Rive 的 `lost_detected` 能证明设备确实丢了）。在此之前，“问题只出现在 IJI1KJG 环境”只是推断。

## 补充 2：截图方式的两个缺陷（runner）
1. **按屏幕像素复制**（`CopyFromScreen`）会把盖在应用上面的窗口一起截进来。B 的 t10、t30 共 7 张被执行方的聊天软件、系统通知和 dotnet 控制台遮挡。
2. **PowerShell 进程不感知 DPI**：K93N3P8 的缩放是 125%，`GetWindowRect` 返回的是虚拟化后的坐标，C 的 W_pre / W_post 全部截到了错误的位置（截到了资源管理器的一部分）。
修法：改用 `PrintWindow(hwnd, PW_RENDERFULLCONTENT)` 按窗口取内容（不受遮挡影响），并在 runner 开头把进程设为 Per-Monitor DPI aware。

## 补充 3：仓库里有执行方的私人内容（需要 pidan 决定）
B 的以下截图拍到了执行方桌面上的聊天软件和通知，包括 pidan 的消息原文、会话列表里的人名和一个邮箱地址的前半部分：
`b/m3xrb_s1_t10_wpost_a.png`、`b/m3xrb_s1_t30_main_a.png`、`b/m3xrb_s1_t30_main_b.png`、`b/m3xrb_s1_t30_wpost_a.png`、`b/m3xrb_s1_t30_wpost_b.png`、`b/m3xrb_s1_t30_wpre_a.png`、`b/m3xrb_s1_t30_wpre_b.png`。
（`b/m3xrb_s1_t10_wpost_b.png` 是任务栏预览，C 的 W_pre / W_post 截到了几个文件夹名，敏感度低。）
这些图对判读没有用处。建议从分支删除；但删除后仍留在 git 历史里，要彻底清除需要改写历史并强制推送，影响所有已拉取的副本。

## 建议的下一步
见会话中的决策项（S1–S4）。
