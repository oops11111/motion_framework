# Gemini · M2-02 设计补洞（LOCK 前）

> 来源：Gemini 网页版 · 日期：2026-10-25（Australia/Perth）  
> 原始拷贝：`/workspace/gemini_m2_02_reply.md`  
> 依据：`docs/m2_02_gate_DRAFT.md`、`docs/deepseek_m2_02_api_draft.md`、`docs/notion_m2_h5_seed_pack.md`  
> 说明：下文为 Gemini 完整回复；勿发明数字/API。本文件**不授权采数**，不改闸门。

# M2-02 设计补洞（LOCK 前 DRAFT 文档）

---

### 1) 预览宿主 v0 验收一步

* **最小 smoke 步骤（≤6步）**：
1. 启动预览宿主 v0 可执行程序。
2. 导入合规的 `motion.json` 测试资产文件。
3. 在资产列表中选择至少一条目标 token 动画（例如 `card-expand-smooth`）。
4. 点击加载，确认 UI 成功绑定并初始化对应视觉元素。
5. 点击播放按钮触发预览。
6. 观察宿主窗口内的动画呈现。


* **PASS/FAIL 判据（一句）**：预览宿主成功加载指定 token 动画并在点击播放后完整呈现、无视觉崩溃即为 PASS，否则为 FAIL。

---

### 2) C# 打断 smoke 场景

* **最小场景步骤（≤8步）**：
1. 初始化托管 `MotionSystem` 并绑定测试 `UIElement`。
2. 调用 `Play()` 启动目标属性动画。
3. 在动画运行中途触发 `RetargetTo()` 传入新目标值或调用 `Cancel()`。
4. 观察接管瞬间的运行表现。


* **连续性观察点**：观察位置、速度的连续性——确认接管瞬间从当前实时状态平滑过渡，无硬切、无瞬时位移跳变或异常速度清零感。
* **PASS/FAIL 判据（一句）**：C# 接口成功执行 Play/Cancel/Retarget 且连续性从当前实时状态平滑接管、无可见跳变即为 PASS，否则为 FAIL。

---

### 3) 对 DRAFT G1/G2 的补洞建议

* **LOCK 建议句（可直接粘贴进闸门 G1/G2）**：
> “G1 验收须通过预览宿主 v0 的 token 动画加载预览 smoke，且 C# 托管 API 须具备 Play / Cancel / RetargetTo 的可打断重定向能力，确保连续性从当前实时状态接管。”


* **会话/样本量建议**：建议 $\ge 3$ 会话或等价 CI 自动化烟雾测试。

---

### 4) 明确声明

* 严禁修改 `composition.h`（sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）。
* 严禁影子 IV（须 WGC/屏一致 seed 或 IV=null+0）。
* 严禁解冻 Bv / 严禁重开 S1/R-3/R-2。
* 成功 ≠ 采用；闸门仍为 DRAFT（本票绝对不授权采数）。