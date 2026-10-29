# M3-RIVE-LOST runner (docs/m3_rive_lost_gate_LOCKED.md). Collects data: run only after
# the gate is LOCKED and APPROVED. Needs an elevated PowerShell, dxcap.exe (Settings >
# Optional features > Graphics Tools), m0rive.dll (built by m0\build_m0.ps1, with the
# device-loss recovery) beside the exe and the Rive asset under <ArtDir>\m0_assets.
#   powershell -ExecutionPolicy Bypass -File m3\run_m3_lost.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <dir> [-Sessions 3]
#   ... -Simulate   # local smoke test: in-process simulated loss, no dxcap, no admin; never counts (K1)
# Each injection (dxcap -forcetdr) resets the whole GPU: close other GPU programs and save
# your work first. The runner enforces K4/K5: >= 45 s between injections, >= 120 s between
# sessions, at most 8 injections per run, and it stops at once if an injection is not
# detected by the probe within 30 s. Don't touch mouse or keyboard while it runs.
param(
    [Parameter(Mandatory = $true)][string]$DemoExe,
    [Parameter(Mandatory = $true)][string]$ArtDir,
    [int]$Sessions = 3,
    [switch]$Simulate,
    [int]$MinGapSec = 45,
    [int]$CooldownSec = 120,
    [int]$MaxInjections = 8,
    [int]$DetectTimeoutSec = 30,
    [int]$SessionTimeoutSec = 1200
)
# Continue, not Stop: PowerShell 5.1 turns a native tool's stderr into a terminating
# error under Stop. Native exit codes are checked explicitly.
$ErrorActionPreference = "Continue"
$prefix = if ($Simulate) { "m3lostsim" } else { "m3lost" }
New-Item -ItemType Directory -Force $ArtDir | Out-Null
$Log = Join-Path $ArtDir "${prefix}_run.log"
function Say($m) { $l = "[{0:yyyy-MM-dd HH:mm:ss}] {1}" -f (Get-Date), $m; Write-Host $l; Add-Content $Log $l }
Add-Type -AssemblyName System.Drawing, System.Windows.Forms
Add-Type -Namespace M3L -Name Win -MemberDefinition @'
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct RECT { public int Left, Top, Right, Bottom; }
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool GetWindowRect(System.IntPtr h, out RECT r);
'@
# App window only (same reason as m0\run_m0.ps1: no desktop contents in the repo).
function Shot($path, $proc) {
    $proc.Refresh()
    $r = New-Object M3L.Win+RECT
    if ($proc.MainWindowHandle -eq [IntPtr]::Zero -or -not [M3L.Win]::GetWindowRect($proc.MainWindowHandle, [ref]$r)) { Say "WARN screenshot: no window"; return }
    $b = New-Object System.Drawing.Rectangle $r.Left, $r.Top, ($r.Right - $r.Left), ($r.Bottom - $r.Top)
    $bmp = New-Object System.Drawing.Bitmap $b.Width, $b.Height
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($b.Location, [System.Drawing.Point]::Empty, $b.Size)
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
}
function ReadKv($sum) {
    $kv = @{}
    if (Test-Path $sum) { Get-Content $sum | ForEach-Object { if ($_ -match '^([^=]+)=(.*)$') { $kv[$Matches[1]] = $Matches[2] } } }
    return $kv
}

Say "M3-LOST run: host=$env:COMPUTERNAME sessions=$Sessions simulate=$Simulate exe=$DemoExe"
Say "machineId(SMBIOS UUID)=$((Get-CimInstance Win32_ComputerSystemProduct).UUID)"
$os = Get-CimInstance Win32_OperatingSystem
Say "os=$($os.Caption) build=$($os.BuildNumber)"
Get-CimInstance Win32_VideoController | ForEach-Object { Say "gpu=$($_.Name) driver=$($_.DriverVersion) refresh=$($_.CurrentRefreshRate)Hz mode=$($_.CurrentHorizontalResolution)x$($_.CurrentVerticalResolution)" }
$composition = Join-Path (Split-Path $PSScriptRoot -Parent) "code\composition.h"
if (Test-Path $composition) { Say ("composition.h sha256 PRE=" + (Get-FileHash $composition -Algorithm SHA256).Hash.ToLower()) }
# TDR settings are read for the record only; the gate forbids changing them (G0).
$tdrKey = "HKLM:\SYSTEM\CurrentControlSet\Control\GraphicsDrivers"
foreach ($n in "TdrLevel", "TdrDelay", "TdrLimitCount", "TdrLimitTime") {
    $v = (Get-ItemProperty $tdrKey -Name $n -ErrorAction SilentlyContinue).$n
    Say ("registry {0}={1}" -f $n, $(if ($null -eq $v) { "default" } else { $v }))
}

if (-not $Simulate) {
    $admin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    if (-not $admin) { Say "BLOCKED: needs an elevated PowerShell (dxcap -forcetdr)"; exit 2 }
    $dxcap = Get-Command dxcap.exe -ErrorAction SilentlyContinue
    if (-not $dxcap) { Say "BLOCKED: dxcap.exe not found; install Settings > Optional features > Graphics Tools"; exit 2 }
    Say "dxcap=$($dxcap.Source)"
}

$runStart = Get-Date
$lastInject = [datetime]::MinValue
$injections = 0
$stopAll = $false
$matrix = @("session,pid,verdict,lost_detected,recovered,recover_ok,recover_max_ms,budget_ok,stability_ok,frames_missed,private_mb_delta,event4101")
for ($s = 1; $s -le $Sessions -and -not $stopAll; $s++) {
    # K4: cooldown between sessions, counted from the last injection.
    if ($lastInject -ne [datetime]::MinValue) {
        $wait = $CooldownSec - ((Get-Date) - $lastInject).TotalSeconds
        if ($wait -gt 0) { Say ("cooldown {0:N0} s before session $s" -f $wait); Start-Sleep -Seconds ([math]::Ceiling($wait)) }
    }
    $pre = Join-Path $ArtDir "${prefix}_s${s}"
    $sum = "${pre}_summary.txt"
    Remove-Item $sum, "${pre}_*.shotreq", "${pre}_inject*.req" -ErrorAction SilentlyContinue
    Get-Process -Name MotionSoftDemo -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep 2
    $sessStart = Get-Date
    $env:MF_M3_LOST_PROBE = "1"; $env:MF_M3_SESSION = "$s"; $env:MF_M0_ARTDIR = $ArtDir
    if ($Simulate) { $env:MF_M3_LOST_SIM = "1" } else { Remove-Item env:MF_M3_LOST_SIM -ErrorAction SilentlyContinue }
    $probeArg = if ($Simulate) { '--m3-lost-sim' } else { '--m3-lost-probe' }
    $p = Start-Process -FilePath $DemoExe -ArgumentList @($probeArg) -WorkingDirectory (Split-Path $DemoExe -Parent) -PassThru -WindowStyle Normal
    Say "session $s pid=$($p.Id)"
    $deadline = (Get-Date).AddSeconds($SessionTimeoutSec)
    while ((Get-Date) -lt $deadline) {
        Get-ChildItem $ArtDir -Filter "${prefix}_s${s}_*.shotreq" -ErrorAction SilentlyContinue | ForEach-Object {
            $target = (Get-Content $_.FullName -Raw).Trim()
            Shot $target $p
            Say "session $s screenshot $(Split-Path $target -Leaf)"
            Remove-Item $_.FullName -ErrorAction SilentlyContinue
        }
        Get-ChildItem $ArtDir -Filter "${prefix}_s${s}_inject*.req" -ErrorAction SilentlyContinue | ForEach-Object {
            $k = [int]($_.BaseName -replace '^.*_inject', '')
            Remove-Item $_.FullName -ErrorAction SilentlyContinue
            if ($injections -ge $MaxInjections) { Say "STOP: injection cap $MaxInjections reached (K4)"; $stopAll = $true; return }
            $wait = $MinGapSec - ((Get-Date) - $lastInject).TotalSeconds
            if ($wait -gt 0) { Say ("session $s inject$k waits {0:N0} s (K5 spacing)" -f $wait); Start-Sleep -Seconds ([math]::Ceiling($wait)) }
            $lastInject = Get-Date
            $injections++
            $out = & dxcap.exe -forcetdr 2>&1
            Say ("session $s inject$k dxcap -forcetdr exit=$LASTEXITCODE out=" + (($out | Out-String).Trim() -replace '\s+', ' '))
            $detected = $false
            $until = (Get-Date).AddSeconds($DetectTimeoutSec)
            while ((Get-Date) -lt $until) {
                if ((Test-Path $sum) -and (Select-String -Path $sum -Pattern "^inject$k\.detected=1" -Quiet)) { $detected = $true; break }
                if ($p.HasExited) { break }
                Start-Sleep -Milliseconds 200
            }
            if ($detected) { Say ("session $s inject$k detected after {0:N1} s" -f ((Get-Date) - $lastInject).TotalSeconds) }
            else { Say "STOP: session $s inject$k not detected within $DetectTimeoutSec s (K5); no further injections"; $stopAll = $true }
        }
        if ($stopAll) { break }
        if ((Test-Path $sum) -and (Select-String -Path $sum -Pattern '^verdict=' -Quiet)) { break }
        if ($p.HasExited) { Say "WARN session $s app exited early (exit code $($p.ExitCode))"; break }
        Start-Sleep -Milliseconds 200
    }
    Start-Sleep 2
    if (-not $p.HasExited) { Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue }
    $kv = ReadKv $sum
    $v = if ($kv.ContainsKey("verdict")) { $kv["verdict"] } elseif ($p.HasExited -and (Test-Path $sum)) { "CRASHED_NO_VERDICT" } else { "INVALID_NO_SUMMARY" }
    if ($stopAll -and $v -notmatch '^(FAIL|CRASHED)') { $v = "STOPPED_" + $v }
    # Injection evidence: "display driver stopped responding and has recovered".
    $ev = @(Get-WinEvent -FilterHashtable @{ LogName = 'System'; Id = 4101; StartTime = $sessStart } -ErrorAction SilentlyContinue)
    $ev | ForEach-Object { Say ("session $s event4101 at {0:yyyy-MM-dd HH:mm:ss} {1}" -f $_.TimeCreated, $_.ProviderName) }
    $matrix += "$s,$($p.Id),$v,$($kv['lost_detected']),$($kv['recovered']),$($kv['recover_ok']),$($kv['recover_max_ms']),$($kv['budget_ok']),$($kv['stability_ok']),$($kv['frames_missed']),$($kv['private_mb_delta']),$($ev.Count)"
    Say "session $s verdict=$v recover_ok=$($kv['recover_ok']) recover_max_ms=$($kv['recover_max_ms']) event4101=$($ev.Count)"
}
Remove-Item env:MF_M3_LOST_PROBE, env:MF_M3_LOST_SIM, env:MF_M3_SESSION, env:MF_M0_ARTDIR -ErrorAction SilentlyContinue
$matrix | Out-File (Join-Path $ArtDir "${prefix}_session_matrix.txt") -Encoding utf8
$bug = @(Get-WinEvent -FilterHashtable @{ LogName = 'System'; Id = 1001; StartTime = $runStart } -ErrorAction SilentlyContinue | Where-Object { $_.ProviderName -match 'BugCheck|WER-SystemErrorReporting' })
Say "injections=$injections bugcheck_events=$($bug.Count)"
if (Test-Path $composition) { Say ("composition.h sha256 POST=" + (Get-FileHash $composition -Algorithm SHA256).Hash.ToLower()) }
Say "RUN_DONE"
