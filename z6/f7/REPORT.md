# Z6 F7 · 合 main 后的 L1 pack 整合收尾（Release 重打，mf_tip → 7e2feb0e）— PASS（dev-only）

> **未复核**：本报告随 `wb/z6-f7` 推送，尚未经评审复核。dev-only，不采数，不计时；PERF-CI HOLD；Success≠adopt；pilot-only，不是正式 L1 类库。

## 1. 基线与标识
- 分支 `wb/z6-f7`，worktree `%USERPROFILE%\mf_z6\f7`，从 main `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a` 拉出（实际 base sha 即此；父提交 = 7e2feb0e；定稿没有要求 F7 叠在其他分支上）。
- 依据：`z6_workbuddy_handover_FINAL_r2.md`（sha256 `82ab4e29db68e943ab04b034d9e3e360c4a84f2a05451ae904772e10761dd2b1`）第三节 F7。
- F4 可选代码未做（等 pidan/评审），所以本次 pack 源就是 main 7e2feb0e 上的原样文件。
- 时间：2026-11-18 07:26–07:33 AWST（Australia/Perth, UTC+8）。

## 2. 授权留痕（逐字）
- pidan 拍板卡 **t206s992**，2026-11-18 07:01 AWST，workbuddy cli 会话：
  - 「wb/z6-* 分支可以普通 push（常设许可）」—— 本分支的普通 push 依据此条。
  - 「Z6 范围内允许用一次性计划任务跑 GUI，用完即删」—— 本票未用。
  - 「Z6 范围内松散包注册和卸载给总许可」—— 本票未用。
- 未获许可、本票未做：合 main、开/合 PR、tag、删远端分支、LOCK、APPROVED、其他系统级步骤、非原样构建开关。

## 3. 改动（`git diff --name-only 7e2feb0e..HEAD`）
| 文件 | 说明 |
|---|---|
| `pilot/l1pack/SHA256SUMS` | 重写：mf_tip = `7e2feb0e…`；新产物 sha256；源文件 `git hash-object`；output_dir 用 `%USERPROFILE%`（旧文件里的本机绝对路径一并去掉） |
| `pilot/l1pack/README.md` | 重写：顶部新增「Z6 F7 重打」一节（mf_tip 7e2feb0e、命令、验收、下游 F11 说明）；Z5-T5-0 原文**逐字保留**在「历史」节下（`git diff` 对该文件只有新增行） |
| `z6/f7/**` | 脚本（`f7_build.cmd`、`f7_verify.ps1`、`f7_collect.ps1`、`f7_composition.ps1`）、原始输出、本报告 |
| `z6/LEDGER_snapshot.md` | 台账快照（定稿第四节要求；不在 F7 验收①列出的路径内，与 F3/F1/F4 相同，已获协调认可，照样写明） |

**没有改任何源文件**：`MotionSoftDemo/MotionSystem.cs`、`MotionEtw.cs`、`code/capi/fm_capi.h/.cpp`、`code/composition.h`、`pilot/l1pack/FluidMotion.L1.csproj`、公开面、E1、`*_LOCKED.md`、`docs/api_v0_frozen.md` 都没动。对这些文件做冻结 diff，结果为空。
- 说明：`pilot/` 下已有文件原则上只追加。这两个文件是定稿 F7 明文要求「重写」的，所以按票办；README 的旧文字原样保留，SHA256SUMS 的旧值见本报告第 5 节和 git 历史。

## 4. 命令（原样构建，无非原样开关）
- `z6/f7/f7_build.cmd`：
  1. vcvars64（VS 2022 BuildTools 17.14.34，`Platform=x64` 由环境带入，与 Z5-T5-0 相同）；
  2. `cl /nologo /std:c++17 /EHsc /O2 /W3 /LD … code\capi\fm_capi.cpp` → `MotionNative\fluidmotion.dll`（csproj 从这里复制进 pack 输出）；
  3. `dotnet build pilot\l1pack\FluidMotion.L1.csproj -c Release --no-incremental`；
  4. 用库内 Z5-T3 SurfaceDump 工具源（`z5/t3_results_dev/K93N3P8/SurfaceDump_Program.cs`），对**新 pack DLL**做公开面 dump 并跑 CheckSurface；
  5. 健全性回归：fm_capi_test Debug/Release（cl 同 Z5-T3 参数）和控制台 devsmoke Debug/Release（`--no-incremental`）。
- `z6/f7/f7_verify.ps1`：`Get-FileHash` 独立重算产物 sha256；`git hash-object` 核对两个源文件；公开面逐行比较。

## 5. 结果（原始输出在 `z6/f7/raw/`）
### 5.1 F7 验收
| 验收项 | 结果 |
|---|---|
| ① `git diff --name-only 7e2feb0e..分支` | `pilot/l1pack/README.md`、`pilot/l1pack/SHA256SUMS`、`z6/f7/**`，另有 `z6/LEDGER_snapshot.md`（见第 3 节） |
| ② `git hash-object` | MotionSystem.cs = `fe72b08f9445723f926a2928345e23311503efec` ✓；MotionEtw.cs = `0404498ab4cb7e96168405a7ffb63591686797ca` ✓（`verify.txt` 两行 `match=True`） |
| ③ 对新 pack DLL 跑 CheckSurface | `CHECKSURFACE fails=0`（`surface_pack_release.checksurface.txt`），工具退出码 0 |
| ④ 新 DLL 公开面 dump 与 `pilot/fluenthub/surface_l1pack.txt` 比较 | 47 行对 47 行，`differing_line_numbers=[]`，**完全相同**（首行也相同）；dump 文件 sha256 `7b67b77bce6b36dc58cc870a17fe1b5588df3c6e526b39ca5a3e36d5280bc5cc`，与 T5 记录的 surface_l1pack 相同 |
| ⑤ `Get-FileHash` 独立再算 | FluidMotion.L1.dll `94d6489d9bec289732ea3e4a8540b052e576a7182bae3f3367a3e72f04e10dc2`；fluidmotion.dll `add4857dead413f8211941381c6aa8f73c546f3ecae1774aced79c464719577d`；demo_motion.json `8d5db5775acad5db0c04cbc3c2a214bb7309670c304b71ff40860757973c410e`，均与新 SHA256SUMS 一致 |
| Release pack 构建 | exit 0，0 警告 0 错误；fluidmotion cl exit 0 |

旧值（Z5-T5-0，mf_tip `d618140b`）：FluidMotion.L1.dll `2e2af911…9865`、fluidmotion.dll `b39d58ef…6996`、demo_motion.json `8d5db577…410e`、MotionEtw.cs `565111f0…`。
- DLL sha 变化的原因：MotionEtw.cs 在 main 上含 Z5 T2 提交 `f5bdf96` 的改动（T5-0 构建于 `wb/z5-t3`，没有这次提交）；另外 MSVC/Roslyn 产物本身不是逐字节可复现的。
- 公开面完全相同，`fm_capi.cpp` blob 没变。

### 5.2 健全性回归（测试计数）
| 项 | Debug | Release |
|---|---|---|
| fm_capi_test | 23 checks, 0 failures | 23 checks, 0 failures |
| devsmoke 控制台 | `verdict=PASS fails=0`，含 `Z5T3_DISPOSE_IDLE emitted=1 pumpOffCallbacks=1` | 同左 |
| devsmoke 构建 | exit 0 | exit 0 |

### 5.3 composition.h
- PRE（07:28）和 POST（07:31）：blob `fd0603c17136c067e89c71e56aef310f2f7e63f4`，归一化 sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`，`match=True`。PRE=POST。

## 6. 偏差与说明
1. **日志处理**：入库副本里本机路径换成 `%USERPROFILE%`；devsmoke 两份日志各 237 行含 FrameTick，其中 235 个 `payload=` 改为 `[omitted]`。原件留在 `%TEMP%\z6\f7\`，清单见 `raw/redaction_manifest.txt`。
2. `z6/LEDGER_snapshot.md` 在验收①列出的路径之外（见第 3 节）。
3. **csproj 里有本机绝对路径**：`pilot/l1pack/FluidMotion.L1.csproj` 注释里有一处 Z5 时写入的本机绝对路径（FluentHub 侧叉位置）。F7 规定「不改任何源文件」，本票没有动它，留给 F11 或另行决定。
4. **pack 构建依赖 MotionNative**：pack 输出里的 `fluidmotion.dll` 来自 worktree 下未跟踪的 `MotionNative\`（csproj 的 `Condition="Exists(...)"`），与 Z5-T5-0 做法相同。
5. 第一次就通过，没有 attempt1。

## 7. 副作用与清理
- 新 worktree `%USERPROFILE%\mf_z6\f7`，本地和远端新增分支 `wb/z6-f7`（删远端分支须另行请示）。
- worktree 内未跟踪的产物：`MotionNative\`、`pilot\l1pack\bin\`、`pilot\l1pack\obj\`（bin/obj 被 .gitignore 忽略）。**F11 要引用这里的 pack DLL，暂不删除**。
- `%TEMP%\z6\f7\`（含 tool、capi_*、ds_*）、`%TEMP%\z6\f7src\`：证据已入库，可删。NuGet 缓存。
- 没有安装、注册表、包注册、计划任务或系统设置改动。

## 8. 停下点
- 无。F11 接着做，用本文件的新 sha。
