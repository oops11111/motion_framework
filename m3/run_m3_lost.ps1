# M3-RIVE-LOST runner (docs/m3_rive_lost_recover_gate_LOCKED.md; r2 rules). Collects data: run only after
# the gate is LOCKED and APPROVED. Needs an elevated PowerShell, m0rive.dll (built by
# m0\build_m0.ps1, with the device-loss recovery) beside the exe and the Rive asset under
# <ArtDir>\m0_assets.
#   powershell -ExecutionPolicy Bypass -File m3\run_m3_lost.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <dir> [-Sessions 3] [-GpuInstanceId <id>]
#   ... -Simulate   # local smoke test: in-process simulated loss, no driver restart, no admin; never counts (K1)
# Each injection restarts the display driver (pnputil /restart-device, r2 L1), which removes
# every D3D device on the machine: close other GPU programs and save your work first. Before
# session 1 the runner restarts the driver once with no app running (L2 self-test). It
# enforces L4: >= 45 s between injections, >= 120 s before each session, at most 8
# injections per run; it stops at once if the adapter is not back to OK within 30 s or an
# injection is not detected by the probe within 30 s. Don't touch mouse or keyboard.
param(
    [Parameter(Mandatory = $true)][string]$DemoExe,
    [Parameter(Mandatory = $true)][string]$ArtDir,
    [int]$Sessions = 3,
    [switch]$Simulate,
    [string]$GpuInstanceId = "",
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

$runStart = Get-Date
$lastInject = [datetime]::MinValue
$injections = 0
$stopAll = $false

function AdapterStatus($id) { (Get-PnpDevice -InstanceId $id -ErrorAction SilentlyContinue).Status }
# One injection (L1): restart the display driver, then wait up to 30 s for the adapter to
# report OK again (L4). Logs pnputil's output and exit code and the adapter status (L3).
# Returns $true when pnputil exited 0 and the adapter is back to OK.
function Inject($tag) {
    $script:lastInject = Get-Date
    $script:injections++
    $out = & pnputil.exe /restart-device "$GpuInstanceId" 2>&1
    $code = $LASTEXITCODE
    Say ("$tag pnputil /restart-device exit=$code out=" + (($out | Out-String).Trim() -replace '\s+', ' '))
    $st = $null
    $until = (Get-Date).AddSeconds(30)
    do { Start-Sleep -Milliseconds 500; $st = AdapterStatus $GpuInstanceId } while ($st -ne 'OK' -and (Get-Date) -lt $until)
    Say ("$tag adapter status=$st after {0:N1} s" -f ((Get-Date) - $script:lastInject).TotalSeconds)
    return ($code -eq 0 -and $st -eq 'OK')
}

if (-not $Simulate) {
    $admin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    if (-not $admin) { Say "BLOCKED: needs an elevated PowerShell (pnputil /restart-device)"; exit 2 }
    if (-not $GpuInstanceId) {
        $cand = @(Get-PnpDevice -Class Display -PresentOnly -ErrorAction SilentlyContinue | Where-Object { $_.Status -eq 'OK' -and $_.FriendlyName -notmatch 'Basic Display|Basic Render|Remote|Indirect' })
        $cand | ForEach-Object { Say "display adapter: $($_.FriendlyName) id=$($_.InstanceId) status=$($_.Status)" }
        if ($cand.Count -ne 1) { Say "BLOCKED: found $($cand.Count) display adapters; pass -GpuInstanceId with the one the app renders on"; exit 2 }
        $GpuInstanceId = $cand[0].InstanceId
    }
    Say "gpu instance id=$GpuInstanceId status=$(AdapterStatus $GpuInstanceId)"
    if ((AdapterStatus $GpuInstanceId) -ne 'OK') { Say "BLOCKED: adapter status is not OK before the run"; exit 2 }
    # L2: injection self-test with no app running; counts toward the cap.
    Get-Process -Name MotionSoftDemo -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Say "self-test: restarting the display driver once with no app running (L2)"
    if (-not (Inject "self-test")) {
        Say "STOP: BLOCKED_INJECT (self-test: driver restart failed or adapter not OK within 30 s)"
        "session,pid,verdict`n0,,BLOCKED_INJECT" | Out-File (Join-Path $ArtDir "${prefix}_session_matrix.txt") -Encoding utf8
        Say "RUN_DONE"
        exit 3
    }
    Say "self-test OK"
}
$matrix = @("session,pid,verdict,lost_detected,recovered,recover_ok,recover_max_ms,budget_ok,stability_ok,frames_missed,private_mb_delta,injections_ok")
for ($s = 1; $s -le $Sessions -and -not $stopAll; $s++) {
    # K4: cooldown between sessions, counted from the last injection.
    if ($lastInject -ne [datetime]::MinValue) {
        $wait = $CooldownSec - ((Get-Date) - $lastInject).TotalSeconds
        if ($wait -gt 0) { Say ("cooldown {0:N0} s before session $s" -f $wait); Start-Sleep -Seconds ([math]::Ceiling($wait)) }
    }
    $pre = Join-Path $ArtDir "${prefix}_s${s}"
    $sum = "${pre}_summary.txt"
    Remove-Item $sum, "${pre}_stderr.txt", "${pre}_*.shotreq", "${pre}_inject*.req" -ErrorAction SilentlyContinue
    Get-Process -Name MotionSoftDemo -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep 2
    $sessInjOk = $true
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
            if (-not (Inject "session $s inject$k")) {
                $sessInjOk = $false
                Say "STOP: session $s inject$k driver restart failed or adapter not OK within 30 s (L4); INVALID_SYSTEM"
                $stopAll = $true
                return
            }
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
    # L3: decide whether the app exited on its own BEFORE the runner ends it; a process the
    # runner killed is KILLED_BY_RUNNER, never a crash.
    $exitedOnOwn = $p.HasExited
    if (-not $exitedOnOwn) { Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue }
    $kv = ReadKv $sum
    $v = if ($kv.ContainsKey("verdict")) { $kv["verdict"] } elseif (-not (Test-Path $sum)) { "INVALID_NO_SUMMARY" } elseif ($exitedOnOwn) { "CRASHED_NO_VERDICT" } else { "KILLED_BY_RUNNER" }
    if ($stopAll -and $v -notmatch '^(FAIL|CRASHED)') { $v = "STOPPED_" + $v }
    # M1: the DLL's stderr (recovery log; Rive's VERIFY_OK message if it aborted).
    $errFile = "${pre}_stderr.txt"
    if (Test-Path $errFile) { Get-Content $errFile -Tail 20 | ForEach-Object { Say "session $s stderr| $_" } }
    if ($kv.ContainsKey("abort_stage")) { Say "session $s abort_stage=$($kv['abort_stage'])" }
    $matrix += "$s,$($p.Id),$v,$($kv['lost_detected']),$($kv['recovered']),$($kv['recover_ok']),$($kv['recover_max_ms']),$($kv['budget_ok']),$($kv['stability_ok']),$($kv['frames_missed']),$($kv['private_mb_delta']),$([int]$sessInjOk)"
    Say "session $s verdict=$v recover_ok=$($kv['recover_ok']) recover_max_ms=$($kv['recover_max_ms']) exited_on_own=$exitedOnOwn injections_ok=$([int]$sessInjOk)"
}
Remove-Item env:MF_M3_LOST_PROBE, env:MF_M3_LOST_SIM, env:MF_M3_SESSION, env:MF_M0_ARTDIR -ErrorAction SilentlyContinue
$matrix | Out-File (Join-Path $ArtDir "${prefix}_session_matrix.txt") -Encoding utf8
$bug = @(Get-WinEvent -FilterHashtable @{ LogName = 'System'; Id = 1001; StartTime = $runStart } -ErrorAction SilentlyContinue | Where-Object { $_.ProviderName -match 'BugCheck|WER-SystemErrorReporting' })
Say "injections=$injections bugcheck_events=$($bug.Count)"
if (Test-Path $composition) { Say ("composition.h sha256 POST=" + (Get-FileHash $composition -Algorithm SHA256).Hash.ToLower()) }
Say "RUN_DONE"
