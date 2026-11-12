# workbuddy 派工：Z3a C-ABI L1 构建与开发冒烟（K93，不采 Z4）

- **TICKET**：Z3A-CABI-L1-BUILD
- **机**：DESKTOP-K93N3P8（旁路克隆；勿动 `C:\Users\admin\motion_framework` 非 git 树，除非该树就是本票工作副本且已确认）
- **编码分支**：`claude/z3a-cabi-l1`
- **建议起点**：`dabd5b780818af2ee2a2dcde47f2fdf788b1011e`；若 `origin/claude/m3-matrix` 已包含「Z3a C-ABI L1 ticket + Z4 r1 LOCK」文档提交，则从**该提交**起分支 / 快进，不要从更旧 tip 起。
- **规范票文**：`docs/z3a_cabi_l1_ticket.md`
- **Claude 派工**：`docs/claude_z3a_cabi_l1_dispatch.md`（代码由 Claude 写；CloudAgent 不可用）
- **UPDATED**：2026-11-10 23:09:00 AWST
- **依据**：pidan 2026-11-10 23:07 AWST「继续，按建议进行」

## 你的角色

只负责在 K93 上**取代码、构建、跑单测与开发用冒烟、回报**。不要自己扩范围改设计，不要把本次跑数写成 Z4。

若 Claude 的提交 / 补丁尚未到达：先确认分支存在与否，**停下回报等待**，不要在 `MotionEngine` 旧名上装作本票已完成，也不要擅自改代码凑绿。

## 步骤

1. `git fetch`。确认 `origin/claude/m3-matrix` 含本票文档（或先按 `docs/workbuddy_z3a_z4_lock_push.md` 落地文档）。
2. 检出或创建 `claude/z3a-cabi-l1`（起点见上）。不要 force-push `main` 或 `claude/m3-matrix`。
3. 确认工作树已有 Claude 的 L1 实现。记下 `git rev-parse HEAD`。
4. 构建 `fluidmotion.dll`（x64 MSVC，`docs/api.md` §7.4），放到 MotionSoftDemo 能加载的位置（与 M2-02 / Z3a 彩排相同）。
   ```bat
   cl /nologo /std:c++17 /EHsc /O2 /W3 /LD /I code\capi /I code /I code\explore code\capi\fm_capi.cpp /Fe:fluidmotion.dll
   ```
5. 构建并跑 `fm_capi_test`（及本票新增的单测）。失败则停下，回报最后 80 行，不要改代码去凑。
6. 构建 MotionSoftDemo（Debug, x64）。
7. **开发用冒烟**（不算 Z4）：用已改成 L1 名字的探针跑 Play→Retarget→Cancel，确认能打出 E1 事件名。产物放到本票临时目录（例如 `%TEMP%\z3a_cabi_l1_dev\`），**不要**写入 `artifacts\m2_02_*`，**不要**当作 Z4 会话，**不要**提交结果目录。
8. 核对 `composition.h` PRE=POST。`git status` 不应出现 FluentHub / Files / `main` 合并。

## 回报（一行）

`Z3A-CABI-L1 | branch=claude/z3a-cabi-l1 | tip=<sha> | fluidmotion.dll=<BUILD_OK/失败> | fm_capi_test=<PASS/失败> | demo=<BUILD_OK/失败> | dev_smoke=<OK/失败> | etw_names=<列出/缺哪些> | composition.h PRE=POST=<是/否> | 非Z4=<是> | 问题=<无/…>`

## 禁

- 不采 Z4；不填 `APPROVED_*`；不等同「APPROVED Z4 r1」。
- 不改 `composition.h`；不合 `main`；不 force-push。
- 不改 FluentHub / Files；不把 0.3·T 写成 API 默认；不做真实合成器后端。
- 开发冒烟结果不进任何结题，不算 Z4。
- PERF-CI 仍 HOLD；禁影子 IV。
