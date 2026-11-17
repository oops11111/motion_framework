@echo off
setlocal
set L=%TEMP%\z5_wave2\t3
set MF_M0_ARTDIR=%TEMP%\z5_wave2\t3\gui_Debug
set MF_Z3A_L1_DEV=
set MF_Z4_L1_SESSION=
set FM_FLUIDMOTION_PATH=
>"%L%\launcher_Debug.log" echo LAUNCHER_START=%DATE% %TIME%
cd /d C:\Users\admin\mf_z5\t3\_z5t3_build_Debug
>>"%L%\launcher_Debug.log" echo RUNNING_EXE=C:\Users\admin\mf_z5\t3\_z5t3_build_Debug\MotionSoftDemo.exe --z3a-l1-dev
"C:\Users\admin\mf_z5\t3\_z5t3_build_Debug\MotionSoftDemo.exe" --z3a-l1-dev
>>"%L%\launcher_Debug.log" echo EXE_EXIT=%ERRORLEVEL%
>>"%L%\launcher_Debug.log" echo LAUNCHER_END=%DATE% %TIME%
