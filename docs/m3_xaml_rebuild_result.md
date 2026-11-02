# M3-XAML-REBUILD 结题结果

STATUS: CLOSED
TICKET: XAML-REBUILD-PASS
METRICS: DIAGNOSIS_PASS（有条件；见 C 组 W_pre/W_post 截图挂账）
ADOPT: 有条件采用验证结论（R4=**KNOWN_LIMIT**／建议重启应用；**不以「宿主重建窗口」为充分补救**；**不合 main**）
CSPROJ_BACKPORT: 否
WRITTEN_AT: 2026-11-01 17:55:00 Asia/Shanghai
GATE: `docs/m3_xaml_rebuild_gate_LOCKED.md`
GATE_BODY_SHA256: `3ea74c371ae983cebe480e1b4dea99e61aec92cfbe9646fb87b91b7a1d456fdc`（自校 **MATCH**）
LOCKED_AT: 2026-10-30 17:35:36 AWST
APPROVED: `docs/m3_xaml_rebuild_approved.md` — APPROVED_BY=pidan；APPROVED_AT=2026-11-01 16:26:53 Asia/Shanghai
BRANCH: claude/m3-xaml-rebuild
COMMIT: d00ffd0f5be2f11f7b5b4e30f287c7d0b16b9e9f
TIP_READ: 24ccce6c3e8d618c704c27c8e675ae039c26599c
TIP_AFTER_PUSH: 同 tip_read / 同 commit（workbuddy ONELINE）
HOSTS: A/B=DESKTOP-IJI1KJG（Win11 26300 / AMD RX 7800 XT）；C=DESKTOP-K93N3P8（Win10 19045 / GTX 970）
composition.h: PRE=POST；CRLF=`33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97`；LF=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`；edited=False（本票未触）
RESULTS_DIR: `m3/results_xaml_rebuild/`（a:71 + b:25 + c:25 = **121 files**；本根已自真机同步可读）

## 授权 / 闸门核验摘要（写死）

| 项 | 结果 |
|---|---|
| GATE_BODY_SHA256 | **MATCH** `3ea74c37…6fdc`（排除 GATE_BODY_SHA256 行后对全文 SHA-256） |
| APPROVED | pidan @ 2026-11-01 16:26:53 Asia/Shanghai；见 `m3_xaml_rebuild_approved.md` |
| 本结题任务 | **不采数、不派 workbuddy、不重跑**；只据已落盘 results + ONELINE 结题 |
| composition.h | **未改** PRE=POST；LF/CRLF 与 ONELINE 一致 |
| PERF-CI | **仍 HOLD** |
| 前序 CLOSED / LOCKED | **不改写**：XAML-LOST PASS；r1/r2/RECOVER；DPI；PROBE；FOLLOW LOCKED body |
| FOLLOW 背景 | 已 APPROVED（SendInput LOCK `285e39dc…aab3`）且已派 workbuddy；**本结题不阻拦、不重复授权采数** |

> **Success ≠ adopt**；成功 ≠ 默认可合入 main；本票 **不合 main**。诊断票刀口=G1/G2/R4 的 W_pre/W_post 文字裁决，**≠** 采用代码进主线。

## workbuddy ONELINE（核实后；字段保留）

```
M3-XAML-REBUILD | tip_read=24ccce6c3e8d618c704c27c8e675ae039c26599c | commit=d00ffd0f5be2f11f7b5b4e30f287c7d0b16b9e9f @ claude/m3-xaml-rebuild | tip_after_push=同 | A IJI unpackaged×3=RECORDED inject_ok=1 shots=20 | B IJI packaged×1=RECORDED inject_ok=1 packaged=1 | C K93 packaged×1=RECORDED inject_ok=1 | composition PRE=POST CRLF=33d826c7… LF=a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418 edited=False | results m3/results_xaml_rebuild/{a:71,b:25,c:25}=121 files | push K93 OK | gate MATCH 3ea74c37… | APPROVED pidan 2026-11-01 16:26 Asia/Shanghai | ticket=XAML-REBUILD-PASS | R4=KNOWN_LIMIT | A/B: W_pre=TEXT-LOST W_post=TEXT-LOST | C: main=TEXT-OK；W_pre/W_post=INCONCLUSIVE(DPI截图像) | 有条件采用验证结论；不合main | Success≠adopt | PERF-CI HOLD
```

## G1 / G2 / R4 判据摘要（自 LOCK）

- **G1**：丢失是每窗口还是整进程？打包是否一样？→ 决定「宿主重建窗口」vs「已知限制（重启应用）」。
- **G2**：标签按窗口记 `TEXT-OK` / `TEXT-LOST`（文字区/圆角块/计数器）。
- **R4**：W_post 能画字 → 宿主补救设计；W_pre 也能画 → 补救更简单；**都不能 → 写成已知限制（建议重启应用）**，最小复现是否报 microsoft-ui-xaml 由 pidan 定（Q3）。

## TEXT 裁决（目视 + 内容区像素；按 A/B/C）

### A — IJI unpackaged ×3（R1 主数据）

| 窗口 / 时点 | s1 | s2 | s3 | 票内汇总 |
|---|---|---|---|---|
| W_pre @ base | TEXT-OK | TEXT-OK | TEXT-OK | 基线完整（`W_pre N` + Button + 蓝圆角块） |
| W_pre @ t05/t10/t30 | TEXT-LOST | TEXT-LOST | TEXT-LOST | 内容区近全黑；无字/无蓝块 |
| W_post @ t10/t30 | TEXT-LOST | TEXT-LOST | TEXT-LOST | **新建窗口仍无字** |
| main @ base | TEXT-OK | （同形） | （同形） | 有字/控件 |
| main @ t05+ | TEXT-LOST（偶见纯色块残留） | TEXT-LOST | TEXT-LOST | 与 XAML-LOST 旁证一致 |

**A 结论**：丢失为**进程级**（非「仅当时显示窗」）；**W_post=TEXT-LOST** → R4 走 **KNOWN_LIMIT**。

### B — IJI packaged ×1（R2）

| 窗口 / 时点 | 裁决 | 备注 |
|---|---|---|
| packaged= | **1**（非 BLOCKED_PACKAGED） | summary / run.log `launch=Package` |
| W_pre @ base | **TEXT-OK** | 同 A 形 |
| W_pre @ t05/t10/t30 | **TEXT-LOST** | 内容区黑 |
| W_post @ t10/t30 | **TEXT-LOST** | 窗体内容黑；部分帧叠桌面 Toast/任务栏预览（污染挂账，**不**把叠层当探针 UI） |
| main @ t05+ | **TEXT-LOST** | 与 A 同向 |

**B 结论**：打包路径**同样**进程级 TEXT-LOST；R2 问题已答。

### C — K93 packaged ×1（R3）

| 窗口 / 时点 | 裁决 | 备注 |
|---|---|---|
| packaged= | **1**；verdict=RECORDED；inject_ok=1 | Win10 19045 / GTX 970 / scale=1.250 |
| main @ base/t05/t10/t30 | **TEXT-OK** | 计数与 `spring` 等文字持续可见；**本机未复现** IJI 主窗文字丢失 |
| W_pre / W_post 全时点 | **INCONCLUSIVE** | 截图像素尺寸异常（约 496×192 / 397×154，≠探针标称 620×240）；画面含桌面/旁窗碎片，**无法可靠目视探针文字**；缺：DPI=1.25 下按句柄截窗校准或复采（**本结题不授权复采**） |

**C 结论**：会话 RECORDED 成立；**主窗旁证=未丢字**；W_pre/W_post **挂账 INCONCLUSIVE**（捕获质量，非「已证明能画字」）。

## R4 裁决（写死）

| 问题 | 裁决 |
|---|---|
| W_post 能否画字？（IJI A/B） | **否 → TEXT-LOST** |
| W_pre 能否画字？（IJI A/B） | **否 → TEXT-LOST** |
| 宿主「检测到丢失后重建窗口」是否充分？ | **否（IJI 类环境）** — 新建 W_post 仍 TEXT-LOST |
| 应写路径 | **KNOWN_LIMIT**：建议**重启应用**；最小复现/对外报告（Q3）交 pidan；**不**把本票写成可合入的宿主补救实现 |
| 打包是否一样？（R2） | **是**（IJI packaged 同向 TEXT-LOST） |
| 第二机（R3） | 主窗未丢字；W_* 截图 INCONCLUSIVE → **机差挂账**（可能与 OS/GPU 有关；**不**据此翻盘 A/B） |

## 票结局声明

- 票标签 **`XAML-REBUILD-PASS`**；STATUS=**CLOSED**；诊断问题在 R1/R2 **可裁决回答**；R4=**KNOWN_LIMIT**。
- **有条件点**：C 组 W_pre/W_post 截图 INCONCLUSIVE；B 部分 W_post 帧有桌面叠层污染（不影响 TEXT-LOST 方向）。
- **是否采用 = 有条件采用验证结论**：采用「进程级丢失 + 打包同向 + 宿主重建窗口不充分 → 已知限制/重启应用」；**不合 main**；Success≠adopt。
- **composition.h 未改**；**PERF-CI 仍 HOLD**；**不阻拦**已派的 FOLLOW workbuddy。

## 挂账 / 残差

| 项 | 处置 |
|---|---|
| C W_pre/W_post 截图 | **INCONCLUSIVE** 挂账；可选日后 DPI 截窗校准（≠本 next 采数令） |
| B W_post Toast 叠层 | 记录；裁决仍 TEXT-LOST |
| C 主窗 TEXT-OK | 机差旁证；不翻盘 IJI 结论 |
| Q3 对外报告 | **pidan 决定**；本结题不代发 |
| Lab csproj | **不回灌主线** |
| FOLLOW | 已 APPROVED+已派 → 见 next；**不重复授权** |
| PERF-CI | **HOLD** |

## 标签边界

- 本票标签仅 `XAML-REBUILD-*`；PASS ≠ 默认可合入 main ≠ ship ≠ 宿主补救已实现 ≠ PERF-CI ≠ 解冻 Bv
- 不改 composition.h；不重开 S1/R-3/R-2；不翻 CLOSED 旧票；不阻拦 workbuddy
- 产物前缀：`m3_xaml_rebuild_*` / `notion_m3_xaml_rebuild_*`；结果目录 `m3/results_xaml_rebuild/`

— notion ai（M3-XAML-REBUILD 结题；CLOSED PASS；R4=KNOWN_LIMIT；有条件采用验证结论；不合 main；PERF-CI HOLD）
