# Z4 r1 采集结果事实清单（K93N3P8 + IJI1KJG）

本文件只记录归档时从落盘文件（及一处只读包查询）核实过的事实。不计试跑，不纳入构建产物。不写结题判断，不写 adopt。Success≠adopt。PERF-CI 在已写出的 `verdict.json` 中均为 `HOLD`。

归档时间：2026-11-11 09:53 Australia/Perth（UTC+8）。归档机器：DESKTOP-K93N3P8，旁路仓 `C:\Users\admin\mf_z3a`。远程：`https://github.com/oops11111/motion_framework.git`，分支 `claude/z3a-cabi-l1`。归档开始时与 `git fetch` 后 `HEAD`、`origin/claude/z3a-cabi-l1`、`git ls-remote origin refs/heads/claude/z3a-cabi-l1` 均为 `fe59c6229784ee7712926e3e0f94bcc0187f9069`。已跟踪文件干净；未跟踪的 `MotionNative/`、`_z4_unpackaged_build/`、`_z4_unpackaged_obj/`、`_z4_unpackaged_out/` 未纳入本次提交。IJI 原目录 `C:\Users\Lh\mf_z3a\z4\results_etw_l1_smoke_r1\IJI1KJG\` 与 K93 原会话目录均未删除。

`.gitignore` 第 11 行仍为 `z4/results_etw_l1_smoke_r1/`，本次未改 ignore 规则；结果树用 `git add -f` 纳入。

## 1. tip、闸门、composition.h

| 项 | 值 | 来源 |
| --- | --- | --- |
| 采数 tip | `fe59c6229784ee7712926e3e0f94bcc0187f9069` | 各 PASS 会话 `env.json` / `session.log`；K93/IJI `git rev-parse HEAD`；K93 `s4`/`s5` 与 IJI `BUILD_NOTES.md` 中的 `z4_build_info.txt` 内容 |
| 闸门路径 | `docs/z4_etw_l1_smoke_gate_r1_LOCKED.md` | 各 PASS `verdict.json` 的 `gate` |
| GATE_BODY_SHA256 | `acb89d1b0cb975e5d63ae2c6fc47adcbc74dddd959b63089b2546b692cadab14` | 各 PASS `verdict.json` 的 `gate_body_sha256`、各 `session.log`；`docs/z4_etw_l1_smoke_r1_approved.md` 留证行。本清单未重新计算 GATE_BODY |
| composition 归一化（CRLF→LF） | `a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418`（27010 字节） | 见下 PRE/POST |
| composition raw（CRLF） | `33d826c77d99c3feb545b79f16a5a3fc9f5ac200716f24759dfcbf8e60196e97`（27533 字节） | 见下 PRE/POST |

composition.h PRE/POST（路径 `code/composition.h`，本票未改该文件）：

- K93 归档开始前（PRE）与写入本清单前（POST）：归一化、raw 哈希与长度同上。
- IJI 只读复查（`C:\Users\Lh\mf_z3a\code\composition.h`，HEAD 同为 `fe59c622…9069`）：归一化、raw 哈希与长度同上。
- 各 PASS 会话 `env.json` 的 `composition_sha256` / `composition_sha256_raw`、`session.log` 对应行，以及 K93 `s4`/`s5`、IJI `BUILD_NOTES.md` 所录 `z4_build_info.txt`，与上表一致。

## 2. 二进制 sha 对照（IJI 与 K93 不同）

数值来自各机 `BUILD_NOTES.md`。归档时对仍留在本地、且 sha 与 `BUILD_NOTES.md` 一致的构建输出做了只读复核（这些构建目录不纳入本次提交）。

| 文件 | K93 非打包 `_z4_unpackaged_build` | K93 松散布局 win-x64（InstallLocation） | IJI 非打包 `_z4_unpackaged_build` | IJI 松散布局 `_z4_loose_layout` |
| --- | --- | --- | --- | --- |
| MotionSoftDemo.exe | `50b00fef5aeb25d20570bb1358016987c21471d388ac12df2dd6e84f3b17fd82`（163840） | 同 sha、同长度（apphost 两侧相同） | `0c6d3a980b648ec8b8c439ce2f6dcc5baa8cee46224169e253b52fd54b828c4b`（163840） | 同 sha、同长度（apphost 两侧相同，预期） |
| MotionSoftDemo.dll（托管） | `03ffd553d2ef753f38a4931c7af1042a3bbf9431ea5d53513080bd94ab534c48`（1315328） | `94efcb509f01dc418625135bad5065a2eceb03747106f496ddd407117b5b322e`（1315328），不同 | `1711f976692339878b59cac75d350d95d26e08a7f640c1c152f2889ff923fd28`（1314816） | `75c9b9e3dd0317a23e4da4e5a3c5443f88612215029e2bd13324c19b1283a46e`（1315328），不同 |
| fluidmotion.dll | `0efac99e8326bfa705a35518ff148ba5a568ff2acfafddce7199e2e9de8e0dea`（438784） | 同 sha、同长度 | `a77dcb62a108700d7d4d7f73eabe63a21d244e0b37da97ffc8f83ce1d4c823ea`（448000） | 同 sha、同长度 |

正式会话文件未记录 `cl.exe` 版本字符串。对上述 sha 相符的 `fluidmotion.dll` 与 `MotionSoftDemo.exe` 读取 PE Optional Header 链接器版本，两机均为 `14.44`。尽管链接器主次版本戳相同，exe / 托管 dll / native dll 的 sha（以及 `fluidmotion.dll` 体积 438784 对 448000、托管非打包 dll 体积 1315328 对 1314816）在两机之间均不同，与各自本机工具链产出一致。K93 `s1`/`s3` 的 `INVALID.md` 所记 exe sha 为 `50b00fef…fd82`。

`z4_build_info.txt` 在 K93 两侧与 IJI 两侧的 sha 均为 `c7169179725d0acb44833e6b8077d29af00b90a8e19d2a08075950709023f3d7`（内容为上表 tip 与 composition 哈希）。

## 3. 构建偏差（独立 OutputPath、临时 props/targets）

均来自会话目录内 `BUILD_NOTES.md`。临时 props/targets 不进仓库。

K93 `s4`/`s5` 共用同一份独立非打包输出（`BUILD_NOTES.md` 两份哈希相同）：

- OutputPath=`C:\Users\admin\mf_z3a\_z4_unpackaged_build\`，BaseIntermediateOutputPath 与 MSBuildProjectExtensionsPath=`C:\Users\admin\mf_z3a\_z4_unpackaged_obj\`。未传 `Z4LooseLayout` / `Z4Package`；求值 `WindowsPackageType=None`。输出无 `AppxManifest.xml` / `resources.pri`。
- 临时导入：`%TEMP%\z4_formal_k93\exclude_default_obj.props` sha `2fcd67a76ec7751781c749251544b6a642bbb990e336b8289797c12dd2b826bf`（248 字节，无 BOM）；`exclude_default_obj.targets` sha `994fae0297e815e8f8739a8ac6c74d15c935eeb399736a4b4ff52164a35f2aed`（307 字节，无 BOM）。用途：排除工程默认 `obj\**`，避免与松散布局留下的生成代码重复定义。
- 构建时间：2026-11-11 07:01:34–07:02:03 Australia/Perth。`s2` 会话目录无 `BUILD_NOTES.md`；`s2` 的 `session.log` 启动 exe 为 `MotionSoftDemo\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\MotionSoftDemo.exe`。

IJI `s1`/`s3` 共用同一份非打包输出（两份 `BUILD_NOTES.md` 哈希相同）；`s2` 为独立松散布局：

- 非打包：OutputPath=`C:\Users\Lh\mf_z3a\_z4_unpackaged_build\`，中间目录 `_z4_unpackaged_obj\`。未传 `Z4LooseLayout` / `Z4Package`；求值 `WindowsPackageType=None`。输出无 `AppxManifest.xml` / `resources.pri`。
- 松散布局：OutputPath=`C:\Users\Lh\mf_z3a\_z4_loose_layout\`，中间目录 `_z4_loose_obj\`。参数含 `-p:Z4Package=true -p:Z4LooseLayout=true -p:AppxPackageSigningEnabled=false -p:GenerateAppxPackageOnBuild=false -p:AppxSymbolPackageEnabled=false`；求值 `WindowsPackageType=MSIX`。`Add-AppxPackage -Register C:\Users\Lh\mf_z3a\_z4_loose_layout\AppxManifest.xml`。`BUILD_NOTES.md` 写明未导入 LocalMachine 证书、未弹 UAC。
- 临时导入：`%TEMP%\z4_formal_iji\exclude_default_obj.props` sha `9e548c9cfdd1205c987585c4c4b1fe839574f33bf1ed16c3fda8e8575baf4836`（247 字节，无 BOM）；`exclude_default_obj.targets` sha `4b67a243c1b2f0a0ea3a494267b0d32b6a113eee1cf28e25a4aa721254254348`（306 字节，无 BOM）。`BUILD_NOTES.md` 记与 K93 同文等效，本机各少 1 字节行尾差异。
- IJI 构建时间按 `BUILD_NOTES.md` 所写为 2026-11-11 08:51–08:53 Asia/Shanghai（与 Australia/Perth 同为 UTC+8）。

合格三条（IJI `BUILD_NOTES.md` 与 K93 `s4`/`s5` `BUILD_NOTES.md`）：托管 dll 哈希与松散布局不同；非打包输出无 AppxManifest/resources.pri；`WindowsPackageType=None`。

## 4. 会话记录

`g2.2` 中 `probe_max_step_dip` 仅为诊断字段。`check_trace` 判据写在各 PASS `session.log` 尾部（`verdict=PASS`，`magic=ok provider=ok guid=absent_from_metadata`，六个事件名均为 ok），与 `verdict.json` 的 `g2.6_etw` 及 trace 字节数一致。K93 `%TEMP%\z4_formal_k93\s{2,4,5}_check_trace.log` 与对应 `session.log` 尾部一致，这些 TEMP 日志不纳入本次提交。

`runtime_matches_frozen` 在各 PASS 的 `verdict.json` / `env.json` 均为 `true`；`invalid_reasons` 均为 `[]`。G2①–⑥ 在各 PASS `verdict.json` 中均为 `pass: true`。打包会话的 `env.json` 保留 LocalState 原路径（`resolved_outdir`），`mirror_note=copied_to_requested_from_inside_package`。

### K93N3P8（OS：`Microsoft Windows NT 10.0.19045.0`，来自 PASS `env.json`）

**s1 — INVALID（非打包启动失败，会话未开始）。** 目录原仅有 `INVALID.md`。原因（摘自该文件）：启动的是松散布局 exe（该路径同时是已注册包 `MotionSoftDemo_1.0.0.0_x64__zb6e6ksq6rq3m` 的 InstallLocation），进程在 WinAppSDK `DeploymentManagerCS.AutoInitialize` 处抛出 `COMException (0x80040154)`（`REGDB_E_CLASSNOTREG`），于模块构造阶段退出，Z4 场景未启动。无 `verdict.json` / `session.log` / `env.json` / `trace.nettrace`。启动时间 2026-11-11 06:38:22.93 Australia/Perth；`EXE_EXIT=-532462766`（`0xE0434352`）；exe sha `50b00fef…fd82`。无 `check_trace`。无 packaged/SignatureKind 会话记录。归档时将仍仅存在于 `%TEMP%\z4_formal_k93\s1_crash\Report.wer` 的 WER 副本拷入本目录 `Report.wer`（sha `9a0ee9ef7c0da15dde42e17a095a30a0704809a9e7b28892e95ab6867b2d88d2`，16266 字节），并在 `INVALID.md` 追加归档备注；TEMP 原副本未删，哈希一致。`INVALID.md` 其余正文未改。

**s2 — PASS（松散注册打包）。** `env.json`：`packaged=true`，`package_full_name=MotionSoftDemo_1.0.0.0_x64__zb6e6ksq6rq3m`，`windows_package_type=msix`。本会话目录无 `BUILD_NOTES.md`，故 SignatureKind 不来自会话树。归档时只读查询 `Get-AppxPackage -Name MotionSoftDemo`：`SignatureKind=None`，`IsDevelopmentMode=True`，`PackageFullName=MotionSoftDemo_1.0.0.0_x64__zb6e6ksq6rq3m`，`InstallLocation=C:\Users\admin\mf_z3a\MotionSoftDemo\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64`，`Status=Ok`。g2.2：`max_frame_step_dip=59.556363960383294`，`probe_max_step_dip=59.556363960383294`（诊断），`frame_step_limit_dip=80`，`frame_step_fails=0`，`jumpFails=0`，`frame_steps_observed=54`，`max_frame_step_dt_ms=31.3512`。trace sha `bf0ba35a1bb66189c9c30c76fa2d9d531416158f3b3cf63988b102c4f1e56adc`（1986434 字节）。`session.log` 中 `check_trace`：`verdict=PASS`。

**s3 — INVALID（非打包启动失败，会话未开始）。** 目录原仅有 `INVALID.md`。原因（摘自该文件）：启动的仍是松散布局构建产物的拷贝（`C:\Users\admin\mf_z3a\_z4_unpackaged_out\`，从已注册 InstallLocation 的 win-x64 拷出并去掉清单），不是一次独立的 `WindowsPackageType=None` 非打包构建。同样在 `DeploymentManagerCS.AutoInitialize` 处 `COMException (0x80040154)`，场景未启动。无会话产物。启动时间 2026-11-11 06:41:26.19 Australia/Perth；exe sha 同为 `50b00fef…fd82`。归档时将 `%TEMP%\z4_formal_k93\s3_crash\Report.wer` 拷入本目录 `Report.wer`（sha `44a18610a4ae71c8da509cb4b7fe5e75c288eb611c4e7a0d11643657da796969`，15622 字节），并在 `INVALID.md` 追加归档备注；TEMP 原副本未删。`INVALID.md` 其余正文未改。

**s4 — PASS（独立非打包）。** `env.json`：`packaged=false`，`windows_package_type=unpackaged`。SignatureKind 不适用。g2.2：`max_frame_step_dip=58.75254321577532`，`probe_max_step_dip=100.52055126421062`（诊断），`frame_step_limit_dip=80`，`frame_step_fails=0`，`jumpFails=0`，`frame_steps_observed=54`，`max_frame_step_dt_ms=31.0939`。trace sha `a9624edeb577d31036d22f19f38210b049f6537605fb55c1af3b57eb164f6043`（1964906 字节）。`check_trace`：`verdict=PASS`。启动 exe：`C:\Users\admin\mf_z3a\_z4_unpackaged_build\MotionSoftDemo.exe`（见 `BUILD_NOTES.md` / `session.log`）。

**s5 — PASS（独立非打包，与 s4 同一份输出）。** `env.json`：`packaged=false`，`windows_package_type=unpackaged`。SignatureKind 不适用。g2.2：`max_frame_step_dip=59.30694151773841`，`probe_max_step_dip=79.32506072134615`（诊断），`frame_step_limit_dip=80`，`frame_step_fails=0`，`jumpFails=0`，`frame_steps_observed=51`，`max_frame_step_dt_ms=30.6259`。trace sha `9053117b55dae776e3a1c5d820ce3f3f965f4eeb9890e7138dfc74a5d5f0d006`（1956154 字节）。`check_trace`：`verdict=PASS`。

### IJI1KJG（OS：`Microsoft Windows NT 10.0.26300.0`，来自 PASS `env.json`）

从 IJI 原样拷入 K93 仓同一相对路径。中转 tar sha256 `53597fd5eea7bced21a80ed965084601487a5018dabf5f078726bdcbab003bd4`（6062080 字节）；清单 sha256 `d01081539b8ef1f09d53d9330ccca61b0af05e5e724bb89fbad8709296af59f4`。解压后 18 个文件与 IJI 本地 sha 全部 MATCH，无缺、无多、无不一致。

**s1 — PASS（非打包）。** `env.json`：`packaged=false`，`windows_package_type=unpackaged`。SignatureKind 不适用。g2.2：`max_frame_step_dip=11.401583976065893`，`probe_max_step_dip=63.552718670445216`（诊断），`frame_step_limit_dip=80`，`frame_step_fails=0`，`jumpFails=0`，`frame_steps_observed=311`，`max_frame_step_dt_ms=5.6892`。trace sha `64874df1b93b1ffbd7d7c131d625a6519185737790d284a8ab70ad902a0be330`（1930250 字节）。`check_trace`：`verdict=PASS`。

**s2 — PASS（松散注册打包）。** `env.json`：`packaged=true`，`package_full_name=MotionSoftDemo_1.0.0.0_x64__zb6e6ksq6rq3m`，`windows_package_type=msix`；`resolved_outdir` 为 package LocalState 原路径。`BUILD_NOTES.md`：`SignatureKind=None`（预期），`IsDevelopmentMode=True`，`InstallLocation=C:\Users\Lh\mf_z3a\_z4_loose_layout`。g2.2：`max_frame_step_dip=11.40885089835919`，`probe_max_step_dip=64.57578701379745`（诊断），`frame_step_limit_dip=80`，`frame_step_fails=0`，`jumpFails=0`，`frame_steps_observed=311`，`max_frame_step_dt_ms=5.6636`。trace sha `857fe5c80b78e52aaf7328f5dadbec25aa595e5fa8bb4b78521132e5174a8e32`（1958710 字节）。`check_trace`：`verdict=PASS`。

**s3 — PASS（非打包，与 s1 同一份输出）。** `env.json`：`packaged=false`，`windows_package_type=unpackaged`。SignatureKind 不适用。g2.2：`max_frame_step_dip=11.39727848613424`，`probe_max_step_dip=62.13105502537627`（诊断），`frame_step_limit_dip=80`，`frame_step_fails=0`，`jumpFails=0`，`frame_steps_observed=305`，`max_frame_step_dt_ms=5.6243`。trace sha `cf6416b87e92a8ed3d85a1f726016dd1d21bcca653f3aa7f178b068abfb082cd`（1929418 字节）。`check_trace`：`verdict=PASS`。

## 5. 纳入文件 sha256（归档写入本清单时）

相对路径相对于 `z4/results_etw_l1_smoke_r1/`。另加本清单 `docs/z4_etw_l1_smoke_r1_k93_iji_manifest.md`（其 sha 在落盘后记录于提交说明或可由 `git hash-object` 复核）。

```
f03df2ab498b6dc235eed13ea8f3030c53d4233d0f53811b54aa0a4bc58325e1         23 IJI1KJG/s1/bad_tokens.json
aa15d0c83f0693045b9e0b38b1b5e252948d15349ee67fe8db78778e5c6b6738       4039 IJI1KJG/s1/BUILD_NOTES.md
fab136d0e1d2b2122f77e3cddd3a5efbf90adba6c1587e30fad37119566cec56       1577 IJI1KJG/s1/env.json
45e09c0507b4999ca6cc7ac24cf16b74307bf07ffb952340c61c30986809b40e      58003 IJI1KJG/s1/session.log
64874df1b93b1ffbd7d7c131d625a6519185737790d284a8ab70ad902a0be330    1930250 IJI1KJG/s1/trace.nettrace
7cd18234583a686f58b29a4cf7c7e41800807704076a56cae509fa2b4b513337       2287 IJI1KJG/s1/verdict.json
f03df2ab498b6dc235eed13ea8f3030c53d4233d0f53811b54aa0a4bc58325e1         23 IJI1KJG/s2/bad_tokens.json
aa0e952498cc0ebb08f6ce208cb145e6bb3f21651b5e33b525cf79153b4491b9       3534 IJI1KJG/s2/BUILD_NOTES.md
1c710b4330a5fcc5b26e2ee5eda280cab128cd78efb6aab76a3367f9400fa32b       2135 IJI1KJG/s2/env.json
79f1dfb9bac92e9d0a04df5347228112273d22761496cea2d7d855a53241a1d9      58506 IJI1KJG/s2/session.log
857fe5c80b78e52aaf7328f5dadbec25aa595e5fa8bb4b78521132e5174a8e32    1958710 IJI1KJG/s2/trace.nettrace
06c5966d4f95146650677f582b7e1c5f6846c4e4029cb5affe8f5eedf8c657ec       2334 IJI1KJG/s2/verdict.json
f03df2ab498b6dc235eed13ea8f3030c53d4233d0f53811b54aa0a4bc58325e1         23 IJI1KJG/s3/bad_tokens.json
aa15d0c83f0693045b9e0b38b1b5e252948d15349ee67fe8db78778e5c6b6738       4039 IJI1KJG/s3/BUILD_NOTES.md
f76c315252de849ab130b8e057fc3348183256eb7202c45597d76e2bce892f3a       1576 IJI1KJG/s3/env.json
3dc197f967fd5e2569d6d21d4e33f336316e0780c7a107d5198cb1fe1393be64      57489 IJI1KJG/s3/session.log
cf6416b87e92a8ed3d85a1f726016dd1d21bcca653f3aa7f178b068abfb082cd    1929418 IJI1KJG/s3/trace.nettrace
9fe321549ad169f9b5a8b67798bf7b2ea0a3b7bb80340e5e8f189041dc886610       2274 IJI1KJG/s3/verdict.json
ad8869d3a14c07714700bb87833c99679052fd12cfd5f8e3753250dc57729c15       3563 K93N3P8/s1/INVALID.md
9a0ee9ef7c0da15dde42e17a095a30a0704809a9e7b28892e95ab6867b2d88d2      16266 K93N3P8/s1/Report.wer
f03df2ab498b6dc235eed13ea8f3030c53d4233d0f53811b54aa0a4bc58325e1         23 K93N3P8/s2/bad_tokens.json
09c04c68169d71cc4f34ff72371e707fdc238f47be0cafcbb161cb76cf5c66b0       2212 K93N3P8/s2/env.json
ce2dd52490266d1cb6f6c9e1d6d10bb6e008c3b578b7c915d8a966bedf452923      22456 K93N3P8/s2/session.log
bf0ba35a1bb66189c9c30c76fa2d9d531416158f3b3cf63988b102c4f1e56adc    1986434 K93N3P8/s2/trace.nettrace
bdd8721cee6cb23f5d888ac28b7b85ebdf724c3660c113ab54cd19edc0c1dc7f       2340 K93N3P8/s2/verdict.json
ed023354e3d8deec372de75c007b8a2e37831d58b3f1ba1fc34dc43718cd2547       3660 K93N3P8/s3/INVALID.md
44a18610a4ae71c8da509cb4b7fe5e75c288eb611c4e7a0d11643657da796969      15622 K93N3P8/s3/Report.wer
f03df2ab498b6dc235eed13ea8f3030c53d4233d0f53811b54aa0a4bc58325e1         23 K93N3P8/s4/bad_tokens.json
d3b6ba436a2e442c853d11d9568fa4e41eabdd5e3e350e2bad461da543b11061       4276 K93N3P8/s4/BUILD_NOTES.md
4f36530ca2d90f61b1875ee8c3e4f5d5b757b49f37fb52c03e37c531656233f0       1598 K93N3P8/s4/env.json
6c4f17a0a272342b89124ae32f2fdbdbe0a7568134de299abc4bd525ab8cf2f1      22039 K93N3P8/s4/session.log
a9624edeb577d31036d22f19f38210b049f6537605fb55c1af3b57eb164f6043    1964906 K93N3P8/s4/trace.nettrace
f9171a38d8b38f8adfb40970ef167915e95700e7042d23bd4723c412e70b330c       2290 K93N3P8/s4/verdict.json
f03df2ab498b6dc235eed13ea8f3030c53d4233d0f53811b54aa0a4bc58325e1         23 K93N3P8/s5/bad_tokens.json
d3b6ba436a2e442c853d11d9568fa4e41eabdd5e3e350e2bad461da543b11061       4276 K93N3P8/s5/BUILD_NOTES.md
57bfc0c1d739c00c44f1f470e7000697930b765545adee92d105192dafa1fa87       1598 K93N3P8/s5/env.json
bb0d2d71dd7db0cfbd1dbf546b228d9880534c7293f28df4ccbb288f7b43495a      21558 K93N3P8/s5/session.log
9053117b55dae776e3a1c5d820ce3f3f965f4eeb9890e7138dfc74a5d5f0d006    1956154 K93N3P8/s5/trace.nettrace
111cd2ca5743466e5dce99ae42c5c5aca3855c444d5b106f7402cb2a598ed95e       2290 K93N3P8/s5/verdict.json
```
