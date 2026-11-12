# Z4 r1 K93N3P8 s3 — INVALID

本会话号 **s3 不复用**。本文件只记录操作问题导致的 INVALID，不改写本目录其它文件（本目录在崩溃后无会话产物）。

## 结论

**INVALID**（操作问题）。会话未开始，没有写出 `verdict.json` / `session.log` / `env.json` / `trace.nettrace`。

非打包会话启动的仍是松散布局构建产物的拷贝（`C:\Users\admin\mf_z3a\_z4_unpackaged_out\`，从已注册 InstallLocation 的 win-x64 拷出并去掉清单文件），不是一次独立的 `WindowsPackageType=None` 非打包构建。进程同样在 WinAppSDK `DeploymentManagerCS.AutoInitialize` 处抛出 `System.Runtime.InteropServices.COMException (0x80040154)`（没有注册类，`REGDB_E_CLASSNOTREG`），于模块构造阶段退出，Z4 场景未启动。

按裁决：本场记 INVALID 并保留现场；不重跑、不复用会话号。另开独立非打包构建后的新会话（s4 / s5）。

## 启动记录

- 时区：Australia/Perth（UTC+8）
- 启动时间：2026-11-11 06:41:26.19
- 崩溃时间：约 2026-11-11 06:41:26.46（.NET Runtime 事件）；Application Error 2026-11-11 06:41:26.64
- 启动器结束：2026-11-11 06:41:28.87
- pid：20076（事件日志 0x4e6c）；启动瞬间进程计数为 2
- EXE_EXIT：-532462766（0xE0434352，CLR 未处理异常）
- 启动命令：`MotionSoftDemo.exe --z4-l1-session C:\Users\admin\mf_z3a\z4\results_etw_l1_smoke_r1\K93N3P8\s3`
- exe 路径：`C:\Users\admin\mf_z3a\_z4_unpackaged_out\MotionSoftDemo.exe`
- exe sha256：`50b00fef5aeb25d20570bb1358016987c21471d388ac12df2dd6e84f3b17fd82`（与松散布局 exe 相同）
- 启动方式：`schtasks /Create /IT /RL LIMITED`，任务名 `WB_Z4_r1_K93_s3`，跑完已删

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

- 原路径：`C:\ProgramData\Microsoft\Windows\WER\ReportArchive\AppCrash_MotionSoftDemo.e_28aabd5ea7dd4e7e274a1b77a4dad92b63b5a720_483570cb_db137495-e52d-41b1-a923-d30f563b92d2`
- 副本：`C:\Users\admin\AppData\Local\Temp\z4_formal_k93\s3_crash`


## 归档备注（2026-11-11 Australia/Perth）

归档时发现 WER 副本仍只在 %TEMP%\\z4_formal_k93\\s3_crash\\Report.wer，尚未进入本会话目录。已将 Report.wer 拷入本目录（sha256=44a18610a4ae71c8da509cb4b7fe5e75c288eb611c4e7a0d11643657da796969，15622 字节）。TEMP 原副本未删，哈希一致。本文件其余正文未改。
