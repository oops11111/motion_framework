# M3-XAML-LOST runner (docs/m3_xaml_lost_gate_LOCKED.md). Collects data: run only after
# the gate is LOCKED and APPROVED. Needs an elevated PowerShell; the Rive comparison
# session also needs m0rive.dll (RECOVER build) beside the exe and the Rive asset under
# <ArtDir>\m0_assets.
#   powershell -ExecutionPolicy Bypass -File m3\run_m3_xaml_lost.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <dir> [-GpuInstanceId <id>]
#   ... -DryRun   # walk through the flow without restarting the driver; no admin; never counts
#   ... -Rebuild -RiveSessions 0   # M3-XAML-REBUILD (docs/m3_xaml_rebuild_gate_LOCKED.md): W_pre / W_post windows
#   ... -LaunchMode Package -DemoProj <project dir>   # packaged (MSIX) launch via dotnet run, as m0\run_m0.ps1 (R2)
# Sessions 1..PlainSessions run the plain probe (no Rive); the next RiveSessions run the same
# probe with the Rive surface (N3). Each session restarts the display driver once
# (pnputil /restart-device) when the probe asks. Injection rules are those of r2: self-test
# first, >= 45 s between injections, >= 120 s before each session, at most 8 per run, stop
# at once if the adapter is not back to OK within 30 s. Don't touch mouse or keyboard.
param(
    [Parameter(Mandatory = $true)][string]$DemoExe,
    [Parameter(Mandatory = $true)][string]$ArtDir,
    [int]$PlainSessions = 3,
    [int]$RiveSessions = 1,
    [switch]$DryRun,
    [switch]$Rebuild,
    [ValidateSet("Exe", "Package")][string]$LaunchMode = "Exe",
    [string]$DemoProj = "",
    [string]$GpuInstanceId = "",
    [int]$MinGapSec = 45,
    [int]$CooldownSec = 120,
    [int]$MaxInjections = 8,
    [int]$SessionTimeoutSec = 900
)
# Continue, not Stop: PowerShell 5.1 turns a native tool's stderr into a terminating
# error under Stop. Native exit codes are checked explicitly.
$ErrorActionPreference = "Continue"
$stem = if ($Rebuild) { "m3xrb" } else { "m3xaml" }
$prefix = if ($DryRun) { "${stem}dry" } else { $stem }
New-Item -ItemType Directory -Force $ArtDir | Out-Null
$Log = Join-Path $ArtDir "${prefix}_run.log"
function Say($m) { $l = "[{0:yyyy-MM-dd HH:mm:ss}] {1}" -f (Get-Date), $m; Write-Host $l; Add-Content $Log $l }
Add-Type -AssemblyName System.Drawing, System.Windows.Forms
Add-Type -Namespace M3X -Name Win -MemberDefinition @'
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct RECT { public int Left, Top, Right, Bottom; }
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool GetWindowRect(System.IntPtr h, out RECT r);
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool IsWindow(System.IntPtr h);
'@
# App window only (same reason as m0\run_m0.ps1: no desktop contents in the repo).
# $hwnd (optional) names the window; otherwise the process's main window (XAML-REBUILD).
function Shot($path, $proc, $hwnd = [IntPtr]::Zero) {
    $proc.Refresh()
    $h = if ($hwnd -ne [IntPtr]::Zero) { $hwnd } else { $proc.MainWindowHandle }
    $r = New-Object M3X.Win+RECT
    if ($h -eq [IntPtr]::Zero -or -not [M3X.Win]::IsWindow($h) -or -not [M3X.Win]::GetWindowRect($h, [ref]$r)) { Say "WARN screenshot: no window"; return }
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

Say "M3-XAML-LOST run: host=$env:COMPUTERNAME plain=$PlainSessions rive=$RiveSessions dryrun=$DryRun exe=$DemoExe"
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

if (-not $DryRun) {
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
$matrix = @("session,pid,with_rive,verdict,inject_ok,rive_recovered,shots")
$total = $PlainSessions + $RiveSessions
for ($s = 1; $s -le $total -and -not $stopAll; $s++) {
    $withRive = $s -gt $PlainSessions
    if ($lastInject -ne [datetime]::MinValue) {
        $wait = $CooldownSec - ((Get-Date) - $lastInject).TotalSeconds
        if ($wait -gt 0) { Say ("cooldown {0:N0} s before session $s" -f $wait); Start-Sleep -Seconds ([math]::Ceiling($wait)) }
    }
    $pre = Join-Path $ArtDir "${stem}_s${s}"
    $sum = "${pre}_summary.txt"
    Remove-Item $sum, "${pre}_stderr.txt", "${pre}_*.shotreq", "${pre}_inject1.*" -ErrorAction SilentlyContinue
    Get-Process -Name MotionSoftDemo -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep 2
    $env:MF_M3_SESSION = "$s"; $env:MF_M0_ARTDIR = $ArtDir
    $probeArg = if ($Rebuild) { '--m3-xaml-rebuild' } elseif ($withRive) { '--m3-xaml-with-rive' } else { '--m3-xaml-lost-probe' }
    # A packaged launch may not see the environment, so the probe also takes these as arguments.
    $appArgs = @($probeArg, "--m3-artdir=$ArtDir", "--m3-session=$s")
    if ($withRive -and $Rebuild) { $appArgs += '--m3-xaml-with-rive' }
    if ($LaunchMode -eq "Exe") {
        $p = Start-Process -FilePath $DemoExe -ArgumentList $appArgs -WorkingDirectory (Split-Path $DemoExe -Parent) -PassThru -WindowStyle Normal
    } else {
        # Same as m0\run_m0.ps1 -LaunchMode Package: MSIX launch profile through dotnet run.
        $json = @{ profiles = @{ "MotionSoftDemo (Package)" = @{ commandName = "MsixPackage"; commandLineArgs = ($appArgs -join ' ') } } } | ConvertTo-Json -Depth 4
        Set-Content (Join-Path $DemoProj "Properties\launchSettings.json") -Value $json -Encoding utf8
        $dotnetArgs = @('run', '-c', 'Debug', '-p:Platform=x64', '--no-build', '--launch-profile', 'MotionSoftDemo (Package)')
        $dn = Start-Process -FilePath 'dotnet' -ArgumentList $dotnetArgs -WorkingDirectory $DemoProj -PassThru -WindowStyle Normal
        Say "session $s dotnetPid=$($dn.Id) (packaged launch)"
        $p = $null
        for ($i = 0; $i -lt 90 -and -not $p; $i++) { Start-Sleep 1; $p = Get-Process -Name MotionSoftDemo -ErrorAction SilentlyContinue | Select-Object -First 1 }
        if (-not $p) {
            Say "session $s BLOCKED_PACKAGED: no MotionSoftDemo process after 90 s (R2: record and skip)"
            $matrix += "$s,,$([int]$withRive),BLOCKED_PACKAGED,0,,0"
            continue
        }
    }
    Say "session $s pid=$($p.Id) with_rive=$withRive rebuild=$Rebuild launch=$LaunchMode"
    $injOk = $false
    $shots = 0
    $deadline = (Get-Date).AddSeconds($SessionTimeoutSec)
    while ((Get-Date) -lt $deadline) {
        Get-ChildItem $ArtDir -Filter "${stem}_s${s}_*.shotreq" -ErrorAction SilentlyContinue | ForEach-Object {
            $req = (Get-Content $_.FullName -Raw).Trim()
            $target = $req; $hw = [IntPtr]::Zero
            if ($req -match '^(.*)\|(\d+)$') { $target = $Matches[1]; $hw = [IntPtr][long]$Matches[2] }
            Shot $target $p $hw
            $shots++
            Say "session $s screenshot $(Split-Path $target -Leaf)"
            Remove-Item $_.FullName -ErrorAction SilentlyContinue
        }
        if (Test-Path "${pre}_inject1.req") {
            Remove-Item "${pre}_inject1.req" -ErrorAction SilentlyContinue
            if ($injections -ge $MaxInjections) { Say "STOP: injection cap $MaxInjections reached"; New-Item -ItemType File "${pre}_inject1.failed" -Force | Out-Null; $stopAll = $true }
            else {
                $wait = $MinGapSec - ((Get-Date) - $lastInject).TotalSeconds
                if ($wait -gt 0) { Say ("session $s inject1 waits {0:N0} s (spacing)" -f $wait); Start-Sleep -Seconds ([math]::Ceiling($wait)) }
                $injOk = if ($DryRun) { Say "session $s inject1 DRY-RUN (no driver restart)"; $true } else { Inject "session $s inject1" }
                if ($injOk) { New-Item -ItemType File "${pre}_inject1.done" -Force | Out-Null }
                else {
                    New-Item -ItemType File "${pre}_inject1.failed" -Force | Out-Null
                    Say "STOP: session $s driver restart failed or adapter not OK within 30 s; INVALID_SYSTEM"
                    $stopAll = $true
                }
            }
        }
        if ((Test-Path $sum) -and (Select-String -Path $sum -Pattern '^verdict=' -Quiet)) { break }
        if ($p.HasExited) { Say "WARN session $s app exited early (exit code $($p.ExitCode))"; break }
        Start-Sleep -Milliseconds 200
    }
    Start-Sleep 2
    # Decide whether the app exited on its own before ending it (r2 L3).
    $exitedOnOwn = $p.HasExited
    if (-not $exitedOnOwn) { Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue }
    $kv = ReadKv $sum
    $v = if ($kv.ContainsKey("verdict")) { $kv["verdict"] } elseif (-not (Test-Path $sum)) { "INVALID_NO_SUMMARY" } elseif ($exitedOnOwn) { "CRASHED_NO_VERDICT" } else { "KILLED_BY_RUNNER" }
    if ($stopAll -and $v -notmatch '^(FAIL|CRASHED)') { $v = "STOPPED_" + $v }
    $errFile = "${pre}_stderr.txt"
    if (Test-Path $errFile) { Get-Content $errFile -Tail 20 | ForEach-Object { Say "session $s stderr| $_" } }
    $matrix += "$s,$($p.Id),$([int]$withRive),$v,$([int]$injOk),$($kv['rive.recovered']),$shots"
    Say "session $s verdict=$v with_rive=$withRive inject_ok=$([int]$injOk) shots=$shots exited_on_own=$exitedOnOwn"
}
Remove-Item env:MF_M3_SESSION, env:MF_M0_ARTDIR -ErrorAction SilentlyContinue
$matrix | Out-File (Join-Path $ArtDir "${prefix}_session_matrix.txt") -Encoding utf8
$bug = @(Get-WinEvent -FilterHashtable @{ LogName = 'System'; Id = 1001; StartTime = $runStart } -ErrorAction SilentlyContinue | Where-Object { $_.ProviderName -match 'BugCheck|WER-SystemErrorReporting' })
Say "injections=$injections bugcheck_events=$($bug.Count)"
if (Test-Path $composition) { Say ("composition.h sha256 POST=" + (Get-FileHash $composition -Algorithm SHA256).Hash.ToLower()) }
Say "RUN_DONE"
