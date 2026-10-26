# Gemini · M2-H5-SEED 方案对比（LOCK 前）

> 来源：Gemini 网页版 · 日期：2026-10-25（Australia/Perth）  
> 原始拷贝：`/workspace/gemini_m2_h5_seed_reply.md`  
> 依据：`docs/m2_h5_seed_gate_DRAFT.md`、`docs/m2_01_result.md`、`docs/notion_m2_01_pack.md`  
> 说明：下文为 Gemini 完整回复；数字仅引用既有结题（#37 ratio=2.600）。本文件**不授权采数**，不改闸门。

# M2-H5-SEED 方案对比（LOCK 前必填）

前序：M2-01 PASS；主因 H5 Seed 源（#37 shadow/screen ratio = 2.600）；修复不碰 `composition.h`。闸门 DRAFT 未 LOCK，本票不授权采数。

## 1) 方案 A：WGC/屏一致速度 seed

* **怎么取 v**：通过 WGC 末两帧位移差与时间差计算瞬时速度（$\Delta x / \Delta t$，单位 DIP，时间基使用 QPC 高精度时间戳）。
* **时基与单位如何写死**：统一采用 DIP/s 作为速度量纲，时间基准严格锁定为 QPC（QueryPerformanceCounter）计数器换算的秒数。
* **与 compositor InitialVelocity 的接线点**：仅在 demo / harness / 控制器层进行输入映射与接线，**绝对禁止**修改底层 `composition.h`。
* **相对 #37 影子臂预期**：比值应落近 1.0 而非 2.600。

## 2) 方案 B：IV=null + InitialVelocity=0 降级

* **行为预期**：强制初始速度为零，弹簧动画完全由静止状态启动，消除了外部速度源引入的估算偏差。
* **打断/重定向观感代价**：在快速交互、连续打断或重定向场景下，由于初始速度归零，会产生轻微的瞬时速度清零跳变感（可结合已知 R-2 跳变风险评估，不引入新编数字）。

## 3) 推荐默认（供 LOCK 写死）

* **推荐选择**：分级方案（先 A，失败或样本不足回退 B）。
* **一句话理由**：方案 A 能够从根本上对齐屏幕与影子追踪速度（回归 1.0），而方案 B 作为兜底可有效消除高频跳变风险。

## 4) 探针建议（供 G2 写死）

* **打断/重定向场景描述**：在弹簧动画运动中途触发连续打断与反向重定向。
* **会话与对照建议**：会话数 $\ge 3$；每会话打断次数对齐历史 #37 的臂结构特征。
* **判据**：$\vert{}v_{\text{seed}} / v_{\text{screen}}\vert{}$ 中位 $\in [0.8, 1.2]$ 且无会话 $\ge 2.0\times$。该判据完全可执行；口径定义以 QPC 同步采样的瞬时速度比值为准。

## 5) 明确声明

* 本票严禁将 compositor 弹簧数学对齐到 `spring.h`。
* 若最终修复路径不可避免需要改动 `composition.h`，则本票仅记录根因与测试结论，修复实施必须另开新票。

* 严禁解冻 Bv / 严禁重开 S1/R-3。
* 成功 $\neq$ 采用，闸门仍为 DRAFT（本票绝对不授权采数）。
* 严禁修改 `composition.h`。
