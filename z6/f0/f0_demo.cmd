@echo off
setlocal EnableDelayedExpansion
rem Z6 F0: unpackaged MotionSoftDemo (WinUI) build, Debug and Release, --no-incremental.
rem Same flags as Z5-T3 t3_dotnet_demo.cmd; exclude_default_obj.props/.targets taken from the repo copy (Z5 T1).
for %%I in ("%~dp0..\..") do set "W=%%~fI"
set "L=%TEMP%\z6\f0"
set "XP=%W%\z3a\results_dev\K93N3P8\exclude_default_obj.props"
set "XT=%W%\z3a\results_dev\K93N3P8\exclude_default_obj.targets"
cd /d "%W%"
>"%L%\demo_build.log" echo DEMO_START=%DATE% %TIME%
for %%C in (Debug Release) do (
  >>"%L%\demo_build.log" echo %%C_RESTORE_START=!DATE! !TIME!
  dotnet restore MotionSoftDemo\MotionSoftDemo.csproj -p:Platform=x64 -p:Configuration=%%C -p:BaseIntermediateOutputPath=%W%\_z6f0_obj_%%C\ -p:MSBuildProjectExtensionsPath=%W%\_z6f0_obj_%%C\ > "%L%\demo_restore_%%C.txt" 2>&1
  >>"%L%\demo_build.log" echo %%C_RESTORE_EXIT=!ERRORLEVEL!
  dotnet build MotionSoftDemo\MotionSoftDemo.csproj -c %%C --no-incremental -p:Platform=x64 -p:OutputPath=%W%\_z6f0_build_%%C\ -p:BaseIntermediateOutputPath=%W%\_z6f0_obj_%%C\ -p:MSBuildProjectExtensionsPath=%W%\_z6f0_obj_%%C\ -p:CustomAfterMicrosoftCommonProps=%XP% -p:CustomAfterMicrosoftCommonTargets=%XT% > "%L%\demo_build_%%C.txt" 2>&1
  >>"%L%\demo_build.log" echo %%C_BUILD_EXIT=!ERRORLEVEL! end=!DATE! !TIME!
)
