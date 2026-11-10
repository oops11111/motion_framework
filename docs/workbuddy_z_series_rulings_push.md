# workbuddy 派工：推 Z-series rulings docs → GitHub

- **TICKET**：Z-SERIES-RULINGS-DOCS-PUSH
- **BRANCH**：`claude/m3-matrix`（不合 main）
- **REPO**：https://github.com/oops11111/motion_framework
- **机**：DESKTOP-K93N3P8（旁路克隆目录，勿动 `C:\Users\admin\motion_framework` 非 git 树）
- **BASE tip（必须先对齐）**：`6464d9931b57e453103984991f49b1f61a422e81`
- **冲突（写死）**：协调任务书曾写「叠在 `b210f1b565fae67d83f5d2edb5254a41fd766993` 之上」。该 tip 之后，远程已有 `78972f0`（Z4 DRAFT）、`54151ac`、`d4692f4`（附录 / INVALID / 后端路径 / r4 采用）、`0c1bd62`（L1 冻结）、`6464d99`（Z3a 彩排派工）。**禁止**把本 tar 解到 `b210f1b5` 或 reset 到该 tip；必须 `git fetch` 后在 **`6464d99`（当前远程 tip）之上**解包提交，否则会覆盖 11-06 已合入的合同冻结与 Z4 正文。
- **tar**：`z_series_rulings_20261110.tar.gz`（sha256 见协调消息 / 本文件落盘后由打包方填写；解到克隆根）
- **UPDATED**：2026-11-10 22:35 AWST（Australia/Perth）
- **依据**：pidan 于 2026-11-10 22:35 AWST 对协调五条建议回复「确认」（再确认 2026-11-06 已落口径）

## 步骤

1. 确认旁路克隆（例 `C:\Users\admin\mf_m3` 或既有 `claude/m3-matrix` 工作树）`git fetch origin` 且在 `claude/m3-matrix`。
2. 核对 `git rev-parse HEAD` **等于** `6464d9931b57e453103984991f49b1f61a422e81`。若落后，先快进到该 tip；若超前，停下来回报，勿强推、勿 rebase 覆盖。
3. 解 tar 覆盖 `docs/`（只这些路径）。本包含 11-06 之后已在远程的文件之**增量修订**，以及两份此前未入库的预算 DRAFT。
4. `git status` 应仅见 docs 变更（见下列文件）。
5. commit：
   ```
   docs: Z-series rulings (L1 frozen, Z4 DRAFT, budget SUPERSEDED, r4 conditional adopt)
   ```
   body 可写：pidan confirmed 2026-11-10 22:35 AWST (reaffirm 2026-11-06). Freeze = L1 contract only; coding still needs a separate ticket; Success≠adopt; no main merge; Z4 DRAFT not LOCKED; PERF-CI HOLD; budget DRAFTs SUPERSEDED (banners only).
6. `git push origin claude/m3-matrix`
7. 回报 tip SHA + `git ls-remote origin refs/heads/claude/m3-matrix` + 落地文件列表与 sha256。

## 本包文件与 sha256（落盘全文）

| 文件 | sha256 | 说明 |
|---|---|---|
| `docs/Z_SERIES_CONVERGE.md` | `ddac1060cbac768f9055984959ad53155b8aa7c0d3be472a6bb64bfa7a113b96` | 主文档；文内 FILE_SHA256（排除自身行）=`6e9ff2fe4735e0e3fd541a99113d538bd5508ace6a0177c3187fb2c7ddb4e795` |
| `docs/api_v0_frozen.md` | `f4323c6800aee5de993f1eacb1df3da03e6ca4952d27e70a55edc5f1c5eccf3e` | L1 FROZEN；补 2026-11-10 再确认 |
| `docs/z4_etw_l1_smoke_gate_DRAFT.md` | `fe27f218e000f2f02bdf6f0f523f1bfe884653765b88fdac72c8afbcac05e557` | 既有 DRAFT 上补再确认；仍 NOT LOCKED；E1–E4 仍开 |
| `docs/m3_matrix_pacing_r4_adoption.md` | `c154d4d5091dd2cd3a465a43c5cc7b09ac4d585b53eb8c9e7f5db1027c712d1e` | 复用既有采用记录（不另开 conditional_adopt） |
| `docs/m3_matrix_pacing_r4_next.md` | `b3608f6da157dd4ee2a2ebde32087a4223d3d2a9f66e2b495a1efd3f41e30e8f` | 仅文首指针；正文快照不改 |
| `docs/m3_matrix_budget_gate_DRAFT.md` | `2134dcbf531e4353d0411082acbc8e8d5a679395c45a1f9bca25e177758eed39` | **新入库** + SUPERSEDED 横幅；文首前 sha `49bbc9757feeade1f5b571c317e01be2c9d14fb9e464fa98438e5f31dba54745` |
| `docs/m3_matrix_render_budget_gate_DRAFT.md` | `20fb4c663c32ad494ed0985381715527fa7e090f2f4f0f43c6cf4a2540458ab7` | **新入库** + SUPERSEDED 横幅；文首前 sha `e10bf9e5bf2944f13faec600a2db1de914d214a55025714ecc921fb9550b5dd1` |
| `docs/m2_mainline_brief.md` | `9d50e07ece3912c3df149d2b512ae6dcb5c5f613cee6da8ee936df553bb0896a` | 只追加 2026-11-10 条目（Y4） |
| `docs/workbuddy_z_series_rulings_push.md` | （本文件；打包后填全文 sha） | 本派工单 |

## 禁

- 不合 main；不改 composition.h；不采数；不改 results；不 LOCK Z4；不解冻 PERF-CI。
- **禁止**以 `b210f1b5` 为底；**禁止** force-push。
