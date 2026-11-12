param(
    [Parameter(Mandatory = $true)][string]$Header,
    [Parameter(Mandatory = $true)][string]$OutFile,
    [Parameter(Mandatory = $true)][string]$Tip
)

$ErrorActionPreference = "Stop"
$bytes = [System.IO.File]::ReadAllBytes($Header)
$sha = [System.Security.Cryptography.SHA256]::Create()
try {
    $raw = ([System.BitConverter]::ToString($sha.ComputeHash($bytes))).Replace("-", "").ToLowerInvariant()
    $norm = New-Object System.Collections.Generic.List[byte]
    for ($i = 0; $i -lt $bytes.Length; $i++) {
        $b = $bytes[$i]
        if ($b -eq 13) {
            $norm.Add(10)
            if (($i + 1) -lt $bytes.Length -and $bytes[$i + 1] -eq 10) { $i++ }
        }
        else {
            $norm.Add($b)
        }
    }
    $normalized = ([System.BitConverter]::ToString($sha.ComputeHash($norm.ToArray()))).Replace("-", "").ToLowerInvariant()
}
finally {
    $sha.Dispose()
}

$tip = ($Tip -replace "\s", "")
$dir = Split-Path -Parent $OutFile
if ($dir -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
@(
    "tip=$tip",
    "composition_sha256=$normalized",
    "composition_sha256_raw=$raw"
) | Set-Content -Encoding ascii -Path $OutFile
