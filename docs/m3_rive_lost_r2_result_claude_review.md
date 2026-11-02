# M3-RIVE-LOST r2 结果复核（Claude）

> 权威结题：`docs/m3_rive_lost_r2_result.md`（notion ai，RIVE-LOST-FAIL，非 METRICS_PASS，未采用）。本文件是独立复核，不改权威结题。
> 依据：`m3/results_lost_r2/`（提交 `382683c`）、`docs/m3_rive_lost_gate_r2_LOCKED.md`、`docs/m3_rive_lost_r2_approved.md`、代码 `05f7576`、钉住的 Rive 源码（`2a043c31`）。

## 结论
- **同意 RIVE-LOST-FAIL、未采用。** 三个会话都在第一次注入后被 Rive `abort()`（`rive_abort=1`），没有一次恢复成功。按 K7 裁决，这是 FAIL，不是 INVALID。
- **注入这次确实生效了**（r1 的问题已解决）：自检 OK（`pnputil` 退出码 0，3.3 s 后显卡状态回到 OK）；每次会话注入后探针都检测到了丢失（`inject1.detected=1`）。
- **runner 标注修正生效**：s1、s2 是 `exited_on_own=False`（应用在 abort 之后、runner 结束它之前还没完全退出），s3 是 `True`；三者的 verdict 都来自 DLL 追加的 `FAIL_ABORT`，没有误标。
- 基线段 seg0 三个会话都正常（589 帧左右，CPU ≈ 0.085·T，GPU 0.016·T，0 失败），与 r1、DPI 一致。

## 需要更正的事实（不影响 FAIL）
1. **闸门是 r2 LOCKED，不是“PROBE”。** 权威结题对照的 `docs/m3_rive_lost_probe_gate_LOCKED.md`（`7afbcd98…6b54`）**不在仓库里**，是协调侧本地副本里另起的文件。本次运行对应的是 `docs/m3_rive_lost_gate_r2_LOCKED.md`，GATE_BODY_SHA256=`dc0770c1f6a6819a5f6cffeb6bea2ef828b93045ec8e75f39910ee9537d29d10`，在 `382683c` 上复算 **MATCH**。
2. **采数有授权**：`docs/m3_rive_lost_r2_approved.md`（pidan @ 2026-10-29 18:15:58 AWST），早于结果提交。
3. **r1 的标签**已按 pidan 的 L5 裁决更正为 INVALID（附录在 `docs/m3_rive_lost_result.md` 末尾）；r2 结题仍写 r1 = INCONCLUSIVE，是同一个本地副本过旧的问题。
4. 协调提交 `a67b142` 又一次用旧副本覆盖了 `docs/m2_mainline_brief.md`，已恢复。

## abort 发生在哪里（分析，未在真机证实）
- 探针先看到 `inject1.detected=1`，说明渲染线程已经进入“丢失”状态。进入这个状态后，渲染循环**不再调用 Rive 画帧**，只做 `RecoverDevice`。所以 abort 几乎一定发生在恢复过程中，而不是 K7 担心的“画到一半设备被移除”。
- 钉住版本的 Rive D3D11 后端里，43 处 `VERIFY_OK`（失败就 `abort()`）**全部在创建资源的路径上**（建上下文、建渲染目标、导入 .riv 时建纹理、建缓冲环），析构里没有。所以 abort 应该发生在“在新设备上建 Rive 上下文 / 导入文件 / 建渲染目标”这一步。
- 模拟冒烟里同一条恢复路径成功（`recover_ok=1`，约 210–221 ms），区别只在于真实注入时**显卡驱动正在重启**。最可能的情况：检测到丢失后立刻（第一次尝试在检测的同一帧里）新建设备；此时驱动还在重启，新设备要么建在即将被移除的旧驱动实例上，要么建在系统临时顶上的“Microsoft 基本显示”适配器上；Rive 在它上面创建资源失败，`VERIFY_OK` 直接 `abort()`。
- 数据里缺两样能直接确认的东西：Rive 的 `VERIFY_OK` 在 abort 前会把出错的源码行和 HRESULT 写到 stderr，但应用的 stderr 没有被保存；也没有记录 abort 时恢复进行到哪一步。

## 下一步
见 `docs/m3_rive_lost_recover_gate_DRAFT.md`。
