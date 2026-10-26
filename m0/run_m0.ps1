# Runs the M0 probes on the lab machine. Collects data, so it may only be run
# after docs/m0_tech_validation_gate_LOCKED.md has APPROVED_* filled in.
#   powershell -ExecutionPolicy Bypass -File m0\run_m0.ps1 -DemoExe <path\MotionSoftDemo.exe> [-Inject] [-Sessions 3] [-PresentMon <path\PresentMon.exe>]
# -Inject enables the SendInput pointer phases (M0-2, M0-3/5); pass it only if the
# APPROVED note grants decision D1.
param(
    [Parameter(Mandatory = $true)][string]$DemoExe,
    [switch]$Inject,
    [int]$Sessions = 3,
    [string]$PresentMon = "",
    [string]$ArtDir = "C:\Users\admin\motion_framework\artifacts",
    [int]$SessionTimeoutSec = 180
)
$ErrorActionPreference = "Stop"
$M0 = $PSScriptRoot
$Out = Join-Path $M0 "out"
$Log = Join-Path $ArtDir "m0_run.log"
New-Item -ItemType Directory -Force $ArtDir | Out-Null
function Say($m) { $l = "[{0:yyyy-MM-dd HH:mm:ss}] {1}" -f (Get-Date), $m; Write-Host $l; Add-Content $Log $l }
Add-Type -AssemblyName System.Drawing, System.Windows.Forms

function Shot($path) {
    $b = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
    $bmp = New-Object System.Drawing.Bitmap $b.Width, $b.Height
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($b.Location, [System.Drawing.Point]::Empty, $b.Size)
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
}

Say "M0 run: host=$env:COMPUTERNAME sessions=$Sessions inject=$($Inject.IsPresent)"
$uuid = (Get-CimInstance Win32_ComputerSystemProduct).UUID
Say "machineId(SMBIOS UUID)=$uuid"
$composition = Join-Path (Split-Path $M0 -Parent) "code\composition.h"
if (Test-Path $composition) { Say ("composition.h sha256 PRE=" + (Get-FileHash $composition -Algorithm SHA256).Hash.ToLower()) }

# --- M0-4: ALLOW_TEARING ---------------------------------------------------------------
$tearDir = Join-Path $ArtDir "m0_tear"
New-Item -ItemType Directory -Force $tearDir | Out-Null
$pm = $null
if ($PresentMon -and (Test-Path $PresentMon)) {
    $pm = Start-Process $PresentMon -PassThru -WindowStyle Hidden -ArgumentList @(
        "--process_name", "m0_tear_probe.exe", "--output_file", (Join-Path $tearDir "presentmon.csv"),
        "--timed", "60", "--terminate_after_timed", "--no_console_stats")
    Start-Sleep 2
    Say "PresentMon pid=$($pm.Id)"
} else { Say "PresentMon not given: PresentMode (independent flip) will not be recorded" }
& (Join-Path $Out "m0_tear_probe.exe") $tearDir 1200 | Out-File (Join-Path $tearDir "stdout.txt")
Say "tear probe exit=$LASTEXITCODE"
if ($pm) { Stop-Process -Id $pm.Id -ErrorAction SilentlyContinue }

# --- M0-1 / M0-2 / M0-3/5: MotionSoftDemo sessions, one process each ----------------------
$matrix = @("session,pid,verdict,m0_1a_frames,m0_1a_cpu_p95_ms,m0_1a_gpu_p95_ms,expr_latency_p50_ms,ui_latency_p50_ms,misalign_px")
for ($s = 1; $s -le $Sessions; $s++) {
    $sum = Join-Path $ArtDir "m0_s${s}_summary.txt"
    Remove-Item $sum -ErrorAction SilentlyContinue
    $env:MF_M0_PROBE = "1"; $env:MF_M0_SESSION = "$s"; $env:MF_M0_INJECT = $(if ($Inject) { "1" } else { "0" })
    $p = Start-Process $DemoExe -PassThru
    Say "session $s pid=$($p.Id)"
    Start-Sleep 8
    Shot (Join-Path $ArtDir "m0_s${s}_screen_m0_1a.png")   # Rive surface visible during M0-1a
    $deadline = (Get-Date).AddSeconds($SessionTimeoutSec)
    while ((Get-Date) -lt $deadline -and -not ((Test-Path $sum) -and (Select-String -Path $sum -Pattern '^verdict=' -Quiet))) { Start-Sleep 1 }
    Start-Sleep 2
    if (-not $p.HasExited) { Stop-Process -Id $p.Id }
    $kv = @{}
    if (Test-Path $sum) { Get-Content $sum | ForEach-Object { if ($_ -match '^([^=]+)=(.*)$') { $kv[$Matches[1]] = $Matches[2] } } }
    $v = if ($kv.ContainsKey("verdict")) { $kv["verdict"] } else { "INVALID_NO_SUMMARY" }
    $matrix += "$s,$($p.Id),$v,$($kv['m0_1a.frames']),$($kv['m0_1a.cpu_p95_ms']),$($kv['m0_1a.gpu_p95_ms']),$($kv['m0_35.expr.latency_p50_ms']),$($kv['m0_35.ui.latency_p50_ms']),$($kv['m0_2.misalign_px_at_drag_speed'])"
    Say "session $s verdict=$v"
}
Remove-Item env:MF_M0_PROBE, env:MF_M0_SESSION, env:MF_M0_INJECT -ErrorAction SilentlyContinue
$matrix | Out-File (Join-Path $ArtDir "m0_session_matrix.txt") -Encoding utf8
if (Test-Path $composition) { Say ("composition.h sha256 POST=" + (Get-FileHash $composition -Algorithm SHA256).Hash.ToLower()) }
Say "RUN_DONE"
