# Z4 r1 IJI1KJG 非打包构建说明（s1 / s3 共用同一份输出）

本文件在会话结束后写入，不改写本目录既有会话产物（verdict.json / env.json / session.log / trace.nettrace / bad_tokens.json）。

本票从一开始就把两种形态分开构建，非打包使用独立 OutputPath / IntermediateOutputPath，不与松散布局共用。

## ① 临时 MSBuild 导入：排除工程默认 obj\**

改 `BaseIntermediateOutputPath` 后，SDK 的 `DefaultItemExcludes` 可能不再排除工程目录 `MotionSoftDemo\obj\**`。构建时通过命令行属性导入仅存在于 `%TEMP%` 的临时 props/targets（不进仓库、不改 csproj / Directory.Build.*）。本次工程默认 `obj\` 实际上未生成。

- props：`C:\Users\Lh\AppData\Local\Temp\z4_formal_iji\exclude_default_obj.props`
  - sha256：`9e548c9cfdd1205c987585c4c4b1fe839574f33bf1ed16c3fda8e8575baf4836`
  - 字节：247
  - UTF-8 BOM：False
- targets：`C:\Users\Lh\AppData\Local\Temp\z4_formal_iji\exclude_default_obj.targets`
  - sha256：`4b67a243c1b2f0a0ea3a494267b0d32b6a113eee1cf28e25a4aa721254254348`
  - 字节：306
  - UTF-8 BOM：False

与 K93 同文等效（K93 为 248 / 307 字节，本机少 1 字节行尾差异，内容相同）。

## ② 独立输出目录与完整命令

- OutputPath = `C:\Users\Lh\mf_z3a\_z4_unpackaged_build\`
- BaseIntermediateOutputPath = `C:\Users\Lh\mf_z3a\_z4_unpackaged_obj\`
- MSBuildProjectExtensionsPath = `C:\Users\Lh\mf_z3a\_z4_unpackaged_obj\`
- 未传 `Z4LooseLayout` / `Z4Package`；求值 `WindowsPackageType=None`
- 未覆盖松散布局输出目录 `_z4_loose_layout`

restore（exit 0，2026-11-11 08:51:49 Asia/Shanghai）：

```
dotnet restore MotionSoftDemo\MotionSoftDemo.csproj -p:Platform=x64 -p:BaseIntermediateOutputPath=C:\Users\Lh\mf_z3a\_z4_unpackaged_obj\ -p:MSBuildProjectExtensionsPath=C:\Users\Lh\mf_z3a\_z4_unpackaged_obj\
```

build（exit 0，2026-11-11 08:51:52–08:52:12 Asia/Shanghai）：

```
dotnet build MotionSoftDemo\MotionSoftDemo.csproj -c Debug -p:Platform=x64 -p:OutputPath=C:\Users\Lh\mf_z3a\_z4_unpackaged_build\ -p:BaseIntermediateOutputPath=C:\Users\Lh\mf_z3a\_z4_unpackaged_obj\ -p:MSBuildProjectExtensionsPath=C:\Users\Lh\mf_z3a\_z4_unpackaged_obj\ -p:CustomAfterMicrosoftCommonProps=C:\Users\Lh\AppData\Local\Temp\z4_formal_iji\exclude_default_obj.props -p:CustomAfterMicrosoftCommonTargets=C:\Users\Lh\AppData\Local\Temp\z4_formal_iji\exclude_default_obj.targets
```

启动会话使用的 exe：`C:\Users\Lh\mf_z3a\_z4_unpackaged_build\MotionSoftDemo.exe`
新输出无 `AppxManifest.xml` / `resources.pri`。

native DLL 预先编译（vcvars64 + cl，exit 0，2026-11-11 08:51:06 Asia/Shanghai）：

```
cl /nologo /std:c++17 /EHsc /O2 /W3 /LD /I code\capi /I code /I code\explore code\capi\fm_capi.cpp /FoMotionNative\ /FeMotionNative\fluidmotion.dll
```

## ③ exe / 托管 dll / fluidmotion.dll sha 对照

| 文件 | 非打包输出 `_z4_unpackaged_build` | 松散布局 `_z4_loose_layout` |
| --- | --- | --- |
| MotionSoftDemo.exe | `0c6d3a980b648ec8b8c439ce2f6dcc5baa8cee46224169e253b52fd54b828c4b`（163840） | 同 sha、同长度（apphost 两侧相同，预期） |
| MotionSoftDemo.dll（托管） | `1711f976692339878b59cac75d350d95d26e08a7f640c1c152f2889ff923fd28`（1314816） | `75c9b9e3dd0317a23e4da4e5a3c5443f88612215029e2bd13324c19b1283a46e`（1315328），不同 |
| fluidmotion.dll | `a77dcb62a108700d7d4d7f73eabe63a21d244e0b37da97ffc8f83ce1d4c823ea`（448000，来自 MotionNative，mtime 2026-11-11 08:51:06 Asia/Shanghai） | 同 sha、同长度 |

合格三条：托管 dll 哈希不同；非打包输出无 AppxManifest.xml / resources.pri；WindowsPackageType=None。

`z4_build_info.txt`（两侧 sha 均为 `c7169179725d0acb44833e6b8077d29af00b90a8e19d2a08075950709023f3d7`）：

```
tip=fe59c6229784ee7712926e3e0f94bcc0187f9069
composition_sha256=a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418
composition_sha256_raw=33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97
```