# 对 DeepSeek M1b spec 的评审（Notion 草稿）

> 来源：Notion AI · Opus 5.5 · 2026-10-20（Asia/Taipei）  
> 输入：`docs/_notion_prompt_deepseek_review_full.txt`（含事实包）  
> 角色：代 Claude 写草稿；Claude Free 约 14:40 台北恢复后定稿。  
> 说明：下文为 Copy response；已还原假链接文件名。

> **更新（2026-10-20 Asia/Taipei ≈14:55）**：Claude Free 已定稿 FINAL。四点全部 Agree；详见 [`docs/claude_m1b_deepseek_review.md`](./claude_m1b_deepseek_review.md)（FINAL）。配套规格为 `docs/claude_m1b_spec.md` **修订 4**。本 Notion 草稿保留不删，仅作历史对照；冲突处以 FINAL 为准。


---

# 对 DeepSeek M1b spec 的评审（Notion 草稿）

## 1. Period / DampingRatio 映射

- **结论**：已结案
- **依据**：deepseek_m1b_spec §1.2 已定案 Period=response（无阻尼），DampingRatio=ζ。真机测得峰峰间隔约 0.511 s（response=0.32、ζ=0.78），据此否决 Period=response/√(1−ζ²)。Claude PARTIAL 验算得 Td≈0.5114 s（T0=0.32、ζ=0.78），与真机结果一致。这说明 H1=T0 成立，H2（阻尼周期）被否决。H3=T0/(2π) 没有对应证据，材料不足以单独论证，但 S1 定案后它已无保留必要。§4 规定轨迹形状门槛为 RMS≤1%·行程，弹簧扫参后最低约为 A 8% / E 6%，没有一组达到 ≤1%。§4.1/§5.3 已结案：这是 NaturalMotion 与 spring.h 的**固有差异**，产品仍用 S1，**§4 形状门槛不放宽**。
- **建议**：claude_m1b_spec 里「H1/H2/H3 由实测二选一」应改为「H1 已定案（与当前代码一致）」，并注明 H2/H3 已否决或作废。形状差异按固有差异记录，不再为了达标去调参。
- **是否还要真机验证**：不要

## 2. InitialValue / InitialVelocity 打断种子

- **结论**：同意（采用 DeepSeek 的显式种子；Claude 的混合 B 不应再作为推荐方案）
- **依据**：DeepSeek §2 规定，弹簧被打断时必须显式设置 InitialValue/InitialVelocity，种子取自影子状态或 spring.h::At。InitialVelocity 默认为 0，不设置就不会继承速度。§5.2 规定 InitialValue 为 null 时的行为只做记录、不作依赖。Claude 推荐的混合 B 是「不设 InitialValue + InitialVelocity 取引擎速度」，这正好依赖了 DeepSeek 明确不作依赖的 null 行为。另外 Claude 自己实测 TryGetScalar 恒为静态值，那么「引擎速度」从哪里读取，材料里没有说明。后备方案 A（引擎播种+前瞻）同样依赖引擎侧状态，可读性问题相同，材料不足以评估。
- **建议**：实现统一走显式种子（影子 / spring.h::At），同时设置 InitialValue 和 InitialVelocity。B 和 A 降级为备选记录。
- **是否还要真机验证**：部分还要（打断时的连续性需要按 Claude 的判据用截帧确认；材料里没有打断场景的量化结果）

## 3. TryStartAnimation 与 StartAnimation+TryGet

- **结论**：同意
- **依据**：DeepSeek §1.3 和审校注指出：不存在 TryStartAnimation。KeyFrame 动画应使用 CreateAnimationController + 三参 StartAnimation，或在 Start 之后用 TryGet 拿到非 null 的控制器。NaturalMotion/弹簧动画的 TryGetAnimationController **恒为 null**，因此禁止依赖控制器做 Progress/PlaybackRate。Claude 实测 TryGetScalar 恒为静态值，结论方向一致：弹簧动画无法从引擎侧读回状态。
- **建议**：代码和文档里删掉 TryStartAnimation 的写法。弹簧路径不再获取控制器，进度和速度一律以影子 / spring.h 为准。KeyFrame 路径保留控制器用法。
- **是否还要真机验证**：不要

## 4. ζ>1 是否 lower

- **结论**：部分同意
- **依据**：审校注①：DeepSeek 建议 v1 对 ζ>1 暂不 lower，但现有 composition.h 规划器只要 caps.springNaturalMotion 为真就会 lower，规格建议和现网行为不一致。§5.2 中「ζ>1 会被 DampingRatio 接受还是截断」仍然待验证。三种方案的取舍如下：直通依赖这个未验证的行为，不可取；软件路径（spring.h）在行为上可控；capability 门控需要在现有条件之外再加 ζ 判断。
- **建议**：同意 v1 对 ζ>1 不 lower，但必须同步修改 composition.h：lower 条件改为「caps.springNaturalMotion 且 ζ≤1」，ζ>1 走软件路径。这样规格和现网行为才一致。
- **是否还要真机验证**：要（确认 DampingRatio 对 ζ>1 是接受还是截断；验证结果出来前保持不 lower）

## 与 Claude PARTIAL 的衔接

- **现状**：claude_m1b_deepseek_review.md 目前只有验算，没有书面的四点结论。本篇是 Notion 草稿，Claude Free 预计台北时间 14:40 左右恢复后接手定稿。
- **可直接引用的验算**：Td≈0.5114 s（T0=0.32、ζ=0.78）；first peak 约 255.7 ms；overshoot≈4.78 px（1.99% travel）。还有若干闭式 RMS 对照数字（例如对照 T0=0.1512、z=0.78 时为 9.89%），只作对照引用，不外推新结论。
- **请 Claude 定稿时处理**：
    1. 把 claude_m1b_spec 的 H1/H2/H3 段改为「H1 已定案」，并同步写明固有差异已结案、§4 门槛不放宽。
    2. 重新评估打断方案的推荐：混合 B 依赖 InitialValue 为 null 时的行为，又依赖读取引擎速度，与 DeepSeek §2、§5.2 冲突。建议改为显式种子。
    3. 第 3 点确认没有分歧，补一句书面结论即可。
    4. 第 4 点需要在 Claude 侧标注「composition.h 现网会 lower ζ>1」的风险，并跟进 §5.2 真机结果。
- **材料不足**：打断连续性的截帧结果、ζ>1 的真机表现、H3 的独立证据，这三项现有材料里都没有，定稿时不要补写数值。
