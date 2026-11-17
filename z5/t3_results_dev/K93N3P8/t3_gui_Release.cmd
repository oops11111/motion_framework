@echo off
setlocal
set L=%TEMP%\z5_wave2\t3
set MF_M0_ARTDIR=%TEMP%\z5_wave2\t3\gui_Release
set MF_Z3A_L1_DEV=
set MF_Z4_L1_SESSION=
set FM_FLUIDMOTION_PATH=
>"%L%\launcher_Release.log" echo LAUNCHER_START=%DATE% %TIME%
cd /d C:\Users\admin\mf_z5\t3\_z5t3_build_Release
>>"%L%\launcher_Release.log" echo RUNNING_EXE=C:\Users\admin\mf_z5\t3\_z5t3_build_Release\MotionSoftDemo.exe --z3a-l1-dev
"C:\Users\admin\mf_z5\t3\_z5t3_build_Release\MotionSoftDemo.exe" --z3a-l1-dev
>>"%L%\launcher_Release.log" echo EXE_EXIT=%ERRORLEVEL%
>>"%L%\launcher_Release.log" echo LAUNCHER_END=%DATE% %TIME%
