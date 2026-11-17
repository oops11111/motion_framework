# Z6 F3 · `fm_capi_test` 补「空动画 + Play + Retarget」C 层用例（K93N3P8，dev-only）

> 只改 `code/capi/fm_capi_test.cpp`（纯追加一个用例块），不改 `fm_capi.h` / `fm_capi.cpp` / composition.h / L1 公开面 / fm_* / E1 / 任何 `*_LOCKED.md` / `docs/api_v0_frozen.md`。不采数（本报告不含计时或帧率结论）；不计任何闸门会话；PERF-CI HOLD；Success≠adopt；不合 main。

- 依据：`z6_workbuddy_handover_FINAL_r2.md`（sha256 `82ab4e29db68e943ab04b034d9e3e360c4a84f2a05451ae904772e10761dd2b1`）第三节 F3。
- 机台：DESKTOP-K93N3P8，用户 admin。
- 分支 `wb/z6-f3`，**本票实际 base sha** = main `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`（开工前 `git ls-remote origin refs/heads/main` 也是这个值）。worktree 在 `%USERPROFILE%\mf_z6\f3`。
- 时间：2026-11-18 07:08–07:12 AWST（Australia/Perth, UTC+8）。

## 1. 改动

在 `main()` 末尾、`fm_destroy(nullptr);` 之后、汇总 `printf` 之前，插入一个独立块（23 行，`git diff` 为 +23 / -0，原有行一行未动；原始 diff 见 `raw/fm_capi_test.diff.txt`）。这个块对应 Z5-T3 devsmoke 用例在 C ABI 层的版本：新建 manual 系统，用新 target 6，只走 `fm_*` 调用，共 12 个 CHECK：

1. `fm_create_manual` 成功，tokens 加载通过；
2. `fm_animation_create(s2, 6, nullptr)` 得到非 0 的空动画；
3. 对空动画 `fm_animation_play` 后跑帧，泵自己回到 idle（`!fm_is_running`）；
4. 对从未采样过的 `offsetX`，不带 `from` 调 Retarget，被拒（返回 0），即 Z5-T3 记录的「from=null 对从未采样的属性被拒」；
5. 被拒的 Retarget 不唤醒泵；
6. 带 `from: 0` Retarget 到 120，返回 1；
7. 泵开，render(on) 回调已到；
8. Retarget 后立即 `fm_current` 读到 `offsetX == 0.0`（起点即 from）；
9. 跑帧直到泵自己停下（帧数在 (0, 1000) 内，只作上限保护，不作计时判据），render(off) 回调已到；
10. 最后 sink 写入的 `offsetX` 正好等于 120.0；
11. 已采样后再 Retarget 到 0，不带 `from`，返回 1；
12. 步进 3 帧后泵仍在运行；随后在泵开、句柄未释放的状态下 `fm_destroy`（`fm_capi.cpp` 的约定：句柄变为 inert，不再回调）。

## 2. 构建与结果（首跑即通过，没有 attempt1）

脚本 `f3_build.cmd`，编译参数和 Z5-T3 `t3_build.cmd` 相同：Release `/O2 /DNDEBUG /MT`，Debug `/Od /Zi /RTC1 /D_DEBUG /MTd`，都是 `cl /std:c++17 /EHsc /W3`；产物在 `%TEMP%\z6\f3\`。

| 项 | Debug | Release | 原始输出 |
|---|---|---|---|
| cl | exit 0 | exit 0 | `raw/build.log`、`raw/cl_fm_capi_test_*.txt` |
| 运行 `fm_capi_test.exe code\capi\demo_motion.json` | exit 0，**`35 checks, 0 failures`** | exit 0，**`35 checks, 0 failures`** | `raw/fm_capi_test_debug_output.txt`、`raw/fm_capi_test_release_output.txt` |

改动前 main 上是 23 checks（见 Z6 F0），新增 12 个，合计 35。输出里的 `demo.enter: 69 frames (575 ms)` 是测试程序原有的固定输出行，本报告不据此作计时结论。exe 的 sha256 见 `raw/redaction_manifest.txt`。

冻结面核对：`git diff --quiet 7e2feb0e -- code/capi/fm_capi.h code/capi/fm_capi.cpp code/composition.h` exit 0。

## 3. composition.h

| | `HEAD:code/composition.h` | 归一化（CRLF→LF）sha256 | 记录 |
|---|---|---|---|
| PRE | `fd0603c17136c067e89c71e56aef310f2f7e63f4` | `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` | `raw/composition_pre.txt` |
| POST | `fd0603c17136c067e89c71e56aef310f2f7e63f4` | `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` | `raw/composition_post.txt` |

PRE = POST = 期望值。

## 4. 偏差

- 日志里的用户目录路径替换成了字面 `%USERPROFILE%`，见 `raw/redaction_manifest.txt`；替换前的原件留在 `%TEMP%\z6\f3\`。
- 构建输出里的系统中文本地化文字按原始字节保存。
- `f3_composition.ps1` 是 `z6/f0/f0_composition.ps1` 的逐字节副本。
- 结论：**PASS（dev-only），未复核**，待协调或 pidan 复核。

## 5. 副作用与清理方法

- 新 worktree 和分支：`%USERPROFILE%\mf_z6\f3`（`wb/z6-f3`，已普通 push）。
- `%TEMP%\z6\f3\`：编译产物、替换前的日志；`%TEMP%\z6\f3src\`：中转文件。证据入库后可以删。
- 没有系统级步骤，没有装任何东西。台账已追加，`z6/LEDGER_snapshot.md` 是本次提交时的副本。

## 6. 授权留痕

- push 依据 pidan 本人原话「wb/z6-* 分支可以普通 push（常设许可）」，来源拍板卡 **t206s992**，workbuddy cli 会话，2026-11-18 07:01 AWST。本分支只做普通 push（`git push -u origin wb/z6-f3`），不 force、不 amend、不 rebase。
- 同卡另外两条：「Z6 范围内允许用一次性计划任务跑 GUI，用完即删」「Z6 范围内松散包注册和卸载给总许可」，本票没有用到。
- 合 main、开或合 PR、tag、删远端分支、LOCK、APPROVED 都不在许可内，本票都没有做。
