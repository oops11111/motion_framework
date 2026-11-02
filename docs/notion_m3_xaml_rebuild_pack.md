# M3-XAML-REBUILD 结题中文包（notion_m3_xaml_rebuild_pack.md）

> DECISION_AT=2026-11-01 17:55:00 Asia/Shanghai  
> 权威结题：`docs/m3_xaml_rebuild_result.md`  
> 闸门：`docs/m3_xaml_rebuild_gate_LOCKED.md`；GATE_BODY_SHA256=`3ea74c371ae983cebe480e1b4dea99e61aec92cfbe9646fb87b91b7a1d456fdc`（**MATCH**）  
> APPROVED：`docs/m3_xaml_rebuild_approved.md` — pidan @ 2026-11-01 16:26:53 Asia/Shanghai  
> 计划：`docs/m3_plan_DRAFT.md` §7（WP1 设备丢失 / XAML 宿主路径 · REBUILD 诊断）  
> 前序：XAML-LOST CLOSED `XAML-LOST-PASS`（**勿改**）；r1/r2/RECOVER/DPI/PROBE（**勿改**）；FOLLOW LOCKED SendInput `285e39dc…aab3` **已 APPROVED 且已派**（**不阻拦**）  
> 分支：tip_read=`24ccce6c3e8d618c704c27c8e675ae039c26599c`；commit=`d00ffd0f5be2f11f7b5b4e30f287c7d0b16b9e9f`；tip_after_push=同 @ **`claude/m3-xaml-rebuild`**  
> composition.h PRE=POST CRLF=`33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97` LF=`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` edited=False  

## 结论

- **票级 = CLOSED**；标签 **`XAML-REBUILD-PASS`**；协调对照 **DIAGNOSIS_PASS（有条件）**；R4 出口=**KNOWN_LIMIT**。
- **是否采用 = 有条件采用验证结论**（成功 ≠ 采用；**不合 main**）。
- **宿主补救 vs 已知限制**：**已知限制优先**——IJI 上 W_post 亦 TEXT-LOST，**「检测到设备丢失后重建窗口」不充分**；建议宿主/试点文档写明 **重启应用**；最小复现对外（Q3）由 pidan 决定。
- **Success≠adopt**；**PERF-CI HOLD**；composition.h **未触**。

## 口径（PASS 有条件；对照 R4）

| 选项 | 决断 | 理由 |
|---|---|---|
| XAML-REBUILD-FAIL | **否** | 非「采数失败」；A/B/C 均 RECORDED inject_ok=1 |
| XAML-REBUILD-INCONCLUSIVE | **否（票级）** | R1/R2 已可裁决；仅 C 的 W_* 截图挂账 |
| **XAML-REBUILD-PASS** | **是（有条件）** | G1 已答：进程级 + 打包同向；R4→KNOWN_LIMIT |
| 采用宿主重建窗口方案 | **否** | W_post TEXT-LOST |
| 采用验证结论（KNOWN_LIMIT） | **有条件是** | 不合 main；Success≠adopt |
| 翻盘 XAML-LOST / FOLLOW | **否** | 硬禁；FOLLOW 已派不阻拦 |

### 有条件采用 vs 不采用（写清）

| 采用什么 | 不采用什么 |
|---|---|
| 验证结论：IJI 类环境丢失为**进程级**；打包同向；W_post 不能画字 → **KNOWN_LIMIT／重启应用** | 整 tip / 分支 **默认可合入 main** |
| 实验分支证据（commit `d00ffd0f…` + `m3/results_xaml_rebuild/` 121 files） | 把「重建窗口」写成 **已验证充分补救** 并开工合入 |
| 机差旁证：K93 主窗未丢字（挂账） | 用 C 翻盘 A/B；或本 pack **授权新采数** |
| — | PERF-CI 解冻 / csproj 回灌主线 / 阻拦 FOLLOW workbuddy |

## TEXT-OK / TEXT-LOST 裁决（重点）

| 组 | 环境 | W_pre | W_post | 进程 vs 窗口 | 打包 |
|---|---|---|---|---|---|
| **A** | IJI unpackaged×3 | base **OK** → 后 **LOST** | **LOST** | **进程级** | N/A（unpackaged） |
| **B** | IJI packaged×1 | base **OK** → 后 **LOST** | **LOST** | **进程级** | **同向** packaged=1 |
| **C** | K93 packaged×1 | **INCONCLUSIVE**（截图像） | **INCONCLUSIVE** | 主窗全程 **TEXT-OK**（旁证） | packaged=1；W_* 不可靠 |

## 授权核验

| 项 | 值 |
|---|---|
| LOCKED | `m3_xaml_rebuild_gate_LOCKED.md`；hash **MATCH** `3ea74c37…6fdc` |
| approved | pidan 2026-11-01 16:26:53 Asia/Shanghai |
| 本结题采数 | **否** |
| tip_read / commit | `24ccce6c…599c` / `d00ffd0f…9e9f` |

## 事实表

| 项 | 值 |
|---|---|
| TICKET / STATUS | `XAML-REBUILD-PASS` / **CLOSED** |
| R4 | **KNOWN_LIMIT** |
| A/B/C | 皆 **RECORDED**；inject_ok=1；A shots=20×3；B/C shots=20 |
| results | `m3/results_xaml_rebuild/` **121** files |
| composition.h | 未触；edited=False |
| PERF-CI | HOLD |

## 产物清单

- `docs/m3_xaml_rebuild_result.md`
- `docs/notion_m3_xaml_rebuild_pack.md`（本包）
- `docs/notion_m3_xaml_rebuild_next.md`
- `docs/m3_xaml_rebuild_result_brief.md`
- brief：`docs/m2_mainline_brief.md`（追加一行）
- 结果：`m3/results_xaml_rebuild/{a,b,c}/`

## 禁令复述

- 不改 composition.h；不合 main；Success≠adopt；PERF-CI HOLD
- 不翻 CLOSED 旧票；不阻拦 workbuddy；本 pack/next **不授权新采数**
- 不为 R2 修部署（本票 B 已成功，无需）

## 挂账 / 残差

| 项 | 处置 |
|---|---|
| C W_* 截图 | INCONCLUSIVE；可选日后 DPI 校准（≠采数令） |
| B Toast 叠层 | 记录 |
| Q3 报告 | pidan |
| 下一张 | 见 next → **FOLLOW 已派待命** |

## 下一票决断

详见 `docs/notion_m3_xaml_rebuild_next.md`。  
**主推：`M3-RIVE-FOLLOW`（已 APPROVED；SendInput LOCK `285e39dc…aab3`；已派 workbuddy）— 待命/并行；勿重复授权采数；勿阻拦。**

— notion ai（M3-XAML-REBUILD 结题；CLOSED PASS；R4=KNOWN_LIMIT；有条件采用；不合 main）
