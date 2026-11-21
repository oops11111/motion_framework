param([int]$OwnedPid,[string]$OwnedHwnd,[switch]$Execute)
$ErrorActionPreference='Stop'
if(-not $Execute){@{mode='dryrun';marker='SettingsButton';runtime='NOT EXECUTED'}|ConvertTo-Json;return}
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
$hwnd=[IntPtr]::new([Convert]::ToInt64($OwnedHwnd,16))
$deadline=[DateTime]::UtcNow.AddSeconds(90)
$ready=$false
while([DateTime]::UtcNow -lt $deadline){
 $root=[System.Windows.Automation.AutomationElement]::FromHandle($hwnd)
 if($null -eq $root -or $root.Current.ProcessId -ne $OwnedPid){throw 'UIA root process mismatch/unavailable'}
 $id=[System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::AutomationIdProperty,'SettingsButton')
 $pidCondition=[System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ProcessIdProperty,$OwnedPid)
 $condition=[System.Windows.Automation.AndCondition]::new($id,$pidCondition)
 $marker=$root.FindFirst([System.Windows.Automation.TreeScope]::Descendants,$condition)
 if($null -ne $marker){
  $visible=-not $marker.Current.IsOffscreen;$enabled=$marker.Current.IsEnabled
  if($visible -and $enabled){$ready=$true;break}
 }
 Start-Sleep -Milliseconds 500
}
@{marker='SettingsButton';exists=($null -ne $marker);visible=($ready -and $visible);enabled=($ready -and $enabled);pid=$OwnedPid;hwnd=$OwnedHwnd;ready=$ready}|ConvertTo-Json
if(-not $ready){exit 1}
