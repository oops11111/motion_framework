# M3-RIVE-LOST 门槛闸门 r2（m3_rive_lost_gate_r2_DRAFT.md）

STATUS=DRAFT（**NOT LOCKED**；本文件**不授权采数**）
LOCKED_AT: （LOCK 时填）
GATE_BODY_SHA256: （LOCK 时算；约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希）

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> 起草：Claude，2026-10-29。取代 r1 `docs/m3_rive_lost_gate_LOCKED.md`（`753c0a6c…f27d`；r1 文件不改，r1 结论按权威结题 `docs/m3_rive_lost_result.md` 保留）。
> 原因：r1 真机运行中 `dxcap -forcetdr` 退出码 0 但没有产生 TDR（系统日志 0 条 4101，探针未检测到丢失），恢复路径没被检验到；另有 runner 把“自己结束的进程”标成崩溃的错误。详见 `docs/m3_rive_lost_result_claude_review.md`。
> 分支：`claude/m3-rive-lost`（r1 的代码和结果都在这里）。

## G0 硬边界（同 r1）
不改 `composition.h`；不重开 S1/R-3/R-2；禁影子 IV；不解冻 Bv；PERF-CI 仍 HOLD；不改 TDR 注册表和显卡驱动设置；未 LOCK 且 APPROVED_* 为空时禁止采数；成功 ≠ 采用 ≠ 默认可合入 main；本机 csproj 补丁不进仓库。

## G1 / G2：与 r1 相同，只改“怎样让设备丢失”
- 要验证的内容（G1）和判据 ①–⑦、PASS/FAIL/INVALID 口径（G2）**全部沿用 r1**，r1 的 K2、K3、K6、K7 裁决继续有效。
- 只把 r1 的 K1（注入手段）、K4/K5 的部分细节换成下面的 L1–L4；`event4101` 不再作为注入证据（见 L1）。
- 结果目录改为 `m3/results_lost_r2/`，不与 r1 数据混用。

## G3 待裁决（L1–L5，附建议）
| # | 问题 | 建议 | 理由 |
|---|---|---|---|
| **L1** | 用什么让设备丢失 | **重启显卡驱动**：管理员执行 `pnputil /restart-device "<显卡实例 ID>"`（Win10 2004 起系统自带；实例 ID 由 runner 用 `Get-PnpDevice -Class Display` 取得）。所有 D3D 设备会收到 `DXGI_ERROR_DEVICE_REMOVED`。注入证据改为：pnputil 退出码 + 显卡设备状态在重启后回到 OK + 探针记录的 removed reason。真正的 GPU 挂起（TDR）作为遗留项，不在本票 | 确定能让设备丢失，而且对应最常见的真实场景（驱动更新、驱动崩溃后重启）。我们的恢复路径不区分丢失原因，所以换手段不影响要验证的内容。备选 a：继续用 `dxcap -forcetdr`（这台 build 26300 上已证实无效）；备选 b：自己写一个死循环计算着色器触发真 TDR（RDNA3 支持计算抢占，可能根本触发不了 TDR，又要多一轮试错） |
| **L2** | 采数前要不要先单独验证注入手段 | **要。** runner 在第 1 个会话前做一次“注入自检”：不启动应用，执行一次 L1，等 30 s，确认 pnputil 退出码 0、显卡状态回到 OK、屏幕恢复。自检不过就停止，记 `BLOCKED_INJECT`，不进会话。自检算在次数上限里 | r1 的问题是注入手段本身无效，却要等到会话里 30 s 超时才发现。先自检，失败时也不会白白占用会话 |
| **L3** | runner 怎么改 | ① 先读 summary 并判断进程是否**自己**退出，再结束进程；被 runner 结束的标 `KILLED_BY_RUNNER`，只有自己退出的才标 `CRASHED_NO_VERDICT`。② 每次注入后立即记录 pnputil 输出、退出码和显卡设备状态。③ 探针和 summary 中的注入方式字段改为 `pnputil-restart` | 修 r1 暴露的标注错误，并让“注入是否生效”在日志里一眼可见 |
| **L4** | 次数、间隔、停止条件 | 沿用 r1：3 个会话 × 每会话 2 次；两次间隔 ≥ 45 s；会话之间 ≥ 120 s；全程 ≤ 8 次（含 L2 自检 1 次，共 7 次）。停止条件同 r1 K5，另加：显卡设备状态 30 s 内没回到 OK 也立即停止，记 `INVALID_SYSTEM` | 驱动重启不是 TDR，不受“60 s 内 5 次蓝屏”的限制，但间隔仍留给显示恢复；保持和 r1 一致便于比较 |
| **L5** | r1 那次运行的标签 | **按 r1 LOCKED G2 记为 INVALID**（“注入命令执行了但探针 30 s 内没检测到丢失”），并在权威结题后附更正：闸门已 LOCK 且 APPROVED（hash 复算 MATCH）；应用没有崩溃（`CRASHED_NO_VERDICT` 是 runner 标注错误）。INCONCLUSIVE 和 INVALID 都是非 PASS，结论不变 | 标签与 LOCK 时写死的口径一致；三处事实更正避免后面的人误以为“未授权采数”或“Rive 崩溃了” |

## G4 实现计划（L 裁决后写，LOCK 前完成）
| 文件 | 改动 |
|---|---|
| `m3/run_m3_lost.ps1` | L1 注入（取显卡实例 ID；`pnputil /restart-device`；记录输出、退出码、设备状态）；L2 自检；L3 标注修正；L4 停止条件；保留 `-Simulate` |
| `MotionSoftDemo/M3LostProbe.cs` | 只改 `mode` 字段和提示文字（`pnputil-restart`）；协议（`inject<k>.req` / `inject<k>.detected=1`）不变 |
| `m0/native/m0rive/*` | **不改**（恢复路径与丢失原因无关；r1 模拟冒烟已跑通） |

## G5 禁令
同 r1；另：r1 数据（`m3/results_lost/`）不改不删，不与 r2 混用；本 DRAFT ≠ 授权采数。
