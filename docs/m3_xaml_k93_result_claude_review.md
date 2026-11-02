# M3-XAML-K93 结果复核（Claude）

> 依据：`m3/results_xaml_k93/`（提交 `bfd76c3`）、`docs/m3_xaml_k93_gate_LOCKED.md`、`docs/m3_xaml_k93_approved.md`、`docs/notion_m3_xaml_k93_toolchain_decision.md`、代码 `104e5f9`。协调结题 `docs/m3_xaml_k93_result.md`（`XAML-K93-PASS`）缺 TEXT 目视判读，本复核补上。

## 授权与执行核验
| 项 | 结果 |
|---|---|
| 闸门 | `22fb8b92…b657`，在 `bfd76c3` 上复算 MATCH（协调结题写“本机未见 LOCK 文件”，文件在分支上） |
| approved | pidan @ 2026-11-01 20:58:26 AWST，早于采数（2026-11-02 09:12:36 UTC+8） |
| 构建路径 | **偏离 G3**：K93N3P8 本机构建失败（`BLOCKED_TOOLCHAIN`），按协调决定（Decision C）在 IJI1KJG 上构建后复制过去（`IJI_BYPASS_COPY`，DLL sha256 `70ed8d14…a15f`）。代码与闸门锁定的提交一致，只是编译地点不同，**不影响测量有效性**；记为执行偏离 |
| 注入 | 自检 + 会话共 2 次，`pnputil` 退出码 0，3.1–3.6 s 回到 OK；0 BugCheck |
| composition.h | PRE=POST |
| 截图 | S1 修正生效：每张都是窗口自身内容，没有截到其他窗口或桌面 |

## 判读（按 LOCKED G2 表第一行）
1. **设备确实丢失了**：Rive 检测到 `0x887A0005`（DXGI_ERROR_DEVICE_REMOVED），`lost_detected=1`、`recovered=1`，恢复 4504.25 ms（前两次尝试时还没有硬件适配器，第 3 次在 GTX 970 上建好设备，再经 500 ms 稳定期；比 IJI1KJG 的 2.1–2.3 s 慢，仍在 RECOVER 的 5 s 上限内）。
2. **XAML 全部能画**：t05、t10、t30 的主窗口（标题、开关、按钮、AnimBox、计数器 38 → 65 → 136）、W_pre（“W_pre” + 计数 39 → 66 → 137、按钮、圆角块）、W_post（“W_post” + 计数 67 → 137、按钮、圆角块）**全部 TEXT-OK**，计数器持续前进；Rive 画面在主窗口中正常恢复。
3. **结论**：同一份代码在 Windows 10 19045 + NVIDIA GTX 970 上，设备丢失后 XAML 正常恢复；在 Windows 11 build 26300（预览通道）+ AMD RX 7800 XT 上不恢复。**XAML-LOST 是 IJI1KJG 这类环境的问题，不是 WinUI 3 普遍的问题。** 按 S4，应向 Windows 反馈中心（预览版）或 AMD 报告；目前还无法区分是系统版本还是显卡驱动造成的。

## 仍未知
- 是 Windows 11 预览版 26300 还是 AMD 驱动 32.0.12033.1030 引起的：需要一台 Windows 11 正式版 + AMD，或 Windows 11 预览版 + NVIDIA 的机器才能分开。
- 打包与否在 IJI1KJG 上已证明无关（REBUILD B 组）。
