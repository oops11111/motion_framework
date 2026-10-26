# Runs the M0 probes on the lab machine (M0-A: DESKTOP-K93N3P8; M0-B: Win11 machine).
# Collects data: run only after the ticket's gate is LOCKED and APPROVED.
#   powershell -ExecutionPolicy Bypass -File m0\run_m0.ps1 -DemoExe <...\MotionSoftDemo.exe> -DemoProj <project dir> [-Inject] [-Sessions 3] [-PresentMon <...\PresentMon.exe>] [-ArtDir <dir>]
param(
    [Parameter(Mandatory = $true)][string]$DemoExe,
    [switch]$Inject,
    [int]$Sessions = 3,
    [string]$PresentMon = "",
    [string]$ArtDir = "C:\Users\admin\motion_framework\artifacts",
    [string]$DemoProj = "C:\Users\admin\motion_framework\MotionSoftDemo",
    [int]$TearFramesPerArm = 30000,   # ~5 s per arm at the M0-A present rate, long enough for PresentMon
    [int]$SessionTimeoutSec = 300
)
$ErrorActionPreference = "Stop"
$M0 = $PSScriptRoot
$Out = Join-Path $M0 "out"
$Log = Join-Path $ArtDir "m0_run.log"
$demoProj = $DemoProj
New-Item -ItemType Directory -Force $ArtDir | Out-Null
function Say($m) { $l = "[{0:yyyy-MM-dd HH:mm:ss}] {1}" -f (Get-Date), $m; Write-Host $l; Add-Content $Log $l }
Add-Type -AssemblyName System.Drawing, System.Windows.Forms
Add-Type -Namespace M0 -Name Win -MemberDefinition @'
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct RECT { public int Left, Top, Right, Bottom; }
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool GetWindowRect(System.IntPtr h, out RECT r);
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool SetForegroundWindow(System.IntPtr h);
'@
# Captures only the app window (M0-A captured the whole desktop, which put browser
# tabs and account names into the repo). Falls back to the primary screen only if
# the window handle is unavailable, and says so in the log.
function Shot($path, $proc) {
    $b = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
    $h = if ($proc) { $proc.Refresh(); $proc.MainWindowHandle } else { [IntPtr]::Zero }
    $r = New-Object M0.Win+RECT
    if ($h -ne [IntPtr]::Zero -and [M0.Win]::GetWindowRect($h, [ref]$r) -and $r.Right -gt $r.Left) {
        [M0.Win]::SetForegroundWindow($h) | Out-Null; Start-Sleep -Milliseconds 300
        $b = New-Object System.Drawing.Rectangle $r.Left, $r.Top, ($r.Right - $r.Left), ($r.Bottom - $r.Top)
    } else { Say "WARN screenshot: no window handle, capturing primary screen" }
    $bmp = New-Object System.Drawing.Bitmap $b.Width, $b.Height
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($b.Location, [System.Drawing.Point]::Empty, $b.Size)
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
}
function Stop-Demo {
    Get-Process -Name MotionSoftDemo -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Get-Process -Name dotnet -ErrorAction SilentlyContinue | Where-Object {
        try { $_.MainWindowTitle -match 'MotionSoft' -or (Get-CimInstance Win32_Process -Filter "ProcessId=$($_.Id)" -EA SilentlyContinue).CommandLine -match 'MotionSoftDemo' } catch { $false }
    } | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep 2
}
function Write-LaunchSettings([int]$sess, [bool]$inj) {
    $injV = if ($inj) { "1" } else { "0" }
    $args = if ($inj) { "--m0-probe --m0-inject" } else { "--m0-probe" }
    $json = @"
{
  "profiles": {
    "MotionSoftDemo (Package)": {
      "commandName": "MsixPackage",
      "commandLineArgs": "$args",
      "environmentVariables": {
        "MF_M0_PROBE": "1",
        "MF_M0_SESSION": "$sess",
        "MF_M0_INJECT": "$injV",
        "MF_M0_ARTDIR": "$($ArtDir.Replace('\','\\'))"
      }
    }
  }
}
"@
    Set-Content (Join-Path $demoProj "Properties\launchSettings.json") -Value $json -Encoding utf8
}
function Stage-NativeDll {
    $dll = Join-Path $Out "m0rive.dll"
    foreach ($cfg in @("Debug","Release")) {
      $d = Join-Path $demoProj "bin\x64\$cfg\net10.0-windows10.0.26100.0\win-x64"
      if (Test-Path $d) {
        Copy-Item $dll $d -Force
        $appx = Join-Path $d "AppX"
        if (Test-Path $appx) { Copy-Item $dll $appx -Force }
      }
    }
}

Say "M0 run: host=$env:COMPUTERNAME sessions=$Sessions inject=$($Inject.IsPresent)"
$uuid = (Get-CimInstance Win32_ComputerSystemProduct).UUID
Say "machineId(SMBIOS UUID)=$uuid"
$os = Get-CimInstance Win32_OperatingSystem
Say "os=$($os.Caption) build=$($os.BuildNumber)"
$admin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
Say "elevated=$admin (PresentMon needs an elevated shell or Performance Log Users membership)"
Get-CimInstance Win32_VideoController | ForEach-Object { Say "gpu=$($_.Name) refresh=$($_.CurrentRefreshRate)Hz mode=$($_.CurrentHorizontalResolution)x$($_.CurrentVerticalResolution)" }
$composition = Join-Path (Split-Path $M0 -Parent) "code\composition.h"
if (Test-Path $composition) { Say ("composition.h sha256 PRE=" + (Get-FileHash $composition -Algorithm SHA256).Hash.ToLower()) }

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
& (Join-Path $Out "m0_tear_probe.exe") $tearDir $TearFramesPerArm | Out-File (Join-Path $tearDir "stdout.txt")
Say "tear probe exit=$LASTEXITCODE"
if ($pm) { Stop-Process -Id $pm.Id -ErrorAction SilentlyContinue }

$matrix = @("session,pid,verdict,refresh_hz,m0_1a_frames,m0_1a_cpu_p95_ms,m0_1a_gpu_p95_ms,m0_1b_frames,expr_latency_p50_ms,ui_latency_p50_ms,misalign_ms,misalign_px")
for ($s = 1; $s -le $Sessions; $s++) {
    $sum = Join-Path $ArtDir "m0_s${s}_summary.txt"
    Remove-Item $sum -ErrorAction SilentlyContinue
    Remove-Item (Join-Path $ArtDir "m0_s${s}_raw.txt") -ErrorAction SilentlyContinue
    Stop-Demo
    Set-Content (Join-Path $ArtDir "m0_session.flag") "$s" -Encoding ascii
    if ($Inject) { Set-Content (Join-Path $ArtDir "m0_inject.flag") "1" -Encoding ascii }
    else { Remove-Item (Join-Path $ArtDir "m0_inject.flag") -ErrorAction SilentlyContinue }
    Write-LaunchSettings $s $Inject.IsPresent
    Stage-NativeDll
    $env:MF_M0_PROBE = "1"; $env:MF_M0_SESSION = "$s"; $env:MF_M0_INJECT = $(if ($Inject) { "1" } else { "0" }); $env:MF_M0_ARTDIR = $ArtDir
    $env:WINAPP_CLI_TELEMETRY_OPTOUT = "1"
    $dotnetArgs = @('run','-c','Debug','-p:Platform=x64','--no-build','--launch-profile','MotionSoftDemo (Package)','--','--m0-probe')
    if ($Inject) { $dotnetArgs += '--m0-inject' }
    $p = Start-Process -FilePath 'dotnet' -ArgumentList $dotnetArgs -WorkingDirectory $demoProj -PassThru -WindowStyle Normal
    Say "session $s dotnetPid=$($p.Id)"
    $app = $null
    for ($i=0; $i -lt 90 -and -not $app; $i++) {
      Start-Sleep 1
      $app = Get-Process -Name MotionSoftDemo -ErrorAction SilentlyContinue | Select-Object -First 1
    }
    if ($app) { Say "session $s appPid=$($app.Id)" } else { Say "session $s WARN no MotionSoftDemo after 90s" }
    Start-Sleep 12
    Shot (Join-Path $ArtDir "m0_s${s}_screen_m0_1a.png") $app
    $deadline = (Get-Date).AddSeconds($SessionTimeoutSec)
    $ok = $false
    while ((Get-Date) -lt $deadline) {
      if ((Test-Path $sum) -and (Select-String -Path $sum -Pattern '^verdict=' -Quiet)) {
        $sessLine = Select-String -Path $sum -Pattern '^session=' | Select-Object -First 1
        if ($sessLine -and $sessLine.Line -eq "session=$s") { $ok = $true; break }
        Say "session $s WARN summary session mismatch: $($sessLine.Line)"
      }
      Start-Sleep 1
    }
    Start-Sleep 2
    if (-not $p.HasExited) { Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue }
    Stop-Demo
    if (Test-Path (Join-Path $demoProj "m0_ui.txt")) {
      Copy-Item (Join-Path $demoProj "m0_ui.txt") (Join-Path $ArtDir "m0_s${s}_ui.txt") -Force
    }
    $kv = @{}
    if (Test-Path $sum) { Get-Content $sum | ForEach-Object { if ($_ -match '^([^=]+)=(.*)$') { $kv[$Matches[1]] = $Matches[2] } } }
    $v = if ($kv.ContainsKey("verdict")) { $kv["verdict"] } else { "INVALID_NO_SUMMARY" }
    if ($kv.ContainsKey("session") -and $kv["session"] -ne "$s") { $v = "INVALID_SESSION_MISMATCH"; Say "session $s INVALID_SESSION_MISMATCH got=$($kv['session'])" }
    $appPid = if ($app) { $app.Id } else { $p.Id }
    $matrix += "$s,$appPid,$v,$($kv['refresh_hz']),$($kv['m0_1a.frames']),$($kv['m0_1a.cpu_p95_ms']),$($kv['m0_1a.gpu_p95_ms']),$($kv['m0_1b.frames']),$($kv['m0_35.expr.latency_p50_ms']),$($kv['m0_35.ui.latency_p50_ms']),$($kv['m0_2.misalign_ms']),$($kv['m0_2.misalign_px_at_drag_speed'])"
    Say "session $s verdict=$v frames=$($kv['m0_1a.frames'])"
}
Remove-Item env:MF_M0_PROBE, env:MF_M0_SESSION, env:MF_M0_INJECT, env:MF_M0_ARTDIR -ErrorAction SilentlyContinue
Remove-Item (Join-Path $ArtDir "m0_session.flag") -ErrorAction SilentlyContinue
$matrix | Out-File (Join-Path $ArtDir "m0_session_matrix.txt") -Encoding utf8
if (Test-Path $composition) { Say ("composition.h sha256 POST=" + (Get-FileHash $composition -Algorithm SHA256).Hash.ToLower()) }
Say "RUN_DONE"
