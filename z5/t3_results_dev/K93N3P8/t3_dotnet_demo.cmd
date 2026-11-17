@echo off
setlocal EnableDelayedExpansion
set W=C:\Users\admin\mf_z5\t3
set L=%TEMP%\z5_wave2\t3
cd /d "%W%"
for %%C in (Debug Release) do (
  >>"%L%\demo_build.log" echo %%C_RESTORE_START=!DATE! !TIME!
  dotnet restore MotionSoftDemo\MotionSoftDemo.csproj -p:Platform=x64 -p:Configuration=%%C -p:BaseIntermediateOutputPath=%W%\_z5t3_obj_%%C\ -p:MSBuildProjectExtensionsPath=%W%\_z5t3_obj_%%C\ > "%L%\demo_restore_%%C.txt" 2>&1
  >>"%L%\demo_build.log" echo %%C_RESTORE_EXIT=!ERRORLEVEL!
  dotnet build MotionSoftDemo\MotionSoftDemo.csproj -c %%C -p:Platform=x64 -p:OutputPath=%W%\_z5t3_build_%%C\ -p:BaseIntermediateOutputPath=%W%\_z5t3_obj_%%C\ -p:MSBuildProjectExtensionsPath=%W%\_z5t3_obj_%%C\ -p:CustomAfterMicrosoftCommonProps=%TEMP%\z5_wave1\t1\exclude_default_obj.props -p:CustomAfterMicrosoftCommonTargets=%TEMP%\z5_wave1\t1\exclude_default_obj.targets > "%L%\demo_build_%%C.txt" 2>&1
  >>"%L%\demo_build.log" echo %%C_BUILD_EXIT=!ERRORLEVEL! end=!TIME!
)
