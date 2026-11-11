# Z4 r1 K93N3P8 非打包构建说明（s4 / s5 共用同一份输出）

本文件在会话结束后写入，不改写本目录既有会话产物（verdict.json / env.json / session.log / trace.nettrace / bad_tokens.json）。
本票未重新构建；沿用 2026-11-11 07:01:34–07:02:03（Australia/Perth, UTC+8）已成功的独立非打包构建。

## ① 临时 MSBuild 导入：排除工程默认 obj\**

改 `BaseIntermediateOutputPath` 后，SDK 的 `DefaultItemExcludes` 不再排除工程目录 `MotionSoftDemo\obj\**`，会把松散布局留下的 `*.g.i.cs` 编进本次非打包编译并报 CS0101/CS0111。因此构建时通过命令行属性导入仅存在于 `%TEMP%` 的临时 props/targets（不进仓库、不改 csproj / Directory.Build.*）。

- props：`C:\Users\admin\AppData\Local\Temp\z4_formal_k93\exclude_default_obj.props`
  - sha256：`2fcd67a76ec7751781c749251544b6a642bbb990e336b8289797c12dd2b826bf`
  - 字节：248
  - UTF-8 BOM：False
- targets：`C:\Users\admin\AppData\Local\Temp\z4_formal_k93\exclude_default_obj.targets`
  - sha256：`994fae0297e815e8f8739a8ac6c74d15c935eeb399736a4b4ff52164a35f2aed`
  - 字节：307
  - UTF-8 BOM：False

### exclude_default_obj.props 全文

```xml
<Project>
  <PropertyGroup>
    <DefaultItemExcludes>$(DefaultItemExcludes);obj\**;**\obj\**</DefaultItemExcludes>
  </PropertyGroup>
  <ItemGroup>
    <Compile Remove="obj\**" />
    <EmbeddedResource Remove="obj\**" />
  </ItemGroup>
</Project>

```

### exclude_default_obj.targets 全文

```xml
<Project>
  <Target Name="Z4RemoveDefaultObjCompile" BeforeTargets="BeforeCompile;CoreCompile;XamlMarkupCompilePass1">
    <ItemGroup>
      <Compile Remove="$([System.IO.Path]::GetFullPath('$(MSBuildProjectDirectory)\obj'))\**" />
      <Compile Remove="obj\**" />
    </ItemGroup>
  </Target>
</Project>

```

## ② 独立输出目录与完整命令

- OutputPath = `C:\Users\admin\mf_z3a\_z4_unpackaged_build\`
- BaseIntermediateOutputPath = `C:\Users\admin\mf_z3a\_z4_unpackaged_obj\`
- MSBuildProjectExtensionsPath = `C:\Users\admin\mf_z3a\_z4_unpackaged_obj\`
- 未传 `Z4LooseLayout` / `Z4Package`；求值 `WindowsPackageType=None`
- 未覆盖松散布局 `MotionSoftDemo\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64`，未改包注册

restore（exit 0）：

```
dotnet restore MotionSoftDemo\MotionSoftDemo.csproj -p:Platform=x64 -p:BaseIntermediateOutputPath=C:\Users\admin\mf_z3a\_z4_unpackaged_obj\ -p:MSBuildProjectExtensionsPath=C:\Users\admin\mf_z3a\_z4_unpackaged_obj\
```

build（exit 0，07:01:34.52–07:02:03.16）：

```
dotnet build MotionSoftDemo\MotionSoftDemo.csproj -c Debug -p:Platform=x64 -p:OutputPath=C:\Users\admin\mf_z3a\_z4_unpackaged_build\ -p:BaseIntermediateOutputPath=C:\Users\admin\mf_z3a\_z4_unpackaged_obj\ -p:MSBuildProjectExtensionsPath=C:\Users\admin\mf_z3a\_z4_unpackaged_obj\ -p:CustomAfterMicrosoftCommonProps=C:\Users\admin\AppData\Local\Temp\z4_formal_k93\exclude_default_obj.props -p:CustomAfterMicrosoftCommonTargets=C:\Users\admin\AppData\Local\Temp\z4_formal_k93\exclude_default_obj.targets
```

启动会话使用的 exe：`C:\Users\admin\mf_z3a\_z4_unpackaged_build\MotionSoftDemo.exe`
新输出无 `AppxManifest.xml` / `resources.pri`。

## ③ exe / 托管 dll / fluidmotion.dll sha 对照

| 文件 | 非打包输出 `_z4_unpackaged_build` | 松散布局 win-x64（InstallLocation） |
| --- | --- | --- |
| MotionSoftDemo.exe | `50b00fef5aeb25d20570bb1358016987c21471d388ac12df2dd6e84f3b17fd82`（163840） | 同 sha、同长度（apphost 两侧相同） |
| MotionSoftDemo.dll（托管） | `03ffd553d2ef753f38a4931c7af1042a3bbf9431ea5d53513080bd94ab534c48`（1315328） | `94efcb509f01dc418625135bad5065a2eceb03747106f496ddd407117b5b322e`（1315328），不同 |
| fluidmotion.dll | `0efac99e8326bfa705a35518ff148ba5a568ff2acfafddce7199e2e9de8e0dea`（438784，来自 MotionNative，mtime 2026-11-11 06:30:38） | 同 sha、同长度 |

`z4_build_info.txt`（两侧 sha 均为 `c7169179725d0acb44833e6b8077d29af00b90a8e19d2a08075950709023f3d7`）：

```
tip=fe59c6229784ee7712926e3e0f94bcc0187f9069
composition_sha256=a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418
composition_sha256_raw=33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97
```