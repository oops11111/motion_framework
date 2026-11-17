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
