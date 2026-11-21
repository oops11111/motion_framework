# Z6 F3: copy raw outputs from %TEMP%\z6\f3 into z6/f3/raw/ for commit.
# Only change: byte-level replacement of the local user-profile path with the literal text %USERPROFILE%.
# raw (pre-replacement) sha256/size of every file are listed in raw/redaction_manifest.txt; also sha256 of the test exes.
$ErrorActionPreference = 'Stop'
$L = Join-Path $env:TEMP 'z6\f3'
$dst = Join-Path $PSScriptRoot 'raw'
New-Item -ItemType Directory -Force $dst | Out-Null
$files = @('build.log','vcvars64_out.txt','cl_fm_capi_test_release.txt','cl_fm_capi_test_debug.txt',
  'fm_capi_test_release_output.txt','fm_capi_test_debug_output.txt','composition_pre.txt','composition_post.txt')
$prof = $env:USERPROFILE
$pats = @([regex]::Escape($prof.Replace('\','\\')), [regex]::Escape($prof.Replace('\','/')), [regex]::Escape($prof))
$latin = [Text.Encoding]::GetEncoding(28591)
$sha = [Security.Cryptography.SHA256]::Create()
function Get-Sha256Hex([byte[]]$b) { -join ($sha.ComputeHash($b) | ForEach-Object { $_.ToString('x2') }) }
$man = @('# file | raw_size | raw_sha256 | committed_size | committed_sha256 | replacements (profile path -> %USERPROFILE%)')
foreach ($f in $files) {
  $src = Join-Path $L $f
  if (-not (Test-Path $src)) { $man += "$f | MISSING"; continue }
  $b = [IO.File]::ReadAllBytes($src)
  $s = $latin.GetString($b); $n = 0
  foreach ($p in $pats) { $n += ([regex]::Matches($s, $p, 'IgnoreCase')).Count; $s = [regex]::Replace($s, $p, '%USERPROFILE%', 'IgnoreCase') }
  $o = $latin.GetBytes($s)
  if ($o.Length -gt 5MB) { throw "$f exceeds 5MB" }
  [IO.File]::WriteAllBytes((Join-Path $dst $f), $o)
  $man += "$f | $($b.Length) | $(Get-Sha256Hex $b) | $($o.Length) | $(Get-Sha256Hex $o) | $n"
}
$man += '# test executables (not committed)'
foreach ($e in 'capi_release\fm_capi_test.exe','capi_debug\fm_capi_test.exe') { $p = Join-Path $L $e; $b = [IO.File]::ReadAllBytes($p); $man += "%TEMP%\z6\f3\$e | $($b.Length) | $(Get-Sha256Hex $b)" }
[IO.File]::WriteAllText((Join-Path $dst 'redaction_manifest.txt'), (($man -join "`n") + "`n"), (New-Object Text.UTF8Encoding($false)))
$man
