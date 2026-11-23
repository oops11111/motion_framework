#Requires -Version 5.1
<#
.SYNOPSIS
    PERF-CI capture harness (tooling only): one demo run with PresentMon + logman ETL + PresentStats ledger.

.DESCRIPTION
    Starts PresentMon (operator-supplied 2.x exe) filtered to the demo process,
    starts a logman trace for the FluidMotion-Motion ETW provider, launches the
    demo with the given arguments, waits for it to exit (or the timeout), stops
    both captures, and writes the artefacts the offline grader needs:
    presentmon.csv, trace.etl, demolog.txt (copy of the demo log carrying the
    ETW_EVENT name=PresentStats line), run.json, and manifest.fragment.json.

    BOUNDARY: tooling only. This script does not authorize data collection for
    a gate verdict, does not lift the PERF-CI HOLD, does not modify the LOCKED
    PERF-CI gate or any frozen source file, and never emits PASS/FAIL itself.
    Grading is done afterwards, offline, by perf_ci_grade.py. A second physical
    machine and the r2 gate (Q1-Q8 rulings) are still required before any
    PERF-CI round counts. See tools/perf_ci/README.md.

    REQUIREMENTS: Windows 10/11; PresentMon 2.x exe supplied via -PresentMon
    (not vendored in this repo); elevation OR Performance Log Users membership
    for logman (a denial is recorded as denied.txt and the run aborts);
    the demo exe built from MotionSoftDemo.csproj.

    EXIT CODES: 0 artefacts complete; 1 demo timeout (demo stopped,
    artefacts still collected); 2 capture setup failure; 3 parameter
    error; 4 required artefact missing. -DryRun exits 0.

.PARAMETER DemoExe
    Full path to the demo exe (e.g. MotionSoftDemo.exe).

.PARAMETER DemoArgs
    Arguments passed to the demo, e.g. "--m2-02". One scenario per invocation.

.PARAMETER PresentMon
    Full path to PresentMon.exe (2.x). Mandatory: a run without a PresentMon
    CSV is INVALID by the gate, so a missing exe is a hard error, not a skip.

.PARAMETER OutDir
    Directory receiving the artefacts. Created if missing.

.PARAMETER RefreshHz
    Display refresh rate in Hz for the run. Mandatory, no default: the grader
    derives the frame budget T = 1000/Hz from it, so it must be stated.

.PARAMETER Scenario
    One of: idle, steady_play, interrupted_retarget (LOCKED gate scenarios).

.PARAMETER Round
    Round number within the scenario (default 1). PIDs must differ per round.

.PARAMETER GpuTier
    One of: igpu, dgpu, lowend-laptop.

.PARAMETER TimeoutSec
    Seconds to wait for the demo to exit on its own (default 300). On timeout
    the demo is stopped, the captures are still collected, and the exit code
    is 1 with timedOut=true in run.json.

.PARAMETER DemoLogDir
    Directory holding the demo UI log. Defaults to the same resolution the
    demo itself uses (DevLog.Directory): $env:MF_M0_ARTDIR if set, else
    $env:TEMP\z3a_cabi_l1_dev, else the demo exe directory.

.PARAMETER DemoLogFile
    Explicit demo log path. Overrides -DemoLogDir discovery.

.PARAMETER DryRun
    Print the planned commands and artefact paths, create nothing, exit 0.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File tools\perf_ci\capture_run.ps1 `
        -DemoExe C:\build\MotionSoftDemo.exe -DemoArgs "--m2-02" `
        -PresentMon C:\tools\PresentMon.exe -OutDir C:\perf\r1 `
        -RefreshHz 120 -Scenario steady_play -Round 1 -GpuTier igpu
#>
[CmdletBinding()]
param(
    [string]$DemoExe = "",
    [string]$DemoArgs = "",
    [string]$PresentMon = "",
    [string]$OutDir = "",
    [double]$RefreshHz = 0,
    [string]$Scenario = "",
    [int]$Round = 1,
    [string]$GpuTier = "",
    [int]$TimeoutSec = 300,
    [string]$DemoLogDir = "",
    [string]$DemoLogFile = "",
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"
$ProviderGuid = "8f3a2c1d-6b4e-4a91-9c7d-2e5f8a1b3c4d"
$TraceName = "FluidMotionPerfCapture"
$Scenarios = @("idle", "steady_play", "interrupted_retarget")
$GpuTiers = @("igpu", "dgpu", "lowend-laptop")

function Say([string]$msg) {
    Write-Host ("[{0:HH:mm:ss}] {1}" -f (Get-Date), $msg)
}

# WinPS 5.1 Set-Content -Encoding UTF8 writes a BOM, which breaks the
# grader's manifest read. All harness outputs are written BOM-less.
function Write-Utf8NoBom([string]$path, [string]$text) {
    [System.IO.File]::WriteAllText($path, $text, (New-Object System.Text.UTF8Encoding($false)))
}

function Fail([string]$msg, [int]$code) {
    Say "ERROR: $msg"
    exit $code
}

if ([string]::IsNullOrWhiteSpace($DemoExe)) { Fail "missing -DemoExe" 3 }
if ([string]::IsNullOrWhiteSpace($PresentMon)) { Fail "missing -PresentMon (PresentMon 2.x exe path is mandatory)" 3 }
if ([string]::IsNullOrWhiteSpace($OutDir)) { Fail "missing -OutDir" 3 }
if ($RefreshHz -le 0) { Fail "missing -RefreshHz (display refresh rate in Hz, no default)" 3 }
if ($Scenarios -notcontains $Scenario) { Fail "bad -Scenario '$Scenario' (want: $($Scenarios -join ', '))" 3 }
if ($GpuTiers -notcontains $GpuTier) { Fail "bad -GpuTier '$GpuTier' (want: $($GpuTiers -join ', '))" 3 }
if ($Round -lt 1) { Fail "-Round must be >= 1" 3 }
if ($TimeoutSec -lt 10) { Fail "-TimeoutSec must be >= 10" 3 }

$demoExeResolved = $DemoExe
$presentMonResolved = $PresentMon
if (-not $DryRun) {
    if (-not (Test-Path $demoExeResolved)) { Fail "demo exe not found: $demoExeResolved" 3 }
    if (-not (Test-Path $presentMonResolved)) { Fail "PresentMon exe not found: $presentMonResolved" 3 }
}

$csvPath = Join-Path $OutDir "presentmon.csv"
$etlPath = Join-Path $OutDir "trace.etl"
$demoLogCopy = Join-Path $OutDir "demolog.txt"
$runJsonPath = Join-Path $OutDir "run.json"
$fragmentPath = Join-Path $OutDir "manifest.fragment.json"
$deniedPath = Join-Path $OutDir "denied.txt"
$exeName = [System.IO.Path]::GetFileName($demoExeResolved)
$demoDir = [System.IO.Path]::GetDirectoryName((Resolve-Path $demoExeResolved -ErrorAction SilentlyContinue).Path)
if ([string]::IsNullOrWhiteSpace($demoDir)) { $demoDir = [System.IO.Path]::GetDirectoryName($demoExeResolved) }
if ([string]::IsNullOrWhiteSpace($DemoLogDir)) {
    # Mirror the demo's own DevLog.Directory resolution order exactly.
    if (-not [string]::IsNullOrWhiteSpace($env:MF_M0_ARTDIR)) { $DemoLogDir = $env:MF_M0_ARTDIR }
    elseif (-not [string]::IsNullOrWhiteSpace($env:TEMP)) { $DemoLogDir = Join-Path $env:TEMP "z3a_cabi_l1_dev" }
    else { $DemoLogDir = $demoDir }
}

Say "demo=$demoExeResolved args='$DemoArgs' scenario=$Scenario round=$Round refreshHz=$RefreshHz gpuTier=$GpuTier timeout=${TimeoutSec}s"
Say "presentmon=$presentMonResolved outdir=$OutDir"

if ($DryRun) {
    Say "DRY-RUN: would start PresentMon --process_name $exeName --output_file $csvPath"
    Say "DRY-RUN: would start logman trace $TraceName -p {$ProviderGuid} -o $etlPath"
    Say "DRY-RUN: would launch demo, wait up to ${TimeoutSec}s, stop captures, copy newest demo log from $DemoLogDir to $demoLogCopy"
    Say "DRY-RUN: would write $runJsonPath and $fragmentPath"
    exit 0
}

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

# Elevation is not strictly required (Performance Log Users membership also
# works for logman), but record the fact either way; a logman denial below is
# the authoritative signal and is kept as evidence.
$principal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
$isAdmin = $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
Say "elevated=$isAdmin (logman needs an elevated shell or Performance Log Users membership)"

$machineId = ""
try {
    $machineId = (Get-CimInstance Win32_ComputerSystemProduct -ErrorAction Stop).UUID
} catch {
    Say "WARN: SMBIOS UUID unreadable ($($_.Exception.Message)); machineId left empty (grader will judge INVALID)"
}
$hostname = $env:COMPUTERNAME
if ([string]::IsNullOrWhiteSpace($hostname)) {
    try { $hostname = (hostname) } catch { $hostname = "" }
    if ([string]::IsNullOrWhiteSpace($hostname)) { Say "WARN: hostname unreadable; machineId/hostname left empty (grader will judge INVALID)" }
}

$pm = $null
$traceStarted = $false
$logmanFailed = $false
$demo = $null
$timedOut = $false
$demoStart = Get-Date

try {
    $pm = Start-Process $presentMonResolved -PassThru -WindowStyle Hidden -ArgumentList @(
        "--process_name", $exeName, "--output_file", $csvPath)
    Say "PresentMon pid=$($pm.Id)"

    $logmanOut = & logman create trace $TraceName -p "{$ProviderGuid}" -o "$etlPath" -ets 2>&1
    if ($LASTEXITCODE -ne 0) {
        # Retry once after deleting a stale session of the same name.
        & logman delete $TraceName 2>&1 | Out-Null
        $logmanOut = & logman create trace $TraceName -p "{$ProviderGuid}" -o "$etlPath" -ets 2>&1
    }
    if ($LASTEXITCODE -ne 0) {
        $evidence = "logman create trace failed (exit $LASTEXITCODE). Command: logman create trace $TraceName -p {$ProviderGuid} -o $etlPath -ets`r`nOutput:`r`n$($logmanOut | Out-String)"
        Write-Utf8NoBom $deniedPath $evidence
        Say "logman denied/failed; evidence written to $deniedPath. EventPipe fallback (dotnet-trace + denial evidence) is a manual step per the gate review; this script does not take it silently."
        $logmanFailed = $true
    } else {
        $traceStarted = $true
        Say "logman trace $TraceName started"
    }

    if (-not $logmanFailed) {
        $startArgs = @{ FilePath = $demoExeResolved; PassThru = $true; WorkingDirectory = $demoDir }
        if (-not [string]::IsNullOrWhiteSpace($DemoArgs)) { $startArgs["ArgumentList"] = $DemoArgs }
        $demo = Start-Process @startArgs
        Say "demo pid=$($demo.Id) started at $demoStart"

        $exited = $demo.WaitForExit($TimeoutSec * 1000)
        if (-not $exited) {
            $timedOut = $true
            Say "WARN: demo did not exit within ${TimeoutSec}s; stopping pid $($demo.Id)"
            Stop-Process -Id $demo.Id -Force -ErrorAction SilentlyContinue
            Start-Sleep -Seconds 2
        } else {
            Say "demo exited code=$($demo.ExitCode)"
        }
    }
} finally {
    if ($traceStarted) {
        & logman stop $TraceName -ets 2>&1 | Out-Null
        & logman delete $TraceName 2>&1 | Out-Null
        Say "logman trace stopped"
    }
    if ($pm -ne $null -and -not $pm.HasExited) {
        # PresentMon exits by itself once the target process exits; allow a
        # flush window, then stop it rather than leaving a stray capture.
        if (-not $pm.WaitForExit(30000)) {
            Say "WARN: PresentMon still running 30s after demo exit; stopping pid $($pm.Id)"
            Stop-Process -Id $pm.Id -Force -ErrorAction SilentlyContinue
        }
    }
}

# --- Artefact verification -------------------------------------------------
$csvRows = 0
if (Test-Path $csvPath) { $csvRows = ((Get-Content $csvPath -ErrorAction SilentlyContinue | Measure-Object -Line).Lines - 1); if ($csvRows -lt 0) { $csvRows = 0 } }
$etlBytes = 0
if (Test-Path $etlPath) { $etlBytes = (Get-Item $etlPath).Length }

$demoLogFound = ""
$presentStatsLine = ""
if (-not [string]::IsNullOrWhiteSpace($DemoLogFile)) {
    if (Test-Path $DemoLogFile) { $demoLogFound = $DemoLogFile }
} else {
    $candidates = Get-ChildItem -Path $DemoLogDir -Filter "*.txt" -ErrorAction SilentlyContinue |
        Where-Object { $_.LastWriteTime -ge $demoStart } |
        Sort-Object LastWriteTime -Descending
    if ($candidates) { $demoLogFound = $candidates[0].FullName }
}
if ($demoLogFound -ne "") {
    Copy-Item $demoLogFound $demoLogCopy -Force
    $hits = Select-String -Path $demoLogCopy -Pattern "ETW_EVENT name=PresentStats id=4 " -ErrorAction SilentlyContinue
    if ($hits) { $presentStatsLine = $hits[-1].Line }
    Say "demo log: $demoLogFound"
} else {
    Say "WARN: no demo log written after demo start was found in $DemoLogDir; presentStats will be missing (grader: INVALID)"
}
if ($presentStatsLine -ne "") { Say "PresentStats: $presentStatsLine" }
else { Say "WARN: no ETW_EVENT name=PresentStats line in the demo log" }

Say "artefacts: presentmon.csv rows=$csvRows, trace.etl bytes=$etlBytes, demolog present=$([bool](Test-Path $demoLogCopy))"

# A run that collected nothing must not report success. A header-only CSV
# (0 rows) is not missing: the idle scenario legitimately presents nothing.
$missingArtifacts = @()
if (-not (Test-Path $csvPath)) { $missingArtifacts += "presentmonCsv" }
if (-not (Test-Path $etlPath) -or $etlBytes -eq 0) { $missingArtifacts += "trace" }
if (-not (Test-Path $demoLogCopy) -or [string]::IsNullOrWhiteSpace($presentStatsLine)) { $missingArtifacts += "presentStats" }
if ($missingArtifacts) { Say "WARN: required artefacts missing: $($missingArtifacts -join ', ') (grader: INVALID)" }

$runMeta = [ordered]@{
    capturedAt    = (Get-Date).ToString("o")
    scenario      = $Scenario
    round         = $Round
    pid           = $(if ($demo) { $demo.Id } else { 0 })
    refreshHz     = $RefreshHz
    demoExe       = $demoExeResolved
    demoArgs      = $DemoArgs
    timedOut      = $timedOut
    presentmonCsv = "presentmon.csv"
    presentmonRows = $csvRows
    trace         = "trace.etl"
    traceBytes    = $etlBytes
    presentStats  = "demolog.txt"
    presentStatsLine = $presentStatsLine
    demoLogSource = $demoLogFound
    demoLogDir = $DemoLogDir
    missingArtifacts = $missingArtifacts
}
Write-Utf8NoBom $runJsonPath ($runMeta | ConvertTo-Json -Depth 3)

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..") -ErrorAction SilentlyContinue
$gatePath = ""
$approvedPath = ""
if ($repoRoot) {
    $gatePath = Join-Path $repoRoot.Path "docs\m2_perf_ci_gate_LOCKED.md"
    $approvedPath = Join-Path $repoRoot.Path "docs\m2_perf_ci_approved.md"
}
$fragment = [ordered]@{
    gate         = $gatePath
    approvedFile = $approvedPath
    machines     = @(
        [ordered]@{
            hostname      = $hostname
            machineId     = $machineId
            gpuTier       = $GpuTier
            physical      = $true
            captureMethod = "logman-etl"
            runs          = @(
                [ordered]@{
                    scenario     = $Scenario
                    round        = $Round
                    pid          = $(if ($demo) { $demo.Id } else { 0 })
                    refreshHz    = $RefreshHz
                    presentmonCsv = "presentmon.csv"
                    trace        = "trace.etl"
                    presentStats = "demolog.txt"
                    stageP95Ms   = $null
                    reconcileNote = ""
                }
            )
        }
    )
}
Write-Utf8NoBom $fragmentPath ($fragment | ConvertTo-Json -Depth 5)
Say "wrote $runJsonPath and $fragmentPath"
Say "NOTE: stageP95Ms is null (WPA stage split not defined yet, Q6 open) and reconcileNote is empty: the grader will judge this fragment INVALID until those are filled and an APPROVED file exists. That is the intended gate behaviour, not a harness failure."

if ($logmanFailed) { exit 2 }
if ($missingArtifacts) { exit 4 }
if ($timedOut) { exit 1 }
exit 0
