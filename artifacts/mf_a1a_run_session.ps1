param(
  [Parameter(Mandatory=$true)][string]$SessionId,
  [int]$MaxWaitSec = 600
)
$ErrorActionPreference = 'Continue'
$artRoot = 'C:\Users\admin\motion_framework\artifacts'
$demo = 'C:\Users\admin\motion_framework\MotionSoftDemo'
$rawPath = Join-Path $artRoot 'p14_50_raw.txt'
$outRaw = Join-Path $artRoot ("t_a1a_s{0}_raw.txt" -f $SessionId)
$runLog = Join-Path $artRoot ("t_a1a_s{0}_run.txt" -f $SessionId)
$probeFlag = Join-Path $artRoot 'r350_probe.flag'
$fullFlag = Join-Path $artRoot 'r350_full.flag'
$stopMark = '===== STAGE A END ====='

function Stop-MotionSoftDemo {
  Get-Process -Name MotionSoftDemo -ErrorAction SilentlyContinue | ForEach-Object {
    try { $_.CloseMainWindow() | Out-Null } catch {}
  }
  Start-Sleep -Seconds 2
  Get-Process -Name MotionSoftDemo -ErrorAction SilentlyContinue | ForEach-Object {
    try { Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue } catch {}
  }
  Start-Sleep -Seconds 1
  -not (Get-Process -Name MotionSoftDemo -ErrorAction SilentlyContinue)
}

"===== T-A1a SESSION $SessionId START $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') stopMark=$stopMark =====" | Tee-Object -FilePath $runLog
"stop clean: $(Stop-MotionSoftDemo)" | Tee-Object -FilePath $runLog -Append

Remove-Item $fullFlag -Force -ErrorAction SilentlyContinue
Set-Content -Path $probeFlag -Value '1' -Encoding ascii

if (Test-Path $rawPath) {
  Copy-Item $rawPath (Join-Path $artRoot ("p14_50_raw_pre_a1a_s{0}.bak" -f $SessionId)) -Force
  Remove-Item $rawPath -Force
}
Remove-Item $outRaw -Force -ErrorAction SilentlyContinue

$env:WINAPP_CLI_TELEMETRY_OPTOUT = '1'
$env:MF_R350_PROBE = '1'
Remove-Item Env:MF_R350_FULL -ErrorAction SilentlyContinue

$stdout = Join-Path $artRoot ("t_a1a_s{0}_stdout.txt" -f $SessionId)
$stderr = Join-Path $artRoot ("t_a1a_s{0}_stderr.txt" -f $SessionId)
$dotnetArgs = @('run','-c','Debug','-p:Platform=x64','--no-build')
"launch: dotnet $($dotnetArgs -join ' ') MF_R350_PROBE=1" | Tee-Object -FilePath $runLog -Append
$proc = Start-Process -FilePath 'dotnet' -ArgumentList $dotnetArgs -WorkingDirectory $demo -PassThru -WindowStyle Hidden -RedirectStandardOutput $stdout -RedirectStandardError $stderr
"dotnetPID=$($proc.Id)" | Tee-Object -FilePath $runLog -Append

$deadline = (Get-Date).AddSeconds($MaxWaitSec)
$saw = $false
$appPid = $null
$script:rebuilt = $false
while ((Get-Date) -lt $deadline) {
  Start-Sleep -Seconds 3
  if (-not $appPid) {
    $p = Get-Process -Name MotionSoftDemo -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($p) { $appPid = $p.Id; "appPID=$appPid" | Tee-Object -FilePath $runLog -Append }
  }
  if (Test-Path $rawPath) {
    $tail = Get-Content $rawPath -Tail 40 -ErrorAction SilentlyContinue
    if ($tail -match [regex]::Escape($stopMark)) {
      $saw = $true
      "SAW $stopMark at $(Get-Date -Format 'HH:mm:ss')" | Tee-Object -FilePath $runLog -Append
      Start-Sleep -Seconds 2
      break
    }
  }
  if ($appPid -and -not (Get-Process -Id $appPid -ErrorAction SilentlyContinue)) {
    "app exited early" | Tee-Object -FilePath $runLog -Append
    break
  }
  if (-not $appPid -and (Test-Path $stderr)) {
    $err = Get-Content $stderr -Raw -ErrorAction SilentlyContinue
    if ($err -match 'does not exist|No build|MSB' -and -not $script:rebuilt) {
      $script:rebuilt = $true
      "no-build failed; rebuilding..." | Tee-Object -FilePath $runLog -Append
      try { Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue } catch {}
      & dotnet build -c Debug -p:Platform=x64 -v q 2>&1 | Tee-Object -FilePath $runLog -Append
      $dotnetArgs = @('run','-c','Debug','-p:Platform=x64','--no-build')
      $proc = Start-Process -FilePath 'dotnet' -ArgumentList $dotnetArgs -WorkingDirectory $demo -PassThru -WindowStyle Hidden -RedirectStandardOutput $stdout -RedirectStandardError $stderr
      "relaunch dotnetPID=$($proc.Id)" | Tee-Object -FilePath $runLog -Append
    }
  }
}

"stop after mark: $(Stop-MotionSoftDemo)" | Tee-Object -FilePath $runLog -Append
if (Test-Path $rawPath) {
  Copy-Item $rawPath $outRaw -Force
  "copied raw -> $outRaw size=$((Get-Item $outRaw).Length)" | Tee-Object -FilePath $runLog -Append
} else {
  "NO RAW at $rawPath" | Tee-Object -FilePath $runLog -Append
}
"sawMark=$saw ===== SESSION $SessionId END $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') =====" | Tee-Object -FilePath $runLog -Append
if (-not $saw) { exit 3 } else { exit 0 }
