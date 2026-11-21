# L1 试点打包 · pilot-only（Z6 F7 重打：mf_tip = main `7e2feb0e`）

> **pilot-only**：这不是正式 L1 类库；正式抽库须另行批准。**不对外分发**。
> composition.h 冻结；L1 公开面 / fm_* / E1 不改；不采数；PERF-CI HOLD；Success≠adopt。

## Z6 F7 重打（2026-11-18 07:29 AWST，K93N3P8，分支 `wb/z6-f7`，**未复核**）

- 依据：Z6 交接定稿 r2 第三节 F7「合 main 后的整合收尾：用 Release 重打 L1 pack，重写 `SHA256SUMS` 和 `README.md`（mf_tip 改为 7e2feb0e）。不改任何源文件」。
- 构建源：main `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`（Z5 经 PR #9 合入后的 main）。命令与 Z5-T5-0 相同（见 `SHA256SUMS` 头部），原样构建，无非原样开关。
- 链接编译的两个源文件：`MotionSoftDemo/MotionSystem.cs` `git hash-object` = `fe72b08f9445723f926a2928345e23311503efec`；`MotionSoftDemo/MotionEtw.cs` = `0404498ab4cb7e96168405a7ffb63591686797ca`（与 Z5-T5-0 时的 `565111f0…` 不同：T5-0 构建于 `wb/z5-t3`，main 上的 MotionEtw.cs 含 Z5 T2 提交 `f5bdf96`「文档补记」的改动；本票未改该文件）。
- 产物 sha256 见 `SHA256SUMS`，并用 `Get-FileHash` 独立再算一遍（`z6/f7/raw/verify.txt`）。`fluidmotion.dll` 由同一源（`fm_capi.cpp` blob 未变）重新编译，MSVC 产物不是逐字节可复现，所以 sha 与 Z5-T5-0 的 `b39d58ef…` 不同；`demo_motion.json` 与 Z5-T5-0 相同。
- 验收证据（`z6/f7/raw/`）：Release pack 构建 0 警告 0 错误；对**新 pack DLL** 跑 CheckSurface，`CHECKSURFACE fails=0`；新 DLL 公开面 dump 与 `pilot/fluenthub/surface_l1pack.txt` 逐行比较，47 行**完全相同**（允许首行不同，实际首行也相同）。
- **下游**：F11（FluentHub 侧叉构建时校验 sha）应以本文件的新 sha 为准；之后任何票改了 pack 源（MotionSystem.cs、MotionEtw.cs、fm_capi.cpp），F7 须重做。
- 构建产物不入库；本地位置见 `SHA256SUMS` 头部。

---

## 历史：Z5-T5-0 原文（以下原样保留，sha 等数值已被上节取代）

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
