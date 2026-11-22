param(
    [Parameter(Mandatory=$true)][string]$ExePath,
    [Parameter(Mandatory=$true)][ValidateSet('Fake','Real')][string]$Mode,
    [Parameter(Mandatory=$true)][string]$StatePath,
    [Parameter(Mandatory=$true)][string]$StdoutPath,
    [Parameter(Mandatory=$true)][string]$StderrPath
)
$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = New-Object System.Text.UTF8Encoding($false)
Add-Type -TypeDefinition @'
using System;
using System.Text;
using System.Runtime.InteropServices;
public static class F5PackageContext {
    [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
    private static extern int GetCurrentPackageFullName(ref int length, StringBuilder name);
    public static string FullName() {
        int length=0;
        int error=GetCurrentPackageFullName(ref length, null);
        if (error!=122) throw new InvalidOperationException("No package identity, error="+error);
        var text=new StringBuilder(length);
        error=GetCurrentPackageFullName(ref length,text);
        if (error!=0) throw new InvalidOperationException("Package identity query failed, error="+error);
        return text.ToString();
    }
}
'@
$identity = [F5PackageContext]::FullName()
$env:PROCESSOR_ARCHITECTURE = 'AMD64' # This fresh diagnostic process and its child only.
if ($Mode -eq 'Fake') { $env:FM_PILOT_F5 = '1' }
else { Remove-Item Env:FM_PILOT_F5 -ErrorAction SilentlyContinue }
$process = Start-Process -FilePath $ExePath -WorkingDirectory (Split-Path -LiteralPath $ExePath) -WindowStyle Hidden -PassThru -RedirectStandardOutput $StdoutPath -RedirectStandardError $StderrPath
$state = [ordered]@{ pid=$process.Id; packageFullName=$identity; mode=$Mode; diagnosticContext=$true; status='running' }
$state | ConvertTo-Json -Compress | Set-Content -LiteralPath $StatePath -Encoding UTF8
$deadline = [DateTime]::UtcNow.AddSeconds(90)
while (-not $process.WaitForExit(1000)) {
    if ([DateTime]::UtcNow -gt $deadline) {
        $state.status = 'cleanup-pending'
        $state | ConvertTo-Json -Compress | Set-Content -LiteralPath $StatePath -Encoding UTF8
        exit 2 # Do not terminate a process or remove a package on a watchdog timeout.
    }
}
$state.status = 'exited'
$state.exitCode = $process.ExitCode
$state | ConvertTo-Json -Compress | Set-Content -LiteralPath $StatePath -Encoding UTF8
exit $process.ExitCode
