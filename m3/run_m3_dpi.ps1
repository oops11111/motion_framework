# M3-RIVE-DPI runner (docs/m3_rive_dpi_gate_LOCKED.md). Collects data: run only after
# the gate is LOCKED and APPROVED. Needs m0rive.dll (built by m0\build_m0.ps1, with
# m0r_resize) beside the exe and the Rive asset under <ArtDir>\m0_assets.
#   powershell -ExecutionPolicy Bypass -File m3\run_m3_dpi.ps1 -DemoExe <...\MotionSoftDemo.exe> -ArtDir <dir> [-Sessions 3]
# The operator switches Settings > Display > Scale between 100% and 125% whenever the
# app prompts (G1/G2) and otherwise leaves mouse and keyboard alone (G4).
param(
    [Parameter(Mandatory = $true)][string]$DemoExe,
    [Parameter(Mandatory = $true)][string]$ArtDir,
    [int]$Sessions = 3,
    [int]$SessionTimeoutSec = 1200
)
$ErrorActionPreference = "Stop"
$Log = Join-Path $ArtDir "m3dpi_run.log"
New-Item -ItemType Directory -Force $ArtDir | Out-Null
function Say($m) { $l = "[{0:yyyy-MM-dd HH:mm:ss}] {1}" -f (Get-Date), $m; Write-Host $l; Add-Content $Log $l }
Add-Type -AssemblyName System.Drawing, System.Windows.Forms
Add-Type -Namespace M3 -Name Win -MemberDefinition @'
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct RECT { public int Left, Top, Right, Bottom; }
[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool GetWindowRect(System.IntPtr h, out RECT r);
'@
# App window only (same reason as m0\run_m0.ps1: no desktop contents in the repo).
function Shot($path, $proc) {
    $proc.Refresh()
    $r = New-Object M3.Win+RECT
    if ($proc.MainWindowHandle -eq [IntPtr]::Zero -or -not [M3.Win]::GetWindowRect($proc.MainWindowHandle, [ref]$r)) { Say "WARN screenshot: no window"; return }
    $b = New-Object System.Drawing.Rectangle $r.Left, $r.Top, ($r.Right - $r.Left), ($r.Bottom - $r.Top)
    $bmp = New-Object System.Drawing.Bitmap $b.Width, $b.Height
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($b.Location, [System.Drawing.Point]::Empty, $b.Size)
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
}

Say "M3-DPI run: host=$env:COMPUTERNAME sessions=$Sessions exe=$DemoExe"
Say "machineId(SMBIOS UUID)=$((Get-CimInstance Win32_ComputerSystemProduct).UUID)"
$os = Get-CimInstance Win32_OperatingSystem
Say "os=$($os.Caption) build=$($os.BuildNumber)"
Get-CimInstance Win32_VideoController | ForEach-Object { Say "gpu=$($_.Name) refresh=$($_.CurrentRefreshRate)Hz mode=$($_.CurrentHorizontalResolution)x$($_.CurrentVerticalResolution)" }
$composition = Join-Path (Split-Path $PSScriptRoot -Parent) "code\composition.h"
if (Test-Path $composition) { Say ("composition.h sha256 PRE=" + (Get-FileHash $composition -Algorithm SHA256).Hash.ToLower()) }

$matrix = @("session,pid,verdict,switches_detected,size_ok,reraster_max_ms,budget_ok,stability_ok,private_mb_delta_100")
for ($s = 1; $s -le $Sessions; $s++) {
    $sum = Join-Path $ArtDir "m3dpi_s${s}_summary.txt"
    Remove-Item $sum, (Join-Path $ArtDir "m3dpi_s${s}_*.shotreq") -ErrorAction SilentlyContinue
    Get-Process -Name MotionSoftDemo -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep 2
    $env:MF_M3_DPI_PROBE = "1"; $env:MF_M3_SESSION = "$s"; $env:MF_M0_ARTDIR = $ArtDir
    $p = Start-Process -FilePath $DemoExe -ArgumentList @('--m3-dpi-probe') -WorkingDirectory (Split-Path $DemoExe -Parent) -PassThru -WindowStyle Normal
    Say "session $s pid=$($p.Id); start at 100% and follow the prompts in the app window"
    $deadline = (Get-Date).AddSeconds($SessionTimeoutSec)
    while ((Get-Date) -lt $deadline) {
        Get-ChildItem $ArtDir -Filter "m3dpi_s${s}_*.shotreq" -ErrorAction SilentlyContinue | ForEach-Object {
            $target = (Get-Content $_.FullName -Raw).Trim()
            Shot $target $p
            Say "session $s screenshot $(Split-Path $target -Leaf)"
            Remove-Item $_.FullName -ErrorAction SilentlyContinue
        }
        if ((Test-Path $sum) -and (Select-String -Path $sum -Pattern '^verdict=' -Quiet)) { break }
        if ($p.HasExited) { Say "WARN session $s app exited early"; break }
        Start-Sleep -Milliseconds 200
    }
    Start-Sleep 2
    if (-not $p.HasExited) { Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue }
    $kv = @{}
    if (Test-Path $sum) { Get-Content $sum | ForEach-Object { if ($_ -match '^([^=]+)=(.*)$') { $kv[$Matches[1]] = $Matches[2] } } }
    $v = if ($kv.ContainsKey("verdict")) { $kv["verdict"] } else { "INVALID_NO_SUMMARY" }
    $det = ($kv.Keys | Where-Object { $_ -match '^switch\d+\.detected$' -and $kv[$_] -eq '1' }).Count
    $sizeOk = if (($kv.Keys | Where-Object { $_ -match '^switch\d+\.size_ok$' -and $kv[$_] -ne '1' }).Count -eq 0 -and $det -gt 0) { 1 } else { 0 }
    $matrix += "$s,$($p.Id),$v,$det,$sizeOk,$($kv['reraster_max_ms']),$($kv['budget_ok']),$($kv['stability_ok']),$($kv['private_mb_delta_100'])"
    Say "session $s verdict=$v switches=$det size_ok=$sizeOk"
}
Remove-Item env:MF_M3_DPI_PROBE, env:MF_M3_SESSION, env:MF_M0_ARTDIR -ErrorAction SilentlyContinue
$matrix | Out-File (Join-Path $ArtDir "m3dpi_session_matrix.txt") -Encoding utf8
if (Test-Path $composition) { Say ("composition.h sha256 POST=" + (Get-FileHash $composition -Algorithm SHA256).Hash.ToLower()) }
Say "RUN_DONE"
