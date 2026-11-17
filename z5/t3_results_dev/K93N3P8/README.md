# Z5-T3 · L1 内部小修 · K93N3P8 验收记录（dev-only）

> composition.h 冻结（未改）；L1 公开面 / fm_* / E1 未改；fm_capi.h/.cpp 未改；不采数；不计任何闸门会话；PERF-CI HOLD；Success≠adopt；不合 main；不改任何 *_LOCKED.md。
> Z4 r1 结论仍绑定 `fe59c622`；今后任何新闸门须绑定含 T3 的 tip。

- 派工：`workbuddy_z5_wave2_dispatch.md`（sha256 `e7deb80ce08c3c42b308852d3d42d04091aa56c97e71e83d5046971734130506`）Z5-T3；开票：pidan 2026-11-18 00:46 AWST「开 Z5-T3、Z5-T5 编码票」
- 分支 `wb/z5-t3`，起点 main `2e57bff384c771cc972840ca98a1f0bb92aaeb98`（前置：T1 验收 PASS，见 `wb/z5-t1` `865c458d`）
- 时间：2026-11-18 01:02–01:07 AWST（Australia/Perth, UTC+8）

## 0. 先 grep（派工第 0 步）

在 2e57bff 全树 `git grep FM_FLUIDMOTION_PATH`：只命中 `MotionSoftDemo/MotionSystem.cs:80`（实现）与 `docs/z3a_z4_r1_claude_review.md:31`（评审建议限制在 Debug）。`z4/`、`tools/z4*`、所有 BUILD_NOTES.md、env.json 中 `release`（不分大小写）只命中 AnimationLifecycle 的 `Released` 状态 / `release_motion_continued` 字段，**无 Release 构建配合 FM_FLUIDMOTION_PATH 的用法**；K93 本机 `%TEMP%\z4_formal_k93\*.cmd/*.log` 亦无。Z4 构建均为 Debug。→ 不停，继续。

## 1. 改动

| 文件 | 改动 |
|---|---|
| `MotionSoftDemo/MotionSystem.cs` | 静态构造中读取 `FM_FLUIDMOTION_PATH` 的整段包进 `#if DEBUG`（Release 忽略）；`DestroyCore` 在帧泵开着时关泵后补发一次 `MotionEtw.Log.IdleTransition(session, 0, 0)`（原 `OnRender` 在 `_disposed` 后不再发） |
| `MotionSoftDemo/Z3aL1DevSmoke.cs` | 新增**必测**用例：新建 manual 系统 → 空动画 Play + Retarget(from: 0 → 120) → 帧泵开着时 Dispose，须恰好 1 条 `IdleTransition`（activeCount=0, pumpOn=0），且 `FramePumpChanged(false)` 恰好 1 次 |
| `docs/api_changelog.md` | 加一行 internal fix（环境变量仅 Debug；Dispose 多一条 IdleTransition；公开面 / fm_* / E1 不变；Z4 r1 仍绑定 fe59c622；新闸门须绑定含 T3 的 tip） |

## 2. 验收

| 项 | Debug | Release |
|---|---|---|
| fm_capi_test（`/Od /MTd /D_DEBUG /RTC1` vs `/O2 /MT /DNDEBUG`） | exit 0，**23 checks, 0 failures** | exit 0，**23 checks, 0 failures** |
| devsmoke 控制台（`z3a_l1_devsmoke`，net8） | **verdict=PASS fails=0**；E1 六名齐；`Z5T3_DISPOSE_IDLE emitted=1 pumpOffCallbacks=1` | 同左 |
| devsmoke GUI（`MotionSoftDemo.exe --z3a-l1-dev`，非打包，`schtasks /IT /RL LIMITED`，跑完删任务） | **verdict=PASS fails=0**；E1 六名齐；emitted=1 | 同左 |
| 公开面反射对表（基线 2e57bff devsmoke 程序集 vs T3 程序集，同配置） | diff **0 行**；CheckSurface fails=0 | diff **0 行**；CheckSurface fails=0 |
| `FM_FLUIDMOTION_PATH=C:\z5_nonexistent\fluidmotion.dll` 诊断运行 | 按设计读取并加载失败（DllNotFoundException，verdict=FAIL，仅诊断） | **被忽略**，verdict=PASS |
| fm_capi.h / fm_capi.cpp / composition.h 相对 2e57bff | `git diff --quiet` exit 0（未改） | — |
| composition.h 归一化 PRE / POST | `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` / 同 | — |

E1 名字 / id / 字段 / GUID 未改（`MotionEtw.cs` 本票未动）。sha256 见 `hashes.txt`。

## 3. 如实记录的偏差

1. **新用例第一次运行 FAIL（测试自身前置条件写错，非产品代码）**：首版用例对全新系统、从未采样过的属性调用 `Retarget("offsetX", 120)`（from=null），原生侧拒绝（返回 false），帧泵未保持开启，4 条 FAIL。改为 `from: 0` 后通过。首次输出原样保留在 `attempt1_testbug/`。这一事实（from=null 对从未采样的属性会被拒绝）也记入 T5 报告。产品代码（MotionSystem.cs）未因此改动。
2. **GUI Debug 首次运行用了旧二进制**：CopyFromBox 保留了盒内文件 mtime，增量构建判定 Debug 已是最新，导致 GUI Debug 跑的是含首版用例的程序集（FAIL 4）。以 `--no-incremental` 重建 Debug 后重跑 PASS。旧运行原样保留在 `attempt1_stale_debug_gui/`。Release GUI 首次即为新版本（PASS）。
3. 输出中系统中文本地化文字按原始字节保存，可能显示为乱码。
4. 本地构建产物（`MotionNative/`、`_z5t3_build_*`、`_z5t3_obj_*`、`%TEMP%\z5_wave2\`）未提交。反射对表工具源码（`SurfaceDump_Program.cs`，dev-only，位于 `%TEMP%\z5_wave2\tool`）附在本目录以便复核。
