param([int]$MaxWaitSec = 240)
$ErrorActionPreference = 'Continue'
$artRoot = 'C:\Users\admin\motion_framework\artifacts'
$demo = 'C:\Users\admin\motion_framework\MotionSoftDemo'
$runLog = Join-Path $artRoot 'm2_h5_seed_run.log'
$stopMark = 'M2-H5-SEED PROBE DONE'
$capRaw = Join-Path $artRoot 'm2_h5_seed_capture_raw.txt'
$capSum = Join-Path $artRoot 'm2_h5_seed_capture_summary.txt'
$sessFile = Join-Path $artRoot 'm2_h5_seed_session.txt'
$hashExpected = 'a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418'

function Stop-MotionSoftDemo {
  Get-Process -Name MotionSoftDemo -ErrorAction SilentlyContinue | ForEach-Object {
    try { $_.CloseMainWindow() | Out-Null } catch {}
  }
  Start-Sleep -Seconds 2
  Get-Process -Name MotionSoftDemo -ErrorAction SilentlyContinue | ForEach-Object {
    try { Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue } catch {}
  }
  Start-Sleep -Seconds 2
  -not (Get-Process -Name MotionSoftDemo -ErrorAction SilentlyContinue)
}

"===== M2-H5-SEED RUN $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') =====" | Tee-Object $runLog
$pre = (Get-FileHash 'C:\Users\admin\motion_framework\code\composition.h' -Algorithm SHA256).Hash.ToLower()
"composition.h PRE=$pre" | Tee-Object $runLog -Append
if ($pre -ne $hashExpected) {
  "INVALID composition.h hash mismatch" | Tee-Object $runLog -Append
  exit 2
}

Set-Content (Join-Path $artRoot 'm2_h5_seed.flag') '1' -Encoding ascii
$env:WINAPP_CLI_TELEMETRY_OPTOUT = '1'
$env:MF_M2_H5_SEED = '1'
Remove-Item Env:MF_M201_NOISE -ErrorAction SilentlyContinue
Remove-Item Env:MF_R350_PROBE -ErrorAction SilentlyContinue
Remove-Item Env:MF_R350_FULL -ErrorAction SilentlyContinue
Remove-Item Env:MF_R2_P4_PROBE -ErrorAction SilentlyContinue
Remove-Item (Join-Path $artRoot 'm2_01_noise.flag') -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $artRoot 'r350_probe.flag') -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $artRoot 'r350_full.flag') -Force -ErrorAction SilentlyContinue

$passN = 0
for ($s = 1; $s -le 3; $s++) {
  "----- SESSION $s START $(Get-Date -Format 'HH:mm:ss') -----" | Tee-Object $runLog -Append
  "stop clean: $(Stop-MotionSoftDemo)" | Tee-Object $runLog -Append
  Set-Content $sessFile "$s" -Encoding ascii
  $env:MF_M2_H5_SEED_SESSION = "$s"
  $raw = Join-Path $artRoot ("m2_h5_seed_s{0}_raw.txt" -f $s)
  $sum = Join-Path $artRoot ("m2_h5_seed_s{0}_summary.txt" -f $s)
  Remove-Item $raw,$sum,$capRaw,$capSum -Force -ErrorAction SilentlyContinue
  $stdout = Join-Path $artRoot ("m2_h5_seed_s{0}_stdout.txt" -f $s)
  $stderr = Join-Path $artRoot ("m2_h5_seed_s{0}_stderr.txt" -f $s)
  $proc = Start-Process -FilePath 'dotnet' -ArgumentList @('run','-c','Debug','-p:Platform=x64','--no-build') -WorkingDirectory $demo -PassThru -WindowStyle Hidden -RedirectStandardOutput $stdout -RedirectStandardError $stderr
  "dotnetPID=$($proc.Id) sessionFile=$s" | Tee-Object $runLog -Append
  $deadline = (Get-Date).AddSeconds($MaxWaitSec)
  $saw = $false
  $appPid = $null
  while ((Get-Date) -lt $deadline) {
    Start-Sleep -Seconds 2
    if (-not $appPid) {
      $p = Get-Process -Name MotionSoftDemo -ErrorAction SilentlyContinue | Select-Object -First 1
      if ($p) { $appPid = $p.Id; "appPID=$appPid" | Tee-Object $runLog -Append }
    }
    $watch = $null
    if (Test-Path $capRaw) { $watch = $capRaw }
    elseif (Test-Path $raw) { $watch = $raw }
    if ($watch) {
      $tail = Get-Content $watch -Raw -ErrorAction SilentlyContinue
      if ($tail -match [regex]::Escape($stopMark)) {
        $saw = $true
        "SAW $stopMark sess=$s via=$(Split-Path $watch -Leaf) at $(Get-Date -Format 'HH:mm:ss')" | Tee-Object $runLog -Append
        Start-Sleep -Seconds 2
        break
      }
    }
    if ($appPid -and -not (Get-Process -Id $appPid -ErrorAction SilentlyContinue)) {
      "app exited early appPID=$appPid" | Tee-Object $runLog -Append
      break
    }
  }
  "stop after: $(Stop-MotionSoftDemo)" | Tee-Object $runLog -Append
  if ((Test-Path $capRaw) -and -not (Test-Path $raw)) { Copy-Item $capRaw $raw -Force }
  if ((Test-Path $capSum) -and -not (Test-Path $sum)) { Copy-Item $capSum $sum -Force }
  if (Test-Path $sum) {
    Get-Content $sum | Tee-Object $runLog -Append
    $body = Get-Content $sum -Raw
    if ($body -match 'SESSION_PASS_CANDIDATE') { $passN++ }
  } else {
    "MISSING summary sess=$s saw=$saw" | Tee-Object $runLog -Append
  }
  "----- SESSION $s END -----" | Tee-Object $runLog -Append
  Start-Sleep -Seconds 3
}

Remove-Item (Join-Path $artRoot 'm2_h5_seed.flag') -Force -ErrorAction SilentlyContinue
Remove-Item $sessFile -Force -ErrorAction SilentlyContinue
Remove-Item Env:MF_M2_H5_SEED -ErrorAction SilentlyContinue
Remove-Item Env:MF_M2_H5_SEED_SESSION -ErrorAction SilentlyContinue
$post = (Get-FileHash 'C:\Users\admin\motion_framework\code\composition.h' -Algorithm SHA256).Hash.ToLower()
"composition.h POST=$post" | Tee-Object $runLog -Append
"PRE_EQ_POST=$($pre -eq $post)" | Tee-Object $runLog -Append
"G2_SESSION_PASS_CANDIDATES=$passN/3" | Tee-Object $runLog -Append
"===== M2-H5-SEED RUN END $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') =====" | Tee-Object $runLog -Append
if ($passN -lt 3) { exit 3 } else { exit 0 }
