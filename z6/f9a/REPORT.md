# Z6 F9a REPORT（未复核）

- 票：F9a（handover r2 第三节），Files（C 试点）原样基线，参照 Z5 T4。
- 分支：`wb/z6-f9a`，从 main `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a` 开出（不叠加其他分支）。
- worktree：`%USERPROFILE%\mf_z6\f9a`（由侧克隆 `%USERPROFILE%\mf_z3a` 建立）。
- 许可原话（pidan，卡 t206s992，2026-11-18 07:01 AWST）：「wb/z6-* 分支可以普通 push（常设许可）」

## 结果
- **原样基线 FAIL**，详见 `pilot/files/BASELINE.md`。Files `git rev-parse HEAD` = `cbfb4ba54a89632fbceb82ea94ada60a9bae3737`（v4.3）。
- run1（上游 CI 的 msbuild 两步，VS 2026 MSBuild 18.6.3）：restore exit=0，build exit=1（CS9057 + MSB4018 缺 MSVC 目录）。
- run2（T4 形式 `dotnet build … -c Debug -p:Platform=x64`，SDK 10.0.300）：exit=1（CS9057）。

## 停下点
- 继续须装更新的 .NET SDK（Roslyn ≥ 5.9）或 VS C++ 工作负载，或改包版本 / 加开关：均为安装或非原样，须 pidan 拍板。未做。
- F9b 未做（等 pidan 说「开」）。

## 入库
- `pilot/files/BASELINE.md`、`pilot/files/f9a_logs/**`（已脱敏日志 + MANIFEST）。
- `z6/f9a/REPORT.md`、`z6/f9a/f9a_collect.ps1`、`z6/f9a/f9a_composition.ps1`（复制自 f7）、`z6/f9a/raw/composition_pre.txt`、`composition_post.txt`。
- `z6/LEDGER_snapshot.md`。
- 不含 Files 源码；未改 Files 任何文件，故无 patch。

## 偏差
- run1 与 run2 是两条不同的原样命令（上游 CI 形式与 T4 形式），不是同一命令重跑；两次日志都入库，不放 `attempt1/`。
- run2 前对 Files 克隆执行了 `git clean -xdf`（只清 run1 产生的被忽略产物）。
- 定稿写 K93 只有 VS 2022 生成工具 17.14；实际另有 VS Community 2026 18.6（无 C++ 工作负载）。run1 用了它的 MSBuild（与上游 CI 的 vs2026 运行器一致），未改任何组件。
- composition.h PRE/POST 见 raw/。

---

## 续跑（2026-11-18 08:25–08:31 AWST，未复核）

- 许可原话（pidan，卡 t208s1016，2026-11-18 08:04 AWST）：「F9a 允许安装新版 .NET SDK（Roslyn≥5.9）和 VS 2026 C++ 工作负载」
- 常设许可（pidan，卡 t206s992，2026-11-18 07:01 AWST）：「wb/z6-* 分支可以普通 push（常设许可）」
- 本追加提交不 amend 上一笔 `75139796`。F9b 未做。未改 Files 源码，未登录 / 未 fork / 未评论第三方仓库。未花钱，未升级成付费版，未接受付费组件，未登录付费账户。未开始 F1 C / F2 / F5。

### 安装前版本
- 进程：`elevated=False`，用户 `admin`（安装器遥测 `isadmin=True` 但 `iselevated=False`）。
- 机器级 .NET SDK：8.0.428、9.0.321、**10.0.300**（`dotnet --version` = 10.0.300，路径 `%ProgramFiles%\dotnet\dotnet.exe`）。
- Roslyn（`Microsoft.CodeAnalysis.dll` ProductVersion / FileVersion）：
  - SDK 10.0.300：5.6.0-2.26230.102 / 5.600.26.23102
  - SDK 9.0.321：4.14.0-3.26064.1
  - SDK 8.0.428：4.11.0-3.25569.22
- VS：Community 2026 18.6.11822.322（`C:\Program Files\Microsoft Visual Studio\18\Community`），工作负载 ManagedDesktop + CoreEditor；另有 VS 2022 Build Tools。`%ProgramFiles%\Microsoft Visual Studio\18\Community\VC\Tools\MSVC` **不存在**。
- 详见 `z6/f9a/raw/rerun/pre/`。

### .NET SDK（用户级，已完成）
- 选定官方免费 SDK **10.0.404**（10.0.4xx 特性带，官方 releases.json `latest-sdk`，发布 2026-11-10；该带对应 Roslyn 5.9）。未装 11.0，以免 `global.json` 的 `rollForward=latestMajor` 跨到下一个主版本。
- 官方脚本：`https://dot.net/v1/dotnet-install.ps1`，sha256 `E8B873E18A81E5C4CD8AB69D84DAC8FEAD291D50B3C44633CD7FDDAD709A13D6`，76676 字节。
- 命令原文（路径入库已换为 `%USERPROFILE%`）见 `raw/rerun/sdk/cmd.txt`：

```
Invoke-WebRequest -Uri https://dot.net/v1/dotnet-install.ps1 -OutFile "%USERPROFILE%\AppData\Local\Temp\z6\f9a\sdk\dotnet-install.ps1" -UseBasicParsing
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%USERPROFILE%\AppData\Local\Temp\z6\f9a\sdk\dotnet-install.ps1" -Version 10.0.404 -Architecture x64 -InstallDir "%USERPROFILE%\.dotnet" -NoPath -SkipNonVersionedFiles:$false -Verbose
```

- 结果：exit=0，约 135s（08:27:14–08:29:28 AWST）。安装到 `%USERPROFILE%\.dotnet`（`-NoPath`，未改用户或系统 PATH）。下载 `https://builds.dotnet.microsoft.com/dotnet/Sdk/10.0.404/dotnet-sdk-10.0.404-win-x64.zip`（303618098 字节）。日志：`raw/rerun/sdk/install.log`。
- 安装后：
  - 用户级：`%USERPROFILE%\.dotnet\dotnet.exe` → SDK **10.0.404**；Roslyn FileVersion **5.900.26.53019**，ProductVersion **5.9.0-1.26529.119+2ab2854867733112c84ac12c4273d7970ac3afdf**（已满足 ≥ 5.9）。
  - 机器级 SDK 未变（仍 8.0.428 / 9.0.321 / 10.0.300）。默认未设 PATH 时 `dotnet` 仍指向 10.0.300。续跑构建须在会话里 `DOTNET_ROOT=%USERPROFILE%\.dotnet` 并把该目录插到 PATH 前面（本次因 VS 停下，尚未这样重跑）。

### VS Community 2026 C++ 工作负载（未完成，已停下）
- 未自行点过 UAC，也未用 `RunAs` 拉起提升。
- 尝试命令原文见 `raw/rerun/vs/cmd.txt`：

```
"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\setup.exe" modify --installPath "C:\Program Files\Microsoft Visual Studio\18\Community" --add Microsoft.VisualStudio.Workload.NativeDesktop --includeRecommended --passive --norestart --noUpdateInstaller
```

- `--includeRecommended` 只为凑齐免费的 C++ 桌面推荐组件（编译器 / 工具集）；未加 `--includeOptional`，未选付费 SKU。`--noUpdateInstaller` 避免顺带升级安装器。
- 结果：**exit=5007**（约 2 秒后退出）。官方含义是 “Operation was blocked - the computer does not meet the requirements”。安装器日志原文（`raw/rerun/vs/dd_installer_20261118083107.log`）：

  `Commands with --quiet or --passive should be run elevated from the beginning.`

- 遥测：`iselevated=False`，`isadmin=True`，`passive=True`。`consent.exe` 未出现，**没有弹出 UAC**（`--passive` / `--quiet` 要求进程一开始就是提升的，安装器拒绝自己去唤起 UAC）。窗口若有闪过，约 1 秒即退出。未点击任何按钮。
- 之后未再试：没有 `Start-Process -Verb RunAs`，没有去掉 `--passive` 去开完整 GUI，没有改工作负载清单。MSVC 目录仍不存在。未重跑 Files 原样构建。

### 结论（续跑至此）
- **SDK 用户级安装 PASS**（10.0.404，Roslyn 5.9）。
- **VS C++ 工作负载未安装**。需要已提升的进程（或由本人在 UAC 上同意）才能继续。原样基线 **尚未重跑**，上一笔结论仍为 FAIL（未启动）。
- F9b 不做。

### 请 pidan / 用户接着做的准确步骤
当前会话不是提升的。请任选其一（均为免费 Community 工作负载，不升级付费版）：

1. **已提升的命令提示符或 PowerShell（管理员）**中执行与上面相同的 `setup.exe modify … --passive …` 命令；若出现 UAC，由本人点「是」。或
2. 去掉 `--passive`，以交互方式打开 Visual Studio Installer，勾选「使用 C++ 的桌面开发」（`Microsoft.VisualStudio.Workload.NativeDesktop`，含推荐组件），再在 UAC 上同意。

完成后通知续跑：同一 Files v4.3（`cbfb4ba54a89632fbceb82ea94ada60a9bae3737`）先 run1 上游 CI msbuild，必要时再 run2（会话需指向用户级 10.0.404 SDK）。

### 入库（本追加提交）
- `z6/f9a/REPORT.md`（本段）、`pilot/files/BASELINE.md`（续跑段）、`pilot/files/f9a_logs/rerun/**`、`z6/f9a/raw/rerun/**`、`z6/f9a/raw/composition_post_rerun.txt`、`z6/LEDGER_snapshot.md`。
- 不含 Files 源码，不含整桌面截图，不含官方 releases.json 全文。

### composition.h
- `raw/composition_post_rerun.txt`（2026-11-18 08:31:48 +08:00）：`match=True`，blob `fd0603c17136c067e89c71e56aef310f2f7e63f4`，normalized `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`，head 仍为 `75139796`（本票不改 composition.h）。

### 偏差与副作用
- 偏差：先用 `--passive` 调用安装器，因而没有出现 UAC、直接 5007 退出；未改用交互 GUI 或 RunAs（避免自行点过 UAC）。SDK 用 `-NoPath`，未写用户 PATH。
- 副作用：新增用户级 SDK 目录 `%USERPROFILE%\.dotnet\`（10.0.404 及配套 10.0.15 packs/shared）；机器级 VS / SDK 未改；`%TEMP%\z6\f9a\` 与 `%TEMP%\dd_installer_*.log` 本机日志。曾短暂打开过 VS Installer（`--help` / `help modify`，已关闭，exit 87 / 被终止）。无包注册，无计划任务，未花钱。

### 停下点
- **停在这里**：VS Community 2026 的 C++ 工作负载需要一开始就是提升的进程，或由本人在 UAC / 安装器 GUI 上同意。未重跑基线，未自行加开关或改包版本。
