# P4 内部交付内容核验检查点

固定 EveryStage drop-prewarm `191f4d65b15e6fecf0118e0b120c7760ef3d8d53`，MF组件源码 `44401d3f002b427d828500331a3e9b4ada5881cd`。P3 PR8已合并；post [45011904252](https://github.com/oops11111/EveryStage/actions/runs/45011904252) build137175853404 与 smoke137177159825 成功。主代理已复核同一 post 制品与六图：[原始收据](P4_EVIDENCE/POST_PRIMARY_ACCEPTANCE.json)。

来源与产品 SHA 固定在 [P4_SOURCE.json](P4_SOURCE.json)；input14750132089和smoke14750575568复用主代理已下载原件，没有重下载替代、重建或执行安装器。完整27输入/9nupkg成员与实际managed/native加载哈希逐一匹配；normalExit0同PID/starttime，9次安装/卸载调用0，三组件组合通过。六张post图 opening仍closed，最终open/resizeopen/Escclosed成立；不把采样当精确时序或在途反向证明，P2控件测试独立保留。

本阶段只逐字复制安装器、同包feed、原notices/元数据、完整post证据和原0.9.1升级ZIP/receipt，在 ignored 友好目录整理中文 [INSTALL](P4_INSTALL.md)、[COMPONENT](P4_COMPONENT.md)、[VERIFY](P4_VERIFY.md)。不改EveryStage/MF组件生产源、旧pilot/报告/冻结API，未触碰用户机现有应用配置。

只读验证包括：源证据SHA与本地Markdown链接，完整交付SHA清单/集合，27输入映射、9包成员、实际模块与退出/安装/图片来源绑定；PowerShell常用入口无需Python，使用.NET SHA256。主代理发现的FileInfo祖先遍历已修正为FileInfo.Directory→DirectoryInfo.Parent，受控新目录普通祖先通过、同一目录内新junction祖先被拒绝；无需管理员/系统更改，无删除或跟随外部目录。

当前这是 **内容核验通过、最终外部签认待MF文档合并/post CI** 的检查点。ROOT_ACCEPTANCE.json将由主代理在全部门完成后写入本地交付目录，绑定该manifest与同一EXE/nupkg字节，再更新本地SHA256SUMS；源码不锁住初步manifest/SUMS或未来签认哈希，避免循环依赖。外部验收不是代码签名；不重写包内candidate/unsealed构建时历史文本，也不把pipeline-tested状态伪称早已final。

限制：内部未签名；hosted runner（可能管理员），非用户机/标准用户/真实设备/Office/性能认证。available notices和license元数据不保证所有内嵌依赖法律资料穷尽；没有发明产品许可证。实际Actions最多3天，post镜像14749774990于2026-11-25T10:19:55Z到期；原基线离线保存只作为已测升级输入，不证明降级兼容性。真实旧镜像wrapper下载/校验曾通过，post本次metadata首试HTTP200，不声称实际触发重试或模拟404分支。
