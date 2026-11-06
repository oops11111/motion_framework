# 协调规则（Y4）

落地时间：2026-11-06 Australia/Perth。来源：pidan 转发的 Y4 条；与现有 M0–M3 真机工作流对齐。

## 规则

1. **写前先同步（sync before writing）**  
   落盘或改 docs/code 前，先拉/对齐当前 tip 与相关 LOCK/APPROVED/派工单，避免基于过期分支改写。

2. **状态日志只追加（only append to the status log）**  
   `docs/m2_mainline_brief.md` 及同类主线日志只追加条目，不改写历史行。

3. **采数前先写批准记录（write the approval record before collection）**  
   真机采数前必须有本 chat 明示的 APPROVED，并先落 `*_approved.md`（含 APPROVED_AT 与 gate 哈希核对）。无 APPROVED 不得开 lab。

4. **实验室只加结果文件（the lab only adds result files）**  
   workbuddy / 真机侧只新增结果目录与结果文件（及必要的旁路推送），不改 composition.h、不改闸门正文、不改结题裁决。

5. **本地改动要上报（report any local changes）**  
   旁路克隆、本机补丁、DLL 哈希、未进 git 的改动，须在回报里写明路径与差异。

## 角色

| 角色 | 谁 | 职责 |
|------|----|------|
| 协调 | 动效框架 bot | 派工、落盘、拍板卡、结题收敛 |
| 实验室 | workbuddy cli（IJI 采数 / K93 推送） | 按派工单采数与推送；只加结果 |
| 评审/规格 | Notion AI / Claude | DRAFT、结题、英文定稿（Claude 额度空窗期由 Notion 顶） |

## 禁区（沿用）

- 不改 `composition.h`（除非另有 LOCK 授权）
- Success ≠ adopt；不合 `main`
- PERF-CI 默认 HOLD，除非另拍板
- 采数须 pidan APPROVED
