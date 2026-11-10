# workbuddy 派工：推 Z1 B/C 入席 + E1–E4 裁决 docs → GitHub

- **TICKET**：Z1-E-RULINGS-DOCS-PUSH
- **BRANCH**：`claude/m3-matrix`（不合 main）
- **REPO**：https://github.com/oops11111/motion_framework
- **机**：DESKTOP-K93N3P8（旁路克隆目录，勿动 `C:\Users\admin\motion_framework` 非 git 树）
- **BASE tip（必须先对齐）**：`0c8b7566d9e7ddeeab4f48ff65db39b699851036`
- **冲突（写死）**：本包必须叠在 **`0c8b7566d9e7ddeeab4f48ff65db39b699851036`**（当前任务书给出的远程 tip）之上。`git fetch` 后若 `HEAD` 或 `origin/claude/m3-matrix` **不是**该 tip（已超前或对不上），**停下来回报**，禁止 force-push，禁止 rebase 覆盖，禁止重置到更旧 tip。
- **tar**：`z1_e_rulings_20261110.tar.gz`（全文 sha256 见文首 **TAR_SHA256**；解到克隆根）
- **TAR_SHA256**：`2f0c27e9860f331ee73dc82699865c8ec72be703c1559279fa18434f03030f52`（本包 tar 全文；该行不计入本文件 FILE_SHA256。因派工单打进 tar，此值在打入后再写入并重打一包，以重打后的包为准，见打包说明。）
- **UPDATED**：2026-11-10 22:59 AWST（Australia/Perth）
- **依据**：pidan 于 2026-11-10 委托 GitHub 选 Z1 B/C，并书面委托「E1–E4 你来定」；随后于 2026-11-10 22:59 AWST 书面「同意 B=FluentHub、C=Files，推上去」。
- **FILE_SHA256**：`b5498947116af3bfc54deb2e96a3d972e746f621de03cbfc7ac08af9fefba9e9`（对落盘全文做 SHA-256，**排除**以「- **FILE_SHA256**」开头的那一行，以及以「- **TAR_SHA256**」开头的那一行）

## 步骤

1. 确认旁路克隆（例 `C:\Users\admin\mf_m3` 或既有 `claude/m3-matrix` 工作树）`git fetch origin` 且在 `claude/m3-matrix`。
2. 核对 `git rev-parse HEAD` **等于** `0c8b7566d9e7ddeeab4f48ff65db39b699851036`。若落后，先快进到该 tip；若超前或不一致，停下来回报，勿强推、勿 rebase 覆盖。
3. 解 tar 覆盖 `docs/`（只这些路径）。
4. `git status` 应仅见 docs 变更（见下列文件）。
5. commit：
   ```
   docs: Z1 B/C seated (FluentHub, Files) + E1–E4 rulings
   ```
   body 可写：pidan approved in writing 2026-11-10 22:59 AWST: 「同意 B=FluentHub、C=Files，推上去」. B/C seated (APPROVED BY PIDAN); fork-local only; no upstream PRs; Success≠adopt; no Winner chosen; pilot coding still needs a separate ticket. Backups (GetStoreApp / Wino-Mail) remain backups. E1–E4 coordinator rulings stand; Z4 still NOT LOCKED; no collection; PERF-CI HOLD; no main merge.
6. `git push origin claude/m3-matrix`
7. 回报 tip SHA + `git ls-remote origin refs/heads/claude/m3-matrix` + 落地文件列表与 sha256。

## 本包文件与 sha256（落盘全文）

| 文件 | sha256 | 说明 |
|---|---|---|
| `docs/Z_SERIES_CONVERGE.md` | `3107558d97aef2537515fedf433e4fcc88c8b15e49caae5becd3093c71111c59` | 主文档；文内 FILE_SHA256（排除自身行）=`f546d3834b691916ff2c7943e34e20e2243aa92d9199cb1f9920f32a06240420`；B/C 已入席 |
| `docs/z1_pilot_scoring_table_DRAFT.md` | `30f57f29dd111c4b12b3f7394f10547825dea4e5aa87c07c49e561965e632115` | B/C 标 **APPROVED BY PIDAN**（已入席）；Winner 空白；备份仍为备份 |
| `docs/z4_etw_l1_smoke_gate_DRAFT.md` | `0ba2611eb9f081b6328d60a2632f0abeeac41ff52346c94170047643eb1a085c` | 与 22:56 包相同（E1–E4 裁决未再改）；**仍 NOT LOCKED**；不授权采数 |
| `docs/workbuddy_z1_e_rulings_push.md` | （本文件；全文 sha 见文首 FILE_SHA256 约定下的哈希，以及打包方回报的落盘全文 sha） | 本派工单 |

## 打包说明（TAR_SHA256）

文首 `TAR_SHA256` 排除在本文件 `FILE_SHA256` 之外。流程：先按占位写入并计算 FILE_SHA256，再打 tar 得哈希，写入 `TAR_SHA256`，再重打一包。重打后 tar 的全文哈希若与文首值不同（因该行本身被打进包），**以重打后的包全文 sha256 为准**（亦写入 `_push/z1_e_rulings_20261110.sha256` 与协调回报）。核对应比对收到的 tar 与协调回报，而不是要求文首自指固定点。

## 禁

- 不合 main；不改 composition.h；不采数；不改 results；不 LOCK Z4；不解冻 PERF-CI。
- **禁止**以 `0c8b7566` 以外的 tip 为底（若远程已超前则停报）；**禁止** force-push。
- 入席 ≠ 选定 Winner ≠ 授权试点编码（试点编码仍须另票）。试点仅侧叉本地，不向上游提 PR；Success≠adopt。
