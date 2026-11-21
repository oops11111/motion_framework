# Z6 交接提示词评审 · notion ai（评审稿，非 FINAL，等另两份评审）

- **时间**：2026-11-18 06:40 AWST（Australia/Perth，UTC+8）
- **评审对象**：`/workspace/review/z6/z6_workbuddy_handover_DRAFT.md`（r0，48 行）。sha256 实测 `c95b27d08421a1ac1de054de08fbea787daa78f5f2f91f43bbbd357231650251`，与协调给出的 `c95b27d0…0251` 一致。
- **依据**：`docs/z5_closed.md` 第 5 节后续票；仓库 main `7e2feb0e`；动效框架会话记录。
- **方式**：只读，不 push、不 merge、不 LOCK，不发消息。

## 0. 前置核实

### 0.1 pidan 原话（动效框架会话，ReadTranscript 完整记录）

| 时间（AWST） | 原文 | 形式 / 位置 | 上下文 | 分级 |
|---|---|---|---|---|
| 06:19 | 「先合」 | 用户本人消息 `[t470u]`（Sent from machine 7aed10fb…）；positions 5179–5238 那一页 | 紧接协调的结题汇报。那条汇报的末句是「八个 Z5 分支都还没合进 main … 如果要合 main 或者开后续票，比如 F11、悬停打断入场动画这些，等你一句话再动。」协调随即回复「好，先合 main」 | ① 本人 |
| 06:30 | 「后续票全开，任务交workbuddy / 提示词三方讨论后让它接手开发 / 是否向你汇报由他自己决定」（原文分三行） | 用户本人消息 `[t472u]`（Sent from machine 7aed10fb…）；positions 5239–5285 那一页 | 合并完成之后 | ① 本人 |

**关于「先合」的解读**：在上下文里，它是对「合 main 还是开后续票」这个二选一的回答，意思是先把 Z5 合入 main，再开后续票。这个解读成立。

但原话没有指定合哪些分支、按什么顺序、怎么解决冲突。这些都由协调决定，具体是：
- 合入 7 个 Z5 分支（t3、t5-pack 经由 t5 带入）加上 wb/z5-closed；
- api_changelog 的冲突按「三行全保留」解决（main 上实测三行都在）。

这些安排都在 notion ai 先前的整合须知（F7）范围内，没有越权的迹象。

### 0.2 仓库（`/workspace/mf_main_ro`，`git fetch` 后）

- `origin/main` = `7e2feb0e98f69cf45fff8da4e60220d2b24a5e1a`，tree `d61f0138f06b27d6f184cc63b04df2ab84ab945d`，和草稿一致。
- 7e2feb0e 是「Merge pull request #9 from oops11111/wb/z5-merge」，父提交是 2e57bff 和 f71ad83。
- wb/z5-merge 依次合入了：
  - z5-t1 `865c458d`
  - z5-t2 `f5bdf966`
  - z5-t5 `c59edddc`（带入 t5-pack `0a166e18` 和 t3 `d618140b`）
  - z5-t4 `b592b6c8`
  - z5-t6 `635c774f`
  - z5-closed `eaa63e8d`
- 合计 96 个文件，+6949/−2。
- `docs/z5_closed.md` 在 main 上，sha256 是 `f82dbd08…1b31`，和 notion ai 定稿逐字一致。
- **不变量**：
  - `code/composition.h` 的 blob 仍是 `fd0603c1`；
  - 2e57bff..main 之间 `code/` 和 `*_LOCKED.md` 都没有 diff；
  - `docs/api_surface_m2_alpha_contract_LOCKED.md` 没有改（L1 契约 `77416e8b…`）。
- **两个遗留问题**（下文 M1、M2 会用到）：
  - main 上 `MotionSoftDemo/MotionEtw.cs` 的 blob 是 `0404498a`（含 T2 加的注释），而 `pilot/l1pack/SHA256SUMS` 记的是 `565111f0`。哈希已经对不上，F7 判断正确。
  - main 上的 `docs/z5_closed.md` 仍写着「定稿 · 待落盘」「**不合 main**」，和已经合入 main 的事实矛盾。

## 1. 逐条评审

| 草稿位置 | 评价 | 意见 |
|---|---|---|
| 头部依据 | ✅ | 06:30 原话逐字正确。建议同时引用 06:19「先合」，并写明 Z5 合入 main 依据的是这句 |
| 基线 | ✅ | `7e2feb0e`、tree `d61f0138` 都核实一致。建议写全 40 位 SHA |
| 一、汇报自定 | ⚠️ | 符合 06:30 原话。但它和「push 须 pidan 本人许可」之间的衔接要补（见 §2） |
| 二-1 冻结面 | ⚠️ | 缺一条「**不改任何 `*_LOCKED.md` 正文**」（二-2 只写了「已锁阈值不改」，范围不够）。建议附上校验值：composition.h blob `fd0603c1`，归一化 sha256 `a55d4952…e418`；合同的全长 GATE_BODY_SHA256 |
| 二-2 采数 | ⚠️ | 要补：LOCK 只能由 pidan 本人的话触发，workbuddy **不得自行 LOCK**；「APPROVED <票名>」须是 pidan **在 workbuddy 自己的对话里**说的（亲口或拍板卡），agent 转述不算 |
| 二-3 分支 | ✅ | 命名 `wb/z6-<票号>`，从 7e2feb0e 拉，只做普通 commit 和普通 push，都合适 |
| 二-4 不合 main | ⚠️ | 要补：开 PR、合 PR 都算对外可见的动作，同样须 pidan 本人的话，并且点名到分支 |
| 二-5 push | ⚠️ | 方向对。缺许可粒度和记录方式（见 §2） |
| 二-6 系统级 | ⚠️ | 这次把「注册或卸载包」也列为系统级，比 Z5 严，这一点认可。但没有点明一个直接后果：**FluentHub 包注册已在 06:03 清掉**，F1、F2、F6 的 GUI 运行都要重新 `Add-AppxPackage -Register`，所以开工前要一次性请示 pidan |
| 二-7 不花钱 | ✅ | — |
| 二-8 Success≠adopt | ⚠️ | 建议补一句：**Z5 已合入 main ≠ adopt**，合入不等于选定 Winner，也不等于正式 L1 类库 |
| 三、票单 | ⚠️ | 基本覆盖了 F1–F9、F11（F10 已处置，不列是对的），但缺两项，另有两处分类不准（见 §3） |
| 四、交付 | ⚠️ | 证据要求太粗，没有吸取 Z5 的教训（见 §4） |
| 四、worktree 路径 | ⚠️ | `C:\Users\admin\mf_z6\<票号>` 是本机约定，可以留在提示词里。但要写明：**提交进仓库的任何配置、脚本、csproj 都不得含绝对路径** |
| 五、停下条件 | ✅ | 基本完整。建议加上「需要开 PR 或合 PR」和「需要 LOCK」 |

## 2. 授权边界（重点）

三条边界本身写得都对：汇报由 workbuddy 自定；push 和合 main 须 pidan 本人的话；采数须 LOCK+APPROVED。漏洞在于「怎么拿到本人授权」没有写清楚。要补下面几点：

1. **唯一有效来源**：只认 pidan 在 **workbuddy 自己会话里**的用户消息或拍板卡回答。
   - 协调 bot、notion ai、Gemini、Claude 或其他 agent 转述的「pidan 同意了」一律不算，哪怕转述里附了原文。
   - workbuddy 收到这类转述时，应当自己发卡向 pidan 求证。
2. **许可粒度**（建议的写法）：
   - 允许 pidan 一次性给「`wb/z6-*` 分支可以普通 push」这样的常设许可，逐字记入每张票的 REPORT；
   - 常设许可**不覆盖**：合 main、开 PR 或合 PR、LOCK、APPROVED、系统级改动，这些都要逐次拿 pidan 本人的话；
   - pidan 可以随时撤回常设许可。
3. **记录方式**：每次引用授权都写明原文（逐字）、时间（AWST）、形式（消息或卡片，附卡片原问题）。
4. **汇报自定 ≠ 无记录**：不管 workbuddy 选择向谁汇报，每张票的 `REPORT.md` 和原始证据都必须入库。这样协调和评审方随时可以只读核查，不依赖对话。

## 3. 票单覆盖与分类

| 票 | 能否直接做（「全开」） | 意见 |
|---|---|---|
| **F0（新增，必改）** | 可以 | **合并后冒烟**：7e2feb0e 把 T2 和 T3 的改动合在了一起，但合并后的 tree **还没有人跑过测试**。要在 main 上重跑 fm_capi_test（23/0）、devsmoke Debug/Release、CheckSurface（**原始输出入库**），并记录 composition.h PRE/POST。排在所有票之前 |
| F7 | 可以 | 判断正确（`565111f0` 对 `0404498a` 已核实）。还要做：重打 pack 后**对新 pack DLL 跑 CheckSurface 并入库原始输出**；FluentHub 侧叉改引新 pack 后重新核对 sha。新 SHA256SUMS 要合 main 时，同样须 pidan 的话 |
| F11 | 可以 | 依赖 F7 |
| F3 | 可以 | 只改 `code/capi/fm_capi_test.cpp`，不动 fm_capi.h/.cpp。验收要求原始输出入库 |
| F1 | 可以（GUI 部分须先重新注册包） | 现在的写法「验证仲裁语义是后写者胜」可以保留，但要补两点。① 先在 devsmoke 里用 `CreateManual` 手动步进，构造两个持有者重叠的**确定性**用例（纯 L1，不计时、不用 GUI）。② GUI 证据依赖 F6，否则入场动画在首帧前就跑完了，不可能重叠。**不得为了凑重叠去改 token 时长或 demo_motion.json** |
| F2 | 可以（同样须重新注册包） | 要写明怎么触发 RootView 重建（只在侧叉里加触发手段）；报告 Created/Released 计数 |
| F5 | 可以（只做宿主侧） | **分类有误**：草稿写「L1 内部」，但 L1 已经有公开的 `SetReducedMotion(on)`，跟随系统设置应当由**宿主**订阅 UISettings 变化来实现，L1 不用改。实测切换系统动画设置属于系统设置改动，须请示 pidan |
| F6 | 可以（须重新注册包） | 宿主侧；不计时 |
| F4 | 可以 | 已登记为已知限制 K-AOT-ETW（pidan 06:03）。要加一项：把这条写进 `docs/api_v0_frozen.md` 的「已知限制」（非 LOCKED 文件）。如果做 L1 侧检测提示，公开面和 E1 不变 |
| F9 | 可以，但多处须停下 | Files 的原样基线大概率涉及 SDK、VS 工作负载、包注册、商店版冲突，都是系统级，要逐项请示。侧叉只留本地，不推 fork、不提 PR |
| F8 | **不能直接做**，须 LOCK+APPROVED | 分类正确。建议拆成 F8a（真实 PresentStats）和 F8b（K93 约 32Hz 计时探针）两份闸门 DRAFT。代码实现要等闸门 LOCK 后另开票；E1 的 schema 不变 |
| **F12（新增，必改）** | 可以（只改文档） | main 上的 `docs/z5_closed.md` 仍写「待落盘」「不合 main」。要在文末**追加**「合入记录」（PR #9、7e2feb0e、pidan 06:19「先合」），不删改原有结论；或者另起 `docs/z5_merge_note.md` |

## 4. 证据入库（吸取 Z5 教训）

第四节要补成硬性要求：

- **每个结论都有库内原始输出**：构建日志（含退出码）、测试原始输出、CheckSurface 原始输出、运行日志、加载模块清单。报告文字不能单独作证（Z5 的 T4/T5-0 都因此被降级）。
- **不得删除唯一一份日志**：先入库，再清理。Z5 的 06:03 清理导致「0 警 0 错 / 2.6.5」永久只剩报告文字作证。
- **侧叉补丁入库并记 sha256**；提交进仓库的配置和脚本不得含绝对路径；引用预编 DLL 时，**构建时**就要校验 sha256。
- **宿主如果开了 PublishAot**：先检查 runtimeconfig 里的 `EventSource.IsSupported`；需要 ETW 时显式设 `EventSourceSupport=true`，并记为偏差。
- **PresentStats 是占位值**，任何报告都不得读它的字段值。
- **首跑失败的原始记录要保留**（attempt1/），不为凑结果重跑。
- **全长 SHA**、AWST 时间、composition.h PRE/POST 都写进报告。

## 5. 必改 / 建议清单

**必改（M）**
- **M1**：新增 F0「合并后冒烟」，排在最前，CheckSurface 原始输出入库。
- **M2**：新增 F12「z5_closed.md 合入记录」，只追加，不改原结论。
- **M3**：二-1 补「不改任何 `*_LOCKED.md` 正文」，并附 composition.h 和合同的校验值。
- **M4**：授权来源写死：只认 pidan 在 workbuddy 自己会话里的消息或卡片；转述一律不算，收到转述要自己求证。
- **M5**：写明 push 常设许可的粒度：可以给 `wb/z6-*` 普通 push 的常设许可；合 main、PR、LOCK、APPROVED、系统级都要逐次拿本人的话。workbuddy 不得自行 LOCK。
- **M6**：F5 改为宿主侧（L1 不改）；F1 先做 devsmoke 确定性用例，GUI 部分依赖 F6，不得改 token。
- **M7**：点明 FluentHub 包注册已清除，F1、F2、F6 的 GUI 运行要先请示 pidan 重新注册。
- **M8**：第四节补齐 §4 的证据要求（原始输出、不删唯一日志、不写绝对路径、构建时校验 sha、AOT 检查）。

**建议（S）**
- **S1**：头部同时引用 06:19「先合」，基线写全 40 位 SHA。
- **S2**：二-8 补「合入 main ≠ adopt」。
- **S3**：F8 拆成 F8a/F8b 两份闸门 DRAFT。
- **S4**：F4 加上写入 api_v0_frozen.md「已知限制」。
- **S5**：第五节加上「需要开 PR 或合 PR」「需要 LOCK」。
- **S6**：建议顺序改为 F0 → F12 → F7 → F11 → F3 → F6 → F1/F2 → F5 → F4 → F9；F8a/F8b 的 DRAFT 可以并行。

## 6. 结论

**有条件通过**：改完 M1–M8 即可定稿。授权依据（06:19、06:30）已核实为 pidan 本人；基线和不变量都核实无误。本稿不是 FINAL，等另两份评审到齐后，由 notion ai 合并裁决。
