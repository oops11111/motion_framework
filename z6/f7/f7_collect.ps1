# Z6 F7: copy raw outputs from %TEMP%\z6\f7 into z6/f7/raw/ for commit. Originals stay in %TEMP%\z6\f7.
# Changes in committed copies only:
#  1) user-profile path -> literal %USERPROFILE% (byte-level, Latin-1);
#  2) lines containing "name=FrameTick": "payload=<values>" -> "payload=[omitted]" (FrameTick counted only).
# Per-file raw/committed sha256, replacement counts and FrameTick counts: raw/redaction_manifest.txt.
$ErrorActionPreference = 'Stop'
$L = Join-Path $env:TEMP 'z6\f7'
$dst = Join-Path $PSScriptRoot 'raw'
New-Item -ItemType Directory -Force $dst | Out-Null
$files = @('build.log','vcvars64_out.txt','cl_fluidmotion.txt','pack_build_release.txt','surface_tool_build.txt',
  'surface_pack_release.txt','surface_pack_release.checksurface.txt','surface_pack_release.stdout.txt','verify.txt',
  'cl_fm_capi_test_release.txt','cl_fm_capi_test_debug.txt','fm_capi_test_release_output.txt','fm_capi_test_debug_output.txt',
  'dotnet_devsmoke_Debug.txt','dotnet_devsmoke_Release.txt','devsmoke_console_Debug.txt','devsmoke_console_Release.txt',
  'composition_pre.txt','composition_post.txt')
$prof = $env:USERPROFILE
$pats = @([regex]::Escape($prof.Replace('\','\\')), [regex]::Escape($prof.Replace('\','/')), [regex]::Escape($prof))
$latin = [Text.Encoding]::GetEncoding(28591)
$sha = [Security.Cryptography.SHA256]::Create()
function Get-Sha256Hex([byte[]]$b) { -join ($sha.ComputeHash($b) | ForEach-Object { $_.ToString('x2') }) }
$ft = [regex]'(?m)^(?<pre>[^\r\n]*name=FrameTick[^\r\n]*?payload=)[^\s\r\n]+'
$man = @('# file | raw_size | raw_sha256 | committed_size | committed_sha256 | path_replacements | frametick_lines | frametick_payloads_omitted')
foreach ($f in $files) {
  $src = Join-Path $L $f
  if (-not (Test-Path $src)) { $man += "$f | MISSING"; continue }
  $b = [IO.File]::ReadAllBytes($src)
  $s = $latin.GetString($b); $n = 0
  foreach ($p in $pats) { $n += ([regex]::Matches($s, $p, 'IgnoreCase')).Count; $s = [regex]::Replace($s, $p, '%USERPROFILE%', 'IgnoreCase') }
  $ftLines = ([regex]::Matches($s, '(?m)^[^\r\n]*FrameTick')).Count
  $om = $ft.Matches($s).Count
  $s = $ft.Replace($s, '${pre}[omitted]')
  $o = $latin.GetBytes($s)
  if ($o.Length -gt 5MB) { throw "$f exceeds 5MB" }
  [IO.File]::WriteAllBytes((Join-Path $dst $f), $o)
  $man += "$f | $($b.Length) | $(Get-Sha256Hex $b) | $($o.Length) | $(Get-Sha256Hex $o) | $n | $ftLines | $om"
}
[IO.File]::WriteAllText((Join-Path $dst 'redaction_manifest.txt'), (($man -join "`n") + "`n"), (New-Object Text.UTF8Encoding($false)))
$man
