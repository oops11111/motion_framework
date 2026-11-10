# workbuddy 派工：推 Z3a C-ABI L1 票 + Z4 r1 LOCK docs → GitHub

- **TICKET**：Z3A-Z4-LOCK-DOCS-PUSH
- **BRANCH**：`claude/m3-matrix`（不合 main）
- **REPO**：https://github.com/oops11111/motion_framework
- **机**：DESKTOP-K93N3P8（旁路克隆目录，勿动 `C:\Users\admin\motion_framework` 非 git 树）
- **BASE tip（必须先对齐）**：`dabd5b780818af2ee2a2dcde47f2fdf788b1011e`
- **冲突（写死）**：本包必须叠在 **`dabd5b780818af2ee2a2dcde47f2fdf788b1011e`** 之上。`git fetch` 后若 `HEAD` 或 `origin/claude/m3-matrix` **不是**该 tip（已超前或对不上），**停下来回报**，禁止 force-push，禁止 rebase 覆盖，禁止重置到更旧 tip。
- **tar**：`z3a_z4_lock_20261110.tar.gz`（全文 sha256 见协调回报与 `_push/z3a_z4_lock_20261110.sha256`；解到克隆根）
- **UPDATED**：2026-11-10 23:09:00 AWST（Australia/Perth）
- **依据**：pidan 于 2026-11-10 23:07 AWST 书面「继续，按建议进行」——开 Z3a C-ABI 编码票，并准备 Z4 LOCK。

## 步骤

1. 确认旁路克隆 `git fetch origin` 且在 `claude/m3-matrix`。
2. 核对 `git rev-parse HEAD` **等于** `dabd5b780818af2ee2a2dcde47f2fdf788b1011e`。若落后，先快进到该 tip；若超前或不一致，停下来回报，勿强推、勿 rebase 覆盖。
3. 解 tar 覆盖 / 新增 `docs/`（只这些路径）。
4. `git status` 应仅见 docs 变更（见下列文件）。
5. commit：
   ```
   docs: Z3a C-ABI L1 ticket + Z4 r1 LOCK
   ```
   body 可写：pidan 「继续，按建议进行」 2026-11-10 23:07 AWST. Opens docs/z3a_cabi_l1_ticket.md (Claude writes code on claude/z3a-cabi-l1; workbuddy builds on K93; no FluentHub/Files; no Z4 collection). Locks Z4 r1 (GATE_BODY_SHA256=acb89d1b0cb975e5d63ae2c6fc47adcbc74dddd959b63089b2546b692cadab14); APPROVED_* empty; collection needs 「APPROVED Z4 r1」 in chat plus an existing Z3a L1 build. Success≠adopt; no main merge; PERF-CI HOLD.
6. `git push origin claude/m3-matrix`
7. 回报 tip SHA + `git ls-remote origin refs/heads/claude/m3-matrix` + 落地文件列表与 sha256。

本包只推文档到 `claude/m3-matrix`。编码分支 `claude/z3a-cabi-l1` 由 Claude / workbuddy 另建，不在本 push 创建。

## 本包文件与 sha256（落盘全文）

| 文件 | sha256 | 说明 |
|---|---|---|
| `docs/Z_SERIES_CONVERGE.md` | `84cf8704d6933023dae60bd7b6706b2918c71d172673980ba3cfec6c20fb7747` | 主文档；文内 FILE_SHA256（排除自身行）=`9ffbb6289f643784024a28560eabff69fdaf33879b49baccecc63bec6bcaa2e2` |
| `docs/api_v0_frozen.md` | `ec84e863a12acbd26f7bc32e2df016cc2930d02dce315ec0571d1420c5d8f048` | 记录编码票已开；仍不合 main、不采 Z4 |
| `docs/z3a_cabi_l1_ticket.md` | `cf71975232fcd5865746df170767e03457cdde43fff848d54b18a9a324b2c2d4` | 另票开 API v0 L1（C-ABI / MotionSoftDemo） |
| `docs/claude_z3a_cabi_l1_dispatch.md` | `f6bffdd51dce04e16a25bcde514089857ab4cf5694051d8e9c780f49e55c4b0d` | Claude 英文编码派工 |
| `docs/workbuddy_z3a_cabi_l1_dispatch.md` | `e04f84fef9abdce0b07cb7c6f642c258b984f3f747105b703a4a10053354a68b` | workbuddy K93 构建 / 开发冒烟 |
| `docs/z4_etw_l1_smoke_gate_r1_LOCKED.md` | `cd246e5303f51fe5bf74b21cd22b51d56954b705488c5d6310a893ee9bb8e0a9` | Z4 r1 LOCKED；GATE_BODY_SHA256=`acb89d1b0cb975e5d63ae2c6fc47adcbc74dddd959b63089b2546b692cadab14`；APPROVED_* 空 |
| `docs/z4_etw_l1_smoke_gate_DRAFT.md` | `74c46a63b87bf8c4dd9588c07e5a3b2331e5d74bf872457ecd2694059d722c7c` | 文首标 SUPERSEDED_BY_LOCKED |
| `docs/workbuddy_z3a_z4_lock_push.md` | （本文件；全文 sha 见打包方回报） | 本派工单 |

## 禁

- 不合 main；不改 composition.h；不采数；不改 results；不解冻 PERF-CI。
- **禁止**以 `dabd5b78` 以外的 tip 为底（若远程已超前则停报）；**禁止** force-push。
- 本 push **不**创建 `claude/z3a-cabi-l1`，**不**填写 Z4 APPROVED_*，**不**把 LOCK 当成采数授权。
