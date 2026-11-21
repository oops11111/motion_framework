@echo off
setlocal EnableDelayedExpansion
rem Z6 F3: build and run fm_capi_test Debug and Release (same flags as Z5-T3 t3_build.cmd).
rem Paths are derived from this script's location and %TEMP%; nothing machine-specific is hard-coded.
for %%I in ("%~dp0..\..") do set "W=%%~fI"
set "L=%TEMP%\z6\f3"
set "VCVARS=%ProgramFiles(x86)%\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvars64.bat"
if not exist "%L%" mkdir "%L%"
>"%L%\build.log" echo BUILD_START=%DATE% %TIME%
>>"%L%\build.log" echo HEAD=
git -C "%W%" rev-parse HEAD >> "%L%\build.log" 2>&1
call "%VCVARS%" > "%L%\vcvars64_out.txt" 2>&1
>>"%L%\build.log" echo VCVARS_EXIT=!ERRORLEVEL!
cd /d "%W%"
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
>>"%L%\build.log" echo BUILD_END=!DATE! !TIME!
