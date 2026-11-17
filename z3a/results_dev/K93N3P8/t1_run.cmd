@echo off
setlocal
set L=%TEMP%\z5_wave1\t1
set MF_M0_ARTDIR=%TEMP%\z5_wave1\t1\gui_logs
set MF_Z3A_L1_DEV=
set MF_Z4_L1_SESSION=
>"%L%\launcher.log" echo LAUNCHER_START=%DATE% %TIME%
>>"%L%\launcher.log" echo USER=%USERNAME%
>>"%L%\launcher.log" echo SESSIONNAME=%SESSIONNAME%
cd /d C:\Users\admin\mf_z5\t1\_z5_unpackaged_build
>>"%L%\launcher.log" echo RUNNING_EXE=C:\Users\admin\mf_z5\t1\_z5_unpackaged_build\MotionSoftDemo.exe --z3a-l1-dev
"C:\Users\admin\mf_z5\t1\_z5_unpackaged_build\MotionSoftDemo.exe" --z3a-l1-dev
>>"%L%\launcher.log" echo EXE_EXIT=%ERRORLEVEL%
>>"%L%\launcher.log" echo LAUNCHER_END=%DATE% %TIME%
