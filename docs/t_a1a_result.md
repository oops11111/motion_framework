# T-A1a 真机结果（结题）

> 落盘：2026-10-24 23:05 AWST（Australia/Perth）。数据来源：`artifacts/t_a1a_*`（workbuddy 采数，优先于 ONELINE）。

## 票级状态
- **ticket = FAIL**（闸门 G4-2；**非 INVALID**：method_ok=True ×3，used_2x=False，used_magic_single=False，A1c n_invalid=0）
- A-only×3 = **0/3**
- **归因 = 修正后未稳态**（G4-2）
- corr = CONFIRM_F5_REFUTE_2X；OFFSET_METHOD residual=(d0−d_rest)−τ0，d0=39.5，k=median(residual|unsat)，ref=+2.70
- focus τ0 = {21.8, 3.3, 6.8, 8.3, 33.3}；N_h = 0.41；RMS_gate = #52 IV-rezero + seeded(commanded τ0)（seed+k 仅透明报告，不作闸）

## 闸门 / 授权引用
- 闸门：`docs/t_a1a_gate_LOCKED.md`，STATUS=LOCKED，LOCKED_AT=2026-10-24 22:27:54 AWST
- GATE_BODY_SHA256=0750a8402d4118c98025e71863f79bafd689966c2565fa922d0674e8bcf89d1d（PRE/POST match=True）
- APPROVED_BY=pidan，APPROVED_AT=2026-10-24 22:34:47 AWST（`docs/t_a1a_approved.md`）
- composition.h sha256=a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418（PRE 22:44:06 / POST 23:04:16 AWST match=True；**未修改**）

## 会话表
| sess | wall(raw, AWST) | k_est | A1a | A1b | A1c | A2 | A |
|---|---|---|---|---|---|---|---|
| S1 | 22:50:08 | +3.70 | F | F（−120@220 RMS 1.685） | P（RMS_med 0.221） | P（PS, n=8） | F |
| S2 | 22:57:49 | +2.70 | F | P | P（RMS_med 0.233） | P（PS, n=8） | F |
| S3 | 23:00:15 | +3.45 | F | F（+240 RMS 1.995） | P（RMS_med 0.227） | P（PS, n=8） | F |

## A1a 网格（RMS_gate 中位，n=5/格）
| τ0 | S1 | S2 | S3 | cell |
|---|---|---|---|---|
| 21.8 | 0.191 | 0.202 | 0.191 | PASS×3 |
| 3.3 | 1.220 | 1.251 | 1.246 | FAIL×3 |
| 6.8 | 1.320 | 1.323 | 1.328 | FAIL×3 |
| 8.3 | 1.384 | 1.373 | 1.371 | FAIL×3 |
| 33.3 | 0.131 | 0.123 | 0.127 | PASS×3 |

小 τ0（3.3/6.8/8.3）三会话 RMS_gate≈1.22–1.38 ＞ N_h=0.41，跨会话高度一致 → 系统性，非偶发；21.8/33.3 修正后通过。

## 禁令
- **成功≠采用**；本票 FAIL，更不采用；NO-ADOPT。
- **不重开 S1**；**不重开 R-3**（保持 CLOSED）；不升级 Bv；不 ship；不比 #49；不碰 S2。
- 不改 composition.h；不得在原闸门追加/补测拼凑。

## 产物
- `artifacts/t_a1a_summary.txt`、`artifacts/t_a1a_a1a_grid.txt`
- `artifacts/t_a1a_s{1,2,3}.txt` / `artifacts/t_a1a_s{1,2,3}_raw.txt`
- `artifacts/t_a1a_sha_pre.txt`、`artifacts/t_a1a_sha_post.txt`
- `artifacts/p_a1a.py`（grader）、`artifacts/mf_a1a_run_session.ps1`、`MotionSoftDemo/R3Probe.cs`（bak_pre_a1a）

## ONELINE
T-A1a | gate=0750a8402d4118c98025e71863f79bafd689966c2565fa922d0674e8bcf89d1d @ 2026-10-24 22:27:54 AWST | approved=pidan@2026-10-24 22:34:47 AWST | corr=CONFIRM_F5_REFUTE_2X | S1:A1a=F A1b=F A1c=P A2=P | S2:A1a=F A1b=P A1c=P A2=P | S3:A1a=F A1b=F A1c=P A2=P | A-only×3=0/3 | ticket=FAIL | 归因=修正后未稳态 | 成功≠采用；不重开 S1

## 结题（CLOSED）

用户决策（2026-10-24 23:12:41 AWST Australia/Perth）：**关 T-A1a（挂账保留）**。Peer consensus：Gemini + Notion + DeepSeek 皆同意 **FAIL + 修正后未稳态 + 关票挂账**。

- **状态 = CLOSED**；ticket=FAIL（G4-2）；A-only×3=0/3；归因=修正后未稳态。
- **不重开 S1**；**不重开 R-3**（保持 CLOSED）；不改 `composition.h`（sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）。
- 详细结题文件：[t_a1a_closed.md](./t_a1a_closed.md)。挂账见下（亦载于 closed 文件）；**新票另开，不复活本票**。

### 挂账（保留，供未来票拾取）

- **DeepSeek 补洞条件**（新 LOCK 须同时满足，方可再采）：
  1. 小 τ0 残差机制（对 3.3/6.8/8.3 系统性 FAIL 的机制说明）
  2. 具体修正（写死口径，非条件分支）
  3. ≥3 会话三格过，且 21.8/33.3 不回退
  4. 不改 `composition.h`
- **Notion**：验残差–τ0 相关须**另开独立票**（不挂回本票、不重开 T-A1a）。
