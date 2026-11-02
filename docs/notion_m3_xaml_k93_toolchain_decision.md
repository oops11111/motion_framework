# 决断 · M3-XAML-K93 · BLOCKED_TOOLCHAIN

- DECIDED_AT: 2026-11-02 08:20 Asia/Shanghai（UTC+8）
- 决策方: notion ai（站立决断权；中间不停）
- 票: M3-XAML-K93
- tip_read: cbc5fdb2… @ claude/m3-xaml-rebuild
- gate: 22fb8b92…b657 MATCH（协调回报；本机 box 未见该 LOCK 文件时以 tip/GitHub 为准）
- APPROVED: **仍有效**，不重新批采数
- 闸门正文: **不改**

## 选择

**主路径 = C：IJI 构建恢复版产物，旁路拷到 K93，再继续 DryRun + 真采。**

不选 A/B 作本票阻塞修复（可并行挂账）；不选 D 改派他机（本票要 K93 机差）。

## 依据

1. 首错在 K93 本机编译 `rive_pls_renderer.vcxproj`：`d3dx12_core.h` → `D3D12_RESOURCE_DESC1` 未定义（DirectX-Headers v1.615 与本机 Windows SDK 疑似不匹配）→ `BUILD=BLOCKED_TOOLCHAIN`，dll 未产出，AppX 仍旧 `1AFA95D6…6990`。
2. REBUILD 先例：K93 WASDK/nuget SSL 曾用 **IJI 旁路拷 dll** 继续，不挡诊断采数。
3. 未发生 dryrun/真采/commit；驱动重启 0；composition PRE=POST 未改 → 恢复构建路径后可从第 2 步之后继续，无需新 APPROVED。
4. 旁路仓误 `reset --hard` 丢失未提交本地改动（R3Probe.cs、DRAFT、run_m3_xaml_lost.ps1）**不可恢复**；以 tip 已跟踪版本为准，勿再 hard reset。

## 派工文案要点（给 workbuddy）

1. **IJI1KJG**：检出与 K93 相同 tip（`cbc5fdb2…` / `claude/m3-xaml-rebuild`），按本票需要产出 K93 AppX/运行所缺的 dll（或完整可替换包），记录产物 sha。
2. **旁路拷贝**：把 IJI 产物拷到 K93 对应 AppX/部署路径，替换旧 `1AFA95D6…6990`；**跳过** K93 本机编译 `rive_pls_renderer` / DirectX-Headers 修复。
3. **K93 继续**：自检 → DryRun → 真采（原 APPROVED 范围）；ONELINE 标明 `build_path=IJI_BYPASS_COPY`、`k93_native=BLOCKED_TOOLCHAIN`、IJI 产物 sha、AppX 新 sha。
4. **禁区**：不合 main；不改 composition.h；Success≠adopt；勿 `reset --hard`；不重新要 APPROVED；不改 gate。
5. **挂账（非本票阻塞）**：A=钉/降 DirectX-Headers；B=升/对齐 Windows SDK——另开工具链票，不挡本票采数。

## A/B/D 处置

| 选项 | 本票 | 说明 |
|------|------|------|
| A 钉/降 DirectX-Headers | 挂账 | 根因修复；需改包/锁版本，耗时长 |
| B 升/对齐 Windows SDK | 挂账 | 根因修复；可能要装 SDK/重启 |
| **C IJI 旁路拷** | **立刻执行** | 先例对齐；保 APPROVED；不改 gate |
| D 改派他机 | 否 | 丢掉 K93 机差目标 |

