# Gemini · M2-Δv 设计补洞（LOCK 前）

> 来源：Gemini 网页版 · 日期：2026-10-25（Australia/Perth）  
> 原始拷贝：`/workspace/gemini_m2_delta_v_reply.md`  
> 依据：`docs/m2_delta_v_gate_DRAFT.md`、`docs/notion_m2_parallel_next.md`、H5/M2-02 产物  
> 说明：下文为 Gemini 完整回复；勿发明数字/API。本文件**不授权采数**，不改闸门；PERF-CI 仍 HOLD。

# M2-Δv 设计补洞（LOCK 前 DRAFT 文档）

### 1) 格表终稿

* **格表定义**（含 t060 / t156 / t300 结构）：
    * **t060 格**：含 **fwd / rev**；`n = 20`；独立进程会话数 $\ge 2$；判定状态：**可判**。
    * **t156 格**：含 **fwd / rev**（针对历史 t156 结构性不可判问题，本票显式规整为：**$\Delta v$-SKIP**，不得计入 $\Delta v$-PASS，禁止私自重开 R-2 或修改估计器逻辑）；`n = 20`；独立进程会话数 $\ge 2$。
    * **t300 格**：含 **fwd / rev / dbl**；`n = 20`；独立进程会话数 $\ge 2$；判定状态：**可判**。
* **虚拟机合规要求**：仅允许 1 台物理机（`DESKTOP-K93N3P8`）合法执行；**VM = INVALID**。

### 2) `t_s` 切换窗定义

* **切换窗规范**：沿用已有 harness 中的 **E2 / E3 类切换窗**。
* **执行口径**：切换时刻 $t_s$ 上的速度差定义为 $\Delta v = \vert{}v_{\text{post}} - v_{\text{pre}}\vert{}$；时间基准和采样窗口严格取自 $t_s$ 邻域内的有效帧。
* **严正声明**：**绝不做纯帧差分冒充** $\Delta v$。

### 3) 绝对阈终稿

* **绝对阈公式**：维持 DRAFT 提案不变，写死为 `\Delta v_{\text{thresh}} = \max(20 \text{ DIP/s}, 0.05 \cdot |v_{\text{ref}}|)`，其中 `v_ref = v_pre(t_s)`。
* **判定原则**：**彻底去掉**相对臂 A / $\le 0.25 \times \text{A}$ 的老口径，绝对禁止用相对比值冒充绝对门。
* **PASS 判定标准（可直接粘贴）**：在所有“可判”格下，样本量 $n \ge 20$，且单次 $\Delta v \le \text{thresh}$ 的命中次数 $\ge 17/20$，同时 $\Delta v$ 中位数 $\le$ 阈值中位数。

### 4) 与 H5 seed 衔接

* **seed 路径声明**：每会话启动时必须显式声明 seed 路径——统一采用 **WGC/屏一致 seed** 或 **IV = null + 0**。
* **硬隔离红线**：严禁任何形式的影子 IV；绝对不得将 `spring.h` 的闭式 $v$ 直接作 compositor 的 `InitialVelocity`。

### 5) 范围声明

* 本票为单机合法验证（指定物理机 `DESKTOP-K93N3P8`）；虚拟机 **VM = INVALID**。
* 本票 **$\neq$ PERF-CI**；M2-PERF-CI 仍处于 **HOLD** 状态（`5b007dd4…282a`）；本票不涉及 M3 真机测试。

### 6) 对 DRAFT G1/G2 的 LOCK 粘贴句

* **G1/G2 粘贴句 1**：“M2-$\Delta v$ 闸门验收采用绝对阈公式 $\Delta v_{\text{thresh}} = \max(20 \text{ DIP/s}, 0.05 \cdot \vert{}v_{\text{ref}}\vert{})$，剔除所有相对臂 A 比值口径，在 t060 与 t300（t156 标定为 $\Delta v$-SKIP）的合格格下要求每格 $n \ge 20$（$\ge 2$ 独立进程会话）且单次 $\Delta v \le \text{thresh}$ 达 $\ge 17/20$。”
* **G1/G2 粘贴句 2**：“所有测试必须严格对齐 H5 seed 产物（WGC/屏一致或 IV = null + 0），单机合法（`DESKTOP-K93N3P8`，VM = INVALID），严禁影子 IV。”

### 7) 明确声明

* 严禁修改 `composition.h`（sha256 `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）。
* 严禁影子 IV。
* 严禁解冻 Bv / 严禁重开 S1/R-3/R-2。
* 成功 ≠ 采用；闸门仍为 DRAFT（本票绝对不授权采数）。
* 单机合法；VM = INVALID；本票 $\neq$ PERF-CI；不开 M3 真机。
