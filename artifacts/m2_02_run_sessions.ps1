param([int]$MaxWaitSec = 180)
$ErrorActionPreference = 'Continue'
$artRoot = 'C:\Users\admin\motion_framework\artifacts'
$demo = 'C:\Users\admin\motion_framework\MotionSoftDemo'
$toolDir = Join-Path $artRoot 'm2_02_tools'
$runLog = Join-Path $artRoot 'm2_02_run.log'
$stopMark = 'M2-02 PROBE DONE'
$capRaw = Join-Path $artRoot 'm2_02_capture_raw.txt'
$capSum = Join-Path $artRoot 'm2_02_capture_summary.txt'
$sessFile = Join-Path $artRoot 'm2_02_session.txt'
$hashExpected = 'a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418'
$matrix = Join-Path $artRoot 'm2_02_session_matrix.txt'

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

"===== M2-02 RUN $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') AWST =====" | Tee-Object $runLog
$pre = (Get-FileHash 'C:\Users\admin\motion_framework\code\composition.h' -Algorithm SHA256).Hash.ToLower()
"composition.h PRE=$pre" | Tee-Object $runLog -Append
if ($pre -ne $hashExpected) {
  "INVALID composition.h hash mismatch" | Tee-Object $runLog -Append
  exit 2
}

Set-Content (Join-Path $artRoot 'm2_02.flag') '1' -Encoding ascii
$env:WINAPP_CLI_TELEMETRY_OPTOUT = '1'
$env:MF_M2_02 = '1'
Remove-Item Env:MF_M201_NOISE -ErrorAction SilentlyContinue
Remove-Item Env:MF_M2_H5_SEED -ErrorAction SilentlyContinue
Remove-Item Env:MF_R350_PROBE -ErrorAction SilentlyContinue
Remove-Item Env:MF_R350_FULL -ErrorAction SilentlyContinue
Remove-Item (Join-Path $artRoot 'm2_01_noise.flag') -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $artRoot 'm2_h5_seed.flag') -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $artRoot 'r350_probe.flag') -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $artRoot 'r350_full.flag') -Force -ErrorAction SilentlyContinue

"binary=MotionSoftDemo machine=DESKTOP-K93N3P8 provider=FluidMotion-Motion guid=8f3a2c1d-6b4e-4a91-9c7d-2e5f8a1b3c4d" | Set-Content $matrix -Encoding utf8
$passN = 0
$pids = @()
for ($s = 1; $s -le 3; $s++) {
  "----- SESSION $s START $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') -----" | Tee-Object $runLog -Append
  "stop clean: $(Stop-MotionSoftDemo)" | Tee-Object $runLog -Append
  Set-Content $sessFile "$s" -Encoding ascii
  $env:MF_M2_02_SESSION = "$s"
  $raw = Join-Path $artRoot ("m2_02_s{0}_raw.txt" -f $s)
  $sum = Join-Path $artRoot ("m2_02_s{0}_summary.txt" -f $s)
  $nettrace = Join-Path $artRoot ("m2_02_s{0}.nettrace" -f $s)
  Remove-Item $raw,$sum,$capRaw,$capSum,$nettrace -Force -ErrorAction SilentlyContinue
  $stdout = Join-Path $artRoot ("m2_02_s{0}_stdout.txt" -f $s)
  $stderr = Join-Path $artRoot ("m2_02_s{0}_stderr.txt" -f $s)
  $proc = Start-Process -FilePath 'dotnet' -ArgumentList @('run','-c','Debug','-p:Platform=x64','--no-build') -WorkingDirectory $demo -PassThru -WindowStyle Hidden -RedirectStandardOutput $stdout -RedirectStandardError $stderr
  "dotnetPID=$($proc.Id) sessionFile=$s" | Tee-Object $runLog -Append
  $deadline = (Get-Date).AddSeconds($MaxWaitSec)
  $saw = $false
  $appPid = $null
  $traceProc = $null
  while ((Get-Date) -lt $deadline) {
    Start-Sleep -Seconds 1
    if (-not $appPid) {
      $p = Get-Process -Name MotionSoftDemo -ErrorAction SilentlyContinue | Select-Object -First 1
      if ($p) {
        $appPid = $p.Id
        "appPID=$appPid" | Tee-Object $runLog -Append
        # Attach EventPipe before probe timer (~800ms) fires
        try {
          $traceOut = Join-Path $artRoot ("m2_02_s{0}_trace_stdout.txt" -f $s)
          $traceErr = Join-Path $artRoot ("m2_02_s{0}_trace_stderr.txt" -f $s)
          $traceProc = Start-Process -FilePath 'dotnet' -ArgumentList @('tool','run','dotnet-trace','--','collect','-p',"$appPid",'-o',"$nettrace",'--providers','FluidMotion-Motion:0xFFFFFFFF:5','--duration','00:00:00:40') -WorkingDirectory $toolDir -PassThru -WindowStyle Hidden -RedirectStandardOutput $traceOut -RedirectStandardError $traceErr
          "dotnet-tracePID=$($traceProc.Id) -> $nettrace" | Tee-Object $runLog -Append
        } catch {
          "dotnet-trace attach FAILED: $_" | Tee-Object $runLog -Append
        }
      }
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
  # Stop trace if still running
  if ($traceProc -and -not $traceProc.HasExited) {
    try { Stop-Process -Id $traceProc.Id -Force -ErrorAction SilentlyContinue } catch {}
  }
  "stop after: $(Stop-MotionSoftDemo)" | Tee-Object $runLog -Append
  Start-Sleep -Seconds 2
  if ((Test-Path $capRaw) -and -not (Test-Path $raw)) { Copy-Item $capRaw $raw -Force }
  if ((Test-Path $capSum) -and -not (Test-Path $sum)) { Copy-Item $capSum $sum -Force }
  $ntSize = if (Test-Path $nettrace) { (Get-Item $nettrace).Length } else { 0 }
  $row = "sess=$s binary=MotionSoftDemo.exe machine=DESKTOP-K93N3P8 round=$s appPID=$appPid dotnetPID=$($proc.Id) saw=$saw nettraceBytes=$ntSize"
  if (Test-Path $sum) {
    Get-Content $sum | Tee-Object $runLog -Append
    $body = Get-Content $sum -Raw
    if ($body -match 'SESSION_PASS_CANDIDATE') { $passN++; $row += ' verdict=PASS_CANDIDATE' }
    else { $row += ' verdict=FAIL_OR_OTHER' }
  } else {
    "MISSING summary sess=$s saw=$saw" | Tee-Object $runLog -Append
    $row += ' verdict=MISSING_SUMMARY'
  }
  Add-Content $matrix $row
  $pids += $appPid
  "----- SESSION $s END -----" | Tee-Object $runLog -Append
  Start-Sleep -Seconds 3
}

Remove-Item (Join-Path $artRoot 'm2_02.flag') -Force -ErrorAction SilentlyContinue
Remove-Item $sessFile -Force -ErrorAction SilentlyContinue
Remove-Item Env:MF_M2_02 -ErrorAction SilentlyContinue
Remove-Item Env:MF_M2_02_SESSION -ErrorAction SilentlyContinue
$post = (Get-FileHash 'C:\Users\admin\motion_framework\code\composition.h' -Algorithm SHA256).Hash.ToLower()
"composition.h POST=$post" | Tee-Object $runLog -Append
"PRE_EQ_POST=$($pre -eq $post)" | Tee-Object $runLog -Append
"G2_SESSION_PASS_CANDIDATES=$passN/3" | Tee-Object $runLog -Append
"appPIDs=$($pids -join ',')" | Tee-Object $runLog -Append
"===== M2-02 RUN END $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') AWST =====" | Tee-Object $runLog -Append
if ($passN -lt 3) { exit 3 } else { exit 0 }

