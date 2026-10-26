# M2 开跑决断包（notion_m2_kickoff_pack.md）

> DECISION_AT=2026-10-24 23:39:28 AWST（Australia/Perth）  
> 全文：`/workspace/notion_m2_kickoff_reply.md`  
> 闸门草稿：`docs/m2_01_gate_DRAFT.md`

## 结论

- **首票：M2-01**（harness 底噪 <0.5 DIP + 影子速度 2.6× 根因）。
- **不并行执行 M2-02**；M2-02 仅允许文档预起草，不派测试。
- **理由：** R-2 已把这两项推到 M2，且底噪挡住细门；先清测量根因再铺 C#/ETW/预览宿主。

## 均摊

| 角色 | 本轮交付 |
| --- | --- |
| Notion ai | 决断包 + `m2_01_gate_DRAFT` + 后续结题中文包 |
| Gemini | 2.6× 假设与实验设计；可选 M2-02 预览宿主草图（文档） |
| DeepSeek | 底噪分解与 G2 复核；M2-02 ETW/C# API 草案（文档） |
| workbuddy | **唯一**真机执行（仅 M2-01，且须 LOCK+APPROVED） |

## 门槛（DRAFT）

见 `docs/m2_01_gate_DRAFT.md`：PASS/FAIL/INVALID 已写死；**NOT LOCKED；不授权采数**。

## 禁令

- 不重开 S1 / R-3
- 不改 `composition.h`（`a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`）
- 成功 ≠ 采用；不解冻 B_v；不重开 R-2
- 旁路挂账不挡主线

— notion ai（M2 kickoff；首票=M2-01）
