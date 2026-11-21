param(
    [Parameter(Mandatory=$true)][string]$AppUserModelId,
    [Parameter(Mandatory=$true)][string]$StatePath,
    [string]$ActivationArguments = '',
    [switch]$Execute,
    [switch]$ValidateOnly,
    [string]$AuthorityFile,
    [string]$InstructionFile
)
$ErrorActionPreference = 'Stop'
if (-not $Execute -and -not $ValidateOnly) { @{mode='dryrun';aumid=$AppUserModelId;activation='ordinary; not executed'}|ConvertTo-Json; return }
if (-not (Test-Path -LiteralPath $AuthorityFile) -or -not (Test-Path -LiteralPath $InstructionFile)) { throw 'Same-FilesDev authority context and primary instruction required' }
$a=Get-Content -LiteralPath $AuthorityFile -Raw -Encoding UTF8|ConvertFrom-Json
$i=Get-Content -LiteralPath $InstructionFile -Raw -Encoding UTF8|ConvertFrom-Json
if ($a.grant_kind -ne 'F9a FilesDev selected-project deployment, ordinary startup, exact cleanup' -or $a.source -ne 'Direct user response in current root conversation' -or [string]::IsNullOrWhiteSpace($a.user_verbatim)) { throw 'Not a existing direct same-FilesDev grant' }
$hasher=[System.Security.Cryptography.SHA256]::Create()
try { $authorityHash=[BitConverter]::ToString($hasher.ComputeHash([System.IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $AuthorityFile).Path))).Replace('-','') }
finally { $hasher.Dispose() }
if ($i.instruction_kind -ne 'Primary GateM execute' -or [string]::IsNullOrWhiteSpace($i.root_verbatim) -or $i.authority_sha256.ToUpperInvariant() -ne $authorityHash) { throw 'Primary M instruction not bound to this grant' }
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
[ComImport, Guid("2e941141-7f97-4756-ba1d-9decde894a3d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface FilesNormalActivationManager {
    [PreserveSig] int ActivateApplication([MarshalAs(UnmanagedType.LPWStr)] string id,
        [MarshalAs(UnmanagedType.LPWStr)] string arguments, uint options, out uint pid);
}
public static class FilesNormalActivation {
    public static uint Launch(string id, string arguments) {
        object instance=Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("45ba127d-10a8-46ea-8ab7-56ea9078943c")));
        try {
            uint pid;
            // AO_NOERRORUI only; no debug/design flags or package-debug settings.
            Marshal.ThrowExceptionForHR(((FilesNormalActivationManager)instance).ActivateApplication(id, arguments, 2, out pid));
            return pid;
        } finally { Marshal.ReleaseComObject(instance); }
    }
}
'@
if ($ValidateOnly) {
    [ordered]@{mode='ValidateOnly';auth_guard=$true;COM_compiled=$true;activation='NOT_EXECUTED';package_query='NOT_EXECUTED';registration='NOT_EXECUTED'}|ConvertTo-Json
    return
}
$packages=@(Get-AppxPackage -Name FilesDev)
if ($packages.Count -ne 1 -or $packages[0].Publisher -ne 'CN=Files' -or $packages[0].Version.ToString() -ne '4.3.0.0' -or $AppUserModelId -ne ($packages[0].PackageFamilyName+'!App') -or $ActivationArguments -ne '') { throw 'Activation not bound to expected FilesDev App/empty arguments' }
$launchedId = [FilesNormalActivation]::Launch($AppUserModelId, $ActivationArguments)
[ordered]@{ pid=$launchedId; activationArguments=$ActivationArguments; diagnosticContext=$false; activation='IApplicationActivationManager, AO_NOERRORUI' } |
    ConvertTo-Json -Compress | Set-Content -LiteralPath $StatePath -Encoding UTF8
