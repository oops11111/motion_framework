$ErrorActionPreference = 'Stop'
$dir = Join-Path $env:TEMP 'z6\f5b\diag'
New-Item -ItemType Directory -Force -Path $dir | Out-Null
$out = Join-Path $dir 'listener.txt'
function W([string]$m) { Add-Content -Path $out -Value ((Get-Date).ToString('yyyy-MM-ddTHH:mm:ss.fffzzz') + ' ' + $m) -Encoding utf8 }
Set-Content -Path $out -Value '' -Encoding utf8

Add-Type -Namespace Z6F5b -Name Spi -MemberDefinition @'
[DllImport("user32.dll", SetLastError=true)]
public static extern bool SystemParametersInfo(uint a, uint b, System.IntPtr c, uint d);
[DllImport("user32.dll", SetLastError=true)]
public static extern bool SystemParametersInfo(uint a, uint b, ref int c, uint d);
'@

function Get-Spi { $v = 0; $ok = [Z6F5b.Spi]::SystemParametersInfo(0x1042, 0, [ref]$v, 0); return @($ok, $v) }
function Set-Spi([int]$v) { return [Z6F5b.Spi]::SystemParametersInfo(0x1043, 0, [IntPtr]$v, 3) }

[Windows.UI.ViewManagement.UISettings,Windows.UI.ViewManagement,ContentType=WindowsRuntime] | Out-Null
$u = New-Object Windows.UI.ViewManagement.UISettings
W ("start UISettings.AnimationsEnabled=" + $u.AnimationsEnabled + " spi=" + ((Get-Spi) -join ','))
$sub = Register-ObjectEvent -InputObject $u -EventName AnimationsEnabledChanged -Action {
  Add-Content -Path (Join-Path $env:TEMP 'z6\f5b\diag\listener.txt') -Value ((Get-Date).ToString('yyyy-MM-ddTHH:mm:ss.fffzzz') + ' EVENT sender.AnimationsEnabled=' + $Event.Sender.AnimationsEnabled) -Encoding utf8
}
W 'subscribed'

W ('set 0 -> ' + (Set-Spi 0))
Start-Sleep -Seconds 5
W ('after off spi=' + ((Get-Spi) -join ',') + ' freshUISettings=' + (New-Object Windows.UI.ViewManagement.UISettings).AnimationsEnabled)
Start-Sleep -Seconds 2

W ('set 1 -> ' + (Set-Spi 1))
Start-Sleep -Seconds 5
W ('after on spi=' + ((Get-Spi) -join ',') + ' freshUISettings=' + (New-Object Windows.UI.ViewManagement.UISettings).AnimationsEnabled)
Start-Sleep -Seconds 2
W ('end event_lines=' + @(Get-Content $out | Where-Object { $_ -match ' EVENT ' }).Count)
Unregister-Event -SourceIdentifier $sub.Name
