# Z6 F0 (GUI rerun): copy GUI devsmoke outputs from %TEMP%\z6\f0 into z6/f0/raw_gui/.
# Changes made to committed copies (originals stay untouched in %TEMP%\z6\f0):
#  1) user-profile path -> literal %USERPROFILE% (byte-level, as f0_collect.ps1);
#  2) every line containing "name=FrameTick": "payload=<values>" -> "payload=[omitted]"
#     (handover r2 section 4: FrameTick is counted only, no deltaUs values in committed logs).
# Per-file raw/committed sha256, replacement counts and FrameTick line counts: raw_gui/manifest.txt.
$ErrorActionPreference = 'Stop'
$L = Join-Path $env:TEMP 'z6\f0'
$dst = Join-Path $PSScriptRoot 'raw_gui'
New-Item -ItemType Directory -Force $dst | Out-Null
$files = @('launcher_Debug.log','launcher_Release.log','schtasks_gui.txt',
  'gui_Debug\z3a_l1_dev_smoke.txt','gui_Debug\z3a_l1_dev_ui.txt','gui_Release\z3a_l1_dev_smoke.txt','gui_Release\z3a_l1_dev_ui.txt')
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
  $out = Join-Path $dst ($f.Replace('\','__'))
  [IO.File]::WriteAllBytes($out, $o)
  $man += "$f | $($b.Length) | $(Get-Sha256Hex $b) | $($o.Length) | $(Get-Sha256Hex $o) | $n | $ftLines | $om"
}
[IO.File]::WriteAllText((Join-Path $dst 'manifest.txt'), (($man -join "`n") + "`n"), (New-Object Text.UTF8Encoding($false)))
$man
