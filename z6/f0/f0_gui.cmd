@echo off
setlocal
rem Z6 F0: GUI devsmoke, MotionSoftDemo.exe --z3a-l1-dev (unpackaged). Arg 1 = Debug or Release.
rem Launched through a one-shot schtasks /IT /RL LIMITED task, which is deleted afterwards.
for %%I in ("%~dp0..\..") do set "W=%%~fI"
set "C=%~1"
set "L=%TEMP%\z6\f0"
set "MF_M0_ARTDIR=%L%\gui_%C%"
set MF_Z3A_L1_DEV=
set MF_Z4_L1_SESSION=
set FM_FLUIDMOTION_PATH=
>"%L%\launcher_%C%.log" echo LAUNCHER_START=%DATE% %TIME%
cd /d "%W%\_z6f0_build_%C%"
>>"%L%\launcher_%C%.log" echo RUNNING_EXE=_z6f0_build_%C%\MotionSoftDemo.exe --z3a-l1-dev
"%W%\_z6f0_build_%C%\MotionSoftDemo.exe" --z3a-l1-dev
>>"%L%\launcher_%C%.log" echo EXE_EXIT=%ERRORLEVEL%
>>"%L%\launcher_%C%.log" echo LAUNCHER_END=%DATE% %TIME%
