param(
    [Parameter(Mandatory=$true)][string]$AppUserModelId,
    [Parameter(Mandatory=$true)][string]$StatePath,
    [string]$ActivationArguments = '',
    [switch]$Execute,
    [switch]$ValidateOnly,
    [string]$AuthorityFile,
    [string]$BindingFile,
    [string]$InstructionFile
)
$ErrorActionPreference = 'Stop'
if (-not $Execute -and -not $ValidateOnly) { @{mode='dryrun';aumid=$AppUserModelId;activation='ordinary; not executed'}|ConvertTo-Json; return }
if (-not (Test-Path -LiteralPath $AuthorityFile) -or -not (Test-Path -LiteralPath $InstructionFile)) { throw 'F2 authority context and primary instruction required' }
if(-not $BindingFile){throw 'Build/layout binding required'}
. "$PSScriptRoot/RuntimeGuard.ps1"
$binding=Assert-F2Authority $AuthorityFile $InstructionFile $BindingFile ([bool]$ValidateOnly)
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
[ComImport, Guid("2e941141-7f97-4756-ba1d-9decde894a3d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface F2NormalActivationManager {
    [PreserveSig] int ActivateApplication([MarshalAs(UnmanagedType.LPWStr)] string id,
        [MarshalAs(UnmanagedType.LPWStr)] string arguments, uint options, out uint pid);
}
public static class F2NormalActivation {
    public static uint Launch(string id, string arguments) {
        object instance=Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("45ba127d-10a8-46ea-8ab7-56ea9078943c")));
        try {
            uint pid;
            // AO_NOERRORUI only; no debug/design flags or package-debug settings.
            Marshal.ThrowExceptionForHR(((F2NormalActivationManager)instance).ActivateApplication(id, arguments, 2, out pid));
            return pid;
        } finally { Marshal.ReleaseComObject(instance); }
    }
}
'@
if ($ValidateOnly) {
    [ordered]@{mode='ValidateOnly';auth_guard=$true;COM_compiled=$true;activation='NOT_EXECUTED';package_query='NOT_EXECUTED';registration='NOT_EXECUTED'}|ConvertTo-Json
    return
}
$packages=@(Get-AppxPackage -Name 49462fluenthub-uwp.FluentHub)
if ($packages.Count -ne 1 -or $packages[0].Publisher -ne 'CN=4E3C0825-4D00-4ECC-89F8-528E9200B125' -or $packages[0].Version.ToString() -ne '0.11.0.0' -or $AppUserModelId -ne ($packages[0].PackageFamilyName+'!App') -or @('','--fm-pilot-f5=1') -cnotcontains $ActivationArguments) { throw 'Activation not bound to expected 49462fluenthub-uwp.FluentHub App/allowed empty or exact Fake arguments' }
$launchedId = [F2NormalActivation]::Launch($AppUserModelId, $ActivationArguments)
[ordered]@{ pid=$launchedId; activationArguments=$ActivationArguments; diagnosticContext=$false; activation='IApplicationActivationManager, AO_NOERRORUI' } |
    ConvertTo-Json -Compress | Set-Content -LiteralPath $StatePath -Encoding UTF8
