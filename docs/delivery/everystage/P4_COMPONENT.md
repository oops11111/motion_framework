# FluidMotion.L1 0.1.0：内部组件接入

包位于 `component/feed/FluidMotion.L1.0.1.0.nupkg`。固定源码 MF `44401d3f002b427d828500331a3e9b4ada5881cd`；与本次 EveryStage 安装器消费的是同一不可变包。PackageId/AssemblyName 为 FluidMotion.L1，实际命名空间仍为 **MotionSoftDemo**，没有宣称已改名。目标 net8.0，支持的本机资产仅 win-x64；不是 AnyCPU 本机运行时包。

## 精确来源与独立缓存

在自己项目的 NuGet.config 设置本地 feed，并让 FluidMotion.L1 只映射到该源；其他依赖仍使用项目原本可信来源：

```xml
<configuration>
  <packageSources>
    <clear />
    <add key="internal-motion" value="ABSOLUTE_PATH_TO_DELIVERY/component/feed" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="internal-motion"><package pattern="FluidMotion.L1" /></packageSource>
    <packageSource key="nuget.org"><package pattern="*" /></packageSource>
  </packageSourceMapping>
</configuration>
```

显式精确版本（普通0.1.0是最低版本范围，不能替代方括号）：

```xml
<PackageReference Include="FluidMotion.L1" Version="[0.1.0]" />
```

本次属于内部首版，同版本历史候选可能有不同字节。使用新的独立缓存，避免已有0.1.0候选绕过源映射：在当前 PowerShell 会话设置 `$env:NUGET_PACKAGES = 'NEW_EMPTY_CACHE_PATH'`，再按自己项目正常 restore/build；不写用户/系统环境。该命令仅是接入说明，本交付整理未对用户项目执行还原或构建。核对 nupkg SHA `b2e47d89488e51ef32d16b4775298d6171dddfc992eb187d4d7f253243354062`。

## 运行与生命周期

使用公开 MotionSystem/AnimationHandle API，保持 UI owner 线程；正常外层帧泵调用 OnFrame，native apply callback 只应用值或排队，不在 callback 内重入 fm 调用。在途反向以 from:null 连续重定向。空闲停止帧泵，Dispose 时解除系统偏好、父窗口事件/计时器并释放 handle；在 native callback 中遇到销毁须延后本机清理到外层返回。真实系统动画偏好只读，不切换 OS 设置。

可复用的已测试 WinForms 私有 adapter 是 [EveryStage DrawerHeightMotion.cs](https://github.com/oops11111/EveryStage/blob/191f4d65b15e6fecf0118e0b120c7760ef3d8d53/src/Terminal/EveryStage.Terminal/UI/DrawerHeightMotion.cs)。它不是新增公开 API，也不要求复制旧 pilot DLL 或 FmL1Root 缓存。完整公开面与旧基线匹配、缺 native/错误 RID 硬失败和独立真实 API/native 消费验证见包来源 `evidence/input/provenance/framework-result.json`。

## 构建时 candidate 与最终内部验收

nupkg 内 candidate/unsealed 文本和框架 result 是构建时的真实状态，不会为了发布伪造新包字节。最终外部 ROOT_ACCEPTANCE.json 在主审、合并与 CI 完成后绑定该同一包 SHA、应用安装器 SHA 和文档来源；它是内部验收封存，不是公开 NuGet 发布或代码签名。manifest.json 的 assembly 状态也是当时快照，最终状态由外部验收收据与校验结果决定。

依赖元数据/已随包提供的 notices 在 notices 与 evidence/input 中。某些包只给 license URL/expression、没有完整嵌套依赖 notices；不推断 Pdfium 所有内含代码均同一许可证，也不编造产品许可证或声称法律资料穷尽。内部再分发应保留现有 notices 与这些限制。
