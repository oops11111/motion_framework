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

## 闸门版本
manifest 里的 `"gateVersion"` 决定按哪一版判分：
- `"r1"`（默认）：`docs/m2_perf_ci_gate_LOCKED.md`（`5b007dd4…282a`）。
- `"r2"`：`docs/m2_perf_ci_gate_r2_DRAFT.md`，即按 Q1–Q8 裁决修订后的版本。r2 LOCK 之前只接受 `--dry-run`；LOCK 后，把 hash 填进 `RULES["r2"]["sha"]`。

r2 与 r1 的差别：
- Idle 按时长判定：每轮 ≥60 秒，0 次 Present，CPU 和 GPU 均 ≤0.1%。Idle 的 run 需要提供 `durationSec`、`cpuAvgPercent`、`gpuAvgPercent`；
- 降级路径的门槛为帧间隔 P95 ≤ 1.1·T；
- 两边丢帧数都 ≤2 时，不计口径冲突；
- machineId 必须是 SMBIOS UUID；
- 支持只建基线的轮次（`"mode": "baseline"`），生成 `perf_ci_baselines.json`（退出码 4）；判分轮用 `"baselinesFile"` 引用这个文件。

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
