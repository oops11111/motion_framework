# Z4 r1 K93N3P8 s1 — INVALID

本会话号 **s1 不复用**。本文件只记录操作问题导致的 INVALID，不改写本目录其它文件（本目录在崩溃后无会话产物）。

## 结论

**INVALID**（操作问题）。会话未开始，没有写出 `verdict.json` / `session.log` / `env.json` / `trace.nettrace`。

非打包会话启动的是松散布局 exe（该路径同时是已注册包 `MotionSoftDemo_1.0.0.0_x64__zb6e6ksq6rq3m` 的 InstallLocation）。进程在 WinAppSDK `DeploymentManagerCS.AutoInitialize` 处抛出 `System.Runtime.InteropServices.COMException (0x80040154)`（没有注册类，`REGDB_E_CLASSNOTREG`），于模块构造阶段退出，Z4 场景未启动。

按裁决：本场记 INVALID 并保留现场；不重跑、不复用会话号。另开独立非打包构建后的新会话（s4 / s5）。

## 启动记录

- 时区：Australia/Perth（UTC+8）
- 启动时间：2026-11-11 06:38:22.93
- 崩溃时间：约 2026-11-11 06:38:23.10（.NET Runtime 事件）；Application Error 2026-11-11 06:38:23.30
- 启动器结束：2026-11-11 06:38:25.69
- pid：42584（事件日志 0xa658）；启动瞬间还观察到 39472
- EXE_EXIT：-532462766（0xE0434352，CLR 未处理异常）
- 启动命令：`MotionSoftDemo.exe --z4-l1-session C:\Users\admin\mf_z3a\z4\results_etw_l1_smoke_r1\K93N3P8\s1`
- exe 路径：`C:\Users\admin\mf_z3a\MotionSoftDemo\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\MotionSoftDemo.exe`
- exe sha256：`50b00fef5aeb25d20570bb1358016987c21471d388ac12df2dd6e84f3b17fd82`
- 启动方式：`schtasks /Create /IT /RL LIMITED`，任务名 `WB_Z4_r1_K93_s1`，跑完已删

## 异常

.NET Runtime 1026：

```
System.Runtime.InteropServices.COMException (0x80040154): 没有注册类 (0x80040154 (REGDB_E_CLASSNOTREG))
   at System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Int32 errorCode)
   at WinRT.ActivationFactory.Get(String typeName)
   at Microsoft.Windows.ApplicationModel.WindowsAppRuntime.DeploymentInitializeOptions.get__objRef_global__Microsoft_Windows_ApplicationModel_WindowsAppRuntime_DeploymentInitializeOptions()
   at Microsoft.Windows.ApplicationModel.WindowsAppRuntime.DeploymentInitializeOptions..ctor()
   at Microsoft.Windows.ApplicationModel.WindowsAppRuntime.DeploymentManagerCS.AutoInitialize.get_Options() in C:\Users\admin\.nuget\packages\microsoft.windowsappsdk.foundation\2.3.12\include\DeploymentManagerAutoInitializer.cs:line 44
   at Microsoft.Windows.ApplicationModel.WindowsAppRuntime.DeploymentManagerCS.AutoInitialize.AccessWindowsAppSDK() in C:\Users\admin\.nuget\packages\microsoft.windowsappsdk.foundation\2.3.12\include\DeploymentManagerAutoInitializer.cs:line 30
   at Microsoft.Windows.ApplicationModel.WindowsAppRuntime.Common.AutoInitialize.InitializeWindowsAppSDK() in C:\Users\admin\.nuget\packages\microsoft.windowsappsdk.foundation\2.3.12\include\WindowsAppRuntimeAutoInitializer.cs:line 22
   at .cctor()
```

## WER

- 原路径：`C:\ProgramData\Microsoft\Windows\WER\ReportArchive\AppCrash_MotionSoftDemo.e_28aabd5ea7dd4e7e274a1b77a4dad92b63b5a720_483570cb_25a28fa1-0695-4254-9557-b5e82b298732`
- 副本：`C:\Users\admin\AppData\Local\Temp\z4_formal_k93\s1_crash`


## 归档备注（2026-11-11 Australia/Perth）

归档时发现 WER 副本仍只在 %TEMP%\\z4_formal_k93\\s1_crash\\Report.wer，尚未进入本会话目录。已将 Report.wer 拷入本目录（sha256=9a0ee9ef7c0da15dde42e17a095a30a0704809a9e7b28892e95ab6867b2d88d2，16266 字节）。TEMP 原副本未删，哈希一致。本文件其余正文未改。
