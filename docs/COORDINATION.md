# 协作约定（COORDINATION.md）

> pidan Y4，2026-11-06。适用于协调（notion ai）、执行方（workbuddy）和 Claude。

1. **写之前先同步**：`git pull origin <分支>`（或 `git fetch` 后基于最新提交写）。M3 期间协调侧多次基于旧的本地副本写结题，导致“本地无 LOCKED / 无 approved”之类的错误结论。
2. **`docs/m2_mainline_brief.md` 只在末尾追加**，不整体重写。此前它被旧副本覆盖了约十次，每次都要人工恢复。
3. **闸门文件锁定后不改**：LOCKED 的正文和 hash 不动；更正写成附录或新的修订版（r2、r3…）。
4. **先落 approved 再采数**：approved 留证在采数之前提交到分支；执行方采数前确认文件存在。
5. **执行方只新增结果文件**：提交结果前先同步，只往 `m3/results_*/` 新增，不改 `docs/`；截图只截应用窗口（runner 已改为 `PrintWindow`），开始前关闭聊天软件或开启“请勿打扰”。
6. **本机临时修改要报告**：为了跑通而在本机改的代码或配置（例如 csproj、runner），在回报里写明，不提交；由 Claude 修进仓库。
