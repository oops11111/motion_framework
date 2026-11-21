@echo off
setlocal EnableDelayedExpansion
rem Z6 F7: Release rebuild of the L1 pilot pack on main 7e2feb0e (same commands as Z5-T5-0, see pilot/l1pack/SHA256SUMS header),
rem then CheckSurface + public-surface dump of the NEW pack DLL (Z5-T3 SurfaceDump tool source kept in the repo),
rem plus fm_capi_test Debug/Release and console devsmoke Debug/Release as a sanity run. No source file is edited.
rem Paths derive from this script's location and %TEMP%; nothing machine-specific is hard-coded.
for %%I in ("%~dp0..\..") do set "W=%%~fI"
set "L=%TEMP%\z6\f7"
set "VCVARS=%ProgramFiles(x86)%\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvars64.bat"
set "S=%W%\z5\t3_results_dev\K93N3P8"
set FM_FLUIDMOTION_PATH=
set MF_Z3A_L1_DEV=
set MF_Z4_L1_SESSION=
if not exist "%L%" mkdir "%L%"
>"%L%\build.log" echo BUILD_START=%DATE% %TIME%
>>"%L%\build.log" echo HEAD=
git -C "%W%" rev-parse HEAD >> "%L%\build.log" 2>&1
call "%VCVARS%" > "%L%\vcvars64_out.txt" 2>&1
>>"%L%\build.log" echo VCVARS_EXIT=!ERRORLEVEL! Platform=!Platform!
cd /d "%W%"
if not exist MotionNative mkdir MotionNative
cl /nologo /std:c++17 /EHsc /O2 /W3 /LD /I code\capi /I code /I code\explore code\capi\fm_capi.cpp /Fo:MotionNative\ /Fe:MotionNative\fluidmotion.dll > "%L%\cl_fluidmotion.txt" 2>&1
>>"%L%\build.log" echo fluidmotion_cl_exit=!ERRORLEVEL!
dotnet build pilot\l1pack\FluidMotion.L1.csproj -c Release --no-incremental > "%L%\pack_build_release.txt" 2>&1
>>"%L%\build.log" echo pack_build_release_exit=!ERRORLEVEL!
if not exist "%L%\tool" mkdir "%L%\tool"
copy /y "%S%\SurfaceDump_Program.cs" "%L%\tool\Program.cs" > nul
copy /y "%S%\SurfaceDump.csproj.txt" "%L%\tool\SurfaceDump.csproj" > nul
dotnet build "%L%\tool\SurfaceDump.csproj" -c Release -o "%L%\tool\bin" > "%L%\surface_tool_build.txt" 2>&1
>>"%L%\build.log" echo surface_tool_build_exit=!ERRORLEVEL!
dotnet "%L%\tool\bin\SurfaceDump.dll" "%W%\pilot\l1pack\bin\x64\Release\net8.0\FluidMotion.L1.dll" "%L%\surface_pack_release.txt" > "%L%\surface_pack_release.stdout.txt" 2>&1
>>"%L%\build.log" echo checksurface_pack_exit=!ERRORLEVEL!
if not exist "%L%\capi_release" mkdir "%L%\capi_release"
if not exist "%L%\capi_debug" mkdir "%L%\capi_debug"
cl /nologo /std:c++17 /EHsc /O2 /DNDEBUG /MT /W3 /I code\capi /I code /I code\explore code\capi\fm_capi_test.cpp code\capi\fm_capi.cpp /Fo:"%L%\capi_release\\" /Fe:"%L%\capi_release\fm_capi_test.exe" > "%L%\cl_fm_capi_test_release.txt" 2>&1
>>"%L%\build.log" echo fm_capi_test_release_cl_exit=!ERRORLEVEL!
cl /nologo /std:c++17 /EHsc /Od /Zi /RTC1 /D_DEBUG /MTd /W3 /I code\capi /I code /I code\explore code\capi\fm_capi_test.cpp code\capi\fm_capi.cpp /Fo:"%L%\capi_debug\\" /Fd:"%L%\capi_debug\\" /Fe:"%L%\capi_debug\fm_capi_test.exe" > "%L%\cl_fm_capi_test_debug.txt" 2>&1
>>"%L%\build.log" echo fm_capi_test_debug_cl_exit=!ERRORLEVEL!
"%L%\capi_release\fm_capi_test.exe" code\capi\demo_motion.json > "%L%\fm_capi_test_release_output.txt" 2>&1
>>"%L%\build.log" echo fm_capi_test_release_run_exit=!ERRORLEVEL!
"%L%\capi_debug\fm_capi_test.exe" code\capi\demo_motion.json > "%L%\fm_capi_test_debug_output.txt" 2>&1
>>"%L%\build.log" echo fm_capi_test_debug_run_exit=!ERRORLEVEL!
for %%C in (Debug Release) do (
  dotnet build "%W%\MotionSoftDemo\z3a_l1_devsmoke\Z3aL1DevSmoke.csproj" -c %%C --no-incremental -o "%L%\ds_%%C" -p:BaseIntermediateOutputPath=%L%\ds_obj_%%C\ -p:MSBuildProjectExtensionsPath=%L%\ds_obj_%%C\ > "%L%\dotnet_devsmoke_%%C.txt" 2>&1
  >>"%L%\build.log" echo devsmoke_%%C_build_exit=!ERRORLEVEL!
  copy /y "%W%\MotionNative\fluidmotion.dll" "%L%\ds_%%C\" > nul
  >>"%L%\build.log" echo devsmoke_%%C_copy_native_exit=!ERRORLEVEL!
  pushd "%L%\ds_%%C"
  "%L%\ds_%%C\z3a_l1_devsmoke.exe" > "%L%\devsmoke_console_%%C.txt" 2>&1
  >>"%L%\build.log" echo devsmoke_console_%%C_run_exit=!ERRORLEVEL!
  popd
)
>>"%L%\build.log" echo BUILD_END=!DATE! !TIME!
