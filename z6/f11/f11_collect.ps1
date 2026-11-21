# Z6 F11: copy logs from %TEMP%\z6\f11 into z6/f11/raw with path redaction; write manifest.
$w = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$src = Join-Path $env:TEMP 'z6\f11'
$dst = Join-Path $w 'z6\f11\raw'
$enc = New-Object Text.UTF8Encoding($false)
$sha = [Security.Cryptography.SHA256]::Create()
$man = @('# F11 log manifest: path | raw_sha256 | committed_sha256 | bytes_committed', '# redaction: user profile -> %USERPROFILE%, Program Files (x86) -> %ProgramFiles(x86)%, Program Files -> %ProgramFiles%')
Get-ChildItem $src -Recurse -File -Include *.log,*.txt | Where-Object { $_.FullName -notlike '*\src\*' } | Sort-Object FullName | ForEach-Object {
  $rel = $_.FullName.Substring($src.Length + 1)
  $raw = [IO.File]::ReadAllBytes($_.FullName)
  $rs = -join ($sha.ComputeHash($raw) | ForEach-Object { $_.ToString('x2') })
  $t = [IO.File]::ReadAllText($_.FullName)
  $t = $t.Replace($env:USERPROFILE, '%USERPROFILE%').Replace('C:\Program Files (x86)', '%ProgramFiles(x86)%').Replace('C:\Program Files', '%ProgramFiles%')
  $o = Join-Path $dst $rel
  New-Item -ItemType Directory -Force (Split-Path $o) | Out-Null
  [IO.File]::WriteAllText($o, $t, $enc)
  $ob = [IO.File]::ReadAllBytes($o)
  $cs = -join ($sha.ComputeHash($ob) | ForEach-Object { $_.ToString('x2') })
  $man += "$($rel.Replace('\','/')) | $rs | $cs | $($ob.Length)"
}
[IO.File]::WriteAllText((Join-Path $dst 'MANIFEST.txt'), (($man -join "`n") + "`n"), $enc)
$man
