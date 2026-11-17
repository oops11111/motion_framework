# L1 试点打包（Z5-T5-0）· pilot-only

> **pilot-only**：这不是正式 L1 类库；正式抽库须另行批准。**不对外分发**。
> composition.h 冻结；L1 公开面 / fm_* / E1 不改；不采数；PERF-CI HOLD；Success≠adopt；不合 main。

- 派工：`workbuddy_z5_wave2_dispatch.md` Z5-T5-0；开票：pidan 2026-11-18 00:46 AWST「开 Z5-T3、Z5-T5 编码票」
- 分支 `wb/z5-t5-pack`，起点 `wb/z5-t3` tip `d618140b4aaaec1c1a61e42c57e35ea3004f54f5`（含 T3：`FM_FLUIDMOTION_PATH` 仅 Debug 读取；本包为 Release，**不读该环境变量**）
- 本目录只新增三个文件：`FluidMotion.L1.csproj`、`README.md`、`SHA256SUMS`。

## 做什么

- `FluidMotion.L1.csproj` **链接编译** `MotionSoftDemo/MotionSystem.cs` 与 `MotionSoftDemo/MotionEtw.cs`，不改源码；namespace 仍为 `MotionSoftDemo`；`FluidMotion.L1` 只是文件名（输出因此为 `FluidMotion.L1.dll`，未另设 AssemblyName）。Nullable / ImplicitUsings 与 `z3a_l1_devsmoke` 一致（enable / enable），TargetFramework 同为 net8.0。
- 这是 **devsmoke 之外唯一允许的链接编译点**。`MotionSoftDemo/z3a_l1_devsmoke/Z3aL1DevSmoke.csproj` 及其注释未改。
- **FluentHub 不编译 L1 源码**，只消费带 `SHA256SUMS` 的预编 DLL（`FluidMotion.L1.dll` + `fluidmotion.dll` + `demo_motion.json`），引用前须核对 sha256。

## 验收（2026-11-18 01:13 AWST，K93N3P8）

| 项 | 结果 |
|---|---|
| Release 构建 | `dotnet build … -c Release --no-incremental` exit 0（0 警告 0 错误）；`fluidmotion.dll` cl exit 0 |
| 两个源文件相对基线 | `git diff --quiet wb/z5-t3 -- MotionSoftDemo/MotionSystem.cs MotionSoftDemo/MotionEtw.cs` **exit 0**（diff=0）；相对 2e57bff：MotionEtw.cs exit 0，MotionSystem.cs exit 1（即 T3 的改动，预期） |
| CheckSurface 同款反射对表（对 pack 出的 `FluidMotion.L1.dll`） | 20 项全部 ok，fails=0 |
| 公开面对比（pack DLL vs T3 devsmoke Release 程序集） | 仅差 `MotionSoftDemo.Z3aL1DevSmoke`（devsmoke 自身类型，本包不含，预期）；L1 类型与成员完全一致 |
| sha256 | 见 `SHA256SUMS` |

## 已知事项

- `Platform=x64` 由 vcvars64 环境变量带入，输出目录为 `bin\x64\Release\net8.0\`；DLL 本身为 AnyCPU IL。
- 构建产物不入库；本地位置见 `SHA256SUMS` 头部。
