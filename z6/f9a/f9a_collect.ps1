# Z6 F9a: copy build/clone logs from %TEMP%\z6\f9a into pilot/files/f9a_logs with path redaction; write manifest.
$w = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$src = Join-Path $env:TEMP 'z6\f9a'
$dst = Join-Path $w 'pilot\files\f9a_logs'
New-Item -ItemType Directory -Force $dst | Out-Null
$enc = New-Object Text.UTF8Encoding($false)
$sha = [Security.Cryptography.SHA256]::Create()
$man = @("# F9a log manifest: name | raw_sha256 | committed_sha256 | bytes_committed", "# redaction: user profile -> %USERPROFILE%, Program Files (x86) -> %ProgramFiles(x86)%, Program Files -> %ProgramFiles%")
foreach ($n in 'clone.log','cmd.txt','restore.log','build.log','times.txt','cmd2.txt','build2.log','times2.txt') {
  $p = Join-Path $src $n
  $raw = [IO.File]::ReadAllBytes($p)
  $rs = -join ($sha.ComputeHash($raw) | ForEach-Object { $_.ToString('x2') })
  $t = [IO.File]::ReadAllText($p)
  $t = $t.Replace($env:USERPROFILE, '%USERPROFILE%').Replace('C:\Program Files (x86)', '%ProgramFiles(x86)%').Replace('C:\Program Files', '%ProgramFiles%')
  $o = Join-Path $dst $n
  [IO.File]::WriteAllText($o, $t, $enc)
  $ob = [IO.File]::ReadAllBytes($o)
  $cs = -join ($sha.ComputeHash($ob) | ForEach-Object { $_.ToString('x2') })
  $man += "$n | $rs | $cs | $($ob.Length)"
}
[IO.File]::WriteAllText((Join-Path $dst 'MANIFEST.txt'), (($man -join "`n") + "`n"), $enc)
$man
