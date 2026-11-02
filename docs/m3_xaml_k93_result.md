# M3-XAML-K93 结题结果

STATUS: CLOSED
TICKET: XAML-K93-PASS
METRICS: METRICS_PASS（有条件；见 build_path / TEXT 目视缺字段 / recover_ms 记录）
ADOPT: 有条件采用验证结论（K93 packaged+with_rive 上 lost_detected+recovered 成立；**构建路径非 K93 原生**；**不合 main**）
CSPROJ_BACKPORT: 否
WRITTEN_AT: 2026-11-02 09:20:00 Australia/Perth（AWST）
GATE: `docs/m3_xaml_k93_gate_LOCKED.md`（**本机 box 未见该 LOCK 文件**；gate MATCH 由协调/workbuddy 回报 + tip `cbc5fdb2…` @ `claude/m3-xaml-rebuild` 旁证；decision_doc=HIT）
GATE_BODY_SHA256: `22fb8b92d8703619215ffaae3f0d5d9efea003951bb44b42d790d488ac28b657`（协调/workbuddy 报 **MATCH**；结题侧对 tip 原文复算亦 **MATCH**；**本地未见 LOCK 文件**）
LOCKED_AT: 2026-11-01 18:37:27 AWST（tip 旁证）
APPROVED: tip 旁证 `docs/m3_xaml_k93_approved.md` — APPROVED_BY=pidan；APPROVED_AT=2026-11-01 20:58:26 AWST；**原 APPROVED 仍有效**（Decision C 不重新批采数）
DECISION_C: `docs/notion_m3_xaml_k93_toolchain_decision.md` — 主路径 **IJI_BYPASS_COPY**；`k93_native=BLOCKED_TOOLCHAIN`
BRANCH: claude/m3-xaml-rebuild
TIP_READ: cbc5fdb285570fde7d71fb7f3782d02f1663f8fd
COMMIT: bfd76c3660b157ca3c903bb9f2afae07339c6f55
HOST: DESKTOP-K93N3P8（Win10 19045 / GTX 970 / scale=1.250）
build_path: **IJI_BYPASS_COPY**
k93_native: **BLOCKED_TOOLCHAIN**
dll_sha256: `70ed8d14ff0e73793d36dd6799e8972f9877cd652eee54401f35b444de6da15f`（≠旧 `1AFA95D6…6990`）
composition.h: PRE=POST；CRLF=`33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97`；LF=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`；edited=False（本票未触）
RESULTS_DIR: `m3/results_xaml_k93/`（ONELINE **26** files @ commit；本根只读核验 summary/raw/stderr/matrix/run.log；png 以 commit 为准）

## 授权 / 闸门核验摘要（写死）

| 项 | 结果 |
|---|---|
| GATE_BODY_SHA256 | **MATCH** `22fb8b92…b657`（协调/workbuddy 回报；tip 旁证复算 MATCH）；**本地未见 LOCK 文件** |
| decision_doc | **HIT** `docs/notion_m3_xaml_k93_toolchain_decision.md`（Decision C） |
| APPROVED | 原 APPROVED **仍有效**；不重新批采数；tip 旁证 pidan @ 2026-11-01 20:58:26 AWST |
| 本结题任务 | **不采数、不派 workbuddy、不重跑**；只据 ONELINE + 已落盘/已推 results + Decision C 结题 |
| composition.h | **未改** PRE=POST |
| PERF-CI | **仍 HOLD** |
| 前序 CLOSED | **不改写**：XAML-LOST / XAML-REBUILD / FOLLOW / r1/r2/RECOVER / DPI 等 |
| 工具链 A/B | **挂账**；**不挡本票** |

> **Success ≠ adopt**；成功 ≠ 默认可合入 main；本票 **不合 main**。IJI 旁路构建是 **Decision C 明确路径**，不算作弊；须在采用口径标明「有条件：构建路径非 K93 原生」。

## workbuddy ONELINE（核实后；字段保留）

```
M3-XAML-K93 | tip_read=cbc5fdb285570fde7d71fb7f3782d02f1663f8fd | commit=bfd76c3660b157ca3c903bb9f2afae07339c6f55 @ claude/m3-xaml-rebuild | build_path=IJI_BYPASS_COPY | k93_native=BLOCKED_TOOLCHAIN | gate=MATCH 22fb8b92d8703619215ffaae3f0d5d9efea003951bb44b42d790d488ac28b657 | decision_doc=HIT | 原APPROVED仍有效 | dll_sha=70ed8d14ff0e73793d36dd6799e8972f9877cd652eee54401f35b444de6da15f | dryrun=RECORDED(with_rive=1 packaged=1) | sessions=1×RECORDED(with_rive=1 packaged=1 inject_ok=1 shots=20) | rive.lost_detected=1 recovered=1 lost_now=0 recover_ms=4504.25 rive_abort=none reason=0x887A0005 | W_pre=1440x756 W_post=1153x605 | composition PRE=POST 33d826c7…(WinCRLF) / a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418(LF) | capture=OK | results_count=26 → m3/results_xaml_k93/ | 不合main | Success≠adopt | PERF-CI HOLD
```

## G1 / G2 判据摘要（自 tip LOCK 旁证）

- **G1**：K93 上 `pnputil /restart-device` 是否让应用设备丢失（`rive.lost_detected` / `removed_reason`）；丢失后 XAML（主窗 / W_pre / W_post）是否仍能画字。
- **G2**：以 `rive.lost_detected` 为前提的三行表（能画字 / 不能画字 / 未丢失→INCONCLUSIVE_NO_LOSS）。

## 会话事实（只读 results + ONELINE）

| 项 | 值 |
|---|---|
| dryrun | **RECORDED**（with_rive=1 packaged=1） |
| session | **1×RECORDED**；with_rive=1；packaged=1；inject_ok=1；shots=20 |
| launch | Package（run.log） |
| rive.lost_detected | **1** |
| rive.recovered | **1** |
| rive.lost_now | **0** |
| rive.removed_reason | **0x887A0005** |
| rive.recover_ms | **4504.25**（按闸门：**记录**；不升硬 FAIL） |
| rive_abort | **none** |
| stderr recover | attempt=1/2 hr=`0x887A0002`；attempt=3/7 hr=`0x00000000` adapter=GTX 970 |

### 窗口尺寸（ONELINE + summary；**无 TEXT-OK/TEXT-LOST 目视字段**）

| 窗口 / 时点 | 尺寸（summary） | TEXT 目视 |
|---|---|---|
| W_pre @ base（obs.base.window） | **1440×756** | **缺**：results KV / ONELINE **未给** TEXT-OK/TEXT-LOST |
| W_post / post-inject（obs.t05+ window） | **1153×605** | **缺**：同上；本结题**不**代做目视升格 |

> 裁决提示对齐：探测+恢复在 K93 packaged+with_rive 上**成立**；G2「能否画字」行因缺文字裁决字段 → **如实挂账缺字段**，不以本结题虚构 TEXT-OK/LOST。

## 票结局声明

- 票标签 **`XAML-K93-PASS`**；STATUS=**CLOSED**；**METRICS_PASS（有条件）**。
- **有条件点**：① `build_path=IJI_BYPASS_COPY` / `k93_native=BLOCKED_TOOLCHAIN`（Decision C；非 K93 原生编译）；② TEXT 目视裁决字段缺失（仅尺寸）；③ `recover_ms≈4504.25` 仅记录。
- **是否采用 = 有条件采用验证结论**：采用「K93 packaged+with_rive 上 lost_detected=1 + recovered=1 + abort=none」；**标明构建旁路**；**不合 main**；Success≠adopt。
- **composition.h 未改**；**PERF-CI 仍 HOLD**；工具链 A/B **挂账不挡本票**。

## 挂账 / 残差

| 项 | 处置 |
|---|---|
| k93_native=BLOCKED_TOOLCHAIN | A=钉/降 DirectX-Headers；B=升/对齐 Windows SDK → **另开工具链票**（文档挂账；≠本 next 采数令） |
| TEXT 目视缺字段 | 挂账；**不**借本结题授权复采/代目视升格 |
| recover_ms=4504.25 | 记录 |
| IJI_BYPASS_COPY | 采用口径写死「非 K93 原生」 |
| PERF-CI | **HOLD** |
| MATRIX | 见 next；**不插队真机、不授权采数** |

## 标签边界

- 本票标签仅 `XAML-K93-*`；PASS ≠ 默认可合入 main ≠ ship ≠ K93 原生工具链已修 ≠ PERF-CI ≠ 解冻 Bv
- 不改 composition.h；不重开 S1/R-3/R-2；不翻 CLOSED 旧票
- 产物前缀：`m3_xaml_k93_*` / `notion_m3_xaml_k93_*`；结果目录 `m3/results_xaml_k93/`

— notion ai（M3-XAML-K93 结题；CLOSED PASS；METRICS_PASS 有条件；有条件采用验证结论；不合 main；PERF-CI HOLD）
