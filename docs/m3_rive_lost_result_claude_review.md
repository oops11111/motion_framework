# M3-RIVE-LOST 结果复核（Claude）

> 权威结题：`docs/m3_rive_lost_result.md`（notion ai，RIVE-LOST-INCONCLUSIVE，非 METRICS_PASS，未采用）。本文件是独立复核，不改权威结题。
> 依据：`m3/results_lost/`（tip `82a53cc`）、`docs/m3_rive_lost_gate_LOCKED.md`、`docs/m3_rive_lost_approved.md`、代码 `23e08de`。

## 结论
- **同意**：非 PASS、未采用、恢复路径没有被检验到（既没证实也没证伪）、不当日重跑。
- **需要更正三处事实**（不影响“非 PASS”）：
  1. **闸门已 LOCK 并 APPROVED。** 权威结题写“本地无 LOCKED / approved / hash”，是因为协调侧的本地副本是旧的（初始导入 `137c52e`）。分支上 `docs/m3_rive_lost_gate_LOCKED.md`（`3450135`）和 `docs/m3_rive_lost_approved.md`（`b1e1713`）都早于结果提交 `82a53cc`。在 tip 上复算 GATE_BODY_SHA256 = `753c0a6c82f06c81cf741eecba278b926127771befa2a51b515a925a7873f27d`，**MATCH**。采数有授权。
  2. **应用没有崩溃。** `CRASHED_NO_VERDICT` 是 runner 的标注错误：runner 在第 30 s 先 `Stop-Process` 结束了应用，再用 `$p.HasExited` 判断“是否崩溃”，所以被自己结束的进程也标成了崩溃。证据：检测等待循环在应用退出时会立即跳出，而日志显示它等满了 30 s（17:35:38 → 17:36:08）；日志里也没有 `app exited early`。summary 停在 seg0 之后，与“探针还在等待检测”一致。
  3. **设备丢失根本没有发生。** `dxcap -forcetdr` 退出码 0、没有任何输出，但系统日志里没有 4101 事件（`event4101=0`），探针也没检测到丢失。也就是说，注入命令执行了，但没有产生 TDR。
- **按 LOCKED 闸门 G2 的口径，本次属于 INVALID**：“注入命令执行了但探针 30 s 内没检测到丢失（该次作废，runner 停止后续注入）”。权威结题取 INCONCLUSIVE，两者都是“非 PASS、没有结论”，差别只在标签。是否改标签由 pidan 决定（见 r2 DRAFT 的 L5）。

## 已验证的部分
| 项 | 结果 |
|---|---|
| 构建 | BUILD_OK（`build_m0.log`） |
| 模拟冒烟 | SIM_SMOKE=PASS（执行方回报；按派工单不提交数据）：进程内模拟丢失 → 完整恢复路径跑通 |
| 提权 / dxcap | elevated=True；`C:\WINDOWS\system32\DXCap.exe` 存在 |
| 基线段 seg0 | 587 帧，0 失败；CPU P95 1.186 ms（0.089·T），GPU P95 0.209 ms（0.016·T），丢弃 0；与 DPI 票一致 |
| TDR 注册表 | 全部默认值，未改 |
| composition.h | 未改。LF `a55d4952…e418`；Windows 检出时为 CRLF，同一文件哈希 `33d826c7…6e97`（本地按 LF→CRLF 复算一致） |
| 停止规则 | runner 在 30 s 未检测时停止后续注入，只注入 1 次，符合 K5 |

## 为什么 dxcap 没有触发 TDR（推测，未证实）
- `-forcetdr` 依赖“图形工具”与系统内核图形栈的配合。这台机器是 Windows 11 **build 26300**（预览通道），`DXCap.exe` 是否在这个版本上仍然有效，没有公开资料可查。
- 退出码 0、没有输出、没有 4101 事件，说明命令“成功返回”但没起作用，不是权限问题。

## 本次暴露的 runner 问题（r2 修）
1. 判定崩溃前先结束了进程（上面第 2 点）。
2. 没有在注入后马上查 4101 事件，所以“命令执行了但没注入”要等 30 s 超时才知道，而且没有在日志里直接写出“没有 4101 事件”。
3. 没有在会话之前单独验证注入手段是否有效。

## 下一步
见 `docs/m3_rive_lost_gate_r2_DRAFT.md`（换注入手段、修 runner、增加注入自检）。
