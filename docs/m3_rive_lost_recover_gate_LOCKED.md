# M3-RIVE-LOST-RECOVER 门槛闸门（m3_rive_lost_recover_gate_LOCKED.md）

STATUS=LOCKED
LOCKED_AT: 2026-10-30 11:31:47 AWST（Australia/Perth）
GATE_BODY_SHA256: 16202071fd08d091d0a0d825eda2297297ae19227d72a6cf981d4ed8b25484ce
> GATE_BODY_SHA256 计算约定：对落盘全文做 SHA-256，**排除**以 GATE_BODY_SHA256 开头的那一行；LOCKED_AT 计入哈希。本行写入后不得再改 body。

**本 LOCK 不授权采数。** 须另有 approved 留证（APPROVED_* 非空）后才可采数；构建和 `-Simulate` 本地冒烟不采数，可先行。

APPROVED_BY: ＿＿＿＿
APPROVED_AT: ＿＿＿＿

> 起草：Claude，2026-10-30。DRAFT：`docs/m3_rive_lost_recover_gate_DRAFT.md`。接 r2（`docs/m3_rive_lost_gate_r2_LOCKED.md`，`dc0770c1…9d10`，CLOSED RIVE-LOST-FAIL）。r2 的注入手段已经证实有效，三次注入都被检测到，但恢复过程中 Rive `abort()`。分析见 `docs/m3_rive_lost_r2_result_claude_review.md`。票名沿用协调 next 的建议。
> 分支：`claude/m3-rive-lost`。结果目录 `m3/results_lost_recover/`，不与 r1、r2 混用。

## G0 硬边界（同 r2）
不改 `composition.h`；不重开 S1/R-3/R-2；禁影子 IV；不解冻 Bv；PERF-CI 仍 HOLD；不改 TDR 注册表和显卡驱动设置；未 LOCK 且 APPROVED_* 为空时禁止采数；成功 ≠ 采用 ≠ 默认可合入 main；本机 csproj 补丁不进仓库。

## G1 / G2：沿用 r2
要验证的内容、判据 ①–⑦、PASS/FAIL/INVALID 口径、注入手段（`pnputil /restart-device`）、自检、次数和间隔、停止条件全部沿用 r2（以及 r1 的 K2、K3、K6、K7）。本票只改 DLL 里的恢复过程和诊断。

## G3 裁决（pidan：M1–M5 全部按建议）
| # | 问题 | 裁决（= 原建议） | 理由 |
|---|---|---|---|
| **M1** | 诊断：下次 abort 时要拿到什么 | ① `m0r_set_abort_note` 同时把进程的 stderr 重定向到 `<前缀>_stderr.txt`，Rive 的 `VERIFY_OK` 会把出错的源码行、HRESULT 和调用写进去；② 渲染线程在恢复的每一步更新一个“阶段”标记（检测 / 释放旧对象 / 建设备 / SetRenderingDevice / 建 Rive 上下文 / 导入文件 / 建渲染目标 / 第一帧），abort 时一并写进 summary（`abort_stage=`）；③ 每次尝试记录新设备所在适配器的名称、VendorId 和 LUID | r2 只知道“abort 了”，不知道在哪一步、为什么。加上这些，任何结果都能直接定位 |
| **M2** | 恢复前先确认新设备可用 | 检测到丢失后：① 每次尝试都新建 DXGI 工厂并**显式选硬件适配器**（跳过 VendorId 0x1414 的 Microsoft 基本显示 / 基本渲染适配器；没有就等下一次）；② 在这个适配器上建设备后，先用我们自己检查返回值的调用建一张小纹理和一个缓冲，并检查 `GetDeviceRemovedReason`；③ 设备要**连续 500 ms 保持正常**才交给 Rive 和 `SetRenderingDevice`；④ 不合格就丢掉这个设备，100 ms 后重试，上限仍是 r1 的 5 s 判据（判据不变，超时照常判 FAIL） | 最可能的 abort 原因是驱动重启过程中就在新设备上建 Rive 对象。这些检查只改我们自己的代码，不动 Rive；“500 ms 稳定”大约会让恢复时间多 0.5 s，仍远在 5 s 上限内 |
| **M3** | 要不要现在就给 Rive 打补丁 | **本票先不改 Rive**，只做 M1 + M2。如果本票仍然 abort，M1 的诊断会指出具体位置，下一张票再给 Rive 打补丁：`VERIFY_OK` 改为抛异常，由我们在恢复时捕获并重试。这需要用 `--with-exceptions` 重新编译 Rive（目前 Rive 编译时关闭了异常），改动和风险都更大 | 这与 K7 当时的说法（“再崩溃就下一张票专做 Rive 补丁”）不同，所以需要你确认。理由：r2 的证据指向“时机不对”，而不是 Rive 本身扛不住；先用小改动验证这个判断，比直接重编 Rive 便宜。产品化时 Rive 补丁仍然需要（任何时候建资源失败都不应该让应用退出），记为 WP1 待办 |
| **M4** | 采数安排 | 同 r2：自检 1 次 + 3 个会话 × 每会话 2 次，间隔 ≥ 45 s，会话前冷却 ≥ 120 s，全程 ≤ 8 次；先跑 1 个会话的模拟冒烟 | 与 r2 可直接比较 |
| **M5** | r2 结题里的事实错误怎么处理 | 与 r1 的 L5 相同：在 `docs/m3_rive_lost_r2_result.md` 末尾附更正（闸门是 r2 LOCKED `dc0770c1…9d10` 且有 approved；“PROBE 闸门 7afbcd98” 不在仓库；r1 标签已按 L5 更正为 INVALID），正文不改，FAIL 结论不变。另请协调侧先从 origin 同步再写文档（brief 已被旧副本覆盖两次） | 防止后面的人以为采数未授权，或去找一个不存在的闸门 |

## G4 实现（已写，未在真机运行；代码 = 分支 `claude/m3-rive-lost` 提交 `90c4180`）
| 文件 | 改动 | 本地验证 |
|---|---|---|
| `m0/native/m0rive/m0rive.{h,cpp}` | M1：`m0r_set_abort_note` 同时把 stderr 重定向到 `<前缀>_stderr.txt`；渲染线程维护恢复阶段（`frame / detected / make_device / check_device / set_rendering_device / release_old / make_rive_context / import_file / make_offscreen / first_frame`），abort 时写 `abort_stage=`；每次尝试在 stderr 记录结果、HRESULT、适配器名称、VendorId 和 LUID。M2：每次尝试新建 DXGI 工厂，只在硬件适配器上建设备（跳过软件适配器和 VendorId 0x1414）；`CheckDevice` 用检查返回值的调用建 16×16 纹理和 256 字节缓冲，并查 `GetDeviceRemovedReason`；候选设备连续 500 ms 通过检查才交给 `RecoverDevice`（`SetRenderingDevice` → 释放旧对象 → 建 Rive），不通过就丢弃，100 ms 后重试。`m0r_lost_event` 末尾追加 `adapter_vendor`、`rejected` | MinGW 对照钉住的 Rive 头文件（`-Wall -Wextra -Wshadow`）：本文件 0 警告 0 错误 |
| `MotionSoftDemo/M3LostProbe.cs` | `M3-LOST v3`；输出 `inject<k>.adapter_vendor`、`inject<k>.rejected_devices` | 对照 WinAppSDK 2.5.1 编译 0 个 C# 错误 |
| `m3/run_m3_lost.ps1` | 每会话结束时把 `<前缀>_stderr.txt` 的最后 20 行和 `abort_stage` 写进 run log；会话开始前清掉旧的 stderr 文件；其余同 r2 | 本环境没有 PowerShell，人工复查 |

## G5 禁令
同 r2；另：r1、r2 数据不改不删，不混用；本 DRAFT ≠ 授权采数。
