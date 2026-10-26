$ErrorActionPreference = 'Stop'
$root = 'C:\Users\admin\motion_framework'
# MainPage may have landed at root; move if needed
$src = Join-Path $root 'MainPage.xaml.cs'
$dst = Join-Path $root 'MotionSoftDemo\MainPage.xaml.cs'
if (Test-Path $src) {
  Copy-Item -Force $src $dst
  Remove-Item -Force $src
  'MOVED MainPage to MotionSoftDemo'
}
$hits = Select-String -Path $dst -Pattern 'm2DeltaVPrime|m2-delta-v-prime|RunM2DeltaVPrimeProbe'
"MainPage hits=$($hits.Count)"
$hits | Select-Object -First 10 | ForEach-Object { "$($_.LineNumber):$($_.Line.Trim().Substring(0,[Math]::Min(100,$_.Line.Trim().Length)))" }
"probe exists=$(Test-Path (Join-Path $root 'MotionSoftDemo\M2DeltaVPrimeProbe.cs'))"
"prereg exists=$(Test-Path (Join-Path $root 'artifacts\m2_delta_v_prime_prereg.txt'))"
$comp = Join-Path $root 'code\composition.h'
$h = [System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::Create().ComputeHash([System.IO.File]::ReadAllBytes($comp))).Replace('-','').ToLowerInvariant()
"composition.h=$h"
$exp = 'a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418'
if ($h -ne $exp) { throw "composition hash mismatch" }
# stop any running demo
Get-Process -Name MotionSoftDemo -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1
# build
Set-Location (Join-Path $root 'MotionSoftDemo')
$env:WINAPP_CLI_TELEMETRY_OPTOUT = '1'
dotnet build -c Debug -p:Platform=x64 2>&1 | Tee-Object (Join-Path $root 'artifacts\m2_delta_v_prime_build.log')
if ($LASTEXITCODE -ne 0) { throw "build failed $LASTEXITCODE" }
'BUILD_OK'
