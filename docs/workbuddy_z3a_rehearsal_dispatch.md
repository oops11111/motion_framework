# 派题 · Z3a 彩排（DESKTOP-K93N3P8，不采数）

> Wave B 第一步（`docs/Z_SERIES_CONVERGE.md` “开 API v0 前清单”第 4 项：Z3a 跑法 + lab 彩排一次）。API v0 L1 已冻结（`docs/api_surface_m2_alpha_contract_LOCKED.md`，`77416e8b…3a0f`）。
> **这是彩排，不是采数**：不需要 LOCK / APPROVED，**结果不提交、不进任何结题**，只回报能不能跑通、卡在哪里。代码不改。

## 目的
确认 MotionSoftDemo 里现有的 C ABI 软件桥（`fluidmotion.dll` + `FmBridge` / `MotionEngine` + `M202Probe`）在当前分支上仍能构建并跑通 **Play → Retarget → Cancel** 和 ETW 最小 6 事件。“开 API v0”编码会把这层包装改成冻结的 L1 名字（`MotionSystem` / `AnimationHandle` / `MotionTokens`），之后的 Z4 采数也用同一个宿主；先确认基础能跑，免得把旧问题算到新代码头上。

## 0. 前提
- DESKTOP-K93N3P8（M2-02 的原始机器；脚本里的路径是 `C:\Users\admin\motion_framework`）。不需要管理员。
- 本机为 XAML 票做的临时修改（如有）先还原，用分支上的版本。

## 1. 取代码
```powershell
git -C C:\Users\admin\motion_framework fetch origin claude/m3-matrix
git -C C:\Users\admin\motion_framework checkout claude/m3-matrix
git -C C:\Users\admin\motion_framework pull origin claude/m3-matrix
git -C C:\Users\admin\motion_framework log -1 --format=%H
```

## 2. 构建
1. 重新构建 `fluidmotion.dll`（x64 MSVC，见 `docs/api.md` §7.4）：
   ```bat
   cl /nologo /std:c++17 /EHsc /O2 /W3 /LD /I code\capi /I code /I code\explore code\capi\fm_capi.cpp /Fe:fluidmotion.dll
   ```
   按 M2-02 时的方式放到 MotionSoftDemo 能加载的位置（csproj `Content` 链接，或 exe 旁边）。
2. 构建 MotionSoftDemo（Debug, x64；runner 用 `dotnet run --no-build`，所以必须先构建）。
3. 失败就停下回报完整错误（最后 80 行），不要改代码去凑。

## 3. 彩排
1. **先备份** `artifacts\m2_02_*`（runner 会覆盖这些已入库的 M2-02 证据）：
   ```powershell
   New-Item -ItemType Directory -Force C:\Users\admin\z3a_backup | Out-Null
   Copy-Item C:\Users\admin\motion_framework\artifacts\m2_02_* C:\Users\admin\z3a_backup\
   ```
2. 运行：
   ```powershell
   powershell -ExecutionPolicy Bypass -File C:\Users\admin\motion_framework\artifacts\m2_02_run_sessions.ps1
   ```
3. 看 `artifacts\m2_02_s1_summary.txt`…`s3`：每个会话的 verdict，Play / Retarget / Cancel 命中数，`jumpFails`；`artifacts\m2_02_s*_etw_names.txt` 里是否 6 个事件名都在。
4. **还原**：`git -C C:\Users\admin\motion_framework restore artifacts` 并删除本次新生成的未跟踪文件（`git status` 确认 `artifacts\` 无改动）。

## 4. 回报（一行，不提交任何文件）
`Z3a-REHEARSAL | commit=<sha> | fluidmotion.dll=<BUILD_OK/失败> | demo=<BUILD_OK/失败> | sessions=<n> | verdict=<s1/s2/s3> | play/retarget/cancel=<命中数> | jumpFails=<…> | etw6=<全/缺哪些> | composition.h PRE=POST | artifacts 已还原=<是/否> | 问题=<无/…>`

## 禁令
不改 composition.h；不改代码；不提交结果；不把本次彩排当作 Z4 或任何票的数据；禁影子 IV；PERF-CI 仍 HOLD。
