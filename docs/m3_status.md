# M3 状态（m3_status.md）

> 更新：Claude，2026-11-06（pidan Y3）。M3 = “Rive 层 + 首个试点”（plan.md §5，第 22 周）。

## Rive 层（WP1–WP3）：在现有硬件上完成
| 验收项（plan.md M3 门槛） | 状态 | 票 |
|---|---|---|
| Rive 表面支持 DPI 变化 | **完成** | M3-RIVE-DPI PASS（PR #6） |
| Rive 表面支持设备丢失恢复 | **完成**（附已知限制 L-XAML-1） | M3-RIVE-LOST-RECOVER PASS；XAML-LOST / REBUILD / K93 诊断（PR #7） |
| 跟随部件拆分完成 | **完成**（设计约束已采用） | M3-RIVE-FOLLOW PASS（PR #7） |
| 通过混合刷新率测试矩阵 | **部分完成**：75 Hz@100% + 180 Hz@150% 双屏通过 | M3-MATRIX FAIL → PACING r4 PASS |
| 试点 1 开始接入 | **未开始** → 下一步 | WP4 |

## 推迟的矩阵项（等硬件或环境，另开票）
- 200% 缩放（需要 4K 显示器）
- 电池模式下的动态刷新率（需要支持 Win11 DRR 的笔记本）
- RDP、GPU 切换、显示器热插拔
- 60 Hz + 144 Hz 组合（已有 75 + 180 的结果，144 Hz 预计同理，按需补测）

## 未结事项
- L-XAML-1：IJI1KJG 环境下驱动重启后 XAML 不再绘制；外部报告草稿 `docs/m3_xaml_device_lost_report_draft.md` 待 pidan 决定是否提交。
- WP1 产品化：按 `docs/m3_wp1_rive_host_requirements.md`，含 Rive 容错补丁（H9）。
- PERF-CI 仍 HOLD（缺第三台机器、Q6、Q4 Win11 冒烟）。

## 下一步：WP4 试点 1
见 `docs/m3_wp4_pilot_DRAFT.md`。
