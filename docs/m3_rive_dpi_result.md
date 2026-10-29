# M3-RIVE-DPI 结题结果

STATUS: CLOSED
TICKET: RIVE-DPI-PASS
METRICS: METRICS_PASS
ADOPT: 有条件采用（验证结论 / 实验分支证据；非整 tip 默认可合入主线）
CSPROJ_BACKPORT: 否（不回灌主线；Lab 补丁未进 tip；另票再议）
WRITTEN_AT: 2026-10-29 10:00:00 Asia/Shanghai
GATE: docs/m3_rive_dpi_gate_LOCKED.md
GATE_BODY_SHA256: 92e6b4e9fd1014faf74714eb6cb20fc7e998169dc1171785bb8ea43f68d97bf5
LOCKED_AT: 2026-10-28 20:39:58 AWST（Australia/Perth）
APPROVED: docs/m3_rive_dpi_approved.md — pidan@2026-10-28 21:28:50 AWST
BRANCH: claude/m3-rive-dpi
TIP: 67decd39b3824b390740461a356def44b4e47614
HOST: DESKTOP-IJI1KJG；scales=100%↔125%
composition.h: PRE=POST a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418

## 授权核验摘要

| 项 | 结果 |
|---|---|
| approved 文件 | 存在：`docs/m3_rive_dpi_approved.md` |
| APPROVED_BY / AT | pidan / **2026-10-28 21:28:50 AWST**（原文照抄） |
| 采数 | **是**（APPROVED 写死：`m3/run_m3_dpi.ps1 -Sessions 3` → `m3/results_dpi/`） |
| 落地 / 合入 main | APPROVED 写死 **否**（结果评审后经 PR） |
| host / scales | DESKTOP-IJI1KJG / 100↔125（与 G3/G2 一致） |
| composition.h | **不改**；run.log PRE=POST `a55d4952…e418`；本地 `code/composition.h` 同 hash |
| GATE_BODY_SHA256 | 期望 `92e6b4e9fd1014faf74714eb6cb20fc7e998169dc1171785bb8ea43f68d97bf5` |
| 结题复算 hash | **MATCH**（排除以 GATE_BODY_SHA256 开头的那一行后全文 SHA-256）；**未改** LOCKED body/hash |
| tip 内容 | 主要 `m3/results_dpi/`（40 files）+ lab note；WP1 实现在更早 commit `7c6a4b17` |

## G4 / 协调 ONELINE（核实后）

```
M3-RIVE-DPI | commit=67decd39b3824b390740461a356def44b4e47614 | gate=92e6b4e9fd1014faf74714eb6cb20fc7e998169dc1171785bb8ea43f68d97bf5 @ 2026-10-28 20:39:58 AWST | approved=pidan@2026-10-28 21:28:50 AWST | host=DESKTOP-IJI1KJG scales=100↔125 | build=BUILD_OK | sessions=3 | switches=4/4/4 | size_ok=1/1/1 | reraster_max_ms=12.31/11.16/13.33 | budget_ok/stability_ok=1/1|1/1|1/1 | mem_delta_mb=2/0/2 | sharp=PASS(协调目视+结题抽查) | composition.h PRE=POST a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418 | ticket=RIVE-DPI-PASS | METRICS_PASS | 有条件采用；csproj不回灌主线；成功≠采用≠默认可合入main
```

## 票结局声明

- 真机 3 会话对照 LOCKED G2：**①②④⑤⑥ 全会话满足** → 票标签 **`RIVE-DPI-PASS`**；STATUS=**CLOSED**；协调对照闸门 **METRICS_PASS**。
- **明确声明**：**成功 ≠ 采用 ≠ 默认可合入 main ≠ 授权改 composition.h ≠ PERF-CI ≠ 解冻 Bv ≠ 翻盘 M2**。
- **是否采用 = 有条件采用**：采用「本票验证结论」与实验分支上的 DPI 路径证据（`m0r_resize` / probe / runner + `m3/results_dpi/`）；**不**把 tip `67decd39…` 整包视为可直接合入主线的产品交付；WP1 代码合入须另走 PR 评审。
- **csproj 回灌 = 否**：`WindowsPackageType=None` + `WindowsAppSDKSelfContained=true` 仅为 Lab 绕过 `REGDB_E_CLASSNOTREG`（DeploymentManager AutoInitialize），**非改 Rive**；该补丁**未进入 tip 仓库树**（分支上 `MotionSoftDemo.csproj` 无此二属性）；**不回灌主线**；需要时另开 Lab/Demo 启动工具链票。

## METRICS 核实（对照协调；本地读 `results_m3dpi.zip` / tip 40 files）

来源：`m3/results_dpi/m3dpi_session_matrix.txt` + `m3dpi_s{1,2,3}_summary.txt` + `m3dpi_run.log` + `m3dpi_lab_fix_note.txt`。

| 会话 | switches | size_ok | reraster_max_ms | budget_ok | stability_ok | mem_delta_mb | verdict |
|---|---|---|---|---|---|---|---|
| s1 | 4 | 1 | 12.31 | 1 | 1 | 2 | RECORDED |
| s2 | 4 | 1 | 11.16 | 1 | 1 | 0 | RECORDED |
| s3 | 4 | 1 | 13.33 | 1 | 1 | 2 | RECORDED |

与协调 ONELINE **一致**。

### G2 判据对照

| 判据 | 门槛 | 核实 | 结果 |
|---|---|---|---|
| ① 切换≥4 | 每会话 ≥4 | 4/4/4；switchN.detected=1 且 want=got | **PASS** |
| ② 尺寸 0px | round(400×scale)；400↔500 | size_ok=1/1/1；applied_px 400²/500² | **PASS** |
| ③ 重光栅 | P95≤100ms（**只记录**） | max 12.31 / 11.16 / 13.33 ≪100 | **记录**（不挡 PASS） |
| ④ 稳定性 | failures=0；device_removed=0；段帧≥300 | draw_failures=0；device_removed_hr=0；每段帧≥586 | **PASS** |
| ⑤ 预算 | CPU/GPU P95 ≤0.35·T；GPU drop≤10% | frac_T 均 ≪0.35；gpu_dropped=0；budget_ok=1/1/1 | **PASS** |
| ⑥ 清晰度 | 截图 vs 新尺寸 ref：清晰矢量重绘、非模糊放大 | 协调目视 PASS；结题抽查 s1 seg0(100%)/seg1(125%) 边缘清晰；ref.bgra 尺寸 640008=400² / 1000008=500² | **PASS**（目视含协调确认 + 结题抽查） |
| ⑦ 内存 | Δ≤20MB（**只记录**） | 2 / 0 / 2 MB | **记录**（不挡 PASS） |

**ticket 判据 PASS**（①②④⑤⑥）；③⑦仅记录。

## Lab 注记（核实）

- 首轮×3：`INVALID_NO_SUMMARY`（`REGDB_E_CLASSNOTREG` / DeploymentManager AutoInitialize）——见 `m3dpi_run.log` 前段。
- Lab 修（非改 Rive）：本机 `MotionSoftDemo.csproj` 加 `WindowsPackageType=None` + `WindowsAppSDKSelfContained=true` 后重跑 → 3×RECORDED。
- tip `67decd39…` **主要是** `m3/results_dpi/`（40 files）+ lab note；**csproj 补丁未提交进分支树**。

## 采用决断（写死）

| 项 | 决断 | 理由 |
|---|---|---|
| 票标签 | **RIVE-DPI-PASS** | G2 硬判据全过；METRICS_PASS |
| 是否采用 | **有条件采用** | 采用验证结论与实验分支证据；成功≠默认合入主线；tip 以 results 为主 |
| 采用范围 | 验证结论 + DPI 路径证据（`7c6a4b17` 实现 + tip results） | 非整 tip 无评审合入 |
| csproj 回灌主线 | **否** | Lab 绕 CLASSNOTREG；非 Rive 本体；补丁未在 tip；另票再议 |
| csproj 留在 m3 分支 | **可挂账备注**；当前 tip **未含**该文件改动 | 需要时另开工具链票再 LOCK+PR |
| composition.h | **未改** PRE=POST `a55d4952…e418` | 硬禁 |

## 挂账 / 残差

| 项 | 处置 |
|---|---|
| ③ reraster_max_ms ≈13.33 | **记录**；远低于 100ms；不升 FAIL |
| ⑦ mem_delta 2/0/2 MB | **记录**；≤20；不升 FAIL |
| Lab csproj 补丁 | **挂账**：不回灌主线；Demo unpackaged 启动若再遇 CLASSNOTREG → 另开 Lab 工具链票 |
| ⑥ 目视 | 协调目视 PASS；结题抽查 s1 seg0/seg1 支持；全量 15 段截图以 results 归档为准（本结题未逐帧像素 diff） |
| M2 旁路 / VERIFY-FAIL | **不翻盘**；本票不改写任何 M2 FAIL |
| PERF-CI | **仍 HOLD** |
| M3-RIVE-LOST 等 | 见 next；本票不预开采数 |

## 标签边界

- 本票标签仅 `RIVE-DPI-*`；**METRICS_PASS ≠ 默认可合入 main ≠ ship**
- 不重开 S1/R-3/R-2；不解冻 Bv；禁影子 IV；不改 composition.h
- 不把 M2 VERIFY-FAIL 改 PASS；不立刻 Δv″；不抬 thr
- 产物前缀：`m3_rive_dpi_*` / `m3dpi_*` / `notion_m3_rive_dpi_*`

## 写结

- written_at: 2026-10-29 10:00:00 Asia/Shanghai
- notion ai（动效框架执行助手）结题；**本任务未采数、未派 workbuddy**；依据本地闸门复算 + uploads/`results_m3dpi.zip`（40 files）+ tip `67decd39…` + 协调 brief 对照
