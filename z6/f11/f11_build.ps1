# Z6 F11: one FluentHub side-fork build with the pidan-approved switch; logs to %TEMP%\z6\f11\<Label>.
param([Parameter(Mandatory=$true)][string]$Label, [Parameter(Mandatory=$true)][string]$Switch, [string]$Root = '')
$fh = Join-Path $env:USERPROFILE 'pilot\FluentHub'
$lg = Join-Path $env:TEMP "z6\f11\$Label"
New-Item -ItemType Directory -Force $lg | Out-Null
$a = @('build', 'src\FluentHub\FluentHub.csproj', '-c', 'Debug', '-p:Platform=x64', $Switch)
if ($Root -ne '') { $a += "-p:FmL1Root=$Root" }
$cmd = 'dotnet ' + ($a -join ' ')
Set-Location $fh
"cwd=$fh" | Out-File "$lg\cmd.txt" -Encoding utf8
$cmd | Out-File "$lg\cmd.txt" -Append -Encoding utf8
"start $(Get-Date -Format o)" | Out-File "$lg\times.txt" -Encoding utf8
& dotnet @a 2>&1 | Out-File "$lg\build.log" -Encoding utf8
$ec = $LASTEXITCODE
"exit=$ec end $(Get-Date -Format o)" | Out-File "$lg\times.txt" -Append -Encoding utf8
"[$Label] $cmd"
"[$Label] exit=$ec"
Select-String -Path "$lg\build.log" -Pattern 'FmL1VerifyPack|error|NU1903|个警告|个错误|Warning\(s\)|Error\(s\)' | Select-Object -Last 12 | ForEach-Object { $_.Line }
