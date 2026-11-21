# Z6 F11 REPORT（未复核）

- 票：F11（handover r2 第三节）：FluentHub 侧叉 csproj 改为可配置路径；构建时校验 pack 文件 sha256；依赖 F7。
- 分支：`wb/z6-f11`，**叠在 `wb/z6-f7` 上**，父提交 `6b9f4767829add919cb6e626f45208f2ecf59614`（F7 提交；F7 自身父为 main `7e2feb0e`）。叠加原因：定稿写明 F11 依赖 F7，校验用的 sha 来自 F7 的新 SHA256SUMS。
- worktree：`%USERPROFILE%\mf_z6\f11`（由侧克隆 `%USERPROFILE%\mf_z3a` 建立）。
- 侧叉：`%USERPROFILE%\pilot\FluentHub` @ `eb08f0dbcbb43880ab34038b0cb6742f0c0deca5`（T5 改动仍未提交；本票只改 `src/FluentHub/FluentHub.csproj`；未推送、未 fork、未登录）。
- 许可原话：
  - pidan，卡 t206s992，2026-11-18 07:01 AWST：「wb/z6-* 分支可以普通 push（常设许可）」
  - pidan，卡 t208s1010，2026-11-18 07:43 AWST：「F11 批准非原样开关 -p:WarningsNotAsErrors=NU1903，不识别则退回 -p:NuGetAudit=false，仅限本票」

## 结论
- **PASS（非原样（构建开关：-p:WarningsNotAsErrors=NU1903））**：原位构建通过；换目录构建通过；pack 副本改坏后构建按预期失败，复原后再次原位构建通过。
- `-p:WarningsNotAsErrors=NU1903` 被识别：restore 只把 NU1903 报为警告，构建 0 错误。**未退回** `-p:NuGetAudit=false`，因此没有失败日志可留；四次构建都用同一开关。
- 此开关许可仅限 F11，不适用于 F6 / F2 / F5。

## 侧叉改动（`fluenthub_f11_csproj_delta.patch`，相对 T5 状态）
1. 原硬编码 `FmL1PackDir`（Z5 t5pack 路径）改为三个可覆盖属性（命令行 `-p:` 覆盖优先）：
   - `FmL1Root` 默认 `$(USERPROFILE)\mf_z6\f7\`
   - `FmL1PackDir` 默认 `$(FmL1Root)pilot\l1pack\bin\x64\Release\net8.0\`
   - `FmL1Sums` 默认 `$(FmL1Root)pilot\l1pack\SHA256SUMS`
2. 新目标 `FmL1VerifyPack`（`BeforeTargets="BeforeBuild;ResolveAssemblyReferences"`）：从 `FmL1Sums` 读出 `FluidMotion.L1.dll`、`fluidmotion.dll`、`demo_motion.json` 的期望 sha256，用 MSBuild `GetFileHash`（SHA256）算 `FmL1PackDir` 下三个文件；缺 SUMS、缺条目、缺文件或任一不符都 `<Error>`，构建失败。
- 改法由 `f11_patch_csproj.ps1` 完成（保留原 BOM）。
- 全量侧叉 patch（相对上游 eb08f0db，含 T5 改动与本票改动、新文件 NavMotionPilot.cs）：`fluenthub_f11_side_fork.patch`。
- sha256 见 `patches.sha256`：原始（git diff 原样输出）与入库（用户目录替换为 `%USERPROFILE%`）两列。入库版有 2 行因脱敏与原始不同（delta 中被删除的 T5 硬编码路径行；全量 patch 中 T5 新文件头注释里的侧叉路径），按原始 sha 核对需先把 `%USERPROFILE%` 换回本机用户目录。

## 三步验证（同一开关；命令原文见 `raw/<步骤>/cmd.txt`，输出见 `build.log`）
| 步骤 | 命令要点 | 结果 |
|---|---|---|
| v1 原位 | `dotnet build src\FluentHub\FluentHub.csproj -c Debug -p:Platform=x64 -p:WarningsNotAsErrors=NU1903` | exit=0，2 警告（NU1903）0 错误；`FmL1VerifyPack: OK`，三个 sha 与 F7 SUMS 一致 |
| v2 换目录 | 同上 + `-p:FmL1Root=%TEMP%\z6\f11\pack_reloc\`（F7 pack 三文件与 SUMS 的副本） | exit=0，2 警告 0 错误；`FmL1VerifyPack: OK` |
| v3 改坏副本 | 同上 + `-p:FmL1Root=%TEMP%\z6\f11\pack_corrupt\`；副本 `FluidMotion.L1.dll` 末字节 xor 0xFF（sha 变为 `05179cd1…7da5`） | **exit=1（预期失败）**：`FmL1VerifyPack: sha256 mismatch FluidMotion.L1.dll expected=94d6489d… actual=05179CD1…` |
| 复原 | 副本写回原字节，sha 回到 `94d6489d…0dc2`；F7 原件 sha 未变（`raw/v3_corrupt_wnae/corrupt.txt`） | 已复原 |
| v4 复原后原位 | 与 v1 同命令 | exit=0，2 警告 0 错误；`FmL1VerifyPack: OK` |

- 期望值（F7 `pilot/l1pack/SHA256SUMS`）：FluidMotion.L1.dll `94d6489d9bec289732ea3e4a8540b052e576a7182bae3f3367a3e72f04e10dc2`，fluidmotion.dll `add4857dead413f8211941381c6aa8f73c546f3ecae1774aced79c464719577d`，demo_motion.json `8d5db5775acad5db0c04cbc3c2a214bb7309670c304b71ff40860757973c410e`。
- .NET SDK 10.0.300。没有运行 GUI，没有注册包，没有采数。

## 入库
- `z6/f11/REPORT.md`、`f11_patch_csproj.ps1`、`f11_build.ps1`、`f11_collect.ps1`、`f11_composition.ps1`（复制自 f7）
- `z6/f11/fluenthub_f11_csproj_delta.patch`、`z6/f11/fluenthub_f11_side_fork.patch`、`z6/f11/patches.sha256`
- `z6/f11/raw/`：v1–v4 的 cmd / times / build.log、`v3_corrupt_wnae/corrupt.txt`、`MANIFEST.txt`（原始与入库 sha）、`composition_pre.txt` / `composition_post.txt`
- `z6/LEDGER_snapshot.md`
- 不含 FluentHub 源码文件本身。

## 偏差
- 定稿只要求三步；另加 v4（复原后原位再构建），作为「复原」的构建侧证据。
- patch 入库版做了路径脱敏，与 git diff 原始字节不同（两列 sha 都记）。
- 侧叉 bin/obj 产物被本票构建更新（最后一次为 v4 原位）。
