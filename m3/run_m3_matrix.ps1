# M3-MATRIX runner (docs/m3_matrix_gate_LOCKED.md). Collects data: run only after the gate
# is LOCKED and APPROVED. Needs two monitors (X1), m0rive.dll beside the exe and the Rive
# asset under <ArtDir>\m0_assets. No admin rights, no driver restarts, no pointer injection:
# the probe moves its own window between the monitors.
#   powershell -ExecutionPolicy Bypass -File m3\run_m3_matrix.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <dir> [-Sessions 3]
#   ... -Pacing   # M3-MATRIX-PACING (docs/m3_matrix_pacing_gate_LOCKED.md): render loop paced to the
#                 # window's monitor; files m3matrixpacing_*; results go to m3/results_matrix_pacing/
param(
    [Parameter(Mandatory = $true)][string]$DemoExe,
    [Parameter(Mandatory = $true)][string]$ArtDir,
    [int]$Sessions = 3,
    [switch]$Pacing,
    [int]$SessionTimeoutSec = 600
)
# Continue, not Stop: PowerShell 5.1 turns a native tool's stderr into a terminating error.
$ErrorActionPreference = "Continue"
New-Item -ItemType Directory -Force $ArtDir | Out-Null
$stem = if ($Pacing) { "m3matrixpacing" } else { "m3mx" }
$Log = Join-Path $ArtDir "${stem}_run.log"
function Say($m) { $l = "[{0:yyyy-MM-dd HH:mm:ss}] {1}" -f (Get-Date), $m; Write-Host $l; Add-Content $Log $l }
Add-Type -AssemblyName System.Drawing, System.Windows.Forms
Add-Type -Namespace M3X -Name Win -MemberDefinition @'
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct RECT { public int Left, Top, Right, Bottom; }
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool GetWindowRect(System.IntPtr h, out RECT r);
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool IsWindow(System.IntPtr h);
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool PrintWindow(System.IntPtr h, System.IntPtr hdc, uint flags);
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool SetProcessDpiAwarenessContext(System.IntPtr ctx);
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern System.IntPtr SetThreadDpiAwarenessContext(System.IntPtr ctx);
'@
# S1: physical-pixel window rects on scaled displays (K93N3P8 runs at 125%); without this,
# GetWindowRect returns virtualised coordinates. -4 = DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2.
# If the host already fixed its process awareness, the per-thread call in Shot still applies.
if (-not [M3X.Win]::SetProcessDpiAwarenessContext([IntPtr]-4)) { Say "note: process DPI awareness already set; using per-thread awareness for screenshots" }
# App window only (same reason as m0\run_m0.ps1: no desktop contents in the repo).
# $hwnd (optional) names the window; otherwise the process's main window (XAML-REBUILD).
# S1: PrintWindow with PW_RENDERFULLCONTENT (2) asks the window for its own content,
# including DirectComposition/XAML output, so windows on top of it are never captured.
function Shot($path, $proc, $hwnd = [IntPtr]::Zero) {
    $proc.Refresh()
    [void][M3X.Win]::SetThreadDpiAwarenessContext([IntPtr]-4)
    $h = if ($hwnd -ne [IntPtr]::Zero) { $hwnd } else { $proc.MainWindowHandle }
    $r = New-Object M3X.Win+RECT
    if ($h -eq [IntPtr]::Zero -or -not [M3X.Win]::IsWindow($h) -or -not [M3X.Win]::GetWindowRect($h, [ref]$r)) { Say "WARN screenshot: no window"; return }
    $bmp = New-Object System.Drawing.Bitmap ($r.Right - $r.Left), ($r.Bottom - $r.Top)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $hdc = $g.GetHdc()
    $ok = [M3X.Win]::PrintWindow($h, $hdc, 2)
    $g.ReleaseHdc($hdc)
    if (-not $ok) { Say "WARN PrintWindow failed for $(Split-Path $path -Leaf)" }
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
}
function ReadKv($sum) {
    $kv = @{}
    if (Test-Path $sum) { Get-Content $sum | ForEach-Object { if ($_ -match '^([^=]+)=(.*)$') { $kv[$Matches[1]] = $Matches[2] } } }
    return $kv
}

Say "M3-MATRIX run: host=$env:COMPUTERNAME sessions=$Sessions pacing=$Pacing exe=$DemoExe"
Say "machineId(SMBIOS UUID)=$((Get-CimInstance Win32_ComputerSystemProduct).UUID)"
$os = Get-CimInstance Win32_OperatingSystem
Say "os=$($os.Caption) build=$($os.BuildNumber)"
Get-CimInstance Win32_VideoController | ForEach-Object { Say "gpu=$($_.Name) driver=$($_.DriverVersion) refresh=$($_.CurrentRefreshRate)Hz mode=$($_.CurrentHorizontalResolution)x$($_.CurrentVerticalResolution)" }
Add-Type -AssemblyName System.Windows.Forms
[System.Windows.Forms.Screen]::AllScreens | ForEach-Object { Say "screen $($_.DeviceName) primary=$($_.Primary) bounds=$($_.Bounds)" }
$composition = Join-Path (Split-Path $PSScriptRoot -Parent) "code\composition.h"
if (Test-Path $composition) { Say ("composition.h sha256 PRE=" + (Get-FileHash $composition -Algorithm SHA256).Hash.ToLower()) }

$matrix = @("session,pid,verdict,monitors,mixed_refresh,mixed_scale,pacing_ok,budget_ok,stability_ok,size_ok,hitch_ok,scale_changes,col_primary_ok,col_primary_dev_p95_ms,col_primary_share,col_external_ok,col_external_dev_p95_ms,col_external_share")
for ($s = 1; $s -le $Sessions; $s++) {
    $pre = Join-Path $ArtDir "${stem}_s${s}"
    $sum = "${pre}_summary.txt"
    Remove-Item $sum, "${pre}_stderr.txt", "${pre}_*.shotreq" -ErrorAction SilentlyContinue
    Get-Process -Name MotionSoftDemo -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep 2
    $env:MF_M3_MATRIX_PROBE = "1"; $env:MF_M3_SESSION = "$s"; $env:MF_M0_ARTDIR = $ArtDir
    # The data folder and session also go as arguments: the lab found the environment did not
    # reach the app (M3-MATRIX lab note 1).
    $appArgs = @($(if ($Pacing) { '--m3-matrix-pacing' } else { '--m3-matrix-probe' }), "--m3-artdir=$ArtDir", "--m3-session=$s")
    $p = Start-Process -FilePath $DemoExe -ArgumentList $appArgs -WorkingDirectory (Split-Path $DemoExe -Parent) -PassThru -WindowStyle Normal
    Say "session $s pid=$($p.Id)"
    $deadline = (Get-Date).AddSeconds($SessionTimeoutSec)
    while ((Get-Date) -lt $deadline) {
        Get-ChildItem $ArtDir -Filter "${stem}_s${s}_*.shotreq" -ErrorAction SilentlyContinue | ForEach-Object {
            $target = (Get-Content $_.FullName -Raw).Trim()
            Shot $target $p
            Say "session $s screenshot $(Split-Path $target -Leaf)"
            Remove-Item $_.FullName -ErrorAction SilentlyContinue
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
    $matrix += "$s,$($p.Id),$v,$($kv['monitors']),$($kv['mixed_refresh']),$($kv['mixed_scale']),$($kv['pacing_ok']),$($kv['budget_ok']),$($kv['stability_ok']),$($kv['size_ok']),$($kv['hitch_ok']),$($kv['scale_changes']),$($kv['col_primary.ok']),$($kv['col_primary.dev_p95_ms']),$($kv['col_primary.dev_over_half_T_share']),$($kv['col_external.ok']),$($kv['col_external.dev_p95_ms']),$($kv['col_external.dev_over_half_T_share'])"
    Say "session $s verdict=$v pacing_ok=$($kv['pacing_ok']) hitch_ok=$($kv['hitch_ok']) size_ok=$($kv['size_ok']) budget_ok=$($kv['budget_ok'])"
    if ($v -eq "BLOCKED_HARDWARE") { Say "STOP: the probe needs exactly two monitors (X1)"; break }
}
Remove-Item env:MF_M3_MATRIX_PROBE, env:MF_M3_SESSION, env:MF_M0_ARTDIR -ErrorAction SilentlyContinue
$matrix | Out-File (Join-Path $ArtDir "${stem}_session_matrix.txt") -Encoding utf8
if (Test-Path $composition) { Say ("composition.h sha256 POST=" + (Get-FileHash $composition -Algorithm SHA256).Hash.ToLower()) }
Say "RUN_DONE"
