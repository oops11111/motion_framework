# workbuddy 派单：M2-H5-SEED（自包含）

派单时间：2026-10-25 00:18:34 AWST
执行环境：Windows DESKTOP，经 CodeBuddy 执行。

## 授权
- 闸门：`docs/m2_h5_seed_gate_LOCKED.md`（STATUS=LOCKED，LOCKED_AT=2026-10-25 00:17:37 AWST，GATE_BODY_SHA256=28a60c37c08fff4d28136160797182a530d2a15c8d4559f04a4fc23d7a3ea4b0）
- 批准：`docs/m2_h5_seed_approved.md`（APPROVED_BY=pidan，APPROVED_AT=2026-10-25 00:18:34 AWST）
- **严格逐字遵循 LOCKED 闸门**；不得修改闸门 body / hash；不得在本闸门下追加补测（违规 → INVALID，须新 LOCK）。

## 范围（仅 M2-H5-SEED，依 LOCKED G0–G4）
1. **默认路径 = 分级 A→B（G1）**：主路径 A = WGC / 屏一致速度 seed（DIP/s，QPC 时基）；运行时回退 B = `IV=null + InitialVelocity=0`（单次取 WGC 速度失败或会话有效样本不足时）。**票级 PASS 只评 A 路径会话**；全程落 B 的会话不计 PASS，仅对照登记。禁止用 spring.h 影子闭式 v（`mf_spring_at` / spring.h `v(t)`）作 compositor InitialVelocity。
2. **探针（G2）**：弹簧运动中途打断 / 反向重定向（沿用 #37 臂对照结构）；每会话有效打断 **n=12**；有效样本须 `|v_screen| ≥ ε = 5.0 DIP/s`；**3 个进程级独立会话**（3 次不同进程运行、场景完整重建、臂间不共享弹簧状态）。
3. **臂**：{修复臂 A, 降级臂 B(IV=null+0)}；B 仅对照登记，**不计入 3/3**。
4. **指标**：`r_i = |v_seed / v_screen|`，会话中位 `med_r`；v_screen = 打断前末两帧 WGC `Translation.X`，`(x2-x1)/(t2-t1)`，QPC→秒，DIP/s，与 seed 赋值对齐同一打断瞬时。窗口下限保留 0.8（#37 wgc 臂 0.796 须小幅校准进窗），执行中不得改窗口。
5. **PASS**：3/3 会话 `med_r ∈ [0.8, 1.2]` **且** 每个单样本 `r_i < 2.0`（无单样本 ≥2.0×）且相对 #37 shadow 臂 2.600 已消除。**PASS 只锁速度比**；J_vis / Δv_vis / RMS / 位置仅辅证入库。
6. **FAIL**：样本足（≥3 会话且每会话有效 n≥12）但未达 PASS。
7. **INVALID**（任一即整票 INVALID）：a. composition.h sha 不符；b. 未授权采数；c. seed 仍来自 spring.h 影子闭式 v；d. 口径偏离（中位改均值、改臂、改窗口、改分母/ε、改 ≥2.0× 口径等）；e. 会话 <3 或任一计入会话有效 n<12；f. FAIL 后补测回填拼凑。
8. **可改范围**：只改 demo/harness 中 seed 赋值 —— `MotionSoftDemo` / `win_softdemo_src` 的 `MainPage.xaml.cs`、`R3Probe.cs` 及相关 harness。**绝不改** composition.h 内 PlanOverride / PlanGraph（只登记挂账）；**绝不**把 compositor 弹簧数学对齐到 spring.h。

## 硬禁令（G0/G3）
- 不改 `composition.h`；开工前（PRE）与结题时（POST）各校验一次 sha256 = `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`，不符即停并报 INVALID。
- 不重开 S1 / R-3 / R-2；不解冻 / 不升级 B_v。
- 成功 ≠ 采用；旁路挂账不挡本票，本票也不清旁路。

## 产出
- 全部写入 `artifacts/m2_h5_seed_*`（原始样本、逐打断 r_i、会话汇总、A/B 路径标记、composition.h PRE/POST hash 记录、日志）。
- 结题文件：`docs/m2_h5_seed_result.md`，含 G2 判定、证据路径、G3 禁令重申，以及 ONELINE（G4 模板）：
  `M2-H5-SEED | gate=28a60c37c08fff4d28136160797182a530d2a15c8d4559f04a4fc23d7a3ea4b0 @ 2026-10-25 00:17:37 AWST | approved=pidan@2026-10-25 00:18:34 AWST | ratio=<P/F/I> | ticket=<PASS/FAIL/INVALID> | 成功≠采用；不改composition.h；分级A→B；强口径无单样本≥2.0×`

## 参考
- `docs/gemini_m2_h5_seed_design.md`
- `docs/deepseek_m2_h5_seed_review.md`
- `docs/notion_m2_h5_seed_gate_revise.md`
- `docs/m2_01_result.md`（H5 主因）
