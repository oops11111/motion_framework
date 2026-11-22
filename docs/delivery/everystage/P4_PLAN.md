# P4：EveryStage 0.9.2 内部交付计划

验收来源：用户已选择自有 EveryStage 应用、内部本地可安装应用包＋组件包；主代理在 P3 合并后全部检查成功后明确放行本阶段。不是公开 NuGet/GitHub Release。
固定来源：MF main/component source `44401d3f002b427d828500331a3e9b4ada5881cd`；EveryStage drop-prewarm merge `191f4d65b15e6fecf0118e0b120c7760ef3d8d53`；post run45011904252 build137175853404、smoke137177159825 均成功。

1. 依赖与来源：先核 MF main 干净且固定444；复用主代理下载的 post input14750132089（ZIP SHA c0b83279f1628271660092fab184a647a5adf346e8e744084b413bd3b23c552d）与 smoke14750575568（511e1d2a8363cb4a426326330412923b080cac92d27e99628e9a721926e940c3）。交付来源收据；只读 SHA/ZIP/JSON 验证。任何身份、源提交、成员/hash 漂移即停止，不替代下载、不重建。
2. 整理：在全新 ignored `m0/out/EveryStage-0.9.2-internal-45011904252-<GUID>` 逐字节复制已测 EXE、同一 nupkg/feed、真实 notices、完整 smoke 和来源证据、原0.9.1 baseline ZIP/receipt。验证27 inputfiles、9 nupkgmembers、mappedmanaged/native来源、normalExit0同PID/starttime、所有9安装/卸载调用0、3组件选择与清理。生成中文 INSTALL/COMPONENT/VERIFY、完整 SHA256SUMS、交付 manifest。无运行安装器/实际应用、无依赖升级/新构建/ES源码改动。
3. 文档与主审：MF 新 docs-only PR 仅 `docs/delivery/everystage/P4*`（含小证据子目录/只读校验器）。不得提交 EXE/nupkg/大原件，不改旧报告/冻结源/旧公开面。主代理复核 post 六图与全部来源，外部 acceptance 此时标记待最终文档合并，不冒充已最终签认；明确包内 candidate 是构建时状态，同字节外部验收不重写包。
4. CI 与最终封存：定向校验 `python docs/delivery/everystage/P4_VERIFY.py --delivery <fresh-directory> --allow-pending`（最终默认不带此选项）（只读 hash/source/文档完整性）；原有适用 CI 全绿后主代理合并 MF PR、跟 main post，最后签认外部最终 acceptance 同字节；实现代理不合并。最终交付目录路径、source/run/artifact/EXE/package SHA 与限制清晰汇报。

边界：内部未签名；hosted runner（可能管理员）验收不是用户机/标准用户/真实投屏设备/Office/性能认证。全部现有历史失败保持原样，P3 pending checkpoint 仅由本次 final 证据更新当前状态。依赖元数据/已提供 notices 不等同所有嵌套依赖法律完整证明，不发明产品许可证。Actions 实际仅3天保留，离线原 baseline 包与 hash 收据随交付；所有云副本过期则 CI 明确停止，长期输入保留另需范围决定，不改外部设置。无安装/证书/账户/任务/用户资料操作。
