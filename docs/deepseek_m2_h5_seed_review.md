# DeepSeek · M2-H5-SEED 判据复核 + 调用点清单

- 时间：2026-10-25（Australia/Perth, UTC+8）
- Harness：session-`1c0126bb-5d95-48f4-9e22-c431abda649d`；model=deepseek-v4-pro；reasoningEffort=max
- 状态：**文档 only**；不授权采数；不改 DRAFT/LOCKED 闸门本体；不改 composition.h
- 原始回复：`/workspace/tmp_h5/dsh_m2_h5_seed_reply.md`（含推理草稿，正式稿以下方正文为准）

---

# M2-H5-SEED 判据复核 + 调用点清单

## 元信息

| 项 | 内容 |
|---|---|
| 票 | M2-H5-SEED 判据复核 + 调用点清单（文档 only；DRAFT 闸门未 LOCK；本交付不授权采数） |
| 依据文件 | docs/m2_h5_seed_gate_DRAFT.md（闸门 DRAFT，STATUS=DRAFT NOT LOCKED）；docs/m2_01_result.md（M2-01 结题，G3 PRIMARY=H5 Seed 源）；artifacts/m2_01_g3_h1h5_audit.txt（G3 审计，#37 四臂实测）；artifacts/win_r2_p4_b2vel.txt（#37 解读摘要，证据源）；仓库静态扫描（本交付 E 节调用点清单，本机 sha 已核） |
| composition.h 当前 sha256 | `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418` |

硬禁重申（本次及后续任何步骤）：

1. 不改 composition.h；sha256 必须保持 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（PRE=POST，本机已核；任何改动即 INVALID）。
2. 不重开 S1 / R-3；不重开 R-2。
3. 不解冻、不升级 Bv。
4. 成功 ≠ 采用。
5. 本票只改 seed 路径（demo / harness / 控制器），不对齐 compositor 弹簧数学到 spring.h。
6. 未授权采数禁止；本交付（文档 only）不授权任何采数，不得因本文档存在而开始采数。

## 块 1 — 判据复核（对照 DRAFT G2）

### 总评：需补洞

DRAFT G2 的判据方向正确、主体成立：以会话中位速度比为核心，窗口 [0.8,1.2] 且无 ≥2.0× 会话，直接锚定 #37 实测的 2.600 与 0.796 两个已知极值，能同时排除「影子过加速」与「欠 seed / 不设 seed」两类错误。但当前 DRAFT 文本中 n、会话独立性、INVALID 边界均为「LOCK 写死」占位，分母退化（v_screen≈0）无处理规则，「无会话 ≥2.0×」的统计口径有歧义，0.8 下限与实测最优 0.796 存在边界摩擦。按现状不可执行，必须在 LOCK 前补齐缺口；补洞方案见下方第 2) 条写死草案。

### 1) PASS 定义是否够硬

PASS 定义原文：「3/3 会话中位 |v_seed/v_screen| ∈ [0.8,1.2] 且无会话 ≥2.0×；相对 #37 的 2.600 已消除」。

**够硬的部分：**

- 会话中位：对单次打断的异常值稳健，比均值更适合打断探针。
- [0.8,1.2] 窗口：下限 0.8 要求 seed 真实匹配屏上速度，仅「不超调」不达标——zero 臂 ratio=0.000、unset 臂 ratio=nan 均被窗口排除，判据因此同时防住「过 seed」与「欠 seed」。
- 3/3：全部会话通过才 PASS，禁止挑会话。
- ≥2.0× 上限与 #37 锚定：shadow 臂 med_vSet=620.0 / med_v_pre=238.5 → ratio=2.600 是已知病态，2.0× 上限将其与正常区隔开；判据与已定位失败模式一一对应。

**缺口：**

- 缺口 1（n 未写死）：DRAFT 仅「建议≥12 次打断或沿用 #37 臂对照结构」。「样本足」无数字则 FAIL 判定无据，也无法识别「补测拼凑」。
- 缺口 2（会话独立性无定义）：同一进程重复跑 3 轮算 1 个还是 3 个会话？臂间是否共享弹簧状态？不写死则 3/3 可由同一次运行拼出，失去「3 个会话」的意义。
- 缺口 3（分母退化无规则）：v_screen≈0 时比值发散或无定义（#37 已出现 unset 臂 ratio=nan、zero 臂 ratio=0.000）。DRAFT 未规定 |v_screen| 过小样本的处理方式，口径可漂移。
- 缺口 4（「无会话 ≥2.0×」口径歧义）：若指会话中位，则该条与「中位∈[0.8,1.2]」冗余；若指单样本，约束强度完全不同。必须二选一写死（草案取强口径，见下）。
- 缺口 5（0.8 下限与实测最优的边界摩擦）：wgc med_vSet=183.5 / med_v_pre=230.5 → ratio=0.796，是 #37 四臂中唯一贴近窗口且 J/Δv 最优的臂，却恰好低于 0.8。不写死边界说明，修复臂落在 0.796–0.8 区间时会被机械判 FAIL。
- 缺口 6（v_screen 测量口径未写死）：测量取哪几帧、打断前还是打断后，DRAFT 未写死。滞后补偿按 D 属实现细节本复核不冻结，但测量窗口本身是判据口径的一部分，且必须与 seed 时刻定义一致，否则比值天然含滞后偏差。
- 缺口 7（FAIL 判定依赖未锁定参数）：「样本足」取决于未锁定的 n 与会话数，FAIL 判据因此暂不可判定。

### 2) LOCK 前写死草案（可粘贴）

```text
G2-LOCK-0（seed 源）：主路径 = WGC/屏一致速度 seed；降级 = IV=null+0（LOCK 写死默认/分级）；
  禁止 spring.h 影子闭式 v 作 compositor InitialVelocity。
G2-LOCK-1（样本量）：每会话 n=12 次打断/重定向（沿用 #37 臂对照结构 n=12/臂）；
  有效样本 = seed 与 v_screen 均可测得且 |v_screen| ≥ ε（ε 由 LOCK 按测量噪声写死，本复核不代定数值）。
G2-LOCK-2（会话独立性）：会话 = 一次进程级独立运行、场景完整重建、臂间不共享弹簧状态；
  计入 3/3 的 3 个会话必须来自 3 次不同运行（同一进程重复 3 轮只计 1 个会话），且均针对修复路径。
G2-LOCK-3（臂结构）：至少 {修复臂, IV=null+0 降级臂}；降级臂仅作对照登记，不计入 3/3
  （zero 臂 ratio=0.000、unset 臂 ratio=nan，均落于 [0.8,1.2] 之外，降级路径自身无法满足窗口）。
G2-LOCK-4（指标口径）：逐样本 r_i = |v_seed,i / v_screen,i|，会话中位 med_r；
  v_screen 测量窗口与 seed 时刻定义在 LOCK 时写死，两者必须一致（滞后补偿属实现细节，不冻结但不得造成口径漂移）。
G2-LOCK-5（PASS）：3/3 会话 med_r ∈ [0.8,1.2]，且任一单样本 r_i < 2.0（即无单样本 ≥2.0×，
  相对 #37 shadow 2.600 已消除）；若 LOCK 选择弱口径，则改为「无会话 med_r ≥ 2.0×」（与主判据冗余，仅作防御）。
G2-LOCK-6（FAIL）：有效样本足（会话数 ≥3 且每会话有效 n ≥12）但未达 PASS。
G2-LOCK-7（INVALID，任一即整票 INVALID，不允许补测拼凑）：
  a. composition.h sha256 ≠ a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418；
  b. 未授权采数；
  c. seed 仍来自 spring.h 影子闭式 v（seed 赋值语句仍取 mf_spring_at / spring.h v(t)）；
  d. 口径偏离本 G2-LOCK（改中位为均值、改臂、改窗口、改分母处理规则等）；
  e. 会话数 <3 或任一会话有效 n <12；
  f. FAIL 后追加补测回填拼凑。
G2-LOCK-8（0.8 下限说明）：wgc 臂 ratio=0.796 为 #37 实测最优且低于 0.8；
  LOCK 时二选一并写明：保留 0.8 下限（声明 wgc 类 seed 需小幅校准），或调整下限；不得在执行中变口径。
```

### 3) RMS/位置辅证：保留为辅证观测，PASS 只锁速度比

**明确推荐：速度比作唯一 PASS 判据；RMS/位置与 J/Δv 保留为记录性辅证（观测入库，不设 PASS/FAIL 门槛），不并入本票 PASS。**

理由（对照 #37）：

- 速度比是**必要**的：shadow 臂 med_vSet=620.0 / med_v_pre=238.5 → ratio=2.600，同时 J_vis 中位=5.30、Δv_vis 中位=272、峰值中位=100.3，四项同向最差；wgc 臂 med_vSet=183.5 / med_v_pre=230.5 → ratio=0.796，同时 J_vis 中位=2.75、Δv_vis 中位=64、峰值中位=41.5，四项同向最好。速度比与视觉残余同向变差，说明它抓得住本票病根（seed 速度与屏上速度失配）。
- 速度比**非充分**：wgc 臂 ratio=0.796（≈0.8）已是四臂最优，但 J_vis 中位=2.75、Δv_vis 中位=64 仍未达 J≤0.5 / Δv≤20。若把 J/Δv 也锁进本票 PASS，则 #37 实测最优臂亦不达标，PASS 将依赖滞后补偿等非 seed 因素，判据不再针对「seed 路径修复」这一命题。
- 分层结论：本票 PASS 只锁速度比（判 seed 是否修好）；J/Δv 与 RMS/位置留作辅证，其达标挂账到轨迹质量类后续票（如 WGC 滞后补偿）。同时保留 zero/unset 对照臂：zero 臂 ratio=0.000、unset 臂 ratio=nan 证明 [0.8,1.2] 窗口能区分「不设 seed」与「正确 seed」，辅证价值不减。

## 块 2 — 调用点清单（禁改 composition.h）

### 2.1 仓库内「spring.h / 闭式 v → compositor InitialVelocity」调用点（符号级）

路径 A：`win_softdemo_src/MainPage.xaml.cs`（真机 live 同源常在 MotionSoftDemo/MainPage.xaml.cs；仓库箱内副本在 win_softdemo_src）

| 方法（约行号） | 臂/路径 | 赋值语句级细节 |
|---|---|---|
| CompStartSpring（约 L519+） | 通用启动 | `a.InitialValue=(float)v0; a.InitialVelocity=(float)vel0`（v0/vel0 来自影子状态，注释写明 spring.h） |
| RunAlignProbe | 打断路径 | 打断时用 shadow 作 InitialValue / InitialVelocity |
| RunInterruptSeedProbe | 打断路径 | `MfProbe.mf_spring_at(... out seedPos, out seedVel)` → `a2.InitialVelocity=(float)seedVel` |
| RunRateFoldProbe | 通用 | `a.InitialVelocity=(float)ivel`（ivel 自 mf_spring_at） |
| RunR2P11Probe | 臂 Bv | `r.InitialVelocity=(float)shV`；臂 A 类 → shX+shV（mf_spring_at at tS） |
| RunR2P10Probe | 臂 A/B | 同模式，shV |
| RunR2P9Probe | 臂 A/B | 同模式，shV |
| RunR2P8Probe | 臂名以 A 开头 | InitialValue=shX, InitialVelocity=shV |
| RunR2P7Probe | 臂 A0 / Am20 | shX+shV |
| RunR2P2Probe | 臂 A | shX+shV |
| **RunR2P4Probe（派题 #37）** | 臂 shadow | `r.InitialVelocity=(float)shV; vSet=shV`；arms={base, shadow, zero, unset, wgc}；证据源 |
| RunR2PostProbe | interrupt 路径 | `a2.InitialVelocity=(float)seedVel` |
| RunSuite 打断段 | shadow seeded retarget | 注释「shadow seeded」 |

路径 B：`MotionSoftDemo/R3Probe.cs`（及 win_softdemo_src/R3Probe.cs 副本）

| 方法 / Trial | 臂/路径 | 赋值语句级细节 |
|---|---|---|
| RunR3P12Probe / Trial | 臂 Bv / Aex | `a.InitialVelocity=(float)shV`（mf_spring_at at tSeed）；Aex 另设 InitialValue=shX |
| RunR3E3Probe | 臂 Bv | InitialValue unset + InitialVelocity=shV |
| RunR350Probe | 臂 Bv | 同上；另有 d1 overlay `InitialVelocity=(float)ivel`（ivel 自 mf_spring_at，ε̂ 扫参） |

备注：

- 若干 `mf_spring_at` 标注 `// log only; do NOT seed`——这些不是 seed 调用点，清单中标记「仅日志」，不属于修复面。
- 桥接：`MfProbe.mf_spring_at` ← mf_probe.cpp / fm_capi 导出，内部调 spring.h 闭式。改 seed 策略时改的是**调用方赋值**，不是 spring.h 本身。

### 2.2 控制器层（本票可触及「seed 路径」语义，须谨慎）

- `code/controller.h`：`Controller::Retarget` 从当前 owner 的 value/velocity 交接（软件路径 spring 状态）。这是 AppThread/软件控制器语义，**不是**直接写 WinRT `SpringScalarNaturalMotionAnimation.InitialVelocity`。若本票要把「禁止影子 IV」固化到控制器 API 契约，可动文档 + 控制器契约；但**不得**借机改 composition.h。

### 2.3 禁止本票改动（只登记）

- `code/composition.h`：`PlanOverride`（约 L304–313）`sp.At(elapsed)` → `pp.spring.fromVelocity = st.velocity`；`PlanGraph` 弹簧段（约 L357–368）同类。这是 lowering 到 CompositorSpring 的产品路径，本票禁止改；对齐 compositor 数学到 spring.h 或改该 seed 出票外。
- `code/spring.h`：纯数学，不是「喂给 compositor IV」的调用点；勿改，除非另票。
- 冻结/禁令：不重开 S1/R-3；不解冻 Bv；成功≠采用。

### 2.4 本票可能动 vs 禁止动

| 层 | 文件 | 本票 |
|---|---|---|
| demo / harness（可能动） | MotionSoftDemo/R3Probe.cs、win_softdemo_src/R3Probe.cs、win_softdemo_src/MainPage.xaml.cs（及真机 MotionSoftDemo/MainPage.xaml.cs）、相关 harness 脚本/探针接线 | 允许改（仅 seed 赋值路径） |
| 控制器（有条件） | code/controller.h | 仅限 seed 契约/文档级，且不触及 composition.h |
| 文档（可能动） | 本票相关文档 | 允许 |
| 禁止动 | code/composition.h（sha 锁死）、LOCKED 闸门/结果文件本体、DRAFT 闸门本体（本交付不改 DRAFT 文件）、spring.h 数学对齐类改动、采数 | 禁止 |

### 2.5 sha 保持与挂账重申

- composition.h sha256 必须保持给定值：`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（本机已核，PRE=POST）。
- composition.h 内 `PlanOverride` / `PlanGraph` 用 spring.h::At 填 CompositorSpring.fromVelocity 的路径**本票禁止改**，只登记为挂账 / 出票外。

## 第三段（可选）— M2-02 API 草案增量建议

给 `docs/deepseek_m2_02_api_draft.md` 的增量一句（可直接粘贴）：

> H5 未修前 C# 投影勿接影子 IV。

放置位置建议：**§3.4**。若 §3.4 是「合成器路径接续假设/未验证项」所在节，追加至该节末尾；若该节专述接续假设而无风险清单，则放 §3.5。附注理由（供落盘参考，不必进正文）：H5 PRIMARY（spring.h 闭式 v 作 compositor InitialVelocity → 过加速）修复前，C# 投影若接入影子 IV，会把同一病根带进投影层。
