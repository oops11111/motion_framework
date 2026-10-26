> ⚠️ **R-2 已因 #33 重开**（OPEN：seed 来源缺陷，可见跳变约 207 DIP）。本文中 R-2「known-fail / 纯 R-1 衍生 / 20–28 px」等结论已作废，以 [notion_r2_reopen_abc.md](./notion_r2_reopen_abc.md) 为准。

# Gemini R-2 体验评估摘录（意译）

> 来源：Gemini 体验评估 · **2026-10-20**（Asia/Taipei）  
> 性质：协调侧对 Gemini 回复的**意译 / 摘要**，数字不自行编造；技术裁决见 [`notion_r2_review_abc.md`](./notion_r2_review_abc.md)。  
> 约束：不改 `composition.h` / 代码；§4 门槛不放宽；不阻塞 M1b。

---

## 结论

在产品映射 + 全程单一 τ + 显式 seed 条件下，真机打断可见跳变约：

- **|Δpos| ≈ 20–28 px**
- **|Δvel| ≈ 150–180**

R-2 记为 **known-fail**，由 R-1（Composition 映射形状残差）衍生，**不是** seed 逻辑单独缺陷。§4 连续性门槛**不放宽**；**不阻塞 M1b**。对外文档与演示**不得**声称「无缝飞行中改目标 / seamless mid-flight retarget」。

---

## 四条建议摘要

1. **演示卖点**：避免把「飞行中改目标 / mid-flight retarget」当作主卖点镜头；该场景下跳变用户可感知。
2. **优先替代镜头**：优先展示弹簧完整 settle，或「先 Cancel 再重新 Play / Retarget」的清晰重启，而不是途中硬切目标。
3. **若必须演示打断**：改用较软弹簧 + 较大行程，并口头 / 字幕标明「当前为 Composition 映射残差下的已知限制（R-2），非无缝」。
4. **文档口径**：写「支持改目标；连续性受 Composition 映射残差限制（R-2 known-fail，约 20–28 px）」；**不要**写 seamless / 无感打断。详见 [`notion_r2_review_abc.md`](./notion_r2_review_abc.md)、[`usage.md`](./usage.md) §1.7 / §6。

---

## 与工程裁决的对齐

| 项 | 状态 |
|----|------|
| R-2 归因 | R-1 衍生 known-fail |
| §4 门槛 | 不放宽 |
| M1b | 不阻塞；验收报单 τ 指标并标注 known-fail |
| 代码 / composition.h | 本轮不动 |
