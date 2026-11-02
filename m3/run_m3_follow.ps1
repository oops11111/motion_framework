# M3-RIVE-FOLLOW runner (docs/m3_rive_follow_gate_LOCKED.md). Collects data: run only after
# the gate is LOCKED and APPROVED. Needs m0rive.dll (with m0r_set_follow_cursor) beside the
# exe, mfprobe.dll (WGC tracker, as for M0) and the Rive asset under <ArtDir>\m0_assets.
#   powershell -ExecutionPolicy Bypass -File m3\run_m3_follow.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <dir> [-Sessions 3]
# The probe moves the cursor with SendInput (P4): don't touch mouse or keyboard, and keep
# the app window in front and unobstructed. No admin rights and no driver restarts.
param(
    [Parameter(Mandatory = $true)][string]$DemoExe,
    [Parameter(Mandatory = $true)][string]$ArtDir,
    [int]$Sessions = 3,
    [int]$SessionTimeoutSec = 900
)
# Continue, not Stop: PowerShell 5.1 turns a native tool's stderr into a terminating error.
$ErrorActionPreference = "Continue"
New-Item -ItemType Directory -Force $ArtDir | Out-Null
$Log = Join-Path $ArtDir "m3follow_run.log"
function Say($m) { $l = "[{0:yyyy-MM-dd HH:mm:ss}] {1}" -f (Get-Date), $m; Write-Host $l; Add-Content $Log $l }
function ReadKv($sum) {
    $kv = @{}
    if (Test-Path $sum) { Get-Content $sum | ForEach-Object { if ($_ -match '^([^=]+)=(.*)$') { $kv[$Matches[1]] = $Matches[2] } } }
    return $kv
}

Say "M3-FOLLOW run: host=$env:COMPUTERNAME sessions=$Sessions exe=$DemoExe"
Say "machineId(SMBIOS UUID)=$((Get-CimInstance Win32_ComputerSystemProduct).UUID)"
$os = Get-CimInstance Win32_OperatingSystem
Say "os=$($os.Caption) build=$($os.BuildNumber)"
Get-CimInstance Win32_VideoController | ForEach-Object { Say "gpu=$($_.Name) driver=$($_.DriverVersion) refresh=$($_.CurrentRefreshRate)Hz mode=$($_.CurrentHorizontalResolution)x$($_.CurrentVerticalResolution)" }
$composition = Join-Path (Split-Path $PSScriptRoot -Parent) "code\composition.h"
if (Test-Path $composition) { Say ("composition.h sha256 PRE=" + (Get-FileHash $composition -Algorithm SHA256).Hash.ToLower()) }

$paths = "A_surface_ui", "B_surface_latch", "C_split_expr", "D_split_ui"
$matrix = @("session,pid,verdict," + (($paths | ForEach-Object { "$($_)_p50_ms,$($_)_hit" }) -join ",") + ",c_vs_a_frames,b_vs_a_frames,seam_frames,split_default,latch_default,seam_ok,budget_ok")
for ($s = 1; $s -le $Sessions; $s++) {
    $sum = Join-Path $ArtDir "m3follow_s${s}_summary.txt"
    Remove-Item $sum -ErrorAction SilentlyContinue
    Get-Process -Name MotionSoftDemo -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep 2
    $env:MF_M3_FOLLOW_PROBE = "1"; $env:MF_M3_SESSION = "$s"; $env:MF_M0_ARTDIR = $ArtDir
    $p = Start-Process -FilePath $DemoExe -ArgumentList @('--m3-follow-probe') -WorkingDirectory (Split-Path $DemoExe -Parent) -PassThru -WindowStyle Normal
    Say "session $s pid=$($p.Id)"
    $deadline = (Get-Date).AddSeconds($SessionTimeoutSec)
    while ((Get-Date) -lt $deadline) {
        if ((Test-Path $sum) -and (Select-String -Path $sum -Pattern '^verdict=' -Quiet)) { break }
        if ($p.HasExited) { Say "WARN session $s app exited early (exit code $($p.ExitCode))"; break }
        Start-Sleep -Milliseconds 500
    }
    Start-Sleep 2
    # Decide whether the app exited on its own before ending it (r2 L3).
    $exitedOnOwn = $p.HasExited
    if (-not $exitedOnOwn) { Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue }
    $kv = ReadKv $sum
    $v = if ($kv.ContainsKey("verdict")) { $kv["verdict"] } elseif (-not (Test-Path $sum)) { "INVALID_NO_SUMMARY" } elseif ($exitedOnOwn) { "CRASHED_NO_VERDICT" } else { "KILLED_BY_RUNNER" }
    $cols = ($paths | ForEach-Object { "$($kv["follow.$_.latency_p50_ms"]),$($kv["follow.$_.steps_hit"])" }) -join ","
    $matrix += "$s,$($p.Id),$v,$cols,$($kv['rule.c_vs_a_frames']),$($kv['rule.b_vs_a_frames']),$($kv['rule.seam_frames']),$($kv['rule.split_default']),$($kv['rule.latch_default']),$($kv['rule.seam_ok']),$($kv['budget_ok'])"
    Say "session $s verdict=$v c_vs_a=$($kv['rule.c_vs_a_frames']) b_vs_a=$($kv['rule.b_vs_a_frames']) seam=$($kv['rule.seam_frames']) budget_ok=$($kv['budget_ok'])"
}
Remove-Item env:MF_M3_FOLLOW_PROBE, env:MF_M3_SESSION, env:MF_M0_ARTDIR -ErrorAction SilentlyContinue
$matrix | Out-File (Join-Path $ArtDir "m3follow_session_matrix.txt") -Encoding utf8
if (Test-Path $composition) { Say ("composition.h sha256 POST=" + (Get-FileHash $composition -Algorithm SHA256).Hash.ToLower()) }
Say "RUN_DONE"
