# workbuddy 派工：推 Z4 r1 APPROVED 留证 → GitHub

- **TICKET**：Z4-R1-APPROVED-DOCS-PUSH
- **BRANCH**：`claude/m3-matrix`（不合 main）
- **REPO**：https://github.com/oops11111/motion_framework
- **机**：DESKTOP-K93N3P8（旁路克隆目录，勿动 `C:\Users\admin\motion_framework` 非 git 树）
- **BASE tip（必须先对齐）**：`7845d80b7cf57424748892b60dfc84f78bb7b484`
- **冲突（写死）**：本包必须叠在 **`7845d80b7cf57424748892b60dfc84f78bb7b484`** 之上。`git fetch` 后若 `HEAD` 或 `origin/claude/m3-matrix` **不是**该 tip（已超前或对不上），**停下来回报**，禁止 force-push，禁止 rebase 覆盖，禁止重置到更旧 tip。
- **tar**：`z4_r1_approved_20261110.tar.gz`（全文 sha256 见协调回报与 `_push/z4_r1_approved_20261110.sha256`；解到克隆根）
- **UPDATED**：2026-11-10 23:16 AWST（Australia/Perth）
- **依据**：pidan 于 2026-11-10 23:16 AWST 在对话中发送「APPROVED Z4 r1」。

## 步骤

1. 确认旁路克隆 `git fetch origin` 且在 `claude/m3-matrix`。
2. 核对 `git rev-parse HEAD` **等于** `7845d80b7cf57424748892b60dfc84f78bb7b484`。若落后，先快进到该 tip；若超前或不一致，停下来回报，勿强推、勿 rebase 覆盖。
3. 解 tar 覆盖 / 新增 `docs/`（只这些路径）。
4. `git status` 应仅见下列 docs 变更。**不得**改动 `docs/z4_etw_l1_smoke_gate_r1_LOCKED.md`（GATE_BODY_SHA256 必须仍为 `acb89d1b0cb975e5d63ae2c6fc47adcbc74dddd959b63089b2546b692cadab14`）。
5. commit：
   ```
   docs: Z4 r1 APPROVED (collection blocked on Z3a build)
   ```
   body 可写：pidan sent 「APPROVED Z4 r1」 at 2026-11-10 23:16 AWST. Leave证 in docs/z4_etw_l1_smoke_r1_approved.md; LOCKED gate body not edited. Collection still blocked until claude/z3a-cabi-l1 exists and passes ticket acceptance; collection tip must be declared. Results dir z4/results_etw_l1_smoke_r1/. Machines/sessions per E2. Success≠adopt; no main merge; PERF-CI HOLD.
6. `git push origin claude/m3-matrix`
7. 回报 tip SHA + `git ls-remote origin refs/heads/claude/m3-matrix` + 落地文件列表与 sha256，并复核 LOCKED 闸门 hash 未变。

本包只推文档。不创建 `claude/z3a-cabi-l1`，不启动 Z4 采数。

## 本包文件与 sha256（落盘全文）

| 文件 | sha256 | 说明 |
|---|---|---|
| `docs/Z_SERIES_CONVERGE.md` | `b6ba189c6894f3f3a77d88a4a0ab7f81ae9831ff4f6ac4544769691e79659b6e` | 主文档；文内 FILE_SHA256（排除自身行）=`ae31c4f9893dcdeee5808b1c4b05a4e7b609123a7eb55c487a61bc599b8799de` |
| `docs/z4_etw_l1_smoke_r1_approved.md` | `45227bc78ebebb19c1d2eb583c4159e5ffd3994c3fbd9c0d1e2b98467c296ae4` | APPROVED 留证；采数仍阻断 |
| `docs/workbuddy_z4_r1_approved_push.md` | （本文件；全文 sha 见打包方回报） | 本派工单 |

## 禁

- **禁止**修改 `docs/z4_etw_l1_smoke_gate_r1_LOCKED.md`（含填 APPROVED_* 占位）。
- 不合 main；不改 composition.h；现在**不采数**（Z3a L1 构建尚未存在）；不改 results；不解冻 PERF-CI。
- **禁止**以 `7845d80b` 以外的 tip 为底（若远程已超前则停报）；**禁止** force-push。
- 本 push **不**创建编码分支，**不**把本 APPROVED 当成可以立刻开跑的授权。
