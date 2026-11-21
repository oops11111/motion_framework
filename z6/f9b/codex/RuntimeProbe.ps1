param(
    [ValidateSet('Geometry','Modules','Capture','Cursor','Move','RestorePointer','SignalEntry','EntryBurst')][string]$Action='Geometry',
    [switch]$Execute,[switch]$ValidateOnly,
    [string]$AuthorityFile,[string]$InstructionFile,[string]$BindingFile,
    [int]$OwnedPid,[string]$OwnedExe,[string]$OwnedPackage,[string]$OwnedHwnd='0',
    [int]$X,[int]$Y,[string]$OutputPath,[string]$CropFile,[string]$CropSha,[string]$OriginalPointerFile,[string]$OriginalPointerSha
)
$ErrorActionPreference='Stop'
if(-not $Execute -and -not $ValidateOnly){@{mode='dryrun';action=$Action;runtime='NOT_EXECUTED'}|ConvertTo-Json;return}
. "$PSScriptRoot/RuntimeGuard.ps1"
$binding=Assert-F9bAuthority $AuthorityFile $InstructionFile $BindingFile ([bool]$ValidateOnly)
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName WindowsBase
Add-Type -AssemblyName System.Drawing
$references=@('System.dll','System.Core.dll','System.Drawing.dll',[System.Windows.Automation.AutomationElement].Assembly.Location,[System.Windows.Automation.AutomationIdentifier].Assembly.Location,[System.Windows.Rect].Assembly.Location)
Add-Type -Path "$PSScriptRoot/RuntimeProbe.cs" -ReferencedAssemblies $references
if($ValidateOnly){@{mode='ValidateOnly';auth_guard=$true;probe_compiled=$true;processquery='NOT_EXECUTED';UIA='NOT_EXECUTED';input='NOT_EXECUTED';screenshot='NOT_EXECUTED'}|ConvertTo-Json;return}
$handle=[Convert]::ToInt64($OwnedHwnd,16)
switch($Action){
 'Geometry' {[F9bProbe]::Geometry($OwnedPid,$handle,$OwnedExe,$OwnedPackage)|ConvertTo-Json -Depth 5}
 'Modules' {[F9bProbe]::ModuleMetadata($OwnedPid,$OwnedExe,$OwnedPackage)|ConvertTo-Json -Depth 5}
 'Cursor' {[F9bProbe]::Cursor()|ConvertTo-Json}
 'Move' {[F9bProbe]::MoveWithinWindow($OwnedPid,$handle,$OwnedExe,$OwnedPackage,$X,$Y);@{pointer_only=$true}|ConvertTo-Json}
 'SignalEntry' {[F9bProbe]::SignalEntry($OwnedPid,$OwnedExe,$OwnedPackage);@{entry_signal=$true}|ConvertTo-Json}
 {$_ -in @('Capture','EntryBurst')} {
    if((Get-F9bSha $CropFile) -cne $CropSha){throw 'Crop binding changed'}
    $crop=Get-Content -LiteralPath $CropFile -Raw -Encoding UTF8|ConvertFrom-Json
    if($crop.pid -ne $OwnedPid -or $crop.hwnd -cne $OwnedHwnd -or $crop.package -cne $OwnedPackage){throw 'Crop ownership mismatch'}
    $scale=$crop.settings.Dpi/96.0
    if($crop.left -ge $crop.right -or $crop.top -ge $crop.bottom -or $crop.left -lt $crop.settings.Window.Left -or $crop.top -lt $crop.settings.Window.Top -or $crop.right -gt $crop.settings.Window.Right -or $crop.bottom -gt $crop.settings.Window.Bottom -or $crop.left -lt $crop.settings.Left-100*$scale -or $crop.right -gt $crop.settings.Right+8*$scale -or $crop.top -lt $crop.settings.Top-8*$scale -or $crop.bottom -gt $crop.settings.Bottom+8*$scale){throw 'Crop exceeds SettingsRow and bounded padding'}
    if($Action -eq 'Capture' -and (Test-Path -LiteralPath $OutputPath)){throw 'Refuse to overwrite screenshot evidence'}
    if($Action -eq 'EntryBurst') {
        if(-not (Test-Path -LiteralPath $OutputPath -PathType Container)){throw 'Fresh entry evidence directory required'}
        [F9bProbe]::EntryBurst($OwnedPid,$handle,$OwnedExe,$OwnedPackage,$OutputPath,$crop.left,$crop.top,$crop.right,$crop.bottom)|ConvertTo-Json -Depth 5
    } else {
        [F9bProbe]::Capture($OwnedPid,$handle,$OwnedExe,$OwnedPackage,$OutputPath,$crop.left,$crop.top,$crop.right,$crop.bottom)
        @{captured=$true;crop_only=$true}|ConvertTo-Json
    }
 }
 'RestorePointer' {
    if((Get-F9bSha $OriginalPointerFile) -cne $OriginalPointerSha){throw 'Original cursor receipt changed'}
    $saved=Get-Content -LiteralPath $OriginalPointerFile -Raw -Encoding UTF8|ConvertFrom-Json
    [F9bProbe]::RestoreOriginalPointer($saved.X,$saved.Y);@{restored_original_pointer=$true;pointer_only=$true}|ConvertTo-Json
 }
}
