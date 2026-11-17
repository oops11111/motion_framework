@echo off
setlocal EnableDelayedExpansion
set W=C:\Users\admin\mf_z5\t3
set B0=C:\Users\admin\mf_z5\t1
set L=%TEMP%\z5_wave2\t3
set T=%TEMP%\z5_wave2\tool
if not exist "%L%" mkdir "%L%"
>"%L%\build.log" echo BUILD_START=%DATE% %TIME%
call "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvars64.bat" > "%L%\vcvars64_out.txt" 2>&1
>>"%L%\build.log" echo VCVARS_EXIT=%ERRORLEVEL%
cd /d "%W%"
if not exist MotionNative mkdir MotionNative
cl /nologo /std:c++17 /EHsc /O2 /W3 /LD /I code\capi /I code /I code\explore code\capi\fm_capi.cpp /Fo:MotionNative\ /Fe:MotionNative\fluidmotion.dll > "%L%\cl_fluidmotion.txt" 2>&1
>>"%L%\build.log" echo fluidmotion_cl_exit=%ERRORLEVEL%
if not exist "%L%\capi_release" mkdir "%L%\capi_release"
if not exist "%L%\capi_debug" mkdir "%L%\capi_debug"
cl /nologo /std:c++17 /EHsc /O2 /DNDEBUG /MT /W3 /I code\capi /I code /I code\explore code\capi\fm_capi_test.cpp code\capi\fm_capi.cpp /Fo:"%L%\capi_release\\" /Fe:"%L%\capi_release\fm_capi_test.exe" > "%L%\cl_fm_capi_test_release.txt" 2>&1
>>"%L%\build.log" echo fm_capi_test_release_cl_exit=%ERRORLEVEL%
cl /nologo /std:c++17 /EHsc /Od /Zi /RTC1 /D_DEBUG /MTd /W3 /I code\capi /I code /I code\explore code\capi\fm_capi_test.cpp code\capi\fm_capi.cpp /Fo:"%L%\capi_debug\\" /Fd:"%L%\capi_debug\\" /Fe:"%L%\capi_debug\fm_capi_test.exe" > "%L%\cl_fm_capi_test_debug.txt" 2>&1
>>"%L%\build.log" echo fm_capi_test_debug_cl_exit=%ERRORLEVEL%
"%L%\capi_release\fm_capi_test.exe" code\capi\demo_motion.json > "%L%\fm_capi_test_release_output.txt" 2>&1
>>"%L%\build.log" echo fm_capi_test_release_run_exit=%ERRORLEVEL%
"%L%\capi_debug\fm_capi_test.exe" code\capi\demo_motion.json > "%L%\fm_capi_test_debug_output.txt" 2>&1
>>"%L%\build.log" echo fm_capi_test_debug_run_exit=%ERRORLEVEL%
for %%C in (Debug Release) do (
  dotnet build "%W%\MotionSoftDemo\z3a_l1_devsmoke\Z3aL1DevSmoke.csproj" -c %%C -o "%L%\ds_%%C" -p:BaseIntermediateOutputPath=%L%\ds_obj_%%C\ -p:MSBuildProjectExtensionsPath=%L%\ds_obj_%%C\ > "%L%\dotnet_devsmoke_%%C.txt" 2>&1
  >>"%L%\build.log" echo devsmoke_%%C_build_exit=!ERRORLEVEL!
  dotnet build "%B0%\MotionSoftDemo\z3a_l1_devsmoke\Z3aL1DevSmoke.csproj" -c %%C -o "%L%\base_ds_%%C" -p:BaseIntermediateOutputPath=%L%\base_ds_obj_%%C\ -p:MSBuildProjectExtensionsPath=%L%\base_ds_obj_%%C\ > "%L%\dotnet_base_devsmoke_%%C.txt" 2>&1
  >>"%L%\build.log" echo base_devsmoke_%%C_build_exit=!ERRORLEVEL!
  copy /y "%W%\MotionNative\fluidmotion.dll" "%L%\ds_%%C\" > nul
  copy /y "%W%\MotionNative\fluidmotion.dll" "%L%\base_ds_%%C\" > nul
)
>>"%L%\build.log" echo BUILD_END=%DATE% %TIME%
