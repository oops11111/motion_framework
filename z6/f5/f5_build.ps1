# Z6 F5 build: machine SDK 10.0.300 only, approved non-original switch -p:WarningsNotAsErrors=NU1903.
$ErrorActionPreference = 'Stop'
$out = Join-Path $env:TEMP 'z6\f5\build_wnae'
New-Item -ItemType Directory -Force -Path $out | Out-Null
$env:DOTNET_ROOT = "$env:ProgramFiles\dotnet"
$env:PATH = (($env:PATH -split ';') | Where-Object { $_ -and ($_ -notlike '*\.dotnet*') }) -join ';'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'
# Decode native stdout as UTF-8 (the console code page here is 936 and would mangle the CLI's Chinese output).
$utf8 = New-Object System.Text.UTF8Encoding($false)
[Console]::OutputEncoding = $utf8
$OutputEncoding = $utf8

$dotnet = Join-Path $env:ProgramFiles 'dotnet\dotnet.exe'
$ver = & $dotnet --version
$where = (& where.exe dotnet) -join '; '
@("DOTNET_ROOT=%ProgramFiles%\dotnet", "dotnet --version: $ver", "where dotnet: $where") |
  Set-Content -Path (Join-Path $out 'env.txt') -Encoding utf8

$cmd = '"%ProgramFiles%\dotnet\dotnet.exe" build src\FluentHub\FluentHub.csproj -c Debug -p:Platform=x64 -p:WarningsNotAsErrors=NU1903'
Set-Content -Path (Join-Path $out 'cmd.txt') -Value $cmd -Encoding utf8

$cwd = Join-Path $env:USERPROFILE 'pilot\FluentHub'
$start = [DateTimeOffset]::Now
Push-Location $cwd
try {
  & $dotnet build src\FluentHub\FluentHub.csproj -c Debug -p:Platform=x64 -p:WarningsNotAsErrors=NU1903 *>&1 |
    Tee-Object -FilePath (Join-Path $out 'build.log')
  $code = $LASTEXITCODE
} finally { Pop-Location }
$end = [DateTimeOffset]::Now
@("start $($start.ToString('o'))", "exit=$code end $($end.ToString('o')) elapsed_sec=$([math]::Round(($end-$start).TotalSeconds,3))") |
  Set-Content -Path (Join-Path $out 'times.txt') -Encoding utf8
"EXIT=$code"
