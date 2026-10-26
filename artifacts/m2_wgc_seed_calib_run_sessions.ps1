param([int]$MaxWaitSec = 900)
$ErrorActionPreference = 'Continue'
$artRoot = 'C:\Users\admin\motion_framework\artifacts'
$demo = 'C:\Users\admin\motion_framework\MotionSoftDemo'
$runLog = Join-Path $artRoot 'm2_wgc_seed_calib_run.log'
$stopMark = 'M2-WGC-SEED-CALIB PROBE DONE'
$capRaw = Join-Path $artRoot 'm2_wgc_seed_calib_capture_raw.txt'
$capSum = Join-Path $artRoot 'm2_wgc_seed_calib_capture_summary.txt'
$sessFile = Join-Path $artRoot 'm2_wgc_seed_calib_session.txt'
$hashExpected = 'a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418'
$matrix = Join-Path $artRoot 'm2_wgc_seed_calib_session_matrix.txt'

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

"===== M2-WGC-SEED-CALIB RUN $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') Asia/Taipei =====" | Tee-Object $runLog
$compPath = 'C:\Users\admin\motion_framework\code\composition.h'
if (-not (Test-Path -LiteralPath $compPath)) {
  "INVALID composition.h missing at $compPath" | Tee-Object $runLog -Append
  exit 2
}
$pre = [System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::Create().ComputeHash([System.IO.File]::ReadAllBytes($compPath))).Replace('-','').ToLowerInvariant()
"composition.h PRE=$pre expected=$hashExpected" | Tee-Object $runLog -Append
Set-Content (Join-Path $artRoot 'm2_wgc_seed_calib_composition_hash.txt') "PRE=$pre" -Encoding ascii
if ($pre -ne $hashExpected) {
  "INVALID composition.h hash mismatch pre=$pre" | Tee-Object $runLog -Append
  exit 2
}
$prereg = Join-Path $artRoot 'm2_wgc_seed_calib_prereg.txt'
if (-not (Test-Path $prereg)) { "INVALID missing prereg" | Tee-Object $runLog -Append; exit 3 }
$preregHash = [System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::Create().ComputeHash([System.IO.File]::ReadAllBytes($prereg))).Replace('-','').ToLowerInvariant()
"prereg_hash=$preregHash" | Tee-Object $runLog -Append
Set-Content (Join-Path $artRoot 'm2_wgc_seed_calib_prereg_hash.txt') $preregHash -Encoding ascii

Set-Content (Join-Path $artRoot 'm2_wgc_seed_calib.flag') '1' -Encoding ascii
$env:WINAPP_CLI_TELEMETRY_OPTOUT = '1'
$env:MF_M2_WGC_SEED_CALIB = '1'
foreach ($e in @('MF_M2_H5_SEED','MF_M201_NOISE','MF_M2_02','MF_R350_PROBE','MF_R350_FULL','MF_R2_P4_PROBE','MF_M2_DELTA_V','MF_M2_DELTA_V_PRIME','MF_M2_WGC_SEED_ABS')) {
  Remove-Item "Env:$e" -ErrorAction SilentlyContinue
}
foreach ($f in @('m2_h5_seed.flag','m2_01_noise.flag','m2_02.flag','r350_probe.flag','r350_full.flag','m2_delta_v.flag','m2_delta_v_prime.flag','m2_wgc_seed_abs.flag')) {
  Remove-Item (Join-Path $artRoot $f) -Force -ErrorAction SilentlyContinue
}

"" | Set-Content $matrix -Encoding ascii
$passN = 0
for ($s = 1; $s -le 3; $s++) {
  "----- SESSION $s START $(Get-Date -Format 'HH:mm:ss') -----" | Tee-Object $runLog -Append
  "stop clean: $(Stop-MotionSoftDemo)" | Tee-Object $runLog -Append
  Set-Content $sessFile "$s" -Encoding ascii
  $env:MF_M2_WGC_SEED_CALIB_SESSION = "$s"
  $raw = Join-Path $artRoot ("m2_wgc_seed_calib_s{0}_raw.txt" -f $s)
  $sum = Join-Path $artRoot ("m2_wgc_seed_calib_s{0}_summary.txt" -f $s)
  Remove-Item $raw,$sum,$capRaw,$capSum -Force -ErrorAction SilentlyContinue
  $stdout = Join-Path $artRoot ("m2_wgc_seed_calib_s{0}_stdout.txt" -f $s)
  $stderr = Join-Path $artRoot ("m2_wgc_seed_calib_s{0}_stderr.txt" -f $s)
  $proc = Start-Process -FilePath 'dotnet' -ArgumentList @('run','-c','Debug','-p:Platform=x64','--no-build') -WorkingDirectory $demo -PassThru -WindowStyle Normal -RedirectStandardOutput $stdout -RedirectStandardError $stderr
  "dotnetPID=$($proc.Id) sessionFile=$s" | Tee-Object $runLog -Append
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
  $line = "sess=$s binary=MotionSoftDemo.exe machine=$env:COMPUTERNAME appPID=$appPid dotnetPID=$($proc.Id) saw=$saw verdict=$verdict"
  $line | Tee-Object $runLog -Append
  Add-Content $matrix $line
  "----- SESSION $s END -----" | Tee-Object $runLog -Append
  Start-Sleep -Seconds 3
}

Remove-Item (Join-Path $artRoot 'm2_wgc_seed_calib.flag') -Force -ErrorAction SilentlyContinue
Remove-Item $sessFile -Force -ErrorAction SilentlyContinue
Remove-Item Env:MF_M2_WGC_SEED_CALIB -ErrorAction SilentlyContinue
Remove-Item Env:MF_M2_WGC_SEED_CALIB_SESSION -ErrorAction SilentlyContinue
$post = [System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::Create().ComputeHash([System.IO.File]::ReadAllBytes('C:\Users\admin\motion_framework\code\composition.h'))).Replace('-','').ToLowerInvariant()
"composition.h POST=$post" | Tee-Object $runLog -Append
Add-Content (Join-Path $artRoot 'm2_wgc_seed_calib_composition_hash.txt') "POST=$post"
"pass_sessions=$passN/3 PRE=$pre POST=$post" | Tee-Object $runLog -Append
"===== M2-WGC-SEED-CALIB RUN DONE =====" | Tee-Object $runLog -Append
