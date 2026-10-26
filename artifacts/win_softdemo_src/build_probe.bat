@echo off
setlocal
call "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvars64.bat" >nul
cd /d C:\Users\admin\motion_framework\MotionNative
echo ==== BUILD mfprobe.dll ====
cl /nologo /std:c++17 /EHsc /O2 /W3 /LD /DNOMINMAX /DWIN32_LEAN_AND_MEAN /I..\code mf_probe.cpp /Fe:mfprobe.dll /link d3d11.lib dxgi.lib dwmapi.lib windowsapp.lib
echo cl_exit=%ERRORLEVEL%
dumpbin /nologo /exports mfprobe.dll | findstr "mf_"
echo ==== BUILD+RUN probe_cli ====
cl /nologo /std:c++17 /EHsc /O2 /W3 probe_cli.cpp mfprobe.lib /Fe:probe_cli.exe
echo cl_exit=%ERRORLEVEL%
probe_cli.exe
echo run_exit=%ERRORLEVEL%

