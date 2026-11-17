@echo off
setlocal
set W=C:\Users\admin\mf_z5\t1
set L=%TEMP%\z5_wave1\t1
if not exist "%L%" mkdir "%L%"
>"%L%\native_build.log" echo BUILD_START=%DATE% %TIME%
call "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvars64.bat" > "%L%\vcvars64_out.txt" 2>&1
>>"%L%\native_build.log" echo VCVARS_EXIT=%ERRORLEVEL%
cd /d "%W%"
if not exist MotionNative mkdir MotionNative
cl /nologo /std:c++17 /EHsc /O2 /W3 /LD /I code\capi /I code /I code\explore code\capi\fm_capi.cpp /Fo:MotionNative\ /Fe:MotionNative\fluidmotion.dll > "%L%\cl_fluidmotion.txt" 2>&1
>>"%L%\native_build.log" echo fluidmotion_cl_exit=%ERRORLEVEL%
if not exist "%L%\capi_test" mkdir "%L%\capi_test"
cl /nologo /std:c++17 /EHsc /O2 /W3 /I code\capi /I code /I code\explore code\capi\fm_capi_test.cpp code\capi\fm_capi.cpp /Fo:"%L%\capi_test\\" /Fe:"%L%\capi_test\fm_capi_test.exe" > "%L%\cl_fm_capi_test.txt" 2>&1
>>"%L%\native_build.log" echo fm_capi_test_cl_exit=%ERRORLEVEL%
"%L%\capi_test\fm_capi_test.exe" code\capi\demo_motion.json > "%L%\fm_capi_test_output.txt" 2>&1
>>"%L%\native_build.log" echo fm_capi_test_run_exit=%ERRORLEVEL%
>>"%L%\native_build.log" echo BUILD_END=%DATE% %TIME%
