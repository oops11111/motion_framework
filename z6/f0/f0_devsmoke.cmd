@echo off
setlocal EnableDelayedExpansion
rem Z6 F0: run the console devsmoke (z3a_l1_devsmoke) built by f0_build.cmd, Debug then Release.
set "L=%TEMP%\z6\f0"
set FM_FLUIDMOTION_PATH=
set MF_Z3A_L1_DEV=
set MF_Z4_L1_SESSION=
>"%L%\devsmoke_run.log" echo RUN_START=%DATE% %TIME%
for %%C in (Debug Release) do (
  pushd "%L%\ds_%%C"
  "%L%\ds_%%C\z3a_l1_devsmoke.exe" > "%L%\devsmoke_console_%%C.txt" 2>&1
  >>"%L%\devsmoke_run.log" echo devsmoke_console_%%C_exit=!ERRORLEVEL!
  popd
)
>>"%L%\devsmoke_run.log" echo RUN_END=!DATE! !TIME!
