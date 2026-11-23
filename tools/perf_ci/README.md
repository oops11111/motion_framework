# PERF-CI 离线判分脚本

`perf_ci_grade.py` 按 `docs/m2_perf_ci_gate_LOCKED.md`（`5b007dd4…282a`）的 G2-LOCK-9 至 G2-LOCK-13 给已采好的产物判分，输出 PASS、FAIL 或 INVALID 和 G4 ONELINE。只用 Python 标准库，实验室机器和 Linux 上都能跑。

- 只读产物，不启动任何采集，**不授权采数**。PERF-CI 仍 HOLD。
- 闸门哈希不符、或没有 APPROVED 留证文件时，一律判 INVALID。`--dry-run` 跳过 APPROVED 检查，结果标为 `DRY-RUN(...)`，永远不会是 PASS。
- 不读也不改 `composition.h`。

```
python3 tools/perf_ci/perf_ci_grade.py <manifest.json> [--out-dir DIR] [--dry-run] [--frame-column NAME]
python3 -m unittest discover -s tools/perf_ci -v      # 合成数据单测
```
退出码：0 PASS，1 FAIL，2 INVALID，3 DRY-RUN 或用法错误。`--out-dir` 写出 `perf_ci_report.md` 和 `.json`。

## manifest.json

路径都相对 manifest 所在目录（也可写绝对路径）。

```json
{
  "gate": "../../docs/m2_perf_ci_gate_LOCKED.md",
  "approvedFile": "../../docs/m2_perf_ci_approved.md",
  "machines": [{
    "hostname": "DESKTOP-K93N3P8",
    "machineId": "<主机级稳定标识，如 SMBIOS UUID>",
    "gpuTier": "igpu | dgpu | lowend-laptop",
    "physical": true,
    "captureMethod": "logman-etl | eventpipe",
    "logmanDeniedEvidence": "denied.txt",
    "baselines": {
      "idle": {"stageP95Ms": {"ui": 0.9, "render": 2.5, "gpu": 2.4}, "frameP95Ms": 8.6}
    },
    "runs": [{
      "scenario": "idle | steady_play | interrupted_retarget",
      "round": 1, "pid": 10404, "refreshHz": 120,
      "presentmonCsv": "k93_idle_1.csv",
      "trace": "k93_idle_1.etl",
      "presentStats": "k93_idle_1_etw_events.txt",
      "stageP95Ms": {"ui": 0.8, "render": 2.6, "gpu": 2.3},
      "reconcileNote": ""
    }]
  }]
}
```

- `logmanDeniedEvidence` 只在 `captureMethod=eventpipe` 时必填（G2-LOCK-12 的拒绝证据）。
- `stageP95Ms` 是从 WPA 导出的各阶段 P95（单位 ms）。只有 eventpipe 路径允许不填（写 `null`）。不填时退回到“PresentMon 帧间隔 P95 ≤ T”，报告里标 `SUBSTITUTE`。
- `presentStats` 可以是进程内 ledger（取最后一条 `ETW_EVENT name=PresentStats`），也可以是含 `dropped=`、`intervalMs=` 的 key=value 汇总。
- `baselines` 按场景写同机同场景基线版本的 P95。阶段路径用 `stageP95Ms`，替代路径用 `frameP95Ms`。

## 判分口径（脚本里写死的解读）

| 项 | 实现 |
|---|---|
| 有效帧 | PresentMon 行的 ProcessID = `pid`（该列存在时），时长列 `MsBetweenPresents`（1.x）或 `FrameTime`（2.x）可解析、有限且 > 0 |
| 丢帧 | 时长 > 1.5×T，T = 1000/refreshHz；每千帧丢帧数必须**严格** < 1.0 |
| P95 | 最近秩（不插值） |
| 绝对预算 | UI ≤ 12%·T，渲染 ≤ 35%·T，GPU ≤ 35%·T；替代路径为帧间隔 P95 ≤ T |
| 相对回退 | 当前值 > 基线 ×1.10 判 FAIL；缺基线判 INVALID |
| 对账 | 进程内丢帧数 > PresentMon 丢帧数，或 PresentMon 丢帧数 > 2×进程内丢帧数，或 intervalMs 与 PresentMon 均值相差 > 2×，都记口径冲突。`reconcileNote` 为空判 INVALID，非空则只记一条说明 |
| 矩阵 | ≥2 台机器；machineId 非空且互异；≥2 个 GPU 档；`physical=true`；只允许三个 LOCK 场景；每台机器每个场景 ≥2 轮，且 pid 互异 |

判分口径中的开放问题见 `docs/m2_perf_ci_tooling_gate_DRAFT.md`，LOCK 前须确认。

## 采集（capture_run.ps1，tooling-only）

`capture_run.ps1` 在 Windows 上跑一轮 demo，同时采 PresentMon CSV、logman ETL 和 demo 日志（内含 `ETW_EVENT name=PresentStats` 行，判分脚本直接读它作 PresentStats 汇总），产出判分脚本要的产物 + `manifest.fragment.json`。只做采集工具，不判分、不解除 PERF-CI HOLD、不改 LOCKED 闸门与冻结文件。

```powershell
powershell -ExecutionPolicy Bypass -File tools\perf_ci\capture_run.ps1 `
    -DemoExe C:\build\MotionSoftDemo.exe -DemoArgs "--m2-02" `
    -PresentMon C:\tools\PresentMon.exe -OutDir C:\perf\steady_play_r1 `
    -RefreshHz 120 -Scenario steady_play -Round 1 -GpuTier igpu
```

- PresentMon 2.x exe 由操作者提供路径，仓里不带二进制；缺 exe 即硬错（无 CSV 的轮次按闸门必为 INVALID）。
- `-RefreshHz` 必填、无默认值：判分预算 T = 1000/Hz 由它算出，必须实报。
- logman 用 provider GUID `8f3a2c1d-…`（免 manifest 注册）；需要提权或 Performance Log Users 成员身份。logman 被拒时写 `denied.txt` 留证并以 exit 2 退出，不静默改走 EventPipe——EventPipe 降级路径（dotnet-trace + 拒证）按闸门评审是手动步骤。
- machineId 取 SMBIOS UUID（Q8 裁决）；`stageP95Ms` 留空、`reconcileNote` 留空：WPA 阶段导出口径（Q6）未定，判分脚本会据此判 INVALID，这是闸门的有意行为，不是 harness 故障。基线轮（Q7）与 r2 闸门仍待定。
- 退出码：0 产物齐全；1 demo 超时（已停 demo、产物照常收集）；2 采集建立失败；3 参数错误；4 必需产物缺失（CSV 文件不存在、ETL 不存在或 0 字节、demo 日志缺失或无 PresentStats 行；CSV 0 行不算缺，idle 场景合法）。`-DryRun` 只打印计划命令，不建任何东西。
- 产物文件一律写 UTF-8 无 BOM：WinPS 5.1 的 `Set-Content -Encoding UTF8` 带 BOM，判分脚本读 manifest 会直接失败。
- demo 日志目录默认与 demo 自身的 `DevLog.Directory` 解析顺序一致：`MF_M0_ARTDIR` → `%TEMP%\z3a_cabi_l1_dev` → exe 目录；显式 `-DemoLogFile` 优先。
- 判分脚本要在 LF 检出（或 Linux）上跑：闸门哈希按原始字节计算，Windows CRLF 检出的闸门正文哈希与脚本常量不一致，这是判分脚本侧的既有局限，改动它属于 r2 闸门工作，不在本票。

先用 `-DryRun` 核对命令，再在实验室机器上实跑；实跑验证提示词见审计目录的 `windows-test-prompts-perfci-capture.md`（如有）。
