# Z6 F12 · `docs/z5_closed.md` 追加「合入记录」（K93N3P8）

> 只在 `docs/z5_closed.md` 文末追加一节，原有内容一字不改。不改任何源文件、`*_LOCKED.md`、`docs/api_v0_frozen.md`；composition.h 冻结；不采数；PERF-CI HOLD；Success≠adopt（合入 main ≠ adopt，不选 Winner）；不合 main。

- 依据：`z6_workbuddy_handover_FINAL.md`（sha256 `a7eff96135c6189edfffcd51a29892638897382af61dbc6a20d32fd9787541b6`，111 行）第三节 F12。
- 机台：DESKTOP-K93N3P8，用户 admin。
- 分支 `wb/z6-f12`，起点 main `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`（tree `d61f0138f06b27d6f184cc63b04df2ab84ab945d`）。开工前 `git ls-remote origin refs/heads/main` = `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`。
- worktree：`%USERPROFILE%\mf_z6\f12`（由侧路克隆 `%USERPROFILE%\mf_z3a` 执行 `git worktree add`）。
- 时间：2026-11-18 06:55 AWST（Australia/Perth, UTC+8）。

## 1. 做法

1. 用 `git cat-file blob HEAD:docs/z5_closed.md` 取出原文件的仓库字节（blob `52ec802bcfb76d589f63f8dbc62e1332a70cdead`，31539 字节，sha256 `f82dbd08493ca18b073ab5e7242a1da4c55806c9b87c4d96b34432047ba11b31`，LF 行尾，无 BOM，以换行结尾）。
2. 在这些字节后面直接接上追加段（UTF-8、无 BOM、LF），写回 `docs/z5_closed.md`。追加段以一个空行开头，新节编号为「11. 合入记录」（原文已有第 9、10 节）。
3. 核对：新文件前 31539 字节与原 blob 逐字节相同；`git diff` 只有 `+` 行，没有 `-` 行。

## 2. 结果

> 状态：**未复核**（本地提交，未推送）。

| 项 | 值 |
|---|---|
| 本票 base sha | `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a` |
| 新 `docs/z5_closed.md` blob | `6eb86d52b9f1a9e2ec4278be988513bf0d902e6d` |
| 新文件 sha256（仓库字节，LF，无 BOM） | `4193f9811a00956ca23755b8d0f9d6a9b084b7c74f93c597a200a8b790e76398`（32628 字节） |
| 前 31539 字节与原 blob 逐字节相同 | True |
| `git diff --numstat 7e2feb0e -- docs/z5_closed.md` | `10	0	docs/z5_closed.md` |
| 文件开头 BOM | 无（首字节 `0x23`） |
| 文件中 CR 字节数 | 0 |

原始核对输出见 `z6/f12/raw/f12_check.txt`；追加段原样见 `z6/f12/raw/append.md`。

## 3. composition.h

| | `HEAD:code/composition.h` | 归一化（CRLF→LF）sha256 |
|---|---|---|
| PRE | `fd0603c17136c067e89c71e56aef310f2f7e63f4` | `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` |
| POST | `fd0603c17136c067e89c71e56aef310f2f7e63f4` | `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` |

PRE = POST = 期望值（blob `fd0603c17136c067e89c71e56aef310f2f7e63f4`，归一化 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）。

## 4. 偏差

- 原文第 8 节「唯一待办」等句子按要求不改，只在新节里声明它们已被取代。
- 「先合」原话取自交接定稿文件（由协调转来），我没有在自己的会话里直接看到 pidan 说这句话。追加段只把它当作合入依据记录下来，不构成任何新的授权。

## 5. 副作用与清理方法

- 新 worktree 和本地分支：`%USERPROFILE%\mf_z6\f12`（`wb/z6-f12`）。清理方法：`git -C %USERPROFILE%\mf_z3a worktree remove %USERPROFILE%\mf_z6\f12`，然后 `git branch -D wb/z6-f12`。须先确认 commit 已经保存在别处。
- `%TEMP%\z6\` 下的中间文件（原 blob 副本、追加段、提交说明），可以删除。
- 没有构建，没有运行任何程序，没有系统级改动。

## 6. 授权留痕

push 尚未获 pidan 本人许可，本分支仅本地提交。

## 7. 第二次提交：标题下补一行（按交接定稿 r2）

- 依据：`z6_workbuddy_handover_FINAL_r2.md`（sha256 `82ab4e29db68e943ab04b034d9e3e360c4a84f2a05451ae904772e10761dd2b1`，150 行）变更摘要第 2 条；r2 取代 r1。第一次提交 `d3c169bf1078d3be1e6c01a46a4b2e5bc0471fb3` 不改、不 amend，本次为第二个普通提交，父提交即 `d3c169bf1078d3be1e6c01a46a4b2e5bc0471fb3`。
- 做法：仍按仓库字节处理。在第 1 行（标题行）之后、原第 2 行（空行）之前插入一行，行尾 LF，与文件一致。插入的内容原样见 `raw/title_line.txt`。这一做法沿用 Z5 C1 的先例（`b592b6c8`，在 `pilot/fluenthub/BASELINE.md` 标题下插入 `> 最新结论…` 一行）。
- 核对（原始输出 `raw/f12_check2.txt`）：
  - 相对 `d3c169bf1078d3be1e6c01a46a4b2e5bc0471fb3`：`git diff --numstat` = `1	0	docs/z5_closed.md`，`-` 行数 0；
  - 新文件 = 旧文件第 1 行 + 插入行 + 旧文件其余字节，逐字节相同：True；
  - 新 blob `7ad3e88664da3cea6ed686128d2e5fd2a7f330bc`，sha256 `1d7843e77d64bd4990a0c2bfd73c9e318048d7a2d59e44439149bc4f38bfda37`（32797 字节），CR 字节数 0，首字节 `0x23`（无 BOM）；
  - 相对 base `7e2feb0e`：`git diff --numstat` = `11	0	docs/z5_closed.md`，`-` 行数 0。
- composition.h：PRE `fd0603c17136c067e89c71e56aef310f2f7e63f4` / `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`，POST `fd0603c17136c067e89c71e56aef310f2f7e63f4` / `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`，PRE = POST = 期望值。
- 状态：**未复核**（本地提交，未推送）。push 尚未获 pidan 本人许可，本分支仅本地提交。
- 台账：本票动作已记入 K93 本地 `%USERPROFILE%\mf_z6\LEDGER.md`（只追加）。本次不推送，所以没有放 `z6/LEDGER_snapshot.md`。

## 授权留痕（追加，2026-11-18 AWST）

上文「push 尚未获 pidan 本人许可」写于许可之前，原样保留。之后 pidan 本人在拍板卡 **t206s992**（workbuddy cli 会话，2026-11-18 07:01 AWST）上答复，原话逐字如下：

- 「wb/z6-* 分支可以普通 push（常设许可）」
- 「Z6 范围内允许用一次性计划任务跑 GUI，用完即删」
- 「Z6 范围内松散包注册和卸载给总许可」

本分支据第一条做普通 push（`git push -u origin <分支>`），不 force、不 amend、不 rebase。合 main、开或合 PR、tag、删远端分支、LOCK、APPROVED、其他系统级步骤、非原样构建开关，都**不在**上述许可内。台账副本见 `z6/LEDGER_snapshot.md`（取自 K93 本地 `%USERPROFILE%\mf_z6\LEDGER.md`，取于本次提交时）。推送前结果状态为「未复核」。
