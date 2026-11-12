# Z4 r1 IJI1KJG 松散注册打包构建说明（s2）

本文件在会话结束后写入，不改写本目录既有会话产物（verdict.json / env.json / session.log / trace.nettrace / bad_tokens.json）。产物由外部 PowerShell 从 `env.json` 的 `resolved_outdir`（package LocalState）原样拷回，env.json 保留原路径。

本票从一开始就把两种形态分开构建。松散布局输出在独立目录 `_z4_loose_layout`，不覆盖非打包 `_z4_unpackaged_build`。

## ① 临时 MSBuild 导入

与非打包相同的临时 props/targets（不进仓库）：

- props：`C:\Users\Lh\AppData\Local\Temp\z4_formal_iji\exclude_default_obj.props`
  - sha256：`9e548c9cfdd1205c987585c4c4b1fe839574f33bf1ed16c3fda8e8575baf4836`（247 字节，无 BOM）
- targets：`C:\Users\Lh\AppData\Local\Temp\z4_formal_iji\exclude_default_obj.targets`
  - sha256：`4b67a243c1b2f0a0ea3a494267b0d32b6a113eee1cf28e25a4aa721254254348`（306 字节，无 BOM）

## ② 独立输出目录与完整命令

- OutputPath = `C:\Users\Lh\mf_z3a\_z4_loose_layout\`
- BaseIntermediateOutputPath = `C:\Users\Lh\mf_z3a\_z4_loose_obj\`
- MSBuildProjectExtensionsPath = `C:\Users\Lh\mf_z3a\_z4_loose_obj\`
- 参数：`-p:Z4Package=true -p:Z4LooseLayout=true -p:AppxPackageSigningEnabled=false -p:GenerateAppxPackageOnBuild=false -p:AppxSymbolPackageEnabled=false`
- 求值 `WindowsPackageType=MSIX`
- 未导入 LocalMachine 证书，未弹 UAC

restore（exit 0，2026-11-11 08:52:17 Asia/Shanghai）后 build（exit 0，08:52:18–08:52:28 Asia/Shanghai）：

```
dotnet build MotionSoftDemo\MotionSoftDemo.csproj -c Debug -p:Platform=x64 -p:Z4Package=true -p:Z4LooseLayout=true -p:AppxPackageSigningEnabled=false -p:GenerateAppxPackageOnBuild=false -p:AppxSymbolPackageEnabled=false -p:OutputPath=C:\Users\Lh\mf_z3a\_z4_loose_layout\ -p:BaseIntermediateOutputPath=C:\Users\Lh\mf_z3a\_z4_loose_obj\ -p:MSBuildProjectExtensionsPath=C:\Users\Lh\mf_z3a\_z4_loose_obj\ -p:CustomAfterMicrosoftCommonProps=C:\Users\Lh\AppData\Local\Temp\z4_formal_iji\exclude_default_obj.props -p:CustomAfterMicrosoftCommonTargets=C:\Users\Lh\AppData\Local\Temp\z4_formal_iji\exclude_default_obj.targets
```

AppxManifest 在布局根目录：`C:\Users\Lh\mf_z3a\_z4_loose_layout\AppxManifest.xml`

注册（exit 0，未提升，2026-11-11 08:53 Asia/Shanghai）：

```
Add-AppxPackage -Register C:\Users\Lh\mf_z3a\_z4_loose_layout\AppxManifest.xml
```

- PackageFullName = `MotionSoftDemo_1.0.0.0_x64__zb6e6ksq6rq3m`
- PackageFamilyName = `MotionSoftDemo_zb6e6ksq6rq3m`
- SignatureKind = None（预期）
- IsDevelopmentMode = True
- InstallLocation = `C:\Users\Lh\mf_z3a\_z4_loose_layout`
- 启动：执行别名 `C:\Users\Lh\AppData\Local\Microsoft\WindowsApps\MotionSoftDemo.exe`（保证包身份），不是非打包 exe

## ③ exe / 托管 dll / fluidmotion.dll sha 对照

| 文件 | 非打包 `_z4_unpackaged_build` | 松散布局 `_z4_loose_layout` |
| --- | --- | --- |
| MotionSoftDemo.exe | `0c6d3a980b648ec8b8c439ce2f6dcc5baa8cee46224169e253b52fd54b828c4b`（163840） | 同 sha、同长度（apphost 两侧相同，预期） |
| MotionSoftDemo.dll（托管） | `1711f976692339878b59cac75d350d95d26e08a7f640c1c152f2889ff923fd28`（1314816） | `75c9b9e3dd0317a23e4da4e5a3c5443f88612215029e2bd13324c19b1283a46e`（1315328），不同 |
| fluidmotion.dll | `a77dcb62a108700d7d4d7f73eabe63a21d244e0b37da97ffc8f83ce1d4c823ea`（448000） | 同 sha、同长度 |

`z4_build_info.txt` 两侧相同，tip=`fe59c6229784ee7712926e3e0f94bcc0187f9069`。