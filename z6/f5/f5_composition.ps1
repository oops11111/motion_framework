# Z6 F5: record code/composition.h identity (git blob, raw sha256, CRLF->LF normalized sha256).
param([Parameter(Mandatory=$true)][string]$Out, [Parameter(Mandatory=$true)][string]$Label)
$w = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$f = Join-Path $w 'code\composition.h'
$blob = (& git -C $w rev-parse HEAD:code/composition.h).Trim()
$head = (& git -C $w rev-parse HEAD).Trim()
$bytes = [IO.File]::ReadAllBytes($f)
$sha = [Security.Cryptography.SHA256]::Create()
$raw = -join ($sha.ComputeHash($bytes) | ForEach-Object { $_.ToString('x2') })
$txt = [Text.Encoding]::GetEncoding(28591).GetString($bytes).Replace("`r`n", "`n")
$norm = -join ($sha.ComputeHash([Text.Encoding]::GetEncoding(28591).GetBytes($txt)) | ForEach-Object { $_.ToString('x2') })
$t = [DateTimeOffset]::Now.ToString('yyyy-MM-dd HH:mm:ss zzz')
$lines = @("label=$Label", "time=$t", "head=$head", "blob_HEAD:code/composition.h=$blob", "sha256_raw_worktree=$raw", "sha256_normalized_lf=$norm",
  "expected_blob=fd0603c17136c067e89c71e56aef310f2f7e63f4", "expected_normalized=a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418",
  ("match=" + (($blob -eq 'fd0603c17136c067e89c71e56aef310f2f7e63f4') -and ($norm -eq 'a55d4952789f5b805b9830fb725f17e3b37da2675cafc2eb80c316c95178e418'))))
[IO.File]::WriteAllText($Out, (($lines -join "`n") + "`n"), (New-Object Text.UTF8Encoding($false)))
$lines
