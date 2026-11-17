@echo off
setlocal
set W=C:\Users\admin\mf_z5\t1
set L=%TEMP%\z5_wave1\t1
cd /d "%W%"
>"%L%\dotnet_build.log" echo RESTORE_START=%DATE% %TIME%
dotnet restore MotionSoftDemo\MotionSoftDemo.csproj -p:Platform=x64 -p:BaseIntermediateOutputPath=%W%\_z5_unpackaged_obj\ -p:MSBuildProjectExtensionsPath=%W%\_z5_unpackaged_obj\ > "%L%\dotnet_restore_out.txt" 2>&1
>>"%L%\dotnet_build.log" echo RESTORE_EXIT=%ERRORLEVEL%
>>"%L%\dotnet_build.log" echo BUILD_START=%DATE% %TIME%
dotnet build MotionSoftDemo\MotionSoftDemo.csproj -c Debug -p:Platform=x64 -p:OutputPath=%W%\_z5_unpackaged_build\ -p:BaseIntermediateOutputPath=%W%\_z5_unpackaged_obj\ -p:MSBuildProjectExtensionsPath=%W%\_z5_unpackaged_obj\ -p:CustomAfterMicrosoftCommonProps=%L%\exclude_default_obj.props -p:CustomAfterMicrosoftCommonTargets=%L%\exclude_default_obj.targets > "%L%\dotnet_build_out.txt" 2>&1
>>"%L%\dotnet_build.log" echo BUILD_EXIT=%ERRORLEVEL%
>>"%L%\dotnet_build.log" echo BUILD_END=%DATE% %TIME%
