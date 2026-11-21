@echo off
setlocal EnableDelayedExpansion
rem Z6 F0: CheckSurface on the Release devsmoke assembly, using the Z5-T3 SurfaceDump tool source kept in the repo.
for %%I in ("%~dp0..\..") do set "W=%%~fI"
set "L=%TEMP%\z6\f0"
set "S=%W%\z5\t3_results_dev\K93N3P8"
if not exist "%L%\tool" mkdir "%L%\tool"
copy /y "%S%\SurfaceDump_Program.cs" "%L%\tool\Program.cs" > nul
copy /y "%S%\SurfaceDump.csproj.txt" "%L%\tool\SurfaceDump.csproj" > nul
>"%L%\surface.log" echo SURFACE_START=%DATE% %TIME%
dotnet build "%L%\tool\SurfaceDump.csproj" -c Release -o "%L%\tool\bin" > "%L%\surface_tool_build.txt" 2>&1
>>"%L%\surface.log" echo tool_build_exit=!ERRORLEVEL!
dotnet "%L%\tool\bin\SurfaceDump.dll" "%L%\ds_Release\z3a_l1_devsmoke.dll" "%L%\surface_ds_Release.txt" > "%L%\surface_ds_Release.stdout.txt" 2>&1
>>"%L%\surface.log" echo checksurface_exit=!ERRORLEVEL!
>>"%L%\surface.log" echo SURFACE_END=!DATE! !TIME!
