$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = New-Object System.Text.UTF8Encoding($false)
$null = [Windows.UI.ViewManagement.UISettings, Windows.UI.ViewManagement, ContentType = WindowsRuntime]
$settings = New-Object Windows.UI.ViewManagement.UISettings
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class F5ReadOnlySettings {
    [DllImport("user32.dll", SetLastError=true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SystemParametersInfo(uint action, uint size, out int value, uint flags);
    public static bool ClientAreaAnimations() {
        int value;
        if (!SystemParametersInfo(0x1042, 0, out value, 0))
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        return value != 0;
    }
}
'@
$desktop = Get-ItemProperty -LiteralPath 'Registry::HKEY_CURRENT_USER\Control Panel\Desktop'
$metrics = Get-ItemProperty -LiteralPath 'Registry::HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics'
[pscustomobject]@{
    AnimationsEnabled = [bool]$settings.AnimationsEnabled
    SPI_GETCLIENTAREAANIMATION = [F5ReadOnlySettings]::ClientAreaAnimations()
    MinAnimate = [string]$metrics.MinAnimate
    UserPreferencesMask = [BitConverter]::ToString([byte[]]$desktop.UserPreferencesMask).Replace('-', '')
} | ConvertTo-Json -Compress
