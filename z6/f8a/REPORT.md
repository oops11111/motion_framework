# Z6 F8A REPORT（未复核）

- 票：F8A（handover r2 第三节 C 类），只写闸门草稿 `docs/z6_f8a_gate_DRAFT.md`。
- 分支：`wb/z6-f8a`，从 main `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a` 开出（不叠加其他分支；定稿写明 F8a、F8b 草稿可并行，未要求合为一支，故分开）。
- worktree：`%USERPROFILE%\mf_z6\f8a`（由侧克隆 `%USERPROFILE%\mf_z3a` 建立）。
- 许可原话（pidan，卡 t206s992，2026-11-18 07:01 AWST）：「wb/z6-* 分支可以普通 push（常设许可）」

## 做了什么
- 新建 `docs/z6_f8a_gate_DRAFT.md`：STATUS=DRAFT（NOT LOCKED），LOCKED_AT / GATE_BODY_SHA256 / APPROVED_* 留空；绑定 main 7e2feb0e；全部门槛、口径、机台、会话数标「待定」，起草方未填任何数值；T6 / Z4 旧记录只作背景引用（相对路径）。
- `z6/f8a/f8a_composition.ps1`（复制自 f7 同名脚本）与 `z6/f8a/raw/composition_pre.txt`、`composition_post.txt`。
- `z6/LEDGER_snapshot.md`：LEDGER 追加后的快照（在 `z6/f8a/**` 之外，按定稿第四节要求，与 F3/F1/F4/F7 一致）。

## 没做什么
- 未写、未编译、未运行任何探针或计时代码；未试跑任何工具；未采数；未读取任何 PresentStats / FrameTick 字段值。
- 未新建任何 LOCK / APPROVED 文件；未改任何源文件、`*_LOCKED.md`、`docs/api_v0_frozen.md`、`docs/api_changelog.md`、E1、公开面。
- 未开 PR，未合 main，未 force / amend / rebase / reset --hard，未打 tag。

## composition.h
- PRE / POST 均：blob `fd0603c17136c067e89c71e56aef310f2f7e63f4`，normalized `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`，match=True（见 raw/）。

## 留给评审 / pidan
- 草稿 G3 列出的待裁决问题（Q1–Q6）全部未裁决。

## 偏差（2026-11-18 追加，未复核）
1. **文件名大小写错误与第二提交改正**：提交脚本里两个 PowerShell 变量名只差大小写（PowerShell 变量名不区分大小写），后者覆盖前者，导致首个本地提交把草稿记为 `docs/z6_F8A_gate_DRAFT.md`，并漏收 `z6/f8a/**`。push 前发现，按禁 amend 规则追加第二个提交，用 `git mv` 改回 `docs/z6_f8a_gate_DRAFT.md` 并补收 `z6/f8a/**`，之后才 push。故本分支在 main 之上有多个提交。
2. **LEDGER 大写行与更正行**：同一原因，LEDGER 中本票首两行的分支名、worktree、文件名写成大写（`wb/z6-F8A`、`%USERPROFILE%\mf_z6\F8A`、`docs/z6_F8A_gate_DRAFT.md`）。LEDGER 只追加，未改原行，另追加一条「更正」行说明实际名称为小写。
3. **composition_post 标签大写**：`z6/f8a/raw/composition_post.txt` 的 `label=` 写成 `POST F8A`（PRE 为小写 `PRE f8a`）。同一原因；哈希内容不受影响，match=True。
4. **快照含另一票行**：本机 LEDGER 为 F8a、F8b 共用，本分支的 `z6/LEDGER_snapshot.md` 也含 F8b 的行。
