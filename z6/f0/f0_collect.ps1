# Z6 F0: copy raw outputs from %TEMP%\z6\f0 into z6/f0/raw/ for commit.
# The only change made to any file is a byte-level replacement of the local user-profile path with the
# literal text %USERPROFILE% (repo rule: no machine-specific absolute paths in committed files).
# raw (pre-replacement) sha256 and size of every file are listed in raw/redaction_manifest.txt.
$ErrorActionPreference = 'Stop'
$w = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$L = Join-Path $env:TEMP 'z6\f0'
$dst = Join-Path $PSScriptRoot 'raw'
New-Item -ItemType Directory -Force $dst | Out-Null
$files = @('build.log','vcvars64_out.txt','cl_fluidmotion.txt','cl_fm_capi_test_release.txt','cl_fm_capi_test_debug.txt',
  'fm_capi_test_release_output.txt','fm_capi_test_debug_output.txt','dotnet_devsmoke_Debug.txt','dotnet_devsmoke_Release.txt',
  'devsmoke_run.log','devsmoke_console_Debug.txt','devsmoke_console_Release.txt',
  'surface.log','surface_tool_build.txt','surface_ds_Release.txt','surface_ds_Release.checksurface.txt','surface_ds_Release.stdout.txt',
  'demo_build.log','demo_restore_Debug.txt','demo_restore_Release.txt','demo_build_Debug.txt','demo_build_Release.txt',
  'composition_pre.txt','composition_post.txt','hashes.txt')
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
[IO.File]::WriteAllText((Join-Path $dst 'redaction_manifest.txt'), (($man -join "`n") + "`n"), (New-Object Text.UTF8Encoding($false)))
$man
