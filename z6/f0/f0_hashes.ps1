# Z6 F0: sha256 of build artifacts (paths written relative to the worktree or %TEMP%).
param([Parameter(Mandatory=$true)][string]$Out)
$w = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$L = Join-Path $env:TEMP 'z6\f0'
$items = @(
  @($w, 'MotionNative\fluidmotion.dll', '<worktree>'),
  @($L, 'capi_release\fm_capi_test.exe', '%TEMP%\z6\f0'),
  @($L, 'capi_debug\fm_capi_test.exe', '%TEMP%\z6\f0'),
  @($L, 'ds_Debug\z3a_l1_devsmoke.dll', '%TEMP%\z6\f0'),
  @($L, 'ds_Debug\fluidmotion.dll', '%TEMP%\z6\f0'),
  @($L, 'ds_Release\z3a_l1_devsmoke.dll', '%TEMP%\z6\f0'),
  @($L, 'ds_Release\fluidmotion.dll', '%TEMP%\z6\f0'),
  @($w, '_z6f0_build_Debug\MotionSoftDemo.exe', '<worktree>'),
  @($w, '_z6f0_build_Debug\MotionSoftDemo.dll', '<worktree>'),
  @($w, '_z6f0_build_Debug\z4_build_info.txt', '<worktree>'),
  @($w, '_z6f0_build_Release\MotionSoftDemo.exe', '<worktree>'),
  @($w, '_z6f0_build_Release\MotionSoftDemo.dll', '<worktree>'),
  @($w, '_z6f0_build_Release\z4_build_info.txt', '<worktree>'),
  @($L, 'tool\bin\SurfaceDump.dll', '%TEMP%\z6\f0'))
$lines = @("# sha256  size  path   (HEAD $((& git -C $w rev-parse HEAD).Trim()))")
foreach ($i in $items) { $p = Join-Path $i[0] $i[1]; if (Test-Path $p) { $lines += "$((Get-FileHash $p -Algorithm SHA256).Hash.ToLower())  $((Get-Item $p).Length)  $($i[2])\$($i[1])" } else { $lines += "MISSING  $($i[2])\$($i[1])" } }
foreach ($c in 'Debug','Release') { $p = Join-Path $w "_z6f0_build_$c\z4_build_info.txt"; if (Test-Path $p) { $lines += "## _z6f0_build_$c\z4_build_info.txt"; $lines += (Get-Content $p) } }
[IO.File]::WriteAllText($Out, (($lines -join "`n") + "`n"), (New-Object Text.UTF8Encoding($false)))
$lines
