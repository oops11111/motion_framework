# DeepSeek 对 T-A1a 门槛 brief 的门控短意见

- 时间：2026-10-24 22:17:50 Australia/Perth (UTC+8)（会话约 22:15:33–22:17:50）
- session：`session-a8c85820-cbd2-401a-a6d1-46785202184f`
- provider / model：`deepseek-account` / `deepseek-v4-pro`（reasoningEffort=max）
- 依据：`docs/notion_a1a_tracker_brief.md`；连贯 `#52` 意见（A1a 跟踪转出）。

---

`docs/notion_a1a_tracker_brief.md` 在本次工作区及本机常见路径均未检索到落盘文件（工作区仅有 `deepseek_a1a_brief_full_prompt.txt` 等提示语副本）。以下意见以题面给出的权威全文为唯一依据，未另采他文、未发明数字。

---

# T-A1a 门槛 brief 门控短意见（连贯于 #52）

**1) 同意该最小充分集。**

- 与 #52 收敛口径逐项一致：**A1a 归因（τ0=21.8 细网格/修正）→ A-only ×3 重测**，闸门 LOCKED、票级判据 3/3；§4.3 的三条选型理由（会话级挂账须以 A 组整体 3/3 关账；只测 A1a 回答不了挂账；A-only ×3 顺带核查修正未破坏 A1b/A1c/A2）正是 #52 的逻辑，且已明引 #52 建议。
- 焦点 = τ0=21.8 + #51 FAIL 子集 [3.3, 6.8, 8.3, 33.3]；N_h=0.41 沿用 #52 A v2、不新测不另设；范围不扩大到其他网格/指标——与 #52 无冲突。
- 步骤1–3（离线复读→LOCK→APPROVED）是闸门程序，不是扩大的采数范围；步骤1中「可选的分析-only 归因」只用现有数据，未越过「不授权采数」边界。同意。
- 附一条与 #52 口径的呼应：票级 PASS 仅解决「A1a 已清」，成功≠采用、不重开 S1/R-3、Bv 不升级、不作 ship 依据——§0/§5/ONELINE 均已写死，与 #52 的「R-3 关但留账，Bv 兜底不洗白」连贯。

**2) 闸门/硬停无致命漏洞，主干闭环成立**（LOCK→APPROVED→派工→票级 3/3→ONELINE；硬停已覆盖：sha 不符、越界采数、采数后改闸、撞界误读、未获 APPROVED/未 LOCKED 派工、以通过结果重开 R-3/S1 或升级 Bv/采用 Sup-adv/作 ship 依据）。但有三处小洞，必须在 LOCK 文本里补写死，否则与「采数后改闸→VOID」相抵：

- (a) **§4.4 判据表只写死了 A1a 单项**；A1b/A1c/A2 的单项 PASS/FAIL/INVALID 未逐项写死，「A-only 会话全部 PASS」因此不可执行。且 N_h=0.41 的来源引文落在「A1c RMS 中位 ≤ 0.41」，易被读成四项统一用 RMS 中位，而 #52 里 A1b 是 early-pair 归因、A1c 是 ε_fit。LOCK 时须逐项写明并标注沿用 #52 哪条指标定义。
- (b) **§4.5 归因三分支未穷举组合**：「未修正时不复现」+「修正后 A-only ×3 < 3/3」会同时命中分支2与分支3。建议写死优先级：修正后 <3/3 时归因一律记「修正后未稳态」，票级 FAIL 兜底。
- (c) **票级 INVALID 后「须重新走 LOCK 和 APPROVED」与「LOCK 后不可改」的关系未说清**。建议写死：重新 LOCK 须作为新一轮（新 LOCK 时间戳/新 body hash），不得在原闸门上追加；「不得补测拼凑」已写明，保持。
- 补强（非漏洞）：硬停只禁「未获 APPROVED 就派工」，未要求留证；建议 APPROVED 的内容与时间戳落盘（记入闸门或结题文件），否则事后无法自证授权存在。

**3) APPROVED 前还缺一条：闸门文件 LOCKED。**

`docs/t_a1a_gate_LOCKED.md` 尚未落盘——本 brief 里 body hash 仍是「LOCK 时计算填入」的占位，即证闸门未 LOCK。按 §5 顺序与 §8，**LOCKED 与 APPROVED 双条件齐备才可派 workbuddy，缺一不采数**。其前置还欠：步骤1离线复读（分析-only，不采数）尚未交付；且 LOCK 文本必须先补写死 2(a)(b)(c) 与**修正口径**——目前 §4.2 的修正是条件式「如果采用修正」，须在 LOCK 前落实为与 P3 一致的口径（`CONFIRM_F5_REFUTE_2X`，常数偏移优于 2×），不能原样拿去 LOCK。

一句话：最小充分集照 #52 收敛口径，同意；闸门主干闭环、无致命洞，LOCK 前补写死三项判据与修正口径；APPROVED 前只欠「闸门 LOCKED」这一条，未 LOCK 未 APPROVED 不得派工采数。

---

## 附录：派题提示词

```
# DeepSeek：T-A1a 门槛 brief 门控短意见（中文）

读 `docs/notion_a1a_tracker_brief.md`。连贯你 #52：A1a 跟踪转出；若单开则 A1a 归因→A-only×3。

事实：N_h=0.41 沿用；焦点 τ0=21.8 + #51 FAIL 子集；票级 3/3；不重开 R-3/S1；不授权采数直至 APPROVED+LOCK。

请：1) 是否同意该最小充分集；2) 闸门/硬停有无漏洞；3) APPROVED 前还缺哪一条。勿发明数字。
```

（完整提示另含 `notion_a1a_tracker_brief.md` 全文与 #52 意见连贯段，见同会话 raw / `/workspace/deepseek_a1a_brief_full_prompt.txt`。）
