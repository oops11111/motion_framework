param(
    [Parameter(Mandatory=$true)][string]$AppUserModelId,
    [Parameter(Mandatory=$true)][string]$StatePath,
    [string]$ActivationArguments = ''
)
$ErrorActionPreference = 'Stop'
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
[ComImport, Guid("2e941141-7f97-4756-ba1d-9decde894a3d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface F5ApplicationActivationManager {
    [PreserveSig] int ActivateApplication([MarshalAs(UnmanagedType.LPWStr)] string id,
        [MarshalAs(UnmanagedType.LPWStr)] string arguments, uint options, out uint pid);
}
public static class F5NormalActivation {
    public static uint Launch(string id, string arguments) {
        object instance=Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("45ba127d-10a8-46ea-8ab7-56ea9078943c")));
        try {
            uint pid;
            // AO_NOERRORUI only; no debug/design flags or package-debug settings.
            Marshal.ThrowExceptionForHR(((F5ApplicationActivationManager)instance).ActivateApplication(id, arguments, 2, out pid));
            return pid;
        } finally { Marshal.ReleaseComObject(instance); }
    }
}
'@
$launchedId = [F5NormalActivation]::Launch($AppUserModelId, $ActivationArguments)
[ordered]@{ pid=$launchedId; activationArguments=$ActivationArguments; diagnosticContext=$false; activation='IApplicationActivationManager, AO_NOERRORUI' } |
    ConvertTo-Json -Compress | Set-Content -LiteralPath $StatePath -Encoding UTF8
