@echo off
setlocal
call "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvars64.bat" >nul
set CAPI=C:\Users\admin\motion_framework\code\capi
cd /d C:\Users\admin\motion_framework\MotionNative
cl 2>&1 | findstr /r "19\.[0-9]"
echo ==== BUILD fluidmotion.dll (/MT static CRT) ====
cl /nologo /std:c++17 /EHsc /O2 /W3 /MT /LD /I%CAPI% /I%CAPI%\.. /I%CAPI%\..\explore %CAPI%\fm_capi.cpp /Fe:fluidmotion.dll
echo cl_exit=%ERRORLEVEL%
dumpbin /nologo /exports fluidmotion.dll | findstr "fm_"
echo ==== DLL dependents (expect no VCRUNTIME140/MSVCP140) ====
dumpbin /nologo /dependents fluidmotion.dll | findstr /i ".dll"
echo ==== BUILD+RUN fm_capi_test (cl, /MT) ====
if not exist capitest mkdir capitest
cl /nologo /std:c++17 /EHsc /O2 /W3 /MT /I%CAPI% /I%CAPI%\.. /I%CAPI%\..\explore %CAPI%\fm_capi.cpp %CAPI%\fm_capi_test.cpp /Fo:capitest\ /Fe:fm_capi_test.exe
echo cl_exit=%ERRORLEVEL%
pushd %CAPI%
C:\Users\admin\motion_framework\MotionNative\fm_capi_test.exe
echo run_exit=%ERRORLEVEL%
popd
echo ==== BUILD+RUN continuity_test ====
cl /nologo /std:c++17 /EHsc /O2 /W3 /MT /I%CAPI% continuity_test.cpp fluidmotion.lib /Fe:continuity_test.exe
echo cl_exit=%ERRORLEVEL%
continuity_test.exe
echo run_exit=%ERRORLEVEL%
