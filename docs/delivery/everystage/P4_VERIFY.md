# 核验交付与最终验收

不需要先安装 Python。最常用的 PowerShell 只读核验：

```powershell
Get-FileHash -Algorithm SHA256 .\EveryStage-Setup-0.9.2-win-x64.exe
Get-FileHash -Algorithm SHA256 .\component\feed\FluidMotion.L1.0.1.0.nupkg
```

预期安装器 SHA：`e2f5ef48949d7af817de6d56b25eadbaca4bc6bf2331eaec68212f82d4a2a286`。
预期 nupkg SHA：`b2e47d89488e51ef32d16b4775298d6171dddfc992eb187d4d7f253243354062`。

在允许正常运行脚本的 PowerShell 中，`./VERIFY.ps1` 使用 .NET SHA256 只读验证全部 SHA256SUMS、完整文件集合、manifest 产品来源和最终外部验收。无需改执行策略/安装组件。若策略不允许运行，可按上面的交互式命令核验关键文件；不要把未执行的完整检查写成已通过。

缺 ROOT_ACCEPTANCE.json 时报告内容可核验但最终验收待完成，并返回非零；开发整理阶段可显式 `./VERIFY.ps1 -AllowPending` 只验内容并保留 pending 状态。最终收据必须绑定 manifest 与安装器/组件原 SHA、同一 app/component 来源、MF 文档合并及成功 post CI。签认不是可信发布者数字签名。

开发者可用本项目已验证的 Python3.13（无需用户另装）运行只读 `P4_VERIFY.py --delivery <directory>`，完整复核 input映射、9个 nupkg成员、实际模块、正常退出、9安装调用、三组件组合和主审图片 SHA；不重跑 GUI/构建。整理阶段 `--allow-pending`，最终默认要求封存收据。

证据：evidence/input 含原 build-input、component-state、框架/消费收据与依赖元数据；evidence/smoke 是本次 post 原始日志/截图/快捷方式取证。证据里的 `.lnk` 只供读取取证，不应双击执行。baseline 是已测0.9.1升级输入及原 hash 收据，不是降级兼容性保证。SHA256SUMS 不包含自身，外部验收不引用这份清单 SHA，从而没有自引用哈希循环；加入最终 ROOT_ACCEPTANCE 后清单由主代理更新。

固定来源：EveryStage `191f4d65b15e6fecf0118e0b120c7760ef3d8d53`，MF组件 `44401d3f002b427d828500331a3e9b4ada5881cd`，post [45011904252](https://github.com/oops11111/EveryStage/actions/runs/45011904252)。实际 input14750132089 ZIP SHA `c0b83279f1628271660092fab184a647a5adf346e8e744084b413bd3b23c552d`；smoke14750575568 ZIP SHA `511e1d2a8363cb4a426326330412923b080cac92d27e99628e9a721926e940c3`。包与安装器只逐字节复制，没有重建。

Actions 实际3天保留，post镜像14749774990到期2026-11-25T10:19:55Z；原始基线到期2026-11-25T02:45:33Z。离线目录不依赖这些 URL 继续存在，但必须自行妥善保存整个交付目录。以后全部云副本失效会使升级 CI 停止，不自动换版本或凭据。历史 P3 文档的 pending/失败是当时检查点；本次 post 主审与最终外部收据只更新当前验收状态，不改写过去结果。
