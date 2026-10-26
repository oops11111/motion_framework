param([int]$MaxWaitSec = 900)
$ErrorActionPreference = 'Continue'
$artRoot = 'C:\Users\admin\motion_framework\artifacts'
$demo = 'C:\Users\admin\motion_framework\MotionSoftDemo'
$runLog = Join-Path $artRoot 'm2_h5_denom_verify_run.log'
$stopMark = 'M2-H5-DENOM-VERIFY PROBE DONE'
$capRaw = Join-Path $artRoot 'm2_h5_denom_verify_capture_raw.txt'
$capSum = Join-Path $artRoot 'm2_h5_denom_verify_capture_summary.txt'
$sessFile = Join-Path $artRoot 'm2_h5_denom_verify_session.txt'
$phaseFile = Join-Path $artRoot 'm2_h5_denom_verify_phase.txt'
$frozenFile = Join-Path $artRoot 'm2_h5_denom_verify_calib_frozen.txt'
$hashExpected = 'a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418'
$matrix = Join-Path $artRoot 'm2_h5_denom_verify_session_matrix.txt'

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

"===== M2-H5-DENOM-VERIFY PHASE-SCORE RUN $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') Australia/Perth =====" | Tee-Object $runLog
if (-not (Test-Path $frozenFile)) { "INVALID missing frozen calib $frozenFile" | Tee-Object $runLog -Append; exit 3 }
$calibNew = (Get-Content $frozenFile -Raw).Trim()
"frozen_calib_new=$calibNew" | Tee-Object $runLog -Append
if ($calibNew -eq '0.7510829456' -or $calibNew -eq '0.7511') { "INVALID forbidden silent 0.7511" | Tee-Object $runLog -Append; exit 4 }
$compPath = 'C:\Users\admin\motion_framework\code\composition.h'
$preLine = Get-Content (Join-Path $artRoot 'm2_h5_denom_verify_composition_hash.txt') -ErrorAction SilentlyContinue | Select-Object -First 1
"composition hash file: $preLine" | Tee-Object $runLog -Append

Set-Content (Join-Path $artRoot 'm2_h5_denom_verify.flag') '1' -Encoding ascii
Set-Content $phaseFile 'score' -Encoding ascii
$env:WINAPP_CLI_TELEMETRY_OPTOUT = '1'
$env:MF_M2_H5_DENOM_VERIFY = '1'
$env:MF_M2_H5_DENOM_VERIFY_PHASE = 'score'
$env:MF_M2_H5_DENOM_VERIFY_CALIB = $calibNew
foreach ($e in @('MF_M2_H5_SEED','MF_M201_NOISE','MF_M2_02','MF_R350_PROBE','MF_R350_FULL','MF_R2_P4_PROBE','MF_M2_DELTA_V','MF_M2_DELTA_V_PRIME','MF_M2_WGC_SEED_ABS','MF_M2_WGC_SEED_CALIB','MF_M2_WGC_SEED_PATH_B_PRIME','MF_M2_WGC_SEED_PATH_A_PRIME')) {
  Remove-Item "Env:$e" -ErrorAction SilentlyContinue
}
foreach ($f in @('m2_h5_seed.flag','m2_01_noise.flag','m2_02.flag','r350_probe.flag','r350_full.flag','m2_delta_v.flag','m2_delta_v_prime.flag','m2_wgc_seed_abs.flag','m2_wgc_seed_calib.flag','m2_wgc_seed_path_b_prime.flag','m2_wgc_seed_path_a_prime.flag')) {
  Remove-Item (Join-Path $artRoot $f) -Force -ErrorAction SilentlyContinue
}

"" | Set-Content $matrix -Encoding ascii
$passN = 0
for ($s = 1; $s -le 3; $s++) {
  "----- SCORE SESSION $s START $(Get-Date -Format 'HH:mm:ss') -----" | Tee-Object $runLog -Append
  "stop clean: $(Stop-MotionSoftDemo)" | Tee-Object $runLog -Append
  Set-Content $sessFile "$s" -Encoding ascii
  $env:MF_M2_H5_DENOM_VERIFY_SESSION = "$s"
  $raw = Join-Path $artRoot ("m2_h5_denom_verify_s{0}_raw.txt" -f $s)
  $sum = Join-Path $artRoot ("m2_h5_denom_verify_s{0}_summary.txt" -f $s)
  Remove-Item $raw,$sum,$capRaw,$capSum -Force -ErrorAction SilentlyContinue
  $stdout = Join-Path $artRoot ("m2_h5_denom_verify_s{0}_stdout.txt" -f $s)
  $stderr = Join-Path $artRoot ("m2_h5_denom_verify_s{0}_stderr.txt" -f $s)
  $proc = Start-Process -FilePath 'dotnet' -ArgumentList @('run','-c','Debug','-p:Platform=x64','--no-build') -WorkingDirectory $demo -PassThru -WindowStyle Normal -RedirectStandardOutput $stdout -RedirectStandardError $stderr
  "dotnetPID=$($proc.Id) sessionFile=$s phase=score calib=$calibNew" | Tee-Object $runLog -Append
  $deadline = (Get-Date).AddSeconds($MaxWaitSec)
  $saw = $false
  $appPid = $null
  while ((Get-Date) -lt $deadline) {
    Start-Sleep -Seconds 3
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
  $verdict = 'MISSING'
  if (Test-Path $sum) {
    Get-Content $sum | Tee-Object $runLog -Append
    $body = Get-Content $sum -Raw
    if ($body -match 'SESSION_PASS_CANDIDATE') { $passN++; $verdict = 'PASS_CANDIDATE' }
    elseif ($body -match 'SESSION_FAIL_CANDIDATE') { $verdict = 'FAIL_CANDIDATE' }
    elseif ($body -match 'SESSION_INVALID') { $verdict = 'INVALID' }
    elseif ($body -match 'SESSION_INCONCLUSIVE') { $verdict = 'INCONCLUSIVE_CANDIDATE' }
  } else {
    "MISSING summary sess=$s saw=$saw" | Tee-Object $runLog -Append
  }
  $line = "score_sess=$s binary=MotionSoftDemo.exe machine=$env:COMPUTERNAME appPID=$appPid dotnetPID=$($proc.Id) saw=$saw verdict=$verdict calib=$calibNew"
  $line | Tee-Object $runLog -Append
  Add-Content $matrix $line
  "----- SCORE SESSION $s END -----" | Tee-Object $runLog -Append
  Start-Sleep -Seconds 3
}

Remove-Item (Join-Path $artRoot 'm2_h5_denom_verify.flag') -Force -ErrorAction SilentlyContinue
Remove-Item $sessFile -Force -ErrorAction SilentlyContinue
Remove-Item Env:MF_M2_H5_DENOM_VERIFY -ErrorAction SilentlyContinue
Remove-Item Env:MF_M2_H5_DENOM_VERIFY_SESSION -ErrorAction SilentlyContinue
Remove-Item Env:MF_M2_H5_DENOM_VERIFY_CALIB -ErrorAction SilentlyContinue
$post = [System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::Create().ComputeHash([System.IO.File]::ReadAllBytes('C:\Users\admin\motion_framework\code\composition.h'))).Replace('-','').ToLowerInvariant()
"composition.h POST=$post" | Tee-Object $runLog -Append
Add-Content (Join-Path $artRoot 'm2_h5_denom_verify_composition_hash.txt') "POST=$post"
"pass_sessions=$passN/3 POST=$post frozen_calib=$calibNew" | Tee-Object $runLog -Append
"===== M2-H5-DENOM-VERIFY PHASE-SCORE RUN DONE =====" | Tee-Object $runLog -Append
