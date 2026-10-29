# M3-RIVE-DPI 结题中文包（notion_m3_rive_dpi_pack.md）

> DECISION_AT=2026-10-29 10:00:00 Asia/Shanghai  
> 权威结题：`docs/m3_rive_dpi_result.md`（WRITTEN_AT=2026-10-29 10:00:00 Asia/Shanghai）  
> 闸门 LOCKED **勿改 hash**：`docs/m3_rive_dpi_gate_LOCKED.md` GATE_BODY_SHA256=`92e6b4e9fd1014faf74714eb6cb20fc7e998169dc1171785bb8ea43f68d97bf5` @ 2026-10-28 20:39:58 AWST（结题复算 **MATCH**）  
> APPROVED：`docs/m3_rive_dpi_approved.md` — pidan@2026-10-28 21:28:50 AWST（采数=**是**；落地/合入 main=**否**）  
> 计划：`docs/m3_plan_DRAFT.md` §7 第 1 张票（WP1）  
> 协调简报：`docs/m3_rive_dpi_result_brief.md`（METRICS_PASS；成功≠采用）  
> 分支 tip：`claude/m3-rive-dpi` @ `67decd39b3824b390740461a356def44b4e47614`（主要 results；40 files @ `m3/results_dpi/`）  
> composition.h PRE=POST `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`  
> 旁路：`docs/side_parked_hang_degrade.md`（M2 比–绝对冲突已 R2；**本票勿翻盘 M2**）

## 结论

- **票级 = CLOSED**；标签 **`RIVE-DPI-PASS`**；协调对照 **METRICS_PASS**。
- **是否采用 = 有条件采用**（验证结论 / 实验分支证据；非整 tip 默认可合入主线）。
- **csproj 回灌主线 = 否**（Lab 绕 CLASSNOTREG；补丁未进 tip；另票再议）。
- ONELINE（结题）：  
  `M3-RIVE-DPI | commit=67decd39b3824b390740461a356def44b4e47614 | gate=92e6b4e9fd1014faf74714eb6cb20fc7e998169dc1171785bb8ea43f68d97bf5 @ 2026-10-28 20:39:58 AWST | approved=pidan@2026-10-28 21:28:50 AWST | host=DESKTOP-IJI1KJG scales=100↔125 | build=BUILD_OK | sessions=3 | switches=4/4/4 | size_ok=1/1/1 | reraster_max_ms=12.31/11.16/13.33 | budget_ok/stability_ok=1/1|1/1|1/1 | mem_delta_mb=2/0/2 | sharp=PASS | composition.h PRE=POST a55d4952…e418 | ticket=RIVE-DPI-PASS | METRICS_PASS | 有条件采用；csproj不回灌主线`
- **声明**：`RIVE-DPI-PASS` / `METRICS_PASS` ≠ 默认可合入 main ≠ ship ≠ 改 composition.h ≠ PERF-CI ≠ 解冻 Bv ≠ 翻盘 M2 VERIFY-FAIL。
- `composition.h` **本票未触**；本闸门 body/hash **未改**（复算 MATCH）。
- **本票 ≠ PERF-CI**；PERF-CI 仍 HOLD；禁影子 IV；不解冻 Bv；不重开 S1/R-3/R-2；不开与本票无关的 M2 翻盘。

## 授权核验

| 项 | 值 |
|---|---|
| approved 文件 | `docs/m3_rive_dpi_approved.md` |
| APPROVED_BY / AT | pidan / 2026-10-28 21:28:50 AWST |
| 采数 | **是**（3 会话；host=DESKTOP-IJI1KJG） |
| 落地 / 合入 main | APPROVED **否**；结题维持「有条件采用」非默认可合入 |
| gate hash | `92e6b4e9fd1014faf74714eb6cb20fc7e998169dc1171785bb8ea43f68d97bf5` @ 20:39:58 AWST；复算 **MATCH**；**勿改** |
| tip | `67decd39…e47614`；40 files under `m3/results_dpi/` |

## 事实表

| 项 | 值 |
|---|---|
| TICKET / STATUS | `RIVE-DPI-PASS` / **CLOSED** |
| METRICS | **METRICS_PASS**（成功≠采用） |
| 采用 | **有条件采用** |
| csproj 回灌 | **否**（不进主线） |
| gate | `92e6b4e9…97bf5` @ 2026-10-28 20:39:58 AWST |
| approved | pidan@2026-10-28 21:28:50 AWST |
| sessions / switches | 3 / 4/4/4 |
| size_ok | 1/1/1（400↔500） |
| reraster_max_ms | 12.31 / 11.16 / 13.33（③ 记录） |
| budget / stability | 1/1 \| 1/1 \| 1/1 |
| mem_delta_mb | 2 / 0 / 2（⑦ 记录） |
| ⑥ 清晰度 | 协调目视 PASS + 结题抽查 s1 seg0/seg1 |
| composition.h | 未触 `a55d4952…e418` |
| PERF-CI | HOLD |

## Lab / 实现边界

| 项 | 说明 |
|---|---|
| 首轮失败 | ×3 INVALID_NO_SUMMARY：`REGDB_E_CLASSNOTREG` |
| Lab 补丁 | csproj：`WindowsPackageType=None` + `WindowsAppSDKSelfContained=true`（非改 Rive） |
| tip 范围 | 主要 results + lab note；**csproj 改动未提交进分支树** |
| WP1 代码 | 更早 commit `7c6a4b17`（m0r_resize / M3DpiProbe / runner）；合入须 PR |

## 产物清单

- `docs/m3_rive_dpi_result.md`（权威结题）
- `docs/notion_m3_rive_dpi_pack.md`（本包）
- `docs/notion_m3_rive_dpi_next.md`
- 只读引用：LOCKED / DRAFT / approved / `m3_plan_DRAFT.md` / 协调 brief / `m3/results_dpi/*`
- 产物前缀 only：`m3_rive_dpi_*` / `m3dpi_*` / `notion_m3_rive_dpi_*`

## 禁令复述

- 不重开 S1 / R-3 / R-2；不解冻 Bv；不改 composition.h；禁影子 IV
- 成功 ≠ 采用 ≠ 默认可合入 main；PERF-CI HOLD
- 不把 M2 VERIFY-FAIL 改 PASS；不立刻 Δv″；不抬 thr
- 本闸门 `92e6b4e9…97bf5` **不改 body/hash**
- 本结题任务 **不采数、不派 workbuddy**

## 挂账 / 残差

| 项 | 处置 |
|---|---|
| M3-RIVE-DPI（本票） | **CLOSED RIVE-DPI-PASS**；METRICS_PASS；有条件采用 |
| ③ / ⑦ | 记录挂账（max≈13ms；Δ≤2MB） |
| Lab csproj | **不回灌主线**；另票再议 |
| M2 旁路 | 保持；不翻盘 |
| PERF-CI | 继续 HOLD |
| 下一张真机票 | 见 next → **M3-RIVE-LOST**（按 m3_plan；须新 DRAFT→LOCK→APPROVED） |

## 下一票决断

详见 `docs/notion_m3_rive_dpi_next.md`。  
**主推：M3-RIVE-LOST**（WP1 设备丢失恢复；需管理员；新闸门+APPROVED；本 next **不**授权采数）。

— notion ai（M3-RIVE-DPI 结题；CLOSED RIVE-DPI-PASS；METRICS_PASS；有条件采用；csproj 不回灌主线；gate MATCH 不改）
