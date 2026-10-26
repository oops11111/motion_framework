# Builds the M0 native pieces on the Windows lab machine:
#   1. rive-runtime at the pinned tag (core + D3D11 renderer + player sample)
#   2. m0rive.dll        (M0-1 Rive -> composition surface / swapchain)
#   3. m0_tear_probe.exe (M0-4 ALLOW_TEARING)
# and stages the test asset. Needs: VS 2022 C++ build tools (cl), Windows SDK,
# Git for Windows (bash on PATH; Rive's build script shells out to it), network
# access to github.com for the clone and Rive's pinned premake download.
#
#   powershell -ExecutionPolicy Bypass -File m0\build_m0.ps1 [-DemoBinDir <dir with MotionSoftDemo.exe>]
param(
    [string]$DemoBinDir = "",
    [string]$ArtDir = "C:\Users\admin\motion_framework\artifacts"
)
# Continue, not Stop: in Windows PowerShell 5.1 a native tool writing to stderr
# (git progress, cl banner) under 2>&1 would otherwise abort the script. Every
# native step checks $LASTEXITCODE instead.
$ErrorActionPreference = "Continue"
$M0 = $PSScriptRoot
$Ext = Join-Path $M0 "ext"
$Out = Join-Path $M0 "out"
$RiveTag = "runtime-v0.1.545"
$RiveCommit = "2a043c31ccbda27628a2c80fd105c16ea35ed12a"   # 2026-10-10; reviewed against m0rive.cpp
$AssetRel = "tests\unit_tests\assets\parity\Knight_square_2.riv"
$AssetSha = "cec7ff27afbf9506cd64c37cccf40b0e58a8eacd8a91dc7deb54c040ad9addb8"
New-Item -ItemType Directory -Force $Ext, $Out | Out-Null
$Log = Join-Path $Out "build_m0.log"
function Say($m) { $l = "[{0:yyyy-MM-dd HH:mm:ss}] {1}" -f (Get-Date), $m; Write-Host $l; Add-Content $Log $l }

# --- MSVC environment ------------------------------------------------------------
if (-not (Get-Command cl.exe -ErrorAction SilentlyContinue)) {
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    $vs = & $vswhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
    if (-not $vs) { throw "MSVC build tools not found (vswhere)" }
    cmd /c "`"$vs\Common7\Tools\VsDevCmd.bat`" -arch=amd64 -host_arch=amd64 >nul && set" | ForEach-Object {
        if ($_ -match '^([^=]+)=(.*)$') { Set-Item "env:$($Matches[1])" $Matches[2] }
    }
}
Say ("cl: " + ((cl 2>&1 | Select-Object -First 1) -join ""))
if (-not (Get-Command bash.exe -ErrorAction SilentlyContinue)) { throw "bash.exe not on PATH (install Git for Windows with the 'Git from the command line and also from 3rd-party software' option)" }

# --- 1. rive-runtime at the pinned commit ------------------------------------------
$Rive = Join-Path $Ext "rive-runtime"
if (-not (Test-Path $Rive)) {
    Say "cloning rive-runtime $RiveTag"
    git clone --depth 1 --branch $RiveTag https://github.com/rive-app/rive-runtime.git $Rive 2>&1 | Tee-Object -Append $Log
    if ($LASTEXITCODE -ne 0) { throw "git clone failed" }
}
$head = (git -C $Rive rev-parse HEAD).Trim()
Say "rive HEAD=$head"
if ($head -ne $RiveCommit) { throw "rive-runtime HEAD $head != pinned $RiveCommit" }

$RiveOut = Join-Path $Rive "tests\out\release"
if (-not (Test-Path (Join-Path $RiveOut "rive.lib"))) {

    # Rive docs: put build/ on PATH, prefer Git bash over WSL; need make+python3 for build_rive.
    $gitBin = "C:\Program Files\Git\bin"
    $riveBuild = Join-Path $Rive "build"
    $makeBin = "C:\Program Files (x86)\GnuWin32\bin"; $pyTools = Join-Path $M0 "tools"; $env:Path = "$gitBin;$riveBuild;$makeBin;$pyTools;" + $env:Path
    Say "building rive (tests\, --toolset=msc release); first run downloads premake"
    # MSVC on CP936 (zh-CN) treats UTF-8 sources as C4819; Rive builds with /WX so they become C2220.
    # -utf-8 locale fix; -I DirectX-Headers before WindowsSDK so vendored d3d12.h wins __d3d12_h__ guard (SDK 18362 too old for v1.615.0).
    $dxInc = Join-Path $Rive "tests\dependencies\microsoft_DirectX-Headers_v1.615.0\include\directx"
    $env:MSYS2_ARG_CONV_EXCL = "*"
    if (Test-Path $dxInc) {
        $env:CL = "-utf-8 -I`"$dxInc`""
        $env:INCLUDE = "$dxInc;" + $env:INCLUDE
        Say "DXHEADERS_PREPEND: $dxInc"
    } else {
        $env:CL = "-utf-8"
        Say "DXHEADERS_PREPEND: missing $dxInc (externalincludedirs only)"
    }
    Push-Location (Join-Path $Rive "tests")
    try { & ..\build\build_rive.ps1 --toolset=msc release 2>&1 | Tee-Object -Append $Log } finally { Pop-Location }
    if (-not (Test-Path (Join-Path $RiveOut "rive.lib"))) { throw "rive build produced no rive.lib (see $Log)" }
}
$libs = Get-ChildItem $RiveOut -Filter *.lib | Select-Object -ExpandProperty FullName
Say ("rive libs: " + (($libs | ForEach-Object { Split-Path $_ -Leaf }) -join ", "))
if (-not ($libs | Where-Object { $_ -like "*rive_pls_renderer.lib" })) { throw "rive_pls_renderer.lib missing in $RiveOut" }

# --- 2. m0rive.dll -----------------------------------------------------------------
# Must match how Rive was built: /MT (static CRT, Rive's Windows default), release
# defines, and the feature defines build_rive.sh enables by default
# (--with_rive_text --with_rive_layout --with_rive_canvas); they change class layouts.
$defs = "/DWITH_RIVE_TEXT /DWITH_RIVE_LAYOUT /DRIVE_CANVAS /DRIVE_WINDOWS /D_USE_MATH_DEFINES /DNOMINMAX /DNDEBUG /DRELEASE /D_CRT_SECURE_NO_WARNINGS /D_SILENCE_CXX20_IS_POD_DEPRECATION_WARNING"
$inc = "/I`"$Rive\include`" /I`"$Rive\renderer\include`""
$src = Join-Path $M0 "native\m0rive\m0rive.cpp"
$libArgs = ($libs | ForEach-Object { "`"$_`"" }) -join " "
Say "building m0rive.dll"
cmd /c "cl /nologo /std:c++latest /EHsc /O2 /MT /LD $defs $inc `"$src`" /Fe`"$Out\m0rive.dll`" /Fo`"$Out\\`" /link $libArgs d3d11.lib dxgi.lib dxguid.lib dwmapi.lib d3dcompiler.lib user32.lib 2>&1" | Tee-Object -Append $Log
if ($LASTEXITCODE -ne 0) { throw "m0rive.dll link failed (see $Log)" }

# --- 3. m0_tear_probe.exe -------------------------------------------------------------
Say "building m0_tear_probe.exe"
$tsrc = Join-Path $M0 "native\m0_tear_probe.cpp"
cmd /c "cl /nologo /std:c++17 /EHsc /O2 /DNOMINMAX /DWIN32_LEAN_AND_MEAN `"$tsrc`" /Fe`"$Out\m0_tear_probe.exe`" /Fo`"$Out\\`" /link d3d11.lib dxgi.lib dxguid.lib dcomp.lib user32.lib 2>&1" | Tee-Object -Append $Log
if ($LASTEXITCODE -ne 0) { throw "m0_tear_probe.exe build failed (see $Log)" }

# --- 4. stage asset and DLL ----------------------------------------------------------
$assetDir = Join-Path $ArtDir "m0_assets"
New-Item -ItemType Directory -Force $assetDir | Out-Null
Copy-Item (Join-Path $Rive $AssetRel) $assetDir -Force
$sha = (Get-FileHash (Join-Path $assetDir "Knight_square_2.riv") -Algorithm SHA256).Hash.ToLower()
Say "asset sha256=$sha"
if ($sha -ne $AssetSha) { throw "asset hash mismatch" }
if ($DemoBinDir) { Copy-Item "$Out\m0rive.dll" $DemoBinDir -Force; Say "copied m0rive.dll -> $DemoBinDir" }
foreach ($f in "m0rive.dll", "m0_tear_probe.exe") {
    Say ("{0} sha256={1}" -f $f, (Get-FileHash (Join-Path $Out $f) -Algorithm SHA256).Hash.ToLower())
}
Say "BUILD_OK"



