# T-A1a 结题（CLOSED）

> CLOSED_AT=2026-10-24 23:12:41 AWST（Australia/Perth）
> 用户决策：关 T-A1a（挂账保留）。NO composition.h 变更。NO 重开 S1 / R-3。

## 状态

- **STATUS = CLOSED**
- **reason** = 票级 FAIL（G4-2）修正后未稳态；A-only×3=0/3
- Peer consensus：Gemini + Notion + DeepSeek 皆同意 **FAIL + 修正后未稳态 + 关票挂账**

## 挂账 ledger（显式子弹；未来票拾取，不复活本票）

- **DeepSeek 补洞四条**（新 LOCK 须同时满足）：
  1. 小 τ0 残差机制（对 3.3/6.8/8.3 系统性 FAIL）
  2. 具体修正（写死口径）
  3. ≥3 会话三格过，且 21.8/33.3 不回退
  4. 不改 `composition.h`
- **Notion 备注**：验残差–τ0 相关须**另开独立票**（不挂回 T-A1a，不重开本票）。

## 禁令

- **成功 ≠ 采用**；本票 FAIL，更不采用；NO-ADOPT。
- **不重开 S1**；**不重开 R-3**（保持 CLOSED）。
- 不升级 Bv；不 ship；不比 #49；不碰 / 不复活 S2。
- **不改 `composition.h`**（sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）。
- 不得在原闸门追加 / 补测拼凑；新工作须新票 + 新 LOCK。

## 闸门 / 授权引用

- 闸门：`docs/t_a1a_gate_LOCKED.md`，STATUS=LOCKED
- LOCKED_AT=2026-10-24 22:27:54 AWST
- GATE_BODY_SHA256=`0750a8402d4118c98025e71863f79bafd689966c2565fa922d0674e8bcf89d1d`（PRE/POST match=True）
- APPROVED_BY=pidan；APPROVED_AT=2026-10-24 22:34:47 AWST（`docs/t_a1a_approved.md`）
- composition.h sha256=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（PRE/POST match=True；**未修改**）
- corr=CONFIRM_F5_REFUTE_2X；OFFSET_METHOD residual=(d0−d_rest)−τ0；d0=39.5；k=median(residual|unsat)；ref=+2.70

## 真机摘要

| 项 | 值 |
| --- | --- |
| A-only×3 | 0/3 |
| ticket | FAIL（G4-2，非 INVALID） |
| 归因 | 修正后未稳态 |
| A1a | 21.8/33.3 PASS×3；3.3/6.8/8.3 FAIL×3（RMS_gate≈1.22–1.38 ＞ N_h=0.41） |
| A1b | 仅 S2 PASS；S1/S3 FAIL |
| A1c / A2 | 3/3 PASS |

详见 [t_a1a_result.md](./t_a1a_result.md)。

## 产物路径

- `docs/t_a1a_result.md`
- `docs/t_a1a_closed.md`（本文件）
- `docs/t_a1a_gate_LOCKED.md`
- `docs/t_a1a_approved.md`
- `docs/notion_a1a_tracker_brief.md`（§11 CLOSED + ledger）
- `docs/r3_commit_staleness.md`（A1a tracker → CLOSED FAIL 挂账；R-3 仍 CLOSED）
- `artifacts/t_a1a_summary.txt`、`artifacts/t_a1a_a1a_grid.txt`
- `artifacts/t_a1a_s{1,2,3}.txt` / `artifacts/t_a1a_s{1,2,3}_raw.txt`
- `artifacts/t_a1a_sha_pre.txt`、`artifacts/t_a1a_sha_post.txt`
- `artifacts/p_a1a.py`、`artifacts/mf_a1a_run_session.ps1`、`MotionSoftDemo/R3Probe.cs`（bak_pre_a1a）

## ONELINE

T-A1a | CLOSED_AT=2026-10-24 23:12:41 AWST | ticket=FAIL G4-2 修正后未稳态 | A-only×3=0/3 | gate=0750a8402d4118c98025e71863f79bafd689966c2565fa922d0674e8bcf89d1d @ 2026-10-24 22:27:54 AWST | approved=pidan@2026-10-24 22:34:47 AWST | composition.h=a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418 未改 | 挂账=DeepSeek补洞四条+Notion独立票 | 不重开 S1/R-3

> **旁路指针（2026-10-24 23:35:47 AWST）**：挂账项停放于 [side_parked_hang_degrade.md](./side_parked_hang_degrade.md)；**不阻塞 M2**。
